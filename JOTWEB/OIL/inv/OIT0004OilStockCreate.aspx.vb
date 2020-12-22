Option Strict On '一旦On
''************************************************************
' 在庫表登録画面
' 作成日 2020/01/20
' 更新日 2020/01/20
' 作成者 JOT三宅（弘）
' 更新者 JOT三宅（弘）
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 在庫表登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIT0004OilStockCreate
    Inherits Page

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
    '車数から数量を求める定数(車数 × 当定数 ÷ 油種別ウェイト
    Public UKEIRE_BASE_NUM As Decimal = 45
    '何日分増加ドロップダウンを追加するか
    Public SHIP_DATE_ADD_SPAN As Integer = 10

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                'Dim dispDataObj As DemoDispDataClass
                'dispDataObj = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    'Master.RecoverTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonAUTOSUGGESTION" '自動提案ボタン押下
                            WF_ButtonAUTOSUGGESTION_Click()
                        Case "WF_ButtonORDERLIST" '受注作成ボタン押下
                            WF_ButtonORDERLIST_Click()
                        Case "WF_ButtonINPUTCLEAR" '入力値クリアボタン押下
                            WF_ButtonINPUTCLEAR_Click()
                        Case "WF_ButtonGETEMPTURN" '空回日報取込ボタン押下
                            WF_ButtonGETEMPTURN_Click()
                        Case "WF_ButtonRECULC"
                            WF_ButtonRECULC_Click()
                        Case "WF_ButtonUPDATE" '更新ボタン押下
                            WF_ButtonUPDATE_Click()

                        Case "WF_ButtonEND"                 '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_RadioButonClick"
                            WF_RadioButton_Click()
                        Case "ChangeConsignee"
                            ChangeConsignee()
                        Case "WF_Field_DBClick"             'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ButtonOkCommonPopUp" 'カスタムポップアップOK押下時
                            '帳票出力
                            WF_ButtonDownload_Click() 'チェック・条件などもろもろの改修がある為一旦コメント

                            'Case "WF_ButtonCSV" 'ダウンロードボタン押下
                            '    WF_ButtonDownload_Click()
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

        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0004WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet(Optional setConsignee As String = "", Optional SetConsigneeName As String = "")
        Dim mesNo As String = C_MESSAGE_NO.NORMAL
        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If
        '**********************************************
        '画面情報を元に各対象リストを生成
        '**********************************************
        Dim baseDate As String = work.WF_SEL_STYMD.Text
        Dim salesOffice As String = work.WF_SEL_SALESOFFICECODE.Text
        Dim salesOfficeName As String = work.WF_SEL_SALESOFFICE.Text
        Dim shipper As String = work.WF_SEL_SHIPPERCODE.Text
        Dim shipperName As String = work.WF_SEL_SHIPPERNAME.Text
        Dim consignee As String = work.WF_SEL_CONSIGNEE.Text
        Dim consigneeName As String = work.WF_SEL_CONSIGNEENAME.Text
        Dim isOtTrainMode As Boolean = False
        If setConsignee <> "" Then
            consignee = setConsignee
            consigneeName = SetConsigneeName
            Me.hdnChgConsigneeFirstLoad.Value = "1"
        End If
        Dim daysList As Dictionary(Of String, DaysItem)
        Dim oilTypeList As Dictionary(Of String, OilItem)
        Dim trainList As New Dictionary(Of String, TrainListItem)
        Dim dispDataObj As DispDataClass = Nothing

        Dim mitrainList As Dictionary(Of String, TrainListItem) = Nothing
        Dim miOilTypeList As Dictionary(Of String, OilItem) = Nothing
        'DBよりデータ取得し画面用データに加工
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            '日付情報取得（祝祭日含む）
            daysList = GetTargetDateList(sqlCon, baseDate)
            '対象油種取得
            oilTypeList = GetTargetOilType(sqlCon, salesOffice, consignee, shipper)
            If oilTypeList Is Nothing OrElse oilTypeList.Count = 0 Then
                '取り扱い油種が無い場合は何もできないので終了
                mesNo = C_MESSAGE_NO.OIL_STOCK_OILINFO_NOTEXISTS
            End If
            '提案一覧表示可否取得
            Dim canShowSuggestList As Boolean = Me.IsShowSuggestList(sqlCon, consignee)

            '対象列車取得
            If canShowSuggestList Then
                trainList = GetTargetTrain(sqlCon, salesOffice, shipper, consignee)
                'システム管理外列車付与(受注作成しない、在庫計算だけ使う)
                trainList = GetUnmanagedTrain(sqlCon, trainList, salesOffice, shipper, consignee)
                '結果として取り扱い列車が0の場合提案一覧を表示できない為提案表を非表示にする
                If trainList Is Nothing OrElse trainList.Count = 0 Then
                    canShowSuggestList = False
                    mesNo = C_MESSAGE_NO.OIL_SUGGEST_TRAIN_NOTEXISTS
                End If
            Else
                isOtTrainMode = True
                trainList = GetTagetOtTrain(sqlCon, salesOffice, shipper, consignee, daysList)
                If trainList Is Nothing OrElse trainList.Count = 0 Then
                    isOtTrainMode = False
                End If
            End If
            '抽出結果を画面データクラスに展開
            dispDataObj = New DispDataClass(daysList, trainList, oilTypeList, salesOffice, shipper, consignee)
            dispDataObj.SalesOfficeName = salesOfficeName
            dispDataObj.ShipperName = shipperName
            dispDataObj.ConsigneeName = consigneeName
            '提案一覧表示可否設定
            dispDataObj.ShowSuggestList = canShowSuggestList
            'OT用提案一覧表示可否設定
            dispDataObj.IsOtTrainMode = isOtTrainMode
            '前週出荷平均の取得
            dispDataObj = GetLastShipAverage(sqlCon, dispDataObj)
            'ローリー初期表示判定
            Me.hdnDispLorry.Value = IsShowLorryValue(sqlCon, consignee)
            '構内取り有無取得
            dispDataObj = GetMoveInsideData(sqlCon, dispDataObj)
            '既登録データ取得
            dispDataObj = GetTargetStockData(sqlCon, dispDataObj)
            '過去日以外の日付について受入数取得
            dispDataObj = GetReciveFromOrder(sqlCon, dispDataObj)
            '列車運行情報の取得
            dispDataObj = GetTrainOperation(sqlCon, dispDataObj)
            dispDataObj.AsyncDeleteShipper = IsAsyncDeleteShipper(sqlCon, dispDataObj)
            '構内取り設定がある場合、構内取りデータ取得
            If dispDataObj.HasMoveInsideItem Then
                '表構えの為親と構内取り元と同じ列車
                If canShowSuggestList Then
                    '構内取りではない油種「合計」文言を中計と変更
                    dispDataObj.SuggestOilNameList(DispDataClass.SUMMARY_CODE).OilName = "中計"
                    'Dim targetTrainList As List(Of String) = trainList.Keys.ToList
                    mitrainList = GetTargetTrain(sqlCon, dispDataObj.MiSalesOffice, dispDataObj.MiShippersCode, dispDataObj.Consignee, trainList)
                End If
                '油種は持っている元に合わせる（最終的に元と一致する油種じゃないと認めない？）
                miOilTypeList = GetTargetOilType(sqlCon, dispDataObj.MiSalesOffice, dispDataObj.MiConsignee, dispDataObj.MiShippersCode)
                '構内取り用の画面表示クラス生成
                dispDataObj.MiDispData = New DispDataClass(daysList, mitrainList, miOilTypeList, dispDataObj.MiSalesOffice, dispDataObj.MiShippersCode, dispDataObj.MiConsignee)
                dispDataObj.MiDispData.SalesOfficeName = dispDataObj.MiSalesOfficeName
                dispDataObj.MiDispData.ShipperName = dispDataObj.MiShippersName
                dispDataObj.MiDispData.ConsigneeName = dispDataObj.MiConsigneeName
                dispDataObj.MiDispData.AsyncDeleteShipper = IsAsyncDeleteShipper(sqlCon, dispDataObj.MiDispData)
                '前週出荷平均の取得
                dispDataObj.MiDispData = GetLastShipAverage(sqlCon, dispDataObj.MiDispData)
                '既登録データ取得
                dispDataObj.MiDispData = GetTargetStockData(sqlCon, dispDataObj.MiDispData)
                '過去日以外の日付について受入数取得
                dispDataObj.MiDispData = GetReciveFromOrder(sqlCon, dispDataObj.MiDispData)
                dispDataObj = GetUkeireOilstock(sqlCon, dispDataObj)
                dispDataObj.MiDispData.RecalcStockList(False)
                'メインクラスに構内取り情報を紐づけ（参照設定）
                For Each suggestListItem In dispDataObj.SuggestList
                    Dim key = suggestListItem.Key
                    Dim item = dispDataObj.MiDispData.SuggestList(key).SuggestOrderItem
                    suggestListItem.Value.SuggestMiOrderItem = item
                    suggestListItem.Value.RelateMoveInside()
                Next 'suggestListItem
            Else
                dispDataObj = GetUkeireOilstock(sqlCon, dispDataObj)

            End If
            '既登録データ抽出
            If dispDataObj.TrainOperationList IsNot Nothing AndAlso
              (From itm In dispDataObj.TrainList.Values Where itm.TrainNo.Equals("川崎")).Any Then

            End If
        End Using
        '取得値を元に再計算
        dispDataObj.RecalcStockList(False)
        '****************************************
        '画面共通タイトルの左下に油槽所設定
        '****************************************
        Dim additionalCond As String = " and VALUE2 != '9' "
        Dim prmData = work.CreateFIXParam(salesOffice, "CONSIGNEEPATTERN", I_ADDITIONALCONDITION:=additionalCond)
        leftview.SetListBox(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, WW_DUMMY, prmData)
        leftview.ActiveListBox()
        Dim consigneeTag As New StringBuilder
        consigneeTag.AppendLine("<select id='selHeadConsignee' onchange='changeConsignee(this);'>")
        Dim itemString As String = ""
        For Each listItm In leftview.WF_LeftListBox.Items.Cast(Of ListItem)
            If dispDataObj.Consignee = listItm.Value Then
                itemString = "<option value='{0}' selected>{1}</option>"
            Else
                itemString = "<option value='{0}'>{1}</option>"
            End If
            consigneeTag.AppendFormat(itemString, listItm.Value, listItm.Text).AppendLine()
        Next
        consigneeTag.AppendLine("</select>")
        'Master.SetTitleLeftBottomText(dispDataObj.ConsigneeName)
        Master.SetTitleLeftBottomText(consigneeTag.ToString)
        '****************************************
        '生成したデータを画面に貼り付け
        '****************************************
        '1.提案リスト
        Me.pnlSuggestList.Attributes.Remove("data-otmode")
        If dispDataObj.ShowSuggestList = False AndAlso isOtTrainMode = False Then
            pnlSuggestList.Visible = False
            Me.spnInventoryDays.Visible = False
            Me.WF_ButtonGETEMPTURN.Visible = False 'OT且つ取り込み対象オーダーが無いので見せる意味がない
            Me.WF_ButtonAUTOSUGGESTION.Visible = False
            Me.WF_ButtonORDERLIST.Visible = False
            Me.WF_ButtonINPUTCLEAR.Visible = False
            'Me.WF_ButtonGETEMPTURN.Visible = False
        ElseIf isOtTrainMode = True Then
            pnlSuggestList.Visible = True
            Me.spnInventoryDays.Visible = False
            Me.WF_ButtonGETEMPTURN.Visible = True
            Me.WF_ButtonAUTOSUGGESTION.Visible = False
            Me.WF_ButtonORDERLIST.Visible = False
            Me.WF_ButtonINPUTCLEAR.Visible = False
            Me.pnlSuggestList.Attributes.Add("data-otmode", "1")
            frvSuggest.DataSource = New Object() {dispDataObj}
            frvSuggest.DataBind()


        Else
            pnlSuggestList.Visible = True
            Me.spnInventoryDays.Visible = True
            Me.WF_ButtonGETEMPTURN.Visible = True
            Me.WF_ButtonAUTOSUGGESTION.Visible = True
            Me.WF_ButtonORDERLIST.Visible = True
            Me.WF_ButtonINPUTCLEAR.Visible = True
            frvSuggest.DataSource = New Object() {dispDataObj}
            frvSuggest.DataBind()
        End If

        ''2.比重リスト
        'repWeightList.DataSource = dispDataObj.OilTypeList
        'repWeightList.DataBind()
        '3.在庫表
        repStockDate.DataSource = dispDataObj.StockDateDisplay
        repStockDate.DataBind()
        repStockOilTypeItem.DataSource = dispDataObj.StockList
        repStockOilTypeItem.DataBind()
        SaveThisScreenValue(dispDataObj)
        '4.在庫表表示マーク
        lstDispStockOilType.DataSource = oilTypeList.Values
        lstDispStockOilType.DataTextField = "OilName"
        lstDispStockOilType.DataValueField = "OilCode"
        lstDispStockOilType.DataBind()
        For Each lstItem As ListItem In lstDispStockOilType.Items
            lstItem.Selected = True
        Next
        '帳票条件初期値設定
        If setConsignee = "" Then
            Dim firstDay As Date
            Dim lastMonthDay As Date
            With (From itm In daysList Where itm.Value.IsDispArea).First.Value.ItemDate
                firstDay = New Date(.Year, .Month, 1)
                lastMonthDay = firstDay.AddMonths(1).AddDays(1)
            End With
            'ENEOS以外の荷主の場合、ENEOS帳票チェックを非表示
            If Not (shipper = "0005700010" AndAlso salesOffice = "011402") Then
                Me.divChkEneos.Visible = False
            End If
            Me.txtDownloadMonth.Text = Now.ToString("yyyy/MM")
            Me.txtReportFromDate.Text = firstDay.ToString("yyyy/MM/dd")
            'Me.txtReportToDate.Text = lastMonthDay.ToString("yyyy/MM/dd")
        End If
        If mesNo <> C_MESSAGE_NO.NORMAL Then
            Master.Output(mesNo, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        End If
    End Sub
    ''' <summary>
    ''' 自動提案ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonAUTOSUGGESTION_Click()
        '○ エラーレポート準備
        rightview.SetErrorReport("")
        '******************************
        '画面入力情報を取得
        '******************************
        Dim dispClass = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        '******************************
        '入力チェック処理実行
        '******************************
        If WW_Check(dispClass, WF_ButtonClick.Value) = False Then
            Return
        End If
        '******************************
        '自動提案処理実行
        '******************************
        Dim inventoryDays As Integer = 0
        inventoryDays = CInt(Me.WF_INVENTORYDAYS.Text)
        dispClass.AutoSuggest(inventoryDays)
        '******************************
        '画面情報再設定
        '******************************
        'コンストラクタで生成したデータを画面に貼り付け
        '1.提案リスト
        If dispClass.ShowSuggestList = False Then
            pnlSuggestList.Visible = False
        Else
            pnlSuggestList.Visible = True
            frvSuggest.DataSource = New Object() {dispClass}
            frvSuggest.DataBind()
        End If
        '2.比重リスト
        repWeightList.DataSource = dispClass.OilTypeList
        repWeightList.DataBind()
        '3.在庫表
        repStockDate.DataSource = dispClass.StockDateDisplay
        repStockDate.DataBind()
        repStockOilTypeItem.DataSource = dispClass.StockList
        repStockOilTypeItem.DataBind()
    End Sub
    ''' <summary>
    ''' 受注作成ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonORDERLIST_Click()
        '○ エラーレポート準備
        rightview.SetErrorReport("")
        '******************************
        '画面入力情報を取得
        '******************************
        Dim dispClass = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        '******************************
        '入力チェック処理実行
        '******************************
        If WW_Check(dispClass, WF_ButtonClick.Value) = False Then
            Return
        End If
        'SQL接続
        Dim orderInfoList As Dictionary(Of String, OrderItem)
        Dim historyNo As String = ""
        Dim retMsg As List(Of EntryOrderResultItm)
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            '既登録の受注情報取得
            orderInfoList = GetEmptyTurnOrder(sqlCon, dispClass)
            orderInfoList = GetEmptyTurnDetail(sqlCon, dispClass, orderInfoList)
            orderInfoList = GetEmptyTurnMaxDetailNo(sqlCon, dispClass, orderInfoList)
            '履歴番号取得
            Dim entryResult As EntryOrderResultItm = Nothing
            historyNo = GetNewOrderHistoryNo(sqlCon, entryResult)
            If historyNo = "" Then
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ERR, "受注履歴番号取得", needsPopUp:=True)
                Return
            End If
            '******************************
            '更新処理実行
            '******************************
            retMsg = EntryOrderInfo(sqlCon, dispClass, orderInfoList, historyNo, Me.Title)
            '構内取りありの場合(構内取り分のデータを登録
            If dispClass.HasMoveInsideItem Then
                '構内取り分既登録受注情報取得
                orderInfoList = GetEmptyTurnOrder(sqlCon, dispClass.MiDispData, dispClass.Consignee)
                orderInfoList = GetEmptyTurnDetail(sqlCon, dispClass.MiDispData, orderInfoList, True)
                orderInfoList = GetEmptyTurnMaxDetailNo(sqlCon, dispClass.MiDispData, orderInfoList)
                '構内取り分の受注情報登録
                Dim retMiMes = EntryOrderInfo(sqlCon, dispClass.MiDispData, orderInfoList, historyNo, Me.Title, dispClass)

                retMsg.AddRange(retMiMes) '構内取り以外の処理結果メッセージをマージ
            End If
        End Using
        If retMsg Is Nothing OrElse retMsg.Count = 0 Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Dim dummyLabel As New Label
            Dim CS0009MESSAGEout As New CS0009MESSAGEout
            CS0009MESSAGEout.MESSAGEBOX = dummyLabel
            CS0009MESSAGEout.NAEIW = C_NAEIW.WARNING
            Dim argStrSetting As String = "{8}{9}日付:{0}{8}{9}列車:{1}{8}{9}油種:{2}{8}{9}受注№:{3}{8}{9}明細№:{4}{8}{9}営業所:{5}{8}{9}荷主:{6}{8}{9}荷受人:{7}{8}"
            Dim nextBorder As String = "＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊" & ControlChars.CrLf
            For Each msgItm In retMsg
                If dummyLabel.Text <> "" Then
                    rightview.AddErrorReport(nextBorder)
                End If
                dummyLabel.Text = ""
                Dim argString = String.Format(argStrSetting, msgItm.AccDate, msgItm.TrainNo,
                                              msgItm.OilCode, msgItm.OrderNo, msgItm.DetailNo,
                                              msgItm.OfficeCode, msgItm.ShipperCode,
                                              msgItm.ConsigneeCode, ControlChars.CrLf, ControlChars.Tab)
                CS0009MESSAGEout.MESSAGENO = msgItm.MessageId
                CS0009MESSAGEout.PARA01 = argString

                CS0009MESSAGEout.CS0009MESSAGEout()

                rightview.AddErrorReport("■" & Trim(dummyLabel.Text))
            Next
            Master.Output(C_MESSAGE_NO.OIL_SKIPPED_ORDER_ENTRIES_EXISTS, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
        End If

    End Sub
    ''' <summary>
    ''' 空回日報取り込みボタン押下時
    ''' </summary>
    Protected Sub WF_ButtonGETEMPTURN_Click()
        '画面入力値を取得し画面データクラスへ反映
        Dim dispValues = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        '自動提案の値を一旦すべて0に変更
        dispValues.SuggestValueInputValueToZero(True)
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            SqlConnection.ClearPool(sqlCon)
            If dispValues.ShowSuggestList = False Then
                dispValues = EditEmptyTurnCarsNum(sqlCon, dispValues, True)
                dispValues = EditOtEmptyTurnCarsNum(sqlCon, dispValues)
                dispValues.RecalcStockList()
            Else
                dispValues = EditEmptyTurnCarsNum(sqlCon, dispValues)
                If dispValues.HasMoveInsideItem Then
                    dispValues.MiDispData.SuggestValueInputValueToZero(True)
                    dispValues.MiDispData = EditEmptyTurnCarsNum(sqlCon, dispValues.MiDispData)
                    dispValues.MiDispData.RecalcStockList()
                End If
            End If
        End Using
        '1.提案リスト
        If dispValues.ShowSuggestList = False AndAlso dispValues.IsOtTrainMode = False Then
            pnlSuggestList.Visible = False
        ElseIf dispValues.IsOtTrainMode = True Then
            pnlSuggestList.Visible = True
            frvSuggest.DataSource = New Object() {dispValues}
            frvSuggest.DataBind()
        Else
            pnlSuggestList.Visible = True
            frvSuggest.DataSource = New Object() {dispValues}
            frvSuggest.DataBind()
        End If
        '2.比重リスト
        repWeightList.DataSource = dispValues.OilTypeList
        repWeightList.DataBind()
        '3.在庫表
        repStockDate.DataSource = dispValues.StockDateDisplay
        repStockDate.DataBind()
        repStockOilTypeItem.DataSource = dispValues.StockList
        repStockOilTypeItem.DataBind()
    End Sub
    ''' <summary>
    ''' 入力値クリアボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonINPUTCLEAR_Click()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim dispValues = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        dispValues.InputValueToZero()
        If dispValues.HasMoveInsideItem Then
            dispValues.MiDispData.InputValueToZero()
        End If
        'コンストラクタで生成したデータを画面に貼り付け
        '1.提案リスト
        If dispValues.ShowSuggestList = False Then
            pnlSuggestList.Visible = False
        Else
            pnlSuggestList.Visible = True
            frvSuggest.DataSource = New Object() {dispValues}
            frvSuggest.DataBind()
        End If
        '2.比重リスト
        repWeightList.DataSource = dispValues.OilTypeList
        repWeightList.DataBind()
        '3.在庫表
        repStockDate.DataSource = dispValues.StockDateDisplay
        repStockDate.DataBind()
        repStockOilTypeItem.DataSource = dispValues.StockList
        repStockOilTypeItem.DataBind()
    End Sub
    ''' <summary>
    ''' 再計算ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonRECULC_Click()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim dispValues = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        If WW_Check(dispValues, WF_ButtonClick.Value) = False Then
            Return
        End If
        dispValues.RecalcStockList()
        If dispValues.HasMoveInsideItem Then
            dispValues.MiDispData.RecalcStockList()
        End If
        SaveThisScreenValue(dispValues)
        '在庫表再表示
        repStockDate.DataSource = dispValues.StockDateDisplay
        repStockDate.DataBind()
        repStockOilTypeItem.DataSource = dispValues.StockList
        repStockOilTypeItem.DataBind()
    End Sub
    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonUPDATE_Click()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim dispValues = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        If WW_Check(dispValues, WF_ButtonClick.Value) = False Then
            Return
        End If
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            Using sqlTran = sqlCon.BeginTransaction
                Dim errNum As String = ""
                '列車ロック情報更新
                Dim procDate As Date = Date.Now
                EntryTrainOperation(sqlCon, dispValues, errNum, procDate, sqlTran)
                '在庫表テーブル更新
                If EntryStockData(sqlCon, dispValues, errNum, procDate, sqlTran) = False Then
                    Return
                End If
                If dispValues.HasMoveInsideItem Then
                    If EntryStockData(sqlCon, dispValues.MiDispData, errNum, procDate, sqlTran) = False Then
                        Return
                    End If
                End If
                '提案表の値を格納
                If EntryUkeireOilstock(sqlCon, dispValues, errNum, Date.Now, sqlTran) = False Then
                    Return
                End If
                sqlTran.Commit()
            End Using

        End Using
        'コンストラクタで生成したデータを画面に貼り付け
        '1.提案リスト
        If dispValues.ShowSuggestList = False Then
            pnlSuggestList.Visible = False
        Else
            pnlSuggestList.Visible = True
            frvSuggest.DataSource = New Object() {dispValues}
            frvSuggest.DataBind()
        End If
        '2.比重リスト
        repWeightList.DataSource = dispValues.OilTypeList
        repWeightList.DataBind()
        '3.在庫表
        repStockDate.DataSource = dispValues.StockDateDisplay
        repStockDate.DataBind()
        repStockOilTypeItem.DataSource = dispValues.StockList
        repStockOilTypeItem.DataBind()
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
    End Sub
    ''' <summary>
    ''' ダウンロードボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonDownload_Click()
        '******************************
        '入力チェック（年月のみ）
        '******************************
        If WW_Check(Nothing, WF_ButtonClick.Value) = False Then
            Return
        End If
        '******************************
        '画面入力情報を取得
        '******************************
        Dim dispClass = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        '******************************
        'データ収集（画面情報とは異なる日付範囲）
        '******************************
        Dim printDataNormal As DispDataClass = Nothing
        'ENEOS帳票用データ
        Dim printDataHokushin As DispDataClass = Nothing
        Dim printDataKouhu As DispDataClass = Nothing

        With Nothing 'スコープ限定
            Dim baseDate As String = ""
            Dim toDate As String = ""
            Dim daySpan As Integer = 0

            Dim targetMonth As String = ""
            If Me.chkPrintENEOS.Checked = False Then
                '通常帳票のデータ取得（非ENEOS情報の取得）
                '１週前の平均を取る為TO - 7日分を保持するためFrom - 7日をする
                baseDate = CDate(Me.txtDownloadMonth.Text & "/01").AddDays(-7).ToString("yyyy/MM/dd")
                toDate = CDate(Me.txtDownloadMonth.Text & "/01").AddMonths(1).AddDays(1).ToString("yyyy/MM/dd")
                targetMonth = CDate(Me.txtDownloadMonth.Text & "/01").ToString("yyyy/MM")
                daySpan = CInt((CDate(toDate) - CDate(baseDate)).TotalDays) + 1
                printDataNormal = GetPrintData(baseDate, daySpan, targetMonth, dispClass)
            Else
                'ENEOS帳票のデータ取得
                baseDate = CDate(txtReportFromDate.Text).ToString("yyyy/MM/dd")
                toDate = CDate(txtReportFromDate.Text).AddDays(4).ToString("yyyy/MM/dd")
                daySpan = CInt((CDate(toDate) - CDate(baseDate)).TotalDays) + 1
                '北信と甲府の在庫データ取得
                Dim dispHokushin As New DispDataClass(dispClass.SalesOffice, dispClass.Shipper, CONST_CONSIGNEECODE_10)
                printDataHokushin = GetPrintData(baseDate, daySpan, targetMonth, dispHokushin)
                Dim dispKouhu As New DispDataClass(dispClass.SalesOffice, dispClass.Shipper, CONST_CONSIGNEECODE_20)
                printDataKouhu = GetPrintData(baseDate, daySpan, targetMonth, dispKouhu)

            End If

        End With
        '******************************
        '帳票作成処理の実行
        '******************************
        If Me.chkPrintENEOS.Checked = False Then
            Using repCbj = New OIT0004CustomReport(Master.MAPID, Master.MAPID & ".xlsx", printDataNormal)
                Dim url As String
                Try
                    url = repCbj.CreateExcelPrintData
                Catch ex As Exception
                    Return
                End Try
                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                Master.HideCustomPopUp()
            End Using
        Else
            Using repCbj = New OIT0004CustomReportENEOS(Master.MAPID, Master.MAPID & "ENEOS.xlsx", printDataHokushin, printDataKouhu)
                Dim url As String
                Try
                    url = repCbj.CreateExcelPrintData
                Catch ex As Exception
                    Return
                End Try
                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                Master.HideCustomPopUp()
            End Using
        End If

    End Sub
    ''' <summary>
    ''' 帳票出力用データ取得
    ''' </summary>
    ''' <param name="baseDate"></param>
    ''' <param name="daySpan"></param>
    ''' <param name="dispClass"></param>
    ''' <returns></returns>
    Private Function GetPrintData(baseDate As String, daySpan As Integer, targetMonth As String, dispClass As DispDataClass) As DispDataClass
        Dim printData As DispDataClass = Nothing
        Dim showLorry As String = ""

        Dim daysList As Dictionary(Of String, DaysItem)
        Dim oilTypeList As Dictionary(Of String, OilItem)
        Dim trainList As New Dictionary(Of String, TrainListItem)

        Dim mitrainList As Dictionary(Of String, TrainListItem) = Nothing
        Dim miOilTypeList As Dictionary(Of String, OilItem) = Nothing
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            '日付情報取得（祝祭日含む）
            daysList = GetTargetDateList(sqlCon, baseDate, daySpan:=daySpan, isPrint:=True)
            '対象油種取得
            oilTypeList = GetTargetOilType(sqlCon, dispClass.SalesOffice, dispClass.Consignee, dispClass.Shipper, targetMonth)
            '対象列車取得
            Dim canShowSuggestList As Boolean = Me.IsShowSuggestList(sqlCon, dispClass.Consignee)
            If canShowSuggestList Then
                trainList = GetTargetTrain(sqlCon, dispClass.SalesOffice, dispClass.Shipper, dispClass.Consignee)
                'システム管理外列車付与(受注作成しない、在庫計算だけ使う)
                trainList = GetUnmanagedTrain(sqlCon, trainList, dispClass.SalesOffice, dispClass.Shipper, dispClass.Consignee)
                '結果として取り扱い列車が0の場合提案一覧を表示できない為提案表を非表示にする
                If trainList Is Nothing OrElse trainList.Count = 0 Then
                    canShowSuggestList = False
                End If
            End If
            '抽出結果を画面データクラスに展開
            printData = New DispDataClass(daysList, trainList, oilTypeList, dispClass.SalesOffice, dispClass.Shipper, dispClass.Consignee)
            printData.SalesOfficeName = dispClass.SalesOfficeName
            printData.ShipperName = dispClass.ShipperName
            printData.ConsigneeName = dispClass.ConsigneeName
            '提案一覧表示可否設定
            printData.ShowSuggestList = canShowSuggestList
            '前週出荷平均の取得
            printData = GetLastShipAverage(sqlCon, printData)
            'ローリー初期表示判定
            showLorry = IsShowLorryValue(sqlCon, dispClass.Consignee)
            '構内取り有無取得
            printData = GetMoveInsideData(sqlCon, printData)
            '既登録データ取得
            printData = GetTargetStockData(sqlCon, printData, True)
            'ENEOS用の払出数量取得
            If targetMonth = "" Then
                printData = GetPrintTrainAmount(sqlCon, printData)
            End If
            '過去日以外の日付について受入数取得
            printData = GetReciveFromOrder(sqlCon, printData)
            '列車運行情報の取得
            printData = GetTrainOperation(sqlCon, printData)
            printData.AsyncDeleteShipper = IsAsyncDeleteShipper(sqlCon, printData)
            If dispClass.ShowSuggestList Then
                printData = GetPrintUkeireTrainNum(sqlCon, printData)
            Else
                printData = GetPrintOtUkeireTrainNum(sqlCon, printData)
            End If
            '構内取り設定がある場合、構内取りデータ取得
            If printData.HasMoveInsideItem Then
                '表構えの為親と構内取り元と同じ列車
                If canShowSuggestList Then
                    '構内取りではない油種「合計」文言を中計と変更
                    printData.SuggestOilNameList(DispDataClass.SUMMARY_CODE).OilName = "中計"
                    'Dim targetTrainList As List(Of String) = trainList.Keys.ToList
                    mitrainList = GetTargetTrain(sqlCon, printData.MiSalesOffice, printData.MiShippersCode, printData.Consignee, trainList)
                End If
                '油種は持っている元に合わせる（最終的に元と一致する油種じゃないと認めない？）
                miOilTypeList = GetTargetOilType(sqlCon, printData.MiSalesOffice, printData.MiConsignee, printData.MiShippersCode, targetMonth)
                '構内取り用の画面表示クラス生成
                printData.MiDispData = New DispDataClass(daysList, mitrainList, miOilTypeList, printData.MiSalesOffice, printData.MiShippersCode, printData.MiConsignee)
                printData.MiDispData.SalesOfficeName = printData.MiSalesOfficeName
                printData.MiDispData.ShipperName = printData.MiShippersName
                printData.MiDispData.ConsigneeName = printData.MiConsigneeName
                printData.MiDispData.AsyncDeleteShipper = IsAsyncDeleteShipper(sqlCon, printData.MiDispData)
                '前週出荷平均の取得
                printData.MiDispData = GetLastShipAverage(sqlCon, printData.MiDispData)
                '既登録データ取得
                printData.MiDispData = GetTargetStockData(sqlCon, printData.MiDispData, True)
                'ENEOS用の払出数量取得
                If targetMonth = "" Then
                    printData.MiDispData = GetPrintTrainAmount(sqlCon, printData.MiDispData)
                End If
                '過去日以外の日付について受入数取得
                printData.MiDispData = GetReciveFromOrder(sqlCon, printData.MiDispData)
                printData.MiDispData = GetPrintUkeireTrainNum(sqlCon, printData.MiDispData, True, printData.Consignee)
                printData = GetUkeireOilstock(sqlCon, printData)
                printData.MiDispData.RecalcStockList(False)
                'メインクラスに構内取り情報を紐づけ（参照設定）
                For Each suggestListItem In printData.SuggestList
                    Dim key = suggestListItem.Key
                    Dim item = printData.MiDispData.SuggestList(key).SuggestOrderItem
                    suggestListItem.Value.SuggestMiOrderItem = item
                    suggestListItem.Value.RelateMoveInside()
                Next 'suggestListItem
            Else
                printData = GetUkeireOilstock(sqlCon, printData)
            End If
            '取得値を元に再計算
            printData.RecalcStockList(False)
            ''空回日報の車数を埋める
            'If printData.ShowSuggestList = False Then
            '    printData = EditOtEmptyTurnCarsNum(sqlCon, printData)
            '    'printData.RecalcStockList()
            'Else
            '    printData = EditEmptyTurnCarsNum(sqlCon, printData)
            '    If printData.HasMoveInsideItem Then
            '        printData.MiDispData = EditEmptyTurnCarsNum(sqlCon, printData.MiDispData)
            '        'YprintData.MiDispData.RecalcStockList()
            '    End If
            'End If

        End Using
        Return printData
    End Function
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
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "txtReportFromDate"         '年月日
                                Dim targetDate As String = txtReportFromDate.Text.Trim
                                If targetDate = "" Then
                                    targetDate = Now.ToString("yyyy/MM/dd")
                                End If

                                .WF_Calendar.Text = CDate(targetDate).ToString("yyyy/MM/dd")
                            Case "txtReportToDate"
                                'Dim targetDate As String = txtReportToDate.Text.Trim
                                'If targetDate = "" Then
                                '    targetDate = Now.ToString("yyyy/MM/dd")
                                'End If
                                '.WF_Calendar.Text = CDate(targetDate).ToString("yyyy/MM/dd")
                        End Select
                        .ActiveCalendar()

                End Select
            End With

        End If

    End Sub
    ''' <summary>
    ''' 対象列車情報取得
    ''' </summary>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="salesOffice">営業所コード</param>
    ''' <param name="consignee">油槽所コード</param>
    ''' <returns>キー：列車No,値：列車アイテムクラス
    ''' 営業所、油槽所を元に取得した列車情報</returns>
    ''' <remarks>一旦戻り値が無い場合は提案表を出さない仕組みとする</remarks>
    Private Function GetTargetTrain(sqlCon As SqlConnection, salesOffice As String, shipper As String, consignee As String, Optional targetTrainList As Dictionary(Of String, TrainListItem) = Nothing) As Dictionary(Of String, TrainListItem)
        Try
            Dim resultVal As New Dictionary(Of String, TrainListItem)
            Dim retVal As New Dictionary(Of String, TrainListItem)
            Dim sqlStr As New StringBuilder
            sqlStr.AppendLine("SELECT TR.TRAINNO")     '列車No
            sqlStr.AppendLine("      ,isnull(TR.TRAINNAME,'') AS TRAINNAME")
            sqlStr.AppendLine("      ,isnull(TR.MAXTANK1,0)   AS MAXTANK")   '最大牽引数
            sqlStr.AppendLine("      ,TR.TSUMI")      '積置フラグ
            sqlStr.AppendLine("      ,TR.DEPSTATION") '発駅
            sqlStr.AppendLine("      ,DEPST.STATONNAME AS DEPSTATIONNAME") '発駅名
            sqlStr.AppendLine("      ,TR.ARRSTATION") '着駅
            sqlStr.AppendLine("      ,ARRST.STATONNAME AS ARRSTATIONNAME") '発駅名
            sqlStr.AppendLine("      ,isnull(TR.DEPDAYS,0)    AS DEPDAYS")    '発日日数
            sqlStr.AppendLine("      ,isnull(TR.MARGEDAYS,0)  AS MARGEDAYS")  '特継日数
            sqlStr.AppendLine("      ,isnull(TR.ARRDAYS,0)    AS ARRDAYS")    '積車着日数
            sqlStr.AppendLine("      ,isnull(TR.ACCDAYS,0)    AS ACCDAYS")    '受入日数
            sqlStr.AppendLine("      ,isnull(TR.EMPARRDAYS,0) AS EMPARRDAYS") '空車着日数
            sqlStr.AppendLine("      ,isnull(TR.USEDAYS,0)    AS USEDAYS")    '当日利用日数
            sqlStr.AppendLine("      ,FX.VALUE3               AS PLANTCODE")  'プラントコード
            sqlStr.AppendLine("      ,FX.VALUE4               AS PLANTNAME")  'プラント名
            sqlStr.AppendLine("      ,FX.VALUE7               AS PATCODE")    'パターンコード
            sqlStr.AppendLine("      ,FX.VALUE8               AS PATNAME")    'パターン名
            sqlStr.AppendLine("  FROM      OIL.VIW0001_FIXVALUE FX")
            sqlStr.AppendLine(" INNER JOIN OIL.OIM0007_TRAIN    TR")
            sqlStr.AppendLine("         ON FX.CAMPCODE = TR.OFFICECODE")
            sqlStr.AppendLine("        AND FX.CLASS    = @CLASS")
            sqlStr.AppendLine("        AND FX.VALUE1   = @SHIPPERCODE")
            sqlStr.AppendLine("        AND FX.KEYCODE  = TR.ARRSTATION")
            sqlStr.AppendLine(" INNER JOIN OIL.OIM0004_STATION    DEPST")
            sqlStr.AppendLine("         ON DEPST.STATIONCODE + DEPST.BRANCH = TR.DEPSTATION")
            sqlStr.AppendLine("        AND DEPST.DELFLG     = @DELFLG")
            sqlStr.AppendLine(" INNER JOIN OIL.OIM0004_STATION    ARRST")
            sqlStr.AppendLine("         ON ARRST.STATIONCODE + ARRST.BRANCH = TR.ARRSTATION")
            sqlStr.AppendLine("        AND ARRST.DELFLG     = @DELFLG")
            sqlStr.AppendLine(" WHERE FX.CAMPCODE   = @SALESOFFICE")
            sqlStr.AppendLine("   AND FX.VALUE5     = @CONSIGNEECODE")
            sqlStr.AppendLine("   AND FX.DELFLG     = @DELFLG")
            sqlStr.AppendLine("   AND TR.OFFICECODE = @SALESOFFICE")
            sqlStr.AppendLine("   AND TR.DELFLG     = @DELFLG")
            sqlStr.AppendLine(" ORDER BY TR.ZAIKOSORT,TR.TRAINNO,TR.TSUMI")


            Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar).Value = "PATTERNMASTER"
                    .Add("@SALESOFFICE", SqlDbType.NVarChar).Value = salesOffice
                    .Add("@SHIPPERCODE", SqlDbType.NVarChar).Value = shipper
                    .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = consignee
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Dim tlItem As TrainListItem
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    Dim trainCode As String = ""
                    Dim trainName As String = ""
                    Dim decMaxVol As Decimal = 0D

                    While sqlDr.Read
                        trainCode = Convert.ToString(sqlDr("TRAINNO"))
                        trainName = Convert.ToString(sqlDr("TRAINNAME"))
                        decMaxVol = Convert.ToDecimal(sqlDr("MAXTANK"))
                        tlItem = New TrainListItem(trainCode, trainName, decMaxVol) With {
                            .Tsumi = Convert.ToString(sqlDr("TSUMI")),
                            .DepStation = Convert.ToString(sqlDr("DEPSTATION")),
                            .DepStationName = Convert.ToString(sqlDr("DEPSTATIONNAME")),
                            .ArrStation = Convert.ToString(sqlDr("ARRSTATION")),
                            .ArrStationName = Convert.ToString(sqlDr("ARRSTATIONNAME")),
                            .DepDays = Convert.ToDecimal(sqlDr("DEPDAYS")),
                            .MargeDays = Convert.ToDecimal(sqlDr("MARGEDAYS")),
                            .ArrDays = Convert.ToDecimal(sqlDr("ARRDAYS")),
                            .AccDays = Convert.ToDecimal(sqlDr("ACCDAYS")),
                            .EmpArrDays = Convert.ToDecimal(sqlDr("EMPARRDAYS")),
                            .UseDays = Convert.ToDecimal(sqlDr("USEDAYS")),
                            .PlantCode = Convert.ToString(sqlDr("PLANTCODE")),
                            .PlantName = Convert.ToString(sqlDr("PLANTNAME")),
                            .PatCode = Convert.ToString(sqlDr("PATCODE")),
                            .PatName = Convert.ToString(sqlDr("PATNAME"))
                            }
                        '重複列車番号はスキップ
                        If resultVal.ContainsKey(tlItem.TrainNo) Then
                            Continue While
                        End If
                        '構内取り対応(構内取り元の列車Noが存在しない場合はスキップ)
                        If targetTrainList IsNot Nothing AndAlso targetTrainList.ContainsKey(tlItem.TrainNo) = False Then
                            Continue While
                        End If
                        resultVal.Add(tlItem.TrainNo, tlItem)
                    End While
                End Using

            End Using
            '構内取り側の列車と通常側の列車Noを合わせる
            If targetTrainList IsNot Nothing Then
                '構内取になく上部にある列車Noをダミー指定
                For i = 0 To targetTrainList.Count - 1 Step 1
                    Dim retValItm As TrainListItem
                    If resultVal.ContainsKey(targetTrainList.Keys(i)) Then
                        retValItm = resultVal(targetTrainList.Keys(i))
                    Else
                        '構内取側に存在しない場合は追加
                        With targetTrainList.Values(i)
                            retValItm = New TrainListItem(.TrainNo, .TrainName & "（構内取側列車未存在）", .MaxVolume)
                            retValItm.UnmanagedTrain = True 'ない場合は管理対象外とする
                        End With

                    End If
                    retVal.Add(retValItm.TrainNo, retValItm)
                Next i
            Else
                retVal = resultVal
            End If
            Return retVal
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, Me.Title)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0004C Select Train List"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元の後続処理を走らせたくないのでThrow 
        End Try
    End Function
    ''' <summary>
    ''' OT用の列車情報を対象期間内のオーダー情報より生成（なければそもそも作らない)
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="salesOffice"></param>
    ''' <param name="shipper"></param>
    ''' <param name="consignee"></param>
    ''' <returns></returns>
    Private Function GetTagetOtTrain(sqlCon As SqlConnection, salesOffice As String, shipper As String, consignee As String, daysList As Dictionary(Of String, DaysItem)) As Dictionary(Of String, TrainListItem)
        Try
            Dim resultVal As New Dictionary(Of String, TrainListItem)
            Dim retVal As New Dictionary(Of String, TrainListItem)
            Dim sqlStr As New StringBuilder
            '検索値の設定
            Dim dateFrom As String = daysList.First.Value.KeyString
            Dim dateTo As String = daysList.Last.Value.KeyString

            sqlStr.AppendLine("SELECT ODR.TRAINNO")
            sqlStr.AppendLine("      ,MIN(ODR.TRAINNAME) AS TRAINNAME")
            sqlStr.AppendLine("      ,ISNULL(MIN(TRA.ACCDAYS),0) AS ACCDAYS")
            sqlStr.AppendLine("      ,ISNULL(MIN(TRA.MAXTANK1),0) AS MAXVOLUME")
            sqlStr.AppendLine("      ,MIN(TRA.ZAIKOSORT) AS ZAIKOSORT")
            sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
            sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
            sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
            sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
            sqlStr.AppendLine("   AND DTL.OILCODE is not null")
            sqlStr.AppendLine(" LEFT JOIN OIL.OIM0007_TRAIN TRA")
            sqlStr.AppendLine("    ON TRA.OFFICECODE  =  @OFFICECODE")
            sqlStr.AppendLine("   AND TRA.TRAINNO     =  ODR.TRAINNO")
            sqlStr.AppendLine("   AND TRA.TSUMI       =  CASE WHEN ODR.STACKINGFLG = '1' THEN 'T' ELSE 'N' END")
            sqlStr.AppendLine("   AND TRA.DEPSTATION  =  ODR.DEPSTATION")
            sqlStr.AppendLine("   AND TRA.ARRSTATION  =  ODR.ARRSTATION")
            sqlStr.AppendLine(" WHERE ODR.LODDATE   BETWEEN @DATE_FROM AND @ADATE_TO")
            sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
            'sqlStr.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
            '荷主取得条件(JOINTコード考慮)↓
            sqlStr.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
            sqlStr.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
            sqlStr.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
            sqlStr.AppendLine("              ) ")
            sqlStr.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
            sqlStr.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
            sqlStr.AppendLine("              )")
            sqlStr.AppendLine("       )")
            '荷主取得条件(JOINTコード考慮)↑
            '第二荷受人取得条件↓
            'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
            sqlStr.AppendLine("   AND (( ODR.CONSIGNEECODE = @CONSIGNEECODE")
            sqlStr.AppendLine("          AND (    ISNULL(DTL.SECONDCONSIGNEECODE,'') = ''   ")
            sqlStr.AppendLine("                OR DTL.SECONDCONSIGNEECODE = ODR.CONSIGNEECODE ")
            sqlStr.AppendLine("              ) ")
            sqlStr.AppendLine("        ) OR  (     ODR.CONSIGNEECODE   <> @CONSIGNEECODE")
            sqlStr.AppendLine("                AND DTL.SECONDCONSIGNEECODE = @CONSIGNEECODE")
            sqlStr.AppendLine("              )")
            sqlStr.AppendLine("       )")
            '第二荷受人取得条件↑
            sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
            sqlStr.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
            sqlStr.AppendLine(" GROUP BY ODR.TRAINNO")
            sqlStr.AppendLine(" ORDER BY ZAIKOSORT, ODR.TRAINNO")
            Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = salesOffice
                    .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                    .Add("@ADATE_TO", SqlDbType.Date).Value = dateTo
                    .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = shipper
                    .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = consignee
                    .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        Dim trNo As String = Convert.ToString(sqlDr("TRAINNO"))
                        Dim trName As String = Convert.ToString(sqlDr("TRAINNAME"))
                        Dim trMaxVol As Decimal = CDec(Convert.ToString(sqlDr("MAXVOLUME")))
                        Dim trAccDays As Decimal = CDec(Convert.ToString(sqlDr("ACCDAYS")))
                        Dim trItem As New TrainListItem(trNo, trName, trMaxVol)
                        trItem.UnmanagedTrain = True
                        trItem.AccDays = trAccDays
                        retVal.Add(trItem.TrainNo, trItem)
                    End While
                End Using
            End Using
            Return retVal
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, Me.Title)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0004C Select Ot Train List"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元の後続処理を走らせたくないのでThrow 
        End Try
    End Function
    ''' <summary>
    ''' 基準日を元に日付リストを生成
    ''' </summary>
    ''' <param name="baseDate">基準日</param>
    ''' <param name="daySpan">引数BaseDateを含む設定した日付情報を取得(初期値:7)</param>
    ''' <returns>キー：日付、値：日付アイテムクラス</returns>
    ''' <remarks>基準日±発日期間の日付データを取得
    ''' 2020/12/22 30日間表示に変更ByVal daySpan As Integer = 7 → 30</remarks>
    Private Function GetTargetDateList(sqlCon As SqlConnection, baseDate As String, Optional ByVal daySpan As Integer = 30, Optional isPrint As Boolean = False) As Dictionary(Of String, DaysItem)
        Try
            Dim retVal As New Dictionary(Of String, DaysItem)
            '日付型に変換 検索条件よりわたってきている想定なので日付型に確実に変換できる想定
            Dim baseDtm As Date = Date.Parse(baseDate)
            '発日ずらしの期間を-する
            Dim daySpanBuff = SHIP_DATE_ADD_SPAN
            If isPrint Then
                daySpanBuff = 0
            End If
            baseDtm = baseDtm.AddDays(daySpanBuff * -1)
            Dim dispFromDtm As Date = Date.Parse(baseDate)
            Dim dispToDtm As Date = dispFromDtm.AddDays(daySpan - 1)
            Dim dtItm As DaysItem
            '基準日から引数期間のデータを生成
            For i As Integer = 0 To daySpan + (daySpanBuff * 2) - 1
                Dim currentDay As Date = baseDtm.AddDays(i)
                dtItm = New DaysItem(currentDay)
                dtItm.IsDispArea = False
                If (dispFromDtm <= currentDay AndAlso
                   currentDay <= dispToDtm) OrElse
                   isPrint = True Then
                    dtItm.IsDispArea = True
                End If
                retVal.Add(dtItm.KeyString, dtItm)
            Next i

            '祝祭日取得SQLの生成
            Dim sqlStr As New StringBuilder
            sqlStr.AppendLine("SELECT FORMAT(WORKINGYMD,'yyyy/MM/dd')  AS WORKINGYMD")
            sqlStr.AppendLine("      ,WORKINGTEXT")
            sqlStr.AppendLine("  FROM COM.OIS0021_CALENDAR")
            sqlStr.AppendLine(" WHERE WORKINGYMD BETWEEN @FROMDT AND @TODT")
            sqlStr.AppendLine("   AND WORKINGKBN >= @WORKINGKBN")
            sqlStr.AppendLine("   AND DELFLG      = @DELFLG")
            'DBより取得を行い祝祭日情報付与
            Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@FROMDT", SqlDbType.Date).Value = retVal.Keys.First
                    .Add("@TODT", SqlDbType.Date).Value = retVal.Keys.Last
                    .Add("@WORKINGKBN", SqlDbType.NVarChar).Value = "2"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = "0"
                End With

                Dim keyDate As String
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        keyDate = Convert.ToString(sqlDr("WORKINGYMD"))
                        If retVal.ContainsKey(keyDate) Then
                            With retVal(keyDate)
                                .IsHoliday = True '抽出結果は休日扱いなのでTrueを格納
                                .HolidayName = Convert.ToString(sqlDr("WORKINGTEXT"))
                            End With
                        End If
                    End While
                End Using 'sqlDr

            End Using 'sqlCmd
            Return retVal
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, Me.Title)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0004C Select TargetDateList"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Throw '呼出し元の後続処理を走らせたくないのでThrow 
        End Try

    End Function
    ''' <summary>
    ''' 対象油種ディクショナリ作成
    ''' </summary>
    ''' <param name="sqlCon">SQL接続オブジェクト</param>
    ''' <param name="salesOffice">営業所</param>
    ''' <param name="consignee">油槽所（荷受人）コード</param>
    ''' <param name="shipper">荷主コード</param>
    ''' <returns>キー:油種コード、値：油種アイテムクラス</returns>
    Private Function GetTargetOilType(sqlCon As SqlConnection, salesOffice As String, consignee As String, shipper As String, Optional printMonth As String = "") As Dictionary(Of String, OilItem)
        Dim retVal As New Dictionary(Of String, OilItem)
        '営業所に対応する油種コード取得
        Dim sqlStr As New StringBuilder
        sqlStr.AppendLine("SELECT FV.KEYCODE  AS OILCODE")
        sqlStr.AppendLine("      ,FV.VALUE1   AS OILNAME")
        sqlStr.AppendLine("      ,FV.VALUE2   AS SEGMENTOILCODE")
        sqlStr.AppendLine("      ,FV.VALUE3   AS SEGMENTOILNAME")
        sqlStr.AppendLine("      ,FV.VALUE4   AS OTOILCODE")
        sqlStr.AppendLine("      ,FV.VALUE5   AS OTOILNAME")
        sqlStr.AppendLine("      ,FV.VALUE6   AS SHIPPEROILCODE")
        sqlStr.AppendLine("      ,FV.VALUE7   AS SHIPPEROILNAME")
        sqlStr.AppendLine("      ,FV.VALUE8   AS BIGOILCODE")
        sqlStr.AppendLine("      ,FV.VALUE10  AS MIDDLEOILCODE")
        sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FV")
        sqlStr.AppendLine(" WHERE FV.CAMPCODE  = @CAMPCODE")
        sqlStr.AppendLine("   AND FV.CLASS     = @CLASS")
        sqlStr.AppendLine("   AND FV.DELFLG    = @DELFLG")
        sqlStr.AppendLine("   AND FV.VALUE12  != @STOCKFLG")
        sqlStr.AppendLine(" ORDER BY KEYCODE")

        'タンク容量、目標在庫率、D/S、開始年月、終了年月取得用
        Dim sqlConsigneeOilType As New StringBuilder
        sqlConsigneeOilType.AppendLine("SELECT FV.VALUE1 AS OILCODE")
        sqlConsigneeOilType.AppendLine("      ,FV.VALUE2 AS FROMMD")
        sqlConsigneeOilType.AppendLine("      ,FV.VALUE3 AS TOMD")
        sqlConsigneeOilType.AppendLine("      ,FV.VALUE4 AS TANKCAP")
        sqlConsigneeOilType.AppendLine("      ,FV.VALUE5 AS TARGETCAPRATE")
        sqlConsigneeOilType.AppendLine("      ,FV.VALUE6 AS DS")
        sqlConsigneeOilType.AppendLine(" FROM OIL.VIW0001_FIXVALUE FV")
        sqlConsigneeOilType.AppendLine("WHERE FV.CAMPCODE = @CAMPCODE")
        sqlConsigneeOilType.AppendLine("  AND FV.CLASS    = @CLASS")
        sqlConsigneeOilType.AppendLine("  AND FV.KEYCODE  = @CONSIGNEE")
        sqlConsigneeOilType.AppendLine("  AND FV.VALUE7　 = @SHIPPERSCODE")
        sqlConsigneeOilType.AppendLine("  AND FV.DELFLG   = @DELFLG")
        '帳票用の前年同月平均積高の取得
        Dim sqlPrintAmountAvarage As New StringBuilder
        sqlPrintAmountAvarage.AppendLine("SELECT DTL.OILCODE")
        sqlPrintAmountAvarage.AppendLine("     , SUM(isnull(DTL.CARSAMOUNT,0)) / SUM(isnull(DTL.CARSNUMBER,0))    AS AMOUNTAVE")
        sqlPrintAmountAvarage.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlPrintAmountAvarage.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlPrintAmountAvarage.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlPrintAmountAvarage.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlPrintAmountAvarage.AppendLine("   AND DTL.OILCODE is not null")
        sqlPrintAmountAvarage.AppendLine(" WHERE ODR.LODDATE  　BETWEEN @DATE_FROM AND @DATE_TO")
        sqlPrintAmountAvarage.AppendLine("   AND ODR.ACTUALLODDATE is not null")
        sqlPrintAmountAvarage.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        sqlPrintAmountAvarage.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlPrintAmountAvarage.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlPrintAmountAvarage.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlPrintAmountAvarage.AppendLine("              ) ")
        sqlPrintAmountAvarage.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlPrintAmountAvarage.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlPrintAmountAvarage.AppendLine("              )")
        sqlPrintAmountAvarage.AppendLine("       )")
        sqlPrintAmountAvarage.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlPrintAmountAvarage.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        sqlPrintAmountAvarage.AppendLine(" GROUP BY DTL.OILCODE")

        'DBより取得を行い取得情報付与
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            Dim paramCampCode = sqlCmd.Parameters.Add("@CAMPCODE", SqlDbType.NVarChar)
            Dim paramClass = sqlCmd.Parameters.Add("@CLASS", SqlDbType.NVarChar)
            '2つのSQLで変わらない（or不要なパラメータ)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = "0"
                .Add("@STOCKFLG", SqlDbType.NVarChar).Value = "9" '不等号条件
                .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = consignee
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = shipper
            End With
            paramCampCode.Value = salesOffice
            paramClass.Value = "PRODUCTPATTERN"

            Dim oilCode As String
            Dim oilName As String
            Dim segmentOilCode As String
            Dim segmentOilName As String
            Dim otOilCode As String
            Dim otOilName As String
            Dim shipperOilCode As String
            Dim shipperOilName As String

            Dim bigOilCode As String
            Dim midOilCode As String
            Dim oilItm As OilItem
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                While sqlDr.Read
                    oilCode = Convert.ToString(sqlDr("OILCODE"))
                    oilName = Convert.ToString(sqlDr("OILNAME"))
                    segmentOilCode = Convert.ToString(sqlDr("SEGMENTOILCODE"))
                    segmentOilName = Convert.ToString(sqlDr("SEGMENTOILNAME"))
                    otOilCode = Convert.ToString(sqlDr("OTOILCODE"))
                    otOilName = Convert.ToString(sqlDr("OTOILNAME"))
                    shipperOilCode = Convert.ToString(sqlDr("SHIPPEROILCODE"))
                    shipperOilName = Convert.ToString(sqlDr("SHIPPEROILNAME"))
                    bigOilCode = Convert.ToString(sqlDr("BIGOILCODE"))
                    midOilCode = Convert.ToString(sqlDr("MIDDLEOILCODE"))
                    oilItm = New OilItem(oilCode, oilName, bigOilCode, midOilCode) With {
                    .SegmentOilCode = segmentOilCode,
                    .SegmentOilName = segmentOilName,
                    .OtOilCode = otOilCode,
                    .OtOilName = otOilName,
                    .ShipperOilCode = shipperOilCode,
                    .ShipperOilName = shipperOilName
                    }
                    retVal.Add(oilItm.OilCode, oilItm)
                End While
            End Using 'sqlDr

            '油種毎の付帯情報を格納（大本テーブルは油槽所諸元マスタ[OIM0015_SYOGEN]）
            sqlCmd.CommandText = sqlConsigneeOilType.ToString
            paramCampCode.Value = C_DEFAULT_DATAKEY 'Default
            paramClass.Value = "SYOGEN"
            Dim syoGenOilTypeList As New List(Of String)
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                Dim syoGenOilCode As String = ""
                While sqlDr.Read
                    syoGenOilCode = Convert.ToString(sqlDr("OILCODE"))
                    syoGenOilTypeList.Add(syoGenOilCode)
                    If retVal.ContainsKey(syoGenOilCode) Then
                        With retVal(syoGenOilCode)
                            .MaxTankCap = Decimal.Parse(Convert.ToString(sqlDr("TANKCAP")))
                            .TankCapRate = Decimal.Parse(Convert.ToString(sqlDr("TARGETCAPRATE")))
                            .DS = Decimal.Parse(Convert.ToString(sqlDr("DS")))
                            .FromMd = Convert.ToString(sqlDr("FROMMD"))
                            .FromMd = CInt(Split(.FromMd, "/")(0)).ToString("00") & "/" & CInt(Split(.FromMd, "/")(1)).ToString("00")
                            .ToMd = Convert.ToString(sqlDr("TOMD"))
                            .ToMd = CInt(Split(.ToMd, "/")(0)).ToString("00") & "/" & CInt(Split(.ToMd, "/")(1)).ToString("00")
                            If .FromMd.Equals("01/01") AndAlso .ToMd.Equals("12/31") Then
                                .IsOilTypeSwitch = True
                            End If
                        End With
                    End If
                End While
            End Using
            '油槽所諸元マスタにかからない油種を表示対象から除外
            Dim removeKeys = (From itm In retVal Where Not syoGenOilTypeList.Contains(itm.Key) Select itm.Key).ToList
            For Each key In removeKeys
                retVal.Remove(key)
            Next
            '帳票用の前年同月平均積高の取得
            If printMonth <> "" Then
                Dim fromDate As String = CDate(printMonth & "/01").AddYears(-1).ToString("yyyy/MM/dd")
                Dim toDate As String = CDate(fromDate).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd")
                With sqlCmd.Parameters
                    .Add("OFFICECODE", SqlDbType.NVarChar).Value = salesOffice
                    .Add("DATE_FROM", SqlDbType.Date).Value = fromDate
                    .Add("DATE_TO", SqlDbType.Date).Value = toDate
                    .Add("ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
                End With
                sqlCmd.CommandText = sqlPrintAmountAvarage.ToString
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        Dim aveOilCode As String = Convert.ToString(sqlDr("OILCODE"))
                        If retVal.ContainsKey(aveOilCode) AndAlso
                           Convert.ToString(sqlDr("AMOUNTAVE")) <> "" Then
                            retVal(aveOilCode).PrintStockAmountAverage = CDec(sqlDr("AMOUNTAVE"))
                        End If

                    End While
                End Using
            End If
        End Using 'sqlCmd
        Return retVal

    End Function
    ''' <summary>
    ''' 受入データをオーダーより取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    ''' <remarks>受入予定数を設定（過去は在庫テーブルからとる？）</remarks>
    Private Function GetReciveFromOrder(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        Dim sqlStr As New StringBuilder
        Dim retVal = dispData
        'Return retVal '2020/4/24 受注より取得は廃止(完全に不要と判明したら関数まるまる削除）
        '検索値の設定(過去日じゃない日付リストを取得）
        Dim qdataVal = From itm In dispData.StockDate
                       Order By itm.Value.KeyString
                       Select itm.Value.KeyString
        '全て過去日の場合は在庫テーブルから取得するためオーダーから取得しない
        If qdataVal.Any = False Then
            Return retVal
        End If

        Dim dateFrom As String = qdataVal.First '過去日ではない内最初
        Dim dateTo As String = qdataVal.Last    '過去日ではない内最後
        'ACCDATE[受入日（予定）]を元に取得（変更の場合は条件と抽出両方の項目忘れずに）
        sqlStr.AppendLine("SELECT DTL.OILCODE")
        sqlStr.AppendLine("  　 , format(ODR.ACCDATE,'yyyy/MM/dd') AS TARGETDATE")
        sqlStr.AppendLine("     , SUM(isnull(DTL.CARSAMOUNT,0))    AS AMOUNT")
        sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlStr.AppendLine("   AND DTL.OILCODE is not null")
        sqlStr.AppendLine(" WHERE ODR.ACCDATE  　BETWEEN @DATE_FROM AND @DATE_TO")
        sqlStr.AppendLine("   AND ODR.ACTUALLODDATE is not null")
        sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        'sqlStr.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStr.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStr.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '荷主取得条件(JOINTコード考慮)↑
        'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        '第二荷受人取得条件↓
        'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND (( ODR.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.SECONDCONSIGNEECODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.SECONDCONSIGNEECODE = ODR.CONSIGNEECODE  ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     ODR.CONSIGNEECODE   <> @CONSIGNEECODE")
        sqlStr.AppendLine("                AND DTL.SECONDCONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '第二荷受人取得条件↑
        sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStr.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        sqlStr.AppendLine(" GROUP BY DTL.OILCODE,ODR.ACCDATE")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@DATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String
                    Dim recvVal As Decimal = 0D
                    Dim targetDate As String = ""
                    While sqlDr.Read
                        oilCode = Convert.ToString(sqlDr("OILCODE"))
                        '油種未設定または対象油種を持っていないレコードはスキップ
                        If oilCode = "" OrElse retVal.StockList.ContainsKey(oilCode) = False Then
                            Continue While
                        End If
                        targetDate = Convert.ToString(sqlDr("TARGETDATE"))
                        With retVal.StockList(oilCode)
                            If .StockItemList.ContainsKey(targetDate) Then
                                recvVal = Decimal.Parse(Convert.ToString(sqlDr("AMOUNT")))
                                With .StockItemList(targetDate)
                                    .Receive = recvVal.ToString
                                End With
                            End If
                        End With

                    End While
                End If
            End Using 'sqlDr
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 前週出荷平均取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    ''' <remarks>TODO 一旦FromとToは表示日付From Toから7日引いたものとする</remarks>
    Private Function GetLastShipAverage(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        'これもしかすると在庫テーブルから先週の払出量かと
        'オーダーは受入数な気がする(Consigneeしかないので)
        Dim sqlStr As New StringBuilder
        Dim retVal = dispData
        Dim dispDateList = From dateItm In dispData.StockDate Where dateItm.Value.IsDispArea
        '検索値の設定
        Dim dateFrom As String = dispDateList.First.Value.ItemDate.AddDays(-7).ToString("yyyy/MM/dd")
        'Dim dateTo As String = dispData.StockDate.Last.Value.ItemDate.AddDays(-7).ToString("yyyy/MM/dd")
        Dim dateTo As String = dispDateList.First.Value.ItemDate.AddDays(-1).ToString("yyyy/MM/dd")
        sqlStr.AppendLine("SELECT DTL.OILCODE")
        sqlStr.AppendLine("     , SUM(isnull(DTL.CARSAMOUNT,0))                    AS CARSAMOUNT")
        sqlStr.AppendLine("     , DATEDIFF(day ,@ACTUALDATE_FROM ,@ACTUALDATE_TO)  AS DAYSPAN")
        sqlStr.AppendLine("     , ROUND(SUM(isnull(DTL.CARSAMOUNT,0)) / DATEDIFF(day ,@ACTUALDATE_FROM ,@ACTUALDATE_TO),0)  AS SHIPAVERAGE")
        sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlStr.AppendLine("   AND DTL.OILCODE is not null")
        sqlStr.AppendLine(" WHERE ODR.ACTUALLODDATE  BETWEEN @ACTUALDATE_FROM AND @ACTUALDATE_TO")
        sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        'sqlStr.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStr.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStr.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '荷主取得条件(JOINTコード考慮)↑
        sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStr.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        sqlStr.AppendLine(" GROUP BY DTL.OILCODE")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@ACTUALDATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@ACTUALDATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String
                    Dim avaVal As Decimal = 0D
                    While sqlDr.Read
                        oilCode = Convert.ToString(sqlDr("OILCODE"))
                        '油種未設定または対象油種を持っていないレコードはスキップ
                        If oilCode = "" OrElse retVal.StockList.ContainsKey(oilCode) = False Then
                            Continue While
                        End If

                        avaVal = Decimal.Parse(Convert.ToString(sqlDr("SHIPAVERAGE")))
                        retVal.StockList(oilCode).LastShipmentAve = avaVal

                    End While
                End If
            End Using 'sqlDr
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 在庫テーブルより既登録データを取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    ''' <remarks>実績が無く未来日の場合は１年前の実績を払い出しに設定。
    ''' TODO３号⇔軽油読み替え</remarks>
    Private Function GetTargetStockData(sqlCon As SqlConnection, dispData As DispDataClass, Optional isPrint As Boolean = False) As DispDataClass
        Dim retVal As DispDataClass = dispData

        Dim fromDateObj = dispData.StockDate.Values.FirstOrDefault
        Dim toDateObj = dispData.StockDate.Values.LastOrDefault
        '期間外の前日夕在庫取得判定用
        Dim prevDate As String = fromDateObj.ItemDate.AddDays(-1).ToString("yyyy/MM/dd")
        '一年前の過去日取得用
        Dim foundRecList As New List(Of String) '取得必要過去日リスト

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT format(OS.STOCKYMD,'yyyy/MM/dd') AS STOCKYMD")
        sqlStat.AppendLine("      ,OS.OILCODE")
        If isPrint Then
            sqlStat.AppendLine("      ,isnull(sum(OS.MORSTOCK),0)    AS MORSTOCK")
            sqlStat.AppendLine("      ,isnull(sum(OS.SHIPPINGVOL),0) AS SHIPPINGVOL")
            sqlStat.AppendLine("      ,isnull(sum(OS.ARRVOL),0)      AS ARRVOL")
            sqlStat.AppendLine("      ,isnull(sum(OS.ARRLORRYVOL),0) AS ARRLORRYVOL")
            sqlStat.AppendLine("      ,isnull(sum(OS.EVESTOCK),0)    AS EVESTOCK")
        Else
            sqlStat.AppendLine("      ,isnull(OS.MORSTOCK,0)    AS MORSTOCK")
            sqlStat.AppendLine("      ,isnull(OS.SHIPPINGVOL,0) AS SHIPPINGVOL")
            sqlStat.AppendLine("      ,isnull(OS.ARRVOL,0)      AS ARRVOL")
            sqlStat.AppendLine("      ,isnull(OS.ARRLORRYVOL,0) AS ARRLORRYVOL")
            sqlStat.AppendLine("      ,isnull(OS.EVESTOCK,0)    AS EVESTOCK")

        End If
        sqlStat.AppendLine("  FROM OIL.OIT0001_OILSTOCK OS")
        sqlStat.AppendLine(" WHERE OS.STOCKYMD BETWEEN dateadd(day, -1, @FROMDATE) AND @TODATE")
        If isPrint = False Then
            sqlStat.AppendLine("   AND OS.OFFICECODE    = @OFFICECODE")
        End If
        sqlStat.AppendLine("   AND OS.SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStat.AppendLine("   AND OS.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("   AND OS.DELFLG        = @DELFLG")
        If isPrint Then
            sqlStat.AppendLine(" GROUP BY OS.STOCKYMD,OS.OILCODE")
        End If
        sqlStat.AppendLine(" ORDER BY OS.STOCKYMD,OS.OILCODE")

        '保存済みの在庫情報を取得
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            '固定パラメータの設定
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            '可変パラメータ
            Dim paramFromDate = sqlCmd.Parameters.Add("@FROMDATE", SqlDbType.Date)
            Dim paramToDate = sqlCmd.Parameters.Add("@TODATE", SqlDbType.Date)

            paramFromDate.Value = fromDateObj.KeyString
            paramToDate.Value = toDateObj.KeyString
            '**************************
            '指定年月の情報取得
            '**************************
            Using sqlDr = sqlCmd.ExecuteReader
                Dim curDate As String = ""
                Dim oilCode As String = ""
                Dim stockListCol As DispDataClass.StockListCollection = Nothing
                Dim dateValue As DispDataClass.StockListItem = Nothing
                While sqlDr.Read
                    curDate = Convert.ToString(sqlDr("STOCKYMD"))
                    oilCode = Convert.ToString(sqlDr("OILCODE"))
                    '対象油種を保持していない場合
                    If dispData.StockList.ContainsKey(oilCode) = False Then
                        Continue While '次のループへ
                    End If
                    stockListCol = dispData.StockList(oilCode)
                    '前日夕在庫の設定
                    If curDate.Equals(prevDate) Then
                        With stockListCol.StockItemList.FirstOrDefault.Value
                            .LastEveningStock = Decimal.Parse(Convert.ToString(sqlDr("EVESTOCK")))
                            .MorningStock = .LastEveningStock.ToString
                        End With
                        Continue While '前日日付をキーとするデータはないので次のループへ
                    End If
                    'ループ対象の日付毎データが存在しない場合
                    If stockListCol.StockItemList.ContainsKey(curDate) = False Then
                        Continue While '次のループへ
                    End If
                    foundRecList.Add(oilCode & "," & curDate)
                    dateValue = stockListCol.StockItemList(curDate)
                    dateValue.MorningStock = Convert.ToString(sqlDr("MORSTOCK")) '朝在庫
                    dateValue.Send = Convert.ToString(sqlDr("SHIPPINGVOL")) '払出
                    dateValue.Receive = Decimal.Parse(Convert.ToString(sqlDr("ARRVOL"))).ToString   '受入
                    dateValue.ReceiveFromLorry = Convert.ToString(sqlDr("ARRLORRYVOL")) '払出
                End While 'sqlDr.Read
            End Using 'sqlDr
            '上記抽出結果なし且つ範囲が未来日部分に関して１年前の過去実績の払出を設定
            '前年同様日考慮抽出⇒52週分前の日数を引く
            Dim dateSpan As Integer = 7 * 52 '52週分前の日数を引く
            paramFromDate.Value = fromDateObj.ItemDate.AddDays(dateSpan * -1).ToString("yyyy/MM/dd")
            paramToDate.Value = toDateObj.ItemDate.AddYears(dateSpan * -1).ToString("yyyy/MM/dd")
            Using sqlDr = sqlCmd.ExecuteReader
                Dim curDate As String = ""
                Dim oilCode As String = ""
                Dim stockListCol As DispDataClass.StockListCollection = Nothing
                Dim dateValue As DispDataClass.StockListItem = Nothing
                While sqlDr.Read
                    curDate = Convert.ToString(sqlDr("STOCKYMD"))
                    oilCode = Convert.ToString(sqlDr("OILCODE"))
                    curDate = CDate(curDate).AddDays(dateSpan).ToString("yyyy/MM/dd")

                    '対象油種を保持していない場合
                    If dispData.StockList.ContainsKey(oilCode) = False Then
                        Continue While '次のループへ
                    End If
                    '対象日付が無いまたは過去日の場合はスキップ
                    If dispData.StockDate.ContainsKey(curDate) = False OrElse
                       dispData.StockDate(curDate).IsPastDay Then
                        Continue While '次のループへ
                    End If
                    '既に在庫データを保持していた場合は過去データで塗り替えない為スキップ
                    If (foundRecList.Contains(oilCode & "," & curDate)) Then
                        Continue While '次のループへ
                    End If

                    stockListCol = dispData.StockList(oilCode)
                    dateValue = stockListCol.StockItemList(curDate)
                    dateValue.Send = Convert.ToString(sqlDr("SHIPPINGVOL")) '払出
                End While
            End Using 'sqlDr 一年前抽出
        End Using 'sqlCmd

        Return retVal
    End Function
    ''' <summary>
    ''' ENEOS帳票用の受入数量の抽出
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    Private Function GetPrintTrainAmount(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        Dim retVal As DispDataClass = dispData
        If {"10", "20"}.Contains(dispData.Consignee) = False Then
            Return retVal
        End If
        Dim fromDateObj = dispData.StockDate.Values.FirstOrDefault
        Dim toDateObj = dispData.StockDate.Values.LastOrDefault
        Dim hasKeyList As New Dictionary(Of String, String)
        Dim sqlStat As New StringBuilder
        '油種コードをフィールド名に割り当てる変数
        Dim oilCodeToFieldNameList As New Dictionary(Of String, String) From {
            {"1101", "RTANK1"}, {"1001", "HTANK1"}, {"1301", "TTANK1"}, {"1302", "MTTANK1"},
            {"1401", "KTANK1"}, {"1404", "K3TANK1"}, {"2201", "LTANK1"}, {"2101", "ATANK1"}}
        Dim isHokushin As Boolean = False
        If dispData.Consignee = "10" Then
            isHokushin = True
        End If
        sqlStat.AppendLine("SELECT DTL.OILCODE")
        'sqlStat.AppendLine("     , format(ODR.ACTUALLODDATE,'yyyy/MM/dd') AS TARGETDATE")
        sqlStat.AppendLine("     , format(ODR.LODDATE,'yyyy/MM/dd') AS TARGETDATE")
        sqlStat.AppendLine("     , SUM(isnull(DTL.CARSAMOUNT,0))                    AS CARSAMOUNT")
        If isHokushin Then
            sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '5463' THEN DTL.CARSAMOUNT ELSE 0 END),0) AS AMOUNT1 ")
            sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '2085' THEN DTL.CARSAMOUNT ELSE 0 END),0) AS AMOUNT2 ")
            sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '8471' THEN DTL.CARSAMOUNT ELSE 0 END),0) AS AMOUNT3 ")

            'sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '5463' THEN DTL.CARSNUMBER ELSE 0 END),0) AS AMOUNT1 ")
            'sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '2085' THEN DTL.CARSNUMBER ELSE 0 END),0) AS AMOUNT2 ")
            'sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '8471' THEN DTL.CARSNUMBER ELSE 0 END),0) AS AMOUNT3 ")

        Else
            sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '81' AND DTL.FIRSTRETURNFLG = '1' THEN DTL.CARSAMOUNT ELSE 0 END),0) AS AMOUNT1 ")
            sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO IN ('81','83') AND DTL.FIRSTRETURNFLG <> '1' AND DTL.AFTERRETURNFLG <> '1' THEN DTL.CARSAMOUNT ELSE 0 END),0) AS AMOUNT2 ")
            sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO IN ('81','83') AND DTL.AFTERRETURNFLG = '1' THEN DTL.CARSAMOUNT ELSE 0 END),0) AS AMOUNT3 ")

            'sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO = '81' AND DTL.FIRSTRETURNFLG = '1' THEN DTL.CARSNUMBER ELSE 0 END),0) AS AMOUNT1 ")
            'sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO IN ('81','83') AND DTL.FIRSTRETURNFLG <> '1' AND DTL.AFTERRETURNFLG <> '1' THEN DTL.CARSNUMBER ELSE 0 END),0) AS AMOUNT2 ")
            'sqlStat.AppendLine("     , ISNULL(SUM(CASE WHEN ODR.TRAINNO IN ('81','83') AND DTL.AFTERRETURNFLG = '1' THEN DTL.CARSNUMBER ELSE 0 END),0) AS AMOUNT3 ")
        End If
        sqlStat.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlStat.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlStat.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlStat.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlStat.AppendLine("   AND DTL.OILCODE is not null")
        'sqlStat.AppendLine(" WHERE ODR.ACTUALLODDATE  BETWEEN @FROMDATE AND @TODATE")
        sqlStat.AppendLine(" WHERE ODR.LODDATE  BETWEEN @FROMDATE AND @TODATE")
        sqlStat.AppendLine("   AND ODR.ACTUALLODDATE is not null")
        sqlStat.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        'sqlStat.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStat.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStat.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStat.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStat.AppendLine("              ) ")
        sqlStat.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStat.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStat.AppendLine("              )")
        sqlStat.AppendLine("       )")
        '荷主取得条件(JOINTコード考慮)↑
        'sqlStat.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        '第二荷受人取得条件↓
        'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStat.AppendLine("   AND (( ODR.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("          AND (    ISNULL(DTL.SECONDCONSIGNEECODE,'') = ''   ")
        sqlStat.AppendLine("                OR DTL.SECONDCONSIGNEECODE = ODR.CONSIGNEECODE ")
        sqlStat.AppendLine("              ) ")
        sqlStat.AppendLine("        ) OR  (     ODR.CONSIGNEECODE   <> @CONSIGNEECODE")
        sqlStat.AppendLine("                AND DTL.SECONDCONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("              )")
        sqlStat.AppendLine("       )")
        '第二荷受人取得条件↑

        sqlStat.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStat.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        'sqlStat.AppendLine(" GROUP BY DTL.OILCODE,ODR.ACTUALLODDATE")
        sqlStat.AppendLine(" GROUP BY DTL.OILCODE,ODR.LODDATE")

        Dim sqlZaikoTrainAmount As New StringBuilder
        sqlZaikoTrainAmount.AppendLine("SELECT format(STOCKYMD,'yyyy/MM/dd') AS TARGETDATE")
        If isHokushin Then
            sqlZaikoTrainAmount.AppendLine("      ,TRAINNO")
        End If
        For Each fieldName In oilCodeToFieldNameList.Values
            sqlZaikoTrainAmount.AppendFormat("      ,ISNULL(SUM({0}),0)  AS {0}", fieldName).AppendLine()
        Next
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(RTANK1)  AS RTANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(HTANK1)  AS HTANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(TTANK1)  AS TTANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(MTTANK1) AS MTTANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(KTANK1)  AS KTANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(K3TANK1) AS K3TANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(LTANK1)  AS LTANK1")
        'sqlZaikoTrainAmount.AppendLine("      ,SUM(ATANK1)  AS ATANK1")
        sqlZaikoTrainAmount.AppendLine("  FROM OIL.OIT0009_UKEIREOILSTOCK")
        sqlZaikoTrainAmount.AppendLine(" WHERE STOCKYMD   BETWEEN @FROMDATE AND @TODATE")
        sqlZaikoTrainAmount.AppendLine("   AND OFFICECODE    = @OFFICECODE")
        sqlZaikoTrainAmount.AppendLine("   AND SHIPPERSCODE  = @SHIPPERSCODE")
        sqlZaikoTrainAmount.AppendLine("   AND CONSIGNEECODE = @CONSIGNEECODE")
        sqlZaikoTrainAmount.AppendLine("   AND DELFLG        =  @DELFLG")
        sqlZaikoTrainAmount.AppendLine("   AND RTANK1 + HTANK1 + TTANK1 + MTTANK1 + KTANK1 + K3TANK1 + LTANK1 + ATANK1 > 0")
        If isHokushin Then
            sqlZaikoTrainAmount.AppendLine(" GROUP BY STOCKYMD,TRAINNO")
            sqlZaikoTrainAmount.AppendLine(" ORDER BY STOCKYMD,TRAINNO")
        Else
            sqlZaikoTrainAmount.AppendLine(" GROUP BY STOCKYMD")
            sqlZaikoTrainAmount.AppendLine(" ORDER BY STOCKYMD")
        End If


        '保存済みの在庫情報を取得
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            '固定パラメータの設定
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            '可変パラメータ
            Dim paramFromDate = sqlCmd.Parameters.Add("@FROMDATE", SqlDbType.Date)
            Dim paramToDate = sqlCmd.Parameters.Add("@TODATE", SqlDbType.Date)

            paramFromDate.Value = fromDateObj.KeyString
            paramToDate.Value = toDateObj.KeyString
            '**************************
            '指定年月の情報取得
            '**************************
            Using sqlDr = sqlCmd.ExecuteReader
                Dim curDate As String = ""
                Dim oilCode As String = ""
                Dim stockListCol As DispDataClass.StockListCollection = Nothing
                Dim dateValue As DispDataClass.StockListItem = Nothing
                While sqlDr.Read
                    curDate = Convert.ToString(sqlDr("TARGETDATE"))
                    oilCode = Convert.ToString(sqlDr("OILCODE"))
                    '対象油種を保持していない場合
                    If dispData.StockList.ContainsKey(oilCode) = False Then
                        Continue While '次のループへ
                    End If
                    stockListCol = dispData.StockList(oilCode)

                    'ループ対象の日付毎データが存在しない場合
                    If stockListCol.StockItemList.ContainsKey(curDate) = False Then
                        Continue While '次のループへ
                    End If
                    dateValue = stockListCol.StockItemList(curDate)

                    dateValue.Print1stPositionVal = Decimal.Parse(Convert.ToString(sqlDr("AMOUNT1")))
                    dateValue.Print2ndPositionVal = Decimal.Parse(Convert.ToString(sqlDr("AMOUNT2")))
                    dateValue.Print3rdPositionVal = Decimal.Parse(Convert.ToString(sqlDr("AMOUNT3")))
                    If hasKeyList.ContainsKey(oilCode & "@" & curDate) = False Then
                        hasKeyList.Add(oilCode & "@" & curDate, "")
                    End If

                End While 'sqlDr.Read

            End Using 'sqlDr 在庫テーブル側
            '**************************
            '在庫管理側のテーブルよりデータ取得
            '**************************
            sqlCmd.CommandText = sqlZaikoTrainAmount.ToString
            Using sqlDr = sqlCmd.ExecuteReader
                Dim curDate As String = ""
                Dim oilCode As String = ""
                Dim stockListCol As DispDataClass.StockListCollection = Nothing
                Dim dateValue As DispDataClass.StockListItem = Nothing
                While sqlDr.Read
                    curDate = Convert.ToString(sqlDr("TARGETDATE"))
                    '過去日の場合は次のレコード
                    If curDate < Now.ToString("yyyy/MM/dd") Then
                        Continue While
                    End If
                    For Each fieldItems In oilCodeToFieldNameList
                        oilCode = fieldItems.Key
                        If hasKeyList.ContainsKey(oilCode & "@" & curDate) Then
                            Continue For
                        End If
                        '対象油種を保持していない場合
                        If dispData.StockList.ContainsKey(oilCode) = False Then
                            Continue For '次のループへ
                        End If
                        stockListCol = dispData.StockList(oilCode)
                        dateValue = stockListCol.StockItemList(curDate)
                        Dim amount As Decimal = 0
                        Dim carsNum As Decimal = CDec(sqlDr(fieldItems.Value))
                        If stockListCol.OilInfo.Weight > 0 Then
                            amount = Math.Floor(carsNum * 45 / stockListCol.OilInfo.Weight)
                        End If

                        If isHokushin Then
                            Dim trainNum As String = Convert.ToString(sqlDr("TRAINNO"))
                            Select Case trainNum
                                Case "5463"
                                    dateValue.Print1stPositionVal = amount
                                Case "2085"
                                    dateValue.Print2ndPositionVal = amount
                                Case "8471"
                                    dateValue.Print3rdPositionVal = amount
                            End Select
                        Else
                            dateValue.Print1stPositionVal = 0
                            dateValue.Print2ndPositionVal = amount
                            dateValue.Print3rdPositionVal = 0
                        End If

                    Next fieldItems

                End While
            End Using ' sqlDr 在庫管理側
        End Using 'sqlCmd

        Return retVal
    End Function
    ''' <summary>
    ''' 列車運行情報マスタより情報取得
    ''' </summary>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="dispData">画面表示クラス</param>
    ''' <returns></returns>
    Private Function GetTrainOperation(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        If dispData.ShowSuggestList = False Then
            Return dispData
        End If
        Dim trOpeList As New List(Of TrainOperationItem)
        Dim retVal = dispData
        Dim fromDateObj = dispData.StockDate.Values.FirstOrDefault
        Dim toDateObj = dispData.StockDate.Values.LastOrDefault

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT TRO.OFFICECODE                       AS OFFICECODE")
        sqlStat.AppendLine("      ,TRO.TRAINNO                          AS TRAINNO")
        sqlStat.AppendLine("      ,format(TRO.WORKINGDATE,'yyyy/MM/dd') AS WORKINGDATE")
        sqlStat.AppendLine("      ,TRO.TSUMI                            AS TSUMI")
        sqlStat.AppendLine("      ,TRO.DEPSTATION                       AS DEPSTATION")
        sqlStat.AppendLine("      ,TRO.ARRSTATION                       AS ARRSTATION")
        sqlStat.AppendLine("      ,isnull(TRO.RUN,'0')                  AS RUN")
        sqlStat.AppendLine("  FROM OIL.OIM0017_TRAINOPERATION TRO")
        sqlStat.AppendLine(" WHERE TRO.WORKINGDATE BETWEEN @FROMDATE AND @TODATE")
        sqlStat.AppendLine("   AND TRO.DELFLG      = @DELFLG")
        '列車条件をORで積み上げ ここから
        sqlStat.AppendLine("   AND (")
        Dim trainCondTemplate As String = ""
        trainCondTemplate = trainCondTemplate & " (     TRO.TRAINNO    = '{0}' " & ControlChars.CrLf
        trainCondTemplate = trainCondTemplate & "   AND TRO.TSUMI      = '{1}' " & ControlChars.CrLf
        trainCondTemplate = trainCondTemplate & "   AND TRO.DEPSTATION = '{2}' " & ControlChars.CrLf
        trainCondTemplate = trainCondTemplate & "   AND TRO.ARRSTATION = '{3}' " & ControlChars.CrLf
        trainCondTemplate = trainCondTemplate & " ) " & ControlChars.CrLf
        Dim isFirstTime As Boolean = True
        For Each trainItm In dispData.TrainList.Values
            sqlStat.AppendFormat(trainCondTemplate, trainItm.TrainNo,
                                 trainItm.Tsumi, trainItm.DepStation, trainItm.ArrStation).AppendLine()
            If isFirstTime Then
                isFirstTime = False
                trainCondTemplate = " OR " & trainCondTemplate
            End If
        Next trainItm
        sqlStat.AppendLine("       )")
        '列車条件をORで積み上げ ここまで
        sqlStat.AppendLine(" ORDER BY TRO.TRAINNO,TRO.WORKINGDATE")
        '抽出結果なし且つ範囲が未来日部分に関して１年前の過去実績の払出を設定
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            '固定パラメータの設定
            With sqlCmd.Parameters
                .Add("@FROMDATE", SqlDbType.Date).Value = fromDateObj.ItemDate
                .Add("@TODATE", SqlDbType.Date).Value = toDateObj.ItemDate
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE

            End With

            '指定年月の情報取得
            Using sqlDr = sqlCmd.ExecuteReader
                Dim trainOpeItem As TrainOperationItem
                While sqlDr.Read
                    trainOpeItem = New TrainOperationItem
                    trainOpeItem.OfficeCode = Convert.ToString(sqlDr("OFFICECODE"))
                    trainOpeItem.TrainNo = Convert.ToString(sqlDr("TRAINNO"))
                    trainOpeItem.WorkingDate = Convert.ToString(sqlDr("WORKINGDATE"))
                    trainOpeItem.Tsumi = Convert.ToString(sqlDr("TSUMI"))
                    trainOpeItem.DepStation = Convert.ToString(sqlDr("DEPSTATION"))
                    trainOpeItem.ArrStation = Convert.ToString(sqlDr("ARRSTATION"))
                    trainOpeItem.Run = Convert.ToString(sqlDr("RUN"))
                    trOpeList.Add(trainOpeItem)
                End While 'sqlDr.Read
            End Using 'sqlDr
        End Using 'sqlCmd
        retVal.TrainOperationList = trOpeList '2020/02/27 現状保持しとく必要はないが念のため
        Dim targetDate As String = ""
        Dim targetTrainNo As String = ""
        Dim run As String = ""
        For Each sgItm In retVal.SuggestList.Values
            targetDate = sgItm.DayInfo.KeyString

            For Each odrItm In sgItm.SuggestOrderItem.Values
                targetTrainNo = odrItm.TrainInfo.TrainNo
                run = "1"
                run = (From opeItm In trOpeList
                       Where opeItm.TrainNo = targetTrainNo AndAlso
                             opeItm.WorkingDate = targetDate
                       Select Convert.ToString(opeItm.Run)).DefaultIfEmpty("1").First
                '川崎且つ曜日が土日はロック
                If odrItm.TrainInfo.TrainNo.Equals("川崎") AndAlso
                  {"0", "6"}.Contains(sgItm.DayInfo.WeekNum) Then
                    run = "0"
                End If

                If run = "0" Then
                    odrItm.TrainLock = True
                Else
                    odrItm.TrainLock = False
                End If
            Next odrItm

        Next sgItm
        Return retVal
    End Function
    ''' <summary>
    ''' 管理対象外列車が必要な油槽所か判定し管理外の列車情報を追加
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="trainList"></param>
    ''' <param name="salesOffice"></param>
    ''' <param name="shipper"></param>
    ''' <param name="consignee"></param>
    ''' <returns></returns>
    Private Function GetUnmanagedTrain(sqlCon As SqlConnection, trainList As Dictionary(Of String, TrainListItem),
                                       salesOffice As String, shipper As String, consignee As String) As Dictionary(Of String, TrainListItem)
        Dim retVal As Dictionary(Of String, TrainListItem) = trainList
        Dim sqlStr As New StringBuilder
        '1レコード想定
        sqlStr.AppendLine("SELECT rtrim(FX.VALUE3)  AS TRAINNO")
        sqlStr.AppendLine("      ,rtrim(FX.VALUE4)  AS TRAINNAME")
        sqlStr.AppendLine("      ,rtrim(FX.VALUE5)  AS MAXVOLUME")
        sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
        sqlStr.AppendLine(" WHERE FX.CAMPCODE = @CAMPCODE")
        sqlStr.AppendLine("   AND FX.CLASS    = @CLASS")
        sqlStr.AppendLine("   AND FX.KEYCODE  = @OFFICECODE")
        sqlStr.AppendLine("   AND FX.VALUE1   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@CAMPCODE", SqlDbType.NVarChar).Value = "01"
                .Add("@CLASS", SqlDbType.NVarChar).Value = "UNMANAGEDTRAIN"
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = salesOffice
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = consignee
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    sqlDr.Read()
                    Dim trNo As String = Convert.ToString(sqlDr("TRAINNO"))
                    Dim trName As String = Convert.ToString(sqlDr("TRAINNAME"))
                    Dim trMaxVol As Decimal = CDec(Convert.ToString(sqlDr("MAXVOLUME")))
                    Dim trItem As New TrainListItem(trNo, trName, trMaxVol)
                    trItem.AccDays = 1 '一日後着
                    trItem.UnmanagedTrain = True '管理外フラグをOnに変更
                    retVal.Add(trItem.TrainNo, trItem)
                End If
            End Using 'sqlDr
        End Using 'sqlCmd
        Return retVal
    End Function
    ''' <summary>
    ''' 固定値マスタより構内取りデータ取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    ''' <remarks>詳細データは別関数で取得
    ''' 当関数では構内取り先有無、構内取り先営業所、構内取り先油槽所を取得
    ''' 複数構内取り先は現在未考慮（複数取れた場合はデータ並びで先頭）</remarks>
    Private Function GetMoveInsideData(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        Dim retVal As DispDataClass = dispData
        Dim sqlStr As New StringBuilder
        '1レコード想定
        sqlStr.AppendLine("SELECT rtrim(FX.VALUE2)  AS MT_SALESOFFICE")
        sqlStr.AppendLine("      ,rtrim(FX.VALUE3)  AS MT_CONSIGNEECODE")
        sqlStr.AppendLine("      ,rtrim(FX.VALUE4)  AS MT_SHIPPERSCODE")
        sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
        sqlStr.AppendLine(" WHERE FX.CAMPCODE = @CAMPCODE")
        sqlStr.AppendLine("   AND FX.CLASS    = @CLASS")
        sqlStr.AppendLine("   AND FX.KEYCODE  = @OFFICECODE")
        sqlStr.AppendLine("   AND FX.VALUE1   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@CAMPCODE", SqlDbType.NVarChar).Value = "01"
                .Add("@CLASS", SqlDbType.NVarChar).Value = "MOVEINSIDE"
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    sqlDr.Read()
                    retVal.HasMoveInsideItem = True
                    retVal.MiSalesOffice = Convert.ToString(sqlDr("MT_SALESOFFICE"))
                    retVal.MiConsignee = Convert.ToString(sqlDr("MT_CONSIGNEECODE"))
                    retVal.MiShippersCode = Convert.ToString(sqlDr("MT_SHIPPERSCODE"))
                    Dim prmData As Hashtable = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, retVal.MiSalesOffice)
                    Dim rtn As String = ""
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, retVal.MiSalesOffice, retVal.MiSalesOfficeName, rtn, prmData)

                    prmData = work.CreateFIXParam(retVal.MiSalesOffice, "JOINTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, retVal.MiShippersCode, retVal.MiShippersName, rtn, prmData)

                    Dim additionalCond As String = " and VALUE2 != '9' "
                    prmData = work.CreateFIXParam(retVal.MiSalesOffice, "CONSIGNEEPATTERN", I_ADDITIONALCONDITION:=additionalCond)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST, retVal.MiConsignee, retVal.MiConsigneeName, rtn, prmData)

                Else
                    retVal.HasMoveInsideItem = False
                    retVal.MiSalesOffice = ""
                    retVal.MiSalesOfficeName = ""
                    retVal.MiConsignee = ""
                    retVal.MiConsigneeName = ""
                    retVal.MiDispData = Nothing
                End If
            End Using 'sqlDr
        End Using 'sqlCmd
        Return retVal
    End Function
    ''' <summary>
    ''' 車数減少時に受注テーブルより削除するか情報を取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    ''' <remarks>詳細データは別関数で取得
    ''' 当関数では構内取り先有無、構内取り先営業所、構内取り先油槽所を取得
    ''' 複数構内取り先は現在未考慮（複数取れた場合はデータ並びで先頭）</remarks>
    Private Function IsAsyncDeleteShipper(sqlCon As SqlConnection, dispData As DispDataClass) As Boolean
        Dim sqlStr As New StringBuilder
        '1レコード想定
        sqlStr.AppendLine("SELECT 0 AS FIELD01")
        sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
        sqlStr.AppendLine(" WHERE FX.CAMPCODE = @CAMPCODE")
        sqlStr.AppendLine("   AND FX.CLASS    = @CLASS")
        sqlStr.AppendLine("   AND FX.KEYCODE  = @SHIPPERSCODE")
        sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@CAMPCODE", SqlDbType.NVarChar).Value = "01"
                .Add("@CLASS", SqlDbType.NVarChar).Value = "ASYNCDELETESHIPPER"
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                Return sqlDr.HasRows
            End Using 'sqlDr
        End Using 'sqlCmd
    End Function
    ''' <summary>
    ''' 空回日報情報を取得、提案表の車数を更新
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    Private Function EditEmptyTurnCarsNum(sqlCon As SqlConnection, dispData As DispDataClass, Optional isGetAlldays As Boolean = False) As DispDataClass
        'これもしかすると在庫テーブルから先週の払出量かと
        'オーダーは受入数な気がする(Consigneeしかないので)
        Dim sqlStr As New StringBuilder
        Dim retVal = dispData
        '検索値の設定
        Dim dateFrom As String = dispData.StockDate.First.Value.KeyString
        Dim dateTo As String = dispData.StockDate.Last.Value.KeyString

        sqlStr.AppendLine("SELECT DTL.OILCODE")
        sqlStr.AppendLine("     , ODR.TRAINNO")
        sqlStr.AppendLine("     , format(ODR.LODDATE,'yyyy/MM/dd') AS TARGETDATE")
        sqlStr.AppendLine("     , format(ODR.ACCDATE,'yyyy/MM/dd') AS ACCDATE")
        sqlStr.AppendLine("     , SUM(isnull(DTL.CARSNUMBER,0))    AS CARSNUMBER")
        sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlStr.AppendLine("   AND DTL.OILCODE is not null")
        sqlStr.AppendLine(" WHERE ODR.LODDATE   BETWEEN @DATE_FROM AND @ADATE_TO")
        sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        'sqlStr.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStr.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStr.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '荷主取得条件(JOINTコード考慮)↑
        '第二荷受人取得条件↓
        'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND (( ODR.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.SECONDCONSIGNEECODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.SECONDCONSIGNEECODE = ODR.CONSIGNEECODE ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     ODR.CONSIGNEECODE   <> @CONSIGNEECODE")
        sqlStr.AppendLine("                AND DTL.SECONDCONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '第二荷受人取得条件↑

        sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStr.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        sqlStr.AppendLine(" GROUP BY DTL.OILCODE,ODR.TRAINNO,ODR.LODDATE,ODR.ACCDATE")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@ADATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String = ""
                    Dim trainNo As String = ""
                    Dim targetDate As String = ""
                    Dim accDate As String = ""
                    Dim accDays As Integer = 0
                    Dim suggestVal As Decimal = 0D
                    While sqlDr.Read
                        oilCode = Convert.ToString(sqlDr("OILCODE"))
                        trainNo = Convert.ToString(sqlDr("TRAINNO"))
                        targetDate = Convert.ToString(sqlDr("TARGETDATE"))
                        accDate = Convert.ToString(sqlDr("ACCDATE"))
                        Dim tmSpan As TimeSpan = CDate(accDate) - CDate(targetDate)
                        accDays = tmSpan.Days
                        '油種未設定または対象油種を持っていないレコードはスキップ
                        If oilCode = "" OrElse retVal.SuggestOilNameList.ContainsKey(oilCode) = False OrElse
                           targetDate = "" OrElse retVal.SuggestList.ContainsKey(targetDate) = False OrElse
                           trainNo = "" OrElse retVal.SuggestList(targetDate).SuggestOrderItem.ContainsKey(trainNo) = False Then
                            Continue While
                        End If
                        '過去日の場合も設定しない
                        If retVal.SuggestList(targetDate).DayInfo.IsBeforeToday AndAlso isGetAlldays = False Then
                            Continue While
                        End If

                        '画面表示外のデータも設定しない
                        If retVal.SuggestList(targetDate).DayInfo.IsDispArea = False Then
                            Continue While
                        End If

                        With retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).SuggestValuesItem(oilCode)
                            suggestVal = Convert.ToDecimal(sqlDr("CARSNUMBER"))
                            .ItemValue = suggestVal.ToString("#,##0")
                            If retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).TrainInfo.AccDays = accDays Then
                                retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).AccAddDays = ""
                            Else
                                retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).AccAddDays = accDays.ToString
                            End If
                        End With

                    End While
                End If
            End Using 'sqlDr
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' OT用の空回日報情報を取得、提案表の車数を更新
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    Private Function EditOtEmptyTurnCarsNum(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        'これもしかすると在庫テーブルから先週の払出量かと
        'オーダーは受入数な気がする(Consigneeしかないので)
        Dim sqlStr As New StringBuilder
        Dim retVal = dispData
        '検索値の設定
        Dim dateFrom As String = dispData.StockDate.First.Value.KeyString
        Dim dateTo As String = dispData.StockDate.Last.Value.KeyString

        sqlStr.AppendLine("SELECT DTL.OILCODE")
        sqlStr.AppendLine("     , format(ODR.ACCDATE,'yyyy/MM/dd') AS TARGETDATE")
        sqlStr.AppendLine("     , SUM(isnull(DTL.CARSNUMBER,0))    AS CARSNUMBER")
        sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlStr.AppendLine("   AND DTL.OILCODE is not null")
        sqlStr.AppendLine(" WHERE ODR.ACCDATE   BETWEEN @DATE_FROM AND @ADATE_TO")
        sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        'sqlStr.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStr.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStr.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '荷主取得条件(JOINTコード考慮)↑
        '第二荷受人取得条件↓
        'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND (( ODR.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("          AND (    ISNULL(DTL.SECONDCONSIGNEECODE,'') = ''   ")
        sqlStr.AppendLine("                OR DTL.SECONDCONSIGNEECODE = ODR.CONSIGNEECODE ")
        sqlStr.AppendLine("              ) ")
        sqlStr.AppendLine("        ) OR  (     ODR.CONSIGNEECODE   <> @CONSIGNEECODE")
        sqlStr.AppendLine("                AND DTL.SECONDCONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("              )")
        sqlStr.AppendLine("       )")
        '第二荷受人取得条件↑

        sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStr.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        sqlStr.AppendLine(" GROUP BY DTL.OILCODE,ODR.ACCDATE")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@ADATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String = ""
                    Dim trainNo As String = ""
                    Dim targetDate As String = ""
                    Dim stockReceiveVal As Decimal = 0D
                    While sqlDr.Read
                        oilCode = Convert.ToString(sqlDr("OILCODE"))
                        targetDate = Convert.ToString(sqlDr("TARGETDATE"))
                        '油種未設定または対象油種を持っていないレコードはスキップ
                        If oilCode = "" OrElse retVal.StockList.ContainsKey(oilCode) = False OrElse
                           targetDate = "" OrElse retVal.StockList(oilCode).StockItemList.ContainsKey(targetDate) = False Then
                            Continue While
                        End If
                        '過去日の場合も設定しない
                        If retVal.StockList(oilCode).StockItemList(targetDate).DaysItem.IsPastDay Then
                            Continue While
                        End If
                        With retVal.StockList(oilCode).StockItemList(targetDate)
                            Dim weight = retVal.StockList(oilCode).OilInfo.Weight
                            stockReceiveVal = 0D
                            If weight <> 0 Then
                                stockReceiveVal = Math.Floor(Convert.ToDecimal(sqlDr("CARSNUMBER")) * UKEIRE_BASE_NUM / weight)
                            End If
                            .Receive = stockReceiveVal.ToString
                        End With

                    End While
                End If
            End Using 'sqlDr
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 受入車数の取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    Private Function GetUkeireOilstock(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        Dim retVal = dispData
        If dispData.ShowSuggestList = False Then
            '提案表の表示が無い場合は意味がないのでスキップ
            Return dispData
        End If
        Dim sqlStr As New StringBuilder
        '油種コードをフィールド名に割り当てる変数
        Dim oilCodeToFieldNameList As New Dictionary(Of String, String) From {
            {"1101", "RTANK"}, {"1001", "HTANK"}, {"1301", "TTANK"}, {"1302", "MTTANK"},
            {"1401", "KTANK"}, {"1404", "K3TANK"}, {"2201", "LTANK"}, {"2101", "ATANK"}}
        '検索値の設定
        Dim dateFrom As String = dispData.StockDate.First.Value.KeyString
        Dim dateTo As String = dispData.StockDate.Last.Value.KeyString
        sqlStr.AppendLine("SELECT UOS.TRAINNO")
        sqlStr.AppendLine("     , format(UOS.STOCKYMD,'yyyy/MM/dd') AS STOCKYMD")
        sqlStr.AppendLine("     , format(UOS.ACCYMD,'yyyy/MM/dd')   AS ACCYMD")
        sqlStr.AppendLine("     , isnull(UOS.RTANK1,0)    AS RTANK1")
        sqlStr.AppendLine("     , isnull(UOS.HTANK1,0)    AS HTANK1")
        sqlStr.AppendLine("     , isnull(UOS.TTANK1,0)    AS TTANK1")
        sqlStr.AppendLine("     , isnull(UOS.MTTANK1,0)   AS MTTANK1")
        sqlStr.AppendLine("     , isnull(UOS.KTANK1,0)    AS KTANK1")
        sqlStr.AppendLine("     , isnull(UOS.K3TANK1,0)   AS K3TANK1")
        sqlStr.AppendLine("     , isnull(UOS.LTANK1,0)    AS LTANK1")
        sqlStr.AppendLine("     , isnull(UOS.ATANK1,0)    AS ATANK1")

        sqlStr.AppendLine("     , isnull(UOS.RTANK2,0)    AS RTANK2")
        sqlStr.AppendLine("     , isnull(UOS.HTANK2,0)    AS HTANK2")
        sqlStr.AppendLine("     , isnull(UOS.TTANK2,0)    AS TTANK2")
        sqlStr.AppendLine("     , isnull(UOS.MTTANK2,0)   AS MTTANK2")
        sqlStr.AppendLine("     , isnull(UOS.KTANK2,0)    AS KTANK2")
        sqlStr.AppendLine("     , isnull(UOS.K3TANK2,0)   AS K3TANK2")
        sqlStr.AppendLine("     , isnull(UOS.LTANK2,0)    AS LTANK2")
        sqlStr.AppendLine("     , isnull(UOS.ATANK2,0)    AS ATANK2")

        sqlStr.AppendLine("  FROM OIL.OIT0009_UKEIREOILSTOCK UOS")
        sqlStr.AppendLine(" WHERE UOS.STOCKYMD   BETWEEN @DATE_FROM AND @DATE_TO")
        sqlStr.AppendLine("   AND UOS.OFFICECODE      = @OFFICECODE")
        sqlStr.AppendLine("   AND UOS.SHIPPERSCODE    = @SHIPPERSCODE")
        sqlStr.AppendLine("   AND UOS.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND UOS.DELFLG          = @DELFLG")

        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@DATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String = ""
                    Dim trainNo As String = ""
                    Dim targetDate As String = ""
                    Dim accDate As String = ""
                    Dim trainNum As Decimal = 0D
                    Dim miTrainNum As Decimal = 0D
                    Dim hasKawasakiAnyValue As Boolean = False
                    While sqlDr.Read
                        targetDate = Convert.ToString(sqlDr("STOCKYMD"))
                        accDate = Convert.ToString(sqlDr("ACCYMD"))
                        trainNo = Convert.ToString(sqlDr("TRAINNO"))
                        hasKawasakiAnyValue = False
                        '対象日付を保持していない場合はスキップ
                        If retVal.SuggestList.ContainsKey(targetDate) = False Then
                            Continue While
                        End If
                        '対象の列車を保持していない場合はスキップ
                        If retVal.HasMoveInsideItem AndAlso retVal.SuggestList(targetDate).SuggestOrderItem.ContainsKey(trainNo) = False Then
                            Continue While
                        End If
                        Dim suggestDayTrainItm = retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).SuggestValuesItem
                        Dim miSuggestDayTrainItm As Dictionary(Of String, DispDataClass.SuggestItem.SuggestValue)
                        If retVal.HasMoveInsideItem Then
                            miSuggestDayTrainItm = retVal.MiDispData.SuggestList(targetDate).SuggestOrderItem(trainNo).SuggestValuesItem
                        Else
                            miSuggestDayTrainItm = Nothing
                        End If
                        '積込日：受入日の差日が列車情報と不一致の場合、受入日日数の個別設定と認識し格納
                        Dim lodDtm As Date = CDate(targetDate)
                        Dim accDtm As Date = CDate(accDate)
                        Dim accDays As TimeSpan = accDtm - lodDtm
                        If Not accDays.Days = retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).TrainInfo.AccDays Then
                            retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).AccAddDays = accDays.Days.ToString()
                        Else
                            retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).AccAddDays = ""
                        End If

                        For Each oilCodeToFieldName In oilCodeToFieldNameList
                            oilCode = oilCodeToFieldName.Key

                            trainNum = Convert.ToDecimal(sqlDr(oilCodeToFieldName.Value & "1"))
                            miTrainNum = Convert.ToDecimal(sqlDr(oilCodeToFieldName.Value & "2"))
                            If trainNo.Equals("川崎") AndAlso {DayOfWeek.Saturday, DayOfWeek.Sunday}.Contains(CDate(targetDate).DayOfWeek) _
                                AndAlso (trainNum > 0 OrElse miTrainNum > 0) Then
                                hasKawasakiAnyValue = True
                            End If
                            '対象の油種にテーブル内容を転記
                            If suggestDayTrainItm.ContainsKey(oilCode) Then
                                suggestDayTrainItm(oilCode).ItemValue = trainNum.ToString
                            End If
                            If retVal.HasMoveInsideItem AndAlso miSuggestDayTrainItm.ContainsKey(oilCode) Then
                                miSuggestDayTrainItm(oilCode).ItemValue = miTrainNum.ToString
                            End If

                        Next oilCodeToFieldName

                        If hasKawasakiAnyValue Then
                            retVal.SuggestList(targetDate).SuggestOrderItem(trainNo).TrainLock = False
                        End If
                    End While
                End If
            End Using 'sqlDr
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 印刷用の受入車数の取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    Private Function GetPrintUkeireTrainNum(sqlCon As SqlConnection, dispData As DispDataClass, Optional isMiData As Boolean = False, Optional parentConsignee As String = "") As DispDataClass
        Dim retVal = dispData
        retVal.PrintTrainNums = New Dictionary(Of String, PrintTrainNumCollection)
        Dim sqlStr As New StringBuilder
        '油種コードをフィールド名に割り当てる変数
        Dim oilCodeToFieldNameList As New Dictionary(Of String, String) From {
            {"1101", "RTANK"}, {"1001", "HTANK"}, {"1301", "TTANK"}, {"1302", "MTTANK"},
            {"1401", "KTANK"}, {"1404", "K3TANK"}, {"2201", "LTANK"}, {"2101", "ATANK"}}
        '検索値の設定
        Dim dateFrom As String = dispData.StockDate.First.Value.KeyString
        Dim dateTo As String = dispData.StockDate.Last.Value.KeyString
        'sqlStr.AppendLine("SELECT DTL.OILCODE")
        'sqlStr.AppendLine("     , ODR.OFFICECODE")
        'sqlStr.AppendLine("     , MAX(ODR.OFFICENAME) AS OFFICENAME")
        'sqlStr.AppendLine("     , format(ODR.LODDATE,'yyyy/MM/dd') AS TARGETDATE")
        'sqlStr.AppendLine("     , SUM(isnull(DTL.CARSNUMBER,0))    AS CARSNUMBER")
        'sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        'sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        'sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        'sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        'sqlStr.AppendLine("   AND DTL.OILCODE is not null")
        'sqlStr.AppendLine(" WHERE ODR.LODDATE   BETWEEN @DATE_FROM AND @DATE_TO")
        ''sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        ''sqlStr.AppendLine("   AND ODR.SHIPPERSCODE    = @SHIPPERSCODE")
        'sqlStr.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        'sqlStr.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        'sqlStr.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        'sqlStr.AppendLine("              ) ")
        'sqlStr.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        'sqlStr.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        'sqlStr.AppendLine("              )")
        'sqlStr.AppendLine("       )")

        'sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        'sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        'sqlStr.AppendLine("   AND ODR.ORDERSTATUS    <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        'sqlStr.AppendLine(" GROUP BY DTL.OILCODE,ODR.OFFICECODE,ODR.LODDATE")
        'sqlStr.AppendLine(" ORDER BY OILCODE,OFFICECODE,LODDATE")

        sqlStr.AppendLine("SELECT format(UOS.STOCKYMD,'yyyy/MM/dd') AS TARGETDATE")
        sqlStr.AppendLine("      ,UOS.OFFICECODE")
        Dim fieldSuffix As String = "1"
        If isMiData Then
            fieldSuffix = "2"
        End If
        For Each fieldName In oilCodeToFieldNameList.Values
            sqlStr.AppendFormat("      ,ISNULL(SUM(UOS.{0}{1}),0)            AS {0}", fieldName, fieldSuffix)
        Next
        sqlStr.AppendLine("  FROM      OIL.OIT0009_UKEIREOILSTOCK  UOS")
        sqlStr.AppendLine(" WHERE UOS.STOCKYMD BETWEEN @DATE_FROM AND @DATE_TO")
        'sqlStr.AppendLine("   AND UOS.OFFICECODE    = @OFFICECODE")
        sqlStr.AppendLine("   AND UOS.SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStr.AppendLine("   AND UOS.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND UOS.DELFLG        = @DELFLG")
        sqlStr.AppendLine(" GROUP BY UOS.OFFICECODE,UOS.STOCKYMD")
        sqlStr.AppendLine(" ORDER BY UOS.OFFICECODE,UOS.STOCKYMD")
        Dim retDt As DataTable = New DataTable("TMPTABLE")
        With retDt.Columns
            .Add("OFFICECODE", GetType(String))
            .Add("TARGETDATE", GetType(String))
            .Add("CARSNUMBER", GetType(Decimal))
            For Each fieldName In oilCodeToFieldNameList.Values
                .Add(fieldName, GetType(Decimal))
            Next
        End With
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@DATE_TO", SqlDbType.Date).Value = dateTo
                '.Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                If isMiData = False Then
                    .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                Else
                    .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = parentConsignee
                End If

                '.Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = True Then
                    retDt.Load(sqlDr)
                End If
            End Using
        End Using
        '抽出ベース
        ''抽出したOFFICEコード
        'Dim qOfficeCodes = (From itm As DataRow In retDt Group By office = Convert.ToString(itm("OFFICECODE")) Into Group Select office).ToList
        'Dim officeCodes As New Dictionary(Of String, String)
        'For Each officeCode In qOfficeCodes
        '    Dim officeName = (From itm As DataRow In retDt Where Convert.ToString(itm("OFFICECODE")) = officeCode Select Convert.ToString(itm("OFFICENAME"))).FirstOrDefault
        '    officeName = officeName.Replace("営業所", "")
        '    officeCodes.Add(officeCode, officeName)
        'Next
        'If officeCodes Is Nothing OrElse officeCodes.Count = 0 Then
        '    '何もしない
        '    Return retVal
        'End If
        '固定ベース
        Dim officeCodes As New Dictionary(Of String, String)
        '増幅するパターンの場合
        '出光シェル+JONET松本、ENEOS+OT宇都宮、コスモ+OT郡山、コスモ+OT松本
        If dispData.Consignee = "40" AndAlso dispData.Shipper = "0122700010" Then
            officeCodes.Add("011203", "袖ヶ浦")
            officeCodes.Add("012402", "三重塩浜")
        ElseIf dispData.Consignee = "53" AndAlso dispData.Shipper = "0005700010" Then
            officeCodes.Add("011402", "根岸")
            officeCodes.Add("011202", "甲子")
        ElseIf dispData.Consignee = "52" AndAlso dispData.Shipper = "0094000010" Then
            officeCodes.Add("011201", "五井")
            officeCodes.Add("010402", "仙台新港")
        ElseIf dispData.Consignee = "56" AndAlso dispData.Shipper = "0094000010" Then
            officeCodes.Add("011201", "五井")
            officeCodes.Add("012401", "四日市")
        Else
            '上記以外は検索条件通り
            officeCodes.Add(dispData.SalesOffice, dispData.SalesOfficeName.Replace("営業所", ""))
        End If

        '車数0のガワを作成
        For Each oilItm In dispData.OilTypeList.Values
            Dim trainNumTopObj = New PrintTrainNumCollection
            trainNumTopObj.OilInfo = oilItm
            trainNumTopObj.PrintTrainNumList = New Dictionary(Of String, PrintTrainNum)
            For Each officeCode In officeCodes
                Dim printTrainNum = New PrintTrainNum
                printTrainNum.OfficeCode = officeCode.Key
                printTrainNum.OfficeName = officeCode.Value
                printTrainNum.PrintTrainItems = New Dictionary(Of String, PrintTrainItem)
                For Each daysItm In dispData.StockDate.Values
                    Dim printTrainItem = New PrintTrainItem
                    printTrainItem.TrainNum = 0
                    printTrainItem.DateString = daysItm.KeyString
                    printTrainNum.PrintTrainItems.Add(daysItm.KeyString, printTrainItem)
                Next
                trainNumTopObj.PrintTrainNumList.Add(officeCode.Key, printTrainNum)
            Next officeCode
            retVal.PrintTrainNums.Add(oilItm.OilCode, trainNumTopObj)
        Next oilItm
        '抽出結果を振り分け
        For Each dr As DataRow In retDt.Rows
            For Each oilCodeItm In oilCodeToFieldNameList
                Dim oilCode As String = oilCodeItm.Key
                Dim officeCode As String = Convert.ToString(dr("OFFICECODE"))
                Dim dateString As String = Convert.ToString(dr("TARGETDATE"))
                Dim carNum As Decimal = CDec(dr(oilCodeItm.Value))
                If retVal.PrintTrainNums.ContainsKey(oilCode) Then
                    With retVal.PrintTrainNums(oilCode)
                        If .PrintTrainNumList.ContainsKey(officeCode) Then
                            With .PrintTrainNumList(officeCode)
                                If .PrintTrainItems.ContainsKey(dateString) Then
                                    .PrintTrainItems(dateString).TrainNum = carNum
                                End If
                            End With
                        End If
                    End With
                End If
            Next

        Next dr

        Return retVal
    End Function
    ''' <summary>
    ''' 印刷用の受入車数(OT)の取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispData"></param>
    ''' <returns></returns>
    Private Function GetPrintOtUkeireTrainNum(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        Dim retVal = dispData
        retVal.PrintTrainNums = New Dictionary(Of String, PrintTrainNumCollection)
        '検索値の設定
        Dim dateFrom As String = dispData.StockDate.First.Value.KeyString
        Dim dateTo As String = dispData.StockDate.Last.Value.KeyString
        Dim sqlStr As New StringBuilder
        sqlStr.AppendLine("SELECT format(STC.STOCKYMD,'yyyy/MM/dd') AS TARGETDATE ")
        sqlStr.AppendLine("      ,STC.OFFICECODE")
        sqlStr.AppendLine("      ,STC.OILCODE")
        sqlStr.AppendLine("      ,ISNULL(SUM(STC.ARRVOL),0) AS ARRVOL")
        sqlStr.AppendLine("  FROM OIL.OIT0001_OILSTOCK STC")
        sqlStr.AppendLine(" WHERE STC.STOCKYMD BETWEEN @DATE_FROM AND @DATE_TO")
        sqlStr.AppendLine("   AND STC.SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStr.AppendLine("   AND STC.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND STC.DELFLG        = @DELFLG")
        sqlStr.AppendLine(" GROUP BY STC.OILCODE,STC.OFFICECODE,STC.STOCKYMD")
        sqlStr.AppendLine(" ORDER BY STC.OILCODE,STC.OFFICECODE,STC.STOCKYMD")

        Dim retDt As DataTable = New DataTable("TMPTABLE")
        With retDt.Columns
            .Add("OFFICECODE", GetType(String))
            .Add("TARGETDATE", GetType(String))
            .Add("OILCODE", GetType(String))
            .Add("ARRVOL", GetType(Decimal))
        End With
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@DATE_TO", SqlDbType.Date).Value = dateTo
                '.Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                '.Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows = True Then
                    retDt.Load(sqlDr)
                End If
            End Using
        End Using
        Dim officeCodes As New Dictionary(Of String, String)
        '増幅するパターンの場合
        '出光シェル+JONET松本、ENEOS+OT宇都宮、コスモ+OT郡山、コスモ+OT松本
        If dispData.Consignee = "40" AndAlso dispData.Shipper = "0122700010" Then
            officeCodes.Add("011203", "袖ヶ浦")
            officeCodes.Add("012402", "三重塩浜")
        ElseIf dispData.Consignee = "53" AndAlso dispData.Shipper = "0005700010" Then
            officeCodes.Add("011402", "根岸")
            officeCodes.Add("011202", "甲子")
        ElseIf dispData.Consignee = "52" AndAlso dispData.Shipper = "0094000010" Then
            officeCodes.Add("011201", "五井")
            officeCodes.Add("010402", "仙台新港")
        ElseIf dispData.Consignee = "56" AndAlso dispData.Shipper = "0094000010" Then
            officeCodes.Add("011201", "五井")
            officeCodes.Add("012401", "四日市")
        Else
            '上記以外は検索条件通り
            officeCodes.Add(dispData.SalesOffice, dispData.SalesOfficeName.Replace("営業所", ""))
        End If
        '車数0のガワを作成
        For Each oilItm In dispData.OilTypeList.Values
            Dim trainNumTopObj = New PrintTrainNumCollection
            trainNumTopObj.OilInfo = oilItm
            trainNumTopObj.PrintTrainNumList = New Dictionary(Of String, PrintTrainNum)
            For Each officeCode In officeCodes
                Dim printTrainNum = New PrintTrainNum
                printTrainNum.OfficeCode = officeCode.Key
                printTrainNum.OfficeName = officeCode.Value
                printTrainNum.PrintTrainItems = New Dictionary(Of String, PrintTrainItem)
                For Each daysItm In dispData.StockDate.Values
                    Dim printTrainItem = New PrintTrainItem
                    printTrainItem.TrainNum = 0
                    printTrainItem.DateString = daysItm.KeyString
                    printTrainNum.PrintTrainItems.Add(daysItm.KeyString, printTrainItem)
                Next
                trainNumTopObj.PrintTrainNumList.Add(officeCode.Key, printTrainNum)
            Next officeCode
            retVal.PrintTrainNums.Add(oilItm.OilCode, trainNumTopObj)
        Next oilItm
        '抽出結果を振り分け
        For Each dr As DataRow In retDt.Rows

            Dim oilCode As String = Convert.ToString(dr("OILCODE"))
            Dim officeCode As String = Convert.ToString(dr("OFFICECODE"))
            Dim dateString As String = Convert.ToString(dr("TARGETDATE"))
            Dim carVol As Decimal = CDec(dr("ARRVOL"))
            Dim carNum As Decimal = 0
            If retVal.PrintTrainNums.ContainsKey(oilCode) Then
                With retVal.PrintTrainNums(oilCode)
                    If .PrintTrainNumList.ContainsKey(officeCode) Then
                        With .PrintTrainNumList(officeCode)
                            If .PrintTrainItems.ContainsKey(dateString) Then
                                carNum = Math.Floor(carVol * retVal.OilTypeList(oilCode).Weight / 45)
                                .PrintTrainItems(dateString).TrainNum = carNum
                            End If
                        End With
                    End If
                End With
            End If

        Next dr

        Return retVal
    End Function
    ''' <summary>
    ''' 受注作成用のオーダー情報よりアップデート対象を抽出
    ''' </summary>
    ''' <param name="sqlCon">sql接続</param>
    ''' <param name="dispData">画面情報クラス</param>
    ''' <returns>アップデート対象のリストクラスを生成</returns>
    Private Function GetEmptyTurnOrder(sqlCon As SqlConnection, dispData As DispDataClass, Optional parentConsignee As String = "") As Dictionary(Of String, OrderItem)
        Dim sqlStat As New StringBuilder
        Dim retVal As New Dictionary(Of String, OrderItem)
        '画面で選択された（選択されていないものは除去）日付、列車、油種情報を取得
        Dim targetTrainInfo = dispData.GetSuggestCheckedItem
        '前処理の入力チェックがあり、ここにきてありえないが選択されたデータが無い場合そのまま終了
        If targetTrainInfo Is Nothing OrElse targetTrainInfo.Count = 0 Then
            Return retVal
        End If
        sqlStat.AppendLine("SELECT ISNULL(RTRIM(ODR.ORDERNO),'')              AS ORDERNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TRAINNO),'')              AS TRAINNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TRAINNAME),'')            AS TRAINNAME")
        sqlStat.AppendLine("      ,format(ODR.ORDERYMD,'yyyy/MM/dd')          AS ORDERYMD")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OFFICECODE),'')           AS OFFICECODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OFFICENAME),'')           AS OFFICENAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ORDERTYPE),'')            AS ORDERTYPE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.SHIPPERSCODE),'')         AS SHIPPERSCODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.SHIPPERSNAME),'')         AS SHIPPERSNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.BASECODE),'')             AS BASECODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.BASENAME),'')             AS BASENAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.CONSIGNEECODE),'')        AS CONSIGNEECODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.CONSIGNEENAME),'')        AS CONSIGNEENAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.DEPSTATION),'')           AS DEPSTATION")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.DEPSTATIONNAME),'')       AS DEPSTATIONNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ARRSTATION),'')           AS ARRSTATION")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ARRSTATIONNAME),'')       AS ARRSTATIONNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RETSTATION),'')           AS RETSTATION")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RETSTATIONNAME),'')       AS RETSTATIONNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.CHANGERETSTATION),'')     AS CHANGERETSTATION")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.CHANGERETSTATIONNAME),'') AS CHANGERETSTATIONNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ORDERSTATUS),'')      AS ORDERSTATUS")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ORDERINFO),'')        AS ORDERINFO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.STACKINGFLG),'')      AS STACKINGFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.EMPTYTURNFLG),'')     AS EMPTYTURNFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.USEPROPRIETYFLG),'')  AS USEPROPRIETYFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.CONTACTFLG),'')       AS CONTACTFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RESULTFLG),'')        AS RESULTFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.DELIVERYFLG),'')      AS DELIVERYFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.DELIVERYCOUNT),'')    AS DELIVERYCOUNT")
        sqlStat.AppendLine("      ,format(ODR.LODDATE,'yyyy/MM/dd')          AS LODDATE")
        sqlStat.AppendLine("      ,format(ODR.DEPDATE,'yyyy/MM/dd')          AS DEPDATE")
        sqlStat.AppendLine("      ,format(ODR.ARRDATE,'yyyy/MM/dd')          AS ARRDATE")
        sqlStat.AppendLine("      ,format(ODR.ACCDATE,'yyyy/MM/dd')          AS ACCDATE")
        sqlStat.AppendLine("      ,format(ODR.EMPARRDATE,'yyyy/MM/dd')       AS EMPARRDATE")
        sqlStat.AppendLine("      ,format(ODR.ACTUALLODDATE,'yyyy/MM/dd')    AS ACTUALLODDATE")
        sqlStat.AppendLine("      ,format(ODR.ACTUALDEPDATE,'yyyy/MM/dd')    AS ACTUALDEPDATE")
        sqlStat.AppendLine("      ,format(ODR.ACTUALARRDATE,'yyyy/MM/dd')    AS ACTUALARRDATE")
        sqlStat.AppendLine("      ,format(ODR.ACTUALACCDATE,'yyyy/MM/dd')    AS ACTUALACCDATE")
        sqlStat.AppendLine("      ,format(ODR.ACTUALEMPARRDATE,'yyyy/MM/dd') AS ACTUALEMPARRDATE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RTANK),'')            AS RTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.HTANK),'')            AS HTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TTANK),'')            AS TTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.MTTANK),'')           AS MTTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.KTANK),'')            AS KTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.K3TANK),'')           AS K3TANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.K5TANK),'')           AS K5TANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.K10TANK),'')          AS K10TANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.LTANK),'')            AS LTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ATANK),'')            AS ATANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER1OTANK),'')      AS OTHER1OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER2OTANK),'')      AS OTHER2OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER3OTANK),'')      AS OTHER3OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER4OTANK),'')      AS OTHER4OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER5OTANK),'')      AS OTHER5OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER6OTANK),'')      AS OTHER6OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER7OTANK),'')      AS OTHER7OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER8OTANK),'')      AS OTHER8OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER9OTANK),'')      AS OTHER9OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER10OTANK),'')     AS OTHER10OTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TOTALTANK),'')        AS TOTALTANK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RTANKCH),'')          AS RTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.HTANKCH),'')          AS HTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TTANKCH),'')          AS TTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.MTTANKCH),'')         AS MTTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.KTANKCH),'')          AS KTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.K3TANKCH),'')         AS K3TANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.K5TANKCH),'')         AS K5TANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.K10TANKCH),'')        AS K10TANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.LTANKCH),'')          AS LTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ATANKCH),'')          AS ATANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER1OTANKCH),'')    AS OTHER1OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER2OTANKCH),'')    AS OTHER2OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER3OTANKCH),'')    AS OTHER3OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER4OTANKCH),'')    AS OTHER4OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER5OTANKCH),'')    AS OTHER5OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER6OTANKCH),'')    AS OTHER6OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER7OTANKCH),'')    AS OTHER7OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER8OTANKCH),'')    AS OTHER8OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER9OTANKCH),'')    AS OTHER9OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTHER10OTANKCH),'')   AS OTHER10OTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TOTALTANKCH),'')      AS TOTALTANKCH")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TANKLINKNO),'')       AS TANKLINKNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TANKLINKNOMADE),'')   AS TANKLINKNOMADE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.BILLINGNO),'')        AS BILLINGNO")
        sqlStat.AppendLine("      ,format(ODR.KEIJYOYMD,'yyyy/MM/dd')     AS KEIJYOYMD")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.SALSE),'')            AS SALSE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.SALSETAX),'')         AS SALSETAX")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TOTALSALSE),'')       AS TOTALSALSE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.PAYMENT),'')          AS PAYMENT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.PAYMENTTAX),'')       AS PAYMENTTAX")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TOTALPAYMENT),'')     AS TOTALPAYMENT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTFILENAME),'')       AS OTFILENAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RECEIVECOUNT),'')     AS RECEIVECOUNT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.OTSENDSTATUS),'')     AS OTSENDSTATUS")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.RESERVEDSTATUS),'')   AS RESERVEDSTATUS")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.TAKUSOUSTATUS),'')    AS TAKUSOUSTATUS")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.BTRAINNO),'')         AS BTRAINNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.BTRAINNAME),'')       AS BTRAINNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.ANASYORIFLG),'')      AS ANASYORIFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.DELFLG),'')           AS DELFLG")
        sqlStat.AppendLine("      ,format(ODR.INITYMD,'yyyy/MM/dd HH:mm:ss.fff')    AS INITYMD")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.INITUSER),'')         AS INITUSER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.INITTERMID),'')       AS INITTERMID")
        sqlStat.AppendLine("      ,format(ODR.UPDYMD,'yyyy/MM/dd HH:mm:ss.fff')     AS UPDYMD")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.UPDUSER),'')          AS UPDUSER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(ODR.UPDTERMID),'')        AS UPDTERMID")
        sqlStat.AppendLine("      ,format(ODR.RECEIVEYMD,'yyyy/MM/dd HH:mm:ss.fff') AS RECEIVEYMD")
        sqlStat.AppendLine("      ,case when ODR.SHIPPERSCODE  = @SHIPPERSCODE  then '0' else '1' end AS JOINTORDER ")
        sqlStat.AppendLine("      ,case when ODR.CONSIGNEECODE = @CONSIGNEECODE then '0' else '1' end AS SECONDCONSIGNEEORDER ")
        sqlStat.AppendLine("  FROM OIL.OIT0002_ORDER ODR with(nolock)")
        sqlStat.AppendLine(" WHERE ODR.OFFICECODE    = @OFFICECODE")
        'sqlStat.AppendLine("   AND ODR.SHIPPERSCODE  = @SHIPPERSCODE")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStat.AppendLine("   AND EXISTS (SELECT 1 FROM OIL.OIT0003_DETAIL DTL")
        sqlStat.AppendLine("                WHERE DTL.ORDERNO = ODR.ORDERNO")
        sqlStat.AppendLine("                  AND DTL.DELFLG  = @DELFLG")
        sqlStat.AppendLine("                  AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStat.AppendLine("                          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStat.AppendLine("                                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStat.AppendLine("                              ) ")
        sqlStat.AppendLine("                        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStat.AppendLine("                                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStat.AppendLine("                              )")
        sqlStat.AppendLine("                       )")
        sqlStat.AppendLine("    )")
        '荷主取得条件(JOINTコード考慮)↑
        '第二荷受人取得条件↓
        sqlStat.AppendLine("   AND ODR.CONSIGNEECODE = @CONSIGNEECODE")
        'sqlStat.AppendLine("   AND EXISTS (SELECT 1 FROM OIL.OIT0003_DETAIL DTL")
        'sqlStat.AppendLine("                WHERE DTL.ORDERNO = ODR.ORDERNO")
        'sqlStat.AppendLine("                  AND DTL.DELFLG  = @DELFLG")
        'sqlStat.AppendLine("                  AND ((     ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        'sqlStat.AppendLine("                          AND (    ISNULL(DTL.SECONDCONSIGNEECODE,'') = ''   ")
        'sqlStat.AppendLine("                                OR DTL.SECONDCONSIGNEECODE = ODR.CONSIGNEECODE ")
        'sqlStat.AppendLine("                              ) ")
        'sqlStat.AppendLine("                        ) OR  (     ODR.CONSIGNEECODE   <> @CONSIGNEECODE")
        'sqlStat.AppendLine("                                AND DTL.SECONDCONSIGNEECODE = @CONSIGNEECODE")
        'sqlStat.AppendLine("                              )")
        'sqlStat.AppendLine("                       )")
        'sqlStat.AppendLine("    )")
        '第二荷受人取得条件↑
        sqlStat.AppendLine("   AND ODR.DELFLG        = @DELFLG")
        sqlStat.AppendLine("   AND ODR.ORDERSTATUS  <> @ORDERSTATUS_CANCEL") 'キャンセルは含めない
        sqlStat.AppendLine("   AND (")
        '列車No、日付が複数ある為 (列車No 日付) or (列車No 日付) ・・・でつなぐ
        Dim trainDateWhereCond As String = "         (ODR.TRAINNO = '{0}' AND ODR.LODDATE = convert(date,'{1}'))"
        For Each targetTrainitm In targetTrainInfo
            sqlStat.AppendFormat(trainDateWhereCond, targetTrainitm.trainInfo.TrainNo, targetTrainitm.dayInfo.KeyString).AppendLine()
            trainDateWhereCond = "     OR (ODR.TRAINNO = '{0}' AND ODR.LODDATE = convert(date,'{1}'))"
        Next targetTrainitm
        sqlStat.AppendLine("       )")
        Dim retItm As OrderItem
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
                If parentConsignee <> "" Then
                    .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = parentConsignee
                Else
                    .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                End If
                .Add("@ORDERSTATUS_CANCEL", SqlDbType.NVarChar).Value = CONST_ORDERSTATUS_900
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                While sqlDr.Read
                    retItm = New OrderItem(sqlDr)
                    retVal.Add(retItm.OrderNo, retItm)
                End While
            End Using 'sqlDr
        End Using 'sqlCmd
        Return retVal
    End Function
    ''' <summary>
    ''' 受注作成用のオーダー情報よりアップデート対象を抽出
    ''' </summary>
    ''' <param name="sqlCon">sql接続</param>
    ''' <param name="dispData">画面情報クラス</param>
    ''' <returns>アップデート対象のリストクラスを生成</returns>
    Private Function GetEmptyTurnDetail(sqlCon As SqlConnection, dispData As DispDataClass, orderList As Dictionary(Of String, OrderItem), Optional isMi As Boolean = False) As Dictionary(Of String, OrderItem)
        Dim sqlStat As New StringBuilder
        Dim retVal = orderList
        '受注基本データがなければ取得する必要がないのでスキップ
        If orderList Is Nothing OrElse orderList.Count = 0 Then
            Return retVal
        End If
        '画面で選択された（選択されていないものは除去）日付、列車、油種情報を取得
        Dim targetTrainInfo = dispData.GetSuggestCheckedItem
        '前処理の入力チェックがあり、ここにきてありえないが選択されたデータが無い場合そのまま終了
        If targetTrainInfo Is Nothing OrElse targetTrainInfo.Count = 0 Then
            Return retVal
        End If
        sqlStat.AppendLine("SELECT ISNULL(RTRIM(DTL.ORDERNO),'')                 AS ORDERNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.DETAILNO),'')                AS DETAILNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SHIPORDER),'')               AS SHIPORDER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LINEORDER),'')               AS LINEORDER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.TANKNO),'')                  AS TANKNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.KAMOKU),'')                  AS KAMOKU")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.STACKINGORDERNO),'')         AS STACKINGORDERNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.STACKINGFLG),'')             AS STACKINGFLG")

        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.WHOLESALEFLG),'')              AS WHOLESALEFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.INSPECTIONFLG),'')             AS INSPECTIONFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.DETENTIONFLG),'')              AS DETENTIONFLG")

        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.FIRSTRETURNFLG),'')          AS FIRSTRETURNFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.AFTERRETURNFLG),'')          AS AFTERRETURNFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.OTTRANSPORTFLG),'')          AS OTTRANSPORTFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.UPGRADEFLG),'')              AS UPGRADEFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ORDERINFO),'')               AS ORDERINFO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SHIPPERSCODE),'')            AS SHIPPERSCODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SHIPPERSNAME),'')            AS SHIPPERSNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.OILCODE),'')                 AS OILCODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.OILNAME),'')                 AS OILNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ORDERINGTYPE),'')            AS ORDERINGTYPE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ORDERINGOILNAME),'')         AS ORDERINGOILNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.CARSNUMBER),'')              AS CARSNUMBER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.CARSAMOUNT),'')              AS CARSAMOUNT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.RETURNDATETRAIN),'')         AS RETURNDATETRAIN")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.JOINTCODE),'')               AS JOINTCODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.JOINT),'')                   AS JOINT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.REMARK),'')                  AS REMARK")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.CHANGETRAINNO),'')           AS CHANGETRAINNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.CHANGETRAINNAME),'')         AS CHANGETRAINNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SECONDCONSIGNEECODE),'')     AS SECONDCONSIGNEECODE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SECONDCONSIGNEENAME),'')     AS SECONDCONSIGNEENAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SECONDARRSTATION),'')        AS SECONDARRSTATION")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SECONDARRSTATIONNAME),'')    AS SECONDARRSTATIONNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.CHANGERETSTATION),'')        AS CHANGERETSTATION")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.CHANGERETSTATIONNAME),'')    AS CHANGERETSTATIONNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LINE),'')                    AS LINE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.FILLINGPOINT),'')            AS FILLINGPOINT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LOADINGIRILINETRAINNO),'')   AS LOADINGIRILINETRAINNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LOADINGIRILINETRAINNAME),'') AS LOADINGIRILINETRAINNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LOADINGIRILINEORDER),'')     AS LOADINGIRILINEORDER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LOADINGOUTLETTRAINNO),'')    AS LOADINGOUTLETTRAINNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LOADINGOUTLETTRAINNAME),'')  AS LOADINGOUTLETTRAINNAME")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.LOADINGOUTLETORDER),'')      AS LOADINGOUTLETORDER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ACTUALLODDATE),'')           AS ACTUALLODDATE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ACTUALDEPDATE),'')           AS ACTUALDEPDATE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ACTUALARRDATE),'')           AS ACTUALARRDATE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ACTUALACCDATE),'')           AS ACTUALACCDATE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ACTUALEMPARRDATE),'')  AS ACTUALEMPARRDATE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.RESERVEDNO),'')      AS RESERVEDNO")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.OTSENDCOUNT),0)      AS OTSENDCOUNT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.DLRESERVEDCOUNT),0)  AS DLRESERVEDCOUNT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.DLTAKUSOUCOUNT),0)   AS DLTAKUSOUCOUNT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SALSE),'')             AS SALSE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.SALSETAX),'')          AS SALSETAX")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.TOTALSALSE),'')        AS TOTALSALSE")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.PAYMENT),'')           AS PAYMENT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.PAYMENTTAX),'')        AS PAYMENTTAX")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.TOTALPAYMENT),'')      AS TOTALPAYMENT")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.ANASYORIFLG),'')            AS ANASYORIFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.VOLSYORIFLG),'')            AS VOLSYORIFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.DELFLG),'')            AS DELFLG")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(format(DTL.INITYMD,'yyyy/MM/dd HH:mm:ss.fff')),'')    AS INITYMD")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.INITUSER),'')          AS INITUSER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.INITTERMID),'')        AS INITTERMID")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(format(DTL.UPDYMD,'yyyy/MM/dd HH:mm:ss.fff')),'')     AS UPDYMD")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.UPDUSER),'')           AS UPDUSER")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(DTL.UPDTERMID),'')         AS UPDTERMID")
        sqlStat.AppendLine("      ,ISNULL(RTRIM(format(DTL.RECEIVEYMD,'yyyy/MM/dd HH:mm:ss.fff')),'') AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM OIL.OIT0003_DETAIL DTL with(nolock)")
        sqlStat.AppendLine(" WHERE DTL.DELFLG = @DELFLG")
        '荷主取得条件(JOINTコード考慮)↓
        sqlStat.AppendLine("   AND ((     DTL.SHIPPERSCODE   = @SHIPPERSCODE")
        sqlStat.AppendLine("          AND (    ISNULL(DTL.JOINTCODE,'') = ''   ")
        sqlStat.AppendLine("                OR DTL.JOINTCODE = DTL.SHIPPERSCODE ")
        sqlStat.AppendLine("              ) ")
        sqlStat.AppendLine("        ) OR  (     DTL.SHIPPERSCODE   <> @SHIPPERSCODE")
        sqlStat.AppendLine("                AND DTL.JOINTCODE = @SHIPPERSCODE")
        sqlStat.AppendLine("              )")
        sqlStat.AppendLine("       )")
        '荷主取得条件(JOINTコード考慮)↑
        sqlStat.AppendLine("   AND (")
        '対象オーダーNoをOR条件で積み上げ
        Dim orderNoCond As String = "        DTL.ORDERNO = '{0}'"
        For Each odrNo In orderList.Keys
            sqlStat.AppendFormat(orderNoCond, odrNo).AppendLine()
            orderNoCond = "     OR DTL.ORDERNO = '{0}'"
        Next
        sqlStat.AppendLine("       )")
        If isMi Then
            sqlStat.AppendFormat("   AND DTL.SECONDCONSIGNEECODE = '{0}'", dispData.Consignee).AppendLine()
        Else
            sqlStat.AppendFormat("   AND (ISNULL(DTL.SECONDCONSIGNEECODE,'') = '' OR DTL.SECONDCONSIGNEECODE = '{0}')", dispData.Consignee).AppendLine()
        End If
        Dim retItm As OrderDetailItem
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = dispData.Shipper
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                While sqlDr.Read
                    retItm = New OrderDetailItem(sqlDr)
                    retVal(retItm.OrderNo).DetailList.Add(retItm)
                End While
            End Using 'sqlDr
        End Using 'sqlCmd
        Return retVal
    End Function
    ''' <summary>
    ''' 受注作成用のオーダー情報より最大受注明細№を取得
    ''' </summary>
    ''' <param name="sqlCon">sql接続</param>
    ''' <param name="dispData">画面情報クラス</param>
    ''' <returns>最大受注明細Noを格納した受注作成用のオーダー情報</returns>
    Private Function GetEmptyTurnMaxDetailNo(sqlCon As SqlConnection, dispData As DispDataClass, orderList As Dictionary(Of String, OrderItem)) As Dictionary(Of String, OrderItem)
        Dim sqlStat As New StringBuilder
        Dim retVal = orderList
        '既登録データがない場合はそもそも取得する必要がないのでスキップ
        If orderList Is Nothing OrElse orderList.Count = 0 Then
            Return retVal
        End If
        '画面で選択された（選択されていないものは除去）日付、列車、油種情報を取得
        Dim targetTrainInfo = dispData.GetSuggestCheckedItem
        '前処理の入力チェックがあり、ここにきてありえないが選択されたデータが無い場合そのまま終了
        If targetTrainInfo Is Nothing OrElse targetTrainInfo.Count = 0 Then
            Return retVal
        End If
        sqlStat.AppendLine("SELECT ISNULL(RTRIM(DTL.ORDERNO),'')           AS ORDERNO")
        sqlStat.AppendLine("      ,MAX(ISNULL(RTRIM(DTL.DETAILNO),'000'))  AS MAXDETAILNO")
        sqlStat.AppendLine("  FROM OIL.OIT0003_DETAIL DTL  with(nolock)")
        sqlStat.AppendLine(" WHERE (")
        '対象オーダーNoをOR条件で積み上げ
        Dim orderNoCond As String = "        DTL.ORDERNO = '{0}'"
        For Each odrNo In orderList.Keys
            sqlStat.AppendFormat(orderNoCond, odrNo).AppendLine()
            orderNoCond = "     OR DTL.ORDERNO = '{0}'"
        Next
        sqlStat.AppendLine("       )")
        sqlStat.AppendLine("GROUP BY DTL.ORDERNO")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            Dim orderNo As String = ""
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                While sqlDr.Read
                    orderNo = Convert.ToString(sqlDr("ORDERNO"))
                    retVal(orderNo).MaxDetailNo = Convert.ToString(sqlDr("MAXDETAILNO"))
                End While
            End Using 'sqlDr
        End Using 'sqlCmd
        Return retVal
    End Function
    ''' <summary>
    ''' 荷受人マスタより提案表の表示可否を取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="consignee"></param>
    ''' <returns></returns>
    Private Function IsShowSuggestList(sqlCon As SqlConnection, consignee As String) As Boolean
        Dim sqlStr As New StringBuilder
        sqlStr.AppendLine("SELECT NIUKE.CONSIGNEECODE  AS CONSIGNEECODE")
        sqlStr.AppendLine("  FROM OIL.OIM0012_NIUKE NIUKE")
        sqlStr.AppendLine(" WHERE NIUKE.CONSIGNEECODE  = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND NIUKE.STOCKFLG       = @STOCKFLG")
        sqlStr.AppendLine("   AND NIUKE.DELFLG         = @DELFLG")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = consignee
                .Add("@STOCKFLG", SqlDbType.NVarChar).Value = "1" '不等号条件
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Return True
                End If
            End Using 'sqlDr
        End Using
        Return False
    End Function
    ''' <summary>
    ''' ローリー初期表示判定
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="consignee"></param>
    ''' <returns></returns>
    Private Function IsShowLorryValue(sqlCon As SqlConnection, consignee As String) As String
        Dim sqlStr As New StringBuilder
        sqlStr.AppendLine("SELECT FX.KEYCODE  AS CONSIGNEECODE")
        sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
        sqlStr.AppendLine(" WHERE FX.CLASS    = @CLASS")
        sqlStr.AppendLine("   AND FX.KEYCODE  = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@CLASS", SqlDbType.NVarChar).Value = "STOCKLORRYINITSHOW"
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = consignee
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Return "full"
                End If
            End Using 'sqlDr
        End Using
        Return "hideLorry"
    End Function
    ''' <summary>
    ''' 在庫テーブル登録処理
    ''' </summary>
    ''' <param name="sqlCon">SQL接続文字</param>
    ''' <param name="dispDataClass">画面入力データクラス</param>
    ''' <returns></returns>
    ''' <remarks>データがあれば更新、なければ追加（OIT0001_OILSTOCKテーブル内での履歴登録は無し）
    ''' 一旦、新規登録後に油種マスタから削除された後の更新パターンは考慮しない
    ''' （このパターンは画面上は出ないが宙に浮いたDELFLGが生きたままのデータが残る想定）</remarks>
    Private Function EntryStockData(sqlCon As SqlConnection, dispDataClass As DispDataClass, ByRef errNum As String, Optional procDtm As Date = #1900/01/01#, Optional sqlTran As SqlTransaction = Nothing) As Boolean

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("DECLARE @hensuu AS bigint ;")
        sqlStat.AppendLine("    SET @hensuu = 0 ;")
        sqlStat.AppendLine("DECLARE hensuu CURSOR FOR")
        sqlStat.AppendLine("    SELECT ")
        sqlStat.AppendLine("           CAST(UPDTIMSTP AS bigint) AS hensuu")
        sqlStat.AppendLine("      FROM OIL.OIT0001_OILSTOCK")
        sqlStat.AppendLine("     WHERE STOCKYMD      = @STOCKYMD")
        sqlStat.AppendLine("       AND OFFICECODE    = @OFFICECODE")
        sqlStat.AppendLine("       AND OILCODE       = @OILCODE")
        sqlStat.AppendLine("       AND SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStat.AppendLine("       AND CONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("   OPEN hensuu ;")
        sqlStat.AppendLine("  FETCH NEXT FROM hensuu INTO @hensuu ;")
        'UPDATE
        sqlStat.AppendLine("     IF (@@FETCH_STATUS = 0)")
        sqlStat.AppendLine("         UPDATE OIL.OIT0001_OILSTOCK")
        sqlStat.AppendLine("            SET MAXTANKCAP  = @MAXTANKCAP")
        sqlStat.AppendLine("               ,TANKCAPRATE = @TANKCAPRATE")
        sqlStat.AppendLine("               ,DS          = @DS")
        sqlStat.AppendLine("               ,PREVOL      = @PREVOL")
        sqlStat.AppendLine("               ,STOCKDAY    = @STOCKDAY")
        sqlStat.AppendLine("               ,PRESTOCK    = @PRESTOCK")
        sqlStat.AppendLine("               ,MORSTOCK    = @MORSTOCK")
        sqlStat.AppendLine("               ,SHIPPINGVOL = @SHIPPINGVOL")
        sqlStat.AppendLine("               ,ARRVOL      = @ARRVOL")
        sqlStat.AppendLine("               ,ARRLORRYVOL = @ARRLORRYVOL")
        sqlStat.AppendLine("               ,EVESTOCK    = @EVESTOCK")
        sqlStat.AppendLine("               ,DELFLG      = @DELFLG")
        sqlStat.AppendLine("               ,UPDYMD      = @UPDYMD")
        sqlStat.AppendLine("               ,UPDUSER     = @UPDUSER")
        sqlStat.AppendLine("               ,UPDTERMID   = @UPDTERMID")
        sqlStat.AppendLine("               ,RECEIVEYMD  = @RECEIVEYMD")
        sqlStat.AppendLine("          WHERE STOCKYMD      = @STOCKYMD")
        sqlStat.AppendLine("            AND OFFICECODE    = @OFFICECODE")
        sqlStat.AppendLine("            AND OILCODE       = @OILCODE")
        sqlStat.AppendLine("            AND SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStat.AppendLine("            AND CONSIGNEECODE = @CONSIGNEECODE")
        'INSERT
        sqlStat.AppendLine("     IF (@@FETCH_STATUS <> 0)")
        sqlStat.AppendLine("         INSERT INTO OIL.OIT0001_OILSTOCK (")
        sqlStat.AppendLine("             STOCKYMD")
        sqlStat.AppendLine("            ,OFFICECODE")
        sqlStat.AppendLine("            ,OILCODE")
        sqlStat.AppendLine("            ,SHIPPERSCODE")
        sqlStat.AppendLine("            ,CONSIGNEECODE")
        sqlStat.AppendLine("            ,MAXTANKCAP")
        sqlStat.AppendLine("            ,TANKCAPRATE")
        sqlStat.AppendLine("            ,DS")
        sqlStat.AppendLine("            ,PREVOL")
        sqlStat.AppendLine("            ,STOCKDAY")
        sqlStat.AppendLine("            ,PRESTOCK")
        sqlStat.AppendLine("            ,MORSTOCK")
        sqlStat.AppendLine("            ,SHIPPINGVOL")
        sqlStat.AppendLine("            ,ARRVOL")
        sqlStat.AppendLine("            ,ARRLORRYVOL")
        sqlStat.AppendLine("            ,EVESTOCK")
        sqlStat.AppendLine("            ,DELFLG")
        sqlStat.AppendLine("            ,INITYMD")
        sqlStat.AppendLine("            ,INITUSER")
        sqlStat.AppendLine("            ,INITTERMID")
        sqlStat.AppendLine("            ,UPDYMD")
        sqlStat.AppendLine("            ,UPDUSER")
        sqlStat.AppendLine("            ,UPDTERMID")
        sqlStat.AppendLine("            ,RECEIVEYMD")
        sqlStat.AppendLine("         ) VALUES (")
        sqlStat.AppendLine("             @STOCKYMD")
        sqlStat.AppendLine("            ,@OFFICECODE")
        sqlStat.AppendLine("            ,@OILCODE")
        sqlStat.AppendLine("            ,@SHIPPERSCODE")
        sqlStat.AppendLine("            ,@CONSIGNEECODE")
        sqlStat.AppendLine("            ,@MAXTANKCAP")
        sqlStat.AppendLine("            ,@TANKCAPRATE")
        sqlStat.AppendLine("            ,@DS")
        sqlStat.AppendLine("            ,@PREVOL")
        sqlStat.AppendLine("            ,@STOCKDAY")
        sqlStat.AppendLine("            ,@PRESTOCK")
        sqlStat.AppendLine("            ,@MORSTOCK")
        sqlStat.AppendLine("            ,@SHIPPINGVOL")
        sqlStat.AppendLine("            ,@ARRVOL")
        sqlStat.AppendLine("            ,@ARRLORRYVOL")
        sqlStat.AppendLine("            ,@EVESTOCK")
        sqlStat.AppendLine("            ,@DELFLG")
        sqlStat.AppendLine("            ,@INITYMD")
        sqlStat.AppendLine("            ,@INITUSER")
        sqlStat.AppendLine("            ,@INITTERMID")
        sqlStat.AppendLine("            ,@UPDYMD")
        sqlStat.AppendLine("            ,@UPDUSER")
        sqlStat.AppendLine("            ,@UPDTERMID")
        sqlStat.AppendLine("            ,@RECEIVEYMD")
        sqlStat.AppendLine("         );")
        sqlStat.AppendLine("  CLOSE hensuu ;")
        sqlStat.AppendLine("DEALLOCATE hensuu ;")

        'ジャーナル用記載用データ抽出SQL（この関数で登録されたものを取得想定（登録するDBのキーを変えた際は注意））
        Dim journalSqlStat As New StringBuilder
        journalSqlStat.AppendLine("SELECT ")
        journalSqlStat.AppendLine("             STOCKYMD")
        journalSqlStat.AppendLine("            ,OFFICECODE")
        journalSqlStat.AppendLine("            ,OILCODE")
        journalSqlStat.AppendLine("            ,SHIPPERSCODE")
        journalSqlStat.AppendLine("            ,CONSIGNEECODE")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,MAXTANKCAP))  AS MAXTANKCAP")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,TANKCAPRATE)) AS TANKCAPRATE")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,DS))          AS DS")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,PREVOL))      AS PREVOL")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,STOCKDAY))    AS STOCKDAY")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,PRESTOCK))    AS PRESTOCK")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,MORSTOCK))    AS MORSTOCK")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,SHIPPINGVOL)) AS SHIPPINGVOL")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,ARRVOL))      AS ARRVOL")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,ARRLORRYVOL)) AS ARRLORRYVOL")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,EVESTOCK))    AS EVESTOCK")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,DELFLG))      AS DELFLG")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITYMD))     AS INITYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITUSER))    AS INITUSER")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITTERMID))  AS INITTERMID")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDYMD))      AS UPDYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDUSER))     AS UPDUSER")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDTERMID))   AS UPDTERMID")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,RECEIVEYMD))  AS RECEIVEYMD")
        journalSqlStat.AppendLine("  FROM OIL.OIT0001_OILSTOCK WITH(nolock)")
        journalSqlStat.AppendLine(" WHERE OFFICECODE    = @OFFICECODE")
        journalSqlStat.AppendLine("   AND SHIPPERSCODE  = @SHIPPERSCODE")
        journalSqlStat.AppendLine("   AND CONSIGNEECODE = @CONSIGNEECODE")
        journalSqlStat.AppendLine("   AND UPDYMD        = @UPDYMD")

        '処理日付引数が初期値なら現時刻設定
        If procDtm.ToString("yyyy/MM/dd").Equals("1900/01/01") Then
            procDtm = Now
        End If

        'トランザクションしない場合は「sqlCon.BeginTransaction」→「nothing」
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            sqlCmd.CommandTimeout = 300
            '固定パラメータ
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispDataClass.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispDataClass.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispDataClass.Consignee
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@INITYMD", SqlDbType.DateTime).Value = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@INITUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@INITTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = C_DEFAULT_YMD
            End With
            '変動パラメータ
            Dim paramStockYmd = sqlCmd.Parameters.Add("@STOCKYMD", SqlDbType.Date)
            Dim paramOilcode = sqlCmd.Parameters.Add("@OILCODE", SqlDbType.NVarChar)
            Dim paramMaxTankCap = sqlCmd.Parameters.Add("@MAXTANKCAP", SqlDbType.Decimal)
            Dim paramTankCapRate = sqlCmd.Parameters.Add("@TANKCAPRATE", SqlDbType.Decimal)
            Dim paramDS = sqlCmd.Parameters.Add("@DS", SqlDbType.Decimal)
            Dim paramPreVol = sqlCmd.Parameters.Add("@PREVOL", SqlDbType.Decimal)
            Dim paramStockDay = sqlCmd.Parameters.Add("@STOCKDAY", SqlDbType.Decimal)
            Dim paramPreStock = sqlCmd.Parameters.Add("@PRESTOCK", SqlDbType.Decimal)
            Dim paramMorStock = sqlCmd.Parameters.Add("@MORSTOCK", SqlDbType.Decimal)
            Dim paramShippingVol = sqlCmd.Parameters.Add("@SHIPPINGVOL", SqlDbType.Decimal)
            Dim paramArrVol = sqlCmd.Parameters.Add("@ARRVOL", SqlDbType.Decimal)
            Dim paramArrLorryVol = sqlCmd.Parameters.Add("@ARRLORRYVOL", SqlDbType.Decimal)
            Dim paramEveStock = sqlCmd.Parameters.Add("@EVESTOCK", SqlDbType.Decimal)
            '画面データをループしテーブル更新
            '油種別のループ
            For Each stockItem In dispDataClass.StockList.Values
                '油種でのパラメータ設定
                paramOilcode.Value = stockItem.OilTypeCode '油種コード
                paramMaxTankCap.Value = stockItem.TankCapacity 'タンク容量
                paramTankCapRate.Value = stockItem.TargetStockRate 'タンク容量率(目標)
                paramDS.Value = stockItem.DS 'D/S
                paramPreVol.Value = stockItem.LastShipmentAve '前週平均出荷量
                '日付別ループ
                For Each daysValue In stockItem.StockItemList.Values
                    If daysValue.DaysItem.IsPastDay Then
                        Continue For
                    End If
                    paramStockYmd.Value = daysValue.DaysItem.ItemDate   '在庫年月日
                    paramStockDay.Value = daysValue.Retentiondays       '保有日数
                    paramPreStock.Value = daysValue.LastEveningStock    '前日夕在庫
                    paramMorStock.Value = daysValue.MorningStock        '朝在庫
                    paramShippingVol.Value = daysValue.Send             '払出
                    paramArrVol.Value = daysValue.Receive               '受入
                    paramArrLorryVol.Value = daysValue.ReceiveFromLorry 'ローリー受入
                    paramEveStock.Value = daysValue.EveningStock        '夕在庫
                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                Next daysValue
            Next stockItem

            'ジャーナル用のデータ取得
            sqlCmd.CommandText = journalSqlStat.ToString
            Using journalDt As New DataTable,
                  SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    journalDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                journalDt.Load(SQLdr)
                OutputJournal(journalDt)
                journalDt.Clear()
            End Using 'journalDt,journalDt
        End Using 'tran,sqlCmd
        Return True

    End Function
    ''' <summary>
    ''' 列車運行マスタにロック情報を更新
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispDataClass"></param>
    ''' <param name="errNum"></param>
    ''' <param name="procDtm"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Private Function EntryTrainOperation(sqlCon As SqlConnection, dispDataClass As DispDataClass, ByRef errNum As String, Optional procDtm As Date = #1900/01/01#, Optional sqlTran As SqlTransaction = Nothing) As Boolean

        If dispDataClass.ShowSuggestList = False Then
            'OT関連は入力しないのでロック無し
            Return True
        End If
        Dim targetDateData = From daysItm In dispDataClass.SuggestList.Values Where daysItm.DayInfo.IsBeforeToday = False
        '過去日の更新は行わない
        If targetDateData.Any = False Then
            Return True
        End If
        Dim targetTrainData = From dateItm In targetDateData Where (From trainItm In dateItm.SuggestOrderItem.Values Where trainItm.TrainInfo.UnmanagedTrain = False).Any
        '管理対象列車（川崎などを除く）が存在しない場合は更新を行わない
        If targetTrainData.Any = False Then
            Return True
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("DECLARE @hensuu AS bigint ;")
        sqlStat.AppendLine("    SET @hensuu = 0 ;")
        sqlStat.AppendLine("DECLARE hensuu CURSOR FOR")
        sqlStat.AppendLine("    SELECT ")
        sqlStat.AppendLine("           CAST(UPDTIMSTP AS bigint) AS hensuu")
        sqlStat.AppendLine("      FROM OIL.OIM0017_TRAINOPERATION")
        sqlStat.AppendLine("     WHERE OFFICECODE  = @OFFICECODE")
        sqlStat.AppendLine("       AND TRAINNO     = @TRAINNO")
        sqlStat.AppendLine("       AND WORKINGDATE = @WORKINGDATE")
        sqlStat.AppendLine("       AND TSUMI       = @TSUMI")
        sqlStat.AppendLine("       AND DEPSTATION  = @DEPSTATION")
        sqlStat.AppendLine("       AND ARRSTATION  = @ARRSTATION")
        sqlStat.AppendLine("   OPEN hensuu ;")
        sqlStat.AppendLine("  FETCH NEXT FROM hensuu INTO @hensuu ;")
        'UPDATE
        sqlStat.AppendLine("     IF (@@FETCH_STATUS = 0)")
        sqlStat.AppendLine("         UPDATE OIL.OIM0017_TRAINOPERATION")
        sqlStat.AppendLine("            SET RUN         = @RUN")
        sqlStat.AppendLine("               ,DELFLG      = @DELFLG")
        sqlStat.AppendLine("               ,UPDYMD      = @UPDYMD")
        sqlStat.AppendLine("               ,UPDUSER     = @UPDUSER")
        sqlStat.AppendLine("               ,UPDTERMID   = @UPDTERMID")
        sqlStat.AppendLine("               ,RECEIVEYMD  = @RECEIVEYMD")
        sqlStat.AppendLine("          WHERE OFFICECODE  = @OFFICECODE")
        sqlStat.AppendLine("            AND TRAINNO     = @TRAINNO")
        sqlStat.AppendLine("            AND WORKINGDATE = @WORKINGDATE")
        sqlStat.AppendLine("            AND TSUMI       = @TSUMI")
        sqlStat.AppendLine("            AND DEPSTATION  = @DEPSTATION")
        sqlStat.AppendLine("            AND ARRSTATION  = @ARRSTATION")
        'INSERT
        sqlStat.AppendLine("     IF (@@FETCH_STATUS <> 0)")
        sqlStat.AppendLine("         INSERT INTO OIL.OIM0017_TRAINOPERATION (")
        sqlStat.AppendLine("             OFFICECODE")
        sqlStat.AppendLine("            ,TRAINNO")
        sqlStat.AppendLine("            ,TRAINNAME")
        sqlStat.AppendLine("            ,WORKINGDATE")
        sqlStat.AppendLine("            ,TSUMI")
        sqlStat.AppendLine("            ,DEPSTATION")
        sqlStat.AppendLine("            ,ARRSTATION")
        sqlStat.AppendLine("            ,RUN")
        sqlStat.AppendLine("            ,DELFLG")
        sqlStat.AppendLine("            ,INITYMD")
        sqlStat.AppendLine("            ,INITUSER")
        sqlStat.AppendLine("            ,INITTERMID")
        sqlStat.AppendLine("            ,UPDYMD")
        sqlStat.AppendLine("            ,UPDUSER")
        sqlStat.AppendLine("            ,UPDTERMID")
        sqlStat.AppendLine("            ,RECEIVEYMD")
        sqlStat.AppendLine("         ) VALUES (")
        sqlStat.AppendLine("             @OFFICECODE")
        sqlStat.AppendLine("            ,@TRAINNO")
        sqlStat.AppendLine("            ,@TRAINNAME")
        sqlStat.AppendLine("            ,@WORKINGDATE")
        sqlStat.AppendLine("            ,@TSUMI")
        sqlStat.AppendLine("            ,@DEPSTATION")
        sqlStat.AppendLine("            ,@ARRSTATION")
        sqlStat.AppendLine("            ,@RUN")
        sqlStat.AppendLine("            ,@DELFLG")
        sqlStat.AppendLine("            ,@INITYMD")
        sqlStat.AppendLine("            ,@INITUSER")
        sqlStat.AppendLine("            ,@INITTERMID")
        sqlStat.AppendLine("            ,@UPDYMD")
        sqlStat.AppendLine("            ,@UPDUSER")
        sqlStat.AppendLine("            ,@UPDTERMID")
        sqlStat.AppendLine("            ,@RECEIVEYMD")
        sqlStat.AppendLine("         );")
        sqlStat.AppendLine("  CLOSE hensuu ;")
        sqlStat.AppendLine("DEALLOCATE hensuu ;")

        'ジャーナル用記載用データ抽出SQL（この関数で登録されたものを取得想定（登録するDBのキーを変えた際は注意））
        Dim journalSqlStat As New StringBuilder
        journalSqlStat.AppendLine("SELECT ")
        journalSqlStat.AppendLine("             OFFICECODE")
        journalSqlStat.AppendLine("            ,TRAINNO")
        journalSqlStat.AppendLine("            ,isnull(TRAINNAME,'') AS TRAINNAME")
        journalSqlStat.AppendLine("            ,WORKINGDATE")
        journalSqlStat.AppendLine("            ,TSUMI")
        journalSqlStat.AppendLine("            ,DEPSTATION")
        journalSqlStat.AppendLine("            ,ARRSTATION")
        journalSqlStat.AppendLine("            ,RUN")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,DELFLG))      AS DELFLG")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITYMD))     AS INITYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITUSER))    AS INITUSER")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITTERMID))  AS INITTERMID")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDYMD))      AS UPDYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDUSER))     AS UPDUSER")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDTERMID))   AS UPDTERMID")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,RECEIVEYMD))  AS RECEIVEYMD")
        journalSqlStat.AppendLine("  FROM OIL.OIM0017_TRAINOPERATION WITH(nolock)")
        journalSqlStat.AppendLine(" WHERE OFFICECODE  = @OFFICECODE")
        journalSqlStat.AppendLine("   AND UPDUSER     = @UPDUSER")
        journalSqlStat.AppendLine("   AND UPDYMD      = @UPDYMD")

        '処理日付引数が初期値なら現時刻設定
        If procDtm.ToString("yyyy/MM/dd").Equals("1900/01/01") Then
            procDtm = Now
        End If

        'トランザクションしない場合は「sqlCon.BeginTransaction」→「nothing」
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            sqlCmd.CommandTimeout = 300
            '固定パラメータ
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispDataClass.SalesOffice
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@INITYMD", SqlDbType.DateTime).Value = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@INITUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@INITTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = C_DEFAULT_YMD
            End With
            '変動パラメータ
            Dim paramTrainNo = sqlCmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar)
            Dim paramTrainName = sqlCmd.Parameters.Add("@TRAINNAME", SqlDbType.NVarChar)
            Dim paramWorkingDate = sqlCmd.Parameters.Add("@WORKINGDATE", SqlDbType.Date)
            Dim paramTsumi = sqlCmd.Parameters.Add("@TSUMI", SqlDbType.NVarChar)
            Dim paramDepStation = sqlCmd.Parameters.Add("@DEPSTATION", SqlDbType.NVarChar)
            Dim paramArrStation = sqlCmd.Parameters.Add("@ARRSTATION", SqlDbType.NVarChar)
            Dim paramRun = sqlCmd.Parameters.Add("@RUN", SqlDbType.NVarChar)

            '画面データをループしテーブル更新
            '日付別のループ
            For Each dateItem In targetTrainData
                '日付でのパラメータ設定
                paramWorkingDate.Value = dateItem.DayInfo.ItemDate
                '画面外データは対象外
                If dateItem.DayInfo.IsDispArea = False Then
                    Continue For
                End If
                '列車別ループ
                For Each trainItem In dateItem.SuggestOrderItem.Values
                    If trainItem.TrainInfo.UnmanagedTrain Then
                        Continue For
                    End If
                    paramTrainNo.Value = trainItem.TrainInfo.TrainNo
                    paramTrainName.Value = trainItem.TrainInfo.TrainName
                    paramTsumi.Value = trainItem.TrainInfo.Tsumi
                    paramDepStation.Value = trainItem.TrainInfo.DepStation
                    paramArrStation.Value = trainItem.TrainInfo.ArrStation
                    Dim trRun As String
                    trRun = "1"
                    If trainItem.TrainLock Then
                        trRun = "0"
                    End If
                    paramRun.Value = trRun

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                Next trainItem
            Next dateItem

            'ジャーナル用のデータ取得
            sqlCmd.CommandText = journalSqlStat.ToString
            Using journalDt As New DataTable,
                  SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    journalDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                journalDt.Load(SQLdr)
                OutputJournal(journalDt)
                journalDt.Clear()
            End Using 'journalDt,journalDt
        End Using 'tran,sqlCmd
        Return True
    End Function

    ''' <summary>
    ''' 在庫受入車数テーブルの更新
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispDataClass"></param>
    ''' <param name="errNum"></param>
    ''' <param name="procDtm"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Private Function EntryUkeireOilstock(sqlCon As SqlConnection, dispDataClass As DispDataClass, ByRef errNum As String, Optional procDtm As Date = #1900/01/01#, Optional sqlTran As SqlTransaction = Nothing) As Boolean

        '提案表車数の表示が無い場合はスキップ
        If dispDataClass.ShowSuggestList = False Then
            Return True
        End If
        '列車チェックボックス確認(この返却はあくまでチェックがある日付グループを返す)
        'Dim qSelected = (From sugItm In dispDataClass.SuggestList
        '                 Where (From trItm In sugItm.Value.SuggestOrderItem.Values
        '                        Where trItm.CheckValue).Any)
        '20200529 未チェックでも保存
        Dim qSelected = (From sugItm In dispDataClass.SuggestList
                         Where sugItm.Value.DayInfo.IsDispArea AndAlso (From trItm In sugItm.Value.SuggestOrderItem.Values).Any)

        '1つもチェックが無ければスキップ
        If qSelected.Any = False Then
            Return True
        End If
        '油種コードをフィールド名に割り当てる変数
        Dim oilCodeToFieldNameList As New Dictionary(Of String, String) From {
            {"1101", "RTANK"}, {"1001", "HTANK"}, {"1301", "TTANK"}, {"1302", "MTTANK"},
            {"1401", "KTANK"}, {"1404", "K3TANK"}, {"2201", "LTANK"}, {"2101", "ATANK"}}

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("DECLARE @hensuu AS bigint ;")
        sqlStat.AppendLine("    SET @hensuu = 0 ;")
        sqlStat.AppendLine("DECLARE hensuu CURSOR FOR")
        sqlStat.AppendLine("    SELECT ")
        sqlStat.AppendLine("           CAST(UPDTIMSTP AS bigint) AS hensuu")
        sqlStat.AppendLine("      FROM OIL.OIT0009_UKEIREOILSTOCK")
        sqlStat.AppendLine("     WHERE STOCKYMD      = @STOCKYMD")
        sqlStat.AppendLine("       AND OFFICECODE    = @OFFICECODE")
        sqlStat.AppendLine("       AND SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStat.AppendLine("       AND CONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("       AND TRAINNO       = @TRAINNO")
        sqlStat.AppendLine("   OPEN hensuu ;")
        sqlStat.AppendLine("  FETCH NEXT FROM hensuu INTO @hensuu ;")
        'UPDATE
        sqlStat.AppendLine("     IF (@@FETCH_STATUS = 0)")
        sqlStat.AppendLine("         UPDATE OIL.OIT0009_UKEIREOILSTOCK")
        sqlStat.AppendLine("            SET ACCYMD     = @ACCYMD")
        sqlStat.AppendLine("               ,RTANK1     = @RTANK1")
        sqlStat.AppendLine("               ,HTANK1     = @HTANK1")
        sqlStat.AppendLine("               ,TTANK1     = @TTANK1")
        sqlStat.AppendLine("               ,MTTANK1    = @MTTANK1")
        sqlStat.AppendLine("               ,KTANK1     = @KTANK1")
        sqlStat.AppendLine("               ,K3TANK1    = @K3TANK1")
        sqlStat.AppendLine("               ,LTANK1     = @LTANK1")
        sqlStat.AppendLine("               ,ATANK1     = @ATANK1")
        sqlStat.AppendLine("               ,RTANK2     = @RTANK2")
        sqlStat.AppendLine("               ,HTANK2     = @HTANK2")
        sqlStat.AppendLine("               ,TTANK2     = @TTANK2")
        sqlStat.AppendLine("               ,MTTANK2    = @MTTANK2")
        sqlStat.AppendLine("               ,KTANK2     = @KTANK2")
        sqlStat.AppendLine("               ,K3TANK2    = @K3TANK2")
        sqlStat.AppendLine("               ,LTANK2     = @LTANK2")
        sqlStat.AppendLine("               ,ATANK2     = @ATANK2")
        sqlStat.AppendLine("               ,DELFLG     = @DELFLG")
        sqlStat.AppendLine("               ,UPDYMD     = @UPDYMD")
        sqlStat.AppendLine("               ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("               ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("               ,RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("          WHERE STOCKYMD      = @STOCKYMD")
        sqlStat.AppendLine("            AND OFFICECODE    = @OFFICECODE")
        sqlStat.AppendLine("            AND SHIPPERSCODE  = @SHIPPERSCODE")
        sqlStat.AppendLine("            AND CONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("            AND TRAINNO       = @TRAINNO")
        'INSERT
        sqlStat.AppendLine("     IF (@@FETCH_STATUS <> 0)")
        sqlStat.AppendLine("         INSERT INTO OIL.OIT0009_UKEIREOILSTOCK (")
        sqlStat.AppendLine("             STOCKYMD")
        sqlStat.AppendLine("            ,OFFICECODE")
        sqlStat.AppendLine("            ,SHIPPERSCODE")
        sqlStat.AppendLine("            ,CONSIGNEECODE")
        sqlStat.AppendLine("            ,TRAINNO")
        sqlStat.AppendLine("            ,ACCYMD")
        sqlStat.AppendLine("            ,RTANK1")
        sqlStat.AppendLine("            ,HTANK1")
        sqlStat.AppendLine("            ,TTANK1")
        sqlStat.AppendLine("            ,MTTANK1")
        sqlStat.AppendLine("            ,KTANK1")
        sqlStat.AppendLine("            ,K3TANK1")
        sqlStat.AppendLine("            ,LTANK1")
        sqlStat.AppendLine("            ,ATANK1")
        sqlStat.AppendLine("            ,RTANK2")
        sqlStat.AppendLine("            ,HTANK2")
        sqlStat.AppendLine("            ,TTANK2")
        sqlStat.AppendLine("            ,MTTANK2")
        sqlStat.AppendLine("            ,KTANK2")
        sqlStat.AppendLine("            ,K3TANK2")
        sqlStat.AppendLine("            ,LTANK2")
        sqlStat.AppendLine("            ,ATANK2")
        sqlStat.AppendLine("            ,DELFLG")
        sqlStat.AppendLine("            ,INITYMD")
        sqlStat.AppendLine("            ,INITUSER")
        sqlStat.AppendLine("            ,INITTERMID")
        sqlStat.AppendLine("            ,UPDYMD")
        sqlStat.AppendLine("            ,UPDUSER")
        sqlStat.AppendLine("            ,UPDTERMID")
        sqlStat.AppendLine("            ,RECEIVEYMD")
        sqlStat.AppendLine("         ) VALUES (")
        sqlStat.AppendLine("             @STOCKYMD")
        sqlStat.AppendLine("            ,@OFFICECODE")
        sqlStat.AppendLine("            ,@SHIPPERSCODE")
        sqlStat.AppendLine("            ,@CONSIGNEECODE")
        sqlStat.AppendLine("            ,@TRAINNO")
        sqlStat.AppendLine("            ,@ACCYMD")
        sqlStat.AppendLine("            ,@RTANK1")
        sqlStat.AppendLine("            ,@HTANK1")
        sqlStat.AppendLine("            ,@TTANK1")
        sqlStat.AppendLine("            ,@MTTANK1")
        sqlStat.AppendLine("            ,@KTANK1")
        sqlStat.AppendLine("            ,@K3TANK1")
        sqlStat.AppendLine("            ,@LTANK1")
        sqlStat.AppendLine("            ,@ATANK1")
        sqlStat.AppendLine("            ,@RTANK2")
        sqlStat.AppendLine("            ,@HTANK2")
        sqlStat.AppendLine("            ,@TTANK2")
        sqlStat.AppendLine("            ,@MTTANK2")
        sqlStat.AppendLine("            ,@KTANK2")
        sqlStat.AppendLine("            ,@K3TANK2")
        sqlStat.AppendLine("            ,@LTANK2")
        sqlStat.AppendLine("            ,@ATANK2")
        sqlStat.AppendLine("            ,@DELFLG")
        sqlStat.AppendLine("            ,@INITYMD")
        sqlStat.AppendLine("            ,@INITUSER")
        sqlStat.AppendLine("            ,@INITTERMID")
        sqlStat.AppendLine("            ,@UPDYMD")
        sqlStat.AppendLine("            ,@UPDUSER")
        sqlStat.AppendLine("            ,@UPDTERMID")
        sqlStat.AppendLine("            ,@RECEIVEYMD")
        sqlStat.AppendLine("         );")
        sqlStat.AppendLine("  CLOSE hensuu ;")
        sqlStat.AppendLine("DEALLOCATE hensuu ;")

        'ジャーナル用記載用データ抽出SQL（この関数で登録されたものを取得想定（登録するDBのキーを変えた際は注意））
        Dim journalSqlStat As New StringBuilder
        journalSqlStat.AppendLine("SELECT ")
        journalSqlStat.AppendLine("             STOCKYMD")
        journalSqlStat.AppendLine("            ,OFFICECODE")
        journalSqlStat.AppendLine("            ,SHIPPERSCODE")
        journalSqlStat.AppendLine("            ,CONSIGNEECODE")
        journalSqlStat.AppendLine("            ,TRAINNO")
        journalSqlStat.AppendLine("            ,ACCYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,RTANK1))  AS RTANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,HTANK1))  AS HTANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,TTANK1))  AS TTANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,MTTANK1)) AS MTTANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,KTANK1))  AS KTANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,K3TANK1)) AS K3TANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,LTANK1))  AS LTANK1")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,ATANK1))  AS ATANK1")

        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,RTANK2))  AS RTANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,HTANK2))  AS HTANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,TTANK2))  AS TTANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,MTTANK2)) AS MTTANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,KTANK2))  AS KTANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,K3TANK2)) AS K3TANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,LTANK2))  AS LTANK2")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,ATANK2))  AS ATANK2")

        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,DELFLG))      AS DELFLG")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITYMD))     AS INITYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITUSER))    AS INITUSER")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,INITTERMID))  AS INITTERMID")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDYMD))      AS UPDYMD")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDUSER))     AS UPDUSER")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,UPDTERMID))   AS UPDTERMID")
        journalSqlStat.AppendLine("            ,convert(nvarchar,isnull(null,RECEIVEYMD))  AS RECEIVEYMD")
        journalSqlStat.AppendLine("  FROM OIL.OIT0009_UKEIREOILSTOCK WITH(nolock)")
        journalSqlStat.AppendLine(" WHERE OFFICECODE    = @OFFICECODE")
        journalSqlStat.AppendLine("   AND SHIPPERSCODE  = @SHIPPERSCODE")
        journalSqlStat.AppendLine("   AND CONSIGNEECODE = @CONSIGNEECODE")
        journalSqlStat.AppendLine("   AND UPDYMD        = @UPDYMD")

        '処理日付引数が初期値なら現時刻設定
        If procDtm.ToString("yyyy/MM/dd").Equals("1900/01/01") Then
            procDtm = Now
        End If

        'トランザクションしない場合は「sqlCon.BeginTransaction」→「nothing」
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            sqlCmd.CommandTimeout = 300
            '固定パラメータ
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispDataClass.SalesOffice
                .Add("@SHIPPERSCODE", SqlDbType.NVarChar).Value = dispDataClass.Shipper
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispDataClass.Consignee

                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@INITYMD", SqlDbType.DateTime).Value = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@INITUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@INITTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = C_DEFAULT_YMD
            End With
            '変動パラメータ
            Dim paramTrainNo = sqlCmd.Parameters.Add("@TRAINNO", SqlDbType.NVarChar)
            Dim paramStockYmd = sqlCmd.Parameters.Add("@STOCKYMD", SqlDbType.Date)
            Dim paramAccYmd = sqlCmd.Parameters.Add("@ACCYMD", SqlDbType.Date)
            Dim paramTrainCntList As New Dictionary(Of String, SqlParameter)
            For Each oilCodeToFieldItm In oilCodeToFieldNameList
                Dim keyField1 As String = oilCodeToFieldItm.Value & "1"
                Dim keyField2 As String = oilCodeToFieldItm.Value & "2"
                paramTrainCntList.Add(keyField1, sqlCmd.Parameters.Add("@" & keyField1, SqlDbType.Int))
                paramTrainCntList.Add(keyField2, sqlCmd.Parameters.Add("@" & keyField2, SqlDbType.Int))
            Next

            '画面データをループしテーブル更新
            '油種別のループ
            Dim miValues As DispDataClass.SuggestItem.SuggestValues = Nothing
            For Each suggestDaysList In qSelected
                paramStockYmd.Value = suggestDaysList.Value.DayInfo.ItemDate.ToString("yyyy/MM/dd")
                '画面表示外の日付は保存対象外
                If suggestDaysList.Value.DayInfo.IsDispArea = False Then
                    Continue For
                End If

                For Each trItm In suggestDaysList.Value.SuggestOrderItem.Values
                    If trItm.AccAddDays <> "" Then
                        paramAccYmd.Value = suggestDaysList.Value.DayInfo.ItemDate.AddDays(CDec(trItm.AccAddDays)).ToString("yyyy/MM/dd")
                    Else
                        paramAccYmd.Value = suggestDaysList.Value.DayInfo.ItemDate.AddDays(trItm.TrainInfo.AccDays).ToString("yyyy/MM/dd")
                    End If
                    miValues = Nothing
                    If trItm.CheckValue = False Then
                        'Continue For 20200529 未チェックでも保存
                    End If
                    paramTrainNo.Value = trItm.TrainInfo.TrainNo
                    If dispDataClass.HasMoveInsideItem Then
                        If dispDataClass.MiDispData.SuggestList.ContainsKey(suggestDaysList.Key) AndAlso
                           dispDataClass.MiDispData.SuggestList(suggestDaysList.Key).SuggestOrderItem.ContainsKey(trItm.TrainInfo.TrainNo) Then
                            With dispDataClass.MiDispData.SuggestList(suggestDaysList.Key)
                                miValues = .SuggestOrderItem(trItm.TrainInfo.TrainNo)
                            End With
                        End If
                    End If
                    '各値を0クリア
                    For Each prmTrainCnt In paramTrainCntList.Values
                        prmTrainCnt.Value = 0
                    Next prmTrainCnt
                    '通常表のループ
                    For Each oilItm In trItm.SuggestValuesItem
                        If oilCodeToFieldNameList.ContainsKey(oilItm.Key) = False Then
                            Continue For
                        End If
                        Dim fieldName As String = oilCodeToFieldNameList(oilItm.Key) & "1"
                        paramTrainCntList(fieldName).Value = CInt(oilItm.Value.ItemValue)
                    Next
                    '構内取り表のループ
                    If dispDataClass.HasMoveInsideItem AndAlso miValues IsNot Nothing Then
                        For Each oilItm In miValues.SuggestValuesItem
                            If oilCodeToFieldNameList.ContainsKey(oilItm.Key) = False Then
                                Continue For
                            End If
                            Dim fieldName As String = oilCodeToFieldNameList(oilItm.Key) & "2"
                            paramTrainCntList(fieldName).Value = CInt(oilItm.Value.ItemValue)
                        Next
                    End If
                    sqlCmd.ExecuteNonQuery()
                Next trItm
            Next suggestDaysList

            'ジャーナル用のデータ取得
            sqlCmd.CommandText = journalSqlStat.ToString
            Using journalDt As New DataTable,
                  SQLdr As SqlDataReader = sqlCmd.ExecuteReader()
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    journalDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                journalDt.Load(SQLdr)
                OutputJournal(journalDt)
                journalDt.Clear()
            End Using 'journalDt,journalDt
        End Using 'tran,sqlCmd
        Return True

    End Function
    ''' <summary>
    ''' 受注テーブル・詳細登録処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dispDataClass"></param>
    ''' <param name="entredOrderList"></param>
    ''' <param name="historyNo">履歴番号</param>
    ''' <returns></returns>
    ''' <remarks>別途他関数で登録・更新・削除フラグ立て処理は実行する</remarks>
    Private Function EntryOrderInfo(ByVal sqlCon As SqlConnection, dispDataClass As DispDataClass, entredOrderList As Dictionary(Of String, OrderItem), historyNo As String, mapId As String, Optional parentDispDataClass As DispDataClass = Nothing) As List(Of EntryOrderResultItm)
        Dim procDtm As Date = Now
        Dim sqlTran As SqlTransaction = Nothing
        Dim checkedItmList = dispDataClass.GetSuggestCheckedItem
        Dim parentConsignee As String = ""

        If parentDispDataClass IsNot Nothing Then
            parentConsignee = parentDispDataClass.Consignee
        End If
        'Dim orderItm As OrderItem = Nothing
        Dim orderEntList As List(Of OrderItem) = Nothing
        Dim retMessage As New List(Of EntryOrderResultItm)
        'チェックした提案情報のループ(日付、列車毎)
        For Each chkItm In checkedItmList
            '**************************
            'オーダー基本情報部の編集
            '**************************
            '既登録データのキー照合
            '第二荷受人を右記条件を廃止し考慮→itm.ConsigneeCode = dispDataClass.Consignee AndAlso
            Dim qOder = (From itm In entredOrderList.Values
                         Where itm.LodDate = chkItm.dayInfo.KeyString AndAlso
                              ((itm.ShippersCode = dispDataClass.Shipper AndAlso
                                itm.JointOrder = "0") OrElse
                               (itm.JointOrder = "1" AndAlso
                               (From detItm In itm.DetailList Where detItm.JointCode = dispDataClass.Shipper).Any)
                              ) AndAlso
                              ((itm.ConsigneeCode = dispDataClass.Consignee AndAlso
                                itm.SecondConsigneeOrder = "0") OrElse
                               (itm.SecondConsigneeOrder = "1" AndAlso
                               (From detItm In itm.DetailList Where detItm.SecondConsigneeCode = dispDataClass.Consignee).Any)
                              ) AndAlso
                              itm.TrainNo = chkItm.trainInfo.TrainNo
                         Order By itm.SecondConsigneeOrder, itm.JointOrder)
            If qOder.Any = False AndAlso parentDispDataClass IsNot Nothing Then
                qOder = (From itm In entredOrderList.Values
                         Where itm.LodDate = chkItm.dayInfo.KeyString AndAlso
                              ((itm.ShippersCode = dispDataClass.Shipper AndAlso
                                itm.JointOrder = "0") OrElse
                               (itm.JointOrder = "1" AndAlso
                               (From detItm In itm.DetailList Where detItm.JointCode = dispDataClass.Shipper).Any)
                              ) AndAlso
                              itm.ConsigneeCode = parentDispDataClass.Consignee AndAlso
                              itm.TrainNo = chkItm.trainInfo.TrainNo
                         Order By itm.SecondConsigneeOrder, itm.JointOrder)
            End If
            Dim qhasParentOrder = (From itm In entredOrderList.Values
                                   Where itm.LodDate = chkItm.dayInfo.KeyString AndAlso
                              ((itm.ShippersCode = dispDataClass.Shipper AndAlso
                                itm.JointOrder = "0") OrElse
                               (itm.JointOrder = "1" AndAlso
                               (From detItm In itm.DetailList Where detItm.JointCode = dispDataClass.Shipper).Any)
                              ) AndAlso
                              itm.ConsigneeCode = parentConsignee AndAlso
                              itm.TrainNo = chkItm.trainInfo.TrainNo
                                   Order By itm.SecondConsigneeOrder, itm.JointOrder)
            '重複オーダーチェック用クエリーJOINT設定を除き同一受入日、荷主、荷受人、列車番号のキーで抽出
            Dim qDupeOder = (From itm In entredOrderList.Values
                             Where itm.LodDate = chkItm.dayInfo.KeyString AndAlso
                              itm.ShippersCode = dispDataClass.Shipper AndAlso
                              itm.ConsigneeCode = dispDataClass.Consignee AndAlso
                              itm.TrainNo = chkItm.trainInfo.TrainNo AndAlso
                              itm.JointOrder = "0" AndAlso
                              itm.SecondConsigneeOrder = "0"
                             )
            '同一受入日、荷主、荷受人、列車番号のキーにて2件以上のオーダーが存在する場合はスキップ
            If qDupeOder.Count >= 2 Then
                '戻り値エラー情報に格納
                retMessage.Add(New EntryOrderResultItm With {
                               .AccDate = chkItm.dayInfo.KeyString,
                               .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName),
                               .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName),
                               .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName),
                               .TrainNo = chkItm.trainInfo.TrainNo,
                               .OrderNo = "(" & String.Join(",", (From itm In qOder Select itm.OrderNo)) & ")",
                               .MessageId = C_MESSAGE_NO.OIL_ORDER_DUPULICATE_ACCDATE_ERROR
                               })
                Continue For
            End If
            '同一受入日、荷主、荷受人、列車番号の既登録データをorderItm変数にセット(1件またはNothing(0件)となる)
            'orderItm = qOder.FirstOrDefault
            orderEntList = qOder.ToList
            '既登録オーダー（受注テーブル）なし、画面入力の車数がすべて0の場合は登録する意味がないのでスキップ
            If (orderEntList Is Nothing OrElse orderEntList.Count = 0) AndAlso
                (From sitm In chkItm.SuggestOrderItem.Values
                 Where CInt(sitm.ItemValue) > 0).Any = False Then

                '戻り値エラー情報に格納（2020/9/29 スルーするよう修正）
                'retMessage.Add(New EntryOrderResultItm With {
                '               .AccDate = chkItm.dayInfo.KeyString,
                '               .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName),
                '               .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName),
                '               .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName),
                '               .TrainNo = chkItm.trainInfo.TrainNo,
                '               .MessageId = C_MESSAGE_NO.OIL_CANNOT_ENTRY_ORDER
                '               })
                Continue For
            End If

            '既登録オーダー（受注テーブル）のステータスが進んでいたら更新させない
            Dim editableOrderStatusList As New List(Of String) From {
                    CONST_ORDERSTATUS_100,
                    CONST_ORDERSTATUS_200, CONST_ORDERSTATUS_210,
                    CONST_ORDERSTATUS_220, CONST_ORDERSTATUS_230,
                    CONST_ORDERSTATUS_240, CONST_ORDERSTATUS_250,
                    CONST_ORDERSTATUS_260, CONST_ORDERSTATUS_270,
                    CONST_ORDERSTATUS_280, CONST_ORDERSTATUS_290,
                    CONST_ORDERSTATUS_300, CONST_ORDERSTATUS_310
                }
            If (orderEntList IsNot Nothing AndAlso orderEntList.Count > 0) AndAlso
               Not (From orderItm In orderEntList Where editableOrderStatusList.Contains(orderItm.OrderStatus)).Any Then
                '戻り値エラー情報に格納
                Dim noEditableOrderNo As String = (From orderItm In orderEntList Where editableOrderStatusList.Contains(orderItm.OrderStatus) Select orderItm.OrderNo).FirstOrDefault
                retMessage.Add(New EntryOrderResultItm With {
                               .AccDate = chkItm.dayInfo.KeyString,
                               .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName),
                               .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName),
                               .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName),
                               .TrainNo = chkItm.trainInfo.TrainNo,
                               .OrderNo = noEditableOrderNo,
                               .MessageId = C_MESSAGE_NO.OIL_THIS_ORDER_STATUS_ISNOT_PROC
                               })
                Continue For
            End If '既登録オーダーステータスチェック End If
            '(コスモ時、既登録オーダーがある場合)
            '削除時オーダー（受注テーブル）同期させず且つ1つでも油種が減った場合、すべての油種を処理させない、受入日、列車番号は処理しない
            If (orderEntList IsNot Nothing AndAlso orderEntList.Count > 0) AndAlso dispDataClass.AsyncDeleteShipper = True Then
                Dim hasDecrementTrainCnt As Boolean
                Dim decOilCode As String = ""
                Dim decOilName As String = ""
                Dim decOrderNo As String = ""
                Dim decDetailNo As String = ""
                hasDecrementTrainCnt = False
                For Each chkOilVal In chkItm.SuggestOrderItem
                    Dim qOrderDetailOilCount As List(Of OrderDetailItem)
                    qOrderDetailOilCount = New List(Of OrderDetailItem)
                    For Each odrItm In orderEntList
                        Dim qTmp = (From detItm In odrItm.DetailList Where detItm.OilCode = chkOilVal.Key)
                        If qTmp.Any Then
                            qOrderDetailOilCount.AddRange(qTmp)
                        End If
                    Next
                    '受注テーブルの車数と画面入力の車数を比較
                    If qOrderDetailOilCount.Count > CInt(chkOilVal.Value.ItemValue) Then
                        '車数減少を検知したためフラグを耐えてチェックを抜ける
                        decOilCode = chkOilVal.Value.OilInfo.OilName & "(" & chkOilVal.Value.OilInfo.OilCode & ")"
                        decOrderNo = qOrderDetailOilCount(0).OrderNo
                        decDetailNo = qOrderDetailOilCount(0).DetailNo
                        hasDecrementTrainCnt = True
                        Exit For
                    End If
                Next chkOilVal
                '車数減少の場合
                If hasDecrementTrainCnt Then
                    '戻り値エラー情報に格納
                    retMessage.Add(New EntryOrderResultItm With {
                                   .AccDate = chkItm.dayInfo.KeyString,
                                   .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName),
                                   .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName),
                                   .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName),
                                   .TrainNo = chkItm.trainInfo.TrainNo,
                                   .OilCode = decOilCode,
                                   .OrderNo = decOrderNo,
                                   .DetailNo = decDetailNo,
                                   .MessageId = C_MESSAGE_NO.OIL_ASYNC_DELETE_SHIPPER
                                   })
                    Continue For '次の列車・日付キーへスキップ
                End If　'車数減少の場合 End If
            End If 'コスモ時、既登録オーダーがある場合 End If

            '既登録データが無い場合または、ジョイントオーダーのみの場合はオーダー情報を新規生成
            If (orderEntList Is Nothing OrElse orderEntList.Count = 0) OrElse
               ((From orderitm In orderEntList Where orderitm.JointOrder = "0").Any = False
               ) Then
                Dim entryResult As EntryOrderResultItm = Nothing
                'オーダー番号の取得
                Dim orderNo As String = GetNewOrderNo(sqlCon, entryResult)
                'ありえないが新規オーダーNoが取得できない場合はエラーメッセージを設定しスキップ
                If orderNo = "" Then
                    If entryResult Is Nothing Then
                        entryResult = New EntryOrderResultItm With {
                        .MessageId = C_MESSAGE_NO.OIL_CANNOT_GET_NEW_ORDERNO,
                        .Message = "NEWORDERNOGET"
                        }
                    End If
                    With entryResult
                        .AccDate = chkItm.dayInfo.KeyString
                        .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName)
                        .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName)
                        .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName)
                        .TrainNo = chkItm.trainInfo.TrainNo
                    End With
                    retMessage.Add(entryResult)
                    Continue For '次の列車・日付キーへスキップ
                End If 'オーダー番号、未取得時

                '受注情報の車数を除く基本情報の生成
                Dim orderItm = New OrderItem(orderNo, dispDataClass, chkItm, procDtm, Master.USERID, Master.USERTERMID)
                If orderEntList Is Nothing Then
                    orderEntList = New List(Of OrderItem)
                End If
                If parentDispDataClass IsNot Nothing Then
                    orderItm = New OrderItem(orderNo, parentDispDataClass, chkItm, procDtm, Master.USERID, Master.USERTERMID)
                End If
                orderEntList.Add(orderItm)

            End If　'既登録データなし End If
            '**************************
            'オーダー詳細情報部の編集
            '**************************
            '画面上の日付・列車固定の油種部分をループ
            For Each chkOilVal In chkItm.SuggestOrderItem
                '同一油種のデータを取得(※消す際の順番はこのリストのorder byの先頭から、消す順序を制御する場合はこのorder Byの操作をする)
                Dim qOrderDetailOil As New List(Of OrderDetailItem)
                For Each orderItm In (From tmpOdrItm In orderEntList Order By tmpOdrItm.JointOrder)
                    Dim qTmp = (From detItm In orderItm.DetailList Where detItm.OilCode = chkOilVal.Key Order By detItm.DetailNo)
                    If qTmp.Any Then
                        qOrderDetailOil.AddRange(qTmp)
                    End If

                Next

                '何回もqOrderDetailOil(LINQ)のカウントを取ると効率が悪いので変数に一旦格納
                Dim detailTrainCnt = qOrderDetailOil.Count
                'オーダー詳細テーブル既登録の車数と画面上の車数が同値なら特に操作をしないで次の油種へ
                If detailTrainCnt = CInt(chkOilVal.Value.ItemValue) Then
                    Continue For
                End If '未変更用

                'オーダー詳細テーブル既登録の車数と画面上の車数につき
                '画面上の油種が増えていた場合は追加
                If detailTrainCnt < CInt(chkOilVal.Value.ItemValue) Then
                    Dim orderDetItem As OrderDetailItem
                    Dim orderItm = (From tmpOdrItm In orderEntList Where tmpOdrItm.JointOrder = "0").FirstOrDefault
                    '最後にアップデートでカウント書き換える為不要 orderItm.TRCount(chkOilVal.Key) = chkOilVal.Value.ItemValue
                    If orderItm.EntryType = OrderItem.OrderItemEntryType.None Then
                        orderItm.EntryType = OrderItem.OrderItemEntryType.Update
                        orderItm.UpdUser = Master.USERID
                        orderItm.UpdYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                        orderItm.UpdTermId = Master.USERTERMID
                        orderItm.ReceiveYmd = CONST_DEFAULT_RECEIVEYMD
                    End If
                    '油種増分数
                    Dim incrimentCnt = CInt(chkOilVal.Value.ItemValue) - detailTrainCnt
                    Dim curDetailInt = CInt(orderItm.MaxDetailNo)
                    For i = 1 To incrimentCnt
                        curDetailInt = curDetailInt + 1
                        orderDetItem = New OrderDetailItem(orderItm, curDetailInt.ToString("000"),
                                                           dispDataClass, chkOilVal.Value, procDtm, Master.USERID, Master.USERTERMID)
                        If parentDispDataClass IsNot Nothing Then
                            orderDetItem.SecondConsigneeCode = dispDataClass.Consignee
                            orderDetItem.SecondConsigneeName = dispDataClass.ConsigneeName
                        End If
                        orderItm.DetailList.Add(orderDetItem)
                    Next i
                    orderItm.MaxDetailNo = curDetailInt.ToString("000")
                End If '新規追加オーダー詳細増幅
                'オーダー詳細テーブル既登録の車数と画面上の車数につき
                '画面上の油種が減っていた場合は削除フラグ
                If detailTrainCnt > CInt(chkOilVal.Value.ItemValue) Then

                    Dim delCnt As Integer = detailTrainCnt - CInt(chkOilVal.Value.ItemValue)
                    For Each oderDetailItm In qOrderDetailOil
                        If delCnt <= 0 Then
                            Exit For
                        End If
                        '処理種別を None(何もしない)からDelete(論理削除)に変更
                        oderDetailItm.EntryType = OrderDetailItem.DetailEntryType.Delete
                        oderDetailItm.DelFlg = C_DELETE_FLG.DELETE
                        oderDetailItm.UpdUser = Master.USERID
                        oderDetailItm.UpdYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                        oderDetailItm.UpdTermId = Master.USERTERMID
                        delCnt = delCnt - 1
                        '最後にアップデートでカウント書き換える為不要 orderItm.TRCount(chkOilVal.Key) = chkOilVal.Value.ItemValue
                        Dim orderItm = (From tmpOrderItm In orderEntList Where tmpOrderItm.OrderNo = oderDetailItm.OrderNo).FirstOrDefault
                        If orderItm.EntryType = OrderItem.OrderItemEntryType.None Then
                            orderItm.EntryType = OrderItem.OrderItemEntryType.Update
                            orderItm.UpdUser = Master.USERID
                            orderItm.UpdYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                            orderItm.UpdTermId = Master.USERTERMID
                            orderItm.ReceiveYmd = CONST_DEFAULT_RECEIVEYMD
                        End If
                    Next oderDetailItm
                End If
            Next chkOilVal '画面上の日付・列車固定の油種部分をループ End
            '***********************************************
            'オーダー詳細情報部に更新対象があるかチェック
            '***********************************************
            '受注詳細でEntryType=None以外が存在のしない場合、DB登録の意味がないのでスキップ
            If Not (From tmpOdrItm In orderEntList Where (From dtlitm In tmpOdrItm.DetailList Where dtlitm.EntryType <> OrderDetailItem.DetailEntryType.None).Any).Any Then
                'retMessage.Add(New EntryOrderResultItm With {
                '                   .AccDate = chkItm.dayInfo.KeyString,
                '                   .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName),
                '                   .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName),
                '                   .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName),
                '                   .TrainNo = chkItm.trainInfo.TrainNo,
                '                   .MessageId = C_MESSAGE_NO.OIL_CANNOT_ENTRY_ORDER
                '                   })
                Continue For
            End If
            '***********************************************
            '更新処理実行
            '***********************************************
            Dim detailNo As String = ""
            Dim odrNo As String = ""
            Try
                detailNo = ""
                odrNo = ""

                '日付列車単位でのトランザクション
                Using tran = sqlCon.BeginTransaction
                    For Each orderItm In orderEntList
                        odrNo = orderItm.OrderNo
                        'オーダー基本部
                        If orderItm.EntryType = OrderItem.OrderItemEntryType.Insert Then
                            InsertOrder(sqlCon, tran, orderItm)
                        ElseIf orderItm.EntryType = OrderItem.OrderItemEntryType.Update Then
                            UpdateOrder(sqlCon, tran, orderItm)
                        Else
                            Continue For
                        End If
                        '履歴登録（オーダー基本部）
                        If {OrderItem.OrderItemEntryType.Insert, OrderItem.OrderItemEntryType.Update}.Contains(orderItm.EntryType) Then
                            Try
                                EntryHistory.InsertOrderHistory(sqlCon, tran, orderItm.ToHistoryDataTable(historyNo, mapId).Rows(0))
                            Catch ex As Exception
                                If parentDispDataClass Is Nothing Then
                                    Throw
                                End If
                            End Try
                        End If
                        'オーダー詳細部ループ
                        For Each detailItm In orderItm.DetailList
                            detailNo = detailItm.DetailNo
                            If detailItm.EntryType = OrderDetailItem.DetailEntryType.Insert Then
                                InsertOrderDetail(sqlCon, tran, detailItm)
                            ElseIf detailItm.EntryType = OrderDetailItem.DetailEntryType.Delete Then
                                DeleteOrderDetail(sqlCon, tran, detailItm)
                            Else
                                Continue For
                            End If
                            '履歴登録（オーダー詳細部）
                            EntryHistory.InsertOrderDetailHistory(sqlCon, tran, detailItm.ToHistoryDataTable(historyNo, mapId).Rows(0))
                        Next 'detailItm
                    Next 'orderItm
                    'トランザクションコミット
                    tran.Commit()
                    'カウントの更新
                    For Each orderItm In orderEntList
                        UpdateOrderTrainNum(sqlCon, Nothing, orderItm)
                    Next
                End Using 'tran
            Catch ex As Exception
                retMessage.Add(New EntryOrderResultItm With {
                   .AccDate = chkItm.dayInfo.KeyString,
                   .OfficeCode = String.Format("{1}({0})", dispDataClass.SalesOffice, dispDataClass.SalesOfficeName),
                   .ShipperCode = String.Format("{1}({0})", dispDataClass.Shipper, dispDataClass.ShipperName),
                   .ConsigneeCode = String.Format("{1}({0})", dispDataClass.Consignee, dispDataClass.ConsigneeName),
                   .TrainNo = chkItm.trainInfo.TrainNo,
                   .OrderNo = odrNo,
                   .DetailNo = detailNo,
                   .MessageId = C_MESSAGE_NO.DB_ERROR, '一旦このコード
                   .StackTrace = ex.ToString
                   })
                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:OIT0004C ORDERUPDATE"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                Continue For
            End Try
        Next chkItm '日付、列車毎ループ終了
        Return retMessage '処理結果メッセージを返却(0件なら完全に正常想定)
    End Function
    ''' <summary>
    ''' 受注No(新規用)取得
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <returns></returns>
    Private Function GetNewOrderNo(ByVal sqlCon As SqlConnection, ByRef errMes As EntryOrderResultItm) As String
        errMes = Nothing
        Dim retVal As String = ""
        Try
            Dim sqlStr As New StringBuilder
            sqlStr.AppendLine("SELECT FX.KEYCODE  AS ORDERNO")
            sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
            sqlStr.AppendLine(" WHERE FX.CLASS    = @CLASS")
            sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar).Value = "NEWORDERNOGET"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows Then
                        sqlDr.Read()
                        retVal = Convert.ToString(sqlDr("ORDERNO"))
                    Else
                        '取得できないと後続処理ができないのでエラー扱い
                        errMes = New EntryOrderResultItm
                        errMes.MessageId = C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR
                        errMes.Message = "NEWORDERNOGET"
                    End If
                End Using 'sqlDr
            End Using
        Catch ex As Exception
            errMes = New EntryOrderResultItm
            errMes.MessageId = C_MESSAGE_NO.DB_ERROR
            errMes.Message = "GetNewOrderNo Error"
            errMes.StackTrace = ex.ToString()

            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0004C MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0004C MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
        Return retVal
    End Function
    ''' <summary>
    ''' オーダー基本情報追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="orderItm"></param>
    Private Sub InsertOrder(sqlCon As SqlConnection, sqlTran As SqlTransaction, orderItm As OrderItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO OIL.OIT0002_ORDER")
        sqlStat.AppendLine("   (ORDERNO,TRAINNO,TRAINNAME,ORDERYMD,OFFICECODE,OFFICENAME,ORDERTYPE,")
        sqlStat.AppendLine("    SHIPPERSCODE,SHIPPERSNAME,BASECODE,BASENAME,CONSIGNEECODE,CONSIGNEENAME,")
        sqlStat.AppendLine("    DEPSTATION,DEPSTATIONNAME,ARRSTATION,ARRSTATIONNAME,RETSTATION,RETSTATIONNAME,")
        sqlStat.AppendLine("    CHANGERETSTATION,CHANGERETSTATIONNAME,ORDERSTATUS,ORDERINFO,EMPTYTURNFLG,STACKINGFLG,USEPROPRIETYFLG,CONTACTFLG,RESULTFLG,DELIVERYFLG,DELIVERYCOUNT,")
        sqlStat.AppendLine("    LODDATE,DEPDATE,ARRDATE,ACCDATE,EMPARRDATE,ACTUALLODDATE,ACTUALDEPDATE,ACTUALARRDATE,ACTUALACCDATE,ACTUALEMPARRDATE,")
        sqlStat.AppendLine("    RTANK,HTANK,TTANK,MTTANK,KTANK,K3TANK,K5TANK,K10TANK,LTANK,ATANK,")
        sqlStat.AppendLine("    OTHER1OTANK,OTHER2OTANK,OTHER3OTANK,OTHER4OTANK,OTHER5OTANK,")
        sqlStat.AppendLine("    OTHER6OTANK,OTHER7OTANK,OTHER8OTANK,OTHER9OTANK,OTHER10OTANK,")
        sqlStat.AppendLine("    TOTALTANK,")
        sqlStat.AppendLine("    RTANKCH,HTANKCH,TTANKCH,MTTANKCH,KTANKCH,K3TANKCH,K5TANKCH,K10TANKCH,LTANKCH,ATANKCH,")
        sqlStat.AppendLine("    OTHER1OTANKCH,OTHER2OTANKCH,OTHER3OTANKCH,OTHER4OTANKCH,OTHER5OTANKCH,")
        sqlStat.AppendLine("    OTHER6OTANKCH,OTHER7OTANKCH,OTHER8OTANKCH,OTHER9OTANKCH,OTHER10OTANKCH,")
        sqlStat.AppendLine("    TOTALTANKCH,TANKLINKNO,TANKLINKNOMADE,BILLINGNO,KEIJYOYMD,")
        sqlStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,OTFILENAME,RECEIVECOUNT,OTSENDSTATUS,RESERVEDSTATUS,TAKUSOUSTATUS,BTRAINNO,BTRAINNAME,ANASYORIFLG,")
        sqlStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD)")
        sqlStat.AppendLine("    VALUES")
        sqlStat.AppendLine("   (@ORDERNO,@TRAINNO,@TRAINNAME,@ORDERYMD,@OFFICECODE,@OFFICENAME,@ORDERTYPE,")
        sqlStat.AppendLine("    @SHIPPERSCODE,@SHIPPERSNAME,@BASECODE,@BASENAME,@CONSIGNEECODE,@CONSIGNEENAME,")
        sqlStat.AppendLine("    @DEPSTATION,@DEPSTATIONNAME,@ARRSTATION,@ARRSTATIONNAME,@RETSTATION,@RETSTATIONNAME,")
        sqlStat.AppendLine("    @CHANGERETSTATION,@CHANGERETSTATIONNAME,@ORDERSTATUS,@ORDERINFO,@EMPTYTURNFLG,@STACKINGFLG,@USEPROPRIETYFLG,@CONTACTFLG,@RESULTFLG,@DELIVERYFLG,@DELIVERYCOUNT,")
        sqlStat.AppendLine("    @LODDATE,@DEPDATE,@ARRDATE,@ACCDATE,@EMPARRDATE,@ACTUALLODDATE,@ACTUALDEPDATE,@ACTUALARRDATE,@ACTUALACCDATE,@ACTUALEMPARRDATE,")
        sqlStat.AppendLine("    @RTANK,@HTANK,@TTANK,@MTTANK,@KTANK,@K3TANK,@K5TANK,@K10TANK,@LTANK,@ATANK,")
        sqlStat.AppendLine("    @OTHER1OTANK,@OTHER2OTANK,@OTHER3OTANK,@OTHER4OTANK,@OTHER5OTANK,")
        sqlStat.AppendLine("    @OTHER6OTANK,@OTHER7OTANK,@OTHER8OTANK,@OTHER9OTANK,@OTHER10OTANK,")
        sqlStat.AppendLine("    @TOTALTANK,")
        sqlStat.AppendLine("    @RTANKCH,@HTANKCH,@TTANKCH,@MTTANKCH,@KTANKCH,@K3TANKCH,@K5TANKCH,@K10TANKCH,@LTANKCH,@ATANKCH,")
        sqlStat.AppendLine("    @OTHER1OTANKCH,@OTHER2OTANKCH,@OTHER3OTANKCH,@OTHER4OTANKCH,@OTHER5OTANKCH,")
        sqlStat.AppendLine("    @OTHER6OTANKCH,@OTHER7OTANKCH,@OTHER8OTANKCH,@OTHER9OTANKCH,@OTHER10OTANKCH,")
        sqlStat.AppendLine("    @TOTALTANKCH,@TANKLINKNO,@TANKLINKNOMADE,@BILLINGNO,@KEIJYOYMD,")
        sqlStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,@OTFILENAME,@RECEIVECOUNT,@OTSENDSTATUS,@RESERVEDSTATUS,@TAKUSOUSTATUS,@BTRAINNO,@BTRAINNAME,@ANASYORIFLG,")
        sqlStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD)")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("ORDERNO", SqlDbType.NVarChar).Value = orderItm.OrderNo
                .Add("TRAINNO", SqlDbType.NVarChar).Value = orderItm.TrainNo
                .Add("TRAINNAME", SqlDbType.NVarChar).Value = orderItm.TrainName
                .Add("ORDERYMD", SqlDbType.Date).Value = orderItm.OrderYmd
                .Add("OFFICECODE", SqlDbType.NVarChar).Value = orderItm.OfficeCode
                .Add("OFFICENAME", SqlDbType.NVarChar).Value = orderItm.OfficeName
                .Add("ORDERTYPE", SqlDbType.NVarChar).Value = orderItm.OrderType
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = orderItm.ShippersCode
                .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = orderItm.ShippersName
                .Add("BASECODE", SqlDbType.NVarChar).Value = orderItm.BaseCode
                .Add("BASENAME", SqlDbType.NVarChar).Value = orderItm.BaseName
                .Add("CONSIGNEECODE", SqlDbType.NVarChar).Value = orderItm.ConsigneeCode
                .Add("CONSIGNEENAME", SqlDbType.NVarChar).Value = orderItm.ConsigneeName
                .Add("DEPSTATION", SqlDbType.NVarChar).Value = orderItm.DepStation
                .Add("DEPSTATIONNAME", SqlDbType.NVarChar).Value = orderItm.DepStationName
                .Add("ARRSTATION", SqlDbType.NVarChar).Value = orderItm.ArrStation
                .Add("ARRSTATIONNAME", SqlDbType.NVarChar).Value = orderItm.ArrStationName
                .Add("RETSTATION", SqlDbType.NVarChar).Value = orderItm.RetStation
                .Add("RETSTATIONNAME", SqlDbType.NVarChar).Value = orderItm.RetStationName
                .Add("CHANGERETSTATION", SqlDbType.NVarChar).Value = orderItm.ChangeRetStation
                .Add("CHANGERETSTATIONNAME", SqlDbType.NVarChar).Value = orderItm.ChangeRetStationName
                .Add("ORDERSTATUS", SqlDbType.NVarChar).Value = orderItm.OrderStatus
                .Add("ORDERINFO", SqlDbType.NVarChar).Value = orderItm.OrderInfo
                .Add("EMPTYTURNFLG", SqlDbType.NVarChar).Value = orderItm.EmptyTurnFlg
                .Add("STACKINGFLG", SqlDbType.NVarChar).Value = orderItm.StackingFlg
                .Add("USEPROPRIETYFLG", SqlDbType.NVarChar).Value = orderItm.UseProprietyFlg
                .Add("CONTACTFLG", SqlDbType.NVarChar).Value = orderItm.ContactFlg
                .Add("RESULTFLG", SqlDbType.NVarChar).Value = orderItm.ResultFlg
                .Add("DELIVERYFLG", SqlDbType.NVarChar).Value = orderItm.DeliveryFlg
                .Add("DELIVERYCOUNT", SqlDbType.NVarChar).Value = orderItm.DeliveryCount
                .Add("LODDATE", SqlDbType.Date).Value = orderItm.LodDate
                .Add("DEPDATE", SqlDbType.Date).Value = orderItm.DepDate
                .Add("ARRDATE", SqlDbType.Date).Value = orderItm.ArrDate
                .Add("ACCDATE", SqlDbType.Date).Value = orderItm.AccDate
                .Add("EMPARRDATE", SqlDbType.Date).Value = orderItm.EmpArrDate
                .Add("ACTUALLODDATE", SqlDbType.Date).Value = If(orderItm.ActualLodDate = "", CType(DBNull.Value, Object), orderItm.ActualLodDate)
                .Add("ACTUALDEPDATE", SqlDbType.Date).Value = If(orderItm.ActualDepDate = "", CType(DBNull.Value, Object), orderItm.ActualDepDate)
                .Add("ACTUALARRDATE", SqlDbType.Date).Value = If(orderItm.ActualArrDate = "", CType(DBNull.Value, Object), orderItm.ActualArrDate)
                .Add("ACTUALACCDATE", SqlDbType.Date).Value = If(orderItm.ActualAccDate = "", CType(DBNull.Value, Object), orderItm.ActualAccDate)
                .Add("ACTUALEMPARRDATE", SqlDbType.Date).Value = If(orderItm.ActualEmpArrDate = "", CType(DBNull.Value, Object), orderItm.ActualEmpArrDate)
                .Add("RTANK", SqlDbType.Int).Value = orderItm.RTank
                .Add("HTANK", SqlDbType.Int).Value = orderItm.HTank
                .Add("TTANK", SqlDbType.Int).Value = orderItm.TTank
                .Add("MTTANK", SqlDbType.Int).Value = orderItm.MTtank
                .Add("KTANK", SqlDbType.Int).Value = orderItm.KTank
                .Add("K3TANK", SqlDbType.Int).Value = orderItm.K3Tank
                .Add("K5TANK", SqlDbType.Int).Value = orderItm.K5Tank
                .Add("K10TANK", SqlDbType.Int).Value = orderItm.K10Tank
                .Add("LTANK", SqlDbType.Int).Value = orderItm.LTank
                .Add("ATANK", SqlDbType.Int).Value = orderItm.ATank
                .Add("OTHER1OTANK", SqlDbType.Int).Value = orderItm.Other1Otank
                .Add("OTHER2OTANK", SqlDbType.Int).Value = orderItm.Other2OTank
                .Add("OTHER3OTANK", SqlDbType.Int).Value = orderItm.Other3OTank
                .Add("OTHER4OTANK", SqlDbType.Int).Value = orderItm.Other4OTank
                .Add("OTHER5OTANK", SqlDbType.Int).Value = orderItm.Other5OTank
                .Add("OTHER6OTANK", SqlDbType.Int).Value = orderItm.Other6OTank
                .Add("OTHER7OTANK", SqlDbType.Int).Value = orderItm.Other7OTank
                .Add("OTHER8OTANK", SqlDbType.Int).Value = orderItm.Other8OTank
                .Add("OTHER9OTANK", SqlDbType.Int).Value = orderItm.Other9OTank
                .Add("OTHER10OTANK", SqlDbType.Int).Value = orderItm.Other10OTank
                .Add("TOTALTANK", SqlDbType.Int).Value = orderItm.TotalTank
                .Add("RTANKCH", SqlDbType.Int).Value = orderItm.RTankCh
                .Add("HTANKCH", SqlDbType.Int).Value = orderItm.HTankCh
                .Add("TTANKCH", SqlDbType.Int).Value = orderItm.TTankCh
                .Add("MTTANKCH", SqlDbType.Int).Value = orderItm.MtTankCh
                .Add("KTANKCH", SqlDbType.Int).Value = orderItm.KTankCh
                .Add("K3TANKCH", SqlDbType.Int).Value = orderItm.K3TankCh
                .Add("K5TANKCH", SqlDbType.Int).Value = orderItm.K5TankCh
                .Add("K10TANKCH", SqlDbType.Int).Value = orderItm.K10TankCh
                .Add("LTANKCH", SqlDbType.Int).Value = orderItm.LTankCh
                .Add("ATANKCH", SqlDbType.Int).Value = orderItm.ATankCh
                .Add("OTHER1OTANKCH", SqlDbType.Int).Value = orderItm.Other1OTankCh
                .Add("OTHER2OTANKCH", SqlDbType.Int).Value = orderItm.Other2OTankCh
                .Add("OTHER3OTANKCH", SqlDbType.Int).Value = orderItm.Other3OTankCh
                .Add("OTHER4OTANKCH", SqlDbType.Int).Value = orderItm.Other4OTankCh
                .Add("OTHER5OTANKCH", SqlDbType.Int).Value = orderItm.Other5OTankCh
                .Add("OTHER6OTANKCH", SqlDbType.Int).Value = orderItm.Other6OTankCh
                .Add("OTHER7OTANKCH", SqlDbType.Int).Value = orderItm.Other7OTankCh
                .Add("OTHER8OTANKCH", SqlDbType.Int).Value = orderItm.Other8OTankCh
                .Add("OTHER9OTANKCH", SqlDbType.Int).Value = orderItm.Other9OTankCh
                .Add("OTHER10OTANKCH", SqlDbType.Int).Value = orderItm.Other10OTankCh
                .Add("TOTALTANKCH", SqlDbType.Int).Value = orderItm.TotalTankCh
                .Add("TANKLINKNO", SqlDbType.NVarChar).Value = orderItm.TankLinkNo
                .Add("TANKLINKNOMADE", SqlDbType.NVarChar).Value = orderItm.TankLinkNoMade
                .Add("BILLINGNO", SqlDbType.NVarChar).Value = orderItm.BILLINGNO
                .Add("KEIJYOYMD", SqlDbType.Date).Value = If(orderItm.KeijyoYmd = "", CType(DBNull.Value, Object), orderItm.KeijyoYmd)
                .Add("SALSE", SqlDbType.Int).Value = orderItm.Salse
                .Add("SALSETAX", SqlDbType.Int).Value = orderItm.SalseTax
                .Add("TOTALSALSE", SqlDbType.Int).Value = orderItm.TotalSalse
                .Add("PAYMENT", SqlDbType.Int).Value = orderItm.Payment
                .Add("PAYMENTTAX", SqlDbType.Int).Value = orderItm.PaymentTax
                .Add("TOTALPAYMENT", SqlDbType.Int).Value = orderItm.TotalPayment
                .Add("OTFILENAME", SqlDbType.NVarChar).Value = orderItm.OtFileName
                .Add("RECEIVECOUNT", SqlDbType.Int).Value = If(orderItm.ReceiveCount = "", CType(DBNull.Value, Object), orderItm.ReceiveCount)
                .Add("OTSENDSTATUS", SqlDbType.NVarChar).Value = orderItm.OtSendStatus
                .Add("RESERVEDSTATUS", SqlDbType.NVarChar).Value = orderItm.ReservedStatus
                .Add("TAKUSOUSTATUS", SqlDbType.NVarChar).Value = orderItm.TakusouStatus
                .Add("BTRAINNO", SqlDbType.NVarChar).Value = orderItm.BTrainNo
                .Add("BTRAINNAME", SqlDbType.NVarChar).Value = orderItm.BTrainName
                .Add("ANASYORIFLG", SqlDbType.NVarChar).Value = orderItm.AnaSyoriFlg
                .Add("DELFLG", SqlDbType.NVarChar).Value = orderItm.DelFlg
                .Add("INITYMD", SqlDbType.DateTime).Value = orderItm.InitYmd
                .Add("INITUSER", SqlDbType.NVarChar).Value = orderItm.InitUser
                .Add("INITTERMID", SqlDbType.NVarChar).Value = orderItm.InitTermId
                .Add("UPDYMD", SqlDbType.DateTime).Value = orderItm.UpdYmd
                .Add("UPDUSER", SqlDbType.NVarChar).Value = orderItm.UpdUser
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = orderItm.UpdTermId
                .Add("RECEIVEYMD", SqlDbType.DateTime).Value = orderItm.ReceiveYmd
            End With
            sqlCmd.CommandTimeout = 300
            sqlCmd.ExecuteNonQuery()
        End Using
        CS0020JOURNAL.TABLENM = "OIT0002_ORDER"
        CS0020JOURNAL.ACTION = "INSERT"
        CS0020JOURNAL.ROW = orderItm.ToDataTable.Rows(0)
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
    ''' <summary>
    ''' オーダー基本情報更新処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="orderItm"></param>
    Private Sub UpdateOrder(sqlCon As SqlConnection, sqlTran As SqlTransaction, orderItm As OrderItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE OIL.OIT0002_ORDER")
        sqlStat.AppendLine("   SET  RTANK        = @RTANK")
        sqlStat.AppendLine("       ,HTANK        = @HTANK")
        sqlStat.AppendLine("       ,TTANK        = @TTANK")
        sqlStat.AppendLine("       ,MTTANK       = @MTTANK")
        sqlStat.AppendLine("       ,KTANK        = @KTANK")
        sqlStat.AppendLine("       ,K3TANK       = @K3TANK")
        sqlStat.AppendLine("       ,K5TANK       = @K5TANK")
        sqlStat.AppendLine("       ,K10TANK      = @K10TANK")
        sqlStat.AppendLine("       ,LTANK        = @LTANK")
        sqlStat.AppendLine("       ,ATANK        = @ATANK")
        sqlStat.AppendLine("       ,OTHER1OTANK  = @OTHER1OTANK")
        sqlStat.AppendLine("       ,OTHER2OTANK  = @OTHER2OTANK")
        sqlStat.AppendLine("       ,OTHER3OTANK  = @OTHER3OTANK")
        sqlStat.AppendLine("       ,OTHER4OTANK  = @OTHER4OTANK")
        sqlStat.AppendLine("       ,OTHER5OTANK  = @OTHER5OTANK")
        sqlStat.AppendLine("       ,OTHER6OTANK  = @OTHER6OTANK")
        sqlStat.AppendLine("       ,OTHER7OTANK  = @OTHER7OTANK")
        sqlStat.AppendLine("       ,OTHER8OTANK  = @OTHER8OTANK")
        sqlStat.AppendLine("       ,OTHER9OTANK  = @OTHER9OTANK")
        sqlStat.AppendLine("       ,OTHER10OTANK = @OTHER10OTANK")
        sqlStat.AppendLine("       ,TOTALTANK    = @TOTALTANK")
        sqlStat.AppendLine("       ,UPDYMD       = @UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER      = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID    = @UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD   = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("ORDERNO", SqlDbType.NVarChar).Value = orderItm.OrderNo
                .Add("RTANK", SqlDbType.Int).Value = orderItm.RTank
                .Add("HTANK", SqlDbType.Int).Value = orderItm.HTank
                .Add("TTANK", SqlDbType.Int).Value = orderItm.TTank
                .Add("MTTANK", SqlDbType.Int).Value = orderItm.MTtank
                .Add("KTANK", SqlDbType.Int).Value = orderItm.KTank
                .Add("K3TANK", SqlDbType.Int).Value = orderItm.K3Tank
                .Add("K5TANK", SqlDbType.Int).Value = orderItm.K5Tank
                .Add("K10TANK", SqlDbType.Int).Value = orderItm.K10Tank
                .Add("LTANK", SqlDbType.Int).Value = orderItm.LTank
                .Add("ATANK", SqlDbType.Int).Value = orderItm.ATank
                .Add("OTHER1OTANK", SqlDbType.Int).Value = orderItm.Other1Otank
                .Add("OTHER2OTANK", SqlDbType.Int).Value = orderItm.Other2OTank
                .Add("OTHER3OTANK", SqlDbType.Int).Value = orderItm.Other3OTank
                .Add("OTHER4OTANK", SqlDbType.Int).Value = orderItm.Other4OTank
                .Add("OTHER5OTANK", SqlDbType.Int).Value = orderItm.Other5OTank
                .Add("OTHER6OTANK", SqlDbType.Int).Value = orderItm.Other6OTank
                .Add("OTHER7OTANK", SqlDbType.Int).Value = orderItm.Other7OTank
                .Add("OTHER8OTANK", SqlDbType.Int).Value = orderItm.Other8OTank
                .Add("OTHER9OTANK", SqlDbType.Int).Value = orderItm.Other9OTank
                .Add("OTHER10OTANK", SqlDbType.Int).Value = orderItm.Other10OTank
                .Add("TOTALTANK", SqlDbType.Int).Value = orderItm.TotalTank
                .Add("UPDYMD", SqlDbType.DateTime).Value = orderItm.UpdYmd
                .Add("UPDUSER", SqlDbType.NVarChar).Value = orderItm.UpdUser
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = orderItm.UpdTermId
                .Add("RECEIVEYMD", SqlDbType.DateTime).Value = orderItm.ReceiveYmd
            End With
            sqlCmd.CommandTimeout = 300
            sqlCmd.ExecuteNonQuery()
        End Using
        CS0020JOURNAL.TABLENM = "OIT0002_ORDER"
        CS0020JOURNAL.ACTION = "UPDATE"
        CS0020JOURNAL.ROW = orderItm.ToDataTable.Rows(0)
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
    ''' <summary>
    ''' オーダー基本情報更新処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="orderItm"></param>
    Private Sub UpdateOrderTrainNum(sqlCon As SqlConnection, sqlTran As SqlTransaction, orderItm As OrderItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("WITH w_countdetail AS (")
        sqlStat.AppendLine("     SELECT OILCODE ")
        sqlStat.AppendLine("           ,isnull(SUM(CARSNUMBER),0) AS CARSNUMBER")
        sqlStat.AppendLine("       FROM OIL.OIT0003_DETAIL")
        sqlStat.AppendLine("      WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("        AND DELFLG  = @DELFLG_ALIVE")
        sqlStat.AppendLine("    GROUP BY OILCODE")
        sqlStat.AppendLine(")")
        sqlStat.AppendLine("UPDATE OIL.OIT0002_ORDER")
        sqlStat.AppendLine("   SET  RTANK        = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1101'),0)")
        sqlStat.AppendLine("       ,HTANK        = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1001'),0)")
        sqlStat.AppendLine("       ,TTANK        = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1301'),0)")
        sqlStat.AppendLine("       ,MTTANK       = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1302'),0)")
        sqlStat.AppendLine("       ,KTANK        = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1401'),0)")
        sqlStat.AppendLine("       ,K3TANK       = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1404'),0)")
        'sqlStat.AppendLine("       ,K5TANK       = @K5TANK")
        'sqlStat.AppendLine("       ,K10TANK      = @K10TANK")
        sqlStat.AppendLine("       ,LTANK        = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '2201'),0)")
        sqlStat.AppendLine("       ,ATANK        = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '2101'),0)")
        'sqlStat.AppendLine("       ,OTHER1OTANK  = @OTHER1OTANK")
        'sqlStat.AppendLine("       ,OTHER2OTANK  = @OTHER2OTANK")
        'sqlStat.AppendLine("       ,OTHER3OTANK  = @OTHER3OTANK")
        'sqlStat.AppendLine("       ,OTHER4OTANK  = @OTHER4OTANK")
        'sqlStat.AppendLine("       ,OTHER5OTANK  = @OTHER5OTANK")
        'sqlStat.AppendLine("       ,OTHER6OTANK  = @OTHER6OTANK")
        'sqlStat.AppendLine("       ,OTHER7OTANK  = @OTHER7OTANK")
        'sqlStat.AppendLine("       ,OTHER8OTANK  = @OTHER8OTANK")
        'sqlStat.AppendLine("       ,OTHER9OTANK  = @OTHER9OTANK")
        'sqlStat.AppendLine("       ,OTHER10OTANK = @OTHER10OTANK")
        sqlStat.AppendLine("       ,TOTALTANK    = isnull((SELECT SUM(CARSNUMBER) FROM w_countdetail),0)")

        sqlStat.AppendLine("       ,RTANKCH      = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1101'),0)")
        sqlStat.AppendLine("       ,HTANKCH      = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1001'),0)")
        sqlStat.AppendLine("       ,TTANKCH      = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1301'),0)")
        sqlStat.AppendLine("       ,MTTANKCH     = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1302'),0)")
        sqlStat.AppendLine("       ,KTANKCH      = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1401'),0)")
        sqlStat.AppendLine("       ,K3TANKCH     = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '1404'),0)")
        'sqlStat.AppendLine("       ,K5TANKCH     = @K5TANK")
        'sqlStat.AppendLine("       ,K10TANKCH    = @K10TANK")
        sqlStat.AppendLine("       ,LTANKCH      = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '2201'),0)")
        sqlStat.AppendLine("       ,ATANKCH      = isnull((SELECT CARSNUMBER FROM w_countdetail WHERE OILCODE = '2101'),0)")
        'sqlStat.AppendLine("       ,OTHER1OTANKCH = @OTHER1OTANK")
        'sqlStat.AppendLine("       ,OTHER2OTANKCH = @OTHER2OTANK")
        'sqlStat.AppendLine("       ,OTHER3OTANKCH = @OTHER3OTANK")
        'sqlStat.AppendLine("       ,OTHER4OTANKCH = @OTHER4OTANK")
        'sqlStat.AppendLine("       ,OTHER5OTANKCH = @OTHER5OTANK")
        'sqlStat.AppendLine("       ,OTHER6OTANKCH = @OTHER6OTANK")
        'sqlStat.AppendLine("       ,OTHER7OTANKCH = @OTHER7OTANK")
        'sqlStat.AppendLine("       ,OTHER8OTANKCH = @OTHER8OTANK")
        'sqlStat.AppendLine("       ,OTHER9OTANKCH = @OTHER9OTANK")
        'sqlStat.AppendLine("       ,OTHER10OTANKCH = @OTHER10OTANK")
        sqlStat.AppendLine("       ,TOTALTANKCH  = isnull((SELECT SUM(CARSNUMBER) FROM w_countdetail),0)")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("ORDERNO", SqlDbType.NVarChar).Value = orderItm.OrderNo
                .Add("DELFLG_ALIVE", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            sqlCmd.CommandTimeout = 300
            sqlCmd.ExecuteNonQuery()
        End Using

    End Sub
    ''' <summary>
    ''' 受注明細追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="detailItem"></param>
    Public Sub InsertOrderDetail(sqlCon As SqlConnection, sqlTran As SqlTransaction, detailItem As OrderDetailItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO OIL.OIT0003_DETAIL")
        sqlStat.AppendLine("   (ORDERNO,DETAILNO,SHIPORDER,LINEORDER,TANKNO,KAMOKU,STACKINGORDERNO,STACKINGFLG,WHOLESALEFLG,INSPECTIONFLG,DETENTIONFLG,FIRSTRETURNFLG,AFTERRETURNFLG,OTTRANSPORTFLG,UPGRADEFLG,ORDERINFO,")
        sqlStat.AppendLine("    SHIPPERSCODE,SHIPPERSNAME,OILCODE,OILNAME,")
        sqlStat.AppendLine("    ORDERINGTYPE,ORDERINGOILNAME,")
        sqlStat.AppendLine("    CARSNUMBER,CARSAMOUNT,RETURNDATETRAIN,")
        sqlStat.AppendLine("    JOINTCODE,JOINT,REMARK,")
        sqlStat.AppendLine("    CHANGETRAINNO,CHANGETRAINNAME,")
        sqlStat.AppendLine("    SECONDCONSIGNEECODE,SECONDCONSIGNEENAME,")
        sqlStat.AppendLine("    SECONDARRSTATION,SECONDARRSTATIONNAME,")
        sqlStat.AppendLine("    CHANGERETSTATION,CHANGERETSTATIONNAME,")
        sqlStat.AppendLine("    LINE,FILLINGPOINT,")
        sqlStat.AppendLine("    LOADINGIRILINETRAINNO,LOADINGIRILINETRAINNAME,")
        sqlStat.AppendLine("    LOADINGIRILINEORDER,LOADINGOUTLETTRAINNO,")
        sqlStat.AppendLine("    LOADINGOUTLETTRAINNAME,LOADINGOUTLETORDER,")
        sqlStat.AppendLine("    ACTUALLODDATE,ACTUALDEPDATE,ACTUALARRDATE,ACTUALACCDATE,ACTUALEMPARRDATE,RESERVEDNO,OTSENDCOUNT,DLRESERVEDCOUNT,DLTAKUSOUCOUNT,")
        sqlStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,ANASYORIFLG,VOLSYORIFLG,")
        sqlStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD )")
        sqlStat.AppendLine("    VALUES")
        sqlStat.AppendLine("   (@ORDERNO,@DETAILNO,@SHIPORDER,@LINEORDER,@TANKNO,@KAMOKU,@STACKINGORDERNO,@STACKINGFLG,@WHOLESALEFLG,@INSPECTIONFLG,@DETENTIONFLG,@FIRSTRETURNFLG,@AFTERRETURNFLG,@OTTRANSPORTFLG,@UPGRADEFLG,@ORDERINFO,")
        sqlStat.AppendLine("    @SHIPPERSCODE,@SHIPPERSNAME,@OILCODE,@OILNAME,")
        sqlStat.AppendLine("    @ORDERINGTYPE,@ORDERINGOILNAME,")
        sqlStat.AppendLine("    @CARSNUMBER,@CARSAMOUNT,@RETURNDATETRAIN,")
        sqlStat.AppendLine("    @JOINTCODE,@JOINT,@REMARK,")
        sqlStat.AppendLine("    @CHANGETRAINNO,@CHANGETRAINNAME,")
        sqlStat.AppendLine("    @SECONDCONSIGNEECODE,@SECONDCONSIGNEENAME,")
        sqlStat.AppendLine("    @SECONDARRSTATION,@SECONDARRSTATIONNAME,")
        sqlStat.AppendLine("    @CHANGERETSTATION,@CHANGERETSTATIONNAME,")
        sqlStat.AppendLine("    @LINE,@FILLINGPOINT,")
        sqlStat.AppendLine("    @LOADINGIRILINETRAINNO,@LOADINGIRILINETRAINNAME,")
        sqlStat.AppendLine("    @LOADINGIRILINEORDER,@LOADINGOUTLETTRAINNO,")
        sqlStat.AppendLine("    @LOADINGOUTLETTRAINNAME,@LOADINGOUTLETORDER,")
        sqlStat.AppendLine("    @ACTUALLODDATE,@ACTUALDEPDATE,@ACTUALARRDATE,@ACTUALACCDATE,@ACTUALEMPARRDATE,@RESERVEDNO,@OTSENDCOUNT,@DLRESERVEDCOUNT,@DLTAKUSOUCOUNT,")
        sqlStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,@ANASYORIFLG,@VOLSYORIFLG,")
        sqlStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD )")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("ORDERNO", SqlDbType.NVarChar).Value = detailItem.OrderNo
                .Add("DETAILNO", SqlDbType.NVarChar).Value = detailItem.DetailNo
                .Add("SHIPORDER", SqlDbType.NVarChar).Value = detailItem.ShipOrder
                .Add("LINEORDER", SqlDbType.NVarChar).Value = detailItem.LineOrder
                .Add("TANKNO", SqlDbType.NVarChar).Value = detailItem.TankNo
                .Add("KAMOKU", SqlDbType.NVarChar).Value = detailItem.Kamoku
                .Add("STACKINGORDERNO", SqlDbType.NVarChar).Value = detailItem.StackingOrderNo
                .Add("STACKINGFLG", SqlDbType.NVarChar).Value = detailItem.StackingFlg
                .Add("WHOLESALEFLG", SqlDbType.NVarChar).Value = detailItem.WholeSaleFlg
                .Add("INSPECTIONFLG", SqlDbType.NVarChar).Value = detailItem.InspectionFlg
                .Add("DETENTIONFLG", SqlDbType.NVarChar).Value = detailItem.DetentionFlg
                .Add("FIRSTRETURNFLG", SqlDbType.NVarChar).Value = detailItem.FirstReturnFlg
                .Add("AFTERRETURNFLG", SqlDbType.NVarChar).Value = detailItem.AfterReturnFlg
                .Add("OTTRANSPORTFLG", SqlDbType.NVarChar).Value = detailItem.OtTransportFlg
                .Add("UPGRADEFLG", SqlDbType.NVarChar).Value = detailItem.UpgradeFlg
                .Add("ORDERINFO", SqlDbType.NVarChar).Value = detailItem.OrderInfo
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = detailItem.ShippersCode
                .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = detailItem.ShippersName
                .Add("OILCODE", SqlDbType.NVarChar).Value = detailItem.OilCode
                .Add("OILNAME", SqlDbType.NVarChar).Value = detailItem.OilName
                .Add("ORDERINGTYPE", SqlDbType.NVarChar).Value = detailItem.OrderingType
                .Add("ORDERINGOILNAME", SqlDbType.NVarChar).Value = detailItem.OrderingOilName
                .Add("CARSNUMBER", SqlDbType.NVarChar).Value = detailItem.CarsNumber
                .Add("CARSAMOUNT", SqlDbType.NVarChar).Value = detailItem.CarsAmount
                .Add("RETURNDATETRAIN", SqlDbType.NVarChar).Value = detailItem.ReturnDateTrain
                .Add("JOINTCODE", SqlDbType.NVarChar).Value = detailItem.JointCode
                .Add("JOINT", SqlDbType.NVarChar).Value = detailItem.Joint
                .Add("REMARK", SqlDbType.NVarChar).Value = detailItem.Remark
                .Add("CHANGETRAINNO", SqlDbType.NVarChar).Value = detailItem.ChangeTrainNo
                .Add("CHANGETRAINNAME", SqlDbType.NVarChar).Value = detailItem.ChangeTrainName
                .Add("SECONDCONSIGNEECODE", SqlDbType.NVarChar).Value = detailItem.SecondConsigneeCode
                .Add("SECONDCONSIGNEENAME", SqlDbType.NVarChar).Value = detailItem.SecondConsigneeName
                .Add("SECONDARRSTATION", SqlDbType.NVarChar).Value = detailItem.SecondArrStation
                .Add("SECONDARRSTATIONNAME", SqlDbType.NVarChar).Value = detailItem.SecondArrStationName
                .Add("CHANGERETSTATION", SqlDbType.NVarChar).Value = detailItem.ChangeRetStation
                .Add("CHANGERETSTATIONNAME", SqlDbType.NVarChar).Value = detailItem.ChangeRetStationName
                .Add("LINE", SqlDbType.NVarChar).Value = detailItem.Line
                .Add("FILLINGPOINT", SqlDbType.NVarChar).Value = detailItem.FillingPoint
                .Add("LOADINGIRILINETRAINNO", SqlDbType.NVarChar).Value = detailItem.LoadingIriLineTrainNo
                .Add("LOADINGIRILINETRAINNAME", SqlDbType.NVarChar).Value = detailItem.LoadingIriLineTrainName
                .Add("LOADINGIRILINEORDER", SqlDbType.NVarChar).Value = detailItem.LoadingIriLineOrder
                .Add("LOADINGOUTLETTRAINNO", SqlDbType.NVarChar).Value = detailItem.LoadingOutletTrainNo
                .Add("LOADINGOUTLETTRAINNAME", SqlDbType.NVarChar).Value = detailItem.LoadingOutletTrainName
                .Add("LOADINGOUTLETORDER", SqlDbType.NVarChar).Value = detailItem.LoadingOutletOrder
                .Add("ACTUALLODDATE", SqlDbType.NVarChar).Value = If(detailItem.ActualLodDate = "", CType(DBNull.Value, Object), detailItem.ActualLodDate)
                .Add("ACTUALDEPDATE", SqlDbType.NVarChar).Value = If(detailItem.ActualDepDate = "", CType(DBNull.Value, Object), detailItem.ActualDepDate)
                .Add("ACTUALARRDATE", SqlDbType.NVarChar).Value = If(detailItem.ActualArrDate = "", CType(DBNull.Value, Object), detailItem.ActualArrDate)
                .Add("ACTUALACCDATE", SqlDbType.NVarChar).Value = If(detailItem.ActualAccDate = "", CType(DBNull.Value, Object), detailItem.ActualAccDate)
                .Add("ACTUALEMPARRDATE", SqlDbType.NVarChar).Value = If(detailItem.ActualEmpArrDate = "", CType(DBNull.Value, Object), detailItem.ActualEmpArrDate)
                .Add("RESERVEDNO", SqlDbType.NVarChar).Value = detailItem.ReservedNo
                .Add("OTSENDCOUNT", SqlDbType.NVarChar).Value = detailItem.OtSendCount
                .Add("DLRESERVEDCOUNT", SqlDbType.NVarChar).Value = detailItem.DlReservedCount
                .Add("DLTAKUSOUCOUNT", SqlDbType.NVarChar).Value = detailItem.DlTakusouCount
                .Add("SALSE", SqlDbType.NVarChar).Value = detailItem.Salse
                .Add("SALSETAX", SqlDbType.NVarChar).Value = detailItem.SalseTax
                .Add("TOTALSALSE", SqlDbType.NVarChar).Value = detailItem.TotalSalse
                .Add("PAYMENT", SqlDbType.NVarChar).Value = detailItem.Payment
                .Add("PAYMENTTAX", SqlDbType.NVarChar).Value = detailItem.PaymentTax
                .Add("TOTALPAYMENT", SqlDbType.NVarChar).Value = detailItem.TotalPayment

                .Add("ANASYORIFLG", SqlDbType.NVarChar).Value = detailItem.AnaSyoriFlg
                .Add("VOLSYORIFLG", SqlDbType.NVarChar).Value = detailItem.VolSyoriFlg

                .Add("DELFLG", SqlDbType.NVarChar).Value = detailItem.DelFlg
                .Add("INITYMD", SqlDbType.NVarChar).Value = detailItem.InitYmd
                .Add("INITUSER", SqlDbType.NVarChar).Value = detailItem.InitUser
                .Add("INITTERMID", SqlDbType.NVarChar).Value = detailItem.InitTermId
                .Add("UPDYMD", SqlDbType.NVarChar).Value = detailItem.UpdYmd
                .Add("UPDUSER", SqlDbType.NVarChar).Value = detailItem.UpdUser
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = detailItem.UpdTermId
                .Add("RECEIVEYMD", SqlDbType.NVarChar).Value = detailItem.ReceiveYmd
            End With
            sqlCmd.CommandTimeout = 300
            sqlCmd.ExecuteNonQuery()
        End Using
        CS0020JOURNAL.TABLENM = "OIT0003_DETAIL"
        CS0020JOURNAL.ACTION = "INSERT"
        CS0020JOURNAL.ROW = detailItem.ToDataTable.Rows(0)
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
    ''' <summary>
    ''' 受注明細(論理)削除処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="detailItem"></param>
    Public Sub DeleteOrderDetail(sqlCon As SqlConnection, sqlTran As SqlTransaction, detailItem As OrderDetailItem)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE OIL.OIT0003_DETAIL")
        sqlStat.AppendLine("   SET DELFLG     = @DELFLG")
        sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE ORDERNO  = @ORDERNO")
        sqlStat.AppendLine("   AND DETAILNO = @DETAILNO")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("ORDERNO", SqlDbType.NVarChar).Value = detailItem.OrderNo
                .Add("DETAILNO", SqlDbType.NVarChar).Value = detailItem.DetailNo
                .Add("DELFLG", SqlDbType.NVarChar).Value = detailItem.DelFlg
                .Add("UPDYMD", SqlDbType.NVarChar).Value = detailItem.UpdYmd
                .Add("UPDUSER", SqlDbType.NVarChar).Value = detailItem.UpdUser
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = detailItem.UpdTermId
                .Add("RECEIVEYMD", SqlDbType.NVarChar).Value = detailItem.ReceiveYmd
            End With
            sqlCmd.CommandTimeout = 300
            sqlCmd.ExecuteNonQuery()
        End Using
        CS0020JOURNAL.TABLENM = "OIT0003_DETAIL"
        CS0020JOURNAL.ACTION = "DELETE"
        CS0020JOURNAL.ROW = detailItem.ToDataTable.Rows(0)
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
    ''' <summary>
    ''' 受注履歴テーブル用の履歴番号取得
    ''' </summary>
    ''' <returns>履歴番号</returns>
    Private Function GetNewOrderHistoryNo(ByVal sqlCon As SqlConnection, ByRef errMes As EntryOrderResultItm) As String
        errMes = Nothing
        Dim retVal As String = ""
        Try
            Dim sqlStr As New StringBuilder
            sqlStr.AppendLine("SELECT FX.KEYCODE  AS HISTORYNO")
            sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
            sqlStr.AppendLine(" WHERE FX.CLASS    = @CLASS")
            sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar).Value = "NEWHISTORYNOGET"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows Then
                        sqlDr.Read()
                        retVal = Convert.ToString(sqlDr("HISTORYNO"))
                    Else
                        '取得できないと後続処理ができないのでエラー扱い
                        errMes = New EntryOrderResultItm
                        errMes.MessageId = C_MESSAGE_NO.MASTER_NOT_FOUND_ERROR
                        errMes.Message = "NEWHISTORYNOGET"
                    End If
                End Using 'sqlDr
            End Using
        Catch ex As Exception
            errMes = New EntryOrderResultItm
            errMes.MessageId = C_MESSAGE_NO.DB_ERROR
            errMes.Message = "GetNewHistoryNo Error"
            errMes.StackTrace = ex.ToString()

            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0004C MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0004C MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
        Return retVal
    End Function
    ''' <summary>
    ''' ジャーナル書き込み
    ''' </summary>
    ''' <param name="journalDt"></param>
    ''' <returns></returns>
    Private Function OutputJournal(journalDt As DataTable) As Boolean
        For Each dr As DataRow In journalDt.Rows
            CS0020JOURNAL.TABLENM = "OIT0001_OILSTOCK"
            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
            CS0020JOURNAL.ROW = dr
            CS0020JOURNAL.CS0020JOURNAL()
            If Not isNormal(CS0020JOURNAL.ERR) Then
                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                Return False
            End If
        Next
        Return True
    End Function
    ''' <summary>
    ''' 入力チェック処理
    ''' </summary>
    ''' <param name="checkObj"></param>
    ''' <param name="callerButton">呼出し元ボタンID</param>
    ''' <returns>True:正常,False:異常</returns>
    Private Function WW_Check(checkObj As DispDataClass, callerButton As String) As Boolean
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        'ダウンロードボタン押下時は年月のみチェック
        If callerButton = "WF_ButtonOkCommonPopUp" Then

            '日付書式チェック
            If Me.chkPrintENEOS.Checked = False Then
                If Me.txtDownloadMonth.Text.Trim = "" Then
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="帳票年月", needsPopUp:=True)
                    AppendForcusObject(txtDownloadMonth.ClientID)
                    WW_CheckMES1 = "年月入力エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    Return False
                End If
                If IsDate(Me.txtDownloadMonth.Text.Trim & "/01") = False Then
                    Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="帳票年月", needsPopUp:=True)
                    AppendForcusObject(txtDownloadMonth.ClientID)
                    WW_CheckMES1 = "年月入力エラー。" & "(" & Me.txtDownloadMonth.Text.Trim & ")"
                    WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                End If
            Else
                If Me.txtReportFromDate.Text.Trim <> "" Then
                    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "REPORTDATE", Me.txtReportFromDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If Not isNormal(WW_CS0024FCHECKERR) Then
                        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="開始日", needsPopUp:=True)
                        AppendForcusObject(Me.txtReportFromDate.ClientID)
                        WW_CheckMES1 = "開始日書式エラー。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        Return False
                    End If
                Else
                    Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="開始日", needsPopUp:=True)
                    AppendForcusObject(txtDownloadMonth.ClientID)
                    WW_CheckMES1 = "ENEOS時の開始日入力エラー。" & "(" & Me.txtReportFromDate.Text.Trim & ")"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)

                    Return False
                End If
            End If


            'If Me.txtReportToDate.Text.Trim <> "" Then
            '    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "REPORTDATE", Me.txtReportToDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            '    If Not isNormal(WW_CS0024FCHECKERR) Then
            '        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="終了日", needsPopUp:=True)
            '        AppendForcusObject(Me.txtReportToDate.ClientID)
            '        WW_CheckMES1 = "終了日書式エラー。"
            '        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        Return False
            '    End If
            '    'TOを入れてFromが未入力の場合
            '    If Me.chkPrintENEOS.Checked = False AndAlso Me.txtReportFromDate.Text = "" Then
            '        Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="開始日", needsPopUp:=True)
            '        AppendForcusObject(Me.txtReportToDate.ClientID)
            '        WW_CheckMES1 = "開始日・終了日前後関係エラー"
            '        WW_CheckMES2 = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        Return False
            '    End If

            'End If


            ''日付前後関係チェック
            'If Me.chkPrintENEOS.Checked = False AndAlso
            '   Me.txtReportFromDate.Text <> "" AndAlso Me.txtReportToDate.Text <> "" Then
            '    Dim dtmChkFrom As Date = CDate(Me.txtReportFromDate.Text)
            '    Dim dtmChkTo As Date = CDate(Me.txtReportToDate.Text)
            '    If dtmChkFrom > dtmChkTo Then

            '        Master.Output(C_MESSAGE_NO.START_END_DATE_RELATION_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="開始日", needsPopUp:=True)
            '        AppendForcusObject(Me.txtReportToDate.ClientID)
            '        WW_CheckMES1 = "開始日・終了日前後関係エラー"
            '        WW_CheckMES2 = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        Return False
            '    End If
            'End If
            Return True
        End If
        '受注提案タンク車数の一覧を表示している場合
        If checkObj.ShowSuggestList = True Then
            '画面ボタン欄の在庫維持日数(自動提案の場合のみチェック)
            If Me.WF_ButtonAUTOSUGGESTION.ID = callerButton Then
                '〇在庫維持日数
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INVENTORYDAYS", WF_INVENTORYDAYS.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="在庫維持日数", needsPopUp:=True)
                    AppendForcusObject(WF_INVENTORYDAYS.ClientID)
                    WW_CheckMES1 = "在庫維持日数入力エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    Return False
                End If 'isNormal(WW_CS0024FCHECKERR) 

            End If 'Me.WF_ButtonAUTOSUGGESTION.ID = callerButton 
            '1件でもチェックボックスにチェックがある場合Trueを設定する
            Dim hasAnyCheckBoxesCheclked As Boolean = False
            '〇受注提案タンク車数表の提案数（テキストボックス）入力値チェック
            If {"WF_ButtonORDERLIST", "WF_ButtonRECULC"}.Contains(callerButton) Then
                For Each suggestListItm In checkObj.SuggestList.Values
                    Dim trainInfo As TrainListItem = suggestListItm.TrainInfo
                    Dim dayInfo As DaysItem = suggestListItm.DayInfo
                    For Each valueItm In suggestListItm.SuggestOrderItem.Values
                        'チェックボックスにチェックがある場合
                        If valueItm.CheckValue Then
                            'メッセージは前受注提案の値を精査した後
                            hasAnyCheckBoxesCheclked = True
                        End If
                        '日付・列車が元となる各油種の値をチェック
                        For Each svalItm In valueItm.SuggestValuesItem.Values
                            '受入数単項目チェック
                            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUGGESTVALUE", svalItm.ItemValue, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                            If Not isNormal(WW_CS0024FCHECKERR) Then
                                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="受入数", needsPopUp:=True)
                                AppendForcusObject(svalItm.ItemValueTextBoxClientId)
                                WW_CheckMES1 = String.Format("受入数入力エラー。日付:{0},列車:{1},油種:{2}", dayInfo.DispDate, trainInfo.TrainNo, svalItm.OilInfo.OilName)
                                WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                                Return False
                            End If 'isNormal(WW_CS0024FCHECKERR) 
                        Next svalItm

                        If checkObj.HasMoveInsideItem = False Then
                            Continue For
                        End If

                        For Each svalItm In valueItm.MiSuggestValuesItem.Values
                            '受入数単項目チェック
                            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUGGESTVALUE", svalItm.ItemValue, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                            If Not isNormal(WW_CS0024FCHECKERR) Then
                                Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="受入数", needsPopUp:=True)
                                AppendForcusObject(svalItm.ItemValueTextBoxClientId)
                                WW_CheckMES1 = String.Format("受入数入力エラー。日付:{0},列車:{1},油種:{2}", dayInfo.DispDate, trainInfo.TrainNo, svalItm.OilInfo.OilName)
                                WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                                Return False
                            End If 'isNormal(WW_CS0024FCHECKERR) 
                        Next

                    Next valueItm

                Next suggestListItm
            End If '受注提案タンク車入力チェック動作If

            '受注作成だけの入力チェック
            If {"WF_ButtonORDERLIST"}.Contains(callerButton) Then
                '提案リストのチェックボックスがすべてOFFの場合作るべき情報が無いためエラー
                If checkObj.HasSuggestCheckedItem = False Then
                    Master.Output(C_MESSAGE_NO.OIL_ORDER_NO_CHECKED_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="受入数", needsPopUp:=True)
                    'AppendForcusObject(svalItm.ItemValueTextBoxClientId)
                    WW_CheckMES1 = "提案表未チェックエラー"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_ORDER_NO_CHECKED_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    Return False
                End If
            End If
        End If 'checkObj.ShowSuggestList = True '受注提案表が画面表示しているか

        '在庫表 払出入力チェック
        If {"WF_ButtonRECULC", "WF_ButtonUPDATE"}.Contains(callerButton) Then
            For Each stockListItem In checkObj.StockList.Values
                Dim oilName As String = stockListItem.OilTypeName
                For Each itm In stockListItem.StockItemListDisplay.Values

                    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SEND", itm.Send, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If Not isNormal(WW_CS0024FCHECKERR) Then
                        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="払出", needsPopUp:=True)
                        AppendForcusObject(itm.SendTextClientId)
                        WW_CheckMES1 = String.Format("払出入力エラー。日付:{0},油種:{1}", itm.DaysItem.DispDate, oilName)
                        WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        Return False
                    End If 'isNormal(WW_CS0024FCHECKERR) 

                    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MORNINGSTOCK", itm.MorningStock, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If Not isNormal(WW_CS0024FCHECKERR) Then
                        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="朝在庫", needsPopUp:=True)
                        AppendForcusObject(itm.MorningStockClientId)
                        WW_CheckMES1 = String.Format("朝在庫入力エラー。日付:{0},油種:{1}", itm.DaysItem.DispDate, oilName)
                        WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        Return False
                    End If 'isNormal(WW_CS0024FCHECKERR) 
                    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RECEIVEFROMLORRY", itm.ReceiveFromLorry, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If Not isNormal(WW_CS0024FCHECKERR) Then
                        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="ﾛｰﾘｰ受入", needsPopUp:=True)
                        AppendForcusObject(itm.ReceiveFromLorryClientId)
                        WW_CheckMES1 = String.Format("ﾛｰﾘｰ受入入力エラー。日付:{0},油種:{1}", itm.DaysItem.DispDate, oilName)
                        WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        Return False
                    End If 'isNormal(WW_CS0024FCHECKERR) 
                    Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RECEIVEFROMLORRY", itm.ReceiveFromLorry, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)

                    If Not isNormal(WW_CS0024FCHECKERR) Then
                        Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="ﾛｰﾘｰ受入", needsPopUp:=True)
                        AppendForcusObject(itm.ReceiveFromLorryClientId)
                        WW_CheckMES1 = String.Format("受入入力エラー。日付:{0},油種:{1}", itm.DaysItem.DispDate, oilName)
                        WW_CheckMES2 = C_MESSAGE_NO.NUMERIC_VALUE_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        Return False
                    End If 'isNormal(WW_CS0024FCHECKERR) 
                Next itm
            Next stockListItem
        End If

        '最後まで来たらチェック正常
        Return True
    End Function


    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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
            With leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value))
                WW_SelectValue = .Value
                WW_SelectText = .Text
            End With
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "txtReportFromDate"             '年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < CDate(C_DEFAULT_YMD) Then
                            txtReportFromDate.Text = ""
                        Else
                            txtReportFromDate.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    txtReportFromDate.Focus()
                Case "txtReportToDate"             '年月日
                    'Dim WW_DATE As Date
                    'Try
                    '    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    '    If WW_DATE < CDate(C_DEFAULT_YMD) Then
                    '        txtReportToDate.Text = ""
                    '    Else
                    '        txtReportToDate.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    '    End If
                    'Catch ex As Exception
                    'End Try
                    'txtReportToDate.Focus()
                Case Nothing
                    Dim dummy = Nothing
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
                Case "txtReportFromDate"          '年月日
                    txtReportFromDate.Focus()
                Case "txtReportToDate"          '年月日
                    'txtReportToDate.Focus()
                Case Nothing
                    Dim dummy = Nothing
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
    ''' 油槽所変更時イベント
    ''' </summary>
    Protected Sub ChangeConsignee()
        Dim consignee As String = Me.hdnChgConsignee.Value
        Dim consigneeName As String = Me.hdnChgConsigneeName.Value
        If consignee = "" Then
            Return
        End If
        WW_MAPValueSet(consignee, consigneeName)
    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0005row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0005row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0005row) Then
            'WW_ERR_MES &= ControlChars.NewLine & "  --> JOT車番 =" & OIM0005row("TANKNUMBER") & " , "
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


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
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"             '運用部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATIONPATTERN"　 '原常備駅C、臨時常備駅C
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' フォーカスを合わせる要素のIDを保持
    ''' </summary>
    ''' <param name="forcusElmId">フォーカス合わせ対象のID</param>
    Private Sub AppendForcusObject(forcusElmId As String)
        Dim hdnObj As New HtmlInputHidden
        hdnObj.ID = "hdnForcusObjId"
        hdnObj.Name = ""
        hdnObj.Value = forcusElmId
        hdnObj.EnableViewState = False
        Me.work.Controls.Add(hdnObj)
    End Sub
    ''' <summary>
    ''' 画面情報保持クラスを保存
    ''' </summary>
    ''' <param name="dispDataClass"></param>
    ''' <remarks>一旦ViewStateに保存
    ''' （画面の元データクラスをシリアライズ→Base64化→1レコードのDataTableにBase64文字を格納に格納、
    ''' 　共通関数データテーブル退避）</remarks>
    Private Sub SaveThisScreenValue(dispDataClass As DispDataClass)
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim base64Str As String = ""
        Dim noConpressionByte As Byte()
        'クラスをシリアライズ
        Using ms As New IO.MemoryStream()
            formatter.Serialize(ms, dispDataClass)
            noConpressionByte = ms.ToArray
        End Using
        '圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
        Using ms As New IO.MemoryStream(),
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            ds.Write(noConpressionByte, 0, noConpressionByte.Length)
            ds.Close()
            Dim byteDat = ms.ToArray
            base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
        End Using
        'Base64のデータをデータテーブルに移し共通関数で保存
        Using dt As New DataTable("dispData")
            dt.Columns.Add("LINECNT", GetType(Integer)).DefaultValue = 0
            dt.Columns.Add("dat", GetType(String))
            Dim dr As DataRow = dt.NewRow
            dr("dat") = base64Str
            dt.Rows.Add(dr)
            Master.SaveTable(dt)
        End Using
    End Sub
    ''' <summary>
    ''' 画面入力を取得し画面情報保持クラスに反映
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetThisScreenData(frvSuggestObj As FormView, repStockObj As Repeater) As DispDataClass
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim dt As New DataTable
        Dim conmressedByte As Byte()

        Dim retVal As DispDataClass
        '画面情報クラスの復元
        '退避したデータテーブルからBase64文字を取得
        Master.RecoverTable(dt)
        Using dt
            Dim base64Str = Convert.ToString(dt.Rows(0)("dat"))
            conmressedByte = Convert.FromBase64String(base64Str)
        End Using
        '取得した文字をByte化し解凍、画面利用クラスに再格納
        Using inpMs As New IO.MemoryStream(conmressedByte),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            outMs.Position = 0
            retVal = DirectCast(formatter.Deserialize(outMs), DispDataClass)
        End Using

        '提案表 日付リピーター
        Dim repValArea As Repeater = DirectCast(frvSuggestObj.FindControl("repSuggestItem"), Repeater)
        '提案表画面データの入力項目を画面情報保持クラスに反映
        If retVal.ShowSuggestList = True Then
            SetDispSuggestItemValue(retVal, repValArea)
        End If
        '在庫表画面データの入力項目を画面情報保持クラスに反映
        SetDispStockItemValue(retVal, repStockObj)
        Return retVal
    End Function
    ''' <summary>
    ''' 受注提案タンク車数の入力値取得
    ''' </summary>
    ''' <param name="dispDataClass">IN/OUT 画面情報クラス</param>
    ''' <param name="repSuggestItem">提案数リピーターオブジェクト</param>
    ''' <remarks>GetThisScreenDataより呼び出し他で呼び出さない事</remarks>
    Private Sub SetDispSuggestItemValue(ByRef dispDataClass As DispDataClass, repSuggestItem As Repeater)
        Dim hdnSuggestListKeyObj As HiddenField = Nothing
        Dim suggestListKey As String = ""

        Dim trainRepeater As Repeater = Nothing
        Dim trainIdObj As HiddenField = Nothing
        Dim trainId As String = ""
        Dim hdnTrainLock As HiddenField = Nothing
        Dim ddlDays As DropDownList = Nothing
        Dim chkObj As CheckBox = Nothing

        Dim oilTypeItemValue As Repeater = Nothing
        Dim miOilTypeItemValue As Repeater = Nothing

        Dim oilTypeCodeObj As HiddenField = Nothing
        Dim oilTypeCode As String = ""
        Dim suggestValObj As TextBox = Nothing
        Dim suggestVal As String = ""

        Dim dateValueClassItem As DispDataClass.SuggestItem = Nothing
        Dim trainValueClassItem As DispDataClass.SuggestItem.SuggestValues = Nothing
        Dim oilTypeValueClassItem As DispDataClass.SuggestItem.SuggestValue = Nothing
        Dim miDateValueClassItem As DispDataClass.SuggestItem = Nothing
        Dim mitrainValueClassItem As DispDataClass.SuggestItem.SuggestValues = Nothing
        '一段階目 日付別のリピーター
        For Each repSuggestListItem As RepeaterItem In repSuggestItem.Items
            '提案リストの日付キーを取得
            hdnSuggestListKeyObj = DirectCast(repSuggestListItem.FindControl("hdnSuggestListKey"), HiddenField)
            suggestListKey = hdnSuggestListKeyObj.Value
            dateValueClassItem = dispDataClass.SuggestList(suggestListKey)
            '二段階目の列車IDリピーターを取得
            trainRepeater = DirectCast(repSuggestListItem.FindControl("repSuggestTrainItem"), Repeater)
            For Each repSuggestTrainItem As RepeaterItem In trainRepeater.Items
                '列車番号取得
                trainIdObj = DirectCast(repSuggestTrainItem.FindControl("hdnTrainId"), HiddenField)
                trainId = trainIdObj.Value
                'チェックボックス取得
                chkObj = DirectCast(repSuggestTrainItem.FindControl("chkSuggest"), CheckBox)
                '列車ロック情報取得
                hdnTrainLock = DirectCast(repSuggestTrainItem.FindControl("hdnTrainLock"), HiddenField)
                '受入日日数
                ddlDays = DirectCast(repSuggestTrainItem.FindControl("ddlSuggestAddDays"), DropDownList)
                '列車番号別のクラスを取得
                trainValueClassItem = dateValueClassItem.SuggestOrderItem(trainId)
                '画面情報クラスに設定しているチェックOn/Offの情報を格納
                trainValueClassItem.CheckValue = chkObj.Checked
                '画面情報クラスに列車ロック情報格納
                If hdnTrainLock.Value = "Locked" Then
                    trainValueClassItem.TrainLock = True
                Else
                    trainValueClassItem.TrainLock = False
                End If
                If ddlDays.SelectedItem Is Nothing Then
                    trainValueClassItem.AccAddDays = ""
                Else
                    trainValueClassItem.AccAddDays = ddlDays.SelectedValue
                End If

                '三段階目の油種別の提案数リピーターを取得
                oilTypeItemValue = DirectCast(repSuggestTrainItem.FindControl("repSuggestValueItem"), Repeater)
                For Each repOilTypeValItem As RepeaterItem In oilTypeItemValue.Items
                    oilTypeCodeObj = DirectCast(repOilTypeValItem.FindControl("hdnOilTypeCode"), HiddenField)
                    oilTypeCode = oilTypeCodeObj.Value
                    suggestValObj = DirectCast(repOilTypeValItem.FindControl("txtSuggestValue"), TextBox)
                    suggestVal = suggestValObj.Text
                    oilTypeValueClassItem = trainValueClassItem(oilTypeCode)
                    oilTypeValueClassItem.ItemValue = suggestVal
                    oilTypeValueClassItem.ItemValueTextBoxClientId = suggestValObj.ClientID
                Next repOilTypeValItem '三段階目リピーター
                '構内取りが無い場合は次のループへ
                If dispDataClass.HasMoveInsideItem = False Then
                    Continue For
                End If
                miDateValueClassItem = dispDataClass.MiDispData.SuggestList(suggestListKey)
                mitrainValueClassItem = miDateValueClassItem.SuggestOrderItem(trainId)
                miOilTypeItemValue = DirectCast(repSuggestTrainItem.FindControl("repMiSuggestValueItem"), Repeater)

                mitrainValueClassItem.CheckValue = chkObj.Checked

                For Each repMiOilTypeValItem As RepeaterItem In miOilTypeItemValue.Items
                    oilTypeCodeObj = DirectCast(repMiOilTypeValItem.FindControl("hdnOilTypeCode"), HiddenField)
                    oilTypeCode = oilTypeCodeObj.Value
                    suggestValObj = DirectCast(repMiOilTypeValItem.FindControl("txtSuggestValue"), TextBox)
                    suggestVal = suggestValObj.Text
                    oilTypeValueClassItem = mitrainValueClassItem(oilTypeCode)
                    oilTypeValueClassItem.ItemValue = suggestVal
                    oilTypeValueClassItem.ItemValueTextBoxClientId = suggestValObj.ClientID
                Next repMiOilTypeValItem '三段階目リピーター
            Next repSuggestTrainItem '二段階目リピーター
        Next repSuggestListItem '一段階目リピーター
    End Sub
    ''' <summary>
    ''' 在庫表の入力値取得
    ''' </summary>
    ''' <param name="dispDataClass"></param>
    ''' <param name="repStockItemObj"></param>
    ''' <remarks>GetThisScreenDataより呼び出し他で呼び出さない事</remarks>
    Private Sub SetDispStockItemValue(ByRef dispDataClass As DispDataClass, repStockItemObj As Repeater)
        Dim oilTypeCodeObj As HiddenField = Nothing
        Dim oilTypeCode As String = ""

        Dim repStockVal As Repeater = Nothing
        Dim dateKeyObj As HiddenField = Nothing
        Dim dateKeyStr As String = ""
        Dim sendObj As TextBox = Nothing '画面払出テキストボックス
        Dim sendVal As String = ""
        Dim morningStockObj As TextBox = Nothing
        Dim morningStockVal As String = ""
        Dim receiveObj As TextBox = Nothing
        Dim receiveVal As String = ""
        Dim receiveFromLorryObj As TextBox = Nothing
        Dim receiveFromLorryVal As String = ""
        Dim stockListClass = dispDataClass.StockList
        Dim stockListCol As DispDataClass.StockListCollection = Nothing
        Dim stockListItm As DispDataClass.StockListItem = Nothing
        '在庫表リピーターのループ(油種)
        For Each repOilTypeItem As RepeaterItem In repStockItemObj.Items
            oilTypeCodeObj = DirectCast(repOilTypeItem.FindControl("hdnOilTypeCode"), HiddenField)
            oilTypeCode = oilTypeCodeObj.Value
            repStockVal = DirectCast(repOilTypeItem.FindControl("repStockValues"), Repeater)
            stockListCol = stockListClass(oilTypeCode)
            '在庫表リピーターのループ(日付)
            For Each repStockValItem As RepeaterItem In repStockVal.Items
                dateKeyObj = DirectCast(repStockValItem.FindControl("hdnDateKey"), HiddenField)
                dateKeyStr = dateKeyObj.Value
                sendObj = DirectCast(repStockValItem.FindControl("txtSend"), TextBox)
                sendVal = sendObj.Text
                morningStockObj = DirectCast(repStockValItem.FindControl("txtMorningStock"), TextBox)
                morningStockVal = morningStockObj.Text

                receiveObj = DirectCast(repStockValItem.FindControl("txtReceive"), TextBox)
                receiveVal = receiveObj.Text

                receiveFromLorryObj = DirectCast(repStockValItem.FindControl("txtReceiveFromLorry"), TextBox)
                receiveFromLorryVal = receiveFromLorryObj.Text

                stockListItm = stockListCol.StockItemList(dateKeyStr)
                stockListItm.Send = sendVal
                stockListItm.SendTextClientId = sendObj.ClientID
                stockListItm.MorningStock = morningStockVal
                stockListItm.MorningStockClientId = morningStockObj.ClientID
                If dispDataClass.ShowSuggestList = False Then
                    stockListItm.Receive = receiveVal
                    stockListItm.ReceiveClientId = receiveObj.ClientID
                End If
                stockListItm.ReceiveFromLorry = receiveFromLorryVal
                stockListItm.ReceiveFromLorryClientId = receiveFromLorryObj.ClientID
            Next repStockValItem
        Next repOilTypeItem
    End Sub
    ''' <summary>
    ''' 油種保持クラス
    ''' </summary>
    <Serializable>
    Public Class OilItem
        ''' <summary>
        ''' 油種コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OilCode As String = ""
        ''' <summary>
        ''' 油種名
        ''' </summary>
        ''' <returns></returns>
        Public Property OilName As String = ""
        ''' <summary>
        ''' 油種細分コード
        ''' </summary>
        ''' <returns></returns>
        Public Property SegmentOilCode As String = ""
        ''' <summary>
        ''' 油種名（細分）
        ''' </summary>
        Public Property SegmentOilName As String = ""
        ''' <summary>
        ''' OT油種コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OtOilCode As String = ""
        ''' <summary>
        ''' OT油種名
        ''' </summary>
        ''' <returns></returns>
        Public Property OtOilName As String = ""
        ''' <summary>
        ''' 荷主油種コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipperOilCode As String = ""
        ''' <summary>
        ''' 荷主油種名
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipperOilName As String = ""
        ''' <summary>
        ''' 比重
        ''' </summary>
        ''' <returns></returns>
        Public Property Weight As Decimal = 0
        ''' <summary>
        ''' 大分類コード
        ''' </summary>
        ''' <returns></returns>
        Public Property BigOilCode As String = ""
        ''' <summary>
        ''' 中分類コード
        ''' </summary>
        ''' <returns></returns>
        Public Property MiddleOilCode As String = ""
        ''' <summary>
        ''' タンク容量
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>これを元に80%及び目標を在庫を計算</remarks>
        Public Property MaxTankCap As Decimal
        ''' <summary>
        ''' 目標在庫率
        ''' </summary>
        ''' <returns></returns>
        Public Property TankCapRate As Decimal
        ''' <summary>
        ''' D/S
        ''' </summary>
        ''' <returns></returns>
        Public Property DS As Decimal
        ''' <summary>
        ''' 前週出荷平均
        ''' </summary>
        ''' <returns></returns>

        Public Property LastSendAverage As Decimal
        '''' <summary>
        '''' 画面表示期間より１日前の夕在庫
        '''' </summary>
        '''' <returns></returns>
        'Public Property OffScreenLastEveningStock As Decimal
        ''' <summary>
        ''' 油種変更有無(True:あり,False:なし)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>油槽所諸元マスタの開始月日が1/1、終了年月が12/1以外の場合True,それいがいFalse</remarks>
        Public Property IsOilTypeSwitch As Boolean
        ''' <summary>
        ''' 開始月日(MM/dd形式)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>年月0パディング済で格納想定</remarks>
        Public Property FromMd As String
        ''' <summary>
        ''' 終了月日(MM/dd形式)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>年月0パディング済で格納想定</remarks>
        Public Property ToMd As String
        ''' <summary>
        ''' 平均積高(帳票用：前年同月の輸送数量 ÷ 車数）
        ''' </summary>
        ''' <returns></returns>
        Public Property PrintStockAmountAverage As Decimal
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="oilCode"></param>
        ''' <param name="oilName"></param>
        ''' <param name="bigOilCode"></param>
        ''' <param name="middleOilCode"></param>
        Public Sub New(oilCode As String, oilName As String, bigOilCode As String, middleOilCode As String)
            Me.OilCode = oilCode
            Me.OilName = oilName
            Me.BigOilCode = bigOilCode
            Me.MiddleOilCode = middleOilCode
            Me.PrintStockAmountAverage = 0D
            'DOTO 一旦各種値はベタ打ちの為DBより取得が必要
            Select Case oilCode
                Case "1001" 'ハイオク
                    Me.Weight = 0.75D
                    Me.LastSendAverage = 280
                    'Me.OffScreenLastEveningStock = 520
                Case "1101" 'レギュラー
                    Me.Weight = 0.75D
                    Me.LastSendAverage = 800
                    'Me.OffScreenLastEveningStock = 2222
                Case "1301" '灯油
                    Me.Weight = 0.79D
                    Me.LastSendAverage = 250
                    'Me.OffScreenLastEveningStock = 1360
                Case "1401" '軽油
                    Me.Weight = 0.75D
                    Me.LastSendAverage = 5800
                    'Me.OffScreenLastEveningStock = 11000
                Case "2101" 'Ａ重油
                    Me.Weight = 0.87D
                    Me.LastSendAverage = 150
                    'Me.OffScreenLastEveningStock = 500
                Case "2201" 'ＬＳＡ
                    Me.Weight = 0.87D
                    Me.LastSendAverage = 75
                    'Me.OffScreenLastEveningStock = 400
                Case "1302" '未添加灯油
                    Me.Weight = 0.75D
                    Me.LastSendAverage = 800
                    'Me.OffScreenLastEveningStock = 1360
                Case "1404" '３号軽油
                    Me.Weight = 0.82D
                    Me.LastSendAverage = 0
                    'Me.OffScreenLastEveningStock = 10800
                Case Else
                    Me.Weight = 0.75D
                    Me.LastSendAverage = 280
                    'Me.OffScreenLastEveningStock = 1360
            End Select

        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(oilCode As String, oilName As String)
            Me.New(oilCode, oilName, "", "")
        End Sub
        ''' <summary>
        ''' 自身のコピーを別インスタンスで生成する
        ''' </summary>
        ''' <returns></returns>
        Public Function Copy() As OilItem
            Dim retVal As New OilItem(Me.OilCode, Me.OilName, Me.BigOilCode, Me.MiddleOilCode)
            retVal.SegmentOilCode = Me.SegmentOilCode
            retVal.SegmentOilName = Me.SegmentOilName
            retVal.OtOilCode = Me.OtOilCode
            retVal.OtOilName = Me.OtOilName
            retVal.ShipperOilCode = Me.ShipperOilCode
            retVal.ShipperOilName = Me.ShipperOilName
            retVal.Weight = Me.Weight
            retVal.MaxTankCap = Me.MaxTankCap
            retVal.TankCapRate = Me.TankCapRate
            retVal.DS = Me.DS
            retVal.LastSendAverage = Me.LastSendAverage
            retVal.IsOilTypeSwitch = Me.IsOilTypeSwitch
            retVal.FromMd = Me.FromMd
            retVal.ToMd = Me.ToMd
            retVal.PrintStockAmountAverage = Me.PrintStockAmountAverage
            Return retVal
        End Function
    End Class
    ''' <summary>
    ''' 列車番号クラス
    ''' </summary>
    <Serializable>
    Public Class TrainListItem
        ''' <summary>
        ''' 列車番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainNo As String
        ''' <summary>
        ''' 列車名
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainName As String
        ''' <summary>
        ''' 列車最大受入数
        ''' </summary>
        ''' <returns></returns>
        ''' <remark>自動提案の最大数を格納</remark>
        Public Property MaxVolume As Decimal
        ''' <summary>
        ''' 積置フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Tsumi As String
        ''' <summary>
        ''' 発駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property DepStation As String
        ''' <summary>
        ''' 発駅名
        ''' </summary>
        ''' <returns></returns>
        Public Property DepStationName As String
        ''' <summary>
        ''' 着駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrStation As String
        ''' <summary>
        ''' 着駅名
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrStationName As String
        ''' <summary>
        ''' 発日日数
        ''' </summary>
        ''' <returns></returns>
        Public Property DepDays As Decimal
        ''' <summary>
        ''' 特継日数
        ''' </summary>
        ''' <returns></returns>
        Public Property MargeDays As Decimal
        ''' <summary>
        ''' 積車着日数
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrDays As Decimal
        ''' <summary>
        ''' 受入日数
        ''' </summary>
        ''' <returns></returns>
        Public Property AccDays As Decimal
        ''' <summary>
        ''' 空車着日数
        ''' </summary>
        ''' <returns></returns>
        Public Property EmpArrDays As Decimal
        ''' <summary>
        ''' 当日利用日数
        ''' </summary>
        ''' <returns></returns>
        Public Property UseDays As Decimal
        ''' <summary>
        ''' プラントコード
        ''' </summary>
        ''' <returns></returns>
        Public Property PlantCode As String
        ''' <summary>
        ''' プラント名
        ''' </summary>
        ''' <returns></returns>
        Public Property PlantName As String
        ''' <summary>
        ''' パターンコード
        ''' </summary>
        ''' <returns></returns>
        Public Property PatCode As String
        ''' <summary>
        ''' パターン名
        ''' </summary>
        ''' <returns></returns>
        Public Property PatName As String
        ''' <summary>
        ''' 積置可否フラグ(同クラス内プロパティTSUMIで判定)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StackingFlg As String
            Get
                If Me.Tsumi = "T" Then
                    Return "1"
                Else
                    Return "2"
                End If
            End Get
        End Property
        ''' <summary>
        ''' 管理対象外列車(受注作成、シミュレーション対象外の列車判定用)
        ''' </summary>
        ''' <returns></returns>
        Public Property UnmanagedTrain As Boolean = False
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="trainNo">列車番号</param>
        ''' <param name="trainName">列車名</param>
        ''' <param name="maxVolume">列車最大受入数</param>
        Public Sub New(trainNo As String, trainName As String, maxVolume As Decimal)
            Me.TrainNo = trainNo
            Me.TrainName = trainName
            Me.MaxVolume = maxVolume
        End Sub
    End Class
    ''' <summary>
    ''' 日付保持クラス
    ''' </summary>
    <Serializable>
    Public Class DaysItem
        ''' <summary>
        ''' 画面表示用日付書式
        ''' </summary>
        Const DISP_DATEFORMAT As String = "M月d日(ddd)" '"M月d日(<\span>ddd</\span>)"
        ''' <summary>
        ''' キーとなる日付文字列(yyyy/MM/dd)
        ''' </summary>
        ''' <returns></returns>
        Public Property KeyString As String
        ''' <summary>
        ''' 画面表示用日付
        ''' </summary>
        ''' <returns></returns>
        Public Property DispDate As String
        ''' <summary>
        ''' 日付型での対象日付
        ''' </summary>
        ''' <returns></returns>
        Public Property ItemDate As Date
        ''' <summary>
        ''' 祝祭日判定(True:祝祭日,False:通常日)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsHoliday As Boolean = False
        ''' <summary>
        ''' 曜日番号(日:0 ～ 土：6)
        ''' </summary>
        ''' <returns></returns>
        Public Property WeekNum As String
        ''' <summary>
        ''' 休日名称
        ''' </summary>
        ''' <returns></returns>
        Public Property HolidayName As String = ""
        ''' <summary>
        ''' 過去日フラグ（True:過去日,False：現在,現在日と比べ過去日）
        ''' </summary>
        ''' <returns></returns>
        Public Property IsPastDay As Boolean = False
        ''' <summary>
        ''' 当日以前フラグ(True:当日,False:当日を含まない過去日)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>車数提案の入力可否判定に利用</remarks>
        Public Property IsBeforeToday As Boolean = False
        ''' <summary>
        ''' 画面表示範囲日付(True:表示範囲,False:非表示)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsDispArea As Boolean = False
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="day">格納する日付</param>
        Public Sub New(day As Date)
            Me.KeyString = day.ToString("yyyy/MM/dd")
            Me.DispDate = day.ToString(DISP_DATEFORMAT)
            Me.ItemDate = day
            Me.IsHoliday = False '一旦False、別処理で設定
            Me.WeekNum = CInt(day.DayOfWeek).ToString
            Me.HolidayName = "" '一旦ブランク 別処理で設定
            If Me.KeyString > Now.ToString("yyyy/MM/dd") Then
                Me.IsPastDay = False
                Me.IsBeforeToday = False
            ElseIf Me.KeyString = Now.ToString("yyyy/MM/dd") Then
                Me.IsPastDay = False
                Me.IsBeforeToday = True
            Else
                Me.IsPastDay = True
                Me.IsBeforeToday = True
            End If
        End Sub
    End Class
    ''' <summary>
    ''' 在庫管理表検索データクラス
    ''' </summary>
    ''' <remarks>デモ用ですが画面オブジェクト及び外部の変数へは直接アクセスしなこと
    ''' （コンストラクタや引数で受け渡しさせる、別ファイルに外だしした時もワークするように考慮する）
    ''' 当クラス及びサブクラス内でDB操作をする際はきっちりデストラクタ(Finalize)を仕込む
    ''' 場合によってはUsingをサポートするように記述する</remarks>
    <Serializable>
    Public Class DispDataClass
        Public Const SUMMARY_CODE As String = "Summary"
        ''' <summary>
        ''' 営業所
        ''' </summary>
        ''' <returns></returns>
        Public Property SalesOffice As String = ""
        ''' <summary>
        ''' 営業所名
        ''' </summary>
        ''' <returns></returns>
        Public Property SalesOfficeName As String = ""
        ''' <summary>
        ''' 荷主
        ''' </summary>
        ''' <returns></returns>
        Public Property Shipper As String = ""
        ''' <summary>
        ''' 荷主名
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipperName As String = ""
        ''' <summary>
        ''' 削除時非同期荷主（True：非同期、False：同期する）
        ''' </summary>
        ''' <returns></returns>
        Public Property AsyncDeleteShipper As Boolean = False
        ''' <summary>
        ''' 荷受人（油槽所）
        ''' </summary>
        ''' <returns></returns>
        Public Property Consignee As String = ""
        ''' <summary>
        ''' 荷受人名
        ''' </summary>
        ''' <returns></returns>
        Public Property ConsigneeName As String = ""
        ''' <summary>
        ''' 受注提案タンク車数リストプロパティ
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Key=日付 Value=列車、油種、チェックボックス、受入数を加味したリスト</remarks>
        Public Property SuggestList As New Dictionary(Of String, SuggestItem)
        ''' <summary>
        ''' 画面表示用提案タンク車数(SuggestListの参照)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SuggestListDisplay As Dictionary(Of String, SuggestItem)
            Get
                Dim retVal As Dictionary(Of String, SuggestItem) = Nothing
                Dim qretVal = (From itm In Me.SuggestList Where itm.Value.DayInfo.IsDispArea)
                If qretVal.Any Then
                    retVal = qretVal.ToDictionary(Function(x) x.Key, Function(x) x.Value)
                End If
                Return retVal
            End Get
        End Property


        ''' <summary>
        ''' 油種名のディクショナリ
        ''' </summary>
        ''' <returns></returns>
        Public Property SuggestOilNameList As New Dictionary(Of String, OilItem)
        ''' <summary>
        ''' 比重リストアイテム
        ''' </summary>
        ''' <returns></returns>
        Public Property OilTypeList As Dictionary(Of String, OilItem)
        ''' <summary>
        ''' 列車リストアイテム
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainList As Dictionary(Of String, TrainListItem)
        ''' <summary>
        ''' 列車運行情報リスト
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainOperationList As List(Of TrainOperationItem)
        ''' <summary>
        ''' 在庫一覧日付部分
        ''' </summary>
        ''' <returns></returns>
        Public Property StockDate As Dictionary(Of String, DaysItem)
        ''' <summary>
        ''' 画面表示用在庫一覧日付(StockDateを参照)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StockDateDisplay As Dictionary(Of String, DaysItem)
            Get
                Dim retVal As Dictionary(Of String, DaysItem) = Nothing
                Dim qretVal = (From itm In Me.StockDate Where itm.Value.IsDispArea)
                If qretVal.Any Then
                    retVal = qretVal.ToDictionary(Function(x) x.Key, Function(x) x.Value)
                End If
                Return retVal
            End Get
        End Property
        ''' <summary>
        ''' 在庫一覧データ
        ''' </summary>
        ''' <returns></returns>
        Public Property StockList As Dictionary(Of String, StockListCollection)
        ''' <summary>
        ''' 提案書表示可否(True:表示,False:非表示)
        ''' </summary>
        ''' <returns></returns>
        Public Property ShowSuggestList As Boolean = True
        ''' <summary>
        ''' OT提案表表示(True:OTモードで表示、False：OTモードで表示しない)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsOtTrainMode As Boolean = False
        ''' <summary>
        ''' 構内取りデータ有無(True:あり,False:無し(デフォルト))
        ''' </summary>
        ''' <returns></returns>
        Public Property HasMoveInsideItem As Boolean = False
        ''' <summary>
        ''' 構内取り先営業所
        ''' </summary>
        ''' <returns></returns>
        Public Property MiSalesOffice As String = ""
        ''' <summary>
        ''' 構内取り先営業所名
        ''' </summary>
        ''' <returns></returns>
        Public Property MiSalesOfficeName As String = ""
        ''' <summary>
        ''' 荷主コード
        ''' </summary>
        ''' <returns></returns>
        Public Property MiShippersCode As String = ""
        ''' <summary>
        ''' 荷主名
        ''' </summary>
        ''' <returns></returns>
        Public Property MiShippersName As String = ""
        ''' <summary>
        ''' 構内取り先荷受人（油槽所）
        ''' </summary>
        ''' <returns></returns>
        Public Property MiConsignee As String = ""
        ''' <summary>
        ''' 構内取り先荷受人（油槽所）名
        ''' </summary>
        ''' <returns></returns>
        Public Property MiConsigneeName As String = ""
        ''' <summary>
        ''' 構内取り先情報（当クラスと同構造の子供）
        ''' </summary>
        ''' <returns></returns>
        Public Property MiDispData As DispDataClass
        ''' <summary>
        ''' 帳票用の車数保持クラス（最上位層。キー：油種）
        ''' </summary>
        ''' <returns></returns>
        Public Property PrintTrainNums As Dictionary(Of String, PrintTrainNumCollection)
        ''' <summary>
        ''' 油種別のカウント(構内取りも考慮)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OilTypeCount As Integer
            Get
                Dim retVal As Integer = 0
                If SuggestOilNameList IsNot Nothing Then
                    retVal = SuggestOilNameList.Count
                End If
                If MiDispData IsNot Nothing AndAlso MiDispData.SuggestOilNameList IsNot Nothing Then
                    retVal = retVal + MiDispData.SuggestOilNameList.Count
                End If
                Return retVal
            End Get
        End Property

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="daysList">対象日リスト</param>
        ''' <param name="trainList">列車IDリスト</param>
        ''' <param name="oilTypeList">対象油種リスト</param>
        Public Sub New(daysList As Dictionary(Of String, DaysItem), trainList As Dictionary(Of String, TrainListItem), oilTypeList As Dictionary(Of String, OilItem),
                       officeCode As String, shipper As String, consigneeCode As String)
            Me.SalesOffice = officeCode
            Me.Shipper = shipper
            Me.Consignee = consigneeCode
            Me.TrainList = trainList
            '******************************
            'コンストラクタ引数チェック
            '(一旦呼出し元にスローします)
            '******************************
            '引数が日付に変換できない場合エラー
            If daysList Is Nothing OrElse daysList.Count = 0 Then
                Throw New Exception("baseDay dose not convert to date.")
            End If

            If oilTypeList Is Nothing OrElse oilTypeList.Count = 0 Then
                Throw New Exception("oilCodes is empty.")
            End If
            '提案リストデータ作成有無判定
            If trainList Is Nothing OrElse trainList.Count = 0 Then
                Me.ShowSuggestList = False
            End If
            '引数情報をプロパティに保持
            Me.OilTypeList = oilTypeList
            '******************************
            '提案リストデータ作成処理
            '******************************
            If Me.ShowSuggestList = True Then
                '******************************
                ' 提案リスト縦軸の油種名を生成
                '******************************
                Me.SuggestOilNameList = CreateSuggestOilNameList(oilTypeList)
                '******************************
                ' 基準日～基準日＋7 
                ' 提案リスト
                ' 日付ごとのSuggestItemを生成
                '******************************
                Me.SuggestList = New Dictionary(Of String, SuggestItem)
                For Each dayItm In daysList.Values  'i = 0 To 6
                    '列車Noのループ
                    Dim suggestItem = New SuggestItem(dayItm)
                    For Each trainInfo In trainList.Values
                        suggestItem.Add(trainInfo, Me.SuggestOilNameList)
                    Next trainInfo
                    Me.SuggestList.Add(dayItm.KeyString, suggestItem)
                Next 'dayItm
            End If
            '******************************
            ' 比重リスト生成
            '******************************
            'oilTypeListをそのまま使用
            '******************************
            ' 在庫リスト生成
            '******************************
            '表示用ヘッダー日付生成
            Me.StockDate = daysList
            Me.StockList = New Dictionary(Of String, StockListCollection)
            For Each oilNameItem In Me.OilTypeList
                If oilNameItem.Key = SUMMARY_CODE Then
                    Continue For
                End If
                Dim item As New StockListCollection(oilNameItem.Value, Me.StockDate)
                Me.StockList.Add(oilNameItem.Key, item)
            Next 'oilNameItem
        End Sub
        ''' <summary>
        ''' ENEOS帳票用条件生成コンストラクタ
        ''' </summary>
        Public Sub New(officeCode As String, shipper As String, consigneeCode As String)
            Me.SalesOffice = officeCode
            Me.Shipper = shipper
            Me.Consignee = consigneeCode
        End Sub
        ''' <summary>
        ''' 入力項目を0クリア・チェックボックスを未チェックにするメソッド
        ''' </summary>
        ''' <remarks>初日朝在庫は保持</remarks>
        Public Sub InputValueToZero()
            '提案表クリア
            SuggestValueInputValueToZero()

            '在庫表クリア
            For Each odrItem In Me.StockList.Values
                '初日朝在庫及び、払出はクリアしない
                'ローリー受け入れのみクリアする
                For Each trainIdItem In odrItem.StockItemList.Values
                    If trainIdItem.DaysItem.IsDispArea = False Then
                        Continue For
                    End If
                    trainIdItem.ReceiveFromLorry = "0"
                Next
            Next
        End Sub
        ''' <summary>
        ''' 提案表部分の0クリア
        ''' </summary>
        ''' <remarks>自動提案でも使用するため外だし
        ''' 川崎の入力値をクリアするか
        ''' </remarks>
        ''' 
        Public Sub SuggestValueInputValueToZero(Optional isSkipKawasakiInputs As Boolean = False)
            '提案表クリア
            For Each suggestItm In SuggestList.Values
                If suggestItm.DayInfo.IsDispArea = False OrElse suggestItm.DayInfo.IsBeforeToday Then
                    Continue For
                End If

                For Each odrItem In suggestItm.SuggestOrderItem.Values
                    '川崎スキップフラグがあり列車が川崎の場合スキップ(空回日報取込時にコール)
                    If isSkipKawasakiInputs AndAlso odrItem.TrainInfo.TrainNo = "川崎" Then
                        Continue For
                    End If
                    odrItem.CheckValue = False 'チェックボックスを未チェック
                    odrItem.AccAddDays = ""
                    For Each itm In odrItem.SuggestValuesItem.Values
                        itm.ItemValue = "0" 'テキストをすべて0
                    Next
                Next
            Next suggestItm
        End Sub

        ''' <summary>
        ''' 油種名、油種コードリストを生成
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>合計行の付与</remarks>
        Private Function CreateSuggestOilNameList(oilCodes As Dictionary(Of String, OilItem)) As Dictionary(Of String, OilItem)
            Dim retVal As New Dictionary(Of String, OilItem)
            Dim copiedItem As OilItem
            For Each itm In oilCodes
                copiedItem = itm.Value.Copy
                retVal.Add(itm.Key, copiedItem)
            Next
            '合計行の付与
            retVal.Add(SUMMARY_CODE, New OilItem(SUMMARY_CODE, "合計"))
            Return retVal
        End Function
        ''' <summary>
        ''' (内部メソッド)日付、油種での提案受入数の合計（列車の部分を合計）を取得
        ''' </summary>
        ''' <param name="dateKey">日付(yyyy/MM/dd形式)※受入日</param>
        ''' <param name="oilCode">油種コード</param>
        ''' <returns></returns>

        Private Function GetSummarySuggestValue(dateKey As String, oilCode As String) As Decimal
            Dim retVal As Decimal = 0
            'ありえないが対象の日付データkeyが無ければ例外Throw
            If Me.SuggestList.ContainsKey(dateKey) = False Then
                Throw New Exception(String.Format("提案表データ(Key={0})が未存在", dateKey))
            End If
            For Each daysItems In Me.SuggestList.Values

                For Each tgtItm In daysItems.SuggestOrderItem.Values
                    Dim accDays As Decimal = 0
                    If tgtItm.AccAddDays = "" AndAlso IsNumeric(tgtItm.TrainInfo.AccDays) = False Then
                        Continue For
                    End If

                    If tgtItm.AccAddDays <> "" Then
                        accDays = CDec(tgtItm.AccAddDays) * -1

                    Else
                        accDays = tgtItm.TrainInfo.AccDays * -1
                    End If

                    Dim targetDay As String = CDate(dateKey).AddDays(accDays).ToString("yyyy/MM/dd")
                    If Not daysItems.DayInfo.KeyString = targetDay Then
                        Continue For
                    End If
                    'チェックをしている値のみ合計する
                    If tgtItm.CheckValue = True OrElse tgtItm.CheckValue = False Then '20200529 チェック未チェックでもOK戻しやすいよう両条件入れておく本来このIF不要
                        If tgtItm.SuggestValuesItem.ContainsKey(oilCode) Then
                            '入力値 * 45 / Weight
                            Dim suggestItm = tgtItm.SuggestValuesItem(oilCode)
                            Dim calcVal As Decimal = 0
                            If suggestItm.OilInfo.Weight <> 0 Then
                                calcVal = Math.Floor(Decimal.Parse(suggestItm.ItemValue) * 45 / suggestItm.OilInfo.Weight)
                            End If
                            retVal = retVal + calcVal
                        End If
                    End If
                Next
            Next

            Return retVal
        End Function
        ''' <summary>
        ''' 在庫表再計算処理
        ''' </summary>
        ''' <param name="needsSumSuggestValue">提案数を再計算にい含めるか(True(デフォルト):含める,False:含めない)</param>
        ''' <param name="procOilCode">再計算対象油種、未指定時は全油種処理、指定時はその油種のみ</param>
        ''' <param name="procDay">再計算対象日、未指定時は全日付対象</param>
        ''' <remarks>外部用呼出メソッド</remarks>
        Public Sub RecalcStockList(Optional needsSumSuggestValue As Boolean = True, Optional procOilCode As String = "", Optional procDay As String = "")
            Dim firstDispDay As String = (From itm In Me.StockDate.Values Where itm.IsDispArea Select itm.ItemDate.ToString("yyyy/MM/dd")).First
            '日付毎のループ
            For Each stockListItm In Me.StockList.Values
                '再計算対象油種を指定した場合、一致するまでスキップ
                If procOilCode <> "" AndAlso procOilCode <> stockListItm.OilInfo.OilCode Then
                    Continue For
                End If
                '当日の油種ごとのオブジェクトループ
                For Each itm In stockListItm.StockItemList.Values
                    '再計算対象日を指定した場合、一致するまでスキップ
                    If procDay <> "" AndAlso itm.DaysItem.KeyString <> procDay Then
                        Continue For
                    End If
                    Dim itmDate As String = itm.DaysItem.KeyString
                    Dim oilCode As String = stockListItm.OilTypeCode
                    Dim decSendVal As Decimal = Decimal.Parse(itm.Send)
                    Dim decMorningStockVal As Decimal = Decimal.Parse(itm.MorningStock)
                    '前日日付データ取得
                    Dim prevDayKey As String = itm.DaysItem.ItemDate.AddDays(-1).ToString("yyyy/MM/dd")
                    '前日データを元に実行する処理(前日データあり=一覧初日以外)
                    If stockListItm.StockItemList.ContainsKey(prevDayKey) AndAlso firstDispDay <> itm.DaysItem.ItemDate.ToString("yyyy/MM/dd") Then
                        '前日のデータ
                        Dim prevItm = stockListItm.StockItemList(prevDayKey)
                        '◆1行目 前日夕在庫(前日データの夕在庫フィールドを格納)
                        itm.LastEveningStock = prevItm.EveningStock
                        '◆3行目 朝在庫 ※計算順序に営業するため2行目処理より前に持ってくること
                        decMorningStockVal = Decimal.Parse(prevItm.MorningStock) + prevItm.SummaryReceive - Decimal.Parse(prevItm.Send)
                        itm.MorningStock = decMorningStockVal.ToString
                    Else
                        '画面期間外の前日夕在庫が朝在庫となる
                        'decMorningStockVal = stockListItm.OilInfo.OffScreenLastEveningStock
                        'itm.LastEveningStock = decMorningStockVal
                        'itm.MorningStock = decMorningStockVal.ToString
                    End If
                    '◆保有日数(朝在庫 / 前週出荷平均)
                    If stockListItm.LastShipmentAve = 0 Then
                        itm.Retentiondays = 0
                    Else
                        itm.Retentiondays = Math.Round(decMorningStockVal / stockListItm.LastShipmentAve, 1)
                    End If
                    '◆朝在庫 除DS
                    itm.MorningStockWithoutDS = decMorningStockVal - stockListItm.DS
                    '◆受入数 (提案リストの値)
                    If Me.ShowSuggestList = True AndAlso itm.DaysItem.IsBeforeToday = False AndAlso needsSumSuggestValue Then
                        '提案リスト表示時
                        itm.Receive = GetSummarySuggestValue(itmDate, oilCode).ToString
                    Else
                        'itm.Receive = 0 '？？？？？ここはどうする
                    End If
                    '◆払出
                    '入力項目なので無視
                    '◆夕在庫 (朝在庫 + 受入- 払出)
                    itm.EveningStock = decMorningStockVal + itm.SummaryReceive - decSendVal
                    '◆夕在庫D/S (夕在庫 - D/S)
                    itm.EveningStockWithoutDS = itm.EveningStock - stockListItm.DS
                    '◆空き容量 (夕在庫 -  D/S)
                    itm.FreeSpace = stockListItm.TargetStock - ((decMorningStockVal + itm.SummaryReceive) - decSendVal)
                    '◆在庫率
                    If stockListItm.TargetStock = 0 Then
                        itm.StockRate = 0
                    Else
                        '夕在庫 / 目標在庫
                        itm.StockRate = Math.Round(itm.EveningStock / stockListItm.TargetStock, 3)
                    End If
                Next itm '当日の油種ごとのオブジェクトループ

            Next stockListItm '日付毎のループ

        End Sub
        ''' <summary>
        ''' 自動提案計算処理
        ''' </summary>
        ''' <param name="inventoryDays">在庫維持日数</param>
        ''' <remarks>外部呼出用メソッド</remarks>
        Public Sub AutoSuggest(inventoryDays As Integer)
            '一旦0あり先頭
            Dim fromDay As String = (From itm In Me.StockDate Where itm.Value.IsDispArea).First.Value.KeyString
            Dim toDay As String = (From itm In Me.StockDate Where itm.Value.IsDispArea).First.Value.ItemDate.AddDays(inventoryDays - 1).ToString("yyyy/MM/dd")
            '過去日を除く開始日＋inventryDaysが処理条件
            Dim targetDays = From itm In Me.StockDate
                             Where itm.Key >= fromDay AndAlso
                                   itm.Key <= toDay AndAlso
                                   itm.Value.IsBeforeToday = False AndAlso
                                   itm.Value.IsDispArea
                             Order By itm.Key
                             Select itm.Key
            '処理日付が無ければそのまま終了
            If targetDays.Any = False Then
                Return
            End If
            '一旦提案数を0クリア
            Me.SuggestValueInputValueToZero()
            '一旦0で提案数0で再計算(全体)
            Me.RecalcStockList()
            Dim suggestItem As SuggestItem
            Dim suggestTrainItem As SuggestItem.SuggestValues
            Dim suggestTrainOilValue As SuggestItem.SuggestValue
            Dim finishIncremental As Boolean = False
            Dim freeSpaceLists As Dictionary(Of String, Boolean)
            '処理日付のループ
            For Each targetDay In targetDays
                If Me.SuggestList.ContainsKey(targetDay) = False Then
                    Continue For
                End If
                '対象日の列車・油種別の提案リスト取得
                suggestItem = Me.SuggestList(targetDay)
                'フリースペースが満たされた油種、情報を取得（列車別で無駄ループを無くすため）
                '日付単位で初期化
                freeSpaceLists = OilTypeList.ToDictionary(Function(x) x.Key, Function(x) False)

                '列車別ループ
                For Each trainInfo In Me.TrainList.Values
                    '全油種のフリースペースが無い場合は列車別でループする意味がないので次の日付へ
                    '(全油種の数 = フリースペースが無い油種のカウント)
                    If OilTypeList.Count = (From fslItm In freeSpaceLists Where fslItm.Value = True).Count Then
                        Exit For
                    End If
                    If suggestItem.SuggestOrderItem.ContainsKey(trainInfo.TrainNo) = False Then
                        Continue For
                    End If
                    suggestTrainItem = suggestItem.SuggestOrderItem(trainInfo.TrainNo)
                    '列車ロックがかかっている場合計算しない
                    If suggestTrainItem.TrainLock = True Then
                        Continue For
                    End If
                    'システム管理対象外の列車の場合計算しない
                    If suggestTrainItem.TrainInfo.UnmanagedTrain Then
                        Continue For
                    End If
                    '計算対象チェックをOn
                    suggestTrainItem.CheckValue = True
                    finishIncremental = False
                    '油種別ループ
                    While Not finishIncremental
                        For Each oilItem In Me.OilTypeList.Values
                            '列車最大牽引数を超えたら次の列車へ
                            If suggestTrainItem.CanIncremental = False Then
                                finishIncremental = True
                                Exit For
                            End If
                            '全油種のフリースペースが無い場合インクリメント終了
                            '(全油種の数 = フリースペースが無い油種のカウント)
                            If OilTypeList.Count = (From fslItm In freeSpaceLists Where fslItm.Value = True).Count Then
                                finishIncremental = True
                                Exit For
                            End If
                            '対象油種のフリースペースが無い場合はインクリメント＋再計算せず次の油種
                            If freeSpaceLists(oilItem.OilCode) = True Then
                                Continue For
                            End If
                            suggestTrainOilValue = suggestTrainItem(oilItem.OilCode)
                            Dim currentVal As Decimal = Decimal.Parse(suggestTrainOilValue.ItemValue)
                            suggestTrainOilValue.ItemValue = (currentVal + 1).ToString
                            Me.RecalcStockList(procOilCode:=oilItem.OilCode, procDay:=targetDay)
                            With Me.StockList(oilItem.OilCode).StockItemList(targetDay)
                                '計算の結果空き容量が0の場合はインクリメント前に戻し次の油種へ
                                If .FreeSpace < 0 Then
                                    freeSpaceLists(oilItem.OilCode) = True
                                    suggestTrainOilValue.ItemValue = (currentVal).ToString
                                    Me.RecalcStockList(procOilCode:=oilItem.OilCode, procDay:=targetDay)
                                End If
                            End With
                        Next oilItem 'end 油種別ループ
                    End While
                Next trainInfo 'end 列車別ループ
            Next targetDay 'end 日付別ループ
            '残る日付もある為、全体を再度計算
            Me.RecalcStockList()
            '最終的にチェックをすべて外す(ユーザーに受注作成するデータを選ばせる為）
            For Each sgItm In Me.SuggestList.Values
                For Each trItm In sgItm.SuggestOrderItem.Values
                    trItm.CheckValue = False
                Next trItm
            Next sgItm
        End Sub
        ''' <summary>
        ''' 提案一覧にチェックしているデータが存在するか確認（True:チェック項目あり,False:チェック項目なし)
        ''' </summary>
        ''' <returns></returns>
        Public Function HasSuggestCheckedItem() As Boolean
            Dim retVal As Boolean = False

            For Each daysItm In Me.SuggestList.Values
                If (From trainItm In daysItm.SuggestOrderItem.Values Where trainItm.CheckValue).Any Then
                    retVal = True
                    Exit For
                End If
            Next daysItm
            Return retVal
        End Function
        ''' <summary>
        ''' チェックのついた提案表一覧を取得
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSuggestCheckedItem() As List(Of SelectedSuggestValItem)
            Dim retVal As New List(Of SelectedSuggestValItem)
            Dim itm As SelectedSuggestValItem
            '上位日付のループ
            For Each daysItm In Me.SuggestList.Values
                '列車毎のループ
                For Each trainItm In daysItm.SuggestOrderItem.Values
                    '列車にチェックついていないデータは対象がいの為スキップ
                    If trainItm.CheckValue = False Then
                        Continue For
                    End If
                    '管理対象外の列車の場合は不整合データを受注登録させないためスキップ
                    If trainItm.TrainInfo.UnmanagedTrain Then
                        Continue For
                    End If
                    itm = New SelectedSuggestValItem
                    itm.dayInfo = daysItm.DayInfo
                    itm.trainInfo = trainItm.TrainInfo
                    itm.AccAddDays = trainItm.AccAddDays
                    itm.SuggestOrderItem = trainItm.SuggestValuesItem
                    retVal.Add(itm)
                Next trainItm
            Next daysItm
            Return retVal
        End Function
        ''' <summary>
        ''' 印刷用列車数リストの初期化メソッド
        ''' </summary>
        Public Sub SetPrintTrainNumList()
            Me.PrintTrainNums = New Dictionary(Of String, PrintTrainNumCollection)
            For Each oilItm In Me.OilTypeList

            Next oilItm

        End Sub
        ''' <summary>
        ''' 列車Noをキーに持つ受注提案アイテム
        ''' </summary>
        <Serializable>
        Public Class SuggestItem
            ''' <summary>
            ''' 日付情報クラス
            ''' </summary>
            ''' <returns></returns>
            Public Property DayInfo As DaysItem
            ''' <summary>
            ''' 受入数情報格納用ディクショナリ(Key=列車No,Value=一覧の値クラス)
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>Key=列車No,Value=一覧の値クラス</remarks>
            Public Property SuggestOrderItem As Dictionary(Of String, SuggestValues)
            ''' <summary>
            ''' 構内取り受入数情報格納用ディクショナリ（参照渡し）
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>本体は親クラスのMiDispDataであること</remarks>
            Public Property SuggestMiOrderItem As Dictionary(Of String, SuggestValues)

            ''' <summary>
            ''' 【未使用】積置き情報格納用ディクショナリ
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>未使用 一旦残すがしばらくしたら消す</remarks>
            Public Property SuggestLoadingItem As Dictionary(Of String, SuggestValues)
            ''' <summary>
            ''' 列車情報クラス
            ''' </summary>
            ''' <returns></returns>
            Public Property TrainInfo As TrainListItem
            ''' <summary>
            ''' コンストラクタ
            ''' </summary>
            ''' <param name="targetDate">日付情報クラス</param>
            Public Sub New(targetDate As DaysItem)
                Me.DayInfo = targetDate
                Me.SuggestOrderItem = New Dictionary(Of String, SuggestValues)
            End Sub
            ''' <summary>
            ''' 提案データ追加メソッド
            ''' </summary>
            ''' <param name="trainInfo">列車情報クラス</param>
            ''' <param name="oilCodes">油種情報コレクション</param>
            Public Sub Add(trainInfo As TrainListItem, oilCodes As Dictionary(Of String, OilItem))
                Dim orderValues = New SuggestValues
                orderValues.TrainInfo = trainInfo
                For Each oilCodeItem In oilCodes.Values
                    orderValues.Add(oilCodeItem, "0", Me.DayInfo, trainInfo)
                Next
                'orderValues.Add(New OilItem(SUMMARY_CODE, "合計"), "0", Me.DayInfo)
                Me.SuggestOrderItem.Add(trainInfo.TrainNo, orderValues)
                Me.TrainInfo = trainInfo

            End Sub
            ''' <summary>
            ''' 構内取りデータの紐づけ
            ''' </summary>
            Public Sub RelateMoveInside()
                For Each suggestOrder In Me.SuggestOrderItem
                    Dim itm = Me.SuggestMiOrderItem(suggestOrder.Key)
                    suggestOrder.Value.MiSuggestValuesItem = itm.SuggestValuesItem
                Next
            End Sub
            ''' <summary>
            ''' 受注提案キー情報保持
            ''' </summary>
            <Serializable>
            Public Structure SuggestKeys
                ''' <summary>
                ''' インデックス
                ''' </summary>
                Public Index As Integer
                ''' <summary>
                ''' 積込日
                ''' </summary>
                Public LodDate As String

                ''' <summary>
                ''' 自身に格納している情報をBase64情報に変換
                ''' </summary>
                ''' <returns></returns>
                Public Function GetKeyString() As String
                    Dim enc As Encoding = Encoding.GetEncoding("UFT-8")
                    Dim key As String = LodDate & "@" & Index
                    Return Convert.ToBase64String(enc.GetBytes(key))
                End Function
                ''' <summary>
                ''' Base64変換したキー情報を当構造体形式にデコード
                ''' </summary>
                ''' <param name="keyString"></param>
                ''' <returns></returns>
                Public Shared Function GetDecKeyString(keyString As String) As SuggestKeys
                    Dim enc As Encoding = Encoding.GetEncoding("UFT-8")
                    Dim index As Integer = 0
                    Dim lodDate As String = ""
                    If lodDate = "" Then
                        Return Nothing
                    End If
                    Dim decodedString = enc.GetString(Convert.FromBase64String(keyString))
                    Dim splitVal = decodedString.Split("@"c)
                    If Not (splitVal IsNot Nothing AndAlso splitVal.Length > 2 AndAlso IsNumeric(splitVal(1))) Then
                        lodDate = splitVal(0)
                        index = CInt(splitVal(1))
                        Return GetNewSuggestKey(index, lodDate)
                    Else
                        Return Nothing
                    End If

                End Function
                ''' <summary>
                ''' ２値を持つ構造体に変換
                ''' </summary>
                ''' <param name="index"></param>
                ''' <param name="lodDate"></param>
                ''' <returns></returns>
                Public Shared Function GetNewSuggestKey(index As Integer, lodDate As String) As SuggestKeys
                    Return New SuggestKeys With {.LodDate = lodDate, .Index = index}
                End Function
            End Structure
            ''' <summary>
            ''' 受注提案タンク車数用数値情報格納クラス
            ''' </summary>
            <Serializable>
            Public Class SuggestValues
                ''' <summary>
                ''' 受注提案タンク車数用数値情報ディクショナリ
                ''' </summary>
                ''' <returns></returns>
                Public Property SuggestValuesItem As Dictionary(Of String, SuggestValue)
                ''' <summary>
                ''' 構内受け用受注提案タンク車数用数値情報ディクショナリ
                ''' </summary>
                ''' <returns></returns>
                Public Property MiSuggestValuesItem As Dictionary(Of String, SuggestValue)
                ''' <summary>
                ''' 提案表チェックボックスチェック状態
                ''' </summary>
                ''' <returns></returns>
                Public Property CheckValue As Boolean = False
                ''' <summary>
                ''' 列車使用ロック(True:使用不可,False:使用可
                ''' </summary>
                ''' <returns></returns>
                Public Property TrainLock As Boolean = False
                ''' <summary>
                ''' 列車情報クラス
                ''' </summary>
                ''' <returns></returns>
                Public Property TrainInfo As TrainListItem
                ''' <summary>
                ''' 先頭アイテム
                ''' </summary>
                ''' <returns></returns>
                Public Property IsFirstLodDate As Boolean = True
                ''' <summary>
                ''' 加算受入日数（画面上の提案表ドロップダウン）
                ''' </summary>
                ''' <returns></returns>
                Public Property AccAddDays As String = ""
                ''' <summary>
                ''' デフォルトプロパティ
                ''' </summary>
                ''' <param name="oilCode"></param>
                ''' <returns></returns>
                Default Public Property _item(oilCode As String) As SuggestValue
                    Get
                        Return Me.SuggestValuesItem(oilCode)
                    End Get
                    Set(value As SuggestValue)
                        Me.SuggestValuesItem(oilCode) = value
                    End Set
                End Property
                ''' <summary>
                ''' コンストラクタ
                ''' </summary>
                Public Sub New()
                    Me.SuggestValuesItem = New Dictionary(Of String, SuggestValue)
                End Sub
                ''' <summary>
                ''' アイテム追加メソッド
                ''' </summary>
                ''' <param name="oilInfo">油種情報クラス</param>
                ''' <param name="val">計算値</param>
                ''' <param name="dayItm">日付情報</param>
                Public Sub Add(oilInfo As OilItem, val As String, dayItm As DaysItem, trainInfo As TrainListItem)
                    Me.SuggestValuesItem.Add(oilInfo.OilCode, New SuggestValue _
                        With {.ItemValue = val, .OilInfo = oilInfo, .DayInfo = dayItm, .TrainInfo = trainInfo})
                End Sub
                ''' <summary>
                ''' 列車最大牽引車数をオーバーせず追加できるか(True:追加可能,False：追加不可)
                ''' </summary>
                ''' <returns></returns>
                Public Function CanIncremental() As Boolean
                    Dim summaryNum As Decimal = (From itm In Me.SuggestValuesItem
                                                 Where itm.Key <> SUMMARY_CODE
                                                 Select If(IsNumeric(itm.Value.ItemValue), Decimal.Parse(itm.Value.ItemValue), 0D)
                                                 ).Sum
                    If Me.TrainInfo.MaxVolume <= summaryNum Then
                        Return False
                    Else
                        Return True
                    End If
                End Function
            End Class
            ''' <summary>
            ''' 提案値クラス
            ''' </summary>
            <Serializable>
            Public Class SuggestValue
                ''' <summary>
                ''' 油種コード
                ''' </summary>
                ''' <returns></returns>
                Public Property OilCode As String = ""
                Public Property OilInfo As OilItem = Nothing
                ''' <summary>
                ''' 数
                ''' </summary>
                ''' <returns></returns>
                ''' <remarks>画面入力項目の為String</remarks>
                Public Property ItemValue As String = "0"
                ''' <summary>
                ''' 受入数テキストボックスのID
                ''' </summary>
                ''' <returns></returns>
                ''' <remarks>画面情報収集後に設定（初期は未設定なので使用注意）</remarks>
                Public Property ItemValueTextBoxClientId As String = ""
                ''' <summary>
                ''' 日付情報
                ''' </summary>
                ''' <returns></returns>
                Public Property DayInfo As DaysItem = Nothing
                ''' <summary>
                ''' 列車情報
                ''' </summary>
                ''' <returns></returns>
                Public Property TrainInfo As TrainListItem
                ''' <summary>
                ''' 基準日（LodDateから受入日を算出するための日数）
                ''' </summary>
                ''' <returns></returns>
                Public Property AccDays As Integer = 0
                ''' <summary>
                ''' 何もしない場合の初期のAcc日数
                ''' </summary>
                ''' <returns></returns>
                Public Property DafaultAccDate As String = ""
            End Class
        End Class
        ''' <summary>
        ''' 在庫クラス
        ''' </summary>
        <Serializable>
        Public Class StockListCollection
            ''' <summary>
            ''' コンストラクタ
            ''' </summary>
            Public Sub New(oilTypeItem As OilItem,
                           dateItem As Dictionary(Of String, DaysItem))
                Me.OilTypeCode = oilTypeItem.OilCode
                Me.OilTypeName = oilTypeItem.OilName
                Me.OilInfo = oilTypeItem
                '２列目から４列目のタンク容量～前週出荷平均については
                '一旦0
                Me.TankCapacity = oilTypeItem.MaxTankCap
                Me.TargetStock = Math.Round(oilTypeItem.MaxTankCap * oilTypeItem.TankCapRate, 1)
                Me.TargetStockRate = oilTypeItem.TankCapRate
                Me.Stock80 = Math.Round((oilTypeItem.MaxTankCap - oilTypeItem.DS) * 0.8D, 1)
                Me.DS = oilTypeItem.DS
                Me.LastShipmentAve = oilTypeItem.LastSendAverage
                Me.StockItemList = New Dictionary(Of String, StockListItem)
                For Each dateVal In dateItem
                    Dim item = New StockListItem(dateVal.Key, dateVal.Value)
                    'If dateItem.Keys(0) = dateVal.Key Then
                    '    item.LastEveningStock = oilTypeItem.OffScreenLastEveningStock
                    '    item.MorningStock = oilTypeItem.OffScreenLastEveningStock.ToString
                    'End If
                    Me.StockItemList.Add(dateVal.Key, item)

                Next
            End Sub
            ''' <summary>
            ''' 油種情報クラス
            ''' </summary>
            ''' <returns></returns>
            Public Property OilInfo As OilItem = Nothing

            ''' <summary>
            ''' 油種コード
            ''' </summary>
            ''' <returns></returns>
            Public Property OilTypeCode As String = ""
            ''' <summary>
            ''' 油種名
            ''' </summary>
            ''' <returns></returns>
            Public Property OilTypeName As String = ""
            ''' <summary>
            ''' タンク容量
            ''' </summary>
            ''' <returns></returns>
            Public Property TankCapacity As Decimal
            ''' <summary>
            ''' 目標在庫
            ''' </summary>
            ''' <returns></returns>
            Public Property TargetStock As Decimal
            ''' <summary>
            ''' 目標在庫率
            ''' </summary>
            ''' <returns></returns>
            Public Property TargetStockRate As Decimal
            ''' <summary>
            ''' 80%在庫
            ''' </summary>
            ''' <returns></returns>
            Public Property Stock80 As Decimal
            ''' <summary>
            ''' D/S
            ''' </summary>
            ''' <returns></returns>
            Public Property DS As Decimal
            ''' <summary>
            ''' 前週出荷平均
            ''' </summary>
            ''' <returns></returns>
            Public Property LastShipmentAve As Decimal
            ''' <summary>
            ''' 日付別の在庫データ
            ''' </summary>
            ''' <returns></returns>
            Public Property StockItemList As Dictionary(Of String, StockListItem)
            ''' <summary>
            ''' 画面表示範囲の日付別の在庫データ(StockItemList参照)
            ''' </summary>
            ''' <returns></returns>
            Public ReadOnly Property StockItemListDisplay As Dictionary(Of String, StockListItem)
                Get
                    Dim retVal As Dictionary(Of String, StockListItem) = Nothing
                    Dim qretVal = (From itm In Me.StockItemList Where itm.Value.DaysItem.IsDispArea)
                    If qretVal.Any Then
                        retVal = qretVal.ToDictionary(Function(x) x.Key, Function(x) x.Value)
                    End If
                    Return retVal
                End Get
            End Property
        End Class
        <Serializable>
        Public Class StockListItem
            ''' <summary>
            ''' コンストラクタ
            ''' </summary>
            Public Sub New(dispDate As String, dayItm As DaysItem)
                Me.DaysItem = dayItm
                'Demo用、実際イメージ沸いてから値のコンストラクタ引数追加など仕込み方は考える
                Me.LastEveningStock = 12345
                Me.Retentiondays = 0
                Me.MorningStock = "0"
                Me.Receive = "0"
                Me.ReceiveFromLorry = "0"
                Me.Send = "0" '画面入力項目の為文字
                Me.EveningStock = 0
                Me.EveningStockWithoutDS = 0
                Me.FreeSpace = 0
                Me.StockRate = 0
                Me.Print1stPositionVal = 0D
                Me.Print2ndPositionVal = 0D
                Me.Print3rdPositionVal = 0D
            End Sub
            ''' <summary>
            ''' 日付情報クラス
            ''' </summary>
            ''' <returns></returns>
            Public Property DaysItem As DaysItem
            ''' <summary>
            ''' 前日夕在庫
            ''' </summary>
            ''' <returns></returns>
            Public Property LastEveningStock As Decimal
            ''' <summary>
            ''' 保有日数
            ''' </summary>
            ''' <returns></returns>
            Public Property Retentiondays As Decimal
            ''' <summary>
            ''' 朝在庫
            ''' </summary>
            ''' <returns></returns>
            Public Property MorningStock As String
            ''' <summary>
            ''' 朝在庫(画面入力エリアのテキストボックスID)
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>画面情報収集後に設定（初期は未設定なので使用注意）</remarks>
            Public Property MorningStockClientId As String
            ''' <summary>
            ''' 朝在庫D/S除
            ''' </summary>
            ''' <returns></returns>
            Public Property MorningStockWithoutDS As Decimal
            ''' <summary>
            ''' 受入
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>シミュレーション値を格納</remarks>
            Public Property Receive As String
            ''' <summary>
            ''' 受入(画面入力エリアのテキストボックスID)
            ''' </summary>
            ''' <returns></returns>
            Public Property ReceiveClientId As String
            ''' <summary>
            ''' ローリー受入
            ''' </summary>
            ''' <returns></returns>
            Public Property ReceiveFromLorry As String
            ''' <summary>
            ''' ローリー受入(画面入力エリアのテキストボックスID)
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>画面情報収集後に設定（初期は未設定なので使用注意）</remarks>
            Public Property ReceiveFromLorryClientId As String
            ''' <summary>
            ''' 受入合計
            ''' </summary>
            ''' <returns></returns>
            Public ReadOnly Property SummaryReceive As Decimal
                Get
                    Dim retVal As Decimal
                    retVal = If(IsNumeric(Me.Receive), CDec(Me.Receive), 0)
                    Dim lorryVal As Decimal = 0
                    If Decimal.TryParse(Me.ReceiveFromLorry, lorryVal) Then
                        retVal = retVal + lorryVal
                    End If
                    Return retVal
                End Get
            End Property

            ''' <summary>
            ''' 払出(画面入力エリアの為文字列)
            ''' </summary>
            ''' <returns></returns>
            Public Property Send As String
            ''' <summary>
            ''' 払出(画面入力エリアのテキストボックスID)
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>画面情報収集後に設定（初期は未設定なので使用注意）</remarks>
            Public Property SendTextClientId As String
            ''' <summary>
            ''' 夕在庫
            ''' </summary>
            ''' <returns></returns>
            Public Property EveningStock As Decimal
            ''' <summary>
            ''' 夕在庫D/S除
            ''' </summary>
            ''' <returns></returns>
            Public Property EveningStockWithoutDS As Decimal
            ''' <summary>
            ''' 空容量
            ''' </summary>
            ''' <returns></returns>
            Public Property FreeSpace As Decimal
            ''' <summary>
            ''' 在庫率
            ''' </summary>
            ''' <returns></returns>
            Public Property StockRate As Decimal
            ''' <summary>
            ''' Eneos帳票用項目1(北信：5463列車、甲府：81列車/先返し)
            ''' </summary>
            ''' <returns></returns>
            Public Property Print1stPositionVal As Decimal
            ''' <summary>
            ''' Eneos帳票用項目2(北信：2085列車、甲府：81・83列車)
            ''' </summary>
            ''' <returns></returns>
            Public Property Print2ndPositionVal As Decimal
            ''' <summary>
            ''' eneos帳票用項目2(北信：8471列車、甲府：83列車)
            ''' </summary>
            ''' <returns></returns>
            Public Property Print3rdPositionVal As Decimal
        End Class
        ''' <summary>
        ''' 選択されたデータを格納するオーダー
        ''' </summary>
        Public Class SelectedSuggestValItem
            Public Property trainInfo As TrainListItem
            Public Property dayInfo As DaysItem
            Public Property AccAddDays As String
            ''' <summary>
            ''' 受入数情報格納用ディクショナリ(Key=列車No,Value=一覧の値クラス)
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>Key=列車No,Value=一覧の値クラス</remarks>
            Public Property SuggestOrderItem As Dictionary(Of String, SuggestItem.SuggestValue)
        End Class
    End Class
    ''' <summary>
    ''' 列車運行情報アイテムクラス
    ''' </summary>
    <Serializable>
    Public Class TrainOperationItem
        ''' <summary>
        ''' 営業所コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String
        ''' <summary>
        ''' JOT列車番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainNo As String
        ''' <summary>
        ''' 運行日(yyyy/MM/dd形式)
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkingDate As String
        ''' <summary>
        ''' 積込フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Tsumi As String
        ''' <summary>
        ''' 発駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property DepStation As String
        ''' <summary>
        ''' 着駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrStation As String
        ''' <summary>
        ''' 稼働フラグ(0:非稼働  1:稼働)
        ''' </summary>
        ''' <returns></returns>
        Public Property [Run] As String

    End Class
    ''' <summary>
    ''' 受注テーブルアイテムクラス
    ''' </summary>
    ''' <remarks>受注テーブルと側を合わせます※ぶら下がる詳細を除き</remarks>
    Public Class OrderItem
        ''' <summary>
        ''' 受注テーブル更新アクション列挙体
        ''' </summary>
        Public Enum OrderItemEntryType
            ''' <summary>
            ''' 追加
            ''' </summary>
            Insert = 0
            ''' <summary>
            '''　更新
            ''' </summary>
            Update = 1
            ''' <summary>
            ''' 何もしない(既登録全車数0 = 画面全車数0や
            ''' 全油種の車数が既登録、画面ともに一致している場合は意味がないのでスキップするためのフラグ)
            ''' </summary>
            None = 999
        End Enum
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="orderNo">オーダーNo</param>
        ''' <param name="dispDataClass">画面表示クラス</param>
        ''' <param name="chkItm">チェック済アイテム</param>
        ''' <remarks>受注データ0の場合に新規作成するオーダー情報</remarks>
        Public Sub New(orderNo As String, dispDataClass As DispDataClass, chkItm As DispDataClass.SelectedSuggestValItem, procDtm As Date, userID As String, termId As String)
            Me.OrderNo = orderNo
            Me.TrainNo = chkItm.trainInfo.TrainNo
            Me.TrainName = chkItm.trainInfo.TrainName
            Me.OrderYmd = Now.ToString("yyyy/MM/dd")
            Me.OfficeCode = dispDataClass.SalesOffice
            Me.OfficeName = dispDataClass.SalesOfficeName
            Me.OrderType = chkItm.trainInfo.PatCode
            Me.ShippersCode = dispDataClass.Shipper
            Me.ShippersName = dispDataClass.ShipperName
            Me.BaseCode = chkItm.trainInfo.PlantCode
            Me.BaseName = chkItm.trainInfo.PlantName
            Me.ConsigneeCode = dispDataClass.Consignee
            Me.ConsigneeName = dispDataClass.ConsigneeName
            Me.DepStation = chkItm.trainInfo.DepStation
            Me.DepStationName = chkItm.trainInfo.DepStationName
            Me.ArrStation = chkItm.trainInfo.ArrStation
            Me.ArrStationName = chkItm.trainInfo.ArrStationName
            Me.RetStation = "" '空車着駅コード(ブランク)
            Me.RetStationName = "" '空車着駅名(ブランク)
            Me.ChangeRetStation = "" '空車着駅コード（変更後）(ブランク)
            Me.ChangeRetStationName = "" '空車着駅名（変更後）
            Me.OrderStatus = CONST_ORDERSTATUS_100 '受注進行ステータス(「100:受注受付」固定)
            Me.OrderInfo = "" '受注情報(ブランク)
            Me.StackingFlg = chkItm.trainInfo.StackingFlg
            Me.EmptyTurnFlg = "2" '０：未作成、１：作成、2：在庫から作成
            Me.UseProprietyFlg = "1" '利用可否フラグ(「1:利用可」固定)
            Me.ContactFlg = "0" '手配連絡フラグ(「０：未連絡」固定)
            Me.ResultFlg = "0" '結果受理フラグ(「０：未受理」固定)
            Me.DeliveryFlg = "0" '託送指示フラグ(「0:未手配」固定)
            Me.DeliveryCount = "0"
            '基準日を受入予定日より逆算
            'Dim baseDate = chkItm.dayInfo.ItemDate.AddDays(chkItm.trainInfo.AccDays * -1)
            Dim baseDate = chkItm.dayInfo.ItemDate
            '受入予定日を除き基準日より計算
            Me.LodDate = baseDate.ToString("yyyy/MM/dd") 'ACCDATEから逆算
            Me.DepDate = baseDate.AddDays(chkItm.trainInfo.DepDays).ToString("yyyy/MM/dd") 'ACCDATEから逆算
            Me.ArrDate = baseDate.AddDays(chkItm.trainInfo.ArrDays).ToString("yyyy/MM/dd") 'ACCDATEから逆算
            Dim appendScale = 0D
            If chkItm.AccAddDays <> "" Then
                Dim addDayVal As Decimal = CDec(chkItm.AccAddDays)
                appendScale = addDayVal - chkItm.trainInfo.AccDays
            End If
            Me.AccDate = baseDate.AddDays(chkItm.trainInfo.AccDays + appendScale).ToString("yyyy/MM/dd") '受入予定日
            Me.EmpArrDate = baseDate.AddDays(chkItm.trainInfo.EmpArrDays + appendScale).ToString("yyyy/MM/dd") 'ACCDATEから算出
            '実績日は埋めない
            Me.ActualLodDate = ""
            Me.ActualDepDate = ""
            Me.ActualArrDate = ""
            Me.ActualAccDate = ""
            Me.ActualEmpArrDate = ""
            '数量関係は詳細データを作る際に合わせて算出
            Me.RTank = "0"
            Me.HTank = "0"
            Me.TTank = "0"
            Me.MTtank = "0"
            Me.KTank = "0"
            Me.K3Tank = "0"
            Me.K5Tank = "0"
            Me.K10Tank = "0"
            Me.LTank = "0"
            Me.ATank = "0"
            Me.Other1Otank = "0"
            Me.Other2OTank = "0"
            Me.Other3OTank = "0"
            Me.Other4OTank = "0"
            Me.Other5OTank = "0"
            Me.Other6OTank = "0"
            Me.Other7OTank = "0"
            Me.Other8OTank = "0"
            Me.Other9OTank = "0"
            Me.Other10OTank = "0"
            Me.TotalTank = "0"
            Me.RTankCh = "0"
            Me.HTankCh = "0"
            Me.TTankCh = "0"
            Me.MtTankCh = "0"
            Me.KTankCh = "0"
            Me.K3TankCh = "0"
            Me.K5TankCh = "0"
            Me.K10TankCh = "0"
            Me.LTankCh = "0"
            Me.ATankCh = "0"
            Me.Other1OTankCh = "0"
            Me.Other2OTankCh = "0"
            Me.Other3OTankCh = "0"
            Me.Other4OTankCh = "0"
            Me.Other5OTankCh = "0"
            Me.Other6OTankCh = "0"
            Me.Other7OTankCh = "0"
            Me.Other8OTankCh = "0"
            Me.Other9OTankCh = "0"
            Me.Other10OTankCh = "0"
            Me.TotalTankCh = "0"
            Me.TankLinkNo = "" '貨車連結順序表№(ブランク)
            Me.TankLinkNoMade = "" '作成_貨車連結順序表№(ブランク → null）
            Me.BillingNo = ""
            Me.KeijyoYmd = "" '計上日(ブランク)
            '金額系はオール0
            Me.Salse = "0"
            Me.SalseTax = "0"
            Me.TotalSalse = "0"
            Me.Payment = "0"
            Me.PaymentTax = "0"
            Me.TotalPayment = "0"
            Me.OtFileName = ""
            Me.ReceiveCount = ""
            Me.OtSendStatus = "0"
            Me.ReservedStatus = "0"
            Me.TakusouStatus = "0"
            Me.BTrainNo = ""
            Me.BTrainName = ""
            Me.AnaSyoriFlg = ""
            Me.DelFlg = C_DELETE_FLG.ALIVE
            Me.InitYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
            Me.InitUser = userID
            Me.InitTermId = termId
            Me.UpdYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
            Me.UpdUser = userID
            Me.UpdTermId = termId
            Me.ReceiveYmd = CONST_DEFAULT_RECEIVEYMD
            'オーダー詳細部分はインスタンス生成のみ、作成は後続処理で行う
            Me.DetailList = New List(Of OrderDetailItem)
            'DB登録処理アクションはInsert
            Me.EntryType = OrderItemEntryType.Insert
            Me.JointOrder = "0"
            Me.SecondConsigneeOrder = "0"
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="sqlDr">受注テーブルのレコード</param>
        Public Sub New(sqlDr As SqlDataReader)
            If sqlDr Is Nothing Then
                Return
            End If
            'SQLDRの各フィールド値をプロパティにセット
            Me.OrderNo = Convert.ToString(sqlDr("ORDERNO"))
            Me.TrainNo = Convert.ToString(sqlDr("TRAINNO"))
            Me.TrainName = Convert.ToString(sqlDr("TRAINNAME"))
            Me.OrderYmd = Convert.ToString(sqlDr("ORDERYMD"))
            Me.OfficeCode = Convert.ToString(sqlDr("OFFICECODE"))
            Me.OfficeName = Convert.ToString(sqlDr("OFFICENAME"))
            Me.OrderType = Convert.ToString(sqlDr("ORDERTYPE"))
            Me.ShippersCode = Convert.ToString(sqlDr("SHIPPERSCODE"))
            Me.ShippersName = Convert.ToString(sqlDr("SHIPPERSNAME"))
            Me.BaseCode = Convert.ToString(sqlDr("BASECODE"))
            Me.BaseName = Convert.ToString(sqlDr("BASENAME"))
            Me.ConsigneeCode = Convert.ToString(sqlDr("CONSIGNEECODE"))
            Me.ConsigneeName = Convert.ToString(sqlDr("CONSIGNEENAME"))
            Me.DepStation = Convert.ToString(sqlDr("DEPSTATION"))
            Me.DepStationName = Convert.ToString(sqlDr("DEPSTATIONNAME"))
            Me.ArrStation = Convert.ToString(sqlDr("ARRSTATION"))
            Me.ArrStationName = Convert.ToString(sqlDr("ARRSTATIONNAME"))
            Me.RetStation = Convert.ToString(sqlDr("RETSTATION"))
            Me.RetStationName = Convert.ToString(sqlDr("RETSTATIONNAME"))
            Me.ChangeRetStation = Convert.ToString(sqlDr("CHANGERETSTATION"))
            Me.ChangeRetStationName = Convert.ToString(sqlDr("CHANGERETSTATIONNAME"))
            Me.OrderStatus = Convert.ToString(sqlDr("ORDERSTATUS"))
            Me.OrderInfo = Convert.ToString(sqlDr("ORDERINFO"))
            Me.EmptyTurnFlg = Convert.ToString(sqlDr("EMPTYTURNFLG"))
            Me.StackingFlg = Convert.ToString(sqlDr("STACKINGFLG"))
            Me.UseProprietyFlg = Convert.ToString(sqlDr("USEPROPRIETYFLG"))
            Me.ContactFlg = Convert.ToString(sqlDr("CONTACTFLG"))
            Me.ResultFlg = Convert.ToString(sqlDr("RESULTFLG"))
            Me.DeliveryFlg = Convert.ToString(sqlDr("DELIVERYFLG"))
            Me.DeliveryCount = Convert.ToString(sqlDr("DELIVERYCOUNT"))
            Me.LodDate = Convert.ToString(sqlDr("LODDATE"))
            Me.DepDate = Convert.ToString(sqlDr("DEPDATE"))
            Me.ArrDate = Convert.ToString(sqlDr("ARRDATE"))
            Me.AccDate = Convert.ToString(sqlDr("ACCDATE"))
            Me.EmpArrDate = Convert.ToString(sqlDr("EMPARRDATE"))
            Me.ActualLodDate = Convert.ToString(sqlDr("ACTUALLODDATE"))
            Me.ActualDepDate = Convert.ToString(sqlDr("ACTUALDEPDATE"))
            Me.ActualArrDate = Convert.ToString(sqlDr("ACTUALARRDATE"))
            Me.ActualAccDate = Convert.ToString(sqlDr("ACTUALACCDATE"))
            Me.ActualEmpArrDate = Convert.ToString(sqlDr("ACTUALEMPARRDATE"))
            Me.RTank = Convert.ToString(sqlDr("RTANK"))
            Me.HTank = Convert.ToString(sqlDr("HTANK"))
            Me.TTank = Convert.ToString(sqlDr("TTANK"))
            Me.MTtank = Convert.ToString(sqlDr("MTTANK"))
            Me.KTank = Convert.ToString(sqlDr("KTANK"))
            Me.K3Tank = Convert.ToString(sqlDr("K3TANK"))
            Me.K5Tank = Convert.ToString(sqlDr("K5TANK"))
            Me.K10Tank = Convert.ToString(sqlDr("K10TANK"))
            Me.LTank = Convert.ToString(sqlDr("LTANK"))
            Me.ATank = Convert.ToString(sqlDr("ATANK"))
            Me.Other1Otank = Convert.ToString(sqlDr("OTHER1OTANK"))
            Me.Other2OTank = Convert.ToString(sqlDr("OTHER2OTANK"))
            Me.Other3OTank = Convert.ToString(sqlDr("OTHER3OTANK"))
            Me.Other4OTank = Convert.ToString(sqlDr("OTHER4OTANK"))
            Me.Other5OTank = Convert.ToString(sqlDr("OTHER5OTANK"))
            Me.Other6OTank = Convert.ToString(sqlDr("OTHER6OTANK"))
            Me.Other7OTank = Convert.ToString(sqlDr("OTHER7OTANK"))
            Me.Other8OTank = Convert.ToString(sqlDr("OTHER8OTANK"))
            Me.Other9OTank = Convert.ToString(sqlDr("OTHER9OTANK"))
            Me.Other10OTank = Convert.ToString(sqlDr("OTHER10OTANK"))
            Me.TotalTank = Convert.ToString(sqlDr("TOTALTANK"))
            Me.RTankCh = Convert.ToString(sqlDr("RTANKCH"))
            Me.HTankCh = Convert.ToString(sqlDr("HTANKCH"))
            Me.TTankCh = Convert.ToString(sqlDr("TTANKCH"))
            Me.MtTankCh = Convert.ToString(sqlDr("MTTANKCH"))
            Me.KTankCh = Convert.ToString(sqlDr("KTANKCH"))
            Me.K3TankCh = Convert.ToString(sqlDr("K3TANKCH"))
            Me.K5TankCh = Convert.ToString(sqlDr("K5TANKCH"))
            Me.K10TankCh = Convert.ToString(sqlDr("K10TANKCH"))
            Me.LTankCh = Convert.ToString(sqlDr("LTANKCH"))
            Me.ATankCh = Convert.ToString(sqlDr("ATANKCH"))
            Me.Other1OTankCh = Convert.ToString(sqlDr("OTHER1OTANKCH"))
            Me.Other2OTankCh = Convert.ToString(sqlDr("OTHER2OTANKCH"))
            Me.Other3OTankCh = Convert.ToString(sqlDr("OTHER3OTANKCH"))
            Me.Other4OTankCh = Convert.ToString(sqlDr("OTHER4OTANKCH"))
            Me.Other5OTankCh = Convert.ToString(sqlDr("OTHER5OTANKCH"))
            Me.Other6OTankCh = Convert.ToString(sqlDr("OTHER6OTANKCH"))
            Me.Other7OTankCh = Convert.ToString(sqlDr("OTHER7OTANKCH"))
            Me.Other8OTankCh = Convert.ToString(sqlDr("OTHER8OTANKCH"))
            Me.Other9OTankCh = Convert.ToString(sqlDr("OTHER9OTANKCH"))
            Me.Other10OTankCh = Convert.ToString(sqlDr("OTHER10OTANKCH"))
            Me.TotalTankCh = Convert.ToString(sqlDr("TOTALTANKCH"))
            Me.TankLinkNo = Convert.ToString(sqlDr("TANKLINKNO"))
            Me.TankLinkNoMade = Convert.ToString(sqlDr("TANKLINKNOMADE"))
            Me.BillingNo = Convert.ToString(sqlDr("BILLINGNO"))
            Me.KeijyoYmd = Convert.ToString(sqlDr("KEIJYOYMD"))
            Me.Salse = Convert.ToString(sqlDr("SALSE"))
            Me.SalseTax = Convert.ToString(sqlDr("SALSETAX"))
            Me.TotalSalse = Convert.ToString(sqlDr("TOTALSALSE"))
            Me.Payment = Convert.ToString(sqlDr("PAYMENT"))
            Me.PaymentTax = Convert.ToString(sqlDr("PAYMENTTAX"))
            Me.TotalPayment = Convert.ToString(sqlDr("TOTALPAYMENT"))
            Me.OtFileName = Convert.ToString(sqlDr("OTFILENAME"))
            Me.ReceiveCount = Convert.ToString(sqlDr("RECEIVECOUNT"))
            Me.OtSendStatus = Convert.ToString(sqlDr("OTSENDSTATUS"))
            Me.ReservedStatus = Convert.ToString(sqlDr("RESERVEDSTATUS"))
            Me.TakusouStatus = Convert.ToString(sqlDr("TAKUSOUSTATUS"))
            Me.BTrainNo = Convert.ToString(sqlDr("BTRAINNO"))
            Me.BTrainName = Convert.ToString(sqlDr("BTRAINNAME"))
            Me.AnaSyoriFlg = Convert.ToString(sqlDr("ANASYORIFLG"))
            Me.DelFlg = Convert.ToString(sqlDr("DELFLG"))
            Me.InitYmd = Convert.ToString(sqlDr("INITYMD"))
            Me.InitUser = Convert.ToString(sqlDr("INITUSER"))
            Me.InitTermId = Convert.ToString(sqlDr("INITTERMID"))
            Me.UpdYmd = Convert.ToString(sqlDr("UPDYMD"))
            Me.UpdUser = Convert.ToString(sqlDr("UPDUSER"))
            Me.UpdTermId = Convert.ToString(sqlDr("UPDTERMID"))
            Me.ReceiveYmd = Convert.ToString(sqlDr("RECEIVEYMD"))

            Me.DetailList = New List(Of OrderDetailItem)
            'このコンストラクタを通した場合一旦何もしないフラグ
            Me.EntryType = OrderItemEntryType.None
            Me.JointOrder = Convert.ToString(sqlDr("JOINTORDER"))
            Me.SecondConsigneeOrder = Convert.ToString(sqlDr("SECONDCONSIGNEEORDER"))
        End Sub

        ''' <summary>
        ''' 受注№
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String
        ''' <summary>
        ''' 本線列車
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainNo As String
        ''' <summary>
        ''' 本線列車名
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainName As String
        ''' <summary>
        ''' 受注登録日
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderYmd As String
        ''' <summary>
        ''' 受注営業所コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String
        ''' <summary>
        ''' 受注営業所コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeName As String
        ''' <summary>
        ''' 受注パターン
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderType As String
        ''' <summary>
        ''' 荷主コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ShippersCode As String
        ''' <summary>
        ''' 荷主名
        ''' </summary>
        ''' <returns></returns>
        Public Property ShippersName As String
        ''' <summary>
        ''' 基地コード
        ''' </summary>
        ''' <returns></returns>
        Public Property BaseCode As String
        ''' <summary>
        ''' 基地名
        ''' </summary>
        ''' <returns></returns>
        Public Property BaseName As String
        ''' <summary>
        ''' 荷受人コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ConsigneeCode As String
        ''' <summary>
        ''' 荷受人名
        ''' </summary>
        ''' <returns></returns>
        Public Property ConsigneeName As String
        ''' <summary>
        ''' 発駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property DepStation As String
        ''' <summary>
        ''' 発駅名
        ''' </summary>
        ''' <returns></returns>
        Public Property DepStationName As String
        ''' <summary>
        ''' 着駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrStation As String
        ''' <summary>
        ''' 着駅名
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrStationName As String
        ''' <summary>
        ''' 空車着駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property RetStation As String
        ''' <summary>
        ''' 空車着駅名
        ''' </summary>
        ''' <returns></returns>
        Public Property RetStationName As String
        ''' <summary>
        ''' 空車着駅コード（変更後）
        ''' </summary>
        ''' <returns></returns>
        Public Property ChangeRetStation As String
        ''' <summary>
        ''' 空車着駅名（変更後）
        ''' </summary>
        ''' <returns></returns>
        Public Property ChangeRetStationName As String
        ''' <summary>
        ''' 受注進行ステータス
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderStatus As String
        ''' <summary>
        ''' 受注情報
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderInfo As String
        ''' <summary>
        ''' 空回日報可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property EmptyTurnFlg As String
        ''' <summary>
        ''' 積置可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property StackingFlg As String
        ''' <summary>
        ''' 利用可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property UseProprietyFlg As String
        ''' <summary>
        ''' 手配連絡フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property ContactFlg As String
        ''' <summary>
        ''' 結果受理フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property ResultFlg As String
        ''' <summary>
        ''' 託送指示フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property DeliveryFlg As String
        ''' <summary>
        ''' 託送指示送信回数
        ''' </summary>
        ''' <returns></returns>
        Public Property DeliveryCount As String

        ''' <summary>
        ''' 積込日（予定）
        ''' </summary>
        ''' <returns></returns>
        Public Property LodDate As String
        ''' <summary>
        ''' 発日（予定）
        ''' </summary>
        ''' <returns></returns>
        Public Property DepDate As String
        ''' <summary>
        ''' 積車着日（予定）
        ''' </summary>
        ''' <returns></returns>
        Public Property ArrDate As String
        ''' <summary>
        ''' 受入日（予定）
        ''' </summary>
        ''' <returns></returns>
        Public Property AccDate As String
        ''' <summary>
        ''' 空車着日（予定）
        ''' </summary>
        ''' <returns></returns>
        Public Property EmpArrDate As String
        ''' <summary>
        ''' 積込日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualLodDate As String
        ''' <summary>
        ''' 発日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualDepDate As String
        ''' <summary>
        ''' 積車着日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualArrDate As String
        ''' <summary>
        ''' 受入日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualAccDate As String
        ''' <summary>
        ''' 空車着日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualEmpArrDate As String
        ''' <summary>
        ''' 車数（レギュラー）
        ''' </summary>
        ''' <returns></returns>
        Public Property RTank As String
        ''' <summary>
        ''' 車数（ハイオク）
        ''' </summary>
        ''' <returns></returns>
        Public Property HTank As String
        ''' <summary>
        ''' 車数（灯油）
        ''' </summary>
        ''' <returns></returns>
        Public Property TTank As String
        ''' <summary>
        ''' 車数（未添加灯油）
        ''' </summary>
        ''' <returns></returns>
        Public Property MTtank As String
        ''' <summary>
        ''' 車数（軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property KTank As String
        ''' <summary>
        ''' 車数（３号軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property K3Tank As String
        ''' <summary>
        ''' 車数（５号軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property K5Tank As String
        ''' <summary>
        ''' 車数（１０号軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property K10Tank As String
        ''' <summary>
        ''' 車数（LSA）
        ''' </summary>
        ''' <returns></returns>
        Public Property LTank As String
        ''' <summary>
        ''' 車数（A重油）
        ''' </summary>
        ''' <returns></returns>
        Public Property ATank As String
        ''' <summary>
        ''' 車数（その他１）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other1Otank As String
        ''' <summary>
        ''' 車数（その他２）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other2OTank As String
        ''' <summary>
        ''' 車数（その他３）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other3OTank As String
        ''' <summary>
        ''' 車数（その他４）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other4OTank As String
        ''' <summary>
        ''' 車数（その他５）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other5OTank As String
        ''' <summary>
        ''' 車数（その他６）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other6OTank As String
        ''' <summary>
        ''' 車数（その他７）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other7OTank As String
        ''' <summary>
        ''' 車数（その他８）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other8OTank As String
        ''' <summary>
        ''' 車数（その他９）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other9OTank As String
        ''' <summary>
        ''' 車数（その他１０）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other10OTank As String
        ''' <summary>
        ''' 合計車数
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalTank As String
        ''' <summary>
        ''' 変更後_車数（レギュラー）
        ''' </summary>
        ''' <returns></returns>
        Public Property RTankCh As String
        ''' <summary>
        ''' 変更後_車数（ハイオク）
        ''' </summary>
        ''' <returns></returns>
        Public Property HTankCh As String
        ''' <summary>
        ''' 変更後_車数（灯油）
        ''' </summary>
        ''' <returns></returns>
        Public Property TTankCh As String
        ''' <summary>
        ''' 変更後_車数（未添加灯油）
        ''' </summary>
        ''' <returns></returns>
        Public Property MtTankCh As String
        ''' <summary>
        ''' 変更後_車数（軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property KTankCh As String
        ''' <summary>
        ''' 変更後_車数（３号軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property K3TankCh As String
        ''' <summary>
        ''' 変更後_車数（５号軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property K5TankCh As String
        ''' <summary>
        ''' 変更後_車数（１０号軽油）
        ''' </summary>
        ''' <returns></returns>
        Public Property K10TankCh As String
        ''' <summary>
        ''' 変更後_車数（LSA）
        ''' </summary>
        ''' <returns></returns>
        Public Property LTankCh As String
        ''' <summary>
        ''' 変更後_車数（A重油）
        ''' </summary>
        ''' <returns></returns>
        Public Property ATankCh As String
        ''' <summary>
        ''' 変更後_車数（その他１）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other1OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他２）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other2OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他３）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other3OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他４）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other4OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他５）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other5OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他６）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other6OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他７）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other7OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他８）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other8OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他９）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other9OTankCh As String
        ''' <summary>
        ''' 変更後_車数（その他１０）
        ''' </summary>
        ''' <returns></returns>
        Public Property Other10OTankCh As String
        ''' <summary>
        ''' 変更後_合計車数
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalTankCh As String
        ''' <summary>
        ''' 貨車連結順序表№
        ''' </summary>
        ''' <returns></returns>
        Public Property TankLinkNo As String
        ''' <summary>
        ''' 作成_貨車連結順序表№
        ''' </summary>
        ''' <returns></returns>
        Public Property TankLinkNoMade As String
        ''' <summary>
        ''' 請求番号
        ''' </summary>
        ''' <returns></returns>
        Public Property BillingNo As String
        ''' <summary>
        ''' 計上日
        ''' </summary>
        ''' <returns></returns>
        Public Property KeijyoYmd As String
        ''' <summary>
        ''' 売上金額
        ''' </summary>
        ''' <returns></returns>
        Public Property Salse As String
        ''' <summary>
        ''' 売上消費税額
        ''' </summary>
        ''' <returns></returns>
        Public Property SalseTax As String
        ''' <summary>
        ''' 売上合計金額
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalSalse As String
        ''' <summary>
        ''' 支払金額
        ''' </summary>
        ''' <returns></returns>
        Public Property Payment As String
        ''' <summary>
        ''' 支払消費税額
        ''' </summary>
        ''' <returns></returns>
        Public Property PaymentTax As String
        ''' <summary>
        ''' 支払合計金額
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalPayment As String
        ''' <summary>
        ''' OTファイル名
        ''' </summary>
        ''' <returns></returns>
        Public Property OtFileName As String
        ''' <summary>
        ''' OT空回日報受信回数
        ''' </summary>
        ''' <returns></returns>
        Public Property ReceiveCount As String
        ''' <summary>
        ''' OT発送日報送信状況
        ''' </summary>
        ''' <returns></returns>
        Public Property OtSendStatus As String
        ''' <summary>
        ''' 出荷予約ダウンロード状況
        ''' </summary>
        ''' <returns></returns>
        Public Property ReservedStatus As String
        ''' <summary>
        ''' 託送状ダウンロード状況
        ''' </summary>
        ''' <returns></returns>
        Public Property TakusouStatus As String
        ''' <summary>
        ''' 返送列車
        ''' </summary>
        ''' <returns></returns>
        Public Property BTrainNo As String
        ''' <summary>
        ''' 返送列車名
        ''' </summary>
        ''' <returns></returns>
        Public Property BTrainName As String
        ''' <summary>
        ''' 分析テーブル処理フラグ(1:取込済み)
        ''' </summary>
        ''' <returns></returns>
        Public Property AnaSyoriFlg As String

        ''' <summary>
        ''' 削除フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property DelFlg As String
        ''' <summary>
        ''' 登録年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property InitYmd As String
        ''' <summary>
        ''' 登録ユーザーＩＤ
        ''' </summary>
        ''' <returns></returns>
        Public Property InitUser As String
        ''' <summary>
        ''' 登録端末
        ''' </summary>
        ''' <returns></returns>
        Public Property InitTermId As String
        ''' <summary>
        ''' 更新年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdYmd As String
        ''' <summary>
        ''' 更新ユーザーＩＤ
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdUser As String
        ''' <summary>
        ''' 更新端末
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdTermId As String
        ''' <summary>
        ''' 集信日時
        ''' </summary>
        ''' <returns></returns>
        Public Property ReceiveYmd As String
        ''' <summary>
        ''' 更新タイプ
        ''' </summary>
        ''' <returns></returns>
        Public Property EntryType As OrderItemEntryType
        ''' <summary>
        ''' 対象のオーダーNoに紐づくオーダー情報
        ''' </summary>
        ''' <returns></returns>
        Public Property DetailList As List(Of OrderDetailItem)
        ''' <summary>
        ''' 最大DetailNo
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxDetailNo As String = "000"
        ''' <summary>
        ''' 荷主がJOINTで設定されているオーダー('1'荷主がJOINT,'0'それ以外)
        ''' </summary>
        ''' <returns></returns>
        Public Property JointOrder As String = ""
        ''' <summary>
        ''' 第二荷受人設定オーダー
        ''' </summary>
        ''' <returns></returns>
        Public Property SecondConsigneeOrder As String = ""
        ''' <summary>
        ''' 油種コードに合わせた車数を取得・設定するプロパティ
        ''' </summary>
        ''' <param name="oilCode">油種コード</param>
        ''' <returns></returns>
        ''' <remarks>一旦ここで油種コードで格納するフィールドを選別する</remarks>
        Public Property TRCount(oilCode As String) As String
            Get
                Select Case oilCode
                    Case "1101" 'レギュラー
                        Return Me.RTank
                    Case "1001" 'ハイオク
                        Return Me.HTank
                    Case "1301" '灯油
                        Return Me.TTank
                    Case "1302" '未添加灯油
                        Return Me.MTtank
                    Case "1401" '軽油
                        Return Me.KTank
                    Case "1404" '３号軽油
                        Return Me.K3Tank
                    'Case "" '５号軽油
                    '    Return Me.K5Tank
                    'Case "" '１０号軽油
                    '    Return Me.K10Tank
                    Case "2201" 'LSA
                        Return Me.LTank
                    Case "2101" 'A重油
                        Return Me.ATank
                        'Case "" 'JP-8 これも謎
                        '    Return ""
                    Case Else
                        Return "0"
                End Select
            End Get
            Set(value As String)
                Select Case oilCode
                    Case "1101" 'レギュラー
                        Me.RTank = value
                    Case "1001" 'ハイオク
                        Me.HTank = value
                    Case "1301" '灯油
                        Me.TTank = value
                    Case "1302" '未添加灯油
                        Me.MTtank = value
                    Case "1401" '軽油
                        Me.KTank = value
                    Case "1404" '３号軽油
                        Me.K3Tank = value
                    'Case "" '５号軽油
                    '    Return Me.K5Tank
                    'Case "" '１０号軽油
                    '    Return Me.K10Tank
                    Case "2201" 'LSA
                        Me.LTank = value
                    Case "2101" 'A重油
                        Me.ATank = value
                        'Case "" 'JP-8 これも謎
                        '    Return ""
                    Case Else
                        Return
                End Select
                '合計を再計算
                Me.TotalTank = (CInt(Me.RTank) + CInt(Me.HTank) + CInt(Me.TTank) + CInt(Me.MTtank) + CInt(Me.KTank) +
                                CInt(Me.K3Tank) + CInt(Me.K5Tank) + CInt(Me.K10Tank) + CInt(Me.LTank) + CInt(Me.ATank) +
                                CInt(Me.Other1Otank) + CInt(Me.Other2OTank) + CInt(Me.Other3OTank) + CInt(Me.Other4OTank) + CInt(Me.Other5OTank) +
                                CInt(Me.Other6OTank) + CInt(Me.Other7OTank) + CInt(Me.Other8OTank) + CInt(Me.Other9OTank) + CInt(Me.Other10OTank)
                                ).ToString

            End Set
        End Property
        ''' <summary>
        ''' ジャーナル用データテーブル変換メソッド
        ''' </summary>
        ''' <returns>ジャーナル登録用にデータテーブルに変換するメソッド</returns>
        Public Function ToDataTable() As DataTable
            Dim retDt As New DataTable
            With retDt.Columns
                Dim fieldList As New List(Of String) From {
                    "ORDERNO", "TRAINNO", "TRAINNAME", "ORDERYMD", "OFFICECODE", "OFFICENAME", "ORDERTYPE", "SHIPPERSCODE", "SHIPPERSNAME",
                    "BASECODE", "BASENAME", "CONSIGNEECODE", "CONSIGNEENAME", "DEPSTATION", "DEPSTATIONNAME",
                    "ARRSTATION", "ARRSTATIONNAME", "RETSTATION", "RETSTATIONNAME",
                    "CHANGERETSTATION", "CHANGERETSTATIONNAME", "ORDERSTATUS", "ORDERINFO",
                    "EMPTYTURNFLG", "STACKINGFLG", "USEPROPRIETYFLG", "CONTACTFLG", "RESULTFLG", "DELIVERYFLG", "DELIVERYCOUNT",
                    "LODDATE", "DEPDATE", "ARRDATE", "ACCDATE", "EMPARRDATE",
                    "ACTUALLODDATE", "ACTUALDEPDATE", "ACTUALARRDATE", "ACTUALACCDATE", "ACTUALEMPARRDATE",
                    "RTANK", "HTANK", "TTANK", "MTTANK", "KTANK", "K3TANK", "K5TANK", "K10TANK", "LTANK", "ATANK",
                    "OTHER1OTANK", "OTHER2OTANK", "OTHER3OTANK", "OTHER4OTANK", "OTHER5OTANK",
                    "OTHER6OTANK", "OTHER7OTANK", "OTHER8OTANK", "OTHER9OTANK", "OTHER10OTANK", "TOTALTANK",
                    "RTANKCH", "HTANKCH", "TTANKCH", "MTTANKCH", "KTANKCH", "K3TANKCH", "K5TANKCH", "K10TANKCH", "LTANKCH", "ATANKCH",
                    "OTHER1OTANKCH", "OTHER2OTANKCH", "OTHER3OTANKCH", "OTHER4OTANKCH", "OTHER5OTANKCH",
                    "OTHER6OTANKCH", "OTHER7OTANKCH", "OTHER8OTANKCH", "OTHER9OTANKCH", "OTHER10OTANKCH",
                    "TOTALTANKCH", "TANKLINKNO", "TANKLINKNOMADE", "BILLINGNO", "KEIJYOYMD",
                    "SALSE", "SALSETAX", "TOTALSALSE", "PAYMENT", "PAYMENTTAX", "TOTALPAYMENT", "OTFILENAME", "RECEIVECOUNT", "OTSENDSTATUS", "RESERVEDSTATUS", "TAKUSOUSTATUS", "BTRAINNO", "BTRAINNAME", "ANASYORIFLG",
                    "DELFLG", "INITYMD", "INITUSER", "INITTERMID", "UPDYMD", "UPDUSER", "UPDTERMID", "RECEIVEYMD"}
                For Each fieldName In fieldList
                    .Add(fieldName, GetType(String))
                Next
            End With
            Dim dr = retDt.NewRow

            dr("ORDERNO") = Me.OrderNo
            dr("TRAINNO") = Me.TrainNo
            dr("TRAINNAME") = Me.TrainName
            dr("ORDERYMD") = Me.OrderYmd
            dr("OFFICECODE") = Me.OfficeCode
            dr("OFFICENAME") = Me.OfficeName
            dr("ORDERTYPE") = Me.OrderType
            dr("SHIPPERSCODE") = Me.ShippersCode
            dr("SHIPPERSNAME") = Me.ShippersName
            dr("BASECODE") = Me.BaseCode
            dr("BASENAME") = Me.BaseName
            dr("CONSIGNEECODE") = Me.ConsigneeCode
            dr("CONSIGNEENAME") = Me.ConsigneeName
            dr("DEPSTATION") = Me.DepStation
            dr("DEPSTATIONNAME") = Me.DepStationName
            dr("ARRSTATION") = Me.ArrStation
            dr("ARRSTATIONNAME") = Me.ArrStationName
            dr("RETSTATION") = Me.RetStation
            dr("RETSTATIONNAME") = Me.RetStationName
            dr("CHANGERETSTATION") = Me.ChangeRetStation
            dr("CHANGERETSTATIONNAME") = Me.ChangeRetStationName
            dr("ORDERSTATUS") = Me.OrderStatus
            dr("ORDERINFO") = Me.OrderInfo
            dr("EMPTYTURNFLG") = Me.EmptyTurnFlg
            dr("STACKINGFLG") = Me.StackingFlg
            dr("USEPROPRIETYFLG") = Me.UseProprietyFlg
            dr("CONTACTFLG") = Me.ContactFlg
            dr("RESULTFLG") = Me.ResultFlg
            dr("DELIVERYFLG") = Me.DeliveryFlg
            dr("DELIVERYCOUNT") = Me.DeliveryCount
            dr("LODDATE") = Me.LodDate
            dr("DEPDATE") = Me.DepDate
            dr("ARRDATE") = Me.ArrDate
            dr("ACCDATE") = Me.AccDate
            dr("EMPARRDATE") = Me.EmpArrDate
            dr("ACTUALLODDATE") = Me.ActualLodDate
            dr("ACTUALDEPDATE") = Me.ActualDepDate
            dr("ACTUALARRDATE") = Me.ActualArrDate
            dr("ACTUALACCDATE") = Me.ActualAccDate
            dr("ACTUALEMPARRDATE") = Me.ActualEmpArrDate
            dr("RTANK") = Me.RTank
            dr("HTANK") = Me.HTank
            dr("TTANK") = Me.TTank
            dr("MTTANK") = Me.MTtank
            dr("KTANK") = Me.KTank
            dr("K3TANK") = Me.K3Tank
            dr("K5TANK") = Me.K5Tank
            dr("K10TANK") = Me.K10Tank
            dr("LTANK") = Me.LTank
            dr("ATANK") = Me.ATank
            dr("OTHER1OTANK") = Me.Other1Otank
            dr("OTHER2OTANK") = Me.Other2OTank
            dr("OTHER3OTANK") = Me.Other3OTank
            dr("OTHER4OTANK") = Me.Other4OTank
            dr("OTHER5OTANK") = Me.Other5OTank
            dr("OTHER6OTANK") = Me.Other6OTank
            dr("OTHER7OTANK") = Me.Other7OTank
            dr("OTHER8OTANK") = Me.Other8OTank
            dr("OTHER9OTANK") = Me.Other9OTank
            dr("OTHER10OTANK") = Me.Other10OTank
            dr("TOTALTANK") = Me.TotalTank
            dr("RTANKCH") = Me.RTankCh
            dr("HTANKCH") = Me.HTankCh
            dr("TTANKCH") = Me.TTankCh
            dr("MTTANKCH") = Me.MtTankCh
            dr("KTANKCH") = Me.KTankCh
            dr("K3TANKCH") = Me.K3TankCh
            dr("K5TANKCH") = Me.K5TankCh
            dr("K10TANKCH") = Me.K10TankCh
            dr("LTANKCH") = Me.LTankCh
            dr("ATANKCH") = Me.ATankCh
            dr("OTHER1OTANKCH") = Me.Other1OTankCh
            dr("OTHER2OTANKCH") = Me.Other2OTankCh
            dr("OTHER3OTANKCH") = Me.Other3OTankCh
            dr("OTHER4OTANKCH") = Me.Other4OTankCh
            dr("OTHER5OTANKCH") = Me.Other5OTankCh
            dr("OTHER6OTANKCH") = Me.Other6OTankCh
            dr("OTHER7OTANKCH") = Me.Other7OTankCh
            dr("OTHER8OTANKCH") = Me.Other8OTankCh
            dr("OTHER9OTANKCH") = Me.Other9OTankCh
            dr("OTHER10OTANKCH") = Me.Other10OTankCh
            dr("TOTALTANKCH") = Me.TotalTankCh
            dr("TANKLINKNO") = Me.TankLinkNo
            dr("TANKLINKNOMADE") = Me.TankLinkNoMade
            dr("BILLINGNO") = Me.BillingNo
            dr("KEIJYOYMD") = Me.KeijyoYmd
            dr("SALSE") = Me.Salse
            dr("SALSETAX") = Me.SalseTax
            dr("TOTALSALSE") = Me.TotalSalse
            dr("PAYMENT") = Me.Payment
            dr("PAYMENTTAX") = Me.PaymentTax
            dr("TOTALPAYMENT") = Me.TotalPayment
            dr("OTFILENAME") = Me.OtFileName
            dr("RECEIVECOUNT") = Me.ReceiveCount
            dr("OTSENDSTATUS") = Me.OtSendStatus
            dr("RESERVEDSTATUS") = Me.ReservedStatus
            dr("TAKUSOUSTATUS") = Me.TakusouStatus
            dr("BTRAINNO") = Me.BTrainNo
            dr("BTRAINNAME") = Me.BTrainName
            dr("ANASYORIFLG") = Me.AnaSyoriFlg
            dr("DELFLG") = Me.DelFlg
            dr("INITYMD") = Me.InitYmd
            dr("INITUSER") = Me.InitUser
            dr("INITTERMID") = Me.InitTermId
            dr("UPDYMD") = Me.UpdYmd
            dr("UPDUSER") = Me.UpdUser
            dr("UPDTERMID") = Me.UpdTermId
            dr("RECEIVEYMD") = Me.ReceiveYmd
            retDt.Rows.Add(dr)
            Return retDt
        End Function
        ''' <summary>
        ''' 履歴登録用データテーブル作成
        ''' </summary>
        ''' <param name="historyNo"></param>
        ''' <param name="mapId"></param>
        ''' <returns></returns>
        Public Function ToHistoryDataTable(historyNo As String, mapId As String) As DataTable
            Dim retDt = ToDataTable()
            retDt.Columns.Add("HISTORYNO", GetType(String))
            retDt.Columns.Add("MAPID", GetType(String))
            Dim targetRow As DataRow = retDt.Rows(0)

            targetRow.Item("HISTORYNO") = historyNo
            targetRow.Item("MAPID") = mapId

            Dim midifiyDateTyleFields As New List(Of String) From {"ACTUALLODDATE", "ACTUALDEPDATE", "ACTUALARRDATE", "ACTUALACCDATE", "ACTUALEMPARRDATE", "KEIJYOYMD"}
            For Each fieldName In midifiyDateTyleFields
                Dim val As String = Convert.ToString(targetRow.Item(fieldName))
                retDt.Columns.Remove(fieldName)
                retDt.Columns.Add(fieldName, GetType(Date))
                If val = "" Then
                    targetRow.Item(fieldName) = CType(DBNull.Value, Object)
                Else
                    targetRow.Item(fieldName) = val
                End If
            Next fieldName

            Return retDt
        End Function
    End Class
    ''' <summary>
    ''' 受注詳細アイテムクラス
    ''' </summary>
    Public Class OrderDetailItem
        ''' <summary>
        ''' 受注詳細テーブル登録種類列挙
        ''' </summary>
        Public Enum DetailEntryType
            ''' <summary>
            ''' 追加処理(画面車数が増えた場合に立てる)
            ''' </summary>
            Insert = 0 '新規追加
            ''' <summary>
            ''' 更新処理(今のところ発生想定なし且つこの区分時の制御なし）
            ''' </summary>
            Update = 1 'これは今のところ発生しない想定
            ''' <summary>
            ''' 削除(論理削除：画面車数が減った場合に立てる)
            ''' </summary>
            Delete = 2 '論理削除
            ''' <summary>
            ''' 何もしない
            ''' </summary>
            None = 4 '何もしない
        End Enum
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="orderItm">受注テーブル情報クラス</param>
        ''' <param name="detailNo">受注明細№</param>
        ''' <param name="dispDataClass">画面情報クラス</param>
        ''' <param name="chkOilVal">処理対象画面日付・列車・油種で絞り込んだ・情報</param>
        ''' <param name="procDtm">処理日</param>
        ''' <param name="userID">ユーザー名</param>
        ''' <param name="termId">端末ID</param>
        ''' <remarks>このコンストラクタで来た場合はDB追加想定</remarks>
        Public Sub New(orderItm As OrderItem, detailNo As String, dispDataClass As DispDataClass,
                       chkOilVal As DispDataClass.SuggestItem.SuggestValue, procDtm As Date, userID As String, termId As String)
            'SQLDRの各フィールド値をプロパティにセット
            Me.OrderNo = orderItm.OrderNo
            Me.DetailNo = detailNo
            Me.ShipOrder = ""
            Me.LineOrder = ""
            Me.TankNo = ""
            Me.Kamoku = ""
            Me.StackingOrderNo = ""
            Me.StackingFlg = "2"
            Me.WholeSaleFlg = "2"
            Me.InspectionFlg = "2"
            Me.DetentionFlg = "2"
            Me.FirstReturnFlg = "2"
            Me.AfterReturnFlg = "2"
            Me.OtTransportFlg = "2"
            Me.UpgradeFlg = "2"
            Me.OrderInfo = ""
            Me.ShippersCode = dispDataClass.Shipper
            Me.ShippersName = dispDataClass.ShipperName
            Me.OilCode = chkOilVal.OilInfo.OilCode
            Me.OilName = chkOilVal.OilInfo.OilName
            Me.OrderingType = chkOilVal.OilInfo.SegmentOilCode
            Me.OrderingOilName = chkOilVal.OilInfo.SegmentOilName
            Me.CarsNumber = "1"
            Me.CarsAmount = "0"
            Me.ReturnDateTrain = ""
            Me.JointCode = ""
            Me.Joint = ""
            Me.Remark = ""

            Me.ChangeTrainNo = ""
            Me.ChangeTrainName = ""
            Me.SecondConsigneeCode = ""
            Me.SecondConsigneeName = ""
            Me.SecondArrStation = ""
            Me.SecondArrStationName = ""
            Me.ChangeRetStation = ""
            Me.ChangeRetStationName = ""

            Me.Line = ""

            Me.FillingPoint = ""
            Me.LoadingIriLineTrainNo = ""
            Me.LoadingIriLineTrainName = ""
            Me.LoadingIriLineOrder = ""
            Me.LoadingOutletTrainNo = ""
            Me.LoadingOutletTrainName = ""
            Me.LoadingOutletOrder = ""


            Me.ActualLodDate = ""
            Me.ActualDepDate = ""
            Me.ActualArrDate = ""
            Me.ActualAccDate = ""
            Me.ActualEmpArrDate = ""

            Me.ReservedNo = ""
            Me.OtSendCount = "0"
            Me.DlReservedCount = "0"
            Me.DlTakusouCount = "0"

            Me.Salse = "0"
            Me.SalseTax = "0"
            Me.TotalSalse = "0"
            Me.Payment = "0"
            Me.PaymentTax = "0"
            Me.TotalPayment = "0"
            Me.AnaSyoriFlg = ""
            Me.VolSyoriFlg = ""
            Me.DelFlg = C_DELETE_FLG.ALIVE
            Me.InitYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
            Me.InitUser = userID
            Me.InitTermId = termId
            Me.UpdYmd = procDtm.ToString("yyyy/MM/dd HH:mm:ss.FFF")
            Me.UpdUser = userID
            Me.UpdTermId = termId
            Me.ReceiveYmd = CONST_DEFAULT_RECEIVEYMD
            'このコンストラクタを通った場合は追加対象
            Me.EntryType = DetailEntryType.Insert

        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="sqlDr"></param>
        Public Sub New(sqlDr As SqlDataReader)
            If sqlDr Is Nothing Then
                Return
            End If
            'SQLDRの各フィールド値をプロパティにセット
            Me.OrderNo = Convert.ToString(sqlDr("ORDERNO"))
            Me.DetailNo = Convert.ToString(sqlDr("DETAILNO"))
            Me.ShipOrder = Convert.ToString(sqlDr("SHIPORDER"))
            Me.LineOrder = Convert.ToString(sqlDr("LINEORDER"))
            Me.TankNo = Convert.ToString(sqlDr("TANKNO"))
            Me.Kamoku = Convert.ToString(sqlDr("KAMOKU"))
            Me.StackingOrderNo = Convert.ToString(sqlDr("STACKINGORDERNO"))
            Me.StackingFlg = Convert.ToString(sqlDr("STACKINGFLG"))
            Me.WholeSaleFlg = Convert.ToString(sqlDr("WHOLESALEFLG"))
            Me.InspectionFlg = Convert.ToString(sqlDr("INSPECTIONFLG"))
            Me.DetentionFlg = Convert.ToString(sqlDr("DETENTIONFLG"))
            Me.FirstReturnFlg = Convert.ToString(sqlDr("FIRSTRETURNFLG"))
            Me.AfterReturnFlg = Convert.ToString(sqlDr("AFTERRETURNFLG"))
            Me.OtTransportFlg = Convert.ToString(sqlDr("OTTRANSPORTFLG"))
            Me.UpgradeFlg = Convert.ToString(sqlDr("UPGRADEFLG"))
            Me.OrderInfo = Convert.ToString(sqlDr("ORDERINFO"))
            Me.ShippersCode = Convert.ToString(sqlDr("SHIPPERSCODE"))
            Me.ShippersName = Convert.ToString(sqlDr("SHIPPERSNAME"))
            Me.OilCode = Convert.ToString(sqlDr("OILCODE"))
            Me.OilName = Convert.ToString(sqlDr("OILNAME"))
            Me.OrderingType = Convert.ToString(sqlDr("ORDERINGTYPE"))
            Me.OrderingOilName = Convert.ToString(sqlDr("ORDERINGOILNAME"))
            Me.CarsNumber = Convert.ToString(sqlDr("CARSNUMBER"))
            Me.CarsAmount = Convert.ToString(sqlDr("CARSAMOUNT"))
            Me.ReturnDateTrain = Convert.ToString(sqlDr("RETURNDATETRAIN"))
            Me.JointCode = Convert.ToString(sqlDr("JOINTCODE"))
            Me.Joint = Convert.ToString(sqlDr("JOINT"))
            Me.Remark = Convert.ToString(sqlDr("REMARK"))

            Me.ChangeTrainNo = Convert.ToString(sqlDr("CHANGETRAINNO"))
            Me.ChangeTrainName = Convert.ToString(sqlDr("CHANGETRAINNAME"))
            Me.SecondConsigneeCode = Convert.ToString(sqlDr("SECONDCONSIGNEECODE"))
            Me.SecondConsigneeName = Convert.ToString(sqlDr("SECONDCONSIGNEENAME"))
            Me.SecondArrStation = Convert.ToString(sqlDr("SECONDARRSTATION"))
            Me.SecondArrStationName = Convert.ToString(sqlDr("SECONDARRSTATIONNAME"))
            Me.ChangeRetStation = Convert.ToString(sqlDr("CHANGERETSTATION"))
            Me.ChangeRetStationName = Convert.ToString(sqlDr("CHANGERETSTATIONNAME"))

            Me.Line = Convert.ToString(sqlDr("LINE"))

            Me.FillingPoint = Convert.ToString(sqlDr("FILLINGPOINT"))
            Me.LoadingIriLineTrainNo = Convert.ToString(sqlDr("LOADINGIRILINETRAINNO"))
            Me.LoadingIriLineTrainName = Convert.ToString(sqlDr("LOADINGIRILINETRAINNAME"))
            Me.LoadingIriLineOrder = Convert.ToString(sqlDr("LOADINGIRILINEORDER"))
            Me.LoadingOutletTrainNo = Convert.ToString(sqlDr("LOADINGOUTLETTRAINNO"))
            Me.LoadingOutletTrainName = Convert.ToString(sqlDr("LOADINGOUTLETTRAINNAME"))
            Me.LoadingOutletOrder = Convert.ToString(sqlDr("LOADINGOUTLETORDER"))


            Me.ActualLodDate = Convert.ToString(sqlDr("ACTUALLODDATE"))
            Me.ActualDepDate = Convert.ToString(sqlDr("ACTUALDEPDATE"))
            Me.ActualArrDate = Convert.ToString(sqlDr("ACTUALARRDATE"))
            Me.ActualAccDate = Convert.ToString(sqlDr("ACTUALACCDATE"))
            Me.ActualEmpArrDate = Convert.ToString(sqlDr("ACTUALEMPARRDATE"))

            Me.ReservedNo = Convert.ToString(sqlDr("RESERVEDNO"))
            Me.OtSendCount = Convert.ToString(sqlDr("OTSENDCOUNT"))
            Me.DlReservedCount = Convert.ToString(sqlDr("DLRESERVEDCOUNT"))
            Me.DlTakusouCount = Convert.ToString(sqlDr("DLTAKUSOUCOUNT"))

            Me.Salse = Convert.ToString(sqlDr("SALSE"))
            Me.SalseTax = Convert.ToString(sqlDr("SALSETAX"))
            Me.TotalSalse = Convert.ToString(sqlDr("TOTALSALSE"))
            Me.Payment = Convert.ToString(sqlDr("PAYMENT"))
            Me.PaymentTax = Convert.ToString(sqlDr("PAYMENTTAX"))
            Me.TotalPayment = Convert.ToString(sqlDr("TOTALPAYMENT"))

            Me.AnaSyoriFlg = Convert.ToString(sqlDr("ANASYORIFLG"))
            Me.VolSyoriFlg = Convert.ToString(sqlDr("VOLSYORIFLG"))

            Me.DelFlg = Convert.ToString(sqlDr("DELFLG"))
            Me.InitYmd = Convert.ToString(sqlDr("INITYMD"))
            Me.InitUser = Convert.ToString(sqlDr("INITUSER"))
            Me.InitTermId = Convert.ToString(sqlDr("INITTERMID"))
            Me.UpdYmd = Convert.ToString(sqlDr("UPDYMD"))
            Me.UpdUser = Convert.ToString(sqlDr("UPDUSER"))
            Me.UpdTermId = Convert.ToString(sqlDr("UPDTERMID"))
            Me.ReceiveYmd = Convert.ToString(sqlDr("RECEIVEYMD"))
            Me.EntryType = DetailEntryType.None
        End Sub
        ''' <summary>
        ''' 受注№
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String
        ''' <summary>
        ''' 受注明細№
        ''' </summary>
        ''' <returns></returns>
        Public Property DetailNo As String
        ''' <summary>
        ''' 発送順
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipOrder As String
        ''' <summary>
        ''' 入線順
        ''' </summary>
        ''' <returns></returns>
        Public Property LineOrder As String
        ''' <summary>
        ''' タンク車№
        ''' </summary>
        ''' <returns></returns>
        Public Property TankNo As String
        ''' <summary>
        ''' 費用科目
        ''' </summary>
        ''' <returns></returns>
        Public Property Kamoku As String
        ''' <summary>
        ''' 積置受注№
        ''' </summary>
        ''' <returns></returns>
        Public Property StackingOrderNo As String
        ''' <summary>
        ''' 積置可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property StackingFlg As String
        ''' <summary>
        ''' 未卸可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property WholeSaleFlg As String
        ''' <summary>
        ''' 交検可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property InspectionFlg As String
        ''' <summary>
        ''' 留置可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property DetentionFlg As String
        ''' <summary>
        ''' 先返し可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property FirstReturnFlg As String
        ''' <summary>
        ''' 後返し可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property AfterReturnFlg As String
        ''' <summary>
        ''' OT輸送可否フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property OtTransportFlg As String
        ''' <summary>
        ''' 格上可否フラグ(１：格上あり、２：格上なし）
        ''' </summary>
        ''' <returns></returns>
        Public Property UpgradeFlg As String
        ''' <summary>
        ''' 受注情報
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderInfo As String
        ''' <summary>
        ''' 荷主コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ShippersCode As String
        ''' <summary>
        ''' 荷主名
        ''' </summary>
        ''' <returns></returns>
        Public Property ShippersName As String
        ''' <summary>
        ''' 油種コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OilCode As String
        ''' <summary>
        ''' 油種名
        ''' </summary>
        ''' <returns></returns>
        Public Property OilName As String
        ''' <summary>
        ''' 油種区分(受発注用)
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderingType As String
        ''' <summary>
        ''' 油種名(受発注用)
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderingOilName As String
        ''' <summary>
        ''' 車数
        ''' </summary>
        ''' <returns></returns>
        Public Property CarsNumber As String
        ''' <summary>
        ''' 数量
        ''' </summary>
        ''' <returns></returns>
        Public Property CarsAmount As String
        ''' <summary>
        ''' 返送日列車
        ''' </summary>
        ''' <returns></returns>
        Public Property ReturnDateTrain As String
        ''' <summary>
        ''' ジョイントコード
        ''' </summary>
        ''' <returns></returns>
        Public Property JointCode As String
        ''' <summary>
        ''' ジョイント
        ''' </summary>
        ''' <returns></returns>
        Public Property Joint As String
        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns></returns>
        Public Property Remark As String
        ''' <summary>
        ''' 本線列車（変更後）
        ''' </summary>
        ''' <returns></returns>
        Public Property ChangeTrainNo As String
        ''' <summary>
        ''' 本線列車名（変更後）
        ''' </summary>
        ''' <returns></returns>
        Public Property ChangeTrainName As String
        ''' <summary>
        ''' 第2荷受人コード
        ''' </summary>
        ''' <returns></returns>
        Public Property SecondConsigneeCode As String
        ''' <summary>
        ''' 第2荷受人名
        ''' </summary>
        ''' <returns></returns>
        Public Property SecondConsigneeName As String
        ''' <summary>
        ''' 第2着駅コード
        ''' </summary>
        ''' <returns></returns>
        Public Property SecondArrStation As String
        ''' <summary>
        ''' 第2着駅名
        ''' </summary>
        ''' <returns></returns>
        Public Property SecondArrStationName As String
        ''' <summary>
        ''' 空車着駅コード（変更後）
        ''' </summary>
        ''' <returns></returns>
        Public Property ChangeRetStation As String
        ''' <summary>
        ''' 空車着駅名（変更後）
        ''' </summary>
        ''' <returns></returns>
        Public Property ChangeRetStationName As String
        ''' <summary>
        ''' 回線
        ''' </summary>
        ''' <returns></returns>
        Public Property Line As String
        ''' <summary>
        ''' 充填ポイント
        ''' </summary>
        ''' <returns></returns>
        Public Property FillingPoint As String
        ''' <summary>
        ''' 積込入線列車番号
        ''' </summary>
        ''' <returns></returns>
        Public Property LoadingIriLineTrainNo As String
        ''' <summary>
        ''' 積込入線列車番号名
        ''' </summary>
        ''' <returns></returns>
        Public Property LoadingIriLineTrainName As String
        ''' <summary>
        ''' 積込入線順
        ''' </summary>
        ''' <returns></returns>
        Public Property LoadingIriLineOrder As String
        ''' <summary>
        ''' 積込出線列車番号
        ''' </summary>
        ''' <returns></returns>
        Public Property LoadingOutletTrainNo As String
        ''' <summary>
        ''' 積込出線列車番号名
        ''' </summary>
        ''' <returns></returns>
        Public Property LoadingOutletTrainName As String
        ''' <summary>
        ''' 積込出線順
        ''' </summary>
        ''' <returns></returns>
        Public Property LoadingOutletOrder As String
        ''' <summary>
        ''' 積込日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualLodDate As String
        ''' <summary>
        ''' 発日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualDepDate As String
        ''' <summary>
        ''' 積車着日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualArrDate As String
        ''' <summary>
        ''' 受入日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualAccDate As String
        ''' <summary>
        ''' 空車着日（実績）
        ''' </summary>
        ''' <returns></returns>
        Public Property ActualEmpArrDate As String
        ''' <summary>
        ''' 予約番号
        ''' </summary>
        ''' <returns></returns>
        Public Property ReservedNo As String
        ''' <summary>
        ''' OT発送日報送信回数
        ''' </summary>
        ''' <returns></returns>
        Public Property OtSendCount As String
        ''' <summary>
        ''' 出荷予約ダウンロード回数
        ''' </summary>
        ''' <returns></returns>
        Public Property DlReservedCount As String
        ''' <summary>
        ''' 託送状ダウンロード回数
        ''' </summary>
        ''' <returns></returns>
        Public Property DlTakusouCount As String
        ''' <summary>
        ''' 売上金額
        ''' </summary>
        ''' <returns></returns>
        Public Property Salse As String
        ''' <summary>
        ''' 売上消費税額
        ''' </summary>
        ''' <returns></returns>
        Public Property SalseTax As String
        ''' <summary>
        ''' 売上合計金額
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalSalse As String
        ''' <summary>
        ''' 支払金額
        ''' </summary>
        ''' <returns></returns>
        Public Property Payment As String
        ''' <summary>
        ''' 支払消費税額
        ''' </summary>
        ''' <returns></returns>
        Public Property PaymentTax As String
        ''' <summary>
        ''' 支払合計金額
        ''' </summary>
        ''' <returns></returns>
        Public Property TotalPayment As String
        ''' <summary>
        ''' 分析テーブル処理フラグ (1:取込済み)
        ''' </summary>
        ''' <returns></returns>
        Public Property AnaSyoriFlg As String
        ''' <summary>
        ''' 月間輸送量処理フラグ (1:取込済み)
        ''' </summary>
        ''' <returns></returns>
        Public Property VolSyoriFlg As String

        ''' <summary>
        ''' 削除フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property DelFlg As String
        ''' <summary>
        ''' 登録年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property InitYmd As String
        ''' <summary>
        ''' 登録ユーザーＩＤ
        ''' </summary>
        ''' <returns></returns>
        Public Property InitUser As String
        ''' <summary>
        ''' 登録端末
        ''' </summary>
        ''' <returns></returns>
        Public Property InitTermId As String
        ''' <summary>
        ''' 更新年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdYmd As String
        ''' <summary>
        ''' 更新ユーザーＩＤ
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdUser As String
        ''' <summary>
        ''' 更新端末
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdTermId As String
        ''' <summary>
        ''' 集信日時
        ''' </summary>
        ''' <returns></returns>
        Public Property ReceiveYmd As String
        ''' <summary>
        ''' 登録種別
        ''' </summary>
        ''' <returns></returns>
        Public Property EntryType As DetailEntryType
        ''' <summary>
        ''' ジャーナル用データテーブル変換メソッド
        ''' </summary>
        ''' <returns>ジャーナル登録用にデータテーブルに変換するメソッド</returns>
        Public Function ToDataTable() As DataTable
            Dim retDt As New DataTable
            With retDt.Columns
                Dim fieldList As New List(Of String) From {
                   "ORDERNO", "DETAILNO", "SHIPORDER", "LINEORDER", "TANKNO", "KAMOKU", "STACKINGORDERNO", "STACKINGFLG", "WHOLESALEFLG", "INSPECTIONFLG", "DETENTIONFLG", "FIRSTRETURNFLG", "AFTERRETURNFLG", "OTTRANSPORTFLG", "UPGRADEFLG", "ORDERINFO",
                   "SHIPPERSCODE", "SHIPPERSNAME", "OILCODE", "OILNAME", "ORDERINGTYPE",
                   "ORDERINGOILNAME", "CARSNUMBER", "CARSAMOUNT", "RETURNDATETRAIN",
                   "JOINTCODE", "JOINT", "REMARK", "CHANGETRAINNO", "CHANGETRAINNAME",
                   "SECONDCONSIGNEECODE", "SECONDCONSIGNEENAME",
                   "SECONDARRSTATION", "SECONDARRSTATIONNAME",
                   "CHANGERETSTATION", "CHANGERETSTATIONNAME",
                   "LINE", "FILLINGPOINT",
                   "LOADINGIRILINETRAINNO", "LOADINGIRILINETRAINNAME",
                   "LOADINGIRILINEORDER", "LOADINGOUTLETTRAINNO",
                   "LOADINGOUTLETTRAINNAME", "LOADINGOUTLETORDER",
                   "ACTUALLODDATE", "ACTUALDEPDATE",
                   "ACTUALARRDATE", "ACTUALACCDATE",
                   "ACTUALEMPARRDATE", "RESERVEDNO", "OTSENDCOUNT", "DLRESERVEDCOUNT", "DLTAKUSOUCOUNT",
                   "SALSE", "SALSETAX", "TOTALSALSE",
                   "PAYMENT", "PAYMENTTAX", "TOTALPAYMENT", "ANASYORIFLG", "VOLSYORIFLG",
                   "DELFLG", "INITYMD", "INITUSER", "INITTERMID",
                   "UPDYMD", "UPDUSER", "UPDTERMID", "RECEIVEYMD"
                 }
                For Each fieldName In fieldList
                    .Add(fieldName, GetType(String))
                Next
            End With
            Dim dr = retDt.NewRow

            dr("ORDERNO") = Me.OrderNo
            dr("DETAILNO") = Me.DetailNo
            dr("SHIPORDER") = Me.ShipOrder
            dr("LINEORDER") = Me.LineOrder
            dr("TANKNO") = Me.TankNo
            dr("KAMOKU") = Me.Kamoku
            dr("STACKINGORDERNO") = Me.StackingOrderNo
            dr("STACKINGFLG") = Me.StackingFlg
            dr("WHOLESALEFLG") = Me.WholeSaleFlg
            dr("INSPECTIONFLG") = Me.InspectionFlg
            dr("DETENTIONFLG") = Me.DetentionFlg
            dr("FIRSTRETURNFLG") = Me.FirstReturnFlg
            dr("AFTERRETURNFLG") = Me.AfterReturnFlg
            dr("OTTRANSPORTFLG") = Me.OtTransportFlg
            dr("UPGRADEFLG") = Me.UpgradeFlg
            dr("ORDERINFO") = Me.OrderInfo
            dr("SHIPPERSCODE") = Me.ShippersCode
            dr("SHIPPERSNAME") = Me.ShippersName
            dr("OILCODE") = Me.OilCode
            dr("OILNAME") = Me.OilName
            dr("ORDERINGTYPE") = Me.OrderingType
            dr("ORDERINGOILNAME") = Me.OrderingOilName
            dr("CARSNUMBER") = Me.CarsNumber
            dr("CARSAMOUNT") = Me.CarsAmount
            dr("RETURNDATETRAIN") = Me.ReturnDateTrain
            dr("JOINTCODE") = Me.JointCode
            dr("JOINT") = Me.Joint
            dr("REMARK") = Me.Remark
            dr("CHANGETRAINNO") = Me.ChangeTrainNo
            dr("CHANGETRAINNAME") = Me.ChangeTrainName
            dr("SECONDCONSIGNEECODE") = Me.SecondConsigneeCode
            dr("SECONDCONSIGNEENAME") = Me.SecondConsigneeName
            dr("SECONDARRSTATION") = Me.SecondArrStation
            dr("SECONDARRSTATIONNAME") = Me.SecondArrStationName
            dr("CHANGERETSTATION") = Me.ChangeRetStation
            dr("CHANGERETSTATIONNAME") = Me.ChangeRetStationName
            dr("LINE") = Me.Line
            dr("FILLINGPOINT") = Me.FillingPoint
            dr("LOADINGIRILINETRAINNO") = Me.LoadingIriLineTrainNo
            dr("LOADINGIRILINETRAINNAME") = Me.LoadingIriLineTrainName
            dr("LOADINGIRILINEORDER") = Me.LoadingIriLineOrder
            dr("LOADINGOUTLETTRAINNO") = Me.LoadingOutletTrainNo
            dr("LOADINGOUTLETTRAINNAME") = Me.LoadingOutletTrainName
            dr("LOADINGOUTLETORDER") = Me.LoadingOutletOrder
            dr("ACTUALLODDATE") = Me.ActualLodDate
            dr("ACTUALDEPDATE") = Me.ActualDepDate
            dr("ACTUALARRDATE") = Me.ActualArrDate
            dr("ACTUALACCDATE") = Me.ActualAccDate
            dr("ACTUALEMPARRDATE") = Me.ActualEmpArrDate
            dr("RESERVEDNO") = Me.ReservedNo
            dr("OTSENDCOUNT") = Me.OtSendCount
            dr("DLRESERVEDCOUNT") = Me.DlReservedCount
            dr("DLTAKUSOUCOUNT") = Me.DlTakusouCount
            dr("SALSE") = Me.Salse
            dr("SALSETAX") = Me.SalseTax
            dr("TOTALSALSE") = Me.TotalSalse
            dr("PAYMENT") = Me.Payment
            dr("PAYMENTTAX") = Me.PaymentTax
            dr("TOTALPAYMENT") = Me.TotalPayment
            dr("ANASYORIFLG") = Me.AnaSyoriFlg
            dr("VOLSYORIFLG") = Me.VolSyoriFlg
            dr("DELFLG") = Me.DelFlg
            dr("INITYMD") = Me.InitYmd
            dr("INITUSER") = Me.InitUser
            dr("INITTERMID") = Me.InitTermId
            dr("UPDYMD") = Me.UpdYmd
            dr("UPDUSER") = Me.UpdUser
            dr("UPDTERMID") = Me.UpdTermId
            dr("RECEIVEYMD") = Me.ReceiveYmd

            retDt.Rows.Add(dr)
            Return retDt

        End Function
        ''' <summary>
        ''' 履歴登録用データテーブル作成
        ''' </summary>
        ''' <param name="historyNo"></param>
        ''' <param name="mapId"></param>
        ''' <returns></returns>
        Public Function ToHistoryDataTable(historyNo As String, mapId As String) As DataTable
            Dim retDt = ToDataTable()
            retDt.Columns.Add("HISTORYNO", GetType(String))
            retDt.Columns.Add("MAPID", GetType(String))
            Dim targetRow As DataRow = retDt.Rows(0)

            targetRow.Item("HISTORYNO") = historyNo
            targetRow.Item("MAPID") = mapId

            Dim midifiyDateTyleFields As New List(Of String) From {"ACTUALLODDATE", "ACTUALDEPDATE", "ACTUALARRDATE", "ACTUALACCDATE", "ACTUALEMPARRDATE"}
            For Each fieldName In midifiyDateTyleFields
                Dim val As String = Convert.ToString(targetRow.Item(fieldName))
                retDt.Columns.Remove(fieldName)
                retDt.Columns.Add(fieldName, GetType(Date))
                If val = "" Then
                    targetRow.Item(fieldName) = CType(DBNull.Value, Object)
                Else
                    targetRow.Item(fieldName) = val
                End If
            Next fieldName

            Return retDt
        End Function
    End Class
    ''' <summary>
    ''' 受注テーブル更新処理用のメッセージ
    ''' </summary>
    Public Class EntryOrderResultItm
        Public Property OfficeCode As String
        Public Property ShipperCode As String
        Public Property ConsigneeCode As String
        Private _TrainNo As String = ""
        Public Property TrainNo As String
            Get
                If Me._TrainNo = "" Then
                    Return "-"
                Else
                    Return Me._TrainNo
                End If
            End Get
            Set(value As String)
                Me._TrainNo = value
            End Set
        End Property
        Private _AccDate As String = ""
        Public Property AccDate As String
            Get
                If Me._AccDate = "" Then
                    Return "-"
                Else
                    Return Me._AccDate
                End If
            End Get
            Set(value As String)
                Me._AccDate = value
            End Set
        End Property
        Private _OilCode As String = ""
        Public Property OilCode As String
            Get
                If Me._OilCode = "" Then
                    Return "-"
                Else
                    Return Me._OilCode
                End If
            End Get
            Set(value As String)
                Me._OilCode = value
            End Set
        End Property
        Private _OrderNo As String = ""
        Public Property OrderNo As String
            Get
                If Me._OrderNo = "" Then
                    Return "-"
                Else
                    Return Me._OrderNo
                End If
            End Get
            Set(value As String)
                Me._OrderNo = value
            End Set
        End Property
        Private _DetailNo As String
        Public Property DetailNo As String
            Get
                If Me._DetailNo = "" Then
                    Return "-"
                Else
                    Return Me._DetailNo
                End If
            End Get
            Set(value As String)
                Me._DetailNo = value
            End Set
        End Property

        Public Property Message As String '保留 
        Public Property MessageId As String
        Public Property StackTrace As String = "-"


    End Class
    ''' <summary>
    ''' 油種別の列車数情報格納クラス
    ''' </summary>
    Public Class PrintTrainNumCollection
        Public Property OilInfo As OilItem
        Public Property PrintTrainNumList As Dictionary(Of String, PrintTrainNum)

    End Class
    ''' <summary>
    ''' 印刷用列車数格納
    ''' </summary>
    Public Class PrintTrainNum
        ''' <summary>
        ''' 営業所名
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String
        ''' <summary>
        ''' オフィス名
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeName As String
        ''' <summary>
        ''' キー日付の印刷用列車数保持ディクショナリ
        ''' </summary>
        ''' <returns></returns>
        Public Property PrintTrainItems As Dictionary(Of String, PrintTrainItem)
    End Class
    ''' <summary>
    ''' 印刷用、日付別列車数格納アイテムクラス
    ''' </summary>
    Public Class PrintTrainItem
        Public Property DateString As String
        Public Property TrainNum As Decimal = 0D

    End Class
#Region "ViewStateを圧縮 これをしないとViewStateが7万文字近くなり重くなる,実行すると9000文字"
    '   "RepeaterでPoscBack時処理で使用するため保持させる必要上RepeaterのViewState使用停止するのは難しい"

    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal viewState As Object)
        Dim lofF As New LosFormatter
        Using sw As New IO.StringWriter
            lofF.Serialize(sw, viewState)
            Dim viewStateString = sw.ToString()
            Dim bytes = Convert.FromBase64String(viewStateString)
            bytes = CompressByte(bytes)
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes))
        End Using
    End Sub
    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        Dim viewState As String = Request.Form("__VSTATE")
        Dim bytes = Convert.FromBase64String(viewState)
        bytes = DeCompressByte(bytes)
        Dim lofF = New LosFormatter()
        Return lofF.Deserialize(Convert.ToBase64String(bytes))
    End Function
    ''' <summary>
    ''' ByteDetaを圧縮
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function CompressByte(data As Byte()) As Byte()
        Using ms As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress)
            ds.Write(data, 0, data.Length)
            ds.Close()
            Return ms.ToArray
        End Using
    End Function
    ''' <summary>
    ''' Byteデータを解凍
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DeCompressByte(data As Byte()) As Byte()
        Using inpMs As New IO.MemoryStream(data),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            Return outMs.ToArray
        End Using

    End Function
#End Region
End Class

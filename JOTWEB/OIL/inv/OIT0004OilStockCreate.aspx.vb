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
                        Case "WF_ButtonRECULC"
                            WF_ButtonRECULC_Click()
                        Case "WF_ButtonUPDATE" '更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV" 'ダウンロードボタン押下

                        Case "WF_ButtonINSERT" '新規登録ボタン押下

                        Case "WF_ButtonEND"                 '戻るボタン押下
                            WF_ButtonEND_Click()
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
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If
        '**********************************************
        '画面情報を元に各対象リストを生成
        '**********************************************
        Dim baseDate = work.WF_SEL_STYMD.Text
        Dim salesOffice = work.WF_SEL_SALESOFFICECODE.Text
        Dim consignee = work.WF_SEL_CONSIGNEE.Text
        Dim daysList As Dictionary(Of String, DaysItem)
        Dim oilTypeList As Dictionary(Of String, OilItem)
        Dim trainList As Dictionary(Of String, TrainListItem)
        Dim dispDataObj As DispDataClass = Nothing

        Dim mitrainList As Dictionary(Of String, TrainListItem) = Nothing
        Dim miOilTypeList As Dictionary(Of String, OilItem) = Nothing
        'DBよりデータ取得し画面用データに加工
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            '日付情報取得（祝祭日含む）
            daysList = GetTargetDateList(sqlCon, baseDate)
            '対象油種取得
            oilTypeList = GetTargetOilType(sqlCon, salesOffice, consignee)
            '対象列車取得（ここはまだベタ打ち）
            trainList = GetTargetTrain(sqlCon, salesOffice, consignee)
            '抽出結果を画面データクラスに展開
            dispDataObj = New DispDataClass(daysList, trainList, oilTypeList, salesOffice, consignee)
            '前週出荷平均の取得
            dispDataObj = GetLastShipAverage(sqlCon, dispDataObj)
            '提案一覧表示可否取得
            dispDataObj.ShowSuggestList = Me.IsShowSuggestList(sqlCon, consignee)
            '構内取り有無取得
            dispDataObj = GetMoveInsideData(sqlCon, dispDataObj)
            '既登録データ取得
            dispDataObj = GetTargetStockData(sqlCon, dispDataObj)
            '過去日以外の日付について受入数取得
            dispDataObj = GetReciveFromOrder(sqlCon, dispDataObj)
            '構内取り設定がある場合、構内取りデータ取得
            If dispDataObj.HasMoveInsideItem Then
                '構内取りではない油種「合計」文言を中計と変更
                dispDataObj.SuggestOilNameList(DispDataClass.SUMMARY_CODE).OilName = "中計"
                '表構えの為親と構内取り元と同じ列車
                mitrainList = GetTargetTrain(sqlCon, salesOffice, consignee)
                '油種は持っている元に合わせる（最終的に元と一致する油種じゃないと認めない？）
                miOilTypeList = GetTargetOilType(sqlCon, dispDataObj.MiSalesOffice, dispDataObj.MiConsignee)
                '構内取り用の画面表示クラス生成
                dispDataObj.MiDispData = New DispDataClass(daysList, mitrainList, miOilTypeList, dispDataObj.MiSalesOffice, dispDataObj.MiConsignee)
                '前週出荷平均の取得
                dispDataObj.MiDispData = GetLastShipAverage(sqlCon, dispDataObj.MiDispData)
                dispDataObj.MiDispData.RecalcStockList(False)
                For Each suggestListItem In dispDataObj.SuggestList
                    Dim key = suggestListItem.Key
                    Dim item = dispDataObj.MiDispData.SuggestList(key).SuggestOrderItem
                    suggestListItem.Value.SuggestMiOrderItem = item
                    suggestListItem.Value.RelateMoveInside()
                Next
            End If
            '既登録データ抽出

        End Using
        '取得値を元に再計算
        dispDataObj.RecalcStockList(False)

        '****************************************
        '生成したデータを画面に貼り付け
        '****************************************
        '1.提案リスト
        If dispDataObj.ShowSuggestList = False Then
            pnlSuggestList.Visible = False
            Me.spnInventoryDays.Visible = False
            Me.WF_ButtonAUTOSUGGESTION.Visible = False
            Me.WF_ButtonORDERLIST.Visible = False
        Else
            pnlSuggestList.Visible = True
            frvSuggest.DataSource = New Object() {dispDataObj}
            frvSuggest.DataBind()
        End If

        '2.比重リスト
        repWeightList.DataSource = dispDataObj.OilTypeList
        repWeightList.DataBind()
        '3.在庫表
        repStockDate.DataSource = dispDataObj.StockDate
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

    End Sub
    ''' <summary>
    ''' 受注作成ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonORDERLIST_Click()

    End Sub
    ''' <summary>
    ''' 入力値クリアボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonINPUTCLEAR_Click()
        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim dispValues = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        dispValues.InputValueToZero()
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
        repStockDate.DataSource = dispValues.StockDate
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
        SaveThisScreenValue(dispValues)
        '在庫表再表示
        repStockDate.DataSource = dispValues.StockDate
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
            Dim errNum As String = ""
            If EntryStockData(sqlCon, dispValues, errNum, Date.Now) = False Then
                Return
            End If
        End Using

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
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
    ''' 対象列車情報取得
    ''' </summary>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="salesOffice">営業所コード</param>
    ''' <param name="consignee">油槽所コード</param>
    ''' <returns>キー：列車No,値：列車アイテムクラス
    ''' 営業所、油槽所を元に取得した列車情報</returns>
    ''' <remarks>一旦戻り値が無い場合は提案表を出さない仕組みとする</remarks>
    Private Function GetTargetTrain(sqlCon As SqlConnection, salesOffice As String, consignee As String) As Dictionary(Of String, TrainListItem)
        '↓本当はDBから取得！！！のたたき台↓ コメントアウトしSQLなり共通関数なりを利用し整えること
        'Try
        '    Dim retVal As New Dictionary(Of String, TrainListItem)

        '    Dim sqlStr As New StringBuilder
        '    sqlStr.AppendLine("SELECT XXXXX")
        '    sqlStr.AppendLine("  FROM XXXXX")
        '    sqlStr.AppendLine(" WHERE XXXX = @XXXXX")

        '    Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
        '        With sqlCmd.Parameters
        '            .Add("@xxxx", SqlDbType.NVarChar).Value = "xxxx"
        '            .Add("@xxxx", SqlDbType.NVarChar).Value = "xxxx"
        '        End With
        '        Dim tlItem As TrainListItem
        '        Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
        '            While sqlDr.Read
        '                tlItem = New TrainListItem(Convert.ToString(sqlDr("車CODE")), Convert.ToString(sqlDr("車名称")))
        '                retVal.Add(tlItem.TrainNo, tlItem)
        '            End While
        '        End Using

        '    End Using
        '    Return retVal
        'Catch ex As Exception
        '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005C")

        '    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
        '    CS0011LOGWrite.INFPOSI = "DB:OIT0004C Select Train List"
        '    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
        '    CS0011LOGWrite.TEXT = ex.ToString()
        '    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        '    Throw '呼出し元の後続処理を走らせたくないのでThrow 
        'End Try
        '↑本当はDBから取得！！！のたたき台↑
        Dim retVal As New Dictionary(Of String, TrainListItem)
        '袖ヶ浦
        If salesOffice = "011203" AndAlso consignee = "40" Then
            retVal.Add("5972", New TrainListItem("5972", "5972-南松本", 20))
        End If
        If salesOffice = "011203" AndAlso consignee = "30" Then
            retVal.Add("8877", New TrainListItem("8877", "8877-倉賀野", 20))
            retVal.Add("8883", New TrainListItem("8883", "8883-倉賀野", 22))
        End If
        '根岸
        If salesOffice = "011402" AndAlso consignee = "10" Then
            retVal.Add("5463", New TrainListItem("5463", "5463-坂城", 17))
            retVal.Add("2085", New TrainListItem("2085", "2085-坂城", 17))
            retVal.Add("8471", New TrainListItem("8471", "8471-坂城", 17))
        End If
        If salesOffice = "011402" AndAlso consignee = "20" Then
            retVal.Add("81", New TrainListItem("81", "81-竜王", 17))
            retVal.Add("83", New TrainListItem("83", "83-竜王", 13))
        End If
        '三重塩浜
        If salesOffice = "012402" AndAlso consignee = "40" Then
            retVal.Add("5282", New TrainListItem("5282", "5282-南松本", 18))
            retVal.Add("8072", New TrainListItem("8072", "8072-南松本", 18))
        End If
        Return retVal
    End Function
    ''' <summary>
    ''' 基準日を元に日付リストを生成
    ''' </summary>
    ''' <param name="baseDate">基準日</param>
    ''' <param name="daySpan">引数BaseDateを含む設定した日付情報を取得(初期値:7)</param>
    ''' <returns>キー：日付、値：日付アイテムクラス</returns>
    Private Function GetTargetDateList(sqlCon As SqlConnection, baseDate As String, Optional daySpan As Integer = 7) As Dictionary(Of String, DaysItem)
        Try
            Dim retVal As New Dictionary(Of String, DaysItem)
            '日付型に変換 検索条件よりわたってきている想定なので日付型に確実に変換できる想定
            Dim baseDtm As Date = Date.Parse(baseDate)
            Dim dtItm As DaysItem
            '基準日から引数期間のデータを生成
            For i As Integer = 0 To daySpan - 1
                Dim currentDay As Date = baseDtm.AddDays(i)
                dtItm = New DaysItem(currentDay)
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005C")

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
    ''' <returns>キー:油種コード、値：油種アイテムクラス</returns>
    Private Function GetTargetOilType(sqlCon As SqlConnection, salesOffice As String, consignee As String) As Dictionary(Of String, OilItem)
        Dim retVal As New Dictionary(Of String, OilItem)
        '営業所に対応する油種コード取得
        Dim sqlStr As New StringBuilder
        sqlStr.AppendLine("SELECT FV.KEYCODE  AS OILCODE")
        sqlStr.AppendLine("      ,FV.VALUE1   AS OILNAME")
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
        sqlConsigneeOilType.AppendLine("  AND FV.DELFLG   = @DELFLG")

        'DBより取得を行い取得情報付与
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            Dim paramCampCode = sqlCmd.Parameters.Add("@CAMPCODE", SqlDbType.NVarChar)
            Dim paramClass = sqlCmd.Parameters.Add("@CLASS", SqlDbType.NVarChar)
            '2つのSQLで変わらない（or不要なパラメータ)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = "0"
                .Add("@STOCKFLG", SqlDbType.NVarChar).Value = "9" '不等号条件
                .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = consignee
            End With
            paramCampCode.Value = salesOffice
            paramClass.Value = "PRODUCTPATTERN"

            Dim oilCode As String
            Dim oilName As String
            Dim bigOilCode As String
            Dim midOilCode As String
            Dim oilItm As OilItem
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                While sqlDr.Read
                    oilCode = Convert.ToString(sqlDr("OILCODE"))
                    oilName = Convert.ToString(sqlDr("OILNAME"))
                    bigOilCode = Convert.ToString(sqlDr("BIGOILCODE"))
                    midOilCode = Convert.ToString(sqlDr("MIDDLEOILCODE"))
                    oilItm = New OilItem(oilCode, oilName, bigOilCode, midOilCode)
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
        '検索値の設定(過去日じゃない日付リストを取得）
        Dim qdataVal = From itm In dispData.StockDate
                       Where itm.Value.IsPastDay = False
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
        sqlStr.AppendLine("     , SUM(isnull(DTL.CARSAMOUNT,0))    AS CARSAMOUNT")
        sqlStr.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        sqlStr.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DTL")
        sqlStr.AppendLine("    ON ODR.ORDERNO =  DTL.ORDERNO")
        sqlStr.AppendLine("   AND DTL.DELFLG  =  @DELFLG")
        sqlStr.AppendLine("   AND DTL.OILCODE is not null")
        sqlStr.AppendLine(" WHERE ODR.ACCDATE  　BETWEEN @DATE_FROM AND @DATE_TO")
        sqlStr.AppendLine("   AND ODR.OFFICECODE      = @OFFICECODE")
        sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStr.AppendLine(" GROUP BY DTL.OILCODE,ODR.ACCDATE")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@DATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@DATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String
                    Dim recvVal As Decimal = 0D
                    Dim targetDate As String = ""
                    While sqlDr.Read
                        oilCode = Convert.ToString(sqlDr("OILCODE"))
                        '油種未設定または対象油種を持っていないレコードはスキップ
                        If oilCode = "" OrElse retVal.StockList.ContainsKey(oilCode) Then
                            Continue While
                        End If
                        targetDate = Convert.ToString(sqlDr("TARGETDATE"))
                        With retVal.StockList(oilCode)
                            If .StockItemList.ContainsKey(targetDate) Then
                                recvVal = Decimal.Parse(Convert.ToString(sqlDr("CARSAMOUNT")))
                                With .StockItemList(targetDate)
                                    .Receive = recvVal
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
        '検索値の設定
        Dim dateFrom As String = dispData.StockDate.First.Value.ItemDate.AddDays(-7).ToString("yyyy/MM/dd")
        Dim dateTo As String = dispData.StockDate.Last.Value.ItemDate.AddDays(-7).ToString("yyyy/MM/dd")

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
        sqlStr.AppendLine("   AND ODR.CONSIGNEECODE   = @CONSIGNEECODE")
        sqlStr.AppendLine("   AND ODR.DELFLG          = @DELFLG")
        sqlStr.AppendLine(" GROUP BY DTL.OILCODE")
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                .Add("@ACTUALDATE_FROM", SqlDbType.Date).Value = dateFrom
                .Add("@ACTUALDATE_TO", SqlDbType.Date).Value = dateTo
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
            End With

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                If sqlDr.HasRows Then
                    Dim oilCode As String
                    Dim avaVal As Decimal = 0D
                    While sqlDr.Read
                        oilCode = Convert.ToString(sqlDr("OILCODE"))
                        '油種未設定または対象油種を持っていないレコードはスキップ
                        If oilCode = "" OrElse retVal.StockList.ContainsKey(oilCode) Then
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
    Private Function GetTargetStockData(sqlCon As SqlConnection, dispData As DispDataClass) As DispDataClass
        Dim retVal As DispDataClass = dispData

        Dim fromDateObj = dispData.StockDate.Values.FirstOrDefault
        Dim toDateObj = dispData.StockDate.Values.LastOrDefault
        '期間外の前日夕在庫取得判定用
        Dim prevDate As String = fromDateObj.ItemDate.AddDays(-1).ToString("yyyy/MM/dd")
        '一年前の過去日取得用
        Dim pastDateList As New List(Of String) '取得必要過去日リスト
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT format(OS.STOCKYMD,'yyyy/MM/dd') AS STOCKYMD")
        sqlStat.AppendLine("      ,OS.OILCODE")
        sqlStat.AppendLine("      ,isnull(OS.MORSTOCK,0)    AS MORSTOCK")
        sqlStat.AppendLine("      ,isnull(OS.SHIPPINGVOL,0) AS SHIPPINGVOL")
        sqlStat.AppendLine("      ,isnull(OS.ARRVOL,0)      AS ARRVOL")
        sqlStat.AppendLine("      ,isnull(OS.ARRLORRYVOL,0) AS ARRLORRYVOL")
        sqlStat.AppendLine("      ,isnull(OS.EVESTOCK,0)    AS EVESTOCK")
        sqlStat.AppendLine("  FROM OIL.OIT0001_OILSTOCK OS")
        sqlStat.AppendLine(" WHERE OS.STOCKYMD BETWEEN dateadd(day, -1, @FROMDATE) AND @TODATE")
        sqlStat.AppendLine("   AND OS.OFFICECODE    = @OFFICECODE")
        sqlStat.AppendLine("   AND OS.CONSIGNEECODE = @CONSIGNEECODE")
        sqlStat.AppendLine("   AND OS.DELFLG        = @DELFLG")
        sqlStat.AppendLine(" ORDER BY OS.STOCKYMD,OS.OILCODE")
        '抽出結果なし且つ範囲が未来日部分に関して１年前の過去実績の払出を設定
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            '固定パラメータの設定
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispData.SalesOffice
                .Add("@CONSIGNEECODE", SqlDbType.NVarChar).Value = dispData.Consignee
                .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
            End With
            '可変パラメータ
            Dim paramFromDate = sqlCmd.Parameters.Add("@FROMDATE", SqlDbType.Date)
            Dim paramToDate = sqlCmd.Parameters.Add("@TODATE", SqlDbType.Date)

            paramFromDate.Value = fromDateObj.KeyString
            paramToDate.Value = toDateObj.KeyString

            '指定年月の情報取得
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
                    dateValue = stockListCol.StockItemList(curDate)
                    dateValue.MorningStock = Convert.ToString(sqlDr("MORSTOCK")) '朝在庫
                    dateValue.Send = Convert.ToString(sqlDr("SHIPPINGVOL")) '払出
                    dateValue.Receive = Decimal.Parse(Convert.ToString(sqlDr("ARRVOL")))  '受入
                    dateValue.ReceiveFromLorry = Convert.ToString(sqlDr("ARRLORRYVOL")) '払出
                End While 'sqlDr.Read
            End Using 'sqlDr
        End Using 'sqlCmd

        Return retVal
    End Function
    ''' <summary>
    ''' 在庫テーブルより既登録データを取得
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
                    Dim prmData As Hashtable = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, retVal.MiSalesOffice)
                    Dim rtn As String = ""
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, retVal.MiSalesOffice, retVal.MiSalesOfficeName, rtn, prmData)

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
    ''' 在庫テーブル登録処理
    ''' </summary>
    ''' <param name="sqlCon">SQL接続文字</param>
    ''' <param name="dispDataClass">画面入力データクラス</param>
    ''' <returns></returns>
    ''' <remarks>データがあれば更新、なければ追加（OIT0001_OILSTOCKテーブル内での履歴登録は無し）
    ''' 一旦、新規登録後に油種マスタから削除された後の更新パターンは考慮しない
    ''' （このパターンは画面上は出ないが宙に浮いたDELFLGが生きたままのデータが残る想定）</remarks>
    Private Function EntryStockData(sqlCon As SqlConnection, dispDataClass As DispDataClass, ByRef errNum As String, Optional procDtm As Date = #1900/01/01#) As Boolean

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
        sqlStat.AppendLine("            AND CONSIGNEECODE = @CONSIGNEECODE")
        'INSERT
        sqlStat.AppendLine("     IF (@@FETCH_STATUS <> 0)")
        sqlStat.AppendLine("         INSERT INTO OIL.OIT0001_OILSTOCK (")
        sqlStat.AppendLine("             STOCKYMD")
        sqlStat.AppendLine("            ,OFFICECODE")
        sqlStat.AppendLine("            ,OILCODE")
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
        journalSqlStat.AppendLine("  FROM OIL.OIT0001_OILSTOCK")
        journalSqlStat.AppendLine(" WHERE OFFICECODE    = @OFFICECODE")
        journalSqlStat.AppendLine("   AND CONSIGNEECODE = @CONSIGNEECODE")
        journalSqlStat.AppendLine("   AND UPDYMD        = @UPDYMD")

        '処理日付引数が初期値なら現時刻設定
        If procDtm.ToString("yyyy/MM/dd").Equals("1900/01/01") Then
            procDtm = Now
        End If

        'トランザクションしない場合は「sqlCon.BeginTransaction」→「nothing」
        Using tran As SqlTransaction = sqlCon.BeginTransaction(),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
            sqlCmd.CommandTimeout = 300
            '固定パラメータ
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = dispDataClass.SalesOffice
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
            'コミット
            tran.Commit()
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
    ''' ジャーナル書き込み
    ''' </summary>
    ''' <param name="journalDt"></param>
    ''' <returns></returns>
    Private Function OutputJournal(journalDt As DataTable) As Boolean
        For Each dr As DataRow In journalDt.Rows
            CS0020JOURNAL.TABLENM = "OIT0001"
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
        End If 'checkObj.ShowSuggestList = True '受注提案表が画面表示しているか

        '在庫表 払出入力チェック
        If {"WF_ButtonORDERLIST", "WF_ButtonRECULC"}.Contains(callerButton) Then
            For Each stockListItem In checkObj.StockList.Values
                Dim oilName As String = stockListItem.OilTypeName
                For Each itm In stockListItem.StockItemList.Values

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
                '列車番号別のクラスを取得
                trainValueClassItem = dateValueClassItem.SuggestOrderItem(trainId)
                '画面情報クラスに設定しているチェックOn/Offの情報を格納
                trainValueClassItem.CheckValue = chkObj.Checked
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
        Dim receiveFromLorryObj As TextBox = Nothing
        Dim receiveFromLorryVal As String = ""
        Dim stockListClass = dispDataClass.StockList
        Dim stockListCol As DispDataClass.StockListCollection = Nothing
        Dim stockListItm As DispDataClass.StockListItem = Nothing
        For Each repOilTypeItem As RepeaterItem In repStockItemObj.Items
            oilTypeCodeObj = DirectCast(repOilTypeItem.FindControl("hdnOilTypeCode"), HiddenField)
            oilTypeCode = oilTypeCodeObj.Value
            repStockVal = DirectCast(repOilTypeItem.FindControl("repStockValues"), Repeater)
            stockListCol = stockListClass(oilTypeCode)
            For Each repStockValItem As RepeaterItem In repStockVal.Items
                dateKeyObj = DirectCast(repStockValItem.FindControl("hdnDateKey"), HiddenField)
                dateKeyStr = dateKeyObj.Value
                sendObj = DirectCast(repStockValItem.FindControl("txtSend"), TextBox)
                sendVal = sendObj.Text
                morningStockObj = DirectCast(repStockValItem.FindControl("txtMorningStock"), TextBox)
                morningStockVal = morningStockObj.Text
                receiveFromLorryObj = DirectCast(repStockValItem.FindControl("txtReceiveFromLorry"), TextBox)
                receiveFromLorryVal = receiveFromLorryObj.Text

                stockListItm = stockListCol.StockItemList(dateKeyStr)
                stockListItm.Send = sendVal
                stockListItm.SendTextClientId = sendObj.ClientID
                stockListItm.MorningStock = morningStockVal
                stockListItm.MorningStockClientId = morningStockObj.ClientID
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
            retVal.Weight = Me.Weight
            retVal.MaxTankCap = Me.MaxTankCap
            retVal.TankCapRate = Me.TankCapRate
            retVal.DS = Me.DS
            retVal.LastSendAverage = Me.LastSendAverage
            retVal.IsOilTypeSwitch = Me.IsOilTypeSwitch
            retVal.FromMd = Me.FromMd
            retVal.ToMd = Me.ToMd
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
            If Me.KeyString >= Now.ToString("yyyy/MM/dd") Then
                Me.IsPastDay = False
            Else
                Me.IsPastDay = True
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
        ''' 荷受人（油槽所）
        ''' </summary>
        ''' <returns></returns>
        Public Property Consignee As String = ""
        ''' <summary>
        ''' 受注提案タンク車数リストプロパティ
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Key=日付 Value=列車、油種、チェックボックス、受入数を加味したリスト</remarks>
        Public Property SuggestList As New Dictionary(Of String, SuggestItem)
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
        ''' 在庫一覧日付部分
        ''' </summary>
        ''' <returns></returns>
        Public Property StockDate As Dictionary(Of String, DaysItem)
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
                       officeCode As String, consigneeCode As String)
            Me.SalesOffice = officeCode
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
        ''' 入力項目を0クリア・チェックボックスを未チェックにするメソッド
        ''' </summary>
        ''' <remarks>初日朝在庫は保持</remarks>
        Public Sub InputValueToZero()
            '提案表クリア
            SuggestValueInputValueToZero()

            '在庫表クリア
            For Each odrItem In Me.StockList.Values
                For Each trainIdItem In odrItem.StockItemList.Values
                    trainIdItem.Send = "0" '払い出し0クリア
                Next
            Next
        End Sub
        ''' <summary>
        ''' 提案表部分の0クリア
        ''' </summary>
        ''' <remarks>自動提案でも使用するため外だし</remarks>
        Private Sub SuggestValueInputValueToZero()
            '提案表クリア
            For Each suggestItm In SuggestList.Values
                For Each odrItem In suggestItm.SuggestOrderItem.Values
                    odrItem.CheckValue = False 'チェックボックスを未チェック
                    For Each itm In odrItem.SuggestValuesItem.Values
                        itm.ItemValue = "0" 'テキストをすべて0
                    Next
                Next
            Next
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
        ''' <param name="dateKey">日付(yyyy/MM/dd形式)</param>
        ''' <param name="oilCode">油種コード</param>
        ''' <returns></returns>

        Private Function GetSummarySuggestValue(dateKey As String, oilCode As String) As Decimal
            Dim retVal As Decimal = 0
            'ありえないが対象の日付データkeyが無ければ例外Throw
            If Me.SuggestList.ContainsKey(dateKey) = False Then
                Throw New Exception(String.Format("提案表データ(Key={0})が未存在", dateKey))
            End If

            For Each tgtItm In Me.SuggestList(dateKey).SuggestOrderItem.Values
                'チェックをしている値のみ合計する
                If tgtItm.CheckValue Then
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
                    If stockListItm.StockItemList.ContainsKey(prevDayKey) Then
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
                    If Me.ShowSuggestList = True AndAlso itm.DaysItem.IsPastDay = False AndAlso needsSumSuggestValue Then
                        '提案リスト表示時
                        itm.Receive = GetSummarySuggestValue(itmDate, oilCode)
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
            Return
            'TODO inventoryDaysの意味合い0ありにして0は先頭日か？
            '一旦0あり先頭
            '提案表部分の値を0クリア
            SuggestValueInputValueToZero()
            Dim fromDay As String = Me.StockDate.First.Value.KeyString
            Dim toDay As String = Me.StockDate.First.Value.ItemDate.AddDays(inventoryDays).ToString("yyyy/MM/dd")
            '過去日を除く開始日＋inventryDaysが処理条件
            Dim targetDays = From itm In Me.StockDate
                             Where itm.Key >= fromDay AndAlso
                                   itm.Key <= toDay AndAlso
                                   itm.Value.IsPastDay = False
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
            'Dim suggestTrainItem As aaa
            '処理日付のループ
            For Each targetDay In targetDays
                If Me.SuggestList.ContainsKey(targetDay) = False Then
                    Continue For
                End If
                '対象日の列車・油種別の提案リスト取得
                suggestItem = Me.SuggestList(targetDay)
                '列車別ループ
                For Each trainInfo In Me.TrainList.Values
                    'Dim suggestTrainItem = suggestItem.SuggestOrderItem(trainInfo.TrainNo)
                    '油種別ループ
                    For Each oilItem In Me.OilTypeList



                    Next oilItem
                Next trainInfo
            Next targetDay
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
            ''' 受入数情報格納用ディクショナリ
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

            Public Sub RelateMoveInside()
                For Each suggestOrder In Me.SuggestOrderItem
                    Dim itm = Me.SuggestMiOrderItem(suggestOrder.Key)
                    suggestOrder.Value.MiSuggestValuesItem = itm.SuggestValuesItem
                Next
            End Sub
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
                ''' 列車情報クラス
                ''' </summary>
                ''' <returns></returns>
                Public Property TrainInfo As TrainListItem
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
                Me.Stock80 = Math.Round(oilTypeItem.MaxTankCap * 0.8D, 1)
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
                Me.Receive = 0
                Me.ReceiveFromLorry = "0"
                Me.Send = "0" '画面入力項目の為文字
                Me.EveningStock = 0
                Me.EveningStockWithoutDS = 0
                Me.FreeSpace = 0
                Me.StockRate = 0
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
            Public Property Receive As Decimal
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
                    retVal = Me.Receive
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
        End Class
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

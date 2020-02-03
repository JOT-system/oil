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
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            daysList = GetTargetDateList(sqlCon, baseDate)
            oilTypeList = GetTargetOilType(sqlCon, salesOffice)
            trainList = GetTargetTrain(sqlCon, salesOffice, consignee)
        End Using
        Dim dispDataObj = New DispDataClass(daysList, trainList, oilTypeList)
        'コンストラクタで生成したデータを画面に貼り付け
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

        Dim dispClass = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
        If WW_Check(dispClass, WF_ButtonClick.Value) = False Then
            Return
        End If
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
            retVal.Add("5972", New TrainListItem("5972", "5972-南松本"))
        End If
        If salesOffice = "011203" AndAlso consignee = "30" Then
            retVal.Add("8877", New TrainListItem("8877", "8877-倉賀野"))
            retVal.Add("8883", New TrainListItem("8883", "8883-倉賀野"))
        End If
        '根岸
        If salesOffice = "011402" AndAlso consignee = "10" Then
            retVal.Add("5463", New TrainListItem("5463", "5463-坂城"))
            retVal.Add("2085", New TrainListItem("2085", "2085-坂城"))
            retVal.Add("8471", New TrainListItem("8471", "8471-坂城"))
        End If
        If salesOffice = "011402" AndAlso consignee = "20" Then
            retVal.Add("81", New TrainListItem("81", "81-竜王"))
            retVal.Add("83", New TrainListItem("83", "83-竜王"))
        End If
        '三重塩浜
        If salesOffice = "012402" AndAlso consignee = "40" Then
            retVal.Add("5282", New TrainListItem("5282", "5282-南松本"))
            retVal.Add("8072", New TrainListItem("8072", "8072-南松本"))
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
    ''' <returns>キー:油種コード、値：油種アイテムクラス</returns>
    Private Function GetTargetOilType(sqlCon As SqlConnection, salesOffice As String) As Dictionary(Of String, OilItem)
        Dim retVal As New Dictionary(Of String, OilItem)
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
        'FixValueに中分類大分類を追加したらJOIN不要で↑のSQLを元に改修
        'sqlStr.AppendLine("SELECT FV.KEYCODE  AS OILCODE")
        'sqlStr.AppendLine("      ,FV.VALUE1   AS OILNAME")
        'sqlStr.AppendLine("      ,PD.BIGOILCODE      AS BIGOILCODE")
        'sqlStr.AppendLine("      ,PD.MIDDLEOILCODE   AS MIDDLEOILCODE")
        'sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FV")
        'sqlStr.AppendLine(" INNER JOIN OIL.OIM0003_PRODUCT PD")
        'sqlStr.AppendLine("    ON FV.CAMPCODE = PD.OFFICECODE")
        'sqlStr.AppendLine("   AND FV.KEYCODE  = PD.OILCODE")
        'sqlStr.AppendLine("   AND FV.VALUE2   = PD.SEGMENTOILCODE")
        'sqlStr.AppendLine("   AND PD.DELFLG = @DELFLG")
        'sqlStr.AppendLine(" WHERE FV.CAMPCODE = @CAMPCODE")
        'sqlStr.AppendLine("   AND FV.CLASS    = @CLASS")
        'sqlStr.AppendLine("   AND FV.DELFLG   = @DELFLG")
        'sqlStr.AppendLine(" ORDER BY KEYCODE")

        'DBより取得を行い祝祭日情報付与
        Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@CAMPCODE", SqlDbType.NVarChar).Value = salesOffice
                .Add("@CLASS", SqlDbType.NVarChar).Value = "PRODUCTPATTERN"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = "0"
                .Add("@STOCKFLG", SqlDbType.NVarChar).Value = "9" '不等号条件
            End With

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
        End Using 'sqlCmd
        Return retVal

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
        Dim oilTypeCodeObj As HiddenField = Nothing
        Dim oilTypeCode As String = ""
        Dim suggestValObj As TextBox = Nothing
        Dim suggestVal As String = ""

        Dim dateValueClassItem As DispDataClass.SuggestItem = Nothing
        Dim trainValueClassItem As DispDataClass.SuggestItem.SuggestValues = Nothing
        Dim oilTypeValueClassItem As DispDataClass.SuggestItem.SuggestValue = Nothing
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

                stockListItm = stockListCol.StockItemList(dateKeyStr)
                stockListItm.Send = sendVal
                stockListItm.SendTextClientId = sendObj.ClientID
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
            'Weight格納は一旦ベタ打ち
            Select Case oilCode
                Case "1001" 'ハイオク
                    Me.Weight = 0.75D
                Case "1101" 'レギュラー
                    Me.Weight = 0.75D
                Case "1301" '灯油
                    Me.Weight = 0.79D
                Case "1401" '軽油
                    Me.Weight = 0.75D
                Case "2101" 'Ａ重油
                    Me.Weight = 0.87D
                Case "2201" 'ＬＳＡ
                    Me.Weight = 0.87D
                Case "1302" '未添加灯油
                    Me.Weight = 0.75D
                Case "1404" '３号軽油
                    Me.Weight = 0.82D
                Case Else
                    Me.Weight = 0.75D
            End Select

        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(oilCode As String, oilName As String)
            Me.New(oilCode, oilName, "", "")
        End Sub
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
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="trainNo">列車番号</param>
        ''' <param name="trainName">列車名</param>
        Public Sub New(trainNo As String, trainName As String)
            Me.TrainNo = trainNo
            Me.TrainName = trainName
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
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="daysList">対象日リスト</param>
        ''' <param name="trainList">列車IDリスト</param>
        ''' <param name="oilTypeList">対象油種リスト</param>
        Public Sub New(daysList As Dictionary(Of String, DaysItem), trainList As Dictionary(Of String, TrainListItem), oilTypeList As Dictionary(Of String, OilItem))
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
        Public Sub InputValueToZero()
            '提案表クリア
            For Each suggestItm In SuggestList.Values
                For Each odrItem In suggestItm.SuggestOrderItem.Values
                    odrItem.CheckValue = False 'チェックボックスを未チェック
                    For Each itm In odrItem.SuggestValuesItem.Values
                        itm.ItemValue = "0" 'テキストをすべて0
                    Next
                Next
            Next
            '在庫表クリア
            For Each odrItem In Me.StockList.Values
                For Each trainIdItem In odrItem.StockItemList.Values
                    trainIdItem.Send = "0" '払い出し0クリア
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
                copiedItem = New OilItem(itm.Key, itm.Value.OilName, itm.Value.BigOilCode, itm.Value.MiddleOilCode)
                copiedItem.Weight = itm.Value.Weight
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
                        retVal = retVal + Decimal.Parse(tgtItm.SuggestValuesItem(oilCode).ItemValue)
                    End If
                End If
            Next
            Return retVal
        End Function
        ''' <summary>
        ''' 在庫表再計算処理
        ''' </summary>
        ''' <remarks>外部用呼出メソッド</remarks>
        Public Sub RecalcStockList()
            '日付毎のループ
            For Each stockListItm In Me.StockList.Values
                '当日の油種ごとのオブジェクトループ
                For Each itm In stockListItm.StockItemList.Values
                    Dim itmDate As String = itm.DaysItem.KeyString
                    Dim oilCode As String = stockListItm.OilTypeCode
                    '前日日付データ取得
                    Dim prevDayKey As String = itm.DaysItem.ItemDate.AddDays(-1).ToString("yyyy/MM/dd")
                    '前日データを元に実行する処理(前日データあり=一覧初日以外)
                    If stockListItm.StockItemList.ContainsKey(prevDayKey) Then
                        '前日のデータ
                        Dim prevItm = stockListItm.StockItemList(prevDayKey)
                        '◆1行目 前日夕在庫(前日データの夕在庫フィールドを格納)
                        itm.LastEveningStock = prevItm.EveningStock
                        '◆3行目 朝在庫 ※計算順序に営業するため2行目処理より前に持ってくること
                        itm.MorningStock = prevItm.MorningStock + prevItm.Receive - Decimal.Parse(prevItm.Send)

                    End If
                    '◆2行目 保有日数(朝在庫 / 前週出荷平均)
                    If stockListItm.LastShipmentAve = 0 Then
                        itm.Retentiondays = 0
                    Else
                        itm.Retentiondays = stockListItm.LastShipmentAve
                    End If
                    '◆4行目 受入数 (提案リストの値)
                    If Me.ShowSuggestList = True Then
                        '提案リスト表示時
                        itm.Receive = GetSummarySuggestValue(itmDate, oilCode)
                    Else
                        itm.Receive = 0 '？？？？？ここはどうする
                    End If
                    '◆5行目払出
                    '入力項目なので無視
                    '◆6行目 夕在庫 (朝在庫 + 受入- 払出)
                    itm.EveningStock = itm.MorningStock + itm.Receive - Decimal.Parse(itm.Send)
                    '◆7行名 夕在庫D/S (夕在庫 - D/S)
                    itm.EveningStockWithoutDS = itm.EveningStock - stockListItm.DS
                    '◆8行目 空き容量 (夕在庫 -  D/S)
                    itm.FreeSpace = stockListItm.TargetStock - ((itm.MorningStock + itm.Receive) - Decimal.Parse(itm.Send))
                    '◆9行目 在庫率
                    If stockListItm.TargetStockRate = 0 Then
                        itm.StockRate = 0
                    Else
                        itm.StockRate = itm.FreeSpace / stockListItm.TargetStockRate
                    End If
                Next itm '当日の油種ごとのオブジェクトループ

            Next stockListItm '日付毎のループ

        End Sub
        ''' <summary>
        ''' 自動提案計算処理
        ''' </summary>
        ''' <remarks>外部呼出用メソッド</remarks>

        Public Sub AutoSuggest()

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
                    orderValues.Add(oilCodeItem, "0", Me.DayInfo)
                Next
                'orderValues.Add(New OilItem(SUMMARY_CODE, "合計"), "0", Me.DayInfo)
                Me.SuggestOrderItem.Add(trainInfo.TrainNo, orderValues)
                Me.TrainInfo = trainInfo

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
                Public Sub Add(oilInfo As OilItem, val As String, dayItm As DaysItem)
                    Me.SuggestValuesItem.Add(oilInfo.OilCode, New SuggestValue _
                        With {.ItemValue = val, .OilInfo = oilInfo, .DayInfo = dayItm})
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
                Me.TankCapacity = 12345.6D
                Me.TargetStock = 0
                Me.TargetStockRate = 0
                Me.Stock80 = 0
                Me.DS = 0
                Me.LastShipmentAve = 0
                Me.StockItemList = New Dictionary(Of String, StockListItem)
                For Each dateVal In dateItem
                    Dim item = New StockListItem(dateVal.Key, dateVal.Value)
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
                Me.MorningStock = 0
                Me.Receive = 0
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
            Public Property MorningStock As Decimal
            ''' <summary>
            ''' 受入
            ''' </summary>
            ''' <returns></returns>
            Public Property Receive As Decimal
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

Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' タンク所在管理状況画面クラス
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
Public Class OIT0005TankLocCondition
    Inherits System.Web.UI.Page
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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                'Dim dispDataObj As DemoDispDataClass
                'dispDataObj = GetThisScreenData(Me.frvSuggest, Me.repStockOilTypeItem)
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    'Master.RecoverTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value

                        Case "WF_ButtonEND"                 '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_RIGHT_VIEW_DBClick"        '右ボックスダブルクリック
                            WF_RIGHTBOX_DBClick()
                        Case "WF_MEMOChange"                'メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case Else
                            If WF_ButtonClick.Value.StartsWith("WF_ButtonShowList") Then
                                Dim detailKbn As String = ""
                                detailKbn = WF_ButtonClick.Value.Replace("WF_ButtonShowList", "")
                                WF_ButtonShowList_Click(detailKbn)
                            End If
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
        Master.MAPID = OIT0005WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        ''○初期値設定
        'WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        'WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        'rightview.ResetIndex()
        'leftview.ActiveListBox()

        ''右Boxへの値設定
        rightview.MAPIDS = OIT0005WRKINC.MAPIDC
        rightview.MAPID = OIT0005WRKINC.MAPIDL
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0005C Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If
        '**********************************************
        '画面情報を元に各対象リストを生成
        '**********************************************
        Dim salesOffice As String = work.WF_SEL_SALESOFFICECODE.Text
        Dim salesOfficeName As String = work.WF_SEL_SALESOFFICE.Text

        Dim dispData As New DispDataClass(salesOffice)
        'DBよりデータ取得しタンク数量取得
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            For Each condItm In dispData.ConditionList
                condItm = GetTankCondCount(sqlCon, condItm, dispData.SalesOffice)
            Next condItm
        End Using
        '****************************************
        '生成したデータを画面に貼り付け
        '****************************************
        Me.repCondition.DataSource = dispData.ConditionList
        Me.repCondition.DataBind()
    End Sub
    ''' <summary>
    ''' 各種タンク数を取得する
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="condItem">１パネル分の画面情報クラス</param>
    ''' <param name="salesOffice">営業所コード</param>
    ''' <returns></returns>
    Private Function GetTankCondCount(sqlCon As SqlConnection, condItem As ConditionItem, salesOffice As String) As ConditionItem
        Dim retVal = condItem
        Dim viewName As String = work.GetTankViewName(condItem.DetailType)
        'ビュー名が取得できない場合はそのまま終了
        If viewName = "" Then
            Return retVal
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       ISNULL(SUM(CASE WHEN VTS.ISCOUNT1GROUP='1' THEN 1 ELSE 0 END),0) AS COUNTGROUP1")
        sqlStat.AppendLine("      ,ISNULL(SUM(CASE WHEN VTS.ISCOUNT2GROUP='1' THEN 1 ELSE 0 END),0) AS COUNTGROUP2")
        sqlStat.AppendFormat("  FROM {0} VTS", viewName).AppendLine()
        sqlStat.AppendLine(" WHERE VTS.OFFICECODE = @OFFICECODE")
        Using sqlCmd = New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@OFFICECODE", SqlDbType.NVarChar).Value = salesOffice
            End With
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                Dim retVal1 As Decimal = 0
                Dim retVal2 As Decimal = 0
                While sqlDr.Read
                    retVal1 = CDec(sqlDr("COUNTGROUP1"))
                    retVal2 = CDec(sqlDr("COUNTGROUP2"))
                End While
                condItem.Value1 = retVal1
                condItem.Value2 = retVal2
            End Using
        End Using
        Return condItem
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
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(work.WF_SEL_CAMPCODE.Text, WW_DUMMY)

    End Sub
    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub
    ''' <summary>
    ''' 内訳を見るボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonShowList_Click(detailKbn As String)

        ''○ 条件選択画面の入力値退避
        'work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        'work.WF_SEL_ORG.Text = WF_ORG.Text                  '組織コード
        ''営業所
        'work.WF_SEL_SALESOFFICECODEMAP.Text = TxtSalesOffice.Text
        'work.WF_SEL_SALESOFFICECODE.Text = TxtSalesOffice.Text
        'work.WF_SEL_SALESOFFICE.Text = LblSalesOfficeName.Text
        work.WF_COND_DETAILTYPE.Text = detailKbn
        work.WF_COND_DETAILTYPENAME.Text = DispDataClass.GetDetailTypeName(detailKbn)
        '○ 画面レイアウト設定
        If Master.VIEWID = "" Then
            Master.VIEWID = rightview.GetViewId(work.WF_SEL_CAMPCODE.Text)
        End If

        Master.CheckParmissionCode(work.WF_SEL_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.TransitionPage()
        End If
    End Sub
    ''' <summary>
    ''' 画面表示アイテム保持クラス
    ''' </summary>
    <Serializable>
    Public Class DispDataClass
        Public Property SalesOffice As String = ""
        Public Property ConditionList As List(Of ConditionItem)
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Sub New(salesOffice As String)
            Me.SalesOffice = salesOffice
            Me.ConditionList = New List(Of ConditionItem)
            Me.ConditionList.AddRange({New ConditionItem("1", "残車状況", "残車数", 0, "交検間近", 0),
                                       New ConditionItem("2", "輸送状況", "翌日発送分", 0, "輸送中", 0),
                                       New ConditionItem("3", "回送状況", "回送指示中分", 0, "回送中", 0),
                                       New ConditionItem("4", "その他状況", "留置", 0, "その他", 0)})

        End Sub
        ''' <summary>
        ''' 状況表名の取得
        ''' </summary>
        ''' <param name="detailType"></param>
        ''' <returns></returns>
        Public Shared Function GetDetailTypeName(detailType As String) As String
            Dim tmpDetailType As New DispDataClass("")
            Dim retVal As String = (From itm In tmpDetailType.ConditionList Where itm.DetailType = detailType Select itm.ConditionName).FirstOrDefault
            Return retVal
        End Function
    End Class
    ''' <summary>
    ''' 画面表示のボックスアイテム
    ''' </summary>
    <Serializable>
    Public Class ConditionItem
        Public Sub New(detailType As String, conditionName As String, value1Name As String, value1 As Decimal,
                       value2Name As String, value2 As Decimal)
            Me.DetailType = detailType
            Me.ConditionName = conditionName
            Me.Value1Name = value1Name
            Me.Value1 = value1
            Me.Value2Name = value2Name
            Me.Value2 = value2

        End Sub
        Public Property DetailType As String = ""
        Public Property ConditionName As String = ""
        Public Property Value1Name As String = ""
        Public Property Value1 As Decimal = 0
        Public Property Value2Name As String = ""
        Public Property Value2 As Decimal = 0
    End Class

End Class
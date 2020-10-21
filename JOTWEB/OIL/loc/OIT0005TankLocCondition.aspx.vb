Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
Imports JOTWEB.OIT0005WRKINC
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
        Dim salesOfficeInStat As String = GRC0001TILESELECTORWRKINC.GetSelectedSqlInStatement(work.WF_SEL_SALESOFFICE_TILES.Text)

        Dim dispData As New DispDataClass(salesOfficeInStat)
        'DBよりデータ取得しタンク数量取得
        Using sqlCon = CS0050SESSION.getConnection
            sqlCon.Open()
            For Each condItm In dispData.ConditionList
                condItm = GetTankCondCount(sqlCon, condItm, dispData.SalesOfficeInStat)
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
    ''' <param name="salesOfficeInStat">営業所コード</param>
    ''' <returns></returns>
    Private Function GetTankCondCount(sqlCon As SqlConnection, condItem As ConditionItem, salesOfficeInStat As String) As ConditionItem
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
        If condItem.Value3Name <> "" Then
            sqlStat.AppendLine("      ,ISNULL(SUM(CASE WHEN VTS.ISCOUNT3GROUP='1' THEN 1 ELSE 0 END),0) AS COUNTGROUP3")
        End If
        sqlStat.AppendFormat("  FROM {0} VTS", viewName).AppendLine()
        sqlStat.AppendFormat(" WHERE VTS.OFFICECODE IN ({0})", salesOfficeInStat)
        Using sqlCmd = New SqlCommand(sqlStat.ToString, sqlCon)

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                Dim retVal1 As Decimal = 0
                Dim retVal2 As Decimal = 0
                Dim retVal3 As Decimal = 0
                While sqlDr.Read
                    retVal1 = CDec(sqlDr("COUNTGROUP1"))
                    retVal2 = CDec(sqlDr("COUNTGROUP2"))
                    If condItem.Value3Name <> "" Then
                        retVal3 = CDec(sqlDr("COUNTGROUP3"))
                    End If
                End While
                condItem.Value1 = retVal1
                condItem.Value2 = retVal2
                condItem.Value3 = retVal3
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

End Class
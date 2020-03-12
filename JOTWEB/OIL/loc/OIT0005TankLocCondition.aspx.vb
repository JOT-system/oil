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
                        Case "WF_ButtonShowList"
                            WF_ButtonShowList_Click()
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
        'WF_ButtonClick.Value = ""
        'WF_LeftboxOpen.Value = ""
        'WF_RightboxOpen.Value = ""
        'rightview.ResetIndex()
        'leftview.ActiveListBox()

        ''右Boxへの値設定
        'rightview.MAPID = Master.MAPID
        'rightview.MAPVARI = Master.MAPvariant
        'rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        'rightview.PROFID = Master.PROF_REPORT
        'rightview.Initialize(WW_DUMMY)

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

        Dim dispData As New DispDataClass(False)
        '****************************************
        '生成したデータを画面に貼り付け
        '****************************************
        Me.repCondition.DataSource = dispData.ConditionList
        Me.repCondition.DataBind()
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
    ''' 内訳を見るボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonShowList_Click()

        ''○ 条件選択画面の入力値退避
        'work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        'work.WF_SEL_ORG.Text = WF_ORG.Text                  '組織コード
        ''営業所
        'work.WF_SEL_SALESOFFICECODEMAP.Text = TxtSalesOffice.Text
        'work.WF_SEL_SALESOFFICECODE.Text = TxtSalesOffice.Text
        'work.WF_SEL_SALESOFFICE.Text = LblSalesOfficeName.Text

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
        Public Property ConditionList As List(Of ConditionItem)
        ''' <summary>
        ''' デモ用コンストラクタ
        ''' </summary>
        ''' <param name="isDemo"></param>
        Sub New(isDemo As Boolean)
            Me.ConditionList = New List(Of ConditionItem)
            Me.ConditionList.AddRange({New ConditionItem("残車状況", "残車数", 9991, "交検間近", 9992),
                                       New ConditionItem("輸送状況", "翌日発送分", 9993, "輸送中", 9993),
                                       New ConditionItem("回送状況", "回送指示中分", 9993, "回送中", 9993),
                                       New ConditionItem("その他状況", "留置", 9991, "その他", 9992)})

        End Sub
    End Class
    ''' <summary>
    ''' 画面表示のボックスアイテム
    ''' </summary>
    <Serializable>
    Public Class ConditionItem
        Public Sub New(conditionName As String, value1Name As String, value1 As Decimal,
                       value2Name As String, value2 As Decimal)
            Me.ConditionName = conditionName
            Me.Value1Name = value1Name
            Me.Value1 = value1
            Me.Value2Name = value2Name
            Me.Value2 = value2

        End Sub

        Public Property ConditionName As String = ""
        Public Property Value1Name As String = ""
        Public Property Value1 As Decimal = 0
        Public Property Value2Name As String = ""
        Public Property Value2 As Decimal = 0
    End Class

End Class
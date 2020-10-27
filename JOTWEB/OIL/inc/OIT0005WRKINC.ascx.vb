Option Strict On
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0005WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIT0005S"       'MAPID(条件)
    Public Const MAPIDC As String = "OIT0005C"       'MAPID(状況)
    Public Const MAPIDL As String = "OIT0005L"       'MAPID(一覧)
    Public Const MAPIDD As String = "OIT0005D"       'MAPID(詳細(登録))

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    '' <summary>
    '' 運用部署パラメーター
    '' </summary>
    '' <param name="I_COMPCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Public Function CreateORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateORGParam = prmData

    End Function
    ''' <summary>
    ''' 営業所の取得
    ''' </summary>
    ''' <param name="I_SALESOFFICEPT"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateSALESOFFICEParam(ByVal I_COMPCODE As String, ByVal I_SALESOFFICEPT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_SALESOFFICE) = I_SALESOFFICEPT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateSALESOFFICEParam = prmData
    End Function
    '' <summary>
    '' 固定値マスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "", Optional ByVal I_ADDITIONALCONDITION As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        If I_ADDITIONALCONDITION <> "" Then
            prmData.Item(C_PARAMETERS.LP_ADDITINALCONDITION) = I_ADDITIONALCONDITION
        End If
        CreateFIXParam = prmData
    End Function
    ''' <summary>
    ''' ビューNo取得メソッド
    ''' </summary>
    ''' <param name="detailType">状況表種類(1:残車,2:輸送,3:回送,4:その他)</param>
    ''' <returns></returns>
    Public Function GetTankViewName(detailType As String) As String
        Dim viewName As String = ""
        Select Case detailType
            Case "1" '残車状況
                viewName = "OIL.VIW0008_TANKLOC01_ZANSHA"
            Case "2" '輸送状況
                viewName = "OIL.VIW0008_TANKLOC02_TRANS"
            Case "3" '回送中（交検）
                viewName = "OIL.VIW0008_TANKLOC03_FORWARD_KK"
            Case "4" '回送中（全検）
                viewName = "OIL.VIW0008_TANKLOC04_FORWARD_ZK"
            Case "5" '回送中（修理）
                viewName = "OIL.VIW0008_TANKLOC05_FORWARD_RP"
            Case "7" '回送中（疎開留置）
                viewName = "OIL.VIW0008_TANKLOC07_FORWARD_SR"
            Case "8" '回送（ＭＣ）状況
                viewName = "OIL.VIW0008_TANKLOC08_FORWARD_MC"
            'Case "9" 'その他状況
            '    viewName = "OIL.VIW0008_TANKLOC09_OTHERS"
            Case "9" 'その他状況(受注(未卸中・交検中・留置中))
                viewName = "OIL.VIW0008_TANKLOC09_ORDER_MKR"
        End Select
        Return viewName
    End Function
    ''' <summary>
    ''' ビューソート部分取得メソッド
    ''' </summary>
    ''' <param name="detailType">状況表種類(1:残車,2:輸送,3:回送,4:その他)</param>
    ''' <returns></returns>
    Public Function GetTankViewOrderByString(detailType As String) As String
        Dim viewName As String = ""
        Select Case detailType
            Case "1" '残車状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "2" '輸送状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "3" '回送状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "4" '回送状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "5" '回送状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "7" '回送状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "8" '回送状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "9" 'その他状況
                'viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
                viewName = "CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
        End Select
        Return viewName
    End Function

    ''' <summary>
    ''' 画面表示アイテム保持クラス
    ''' </summary>
    <Serializable>
    Public Class DispDataClass
        Public Property SalesOfficeInStat As String = ""
        Public Property ConditionList As List(Of ConditionItem)
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Sub New(salesOfficeInStat As String)
            Me.SalesOfficeInStat = salesOfficeInStat
            Me.ConditionList = New List(Of ConditionItem)
            Me.ConditionList.AddRange({New ConditionItem("1", "残車状況", "残車数", 0, "交検間近", 0),
                                       New ConditionItem("2", "輸送状況", "翌日発送分", 0, "輸送中", 0),
                                       New ConditionItem("3", "回送（交検）", "回送指示中分", 0, "回送中", 0),
                                       New ConditionItem("4", "回送（全検）", "回送指示中分", 0, "回送中", 0),
                                       New ConditionItem("5", "回送（修理）", "回送指示中分", 0, "回送中", 0),
                                       New ConditionItem("7", "回送（<span style='letter-spacing:0;'>疎開留</span>置）", "回送指示中分", 0, "回送中", 0),
                                       New ConditionItem("8", "回送（ＭＣ）", "回送指示中分", 0, "回送中", 0),
                                       New ConditionItem("9", "その他状況", "未卸中", 0, "交検中", 0, "留置中", 0)})
            'New ConditionItem("9", "その他状況", "留置", 0, "その他", 0)})

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
        ''' <summary>
        ''' 状況の合計名のリスト取得
        ''' </summary>
        ''' <param name="detailType"></param>
        ''' <returns></returns>
        Public Shared Function GetDetailInsideNames(detailType As String) As Dictionary(Of String, String)
            Dim tmpDetailType As New DispDataClass("")
            Dim selectedDetail = (From itm In tmpDetailType.ConditionList Where itm.DetailType = detailType Select itm).FirstOrDefault
            Dim dicRetVal As New Dictionary(Of String, String)
            dicRetVal.Add("1", selectedDetail.Value1Name)
            dicRetVal.Add("2", selectedDetail.Value2Name)
            If detailType = "9" Then dicRetVal.Add("3", selectedDetail.Value3Name)
            Return dicRetVal
        End Function
    End Class
    ''' <summary>
    ''' 画面表示のボックスアイテム
    ''' </summary>
    <Serializable>
    Public Class ConditionItem
        Public Sub New(detailType As String, conditionName As String, value1Name As String, value1 As Decimal,
                       value2Name As String, value2 As Decimal, Optional value3Name As String = Nothing, Optional value3 As Decimal = 0)
            Me.DetailType = detailType
            Me.ConditionName = conditionName
            Me.Value1Name = value1Name
            Me.Value1 = value1
            Me.Value2Name = value2Name
            Me.Value2 = value2
            Me.Value3Name = value3Name
            Me.Value3 = value3

        End Sub
        Public Property DetailType As String = ""
        Public Property ConditionName As String = ""
        Public Property Value1Name As String = ""
        Public Property Value1 As Decimal = 0
        Public Property Value2Name As String = ""
        Public Property Value2 As Decimal = 0
        Public Property Value3Name As String = ""
        Public Property Value3 As Decimal = 0
    End Class

End Class

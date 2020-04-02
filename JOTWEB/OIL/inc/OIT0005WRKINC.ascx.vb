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
            Case "3" '回送状況
                viewName = "OIL.VIW0008_TANKLOC03_FORWARD"
            Case "4" 'その他状況
                viewName = "OIL.VIW0008_TANKLOC04_OTHERS"
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
                viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "2" '輸送状況
                viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "3" '回送状況
                viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
            Case "4" 'その他状況
                viewName = "NONOPERATIONDAYS DESC, CONVERT(decimal(16,2),case when isnumeric(TANKNUMBER)=1 then TANKNUMBER else null end)"
        End Select
        Return viewName
    End Function


End Class

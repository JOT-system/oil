Imports System.Data.SqlClient

Public Class GRT00006WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "T00006S"                       'MAPID(選択)
    Public Const MAPID As String = "T00006"                         'MAPID(実行)

    Public Const C_KOUEI_CLASS_CODE As String = "T00006_KOUEIORG"   '光英連携可否判定用FIXVAL KEY
    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks>EMPTY以外</remarks>]
    Public Sub initialize()
    End Sub

    ''' <summary>
    ''' コントロールオブジェクト取得
    ''' </summary>
    ''' <param name="I_FIELD" >コントロール名称</param>
    ''' <returns >Control</returns>
    ''' <remarks>マスターページ内コンテンツ領域(contents1)が対象</remarks>
    Public Function getControl(ByVal I_FIELD As String) As Control
        Try
            Return Page.Master.FindControl("contents1").FindControl(I_FIELD)
        Catch ex As Exception
            ' 指定コントロール不明
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function

    ''' <summary>
    ''' 取引先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createTORIParam(ByVal COMPCODE As String, Optional ByVal ORGCODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.OWNER
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        createTORIParam = prmData
    End Function

    ''' <summary>
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createORGParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        createORGParam = prmData
    End Function

    ''' <summary>
    ''' 届先一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="SHIPCODE">取引先コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createDistinationParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal SHIPCODE As String, ByVal CLASSCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CUSTOMER) = SHIPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CLASSCODE) = CLASSCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_CUSTOMER_TYPE.ALL
        Return prmData
    End Function

    ''' <summary>
    ''' 品名一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE" >部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createGoodsParam(ByVal COMPCODE As String, ByVal ORGCODE As String, Optional ByVal isMaster As Boolean = False) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_COMP) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        If isMaster Then
            '品名名称にコード表示追加
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS_MST
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS
        End If

        Return prmData
    End Function

    ''' <summary>
    ''' 品名1一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createGoods1Param(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS1_MST

        Return prmData
    End Function

    ''' <summary>
    ''' 社員一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function createSTAFFParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If Not String.IsNullOrEmpty(ORGCODE) Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        End If
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.DRIVER
        Return prmData
    End Function

    ''' <summary>
    ''' 業務車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createWorkLorryParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        Return prmData
    End Function

    ''' <summary>
    ''' 統一車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ISFRONT">前方車両フラグ</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createCarCodeParam(ByVal COMPCODE As String, ByVal ISFRONT As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        If ISFRONT Then
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.FRONT
        Else
            prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.REAR
        End If

        Return prmData
    End Function
    ''' <summary>
    ''' 統一車番一覧取得用パラメータ設定
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createCarCodeParam(ByVal COMPCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        Return prmData
    End Function
End Class
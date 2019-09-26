Public Class GRTA0001WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "TA0001S"                        'MAPID(選択)
    Public Const MAPID As String = "TA0001"                          'MAPID(実行)

    Public Class C_TUMI_NAME
        Public Const TUMIOKI As String = "積置"
        Public Const TUMIHAI As String = "積配"
    End Class

    ''' <summary>
    ''' 一覧表示選択情報
    ''' </summary>
    Public Class C_LIST_FUNSEL
        '乗務員別
        Public Const DRIVER As String = "1"
        '車番別
        Public Const CARNUM As String = "2"
        '出荷場所別
        Public Const DESTPOS As String = "3"
    End Class

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXCODE">固定値コード</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        Return prmData
    End Function
    ''' <summary>
    ''' 業務車番一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function CreateLorryParam(ByVal COMPCODE As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        Return prmData
    End Function

    ''' <summary>
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="PRMIT">権限区分</param>
    ''' <returns>検索条件テーブル</returns>
    ''' <remarks></remarks>
    Function CreateSHIPORGParam(ByVal COMPCODE As String, ByVal PRMIT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_PERMISSION) = PRMIT
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        Return prmData
    End Function
End Class
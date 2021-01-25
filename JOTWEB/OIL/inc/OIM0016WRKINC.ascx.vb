Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0016WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0016S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0016L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0016C"       'MAPID(更新)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 管轄受注営業所パラメーター
    ''' </summary>
    ''' <param name="I_OFFICECODE"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateOfficeCodeParam(ByVal I_COMPCODE As String, Optional ByVal I_OFFICECODE As String = Nothing) As Hashtable

        Dim prmData As New Hashtable

        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_SALESOFFICE) = I_OFFICECODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL

        Return prmData

    End Function

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function

End Class
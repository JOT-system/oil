Imports OFFICE.GRIS0005LeftBox

Public Class GRMD0001WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "MD0001S"       'MAPID(選択)
    Public Const MAPID As String = "MD0001"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

    End Sub

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

    ''' <summary>
    ''' 品名１一覧の取得
    ''' </summary>
    ''' <param name="CAMPCODE"></param>
    ''' <param name="OILTYPE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateGoods1Param(ByVal CAMPCODE As String, ByVal OILTYPE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS1_MST
        prmData.Item(C_PARAMETERS.LP_COMPANY) = CAMPCODE
        prmData.Item(C_PARAMETERS.LP_OILTYPE) = OILTYPE
        CreateGoods1Param = prmData
    End Function

End Class

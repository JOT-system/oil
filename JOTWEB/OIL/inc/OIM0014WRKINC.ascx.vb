Imports JOTWEB.GRIS0005LeftBox

Public Class OIM0014WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "OIM0014S"       'MAPID(条件)
    Public Const MAPIDL As String = "OIM0014L"       'MAPID(実行)
    Public Const MAPIDC As String = "OIM0014C"       'MAPID(更新)

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    '' <summary>
    '' 固定値マスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = prmData
    End Function

End Class

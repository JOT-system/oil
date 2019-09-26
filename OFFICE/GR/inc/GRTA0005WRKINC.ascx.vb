Public Class GRTA0005WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "TA0005S"                        'MAPID(選択)
    Public Const MAPID As String = "TA0005"                          'MAPID(実行)

    ''' <summary>
    ''' セレクター項目（全て）
    ''' </summary>
    Public Class ALL_SELECTOR
        Public Const CODE As String = "00000"
        Public Const NAME As String = "全て"
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

End Class
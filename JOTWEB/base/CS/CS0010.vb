Option Strict On
''' <summary>
''' 文字編集
''' </summary>
''' <remarks>不要文字を除去する</remarks>
Public Structure CS0010CHARget

    ''' <summary>
    ''' 除去する対象の文字列
    ''' </summary>
    ''' <value>除去元文字列</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CHARIN() As String

    ''' <summary>
    ''' 除去した後の文字列
    ''' </summary>
    ''' <value>除去先文字列</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CHAROUT() As String

    ''' <summary>
    ''' 不要文字を除去する
    ''' </summary>
    ''' <remarks><para>不要文字を除去する</para>
    ''' </remarks>
    Public Sub CS0010CHARget()

        CHAROUT = CHARIN

        '●特定文字の置き換え
        '半角文字11文字：< > ' % , " ; & | `
        CHAROUT = CHAROUT.Replace("<", "")
        CHAROUT = CHAROUT.Replace(">", "")
        CHAROUT = CHAROUT.Replace("'", "")
        CHAROUT = CHAROUT.Replace("%", "")
        CHAROUT = CHAROUT.Replace(",", "")
        CHAROUT = CHAROUT.Replace(";", "")
        CHAROUT = CHAROUT.Replace("&", "")
        CHAROUT = CHAROUT.Replace("|", "")
        CHAROUT = CHAROUT.Replace("`", "")
        CHAROUT = CHAROUT.Replace(ControlChars.Quote, "")

        '全角文字3文字：％ ∥ － （全角のマイナス）
        CHAROUT = CHAROUT.Replace("％", "")
        CHAROUT = CHAROUT.Replace("∥", "")

        'タブ,キャリッジリターン文字とラインフィード文字,キャリッジリターン文字,ラインフィード文字,改行文字,バックスペース文字
        CHAROUT = CHAROUT.Replace(ControlChars.Tab, "")
        CHAROUT = CHAROUT.Replace(ControlChars.CrLf, "")
        CHAROUT = CHAROUT.Replace(ControlChars.Cr, "")
        CHAROUT = CHAROUT.Replace(ControlChars.Lf, "")
        CHAROUT = CHAROUT.Replace(ControlChars.NewLine, "")
        CHAROUT = CHAROUT.Replace(ControlChars.Back, "")

    End Sub

End Structure

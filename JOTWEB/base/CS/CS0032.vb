Imports System.IO

''' <summary>
''' TableData(Grid)復元　…　性能対策
''' </summary>
''' <remarks></remarks>
Public Structure CS0032TABLERecover
    ''' <summary>
    ''' 復元モード
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum RECOVERY_MODE As Integer
        DATA_ONLY
        WITH_HEADER
        HEAD_ONLY
    End Enum
    ''' <summary>
    ''' 取得データ格納ディレクトリ
    ''' </summary>
    ''' <value>ディレクトリ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FILEDIR() As String
    ''' <summary>
    ''' 格納対象テーブルデータ
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TBLDATA() As System.Data.DataTable
    ''' <summary>
    ''' 復元モード
    ''' </summary>
    ''' <value>復元モード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RECOVERMODE() As RECOVERY_MODE
    ''' <summary>
    ''' 格納後のテーブルデータ
    ''' </summary>
    ''' <value>テーブルデータ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OUTTBL() As System.Data.DataTable
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:0,ERR:5001(Customize),ERR:5002(Customize/Program)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0032TABLERecover"
    ''' <summary>
    ''' 各画面の検索結果情報再取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0032TABLERecover()
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

        '●In PARAMチェック
        '　書込先：c:\appl\applfiles\XML_TMP\yyyyMMdd-Userid-MAPID-MAPvariant-HHmmss.txt
        'PARAM01: FILEDIR
        If IsNothing(FILEDIR) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FILEDIR"                        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        ElseIf System.IO.File.Exists(FILEDIR) = False Then

            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FILEDIR"                        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: TBLDATA
        If (RECOVERMODE = RECOVERY_MODE.DATA_ONLY) Then
            If IsNothing(TBLDATA) Then
                ERR = C_MESSAGE_NO.DLL_IF_ERROR

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "TBLDATA"                        '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                 '
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Exit Sub
            End If
        End If

        '●TableData(Grid)退避

        '○初期処理
        OUTTBL = New DataTable
        OUTTBL.Clear()
        If (RECOVERMODE = RECOVERY_MODE.HEAD_ONLY Or RECOVERMODE = RECOVERY_MODE.WITH_HEADER) Then
            Dim FILEHEADER As String = FILEDIR.Substring(0, FILEDIR.LastIndexOf(".")) & "_head.txt"
            Dim hfs As New System.IO.StreamReader(FILEHEADER, System.Text.Encoding.UTF8)
            Dim linedata As String = hfs.ReadLine
            Do Until linedata = Nothing

                Dim WW_item() As String = linedata.Split(ControlChars.Tab)

                'データ格納行データ準備
                OUTTBL.Columns.Add(WW_item(0), Type.GetType(WW_item(1)))
                linedata = hfs.ReadLine
            Loop

            hfs.Close()
            hfs.Dispose()
            hfs = Nothing
        Else
            For Each Column As DataColumn In TBLDATA.Columns
                OUTTBL.Columns.Add(Column.ColumnName, Column.DataType)
            Next

        End If
        'ヘッダーのみコピーの場合データは処理しない
        If RECOVERMODE = RECOVERY_MODE.HEAD_ONLY Then
            ERR = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        '○退避処理

        Try
            Dim WW_LineData As String
            Dim WW_Row As DataRow
            Using fs As New System.IO.StreamReader(FILEDIR, System.Text.Encoding.UTF8)
                Dim sr = New StringReader(fs.ReadToEnd())
                Do
                    WW_LineData = sr.ReadLine()
                    If WW_LineData = Nothing Then
                        Exit Do
                    End If

                    Dim WW_item() As String = WW_LineData.Split(ControlChars.Tab)

                    'データ格納行データ準備
                    WW_Row = OUTTBL.NewRow

                    For i As Integer = 0 To OUTTBL.Columns.Count - 1
                        WW_Row.Item(i) = WW_item(i)
                    Next

                    OUTTBL.Rows.Add(WW_Row)

                Loop

                fs.Close()
            End Using

        Catch ex As Exception
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "I_Table ADD"                      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Structure

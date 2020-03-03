Option Strict On
''' <summary>
''' TableData(Grid)退避　…　性能対策
''' </summary>
''' <remarks></remarks>
Public Structure CS0031TABLEsave
    ''' <summary>
    ''' 保存モード
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum SAVING_MODE As Integer
        DATA_ONLY
        WITH_HEADER
    End Enum
    ''' <summary>
    ''' データ格納ディレクトリ
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
    ''' ヘッダー情報保存可否
    ''' </summary>
    ''' <value>ヘッダー情報保存可否</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SAVEMODE() As SAVING_MODE
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
    Public Const METHOD_NAME As String = "CS0031TABLEsave"
    ''' <summary>
    ''' 各画面の検索結果情報保存
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0031TABLEsave()
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get

        '<< エラー説明 >>
        'O_ERR = OK:0,ERR:5001(Customize),ERR:5002(Customize/Program)

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
        End If

        'PARAM02: TBLDATA
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

        Dim WW_TBLview As DataView = New DataView(TBLDATA)
        WW_TBLview.Sort = "LINECNT"

        '●TableData(Grid)退避

        Dim FILEHEADER As String = FILEDIR.Substring(0, FILEDIR.LastIndexOf(".")) & "_head.txt"
        If System.IO.File.Exists(FILEHEADER) Then
            System.IO.File.Delete(FILEHEADER)
        End If
        '○初期処理
        '前回処理・同一ファイルを削除
        If System.IO.File.Exists(FILEDIR) Then
            System.IO.File.Delete(FILEDIR)
        End If

        '○退避処理
        If SAVEMODE = SAVING_MODE.WITH_HEADER Then
            '書込ファイル（追加書き込み）を開く
            Using saveHFs As New IO.FileStream(FILEHEADER, IO.FileMode.Create, IO.FileAccess.Write),
                  SaveHF As New System.IO.StreamWriter(saveHFs, System.Text.Encoding.UTF8)
                Dim SAVEHstr As New System.Text.StringBuilder()
                'ヘッダー部分を保存する
                For Each Column As DataColumn In TBLDATA.Columns
                    SAVEHstr.Append(Column.ColumnName)
                    SAVEHstr.Append(ControlChars.Tab)
                    SAVEHstr.Append(Column.DataType)
                    SAVEHstr.Append(ControlChars.NewLine)
                Next
                'ファイル書き込み()
                Try
                    SaveHF.Write(SAVEHstr)
                Catch ex As System.SystemException
                    ERR = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Text File Write"                  '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                    CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT & "(" & ex.ToString & ")"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                End Try
                '閉じる
                SaveHF.Close()
                SaveHF.Dispose()
                SAVEHstr.Clear()
                SAVEHstr = Nothing
                saveHFs.Close()
            End Using
        End If

        '書込ファイル（追加書き込み）を開く
        Using saveFs As New IO.FileStream(FILEDIR, IO.FileMode.Create, IO.FileAccess.Write),
              SaveF As New System.IO.StreamWriter(saveFs, System.Text.Encoding.UTF8)

            Dim SAVEstr As New System.Text.StringBuilder
            Dim wITEMarray() As Object
            SaveF.AutoFlush = False
            '行ループ
            For i As Integer = 0 To WW_TBLview.Count - 1
                wITEMarray = WW_TBLview.Item(i).Row.ItemArray

                '列データをタブ区切りに変換
                For j As Integer = 0 To wITEMarray.Length - 1
                    'データ追加
                    If j = 0 Then
                        SAVEstr.Append(ControlChars.Quote & wITEMarray(j).ToString.Replace(ControlChars.Quote, ControlChars.Quote & ControlChars.Quote) & ControlChars.Quote)
                    Else
                        SAVEstr.Append(ControlChars.Tab)
                        SAVEstr.Append(ControlChars.Quote & wITEMarray(j).ToString.Replace(ControlChars.Quote, ControlChars.Quote & ControlChars.Quote) & ControlChars.Quote)
                    End If

                Next j
                SAVEstr.Append(ControlChars.NewLine)
                'ファイル書き込み()
                Try
                    SaveF.Write(SAVEstr)
                    SaveF.Flush()
                Catch ex As System.SystemException
                    ERR = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME              'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Text File Write"                  '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                   '
                    CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT & "(" & ex.ToString & ")"
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End Try
                SAVEstr.Clear()

            Next i

            '閉じる
            SaveF.Close()
            saveFs.Close()
        End Using

        ERR = C_MESSAGE_NO.NORMAL

        WW_TBLview.Dispose()
        WW_TBLview = Nothing

    End Sub

End Structure

Option Strict On
''' <summary>
''' 更新ジャーナル出力
''' </summary>
''' <remarks></remarks>
Public Structure CS0020JOURNAL

    ''' <summary>
    ''' テーブル名
    ''' </summary>
    ''' <value>テーブル名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TABLENM() As String

    ''' <summary>
    ''' アクション
    ''' </summary>
    ''' <value>アクション</value>
    ''' <returns></returns>
    ''' <remarks>UPDATE、INSERT、DELETE</remarks>
    Public Property ACTION() As String

    ''' <summary>
    ''' テーブル内容（変更後）
    ''' </summary>
    ''' <value>テーブル内容</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROW() As DataRow

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
    Public Const METHOD_NAME As String = "CS0020JOURNAL"

    ''' <summary>
    ''' 更新ジャーナル出力
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0020JOURNAL()

        '●In PARAMチェック

        'PARAM01: TABLENM(テーブル名)
        If IsNothing(TABLENM) And TABLENM = "" Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02: ACTION
        If IsNothing(ACTION) And ACTION = "" Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM04: ROW
        If IsNothing(ROW) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'セッション制御宣言
        Dim sm As New CS0050SESSION
        '●更新ジャーナル出力
        Try
            '更新ジャーナル出力パス作成
            '(会社＋日付（時分秒）＋マスタ(I_TABLENM)＋UPDATE(I_ACTION)＋部署)
            Dim W_JNLDIR As String = _
                sm.JORNAL_PATH & "\" & _
                sm.TERM_COMPANY & "-" & _
                DateTime.Now.ToString("yyyyMMddHHmmss") & _
                DateTime.Now.Millisecond & "_" & _
                TABLENM & "(" & _
                ACTION & ")_" & _
                sm.TERMID & ".txt"
            Using JNL As New System.IO.StreamWriter(W_JNLDIR, True, System.Text.Encoding.UTF8)
                'ROWデータのCSV(tab)変換
                Dim CSVstr As New StringBuilder
                For Each value As Object In ROW.ItemArray
                    If CSVstr.Length > 0 Then
                        CSVstr.Append(ControlChars.Tab)
                    End If
                    CSVstr.Append(Convert.ToString(value))
                Next
                '改行
                CSVstr.Append(ControlChars.NewLine)
                'ＥＲＲＬｏｇ出力
                Dim ERRTEXT As String
                ERRTEXT = "DATETIME = " & DateTime.Now.ToString("yyyyMMddHHmmss") & " , "
                ERRTEXT = ERRTEXT & "Camp = " & sm.TERM_COMPANY & " , "
                ERRTEXT = ERRTEXT & "Userid = " & sm.USERID & " , "
                ERRTEXT = ERRTEXT & "Namespace = " & sm.NAMESPACE_VALUE & " , "
                ERRTEXT = ERRTEXT & "Class = " & sm.CLASS_NAME & " , "
                ERRTEXT = ERRTEXT & "Tablenm = " & TABLENM & " , "
                ERRTEXT = ERRTEXT & "Action = " & ACTION & " , "
                ERRTEXT = ERRTEXT & "Term = " & sm.TERMID & " , "
                ERRTEXT = ERRTEXT & "TEXT = " & CSVstr.ToString
                JNL.Write(ERRTEXT)

                '閉じる
                JNL.Close()
            End Using

            ERR = C_MESSAGE_NO.NORMAL

            '全体
        Catch ex As System.SystemException
            'エラー処理でのエラーは未対応
            ERR = C_MESSAGE_NO.FILE_IO_ERROR
            Exit Sub

        End Try

    End Sub

End Structure

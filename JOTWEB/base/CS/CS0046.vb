Imports System.Data.SqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' ツリービュー（組織）表示
''' </summary>
''' <remarks>未使用</remarks>
Public Structure CS0046TREEget
    ''' <summary>
    ''' Object(組織、勘定科目)
    ''' </summary>
    ''' <value>OBJECT</value>
    ''' <returns>OBJECT</returns>
    ''' <remarks></remarks>
    Public Property OBJ() As String
    ''' <summary>
    ''' 構造コード
    ''' </summary>
    ''' <value>構造コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STRUCT() As String
    ''' <summary>
    ''' TreeVeiw Object
    ''' </summary>
    ''' <value>TreeVeiw Object</value>
    ''' <returns>TreeVeiw Object</returns>
    ''' <remarks></remarks>
    Public Property TreeObj() As Object
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns>会社コード</returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 開始年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STYMD() As String
    ''' <summary>
    ''' 終了年月日
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENDYMD() As String
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 構造体/関数名
    ''' </summary>
    ''' <remarks></remarks>
    Public Const METHOD_NAME As String = "CS0046TREEget"
    ''' <summary>
    ''' ツリービュー（組織）表示
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0046TREEget()
        Dim CS0009MESSAGEout As New CS0009MESSAGEout        'Message out
        Dim CS0011LOGWrite As New CS0011LOGWrite            'ログ格納ディレクトリ取得
        Dim CS0013UPROFview As New CS0013ProfView           'ユーザプロファイル（GridView）設定
        Dim CS0026TBLSORTget As New CS0026TBLSORT           'GridView用テーブルソート文字列取得

        ERR = C_MESSAGE_NO.NORMAL

        '●In PARAMチェック

        'PARAM01:OBJ(必須)
        If IsNothing(OBJ) Then
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWrite.INFPOSI = "OBJ"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM02:STRUCT(必須)
        If IsNothing(STRUCT) Then
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWrite.INFPOSI = "STRUCT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If

        'PARAM04: CAMPCODE
        If IsNothing(CAMPCODE) Then
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME           'SUBクラス名
            CS0011LOGWrite.INFPOSI = "CAMPCODE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Exit Sub
        End If
        'セッション制御宣言
        Dim sm As New CS0050SESSION

        If IsNothing(STYMD) Then
            STYMD = sm.SELECTED_START_DATE
        End If

        If IsNothing(ENDYMD) Then
            ENDYMD = sm.SELECTED_END_DATE
        End If

        '****************************
        '*** Ｗｏｒｋ             ***
        '****************************
        '検索結果格納ds
        Dim M00006tbl As DataTable                                  'Grid格納用テーブル
        Dim M00006row As DataRow                                    '行のロウデータ

        Dim WW_RootND As TreeNode  'ルートノード
        Dim WW_ParND As TreeNode   '親ノード
        Dim WW_ND As TreeNode      'ノード
        Dim WW_DATE As Date

        '■ 初期処理
        'M00006テンポラリDB項目作成
        M00006tbl = New DataTable

        'オブジェクト内容検索
        Try
            'DataBase接続文字
            Dim SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String = _
                 "SELECT isnull(rtrim(M6.USERID),'')            as USERID ,     " _
               & "       isnull(rtrim(M6.CAMPCODE),'')          as CAMPCODE ,   " _
               & "       isnull(rtrim(M6.OBJECT),'')            as OBJECT ,     " _
               & "       isnull(rtrim(M6.STRUCT),'')            as STRUCT ,     " _
               & "       isnull(rtrim(M6.SEQ),'')               as SEQ ,        " _
               & "       isnull(rtrim(M6.CODE),'')              as CODE ,       " _
               & "       isnull(rtrim(M6.GRCODE01),'')          as GRCODE01 ,   " _
               & "       isnull(rtrim(M6.GRCODE02),'')          as GRCODE02 ,   " _
               & "       isnull(rtrim(M6.GRCODE03),'')          as GRCODE03 ,   " _
               & "       isnull(rtrim(M6.GRCODE04),'')          as GRCODE04 ,   " _
               & "       isnull(rtrim(M6.GRCODE05),'')          as GRCODE05 ,   " _
               & "       isnull(rtrim(M6.GRCODE06),'')          as GRCODE06 ,   " _
               & "       isnull(rtrim(M6.GRCODE07),'')          as GRCODE07 ,   " _
               & "       isnull(rtrim(M6.GRCODE08),'')          as GRCODE08 ,   " _
               & "       isnull(rtrim(M6.GRCODE09),'')          as GRCODE09 ,   " _
               & "       isnull(rtrim(M6.GRCODE10),'')          as GRCODE10 ,   " _
               & "       M6.STYMD                               as STYMD,       " _
               & "       M6.ENDYMD                              as ENDYMD       " _
               & " FROM    M0006_STRUCT M6                                      " _
               & " Where   M6.USERID     = 'Default'                            " _
               & "   and   M6.CAMPCODE   = @P1                                  " _
               & "   and   M6.OBJECT     = @P2                                  " _
               & "   and   M6.STRUCT     = @P3                                  " _
               & "   and   M6.ENDYMD    >= @P4                                  " _
               & "   and   M6.STYMD     <= @P5                                  " _
               & "   and   M6.DELFLG    <> '1'                                  " _
               & " ORDER BY M6.SEQ                                              "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 50)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            PARA1.Value = CAMPCODE
            PARA2.Value = OBJ
            PARA3.Value = STRUCT
            PARA4.Value = STYMD
            PARA5.Value = ENDYMD
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            '■テーブル検索結果をテーブル退避
            'M00006テンポラリDB項目作成
            If M00006tbl.Columns.Count = 0 Then
            Else
                M00006tbl.Columns.Clear()
            End If

            M00006tbl.Clear()
            M00006tbl.Columns.Add("USERID", GetType(String))
            M00006tbl.Columns.Add("CAMPCODE", GetType(String))
            M00006tbl.Columns.Add("OBJECT", GetType(String))
            M00006tbl.Columns.Add("STRUCT", GetType(String))
            M00006tbl.Columns.Add("SEQ", GetType(String))
            M00006tbl.Columns.Add("CODE", GetType(String))
            M00006tbl.Columns.Add("GRCODE01", GetType(String))
            M00006tbl.Columns.Add("GRCODE02", GetType(String))
            M00006tbl.Columns.Add("GRCODE03", GetType(String))
            M00006tbl.Columns.Add("GRCODE04", GetType(String))
            M00006tbl.Columns.Add("GRCODE05", GetType(String))
            M00006tbl.Columns.Add("GRCODE06", GetType(String))
            M00006tbl.Columns.Add("GRCODE07", GetType(String))
            M00006tbl.Columns.Add("GRCODE08", GetType(String))
            M00006tbl.Columns.Add("GRCODE09", GetType(String))
            M00006tbl.Columns.Add("GRCODE10", GetType(String))
            M00006tbl.Columns.Add("STYMD", GetType(String))
            M00006tbl.Columns.Add("ENDYMD", GetType(String))

            'M00006tbl値設定
            While SQLdr.Read

                '○テーブル初期化
                M00006row = M00006tbl.NewRow()

                '○データ設定

                'Table設定項目
                M00006row("USERID") = SQLdr("USERID")
                M00006row("CAMPCODE") = SQLdr("CAMPCODE")
                M00006row("OBJECT") = SQLdr("OBJECT")
                M00006row("STRUCT") = SQLdr("STRUCT")
                M00006row("SEQ") = SQLdr("SEQ")
                M00006row("CODE") = SQLdr("CODE")
                M00006row("GRCODE01") = SQLdr("GRCODE01")
                M00006row("GRCODE02") = SQLdr("GRCODE02")
                M00006row("GRCODE03") = SQLdr("GRCODE03")
                M00006row("GRCODE04") = SQLdr("GRCODE04")
                M00006row("GRCODE05") = SQLdr("GRCODE05")
                M00006row("GRCODE06") = SQLdr("GRCODE06")
                M00006row("GRCODE07") = SQLdr("GRCODE07")
                M00006row("GRCODE08") = SQLdr("GRCODE08")
                M00006row("GRCODE09") = SQLdr("GRCODE09")
                M00006row("GRCODE10") = SQLdr("GRCODE10")
                If IsDBNull(SQLdr("STYMD")) Then
                    M00006row("STYMD") = ""
                Else
                    WW_DATE = SQLdr("STYMD")
                    M00006row("STYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If
                If IsDBNull(SQLdr("ENDYMD")) Then
                    M00006row("ENDYMD") = ""
                Else
                    WW_DATE = SQLdr("ENDYMD")
                    M00006row("ENDYMD") = WW_DATE.ToString("yyyy/MM/dd")
                End If

                M00006tbl.Rows.Add(M00006row)

            End While

            'Close
            SQLdr.Close() 'Reader(Close)
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = METHOD_NAME                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:M0006_STRUCT Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '■ TreeView設定処理
        TreeObj.Nodes.Clear()

        '○Rootノード設定
        WW_RootND = New TreeNode()
        Select Case OBJ
            Case "ORG"
                WW_RootND.Text = "全社"
                WW_RootND.Value = "全社"
            Case "AC"
                WW_RootND.Text = "全勘定科目"
                WW_RootND.Value = "全勘定科目"
            Case Else
                WW_RootND.Text = "全体"
                WW_RootND.Value = "全体"
        End Select
        TreeObj.Nodes.Add(WW_RootND)
        WW_RootND.Select()
        WW_ParND = WW_RootND

        '○従属ノード設定
        For Each TBLrow As DataRow In M00006tbl.Rows

            '** 初期設定(Rootノード)
            WW_ParND = WW_RootND
            WW_ParND.Select()

            '** 上位からのノード追加（ルート）
            If TBLrow("GRCODE01") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE01")
                WW_ND.Value = TBLrow("GRCODE01")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE01") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE02") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE02")
                WW_ND.Value = TBLrow("GRCODE02")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE02") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE03") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE03")
                WW_ND.Value = TBLrow("GRCODE03")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE03") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE04") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE04")
                WW_ND.Value = TBLrow("GRCODE04")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE04") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE05") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE05")
                WW_ND.Value = TBLrow("GRCODE05")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE05") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE06") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE06")
                WW_ND.Value = TBLrow("GRCODE06")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE06") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE07") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE07")
                WW_ND.Value = TBLrow("GRCODE07")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE07") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE08") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE08")
                WW_ND.Value = TBLrow("GRCODE08")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE08") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE09") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE09")
                WW_ND.Value = TBLrow("GRCODE09")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE09") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If

            If TBLrow("GRCODE10") <> "" Then
                WW_ND = New TreeNode
                WW_ND.Text = TBLrow("GRCODE10")
                WW_ND.Value = TBLrow("GRCODE10")

                If WW_ParND.ChildNodes.Count = 0 Then
                    WW_ParND.ChildNodes.Add(WW_ND)
                    WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                    WW_ParND.Select()
                Else
                    For j As Integer = 0 To WW_ParND.ChildNodes.Count - 1
                        If WW_ParND.ChildNodes.Item(j).Text = TBLrow("GRCODE10") Then
                            WW_ParND = WW_ParND.ChildNodes.Item(j)
                            WW_ParND.Select()
                            Exit For
                        End If
                        If (j >= (WW_ParND.ChildNodes.Count - 1)) Then
                            WW_ParND.ChildNodes.Add(WW_ND)
                            WW_ParND = WW_ParND.ChildNodes.Item(WW_ParND.ChildNodes.Count - 1)
                            WW_ParND.Select()
                        End If
                    Next
                End If
            End If



        Next

        '■ Clean up
        'ワークテーブル解放
        M00006tbl.Dispose()
        M00006tbl = Nothing

    End Sub

End Structure


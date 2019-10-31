Imports System.Data.SqlClient

''' <summary>
''' ユーザプロファイル（表示画面情報ソート文字列）
''' </summary>
''' <remarks></remarks>
Public Class CS0026TBLSORT
    ''' <summary>
    ''' 会社コード 
    ''' </summary>
    Public Property COMPCODE As String
    ''' <summary>
    ''' プロフィールID 
    ''' </summary>
    Public Property PROFID As String
    ''' <summary>
    ''' MAPID
    ''' </summary>
    Public Property MAPID As String
    ''' <summary>
    ''' VIEW　VARIANT
    ''' </summary>
    Public Property VARI As String
    ''' <summary>
    ''' TABID
    ''' </summary>
    Public Property TAB As String
    ''' <summary>
    ''' エラーメッセージ
    ''' </summary>
    Public Property ERR As String
    ''' <summary>
    ''' 情報元テーブル
    ''' </summary>
    Public Property TABLE As DataTable
    ''' <summary>
    ''' フィルター
    ''' </summary>
    Public Property FILTER As String
    ''' <summary>
    ''' ソート順 
    ''' </summary>
    Public Property SORTING As String

    'セッション制御宣言
    Protected sm As New CS0050SESSION


    ''' <summary>
    ''' 並び替えを行う
    ''' </summary>
    ''' <param name="O_TBL">並び替え結果</param>
    ''' <remarks></remarks>
    Public Sub Sort(ByRef O_TBL As DataTable)

        O_TBL = Sort()
    End Sub
    ''' <summary>
    ''' 並び替えを行う
    ''' </summary>
    ''' <returns>並び替え結果</returns>
    ''' <remarks></remarks>
    Public Function Sort() As DataTable

        '●In PARAMチェック
        'PARAM01: TABLE
        If IsNothing(TABLE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TABLE"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End If
        'PARAM02: SORTING
        If IsNothing(SORTING) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "SORTING"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End If
        Using WW_VIEW As DataView = New DataView(TABLE)
            WW_VIEW.Sort = SORTING
            If Not String.IsNullOrWhiteSpace(FILTER) Then
                WW_VIEW.RowFilter = FILTER
            End If
            Return WW_VIEW.ToTable()
        End Using
    End Function

    ''' <summary>
    ''' フィルタの条件に応じて分割する
    ''' </summary>
    ''' <param name="O_TBL1">フィルタの条件に当てはまるデータ群</param>
    ''' <param name="O_TBL2">フィルタの条件に当てはまらないデータ群</param>
    ''' <remarks></remarks>
    Public Sub SplitTable(ByRef O_TBL1 As DataTable, ByRef O_TBL2 As DataTable)
        '指定日データを分離
        Dim flst As New List(Of DataRow)
        Dim flsths As New List(Of Integer)
        Using WW_VIEW As New DataView(TABLE)
            O_TBL1 = If(O_TBL1, New DataTable)
            WW_VIEW.RowFilter = FILTER
            For int As Integer = 0 To WW_VIEW.Count - 1
                flst.Add(WW_VIEW(int).Row)
                flsths.Add(WW_VIEW(int).Row.GetHashCode)
            Next
            O_TBL1 = WW_VIEW.ToTable()
        End Using

        For Each row As DataRow In TABLE.Rows
            If Not (flst.Contains(row)) Then
                If IsNothing(O_TBL2) Then
                    O_TBL2 = New DataTable
                    For Each colum As DataColumn In TABLE.Columns
                        O_TBL2.Columns.Add(colum.ColumnName, colum.DataType)
                    Next
                End If
                Dim inrow = O_TBL2.NewRow()
                inrow.ItemArray = row.ItemArray
                O_TBL2.Rows.Add(inrow)
            End If
        Next

    End Sub
    ''' <summary>
    ''' ソートと採番を行う
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SortandNumbring()
        '〇ソート条件取得
        GetSorting()
        '〇ソートしないデータのバックアップ
        Dim tbl1 As DataTable = Nothing
        Dim tbl2 As DataTable = Nothing
        If Not String.IsNullOrEmpty(FILTER) Then
            SplitTable(tbl1, tbl2)
            TABLE = tbl1
        End If
        '〇ソート処理
        Dim WW_tbl As DataTable = Sort()
        Dim cnt As Integer = 1
        '〇採番する
        For Each row As DataRow In WW_tbl.Rows
            row("LINECNT") = cnt
            cnt = cnt + 1
        Next
        '〇最後に統合する
        If Not IsNothing(tbl2) Then
            WW_tbl.Merge(tbl2)
        End If
        TABLE = WW_tbl
    End Sub

    ''' <summary>
    ''' ソート条件の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GetSorting()
        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)

        '●In PARAMチェック
        'PARAM01: COMPCODE
        If IsNothing(COMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "COMPCODE"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        'PARAM01: PROFID
        If IsNothing(PROFID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "PROFID"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If
        'PARAM01: MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM02: VARI
        If IsNothing(VARI) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0026TBLSORTget"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "VARIANT"                           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "システム管理者へ連絡して下さい(In PARAM Err)"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        'PARAM03: I_TAB

        '●初期処理
        ERR = C_MESSAGE_NO.DLL_IF_ERROR
        SORTING = ""

        '●ユーザプロファイル（Gridソート文字列）取得
        'ユーザプロファイル（ビュー）… 個別設定値を検索
        Try
            'DataBase接続文字
            Dim SQLcon As SqlConnection = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
                 " SELECT FIELD , EFFECT         " _
               & "       , SORTKBN    as SORTKBN " _
               & " FROM  COM.OIS0012_PROFMVIEW         " _
               & " WHERE                         " _
               & "       CAMPCODE = @P1          " _
               & "   and PROFID   = @P2          " _
               & "   and MAPID    = @P3          " _
               & "   and VARIANT  = @P4          " _
               & "   and HDKBN    = 'H'          " _
               & "   and TABID    = @P5          " _
               & "   and TITLEKBN = 'I'          " _
               & "   and STYMD   <= @P6          " _
               & "   and ENDYMD  >= @P6          " _
               & "   and SORTORDER     > 0       " _
               & "   and DELFLG  <> '1'          " _
               & " ORDER BY SORTORDER ASC        "
            '(説明 OIS0012_PROFMVIEW)　…　画面一覧データの特定方法
            '   HD区分=H(ヘッダー)の場合、TABフィールドは有効(複数TAB対応)。※左右位置は無効フィールド。
            '   HD区分=D(ディテール)の場合、TABフィールド(複数TAB対応)および左右位置は有効。

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PROFID As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim P_MAPID As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim P_VARIANT As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 50)
            Dim P_TABID As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar, 50)
            Dim P_DATE As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.Date)
            P_CAMPCODE.Value = COMPCODE
            P_MAPID.Value = MAPID
            P_VARIANT.Value = VARI
            P_TABID.Value = TAB
            P_DATE.Value = Date.Now

            'セッション変数のPROFIDでデータを取得し、取得できない場合は'Default'で検索
            For Each key As String In {PROFID, C_DEFAULT_DATAKEY}
                P_PROFID.Value = key '動的パラメータに値を設定
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    If SQLdr.HasRows = True Then
                        'GridViewの列項目作成
                        While SQLdr.Read
                            If SQLdr("EFFECT") = "Y" Then
                                If SORTING = "" Then
                                    SORTING = SQLdr("FIELD") & " " & SQLdr("SORTKBN")
                                Else
                                    SORTING = SORTING & " , " & SQLdr("FIELD") & " " & SQLdr("SORTKBN")
                                End If
                            End If

                        End While
                        ERR = C_MESSAGE_NO.NORMAL

                        Exit For
                    End If
                End Using
            Next
            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = "getSorting"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0010_UPROFVIEW Select"        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try


    End Sub

End Class

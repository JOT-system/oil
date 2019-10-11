Imports System.Data.SqlClient

''' <summary>
''' ■車両台帳更新
''' </summary>
''' <remarks></remarks>
Public Class GRMA002UPDATE

    '統計DB出力dll Interface
    ''' <summary>
    ''' DB接続情報
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    ''' <summary>
    ''' 日報情報テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property T0005tbl As DataTable                                     '日報テーブル
    ''' <summary>
    ''' 更新ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDUSERID As String                                       '更新ユーザID
    ''' <summary>
    ''' 更新端末ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDTERMID As String                                       '更新端末ID
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String                                             'リターン値
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                        'LogOutput DirString Get
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                          'テーブルソート
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                          'セッション管理

    ''' <summary>
    ''' MA002更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Update()
        Try
            ERR = C_MESSAGE_NO.NORMAL

            '更新SQL文･･･マスタへ更新
            Dim WW_T0005UPDtbl As New DataTable
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

            '----------------------------------------------------------------------------------------------------
            'ＤＢ更新（車両台帳）
            '----------------------------------------------------------------------------------------------------
            '正乗務員の帰庫レコードより終了メータを取得し、車両台帳を更新する（台帳の累計走行距離　＜　終了メータのとき更新）
            CS0026TblSort.TABLE = T0005tbl
            CS0026TblSort.FILTER = "OPERATION='" & C_LIST_OPERATION_CODE.UPDATING & "' and SELECT = '1' and CREWKBN = '1' and WORKKBN='F3' and DELFLG = '0'"
            CS0026TblSort.SORTING = "DELFLG DESC, SELECT, CAMPCODE, SHIPORG, TERMKBN, CREWKBN, YMD, STAFFCODE, WORKKBN"
            WW_T0005UPDtbl = CS0026TblSort.sort()

            For Each WW_updRow As DataRow In WW_T0005UPDtbl.Rows
                '車両台帳の更新処理
                UpdateTableToMA002(WW_updRow, WW_RTN)
                If Not isNormal(WW_RTN) Then
                    'SQLtrn.Rollback()
                    ERR = WW_RTN
                    Exit Sub
                End If
            Next

        Catch ex As Exception
            'SQLtrn.Rollback()
            CS0011LOGWRITE.INFSUBCLASS = "MA002UPDATE"                  'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "例外発生"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' MA002更新処理
    ''' </summary>
    ''' <param name="I_ROW">更新対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub UpdateTableToMA002(
                            ByVal I_ROW As DataRow,
                            ByRef O_RTN As String)

        Dim WW_DATENOW As DateTime = Date.Now

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE MA002_SHARYOA " _
                        & "SET MANGTTLDIST = @P06 " _
                        & "  , UPDYMD      = @P07 " _
                        & "  , UPDUSER     = @P08 " _
                        & "  , UPDTERMID   = @P09 " _
                        & "  , RECEIVEYMD  = @P10  " _
                        & "WHERE CAMPCODE    =  @P01 " _
                        & "  AND SHARYOTYPE  =  @P02 " _
                        & "  AND TSHABAN     =  @P03 " _
                        & "  AND MANGTTLDIST <  @P06 " _
                        & "  AND STYMD       <= @P04 " _
                        & "  AND ENDYMD      >= @P05 " _
                        & "  AND DELFLG      <> '1'  "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Int)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.DateTime)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)

                PARA01.Value = I_ROW("CAMPCODE")
                PARA02.Value = I_ROW("SHARYOTYPEF")
                PARA03.Value = I_ROW("TSHABANF")
                PARA04.Value = WW_DATENOW
                PARA05.Value = WW_DATENOW
                PARA06.Value = CInt(I_ROW("ENDMATER"))
                PARA07.Value = WW_DATENOW
                PARA08.Value = UPDUSERID
                PARA09.Value = UPDTERMID
                PARA10.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                'CLOSE
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MA002_Update"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:UPDATE MA002_SHARYOA"          '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub

End Class

''' <summary>
''' 車両稼働状況更新
''' </summary>
''' <remarks></remarks>
Public Class GRTA001UPDATE

    '統計DB出力dll Interface
    ''' <summary>
    ''' DB接続情報
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    ''' <summary>
    ''' 日報情報テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property T0005tbl As DataTable                                     '車両管理状況テーブル
    ''' <summary>
    ''' 更新ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDUSERID As String                                       '更新ユーザID
    ''' <summary>
    ''' 更新端末ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDTERMID As String                                       '更新端末ID
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String                                             'リターン値
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                        'LogOutput DirString Get
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                          'テーブルソート
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                          'セッション管理

    ''' <summary>
    ''' 車両稼働状況テーブル 更新
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Update()

        Try
            ERR = C_MESSAGE_NO.NORMAL

            '更新SQL文･･･マスタへ更新
            Dim WW_TA001UPDtbl As New DataTable
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

            '----------------------------------------------------------------------------------------------------
            'ＤＢ更新（車両稼働状況）
            '----------------------------------------------------------------------------------------------------
            '正乗務員の帰庫レコードより終了メータを取得し、車両台帳を更新する（台帳の累計走行距離　＜　終了メータのとき更新）

            CS0026TBLSORT.TABLE = T0005tbl
            CS0026TBLSORT.FILTER = "OPERATION='更新' and SELECT = '1' and CREWKBN = '1' and WORKKBN='F3' and DELFLG = '0'"
            CS0026TblSort.SORTING = "DELFLG DESC, SELECT, CAMPCODE, SHIPORG, TERMKBN, CREWKBN, YMD, STAFFCODE, WORKKBN"
            WW_TA001UPDtbl = CS0026TBLSORT.sort()

            For Each WW_updRow As DataRow In WW_TA001UPDtbl.Rows
                If String.IsNullOrEmpty(WW_updRow("SHARYOTYPEF")) OrElse String.IsNullOrEmpty(WW_updRow("TSHABANF")) Then Continue For

                '車両台帳の更新処理
                UpdateTableToTA001(WW_updRow, WW_RTN)
                If Not isNormal(WW_RTN) Then
                    'SQLtrn.Rollback()
                    ERR = WW_RTN
                    Exit Sub
                End If
            Next
        Catch ex As Exception
            'SQLtrn.Rollback()
            CS0011LOGWRITE.INFSUBCLASS = "CS0047MA002UPDATE"            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "例外発生"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' TA001更新処理
    ''' </summary>
    ''' <param name="I_ROW">更新対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub UpdateTableToTA001(
                            ByVal I_ROW As DataRow,
                            ByRef O_RTN As String)

        Dim WW_DATENOW As DateTime = Date.Now

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '日報ＤＢ更新
            Dim SQLStr As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu  " _
                        & "     FROM TA001_SHARYOSTAT " _
                        & " WHERE CAMPCODE     = @P01 " _
                        & "  and SHARYOTYPE    = @P02 " _
                        & "  and TSHABAN       = @P03 " _
                        & "  and MANGUORG      = @P04 " _
                        & "  and KADOYMD       = @P05 " _
                        & "  and DELFLG       <> '1' ; " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE TA001_SHARYOSTAT " _
                        & "    SET DISTANCE    = @P06 " _
                        & "      , UPDYMD      = @P09 " _
                        & "      , UPDUSER     = @P10 " _
                        & "      , UPDTERMID   = @P11 " _
                        & "      , RECEIVEYMD  = @P12  " _
                        & " WHERE CAMPCODE     = @P01 " _
                        & "   and SHARYOTYPE   = @P02 " _
                        & "   and TSHABAN      = @P03 " _
                        & "   and MANGUORG     = @P04 " _
                        & "   and KADOYMD      = @P05 " _
                        & "   and DELFLG      <> '1' ; " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO TA001_SHARYOSTAT " _
                        & "             (CAMPCODE , " _
                        & "              SHARYOTYPE , " _
                        & "              TSHABAN , " _
                        & "              MANGUORG , " _
                        & "              KADOYMD , " _
                        & "              DISTANCE , " _
                        & "              DELFLG , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER ,  " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD ) " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10, " _
                        & "              @P11,@P12); " _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Decimal)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.DateTime)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.DateTime)

                PARA01.Value = I_ROW("CAMPCODE")
                PARA02.Value = I_ROW("SHARYOTYPEF")
                PARA03.Value = I_ROW("TSHABANF")
                PARA04.Value = I_ROW("SHIPORG")
                PARA05.Value = I_ROW("YMD")
                PARA06.Value = I_ROW("SOUDISTANCE")
                PARA07.Value = C_DELETE_FLG.ALIVE
                PARA08.Value = WW_DATENOW
                PARA09.Value = WW_DATENOW
                PARA10.Value = UPDUSERID
                PARA11.Value = UPDTERMID
                PARA12.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                'CLOSE
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "TA001_Update"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:UPDATE TA001_SHARYOSTAT"       '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub

End Class

''' <summary>
''' 乗務員マスタ（配送受注用）更新
''' </summary>
''' <remarks></remarks>
Public Class GRMB003UPDATE

    '統計DB出力dll Interface
    ''' <summary>
    ''' DB接続情報
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    ''' <summary>
    ''' 作業部署コード
    ''' </summary>
    ''' <returns></returns>
    Public Property SORG As String                                            '作業部署
    ''' <summary>
    ''' 日報情報テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property T0005tbl As DataTable                                     '日報テーブル
    ''' <summary>
    ''' 更新ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDUSERID As String                                       '更新ユーザID
    ''' <summary>
    ''' 更新端末ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDTERMID As String                                       '更新端末ID
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String                                             'リターン値
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                        'LogOutput DirString Get
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                          'テーブルソート
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                          'セッション管理
    ''' <summary>
    '''  社員マスタテーブル
    ''' </summary>
    Private MB001tbl As DataTable                                       '社員マスタテーブル
    ''' <summary>
    ''' 更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Update()

        Try
            ERR = C_MESSAGE_NO.NORMAL

            '更新SQL文･･･マスタへ更新
            Dim WW_T0005WKtbl As New DataTable
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

            '----------------------------------------------------------------------------------------------------
            '乗務員マスタ（配送受注用）更新
            '----------------------------------------------------------------------------------------------------
            Dim MB003tbl As DataTable = New DataTable
            MB003tbl.Columns.Add("CAMPCODE", GetType(String))
            MB003tbl.Columns.Add("STAFFCODE", GetType(String))
            MB003tbl.Columns.Add("YMD", GetType(Date))
            MB003tbl.Columns.Add("HOLIDAYKBN", GetType(String))
            MB003tbl.Columns.Add("ORVERTIME", GetType(String))
            MB003tbl.Columns.Add("DISTANCE", GetType(Decimal))
            MB003tbl.Columns.Add("UNLOADCNT", GetType(Integer))
            MB003tbl.Columns.Add("DANGCNT", GetType(Integer))
            MB003tbl.Columns.Add("DELFLG", GetType(String))
            MB003tbl.Columns.Add("INITYMD", GetType(Date))
            MB003tbl.Columns.Add("UPDYMD", GetType(DateTime))
            MB003tbl.Columns.Add("UPDUSER", GetType(String))
            MB003tbl.Columns.Add("UPDTERMID", GetType(String))
            MB003tbl.Columns.Add("RECEIVEYMD", GetType(DateTime))

            Dim WW_UNLOADCNT As Integer = 0
            Dim WW_BREAKTIME As Integer = 0
            Dim WW_WORKINGH As Integer = 0
            Dim WW_WORKTIME As Integer = 0
            Dim WW_TIME As String = String.Empty
            Dim WW_IDX As Integer = 0

            '更新対象のヘッダを抽出（上記、削除以外）
            CS0026TblSort.TABLE = T0005tbl
            CS0026TBLSORT.FILTER = String.Empty
            CS0026TBLSORT.SORTING = "SELECT, TERMKBN, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_T0005WKtbl = CS0026TblSort.sort()

            For i As Integer = 0 To WW_T0005WKtbl.Rows.Count - 1
                Dim WW_row_i As DataRow = WW_T0005WKtbl.Rows(i)
                Dim WW_updRow As DataRow = MB003tbl.NewRow

                If WW_row_i("HDKBN") = "H" AndAlso WW_row_i("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
                    '所定労働時間取得
                    getWorkingH(WW_row_i, SORG, WW_TIME, WW_RTN)
                    If Not isNormal(WW_RTN) Then
                        'SQLtrn.Rollback()
                        ERR = WW_RTN
                        Exit Sub
                    End If
                    WW_WORKINGH = TimeSpan.Parse(WW_TIME).TotalMinutes
                    '拘束時間（始業～終業＋１０分）
                    WW_TIME = DateAdd("n", 10, WW_row_i("ENDTIME")).ToString("HH:mm")
                    WW_WORKTIME = DateDiff("n", WW_row_i("STDATE") & " " & WW_row_i("STTIME"), WW_row_i("ENDDATE") & " " & WW_row_i("ENDTIME"))

                    WW_UNLOADCNT = 0
                    WW_BREAKTIME = 0

                    '明細行から、荷卸回数および、休憩時間を取得
                    For j As Integer = i + 1 To WW_T0005WKtbl.Rows.Count - 1
                        Dim WW_row_j As DataRow = WW_T0005WKtbl.Rows(j)
                        '次のヘッダまで
                        If WW_row_j("HDKBN") = "H" Then
                            i = j - 1
                            Exit For
                        ElseIf WW_row_j("HDKBN") = "D" AndAlso
                            WW_row_j("TERMKBN") = WW_row_i("TERMKBN") AndAlso
                            WW_row_j("YMD") = WW_row_i("YMD") AndAlso
                            WW_row_j("STAFFCODE") = WW_row_i("STAFFCODE") AndAlso
                            WW_row_j("DELFLG") = C_DELETE_FLG.ALIVE Then
                            If WW_row_j("WORKKBN") = "B3" Then
                                WW_UNLOADCNT += 1
                            End If

                            '休憩
                            If WW_row_j("WORKKBN") = "BB" Then
                                WW_BREAKTIME += TimeSpan.Parse(WW_row_j("WORKTIME")).TotalMinutes
                            End If
                        End If
                    Next

                    '拘束時間－所定労働時間－休憩時間＝残業時間
                    Dim WW_ORVERTIME As Integer = WW_WORKTIME - WW_WORKINGH - WW_BREAKTIME
                    If WW_ORVERTIME >= 1440 Then
                        WW_ORVERTIME = 1439
                    End If
                    If WW_ORVERTIME < 0 Then
                        WW_ORVERTIME = 0
                    End If
                    If WW_T0005WKtbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                        WW_ORVERTIME = 0
                    End If

                    '出力編集
                    WW_updRow("CAMPCODE") = WW_row_i("CAMPCODE")
                    WW_updRow("STAFFCODE") = WW_row_i("STAFFCODE")
                    WW_updRow("YMD") = WW_row_i("YMD")
                    WW_updRow("DISTANCE") = WW_row_i("SOUDISTANCE")
                    WW_updRow("ORVERTIME") = Format(Int(WW_ORVERTIME / 60) * 100 + WW_ORVERTIME Mod 60, "0#:##")
                    WW_updRow("UNLOADCNT") = WW_UNLOADCNT

                    '乗務員マスタ（配送受注用）更新処理
                    UpdateTableToMB003(WW_updRow, WW_RTN)
                    If Not isNormal(WW_RTN) Then
                        'SQLtrn.Rollback()
                        ERR = WW_RTN
                        Exit Sub
                    End If
                End If
            Next

        Catch ex As Exception
            'SQLtrn.Rollback()
            CS0011LOGWRITE.INFSUBCLASS = "CS0048MB003UPDATE"            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "例外発生"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 所定労働時間取得
    ''' </summary>
    ''' <param name="I_ROW">対象行</param>
    ''' <param name="I_SORG">作業部署コード</param>
    ''' <param name="O_WORKING_H">取得所定労働時間</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub GetWorkingH(ByVal I_ROW As DataRow,
                            ByVal I_SORG As String,
                            ByRef O_WORKING_H As String,
                            ByRef O_RTN As String)
        Dim WW_date As String = String.Empty
        Dim WW_RTN As String = C_MESSAGE_NO.NORMAL
        Using T0008COM As New GRT0008COM
            Try
                O_RTN = C_MESSAGE_NO.NORMAL
                'DataBase接続文字
                Dim SQLStr As String =
                           " SELECT   B.HORG            as HORG     , " _
                         & "          B.STAFFKBN        as STAFFKBN , " _
                         & "          rtrim(B.WORKINGH) as WORKINGH   " _
                         & " FROM       MB001_STAFF       A " _
                         & " INNER JOIN MB004_WORKINGH    B " _
                         & "       ON    B.CAMPCODE    = A.CAMPCODE " _
                         & "       and   B.STAFFKBN    = A.STAFFKBN " _
                         & "       and   B.STYMD      <= @P04 " _
                         & "       and   B.ENDYMD     >= @P03 " _
                         & "       and   B.DELFLG     <> '1' " _
                         & " WHERE   A.CAMPCODE    = @P01 " _
                         & "       and   A.STAFFCODE   = @P02 " _
                         & "       and   A.STYMD      <= @P04 " _
                         & "       and   A.ENDYMD     >= @P03 " _
                         & "       and   A.DELFLG     <> '1' "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon, SQLtrn)
                    Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                    Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)

                    PARA1.Value = I_ROW("CAMPCODE")
                    PARA2.Value = I_ROW("STAFFCODE")
                    PARA3.Value = I_ROW("YMD")
                    PARA4.Value = I_ROW("YMD")
                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        O_WORKING_H = "07:30"
                        While SQLdr.Read
                            '正社員
                            If Mid(SQLdr("STAFFKBN"), 3, 2) >= "10" AndAlso Mid(SQLdr("STAFFKBN"), 3, 2) < "30" Then
                                '所属部署が総務のレコードを取得
                                If T0008COM.IsGeneralAffair(I_ROW("CAMPCODE"), SQLdr("HORG"), WW_RTN) Then
                                    WW_date = SQLdr("WORKINGH")
                                    O_WORKING_H = CDate(WW_date).ToString("HH:mm")
                                End If
                            Else
                                '該当する所属部署のレコードを取得
                                If SQLdr("HORG") = I_SORG Then
                                    WW_date = SQLdr("WORKINGH")
                                    O_WORKING_H = CDate(WW_date).ToString("HH:mm")
                                End If
                            End If
                        End While

                        'Close
                    End Using

                End Using

            Catch ex As Exception
                CS0011LOGWRITE.INFSUBCLASS = "MB004_GetWorkingH"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "DB:MB004_WORKINGH Select"         '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                O_RTN = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' MB003更新処理
    ''' </summary>
    ''' <param name="I_ROW">対象行</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub UpdateTableToMB003(ByVal I_ROW As DataRow,
                             ByRef O_RTN As String)
        Dim WW_DATENOW As DateTime = Date.Now

        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '日報ＤＢ更新
            Dim SQLStr As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu  " _
                        & "     FROM MB003_HSTAFF " _
                        & "WHERE CAMPCODE  = @P01 " _
                        & "  and STAFFCODE = @P02 " _
                        & "  and YMD       = @P03 " _
                        & "  and DELFLG   <> '1' ; " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE MB003_HSTAFF " _
                        & "    SET ORVERTIME   = @P05 " _
                        & "      , DISTANCE    = @P06 " _
                        & "      , UNLOADCNT   = @P07 " _
                        & "      , DANGCNT     = @P08 " _
                        & "      , UPDYMD      = @P11 " _
                        & "      , UPDUSER     = @P12 " _
                        & "      , UPDTERMID   = @P13 " _
                        & "      , RECEIVEYMD  = @P14  " _
                        & "    WHERE CAMPCODE  = @P01 " _
                        & "      and STAFFCODE = @P02 " _
                        & "      and YMD       = @P03 " _
                        & "      and DELFLG   <> '1' ; " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO MB003_HSTAFF " _
                        & "             (CAMPCODE , " _
                        & "              STAFFCODE , " _
                        & "              YMD , " _
                        & "              HOLIDAYKBN , " _
                        & "              ORVERTIME , " _
                        & "              DISTANCE , " _
                        & "              UNLOADCNT , " _
                        & "              DANGCNT , " _
                        & "              DELFLG , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER ,  " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD ) " _
                        & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10, " _
                        & "              @P11,@P12,@P13,@P14); " _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Time)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Decimal)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.Int)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Int)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.SmallDateTime)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)
                PARA01.Value = I_ROW("CAMPCODE")
                PARA02.Value = I_ROW("STAFFCODE")
                PARA03.Value = I_ROW("YMD")
                PARA04.Value = String.Empty
                PARA05.Value = I_ROW("ORVERTIME")
                PARA06.Value = I_ROW("DISTANCE")
                PARA07.Value = I_ROW("UNLOADCNT")
                PARA08.Value = 0
                PARA09.Value = C_DELETE_FLG.ALIVE
                PARA10.Value = WW_DATENOW
                PARA11.Value = WW_DATENOW
                PARA12.Value = UPDUSERID
                PARA13.Value = UPDTERMID
                PARA14.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                'CLOSE
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB003_Update"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:UPDATE MB003_HSTAFF"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

End Class

''' <summary>
''' 届先マスタ更新
''' </summary>
''' <remarks></remarks>
Public Class GRMC006UPDATE
    '統計DB出力dll Interface
    ''' <summary>
    ''' DB接続情報
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property CAMPCODE As String                                        '会社コード
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <returns></returns>
    Public Property UORG As String                                            '部署コード
    ''' <summary>
    ''' 日報情報テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property T0005tbl As DataTable                                     '日報テーブル
    ''' <summary>
    ''' 更新ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDUSERID As String                                       '更新ユーザID
    ''' <summary>
    ''' 更新端末ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDTERMID As String                                       '更新端末ID
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String                                             'リターン値
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                        'LogOutput DirString Get
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                          'テーブルソート
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                          'セッション管理
    ''' <summary>
    ''' 届先マスタテーブル
    ''' </summary>
    Private MC006tbl As DataTable                                       '届先マスタテーブル
    ''' <summary>
    ''' 光英取込区分（JX、TG)
    ''' </summary>
    Private Const KOUEI_TYPE_JXTG As String = "jxtg"
    ''' <summary>
    ''' 光英取込区分（COSMO)
    ''' </summary>
    Private Const KOUEI_TYPE_COSMO As String = "cosmo"
    ''' <summary>
    '''  MC006tbl（届先マスタ）編集
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Update(ByVal I_CSVTYPE As String, ByVal I_LEGACY_MODE As Boolean, ByVal I_TBL As DataTable)

        Dim WW_RTN As String = String.Empty

        ERR = C_MESSAGE_NO.NORMAL

        '---------------------------------------------
        '実績（shasai.csv）からマスター登録：名称はJX,COSMO,TGの場合は光英マスタを参照して取得する。ない場合は空白
        '---------------------------------------------
        '出荷場所（荷主 "FIELD34"、出荷場所 "FIELD35"、緯度 FIELD52、経度 FIELD53）
        Dim B2query = (From x As DataRow In I_TBL.Rows
                       Where x("FIELD13") = "B2"
                       Order By x("FIELD4"), x("FIELD10"), x("FIELD6"), x("FIELD8")
                       Select FIELD4 = x("FIELD4"), FIELD10 = x("FIELD10"), FIELD6 = x("FIELD6"), FIELD8 = x("FIELD8"), FIELD52 = x("FIELD52"), FIELD53 = x("FIELD53")
                     ).Distinct
        For Each B2FILED In B2query

            Dim E2query = (From x As DataRow In I_TBL.Rows
                           Where x("FIELD13") = "E2" And B2FILED.FIELD4 = x("FIELD4") And B2FILED.FIELD10 = x("FIELD10") And B2FILED.FIELD6 = x("FIELD6") And B2FILED.FIELD8 < x("FIELD8")
                           Select FIELD34 = x("FIELD34"), FIELD35 = x("FIELD35")
                          ).Distinct

            For Each E2FILED In E2query
                Select Case I_CSVTYPE
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                        SetValueFromMC006(I_CSVTYPE, I_LEGACY_MODE, GRT00005WRKINC.C_TORICODE_JX, E2FILED.FIELD35, String.Empty, "2", If(IsDBNull(B2FILED.FIELD52), String.Empty, B2FILED.FIELD52), If(IsDBNull(B2FILED.FIELD53), String.Empty, B2FILED.FIELD53))
                    Case GRT00005WRKINC.TERM_TYPE.COSMO
                        SetValueFromMC006(I_CSVTYPE, I_LEGACY_MODE, GRT00005WRKINC.C_TORICODE_COSMO, E2FILED.FIELD35, String.Empty, "2", If(IsDBNull(B2FILED.FIELD52), String.Empty, B2FILED.FIELD52), If(IsDBNull(B2FILED.FIELD53), String.Empty, B2FILED.FIELD53))
                    Case Else
                        SetValueFromMC006(I_CSVTYPE, I_LEGACY_MODE, E2FILED.FIELD34, E2FILED.FIELD35, String.Empty, "2", B2FILED.FIELD52, B2FILED.FIELD53)
                End Select
                '届先マスタ（出荷場所）追加
                UpdateMC006Tbl(WW_RTN)
                If Not isNormal(WW_RTN) Then
                    'SQLtrn.Rollback()
                    ERR = WW_RTN
                    Exit Sub
                End If
            Next
        Next

        '届先（荷主 "FIELD34",、届先 "FIELD36"、緯度 FIELD52、経度 FIELD53）
        Dim B3query = (From x As DataRow In I_TBL.Rows
                       Where x("FIELD13") = "B3"
                       Order By x("FIELD4"), x("FIELD10"), x("FIELD6"), x("FIELD8")
                       Select FIELD4 = x("FIELD4"), FIELD10 = x("FIELD10"), FIELD6 = x("FIELD6"), FIELD8 = x("FIELD8"), FIELD52 = x("FIELD52"), FIELD53 = x("FIELD53")
                     ).Distinct
        For Each B3FILED In B3query
            Dim E2query = (From x As DataRow In I_TBL.Rows
                           Where x("FIELD13") = "E2" And B3FILED.FIELD4 = x("FIELD4") And B3FILED.FIELD10 = x("FIELD10") And B3FILED.FIELD6 = x("FIELD6") And B3FILED.FIELD8 < x("FIELD8")
                           Select FIELD34 = x("FIELD34"), FIELD36 = x("FIELD36"), FIELD37 = x("FIELD37")
                          ).Distinct

            For Each E2FILED In E2query

                Select Case I_CSVTYPE
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                        SetValueFromMC006(I_CSVTYPE, I_LEGACY_MODE, GRT00005WRKINC.C_TORICODE_JX, E2FILED.FIELD36, E2FILED.FIELD37, "1", If(IsDBNull(B3FILED.FIELD52), String.Empty, B3FILED.FIELD52), If(IsDBNull(B3FILED.FIELD53), String.Empty, B3FILED.FIELD53))
                    Case GRT00005WRKINC.TERM_TYPE.COSMO
                        SetValueFromMC006(I_CSVTYPE, I_LEGACY_MODE, GRT00005WRKINC.C_TORICODE_COSMO, E2FILED.FIELD36, E2FILED.FIELD37, "1", If(IsDBNull(B3FILED.FIELD52), String.Empty, B3FILED.FIELD52), If(IsDBNull(B3FILED.FIELD53), String.Empty, B3FILED.FIELD53))
                    Case Else
                        SetValueFromMC006(I_CSVTYPE, I_LEGACY_MODE, E2FILED.FIELD34, E2FILED.FIELD36, E2FILED.FIELD37, "1", B3FILED.FIELD52, B3FILED.FIELD53)
                End Select
                '届先マスタ（届先）追加
                UpdateMC006Tbl(WW_RTN)
                If Not isNormal(WW_RTN) Then
                    'SQLtrn.Rollback()
                    ERR = WW_RTN
                    Exit Sub
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' MC006の項目を設定する（情報がない場合は光英マスタから取得する）
    ''' </summary>
    ''' <param name="I_CSVTYPE">取込CSVタイプ</param>
    ''' <param name="I_LEGACY_MODE">レガシーフラグ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_TODOKECODE">届先コード</param>
    ''' <param name="I_CLASS">種別</param>
    ''' <param name="I_LATITUDE">経度</param>
    ''' <param name="I_LONGITUDE">緯度</param>
    ''' <remarks></remarks>
    Protected Sub SetValueFromMC006(ByVal I_CSVTYPE As String, ByVal I_LEGACY_MODE As Boolean, ByVal I_TORICODE As String, ByVal I_TODOKECODE As String, ByVal I_TODOKESEQ As String, ByVal I_CLASS As String, ByVal I_LATITUDE As String, ByVal I_LONGITUDE As String)

        Try
            '〇MC006tbl作成
            AddColumForMC006tbl(MC006tbl)
            Dim MC006Row As DataRow = MC006tbl.NewRow()
            MC006tbl.Rows.Add(MC006Row)
            '〇初期化
            InitialMC006tbl(MC006Row)
            '〇項目の設定
            MC006Row("CAMPCODE") = CAMPCODE
            MC006Row("UORG") = UORG
            MC006Row("TORICODE") = I_TORICODE

            If I_CLASS = "1" Then
                Select Case I_CSVTYPE
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                        MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE.PadLeft(9, "0")
                        If I_LEGACY_MODE Then MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE.PadLeft(6, "0") & I_TODOKESEQ.PadLeft(3, "0")
                    Case GRT00005WRKINC.TERM_TYPE.COSMO
                        MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & I_TODOKECODE.PadLeft(11, "0")
                    Case Else
                        MC006Row("TODOKECODE") = I_TODOKECODE
                End Select
            ElseIf I_CLASS = "2" Then
                Select Case I_CSVTYPE
                    Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                        MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.JX & I_TODOKECODE.PadLeft(4, "0")
                    Case GRT00005WRKINC.TERM_TYPE.COSMO
                        MC006Row("TODOKECODE") = C_ANOTHER_SYSTEMS_DISTINATION_PREFIX.COSMO & I_TODOKECODE.PadLeft(4, "0")
                    Case Else
                        MC006Row("TODOKECODE") = I_TODOKECODE
                End Select
            End If

            MC006Row("TODOKENAMES") = ""
            MC006Row("YTODOKECODE") = I_TODOKECODE
            MC006Row("LATITUDE") = I_LATITUDE
            MC006Row("LONGITUDE") = I_LONGITUDE
            MC006Row("CLASS") = I_CLASS
            'JX,COSMO,TGの場合にマスタを参照する
            If I_CSVTYPE = GRT00005WRKINC.TERM_TYPE.JX OrElse
               I_CSVTYPE = GRT00005WRKINC.TERM_TYPE.COSMO OrElse
               I_CSVTYPE = GRT00005WRKINC.TERM_TYPE.TG Then
                Dim SQLStr As String = " SELECT                         " _
                                     & "    TODOKESAKICODE           ,  " _
                                     & "    NAME                     ,  " _
                                     & "    LATITUDE                 ,  " _
                                     & "    LONGITUDE                   " _
                                     & " FROM                           " _
                                     & "    W0002_KOUEITODOKESAKI       " _
                                     & " WHERE                          " _
                                     & "       KOUEITYPE       = @P01   " _
                                     & "   AND TODOKESAKICODE  = @P02   " _
                                     & "                                "
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon, SQLtrn)
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)

                    PARA01.Value = ConvKoueiType(I_CSVTYPE)
                    If I_CLASS = "1" Then
                        Select Case I_CSVTYPE
                            Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                                PARA02.Value =
                                       If(I_LEGACY_MODE, I_TODOKECODE.PadLeft(6, "0"), I_TODOKECODE.PadLeft(9, "0")) &
                                       If(I_LEGACY_MODE, I_TODOKESEQ.PadLeft(3, "0"), "")
                            Case GRT00005WRKINC.TERM_TYPE.COSMO
                                PARA02.Value =
                                       I_TODOKECODE.PadLeft(11, "0")
                            Case Else
                                PARA02.Value = I_TODOKECODE
                        End Select
                    ElseIf I_CLASS = "2" Then
                        PARA02.Value = I_TODOKECODE.PadLeft(4, "0")
                    End If

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            MC006Row("TODOKENAMES") = SQLdr("NAME")
                            MC006Row("LATITUDE") = If(String.IsNullOrEmpty(MC006Row("LATITUDE")), SQLdr("LATITUDE"), MC006Row("LATITUDE"))
                            MC006Row("LONGITUDE") = If(String.IsNullOrEmpty(MC006Row("LONGITUDE")), SQLdr("LONGITUDE"), MC006Row("LONGITUDE"))
                        End If
                    End Using
                End Using
            End If

            Dim clsGeoCoder = New CS0055GeoCoder()
            Dim address As CS0055GeoCoder.AddressInfo = clsGeoCoder.GetAddress(MC006Row("LATITUDE"), MC006Row("LONGITUDE"))
            If IsNothing(address) Then
                '取得エラー時継続
            Else
                MC006Row("ADDR1") = address.Address1
                MC006Row("ADDR2") = address.Address2
                MC006Row("ADDR3") = address.Address3
                MC006Row("ADDR4") = address.Address4
                MC006Row("CITIES") = address.CityCode
            End If
            clsGeoCoder = Nothing

        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' 届先マスタ登録
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub UpdateMC006Tbl(ByRef O_RTN As String)

        Dim WW_DATENOW As DateTime = Date.Now
        Dim WW_ORDERNO As String = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        Try

            Dim SQLStr As String =
                       " DECLARE @hensuu as bigint ;                                        " _
                     & " set @hensuu = 0 ;                                                  " _
                     & " DECLARE hensuu CURSOR FOR                                          " _
                     & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                       " _
                     & "     FROM MC006_TODOKESAKI                                          " _
                     & "     WHERE    CAMPCODE      = @P01                                  " _
                     & "       and    rtrim(TODOKECODE)    = @P03 ;                         " _
                     & "                                                                    " _
                     & " OPEN hensuu ;                                                      " _
                     & " FETCH NEXT FROM hensuu INTO @hensuu ;                              " _
                     & " IF ( @@FETCH_STATUS = 0 )                                          " _
                     & "    UPDATE MC006_TODOKESAKI                                         " _
                     & "    SET LATITUDE     = @P17                                         " _
                     & "      , LONGITUDE    = @P18                                         " _
                     & "      , ADDR1        = @P10                                         " _
                     & "      , ADDR2        = @P11                                         " _
                     & "      , ADDR3        = @P12                                         " _
                     & "      , ADDR4        = @P13                                         " _
                     & "      , CITIES       = @P19                                         " _
                     & "      , DELFLG       = @P34                                         " _
                     & "      , UPDYMD       = @P36                                         " _
                     & "      , UPDUSER      = @P37                                         " _
                     & "      , UPDTERMID    = @P38                                         " _
                     & "      , RECEIVEYMD   = @P39                                         " _
                     & "     WHERE    CAMPCODE             = @P01                           " _
                     & "       and    rtrim(TODOKECODE)    = @P03 ;                         " _
                     & "                                                                    " _
                     & " IF ( @@FETCH_STATUS <> 0 )                                         " _
                     & "    INSERT INTO MC006_TODOKESAKI                                    " _
                     & "             (CAMPCODE ,                                            " _
                     & "              TORICODE ,                                            " _
                     & "              TODOKECODE ,                                          " _
                     & "              NAMES ,                                               " _
                     & "              NAMEL ,                                               " _
                     & "              NAMESK ,                                              " _
                     & "              NAMELK ,                                              " _
                     & "              POSTNUM1 ,                                            " _
                     & "              POSTNUM2 ,                                            " _
                     & "              ADDR1 ,                                               " _
                     & "              ADDR2 ,                                               " _
                     & "              ADDR3 ,                                               " _
                     & "              ADDR4 ,                                               " _
                     & "              TEL ,                                                 " _
                     & "              FAX ,                                                 " _
                     & "              MAIL ,                                                " _
                     & "              LATITUDE ,                                            " _
                     & "              LONGITUDE ,                                           " _
                     & "              CITIES ,                                              " _
                     & "              MORG ,                                                " _
                     & "              NOTES1 ,                                              " _
                     & "              NOTES2 ,                                              " _
                     & "              NOTES3 ,                                              " _
                     & "              NOTES4 ,                                              " _
                     & "              NOTES5 ,                                              " _
                     & "              NOTES6 ,                                              " _
                     & "              NOTES7 ,                                              " _
                     & "              NOTES8 ,                                              " _
                     & "              NOTES9 ,                                              " _
                     & "              NOTES10 ,                                             " _
                     & "              CLASS ,                                               " _
                     & "              STYMD ,                                               " _
                     & "              ENDYMD ,                                              " _
                     & "              DELFLG ,                                              " _
                     & "              INITYMD ,                                             " _
                     & "              UPDYMD ,                                              " _
                     & "              UPDUSER ,                                             " _
                     & "              UPDTERMID ,                                           " _
                     & "              RECEIVEYMD )                                          " _
                     & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,    " _
                     & "              @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20,    " _
                     & "              @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30,    " _
                     & "              @P31,@P32,@P33,@P34,@P35,@P36,@P37,@P38,@P39);        " _
                     & " CLOSE hensuu ;                                                     " _
                     & " DEALLOCATE hensuu ;                                                "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 3)
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 4)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar, 13)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar, 13)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar, 50)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar, 70)
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.DateTime)
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.DateTime)
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.DateTime)
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)

                PARA01.Value = MC006tbl.Rows(0)("CAMPCODE")
                PARA02.Value = MC006tbl.Rows(0)("TORICODE")
                PARA03.Value = MC006tbl.Rows(0)("TODOKECODE")
                PARA04.Value = MC006tbl.Rows(0)("TODOKENAMES")
                PARA05.Value = MC006tbl.Rows(0)("TODOKENAMEL")
                PARA06.Value = MC006tbl.Rows(0)("NAMESK")
                PARA07.Value = MC006tbl.Rows(0)("NAMELK")
                PARA08.Value = MC006tbl.Rows(0)("POSTNUM1")
                PARA09.Value = MC006tbl.Rows(0)("POSTNUM2")
                PARA10.Value = MC006tbl.Rows(0)("ADDR1")
                PARA11.Value = MC006tbl.Rows(0)("ADDR2")
                PARA12.Value = MC006tbl.Rows(0)("ADDR3")
                PARA13.Value = MC006tbl.Rows(0)("ADDR4")
                PARA14.Value = MC006tbl.Rows(0)("TEL")
                PARA15.Value = MC006tbl.Rows(0)("FAX")
                PARA16.Value = MC006tbl.Rows(0)("MAIL")
                PARA17.Value = MC006tbl.Rows(0)("LATITUDE")
                PARA18.Value = MC006tbl.Rows(0)("LONGITUDE")
                PARA19.Value = MC006tbl.Rows(0)("CITIES")
                PARA20.Value = MC006tbl.Rows(0)("MORG")
                PARA21.Value = MC006tbl.Rows(0)("NOTES1")
                PARA22.Value = MC006tbl.Rows(0)("NOTES2")
                PARA23.Value = MC006tbl.Rows(0)("NOTES3")
                PARA24.Value = MC006tbl.Rows(0)("NOTES4")
                PARA25.Value = MC006tbl.Rows(0)("NOTES5")
                PARA26.Value = MC006tbl.Rows(0)("NOTES6")
                PARA27.Value = MC006tbl.Rows(0)("NOTES7")
                PARA28.Value = MC006tbl.Rows(0)("NOTES8")
                PARA29.Value = MC006tbl.Rows(0)("NOTES9")
                PARA30.Value = MC006tbl.Rows(0)("NOTES10")
                PARA31.Value = MC006tbl.Rows(0)("CLASS")
                PARA32.Value = MC006tbl.Rows(0)("STYMD")
                PARA33.Value = MC006tbl.Rows(0)("ENDYMD")
                PARA34.Value = MC006tbl.Rows(0)("DELFLG")
                PARA35.Value = WW_DATENOW
                PARA36.Value = WW_DATENOW
                PARA37.Value = MC006tbl.Rows(0)("UPDUSER")
                PARA38.Value = MC006tbl.Rows(0)("UPDTERMID")
                PARA39.Value = C_DEFAULT_YMD
                SQLcmd.ExecuteNonQuery()
            End Using

            Dim SQLStr2 As String =
                       " DECLARE @hensuu as bigint ;                                        " _
                     & " set @hensuu = 0 ;                                                  " _
                     & " DECLARE hensuu CURSOR FOR                                          " _
                     & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu                       " _
                     & "     FROM MC007_TODKORG                                             " _
                     & "     WHERE    CAMPCODE      = @P01                                  " _
                     & "       and    TODOKECODE    = @P03                                  " _
                     & "       and    UORG          = @P04 ;                                " _
                     & "                                                                    " _
                     & " OPEN hensuu ;                                                      " _
                     & " FETCH NEXT FROM hensuu INTO @hensuu ;                              " _
                     & " IF ( @@FETCH_STATUS <> 0 )                                         " _
                     & "    INSERT INTO MC007_TODKORG                                       " _
                     & "             (CAMPCODE ,                                            " _
                     & "              TORICODE ,                                            " _
                     & "              TODOKECODE ,                                          " _
                     & "              UORG ,                                                " _
                     & "              ARRIVTIME ,                                           " _
                     & "              DISTANCE ,                                            " _
                     & "              SEQ ,                                                 " _
                     & "              YTODOKECODE ,                                         " _
                     & "              DELFLG ,                                              " _
                     & "              INITYMD ,                                             " _
                     & "              UPDYMD ,                                              " _
                     & "              UPDUSER ,                                             " _
                     & "              UPDTERMID ,                                           " _
                     & "              RECEIVEYMD )                                          " _
                     & "      VALUES (@P01,@P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10,    " _
                     & "              @P11,@P12,@P13,@P14);                                 " _
                     & " CLOSE hensuu ;                                                     " _
                     & " DEALLOCATE hensuu ;                                                "

            Using SQLcmd2 As New SqlCommand(SQLStr2, SQLcon, SQLtrn)
                Dim PARA201 As SqlParameter = SQLcmd2.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA202 As SqlParameter = SQLcmd2.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA203 As SqlParameter = SQLcmd2.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA204 As SqlParameter = SQLcmd2.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA205 As SqlParameter = SQLcmd2.Parameters.Add("@P05", System.Data.SqlDbType.Time)
                Dim PARA206 As SqlParameter = SQLcmd2.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 3)
                Dim PARA207 As SqlParameter = SQLcmd2.Parameters.Add("@P07", System.Data.SqlDbType.Int)
                Dim PARA208 As SqlParameter = SQLcmd2.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar, 9)
                Dim PARA209 As SqlParameter = SQLcmd2.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar, 1)
                Dim PARA210 As SqlParameter = SQLcmd2.Parameters.Add("@P10", System.Data.SqlDbType.DateTime)
                Dim PARA211 As SqlParameter = SQLcmd2.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
                Dim PARA212 As SqlParameter = SQLcmd2.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA213 As SqlParameter = SQLcmd2.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA214 As SqlParameter = SQLcmd2.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

                PARA201.Value = MC006tbl.Rows(0)("CAMPCODE")
                PARA202.Value = MC006tbl.Rows(0)("TORICODE")
                PARA203.Value = MC006tbl.Rows(0)("TODOKECODE")
                PARA204.Value = MC006tbl.Rows(0)("UORG")
                PARA205.Value = MC006tbl.Rows(0)("ARRIVTIME")
                PARA206.Value = MC006tbl.Rows(0)("DISTANCE")
                PARA207.Value = MC006tbl.Rows(0)("SEQ")
                PARA208.Value = MC006tbl.Rows(0)("YTODOKECODE")
                PARA209.Value = MC006tbl.Rows(0)("DELFLG")
                PARA210.Value = WW_DATENOW
                PARA211.Value = WW_DATENOW
                PARA212.Value = MC006tbl.Rows(0)("UPDUSER")
                PARA213.Value = MC006tbl.Rows(0)("UPDTERMID")
                PARA214.Value = C_DEFAULT_YMD
                SQLcmd2.ExecuteNonQuery()

                'CLOSE
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MC006_Update"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSERT MC006_Update"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' 届先ローカルテーブル項目設定
    ''' </summary>
    ''' <param name="IO_TBL">ローカルテーブル</param>
    ''' <remarks></remarks>
    Public Sub AddColumForMC006tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable

        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

        'MC006DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("LINECNT", GetType(Integer))
        IO_TBL.Columns.Add("OPERATION", GetType(String))
        IO_TBL.Columns.Add("TIMSTP", GetType(String))
        IO_TBL.Columns.Add("SELECT", GetType(Integer))
        IO_TBL.Columns.Add("HIDDEN", GetType(Integer))

        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("CAMPNAMES", GetType(String))
        IO_TBL.Columns.Add("TORICODE", GetType(String))
        IO_TBL.Columns.Add("TORINAMES", GetType(String))
        IO_TBL.Columns.Add("TORINAMEL", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("TODOKENAMES", GetType(String))
        IO_TBL.Columns.Add("TODOKENAMEL", GetType(String))
        IO_TBL.Columns.Add("NAMESK", GetType(String))
        IO_TBL.Columns.Add("NAMELK", GetType(String))
        IO_TBL.Columns.Add("POSTNUM1", GetType(String))
        IO_TBL.Columns.Add("POSTNUM2", GetType(String))
        IO_TBL.Columns.Add("ADDR1", GetType(String))
        IO_TBL.Columns.Add("ADDR2", GetType(String))
        IO_TBL.Columns.Add("ADDR3", GetType(String))
        IO_TBL.Columns.Add("ADDR4", GetType(String))
        IO_TBL.Columns.Add("TEL", GetType(String))
        IO_TBL.Columns.Add("FAX", GetType(String))
        IO_TBL.Columns.Add("MAIL", GetType(String))
        IO_TBL.Columns.Add("LATITUDE", GetType(String))
        IO_TBL.Columns.Add("LONGITUDE", GetType(String))
        IO_TBL.Columns.Add("CITIES", GetType(String))
        IO_TBL.Columns.Add("MORG", GetType(String))
        IO_TBL.Columns.Add("NOTES1", GetType(String))
        IO_TBL.Columns.Add("NOTES2", GetType(String))
        IO_TBL.Columns.Add("NOTES3", GetType(String))
        IO_TBL.Columns.Add("NOTES4", GetType(String))
        IO_TBL.Columns.Add("NOTES5", GetType(String))
        IO_TBL.Columns.Add("NOTES6", GetType(String))
        IO_TBL.Columns.Add("NOTES7", GetType(String))
        IO_TBL.Columns.Add("NOTES8", GetType(String))
        IO_TBL.Columns.Add("NOTES9", GetType(String))
        IO_TBL.Columns.Add("NOTES10", GetType(String))
        IO_TBL.Columns.Add("CLASS", GetType(String))
        IO_TBL.Columns.Add("STYMD", GetType(String))
        IO_TBL.Columns.Add("ENDYMD", GetType(String))

        IO_TBL.Columns.Add("UORG", GetType(String))
        IO_TBL.Columns.Add("ARRIVTIME", GetType(String))
        IO_TBL.Columns.Add("DISTANCE", GetType(Integer))
        IO_TBL.Columns.Add("SEQ", GetType(Integer))
        IO_TBL.Columns.Add("YTODOKECODE", GetType(String))

        IO_TBL.Columns.Add("DELFLG", GetType(String))
        IO_TBL.Columns.Add("INITYMD", GetType(String))
        IO_TBL.Columns.Add("UPDYMD", GetType(String))
        IO_TBL.Columns.Add("UPDUSER", GetType(String))
        IO_TBL.Columns.Add("UPDTERMID", GetType(String))
        IO_TBL.Columns.Add("RECEIVEYMD", GetType(String))

    End Sub
    ''' <summary>
    ''' 届先ローカルテーブル項目初期化
    ''' </summary>
    ''' <param name="IO_ROW">ローカル行</param>
    ''' <remarks></remarks>
    Public Sub InitialMC006tbl(ByRef IO_ROW As DataRow)

        IO_ROW("LINECNT") = 0
        IO_ROW("OPERATION") = String.Empty
        IO_ROW("TIMSTP") = 0
        IO_ROW("SELECT") = 0
        IO_ROW("HIDDEN") = 0

        IO_ROW("CAMPCODE") = CAMPCODE
        IO_ROW("TORICODE") = String.Empty
        IO_ROW("TORINAMES") = String.Empty
        IO_ROW("TORINAMEL") = String.Empty
        IO_ROW("TODOKECODE") = String.Empty
        IO_ROW("TODOKENAMES") = String.Empty
        IO_ROW("TODOKENAMEL") = String.Empty
        IO_ROW("NAMESK") = String.Empty
        IO_ROW("NAMELK") = String.Empty
        IO_ROW("POSTNUM1") = String.Empty
        IO_ROW("POSTNUM2") = String.Empty
        IO_ROW("ADDR1") = String.Empty
        IO_ROW("ADDR2") = String.Empty
        IO_ROW("ADDR3") = String.Empty
        IO_ROW("ADDR4") = String.Empty
        IO_ROW("TEL") = String.Empty
        IO_ROW("FAX") = String.Empty
        IO_ROW("MAIL") = String.Empty
        IO_ROW("LATITUDE") = String.Empty
        IO_ROW("LONGITUDE") = String.Empty
        IO_ROW("CITIES") = String.Empty
        IO_ROW("MORG") = UORG
        IO_ROW("NOTES1") = String.Empty
        IO_ROW("NOTES2") = String.Empty
        IO_ROW("NOTES3") = String.Empty
        IO_ROW("NOTES4") = String.Empty
        IO_ROW("NOTES5") = String.Empty
        IO_ROW("NOTES6") = String.Empty
        IO_ROW("NOTES7") = String.Empty
        IO_ROW("NOTES8") = String.Empty
        IO_ROW("NOTES9") = String.Empty
        IO_ROW("NOTES10") = String.Empty
        IO_ROW("CLASS") = String.Empty
        IO_ROW("STYMD") = "2000/01/01"
        IO_ROW("ENDYMD") = C_MAX_YMD

        IO_ROW("UORG") = UORG
        IO_ROW("ARRIVTIME") = "00:00:00"
        IO_ROW("DISTANCE") = 0
        IO_ROW("SEQ") = 1000
        IO_ROW("YTODOKECODE") = String.Empty

        IO_ROW("DELFLG") = C_DELETE_FLG.ALIVE
        IO_ROW("INITYMD") = String.Empty
        IO_ROW("UPDYMD") = String.Empty
        IO_ROW("UPDUSER") = UPDUSERID
        IO_ROW("UPDTERMID") = UPDTERMID
        IO_ROW("RECEIVEYMD") = String.Empty

    End Sub
    ''' <summary>
    ''' CSVのモードから光英のマスタタイプに変換する
    ''' </summary>
    ''' <param name="I_MODE">CSVモード</param>
    ''' <returns>マスタタイプ</returns>
    ''' <remarks></remarks>
    Protected Function ConvKoueiType(ByVal I_MODE) As String
        Select Case I_MODE
            Case GRT00005WRKINC.TERM_TYPE.JX, GRT00005WRKINC.TERM_TYPE.TG
                Return KOUEI_TYPE_JXTG
            Case GRT00005WRKINC.TERM_TYPE.COSMO
                Return KOUEI_TYPE_COSMO
            Case Else
                Return String.Empty
        End Select
    End Function
End Class

''' <summary>
''' '■配送受注、荷主受注ＤＢ更新
''' </summary>
''' <remarks></remarks>
Public Class GRT0004UPDATE
    '統計DB出力dll Interface
    ''' <summary>
    ''' DB接続情報
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    ''' <summary>
    ''' 日報情報テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property T0005tbl As DataTable                                     '日報テーブル
    ''' <summary>
    ''' 業務車番リストボックス
    ''' </summary>
    ''' <returns></returns>
    Public Property ListBoxGSHABAN As ListBox                                 '業務車番リストボックス
    ''' <summary>
    ''' 日報テーブル（帰値用）
    ''' </summary>
    ''' <returns></returns>
    Public Property RTNTbl As DataTable                                       '日報テーブル
    ''' <summary>
    ''' 更新ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDUSERID As String                                       '更新ユーザID
    ''' <summary>
    ''' 更新端末ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDTERMID As String                                       '更新端末ID
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String                                             'リターン値
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                              'LogOutput DirString Get
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                                'テーブルソート
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                                'セッション管理

    ''' <summary>
    ''' 更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Update()

        Try
            ERR = C_MESSAGE_NO.NORMAL

            '更新SQL文･･･マスタへ更新
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

            '----------------------------------------------------------------------------------------------------
            '受注予約、受注配送ＤＢ追加
            '----------------------------------------------------------------------------------------------------
            '荷卸レコードより受注DBを更新する
            Dim WW_T0003tbl As New DataTable
            Dim WW_T0004tbl As New DataTable
            Dim WW_T0004SUMtbl As New DataTable
            'T4更新用データ
            AddColumnT0004tbl(WW_T0004tbl)

            'T00004UPDtbl更新データ作成
            GetT0004UPDtbl(WW_T0004tbl, WW_RTN)
            If Not isNormal(WW_RTN) Then
                'SQLtrn.Rollback()
                ERR = WW_RTN
                Exit Sub
            End If

            ' ***  T00004SUMtbl更新データ作成
            getT0004SUMtbl(WW_T0004tbl, WW_T0004SUMtbl, WW_RTN)
            If Not isNormal(WW_RTN) Then
                'SQLtrn.Rollback()
                ERR = WW_RTN
                Exit Sub
            End If

            Dim WW_DATENOW As Date = Date.Now

            ' ***  T00003・T00004tbl関連データ削除
            DeleteT0003AndT0004(WW_T0004tbl, WW_DATENOW, WW_RTN)
            If Not isNormal(WW_RTN) Then
                'SQLtrn.Rollback()
                ERR = WW_RTN
                Exit Sub
            End If

            ' ***  T0004tbl追加
            InsertT0003OrT0004("T0004_HORDER", WW_T0004tbl, WW_DATENOW, WW_RTN)
            If Not isNormal(WW_RTN) Then
                'SQLtrn.Rollback()
                ERR = WW_RTN
                Exit Sub
            End If

            ' ***  T0003tbl追加
            InsertT0003OrT0004("T0003_NIORDER", WW_T0004SUMtbl, WW_DATENOW, WW_RTN)
            If Not isNormal(WW_RTN) Then
                'SQLtrn.Rollback()
                ERR = WW_RTN
                Exit Sub
            End If

            rtnTbl = T0005tbl.Copy

        Catch ex As Exception
            'SQLtrn.Rollback()
            CS0011LOGWRITE.INFSUBCLASS = "CS0049T0004UPDATE"            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "例外発生"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub
        End Try
    End Sub

    ' *** 
    ''' <summary>
    '''  T00004UPDtbl更新データ（画面表示受注+画面非表示受注）作成
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub GetT0004UPDtbl(ByRef IO_TBL As DataTable, ByRef O_RTN As String)

        '更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_SORTstr As String = String.Empty
        Dim WW_FILLstr As String = String.Empty

        '更新元データ用View
        Dim WW_TBLview As DataView

        Dim WW_TORICODE As String = String.Empty
        Dim WW_OILTYPE As String = String.Empty
        Dim WW_SHUKADATE As String = String.Empty
        Dim WW_KIJUNDATE As String = String.Empty
        Dim WW_ORDERORG As String = String.Empty
        Dim WW_SHIPORG As String = String.Empty

        Dim WW_SHUKODATE As String = String.Empty
        Dim WW_TODOKEDATE As String = String.Empty
        Dim WW_GSHABAN As String = String.Empty
        Dim WW_STAFFCODE As String = String.Empty
        Dim WW_TRIPNO As String = String.Empty
        Dim WW_DROPNO As String = String.Empty

        Dim WW_FILTER As String = String.Empty

        '■■■ 受注最新レコード(DB格納)をT0004UPDtblへ格納 ■■■
        '○作業用DBのカラム設定
        Dim WW_T0004tbl As DataTable = IO_TBL.Clone
        Dim WW_T0004WKtbl As DataTable = IO_TBL.Clone
        Dim WW_T0004UPDtbl As DataTable = IO_TBL.Clone
        Dim WW_T0005WKtbl As DataTable = T0005tbl.Clone

        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）
        WW_TBLview = New DataView(T0005tbl)
        WW_TBLview.Sort = "SELECT, TERMKBN, SHIPORG, TORICODE, YMD, OILTYPE1"
        WW_FILTER = String.Empty
        WW_FILTER = WW_FILTER & "OPERATION = '更新' and "
        WW_FILTER = WW_FILTER & "HDKBN     = 'D' and "
        WW_FILTER = WW_FILTER & "CREWKBN   = '1' and "
        WW_FILTER = WW_FILTER & "WORKKBN   = 'B3' "
        WW_TBLview.RowFilter = WW_FILTER

        WW_T0005WKtbl = WW_TBLview.ToTable

        '■■■ 受注番号　自動採番 ■■■

        '○　受注番号　自動採番
        For i As Integer = 0 To WW_T0005WKtbl.Rows.Count - 1
            Dim WW_T0005row As DataRow = WW_T0005WKtbl.Rows(i)

            If String.IsNullOrEmpty(WW_T0005row("ORDERNO")) AndAlso
               WW_T0005row("DELFLG") = C_DELETE_FLG.ALIVE Then
                '受注番号採番
                Dim CS0033 As New CS0033AutoNumber
                CS0033.CAMPCODE = WW_T0005row("CAMPCODE")
                CS0033.MORG = WW_T0005row("SHIPORG")
                CS0033.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.ORDERNO
                CS0033.USERID = Me.UPDUSERID
                CS0033.getAutoNumber()
                If isNormal(CS0033.ERR) Then
                    '他レコードへ反映
                    For Each WW_row_j As DataRow In WW_T0005WKtbl.Rows
                        If WW_T0005row("URIKBN") = "1" Then
                            '出荷基準の場合、出荷日
                            If WW_row_j("TORICODE") = WW_T0005row("TORICODE") AndAlso
                               WW_row_j("OILTYPE1") = WW_T0005row("OILTYPE1") AndAlso
                               WW_row_j("SHUKADATE") = WW_T0005row("SHUKADATE") AndAlso
                               WW_row_j("SHIPORG") = WW_T0005row("SHIPORG") Then

                                WW_row_j("ORDERNO") = CS0033.SEQ

                            End If
                        Else
                            '着地基準の場合、出荷日
                            If WW_row_j("TORICODE") = WW_T0005row("TORICODE") AndAlso
                               WW_row_j("OILTYPE1") = WW_T0005row("OILTYPE1") AndAlso
                               WW_row_j("TODOKEDATE") = WW_T0005row("TODOKEDATE") AndAlso
                               WW_row_j("SHIPORG") = WW_T0005row("SHIPORG") Then

                                WW_row_j("ORDERNO") = CS0033.SEQ

                            End If
                        End If
                    Next
                Else
                    O_RTN = CS0033.ERR
                    Exit Sub
                End If
            End If

        Next

        CS0026TblSort.TABLE = WW_T0005WKtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "SELECT, CAMPCODE, SHIPORG, YMD, STAFFCODE, STDATE, STTIME, SEQ, NIPPONO, WORKKBN, GSHABAN"
        WW_T0005WKtbl = CS0026TblSort.sort()

        CS0026TblSort.TABLE = T0005tbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "SELECT, CAMPCODE, SHIPORG, YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, SEQ, NIPPONO, WORKKBN, GSHABAN"
        T0005tbl = CS0026TblSort.sort()

        Dim WW_IDX As Integer = 0
        '元（T0005）へ反映
        For i As Integer = 0 To WW_T0005WKtbl.Rows.Count - 1
            Dim WW_T0005row As DataRow = WW_T0005WKtbl.Rows(i)
            For j As Integer = WW_IDX To T0005tbl.Rows.Count - 1
                Dim T0005row As DataRow = T0005tbl.Rows(j)
                If T0005row("CAMPCODE") = WW_T0005row("CAMPCODE") AndAlso
                    T0005row("SHIPORG") = WW_T0005row("SHIPORG") AndAlso
                    T0005row("YMD") = WW_T0005row("YMD") AndAlso
                    T0005row("STAFFCODE") = WW_T0005row("STAFFCODE") AndAlso
                    T0005row("NIPPONO") = WW_T0005row("NIPPONO") AndAlso
                    T0005row("WORKKBN") = WW_T0005row("WORKKBN") AndAlso
                    T0005row("GSHABAN") = WW_T0005row("GSHABAN") AndAlso
                    T0005row("STDATE") = WW_T0005row("STDATE") AndAlso
                    T0005row("STTIME") = WW_T0005row("STTIME") AndAlso
                    T0005row("SEQ") = WW_T0005row("SEQ") AndAlso
                    T0005row("SELECT") = "1" AndAlso
                    T0005row("DELFLG") = C_DELETE_FLG.ALIVE Then

                    T0005row("ORDERNO") = WW_T0005row("ORDERNO")
                    WW_IDX = j + 1
                    Exit For
                End If
            Next
        Next

        '副乗務員への反映（正乗務員と同じ受注番号）
        WW_IDX = 0
        For Each WW_T0005row As DataRow In WW_T0005WKtbl.Rows
            If IsDBNull(WW_T0005row("SUBSTAFFCODE")) = True Then
                WW_T0005row("SUBSTAFFCODE") = String.Empty
            End If
            If WW_T0005row("SUBSTAFFCODE") <> "" Then
                For j As Integer = WW_IDX To T0005tbl.Rows.Count - 1
                    Dim T0005row As DataRow = T0005tbl.Rows(j)
                    If T0005row("CAMPCODE") = WW_T0005row("CAMPCODE") AndAlso
                        T0005row("SHIPORG") = WW_T0005row("SHIPORG") AndAlso
                        T0005row("YMD") = WW_T0005row("YMD") AndAlso
                        T0005row("STAFFCODE") = WW_T0005row("SUBSTAFFCODE") AndAlso
                        T0005row("NIPPONO") = WW_T0005row("NIPPONO") AndAlso
                        T0005row("WORKKBN") = WW_T0005row("WORKKBN") AndAlso
                        T0005row("GSHABAN") = WW_T0005row("GSHABAN") AndAlso
                        T0005row("STDATE") = WW_T0005row("STDATE") AndAlso
                        T0005row("STTIME") = WW_T0005row("STTIME") AndAlso
                        T0005row("SEQ") = WW_T0005row("SEQ") AndAlso
                        T0005row("SELECT") = "1" AndAlso
                        T0005row("DELFLG") = C_DELETE_FLG.ALIVE Then

                        T0005row("ORDERNO") = WW_T0005row("ORDERNO")
                        WW_IDX = j + 1
                        Exit For
                    End If
                Next
            End If
        Next

        '日報より配送受注へフォーマット変換
        Dim WW_DATE As Date = Date.Now
        For Each WW_T0005Row As DataRow In WW_T0005WKtbl.Rows
            '８明細（品名）分処理する
            For i As Integer = 1 To 8
                Select Case i
                    Case 1
                        WW_T0005Row("SEQ") = "01"
                    Case 2
                        WW_T0005Row("SEQ") = "02"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE2")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT12")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT22")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE2")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO2")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI2")
                    Case 3
                        WW_T0005Row("SEQ") = "03"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE3")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT13")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT23")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE3")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO3")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI3")
                    Case 4
                        WW_T0005Row("SEQ") = "04"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE4")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT14")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT24")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE4")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO4")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI4")
                    Case 5
                        WW_T0005Row("SEQ") = "05"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE5")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT15")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT25")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE5")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO5")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI5")
                    Case 6
                        WW_T0005Row("SEQ") = "06"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE6")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT16")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT26")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE6")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO6")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI6")
                    Case 7
                        WW_T0005Row("SEQ") = "07"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE7")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT17")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT27")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE7")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO7")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI7")
                    Case 8
                        WW_T0005Row("SEQ") = "08"
                        WW_T0005Row("OILTYPE1") = WW_T0005Row("OILTYPE8")
                        WW_T0005Row("PRODUCT11") = WW_T0005Row("PRODUCT18")
                        WW_T0005Row("PRODUCT21") = WW_T0005Row("PRODUCT28")
                        WW_T0005Row("PRODUCTCODE1") = WW_T0005Row("PRODUCTCODE8")
                        WW_T0005Row("SURYO1") = WW_T0005Row("SURYO8")
                        WW_T0005Row("STANI1") = WW_T0005Row("STANI8")
                End Select

                If WW_T0005Row("OILTYPE1") = String.Empty Then
                    Continue For
                End If

                EditT0004(WW_T0005Row, WW_DATE, WW_T0004tbl)
            Next
        Next

        CS0026TblSort.TABLE = WW_T0004tbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,SHUKODATE ,SHIPORG ,GSHABAN ,STAFFCODE, TRIPNO ,DROPNO , SEQ"
        WW_T0004tbl = CS0026TblSort.sort()

        '○更新対象受注のDB格納レコードを全て取得
        '　　　　｜　　　６／１　　｜　　●６／２　　｜　　６／３　　　　参考（受注番号の採番結果）
        '－－－－＋－－－－－－－－＋－－－－－－－－＋－－－－－－－－　　出荷日ベース　　届日ベース
        '①取Ａ　｜出庫、出荷、届　｜　　　　　　　　｜　　　　　　　　　　受注番号１　　　受注番号１　
        '②取Ａ　｜　　　出庫、出荷｜出庫、届　　　　｜　　　　　　　　　　受注番号１　　　受注番号２
        '③取Ａ　｜　　　　　　　　｜出庫、出荷、届　｜　　　　　　　　　　受注番号２　　　受注番号２
        '④取Ａ　｜　　　　　　　　｜　　　出庫、出荷｜出庫、届　　　　　　受注番号２　　　受注番号３
        '⑤取Ａ　｜　　　　　　　　｜　　　　　　　　｜出庫、出荷、届　　　受注番号３　　　受注番号３

        '例）６／２出庫の実績をターゲットとする
        '②③を取得する

        For Each WW_T0004Row As DataRow In WW_T0004tbl.Rows

            If WW_T0004Row("TORICODE") = WW_TORICODE AndAlso
               WW_T0004Row("OILTYPE") = WW_OILTYPE AndAlso
               WW_T0004Row("SHUKODATE") = WW_SHUKODATE AndAlso
               WW_T0004Row("TODOKEDATE") = WW_TODOKEDATE AndAlso
               WW_T0004Row("SHIPORG") = WW_SHIPORG AndAlso
               WW_T0004Row("GSHABAN") = WW_GSHABAN AndAlso
               WW_T0004Row("STAFFCODE") = WW_STAFFCODE Then
            Else
                '配送受注の取得
                SelectT0004("1", WW_T0004Row, WW_T0004WKtbl, O_RTN)
                If Not isNormal(O_RTN) Then
                    Exit Sub
                End If
            End If

            WW_TORICODE = WW_T0004Row("TORICODE")
            WW_OILTYPE = WW_T0004Row("OILTYPE")
            WW_SHUKODATE = WW_T0004Row("SHUKODATE")
            WW_TODOKEDATE = WW_T0004Row("TODOKEDATE")
            WW_SHIPORG = WW_T0004Row("SHIPORG")
            WW_GSHABAN = WW_T0004Row("GSHABAN")
            WW_STAFFCODE = WW_T0004Row("STAFFCODE")
        Next

        CS0026TblSort.TABLE = WW_T0004WKtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG  ,GSHABAN ,TRIPNO ,DROPNO , SEQ"
        WW_T0004WKtbl = CS0026TblSort.sort()

        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty

        For Each WW_Row As DataRow In WW_T0004WKtbl.Rows

            If WW_Row("TORICODE") = WW_TORICODE AndAlso
               WW_Row("OILTYPE") = WW_OILTYPE AndAlso
               WW_Row("KIJUNDATE") = WW_KIJUNDATE AndAlso
               WW_Row("ORDERORG") = WW_ORDERORG AndAlso
               WW_Row("SHIPORG") = WW_SHIPORG Then
            Else
                '配送受注の取得
                SelectT0004("2", WW_Row, WW_T0004UPDtbl, O_RTN)
                If Not isNormal(O_RTN) Then
                    Exit Sub
                End If
            End If

            WW_TORICODE = WW_Row("TORICODE")
            WW_OILTYPE = WW_Row("OILTYPE")
            WW_KIJUNDATE = WW_Row("KIJUNDATE")
            WW_ORDERORG = WW_Row("ORDERORG")
            WW_SHIPORG = WW_Row("SHIPORG")
        Next

        '■■■ 実績数量更新 ■■■
        '削除データから順に処理する
        CS0026TblSort.TABLE = WW_T0004tbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "DELFLG DESC, TORICODE ,OILTYPE ,SHUKODATE ,SHIPORG ,GSHABAN ,STAFFCODE, TRIPNO ,DROPNO , SEQ"
        WW_T0004tbl = CS0026TblSort.sort()
        WW_T0004WKtbl.Clear()
        For Each WW_T0004row As DataRow In WW_T0004tbl.Rows

            Dim WW_UPD As String = "OFF"
            For j As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
                Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(j)
                '配送受注に存在したら更新
                If WW_T0004row("TORICODE") = WW_T0004UPDrow("TORICODE") AndAlso
                   WW_T0004row("OILTYPE") = WW_T0004UPDrow("OILTYPE") AndAlso
                   WW_T0004row("ORDERORG") = WW_T0004UPDrow("ORDERORG") AndAlso
                   WW_T0004row("SHIPORG") = WW_T0004UPDrow("SHIPORG") AndAlso
                   WW_T0004row("GSHABAN") = WW_T0004UPDrow("GSHABAN") AndAlso
                   WW_T0004row("TRIPNO") = WW_T0004UPDrow("TRIPNO") AndAlso
                   WW_T0004row("DROPNO") = WW_T0004UPDrow("DROPNO") AndAlso
                   WW_T0004row("TODOKEDATE") = WW_T0004UPDrow("TODOKEDATE") AndAlso
                   WW_T0004row("PRODUCT2") = WW_T0004UPDrow("PRODUCT2") Then
                    '配車済'2'、実績があれば、受注実績'3'
                    If WW_T0004row("DELFLG") = C_DELETE_FLG.ALIVE Then
                        WW_T0004UPDrow("STATUS") = "3"
                        WW_T0004UPDrow("STANI") = WW_T0004row("STANI")
                        WW_T0004UPDrow("JSURYO") = WW_T0004row("JSURYO")
                        WW_T0004UPDrow("JDAISU") = 1
                        WW_UPD = "ON"
                    Else
                        WW_T0004UPDrow("STATUS") = "2"
                        WW_T0004UPDrow("STANI") = String.Empty
                        WW_T0004UPDrow("JSURYO") = 0
                        WW_T0004UPDrow("JDAISU") = 0
                        WW_UPD = "ON"
                    End If
                    Exit For
                End If
            Next

            '配送受注に存在しなかった追加、データを保存（削除は捨てる）
            If WW_UPD = "OFF" AndAlso WW_T0004row("DELFLG") = C_DELETE_FLG.ALIVE Then
                Dim WW_Row As DataRow = WW_T0004WKtbl.NewRow
                WW_Row.ItemArray = WW_T0004row.ItemArray
                WW_T0004WKtbl.Rows.Add(WW_Row)
            End If
        Next

        '追加データをマージ
        WW_T0004UPDtbl.Merge(WW_T0004WKtbl)

        '関連受注の予定および実績数量がゼロの場合、削除
        For i As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
            Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(i)
            If WW_T0004UPDrow("SURYO") = 0 And WW_T0004UPDrow("JSURYO") = 0 Then
                WW_T0004UPDrow("DELFLG") = C_DELETE_FLG.DELETE
            End If
        Next

        CS0026TblSort.TABLE = WW_T0004UPDtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,SHUKODATE ,GSHABAN ,TRIPNO ,DROPNO , SEQ"
        WW_T0004UPDtbl = CS0026TblSort.sort()

        '■■■ T00004UPDtblのDetailNO、SEQを再付番 ■■■
        Dim WW_DETAILNO As Integer = 0
        Dim WW_SEQ As Integer = 0

        '○DetailNO再付番
        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty
        WW_SHUKODATE = String.Empty
        WW_GSHABAN = String.Empty
        WW_TRIPNO = String.Empty
        WW_DROPNO = String.Empty
        For i As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
            Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(i)

            If WW_T0004UPDrow("DELFLG") <> C_DELETE_FLG.DELETE Then
                If WW_TORICODE = WW_T0004UPDrow("TORICODE") And
                   WW_OILTYPE = WW_T0004UPDrow("OILTYPE") And
                   WW_KIJUNDATE = WW_T0004UPDrow("KIJUNDATE") And
                   WW_ORDERORG = WW_T0004UPDrow("ORDERORG") And
                   WW_SHIPORG = WW_T0004UPDrow("SHIPORG") Then
                    WW_DETAILNO = WW_DETAILNO + 1
                    WW_T0004UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")
                Else
                    WW_DETAILNO = 1
                    WW_T0004UPDrow("DETAILNO") = WW_DETAILNO.ToString("000")

                    WW_TORICODE = WW_T0004UPDrow("TORICODE")
                    WW_OILTYPE = WW_T0004UPDrow("OILTYPE")
                    WW_KIJUNDATE = WW_T0004UPDrow("KIJUNDATE")
                    WW_ORDERORG = WW_T0004UPDrow("ORDERORG")
                    WW_SHIPORG = WW_T0004UPDrow("SHIPORG")

                End If
            End If

        Next

        'DETAIL番号を反映
        CS0026TblSort.TABLE = WW_T0004UPDtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "CAMPCODE ,SHIPORG ,SHUKODATE ,STAFFCODE ,TODOKEDATE, TRIPNO,DROPNO , SEQ"
        WW_T0004UPDtbl = CS0026TblSort.sort()

        Dim T4_KEY As String = String.Empty
        Dim T5_KEY As String = String.Empty
        WW_IDX = 0
        For i As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
            Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(i)
            T4_KEY = WW_T0004UPDrow("CAMPCODE") & "_" & WW_T0004UPDrow("SHIPORG") & "_" & WW_T0004UPDrow("SHUKODATE") & "_" & WW_T0004UPDrow("STAFFCODE") & "_" & WW_T0004UPDrow("TODOKEDATE") & "_" & WW_T0004UPDrow("TRIPNO") & "_" & WW_T0004UPDrow("DROPNO")
            For j As Integer = WW_IDX To T0005tbl.Rows.Count - 1
                Dim T0005row As DataRow = T0005tbl.Rows(j)
                If T0005row("WORKKBN") <> "B3" Then
                    Continue For
                End If
                If T0005row("SELECT") = "0" OrElse
                   T0005row("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                T5_KEY = T0005row("CAMPCODE") & "_" & T0005row("SHIPORG") & "_" & T0005row("YMD") & "_" & T0005row("STAFFCODE") & "_" & T0005row("TODOKEDATE") & "_" & T0005row("TRIPNO") & "_" & T0005row("DROPNO")

                'Ｔ４キーが大きい場合、Ｔ５を進める
                If T5_KEY < T4_KEY Then
                    Continue For
                End If

                'Ｔ５キーが大きい場合、Ｔ４を進め、Ｔ５は現在から処理する
                If T5_KEY > T4_KEY Then
                    WW_IDX = j + 1
                    Exit For
                End If

                If T5_KEY = T4_KEY Then
                    T0005row("ORDERNO") = WW_T0004UPDrow("ORDERNO")
                    T0005row("DETAILNO") = WW_T0004UPDrow("DETAILNO")

                    '荷積に受注番号を設定
                    For k As Integer = j - 1 To 0 Step -1
                        Dim T0005row_k As DataRow = T0005tbl.Rows(k)
                        If T0005row_k("YMD") = T0005row("YMD") And
                           T0005row_k("STAFFCODE") = T0005row("STAFFCODE") And
                           T0005row_k("NIPPONO") = T0005row("NIPPONO") Then
                            If T0005row_k("WORKKBN") = "B2" Then
                                T0005row("ORDERNO") = WW_T0004UPDrow("ORDERNO")
                                T0005row("DETAILNO") = WW_T0004UPDrow("DETAILNO")
                                Exit For
                            End If
                        Else
                            Exit For
                        End If
                    Next
                    WW_IDX = j + 1
                    Exit For
                End If
            Next
        Next

        'DETAIL番号を反映
        CS0026TblSort.TABLE = WW_T0004UPDtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "CAMPCODE ,SHIPORG ,SHUKODATE ,SUBSTAFFCODE ,TODOKEDATE, TRIPNO,DROPNO , SEQ"
        WW_T0004UPDtbl = CS0026TblSort.sort()

        T4_KEY = String.Empty
        T5_KEY = String.Empty
        WW_IDX = 0
        For i As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
            Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(i)
            T4_KEY = WW_T0004UPDrow("CAMPCODE") & "_" & WW_T0004UPDrow("SHIPORG") & "_" & WW_T0004UPDrow("SHUKODATE") & "_" & WW_T0004UPDrow("SUBSTAFFCODE") & "_" & WW_T0004UPDrow("TODOKEDATE") & "_" & WW_T0004UPDrow("TRIPNO") & "_" & WW_T0004UPDrow("DROPNO")
            If IsDBNull(WW_T0004UPDrow("SUBSTAFFCODE")) = True Then
                WW_T0004UPDrow("SUBSTAFFCODE") = String.Empty
            End If
            If WW_T0004UPDrow("SUBSTAFFCODE") <> "" Then
                For j As Integer = WW_IDX To T0005tbl.Rows.Count - 1
                    Dim T0005row As DataRow = T0005tbl.Rows(j)
                    If T0005row("WORKKBN") <> "B3" Then
                        Continue For
                    End If
                    If T0005row("SELECT") = "0" OrElse
                       T0005row("DELFLG") = C_DELETE_FLG.DELETE Then
                        Continue For
                    End If

                    T5_KEY = T0005row("CAMPCODE") & "_" & T0005row("SHIPORG") & "_" & T0005row("YMD") & "_" & T0005row("STAFFCODE") & "_" & T0005row("TODOKEDATE") & "_" & T0005row("TRIPNO") & "_" & T0005row("DROPNO")

                    'Ｔ４キーが大きい場合、Ｔ５を進める
                    If T5_KEY < T4_KEY Then
                        Continue For
                    End If

                    'Ｔ５キーが大きい場合、Ｔ４を進め、Ｔ５は現在から処理する
                    If T5_KEY > T4_KEY Then
                        WW_IDX = j + 1
                        Exit For
                    End If

                    If T5_KEY = T4_KEY Then

                        T0005row("ORDERNO") = WW_T0004UPDrow("ORDERNO")
                        T0005row("DETAILNO") = WW_T0004UPDrow("DETAILNO")

                        For k As Integer = j - 1 To 0 Step -1
                            Dim T0005row_k As DataRow = T0005tbl.Rows(k)
                            If T0005row_k("YMD") = T0005row("YMD") And
                               T0005row_k("STAFFCODE") = T0005row("STAFFCODE") And
                               T0005row_k("NIPPONO") = T0005row("NIPPONO") Then
                                If T0005row_k("WORKKBN") = "B2" Then
                                    T0005row("ORDERNO") = WW_T0004UPDrow("ORDERNO")
                                    T0005row("DETAILNO") = WW_T0004UPDrow("DETAILNO")
                                    Exit For
                                End If
                            Else
                                Exit For
                            End If
                        Next
                        WW_IDX = j + 1
                        Exit For
                    End If
                Next
            End If
        Next

        '○台数設定
        CS0026TblSort.TABLE = WW_T0004UPDtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,SHUKODATE ,GSHABAN ,TRIPNO ,DROPNO , SEQ"
        WW_T0004UPDtbl = CS0026TblSort.sort()

        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty
        WW_SHUKODATE = String.Empty
        WW_GSHABAN = String.Empty
        WW_TRIPNO = String.Empty
        For i As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
            Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(i)

            If WW_T0004UPDrow("DELFLG") <> C_DELETE_FLG.DELETE Then
                If WW_TORICODE = WW_T0004UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = WW_T0004UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = WW_T0004UPDrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = WW_T0004UPDrow("ORDERORG") AndAlso
                   WW_SHIPORG = WW_T0004UPDrow("SHIPORG") AndAlso
                   WW_SHUKODATE = WW_T0004UPDrow("SHUKODATE") AndAlso
                   WW_GSHABAN = WW_T0004UPDrow("GSHABAN") AndAlso
                   WW_TRIPNO = WW_T0004UPDrow("TRIPNO") Then
                    WW_T0004UPDrow("DAISU") = 0
                    WW_T0004UPDrow("JDAISU") = 0
                Else
                    If WW_T0004UPDrow("JSURYO") = 0 Then
                        WW_T0004UPDrow("JDAISU") = 0
                    Else
                        WW_T0004UPDrow("JDAISU") = 1
                    End If
                End If
                WW_TORICODE = WW_T0004UPDrow("TORICODE")
                WW_OILTYPE = WW_T0004UPDrow("OILTYPE")
                WW_KIJUNDATE = WW_T0004UPDrow("KIJUNDATE")
                WW_ORDERORG = WW_T0004UPDrow("ORDERORG")
                WW_SHIPORG = WW_T0004UPDrow("SHIPORG")
                WW_SHUKODATE = WW_T0004UPDrow("SHUKODATE")
                WW_GSHABAN = WW_T0004UPDrow("GSHABAN")
                WW_TRIPNO = WW_T0004UPDrow("TRIPNO")

            End If

        Next

        '○SEQ再付番
        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty
        WW_SHUKODATE = String.Empty
        WW_GSHABAN = String.Empty
        WW_TRIPNO = String.Empty
        WW_DROPNO = String.Empty
        For i As Integer = 0 To WW_T0004UPDtbl.Rows.Count - 1
            Dim WW_T0004UPDrow As DataRow = WW_T0004UPDtbl.Rows(i)

            If WW_T0004UPDrow("DELFLG") <> C_DELETE_FLG.DELETE Then
                If WW_TORICODE = WW_T0004UPDrow("TORICODE") AndAlso
                   WW_OILTYPE = WW_T0004UPDrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = WW_T0004UPDrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = WW_T0004UPDrow("ORDERORG") AndAlso
                   WW_SHIPORG = WW_T0004UPDrow("SHIPORG") AndAlso
                   WW_SHUKODATE = WW_T0004UPDrow("SHUKODATE") AndAlso
                   WW_GSHABAN = WW_T0004UPDrow("GSHABAN") AndAlso
                   WW_TRIPNO = WW_T0004UPDrow("TRIPNO") AndAlso
                   WW_DROPNO = WW_T0004UPDrow("DROPNO") Then
                    WW_SEQ = WW_SEQ + 1
                    WW_T0004UPDrow("SEQ") = WW_SEQ.ToString("00")
                Else
                    WW_SEQ = 1
                    WW_T0004UPDrow("SEQ") = WW_SEQ.ToString("00")
                    WW_TORICODE = WW_T0004UPDrow("TORICODE")
                    WW_OILTYPE = WW_T0004UPDrow("OILTYPE")
                    WW_KIJUNDATE = WW_T0004UPDrow("KIJUNDATE")
                    WW_ORDERORG = WW_T0004UPDrow("ORDERORG")
                    WW_SHIPORG = WW_T0004UPDrow("SHIPORG")
                    WW_SHUKODATE = WW_T0004UPDrow("SHUKODATE")
                    WW_GSHABAN = WW_T0004UPDrow("GSHABAN")
                    WW_TRIPNO = WW_T0004UPDrow("TRIPNO")
                    WW_DROPNO = WW_T0004UPDrow("DROPNO")

                End If
            End If

        Next

        IO_TBL = WW_T0004UPDtbl.Copy

        '○close
        WW_TBLview.Dispose()
        WW_TBLview = Nothing
        WW_T0004WKtbl.Dispose()
        WW_T0004WKtbl = Nothing
        WW_T0004UPDtbl.Dispose()
        WW_T0004UPDtbl = Nothing
        WW_T0005WKtbl.Dispose()
        WW_T0005WKtbl = Nothing

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' T00004SUMtbl更新データ作成
    ''' </summary>
    ''' <param name="I_TBL"></param>
    ''' <param name="O_TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub GetT0004SUMtbl(ByVal I_TBL As DataTable, ByRef O_TBL As DataTable, ByRef O_RTN As String)
        Dim GS0029T3CNTLget As New GS0029T3CNTLget          '荷主受注集計制御マスタ取得

        '更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_SORTstr As String = String.Empty
        Dim WW_FILLstr As String = String.Empty

        Dim WW_TORICODE As String = String.Empty
        Dim WW_OILTYPE As String = String.Empty
        Dim WW_SHUKADATE As String = String.Empty
        Dim WW_KIJUNDATE As String = String.Empty
        Dim WW_ORDERORG As String = String.Empty
        Dim WW_SHIPORG As String = String.Empty
        Dim WW_DETAILNO As String = String.Empty

        O_TBL = I_TBL.Clone

        '■■■ 荷主受注(T00004SUMtbl)作成 ■■■

        'Sort　…　念のため
        CS0026TblSort.TABLE = I_TBL
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        I_TBL = CS0026TblSort.sort()

        '○集計用DBへ構造＆データをコピー
        Dim WW_T0004SUMtbl As DataTable = I_TBL.Copy()
        '積置レコード削除
        CS0026TblSort.TABLE = WW_T0004SUMtbl
        CS0026TblSort.FILTER = "(TUMIOKIKBN <> '1') or (TUMIOKIKBN = '1' and SHUKODATE <> SHUKADATE)"
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        WW_T0004SUMtbl = CS0026TblSort.sort()

        '削除レコード削除
        CS0026TblSort.TABLE = WW_T0004SUMtbl
        CS0026TblSort.FILTER = "DELFLG <> '1'"
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        WW_T0004SUMtbl = CS0026TblSort.sort()
        '○マスク処理
        Dim WW_TODOKEDATE As String = String.Empty
        Dim WW_SHUKODATE As String = String.Empty
        Dim WW_SHUKABASHO As String = String.Empty
        Dim WW_GSHABAN As String = String.Empty
        Dim WW_SHAFUKU As String = String.Empty
        Dim WW_STAFFCODE As String = String.Empty
        Dim WW_TODOKECODE As String = String.Empty
        Dim WW_PRODUCT1 As String = String.Empty
        Dim WW_PRODUCT2 As String = String.Empty
        Dim WW_TUMIOKIKBN As String = String.Empty

        For i As Integer = 0 To WW_T0004SUMtbl.Rows.Count - 1
            Dim WW_T0004SUMrow As DataRow = WW_T0004SUMtbl.Rows(i)

            '荷主受注集計制御マスタ取得()
            GS0029T3CNTLget.CAMPCODE = WW_T0004SUMrow("CAMPCODE")
            GS0029T3CNTLget.TORICODE = WW_T0004SUMrow("TORICODE")
            GS0029T3CNTLget.OILTYPE = WW_T0004SUMrow("OILTYPE")
            GS0029T3CNTLget.ORDERORG = WW_T0004SUMrow("ORDERORG")
            GS0029T3CNTLget.KIJUNDATE = WW_T0004SUMrow("KIJUNDATE")
            GS0029T3CNTLget.GS0029T3CNTLget()

            If Not isNormal(GS0029T3CNTLget.ERR) Then
                CS0011LOGWRITE.INFSUBCLASS = "T0004SUMtblget"               'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "GS0029T3CNTLget"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = "荷主受注集計制御マスタ登録なし(" & WW_T0004SUMrow("TORICODE") & ")"
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                O_RTN = GS0029T3CNTLget.ERR
                Exit Sub
            End If

            If GS0029T3CNTLget.CNTL01 <> "1" Then                                '集計区分(積置区分)
                WW_T0004SUMrow("TUMIOKIKBN") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL02 <> "1" Then                                '集計区分(出庫日)
                WW_T0004SUMrow("SHUKODATE") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL03 <> "1" Then                                '集計区分(出荷場所)
                WW_T0004SUMrow("SHUKABASHO") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL04 <> "1" Then                                '集計区分(業務車番)
                WW_T0004SUMrow("GSHABAN") = String.Empty
                WW_T0004SUMrow("SHARYOTYPEF") = String.Empty
                WW_T0004SUMrow("TSHABANF") = String.Empty
                WW_T0004SUMrow("SHARYOTYPEB") = String.Empty
                WW_T0004SUMrow("TSHABANB") = String.Empty
                WW_T0004SUMrow("SHARYOTYPEB2") = String.Empty
                WW_T0004SUMrow("TSHABANB2") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL05 <> "1" Then                                '集計区分(車腹(積載量))
                WW_T0004SUMrow("SHAFUKU") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL06 <> "1" Then                                '集計区分(乗務員コード)
                WW_T0004SUMrow("STAFFCODE") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL07 <> "1" Then                                '集計区分(届先コード)
                WW_T0004SUMrow("TODOKECODE") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL08 <> "1" Then                                '集計区分(品名１)
                WW_T0004SUMrow("PRODUCT1") = String.Empty
            End If
            If GS0029T3CNTLget.CNTL09 <> "1" Then                                '集計区分(品名２)
                WW_T0004SUMrow("PRODUCT2") = String.Empty
            End If
            'TRIPNO ,DROPNO , SEQクリア
            WW_T0004SUMrow("TRIPNO") = "000"
            WW_T0004SUMrow("DROPNO") = "000"
            WW_T0004SUMrow("SEQ") = "00"

        Next

        '■■■ DetailNO毎に台数、数量をサマリ ■■■
        Dim SURYO_SUM As Decimal = 0
        Dim JSURYO_SUM As Decimal = 0
        Dim DAISU_SUM As Long = 0
        Dim JDAISU_SUM As Long = 0
        Dim STATUS_MAX As String = String.Empty

        'sort
        CS0026TblSort.TABLE = WW_T0004SUMtbl
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "DELFLG ,TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,TUMIOKIKBN ,SHUKADATE ,TODOKEDATE ,SHUKODATE ,SHUKABASHO ,GSHABAN ,SHAFUKU ,STAFFCODE ,TODOKECODE ,PRODUCT1 ,PRODUCT2"
        WW_T0004SUMtbl = CS0026TblSort.sort()

        '最終行から初回行へループ
        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty
        WW_DETAILNO = String.Empty

        For i As Integer = 0 To WW_T0004SUMtbl.Rows.Count - 1

            Dim WW_T0004SUMrow As DataRow = WW_T0004SUMtbl.Rows(i)

            '受注＋DetailNo毎に集計　…　受注＋DetailNo＋荷主受注集計制御でブレイク
            If WW_TORICODE = WW_T0004SUMrow("TORICODE") AndAlso
               WW_OILTYPE = WW_T0004SUMrow("OILTYPE") AndAlso
               WW_KIJUNDATE = WW_T0004SUMrow("KIJUNDATE") AndAlso
               WW_ORDERORG = WW_T0004SUMrow("ORDERORG") AndAlso
               WW_SHIPORG = WW_T0004SUMrow("SHIPORG") AndAlso
               WW_TODOKEDATE = WW_T0004SUMrow("TODOKEDATE") AndAlso
               WW_SHUKODATE = WW_T0004SUMrow("SHUKODATE") AndAlso
               WW_SHUKADATE = WW_T0004SUMrow("SHUKADATE") AndAlso
               WW_SHUKABASHO = WW_T0004SUMrow("SHUKABASHO") AndAlso
               WW_GSHABAN = WW_T0004SUMrow("GSHABAN") AndAlso
               WW_SHAFUKU = WW_T0004SUMrow("SHAFUKU") AndAlso
               WW_STAFFCODE = WW_T0004SUMrow("STAFFCODE") AndAlso
               WW_TODOKECODE = WW_T0004SUMrow("TODOKECODE") AndAlso
               WW_TUMIOKIKBN = WW_T0004SUMrow("TUMIOKIKBN") AndAlso
               WW_PRODUCT1 = WW_T0004SUMrow("PRODUCT1") AndAlso
               WW_PRODUCT2 = WW_T0004SUMrow("PRODUCT2") Then

            Else
                SURYO_SUM = 0
                DAISU_SUM = 0
                JSURYO_SUM = 0
                JDAISU_SUM = 0
                STATUS_MAX = String.Empty

                For j As Integer = i To WW_T0004SUMtbl.Rows.Count - 1
                    If WW_T0004SUMtbl.Rows(j)("TORICODE") = WW_T0004SUMrow("TORICODE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("OILTYPE") = WW_T0004SUMrow("OILTYPE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("KIJUNDATE") = WW_T0004SUMrow("KIJUNDATE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("ORDERORG") = WW_T0004SUMrow("ORDERORG") AndAlso
                       WW_T0004SUMtbl.Rows(j)("SHIPORG") = WW_T0004SUMrow("SHIPORG") AndAlso
                       WW_T0004SUMtbl.Rows(j)("TODOKEDATE") = WW_T0004SUMrow("TODOKEDATE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("SHUKODATE") = WW_T0004SUMrow("SHUKODATE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("SHUKADATE") = WW_T0004SUMrow("SHUKADATE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("SHUKABASHO") = WW_T0004SUMrow("SHUKABASHO") AndAlso
                       WW_T0004SUMtbl.Rows(j)("GSHABAN") = WW_T0004SUMrow("GSHABAN") AndAlso
                       WW_T0004SUMtbl.Rows(j)("SHAFUKU") = WW_T0004SUMrow("SHAFUKU") AndAlso
                       WW_T0004SUMtbl.Rows(j)("STAFFCODE") = WW_T0004SUMrow("STAFFCODE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("TODOKECODE") = WW_T0004SUMrow("TODOKECODE") AndAlso
                       WW_T0004SUMtbl.Rows(j)("TUMIOKIKBN") = WW_T0004SUMrow("TUMIOKIKBN") AndAlso
                       WW_T0004SUMtbl.Rows(j)("PRODUCT1") = WW_T0004SUMrow("PRODUCT1") AndAlso
                       WW_T0004SUMtbl.Rows(j)("PRODUCT2") = WW_T0004SUMrow("PRODUCT2") AndAlso
                       WW_T0004SUMtbl.Rows(j)("DELFLG") <> C_DELETE_FLG.DELETE Then

                        Try
                            SURYO_SUM = SURYO_SUM + CDbl(WW_T0004SUMtbl.Rows(j)("SURYO"))
                            JSURYO_SUM = JSURYO_SUM + CDbl(WW_T0004SUMtbl.Rows(j)("JSURYO"))
                        Catch ex As Exception
                        End Try

                        Try
                            DAISU_SUM = DAISU_SUM + CInt(WW_T0004SUMtbl.Rows(j)("DAISU"))
                            JDAISU_SUM = JDAISU_SUM + CInt(WW_T0004SUMtbl.Rows(j)("JDAISU"))
                        Catch ex As Exception
                        End Try

                        If STATUS_MAX < WW_T0004SUMtbl.Rows(j)("STATUS") Then
                            STATUS_MAX = WW_T0004SUMtbl.Rows(j)("STATUS")
                        End If
                    Else
                        Exit For
                    End If

                Next

                'サマリ結果を反映
                WW_T0004SUMrow("SURYO") = SURYO_SUM.ToString("0.000")
                WW_T0004SUMrow("DAISU") = DAISU_SUM.ToString("0")
                WW_T0004SUMrow("JSURYO") = JSURYO_SUM.ToString("0.000")
                WW_T0004SUMrow("JDAISU") = JDAISU_SUM.ToString("0")
                WW_T0004SUMrow("STATUS") = STATUS_MAX
                If WW_T0004SUMrow("SURYO") = 0 AndAlso WW_T0004SUMrow("JSURYO") = 0 Then
                    WW_T0004SUMrow("DELFLG") = "1"
                End If
                Dim WW_Row As DataRow = O_TBL.NewRow
                WW_Row.ItemArray = WW_T0004SUMrow.ItemArray
                O_TBL.Rows.Add(WW_Row)

                WW_TORICODE = WW_T0004SUMrow("TORICODE")
                WW_OILTYPE = WW_T0004SUMrow("OILTYPE")
                WW_KIJUNDATE = WW_T0004SUMrow("KIJUNDATE")
                WW_ORDERORG = WW_T0004SUMrow("ORDERORG")
                WW_SHIPORG = WW_T0004SUMrow("SHIPORG")
                WW_TODOKEDATE = WW_T0004SUMrow("TODOKEDATE")
                WW_SHUKODATE = WW_T0004SUMrow("SHUKODATE")
                WW_SHUKADATE = WW_T0004SUMrow("SHUKADATE")
                WW_SHUKABASHO = WW_T0004SUMrow("SHUKABASHO")
                WW_GSHABAN = WW_T0004SUMrow("GSHABAN")
                WW_SHAFUKU = WW_T0004SUMrow("SHAFUKU")
                WW_STAFFCODE = WW_T0004SUMrow("STAFFCODE")
                WW_TODOKECODE = WW_T0004SUMrow("TODOKECODE")
                WW_TUMIOKIKBN = WW_T0004SUMrow("TUMIOKIKBN")
                WW_PRODUCT1 = WW_T0004SUMrow("PRODUCT1")
                WW_PRODUCT2 = WW_T0004SUMrow("PRODUCT2")

            End If

        Next
        '○不要レコード削除
        CS0026TblSort.TABLE = O_TBL
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG"
        O_TBL = CS0026TblSort.sort()


        '■■■ T00004SUMtblのDetailNO、SEQを再付番 ■■■

        '○DetailNO再付番
        Dim WW_DETAILNOcnt As Integer = 0
        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty
        For Each T00004SUMrow As DataRow In O_TBL.Rows

            If T00004SUMrow("DELFLG") <> C_DELETE_FLG.DELETE Then
                If WW_TORICODE = T00004SUMrow("TORICODE") AndAlso
                   WW_OILTYPE = T00004SUMrow("OILTYPE") AndAlso
                   WW_KIJUNDATE = T00004SUMrow("KIJUNDATE") AndAlso
                   WW_ORDERORG = T00004SUMrow("ORDERORG") AndAlso
                   WW_SHIPORG = T00004SUMrow("SHIPORG") Then
                    WW_DETAILNOcnt = WW_DETAILNOcnt + 1
                    T00004SUMrow("DETAILNO") = WW_DETAILNOcnt.ToString("000")
                Else
                    WW_DETAILNOcnt = 1
                    T00004SUMrow("DETAILNO") = WW_DETAILNOcnt.ToString("000")

                    WW_TORICODE = T00004SUMrow("TORICODE")
                    WW_OILTYPE = T00004SUMrow("OILTYPE")
                    WW_KIJUNDATE = T00004SUMrow("KIJUNDATE")
                    WW_ORDERORG = T00004SUMrow("ORDERORG")
                    WW_SHIPORG = T00004SUMrow("SHIPORG")

                End If
            End If

        Next

        '○close
        WW_T0004SUMtbl.Dispose()
        WW_T0004SUMtbl = Nothing

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    '''  T0004tbl（配送受注）編集
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <param name="I_DATENOW"></param>
    ''' <param name="O_TBL4"></param>
    ''' <remarks></remarks>
    Private Sub EditT0004(ByVal I_ROW As DataRow,
                           ByVal I_DATENOW As Date,
                           ByRef O_TBL4 As DataTable)

        Dim WW_CONVERT As String = String.Empty
        Dim WW_TEXT As String = String.Empty
        Dim WW_RTN As String = String.Empty

        Dim WW_T0004row As DataRow = O_TBL4.NewRow

        '会社コード
        WW_T0004row("CAMPCODE") = I_ROW("CAMPCODE")
        '端末設置部署
        WW_T0004row("TERMORG") = CS0050Session.APSV_ORG
        '受注番号
        WW_T0004row("ORDERNO") = I_ROW("ORDERNO")
        '明細№
        WW_T0004row("DETAILNO") = I_ROW("DETAILNO").PadLeft(3, "0")
        'トリップ
        WW_T0004row("TRIPNO") = I_ROW("TRIPNO").PadLeft(3, "0")
        'ドロップ
        WW_T0004row("DROPNO") = I_ROW("DROPNO").PadLeft(3, "0")
        '枝番
        WW_T0004row("SEQ") = I_ROW("SEQ").PadLeft(2, "0")
        'エントリー日時
        WW_T0004row("ENTRYDATE") = I_DATENOW.ToString("yyyyMMddHHmmss")
        '取引先
        WW_T0004row("TORICODE") = I_ROW("TORICODE")
        '油種
        WW_T0004row("OILTYPE") = I_ROW("OILTYPE1")
        '請求先コード
        WW_T0004row("STORICODE") = String.Empty
        GetSTORICODE(I_ROW, WW_T0004row("STORICODE"))
        '受注受付部署
        WW_T0004row("ORDERORG") = I_ROW("SHIPORG")
        '基準日
        If I_ROW("URIKBN") = "1" Then
            WW_T0004row("KIJUNDATE") = I_ROW("SHUKADATE")
        Else
            WW_T0004row("KIJUNDATE") = I_ROW("TODOKEDATE")
        End If
        '出庫日
        WW_T0004row("SHUKODATE") = I_ROW("YMD")
        '帰庫日
        WW_T0004row("KIKODATE") = I_ROW("YMD")
        '出荷日
        WW_T0004row("SHUKADATE") = I_ROW("SHUKADATE")
        '積置き
        WW_T0004row("TUMIOKIKBN") = I_ROW("TUMIOKIKBN")
        '売上計上基準（出荷基準）
        WW_T0004row("URIKBN") = I_ROW("URIKBN")
        '状態（受注実績）
        WW_T0004row("STATUS") = "3"
        '出荷部署
        WW_T0004row("SHIPORG") = I_ROW("SHIPORG")
        '出荷場所
        WW_T0004row("SHUKABASHO") = I_ROW("SHUKABASHO")
        '時間指定（入構）
        WW_T0004row("INTIME") = String.Empty
        '時間指定（出構）
        WW_T0004row("OUTTIME") = String.Empty
        '出荷伝票番号
        WW_T0004row("SHUKADENNO") = String.Empty
        '積順
        WW_T0004row("TUMISEQ") = "0"
        '積場
        WW_T0004row("TUMIBA") = String.Empty
        'ゲート
        WW_T0004row("GATE") = String.Empty
        '業務車番
        WW_T0004row("GSHABAN") = I_ROW("GSHABAN")
        '両目
        WW_T0004row("RYOME") = "1"
        'コンテナシャーシ
        WW_T0004row("CONTCHASSIS") = I_ROW("CONTCHASSIS")
        '車腹
        WW_T0004row("SHAFUKU") = "0.000"
        For i As Integer = 0 To ListBoxGSHABAN.Items.Count - 1
            Dim WW_SPLIT() As String = ListBoxGSHABAN.Items(i).Value.Split(",")
            If WW_SPLIT(2) = I_ROW("GSHABAN") Then
                Dim WW_SPLIT2() As String = ListBoxGSHABAN.Items(i).Text.Split(" ")
                WW_T0004row("SHAFUKU") = WW_SPLIT2(6)
            End If
        Next
        '乗務員コード
        WW_T0004row("STAFFCODE") = I_ROW("STAFFCODE")
        '副乗務員コード
        WW_T0004row("SUBSTAFFCODE") = I_ROW("SUBSTAFFCODE")
        '出勤時間
        WW_T0004row("STTIME") = String.Empty
        '荷主受注番号
        WW_T0004row("TORIORDERNO") = String.Empty
        '届日
        WW_T0004row("TODOKEDATE") = I_ROW("TODOKEDATE")
        '時間指定(配送)
        WW_T0004row("TODOKETIME") = String.Empty
        '届先
        WW_T0004row("TODOKECODE") = I_ROW("TODOKECODE")
        '品名１
        WW_T0004row("PRODUCT1") = I_ROW("PRODUCT11")
        '品名２
        WW_T0004row("PRODUCT2") = I_ROW("PRODUCT21")
        '品名コード
        WW_T0004row("PRODUCTCODE") = I_ROW("PRODUCTCODE1")
        'P比率
        WW_T0004row("PRATIO") = "0.0"
        '臭有無
        WW_T0004row("SMELLKBN") = String.Empty
        'コンテナ番号
        WW_T0004row("CONTNO") = String.Empty
        '数量
        WW_T0004row("SURYO") = "0.000"
        '台数
        WW_T0004row("DAISU") = "0"
        '配送実績数量
        WW_T0004row("JSURYO") = CType(I_ROW("SURYO1"), Double)
        '配送実績台数
        WW_T0004row("JDAISU") = "1"
        '配送単位
        WW_T0004row("HTANI") = I_ROW("STANI1")
        '配送実績単位
        WW_T0004row("STANI") = I_ROW("STANI1")
        '備考１～６
        WW_T0004row("REMARKS1") = String.Empty
        WW_T0004row("REMARKS2") = String.Empty
        WW_T0004row("REMARKS3") = String.Empty
        WW_T0004row("REMARKS4") = String.Empty
        WW_T0004row("REMARKS5") = String.Empty
        WW_T0004row("REMARKS6") = String.Empty
        WW_T0004row("SHARYOTYPEF") = I_ROW("SHARYOTYPEF")
        WW_T0004row("TSHABANF") = I_ROW("TSHABANF")
        WW_T0004row("SHARYOTYPEB") = I_ROW("SHARYOTYPEB")
        WW_T0004row("TSHABANB") = I_ROW("TSHABANB")
        WW_T0004row("SHARYOTYPEB2") = I_ROW("SHARYOTYPEB2")
        WW_T0004row("TSHABANB2") = I_ROW("TSHABANB2")
        WW_T0004row("TAXKBN") = I_ROW("TAXKBN")
        '削除フラグ
        WW_T0004row("DELFLG") = I_ROW("DELFLG")
        WW_T0004row("INITYMD") = I_DATENOW
        WW_T0004row("UPDYMD") = I_DATENOW
        WW_T0004row("UPDUSER") = UPDUSERID
        WW_T0004row("UPDTERMID") = UPDTERMID
        WW_T0004row("RECEIVEYMD") = C_DEFAULT_YMD

        O_TBL4.Rows.Add(WW_T0004row)

        ''荷主受注DB編集（枝番=1をベースに数量をサマリーする）
        ''　※ここでは、枝番=1よりベースのレコードを作成するだけ！
    End Sub

    ''' <summary>
    '''  請求先取得
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <param name="O_STORICODE"></param>
    ''' <remarks></remarks>
    Private Sub GetStoriCode(ByVal I_ROW As DataRow, ByRef O_STORICODE As String)
        Dim WW_date As String = String.Empty

        Try
            O_STORICODE = String.Empty
            'DataBase接続文字
            Dim SQLStr As String =
                  "       SELECT rtrim(A.STORICODE)  as STORICODE       " _
                & "         FROM MC003_TORIORG      as A 			    " _
                & "   INNER JOIN MC002_TORIHIKISAKI as B 		        " _
                & "           ON B.TORICODE   	= A.STORICODE 		    " _
                & "          and B.STYMD       <= @P1 				    " _
                & "          and B.ENDYMD      >= @P1 				    " _
                & "          and B.DELFLG      <> '1' 				    " _
                & "        Where A.CAMPCODE     = @P2 				    " _
                & "          and A.UORG     	= @P3 				    " _
                & "          and A.TORICODE 	= @P4 				    " _
                & "          and A.DELFLG      <> '1' 				    "

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)
                PARA1.Value = Date.Now
                PARA2.Value = I_ROW("CAMPCODE")
                PARA3.Value = I_ROW("SHIPORG")
                PARA4.Value = I_ROW("TORICODE")
                SQLcmd.CommandTimeout = 300
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        '○出力編集
                        O_STORICODE = SQLdr("STORICODE")
                    End While

                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GetStoriCode"                      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MC002_TORIHIKISAKI Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.NORMAL
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 配送受注ＤＢ取得
    ''' </summary>
    ''' <param name="I_SELKBN"></param>
    ''' <param name="I_ROW"></param>
    ''' <param name="O_TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub SelectT0004(ByVal I_SELKBN As String, ByVal I_ROW As DataRow, ByRef O_TBL As DataTable, ByRef O_RTN As String)

        'オブジェクト内容検索
        Try
            Dim WW_T0004WKtbl As DataTable = O_TBL.Clone
            Dim SQLStr As String = String.Empty
            Dim SQLWhere As String = String.Empty

            '検索SQL文
            SQLStr =
                 "SELECT isnull(rtrim(A.CAMPCODE),'')          as CAMPCODE ,       " _
               & "       isnull(rtrim(A.TERMORG),'')           as TERMORG ,        " _
               & "       isnull(rtrim(A.ORDERNO),'')           as ORDERNO ,        " _
               & "       isnull(rtrim(A.DETAILNO),'')          as DETAILNO ,       " _
               & "       isnull(rtrim(A.TRIPNO),'')            as TRIPNO ,         " _
               & "       isnull(rtrim(A.DROPNO),'')            as DROPNO ,         " _
               & "       isnull(rtrim(A.SEQ),'')               as SEQ ,            " _
               & "       isnull(rtrim(A.TORICODE),'')          as TORICODE ,       " _
               & "       isnull(rtrim(A.OILTYPE),'')           as OILTYPE ,        " _
               & "       isnull(rtrim(A.STORICODE),'')         as STORICODE ,      " _
               & "       isnull(rtrim(A.ORDERORG),'')          as ORDERORG ,       " _
               & "       isnull(rtrim(A.KIJUNDATE),'')         as KIJUNDATE ,      " _
               & "       isnull(rtrim(A.SHUKODATE),'')         as SHUKODATE ,      " _
               & "       isnull(rtrim(A.KIKODATE),'')          as KIKODATE ,       " _
               & "       isnull(rtrim(A.SHUKADATE),'')         as SHUKADATE ,      " _
               & "       isnull(rtrim(A.TUMIOKIKBN),'')        as TUMIOKIKBN ,     " _
               & "       isnull(rtrim(A.URIKBN),'')            as URIKBN ,         " _
               & "       isnull(rtrim(A.STATUS),'')            as STATUS ,         " _
               & "       isnull(rtrim(A.SHIPORG),'')           as SHIPORG ,        " _
               & "       isnull(rtrim(A.SHUKABASHO),'')        as SHUKABASHO ,     " _
               & "       isnull(rtrim(A.INTIME),'')            as INTIME ,         " _
               & "       isnull(rtrim(A.OUTTIME),'')           as OUTTIME ,        " _
               & "       isnull(rtrim(A.SHUKADENNO),'')        as SHUKADENNO ,     " _
               & "       isnull(rtrim(A.TUMISEQ),'')           as TUMISEQ ,        " _
               & "       isnull(rtrim(A.TUMIBA),'')            as TUMIBA ,         " _
               & "       isnull(rtrim(A.GATE),'')              as GATE ,           " _
               & "       isnull(rtrim(A.GSHABAN),'')           as GSHABAN ,        " _
               & "       isnull(rtrim(A.RYOME),'')             as RYOME ,          " _
               & "       isnull(rtrim(A.CONTCHASSIS),'')       as CONTCHASSIS ,    " _
               & "       isnull(rtrim(A.SHAFUKU),'')           as SHAFUKU ,        " _
               & "       isnull(rtrim(A.STAFFCODE),'')         as STAFFCODE ,      " _
               & "       isnull(rtrim(A.SUBSTAFFCODE),'')      as SUBSTAFFCODE ,   " _
               & "       isnull(rtrim(A.STTIME),'')            as STTIME ,         " _
               & "       isnull(rtrim(A.TORIORDERNO),'')       as TORIORDERNO ,    " _
               & "       isnull(rtrim(A.TODOKEDATE),'')        as TODOKEDATE ,     " _
               & "       isnull(rtrim(A.TODOKETIME),'')        as TODOKETIME ,     " _
               & "       isnull(rtrim(A.TODOKECODE),'')        as TODOKECODE ,     " _
               & "       isnull(rtrim(A.PRODUCT1),'')          as PRODUCT1 ,       " _
               & "       isnull(rtrim(A.PRODUCT2),'')          as PRODUCT2 ,       " _
               & "       isnull(rtrim(A.PRODUCTCODE),'')       as PRODUCTCODE,     " _
               & "       isnull(rtrim(A.PRATIO),'')            as PRATIO ,         " _
               & "       isnull(rtrim(A.SMELLKBN),'')          as SMELLKBN ,       " _
               & "       isnull(rtrim(A.CONTNO),'')            as CONTNO ,         " _
               & "       isnull(rtrim(A.HTANI),'')             as HTANI ,          " _
               & "       isnull(rtrim(A.SURYO),'')             as SURYO ,          " _
               & "       isnull(rtrim(A.DAISU),'')             as DAISU ,          " _
               & "       isnull(rtrim(A.STANI),'')             as STANI ,          " _
               & "       isnull(rtrim(A.JSURYO),'')            as JSURYO ,         " _
               & "       isnull(rtrim(A.JDAISU),'')            as JDAISU ,         " _
               & "       isnull(rtrim(A.REMARKS1),'')          as REMARKS1 ,       " _
               & "       isnull(rtrim(A.REMARKS2),'')          as REMARKS2 ,       " _
               & "       isnull(rtrim(A.REMARKS3),'')          as REMARKS3 ,       " _
               & "       isnull(rtrim(A.REMARKS4),'')          as REMARKS4 ,       " _
               & "       isnull(rtrim(A.REMARKS5),'')          as REMARKS5 ,       " _
               & "       isnull(rtrim(A.REMARKS6),'')          as REMARKS6 ,       " _
               & "       isnull(rtrim(A.SHARYOTYPEF),'')       as SHARYOTYPEF ,    " _
               & "       isnull(rtrim(A.TSHABANF),'')          as TSHABANF ,       " _
               & "       isnull(rtrim(A.SHARYOTYPEB),'')       as SHARYOTYPEB ,    " _
               & "       isnull(rtrim(A.TSHABANB),'')          as TSHABANB ,       " _
               & "       isnull(rtrim(A.SHARYOTYPEB2),'')      as SHARYOTYPEB2 ,   " _
               & "       isnull(rtrim(A.TSHABANB2),'')         as TSHABANB2 ,      " _
               & "       isnull(rtrim(A.TAXKBN),'')            as TAXKBN ,         " _
               & "       isnull(rtrim(A.DELFLG),'')            as DELFLG           " _
               & "  FROM T0004_HORDER AS A								"

            If I_SELKBN = "1" Then
                SQLWhere =
                     " WHERE A.CAMPCODE         = @P01                      " _
                   & "  and  A.TORICODE         = @P02                      " _
                   & "  and  A.OILTYPE          = @P03           		    " _
                   & "  and  A.SHIPORG          = @P04           		    " _
                   & "  and  A.TODOKEDATE       = @P05           		    " _
                   & "  and  A.GSHABAN          = @P06           		    " _
                   & "  and  A.STAFFCODE        = @P07           		    " _
                   & "  and  A.SHUKODATE        = @P08           		    " _
                   & "  and  A.DELFLG          <> '1'                       " _
                   & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.SHUKADATE ,       " _
                   & " 		    A.ORDERORG  ,A.SHIPORG ,A.GSHABAN           "
            ElseIf I_SELKBN = "2" Then
                SQLWhere =
                     " WHERE A.CAMPCODE         = @P01                      " _
                   & "  and  A.TORICODE         = @P02                      " _
                   & "  and  A.OILTYPE          = @P03           		    " _
                   & "  and  A.SHIPORG          = @P04           		    " _
                   & "  and  A.KIJUNDATE        = @P05           		    " _
                   & "  and  A.ORDERORG         = @P06           		    " _
                   & "  and  A.DELFLG          <> '1'                       " _
                   & " ORDER BY A.TORICODE  ,A.OILTYPE ,A.KIJUNDATE ,       " _
                   & " 		    A.ORDERORG  ,A.SHIPORG ,A.GSHABAN           "
            End If

            SQLStr = SQLStr & SQLWhere
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon, SQLtrn)

                If I_SELKBN = "1" Then
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)  '荷主
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)  '油種
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出庫日、出荷日
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)  '業務車番
                    Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 20)  '乗務員コード
                    Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.Date)      '出庫日
                    '○関連受注指定
                    PARA01.Value = I_ROW("CAMPCODE")        '会社
                    PARA02.Value = I_ROW("TORICODE")        '取引先
                    PARA03.Value = I_ROW("OILTYPE")         '油種
                    PARA04.Value = I_ROW("SHIPORG")         '出荷部署
                    PARA05.Value = I_ROW("TODOKEDATE")      '届日
                    PARA06.Value = I_ROW("GSHABAN")         '業務車番
                    PARA07.Value = I_ROW("STAFFCODE")       '乗務員コード
                    PARA08.Value = I_ROW("SHUKODATE")       '届日
                ElseIf I_SELKBN = "2" Then
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)  '荷主
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)  '油種
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)  '出荷部署
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)      '出庫日、出荷日
                    Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)  '受注部署
                    '○関連受注指定
                    PARA01.Value = I_ROW("CAMPCODE")        '会社
                    PARA02.Value = I_ROW("TORICODE")        '取引先
                    PARA03.Value = I_ROW("OILTYPE")         '油種
                    PARA04.Value = I_ROW("SHIPORG")         '出荷部署
                    PARA05.Value = I_ROW("KIJUNDATE")       '基準日
                    PARA06.Value = I_ROW("ORDERORG")        '受注部署
                End If

                '■SQL実行
                SQLcmd.CommandTimeout = 300
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    '■テーブル検索結果をテーブル格納
                    WW_T0004WKtbl.Load(SQLdr)
                End Using
                Dim WW_DATE As String = String.Empty
                For i As Integer = 0 To WW_T0004WKtbl.Rows.Count - 1
                    Dim WW_row As DataRow = WW_T0004WKtbl.Rows(i)
                    WW_DATE = CDate(WW_row("KIJUNDATE")).ToString("yyyy/MM/dd")
                    WW_row("KIJUNDATE") = WW_DATE
                    WW_DATE = CDate(WW_row("SHUKODATE")).ToString("yyyy/MM/dd")
                    WW_row("SHUKODATE") = WW_DATE
                    WW_DATE = CDate(WW_row("SHUKADATE")).ToString("yyyy/MM/dd")
                    WW_row("SHUKADATE") = WW_DATE
                    WW_DATE = CDate(WW_row("KIKODATE")).ToString("yyyy/MM/dd")
                    WW_row("KIKODATE") = WW_DATE
                    WW_DATE = CDate(WW_row("TODOKEDATE")).ToString("yyyy/MM/dd")
                    WW_row("TODOKEDATE") = WW_DATE
                Next
                O_TBL.Merge(WW_T0004WKtbl, False)

            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0004_Select"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0004_HORDER SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' T0003・T0004tbl関連データ削除
    ''' </summary>
    ''' <param name="I_TBL"></param>
    ''' <param name="I_DATENOW"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub DeleteT0003AndT0004(ByVal I_TBL As DataTable, ByVal I_DATENOW As Date, ByRef O_RTN As String)

        '更新対象受注の画面非表示（他出庫日）を取得。配送受注の更新最小単位は出荷部署単位。

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_SORTstr As String = String.Empty
        Dim WW_FILLstr As String = String.Empty

        Dim WW_TORICODE As String = String.Empty
        Dim WW_OILTYPE As String = String.Empty
        Dim WW_SHUKADATE As String = String.Empty
        Dim WW_KIJUNDATE As String = String.Empty
        Dim WW_ORDERORG As String = String.Empty
        Dim WW_SHIPORG As String = String.Empty

        '■■■ T0004tbl関連の荷主受注・配送受注を論理削除 ■■■　…　削除情報はT0004tblに存在
        'Sort
        CS0026TblSort.TABLE = I_TBL
        CS0026TblSort.FILTER = String.Empty
        CS0026TblSort.SORTING = "TORICODE ,OILTYPE ,KIJUNDATE ,ORDERORG ,SHIPORG ,DELFLG"
        I_TBL = CS0026TblSort.sort()

        WW_TORICODE = String.Empty
        WW_OILTYPE = String.Empty
        WW_SHUKADATE = String.Empty
        WW_KIJUNDATE = String.Empty
        WW_ORDERORG = String.Empty
        WW_SHIPORG = String.Empty

        '更新SQL文･･･配送受注の現受注番号を一括論理削除
        Dim SQLT0004Str As String =
                  " UPDATE T0004_HORDER             " _
                & "    SET UPDYMD      = @P11,      " _
                & "        UPDUSER     = @P12,      " _
                & "        UPDTERMID   = @P13,      " _
                & "        RECEIVEYMD  = @P14,      " _
                & "        DELFLG      = '1'        " _
                & "  WHERE CAMPCODE    = @P01       " _
                & "    AND TORICODE    = @P02       " _
                & "    AND OILTYPE     = @P03       " _
                & "    AND ORDERORG    = @P04       " _
                & "    AND SHIPORG     = @P05       " _
                & "    AND KIJUNDATE   = @P06       " _
                & "    AND DELFLG     <> '1'        "

        Dim SQL4cmd As New SqlCommand(SQLT0004Str, SQLcon, SQLtrn)
        Dim PARA401 As SqlParameter = SQL4cmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA402 As SqlParameter = SQL4cmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA403 As SqlParameter = SQL4cmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA404 As SqlParameter = SQL4cmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA405 As SqlParameter = SQL4cmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA406 As SqlParameter = SQL4cmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)

        Dim PARA411 As SqlParameter = SQL4cmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
        Dim PARA412 As SqlParameter = SQL4cmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA413 As SqlParameter = SQL4cmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 30)
        Dim PARA414 As SqlParameter = SQL4cmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

        '更新SQL文･･･荷主受注の現受注番号を一括論理削除
        Dim SQLT0003Str As String =
                  " UPDATE T0003_NIORDER            " _
                & "    SET UPDYMD      = @P11,      " _
                & "        UPDUSER     = @P12,      " _
                & "        UPDTERMID   = @P13,      " _
                & "        RECEIVEYMD  = @P14,      " _
                & "        DELFLG      = '1'        " _
                & "  WHERE CAMPCODE    = @P01       " _
                & "    AND TORICODE    = @P02       " _
                & "    AND OILTYPE     = @P03       " _
                & "    AND ORDERORG    = @P04       " _
                & "    AND SHIPORG     = @P05       " _
                & "    AND KIJUNDATE   = @P06       " _
                & "    AND DELFLG     <> '1'        "
        Dim SQL3cmd As New SqlCommand(SQLT0003Str, SQLcon, SQLtrn)
        Dim PARA301 As SqlParameter = SQL3cmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA302 As SqlParameter = SQL3cmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA303 As SqlParameter = SQL3cmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA304 As SqlParameter = SQL3cmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA305 As SqlParameter = SQL3cmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA306 As SqlParameter = SQL3cmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)

        Dim PARA311 As SqlParameter = SQL3cmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
        Dim PARA312 As SqlParameter = SQL3cmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar, 20)
        Dim PARA313 As SqlParameter = SQL3cmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar, 30)
        Dim PARA314 As SqlParameter = SQL3cmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

        For Each WW_T0004DLTrow As DataRow In I_TBL.Rows

            If WW_T0004DLTrow("TORICODE") = WW_TORICODE AndAlso
               WW_T0004DLTrow("OILTYPE") = WW_OILTYPE AndAlso
               WW_T0004DLTrow("KIJUNDATE") = WW_KIJUNDATE AndAlso
               WW_T0004DLTrow("ORDERORG") = WW_ORDERORG AndAlso
               WW_T0004DLTrow("SHIPORG") = WW_SHIPORG Then
            Else
                '○T0004tbl関連の配送受注を論理削除
                Try

                    PARA401.Value = WW_T0004DLTrow("CAMPCODE")
                    PARA402.Value = WW_T0004DLTrow("TORICODE")
                    PARA403.Value = WW_T0004DLTrow("OILTYPE")
                    PARA404.Value = WW_T0004DLTrow("ORDERORG")
                    PARA405.Value = WW_T0004DLTrow("SHIPORG")
                    PARA406.Value = WW_T0004DLTrow("KIJUNDATE")

                    PARA411.Value = I_DATENOW
                    PARA412.Value = UPDUSERID
                    PARA413.Value = UPDTERMID
                    PARA414.Value = C_DEFAULT_YMD

                    SQL4cmd.CommandTimeout = 300
                    SQL4cmd.ExecuteNonQuery()

                Catch ex As Exception

                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0004_HORDER(old) DEL"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try

                '○T0004tbl関連の荷主受注を論理削除
                Try

                    PARA301.Value = WW_T0004DLTrow("CAMPCODE")
                    PARA302.Value = WW_T0004DLTrow("TORICODE")
                    PARA303.Value = WW_T0004DLTrow("OILTYPE")
                    PARA304.Value = WW_T0004DLTrow("ORDERORG")
                    PARA305.Value = WW_T0004DLTrow("SHIPORG")
                    PARA306.Value = WW_T0004DLTrow("KIJUNDATE")

                    PARA311.Value = I_DATENOW
                    PARA312.Value = UPDUSERID
                    PARA313.Value = UPDTERMID
                    PARA314.Value = C_DEFAULT_YMD

                    SQL3cmd.CommandTimeout = 300
                    SQL3cmd.ExecuteNonQuery()


                Catch ex As Exception
                    CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "DB:T0003_NIORDER(old) DEL"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    O_RTN = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try

                WW_TORICODE = WW_T0004DLTrow("TORICODE")
                WW_OILTYPE = WW_T0004DLTrow("OILTYPE")
                WW_KIJUNDATE = WW_T0004DLTrow("KIJUNDATE")
                WW_ORDERORG = WW_T0004DLTrow("ORDERORG")
                WW_SHIPORG = WW_T0004DLTrow("SHIPORG")

            End If

        Next
        'CLOSE
        SQL4cmd.Dispose()
        SQL4cmd = Nothing
        'CLOSE
        SQL3cmd.Dispose()
        SQL3cmd = Nothing

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    '''  荷主受注カラム設定
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Private Sub AddColumnT0003tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()
        'T0003DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("TORICODE", GetType(String))
        IO_TBL.Columns.Add("OILTYPE", GetType(String))
        IO_TBL.Columns.Add("ORDERORG", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))
        IO_TBL.Columns.Add("KIJUNDATE", GetType(String))
        IO_TBL.Columns.Add("ORDERNO", GetType(String))
        IO_TBL.Columns.Add("DETAILNO", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("TRIPNO", GetType(String))
        IO_TBL.Columns.Add("DROPNO", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("STATUS", GetType(String))
        IO_TBL.Columns.Add("TUMIOKIKBN", GetType(String))
        IO_TBL.Columns.Add("SHUKODATE", GetType(String))
        IO_TBL.Columns.Add("KIKODATE", GetType(String))
        IO_TBL.Columns.Add("SHUKADATE", GetType(String))
        IO_TBL.Columns.Add("TODOKEDATE", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO", GetType(String))
        IO_TBL.Columns.Add("GATE", GetType(String))
        IO_TBL.Columns.Add("TUMIBA", GetType(String))
        IO_TBL.Columns.Add("TUMISEQ", GetType(String))
        IO_TBL.Columns.Add("SHUKADENNO", GetType(String))
        IO_TBL.Columns.Add("INTIME", GetType(String))
        IO_TBL.Columns.Add("OUTTIME", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("SUBSTAFFCODE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("RYOME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("TODOKETIME", GetType(String))
        IO_TBL.Columns.Add("PRODUCT1", GetType(String))
        IO_TBL.Columns.Add("PRODUCT2", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE", GetType(String))
        IO_TBL.Columns.Add("CONTNO", GetType(String))
        IO_TBL.Columns.Add("PRATIO", GetType(String))
        IO_TBL.Columns.Add("SMELLKBN", GetType(String))
        IO_TBL.Columns.Add("SHAFUKU", GetType(String))
        IO_TBL.Columns.Add("HTANI", GetType(String))
        IO_TBL.Columns.Add("SURYO", GetType(String))
        IO_TBL.Columns.Add("DAISU", GetType(String))
        IO_TBL.Columns.Add("STANI", GetType(String))
        IO_TBL.Columns.Add("JSURYO", GetType(String))
        IO_TBL.Columns.Add("JDAISU", GetType(String))
        IO_TBL.Columns.Add("REMARKS1", GetType(String))
        IO_TBL.Columns.Add("REMARKS2", GetType(String))
        IO_TBL.Columns.Add("REMARKS3", GetType(String))
        IO_TBL.Columns.Add("REMARKS4", GetType(String))
        IO_TBL.Columns.Add("REMARKS5", GetType(String))
        IO_TBL.Columns.Add("REMARKS6", GetType(String))
        IO_TBL.Columns.Add("TORIORDERNO", GetType(String))
        IO_TBL.Columns.Add("STORICODE", GetType(String))
        IO_TBL.Columns.Add("URIKBN", GetType(String))
        IO_TBL.Columns.Add("TERMORG", GetType(String))
        IO_TBL.Columns.Add("CONTCHASSIS", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEF", GetType(String))
        IO_TBL.Columns.Add("TSHABANF", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB", GetType(String))
        IO_TBL.Columns.Add("TSHABANB", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB2", GetType(String))
        IO_TBL.Columns.Add("TSHABANB2", GetType(String))
        IO_TBL.Columns.Add("TAXKBN", GetType(String))
        IO_TBL.Columns.Add("DELFLG", GetType(String))
        IO_TBL.Columns.Add("INITYMD", GetType(String))
        IO_TBL.Columns.Add("UPDYMD", GetType(String))
        IO_TBL.Columns.Add("UPDUSER", GetType(String))
        IO_TBL.Columns.Add("UPDTERMID", GetType(String))
        IO_TBL.Columns.Add("RECEIVEYMD", GetType(String))

    End Sub

    ''' <summary>
    ''' 配送受注カラム設定
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Private Sub AddColumnT0004tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

        'T0004DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("TORICODE", GetType(String))
        IO_TBL.Columns.Add("OILTYPE", GetType(String))
        IO_TBL.Columns.Add("ORDERORG", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))
        IO_TBL.Columns.Add("KIJUNDATE", GetType(String))
        IO_TBL.Columns.Add("ORDERNO", GetType(String))
        IO_TBL.Columns.Add("DETAILNO", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("TRIPNO", GetType(String))
        IO_TBL.Columns.Add("DROPNO", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("SHUKODATE", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("STATUS", GetType(String))
        IO_TBL.Columns.Add("TUMIOKIKBN", GetType(String))
        IO_TBL.Columns.Add("KIKODATE", GetType(String))
        IO_TBL.Columns.Add("SHUKADATE", GetType(String))
        IO_TBL.Columns.Add("TODOKEDATE", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO", GetType(String))
        IO_TBL.Columns.Add("GATE", GetType(String))
        IO_TBL.Columns.Add("TUMIBA", GetType(String))
        IO_TBL.Columns.Add("TUMISEQ", GetType(String))
        IO_TBL.Columns.Add("SHUKADENNO", GetType(String))
        IO_TBL.Columns.Add("INTIME", GetType(String))
        IO_TBL.Columns.Add("OUTTIME", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("SUBSTAFFCODE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("RYOME", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("TODOKETIME", GetType(String))
        IO_TBL.Columns.Add("PRODUCT1", GetType(String))
        IO_TBL.Columns.Add("PRODUCT2", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE", GetType(String))
        IO_TBL.Columns.Add("CONTNO", GetType(String))
        IO_TBL.Columns.Add("PRATIO", GetType(String))
        IO_TBL.Columns.Add("SMELLKBN", GetType(String))
        IO_TBL.Columns.Add("SHAFUKU", GetType(String))
        IO_TBL.Columns.Add("HTANI", GetType(String))
        IO_TBL.Columns.Add("SURYO", GetType(String))
        IO_TBL.Columns.Add("DAISU", GetType(String))
        IO_TBL.Columns.Add("STANI", GetType(String))
        IO_TBL.Columns.Add("JSURYO", GetType(String))
        IO_TBL.Columns.Add("JDAISU", GetType(String))
        IO_TBL.Columns.Add("REMARKS1", GetType(String))
        IO_TBL.Columns.Add("REMARKS2", GetType(String))
        IO_TBL.Columns.Add("REMARKS3", GetType(String))
        IO_TBL.Columns.Add("REMARKS4", GetType(String))
        IO_TBL.Columns.Add("REMARKS5", GetType(String))
        IO_TBL.Columns.Add("REMARKS6", GetType(String))
        IO_TBL.Columns.Add("TORIORDERNO", GetType(String))
        IO_TBL.Columns.Add("STORICODE", GetType(String))
        IO_TBL.Columns.Add("URIKBN", GetType(String))
        IO_TBL.Columns.Add("TERMORG", GetType(String))
        IO_TBL.Columns.Add("CONTCHASSIS", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEF", GetType(String))
        IO_TBL.Columns.Add("TSHABANF", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB", GetType(String))
        IO_TBL.Columns.Add("TSHABANB", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB2", GetType(String))
        IO_TBL.Columns.Add("TSHABANB2", GetType(String))
        IO_TBL.Columns.Add("TAXKBN", GetType(String))
        IO_TBL.Columns.Add("DELFLG", GetType(String))
        IO_TBL.Columns.Add("INITYMD", GetType(String))
        IO_TBL.Columns.Add("UPDYMD", GetType(String))
        IO_TBL.Columns.Add("UPDUSER", GetType(String))
        IO_TBL.Columns.Add("UPDTERMID", GetType(String))
        IO_TBL.Columns.Add("RECEIVEYMD", GetType(String))

    End Sub

    ''' <summary>
    ''' T0003かT0004に登録する
    ''' </summary>
    ''' <param name="I_TBLNAME"></param>
    ''' <param name="I_TBL"></param>
    ''' <param name="I_DATENOW"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub InsertT0003OrT0004(ByVal I_TBLNAME As String,
                                 ByVal I_TBL As DataTable,
                                 ByVal I_DATENOW As Date,
                                 ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_SORTstr As String = String.Empty
        Dim WW_FILLstr As String = String.Empty

        Dim WW_WKtbl As New DataTable
        'T3更新用データ
        If I_TBLNAME = "T0003_NIORDER" Then
            AddColumnT0003tbl(WW_WKtbl)
        Else
            AddColumnT0004tbl(WW_WKtbl)
        End If

        '■■■ T0004tblより配送受注追加 ■■■
        '
        For Each WW_UPDRow As DataRow In I_TBL.Rows

            Dim WW_WKrow As DataRow = WW_WKtbl.NewRow
            If WW_UPDRow("DELFLG") = C_DELETE_FLG.DELETE Then Continue For

            WW_WKrow("CAMPCODE") = WW_UPDRow("CAMPCODE")                         '会社コード(CAMPCODE)
            WW_WKrow("TERMORG") = WW_UPDRow("TERMORG")                           '端末設置部署(TERMORG)
            WW_WKrow("ORDERNO") = WW_UPDRow("ORDERNO").PadLeft(7, "0")           '受注番号(ORDERNO)
            WW_WKrow("DETAILNO") = WW_UPDRow("DETAILNO").PadLeft(3, "0")         '明細№(DETAILNO)
            WW_WKrow("TRIPNO") = WW_UPDRow("TRIPNO").PadLeft(3, "0")             'トリップ(TRIPNO)
            WW_WKrow("DROPNO") = WW_UPDRow("DROPNO").PadLeft(3, "0")             'ドロップ(DROPNO)
            WW_WKrow("SEQ") = WW_UPDRow("SEQ").PadLeft(2, "0")                   '枝番(SEQ)
            WW_WKrow("ENTRYDATE") = I_DATENOW.ToString("yyyyMMddHHmmssfff")             'エントリー日時(ENTRYDATE)
            WW_WKrow("TORICODE") = WW_UPDRow("TORICODE")                         '取引先コード(TORICODE)
            WW_WKrow("OILTYPE") = WW_UPDRow("OILTYPE")                           '油種(OILTYPE)
            WW_WKrow("STORICODE") = WW_UPDRow("STORICODE")                       '請求取引先コード(STORICODE)
            WW_WKrow("ORDERORG") = WW_UPDRow("ORDERORG")                         '受注受付部署(ORDERORG)
            If WW_UPDRow("SHUKODATE") = String.Empty Then                                        '出庫日(SHUKODATE)
                WW_WKrow("SHUKODATE") = C_DEFAULT_YMD
            Else
                WW_WKrow("SHUKODATE") = RTrim(WW_UPDRow("SHUKODATE"))
            End If
            If WW_UPDRow("KIKODATE") = String.Empty Then                                        '帰庫日(KIKODATE)
                WW_WKrow("KIKODATE") = C_DEFAULT_YMD
            Else
                WW_WKrow("KIKODATE") = RTrim(WW_UPDRow("KIKODATE"))
            End If
            If WW_UPDRow("SHUKADATE") = String.Empty Then                                       '出荷日(SHUKADATE)
                WW_WKrow("SHUKADATE") = C_DEFAULT_YMD
            Else
                WW_WKrow("SHUKADATE") = RTrim(WW_UPDRow("SHUKADATE"))
            End If
            WW_WKrow("TUMIOKIKBN") = WW_UPDRow("TUMIOKIKBN")                    '積置区分(TUMIOKIKBN)
            WW_WKrow("URIKBN") = WW_UPDRow("URIKBN")                            '売上計上基準(URIKBN)
            WW_WKrow("STATUS") = WW_UPDRow("STATUS")                            '状態(STATUS)
            WW_WKrow("SHIPORG") = WW_UPDRow("SHIPORG")                          '出荷部署(SHIPORG)
            WW_WKrow("SHUKABASHO") = WW_UPDRow("SHUKABASHO")                    '出荷場所(SHUKABASHO)
            WW_WKrow("INTIME") = WW_UPDRow("INTIME")                            '時間指定（入構）(INTIME)
            WW_WKrow("OUTTIME") = WW_UPDRow("OUTTIME")                          '時間指定（出構）(OUTTIME)
            WW_WKrow("SHUKADENNO") = WW_UPDRow("SHUKADENNO")                    '出荷伝票番号(SHUKADENNO)
            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("TUMISEQ"))) Then            '積順(TUMISEQ)
                WW_WKrow("TUMISEQ") = 0
            Else
                WW_WKrow("TUMISEQ") = WW_UPDRow("TUMISEQ")
            End If
            WW_WKrow("TUMIBA") = WW_UPDRow("TUMIBA")                            '積場(TUMIBA)
            WW_WKrow("GATE") = WW_UPDRow("GATE")                                'ゲート(GATE)
            WW_WKrow("GSHABAN") = WW_UPDRow("GSHABAN")                          '業務車番(GSHABAN)
            WW_WKrow("RYOME") = WW_UPDRow("RYOME")                              '両目(RYOME)
            WW_WKrow("CONTCHASSIS") = WW_UPDRow("CONTCHASSIS")                  'コンテナシャーシ(CONTCHASSIS)
            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("SHAFUKU"))) Then            '車腹（積載量）(SHAFUKU)
                WW_WKrow("SHAFUKU") = 0.0
            Else
                WW_WKrow("SHAFUKU") = CType(WW_UPDRow("SHAFUKU"), Double)
            End If
            WW_WKrow("STAFFCODE") = WW_UPDRow("STAFFCODE")                      '乗務員コード(STAFFCODE)
            WW_WKrow("SUBSTAFFCODE") = WW_UPDRow("SUBSTAFFCODE")                '副乗務員コード(SUBSTAFFCODE)
            WW_WKrow("STTIME") = WW_UPDRow("STTIME")                            '出勤時間(STTIME)
            WW_WKrow("TORIORDERNO") = String.Empty                                               '荷主受注番号(TORIORDERNO)
            If RTrim(WW_UPDRow("TODOKEDATE")) = String.Empty Then                               '届日(TODOKEDATE)
                WW_WKrow("TODOKEDATE") = C_DEFAULT_YMD
            Else
                WW_WKrow("TODOKEDATE") = RTrim(WW_UPDRow("TODOKEDATE"))
            End If
            WW_WKrow("TODOKETIME") = WW_UPDRow("TODOKETIME")                    '時間指定（配送）(TODOKETIME)
            WW_WKrow("TODOKECODE") = WW_UPDRow("TODOKECODE")                    '届先コード(TODOKECODE)
            WW_WKrow("PRODUCT1") = WW_UPDRow("PRODUCT1")                        '品名１(PRODUCT1)
            WW_WKrow("PRODUCT2") = WW_UPDRow("PRODUCT2")                        '品名２(PRODUCT2)
            WW_WKrow("PRODUCTCODE") = WW_UPDRow("PRODUCTCODE")                  '品名コード(PRODUCTCODE)

            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("PRATIO"))) Then             'Ｐ比率(PRATIO)
                WW_WKrow("PRATIO") = 0.0
            Else
                WW_WKrow("PRATIO") = CType(WW_UPDRow("PRATIO"), Double)
            End If
            WW_WKrow("SMELLKBN") = WW_UPDRow("SMELLKBN")                         '臭有無(SMELLKBN)
            WW_WKrow("CONTNO") = WW_UPDRow("CONTNO")                             'コンテナ番号(CONTNO)
            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("SURYO"))) Then               '数量(SURYO)
                WW_WKrow("SURYO") = 0.0
            Else
                WW_WKrow("SURYO") = CType(WW_UPDRow("SURYO"), Double)
            End If
            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("DAISU"))) Then               '台数(DAISU)
                WW_WKrow("DAISU") = 0
            Else
                WW_WKrow("DAISU") = CType(WW_UPDRow("DAISU"), Double)
            End If
            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("JSURYO"))) Then              '数量(JSURYO)
                WW_WKrow("JSURYO") = 0.0
            Else
                WW_WKrow("JSURYO") = CType(WW_UPDRow("JSURYO"), Double)          '配送実績数量(JSURYO)
            End If
            If String.IsNullOrWhiteSpace(RTrim(WW_UPDRow("JDAISU"))) Then              '台数(JDAISU)
                WW_WKrow("JDAISU") = 0
            Else
                WW_WKrow("JDAISU") = CType(WW_UPDRow("JDAISU"), Double)
            End If
            WW_WKrow("REMARKS1") = WW_UPDRow("REMARKS1")                         '備考１(REMARKS1)
            WW_WKrow("REMARKS2") = WW_UPDRow("REMARKS2")                         '備考２(REMARKS2)
            WW_WKrow("REMARKS3") = WW_UPDRow("REMARKS3")                         '備考３(REMARKS3)
            WW_WKrow("REMARKS4") = WW_UPDRow("REMARKS4")                         '備考４(REMARKS4)
            WW_WKrow("REMARKS5") = WW_UPDRow("REMARKS5")                         '備考５(REMARKS5)
            WW_WKrow("REMARKS6") = WW_UPDRow("REMARKS6")                         '備考６(REMARKS6)
            WW_WKrow("DELFLG") = WW_UPDRow("DELFLG")                             '削除フラグ(DELFLG)
            WW_WKrow("INITYMD") = I_DATENOW                                             '登録年月日(INITYMD)
            WW_WKrow("UPDYMD") = I_DATENOW                                              '更新年月日(UPDYMD)
            WW_WKrow("UPDUSER") = CS0050Session.USERID                '更新ユーザＩＤ(UPDUSER)
            WW_WKrow("UPDTERMID") = CS0050Session.TERMID            '更新端末(UPDTERMID)
            WW_WKrow("RECEIVEYMD") = C_DEFAULT_YMD                                      '集信日時(RECEIVEYMD)
            '基準日＝出荷日 7/11
            If WW_UPDRow("KIJUNDATE") = String.Empty Then                                        '基準日(KIJUNDATE)
                WW_WKrow("KIJUNDATE") = C_DEFAULT_YMD
            Else
                WW_WKrow("KIJUNDATE") = RTrim(WW_UPDRow("KIJUNDATE"))
            End If
            WW_WKrow("SHARYOTYPEF") = WW_UPDRow("SHARYOTYPEF")                   '統一車番(SHARYOTYPEF)
            WW_WKrow("TSHABANF") = WW_UPDRow("TSHABANF")                         '統一車番(TSHABANF)
            WW_WKrow("SHARYOTYPEB") = WW_UPDRow("SHARYOTYPEB")                   '統一車番(SHARYOTYPEB)
            WW_WKrow("TSHABANB") = WW_UPDRow("TSHABANB")                         '統一車番(TSHABANB)
            WW_WKrow("SHARYOTYPEB2") = WW_UPDRow("SHARYOTYPEB2")                 '統一車番(SHARYOTYPEB2)
            WW_WKrow("TSHABANB2") = WW_UPDRow("TSHABANB2")                       '統一車番(TSHABANB2)
            WW_WKrow("HTANI") = WW_UPDRow("HTANI")                               '配送単位(HTANI)
            WW_WKrow("STANI") = WW_UPDRow("STANI")                               '配送実績単位(STANI)
            WW_WKrow("TAXKBN") = WW_UPDRow("TAXKBN")                             '税区分(TAXKBN)

            WW_WKtbl.Rows.Add(WW_WKrow)

        Next

        Try
            'テンポラリテーブル作成
            Dim WW_TempTbl As String = String.Empty
            If I_TBLNAME = "T0003_NIORDER" Then
                WW_TempTbl = "#T3temp"
                CreateT0003TempTbl(WW_TempTbl)
            Else
                WW_TempTbl = "#T4temp"
                CreateT0004TempTbl(WW_TempTbl)
            End If

            '一旦テンポラリテーブルに出力
            Using bc As New SqlClient.SqlBulkCopy(SQLcon)
                bc.DestinationTableName = WW_TempTbl
                bc.WriteToServer(WW_WKtbl)
            End Using

            'テンポラリテーブルから日報ＤＢを出力
            InsertT003OrT004FromTempTable(I_TBLNAME, WW_TempTbl)

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:" & I_TBLNAME & " INSERT"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try
    End Sub
    ''' <summary>
    ''' テンポラリテーブルから実テーブルに登録する
    ''' </summary>
    ''' <param name="I_TBLNAME">登録テーブル</param>
    ''' <param name="I_TEMPTBLENAME">テンポラリテーブル</param>
    ''' <remarks></remarks>
    Public Sub InsertT003OrT004FromTempTable(ByVal I_TBLNAME As String, ByVal I_TEMPTBLENAME As String)

        '検索SQL文
        '〇配送受注DB登録
        Dim SQLStr As String =
               " INSERT INTO " & I_TBLNAME _
             & " ( " _
             & "  CAMPCODE ," _
             & "  TORICODE ," _
             & "  OILTYPE ," _
             & "  ORDERORG ," _
             & "  SHIPORG ," _
             & "  KIJUNDATE ," _
             & "  ORDERNO ," _
             & "  DETAILNO ," _
             & "  GSHABAN ," _
             & "  TRIPNO ," _
             & "  DROPNO ," _
             & "  SEQ ," _
             & "  SHUKODATE ," _
             & "  ENTRYDATE ," _
             & "  STATUS ," _
             & "  TUMIOKIKBN ," _
             & "  KIKODATE ," _
             & "  SHUKADATE ," _
             & "  TODOKEDATE ," _
             & "  SHUKABASHO ," _
             & "  GATE ," _
             & "  TUMIBA ," _
             & "  TUMISEQ ," _
             & "  SHUKADENNO ," _
             & "  INTIME ," _
             & "  OUTTIME ," _
             & "  STAFFCODE ," _
             & "  SUBSTAFFCODE ," _
             & "  STTIME ," _
             & "  RYOME ," _
             & "  TODOKECODE ," _
             & "  TODOKETIME ," _
             & "  PRODUCT1 ," _
             & "  PRODUCT2 ," _
             & "  PRODUCTCODE, " _
             & "  CONTNO ," _
             & "  PRATIO ," _
             & "  SMELLKBN ," _
             & "  SHAFUKU ," _
             & "  HTANI ," _
             & "  SURYO ," _
             & "  DAISU ," _
             & "  STANI ," _
             & "  JSURYO ," _
             & "  JDAISU ," _
             & "  REMARKS1 ," _
             & "  REMARKS2 ," _
             & "  REMARKS3 ," _
             & "  REMARKS4 ," _
             & "  REMARKS5 ," _
             & "  REMARKS6 ," _
             & "  TORIORDERNO ," _
             & "  STORICODE ," _
             & "  URIKBN ," _
             & "  TERMORG ," _
             & "  CONTCHASSIS ," _
             & "  SHARYOTYPEF ," _
             & "  TSHABANF ," _
             & "  SHARYOTYPEB ," _
             & "  TSHABANB ," _
             & "  SHARYOTYPEB2 ," _
             & "  TSHABANB2 ," _
             & "  TAXKBN ," _
             & "  DELFLG ," _
             & "  INITYMD ," _
             & "  UPDYMD ," _
             & "  UPDUSER ," _
             & "  UPDTERMID ," _
             & "  RECEIVEYMD " _
             & " ) " _
             & " SELECT  " _
             & "  CAMPCODE ," _
             & "  TORICODE ," _
             & "  OILTYPE ," _
             & "  ORDERORG ," _
             & "  SHIPORG ," _
             & "  KIJUNDATE ," _
             & "  ORDERNO ," _
             & "  DETAILNO ," _
             & "  GSHABAN ," _
             & "  TRIPNO ," _
             & "  DROPNO ," _
             & "  SEQ ," _
             & "  SHUKODATE ," _
             & "  ENTRYDATE ," _
             & "  STATUS ," _
             & "  TUMIOKIKBN ," _
             & "  KIKODATE ," _
             & "  SHUKADATE ," _
             & "  TODOKEDATE ," _
             & "  SHUKABASHO ," _
             & "  GATE ," _
             & "  TUMIBA ," _
             & "  TUMISEQ ," _
             & "  SHUKADENNO ," _
             & "  INTIME ," _
             & "  OUTTIME ," _
             & "  STAFFCODE ," _
             & "  SUBSTAFFCODE ," _
             & "  STTIME ," _
             & "  RYOME ," _
             & "  TODOKECODE ," _
             & "  TODOKETIME ," _
             & "  PRODUCT1 ," _
             & "  PRODUCT2 ," _
             & "  PRODUCTCODE, " _
             & "  CONTNO ," _
             & "  PRATIO ," _
             & "  SMELLKBN ," _
             & "  SHAFUKU ," _
             & "  HTANI ," _
             & "  SURYO ," _
             & "  DAISU ," _
             & "  STANI ," _
             & "  JSURYO ," _
             & "  JDAISU ," _
             & "  REMARKS1 ," _
             & "  REMARKS2 ," _
             & "  REMARKS3 ," _
             & "  REMARKS4 ," _
             & "  REMARKS5 ," _
             & "  REMARKS6 ," _
             & "  TORIORDERNO ," _
             & "  STORICODE ," _
             & "  URIKBN ," _
             & "  TERMORG ," _
             & "  CONTCHASSIS ," _
             & "  SHARYOTYPEF ," _
             & "  TSHABANF ," _
             & "  SHARYOTYPEB ," _
             & "  TSHABANB ," _
             & "  SHARYOTYPEB2 ," _
             & "  TSHABANB2 ," _
             & "  TAXKBN ," _
             & "  DELFLG ," _
             & "  INITYMD ," _
             & "  UPDYMD ," _
             & "  UPDUSER ," _
             & "  UPDTERMID ," _
             & "  RECEIVEYMD " _
             & "   FROM " & I_TEMPTBLENAME & ";"

        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

        'CLOSE
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub

    ''' <summary>
    ''' T0003用テンポラリテーブル作成
    ''' </summary>
    ''' <param name="I_TBLNAME"></param>
    ''' <remarks></remarks>
    Public Sub CreateT0003TempTbl(ByVal I_TBLNAME As String)

        Dim SQLStr As String = String.Empty

        'テンポラリーテーブルを作成する
        SQLStr = "CREATE TABLE " & I_TBLNAME _
             & " ( " _
             & "  CAMPCODE nvarchar(20)," _
             & "  TORICODE nvarchar(20)," _
             & "  OILTYPE nvarchar(20)," _
             & "  ORDERORG nvarchar(20)," _
             & "  SHIPORG nvarchar(20)," _
             & "  KIJUNDATE date," _
             & "  ORDERNO nvarchar(10)," _
             & "  DETAILNO nvarchar(10)," _
             & "  ENTRYDATE nvarchar(25)," _
             & "  GSHABAN nvarchar(20)," _
             & "  TRIPNO nvarchar(10)," _
             & "  DROPNO nvarchar(10)," _
             & "  SEQ nvarchar(2)," _
             & "  STATUS nvarchar(1)," _
             & "  TUMIOKIKBN nvarchar(1)," _
             & "  SHUKODATE date," _
             & "  KIKODATE date," _
             & "  SHUKADATE date," _
             & "  TODOKEDATE date," _
             & "  SHUKABASHO nvarchar(20)," _
             & "  GATE nvarchar(20)," _
             & "  TUMIBA nvarchar(20)," _
             & "  TUMISEQ nvarchar(20)," _
             & "  SHUKADENNO nvarchar(20)," _
             & "  INTIME nvarchar(10)," _
             & "  OUTTIME nvarchar(10)," _
             & "  STAFFCODE nvarchar(20)," _
             & "  SUBSTAFFCODE nvarchar(20)," _
             & "  STTIME nvarchar(10)," _
             & "  RYOME nvarchar(1)," _
             & "  TODOKECODE nvarchar(20)," _
             & "  TODOKETIME nvarchar(10)," _
             & "  PRODUCT1 nvarchar(20)," _
             & "  PRODUCT2 nvarchar(20)," _
             & "  PRODUCTCODE nvarchar(30)," _
             & "  CONTNO nvarchar(20)," _
             & "  PRATIO numeric(4, 1)," _
             & "  SMELLKBN nvarchar(1)," _
             & "  SHAFUKU numeric(6, 3)," _
             & "  HTANI nvarchar(10)," _
             & "  SURYO numeric(10, 3)," _
             & "  DAISU numeric(3, 0)," _
             & "  STANI nvarchar(10)," _
             & "  JSURYO numeric(10, 3)," _
             & "  JDAISU numeric(3, 0)," _
             & "  REMARKS1 nvarchar(50)," _
             & "  REMARKS2 nvarchar(50)," _
             & "  REMARKS3 nvarchar(50)," _
             & "  REMARKS4 nvarchar(50)," _
             & "  REMARKS5 nvarchar(50)," _
             & "  REMARKS6 nvarchar(50)," _
             & "  TORIORDERNO nvarchar(20)," _
             & "  STORICODE nvarchar(20)," _
             & "  URIKBN nvarchar(1)," _
             & "  TERMORG nvarchar(15)," _
             & "  CONTCHASSIS nvarchar(20)," _
             & "  SHARYOTYPEF nvarchar(1)," _
             & "  TSHABANF nvarchar(20)," _
             & "  SHARYOTYPEB nvarchar(1)," _
             & "  TSHABANB nvarchar(20)," _
             & "  SHARYOTYPEB2 nvarchar(1)," _
             & "  TSHABANB2 nvarchar(20)," _
             & "  TAXKBN nvarchar(10)," _
             & "  DELFLG nvarchar(1)," _
             & "  INITYMD smalldatetime," _
             & "  UPDYMD datetime," _
             & "  UPDUSER nvarchar(20)," _
             & "  UPDTERMID nvarchar(30)," _
             & "  RECEIVEYMD datetime" _
             & " ) "

        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        SQLcmd.ExecuteNonQuery()
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub

    ''' <summary>
    ''' T0004用テンポラリテーブル作成
    ''' </summary>
    ''' <param name="I_TBLNAME"></param>
    ''' <remarks></remarks>
    Public Sub CreateT0004TempTbl(ByVal I_TBLNAME As String)

        Dim SQLStr As String = String.Empty

        'テンポラリーテーブルを作成する
        SQLStr = "CREATE TABLE " & I_TBLNAME _
             & " ( " _
             & "  CAMPCODE nvarchar(20)," _
             & "  TORICODE nvarchar(20)," _
             & "  OILTYPE nvarchar(20)," _
             & "  ORDERORG nvarchar(20)," _
             & "  SHIPORG nvarchar(20)," _
             & "  KIJUNDATE date," _
             & "  ORDERNO nvarchar(10)," _
             & "  DETAILNO nvarchar(10)," _
             & "  GSHABAN nvarchar(20)," _
             & "  TRIPNO nvarchar(10)," _
             & "  DROPNO nvarchar(10)," _
             & "  SEQ nvarchar(2)," _
             & "  SHUKODATE date," _
             & "  ENTRYDATE nvarchar(25)," _
             & "  STATUS nvarchar(1)," _
             & "  TUMIOKIKBN nvarchar(1)," _
             & "  KIKODATE date," _
             & "  SHUKADATE date," _
             & "  TODOKEDATE date," _
             & "  SHUKABASHO nvarchar(20)," _
             & "  GATE nvarchar(20)," _
             & "  TUMIBA nvarchar(20)," _
             & "  TUMISEQ nvarchar(20)," _
             & "  SHUKADENNO nvarchar(20)," _
             & "  INTIME nvarchar(10)," _
             & "  OUTTIME nvarchar(10)," _
             & "  STAFFCODE nvarchar(20)," _
             & "  SUBSTAFFCODE nvarchar(20)," _
             & "  STTIME nvarchar(10)," _
             & "  RYOME nvarchar(1)," _
             & "  TODOKECODE nvarchar(20)," _
             & "  TODOKETIME nvarchar(10)," _
             & "  PRODUCT1 nvarchar(20)," _
             & "  PRODUCT2 nvarchar(20)," _
             & "  PRODUCTCODE nvarchar(30)," _
             & "  CONTNO nvarchar(20)," _
             & "  PRATIO numeric(4, 1)," _
             & "  SMELLKBN nvarchar(1)," _
             & "  SHAFUKU numeric(6, 3)," _
             & "  HTANI nvarchar(10)," _
             & "  SURYO numeric(10, 3)," _
             & "  DAISU numeric(3, 0)," _
             & "  STANI nvarchar(10)," _
             & "  JSURYO numeric(10, 3)," _
             & "  JDAISU numeric(3, 0)," _
             & "  REMARKS1 nvarchar(50)," _
             & "  REMARKS2 nvarchar(50)," _
             & "  REMARKS3 nvarchar(50)," _
             & "  REMARKS4 nvarchar(50)," _
             & "  REMARKS5 nvarchar(50)," _
             & "  REMARKS6 nvarchar(50)," _
             & "  TORIORDERNO nvarchar(20)," _
             & "  STORICODE nvarchar(20)," _
             & "  URIKBN nvarchar(1)," _
             & "  TERMORG nvarchar(15)," _
             & "  CONTCHASSIS nvarchar(20)," _
             & "  SHARYOTYPEF nvarchar(1)," _
             & "  TSHABANF nvarchar(20)," _
             & "  SHARYOTYPEB nvarchar(1)," _
             & "  TSHABANB nvarchar(20)," _
             & "  SHARYOTYPEB2 nvarchar(1)," _
             & "  TSHABANB2 nvarchar(20)," _
             & "  TAXKBN nvarchar(10)," _
             & "  DELFLG nvarchar(1)," _
             & "  INITYMD smalldatetime," _
             & "  UPDYMD datetime," _
             & "  UPDUSER nvarchar(20)," _
             & "  UPDTERMID nvarchar(30)," _
             & "  RECEIVEYMD datetime," _
             & " ) "

        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        SQLcmd.ExecuteNonQuery()
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub

End Class

''' <summary>
''' 日報ＤＢ更新
''' </summary>
''' <remarks></remarks>
Public Class GRT0005UPDATE

    '統計DB出力dll Interface
    ''' <summary>
    ''' DB接続情報
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <returns></returns>
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    ''' <summary>
    ''' 日報情報テーブル
    ''' </summary>
    ''' <returns></returns>
    Public Property T0005tbl As DataTable                                     '日報テーブル
    ''' <summary>
    ''' 光英ファイル一覧（分割後）
    ''' </summary>
    ''' <returns></returns>
    Public Property KOUEIFILES As ListBox                                     '光英ファイル（分割後）
    ''' <summary>
    ''' 登録日付
    ''' </summary>
    ''' <returns></returns>
    Public Property ENTRYDATE As Date                                         'エントリー日付
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <returns></returns>
    Public Property CAMPCODE As String                                        '会社コード
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <returns></returns>
    Public Property SORG As String                                            '部署コード
    ''' <summary>
    ''' 更新ユーザID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDUSERID As String                                       '更新ユーザID
    ''' <summary>
    ''' 更新端末ID
    ''' </summary>
    ''' <returns></returns>
    Public Property UPDTERMID As String                                       '更新端末ID
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property ERR As String                                             'リターン値
    ''' <summary>
    ''' ログ出力クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                              'LogOutput DirString Get
    ''' <summary>
    ''' ジャーナル出力クラス
    ''' </summary>
    Private CS0020JOURNAL As New CS0020JOURNAL                                'Journal Out
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TblSort As New CS0026TBLSORT                                'テーブルソート
    ''' <summary>
    ''' 従業員マスタ取得クラス
    ''' </summary>
    Private CS0043STAFFORGget As New CS0043STAFFORGget                        '従業員マスタ取得
    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private CS0050Session As New CS0050SESSION                                'セッション管理

    ''' <summary>
    ''' 日報更新処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Update()

        Dim WW_T5tblEx As DataTable = New DataTable
        Dim WW_T5rowEx As DataRow = Nothing
        Try
            ERR = C_MESSAGE_NO.NORMAL

            '更新SQL文･･･マスタへ更新
            Dim WW_T0005UPDtbl As New DataTable
            Dim WW_RTN As String = C_MESSAGE_NO.NORMAL

            '----------------------------------------------------------------------------------------------------
            '日報ＤＢ追加
            '----------------------------------------------------------------------------------------------------
            CS0026TblSort.TABLE = T0005tbl
            CS0026TblSort.FILTER = String.Empty
            CS0026TblSort.SORTING = "YMD, STAFFCODE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            T0005tbl = CS0026TblSort.sort()
            '念のため、荷卸から直前の荷積に受注番号、トリップ番号を再度設定する
            For i As Integer = 0 To T0005tbl.Rows.Count - 1
                Dim T0005row_i As DataRow = T0005tbl.Rows(i)

                If T0005row_i("WORKKBN") = "B3" AndAlso T0005row_i("SELECT") = "1" Then
                    For j As Integer = i - 1 To 0 Step -1
                        Dim T0005row_j As DataRow = T0005tbl.Rows(j)
                        If T0005row_j("YMD") = T0005row_i("YMD") AndAlso
                           T0005row_j("STAFFCODE") = T0005row_i("STAFFCODE") AndAlso
                           T0005row_j("NIPPONO") = T0005row_i("NIPPONO") Then
                            If T0005row_j("WORKKBN") = "B2" Then
                                T0005row_j("ORDERNO") = T0005row_i("ORDERNO")
                                T0005row_j("TRIPNO") = T0005row_i("TRIPNO")
                                Exit For
                            End If
                        Else
                            Exit For
                        End If
                    Next
                End If
            Next

            Dim WW_T0005tbl As DataTable = New DataTable
            AddColumnT0005UPDtbl(WW_T0005tbl)

            '----------------------------------------------------------------------------------------------------
            '日報、勤怠ＤＢ削除
            '----------------------------------------------------------------------------------------------------
            '更新対象は、全て削除
            CS0026TblSort.TABLE = T0005tbl
            CS0026TblSort.FILTER = "OPERATION = '更新' and SELECT = '1'"
            CS0026TblSort.SORTING = "SELECT, YMD, STAFFCODE, HDKBN DESC, CREWKBN, STDATE, STTIME, ENDDATE, ENDTIME, WORKKBN"
            WW_T0005UPDtbl = CS0026TblSort.sort()

            WW_T5tblEx = WW_T0005UPDtbl.Clone
            WW_T5rowEx = WW_T5tblEx.NewRow
            For Each WW_T5row As DataRow In WW_T0005UPDtbl.Rows
                WW_T5rowEx.ItemArray = WW_T5row.ItemArray
                If WW_T5row("HDKBN") = "H" Then
                    '日報ＤＢ削除処理
                    DeleteT0005(WW_T5row, ENTRYDATE, WW_RTN)
                    If Not isNormal(WW_RTN) Then
                        'SQLtrn.Rollback()
                        ERR = WW_RTN
                        Exit Sub
                    End If
                End If

                If WW_T5row("HDKBN") = "D" AndAlso WW_T5row("DELFLG") = C_DELETE_FLG.ALIVE Then
                    '日報ＤＢ編集
                    Dim WW_oRow As DataRow = Nothing
                    EditT0005(WW_T5row, ENTRYDATE, WW_T0005tbl, WW_oRow, WW_RTN)
                    InsertT0005(WW_oRow)

                    '光英受信ファイル削除（更新OKの場合のみ）
                    If Not IsNothing(KOUEIFILES) Then
                        For Each file As ListItem In KOUEIFILES.Items
                            Dim f = New System.IO.FileInfo(file.Value)
                            Dim WW_KEYWORD As String = Replace(WW_T5row("YMD"), "/", "") & "_" & WW_T5row("NIPPONO")
                            If f.FullName.IndexOf(WW_KEYWORD) > 0 AndAlso f.FullName.ToLower.EndsWith(".csv") Then
                                If f.Exists Then
                                    '光英連携が安定稼働するまでは論理削除
                                    Dim bakFileName As New System.IO.FileInfo(f.FullName & ".used")
                                    If bakFileName.Exists Then
                                        bakFileName.Delete()
                                    End If
                                    f.MoveTo(bakFileName.FullName)
                                End If
                            End If
                        Next
                    End If
                End If
            Next

            WW_T0005UPDtbl.Dispose()
            WW_T0005UPDtbl = Nothing
            WW_T0005tbl.Dispose()
            WW_T0005tbl = Nothing

        Catch ex As Exception
            'SQLtrn.Rollback()
            ERR = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.INFSUBCLASS = "CS0050T0005UPDATE"            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "例外発生"                         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            Dim WW As String = ""
            Try
                For i As Integer = 0 To WW_T5rowEx.ItemArray.Length - 1
                    WW = WW & WW_T5rowEx.ItemArray(i).ToString
                Next
            Catch ex2 As Exception
            End Try
            CS0011LOGWRITE.TEXT = ex.ToString() & "Data='" & WW & "'"
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' T0005テンポラリーテーブル作成
    ''' </summary>
    ''' <param name="I_TBLNAME"></param>
    ''' <remarks></remarks>
    Public Sub CreateT0005TempTbl(ByVal I_TBLNAME As String)

        Dim SQLStr As String = String.Empty

        'テンポラリーテーブルを作成する
        SQLStr = "CREATE TABLE " & I_TBLNAME _
                & " ( " _
                & "  CAMPCODE nvarchar(20)," _
                & "  SHIPORG nvarchar(15)," _
                & "  TERMKBN nvarchar(1)," _
                & "  YMD date," _
                & "  STAFFCODE nvarchar(20)," _
                & "  SEQ int," _
                & "  ENTRYDATE nvarchar(14)," _
                & "  CREWKBN nvarchar(1)," _
                & "  NIPPONO nvarchar(20)," _
                & "  WORKKBN nvarchar(2)," _
                & "  GSHABAN nvarchar(20)," _
                & "  SUBSTAFFCODE nvarchar(20)," _
                & "  STDATE date," _
                & "  STTIME time," _
                & "  ENDDATE date," _
                & "  ENDTIME time," _
                & "  WORKTIME int," _
                & "  MOVETIME int," _
                & "  ACTTIME int," _
                & "  PRATE int," _
                & "  CASH int," _
                & "  TICKET int," _
                & "  ETC int," _
                & "  TOTALTOLL int," _
                & "  STMATER numeric(9, 2)," _
                & "  ENDMATER numeric(9, 2)," _
                & "  RUIDISTANCE numeric(9, 2)," _
                & "  SOUDISTANCE numeric(7, 2)," _
                & "  JIDISTANCE numeric(7, 2)," _
                & "  KUDISTANCE numeric(7, 2)," _
                & "  IPPDISTANCE numeric(7, 2)," _
                & "  KOSDISTANCE numeric(7, 2)," _
                & "  IPPJIDISTANCE numeric(7, 2)," _
                & "  IPPKUDISTANCE numeric(7, 2)," _
                & "  KOSJIDISTANCE numeric(7, 2)," _
                & "  KOSKUDISTANCE numeric(7, 2)," _
                & "  KYUYU numeric(5, 2)," _
                & "  TORICODE nvarchar(20)," _
                & "  SHUKABASHO nvarchar(20)," _
                & "  SHUKADATE date," _
                & "  TODOKECODE nvarchar(20)," _
                & "  TODOKEDATE date," _
                & "  OILTYPE1 nvarchar(20)," _
                & "  PRODUCT11 nvarchar(20)," _
                & "  PRODUCT21 nvarchar(20)," _
                & "  PRODUCTCODE1 nvarchar(30)," _
                & "  STANI1 nvarchar(10)," _
                & "  SURYO1 numeric(6, 3)," _
                & "  OILTYPE2 nvarchar(20)," _
                & "  PRODUCT12 nvarchar(20)," _
                & "  PRODUCT22 nvarchar(20)," _
                & "  PRODUCTCODE2 nvarchar(30)," _
                & "  STANI2 nvarchar(10)," _
                & "  SURYO2 numeric(6, 3)," _
                & "  OILTYPE3 nvarchar(20)," _
                & "  PRODUCT13 nvarchar(20)," _
                & "  PRODUCT23 nvarchar(20)," _
                & "  PRODUCTCODE3 nvarchar(30)," _
                & "  STANI3 nvarchar(10)," _
                & "  SURYO3 numeric(6, 3)," _
                & "  OILTYPE4 nvarchar(20)," _
                & "  PRODUCT14 nvarchar(20)," _
                & "  PRODUCT24 nvarchar(20)," _
                & "  PRODUCTCODE4 nvarchar(30)," _
                & "  STANI4 nvarchar(10)," _
                & "  SURYO4 numeric(6, 3)," _
                & "  OILTYPE5 nvarchar(20)," _
                & "  PRODUCT15 nvarchar(20)," _
                & "  PRODUCT25 nvarchar(20)," _
                & "  PRODUCTCODE5 nvarchar(30)," _
                & "  STANI5 nvarchar(10)," _
                & "  SURYO5 numeric(6, 3)," _
                & "  OILTYPE6 nvarchar(20)," _
                & "  PRODUCT16 nvarchar(20)," _
                & "  PRODUCT26 nvarchar(20)," _
                & "  PRODUCTCODE6 nvarchar(30)," _
                & "  STANI6 nvarchar(10)," _
                & "  SURYO6 numeric(6, 3)," _
                & "  OILTYPE7 nvarchar(20)," _
                & "  PRODUCT17 nvarchar(20)," _
                & "  PRODUCT27 nvarchar(20)," _
                & "  PRODUCTCODE7 nvarchar(30)," _
                & "  STANI7 nvarchar(10)," _
                & "  SURYO7 numeric(6, 3)," _
                & "  OILTYPE8 nvarchar(20)," _
                & "  PRODUCT18 nvarchar(20)," _
                & "  PRODUCT28 nvarchar(20)," _
                & "  PRODUCTCODE8 nvarchar(30)," _
                & "  STANI8 nvarchar(10)," _
                & "  SURYO8 numeric(6, 3)," _
                & "  TOTALSURYO numeric(7, 3)," _
                & "  TUMIOKIKBN nvarchar(1)," _
                & "  ORDERNO nvarchar(10)," _
                & "  DETAILNO nvarchar(10)," _
                & "  TRIPNO nvarchar(10)," _
                & "  DROPNO nvarchar(10)," _
                & "  JISSKIKBN nvarchar(1)," _
                & "  URIKBN nvarchar(1)," _
                & "  STORICODE nvarchar(20)," _
                & "  CONTCHASSIS nvarchar(20)," _
                & "  SHARYOTYPEF nvarchar(1)," _
                & "  TSHABANF nvarchar(20)," _
                & "  SHARYOTYPEB nvarchar(1)," _
                & "  TSHABANB nvarchar(20)," _
                & "  SHARYOTYPEB2 nvarchar(1)," _
                & "  TSHABANB2 nvarchar(20)," _
                & "  TAXKBN nvarchar(10)," _
                & "  LATITUDE nvarchar(20)," _
                & "  LONGITUDE nvarchar(20)," _
                & "  L1SHUKODATE date," _
                & "  L1SHUKADATE date," _
                & "  L1TODOKEDATE date," _
                & "  L1TRIPNO nvarchar(10)," _
                & "  L1DROPNO nvarchar(10)," _
                & "  L1TORICODE nvarchar(20)," _
                & "  L1URIKBN nvarchar(1)," _
                & "  L1STORICODE nvarchar(20)," _
                & "  L1TODOKECODE nvarchar(20)," _
                & "  L1SHUKABASHO nvarchar(20)," _
                & "  L1CREWKBN nvarchar(1)," _
                & "  L1STAFFKBN nvarchar(5)," _
                & "  L1STAFFCODE nvarchar(20)," _
                & "  L1SUBSTAFFCODE nvarchar(20)," _
                & "  L1ORDERNO nvarchar(10)," _
                & "  L1DETAILNO nvarchar(10)," _
                & "  L1ORDERORG nvarchar(20)," _
                & "  L1KAISO nvarchar(20)," _
                & "  L1KUSHAKBN nvarchar(20)," _
                & "  L1IPPDISTANCE numeric(7, 2)," _
                & "  L1KOSDISTANCE numeric(7, 2)," _
                & "  L1IPPJIDISTANCE numeric(7, 2)," _
                & "  L1IPPKUDISTANCE numeric(7, 2)," _
                & "  L1KOSJIDISTANCE numeric(7, 2)," _
                & "  L1KOSKUDISTANCE numeric(7, 2)," _
                & "  L1WORKTIME int," _
                & "  L1MOVETIME int," _
                & "  L1ACTTIME int," _
                & "  L1JIMOVETIME int," _
                & "  L1KUMOVETIME int," _
                & "  DELFLG nvarchar(1)," _
                & "  INITYMD smalldatetime," _
                & "  UPDYMD datetime," _
                & "  UPDUSER nvarchar(20)," _
                & "  UPDTERMID nvarchar(30)," _
                & "  RECEIVEYMD datetime" _
                & " ) "

        Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub
    ''' <summary>
    ''' 日報テーブル登録処理
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <remarks></remarks>
    Public Sub InsertT0005(ByVal I_ROW As DataRow)

        '検索SQL文
        '〇配送受注DB登録
        Dim SQLStr As String =
                  " INSERT INTO T0005_NIPPO " _
                & " ( " _
                & "        CAMPCODE," _
                & "        SHIPORG," _
                & "        TERMKBN," _
                & "        YMD," _
                & "        STAFFCODE," _
                & "        SEQ," _
                & "        ENTRYDATE," _
                & "        CREWKBN," _
                & "        NIPPONO," _
                & "        WORKKBN," _
                & "        GSHABAN," _
                & "        SUBSTAFFCODE," _
                & "        STDATE," _
                & "        STTIME," _
                & "        ENDDATE," _
                & "        ENDTIME," _
                & "        WORKTIME," _
                & "        MOVETIME," _
                & "        ACTTIME," _
                & "        PRATE," _
                & "        CASH," _
                & "        TICKET," _
                & "        ETC," _
                & "        TOTALTOLL," _
                & "        STMATER," _
                & "        ENDMATER," _
                & "        RUIDISTANCE," _
                & "        SOUDISTANCE," _
                & "        JIDISTANCE," _
                & "        KUDISTANCE," _
                & "        IPPDISTANCE," _
                & "        KOSDISTANCE," _
                & "        IPPJIDISTANCE," _
                & "        IPPKUDISTANCE," _
                & "        KOSJIDISTANCE," _
                & "        KOSKUDISTANCE," _
                & "        KYUYU," _
                & "        TORICODE," _
                & "        SHUKABASHO," _
                & "        SHUKADATE," _
                & "        TODOKECODE," _
                & "        TODOKEDATE," _
                & "        OILTYPE1," _
                & "        PRODUCT11," _
                & "        PRODUCT21," _
                & "        PRODUCTCODE1," _
                & "        STANI1," _
                & "        SURYO1," _
                & "        OILTYPE2," _
                & "        PRODUCT12," _
                & "        PRODUCT22," _
                & "        PRODUCTCODE2," _
                & "        STANI2," _
                & "        SURYO2," _
                & "        OILTYPE3," _
                & "        PRODUCT13," _
                & "        PRODUCT23," _
                & "        PRODUCTCODE3," _
                & "        STANI3," _
                & "        SURYO3," _
                & "        OILTYPE4," _
                & "        PRODUCT14," _
                & "        PRODUCT24," _
                & "        PRODUCTCODE4," _
                & "        STANI4," _
                & "        SURYO4," _
                & "        OILTYPE5," _
                & "        PRODUCT15," _
                & "        PRODUCT25," _
                & "        PRODUCTCODE5," _
                & "        STANI5," _
                & "        SURYO5," _
                & "        OILTYPE6," _
                & "        PRODUCT16," _
                & "        PRODUCT26," _
                & "        PRODUCTCODE6," _
                & "        STANI6," _
                & "        SURYO6," _
                & "        OILTYPE7," _
                & "        PRODUCT17," _
                & "        PRODUCT27," _
                & "        PRODUCTCODE7," _
                & "        STANI7," _
                & "        SURYO7," _
                & "        OILTYPE8," _
                & "        PRODUCTCODE8," _
                & "        PRODUCT18," _
                & "        PRODUCT28," _
                & "        STANI8," _
                & "        SURYO8," _
                & "        TOTALSURYO," _
                & "        TUMIOKIKBN," _
                & "        ORDERNO," _
                & "        DETAILNO," _
                & "        TRIPNO," _
                & "        DROPNO," _
                & "        JISSKIKBN," _
                & "        URIKBN," _
                & "        STORICODE," _
                & "        CONTCHASSIS," _
                & "        SHARYOTYPEF," _
                & "        TSHABANF," _
                & "        SHARYOTYPEB," _
                & "        TSHABANB," _
                & "        SHARYOTYPEB2," _
                & "        TSHABANB2," _
                & "        TAXKBN," _
                & "        LATITUDE," _
                & "        LONGITUDE," _
                & "        L1SHUKODATE," _
                & "        L1SHUKADATE," _
                & "        L1TODOKEDATE," _
                & "        L1TRIPNO," _
                & "        L1DROPNO," _
                & "        L1TORICODE," _
                & "        L1URIKBN," _
                & "        L1STORICODE," _
                & "        L1TODOKECODE," _
                & "        L1SHUKABASHO," _
                & "        L1CREWKBN," _
                & "        L1STAFFKBN," _
                & "        L1STAFFCODE," _
                & "        L1SUBSTAFFCODE," _
                & "        L1ORDERNO," _
                & "        L1DETAILNO," _
                & "        L1ORDERORG," _
                & "        L1KAISO," _
                & "        L1KUSHAKBN," _
                & "        L1IPPDISTANCE," _
                & "        L1KOSDISTANCE," _
                & "        L1IPPJIDISTANCE," _
                & "        L1IPPKUDISTANCE," _
                & "        L1KOSJIDISTANCE," _
                & "        L1KOSKUDISTANCE," _
                & "        L1WORKTIME," _
                & "        L1MOVETIME," _
                & "        L1ACTTIME," _
                & "        L1JIMOVETIME," _
                & "        L1KUMOVETIME," _
                & "        L1HAISOGROUP," _
                & "        DELFLG," _
                & "        INITYMD," _
                & "        UPDYMD," _
                & "        UPDUSER," _
                & "        UPDTERMID," _
                & "        RECEIVEYMD " _
                & " ) " _
                & " VALUES(  " _
                & "        @CAMPCODE," _
                & "        @SHIPORG," _
                & "        @TERMKBN," _
                & "        @YMD," _
                & "        @STAFFCODE," _
                & "        @SEQ," _
                & "        @ENTRYDATE," _
                & "        @CREWKBN," _
                & "        @NIPPONO," _
                & "        @WORKKBN," _
                & "        @GSHABAN," _
                & "        @SUBSTAFFCODE," _
                & "        @STDATE," _
                & "        @STTIME," _
                & "        @ENDDATE," _
                & "        @ENDTIME," _
                & "        @WORKTIME," _
                & "        @MOVETIME," _
                & "        @ACTTIME," _
                & "        @PRATE," _
                & "        @CASH," _
                & "        @TICKET," _
                & "        @ETC," _
                & "        @TOTALTOLL," _
                & "        @STMATER," _
                & "        @ENDMATER," _
                & "        @RUIDISTANCE," _
                & "        @SOUDISTANCE," _
                & "        @JIDISTANCE," _
                & "        @KUDISTANCE," _
                & "        @IPPDISTANCE," _
                & "        @KOSDISTANCE," _
                & "        @IPPJIDISTANCE," _
                & "        @IPPKUDISTANCE," _
                & "        @KOSJIDISTANCE," _
                & "        @KOSKUDISTANCE," _
                & "        @KYUYU," _
                & "        @TORICODE," _
                & "        @SHUKABASHO," _
                & "        @SHUKADATE," _
                & "        @TODOKECODE," _
                & "        @TODOKEDATE," _
                & "        @OILTYPE1," _
                & "        @PRODUCT11," _
                & "        @PRODUCT21," _
                & "        @PRODUCTCODE1," _
                & "        @STANI1," _
                & "        @SURYO1," _
                & "        @OILTYPE2," _
                & "        @PRODUCT12," _
                & "        @PRODUCT22," _
                & "        @PRODUCTCODE2," _
                & "        @STANI2," _
                & "        @SURYO2," _
                & "        @OILTYPE3," _
                & "        @PRODUCT13," _
                & "        @PRODUCT23," _
                & "        @PRODUCTCODE3," _
                & "        @STANI3," _
                & "        @SURYO3," _
                & "        @OILTYPE4," _
                & "        @PRODUCT14," _
                & "        @PRODUCT24," _
                & "        @PRODUCTCODE4," _
                & "        @STANI4," _
                & "        @SURYO4," _
                & "        @OILTYPE5," _
                & "        @PRODUCT15," _
                & "        @PRODUCT25," _
                & "        @PRODUCTCODE5," _
                & "        @STANI5," _
                & "        @SURYO5," _
                & "        @OILTYPE6," _
                & "        @PRODUCT16," _
                & "        @PRODUCT26," _
                & "        @PRODUCTCODE6," _
                & "        @STANI6," _
                & "        @SURYO6," _
                & "        @OILTYPE7," _
                & "        @PRODUCT17," _
                & "        @PRODUCT27," _
                & "        @PRODUCTCODE7," _
                & "        @STANI7," _
                & "        @SURYO7," _
                & "        @OILTYPE8," _
                & "        @PRODUCT18," _
                & "        @PRODUCT28," _
                & "        @PRODUCTCODE8," _
                & "        @STANI8," _
                & "        @SURYO8," _
                & "        @TOTALSURYO," _
                & "        @TUMIOKIKBN," _
                & "        @ORDERNO," _
                & "        @DETAILNO," _
                & "        @TRIPNO," _
                & "        @DROPNO," _
                & "        @JISSKIKBN," _
                & "        @URIKBN," _
                & "        @STORICODE," _
                & "        @CONTCHASSIS," _
                & "        @SHARYOTYPEF," _
                & "        @TSHABANF," _
                & "        @SHARYOTYPEB," _
                & "        @TSHABANB," _
                & "        @SHARYOTYPEB2," _
                & "        @TSHABANB2," _
                & "        @TAXKBN," _
                & "        @LATITUDE," _
                & "        @LONGITUDE," _
                & "        @L1SHUKODATE," _
                & "        @L1SHUKADATE," _
                & "        @L1TODOKEDATE," _
                & "        @L1TRIPNO," _
                & "        @L1DROPNO," _
                & "        @L1TORICODE," _
                & "        @L1URIKBN," _
                & "        @L1STORICODE," _
                & "        @L1TODOKECODE," _
                & "        @L1SHUKABASHO," _
                & "        @L1CREWKBN," _
                & "        @L1STAFFKBN," _
                & "        @L1STAFFCODE," _
                & "        @L1SUBSTAFFCODE," _
                & "        @L1ORDERNO," _
                & "        @L1DETAILNO," _
                & "        @L1ORDERORG," _
                & "        @L1KAISO," _
                & "        @L1KUSHAKBN," _
                & "        @L1IPPDISTANCE," _
                & "        @L1KOSDISTANCE," _
                & "        @L1IPPJIDISTANCE," _
                & "        @L1IPPKUDISTANCE," _
                & "        @L1KOSJIDISTANCE," _
                & "        @L1KOSKUDISTANCE," _
                & "        @L1WORKTIME," _
                & "        @L1MOVETIME," _
                & "        @L1ACTTIME," _
                & "        @L1JIMOVETIME," _
                & "        @L1KUMOVETIME," _
                & "        @L1HAISOGROUP," _
                & "        @DELFLG," _
                & "        @INITYMD," _
                & "        @UPDYMD," _
                & "        @UPDUSER," _
                & "        @UPDTERMID," _
                & "        @RECEIVEYMD); "

        Using SQLcmd As New SqlCommand(SQLStr, SQLcon, SQLtrn)
            Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SHIPORG As SqlParameter = SQLcmd.Parameters.Add("@SHIPORG", System.Data.SqlDbType.NVarChar, 15)
            Dim P_TERMKBN As SqlParameter = SQLcmd.Parameters.Add("@TERMKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_YMD As SqlParameter = SQLcmd.Parameters.Add("@YMD", System.Data.SqlDbType.Date)
            Dim P_STAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SEQ As SqlParameter = SQLcmd.Parameters.Add("@SEQ", System.Data.SqlDbType.Int)
            Dim P_ENTRYDATE As SqlParameter = SQLcmd.Parameters.Add("@ENTRYDATE", System.Data.SqlDbType.NVarChar, 25)
            Dim P_CREWKBN As SqlParameter = SQLcmd.Parameters.Add("@CREWKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_NIPPONO As SqlParameter = SQLcmd.Parameters.Add("@NIPPONO", System.Data.SqlDbType.NVarChar, 20)
            Dim P_WORKKBN As SqlParameter = SQLcmd.Parameters.Add("@WORKKBN", System.Data.SqlDbType.NVarChar, 2)
            Dim P_GSHABAN As SqlParameter = SQLcmd.Parameters.Add("@GSHABAN", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SUBSTAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@SUBSTAFFCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_STDATE As SqlParameter = SQLcmd.Parameters.Add("@STDATE", System.Data.SqlDbType.Date)
            Dim P_STTIME As SqlParameter = SQLcmd.Parameters.Add("@STTIME", System.Data.SqlDbType.Time)
            Dim P_ENDDATE As SqlParameter = SQLcmd.Parameters.Add("@ENDDATE", System.Data.SqlDbType.Date)
            Dim P_ENDTIME As SqlParameter = SQLcmd.Parameters.Add("@ENDTIME", System.Data.SqlDbType.Time)
            Dim P_WORKTIME As SqlParameter = SQLcmd.Parameters.Add("@WORKTIME", System.Data.SqlDbType.Int)
            Dim P_MOVETIME As SqlParameter = SQLcmd.Parameters.Add("@MOVETIME", System.Data.SqlDbType.Int)
            Dim P_ACTTIME As SqlParameter = SQLcmd.Parameters.Add("@ACTTIME", System.Data.SqlDbType.Int)
            Dim P_PRATE As SqlParameter = SQLcmd.Parameters.Add("@PRATE", System.Data.SqlDbType.Int)
            Dim P_CASH As SqlParameter = SQLcmd.Parameters.Add("@CASH", System.Data.SqlDbType.Int)
            Dim P_TICKET As SqlParameter = SQLcmd.Parameters.Add("@TICKET", System.Data.SqlDbType.Int)
            Dim P_ETC As SqlParameter = SQLcmd.Parameters.Add("@ETC", System.Data.SqlDbType.Int)
            Dim P_TOTALTOLL As SqlParameter = SQLcmd.Parameters.Add("@TOTALTOLL", System.Data.SqlDbType.Int)
            Dim P_STMATER As SqlParameter = SQLcmd.Parameters.Add("@STMATER", System.Data.SqlDbType.Decimal)
            Dim P_ENDMATER As SqlParameter = SQLcmd.Parameters.Add("@ENDMATER", System.Data.SqlDbType.Decimal)
            Dim P_RUIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@RUIDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_SOUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@SOUDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_JIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@JIDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_KUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@KUDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_IPPDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@IPPDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_KOSDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@KOSDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_IPPJIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@IPPJIDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_IPPKUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@IPPKUDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_KOSJIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@KOSJIDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_KOSKUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@KOSKUDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_KYUYU As SqlParameter = SQLcmd.Parameters.Add("@KYUYU", System.Data.SqlDbType.Decimal)
            Dim P_TORICODE As SqlParameter = SQLcmd.Parameters.Add("@TORICODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SHUKABASHO As SqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SHUKADATE As SqlParameter = SQLcmd.Parameters.Add("@SHUKADATE", System.Data.SqlDbType.Date)
            Dim P_TODOKECODE As SqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_TODOKEDATE As SqlParameter = SQLcmd.Parameters.Add("@TODOKEDATE", System.Data.SqlDbType.Date)
            Dim P_OILTYPE1 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE1", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT11 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT11", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT21 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT21", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE1 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE1", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI1 As SqlParameter = SQLcmd.Parameters.Add("@STANI1", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO1 As SqlParameter = SQLcmd.Parameters.Add("@SURYO1", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE2 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE2", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT12 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT12", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT22 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT22", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE2 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE2", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI2 As SqlParameter = SQLcmd.Parameters.Add("@STANI2", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO2 As SqlParameter = SQLcmd.Parameters.Add("@SURYO2", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE3 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE3", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT13 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT13", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT23 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT23", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE3 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE3", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI3 As SqlParameter = SQLcmd.Parameters.Add("@STANI3", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO3 As SqlParameter = SQLcmd.Parameters.Add("@SURYO3", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE4 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE4", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT14 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT14", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT24 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT24", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE4 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE4", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI4 As SqlParameter = SQLcmd.Parameters.Add("@STANI4", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO4 As SqlParameter = SQLcmd.Parameters.Add("@SURYO4", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE5 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE5", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT15 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT15", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT25 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT25", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE5 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE5", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI5 As SqlParameter = SQLcmd.Parameters.Add("@STANI5", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO5 As SqlParameter = SQLcmd.Parameters.Add("@SURYO5", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE6 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE6", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT16 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT16", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT26 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT26", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE6 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE6", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI6 As SqlParameter = SQLcmd.Parameters.Add("@STANI6", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO6 As SqlParameter = SQLcmd.Parameters.Add("@SURYO6", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE7 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE7", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT17 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT17", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT27 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT27", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE7 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE7", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI7 As SqlParameter = SQLcmd.Parameters.Add("@STANI7", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO7 As SqlParameter = SQLcmd.Parameters.Add("@SURYO7", System.Data.SqlDbType.Decimal)
            Dim P_OILTYPE8 As SqlParameter = SQLcmd.Parameters.Add("@OILTYPE8", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT18 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT18", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCT28 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCT28", System.Data.SqlDbType.NVarChar, 20)
            Dim P_PRODUCTCODE8 As SqlParameter = SQLcmd.Parameters.Add("@PRODUCTCODE8", System.Data.SqlDbType.NVarChar, 30)
            Dim P_STANI8 As SqlParameter = SQLcmd.Parameters.Add("@STANI8", System.Data.SqlDbType.NVarChar, 10)
            Dim P_SURYO8 As SqlParameter = SQLcmd.Parameters.Add("@SURYO8", System.Data.SqlDbType.Decimal)
            Dim P_TOTALSURYO As SqlParameter = SQLcmd.Parameters.Add("@TOTALSURYO", System.Data.SqlDbType.Decimal)
            Dim P_TUMIOKIKBN As SqlParameter = SQLcmd.Parameters.Add("@TUMIOKIKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@DETAILNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_TRIPNO As SqlParameter = SQLcmd.Parameters.Add("@TRIPNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_DROPNO As SqlParameter = SQLcmd.Parameters.Add("@DROPNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_JISSKIKBN As SqlParameter = SQLcmd.Parameters.Add("@JISSKIKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_URIKBN As SqlParameter = SQLcmd.Parameters.Add("@URIKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_STORICODE As SqlParameter = SQLcmd.Parameters.Add("@STORICODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_CONTCHASSIS As SqlParameter = SQLcmd.Parameters.Add("@CONTCHASSIS", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SHARYOTYPEF As SqlParameter = SQLcmd.Parameters.Add("@SHARYOTYPEF", System.Data.SqlDbType.NVarChar, 1)
            Dim P_TSHABANF As SqlParameter = SQLcmd.Parameters.Add("@TSHABANF", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SHARYOTYPEB As SqlParameter = SQLcmd.Parameters.Add("@SHARYOTYPEB", System.Data.SqlDbType.NVarChar, 1)
            Dim P_TSHABANB As SqlParameter = SQLcmd.Parameters.Add("@TSHABANB", System.Data.SqlDbType.NVarChar, 20)
            Dim P_SHARYOTYPEB2 As SqlParameter = SQLcmd.Parameters.Add("@SHARYOTYPEB2", System.Data.SqlDbType.NVarChar, 1)
            Dim P_TSHABANB2 As SqlParameter = SQLcmd.Parameters.Add("@TSHABANB2", System.Data.SqlDbType.NVarChar, 20)
            Dim P_TAXKBN As SqlParameter = SQLcmd.Parameters.Add("@TAXKBN", System.Data.SqlDbType.NVarChar, 10)
            Dim P_LATITUDE As SqlParameter = SQLcmd.Parameters.Add("@LATITUDE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_LONGITUDE As SqlParameter = SQLcmd.Parameters.Add("@LONGITUDE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1SHUKODATE As SqlParameter = SQLcmd.Parameters.Add("@L1SHUKODATE", System.Data.SqlDbType.Date)
            Dim P_L1SHUKADATE As SqlParameter = SQLcmd.Parameters.Add("@L1SHUKADATE", System.Data.SqlDbType.Date)
            Dim P_L1TODOKEDATE As SqlParameter = SQLcmd.Parameters.Add("@L1TODOKEDATE", System.Data.SqlDbType.Date)
            Dim P_L1TRIPNO As SqlParameter = SQLcmd.Parameters.Add("@L1TRIPNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_L1DROPNO As SqlParameter = SQLcmd.Parameters.Add("@L1DROPNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_L1TORICODE As SqlParameter = SQLcmd.Parameters.Add("@L1TORICODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1URIKBN As SqlParameter = SQLcmd.Parameters.Add("@L1URIKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_L1STORICODE As SqlParameter = SQLcmd.Parameters.Add("@L1STORICODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1TODOKECODE As SqlParameter = SQLcmd.Parameters.Add("@L1TODOKECODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1SHUKABASHO As SqlParameter = SQLcmd.Parameters.Add("@L1SHUKABASHO", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1CREWKBN As SqlParameter = SQLcmd.Parameters.Add("@L1CREWKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_L1STAFFKBN As SqlParameter = SQLcmd.Parameters.Add("@L1STAFFKBN", System.Data.SqlDbType.NVarChar, 5)
            Dim P_L1STAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@L1STAFFCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1SUBSTAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@L1SUBSTAFFCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@L1ORDERNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_L1DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@L1DETAILNO", System.Data.SqlDbType.NVarChar, 10)
            Dim P_L1ORDERORG As SqlParameter = SQLcmd.Parameters.Add("@L1ORDERORG", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1KAISO As SqlParameter = SQLcmd.Parameters.Add("@L1KAISO", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1KUSHAKBN As SqlParameter = SQLcmd.Parameters.Add("@L1KUSHAKBN", System.Data.SqlDbType.NVarChar, 20)
            Dim P_L1IPPDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@L1IPPDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_L1KOSDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@L1KOSDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_L1IPPJIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@L1IPPJIDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_L1IPPKUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@L1IPPKUDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_L1KOSJIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@L1KOSJIDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_L1KOSKUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@L1KOSKUDISTANCE", System.Data.SqlDbType.Decimal)
            Dim P_L1WORKTIME As SqlParameter = SQLcmd.Parameters.Add("@L1WORKTIME", System.Data.SqlDbType.Int)
            Dim P_L1MOVETIME As SqlParameter = SQLcmd.Parameters.Add("@L1MOVETIME", System.Data.SqlDbType.Int)
            Dim P_L1ACTTIME As SqlParameter = SQLcmd.Parameters.Add("@L1ACTTIME", System.Data.SqlDbType.Int)
            Dim P_L1JIMOVETIME As SqlParameter = SQLcmd.Parameters.Add("@L1JIMOVETIME", System.Data.SqlDbType.Int)
            Dim P_L1KUMOVETIME As SqlParameter = SQLcmd.Parameters.Add("@L1KUMOVETIME", System.Data.SqlDbType.Int)
            Dim P_L1HAISOGROUP As SqlParameter = SQLcmd.Parameters.Add("@L1HAISOGROUP", System.Data.SqlDbType.NVarChar, 20)
            Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
            Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar, 20)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar, 30)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            P_CAMPCODE.Value = I_ROW("CAMPCODE")
            P_SHIPORG.Value = I_ROW("SHIPORG")
            P_TERMKBN.Value = I_ROW("TERMKBN")
            P_YMD.Value = I_ROW("YMD")
            P_STAFFCODE.Value = I_ROW("STAFFCODE")
            P_SEQ.Value = I_ROW("SEQ")
            P_ENTRYDATE.Value = I_ROW("ENTRYDATE")
            P_CREWKBN.Value = I_ROW("CREWKBN")
            P_NIPPONO.Value = I_ROW("NIPPONO")
            P_WORKKBN.Value = I_ROW("WORKKBN")
            P_GSHABAN.Value = I_ROW("GSHABAN")
            P_SUBSTAFFCODE.Value = I_ROW("SUBSTAFFCODE")
            P_STDATE.Value = I_ROW("STDATE")
            P_STTIME.Value = I_ROW("STTIME")
            P_ENDDATE.Value = I_ROW("ENDDATE")
            P_ENDTIME.Value = I_ROW("ENDTIME")
            P_WORKTIME.Value = I_ROW("WORKTIME")
            P_MOVETIME.Value = I_ROW("MOVETIME")
            P_ACTTIME.Value = I_ROW("ACTTIME")
            P_PRATE.Value = I_ROW("PRATE")
            P_CASH.Value = I_ROW("CASH")
            P_TICKET.Value = I_ROW("TICKET")
            P_ETC.Value = I_ROW("ETC")
            P_TOTALTOLL.Value = I_ROW("TOTALTOLL")
            P_STMATER.Value = I_ROW("STMATER")
            P_ENDMATER.Value = I_ROW("ENDMATER")
            P_RUIDISTANCE.Value = I_ROW("RUIDISTANCE")
            P_SOUDISTANCE.Value = I_ROW("SOUDISTANCE")
            P_JIDISTANCE.Value = I_ROW("JIDISTANCE")
            P_KUDISTANCE.Value = I_ROW("KUDISTANCE")
            P_IPPDISTANCE.Value = I_ROW("IPPDISTANCE")
            P_KOSDISTANCE.Value = I_ROW("KOSDISTANCE")
            P_IPPJIDISTANCE.Value = I_ROW("IPPJIDISTANCE")
            P_IPPKUDISTANCE.Value = I_ROW("IPPKUDISTANCE")
            P_KOSJIDISTANCE.Value = I_ROW("KOSJIDISTANCE")
            P_KOSKUDISTANCE.Value = I_ROW("KOSKUDISTANCE")
            P_KYUYU.Value = I_ROW("KYUYU")
            P_TORICODE.Value = I_ROW("TORICODE")
            P_SHUKABASHO.Value = I_ROW("SHUKABASHO")
            P_SHUKADATE.Value = I_ROW("SHUKADATE")
            P_TODOKECODE.Value = I_ROW("TODOKECODE")
            P_TODOKEDATE.Value = I_ROW("TODOKEDATE")
            P_OILTYPE1.Value = I_ROW("OILTYPE1")
            P_PRODUCT11.Value = I_ROW("PRODUCT11")
            P_PRODUCT21.Value = I_ROW("PRODUCT21")
            P_PRODUCTCODE1.Value = I_ROW("PRODUCTCODE1")
            P_STANI1.Value = I_ROW("STANI1")
            P_SURYO1.Value = I_ROW("SURYO1")
            P_OILTYPE2.Value = I_ROW("OILTYPE2")
            P_PRODUCT12.Value = I_ROW("PRODUCT12")
            P_PRODUCT22.Value = I_ROW("PRODUCT22")
            P_PRODUCTCODE2.Value = I_ROW("PRODUCTCODE2")
            P_STANI2.Value = I_ROW("STANI2")
            P_SURYO2.Value = I_ROW("SURYO2")
            P_OILTYPE3.Value = I_ROW("OILTYPE3")
            P_PRODUCT13.Value = I_ROW("PRODUCT13")
            P_PRODUCT23.Value = I_ROW("PRODUCT23")
            P_PRODUCTCODE3.Value = I_ROW("PRODUCTCODE3")
            P_STANI3.Value = I_ROW("STANI3")
            P_SURYO3.Value = I_ROW("SURYO3")
            P_OILTYPE4.Value = I_ROW("OILTYPE4")
            P_PRODUCT14.Value = I_ROW("PRODUCT14")
            P_PRODUCT24.Value = I_ROW("PRODUCT24")
            P_PRODUCTCODE4.Value = I_ROW("PRODUCTCODE4")
            P_STANI4.Value = I_ROW("STANI4")
            P_SURYO4.Value = I_ROW("SURYO4")
            P_OILTYPE5.Value = I_ROW("OILTYPE5")
            P_PRODUCT15.Value = I_ROW("PRODUCT15")
            P_PRODUCT25.Value = I_ROW("PRODUCT25")
            P_PRODUCTCODE5.Value = I_ROW("PRODUCTCODE5")
            P_STANI5.Value = I_ROW("STANI5")
            P_SURYO5.Value = I_ROW("SURYO5")
            P_OILTYPE6.Value = I_ROW("OILTYPE6")
            P_PRODUCT16.Value = I_ROW("PRODUCT16")
            P_PRODUCT26.Value = I_ROW("PRODUCT26")
            P_PRODUCTCODE6.Value = I_ROW("PRODUCTCODE6")
            P_STANI6.Value = I_ROW("STANI6")
            P_SURYO6.Value = I_ROW("SURYO6")
            P_OILTYPE7.Value = I_ROW("OILTYPE7")
            P_PRODUCT17.Value = I_ROW("PRODUCT17")
            P_PRODUCT27.Value = I_ROW("PRODUCT27")
            P_PRODUCTCODE7.Value = I_ROW("PRODUCTCODE7")
            P_STANI7.Value = I_ROW("STANI7")
            P_SURYO7.Value = I_ROW("SURYO7")
            P_OILTYPE8.Value = I_ROW("OILTYPE8")
            P_PRODUCT18.Value = I_ROW("PRODUCT18")
            P_PRODUCT28.Value = I_ROW("PRODUCT28")
            P_PRODUCTCODE8.Value = I_ROW("PRODUCTCODE8")
            P_STANI8.Value = I_ROW("STANI8")
            P_SURYO8.Value = I_ROW("SURYO8")
            P_TOTALSURYO.Value = I_ROW("TOTALSURYO")
            P_TUMIOKIKBN.Value = I_ROW("TUMIOKIKBN")
            P_ORDERNO.Value = I_ROW("ORDERNO")
            P_DETAILNO.Value = I_ROW("DETAILNO")
            P_TRIPNO.Value = I_ROW("TRIPNO")
            P_DROPNO.Value = I_ROW("DROPNO")
            P_JISSKIKBN.Value = I_ROW("JISSKIKBN")
            P_URIKBN.Value = I_ROW("URIKBN")
            P_STORICODE.Value = I_ROW("STORICODE")
            P_CONTCHASSIS.Value = I_ROW("CONTCHASSIS")
            P_SHARYOTYPEF.Value = I_ROW("SHARYOTYPEF")
            P_TSHABANF.Value = I_ROW("TSHABANF")
            P_SHARYOTYPEB.Value = I_ROW("SHARYOTYPEB")
            P_TSHABANB.Value = I_ROW("TSHABANB")
            P_SHARYOTYPEB2.Value = I_ROW("SHARYOTYPEB2")
            P_TSHABANB2.Value = I_ROW("TSHABANB2")
            P_TAXKBN.Value = I_ROW("TAXKBN")
            P_LATITUDE.Value = I_ROW("LATITUDE")
            P_LONGITUDE.Value = I_ROW("LONGITUDE")
            P_L1SHUKODATE.Value = I_ROW("L1SHUKODATE")
            P_L1SHUKADATE.Value = I_ROW("L1SHUKADATE")
            P_L1TODOKEDATE.Value = I_ROW("L1TODOKEDATE")
            P_L1TRIPNO.Value = I_ROW("L1TRIPNO")
            P_L1DROPNO.Value = I_ROW("L1DROPNO")
            P_L1TORICODE.Value = I_ROW("L1TORICODE")
            P_L1URIKBN.Value = I_ROW("L1URIKBN")
            P_L1STORICODE.Value = I_ROW("L1STORICODE")
            P_L1TODOKECODE.Value = I_ROW("L1TODOKECODE")
            P_L1SHUKABASHO.Value = I_ROW("L1SHUKABASHO")
            P_L1CREWKBN.Value = I_ROW("L1CREWKBN")
            P_L1STAFFKBN.Value = I_ROW("L1STAFFKBN")
            P_L1STAFFCODE.Value = I_ROW("L1STAFFCODE")
            P_L1SUBSTAFFCODE.Value = I_ROW("L1SUBSTAFFCODE")
            P_L1ORDERNO.Value = I_ROW("L1ORDERNO")
            P_L1DETAILNO.Value = I_ROW("L1DETAILNO")
            P_L1ORDERORG.Value = I_ROW("L1ORDERORG")
            P_L1KAISO.Value = I_ROW("L1KAISO")
            P_L1KUSHAKBN.Value = I_ROW("L1KUSHAKBN")
            P_L1IPPDISTANCE.Value = I_ROW("L1IPPDISTANCE")
            P_L1KOSDISTANCE.Value = I_ROW("L1KOSDISTANCE")
            P_L1IPPJIDISTANCE.Value = I_ROW("L1IPPJIDISTANCE")
            P_L1IPPKUDISTANCE.Value = I_ROW("L1IPPKUDISTANCE")
            P_L1KOSJIDISTANCE.Value = I_ROW("L1KOSJIDISTANCE")
            P_L1KOSKUDISTANCE.Value = I_ROW("L1KOSKUDISTANCE")
            P_L1WORKTIME.Value = I_ROW("L1WORKTIME")
            P_L1MOVETIME.Value = I_ROW("L1MOVETIME")
            P_L1ACTTIME.Value = I_ROW("L1ACTTIME")
            P_L1JIMOVETIME.Value = I_ROW("L1JIMOVETIME")
            P_L1KUMOVETIME.Value = I_ROW("L1KUMOVETIME")
            P_L1HAISOGROUP.Value = I_ROW("L1HAISOGROUP")
            P_DELFLG.Value = I_ROW("DELFLG")
            P_INITYMD.Value = I_ROW("INITYMD")
            P_UPDYMD.Value = I_ROW("UPDYMD")
            P_UPDUSER.Value = I_ROW("UPDUSER")
            P_UPDTERMID.Value = I_ROW("UPDTERMID")
            P_RECEIVEYMD.Value = I_ROW("RECEIVEYMD")

            SQLcmd.CommandTimeout = 300
            SQLcmd.ExecuteNonQuery()

            'CLOSE
        End Using

    End Sub

    ''' <summary>
    ''' 日報ＤＢカラム定義をローカルテーブルに登録
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    ''' <remarks></remarks>
    Private Sub AddColumnT0005UPDtbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()
        'T0005DB項目作成
        IO_TBL.Clear()
        IO_TBL.Columns.Add("CAMPCODE", GetType(String))
        IO_TBL.Columns.Add("SHIPORG", GetType(String))
        IO_TBL.Columns.Add("TERMKBN", GetType(String))
        IO_TBL.Columns.Add("YMD", GetType(String))
        IO_TBL.Columns.Add("STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("SEQ", GetType(String))
        IO_TBL.Columns.Add("ENTRYDATE", GetType(String))
        IO_TBL.Columns.Add("CREWKBN", GetType(String))
        IO_TBL.Columns.Add("NIPPONO", GetType(String))
        IO_TBL.Columns.Add("WORKKBN", GetType(String))
        IO_TBL.Columns.Add("GSHABAN", GetType(String))
        IO_TBL.Columns.Add("SUBSTAFFCODE", GetType(String))
        IO_TBL.Columns.Add("STDATE", GetType(String))
        IO_TBL.Columns.Add("STTIME", GetType(String))
        IO_TBL.Columns.Add("ENDDATE", GetType(String))
        IO_TBL.Columns.Add("ENDTIME", GetType(String))
        IO_TBL.Columns.Add("WORKTIME", GetType(String))
        IO_TBL.Columns.Add("MOVETIME", GetType(String))
        IO_TBL.Columns.Add("ACTTIME", GetType(String))
        IO_TBL.Columns.Add("PRATE", GetType(String))
        IO_TBL.Columns.Add("CASH", GetType(String))
        IO_TBL.Columns.Add("TICKET", GetType(String))
        IO_TBL.Columns.Add("ETC", GetType(String))
        IO_TBL.Columns.Add("TOTALTOLL", GetType(String))
        IO_TBL.Columns.Add("STMATER", GetType(String))
        IO_TBL.Columns.Add("ENDMATER", GetType(String))
        IO_TBL.Columns.Add("RUIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("SOUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("JIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("IPPDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KOSDISTANCE", GetType(String))
        IO_TBL.Columns.Add("IPPJIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("IPPKUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KOSJIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KOSKUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("KYUYU", GetType(String))
        IO_TBL.Columns.Add("TORICODE", GetType(String))
        IO_TBL.Columns.Add("SHUKABASHO", GetType(String))
        IO_TBL.Columns.Add("SHUKADATE", GetType(String))
        IO_TBL.Columns.Add("TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("TODOKEDATE", GetType(String))
        IO_TBL.Columns.Add("OILTYPE1", GetType(String))
        IO_TBL.Columns.Add("PRODUCT11", GetType(String))
        IO_TBL.Columns.Add("PRODUCT21", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE1", GetType(String))
        IO_TBL.Columns.Add("STANI1", GetType(String))
        IO_TBL.Columns.Add("SURYO1", GetType(String))
        IO_TBL.Columns.Add("OILTYPE2", GetType(String))
        IO_TBL.Columns.Add("PRODUCT12", GetType(String))
        IO_TBL.Columns.Add("PRODUCT22", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE2", GetType(String))
        IO_TBL.Columns.Add("STANI2", GetType(String))
        IO_TBL.Columns.Add("SURYO2", GetType(String))
        IO_TBL.Columns.Add("OILTYPE3", GetType(String))
        IO_TBL.Columns.Add("PRODUCT13", GetType(String))
        IO_TBL.Columns.Add("PRODUCT23", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE3", GetType(String))
        IO_TBL.Columns.Add("STANI3", GetType(String))
        IO_TBL.Columns.Add("SURYO3", GetType(String))
        IO_TBL.Columns.Add("OILTYPE4", GetType(String))
        IO_TBL.Columns.Add("PRODUCT14", GetType(String))
        IO_TBL.Columns.Add("PRODUCT24", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE4", GetType(String))
        IO_TBL.Columns.Add("STANI4", GetType(String))
        IO_TBL.Columns.Add("SURYO4", GetType(String))
        IO_TBL.Columns.Add("OILTYPE5", GetType(String))
        IO_TBL.Columns.Add("PRODUCT15", GetType(String))
        IO_TBL.Columns.Add("PRODUCT25", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE5", GetType(String))
        IO_TBL.Columns.Add("STANI5", GetType(String))
        IO_TBL.Columns.Add("SURYO5", GetType(String))
        IO_TBL.Columns.Add("OILTYPE6", GetType(String))
        IO_TBL.Columns.Add("PRODUCT16", GetType(String))
        IO_TBL.Columns.Add("PRODUCT26", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE6", GetType(String))
        IO_TBL.Columns.Add("STANI6", GetType(String))
        IO_TBL.Columns.Add("SURYO6", GetType(String))
        IO_TBL.Columns.Add("OILTYPE7", GetType(String))
        IO_TBL.Columns.Add("PRODUCT17", GetType(String))
        IO_TBL.Columns.Add("PRODUCT27", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE7", GetType(String))
        IO_TBL.Columns.Add("STANI7", GetType(String))
        IO_TBL.Columns.Add("SURYO7", GetType(String))
        IO_TBL.Columns.Add("OILTYPE8", GetType(String))
        IO_TBL.Columns.Add("PRODUCT18", GetType(String))
        IO_TBL.Columns.Add("PRODUCT28", GetType(String))
        IO_TBL.Columns.Add("PRODUCTCODE8", GetType(String))
        IO_TBL.Columns.Add("STANI8", GetType(String))
        IO_TBL.Columns.Add("SURYO8", GetType(String))
        IO_TBL.Columns.Add("TOTALSURYO", GetType(String))
        IO_TBL.Columns.Add("TUMIOKIKBN", GetType(String))
        IO_TBL.Columns.Add("ORDERNO", GetType(String))
        IO_TBL.Columns.Add("DETAILNO", GetType(String))
        IO_TBL.Columns.Add("TRIPNO", GetType(String))
        IO_TBL.Columns.Add("DROPNO", GetType(String))
        IO_TBL.Columns.Add("JISSKIKBN", GetType(String))
        IO_TBL.Columns.Add("URIKBN", GetType(String))
        IO_TBL.Columns.Add("STORICODE", GetType(String))
        IO_TBL.Columns.Add("CONTCHASSIS", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEF", GetType(String))
        IO_TBL.Columns.Add("TSHABANF", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB", GetType(String))
        IO_TBL.Columns.Add("TSHABANB", GetType(String))
        IO_TBL.Columns.Add("SHARYOTYPEB2", GetType(String))
        IO_TBL.Columns.Add("TSHABANB2", GetType(String))
        IO_TBL.Columns.Add("TAXKBN", GetType(String))
        IO_TBL.Columns.Add("LATITUDE", GetType(String))
        IO_TBL.Columns.Add("LONGITUDE", GetType(String))

        IO_TBL.Columns.Add("L1SHUKODATE", GetType(String))
        IO_TBL.Columns.Add("L1SHUKADATE", GetType(String))
        IO_TBL.Columns.Add("L1TODOKEDATE", GetType(String))
        IO_TBL.Columns.Add("L1TRIPNO", GetType(String))
        IO_TBL.Columns.Add("L1DROPNO", GetType(String))
        IO_TBL.Columns.Add("L1TORICODE", GetType(String))
        IO_TBL.Columns.Add("L1URIKBN", GetType(String))
        IO_TBL.Columns.Add("L1STORICODE", GetType(String))
        IO_TBL.Columns.Add("L1TODOKECODE", GetType(String))
        IO_TBL.Columns.Add("L1SHUKABASHO", GetType(String))
        IO_TBL.Columns.Add("L1CREWKBN", GetType(String))
        IO_TBL.Columns.Add("L1STAFFKBN", GetType(String))
        IO_TBL.Columns.Add("L1STAFFCODE", GetType(String))
        IO_TBL.Columns.Add("L1SUBSTAFFCODE", GetType(String))
        IO_TBL.Columns.Add("L1ORDERNO", GetType(String))
        IO_TBL.Columns.Add("L1DETAILNO", GetType(String))
        IO_TBL.Columns.Add("L1ORDERORG", GetType(String))
        IO_TBL.Columns.Add("L1KAISO", GetType(String))
        IO_TBL.Columns.Add("L1KUSHAKBN", GetType(String))
        IO_TBL.Columns.Add("L1IPPDISTANCE", GetType(String))
        IO_TBL.Columns.Add("L1KOSDISTANCE", GetType(String))
        IO_TBL.Columns.Add("L1IPPJIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("L1IPPKUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("L1KOSJIDISTANCE", GetType(String))
        IO_TBL.Columns.Add("L1KOSKUDISTANCE", GetType(String))
        IO_TBL.Columns.Add("L1WORKTIME", GetType(String))
        IO_TBL.Columns.Add("L1MOVETIME", GetType(String))
        IO_TBL.Columns.Add("L1ACTTIME", GetType(String))
        IO_TBL.Columns.Add("L1JIMOVETIME", GetType(String))
        IO_TBL.Columns.Add("L1KUMOVETIME", GetType(String))
        IO_TBL.Columns.Add("L1HAISOGROUP", GetType(String))

        IO_TBL.Columns.Add("DELFLG", GetType(String))
        IO_TBL.Columns.Add("INITYMD", GetType(String))
        IO_TBL.Columns.Add("UPDYMD", GetType(String))
        IO_TBL.Columns.Add("UPDUSER", GetType(String))
        IO_TBL.Columns.Add("UPDTERMID", GetType(String))
        IO_TBL.Columns.Add("RECEIVEYMD", GetType(String))

    End Sub
    ''' <summary>
    ''' 指定された所属部署より端末IDを取得する
    ''' </summary>
    ''' <param name="I_TERM_ORG">端末が所属している部署</param>
    ''' <param name="O_TERMID">端末ID</param>
    ''' <remarks></remarks>
    Private Sub GetTermID(ByVal I_TERM_ORG As String,
                          ByRef O_TERMID As String)

        'DataBase接続文字
        Dim SQL_Str As String = String.Empty
        '指定された端末ORGより端末IDを取得
        SQL_Str =
                    " SELECT DISTINCT A.TERMID as TERMID " &
                    " FROM S0001_TERM A " &
                    " WHERE A.TERMORG      =  '" & I_TERM_ORG & "' " &
                    " AND   A.TERMCLASS    >= '1' " &
                    " AND   A.DELFLG       <> '1' "

        Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            SQLcmd.CommandTimeout = 300
            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                O_TERMID = String.Empty
                While SQLdr.Read
                    O_TERMID = SQLdr("TERMID")
                End While

            End Using
        End Using

    End Sub

    ''' <summary>
    ''' 指定された端末IDとテーブルIDをもとに振分先端末ID（配列）を取得する
    ''' </summary>
    ''' <param name="I_TERMID">検索端末ID</param>
    ''' <param name="I_DATATERMID">データ作成端末ID</param>
    ''' <param name="I_TABLEID">テーブルID</param>
    ''' <param name="O_TODATATERMARRAY">データ配信先端末ID（配列)</param>
    ''' <param name="O_SERNTERMARRAY">配信先端末ID（配列)</param>
    ''' <remarks></remarks>
    Private Sub GetSendTermArry(ByVal I_TERMID As String,
                                ByVal I_DATATERMID As String,
                                ByVal I_TABLEID As String,
                                ByRef O_TODATATERMARRAY As Object,
                                ByRef O_SERNTERMARRAY As Object)

        Dim SQL_Str As String = String.Empty
        '指定された端末ID、テーブルIDより振分先を取得
        SQL_Str =
                    " SELECT DISTINCT A.TODATATERMID as TODATATERMID, A.SENDTERMID as SENDTERMID " &
                    " FROM S0018_SENDTERM A " &
                    " INNER JOIN S0001_TERM B " &
                    " ON    A.SENDTERMID   =  B.TERMID " &
                    " AND   B.DELFLG       <> '1' " &
                    " WHERE A.TERMID       =  '" & I_TERMID & "' " &
                    " AND   A.FRDATATERMID =  '" & I_DATATERMID & "' " &
                    " AND   A.TBLID        =  '" & I_TABLEID & "' " &
                    " AND   A.DELFLG       <> '1' "

        Using SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            SQLcmd.CommandTimeout = 300
            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                O_TODATATERMARRAY.Clear()
                O_SERNTERMARRAY.Clear()

                While SQLdr.Read
                    O_TODATATERMARRAY.Add(SQLdr("TODATATERMID"))
                    O_SERNTERMARRAY.Add(SQLdr("SENDTERMID"))
                End While
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' 日報テーブル削除処理（論理削除）
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <param name="I_DATENOW"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub DeleteT0005(ByVal I_ROW As DataRow, ByVal I_DATENOW As Date, ByRef O_RTN As String)
        Try
            O_RTN = C_MESSAGE_NO.NORMAL

            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE T0005_NIPPO " _
                      & "SET DELFLG      = '1' " _
                      & "  , UPDYMD      = @P05 " _
                      & "  , UPDUSER     = @P06 " _
                      & "  , UPDTERMID   = @P07 " _
                      & "  , RECEIVEYMD  = @P08  " _
                      & "WHERE CAMPCODE  = @P01 " _
                      & "  and SHIPORG   = @P02 " _
                      & "  and YMD       = @P03 " _
                      & "  and STAFFCODE = @P04 " _
                      & "  and DELFLG   <> '1' ; "

            Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon, SQLtrn)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.DateTime)
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar, 20)
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar, 30)
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.DateTime)

                PARA01.Value = I_ROW("CAMPCODE")
                PARA02.Value = I_ROW("SHIPORG")
                PARA03.Value = I_ROW("YMD")
                PARA04.Value = I_ROW("STAFFCODE")
                PARA05.Value = I_DATENOW
                PARA06.Value = UPDUSERID
                PARA07.Value = UPDTERMID
                PARA08.Value = C_DEFAULT_YMD

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                'CLOSE
            End Using

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.DB_ERROR

            CS0011LOGWRITE.INFSUBCLASS = "T0005_Delete"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:UPDATE T0005_NIPPO"      '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = O_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  日報ＤＢ編集                                                          ***
    ''' <summary>
    ''' 日報テーブル編集処理
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <param name="I_DATE"></param>
    ''' <param name="O_TBL"></param>
    ''' <param name="O_ROW"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditT0005(ByRef I_ROW As DataRow, ByVal I_DATE As Date, ByRef O_TBL As DataTable, ByRef O_ROW As DataRow, ByRef O_RTN As String)

        Dim WW_DATENOW As DateTime = Date.Now

        O_RTN = C_MESSAGE_NO.NORMAL

        O_ROW = O_TBL.NewRow

        O_ROW("CAMPCODE") = I_ROW("CAMPCODE")
        O_ROW("SHIPORG") = I_ROW("SHIPORG")
        O_ROW("TERMKBN") = I_ROW("TERMKBN")
        O_ROW("YMD") = I_ROW("YMD")
        O_ROW("NIPPONO") = I_ROW("NIPPONO")
        O_ROW("SEQ") = Val(I_ROW("SEQ"))
        O_ROW("WORKKBN") = I_ROW("WORKKBN")
        O_ROW("STAFFCODE") = I_ROW("STAFFCODE")
        If I_ROW("DELFLG") = "1" Then
            O_ROW("ENTRYDATE") = I_ROW("ENTRYDATE")
        Else
            O_ROW("ENTRYDATE") = I_DATE.ToString("yyyyMMddHHmmssfff")
        End If
        O_ROW("SUBSTAFFCODE") = I_ROW("SUBSTAFFCODE")
        O_ROW("CREWKBN") = I_ROW("CREWKBN")
        O_ROW("GSHABAN") = I_ROW("GSHABAN")
        O_ROW("STDATE") = I_ROW("STDATE")
        O_ROW("STTIME") = I_ROW("STTIME")
        O_ROW("ENDDATE") = I_ROW("ENDDATE")
        O_ROW("ENDTIME") = I_ROW("ENDTIME")
        Try
            O_ROW("WORKTIME") = TimeSpan.Parse(I_ROW("WORKTIME")).TotalMinutes
        Catch ex As Exception
            O_ROW("WORKTIME") = 0
        End Try
        Try
            O_ROW("MOVETIME") = TimeSpan.Parse(I_ROW("MOVETIME")).TotalMinutes
        Catch ex As Exception
            O_ROW("MOVETIME") = 0
        End Try
        Try
            O_ROW("ACTTIME") = TimeSpan.Parse(I_ROW("ACTTIME")).TotalMinutes
        Catch ex As Exception
            O_ROW("ACTTIME") = 0
        End Try
        Dim WW_int As Integer = 0
        Try
            WW_int = I_ROW("PRATE").replace(",", "")
        Catch ex As Exception
            WW_int = 0
        End Try
        O_ROW("PRATE") = WW_int
        Try
            WW_int = I_ROW("CASH").replace(",", "")
        Catch ex As Exception
            WW_int = 0
        End Try
        O_ROW("CASH") = WW_int
        Try
            WW_int = I_ROW("TICKET").replace(",", "")
        Catch ex As Exception
            WW_int = 0
        End Try
        O_ROW("TICKET") = WW_int
        Try
            WW_int = I_ROW("ETC").replace(",", "")
        Catch ex As Exception
            WW_int = 0
        End Try
        O_ROW("ETC") = WW_int
        Try
            WW_int = I_ROW("TOTALTOLL").replace(",", "")
        Catch ex As Exception
            WW_int = 0
        End Try
        O_ROW("TOTALTOLL") = WW_int

        Dim WW_dbl As Double = 0
        Try
            WW_dbl = I_ROW("STMATER").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("STMATER") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("ENDMATER").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("ENDMATER") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("RUIDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("RUIDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("SOUDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SOUDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("JIDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("JIDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("KUDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("KUDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("IPPDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("IPPDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("KOSDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("KOSDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("IPPJIDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("IPPJIDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("IPPKUDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("IPPKUDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("KOSJIDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("KOSJIDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("KOSKUDISTANCE").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("KOSKUDISTANCE") = WW_dbl.ToString("0.00")
        Try
            WW_dbl = I_ROW("KYUYU").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("KYUYU") = WW_dbl.ToString("0.00")
        O_ROW("TORICODE") = I_ROW("TORICODE")
        O_ROW("SHUKABASHO") = I_ROW("SHUKABASHO")
        If I_ROW("SHUKADATE") = String.Empty Then
            O_ROW("SHUKADATE") = DBNull.Value
        Else
            O_ROW("SHUKADATE") = I_ROW("SHUKADATE")
        End If
        O_ROW("TODOKECODE") = I_ROW("TODOKECODE")
        If I_ROW("TODOKEDATE") = String.Empty Then
            O_ROW("TODOKEDATE") = DBNull.Value
        Else
            O_ROW("TODOKEDATE") = I_ROW("TODOKEDATE")
        End If
        O_ROW("OILTYPE1") = I_ROW("OILTYPE1")
        O_ROW("PRODUCT11") = I_ROW("PRODUCT11")
        O_ROW("PRODUCT21") = I_ROW("PRODUCT21")
        O_ROW("PRODUCTCODE1") = I_ROW("PRODUCTCODE1")
        O_ROW("STANI1") = I_ROW("STANI1")
        Try
            WW_dbl = I_ROW("SURYO1").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO1") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE2") = I_ROW("OILTYPE2")
        O_ROW("PRODUCT12") = I_ROW("PRODUCT12")
        O_ROW("PRODUCT22") = I_ROW("PRODUCT22")
        O_ROW("PRODUCTCODE2") = I_ROW("PRODUCTCODE2")
        O_ROW("STANI2") = I_ROW("STANI2")
        Try
            WW_dbl = I_ROW("SURYO2").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO2") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE3") = I_ROW("OILTYPE3")
        O_ROW("PRODUCT13") = I_ROW("PRODUCT13")
        O_ROW("PRODUCT23") = I_ROW("PRODUCT23")
        O_ROW("PRODUCTCODE3") = I_ROW("PRODUCTCODE3")
        O_ROW("STANI3") = I_ROW("STANI3")
        Try
            WW_dbl = I_ROW("SURYO3").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO3") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE4") = I_ROW("OILTYPE4")
        O_ROW("PRODUCT14") = I_ROW("PRODUCT14")
        O_ROW("PRODUCT24") = I_ROW("PRODUCT24")
        O_ROW("PRODUCTCODE4") = I_ROW("PRODUCTCODE4")
        O_ROW("STANI4") = I_ROW("STANI4")
        Try
            WW_dbl = I_ROW("SURYO4").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO4") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE5") = I_ROW("OILTYPE5")
        O_ROW("PRODUCT15") = I_ROW("PRODUCT15")
        O_ROW("PRODUCT25") = I_ROW("PRODUCT25")
        O_ROW("PRODUCTCODE5") = I_ROW("PRODUCTCODE5")
        O_ROW("STANI5") = I_ROW("STANI5")
        Try
            WW_dbl = I_ROW("SURYO5").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO5") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE6") = I_ROW("OILTYPE6")
        O_ROW("PRODUCT16") = I_ROW("PRODUCT16")
        O_ROW("PRODUCT26") = I_ROW("PRODUCT26")
        O_ROW("PRODUCTCODE6") = I_ROW("PRODUCTCODE6")
        O_ROW("STANI6") = I_ROW("STANI6")
        Try
            WW_dbl = I_ROW("SURYO6").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO6") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE7") = I_ROW("OILTYPE7")
        O_ROW("PRODUCT17") = I_ROW("PRODUCT17")
        O_ROW("PRODUCT27") = I_ROW("PRODUCT27")
        O_ROW("PRODUCTCODE7") = I_ROW("PRODUCTCODE7")
        O_ROW("STANI7") = I_ROW("STANI7")
        Try
            WW_dbl = I_ROW("SURYO7").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO7") = WW_dbl.ToString("0.000")
        O_ROW("OILTYPE8") = I_ROW("OILTYPE8")
        O_ROW("PRODUCT18") = I_ROW("PRODUCT18")
        O_ROW("PRODUCT28") = I_ROW("PRODUCT28")
        O_ROW("PRODUCTCODE8") = I_ROW("PRODUCTCODE8")
        O_ROW("STANI8") = I_ROW("STANI8")
        Try
            WW_dbl = I_ROW("SURYO8").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("SURYO8") = WW_dbl.ToString("0.000")
        Try
            WW_dbl = I_ROW("TOTALSURYO").replace(",", "")
        Catch ex As Exception
            WW_dbl = 0
        End Try
        O_ROW("TOTALSURYO") = WW_dbl.ToString("0.000")
        O_ROW("TUMIOKIKBN") = I_ROW("TUMIOKIKBN")
        O_ROW("ORDERNO") = I_ROW("ORDERNO")
        O_ROW("DETAILNO") = I_ROW("DETAILNO")
        O_ROW("TRIPNO") = I_ROW("TRIPNO").replace("新", "")
        O_ROW("DROPNO") = I_ROW("DROPNO")
        O_ROW("JISSKIKBN") = I_ROW("JISSKIKBN")
        O_ROW("URIKBN") = I_ROW("URIKBN")
        O_ROW("STORICODE") = I_ROW("STORICODE")
        O_ROW("CONTCHASSIS") = I_ROW("CONTCHASSIS")
        O_ROW("SHARYOTYPEF") = I_ROW("SHARYOTYPEF")
        O_ROW("TSHABANF") = I_ROW("TSHABANF")
        O_ROW("SHARYOTYPEB") = I_ROW("SHARYOTYPEB")
        O_ROW("TSHABANB") = I_ROW("TSHABANB")
        O_ROW("SHARYOTYPEB2") = I_ROW("SHARYOTYPEB2")
        O_ROW("TSHABANB2") = I_ROW("TSHABANB2")
        O_ROW("TAXKBN") = I_ROW("TAXKBN")
        O_ROW("LATITUDE") = I_ROW("LATITUDE")
        O_ROW("LONGITUDE") = I_ROW("LONGITUDE")

        If IsDBNull("wSHUKODATE") Then
            O_ROW("L1SHUKODATE") = DBNull.Value
        Else
            If I_ROW("wSHUKODATE") = String.Empty Then
                O_ROW("L1SHUKODATE") = DBNull.Value
            Else
                O_ROW("L1SHUKODATE") = I_ROW("wSHUKODATE")
            End If
        End If
        If IsDBNull("wSHUKADATE") Then
            O_ROW("L1SHUKADATE") = DBNull.Value
        Else
            If I_ROW("wSHUKADATE") = String.Empty Then
                O_ROW("L1SHUKADATE") = DBNull.Value
            Else
                O_ROW("L1SHUKADATE") = I_ROW("wSHUKADATE")
            End If
        End If
        If IsDBNull("wTODOKEDATE") Then
            O_ROW("L1TODOKEDATE") = DBNull.Value
        Else
            If I_ROW("wTODOKEDATE") = String.Empty Then
                O_ROW("L1TODOKEDATE") = DBNull.Value
            Else
                O_ROW("L1TODOKEDATE") = I_ROW("wTODOKEDATE")
            End If
        End If
        O_ROW("L1TRIPNO") = I_ROW("wTRIPNO_K")
        O_ROW("L1DROPNO") = I_ROW("wDROPNO")
        O_ROW("L1TORICODE") = I_ROW("wTORICODE")
        O_ROW("L1URIKBN") = I_ROW("wURIKBN")
        O_ROW("L1STORICODE") = I_ROW("wSTORICODE")
        O_ROW("L1TODOKECODE") = I_ROW("wTODOKECODE")
        O_ROW("L1SHUKABASHO") = I_ROW("wSHUKABASHO")
        O_ROW("L1CREWKBN") = I_ROW("wCREWKBN")
        O_ROW("L1STAFFKBN") = I_ROW("wSTAFFKBN")
        O_ROW("L1STAFFCODE") = I_ROW("wSTAFFCODE")
        O_ROW("L1SUBSTAFFCODE") = I_ROW("wSUBSTAFFCODE")
        O_ROW("L1ORDERNO") = I_ROW("wORDERNO")
        O_ROW("L1DETAILNO") = I_ROW("wDETAILNO")
        O_ROW("L1ORDERORG") = I_ROW("wORDERORG")
        O_ROW("L1KAISO") = I_ROW("wKAISO")
        O_ROW("L1KUSHAKBN") = I_ROW("wKUSHAKBN")
        O_ROW("L1IPPDISTANCE") = Val(I_ROW("wIPPDISTANCE"))
        O_ROW("L1KOSDISTANCE") = Val(I_ROW("wKOSDISTANCE"))
        O_ROW("L1IPPJIDISTANCE") = Val(I_ROW("wIPPJIDISTANCE"))
        O_ROW("L1IPPKUDISTANCE") = Val(I_ROW("wIPPKUDISTANCE"))
        O_ROW("L1KOSJIDISTANCE") = Val(I_ROW("wKOSJIDISTANCE"))
        O_ROW("L1KOSKUDISTANCE") = Val(I_ROW("wKOSKUDISTANCE"))
        O_ROW("L1WORKTIME") = Val(I_ROW("wWORKTIME"))
        O_ROW("L1MOVETIME") = Val(I_ROW("wMOVETIME"))
        O_ROW("L1ACTTIME") = Val(I_ROW("wACTTIME"))
        O_ROW("L1JIMOVETIME") = Val(I_ROW("wJIMOVETIME"))
        O_ROW("L1KUMOVETIME") = Val(I_ROW("wKUMOVETIME"))
        O_ROW("L1HAISOGROUP") = I_ROW("wHaisoGroup")

        O_ROW("DELFLG") = I_ROW("DELFLG")
        O_ROW("INITYMD") = I_DATE
        O_ROW("UPDYMD") = I_DATE
        O_ROW("UPDUSER") = UPDUSERID
        O_ROW("UPDTERMID") = UPDTERMID
        O_ROW("RECEIVEYMD") = C_DEFAULT_YMD

        O_TBL.Rows.Add(O_ROW)
    End Sub


End Class

''' <summary>
''' 日報画面共通クラス
''' </summary>
''' <remarks></remarks>
Public Class GRT0005COM
    ''' <summary>
    ''' 共有勘定科目判定テーブル
    ''' </summary>
    Private Shared ML002tbl As DataTable = New DataTable                    '勘定科目判定テーブル
    ''' <summary>
    ''' ログ出力管理クラス
    ''' </summary>
    Private CS0011LOGWRITE As New CS0011LOGWrite                            'LogOutput DirString Get
    ''' <summary>
    ''' テーブルソート
    ''' </summary>
    Private CS0026TBLSort As New CS0026TBLSORT
    ''' <summary>
    ''' セッション管理クラス
    ''' </summary>
    Private CS0050Session As New CS0050SESSION
    '統計DB出力dll Interface
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <returns></returns>
    Public Property O_ERR As String                                          'リターン値
    ''' <summary>
    ''' 日報ローカルテーブル項目設定
    ''' </summary>
    ''' <param name="IO_TBL">ローカルテーブル</param>
    ''' <remarks></remarks>
    Public Sub AddColumnT0005tbl(ByRef IO_TBL As DataTable)

        If IsNothing(IO_TBL) Then IO_TBL = New DataTable
        If IO_TBL.Columns.Count <> 0 Then IO_TBL.Columns.Clear()

        'T0005DB項目作成
        IO_TBL.Clear()
        With IO_TBL.Columns
            .Add("LINECNT", GetType(Integer))
            .Add("OPERATION", GetType(String))
            .Add("TIMSTP", GetType(String))
            .Add("SELECT", GetType(Integer))
            .Add("HIDDEN", GetType(Integer))

            .Add("ORDERUMU", GetType(String))
            .Add("EXTRACTCNT", GetType(Integer))
            .Add("CTRL", GetType(String))
            .Add("TWOMANTRIP", GetType(String))

            .Add("CAMPCODE", GetType(String))
            .Add("CAMPNAMES", GetType(String))
            .Add("SHIPORG", GetType(String))
            .Add("SHIPORGNAMES", GetType(String))
            .Add("TERMKBN", GetType(String))
            .Add("TERMKBNNAMES", GetType(String))
            .Add("YMD", GetType(String))
            .Add("NIPPONO", GetType(String))
            .Add("HDKBN", GetType(String))
            .Add("WORKKBN", GetType(String))
            .Add("WORKKBNNAMES", GetType(String))
            .Add("SEQ", GetType(String))
            .Add("STAFFCODE", GetType(String))
            .Add("ENTRYDATE", GetType(String))
            .Add("STAFFNAMES", GetType(String))
            .Add("SUBSTAFFCODE", GetType(String))
            .Add("SUBSTAFFNAMES", GetType(String))
            .Add("CREWKBN", GetType(String))
            .Add("CREWKBNNAMES", GetType(String))
            .Add("GSHABAN", GetType(String))
            .Add("GSHABANLICNPLTNO", GetType(String))
            .Add("STDATE", GetType(String))
            .Add("STTIME", GetType(String))
            .Add("ENDDATE", GetType(String))
            .Add("ENDTIME", GetType(String))
            .Add("WORKTIME", GetType(String))
            .Add("MOVETIME", GetType(String))
            .Add("ACTTIME", GetType(String))
            .Add("PRATE", GetType(String))
            .Add("CASH", GetType(String))
            .Add("TICKET", GetType(String))
            .Add("ETC", GetType(String))
            .Add("TOTALTOLL", GetType(String))
            .Add("STMATER", GetType(String))
            .Add("ENDMATER", GetType(String))
            .Add("RUIDISTANCE", GetType(String))
            .Add("SOUDISTANCE", GetType(String))
            .Add("JIDISTANCE", GetType(String))
            .Add("KUDISTANCE", GetType(String))
            .Add("IPPDISTANCE", GetType(String))
            .Add("KOSDISTANCE", GetType(String))
            .Add("IPPJIDISTANCE", GetType(String))
            .Add("IPPKUDISTANCE", GetType(String))
            .Add("KOSJIDISTANCE", GetType(String))
            .Add("KOSKUDISTANCE", GetType(String))
            .Add("KYUYU", GetType(String))
            .Add("TORICODE", GetType(String))
            .Add("TORINAMES", GetType(String))
            .Add("SHUKABASHO", GetType(String))
            .Add("SHUKABASHONAMES", GetType(String))
            .Add("SHUKADATE", GetType(String))
            .Add("TODOKECODE", GetType(String))
            .Add("TODOKENAMES", GetType(String))
            .Add("TODOKEDATE", GetType(String))
            .Add("OILTYPE1", GetType(String))
            .Add("PRODUCT11", GetType(String))
            .Add("PRODUCT21", GetType(String))
            .Add("PRODUCTCODE1", GetType(String))
            .Add("PRODUCT1NAMES", GetType(String))
            .Add("SURYO1", GetType(String))
            .Add("STANI1", GetType(String))
            .Add("STANI1NAMES", GetType(String))
            .Add("OILTYPE2", GetType(String))
            .Add("PRODUCT12", GetType(String))
            .Add("PRODUCT22", GetType(String))
            .Add("PRODUCTCODE2", GetType(String))
            .Add("PRODUCT2NAMES", GetType(String))
            .Add("SURYO2", GetType(String))
            .Add("STANI2", GetType(String))
            .Add("STANI2NAMES", GetType(String))
            .Add("OILTYPE3", GetType(String))
            .Add("PRODUCT13", GetType(String))
            .Add("PRODUCT23", GetType(String))
            .Add("PRODUCTCODE3", GetType(String))
            .Add("PRODUCT3NAMES", GetType(String))
            .Add("SURYO3", GetType(String))
            .Add("STANI3", GetType(String))
            .Add("STANI3NAMES", GetType(String))
            .Add("OILTYPE4", GetType(String))
            .Add("PRODUCT14", GetType(String))
            .Add("PRODUCT24", GetType(String))
            .Add("PRODUCTCODE4", GetType(String))
            .Add("PRODUCT4NAMES", GetType(String))
            .Add("SURYO4", GetType(String))
            .Add("STANI4", GetType(String))
            .Add("STANI4NAMES", GetType(String))
            .Add("OILTYPE5", GetType(String))
            .Add("PRODUCT15", GetType(String))
            .Add("PRODUCT25", GetType(String))
            .Add("PRODUCTCODE5", GetType(String))
            .Add("PRODUCT5NAMES", GetType(String))
            .Add("SURYO5", GetType(String))
            .Add("STANI5", GetType(String))
            .Add("STANI5NAMES", GetType(String))
            .Add("OILTYPE6", GetType(String))
            .Add("PRODUCT16", GetType(String))
            .Add("PRODUCT26", GetType(String))
            .Add("PRODUCTCODE6", GetType(String))
            .Add("PRODUCT6NAMES", GetType(String))
            .Add("SURYO6", GetType(String))
            .Add("STANI6", GetType(String))
            .Add("STANI6NAMES", GetType(String))
            .Add("OILTYPE7", GetType(String))
            .Add("PRODUCT17", GetType(String))
            .Add("PRODUCT27", GetType(String))
            .Add("PRODUCTCODE7", GetType(String))
            .Add("PRODUCT7NAMES", GetType(String))
            .Add("SURYO7", GetType(String))
            .Add("STANI7", GetType(String))
            .Add("STANI7NAMES", GetType(String))
            .Add("OILTYPE8", GetType(String))
            .Add("PRODUCT18", GetType(String))
            .Add("PRODUCT28", GetType(String))
            .Add("PRODUCTCODE8", GetType(String))
            .Add("PRODUCT8NAMES", GetType(String))
            .Add("SURYO8", GetType(String))
            .Add("STANI8", GetType(String))
            .Add("STANI8NAMES", GetType(String))
            .Add("TOTALSURYO", GetType(String))
            .Add("ORDERNO", GetType(String))
            .Add("DETAILNO", GetType(String))
            .Add("TRIPNO", GetType(String))
            .Add("DROPNO", GetType(String))
            .Add("JISSKIKBN", GetType(String))
            .Add("JISSKIKBNNAMES", GetType(String))
            .Add("URIKBN", GetType(String))
            .Add("URIKBNNAMES", GetType(String))
            .Add("TUMIOKIKBN", GetType(String))
            .Add("TUMIOKIKBNNAMES", GetType(String))

            .Add("STORICODE", GetType(String))
            .Add("STORICODENAMES", GetType(String))
            .Add("CONTCHASSIS", GetType(String))
            .Add("CONTCHASSISLICNPLTNO", GetType(String))

            .Add("SHARYOTYPEF", GetType(String))
            .Add("TSHABANF", GetType(String))
            .Add("SHARYOTYPEB", GetType(String))
            .Add("TSHABANB", GetType(String))
            .Add("SHARYOTYPEB2", GetType(String))
            .Add("TSHABANB2", GetType(String))
            .Add("TAXKBN", GetType(String))
            .Add("TAXKBNNAMES", GetType(String))
            .Add("LATITUDE", GetType(String))
            .Add("LONGITUDE", GetType(String))
            .Add("DELFLG", GetType(String))

            .Add("HOLIDAYKBN", GetType(String))
            .Add("TORITYPE01", GetType(String))
            .Add("TORITYPE02", GetType(String))
            .Add("TORITYPE03", GetType(String))
            .Add("TORITYPE04", GetType(String))
            .Add("TORITYPE05", GetType(String))
            .Add("SUPPLIERKBN", GetType(String))
            .Add("SUPPLIER", GetType(String))
            .Add("MANGOILTYPE", GetType(String))
            .Add("MANGMORG1", GetType(String))
            .Add("MANGSORG1", GetType(String))
            .Add("MANGUORG1", GetType(String))
            .Add("BASELEASE1", GetType(String))
            .Add("MANGMORG2", GetType(String))
            .Add("MANGSORG2", GetType(String))
            .Add("MANGUORG2", GetType(String))
            .Add("BASELEASE2", GetType(String))
            .Add("MANGMORG3", GetType(String))
            .Add("MANGSORG3", GetType(String))
            .Add("MANGUORG3", GetType(String))
            .Add("BASELEASE3", GetType(String))
            .Add("STAFFKBN", GetType(String))
            .Add("MORG", GetType(String))
            .Add("HORG", GetType(String))
            .Add("SUBSTAFFKBN", GetType(String))
            .Add("SUBMORG", GetType(String))
            .Add("SUBHORG", GetType(String))

            .Add("ORDERORG", GetType(String))
            '.Add("YSURYO1", GetType(String))
            '.Add("HTANI1", GetType(String))
            '.Add("YSURYO2", GetType(String))
            '.Add("HTANI2", GetType(String))
            '.Add("YSURYO3", GetType(String))
            '.Add("HTANI3", GetType(String))
            '.Add("YSURYO4", GetType(String))
            '.Add("HTANI4", GetType(String))
            '.Add("YSURYO5", GetType(String))
            '.Add("HTANI5", GetType(String))
            '.Add("YSURYO6", GetType(String))
            '.Add("HTANI6", GetType(String))
            '.Add("YSURYO7", GetType(String))
            '.Add("HTANI7", GetType(String))
            '.Add("YSURYO8", GetType(String))
            '.Add("HTANI8", GetType(String))

            .Add("wSHUKODATE", GetType(String))             '出庫日
            .Add("wSHUKADATE", GetType(String))             '出荷日
            .Add("wTODOKEDATE", GetType(String))            '届日
            .Add("wTRIPNO_K", GetType(String))              '仮付番トリップ
            .Add("wTRIPNO", GetType(String))                'トリップ
            .Add("wDROPNO", GetType(String))                'ドロップ

            .Add("wTORICODE", GetType(String))              '荷主コード
            .Add("wURIKBN", GetType(String))                '売上計上基準
            .Add("wSTORICODE", GetType(String))             '販売店コード
            .Add("wTODOKECODE", GetType(String))            '届先コード
            .Add("wSHUKABASHO", GetType(String))            '出荷場所

            .Add("wCREWKBN", GetType(String))               '正副区分
            .Add("wSTAFFKBN", GetType(String))              '社員区分
            .Add("wSTAFFCODE", GetType(String))             '従業員コード
            .Add("wSUBSTAFFCODE", GetType(String))          '従業員コード（副）

            .Add("wORDERNO", GetType(String))               '受注番号
            .Add("wDETAILNO", GetType(String))              '明細№
            .Add("wORDERORG", GetType(String))              '受注部署

            .Add("wKAISO", GetType(String))                 '回送
            .Add("wKUSHAKBN", GetType(String))

            .Add("wTRIPDROPcnt", GetType(String))
            .Add("wDATECHANGE", GetType(String))
            .Add("wLASTstat", GetType(String))
            .Add("wFirstCNTUP", GetType(String))
            .Add("wF1F3flg", GetType(String))

            .Add("wIPPDISTANCE", GetType(String))           '一般
            .Add("wKOSDISTANCE", GetType(String))           '高速

            .Add("wIPPJIDISTANCE", GetType(String))         '一般実車
            .Add("wIPPKUDISTANCE", GetType(String))         '一般空車

            .Add("wKOSJIDISTANCE", GetType(String))         '高速実車
            .Add("wKOSKUDISTANCE", GetType(String))         '高速空車

            .Add("wWORKTIME", GetType(String))              '作業分
            .Add("wMOVETIME", GetType(String))              '移動分
            .Add("wACTTIME", GetType(String))               '稼働分

            .Add("wJIMOVETIME", GetType(String))            '実車走行分
            .Add("wKUMOVETIME", GetType(String))            '空車走行分
            .Add("wKAIJI", GetType(String))                 '回次
            .Add("wSUISOKBN", GetType(String))              '水素区分

            .Add("wShachuhaku", GetType(String))            '車中泊
            .Add("wHaisoGroup", GetType(String))            '配送グループ分類

        End With
    End Sub

    ''' <summary>
    ''' 編集可能エリアを初期化する
    ''' </summary>
    ''' <param name="IO_ROW"></param>
    ''' <remarks></remarks>
    Public Sub InitialT5INPRow(ByRef IO_ROW As DataRow)
        IO_ROW("LINECNT") = 0
        IO_ROW("OPERATION") = String.Empty
        IO_ROW("TIMSTP") = "0"
        IO_ROW("SELECT") = "1"
        IO_ROW("HIDDEN") = "0"
        IO_ROW("ORDERUMU") = String.Empty
        IO_ROW("EXTRACTCNT") = "0"
        IO_ROW("CTRL") = String.Empty
        IO_ROW("TWOMANTRIP") = String.Empty
        'IO_ROW("SHUKINTIME")  = String.Empty 
        'IO_ROW("JIKUKBN")  = String.Empty 
        'IO_ROW("DROPKBN")  = String.Empty 
        'IO_ROW("HAISOKBN")  = String.Empty 
        IO_ROW("CAMPCODE") = String.Empty
        IO_ROW("CAMPNAMES") = String.Empty
        IO_ROW("SHIPORG") = String.Empty
        IO_ROW("SHIPORGNAMES") = String.Empty
        IO_ROW("TERMKBN") = String.Empty
        IO_ROW("TERMKBNNAMES") = String.Empty
        IO_ROW("YMD") = String.Empty
        IO_ROW("NIPPONO") = String.Empty
        IO_ROW("HDKBN") = String.Empty
        IO_ROW("WORKKBN") = String.Empty
        IO_ROW("WORKKBNNAMES") = String.Empty
        IO_ROW("SEQ") = "000"
        IO_ROW("STAFFCODE") = String.Empty
        IO_ROW("ENTRYDATE") = String.Empty
        IO_ROW("STAFFNAMES") = String.Empty
        IO_ROW("SUBSTAFFCODE") = String.Empty
        IO_ROW("SUBSTAFFNAMES") = String.Empty
        IO_ROW("CREWKBN") = String.Empty
        IO_ROW("CREWKBNNAMES") = String.Empty
        IO_ROW("GSHABAN") = String.Empty
        IO_ROW("GSHABANLICNPLTNO") = String.Empty
        IO_ROW("STDATE") = String.Empty
        IO_ROW("STTIME") = String.Empty
        IO_ROW("ENDDATE") = String.Empty
        IO_ROW("ENDTIME") = String.Empty
        IO_ROW("WORKTIME") = "00:00"
        IO_ROW("MOVETIME") = "00:00"
        IO_ROW("ACTTIME") = "00:00"
        IO_ROW("PRATE") = "0"
        IO_ROW("CASH") = "0"
        IO_ROW("TICKET") = "0"
        IO_ROW("ETC") = "0"
        IO_ROW("TOTALTOLL") = "0"
        IO_ROW("STMATER") = "0.00"
        IO_ROW("ENDMATER") = "0.00"
        IO_ROW("RUIDISTANCE") = "0.00"
        IO_ROW("SOUDISTANCE") = "0.00"
        IO_ROW("JIDISTANCE") = "0.00"
        IO_ROW("KUDISTANCE") = "0.00"
        IO_ROW("IPPDISTANCE") = "0.00"
        IO_ROW("KOSDISTANCE") = "0.00"
        IO_ROW("IPPJIDISTANCE") = "0.00"
        IO_ROW("IPPKUDISTANCE") = "0.00"
        IO_ROW("KOSJIDISTANCE") = "0.00"
        IO_ROW("KOSKUDISTANCE") = "0.00"
        IO_ROW("KYUYU") = "0.00"
        IO_ROW("TORICODE") = String.Empty
        IO_ROW("TORINAMES") = String.Empty
        IO_ROW("SHUKABASHO") = String.Empty
        IO_ROW("SHUKABASHONAMES") = String.Empty
        IO_ROW("SHUKADATE") = String.Empty
        IO_ROW("TODOKECODE") = String.Empty
        IO_ROW("TODOKENAMES") = String.Empty
        IO_ROW("TODOKEDATE") = String.Empty
        IO_ROW("OILTYPE1") = String.Empty
        IO_ROW("PRODUCT11") = String.Empty
        IO_ROW("PRODUCT21") = String.Empty
        IO_ROW("PRODUCTCODE1") = String.Empty
        IO_ROW("PRODUCT1NAMES") = String.Empty
        IO_ROW("SURYO1") = "0.000"
        IO_ROW("STANI1") = String.Empty
        IO_ROW("STANI1NAMES") = String.Empty
        IO_ROW("OILTYPE2") = String.Empty
        IO_ROW("PRODUCT12") = String.Empty
        IO_ROW("PRODUCT22") = String.Empty
        IO_ROW("PRODUCTCODE2") = String.Empty
        IO_ROW("PRODUCT2NAMES") = String.Empty
        IO_ROW("SURYO2") = "0.000"
        IO_ROW("STANI2") = String.Empty
        IO_ROW("STANI2NAMES") = String.Empty
        IO_ROW("OILTYPE3") = String.Empty
        IO_ROW("PRODUCT13") = String.Empty
        IO_ROW("PRODUCT23") = String.Empty
        IO_ROW("PRODUCTCODE3") = String.Empty
        IO_ROW("PRODUCT3NAMES") = String.Empty
        IO_ROW("SURYO3") = "0.000"
        IO_ROW("STANI3") = String.Empty
        IO_ROW("STANI3NAMES") = String.Empty
        IO_ROW("OILTYPE4") = String.Empty
        IO_ROW("PRODUCT14") = String.Empty
        IO_ROW("PRODUCT24") = String.Empty
        IO_ROW("PRODUCTCODE4") = String.Empty
        IO_ROW("PRODUCT4NAMES") = String.Empty
        IO_ROW("SURYO4") = "0.000"
        IO_ROW("STANI4") = String.Empty
        IO_ROW("STANI4NAMES") = String.Empty
        IO_ROW("OILTYPE5") = String.Empty
        IO_ROW("PRODUCT15") = String.Empty
        IO_ROW("PRODUCT25") = String.Empty
        IO_ROW("PRODUCTCODE5") = String.Empty
        IO_ROW("PRODUCT5NAMES") = String.Empty
        IO_ROW("SURYO5") = "0.000"
        IO_ROW("STANI5") = String.Empty
        IO_ROW("STANI5NAMES") = String.Empty
        IO_ROW("OILTYPE6") = String.Empty
        IO_ROW("PRODUCT16") = String.Empty
        IO_ROW("PRODUCT26") = String.Empty
        IO_ROW("PRODUCTCODE6") = String.Empty
        IO_ROW("PRODUCT6NAMES") = String.Empty
        IO_ROW("SURYO6") = "0.000"
        IO_ROW("STANI6") = String.Empty
        IO_ROW("STANI6NAMES") = String.Empty
        IO_ROW("OILTYPE7") = String.Empty
        IO_ROW("PRODUCT17") = String.Empty
        IO_ROW("PRODUCT27") = String.Empty
        IO_ROW("PRODUCTCODE7") = String.Empty
        IO_ROW("PRODUCT7NAMES") = String.Empty
        IO_ROW("SURYO7") = "0.000"
        IO_ROW("STANI7") = String.Empty
        IO_ROW("STANI7NAMES") = String.Empty
        IO_ROW("OILTYPE8") = String.Empty
        IO_ROW("PRODUCT18") = String.Empty
        IO_ROW("PRODUCT28") = String.Empty
        IO_ROW("PRODUCTCODE8") = String.Empty
        IO_ROW("PRODUCT8NAMES") = String.Empty
        IO_ROW("SURYO8") = "0.000"
        IO_ROW("STANI8") = String.Empty
        IO_ROW("STANI8NAMES") = String.Empty
        IO_ROW("TOTALSURYO") = "0.000"
        IO_ROW("ORDERNO") = String.Empty
        IO_ROW("DETAILNO") = String.Empty
        IO_ROW("TRIPNO") = String.Empty
        IO_ROW("DROPNO") = String.Empty
        IO_ROW("JISSKIKBN") = String.Empty
        IO_ROW("JISSKIKBNNAMES") = String.Empty
        IO_ROW("URIKBN") = String.Empty
        IO_ROW("URIKBNNAMES") = String.Empty
        IO_ROW("TUMIOKIKBN") = String.Empty
        IO_ROW("TUMIOKIKBNNAMES") = String.Empty
        IO_ROW("STORICODE") = String.Empty
        IO_ROW("STORICODENAMES") = String.Empty
        IO_ROW("CONTCHASSIS") = String.Empty
        IO_ROW("CONTCHASSISLICNPLTNO") = String.Empty
        IO_ROW("SHARYOTYPEF") = String.Empty
        IO_ROW("TSHABANF") = String.Empty
        IO_ROW("SHARYOTYPEB") = String.Empty
        IO_ROW("TSHABANB") = String.Empty
        IO_ROW("SHARYOTYPEB2") = String.Empty
        IO_ROW("TSHABANB2") = String.Empty
        IO_ROW("TAXKBN") = String.Empty
        IO_ROW("TAXKBNNAMES") = String.Empty
        IO_ROW("LATITUDE") = String.Empty
        IO_ROW("LONGITUDE") = String.Empty
        IO_ROW("DELFLG") = C_DELETE_FLG.ALIVE
        IO_ROW("HOLIDAYKBN") = String.Empty
        IO_ROW("TORITYPE01") = String.Empty
        IO_ROW("TORITYPE02") = String.Empty
        IO_ROW("TORITYPE03") = String.Empty
        IO_ROW("TORITYPE04") = String.Empty
        IO_ROW("TORITYPE05") = String.Empty
        IO_ROW("SUPPLIERKBN") = String.Empty
        IO_ROW("SUPPLIER") = String.Empty
        IO_ROW("MANGOILTYPE") = String.Empty
        IO_ROW("MANGMORG1") = String.Empty
        IO_ROW("MANGSORG1") = String.Empty
        IO_ROW("MANGUORG1") = String.Empty
        IO_ROW("BASELEASE1") = String.Empty
        IO_ROW("MANGMORG2") = String.Empty
        IO_ROW("MANGSORG2") = String.Empty
        IO_ROW("MANGUORG2") = String.Empty
        IO_ROW("BASELEASE2") = String.Empty
        IO_ROW("MANGMORG3") = String.Empty
        IO_ROW("MANGSORG3") = String.Empty
        IO_ROW("MANGUORG3") = String.Empty
        IO_ROW("BASELEASE3") = String.Empty
        IO_ROW("STAFFKBN") = String.Empty
        IO_ROW("MORG") = String.Empty
        IO_ROW("HORG") = String.Empty
        IO_ROW("SUBSTAFFKBN") = String.Empty
        IO_ROW("SUBMORG") = String.Empty
        IO_ROW("SUBHORG") = String.Empty
        IO_ROW("ORDERORG") = String.Empty

        IO_ROW("wSHUKODATE") = String.Empty              '出庫日
        IO_ROW("wSHUKADATE") = String.Empty              '出荷日
        IO_ROW("wTODOKEDATE") = String.Empty             '届日
        IO_ROW("wTRIPNO_K") = String.Empty               '仮付番トリップ
        IO_ROW("wTRIPNO") = String.Empty                 'トリップ
        IO_ROW("wDROPNO") = String.Empty                 'ドロップ

        IO_ROW("wTORICODE") = String.Empty               '荷主コード
        IO_ROW("wURIKBN") = String.Empty                 '売上計上基準
        IO_ROW("wSTORICODE") = String.Empty              '販売店コード
        IO_ROW("wTODOKECODE") = String.Empty             '届先コード
        IO_ROW("wSHUKABASHO") = String.Empty             '出荷場所

        IO_ROW("wCREWKBN") = String.Empty                '正副区分
        IO_ROW("wSTAFFKBN") = String.Empty               '社員区分
        IO_ROW("wSTAFFCODE") = String.Empty              '従業員コード
        IO_ROW("wSUBSTAFFCODE") = String.Empty           '従業員コード（副）

        IO_ROW("wORDERNO") = String.Empty                '受注番号
        IO_ROW("wDETAILNO") = String.Empty               '明細№
        IO_ROW("wORDERORG") = String.Empty               '受注部署

        IO_ROW("wKAISO") = String.Empty                  '回送
        IO_ROW("wKUSHAKBN") = String.Empty

        IO_ROW("wTRIPDROPcnt") = String.Empty
        IO_ROW("wDATECHANGE") = String.Empty
        IO_ROW("wLASTstat") = String.Empty
        IO_ROW("wFirstCNTUP") = String.Empty
        IO_ROW("wF1F3flg") = String.Empty

        IO_ROW("wIPPDISTANCE") = "0"           '一般
        IO_ROW("wKOSDISTANCE") = "0"           '高速

        IO_ROW("wIPPJIDISTANCE") = "0"         '一般実車
        IO_ROW("wIPPKUDISTANCE") = "0"         '一般空車

        IO_ROW("wKOSJIDISTANCE") = "0"         '高速実車
        IO_ROW("wKOSKUDISTANCE") = "0"         '高速空車

        IO_ROW("wWORKTIME") = String.Empty               '作業分
        IO_ROW("wMOVETIME") = String.Empty               '移動分
        IO_ROW("wACTTIME") = String.Empty                '稼働分

        IO_ROW("wJIMOVETIME") = String.Empty             '実車走行分
        IO_ROW("wKUMOVETIME") = String.Empty             '空車走行分
        IO_ROW("wKAIJI") = String.Empty                  '回次
        IO_ROW("wSUISOKBN") = String.Empty               '水素区分

        IO_ROW("wShachuhaku") = String.Empty            '車中泊
        IO_ROW("wHaisoGroup") = String.Empty            '配送グループ分類
    End Sub

    ''' <summary>
    ''' カレンダーデータの休暇区分を取得する
    ''' </summary>
    ''' <param name="I_ROW">チェック対象行</param>
    ''' <param name="O_HOLIDAYKBN">休暇区分</param>
    ''' <remarks></remarks>
    Private Sub GetHolidayKbn(ByVal I_ROW As DataRow, ByRef O_HOLIDAYKBN As String)

        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     "SELECT isnull(rtrim(WORKINGKBN),'') as WORKINGKBN " _
                   & " FROM  MB005_CALENDAR " _
                   & " Where CAMPCODE   = @CAMPCODE " _
                   & "   and WORKINGYMD = @WORKINGYMD " _
                   & "   and DELFLG    <> @DELFLG "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_WORKINGYMD As SqlParameter = SQLcmd.Parameters.Add("@WORKINGYMD", System.Data.SqlDbType.Date)
                    Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
                    P_CAMPCODE.Value = I_ROW("CAMPCODE")
                    P_WORKINGYMD.Value = I_ROW("YMD")
                    P_DELFLG.Value = C_DELETE_FLG.DELETE

                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            '○出力編集
                            O_HOLIDAYKBN = SQLdr("WORKINGKBN")
                        End While

                    End Using

                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GetHOLIDAYKBN"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MB005_CALENDAR Select"         '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 車番部署マスタの（勤怠用項目）取得 
    ''' </summary>
    ''' <param name="I_ROW"></param>
    ''' <param name="O_OILKBN"></param>
    ''' <param name="O_SHARYOKBN"></param>
    ''' <remarks></remarks>
    Private Sub GetMA006(ByVal I_ROW As DataRow, ByRef O_OILKBN As String, ByRef O_SHARYOKBN As String)

        O_OILKBN = String.Empty
        O_SHARYOKBN = String.Empty
        Try
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                Dim SQLStr As String =
                     "SELECT isnull(rtrim(OILKBN),'') as OILKBN " _
                   & "      ,isnull(rtrim(SHARYOKBN),'') as SHARYOKBN " _
                   & " FROM  MA006_SHABANORG " _
                   & " Where CAMPCODE   = @CAMPCODE " _
                   & "   and MANGUORG   = @MANGUORG " _
                   & "   and GSHABAN    = @GSHABAN " _
                   & "   and DELFLG    <> @DELFLG "

                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_MANGUORG As SqlParameter = SQLcmd.Parameters.Add("@MANGUORG", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_GSHABAN As SqlParameter = SQLcmd.Parameters.Add("@GSHABAN", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
                    P_CAMPCODE.Value = I_ROW("CAMPCODE")
                    P_MANGUORG.Value = I_ROW("SHIPORG")
                    P_GSHABAN.Value = I_ROW("GSHABAN")
                    P_DELFLG.Value = C_DELETE_FLG.DELETE
                    SQLcmd.CommandTimeout = 300
                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            '○出力編集
                            O_OILKBN = SQLdr("OILKBN")
                            O_SHARYOKBN = SQLdr("SHARYOKBN")
                        End While

                    End Using

                End Using
            End Using
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GetMA006"                     'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:MA006_SHABANORG Select"        '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' T00005データ整備
    ''' </summary>
    ''' <param name="I_T5tbl"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub ReEditT0005(ByRef I_T5tbl As DataTable, ByRef O_RTN As String)
        If I_T5tbl.Rows.Count > 0 Then ReEditT0005(I_T5tbl, I_T5tbl.Rows(0).Item("CAMPCODE"), O_RTN)
    End Sub
    ''' <summary>
    ''' T00005データ整備
    ''' </summary>
    ''' <param name="I_T5tbl"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub ReEditT0005(ByRef I_T5tbl As DataTable, ByVal I_COMPANYCODE As String, ByRef O_RTN As String)

        'DataBase接続文字
        Using SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '〇データ整備（カレンダ）　…　休日区分取得
            ReEditCalendarT0005(I_T5tbl, SQLcon)

            '〇データ整備（乗務員）　…　組織、社員区分、名前取得
            ReEditStaffT0005(I_T5tbl, SQLcon)

            '〇データ整備（業務車番、日報番号）
            ReEditGShabanT0005(I_T5tbl, SQLcon)

            '〇データ整備（取引先情報）
            ReEditToriDataT0005(I_T5tbl, SQLcon)

            '〇データ整備（当日の最初の荷積荷卸を調べる）　2017/7/1追加
            If I_COMPANYCODE = GRT00005WRKINC.C_COMP_NJS Then
                'NJSの場合
                ReEditTumiOkiNJST0005(I_COMPANYCODE, I_T5tbl)
            Else
                'NJS以外の場合
                ReEditTumiOkiT0005(I_T5tbl)
            End If

            '〇データ整備（トリップ・ドロップ再付番）
            If I_COMPANYCODE = GRT00005WRKINC.C_COMP_NJS Then
                'NJSの場合
                ReEditTripDropT0005NJS(I_T5tbl)
            ElseIf I_COMPANYCODE = GRT00005WRKINC.C_COMP_JKT Then
                'JKTの場合
                ReEditTripDropT0005JKT(I_T5tbl)
            Else
                'NJS、JKT以外の場合
                ReEditTripDropT0005(I_T5tbl)
            End If

            '〇データ整備（トリップ・ドロップ全明細反映）
            ReEditTripDropT0005Detail(I_T5tbl)

            '〇データ整備（実車空車判定1）　…　朝一～初回荷積
            ReEditFirstKushaT0005(I_T5tbl)

            '〇データ整備（実車空車判定2）　…　最終荷卸一～出庫日最終　or 同一トリップ内の最終荷積荷卸直後～最終作業（荷積荷卸以外）
            ReEditNextKushaT0005(I_T5tbl)

            '〇データ整備（回送判定＋距離・時間設定）　…　最終荷卸一～出庫日最終　or 同一トリップ内の最終荷積荷卸直後～最終作業（荷積荷卸以外）
            If I_COMPANYCODE = GRT00005WRKINC.C_COMP_NJS Then
                ReEditKaisoT0005NJS(I_T5tbl)
            Else
                ReEditKaisoT0005(I_T5tbl)
            End If

            '〇データ整備（配送時間・距離補正1）　…　帰庫に隠れている配送時間および走行距離、求める
            ReEditDistance1T0005(I_T5tbl)

            '〇データ整備（配送時間・距離補正2）　…　荷積に表現されている配送時間および走行距離は、直前作業に含まれる
            ReEditDistance2T0005(I_T5tbl)

            '〇データ整備（配送時間・距離補正3）　…　帰庫以降に表現されている配送時間および走行距離は、帰庫作業に含める
            ReEditDistance3T0005(I_T5tbl)

            '〇データ整備（配送時間・距離補正4）　…　休憩(BB)・他作業(BX)に表現されている配送時間および走行距離は、直後作業に含まれる
            ReEditDistance4T0005(I_T5tbl)

            ' ***  T0005データ整備（回次設定）
            ReEditKaijiT0005(I_T5tbl)

        End Using
    End Sub

    ''' <summary>
    '''  T0005データ整備（カレンダ）
    ''' </summary>
    ''' <param name="IO_T5tbl">編集対象テーブル</param>
    ''' <param name="I_SQLcon">DBコネクション</param>
    ''' <remarks></remarks>
    Public Sub ReEditCalendarT0005(ByRef IO_T5tbl As DataTable, ByVal I_SQLcon As SqlConnection)

        Dim wCAMPCODE As String = String.Empty
        Dim wYMD As String = String.Empty

        Dim wHOLIDAYKBN As String               '休日区分

        '〇カレンダ処理
        'ソート
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, YMD"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（業務車番）
        wCAMPCODE = String.Empty
        wYMD = String.Empty
        wHOLIDAYKBN = String.Empty                         '休日区分


        Using WW_I_T5tbl As DataTable = IO_T5tbl.Clone

            Dim SQLStr As String =
                      " SELECT isnull(rtrim(WORKINGKBN),'') as WORKINGKBN " _
                    & " FROM  MB005_CALENDAR " _
                    & " WHERE CAMPCODE   = @CAMPCODE " _
                    & "   AND WORKINGYMD = @WORKINGYMD " _
                    & "   AND DELFLG    <> '1' "

            Using SQLcmd As New SqlCommand(SQLStr, I_SQLcon)
                SQLcmd.CommandTimeout = 300

                Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
                Dim P_WORKINGYMD As SqlParameter = SQLcmd.Parameters.Add("@WORKINGYMD", System.Data.SqlDbType.Date)

                For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
                    Dim T0005row As DataRow = WW_I_T5tbl.NewRow
                    T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

                    If IsDBNull(T0005row("YMD")) Then
                        T0005row("YMD") = String.Empty
                    End If

                    If T0005row("YMD") = String.Empty Then
                        T0005row("HOLIDAYKBN") = String.Empty
                    Else
                        '〇会社か日付が異なる場合再取得する
                        If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse (T0005row("YMD") <> wYMD) Then
                            wCAMPCODE = T0005row("CAMPCODE")
                            wYMD = T0005row("YMD")

                            wHOLIDAYKBN = String.Empty

                            'カレンダ検索
                            Try
                                P_CAMPCODE.Value = wCAMPCODE
                                P_WORKINGYMD.Value = wYMD

                                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                                While SQLdr.Read
                                    wHOLIDAYKBN = SQLdr("WORKINGKBN")
                                End While

                                'Close
                                SQLdr.Close() 'Reader(Close)
                                SQLdr = Nothing

                            Catch ex As Exception
                                CS0011LOGWRITE.INFSUBCLASS = "T0005REEDIT_CALENDAR"                     'SUBクラス名
                                CS0011LOGWRITE.INFPOSI = "DB:MB005_CALENDAR Select"        '
                                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                                CS0011LOGWRITE.TEXT = ex.ToString()
                                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                            End Try
                        End If

                        T0005row("HOLIDAYKBN") = wHOLIDAYKBN

                    End If
                    WW_I_T5tbl.Rows.Add(T0005row)
                Next
            End Using

            IO_T5tbl = WW_I_T5tbl.Copy
        End Using
    End Sub

    ''' <summary>
    '''  T0005データ整備（乗務員）
    ''' </summary>
    ''' <param name="IO_T5tbl">編集対象テーブル</param>
    ''' <param name="I_SQLcon">DBコネクション</param>
    ''' <remarks></remarks>
    Public Sub ReEditStaffT0005(ByRef IO_T5tbl As DataTable, ByVal I_SQLcon As SqlConnection)

        Dim wCAMPCODE As String = String.Empty
        Dim wSTAFFCODE As String = String.Empty
        Dim wYMD As String = String.Empty

        Dim wMB1_MORG As String = String.Empty                  '管理部署
        Dim wMB1_HORG As String = String.Empty                  '配属部署
        Dim wMB1_STAFFKBN As String = String.Empty              '社員区分
        Dim wMB1_STAFFNAMES As String = String.Empty            '社員名称

        '〇正乗務員処理
        'ソート
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, STAFFCODE, YMD"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        'データ整備（業務車番）
        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone

        Dim SQLStr As String =
           " SELECT isnull(rtrim(A.MORG),'') as MORG, isnull(rtrim(A.HORG),'') as HORG, isnull(rtrim(A.STAFFKBN),'') as STAFFKBN, isnull(rtrim(A.STAFFNAMES),'') as STAFFNAMES " _
         & "     FROM MB001_STAFF A " _
         & "     WHERE   A.CAMPCODE    = @P01 " _
         & "       and   A.STAFFCODE   = @P02 " _
         & "       and   A.STYMD      <= @P04 " _
         & "       and   A.ENDYMD     >= @P03 " _
         & "       and   A.DELFLG     <> '1' "

        Using SQLcmd As New SqlCommand(SQLStr, I_SQLcon)

            SQLcmd.CommandTimeout = 300
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)

            For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
                Dim T0005row As DataRow = WW_I_T5tbl.NewRow
                T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray
                '〇BDNULLの初期化
                T0005row("STAFFCODE") = If(IsDBNull(T0005row("STAFFCODE")), String.Empty, T0005row("STAFFCODE"))
                If String.IsNullOrEmpty(T0005row("STAFFCODE")) Then
                    T0005row("MORG") = String.Empty
                    T0005row("HORG") = String.Empty
                    T0005row("STAFFKBN") = String.Empty
                    T0005row("STAFFNAMES") = String.Empty
                Else
                    '対象（始業、終業以外）
                    '会社コードか乗務員が変更された場合に処理する
                    If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso T0005row("STAFFCODE") = wSTAFFCODE) Then
                        wCAMPCODE = T0005row("CAMPCODE")
                        wSTAFFCODE = T0005row("STAFFCODE")
                        wYMD = T0005row("YMD")

                        wMB1_MORG = String.Empty
                        wMB1_HORG = String.Empty
                        wMB1_STAFFKBN = String.Empty
                        wMB1_STAFFNAMES = String.Empty

                        '従業員マスタ検索
                        Try
                            PARA1.Value = wCAMPCODE
                            PARA2.Value = wSTAFFCODE
                            PARA3.Value = wYMD
                            PARA4.Value = wYMD
                            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            If SQLdr.Read Then
                                wMB1_MORG = SQLdr("MORG")
                                wMB1_HORG = SQLdr("HORG")
                                wMB1_STAFFKBN = SQLdr("STAFFKBN")
                                wMB1_STAFFNAMES = SQLdr("STAFFNAMES")
                            End If

                            'Close
                            SQLdr.Close() 'Reader(Close)
                            SQLdr = Nothing

                        Catch ex As Exception
                            CS0011LOGWRITE.INFSUBCLASS = "T0005REEDIT_STAFF"        'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"        '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力

                        End Try

                    End If

                    T0005row("MORG") = wMB1_MORG
                    T0005row("HORG") = wMB1_HORG
                    T0005row("STAFFKBN") = wMB1_STAFFKBN
                    T0005row("STAFFNAMES") = wMB1_STAFFNAMES

                End If

                WW_I_T5tbl.Rows.Add(T0005row)
            Next

            IO_T5tbl = WW_I_T5tbl.Copy

            '〇副乗務員処理
            'ソート
            CS0026TBLSort.TABLE = IO_T5tbl
            CS0026TBLSort.SORTING = "CAMPCODE, SUBSTAFFCODE, YMD"
            CS0026TBLSort.FILTER = String.Empty
            IO_T5tbl = CS0026TBLSort.sort()

            wCAMPCODE = String.Empty
            wSTAFFCODE = String.Empty
            wYMD = String.Empty
            wMB1_MORG = String.Empty                           '管理部署
            wMB1_HORG = String.Empty                           '配属部署
            wMB1_STAFFKBN = String.Empty                       '社員区分
            wMB1_STAFFNAMES = String.Empty                     '社員名称

            'データ整備（業務車番）
            WW_I_T5tbl.Clear()
            For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
                Dim T0005row As DataRow = WW_I_T5tbl.NewRow
                T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

                '〇BDNULLの初期化
                T0005row("SUBSTAFFCODE") = If(IsDBNull(T0005row("SUBSTAFFCODE")), String.Empty, T0005row("SUBSTAFFCODE"))
                '副乗務員がいない場合は初期化
                If String.IsNullOrEmpty(T0005row("SUBSTAFFCODE")) Then
                    T0005row("SUBMORG") = String.Empty
                    T0005row("SUBHORG") = String.Empty
                    T0005row("SUBSTAFFKBN") = String.Empty
                    T0005row("SUBSTAFFNAMES") = String.Empty
                Else
                    '会社または副乗務員が異なる場合再取得する
                    If T0005row("CAMPCODE") <> wCAMPCODE OrElse T0005row("SUBSTAFFCODE") <> wSTAFFCODE Then
                        wCAMPCODE = T0005row("CAMPCODE")
                        wSTAFFCODE = T0005row("SUBSTAFFCODE")
                        wYMD = T0005row("YMD")

                        wMB1_MORG = String.Empty
                        wMB1_HORG = String.Empty
                        wMB1_STAFFKBN = String.Empty
                        wMB1_STAFFNAMES = String.Empty

                        '従業員マスタ検索
                        Try
                            PARA1.Value = wCAMPCODE
                            PARA2.Value = wSTAFFCODE
                            PARA3.Value = wYMD
                            PARA4.Value = wYMD
                            SQLcmd.CommandTimeout = 300
                            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            If SQLdr.Read Then
                                wMB1_MORG = SQLdr("MORG")
                                wMB1_HORG = SQLdr("HORG")
                                wMB1_STAFFKBN = SQLdr("STAFFKBN")
                                wMB1_STAFFNAMES = SQLdr("STAFFNAMES")
                            End If

                            'Close
                            SQLdr.Close() 'Reader(Close)
                            SQLdr = Nothing

                        Catch ex As Exception
                            CS0011LOGWRITE.INFSUBCLASS = "T0005REEDIT_STAFF"                     'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:MB001_STAFF Select"        '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                        End Try

                    End If

                    T0005row("SUBMORG") = wMB1_MORG
                    T0005row("SUBHORG") = wMB1_HORG
                    T0005row("SUBSTAFFKBN") = wMB1_STAFFKBN
                    T0005row("SUBSTAFFNAMES") = wMB1_STAFFNAMES

                End If
                WW_I_T5tbl.Rows.Add(T0005row)
            Next

        End Using

        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing
    End Sub

    ''' <summary>
    ''' T0005データ整備（業務車番）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <param name="I_SQLcon"></param>
    ''' <remarks></remarks>
    Public Sub ReEditGShabanT0005(ByRef IO_T5tbl As DataTable, ByVal I_SQLcon As SqlConnection)

        Dim wCAMPCODE As String = String.Empty
        Dim wSTAFFCODE As String = String.Empty
        Dim wNIPPONO As String = String.Empty
        Dim wYMD As String = String.Empty
        Dim wGSHABAN As String = String.Empty
        Dim wSHIPORG As String = String.Empty

        Dim wMA6_GSHABAN As String = String.Empty
        Dim wMA6_MANGSUPPL As String = String.Empty
        Dim wMA6_OILTYPE As String = String.Empty
        Dim wMA6_SHARYOTYPEF As String = String.Empty
        Dim wMA6_TSHABANF As String = String.Empty
        Dim wMA6_SHARYOTYPEB As String = String.Empty
        Dim wMA6_TSHABANB As String = String.Empty
        Dim wMA6_SHARYOTYPEB2 As String = String.Empty
        Dim wMA6_TSHABANB2 As String = String.Empty
        Dim wMA6_MANGMORGF As String = String.Empty
        Dim wMA6_MANGSORGF As String = String.Empty
        Dim wMA6_MANGMORGB As String = String.Empty
        Dim wMA6_MANGSORGB As String = String.Empty
        Dim wMA6_MANGMORG2 As String = String.Empty
        Dim wMA6_MANGSORG2 As String = String.Empty
        Dim wMA6_BASELEASEF As String = String.Empty
        Dim wMA6_BASELEASEB As String = String.Empty
        Dim wMA6_BASELEASE2 As String = String.Empty
        Dim wMA6_SUISOKBN As String = String.Empty

        '〇T0005データ整備（マスタ項目設定）

        'ソート（乗務員、出庫日、日報、開始日時）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, SHIPORG, YMD"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone

        'DB検索
        Dim SQLStr As String =
                 " SELECT isnull(rtrim(A.GSHABAN),'') 		as GSHABAN ,   		    " _
               & "        isnull(rtrim(A.MANGSUPPL),'')     as MANGSUPPL ,          " _
               & "        isnull(rtrim(B.MANGOILTYPE),'') 	as OILTYPE ,            " _
               & "        isnull(rtrim(A.SHARYOTYPEF),'')   as SHARYOTYPEF ,        " _
               & "        isnull(rtrim(A.TSHABANF),'')      as TSHABANF ,           " _
               & "        isnull(rtrim(A.SHARYOTYPEB),'')   as SHARYOTYPEB ,        " _
               & "        isnull(rtrim(A.TSHABANB),'')      as TSHABANB ,           " _
               & "        isnull(rtrim(A.SHARYOTYPEB2),'')  as SHARYOTYPEB2 ,       " _
               & "        isnull(rtrim(A.TSHABANB2),'')     as TSHABANB2 ,          " _
               & "        isnull(rtrim(B.BASELEASE),'')     as BASELEASEF ,         " _
               & "        isnull(rtrim(C.BASELEASE),'')     as BASELEASEB ,         " _
               & "        isnull(rtrim(D.BASELEASE),'')     as BASELEASE2 ,         " _
               & "        isnull(rtrim(B.MANGMORG),'')      as MANGMORGF ,          " _
               & "        isnull(rtrim(B.MANGSORG),'')      as MANGSORGF ,          " _
               & "        isnull(rtrim(C.MANGMORG),'')      as MANGMORGB ,          " _
               & "        isnull(rtrim(C.MANGSORG),'')      as MANGSORGB ,          " _
               & "        isnull(rtrim(D.MANGMORG),'')      as MANGMORG2 ,          " _
               & "        isnull(rtrim(D.MANGSORG),'')      as MANGSORG2 ,          " _
               & "        isnull(rtrim(A.SUISOKBN),'')      as SUISOKBN             " _
               & "   FROM MA006_SHABANORG   as A                        " _
               & "   LEFT JOIN MA002_SHARYOA B 						    " _
               & "     ON B.CAMPCODE   	= A.CAMPCODE 				    " _
               & "    and B.SHARYOTYPE  = A.SHARYOTYPEF 		        " _
               & "    and B.TSHABAN     = A.TSHABANF 		            " _
               & "    and B.STYMD      <= @P1                           " _
               & "    and B.ENDYMD     >= @P1                           " _
               & "    and B.DELFLG     <> '1' 						    " _
               & "   LEFT JOIN MA002_SHARYOA C 						    " _
               & "     ON C.CAMPCODE   	= A.CAMPCODE 				    " _
               & "    and C.SHARYOTYPE  = A.SHARYOTYPEB 		        " _
               & "    and C.TSHABAN     = A.TSHABANB 		            " _
               & "    and C.STYMD      <= @P1                           " _
               & "    and C.ENDYMD     >= @P1                           " _
               & "    and C.DELFLG     <> '1' 						    " _
               & "   LEFT JOIN MA002_SHARYOA D 						    " _
               & "     ON D.CAMPCODE   	= A.CAMPCODE 				    " _
               & "    and D.SHARYOTYPE  = A.SHARYOTYPEB2 		        " _
               & "    and D.TSHABAN     = A.TSHABANB2 		            " _
               & "    and D.STYMD      <= @P1                           " _
               & "    and D.ENDYMD     >= @P1                           " _
               & "    and D.DELFLG     <> '1' 						    " _
               & "   LEFT JOIN MA003_SHARYOB F 						    " _
               & "     ON F.CAMPCODE  　= A.CAMPCODE 				    " _
               & "    and F.SHARYOTYPE  = A.SHARYOTYPEF 		        " _
               & "    and F.TSHABAN     = A.TSHABANF 			        " _
               & "    and F.STYMD      <= @P1                           " _
               & "    and F.ENDYMD     >= @P1                           " _
               & "    and F.DELFLG     <> '1' 						    " _
               & "   LEFT JOIN MA003_SHARYOB G 						    " _
               & "     ON G.CAMPCODE   	= A.CAMPCODE 				    " _
               & "    and G.SHARYOTYPE  = A.SHARYOTYPEB 		        " _
               & "    and G.TSHABAN     = A.TSHABANB 	                " _
               & "    and G.STYMD      <= @P1                           " _
               & "    and G.ENDYMD     >= @P1                           " _
               & "    and G.DELFLG     <> '1' 						    " _
               & "   LEFT JOIN MA003_SHARYOB H 						    " _
               & "     ON H.CAMPCODE   	= A.CAMPCODE 				    " _
               & "    and H.SHARYOTYPE  = A.SHARYOTYPEB2 		        " _
               & "    and H.TSHABAN     = A.TSHABANB2 	                " _
               & "    and H.STYMD      <= @P1                           " _
               & "    and H.ENDYMD     >= @P1                           " _
               & "    and H.DELFLG     <> '1' 						    " _
               & "  Where A.CAMPCODE  = @P2                             " _
               & "    and A.MANGUORG  = @P3                             " _
               & "    and A.GSHABAN   = @P4                             " _
               & "    and A.DELFLG   <> '1'                             " _
               & "  ORDER BY A.SEQ ,A.GSHABAN                           "

        Using SQLcmd As New SqlCommand(SQLStr, I_SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 20)

            For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
                Dim T0005row As DataRow = WW_I_T5tbl.NewRow
                T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray
                If IsDBNull(T0005row("GSHABAN")) Then T0005row("GSHABAN") = String.Empty

                '対象（始業、終業以外）
                If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") OrElse String.IsNullOrEmpty(T0005row("GSHABAN")) Then
                    T0005row("SUPPLIERKBN") = String.Empty
                    T0005row("SUPPLIER") = String.Empty
                    T0005row("MANGOILTYPE") = String.Empty
                    T0005row("SHARYOTYPEF") = String.Empty
                    T0005row("TSHABANF") = String.Empty
                    T0005row("MANGMORG1") = String.Empty
                    T0005row("MANGSORG1") = String.Empty
                    T0005row("BASELEASE1") = String.Empty
                    T0005row("SHARYOTYPEB") = String.Empty
                    T0005row("TSHABANB") = String.Empty
                    T0005row("MANGMORG2") = String.Empty
                    T0005row("MANGSORG2") = String.Empty
                    T0005row("BASELEASE2") = String.Empty
                    T0005row("SHARYOTYPEB2") = String.Empty
                    T0005row("TSHABANB2") = String.Empty
                    T0005row("MANGMORG3") = String.Empty
                    T0005row("MANGSORG3") = String.Empty
                    T0005row("BASELEASE3") = String.Empty
                    T0005row("wSUISOKBN") = String.Empty
                Else
                    If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse (T0005row("GSHABAN") <> wGSHABAN) OrElse (T0005row("SHIPORG") <> wSHIPORG) Then
                        wCAMPCODE = T0005row("CAMPCODE")
                        wGSHABAN = T0005row("GSHABAN")
                        wSHIPORG = T0005row("SHIPORG")
                        wYMD = T0005row("YMD")

                        Try
                            wMA6_GSHABAN = String.Empty
                            wMA6_MANGSUPPL = String.Empty
                            wMA6_OILTYPE = String.Empty
                            wMA6_SHARYOTYPEF = String.Empty
                            wMA6_TSHABANF = String.Empty
                            wMA6_SHARYOTYPEB = String.Empty
                            wMA6_TSHABANB = String.Empty
                            wMA6_SHARYOTYPEB2 = String.Empty
                            wMA6_TSHABANB2 = String.Empty
                            wMA6_MANGMORGF = String.Empty
                            wMA6_MANGSORGF = String.Empty
                            wMA6_MANGMORGB = String.Empty
                            wMA6_MANGSORGB = String.Empty
                            wMA6_MANGMORG2 = String.Empty
                            wMA6_MANGSORG2 = String.Empty
                            wMA6_BASELEASEF = String.Empty
                            wMA6_BASELEASEB = String.Empty
                            wMA6_BASELEASE2 = String.Empty
                            wMA6_SUISOKBN = String.Empty

                            PARA1.Value = wYMD
                            PARA2.Value = wCAMPCODE
                            PARA3.Value = wSHIPORG
                            PARA4.Value = wGSHABAN

                            '○SQL実行
                            SQLcmd.CommandTimeout = 300
                            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            '○出力設定
                            If SQLdr.Read Then

                                wMA6_GSHABAN = SQLdr("GSHABAN")
                                wMA6_MANGSUPPL = SQLdr("MANGSUPPL")
                                wMA6_OILTYPE = SQLdr("OILTYPE")
                                wMA6_SHARYOTYPEF = SQLdr("SHARYOTYPEF")
                                wMA6_TSHABANF = SQLdr("TSHABANF")
                                wMA6_SHARYOTYPEB = SQLdr("SHARYOTYPEB")
                                wMA6_TSHABANB = SQLdr("TSHABANB")
                                wMA6_SHARYOTYPEB2 = SQLdr("SHARYOTYPEB2")
                                wMA6_TSHABANB2 = SQLdr("TSHABANB2")
                                wMA6_MANGMORGF = SQLdr("MANGMORGF")
                                wMA6_MANGSORGF = SQLdr("MANGSORGF")
                                wMA6_MANGMORGB = SQLdr("MANGMORGB")
                                wMA6_MANGSORGB = SQLdr("MANGSORGB")
                                wMA6_MANGMORG2 = SQLdr("MANGMORG2")
                                wMA6_MANGSORG2 = SQLdr("MANGSORG2")
                                wMA6_BASELEASEF = SQLdr("BASELEASEF")
                                wMA6_BASELEASEB = SQLdr("BASELEASEB")
                                wMA6_BASELEASE2 = SQLdr("BASELEASE2")
                                wMA6_SUISOKBN = SQLdr("SUISOKBN")

                            End If

                            'Close()
                            SQLdr.Close() 'Reader(Close)
                            SQLdr = Nothing

                        Catch ex As Exception
                            CS0011LOGWRITE.INFSUBCLASS = "T0005REEDIT_STAFF"                     'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:MA002_SHARYOA Select"        '
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                        End Try

                    End If

                    If String.IsNullOrEmpty(wMA6_MANGSUPPL) Then                                            '社有・庸車区分
                        T0005row("SUPPLIERKBN") = "1"
                    Else
                        T0005row("SUPPLIERKBN") = "2"
                    End If
                    T0005row("SUPPLIER") = wMA6_MANGSUPPL                                       '庸車会社
                    T0005row("MANGOILTYPE") = wMA6_OILTYPE                                      '車両登録油種
                    T0005row("SHARYOTYPEF") = wMA6_SHARYOTYPEF                                  '統一車番(上)1
                    T0005row("TSHABANF") = wMA6_TSHABANF                                        '統一車番(下)1
                    T0005row("MANGMORG1") = wMA6_MANGMORGF                                      '車両管理部署1
                    T0005row("MANGSORG1") = wMA6_MANGSORGF                                      '車両設置部署1
                    T0005row("BASELEASE1") = wMA6_BASELEASEF                                    '車両所有1
                    T0005row("SHARYOTYPEB") = wMA6_SHARYOTYPEB                                  '統一車番(上)2
                    T0005row("TSHABANB") = wMA6_TSHABANB                                        '統一車番(下)2
                    T0005row("MANGMORG2") = wMA6_MANGMORGB                                      '車両管理部署2
                    T0005row("MANGSORG2") = wMA6_MANGSORGB                                      '車両設置部署2
                    T0005row("BASELEASE2") = wMA6_BASELEASEB                                    '車両所有2
                    T0005row("SHARYOTYPEB2") = wMA6_SHARYOTYPEB2                                '統一車番(上)3
                    T0005row("TSHABANB2") = wMA6_TSHABANB2                                      '統一車番(下)3
                    T0005row("MANGMORG3") = wMA6_MANGMORG2                                      '車両管理部署3
                    T0005row("MANGSORG3") = wMA6_MANGSORG2                                      '車両設置部署3
                    T0005row("BASELEASE3") = wMA6_BASELEASE2                                    '車両所有3
                    T0005row("wSUISOKBN") = wMA6_SUISOKBN                                       '水素区分

                End If
                WW_I_T5tbl.Rows.Add(T0005row)
            Next
        End Using
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing
    End Sub

    ''' <summary>
    ''' T0005データ整備（取引先情報）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <param name="I_SQLcon"></param>
    ''' <remarks></remarks>
    Public Sub ReEditToriDataT0005(ByRef IO_T5tbl As DataTable, ByVal I_SQLcon As SqlConnection)

        Dim wCAMPCODE As String = String.Empty
        Dim wYMD As String = String.Empty
        Dim wTORICODE As String = String.Empty
        Dim wSHIPORG As String = String.Empty

        Dim wMC3_STORICODE As String = String.Empty
        Dim wMC3_TORITYPE01 As String = String.Empty
        Dim wMC3_TORITYPE02 As String = String.Empty
        Dim wMC3_TORITYPE03 As String = String.Empty
        Dim wMC3_TORITYPE04 As String = String.Empty
        Dim wMC3_TORITYPE05 As String = String.Empty

        '〇T0005データ整備（取引先情報）

        'ソート（取引先、出庫日、日報、開始日時）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, TORICODE, SHIPORG, YMD"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone

        Dim SQLStr As String =
                         " SELECT " _
                       & "       isnull(rtrim(TORITYPE01),'') as TORITYPE01     " _
                       & "     , isnull(rtrim(TORITYPE02),'') as TORITYPE02     " _
                       & "     , isnull(rtrim(TORITYPE03),'') as TORITYPE03     " _
                       & "     , isnull(rtrim(TORITYPE04),'') as TORITYPE04     " _
                       & "     , isnull(rtrim(TORITYPE05),'') as TORITYPE05     " _
                       & "     , isnull(rtrim(STORICODE),'')  as STORICODE      " _
                       & " FROM  MC003_TORIORG                                  " _
                       & " Where TORICODE   = @P1                               " _
                       & "    and CAMPCODE   = @P2                              " _
                       & "    and UORG       = @P3                              " _
                       & "    and DELFLG    <> '1'                              "

        Using SQLcmd As New SqlCommand(SQLStr, I_SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar, 20)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar, 20)
            SQLcmd.CommandTimeout = 300

            For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
                Dim T0005row As DataRow = WW_I_T5tbl.NewRow
                T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

                If T0005row("HDKBN") = "H" OrElse
                    T0005row("WORKKBN") = "A1" OrElse
                    T0005row("WORKKBN") = "Z1" OrElse
                    IsNothing(T0005row("TORICODE")) Then

                    T0005row("STORICODE") = String.Empty
                    T0005row("TORITYPE01") = String.Empty
                    T0005row("TORITYPE02") = String.Empty
                    T0005row("TORITYPE03") = String.Empty
                    T0005row("TORITYPE04") = String.Empty
                    T0005row("TORITYPE05") = String.Empty
                Else  '対象（始業、終業以外）

                    If T0005row("CAMPCODE") <> wCAMPCODE OrElse
                       T0005row("TORICODE") <> wTORICODE OrElse
                       T0005row("SHIPORG") <> wSHIPORG Then
                        wCAMPCODE = T0005row("CAMPCODE")
                        wTORICODE = T0005row("TORICODE")
                        wSHIPORG = T0005row("SHIPORG")

                        Try
                            wMC3_STORICODE = String.Empty
                            wMC3_TORITYPE01 = String.Empty
                            wMC3_TORITYPE02 = String.Empty
                            wMC3_TORITYPE03 = String.Empty
                            wMC3_TORITYPE04 = String.Empty
                            wMC3_TORITYPE05 = String.Empty

                            'DB検索

                            PARA1.Value = wTORICODE
                            PARA2.Value = wCAMPCODE
                            PARA3.Value = wSHIPORG

                            '○SQL実行
                            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                            '○出力設定
                            If SQLdr.Read Then

                                wMC3_STORICODE = SQLdr("STORICODE")
                                wMC3_TORITYPE01 = SQLdr("TORITYPE01")
                                wMC3_TORITYPE02 = SQLdr("TORITYPE02")
                                wMC3_TORITYPE03 = SQLdr("TORITYPE03")
                                wMC3_TORITYPE04 = SQLdr("TORITYPE04")
                                wMC3_TORITYPE05 = SQLdr("TORITYPE05")
                            End If

                            'Close()
                            SQLdr.Close() 'Reader(Close)
                            SQLdr = Nothing

                        Catch ex As Exception
                            CS0011LOGWRITE.INFSUBCLASS = "T0005REEDIT_TORI"                     'SUBクラス名
                            CS0011LOGWRITE.INFPOSI = "DB:MC003_TORIORG Select"
                            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                            CS0011LOGWRITE.TEXT = ex.ToString()
                            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                        End Try

                    End If

                    T0005row("STORICODE") = wMC3_STORICODE                                      '請求取引先
                    T0005row("TORITYPE01") = wMC3_TORITYPE01                                    '取引先タイプ１
                    T0005row("TORITYPE02") = wMC3_TORITYPE02                                    '取引先タイプ２
                    T0005row("TORITYPE03") = wMC3_TORITYPE03                                    '取引先タイプ３
                    T0005row("TORITYPE04") = wMC3_TORITYPE04                                    '取引先タイプ４
                    T0005row("TORITYPE05") = wMC3_TORITYPE05                                    '取引先タイプ５

                End If
                WW_I_T5tbl.Rows.Add(T0005row)
            Next
        End Using
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing
    End Sub

    ''' <summary>
    ''' T0005データ整備（当日の最初の荷積荷卸を調べる）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditTumiOkiT0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD, STDATE, STTIME DESC, CREWKBN DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備2（トリップ・ドロップ再付番）
        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wYMD As String = String.Empty                  '出庫日

        'その他（判定フラグ）
        Dim wLASTstat As String = String.Empty             '出庫日最終作業（"荷積"or"荷卸"or""）

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            'ヘッダー、始業（A1）終業（Z1)は対象外
            If T0005row("HDKBN") = "H" OrElse
               T0005row("WORKKBN") = "A1" OrElse
               T0005row("WORKKBN") = "Z1" Then
                T0005row("wF1F3flg") = String.Empty

            Else '対象（始業、終業以外）

                '業務車番Break（業務車番、出庫日Break）
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                        T0005row("GSHABAN") = wGSHABAN AndAlso
                        T0005row("YMD") = wYMD) Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    wLASTstat = String.Empty

                End If

                '荷積の場合
                If (T0005row("WORKKBN") = "B2") AndAlso (T0005row("CREWKBN") = "1") Then

                    wLASTstat = "荷積"

                    '荷卸の場合
                ElseIf (T0005row("WORKKBN") = "B3") AndAlso (T0005row("CREWKBN") = "1") Then

                    wLASTstat = "荷卸"
                End If

                '〇項目設定1
                '各種状態判定設定
                T0005row("wLASTstat") = wLASTstat

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T0005データ整備、NJS専用（当日の最初の荷積荷卸を調べる）
    ''' </summary>
    ''' <param name="I_CAMPCODE"></param>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditTumiOkiNJST0005(ByVal I_CAMPCODE As String, ByRef IO_T5tbl As DataTable)
        '〇ソート（乗務員、業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, STAFFCODE, GSHABAN, YMD, STDATE, STTIME DESC, CREWKBN DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()
        '〇データ整備2（トリップ・ドロップ再付番）
        '　Keyブレーク用
        Dim wCAMPCODE As String = ""            '会社
        Dim wSTAFFCODE As String = ""           '乗務員
        Dim wGSHABAN As String = ""             '業務車番
        Dim wYMD As String = ""                 '出庫日

        'その他（判定フラグ）
        Dim wLASTstat As String = ""            '出庫日最終作業（"荷積"or"荷卸"or""）
        Dim wF3shako As String = ""             '帰庫した場所
        Dim wShachuhaku As String = ""          '車中泊有無


        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
                T0005row("wF1F3flg") = ""
            Else
                '業務車番Break（業務車番、出庫日Break）
                If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse (T0005row("STAFFCODE") <> wSTAFFCODE) OrElse (T0005row("GSHABAN") <> wGSHABAN) OrElse (T0005row("YMD") <> wYMD) Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")
                    wLASTstat = ""
                    wShachuhaku = ""
                    wF3shako = ""
                End If

                '帰庫の場合
                If (T0005row("WORKKBN") = "F3") AndAlso (T0005row("CREWKBN") = "1") Then
                    '車庫チェックで車庫の場合車庫判定する
                    wF3shako = If(ShakoCheck(I_CAMPCODE, T0005row("LATITUDE"), T0005row("LONGITUDE")) = "OK", "車庫", "車庫以外")
                End If

                '出庫の場合
                If (T0005row("WORKKBN") = "F1") AndAlso (T0005row("CREWKBN") = "1") Then
                    If ShakoCheck(I_CAMPCODE, T0005row("LATITUDE"), T0005row("LONGITUDE")) = "OK" Then
                        wShachuhaku = If(wF3shako = "車庫以外", "車中泊１日目", "")
                    Else
                        wShachuhaku = "車中泊"
                    End If
                End If

                '荷積の場合
                If (T0005row("WORKKBN") = "B2") AndAlso (T0005row("CREWKBN") = "1") Then wLASTstat = "荷積"
                '荷卸の場合
                If (T0005row("WORKKBN") = "B3") AndAlso (T0005row("CREWKBN") = "1") Then wLASTstat = "荷卸"

                '〇項目設定1
                '各種状態判定設定
                T0005row("wLASTstat") = wLASTstat
                T0005row("wShachuhaku") = wShachuhaku
                wShachuhaku = ""

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next

        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Clear()
        wCAMPCODE = ""            '会社
        wSTAFFCODE = ""           '乗務員
        wGSHABAN = ""             '業務車番
        wYMD = ""                 '出庫日

        Dim wSttime As String = ""
        Dim wEndtime As String = ""
        Dim wG1 As Boolean = False

        '〇ソート（乗務員、業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, STAFFCODE, GSHABAN, YMD, STDATE, STTIME, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If (T0005row("HDKBN") <> "H") AndAlso (T0005row("WORKKBN") <> "A1") AndAlso (T0005row("WORKKBN") <> "Z1") Then
                If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse (T0005row("STAFFCODE") <> wSTAFFCODE) OrElse (T0005row("GSHABAN") <> wGSHABAN) OrElse (T0005row("YMD") <> wYMD) Then
                    wCAMPCODE = T0005row("CAMPCODE")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")
                    wShachuhaku = ""
                End If

                wLASTstat = T0005row("wLASTstat")
                '車中泊の場合、荷卸に向けて運搬中なのでステータスを荷卸とする）
                If T0005row("wShachuhaku") Like "車中泊*" Then
                    wShachuhaku = T0005row("wShachuhaku")
                    If wLASTstat = "" Then wLASTstat = "荷卸"
                End If

                '〇項目設定1
                'まず、配送（G1）の時間内の作業に「配送」を設定し、それ以外をグループ作業と設定
                T0005row("wLASTstat") = wLASTstat
                T0005row("wShachuhaku") = wShachuhaku
                T0005row("wHaisoGroup") = "グループ"

                If T0005row("WORKKBN") = "G1" Then
                    wG1 = True
                    wSttime = T0005row("STTIME")
                    wEndtime = T0005row("ENDTIME")
                End If
                '〇G1とG１の開始/終了時間内のデータに配送グループを設定する
                If wG1 Then
                    If wSttime <= T0005row("STTIME") AndAlso wEndtime >= T0005row("ENDTIME") Then
                        T0005row("wHaisoGroup") = "配送"
                    Else
                        wG1 = False
                    End If
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next

        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Clear()
        wCAMPCODE = ""            '会社
        wSTAFFCODE = ""           '乗務員
        wGSHABAN = ""             '業務車番
        wYMD = ""                 '出庫日

        Dim wHaiso As Boolean = False
        Dim wF1 As Boolean = False
        Dim wB3 As Boolean = False
        '〇ソート（乗務員、業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, STAFFCODE, GSHABAN, YMD, STDATE, STTIME DESC, ENDTIME DESC, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If (T0005row("HDKBN") <> "H") AndAlso (T0005row("WORKKBN") <> "A1") AndAlso (T0005row("WORKKBN") <> "Z1") Then
                If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse
                   (T0005row("STAFFCODE") <> wSTAFFCODE) OrElse
                   (T0005row("GSHABAN") <> wGSHABAN) OrElse
                   (T0005row("YMD") <> wYMD) Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    wF1 = False
                    wB3 = False
                    wHaiso = False
                End If

                If T0005row("WORKKBN") = "F1" Then
                    wF1 = True
                End If
                If T0005row("WORKKBN") = "B3" Then
                    wB3 = True
                End If
                If T0005row("wHaisoGroup") = "配送" Then
                    wHaiso = True
                End If

                '念のため（配送時間（G1）前後の出庫、荷卸を配送作業とする）
                If wHaiso AndAlso T0005row("wHaisoGroup") = "グループ" Then
                    If Not wF1 OrElse Not wB3 Then T0005row("wHaisoGroup") = "配送"
                End If
                '配送
                If wF1 AndAlso wHaiso AndAlso T0005row("wHaisoGroup") = "グループ" Then
                    wF1 = False
                    wHaiso = False
                    T0005row("wHaisoGroup") = "配送"
                End If
                If wB3 AndAlso wHaiso AndAlso T0005row("wHaisoGroup") = "グループ" Then
                    wB3 = False
                    wHaiso = False
                    T0005row("wHaisoGroup") = "配送"
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next

        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Clear()
        wCAMPCODE = ""            '会社
        wSTAFFCODE = ""           '乗務員
        wGSHABAN = ""             '業務車番
        wYMD = ""                 '出庫日

        wHaiso = False
        Dim wF3 As Boolean = False
        wB3 = False
        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, STAFFCODE, GSHABAN, YMD, STDATE, STTIME, ENDTIME, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If (T0005row("HDKBN") <> "H") AndAlso (T0005row("WORKKBN") <> "A1") AndAlso (T0005row("WORKKBN") <> "Z1") Then
                If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse (T0005row("STAFFCODE") <> wSTAFFCODE) OrElse (T0005row("GSHABAN") <> wGSHABAN) OrElse (T0005row("YMD") <> wYMD) Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    wF3 = False
                    wB3 = False
                    wHaiso = False
                End If
                If T0005row("WORKKBN") = "F3" Then
                    wF3 = True
                End If
                If T0005row("WORKKBN") = "B3" Then
                    wB3 = True
                End If
                If T0005row("wHaisoGroup") = "配送" Then
                    wHaiso = True
                End If

                If wHaiso AndAlso T0005row("wHaisoGroup") = "グループ" Then
                    If Not wF3 OrElse Not wB3 Then T0005row("wHaisoGroup") = "配送"
                End If
                If wF3 AndAlso wHaiso AndAlso T0005row("wHaisoGroup") = "グループ" Then
                    wF3 = False
                    wHaiso = False
                    T0005row("wHaisoGroup") = "配送"
                End If
                If wB3 AndAlso wHaiso AndAlso T0005row("wHaisoGroup") = "グループ" Then
                    wB3 = False
                    wHaiso = False
                    T0005row("wHaisoGroup") = "配送"
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next

        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub
    ''' <summary>
    ''' T0005データ整備（仮トリップ・ドロップ再付番）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditTripDropT0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD, STDATE, STTIME, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備2（トリップ・ドロップ再付番）
        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wYMD As String = String.Empty                  '出庫日

        Dim wTRIPNO As Long = 0                 '仮トリップNo
        Dim wDROPNO As Integer = 0              '仮ドロップNo

        'その他（判定フラグ）
        Dim wTRIPDROPcnt As Integer = 0                     '出庫～帰庫内の荷積積卸回数（回送判定用）
        Dim wLASTstat As String = String.Empty             '前日最終作業（"荷積"or"荷卸"or""）
        Dim wGSHABANCHANGE As String = String.Empty        '業務車番先頭フラグ
        Dim wF1F3flg As Integer = 0                         '処理内の出庫CNT（連番）
        Dim wA1flg As String = String.Empty                'A1判定
        Dim wF1flg As String = String.Empty                'F1判定

        Dim wTRIPskip As String = String.Empty             '前日終了が卸作業で、翌日トリップカウントアップした場合"ON"
        Dim wDROPskip As String = String.Empty             '初回荷卸

        '　トリップ内共通データ設定用
        Dim wTRIP As String = String.Empty                 '実トリップ退避
        Dim wSHUKADATE As String = String.Empty            '出荷日付
        Dim wSHUKABASHO As String = String.Empty           '出荷場所
        Dim wSTAFFCODE As String = String.Empty            '従業員コード
        Dim wSUBSTAFFCODE As String = String.Empty         '従業員コード（副）
        Dim wCREWKBN As String = String.Empty              '正副区分

        '　ドロップ内共通データ設定用
        Dim wTODOKEDATE As String = String.Empty           '届日付
        Dim wTORICODE As String = String.Empty             '荷主（荷卸のタイミングのみデータ入力される）
        Dim wURIKBN As String = String.Empty               '売上計上基準
        Dim wSTORICODE As String = String.Empty            '販売店
        Dim wTODOKECODE As String = String.Empty           '届先
        Dim wORDERNO As String = String.Empty              '受注番号
        Dim wDETAILNO As String = String.Empty             '明細№
        Dim wORDERORG As String = String.Empty             '受注部署

        Dim wSAVEstat As String = String.Empty             '

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '始業・就業の場合
            If T0005row("HDKBN") = "H" OrElse
               T0005row("WORKKBN") = "A1" OrElse
               T0005row("WORKKBN") = "Z1" Then
                T0005row("wF1F3flg") = String.Empty

                'A1が先かF1が先か？
                If T0005row("WORKKBN") = "A1" AndAlso (T0005row("CREWKBN") = "1") Then
                    'F1先行フラグがない場合
                    If String.IsNullOrEmpty(wF1flg) Then
                        'A1先行フラグをON
                        wA1flg = "ON"
                        '荷積荷卸CNTアップ
                        wF1F3flg = wF1F3flg + 1
                        wTRIPDROPcnt = 0
                    Else
                        wF1flg = String.Empty
                    End If
                End If
            Else '対象（始業、終業以外）
                '業務車番Break（車番別トリップ・ドロップをクリア）
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                        T0005row("GSHABAN") = wGSHABAN AndAlso
                        T0005row("YMD") = wYMD) Then

                    '(説明)  T0005row("wLASTstat")には、当日最初作業が設定されている
                    wLASTstat = T0005row("wLASTstat")

                    '同一車番の複数日配送＆積置
                    If T0005row("CAMPCODE") = wCAMPCODE AndAlso T0005row("GSHABAN") = wGSHABAN AndAlso T0005row("wLASTstat") = "荷卸" Then
                        'カウントアップしない
                        wSAVEstat = "積配"
                        wDROPskip = "ON"
                    Else
                        '後続作業に荷積が有るものとしてトリップをカウントアップ(先頭からトリップドロップ設定したいため)
                        wTRIPNO = 1
                        wTRIPskip = "ON"
                        wDROPNO = 1
                        wDROPskip = "ON"

                        wSAVEstat = String.Empty

                    End If

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    'トリップ共通設定項目を退避
                    If wSAVEstat <> "積配" Then
                        wTRIP = String.Empty                 '実トリップ
                        wSHUKADATE = String.Empty            '出荷日付
                        wSHUKABASHO = String.Empty           '出荷場所

                        'ドロップ共通設定項目クリア
                        wTODOKEDATE = String.Empty           '届日付
                        wTORICODE = String.Empty             '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = String.Empty               '売上計上基準
                        wSTORICODE = String.Empty            '販売店
                        wTODOKECODE = String.Empty           '届先
                        wORDERNO = String.Empty              '受注番号
                        wDETAILNO = String.Empty             '明細№
                        wORDERORG = String.Empty             '受注部署
                    End If

                End If

                If T0005row("CREWKBN") = "1" Then
                    '〇トリップカウントアップ（添乗員は判定しない）
                    If T0005row("WORKKBN") = "B2" Then
                        If wSAVEstat = "積配" Then
                            wTRIPNO = 1
                            wDROPNO = 1
                            wDROPskip = "ON"

                        ElseIf wTRIPskip = "ON" Then
                            wTRIPskip = "OFF"                               '出庫日先頭でカウントアップ済の場合、カウントアップしない
                        Else
                            wTRIPNO = wTRIPNO + 1
                            wDROPNO = 1
                            wDROPskip = "ON"
                        End If
                        wSAVEstat = String.Empty

                        'トリップ共通設定項目を退避
                        wSHUKADATE = T0005row("YMD")                        '出荷日付
                        wTRIP = T0005row("TRIPNO")                          '実トリップ退避
                        wSHUKABASHO = T0005row("SHUKABASHO")                '出荷場所

                        'ドロップ共通設定項目クリア
                        wTODOKEDATE = String.Empty                                     '届日付
                        wTORICODE = String.Empty                                       '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = String.Empty                                         '売上計上基準
                        wSTORICODE = String.Empty                                      '販売店
                        wTODOKECODE = String.Empty                                     '届先
                        wORDERNO = String.Empty                                        '受注番号
                        wDETAILNO = String.Empty                                       '明細№
                        wORDERORG = String.Empty                                       '受注部署

                        '出庫日内の荷積荷卸回数カウントアップ
                        wTRIPDROPcnt = wTRIPDROPcnt + 1

                        '〇ドロップカウントアップ（添乗員は判定しない）
                    ElseIf (T0005row("WORKKBN") = "B3") Then
                        If wDROPskip = "ON" Then
                            wDROPskip = "OFF"
                        Else
                            wDROPNO = wDROPNO + 1
                        End If

                        'ドロップ共通設定項目を退避
                        wTODOKEDATE = T0005row("YMD")                       '届日付
                        wTORICODE = T0005row("TORICODE")                    '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = T0005row("URIKBN")                        '売上計上基準
                        wSTORICODE = T0005row("STORICODE")                  '販売店
                        wTODOKECODE = T0005row("TODOKECODE")                '届先
                        wORDERNO = T0005row("ORDERNO")                      '受注番号
                        wDETAILNO = T0005row("DETAILNO")                    '明細№
                        wORDERORG = T0005row("ORDERORG")                    '受注番号

                        '出庫日内の荷積荷卸回数カウントアップ
                        wTRIPDROPcnt = wTRIPDROPcnt + 1

                        '〇ドロップカウントアップ（添乗員は判定しない）
                    ElseIf (T0005row("WORKKBN") = "BY") Then
                        '出庫日内の荷積荷卸回数カウントアップ
                        wTRIPDROPcnt = wTRIPDROPcnt + 1

                        '〇出庫帰庫判定（添乗員は判定しない）
                        '出庫
                    ElseIf (T0005row("WORKKBN") = "F1") Then
                        'A1が先かF1が先か？
                        If String.IsNullOrEmpty(wA1flg) Then
                            'F1が先頭の場合
                            wF1flg = "ON"
                            '荷積荷卸CNTアップ
                            wF1F3flg = wF1F3flg + 1
                            wTRIPDROPcnt = 0
                        Else
                            wA1flg = String.Empty
                        End If

                    End If
                End If
                '〇項目設定1
                T0005row("wTRIPNO_K") = wTRIPNO.ToString("000")
                T0005row("wDROPNO") = wDROPNO.ToString("000")

                '共通設定
                T0005row("wSHUKODATE") = T0005row("YMD")                '出庫日付
                T0005row("wSTAFFCODE") = T0005row("STAFFCODE")          '従業員コード
                T0005row("wSUBSTAFFCODE") = T0005row("SUBSTAFFCODE")    '従業員コード（副）
                T0005row("wCREWKBN") = T0005row("CREWKBN")              '正副区分

                'トリップ共通設定
                T0005row("wSHUKADATE") = wSHUKADATE                     '出荷日付
                T0005row("wTRIPNO") = wTRIP                             '実トリップ退避
                T0005row("wSHUKABASHO") = wSHUKABASHO                   '出荷場所

                'ドロップ共通設定
                T0005row("wTODOKEDATE") = wTODOKEDATE                   '届日付
                T0005row("wTORICODE") = wTORICODE                       '荷主（荷卸のタイミングのみデータ入力される）
                T0005row("wURIKBN") = wURIKBN                           '売上計上基準
                T0005row("wSTORICODE") = wSTORICODE                     '販売店
                T0005row("wTODOKECODE") = wTODOKECODE                   '届先
                T0005row("wORDERNO") = wORDERNO                         '受注番号
                T0005row("wDETAILNO") = wDETAILNO                       '明細№
                T0005row("wORDERORG") = wORDERORG                       '受注部署

                '各種状態判定設定
                T0005row("wDATECHANGE") = String.Empty
                T0005row("wFirstCNTUP") = wTRIPskip
                T0005row("wLASTstat") = wLASTstat                       '当日最初の作業を出庫日内へ反映

                '〇項目設定2　…　下車勤務判断準備（出庫～帰庫CNT）

                T0005row("wF1F3flg") = wF1F3flg
                T0005row("wTRIPDROPcnt") = wTRIPDROPcnt

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T0005データ整備、JKT専用（仮トリップ・ドロップ再付番）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditTripDropT0005JKT(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD, STDATE, STTIME, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備2（トリップ・ドロップ再付番）
        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wYMD As String = String.Empty                  '出庫日

        Dim wTRIPNO As Long = 0                 '仮トリップNo
        Dim wDROPNO As Integer = 0              '仮ドロップNo

        'その他（判定フラグ）
        Dim wTRIPDROPcnt As Integer = 0                     '出庫～帰庫内の荷積積卸回数（回送判定用）
        Dim wLASTstat As String = String.Empty             '前日最終作業（"荷積"or"荷卸"or""）
        Dim wGSHABANCHANGE As String = String.Empty        '業務車番先頭フラグ
        Dim wF1F3flg As Integer = 0                         '処理内の出庫CNT（連番）
        Dim wA1flg As String = String.Empty                'A1判定
        Dim wF1flg As String = String.Empty                'F1判定

        Dim wTRIPskip As String = String.Empty             '前日終了が卸作業で、翌日トリップカウントアップした場合"ON"
        Dim wDROPskip As String = String.Empty             '初回荷卸

        '　トリップ内共通データ設定用
        Dim wTRIP As String = String.Empty                 '実トリップ退避
        Dim wSHUKADATE As String = String.Empty            '出荷日付
        Dim wSHUKABASHO As String = String.Empty           '出荷場所
        Dim wSTAFFCODE As String = String.Empty            '従業員コード
        Dim wSUBSTAFFCODE As String = String.Empty         '従業員コード（副）
        Dim wCREWKBN As String = String.Empty              '正副区分

        '　ドロップ内共通データ設定用
        Dim wTODOKEDATE As String = String.Empty           '届日付
        Dim wTORICODE As String = String.Empty             '荷主（荷卸のタイミングのみデータ入力される）
        Dim wURIKBN As String = String.Empty               '売上計上基準
        Dim wSTORICODE As String = String.Empty            '販売店
        Dim wTODOKECODE As String = String.Empty           '届先
        Dim wORDERNO As String = String.Empty              '受注番号
        Dim wDETAILNO As String = String.Empty             '明細№
        Dim wORDERORG As String = String.Empty             '受注部署

        Dim wSAVEstat As String = String.Empty             '

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '始業・就業の場合
            If T0005row("HDKBN") = "H" OrElse
               T0005row("WORKKBN") = "A1" OrElse
               T0005row("WORKKBN") = "Z1" Then
                T0005row("wF1F3flg") = String.Empty

                'A1が先かF1が先か？
                If T0005row("WORKKBN") = "A1" AndAlso (T0005row("CREWKBN") = "1") Then
                    'F1先行フラグがない場合
                    If String.IsNullOrEmpty(wF1flg) Then
                        'A1先行フラグをON
                        wA1flg = "ON"
                        '荷積荷卸CNTアップ
                        wF1F3flg = wF1F3flg + 1
                        wTRIPDROPcnt = 0
                    Else
                        wF1flg = String.Empty
                    End If
                End If
            Else '対象（始業、終業以外）
                '業務車番Break（車番別トリップ・ドロップをクリア）
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                        T0005row("GSHABAN") = wGSHABAN AndAlso
                        T0005row("YMD") = wYMD) Then

                    '(説明)  T0005row("wLASTstat")には、当日最初作業が設定されている
                    wLASTstat = T0005row("wLASTstat")

                    '同一車番（石油、高圧）の複数日配送＆積置
                    If T0005row("CAMPCODE") = wCAMPCODE AndAlso
                       T0005row("GSHABAN") = wGSHABAN AndAlso
                       T0005row("wLASTstat") = "荷卸" AndAlso
                       T0005row("MANGOILTYPE") <> GRT00005WRKINC.C_OILTYPE03 AndAlso
                       T0005row("MANGOILTYPE") <> GRT00005WRKINC.C_OILTYPE04 Then
                        'カウントアップしない
                        wSAVEstat = "積配"
                        wDROPskip = "ON"
                    Else
                        '後続作業に荷積が有るものとしてトリップをカウントアップ(先頭からトリップドロップ設定したいため)
                        wTRIPNO = 1
                        wTRIPskip = "ON"
                        wDROPNO = 1
                        wDROPskip = "ON"

                        wSAVEstat = String.Empty

                    End If

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    'トリップ共通設定項目を退避
                    If wSAVEstat <> "積配" Then
                        wTRIP = String.Empty                 '実トリップ
                        wSHUKADATE = String.Empty            '出荷日付
                        wSHUKABASHO = String.Empty           '出荷場所

                        'ドロップ共通設定項目クリア
                        wTODOKEDATE = String.Empty           '届日付
                        wTORICODE = String.Empty             '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = String.Empty               '売上計上基準
                        wSTORICODE = String.Empty            '販売店
                        wTODOKECODE = String.Empty           '届先
                        wORDERNO = String.Empty              '受注番号
                        wDETAILNO = String.Empty             '明細№
                        wORDERORG = String.Empty             '受注部署
                    End If

                End If

                If T0005row("CREWKBN") = "1" Then
                    '〇トリップカウントアップ（添乗員は判定しない）
                    If T0005row("WORKKBN") = "B2" Then
                        If wSAVEstat = "積配" Then
                            wTRIPNO = 1
                            wDROPNO = 1
                            wDROPskip = "ON"

                        ElseIf wTRIPskip = "ON" Then
                            wTRIPskip = "OFF"                               '出庫日先頭でカウントアップ済の場合、カウントアップしない
                        Else
                            wTRIPNO = wTRIPNO + 1
                            wDROPNO = 1
                            wDROPskip = "ON"
                        End If
                        wSAVEstat = String.Empty

                        'トリップ共通設定項目を退避
                        wSHUKADATE = T0005row("YMD")                        '出荷日付
                        wTRIP = T0005row("TRIPNO")                          '実トリップ退避
                        wSHUKABASHO = T0005row("SHUKABASHO")                '出荷場所

                        'ドロップ共通設定項目クリア
                        wTODOKEDATE = String.Empty                                     '届日付
                        wTORICODE = String.Empty                                       '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = String.Empty                                         '売上計上基準
                        wSTORICODE = String.Empty                                      '販売店
                        wTODOKECODE = String.Empty                                     '届先
                        wORDERNO = String.Empty                                        '受注番号
                        wDETAILNO = String.Empty                                       '明細№
                        wORDERORG = String.Empty                                       '受注部署

                        '出庫日内の荷積荷卸回数カウントアップ
                        wTRIPDROPcnt = wTRIPDROPcnt + 1

                        '〇ドロップカウントアップ（添乗員は判定しない）
                    ElseIf (T0005row("WORKKBN") = "B3") Then
                        If wDROPskip = "ON" Then
                            wDROPskip = "OFF"
                        Else
                            wDROPNO = wDROPNO + 1
                        End If

                        'ドロップ共通設定項目を退避
                        wTODOKEDATE = T0005row("YMD")                       '届日付
                        wTORICODE = T0005row("TORICODE")                    '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = T0005row("URIKBN")                        '売上計上基準
                        wSTORICODE = T0005row("STORICODE")                  '販売店
                        wTODOKECODE = T0005row("TODOKECODE")                '届先
                        wORDERNO = T0005row("ORDERNO")                      '受注番号
                        wDETAILNO = T0005row("DETAILNO")                    '明細№
                        wORDERORG = T0005row("ORDERORG")                    '受注番号

                        '出庫日内の荷積荷卸回数カウントアップ
                        wTRIPDROPcnt = wTRIPDROPcnt + 1

                        '〇ドロップカウントアップ（添乗員は判定しない）
                    ElseIf (T0005row("WORKKBN") = "BY") Then
                        '出庫日内の荷積荷卸回数カウントアップ
                        wTRIPDROPcnt = wTRIPDROPcnt + 1

                        '〇出庫帰庫判定（添乗員は判定しない）
                        '出庫
                    ElseIf (T0005row("WORKKBN") = "F1") Then
                        'A1が先かF1が先か？
                        If String.IsNullOrEmpty(wA1flg) Then
                            'F1が先頭の場合
                            wF1flg = "ON"
                            '荷積荷卸CNTアップ
                            wF1F3flg = wF1F3flg + 1
                            wTRIPDROPcnt = 0
                        Else
                            wA1flg = String.Empty
                        End If

                    End If
                End If
                '〇項目設定1
                T0005row("wTRIPNO_K") = wTRIPNO.ToString("000")
                T0005row("wDROPNO") = wDROPNO.ToString("000")

                '共通設定
                T0005row("wSHUKODATE") = T0005row("YMD")                '出庫日付
                T0005row("wSTAFFCODE") = T0005row("STAFFCODE")          '従業員コード
                T0005row("wSUBSTAFFCODE") = T0005row("SUBSTAFFCODE")    '従業員コード（副）
                T0005row("wCREWKBN") = T0005row("CREWKBN")              '正副区分

                'トリップ共通設定
                T0005row("wSHUKADATE") = wSHUKADATE                     '出荷日付
                T0005row("wTRIPNO") = wTRIP                             '実トリップ退避
                T0005row("wSHUKABASHO") = wSHUKABASHO                   '出荷場所

                'ドロップ共通設定
                T0005row("wTODOKEDATE") = wTODOKEDATE                   '届日付
                T0005row("wTORICODE") = wTORICODE                       '荷主（荷卸のタイミングのみデータ入力される）
                T0005row("wURIKBN") = wURIKBN                           '売上計上基準
                T0005row("wSTORICODE") = wSTORICODE                     '販売店
                T0005row("wTODOKECODE") = wTODOKECODE                   '届先
                T0005row("wORDERNO") = wORDERNO                         '受注番号
                T0005row("wDETAILNO") = wDETAILNO                       '明細№
                T0005row("wORDERORG") = wORDERORG                       '受注部署

                '各種状態判定設定
                T0005row("wDATECHANGE") = String.Empty
                T0005row("wFirstCNTUP") = wTRIPskip
                T0005row("wLASTstat") = wLASTstat                       '当日最初の作業を出庫日内へ反映

                '〇項目設定2　…　下車勤務判断準備（出庫～帰庫CNT）

                T0005row("wF1F3flg") = wF1F3flg
                T0005row("wTRIPDROPcnt") = wTRIPDROPcnt

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T0005データ整備、NJS専用（仮トリップ・ドロップ再付番）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditTripDropT0005NJS(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD, STDATE, STTIME, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備2（トリップ・ドロップ再付番）
        '　Keyブレーク用
        Dim wCAMPCODE As String = ""            '会社
        Dim wGSHABAN As String = ""             '業務車番
        Dim wYMD As String = ""                 '出庫日

        Dim wTRIPNO As Long = 0                 '仮トリップNo
        Dim wDROPNO As Integer = 0              '仮ドロップNo

        'その他（判定フラグ）
        Dim wTRIPDROPcnt As Integer = 0         '出庫～帰庫内の荷積積卸回数（回送判定用）
        Dim wLASTstat As String = ""            '前日最終作業（"荷積"or"荷卸"or""）
        Dim wGSHABANCHANGE As String = ""       '業務車番先頭フラグ
        Dim wF1F3flg As Integer = 0             '処理内の出庫CNT（連番）
        Dim wA1flg As String = ""               'A1判定
        Dim wF1flg As String = ""               'F1判定

        Dim wTRIPskip As String = ""            '前日終了が卸作業で、翌日トリップカウントアップした場合"ON"
        Dim wDROPskip As String = ""            '初回荷卸

        '　トリップ内共通データ設定用
        Dim wTRIP As String = ""                '実トリップ退避
        Dim wSHUKADATE As String = ""           '出荷日付
        Dim wSHUKABASHO As String = ""          '出荷場所
        Dim wSTAFFCODE As String = ""           '従業員コード
        Dim wSUBSTAFFCODE As String = ""        '従業員コード（副）
        Dim wCREWKBN As String = ""             '正副区分

        '　ドロップ内共通データ設定用
        Dim wTODOKEDATE As String = ""          '届日付
        Dim wTORICODE As String = ""            '荷主（荷卸のタイミングのみデータ入力される）
        Dim wURIKBN As String = ""              '売上計上基準
        Dim wSTORICODE As String = ""           '販売店
        Dim wTODOKECODE As String = ""          '届先
        Dim wORDERNO As String = ""             '受注番号
        Dim wDETAILNO As String = ""            '明細№
        Dim wORDERORG As String = ""            '受注部署

        Dim wSAVEstat As String = ""            '

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If (T0005row("HDKBN") = "H") Or (T0005row("WORKKBN") = "A1") Or (T0005row("WORKKBN") = "Z1") Then
                T0005row("wF1F3flg") = ""

                'A1が先かF1が先か？
                If T0005row("WORKKBN") = "A1" And (T0005row("CREWKBN") = "1") Then
                    If wF1flg = "" Then
                        'A1が先頭の場合
                        wA1flg = "ON"
                        '荷積荷卸CNTアップ
                        wF1F3flg = wF1F3flg + 1
                        wTRIPDROPcnt = 0
                    Else
                        wF1flg = ""
                    End If
                End If
            Else
                '業務車番Break（車番別トリップ・ドロップをクリア）
                If (T0005row("CAMPCODE") <> wCAMPCODE) OrElse (T0005row("GSHABAN") <> wGSHABAN) OrElse (T0005row("YMD") <> wYMD) Then

                    '(説明)  T0005row("wLASTstat")には、当日最初作業が設定されている
                    wLASTstat = T0005row("wLASTstat")

                    '同一車番（石油、高圧）の複数日配送＆積置
                    If T0005row("CAMPCODE") = wCAMPCODE AndAlso
                       T0005row("GSHABAN") = wGSHABAN AndAlso
                       T0005row("wLASTstat") = "荷卸" AndAlso
                       T0005row("MANGOILTYPE") <> GRT00005WRKINC.C_OILTYPE03 AndAlso
                       T0005row("MANGOILTYPE") <> GRT00005WRKINC.C_OILTYPE04 Then

                        'カウントアップしない

                        wSAVEstat = "積配"
                        wDROPskip = "ON"

                    Else
                        '後続作業に荷積が有るものとしてトリップをカウントアップ(先頭からトリップドロップ設定したいため)
                        wTRIPNO = 1
                        wTRIPskip = "ON"
                        wDROPNO = 1
                        wDROPskip = "ON"

                        wSAVEstat = ""

                    End If

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    'トリップ共通設定項目を退避
                    If wSAVEstat = "積配" Then
                    Else
                        wTRIP = ""                '実トリップ
                        wSHUKADATE = ""           '出荷日付
                        wSHUKABASHO = ""          '出荷場所

                        'ドロップ共通設定項目クリア
                        wTODOKEDATE = ""          '届日付
                        wTORICODE = ""            '荷主（荷卸のタイミングのみデータ入力される）
                        wURIKBN = ""              '売上計上基準
                        wSTORICODE = ""           '販売店
                        wTODOKECODE = ""          '届先
                        wORDERNO = ""             '受注番号
                        wDETAILNO = ""            '明細№
                        wORDERORG = ""            '受注部署
                    End If

                End If

                '〇トリップカウントアップ（添乗員は判定しない）
                If T0005row("WORKKBN") = "B2" AndAlso T0005row("CREWKBN") = "1" Then
                    If wSAVEstat = "積配" Then
                        wTRIPNO = 1
                        wDROPNO = 1
                        wDROPskip = "ON"

                    Else
                        If wTRIPskip = "ON" Then
                            wTRIPskip = "OFF"                               '出庫日先頭でカウントアップ済の場合、カウントアップしない
                        Else
                            wTRIPNO = wTRIPNO + 1
                            wDROPNO = 1
                            wDROPskip = "ON"
                        End If
                    End If

                    wSAVEstat = ""

                    'トリップ共通設定項目を退避
                    wSHUKADATE = T0005row("YMD")                        '出荷日付
                    wTRIP = T0005row("TRIPNO")                          '実トリップ退避
                    wSHUKABASHO = T0005row("SHUKABASHO")                '出荷場所

                    'ドロップ共通設定項目クリア
                    wTODOKEDATE = ""                                    '届日付
                    wTORICODE = ""                                      '荷主（荷卸のタイミングのみデータ入力される）
                    wURIKBN = ""                                        '売上計上基準
                    wSTORICODE = ""                                     '販売店
                    wTODOKECODE = ""                                    '届先
                    wORDERNO = ""                                       '受注番号
                    wDETAILNO = ""                                      '明細№
                    wORDERORG = ""                                      '受注部署

                    '出庫日内の荷積荷卸回数カウントアップ
                    wTRIPDROPcnt = wTRIPDROPcnt + 1

                End If

                '〇ドロップカウントアップ（添乗員は判定しない）
                If (T0005row("WORKKBN") = "B3") AndAlso (T0005row("CREWKBN") = "1") Then
                    If wDROPskip = "ON" Then
                        wDROPskip = "OFF"
                    Else
                        wDROPNO = wDROPNO + 1
                    End If

                    'ドロップ共通設定項目を退避
                    wTODOKEDATE = T0005row("YMD")                       '届日付
                    wTORICODE = T0005row("TORICODE")                    '荷主（荷卸のタイミングのみデータ入力される）
                    wURIKBN = T0005row("URIKBN")                        '売上計上基準
                    wSTORICODE = T0005row("STORICODE")                  '販売店
                    wTODOKECODE = T0005row("TODOKECODE")                '届先
                    wORDERNO = T0005row("ORDERNO")                      '受注番号
                    wDETAILNO = T0005row("DETAILNO")                    '明細№
                    wORDERORG = T0005row("ORDERORG")                    '受注番号

                    If wSHUKADATE = "" Then
                        wSHUKADATE = T0005row("YMD")                    '出荷日付
                    End If

                    '出庫日内の荷積荷卸回数カウントアップ
                    wTRIPDROPcnt = wTRIPDROPcnt + 1

                End If

                '〇ドロップカウントアップ（添乗員は判定しない）
                '出庫日内の荷積荷卸回数カウントアップ
                If (T0005row("WORKKBN") = "BY") AndAlso (T0005row("CREWKBN") = "1") Then wTRIPDROPcnt = wTRIPDROPcnt + 1
                '〇出庫帰庫判定（添乗員は判定しない）
                '出庫
                If (T0005row("WORKKBN") = "F1") AndAlso (T0005row("CREWKBN") = "1") Then
                    'A1が先かF1が先か？
                    If wA1flg = "" Then
                        'F1が先頭の場合
                        wF1flg = "ON"
                        '荷積荷卸CNTアップ
                        wF1F3flg = wF1F3flg + 1
                        wTRIPDROPcnt = 0
                    Else
                        wA1flg = ""
                    End If

                End If

                '〇項目設定1
                T0005row("wTRIPNO_K") = wTRIPNO.ToString("000")
                T0005row("wDROPNO") = wDROPNO.ToString("000")

                '共通設定
                T0005row("wSHUKODATE") = T0005row("YMD")                '出庫日付
                T0005row("wSTAFFCODE") = T0005row("STAFFCODE")          '従業員コード
                T0005row("wSUBSTAFFCODE") = T0005row("SUBSTAFFCODE")    '従業員コード（副）
                T0005row("wCREWKBN") = T0005row("CREWKBN")              '正副区分

                'トリップ共通設定
                T0005row("wSHUKADATE") = wSHUKADATE                     '出荷日付
                T0005row("wTRIPNO") = wTRIP                             '実トリップ退避
                T0005row("wSHUKABASHO") = wSHUKABASHO                   '出荷場所

                'ドロップ共通設定
                T0005row("wTODOKEDATE") = wTODOKEDATE                   '届日付
                T0005row("wTORICODE") = wTORICODE                       '荷主（荷卸のタイミングのみデータ入力される）
                T0005row("wURIKBN") = wURIKBN                           '売上計上基準
                T0005row("wSTORICODE") = wSTORICODE                     '販売店
                T0005row("wTODOKECODE") = wTODOKECODE                   '届先
                T0005row("wORDERNO") = wORDERNO                         '受注番号
                T0005row("wDETAILNO") = wDETAILNO                       '明細№
                T0005row("wORDERORG") = wORDERORG                       '受注部署

                '各種状態判定設定
                T0005row("wDATECHANGE") = ""
                T0005row("wFirstCNTUP") = wTRIPskip
                T0005row("wLASTstat") = wLASTstat                       '当日最初の作業を出庫日内へ反映

                '〇項目設定2　…　下車勤務判断準備（出庫～帰庫CNT）

                T0005row("wF1F3flg") = wF1F3flg
                T0005row("wTRIPDROPcnt") = wTRIPDROPcnt

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub
    ''' <summary>
    ''' T0005データ整備2（トリップ・ドロップ全明細反映）  …　日付降順処理 
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditTripDropT0005Detail(ByRef IO_T5tbl As DataTable)

        '〇データ整備（実車空車判定2）　…　最終荷卸一～出庫日最終　or 同一トリップ内の最終荷積荷卸直後～最終作業（荷積荷卸以外）

        'ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD DESC, STDATE DESC, STTIME DESC, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wYMD As String = String.Empty                  '出庫日
        Dim wTRIPNO_K As String = String.Empty             '仮トリップNo
        Dim wDROPNO As String = String.Empty               '仮ドロップNo

        '　トリップ内共通データ設定用
        Dim wSHUKADATE As String = String.Empty            '出荷日付
        Dim wSHUKABASHO As String = String.Empty           '出荷場所

        '　ドロップ内共通データ設定用
        Dim wTODOKEDATE As String = String.Empty           '届日付
        Dim wTORICODE As String = String.Empty             '荷主（荷卸のタイミングのみデータ入力される）
        Dim wURIKBN As String = String.Empty               '売上計上基準
        Dim wSTORICODE As String = String.Empty            '販売店
        Dim wTODOKECODE As String = String.Empty           '届先
        Dim wORDERNO As String = String.Empty              '受注番号
        Dim wDETAILNO As String = String.Empty             '明細№
        Dim wORDERORG As String = String.Empty             '受注部署

        '○トリップ情報（出荷日、出荷場所）の空欄を埋める
        wCAMPCODE = String.Empty
        wGSHABAN = String.Empty
        wYMD = String.Empty

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            If Not (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
                '判定：同一業務車番・出庫日・仮トリップNo・仮ドロップNoがブレイク時、設定情報を退避
                If T0005row("CAMPCODE") <> wCAMPCODE OrElse T0005row("GSHABAN") <> wGSHABAN OrElse T0005row("YMD") <> wYMD Then
                    'KeyBreak
                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wYMD = T0005row("YMD")

                    'Workクリア
                    wSHUKADATE = String.Empty
                    wSHUKABASHO = String.Empty

                End If

                '〇取得(同一出荷日の直前レコード内容を退避
                If Not String.IsNullOrEmpty(T0005row("wSHUKADATE")) Then wSHUKADATE = T0005row("wSHUKADATE")
                If Not String.IsNullOrEmpty(T0005row("wSHUKABASHO")) Then wSHUKABASHO = T0005row("wSHUKABASHO")

                '〇反映
                'トリップ共通設定
                If Not String.IsNullOrEmpty(wSHUKADATE) AndAlso String.IsNullOrEmpty(T0005row("wSHUKADATE")) Then T0005row("wSHUKADATE") = wSHUKADATE '出荷日付
                If Not String.IsNullOrEmpty(wSHUKABASHO) AndAlso String.IsNullOrEmpty(T0005row("wSHUKABASHO")) Then T0005row("wSHUKABASHO") = wSHUKABASHO '出荷場所

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next

        IO_T5tbl = WW_I_T5tbl.Copy

        '○ドロップ情報を埋める
        wCAMPCODE = String.Empty
        wGSHABAN = String.Empty
        wSHUKADATE = String.Empty
        wTRIPNO_K = String.Empty
        wDROPNO = String.Empty

        WW_I_T5tbl.Clear()
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            If Not (T0005row("HDKBN") = "H" OrElse T0005row("WORKKBN") = "A1" OrElse T0005row("WORKKBN") = "Z1") Then
                '判定：同一業務車番・出庫日・仮トリップNo・仮ドロップNoがブレイク時、設定情報を退避
                If T0005row("CAMPCODE") <> wCAMPCODE OrElse
                   T0005row("GSHABAN") <> wGSHABAN OrElse
                   T0005row("wSHUKADATE") <> wSHUKADATE OrElse
                   T0005row("wTRIPNO_K") <> wTRIPNO_K OrElse
                   T0005row("wDROPNO") <> wDROPNO Then

                    'KeyBreak
                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSHUKADATE = T0005row("wSHUKADATE")
                    wTRIPNO_K = T0005row("wTRIPNO_K")
                    wDROPNO = T0005row("wDROPNO")

                    'ドロップ共通設定項目を退避
                    wTODOKEDATE = T0005row("wTODOKEDATE")       '届日付
                    wTORICODE = T0005row("wTORICODE")           '荷主（荷卸のタイミングのみデータ入力される）
                    wURIKBN = T0005row("wURIKBN")               '売上計上基準
                    wSTORICODE = T0005row("wSTORICODE")         '販売店
                    wTODOKECODE = T0005row("wTODOKECODE")       '届先
                    wORDERNO = T0005row("wORDERNO")             '受注番号
                    wDETAILNO = T0005row("wDETAILNO")           '明細№
                    wORDERORG = T0005row("wORDERORG")           '受注部署
                End If

                '〇判定２用項目設定　…　トリップドロップ情報バラマキ
                'ドロップ共通設定
                T0005row("wTODOKEDATE") = wTODOKEDATE                   '届日付
                T0005row("wTORICODE") = wTORICODE                       '荷主（荷卸のタイミングのみデータ入力される）
                T0005row("wURIKBN") = wURIKBN                           '売上計上基準
                T0005row("wSTORICODE") = wSTORICODE                     '販売店
                T0005row("wTODOKECODE") = wTODOKECODE                   '届先
                T0005row("wORDERNO") = wORDERNO                         '受注番号
                T0005row("wDETAILNO") = wDETAILNO                       '明細№
                T0005row("wORDERORG") = wORDERORG                       '受注部署

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next

        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T0005データ整備（実車空車判定１）　…　空車：トリップ先頭～荷積、実車：荷積以降
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditFirstKushaT0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時(降順)、乗務員区分(降順)）       ※添乗員が先に来る様にソート（初回荷積は添乗員含め空車としたい
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, wSHUKADATE, wTRIPNO_K, wDROPNO, STDATE, STTIME, CREWKBN DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（出庫日最終作業、実車空車判定1）　…　朝一～初回荷積
        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSHUKADATE As String = String.Empty            '出荷日付
        Dim wTRIPNO_K As String = String.Empty             'トリップNo
        Dim wDROPNO As String = String.Empty               'ドロップNo

        '　実車空車
        Dim wKUSHAKBN As String = String.Empty

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
                T0005row("wKUSHAKBN") = String.Empty
            Else
                '出庫日先頭作業判定（業務車番、出庫日Break）
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                       T0005row("GSHABAN") = wGSHABAN AndAlso
                       T0005row("wSHUKADATE") = wSHUKADATE AndAlso
                       T0005row("wTRIPNO_K") = wTRIPNO_K AndAlso
                       T0005row("wDROPNO") = wDROPNO) Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSHUKADATE = T0005row("wSHUKADATE")
                    wTRIPNO_K = T0005row("wTRIPNO_K")
                    wDROPNO = T0005row("wDROPNO")

                    wKUSHAKBN = "空車"

                End If

                '〇項目設定　…　トリップ先頭～荷積→空車、荷積以降→実車
                T0005row("wKUSHAKBN") = wKUSHAKBN

                '〇判定　…　荷積は空車とする
                If T0005row("WORKKBN") = "B2" AndAlso T0005row("CREWKBN") = "1" Then wKUSHAKBN = String.Empty

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T00005データ整備（実車空車判定2）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditNextKushaT0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時(降順)、乗務員区分(降順)）       ※添乗員が先に来る様にソート（初回荷積は添乗員含め空車としたい
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, wSHUKADATE, STDATE, STTIME DESC, wTRIPNO_K, wDROPNO, CREWKBN DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（出庫日最終作業、実車空車判定1）　…　朝一～初回荷積
        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSHUKADATE As String = String.Empty            '出荷日付
        Dim wTRIPNO_K As String = String.Empty             'トリップNo
        Dim wDROPNO As String = String.Empty               'ドロップNo

        '　実車空車
        Dim wKUSHAKBN As String = String.Empty

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
                T0005row("wKUSHAKBN") = String.Empty
            Else
                '出庫日先頭作業判定（業務車番、出庫日Break）
                If T0005row("CAMPCODE") <> wCAMPCODE OrElse
                   T0005row("GSHABAN") <> wGSHABAN OrElse
                   T0005row("wSHUKADATE") <> wSHUKADATE OrElse
                   T0005row("wTRIPNO_K") <> wTRIPNO_K OrElse
                   T0005row("wDROPNO") <> wDROPNO Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSHUKADATE = T0005row("wSHUKADATE")
                    wTRIPNO_K = T0005row("wTRIPNO_K")
                    wDROPNO = T0005row("wDROPNO")

                    wKUSHAKBN = "空車"

                End If

                '〇判定
                If T0005row("WORKKBN") = "B3" Then wKUSHAKBN = String.Empty
                '〇項目設定　…　トリップ先頭～荷積→空車、荷積以降→実車
                If wKUSHAKBN = "空車" Then T0005row("wKUSHAKBN") = wKUSHAKBN

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T00005データ整備（回送判定＋距離・時間設定）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditKaisoT0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD DESC, STDATE DESC, STTIME DESC, CREWKBN DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（回送判定）　…　最終荷卸一～出庫日最終　or 同一トリップ内の最終荷積荷卸直後～最終作業（荷積荷卸以外）

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wF1F3flg As Integer = 0              '出庫～帰庫間の荷積荷卸CNT

        'その他
        Dim wKAISO As String = String.Empty                '回送判定
        Dim wTRIPDROPcnt As Integer = 0         '荷積積卸回数（回送判定用）
        Dim wLASTstat As String = String.Empty             '最終作業（"荷積"or"荷卸"or""）

        Dim wINT As Integer = 0
        Dim wDouble As Double = 0

        '   ・回送判定
        '       出庫～帰庫に荷積荷卸が無い場合、回送とする

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
                T0005row("wTRIPNO_K") = String.Empty
                T0005row("wDROPNO") = String.Empty

            Else
                '処理対象（始業・終業・休憩以外）

                '〇回送判定　…　出庫日荷積荷卸有無を判定（業務車番、同一出庫(wF1F3flg) Break）
                If T0005row("CAMPCODE") <> wCAMPCODE OrElse
                   T0005row("GSHABAN") <> wGSHABAN OrElse
                   T0005row("wF1F3flg") <> wF1F3flg Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wF1F3flg = T0005row("wF1F3flg")

                    Try
                        wTRIPDROPcnt = CInt(T0005row("wTRIPDROPcnt"))
                    Catch ex As Exception
                        wTRIPDROPcnt = 0
                    End Try
                    '〇TRIP/DROPの件数が１件も無い場合、回送
                    wKAISO = If(wTRIPDROPcnt > 0, String.Empty, "回送")
                    '〇水素の場合
                    If T0005row("wSUISOKBN") = "1" Then wKAISO = String.Empty
                    T0005row("wKAISO") = wKAISO
                End If
            End If

            '〇項目設定　…　WORK-作業時間、距離を設定
            If (T0005row("HDKBN") = "H") Then
                '一般高速
                T0005row("wIPPDISTANCE") = "0000000.00"
                T0005row("wKOSDISTANCE") = "0000000.00"
                '一般実車
                T0005row("wIPPJIDISTANCE") = "0000000.00"
                '一般空車
                T0005row("wIPPKUDISTANCE") = "0000000.00"
                '高速実車
                T0005row("wKOSJIDISTANCE") = "0000000.00"
                '高速空車
                T0005row("wKOSKUDISTANCE") = "0000000.00"

                '作業分
                T0005row("wWORKTIME") = "0"
                '移動分
                T0005row("wMOVETIME") = "0"
                '稼働分
                T0005row("wACTTIME") = "0"
                '実車走行分
                T0005row("wJIMOVETIME") = "0"
                '空車走行分
                T0005row("wKUMOVETIME") = "0"
            Else
                If T0005row("TERMKBN") = GRT00005WRKINC.TERM_TYPE.YAZAKI Then
                    If T0005row("WORKKBN") = "F3" AndAlso
                       Val(T0005row("IPPDISTANCE")) = 0 AndAlso
                       Val(T0005row("KOSDISTANCE")) = 0 AndAlso
                       Val(T0005row("IPPKUDISTANCE")) = 0 AndAlso
                       Val(T0005row("KOSKUDISTANCE")) = 0 AndAlso
                       Val(T0005row("IPPJIDISTANCE")) = 0 AndAlso
                       Val(T0005row("KOSJIDISTANCE")) = 0 Then
                        Dim wJIdbl As Double = 0
                        Dim wKUdbl As Double = 0
                        Try
                            wJIdbl = CDbl(T0005row("JIDISTANCE"))
                        Catch ex As Exception
                            wJIdbl = 0
                        End Try
                        Try
                            wKUdbl = CDbl(T0005row("KUDISTANCE"))
                        Catch ex As Exception
                            wKUdbl = 0
                        End Try
                        '一般
                        T0005row("wIPPDISTANCE") = (wJIdbl + wKUdbl).ToString("0000000.00")
                        '高速
                        T0005row("wKOSDISTANCE") = "0000000.00"
                        '一般空車
                        T0005row("wIPPKUDISTANCE") = wKUdbl.ToString("0000000.00")
                        '高速空車
                        T0005row("wKOSKUDISTANCE") = "0000000.00"
                        '一般実車
                        T0005row("wIPPJIDISTANCE") = wJIdbl.ToString("0000000.00")
                        '高速実車
                        T0005row("wKOSJIDISTANCE") = "0000000.00"
                    Else
                        '一般
                        T0005row("wIPPDISTANCE") = TryConvDouble(T0005row("IPPDISTANCE"), "0000000.00")

                        '高速
                        T0005row("wKOSDISTANCE") = TryConvDouble(T0005row("KOSDISTANCE"), "0000000.00")

                        '一般空車
                        T0005row("wIPPKUDISTANCE") = TryConvDouble(T0005row("IPPKUDISTANCE"), "0000000.00")

                        '高速空車
                        T0005row("wKOSKUDISTANCE") = TryConvDouble(T0005row("KOSKUDISTANCE"), "0000000.00")

                        '一般実車
                        T0005row("wIPPJIDISTANCE") = TryConvDouble(T0005row("IPPJIDISTANCE"), "0000000.00")

                        '高速実車
                        T0005row("wKOSJIDISTANCE") = TryConvDouble(T0005row("KOSJIDISTANCE"), "0000000.00")
                    End If
                Else
                    '一般
                    T0005row("wIPPDISTANCE") = TryConvDouble(T0005row("IPPDISTANCE"), "0000000.00")

                    '高速
                    T0005row("wKOSDISTANCE") = TryConvDouble(T0005row("KOSDISTANCE"), "0000000.00")

                    '一般空車
                    T0005row("wIPPKUDISTANCE") = TryConvDouble(T0005row("IPPKUDISTANCE"), "0000000.00")

                    '高速空車
                    T0005row("wKOSKUDISTANCE") = TryConvDouble(T0005row("KOSKUDISTANCE"), "0000000.00")

                    '一般実車
                    T0005row("wIPPJIDISTANCE") = TryConvDouble(T0005row("IPPJIDISTANCE"), "0000000.00")

                    '高速実車
                    T0005row("wKOSJIDISTANCE") = TryConvDouble(T0005row("KOSJIDISTANCE"), "0000000.00")

                End If

                '作業分
                If IsDBNull(T0005row("WORKTIME")) Then
                    wINT = 0
                Else
                    Try
                        Dim WW_TIME As String() = T0005row("WORKTIME").Split(":")
                        Try
                            wINT = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                T0005row("wWORKTIME") = wINT.ToString()

                '移動分
                If IsDBNull(T0005row("MOVETIME")) Then
                    wINT = 0
                Else
                    Try
                        Dim WW_TIME As String() = T0005row("MOVETIME").Split(":")
                        Try
                            wINT = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                T0005row("wMOVETIME") = wINT.ToString()

                '稼働分
                If IsDBNull(T0005row("ACTTIME")) Then
                    wINT = 0
                Else
                    Try
                        Dim WW_TIME As String() = T0005row("ACTTIME").Split(":")
                        Try
                            wINT = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                T0005row("wACTTIME") = wINT.ToString()

                '空車実車判断にて再設定
                If T0005row("wKUSHAKBN") = "空車" Then
                    '実車分
                    T0005row("wJIMOVETIME") = "0"
                    '空車分
                    T0005row("wKUMOVETIME") = T0005row("wMOVETIME")
                Else
                    '実車分
                    T0005row("wJIMOVETIME") = T0005row("wMOVETIME")
                    '空車分
                    T0005row("wKUMOVETIME") = "0"
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T00005データ整備 NJS専用（回送判定＋距離・時間設定）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditKaisoT0005NJS(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, YMD DESC, STDATE DESC, STTIME DESC, CREWKBN DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()
        '〇データ整備（回送判定）　…　最終荷卸一～出庫日最終　or 同一トリップ内の最終荷積荷卸直後～最終作業（荷積荷卸以外）
        '　Keyブレーク用
        Dim wCAMPCODE As String = ""            '会社
        Dim wGSHABAN As String = ""             '業務車番
        Dim wF1F3flg As Integer = 0              '出庫～帰庫間の荷積荷卸CNT

        'その他
        Dim wKAISO As String = ""               '回送判定
        Dim wTRIPDROPcnt As Integer = 0         '荷積積卸回数（回送判定用）
        Dim wLASTstat As String = ""            '最終作業（"荷積"or"荷卸"or""）

        Dim wINT As Integer = 0
        Dim wDouble As Double = 0

        '   ・回送判定
        '       出庫～帰庫に荷積荷卸が無い場合、回送とする

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
                T0005row("wTRIPNO_K") = ""
                T0005row("wDROPNO") = ""
            Else
                '処理対象（始業・終業・休憩以外）
                '〇回送判定　…　グループ作業を回送扱いとする
                If T0005row("wHaisoGroup") = "グループ" Then
                    T0005row("wKAISO") = "回送"
                Else
                    T0005row("wKAISO") = ""
                End If

            End If

            '〇項目設定　…　WORK-作業時間、距離を設定
            If (T0005row("HDKBN") = "H") Then
                '一般高速
                T0005row("wIPPDISTANCE") = "0000000.00"
                T0005row("wKOSDISTANCE") = "0000000.00"
                '一般実車
                T0005row("wIPPJIDISTANCE") = "0000000.00"
                '一般空車
                T0005row("wIPPKUDISTANCE") = "0000000.00"
                '高速実車
                T0005row("wKOSJIDISTANCE") = "0000000.00"
                '高速空車
                T0005row("wKOSKUDISTANCE") = "0000000.00"
                '作業分
                T0005row("wWORKTIME") = "0"
                '移動分
                T0005row("wMOVETIME") = "0"
                '稼働分
                T0005row("wACTTIME") = "0"
                '実車走行分
                T0005row("wJIMOVETIME") = "0"
                '空車走行分
                T0005row("wKUMOVETIME") = "0"
            Else
                If T0005row("TERMKBN") = GRT00005WRKINC.TERM_TYPE.YAZAKI Then
                    If T0005row("WORKKBN") = "F3" AndAlso
                       Val(T0005row("IPPDISTANCE")) = 0 AndAlso
                       Val(T0005row("KOSDISTANCE")) = 0 AndAlso
                       Val(T0005row("IPPKUDISTANCE")) = 0 AndAlso
                       Val(T0005row("KOSKUDISTANCE")) = 0 AndAlso
                       Val(T0005row("IPPJIDISTANCE")) = 0 AndAlso
                       Val(T0005row("KOSJIDISTANCE")) = 0 Then
                        Dim wJIdbl As Double = 0
                        Dim wKUdbl As Double = 0
                        Try
                            wJIdbl = CDbl(T0005row("JIDISTANCE"))
                        Catch ex As Exception
                            wJIdbl = 0
                        End Try
                        Try
                            wKUdbl = CDbl(T0005row("KUDISTANCE"))
                        Catch ex As Exception
                            wKUdbl = 0
                        End Try
                        '一般
                        T0005row("wIPPDISTANCE") = (wJIdbl + wKUdbl).ToString("0000000.00")
                        '高速
                        T0005row("wKOSDISTANCE") = "0000000.00"
                        '一般空車
                        T0005row("wIPPKUDISTANCE") = wKUdbl.ToString("0000000.00")
                        '高速空車
                        T0005row("wKOSKUDISTANCE") = "0000000.00"
                        '一般実車
                        T0005row("wIPPJIDISTANCE") = wJIdbl.ToString("0000000.00")
                        '高速実車
                        T0005row("wKOSJIDISTANCE") = "0000000.00"
                    Else
                        '一般
                        T0005row("wIPPDISTANCE") = TryConvDouble(T0005row("IPPDISTANCE"), "0000000.00")

                        '高速
                        T0005row("wKOSDISTANCE") = TryConvDouble(T0005row("KOSDISTANCE"), "0000000.00")

                        '一般空車
                        T0005row("wIPPKUDISTANCE") = TryConvDouble(T0005row("IPPKUDISTANCE"), "0000000.00")

                        '高速空車
                        T0005row("wKOSKUDISTANCE") = TryConvDouble(T0005row("KOSKUDISTANCE"), "0000000.00")

                        '一般実車
                        T0005row("wIPPJIDISTANCE") = TryConvDouble(T0005row("IPPJIDISTANCE"), "0000000.00")

                        '高速実車
                        T0005row("wKOSJIDISTANCE") = TryConvDouble(T0005row("KOSJIDISTANCE"), "0000000.00")

                    End If
                Else
                    '一般
                    T0005row("wIPPDISTANCE") = TryConvDouble(T0005row("IPPDISTANCE"), "0000000.00")

                    '高速
                    T0005row("wKOSDISTANCE") = TryConvDouble(T0005row("KOSDISTANCE"), "0000000.00")

                    '一般空車
                    T0005row("wIPPKUDISTANCE") = TryConvDouble(T0005row("IPPKUDISTANCE"), "0000000.00")

                    '高速空車
                    T0005row("wKOSKUDISTANCE") = TryConvDouble(T0005row("KOSKUDISTANCE"), "0000000.00")

                    '一般実車
                    T0005row("wIPPJIDISTANCE") = TryConvDouble(T0005row("IPPJIDISTANCE"), "0000000.00")

                    '高速実車
                    T0005row("wKOSJIDISTANCE") = TryConvDouble(T0005row("KOSJIDISTANCE"), "0000000.00")

                End If

                '作業分
                If IsDBNull(T0005row("WORKTIME")) Then
                    wINT = 0
                Else
                    Try
                        Dim WW_TIME As String() = T0005row("WORKTIME").Split(":")
                        Try
                            wINT = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                T0005row("wWORKTIME") = wINT.ToString()

                '移動分
                If IsDBNull(T0005row("MOVETIME")) Then
                    wINT = 0
                Else
                    Try
                        Dim WW_TIME As String() = T0005row("MOVETIME").Split(":")
                        Try
                            wINT = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                T0005row("wMOVETIME") = wINT.ToString()

                '稼働分
                If IsDBNull(T0005row("ACTTIME")) Then
                    wINT = 0
                Else
                    Try
                        Dim WW_TIME As String() = T0005row("ACTTIME").Split(":")
                        Try
                            wINT = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                T0005row("wACTTIME") = wINT.ToString()

                '空車実車判断にて再設定
                If T0005row("wKUSHAKBN") = "空車" Then
                    '実車分
                    T0005row("wJIMOVETIME") = "0"
                    '空車分
                    T0005row("wKUMOVETIME") = T0005row("wMOVETIME")
                Else
                    '実車分
                    T0005row("wJIMOVETIME") = T0005row("wMOVETIME")
                    '空車分
                    T0005row("wKUMOVETIME") = "0"
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub
    ''' <summary>
    ''' T00005データ整備（配送時間・距離補正０）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditDistance1T0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, STAFFCODE, NIPPONO, YMD DESC, STDATE DESC, STTIME DESC, SEQ DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（配送時間・距離補正０）　…　帰庫に隠れている配送時間および走行距離、求める

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSTAFFCODE As String = String.Empty            '従業員コード
        Dim wYMD As String = String.Empty                  '出庫日
        Dim wNIPPONO As String = String.Empty               '日報番号

        Dim wIPPDISTANCE As Double = 0          '一般(7,2)
        Dim wKOSDISTANCE As Double = 0          '高速(7,2)
        Dim wIPPJIDISTANCE As Double = 0        '一般実車(7,2)
        Dim wIPPKUDISTANCE As Double = 0        '一般空車(7,2)
        Dim wKOSJIDISTANCE As Double = 0        '高速実車(7,2)
        Dim wKOSKUDISTANCE As Double = 0        '高速空車(7,2)

        Dim wINT As Integer = 0
        Dim wDouble As Double = 0
        Dim wF3 As String = String.Empty
        Dim wF3row As DataRow = Nothing


        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = IO_T5tbl.Rows(i)

            '対象（始業、終業以外）
            If (T0005row("HDKBN") = "H") OrElse (T0005row("WORKKBN") = "A1") OrElse (T0005row("WORKKBN") = "Z1") Then
            Else
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                        T0005row("GSHABAN") = wGSHABAN AndAlso
                        T0005row("STAFFCODE") = wSTAFFCODE AndAlso
                        T0005row("YMD") = wYMD AndAlso
                        T0005row("NIPPONO") = wNIPPONO) Then

                    If wF3 = "ON" Then
                        wF3 = "OFF"
                        '一般距離
                        Try
                            wDouble = CDbl(wF3row("wIPPDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPDISTANCE = wDouble - wIPPDISTANCE
                        wF3row("wIPPDISTANCE") = wIPPDISTANCE.ToString("0000000.00")

                        '高速距離
                        Try
                            wDouble = CDbl(wF3row("wKOSDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSDISTANCE = wDouble - wKOSDISTANCE
                        wF3row("wKOSDISTANCE") = wKOSDISTANCE.ToString("0000000.00")

                        '一般実車距離
                        Try
                            wDouble = CDbl(wF3row("wIPPJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPJIDISTANCE = wDouble - wIPPJIDISTANCE
                        wF3row("wIPPJIDISTANCE") = wIPPJIDISTANCE.ToString("0000000.00")

                        '一般空車距離
                        Try
                            wDouble = CDbl(wF3row("wIPPKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPKUDISTANCE = wDouble - wIPPKUDISTANCE
                        wF3row("wIPPKUDISTANCE") = wIPPKUDISTANCE.ToString("0000000.00")

                        '高速実車距離
                        Try
                            wDouble = CDbl(wF3row("wKOSJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSJIDISTANCE = wDouble - wKOSJIDISTANCE
                        wF3row("wKOSJIDISTANCE") = wKOSJIDISTANCE.ToString("0000000.00")

                        '高速空車距離
                        Try
                            wDouble = CDbl(wF3row("wKOSKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSKUDISTANCE = wDouble - wKOSKUDISTANCE
                        wF3row("wKOSKUDISTANCE") = wKOSKUDISTANCE.ToString("0000000.00")
                    End If

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wYMD = T0005row("YMD")
                    wNIPPONO = T0005row("NIPPONO")

                    '同一車番・乗務員・出庫日内で調整
                    wIPPDISTANCE = 0                            '一般(7,2)
                    wKOSDISTANCE = 0                            '高速(7,2)
                    wIPPJIDISTANCE = 0                          '一般実車(7,2)
                    wIPPKUDISTANCE = 0                          '一般空車(7,2)
                    wKOSJIDISTANCE = 0                          '高速実車(7,2)
                    wKOSKUDISTANCE = 0                          '高速空車(7,2)
                End If

                If T0005row("WORKKBN") = "F3" Then
                    wF3row = IO_T5tbl.Rows(i)
                    wF3 = "ON"
                End If

                If T0005row("WORKKBN") <> "F3" Then
                    'WORK退避
                    Try
                        wDouble = CDbl(T0005row("wIPPDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPDISTANCE = wIPPDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSDISTANCE = wKOSDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble

                End If
            End If

        Next
        If wF3 = "ON" Then
            Try
                wDouble = CDbl(wF3row("wIPPDISTANCE"))
            Catch ex As Exception
                wDouble = 0
            End Try
            wIPPDISTANCE = wDouble - wIPPDISTANCE
            wF3row("wIPPDISTANCE") = wIPPDISTANCE.ToString("0000000.00")

            Try
                wDouble = CDbl(wF3row("wKOSDISTANCE"))
            Catch ex As Exception
                wDouble = 0
            End Try
            wKOSDISTANCE = wDouble - wKOSDISTANCE
            wF3row("wKOSDISTANCE") = wKOSDISTANCE.ToString("0000000.00")

            Try
                wDouble = CDbl(wF3row("wIPPJIDISTANCE"))
            Catch ex As Exception
                wDouble = 0
            End Try
            wIPPJIDISTANCE = wDouble - wIPPJIDISTANCE
            wF3row("wIPPJIDISTANCE") = wIPPJIDISTANCE.ToString("0000000.00")

            Try
                wDouble = CDbl(wF3row("wIPPJIDISTANCE"))
            Catch ex As Exception
                wDouble = 0
            End Try
            wIPPJIDISTANCE = wDouble - wIPPJIDISTANCE
            wF3row("wIPPJIDISTANCE") = wIPPJIDISTANCE.ToString("0000000.00")
        End If

    End Sub

    ''' <summary>
    ''' T00005データ整備（配送時間・距離補正１）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditDistance2T0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, STAFFCODE, YMD DESC, STDATE DESC, STTIME DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（配送時間・距離補正１）　…　荷積に表現されている配送時間および走行距離は、直前作業に含まれる

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSTAFFCODE As String = String.Empty            '従業員コード
        Dim wYMD As String = String.Empty                  '出庫日

        Dim wIPPDISTANCE As Double = 0          '一般(7,2)
        Dim wKOSDISTANCE As Double = 0          '高速(7,2)
        Dim wIPPJIDISTANCE As Double = 0        '一般実車(7,2)
        Dim wIPPKUDISTANCE As Double = 0        '一般空車(7,2)
        Dim wKOSJIDISTANCE As Double = 0        '高速実車(7,2)
        Dim wKOSKUDISTANCE As Double = 0        '高速空車(7,2)
        Dim wWORKTIME As Integer = 0            '作業分
        Dim wMOVETIME As Integer = 0            '移動分
        Dim wACTTIME As Integer = 0             '稼働分

        Dim wINT As Integer = 0
        Dim wDouble As Double = 0


        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If Not (T0005row("HDKBN") = "H" OrElse T0005row("WORKKBN") = "A1" OrElse T0005row("WORKKBN") = "Z1") Then
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                        T0005row("GSHABAN") = wGSHABAN AndAlso
                        T0005row("STAFFCODE") = wSTAFFCODE AndAlso
                        T0005row("YMD") = wYMD) Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wYMD = T0005row("YMD")

                    '同一車番・乗務員・出庫日内で調整
                    wIPPDISTANCE = 0                            '一般(7,2)
                    wKOSDISTANCE = 0                            '高速(7,2)
                    wIPPJIDISTANCE = 0                          '一般実車(7,2)
                    wIPPKUDISTANCE = 0                          '一般空車(7,2)
                    wKOSJIDISTANCE = 0                          '高速実車(7,2)
                    wKOSKUDISTANCE = 0                          '高速空車(7,2)

                    wMOVETIME = 0                               '移動分
                End If

                '荷積の場合、値退避。元値はクリア
                If (T0005row("WORKKBN") = "B2") Then            '連続した荷積あり→一旦加算

                    'WORK退避
                    Try
                        wDouble = CDbl(T0005row("wIPPDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPDISTANCE = wIPPDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSDISTANCE = wKOSDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble

                    Try
                        wINT = CInt(T0005row("wMOVETIME"))
                    Catch ex As Exception
                        wINT = 0
                    End Try
                    wMOVETIME = wMOVETIME + wINT

                    '項目クリア
                    T0005row("wIPPDISTANCE") = "0000000.00"
                    T0005row("wKOSDISTANCE") = "0000000.00"
                    T0005row("wIPPJIDISTANCE") = "0000000.00"
                    T0005row("wIPPKUDISTANCE") = "0000000.00"
                    T0005row("wKOSJIDISTANCE") = "0000000.00"
                    T0005row("wKOSKUDISTANCE") = "0000000.00"
                    T0005row("wMOVETIME") = "0"
                    T0005row("wJIMOVETIME") = "0"
                    T0005row("wKUMOVETIME") = "0"
                    T0005row("wACTTIME") = T0005row("wWORKTIME")
                Else
                    '荷積以外作業+荷積作業　→　荷積作業以外
                    If wIPPDISTANCE > 0 Then                            '一般(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPDISTANCE = wIPPDISTANCE + wDouble
                        T0005row("wIPPDISTANCE") = wIPPDISTANCE.ToString("0000000.00")
                        wIPPDISTANCE = 0
                    End If

                    If wKOSDISTANCE > 0 Then                            '高速(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSDISTANCE = wKOSDISTANCE + wDouble
                        T0005row("wKOSDISTANCE") = wKOSDISTANCE.ToString("0000000.00")
                        wKOSDISTANCE = 0
                    End If

                    If wIPPJIDISTANCE > 0 Then                         '一般実車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble
                        T0005row("wIPPJIDISTANCE") = wIPPJIDISTANCE.ToString("0000000.00")
                        wIPPJIDISTANCE = 0
                    End If

                    If wIPPKUDISTANCE > 0 Then                          '一般空車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble
                        T0005row("wIPPKUDISTANCE") = wIPPKUDISTANCE.ToString("0000000.00")
                        wIPPKUDISTANCE = 0
                    End If

                    If wKOSJIDISTANCE > 0 Then                          '高速実車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble
                        T0005row("wKOSJIDISTANCE") = wKOSJIDISTANCE.ToString("0000000.00")
                        wKOSJIDISTANCE = 0
                    End If

                    If wKOSKUDISTANCE > 0 Then                          '高速空車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble
                        T0005row("wKOSKUDISTANCE") = wKOSKUDISTANCE.ToString("0000000.00")
                        wKOSKUDISTANCE = 0
                    End If

                    If wMOVETIME > 0 Then                               '移動分
                        Try
                            wINT = CInt(T0005row("wWORKTIME"))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                        wWORKTIME = wINT

                        Try
                            wINT = CInt(T0005row("wMOVETIME"))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                        wMOVETIME = wMOVETIME + wINT
                        T0005row("wMOVETIME") = wMOVETIME.ToString()
                        '空車実車判断にて再設定
                        If T0005row("wKUSHAKBN") = "空車" Then
                            '実車分
                            T0005row("wJIMOVETIME") = "0"
                            '空車分
                            T0005row("wKUMOVETIME") = T0005row("wMOVETIME")
                        Else
                            '実車分
                            T0005row("wJIMOVETIME") = T0005row("wMOVETIME")
                            '空車分
                            T0005row("wKUMOVETIME") = "0"
                        End If

                        '再計算
                        wACTTIME = wWORKTIME + wMOVETIME
                        T0005row("wACTTIME") = wACTTIME.ToString()

                        wWORKTIME = 0
                        wMOVETIME = 0
                        wACTTIME = 0
                    End If
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing
    End Sub

    ''' <summary>
    ''' T00005データ整備（配送時間・距離補正１－２）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditDistance3T0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, STAFFCODE, NIPPONO, YMD DESC, STDATE DESC, STTIME DESC, SEQ DESC"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備（配送時間・距離補正１）　…　荷積に表現されている配送時間および走行距離は、直前作業に含まれる

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSTAFFCODE As String = String.Empty            '従業員コード
        Dim wYMD As String = String.Empty                  '出庫日
        Dim wNIPPONO As String = String.Empty              '日報番号

        Dim wIPPDISTANCE As Double = 0          '一般(7,2)
        Dim wKOSDISTANCE As Double = 0          '高速(7,2)
        Dim wIPPJIDISTANCE As Double = 0        '一般実車(7,2)
        Dim wIPPKUDISTANCE As Double = 0        '一般空車(7,2)
        Dim wKOSJIDISTANCE As Double = 0        '高速実車(7,2)
        Dim wKOSKUDISTANCE As Double = 0        '高速空車(7,2)

        Dim wINT As Integer = 0
        Dim wDouble As Double = 0
        Dim wF3 As String = String.Empty


        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If T0005row("HDKBN") <> "H" AndAlso T0005row("WORKKBN") <> "A1" AndAlso T0005row("WORKKBN") <> "Z1" Then
                If T0005row("CAMPCODE") <> wCAMPCODE OrElse
                   T0005row("GSHABAN") <> wGSHABAN OrElse
                   T0005row("STAFFCODE") <> wSTAFFCODE OrElse
                   T0005row("YMD") <> wYMD OrElse
                   T0005row("NIPPONO") <> wNIPPONO Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wYMD = T0005row("YMD")
                    wNIPPONO = T0005row("NIPPONO")
                    wF3 = "OFF"

                    '同一車番・乗務員・出庫日内で調整
                    wIPPDISTANCE = 0                            '一般(7,2)
                    wKOSDISTANCE = 0                            '高速(7,2)
                    wIPPJIDISTANCE = 0                          '一般実車(7,2)
                    wIPPKUDISTANCE = 0                          '一般空車(7,2)
                    wKOSJIDISTANCE = 0                          '高速実車(7,2)
                    wKOSKUDISTANCE = 0                          '高速空車(7,2)
                End If

                If wF3 = "OFF" Then
                    'WORK退避
                    Try
                        wDouble = CDbl(T0005row("wIPPDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPDISTANCE = wIPPDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSDISTANCE = wKOSDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble

                    '項目クリア
                    T0005row("wIPPDISTANCE") = "0000000.00"
                    T0005row("wKOSDISTANCE") = "0000000.00"
                    T0005row("wIPPJIDISTANCE") = "0000000.00"
                    T0005row("wIPPKUDISTANCE") = "0000000.00"
                    T0005row("wKOSJIDISTANCE") = "0000000.00"
                    T0005row("wKOSKUDISTANCE") = "0000000.00"
                End If

                '荷積の場合、値退避。元値はクリア
                If (T0005row("WORKKBN") = "F3") Then
                    wF3 = "ON"
                    If wIPPDISTANCE > 0 Then                            '一般(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPDISTANCE = wIPPDISTANCE + wDouble
                        T0005row("wIPPDISTANCE") = wIPPDISTANCE.ToString("0000000.00")
                        wIPPDISTANCE = 0
                    End If

                    If wKOSDISTANCE > 0 Then                            '高速(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSDISTANCE = wKOSDISTANCE + wDouble
                        T0005row("wKOSDISTANCE") = wKOSDISTANCE.ToString("0000000.00")
                        wKOSDISTANCE = 0
                    End If

                    If wIPPJIDISTANCE > 0 Then                         '一般実車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble
                        T0005row("wIPPJIDISTANCE") = wIPPJIDISTANCE.ToString("0000000.00")
                        wIPPJIDISTANCE = 0
                    End If

                    If wIPPKUDISTANCE > 0 Then                          '一般空車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble
                        T0005row("wIPPKUDISTANCE") = wIPPKUDISTANCE.ToString("0000000.00")
                        wIPPKUDISTANCE = 0
                    End If

                    If wKOSJIDISTANCE > 0 Then                          '高速実車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble
                        T0005row("wKOSJIDISTANCE") = wKOSJIDISTANCE.ToString("0000000.00")
                        wKOSJIDISTANCE = 0
                    End If

                    If wKOSKUDISTANCE > 0 Then                          '高速空車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble
                        T0005row("wKOSKUDISTANCE") = wKOSKUDISTANCE.ToString("0000000.00")
                        wKOSKUDISTANCE = 0
                    End If
                End If
            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing
    End Sub

    ''' <summary>
    ''' T00005データ整備（配送時間・距離補正２）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditDistance4T0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出庫日、開始日時、乗務員区分）
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, STAFFCODE, YMD, STDATE, STTIME, SEQ"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '〇データ整備7（配送時間・距離補正２）　…　休憩(BB)・他作業(BX)に表現されている配送時間および走行距離は、直後作業に含まれる

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSTAFFCODE As String = String.Empty            '従業員コード
        Dim wYMD As String = String.Empty                  '出庫日

        Dim wIPPDISTANCE As Double = 0          '一般(7,2)
        Dim wKOSDISTANCE As Double = 0          '高速(7,2)
        Dim wIPPJIDISTANCE As Double = 0        '一般実車(7,2)
        Dim wIPPKUDISTANCE As Double = 0        '一般空車(7,2)
        Dim wKOSJIDISTANCE As Double = 0        '高速実車(7,2)
        Dim wKOSKUDISTANCE As Double = 0        '高速空車(7,2)
        Dim wWORKTIME As Integer = 0            '作業分
        Dim wMOVETIME As Integer = 0            '移動分
        Dim wACTTIME As Integer = 0             '稼働分

        Dim wINT As Integer = 0
        Dim wDouble As Double = 0

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            '対象（始業、終業以外）
            If T0005row("HDKBN") <> "H" AndAlso T0005row("WORKKBN") <> "A1" AndAlso T0005row("WORKKBN") <> "Z1" Then
                If T0005row("CAMPCODE") <> wCAMPCODE OrElse
                   T0005row("GSHABAN") <> wGSHABAN OrElse
                   T0005row("STAFFCODE") <> wSTAFFCODE OrElse
                   T0005row("YMD") <> wYMD Then

                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSTAFFCODE = T0005row("STAFFCODE")
                    wYMD = T0005row("YMD")

                    '同一車番・乗務員・出庫日内で調整
                    wIPPDISTANCE = 0                            '一般(7,2)
                    wKOSDISTANCE = 0                            '高速(7,2)
                    wIPPJIDISTANCE = 0                          '一般実車(7,2)
                    wIPPKUDISTANCE = 0                          '一般空車(7,2)
                    wKOSJIDISTANCE = 0                          '高速実車(7,2)
                    wKOSKUDISTANCE = 0                          '高速空車(7,2)

                    'wWORKTIME = 0                              '作業分
                    wMOVETIME = 0                               '移動分
                    'wACTTIME = 0                               '稼働分
                End If

                '休憩・他作業の場合、値退避。元値はクリア
                If (T0005row("WORKKBN") = "BB") OrElse (T0005row("WORKKBN") = "BX") Then

                    'WORK退避
                    Try
                        wDouble = CDbl(T0005row("wIPPDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPDISTANCE = wIPPDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSDISTANCE = wKOSDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble

                    Try
                        wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                    wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble

                    'wWORKTIME = T0005row("wWORKTIME")      …　作業時間ずらす必要なし
                    Try
                        wINT = CInt(T0005row("wMOVETIME"))
                    Catch ex As Exception
                        wINT = 0
                    End Try
                    wMOVETIME = wMOVETIME + wINT
                    'wACTTIME = T0005row("wACTTIME")        …　再計算対象      

                    '項目クリア
                    T0005row("wIPPDISTANCE") = "0000000.00"
                    T0005row("wKOSDISTANCE") = "0000000.00"
                    T0005row("wIPPJIDISTANCE") = "0000000.00"
                    T0005row("wIPPKUDISTANCE") = "0000000.00"
                    T0005row("wKOSJIDISTANCE") = "0000000.00"
                    T0005row("wKOSKUDISTANCE") = "0000000.00"
                    T0005row("wMOVETIME") = "0"
                    T0005row("wJIMOVETIME") = "0"
                    T0005row("wKUMOVETIME") = "0"
                    T0005row("wACTTIME") = T0005row("wWORKTIME")
                Else
                    '荷積以外作業+荷積作業　→　荷積作業以外
                    If wIPPDISTANCE > 0 Then                            '一般(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPDISTANCE = wIPPDISTANCE + wDouble
                        T0005row("wIPPDISTANCE") = wIPPDISTANCE.ToString("0000000.00")
                        wIPPDISTANCE = 0
                    End If

                    If wKOSDISTANCE > 0 Then                            '高速(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSDISTANCE = wKOSDISTANCE + wDouble
                        T0005row("wKOSDISTANCE") = wKOSDISTANCE.ToString("0000000.00")
                        wKOSDISTANCE = 0
                    End If

                    If wIPPJIDISTANCE > 0 Then                         '一般実車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPJIDISTANCE = wIPPJIDISTANCE + wDouble
                        T0005row("wIPPJIDISTANCE") = wIPPJIDISTANCE.ToString("0000000.00")
                        wIPPJIDISTANCE = 0
                    End If

                    If wIPPKUDISTANCE > 0 Then                          '一般空車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wIPPKUDISTANCE = wIPPKUDISTANCE + wDouble
                        T0005row("wIPPKUDISTANCE") = wIPPKUDISTANCE.ToString("0000000.00")
                        wIPPKUDISTANCE = 0
                    End If

                    If wKOSJIDISTANCE > 0 Then                          '高速実車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSJIDISTANCE = wKOSJIDISTANCE + wDouble
                        T0005row("wKOSJIDISTANCE") = wKOSJIDISTANCE.ToString("0000000.00")
                        wKOSJIDISTANCE = 0
                    End If

                    If wKOSKUDISTANCE > 0 Then                          '高速空車(7,2)
                        Try
                            wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                        wKOSKUDISTANCE = wKOSKUDISTANCE + wDouble
                        T0005row("wKOSKUDISTANCE") = wKOSKUDISTANCE.ToString("0000000.00")
                        wKOSKUDISTANCE = 0
                    End If

                    If wMOVETIME > 0 Then                               '移動分
                        Try
                            wINT = CInt(T0005row("wWORKTIME"))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                        wWORKTIME = wINT

                        Try
                            wINT = CInt(T0005row("wMOVETIME"))
                        Catch ex As Exception
                            wINT = 0
                        End Try
                        wMOVETIME = wMOVETIME + wINT
                        T0005row("wMOVETIME") = wMOVETIME.ToString()
                        '空車実車判断にて再設定
                        If T0005row("wKUSHAKBN") = "空車" Then
                            '実車分
                            T0005row("wJIMOVETIME") = "0"
                            '空車分
                            T0005row("wKUMOVETIME") = T0005row("wMOVETIME")
                        Else
                            '実車分
                            T0005row("wJIMOVETIME") = T0005row("wMOVETIME")
                            '空車分
                            T0005row("wKUMOVETIME") = "0"
                        End If

                        '再計算
                        wACTTIME = wWORKTIME + wMOVETIME
                        T0005row("wACTTIME") = wACTTIME.ToString()

                        wWORKTIME = 0
                        wMOVETIME = 0
                        wACTTIME = 0
                    End If
                End If
            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing
    End Sub

    ''' <summary>
    ''' T0005データ整備（回次設定）
    ''' </summary>
    ''' <param name="IO_T5tbl"></param>
    ''' <remarks></remarks>
    Public Sub ReEditKaijiT0005(ByRef IO_T5tbl As DataTable)

        '〇ソート（業務車番、出荷日、開始日時(降順)、乗務員区分）       
        CS0026TBLSort.TABLE = IO_T5tbl
        CS0026TBLSort.SORTING = "CAMPCODE, GSHABAN, wSHUKADATE, wTRIPNO_K, wDROPNO, STDATE, STTIME, CREWKBN"
        CS0026TBLSort.FILTER = String.Empty
        IO_T5tbl = CS0026TBLSort.sort()

        '　Keyブレーク用
        Dim wCAMPCODE As String = String.Empty             '会社
        Dim wGSHABAN As String = String.Empty              '業務車番
        Dim wSHUKADATE As String = String.Empty            '出荷日付
        Dim wTRIPNO_K As String = String.Empty             'トリップNo

        Dim wKAIJI As Integer = 0               '回次付番
        Dim wB3cnt As Integer = 0               '荷卸の出現

        '　実車空車
        Dim wKUSHAKBN As String = String.Empty

        Dim WW_I_T5tbl As DataTable = IO_T5tbl.Clone
        For i As Integer = 0 To IO_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = WW_I_T5tbl.NewRow
            T0005row.ItemArray = IO_T5tbl.Rows(i).ItemArray

            T0005row("wKAIJI") = 0

            If Not (T0005row("HDKBN") = "H" OrElse T0005row("WORKKBN") = "A1" OrElse T0005row("WORKKBN") = "Z1") Then
                '出庫日先頭作業判定（業務車番、出庫日Break）
                If Not (T0005row("CAMPCODE") = wCAMPCODE AndAlso
                       T0005row("GSHABAN") = wGSHABAN AndAlso
                       T0005row("wSHUKADATE") = wSHUKADATE AndAlso
                       T0005row("wTRIPNO_K") = wTRIPNO_K) Then
                    wCAMPCODE = T0005row("CAMPCODE")
                    wGSHABAN = T0005row("GSHABAN")
                    wSHUKADATE = T0005row("wSHUKADATE")
                    wTRIPNO_K = T0005row("wTRIPNO_K")

                    '荷積の出現をカウント
                    wB3cnt = 0
                End If

                '〇荷積の場合、荷卸（B3）カウントをクリア
                If T0005row("WORKKBN") = "B2" AndAlso T0005row("CREWKBN") = "1" Then
                    '荷積の出現をカウント
                    wB3cnt = 0
                End If

                '〇最初の荷卸の場合、回次に１を設定
                If (T0005row("WORKKBN") = "B3") AndAlso (T0005row("CREWKBN") = "1") Then
                    '荷積後の最初の荷卸または、積配のとき回次を設定
                    If wB3cnt = 0 Then
                        T0005row("wKAIJI") = "1"
                    Else
                        T0005row("wKAIJI") = "0"
                    End If

                    wB3cnt = wB3cnt + 1
                End If

            End If
            WW_I_T5tbl.Rows.Add(T0005row)
        Next
        IO_T5tbl = WW_I_T5tbl.Copy

        WW_I_T5tbl.Dispose()
        WW_I_T5tbl = Nothing

    End Sub

    ''' <summary>
    ''' T0005_テーブル重複チェック処理
    ''' </summary>
    ''' <param name="I_TBL"></param>
    ''' <param name="O_ErrMsg"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub CheckDuplicateDataT0005(ByVal I_TBL As DataTable, ByRef O_ERRMSG As String, ByRef O_RTN As String)

        Try
            O_RTN = C_MESSAGE_NO.NORMAL
            O_ERRMSG = String.Empty

            CS0026TBLSort.TABLE = I_TBL
            CS0026TBLSort.SORTING = "SHIPORG, TERMKBN, YMD, STAFFCODE, HDKBN DESC, SEQ, DELFLG, SELECT"
            CS0026TBLSort.FILTER = String.Empty
            Dim WW_T5SELtbl As DataTable = CS0026TBLSort.sort()

            Dim WW_NEWKEY As String = String.Empty
            Dim WW_OLDKEY As String = String.Empty

            For Each WW_T5row As DataRow In WW_T5SELtbl.Rows
                If WW_T5row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING AndAlso
                   WW_T5row("SELECT") = "1" AndAlso
                   WW_T5row("HDKBN") = "D" Then

                    WW_NEWKEY = WW_T5row("SHIPORG") & WW_T5row("TERMKBN") & WW_T5row("YMD") & WW_T5row("STAFFCODE") & WW_T5row("SEQ") & WW_T5row("DELFLG")
                    If WW_OLDKEY = WW_NEWKEY Then
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = String.Empty
                        WW_ERR_MES = "・データ重複です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日付        =" & WW_T5row("YMD") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 出荷部署    =" & WW_T5row("SHIPORG") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員      =" & WW_T5row("STAFFCODE") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 端末区分    =" & WW_T5row("TERMKBN") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ＳＥＱ      =" & WW_T5row("SEQ") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 作業区分    =" & WW_T5row("WORKKBN") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 削除フラグ  =" & WW_T5row("DELFLG") & " ,"
                        O_ERRMSG = O_ERRMSG & ControlChars.NewLine & WW_ERR_MES
                        O_RTN = C_MESSAGE_NO.OVERLAP_DATA_ERROR
                    End If
                    WW_OLDKEY = WW_NEWKEY
                End If
            Next

            WW_T5SELtbl.Dispose()
            WW_T5SELtbl = Nothing

        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.INFSUBCLASS = "T0005_DuplCheck"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = String.Empty                                  '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = O_RTN
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    '''  L00001統計ＤＢ編集 
    ''' </summary>
    ''' <param name="I_T5tbl"></param>
    ''' <param name="I_L1tbl"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub EditL00001(ByVal I_T5tbl As DataTable, ByVal I_L1tbl As DataTable, ByRef O_RTN As String)

        '■■■ 統計ＤＢ ■■■


        Dim WW_SEQNO As Integer = 0                     '伝票別連番（貸借で同一の連番を付番）
        Dim wINT As Integer = 0
        Dim wDouble As Double = 0
        Dim wDouble2 As Double = 0
        Dim wOutWorkTime As Double = 0
        Dim wDATE As DateTime
        Dim wA1DATE As DateTime = C_DEFAULT_YMD
        Dim wF1DATE As DateTime = C_DEFAULT_YMD
        Dim wF1DATESV As DateTime = C_DEFAULT_YMD
        Dim wF3DATESV As DateTime = C_DEFAULT_YMD
        Dim wGSHABAN As String = String.Empty
        Dim wPAYOILKBN As String = String.Empty
        Dim wSHARYOKBN As String = String.Empty

        Dim wF1F3flg As String = "OFF"

        Dim I_ACTtype As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL


        '有効データ（追加処理）
        For i As Integer = 0 To I_T5tbl.Rows.Count - 1
            Dim T0005row As DataRow = I_T5tbl.Rows(i)

            If T0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            Else
                Continue For
            End If

            If T0005row("HDKBN") = "H" OrElse T0005row("WORKKBN") = "Z1" Then
                wA1DATE = C_DEFAULT_YMD
                wF1DATE = C_DEFAULT_YMD
                Continue For
            End If

            '始業時間の取得
            If T0005row("WORKKBN") = "A1" Then
                If IsDBNull(T0005row("STDATE")) Then
                    wA1DATE = C_DEFAULT_YMD
                Else
                    Try
                        wA1DATE = CDate(T0005row("STDATE") & " " & T0005row("STTIME"))
                    Catch ex As Exception
                        wA1DATE = C_DEFAULT_YMD
                    End Try
                End If
                Continue For
            End If

            '出庫時間の取得
            If T0005row("WORKKBN") = "F1" Then
                wF1F3flg = "ON"

                If IsDBNull(T0005row("STDATE")) Then
                    wF1DATE = C_DEFAULT_YMD
                Else
                    Try
                        wF1DATE = CDate(T0005row("STDATE") & " " & T0005row("STTIME"))
                    Catch ex As Exception
                        wF1DATE = C_DEFAULT_YMD
                    End Try
                End If
                wF1DATESV = wF1DATE
                For j As Integer = i + 1 To I_T5tbl.Rows.Count - 1
                    Dim F3row As DataRow = I_T5tbl.Rows(j)
                    If T0005row("YMD") = F3row("YMD") And
                       T0005row("STAFFCODE") = F3row("STAFFCODE") Then
                        If F3row("WORKKBN") = "F3" Then
                            If IsDBNull(F3row("STDATE")) Then
                                wF3DATESV = C_DEFAULT_YMD
                            Else
                                Try
                                    wF3DATESV = CDate(F3row("STDATE") & " " & F3row("STTIME"))
                                Catch ex As Exception
                                    wF3DATESV = C_DEFAULT_YMD
                                End Try
                            End If
                        End If
                    Else
                        Exit For
                    End If
                Next
            End If


            If wF1F3flg = "ON" Then
                Dim L00001row As DataRow = I_L1tbl.NewRow

                '---------------------------------------------------------
                'L1出力編集         新規
                '---------------------------------------------------------

                '〇ヘッダー情報
                L00001row("CAMPCODE") = T0005row("CAMPCODE")                                '会社コード
                L00001row("MOTOCHO") = "LO"                                                 '元帳（非会計を設定）
                L00001row("VERSION") = "000"                                                'バージョン
                L00001row("DENTYPE") = "T05"                                                '伝票タイプ
                L00001row("TENKI") = "0"                                                    '統計転記

                wDATE = C_DEFAULT_YMD
                Try
                    wDATE = CDate(T0005row("YMD"))
                Catch ex As Exception
                End Try
                L00001row("KEIJOYMD") = wDATE                                               '計上日付（売上：出荷日or届日。原価の場合：出庫日）

                Try
                    wDATE = CDate(T0005row("YMD"))
                Catch ex As Exception
                    wDATE = C_DEFAULT_YMD
                End Try
                L00001row("DENYMD") = wDATE                                                 '伝票日付

                L00001row("DENNO") = String.Empty                                                      '伝票番号(★★★★★)
                'L00001row("DENNO") = HttpContext.Current.Session("APSRVOrg") & CDate(L00001row("KEIJOYMD")).ToString("yyyy") & WW_SEQ

                L00001row("KANRENDENNO") = T0005row("SHIPORG") & " " _
                          & T0005row("YMD") & " " _
                          & T0005row("wSTAFFCODE") & " " _
                          & T0005row("GSHABAN")                                             '関連伝票No＋明細No

                L00001row("DTLNO") = T0005row("SEQ")                                        '明細番号
                L00001row("INQKBN") = 0                                                     '照会区分(ACにより決定★★★★★)
                L00001row("ACDCKBN") = String.Empty                                                    '貸借区分(ACにより決定★★★★★)
                L00001row("ACACHANTEI") = String.Empty                                                 '仕訳決定コード(ACにより決定★★★★★)
                L00001row("ACCODE") = String.Empty                                                     '勘定科目コード(ACにより決定★★★★★)
                L00001row("SUBACCODE") = String.Empty                                                  '補助科目コード(ACにより決定★★★★★)


                '〇経理情報
                L00001row("ACTORICODE") = String.Empty                                                 '取引先コード
                L00001row("ACOILTYPE") = String.Empty                                                  '油種
                L00001row("ACSHARYOTYPE") = String.Empty                                               '統一車番(上)
                L00001row("ACTSHABAN") = String.Empty                                                  '統一車番(下)
                L00001row("ACSTAFFCODE") = String.Empty                                                '従業員コード
                L00001row("ACBANKAC") = String.Empty                                                   '銀行口座
                L00001row("ACKEIJOMORG") = T0005row("MORG")                                            '計上管理部署コード(部・支店)
                L00001row("ACKEIJOORG") = T0005row("SHIPORG")                                          '計上部署コード
                L00001row("ACTAXKBN") = String.Empty                                                   '税区分
                L00001row("ACAMT") = 0                                                                 '金額

                '〇日付情報
                Try
                    wDATE = CDate(T0005row("YMD"))
                Catch ex As Exception
                    wDATE = C_DEFAULT_YMD
                End Try
                L00001row("NACSHUKODATE") = wDATE                                           '出庫日

                If IsDBNull(T0005row("wSHUKADATE")) Then
                    wDATE = C_DEFAULT_YMD
                Else
                    Try
                        wDATE = CDate(T0005row("wSHUKADATE"))
                    Catch ex As Exception
                        wDATE = C_DEFAULT_YMD
                    End Try
                End If
                L00001row("NACSHUKADATE") = wDATE                                           '出荷日

                If IsDBNull(T0005row("wTODOKEDATE")) Then
                    wDATE = C_DEFAULT_YMD
                Else
                    Try
                        wDATE = CDate(T0005row("wTODOKEDATE"))
                    Catch ex As Exception
                        Try
                            wDATE = CDate(T0005row("YMD"))
                        Catch ex2 As Exception
                            wDATE = C_DEFAULT_YMD
                        End Try
                    End Try
                End If
                L00001row("NACTODOKEDATE") = wDATE                                          '届日

                '〇荷主情報
                If T0005row("wTORICODE") = String.Empty Then
                    L00001row("NACTORICODE") = T0005row("TORICODE")                         '荷主コード
                Else
                    L00001row("NACTORICODE") = T0005row("wTORICODE")                        '荷主コード
                End If
                If T0005row("wURIKBN") = String.Empty Then
                    L00001row("NACURIKBN") = T0005row("URIKBN")                             '売上計上基準
                Else
                    L00001row("NACURIKBN") = T0005row("wURIKBN")                            '売上計上基準
                End If
                If T0005row("wTODOKECODE") = String.Empty Then
                    L00001row("NACTODOKECODE") = T0005row("TODOKECODE")                     '届先コード
                Else
                    L00001row("NACTODOKECODE") = T0005row("wTODOKECODE")                    '届先コード
                End If
                If T0005row("wSTORICODE") = String.Empty Then
                    L00001row("NACSTORICODE") = T0005row("STORICODE")                       '販売店コード
                Else
                    L00001row("NACSTORICODE") = T0005row("wSTORICODE")                      '販売店コード
                End If

                L00001row("NACSHUKABASHO") = T0005row("wSHUKABASHO")                        '出荷場所
                L00001row("NACTORITYPE01") = T0005row("TORITYPE01")                         '取引先・取引タイプ01
                L00001row("NACTORITYPE02") = T0005row("TORITYPE02")                         '取引先・取引タイプ02
                L00001row("NACTORITYPE03") = T0005row("TORITYPE03")                         '取引先・取引タイプ03
                L00001row("NACTORITYPE04") = T0005row("TORITYPE04")                         '取引先・取引タイプ04
                L00001row("NACTORITYPE05") = T0005row("TORITYPE05")                         '取引先・取引タイプ05
                L00001row("NACOILTYPE") = T0005row("OILTYPE1")                              '油種     …　売上計上時、明細都度、品名1～8該当項目を設定する事
                L00001row("NACPRODUCT1") = T0005row("PRODUCT11")                            '品名１    …　売上計上時、明細都度、明細都度、品名1～8該当項目を設定する事
                L00001row("NACPRODUCT2") = T0005row("PRODUCT21")                            '品名２    …　売上計上時、明細都度、明細都度、品名1～8該当項目を設定する事
                L00001row("NACPRODUCTCODE") = T0005row("PRODUCTCODE1")                      '品名コード…　売上計上時、明細都度、明細都度、品名1～8該当項目を設定する事

                '〇車番情報
                L00001row("NACGSHABAN") = T0005row("GSHABAN")                               '業務車番
                L00001row("NACSUPPLIERKBN") = T0005row("SUPPLIERKBN")                       '社有・庸車区分…業務車番共通設定済
                L00001row("NACSUPPLIER") = T0005row("SUPPLIER")                             '庸車会社…業務車番共通設定済
                L00001row("NACSHARYOOILTYPE") = T0005row("MANGOILTYPE")                     '車両登録油種…業務車番共通設定済
                L00001row("NACSHARYOTYPE1") = T0005row("SHARYOTYPEF")                       '統一車番(上)1…業務車番共通設定済
                L00001row("NACTSHABAN1") = T0005row("TSHABANF")                             '統一車番(下)1…業務車番共通設定済
                L00001row("NACMANGMORG1") = T0005row("MANGMORG1")                           '車両管理部署1…業務車番共通設定済
                L00001row("NACMANGSORG1") = T0005row("MANGSORG1")                           '車両設置部署1…業務車番共通設定済
                L00001row("NACMANGUORG1") = T0005row("SHIPORG")                             '車両運用部署1
                If T0005row("BASELEASE1") = String.Empty Then
                    L00001row("NACBASELEASE1") = "01"                                       '車両所有1…業務車番共通設定済
                Else
                    L00001row("NACBASELEASE1") = T0005row("BASELEASE1")                     '車両所有1…業務車番共通設定済
                End If
                L00001row("NACSHARYOTYPE2") = T0005row("SHARYOTYPEB")                       '統一車番(上)2…業務車番共通設定済
                L00001row("NACTSHABAN2") = T0005row("TSHABANB")                             '統一車番(下)2…業務車番共通設定済
                L00001row("NACMANGMORG2") = T0005row("MANGMORG2")                           '車両管理部署2…業務車番共通設定済
                L00001row("NACMANGSORG2") = T0005row("MANGSORG2")                           '車両設置部署2…業務車番共通設定済
                L00001row("NACMANGUORG2") = T0005row("SHIPORG")                             '車両運用部署2
                L00001row("NACBASELEASE2") = T0005row("BASELEASE2")                         '車両所有2…業務車番共通設定済
                L00001row("NACSHARYOTYPE3") = T0005row("SHARYOTYPEB2")                      '統一車番(上)3…業務車番共通設定済
                L00001row("NACTSHABAN3") = T0005row("TSHABANB2")                            '統一車番(下)3…業務車番共通設定済
                L00001row("NACMANGMORG3") = T0005row("MANGMORG3")                           '車両管理部署3…業務車番共通設定済
                L00001row("NACMANGSORG3") = T0005row("MANGSORG3")                           '車両設置部署3…業務車番共通設定済
                L00001row("NACMANGUORG3") = T0005row("SHIPORG")                             '車両運用部署3
                L00001row("NACBASELEASE3") = T0005row("BASELEASE3")                         '車両所有3…業務車番共通設定済

                '〇乗務員情報
                L00001row("NACCREWKBN") = T0005row("wCREWKBN")                              '正副区分
                L00001row("NACSTAFFCODE") = T0005row("wSTAFFCODE")                          '従業員コード（正）…乗務員共通設定済
                L00001row("NACSTAFFKBN") = T0005row("STAFFKBN")                             '社員区分（正）…乗務員共通設定済
                L00001row("NACMORG") = T0005row("MORG")                                     '管理部署（正）…乗務員共通設定済
                L00001row("NACHORG") = T0005row("HORG")                                     '配属部署（正）…乗務員共通設定済
                L00001row("NACSORG") = T0005row("SHIPORG")                                  '作業部署（正）…乗務員共通設定済

                L00001row("NACSTAFFCODE2") = T0005row("wSUBSTAFFCODE")                      '従業員コード（副）
                If IsDBNull(T0005row("wSUBSTAFFCODE")) Then
                    T0005row("wSUBSTAFFCODE") = String.Empty
                End If
                If T0005row("wSUBSTAFFCODE") = String.Empty Then
                    L00001row("NACSTAFFKBN2") = String.Empty
                    L00001row("NACMORG2") = String.Empty
                    L00001row("NACHORG2") = String.Empty
                    L00001row("NACSORG2") = String.Empty
                Else
                    L00001row("NACSTAFFKBN2") = T0005row("SUBSTAFFKBN")                     '社員区分（副）…乗務員共通設定済
                    L00001row("NACMORG2") = T0005row("SUBMORG")                             '管理部署（副）…乗務員共通設定済
                    L00001row("NACHORG2") = T0005row("SUBHORG")                             '配属部署（副）…乗務員共通設定済
                    L00001row("NACSORG2") = T0005row("SHIPORG")                             '作業部署（副）
                End If

                '〇受注・回次情報
                L00001row("NACORDERNO") = T0005row("wORDERNO")                              '受注番号
                L00001row("NACDETAILNO") = T0005row("wDETAILNO")                            '明細№
                L00001row("NACTRIPNO") = T0005row("wTRIPNO_K")                              'トリップ
                L00001row("NACDROPNO") = T0005row("wDROPNO")                                'ドロップ
                L00001row("NACSEQ") = "01"                                                  'SEQ
                If T0005row("wORDERORG") = String.Empty Then
                    L00001row("NACORDERORG") = T0005row("SHIPORG")                          '受注部署
                Else
                    L00001row("NACORDERORG") = T0005row("wORDERORG")                        '受注部署
                End If
                L00001row("NACSHIPORG") = T0005row("SHIPORG")                               '配送部署
                '〇数量情報
                L00001row("NACSURYO") = 0                                                   '受注・数量
                L00001row("NACTANI") = String.Empty                                                    '受注・単位

                Try
                    wDouble = CDbl(T0005row("SURYO1"))
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001row("NACJSURYO") = wDouble                                            '実績・配送数量    …　売上計上時、明細都度、明細都度、品名1～8該当項目を設定する事
                L00001row("NACSTANI") = T0005row("STANI1")                                  '実績・配送単位    …　売上計上時、明細都度、明細都度、品名1～8該当項目を設定する事

                '〇距離情報
                If IsDBNull(T0005row("wKAISO")) Then
                    T0005row("wKAISO") = String.Empty
                End If
                If T0005row("wKAISO") = "回送" Then
                    L00001row("NACHAIDISTANCE") = 0
                Else
                    If IsDBNull(T0005row("wIPPDISTANCE")) Then
                        wDouble = 0
                    Else
                        Try
                            wDouble = CDbl(T0005row("wIPPDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                    End If
                    L00001row("NACHAIDISTANCE") = wDouble
                    If IsDBNull(T0005row("wKOSDISTANCE")) Then
                        wDouble = 0
                    Else
                        Try
                            wDouble = CDbl(T0005row("wKOSDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                    End If
                    L00001row("NACHAIDISTANCE") = L00001row("NACHAIDISTANCE") + wDouble     '実績・配送距離
                End If

                If T0005row("wKAISO") = "回送" Then
                    If IsDBNull(T0005row("wIPPDISTANCE")) Then
                        wDouble = 0
                    Else
                        Try
                            wDouble = CDbl(T0005row("wIPPDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                    End If
                    L00001row("NACKAIDISTANCE") = wDouble
                    If IsDBNull(T0005row("wKOSDISTANCE")) Then
                        wDouble = 0
                    Else
                        Try
                            wDouble = CDbl(T0005row("wKOSDISTANCE"))
                        Catch ex As Exception
                            wDouble = 0
                        End Try
                    End If
                    L00001row("NACKAIDISTANCE") = L00001row("NACKAIDISTANCE") + wDouble     '実績・回送作業距離
                Else
                    L00001row("NACKAIDISTANCE") = 0
                End If

                L00001row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離

                L00001row("NACTTLDISTANCE") = L00001row("NACHAIDISTANCE") + L00001row("NACKAIDISTANCE") + L00001row("NACCHODISTANCE")   '実績・配送距離合計Σ

                '〇時間情報
                L00001row("NACHAISTDATE") = wF1DATESV                                        '実績・配送作業開始日時

                L00001row("NACHAIENDDATE") = wF3DATESV                                       '実績・配送作業終了日時

                If IsDBNull(T0005row("wWORKTIME")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wWORKTIME"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                If IsDBNull(T0005row("wMOVETIME")) Then
                    wDouble2 = 0
                Else
                    Try
                        wDouble2 = CDbl(T0005row("wMOVETIME"))
                    Catch ex As Exception
                        wDouble2 = 0
                    End Try
                End If
                L00001row("NACHAIWORKTIME") = wDouble + wDouble2                            '実績・配送作業時間

                If IsDBNull(T0005row("STDATE")) Then                                        'OK
                    wDATE = C_DEFAULT_YMD
                Else
                    Try
                        wDATE = CDate(T0005row("STDATE") & " " & T0005row("STTIME"))        'OK
                    Catch ex As Exception
                        wDATE = C_DEFAULT_YMD
                    End Try
                End If
                L00001row("NACGESSTDATE") = wDATE                                           '実績・下車作業開始日時

                If IsDBNull(T0005row("ENDDATE")) Then                                       'OK
                    wDATE = C_DEFAULT_YMD
                Else
                    Try
                        wDATE = CDate(T0005row("ENDDATE") & " " & T0005row("ENDTIME"))      'OK
                    Catch ex As Exception
                        wDATE = C_DEFAULT_YMD
                    End Try
                End If
                L00001row("NACGESENDDATE") = wDATE                                          '実績・下車作業終了日時

                If IsDBNull(T0005row("wWORKTIME")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wWORKTIME"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                If IsDBNull(T0005row("wMOVETIME")) Then
                    wDouble2 = 0
                Else
                    Try
                        wDouble2 = CDbl(T0005row("wMOVETIME"))
                    Catch ex As Exception
                        wDouble2 = 0
                    End Try
                End If

                If wA1DATE = CDate(C_DEFAULT_YMD) Or wF1DATE = CDate(C_DEFAULT_YMD) Then
                    wOutWorkTime = 0
                Else
                    If wA1DATE > wF1DATE Then
                        wOutWorkTime = DateDiff("n", wF1DATE, wA1DATE)
                        '計算結果は出庫レコードに設定（＝計算後クリア）
                        wA1DATE = C_DEFAULT_YMD
                        wF1DATE = C_DEFAULT_YMD
                    Else
                        wOutWorkTime = 0
                    End If
                End If

                L00001row("NACGESWORKTIME") = wDouble + wDouble2                            '実績・下車作業時間（分）
                L00001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                L00001row("NACOUTWORKTIME") = wOutWorkTime                                  '実績・就業外時間（分）
                L00001row("NACTTLWORKTIME") = L00001row("NACGESWORKTIME") + L00001row("NACCHOWORKTIME") + L00001row("NACOUTWORKTIME") '実績・配送合計時間Σ

                If IsDBNull(T0005row("STDATE")) Then                                        'OK
                    wDATE = C_DEFAULT_YMD
                Else
                    Try
                        wDATE = CDate(T0005row("STDATE") & " " & T0005row("STTIME"))        'OK
                    Catch ex As Exception
                        wDATE = C_DEFAULT_YMD
                    End Try
                End If
                L00001row("NACBREAKSTDATE") = wDATE                                         '実績・休憩開始日時

                If IsDBNull(T0005row("ENDDATE")) Then                                       'OK
                    wDATE = C_DEFAULT_YMD
                Else
                    Try
                        wDATE = CDate(T0005row("ENDDATE") & " " & T0005row("ENDTIME"))      'OK
                    Catch ex As Exception
                        wDATE = C_DEFAULT_YMD
                    End Try
                End If
                L00001row("NACBREAKENDDATE") = wDATE                                        '実績・休憩終了日時

                If IsDBNull(T0005row("wWORKTIME")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wWORKTIME"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACBREAKTIME") = wDouble                                         '実績・休憩時間
                L00001row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
                L00001row("NACTTLBREAKTIME") = L00001row("NACBREAKTIME") + L00001row("NACCHOBREAKTIME") '実績・休憩合計時間Σ（分）

                '〇その他情報
                If IsDBNull(T0005row("CASH")) Then
                    wINT = 0
                Else
                    Try
                        wINT = CInt(T0005row("CASH"))
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                L00001row("NACCASH") = wINT                                                 '実績・現金

                If IsDBNull(T0005row("ETC")) Then
                    wINT = 0
                Else
                    Try
                        wINT = CInt(T0005row("ETC"))
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                L00001row("NACETC") = wINT                                                  '実績・ETC

                If IsDBNull(T0005row("TICKET")) Then
                    wINT = 0
                Else
                    Try
                        wINT = CInt(T0005row("TICKET"))
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                L00001row("NACTICKET") = wINT
                If IsDBNull(T0005row("PRATE")) Then
                    wINT = 0
                Else
                    Try
                        wINT = CInt(T0005row("PRATE"))
                    Catch ex As Exception
                        wINT = 0
                    End Try
                End If
                L00001row("NACTICKET") = L00001row("NACTICKET") + wINT                      '実績・回数券

                If IsDBNull(T0005row("KYUYU")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("KYUYU"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACKYUYU") = wDouble                                             '実績・軽油

                If T0005row("WORKKBN") = "B3" Then
                    L00001row("NACUNLOADCNT") = 1                                           '実績・荷卸回数
                    L00001row("NACCHOUNLOADCNT") = 0                                        '実績・荷卸回数調整
                    L00001row("NACTTLUNLOADCNT") = 1                                        '実績・荷卸回数合計Σ
                Else
                    L00001row("NACUNLOADCNT") = 0                                           '実績・荷卸回数
                    L00001row("NACCHOUNLOADCNT") = 0                                        '実績・荷卸回数調整
                    L00001row("NACTTLUNLOADCNT") = 0                                        '実績・荷卸回数合計Σ
                End If

                '実績・回次
                If IsDBNull(T0005row("wKAIJI")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wKAIJI"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACKAIJI") = wDouble                                            '実績・回次

                '時間情報
                If IsDBNull(T0005row("wJIMOVETIME")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wJIMOVETIME"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACJITIME") = wDouble                                            '実績・実車走行時間
                L00001row("NACJICHOSTIME") = 0                                              '実績・実車走行時間調整（分）
                L00001row("NACJITTLETIME") = L00001row("NACJITIME") + L00001row("NACJICHOSTIME")    '実績・実車時間合計Σ

                If IsDBNull(T0005row("wKUMOVETIME")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wKUMOVETIME"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACKUTIME") = wDouble                                            '実績・空車走行時間
                L00001row("NACKUCHOTIME") = 0                                               '実績・空車走行時間調整（分）
                L00001row("NACKUTTLTIME") = L00001row("NACKUTIME") + L00001row("NACKUCHOTIME")  '実績・空車走行時間合計Σ

                If IsDBNull(T0005row("wIPPJIDISTANCE")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wIPPJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACJIDISTANCE") = wDouble

                If IsDBNull(T0005row("wKOSJIDISTANCE")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wKOSJIDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACJIDISTANCE") = L00001row("NACJIDISTANCE") + wDouble           '実績・実車距離
                L00001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L00001row("NACJITTLDISTANCE") = L00001row("NACJIDISTANCE") + L00001row("NACJICHODISTANCE")  '実績・実車距離合計Σ

                If IsDBNull(T0005row("wIPPKUDISTANCE")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wIPPKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACKUDISTANCE") = wDouble

                If IsDBNull(T0005row("wKOSKUDISTANCE")) Then
                    wDouble = 0
                Else
                    Try
                        wDouble = CDbl(T0005row("wKOSKUDISTANCE"))
                    Catch ex As Exception
                        wDouble = 0
                    End Try
                End If
                L00001row("NACKUDISTANCE") = L00001row("NACKUDISTANCE") + wDouble           '実績・空車距離
                L00001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L00001row("NACKUTTLDISTANCE") = L00001row("NACKUDISTANCE") + L00001row("NACKUCHODISTANCE")  '実績・空車距離合計Σ

                '〇運賃情報
                L00001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L00001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L00001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L00001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ

                L00001row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
                L00001row("NACOFFICETIME") = 0                                              '実績・事務時間
                L00001row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間

                L00001row("PAYSHUSHADATE") = C_DEFAULT_YMD                                   '出社日時
                L00001row("PAYTAISHADATE") = C_DEFAULT_YMD                                   '退社日時

                L00001row("PAYSTAFFKBN") = T0005row("STAFFKBN")                             '社員区分…乗務員共通設定済
                L00001row("PAYSTAFFCODE") = T0005row("wSTAFFCODE")                          '従業員コード
                L00001row("PAYMORG") = T0005row("MORG")                                     '従業員管理部署…乗務員共通設定済
                L00001row("PAYHORG") = T0005row("HORG")                                     '従業員配属部署…乗務員共通設定済

                L00001row("PAYHOLIDAYKBN") = T0005row("HOLIDAYKBN")                         '休日区分…カレンダ共通設定済
                L00001row("PAYKBN") = String.Empty                                                     '勤怠区分
                L00001row("PAYSHUKCHOKKBN") = String.Empty                                             '宿日直区分
                L00001row("PAYJYOMUKBN") = String.Empty
                '乗務区分
                If wGSHABAN <> T0005row("GSHABAN") Then
                    GetMA006(T0005row, wPAYOILKBN, wSHARYOKBN)
                End If
                wGSHABAN = T0005row("GSHABAN")

                L00001row("PAYOILKBN") = wPAYOILKBN                                         '勤怠用油種区分
                L00001row("PAYSHARYOKBN") = wSHARYOKBN                                      '勤怠用車両区分
                L00001row("PAYWORKNISSU") = 0                                               '所労
                L00001row("PAYSHOUKETUNISSU") = 0                                           '傷欠
                L00001row("PAYKUMIKETUNISSU") = 0                                           '組欠
                L00001row("PAYETCKETUNISSU") = 0                                            '他欠
                L00001row("PAYNENKYUNISSU") = 0                                             '年休
                L00001row("PAYTOKUKYUNISSU") = 0                                            '特休
                L00001row("PAYCHIKOKSOTAINISSU") = 0                                        '遅早
                L00001row("PAYSTOCKNISSU") = 0                                              'ストック休暇
                L00001row("PAYKYOTEIWEEKNISSU") = 0                                         '協定週休
                L00001row("PAYWEEKNISSU") = 0                                               '週休
                L00001row("PAYDAIKYUNISSU") = 0                                             '代休
                L00001row("PAYWORKTIME") = 0                                                '所定労働時間（分）
                L00001row("PAYNIGHTTIME") = 0                                               '所定深夜時間（分）
                L00001row("PAYORVERTIME") = 0                                               '平日残業時間（分）
                L00001row("PAYWNIGHTTIME") = 0                                              '平日深夜時間（分）
                L00001row("PAYWSWORKTIME") = 0                                              '日曜出勤時間（分）
                L00001row("PAYSNIGHTTIME") = 0                                              '日曜深夜時間（分）
                L00001row("PAYHWORKTIME") = 0                                               '休日出勤時間（分）
                L00001row("PAYHNIGHTTIME") = 0                                              '休日深夜時間（分）
                L00001row("PAYBREAKTIME") = 0                                               '休憩時間（分）

                L00001row("PAYNENSHINISSU") = 0                                             '年始出勤
                L00001row("PAYSHUKCHOKNNISSU") = 0                                          '宿日直年始
                L00001row("PAYSHUKCHOKNISSU") = 0                                           '宿日直通常
                L00001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                L00001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                L00001row("PAYTOKSAAKAISU") = 0                                             '特作A
                L00001row("PAYTOKSABKAISU") = 0                                             '特作B
                L00001row("PAYTOKSACKAISU") = 0                                             '特作C
                L00001row("PAYTENKOKAISU") = 0                                              '点呼回数
                L00001row("PAYHOANTIME") = 0                                                '保安検査入力（分）
                L00001row("PAYKOATUTIME") = 0                                               '高圧作業入力（分）
                L00001row("PAYTOKUSA1TIME") = 0                                             '特作Ⅰ（分）
                L00001row("PAYPONPNISSU") = 0                                               'ポンプ
                L00001row("PAYBULKNISSU") = 0                                               'バルク
                L00001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L00001row("PAYBKINMUKAISU") = 0                                             'B勤務
                L00001row("PAYYENDTIME") = "00:00"                                          '予定退社時刻
                L00001row("PAYAPPLYID") = String.Empty                                                 '申請ID
                L00001row("PAYRIYU") = String.Empty                                                    '理由
                L00001row("PAYRIYUETC") = String.Empty                                                 '理由その他
                L00001row("APPKIJUN") = String.Empty                                                   '配賦基準
                L00001row("APPKEY") = String.Empty                                                     '配賦統計キー

                L00001row("WORKKBN") = T0005row("WORKKBN")                                  '作業区分
                L00001row("KEYSTAFFCODE") = T0005row("STAFFCODE")                           '従業員コードキー
                L00001row("KEYGSHABAN") = T0005row("GSHABAN")                               '業務車番キー
                L00001row("KEYTRIPNO") = T0005row("wTRIPNO_K")                              'トリップキー
                L00001row("KEYDROPNO") = T0005row("wDROPNO")                                'ドロップキー

                L00001row("DELFLG") = C_DELETE_FLG.ALIVE                                    '削除フラグ


                '--------------------------------------------------------
                '統計ＤＢ出力編集
                '--------------------------------------------------------
                '売上
                If T0005row("WORKKBN") = "B3" And T0005row("CREWKBN") = "1" Then
                    '売上（仮伝票番号付番:MAX8明細のためSUBの中でカウントする）
                    EditUriDataL0001tbl(T0005row, L00001row, I_L1tbl, WW_SEQNO, O_RTN)
                End If

                '労務費（配送）
                '出庫（F1）～帰庫（F3）、休憩を除く
                If T0005row("wKAISO") <> "回送" And T0005row("WORKKBN") <> "BB" Then
                    '配送（仮伝票番号付番）　
                    WW_SEQNO += 1
                    L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                    EditHaisoL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                End If

                '労務費（回送）
                '出庫（F1）～帰庫（F3）、休憩を除く
                If T0005row("wKAISO") = "回送" And T0005row("WORKKBN") <> "BB" Then
                    '回送（仮伝票番号付番）　
                    WW_SEQNO += 1
                    L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                    EditKaisoL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                End If

                '労務費（休憩）
                If T0005row("WORKKBN") = "BB" Then
                    '休憩（仮伝票番号付番）
                    WW_SEQNO += 1
                    L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                    EditBreakTimeL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                End If

                '車両（配送自社 or リース）
                '出庫（F1）～帰庫（F3）、休憩を除く
                If T0005row("wKAISO") <> "回送" And T0005row("WORKKBN") <> "BB" And T0005row("CREWKBN") = "1" Then
                    '統一車番が設定されていない時の対応、自社とする
                    If T0005row("BASELEASE1") = String.Empty Then
                        T0005row("BASELEASE1") = "01"
                    End If

                    Select Case T0005row("BASELEASE1")
                        Case "01", "02", "03", "05"
                            '（仮伝票番号付番）
                            WW_SEQNO += 1
                            L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                            EditSharyoHaisoL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                    End Select
                End If

                '車両（回送自社 or リース）
                '出庫（F1）～帰庫（F3）、休憩を除く
                If T0005row("wKAISO") = "回送" And T0005row("WORKKBN") <> "BB" And T0005row("CREWKBN") = "1" Then
                    '統一車番が設定されていない時の対応、自社とする
                    If T0005row("BASELEASE1") = String.Empty Then
                        T0005row("BASELEASE1") = "01"
                    End If

                    Select Case T0005row("BASELEASE1")
                        Case "01", "02", "03", "05"
                            '（仮伝票番号付番）
                            WW_SEQNO += 1
                            L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                            EditSharyoKaisoL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                    End Select
                End If


                '軽油、通行料（帰庫レコードに保持している）
                If T0005row("WORKKBN") = "F3" And T0005row("CREWKBN") = "1" Then
                    If Val(T0005row("KYUYU")) = 0 Then
                    Else
                        'その他（軽油）（仮伝票番号付番）
                        WW_SEQNO += 1
                        L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                        EditOilL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                    End If

                    If Val(T0005row("CASH")) = 0 And
                       Val(T0005row("TICKET")) = 0 And
                       Val(T0005row("ETC")) = 0 Then
                    Else
                        'その他（通行料）（仮伝票番号付番）
                        WW_SEQNO += 1
                        L00001row("DENNO") = WW_SEQNO.ToString("0000000")
                        EditTollL0001tbl(T0005row, L00001row, I_L1tbl, O_RTN)
                    End If

                End If
            End If
            If T0005row("WORKKBN") = "F3" Then
                wF1F3flg = "OFF"
            End If

        Next

    End Sub

    ''' <summary>
    ''' L00001tbl追加（売上編集）
    ''' </summary>
    ''' <param name="I_T5ROW">日報行</param>
    ''' <param name="I_L1ROW">統計情報行</param>
    ''' <param name="IO_L1TBL">統計情報テーブル</param>
    ''' <param name="IO_DENNO">伝票番号</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Private Sub EditUriDataL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef IO_DENNO As Integer, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_EDA As String = String.Empty
        Dim WW_CAMPCODE As String = String.Empty
        Dim WW_STYMD As String = String.Empty
        Dim WW_ENDYMD As String = String.Empty
        Dim WW_MOTOCHO As String = String.Empty
        Dim WW_DENTYPE As String = String.Empty
        Dim WW_TORICODE As String = String.Empty
        Dim WW_TORITYPE01 As String = String.Empty
        Dim WW_TORITYPE02 As String = String.Empty
        Dim WW_TORITYPE03 As String = String.Empty
        Dim WW_TORITYPE04 As String = String.Empty
        Dim WW_TORITYPE05 As String = String.Empty
        Dim WW_URIKBN As String = String.Empty
        Dim WW_STORICODE As String = String.Empty
        Dim WW_OILTYPE As String = String.Empty
        Dim WW_PRODUCT1 As String = String.Empty
        Dim WW_SUPPLIERKBN As String = String.Empty
        Dim WW_MANGSORG As String = String.Empty
        Dim WW_MANGUORG As String = String.Empty
        Dim WW_BASELEASE As String = String.Empty
        Dim WW_STAFFKBN As String = String.Empty
        Dim WW_HORG As String = String.Empty
        Dim WW_SORG As String = String.Empty

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        Dim wDATE As DateTime

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray

        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）

        For j = 1 To 8

            Select Case j
                Case 1
                    If String.IsNullOrEmpty(t5row("OILTYPE1")) Then
                        Continue For
                    End If
                Case 2
                    If String.IsNullOrEmpty(t5row("OILTYPE2")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE2")
                    t5row("PRODUCT11") = t5row("PRODUCT12")
                    t5row("PRODUCT21") = t5row("PRODUCT22")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE2")
                    t5row("STANI1") = t5row("STANI2")
                    t5row("SURYO1") = t5row("SURYO2")
                Case 3
                    If String.IsNullOrEmpty(t5row("OILTYPE3")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE3")
                    t5row("PRODUCT11") = t5row("PRODUCT13")
                    t5row("PRODUCT21") = t5row("PRODUCT23")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE3")
                    t5row("STANI1") = t5row("STANI3")
                    t5row("SURYO1") = t5row("SURYO3")
                Case 4
                    If String.IsNullOrEmpty(t5row("OILTYPE4")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE4")
                    t5row("PRODUCT11") = t5row("PRODUCT14")
                    t5row("PRODUCT21") = t5row("PRODUCT24")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE4")
                    t5row("STANI1") = t5row("STANI4")
                    t5row("SURYO1") = t5row("SURYO4")
                Case 5
                    If String.IsNullOrEmpty(t5row("OILTYPE5")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE5")
                    t5row("PRODUCT11") = t5row("PRODUCT15")
                    t5row("PRODUCT21") = t5row("PRODUCT25")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE5")
                    t5row("STANI1") = t5row("STANI5")
                    t5row("SURYO1") = t5row("SURYO5")
                Case 6
                    If String.IsNullOrEmpty(t5row("OILTYPE6")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE6")
                    t5row("PRODUCT11") = t5row("PRODUCT16")
                    t5row("PRODUCT21") = t5row("PRODUCT26")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE6")
                    t5row("STANI1") = t5row("STANI6")
                    t5row("SURYO1") = t5row("SURYO6")
                Case 7
                    If String.IsNullOrEmpty(t5row("OILTYPE7")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE7")
                    t5row("PRODUCT11") = t5row("PRODUCT17")
                    t5row("PRODUCT21") = t5row("PRODUCT27")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE7")
                    t5row("STANI1") = t5row("STANI7")
                    t5row("SURYO1") = t5row("SURYO7")
                Case 8
                    If String.IsNullOrEmpty(t5row("OILTYPE8")) Then
                        Continue For
                    End If
                    t5row("OILTYPE1") = t5row("OILTYPE8")
                    t5row("PRODUCT11") = t5row("PRODUCT18")
                    t5row("PRODUCT21") = t5row("PRODUCT28")
                    t5row("PRODUCTCODE1") = t5row("PRODUCTCODE8")
                    t5row("STANI1") = t5row("STANI8")
                    t5row("SURYO1") = t5row("SURYO8")
            End Select
            WW_EDA = j.ToString("D2")

            wDATE = C_DEFAULT_YMD
            If t5row("wURIKBN") = "1" Then
                '出荷基準の場合、出荷日
                Try
                    wDATE = CDate(t5row("SHUKADATE"))
                Catch ex As Exception
                End Try
            Else
                '着地基準の場合、出荷日
                Try
                    wDATE = CDate(t5row("TODOKEDATE"))
                Catch ex As Exception
                End Try
            End If

            l1row("KEIJOYMD") = wDATE

            IO_DENNO += 1
            l1row("DENNO") = IO_DENNO
            l1row("NACOILTYPE") = t5row("OILTYPE1")                               '油種
            l1row("NACPRODUCT1") = t5row("PRODUCT11")                             '品名１
            l1row("NACPRODUCT2") = t5row("PRODUCT21")                             '品名２
            l1row("NACPRODUCTCODE") = t5row("PRODUCTCODE1")                       '品名コード
            l1row("NACSEQ") = j.ToString("00")                                    'SEQ

            If t5row("TERMKBN") = GRT00005WRKINC.TERM_TYPE.JX Then
                l1row("NACSURYO") = t5row("SURYO1")                               '受注・数量
                l1row("NACTANI") = t5row("STANI1")                                '受注・単位
            End If
            l1row("NACJSURYO") = t5row("SURYO1")                                  '実績・配送数量
            l1row("NACSTANI") = t5row("STANI1")                                   '実績・配送単位

            l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
            l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
            l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
            l1row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ

            l1row("NACHAISTDATE") = C_DEFAULT_YMD                                    '実績・配送作業開始日時
            l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                   '実績・配送作業終了日時
            l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
            l1row("NACGESSTDATE") = C_DEFAULT_YMD                                    '実績・下車作業開始日時
            l1row("NACGESENDDATE") = C_DEFAULT_YMD                                   '実績・下車作業終了日時
            l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
            l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
            l1row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）

            l1row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）

            l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                  '実績・休憩開始日時
            l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                 '実績・休憩終了日時
            l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
            l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
            l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）

            l1row("NACCASH") = 0                                                    '実績・現金
            l1row("NACETC") = 0                                                     '実績・ETC
            l1row("NACTICKET") = 0                                                  '実績・回数券
            l1row("NACKYUYU") = 0                                                   '実績・軽油

            l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
            l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
            l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
            l1row("NACKAIJI") = 0                                                   '実績・回次

            l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
            l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
            l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
            l1row("NACKUTIME") = 0                                                  '実績・空車時間（分）
            l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
            l1row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）

            l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
            l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
            l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
            l1row("NACKUDISTANCE") = 0                                              '実績・空車距離
            l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
            l1row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ

            l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
            l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
            l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
            l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ

            l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
            l1row("NACOFFICETIME") = 0                                              '実績・事務時間
            l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
            l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                   '出社日時
            l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                   '退社日時
            l1row("PAYOILKBN") = String.Empty                                                        '勤怠用油種区分
            l1row("PAYSHARYOKBN") = String.Empty                                                     '勤怠用車両区分

            '勘定科目判定テーブル検索
            If WW_CAMPCODE = l1row("CAMPCODE") AndAlso
               WW_STYMD = Format(l1row("KEIJOYMD"), "yyyy/MM/dd") AndAlso
               WW_ENDYMD = Format(l1row("KEIJOYMD"), "yyyy/MM/dd") AndAlso
               WW_MOTOCHO = "LO" AndAlso
               WW_DENTYPE = "T05" AndAlso
               WW_TORICODE = l1row("NACTORICODE") AndAlso
               WW_TORITYPE01 = l1row("NACTORITYPE01") AndAlso
               WW_TORITYPE02 = l1row("NACTORITYPE02") AndAlso
               WW_TORITYPE03 = l1row("NACTORITYPE03") AndAlso
               WW_TORITYPE04 = l1row("NACTORITYPE04") AndAlso
               WW_TORITYPE05 = l1row("NACTORITYPE05") AndAlso
               WW_URIKBN = l1row("NACURIKBN") AndAlso
               WW_STORICODE = l1row("NACSTORICODE") AndAlso
               WW_OILTYPE = l1row("NACOILTYPE") AndAlso
               WW_PRODUCT1 = l1row("NACPRODUCT1") AndAlso
               WW_SUPPLIERKBN = l1row("NACSUPPLIERKBN") AndAlso
               WW_MANGSORG = l1row("NACMANGSORG1") AndAlso
               WW_MANGUORG = l1row("NACMANGUORG1") AndAlso
               WW_BASELEASE = l1row("NACBASELEASE1") AndAlso
               WW_STAFFKBN = l1row("NACSTAFFKBN") AndAlso
               WW_HORG = l1row("NACHORG") AndAlso
               WW_SORG = l1row("NACSORG") Then
            Else

                CS0038ACCODEget.TBL = ML002tbl                                             '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
                CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
                CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
                CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ

                CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "URD"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                WW_ACCODE_D = CS0038ACCODEget.ACCODE
                WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
                WW_INQKBN_D = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "URC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                WW_ACCODE_C = CS0038ACCODEget.ACCODE
                WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
                WW_INQKBN_C = CS0038ACCODEget.INQKBN

            End If


            WW_CAMPCODE = l1row("CAMPCODE")                            '会社コード
            WW_STYMD = Format(l1row("KEIJOYMD"), "yyyy/MM/dd")         '開始日
            WW_ENDYMD = Format(l1row("KEIJOYMD"), "yyyy/MM/dd")        '終了日
            WW_MOTOCHO = "LO"                                            '元帳
            WW_DENTYPE = "T05"                                           '伝票タイプ
            WW_TORICODE = l1row("NACTORICODE")                         '荷主コード
            WW_TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
            WW_TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
            WW_TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
            WW_TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
            WW_TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
            WW_URIKBN = l1row("NACURIKBN")                             '売上計上基準
            WW_STORICODE = l1row("NACSTORICODE")                       '販売店コード
            WW_OILTYPE = l1row("NACOILTYPE")                           '油種
            WW_PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
            WW_SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
            WW_MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
            WW_MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
            WW_BASELEASE = l1row("NACBASELEASE1")                      '車両所有
            WW_STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
            WW_HORG = l1row("NACHORG")                                 '配属部署
            WW_SORG = l1row("NACSORG")                                 '作業部署

            Dim WW_ROW As DataRow

            '------------------------------------------------------
            '追加データ
            '------------------------------------------------------
            If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
                '●借方
                If WW_INQKBN_D = "1" Then
                    l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                    l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                    l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                    l1row("ACDCKBN") = "D"                                        '貸借区分
                    l1row("ACACHANTEI") = "URD"                                   '勘定科目判定コード
                    l1row("DTLNO") = "01"                                         '明細番号

                    WW_ROW = IO_L1TBL.NewRow
                    WW_ROW.ItemArray = l1row.ItemArray
                    IO_L1TBL.Rows.Add(WW_ROW)
                End If

                '●貸方
                If WW_INQKBN_C = "1" Then
                    l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                    l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                    l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                    l1row("ACDCKBN") = "C"                                        '貸借区分
                    l1row("ACACHANTEI") = "URC"                                   '勘定科目判定コード
                    l1row("DTLNO") = "02"                                         '明細番号

                    WW_ROW = IO_L1TBL.NewRow
                    WW_ROW.ItemArray = l1row.ItemArray
                    IO_L1TBL.Rows.Add(WW_ROW)
                End If
            End If

        Next

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' L00001tbl追加（労務費（配送）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditHaisoL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray


        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）

        l1row("NACOILTYPE") = String.Empty                                      '油種
        l1row("NACPRODUCT1") = String.Empty                                     '品名１
        l1row("NACPRODUCT2") = String.Empty                                     '品名２
        l1row("NACPRODUCTCODE") = String.Empty                                  '品名コード
        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                        '実績・配送単位
        l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACGESSTDATE") = C_DEFAULT_YMD                                   '実績・下車作業開始日時
        l1row("NACGESENDDATE") = C_DEFAULT_YMD                                  '実績・下車作業終了日時
        l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                 '実績・休憩開始日時
        l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                '実績・休憩終了日時
        l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
        l1row("NACCASH") = 0                                                    '実績・現金
        l1row("NACETC") = 0                                                     '実績・ETC
        l1row("NACTICKET") = 0                                                  '実績・回数券
        l1row("NACKYUYU") = 0                                                   '実績・軽油
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACKAIJI") = 0                                                   '実績・回次
        l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
        l1row("NACKUTIME") = 0                                                  '実績・空車時間（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
        l1row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
        l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
        l1row("NACKUDISTANCE") = 0                                              '実績・空車距離
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
        l1row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
        l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                  '退社日時
        l1row("PAYJYOMUKBN") = "1"                                              '乗務区分

        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                              '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ

        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = "HSD"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = "HSC"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN


        Dim WW_ROW As DataRow
        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = "HSD"                                   '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = "HSC"                                   '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    '''  L00001tbl追加（労務費（回送）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditKaisoL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray


        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）

        l1row("NACSURYO") = 0                                                   '受注・数量
        l1row("NACTANI") = String.Empty                                                    '受注・単位
        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                                   '実績・配送単位

        l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACHAISTDATE") = C_DEFAULT_YMD                                   '実績・配送作業開始日時
        l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                  '実績・配送作業終了日時
        l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                 '実績・休憩開始日時
        l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                '実績・休憩終了日時
        l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
        l1row("NACCASH") = 0                                                    '実績・現金
        l1row("NACETC") = 0                                                     '実績・ETC
        l1row("NACTICKET") = 0                                                  '実績・回数券
        l1row("NACKYUYU") = 0                                                   '実績・軽油
        l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
        l1row("NACKAIJI") = 0                                                   '実績・回次
        l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
        l1row("NACKUTIME") = 0                                                  '実績・空車時間（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
        l1row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
        l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
        l1row("NACKUDISTANCE") = 0                                              '実績・空車距離
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
        l1row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ

        l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                               '退社日時
        l1row("PAYJYOMUKBN") = "1"                                              '乗務区分
        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                              '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ
        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = "KSD"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = "KSC"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN


        Dim WW_ROW As DataRow
        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = "KSD"                                   '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = "KSC"                                   '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    '''  L00001tbl追加（労務費（休憩）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditBreakTimeL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray

        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                                   '実績・配送単位
        l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
        l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
        l1row("NACHAISTDATE") = C_DEFAULT_YMD                                   '実績・配送作業開始日時
        l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                  '実績・配送作業終了日時
        l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
        l1row("NACGESSTDATE") = C_DEFAULT_YMD                                   '実績・下車作業開始日時
        l1row("NACGESENDDATE") = C_DEFAULT_YMD                                  '実績・下車作業終了日時
        l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
        l1row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACCASH") = 0                                                    '実績・現金
        l1row("NACETC") = 0                                                     '実績・ETC
        l1row("NACTICKET") = 0                                                  '実績・回数券
        l1row("NACKYUYU") = 0                                                   '実績・軽油
        l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
        l1row("NACKAIJI") = 0                                                   '実績・回次
        l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
        l1row("NACKUTIME") = 0                                                  '実績・空車時間（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
        l1row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
        l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
        l1row("NACKUDISTANCE") = 0                                              '実績・空車距離
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
        l1row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
        l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                  '退社日時
        l1row("PAYJYOMUKBN") = "2"                                              '乗務区分
        l1row("PAYOILKBN") = String.Empty                                                  '勤怠用油種区分
        l1row("PAYSHARYOKBN") = String.Empty                                               '勤怠用車両区分
        l1row("PAYBREAKTIME") = l1row("NACBREAKTIME")                           '休憩時間（分）
        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                              '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ

        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = "RSD"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = "RSC"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN

        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        Dim WW_ROW As DataRow
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = "RSD"                                   '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = "RSC"                                   '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' L00001tbl追加（車両（配送）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditSharyoHaisoL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray

        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）

        l1row("NACOILTYPE") = String.Empty                                      '油種
        l1row("NACPRODUCT1") = String.Empty                                     '品名１
        l1row("NACPRODUCT2") = String.Empty                                     '品名２
        l1row("NACPRODUCTCODE") = String.Empty                                  '品名コード
        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                        '実績・配送単位
        l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
        l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
        l1row("NACHAISTDATE") = C_DEFAULT_YMD                                   '実績・配送作業開始日時
        l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                  '実績・配送作業終了日時
        l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
        l1row("NACGESSTDATE") = C_DEFAULT_YMD                                   '実績・下車作業開始日時
        l1row("NACGESENDDATE") = C_DEFAULT_YMD                                  '実績・下車作業終了日時
        l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
        l1row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）
        l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                 '実績・休憩開始日時
        l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                '実績・休憩終了日時
        l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
        l1row("NACCASH") = 0                                                    '実績・現金
        l1row("NACETC") = 0                                                     '実績・ETC
        l1row("NACTICKET") = 0                                                  '実績・回数券
        l1row("NACKYUYU") = 0                                                   '実績・軽油
        l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
        l1row("NACOFFICESORG") = String.Empty                                   '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                  '退社日時
        l1row("PAYHOLIDAYKBN") = String.Empty                                   '休日区分
        l1row("PAYOILKBN") = String.Empty                                       '勤怠用油種区分
        l1row("PAYSHARYOKBN") = String.Empty                                     '勤怠用車両区分

        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                           '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                           '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                          '伝票タイプ

        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        Dim WW_ACHANTEI_D As String = String.Empty
        Dim WW_ACHANTEI_C As String = String.Empty
        Select Case t5row("BASELEASE1")
            Case "01"
                '自社
                WW_ACHANTEI_D = "HJD"
                WW_ACHANTEI_C = "HJC"
            Case "02", "03", "05"
                'リース車、JOTリース、OPリース
                WW_ACHANTEI_D = "HLD"
                WW_ACHANTEI_C = "HLC"
        End Select

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = WW_ACHANTEI_D                                           '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = WW_ACHANTEI_C                                           '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN


        Dim WW_ROW As DataRow


        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = WW_ACHANTEI_D                           '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = WW_ACHANTEI_C                           '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    '''  L00001tbl追加（車両（回送）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditSharyoKaisoL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray

        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）
        l1row("NACSURYO") = 0                                                   '受注・数量
        l1row("NACTANI") = String.Empty                                                    '受注・単位
        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                                   '実績・配送単位

        l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
        l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ

        l1row("NACHAISTDATE") = C_DEFAULT_YMD                                   '実績・配送作業開始日時
        l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                  '実績・配送作業終了日時
        l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
        l1row("NACGESSTDATE") = C_DEFAULT_YMD                                   '実績・下車作業開始日時
        l1row("NACGESENDDATE") = C_DEFAULT_YMD                                  '実績・下車作業終了日時
        l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）

        l1row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）

        l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                 '実績・休憩開始日時
        l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                '実績・休憩終了日時
        l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）

        l1row("NACCASH") = 0                                                    '実績・現金
        l1row("NACETC") = 0                                                     '実績・ETC
        l1row("NACTICKET") = 0                                                  '実績・回数券
        l1row("NACKYUYU") = 0                                                   '実績・軽油

        l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
        l1row("NACKAIJI") = 0                                                   '実績・回次

        l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）

        l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整

        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ

        l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                  '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                  '退社日時

        l1row("PAYHOLIDAYKBN") = String.Empty                                              '休日区分
        l1row("PAYOILKBN") = String.Empty                                                  '勤怠用油種区分
        l1row("PAYSHARYOKBN") = String.Empty                                               '勤怠用車両区分

        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                             '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ

        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        Dim WW_ACHANTEI_D As String = String.Empty
        Dim WW_ACHANTEI_C As String = String.Empty
        Select Case t5row("BASELEASE1")
            Case "01"
                '自社
                WW_ACHANTEI_D = "KJD"
                WW_ACHANTEI_C = "KJC"
            Case "02", "03", "05"
                'リース車、JOTリース、OPリース
                WW_ACHANTEI_D = "KLD"
                WW_ACHANTEI_C = "KLC"
        End Select

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = WW_ACHANTEI_D                                           '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = WW_ACHANTEI_C                                           '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN


        Dim WW_ROW As DataRow
        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = WW_ACHANTEI_D                           '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = WW_ACHANTEI_C                           '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' L00001tbl追加（その他（軽油）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditOilL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray

        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）

        l1row("NACSURYO") = 0                                                   '受注・数量
        l1row("NACTANI") = String.Empty                                                    '受注・単位
        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                                   '実績・配送単位

        l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
        l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ

        l1row("NACHAISTDATE") = C_DEFAULT_YMD                                    '実績・配送作業開始日時
        l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                   '実績・配送作業終了日時
        l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
        l1row("NACGESSTDATE") = C_DEFAULT_YMD                                    '実績・下車作業開始日時
        l1row("NACGESENDDATE") = C_DEFAULT_YMD                                   '実績・下車作業終了日時
        l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）

        l1row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）

        l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                  '実績・休憩開始日時
        l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                 '実績・休憩終了日時
        l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）

        l1row("NACCASH") = 0                                                    '実績・現金
        l1row("NACETC") = 0                                                     '実績・ETC
        l1row("NACTICKET") = 0                                                  '実績・回数券
        l1row("NACKYUYU") = t5row("KYUYU")                                    '実績・軽油

        l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
        l1row("NACKAIJI") = 0                                                   '実績・回次

        l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
        l1row("NACKUTIME") = 0                                                  '実績・空車時間（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
        l1row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）

        l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
        l1row("NACKUDISTANCE") = 0                                              '実績・空車距離
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
        l1row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ

        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ

        l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                   '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                   '退社日時

        l1row("PAYSTAFFCODE") = String.Empty                                               '従業員コード
        l1row("PAYSTAFFKBN") = String.Empty                                                '社員区分
        l1row("PAYMORG") = String.Empty                                                    '従業員管理部署
        l1row("PAYHORG") = String.Empty                                                    '従業員配属部署
        l1row("PAYHOLIDAYKBN") = String.Empty                                              '休日区分
        l1row("PAYOILKBN") = String.Empty                                                        '勤怠用油種区分
        l1row("PAYSHARYOKBN") = String.Empty                                                     '勤怠用車両区分

        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                             '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ

        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = "KED"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = "KEC"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN


        Dim WW_ROW As DataRow

        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = "KED"                                   '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = "KEC"                                   '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' L00001tbl追加（その他（通行料）編集）
    ''' </summary>
    ''' <param name="I_T5ROW"></param>
    ''' <param name="I_L1ROW"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Private Sub EditTollL0001tbl(ByVal I_T5ROW As DataRow, ByVal I_L1ROW As DataRow, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0038ACCODEget As New CS0038ACCODEget          '勘定科目判定

        Dim WW_ACCODE_D As String = String.Empty
        Dim WW_SUBACCODE_D As String = String.Empty
        Dim WW_INQKBN_D As String = String.Empty
        Dim WW_ACCODE_C As String = String.Empty
        Dim WW_SUBACCODE_C As String = String.Empty
        Dim WW_INQKBN_C As String = String.Empty

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_T0005tbl As DataTable = New DataTable
        AddColumnT0005tbl(WW_T0005tbl)
        Dim t5row As DataRow = WW_T0005tbl.NewRow
        Dim l1row As DataRow = IO_L1TBL.NewRow
        t5row.ItemArray = I_T5ROW.ItemArray
        l1row.ItemArray = I_L1ROW.ItemArray

        '■■■ 統計ＤＢ ■■■
        '今回更新対象を抽出（荷卸、正乗務員のみ）
        '※ＤＢ登録済で変更発生したもの（変更前の元データはSELECT='0'（対象外）、DELFLG='1'（削除）として保存されている）

        l1row("NACSURYO") = 0                                                   '受注・数量
        l1row("NACTANI") = String.Empty                                                    '受注・単位
        l1row("NACJSURYO") = 0                                                  '実績・配送数量
        l1row("NACSTANI") = String.Empty                                                   '実績・配送単位

        l1row("NACHAIDISTANCE") = 0                                             '実績・配送距離
        l1row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
        l1row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
        l1row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ

        l1row("NACHAISTDATE") = C_DEFAULT_YMD                                    '実績・配送作業開始日時
        l1row("NACHAIENDDATE") = C_DEFAULT_YMD                                   '実績・配送作業終了日時
        l1row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
        l1row("NACGESSTDATE") = C_DEFAULT_YMD                                    '実績・下車作業開始日時
        l1row("NACGESENDDATE") = C_DEFAULT_YMD                                   '実績・下車作業終了日時
        l1row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
        l1row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
        l1row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）

        l1row("NACOUTWORKTIME") = 0                                             '実績・就業外時間（分）

        l1row("NACBREAKSTDATE") = C_DEFAULT_YMD                                  '実績・休憩開始日時
        l1row("NACBREAKENDDATE") = C_DEFAULT_YMD                                 '実績・休憩終了日時
        l1row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
        l1row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
        l1row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）

        l1row("NACCASH") = Val(Replace(t5row("CASH"), ",", ""))               '実績・現金
        l1row("NACETC") = Val(Replace(t5row("ETC"), ",", ""))                 '実績・ETC
        l1row("NACTICKET") = Val(Replace(t5row("TICKET"), ",", ""))           '実績・回数券
        l1row("NACKYUYU") = 0                                                   '実績・軽油

        l1row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
        l1row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
        l1row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
        l1row("NACKAIJI") = 0                                                   '実績・回次

        l1row("NACJITIME") = 0                                                  '実績・実車時間（分）
        l1row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
        l1row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
        l1row("NACKUTIME") = 0                                                  '実績・空車時間（分）
        l1row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
        l1row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）

        l1row("NACJIDISTANCE") = 0                                              '実績・実車距離
        l1row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
        l1row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
        l1row("NACKUDISTANCE") = 0                                              '実績・空車距離
        l1row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
        l1row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ

        l1row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
        l1row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
        l1row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
        l1row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ

        l1row("NACOFFICESORG") = String.Empty                                              '実績・作業部署
        l1row("NACOFFICETIME") = 0                                              '実績・事務時間
        l1row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
        l1row("PAYSHUSHADATE") = C_DEFAULT_YMD                                   '出社日時
        l1row("PAYTAISHADATE") = C_DEFAULT_YMD                                   '退社日時

        l1row("PAYSTAFFCODE") = String.Empty                                               '従業員コード
        l1row("PAYSTAFFKBN") = String.Empty                                                '社員区分
        l1row("PAYMORG") = String.Empty                                                    '従業員管理部署
        l1row("PAYHORG") = String.Empty                                                    '従業員配属部署
        l1row("PAYHOLIDAYKBN") = String.Empty                                              '休日区分
        l1row("PAYOILKBN") = String.Empty                                                        '勤怠用油種区分
        l1row("PAYSHARYOKBN") = String.Empty                                                     '勤怠用車両区分

        '勘定科目判定テーブル検索
        CS0038ACCODEget.TBL = ML002tbl                                             '勘定科目判定テーブル
        CS0038ACCODEget.CAMPCODE = l1row("CAMPCODE")                             '会社コード
        CS0038ACCODEget.STYMD = l1row("KEIJOYMD")                                '開始日
        CS0038ACCODEget.ENDYMD = l1row("KEIJOYMD")                               '終了日
        CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
        CS0038ACCODEget.DENTYPE = "T05"                                            '伝票タイプ

        CS0038ACCODEget.TORICODE = l1row("NACTORICODE")                         '荷主コード
        CS0038ACCODEget.TORITYPE01 = l1row("NACTORITYPE01")                     '取引タイプ01
        CS0038ACCODEget.TORITYPE02 = l1row("NACTORITYPE02")                     '取引タイプ02
        CS0038ACCODEget.TORITYPE03 = l1row("NACTORITYPE03")                     '取引タイプ03
        CS0038ACCODEget.TORITYPE04 = l1row("NACTORITYPE04")                     '取引タイプ04
        CS0038ACCODEget.TORITYPE05 = l1row("NACTORITYPE05")                     '取引タイプ05
        CS0038ACCODEget.URIKBN = l1row("NACURIKBN")                             '売上計上基準
        CS0038ACCODEget.STORICODE = l1row("NACSTORICODE")                       '販売店コード
        CS0038ACCODEget.OILTYPE = l1row("NACOILTYPE")                           '油種
        CS0038ACCODEget.PRODUCT1 = l1row("NACPRODUCT1")                         '品名１
        CS0038ACCODEget.SUPPLIERKBN = l1row("NACSUPPLIERKBN")                   '社有・庸車区分
        CS0038ACCODEget.MANGSORG = l1row("NACMANGSORG1")                        '車両設置部署
        CS0038ACCODEget.MANGUORG = l1row("NACMANGUORG1")                        '車両運用部署
        CS0038ACCODEget.BASELEASE = l1row("NACBASELEASE1")                      '車両所有
        CS0038ACCODEget.STAFFKBN = l1row("NACSTAFFKBN")                         '社員区分
        CS0038ACCODEget.HORG = l1row("NACHORG")                                 '配属部署
        CS0038ACCODEget.SORG = l1row("NACSORG")                                 '作業部署

        '勘定科目判定テーブル検索（借方）
        CS0038ACCODEget.ACHANTEI = "TUD"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_D = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_D = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_D = CS0038ACCODEget.INQKBN

        '勘定科目判定テーブル検索（貸方）
        CS0038ACCODEget.ACHANTEI = "TUC"                                            '勘定科目判定コード
        CS0038ACCODEget.CS0038ACCODEget()
        WW_ACCODE_C = CS0038ACCODEget.ACCODE
        WW_SUBACCODE_C = CS0038ACCODEget.SUBACCODE
        WW_INQKBN_C = CS0038ACCODEget.INQKBN


        Dim WW_ROW As DataRow

        '------------------------------------------------------
        '追加データ
        '------------------------------------------------------
        If t5row("DELFLG") = C_DELETE_FLG.ALIVE Then
            '●借方
            If WW_INQKBN_D = "1" Then
                l1row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_D                                 '照会区分
                l1row("ACDCKBN") = "D"                                        '貸借区分
                l1row("ACACHANTEI") = "TUD"                                   '勘定科目判定コード
                l1row("DTLNO") = "01"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If

            '●貸方
            If WW_INQKBN_C = "1" Then
                l1row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                l1row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                l1row("INQKBN") = WW_INQKBN_C                                 '照会区分
                l1row("ACDCKBN") = "C"                                        '貸借区分
                l1row("ACACHANTEI") = "TUC"                                   '勘定科目判定コード
                l1row("DTLNO") = "02"                                         '明細番号

                WW_ROW = IO_L1TBL.NewRow
                WW_ROW.ItemArray = l1row.ItemArray
                IO_L1TBL.Rows.Add(WW_ROW)
            End If
        End If

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' １次サマリー
    ''' </summary>
    ''' <param name="I_CAMPCODE"></param>
    ''' <param name="I_SORG"></param>
    ''' <param name="I_USERID"></param>
    ''' <param name="IO_L1TBL"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Public Sub SumL00001(ByVal I_CAMPCODE As String, ByVal I_SORG As String, ByVal I_USERID As String, ByRef IO_L1TBL As DataTable, ByRef O_RTN As String)
        Dim CS0033 As New CS0033AutoNumber            '伝票番号採番

        Dim wINT As Integer
        Dim wDouble As Double

        Dim L00001URtbl As DataTable = IO_L1TBL.Clone
        Dim L00001SUMtbl As DataTable = IO_L1TBL.Clone
        Dim L00001SUMrow As DataRow = Nothing
        Dim L00001SVrow As DataRow = Nothing

        Dim WW_ACACHANTEI As String = String.Empty
        Dim WW_NACSHUKODATE As String = String.Empty
        Dim WW_NACSHUKADATE As String = String.Empty
        Dim WW_NACTODOKEDATE As String = String.Empty
        Dim WW_KEIJOYMD As String = String.Empty
        Dim WW_NACTORICODE As String = String.Empty
        Dim WW_NACSHIPORG As String = String.Empty
        Dim WW_KEYGSHABAN As String = String.Empty
        Dim WW_NACSTAFFCODE As String = String.Empty
        Dim WW_KEYTRIPNO As String = String.Empty
        Dim WW_KEYDROPNO As String = String.Empty
        Dim WW_NACSEQ As String = String.Empty
        Dim WW_NACSURYOG As Double = 0
        Dim WW_NACJSURYOG As Double = 0
        Dim WW_FIRST As Boolean = False
        Dim WW_CNT As Integer = 0

        '***********************************************************************************************
        '一時サマリ（出庫日、出荷日、届日、荷主、出荷部署、業務車番、乗務員、トリップ、ドロップ別）
        '***********************************************************************************************
        '売上抽出
        CS0026TBLSort.TABLE = IO_L1TBL
        CS0026TBLSort.FILTER = "ACACHANTEI = 'URC' or ACACHANTEI = 'URD'"
        CS0026TBLSort.SORTING = "ACACHANTEI,NACSHUKODATE,NACSHUKADATE,NACTODOKEDATE,KEIJOYMD,NACTORICODE,NACSHIPORG,KEYGSHABAN,NACSTAFFCODE,KEYTRIPNO,KEYDROPNO,NACSEQ"
        L00001URtbl = CS0026TBLSort.sort()

        '売上以外抽出
        CS0026TBLSort.TABLE = IO_L1TBL
        CS0026TBLSort.FILTER = "ACACHANTEI <> 'URC' and ACACHANTEI <> 'URD'"
        CS0026TBLSort.SORTING = "ACACHANTEI,NACSHUKODATE,NACSHUKADATE,NACTODOKEDATE,KEIJOYMD,NACTORICODE,NACSHIPORG,KEYGSHABAN,NACSTAFFCODE,KEYTRIPNO,KEYDROPNO,NACSEQ"
        IO_L1TBL = CS0026TBLSort.sort()

        WW_ACACHANTEI = String.Empty
        WW_NACSHUKODATE = String.Empty
        WW_NACSHUKADATE = String.Empty
        WW_NACTODOKEDATE = String.Empty
        WW_KEIJOYMD = String.Empty
        WW_NACTORICODE = String.Empty
        WW_NACSHIPORG = String.Empty
        WW_KEYGSHABAN = String.Empty
        WW_NACSTAFFCODE = String.Empty
        WW_KEYTRIPNO = String.Empty
        WW_KEYDROPNO = String.Empty
        WW_NACSEQ = String.Empty
        WW_FIRST = False
        L00001SUMtbl.Clear()
        L00001SUMrow = Nothing
        L00001SVrow = Nothing

        For Each L00001WKrow As DataRow In IO_L1TBL.Rows

            If Not WW_FIRST Then
                WW_ACACHANTEI = L00001WKrow("ACACHANTEI")
                WW_NACSHUKODATE = L00001WKrow("NACSHUKODATE")
                WW_NACSHUKADATE = L00001WKrow("NACSHUKADATE")
                WW_NACTODOKEDATE = L00001WKrow("NACTODOKEDATE")
                WW_KEIJOYMD = L00001WKrow("KEIJOYMD")
                WW_NACTORICODE = L00001WKrow("NACTORICODE")
                WW_NACSHIPORG = L00001WKrow("NACSHIPORG")
                WW_KEYGSHABAN = L00001WKrow("KEYGSHABAN")
                WW_NACSTAFFCODE = L00001WKrow("NACSTAFFCODE")
                WW_KEYTRIPNO = L00001WKrow("KEYTRIPNO")
                WW_KEYDROPNO = L00001WKrow("KEYDROPNO")
                WW_NACSEQ = L00001WKrow("NACSEQ")

                L00001SVrow = L00001SUMtbl.NewRow
                L00001SVrow.ItemArray = L00001WKrow.ItemArray
                'サマリー項目初期化
                InitialSumItem(L00001SVrow)
                WW_FIRST = True
            End If

            If L00001WKrow("ACACHANTEI") = WW_ACACHANTEI AndAlso
               L00001WKrow("NACSHUKODATE") = WW_NACSHUKODATE AndAlso
               L00001WKrow("NACSHUKADATE") = WW_NACSHUKADATE AndAlso
               L00001WKrow("NACTODOKEDATE") = WW_NACTODOKEDATE AndAlso
               L00001WKrow("KEIJOYMD") = WW_KEIJOYMD AndAlso
               L00001WKrow("NACTORICODE") = WW_NACTORICODE AndAlso
               L00001WKrow("NACSHIPORG") = WW_NACSHIPORG AndAlso
               L00001WKrow("KEYGSHABAN") = WW_KEYGSHABAN AndAlso
               L00001WKrow("NACSTAFFCODE") = WW_NACSTAFFCODE AndAlso
               L00001WKrow("KEYTRIPNO") = WW_KEYTRIPNO AndAlso
               L00001WKrow("KEYDROPNO") = WW_KEYDROPNO AndAlso
               L00001WKrow("NACSEQ") = WW_NACSEQ Then
            Else
                L00001SUMrow = L00001SUMtbl.NewRow
                L00001SUMrow.ItemArray = L00001SVrow.ItemArray

                'サマリー結果保存
                L00001SUMtbl.Rows.Add(L00001SUMrow)


                '次処理データを設定
                L00001SVrow = L00001SUMtbl.NewRow
                L00001SVrow.ItemArray = L00001WKrow.ItemArray
                'サマリー項目初期化
                InitialSumItem(L00001SVrow)
            End If

            '労務費（配送作業）
            If L00001WKrow("ACACHANTEI") = "HSC" OrElse L00001WKrow("ACACHANTEI") = "HSD" Then
                Try
                    wDouble = L00001WKrow("NACHAIDISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACHAIDISTANCE") = Val(L00001SVrow("NACHAIDISTANCE")) + wDouble               '実績・配送距離
            End If

            '労務費（回送）
            If L00001WKrow("ACACHANTEI") = "KSC" OrElse L00001WKrow("ACACHANTEI") = "KSD" Then
                Try
                    wDouble = L00001WKrow("NACKAIDISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACKAIDISTANCE") = Val(L00001SVrow("NACKAIDISTANCE")) + wDouble               '実績・下車作業距離
            End If

            Try
                wDouble = L00001WKrow("NACCHODISTANCE")
            Catch ex As Exception
                wDouble = 0
            End Try
            L00001SVrow("NACCHODISTANCE") = Val(L00001SVrow("NACCHODISTANCE")) + wDouble                   '実績・勤怠調整距離

            Try
                wDouble = L00001WKrow("NACTTLDISTANCE")
            Catch ex As Exception
                wDouble = 0
            End Try
            L00001SVrow("NACTTLDISTANCE") = Val(L00001SVrow("NACTTLDISTANCE")) + wDouble                   '実績・配送距離合計Σ

            '労務費（配送作業）
            If L00001WKrow("ACACHANTEI") = "HSC" OrElse L00001WKrow("ACACHANTEI") = "HSD" Then
                L00001SVrow("NACHAISTDATE") = L00001WKrow("NACHAISTDATE")                                  '実績・配送作業開始日時
                L00001SVrow("NACHAIENDDATE") = L00001WKrow("NACHAIENDDATE")                                '実績・配送作業終了日時
                Try
                    wINT = L00001WKrow("NACHAIWORKTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACHAIWORKTIME") = Val(L00001SVrow("NACHAIWORKTIME")) + wINT               '実績・配送作業時間
            End If

            '労務費（回送）
            If L00001WKrow("ACACHANTEI") = "KSC" OrElse L00001WKrow("ACACHANTEI") = "KSD" Then
                L00001SVrow("NACGESSTDATE") = C_DEFAULT_YMD                                              '実績・下車作業開始日時
                L00001SVrow("NACGESENDDATE") = C_DEFAULT_YMD                                             '実績・下車作業終了日時
                Try
                    wINT = L00001WKrow("NACGESWORKTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACGESWORKTIME") = Val(L00001SVrow("NACGESWORKTIME")) + wINT               '実績・下車作業時間
            End If


            Try
                wINT = L00001WKrow("NACCHOWORKTIME")
            Catch ex As Exception
                wINT = 0
            End Try
            L00001SVrow("NACCHOWORKTIME") = Val(L00001SVrow("NACCHOWORKTIME")) + wINT                   '実績・勤怠調整時間

            Try
                wINT = L00001WKrow("NACTTLWORKTIME")
            Catch ex As Exception
                wINT = 0
            End Try
            L00001SVrow("NACTTLWORKTIME") = Val(L00001SVrow("NACTTLWORKTIME")) + wINT                   '実績・配送合計時間Σ


            '労務費（配送作業）& 労務費（回送）
            If L00001WKrow("ACACHANTEI") = "HSC" OrElse
               L00001WKrow("ACACHANTEI") = "HSD" OrElse
               L00001WKrow("ACACHANTEI") = "KSC" OrElse
               L00001WKrow("ACACHANTEI") = "KSD" Then
                Try
                    wINT = L00001WKrow("NACOUTWORKTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACOUTWORKTIME") = Val(L00001SVrow("NACOUTWORKTIME")) + wINT               '実績・就業外時間
            End If


            '労務費（休憩）
            If L00001WKrow("ACACHANTEI") = "RSC" OrElse L00001WKrow("ACACHANTEI") = "RSD" Then
                L00001SVrow("NACBREAKSTDATE") = C_DEFAULT_YMD                                            '実績・休憩開始日時
                L00001SVrow("NACBREAKENDDATE") = C_DEFAULT_YMD                                           '実績・休憩終了日時
                Try
                    wINT = L00001WKrow("NACBREAKTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACBREAKTIME") = Val(L00001SVrow("NACBREAKTIME")) + wINT                  '実績・休憩時間
                Try
                    wINT = L00001WKrow("NACCHOBREAKTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACCHOBREAKTIME") = Val(L00001SVrow("NACCHOBREAKTIME")) + wINT            '実績・休憩調整時間
                Try
                    wINT = L00001WKrow("NACTTLBREAKTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACTTLBREAKTIME") = Val(L00001SVrow("NACTTLBREAKTIME")) + wINT            '実績・休憩合計時間Σ
            End If

            '通行料
            If L00001WKrow("ACACHANTEI") = "TUC" OrElse L00001WKrow("ACACHANTEI") = "TUD" Then
                Try
                    wINT = L00001WKrow("NACCASH")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACCASH") = Val(L00001SVrow("NACCASH")) + wINT                            '実績・現金

                Try
                    wINT = L00001WKrow("NACETC")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACETC") = Val(L00001SVrow("NACETC")) + wINT                              '実績・ETC

                Try
                    wINT = L00001WKrow("NACTICKET")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACTICKET") = Val(L00001SVrow("NACTICKET")) + wINT                        '実績・回数券
            End If

            '軽油
            If L00001WKrow("ACACHANTEI") = "KEC" OrElse L00001WKrow("ACACHANTEI") = "KED" Then
                Try
                    wDouble = L00001WKrow("NACKYUYU")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACKYUYU") = Val(L00001SVrow("NACKYUYU")) + wDouble                          '実績・軽油
            End If

            '労務費（配送作業）
            If L00001WKrow("ACACHANTEI") = "HSC" OrElse L00001WKrow("ACACHANTEI") = "HSD" Then
                Try
                    wINT = L00001WKrow("NACUNLOADCNT")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACUNLOADCNT") = Val(L00001SVrow("NACUNLOADCNT")) + wINT                  '実績・荷卸回数
                Try
                    wINT = L00001WKrow("NACCHOUNLOADCNT")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACCHOUNLOADCNT") = Val(L00001SVrow("NACCHOUNLOADCNT")) + wINT            '実績・荷卸回数調整
                Try
                    wINT = L00001WKrow("NACTTLUNLOADCNT")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACTTLUNLOADCNT") = Val(L00001SVrow("NACTTLUNLOADCNT")) + wINT            '実績・荷卸回数合計Σ
            End If

            '車両稼動（自社＆リース）
            If L00001WKrow("ACACHANTEI") = "HJC" OrElse
               L00001WKrow("ACACHANTEI") = "HJD" OrElse
               L00001WKrow("ACACHANTEI") = "HLC" OrElse
               L00001WKrow("ACACHANTEI") = "HLD" Then

                If Val(L00001WKrow("NACKAIJI")) = 0 Then
                Else
                    Try
                        wINT = L00001WKrow("NACKAIJI")
                    Catch ex As Exception
                        wINT = 0
                    End Try
                    L00001SVrow("NACKAIJI") = wINT                                                     '実績・回次
                End If

                Try
                    wINT = L00001WKrow("NACJITIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACJITIME") = Val(L00001SVrow("NACJITIME")) + wINT                        '実績・実車時間
                Try
                    wINT = L00001WKrow("NACJICHOSTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACJICHOSTIME") = Val(L00001SVrow("NACJICHOSTIME")) + wINT                '実績・実車時間調整
                Try
                    wINT = L00001WKrow("NACJITTLETIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACJITTLETIME") = Val(L00001SVrow("NACJITTLETIME")) + wINT                '実績・実車時間合計Σ
            End If


            '車両稼動（自社＆リース）、回送（自社＆リース）
            If L00001WKrow("ACACHANTEI") = "HJC" OrElse L00001WKrow("ACACHANTEI") = "HJD" OrElse
               L00001WKrow("ACACHANTEI") = "HLC" OrElse L00001WKrow("ACACHANTEI") = "HLD" OrElse
               L00001WKrow("ACACHANTEI") = "KJC" OrElse L00001WKrow("ACACHANTEI") = "KJD" OrElse
               L00001WKrow("ACACHANTEI") = "KLC" OrElse L00001WKrow("ACACHANTEI") = "KLD" Then
                Try
                    wINT = L00001WKrow("NACKUTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACKUTIME") = Val(L00001SVrow("NACKUTIME")) + wINT                        '実績・空車時間
                Try
                    wINT = L00001WKrow("NACKUCHOTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACKUCHOTIME") = Val(L00001SVrow("NACKUCHOTIME")) + wINT                  '実績・空車時間調整
                Try
                    wINT = L00001WKrow("NACKUTTLTIME")
                Catch ex As Exception
                    wINT = 0
                End Try
                L00001SVrow("NACKUTTLTIME") = Val(L00001SVrow("NACKUTTLTIME")) + wINT                  '実績・空車時間合計Σ
            End If

            '車両稼動（自社＆リース）
            If L00001WKrow("ACACHANTEI") = "HJC" OrElse L00001WKrow("ACACHANTEI") = "HJD" OrElse
               L00001WKrow("ACACHANTEI") = "HLC" OrElse L00001WKrow("ACACHANTEI") = "HLD" Then
                Try
                    wDouble = L00001WKrow("NACJIDISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACJIDISTANCE") = Val(L00001SVrow("NACJIDISTANCE")) + wDouble                '実績・実車距離
                Try
                    wDouble = L00001WKrow("NACJICHODISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACJICHODISTANCE") = Val(L00001SVrow("NACJICHODISTANCE")) + wDouble          '実績・実車距離調整
                Try
                    wDouble = L00001WKrow("NACJITTLDISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACJITTLDISTANCE") = Val(L00001SVrow("NACJITTLDISTANCE")) + wDouble          '実績・実車距離合計Σ
            End If

            '車両稼動（自社＆リース）、回送（自社＆リース）
            If L00001WKrow("ACACHANTEI") = "HJC" OrElse L00001WKrow("ACACHANTEI") = "HJD" OrElse
               L00001WKrow("ACACHANTEI") = "HLC" OrElse L00001WKrow("ACACHANTEI") = "HLD" OrElse
               L00001WKrow("ACACHANTEI") = "KJC" OrElse L00001WKrow("ACACHANTEI") = "KJD" OrElse
               L00001WKrow("ACACHANTEI") = "KLC" OrElse L00001WKrow("ACACHANTEI") = "KLD" Then
                Try
                    wDouble = L00001WKrow("NACKUDISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACKUDISTANCE") = Val(L00001SVrow("NACKUDISTANCE")) + wDouble                '実績・空車距離
                Try
                    wDouble = L00001WKrow("NACKUCHODISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACKUCHODISTANCE") = Val(L00001SVrow("NACKUCHODISTANCE")) + wDouble          '実績・空車距離調整
                Try
                    wDouble = L00001WKrow("NACKUTTLDISTANCE")
                Catch ex As Exception
                    wDouble = 0
                End Try
                L00001SVrow("NACKUTTLDISTANCE") = Val(L00001SVrow("NACKUTTLDISTANCE")) + wDouble          '実績・空車距離合計Σ
            End If

            L00001SVrow("PAYSTAFFKBN") = L00001WKrow("PAYSTAFFKBN")                                 '社員区分
            L00001SVrow("PAYSTAFFCODE") = L00001WKrow("PAYSTAFFCODE")                               '従業員
            L00001SVrow("PAYMORG") = L00001WKrow("PAYMORG")                                         '従業員管理部署
            L00001SVrow("PAYHORG") = L00001WKrow("PAYHORG")                                         '従業員配属部署
            L00001SVrow("PAYOILKBN") = L00001WKrow("PAYOILKBN")                                     '勤怠用油種区分
            L00001SVrow("PAYSHARYOKBN") = L00001WKrow("PAYSHARYOKBN")                               '勤怠用車両区分
            L00001SVrow("WORKKBN") = String.Empty

            WW_ACACHANTEI = L00001WKrow("ACACHANTEI")
            WW_NACSHUKODATE = L00001WKrow("NACSHUKODATE")
            WW_NACSHUKADATE = L00001WKrow("NACSHUKADATE")
            WW_NACTODOKEDATE = L00001WKrow("NACTODOKEDATE")
            WW_KEIJOYMD = L00001WKrow("KEIJOYMD")
            WW_NACTORICODE = L00001WKrow("NACTORICODE")
            WW_NACSHIPORG = L00001WKrow("NACSHIPORG")
            WW_KEYGSHABAN = L00001WKrow("KEYGSHABAN")
            WW_NACSTAFFCODE = L00001WKrow("NACSTAFFCODE")
            WW_KEYTRIPNO = L00001WKrow("KEYTRIPNO")
            WW_KEYDROPNO = L00001WKrow("KEYDROPNO")
            WW_NACSEQ = L00001WKrow("NACSEQ")

        Next
        '最終レコードの出力
        If IO_L1TBL.Rows.Count > 0 Then
            L00001SUMrow = L00001SUMtbl.NewRow
            L00001SUMrow.ItemArray = L00001SVrow.ItemArray
            L00001SUMtbl.Rows.Add(L00001SUMrow)
        End If


        'サマリー結果で入れ替え
        IO_L1TBL = L00001SUMtbl.Copy
        IO_L1TBL.Merge(L00001URtbl)

        '---------------------------------------------------------------
        '正規伝票№採番（仮伝票№（貸借同一連番）でソート）
        '---------------------------------------------------------------
        CS0026TBLSort.TABLE = IO_L1TBL
        CS0026TBLSort.FILTER = String.Empty
        CS0026TBLSort.SORTING = "DENNO, ACACHANTEI"
        IO_L1TBL = CS0026TBLSort.sort()

        Dim OLD_DENNO As String = String.Empty
        Dim NEW_DENNO As String = String.Empty
        Dim WW_SEQ As String = String.Empty
        For Each L00001WKrow As DataRow In IO_L1TBL.Rows
            NEW_DENNO = L00001WKrow("DENNO")

            If OLD_DENNO <> NEW_DENNO Then
                '伝票番号採番
                WW_SEQ = "000000"
                CS0033.CAMPCODE = L00001WKrow("CAMPCODE")
                CS0033.MORG = I_SORG
                CS0033.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033.USERID = I_USERID
                CS0033.getAutoNumber()
                If isNormal(CS0033.ERR) Then
                    WW_SEQ = CS0033.SEQ
                Else
                    O_RTN = CS0033.ERR
                    Exit Sub
                End If
                '伝票番号
                L00001WKrow("DENNO") = I_SORG &
                                    CDate(L00001WKrow("KEIJOYMD")).ToString("yyyy") &
                                    WW_SEQ
            Else
                '伝票番号
                L00001WKrow("DENNO") = I_SORG &
                                    CDate(L00001WKrow("KEIJOYMD")).ToString("yyyy") &
                                    WW_SEQ
            End If

            OLD_DENNO = NEW_DENNO
        Next
        L00001SUMtbl.Clear()
        L00001SUMtbl = Nothing
        L00001URtbl.Clear()
        L00001URtbl = Nothing

    End Sub

    ''' <summary>
    ''' サマリー項目初期化処理
    ''' </summary>
    ''' <param name="IO_ROW">初期化対象行</param>
    ''' <remarks></remarks>
    Private Sub InitialSumItem(ByRef IO_ROW As DataRow)

        IO_ROW("NACCASH") = 0                           '現金
        IO_ROW("NACETC") = 0                            'ＥＴＣ
        IO_ROW("NACTICKET") = 0                         'チケット
        IO_ROW("NACKYUYU") = 0                          '軽油

        IO_ROW("NACSURYO") = 0                          '受注・数量
        IO_ROW("NACJSURYO") = 0                         '実績・配送数量
        IO_ROW("NACHAIDISTANCE") = 0                    '実績・配送距離
        IO_ROW("NACKAIDISTANCE") = 0                    '実績・下車作業距離
        IO_ROW("NACCHODISTANCE") = 0                    '実績・勤怠調整距離
        IO_ROW("NACTTLDISTANCE") = 0                    '実績・配送距離合計Σ
        IO_ROW("NACHAIWORKTIME") = 0                    '実績・配送作業時間
        IO_ROW("NACGESWORKTIME") = 0                    '実績・下車作業時間
        IO_ROW("NACCHOWORKTIME") = 0                    '実績・勤怠調整時間
        IO_ROW("NACTTLWORKTIME") = 0                    '実績・配送合計時間Σ
        IO_ROW("NACOUTWORKTIME") = 0                    '実績・就業外時間
        IO_ROW("NACBREAKTIME") = 0                      '実績・休憩時間
        IO_ROW("NACTTLBREAKTIME") = 0                   '実績・休憩合計時間Σ
        IO_ROW("NACUNLOADCNT") = 0                      '実績・荷卸回数
        IO_ROW("NACCHOUNLOADCNT") = 0                   '実績・荷卸回数調整
        IO_ROW("NACTTLUNLOADCNT") = 0                   '実績・荷卸回数合計Σ
        IO_ROW("NACJITIME") = 0                         '実績・実車時間
        IO_ROW("NACJICHOSTIME") = 0                     '実績・実車時間調整
        IO_ROW("NACJITTLETIME") = 0                     '実績・実車時間合計Σ
        IO_ROW("NACKUTIME") = 0                         '実績・空車時間
        IO_ROW("NACKUCHOTIME") = 0                      '実績・空車時間調整
        IO_ROW("NACKUTTLTIME") = 0                      '実績・空車時間合計Σ
        IO_ROW("NACJIDISTANCE") = 0                     '実績・実車距離
        IO_ROW("NACJICHODISTANCE") = 0                  '実績・実車距離調整
        IO_ROW("NACJITTLDISTANCE") = 0                  '実績・実車距離合計Σ
        IO_ROW("NACKUDISTANCE") = 0                     '実績・空車距離
        IO_ROW("NACKUCHODISTANCE") = 0                  '実績・空車距離調整
        IO_ROW("NACKUTTLDISTANCE") = 0                  '実績・空車距離合計Σ

    End Sub
    ''' <summary>
    ''' 文字列を実数か確認した後、書式の形に変換する。
    ''' </summary>
    ''' <param name="I_PARAM">変換したい文字列（実数型）</param>
    ''' <param name="I_FORMAT">変換書式</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function TryConvDouble(ByVal I_PARAM As String, ByVal I_FORMAT As String) As String

        Dim W_DOUBLE As Double
        If IsDBNull(I_PARAM) OrElse String.IsNullOrEmpty(I_PARAM) Then Return 0.ToString(I_FORMAT)

        Try
            W_DOUBLE = CDbl(I_PARAM)
        Catch ex As Exception
            W_DOUBLE = 0
        End Try
        Return W_DOUBLE.ToString(I_FORMAT)
    End Function

    ''' <summary>
    ''' 時間の書式変更
    ''' </summary>
    ''' <param name="I_PARAM">変更対象の時刻(Minutes)</param>
    ''' <returns>変更後の時刻(HH:MM)</returns>
    ''' <remarks></remarks>
    Friend Function MinutesToHHMM(ByVal I_PARAM As Integer) As String
        Dim WW_HHMM As Integer = 0
        Dim WW_ABS As Integer = System.Math.Abs(I_PARAM)

        WW_HHMM = Int(WW_ABS / 60) * 100 + WW_ABS Mod 60
        If I_PARAM < 0 Then
            WW_HHMM = WW_HHMM * -1
        End If
        Return Format(WW_HHMM, "0#:##")
    End Function
    ''' <summary>
    ''' 時間の書式変更
    ''' </summary>
    ''' <param name="I_PARM">変更対象の時刻(HH:MM)</param>
    ''' <returns>変更後の時刻(Minutes)</returns>
    ''' <remarks></remarks>
    Friend Function HHMMToMinutes(ByVal I_PARM As String) As Integer
        Dim WW_TIME As String() = {}
        Dim WW_SIGN As String = "+"
        Dim WW_MINUITES As Integer = 0
        If Not IsNothing(I_PARM) Then
            If Mid(I_PARM, 1, 1) = "-" Then
                WW_SIGN = "-"
                WW_TIME = I_PARM.Replace("-", "").Split(":")
            Else
                WW_SIGN = "+"
                WW_TIME = I_PARM.Split(":")
            End If
            If WW_TIME.Count > 1 Then
                WW_MINUITES = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                If WW_SIGN = "-" Then
                    WW_MINUITES = WW_MINUITES * -1
                End If
            End If
        End If
        Return WW_MINUITES

    End Function

    ''' <summary>
    ''' 車検切れ・容器検査切れチェック対象部署
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Function IsInspectionOrg(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByRef O_RTN As String) As Boolean

        Const CLASS_CODE As String = "INSPECTIONORG"
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using GS0032 As New GS0032FIXVALUElst
                GS0032.CAMPCODE = I_COMPCODE
                GS0032.CLAS = CLASS_CODE
                GS0032.STDATE = Date.Now
                GS0032.ENDDATE = Date.Now
                GS0032.GS0032FIXVALUElst()
                If Not isNormal(GS0032.ERR) Then
                    O_RTN = GS0032.ERR
                    Return False
                End If
                '存在する場合TRUE、しない場合FALSEを帰す
                Return (Not IsNothing(GS0032.VALUE1.Items.FindByValue(I_ORGCODE)))
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GRT0005COM"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:INSPECTIONORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Return False
        End Try

    End Function
    ''' <summary>
    ''' 車庫チェック
    ''' </summary>
    ''' <param name="I_CAMPCODE">会社コード</param>
    ''' <param name="I_LATITUDE">チェック対象緯度</param>
    ''' <param name="I_LONGITUDE">チェック対象経度</param>
    ''' <returns>範囲内判定　TRUE:範囲内　FALSE:範囲外</returns>
    Public Function ShakoCheck(ByVal I_CAMPCODE As String, ByVal I_LATITUDE As String, ByVal I_LONGITUDE As String) As String

        '○　緯度ListBox
        Dim ListBoxLATITUDE As ListBox = New ListBox
        '○　経度ListBox
        Dim ListBoxLONGITUDE As ListBox = New ListBox

        ShakoCheck = "NG"

        '緯度経度が空の時、判断できないため車庫とみなす
        If I_LATITUDE = String.Empty OrElse I_LONGITUDE = String.Empty Then
            ShakoCheck = "OK"
            Exit Function
        End If

        Using GS0007FIXVALUElst As New GS0007FIXVALUElst
            '○　緯度ListBox設定              
            GS0007FIXVALUElst.CAMPCODE = I_CAMPCODE
            GS0007FIXVALUElst.CLAS = "IDOKEIDO"
            GS0007FIXVALUElst.LISTBOX1 = ListBoxLATITUDE
            GS0007FIXVALUElst.LISTBOX2 = ListBoxLONGITUDE
            GS0007FIXVALUElst.GS0007FIXVALUElst()
            If isNormal(GS0007FIXVALUElst.ERR) Then
                ListBoxLATITUDE = GS0007FIXVALUElst.LISTBOX1
                ListBoxLONGITUDE = GS0007FIXVALUElst.LISTBOX2
            Else
                ShakoCheck = "NG"
                Exit Function
            End If
        End Using
        '緯度・経度（車庫）判定
        For i As Integer = 0 To ListBoxLATITUDE.Items.Count - 1
            If I_LATITUDE Like ListBoxLATITUDE.Items(i).Text AndAlso
               I_LONGITUDE Like ListBoxLONGITUDE.Items(i).Text Then
                ShakoCheck = "OK"
                Exit For
            End If
        Next

    End Function

    ''' <summary>
    ''' 光英連携可能部署
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="I_CLASS">区分</param>
    ''' <param name="O_RTN">ERRCODE</param>
    ''' <returns>可否判定</returns>
    ''' <remarks></remarks>
    Public Function IsKoueiAvailableOrg(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByVal I_CLASS As String, ByRef O_RTN As String) As Boolean

        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using GS0032 As New GS0032FIXVALUElst
                GS0032.CAMPCODE = I_COMPCODE
                GS0032.CLAS = I_CLASS
                GS0032.STDATE = Date.Now
                GS0032.ENDDATE = Date.Now
                GS0032.GS0032FIXVALUElst()
                If Not isNormal(GS0032.ERR) Then
                    O_RTN = GS0032.ERR
                    Return False
                End If
                '存在する場合TRUE、しない場合FALSEを帰す
                Return (Not IsNothing(GS0032.VALUE1.Items.FindByValue(I_ORGCODE)))
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GRT0005COM"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:KoueiAvailableOrg Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Return False
        End Try

    End Function
End Class


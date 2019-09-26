Imports System.Data.SqlClient


'■勤怠共通
Public Class GRT0007COM

    '統計DB出力dll Interface
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    Public Property T0007tbl As DataTable                                     '日報テーブル
    Public Property ERR As String                                             'リターン値

    Private CS0011LOGWRITE As New CS0011LOGWrite                              'LogOutput DirString Get
    Private CS0026TblSort As New CS0026TBLSORT                                'テーブルソート
    Private CS0050Session As New CS0050SESSION                                'セッション管理
    Private CS0038ACCODEget As New CS0038ACCODEget                            '勘定科目取得
    Private CS0006TERMchk As New CS0006TERMchk                                '端末IDチェック
    Private CS0033AutoNumber As New CS0033AutoNumber                          '採番
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst                        'Leftボックス用固定値リスト取得

    ' ***  月調整レコードの作成
    Public Sub T0007_ChoseiRecodeCreate(ByRef ioTbl As DataTable)

        Dim WW_IDX As Integer = 0

        Try
            Dim WW_T0007TTLtbl As DataTable = ioTbl.Clone
            Dim WW_T0007ETCtbl As DataTable = ioTbl.Clone
            For i As Integer = 0 To ioTbl.Rows.Count - 1
                Dim ioTblrow As DataRow = ioTbl.Rows(i)

                '勤怠のヘッダレコードを取得（月調整を再作成するため捨てる）
                If ioTblrow("SELECT") = "1" AndAlso ioTblrow("HDKBN") = "H" AndAlso ioTblrow("RECODEKBN") = "1" Then
                    Continue For
                ElseIf ioTblrow("SELECT") = "1" AndAlso ioTblrow("HDKBN") = "H" AndAlso ioTblrow("RECODEKBN") = "2" Then
                    Dim HEADrow As DataRow = WW_T0007TTLtbl.NewRow
                    HEADrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007TTLtbl.Rows.Add(HEADrow)
                Else
                    '勤怠の明細レコードと削除レコードを取得
                    Dim DTLrow As DataRow = WW_T0007ETCtbl.NewRow
                    DTLrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007ETCtbl.Rows.Add(DTLrow)
                End If
            Next

            '月調整レコードを１レコード毎回作成する（DB出力しない、抽出が容易な合計レコードから作成することにした）
            Dim WW_T0007CHOtbl As DataTable = ioTbl.Clone

            For Each WW_TTLrow As DataRow In WW_T0007TTLtbl.Rows
                '月調整レコード作成準備
                Dim WW_CHOrow As DataRow = WW_T0007CHOtbl.NewRow
                WW_CHOrow.ItemArray = WW_TTLrow.ItemArray
                SumItem_Init(WW_CHOrow)

                WW_CHOrow("WORKTIME") = "00:00"
                WW_CHOrow("MOVETIME") = "00:00"
                WW_CHOrow("ACTTIME") = "00:00"
                WW_CHOrow("HAIDISTANCETTL") = Val(WW_CHOrow("HAIDISTANCE")) + Val(WW_CHOrow("HAIDISTANCECHO"))
                WW_CHOrow("KAIDISTANCETTL") = Val(WW_CHOrow("KAIDISTANCE")) + Val(WW_CHOrow("KAIDISTANCECHO"))
                WW_CHOrow("UNLOADCNTTTL") = Val(WW_CHOrow("UNLOADCNT")) + Val(WW_CHOrow("UNLOADCNTCHO"))
                WW_CHOrow("BREAKTIMETTL") = HHMMtoMinutes(WW_CHOrow("NIPPOBREAKTIME")) + HHMMtoMinutes(WW_CHOrow("BREAKTIME")) + HHMMtoMinutes(WW_CHOrow("BREAKTIMECHO"))
                WW_CHOrow("NIGHTTIMETTL") = HHMMtoMinutes(WW_CHOrow("NIGHTTIME")) + HHMMtoMinutes(WW_CHOrow("NIGHTTIMECHO"))
                WW_CHOrow("ORVERTIMETTL") = HHMMtoMinutes(WW_CHOrow("ORVERTIME")) + HHMMtoMinutes(WW_CHOrow("ORVERTIMECHO"))
                WW_CHOrow("WNIGHTTIMETTL") = HHMMtoMinutes(WW_CHOrow("WNIGHTTIME")) + HHMMtoMinutes(WW_CHOrow("WNIGHTTIMECHO"))
                WW_CHOrow("SWORKTIMETTL") = HHMMtoMinutes(WW_CHOrow("SWORKTIME")) + HHMMtoMinutes(WW_CHOrow("SWORKTIMECHO"))
                WW_CHOrow("SNIGHTTIMETTL") = HHMMtoMinutes(WW_CHOrow("SNIGHTTIME")) + HHMMtoMinutes(WW_CHOrow("SNIGHTTIMECHO"))
                WW_CHOrow("HWORKTIMETTL") = HHMMtoMinutes(WW_CHOrow("HWORKTIME")) + HHMMtoMinutes(WW_CHOrow("HWORKTIMECHO"))
                WW_CHOrow("HNIGHTTIMETTL") = HHMMtoMinutes(WW_CHOrow("HNIGHTTIME")) + HHMMtoMinutes(WW_CHOrow("HNIGHTTIMECHO"))
                WW_CHOrow("SHOUKETUNISSUTTL") = Val(WW_CHOrow("SHOUKETUNISSU")) + Val(WW_CHOrow("SHOUKETUNISSUCHO"))
                WW_CHOrow("KUMIKETUNISSUTTL") = Val(WW_CHOrow("KUMIKETUNISSU")) + Val(WW_CHOrow("KUMIKETUNISSUCHO"))
                WW_CHOrow("ETCKETUNISSUTTL") = Val(WW_CHOrow("ETCKETUNISSU")) + Val(WW_CHOrow("ETCKETUNISSUCHO"))
                WW_CHOrow("NENKYUNISSUTTL") = Val(WW_CHOrow("NENKYUNISSU")) + Val(WW_CHOrow("NENKYUNISSUCHO"))
                WW_CHOrow("TOKUKYUNISSUTTL") = Val(WW_CHOrow("TOKUKYUNISSU")) + Val(WW_CHOrow("TOKUKYUNISSUCHO"))
                WW_CHOrow("CHIKOKSOTAINISSUTTL") = Val(WW_CHOrow("CHIKOKSOTAINISSU")) + Val(WW_CHOrow("CHIKOKSOTAINISSUCHO"))
                WW_CHOrow("STOCKNISSUTTL") = Val(WW_CHOrow("STOCKNISSU")) + Val(WW_CHOrow("STOCKNISSUCHO"))
                WW_CHOrow("KYOTEIWEEKNISSUTTL") = Val(WW_CHOrow("KYOTEIWEEKNISSU")) + Val(WW_CHOrow("KYOTEIWEEKNISSUCHO"))
                WW_CHOrow("WEEKNISSUTTL") = Val(WW_CHOrow("WEEKNISSU")) + Val(WW_CHOrow("WEEKNISSUCHO"))
                WW_CHOrow("DAIKYUNISSUTTL") = Val(WW_CHOrow("DAIKYUNISSU")) + Val(WW_CHOrow("DAIKYUNISSUCHO"))
                WW_CHOrow("NENSHINISSUTTL") = Val(WW_CHOrow("NENSHINISSU")) + Val(WW_CHOrow("NENSHINISSUCHO"))
                WW_CHOrow("SHUKCHOKNNISSUTTL") = Val(WW_CHOrow("SHUKCHOKNNISSU")) + Val(WW_CHOrow("SHUKCHOKNNISSUCHO"))
                WW_CHOrow("SHUKCHOKNISSUTTL") = Val(WW_CHOrow("SHUKCHOKNISSU")) + Val(WW_CHOrow("SHUKCHOKNISSUCHO"))
                '2018/02/08 追加
                If WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") AndAlso
                   WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") AndAlso
                   WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUCHO") Then
                    WW_CHOrow("SHUKCHOKNHLDNISSUTTL") = Val(WW_CHOrow("SHUKCHOKNHLDNISSU")) + Val(WW_CHOrow("SHUKCHOKNHLDNISSUCHO"))
                End If
                If WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") AndAlso
                   WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") AndAlso
                   WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSUCHO") Then
                    WW_CHOrow("SHUKCHOKHLDNISSUTTL") = Val(WW_CHOrow("SHUKCHOKHLDNISSU")) + Val(WW_CHOrow("SHUKCHOKHLDNISSUCHO"))
                End If
                '2018/02/08 追加
                WW_CHOrow("TOKSAAKAISUTTL") = Val(WW_CHOrow("TOKSAAKAISU")) + Val(WW_CHOrow("TOKSAAKAISUCHO"))
                WW_CHOrow("TOKSABKAISUTTL") = Val(WW_CHOrow("TOKSABKAISU")) + Val(WW_CHOrow("TOKSABKAISUCHO"))
                WW_CHOrow("TOKSACKAISUTTL") = Val(WW_CHOrow("TOKSACKAISU")) + Val(WW_CHOrow("TOKSACKAISUCHO"))
                '2018/04/18 追加
                If WW_CHOrow.Table.Columns.Contains("TENKOKAISUTTL") AndAlso
                   WW_CHOrow.Table.Columns.Contains("TENKOKAISU") AndAlso
                   WW_CHOrow.Table.Columns.Contains("TENKOKAISUCHO") Then
                    WW_CHOrow("TENKOKAISUTTL") = Val(WW_CHOrow("TENKOKAISU")) + Val(WW_CHOrow("TENKOKAISUCHO"))
                End If
                WW_CHOrow("HOANTIMETTL") = HHMMtoMinutes(WW_CHOrow("HOANTIME")) + HHMMtoMinutes(WW_CHOrow("HOANTIMECHO"))
                WW_CHOrow("KOATUTIMETTL") = HHMMtoMinutes(WW_CHOrow("KOATUTIME")) + HHMMtoMinutes(WW_CHOrow("KOATUTIMECHO"))
                WW_CHOrow("TOKUSA1TIMETTL") = HHMMtoMinutes(WW_CHOrow("TOKUSA1TIME")) + HHMMtoMinutes(WW_CHOrow("TOKUSA1TIMECHO"))
                WW_CHOrow("PONPNISSUTTL") = Val(WW_CHOrow("PONPNISSU")) + Val(WW_CHOrow("PONPNISSUCHO"))
                WW_CHOrow("BULKNISSUTTL") = Val(WW_CHOrow("BULKNISSU")) + Val(WW_CHOrow("BULKNISSUCHO"))
                WW_CHOrow("TRAILERNISSUTTL") = Val(WW_CHOrow("TRAILERNISSU")) + Val(WW_CHOrow("TRAILERNISSUCHO"))
                WW_CHOrow("BKINMUKAISUTTL") = Val(WW_CHOrow("BKINMUKAISU")) + Val(WW_CHOrow("BKINMUKAISUCHO"))
                WW_CHOrow("HAYADETIMETTL") = HHMMtoMinutes(WW_CHOrow("HAYADETIME")) + HHMMtoMinutes(WW_CHOrow("HAYADETIMECHO"))
                'NJS用
                WW_CHOrow("NENMATUNISSUTTL") = Val(WW_CHOrow("NENMATUNISSU")) + Val(WW_CHOrow("NENMATUNISSUCHO"))
                WW_CHOrow("SHACHUHAKNISSUTTL") = Val(WW_CHOrow("SHACHUHAKNISSU")) + Val(WW_CHOrow("SHACHUHAKNISSUCHO"))
                WW_CHOrow("HAISOTIME") = HHMMtoMinutes(WW_CHOrow("HAISOTIME"))
                WW_CHOrow("JIKYUSHATIMETTL") = HHMMtoMinutes(WW_CHOrow("JIKYUSHATIME")) + HHMMtoMinutes(WW_CHOrow("JIKYUSHATIMECHO"))
                WW_CHOrow("JIKYUSHATIMECHO") = HHMMtoMinutes(WW_CHOrow("JIKYUSHATIMECHO"))
                WW_CHOrow("MODELDISTANCETTL") = Val(WW_CHOrow("MODELDISTANCE")) + Val(WW_CHOrow("MODELDISTANCECHO"))
                '近石用
                WW_CHOrow("WWORKTIMETTL") = HHMMtoMinutes(WW_CHOrow("WWORKTIME")) + HHMMtoMinutes(WW_CHOrow("WWORKTIMECHO"))
                WW_CHOrow("WWORKTIME") = HHMMtoMinutes(WW_CHOrow("WWORKTIME"))
                WW_CHOrow("WWORKTIMECHO") = HHMMtoMinutes(WW_CHOrow("WWORKTIMECHO"))
                WW_CHOrow("JYOMUTIMETTL") = HHMMtoMinutes(WW_CHOrow("JYOMUTIME")) + HHMMtoMinutes(WW_CHOrow("JYOMUTIMECHO"))
                WW_CHOrow("JYOMUTIME") = HHMMtoMinutes(WW_CHOrow("JYOMUTIME"))
                WW_CHOrow("JYOMUTIMECHO") = HHMMtoMinutes(WW_CHOrow("JYOMUTIMECHO"))
                WW_CHOrow("SDAIWORKTIMETTL") = HHMMtoMinutes(WW_CHOrow("SDAIWORKTIME")) + HHMMtoMinutes(WW_CHOrow("SDAIWORKTIMECHO"))
                WW_CHOrow("SDAINIGHTTIMETTL") = HHMMtoMinutes(WW_CHOrow("SDAINIGHTTIME")) + HHMMtoMinutes(WW_CHOrow("SDAINIGHTTIMECHO"))
                WW_CHOrow("HDAIWORKTIMETTL") = HHMMtoMinutes(WW_CHOrow("HDAIWORKTIME")) + HHMMtoMinutes(WW_CHOrow("HDAIWORKTIMECHO"))
                WW_CHOrow("HDAINIGHTTIMETTL") = HHMMtoMinutes(WW_CHOrow("HDAINIGHTTIME")) + HHMMtoMinutes(WW_CHOrow("HDAINIGHTTIMECHO"))
                WW_CHOrow("HWORKNISSUTTL") = Val(WW_CHOrow("HWORKNISSU")) + Val(WW_CHOrow("HWORKNISSUCHO"))
                If WW_CHOrow("CAMPCODE") = "03" Then
                    WW_CHOrow("WORKNISSUTTL") = Val(WW_CHOrow("WORKNISSU")) + Val(WW_CHOrow("WORKNISSUCHO"))
                End If
                WW_CHOrow("KAITENCNTTTL") = Val(WW_CHOrow("KAITENCNT")) + Val(WW_CHOrow("KAITENCNTCHO"))

                WW_CHOrow("KAITENCNTTTL1_1") = Val(WW_CHOrow("KAITENCNT1_1")) + Val(WW_CHOrow("KAITENCNTCHO1_1"))
                WW_CHOrow("KAITENCNTTTL1_2") = Val(WW_CHOrow("KAITENCNT1_2")) + Val(WW_CHOrow("KAITENCNTCHO1_2"))
                WW_CHOrow("KAITENCNTTTL1_3") = Val(WW_CHOrow("KAITENCNT1_3")) + Val(WW_CHOrow("KAITENCNTCHO1_3"))
                WW_CHOrow("KAITENCNTTTL1_4") = Val(WW_CHOrow("KAITENCNT1_4")) + Val(WW_CHOrow("KAITENCNTCHO1_4"))
                WW_CHOrow("KAITENCNTTTL2_1") = Val(WW_CHOrow("KAITENCNT2_1")) + Val(WW_CHOrow("KAITENCNTCHO2_1"))
                WW_CHOrow("KAITENCNTTTL2_2") = Val(WW_CHOrow("KAITENCNT2_2")) + Val(WW_CHOrow("KAITENCNTCHO2_2"))
                WW_CHOrow("KAITENCNTTTL2_3") = Val(WW_CHOrow("KAITENCNT2_3")) + Val(WW_CHOrow("KAITENCNTCHO2_3"))
                WW_CHOrow("KAITENCNTTTL2_4") = Val(WW_CHOrow("KAITENCNT2_4")) + Val(WW_CHOrow("KAITENCNTCHO2_4"))

                'JKT用
                WW_CHOrow("SENJYOCNTTTL") = Val(WW_CHOrow("SENJYOCNT")) + Val(WW_CHOrow("SENJYOCNTCHO"))
                WW_CHOrow("UNLOADADDCNT1TTL") = Val(WW_CHOrow("UNLOADADDCNT1")) + Val(WW_CHOrow("UNLOADADDCNT1CHO"))
                WW_CHOrow("UNLOADADDCNT2TTL") = Val(WW_CHOrow("UNLOADADDCNT2")) + Val(WW_CHOrow("UNLOADADDCNT2CHO"))
                WW_CHOrow("UNLOADADDCNT3TTL") = Val(WW_CHOrow("UNLOADADDCNT3")) + Val(WW_CHOrow("UNLOADADDCNT3CHO"))
                WW_CHOrow("UNLOADADDCNT4TTL") = Val(WW_CHOrow("UNLOADADDCNT4")) + Val(WW_CHOrow("UNLOADADDCNT4CHO"))
                WW_CHOrow("LOADINGCNT1TTL") = Val(WW_CHOrow("LOADINGCNT1")) + Val(WW_CHOrow("LOADINGCNT1CHO"))
                WW_CHOrow("LOADINGCNT2TTL") = Val(WW_CHOrow("LOADINGCNT2")) + Val(WW_CHOrow("LOADINGCNT2CHO"))
                WW_CHOrow("SHORTDISTANCE1TTL") = Val(WW_CHOrow("SHORTDISTANCE1")) + Val(WW_CHOrow("SHORTDISTANCE1CHO"))
                WW_CHOrow("SHORTDISTANCE2TTL") = Val(WW_CHOrow("SHORTDISTANCE2")) + Val(WW_CHOrow("SHORTDISTANCE2CHO"))

                WW_CHOrow("BINDTIME") = HHMMtoMinutes(WW_CHOrow("BINDTIME"))
                WW_CHOrow("BREAKTIME") = HHMMtoMinutes(WW_CHOrow("BREAKTIME"))
                WW_CHOrow("NIGHTTIME") = HHMMtoMinutes(WW_CHOrow("NIGHTTIME"))
                WW_CHOrow("ORVERTIME") = HHMMtoMinutes(WW_CHOrow("ORVERTIME"))
                WW_CHOrow("WNIGHTTIME") = HHMMtoMinutes(WW_CHOrow("WNIGHTTIME"))
                WW_CHOrow("SWORKTIME") = HHMMtoMinutes(WW_CHOrow("SWORKTIME"))
                WW_CHOrow("SNIGHTTIME") = HHMMtoMinutes(WW_CHOrow("SNIGHTTIME"))
                WW_CHOrow("HWORKTIME") = HHMMtoMinutes(WW_CHOrow("HWORKTIME"))
                WW_CHOrow("HNIGHTTIME") = HHMMtoMinutes(WW_CHOrow("HNIGHTTIME"))
                WW_CHOrow("HOANTIME") = HHMMtoMinutes(WW_CHOrow("HOANTIME"))
                WW_CHOrow("KOATUTIME") = HHMMtoMinutes(WW_CHOrow("KOATUTIME"))
                WW_CHOrow("TOKUSA1TIME") = HHMMtoMinutes(WW_CHOrow("TOKUSA1TIME"))
                WW_CHOrow("HAYADETIME") = HHMMtoMinutes(WW_CHOrow("HAYADETIME"))
                WW_CHOrow("BREAKTIMECHO") = HHMMtoMinutes(WW_CHOrow("BREAKTIMECHO"))
                WW_CHOrow("NIGHTTIMECHO") = HHMMtoMinutes(WW_CHOrow("NIGHTTIMECHO"))
                WW_CHOrow("ORVERTIMECHO") = HHMMtoMinutes(WW_CHOrow("ORVERTIMECHO"))
                If WW_CHOrow.Table.Columns.Contains("ORVERTIMEADD") Then
                    WW_CHOrow("ORVERTIMEADD") = HHMMtoMinutes(WW_CHOrow("ORVERTIMEADD"))
                End If
                WW_CHOrow("WNIGHTTIMECHO") = HHMMtoMinutes(WW_CHOrow("WNIGHTTIMECHO"))
                If WW_CHOrow.Table.Columns.Contains("WNIGHTTIMEADD") Then
                    WW_CHOrow("WNIGHTTIMEADD") = HHMMtoMinutes(WW_CHOrow("WNIGHTTIMEADD"))
                End If
                WW_CHOrow("SWORKTIMECHO") = HHMMtoMinutes(WW_CHOrow("SWORKTIMECHO"))
                If WW_CHOrow.Table.Columns.Contains("SWORKTIMEADD") Then
                    WW_CHOrow("SWORKTIMEADD") = HHMMtoMinutes(WW_CHOrow("SWORKTIMEADD"))
                End If
                WW_CHOrow("SNIGHTTIMECHO") = HHMMtoMinutes(WW_CHOrow("SNIGHTTIMECHO"))
                If WW_CHOrow.Table.Columns.Contains("SNIGHTTIMEADD") Then
                    WW_CHOrow("SNIGHTTIMEADD") = HHMMtoMinutes(WW_CHOrow("SNIGHTTIMEADD"))
                End If
                WW_CHOrow("HWORKTIMECHO") = HHMMtoMinutes(WW_CHOrow("HWORKTIMECHO"))
                WW_CHOrow("HNIGHTTIMECHO") = HHMMtoMinutes(WW_CHOrow("HNIGHTTIMECHO"))
                WW_CHOrow("HOANTIMECHO") = HHMMtoMinutes(WW_CHOrow("HOANTIMECHO"))
                WW_CHOrow("KOATUTIMECHO") = HHMMtoMinutes(WW_CHOrow("KOATUTIMECHO"))
                WW_CHOrow("TOKUSA1TIMECHO") = HHMMtoMinutes(WW_CHOrow("TOKUSA1TIMECHO"))
                WW_CHOrow("HAYADETIMECHO") = HHMMtoMinutes(WW_CHOrow("HAYADETIMECHO"))

                WW_CHOrow("LINECNT") = "0"
                WW_CHOrow("SELECT") = "1" '0:対象外、1:対象
                WW_CHOrow("HIDDEN") = "0" '0:表示、1:非表示
                WW_CHOrow("TIMSTP") = "0"
                WW_CHOrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                WW_CHOrow("HDKBN") = "H"
                WW_CHOrow("RECODEKBN") = "1"
                WW_CHOrow("WORKINGWEEK") = ""
                WW_CHOrow("WORKINGWEEKNAMES") = ""
                WW_CHOrow("RECODEKBNNAMES") = "月調整"
                WW_CHOrow("STATUS") = "月調整"
                WW_CHOrow("HOLIDAYKBN") = ""
                WW_CHOrow("HOLIDAYKBNNAMES") = ""
                WW_CHOrow("PAYKBN") = ""
                WW_CHOrow("PAYKBNNAMES") = ""
                WW_CHOrow("STTIME") = ""
                WW_CHOrow("ENDTIME") = ""
                WW_CHOrow("WORKTIME") = ""
                WW_CHOrow("MOVETIME") = ""
                WW_CHOrow("ACTTIME") = ""
                WW_CHOrow("DELFLG") = C_DELETE_FLG.ALIVE
                '時間項目変換（分→時間（HH:MM））
                TimeItemFormat(WW_CHOrow)

                WW_T0007CHOtbl.Rows.Add(WW_CHOrow)
            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007TTLtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007ETCtbl)

            '月調整のマージ
            ioTbl.Merge(WW_T0007CHOtbl)

            WW_T0007TTLtbl.Dispose()
            WW_T0007TTLtbl = Nothing
            WW_T0007ETCtbl.Dispose()
            WW_T0007ETCtbl = Nothing
            WW_T0007CHOtbl.Dispose()
            WW_T0007CHOtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_CreHead"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  月合計レコードの作成
    Public Sub T0007_TotalRecodeCreate(ByRef ioTbl As DataTable)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Dim WW_IDX As Integer = 0

        Try
            Dim WW_T0007tbl As DataTable = ioTbl.Clone

            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, SELECT, RECODEKBN"
            ioTbl = CS0026TblSort.sort()

            Dim WW_T0007DELtbl As DataTable = ioTbl.Clone
            Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
            Dim WW_T0007DTLtbl As DataTable = ioTbl.Clone
            For i As Integer = 0 To ioTbl.Rows.Count - 1
                Dim ioTblrow As DataRow = ioTbl.Rows(i)

                '削除レコードを取得
                If ioTblrow("SELECT") = "0" Then
                    Dim DELrow As DataRow = WW_T0007DELtbl.NewRow
                    DELrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007DELtbl.Rows.Add(DELrow)
                End If

                '勤怠のヘッダレコードを取得（月調整を再作成するため捨てる）
                If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "H" And ioTblrow("RECODEKBN") <> "1" Then
                    Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                    HEADrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007HEADtbl.Rows.Add(HEADrow)
                End If

                '勤怠の明細レコードを取得
                If ioTblrow("SELECT") = "1" And ioTblrow("HDKBN") = "D" Then
                    Dim DTLrow As DataRow = WW_T0007DTLtbl.NewRow
                    DTLrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007DTLtbl.Rows.Add(DTLrow)
                End If
            Next



            Dim iT0007DTLview As DataView
            iT0007DTLview = New DataView(WW_T0007DTLtbl)
            iT0007DTLview.Sort = "RECODEKBN, STAFFCODE"

            WW_IDX = 0
            For Each WW_CHOrow As DataRow In WW_T0007HEADtbl.Rows
                If WW_CHOrow("RECODEKBN") <> "2" Then
                    Continue For
                End If

                '集計項目初期クリア
                SumItem_Init(WW_CHOrow)

                For i As Integer = WW_IDX To WW_T0007HEADtbl.Rows.Count - 1
                    Dim WW_HEADrow As DataRow = WW_T0007HEADtbl.Rows(i)
                    If WW_HEADrow("STAFFCODE") = WW_CHOrow("STAFFCODE") Then
                        If WW_HEADrow("RECODEKBN") = "0" Then
                            '合計（日別のレコードを加算）
                            WW_CHOrow("WORKTIME") = Val(WW_CHOrow("WORKTIME")) + HHMMtoMinutes(WW_HEADrow("WORKTIME"))
                            WW_CHOrow("MOVETIME") = Val(WW_CHOrow("MOVETIME")) + HHMMtoMinutes(WW_HEADrow("MOVETIME"))
                            WW_CHOrow("ACTTIME") = Val(WW_CHOrow("ACTTIME")) + HHMMtoMinutes(WW_HEADrow("ACTTIME"))
                            WW_CHOrow("HAIDISTANCE") = Val(WW_CHOrow("HAIDISTANCE")) + WW_HEADrow("HAIDISTANCE")
                            WW_CHOrow("HAIDISTANCECHO") = 0
                            WW_CHOrow("KAIDISTANCE") = Val(WW_CHOrow("KAIDISTANCE")) + WW_HEADrow("KAIDISTANCE")
                            WW_CHOrow("KAIDISTANCECHO") = 0
                            WW_CHOrow("UNLOADCNT") = Val(WW_CHOrow("UNLOADCNT")) + WW_HEADrow("UNLOADCNT")
                            WW_CHOrow("UNLOADCNTCHO") = 0
                            If HHMMtoMinutes(WW_HEADrow("ACTTIME")) > 0 Then
                                WW_CHOrow("BINDTIME") = Val(WW_CHOrow("BINDTIME")) + HHMMtoMinutes(WW_HEADrow("BINDTIME"))
                            Else
                                WW_CHOrow("BINDTIME") = Val(WW_CHOrow("BINDTIME")) + 0
                            End If
                            WW_CHOrow("NIPPOBREAKTIME") = Val(WW_CHOrow("NIPPOBREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                            WW_CHOrow("BREAKTIME") = Val(WW_CHOrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                            WW_CHOrow("BREAKTIMECHO") = 0
                            WW_CHOrow("NIGHTTIME") = Val(WW_CHOrow("NIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("NIGHTTIME"))
                            WW_CHOrow("NIGHTTIMECHO") = 0
                            WW_CHOrow("ORVERTIME") = Val(WW_CHOrow("ORVERTIME")) + HHMMtoMinutes(WW_HEADrow("ORVERTIME"))
                            WW_CHOrow("ORVERTIMECHO") = 0
                            If WW_HEADrow.Table.Columns.Contains("ORVERTIMEADD") Then
                                WW_CHOrow("ORVERTIMEADD") = Val(WW_CHOrow("ORVERTIMEADD")) + HHMMtoMinutes(WW_HEADrow("ORVERTIMEADD"))
                            End If
                            WW_CHOrow("WNIGHTTIME") = Val(WW_CHOrow("WNIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("WNIGHTTIME"))
                            WW_CHOrow("WNIGHTTIMECHO") = 0
                            If WW_HEADrow.Table.Columns.Contains("WNIGHTTIMEADD") Then
                                WW_CHOrow("WNIGHTTIMEADD") = Val(WW_CHOrow("WNIGHTTIMEADD")) + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMEADD"))
                            End If
                            WW_CHOrow("SWORKTIME") = Val(WW_CHOrow("SWORKTIME")) + HHMMtoMinutes(WW_HEADrow("SWORKTIME"))
                            WW_CHOrow("SWORKTIMECHO") = 0
                            If WW_HEADrow.Table.Columns.Contains("SWORKTIMEADD") Then
                                WW_CHOrow("SWORKTIMEADD") = Val(WW_CHOrow("SWORKTIMEADD")) + HHMMtoMinutes(WW_HEADrow("SWORKTIMEADD"))
                            End If
                            WW_CHOrow("SNIGHTTIME") = Val(WW_CHOrow("SNIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("SNIGHTTIME"))
                            WW_CHOrow("SNIGHTTIMECHO") = 0
                            If WW_HEADrow.Table.Columns.Contains("SNIGHTTIMEADD") Then
                                WW_CHOrow("SNIGHTTIMEADD") = Val(WW_CHOrow("SNIGHTTIMEADD")) + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMEADD"))
                            End If
                            WW_CHOrow("HWORKTIME") = Val(WW_CHOrow("HWORKTIME")) + HHMMtoMinutes(WW_HEADrow("HWORKTIME"))
                            WW_CHOrow("HWORKTIMECHO") = 0
                            WW_CHOrow("HNIGHTTIME") = Val(WW_CHOrow("HNIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("HNIGHTTIME"))
                            WW_CHOrow("HNIGHTTIMECHO") = 0
                            WW_CHOrow("SHOUKETUNISSU") = Val(WW_CHOrow("SHOUKETUNISSU")) + WW_HEADrow("SHOUKETUNISSU")
                            WW_CHOrow("SHOUKETUNISSUCHO") = 0
                            WW_CHOrow("KUMIKETUNISSU") = Val(WW_CHOrow("KUMIKETUNISSU")) + WW_HEADrow("KUMIKETUNISSU")
                            WW_CHOrow("KUMIKETUNISSUCHO") = 0
                            WW_CHOrow("ETCKETUNISSU") = Val(WW_CHOrow("ETCKETUNISSU")) + WW_HEADrow("ETCKETUNISSU")
                            WW_CHOrow("ETCKETUNISSUCHO") = 0
                            WW_CHOrow("NENKYUNISSU") = Val(WW_CHOrow("NENKYUNISSU")) + WW_HEADrow("NENKYUNISSU")
                            WW_CHOrow("NENKYUNISSUCHO") = 0
                            WW_CHOrow("TOKUKYUNISSU") = Val(WW_CHOrow("TOKUKYUNISSU")) + WW_HEADrow("TOKUKYUNISSU")
                            WW_CHOrow("TOKUKYUNISSUCHO") = 0
                            WW_CHOrow("CHIKOKSOTAINISSU") = Val(WW_CHOrow("CHIKOKSOTAINISSU")) + WW_HEADrow("CHIKOKSOTAINISSU")
                            WW_CHOrow("CHIKOKSOTAINISSUCHO") = 0
                            WW_CHOrow("STOCKNISSU") = Val(WW_CHOrow("STOCKNISSU")) + WW_HEADrow("STOCKNISSU")
                            WW_CHOrow("STOCKNISSUCHO") = 0
                            WW_CHOrow("KYOTEIWEEKNISSU") = Val(WW_CHOrow("KYOTEIWEEKNISSU")) + WW_HEADrow("KYOTEIWEEKNISSU")
                            WW_CHOrow("KYOTEIWEEKNISSUCHO") = 0
                            WW_CHOrow("WEEKNISSU") = Val(WW_CHOrow("WEEKNISSU")) + WW_HEADrow("WEEKNISSU")
                            WW_CHOrow("WEEKNISSUCHO") = 0
                            WW_CHOrow("DAIKYUNISSU") = Val(WW_CHOrow("DAIKYUNISSU")) + WW_HEADrow("DAIKYUNISSU")
                            WW_CHOrow("DAIKYUNISSUCHO") = 0
                            WW_CHOrow("NENSHINISSU") = Val(WW_CHOrow("NENSHINISSU")) + WW_HEADrow("NENSHINISSU")
                            WW_CHOrow("NENSHINISSUCHO") = 0
                            WW_CHOrow("SHUKCHOKNNISSU") = Val(WW_CHOrow("SHUKCHOKNNISSU")) + WW_HEADrow("SHUKCHOKNNISSU")
                            WW_CHOrow("SHUKCHOKNNISSUCHO") = 0
                            WW_CHOrow("SHUKCHOKNISSU") = Val(WW_CHOrow("SHUKCHOKNISSU")) + WW_HEADrow("SHUKCHOKNISSU")
                            WW_CHOrow("SHUKCHOKNISSUCHO") = 0

                            '2018/02/08 追加
                            If WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") And
                               WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUCHO") Then
                                WW_CHOrow("SHUKCHOKNHLDNISSU") = Val(WW_CHOrow("SHUKCHOKNHLDNISSU")) + WW_HEADrow("SHUKCHOKNHLDNISSU")
                                WW_CHOrow("SHUKCHOKNHLDNISSUCHO") = 0
                            End If
                            If WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") And
                               WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSUCHO") Then
                                WW_CHOrow("SHUKCHOKHLDNISSU") = Val(WW_CHOrow("SHUKCHOKHLDNISSU")) + WW_HEADrow("SHUKCHOKHLDNISSU")
                                WW_CHOrow("SHUKCHOKHLDNISSUCHO") = 0
                            End If
                            '2018/02/08 追加

                            WW_CHOrow("TOKSAAKAISU") = Val(WW_CHOrow("TOKSAAKAISU")) + WW_HEADrow("TOKSAAKAISU")
                            WW_CHOrow("TOKSAAKAISUCHO") = 0
                            WW_CHOrow("TOKSABKAISU") = Val(WW_CHOrow("TOKSABKAISU")) + WW_HEADrow("TOKSABKAISU")
                            WW_CHOrow("TOKSABKAISUCHO") = 0
                            WW_CHOrow("TOKSACKAISU") = Val(WW_CHOrow("TOKSACKAISU")) + WW_HEADrow("TOKSACKAISU")
                            WW_CHOrow("TOKSACKAISUCHO") = 0
                            '2018/04/17 追加
                            If WW_CHOrow.Table.Columns.Contains("TENKOKAISU") And
                               WW_CHOrow.Table.Columns.Contains("TENKOKAISUCHO") Then
                                WW_CHOrow("TENKOKAISU") = Val(WW_CHOrow("TENKOKAISU")) + WW_HEADrow("TENKOKAISU")
                                WW_CHOrow("TENKOKAISUCHO") = 0
                            End If
                            WW_CHOrow("HOANTIME") = Val(WW_CHOrow("HOANTIME")) + HHMMtoMinutes(WW_HEADrow("HOANTIME"))
                            WW_CHOrow("HOANTIMECHO") = 0
                            WW_CHOrow("KOATUTIME") = Val(WW_CHOrow("KOATUTIME")) + HHMMtoMinutes(WW_HEADrow("KOATUTIME"))
                            WW_CHOrow("KOATUTIMECHO") = 0
                            WW_CHOrow("TOKUSA1TIME") = Val(WW_CHOrow("TOKUSA1TIME")) + HHMMtoMinutes(WW_HEADrow("TOKUSA1TIME"))
                            WW_CHOrow("TOKUSA1TIMECHO") = 0
                            WW_CHOrow("PONPNISSU") = Val(WW_CHOrow("PONPNISSU")) + WW_HEADrow("PONPNISSU")
                            WW_CHOrow("PONPNISSUCHO") = 0
                            WW_CHOrow("BULKNISSU") = Val(WW_CHOrow("BULKNISSU")) + WW_HEADrow("BULKNISSU")
                            WW_CHOrow("BULKNISSUCHO") = 0
                            WW_CHOrow("TRAILERNISSU") = Val(WW_CHOrow("TRAILERNISSU")) + WW_HEADrow("TRAILERNISSU")
                            WW_CHOrow("TRAILERNISSUCHO") = 0
                            WW_CHOrow("BKINMUKAISU") = Val(WW_CHOrow("BKINMUKAISU")) + WW_HEADrow("BKINMUKAISU")
                            WW_CHOrow("BKINMUKAISUCHO") = 0
                            WW_CHOrow("HAYADETIME") = Val(WW_CHOrow("HAYADETIME")) + HHMMtoMinutes(WW_HEADrow("HAYADETIME"))
                            WW_CHOrow("HAYADETIMECHO") = 0
                            'NJS
                            WW_CHOrow("NENMATUNISSU") = Val(WW_CHOrow("NENMATUNISSU")) + WW_HEADrow("NENMATUNISSU")
                            WW_CHOrow("NENMATUNISSUCHO") = 0
                            WW_CHOrow("SHACHUHAKNISSU") = Val(WW_CHOrow("SHACHUHAKNISSU")) + WW_HEADrow("SHACHUHAKNISSU")
                            WW_CHOrow("SHACHUHAKNISSUCHO") = 0
                            WW_CHOrow("HAISOTIME") = Val(WW_CHOrow("HAISOTIME")) + HHMMtoMinutes(WW_HEADrow("HAISOTIME"))

                            WW_CHOrow("JIKYUSHATIME") = Val(WW_CHOrow("JIKYUSHATIME")) + HHMMtoMinutes(WW_HEADrow("JIKYUSHATIME"))
                            WW_CHOrow("JIKYUSHATIMECHO") = 0
                            For j As Integer = 1 To 6
                                Dim WW_MODELDISTANCE As String = "T10MODELDISTANCE" & j.ToString
                                WW_CHOrow("MODELDISTANCE") = Val(WW_CHOrow("MODELDISTANCE")) + Val(WW_HEADrow(WW_MODELDISTANCE))
                            Next
                            WW_CHOrow("MODELDISTANCECHO") = 0
                            '近石
                            WW_CHOrow("HDAIWORKTIME") = Val(WW_CHOrow("HDAIWORKTIME")) + HHMMtoMinutes(WW_HEADrow("HDAIWORKTIME"))
                            WW_CHOrow("HDAIWORKTIMECHO") = 0
                            WW_CHOrow("HDAINIGHTTIME") = Val(WW_CHOrow("HDAINIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("HDAINIGHTTIME"))
                            WW_CHOrow("HDAINIGHTTIMECHO") = 0
                            WW_CHOrow("SDAIWORKTIME") = Val(WW_CHOrow("SDAIWORKTIME")) + HHMMtoMinutes(WW_HEADrow("SDAIWORKTIME"))
                            WW_CHOrow("SDAIWORKTIMECHO") = 0
                            WW_CHOrow("SDAINIGHTTIME") = Val(WW_CHOrow("SDAINIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("SDAINIGHTTIME"))
                            WW_CHOrow("SDAINIGHTTIMECHO") = 0

                            WW_CHOrow("WWORKTIME") = Val(WW_CHOrow("WWORKTIME")) + HHMMtoMinutes(WW_HEADrow("WWORKTIME"))
                            WW_CHOrow("WWORKTIMECHO") = 0
                            WW_CHOrow("JYOMUTIME") = Val(WW_CHOrow("JYOMUTIME")) + HHMMtoMinutes(WW_HEADrow("JYOMUTIME"))
                            WW_CHOrow("JYOMUTIMECHO") = 0
                            WW_CHOrow("HWORKNISSU") = Val(WW_CHOrow("HWORKNISSU")) + WW_HEADrow("HWORKNISSU")
                            WW_CHOrow("HWORKNISSUCHO") = 0
                            If WW_CHOrow("CAMPCODE") = "03" Then
                                WW_CHOrow("WORKNISSU") = Val(WW_CHOrow("WORKNISSU")) + WW_HEADrow("WORKNISSU")
                                WW_CHOrow("WORKNISSUCHO") = 0
                            End If
                            WW_CHOrow("KAITENCNT") = Val(WW_CHOrow("KAITENCNT")) + WW_HEADrow("KAITENCNT")
                            WW_CHOrow("KAITENCNTCHO") = 0
                            WW_CHOrow("KAITENCNT1_1") = Val(WW_CHOrow("KAITENCNT1_1")) + Val(WW_HEADrow("KAITENCNT1_1"))
                            WW_CHOrow("KAITENCNTCHO1_1") = 0
                            WW_CHOrow("KAITENCNT1_2") = Val(WW_CHOrow("KAITENCNT1_2")) + Val(WW_HEADrow("KAITENCNT1_2"))
                            WW_CHOrow("KAITENCNTCHO1_2") = 0
                            WW_CHOrow("KAITENCNT1_3") = Val(WW_CHOrow("KAITENCNT1_3")) + Val(WW_HEADrow("KAITENCNT1_3"))
                            WW_CHOrow("KAITENCNTCHO1_3") = 0
                            WW_CHOrow("KAITENCNT1_4") = Val(WW_CHOrow("KAITENCNT1_4")) + Val(WW_HEADrow("KAITENCNT1_4"))
                            WW_CHOrow("KAITENCNTCHO1_4") = 0
                            WW_CHOrow("KAITENCNT2_1") = Val(WW_CHOrow("KAITENCNT2_1")) + Val(WW_HEADrow("KAITENCNT2_1"))
                            WW_CHOrow("KAITENCNTCHO2_1") = 0
                            WW_CHOrow("KAITENCNT2_2") = Val(WW_CHOrow("KAITENCNT2_2")) + Val(WW_HEADrow("KAITENCNT2_2"))
                            WW_CHOrow("KAITENCNTCHO2_2") = 0
                            WW_CHOrow("KAITENCNT2_3") = Val(WW_CHOrow("KAITENCNT2_3")) + Val(WW_HEADrow("KAITENCNT2_3"))
                            WW_CHOrow("KAITENCNTCHO2_3") = 0
                            WW_CHOrow("KAITENCNT2_4") = Val(WW_CHOrow("KAITENCNT2_4")) + Val(WW_HEADrow("KAITENCNT2_4"))
                            WW_CHOrow("KAITENCNTCHO2_4") = 0

                            'ＪＫＴ
                            WW_CHOrow("SENJYOCNT") = Val(WW_CHOrow("SENJYOCNT")) + Val(WW_HEADrow("SENJYOCNT"))
                            WW_CHOrow("SENJYOCNTCHO") = 0
                            WW_CHOrow("UNLOADADDCNT1") = Val(WW_CHOrow("UNLOADADDCNT1")) + Val(WW_HEADrow("UNLOADADDCNT1"))
                            WW_CHOrow("UNLOADADDCNT1CHO") = 0
                            WW_CHOrow("UNLOADADDCNT2") = Val(WW_CHOrow("UNLOADADDCNT2")) + Val(WW_HEADrow("UNLOADADDCNT2"))
                            WW_CHOrow("UNLOADADDCNT2CHO") = 0
                            WW_CHOrow("UNLOADADDCNT3") = Val(WW_CHOrow("UNLOADADDCNT3")) + Val(WW_HEADrow("UNLOADADDCNT3"))
                            WW_CHOrow("UNLOADADDCNT3CHO") = 0
                            WW_CHOrow("UNLOADADDCNT4") = Val(WW_CHOrow("UNLOADADDCNT4")) + Val(WW_HEADrow("UNLOADADDCNT4"))
                            WW_CHOrow("UNLOADADDCNT4CHO") = 0
                            WW_CHOrow("LOADINGCNT1") = Val(WW_CHOrow("LOADINGCNT1")) + Val(WW_HEADrow("LOADINGCNT1"))
                            WW_CHOrow("LOADINGCNT1CHO") = 0
                            WW_CHOrow("LOADINGCNT2") = Val(WW_CHOrow("LOADINGCNT2")) + Val(WW_HEADrow("LOADINGCNT2"))
                            WW_CHOrow("LOADINGCNT2CHO") = 0
                            WW_CHOrow("SHORTDISTANCE1") = Val(WW_CHOrow("SHORTDISTANCE1")) + Val(WW_HEADrow("SHORTDISTANCE1"))
                            WW_CHOrow("SHORTDISTANCE1CHO") = 0
                            WW_CHOrow("SHORTDISTANCE2") = Val(WW_CHOrow("SHORTDISTANCE2")) + Val(WW_HEADrow("SHORTDISTANCE2"))
                            WW_CHOrow("SHORTDISTANCE2CHO") = 0

                        End If
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                '該当する合計明細レコード抽出
                iT0007DTLview.RowFilter = "RECODEKBN = '2' and STAFFCODE ='" & WW_CHOrow("STAFFCODE") & "'"
                Dim WW_WKDTLtbl As DataTable = iT0007DTLview.ToTable()
                For i As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                    Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(i)
                    WW_WKDTLrow("UNLOADCNT") = 0
                    WW_WKDTLrow("UNLOADCNTCHO") = 0
                    WW_WKDTLrow("UNLOADCNTTTL") = 0
                    WW_WKDTLrow("HAIDISTANCE") = 0
                    WW_WKDTLrow("HAIDISTANCECHO") = 0
                    WW_WKDTLrow("HAIDISTANCETTL") = 0
                    WW_WKDTLrow("MODELDISTANCE") = 0
                    WW_WKDTLrow("MODELDISTANCECHO") = 0
                    WW_WKDTLrow("MODELDISTANCETTL") = 0

                    Select Case WW_WKDTLrow("OILPAYKBN")
                        Case "01"  '一般
                            If WW_WKDTLrow("SHARYOKBN") = "1" Then
                                WW_WKDTLrow("KAITENCNT1_1") = WW_CHOrow("KAITENCNT1_1")
                                WW_WKDTLrow("KAITENCNTTTL1_1") = WW_CHOrow("KAITENCNT1_1")
                            End If
                            If WW_WKDTLrow("SHARYOKBN") = "2" Then
                                WW_WKDTLrow("KAITENCNT2_1") = WW_CHOrow("KAITENCNT2_1")
                                WW_WKDTLrow("KAITENCNTTTL2_1") = WW_CHOrow("KAITENCNT2_1")
                            End If
                        Case "02"  '潤滑油
                            If WW_WKDTLrow("SHARYOKBN") = "1" Then
                                WW_WKDTLrow("KAITENCNT1_2") = WW_CHOrow("KAITENCNT1_2")
                                WW_WKDTLrow("KAITENCNTTTL1_2") = WW_CHOrow("KAITENCNT1_2")
                            End If
                            If WW_WKDTLrow("SHARYOKBN") = "2" Then
                                WW_WKDTLrow("KAITENCNT2_2") = WW_CHOrow("KAITENCNT2_2")
                                WW_WKDTLrow("KAITENCNTTTL2_2") = WW_CHOrow("KAITENCNT2_2")
                            End If
                        Case "03"  'ＬＰ等
                            If WW_WKDTLrow("SHARYOKBN") = "1" Then
                                WW_WKDTLrow("KAITENCNT1_3") = WW_CHOrow("KAITENCNT1_3")
                                WW_WKDTLrow("KAITENCNTTTL1_3") = WW_CHOrow("KAITENCNT1_3")
                            End If
                            If WW_WKDTLrow("SHARYOKBN") = "2" Then
                                WW_WKDTLrow("KAITENCNT2_3") = WW_CHOrow("KAITENCNT2_3")
                                WW_WKDTLrow("KAITENCNTTTL2_3") = WW_CHOrow("KAITENCNT2_3")
                            End If
                        Case "04"  'ＬＮＧ
                            If WW_WKDTLrow("SHARYOKBN") = "1" Then
                                WW_WKDTLrow("KAITENCNT1_4") = WW_CHOrow("KAITENCNT1_4")
                                WW_WKDTLrow("KAITENCNTTTL1_4") = WW_CHOrow("KAITENCNT1_4")
                            End If
                            If WW_WKDTLrow("SHARYOKBN") = "2" Then
                                WW_WKDTLrow("KAITENCNT2_4") = WW_CHOrow("KAITENCNT2_4")
                                WW_WKDTLrow("KAITENCNTTTL2_4") = WW_CHOrow("KAITENCNT2_4")
                            End If
                        Case "05"  'コンテナ
                        Case "06"  '酸素
                        Case "07"  '窒素・ｱﾙｺﾞﾝ
                        Case "08"  'メタノール
                        Case "09"  'ラテックス
                        Case "10" '水素

                    End Select
                Next

                Dim iT0007view As DataView
                iT0007view = New DataView(WW_T0007DTLtbl)
                iT0007view.Sort = "WORKDATE, STAFFCODE, HDKBN, RECODEKBN "
                iT0007view.RowFilter = "HDKBN = 'D' and RECODEKBN ='0' and DATAKBN = 'N' and STAFFCODE ='" & WW_CHOrow("STAFFCODE") & "'"
                Dim wT0007tbl As DataTable = iT0007view.ToTable

                Dim WW_OILPAYKBN As String = ""
                Dim WW_SHARYOKBN As String = ""
                For Each WW_DTLrow As DataRow In wT0007tbl.Rows
                    WW_OILPAYKBN = WW_DTLrow("OILPAYKBN2")
                    WW_SHARYOKBN = WW_DTLrow("SHARYOKBN2")

                    If WW_DTLrow("WORKKBN") = "B3" AndAlso WW_OILPAYKBN <> "10" Then
                        For j As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                            Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(j)
                            If WW_WKDTLrow("STAFFCODE") = WW_DTLrow("STAFFCODE") AndAlso
                               WW_WKDTLrow("SHARYOKBN") = WW_DTLrow("SHARYOKBN") AndAlso
                               WW_WKDTLrow("OILPAYKBN") = WW_DTLrow("OILPAYKBN") Then
                                WW_WKDTLrow("UNLOADCNT") = Val(WW_WKDTLrow("UNLOADCNT")) + 1
                                WW_WKDTLrow("UNLOADCNTTTL") = Val(WW_WKDTLrow("UNLOADCNTTTL")) + 1
                            End If
                        Next
                    End If

                    If WW_DTLrow("WORKKBN") = "F3" Then
                        '明細（車両区分、油種毎）に配送キロを設定
                        If IsDBNull(WW_DTLrow("L1KAISO")) Then
                            WW_DTLrow("L1KAISO") = ""                     '念のため!
                        End If
                        If WW_DTLrow("L1KAISO") <> "回送" OrElse WW_OILPAYKBN = "10" Then
                            For j As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                                Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(j)
                                If WW_WKDTLrow("STAFFCODE") = WW_DTLrow("STAFFCODE") AndAlso
                                   WW_WKDTLrow("SHARYOKBN") = WW_SHARYOKBN AndAlso
                                   WW_WKDTLrow("OILPAYKBN") = WW_OILPAYKBN Then
                                    WW_WKDTLrow("HAIDISTANCE") = Val(WW_WKDTLrow("HAIDISTANCE")) + WW_DTLrow("HAIDISTANCE")
                                    WW_WKDTLrow("HAIDISTANCETTL") = Val(WW_WKDTLrow("HAIDISTANCETTL")) + WW_DTLrow("HAIDISTANCE")
                                End If
                            Next
                        End If

                        WW_OILPAYKBN = ""
                        WW_SHARYOKBN = ""
                    End If

                Next

                For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
                    If WW_HEADrow("RECODEKBN") = "0" Then

                        For j As Integer = 0 To WW_WKDTLtbl.Rows.Count - 1
                            Dim WW_WKDTLrow As DataRow = WW_WKDTLtbl.Rows(j)
                            For k As Integer = 1 To 6
                                Dim WW_T10SHARYOKBN As String = "T10SHARYOKBN" & k.ToString
                                Dim WW_T10OILPAYKBN As String = "T10OILPAYKBN" & k.ToString
                                Dim WW_T10MODELDISTANCE As String = "T10MODELDISTANCE" & k.ToString
                                If WW_WKDTLrow("STAFFCODE") = WW_HEADrow("STAFFCODE") AndAlso
                                   WW_WKDTLrow("SHARYOKBN") = WW_HEADrow(WW_T10SHARYOKBN) AndAlso
                                   WW_WKDTLrow("OILPAYKBN") = WW_HEADrow(WW_T10OILPAYKBN) Then
                                    WW_WKDTLrow("MODELDISTANCE") = Val(WW_WKDTLrow("MODELDISTANCE")) + Val(WW_HEADrow(WW_T10MODELDISTANCE))
                                    WW_WKDTLrow("MODELDISTANCETTL") = Val(WW_WKDTLrow("MODELDISTANCETTL")) + Val(WW_HEADrow(WW_T10MODELDISTANCE))
                                End If
                            Next
                        Next
                    End If
                Next

                '合計明細レコードの累積
                WW_T0007tbl.Merge(WW_WKDTLtbl)

                'ステータスに"合計"の文字列を設定
                WW_CHOrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                WW_CHOrow("STATUS") = WW_CHOrow("RECODEKBNNAMES")
                WW_CHOrow("PAYKBN") = ""
                WW_CHOrow("PAYKBNNAMES") = ""
                WW_CHOrow("HAIDISTANCETTL") = Val(WW_CHOrow("HAIDISTANCE"))
                WW_CHOrow("KAIDISTANCETTL") = Val(WW_CHOrow("KAIDISTANCE"))
                WW_CHOrow("UNLOADCNTTTL") = Val(WW_CHOrow("UNLOADCNT"))
                WW_CHOrow("BREAKTIMETTL") = Val(WW_CHOrow("NIPPOBREAKTIME")) + Val(WW_CHOrow("BREAKTIME"))
                WW_CHOrow("NIGHTTIMETTL") = Val(WW_CHOrow("NIGHTTIME"))
                WW_CHOrow("ORVERTIMETTL") = Val(WW_CHOrow("ORVERTIME"))
                WW_CHOrow("WNIGHTTIMETTL") = Val(WW_CHOrow("WNIGHTTIME"))
                WW_CHOrow("SWORKTIMETTL") = Val(WW_CHOrow("SWORKTIME"))
                WW_CHOrow("SNIGHTTIMETTL") = Val(WW_CHOrow("SNIGHTTIME"))
                WW_CHOrow("HWORKTIMETTL") = Val(WW_CHOrow("HWORKTIME"))
                WW_CHOrow("HNIGHTTIMETTL") = Val(WW_CHOrow("HNIGHTTIME"))
                WW_CHOrow("SHOUKETUNISSUTTL") = Val(WW_CHOrow("SHOUKETUNISSU"))
                WW_CHOrow("KUMIKETUNISSUTTL") = Val(WW_CHOrow("KUMIKETUNISSU"))
                WW_CHOrow("ETCKETUNISSUTTL") = Val(WW_CHOrow("ETCKETUNISSU"))
                WW_CHOrow("NENKYUNISSUTTL") = Val(WW_CHOrow("NENKYUNISSU"))
                WW_CHOrow("TOKUKYUNISSUTTL") = Val(WW_CHOrow("TOKUKYUNISSU"))
                WW_CHOrow("CHIKOKSOTAINISSUTTL") = Val(WW_CHOrow("CHIKOKSOTAINISSU"))
                WW_CHOrow("STOCKNISSUTTL") = Val(WW_CHOrow("STOCKNISSU"))
                WW_CHOrow("KYOTEIWEEKNISSUTTL") = Val(WW_CHOrow("KYOTEIWEEKNISSU"))
                WW_CHOrow("WEEKNISSUTTL") = Val(WW_CHOrow("WEEKNISSU"))
                WW_CHOrow("DAIKYUNISSUTTL") = Val(WW_CHOrow("DAIKYUNISSU"))
                WW_CHOrow("NENSHINISSUTTL") = Val(WW_CHOrow("NENSHINISSU"))
                WW_CHOrow("SHUKCHOKNNISSUTTL") = Val(WW_CHOrow("SHUKCHOKNNISSU"))
                WW_CHOrow("SHUKCHOKNISSUTTL") = Val(WW_CHOrow("SHUKCHOKNISSU"))

                '2018/02/08 追加
                If WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") AndAlso
                   WW_CHOrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                    WW_CHOrow("SHUKCHOKNHLDNISSUTTL") = Val(WW_CHOrow("SHUKCHOKNHLDNISSU"))
                End If
                If WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") AndAlso
                   WW_CHOrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                    WW_CHOrow("SHUKCHOKHLDNISSUTTL") = Val(WW_CHOrow("SHUKCHOKHLDNISSU"))
                End If
                '2018/02/08 追加

                WW_CHOrow("TOKSAAKAISUTTL") = Val(WW_CHOrow("TOKSAAKAISU"))
                WW_CHOrow("TOKSABKAISUTTL") = Val(WW_CHOrow("TOKSABKAISU"))
                WW_CHOrow("TOKSACKAISUTTL") = Val(WW_CHOrow("TOKSACKAISU"))
                '2018/04/17 追加
                If WW_CHOrow.Table.Columns.Contains("TENKOKAISUTTL") AndAlso
                   WW_CHOrow.Table.Columns.Contains("TENKOKAISU") Then
                    WW_CHOrow("TENKOKAISUTTL") = Val(WW_CHOrow("TENKOKAISU"))
                End If
                WW_CHOrow("HOANTIMETTL") = Val(WW_CHOrow("HOANTIME"))
                WW_CHOrow("KOATUTIMETTL") = Val(WW_CHOrow("KOATUTIME"))
                WW_CHOrow("TOKUSA1TIMETTL") = Val(WW_CHOrow("TOKUSA1TIME"))
                WW_CHOrow("PONPNISSUTTL") = Val(WW_CHOrow("PONPNISSU"))
                WW_CHOrow("BULKNISSUTTL") = Val(WW_CHOrow("BULKNISSU"))
                WW_CHOrow("TRAILERNISSUTTL") = Val(WW_CHOrow("TRAILERNISSU"))
                WW_CHOrow("BKINMUKAISUTTL") = Val(WW_CHOrow("BKINMUKAISU"))
                WW_CHOrow("HAYADETIMETTL") = Val(WW_CHOrow("HAYADETIME"))

                WW_CHOrow("BREAKTIMECHO") = 0
                WW_CHOrow("NIGHTTIMECHO") = 0
                WW_CHOrow("ORVERTIMECHO") = 0
                WW_CHOrow("WNIGHTTIMECHO") = 0
                WW_CHOrow("SWORKTIMECHO") = 0
                WW_CHOrow("SNIGHTTIMECHO") = 0
                WW_CHOrow("HWORKTIMECHO") = 0
                WW_CHOrow("HNIGHTTIMECHO") = 0
                WW_CHOrow("HOANTIMECHO") = 0
                WW_CHOrow("KOATUTIMECHO") = 0
                WW_CHOrow("TOKUSA1TIMECHO") = 0
                WW_CHOrow("HAYADETIMECHO") = 0
                'NJS
                WW_CHOrow("SHACHUHAKNISSUTTL") = Val(WW_CHOrow("SHACHUHAKNISSU"))
                WW_CHOrow("JIKYUSHATIMETTL") = Val(WW_CHOrow("JIKYUSHATIME"))
                WW_CHOrow("NENMATUNISSUTTL") = Val(WW_CHOrow("NENMATUNISSU"))
                WW_CHOrow("MODELDISTANCETTL") = Val(WW_CHOrow("MODELDISTANCE"))
                '近石
                WW_CHOrow("HDAIWORKTIMETTL") = Val(WW_CHOrow("HDAIWORKTIME"))
                WW_CHOrow("HDAINIGHTTIMETTL") = Val(WW_CHOrow("HDAINIGHTTIME"))
                WW_CHOrow("SDAIWORKTIMETTL") = Val(WW_CHOrow("SDAIWORKTIME"))
                WW_CHOrow("SDAINIGHTTIMETTL") = Val(WW_CHOrow("SDAINIGHTTIME"))
                WW_CHOrow("WWORKTIMETTL") = Val(WW_CHOrow("WWORKTIME"))
                WW_CHOrow("WWORKTIMECHO") = 0
                WW_CHOrow("JYOMUTIMETTL") = Val(WW_CHOrow("JYOMUTIME"))
                WW_CHOrow("JYOMUTIMECHO") = 0
                WW_CHOrow("HWORKNISSUTTL") = Val(WW_CHOrow("HWORKNISSU"))
                If WW_CHOrow("CAMPCODE") = "03" Then
                    WW_CHOrow("WORKNISSUTTL") = Val(WW_CHOrow("WORKNISSU"))
                End If
                WW_CHOrow("KAITENCNTTTL") = Val(WW_CHOrow("KAITENCNT1_1")) +
                                            Val(WW_CHOrow("KAITENCNT1_2")) +
                                            Val(WW_CHOrow("KAITENCNT1_3")) +
                                            Val(WW_CHOrow("KAITENCNT1_4")) +
                                            Val(WW_CHOrow("KAITENCNT2_1")) +
                                            Val(WW_CHOrow("KAITENCNT2_2")) +
                                            Val(WW_CHOrow("KAITENCNT2_3")) +
                                            Val(WW_CHOrow("KAITENCNT2_4"))
                WW_CHOrow("KAITENCNTTTL1_1") = Val(WW_CHOrow("KAITENCNT1_1"))
                WW_CHOrow("KAITENCNTTTL1_2") = Val(WW_CHOrow("KAITENCNT1_2"))
                WW_CHOrow("KAITENCNTTTL1_3") = Val(WW_CHOrow("KAITENCNT1_3"))
                WW_CHOrow("KAITENCNTTTL1_4") = Val(WW_CHOrow("KAITENCNT1_4"))
                WW_CHOrow("KAITENCNTTTL2_1") = Val(WW_CHOrow("KAITENCNT2_1"))
                WW_CHOrow("KAITENCNTTTL2_2") = Val(WW_CHOrow("KAITENCNT2_2"))
                WW_CHOrow("KAITENCNTTTL2_3") = Val(WW_CHOrow("KAITENCNT2_3"))
                WW_CHOrow("KAITENCNTTTL2_4") = Val(WW_CHOrow("KAITENCNT2_4"))

                'JKT
                WW_CHOrow("SENJYOCNTTTL") = Val(WW_CHOrow("SENJYOCNT"))
                WW_CHOrow("UNLOADADDCNT1TTL") = Val(WW_CHOrow("UNLOADADDCNT1"))
                WW_CHOrow("UNLOADADDCNT2TTL") = Val(WW_CHOrow("UNLOADADDCNT2"))
                WW_CHOrow("UNLOADADDCNT3TTL") = Val(WW_CHOrow("UNLOADADDCNT3"))
                WW_CHOrow("UNLOADADDCNT4TTL") = Val(WW_CHOrow("UNLOADADDCNT4"))
                WW_CHOrow("LOADINGCNT1TTL") = Val(WW_CHOrow("LOADINGCNT1"))
                WW_CHOrow("LOADINGCNT2TTL") = Val(WW_CHOrow("LOADINGCNT2"))
                WW_CHOrow("SHORTDISTANCE1TTL") = Val(WW_CHOrow("SHORTDISTANCE1"))
                WW_CHOrow("SHORTDISTANCE2TTL") = Val(WW_CHOrow("SHORTDISTANCE2"))

                '時間項目変換（分→時間（HH:MM））
                TimeItemFormat(WW_CHOrow)
            Next

            '合計明細を削除
            CS0026TblSort.TABLE = WW_T0007DTLtbl
            CS0026TblSort.FILTER = "RECODEKBN <> '2'"
            CS0026TblSort.SORTING = "RECODEKBN, STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '勤怠合計明細のマージ
            ioTbl.Merge(WW_T0007tbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_TotalRecodeCreate"      'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  月合計レコードの編集
    Public Sub T0007_TotalRecodeEdit(ByRef ioTbl As DataTable)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Dim WW_IDX As Integer = 0

        Try
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME, SELECT, RECODEKBN"
            ioTbl = CS0026TblSort.sort()

            Dim WW_T0007HEADtbl As DataTable = ioTbl.Clone
            Dim WW_T0007TTLtbl As DataTable = ioTbl.Clone
            Dim WW_T0007ETCtbl As DataTable = ioTbl.Clone
            For i As Integer = 0 To ioTbl.Rows.Count - 1
                Dim ioTblrow As DataRow = ioTbl.Rows(i)

                '勤怠のヘッダレコードを取得（月調整を再作成するため捨てる）
                If ioTblrow("SELECT") = "1" AndAlso ioTblrow("HDKBN") = "H" AndAlso ioTblrow("RECODEKBN") = "1" Then
                    Continue For
                ElseIf ioTblrow("SELECT") = "1" AndAlso ioTblrow("HDKBN") = "H" AndAlso ioTblrow("RECODEKBN") = "0" Then
                    Dim HEADrow As DataRow = WW_T0007HEADtbl.NewRow
                    HEADrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007HEADtbl.Rows.Add(HEADrow)
                ElseIf ioTblrow("SELECT") = "1" AndAlso ioTblrow("HDKBN") = "H" AndAlso ioTblrow("RECODEKBN") = "2" Then
                    Dim TTLrow As DataRow = WW_T0007TTLtbl.NewRow
                    TTLrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007TTLtbl.Rows.Add(TTLrow)
                Else
                    '勤怠の明細レコードと削除レコードを取得
                    Dim DTLrow As DataRow = WW_T0007ETCtbl.NewRow
                    DTLrow.ItemArray = ioTblrow.ItemArray
                    WW_T0007ETCtbl.Rows.Add(DTLrow)
                End If
            Next


            WW_IDX = 0
            Dim WW_TTLNEWtbl As DataTable = WW_T0007TTLtbl.Clone

            For Each WW_INrow As DataRow In WW_T0007TTLtbl.Rows
                Dim WW_TTLrow As DataRow = WW_TTLNEWtbl.NewRow
                WW_TTLrow.ItemArray = WW_INrow.ItemArray

                '集計項目初期クリア
                SumItem_Init(WW_TTLrow)

                For i As Integer = WW_IDX To WW_T0007HEADtbl.Rows.Count - 1
                    Dim WW_HEADrow As DataRow = WW_T0007HEADtbl.Rows(i)
                    If WW_HEADrow("STAFFCODE") = WW_TTLrow("STAFFCODE") Then
                        '合計（日別のレコードを加算）
                        WW_TTLrow("WORKTIME") = Val(WW_TTLrow("WORKTIME")) + HHMMtoMinutes(WW_HEADrow("WORKTIME"))
                        WW_TTLrow("MOVETIME") = Val(WW_TTLrow("MOVETIME")) + HHMMtoMinutes(WW_HEADrow("MOVETIME"))
                        WW_TTLrow("ACTTIME") = Val(WW_TTLrow("ACTTIME")) + HHMMtoMinutes(WW_HEADrow("ACTTIME"))
                        WW_TTLrow("HAIDISTANCE") = Val(WW_TTLrow("HAIDISTANCE")) + WW_HEADrow("HAIDISTANCE")
                        WW_TTLrow("KAIDISTANCE") = Val(WW_TTLrow("KAIDISTANCE")) + WW_HEADrow("KAIDISTANCE")
                        WW_TTLrow("UNLOADCNT") = Val(WW_TTLrow("UNLOADCNT")) + WW_HEADrow("UNLOADCNT")
                        If HHMMtoMinutes(WW_HEADrow("ACTTIME")) > 0 Then
                            WW_TTLrow("BINDTIME") = Val(WW_TTLrow("BINDTIME")) + HHMMtoMinutes(WW_HEADrow("BINDTIME"))
                        Else
                            WW_TTLrow("BINDTIME") = Val(WW_TTLrow("BINDTIME")) + 0
                        End If
                        WW_TTLrow("NIPPOBREAKTIME") = Val(WW_TTLrow("NIPPOBREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                        WW_TTLrow("BREAKTIME") = Val(WW_TTLrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                        WW_TTLrow("NIGHTTIME") = Val(WW_TTLrow("NIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("NIGHTTIME"))
                        WW_TTLrow("ORVERTIME") = Val(WW_TTLrow("ORVERTIME")) + HHMMtoMinutes(WW_HEADrow("ORVERTIME"))
                        If WW_TTLrow.Table.Columns.Contains("ORVERTIMEADD") Then
                            WW_TTLrow("ORVERTIMEADD") = Val(WW_TTLrow("ORVERTIMEADD")) + HHMMtoMinutes(WW_HEADrow("ORVERTIMEADD"))
                        End If
                        WW_TTLrow("WNIGHTTIME") = Val(WW_TTLrow("WNIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("WNIGHTTIME"))
                        If WW_TTLrow.Table.Columns.Contains("WNIGHTTIMEADD") Then
                            WW_TTLrow("WNIGHTTIMEADD") = Val(WW_TTLrow("WNIGHTTIMEADD")) + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMEADD"))
                        End If
                        WW_TTLrow("SWORKTIME") = Val(WW_TTLrow("SWORKTIME")) + HHMMtoMinutes(WW_HEADrow("SWORKTIME"))
                        If WW_TTLrow.Table.Columns.Contains("SWORKTIMEADD") Then
                            WW_TTLrow("SWORKTIMEADD") = Val(WW_TTLrow("SWORKTIMEADD")) + HHMMtoMinutes(WW_HEADrow("SWORKTIMEADD"))
                        End If
                        WW_TTLrow("SNIGHTTIME") = Val(WW_TTLrow("SNIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("SNIGHTTIME"))
                        If WW_TTLrow.Table.Columns.Contains("SNIGHTTIMEADD") Then
                            WW_TTLrow("SNIGHTTIMEADD") = Val(WW_TTLrow("SNIGHTTIMEADD")) + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMEADD"))
                        End If
                        WW_TTLrow("HWORKTIME") = Val(WW_TTLrow("HWORKTIME")) + HHMMtoMinutes(WW_HEADrow("HWORKTIME"))
                        WW_TTLrow("HNIGHTTIME") = Val(WW_TTLrow("HNIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("HNIGHTTIME"))
                        WW_TTLrow("SHOUKETUNISSU") = Val(WW_TTLrow("SHOUKETUNISSU")) + WW_HEADrow("SHOUKETUNISSU")
                        WW_TTLrow("KUMIKETUNISSU") = Val(WW_TTLrow("KUMIKETUNISSU")) + WW_HEADrow("KUMIKETUNISSU")
                        WW_TTLrow("ETCKETUNISSU") = Val(WW_TTLrow("ETCKETUNISSU")) + WW_HEADrow("ETCKETUNISSU")
                        WW_TTLrow("NENKYUNISSU") = Val(WW_TTLrow("NENKYUNISSU")) + WW_HEADrow("NENKYUNISSU")
                        WW_TTLrow("TOKUKYUNISSU") = Val(WW_TTLrow("TOKUKYUNISSU")) + WW_HEADrow("TOKUKYUNISSU")
                        WW_TTLrow("CHIKOKSOTAINISSU") = Val(WW_TTLrow("CHIKOKSOTAINISSU")) + WW_HEADrow("CHIKOKSOTAINISSU")
                        WW_TTLrow("STOCKNISSU") = Val(WW_TTLrow("STOCKNISSU")) + WW_HEADrow("STOCKNISSU")
                        WW_TTLrow("KYOTEIWEEKNISSU") = Val(WW_TTLrow("KYOTEIWEEKNISSU")) + WW_HEADrow("KYOTEIWEEKNISSU")
                        WW_TTLrow("WEEKNISSU") = Val(WW_TTLrow("WEEKNISSU")) + WW_HEADrow("WEEKNISSU")
                        WW_TTLrow("DAIKYUNISSU") = Val(WW_TTLrow("DAIKYUNISSU")) + WW_HEADrow("DAIKYUNISSU")
                        WW_TTLrow("NENSHINISSU") = Val(WW_TTLrow("NENSHINISSU")) + WW_HEADrow("NENSHINISSU")
                        WW_TTLrow("SHUKCHOKNNISSU") = Val(WW_TTLrow("SHUKCHOKNNISSU")) + WW_HEADrow("SHUKCHOKNNISSU")
                        WW_TTLrow("SHUKCHOKNISSU") = Val(WW_TTLrow("SHUKCHOKNISSU")) + WW_HEADrow("SHUKCHOKNISSU")
                        '2018/02/08 追加
                        If WW_TTLrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                            WW_TTLrow("SHUKCHOKNHLDNISSU") = Val(WW_TTLrow("SHUKCHOKNHLDNISSU")) + WW_HEADrow("SHUKCHOKNHLDNISSU")
                        End If
                        If WW_TTLrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                            WW_TTLrow("SHUKCHOKHLDNISSU") = Val(WW_TTLrow("SHUKCHOKHLDNISSU")) + WW_HEADrow("SHUKCHOKHLDNISSU")
                        End If
                        '2018/02/08 追加
                        WW_TTLrow("TOKSAAKAISU") = Val(WW_TTLrow("TOKSAAKAISU")) + WW_HEADrow("TOKSAAKAISU")
                        WW_TTLrow("TOKSABKAISU") = Val(WW_TTLrow("TOKSABKAISU")) + WW_HEADrow("TOKSABKAISU")
                        WW_TTLrow("TOKSACKAISU") = Val(WW_TTLrow("TOKSACKAISU")) + WW_HEADrow("TOKSACKAISU")
                        '2018/04/17 追加
                        If WW_TTLrow.Table.Columns.Contains("TENKOKAISU") Then
                            WW_TTLrow("TENKOKAISU") = Val(WW_TTLrow("TENKOKAISU")) + WW_HEADrow("TENKOKAISU")
                        End If
                        WW_TTLrow("HOANTIME") = Val(WW_TTLrow("HOANTIME")) + HHMMtoMinutes(WW_HEADrow("HOANTIME"))
                        WW_TTLrow("KOATUTIME") = Val(WW_TTLrow("KOATUTIME")) + HHMMtoMinutes(WW_HEADrow("KOATUTIME"))
                        WW_TTLrow("TOKUSA1TIME") = Val(WW_TTLrow("TOKUSA1TIME")) + HHMMtoMinutes(WW_HEADrow("TOKUSA1TIME"))
                        WW_TTLrow("PONPNISSU") = Val(WW_TTLrow("PONPNISSU")) + WW_HEADrow("PONPNISSU")
                        WW_TTLrow("BULKNISSU") = Val(WW_TTLrow("BULKNISSU")) + WW_HEADrow("BULKNISSU")
                        WW_TTLrow("TRAILERNISSU") = Val(WW_TTLrow("TRAILERNISSU")) + WW_HEADrow("TRAILERNISSU")
                        WW_TTLrow("BKINMUKAISU") = Val(WW_TTLrow("BKINMUKAISU")) + WW_HEADrow("BKINMUKAISU")
                        WW_TTLrow("HAYADETIME") = Val(WW_TTLrow("HAYADETIME")) + HHMMtoMinutes(WW_HEADrow("HAYADETIME"))
                        'NJS
                        WW_TTLrow("HAISOTIME") = Val(WW_TTLrow("HAISOTIME")) + HHMMtoMinutes(WW_HEADrow("HAISOTIME"))
                        WW_TTLrow("JIKYUSHATIME") = Val(WW_TTLrow("JIKYUSHATIME")) + HHMMtoMinutes(WW_HEADrow("JIKYUSHATIME"))
                        WW_TTLrow("NENMATUNISSU") = Val(WW_TTLrow("NENMATUNISSU")) + Val(WW_HEADrow("NENMATUNISSU"))
                        WW_TTLrow("SHACHUHAKNISSU") = Val(WW_TTLrow("SHACHUHAKNISSU")) + Val(WW_HEADrow("SHACHUHAKNISSU"))

                        For j As Integer = 1 To 6
                            Dim WW_MODELDISTANCE As String = "T10MODELDISTANCE" & j.ToString
                            WW_TTLrow("MODELDISTANCE") = Val(WW_TTLrow("MODELDISTANCE")) + Val(WW_HEADrow(WW_MODELDISTANCE))
                        Next

                        '近石
                        WW_TTLrow("HDAIWORKTIME") = Val(WW_TTLrow("HDAIWORKTIME")) + HHMMtoMinutes(WW_HEADrow("HDAIWORKTIME"))
                        WW_TTLrow("HDAINIGHTTIME") = Val(WW_TTLrow("HDAINIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("HDAINIGHTTIME"))
                        WW_TTLrow("SDAIWORKTIME") = Val(WW_TTLrow("SDAIWORKTIME")) + HHMMtoMinutes(WW_HEADrow("SDAIWORKTIME"))
                        WW_TTLrow("SDAINIGHTTIME") = Val(WW_TTLrow("SDAINIGHTTIME")) + HHMMtoMinutes(WW_HEADrow("SDAINIGHTTIME"))
                        WW_TTLrow("WWORKTIME") = Val(WW_TTLrow("WWORKTIME")) + HHMMtoMinutes(WW_HEADrow("WWORKTIME"))
                        WW_TTLrow("JYOMUTIME") = Val(WW_TTLrow("JYOMUTIME")) + HHMMtoMinutes(WW_HEADrow("JYOMUTIME"))
                        WW_TTLrow("HWORKNISSU") = Val(WW_TTLrow("HWORKNISSU")) + Val(WW_HEADrow("HWORKNISSU"))
                        If WW_HEADrow("CAMPCODE") = "03" Then
                            WW_TTLrow("WORKNISSU") = Val(WW_TTLrow("WORKNISSU")) + Val(WW_HEADrow("WORKNISSU"))
                        End If
                        WW_TTLrow("KAITENCNT") = Val(WW_TTLrow("KAITENCNT")) + Val(WW_HEADrow("KAITENCNT"))
                        WW_TTLrow("KAITENCNT1_1") = Val(WW_TTLrow("KAITENCNT1_1")) + Val(WW_HEADrow("KAITENCNT1_1"))
                        WW_TTLrow("KAITENCNT1_2") = Val(WW_TTLrow("KAITENCNT1_2")) + Val(WW_HEADrow("KAITENCNT1_2"))
                        WW_TTLrow("KAITENCNT1_3") = Val(WW_TTLrow("KAITENCNT1_3")) + Val(WW_HEADrow("KAITENCNT1_3"))
                        WW_TTLrow("KAITENCNT1_4") = Val(WW_TTLrow("KAITENCNT1_4")) + Val(WW_HEADrow("KAITENCNT1_4"))
                        WW_TTLrow("KAITENCNT2_1") = Val(WW_TTLrow("KAITENCNT2_1")) + Val(WW_HEADrow("KAITENCNT2_1"))
                        WW_TTLrow("KAITENCNT2_2") = Val(WW_TTLrow("KAITENCNT2_2")) + Val(WW_HEADrow("KAITENCNT2_2"))
                        WW_TTLrow("KAITENCNT2_3") = Val(WW_TTLrow("KAITENCNT2_3")) + Val(WW_HEADrow("KAITENCNT2_3"))
                        WW_TTLrow("KAITENCNT2_4") = Val(WW_TTLrow("KAITENCNT2_4")) + Val(WW_HEADrow("KAITENCNT2_4"))
                        'JKT
                        WW_TTLrow("SENJYOCNT") = Val(WW_TTLrow("SENJYOCNT")) + Val(WW_HEADrow("SENJYOCNT"))
                        WW_TTLrow("UNLOADADDCNT1") = Val(WW_TTLrow("UNLOADADDCNT1")) + Val(WW_HEADrow("UNLOADADDCNT1"))
                        WW_TTLrow("UNLOADADDCNT2") = Val(WW_TTLrow("UNLOADADDCNT2")) + Val(WW_HEADrow("UNLOADADDCNT2"))
                        WW_TTLrow("UNLOADADDCNT3") = Val(WW_TTLrow("UNLOADADDCNT3")) + Val(WW_HEADrow("UNLOADADDCNT3"))
                        WW_TTLrow("UNLOADADDCNT4") = Val(WW_TTLrow("UNLOADADDCNT4")) + Val(WW_HEADrow("UNLOADADDCNT4"))
                        WW_TTLrow("LOADINGCNT1") = Val(WW_TTLrow("LOADINGCNT1")) + Val(WW_HEADrow("LOADINGCNT1"))
                        WW_TTLrow("LOADINGCNT2") = Val(WW_TTLrow("LOADINGCNT2")) + Val(WW_HEADrow("LOADINGCNT2"))
                        WW_TTLrow("SHORTDISTANCE1") = Val(WW_TTLrow("SHORTDISTANCE1")) + Val(WW_HEADrow("SHORTDISTANCE1"))
                        WW_TTLrow("SHORTDISTANCE2") = Val(WW_TTLrow("SHORTDISTANCE2")) + Val(WW_HEADrow("SHORTDISTANCE2"))
                    Else
                        WW_IDX = i
                        Exit For
                    End If
                Next

                'ステータスに"合計"の文字列を設定
                WW_TTLrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                WW_TTLrow("STATUS") = WW_TTLrow("RECODEKBNNAMES")
                WW_TTLrow("PAYKBN") = ""
                WW_TTLrow("PAYKBNNAMES") = ""
                WW_TTLrow("HAIDISTANCETTL") = Val(WW_TTLrow("HAIDISTANCE")) + Val(WW_TTLrow("HAIDISTANCECHO"))
                WW_TTLrow("KAIDISTANCETTL") = Val(WW_TTLrow("KAIDISTANCE")) + Val(WW_TTLrow("KAIDISTANCECHO"))
                WW_TTLrow("UNLOADCNTTTL") = Val(WW_TTLrow("UNLOADCNT")) + Val(WW_TTLrow("UNLOADCNTCHO"))
                WW_TTLrow("BREAKTIMETTL") = Val(WW_TTLrow("NIPPOBREAKTIME")) + Val(WW_TTLrow("BREAKTIME")) + HHMMtoMinutes(WW_TTLrow("BREAKTIMECHO"))
                WW_TTLrow("NIGHTTIMETTL") = Val(WW_TTLrow("NIGHTTIME")) + HHMMtoMinutes(WW_TTLrow("NIGHTTIMECHO"))
                WW_TTLrow("ORVERTIMETTL") = Val(WW_TTLrow("ORVERTIME")) + HHMMtoMinutes(WW_TTLrow("ORVERTIMECHO"))
                WW_TTLrow("WNIGHTTIMETTL") = Val(WW_TTLrow("WNIGHTTIME")) + HHMMtoMinutes(WW_TTLrow("WNIGHTTIMECHO"))
                WW_TTLrow("SWORKTIMETTL") = Val(WW_TTLrow("SWORKTIME")) + HHMMtoMinutes(WW_TTLrow("SWORKTIMECHO"))
                WW_TTLrow("SNIGHTTIMETTL") = Val(WW_TTLrow("SNIGHTTIME")) + HHMMtoMinutes(WW_TTLrow("SNIGHTTIMECHO"))
                WW_TTLrow("HWORKTIMETTL") = Val(WW_TTLrow("HWORKTIME")) + HHMMtoMinutes(WW_TTLrow("HWORKTIMECHO"))
                WW_TTLrow("HNIGHTTIMETTL") = Val(WW_TTLrow("HNIGHTTIME")) + HHMMtoMinutes(WW_TTLrow("HNIGHTTIMECHO"))
                WW_TTLrow("SHOUKETUNISSUTTL") = Val(WW_TTLrow("SHOUKETUNISSU")) + Val(WW_TTLrow("SHOUKETUNISSUCHO"))
                WW_TTLrow("KUMIKETUNISSUTTL") = Val(WW_TTLrow("KUMIKETUNISSU")) + Val(WW_TTLrow("KUMIKETUNISSUCHO"))
                WW_TTLrow("ETCKETUNISSUTTL") = Val(WW_TTLrow("ETCKETUNISSU")) + Val(WW_TTLrow("ETCKETUNISSUCHO"))
                WW_TTLrow("NENKYUNISSUTTL") = Val(WW_TTLrow("NENKYUNISSU")) + Val(WW_TTLrow("NENKYUNISSUCHO"))
                WW_TTLrow("TOKUKYUNISSUTTL") = Val(WW_TTLrow("TOKUKYUNISSU")) + Val(WW_TTLrow("TOKUKYUNISSUCHO"))
                WW_TTLrow("CHIKOKSOTAINISSUTTL") = Val(WW_TTLrow("CHIKOKSOTAINISSU")) + Val(WW_TTLrow("CHIKOKSOTAINISSUCHO"))
                WW_TTLrow("STOCKNISSUTTL") = Val(WW_TTLrow("STOCKNISSU")) + Val(WW_TTLrow("STOCKNISSUCHO"))
                WW_TTLrow("KYOTEIWEEKNISSUTTL") = Val(WW_TTLrow("KYOTEIWEEKNISSU")) + Val(WW_TTLrow("KYOTEIWEEKNISSUCHO"))
                WW_TTLrow("WEEKNISSUTTL") = Val(WW_TTLrow("WEEKNISSU")) + Val(WW_TTLrow("WEEKNISSUCHO"))
                WW_TTLrow("DAIKYUNISSUTTL") = Val(WW_TTLrow("DAIKYUNISSU")) + Val(WW_TTLrow("DAIKYUNISSUCHO"))
                WW_TTLrow("NENSHINISSUTTL") = Val(WW_TTLrow("NENSHINISSU")) + Val(WW_TTLrow("NENSHINISSUCHO"))
                WW_TTLrow("SHUKCHOKNNISSUTTL") = Val(WW_TTLrow("SHUKCHOKNNISSU")) + Val(WW_TTLrow("SHUKCHOKNNISSUCHO"))
                WW_TTLrow("SHUKCHOKNISSUTTL") = Val(WW_TTLrow("SHUKCHOKNISSU")) + Val(WW_TTLrow("SHUKCHOKNISSUCHO"))
                '2018/02/08 追加
                If WW_TTLrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") AndAlso
                   WW_TTLrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") AndAlso
                   WW_TTLrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUCHO") Then
                    WW_TTLrow("SHUKCHOKNHLDNISSUTTL") = Val(WW_TTLrow("SHUKCHOKNHLDNISSU")) + Val(WW_TTLrow("SHUKCHOKNHLDNISSUCHO"))
                End If
                If WW_TTLrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") AndAlso
                   WW_TTLrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") AndAlso
                   WW_TTLrow.Table.Columns.Contains("SHUKCHOKHLDNISSUCHO") Then
                    WW_TTLrow("SHUKCHOKHLDNISSUTTL") = Val(WW_TTLrow("SHUKCHOKHLDNISSU")) + Val(WW_TTLrow("SHUKCHOKHLDNISSUCHO"))
                End If
                '2018/02/08 追加
                WW_TTLrow("TOKSAAKAISUTTL") = Val(WW_TTLrow("TOKSAAKAISU")) + Val(WW_TTLrow("TOKSAAKAISUCHO"))
                WW_TTLrow("TOKSABKAISUTTL") = Val(WW_TTLrow("TOKSABKAISU")) + Val(WW_TTLrow("TOKSABKAISUCHO"))
                WW_TTLrow("TOKSACKAISUTTL") = Val(WW_TTLrow("TOKSACKAISU")) + Val(WW_TTLrow("TOKSACKAISUCHO"))
                '2018/04/17 追加
                If WW_TTLrow.Table.Columns.Contains("TENKOKAISUTTL") AndAlso
                   WW_TTLrow.Table.Columns.Contains("TENKOKAISU") AndAlso
                   WW_TTLrow.Table.Columns.Contains("TENKOKAISUCHO") Then
                    WW_TTLrow("TENKOKAISUTTL") = Val(WW_TTLrow("TENKOKAISU")) + Val(WW_TTLrow("TENKOKAISUCHO"))
                End If
                WW_TTLrow("HOANTIMETTL") = Val(WW_TTLrow("HOANTIME")) + HHMMtoMinutes(WW_TTLrow("HOANTIMECHO"))
                WW_TTLrow("KOATUTIMETTL") = Val(WW_TTLrow("KOATUTIME")) + HHMMtoMinutes(WW_TTLrow("KOATUTIMECHO"))
                WW_TTLrow("TOKUSA1TIMETTL") = Val(WW_TTLrow("TOKUSA1TIME")) + HHMMtoMinutes(WW_TTLrow("TOKUSA1TIMECHO"))
                WW_TTLrow("PONPNISSUTTL") = Val(WW_TTLrow("PONPNISSU")) + Val(WW_TTLrow("PONPNISSUCHO"))
                WW_TTLrow("BULKNISSUTTL") = Val(WW_TTLrow("BULKNISSU")) + Val(WW_TTLrow("BULKNISSUCHO"))
                WW_TTLrow("TRAILERNISSUTTL") = Val(WW_TTLrow("TRAILERNISSU")) + Val(WW_TTLrow("TRAILERNISSUCHO"))
                WW_TTLrow("BKINMUKAISUTTL") = Val(WW_TTLrow("BKINMUKAISU")) + Val(WW_TTLrow("BKINMUKAISUCHO"))
                WW_TTLrow("HAYADETIMETTL") = Val(WW_TTLrow("HAYADETIME")) + HHMMtoMinutes(WW_TTLrow("HAYADETIMECHO"))

                WW_TTLrow("BREAKTIMECHO") = HHMMtoMinutes(WW_TTLrow("BREAKTIMECHO"))
                WW_TTLrow("NIGHTTIMECHO") = HHMMtoMinutes(WW_TTLrow("NIGHTTIMECHO"))
                WW_TTLrow("ORVERTIMECHO") = HHMMtoMinutes(WW_TTLrow("ORVERTIMECHO"))
                WW_TTLrow("WNIGHTTIMECHO") = HHMMtoMinutes(WW_TTLrow("WNIGHTTIMECHO"))
                WW_TTLrow("SWORKTIMECHO") = HHMMtoMinutes(WW_TTLrow("SWORKTIMECHO"))
                WW_TTLrow("SNIGHTTIMECHO") = HHMMtoMinutes(WW_TTLrow("SNIGHTTIMECHO"))
                WW_TTLrow("HWORKTIMECHO") = HHMMtoMinutes(WW_TTLrow("HWORKTIMECHO"))
                WW_TTLrow("HNIGHTTIMECHO") = HHMMtoMinutes(WW_TTLrow("HNIGHTTIMECHO"))
                WW_TTLrow("HOANTIMECHO") = HHMMtoMinutes(WW_TTLrow("HOANTIMECHO"))
                WW_TTLrow("KOATUTIMECHO") = HHMMtoMinutes(WW_TTLrow("KOATUTIMECHO"))
                WW_TTLrow("TOKUSA1TIMECHO") = HHMMtoMinutes(WW_TTLrow("TOKUSA1TIMECHO"))
                WW_TTLrow("HAYADETIMECHO") = HHMMtoMinutes(WW_TTLrow("HAYADETIMECHO"))
                'NJS
                WW_TTLrow("NENMATUNISSUTTL") = Val(WW_TTLrow("NENMATUNISSU")) + Val(WW_TTLrow("NENMATUNISSUCHO"))
                WW_TTLrow("SHACHUHAKNISSUTTL") = Val(WW_TTLrow("SHACHUHAKNISSU")) + Val(WW_TTLrow("SHACHUHAKNISSUCHO"))
                WW_TTLrow("JIKYUSHATIMETTL") = Val(WW_TTLrow("JIKYUSHATIME")) + HHMMtoMinutes(WW_TTLrow("JIKYUSHATIMECHO"))
                WW_TTLrow("JIKYUSHATIMECHO") = HHMMtoMinutes(WW_TTLrow("JIKYUSHATIMECHO"))
                WW_TTLrow("MODELDISTANCETTL") = Val(WW_TTLrow("MODELDISTANCE")) + Val(WW_TTLrow("MODELDISTANCECHO"))

                '近石
                WW_TTLrow("HDAIWORKTIMETTL") = Val(WW_TTLrow("HDAIWORKTIME")) + HHMMtoMinutes(WW_TTLrow("HDAIWORKTIMECHO"))
                WW_TTLrow("HDAINIGHTTIMETTL") = Val(WW_TTLrow("HDAINIGHTTIME")) + HHMMtoMinutes(WW_TTLrow("HDAINIGHTTIMECHO"))
                WW_TTLrow("SDAIWORKTIMETTL") = Val(WW_TTLrow("SDAIWORKTIME")) + HHMMtoMinutes(WW_TTLrow("SDAIWORKTIMECHO"))
                WW_TTLrow("SDAINIGHTTIMETTL") = Val(WW_TTLrow("SDAINIGHTTIME")) + HHMMtoMinutes(WW_TTLrow("SDAINIGHTTIMECHO"))
                WW_TTLrow("WWORKTIMETTL") = Val(WW_TTLrow("WWORKTIME")) + HHMMtoMinutes(WW_TTLrow("WWORKTIMECHO"))
                WW_TTLrow("WWORKTIMECHO") = HHMMtoMinutes(WW_TTLrow("WWORKTIMECHO"))
                WW_TTLrow("JYOMUTIMETTL") = Val(WW_TTLrow("JYOMUTIME")) + HHMMtoMinutes(WW_TTLrow("JYOMUTIMECHO"))
                WW_TTLrow("JYOMUTIMECHO") = HHMMtoMinutes(WW_TTLrow("JYOMUTIMECHO"))
                WW_TTLrow("HWORKNISSUTTL") = Val(WW_TTLrow("HWORKNISSU")) + Val(WW_TTLrow("HWORKNISSUCHO"))
                If WW_TTLrow("CAMPCODE") = "03" Then
                    WW_TTLrow("WORKNISSUTTL") = Val(WW_TTLrow("WORKNISSU")) + Val(WW_TTLrow("WORKNISSUCHO"))
                End If
                WW_TTLrow("KAITENCNTTTL") = Val(WW_TTLrow("KAITENCNT")) + Val(WW_TTLrow("KAITENCNTCHO"))
                WW_TTLrow("KAITENCNTTTL1_1") = Val(WW_TTLrow("KAITENCNT1_1")) + Val(WW_TTLrow("KAITENCNTCHO1_1"))
                WW_TTLrow("KAITENCNTTTL1_2") = Val(WW_TTLrow("KAITENCNT1_2")) + Val(WW_TTLrow("KAITENCNTCHO1_2"))
                WW_TTLrow("KAITENCNTTTL1_3") = Val(WW_TTLrow("KAITENCNT1_3")) + Val(WW_TTLrow("KAITENCNTCHO1_3"))
                WW_TTLrow("KAITENCNTTTL1_4") = Val(WW_TTLrow("KAITENCNT1_4")) + Val(WW_TTLrow("KAITENCNTCHO1_4"))
                WW_TTLrow("KAITENCNTTTL2_1") = Val(WW_TTLrow("KAITENCNT2_1")) + Val(WW_TTLrow("KAITENCNTCHO2_1"))
                WW_TTLrow("KAITENCNTTTL2_2") = Val(WW_TTLrow("KAITENCNT2_2")) + Val(WW_TTLrow("KAITENCNTCHO2_2"))
                WW_TTLrow("KAITENCNTTTL2_3") = Val(WW_TTLrow("KAITENCNT2_3")) + Val(WW_TTLrow("KAITENCNTCHO2_3"))
                WW_TTLrow("KAITENCNTTTL2_4") = Val(WW_TTLrow("KAITENCNT2_4")) + Val(WW_TTLrow("KAITENCNTCHO2_4"))

                'JKT
                WW_TTLrow("SENJYOCNTTTL") = Val(WW_TTLrow("SENJYOCNT")) + Val(WW_TTLrow("SENJYOCNTCHO"))
                WW_TTLrow("UNLOADADDCNT1TTL") = Val(WW_TTLrow("UNLOADADDCNT1")) + Val(WW_TTLrow("UNLOADADDCNT1CHO"))
                WW_TTLrow("UNLOADADDCNT2TTL") = Val(WW_TTLrow("UNLOADADDCNT2")) + Val(WW_TTLrow("UNLOADADDCNT2CHO"))
                WW_TTLrow("UNLOADADDCNT3TTL") = Val(WW_TTLrow("UNLOADADDCNT3")) + Val(WW_TTLrow("UNLOADADDCNT3CHO"))
                WW_TTLrow("UNLOADADDCNT4TTL") = Val(WW_TTLrow("UNLOADADDCNT4")) + Val(WW_TTLrow("UNLOADADDCNT4CHO"))
                WW_TTLrow("LOADINGCNT1TTL") = Val(WW_TTLrow("LOADINGCNT1")) + Val(WW_TTLrow("LOADINGCNT1CHO"))
                WW_TTLrow("LOADINGCNT2TTL") = Val(WW_TTLrow("LOADINGCNT2")) + Val(WW_TTLrow("LOADINGCNT2CHO"))
                WW_TTLrow("SHORTDISTANCE1TTL") = Val(WW_TTLrow("SHORTDISTANCE1")) + Val(WW_TTLrow("SHORTDISTANCE1CHO"))
                WW_TTLrow("SHORTDISTANCE2TTL") = Val(WW_TTLrow("SHORTDISTANCE2")) + Val(WW_TTLrow("SHORTDISTANCE2CHO"))

                '時間項目変換（分→時間（HH:MM））
                TimeItemFormat(WW_TTLrow)

                WW_TTLNEWtbl.Rows.Add(WW_TTLrow)
            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '合計のマージ
            ioTbl.Merge(WW_TTLNEWtbl)

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007ETCtbl)


            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007TTLtbl.Dispose()
            WW_T0007TTLtbl = Nothing
            WW_T0007ETCtbl.Dispose()
            WW_T0007ETCtbl = Nothing
            WW_TTLNEWtbl.Dispose()
            WW_TTLNEWtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_TotalRecodeEdit"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  重複チェック
    Public Sub T0007_DuplCheck(ByRef iTbl As DataTable, ByRef oErrMsg As String, ByRef oRtn As String)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Try
            oRtn = C_MESSAGE_NO.NORMAL
            oErrMsg = ""

            Dim WW_T7SELtbl As DataTable = iTbl.Clone

            CS0026TblSort.TABLE = iTbl
            CS0026TblSort.FILTER = ""
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, RECODEKBN, SEQ, SELECT"
            WW_T7SELtbl = CS0026TblSort.sort()

            Dim WW_NEWKEY As String = ""
            Dim WW_OLDKEY As String = ""

            For Each WW_CHOrow As DataRow In WW_T7SELtbl.Rows
                If WW_CHOrow("SELECT") = "1" And WW_CHOrow("RECODEKBN") <> "1" And WW_CHOrow("DATAKBN") = "K" Then
                    WW_NEWKEY = WW_CHOrow("STAFFCODE") & WW_CHOrow("WORKDATE") & WW_CHOrow("HDKBN") & WW_CHOrow("RECODEKBN") & WW_CHOrow("SEQ")
                    If WW_OLDKEY = WW_NEWKEY Then
                        'エラーレポート編集
                        Dim WW_ERR_MES As String = ""
                        WW_ERR_MES = "・データ重複です。"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 日付        =" & WW_CHOrow("WORKDATE") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 配属部署    =" & WW_CHOrow("HORG") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 配属部署名  =" & WW_CHOrow("HORGNAMES") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員      =" & WW_CHOrow("STAFFCODE") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> 従業員名    =" & WW_CHOrow("STAFFNAMES") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> HD区分      =" & WW_CHOrow("HDKBN") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> レコード区分=" & WW_CHOrow("RECODEKBN") & " ,"
                        WW_ERR_MES = WW_ERR_MES & ControlChars.NewLine & "  --> ＳＥＱ      =" & WW_CHOrow("SEQ") & " ,"
                        oErrMsg = oErrMsg & ControlChars.NewLine & WW_ERR_MES
                        oRtn = "10052"
                    End If
                    WW_OLDKEY = WW_NEWKEY
                End If
            Next

            WW_T7SELtbl.Dispose()
            WW_T7SELtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_DuplCheck"        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            oRtn = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            Exit Sub

        End Try

    End Sub

    ' ***  残業計算
    Public Sub T0007_KintaiCalc_OLD(ByRef ioTbl As DataTable, ByRef iTbl As DataTable, Optional ByVal hydFlg As Boolean = False)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite
'        Dim COMMON As New OFFICE.COMMON

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_IDX2 As Integer = 0
        Dim WW_IDX3 As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""

        '新潟東港
        Const CONST_HIGASHIKO As String = "021506"

        Try
            '削除レコードを取得
            Dim WW_T0007DELtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DELtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl = CS0026TblSort.sort()

            '勤怠の明細レコードを取得
            Dim WW_T0007DTLtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '休憩レコードを取得
            Dim WW_T0007BBtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and WORKKBN = 'BB' "
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007BBtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl2 As DataTable = New DataTable
            CS0026TblSort.TABLE = iTbl
            CS0026TblSort.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and DELFLG = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl2 = CS0026TblSort.sort()

            '直前、翌日取得用VIEW
            Dim iT0007view As DataView
            iT0007view = New DataView(WW_T0007HEADtbl2)
            iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

            WW_IDX = 0
            For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
                'STATUS<>''（勤怠に変更が発生しているレコード）
                If WW_HEADrow("RECODEKBN") = "0" Then
                Else
                    Continue For
                End If

                '************************************************************
                '*   勤怠日数設定                                           *
                '************************************************************
                NissuItem_Init(WW_HEADrow)
                Select Case WW_HEADrow("PAYKBN")
                    Case "00"
                        '○勤怠区分(00:通常) …　出勤扱い(所労=1 )
                        If WW_HEADrow("HOLIDAYKBN") = "0" Then
                        End If
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                '2018/02/01 追加
                                If WW_HEADrow("STTIME") = "00:00" And WW_HEADrow("ENDTIME") = "00:00" Then
                                Else
                                    WW_HEADrow("NENSHINISSU") = 1    '年始出勤日数
                                    WW_HEADrow("NENSHINISSUTTL") = 1 '年始出勤日数
                                End If
                                '2018/02/01 追加
                            End If
                        End If
                    Case "01"
                        '○勤怠区分(01:年休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("NENKYUNISSU") = 1        '年次有給休暇
                            WW_HEADrow("NENKYUNISSUTTL") = 1     '年次有給休暇
                        End If
                    Case "02"
                        '○勤怠区分(2:特休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("TOKUKYUNISSU") = 1       '特別有給休暇
                            WW_HEADrow("TOKUKYUNISSUTTL") = 1    '特別有給休暇
                        End If
                    Case "03"
                        '○勤怠区分(3:遅刻早退) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("CHIKOKSOTAINISSU") = 1    '遅刻早退日数
                            WW_HEADrow("CHIKOKSOTAINISSUTTL") = 1 '遅刻早退日数
                        End If
                    Case "04"
                        '○勤怠区分(4:ｽﾄｯｸ休暇) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("STOCKNISSU") = 1         'ストック休暇日数
                            WW_HEADrow("STOCKNISSUTTL") = 1      'ストック休暇日数
                        End If
                    Case "05"
                        '○勤怠区分(5:協約週休) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KYOTEIWEEKNISSU") = 1     '協定週休日数
                            WW_HEADrow("KYOTEIWEEKNISSUTTL") = 1  '協定週休日数
                        End If
                    Case "06"
                        '○勤怠区分(6:協約外週休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("WEEKNISSU") = 1          '週休日数
                            WW_HEADrow("WEEKNISSUTTL") = 1       '週休日数
                        End If
                    Case "07"
                        '○勤怠区分(7:傷欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("SHOUKETUNISSU") = 1      '傷欠勤日数
                            WW_HEADrow("SHOUKETUNISSUTTL") = 1   '傷欠勤日数
                        End If
                    Case "08"
                        '○勤怠区分(8:組欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KUMIKETUNISSU") = 1      '組合欠勤日数
                            WW_HEADrow("KUMIKETUNISSUTTL") = 1   '組合欠勤日数
                        End If
                    Case "09"
                        '○勤怠区分(9:他欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("ETCKETUNISSU") = 1       'その他欠勤日数
                            WW_HEADrow("ETCKETUNISSUTTL") = 1    'その他欠勤日数
                        End If
                    Case "10"
                        '○勤怠区分(10:代休出勤) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        End If
                    Case "11"
                        '○勤怠区分(11:代休取得) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("DAIKYUNISSU") = 1        '代休取得日数
                            WW_HEADrow("DAIKYUNISSUTTL") = 1     '代休取得日数
                        End If
                    Case "12"
                        '○勤怠区分(12:年始出勤取得) …　出勤扱い(所労=1 )
                        WW_HEADrow("NENSHINISSU") = 1            '年始出勤日数
                        WW_HEADrow("NENSHINISSUTTL") = 1         '年始出勤日数
                End Select

                '************************************************************
                '*   宿日直設定                                             *
                '************************************************************
                Select Case WW_HEADrow("SHUKCHOKKBN")
                    Case "0"
                        '○宿日直区分(0:なし)
                        WW_HEADrow("SHUKCHOKNNISSU") = 0             '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNNISSUTTL") = 0          '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNISSU") = 0              '宿日直通常日数
                        WW_HEADrow("SHUKCHOKNISSUTTL") = 0           '宿日直通常日数
                        '2018/02/08 追加
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSU") = 0          '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0       '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKHLDNISSU") = 0           '宿直(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0        '宿直(翌日休み)
                        End If
                        '2018/02/08 追加


                    Case "1", "2"
                        '○宿日直区分(1:日直、2:宿直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 1    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 1     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1  '宿日直通常日数
                            End If
                        End If

                    Case "3"
                        '○宿日直区分(3:宿日直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 2    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 2 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 2     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 2  '宿日直通常日数
                            End If
                        End If

                    Case "4"
                        '○宿日直区分(4:宿直(翌日休み)／宿直(割増有り))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 1     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 1  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 0        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 0     '宿日直通常日数
                            End If
                        End If

                    Case "5"
                        '○宿日直区分(5:宿直(翌日営業)／宿直(割増無し))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 0     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 1        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1     '宿日直通常日数
                            End If
                        End If
                End Select



                '************************************************************
                '*   勤怠時間設定                                           *
                '************************************************************
                '   前提：出勤時刻は、当日0時から21時59分まで
                ' 　    ：退社時刻は、翌日5時まで
                '○退社日が出社当日～翌日 and 出社日時 < 退社日時 のみ時間計算を行う
                '以降処理で判定用(出社日時、退社日時)を算出
                Dim WW_STDATETIME As Date
                Dim WW_ENDDATETIME As Date

                '出社、退社が未入力の場合、残業計算しない
                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) And
                   IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                Else
                    Continue For
                End If

                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                '直前および、翌日の勤務状況取得
                Dim WW_YOKUHOLIDAYKBN As String = ""
                Dim WW_YOKUACTTIME As String = ""

                'If WW_HEADrow("STAFFKBN") Like "03*" Then

                Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))


                '翌日の勤務
                WW_YOKUHOLIDAYKBN = "0"
                Dim WW_YOKUDATE As String = dt.AddDays(1).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_YOKUDATE & "#"
                If iT0007view.Count > 0 Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "1" Then
                        WW_YOKUHOLIDAYKBN = "1"
                    End If

                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "2" Or
                       iT0007view.Item(0).Row("PAYKBN") = "01" Or
                       iT0007view.Item(0).Row("PAYKBN") = "02" Or
                       iT0007view.Item(0).Row("PAYKBN") = "04" Or
                       iT0007view.Item(0).Row("PAYKBN") = "05" Or
                       iT0007view.Item(0).Row("PAYKBN") = "06" Or
                       iT0007view.Item(0).Row("PAYKBN") = "07" Or
                       iT0007view.Item(0).Row("PAYKBN") = "08" Or
                       iT0007view.Item(0).Row("PAYKBN") = "09" Or
                       iT0007view.Item(0).Row("PAYKBN") = "11" Or
                       iT0007view.Item(0).Row("PAYKBN") = "13" Or
                       iT0007view.Item(0).Row("PAYKBN") = "15" Then
                        WW_YOKUHOLIDAYKBN = "2"
                    End If

                    If WW_HEADrow("HORG") <> CONST_HIGASHIKO Then
                        '************************************************************
                        '*   一般（新潟東港以外）                                   *
                        '************************************************************
                        If WW_YOKUHOLIDAYKBN = "1" Or WW_YOKUHOLIDAYKBN = "2" Then
                            If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                                '稼働あり
                                WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                            End If
                        End If
                    Else
                        '************************************************************
                        '*   新潟東港専用                                           *
                        '************************************************************
                        '稼働あり
                        If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                            '稼働あり
                            WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                        End If
                    End If
                Else
                    '翌日勤務未入力の場合、カレンダーより（救済）
                    MB005_Select(WW_HEADrow("CAMPCODE"), WW_YOKUDATE, WW_YOKUHOLIDAYKBN, WW_RTN)
                    If WW_RTN <> "00000" Then
                        'カレンダー取得できず（救済）
                        If Weekday(DateSerial(Year(CDate(WW_YOKUDATE)), Month(CDate(WW_YOKUDATE)), Day(CDate(WW_YOKUDATE)))) = 1 Then
                            '日曜日
                            WW_YOKUHOLIDAYKBN = 1
                        Else
                            '平日
                            WW_YOKUHOLIDAYKBN = 0
                        End If
                    End If
                    WW_YOKUACTTIME = ""
                End If
                'End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) Then
                    WW_STDATETIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                End If
                If IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                    WW_ENDDATETIME = CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME"))
                End If

                '○出社日時、退社日時の計算　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    '・出社を拘束開始とする(拘束時がZERO時、救済措置)
                    If WW_HEADrow("BINDSTDATE") = "" Then
                        If IsDate(WW_HEADrow("STTIME")) Then
                            WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                        Else
                            WW_HEADrow("BINDSTDATE") = "05:00"
                        End If
                    End If
                    '・拘束開始5時未満は5時とする
                    If IsDate(WW_HEADrow("BINDSTDATE")) Then
                        If WW_HEADrow("STDATE") < WW_HEADrow("WORKDATE") Then
                            WW_HEADrow("BINDSTDATE") = "05:00"
                        End If
                        If CDate(WW_HEADrow("BINDSTDATE")).ToString("HHmm") < "0500" Then
                            WW_HEADrow("BINDSTDATE") = "05:00"
                        End If
                    End If

                    '************************************************************
                    '*   新潟東港専用                                           *
                    '************************************************************
                    If WW_HEADrow("HORG") = CONST_HIGASHIKO Then
                        '日跨り（０：当日のみ、１：日跨り）
                        Dim WW_DAYS As Integer = DateDiff("d", CDate(WW_STDATETIME.ToString("yyyy/MM/dd")), CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd")))

                        '当日22:00～23:59の出社で翌日5:00以降の退社の場合、
                        '拘束開始＝出社開始とし通常通り残業計算する。但し、残業計算の結果を翌5:00出社として再設定する
                        If WW_DAYS = 1 And
                           CDate(WW_HEADrow("STTIME")).ToString("HH") > "21" And
                           CDate(WW_HEADrow("STTIME")).ToString("HH") < "24" And
                           CDate(WW_HEADrow("ENDTIME")).ToString("HH") > "04" Then
                            If IsDate(WW_HEADrow("STTIME")) Then
                                WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                            End If
                        End If
                    End If
                    '************************************************************
                End If

                '●時間算出（拘束開始日時、拘束終了日時）

                '○初期設定　★  共通処理(事務員+乗務員)　★
                Dim WW_BINDSTTIME As DateTime
                Dim WW_BINDENDTIME As DateTime
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & WW_HEADrow("BINDSTDATE"))
                    WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & CDate(WW_HEADrow("BINDSTDATE")).ToString("HH:mm"))
                End If

                '○拘束終了日時の設定　★  事務員処理　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '出社日時＋拘束時間(7:30)＋休憩(通常休憩)　…１時間取らないケース有。????再検討必要
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(HHMMtoMinutes(WW_HEADrow("BREAKTIME")))
                    '2018/02/06 追加
                End If

                '○拘束終了日時の設定　★  乗務員処理　★
                '   　　説明：拘束終了日時　…　実際の休憩を含む拘束終了時間（残業開始時間）
                '             拘束終了時間に休憩が含まれる場合、拘束終了時間を休憩分延長する
                Dim WW_BREAKTIMEZAN As Integer = 0
                Dim WW_MIN As Integer = 0
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then
                    Dim WW_BREAKTIMETTL As Integer = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    If WW_BREAKTIMETTL > 60 Then
                        WW_BREAKTIMEZAN = WW_BREAKTIMETTL - 60
                        WW_MIN = 60
                    Else
                        WW_BREAKTIMEZAN = 0
                        WW_MIN = WW_BREAKTIMETTL
                    End If
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(WW_MIN)
                End If

                '●時間算出（所定内通常分_作業、所定内深夜分_作業、所定内深夜分2_作業、所定外通常分_作業、所定外深夜分_作業、休日通常分_作業、休日深夜分_作業、休日深夜分2_作業）
                Dim WK_WORKTIME_SAGYO As Integer = 0         '平日＆所定内＆深夜以外（当日分）
                Dim WK_WORKTIME_SAGYO2 As Integer = 0        '平日＆所定内＆深夜以外（翌日分）
                Dim WK_NIGHTTIME_SAGYO As Integer = 0        '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_SAGYO As Integer = 0    '平日＆所定内＆深夜　　（24:00～29:00）
                Dim WK_YOKU0to5NIGHT_SAGYO2 As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_SAGYO As Integer = 0      '平日＆所定外＆深夜以外（当日分）
                Dim WK_OUTWORKTIME_SAGYO2 As Integer = 0     '平日＆所定外＆深夜以外（翌日分）
                Dim WK_OUTNIGHTTIME_SAGYO As Integer = 0     '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_SAGYO As Integer = 0        '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_SAGYO As Integer = 0       '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_SAGYO2 As Integer = 0      '休日＆残業　＆深夜     (24:00～29:00)

                '休憩時間
                Dim WK_WORKTIME_KYUKEI As Integer = 0        '平日＆所定内＆深夜以外
                Dim WK_NIGHTTIME_KYUKEI As Integer = 0       '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_KYUKEI As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_KYUKEI As Integer = 0     '平日＆所定外＆深夜以外
                Dim WK_OUTNIGHTTIME_KYUKEI As Integer = 0    '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_KYUKEI As Integer = 0       '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_KYUKEI As Integer = 0      '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_KYUKEI2 As Integer = 0     '休日＆残業　＆深夜     (24:00～29:00)

                Dim WW_累積分 As Integer = 0

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    ' In  : WW_STDATETIME             出社日時
                    '       WW_BINDSTTIME             拘束開始日時
                    '       WW_BINDENDTIME            拘束終了日時
                    '       0                         休日区分 = 0(固定)
                    '       WW_STDATETIME             出社日時
                    '       WW_ENDDATETIME            退社日時
                    ' Out : WK_WORKTIME_SAGYO         5:00～22:00（所定内通常）
                    '       WK_WORKTIME_SAGYO2        翌5:00～22:00（所定内通常）    
                    '       WK_NIGHTTIME_SAGYO        22:00～24:00（深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO    翌0:00～5:00（所定内深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO2   翌0:00～5:00（深夜）
                    '       WK_OUTWORKTIME_SAGYO      5:00～22:00（残業）　　← 法定、法定外休日のみ
                    '       WK_OUTWORKTIME_SAGYO2     翌5:00～22:00（残業）
                    '       WK_OUTNIGHTTIME_SAGYO     0:00～5:00（5時前深夜）
                    '       WK_HWORKTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO2,
                    '       WW_累積分
                    Call NightTimeMinuteGet(WW_STDATETIME,
                                            WW_BINDSTTIME,
                                            WW_BINDENDTIME,
                                            0,
                                            WW_STDATETIME,
                                            WW_ENDDATETIME,
                                            WK_WORKTIME_SAGYO,
                                            WK_WORKTIME_SAGYO2,
                                            WK_NIGHTTIME_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO2,
                                            WK_OUTWORKTIME_SAGYO,
                                            WK_OUTWORKTIME_SAGYO2,
                                            WK_OUTNIGHTTIME_SAGYO,
                                            WK_HWORKTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO2,
                                            WW_累積分)
                End If

                '○休憩時間計算　★  事務員処理　★
                If Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WK_WORKTIME_KYUKEI = HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                    '--------------------------------------------------------------------
                    '2018/02/06 追加
                End If

                '○休憩時間計算　★  乗務員処理　★
                If WW_HEADrow("STAFFKBN") Like "03*" Then
                    Dim WW_BREAKTIME As Integer = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    WK_WORKTIME_KYUKEI = WW_BREAKTIME
                    'If WW_HEADrow("HOLIDAYKBN") = 0 Then
                    '    WK_WORKTIME_KYUKEI = WW_BREAKTIME
                    'Else
                    '    WK_HWORKTIME_KYUKEI = WW_BREAKTIME
                    'End If
                    'Dim WW_MATCH As String = "OFF"
                    'For i As Integer = WW_IDX To WW_T0007BBtbl.Rows.Count - 1
                    '    Dim WW_BBrow As DataRow = WW_T0007BBtbl.Rows(i)
                    '    If WW_BBrow("STAFFCODE") = WW_HEADrow("STAFFCODE") And
                    '       WW_BBrow("WORKDATE") = WW_HEADrow("WORKDATE") Then
                    '        ' In  : WK_出社日時、WW_休憩開始日時、WW_休憩終了日時
                    '        ' Out : WK_WORKTIME_KYUKEI、WK_NIGHTTIME_KYUKEI、WK_YOKU0to5NIGHT_KYUKEI、WK_OUTWORKTIME_KYUKEI、WK_OUTNIGHTTIME_KYUKEI、
                    '        '       WK_HWORKTIME_KYUKEI、WK_HNIGHTTIME_KYUKEI、WK_HNIGHTTIME_KYUKEI2　←休日用（未使用：休日区分=0とするため）
                    '        ' 参照: WK_拘束開始日時 、WK_拘束終了日時
                    '        Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                    '        Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                    '        Call NightTimeMinuteGet(WW_STDATETIME, WW_BINDSTTIME, WW_BINDENDTIME, 0, WW_STBREAKTIME, WW_ENDBREAKTIME,
                    '                                WK_WORKTIME_KYUKEI, WK_NIGHTTIME_KYUKEI, WK_YOKU0to5NIGHT_KYUKEI, WK_OUTWORKTIME_KYUKEI, WK_OUTNIGHTTIME_KYUKEI,
                    '                                WK_HWORKTIME_KYUKEI, WK_HNIGHTTIME_KYUKEI, WK_HNIGHTTIME_KYUKEI2, WW_累積分)
                    '        WW_MATCH = "ON"
                    '    Else
                    '        If WW_MATCH = "ON" Then
                    '            WW_IDX = i
                    '            Exit For
                    '        End If
                    '    End If
                    'Next
                End If
                '************************************************************
                '*   残業設定                                               *
                '************************************************************
                '○異常事態の救済　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    '・拘束開始前に退社の場合
                    If DateDiff("n", WW_BINDSTTIME, WW_ENDDATETIME) < 0 Then
                        WW_BINDSTTIME = WW_STDATETIME
                    End If
                End If

                '○マイナス時間クリア
                Dim WW_ORVERTIME As Integer = 0   '平日残業時
                Dim WW_WNIGHTTIME As Integer = 0  '平日深夜時
                Dim WW_NIGHTTIME As Integer = 0   '所定内深夜時
                Dim WW_HWORKTIME As Integer = 0   '休日出勤時
                Dim WW_HNIGHTTIME As Integer = 0  '休日深夜時
                Dim WW_SWORKTIME As Integer = 0   '日曜出勤時
                Dim WW_SNIGHTTIME As Integer = 0  '日曜深夜時

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    If WW_HEADrow("HORG") <> CONST_HIGASHIKO Then
                        '************************************************************
                        '*   一般（新潟東港以外）                                   *
                        '************************************************************
                        Select Case WW_HEADrow("HOLIDAYKBN")
                            '○平日
                            Case "0"
                                '平日残業
                                If WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 平日残業時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_ORVERTIME = 0                                           ' 平日残業時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2) ' 休憩残算出
                                End If

                                '所定内深夜
                                WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO     ' 所定内深夜時

                                '翌日平日の場合
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '深夜時間(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                    End If
                                End If

                                '翌日日曜日の場合
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    If WW_YOKUACTTIME = "" Then
                                        If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                        End If

                                        '稼働なし、日曜深夜(24:00～翌5:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり、深夜時間(24:00～翌5:00)
                                        '深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                        End If
                                    End If

                                End If

                                '翌日法定外休日の場合
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    If WW_YOKUACTTIME = "" Then
                                        If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                        End If

                                        '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり 深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                        End If
                                    End If

                                End If


                            Case "1"
                                '○法定休日（日曜）出勤

                                If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_SWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 日曜出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_SWORKTIME = 0                                           ' 日曜出勤時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                                End If

                                If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 日曜深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_SNIGHTTIME = 0                                                                ' 日曜深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                                End If

                                '翌日平日の場合
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2   ' 休憩残算出
                                    End If

                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_ORVERTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 平日残業
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_ORVERTIME = 0                                              ' 平日残業
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                    End If

                                End If

                                '翌日法定外休日の場合
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                    End If

                                    If WW_YOKUACTTIME = "" Then
                                        '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり 日曜深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SNIGHTTIME = WW_SNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 日曜深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SNIGHTTIME = WW_SNIGHTTIME + 0                                          ' 日曜深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                        End If
                                    End If
                                End If

                            Case "2"
                                '○法定外休日（祝日、会社指定休日）
                                ' 休日出勤時
                                If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_HWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 休日出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HWORKTIME = 0                                           ' 休日出勤時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                                End If

                                ' 休日深夜時
                                If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_HNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 休日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HNIGHTTIME = 0                                                                ' 休日深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                                End If

                                '翌日平日の場合
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    ' 休日出勤時
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                    End If

                                    ' 休日深夜時
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                    End If
                                End If


                                '翌日日曜日の場合
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    If WW_YOKUACTTIME = "" Then
                                        '稼働なし、日曜深夜(24:00～翌5:00)
                                        ' 日曜出勤時
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 日曜出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SWORKTIME = 0                                            ' 日曜出勤時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2   ' 休憩残算出
                                        End If

                                        ' 日曜深夜時
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SNIGHTTIME = 0                                           ' 日曜深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり、休日深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        ' 休日出勤時
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                        End If

                                        ' 休日深夜時
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN    ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                            ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                  ' 休憩残算出
                                        End If
                                    End If
                                End If

                                '翌日法定外休日の場合
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    ' 休日出勤時
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = WW_HWORKTIME + 0                                            ' 休日出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2                  ' 休憩残算出
                                    End If

                                    ' 休日深夜時
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                    End If
                                End If

                        End Select
                    Else
                        '************************************************************
                        '*   新潟東港専用                                           *
                        '************************************************************
                        '日跨り（０：当日のみ、１：日跨り）
                        Dim WW_DAYS As Integer = DateDiff("d", CDate(WW_STDATETIME.ToString("yyyy/MM/dd")), CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd")))

                        '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                        '上で設定した拘束開始＝出社開始を翌5:00にここで置き換える（DB上は、5:00）
                        If WW_DAYS = 1 And
                           CDate(WW_HEADrow("STTIME")).ToString("HH") > "21" And
                           CDate(WW_HEADrow("STTIME")).ToString("HH") < "24" And
                           CDate(WW_HEADrow("ENDTIME")).ToString("HH") > "04" Then
                            WW_HEADrow("BINDSTDATE") = "05:00"
                        End If

                        '↓↓↓↓↓　旧システム（ACCESE）と同じロジック
                        Select Case WW_HEADrow("HOLIDAYKBN")
                            '○平日
                            Case "0"
                                '平日残業
                                WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2         ' 平日残業時

                                '所定内深夜
                                WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO          ' 所定内深夜時

                                ' 平日深夜時（＝平日深夜）
                                WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2     ' 平日深夜時

                                '---------------------------
                                '翌日日曜日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    '日跨り
                                    If WW_DAYS = 1 Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SNIGHTTIME = WW_ENDTIME                                                      ' 日曜深夜時
                                        If WW_SNIGHTTIME > WW_WNIGHTTIME Then
                                            WW_WNIGHTTIME = 0                                                           ' 平日深夜時
                                            WW_NIGHTTIME = WW_SNIGHTTIME                                                ' 所定内深夜時
                                        Else
                                            WW_WNIGHTTIME = WW_WNIGHTTIME - WW_SNIGHTTIME                               ' 平日深夜時
                                        End If
                                    End If

                                    '当日13時以降の出社で、退社が翌5時以降
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "12" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '拘束終了が当日22:00～23:59
                                        If WW_BINDENDTIME.ToString("HH") > "21" And WW_BINDENDTIME.ToString("HH") < "24" Then
                                            WW_NIGHTTIME = WK_NIGHTTIME_SAGYO                               ' 所定内深夜時
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO                           ' 平日深夜時
                                        Else
                                            WW_NIGHTTIME = 120                                              ' 所定内深夜時
                                        End If
                                        WW_SNIGHTTIME = 300                                                 ' 日曜深夜時
                                        WW_SWORKTIME = WW_ORVERTIME                                         ' 日曜出勤時
                                        WW_ORVERTIME = 0                                                    ' 平日残業
                                    End If
                                End If

                                '---------------------------
                                '翌日が休日or平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Or WW_YOKUHOLIDAYKBN = "0" Then
                                    '翌日稼働なし
                                    If WW_YOKUACTTIME = "" Then
                                        '日跨り
                                        If WW_DAYS = 1 Then
                                            If WW_ENDDATETIME.ToString("HH") > "05" Then
                                                '所定内深夜　－（5時間（0時～5時）－　所定外深夜）　
                                                WW_NIGHTTIME = WW_NIGHTTIME - (300 - WW_WNIGHTTIME)             ' 所定内深夜時
                                                WW_WNIGHTTIME = 300                                             ' 平日深夜時
                                            End If

                                            If WW_ENDDATETIME.ToString("HH") < "06" Then
                                                '深夜　＋　（所定内深夜　－　2時間（22時～0時））　
                                                WW_WNIGHTTIME = WW_WNIGHTTIME + (WW_NIGHTTIME - 120)            ' 平日深夜時
                                                WW_NIGHTTIME = 120                                              ' 所定内深夜時
                                            End If
                                        End If
                                    End If
                                End If

                                '---------------------------
                                '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                '---------------------------
                                If WW_DAYS = 1 And
                                   WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                    '所定内深夜を深夜に設定
                                    WW_WNIGHTTIME = WW_NIGHTTIME                                            ' 平日深夜時
                                    WW_NIGHTTIME = 0                                                        ' 所定内深夜時
                                End If

                                '---------------------------
                                '翌日が平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間　－　拘束時間（８時間）－1時間（休憩）－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        Dim WW_BINDTIME As Integer = DateDiff("n", WW_BINDSTTIME, WW_BINDENDTIME)
                                        WW_ORVERTIME = WW_ENDTIME - WW_BINDTIME - 60 - 300                  ' 平日残業時
                                    End If
                                End If

                                '---------------------------
                                '翌日が日曜の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間　－　休憩　－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                                ' 日曜出勤時
                                        '1440 = 0:00～24:00を意味する
                                        Dim WW_STTIME As Integer = DateDiff("n", CDate(WW_STDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_STDATETIME)
                                        WW_WNIGHTTIME = 1440 - WW_STTIME                                                    ' 平日深夜時
                                    End If

                                End If

                                '---------------------------
                                '翌日が休みの場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間　－　休憩　－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_HWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                                ' 休日出勤時
                                        WW_ORVERTIME = 0                                                                    ' 平日深夜時
                                    End If

                                End If

                            Case "1"
                                '○法定休日（日曜）出勤
                                ' 日曜出勤時
                                WW_SWORKTIME = WK_WORKTIME_SAGYO + WK_WORKTIME_SAGYO2 + WK_YOKU0to5NIGHT_SAGYO + WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2    ' 日曜出勤時
                                WW_SWORKTIME = WW_SWORKTIME - WK_WORKTIME_KYUKEI                                         ' 日曜出勤時

                                ' 日曜深夜時
                                WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2     ' 平日深夜時

                                '---------------------------
                                '翌日が平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日5時以降の出社で、退社が翌5時以前
                                    If WW_DAYS = 1 And WW_ENDDATETIME.ToString("HH") < "06" Then
                                        WW_WNIGHTTIME = WW_SNIGHTTIME - 120                                             ' 平日深夜時
                                        WW_SNIGHTTIME = 120                                                             ' 日曜深夜時
                                    End If
                                End If

                                '---------------------------
                                '翌日が休みの場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '当日5時以降の出社で、退社が翌5時以前
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "04" And WW_ENDDATETIME.ToString("HH") < "05" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_WNIGHTTIME = WW_ENDTIME                                                  ' 平日深夜時
                                        WW_SNIGHTTIME = WW_SNIGHTTIME - WW_WNIGHTTIME                               ' 日曜深夜時
                                    End If

                                End If

                                '---------------------------
                                '当日22時以前の出社で、退社が翌5時以降
                                '---------------------------
                                If WW_DAYS = 1 And
                                    WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                    WW_WNIGHTTIME = 300                                                             ' 平日深夜時
                                    WW_SNIGHTTIME = WW_SNIGHTTIME - WW_WNIGHTTIME                                   ' 日曜深夜時

                                    Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                    '翌日平日の場合
                                    If WW_YOKUHOLIDAYKBN = "0" Then
                                        WW_SWORKTIME = WW_SWORKTIME - (WW_ENDTIME - 300)                            ' 日曜出勤時
                                    End If
                                    'ACCESSの計算を修正 2018/6/13
                                    'WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                    WW_ORVERTIME = WW_ENDTIME - 60 - 300                                            ' 平日残業
                                End If

                                '---------------------------
                                '翌日平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        WW_WNIGHTTIME = 300                                                             ' 平日深夜時
                                        WW_SNIGHTTIME = WW_SNIGHTTIME - WW_WNIGHTTIME                                   ' 日曜深夜時
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_SWORKTIME - (WW_ENDTIME - 360)                                ' 日曜出勤時
                                        WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                    End If
                                End If

                                '---------------------------
                                '翌日が休みの場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_WNIGHTTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                           ' 平日深夜時
                                        WW_ORVERTIME = 0                                                                ' 平日残業
                                    End If

                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        If WK_WORKTIME_KYUKEI < 60 Then
                                            WW_HWORKTIME = WW_ENDTIME - 60 - 300                                        ' 休日出勤時
                                        Else
                                            WW_HWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                        ' 休日出勤時
                                        End If
                                        Dim WW_STTIME As Integer = DateDiff("n", CDate(WW_STDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_STDATETIME)
                                        '1320 = 0:00～22:00を意味する
                                        WW_SWORKTIME = 1320 - WW_STTIME                                                 ' 日曜出勤時
                                        WW_ORVERTIME = 0                                                                ' 平日残業
                                    End If
                                End If

                                '---------------------------
                                '翌日平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_STTIME As Integer = DateDiff("n", CDate(WW_STDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_STDATETIME)
                                        '1320 = 0:00～22:00を意味する
                                        WW_SWORKTIME = 1320 - WW_STTIME                                                 ' 日曜出勤時
                                    End If
                                End If

                            Case "2"
                                '○法定外休日（祝日、会社指定休日）
                                ' 休日出勤時
                                WW_HWORKTIME = WK_WORKTIME_SAGYO + WK_WORKTIME_SAGYO2 + WK_YOKU0to5NIGHT_SAGYO + WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2    ' 休日出勤時
                                WW_HWORKTIME = WW_HWORKTIME - WK_WORKTIME_KYUKEI                                         ' 休日出勤時

                                ' 平日深夜時（＝平日深夜）
                                WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2     ' 平日深夜時

                                '---------------------------
                                '翌日が日曜の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    '当日5時以降の出社で、退社が翌5時以前
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "04" And WW_ENDDATETIME.ToString("HH") < "05" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SNIGHTTIME = WW_ENDTIME                                                      ' 日曜深夜時
                                        WW_WNIGHTTIME = WW_WNIGHTTIME - WW_SNIGHTTIME                                   ' 平日深夜時
                                    End If
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する。
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                            ' 日曜出勤時
                                    End If

                                    '---------------------------
                                    'ACCESSの計算を修正（追加ロジック）　→　明休（日曜出勤したことと判断する）
                                    '当日22時以前の出社で、退社が翌5時以降
                                    '---------------------------
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        WW_WNIGHTTIME = 120                                                             ' 平日深夜時
                                        WW_SNIGHTTIME = 300                                                             ' 日曜深夜時

                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_ENDTIME - 300                                                 ' 日曜出勤時
                                        WW_HWORKTIME = WW_HWORKTIME - WW_SWORKTIME                                      ' 休日出勤時
                                    End If

                                End If

                                '---------------------------
                                '翌日平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する。
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間（0時～退社）－　7.5時間（拘束時間）－1時間（休憩）－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                        WW_HWORKTIME = 0                                                                ' 休日出勤時
                                    End If

                                    '出社が22時前で退社が翌5時以降
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_HWORKTIME = WW_HWORKTIME - (WW_ENDTIME - 360)                                ' 休日出勤時
                                        'ACCESSの計算を修正 2018/6/13
                                        'WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                        WW_ORVERTIME = WW_ENDTIME - 60 - 300                                            ' 平日残業
                                    End If
                                End If
                        End Select
                    End If

                End If

                '************************************************************
                '*   マイナス時間クリア                                     *
                '************************************************************


                '○マイナス時間クリア
                '平日残業時
                If WW_ORVERTIME < 0 Then
                    WW_HEADrow("ORVERTIME") = "00:00"
                Else
                    WW_HEADrow("ORVERTIME") = formatHHMM(WW_ORVERTIME)
                End If
                WW_HEADrow("ORVERTIMETTL") = formatHHMM(WW_ORVERTIME + HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO")))
                '平日深夜時
                If WW_WNIGHTTIME < 0 Then
                    WW_HEADrow("WNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("WNIGHTTIME") = formatHHMM(WW_WNIGHTTIME)
                End If
                WW_HEADrow("WNIGHTTIMETTL") = formatHHMM(WW_WNIGHTTIME + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO")))
                '所定内深夜時
                If WW_NIGHTTIME < 0 Then
                    WW_HEADrow("NIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("NIGHTTIME") = formatHHMM(WW_NIGHTTIME)
                End If
                WW_HEADrow("NIGHTTIMETTL") = formatHHMM(WW_NIGHTTIME + HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO")))
                '休日出勤時
                If WW_HWORKTIME < 0 Then
                    WW_HEADrow("HWORKTIME") = "00:00"
                Else
                    WW_HEADrow("HWORKTIME") = formatHHMM(WW_HWORKTIME)
                End If
                WW_HEADrow("HWORKTIMETTL") = formatHHMM(WW_HWORKTIME + HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO")))
                '休日深夜時
                If WW_HNIGHTTIME < 0 Then
                    WW_HEADrow("HNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("HNIGHTTIME") = formatHHMM(WW_HNIGHTTIME)
                End If
                WW_HEADrow("HNIGHTTIMETTL") = formatHHMM(WW_HNIGHTTIME + HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO")))
                '日曜出勤時
                If WW_SWORKTIME < 0 Then
                    WW_HEADrow("SWORKTIME") = "00:00"
                Else
                    WW_HEADrow("SWORKTIME") = formatHHMM(WW_SWORKTIME)
                End If
                WW_HEADrow("SWORKTIMETTL") = formatHHMM(WW_SWORKTIME + HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO")))
                '日曜深夜時
                If WW_SNIGHTTIME < 0 Then
                    WW_HEADrow("SNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("SNIGHTTIME") = formatHHMM(WW_SNIGHTTIME)
                End If
                WW_HEADrow("SNIGHTTIMETTL") = formatHHMM(WW_SNIGHTTIME + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO")))

                '早出補填（2019/09以前は計算しないため）
                WW_HEADrow("HAYADETIME") = "00:00"
                WW_HEADrow("HAYADETIMETTL") = "00:00"

                'WW_HEADrow("STATUS") = ""
                WW_HEADrow("OPERATION") = "更新"

            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing
            WW_T0007BBtbl.Dispose()
            WW_T0007BBtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_KintaiCalc_OLD"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = "A"                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = "00001"
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  残業計算（ＥＮＥＸ専用）
    Public Sub T0007_KintaiCalc(ByRef ioTbl As DataTable, ByRef iTbl As DataTable, Optional ByVal hydFlg As Boolean = False)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_IDX2 As Integer = 0
        Dim WW_IDX3 As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""


        Try

            '削除レコードを取得
            Dim WW_T0007DELtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DELtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl = CS0026TblSort.sort()

            '勤怠の明細レコードを取得
            Dim WW_T0007DTLtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '休憩レコードを取得
            Dim WW_T0007BBtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and WORKKBN = 'BB' "
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007BBtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl2 As DataTable = New DataTable
            CS0026TblSort.TABLE = iTbl
            CS0026TblSort.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and DELFLG = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl2 = CS0026TblSort.sort()

            '直前、翌日取得用VIEW
            Dim iT0007view As DataView
            iT0007view = New DataView(WW_T0007HEADtbl2)
            iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

            WW_IDX = 0
            For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
                'STATUS<>''（勤怠に変更が発生しているレコード）
                If WW_HEADrow("RECODEKBN") = "0" Then
                Else
                    Continue For
                End If

                '特殊計算部署取得
                Dim specialOrg As ListBox = getList(WW_HEADrow("CAMPCODE"), GRT00007WRKINC.CONST_SPEC)

                '************************************************************
                '*   勤怠日数設定                                           *
                '************************************************************
                NissuItem_Init(WW_HEADrow)
                Select Case WW_HEADrow("PAYKBN")
                    Case "00"
                        '○勤怠区分(00:通常) …　出勤扱い(所労=1 )
                        If WW_HEADrow("HOLIDAYKBN") = "0" Then
                        End If
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                '2018/02/01 追加
                                If WW_HEADrow("STTIME") = "00:00" And WW_HEADrow("ENDTIME") = "00:00" Then
                                Else
                                    WW_HEADrow("NENSHINISSU") = 1    '年始出勤日数
                                    WW_HEADrow("NENSHINISSUTTL") = 1 '年始出勤日数
                                End If
                                '2018/02/01 追加
                            End If
                        End If
                    Case "01"
                        '○勤怠区分(01:年休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("NENKYUNISSU") = 1        '年次有給休暇
                            WW_HEADrow("NENKYUNISSUTTL") = 1     '年次有給休暇
                        End If
                    Case "02"
                        '○勤怠区分(2:特休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("TOKUKYUNISSU") = 1       '特別有給休暇
                            WW_HEADrow("TOKUKYUNISSUTTL") = 1    '特別有給休暇
                        End If
                    Case "03"
                        '○勤怠区分(3:遅刻早退) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("CHIKOKSOTAINISSU") = 1    '遅刻早退日数
                            WW_HEADrow("CHIKOKSOTAINISSUTTL") = 1 '遅刻早退日数
                        End If
                    Case "04"
                        '○勤怠区分(4:ｽﾄｯｸ休暇) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("STOCKNISSU") = 1         'ストック休暇日数
                            WW_HEADrow("STOCKNISSUTTL") = 1      'ストック休暇日数
                        End If
                    Case "05"
                        '○勤怠区分(5:協約週休) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KYOTEIWEEKNISSU") = 1     '協定週休日数
                            WW_HEADrow("KYOTEIWEEKNISSUTTL") = 1  '協定週休日数
                        End If
                    Case "06"
                        '○勤怠区分(6:協約外週休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("WEEKNISSU") = 1          '週休日数
                            WW_HEADrow("WEEKNISSUTTL") = 1       '週休日数
                        End If
                    Case "07"
                        '○勤怠区分(7:傷欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("SHOUKETUNISSU") = 1      '傷欠勤日数
                            WW_HEADrow("SHOUKETUNISSUTTL") = 1   '傷欠勤日数
                        End If
                    Case "08"
                        '○勤怠区分(8:組欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KUMIKETUNISSU") = 1      '組合欠勤日数
                            WW_HEADrow("KUMIKETUNISSUTTL") = 1   '組合欠勤日数
                        End If
                    Case "09"
                        '○勤怠区分(9:他欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("ETCKETUNISSU") = 1       'その他欠勤日数
                            WW_HEADrow("ETCKETUNISSUTTL") = 1    'その他欠勤日数
                        End If
                    Case "10"
                        '○勤怠区分(10:代休出勤) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        End If
                    Case "11"
                        '○勤怠区分(11:代休取得) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("DAIKYUNISSU") = 1        '代休取得日数
                            WW_HEADrow("DAIKYUNISSUTTL") = 1     '代休取得日数
                        End If
                    Case "12"
                        '○勤怠区分(12:年始出勤取得) …　出勤扱い(所労=1 )
                        WW_HEADrow("NENSHINISSU") = 1            '年始出勤日数
                        WW_HEADrow("NENSHINISSUTTL") = 1         '年始出勤日数
                End Select

                '************************************************************
                '*   宿日直設定                                             *
                '************************************************************
                Select Case WW_HEADrow("SHUKCHOKKBN")
                    Case "0"
                        '○宿日直区分(0:なし)
                        WW_HEADrow("SHUKCHOKNNISSU") = 0             '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNNISSUTTL") = 0          '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNISSU") = 0              '宿日直通常日数
                        WW_HEADrow("SHUKCHOKNISSUTTL") = 0           '宿日直通常日数
                        '2018/02/08 追加
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSU") = 0          '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0       '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKHLDNISSU") = 0           '宿直(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0        '宿直(翌日休み)
                        End If
                        '2018/02/08 追加


                    Case "1", "2"
                        '○宿日直区分(1:日直、2:宿直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 1    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 1     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1  '宿日直通常日数
                            End If
                        End If

                    Case "3"
                        '○宿日直区分(3:宿日直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 2    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 2 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 2     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 2  '宿日直通常日数
                            End If
                        End If

                    Case "4"
                        '○宿日直区分(4:宿直(翌日休み)／宿直(割増有り))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 1     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 1  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 0        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 0     '宿日直通常日数
                            End If
                        End If

                    Case "5"
                        '○宿日直区分(5:宿直(翌日営業)／宿直(割増無し))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 0     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 1        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1     '宿日直通常日数
                            End If
                        End If
                End Select



                '************************************************************
                '*   勤怠時間設定                                           *
                '************************************************************
                '   前提：出勤時刻は、当日0時から21時59分まで
                ' 　    ：退社時刻は、翌日5時まで
                '○退社日が出社当日～翌日 and 出社日時 < 退社日時 のみ時間計算を行う
                '以降処理で判定用(出社日時、退社日時)を算出
                Dim WW_STDATETIME As Date
                Dim WW_ENDDATETIME As Date

                '出社、退社が未入力の場合、残業計算しない
                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) And
                   IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                Else
                    Continue For
                End If

                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                '直前および、翌日の勤務状況取得
                Dim WW_YOKUHOLIDAYKBN As String = ""
                Dim WW_YOKUACTTIME As String = ""

                'If WW_HEADrow("STAFFKBN") Like "03*" Then

                Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))


                '翌日の勤務
                WW_YOKUHOLIDAYKBN = "0"
                Dim WW_YOKUDATE As String = dt.AddDays(1).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_YOKUDATE & "#"
                If iT0007view.Count > 0 Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "1" Then
                        WW_YOKUHOLIDAYKBN = "1"
                    End If

                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "2" Or
                       iT0007view.Item(0).Row("PAYKBN") = "01" Or
                       iT0007view.Item(0).Row("PAYKBN") = "02" Or
                       iT0007view.Item(0).Row("PAYKBN") = "04" Or
                       iT0007view.Item(0).Row("PAYKBN") = "05" Or
                       iT0007view.Item(0).Row("PAYKBN") = "06" Or
                       iT0007view.Item(0).Row("PAYKBN") = "07" Or
                       iT0007view.Item(0).Row("PAYKBN") = "08" Or
                       iT0007view.Item(0).Row("PAYKBN") = "09" Or
                       iT0007view.Item(0).Row("PAYKBN") = "11" Or
                       iT0007view.Item(0).Row("PAYKBN") = "13" Or
                       iT0007view.Item(0).Row("PAYKBN") = "15" Then
                        WW_YOKUHOLIDAYKBN = "2"
                    End If

                    If IsNothing(specialOrg.Items.FindByValue(WW_HEADrow("HORG"))) Then

                        '************************************************************
                        '*   一般（新潟東港以外）                                   *
                        '************************************************************
                        If WW_YOKUHOLIDAYKBN = "1" Or WW_YOKUHOLIDAYKBN = "2" Then
                            If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                                '稼働あり
                                WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                            End If
                        End If
                    Else
                        '************************************************************
                        '*   新潟東港専用                                           *
                        '************************************************************
                        '稼働あり
                        If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                            '稼働あり
                            WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                        End If
                    End If
                Else
                    '翌日勤務未入力の場合、カレンダーより（救済）
                    MB005_Select(WW_HEADrow("CAMPCODE"), WW_YOKUDATE, WW_YOKUHOLIDAYKBN, WW_RTN)
                    If WW_RTN <> C_MESSAGE_NO.NORMAL Then
                        'カレンダー取得できず（救済）
                        If Weekday(DateSerial(Year(CDate(WW_YOKUDATE)), Month(CDate(WW_YOKUDATE)), Day(CDate(WW_YOKUDATE)))) = 1 Then
                            '日曜日
                            WW_YOKUHOLIDAYKBN = 1
                        Else
                            '平日
                            WW_YOKUHOLIDAYKBN = 0
                        End If
                    End If
                    WW_YOKUACTTIME = ""
                End If
                'End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) Then
                    WW_STDATETIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                End If
                If IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                    WW_ENDDATETIME = CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME"))
                End If

                '○出社日時、退社日時の計算　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    If IsNothing(specialOrg.Items.FindByValue(WW_HEADrow("HORG"))) Then

                        '拘束開始＝出社
                        If IsDate(WW_HEADrow("STTIME")) Then
                            WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                        Else
                            WW_HEADrow("BINDSTDATE") = "00:00"
                        End If
                        ''・拘束開始0時未満は0時とする
                        If IsDate(WW_HEADrow("BINDSTDATE")) Then
                            If WW_HEADrow("STDATE") < WW_HEADrow("WORKDATE") Then
                                WW_HEADrow("BINDSTDATE") = "00:00"
                            End If
                        End If

                    Else
                        '************************************************************
                        '*   新潟東港専用                                           *
                        '************************************************************
                        '・出社を拘束開始とする(拘束時がZERO時、救済措置)
                        If WW_HEADrow("BINDSTDATE") = "" Then
                            If IsDate(WW_HEADrow("STTIME")) Then
                                WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                            Else
                                WW_HEADrow("BINDSTDATE") = "00:00"
                            End If
                        End If
                        '・拘束開始5時未満は5時とする
                        If IsDate(WW_HEADrow("BINDSTDATE")) Then
                            If WW_HEADrow("STDATE") < WW_HEADrow("WORKDATE") Then
                                WW_HEADrow("BINDSTDATE") = "05:00"
                            End If
                            If CDate(WW_HEADrow("BINDSTDATE")).ToString("HHmm") < "0500" Then
                                WW_HEADrow("BINDSTDATE") = "05:00"
                            End If
                        End If

                        '日跨り（０：当日のみ、１：日跨り）
                        Dim WW_DAYS As Integer = DateDiff("d", CDate(WW_STDATETIME.ToString("yyyy/MM/dd")), CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd")))

                        '当日22:00～23:59の出社で翌日5:00以降の退社の場合、
                        '拘束開始＝出社開始とし通常通り残業計算する。但し、残業計算の結果を翌5:00出社として再設定する
                        If WW_DAYS = 1 And
                                           CDate(WW_HEADrow("STTIME")).ToString("HH") > "21" And
                                           CDate(WW_HEADrow("STTIME")).ToString("HH") < "24" And
                                           CDate(WW_HEADrow("ENDTIME")).ToString("HH") > "04" Then
                            If IsDate(WW_HEADrow("STTIME")) Then
                                WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                            End If
                        End If
                    End If
                    '************************************************************
                End If

                '●時間算出（拘束開始日時、拘束終了日時）

                '○初期設定　★  共通処理(事務員+乗務員)　★
                Dim WW_BINDSTTIME As DateTime
                Dim WW_BINDENDTIME As DateTime
                '************************************************************
                '*   新潟東港以外                                           *
                '************************************************************
                If IsNothing(specialOrg.Items.FindByValue(WW_HEADrow("HORG"))) Then
                    If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                        Select Case WW_HEADrow("HOLIDAYKBN")
                            Case "0"
                                WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & WW_HEADrow("BINDSTDATE"))
                                WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & CDate(WW_HEADrow("BINDSTDATE")).ToString("HH:mm"))
                            Case "1", "2"
                                '拘束開始＝日曜、祝日の場合は今まで通り5:00を起点に計算とする
                                If IsDate(WW_HEADrow("BINDSTDATE")) Then
                                    If WW_HEADrow("STDATE") < WW_HEADrow("WORKDATE") Then
                                        WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & "05:00")
                                        WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & WW_BINDSTTIME.ToString("HH:mm"))
                                    End If
                                    If CDate(WW_HEADrow("BINDSTDATE")).ToString("HHmm") < "0500" Then
                                        WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & "05:00")
                                        WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & WW_BINDSTTIME.ToString("HH:mm"))
                                    End If
                                End If
                        End Select
                    End If
                Else
                    '************************************************************
                    '*   新潟東港専用                                           *
                    '************************************************************
                    If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                        WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & WW_HEADrow("BINDSTDATE"))
                        WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & CDate(WW_HEADrow("BINDSTDATE")).ToString("HH:mm"))
                    End If
                End If

                '○拘束終了日時の設定　★  事務員処理　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '出社日時＋拘束時間(7:30)＋休憩(通常休憩)　…１時間取らないケース有。????再検討必要
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(HHMMtoMinutes(WW_HEADrow("BREAKTIME")))
                    '2018/02/06 追加
                End If

                '○拘束終了日時の設定　★  乗務員処理　★
                '   　　説明：拘束終了日時　…　実際の休憩を含む拘束終了時間（残業開始時間）
                '             拘束終了時間に休憩が含まれる場合、拘束終了時間を休憩分延長する
                Dim WW_BREAKTIMEZAN As Integer = 0
                Dim WW_MIN As Integer = 0
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then
                    Dim WW_BREAKTIMETTL As Integer = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    If WW_BREAKTIMETTL > 60 Then
                        WW_BREAKTIMEZAN = WW_BREAKTIMETTL - 60
                        WW_MIN = 60
                    Else
                        WW_BREAKTIMEZAN = 0
                        WW_MIN = WW_BREAKTIMETTL
                    End If
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(WW_MIN)
                End If

                '●時間算出（所定内通常分_作業、所定内深夜分_作業、所定内深夜分2_作業、所定外通常分_作業、所定外深夜分_作業、休日通常分_作業、休日深夜分_作業、休日深夜分2_作業）
                Dim WK_WORKTIME_SAGYO As Integer = 0         '平日＆所定内＆深夜以外（当日分）
                Dim WK_WORKTIME_SAGYO2 As Integer = 0        '平日＆所定内＆深夜以外（翌日分）
                Dim WK_NIGHTTIME_SAGYO As Integer = 0        '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_SAGYO As Integer = 0    '平日＆所定内＆深夜　　（24:00～29:00）
                Dim WK_YOKU0to5NIGHT_SAGYO2 As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_SAGYO As Integer = 0      '平日＆所定外＆深夜以外（当日分）
                Dim WK_OUTWORKTIME_SAGYO2 As Integer = 0     '平日＆所定外＆深夜以外（翌日分）
                Dim WK_OUTNIGHTTIME_SAGYO As Integer = 0     '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_SAGYO As Integer = 0        '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_SAGYO As Integer = 0       '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_SAGYO2 As Integer = 0      '休日＆残業　＆深夜     (24:00～29:00)

                '休憩時間
                Dim WK_WORKTIME_KYUKEI As Integer = 0        '平日＆所定内＆深夜以外
                Dim WK_NIGHTTIME_KYUKEI As Integer = 0       '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_KYUKEI As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_KYUKEI As Integer = 0     '平日＆所定外＆深夜以外
                Dim WK_OUTNIGHTTIME_KYUKEI As Integer = 0    '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_KYUKEI As Integer = 0       '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_KYUKEI As Integer = 0      '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_KYUKEI2 As Integer = 0     '休日＆残業　＆深夜     (24:00～29:00)

                Dim WW_累積分 As Integer = 0
                Dim WW_累積分_JISA As Integer = 0

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    ' In  : WW_STDATETIME             出社日時
                    '       WW_BINDSTTIME             拘束開始日時
                    '       WW_BINDENDTIME            拘束終了日時
                    '       0                         休日区分 = 0(固定)
                    '       WW_STDATETIME             出社日時
                    '       WW_ENDDATETIME            退社日時
                    ' Out : WK_WORKTIME_SAGYO         5:00～22:00（所定内通常）
                    '       WK_WORKTIME_SAGYO2        翌5:00～22:00（所定内通常）    
                    '       WK_NIGHTTIME_SAGYO        22:00～24:00（深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO    翌0:00～5:00（所定内深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO2   翌0:00～5:00（深夜）
                    '       WK_OUTWORKTIME_SAGYO      5:00～22:00（残業）　　← 法定、法定外休日のみ
                    '       WK_OUTWORKTIME_SAGYO2     翌5:00～22:00（残業）
                    '       WK_OUTNIGHTTIME_SAGYO     0:00～5:00（5時前深夜）
                    '       WK_HWORKTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO2,
                    '       WW_累積分
                    Call NightTimeMinuteGet(WW_STDATETIME,
                                            WW_BINDSTTIME,
                                            WW_BINDENDTIME,
                                            0,
                                            WW_STDATETIME,
                                            WW_ENDDATETIME,
                                            WK_WORKTIME_SAGYO,
                                            WK_WORKTIME_SAGYO2,
                                            WK_NIGHTTIME_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO2,
                                            WK_OUTWORKTIME_SAGYO,
                                            WK_OUTWORKTIME_SAGYO2,
                                            WK_OUTNIGHTTIME_SAGYO,
                                            WK_HWORKTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO2,
                                            WW_累積分)
                End If

                '○休憩時間計算　★  事務員処理　★
                If Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WK_WORKTIME_KYUKEI = HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                    '--------------------------------------------------------------------
                    '2018/02/06 追加
                End If

                '○休憩時間計算　★  乗務員処理　★
                If WW_HEADrow("STAFFKBN") Like "03*" Then
                    Dim WW_BREAKTIME As Integer = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    WK_WORKTIME_KYUKEI = WW_BREAKTIME
                    'If WW_HEADrow("HOLIDAYKBN") = 0 Then
                    '    WK_WORKTIME_KYUKEI = WW_BREAKTIME
                    'Else
                    '    WK_HWORKTIME_KYUKEI = WW_BREAKTIME
                    'End If
                    'Dim WW_MATCH As String = "OFF"
                    'For i As Integer = WW_IDX To WW_T0007BBtbl.Rows.Count - 1
                    '    Dim WW_BBrow As DataRow = WW_T0007BBtbl.Rows(i)
                    '    If WW_BBrow("STAFFCODE") = WW_HEADrow("STAFFCODE") And
                    '       WW_BBrow("WORKDATE") = WW_HEADrow("WORKDATE") Then
                    '        ' In  : WK_出社日時、WW_休憩開始日時、WW_休憩終了日時
                    '        ' Out : WK_WORKTIME_KYUKEI、WK_NIGHTTIME_KYUKEI、WK_YOKU0to5NIGHT_KYUKEI、WK_OUTWORKTIME_KYUKEI、WK_OUTNIGHTTIME_KYUKEI、
                    '        '       WK_HWORKTIME_KYUKEI、WK_HNIGHTTIME_KYUKEI、WK_HNIGHTTIME_KYUKEI2　←休日用（未使用：休日区分=0とするため）
                    '        ' 参照: WK_拘束開始日時 、WK_拘束終了日時
                    '        Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                    '        Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                    '        Call NightTimeMinuteGet(WW_STDATETIME, WW_BINDSTTIME, WW_BINDENDTIME, 0, WW_STBREAKTIME, WW_ENDBREAKTIME,
                    '                                WK_WORKTIME_KYUKEI, WK_NIGHTTIME_KYUKEI, WK_YOKU0to5NIGHT_KYUKEI, WK_OUTWORKTIME_KYUKEI, WK_OUTNIGHTTIME_KYUKEI,
                    '                                WK_HWORKTIME_KYUKEI, WK_HNIGHTTIME_KYUKEI, WK_HNIGHTTIME_KYUKEI2, WW_累積分)
                    '        WW_MATCH = "ON"
                    '    Else
                    '        If WW_MATCH = "ON" Then
                    '            WW_IDX = i
                    '            Exit For
                    '        End If
                    '    End If
                    'Next
                End If
                '************************************************************
                '*   残業設定                                               *
                '************************************************************
                '○異常事態の救済　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    '・拘束開始前に退社の場合
                    If DateDiff("n", WW_BINDSTTIME, WW_ENDDATETIME) < 0 Then
                        WW_BINDSTTIME = WW_STDATETIME
                    End If
                End If

                '○マイナス時間クリア
                Dim WW_ORVERTIME As Integer = 0   '平日残業時
                Dim WW_WNIGHTTIME As Integer = 0  '平日深夜時
                Dim WW_NIGHTTIME As Integer = 0   '所定内深夜時
                Dim WW_HWORKTIME As Integer = 0   '休日出勤時
                Dim WW_HNIGHTTIME As Integer = 0  '休日深夜時
                Dim WW_SWORKTIME As Integer = 0   '日曜出勤時
                Dim WW_SNIGHTTIME As Integer = 0  '日曜深夜時
                Dim WW_HAYADETIME As Integer = 0  '早出補填時間

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    If IsNothing(specialOrg.Items.FindByValue(WW_HEADrow("HORG"))) Then
                        '************************************************************
                        '*   一般（新潟東港以外）                                   *
                        '************************************************************
                        Select Case WW_HEADrow("HOLIDAYKBN")
                            '○平日
                            Case "0"
                                '平日残業
                                If WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 平日残業時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_ORVERTIME = 0                                           ' 平日残業時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2) ' 休憩残算出
                                End If

                                '所定内深夜
                                WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO     ' 所定内深夜時

                                '翌日平日の場合
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '深夜時間(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                    End If
                                End If

                                '翌日日曜日の場合
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    If WW_YOKUACTTIME = "" Then
                                        If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                        End If

                                        '稼働なし、日曜深夜(24:00～翌5:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり、深夜時間(24:00～翌5:00)
                                        '深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                        End If
                                    End If

                                End If

                                '翌日法定外休日の場合
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    If WW_YOKUACTTIME = "" Then
                                        If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                        End If

                                        '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり 深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                        End If
                                    End If

                                End If


                            Case "1"
                                '○法定休日（日曜）出勤

                                If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_SWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 日曜出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_SWORKTIME = 0                                           ' 日曜出勤時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                                End If

                                If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 日曜深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_SNIGHTTIME = 0                                                                ' 日曜深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                                End If

                                '翌日平日の場合
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2   ' 休憩残算出
                                    End If

                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_ORVERTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 平日残業
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_ORVERTIME = 0                                              ' 平日残業
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                    End If

                                End If

                                '翌日法定外休日の場合
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                    End If

                                    If WW_YOKUACTTIME = "" Then
                                        '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり 日曜深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SNIGHTTIME = WW_SNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 日曜深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SNIGHTTIME = WW_SNIGHTTIME + 0                                          ' 日曜深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                        End If
                                    End If
                                End If

                            Case "2"
                                '○法定外休日（祝日、会社指定休日）
                                ' 休日出勤時
                                If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_HWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 休日出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HWORKTIME = 0                                           ' 休日出勤時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                                End If

                                ' 休日深夜時
                                If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                    WW_HNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 休日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HNIGHTTIME = 0                                                                ' 休日深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                                End If

                                '翌日平日の場合
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    ' 休日出勤時
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                    End If

                                    ' 休日深夜時
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                    End If
                                End If


                                '翌日日曜日の場合
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    If WW_YOKUACTTIME = "" Then
                                        '稼働なし、日曜深夜(24:00～翌5:00)
                                        ' 日曜出勤時
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 日曜出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SWORKTIME = 0                                            ' 日曜出勤時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2   ' 休憩残算出
                                        End If

                                        ' 日曜深夜時
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_SNIGHTTIME = 0                                           ' 日曜深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                        End If
                                    Else
                                        '稼働あり、休日深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                        ' 休日出勤時
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                        End If

                                        ' 休日深夜時
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN    ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                            ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                  ' 休憩残算出
                                        End If
                                    End If
                                End If

                                '翌日法定外休日の場合
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    ' 休日出勤時
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = WW_HWORKTIME + 0                                            ' 休日出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2                  ' 休憩残算出
                                    End If

                                    ' 休日深夜時
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                    End If
                                End If

                        End Select

                        '早出補填計算
                        Dim WW_ZANTTL As Integer = WW_ORVERTIME + WW_WNIGHTTIME + WW_HWORKTIME + WW_HNIGHTTIME + WW_SWORKTIME + WW_SNIGHTTIME
                        Dim WW_STDATETIME0000 As Date = CDate(WW_HEADrow("WORKDATE") & " " & "00:00")
                        Dim WW_ENDDATETIME0500 As Date = CDate(WW_HEADrow("WORKDATE") & " " & "05:00")

                        If WW_BINDSTTIME >= WW_STDATETIME0000 And
                           WW_BINDSTTIME <= WW_ENDDATETIME0500 Then
                            WW_HAYADETIME = DateDiff("n", WW_BINDSTTIME, WW_ENDDATETIME0500)
                            If WW_HAYADETIME > WW_ZANTTL Then
                                WW_HAYADETIME = WW_HAYADETIME - WW_ZANTTL
                            Else
                                WW_HAYADETIME = 0
                            End If
                        End If
                    Else
                        '************************************************************
                        '*   新潟東港専用                                           *
                        '************************************************************
                        '日跨り（０：当日のみ、１：日跨り）
                        Dim WW_DAYS As Integer = DateDiff("d", CDate(WW_STDATETIME.ToString("yyyy/MM/dd")), CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd")))

                        '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                        '上で設定した拘束開始＝出社開始を翌5:00にここで置き換える（DB上は、5:00）
                        If WW_DAYS = 1 And
                           CDate(WW_HEADrow("STTIME")).ToString("HH") > "21" And
                           CDate(WW_HEADrow("STTIME")).ToString("HH") < "24" And
                           CDate(WW_HEADrow("ENDTIME")).ToString("HH") > "04" Then
                            WW_HEADrow("BINDSTDATE") = "05:00"
                        End If

                        '↓↓↓↓↓　旧システム（ACCESE）と同じロジック
                        Select Case WW_HEADrow("HOLIDAYKBN")
                            '○平日
                            Case "0"
                                '平日残業
                                WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2         ' 平日残業時

                                '所定内深夜
                                WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO          ' 所定内深夜時

                                ' 平日深夜時（＝平日深夜）
                                WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2     ' 平日深夜時

                                '---------------------------
                                '翌日日曜日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    '日跨り
                                    If WW_DAYS = 1 Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SNIGHTTIME = WW_ENDTIME                                                      ' 日曜深夜時
                                        If WW_SNIGHTTIME > WW_WNIGHTTIME Then
                                            WW_WNIGHTTIME = 0                                                           ' 平日深夜時
                                            WW_NIGHTTIME = WW_SNIGHTTIME                                                ' 所定内深夜時
                                        Else
                                            WW_WNIGHTTIME = WW_WNIGHTTIME - WW_SNIGHTTIME                               ' 平日深夜時
                                        End If
                                    End If

                                    '当日13時以降の出社で、退社が翌5時以降
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "12" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '拘束終了が当日22:00～23:59
                                        If WW_BINDENDTIME.ToString("HH") > "21" And WW_BINDENDTIME.ToString("HH") < "24" Then
                                            WW_NIGHTTIME = WK_NIGHTTIME_SAGYO                               ' 所定内深夜時
                                            WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO                           ' 平日深夜時
                                        Else
                                            WW_NIGHTTIME = 120                                              ' 所定内深夜時
                                        End If
                                        WW_SNIGHTTIME = 300                                                 ' 日曜深夜時
                                        WW_SWORKTIME = WW_ORVERTIME                                         ' 日曜出勤時
                                        WW_ORVERTIME = 0                                                    ' 平日残業
                                    End If
                                End If

                                '---------------------------
                                '翌日が休日or平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Or WW_YOKUHOLIDAYKBN = "0" Then
                                    '翌日稼働なし
                                    If WW_YOKUACTTIME = "" Then
                                        '日跨り
                                        If WW_DAYS = 1 Then
                                            If WW_ENDDATETIME.ToString("HH") > "05" Then
                                                '所定内深夜　－（5時間（0時～5時）－　所定外深夜）　
                                                WW_NIGHTTIME = WW_NIGHTTIME - (300 - WW_WNIGHTTIME)             ' 所定内深夜時
                                                WW_WNIGHTTIME = 300                                             ' 平日深夜時
                                            End If

                                            If WW_ENDDATETIME.ToString("HH") < "06" Then
                                                '深夜　＋　（所定内深夜　－　2時間（22時～0時））　
                                                WW_WNIGHTTIME = WW_WNIGHTTIME + (WW_NIGHTTIME - 120)            ' 平日深夜時
                                                WW_NIGHTTIME = 120                                              ' 所定内深夜時
                                            End If
                                        End If
                                    End If
                                End If

                                '---------------------------
                                '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                '---------------------------
                                If WW_DAYS = 1 And
                                   WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                    '所定内深夜を深夜に設定
                                    WW_WNIGHTTIME = WW_NIGHTTIME                                            ' 平日深夜時
                                    WW_NIGHTTIME = 0                                                        ' 所定内深夜時
                                End If

                                '---------------------------
                                '翌日が平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間　－　拘束時間（８時間）－1時間（休憩）－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        Dim WW_BINDTIME As Integer = DateDiff("n", WW_BINDSTTIME, WW_BINDENDTIME)
                                        WW_ORVERTIME = WW_ENDTIME - WW_BINDTIME - 60 - 300                  ' 平日残業時
                                    End If
                                End If

                                '---------------------------
                                '翌日が日曜の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間　－　休憩　－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                                ' 日曜出勤時
                                        '1440 = 0:00～24:00を意味する
                                        Dim WW_STTIME As Integer = DateDiff("n", CDate(WW_STDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_STDATETIME)
                                        WW_WNIGHTTIME = 1440 - WW_STTIME                                                    ' 平日深夜時
                                    End If

                                End If

                                '---------------------------
                                '翌日が休みの場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間　－　休憩　－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_HWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                                ' 休日出勤時
                                        WW_ORVERTIME = 0                                                                    ' 平日深夜時
                                    End If

                                End If

                            Case "1"
                                '○法定休日（日曜）出勤
                                ' 日曜出勤時
                                WW_SWORKTIME = WK_WORKTIME_SAGYO + WK_WORKTIME_SAGYO2 + WK_YOKU0to5NIGHT_SAGYO + WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2    ' 日曜出勤時
                                WW_SWORKTIME = WW_SWORKTIME - WK_WORKTIME_KYUKEI                                         ' 日曜出勤時

                                ' 日曜深夜時
                                WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2     ' 平日深夜時

                                '---------------------------
                                '翌日が平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日5時以降の出社で、退社が翌5時以前
                                    If WW_DAYS = 1 And WW_ENDDATETIME.ToString("HH") < "06" Then
                                        WW_WNIGHTTIME = WW_SNIGHTTIME - 120                                             ' 平日深夜時
                                        WW_SNIGHTTIME = 120                                                             ' 日曜深夜時
                                    End If
                                End If

                                '---------------------------
                                '翌日が休みの場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '当日5時以降の出社で、退社が翌5時以前
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "04" And WW_ENDDATETIME.ToString("HH") < "05" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_WNIGHTTIME = WW_ENDTIME                                                  ' 平日深夜時
                                        WW_SNIGHTTIME = WW_SNIGHTTIME - WW_WNIGHTTIME                               ' 日曜深夜時
                                    End If

                                End If

                                '---------------------------
                                '当日22時以前の出社で、退社が翌5時以降
                                '---------------------------
                                If WW_DAYS = 1 And
                                    WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                    WW_WNIGHTTIME = 300                                                             ' 平日深夜時
                                    WW_SNIGHTTIME = WW_SNIGHTTIME - WW_WNIGHTTIME                                   ' 日曜深夜時

                                    Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                    '翌日平日の場合
                                    If WW_YOKUHOLIDAYKBN = "0" Then
                                        WW_SWORKTIME = WW_SWORKTIME - (WW_ENDTIME - 300)                            ' 日曜出勤時
                                    End If
                                    'ACCESSの計算を修正 2018/6/13
                                    'WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                    WW_ORVERTIME = WW_ENDTIME - 60 - 300                                            ' 平日残業
                                End If

                                '---------------------------
                                '翌日平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        WW_WNIGHTTIME = 300                                                             ' 平日深夜時
                                        WW_SNIGHTTIME = WW_SNIGHTTIME - WW_WNIGHTTIME                                   ' 日曜深夜時
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_SWORKTIME - (WW_ENDTIME - 360)                                ' 日曜出勤時
                                        WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                    End If
                                End If

                                '---------------------------
                                '翌日が休みの場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "2" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_WNIGHTTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                           ' 平日深夜時
                                        WW_ORVERTIME = 0                                                                ' 平日残業
                                    End If

                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        If WK_WORKTIME_KYUKEI < 60 Then
                                            WW_HWORKTIME = WW_ENDTIME - 60 - 300                                        ' 休日出勤時
                                        Else
                                            WW_HWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                        ' 休日出勤時
                                        End If
                                        Dim WW_STTIME As Integer = DateDiff("n", CDate(WW_STDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_STDATETIME)
                                        '1320 = 0:00～22:00を意味する
                                        WW_SWORKTIME = 1320 - WW_STTIME                                                 ' 日曜出勤時
                                        WW_ORVERTIME = 0                                                                ' 平日残業
                                    End If
                                End If

                                '---------------------------
                                '翌日平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_STTIME As Integer = DateDiff("n", CDate(WW_STDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_STDATETIME)
                                        '1320 = 0:00～22:00を意味する
                                        WW_SWORKTIME = 1320 - WW_STTIME                                                 ' 日曜出勤時
                                    End If
                                End If

                            Case "2"
                                '○法定外休日（祝日、会社指定休日）
                                ' 休日出勤時
                                WW_HWORKTIME = WK_WORKTIME_SAGYO + WK_WORKTIME_SAGYO2 + WK_YOKU0to5NIGHT_SAGYO + WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2    ' 休日出勤時
                                WW_HWORKTIME = WW_HWORKTIME - WK_WORKTIME_KYUKEI                                         ' 休日出勤時

                                ' 平日深夜時（＝平日深夜）
                                WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2     ' 平日深夜時

                                '---------------------------
                                '翌日が日曜の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "1" Then
                                    '当日5時以降の出社で、退社が翌5時以前
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "04" And WW_ENDDATETIME.ToString("HH") < "05" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SNIGHTTIME = WW_ENDTIME                                                      ' 日曜深夜時
                                        WW_WNIGHTTIME = WW_WNIGHTTIME - WW_SNIGHTTIME                                   ' 平日深夜時
                                    End If
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する。
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_ENDTIME - WK_WORKTIME_KYUKEI - 300                            ' 日曜出勤時
                                    End If

                                    '---------------------------
                                    'ACCESSの計算を修正（追加ロジック）　→　明休（日曜出勤したことと判断する）
                                    '当日22時以前の出社で、退社が翌5時以降
                                    '---------------------------
                                    If WW_DAYS = 1 And
                                        WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        WW_WNIGHTTIME = 120                                                             ' 平日深夜時
                                        WW_SNIGHTTIME = 300                                                             ' 日曜深夜時

                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_SWORKTIME = WW_ENDTIME - 300                                                 ' 日曜出勤時
                                        WW_HWORKTIME = WW_HWORKTIME - WW_SWORKTIME                                      ' 休日出勤時
                                    End If

                                End If

                                '---------------------------
                                '翌日平日の場合
                                '---------------------------
                                If WW_YOKUHOLIDAYKBN = "0" Then
                                    '当日22:00～23:59の出社で翌日5:00以降の退社の場合、拘束開始を翌日5:00と判断する。
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") > "21" And WW_STDATETIME.ToString("HH") < "24" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        '退社時間（0時～退社）－　7.5時間（拘束時間）－1時間（休憩）－　5時間（0時～5時）
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                        WW_HWORKTIME = 0                                                                ' 休日出勤時
                                    End If

                                    '出社が22時前で退社が翌5時以降
                                    If WW_DAYS = 1 And
                                       WW_STDATETIME.ToString("HH") < "22" And WW_ENDDATETIME.ToString("HH") > "04" Then
                                        Dim WW_ENDTIME As Integer = DateDiff("n", CDate(WW_ENDDATETIME.ToString("yyyy/MM/dd") & " 00:00"), WW_ENDDATETIME)
                                        WW_HWORKTIME = WW_HWORKTIME - (WW_ENDTIME - 360)                                ' 休日出勤時
                                        'ACCESSの計算を修正 2018/6/13
                                        'WW_ORVERTIME = WW_ENDTIME - 450 - 60 - 300                                      ' 平日残業
                                        WW_ORVERTIME = WW_ENDTIME - 60 - 300                                            ' 平日残業
                                    End If
                                End If
                        End Select
                    End If

                End If

                '************************************************************
                '*   マイナス時間クリア                                     *
                '************************************************************


                '○マイナス時間クリア
                '平日残業時
                If WW_ORVERTIME < 0 Then
                    WW_HEADrow("ORVERTIME") = "00:00"
                    WW_HEADrow("ORVERTIMETTL") = "00:00"
                Else
                    WW_HEADrow("ORVERTIME") = formatHHMM(WW_ORVERTIME)
                    WW_HEADrow("ORVERTIMETTL") = formatHHMM(WW_ORVERTIME + HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO")))
                End If
                '平日深夜時
                If WW_WNIGHTTIME < 0 Then
                    WW_HEADrow("WNIGHTTIME") = "00:00"
                    WW_HEADrow("WNIGHTTIMETTL") = "00:00"
                Else
                    WW_HEADrow("WNIGHTTIME") = formatHHMM(WW_WNIGHTTIME)
                    WW_HEADrow("WNIGHTTIMETTL") = formatHHMM(WW_WNIGHTTIME + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO")))
                End If
                '所定内深夜時
                If WW_NIGHTTIME < 0 Then
                    WW_HEADrow("NIGHTTIME") = "00:00"
                    WW_HEADrow("NIGHTTIMETTL") = "00:00"
                Else
                    WW_HEADrow("NIGHTTIME") = formatHHMM(WW_NIGHTTIME)
                    WW_HEADrow("NIGHTTIMETTL") = formatHHMM(WW_NIGHTTIME + HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO")))
                End If
                '休日出勤時
                If WW_HWORKTIME < 0 Then
                    WW_HEADrow("HWORKTIME") = "00:00"
                    WW_HEADrow("HWORKTIMETTL") = "00:00"
                Else
                    WW_HEADrow("HWORKTIME") = formatHHMM(WW_HWORKTIME)
                    WW_HEADrow("HWORKTIMETTL") = formatHHMM(WW_HWORKTIME + HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO")))
                End If
                '休日深夜時
                If WW_HNIGHTTIME < 0 Then
                    WW_HEADrow("HNIGHTTIME") = "00:00"
                    WW_HEADrow("HNIGHTTIMETTL") = "00:00"
                Else
                    WW_HEADrow("HNIGHTTIME") = formatHHMM(WW_HNIGHTTIME)
                    WW_HEADrow("HNIGHTTIMETTL") = formatHHMM(WW_HNIGHTTIME + HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO")))
                End If
                '日曜出勤時
                If WW_SWORKTIME < 0 Then
                    WW_HEADrow("SWORKTIME") = "00:00"
                    WW_HEADrow("SWORKTIMETTL") = "00:00"
                Else
                    WW_HEADrow("SWORKTIME") = formatHHMM(WW_SWORKTIME)
                    WW_HEADrow("SWORKTIMETTL") = formatHHMM(WW_SWORKTIME + HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO")))
                End If
                '日曜深夜時
                If WW_SNIGHTTIME < 0 Then
                    WW_HEADrow("SNIGHTTIME") = "00:00"
                    WW_HEADrow("SNIGHTTIMETTL") = "00:00"
                Else
                    WW_HEADrow("SNIGHTTIME") = formatHHMM(WW_SNIGHTTIME)
                    WW_HEADrow("SNIGHTTIMETTL") = formatHHMM(WW_SNIGHTTIME + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO")))
                End If

                '早出補填時
                If WW_HAYADETIME < 0 Then
                    WW_HEADrow("HAYADETIME") = "00:00"
                    WW_HEADrow("HAYADETIMETTL") = "00:00"
                Else

                    If Not hydFlg Then

                        WW_HEADrow("HAYADETIME") = formatHHMM(WW_HAYADETIME)
                        WW_HEADrow("HAYADETIMETTL") = formatHHMM(WW_HAYADETIME + HHMMtoMinutes(WW_HEADrow("HAYADETIMECHO")))

                    End If
                End If

                'WW_HEADrow("STATUS") = ""
                WW_HEADrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing
            WW_T0007BBtbl.Dispose()
            WW_T0007BBtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_KintaiCalc"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  残業計算（近石専用）
    Public Sub T0007_KintaiCalc_KNK(ByRef ioTbl As DataTable, ByRef iTbl As DataTable)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_IDX2 As Integer = 0
        Dim WW_IDX3 As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""

        Try
            '削除レコードを取得
            Dim WW_T0007DELtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DELtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl = CS0026TblSort.sort()

            '勤怠の明細レコードを取得
            Dim WW_T0007DTLtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '休憩レコードを取得
            Dim WW_T0007BBtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and WORKKBN = 'BB' "
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007BBtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl2 As DataTable = New DataTable
            CS0026TblSort.TABLE = iTbl
            CS0026TblSort.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and DELFLG = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl2 = CS0026TblSort.sort()

            '直前、翌日取得用VIEW
            Dim iT0007view As DataView
            iT0007view = New DataView(WW_T0007HEADtbl2)
            iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

            WW_IDX = 0
            For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
                'STATUS<>''（勤怠に変更が発生しているレコード）
                If WW_HEADrow("RECODEKBN") = "0" Then
                Else
                    Continue For
                End If

                '************************************************************
                '*   勤怠日数設定                                           *
                '************************************************************
                NissuItem_Init(WW_HEADrow)
                Select Case WW_HEADrow("PAYKBN")
                    Case "00"
                        '○勤怠区分(00:通常) …　出勤扱い(所労=1 )
                        If WW_HEADrow("HOLIDAYKBN") = "0" Then
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        Else
                            If WW_HEADrow("STTIME") = "00:00" And WW_HEADrow("ENDTIME") = "00:00" Then
                            Else
                                WW_HEADrow("HWORKNISSU") = 1         '休日出勤日数
                                WW_HEADrow("HWORKNISSUTTL") = 1      '休日出勤日数
                            End If
                        End If
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                '2018/02/01 追加
                                If WW_HEADrow("STTIME") = "00:00" And WW_HEADrow("ENDTIME") = "00:00" Then
                                Else
                                    WW_HEADrow("NENSHINISSU") = 1    '年始出勤日数
                                    WW_HEADrow("NENSHINISSUTTL") = 1 '年始出勤日数
                                End If
                                '2018/02/01 追加
                            End If
                        End If
                    Case "01"
                        '○勤怠区分(01:年休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("NENKYUNISSU") = 1        '年次有給休暇
                            WW_HEADrow("NENKYUNISSUTTL") = 1     '年次有給休暇
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "02"
                        '○勤怠区分(2:特休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("TOKUKYUNISSU") = 1       '特別有給休暇
                            WW_HEADrow("TOKUKYUNISSUTTL") = 1    '特別有給休暇
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "03"
                        '○勤怠区分(3:遅刻早退) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("CHIKOKSOTAINISSU") = 1    '遅刻早退日数
                            WW_HEADrow("CHIKOKSOTAINISSUTTL") = 1 '遅刻早退日数
                            WW_HEADrow("WORKNISSU") = 1           '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1        '所定労働日数
                        End If
                    Case "04"
                        '○勤怠区分(4:ｽﾄｯｸ休暇) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("STOCKNISSU") = 1         'ストック休暇日数
                            WW_HEADrow("STOCKNISSUTTL") = 1      'ストック休暇日数
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "05"
                        '○勤怠区分(5:協約週休) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KYOTEIWEEKNISSU") = 1     '協定週休日数
                            WW_HEADrow("KYOTEIWEEKNISSUTTL") = 1  '協定週休日数
                        End If
                    Case "06"
                        '○勤怠区分(6:協約外週休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("WEEKNISSU") = 1          '週休日数
                            WW_HEADrow("WEEKNISSUTTL") = 1       '週休日数
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "07"
                        '○勤怠区分(7:傷欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("SHOUKETUNISSU") = 1      '傷欠勤日数
                            WW_HEADrow("SHOUKETUNISSUTTL") = 1   '傷欠勤日数
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "08"
                        '○勤怠区分(8:組欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KUMIKETUNISSU") = 1      '組合欠勤日数
                            WW_HEADrow("KUMIKETUNISSUTTL") = 1   '組合欠勤日数
                        End If
                    Case "09"
                        '○勤怠区分(9:他欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("ETCKETUNISSU") = 1       'その他欠勤日数
                            WW_HEADrow("ETCKETUNISSUTTL") = 1    'その他欠勤日数
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "10"
                        '○勤怠区分(10:代休出勤) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            'WW_HEADrow("PAYKBN") = "00"
                            'WW_HEADrow("PAYKBNNAMES") = "通常"
                        End If
                    Case "11"
                        '○勤怠区分(11:代休取得) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("DAIKYUNISSU") = 1        '代休取得日数
                            WW_HEADrow("DAIKYUNISSUTTL") = 1     '代休取得日数
                            WW_HEADrow("WORKNISSU") = 1          '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1       '所定労働日数
                        End If
                    Case "12"
                        '○勤怠区分(12:振替出勤) 
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            'WW_HEADrow("PAYKBN") = "00"
                            'WW_HEADrow("PAYKBNNAMES") = "通常"
                        End If
                    Case "13"
                        '○勤怠区分(13:振替取得)
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("WORKNISSU") = 1         '所定労働日数
                            WW_HEADrow("WORKNISSUTTL") = 1      '所定労働日数
                        End If
                End Select

                '************************************************************
                '*   宿日直設定                                             *
                '************************************************************
                Select Case WW_HEADrow("SHUKCHOKKBN")
                    Case "0"
                        '○宿日直区分(0:なし)
                        WW_HEADrow("SHUKCHOKNNISSU") = 0             '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNNISSUTTL") = 0          '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNISSU") = 0              '宿日直通常日数
                        WW_HEADrow("SHUKCHOKNISSUTTL") = 0           '宿日直通常日数
                        '2018/02/08 追加
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSU") = 0          '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0       '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKHLDNISSU") = 0           '宿直(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0        '宿直(翌日休み)
                        End If
                        '2018/02/08 追加


                    Case "1", "2"
                        '○宿日直区分(1:日直、2:宿直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 1    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 1     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1  '宿日直通常日数
                            End If
                        End If

                    Case "3"
                        '○宿日直区分(3:宿日直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 2    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 2 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 2     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 2  '宿日直通常日数
                            End If
                        End If

                    Case "4"
                        '○宿日直区分(4:宿直(翌日休み)／宿直(割増有り))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 1     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 1  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 0        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 0     '宿日直通常日数
                            End If
                        End If

                    Case "5"
                        '○宿日直区分(5:宿直(翌日営業)／宿直(割増無し))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 0     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 1        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1     '宿日直通常日数
                            End If
                        End If
                End Select

                '************************************************************
                '乗務時間計（距離がゼロ以外）
                '************************************************************
                If WW_HEADrow("HAIDISTANCE") > 0 Or WW_HEADrow("KAIDISTANCE") > 0 Then
                    WW_HEADrow("JYOMUTIME") = Minute5Edit(formatHHMM(HHMMtoMinutes(WW_HEADrow("WORKTIME")) - HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))))
                    WW_HEADrow("JYOMUTIMETTL") = Minute5Edit(formatHHMM(HHMMtoMinutes(WW_HEADrow("WORKTIME")) - HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))))
                Else
                    WW_HEADrow("JYOMUTIME") = "00:00"
                    WW_HEADrow("JYOMUTIMETTL") = "00:00"
                End If

                '************************************************************
                '所定内計時間（平日の場合）
                '************************************************************
                If CheckHOLIDAY(WW_HEADrow("HOLIDAYKBN"), WW_HEADrow("PAYKBN")) = False Then
                    WW_HEADrow("WWORKTIME") = WW_HEADrow("BINDTIME")
                    WW_HEADrow("WWORKTIMETTL") = WW_HEADrow("BINDTIME")
                End If

                '************************************************************
                '*   勤怠時間設定                                           *
                '************************************************************
                '   前提：出勤時刻は、当日0時から21時59分まで
                ' 　    ：退社時刻は、翌日5時まで
                '○退社日が出社当日～翌日 and 出社日時 < 退社日時 のみ時間計算を行う
                '以降処理で判定用(出社日時、退社日時)を算出
                Dim WW_STDATETIME As Date
                Dim WW_ENDDATETIME As Date

                '出社、退社が未入力の場合、残業計算しない
                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) And
                   IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                Else
                    Continue For
                End If

                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                '直前および、翌日の勤務状況取得
                Dim WW_YOKUHOLIDAYKBN As String = ""
                Dim WW_YOKUACTTIME As String = ""

                'If WW_HEADrow("STAFFKBN") Like "03*" Then

                Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))


                '翌日の勤務
                WW_YOKUHOLIDAYKBN = "0"
                Dim WW_YOKUDATE As String = dt.AddDays(1).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_YOKUDATE & "#"
                If iT0007view.Count > 0 Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "1" Then
                        WW_YOKUHOLIDAYKBN = "1"
                    End If

                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "2" Or
                       iT0007view.Item(0).Row("PAYKBN") = "01" Or
                       iT0007view.Item(0).Row("PAYKBN") = "02" Or
                       iT0007view.Item(0).Row("PAYKBN") = "04" Or
                       iT0007view.Item(0).Row("PAYKBN") = "05" Or
                       iT0007view.Item(0).Row("PAYKBN") = "06" Or
                       iT0007view.Item(0).Row("PAYKBN") = "07" Or
                       iT0007view.Item(0).Row("PAYKBN") = "08" Or
                       iT0007view.Item(0).Row("PAYKBN") = "09" Or
                       iT0007view.Item(0).Row("PAYKBN") = "11" Or
                       iT0007view.Item(0).Row("PAYKBN") = "13" Or
                       iT0007view.Item(0).Row("PAYKBN") = "15" Then
                        WW_YOKUHOLIDAYKBN = "2"
                    End If

                    '************************************************************
                    '*   一般（新潟東港以外）                                   *
                    '************************************************************
                    If WW_YOKUHOLIDAYKBN = "1" Or WW_YOKUHOLIDAYKBN = "2" Then
                        If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                            '稼働あり
                            WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                        End If
                    End If
                Else
                    '翌日勤務未入力の場合、カレンダーより（救済）
                    MB005_Select(WW_HEADrow("CAMPCODE"), WW_YOKUDATE, WW_YOKUHOLIDAYKBN, WW_RTN)
                    If WW_RTN <> C_MESSAGE_NO.NORMAL Then
                        'カレンダー取得できず（救済）
                        If Weekday(DateSerial(Year(CDate(WW_YOKUDATE)), Month(CDate(WW_YOKUDATE)), Day(CDate(WW_YOKUDATE)))) = 1 Then
                            '日曜日
                            WW_YOKUHOLIDAYKBN = 1
                        Else
                            '平日
                            WW_YOKUHOLIDAYKBN = 0
                        End If
                    End If
                    WW_YOKUACTTIME = ""
                End If
                'End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) Then
                    WW_STDATETIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                End If
                If IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                    WW_ENDDATETIME = CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME"))
                End If

                '○出社日時、退社日時の計算　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    '・出社を拘束開始とする(拘束時がZERO時、救済措置)
                    WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                    '・拘束開始1時半未満は1時半とする
                    If IsDate(WW_HEADrow("BINDSTDATE")) Then
                        If CDate(WW_HEADrow("BINDSTDATE")).ToString("HHmm") > "0000" And CDate(WW_HEADrow("BINDSTDATE")).ToString("HHmm") < "0130" Then
                            WW_HEADrow("BINDSTDATE") = "01:30"
                        End If
                    End If

                End If

                '●時間算出（拘束開始日時、拘束終了日時）

                '○初期設定　★  共通処理(事務員+乗務員)　★
                Dim WW_BINDSTTIME As DateTime
                Dim WW_BINDENDTIME As DateTime
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & WW_HEADrow("BINDSTDATE"))
                    WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & CDate(WW_HEADrow("BINDSTDATE")).ToString("HH:mm"))
                End If

                Dim WW_WORKINGH As String = WW_HEADrow("BINDTIME")
                Select Case WW_HEADrow("HOLIDAYKBN")
                    '○平日
                    Case "1", "2"
                        Select Case WW_HEADrow("PAYKBN")
                            Case "10", "12"
                                '所定拘束時間取得
                                WORKINGHget(WW_HEADrow, WW_WORKINGH, WW_RTN)
                        End Select
                End Select

                '○拘束終了日時の設定　★  事務員処理　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '出社日時＋拘束時間(7:30)＋休憩(通常休憩)　…１時間取らないケース有。????再検討必要
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_WORKINGH).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_WORKINGH).ToString("mm"))
                    'WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    'WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(HHMMtoMinutes(WW_HEADrow("BREAKTIME")))
                    '2018/02/06 追加
                End If

                '○拘束終了日時の設定　★  乗務員処理　★
                '   　　説明：拘束終了日時　…　実際の休憩を含む拘束終了時間（残業開始時間）
                '             拘束終了時間に休憩が含まれる場合、拘束終了時間を休憩分延長する
                Dim WW_BREAKTIMEZAN As Integer = 0
                Dim WW_BREAKTIMETTL As Integer = 0
                Dim WW_MIN As Integer = 0
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then
                    WW_BREAKTIMETTL = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    If WW_BREAKTIMETTL > 60 Then
                        WW_BREAKTIMEZAN = WW_BREAKTIMETTL - 60
                        WW_MIN = 60
                    Else
                        WW_BREAKTIMEZAN = 0
                        WW_MIN = WW_BREAKTIMETTL
                    End If

                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_WORKINGH).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_WORKINGH).ToString("mm"))
                    'WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    'WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(WW_MIN)
                End If

                '●時間算出（所定内通常分_作業、所定内深夜分_作業、所定内深夜分2_作業、所定外通常分_作業、所定外深夜分_作業、休日通常分_作業、休日深夜分_作業、休日深夜分2_作業）
                Dim WK_WORKTIME_SAGYO As Integer = 0         '平日＆所定内＆深夜以外（当日分）
                Dim WK_WORKTIME_SAGYO2 As Integer = 0        '平日＆所定内＆深夜以外（翌日分）
                Dim WK_NIGHTTIME_SAGYO As Integer = 0        '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_SAGYO As Integer = 0    '平日＆所定内＆深夜　　（24:00～29:00）
                Dim WK_YOKU0to5NIGHT_SAGYO2 As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_SAGYO As Integer = 0      '平日＆所定外＆深夜以外（当日分）
                Dim WK_OUTWORKTIME_SAGYO2 As Integer = 0     '平日＆所定外＆深夜以外（翌日分）
                Dim WK_OUTNIGHTTIME_SAGYO As Integer = 0     '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_SAGYO As Integer = 0        '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_SAGYO As Integer = 0       '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_SAGYO2 As Integer = 0      '休日＆残業　＆深夜     (24:00～29:00)

                '休憩時間
                Dim WK_WORKTIME_KYUKEI As Integer = 0        '平日＆所定内＆深夜以外
                Dim WK_NIGHTTIME_KYUKEI As Integer = 0       '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_KYUKEI As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_KYUKEI As Integer = 0     '平日＆所定外＆深夜以外
                Dim WK_OUTNIGHTTIME_KYUKEI As Integer = 0    '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_KYUKEI As Integer = 0       '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_KYUKEI As Integer = 0      '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_KYUKEI2 As Integer = 0     '休日＆残業　＆深夜     (24:00～29:00)

                Dim WW_累積分 As Integer = 0

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    ' In  : WW_STDATETIME             出社日時
                    '       WW_BINDSTTIME             拘束開始日時
                    '       WW_BINDENDTIME            拘束終了日時
                    '       0                         休日区分 = 0(固定)
                    '       WW_STDATETIME             出社日時
                    '       WW_ENDDATETIME            退社日時
                    ' Out : WK_WORKTIME_SAGYO         5:00～22:00（所定内通常）
                    '       WK_WORKTIME_SAGYO2        翌5:00～22:00（所定内通常）    
                    '       WK_NIGHTTIME_SAGYO        22:00～24:00（深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO    翌0:00～5:00（所定内深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO2   翌0:00～5:00（深夜）
                    '       WK_OUTWORKTIME_SAGYO      5:00～22:00（残業）　　← 法定、法定外休日のみ
                    '       WK_OUTWORKTIME_SAGYO2     翌5:00～22:00（残業）
                    '       WK_OUTNIGHTTIME_SAGYO     0:00～5:00（5時前深夜）
                    '       WK_HWORKTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO2,
                    '       WW_累積分
                    Call NightTimeMinuteGet(WW_STDATETIME,
                                            WW_BINDSTTIME,
                                            WW_BINDENDTIME,
                                            0,
                                            WW_STDATETIME,
                                            WW_ENDDATETIME,
                                            WK_WORKTIME_SAGYO,
                                            WK_WORKTIME_SAGYO2,
                                            WK_NIGHTTIME_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO2,
                                            WK_OUTWORKTIME_SAGYO,
                                            WK_OUTWORKTIME_SAGYO2,
                                            WK_OUTNIGHTTIME_SAGYO,
                                            WK_HWORKTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO2,
                                            WW_累積分)
                End If

                '○休憩時間計算　★  事務員処理　★
                If Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WK_WORKTIME_KYUKEI = HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                    '--------------------------------------------------------------------
                    '2018/02/06 追加
                End If

                '************************************************************
                '*   残業設定                                               *
                '************************************************************
                '○異常事態の救済　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    '・拘束開始前に退社の場合
                    If DateDiff("n", WW_BINDSTTIME, WW_ENDDATETIME) < 0 Then
                        WW_BINDSTTIME = WW_STDATETIME
                    End If
                End If

                '○マイナス時間クリア
                Dim WW_ORVERTIME As Integer = 0      '平日残業時
                Dim WW_WNIGHTTIME As Integer = 0     '平日深夜時
                Dim WW_NIGHTTIME As Integer = 0      '所定内深夜時
                Dim WW_HWORKTIME As Integer = 0      '休日出勤時
                Dim WW_HNIGHTTIME As Integer = 0     '休日深夜時
                Dim WW_HDAIWORKTIME As Integer = 0   '代休出勤時
                Dim WW_HDAINIGHTTIME As Integer = 0  '代休深夜時
                Dim WW_SWORKTIME As Integer = 0      '日曜出勤時
                Dim WW_SNIGHTTIME As Integer = 0     '日曜深夜時
                Dim WW_SDAIWORKTIME As Integer = 0   '日曜代休出勤時
                Dim WW_SDAINIGHTTIME As Integer = 0  '日曜代休深夜時
                Dim WW_WWORKTIME As Integer = HHMMtoMinutes(WW_HEADrow("WWORKTIME"))     '所定内時

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    Select Case WW_HEADrow("HOLIDAYKBN")
                        '○平日
                        Case "0"
                            '平日残業
                            If WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 平日残業時
                                WW_BREAKTIMEZAN = 0
                            Else
                                WW_ORVERTIME = 0                                           ' 平日残業時
                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2) ' 休憩残算出
                            End If

                            '所定内深夜
                            WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO     ' 所定内深夜時

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                '深夜時間(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                End If
                            End If

                            '翌日日曜日の場合
                            If WW_YOKUHOLIDAYKBN = "1" Then
                                If WW_YOKUACTTIME = "" Then
                                    If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                    End If

                                    '稼働なし、日曜深夜(24:00～翌5:00)
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                    End If
                                Else
                                    '稼働あり、深夜時間(24:00～翌5:00)
                                    '深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                    End If
                                End If

                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                If WW_YOKUACTTIME = "" Then
                                    If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                    End If

                                    '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                    End If
                                Else
                                    '稼働あり 深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                    End If
                                End If

                            End If


                        Case "1"
                            '○法定休日（日曜）出勤
                            Select Case WW_HEADrow("PAYKBN")
                                Case "10" '代休出勤
                                    '日曜日の代休出勤の場合、所定労働時間を取得し残業計算する（通常は、日曜日は所定労働時間ゼロ（全て残業））
                                    '※所定労働時間の取得は、上記の残業計算前に取得
                                    WW_SDAIWORKTIME = WK_WORKTIME_SAGYO - WW_BREAKTIMETTL                           ' 日曜代休出勤時（所定内）
                                    WW_SDAINIGHTTIME = WK_NIGHTTIME_SAGYO                                           ' 日曜代休深夜時（所定内深夜）
                                    WW_SWORKTIME = WK_OUTWORKTIME_SAGYO                                             ' 日曜出勤時（残業）
                                    WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO                                           ' 日曜深夜時（深夜残業）
                                Case "12" '振替出勤
                                    WW_WWORKTIME = WK_WORKTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMETTL         ' 所定内時
                                    WW_NIGHTTIME = WK_NIGHTTIME_SAGYO                                               ' 所定内深夜時
                                    WW_ORVERTIME = WK_OUTWORKTIME_SAGYO                                             ' 平日残業時
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO                                           ' 平日深夜時
                                Case Else
                                    If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_SWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN                       ' 日曜出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SWORKTIME = 0                                                            ' 日曜出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO                    ' 休憩残算出
                                    End If

                                    If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 日曜深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SNIGHTTIME = 0                                                                ' 日曜深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                                    End If

                                    '翌日平日の場合
                                    If WW_YOKUHOLIDAYKBN = "0" Then
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_WNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_WNIGHTTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2   ' 休憩残算出
                                        End If

                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_ORVERTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 平日残業
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_ORVERTIME = 0                                              ' 平日残業
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                        End If

                                    End If

                                    '翌日法定外休日の場合
                                    If WW_YOKUHOLIDAYKBN = "2" Then
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 休日出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HWORKTIME = 0
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                        End If

                                        If WW_YOKUACTTIME = "" Then
                                            '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                            If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                                WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                                WW_BREAKTIMEZAN = 0
                                            Else
                                                WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                            End If
                                        Else
                                            '稼働あり 日曜深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                            If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                                WW_SNIGHTTIME = WW_SNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 日曜深夜時
                                                WW_BREAKTIMEZAN = 0
                                            Else
                                                WW_SNIGHTTIME = WW_SNIGHTTIME + 0                                          ' 日曜深夜時
                                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                            End If
                                        End If
                                    End If
                            End Select


                        Case "2"
                            '○法定外休日（祝日、会社指定休日）
                            Select Case WW_HEADrow("PAYKBN")
                                Case "10" '日曜代休出勤
                                    '祝日（日曜以外）の代休出勤の場合、所定労働時間を取得し残業計算する（通常は、祝日は所定労働時間ゼロ（全て残業））
                                    '※所定労働時間の取得は、上記の残業計算前に取得
                                    WW_HDAIWORKTIME = WK_WORKTIME_SAGYO - WW_BREAKTIMETTL                           ' 代休出勤時（所定内）
                                    WW_HDAINIGHTTIME = WK_NIGHTTIME_SAGYO                                           ' 代休深夜時（所定内深夜）
                                    WW_HWORKTIME = WK_OUTWORKTIME_SAGYO                                             ' 休日出勤時（残業）
                                    WW_HNIGHTTIME = WK_OUTNIGHTTIME_SAGYO                                           ' 休日深夜時（深夜残業）
                                Case "12" '振替出勤
                                    WW_WWORKTIME = WK_WORKTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMETTL         ' 所定内時
                                    WW_NIGHTTIME = WK_NIGHTTIME_SAGYO                                               ' 所定内深夜時
                                    WW_ORVERTIME = WK_OUTWORKTIME_SAGYO                                             ' 平日残業時
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO                                           ' 平日深夜時
                                Case Else
                                    ' 休日出勤時
                                    If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = 0                                           ' 休日出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                                    End If

                                    ' 休日深夜時
                                    If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = 0                                                                ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                                    End If

                                    '翌日平日の場合
                                    If WW_YOKUHOLIDAYKBN = "0" Then
                                        '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                        ' 休日出勤時
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                        End If

                                        ' 休日深夜時
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                        End If
                                    End If


                                    '翌日日曜日の場合
                                    If WW_YOKUHOLIDAYKBN = "1" Then
                                        If WW_YOKUACTTIME = "" Then
                                            '稼働なし、日曜深夜(24:00～翌5:00)
                                            ' 日曜出勤時
                                            If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                                WW_SWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 日曜出勤時
                                                WW_BREAKTIMEZAN = 0
                                            Else
                                                WW_SWORKTIME = 0                                            ' 日曜出勤時
                                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2   ' 休憩残算出
                                            End If

                                            ' 日曜深夜時
                                            If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                                WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                                WW_BREAKTIMEZAN = 0
                                            Else
                                                WW_SNIGHTTIME = 0                                           ' 日曜深夜時
                                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                            End If
                                        Else
                                            '稼働あり、休日深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                            ' 休日出勤時
                                            If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                                WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                                WW_BREAKTIMEZAN = 0
                                            Else
                                                WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                            End If

                                            ' 休日深夜時
                                            If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                                WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN    ' 休日深夜時
                                                WW_BREAKTIMEZAN = 0
                                            Else
                                                WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                            ' 休日深夜時
                                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                  ' 休憩残算出
                                            End If
                                        End If
                                    End If

                                    '翌日法定外休日の場合
                                    If WW_YOKUHOLIDAYKBN = "2" Then
                                        '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                        ' 休日出勤時
                                        If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 休日出勤時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HWORKTIME = WW_HWORKTIME + 0                                            ' 休日出勤時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2                  ' 休憩残算出
                                        End If

                                        ' 休日深夜時
                                        If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                            WW_BREAKTIMEZAN = 0
                                        Else
                                            WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                            WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                        End If
                                    End If
                            End Select
                    End Select

                End If

                '************************************************************
                '*   マイナス時間クリア                                     *
                '************************************************************


                '○マイナス時間クリア
                '所定内時
                If WW_WWORKTIME < 0 Then
                    WW_HEADrow("WWORKTIME") = "00:00"
                Else
                    WW_HEADrow("WWORKTIME") = Minute5Edit(formatHHMM(WW_WWORKTIME))
                End If
                WW_HEADrow("WWORKTIMETTL") = Minute5Edit(formatHHMM(WW_WWORKTIME + HHMMtoMinutes(WW_HEADrow("WWORKTIMECHO"))))

                '平日残業時
                If WW_ORVERTIME < 0 Then
                    WW_HEADrow("ORVERTIME") = "00:00"
                Else
                    WW_HEADrow("ORVERTIME") = Minute5Edit(formatHHMM(WW_ORVERTIME))
                End If
                WW_HEADrow("ORVERTIMETTL") = Minute5Edit(formatHHMM(WW_ORVERTIME + HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO"))))
                '平日深夜時
                If WW_WNIGHTTIME < 0 Then
                    WW_HEADrow("WNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("WNIGHTTIME") = Minute5Edit(formatHHMM(WW_WNIGHTTIME))
                End If
                WW_HEADrow("WNIGHTTIMETTL") = Minute5Edit(formatHHMM(WW_WNIGHTTIME + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO"))))
                '所定内深夜時
                If WW_NIGHTTIME < 0 Then
                    WW_HEADrow("NIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("NIGHTTIME") = Minute5Edit(formatHHMM(WW_NIGHTTIME))
                End If
                WW_HEADrow("NIGHTTIMETTL") = Minute5Edit(formatHHMM(WW_NIGHTTIME + HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO"))))
                '休日出勤時
                If WW_HWORKTIME < 0 Then
                    WW_HEADrow("HWORKTIME") = "00:00"
                Else
                    WW_HEADrow("HWORKTIME") = Minute5Edit(formatHHMM(WW_HWORKTIME))
                End If
                WW_HEADrow("HWORKTIMETTL") = Minute5Edit(formatHHMM(WW_HWORKTIME + HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO"))))
                '代休出勤時
                If WW_HDAIWORKTIME < 0 Then
                    WW_HEADrow("HDAIWORKTIME") = "00:00"
                Else
                    WW_HEADrow("HDAIWORKTIME") = Minute5Edit(formatHHMM(WW_HDAIWORKTIME))
                End If
                WW_HEADrow("HDAIWORKTIMETTL") = Minute5Edit(formatHHMM(WW_HDAIWORKTIME + HHMMtoMinutes(WW_HEADrow("HDAIWORKTIMECHO"))))
                '休日深夜時
                If WW_HNIGHTTIME < 0 Then
                    WW_HEADrow("HNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("HNIGHTTIME") = Minute5Edit(formatHHMM(WW_HNIGHTTIME))
                End If
                WW_HEADrow("HNIGHTTIMETTL") = Minute5Edit(formatHHMM(WW_HNIGHTTIME + HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO"))))
                '代休深夜時
                If WW_HDAINIGHTTIME < 0 Then
                    WW_HEADrow("HDAINIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("HDAINIGHTTIME") = Minute5Edit(formatHHMM(WW_HDAINIGHTTIME))
                End If
                WW_HEADrow("HDAINIGHTTIMETTL") = Minute5Edit(formatHHMM(WW_HDAINIGHTTIME + HHMMtoMinutes(WW_HEADrow("HDAINIGHTTIMECHO"))))
                '日曜出勤時
                If WW_SWORKTIME < 0 Then
                    WW_HEADrow("SWORKTIME") = "00:00"
                Else
                    WW_HEADrow("SWORKTIME") = Minute5Edit(formatHHMM(WW_SWORKTIME))
                End If
                WW_HEADrow("SWORKTIMETTL") = Minute5Edit(formatHHMM(WW_SWORKTIME + HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO"))))
                '日曜代休出勤時
                If WW_SDAIWORKTIME < 0 Then
                    WW_HEADrow("SDAIWORKTIME") = "00:00"
                Else
                    WW_HEADrow("SDAIWORKTIME") = Minute5Edit(formatHHMM(WW_SDAIWORKTIME))
                End If
                WW_HEADrow("SDAIWORKTIMETTL") = Minute5Edit(formatHHMM(WW_SDAIWORKTIME + HHMMtoMinutes(WW_HEADrow("SDAIWORKTIMECHO"))))
                '日曜深夜時
                If WW_SNIGHTTIME < 0 Then
                    WW_HEADrow("SNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("SNIGHTTIME") = Minute5Edit(formatHHMM(WW_SNIGHTTIME))
                End If
                WW_HEADrow("SNIGHTTIMETTL") = Minute5Edit(formatHHMM(WW_SNIGHTTIME + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO"))))
                '日曜代休深夜時
                If WW_SDAINIGHTTIME < 0 Then
                    WW_HEADrow("SDAINIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("SDAINIGHTTIME") = Minute5Edit(formatHHMM(WW_SDAINIGHTTIME))
                End If
                WW_HEADrow("SDAINIGHTTIMETTL") = Minute5Edit(formatHHMM(WW_SDAINIGHTTIME + HHMMtoMinutes(WW_HEADrow("SDAINIGHTTIMECHO"))))

                'WW_HEADrow("STATUS") = ""
                WW_HEADrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing
            WW_T0007BBtbl.Dispose()
            WW_T0007BBtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_KintaiCalc"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ' ***  残業計算（ＮＪＳ専用）
    Public Sub T0007_KintaiCalc_NJS(ByRef ioTbl As DataTable, ByRef iTbl As DataTable, Optional ByVal iTokusaKbn As String = "")
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_IDX2 As Integer = 0
        Dim WW_IDX3 As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""

        Try
            '削除レコードを取得
            Dim WW_T0007DELtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DELtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl = CS0026TblSort.sort()

            '勤怠の明細レコードを取得
            Dim WW_T0007DTLtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '休憩レコードを取得
            Dim WW_T0007BBtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and WORKKBN = 'BB' "
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007BBtbl = CS0026TblSort.sort()

            '配送レコードを取得
            Dim WW_T0007G1tbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and WORKKBN = 'G1' "
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007G1tbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl2 As DataTable = New DataTable
            CS0026TblSort.TABLE = iTbl
            CS0026TblSort.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and DELFLG = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl2 = CS0026TblSort.sort()

            '直前、翌日取得用VIEW
            Dim iT0007view As DataView
            iT0007view = New DataView(WW_T0007HEADtbl2)
            iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

            WW_IDX = 0
            WW_IDX2 = 0
            For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
                'STATUS<>''（勤怠に変更が発生しているレコード）
                If WW_HEADrow("RECODEKBN") = "0" Then
                Else
                    Continue For
                End If

                '************************************************************
                '*   勤怠日数設定                                           *
                '************************************************************
                NissuItem_Init(WW_HEADrow)
                Select Case WW_HEADrow("PAYKBN")
                    Case "00"
                        '○勤怠区分(00:通常) …　出勤扱い(所労=1 )
                        If WW_HEADrow("HOLIDAYKBN") = "0" Then
                        End If
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                '2018/02/01 追加
                                If WW_HEADrow("STTIME") = "00:00" And WW_HEADrow("ENDTIME") = "00:00" Then
                                Else
                                    WW_HEADrow("NENSHINISSU") = 0    '年始出勤日数
                                    WW_HEADrow("NENSHINISSUTTL") = 0 '年始出勤日数
                                End If
                                '2018/02/01 追加
                            End If
                        End If
                    Case "01"
                        '○勤怠区分(01:年休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("NENKYUNISSU") = 1        '年次有給休暇
                            WW_HEADrow("NENKYUNISSUTTL") = 1     '年次有給休暇
                        End If
                    Case "02"
                        '○勤怠区分(2:特休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("TOKUKYUNISSU") = 1       '特別有給休暇
                            WW_HEADrow("TOKUKYUNISSUTTL") = 1    '特別有給休暇
                        End If
                    Case "03"
                        '○勤怠区分(3:遅刻早退) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("CHIKOKSOTAINISSU") = 1    '遅刻早退日数
                            WW_HEADrow("CHIKOKSOTAINISSUTTL") = 1 '遅刻早退日数
                        End If
                    Case "04"
                        '○勤怠区分(4:ｽﾄｯｸ休暇) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("STOCKNISSU") = 1         'ストック休暇日数
                            WW_HEADrow("STOCKNISSUTTL") = 1      'ストック休暇日数
                        End If
                    Case "05"
                        '○勤怠区分(5:協約週休) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KYOTEIWEEKNISSU") = 1     '協定週休日数
                            WW_HEADrow("KYOTEIWEEKNISSUTTL") = 1  '協定週休日数
                        End If
                    Case "06"
                        '○勤怠区分(6:協約外週休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("WEEKNISSU") = 1          '週休日数
                            WW_HEADrow("WEEKNISSUTTL") = 1       '週休日数
                        End If
                    Case "07"
                        '○勤怠区分(7:傷欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("SHOUKETUNISSU") = 1      '傷欠勤日数
                            WW_HEADrow("SHOUKETUNISSUTTL") = 1   '傷欠勤日数
                        End If
                    Case "08"
                        '○勤怠区分(8:組欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KUMIKETUNISSU") = 1      '組合欠勤日数
                            WW_HEADrow("KUMIKETUNISSUTTL") = 1   '組合欠勤日数
                        End If
                    Case "09"
                        '○勤怠区分(9:他欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("ETCKETUNISSU") = 1       'その他欠勤日数
                            WW_HEADrow("ETCKETUNISSUTTL") = 1    'その他欠勤日数
                        End If
                    Case "10"
                        '○勤怠区分(10:代休出勤) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        End If
                    Case "11"
                        '○勤怠区分(11:代休取得) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("DAIKYUNISSU") = 1        '代休取得日数
                            WW_HEADrow("DAIKYUNISSUTTL") = 1     '代休取得日数
                        End If
                    Case "12"
                        '○勤怠区分(12:年始出勤取得) …　出勤扱い(所労=1 )
                        'NJSの場合、12/31、1/1～1/3（年末年始出勤は、1月給与払い）。手入力するため自動判定不要（ユーザー要望）
                        WW_HEADrow("NENSHINISSU") = 0            '年始出勤日数
                        WW_HEADrow("NENSHINISSUTTL") = 0         '年始出勤日数
                    Case "17"
                        '○勤怠区分(17:年末出勤取得) …　出勤扱い(所労=1 )
                        'NJSの場合、12/31、1/1～1/3（年末年始出勤は、1月給与払い）。手入力するため自動判定不要（ユーザー要望）
                        WW_HEADrow("NENMATUNISSU") = 0            '年始出勤日数
                        WW_HEADrow("NENMATUNISSUTTL") = 0         '年始出勤日数
                End Select

                '************************************************************
                '*   宿日直設定                                             *
                '************************************************************
                Select Case WW_HEADrow("SHUKCHOKKBN")
                    Case "0"
                        '○宿日直区分(0:なし)
                        WW_HEADrow("SHUKCHOKNNISSU") = 0             '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNNISSUTTL") = 0          '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNISSU") = 0              '宿日直通常日数
                        WW_HEADrow("SHUKCHOKNISSUTTL") = 0           '宿日直通常日数
                        '2018/02/08 追加
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSU") = 0          '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0       '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKHLDNISSU") = 0           '宿直(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0        '宿直(翌日休み)
                        End If
                        '2018/02/08 追加


                    Case "1", "2"
                        '○宿日直区分(1:日直、2:宿直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 1    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 1     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1  '宿日直通常日数
                            End If
                        End If

                    Case "3"
                        '○宿日直区分(3:宿日直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 2    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 2 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 2     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 2  '宿日直通常日数
                            End If
                        End If

                    Case "4"
                        '○宿日直区分(4:宿直(翌日休み)／宿直(割増有り))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 1     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 1  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 0        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 0     '宿日直通常日数
                            End If
                        End If

                    Case "5"
                        '○宿日直区分(5:宿直(翌日営業)／宿直(割増無し))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 0     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 1        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1     '宿日直通常日数
                            End If
                        End If
                End Select

                '************************************************************
                '*   車中泊（１日目）設定                                   *
                '************************************************************
                Select Case WW_HEADrow("SHACHUHAKKBN")
                    Case "1"
                        WW_HEADrow("SHACHUHAKNISSU") = 1    '車中泊（１日目）
                        WW_HEADrow("SHACHUHAKNISSUTTL") = 1 '車中泊（１日目）
                End Select


                '************************************************************
                '*   勤怠時間設定                                           *
                '************************************************************
                '   前提：出勤時刻は、当日0時から21時59分まで
                ' 　    ：退社時刻は、翌日5時まで
                '○退社日が出社当日～翌日 and 出社日時 < 退社日時 のみ時間計算を行う
                '以降処理で判定用(出社日時、退社日時)を算出
                Dim WW_STDATETIME As Date
                Dim WW_ENDDATETIME As Date

                '出社、退社が未入力の場合、残業計算しない
                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) And
                   IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                Else
                    Continue For
                End If

                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                '直前および、翌日の勤務状況取得
                Dim WW_YOKUHOLIDAYKBN As String = ""
                Dim WW_YOKUACTTIME As String = ""

                'If WW_HEADrow("STAFFKBN") Like "03*" Then

                Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))


                '翌日の勤務
                WW_YOKUHOLIDAYKBN = "0"
                Dim WW_YOKUDATE As String = dt.AddDays(1).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_YOKUDATE & "#"
                If iT0007view.Count > 0 Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "1" Then
                        WW_YOKUHOLIDAYKBN = "1"
                    End If

                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "2" Or
                       iT0007view.Item(0).Row("PAYKBN") = "01" Or
                       iT0007view.Item(0).Row("PAYKBN") = "02" Or
                       iT0007view.Item(0).Row("PAYKBN") = "04" Or
                       iT0007view.Item(0).Row("PAYKBN") = "05" Or
                       iT0007view.Item(0).Row("PAYKBN") = "06" Or
                       iT0007view.Item(0).Row("PAYKBN") = "07" Or
                       iT0007view.Item(0).Row("PAYKBN") = "08" Or
                       iT0007view.Item(0).Row("PAYKBN") = "09" Or
                       iT0007view.Item(0).Row("PAYKBN") = "11" Or
                       iT0007view.Item(0).Row("PAYKBN") = "13" Or
                       iT0007view.Item(0).Row("PAYKBN") = "15" Then
                        WW_YOKUHOLIDAYKBN = "2"
                    End If

                    If WW_YOKUHOLIDAYKBN = "1" Or WW_YOKUHOLIDAYKBN = "2" Then
                        If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                            '稼働あり
                            WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                        End If
                    End If
                Else
                    '翌日勤務未入力の場合、カレンダーより（救済）
                    MB005_Select(WW_HEADrow("CAMPCODE"), WW_YOKUDATE, WW_YOKUHOLIDAYKBN, WW_RTN)
                    If WW_RTN <> C_MESSAGE_NO.NORMAL Then
                        'カレンダー取得できず（救済）
                        If Weekday(DateSerial(Year(CDate(WW_YOKUDATE)), Month(CDate(WW_YOKUDATE)), Day(CDate(WW_YOKUDATE)))) = 1 Then
                            '日曜日
                            WW_YOKUHOLIDAYKBN = 1
                        Else
                            '平日
                            WW_YOKUHOLIDAYKBN = 0
                        End If
                    End If
                    WW_YOKUACTTIME = ""
                End If
                'End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) Then
                    WW_STDATETIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                End If
                If IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                    WW_ENDDATETIME = CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME"))
                End If

                '○出社日時、退社日時の計算　★  共通処理(事務員+乗務員)　★
                Dim WW_BINDST As String = WW_HEADrow("STTIME")
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    '・出社を拘束開始とする(拘束時がZERO時、救済措置)
                    If WW_HEADrow("BINDSTDATE") = "" Then
                        If IsDate(WW_HEADrow("STTIME")) Then
                            WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                            WW_BINDST = WW_HEADrow("STTIME")
                        Else
                            WW_HEADrow("BINDSTDATE") = "05:00"
                            WW_BINDST = "05:00"
                        End If
                    End If
                    '・拘束開始5時未満は5時とする
                    If IsDate(WW_HEADrow("BINDSTDATE")) Then
                        If WW_HEADrow("STDATE") < WW_HEADrow("WORKDATE") Then
                            WW_BINDST = "05:00"
                        End If
                        If CDate(WW_BINDST).ToString("HHmm") < "0500" Then
                            WW_BINDST = "05:00"
                        End If
                    End If

                End If

                '●時間算出（拘束開始日時、拘束終了日時）

                '○初期設定　★  共通処理(事務員+乗務員)　★
                Dim WW_BINDSTTIME As DateTime
                Dim WW_BINDENDTIME As DateTime
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & WW_BINDST)
                    WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & CDate(WW_BINDSTTIME.ToString("HH:mm")))
                End If

                '○拘束終了日時の設定　★  事務員処理　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '出社日時＋拘束時間(7:30)＋休憩(通常休憩)　…１時間取らないケース有。????再検討必要
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(HHMMtoMinutes(WW_HEADrow("BREAKTIME")))
                    '2018/02/06 追加
                End If

                '○拘束終了日時の設定　★  乗務員処理　★
                '   　　説明：拘束終了日時　…　実際の休憩を含む拘束終了時間（残業開始時間）
                '             拘束終了時間に休憩が含まれる場合、拘束終了時間を休憩分延長する
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))

                    Dim WW_BBSELTBL As DataTable = New DataTable
                    Dim WW_Filter As String = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_HEADrow("WORKDATE") & "#"

                    CS0026TblSort.TABLE = WW_T0007BBtbl
                    CS0026TblSort.FILTER = WW_Filter
                    CS0026TblSort.SORTING = "STDATE, STTIME, ENDDATE, ENDTIME"
                    WW_BBSELTBL = CS0026TblSort.sort()

                    For i As Integer = 0 To WW_BBSELTBL.Rows.Count - 1
                        Dim WW_BBrow As DataRow = WW_BBSELTBL.Rows(i)
                        Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                        Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                        WW_BINDENDTIME = BindEndTimeGet(WW_BINDSTTIME, WW_BINDENDTIME, WW_STBREAKTIME, WW_ENDBREAKTIME)
                    Next
                End If

                '○稼働時間算出　★  乗務員処理　★
                Dim WW_ACTTIME As Integer = 0
                Dim WW_STACTTIME As Date
                Dim WW_ENDACTTIME As Date
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then
                    WW_STACTTIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                    WW_ENDACTTIME = CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME"))
                    WW_ACTTIME = DateDiff("n", WW_STACTTIME, WW_ENDACTTIME)

                    Dim WW_BBSELTBL As DataTable = New DataTable
                    Dim WW_Filter As String = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_HEADrow("WORKDATE") & "#"

                    CS0026TblSort.TABLE = WW_T0007BBtbl
                    CS0026TblSort.FILTER = WW_Filter
                    CS0026TblSort.SORTING = "STDATE, STTIME, ENDDATE, ENDTIME"
                    WW_BBSELTBL = CS0026TblSort.sort()

                    For i As Integer = 0 To WW_BBSELTBL.Rows.Count - 1
                        Dim WW_BBrow As DataRow = WW_BBSELTBL.Rows(i)
                        Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                        Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                        WW_ENDACTTIME = BindEndTimeGet(WW_STACTTIME, WW_ENDACTTIME, WW_STBREAKTIME, WW_ENDBREAKTIME)
                    Next
                End If
                WW_ACTTIME = WW_ACTTIME - (DateDiff("n", WW_STACTTIME, WW_ENDACTTIME) - WW_ACTTIME)

                '○配送時間、特作時間算出　★  乗務員処理　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then

                    Dim WW_HAISO As Integer = 0
                    Dim WW_STHAISOTIME As Date
                    Dim WW_ENDHAISOTIME As Date
                    Dim WW_ENDHAISOTIME2 As Date
                    Dim WW_LATITUDE_F1 As String = ""
                    Dim WW_LONGITUDE_F1 As String = ""
                    Dim WW_F1 As String = "OFF"
                    Dim WW_G1SELTBL As DataTable = New DataTable
                    Dim WW_Filter As String = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_HEADrow("WORKDATE") & "#"

                    CS0026TblSort.TABLE = WW_T0007G1tbl
                    CS0026TblSort.FILTER = WW_Filter
                    CS0026TblSort.SORTING = "STDATE, STTIME, ENDDATE, ENDTIME"
                    WW_G1SELTBL = CS0026TblSort.sort()

                    For i As Integer = 0 To WW_G1SELTBL.Rows.Count - 1
                        Dim WW_G1row As DataRow = WW_G1SELTBL.Rows(i)
                        If i = 0 Then
                            Dim WW_date As DateTime = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                            Dim WW_date2 As DateTime = CDate(WW_G1row("STDATE") & " " & WW_G1row("STTIME"))
                            If WW_date = WW_date2 Or
                               WW_date = CDate(WW_date2.AddMinutes(-10).ToString("yyyy/MM/dd HH:mm")) Then
                                WW_STHAISOTIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                            Else
                                WW_STHAISOTIME = CDate(WW_G1row("STDATE") & " " & WW_G1row("STTIME"))
                            End If
                        End If
                        WW_ENDHAISOTIME = CDate(WW_G1row("ENDDATE") & " " & WW_G1row("ENDTIME"))
                        WW_ENDHAISOTIME2 = CDate(WW_G1row("ENDDATE") & " " & WW_G1row("ENDTIME"))

                        Dim WW_BBSELTBL As DataTable = New DataTable
                        WW_Filter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_HEADrow("WORKDATE") & "#"

                        CS0026TblSort.TABLE = WW_T0007BBtbl
                        CS0026TblSort.FILTER = WW_Filter
                        CS0026TblSort.SORTING = "STDATE, STTIME, ENDDATE, ENDTIME"
                        WW_BBSELTBL = CS0026TblSort.sort()

                        For j As Integer = 0 To WW_BBSELTBL.Rows.Count - 1
                            Dim WW_BBrow As DataRow = WW_BBSELTBL.Rows(j)
                            Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                            Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                            WW_ENDHAISOTIME2 = BindEndTimeGet(WW_STHAISOTIME, WW_ENDHAISOTIME2, WW_STBREAKTIME, WW_ENDBREAKTIME)
                        Next
                        WW_HAISO += DateDiff("n", WW_STHAISOTIME, WW_ENDHAISOTIME) - DateDiff("n", WW_ENDHAISOTIME, WW_ENDHAISOTIME2)
                    Next
                    WW_HEADrow("HAISOTIME") = formatHHMM(WW_HAISO)

                    '日報を取込だ場合のみ自動計算する（以外は、入力値を有効とするため計算しない
                    If iTokusaKbn = "TOKUSA" Then
                        If CheckHOLIDAY(WW_HEADrow("HOLIDAYKBN"), WW_HEADrow("PAYKBN")) = False Then
                            If WW_ACTTIME >= 450 Then
                                '7:30以上の場合
                                If 450 - WW_HAISO > 0 Then
                                    WW_HEADrow("TOKUSA1TIME") = formatHHMM(450 - WW_HAISO)
                                    WW_HEADrow("TOKUSA1TIMETTL") = WW_HEADrow("TOKUSA1TIME")
                                End If
                            Else
                                If WW_ACTTIME - WW_HAISO > 0 Then
                                    WW_HEADrow("TOKUSA1TIME") = formatHHMM(WW_ACTTIME - WW_HAISO)
                                    WW_HEADrow("TOKUSA1TIMETTL") = WW_HEADrow("TOKUSA1TIME")
                                End If
                            End If
                        Else
                            '休みの場合は、グループ作業（特作）計算しない
                            WW_HEADrow("TOKUSA1TIME") = formatHHMM(0)
                            WW_HEADrow("TOKUSA1TIMETTL") = WW_HEADrow("TOKUSA1TIME")
                        End If
                    End If
                End If

                '●時間算出（所定内通常分_作業、所定内深夜分_作業、所定内深夜分2_作業、所定外通常分_作業、所定外深夜分_作業、休日通常分_作業、休日深夜分_作業、休日深夜分2_作業）
                Dim WK_WORKTIME_SAGYO As Integer = 0         '平日＆所定内＆深夜以外（当日分）
                Dim WK_WORKTIME_SAGYO2 As Integer = 0        '平日＆所定内＆深夜以外（翌日分）
                Dim WK_NIGHTTIME_SAGYO As Integer = 0        '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_SAGYO As Integer = 0    '平日＆所定内＆深夜　　（24:00～29:00）
                Dim WK_YOKU0to5NIGHT_SAGYO2 As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_SAGYO As Integer = 0      '平日＆所定外＆深夜以外（当日分）
                Dim WK_OUTWORKTIME_SAGYO2 As Integer = 0     '平日＆所定外＆深夜以外（翌日分）
                Dim WK_OUTNIGHTTIME_SAGYO As Integer = 0     '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_SAGYO As Integer = 0        '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_SAGYO As Integer = 0       '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_SAGYO2 As Integer = 0      '休日＆残業　＆深夜     (24:00～29:00)

                '休憩時間
                Dim WK_WORKTIME_KYUKEI As Integer = 0        '平日＆所定内＆深夜以外
                Dim WK_WORKTIME_KYUKEI2 As Integer = 0       '平日＆所定内＆深夜以外
                Dim WK_NIGHTTIME_KYUKEI As Integer = 0       '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_KYUKEI As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_YOKU0to5NIGHT_KYUKEI2 As Integer = 0  '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_KYUKEI As Integer = 0     '平日＆所定外＆深夜以外
                Dim WK_OUTWORKTIME_KYUKEI2 As Integer = 0    '平日＆所定外＆深夜以外
                Dim WK_OUTNIGHTTIME_KYUKEI As Integer = 0    '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_KYUKEI As Integer = 0       '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_KYUKEI As Integer = 0      '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_KYUKEI2 As Integer = 0     '休日＆残業　＆深夜     (24:00～29:00)

                Dim WW_累積分 As Integer = 0

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    ' In  : WW_STDATETIME             出社日時
                    '       WW_BINDSTTIME             拘束開始日時
                    '       WW_BINDENDTIME            拘束終了日時
                    '       0                         休日区分 = 0(固定)
                    '       WW_STDATETIME             出社日時
                    '       WW_ENDDATETIME            退社日時
                    ' Out : WK_WORKTIME_SAGYO         5:00～22:00（所定内通常）
                    '       WK_WORKTIME_SAGYO2        翌5:00～22:00（所定内通常）    
                    '       WK_NIGHTTIME_SAGYO        22:00～24:00（深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO    翌0:00～5:00（所定内深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO2   翌0:00～5:00（深夜）
                    '       WK_OUTWORKTIME_SAGYO      5:00～22:00（残業）　　← 法定、法定外休日のみ
                    '       WK_OUTWORKTIME_SAGYO2     翌5:00～22:00（残業）
                    '       WK_OUTNIGHTTIME_SAGYO     0:00～5:00（5時前深夜）
                    '       WK_HWORKTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO2,
                    '       WW_累積分
                    Call NightTimeMinuteGet(WW_STDATETIME,
                                            WW_BINDSTTIME,
                                            WW_BINDENDTIME,
                                            0,
                                            WW_STDATETIME,
                                            WW_ENDDATETIME,
                                            WK_WORKTIME_SAGYO,
                                            WK_WORKTIME_SAGYO2,
                                            WK_NIGHTTIME_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO2,
                                            WK_OUTWORKTIME_SAGYO,
                                            WK_OUTWORKTIME_SAGYO2,
                                            WK_OUTNIGHTTIME_SAGYO,
                                            WK_HWORKTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO2,
                                            WW_累積分)
                End If

                '○休憩時間計算　★  事務員処理　★
                If Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WK_WORKTIME_KYUKEI = HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                    '--------------------------------------------------------------------
                    '2018/02/06 追加
                End If

                '○休憩時間計算　★  乗務員処理　★
                If WW_HEADrow("STAFFKBN") Like "03*" Then
                    Dim WW_MATCH As String = "OFF"
                    For i As Integer = WW_IDX To WW_T0007BBtbl.Rows.Count - 1
                        Dim WW_BBrow As DataRow = WW_T0007BBtbl.Rows(i)
                        If WW_BBrow("STAFFCODE") = WW_HEADrow("STAFFCODE") And
                           WW_BBrow("WORKDATE") = WW_HEADrow("WORKDATE") Then
                            ' In  : WW_STDATETIME             出社日時
                            '       WW_BINDSTTIME             拘束開始日時
                            '       WW_BINDENDTIME            拘束終了日時
                            '       0                         休日区分 = 0(固定)
                            '       WW_STDATETIME             出社日時
                            '       WW_ENDDATETIME            退社日時
                            ' Out : WK_NIGHTTIME_KYUKEI       5:00～22:00（所定内通常）
                            '       WK_WORKTIME_KYUKEI2       翌5:00～22:00（所定内通常）    
                            '       WK_NIGHTTIME_KYUKEI       22:00～24:00（深夜）
                            '       WK_YOKU0to5NIGHT_KYUKEI   翌0:00～5:00（所定内深夜）
                            '       WK_YOKU0to5NIGHT_SAGYO2   翌0:00～5:00（深夜）
                            '       WK_OUTWORKTIME_KYUKEI     5:00～22:00（残業）　　← 法定、法定外休日のみ
                            '       WK_OUTWORKTIME_KYUKEI2    翌5:00～22:00（残業）
                            '       WK_OUTNIGHTTIME_KYUKEI    0:00～5:00（5時前深夜）
                            '       WK_HWORKTIME_KYUKEI,
                            '       WK_HNIGHTTIME_KYUKEI,
                            '       WK_HNIGHTTIME_KYUKEI2,
                            '       WW_累積分
                            Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                            Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                            Call NightTimeMinuteGet(WW_STDATETIME,
                                                    WW_BINDSTTIME,
                                                    WW_BINDENDTIME,
                                                    0,
                                                    WW_STBREAKTIME,
                                                    WW_ENDBREAKTIME,
                                                    WK_WORKTIME_KYUKEI,
                                                    WK_WORKTIME_KYUKEI2,
                                                    WK_NIGHTTIME_KYUKEI,
                                                    WK_YOKU0to5NIGHT_KYUKEI,
                                                    WK_YOKU0to5NIGHT_SAGYO2,
                                                    WK_OUTWORKTIME_KYUKEI,
                                                    WK_OUTWORKTIME_KYUKEI2,
                                                    WK_OUTNIGHTTIME_KYUKEI,
                                                    WK_HWORKTIME_KYUKEI,
                                                    WK_HNIGHTTIME_KYUKEI,
                                                    WK_HNIGHTTIME_KYUKEI2,
                                                    WW_累積分)
                            WW_MATCH = "ON"
                        Else
                            If WW_MATCH = "ON" Then
                                WW_IDX = i
                                Exit For
                            End If
                        End If
                    Next
                End If
                '************************************************************
                '*   残業設定                                               *
                '************************************************************
                '○異常事態の救済　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    '・拘束開始前に退社の場合
                    If DateDiff("n", WW_BINDSTTIME, WW_ENDDATETIME) < 0 Then
                        WW_BINDSTTIME = WW_STDATETIME
                    End If
                End If

                '○マイナス時間クリア
                Dim WW_ORVERTIME As Integer = 0   '平日残業時
                Dim WW_WNIGHTTIME As Integer = 0  '平日深夜時
                Dim WW_NIGHTTIME As Integer = 0   '所定内深夜時
                Dim WW_HWORKTIME As Integer = 0   '休日出勤時
                Dim WW_HNIGHTTIME As Integer = 0  '休日深夜時
                Dim WW_SWORKTIME As Integer = 0   '日曜出勤時
                Dim WW_SNIGHTTIME As Integer = 0  '日曜深夜時

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    Select Case WW_HEADrow("HOLIDAYKBN")
                        '○平日
                        Case "0"
                            '平日残業
                            WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI - WK_OUTWORKTIME_KYUKEI2             ' 平日残業時

                            '所定内深夜
                            WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO - WK_NIGHTTIME_KYUKEI - WK_YOKU0to5NIGHT_KYUKEI               ' 所定内深夜時

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                '深夜時間(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WK_OUTNIGHTTIME_KYUKEI - WK_YOKU0to5NIGHT_KYUKEI2  ' 平日深夜時
                            End If

                            '翌日日曜日の場合
                            If WW_YOKUHOLIDAYKBN = "1" Then
                                If WW_YOKUACTTIME = "" Then
                                    '稼働なし、深夜時間(24:00～翌5:00)
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WK_OUTNIGHTTIME_KYUKEI       ' 平日深夜時

                                    WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2   ' 日曜深夜時
                                Else
                                    '稼働あり、深夜時間(24:00～翌5:00)
                                    '深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WK_OUTNIGHTTIME_KYUKEI - WK_YOKU0to5NIGHT_KYUKEI2   ' 平日深夜時
                                End If

                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                If WW_YOKUACTTIME = "" Then
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WK_OUTNIGHTTIME_KYUKEI    ' 平日深夜時

                                    '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2   ' 休日深夜時
                                Else
                                    '稼働あり 深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WK_OUTNIGHTTIME_KYUKEI - WK_YOKU0to5NIGHT_KYUKEI2    ' 平日深夜時
                                End If

                            End If


                        Case "1"
                            '○法定休日（日曜）出勤

                            WW_SWORKTIME = WK_OUTWORKTIME_SAGYO - WK_OUTWORKTIME_KYUKEI      ' 日曜出勤時

                            WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WK_OUTNIGHTTIME_KYUKEI - WK_NIGHTTIME_KYUKEI    ' 日曜深夜時

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                WW_WNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2     ' 平日深夜時

                                WW_ORVERTIME = WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI2        ' 平日残業

                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                WW_HWORKTIME = WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI2        ' 休日出勤時

                                If WW_YOKUACTTIME = "" Then
                                    '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2   ' 休日深夜時
                                Else
                                    '稼働あり 日曜深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    WW_SNIGHTTIME = WW_SNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2  ' 日曜深夜時
                                End If
                            End If

                        Case "2"
                            '○法定外休日（祝日、会社指定休日）
                            ' 休日出勤時
                            WW_HWORKTIME = WK_OUTWORKTIME_SAGYO - WK_OUTWORKTIME_KYUKEI    ' 休日出勤時

                            ' 休日深夜時
                            WW_HNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WK_OUTNIGHTTIME_KYUKEI - WK_NIGHTTIME_KYUKEI    ' 休日深夜時

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                ' 休日出勤時
                                WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI2  ' 休日出勤時

                                ' 休日深夜時
                                WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2  ' 休日深夜時
                            End If


                            '翌日日曜日の場合
                            If WW_YOKUHOLIDAYKBN = "1" Then
                                If WW_YOKUACTTIME = "" Then
                                    '稼働なし、日曜深夜(24:00～翌5:00)
                                    ' 日曜出勤時
                                    WW_SWORKTIME = WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI2      ' 日曜出勤時

                                    ' 日曜深夜時
                                    WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2   ' 日曜深夜時
                                Else
                                    '稼働あり、休日深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    ' 休日出勤時
                                    WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI2  ' 休日出勤時

                                    ' 休日深夜時
                                    WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2    ' 休日深夜時
                                End If
                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                ' 休日出勤時
                                WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WK_OUTWORKTIME_KYUKEI2      ' 休日出勤時

                                ' 休日深夜時
                                WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WK_YOKU0to5NIGHT_KYUKEI2  ' 休日深夜時
                            End If

                    End Select

                End If

                '************************************************************
                '*   マイナス時間クリア                                     *
                '************************************************************


                '○マイナス時間クリア
                '平日残業時
                If WW_ORVERTIME < 0 Then
                    WW_ORVERTIME = 0
                    WW_HEADrow("ORVERTIME") = "00:00"
                Else
                    WW_HEADrow("ORVERTIME") = formatHHMM(WW_ORVERTIME)
                End If
                WW_HEADrow("ORVERTIMETTL") = formatHHMM(WW_ORVERTIME + HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO")))
                '平日深夜時
                If WW_WNIGHTTIME < 0 Then
                    WW_WNIGHTTIME = 0
                    WW_HEADrow("WNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("WNIGHTTIME") = formatHHMM(WW_WNIGHTTIME)
                End If
                WW_HEADrow("WNIGHTTIMETTL") = formatHHMM(WW_WNIGHTTIME + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO")))
                '所定内深夜時
                If WW_NIGHTTIME < 0 Then
                    WW_NIGHTTIME = 0
                    WW_HEADrow("NIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("NIGHTTIME") = formatHHMM(WW_NIGHTTIME)
                End If
                WW_HEADrow("NIGHTTIMETTL") = formatHHMM(WW_NIGHTTIME + HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO")))
                '休日出勤時
                If WW_HWORKTIME < 0 Then
                    WW_HWORKTIME = 0
                    WW_HEADrow("HWORKTIME") = "00:00"
                Else
                    WW_HEADrow("HWORKTIME") = formatHHMM(WW_HWORKTIME)
                End If
                WW_HEADrow("HWORKTIMETTL") = formatHHMM(WW_HWORKTIME + HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO")))
                '休日深夜時
                If WW_HNIGHTTIME < 0 Then
                    WW_HNIGHTTIME = 0
                    WW_HEADrow("HNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("HNIGHTTIME") = formatHHMM(WW_HNIGHTTIME)
                End If
                WW_HEADrow("HNIGHTTIMETTL") = formatHHMM(WW_HNIGHTTIME + HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO")))
                '日曜出勤時
                If WW_SWORKTIME < 0 Then
                    WW_SWORKTIME = 0
                    WW_HEADrow("SWORKTIME") = "00:00"
                Else
                    WW_HEADrow("SWORKTIME") = formatHHMM(WW_SWORKTIME)
                End If
                WW_HEADrow("SWORKTIMETTL") = formatHHMM(WW_SWORKTIME + HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO")))
                '日曜深夜時
                If WW_SNIGHTTIME < 0 Then
                    WW_SNIGHTTIME = 0
                    WW_HEADrow("SNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("SNIGHTTIME") = formatHHMM(WW_SNIGHTTIME)
                End If
                WW_HEADrow("SNIGHTTIMETTL") = formatHHMM(WW_SNIGHTTIME + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO")))

                'WW_HEADrow("STATUS") = ""
                WW_HEADrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing
            WW_T0007BBtbl.Dispose()
            WW_T0007BBtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_KintaiCalc"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub
    ' ***  残業計算（ＪＫトランス専用）
    Public Sub T0007_KintaiCalc_JKT(ByRef ioTbl As DataTable, ByRef iTbl As DataTable)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Dim WW_LINECNT As Integer = 0
        Dim WW_IDX As Integer = 0
        Dim WW_IDX2 As Integer = 0
        Dim WW_IDX3 As Integer = 0
        Dim WW_CONVERT As String = ""
        Dim WW_RTN As String = ""

        Try
            '削除レコードを取得
            Dim WW_T0007DELtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DELtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'H'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl = CS0026TblSort.sort()

            '勤怠の明細レコードを取得
            Dim WW_T0007DTLtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and HDKBN = 'D'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007DTLtbl = CS0026TblSort.sort()

            '休憩レコードを取得
            Dim WW_T0007BBtbl As DataTable = New DataTable
            CS0026TblSort.TABLE = ioTbl
            CS0026TblSort.FILTER = "SELECT = '1' and WORKKBN = 'BB' "
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007BBtbl = CS0026TblSort.sort()

            '勤怠のヘッダレコードを取得
            Dim WW_T0007HEADtbl2 As DataTable = New DataTable
            CS0026TblSort.TABLE = iTbl
            CS0026TblSort.FILTER = "HDKBN = 'H' and RECODEKBN = '0' and DELFLG = '0'"
            CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"
            WW_T0007HEADtbl2 = CS0026TblSort.sort()

            '直前、翌日取得用VIEW
            Dim iT0007view As DataView
            iT0007view = New DataView(WW_T0007HEADtbl2)
            iT0007view.Sort = "STAFFCODE, WORKDATE, STDATE, STTIME, ENDDATE, ENDTIME"

            WW_IDX = 0
            For Each WW_HEADrow As DataRow In WW_T0007HEADtbl.Rows
                'STATUS<>''（勤怠に変更が発生しているレコード）
                If WW_HEADrow("RECODEKBN") = "0" Then
                Else
                    Continue For
                End If

                '************************************************************
                '*   勤怠日数設定                                           *
                '************************************************************
                NissuItem_Init(WW_HEADrow)
                Select Case WW_HEADrow("PAYKBN")
                    Case "00"
                        '○勤怠区分(00:通常) …　出勤扱い(所労=1 )
                        If WW_HEADrow("HOLIDAYKBN") = "0" Then
                        End If
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                '2018/02/01 追加
                                If WW_HEADrow("STTIME") = "00:00" And WW_HEADrow("ENDTIME") = "00:00" Then
                                Else
                                    WW_HEADrow("NENSHINISSU") = 1    '年始出勤日数
                                    WW_HEADrow("NENSHINISSUTTL") = 1 '年始出勤日数
                                End If
                                '2018/02/01 追加
                            End If
                        End If
                    Case "01"
                        '○勤怠区分(01:年休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("NENKYUNISSU") = 1        '年次有給休暇
                            WW_HEADrow("NENKYUNISSUTTL") = 1     '年次有給休暇
                        End If
                    Case "02"
                        '○勤怠区分(2:特休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("TOKUKYUNISSU") = 1       '特別有給休暇
                            WW_HEADrow("TOKUKYUNISSUTTL") = 1    '特別有給休暇
                        End If
                    Case "03"
                        '○勤怠区分(3:遅刻早退) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("CHIKOKSOTAINISSU") = 1    '遅刻早退日数
                            WW_HEADrow("CHIKOKSOTAINISSUTTL") = 1 '遅刻早退日数
                        End If
                    Case "04"
                        '○勤怠区分(4:ｽﾄｯｸ休暇) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("STOCKNISSU") = 1         'ストック休暇日数
                            WW_HEADrow("STOCKNISSUTTL") = 1      'ストック休暇日数
                        End If
                    Case "05"
                        '○勤怠区分(5:協約週休) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KYOTEIWEEKNISSU") = 1     '協定週休日数
                            WW_HEADrow("KYOTEIWEEKNISSUTTL") = 1  '協定週休日数
                        End If
                    Case "06"
                        '○勤怠区分(6:協約外週休) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("WEEKNISSU") = 1          '週休日数
                            WW_HEADrow("WEEKNISSUTTL") = 1       '週休日数
                        End If
                    Case "07"
                        '○勤怠区分(7:傷欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("SHOUKETUNISSU") = 1      '傷欠勤日数
                            WW_HEADrow("SHOUKETUNISSUTTL") = 1   '傷欠勤日数
                        End If
                    Case "08"
                        '○勤怠区分(8:組欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("KUMIKETUNISSU") = 1      '組合欠勤日数
                            WW_HEADrow("KUMIKETUNISSUTTL") = 1   '組合欠勤日数
                        End If
                    Case "09"
                        '○勤怠区分(9:他欠) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("ETCKETUNISSU") = 1       'その他欠勤日数
                            WW_HEADrow("ETCKETUNISSUTTL") = 1    'その他欠勤日数
                        End If
                    Case "10"
                        '○勤怠区分(10:代休出勤) …　出勤外扱い(所労=0 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        End If
                    Case "11"
                        '○勤怠区分(11:代休取得) …　出勤扱い(所労=1 )
                        '休日の年休取得は勤怠区分(=0)とする
                        If WW_HEADrow("HOLIDAYKBN") <> "0" Then
                            WW_HEADrow("PAYKBN") = "00"
                            WW_HEADrow("PAYKBNNAMES") = "通常"
                        Else
                            WW_HEADrow("DAIKYUNISSU") = 1        '代休取得日数
                            WW_HEADrow("DAIKYUNISSUTTL") = 1     '代休取得日数
                        End If
                    Case "12"
                        '○勤怠区分(12:年始出勤取得) …　出勤扱い(所労=1 )
                        WW_HEADrow("NENSHINISSU") = 1            '年始出勤日数
                        WW_HEADrow("NENSHINISSUTTL") = 1         '年始出勤日数
                End Select

                '************************************************************
                '*   宿日直設定                                             *
                '************************************************************
                Select Case WW_HEADrow("SHUKCHOKKBN")
                    Case "0"
                        '○宿日直区分(0:なし)
                        WW_HEADrow("SHUKCHOKNNISSU") = 0             '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNNISSUTTL") = 0          '宿日直年始日数
                        WW_HEADrow("SHUKCHOKNISSU") = 0              '宿日直通常日数
                        WW_HEADrow("SHUKCHOKNISSUTTL") = 0           '宿日直通常日数
                        '2018/02/08 追加
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSU") = 0          '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0       '宿直年末年始(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
                            WW_HEADrow("SHUKCHOKHLDNISSU") = 0           '宿直(翌日休み)
                        End If
                        If WW_HEADrow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") Then
                            WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0        '宿直(翌日休み)
                        End If
                        '2018/02/08 追加


                    Case "1", "2"
                        '○宿日直区分(1:日直、2:宿直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 1    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 1     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1  '宿日直通常日数
                            End If
                        End If

                    Case "3"
                        '○宿日直区分(3:宿日直)
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNNISSU") = 2    '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 2 '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKNISSU") = 2     '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 2  '宿日直通常日数
                            End If
                        End If

                    Case "4"
                        '○宿日直区分(4:宿直(翌日休み)／宿直(割増有り))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 1    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 1 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 0       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 0    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 1     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 1  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 0        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 0     '宿日直通常日数
                            End If
                        End If

                    Case "5"
                        '○宿日直区分(5:宿直(翌日営業)／宿直(割増無し))
                        If IsDate(WW_HEADrow("STDATE")) Then
                            If CDate(WW_HEADrow("STDATE")).Month = 1 And CDate(WW_HEADrow("STDATE")).Day < 4 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            ElseIf CDate(WW_HEADrow("STDATE")).Month = 12 And CDate(WW_HEADrow("STDATE")).Day = 31 Then
                                WW_HEADrow("SHUKCHOKNHLDNISSU") = 0    '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNHLDNISSUTTL") = 0 '宿直年末年始(翌日休み)
                                WW_HEADrow("SHUKCHOKNNISSU") = 1       '宿日直年始日数
                                WW_HEADrow("SHUKCHOKNNISSUTTL") = 1    '宿日直年始日数
                            Else
                                WW_HEADrow("SHUKCHOKHLDNISSU") = 0     '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKHLDNISSUTTL") = 0  '宿直(翌日休み)
                                WW_HEADrow("SHUKCHOKNISSU") = 1        '宿日直通常日数
                                WW_HEADrow("SHUKCHOKNISSUTTL") = 1     '宿日直通常日数
                            End If
                        End If
                End Select


                '************************************************************
                '*   車中泊設定                                             *
                '************************************************************
                Select Case WW_HEADrow("SHACHUHAKKBN")
                    Case "1"
                        WW_HEADrow("SHACHUHAKNISSU") = 1    '車中泊（１日目）
                        WW_HEADrow("SHACHUHAKNISSUTTL") = 1 '車中泊（１日目）
                End Select

                '************************************************************
                '*   勤怠時間設定                                           *
                '************************************************************
                '   前提：出勤時刻は、当日0時から21時59分まで
                ' 　    ：退社時刻は、翌日5時まで
                '○退社日が出社当日～翌日 and 出社日時 < 退社日時 のみ時間計算を行う
                '以降処理で判定用(出社日時、退社日時)を算出
                Dim WW_STDATETIME As Date
                Dim WW_ENDDATETIME As Date

                '出社、退社が未入力の場合、残業計算しない
                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) And
                   IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                Else
                    Continue For
                End If

                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                '直前および、翌日の勤務状況取得
                Dim WW_YOKUHOLIDAYKBN As String = ""
                Dim WW_YOKUACTTIME As String = ""

                'If WW_HEADrow("STAFFKBN") Like "03*" Then

                Dim dt As Date = CDate(WW_HEADrow("WORKDATE"))


                '翌日の勤務
                WW_YOKUHOLIDAYKBN = "0"
                Dim WW_YOKUDATE As String = dt.AddDays(1).ToString("yyyy/MM/dd")
                iT0007view.RowFilter = "STAFFCODE = '" & WW_HEADrow("STAFFCODE") & "' and WORKDATE = #" & WW_YOKUDATE & "#"
                If iT0007view.Count > 0 Then
                    '1:法定休日、2:法定外休日
                    '01:年休, 02 : 特休, 04 : ｽﾄｯｸ, 05 : 協約週休, 06 : 週休
                    '07:傷欠, 08 : 組欠, 09 : 他欠, 11 : 代休, 13 : 指定休, 15 : 振休
                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "1" Then
                        WW_YOKUHOLIDAYKBN = "1"
                    End If

                    If iT0007view.Item(0).Row("HOLIDAYKBN") = "2" Or
                       iT0007view.Item(0).Row("PAYKBN") = "01" Or
                       iT0007view.Item(0).Row("PAYKBN") = "02" Or
                       iT0007view.Item(0).Row("PAYKBN") = "04" Or
                       iT0007view.Item(0).Row("PAYKBN") = "05" Or
                       iT0007view.Item(0).Row("PAYKBN") = "06" Or
                       iT0007view.Item(0).Row("PAYKBN") = "07" Or
                       iT0007view.Item(0).Row("PAYKBN") = "08" Or
                       iT0007view.Item(0).Row("PAYKBN") = "09" Or
                       iT0007view.Item(0).Row("PAYKBN") = "11" Or
                       iT0007view.Item(0).Row("PAYKBN") = "13" Or
                       iT0007view.Item(0).Row("PAYKBN") = "15" Then
                        WW_YOKUHOLIDAYKBN = "2"
                    End If

                    '************************************************************
                    '*   一般（新潟東港以外）                                   *
                    '************************************************************
                    If WW_YOKUHOLIDAYKBN = "1" Or WW_YOKUHOLIDAYKBN = "2" Then
                        If Val(iT0007view.Item(0).Row("ACTTIME")) > 0 Then
                            '稼働あり
                            WW_YOKUACTTIME = iT0007view.Item(0).Row("ACTTIME")
                        End If
                    End If
                Else
                    '翌日勤務未入力の場合、カレンダーより（救済）
                    MB005_Select(WW_HEADrow("CAMPCODE"), WW_YOKUDATE, WW_YOKUHOLIDAYKBN, WW_RTN)
                    If WW_RTN <> C_MESSAGE_NO.NORMAL Then
                        'カレンダー取得できず（救済）
                        If Weekday(DateSerial(Year(CDate(WW_YOKUDATE)), Month(CDate(WW_YOKUDATE)), Day(CDate(WW_YOKUDATE)))) = 1 Then
                            '日曜日
                            WW_YOKUHOLIDAYKBN = 1
                        Else
                            '平日
                            WW_YOKUHOLIDAYKBN = 0
                        End If
                    End If
                    WW_YOKUACTTIME = ""
                End If
                'End If
                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


                If IsDate(WW_HEADrow("STDATE")) And IsDate(WW_HEADrow("STTIME")) Then
                    WW_STDATETIME = CDate(WW_HEADrow("STDATE") & " " & WW_HEADrow("STTIME"))
                End If
                If IsDate(WW_HEADrow("ENDDATE")) And IsDate(WW_HEADrow("ENDTIME")) Then
                    WW_ENDDATETIME = CDate(WW_HEADrow("ENDDATE") & " " & WW_HEADrow("ENDTIME"))
                End If

                '○出社日時、退社日時の計算　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    '・出社を拘束開始とする(拘束時がZERO時、救済措置)
                    If WW_HEADrow("BINDSTDATE") = "" Then
                        If IsDate(WW_HEADrow("STTIME")) Then
                            WW_HEADrow("BINDSTDATE") = WW_HEADrow("STTIME")
                        Else
                            WW_HEADrow("BINDSTDATE") = "03:00"
                        End If
                    End If
                    '・拘束開始5時未満は5時とする
                    If IsDate(WW_HEADrow("BINDSTDATE")) Then
                        If WW_HEADrow("STDATE") < WW_HEADrow("WORKDATE") Then
                            WW_HEADrow("BINDSTDATE") = "03:00"
                        End If
                        If CDate(WW_HEADrow("BINDSTDATE")).ToString("HHmm") < "0300" Then
                            WW_HEADrow("BINDSTDATE") = "03:00"
                        End If
                    End If

                End If

                '●時間算出（拘束開始日時、拘束終了日時）

                '○初期設定　★  共通処理(事務員+乗務員)　★
                Dim WW_BINDSTTIME As DateTime
                Dim WW_BINDENDTIME As DateTime
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    WW_BINDSTTIME = CDate(WW_HEADrow("WORKDATE") & " " & WW_HEADrow("BINDSTDATE"))
                    WW_BINDENDTIME = CDate(WW_BINDSTTIME.ToString("yyyy/MM/dd") & " " & CDate(WW_HEADrow("BINDSTDATE")).ToString("HH:mm"))
                End If

                '○拘束終了日時の設定　★  事務員処理　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '出社日時＋拘束時間(7:30)＋休憩(通常休憩)　…１時間取らないケース有。????再検討必要
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(HHMMtoMinutes(WW_HEADrow("BREAKTIME")))
                    '2018/02/06 追加
                End If

                '○拘束終了日時の設定　★  乗務員処理　★
                '   　　説明：拘束終了日時　…　実際の休憩を含む拘束終了時間（残業開始時間）
                '             拘束終了時間に休憩が含まれる場合、拘束終了時間を休憩分延長する
                Dim WW_BREAKTIMETTL As Integer = 0
                Dim WW_BREAKTIMEZAN As Integer = 0
                Dim WW_MIN As Integer = 0
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME And WW_HEADrow("STAFFKBN") Like "03*" Then
                    WW_BREAKTIMETTL = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    If WW_BREAKTIMETTL > 60 Then
                        WW_BREAKTIMEZAN = WW_BREAKTIMETTL - 60
                        WW_MIN = 60
                    Else
                        WW_BREAKTIMEZAN = 0
                        WW_MIN = WW_BREAKTIMETTL
                    End If
                    WW_BINDENDTIME = WW_BINDSTTIME
                    WW_BINDENDTIME = WW_BINDENDTIME.AddHours(CDate(WW_HEADrow("BINDTIME")).ToString("HH"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(CDate(WW_HEADrow("BINDTIME")).ToString("mm"))
                    WW_BINDENDTIME = WW_BINDENDTIME.AddMinutes(WW_MIN)
                End If

                '●時間算出（所定内通常分_作業、所定内深夜分_作業、所定内深夜分2_作業、所定外通常分_作業、所定外深夜分_作業、休日通常分_作業、休日深夜分_作業、休日深夜分2_作業）
                Dim WK_WORKTIME_SAGYO As Integer = 0         '平日＆所定内＆深夜以外（当日分）
                Dim WK_WORKTIME_SAGYO2 As Integer = 0        '平日＆所定内＆深夜以外（翌日分）
                Dim WK_NIGHTTIME_SAGYO As Integer = 0        '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_SAGYO As Integer = 0    '平日＆所定内＆深夜　　（24:00～29:00）
                Dim WK_YOKU0to5NIGHT_SAGYO2 As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_SAGYO As Integer = 0      '平日＆所定外＆深夜以外（当日分）
                Dim WK_OUTWORKTIME_SAGYO2 As Integer = 0     '平日＆所定外＆深夜以外（翌日分）
                Dim WK_OUTNIGHTTIME_SAGYO As Integer = 0     '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_SAGYO As Integer = 0        '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_SAGYO As Integer = 0       '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_SAGYO2 As Integer = 0      '休日＆残業　＆深夜     (24:00～29:00)

                '休憩時間
                Dim WK_WORKTIME_KYUKEI As Integer = 0        '平日＆所定内＆深夜以外
                Dim WK_NIGHTTIME_KYUKEI As Integer = 0       '平日＆所定内＆深夜　　（0:00～5:00 + 22:00～24:00 + 46:00～48:00）
                Dim WK_YOKU0to5NIGHT_KYUKEI As Integer = 0   '平日＆所定外＆深夜　　（24:00～29:00）
                Dim WK_OUTWORKTIME_KYUKEI As Integer = 0     '平日＆所定外＆深夜以外
                Dim WK_OUTNIGHTTIME_KYUKEI As Integer = 0    '平日＆所定外＆深夜　　（0:00～5:00 + 22:00～29:00 + 46:00～48:00）
                Dim WK_HWORKTIME_KYUKEI As Integer = 0       '休日＆残業　＆深夜以外
                Dim WK_HNIGHTTIME_KYUKEI As Integer = 0      '休日＆残業　＆深夜     (0:00～5:00 + 22:00～24:00 + 46:00～48:00)
                Dim WK_HNIGHTTIME_KYUKEI2 As Integer = 0     '休日＆残業　＆深夜     (24:00～29:00)

                Dim WW_累積分 As Integer = 0

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    ' In  : WW_STDATETIME             出社日時
                    '       WW_BINDSTTIME             拘束開始日時
                    '       WW_BINDENDTIME            拘束終了日時
                    '       0                         休日区分 = 0(固定)
                    '       WW_STDATETIME             出社日時
                    '       WW_ENDDATETIME            退社日時
                    ' Out : WK_WORKTIME_SAGYO         5:00～22:00（所定内通常）
                    '       WK_WORKTIME_SAGYO2        翌5:00～22:00（所定内通常）    
                    '       WK_NIGHTTIME_SAGYO        22:00～24:00（深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO    翌0:00～5:00（所定内深夜）
                    '       WK_YOKU0to5NIGHT_SAGYO2   翌0:00～5:00（深夜）
                    '       WK_OUTWORKTIME_SAGYO      5:00～22:00（残業）　　← 法定、法定外休日のみ
                    '       WK_OUTWORKTIME_SAGYO2     翌5:00～22:00（残業）
                    '       WK_OUTNIGHTTIME_SAGYO     0:00～5:00（5時前深夜）
                    '       WK_HWORKTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO,
                    '       WK_HNIGHTTIME_SAGYO2,
                    '       WW_累積分
                    Call NightTimeMinuteGet(WW_STDATETIME,
                                            WW_BINDSTTIME,
                                            WW_BINDENDTIME,
                                            0,
                                            WW_STDATETIME,
                                            WW_ENDDATETIME,
                                            WK_WORKTIME_SAGYO,
                                            WK_WORKTIME_SAGYO2,
                                            WK_NIGHTTIME_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO,
                                            WK_YOKU0to5NIGHT_SAGYO2,
                                            WK_OUTWORKTIME_SAGYO,
                                            WK_OUTWORKTIME_SAGYO2,
                                            WK_OUTNIGHTTIME_SAGYO,
                                            WK_HWORKTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO,
                                            WK_HNIGHTTIME_SAGYO2,
                                            WW_累積分)
                End If

                '○休憩時間計算　★  事務員処理　★
                If Not WW_HEADrow("STAFFKBN") Like "03*" Then
                    '2018/02/06 追加
                    '入力値（休憩）で計算する
                    WK_WORKTIME_KYUKEI = HHMMtoMinutes(WW_HEADrow("BREAKTIME"))
                    '--------------------------------------------------------------------
                    '2018/02/06 追加
                End If

                '○休憩時間計算　★  乗務員処理　★
                If WW_HEADrow("STAFFKBN") Like "03*" Then
                    Dim WW_BREAKTIME As Integer = HHMMtoMinutes(WW_HEADrow("BREAKTIME")) + HHMMtoMinutes(WW_HEADrow("NIPPOBREAKTIME"))
                    WK_WORKTIME_KYUKEI = WW_BREAKTIME
                    'If WW_HEADrow("HOLIDAYKBN") = 0 Then
                    '    WK_WORKTIME_KYUKEI = WW_BREAKTIME
                    'Else
                    '    WK_HWORKTIME_KYUKEI = WW_BREAKTIME
                    'End If
                    'Dim WW_MATCH As String = "OFF"
                    'For i As Integer = WW_IDX To WW_T0007BBtbl.Rows.Count - 1
                    '    Dim WW_BBrow As DataRow = WW_T0007BBtbl.Rows(i)
                    '    If WW_BBrow("STAFFCODE") = WW_HEADrow("STAFFCODE") And
                    '       WW_BBrow("WORKDATE") = WW_HEADrow("WORKDATE") Then
                    '        ' In  : WK_出社日時、WW_休憩開始日時、WW_休憩終了日時
                    '        ' Out : WK_WORKTIME_KYUKEI、WK_NIGHTTIME_KYUKEI、WK_YOKU0to5NIGHT_KYUKEI、WK_OUTWORKTIME_KYUKEI、WK_OUTNIGHTTIME_KYUKEI、
                    '        '       WK_HWORKTIME_KYUKEI、WK_HNIGHTTIME_KYUKEI、WK_HNIGHTTIME_KYUKEI2　←休日用（未使用：休日区分=0とするため）
                    '        ' 参照: WK_拘束開始日時 、WK_拘束終了日時
                    '        Dim WW_STBREAKTIME As Date = CDate(WW_BBrow("STDATE") & " " & WW_BBrow("STTIME"))
                    '        Dim WW_ENDBREAKTIME As Date = CDate(WW_BBrow("ENDDATE") & " " & WW_BBrow("ENDTIME"))
                    '        Call NightTimeMinuteGet(WW_STDATETIME, WW_BINDSTTIME, WW_BINDENDTIME, 0, WW_STBREAKTIME, WW_ENDBREAKTIME,
                    '                                WK_WORKTIME_KYUKEI, WK_NIGHTTIME_KYUKEI, WK_YOKU0to5NIGHT_KYUKEI, WK_OUTWORKTIME_KYUKEI, WK_OUTNIGHTTIME_KYUKEI,
                    '                                WK_HWORKTIME_KYUKEI, WK_HNIGHTTIME_KYUKEI, WK_HNIGHTTIME_KYUKEI2, WW_累積分)
                    '        WW_MATCH = "ON"
                    '    Else
                    '        If WW_MATCH = "ON" Then
                    '            WW_IDX = i
                    '            Exit For
                    '        End If
                    '    End If
                    'Next
                End If
                '************************************************************
                '*   残業設定                                               *
                '************************************************************
                '○異常事態の救済　★  共通処理(事務員+乗務員)　★
                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then
                    '・拘束開始前に退社の場合
                    If DateDiff("n", WW_BINDSTTIME, WW_ENDDATETIME) < 0 Then
                        WW_BINDSTTIME = WW_STDATETIME
                    End If
                End If

                '○マイナス時間クリア
                Dim WW_ORVERTIME As Integer = 0   '平日残業時
                Dim WW_WNIGHTTIME As Integer = 0  '平日深夜時
                Dim WW_NIGHTTIME As Integer = 0   '所定内深夜時
                Dim WW_HWORKTIME As Integer = 0   '休日出勤時
                Dim WW_HNIGHTTIME As Integer = 0  '休日深夜時
                Dim WW_SWORKTIME As Integer = 0   '日曜出勤時
                Dim WW_SNIGHTTIME As Integer = 0  '日曜深夜時

                If DateDiff("d", WW_STDATETIME, WW_ENDDATETIME) <= 1 And WW_STDATETIME < WW_ENDDATETIME Then

                    Select Case WW_HEADrow("HOLIDAYKBN")
                        '○平日
                        Case "0"
                            '平日残業
                            If WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                WW_ORVERTIME = WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 平日残業時
                                WW_BREAKTIMEZAN = 0
                            Else
                                WW_ORVERTIME = 0                                           ' 平日残業時
                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTWORKTIME_SAGYO + WK_OUTWORKTIME_SAGYO2) ' 休憩残算出
                            End If

                            '所定内深夜
                            WW_NIGHTTIME = WK_NIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO     ' 所定内深夜時

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                '深夜時間(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                End If
                            End If

                            '翌日日曜日の場合
                            If WW_YOKUHOLIDAYKBN = "1" Then
                                If WW_YOKUACTTIME = "" Then
                                    If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                    End If

                                    '稼働なし、日曜深夜(24:00～翌5:00)
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                    End If
                                Else
                                    '稼働あり、深夜時間(24:00～翌5:00)
                                    '深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                    End If
                                End If

                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                If WW_YOKUACTTIME = "" Then
                                    If WK_OUTNIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO - WW_BREAKTIMEZAN    ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTNIGHTTIME_SAGYO   ' 休憩残算出
                                    End If

                                    '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                    End If
                                Else
                                    '稼働あり 深夜時間(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    If WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_WNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_WNIGHTTIME = 0                                                                     ' 平日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_YOKU0to5NIGHT_SAGYO2) ' 休憩残算出
                                    End If
                                End If

                            End If


                        Case "1"
                            '○法定休日（日曜）出勤

                            If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                WW_SWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 日曜出勤時
                                WW_BREAKTIMEZAN = 0
                            Else
                                WW_SWORKTIME = 0                                           ' 日曜出勤時
                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                            End If

                            If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                WW_SNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 日曜深夜時
                                WW_BREAKTIMEZAN = 0
                            Else
                                WW_SNIGHTTIME = 0                                                                ' 日曜深夜時
                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                            End If

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_WNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN     ' 平日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_WNIGHTTIME = 0
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2   ' 休憩残算出
                                End If

                                If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_ORVERTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 平日残業
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_ORVERTIME = 0                                              ' 平日残業
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                End If

                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_HWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN        ' 休日出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HWORKTIME = 0
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2     ' 休憩残算出
                                End If

                                If WW_YOKUACTTIME = "" Then
                                    '稼働なし 休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = 0                                           ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                    End If
                                Else
                                    '稼働あり 日曜深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_SNIGHTTIME = WW_SNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 日曜深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SNIGHTTIME = WW_SNIGHTTIME + 0                                          ' 日曜深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                    End If
                                End If
                            End If

                        Case "2"
                            '○法定外休日（祝日、会社指定休日）
                            ' 休日出勤時
                            If WK_OUTWORKTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                WW_HWORKTIME = WK_OUTWORKTIME_SAGYO - WW_BREAKTIMEZAN      ' 休日出勤時
                                WW_BREAKTIMEZAN = 0
                            Else
                                WW_HWORKTIME = 0                                           ' 休日出勤時
                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO   ' 休憩残算出
                            End If

                            ' 休日深夜時
                            If WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO >= WW_BREAKTIMEZAN Then
                                WW_HNIGHTTIME = WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO - WW_BREAKTIMEZAN     ' 休日深夜時
                                WW_BREAKTIMEZAN = 0
                            Else
                                WW_HNIGHTTIME = 0                                                                ' 休日深夜時
                                WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - (WK_OUTNIGHTTIME_SAGYO + WK_NIGHTTIME_SAGYO) ' 休憩残算出
                            End If

                            '翌日平日の場合
                            If WW_YOKUHOLIDAYKBN = "0" Then
                                '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                ' 休日出勤時
                                If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                End If

                                ' 休日深夜時
                                If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                End If
                            End If


                            '翌日日曜日の場合
                            If WW_YOKUHOLIDAYKBN = "1" Then
                                If WW_YOKUACTTIME = "" Then
                                    '稼働なし、日曜深夜(24:00～翌5:00)
                                    ' 日曜出勤時
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_SWORKTIME = WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 日曜出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SWORKTIME = 0                                            ' 日曜出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2   ' 休憩残算出
                                    End If

                                    ' 日曜深夜時
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_SNIGHTTIME = WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN   ' 日曜深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_SNIGHTTIME = 0                                           ' 日曜深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2 ' 休憩残算出
                                    End If
                                Else
                                    '稼働あり、休日深夜(0:00～5:00、22:00～24:00、翌22:00～翌24:00)
                                    ' 休日出勤時
                                    If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN  ' 休日出勤時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HWORKTIME = WW_HWORKTIME + 0                                        ' 休日出勤時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2              ' 休憩残算出
                                    End If

                                    ' 休日深夜時
                                    If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN    ' 休日深夜時
                                        WW_BREAKTIMEZAN = 0
                                    Else
                                        WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                            ' 休日深夜時
                                        WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                  ' 休憩残算出
                                    End If
                                End If
                            End If

                            '翌日法定外休日の場合
                            If WW_YOKUHOLIDAYKBN = "2" Then
                                '休日深夜(0:00～5:00、22:00～24:00、24:00～翌5:00、翌22:00～翌24:00)
                                ' 休日出勤時
                                If WK_OUTWORKTIME_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_HWORKTIME = WW_HWORKTIME + WK_OUTWORKTIME_SAGYO2 - WW_BREAKTIMEZAN      ' 休日出勤時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HWORKTIME = WW_HWORKTIME + 0                                            ' 休日出勤時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_OUTWORKTIME_SAGYO2                  ' 休憩残算出
                                End If

                                ' 休日深夜時
                                If WK_YOKU0to5NIGHT_SAGYO2 >= WW_BREAKTIMEZAN Then
                                    WW_HNIGHTTIME = WW_HNIGHTTIME + WK_YOKU0to5NIGHT_SAGYO2 - WW_BREAKTIMEZAN  ' 休日深夜時
                                    WW_BREAKTIMEZAN = 0
                                Else
                                    WW_HNIGHTTIME = WW_HNIGHTTIME + 0                                          ' 休日深夜時
                                    WW_BREAKTIMEZAN = WW_BREAKTIMEZAN - WK_YOKU0to5NIGHT_SAGYO2                ' 休憩残算出
                                End If
                            End If

                    End Select

                End If

                '************************************************************
                '*   マイナス時間クリア                                     *
                '************************************************************


                '○マイナス時間クリア
                '平日残業時
                If WW_ORVERTIME < 0 Then
                    WW_HEADrow("ORVERTIME") = "00:00"
                Else
                    WW_HEADrow("ORVERTIME") = formatHHMM(WW_ORVERTIME)
                End If
                WW_HEADrow("ORVERTIMETTL") = formatHHMM(WW_ORVERTIME + HHMMtoMinutes(WW_HEADrow("ORVERTIMECHO")))
                '平日深夜時
                If WW_WNIGHTTIME < 0 Then
                    WW_HEADrow("WNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("WNIGHTTIME") = formatHHMM(WW_WNIGHTTIME)
                End If
                WW_HEADrow("WNIGHTTIMETTL") = formatHHMM(WW_WNIGHTTIME + HHMMtoMinutes(WW_HEADrow("WNIGHTTIMECHO")))
                '所定内深夜時
                If WW_NIGHTTIME < 0 Then
                    WW_HEADrow("NIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("NIGHTTIME") = formatHHMM(WW_NIGHTTIME)
                End If
                WW_HEADrow("NIGHTTIMETTL") = formatHHMM(WW_NIGHTTIME + HHMMtoMinutes(WW_HEADrow("NIGHTTIMECHO")))
                '休日出勤時
                If WW_HWORKTIME < 0 Then
                    WW_HEADrow("HWORKTIME") = "00:00"
                Else
                    WW_HEADrow("HWORKTIME") = formatHHMM(WW_HWORKTIME)
                End If
                WW_HEADrow("HWORKTIMETTL") = formatHHMM(WW_HWORKTIME + HHMMtoMinutes(WW_HEADrow("HWORKTIMECHO")))
                '休日深夜時
                If WW_HNIGHTTIME < 0 Then
                    WW_HEADrow("HNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("HNIGHTTIME") = formatHHMM(WW_HNIGHTTIME)
                End If
                WW_HEADrow("HNIGHTTIMETTL") = formatHHMM(WW_HNIGHTTIME + HHMMtoMinutes(WW_HEADrow("HNIGHTTIMECHO")))
                '日曜出勤時
                If WW_SWORKTIME < 0 Then
                    WW_HEADrow("SWORKTIME") = "00:00"
                Else
                    WW_HEADrow("SWORKTIME") = formatHHMM(WW_SWORKTIME)
                End If
                WW_HEADrow("SWORKTIMETTL") = formatHHMM(WW_SWORKTIME + HHMMtoMinutes(WW_HEADrow("SWORKTIMECHO")))
                '日曜深夜時
                If WW_SNIGHTTIME < 0 Then
                    WW_HEADrow("SNIGHTTIME") = "00:00"
                Else
                    WW_HEADrow("SNIGHTTIME") = formatHHMM(WW_SNIGHTTIME)
                End If
                WW_HEADrow("SNIGHTTIMETTL") = formatHHMM(WW_SNIGHTTIME + HHMMtoMinutes(WW_HEADrow("SNIGHTTIMECHO")))

                '時給者所定時間（社員区分に英字が含まれいる場合を時給者と判定）
                Dim WW_JIKYUSHATIME As Integer = 0
                If Regex.IsMatch(WW_HEADrow("STAFFKBN"), "[a-zA-Z]") Then
                    '時給者所定時間=稼動時間-休憩
                    WW_JIKYUSHATIME = DateDiff("n", WW_STDATETIME, WW_ENDDATETIME) - WW_BREAKTIMETTL
                    '時給者所定時間=時給者所定時間-平日残業-平日深夜-休日残業-休日深夜-日曜残業-日曜深夜
                    WW_JIKYUSHATIME = WW_JIKYUSHATIME - WW_ORVERTIME - WW_WNIGHTTIME - WW_HWORKTIME - WW_HNIGHTTIME - WW_SWORKTIME - WW_SNIGHTTIME
                    If WW_JIKYUSHATIME < 0 Then
                        WW_HEADrow("JIKYUSHATIME") = "00:00"
                    Else
                        WW_HEADrow("JIKYUSHATIME") = formatHHMM(WW_JIKYUSHATIME)
                    End If
                    WW_HEADrow("JIKYUSHATIMETTL") = formatHHMM(WW_JIKYUSHATIME + HHMMtoMinutes(WW_HEADrow("JIKYUSHATIMECHO")))
                End If

                'WW_HEADrow("STATUS") = ""
                WW_HEADrow("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

            Next

            '勤怠ヘッダのコピー
            ioTbl = WW_T0007HEADtbl.Copy

            '勤怠明細のマージ
            ioTbl.Merge(WW_T0007DTLtbl)

            '更新元（削除）データの戻し
            ioTbl.Merge(WW_T0007DELtbl)

            WW_T0007HEADtbl.Dispose()
            WW_T0007HEADtbl = Nothing
            WW_T0007DTLtbl.Dispose()
            WW_T0007DTLtbl = Nothing
            WW_T0007DELtbl.Dispose()
            WW_T0007DELtbl = Nothing
            WW_T0007BBtbl.Dispose()
            WW_T0007BBtbl = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_KintaiCalc"             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ""                                 '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub
    '------------------------------------------------------------------------------------------------------------
    '    iSTDATETIME～iENDDATETIMEの構成時間を取得する。    ※構成時間：平日(+日曜)/休日、所定内/所定外、深夜/通常
    '------------------------------------------------------------------------------------------------------------
    Private Sub NightTimeMinuteGet(
                                ByRef iSHUSHADATE As Date,
                                ByRef iBINDSTTIME As Date,
                                ByRef iBINDENDTIME As Date,
                                ByRef iHOLIDAYKBN As Long,
                                ByRef iSTDATETIME As Date,
                                ByRef iENDDATETIME As Date,
                                ByRef ioWORKTIME As Long,
                                ByRef ioWORKTIME2 As Long,
                                ByRef ioNIGHTTIME As Long,
                                ByRef ioYOKU0to5Night As Long,
                                ByRef ioYOKU0to5Night2 As Long,
                                ByRef ioOUTWORKTIME As Long,
                                ByRef ioOUTWORKTIME2 As Long,
                                ByRef ioOUTNIGHTTIME As Long,
                                ByRef ioHWORKTIME As Long,
                                ByRef ioHNIGHTTIME As Long,
                                ByRef ioHNIGHTTIME2 As Long,
                                ByRef ioRUISEKI As Long)
        '所定内について
        '　ioWORKTIME :所定内で5:00～22:00+29:00～46:00
        '　ioNIGHTTIME :所定内で0:00～5:00+22:00～24:00+46:00～48:00
        '　ioYOKU0to5Night:所定内で24:00～29:00

        '●計算準備（深夜開始・深夜終了算出）
        Dim WW_NIGHT_TOU05 As Date  '     5：00
        Dim WW_NIGHT_TOU22 As Date  '    22：00
        Dim WW_NIGHT_TOU24 As Date  '    24：00

        Dim WW_NIGHT_YOK05 As Date  '翌   5：00
        Dim WW_NIGHT_YOK22 As Date  '翌  22：00
        Dim WW_NIGHT_YOK24 As Date  '翌  24：00

        'IF用ワーク
        Dim WW_WORKTIME As Long
        Dim WW_OVERTIME As Long

        '●深夜判断値の設定
        WW_NIGHT_TOU05 = CDate(iSHUSHADATE.ToString("yyyy/MM/dd") & " " & "05:00")

        WW_NIGHT_TOU22 = CDate(iSHUSHADATE.ToString("yyyy/MM/dd") & " " & "22:00")

        WW_NIGHT_TOU24 = CDate(iSHUSHADATE.ToString("yyyy/MM/dd") & " " & "00:00")
        WW_NIGHT_TOU24 = DateAdd("D", 1, WW_NIGHT_TOU24)

        WW_NIGHT_YOK05 = CDate(iSHUSHADATE.ToString("yyyy/MM/dd") & " " & "05:00")
        WW_NIGHT_YOK05 = DateAdd("D", 1, WW_NIGHT_YOK05)

        WW_NIGHT_YOK22 = CDate(iSHUSHADATE.ToString("yyyy/MM/dd") & " " & "22:00")
        WW_NIGHT_YOK22 = DateAdd("D", 1, WW_NIGHT_YOK22)

        WW_NIGHT_YOK24 = CDate(iSHUSHADATE.ToString("yyyy/MM/dd") & " " & "00:00")
        WW_NIGHT_YOK24 = DateAdd("D", 2, WW_NIGHT_YOK24)

        '●累積の設定
        ioRUISEKI = ioRUISEKI + DateDiff("n", iSTDATETIME, iENDDATETIME)

        '●所定内通常分・所定内深夜分・所定外通常分・所定外深夜分・休日通常分・休日深夜分の設定
        Select Case iHOLIDAYKBN

            ' *********************************************************
            ' *   平日処理　-　深夜＆所定判断により値設定する         *
            ' *********************************************************
            Case 0

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：5時前　ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME <= WW_NIGHT_TOU05 _
                Then
                    If iENDDATETIME <= WW_NIGHT_TOU05 _
                    Then
                        '○終了(～5時前)

                        '開始～終了⇒平日深夜
                        Call WorkTimeMinuteGet(iSTDATETIME, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                    Else
                        If iENDDATETIME <= WW_NIGHT_TOU22 _
                        Then
                            '○終了(5時過～22時)

                            '開始～朝5 ⇒平日深夜
                            Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                            ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                            '朝5 ～終了⇒平日通常
                            Call WorkTimeMinuteGet(WW_NIGHT_TOU05, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioWORKTIME = ioWORKTIME + WW_WORKTIME
                            ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME
                        Else
                            If iENDDATETIME <= WW_NIGHT_TOU24 _
                            Then
                                '○終了(22時過～24時)

                                '開始～朝5 ⇒平日深夜
                                Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                '朝5 ～夜22⇒平日通常
                                Call WorkTimeMinuteGet(WW_NIGHT_TOU05, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                '夜22～終了⇒平日深夜
                                Call WorkTimeMinuteGet(WW_NIGHT_TOU22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                            Else
                                If iENDDATETIME <= WW_NIGHT_YOK05 _
                                Then
                                    '○終了(24時過～翌5時)

                                    '開始～朝5 ⇒平日深夜
                                    Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                    ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                    '朝5 ～夜22⇒平日通常
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU05, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                    ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                    '夜22～夜24⇒平日深夜
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU22, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                    ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                    '夜24～終了⇒平日深夜   '★★★★
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU24, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                    ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                    'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                                Else
                                    If iENDDATETIME <= WW_NIGHT_YOK22 _
                                    Then
                                        '○終了(翌5時過～翌22時)

                                        '開始～朝5 ⇒平日深夜
                                        Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                        '朝5 ～夜22⇒平日通常
                                        Call WorkTimeMinuteGet(WW_NIGHT_TOU05, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                        ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                        '夜22～夜24⇒平日深夜
                                        Call WorkTimeMinuteGet(WW_NIGHT_TOU22, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                        '夜24～翌5 ⇒平日深夜   '★★★★
                                        Call WorkTimeMinuteGet(WW_NIGHT_TOU24, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                        ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                        'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                        '翌5 ～終了⇒平日通常
                                        Call WorkTimeMinuteGet(WW_NIGHT_YOK05, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioWORKTIME2 = ioWORKTIME2 + WW_WORKTIME
                                        ioOUTWORKTIME2 = ioOUTWORKTIME2 + WW_OVERTIME
                                    Else
                                        '○終了(翌22時過～翌24時) …　翌24以降は未サポート

                                        '開始～朝5 ⇒平日深夜
                                        Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                        '朝5 ～夜22⇒平日通常
                                        Call WorkTimeMinuteGet(WW_NIGHT_TOU05, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                        ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                        '夜22～夜24⇒平日深夜
                                        Call WorkTimeMinuteGet(WW_NIGHT_TOU22, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                        '夜24～朝5 ⇒平日深夜   '★★★★
                                        Call WorkTimeMinuteGet(WW_NIGHT_TOU24, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                        ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                        'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                        '翌5 ～翌22⇒平日通常
                                        Call WorkTimeMinuteGet(WW_NIGHT_YOK05, WW_NIGHT_YOK22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioWORKTIME2 = ioWORKTIME2 + WW_WORKTIME
                                        ioOUTWORKTIME2 = ioOUTWORKTIME2 + WW_OVERTIME

                                        '翌22～終了⇒平日深夜
                                        Call WorkTimeMinuteGet(WW_NIGHT_YOK22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：5時過～22時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_TOU05 And iSTDATETIME <= WW_NIGHT_TOU22 _
                Then
                    If iENDDATETIME <= WW_NIGHT_TOU22 _
                    Then
                        '○終了(5時過～22時)

                        '開始 ～終了⇒平日通常
                        Call WorkTimeMinuteGet(iSTDATETIME, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioWORKTIME = ioWORKTIME + WW_WORKTIME
                        ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME
                    Else
                        If iENDDATETIME <= WW_NIGHT_TOU24 _
                        Then
                            '○終了(22時過～24時)

                            '開始 ～夜22⇒平日通常
                            Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioWORKTIME = ioWORKTIME + WW_WORKTIME
                            ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                            '夜22～終了⇒平日深夜
                            Call WorkTimeMinuteGet(WW_NIGHT_TOU22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                            ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                        Else
                            If iENDDATETIME <= WW_NIGHT_YOK05 _
                            Then
                                '○終了(24時過～翌5時)

                                '開始 ～夜22⇒平日通常
                                Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                '夜22～夜24⇒平日深夜
                                Call WorkTimeMinuteGet(WW_NIGHT_TOU22, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                '夜24～終了⇒平日深夜   '★★★★
                                Call WorkTimeMinuteGet(WW_NIGHT_TOU24, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                            Else
                                If iENDDATETIME <= WW_NIGHT_YOK22 _
                                Then
                                    '○終了(翌5時過～翌22時)

                                    '開始 ～夜22⇒平日通常
                                    Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                    ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                    '夜22～夜24⇒平日深夜
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU22, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                    ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                    '夜24～翌5 ⇒平日深夜   '★★★★
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU24, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                    ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                    'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                    '翌5 ～終了⇒平日通常
                                    Call WorkTimeMinuteGet(WW_NIGHT_YOK05, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioWORKTIME2 = ioWORKTIME2 + WW_WORKTIME
                                    ioOUTWORKTIME2 = ioOUTWORKTIME2 + WW_OVERTIME
                                Else
                                    '○終了(翌22時過～翌24時)

                                    '開始 ～夜22⇒平日通常
                                    Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                    ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                    '夜22～夜24⇒平日深夜
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU22, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                    ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                    '夜24～翌5 ⇒平日深夜   '★★★★
                                    Call WorkTimeMinuteGet(WW_NIGHT_TOU24, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                    ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                    'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                    '翌5 ～翌22⇒平日通常
                                    Call WorkTimeMinuteGet(WW_NIGHT_YOK05, WW_NIGHT_YOK22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioWORKTIME2 = ioWORKTIME2 + WW_WORKTIME
                                    ioOUTWORKTIME2 = ioOUTWORKTIME2 + WW_OVERTIME

                                    '翌22～終了⇒平日深夜
                                    Call WorkTimeMinuteGet(WW_NIGHT_YOK22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                    ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                    ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                                End If
                            End If
                        End If
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：22時過～24時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_TOU22 And iSTDATETIME <= WW_NIGHT_TOU24 _
                Then
                    If iENDDATETIME <= WW_NIGHT_TOU24 _
                    Then
                        '○終了(22時過～24時)

                        '開始～終了⇒平日深夜
                        Call WorkTimeMinuteGet(iSTDATETIME, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                    Else
                        If iENDDATETIME <= WW_NIGHT_YOK05 _
                        Then
                            '○終了(24時過～翌5時)

                            '開始～夜24⇒平日深夜
                            Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                            ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                            '夜24～終了⇒平日深夜   '★★★★
                            Call WorkTimeMinuteGet(WW_NIGHT_TOU24, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                            ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                            'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                        Else
                            If iENDDATETIME <= WW_NIGHT_YOK22 _
                            Then
                                '○終了(翌5時過～翌22時)

                                '開始～夜24⇒平日深夜
                                Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                '夜24～翌5 ⇒平日深夜   '★★★★
                                Call WorkTimeMinuteGet(WW_NIGHT_TOU24, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                '翌5 ～終了⇒平日通常
                                Call WorkTimeMinuteGet(WW_NIGHT_YOK05, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME
                            Else
                                '○終了(翌22時過～翌24時)

                                '開始～夜24⇒平日深夜
                                Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_TOU24, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                '夜24～翌5 ⇒平日深夜   '★★★★
                                Call WorkTimeMinuteGet(WW_NIGHT_TOU24, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                                ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                                'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                                '翌5 ～翌22⇒平日通常
                                Call WorkTimeMinuteGet(WW_NIGHT_YOK05, WW_NIGHT_YOK22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioWORKTIME = ioWORKTIME + WW_WORKTIME
                                ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                                '翌22～終了⇒平日深夜
                                Call WorkTimeMinuteGet(WW_NIGHT_YOK22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                                ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                                ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                            End If
                        End If

                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：24時過～翌5時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_TOU24 And iSTDATETIME <= WW_NIGHT_YOK05 _
                Then
                    If iENDDATETIME <= WW_NIGHT_YOK05 _
                    Then
                        '○終了(22時過～翌5時)

                        '開始～終了⇒平日深夜   '★★★★
                        Call WorkTimeMinuteGet(iSTDATETIME, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                        ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                        'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                    Else
                        If iENDDATETIME <= WW_NIGHT_YOK22 _
                        Then
                            '○終了(翌5時過～翌22時)

                            '開始～翌5 ⇒平日深夜   '★★★★
                            Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                            ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                            'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                            '翌5 ～終了⇒平日通常
                            Call WorkTimeMinuteGet(WW_NIGHT_YOK05, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioWORKTIME2 = ioWORKTIME2 + WW_WORKTIME
                            ioOUTWORKTIME2 = ioOUTWORKTIME2 + WW_OVERTIME
                        Else
                            '○終了(翌22時過～翌24時)

                            '開始～翌5 ⇒平日深夜   '★★★★
                            Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_YOK05, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioYOKU0to5Night = ioYOKU0to5Night + WW_WORKTIME
                            ioYOKU0to5Night2 = ioYOKU0to5Night2 + WW_OVERTIME
                            'ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME

                            '翌5 ～翌22⇒平日通常
                            Call WorkTimeMinuteGet(WW_NIGHT_YOK05, WW_NIGHT_YOK22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioWORKTIME2 = ioWORKTIME2 + WW_WORKTIME
                            ioOUTWORKTIME2 = ioOUTWORKTIME2 + WW_OVERTIME

                            '翌22～終了⇒平日深夜
                            Call WorkTimeMinuteGet(WW_NIGHT_YOK22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                            ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                            ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                        End If
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：翌5時過～翌22時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_YOK05 And iSTDATETIME <= WW_NIGHT_YOK22 _
                Then
                    If iENDDATETIME <= WW_NIGHT_YOK22 _
                    Then
                        '○終了(翌5時過～翌22時)

                        '開始 ～終了⇒平日通常
                        Call WorkTimeMinuteGet(iSTDATETIME, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioWORKTIME = ioWORKTIME + WW_WORKTIME
                        ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME
                    Else
                        '○終了(翌22時過～翌24時) …　翌24以降は未サポート

                        '開始 ～翌22⇒平日通常
                        Call WorkTimeMinuteGet(iSTDATETIME, WW_NIGHT_YOK22, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioWORKTIME = ioWORKTIME + WW_WORKTIME
                        ioOUTWORKTIME = ioOUTWORKTIME + WW_OVERTIME

                        '翌22～終了⇒平日深夜
                        Call WorkTimeMinuteGet(WW_NIGHT_YOK22, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                        ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                        ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：翌22時過～翌24時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_YOK22 And iSTDATETIME <= WW_NIGHT_YOK24 _
                Then
                    '○終了(翌22時過～翌24時) …　翌24以降は未サポート

                    '開始～終了⇒平日深夜
                    Call WorkTimeMinuteGet(iSTDATETIME, iENDDATETIME, iBINDSTTIME, iBINDENDTIME, WW_WORKTIME, WW_OVERTIME)
                    ioNIGHTTIME = ioNIGHTTIME + WW_WORKTIME
                    ioOUTNIGHTTIME = ioOUTNIGHTTIME + WW_OVERTIME
                End If


                ' ********************************************************************************************
                ' *   法定・法定外休日(=1or2)処理　-　深夜＆所定判断により値設定する                         *
                ' ********************************************************************************************
            Case Else
                ' ---------------------------------------------------------
                ' *  ○○○ 開始：5時前　ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME <= WW_NIGHT_TOU05 _
                Then
                    If iENDDATETIME <= WW_NIGHT_TOU05 _
                    Then
                        '○終了(～5時前)

                        '開始～終了⇒休日深夜
                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, iENDDATETIME)

                    Else
                        If iENDDATETIME <= WW_NIGHT_TOU22 _
                        Then
                            '○終了(5時過～22時)

                            '開始～朝5 ⇒休日深夜
                            ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU05)

                            '朝5 ～終了⇒休日通常
                            ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_TOU05, iENDDATETIME)
                        Else
                            If iENDDATETIME <= WW_NIGHT_TOU24 _
                            Then
                                '○終了(22時過～24時)

                                '開始～朝5 ⇒休日深夜
                                ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU05)

                                '朝5 ～夜22⇒休日通常
                                ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_TOU05, WW_NIGHT_TOU22)

                                '夜22～終了⇒休日深夜
                                ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, iENDDATETIME)
                            Else
                                If iENDDATETIME <= WW_NIGHT_YOK05 _
                                Then
                                    '○終了(24時過～翌5時)

                                    '開始～朝5 ⇒休日深夜
                                    ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU05)

                                    '朝5 ～夜22⇒休日通常
                                    ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_TOU05, WW_NIGHT_TOU22)

                                    '夜22～夜24⇒休日深夜
                                    ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, WW_NIGHT_TOU24)

                                    '夜24～終了⇒休日深夜   '★★★★
                                    ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, iENDDATETIME)
                                Else
                                    If iENDDATETIME <= WW_NIGHT_YOK22 _
                                    Then
                                        '○終了(翌5時過～翌22時)

                                        '開始～朝5 ⇒休日深夜
                                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU05)

                                        '朝5 ～夜22⇒休日通常
                                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_TOU05, WW_NIGHT_TOU22)

                                        '夜22～夜24⇒休日深夜
                                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, WW_NIGHT_TOU24)

                                        '夜24～翌5 ⇒休日深夜   '★★★★
                                        ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, WW_NIGHT_YOK05)

                                        '翌5 ～終了⇒休日通常
                                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, iENDDATETIME)
                                    Else
                                        '○終了(翌22時過～翌24時) …　翌24以降は未サポート

                                        '開始～朝5 ⇒休日深夜
                                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU05)

                                        '朝5 ～夜22⇒休日通常
                                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_TOU05, WW_NIGHT_TOU22)

                                        '夜22～夜24⇒休日深夜
                                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, WW_NIGHT_TOU24)

                                        '夜24～翌5 ⇒休日深夜   '★★★★
                                        ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, WW_NIGHT_YOK05)

                                        '翌5 ～翌22⇒休日通常
                                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, WW_NIGHT_YOK22)

                                        '翌22～終了⇒休日深夜
                                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_YOK22, iENDDATETIME)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：5時過～22時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_TOU05 And iSTDATETIME <= WW_NIGHT_TOU22 _
                Then
                    If iENDDATETIME <= WW_NIGHT_TOU22 _
                    Then
                        '○終了(5時過～22時)

                        '開始 ～終了⇒休日通常
                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, iENDDATETIME)
                    Else
                        If iENDDATETIME <= WW_NIGHT_TOU24 _
                        Then
                            '○終了(22時過～24時)

                            '開始 ～夜22⇒休日通常
                            ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU22)

                            '夜22～終了⇒休日深夜
                            ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, iENDDATETIME)
                        Else
                            If iENDDATETIME <= WW_NIGHT_YOK05 _
                            Then
                                '○終了(24時過～翌5時)

                                '開始 ～夜22⇒休日通常
                                ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU22)

                                '夜22～夜24⇒休日深夜
                                ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, WW_NIGHT_TOU24)

                                '夜24～終了⇒休日深夜   '★★★★
                                ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, iENDDATETIME)
                            Else
                                If iENDDATETIME <= WW_NIGHT_YOK22 _
                                Then
                                    '○終了(翌5時過～翌22時)

                                    '開始 ～夜22⇒休日通常
                                    ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU22)

                                    '夜22～夜24⇒休日深夜
                                    ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, WW_NIGHT_TOU24)

                                    '夜24～翌5 ⇒休日深夜   '★★★★
                                    ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, WW_NIGHT_YOK05)

                                    '翌5 ～終了⇒休日通常
                                    ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, iENDDATETIME)
                                Else
                                    '○終了(翌22時過～翌24時)

                                    '開始～夜22⇒休日通常
                                    ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU22)

                                    '夜22～夜24⇒休日深夜
                                    ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_TOU22, WW_NIGHT_TOU24)

                                    '夜24～翌5 ⇒休日深夜   '★★★★
                                    ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, WW_NIGHT_YOK05)

                                    '翌5 ～翌22⇒休日通常
                                    ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, WW_NIGHT_YOK22)

                                    '翌22～終了⇒休日深夜
                                    ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_YOK22, iENDDATETIME)
                                End If
                            End If
                        End If
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：22時過～24時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_TOU22 And iSTDATETIME <= WW_NIGHT_TOU24 _
                Then
                    If iENDDATETIME <= WW_NIGHT_TOU24 _
                    Then
                        '○終了(22時過～24時)

                        '開始～終了⇒休日深夜
                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, iENDDATETIME)
                    Else
                        If iENDDATETIME <= WW_NIGHT_YOK05 _
                        Then
                            '○終了(24時過～翌05時)

                            '開始～24時⇒休日深夜
                            ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU24)

                            '24時～終了⇒休日深夜   '★★★★
                            ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, iENDDATETIME)
                        Else
                            If iENDDATETIME <= WW_NIGHT_YOK22 _
                            Then
                                '○終了(翌05時過～翌22時)

                                '開始～夜24⇒休日深夜
                                ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU24)

                                '夜24～翌5 ⇒休日深夜   '★★★★
                                ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, WW_NIGHT_YOK05)

                                '翌5 ～終了⇒休日通常
                                ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, iENDDATETIME)
                            Else
                                '○終了(翌22時過～翌24時)

                                '開始～夜24⇒休日深夜
                                ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_TOU24)

                                '夜24～翌5 ⇒休日深夜   '★★★★
                                ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", WW_NIGHT_TOU24, WW_NIGHT_YOK05)

                                '翌5 ～翌22⇒休日通常
                                ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, WW_NIGHT_YOK22)

                                '翌22～終了⇒休日深夜
                                ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_YOK22, iENDDATETIME)
                            End If
                        End If
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：24時過～翌5時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_TOU24 And iSTDATETIME <= WW_NIGHT_YOK05 _
                Then
                    If iENDDATETIME <= WW_NIGHT_YOK05 _
                    Then
                        '○終了(24時過～翌5時)

                        '開始～終了⇒休日深夜   '★★★★
                        ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", iSTDATETIME, iENDDATETIME)
                    Else
                        If iENDDATETIME <= WW_NIGHT_YOK22 _
                        Then
                            '○終了(翌5時過～翌22時)

                            '開始～翌5 ⇒休日深夜   '★★★★
                            ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", iSTDATETIME, WW_NIGHT_YOK05)

                            '翌5 ～終了⇒休日通常
                            ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, iENDDATETIME)
                        Else
                            '○終了(翌22時過～翌24時)

                            '開始～翌5 ⇒休日深夜   '★★★★
                            ioHNIGHTTIME2 = ioHNIGHTTIME2 + DateDiff("n", iSTDATETIME, WW_NIGHT_YOK05)

                            '翌5 ～翌22⇒休日通常
                            ioHWORKTIME = ioHWORKTIME + DateDiff("n", WW_NIGHT_YOK05, WW_NIGHT_YOK22)

                            '翌22～終了⇒休日深夜
                            ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_YOK22, iENDDATETIME)
                        End If
                    End If
                End If


                ' ---------------------------------------------------------
                ' *  ○○○ 開始：翌5時過～翌22時 ケース ○○○
                ' ---------------------------------------------------------
                If iSTDATETIME > WW_NIGHT_YOK05 And iSTDATETIME <= WW_NIGHT_YOK22 _
                Then
                    If iENDDATETIME <= WW_NIGHT_YOK22 _
                    Then
                        '○終了(翌5時過～翌22時)

                        '開始 ～終了⇒休日通常
                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, iENDDATETIME)
                    Else
                        '○終了(翌22時過～翌24時) …　翌24以降は未サポート

                        '開始 ～翌22⇒休日通常
                        ioHWORKTIME = ioHWORKTIME + DateDiff("n", iSTDATETIME, WW_NIGHT_YOK22)

                        '翌22～終了⇒休日深夜
                        ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", WW_NIGHT_YOK22, iENDDATETIME)
                    End If
                End If

                ' ---------------------------------------------------------
                ' *  ○○○ 開始：翌22時過～翌24時 ケース ○○○
                ' ---------------------------------------------------------

                If iSTDATETIME > WW_NIGHT_YOK22 And iSTDATETIME <= WW_NIGHT_YOK24 _
                Then
                    '○終了(翌22時過～翌24時) …　翌24以降は未サポート

                    '開始～終了⇒休日深夜
                    ioHNIGHTTIME = ioHNIGHTTIME + DateDiff("n", iSTDATETIME, iENDDATETIME)
                End If

        End Select


    End Sub

    '------------------------------------------------------------
    ' iSTDATE～iENDDATEを所定および残業へ分解
    '------------------------------------------------------------
    Private Sub WorkTimeMinuteGet(ByRef iSTDATE As Date, ByRef iENDDATE As Date, ByRef iSTBIND As Date, ByRef iENDBIND As Date,
                                  ByRef oWORKTIME As Long, ByRef oOVERTIME As Long)

        oWORKTIME = 0
        oOVERTIME = 0

        ' *********************************************************
        ' * iSTDATE：所定開始前 ケース                         *
        ' *********************************************************
        If iSTDATE <= iSTBIND _
        Then
            If iENDDATE <= iSTBIND _
            Then
                'In開始(iSTBIND前)～In終了(iSTBIND前)
                '★iSTDATE～iENDDATE⇒oOVERTIME
                oOVERTIME = DateDiff("n", iSTDATE, iENDDATE)
            Else
                If iENDDATE <= iENDBIND _
                Then
                    'In開始(iSTBIND前)～In終了(iSTBIND後&iENDBIND前)
                    '★iSTDATE～iSTBIND⇒oOVERTIME
                    oOVERTIME = DateDiff("n", iSTDATE, iSTBIND)
                    '★iSTBIND～iENDDATE⇒oWORKTIME
                    oWORKTIME = DateDiff("n", iSTBIND, iENDDATE)
                Else
                    'In開始(iSTBIND前)～In終了(iENDBIND後)
                    '★iSTDATE～iSTBIND⇒oOVERTIME
                    oOVERTIME = DateDiff("n", iSTDATE, iSTBIND)
                    '★iSTBIND～iENDBIND⇒oWORKTIME
                    oWORKTIME = DateDiff("n", iSTBIND, iENDBIND)
                    '★iENDBIND～iENDDATE⇒oOVERTIME
                    oOVERTIME = oOVERTIME + DateDiff("n", iENDBIND, iENDDATE)
                End If
            End If
        End If

        ' *********************************************************
        ' * iSTDATE：所定開始過 & 所定終了前 ケース            *
        ' *********************************************************
        If iSTDATE > iSTBIND And iSTDATE <= iENDBIND _
        Then
            If iENDDATE <= iENDBIND _
            Then
                'In開始(iSTBIND後)～In終了(iSTBIND後&iENDBIND前)
                '★iSTDATE～iENDDATE⇒oWORKTIME
                oWORKTIME = DateDiff("n", iSTDATE, iENDDATE)
            Else
                'In開始(iSTBIND後)～In終了(iENDBIND後)
                '★iSTDATE～iENDBIND⇒oWORKTIME
                oWORKTIME = DateDiff("n", iSTDATE, iENDBIND)
                '★iENDBIND～iENDDATE⇒oOVERTIME
                oOVERTIME = DateDiff("n", iENDBIND, iENDDATE)
            End If
        End If

        ' *********************************************************
        ' * iSTDATE：所定開始過 & 所定終了前 ケース            *
        ' *********************************************************
        If iSTDATE > iENDBIND _
        Then
            'In開始(iENDBIND後)～In終了(iENDBIND後)
            '★iSTDATE～iENDDATE⇒oOVERTIME
            oOVERTIME = DateDiff("n", iSTDATE, iENDDATE)
        End If

    End Sub

    Private Function BindEndTimeGet(ByRef iSTBIND As Date, iENDBIND As Date, iSTDATETIME As Date, iENDDATETIME As Date) As Date

        '初期設定
        Dim WW_DATETIME As Date = iENDBIND

        '●  拘束終了日時のずらし込み

        ' ---------------------------------------------------------
        ' *  ○○○ 開始：拘束開始前　ケース ○○○
        ' ---------------------------------------------------------
        If iSTDATETIME <= iSTBIND _
        Then
            If iENDDATETIME <= iSTBIND _
            Then
                '○終了(～拘束開始前)

                '対象外
            Else
                If iENDDATETIME <= iENDBIND _
                Then
                    '○終了(～実拘束終了前)

                    '加算(iSTBIND～iENDDATETIME)
                    WW_DATETIME = DateAdd("n", DateDiff("n", iSTBIND, iENDDATETIME), WW_DATETIME)
                Else
                    '○終了(実拘束終了～)

                    '加算(iSTBIND～iENDBIND)
                    'WW_DATETIME = DateAdd("n", DateDiff("n", iSTBIND, iENDBIND), WW_DATETIME)
                    'ACCESSに合わせる
                    WW_DATETIME = DateAdd("n", DateDiff("n", iSTBIND, iENDDATETIME), WW_DATETIME)
                End If
            End If
        Else
        End If

        ' ---------------------------------------------------------
        ' *  ○○○ 開始：拘束開始過 ＆拘束終了前 　ケース ○○○
        ' ---------------------------------------------------------
        If iSTDATETIME > iSTBIND And iSTDATETIME < iENDBIND _
        Then
            If iENDDATETIME <= iENDBIND _
            Then
                '○終了(～実拘束終了前)

                '加算(iSTDATETIME～iENDDATETIME)
                WW_DATETIME = DateAdd("n", DateDiff("n", iSTDATETIME, iENDDATETIME), WW_DATETIME)
            Else
                '○終了(実拘束終了～)

                '加算(iSTDATETIME～iENDDATETIME)
                'WW_DATETIME = DateAdd("n", DateDiff("n", iSTDATETIME, iENDBIND), WW_DATETIME)
                'ACCESSに合わせる
                WW_DATETIME = DateAdd("n", DateDiff("n", iSTDATETIME, iENDDATETIME), WW_DATETIME)
            End If
        Else
        End If

        ' ---------------------------------------------------------
        ' *  ○○○ 開始：拘束終了後　ケース ○○○
        ' ---------------------------------------------------------
        '対象外

        ' *********************************************************
        '●  リターン設定
        ' *********************************************************
        BindEndTimeGet = WW_DATETIME

    End Function

    ' ***  編集エリア初期化
    Public Sub INProw_Init(ByVal iCamp As String, ByRef ioRow As DataRow)
        ioRow("LINECNT") = 0
        ioRow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        ioRow("TIMSTP") = "0"
        ioRow("SELECT") = "1"
        ioRow("HIDDEN") = "0"
        ioRow("EXTRACTCNT") = "0"

        ioRow("STATUS") = ""
        ioRow("CAMPCODE") = iCamp
        ioRow("CAMPNAMES") = ""
        ioRow("TAISHOYM") = ""
        ioRow("STAFFCODE") = ""
        ioRow("STAFFNAMES") = ""
        ioRow("WORKDATE") = ""
        ioRow("WORKINGWEEK") = ""
        ioRow("WORKINGWEEKNAMES") = ""
        ioRow("HDKBN") = ""
        ioRow("RECODEKBN") = ""
        ioRow("RECODEKBNNAMES") = ""
        ioRow("SEQ") = ""
        ioRow("ENTRYDATE") = ""
        ioRow("NIPPOLINKCODE") = ""
        ioRow("MORG") = ""
        ioRow("MORGNAMES") = ""
        ioRow("HORG") = ""
        ioRow("HORGNAMES") = ""
        ioRow("SORG") = ""
        ioRow("SORGNAMES") = ""
        ioRow("STAFFKBN") = ""
        ioRow("STAFFKBNNAMES") = ""
        ioRow("HOLIDAYKBN") = ""
        ioRow("HOLIDAYKBNNAMES") = ""
        ioRow("PAYKBN") = ""
        ioRow("PAYKBNNAMES") = ""
        ioRow("SHUKCHOKKBN") = ""
        ioRow("SHUKCHOKKBNNAMES") = ""
        ioRow("WORKKBN") = ""
        ioRow("WORKKBNNAMES") = ""
        ioRow("STDATE") = ""
        ioRow("STTIME") = "00:00"
        ioRow("ENDDATE") = ""
        ioRow("ENDTIME") = "00:00"
        ioRow("WORKTIME") = "00:00"
        ioRow("MOVETIME") = "00:00"
        ioRow("ACTTIME") = "00:00"
        ioRow("BINDSTDATE") = "00:00"
        ioRow("BINDTIME") = "00:00"
        ioRow("NIPPOBREAKTIME") = "00:00"
        ioRow("BREAKTIME") = "00:00"
        ioRow("BREAKTIMECHO") = "00:00"
        ioRow("BREAKTIMETTL") = "00:00"
        ioRow("NIGHTTIME") = "00:00"
        ioRow("NIGHTTIMECHO") = "00:00"
        ioRow("NIGHTTIMETTL") = "00:00"
        ioRow("ORVERTIME") = "00:00"
        ioRow("ORVERTIMECHO") = "00:00"
        If ioRow.Table.Columns.Contains("ORVERTIMEADD") Then
            ioRow("ORVERTIMEADD") = "00:00"
        End If
        ioRow("ORVERTIMETTL") = "00:00"
        ioRow("WNIGHTTIME") = "00:00"
        ioRow("WNIGHTTIMECHO") = "00:00"
        If ioRow.Table.Columns.Contains("WNIGHTTIMEADD") Then
            ioRow("WNIGHTTIMEADD") = "00:00"
        End If
        ioRow("WNIGHTTIMETTL") = "00:00"
        ioRow("SWORKTIME") = "00:00"
        ioRow("SWORKTIMECHO") = "00:00"
        If ioRow.Table.Columns.Contains("SWORKTIMEADD") Then
            ioRow("SWORKTIMEADD") = "00:00"
        End If
        ioRow("SWORKTIMETTL") = "00:00"
        ioRow("SNIGHTTIME") = "00:00"
        ioRow("SNIGHTTIMECHO") = "00:00"
        If ioRow.Table.Columns.Contains("SNIGHTTIMEADD") Then
            ioRow("SNIGHTTIMEADD") = "00:00"
        End If
        ioRow("SNIGHTTIMETTL") = "00:00"
        ioRow("HWORKTIME") = "00:00"
        ioRow("HWORKTIMECHO") = "00:00"
        ioRow("HWORKTIMETTL") = "00:00"
        ioRow("HNIGHTTIME") = "00:00"
        ioRow("HNIGHTTIMECHO") = "00:00"
        ioRow("HNIGHTTIMETTL") = "00:00"
        ioRow("WORKNISSU") = "0"
        ioRow("WORKNISSUCHO") = "0"
        ioRow("WORKNISSUTTL") = "0"
        ioRow("SHOUKETUNISSU") = "0"
        ioRow("SHOUKETUNISSUCHO") = "0"
        ioRow("SHOUKETUNISSUTTL") = "0"
        ioRow("KUMIKETUNISSU") = "0"
        ioRow("KUMIKETUNISSUCHO") = "0"
        ioRow("KUMIKETUNISSUTTL") = "0"
        ioRow("ETCKETUNISSU") = "0"
        ioRow("ETCKETUNISSUCHO") = "0"
        ioRow("ETCKETUNISSUTTL") = "0"
        ioRow("NENKYUNISSU") = "0"
        ioRow("NENKYUNISSUCHO") = "0"
        ioRow("NENKYUNISSUTTL") = "0"
        ioRow("TOKUKYUNISSU") = "0"
        ioRow("TOKUKYUNISSUCHO") = "0"
        ioRow("TOKUKYUNISSUTTL") = "0"
        ioRow("CHIKOKSOTAINISSU") = "0"
        ioRow("CHIKOKSOTAINISSUCHO") = "0"
        ioRow("CHIKOKSOTAINISSUTTL") = "0"
        ioRow("STOCKNISSU") = "0"
        ioRow("STOCKNISSUCHO") = "0"
        ioRow("STOCKNISSUTTL") = "0"
        ioRow("KYOTEIWEEKNISSU") = "0"
        ioRow("KYOTEIWEEKNISSUCHO") = "0"
        ioRow("KYOTEIWEEKNISSUTTL") = "0"
        ioRow("WEEKNISSU") = "0"
        ioRow("WEEKNISSUCHO") = "0"
        ioRow("WEEKNISSUTTL") = "0"
        ioRow("DAIKYUNISSU") = "0"
        ioRow("DAIKYUNISSUCHO") = "0"
        ioRow("DAIKYUNISSUTTL") = "0"
        ioRow("NENSHINISSU") = "0"
        ioRow("NENSHINISSUCHO") = "0"
        ioRow("NENSHINISSUTTL") = "0"
        ioRow("SHUKCHOKNNISSU") = "0"
        ioRow("SHUKCHOKNNISSUCHO") = "0"
        ioRow("SHUKCHOKNNISSUTTL") = "0"
        ioRow("SHUKCHOKNISSU") = "0"
        ioRow("SHUKCHOKNISSUCHO") = "0"
        ioRow("SHUKCHOKNISSUTTL") = "0"
        If ioRow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
            ioRow("SHUKCHOKNHLDNISSU") = "0"
        End If
        If ioRow.Table.Columns.Contains("SHUKCHOKNHLDNISSUCHO") Then
            ioRow("SHUKCHOKNHLDNISSUCHO") = "0"
        End If
        If ioRow.Table.Columns.Contains("SHUKCHOKNHLDNISSUTTL") Then
            ioRow("SHUKCHOKNHLDNISSUTTL") = "0"
        End If
        If ioRow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
            ioRow("SHUKCHOKHLDNISSU") = "0"
        End If
        If ioRow.Table.Columns.Contains("SHUKCHOKHLDNISSUCHO") Then
            ioRow("SHUKCHOKHLDNISSUCHO") = "0"
        End If
        If ioRow.Table.Columns.Contains("SHUKCHOKHLDNISSUTTL") Then
            ioRow("SHUKCHOKHLDNISSUTTL") = "0"
        End If
        ioRow("TOKSAAKAISU") = "0"
        ioRow("TOKSAAKAISUCHO") = "0"
        ioRow("TOKSAAKAISUTTL") = "0"
        ioRow("TOKSABKAISU") = "0"
        ioRow("TOKSABKAISUCHO") = "0"
        ioRow("TOKSABKAISUTTL") = "0"
        ioRow("TOKSACKAISU") = "0"
        ioRow("TOKSACKAISUCHO") = "0"
        ioRow("TOKSACKAISUTTL") = "0"

        If ioRow.Table.Columns.Contains("TENKOKAISU") Then
            ioRow("TENKOKAISU") = "0"
        End If
        If ioRow.Table.Columns.Contains("TENKOKAISUCHO") Then
            ioRow("TENKOKAISUCHO") = "0"
        End If
        If ioRow.Table.Columns.Contains("TENKOKAISUTTL") Then
            ioRow("TENKOKAISUTTL") = "0"
        End If

        ioRow("HOANTIME") = "00:00"
        ioRow("HOANTIMECHO") = "00:00"
        ioRow("HOANTIMETTL") = "00:00"
        ioRow("KOATUTIME") = "00:00"
        ioRow("KOATUTIMECHO") = "00:00"
        ioRow("KOATUTIMETTL") = "00:00"
        ioRow("TOKUSA1TIME") = "00:00"
        ioRow("TOKUSA1TIMECHO") = "00:00"
        ioRow("TOKUSA1TIMETTL") = "00:00"
        ioRow("HAYADETIME") = "00:00"
        ioRow("HAYADETIMECHO") = "00:00"
        ioRow("HAYADETIMETTL") = "00:00"
        ioRow("PONPNISSU") = "0"
        ioRow("PONPNISSUCHO") = "0"
        ioRow("PONPNISSUTTL") = "0"
        ioRow("BULKNISSU") = "0"
        ioRow("BULKNISSUCHO") = "0"
        ioRow("BULKNISSUTTL") = "0"
        ioRow("TRAILERNISSU") = "0"
        ioRow("TRAILERNISSUCHO") = "0"
        ioRow("TRAILERNISSUTTL") = "0"
        ioRow("BKINMUKAISU") = "0"
        ioRow("BKINMUKAISUCHO") = "0"
        ioRow("BKINMUKAISUTTL") = "0"
        ioRow("SHARYOKBN") = ""
        ioRow("SHARYOKBNNAMES") = ""
        ioRow("OILPAYKBN") = ""
        ioRow("OILPAYKBNNAMES") = ""
        ioRow("UNLOADCNT") = "0"
        ioRow("UNLOADCNTCHO") = "0"
        ioRow("UNLOADCNTTTL") = "0"
        ioRow("HAIDISTANCE") = "0"
        ioRow("HAIDISTANCECHO") = "0"
        ioRow("HAIDISTANCETTL") = "0"
        ioRow("KAIDISTANCE") = "0"
        ioRow("KAIDISTANCECHO") = "0"
        ioRow("KAIDISTANCETTL") = "0"
        ioRow("DELFLG") = C_DELETE_FLG.ALIVE

        ioRow("DATAKBN") = ""
        ioRow("NIPPOLINKCODE") = ""
        ioRow("SHIPORG") = ""
        ioRow("SHIPORGNAMES") = ""
        ioRow("NIPPONO") = ""
        ioRow("GSHABAN") = ""
        ioRow("RUIDISTANCE") = 0
        ioRow("JIDISTANCE") = 0
        ioRow("KUDISTANCE") = 0

        If ioRow.Table.Columns.Contains("APPLYID") Then
            ioRow("APPLYID") = ""
        End If
        If ioRow.Table.Columns.Contains("STATUS") Then
            ioRow("STATUS") = ""
        End If
        If ioRow.Table.Columns.Contains("STATUSTEXT") Then
            ioRow("STATUSTEXT") = ""
        End If
        If ioRow.Table.Columns.Contains("YENDTIME") Then
            ioRow("YENDTIME") = "00:00"
        End If
        If ioRow.Table.Columns.Contains("RIYU") Then
            ioRow("RIYU") = ""
        End If
        If ioRow.Table.Columns.Contains("RIYUNAMES") Then
            ioRow("RIYUNAMES") = ""
        End If
        If ioRow.Table.Columns.Contains("RIYUETC") Then
            ioRow("RIYUETC") = ""
        End If
        If ioRow.Table.Columns.Contains("ENTRYFLG") Then
            ioRow("ENTRYFLG") = "0"
        End If
        If ioRow.Table.Columns.Contains("DRAWALFLG") Then
            ioRow("DRAWALFLG") = "0"
        End If

        If ioRow.Table.Columns.Contains("LATITUDE") Then
            ioRow("LATITUDE") = ""
        End If
        If ioRow.Table.Columns.Contains("LONGITUDE") Then
            ioRow("LONGITUDE") = ""
        End If
        'NJS専用
        ioRow("SHACHUHAKKBN") = "0"
        ioRow("SHACHUHAKKBNNAMES") = ""
        ioRow("HAISOTIME") = "00:00"
        ioRow("NENMATUNISSU") = "0"
        ioRow("NENMATUNISSUCHO") = "0"
        ioRow("NENMATUNISSUTTL") = "0"
        ioRow("SHACHUHAKNISSU") = "0"
        ioRow("SHACHUHAKNISSUCHO") = "0"
        ioRow("SHACHUHAKNISSUTTL") = "0"
        ioRow("MODELDISTANCE") = "0"
        ioRow("MODELDISTANCECHO") = "0"
        ioRow("MODELDISTANCETTL") = "0"
        ioRow("JIKYUSHATIME") = "00:00"
        ioRow("JIKYUSHATIMECHO") = "00:00"
        ioRow("JIKYUSHATIMETTL") = "00:00"
        '近石専用
        ioRow("HDAIWORKTIME") = "00:00"
        ioRow("HDAIWORKTIMECHO") = "00:00"
        ioRow("HDAIWORKTIMETTL") = "00:00"
        ioRow("HDAINIGHTTIME") = "00:00"
        ioRow("HDAINIGHTTIMECHO") = "00:00"
        ioRow("HDAINIGHTTIMETTL") = "00:00"
        ioRow("SDAIWORKTIME") = "00:00"
        ioRow("SDAIWORKTIMECHO") = "00:00"
        ioRow("SDAIWORKTIMETTL") = "00:00"
        ioRow("SDAINIGHTTIME") = "00:00"
        ioRow("SDAINIGHTTIMECHO") = "00:00"
        ioRow("SDAINIGHTTIMETTL") = "00:00"
        ioRow("WWORKTIME") = "00:00"
        ioRow("WWORKTIMECHO") = "00:00"
        ioRow("WWORKTIMETTL") = "00:00"
        ioRow("JYOMUTIME") = "00:00"
        ioRow("JYOMUTIMECHO") = "00:00"
        ioRow("JYOMUTIMETTL") = "00:00"
        ioRow("HWORKNISSU") = "0"
        ioRow("HWORKNISSUCHO") = "0"
        ioRow("HWORKNISSUTTL") = "0"
        ioRow("KAITENCNT") = "0"
        ioRow("KAITENCNTCHO") = "0"
        ioRow("KAITENCNTTTL") = "0"
        ioRow("KAITENCNT1_1") = "0"
        ioRow("KAITENCNTCHO1_1") = "0"
        ioRow("KAITENCNTTTL1_1") = "0"
        ioRow("KAITENCNT1_2") = "0"
        ioRow("KAITENCNTCHO1_2") = "0"
        ioRow("KAITENCNTTTL1_2") = "0"
        ioRow("KAITENCNT1_3") = "0"
        ioRow("KAITENCNTCHO1_3") = "0"
        ioRow("KAITENCNTTTL1_3") = "0"
        ioRow("KAITENCNT1_4") = "0"
        ioRow("KAITENCNTCHO1_4") = "0"
        ioRow("KAITENCNTTTL1_4") = "0"
        ioRow("KAITENCNT2_1") = "0"
        ioRow("KAITENCNTCHO2_1") = "0"
        ioRow("KAITENCNTTTL2_1") = "0"
        ioRow("KAITENCNT2_2") = "0"
        ioRow("KAITENCNTCHO2_2") = "0"
        ioRow("KAITENCNTTTL2_2") = "0"
        ioRow("KAITENCNT2_3") = "0"
        ioRow("KAITENCNTCHO2_3") = "0"
        ioRow("KAITENCNTTTL2_3") = "0"
        ioRow("KAITENCNT2_4") = "0"
        ioRow("KAITENCNTCHO2_4") = "0"
        ioRow("KAITENCNTTTL2_4") = "0"
        'JKT専用
        ioRow("SENJYOCNT") = "0"
        ioRow("SENJYOCNTCHO") = "0"
        ioRow("UNLOADADDCNT1") = "0"
        ioRow("UNLOADADDCNT1CHO") = "0"
        ioRow("UNLOADADDCNT2") = "0"
        ioRow("UNLOADADDCNT2CHO") = "0"
        ioRow("UNLOADADDCNT3") = "0"
        ioRow("UNLOADADDCNT3CHO") = "0"
        ioRow("UNLOADADDCNT4") = "0"
        ioRow("UNLOADADDCNT4CHO") = "0"
        ioRow("LOADINGCNT1") = "0"
        ioRow("LOADINGCNT1CHO") = "0"
        ioRow("LOADINGCNT2") = "0"
        ioRow("LOADINGCNT2CHO") = "0"
        ioRow("SHORTDISTANCE1") = "0"
        ioRow("SHORTDISTANCE1CHO") = "0"
        ioRow("SHORTDISTANCE2") = "0"
        ioRow("SHORTDISTANCE2CHO") = "0"

    End Sub

    ' ***  集計エリア初期化
    Public Sub SumItem_Init(ByRef ioRow As DataRow)

        ioRow("WORKTIME") = "00:00"
        ioRow("MOVETIME") = "00:00"
        ioRow("ACTTIME") = "00:00"
        ioRow("HAIDISTANCE") = "0"
        ioRow("KAIDISTANCE") = "0"
        ioRow("UNLOADCNT") = "0"
        ioRow("BINDSTDATE") = "00:00"
        ioRow("BINDTIME") = "00:00"
        ioRow("NIPPOBREAKTIME") = "00:00"
        ioRow("BREAKTIME") = "00:00"
        ioRow("NIGHTTIME") = "00:00"
        ioRow("ORVERTIME") = "00:00"
        If ioRow.Table.Columns.Contains("ORVERTIMEADD") Then
            ioRow("ORVERTIMEADD") = "00:00"
        End If
        ioRow("WNIGHTTIME") = "00:00"
        If ioRow.Table.Columns.Contains("WNIGHTTIMEADD") Then
            ioRow("WNIGHTTIMEADD") = "00:00"
        End If
        ioRow("SWORKTIME") = "00:00"
        If ioRow.Table.Columns.Contains("SWORKTIMEADD") Then
            ioRow("SWORKTIMEADD") = "00:00"
        End If
        ioRow("SNIGHTTIME") = "00:00"
        If ioRow.Table.Columns.Contains("SNIGHTTIMEADD") Then
            ioRow("SNIGHTTIMEADD") = "00:00"
        End If
        ioRow("HWORKTIME") = "00:00"
        ioRow("HNIGHTTIME") = "00:00"
        ioRow("SHOUKETUNISSU") = "0"
        ioRow("KUMIKETUNISSU") = "0"
        ioRow("ETCKETUNISSU") = "0"
        ioRow("NENKYUNISSU") = "0"
        ioRow("TOKUKYUNISSU") = "0"
        ioRow("CHIKOKSOTAINISSU") = "0"
        ioRow("STOCKNISSU") = "0"
        ioRow("KYOTEIWEEKNISSU") = "0"
        ioRow("WEEKNISSU") = "0"
        ioRow("DAIKYUNISSU") = "0"
        ioRow("NENSHINISSU") = "0"
        ioRow("SHUKCHOKNNISSU") = "0"
        ioRow("SHUKCHOKNISSU") = "0"
        '2018/02/08 追加
        If ioRow.Table.Columns.Contains("SHUKCHOKNHLDNISSU") Then
            ioRow("SHUKCHOKNHLDNISSU") = "0"
        End If
        If ioRow.Table.Columns.Contains("SHUKCHOKHLDNISSU") Then
            ioRow("SHUKCHOKHLDNISSU") = "0"
        End If
        '2018/02/08 追加
        ioRow("TOKSAAKAISU") = "0"
        ioRow("TOKSABKAISU") = "0"
        ioRow("TOKSACKAISU") = "0"
        '2018/04/17 追加
        If ioRow.Table.Columns.Contains("TENKOKAISU") Then
            ioRow("TENKOKAISU") = "0"
        End If
        ioRow("HOANTIME") = "00:00"
        ioRow("KOATUTIME") = "00:00"
        ioRow("TOKUSA1TIME") = "00:00"
        ioRow("HAYADETIME") = "00:00"
        ioRow("PONPNISSU") = "0"
        ioRow("BULKNISSU") = "0"
        ioRow("TRAILERNISSU") = "0"
        ioRow("BKINMUKAISU") = "0"

        ioRow("RUIDISTANCE") = "0"
        ioRow("JIDISTANCE") = "0"
        ioRow("KUDISTANCE") = "0"

        'NJS専用
        ioRow("HAISOTIME") = "00:00"
        ioRow("NENMATUNISSU") = "0"
        ioRow("SHACHUHAKNISSU") = "0"
        ioRow("MODELDISTANCE") = "0"
        ioRow("JIKYUSHATIME") = "00:00"
        '近石専用
        ioRow("HDAIWORKTIME") = "00:00"
        ioRow("HDAINIGHTTIME") = "00:00"
        ioRow("SDAIWORKTIME") = "00:00"
        ioRow("SDAINIGHTTIME") = "00:00"
        ioRow("WWORKTIME") = "00:00"
        ioRow("JYOMUTIME") = "00:00"
        ioRow("HWORKNISSU") = "0"
        If ioRow("CAMPCODE") = "03" Then
            ioRow("WORKNISSU") = "0"
        End If
        ioRow("KAITENCNT") = "0"
        ioRow("KAITENCNT1_1") = "0"
        ioRow("KAITENCNT1_2") = "0"
        ioRow("KAITENCNT1_3") = "0"
        ioRow("KAITENCNT1_4") = "0"
        ioRow("KAITENCNT2_1") = "0"
        ioRow("KAITENCNT2_2") = "0"
        ioRow("KAITENCNT2_3") = "0"
        ioRow("KAITENCNT2_4") = "0"

        'JKT専用
        ioRow("SENJYOCNT") = "0"
        ioRow("UNLOADADDCNT1") = "0"
        ioRow("UNLOADADDCNT2") = "0"
        ioRow("UNLOADADDCNT3") = "0"
        ioRow("UNLOADADDCNT4") = "0"
        ioRow("LOADINGCNT1") = "0"
        ioRow("LOADINGCNT2") = "0"
        ioRow("SHORTDISTANCE1") = "0"
        ioRow("SHORTDISTANCE2") = "0"

    End Sub

    ' ***  日数エリア初期化
    Public Sub NissuItem_Init(ByRef ioRow As DataRow)

        ioRow("WORKNISSU") = "0"
        ioRow("WORKNISSUCHO") = "0"
        ioRow("WORKNISSUTTL") = "0"
        ioRow("SHOUKETUNISSU") = "0"
        ioRow("SHOUKETUNISSUCHO") = "0"
        ioRow("SHOUKETUNISSUTTL") = "0"
        ioRow("KUMIKETUNISSU") = "0"
        ioRow("KUMIKETUNISSUCHO") = "0"
        ioRow("KUMIKETUNISSUTTL") = "0"
        ioRow("ETCKETUNISSU") = "0"
        ioRow("ETCKETUNISSUCHO") = "0"
        ioRow("ETCKETUNISSUTTL") = "0"
        ioRow("NENKYUNISSU") = "0"
        ioRow("NENKYUNISSUCHO") = "0"
        ioRow("NENKYUNISSUTTL") = "0"
        ioRow("TOKUKYUNISSU") = "0"
        ioRow("TOKUKYUNISSUCHO") = "0"
        ioRow("TOKUKYUNISSUTTL") = "0"
        ioRow("CHIKOKSOTAINISSU") = "0"
        ioRow("CHIKOKSOTAINISSUCHO") = "0"
        ioRow("CHIKOKSOTAINISSUTTL") = "0"
        ioRow("STOCKNISSU") = "0"
        ioRow("STOCKNISSUCHO") = "0"
        ioRow("STOCKNISSUTTL") = "0"
        ioRow("KYOTEIWEEKNISSU") = "0"
        ioRow("KYOTEIWEEKNISSUCHO") = "0"
        ioRow("KYOTEIWEEKNISSUTTL") = "0"
        ioRow("WEEKNISSU") = "0"
        ioRow("WEEKNISSUCHO") = "0"
        ioRow("WEEKNISSUTTL") = "0"
        ioRow("DAIKYUNISSU") = "0"
        ioRow("DAIKYUNISSUCHO") = "0"
        ioRow("DAIKYUNISSUTTL") = "0"
        ioRow("NENSHINISSU") = "0"
        ioRow("NENSHINISSUCHO") = "0"
        ioRow("NENSHINISSUTTL") = "0"
        ioRow("SHUKCHOKNNISSU") = "0"
        ioRow("SHUKCHOKNNISSUCHO") = "0"
        ioRow("SHUKCHOKNNISSUTTL") = "0"
        ioRow("SHUKCHOKNISSU") = "0"
        ioRow("SHUKCHOKNISSUCHO") = "0"
        ioRow("SHUKCHOKNISSUTTL") = "0"
        'NJS専用
        ioRow("NENMATUNISSU") = "0"
        ioRow("NENMATUNISSUCHO") = "0"
        ioRow("NENMATUNISSUTTL") = "0"
        ioRow("SHACHUHAKNISSU") = "0"
        ioRow("SHACHUHAKNISSUCHO") = "0"
        ioRow("SHACHUHAKNISSUTTL") = "0"
        '近石専用
        ioRow("HWORKNISSU") = "0"
        ioRow("HWORKNISSUCHO") = "0"
        ioRow("HWORKNISSUTTL") = "0"
        'JKT専用（なし）

    End Sub

    ' ***  時間項目編集（HH:MM）
    Public Sub TimeItemFormat(ByRef ioRow As DataRow)

        If ioRow("WORKTIME") <> "" Then
            ioRow("WORKTIME") = formatHHMM(Val(ioRow("WORKTIME")))
        End If
        If ioRow("MOVETIME") <> "" Then
            ioRow("MOVETIME") = formatHHMM(Val(ioRow("MOVETIME")))
        End If
        If ioRow("ACTTIME") <> "" Then
            ioRow("ACTTIME") = formatHHMM(Val(ioRow("ACTTIME")))
        End If
        ioRow("BINDTIME") = formatHHMM(Val(ioRow("BINDTIME")))
        ioRow("BREAKTIME") = formatHHMM(Val(ioRow("BREAKTIME")))
        ioRow("BREAKTIMECHO") = formatHHMM(Val(ioRow("BREAKTIMECHO")))
        ioRow("BREAKTIMETTL") = formatHHMM(Val(ioRow("BREAKTIMETTL")))
        ioRow("NIGHTTIME") = formatHHMM(Val(ioRow("NIGHTTIME")))
        ioRow("NIGHTTIMECHO") = formatHHMM(Val(ioRow("NIGHTTIMECHO")))
        ioRow("NIGHTTIMETTL") = formatHHMM(Val(ioRow("NIGHTTIMETTL")))
        ioRow("ORVERTIME") = formatHHMM(Val(ioRow("ORVERTIME")))
        ioRow("ORVERTIMECHO") = formatHHMM(Val(ioRow("ORVERTIMECHO")))
        If ioRow.Table.Columns.Contains("ORVERTIMEADD") Then
            ioRow("ORVERTIMEADD") = formatHHMM(Val(ioRow("ORVERTIMEADD")))
        End If
        ioRow("ORVERTIMETTL") = formatHHMM(Val(ioRow("ORVERTIMETTL")))
        ioRow("WNIGHTTIME") = formatHHMM(Val(ioRow("WNIGHTTIME")))
        ioRow("WNIGHTTIMECHO") = formatHHMM(Val(ioRow("WNIGHTTIMECHO")))
        If ioRow.Table.Columns.Contains("WNIGHTTIMEADD") Then
            ioRow("WNIGHTTIMEADD") = formatHHMM(Val(ioRow("WNIGHTTIMEADD")))
        End If
        ioRow("WNIGHTTIMETTL") = formatHHMM(Val(ioRow("WNIGHTTIMETTL")))
        ioRow("SWORKTIME") = formatHHMM(Val(ioRow("SWORKTIME")))
        ioRow("SWORKTIMECHO") = formatHHMM(Val(ioRow("SWORKTIMECHO")))
        If ioRow.Table.Columns.Contains("SWORKTIMEADD") Then
            ioRow("SWORKTIMEADD") = formatHHMM(Val(ioRow("SWORKTIMEADD")))
        End If
        ioRow("SWORKTIMETTL") = formatHHMM(Val(ioRow("SWORKTIMETTL")))
        ioRow("SNIGHTTIME") = formatHHMM(Val(ioRow("SNIGHTTIME")))
        ioRow("SNIGHTTIMECHO") = formatHHMM(Val(ioRow("SNIGHTTIMECHO")))
        If ioRow.Table.Columns.Contains("SNIGHTTIMEADD") Then
            ioRow("SNIGHTTIMEADD") = formatHHMM(Val(ioRow("SNIGHTTIMEADD")))
        End If
        ioRow("SNIGHTTIMETTL") = formatHHMM(Val(ioRow("SNIGHTTIMETTL")))
        ioRow("HWORKTIME") = formatHHMM(Val(ioRow("HWORKTIME")))
        ioRow("HWORKTIMECHO") = formatHHMM(Val(ioRow("HWORKTIMECHO")))
        ioRow("HWORKTIMETTL") = formatHHMM(Val(ioRow("HWORKTIMETTL")))
        ioRow("HNIGHTTIME") = formatHHMM(Val(ioRow("HNIGHTTIME")))
        ioRow("HNIGHTTIMECHO") = formatHHMM(Val(ioRow("HNIGHTTIMECHO")))
        ioRow("HNIGHTTIMETTL") = formatHHMM(Val(ioRow("HNIGHTTIMETTL")))
        ioRow("HOANTIME") = formatHHMM(Val(ioRow("HOANTIME")))
        ioRow("HOANTIMECHO") = formatHHMM(Val(ioRow("HOANTIMECHO")))
        ioRow("HOANTIMETTL") = formatHHMM(Val(ioRow("HOANTIMETTL")))
        ioRow("KOATUTIME") = formatHHMM(Val(ioRow("KOATUTIME")))
        ioRow("KOATUTIMECHO") = formatHHMM(Val(ioRow("KOATUTIMECHO")))
        ioRow("KOATUTIMETTL") = formatHHMM(Val(ioRow("KOATUTIMETTL")))
        ioRow("TOKUSA1TIME") = formatHHMM(Val(ioRow("TOKUSA1TIME")))
        ioRow("TOKUSA1TIMECHO") = formatHHMM(Val(ioRow("TOKUSA1TIMECHO")))
        ioRow("TOKUSA1TIMETTL") = formatHHMM(Val(ioRow("TOKUSA1TIMETTL")))
        ioRow("HAYADETIME") = formatHHMM(Val(ioRow("HAYADETIME")))
        ioRow("HAYADETIMECHO") = formatHHMM(Val(ioRow("HAYADETIMECHO")))
        ioRow("HAYADETIMETTL") = formatHHMM(Val(ioRow("HAYADETIMETTL")))

        'NJS用
        If ioRow("HAISOTIME") <> "" Then
            ioRow("HAISOTIME") = formatHHMM(Val(ioRow("HAISOTIME")))
        End If
        ioRow("JIKYUSHATIME") = formatHHMM(Val(ioRow("JIKYUSHATIME")))
        ioRow("JIKYUSHATIMECHO") = formatHHMM(Val(ioRow("JIKYUSHATIMECHO")))
        ioRow("JIKYUSHATIMETTL") = formatHHMM(Val(ioRow("JIKYUSHATIMETTL")))

        '近石用
        ioRow("HDAIWORKTIME") = formatHHMM(Val(ioRow("HDAIWORKTIME")))
        ioRow("HDAIWORKTIMECHO") = formatHHMM(Val(ioRow("HDAIWORKTIMECHO")))
        ioRow("HDAIWORKTIMETTL") = formatHHMM(Val(ioRow("HDAIWORKTIMETTL")))
        ioRow("HDAINIGHTTIME") = formatHHMM(Val(ioRow("HDAINIGHTTIME")))
        ioRow("HDAINIGHTTIMECHO") = formatHHMM(Val(ioRow("HDAINIGHTTIMECHO")))
        ioRow("HDAINIGHTTIMETTL") = formatHHMM(Val(ioRow("HDAINIGHTTIMETTL")))
        ioRow("SDAIWORKTIME") = formatHHMM(Val(ioRow("SDAIWORKTIME")))
        ioRow("SDAIWORKTIMECHO") = formatHHMM(Val(ioRow("SDAIWORKTIMECHO")))
        ioRow("SDAIWORKTIMETTL") = formatHHMM(Val(ioRow("SDAIWORKTIMETTL")))
        ioRow("SDAINIGHTTIME") = formatHHMM(Val(ioRow("SDAINIGHTTIME")))
        ioRow("SDAINIGHTTIMECHO") = formatHHMM(Val(ioRow("SDAINIGHTTIMECHO")))
        ioRow("SDAINIGHTTIMETTL") = formatHHMM(Val(ioRow("SDAINIGHTTIMETTL")))
        ioRow("JYOMUTIME") = formatHHMM(Val(ioRow("JYOMUTIME")))
        ioRow("JYOMUTIMECHO") = formatHHMM(Val(ioRow("JYOMUTIMECHO")))
        ioRow("JYOMUTIMETTL") = formatHHMM(Val(ioRow("JYOMUTIMETTL")))
        ioRow("WWORKTIME") = formatHHMM(Val(ioRow("WWORKTIME")))
        ioRow("WWORKTIMECHO") = formatHHMM(Val(ioRow("WWORKTIMECHO")))
        ioRow("WWORKTIMETTL") = formatHHMM(Val(ioRow("WWORKTIMETTL")))

    End Sub

    ' ***  出社時間切り上げ（５分単位）
    Function Minute5Edit(ByVal iParm As String) As String
        Dim WW_Minutes As Integer = HHMMtoMinutes(iParm)
        Dim WW_HH As Integer = 0
        Dim WW_MM As Integer = 0

        WW_HH = Int(Int(WW_Minutes / 5 + 0.8) / 12)
        WW_MM = (Int(WW_Minutes / 5 + 0.8) - (WW_HH * 12)) * 5

        Minute5Edit = Val(WW_HH).ToString("00") & ":" & Val(WW_MM).ToString("00")

    End Function

    ' ***  出社時間切り下げ（１０分単位）
    Function Minute10Edit(ByVal iParm As String) As String
        Dim WW_TIME As String() = iParm.Split(":")

        '５時以降
        If Val(WW_TIME(0)) >= 5 Then
            If Val(WW_TIME(1)) >= 50 And Val(WW_TIME(1)) <= 59 Then
                WW_TIME(1) = "50"
            End If
            If Val(WW_TIME(1)) >= 40 And Val(WW_TIME(1)) <= 49 Then
                WW_TIME(1) = "40"
            End If
            If Val(WW_TIME(1)) >= 30 And Val(WW_TIME(1)) <= 39 Then
                WW_TIME(1) = "30"
            End If
            If Val(WW_TIME(1)) >= 20 And Val(WW_TIME(1)) <= 29 Then
                WW_TIME(1) = "20"
            End If
            If Val(WW_TIME(1)) >= 10 And Val(WW_TIME(1)) <= 19 Then
                WW_TIME(1) = "10"
            End If
            If Val(WW_TIME(1)) >= 0 And Val(WW_TIME(1)) <= 9 Then
                WW_TIME(1) = "00"
            End If
            Minute10Edit = Val(WW_TIME(0)).ToString("00") & ":" & Val(WW_TIME(1)).ToString("00")
        Else
            Minute10Edit = iParm
        End If

    End Function

    ' ***  出社時間切り下げ（３０分単位）
    Function Minute30Edit(ByVal iParm As String) As String
        Dim WW_TIME As String() = iParm.Split(":")

        If Val(WW_TIME(1)) >= 0 And Val(WW_TIME(1)) <= 15 Then
            WW_TIME(1) = "00"
        End If
        If Val(WW_TIME(1)) > 15 And Val(WW_TIME(1)) <= 30 Then
            WW_TIME(1) = "30"
        End If
        If Val(WW_TIME(1)) > 30 And Val(WW_TIME(1)) <= 45 Then
            WW_TIME(1) = "30"
        End If
        If Val(WW_TIME(1)) > 45 And Val(WW_TIME(1)) <= 59 Then
            WW_TIME(0) = (Val(WW_TIME(0)) + 1).ToString("00")
            WW_TIME(1) = "00"
        End If
        Minute30Edit = Val(WW_TIME(0)).ToString("00") & ":" & Val(WW_TIME(1)).ToString("00")

    End Function

    ' ***  勤怠締テーブル取得
    Public Sub T00008get(ByRef iCampcode As String,
                         ByRef iOrg As String,
                         ByRef iTaishoYM As String,
                         ByRef oLIMITFLG As String,
                         ByRef oRtn As String)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite            'LogOutput DirString Get

        oRtn = C_MESSAGE_NO.NORMAL
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(A.LIMITFLG,0) as LIMITFLG " _
               & "  from  T0008_KINTAISTAT A " _
               & " where  CAMPCODE  = @CAMPCODE " _
               & "   and  ORGCODE   = @HORG " _
               & "   and  LIMITYM   = @TAISHOYM " _
               & "   and  DELFLG   <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar)
            '○関連受注指定
            PARA01.Value = iCampcode
            PARA02.Value = iOrg
            PARA03.Value = iTaishoYM

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                oLIMITFLG = SQLdr("LIMITFLG")
            End While

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0008_KINTAISTAT"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "T0008_KINTAISTAT SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  権限チェック取得
    Public Sub OrgCheck(ByRef iOrg As String,
                         ByRef oPERMITCODE As String,
                         ByRef oRtn As String)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite            'LogOutput DirString Get

        oRtn = C_MESSAGE_NO.NORMAL
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(B.PERMITCODE,0) as PERMITCODE                          " _
               & " FROM S0012_SRVAUTHOR as A                                            " _
               & " INNER JOIN S0006_ROLE      as B                                      " _
               & "         ON  B.CAMPCODE  = A.CAMPCODE                                 " _
               & "        and  B.OBJECT    = A.OBJECT                                   " _
               & "        and  B.ROLE      = A.ROLE                                     " _
               & "        and  B.CODE      = @ORG                                       " _
               & "        and  B.PERMITCODE > 1                                         " _
               & "        and  B.STYMD    <= @YMD                                       " _
               & "        and  B.ENDYMD   >= @YMD                                       " _
               & "        and  B.DELFLG   <> '1'                                        " _
               & " WHERE       A.TERMID    = @TERMID                                    " _
               & "        and  A.OBJECT    = 'SRVORG'                                   " _
               & "        and  A.STYMD    <= @YMD                                       " _
               & "        and  A.ENDYMD   >= @YMD                                       " _
               & "        and  A.DELFLG   <> '1'                                        "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@TERMID", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@ORG", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@YMD", System.Data.SqlDbType.NVarChar)
            '○関連受注指定
            PARA01.Value = CS0050Session.APSV_ID
            PARA02.Value = iOrg
            PARA03.Value = Date.Now

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                oPERMITCODE = SQLdr("PERMITCODE")
            End While

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "S0012_SRVAUTHOR"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "S0012_SRVAUTHOR SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    Public Sub L1Insert(ByVal iRow As DataRow, ByRef I_SQLcon As SqlConnection)

        '検索SQL文
        '〇配送受注DB登録
        Dim SQLStr As String =
                  " INSERT INTO L0001_TOKEI " _
                & " ( " _
                & "        CAMPCODE, " _
                & "        MOTOCHO, " _
                & "        VERSION, " _
                & "        DENTYPE, " _
                & "        TENKI, " _
                & "        KEIJOYMD, " _
                & "        DENYMD, " _
                & "        DENNO, " _
                & "        KANRENDENNO, " _
                & "        DTLNO, " _
                & "        INQKBN, " _
                & "        ACDCKBN, " _
                & "        ACACHANTEI, " _
                & "        ACCODE, " _
                & "        SUBACCODE, " _
                & "        ACTORICODE, " _
                & "        ACOILTYPE, " _
                & "        ACSHARYOTYPE, " _
                & "        ACTSHABAN, " _
                & "        ACSTAFFCODE, " _
                & "        ACBANKAC, " _
                & "        ACKEIJOMORG, " _
                & "        ACKEIJOORG, " _
                & "        ACTAXKBN, " _
                & "        ACAMT, " _
                & "        NACSHUKODATE, " _
                & "        NACSHUKADATE, " _
                & "        NACTODOKEDATE, " _
                & "        NACTORICODE, " _
                & "        NACURIKBN, " _
                & "        NACTODOKECODE, " _
                & "        NACSTORICODE, " _
                & "        NACSHUKABASHO, " _
                & "        NACTORITYPE01, " _
                & "        NACTORITYPE02, " _
                & "        NACTORITYPE03, " _
                & "        NACTORITYPE04, " _
                & "        NACTORITYPE05, " _
                & "        NACOILTYPE, " _
                & "        NACPRODUCT1, " _
                & "        NACPRODUCT2, " _
                & "        NACGSHABAN, " _
                & "        NACSUPPLIERKBN, " _
                & "        NACSUPPLIER, " _
                & "        NACSHARYOOILTYPE, " _
                & "        NACSHARYOTYPE1, " _
                & "        NACTSHABAN1, " _
                & "        NACMANGMORG1, " _
                & "        NACMANGSORG1, " _
                & "        NACMANGUORG1, " _
                & "        NACBASELEASE1, " _
                & "        NACSHARYOTYPE2, " _
                & "        NACTSHABAN2, " _
                & "        NACMANGMORG2, " _
                & "        NACMANGSORG2, " _
                & "        NACMANGUORG2, " _
                & "        NACBASELEASE2, " _
                & "        NACSHARYOTYPE3, " _
                & "        NACTSHABAN3, " _
                & "        NACMANGMORG3, " _
                & "        NACMANGSORG3, " _
                & "        NACMANGUORG3, " _
                & "        NACBASELEASE3, " _
                & "        NACCREWKBN, " _
                & "        NACSTAFFCODE, " _
                & "        NACSTAFFKBN, " _
                & "        NACMORG, " _
                & "        NACHORG, " _
                & "        NACSORG, " _
                & "        NACSTAFFCODE2, " _
                & "        NACSTAFFKBN2, " _
                & "        NACMORG2, " _
                & "        NACHORG2, " _
                & "        NACSORG2, " _
                & "        NACORDERNO, " _
                & "        NACDETAILNO, " _
                & "        NACTRIPNO, " _
                & "        NACDROPNO, " _
                & "        NACSEQ, " _
                & "        NACORDERORG, " _
                & "        NACSHIPORG, " _
                & "        NACSURYO, " _
                & "        NACTANI, " _
                & "        NACJSURYO, " _
                & "        NACSTANI, " _
                & "        NACHAIDISTANCE, " _
                & "        NACKAIDISTANCE, " _
                & "        NACCHODISTANCE, " _
                & "        NACTTLDISTANCE, " _
                & "        NACHAISTDATE, " _
                & "        NACHAIENDDATE, " _
                & "        NACHAIWORKTIME, " _
                & "        NACGESSTDATE, " _
                & "        NACGESENDDATE, " _
                & "        NACGESWORKTIME, " _
                & "        NACCHOWORKTIME, " _
                & "        NACTTLWORKTIME, " _
                & "        NACOUTWORKTIME, " _
                & "        NACBREAKSTDATE, " _
                & "        NACBREAKENDDATE, " _
                & "        NACBREAKTIME, " _
                & "        NACCHOBREAKTIME, " _
                & "        NACTTLBREAKTIME, " _
                & "        NACCASH, " _
                & "        NACETC, " _
                & "        NACTICKET, " _
                & "        NACKYUYU, " _
                & "        NACUNLOADCNT, " _
                & "        NACCHOUNLOADCNT, " _
                & "        NACTTLUNLOADCNT, " _
                & "        NACKAIJI, " _
                & "        NACJITIME, " _
                & "        NACJICHOSTIME, " _
                & "        NACJITTLETIME, " _
                & "        NACKUTIME, " _
                & "        NACKUCHOTIME, " _
                & "        NACKUTTLTIME, " _
                & "        NACJIDISTANCE, " _
                & "        NACJICHODISTANCE, " _
                & "        NACJITTLDISTANCE, " _
                & "        NACKUDISTANCE, " _
                & "        NACKUCHODISTANCE, " _
                & "        NACKUTTLDISTANCE, " _
                & "        NACTARIFFFARE, " _
                & "        NACFIXEDFARE, " _
                & "        NACINCHOFARE, " _
                & "        NACTTLFARE, " _
                & "        NACOFFICESORG, " _
                & "        NACOFFICETIME, " _
                & "        NACOFFICEBREAKTIME, " _
                & "        PAYSHUSHADATE, " _
                & "        PAYTAISHADATE, " _
                & "        PAYSTAFFKBN, " _
                & "        PAYSTAFFCODE, " _
                & "        PAYMORG, " _
                & "        PAYHORG, " _
                & "        PAYHOLIDAYKBN, " _
                & "        PAYKBN, " _
                & "        PAYSHUKCHOKKBN, " _
                & "        PAYJYOMUKBN, " _
                & "        PAYOILKBN, " _
                & "        PAYSHARYOKBN, " _
                & "        PAYWORKNISSU, " _
                & "        PAYSHOUKETUNISSU, " _
                & "        PAYKUMIKETUNISSU, " _
                & "        PAYETCKETUNISSU, " _
                & "        PAYNENKYUNISSU, " _
                & "        PAYTOKUKYUNISSU, " _
                & "        PAYCHIKOKSOTAINISSU, " _
                & "        PAYSTOCKNISSU, " _
                & "        PAYKYOTEIWEEKNISSU, " _
                & "        PAYWEEKNISSU, " _
                & "        PAYDAIKYUNISSU, " _
                & "        PAYWORKTIME, " _
                & "        PAYWWORKTIME, " _
                & "        PAYNIGHTTIME, " _
                & "        PAYORVERTIME, " _
                & "        PAYWNIGHTTIME, " _
                & "        PAYWSWORKTIME, " _
                & "        PAYSNIGHTTIME, " _
                & "        PAYSDAIWORKTIME, " _
                & "        PAYSDAINIGHTTIME, " _
                & "        PAYHWORKTIME, " _
                & "        PAYHNIGHTTIME, " _
                & "        PAYHDAIWORKTIME, " _
                & "        PAYHDAINIGHTTIME, " _
                & "        PAYBREAKTIME, " _
                & "        PAYNENSHINISSU, " _
                & "        PAYNENMATUNISSU, " _
                & "        PAYSHUKCHOKNNISSU, " _
                & "        PAYSHUKCHOKNISSU, " _
                & "        PAYSHUKCHOKNHLDNISSU, " _
                & "        PAYSHUKCHOKHLDNISSU, " _
                & "        PAYTOKSAAKAISU, " _
                & "        PAYTOKSABKAISU, " _
                & "        PAYTOKSACKAISU, " _
                & "        PAYTENKOKAISU, " _
                & "        PAYHOANTIME, " _
                & "        PAYKOATUTIME, " _
                & "        PAYTOKUSA1TIME, " _
                & "        PAYPONPNISSU, " _
                & "        PAYBULKNISSU, " _
                & "        PAYTRAILERNISSU, " _
                & "        PAYBKINMUKAISU, " _
                & "        PAYYENDTIME, " _
                & "        PAYAPPLYID, " _
                & "        PAYRIYU, " _
                & "        PAYRIYUETC, " _
                & "        PAYHAYADETIME, " _
                & "        PAYHAISOTIME, " _
                & "        PAYSHACHUHAKNISSU, " _
                & "        PAYMODELDISTANCE, " _
                & "        PAYJIKYUSHATIME, " _
                & "        PAYJYOMUTIME, " _
                & "        PAYHWORKNISSU, " _
                & "        PAYKAITENCNT, " _
                & "        PAYSENJYOCNT, " _
                & "        PAYUNLOADADDCNT1, " _
                & "        PAYUNLOADADDCNT2, " _
                & "        PAYUNLOADADDCNT3, " _
                & "        PAYUNLOADADDCNT4, " _
                & "        PAYSHORTDISTANCE1, " _
                & "        PAYSHORTDISTANCE2, " _
                & "        APPKIJUN, " _
                & "        APPKEY, " _
                & "        WORKKBN, " _
                & "        KEYSTAFFCODE, " _
                & "        KEYGSHABAN, " _
                & "        KEYTRIPNO, " _
                & "        KEYDROPNO, " _
                & "        DELFLG, " _
                & "        INITYMD, " _
                & "        UPDYMD, " _
                & "        UPDUSER, " _
                & "        UPDTERMID, " _
                & "        RECEIVEYMD " _
                & " ) " _
                & " VALUES(  " _
                & "        @CAMPCODE, " _
                & "        @MOTOCHO, " _
                & "        @VERSION, " _
                & "        @DENTYPE, " _
                & "        @TENKI, " _
                & "        @KEIJOYMD, " _
                & "        @DENYMD, " _
                & "        @DENNO, " _
                & "        @KANRENDENNO, " _
                & "        @DTLNO, " _
                & "        @INQKBN, " _
                & "        @ACDCKBN, " _
                & "        @ACACHANTEI, " _
                & "        @ACCODE, " _
                & "        @SUBACCODE, " _
                & "        @ACTORICODE, " _
                & "        @ACOILTYPE, " _
                & "        @ACSHARYOTYPE, " _
                & "        @ACTSHABAN, " _
                & "        @ACSTAFFCODE, " _
                & "        @ACBANKAC, " _
                & "        @ACKEIJOMORG, " _
                & "        @ACKEIJOORG, " _
                & "        @ACTAXKBN, " _
                & "        @ACAMT, " _
                & "        @NACSHUKODATE, " _
                & "        @NACSHUKADATE, " _
                & "        @NACTODOKEDATE, " _
                & "        @NACTORICODE, " _
                & "        @NACURIKBN, " _
                & "        @NACTODOKECODE, " _
                & "        @NACSTORICODE, " _
                & "        @NACSHUKABASHO, " _
                & "        @NACTORITYPE01, " _
                & "        @NACTORITYPE02, " _
                & "        @NACTORITYPE03, " _
                & "        @NACTORITYPE04, " _
                & "        @NACTORITYPE05, " _
                & "        @NACOILTYPE, " _
                & "        @NACPRODUCT1, " _
                & "        @NACPRODUCT2, " _
                & "        @NACGSHABAN, " _
                & "        @NACSUPPLIERKBN, " _
                & "        @NACSUPPLIER, " _
                & "        @NACSHARYOOILTYPE, " _
                & "        @NACSHARYOTYPE1, " _
                & "        @NACTSHABAN1, " _
                & "        @NACMANGMORG1, " _
                & "        @NACMANGSORG1, " _
                & "        @NACMANGUORG1, " _
                & "        @NACBASELEASE1, " _
                & "        @NACSHARYOTYPE2, " _
                & "        @NACTSHABAN2, " _
                & "        @NACMANGMORG2, " _
                & "        @NACMANGSORG2, " _
                & "        @NACMANGUORG2, " _
                & "        @NACBASELEASE2, " _
                & "        @NACSHARYOTYPE3, " _
                & "        @NACTSHABAN3, " _
                & "        @NACMANGMORG3, " _
                & "        @NACMANGSORG3, " _
                & "        @NACMANGUORG3, " _
                & "        @NACBASELEASE3, " _
                & "        @NACCREWKBN, " _
                & "        @NACSTAFFCODE, " _
                & "        @NACSTAFFKBN, " _
                & "        @NACMORG, " _
                & "        @NACHORG, " _
                & "        @NACSORG, " _
                & "        @NACSTAFFCODE2, " _
                & "        @NACSTAFFKBN2, " _
                & "        @NACMORG2, " _
                & "        @NACHORG2, " _
                & "        @NACSORG2, " _
                & "        @NACORDERNO, " _
                & "        @NACDETAILNO, " _
                & "        @NACTRIPNO, " _
                & "        @NACDROPNO, " _
                & "        @NACSEQ, " _
                & "        @NACORDERORG, " _
                & "        @NACSHIPORG, " _
                & "        @NACSURYO, " _
                & "        @NACTANI, " _
                & "        @NACJSURYO, " _
                & "        @NACSTANI, " _
                & "        @NACHAIDISTANCE, " _
                & "        @NACKAIDISTANCE, " _
                & "        @NACCHODISTANCE, " _
                & "        @NACTTLDISTANCE, " _
                & "        @NACHAISTDATE, " _
                & "        @NACHAIENDDATE, " _
                & "        @NACHAIWORKTIME, " _
                & "        @NACGESSTDATE, " _
                & "        @NACGESENDDATE, " _
                & "        @NACGESWORKTIME, " _
                & "        @NACCHOWORKTIME, " _
                & "        @NACTTLWORKTIME, " _
                & "        @NACOUTWORKTIME, " _
                & "        @NACBREAKSTDATE, " _
                & "        @NACBREAKENDDATE, " _
                & "        @NACBREAKTIME, " _
                & "        @NACCHOBREAKTIME, " _
                & "        @NACTTLBREAKTIME, " _
                & "        @NACCASH, " _
                & "        @NACETC, " _
                & "        @NACTICKET, " _
                & "        @NACKYUYU, " _
                & "        @NACUNLOADCNT, " _
                & "        @NACCHOUNLOADCNT, " _
                & "        @NACTTLUNLOADCNT, " _
                & "        @NACKAIJI, " _
                & "        @NACJITIME, " _
                & "        @NACJICHOSTIME, " _
                & "        @NACJITTLETIME, " _
                & "        @NACKUTIME, " _
                & "        @NACKUCHOTIME, " _
                & "        @NACKUTTLTIME, " _
                & "        @NACJIDISTANCE, " _
                & "        @NACJICHODISTANCE, " _
                & "        @NACJITTLDISTANCE, " _
                & "        @NACKUDISTANCE, " _
                & "        @NACKUCHODISTANCE, " _
                & "        @NACKUTTLDISTANCE, " _
                & "        @NACTARIFFFARE, " _
                & "        @NACFIXEDFARE, " _
                & "        @NACINCHOFARE, " _
                & "        @NACTTLFARE, " _
                & "        @NACOFFICESORG, " _
                & "        @NACOFFICETIME, " _
                & "        @NACOFFICEBREAKTIME, " _
                & "        @PAYSHUSHADATE, " _
                & "        @PAYTAISHADATE, " _
                & "        @PAYSTAFFKBN, " _
                & "        @PAYSTAFFCODE, " _
                & "        @PAYMORG, " _
                & "        @PAYHORG, " _
                & "        @PAYHOLIDAYKBN, " _
                & "        @PAYKBN, " _
                & "        @PAYSHUKCHOKKBN, " _
                & "        @PAYJYOMUKBN, " _
                & "        @PAYOILKBN, " _
                & "        @PAYSHARYOKBN, " _
                & "        @PAYWORKNISSU, " _
                & "        @PAYSHOUKETUNISSU, " _
                & "        @PAYKUMIKETUNISSU, " _
                & "        @PAYETCKETUNISSU, " _
                & "        @PAYNENKYUNISSU, " _
                & "        @PAYTOKUKYUNISSU, " _
                & "        @PAYCHIKOKSOTAINISSU, " _
                & "        @PAYSTOCKNISSU, " _
                & "        @PAYKYOTEIWEEKNISSU, " _
                & "        @PAYWEEKNISSU, " _
                & "        @PAYDAIKYUNISSU, " _
                & "        @PAYWORKTIME, " _
                & "        @PAYWWORKTIME, " _
                & "        @PAYNIGHTTIME, " _
                & "        @PAYORVERTIME, " _
                & "        @PAYWNIGHTTIME, " _
                & "        @PAYWSWORKTIME, " _
                & "        @PAYSNIGHTTIME, " _
                & "        @PAYSDAIWORKTIME, " _
                & "        @PAYSDAINIGHTTIME, " _
                & "        @PAYHWORKTIME, " _
                & "        @PAYHNIGHTTIME, " _
                & "        @PAYHDAIWORKTIME, " _
                & "        @PAYHDAINIGHTTIME, " _
                & "        @PAYBREAKTIME, " _
                & "        @PAYNENSHINISSU, " _
                & "        @PAYNENMATUNISSU, " _
                & "        @PAYSHUKCHOKNNISSU, " _
                & "        @PAYSHUKCHOKNISSU, " _
                & "        @PAYSHUKCHOKNHLDNISSU, " _
                & "        @PAYSHUKCHOKHLDNISSU, " _
                & "        @PAYTOKSAAKAISU, " _
                & "        @PAYTOKSABKAISU, " _
                & "        @PAYTOKSACKAISU, " _
                & "        @PAYTENKOKAISU, " _
                & "        @PAYHOANTIME, " _
                & "        @PAYKOATUTIME, " _
                & "        @PAYTOKUSA1TIME, " _
                & "        @PAYPONPNISSU, " _
                & "        @PAYBULKNISSU, " _
                & "        @PAYTRAILERNISSU, " _
                & "        @PAYBKINMUKAISU, " _
                & "        @PAYYENDTIME, " _
                & "        @PAYAPPLYID, " _
                & "        @PAYRIYU, " _
                & "        @PAYRIYUETC, " _
                & "        @PAYHAYADETIME, " _
                & "        @PAYHAISOTIME, " _
                & "        @PAYSHACHUHAKNISSU, " _
                & "        @PAYMODELDISTANCE, " _
                & "        @PAYJIKYUSHATIME, " _
                & "        @PAYJYOMUTIME, " _
                & "        @PAYHWORKNISSU, " _
                & "        @PAYKAITENCNT, " _
                & "        @PAYSENJYOCNT, " _
                & "        @PAYUNLOADADDCNT1, " _
                & "        @PAYUNLOADADDCNT2, " _
                & "        @PAYUNLOADADDCNT3, " _
                & "        @PAYUNLOADADDCNT4, " _
                & "        @PAYSHORTDISTANCE1, " _
                & "        @PAYSHORTDISTANCE2, " _
                & "        @APPKIJUN, " _
                & "        @APPKEY, " _
                & "        @WORKKBN, " _
                & "        @KEYSTAFFCODE, " _
                & "        @KEYGSHABAN, " _
                & "        @KEYTRIPNO, " _
                & "        @KEYDROPNO, " _
                & "        @DELFLG, " _
                & "        @INITYMD, " _
                & "        @UPDYMD, " _
                & "        @UPDUSER, " _
                & "        @UPDTERMID, " _
                & "        @RECEIVEYMD); "

        Dim SQLcmd As New SqlCommand(SQLStr, I_SQLcon)
        Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_MOTOCHO As SqlParameter = SQLcmd.Parameters.Add("@MOTOCHO", System.Data.SqlDbType.NVarChar, 20)
        Dim P_VERSION As SqlParameter = SQLcmd.Parameters.Add("@VERSION", System.Data.SqlDbType.NVarChar, 3)
        Dim P_DENTYPE As SqlParameter = SQLcmd.Parameters.Add("@DENTYPE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_TENKI As SqlParameter = SQLcmd.Parameters.Add("@TENKI", System.Data.SqlDbType.NVarChar, 20)
        Dim P_KEIJOYMD As SqlParameter = SQLcmd.Parameters.Add("@KEIJOYMD", System.Data.SqlDbType.Date)
        Dim P_DENYMD As SqlParameter = SQLcmd.Parameters.Add("@DENYMD", System.Data.SqlDbType.Date)
        Dim P_DENNO As SqlParameter = SQLcmd.Parameters.Add("@DENNO", System.Data.SqlDbType.NVarChar, 20)
        Dim P_KANRENDENNO As SqlParameter = SQLcmd.Parameters.Add("@KANRENDENNO", System.Data.SqlDbType.NVarChar, 50)
        Dim P_DTLNO As SqlParameter = SQLcmd.Parameters.Add("@DTLNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_INQKBN As SqlParameter = SQLcmd.Parameters.Add("@INQKBN", System.Data.SqlDbType.NVarChar, 10)
        Dim P_ACDCKBN As SqlParameter = SQLcmd.Parameters.Add("@ACDCKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_ACACHANTEI As SqlParameter = SQLcmd.Parameters.Add("@ACACHANTEI", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACCODE As SqlParameter = SQLcmd.Parameters.Add("@ACCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_SUBACCODE As SqlParameter = SQLcmd.Parameters.Add("@SUBACCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACTORICODE As SqlParameter = SQLcmd.Parameters.Add("@ACTORICODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACOILTYPE As SqlParameter = SQLcmd.Parameters.Add("@ACOILTYPE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACSHARYOTYPE As SqlParameter = SQLcmd.Parameters.Add("@ACSHARYOTYPE", System.Data.SqlDbType.NVarChar, 1)
        Dim P_ACTSHABAN As SqlParameter = SQLcmd.Parameters.Add("@ACTSHABAN", System.Data.SqlDbType.NVarChar, 19)
        Dim P_ACSTAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@ACSTAFFCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACBANKAC As SqlParameter = SQLcmd.Parameters.Add("@ACBANKAC", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACKEIJOMORG As SqlParameter = SQLcmd.Parameters.Add("@ACKEIJOMORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACKEIJOORG As SqlParameter = SQLcmd.Parameters.Add("@ACKEIJOORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_ACTAXKBN As SqlParameter = SQLcmd.Parameters.Add("@ACTAXKBN", System.Data.SqlDbType.NVarChar, 10)
        Dim P_ACAMT As SqlParameter = SQLcmd.Parameters.Add("@ACAMT", System.Data.SqlDbType.Int)
        Dim P_NACSHUKODATE As SqlParameter = SQLcmd.Parameters.Add("@NACSHUKODATE", System.Data.SqlDbType.Date)
        Dim P_NACSHUKADATE As SqlParameter = SQLcmd.Parameters.Add("@NACSHUKADATE", System.Data.SqlDbType.Date)
        Dim P_NACTODOKEDATE As SqlParameter = SQLcmd.Parameters.Add("@NACTODOKEDATE", System.Data.SqlDbType.Date)
        Dim P_NACTORICODE As SqlParameter = SQLcmd.Parameters.Add("@NACTORICODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACURIKBN As SqlParameter = SQLcmd.Parameters.Add("@NACURIKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_NACTODOKECODE As SqlParameter = SQLcmd.Parameters.Add("@NACTODOKECODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSTORICODE As SqlParameter = SQLcmd.Parameters.Add("@NACSTORICODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSHUKABASHO As SqlParameter = SQLcmd.Parameters.Add("@NACSHUKABASHO", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACTORITYPE01 As SqlParameter = SQLcmd.Parameters.Add("@NACTORITYPE01", System.Data.SqlDbType.NVarChar, 3)
        Dim P_NACTORITYPE02 As SqlParameter = SQLcmd.Parameters.Add("@NACTORITYPE02", System.Data.SqlDbType.NVarChar, 3)
        Dim P_NACTORITYPE03 As SqlParameter = SQLcmd.Parameters.Add("@NACTORITYPE03", System.Data.SqlDbType.NVarChar, 3)
        Dim P_NACTORITYPE04 As SqlParameter = SQLcmd.Parameters.Add("@NACTORITYPE04", System.Data.SqlDbType.NVarChar, 3)
        Dim P_NACTORITYPE05 As SqlParameter = SQLcmd.Parameters.Add("@NACTORITYPE05", System.Data.SqlDbType.NVarChar, 3)
        Dim P_NACOILTYPE As SqlParameter = SQLcmd.Parameters.Add("@NACOILTYPE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACPRODUCT1 As SqlParameter = SQLcmd.Parameters.Add("@NACPRODUCT1", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACPRODUCT2 As SqlParameter = SQLcmd.Parameters.Add("@NACPRODUCT2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACGSHABAN As SqlParameter = SQLcmd.Parameters.Add("@NACGSHABAN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSUPPLIERKBN As SqlParameter = SQLcmd.Parameters.Add("@NACSUPPLIERKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_NACSUPPLIER As SqlParameter = SQLcmd.Parameters.Add("@NACSUPPLIER", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSHARYOOILTYPE As SqlParameter = SQLcmd.Parameters.Add("@NACSHARYOOILTYPE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSHARYOTYPE1 As SqlParameter = SQLcmd.Parameters.Add("@NACSHARYOTYPE1", System.Data.SqlDbType.NVarChar, 1)
        Dim P_NACTSHABAN1 As SqlParameter = SQLcmd.Parameters.Add("@NACTSHABAN1", System.Data.SqlDbType.NVarChar, 19)
        Dim P_NACMANGMORG1 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGMORG1", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACMANGSORG1 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGSORG1", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACMANGUORG1 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGUORG1", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACBASELEASE1 As SqlParameter = SQLcmd.Parameters.Add("@NACBASELEASE1", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSHARYOTYPE2 As SqlParameter = SQLcmd.Parameters.Add("@NACSHARYOTYPE2", System.Data.SqlDbType.NVarChar, 1)
        Dim P_NACTSHABAN2 As SqlParameter = SQLcmd.Parameters.Add("@NACTSHABAN2", System.Data.SqlDbType.NVarChar, 19)
        Dim P_NACMANGMORG2 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGMORG2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACMANGSORG2 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGSORG2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACMANGUORG2 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGUORG2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACBASELEASE2 As SqlParameter = SQLcmd.Parameters.Add("@NACBASELEASE2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSHARYOTYPE3 As SqlParameter = SQLcmd.Parameters.Add("@NACSHARYOTYPE3", System.Data.SqlDbType.NVarChar, 1)
        Dim P_NACTSHABAN3 As SqlParameter = SQLcmd.Parameters.Add("@NACTSHABAN3", System.Data.SqlDbType.NVarChar, 19)
        Dim P_NACMANGMORG3 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGMORG3", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACMANGSORG3 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGSORG3", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACMANGUORG3 As SqlParameter = SQLcmd.Parameters.Add("@NACMANGUORG3", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACBASELEASE3 As SqlParameter = SQLcmd.Parameters.Add("@NACBASELEASE3", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACCREWKBN As SqlParameter = SQLcmd.Parameters.Add("@NACCREWKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_NACSTAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@NACSTAFFCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSTAFFKBN As SqlParameter = SQLcmd.Parameters.Add("@NACSTAFFKBN", System.Data.SqlDbType.NVarChar, 5)
        Dim P_NACMORG As SqlParameter = SQLcmd.Parameters.Add("@NACMORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACHORG As SqlParameter = SQLcmd.Parameters.Add("@NACHORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSORG As SqlParameter = SQLcmd.Parameters.Add("@NACSORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSTAFFCODE2 As SqlParameter = SQLcmd.Parameters.Add("@NACSTAFFCODE2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSTAFFKBN2 As SqlParameter = SQLcmd.Parameters.Add("@NACSTAFFKBN2", System.Data.SqlDbType.NVarChar, 5)
        Dim P_NACMORG2 As SqlParameter = SQLcmd.Parameters.Add("@NACMORG2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACHORG2 As SqlParameter = SQLcmd.Parameters.Add("@NACHORG2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSORG2 As SqlParameter = SQLcmd.Parameters.Add("@NACSORG2", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACORDERNO As SqlParameter = SQLcmd.Parameters.Add("@NACORDERNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_NACDETAILNO As SqlParameter = SQLcmd.Parameters.Add("@NACDETAILNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_NACTRIPNO As SqlParameter = SQLcmd.Parameters.Add("@NACTRIPNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_NACDROPNO As SqlParameter = SQLcmd.Parameters.Add("@NACDROPNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_NACSEQ As SqlParameter = SQLcmd.Parameters.Add("@NACSEQ", System.Data.SqlDbType.NVarChar, 2)
        Dim P_NACORDERORG As SqlParameter = SQLcmd.Parameters.Add("@NACORDERORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSHIPORG As SqlParameter = SQLcmd.Parameters.Add("@NACSHIPORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACSURYO As SqlParameter = SQLcmd.Parameters.Add("@NACSURYO", System.Data.SqlDbType.Decimal)
        Dim P_NACTANI As SqlParameter = SQLcmd.Parameters.Add("@NACTANI", System.Data.SqlDbType.NVarChar, 10)
        Dim P_NACJSURYO As SqlParameter = SQLcmd.Parameters.Add("@NACJSURYO", System.Data.SqlDbType.Decimal)
        Dim P_NACSTANI As SqlParameter = SQLcmd.Parameters.Add("@NACSTANI", System.Data.SqlDbType.NVarChar, 10)
        Dim P_NACHAIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACHAIDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACKAIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACKAIDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACCHODISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACCHODISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACTTLDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACTTLDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACHAISTDATE As SqlParameter = SQLcmd.Parameters.Add("@NACHAISTDATE", System.Data.SqlDbType.DateTime)
        Dim P_NACHAIENDDATE As SqlParameter = SQLcmd.Parameters.Add("@NACHAIENDDATE", System.Data.SqlDbType.DateTime)
        Dim P_NACHAIWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACHAIWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACGESSTDATE As SqlParameter = SQLcmd.Parameters.Add("@NACGESSTDATE", System.Data.SqlDbType.DateTime)
        Dim P_NACGESENDDATE As SqlParameter = SQLcmd.Parameters.Add("@NACGESENDDATE", System.Data.SqlDbType.DateTime)
        Dim P_NACGESWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACGESWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACCHOWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACCHOWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACTTLWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACTTLWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACOUTWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACOUTWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACBREAKSTDATE As SqlParameter = SQLcmd.Parameters.Add("@NACBREAKSTDATE", System.Data.SqlDbType.DateTime)
        Dim P_NACBREAKENDDATE As SqlParameter = SQLcmd.Parameters.Add("@NACBREAKENDDATE", System.Data.SqlDbType.DateTime)
        Dim P_NACBREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACBREAKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACCHOBREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACCHOBREAKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACTTLBREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACTTLBREAKTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACCASH As SqlParameter = SQLcmd.Parameters.Add("@NACCASH", System.Data.SqlDbType.Int)
        Dim P_NACETC As SqlParameter = SQLcmd.Parameters.Add("@NACETC", System.Data.SqlDbType.Int)
        Dim P_NACTICKET As SqlParameter = SQLcmd.Parameters.Add("@NACTICKET", System.Data.SqlDbType.Int)
        Dim P_NACKYUYU As SqlParameter = SQLcmd.Parameters.Add("@NACKYUYU", System.Data.SqlDbType.Decimal)
        Dim P_NACUNLOADCNT As SqlParameter = SQLcmd.Parameters.Add("@NACUNLOADCNT", System.Data.SqlDbType.Decimal)
        Dim P_NACCHOUNLOADCNT As SqlParameter = SQLcmd.Parameters.Add("@NACCHOUNLOADCNT", System.Data.SqlDbType.Decimal)
        Dim P_NACTTLUNLOADCNT As SqlParameter = SQLcmd.Parameters.Add("@NACTTLUNLOADCNT", System.Data.SqlDbType.Decimal)
        Dim P_NACKAIJI As SqlParameter = SQLcmd.Parameters.Add("@NACKAIJI", System.Data.SqlDbType.Decimal)
        Dim P_NACJITIME As SqlParameter = SQLcmd.Parameters.Add("@NACJITIME", System.Data.SqlDbType.Decimal)
        Dim P_NACJICHOSTIME As SqlParameter = SQLcmd.Parameters.Add("@NACJICHOSTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACJITTLETIME As SqlParameter = SQLcmd.Parameters.Add("@NACJITTLETIME", System.Data.SqlDbType.Decimal)
        Dim P_NACKUTIME As SqlParameter = SQLcmd.Parameters.Add("@NACKUTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACKUCHOTIME As SqlParameter = SQLcmd.Parameters.Add("@NACKUCHOTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACKUTTLTIME As SqlParameter = SQLcmd.Parameters.Add("@NACKUTTLTIME", System.Data.SqlDbType.Decimal)
        Dim P_NACJIDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACJIDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACJICHODISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACJICHODISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACJITTLDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACJITTLDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACKUDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACKUDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACKUCHODISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACKUCHODISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACKUTTLDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@NACKUTTLDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_NACTARIFFFARE As SqlParameter = SQLcmd.Parameters.Add("@NACTARIFFFARE", System.Data.SqlDbType.Int)
        Dim P_NACFIXEDFARE As SqlParameter = SQLcmd.Parameters.Add("@NACFIXEDFARE", System.Data.SqlDbType.Int)
        Dim P_NACINCHOFARE As SqlParameter = SQLcmd.Parameters.Add("@NACINCHOFARE", System.Data.SqlDbType.Int)
        Dim P_NACTTLFARE As SqlParameter = SQLcmd.Parameters.Add("@NACTTLFARE", System.Data.SqlDbType.Int)
        Dim P_NACOFFICESORG As SqlParameter = SQLcmd.Parameters.Add("@NACOFFICESORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_NACOFFICETIME As SqlParameter = SQLcmd.Parameters.Add("@NACOFFICETIME", System.Data.SqlDbType.Decimal)
        Dim P_NACOFFICEBREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@NACOFFICEBREAKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHUSHADATE As SqlParameter = SQLcmd.Parameters.Add("@PAYSHUSHADATE", System.Data.SqlDbType.DateTime)
        Dim P_PAYTAISHADATE As SqlParameter = SQLcmd.Parameters.Add("@PAYTAISHADATE", System.Data.SqlDbType.DateTime)
        Dim P_PAYSTAFFKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYSTAFFKBN", System.Data.SqlDbType.NVarChar, 5)
        Dim P_PAYSTAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@PAYSTAFFCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYMORG As SqlParameter = SQLcmd.Parameters.Add("@PAYMORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYHORG As SqlParameter = SQLcmd.Parameters.Add("@PAYHORG", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYHOLIDAYKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYHOLIDAYKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_PAYKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYSHUKCHOKKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYSHUKCHOKKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYJYOMUKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYJYOMUKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYOILKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYOILKBN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_PAYSHARYOKBN As SqlParameter = SQLcmd.Parameters.Add("@PAYSHARYOKBN", System.Data.SqlDbType.NVarChar, 1)
        Dim P_PAYWORKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYWORKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHOUKETUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSHOUKETUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYKUMIKETUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYKUMIKETUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYETCKETUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYETCKETUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYNENKYUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYNENKYUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYTOKUKYUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYTOKUKYUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYCHIKOKSOTAINISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYCHIKOKSOTAINISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYSTOCKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSTOCKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYKYOTEIWEEKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYKYOTEIWEEKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYWEEKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYWEEKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYDAIKYUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYDAIKYUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYWWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYWWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYNIGHTTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYORVERTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYORVERTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYWNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYWNIGHTTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYWSWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYWSWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYSNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYSNIGHTTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYSDAIWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYSDAIWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYSDAINIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYSDAINIGHTTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYHWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYHNIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHNIGHTTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYHDAIWORKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHDAIWORKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYHDAINIGHTTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHDAINIGHTTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYBREAKTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYBREAKTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYNENSHINISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYNENSHINISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYNENMATUNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYNENMATUNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHUKCHOKNNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSHUKCHOKNNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHUKCHOKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSHUKCHOKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHUKCHOKNHLDNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSHUKCHOKNHLDNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHUKCHOKHLDNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSHUKCHOKHLDNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYTOKSAAKAISU As SqlParameter = SQLcmd.Parameters.Add("@PAYTOKSAAKAISU", System.Data.SqlDbType.Decimal)
        Dim P_PAYTOKSABKAISU As SqlParameter = SQLcmd.Parameters.Add("@PAYTOKSABKAISU", System.Data.SqlDbType.Decimal)
        Dim P_PAYTOKSACKAISU As SqlParameter = SQLcmd.Parameters.Add("@PAYTOKSACKAISU", System.Data.SqlDbType.Decimal)
        Dim P_PAYTENKOKAISU As SqlParameter = SQLcmd.Parameters.Add("@PAYTENKOKAISU", System.Data.SqlDbType.Decimal)
        Dim P_PAYHOANTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHOANTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYKOATUTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYKOATUTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYTOKUSA1TIME As SqlParameter = SQLcmd.Parameters.Add("@PAYTOKUSA1TIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYPONPNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYPONPNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYBULKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYBULKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYTRAILERNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYTRAILERNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYBKINMUKAISU As SqlParameter = SQLcmd.Parameters.Add("@PAYBKINMUKAISU", System.Data.SqlDbType.Decimal)
        Dim P_PAYAPPLYID As SqlParameter = SQLcmd.Parameters.Add("@PAYAPPLYID", System.Data.SqlDbType.NVarChar, 30)
        Dim P_PAYYENDTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYYENDTIME", System.Data.SqlDbType.NVarChar, 30)
        Dim P_PAYRIYU As SqlParameter = SQLcmd.Parameters.Add("@PAYRIYU", System.Data.SqlDbType.NVarChar, 2)
        Dim P_PAYRIYUETC As SqlParameter = SQLcmd.Parameters.Add("@PAYRIYUETC", System.Data.SqlDbType.NVarChar, 200)

        Dim P_PAYHAYADETIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHAYADETIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYHAISOTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYHAISOTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHACHUHAKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYSHACHUHAKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYMODELDISTANCE As SqlParameter = SQLcmd.Parameters.Add("@PAYMODELDISTANCE", System.Data.SqlDbType.Decimal)
        Dim P_PAYJIKYUSHATIME As SqlParameter = SQLcmd.Parameters.Add("@PAYJIKYUSHATIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYJYOMUTIME As SqlParameter = SQLcmd.Parameters.Add("@PAYJYOMUTIME", System.Data.SqlDbType.Decimal)
        Dim P_PAYHWORKNISSU As SqlParameter = SQLcmd.Parameters.Add("@PAYHWORKNISSU", System.Data.SqlDbType.Decimal)
        Dim P_PAYKAITENCNT As SqlParameter = SQLcmd.Parameters.Add("@PAYKAITENCNT", System.Data.SqlDbType.Decimal)
        Dim P_PAYSENJYOCNT As SqlParameter = SQLcmd.Parameters.Add("@PAYSENJYOCNT", System.Data.SqlDbType.Decimal)
        Dim P_PAYUNLOADADDCNT1 As SqlParameter = SQLcmd.Parameters.Add("@PAYUNLOADADDCNT1", System.Data.SqlDbType.Decimal)
        Dim P_PAYUNLOADADDCNT2 As SqlParameter = SQLcmd.Parameters.Add("@PAYUNLOADADDCNT2", System.Data.SqlDbType.Decimal)
        Dim P_PAYUNLOADADDCNT3 As SqlParameter = SQLcmd.Parameters.Add("@PAYUNLOADADDCNT3", System.Data.SqlDbType.Decimal)
        Dim P_PAYUNLOADADDCNT4 As SqlParameter = SQLcmd.Parameters.Add("@PAYUNLOADADDCNT4", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHORTDISTANCE1 As SqlParameter = SQLcmd.Parameters.Add("@PAYSHORTDISTANCE1", System.Data.SqlDbType.Decimal)
        Dim P_PAYSHORTDISTANCE2 As SqlParameter = SQLcmd.Parameters.Add("@PAYSHORTDISTANCE2", System.Data.SqlDbType.Decimal)

        Dim P_APPKIJUN As SqlParameter = SQLcmd.Parameters.Add("@APPKIJUN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_APPKEY As SqlParameter = SQLcmd.Parameters.Add("@APPKEY", System.Data.SqlDbType.NVarChar, 20)
        Dim P_WORKKBN As SqlParameter = SQLcmd.Parameters.Add("@WORKKBN", System.Data.SqlDbType.NVarChar, 2)
        Dim P_KEYSTAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@KEYSTAFFCODE", System.Data.SqlDbType.NVarChar, 20)
        Dim P_KEYGSHABAN As SqlParameter = SQLcmd.Parameters.Add("@KEYGSHABAN", System.Data.SqlDbType.NVarChar, 20)
        Dim P_KEYTRIPNO As SqlParameter = SQLcmd.Parameters.Add("@KEYTRIPNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_KEYDROPNO As SqlParameter = SQLcmd.Parameters.Add("@KEYDROPNO", System.Data.SqlDbType.NVarChar, 10)
        Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1)
        Dim P_INITYMD As SqlParameter = SQLcmd.Parameters.Add("@INITYMD", System.Data.SqlDbType.DateTime)
        Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
        Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar, 20)
        Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar, 30)
        Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

        P_CAMPCODE.Value = iRow("CAMPCODE")
        P_MOTOCHO.Value = iRow("MOTOCHO")
        P_VERSION.Value = iRow("VERSION")
        P_DENTYPE.Value = iRow("DENTYPE")
        P_TENKI.Value = iRow("TENKI")
        P_KEIJOYMD.Value = iRow("KEIJOYMD")
        P_DENYMD.Value = iRow("DENYMD")
        P_DENNO.Value = iRow("DENNO")
        P_KANRENDENNO.Value = iRow("KANRENDENNO")
        P_DTLNO.Value = iRow("DTLNO")
        P_INQKBN.Value = iRow("INQKBN")
        P_ACDCKBN.Value = iRow("ACDCKBN")
        P_ACACHANTEI.Value = iRow("ACACHANTEI")
        P_ACCODE.Value = iRow("ACCODE")
        P_SUBACCODE.Value = iRow("SUBACCODE")
        P_ACTORICODE.Value = iRow("ACTORICODE")
        P_ACOILTYPE.Value = iRow("ACOILTYPE")
        P_ACSHARYOTYPE.Value = iRow("ACSHARYOTYPE")
        P_ACTSHABAN.Value = iRow("ACTSHABAN")
        P_ACSTAFFCODE.Value = iRow("ACSTAFFCODE")
        P_ACBANKAC.Value = iRow("ACBANKAC")
        P_ACKEIJOMORG.Value = iRow("ACKEIJOMORG")
        P_ACKEIJOORG.Value = iRow("ACKEIJOORG")
        P_ACTAXKBN.Value = iRow("ACTAXKBN")
        P_ACAMT.Value = iRow("ACAMT")
        P_NACSHUKODATE.Value = iRow("NACSHUKODATE")
        P_NACSHUKADATE.Value = iRow("NACSHUKADATE")
        P_NACTODOKEDATE.Value = iRow("NACTODOKEDATE")
        P_NACTORICODE.Value = iRow("NACTORICODE")
        P_NACURIKBN.Value = iRow("NACURIKBN")
        P_NACTODOKECODE.Value = iRow("NACTODOKECODE")
        P_NACSTORICODE.Value = iRow("NACSTORICODE")
        P_NACSHUKABASHO.Value = iRow("NACSHUKABASHO")
        P_NACTORITYPE01.Value = iRow("NACTORITYPE01")
        P_NACTORITYPE02.Value = iRow("NACTORITYPE02")
        P_NACTORITYPE03.Value = iRow("NACTORITYPE03")
        P_NACTORITYPE04.Value = iRow("NACTORITYPE04")
        P_NACTORITYPE05.Value = iRow("NACTORITYPE05")
        P_NACOILTYPE.Value = iRow("NACOILTYPE")
        P_NACPRODUCT1.Value = iRow("NACPRODUCT1")
        P_NACPRODUCT2.Value = iRow("NACPRODUCT2")
        P_NACGSHABAN.Value = iRow("NACGSHABAN")
        P_NACSUPPLIERKBN.Value = iRow("NACSUPPLIERKBN")
        P_NACSUPPLIER.Value = iRow("NACSUPPLIER")
        P_NACSHARYOOILTYPE.Value = iRow("NACSHARYOOILTYPE")
        P_NACSHARYOTYPE1.Value = iRow("NACSHARYOTYPE1")
        P_NACTSHABAN1.Value = iRow("NACTSHABAN1")
        P_NACMANGMORG1.Value = iRow("NACMANGMORG1")
        P_NACMANGSORG1.Value = iRow("NACMANGSORG1")
        P_NACMANGUORG1.Value = iRow("NACMANGUORG1")
        P_NACBASELEASE1.Value = iRow("NACBASELEASE1")
        P_NACSHARYOTYPE2.Value = iRow("NACSHARYOTYPE2")
        P_NACTSHABAN2.Value = iRow("NACTSHABAN2")
        P_NACMANGMORG2.Value = iRow("NACMANGMORG2")
        P_NACMANGSORG2.Value = iRow("NACMANGSORG2")
        P_NACMANGUORG2.Value = iRow("NACMANGUORG2")
        P_NACBASELEASE2.Value = iRow("NACBASELEASE2")
        P_NACSHARYOTYPE3.Value = iRow("NACSHARYOTYPE3")
        P_NACTSHABAN3.Value = iRow("NACTSHABAN3")
        P_NACMANGMORG3.Value = iRow("NACMANGMORG3")
        P_NACMANGSORG3.Value = iRow("NACMANGSORG3")
        P_NACMANGUORG3.Value = iRow("NACMANGUORG3")
        P_NACBASELEASE3.Value = iRow("NACBASELEASE3")
        P_NACCREWKBN.Value = iRow("NACCREWKBN")
        P_NACSTAFFCODE.Value = iRow("NACSTAFFCODE")
        P_NACSTAFFKBN.Value = iRow("NACSTAFFKBN")
        P_NACMORG.Value = iRow("NACMORG")
        P_NACHORG.Value = iRow("NACHORG")
        P_NACSORG.Value = iRow("NACSORG")
        P_NACSTAFFCODE2.Value = iRow("NACSTAFFCODE2")
        P_NACSTAFFKBN2.Value = iRow("NACSTAFFKBN2")
        P_NACMORG2.Value = iRow("NACMORG2")
        P_NACHORG2.Value = iRow("NACHORG2")
        P_NACSORG2.Value = iRow("NACSORG2")
        P_NACORDERNO.Value = iRow("NACORDERNO")
        P_NACDETAILNO.Value = iRow("NACDETAILNO")
        P_NACTRIPNO.Value = iRow("NACTRIPNO")
        P_NACDROPNO.Value = iRow("NACDROPNO")
        P_NACSEQ.Value = iRow("NACSEQ")
        P_NACORDERORG.Value = iRow("NACORDERORG")
        P_NACSHIPORG.Value = iRow("NACSHIPORG")
        P_NACSURYO.Value = iRow("NACSURYO")
        P_NACTANI.Value = iRow("NACTANI")
        P_NACJSURYO.Value = iRow("NACJSURYO")
        P_NACSTANI.Value = iRow("NACSTANI")
        P_NACHAIDISTANCE.Value = iRow("NACHAIDISTANCE")
        P_NACKAIDISTANCE.Value = iRow("NACKAIDISTANCE")
        P_NACCHODISTANCE.Value = iRow("NACCHODISTANCE")
        P_NACTTLDISTANCE.Value = iRow("NACTTLDISTANCE")
        P_NACHAISTDATE.Value = iRow("NACHAISTDATE")
        P_NACHAIENDDATE.Value = iRow("NACHAIENDDATE")
        P_NACHAIWORKTIME.Value = iRow("NACHAIWORKTIME")
        P_NACGESSTDATE.Value = iRow("NACGESSTDATE")
        P_NACGESENDDATE.Value = iRow("NACGESENDDATE")
        P_NACGESWORKTIME.Value = iRow("NACGESWORKTIME")
        P_NACCHOWORKTIME.Value = iRow("NACCHOWORKTIME")
        P_NACTTLWORKTIME.Value = iRow("NACTTLWORKTIME")
        P_NACOUTWORKTIME.Value = iRow("NACOUTWORKTIME")
        P_NACBREAKSTDATE.Value = iRow("NACBREAKSTDATE")
        P_NACBREAKENDDATE.Value = iRow("NACBREAKENDDATE")
        P_NACBREAKTIME.Value = iRow("NACBREAKTIME")
        P_NACCHOBREAKTIME.Value = iRow("NACCHOBREAKTIME")
        P_NACTTLBREAKTIME.Value = iRow("NACTTLBREAKTIME")
        P_NACCASH.Value = iRow("NACCASH")
        P_NACETC.Value = iRow("NACETC")
        P_NACTICKET.Value = iRow("NACTICKET")
        P_NACKYUYU.Value = iRow("NACKYUYU")
        P_NACUNLOADCNT.Value = iRow("NACUNLOADCNT")
        P_NACCHOUNLOADCNT.Value = iRow("NACCHOUNLOADCNT")
        P_NACTTLUNLOADCNT.Value = iRow("NACTTLUNLOADCNT")
        P_NACKAIJI.Value = iRow("NACKAIJI")
        P_NACJITIME.Value = iRow("NACJITIME")
        P_NACJICHOSTIME.Value = iRow("NACJICHOSTIME")
        P_NACJITTLETIME.Value = iRow("NACJITTLETIME")
        P_NACKUTIME.Value = iRow("NACKUTIME")
        P_NACKUCHOTIME.Value = iRow("NACKUCHOTIME")
        P_NACKUTTLTIME.Value = iRow("NACKUTTLTIME")
        P_NACJIDISTANCE.Value = iRow("NACJIDISTANCE")
        P_NACJICHODISTANCE.Value = iRow("NACJICHODISTANCE")
        P_NACJITTLDISTANCE.Value = iRow("NACJITTLDISTANCE")
        P_NACKUDISTANCE.Value = iRow("NACKUDISTANCE")
        P_NACKUCHODISTANCE.Value = iRow("NACKUCHODISTANCE")
        P_NACKUTTLDISTANCE.Value = iRow("NACKUTTLDISTANCE")
        P_NACTARIFFFARE.Value = iRow("NACTARIFFFARE")
        P_NACFIXEDFARE.Value = iRow("NACFIXEDFARE")
        P_NACINCHOFARE.Value = iRow("NACINCHOFARE")
        P_NACTTLFARE.Value = iRow("NACTTLFARE")
        P_NACOFFICESORG.Value = iRow("NACOFFICESORG")
        P_NACOFFICETIME.Value = iRow("NACOFFICETIME")
        P_NACOFFICEBREAKTIME.Value = iRow("NACOFFICEBREAKTIME")
        P_PAYSHUSHADATE.Value = iRow("PAYSHUSHADATE")
        P_PAYTAISHADATE.Value = iRow("PAYTAISHADATE")
        P_PAYSTAFFKBN.Value = iRow("PAYSTAFFKBN")
        P_PAYSTAFFCODE.Value = iRow("PAYSTAFFCODE")
        P_PAYMORG.Value = iRow("PAYMORG")
        P_PAYHORG.Value = iRow("PAYHORG")
        P_PAYHOLIDAYKBN.Value = iRow("PAYHOLIDAYKBN")
        P_PAYKBN.Value = iRow("PAYKBN")
        P_PAYSHUKCHOKKBN.Value = iRow("PAYSHUKCHOKKBN")
        P_PAYJYOMUKBN.Value = iRow("PAYJYOMUKBN")
        P_PAYOILKBN.Value = iRow("PAYOILKBN")
        P_PAYSHARYOKBN.Value = iRow("PAYSHARYOKBN")
        P_PAYWORKNISSU.Value = iRow("PAYWORKNISSU")
        P_PAYSHOUKETUNISSU.Value = iRow("PAYSHOUKETUNISSU")
        P_PAYKUMIKETUNISSU.Value = iRow("PAYKUMIKETUNISSU")
        P_PAYETCKETUNISSU.Value = iRow("PAYETCKETUNISSU")
        P_PAYNENKYUNISSU.Value = iRow("PAYNENKYUNISSU")
        P_PAYTOKUKYUNISSU.Value = iRow("PAYTOKUKYUNISSU")
        P_PAYCHIKOKSOTAINISSU.Value = iRow("PAYCHIKOKSOTAINISSU")
        P_PAYSTOCKNISSU.Value = iRow("PAYSTOCKNISSU")
        P_PAYKYOTEIWEEKNISSU.Value = iRow("PAYKYOTEIWEEKNISSU")
        P_PAYWEEKNISSU.Value = iRow("PAYWEEKNISSU")
        P_PAYDAIKYUNISSU.Value = iRow("PAYDAIKYUNISSU")
        P_PAYWORKTIME.Value = iRow("PAYWORKTIME")
        P_PAYWWORKTIME.Value = iRow("PAYWWORKTIME")
        P_PAYNIGHTTIME.Value = iRow("PAYNIGHTTIME")
        P_PAYORVERTIME.Value = iRow("PAYORVERTIME")
        P_PAYWNIGHTTIME.Value = iRow("PAYWNIGHTTIME")
        P_PAYWSWORKTIME.Value = iRow("PAYWSWORKTIME")
        P_PAYSNIGHTTIME.Value = iRow("PAYSNIGHTTIME")
        P_PAYSDAIWORKTIME.Value = iRow("PAYSDAIWORKTIME")
        P_PAYSDAINIGHTTIME.Value = iRow("PAYSDAINIGHTTIME")
        P_PAYHWORKTIME.Value = iRow("PAYHWORKTIME")
        P_PAYHNIGHTTIME.Value = iRow("PAYHNIGHTTIME")
        P_PAYHDAIWORKTIME.Value = iRow("PAYHDAIWORKTIME")
        P_PAYHDAINIGHTTIME.Value = iRow("PAYHDAINIGHTTIME")
        P_PAYBREAKTIME.Value = iRow("PAYBREAKTIME")
        P_PAYNENSHINISSU.Value = iRow("PAYNENSHINISSU")
        P_PAYNENMATUNISSU.Value = iRow("PAYNENMATUNISSU")
        P_PAYSHUKCHOKNNISSU.Value = iRow("PAYSHUKCHOKNNISSU")
        P_PAYSHUKCHOKNISSU.Value = iRow("PAYSHUKCHOKNISSU")
        P_PAYSHUKCHOKNHLDNISSU.Value = iRow("PAYSHUKCHOKNHLDNISSU")
        P_PAYSHUKCHOKHLDNISSU.Value = iRow("PAYSHUKCHOKHLDNISSU")
        P_PAYTOKSAAKAISU.Value = iRow("PAYTOKSAAKAISU")
        P_PAYTOKSABKAISU.Value = iRow("PAYTOKSABKAISU")
        P_PAYTOKSACKAISU.Value = iRow("PAYTOKSACKAISU")
        P_PAYTENKOKAISU.Value = iRow("PAYTENKOKAISU")
        P_PAYHOANTIME.Value = iRow("PAYHOANTIME")
        P_PAYKOATUTIME.Value = iRow("PAYKOATUTIME")
        P_PAYTOKUSA1TIME.Value = iRow("PAYTOKUSA1TIME")
        P_PAYPONPNISSU.Value = iRow("PAYPONPNISSU")
        P_PAYBULKNISSU.Value = iRow("PAYBULKNISSU")
        P_PAYTRAILERNISSU.Value = iRow("PAYTRAILERNISSU")
        P_PAYBKINMUKAISU.Value = iRow("PAYBKINMUKAISU")
        P_PAYYENDTIME.Value = iRow("PAYYENDTIME")
        P_PAYAPPLYID.Value = iRow("PAYAPPLYID")
        P_PAYRIYU.Value = iRow("PAYRIYU")
        P_PAYRIYUETC.Value = iRow("PAYRIYUETC")

        P_PAYHAYADETIME.Value = iRow("PAYHAYADETIME")
        P_PAYHAISOTIME.Value = iRow("PAYHAISOTIME")
        P_PAYSHACHUHAKNISSU.Value = iRow("PAYSHACHUHAKNISSU")
        P_PAYMODELDISTANCE.Value = iRow("PAYMODELDISTANCE")
        P_PAYJIKYUSHATIME.Value = iRow("PAYJIKYUSHATIME")
        P_PAYJYOMUTIME.Value = iRow("PAYJYOMUTIME")
        P_PAYHWORKNISSU.Value = iRow("PAYHWORKNISSU")
        P_PAYKAITENCNT.Value = iRow("PAYKAITENCNT")
        P_PAYSENJYOCNT.Value = iRow("PAYSENJYOCNT")
        P_PAYUNLOADADDCNT1.Value = iRow("PAYUNLOADADDCNT1")
        P_PAYUNLOADADDCNT2.Value = iRow("PAYUNLOADADDCNT2")
        P_PAYUNLOADADDCNT3.Value = iRow("PAYUNLOADADDCNT3")
        P_PAYUNLOADADDCNT4.Value = iRow("PAYUNLOADADDCNT4")
        P_PAYSHORTDISTANCE1.Value = iRow("PAYSHORTDISTANCE1")
        P_PAYSHORTDISTANCE2.Value = iRow("PAYSHORTDISTANCE2")

        P_APPKIJUN.Value = iRow("APPKIJUN")
        P_APPKEY.Value = iRow("APPKEY")
        P_WORKKBN.Value = iRow("WORKKBN")
        P_KEYSTAFFCODE.Value = iRow("KEYSTAFFCODE")
        P_KEYGSHABAN.Value = iRow("KEYGSHABAN")
        P_KEYTRIPNO.Value = iRow("KEYTRIPNO")
        P_KEYDROPNO.Value = iRow("KEYDROPNO")
        P_DELFLG.Value = iRow("DELFLG")
        P_INITYMD.Value = iRow("INITYMD")
        P_UPDYMD.Value = iRow("UPDYMD")
        P_UPDUSER.Value = iRow("UPDUSER")
        P_UPDTERMID.Value = iRow("UPDTERMID")
        P_RECEIVEYMD.Value = iRow("RECEIVEYMD")

        SQLcmd.CommandTimeout = 300
        SQLcmd.ExecuteNonQuery()

        'CLOSE
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub


    ' ***  カレンダーＤＢ取得
    Public Sub MB005_Select(ByVal iCAMP As String,
                            ByRef iDate As String,
                            ByRef oWORKINGKBN As String,
                            ByRef oRtn As String)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite            'LogOutput DirString Get

        oRtn = C_MESSAGE_NO.NORMAL
        'オブジェクト内容検索
        Try
            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(rtrim(A.WORKINGKBN),'') as WORKINGKBN  " _
               & "  from  MB005_CALENDAR A " _
               & "  where A.CAMPCODE = @CAMPCODE " _
               & "    and A.WORKINGYMD = @WORKDATE " _
               & "    and A.DELFLG <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@WORKDATE", System.Data.SqlDbType.NVarChar)
            '○関連受注指定
            PARA01.Value = iCAMP
            PARA02.Value = iDate

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            While SQLdr.Read
                oWORKINGKBN = SQLdr("WORKINGKBN")
            End While

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB005_CALENDAR"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB005_CALENDAR SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  所定労働時間取得
    Public Sub WORKINGHget(ByRef iRow As DataRow,
                                ByRef oWORKINGH As String,
                                ByRef oRtn As String)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        oRtn = C_MESSAGE_NO.NORMAL
        Try
            Dim WW_MB004tbl As DataTable = New DataTable

            WW_MB004tbl.Columns.Add("WORKINGH", GetType(String))

            Dim SQLStr As String = ""
            'DataBase接続文字
            Dim SQLcon As SqlConnection = CS0050Session.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                 " select isnull(A.WORKINGH,'00:00:00') as WORKINGH " _
               & "  from  MB004_WORKINGH A " _
               & " where  CAMPCODE  = @CAMPCODE " _
               & "   and  HORG      = @HORG " _
               & "   and  STAFFKBN  = @STAFFKBN " _
               & "   and  A.STYMD  <= @STYMD " _
               & "   and  A.ENDYMD >= @ENDYMD " _
               & "   and  DELFLG   <> '1'  "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@HORG", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@STAFFKBN", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            '○関連受注指定
            PARA01.Value = iRow("CAMPCODE")
            PARA02.Value = iRow("HORG")
            PARA03.Value = iRow("STAFFKBN")
            PARA04.Value = iRow("WORKDATE")
            PARA05.Value = iRow("WORKDATE")

            '■SQL実行
            Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

            WW_MB004tbl.Load(SQLdr)

            oWORKINGH = "12:00"
            For Each MB4row As DataRow In WW_MB004tbl.Rows
                If IsDate(MB4row("WORKINGH")) Then
                    oWORKINGH = CDate(MB4row("WORKINGH")).ToString("hh:mm")
                End If
            Next

            SQLdr.Close()
            SQLdr = Nothing

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close() 'DataBase接続(Close)
            SQLcon.Dispose()
            SQLcon = Nothing

            WW_MB004tbl.Dispose()
            WW_MB004tbl = Nothing
        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "MB004_WORKINGH"                'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MB004_WORKINGH SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try


    End Sub

    ' ***  モデル距離テーブル
    Public Sub ModelDistanceTbl(ByRef iTBL As DataTable, ByVal iCAMP As String, ByVal iTAISHOYM As String,
                                ByVal iListBoxMODELCODE As ListBox, ByVal iListBoxMODELDISTANCE As ListBox,
                                ByRef oTBL As DataTable,
                                ByVal UPDUSERID As String, ByVal UPDTERMID As String)

        T0010tbl_ColumnsAdd(oTBL)

        Dim WW_WORKDATE As String = ""
        Dim WW_STAFFCODE As String = ""
        Dim WW_B3CNT As Integer = 0
        Dim WW_oTBLrow As DataRow = Nothing
        Dim WW_iTBL As DataTable = iTBL.Clone
        Dim WW_iTBL2 As DataTable = iTBL.Clone

        CS0026TblSort.TABLE = iTBL
        CS0026TblSort.FILTER = "WORKKBN='B3'"
        CS0026TblSort.SORTING = "YMD,STAFFCODE,STDATE,STTIME,ENDDATE,ENDTIME,WORKKBN"
        WW_iTBL = CS0026TblSort.sort()

        CS0026TblSort.TABLE = iTBL
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "YMD,STAFFCODE,STDATE DESC,STTIME DESC,ENDDATE DESC,ENDTIME DESC,WORKKBN"
        WW_iTBL2 = CS0026TblSort.sort()

        For i As Integer = 0 To WW_iTBL2.Rows.Count - 1
            Dim WW_iTBLrow As DataRow = WW_iTBL2.Rows(i)
            If i = 0 Then
                WW_WORKDATE = WW_iTBLrow("YMD")
                WW_STAFFCODE = WW_iTBLrow("STAFFCODE")
            End If

            If WW_WORKDATE = WW_iTBLrow("YMD") And
               WW_STAFFCODE = WW_iTBLrow("STAFFCODE") Then

                If WW_iTBLrow("WORKKBN") = "B3" Then
                    WW_B3CNT += 1
                End If

                If WW_iTBLrow("WORKKBN") = "B2" Then
                    If WW_B3CNT = 0 Then
                        WW_oTBLrow = WW_iTBL.NewRow
                        WW_oTBLrow.ItemArray = WW_iTBLrow.ItemArray
                        WW_iTBL.Rows.Add(WW_oTBLrow)
                    End If
                End If
            Else
                WW_B3CNT = 0
            End If
            WW_WORKDATE = WW_iTBLrow("YMD")
            WW_STAFFCODE = WW_iTBLrow("STAFFCODE")
        Next

        CS0026TblSort.TABLE = WW_iTBL
        CS0026TblSort.FILTER = ""
        CS0026TblSort.SORTING = "YMD,STAFFCODE,STDATE,STTIME,ENDDATE,ENDTIME,WORKKBN"
        WW_iTBL = CS0026TblSort.sort()

        Dim WW_SHUKABASHO As String = ""
        Dim WW_TODOKECODE As String = ""
        Dim WW_SHARYOKBN As String = ""
        Dim WW_OILPAYKBN As String = ""
        Dim WW_MODELDISTANCE As String = ""
        Dim WW_MODIFYKBN As String = ""
        WW_B3CNT = 0
        For i As Integer = 0 To WW_iTBL.Rows.Count - 1
            Dim WW_iTBLrow As DataRow = WW_iTBL.Rows(i)
            If i = 0 Then
                WW_WORKDATE = WW_iTBLrow("YMD")
                WW_STAFFCODE = WW_iTBLrow("STAFFCODE")
                WW_oTBLrow = oTBL.NewRow
            End If

            If WW_WORKDATE = WW_iTBLrow("YMD") And
               WW_STAFFCODE = WW_iTBLrow("STAFFCODE") Then
                WW_oTBLrow("WORKDATE") = WW_iTBLrow("YMD")
                WW_oTBLrow("STAFFCODE") = WW_iTBLrow("STAFFCODE")

                WW_B3CNT += 1
                WW_SHUKABASHO = "SHUKABASHO" & WW_B3CNT.ToString("0")
                WW_TODOKECODE = "TODOKECODE" & WW_B3CNT.ToString("0")
                WW_SHARYOKBN = "SHARYOKBN" & WW_B3CNT.ToString("0")
                WW_OILPAYKBN = "OILPAYKBN" & WW_B3CNT.ToString("0")
                WW_MODELDISTANCE = "MODELDISTANCE" & WW_B3CNT.ToString("0")
                WW_MODIFYKBN = "MODIFYKBN" & WW_B3CNT.ToString("0")
                WW_oTBLrow(WW_SHUKABASHO) = WW_iTBLrow("SHUKABASHO")
                WW_oTBLrow(WW_TODOKECODE) = WW_iTBLrow("TODOKECODE")
                WW_oTBLrow(WW_SHARYOKBN) = WW_iTBLrow("SHARYOKBN")
                WW_oTBLrow(WW_OILPAYKBN) = WW_iTBLrow("OILPAYKBN")

                If WW_iTBLrow("WORKKBN") = "B3" Then
                    If WW_iTBLrow("MODELDISTANCE2") > 0 Then
                        WW_oTBLrow(WW_MODELDISTANCE) = WW_iTBLrow("MODELDISTANCE2")
                    Else
                        WW_oTBLrow(WW_MODELDISTANCE) = WW_iTBLrow("MODELDISTANCE1")
                    End If
                End If
                If WW_iTBLrow("WORKKBN") = "B2" Then
                    WW_oTBLrow(WW_MODELDISTANCE) = WW_iTBLrow("MODELDISTANCE3")
                End If
                WW_oTBLrow(WW_MODIFYKBN) = "0"
            Else
                For j As Integer = WW_B3CNT + 1 To 6
                    WW_SHUKABASHO = "SHUKABASHO" & j.ToString("0")
                    WW_TODOKECODE = "TODOKECODE" & j.ToString("0")
                    WW_SHARYOKBN = "SHARYOKBN" & j.ToString("0")
                    WW_OILPAYKBN = "OILPAYKBN" & j.ToString("0")
                    WW_MODELDISTANCE = "MODELDISTANCE" & j.ToString("0")
                    WW_MODIFYKBN = "MODIFYKBN" & j.ToString("0")
                    WW_oTBLrow(WW_SHUKABASHO) = ""
                    WW_oTBLrow(WW_TODOKECODE) = ""
                    WW_oTBLrow(WW_SHARYOKBN) = ""
                    WW_oTBLrow(WW_OILPAYKBN) = ""
                    WW_oTBLrow(WW_MODELDISTANCE) = 0
                    WW_oTBLrow(WW_MODIFYKBN) = "0"
                Next
                WW_oTBLrow("CAMPCODE") = iCAMP
                WW_oTBLrow("TAISHOYM") = iTAISHOYM
                WW_oTBLrow("SAVECNT") = WW_B3CNT
                WW_oTBLrow("DELFLG") = C_DELETE_FLG.ALIVE
                WW_oTBLrow("INITYMD") = Date.Now.ToString("yyyy/MM/dd")
                WW_oTBLrow("UPDYMD") = Date.Now.ToString("yyyy/MM/dd")
                WW_oTBLrow("UPDUSER") = UPDUSERID
                WW_oTBLrow("UPDTERMID") = UPDTERMID
                WW_oTBLrow("RECEIVEYMD") = C_DEFAULT_YMD
                oTBL.Rows.Add(WW_oTBLrow)
                WW_oTBLrow = oTBL.NewRow
                WW_B3CNT = 0

                WW_oTBLrow("WORKDATE") = WW_iTBLrow("YMD")
                WW_oTBLrow("STAFFCODE") = WW_iTBLrow("STAFFCODE")
                WW_B3CNT += 1
                WW_SHUKABASHO = "SHUKABASHO" & WW_B3CNT.ToString("0")
                WW_TODOKECODE = "TODOKECODE" & WW_B3CNT.ToString("0")
                WW_SHARYOKBN = "SHARYOKBN" & WW_B3CNT.ToString("0")
                WW_OILPAYKBN = "OILPAYKBN" & WW_B3CNT.ToString("0")
                WW_MODELDISTANCE = "MODELDISTANCE" & WW_B3CNT.ToString("0")
                WW_MODIFYKBN = "MODIFYKBN" & WW_B3CNT.ToString("0")
                WW_oTBLrow(WW_SHUKABASHO) = WW_iTBLrow("SHUKABASHO")
                WW_oTBLrow(WW_TODOKECODE) = WW_iTBLrow("TODOKECODE")
                WW_oTBLrow(WW_SHARYOKBN) = WW_iTBLrow("SHARYOKBN")
                WW_oTBLrow(WW_OILPAYKBN) = WW_iTBLrow("OILPAYKBN")
                If WW_iTBLrow("WORKKBN") = "B3" Then
                    If WW_iTBLrow("MODELDISTANCE2") > 0 Then
                        WW_oTBLrow(WW_MODELDISTANCE) = WW_iTBLrow("MODELDISTANCE2")
                    Else
                        WW_oTBLrow(WW_MODELDISTANCE) = WW_iTBLrow("MODELDISTANCE1")
                    End If
                End If
                If WW_iTBLrow("WORKKBN") = "B2" Then
                    WW_oTBLrow(WW_MODELDISTANCE) = WW_iTBLrow("MODELDISTANCE3")
                End If
                WW_oTBLrow(WW_MODIFYKBN) = "0"
            End If
            WW_WORKDATE = WW_iTBLrow("YMD")
            WW_STAFFCODE = WW_iTBLrow("STAFFCODE")
        Next
        If WW_iTBL.Rows.Count > 0 Then
            For j As Integer = WW_B3CNT + 1 To 6
                WW_SHUKABASHO = "SHUKABASHO" & j.ToString("0")
                WW_TODOKECODE = "TODOKECODE" & j.ToString("0")
                WW_SHARYOKBN = "SHARYOKBN" & j.ToString("0")
                WW_OILPAYKBN = "OILPAYKBN" & j.ToString("0")
                WW_MODELDISTANCE = "MODELDISTANCE" & j.ToString("0")
                WW_MODIFYKBN = "MODIFYKBN" & j.ToString("0")
                WW_oTBLrow(WW_SHUKABASHO) = ""
                WW_oTBLrow(WW_TODOKECODE) = ""
                WW_oTBLrow(WW_SHARYOKBN) = ""
                WW_oTBLrow(WW_OILPAYKBN) = ""
                WW_oTBLrow(WW_MODELDISTANCE) = 0
                WW_oTBLrow(WW_MODIFYKBN) = "0"
            Next
            WW_oTBLrow("CAMPCODE") = iCAMP
            WW_oTBLrow("TAISHOYM") = iTAISHOYM
            WW_oTBLrow("SAVECNT") = WW_B3CNT
            WW_oTBLrow("DELFLG") = C_DELETE_FLG.ALIVE
            WW_oTBLrow("INITYMD") = Date.Now.ToString("yyyy/MM/dd")
            WW_oTBLrow("UPDYMD") = Date.Now.ToString("yyyy/MM/dd")
            WW_oTBLrow("UPDUSER") = UPDUSERID
            WW_oTBLrow("UPDTERMID") = UPDTERMID
            WW_oTBLrow("RECEIVEYMD") = C_DEFAULT_YMD
            oTBL.Rows.Add(WW_oTBLrow)

            '特殊処理（モデル距離
            ' 下記の出荷場所→届先の繰り返し配送の場合、回転数によりモデル距離を設定する
            '　　ケミカルロジテック -ＪＳＲ四日市工場
            '　　辰巳商会 -ＪＳＲ四日市工場
            '    １回転：通常モデル距離（モデル距離マスタより）
            '    ２回転：１回目を89km、２回目を89km
            '    ３回転：１回目を89km、２回目を89km、３回目を59km
            '    ４回転：１回目を89km、２回目を89km、３回目を59km、４回目を69km

            For i As Integer = 0 To oTBL.Rows.Count - 1
                WW_oTBLrow = oTBL.Rows(i)
                If WW_oTBLrow("SAVECNT") = 2 Then
                    For j As Integer = 0 To iListBoxMODELCODE.Items.Count - 1
                        If WW_oTBLrow("SHUKABASHO1") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE1") = iListBoxMODELCODE.Items(j).Text And
                           WW_oTBLrow("SHUKABASHO2") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE2") = iListBoxMODELCODE.Items(j).Text Then
                            WW_oTBLrow("MODELDISTANCE1") = iListBoxMODELDISTANCE.Items(0).Text
                            WW_oTBLrow("MODELDISTANCE2") = iListBoxMODELDISTANCE.Items(1).Text
                        End If
                    Next
                End If
                If WW_oTBLrow("SAVECNT") = 3 Then
                    For j As Integer = 0 To iListBoxMODELCODE.Items.Count - 1
                        If WW_oTBLrow("SHUKABASHO1") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE1") = iListBoxMODELCODE.Items(j).Text And
                           WW_oTBLrow("SHUKABASHO2") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE2") = iListBoxMODELCODE.Items(j).Text And
                           WW_oTBLrow("SHUKABASHO3") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE3") = iListBoxMODELCODE.Items(j).Text Then
                            WW_oTBLrow("MODELDISTANCE1") = iListBoxMODELDISTANCE.Items(0).Text
                            WW_oTBLrow("MODELDISTANCE2") = iListBoxMODELDISTANCE.Items(1).Text
                            WW_oTBLrow("MODELDISTANCE3") = iListBoxMODELDISTANCE.Items(2).Text
                        End If
                    Next
                End If
                If WW_oTBLrow("SAVECNT") = 4 Then
                    For j As Integer = 0 To iListBoxMODELCODE.Items.Count - 1
                        If WW_oTBLrow("SHUKABASHO1") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE1") = iListBoxMODELCODE.Items(j).Text And
                           WW_oTBLrow("SHUKABASHO2") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE2") = iListBoxMODELCODE.Items(j).Text And
                           WW_oTBLrow("SHUKABASHO3") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE3") = iListBoxMODELCODE.Items(j).Text And
                           WW_oTBLrow("SHUKABASHO4") = iListBoxMODELCODE.Items(j).Value And WW_oTBLrow("TODOKECODE4") = iListBoxMODELCODE.Items(j).Text Then
                            WW_oTBLrow("MODELDISTANCE1") = iListBoxMODELDISTANCE.Items(0).Text
                            WW_oTBLrow("MODELDISTANCE2") = iListBoxMODELDISTANCE.Items(1).Text
                            WW_oTBLrow("MODELDISTANCE3") = iListBoxMODELDISTANCE.Items(2).Text
                            WW_oTBLrow("MODELDISTANCE4") = iListBoxMODELDISTANCE.Items(3).Text
                        End If
                    Next
                End If
            Next
        End If

    End Sub

    ' ***  時間変換（分→時:分）
    Function formatHHMM(ByVal iParm As Integer) As String
        Dim WW_HHMM As Integer = 0
        Dim WW_ABS As Integer = System.Math.Abs(iParm)

        WW_HHMM = Int(WW_ABS / 60) * 100 + WW_ABS Mod 60
        If iParm < 0 Then
            WW_HHMM = WW_HHMM * -1
        End If
        formatHHMM = Format(WW_HHMM, "0#:##")
    End Function

    '変換（時：分→分）
    Public Function HHMMtoMinutes(ByVal iParm As String) As Integer
        Dim WW_TIME As String() = {}
        Dim WW_SIGN As String = "+"

        If iParm = Nothing Then
            HHMMtoMinutes = 0
        Else
            If Mid(iParm, 1, 1) = "-" Then
                WW_SIGN = "-"
                WW_TIME = iParm.Replace("-", "").Split(":")
            Else
                WW_SIGN = "+"
                WW_TIME = iParm.Split(":")
            End If
            If WW_TIME.Count > 1 Then
                HHMMtoMinutes = Val(WW_TIME(0)) * 60 + Val(WW_TIME(1))
                If WW_SIGN = "-" Then
                    HHMMtoMinutes = HHMMtoMinutes * -1
                End If
            Else
                HHMMtoMinutes = 0
            End If
        End If

    End Function

    Public Function CheckHOLIDAY(ByVal iHOLIDAYKBN As String, ByVal iPAYKBN As String) As Boolean
        '休日区分
        '1:法定休日、2:法定外休日

        '勤怠区分
        '00:通常 , 01:年休 , 02:特休 , 03:遅刻早退 , 04:ｽﾄｯｸ , 05:協約週休 ,
        '06:週休 , 07:傷欠 , 08:組欠 , 09:他欠 , 11:代休 ,
        '12:年始出勤 , 13:指定休 , 14:出張 , 15:振休 , 16:休業
        If iHOLIDAYKBN = "1" OrElse
           iHOLIDAYKBN = "2" OrElse
           iPAYKBN = "01" OrElse
           iPAYKBN = "02" OrElse
           iPAYKBN = "04" OrElse
           iPAYKBN = "05" OrElse
           iPAYKBN = "06" OrElse
           iPAYKBN = "07" OrElse
           iPAYKBN = "08" OrElse
           iPAYKBN = "09" OrElse
           iPAYKBN = "11" OrElse
           iPAYKBN = "13" OrElse
           iPAYKBN = "15" Then
            CheckHOLIDAY = True
        Else
            CheckHOLIDAY = False
        End If
    End Function

    Public Sub T0005tbl_ColumnsAdd(ByRef iTbl As DataTable)

        If iTbl.Columns.Count = 0 Then
        Else
            iTbl.Columns.Clear()
        End If

        'T0005DB項目作成
        iTbl.Clear()
        iTbl.Columns.Add("LINECNT", GetType(Integer))
        iTbl.Columns.Add("OPERATION", GetType(String))
        iTbl.Columns.Add("TIMSTP", GetType(String))
        iTbl.Columns.Add("SELECT", GetType(Integer))
        iTbl.Columns.Add("HIDDEN", GetType(Integer))

        iTbl.Columns.Add("CAMPCODE", GetType(String))
        iTbl.Columns.Add("CAMPNAMES", GetType(String))
        iTbl.Columns.Add("SHIPORG", GetType(String))
        iTbl.Columns.Add("SHIPORGNAMES", GetType(String))
        iTbl.Columns.Add("TERMKBN", GetType(String))
        iTbl.Columns.Add("TERMKBNNAMES", GetType(String))
        iTbl.Columns.Add("YMD", GetType(String))
        iTbl.Columns.Add("NIPPONO", GetType(String))
        iTbl.Columns.Add("HDKBN", GetType(String))
        iTbl.Columns.Add("WORKKBN", GetType(String))
        iTbl.Columns.Add("WORKKBNNAMES", GetType(String))
        iTbl.Columns.Add("SEQ", GetType(String))
        iTbl.Columns.Add("STAFFCODE", GetType(String))
        iTbl.Columns.Add("ENTRYDATE", GetType(String))
        iTbl.Columns.Add("STAFFNAMES", GetType(String))
        iTbl.Columns.Add("SUBSTAFFCODE", GetType(String))
        iTbl.Columns.Add("SUBSTAFFNAMES", GetType(String))
        iTbl.Columns.Add("CREWKBN", GetType(String))
        iTbl.Columns.Add("CREWKBNNAMES", GetType(String))
        iTbl.Columns.Add("GSHABAN", GetType(String))
        iTbl.Columns.Add("GSHABANLICNPLTNO", GetType(String))
        iTbl.Columns.Add("STDATE", GetType(String))
        iTbl.Columns.Add("STTIME", GetType(String))
        iTbl.Columns.Add("ENDDATE", GetType(String))
        iTbl.Columns.Add("ENDTIME", GetType(String))
        iTbl.Columns.Add("WORKTIME", GetType(String))
        iTbl.Columns.Add("MOVETIME", GetType(String))
        iTbl.Columns.Add("ACTTIME", GetType(String))
        iTbl.Columns.Add("PRATE", GetType(String))
        iTbl.Columns.Add("CASH", GetType(String))
        iTbl.Columns.Add("TICKET", GetType(String))
        iTbl.Columns.Add("ETC", GetType(String))
        iTbl.Columns.Add("TOTALTOLL", GetType(String))
        iTbl.Columns.Add("STMATER", GetType(String))
        iTbl.Columns.Add("ENDMATER", GetType(String))
        iTbl.Columns.Add("RUIDISTANCE", GetType(String))
        iTbl.Columns.Add("SOUDISTANCE", GetType(String))
        iTbl.Columns.Add("JIDISTANCE", GetType(String))
        iTbl.Columns.Add("KUDISTANCE", GetType(String))
        iTbl.Columns.Add("IPPDISTANCE", GetType(String))
        iTbl.Columns.Add("KOSDISTANCE", GetType(String))
        iTbl.Columns.Add("IPPJIDISTANCE", GetType(String))
        iTbl.Columns.Add("IPPKUDISTANCE", GetType(String))
        iTbl.Columns.Add("KOSJIDISTANCE", GetType(String))
        iTbl.Columns.Add("KOSKUDISTANCE", GetType(String))
        iTbl.Columns.Add("KYUYU", GetType(String))
        iTbl.Columns.Add("TORICODE", GetType(String))
        iTbl.Columns.Add("TORINAMES", GetType(String))
        iTbl.Columns.Add("SHUKABASHO", GetType(String))
        iTbl.Columns.Add("SHUKABASHONAMES", GetType(String))
        iTbl.Columns.Add("TODOKECODE", GetType(String))
        iTbl.Columns.Add("TODOKENAMES", GetType(String))
        iTbl.Columns.Add("TODOKEDATE", GetType(String))
        iTbl.Columns.Add("OILTYPE1", GetType(String))
        iTbl.Columns.Add("PRODUCT11", GetType(String))
        iTbl.Columns.Add("PRODUCT21", GetType(String))
        iTbl.Columns.Add("PRODUCT1NAMES", GetType(String))
        iTbl.Columns.Add("SURYO1", GetType(String))
        iTbl.Columns.Add("STANI1", GetType(String))
        iTbl.Columns.Add("STANI1NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE2", GetType(String))
        iTbl.Columns.Add("PRODUCT12", GetType(String))
        iTbl.Columns.Add("PRODUCT22", GetType(String))
        iTbl.Columns.Add("PRODUCT2NAMES", GetType(String))
        iTbl.Columns.Add("SURYO2", GetType(String))
        iTbl.Columns.Add("STANI2", GetType(String))
        iTbl.Columns.Add("STANI2NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE3", GetType(String))
        iTbl.Columns.Add("PRODUCT13", GetType(String))
        iTbl.Columns.Add("PRODUCT23", GetType(String))
        iTbl.Columns.Add("PRODUCT3NAMES", GetType(String))
        iTbl.Columns.Add("SURYO3", GetType(String))
        iTbl.Columns.Add("STANI3", GetType(String))
        iTbl.Columns.Add("STANI3NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE4", GetType(String))
        iTbl.Columns.Add("PRODUCT14", GetType(String))
        iTbl.Columns.Add("PRODUCT24", GetType(String))
        iTbl.Columns.Add("PRODUCT4NAMES", GetType(String))
        iTbl.Columns.Add("SURYO4", GetType(String))
        iTbl.Columns.Add("STANI4", GetType(String))
        iTbl.Columns.Add("STANI4NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE5", GetType(String))
        iTbl.Columns.Add("PRODUCT15", GetType(String))
        iTbl.Columns.Add("PRODUCT25", GetType(String))
        iTbl.Columns.Add("PRODUCT5NAMES", GetType(String))
        iTbl.Columns.Add("SURYO5", GetType(String))
        iTbl.Columns.Add("STANI5", GetType(String))
        iTbl.Columns.Add("STANI5NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE6", GetType(String))
        iTbl.Columns.Add("PRODUCT16", GetType(String))
        iTbl.Columns.Add("PRODUCT26", GetType(String))
        iTbl.Columns.Add("PRODUCT6NAMES", GetType(String))
        iTbl.Columns.Add("SURYO6", GetType(String))
        iTbl.Columns.Add("STANI6", GetType(String))
        iTbl.Columns.Add("STANI6NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE7", GetType(String))
        iTbl.Columns.Add("PRODUCT17", GetType(String))
        iTbl.Columns.Add("PRODUCT27", GetType(String))
        iTbl.Columns.Add("PRODUCT7NAMES", GetType(String))
        iTbl.Columns.Add("SURYO7", GetType(String))
        iTbl.Columns.Add("STANI7", GetType(String))
        iTbl.Columns.Add("STANI7NAMES", GetType(String))
        iTbl.Columns.Add("OILTYPE8", GetType(String))
        iTbl.Columns.Add("PRODUCT18", GetType(String))
        iTbl.Columns.Add("PRODUCT28", GetType(String))
        iTbl.Columns.Add("PRODUCT8NAMES", GetType(String))
        iTbl.Columns.Add("SURYO8", GetType(String))
        iTbl.Columns.Add("STANI8", GetType(String))
        iTbl.Columns.Add("STANI8NAMES", GetType(String))
        iTbl.Columns.Add("TOTALSURYO", GetType(String))
        iTbl.Columns.Add("TUMIOKIKBN", GetType(String))
        iTbl.Columns.Add("TUMIOKIKBNNAMES", GetType(String))
        iTbl.Columns.Add("ORDERNO", GetType(String))
        iTbl.Columns.Add("DETAILNO", GetType(String))
        iTbl.Columns.Add("TRIPNO", GetType(String))
        iTbl.Columns.Add("DROPNO", GetType(String))
        iTbl.Columns.Add("JISSKIKBN", GetType(String))
        iTbl.Columns.Add("JISSKIKBNNAMES", GetType(String))
        iTbl.Columns.Add("URIKBN", GetType(String))
        iTbl.Columns.Add("URIKBNNAMES", GetType(String))

        'iTbl.Columns.Add("STORICODE", GetType(String))
        'iTbl.Columns.Add("STORICODENAMES", GetType(String))
        'iTbl.Columns.Add("CONTCHASSIS", GetType(String))
        'iTbl.Columns.Add("CONTCHASSISLICNPLTNO", GetType(String))

        iTbl.Columns.Add("SHARYOTYPEF", GetType(String))
        iTbl.Columns.Add("TSHABANF", GetType(String))
        iTbl.Columns.Add("SHARYOTYPEB", GetType(String))
        iTbl.Columns.Add("TSHABANB", GetType(String))
        iTbl.Columns.Add("SHARYOTYPEB2", GetType(String))
        iTbl.Columns.Add("TSHABANB2", GetType(String))
        iTbl.Columns.Add("TAXKBN", GetType(String))
        iTbl.Columns.Add("TAXKBNNAMES", GetType(String))
        iTbl.Columns.Add("LATITUDE", GetType(String))
        iTbl.Columns.Add("LONGITUDE", GetType(String))
        iTbl.Columns.Add("DELFLG", GetType(String))

        iTbl.Columns.Add("SHARYOKBN", GetType(String))
        iTbl.Columns.Add("SHARYOKBNNAMES", GetType(String))
        iTbl.Columns.Add("OILPAYKBN", GetType(String))
        iTbl.Columns.Add("OILPAYKBNNAMES", GetType(String))
        iTbl.Columns.Add("SUISOKBN", GetType(String))
        iTbl.Columns.Add("SUISOKBNNAMES", GetType(String))
        iTbl.Columns.Add("L1KAISO", GetType(String))

        iTbl.Columns.Add("WORKINGWEEK", GetType(String))
        iTbl.Columns.Add("WORKINGWEEKNAMES", GetType(String))
        iTbl.Columns.Add("HOLIDAYKBN", GetType(String))
        iTbl.Columns.Add("HOLIDAYKBNNAMES", GetType(String))
        iTbl.Columns.Add("MORG", GetType(String))
        iTbl.Columns.Add("MORGNAMES", GetType(String))
        iTbl.Columns.Add("HORG", GetType(String))
        iTbl.Columns.Add("HORGNAMES", GetType(String))
        iTbl.Columns.Add("SORG", GetType(String))
        iTbl.Columns.Add("SORGNAMES", GetType(String))
        iTbl.Columns.Add("STAFFKBN", GetType(String))
        iTbl.Columns.Add("STAFFKBNNAMES", GetType(String))

        iTbl.Columns.Add("UPDYMD", GetType(String))

        iTbl.Columns.Add("MODELDISTANCE1", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE2", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE3", GetType(String))
        iTbl.Columns.Add("wHaisoGroup", GetType(String))
        iTbl.Columns.Add("UNLOADADDTANKA", GetType(String))
        iTbl.Columns.Add("LOADINGTANKA", GetType(String))

        For Each col As DataColumn In iTbl.Columns
            If col.DataType = GetType(String) AndAlso
                col.DefaultValue Is DBNull.Value Then

                col.DefaultValue = ""
            End If
        Next
    End Sub

    Public Sub T0007tbl_ColumnsAdd(ByRef iTbl As DataTable)

        If iTbl.Columns.Count = 0 Then
        Else
            iTbl.Columns.Clear()
        End If

        'T0007DB項目作成
        iTbl.Clear()
        iTbl.Columns.Add("LINECNT", GetType(Integer))
        iTbl.Columns.Add("OPERATION", GetType(String))
        iTbl.Columns.Add("TIMSTP", GetType(String))
        iTbl.Columns.Add("SELECT", GetType(Integer))
        iTbl.Columns.Add("HIDDEN", GetType(Integer))
        iTbl.Columns.Add("EXTRACTCNT", GetType(String))

        iTbl.Columns.Add("STATUS", GetType(String))
        iTbl.Columns.Add("CAMPCODE", GetType(String))
        iTbl.Columns.Add("CAMPNAMES", GetType(String))
        iTbl.Columns.Add("TAISHOYM", GetType(String))
        iTbl.Columns.Add("STAFFCODE", GetType(String))
        iTbl.Columns.Add("STAFFNAMES", GetType(String))
        iTbl.Columns.Add("WORKDATE", GetType(String))
        iTbl.Columns.Add("WORKINGWEEK", GetType(String))
        iTbl.Columns.Add("WORKINGWEEKNAMES", GetType(String))
        iTbl.Columns.Add("HDKBN", GetType(String))
        iTbl.Columns.Add("RECODEKBN", GetType(String))
        iTbl.Columns.Add("RECODEKBNNAMES", GetType(String))
        iTbl.Columns.Add("SEQ", GetType(String))
        iTbl.Columns.Add("ENTRYDATE", GetType(String))
        iTbl.Columns.Add("NIPPOLINKCODE", GetType(String))
        iTbl.Columns.Add("MORG", GetType(String))
        iTbl.Columns.Add("MORGNAMES", GetType(String))
        iTbl.Columns.Add("HORG", GetType(String))
        iTbl.Columns.Add("HORGNAMES", GetType(String))
        iTbl.Columns.Add("SORG", GetType(String))
        iTbl.Columns.Add("SORGNAMES", GetType(String))
        iTbl.Columns.Add("STAFFKBN", GetType(String))
        iTbl.Columns.Add("STAFFKBNNAMES", GetType(String))
        iTbl.Columns.Add("HOLIDAYKBN", GetType(String))
        iTbl.Columns.Add("HOLIDAYKBNNAMES", GetType(String))
        iTbl.Columns.Add("PAYKBN", GetType(String))
        iTbl.Columns.Add("PAYKBNNAMES", GetType(String))
        iTbl.Columns.Add("SHUKCHOKKBN", GetType(String))
        iTbl.Columns.Add("SHUKCHOKKBNNAMES", GetType(String))
        iTbl.Columns.Add("WORKKBN", GetType(String))
        iTbl.Columns.Add("WORKKBNNAMES", GetType(String))
        iTbl.Columns.Add("STDATE", GetType(String))
        iTbl.Columns.Add("STTIME", GetType(String))
        iTbl.Columns.Add("ENDDATE", GetType(String))
        iTbl.Columns.Add("ENDTIME", GetType(String))
        iTbl.Columns.Add("WORKTIME", GetType(String))
        iTbl.Columns.Add("MOVETIME", GetType(String))
        iTbl.Columns.Add("ACTTIME", GetType(String))
        iTbl.Columns.Add("BINDSTDATE", GetType(String))
        iTbl.Columns.Add("BINDTIMEMIN", GetType(String))
        iTbl.Columns.Add("BINDTIME", GetType(String))
        iTbl.Columns.Add("NIPPOBREAKTIME", GetType(String))
        iTbl.Columns.Add("BREAKTIME", GetType(String))
        iTbl.Columns.Add("BREAKTIMECHO", GetType(String))
        iTbl.Columns.Add("BREAKTIMETTL", GetType(String))
        iTbl.Columns.Add("NIGHTTIME", GetType(String))
        iTbl.Columns.Add("NIGHTTIMECHO", GetType(String))
        iTbl.Columns.Add("NIGHTTIMETTL", GetType(String))
        iTbl.Columns.Add("ORVERTIME", GetType(String))
        iTbl.Columns.Add("ORVERTIMECHO", GetType(String))
        iTbl.Columns.Add("ORVERTIMETTL", GetType(String))
        iTbl.Columns.Add("WNIGHTTIME", GetType(String))
        iTbl.Columns.Add("WNIGHTTIMECHO", GetType(String))
        iTbl.Columns.Add("WNIGHTTIMETTL", GetType(String))
        iTbl.Columns.Add("SWORKTIME", GetType(String))
        iTbl.Columns.Add("SWORKTIMECHO", GetType(String))
        iTbl.Columns.Add("SWORKTIMETTL", GetType(String))
        iTbl.Columns.Add("SNIGHTTIME", GetType(String))
        iTbl.Columns.Add("SNIGHTTIMECHO", GetType(String))
        iTbl.Columns.Add("SNIGHTTIMETTL", GetType(String))
        iTbl.Columns.Add("HWORKTIME", GetType(String))
        iTbl.Columns.Add("HWORKTIMECHO", GetType(String))
        iTbl.Columns.Add("HWORKTIMETTL", GetType(String))
        iTbl.Columns.Add("HNIGHTTIME", GetType(String))
        iTbl.Columns.Add("HNIGHTTIMECHO", GetType(String))
        iTbl.Columns.Add("HNIGHTTIMETTL", GetType(String))
        iTbl.Columns.Add("WORKNISSU", GetType(String))
        iTbl.Columns.Add("WORKNISSUCHO", GetType(String))
        iTbl.Columns.Add("WORKNISSUTTL", GetType(String))
        iTbl.Columns.Add("SHOUKETUNISSU", GetType(String))
        iTbl.Columns.Add("SHOUKETUNISSUCHO", GetType(String))
        iTbl.Columns.Add("SHOUKETUNISSUTTL", GetType(String))
        iTbl.Columns.Add("KUMIKETUNISSU", GetType(String))
        iTbl.Columns.Add("KUMIKETUNISSUCHO", GetType(String))
        iTbl.Columns.Add("KUMIKETUNISSUTTL", GetType(String))
        iTbl.Columns.Add("ETCKETUNISSU", GetType(String))
        iTbl.Columns.Add("ETCKETUNISSUCHO", GetType(String))
        iTbl.Columns.Add("ETCKETUNISSUTTL", GetType(String))
        iTbl.Columns.Add("NENKYUNISSU", GetType(String))
        iTbl.Columns.Add("NENKYUNISSUCHO", GetType(String))
        iTbl.Columns.Add("NENKYUNISSUTTL", GetType(String))
        iTbl.Columns.Add("TOKUKYUNISSU", GetType(String))
        iTbl.Columns.Add("TOKUKYUNISSUCHO", GetType(String))
        iTbl.Columns.Add("TOKUKYUNISSUTTL", GetType(String))
        iTbl.Columns.Add("CHIKOKSOTAINISSU", GetType(String))
        iTbl.Columns.Add("CHIKOKSOTAINISSUCHO", GetType(String))
        iTbl.Columns.Add("CHIKOKSOTAINISSUTTL", GetType(String))
        iTbl.Columns.Add("STOCKNISSU", GetType(String))
        iTbl.Columns.Add("STOCKNISSUCHO", GetType(String))
        iTbl.Columns.Add("STOCKNISSUTTL", GetType(String))
        iTbl.Columns.Add("KYOTEIWEEKNISSU", GetType(String))
        iTbl.Columns.Add("KYOTEIWEEKNISSUCHO", GetType(String))
        iTbl.Columns.Add("KYOTEIWEEKNISSUTTL", GetType(String))
        iTbl.Columns.Add("WEEKNISSU", GetType(String))
        iTbl.Columns.Add("WEEKNISSUCHO", GetType(String))
        iTbl.Columns.Add("WEEKNISSUTTL", GetType(String))
        iTbl.Columns.Add("DAIKYUNISSU", GetType(String))
        iTbl.Columns.Add("DAIKYUNISSUCHO", GetType(String))
        iTbl.Columns.Add("DAIKYUNISSUTTL", GetType(String))
        iTbl.Columns.Add("NENSHINISSU", GetType(String))
        iTbl.Columns.Add("NENSHINISSUCHO", GetType(String))
        iTbl.Columns.Add("NENSHINISSUTTL", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNNISSU", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNNISSUCHO", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNNISSUTTL", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNISSU", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNISSUCHO", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNISSUTTL", GetType(String))

        iTbl.Columns.Add("SHUKCHOKNHLDNISSU", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNHLDNISSUCHO", GetType(String))
        iTbl.Columns.Add("SHUKCHOKNHLDNISSUTTL", GetType(String))
        iTbl.Columns.Add("SHUKCHOKHLDNISSU", GetType(String))
        iTbl.Columns.Add("SHUKCHOKHLDNISSUCHO", GetType(String))
        iTbl.Columns.Add("SHUKCHOKHLDNISSUTTL", GetType(String))

        iTbl.Columns.Add("TOKSAAKAISU", GetType(String))
        iTbl.Columns.Add("TOKSAAKAISUCHO", GetType(String))
        iTbl.Columns.Add("TOKSAAKAISUTTL", GetType(String))
        iTbl.Columns.Add("TOKSABKAISU", GetType(String))
        iTbl.Columns.Add("TOKSABKAISUCHO", GetType(String))
        iTbl.Columns.Add("TOKSABKAISUTTL", GetType(String))
        iTbl.Columns.Add("TOKSACKAISU", GetType(String))
        iTbl.Columns.Add("TOKSACKAISUCHO", GetType(String))
        iTbl.Columns.Add("TOKSACKAISUTTL", GetType(String))
        iTbl.Columns.Add("TENKOKAISU", GetType(String))
        iTbl.Columns.Add("TENKOKAISUCHO", GetType(String))
        iTbl.Columns.Add("TENKOKAISUTTL", GetType(String))
        iTbl.Columns.Add("HOANTIME", GetType(String))
        iTbl.Columns.Add("HOANTIMECHO", GetType(String))
        iTbl.Columns.Add("HOANTIMETTL", GetType(String))
        iTbl.Columns.Add("KOATUTIME", GetType(String))
        iTbl.Columns.Add("KOATUTIMECHO", GetType(String))
        iTbl.Columns.Add("KOATUTIMETTL", GetType(String))
        iTbl.Columns.Add("TOKUSA1TIME", GetType(String))
        iTbl.Columns.Add("TOKUSA1TIMECHO", GetType(String))
        iTbl.Columns.Add("TOKUSA1TIMETTL", GetType(String))
        iTbl.Columns.Add("HAYADETIME", GetType(String))
        iTbl.Columns.Add("HAYADETIMECHO", GetType(String))
        iTbl.Columns.Add("HAYADETIMETTL", GetType(String))
        iTbl.Columns.Add("PONPNISSU", GetType(String))
        iTbl.Columns.Add("PONPNISSUCHO", GetType(String))
        iTbl.Columns.Add("PONPNISSUTTL", GetType(String))
        iTbl.Columns.Add("BULKNISSU", GetType(String))
        iTbl.Columns.Add("BULKNISSUCHO", GetType(String))
        iTbl.Columns.Add("BULKNISSUTTL", GetType(String))
        iTbl.Columns.Add("TRAILERNISSU", GetType(String))
        iTbl.Columns.Add("TRAILERNISSUCHO", GetType(String))
        iTbl.Columns.Add("TRAILERNISSUTTL", GetType(String))
        iTbl.Columns.Add("BKINMUKAISU", GetType(String))
        iTbl.Columns.Add("BKINMUKAISUCHO", GetType(String))
        iTbl.Columns.Add("BKINMUKAISUTTL", GetType(String))
        iTbl.Columns.Add("SHARYOKBN", GetType(String))
        iTbl.Columns.Add("SHARYOKBNNAMES", GetType(String))
        iTbl.Columns.Add("OILPAYKBN", GetType(String))
        iTbl.Columns.Add("OILPAYKBNNAMES", GetType(String))
        iTbl.Columns.Add("SHARYOKBN2", GetType(String))
        iTbl.Columns.Add("SHARYOKBNNAMES2", GetType(String))
        iTbl.Columns.Add("OILPAYKBN2", GetType(String))
        iTbl.Columns.Add("OILPAYKBNNAMES2", GetType(String))
        iTbl.Columns.Add("UNLOADCNT", GetType(String))
        iTbl.Columns.Add("UNLOADCNTCHO", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL", GetType(String))
        iTbl.Columns.Add("HAIDISTANCE", GetType(String))
        iTbl.Columns.Add("HAIDISTANCECHO", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL", GetType(String))
        iTbl.Columns.Add("KAIDISTANCE", GetType(String))
        iTbl.Columns.Add("KAIDISTANCECHO", GetType(String))
        iTbl.Columns.Add("KAIDISTANCETTL", GetType(String))
        iTbl.Columns.Add("DELFLG", GetType(String))

        iTbl.Columns.Add("DATAKBN", GetType(String))
        iTbl.Columns.Add("SHIPORG", GetType(String))
        iTbl.Columns.Add("SHIPORGNAMES", GetType(String))
        iTbl.Columns.Add("NIPPONO", GetType(String))
        iTbl.Columns.Add("GSHABAN", GetType(String))
        iTbl.Columns.Add("RUIDISTANCE", GetType(String))
        iTbl.Columns.Add("JIDISTANCE", GetType(String))
        iTbl.Columns.Add("KUDISTANCE", GetType(String))

        iTbl.Columns.Add("T5ENTRYDATE", GetType(String))
        iTbl.Columns.Add("L1KAISO", GetType(String))

        iTbl.Columns.Add("LATITUDE", GetType(String))
        iTbl.Columns.Add("LONGITUDE", GetType(String))

        iTbl.Columns.Add("ORGSEQ", GetType(Integer))

        '〇Excel月合計追加項目
        'T0007DBExcel追加項目作成
        iTbl.Columns.Add("UNLOADCNTTTL0101", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0102", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0103", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0104", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0105", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0106", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0107", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0108", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0109", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0110", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0201", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0202", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0203", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0204", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0205", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0206", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0207", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0208", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0209", GetType(String))
        iTbl.Columns.Add("UNLOADCNTTTL0210", GetType(String))

        iTbl.Columns.Add("HAIDISTANCETTL0101", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0102", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0103", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0104", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0105", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0106", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0107", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0108", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0109", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0110", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0201", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0202", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0203", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0204", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0205", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0206", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0207", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0208", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0209", GetType(String))
        iTbl.Columns.Add("HAIDISTANCETTL0210", GetType(String))

        iTbl.Columns.Add("DBUMUFLG", GetType(String))

        'NJS専用
        iTbl.Columns.Add("SHACHUHAKKBN", GetType(String))
        iTbl.Columns.Add("SHACHUHAKKBNNAMES", GetType(String))
        iTbl.Columns.Add("HAISOTIME", GetType(String))
        iTbl.Columns.Add("NENMATUNISSU", GetType(String))
        iTbl.Columns.Add("NENMATUNISSUCHO", GetType(String))
        iTbl.Columns.Add("NENMATUNISSUTTL", GetType(String))
        iTbl.Columns.Add("SHACHUHAKNISSU", GetType(String))
        iTbl.Columns.Add("SHACHUHAKNISSUCHO", GetType(String))
        iTbl.Columns.Add("SHACHUHAKNISSUTTL", GetType(String))
        iTbl.Columns.Add("JIKYUSHATIME", GetType(String))
        iTbl.Columns.Add("JIKYUSHATIMECHO", GetType(String))
        iTbl.Columns.Add("JIKYUSHATIMETTL", GetType(String))
        iTbl.Columns.Add("wHaisoGroup", GetType(String))

        iTbl.Columns.Add("MODELDISTANCE", GetType(String))
        iTbl.Columns.Add("MODELDISTANCECHO", GetType(String))
        iTbl.Columns.Add("MODELDISTANCETTL", GetType(String))
        iTbl.Columns.Add("T10SAVECNT", GetType(String))
        iTbl.Columns.Add("T10SHARYOKBN1", GetType(String))
        iTbl.Columns.Add("T10OILPAYKBN1", GetType(String))
        iTbl.Columns.Add("T10SHUKABASHO1", GetType(String))
        iTbl.Columns.Add("T10TODOKECODE1", GetType(String))
        iTbl.Columns.Add("T10MODELDISTANCE1", GetType(String))
        iTbl.Columns.Add("T10MODIFYKBN1", GetType(String))
        iTbl.Columns.Add("T10SHARYOKBN2", GetType(String))
        iTbl.Columns.Add("T10OILPAYKBN2", GetType(String))
        iTbl.Columns.Add("T10SHUKABASHO2", GetType(String))
        iTbl.Columns.Add("T10TODOKECODE2", GetType(String))
        iTbl.Columns.Add("T10MODELDISTANCE2", GetType(String))
        iTbl.Columns.Add("T10MODIFYKBN2", GetType(String))
        iTbl.Columns.Add("T10SHARYOKBN3", GetType(String))
        iTbl.Columns.Add("T10OILPAYKBN3", GetType(String))
        iTbl.Columns.Add("T10SHUKABASHO3", GetType(String))
        iTbl.Columns.Add("T10TODOKECODE3", GetType(String))
        iTbl.Columns.Add("T10MODELDISTANCE3", GetType(String))
        iTbl.Columns.Add("T10MODIFYKBN3", GetType(String))
        iTbl.Columns.Add("T10SHARYOKBN4", GetType(String))
        iTbl.Columns.Add("T10OILPAYKBN4", GetType(String))
        iTbl.Columns.Add("T10SHUKABASHO4", GetType(String))
        iTbl.Columns.Add("T10TODOKECODE4", GetType(String))
        iTbl.Columns.Add("T10MODELDISTANCE4", GetType(String))
        iTbl.Columns.Add("T10MODIFYKBN4", GetType(String))
        iTbl.Columns.Add("T10SHARYOKBN5", GetType(String))
        iTbl.Columns.Add("T10OILPAYKBN5", GetType(String))
        iTbl.Columns.Add("T10SHUKABASHO5", GetType(String))
        iTbl.Columns.Add("T10TODOKECODE5", GetType(String))
        iTbl.Columns.Add("T10MODELDISTANCE5", GetType(String))
        iTbl.Columns.Add("T10MODIFYKBN5", GetType(String))
        iTbl.Columns.Add("T10SHARYOKBN6", GetType(String))
        iTbl.Columns.Add("T10OILPAYKBN6", GetType(String))
        iTbl.Columns.Add("T10SHUKABASHO6", GetType(String))
        iTbl.Columns.Add("T10TODOKECODE6", GetType(String))
        iTbl.Columns.Add("T10MODELDISTANCE6", GetType(String))
        iTbl.Columns.Add("T10MODIFYKBN6", GetType(String))

        '近石専用
        iTbl.Columns.Add("HDAIWORKTIME", GetType(String))
        iTbl.Columns.Add("HDAIWORKTIMECHO", GetType(String))
        iTbl.Columns.Add("HDAIWORKTIMETTL", GetType(String))
        iTbl.Columns.Add("HDAINIGHTTIME", GetType(String))
        iTbl.Columns.Add("HDAINIGHTTIMECHO", GetType(String))
        iTbl.Columns.Add("HDAINIGHTTIMETTL", GetType(String))
        iTbl.Columns.Add("SDAIWORKTIME", GetType(String))
        iTbl.Columns.Add("SDAIWORKTIMECHO", GetType(String))
        iTbl.Columns.Add("SDAIWORKTIMETTL", GetType(String))
        iTbl.Columns.Add("SDAINIGHTTIME", GetType(String))
        iTbl.Columns.Add("SDAINIGHTTIMECHO", GetType(String))
        iTbl.Columns.Add("SDAINIGHTTIMETTL", GetType(String))
        iTbl.Columns.Add("WWORKTIME", GetType(String))
        iTbl.Columns.Add("WWORKTIMECHO", GetType(String))
        iTbl.Columns.Add("WWORKTIMETTL", GetType(String))
        iTbl.Columns.Add("JYOMUTIME", GetType(String))
        iTbl.Columns.Add("JYOMUTIMECHO", GetType(String))
        iTbl.Columns.Add("JYOMUTIMETTL", GetType(String))
        iTbl.Columns.Add("HWORKNISSU", GetType(String))
        iTbl.Columns.Add("HWORKNISSUCHO", GetType(String))
        iTbl.Columns.Add("HWORKNISSUTTL", GetType(String))
        iTbl.Columns.Add("KAITENCNT", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL", GetType(String))
        iTbl.Columns.Add("TRIPNO", GetType(String))
        iTbl.Columns.Add("KAITENCNT1_1", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO1_1", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL1_1", GetType(String))
        iTbl.Columns.Add("KAITENCNT1_2", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO1_2", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL1_2", GetType(String))
        iTbl.Columns.Add("KAITENCNT1_3", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO1_3", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL1_3", GetType(String))
        iTbl.Columns.Add("KAITENCNT1_4", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO1_4", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL1_4", GetType(String))
        iTbl.Columns.Add("KAITENCNT2_1", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO2_1", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL2_1", GetType(String))
        iTbl.Columns.Add("KAITENCNT2_2", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO2_2", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL2_2", GetType(String))
        iTbl.Columns.Add("KAITENCNT2_3", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO2_3", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL2_3", GetType(String))
        iTbl.Columns.Add("KAITENCNT2_4", GetType(String))
        iTbl.Columns.Add("KAITENCNTCHO2_4", GetType(String))
        iTbl.Columns.Add("KAITENCNTTTL2_4", GetType(String))

        'ＪＫＴ専用
        iTbl.Columns.Add("SENJYOCNT", GetType(String))
        iTbl.Columns.Add("SENJYOCNTCHO", GetType(String))
        iTbl.Columns.Add("SENJYOCNTTTL", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT1", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT1CHO", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT1TTL", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT2", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT2CHO", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT2TTL", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT3", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT3CHO", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT3TTL", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT4", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT4CHO", GetType(String))
        iTbl.Columns.Add("UNLOADADDCNT4TTL", GetType(String))
        iTbl.Columns.Add("LOADINGCNT1", GetType(String))
        iTbl.Columns.Add("LOADINGCNT1CHO", GetType(String))
        iTbl.Columns.Add("LOADINGCNT1TTL", GetType(String))
        iTbl.Columns.Add("LOADINGCNT2", GetType(String))
        iTbl.Columns.Add("LOADINGCNT2CHO", GetType(String))
        iTbl.Columns.Add("LOADINGCNT2TTL", GetType(String))
        iTbl.Columns.Add("SHORTDISTANCE1", GetType(String))
        iTbl.Columns.Add("SHORTDISTANCE1CHO", GetType(String))
        iTbl.Columns.Add("SHORTDISTANCE1TTL", GetType(String))
        iTbl.Columns.Add("SHORTDISTANCE2", GetType(String))
        iTbl.Columns.Add("SHORTDISTANCE2CHO", GetType(String))
        iTbl.Columns.Add("SHORTDISTANCE2TTL", GetType(String))

        For Each col As DataColumn In iTbl.Columns
            If col.DataType = GetType(String) AndAlso
                col.DefaultValue Is DBNull.Value Then

                col.DefaultValue = ""

            ElseIf col.DataType = GetType(Integer) Then

                col.DefaultValue = 0

            End If
        Next

    End Sub

    Public Sub T0010tbl_ColumnsAdd(ByRef iTbl As DataTable)

        If iTbl.Columns.Count = 0 Then
        Else
            iTbl.Columns.Clear()
        End If

        'モデル距離項目作成
        iTbl.Clear()
        iTbl.Columns.Add("CAMPCODE", GetType(String))
        iTbl.Columns.Add("TAISHOYM", GetType(String))
        iTbl.Columns.Add("STAFFCODE", GetType(String))
        iTbl.Columns.Add("WORKDATE", GetType(String))
        iTbl.Columns.Add("SAVECNT", GetType(Integer))
        iTbl.Columns.Add("SHARYOKBN1", GetType(String))
        iTbl.Columns.Add("OILPAYKBN1", GetType(String))
        iTbl.Columns.Add("SHUKABASHO1", GetType(String))
        iTbl.Columns.Add("TODOKECODE1", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE1", GetType(Integer))
        iTbl.Columns.Add("MODIFYKBN1", GetType(String))
        iTbl.Columns.Add("SHARYOKBN2", GetType(String))
        iTbl.Columns.Add("OILPAYKBN2", GetType(String))
        iTbl.Columns.Add("SHUKABASHO2", GetType(String))
        iTbl.Columns.Add("TODOKECODE2", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE2", GetType(Integer))
        iTbl.Columns.Add("MODIFYKBN2", GetType(String))
        iTbl.Columns.Add("SHARYOKBN3", GetType(String))
        iTbl.Columns.Add("OILPAYKBN3", GetType(String))
        iTbl.Columns.Add("SHUKABASHO3", GetType(String))
        iTbl.Columns.Add("TODOKECODE3", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE3", GetType(Integer))
        iTbl.Columns.Add("MODIFYKBN3", GetType(String))
        iTbl.Columns.Add("SHARYOKBN4", GetType(String))
        iTbl.Columns.Add("OILPAYKBN4", GetType(String))
        iTbl.Columns.Add("SHUKABASHO4", GetType(String))
        iTbl.Columns.Add("TODOKECODE4", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE4", GetType(Integer))
        iTbl.Columns.Add("MODIFYKBN4", GetType(String))
        iTbl.Columns.Add("SHARYOKBN5", GetType(String))
        iTbl.Columns.Add("OILPAYKBN5", GetType(String))
        iTbl.Columns.Add("SHUKABASHO5", GetType(String))
        iTbl.Columns.Add("TODOKECODE5", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE5", GetType(Integer))
        iTbl.Columns.Add("MODIFYKBN5", GetType(String))
        iTbl.Columns.Add("SHARYOKBN6", GetType(String))
        iTbl.Columns.Add("OILPAYKBN6", GetType(String))
        iTbl.Columns.Add("SHUKABASHO6", GetType(String))
        iTbl.Columns.Add("TODOKECODE6", GetType(String))
        iTbl.Columns.Add("MODELDISTANCE6", GetType(Integer))
        iTbl.Columns.Add("MODIFYKBN6", GetType(String))
        iTbl.Columns.Add("DELFLG", GetType(String))
        iTbl.Columns.Add("INITYMD", GetType(String))
        iTbl.Columns.Add("UPDYMD", GetType(String))
        iTbl.Columns.Add("UPDUSER", GetType(String))
        iTbl.Columns.Add("UPDTERMID", GetType(String))
        iTbl.Columns.Add("RECEIVEYMD", GetType(String))

        For Each col As DataColumn In iTbl.Columns
            If col.DataType = GetType(String) AndAlso
                col.DefaultValue Is DBNull.Value Then

                col.DefaultValue = ""
            End If
        Next
    End Sub

    ' ***  L0001tbl編集（その他作業・乗務員）
    Public Sub L0001tblEtcEdit(ByVal I_USERID As String, ByRef I_T7tbl As DataTable, ByRef IO_L1tbl As DataTable, ByRef O_RTN As String)

        Dim WW_DATENOW As Date = Date.Now
        Dim WW_M0008tbl As New DataTable
        Dim WW_T0007tbl As New DataTable
        Dim T0007row As DataRow = Nothing
        Dim T0007HEADRow As DataRow = Nothing
        Dim L0001row As DataRow = Nothing
        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ T00004UPDtblより統計ＤＢ追加 ■■■
        '
        CS0026TblSort.TABLE = I_T7tbl
        CS0026TblSort.FILTER = "OPERATION = '更新' and STAFFKBN like '03*'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007tbl = CS0026TblSort.sort()

        For i As Integer = 0 To WW_T0007tbl.Rows.Count - 1
            Try

                T0007row = WW_T0007tbl.Rows(i)

                'ヘッダレコードをキープ
                If T0007row("RECODEKBN") = "0" And T0007row("HDKBN") = "H" Then
                    T0007HEADRow = T0007row
                End If

                If T0007row("RECODEKBN") = "0" And T0007row("HDKBN") = "D" And T0007row("WORKKBN") = "BX" Then
                Else
                    Continue For
                End If

                L0001row = IO_L1tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.CAMPCODE = T0007row("CAMPCODE")
                CS0033AutoNumber.MORG = T0007row("HORG")
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.USERID = I_USERID
                CS0033AutoNumber.getAutoNumber()
                If CS0033AutoNumber.ERR = C_MESSAGE_NO.NORMAL Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    CS0011LOGWRITE.INFSUBCLASS = "L0001tblEtcEdit"       'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "L0001tblEtcEdit"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "採番エラー"
                    CS0011LOGWRITE.MESSAGENO = CS0033AutoNumber.ERR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If
                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L0001row("CAMPCODE") = T0007HEADRow("CAMPCODE")                            '会社コード
                L0001row("MOTOCHO") = "LO"                                                 '元帳（非会計予定を設定）
                L0001row("VERSION") = "000"                                                'バージョン
                L0001row("DENTYPE") = "T07"                                                '伝票タイプ
                L0001row("TENKI") = "0"                                                    '統計転記
                L0001row("KEIJOYMD") = T0007HEADRow("WORKDATE")                            '計上日付（勤務年月日を設定）
                L0001row("DENYMD") = T0007HEADRow("WORKDATE")                              '伝票日付（勤務年月日設定）
                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(T0007HEADRow("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                L0001row("DENNO") = T0007HEADRow("HORG") &
                                    WW_DENNO &
                                    WW_SEQ
                '関連伝票No＋明細No
                L0001row("KANRENDENNO") = T0007HEADRow("HORG") & " " _
                              & T0007HEADRow("STAFFCODE") & " " _
                              & T0007HEADRow("WORKDATE") & " "

                L0001row("ACTORICODE") = ""                                                 '取引先コード
                L0001row("ACOILTYPE") = ""                                                  '油種
                L0001row("ACSHARYOTYPE") = ""                                               '統一車番(上)
                L0001row("ACTSHABAN") = ""                                                  '統一車番(下)
                L0001row("ACSTAFFCODE") = ""                                                '従業員コード
                L0001row("ACBANKAC") = ""                                                   '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050Session.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If CS0006TERMchk.ERR = C_MESSAGE_NO.NORMAL Then
                    L0001row("ACKEIJOMORG") = CS0006TERMchk.MORG                         '計上管理部署コード(部・支店）)
                Else
                    L0001row("ACKEIJOMORG") = T0007HEADRow("MORG")                       '計上管理部署コード（管理部署）
                End If

                L0001row("ACTAXKBN") = 0                                                 '税区分
                L0001row("ACAMT") = 0                                                    '金額
                L0001row("NACSHUKODATE") = T0007HEADRow("WORKDATE")                      '勤務日
                L0001row("NACSHUKADATE") = "1950/01/01"                                  '出荷日
                L0001row("NACTODOKEDATE") = "1950/01/01"                                 '届日
                L0001row("NACTORICODE") = ""                                             '荷主コード
                L0001row("NACURIKBN") = ""                                               '売上計上基準
                L0001row("NACTODOKECODE") = ""                                           '届先コード
                L0001row("NACSTORICODE") = ""                                            '販売店コード
                L0001row("NACSHUKABASHO") = ""                                           '出荷場所

                L0001row("NACTORITYPE01") = ""                                           '取引先・取引タイプ01
                L0001row("NACTORITYPE02") = ""                                           '取引先・取引タイプ02
                L0001row("NACTORITYPE03") = ""                                           '取引先・取引タイプ03
                L0001row("NACTORITYPE04") = ""                                           '取引先・取引タイプ04
                L0001row("NACTORITYPE05") = ""                                           '取引先・取引タイプ05

                L0001row("NACOILTYPE") = ""                                              '油種
                L0001row("NACPRODUCT1") = ""                                             '品名１
                L0001row("NACPRODUCT2") = ""                                             '品名２

                L0001row("NACGSHABAN") = ""                                              '業務車番

                L0001row("NACSUPPLIERKBN") = ""                                          '社有・庸車区分
                L0001row("NACSUPPLIER") = ""                                             '庸車会社

                L0001row("NACSHARYOOILTYPE") = ""                                        '車両登録油種

                L0001row("NACSHARYOTYPE1") = ""                                          '統一車番(上)1
                L0001row("NACTSHABAN1") = ""                                             '統一車番(下)1
                L0001row("NACMANGMORG1") = ""                                            '車両管理部署1
                L0001row("NACMANGSORG1") = ""                                            '車両設置部署1
                L0001row("NACMANGUORG1") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE1") = ""                                           '車両所有1

                L0001row("NACSHARYOTYPE2") = ""                                          '統一車番(上)2
                L0001row("NACTSHABAN2") = ""                                             '統一車番(下)2
                L0001row("NACMANGMORG2") = ""                                            '車両管理部署2
                L0001row("NACMANGSORG2") = ""                                            '車両設置部署2
                L0001row("NACMANGUORG2") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE2") = ""                                           '車両所有2

                L0001row("NACSHARYOTYPE3") = ""                                          '統一車番(上)3
                L0001row("NACTSHABAN3") = ""                                             '統一車番(下)3
                L0001row("NACMANGMORG3") = ""                                            '車両管理部署3
                L0001row("NACMANGSORG3") = ""                                            '車両設置部署3
                L0001row("NACMANGUORG3") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE3") = ""                                           '車両所有3

                L0001row("NACCREWKBN") = ""                                              '正副区分
                L0001row("NACSTAFFCODE") = ""                                            '従業員コード（正）

                L0001row("NACSTAFFKBN") = ""                                             '社員区分（正）
                L0001row("NACMORG") = ""                                                 '管理部署（正）
                L0001row("NACHORG") = ""                                                 '配属部署（正）
                L0001row("NACSORG") = ""                                                 '作業部署（正）

                L0001row("NACSTAFFCODE2") = ""                                           '従業員コード（副）

                L0001row("NACSTAFFKBN2") = ""                                            '社員区分（副）
                L0001row("NACMORG2") = ""                                                '管理部署（副）
                L0001row("NACHORG2") = ""                                                '配属部署（副）
                L0001row("NACSORG2") = ""                                                '作業部署（副）

                L0001row("NACORDERNO") = ""                                              '受注番号
                L0001row("NACDETAILNO") = ""                                             '明細№
                L0001row("NACTRIPNO") = ""                                               'トリップ
                L0001row("NACDROPNO") = ""                                               'ドロップ
                L0001row("NACSEQ") = ""                                                  'SEQ

                L0001row("NACORDERORG") = ""                                             '受注部署
                L0001row("NACSHIPORG") = ""                                              '配送部署
                L0001row("NACSURYO") = 0                                                 '受注・数量
                L0001row("NACTANI") = ""                                                 '受注・単位
                L0001row("NACJSURYO") = 0                                                '実績・配送数量
                L0001row("NACSTANI") = ""                                                '実績・配送単位
                L0001row("NACHAIDISTANCE") = 0                                           '実績・配送距離
                L0001row("NACKAIDISTANCE") = 0                                           '実績・回送作業距離
                L0001row("NACCHODISTANCE") = 0                                           '実績・勤怠調整距離
                L0001row("NACTTLDISTANCE") = 0                                           '実績・配送距離合計Σ
                L0001row("NACHAISTDATE") = "1950/01/01"                                  '実績・配送作業開始日時
                L0001row("NACHAIENDDATE") = "1950/01/01"                                 '実績・配送作業終了日時
                L0001row("NACHAIWORKTIME") = 0                                           '実績・配送作業時間（分）
                L0001row("NACGESSTDATE") = "1950/01/01"                                  '実績・下車作業開始日時
                L0001row("NACGESENDDATE") = "1950/01/01"                                 '実績・下車作業終了日時
                L0001row("NACGESWORKTIME") = 0                                           '実績・下車作業時間（分）
                L0001row("NACCHOWORKTIME") = HHMMtoMinutes(T0007row("WORKTIME"))         '実績・勤怠調整時間（分）
                L0001row("NACTTLWORKTIME") = HHMMtoMinutes(T0007row("WORKTIME"))         '実績・配送合計時間Σ（分）
                L0001row("NACOUTWORKTIME") = 0                                           '実績・就業外時間
                L0001row("NACBREAKSTDATE") = "1950/01/01"                                '実績・休憩開始日時
                L0001row("NACBREAKENDDATE") = "1950/01/01"                               '実績・休憩終了日時
                L0001row("NACBREAKTIME") = 0                                             '実績・休憩時間（分）
                L0001row("NACCHOBREAKTIME") = 0                                          '実績・休憩調整時間（分）
                L0001row("NACTTLBREAKTIME") = 0                                          '実績・休憩合計時間Σ（分）
                L0001row("NACCASH") = 0                                                  '実績・現金
                L0001row("NACETC") = 0                                                   '実績・ETC
                L0001row("NACTICKET") = 0                                                '実績・回数券
                L0001row("NACKYUYU") = 0                                                 '実績・軽油
                L0001row("NACUNLOADCNT") = 0                                             '実績・荷卸回数
                L0001row("NACCHOUNLOADCNT") = 0                                          '実績・荷卸回数調整
                L0001row("NACTTLUNLOADCNT") = 0                                          '実績・荷卸回数合計Σ
                L0001row("NACKAIJI") = 0                                                 '実績・回次
                L0001row("NACJITIME") = 0                                                '実績・実車時間（分）
                L0001row("NACJICHOSTIME") = 0                                            '実績・実車時間調整（分）
                L0001row("NACJITTLETIME") = 0                                            '実績・実車時間合計Σ（分）
                L0001row("NACKUTIME") = 0                                                '実績・空車時間（分）
                L0001row("NACKUCHOTIME") = 0                                             '実績・空車時間調整（分）
                L0001row("NACKUTTLTIME") = 0                                             '実績・空車時間合計Σ（分）
                L0001row("NACJIDISTANCE") = 0                                            '実績・実車距離
                L0001row("NACJICHODISTANCE") = 0                                         '実績・実車距離調整
                L0001row("NACJITTLDISTANCE") = 0                                         '実績・実車距離合計Σ
                L0001row("NACKUDISTANCE") = 0                                            '実績・空車距離
                L0001row("NACKUCHODISTANCE") = 0                                         '実績・空車距離調整
                L0001row("NACKUTTLDISTANCE") = 0                                         '実績・空車距離合計Σ
                L0001row("NACTARIFFFARE") = 0                                            '実績・運賃タリフ額
                L0001row("NACFIXEDFARE") = 0                                             '実績・運賃固定額
                L0001row("NACINCHOFARE") = 0                                             '実績・運賃手入力調整額
                L0001row("NACTTLFARE") = 0                                               '実績・運賃合計額Σ
                L0001row("NACOFFICESORG") = T0007HEADRow("SORG")                         '実績・作業部署
                L0001row("NACOFFICETIME") = 0                                            '実績・事務時間
                L0001row("NACOFFICEBREAKTIME") = 0                                       '実績・事務休憩時間
                L0001row("PAYSHUSHADATE") = T0007HEADRow("STDATE") & " " & T0007HEADRow("STTIME")  '出社日時
                L0001row("PAYTAISHADATE") = T0007HEADRow("ENDDATE") & " " & T0007HEADRow("ENDTIME") '退社日時
                L0001row("PAYSTAFFCODE") = T0007HEADRow("STAFFCODE")                           '従業員コード
                L0001row("PAYSTAFFKBN") = T0007HEADRow("STAFFKBN")                             '社員区分
                L0001row("PAYMORG") = T0007HEADRow("MORG")                                     '従業員管理部署
                L0001row("PAYHORG") = T0007HEADRow("HORG")                                     '従業員配属部署
                L0001row("PAYHOLIDAYKBN") = T0007HEADRow("HOLIDAYKBN")                     '休日区分
                L0001row("PAYKBN") = T0007HEADRow("PAYKBN")                                '勤怠区分
                L0001row("PAYSHUKCHOKKBN") = T0007HEADRow("SHUKCHOKKBN")                   '宿日直区分
                L0001row("PAYJYOMUKBN") = "2"                                              '乗務区分(2:下車勤務）
                L0001row("PAYOILKBN") = ""                                                 '勤怠用油種区分
                L0001row("PAYSHARYOKBN") = ""                                              '勤怠用車両区分
                L0001row("PAYWORKNISSU") = 0                                               '所労
                L0001row("PAYSHOUKETUNISSU") = 0                                           '傷欠
                L0001row("PAYKUMIKETUNISSU") = 0                                           '組欠
                L0001row("PAYETCKETUNISSU") = 0                                            '他欠
                L0001row("PAYNENKYUNISSU") = 0                                             '年休
                L0001row("PAYTOKUKYUNISSU") = 0                                            '特休
                L0001row("PAYCHIKOKSOTAINISSU") = 0                                        '遅早
                L0001row("PAYSTOCKNISSU") = 0                                              'ストック休暇
                L0001row("PAYKYOTEIWEEKNISSU") = 0                                         '協定週休
                L0001row("PAYWEEKNISSU") = 0                                               '週休
                L0001row("PAYDAIKYUNISSU") = 0                                             '代休
                L0001row("PAYWORKTIME") = 0                                                '所定労働時間（分）
                L0001row("PAYWWORKTIME") = 0                                               '所定内時間（分）
                L0001row("PAYNIGHTTIME") = 0                                               '所定深夜時間（分）
                L0001row("PAYORVERTIME") = 0                                               '平日残業時間（分）
                L0001row("PAYWNIGHTTIME") = 0                                              '平日深夜時間（分）
                L0001row("PAYWSWORKTIME") = 0                                              '日曜出勤時間（分）
                L0001row("PAYSNIGHTTIME") = 0                                              '日曜深夜時間（分）
                L0001row("PAYSDAIWORKTIME") = 0                                            '日曜代休出勤時間（分）
                L0001row("PAYSDAINIGHTTIME") = 0                                           '日曜代休深夜時間（分）
                L0001row("PAYHWORKTIME") = 0                                               '休日出勤時間（分）
                L0001row("PAYHNIGHTTIME") = 0                                              '休日深夜時間（分）
                L0001row("PAYHDAIWORKTIME") = 0                                            '休日代休出勤時間（分）
                L0001row("PAYHDAINIGHTTIME") = 0                                           '休日代休深夜時間（分）
                L0001row("PAYBREAKTIME") = 0                                               '休憩時間（分）

                L0001row("PAYNENSHINISSU") = T0007HEADRow("NENSHINISSUTTL")                '年始出勤
                L0001row("PAYNENMATUNISSU") = T0007HEADRow("NENMATUNISSUTTL")              '年末出勤
                L0001row("PAYSHUKCHOKNNISSU") = T0007HEADRow("SHUKCHOKNNISSUTTL")          '宿日直年始
                L0001row("PAYSHUKCHOKNISSU") = T0007HEADRow("SHUKCHOKNISSUTTL")            '宿日直通常
                L0001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                L0001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                L0001row("PAYTOKSAAKAISU") = 0                                             '特作A
                L0001row("PAYTOKSABKAISU") = 0                                             '特作B
                L0001row("PAYTOKSACKAISU") = 0                                             '特作C
                L0001row("PAYTENKOKAISU") = 0                                              '点呼回数
                L0001row("PAYHOANTIME") = 0                                                '保安検査入力（分）
                L0001row("PAYKOATUTIME") = 0                                               '高圧作業入力（分）
                L0001row("PAYTOKUSA1TIME") = 0                                             '特作Ⅰ（分）
                'L0001row("PAYHAYADETIME") = 0                                              '時差出勤手当（分）
                L0001row("PAYPONPNISSU") = 0                                               'ポンプ
                L0001row("PAYBULKNISSU") = 0                                               'バルク
                L0001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L0001row("PAYBKINMUKAISU") = 0                                             'B勤務

                L0001row("PAYYENDTIME") = 0                                                '予定終了時間
                L0001row("PAYAPPLYID") = ""                                                '申請ID
                L0001row("PAYRIYU") = ""                                                   '理由
                L0001row("PAYRIYUETC") = ""                                                '理由(その他）
                L0001row("PAYHAYADETIME") = 0                                              '早出補填時間
                L0001row("PAYHAISOTIME") = 0                                               '配送時間
                L0001row("PAYSHACHUHAKNISSU") = 0                                          '車中泊日数
                L0001row("PAYMODELDISTANCE") = 0                                           'モデル距離
                L0001row("PAYJIKYUSHATIME") = 0                                            '時給者時間
                L0001row("PAYJYOMUTIME") = 0                                               '乗務時間
                L0001row("PAYHWORKNISSU") = 0                                              '休日出勤日数
                L0001row("PAYKAITENCNT") = 0                                               '回転数
                L0001row("PAYSENJYOCNT") = 0                                               '洗浄回数
                L0001row("PAYUNLOADADDCNT1") = 0                                           '危険物荷卸回数1
                L0001row("PAYUNLOADADDCNT2") = 0                                           '危険物荷卸回数2
                L0001row("PAYUNLOADADDCNT3") = 0                                           '危険物荷卸回数3
                L0001row("PAYUNLOADADDCNT4") = 0                                           '危険物荷卸回数4
                L0001row("PAYSHORTDISTANCE1") = 0                                          '短距離手当1
                L0001row("PAYSHORTDISTANCE2") = 0                                          '短距離手当2

                L0001row("APPKIJUN") = ""                                                  '配賦基準
                L0001row("APPKEY") = ""                                                    '配賦統計キー

                L0001row("WORKKBN") = T0007HEADRow("WORKKBN")                              '作業区分
                L0001row("KEYSTAFFCODE") = T0007HEADRow("STAFFCODE")                       '従業員コードキー
                L0001row("KEYGSHABAN") = ""                                                '業務車番キー
                L0001row("KEYTRIPNO") = ""                                                 'トリップキー
                L0001row("KEYDROPNO") = ""                                                 'ドロップキー

                L0001row("DELFLG") = "0"                                                   '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                          '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L0001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L0001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L0001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                            '伝票タイプ

                CS0038ACCODEget.TORICODE = L0001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L0001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L0001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L0001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L0001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L0001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L0001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L0001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L0001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L0001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L0001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L0001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L0001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L0001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L0001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L0001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L0001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "ELD"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "ELC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow
                '------------------------------------------------------
                '削除データ
                '------------------------------------------------------
                'If T0007HEADrow("DELFLG") = "1" Then
                '    '●借方
                '    L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                '    L0001row("INQKBN") = WW_INQKBN_D                                         '照会区分
                '    L0001row("ACDCKBN") = "D"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "ELD"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "01"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("SORG")                    '計上部署コード（作業部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)

                '    '●貸方
                '    L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                '    L0001row("INQKBN") = "0"                                         '照会区分
                '    L0001row("ACDCKBN") = "C"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "ELC"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "02"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)
                'End If

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                If T0007HEADRow("DELFLG") = "0" Then
                    '●借方
                    If WW_INQKBN_D = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                        L0001row("ACDCKBN") = "D"                                        '貸借区分
                        L0001row("ACACHANTEI") = "ELD"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "01"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADRow("SORG")                    '計上部署コード（作業部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If

                    '●貸方
                    If WW_INQKBN_C = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                        L0001row("ACDCKBN") = "C"                                        '貸借区分
                        L0001row("ACACHANTEI") = "ELC"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "02"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADRow("HORG")                    '計上部署コード（配属部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If
                End If
            Catch ex As Exception
                'ROWデータのCSV(tab)変換
                Dim WW_CSV As String = ""
                DatarowToCsv(WW_T0007tbl.Rows(i), WW_CSV)

                CS0011LOGWRITE.INFSUBCLASS = "L0001tblEtcEdit"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "L0001tblEtcEdit"                  '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA=(" & WW_CSV & ")"
                CS0011LOGWRITE.MESSAGENO = "00001"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Throw

            End Try

        Next

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  L0001tbl編集（休憩・乗務員）
    Public Sub L0001tblBreakEdit(ByVal I_USERID As String, ByRef I_T7tbl As DataTable, ByRef IO_L1tbl As DataTable, ByRef O_RTN As String)

        Dim WW_DATENOW As Date = Date.Now
        Dim WW_M0008tbl As New DataTable
        Dim WW_T0007tbl As New DataTable
        Dim T0007HEADrow As DataRow = Nothing
        Dim T0007row As DataRow = Nothing
        Dim L0001row As DataRow = Nothing
        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ T00004UPDtblより統計ＤＢ追加 ■■■
        '
        CS0026TblSort.TABLE = I_T7tbl
        CS0026TblSort.FILTER = "OPERATION = '更新' and STAFFKBN like '03*'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007tbl = CS0026TblSort.sort()

        For i As Integer = 0 To WW_T0007tbl.Rows.Count - 1
            Try

                T0007row = WW_T0007tbl.Rows(i)

                'ヘッダレコードをキープ
                If T0007row("RECODEKBN") = "0" And T0007row("HDKBN") = "H" Then
                    T0007HEADrow = T0007row
                End If

                If T0007row("RECODEKBN") = "0" And T0007row("HDKBN") = "D" And T0007row("WORKKBN") = "BB" Then
                Else
                    Continue For
                End If

                L0001row = IO_L1tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.CAMPCODE = T0007row("CAMPCODE")
                CS0033AutoNumber.MORG = T0007row("HORG")
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.USERID = I_USERID
                CS0033AutoNumber.getAutoNumber()
                If CS0033AutoNumber.ERR = C_MESSAGE_NO.NORMAL Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    CS0011LOGWRITE.INFSUBCLASS = "L0001tblBreakEdit"       'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "L0001tblBreakEdit"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "採番エラー"
                    CS0011LOGWRITE.MESSAGENO = CS0033AutoNumber.ERR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If

                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L0001row("CAMPCODE") = T0007HEADrow("CAMPCODE")                              '会社コード
                L0001row("MOTOCHO") = "LO"                                                   '元帳（非会計予定を設定）
                L0001row("VERSION") = "000"                                                  'バージョン
                L0001row("DENTYPE") = "T07"                                                  '伝票タイプ
                L0001row("TENKI") = "0"                                                      '統計転記
                L0001row("KEIJOYMD") = T0007HEADrow("WORKDATE")                              '計上日付（勤務年月日を設定）
                L0001row("DENYMD") = T0007HEADrow("WORKDATE")                                '伝票日付（勤務年月日設定）
                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(T0007HEADrow("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                L0001row("DENNO") = T0007HEADrow("HORG") &
                                    WW_DENNO &
                                    WW_SEQ
                '関連伝票No＋明細No
                L0001row("KANRENDENNO") = T0007HEADrow("HORG") & " " _
                              & T0007HEADrow("STAFFCODE") & " " _
                              & T0007HEADrow("WORKDATE") & " "

                L0001row("ACTORICODE") = ""                                                 '取引先コード
                L0001row("ACOILTYPE") = ""                                                  '油種
                L0001row("ACSHARYOTYPE") = ""                                               '統一車番(上)
                L0001row("ACTSHABAN") = ""                                                  '統一車番(下)
                L0001row("ACSTAFFCODE") = ""                                                '従業員コード
                L0001row("ACBANKAC") = ""                                                   '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050Session.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If CS0006TERMchk.ERR = C_MESSAGE_NO.NORMAL Then
                    L0001row("ACKEIJOMORG") = CS0006TERMchk.MORG                          '計上管理部署コード(部・支店）)
                Else
                    L0001row("ACKEIJOMORG") = T0007HEADrow("MORG")                        '計上管理部署コード（管理部署）
                End If

                L0001row("ACTAXKBN") = 0                                                 '税区分
                L0001row("ACAMT") = 0                                                    '金額
                L0001row("NACSHUKODATE") = T0007HEADrow("WORKDATE")                          '勤務日
                L0001row("NACSHUKADATE") = "1950/01/01"                                  '出荷日
                L0001row("NACTODOKEDATE") = "1950/01/01"                                 '届日
                L0001row("NACTORICODE") = ""                                             '荷主コード
                L0001row("NACURIKBN") = ""                                               '売上計上基準
                L0001row("NACTODOKECODE") = ""                                           '届先コード
                L0001row("NACSTORICODE") = ""                                            '販売店コード
                L0001row("NACSHUKABASHO") = ""                                           '出荷場所

                L0001row("NACTORITYPE01") = ""                                           '取引先・取引タイプ01
                L0001row("NACTORITYPE02") = ""                                           '取引先・取引タイプ02
                L0001row("NACTORITYPE03") = ""                                           '取引先・取引タイプ03
                L0001row("NACTORITYPE04") = ""                                           '取引先・取引タイプ04
                L0001row("NACTORITYPE05") = ""                                           '取引先・取引タイプ05

                L0001row("NACOILTYPE") = ""                                              '油種
                L0001row("NACPRODUCT1") = ""                                             '品名１
                L0001row("NACPRODUCT2") = ""                                             '品名２

                L0001row("NACGSHABAN") = ""                                              '業務車番

                L0001row("NACSUPPLIERKBN") = ""                                          '社有・庸車区分
                L0001row("NACSUPPLIER") = ""                                             '庸車会社

                L0001row("NACSHARYOOILTYPE") = ""                                        '車両登録油種

                L0001row("NACSHARYOTYPE1") = ""                                          '統一車番(上)1
                L0001row("NACTSHABAN1") = ""                                             '統一車番(下)1
                L0001row("NACMANGMORG1") = ""                                            '車両管理部署1
                L0001row("NACMANGSORG1") = ""                                            '車両設置部署1
                L0001row("NACMANGUORG1") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE1") = ""                                           '車両所有1

                L0001row("NACSHARYOTYPE2") = ""                                          '統一車番(上)2
                L0001row("NACTSHABAN2") = ""                                             '統一車番(下)2
                L0001row("NACMANGMORG2") = ""                                            '車両管理部署2
                L0001row("NACMANGSORG2") = ""                                            '車両設置部署2
                L0001row("NACMANGUORG2") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE2") = ""                                           '車両所有2

                L0001row("NACSHARYOTYPE3") = ""                                          '統一車番(上)3
                L0001row("NACTSHABAN3") = ""                                             '統一車番(下)3
                L0001row("NACMANGMORG3") = ""                                            '車両管理部署3
                L0001row("NACMANGSORG3") = ""                                            '車両設置部署3
                L0001row("NACMANGUORG3") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE3") = ""                                           '車両所有3

                L0001row("NACCREWKBN") = ""                                              '正副区分
                L0001row("NACSTAFFCODE") = ""                                            '従業員コード（正）

                L0001row("NACSTAFFKBN") = ""                                             '社員区分（正）
                L0001row("NACMORG") = ""                                                 '管理部署（正）
                L0001row("NACHORG") = ""                                                 '配属部署（正）
                L0001row("NACSORG") = ""                                                 '作業部署（正）

                L0001row("NACSTAFFCODE2") = ""                                           '従業員コード（副）

                L0001row("NACSTAFFKBN2") = ""                                            '社員区分（副）
                L0001row("NACMORG2") = ""                                                '管理部署（副）
                L0001row("NACHORG2") = ""                                                '配属部署（副）
                L0001row("NACSORG2") = ""                                                '作業部署（副）

                L0001row("NACORDERNO") = ""                                              '受注番号
                L0001row("NACDETAILNO") = ""                                             '明細№
                L0001row("NACTRIPNO") = ""                                               'トリップ
                L0001row("NACDROPNO") = ""                                               'ドロップ
                L0001row("NACSEQ") = ""                                                  'SEQ

                L0001row("NACORDERORG") = ""                                             '受注部署
                L0001row("NACSHIPORG") = ""                                              '配送部署
                L0001row("NACSURYO") = 0                                                 '受注・数量
                L0001row("NACTANI") = ""                                                 '受注・単位
                L0001row("NACJSURYO") = 0                                                  '実績・配送数量
                L0001row("NACSTANI") = ""                                                  '実績・配送単位
                L0001row("NACHAIDISTANCE") = 0                                             '実績・配送距離
                L0001row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
                L0001row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
                L0001row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
                L0001row("NACHAISTDATE") = "1950/01/01"                                    '実績・配送作業開始日時
                L0001row("NACHAIENDDATE") = "1950/01/01"                                   '実績・配送作業終了日時
                L0001row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
                L0001row("NACGESSTDATE") = "1950/01/01"                                    '実績・下車作業開始日時
                L0001row("NACGESENDDATE") = "1950/01/01"                                   '実績・下車作業終了日時
                L0001row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
                L0001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                L0001row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
                L0001row("NACOUTWORKTIME") = 0                                             '実績・就業外時間
                L0001row("NACBREAKSTDATE") = "1950/01/01"                                  '実績・休憩開始日時
                L0001row("NACBREAKENDDATE") = "1950/01/01"                                 '実績・休憩終了日時
                L0001row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
                L0001row("NACCHOBREAKTIME") = HHMMtoMinutes(T0007row("WORKTIME"))          '実績・休憩調整時間（分）
                L0001row("NACTTLBREAKTIME") = HHMMtoMinutes(T0007row("WORKTIME"))          '実績・休憩合計時間Σ（分）
                L0001row("NACCASH") = 0                                                    '実績・現金
                L0001row("NACETC") = 0                                                     '実績・ETC
                L0001row("NACTICKET") = 0                                                  '実績・回数券
                L0001row("NACKYUYU") = 0                                                   '実績・軽油
                L0001row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
                L0001row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
                L0001row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
                L0001row("NACKAIJI") = 0                                                   '実績・回次
                L0001row("NACJITIME") = 0                                                  '実績・実車時間（分）
                L0001row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
                L0001row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
                L0001row("NACKUTIME") = 0                                                  '実績・空車時間（分）
                L0001row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
                L0001row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
                L0001row("NACJIDISTANCE") = 0                                              '実績・実車距離
                L0001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L0001row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
                L0001row("NACKUDISTANCE") = 0                                              '実績・空車距離
                L0001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L0001row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
                L0001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L0001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L0001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L0001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
                L0001row("NACOFFICESORG") = T0007HEADrow("SORG")                           '実績・作業部署
                L0001row("NACOFFICETIME") = 0                                              '実績・事務時間
                L0001row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
                L0001row("PAYSHUSHADATE") = T0007HEADrow("STDATE") & " " & T0007HEADrow("STTIME")  '出社日時
                L0001row("PAYTAISHADATE") = T0007HEADrow("ENDDATE") & " " & T0007HEADrow("ENDTIME") '退社日時
                L0001row("PAYSTAFFCODE") = T0007HEADrow("STAFFCODE")                           '従業員コード
                L0001row("PAYSTAFFKBN") = T0007HEADrow("STAFFKBN")                             '社員区分
                L0001row("PAYMORG") = T0007HEADrow("MORG")                                     '従業員管理部署
                L0001row("PAYHORG") = T0007HEADrow("HORG")                                     '従業員配属部署
                L0001row("PAYHOLIDAYKBN") = T0007HEADrow("HOLIDAYKBN")                     '休日区分
                L0001row("PAYKBN") = T0007HEADrow("PAYKBN")                                '勤怠区分
                L0001row("PAYSHUKCHOKKBN") = T0007HEADrow("SHUKCHOKKBN")                   '宿日直区分
                L0001row("PAYJYOMUKBN") = "2"                                              '乗務区分
                L0001row("PAYOILKBN") = ""                                                 '勤怠用油種区分
                L0001row("PAYSHARYOKBN") = ""                                              '勤怠用車両区分
                L0001row("PAYWORKNISSU") = 0                                               '所労
                L0001row("PAYSHOUKETUNISSU") = 0                                           '傷欠
                L0001row("PAYKUMIKETUNISSU") = 0                                           '組欠
                L0001row("PAYETCKETUNISSU") = 0                                            '他欠
                L0001row("PAYNENKYUNISSU") = 0                                             '年休
                L0001row("PAYTOKUKYUNISSU") = 0                                            '特休
                L0001row("PAYCHIKOKSOTAINISSU") = 0                                        '遅早
                L0001row("PAYSTOCKNISSU") = 0                                              'ストック休暇
                L0001row("PAYKYOTEIWEEKNISSU") = 0                                         '協定週休
                L0001row("PAYWEEKNISSU") = 0                                               '週休
                L0001row("PAYDAIKYUNISSU") = 0                                             '代休
                L0001row("PAYWORKTIME") = 0                                                '所定労働時間（分）
                L0001row("PAYWWORKTIME") = 0                                               '所定内時間（分）
                L0001row("PAYNIGHTTIME") = 0                                               '所定深夜時間（分）
                L0001row("PAYORVERTIME") = 0                                               '平日残業時間（分）
                L0001row("PAYWNIGHTTIME") = 0                                              '平日深夜時間（分）
                L0001row("PAYWSWORKTIME") = 0                                              '日曜出勤時間（分）
                L0001row("PAYSNIGHTTIME") = 0                                              '日曜深夜時間（分）
                L0001row("PAYSDAIWORKTIME") = 0                                            '日曜代休出勤時間（分）
                L0001row("PAYSDAINIGHTTIME") = 0                                           '日曜代休深夜時間（分）
                L0001row("PAYHWORKTIME") = 0                                               '休日出勤時間（分）
                L0001row("PAYHNIGHTTIME") = 0                                              '休日深夜時間（分）
                L0001row("PAYHDAIWORKTIME") = 0                                            '休日代休出勤時間（分）
                L0001row("PAYHDAINIGHTTIME") = 0                                           '休日代休深夜時間（分）
                L0001row("PAYBREAKTIME") = 0                                               '休憩時間（分）

                L0001row("PAYNENSHINISSU") = T0007HEADrow("NENSHINISSUTTL")                '年始出勤
                L0001row("PAYNENMATUNISSU") = T0007HEADrow("NENMATUNISSUTTL")              '年末出勤
                L0001row("PAYSHUKCHOKNNISSU") = T0007HEADrow("SHUKCHOKNNISSUTTL")          '宿日直年始
                L0001row("PAYSHUKCHOKNISSU") = T0007HEADrow("SHUKCHOKNISSUTTL")            '宿日直通常
                L0001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                L0001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                L0001row("PAYTOKSAAKAISU") = 0                                             '特作A
                L0001row("PAYTOKSABKAISU") = 0                                             '特作B
                L0001row("PAYTOKSACKAISU") = 0                                             '特作C
                L0001row("PAYTENKOKAISU") = 0                                              '点呼回数
                L0001row("PAYHOANTIME") = 0                                                '保安検査入力（分）
                L0001row("PAYKOATUTIME") = 0                                               '高圧作業入力（分）
                L0001row("PAYTOKUSA1TIME") = 0                                             '特作Ⅰ（分）
                L0001row("PAYPONPNISSU") = 0                                               'ポンプ
                L0001row("PAYBULKNISSU") = 0                                               'バルク
                L0001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L0001row("PAYBKINMUKAISU") = 0                                             'B勤務

                L0001row("PAYHAYADETIME") = 0                                              '早出補填時間
                L0001row("PAYHAISOTIME") = 0                                               '配送時間
                L0001row("PAYSHACHUHAKNISSU") = 0                                          '車中泊日数
                L0001row("PAYMODELDISTANCE") = 0                                           'モデル距離
                L0001row("PAYJIKYUSHATIME") = 0                                            '時給者時間
                L0001row("PAYJYOMUTIME") = 0                                               '乗務時間
                L0001row("PAYHWORKNISSU") = 0                                              '休日出勤日数
                L0001row("PAYKAITENCNT") = 0                                               '回転数
                L0001row("PAYSENJYOCNT") = 0                                               '洗浄回数
                L0001row("PAYUNLOADADDCNT1") = 0                                           '危険物荷卸回数1
                L0001row("PAYUNLOADADDCNT2") = 0                                           '危険物荷卸回数2
                L0001row("PAYUNLOADADDCNT3") = 0                                           '危険物荷卸回数3
                L0001row("PAYUNLOADADDCNT4") = 0                                           '危険物荷卸回数4
                L0001row("PAYSHORTDISTANCE1") = 0                                          '短距離手当1
                L0001row("PAYSHORTDISTANCE2") = 0                                          '短距離手当2

                L0001row("APPKIJUN") = ""                                                  '配賦基準
                L0001row("APPKEY") = ""                                                    '配賦統計キー

                L0001row("WORKKBN") = T0007HEADrow("WORKKBN")                              '作業区分
                L0001row("KEYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コードキー
                L0001row("KEYGSHABAN") = ""                                                '業務車番キー
                L0001row("KEYTRIPNO") = ""                                                 'トリップキー
                L0001row("KEYDROPNO") = ""                                                 'ドロップキー

                L0001row("DELFLG") = "0"                                                   '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                          '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L0001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L0001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L0001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                            '伝票タイプ

                CS0038ACCODEget.TORICODE = L0001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L0001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L0001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L0001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L0001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L0001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L0001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L0001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L0001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L0001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L0001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L0001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L0001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L0001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L0001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L0001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L0001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "HRD"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "HRC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow
                '------------------------------------------------------
                '削除データ
                '------------------------------------------------------
                'If T0007HEADrow("DELFLG") = "1" Then
                '    '●借方
                '    L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                '    L0001row("INQKBN") = "1"                                         '照会区分
                '    L0001row("ACDCKBN") = "D"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "ERD"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "01"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("SORG")                    '計上部署コード（作業部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)

                '    '●貸方
                '    L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                '    L0001row("INQKBN") = "0"                                         '照会区分
                '    L0001row("ACDCKBN") = "C"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "ERC"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "02"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)
                'End If

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                If T0007HEADrow("DELFLG") = "0" Then
                    '●借方
                    If WW_INQKBN_D = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                        L0001row("ACDCKBN") = "D"                                        '貸借区分
                        L0001row("ACACHANTEI") = "HRD"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "01"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("SORG")                    '計上部署コード（作業部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If

                    '●貸方
                    If WW_INQKBN_C = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                        L0001row("ACDCKBN") = "C"                                        '貸借区分
                        L0001row("ACACHANTEI") = "HRC"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "02"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If
                End If
            Catch ex As Exception
                'ROWデータのCSV(tab)変換
                Dim WW_CSV As String = ""
                DatarowToCsv(WW_T0007tbl.Rows(i), WW_CSV)

                CS0011LOGWRITE.INFSUBCLASS = "L0001tblBreakEdit"            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "L0001tblBreakEdit"                '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA=(" & WW_CSV & ")"
                CS0011LOGWRITE.MESSAGENO = "00001"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Throw

            End Try

        Next

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  L0001tbl編集（日別合計・乗務員）
    Public Sub L0001tblDailyTtlEdit(ByVal I_USERID As String, ByRef I_T7tbl As DataTable, ByRef IO_L1tbl As DataTable, ByRef O_RTN As String)

        Dim WW_DATENOW As Date = Date.Now
        Dim WW_M0008tbl As New DataTable
        Dim WW_T0007tbl As New DataTable
        Dim T0007HEADrow As DataRow = Nothing
        Dim L0001row As DataRow = Nothing
        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ T00004UPDtblより統計ＤＢ追加 ■■■
        '
        CS0026TblSort.TABLE = I_T7tbl
        CS0026TblSort.FILTER = "OPERATION = '更新' and STAFFKBN like '03*'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007tbl = CS0026TblSort.sort()

        For i As Integer = 0 To WW_T0007tbl.Rows.Count - 1
            Try

                T0007HEADrow = WW_T0007tbl.Rows(i)

                'ヘッダレコードをキープ
                If T0007HEADrow("RECODEKBN") = "0" AndAlso T0007HEADrow("HDKBN") = "H" Then
                Else
                    Continue For
                End If

                L0001row = IO_L1tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.CAMPCODE = T0007HEADrow("CAMPCODE")
                CS0033AutoNumber.MORG = T0007HEADrow("HORG")
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.USERID = I_USERID
                CS0033AutoNumber.getAutoNumber()
                If CS0033AutoNumber.ERR = C_MESSAGE_NO.NORMAL Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    CS0011LOGWRITE.INFSUBCLASS = "L0001tblDailyTtlEdit"       'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "L0001tblDailyTtlEdit"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "採番エラー"
                    CS0011LOGWRITE.MESSAGENO = CS0033AutoNumber.ERR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If

                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L0001row("CAMPCODE") = T0007HEADrow("CAMPCODE")                              '会社コード
                L0001row("MOTOCHO") = "LO"                                                   '元帳（非会計予定を設定）
                L0001row("VERSION") = "000"                                                  'バージョン
                L0001row("DENTYPE") = "T07"                                                  '伝票タイプ
                L0001row("TENKI") = "0"                                                      '統計転記
                L0001row("KEIJOYMD") = T0007HEADrow("WORKDATE")                              '計上日付（勤務年月日を設定）
                L0001row("DENYMD") = T0007HEADrow("WORKDATE")                                '伝票日付（勤務年月日設定）
                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(T0007HEADrow("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                L0001row("DENNO") = T0007HEADrow("HORG") &
                                    WW_DENNO &
                                    WW_SEQ
                '関連伝票No＋明細No
                L0001row("KANRENDENNO") = T0007HEADrow("HORG") & " " _
                              & T0007HEADrow("STAFFCODE") & " " _
                              & T0007HEADrow("WORKDATE") & " "

                L0001row("ACTORICODE") = ""                                                 '取引先コード
                L0001row("ACOILTYPE") = ""                                                  '油種
                L0001row("ACSHARYOTYPE") = ""                                               '統一車番(上)
                L0001row("ACTSHABAN") = ""                                                  '統一車番(下)
                L0001row("ACSTAFFCODE") = ""                                                '従業員コード
                L0001row("ACBANKAC") = ""                                                   '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050Session.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If CS0006TERMchk.ERR = C_MESSAGE_NO.NORMAL Then
                    L0001row("ACKEIJOMORG") = CS0006TERMchk.MORG                         '計上管理部署コード(部・支店）)
                Else
                    L0001row("ACKEIJOMORG") = T0007HEADrow("MORG")                       '計上管理部署コード（管理部署）
                End If

                L0001row("ACTAXKBN") = 0                                                 '税区分
                L0001row("ACAMT") = 0                                                    '金額
                L0001row("NACSHUKODATE") = T0007HEADrow("WORKDATE")                      '勤務日
                L0001row("NACSHUKADATE") = "1950/01/01"                                  '出荷日
                L0001row("NACTODOKEDATE") = "1950/01/01"                                 '届日
                L0001row("NACTORICODE") = ""                                             '荷主コード
                L0001row("NACURIKBN") = ""                                               '売上計上基準
                L0001row("NACTODOKECODE") = ""                                           '届先コード
                L0001row("NACSTORICODE") = ""                                            '販売店コード
                L0001row("NACSHUKABASHO") = ""                                           '出荷場所

                L0001row("NACTORITYPE01") = ""                                           '取引先・取引タイプ01
                L0001row("NACTORITYPE02") = ""                                           '取引先・取引タイプ02
                L0001row("NACTORITYPE03") = ""                                           '取引先・取引タイプ03
                L0001row("NACTORITYPE04") = ""                                           '取引先・取引タイプ04
                L0001row("NACTORITYPE05") = ""                                           '取引先・取引タイプ05

                L0001row("NACOILTYPE") = ""                                              '油種
                L0001row("NACPRODUCT1") = ""                                             '品名１
                L0001row("NACPRODUCT2") = ""                                             '品名２

                L0001row("NACGSHABAN") = ""                                              '業務車番

                L0001row("NACSUPPLIERKBN") = ""                                          '社有・庸車区分
                L0001row("NACSUPPLIER") = ""                                             '庸車会社

                L0001row("NACSHARYOOILTYPE") = ""                                        '車両登録油種

                L0001row("NACSHARYOTYPE1") = ""                                          '統一車番(上)1
                L0001row("NACTSHABAN1") = ""                                             '統一車番(下)1
                L0001row("NACMANGMORG1") = ""                                            '車両管理部署1
                L0001row("NACMANGSORG1") = ""                                            '車両設置部署1
                L0001row("NACMANGUORG1") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE1") = ""                                           '車両所有1

                L0001row("NACSHARYOTYPE2") = ""                                          '統一車番(上)2
                L0001row("NACTSHABAN2") = ""                                             '統一車番(下)2
                L0001row("NACMANGMORG2") = ""                                            '車両管理部署2
                L0001row("NACMANGSORG2") = ""                                            '車両設置部署2
                L0001row("NACMANGUORG2") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE2") = ""                                           '車両所有2

                L0001row("NACSHARYOTYPE3") = ""                                          '統一車番(上)3
                L0001row("NACTSHABAN3") = ""                                             '統一車番(下)3
                L0001row("NACMANGMORG3") = ""                                            '車両管理部署3
                L0001row("NACMANGSORG3") = ""                                            '車両設置部署3
                L0001row("NACMANGUORG3") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE3") = ""                                           '車両所有3

                L0001row("NACCREWKBN") = ""                                              '正副区分
                L0001row("NACSTAFFCODE") = ""                                            '従業員コード（正）

                L0001row("NACSTAFFKBN") = ""                                             '社員区分（正）
                L0001row("NACMORG") = ""                                                 '管理部署（正）
                L0001row("NACHORG") = ""                                                 '配属部署（正）
                L0001row("NACSORG") = ""                                                 '作業部署（正）

                L0001row("NACSTAFFCODE2") = ""                                           '従業員コード（副）

                L0001row("NACSTAFFKBN2") = ""                                            '社員区分（副）
                L0001row("NACMORG2") = ""                                                '管理部署（副）
                L0001row("NACHORG2") = ""                                                '配属部署（副）
                L0001row("NACSORG2") = ""                                                '作業部署（副）

                L0001row("NACORDERNO") = ""                                              '受注番号
                L0001row("NACDETAILNO") = ""                                             '明細№
                L0001row("NACTRIPNO") = ""                                               'トリップ
                L0001row("NACDROPNO") = ""                                               'ドロップ
                L0001row("NACSEQ") = ""                                                  'SEQ

                L0001row("NACORDERORG") = ""                                             '受注部署
                L0001row("NACSHIPORG") = ""                                              '配送部署
                L0001row("NACSURYO") = 0                                                 '受注・数量
                L0001row("NACTANI") = ""                                                 '受注・単位
                L0001row("NACJSURYO") = 0                                                  '実績・配送数量
                L0001row("NACSTANI") = ""                                                  '実績・配送単位
                L0001row("NACHAIDISTANCE") = 0                                             '実績・配送距離
                L0001row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
                L0001row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
                L0001row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
                L0001row("NACHAISTDATE") = "1950/01/01"                                    '実績・配送作業開始日時
                L0001row("NACHAIENDDATE") = "1950/01/01"                                   '実績・配送作業終了日時
                L0001row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
                L0001row("NACGESSTDATE") = "1950/01/01"                                    '実績・下車作業開始日時
                L0001row("NACGESENDDATE") = "1950/01/01"                                   '実績・下車作業終了日時
                L0001row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
                L0001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                L0001row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
                L0001row("NACOUTWORKTIME") = 0                                             '実績・就業外時間
                L0001row("NACBREAKSTDATE") = "1950/01/01"                                  '実績・休憩開始日時
                L0001row("NACBREAKENDDATE") = "1950/01/01"                                 '実績・休憩終了日時
                L0001row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
                L0001row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
                L0001row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
                L0001row("NACCASH") = 0                                                    '実績・現金
                L0001row("NACETC") = 0                                                     '実績・ETC
                L0001row("NACTICKET") = 0                                                  '実績・回数券
                L0001row("NACKYUYU") = 0                                                   '実績・軽油
                L0001row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
                L0001row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
                L0001row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
                L0001row("NACKAIJI") = 0                                                   '実績・回次
                L0001row("NACJITIME") = 0                                                  '実績・実車時間（分）
                L0001row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
                L0001row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
                L0001row("NACKUTIME") = 0                                                  '実績・空車時間（分）
                L0001row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
                L0001row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
                L0001row("NACJIDISTANCE") = 0                                              '実績・実車距離
                L0001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L0001row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
                L0001row("NACKUDISTANCE") = 0                                              '実績・空車距離
                L0001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L0001row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
                L0001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L0001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L0001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L0001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
                L0001row("NACOFFICESORG") = T0007HEADrow("SORG")                           '実績・作業部署
                L0001row("NACOFFICETIME") = 0                                              '実績・事務時間
                L0001row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
                L0001row("PAYSHUSHADATE") = T0007HEADrow("STDATE") & " " & T0007HEADrow("STTIME")  '出社日時
                L0001row("PAYTAISHADATE") = T0007HEADrow("ENDDATE") & " " & T0007HEADrow("ENDTIME") '退社日時
                L0001row("PAYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コード
                L0001row("PAYSTAFFKBN") = T0007HEADrow("STAFFKBN")                         '社員区分
                L0001row("PAYMORG") = T0007HEADrow("MORG")                                 '従業員管理部署
                L0001row("PAYHORG") = T0007HEADrow("HORG")                                 '従業員配属部署
                L0001row("PAYHOLIDAYKBN") = T0007HEADrow("HOLIDAYKBN")                     '休日区分
                L0001row("PAYKBN") = T0007HEADrow("PAYKBN")                                '勤怠区分
                L0001row("PAYSHUKCHOKKBN") = T0007HEADrow("SHUKCHOKKBN")                   '宿日直区分
                L0001row("PAYJYOMUKBN") = "2"                                              '乗務区分
                L0001row("PAYOILKBN") = ""                                                 '勤怠用油種区分
                L0001row("PAYSHARYOKBN") = ""                                              '勤怠用車両区分
                If T0007HEADrow("HOLIDAYKBN") = "0" Then
                    L0001row("PAYWORKNISSU") = 1                                           '所労
                Else
                    L0001row("PAYWORKNISSU") = 0                                           '所労
                End If
                L0001row("PAYSHOUKETUNISSU") = T0007HEADrow("SHOUKETUNISSUTTL")            '傷欠
                L0001row("PAYKUMIKETUNISSU") = T0007HEADrow("KUMIKETUNISSUTTL")            '組欠
                L0001row("PAYETCKETUNISSU") = T0007HEADrow("ETCKETUNISSUTTL")              '他欠
                L0001row("PAYNENKYUNISSU") = T0007HEADrow("NENKYUNISSUTTL")                '年休
                L0001row("PAYTOKUKYUNISSU") = T0007HEADrow("TOKUKYUNISSUTTL")              '特休
                L0001row("PAYCHIKOKSOTAINISSU") = T0007HEADrow("CHIKOKSOTAINISSUTTL")      '遅早
                L0001row("PAYSTOCKNISSU") = T0007HEADrow("STOCKNISSUTTL")                  'ストック休暇
                L0001row("PAYKYOTEIWEEKNISSU") = T0007HEADrow("KYOTEIWEEKNISSUTTL")        '協定週休
                L0001row("PAYWEEKNISSU") = T0007HEADrow("WEEKNISSUTTL")                    '週休
                L0001row("PAYDAIKYUNISSU") = T0007HEADrow("DAIKYUNISSUTTL")                '代休
                L0001row("PAYWORKTIME") = HHMMtoMinutes(T0007HEADrow("BINDTIME"))          '所定労働時間（分）
                L0001row("PAYWWORKTIME") = HHMMtoMinutes(T0007HEADrow("WWORKTIMETTL"))     '所定内時間（分）
                L0001row("PAYNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("NIGHTTIMETTL"))     '所定深夜時間（分）
                L0001row("PAYORVERTIME") = HHMMtoMinutes(T0007HEADrow("ORVERTIMETTL"))     '平日残業時間（分）
                L0001row("PAYWNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("WNIGHTTIMETTL"))   '平日深夜時間（分）
                L0001row("PAYWSWORKTIME") = HHMMtoMinutes(T0007HEADrow("SWORKTIMETTL"))    '日曜出勤時間（分）
                L0001row("PAYSNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("SNIGHTTIMETTL"))   '日曜深夜時間（分）
                L0001row("PAYSDAIWORKTIME") = HHMMtoMinutes(T0007HEADrow("SDAIWORKTIMETTL"))    '日曜代休出勤時間（分）
                L0001row("PAYSDAINIGHTTIME") = HHMMtoMinutes(T0007HEADrow("SDAINIGHTTIMETTL"))  '日曜代休深夜時間（分）
                L0001row("PAYHWORKTIME") = HHMMtoMinutes(T0007HEADrow("HWORKTIMETTL"))     '休日出勤時間（分）
                L0001row("PAYHNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("HNIGHTTIMETTL"))   '休日深夜時間（分）
                L0001row("PAYHDAIWORKTIME") = HHMMtoMinutes(T0007HEADrow("HDAIWORKTIMETTL"))     '休日代休出勤時間（分）
                L0001row("PAYHDAINIGHTTIME") = HHMMtoMinutes(T0007HEADrow("HDAINIGHTTIMETTL"))   '休日代休深夜時間（分）
                L0001row("PAYBREAKTIME") = HHMMtoMinutes(T0007HEADrow("BREAKTIME"))        '休憩時間（分）

                L0001row("PAYNENSHINISSU") = T0007HEADrow("NENSHINISSUTTL")                '年始出勤
                L0001row("PAYNENMATUNISSU") = T0007HEADrow("NENMATUNISSUTTL")              '年末出勤
                L0001row("PAYSHUKCHOKNNISSU") = T0007HEADrow("SHUKCHOKNNISSUTTL")          '宿日直年始
                L0001row("PAYSHUKCHOKNISSU") = T0007HEADrow("SHUKCHOKNISSUTTL")            '宿日直通常
                L0001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                L0001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                L0001row("PAYTOKSAAKAISU") = T0007HEADrow("TOKSAAKAISUTTL")                '特作A
                L0001row("PAYTOKSABKAISU") = T0007HEADrow("TOKSABKAISUTTL")                '特作B
                L0001row("PAYTOKSACKAISU") = T0007HEADrow("TOKSACKAISUTTL")                '特作C
                L0001row("PAYTENKOKAISU") = T0007HEADrow("TENKOKAISUTTL")                  '点呼回数
                L0001row("PAYHOANTIME") = HHMMtoMinutes(T0007HEADrow("HOANTIMETTL"))       '保安検査入力（分）
                L0001row("PAYKOATUTIME") = HHMMtoMinutes(T0007HEADrow("KOATUTIMETTL"))     '高圧作業入力（分）
                L0001row("PAYTOKUSA1TIME") = HHMMtoMinutes(T0007HEADrow("TOKUSA1TIMETTL")) '特作Ⅰ（分）
                L0001row("PAYPONPNISSU") = 0                                               'ポンプ
                L0001row("PAYBULKNISSU") = 0                                               'バルク
                L0001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L0001row("PAYBKINMUKAISU") = 0                                             'B勤務

                L0001row("PAYYENDTIME") = 0                                                '予定終了時間
                L0001row("PAYAPPLYID") = ""                                                '申請ID
                L0001row("PAYRIYU") = ""                                                   '理由
                L0001row("PAYRIYUETC") = ""                                                '理由(その他）

                L0001row("PAYHAYADETIME") = HHMMtoMinutes(T0007HEADrow("HAYADETIMETTL"))     '早出補填時間
                L0001row("PAYHAISOTIME") = HHMMtoMinutes(T0007HEADrow("HAISOTIME"))          '配送時間
                L0001row("PAYSHACHUHAKNISSU") = Val(T0007HEADrow("SHACHUHAKNISSUTTL"))                '車中泊日数
                L0001row("PAYMODELDISTANCE") = T0007HEADrow("MODELDISTANCETTL")                       'モデル距離
                L0001row("PAYJIKYUSHATIME") = HHMMtoMinutes(T0007HEADrow("JIKYUSHATIMETTL")) '時給者時間
                L0001row("PAYJYOMUTIME") = HHMMtoMinutes(T0007HEADrow("JYOMUTIMETTL"))       '乗務時間
                L0001row("PAYHWORKNISSU") = T0007HEADrow("HWORKNISSUTTL")                             '休日出勤日数
                L0001row("PAYKAITENCNT") = T0007HEADrow("KAITENCNTTTL")                               '回転数
                L0001row("PAYSENJYOCNT") = T0007HEADrow("SENJYOCNTTTL")                               '洗浄回数
                L0001row("PAYUNLOADADDCNT1") = T0007HEADrow("UNLOADADDCNT1TTL")                       '危険物荷卸回数1
                L0001row("PAYUNLOADADDCNT2") = T0007HEADrow("UNLOADADDCNT2TTL")                       '危険物荷卸回数2
                L0001row("PAYUNLOADADDCNT3") = T0007HEADrow("UNLOADADDCNT3TTL")                       '危険物荷卸回数3
                L0001row("PAYUNLOADADDCNT4") = T0007HEADrow("UNLOADADDCNT4TTL")                       '危険物荷卸回数4
                L0001row("PAYSHORTDISTANCE1") = T0007HEADrow("SHORTDISTANCE1TTL")                     '短距離手当1
                L0001row("PAYSHORTDISTANCE2") = T0007HEADrow("SHORTDISTANCE2TTL")                     '短距離手当2
                L0001row("APPKIJUN") = ""                                                  '配賦基準
                L0001row("APPKEY") = ""                                                    '配賦統計キー

                L0001row("WORKKBN") = T0007HEADrow("WORKKBN")                              '作業区分
                L0001row("KEYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コードキー
                L0001row("KEYGSHABAN") = ""                                                '業務車番キー
                L0001row("KEYTRIPNO") = ""                                                 'トリップキー
                L0001row("KEYDROPNO") = ""                                                 'ドロップキー

                L0001row("DELFLG") = "0"                                                   '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                          '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L0001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L0001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L0001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                            '伝票タイプ

                CS0038ACCODEget.TORICODE = L0001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L0001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L0001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L0001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L0001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L0001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L0001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L0001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L0001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L0001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L0001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L0001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L0001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L0001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L0001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L0001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L0001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "ERD"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "ERC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow
                '------------------------------------------------------
                '削除データ
                '------------------------------------------------------
                'If T0007HEADrow("DELFLG") = "1" Then
                '    '●借方
                '    L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                '    L0001row("INQKBN") = "1"                                         '照会区分
                '    L0001row("ACDCKBN") = "D"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "HRD"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "01"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("SORG")                    '計上部署コード（作業部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)

                '    '●貸方
                '    L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                '    L0001row("INQKBN") = "0"                                         '照会区分
                '    L0001row("ACDCKBN") = "C"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "HRC"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "02"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)
                'End If

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                If T0007HEADrow("DELFLG") = "0" Then
                    '●借方
                    If WW_INQKBN_D = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                        L0001row("ACDCKBN") = "D"                                        '貸借区分
                        L0001row("ACACHANTEI") = "ERD"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "01"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("SORG")                    '計上部署コード（作業部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If

                    '●貸方
                    If WW_INQKBN_C = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                        L0001row("ACDCKBN") = "C"                                        '貸借区分
                        L0001row("ACACHANTEI") = "ERC"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "02"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If
                End If
            Catch ex As Exception
                'ROWデータのCSV(tab)変換
                Dim WW_CSV As String = ""
                DatarowToCsv(WW_T0007tbl.Rows(i), WW_CSV)

                CS0011LOGWRITE.INFSUBCLASS = "L0001tblDailyTtlEdit"         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "L0001tblDailyTtlEdit"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA=(" & WW_CSV & ")"
                CS0011LOGWRITE.MESSAGENO = "00001"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Throw

            End Try

        Next

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  L0001tbl編集（事務員）
    Public Sub L0001tblJimEdit(ByVal I_USERID As String, ByRef I_T7tbl As DataTable, ByRef IO_L1tbl As DataTable, ByRef O_RTN As String)

        Dim WW_DATENOW As Date = Date.Now
        Dim WW_M0008tbl As New DataTable
        Dim WW_T0007tbl As New DataTable
        Dim T0007HEADrow As DataRow = Nothing
        Dim T0007row As DataRow = Nothing
        Dim L0001row As DataRow = Nothing
        O_RTN = C_MESSAGE_NO.NORMAL

        '■■■ T00004UPDtblより統計ＤＢ追加 ■■■
        '
        CS0026TblSort.TABLE = I_T7tbl
        CS0026TblSort.FILTER = "OPERATION = '更新' and STAFFKBN not like '03*'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007tbl = CS0026TblSort.sort()

        For i As Integer = 0 To WW_T0007tbl.Rows.Count - 1
            Try

                T0007HEADrow = WW_T0007tbl.Rows(i)

                'ヘッダレコードをキープ
                If T0007HEADrow("RECODEKBN") = "0" And T0007HEADrow("HDKBN") = "H" Then
                Else
                    Continue For
                End If

                L0001row = IO_L1tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.CAMPCODE = T0007row("CAMPCODE")
                CS0033AutoNumber.MORG = T0007row("HORG")
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.USERID = I_USERID
                CS0033AutoNumber.getAutoNumber()
                If CS0033AutoNumber.ERR = C_MESSAGE_NO.NORMAL Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    CS0011LOGWRITE.INFSUBCLASS = "L0001tblJimEdit"       'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "L0001tblJimEdit"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "採番エラー"
                    CS0011LOGWRITE.MESSAGENO = CS0033AutoNumber.ERR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If

                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L0001row("CAMPCODE") = T0007HEADrow("CAMPCODE")                              '会社コード
                L0001row("MOTOCHO") = "LO"                                                   '元帳（非会計予定を設定）
                L0001row("VERSION") = "000"                                                  'バージョン
                L0001row("DENTYPE") = "T07"                                                  '伝票タイプ
                L0001row("TENKI") = "0"                                                      '統計転記
                L0001row("KEIJOYMD") = T0007HEADrow("WORKDATE")                              '計上日付（勤務年月日を設定）
                L0001row("DENYMD") = T0007HEADrow("WORKDATE")                                '伝票日付（勤務年月日設定）
                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(T0007HEADrow("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                L0001row("DENNO") = T0007HEADrow("HORG") &
                                    WW_DENNO &
                                    WW_SEQ
                '関連伝票No＋明細No
                L0001row("KANRENDENNO") = T0007HEADrow("HORG") & " " _
                              & T0007HEADrow("STAFFCODE") & " " _
                              & T0007HEADrow("WORKDATE") & " "

                L0001row("ACTORICODE") = ""                                                 '取引先コード
                L0001row("ACOILTYPE") = ""                                                  '油種
                L0001row("ACSHARYOTYPE") = ""                                               '統一車番(上)
                L0001row("ACTSHABAN") = ""                                                  '統一車番(下)
                L0001row("ACSTAFFCODE") = ""                                                '従業員コード
                L0001row("ACBANKAC") = ""                                                   '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050Session.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If CS0006TERMchk.ERR = C_MESSAGE_NO.NORMAL Then
                    L0001row("ACKEIJOMORG") = CS0006TERMchk.MORG                          '計上管理部署コード(部・支店）)
                Else
                    L0001row("ACKEIJOMORG") = T0007HEADrow("MORG")                        '計上管理部署コード（管理部署）
                End If

                L0001row("ACTAXKBN") = 0                                                 '税区分
                L0001row("ACAMT") = 0                                                    '金額
                L0001row("NACSHUKODATE") = T0007HEADrow("WORKDATE")                      '勤務日
                L0001row("NACSHUKADATE") = "1950/01/01"                                  '出荷日
                L0001row("NACTODOKEDATE") = "1950/01/01"                                 '届日
                L0001row("NACTORICODE") = ""                                             '荷主コード
                L0001row("NACURIKBN") = ""                                               '売上計上基準
                L0001row("NACTODOKECODE") = ""                                           '届先コード
                L0001row("NACSTORICODE") = ""                                            '販売店コード
                L0001row("NACSHUKABASHO") = ""                                           '出荷場所

                L0001row("NACTORITYPE01") = ""                                           '取引先・取引タイプ01
                L0001row("NACTORITYPE02") = ""                                           '取引先・取引タイプ02
                L0001row("NACTORITYPE03") = ""                                           '取引先・取引タイプ03
                L0001row("NACTORITYPE04") = ""                                           '取引先・取引タイプ04
                L0001row("NACTORITYPE05") = ""                                           '取引先・取引タイプ05

                L0001row("NACOILTYPE") = ""                                              '油種
                L0001row("NACPRODUCT1") = ""                                             '品名１
                L0001row("NACPRODUCT2") = ""                                             '品名２

                L0001row("NACGSHABAN") = ""                                              '業務車番

                L0001row("NACSUPPLIERKBN") = ""                                          '社有・庸車区分
                L0001row("NACSUPPLIER") = ""                                             '庸車会社

                L0001row("NACSHARYOOILTYPE") = ""                                        '車両登録油種

                L0001row("NACSHARYOTYPE1") = ""                                          '統一車番(上)1
                L0001row("NACTSHABAN1") = ""                                             '統一車番(下)1
                L0001row("NACMANGMORG1") = ""                                            '車両管理部署1
                L0001row("NACMANGSORG1") = ""                                            '車両設置部署1
                L0001row("NACMANGUORG1") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE1") = ""                                           '車両所有1

                L0001row("NACSHARYOTYPE2") = ""                                          '統一車番(上)2
                L0001row("NACTSHABAN2") = ""                                             '統一車番(下)2
                L0001row("NACMANGMORG2") = ""                                            '車両管理部署2
                L0001row("NACMANGSORG2") = ""                                            '車両設置部署2
                L0001row("NACMANGUORG2") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE2") = ""                                           '車両所有2

                L0001row("NACSHARYOTYPE3") = ""                                          '統一車番(上)3
                L0001row("NACTSHABAN3") = ""                                             '統一車番(下)3
                L0001row("NACMANGMORG3") = ""                                            '車両管理部署3
                L0001row("NACMANGSORG3") = ""                                            '車両設置部署3
                L0001row("NACMANGUORG3") = ""                                            '車両運用部署1
                L0001row("NACBASELEASE3") = ""                                           '車両所有3

                L0001row("NACCREWKBN") = ""                                              '正副区分
                L0001row("NACSTAFFCODE") = ""                                            '従業員コード（正）

                L0001row("NACSTAFFKBN") = ""                                             '社員区分（正）
                L0001row("NACMORG") = ""                                                 '管理部署（正）
                L0001row("NACHORG") = ""                                                 '配属部署（正）
                L0001row("NACSORG") = ""                                                 '作業部署（正）

                L0001row("NACSTAFFCODE2") = ""                                           '従業員コード（副）

                L0001row("NACSTAFFKBN2") = ""                                            '社員区分（副）
                L0001row("NACMORG2") = ""                                                '管理部署（副）
                L0001row("NACHORG2") = ""                                                '配属部署（副）
                L0001row("NACSORG2") = ""                                                '作業部署（副）

                L0001row("NACORDERNO") = ""                                              '受注番号
                L0001row("NACDETAILNO") = ""                                             '明細№
                L0001row("NACTRIPNO") = ""                                               'トリップ
                L0001row("NACDROPNO") = ""                                               'ドロップ
                L0001row("NACSEQ") = ""                                                  'SEQ

                L0001row("NACORDERORG") = ""                                             '受注部署
                L0001row("NACSHIPORG") = ""                                              '配送部署
                L0001row("NACSURYO") = 0                                                 '受注・数量
                L0001row("NACTANI") = ""                                                 '受注・単位
                L0001row("NACJSURYO") = 0                                                  '実績・配送数量
                L0001row("NACSTANI") = ""                                                  '実績・配送単位
                L0001row("NACHAIDISTANCE") = 0                                             '実績・配送距離
                L0001row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
                L0001row("NACCHODISTANCE") = 0                                             '実績・勤怠調整距離
                L0001row("NACTTLDISTANCE") = 0                                             '実績・配送距離合計Σ
                L0001row("NACHAISTDATE") = "1950/01/01"                                    '実績・配送作業開始日時
                L0001row("NACHAIENDDATE") = "1950/01/01"                                   '実績・配送作業終了日時
                L0001row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
                L0001row("NACGESSTDATE") = "1950/01/01"                                    '実績・下車作業開始日時
                L0001row("NACGESENDDATE") = "1950/01/01"                                   '実績・下車作業終了日時
                L0001row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）
                L0001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                L0001row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
                L0001row("NACOUTWORKTIME") = 0                                             '実績・就業外時間
                L0001row("NACBREAKSTDATE") = "1950/01/01"                                  '実績・休憩開始日時
                L0001row("NACBREAKENDDATE") = "1950/01/01"                                 '実績・休憩終了日時
                L0001row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
                L0001row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
                L0001row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
                L0001row("NACCASH") = 0                                                    '実績・現金
                L0001row("NACETC") = 0                                                     '実績・ETC
                L0001row("NACTICKET") = 0                                                  '実績・回数券
                L0001row("NACKYUYU") = 0                                                   '実績・軽油
                L0001row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
                L0001row("NACCHOUNLOADCNT") = 0                                            '実績・荷卸回数調整
                L0001row("NACTTLUNLOADCNT") = 0                                            '実績・荷卸回数合計Σ
                L0001row("NACKAIJI") = 0                                                   '実績・回次
                L0001row("NACJITIME") = 0                                                  '実績・実車時間（分）
                L0001row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
                L0001row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
                L0001row("NACKUTIME") = 0                                                  '実績・空車時間（分）
                L0001row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
                L0001row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
                L0001row("NACJIDISTANCE") = 0                                              '実績・実車距離
                L0001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L0001row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
                L0001row("NACKUDISTANCE") = 0                                              '実績・空車距離
                L0001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L0001row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
                L0001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L0001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L0001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L0001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
                L0001row("NACOFFICESORG") = T0007HEADrow("SORG")                           '実績・作業部署
                L0001row("NACOFFICETIME") = HHMMtoMinutes(T0007HEADrow("WORKTIME"))        '実績・事務時間
                L0001row("NACOFFICEBREAKTIME") = HHMMtoMinutes(T0007HEADrow("BREAKTIMETTL")) '実績・事務休憩時間
                L0001row("PAYSHUSHADATE") = T0007HEADrow("STDATE") & " " & T0007HEADrow("STTIME")  '出社日時
                L0001row("PAYTAISHADATE") = T0007HEADrow("ENDDATE") & " " & T0007HEADrow("ENDTIME") '退社日時
                L0001row("PAYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コード
                L0001row("PAYSTAFFKBN") = T0007HEADrow("STAFFKBN")                         '社員区分
                L0001row("PAYMORG") = T0007HEADrow("MORG")                                 '従業員管理部署
                L0001row("PAYHORG") = T0007HEADrow("HORG")                                 '従業員配属部署
                L0001row("PAYHOLIDAYKBN") = T0007HEADrow("HOLIDAYKBN")                     '休日区分
                L0001row("PAYKBN") = T0007HEADrow("PAYKBN")                                '勤怠区分
                L0001row("PAYSHUKCHOKKBN") = T0007HEADrow("SHUKCHOKKBN")                   '宿日直区分
                L0001row("PAYJYOMUKBN") = "3"                                              '乗務区分
                L0001row("PAYOILKBN") = ""                                                 '勤怠用油種区分
                L0001row("PAYSHARYOKBN") = ""                                              '勤怠用車両区分
                If T0007HEADrow("HOLIDAYKBN") = "0" Then
                    L0001row("PAYWORKNISSU") = 1                                           '所労
                Else
                    L0001row("PAYWORKNISSU") = 0                                           '所労
                End If
                L0001row("PAYSHOUKETUNISSU") = T0007HEADrow("SHOUKETUNISSUTTL")            '傷欠
                L0001row("PAYKUMIKETUNISSU") = T0007HEADrow("KUMIKETUNISSUTTL")            '組欠
                L0001row("PAYETCKETUNISSU") = T0007HEADrow("ETCKETUNISSUTTL")              '他欠
                L0001row("PAYNENKYUNISSU") = T0007HEADrow("NENKYUNISSUTTL")                '年休
                L0001row("PAYTOKUKYUNISSU") = T0007HEADrow("TOKUKYUNISSUTTL")              '特休
                L0001row("PAYCHIKOKSOTAINISSU") = T0007HEADrow("CHIKOKSOTAINISSUTTL")      '遅早
                L0001row("PAYSTOCKNISSU") = T0007HEADrow("STOCKNISSUTTL")                  'ストック休暇
                L0001row("PAYKYOTEIWEEKNISSU") = T0007HEADrow("KYOTEIWEEKNISSUTTL")        '協定週休
                L0001row("PAYWEEKNISSU") = T0007HEADrow("WEEKNISSUTTL")                    '週休
                L0001row("PAYDAIKYUNISSU") = T0007HEADrow("DAIKYUNISSUTTL")                '代休
                L0001row("PAYWORKTIME") = HHMMtoMinutes(T0007HEADrow("BINDTIME"))          '所定労働時間（分）
                L0001row("PAYWWORKTIME") = HHMMtoMinutes(T0007HEADrow("WWORKTIMETTL"))     '所定内時間（分）
                L0001row("PAYNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("NIGHTTIMETTL"))     '所定深夜時間（分）
                L0001row("PAYORVERTIME") = HHMMtoMinutes(T0007HEADrow("ORVERTIMETTL"))     '平日残業時間（分）
                L0001row("PAYWNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("WNIGHTTIMETTL"))   '平日深夜時間（分）
                L0001row("PAYWSWORKTIME") = HHMMtoMinutes(T0007HEADrow("SWORKTIMETTL"))    '日曜出勤時間（分）
                L0001row("PAYSNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("SNIGHTTIMETTL"))   '日曜深夜時間（分）
                L0001row("PAYSDAIWORKTIME") = HHMMtoMinutes(T0007HEADrow("SDAIWORKTIMETTL"))    '日曜代休出勤時間（分）
                L0001row("PAYSDAINIGHTTIME") = HHMMtoMinutes(T0007HEADrow("SDAINIGHTTIMETTL"))   '日曜代休深夜時間（分）
                L0001row("PAYHWORKTIME") = HHMMtoMinutes(T0007HEADrow("HWORKTIMETTL"))     '休日出勤時間（分）
                L0001row("PAYHNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("HNIGHTTIMETTL"))   '休日深夜時間（分）
                L0001row("PAYHDAIWORKTIME") = HHMMtoMinutes(T0007HEADrow("HDAIWORKTIMETTL"))     '休日代休出勤時間（分）
                L0001row("PAYHDAINIGHTTIME") = HHMMtoMinutes(T0007HEADrow("HDAINIGHTTIMETTL"))   '休日代休深夜時間（分）
                L0001row("PAYBREAKTIME") = HHMMtoMinutes(T0007HEADrow("BREAKTIMETTL"))     '休憩時間（分）

                L0001row("PAYNENSHINISSU") = T0007HEADrow("NENSHINISSUTTL")                '年始出勤
                L0001row("PAYNENMATUNISSU") = T0007HEADrow("NENMATUNISSUTTL")              '年末出勤
                L0001row("PAYSHUKCHOKNNISSU") = T0007HEADrow("SHUKCHOKNNISSUTTL")          '宿日直年始
                L0001row("PAYSHUKCHOKNISSU") = T0007HEADrow("SHUKCHOKNISSUTTL")            '宿日直通常
                L0001row("PAYSHUKCHOKNHLDNISSU") = T0007HEADrow("SHUKCHOKNHLDNISSUTTL")    '宿日直年始（翌日休み）
                L0001row("PAYSHUKCHOKHLDNISSU") = T0007HEADrow("SHUKCHOKHLDNISSUTTL")      '宿日直通常（翌日休み）
                L0001row("PAYTOKSAAKAISU") = T0007HEADrow("TOKSAAKAISUTTL")                '特作A
                L0001row("PAYTOKSABKAISU") = T0007HEADrow("TOKSABKAISUTTL")                '特作B
                L0001row("PAYTOKSACKAISU") = T0007HEADrow("TOKSACKAISUTTL")                '特作C
                L0001row("PAYTENKOKAISU") = T0007HEADrow("TENKOKAISUTTL")                  '点呼回数
                L0001row("PAYHOANTIME") = HHMMtoMinutes(T0007HEADrow("HOANTIMETTL"))       '保安検査入力（分）
                L0001row("PAYKOATUTIME") = HHMMtoMinutes(T0007HEADrow("KOATUTIMETTL"))     '高圧作業入力（分）
                L0001row("PAYTOKUSA1TIME") = HHMMtoMinutes(T0007HEADrow("TOKUSA1TIMETTL")) '特作Ⅰ（分）
                L0001row("PAYPONPNISSU") = 0                                               'ポンプ
                L0001row("PAYBULKNISSU") = 0                                               'バルク
                L0001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                L0001row("PAYBKINMUKAISU") = 0                                             'B勤務

                L0001row("PAYYENDTIME") = 0                                                '予定終了時間
                L0001row("PAYAPPLYID") = ""                                                '申請ID
                L0001row("PAYRIYU") = ""                                                   '理由
                L0001row("PAYRIYUETC") = ""                                                '理由(その他）
                L0001row("PAYHAYADETIME") = 0                                              '早出補填時間
                L0001row("PAYHAISOTIME") = 0                                               '配送時間
                L0001row("PAYSHACHUHAKNISSU") = 0                                          '車中泊日数
                L0001row("PAYMODELDISTANCE") = 0                                           'モデル距離
                L0001row("PAYJIKYUSHATIME") = 0                                            '時給者時間
                L0001row("PAYJYOMUTIME") = 0                                               '乗務時間
                L0001row("PAYHWORKNISSU") = 0                                              '休日出勤日数
                L0001row("PAYKAITENCNT") = 0                                               '回転数
                L0001row("PAYSENJYOCNT") = 0                                               '洗浄回数
                L0001row("PAYUNLOADADDCNT1") = 0                                           '危険物荷卸回数1
                L0001row("PAYUNLOADADDCNT2") = 0                                           '危険物荷卸回数2
                L0001row("PAYUNLOADADDCNT3") = 0                                           '危険物荷卸回数3
                L0001row("PAYUNLOADADDCNT4") = 0                                           '危険物荷卸回数4
                L0001row("PAYSHORTDISTANCE1") = 0                                          '短距離手当1
                L0001row("PAYSHORTDISTANCE2") = 0                                          '短距離手当2

                L0001row("APPKIJUN") = ""                                                  '配賦基準
                L0001row("APPKEY") = ""                                                    '配賦統計キー

                L0001row("WORKKBN") = T0007HEADrow("WORKKBN")                              '作業区分
                L0001row("KEYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コードキー
                L0001row("KEYGSHABAN") = ""                                                '業務車番キー
                L0001row("KEYTRIPNO") = ""                                                 'トリップキー
                L0001row("KEYDROPNO") = ""                                                 'ドロップキー

                L0001row("DELFLG") = "0"                                                   '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                          '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L0001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L0001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L0001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                            '伝票タイプ

                CS0038ACCODEget.TORICODE = L0001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L0001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L0001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L0001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L0001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L0001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L0001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L0001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L0001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L0001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L0001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L0001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L0001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L0001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L0001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L0001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L0001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "JMD"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "JMC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow
                '------------------------------------------------------
                '削除データ
                '------------------------------------------------------
                'If T0007HEADrow("DELFLG") = "1" Then
                '    '●借方
                '    L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                '    L0001row("INQKBN") = "1"                                         '照会区分
                '    L0001row("ACDCKBN") = "D"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "JMD"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "01"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（作業部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)

                '    '●貸方
                '    L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                '    L0001row("INQKBN") = "0"                                         '照会区分
                '    L0001row("ACDCKBN") = "C"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "JMC"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "02"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)
                'End If

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                If T0007HEADrow("DELFLG") = "0" Then
                    '●借方
                    If WW_INQKBN_D = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                        L0001row("ACDCKBN") = "D"                                        '貸借区分
                        L0001row("ACACHANTEI") = "JMD"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "01"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（作業部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If

                    '●貸方
                    If WW_INQKBN_C = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                        L0001row("ACDCKBN") = "C"                                        '貸借区分
                        L0001row("ACACHANTEI") = "JMC"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "02"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If
                End If
            Catch ex As Exception
                'ROWデータのCSV(tab)変換
                Dim WW_CSV As String = ""
                DatarowToCsv(WW_T0007tbl.Rows(i), WW_CSV)

                CS0011LOGWRITE.INFSUBCLASS = "L0001tblDailyTtlEdit"         'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "L0001tblDailyTtlEdit"             '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA=(" & WW_CSV & ")"
                CS0011LOGWRITE.MESSAGENO = "00001"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Throw

            End Try

        Next

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  L0001tbl編集（月合計・ジャーナル）
    Public Sub L0001tbMonthlylTtlEdit(ByVal I_USERID As String, ByRef I_T7tbl As DataTable, ByRef IO_L1tbl As DataTable, ByRef O_RTN As String)

        Dim WW_DATENOW As Date = Date.Now
        Dim WW_M0008tbl As New DataTable
        Dim WW_T0007tbl As New DataTable
        Dim T0007HEADrow As DataRow = Nothing
        Dim T0007DTLrow As DataRow = Nothing
        Dim L0001row As DataRow = Nothing
        O_RTN = C_MESSAGE_NO.NORMAL

        CS0026TblSort.TABLE = I_T7tbl
        CS0026TblSort.FILTER = "OPERATION = '更新' and RECODEKBN = '2'"
        CS0026TblSort.SORTING = "STAFFCODE, WORKDATE, RECODEKBN, HDKBN DESC, STDATE, STTIME, ENDDATE, ENDTIME"
        WW_T0007tbl = CS0026TblSort.sort()

        For i As Integer = 0 To WW_T0007tbl.Rows.Count - 1
            Try

                'ヘッダレコードをキープ
                If WW_T0007tbl.Rows(i)("HDKBN") = "H" Then
                    T0007HEADrow = WW_T0007tbl.Rows(i)
                    Continue For
                End If

                If WW_T0007tbl.Rows(i)("HDKBN") = "D" Then
                    T0007DTLrow = WW_T0007tbl.Rows(i)

                    If IsNothing(T0007HEADrow) Then
                        Continue For
                    End If
                End If

                L0001row = IO_L1tbl.NewRow

                Dim WW_SEQ As String = "000000"

                '伝票番号採番
                CS0033AutoNumber.CAMPCODE = T0007HEADrow("CAMPCODE")
                CS0033AutoNumber.MORG = T0007HEADrow("HORG")
                CS0033AutoNumber.SEQTYPE = CS0033AutoNumber.C_SEQTYPE.DENNO
                CS0033AutoNumber.USERID = I_USERID
                CS0033AutoNumber.getAutoNumber()
                If CS0033AutoNumber.ERR = C_MESSAGE_NO.NORMAL Then
                    WW_SEQ = CS0033AutoNumber.SEQ
                Else
                    CS0011LOGWRITE.INFSUBCLASS = "L0001tbMonthlylTtlEdit"       'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "L0001tbMonthlylTtlEdit"           '
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                    CS0011LOGWRITE.TEXT = "採番エラー"
                    CS0011LOGWRITE.MESSAGENO = CS0033AutoNumber.ERR
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                    Exit Sub
                End If

                '---------------------------------------------------------
                'L1出力編集
                '---------------------------------------------------------
                L0001row("CAMPCODE") = T0007HEADrow("CAMPCODE")                              '会社コード
                L0001row("MOTOCHO") = "LO"                                                   '元帳（非会計予定を設定）
                L0001row("VERSION") = "000"                                                  'バージョン
                L0001row("DENTYPE") = "T07"                                                  '伝票タイプ
                L0001row("TENKI") = "0"                                                      '統計転記
                L0001row("KEIJOYMD") = T0007HEADrow("WORKDATE")                              '計上日付（勤務年月日を設定）
                L0001row("DENYMD") = T0007HEADrow("WORKDATE")                                '伝票日付（勤務年月日設定）
                '伝票番号
                Dim WW_DENNO As String = ""
                Try
                    WW_DENNO = CDate(T0007HEADrow("WORKDATE")).ToString("yyyy")
                Catch ex As Exception
                    WW_DENNO = Date.Now.ToString("yyyy")
                End Try
                L0001row("DENNO") = T0007HEADrow("HORG") &
                                    WW_DENNO &
                                    WW_SEQ
                '関連伝票No＋明細No
                L0001row("KANRENDENNO") = T0007HEADrow("HORG") & " " _
                              & T0007HEADrow("STAFFCODE") & " " _
                              & T0007HEADrow("WORKDATE") & " "

                L0001row("ACTORICODE") = ""                                                 '取引先コード
                L0001row("ACOILTYPE") = ""                                                  '油種
                L0001row("ACSHARYOTYPE") = ""                                               '統一車番(上)
                L0001row("ACTSHABAN") = ""                                                  '統一車番(下)
                L0001row("ACSTAFFCODE") = ""                                                '従業員コード
                L0001row("ACBANKAC") = ""                                                   '銀行口座

                '端末マスタより管理部署を取得
                CS0006TERMchk.TERMID = CS0050Session.APSV_ID
                CS0006TERMchk.CS0006TERMchk()
                If CS0006TERMchk.ERR = C_MESSAGE_NO.NORMAL Then
                    L0001row("ACKEIJOMORG") = CS0006TERMchk.MORG                          '計上管理部署コード(部・支店）)
                Else
                    L0001row("ACKEIJOMORG") = T0007HEADrow("MORG")                        '計上管理部署コード（管理部署）
                End If

                L0001row("ACTAXKBN") = 0                                                 '税区分
                L0001row("ACAMT") = 0                                                    '金額
                L0001row("NACSHUKODATE") = T0007HEADrow("WORKDATE")                      '勤務日
                L0001row("NACSHUKADATE") = "1950/01/01"                                  '出荷日
                L0001row("NACTODOKEDATE") = "1950/01/01"                                 '届日
                L0001row("NACTORICODE") = ""                                             '荷主コード
                L0001row("NACURIKBN") = ""                                               '売上計上基準
                L0001row("NACTODOKECODE") = ""                                           '届先コード
                L0001row("NACSTORICODE") = ""                                            '販売店コード
                L0001row("NACSHUKABASHO") = ""                                           '出荷場所

                L0001row("NACTORITYPE01") = ""                                          '取引先・取引タイプ01
                L0001row("NACTORITYPE02") = ""                                          '取引先・取引タイプ02
                L0001row("NACTORITYPE03") = ""                                          '取引先・取引タイプ03
                L0001row("NACTORITYPE04") = ""                                          '取引先・取引タイプ04
                L0001row("NACTORITYPE05") = ""                                          '取引先・取引タイプ05

                L0001row("NACOILTYPE") = ""                                             '油種
                L0001row("NACPRODUCT1") = ""                                            '品名１
                L0001row("NACPRODUCT2") = ""                                            '品名２

                L0001row("NACGSHABAN") = ""                                             '業務車番

                L0001row("NACSUPPLIERKBN") = ""                                         '社有・庸車区分
                L0001row("NACSUPPLIER") = ""                                            '庸車会社

                L0001row("NACSHARYOOILTYPE") = ""                                       '車両登録油種

                L0001row("NACSHARYOTYPE1") = ""                                         '統一車番(上)1
                L0001row("NACTSHABAN1") = ""                                            '統一車番(下)1
                L0001row("NACMANGMORG1") = ""                                           '車両管理部署1
                L0001row("NACMANGSORG1") = ""                                           '車両設置部署1
                L0001row("NACMANGUORG1") = ""                                           '車両運用部署1
                L0001row("NACBASELEASE1") = ""                                          '車両所有1

                L0001row("NACSHARYOTYPE2") = ""                                         '統一車番(上)2
                L0001row("NACTSHABAN2") = ""                                            '統一車番(下)2
                L0001row("NACMANGMORG2") = ""                                           '車両管理部署2
                L0001row("NACMANGSORG2") = ""                                           '車両設置部署2
                L0001row("NACMANGUORG2") = ""                                           '車両運用部署1
                L0001row("NACBASELEASE2") = ""                                          '車両所有2

                L0001row("NACSHARYOTYPE3") = ""                                         '統一車番(上)3
                L0001row("NACTSHABAN3") = ""                                            '統一車番(下)3
                L0001row("NACMANGMORG3") = ""                                           '車両管理部署3
                L0001row("NACMANGSORG3") = ""                                           '車両設置部署3
                L0001row("NACMANGUORG3") = ""                                           '車両運用部署1
                L0001row("NACBASELEASE3") = ""                                          '車両所有3

                L0001row("NACCREWKBN") = ""                                             '正副区分
                L0001row("NACSTAFFCODE") = ""                                           '従業員コード（正）

                L0001row("NACSTAFFKBN") = ""                                            '社員区分（正）
                L0001row("NACMORG") = ""                                                '管理部署（正）
                L0001row("NACHORG") = ""                                                '配属部署（正）
                L0001row("NACSORG") = ""                                                '作業部署（正）

                L0001row("NACSTAFFCODE2") = ""                                          '従業員コード（副）

                L0001row("NACSTAFFKBN2") = ""                                           '社員区分（副）
                L0001row("NACMORG2") = ""                                               '管理部署（副）
                L0001row("NACHORG2") = ""                                               '配属部署（副）
                L0001row("NACSORG2") = ""                                               '作業部署（副）

                L0001row("NACORDERNO") = ""                                             '受注番号
                L0001row("NACDETAILNO") = ""                                            '明細№
                L0001row("NACTRIPNO") = ""                                              'トリップ
                L0001row("NACDROPNO") = ""                                              'ドロップ
                L0001row("NACSEQ") = ""                                                 'SEQ

                L0001row("NACORDERORG") = ""                                            '受注部署
                L0001row("NACSHIPORG") = ""                                             '配送部署
                L0001row("NACSURYO") = 0                                                '受注・数量
                L0001row("NACTANI") = ""                                                '受注・単位
                L0001row("NACJSURYO") = 0                                                  '実績・配送数量
                L0001row("NACSTANI") = ""                                                  '実績・配送単位
                L0001row("NACHAIDISTANCE") = 0                                             '実績・配送距離
                L0001row("NACKAIDISTANCE") = 0                                             '実績・回送作業距離
                L0001row("NACCHODISTANCE") = T0007DTLrow("HAIDISTANCECHO")                 '実績・勤怠調整距離
                L0001row("NACTTLDISTANCE") = T0007DTLrow("HAIDISTANCECHO")                 '実績・配送距離合計Σ
                L0001row("NACHAISTDATE") = "1950/01/01"                                    '実績・配送作業開始日時
                L0001row("NACHAIENDDATE") = "1950/01/01"                                   '実績・配送作業終了日時
                L0001row("NACHAIWORKTIME") = 0                                             '実績・配送作業時間（分）
                L0001row("NACGESSTDATE") = "1950/01/01"                                    '実績・下車作業開始日時
                L0001row("NACGESENDDATE") = "1950/01/01"                                   '実績・下車作業終了日時
                L0001row("NACGESWORKTIME") = 0                                             '実績・下車作業時間（分）

                Dim WW_NIGHTTIME As Integer = 0                                               '所定深夜時間（分）
                Dim WW_ORVERTIME As Integer = 0                                               '平日残業時間（分）
                Dim WW_WNIGHTTIME As Integer = 0                                              '平日深夜時間（分）
                Dim WW_WSWORKTIME As Integer = 0                                              '日曜出勤時間（分）
                Dim WW_SNIGHTTIME As Integer = 0                                              '日曜深夜時間（分）
                Dim WW_SDAIWORKTIME As Integer = 0                                            '日曜代休出勤時間（分）
                Dim WW_SDAINIGHTTIME As Integer = 0                                           '日曜代休深夜時間（分）
                Dim WW_HWORKTIME As Integer = 0                                               '休日出勤時間（分）
                Dim WW_HNIGHTTIME As Integer = 0                                              '休日深夜時間（分）
                Dim WW_HDAIWORKTIME As Integer = 0                                            '休日代休出勤時間（分）
                Dim WW_HDAINIGHTTIME As Integer = 0                                           '休日代休深夜時間（分）

                WW_NIGHTTIME = HHMMtoMinutes(T0007HEADrow("NIGHTTIMECHO"))                '所定深夜時間（分）
                WW_ORVERTIME = HHMMtoMinutes(T0007HEADrow("ORVERTIMECHO"))                '平日残業時間（分）
                WW_WNIGHTTIME = HHMMtoMinutes(T0007HEADrow("WNIGHTTIMECHO"))              '平日深夜時間（分）
                WW_WSWORKTIME = HHMMtoMinutes(T0007HEADrow("SWORKTIMECHO"))               '日曜出勤時間（分）
                WW_SNIGHTTIME = HHMMtoMinutes(T0007HEADrow("SNIGHTTIMECHO"))              '日曜深夜時間（分）
                WW_SDAIWORKTIME = HHMMtoMinutes(T0007HEADrow("SDAIWORKTIMECHO"))          '日曜代休出勤時間（分）
                WW_SDAINIGHTTIME = HHMMtoMinutes(T0007HEADrow("SDAINIGHTTIMECHO"))        '日曜代休深夜時間（分）
                WW_HWORKTIME = HHMMtoMinutes(T0007HEADrow("HWORKTIMECHO"))                '休日出勤時間（分）
                WW_HNIGHTTIME = HHMMtoMinutes(T0007HEADrow("HNIGHTTIMECHO"))              '休日深夜時間（分）
                WW_HDAIWORKTIME = HHMMtoMinutes(T0007HEADrow("HDAIWORKTIMECHO"))          '休日代休出勤時間（分）
                WW_HDAINIGHTTIME = HHMMtoMinutes(T0007HEADrow("HDAINIGHTTIMECHO"))        '休日代休深夜時間（分）

                '実績・勤怠調整時間（分）
                If IsDBNull(T0007DTLrow("SHARYOKBN")) Then
                    T0007DTLrow("SHARYOKBN") = ""
                End If
                If IsDBNull(T0007DTLrow("OILPAYKBN")) Then
                    T0007DTLrow("OILPAYKBN") = ""
                End If
                If T0007DTLrow("SHARYOKBN") = "1" And T0007DTLrow("OILPAYKBN") = "01" Then
                    L0001row("NACCHOWORKTIME") =
                        WW_NIGHTTIME + WW_ORVERTIME + WW_WNIGHTTIME + WW_WSWORKTIME + WW_SNIGHTTIME + WW_SDAIWORKTIME + WW_SDAINIGHTTIME + WW_HWORKTIME + WW_HNIGHTTIME + WW_HDAIWORKTIME + WW_HDAINIGHTTIME
                    L0001row("NACTTLWORKTIME") = L0001row("NACCHOWORKTIME")                    '実績・配送合計時間Σ（分）
                Else
                    L0001row("NACCHOWORKTIME") = 0                                             '実績・勤怠調整時間（分）
                    L0001row("NACTTLWORKTIME") = 0                                             '実績・配送合計時間Σ（分）
                End If
                L0001row("NACOUTWORKTIME") = 0                                             '実績・就業外時間
                L0001row("NACBREAKSTDATE") = "1950/01/01"                                  '実績・休憩開始日時
                L0001row("NACBREAKENDDATE") = "1950/01/01"                                 '実績・休憩終了日時
                L0001row("NACBREAKTIME") = 0                                               '実績・休憩時間（分）
                L0001row("NACCHOBREAKTIME") = 0                                            '実績・休憩調整時間（分）
                L0001row("NACTTLBREAKTIME") = 0                                            '実績・休憩合計時間Σ（分）
                L0001row("NACCASH") = 0                                                    '実績・現金
                L0001row("NACETC") = 0                                                     '実績・ETC
                L0001row("NACTICKET") = 0                                                  '実績・回数券
                L0001row("NACKYUYU") = 0                                                   '実績・軽油
                L0001row("NACUNLOADCNT") = 0                                               '実績・荷卸回数
                L0001row("NACCHOUNLOADCNT") = T0007DTLrow("UNLOADCNTCHO")                  '実績・荷卸回数調整
                L0001row("NACTTLUNLOADCNT") = T0007DTLrow("UNLOADCNTCHO")                  '実績・荷卸回数合計Σ
                L0001row("NACKAIJI") = 0                                                   '実績・回次
                L0001row("NACJITIME") = 0                                                  '実績・実車時間（分）
                L0001row("NACJICHOSTIME") = 0                                              '実績・実車時間調整（分）
                L0001row("NACJITTLETIME") = 0                                              '実績・実車時間合計Σ（分）
                L0001row("NACKUTIME") = 0                                                  '実績・空車時間（分）
                L0001row("NACKUCHOTIME") = 0                                               '実績・空車時間調整（分）
                L0001row("NACKUTTLTIME") = 0                                               '実績・空車時間合計Σ（分）
                L0001row("NACJIDISTANCE") = 0                                              '実績・実車距離
                L0001row("NACJICHODISTANCE") = 0                                           '実績・実車距離調整
                L0001row("NACJITTLDISTANCE") = 0                                           '実績・実車距離合計Σ
                L0001row("NACKUDISTANCE") = 0                                              '実績・空車距離
                L0001row("NACKUCHODISTANCE") = 0                                           '実績・空車距離調整
                L0001row("NACKUTTLDISTANCE") = 0                                           '実績・空車距離合計Σ
                L0001row("NACTARIFFFARE") = 0                                              '実績・運賃タリフ額
                L0001row("NACFIXEDFARE") = 0                                               '実績・運賃固定額
                L0001row("NACINCHOFARE") = 0                                               '実績・運賃手入力調整額
                L0001row("NACTTLFARE") = 0                                                 '実績・運賃合計額Σ
                L0001row("NACOFFICESORG") = T0007HEADrow("SORG")                           '実績・作業部署
                If T0007DTLrow("SHARYOKBN") = "1" And T0007DTLrow("OILPAYKBN") = "01" Then
                    If T0007HEADrow("STAFFKBN") Like "03*" Then
                        L0001row("NACOFFICETIME") = 0                                          '実績・事務時間
                    Else
                        L0001row("NACOFFICETIME") = HHMMtoMinutes(T0007HEADrow("WORKTIME"))    '実績・事務時間
                    End If
                Else
                    L0001row("NACOFFICETIME") = 0                                              '実績・事務時間
                End If
                L0001row("NACOFFICEBREAKTIME") = 0                                         '実績・事務休憩時間
                L0001row("PAYSHUSHADATE") = "1950/01/01"                                   '出社日時
                L0001row("PAYTAISHADATE") = "1950/01/01"                                   '退社日時
                L0001row("PAYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コード
                L0001row("PAYSTAFFKBN") = T0007HEADrow("STAFFKBN")                         '社員区分
                L0001row("PAYMORG") = T0007HEADrow("MORG")                                 '従業員管理部署
                L0001row("PAYHORG") = T0007HEADrow("HORG")                                 '従業員配属部署
                L0001row("PAYHOLIDAYKBN") = T0007HEADrow("HOLIDAYKBN")                     '休日区分
                L0001row("PAYKBN") = T0007HEADrow("PAYKBN")                                '勤怠区分
                L0001row("PAYSHUKCHOKKBN") = T0007HEADrow("SHUKCHOKKBN")                   '宿日直区分
                L0001row("PAYJYOMUKBN") = "3"                                              '乗務区分

                L0001row("PAYOILKBN") = T0007DTLrow("OILPAYKBN")                           '勤怠用油種区分
                L0001row("PAYSHARYOKBN") = T0007DTLrow("SHARYOKBN")                        '勤怠用車両区分

                If T0007DTLrow("SHARYOKBN") = "1" And T0007DTLrow("OILPAYKBN") = "01" Then
                    L0001row("PAYWORKNISSU") = T0007HEADrow("WORKNISSUCHO")                    '所労
                    L0001row("PAYSHOUKETUNISSU") = T0007HEADrow("SHOUKETUNISSUCHO")            '傷欠
                    L0001row("PAYKUMIKETUNISSU") = T0007HEADrow("KUMIKETUNISSUCHO")            '組欠
                    L0001row("PAYETCKETUNISSU") = T0007HEADrow("ETCKETUNISSUCHO")              '他欠
                    L0001row("PAYNENKYUNISSU") = T0007HEADrow("NENKYUNISSUCHO")                '年休
                    L0001row("PAYTOKUKYUNISSU") = T0007HEADrow("TOKUKYUNISSUCHO")              '特休
                    L0001row("PAYCHIKOKSOTAINISSU") = T0007HEADrow("CHIKOKSOTAINISSUCHO")      '遅早
                    L0001row("PAYSTOCKNISSU") = T0007HEADrow("STOCKNISSUCHO")                  'ストック休暇
                    L0001row("PAYKYOTEIWEEKNISSU") = T0007HEADrow("KYOTEIWEEKNISSUCHO")        '協定週休
                    L0001row("PAYWEEKNISSU") = T0007HEADrow("WEEKNISSUCHO")                    '週休
                    L0001row("PAYDAIKYUNISSU") = T0007HEADrow("DAIKYUNISSUCHO")                '代休
                    L0001row("PAYWORKTIME") = HHMMtoMinutes(T0007HEADrow("BINDTIME"))          '所定労働時間（分）
                    L0001row("PAYWWORKTIME") = HHMMtoMinutes(T0007HEADrow("WWORKTIMETTL"))     '所定内時間（分）
                    L0001row("PAYNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("NIGHTTIMECHO"))     '所定深夜時間（分）
                    L0001row("PAYORVERTIME") = HHMMtoMinutes(T0007HEADrow("ORVERTIMECHO"))     '平日残業時間（分）
                    L0001row("PAYWNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("WNIGHTTIMECHO"))   '平日深夜時間（分）
                    L0001row("PAYWSWORKTIME") = HHMMtoMinutes(T0007HEADrow("SWORKTIMECHO"))    '日曜出勤時間（分）
                    L0001row("PAYSNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("SNIGHTTIMECHO"))   '日曜深夜時間（分）
                    L0001row("PAYSDAIWORKTIME") = HHMMtoMinutes(T0007HEADrow("SDAIWORKTIMETTL"))    '日曜出勤時間（分）
                    L0001row("PAYSDAINIGHTTIME") = HHMMtoMinutes(T0007HEADrow("SDAINIGHTTIMETTL"))  '日曜深夜時間（分）
                    L0001row("PAYHWORKTIME") = HHMMtoMinutes(T0007HEADrow("HWORKTIMECHO"))     '休日出勤時間（分）
                    L0001row("PAYHNIGHTTIME") = HHMMtoMinutes(T0007HEADrow("HNIGHTTIMECHO"))   '休日深夜時間（分）
                    L0001row("PAYHDAIWORKTIME") = HHMMtoMinutes(T0007HEADrow("HDAIWORKTIMETTL"))     '休日代休出勤時間（分）
                    L0001row("PAYHDAINIGHTTIME") = HHMMtoMinutes(T0007HEADrow("HDAINIGHTTIMETTL"))   '休日代休深夜時間（分）
                    L0001row("PAYBREAKTIME") = HHMMtoMinutes(T0007HEADrow("BREAKTIMECHO"))     '休憩時間（分）

                    L0001row("PAYNENSHINISSU") = T0007HEADrow("NENSHINISSUCHO")                '年始出勤
                    L0001row("PAYNENMATUNISSU") = T0007HEADrow("NENMATUNISSUTTL")              '年末出勤
                    L0001row("PAYSHUKCHOKNNISSU") = T0007HEADrow("SHUKCHOKNNISSUCHO")          '宿日直年始
                    L0001row("PAYSHUKCHOKNISSU") = T0007HEADrow("SHUKCHOKNISSUCHO")            '宿日直通常
                    L0001row("PAYSHUKCHOKNHLDNISSU") = T0007HEADrow("SHUKCHOKNHLDNISSUCHO")    '宿日直年始（翌日休み）
                    L0001row("PAYSHUKCHOKHLDNISSU") = T0007HEADrow("SHUKCHOKHLDNISSUCHO")      '宿日直通常（翌日休み）
                    L0001row("PAYTOKSAAKAISU") = T0007HEADrow("TOKSAAKAISUCHO")                '特作A
                    L0001row("PAYTOKSABKAISU") = T0007HEADrow("TOKSABKAISUCHO")                '特作B
                    L0001row("PAYTOKSACKAISU") = T0007HEADrow("TOKSACKAISUCHO")                '特作C
                    L0001row("PAYTENKOKAISU") = T0007HEADrow("TENKOKAISUCHO")                  '点呼回数
                    L0001row("PAYHOANTIME") = HHMMtoMinutes(T0007HEADrow("HOANTIMECHO"))       '保安検査入力（分）
                    L0001row("PAYKOATUTIME") = HHMMtoMinutes(T0007HEADrow("KOATUTIMECHO"))     '高圧作業入力（分）
                    L0001row("PAYTOKUSA1TIME") = HHMMtoMinutes(T0007HEADrow("TOKUSA1TIMECHO")) '特作Ⅰ（分）
                    L0001row("PAYPONPNISSU") = T0007HEADrow("PONPNISSUCHO")                    'ポンプ
                    L0001row("PAYBULKNISSU") = T0007HEADrow("BULKNISSUCHO")                    'バルク
                    L0001row("PAYTRAILERNISSU") = T0007HEADrow("TRAILERNISSUCHO")              'トレーラ
                    L0001row("PAYBKINMUKAISU") = T0007HEADrow("BKINMUKAISUCHO")                'B勤務
                    L0001row("PAYHAYADETIME") = HHMMtoMinutes(T0007HEADrow("HAYADETIMETTL"))     '早出補填時間
                    L0001row("PAYHAISOTIME") = HHMMtoMinutes(T0007HEADrow("HAISOTIME"))          '配送時間
                    L0001row("PAYSHACHUHAKNISSU") = Val(T0007HEADrow("SHACHUHAKNISSUTTL"))                '車中泊日数
                    L0001row("PAYMODELDISTANCE") = T0007HEADrow("MODELDISTANCETTL")                       'モデル距離
                    L0001row("PAYJIKYUSHATIME") = HHMMtoMinutes(T0007HEADrow("JIKYUSHATIMETTL")) '時給者時間
                    L0001row("PAYJYOMUTIME") = HHMMtoMinutes(T0007HEADrow("JYOMUTIMETTL"))       '乗務時間
                    L0001row("PAYHWORKNISSU") = T0007HEADrow("HWORKNISSUTTL")                             '休日出勤日数
                    L0001row("PAYKAITENCNT") = T0007HEADrow("KAITENCNTTTL")                               '回転数
                    L0001row("PAYSENJYOCNT") = T0007HEADrow("SENJYOCNTTTL")                               '洗浄回数
                    L0001row("PAYUNLOADADDCNT1") = T0007HEADrow("UNLOADADDCNT1TTL")                       '危険物荷卸回数1
                    L0001row("PAYUNLOADADDCNT2") = T0007HEADrow("UNLOADADDCNT2TTL")                       '危険物荷卸回数2
                    L0001row("PAYUNLOADADDCNT3") = T0007HEADrow("UNLOADADDCNT3TTL")                       '危険物荷卸回数3
                    L0001row("PAYUNLOADADDCNT4") = T0007HEADrow("UNLOADADDCNT4TTL")                       '危険物荷卸回数4
                    L0001row("PAYSHORTDISTANCE1") = T0007HEADrow("SHORTDISTANCE1TTL")                     '短距離手当1
                    L0001row("PAYSHORTDISTANCE2") = T0007HEADrow("SHORTDISTANCE2TTL")                     '短距離手当2
                Else
                    L0001row("PAYWORKNISSU") = 0                                               '所労
                    L0001row("PAYSHOUKETUNISSU") = 0                                           '傷欠
                    L0001row("PAYKUMIKETUNISSU") = 0                                           '組欠
                    L0001row("PAYETCKETUNISSU") = 0                                            '他欠
                    L0001row("PAYNENKYUNISSU") = 0                                             '年休
                    L0001row("PAYTOKUKYUNISSU") = 0                                            '特休
                    L0001row("PAYCHIKOKSOTAINISSU") = 0                                        '遅早
                    L0001row("PAYSTOCKNISSU") = 0                                              'ストック休暇
                    L0001row("PAYKYOTEIWEEKNISSU") = 0                                         '協定週休
                    L0001row("PAYWEEKNISSU") = 0                                               '週休
                    L0001row("PAYDAIKYUNISSU") = 0                                             '代休
                    L0001row("PAYWORKTIME") = 0                                                '所定労働時間（分）
                    L0001row("PAYWWORKTIME") = 0                                               '所定内時間（分）
                    L0001row("PAYNIGHTTIME") = 0                                               '所定深夜時間（分）
                    L0001row("PAYORVERTIME") = 0                                               '平日残業時間（分）
                    L0001row("PAYWNIGHTTIME") = 0                                              '平日深夜時間（分）
                    L0001row("PAYWSWORKTIME") = 0                                              '日曜出勤時間（分）
                    L0001row("PAYSNIGHTTIME") = 0                                              '日曜深夜時間（分）
                    L0001row("PAYSDAIWORKTIME") = 0                                            '日曜代休出勤時間（分）
                    L0001row("PAYSDAINIGHTTIME") = 0                                           '日曜代休深夜時間（分）
                    L0001row("PAYHWORKTIME") = 0                                               '休日出勤時間（分）
                    L0001row("PAYHNIGHTTIME") = 0                                              '休日深夜時間（分）
                    L0001row("PAYHDAIWORKTIME") = 0                                            '休日代休出勤時間（分）
                    L0001row("PAYHDAINIGHTTIME") = 0                                           '休日代休深夜時間（分）
                    L0001row("PAYBREAKTIME") = 0                                               '休憩時間（分）

                    L0001row("PAYNENSHINISSU") = 0                                             '年始出勤
                    L0001row("PAYNENMATUNISSU") = 0                                            '年末出勤
                    L0001row("PAYSHUKCHOKNNISSU") = 0                                          '宿日直年始
                    L0001row("PAYSHUKCHOKNISSU") = 0                                           '宿日直通常
                    L0001row("PAYSHUKCHOKNHLDNISSU") = 0                                       '宿日直年始（翌日休み）
                    L0001row("PAYSHUKCHOKHLDNISSU") = 0                                        '宿日直通常（翌日休み）
                    L0001row("PAYTOKSAAKAISU") = 0                                             '特作A
                    L0001row("PAYTOKSABKAISU") = 0                                             '特作B
                    L0001row("PAYTOKSACKAISU") = 0                                             '特作C
                    L0001row("PAYTENKOKAISU") = 0                                              '点呼回数
                    L0001row("PAYHOANTIME") = 0                                                '保安検査入力（分）
                    L0001row("PAYKOATUTIME") = 0                                               '高圧作業入力（分）
                    L0001row("PAYTOKUSA1TIME") = 0                                             '特作Ⅰ（分）
                    L0001row("PAYPONPNISSU") = 0                                               'ポンプ
                    L0001row("PAYBULKNISSU") = 0                                               'バルク
                    L0001row("PAYTRAILERNISSU") = 0                                            'トレーラ
                    L0001row("PAYBKINMUKAISU") = 0                                             'B勤務
                    L0001row("PAYHAYADETIME") = 0                                              '早出補填時間
                    L0001row("PAYHAISOTIME") = 0                                               '配送時間
                    L0001row("PAYSHACHUHAKNISSU") = 0                                          '車中泊日数
                    L0001row("PAYMODELDISTANCE") = 0                                           'モデル距離
                    L0001row("PAYJIKYUSHATIME") = 0                                            '時給者時間
                    L0001row("PAYJYOMUTIME") = 0                                               '乗務時間
                    L0001row("PAYHWORKNISSU") = 0                                              '休日出勤日数
                    L0001row("PAYKAITENCNT") = 0                                               '回転数
                    L0001row("PAYSENJYOCNT") = 0                                               '洗浄回数
                    L0001row("PAYUNLOADADDCNT1") = 0                                           '危険物荷卸回数1
                    L0001row("PAYUNLOADADDCNT2") = 0                                           '危険物荷卸回数2
                    L0001row("PAYUNLOADADDCNT3") = 0                                           '危険物荷卸回数3
                    L0001row("PAYUNLOADADDCNT4") = 0                                           '危険物荷卸回数4
                    L0001row("PAYSHORTDISTANCE1") = 0                                          '短距離手当1
                    L0001row("PAYSHORTDISTANCE2") = 0                                          '短距離手当2
                End If
                L0001row("PAYYENDTIME") = 0                                                '予定終了時間
                L0001row("PAYAPPLYID") = ""                                                '申請ID
                L0001row("PAYRIYU") = ""                                                   '理由
                L0001row("PAYRIYUETC") = ""                                                '理由(その他）
                L0001row("APPKIJUN") = ""                                                  '配賦基準
                L0001row("APPKEY") = ""                                                    '配賦統計キー

                L0001row("WORKKBN") = T0007HEADrow("WORKKBN")                              '作業区分
                L0001row("KEYSTAFFCODE") = T0007HEADrow("STAFFCODE")                       '従業員コードキー
                L0001row("KEYGSHABAN") = ""                                                '業務車番キー
                L0001row("KEYTRIPNO") = ""                                                 'トリップキー
                L0001row("KEYDROPNO") = ""                                                 'ドロップキー

                L0001row("DELFLG") = "0"                                                   '削除フラグ

                '勘定科目判定テーブル検索（共通設定項目）
                CS0038ACCODEget.TBL = WW_M0008tbl                                          '勘定科目判定テーブル
                CS0038ACCODEget.CAMPCODE = L0001row("CAMPCODE")                            '会社コード
                CS0038ACCODEget.STYMD = L0001row("KEIJOYMD")                               '開始日
                CS0038ACCODEget.ENDYMD = L0001row("KEIJOYMD")                              '終了日
                CS0038ACCODEget.MOTOCHO = "LO"                                             '元帳
                CS0038ACCODEget.DENTYPE = "T07"                                            '伝票タイプ

                CS0038ACCODEget.TORICODE = L0001row("NACTORICODE")                         '荷主コード
                CS0038ACCODEget.TORITYPE01 = L0001row("NACTORITYPE01")                     '取引タイプ01
                CS0038ACCODEget.TORITYPE02 = L0001row("NACTORITYPE02")                     '取引タイプ02
                CS0038ACCODEget.TORITYPE03 = L0001row("NACTORITYPE03")                     '取引タイプ03
                CS0038ACCODEget.TORITYPE04 = L0001row("NACTORITYPE04")                     '取引タイプ04
                CS0038ACCODEget.TORITYPE05 = L0001row("NACTORITYPE05")                     '取引タイプ05
                CS0038ACCODEget.URIKBN = L0001row("NACURIKBN")                             '売上計上基準
                CS0038ACCODEget.STORICODE = L0001row("NACSTORICODE")                       '販売店コード
                CS0038ACCODEget.OILTYPE = L0001row("NACOILTYPE")                           '油種
                CS0038ACCODEget.PRODUCT1 = L0001row("NACPRODUCT1")                         '品名１
                CS0038ACCODEget.SUPPLIERKBN = L0001row("NACSUPPLIERKBN")                   '社有・庸車区分
                CS0038ACCODEget.MANGSORG = L0001row("NACMANGSORG1")                        '車両設置部署
                CS0038ACCODEget.MANGUORG = L0001row("NACMANGUORG1")                        '車両運用部署
                CS0038ACCODEget.BASELEASE = L0001row("NACBASELEASE1")                      '車両所有
                CS0038ACCODEget.STAFFKBN = L0001row("NACSTAFFKBN")                         '社員区分
                CS0038ACCODEget.HORG = L0001row("NACHORG")                                 '配属部署
                CS0038ACCODEget.SORG = L0001row("NACSORG")                                 '作業部署

                '勘定科目判定テーブル検索（借方）
                CS0038ACCODEget.ACHANTEI = "AMD"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_D As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_D As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_D As String = CS0038ACCODEget.INQKBN

                '勘定科目判定テーブル検索（貸方）
                CS0038ACCODEget.ACHANTEI = "AMC"                                            '勘定科目判定コード
                CS0038ACCODEget.CS0038ACCODEget()
                Dim WW_ACCODE_C As String = CS0038ACCODEget.ACCODE
                Dim WW_SUBACCODE_C As String = CS0038ACCODEget.SUBACCODE
                Dim WW_INQKBN_C As String = CS0038ACCODEget.INQKBN

                Dim WW_ROW As DataRow
                '------------------------------------------------------
                '削除データ
                '------------------------------------------------------
                'If T0007HEADrow("DELFLG") = "1" Then
                '    '●借方
                '    L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                '    L0001row("INQKBN") = "1"                                         '照会区分
                '    L0001row("ACDCKBN") = "D"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "AMD"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "01"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（作業部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)

                '    '●貸方
                '    L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                '    L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                '    L0001row("INQKBN") = "0"                                         '照会区分
                '    L0001row("ACDCKBN") = "C"                                        '貸借区分
                '    L0001row("ACACHANTEI") = "AMC"                                   '勘定科目判定コード
                '    L0001row("DTLNO") = "02"                                         '明細番号
                '    L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                '    WW_ROW = L0001tbl.NewRow
                '    WW_ROW.ItemArray = L0001row.ItemArray
                '    L0001tbl.Rows.Add(WW_ROW)
                'End If

                '------------------------------------------------------
                '追加データ
                '------------------------------------------------------
                If T0007HEADrow("DELFLG") = "0" Then
                    '●借方
                    If WW_INQKBN_D = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_D                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_D                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_D                                 '照会区分
                        L0001row("ACDCKBN") = "D"                                        '貸借区分
                        L0001row("ACACHANTEI") = "AMD"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "01"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（作業部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If

                    '●貸方
                    If WW_INQKBN_C = "1" Then
                        L0001row("ACCODE") = WW_ACCODE_C                                 '勘定科目コード
                        L0001row("SUBACCODE") = WW_SUBACCODE_C                           '補助科目コード
                        L0001row("INQKBN") = WW_INQKBN_C                                 '照会区分
                        L0001row("ACDCKBN") = "C"                                        '貸借区分
                        L0001row("ACACHANTEI") = "AMC"                                   '勘定科目判定コード
                        L0001row("DTLNO") = "02"                                         '明細番号
                        L0001row("ACKEIJOORG") = T0007HEADrow("HORG")                    '計上部署コード（配属部署）

                        WW_ROW = IO_L1tbl.NewRow
                        WW_ROW.ItemArray = L0001row.ItemArray
                        IO_L1tbl.Rows.Add(WW_ROW)
                    End If
                End If

            Catch ex As Exception
                'ROWデータのCSV(tab)変換
                Dim WW_CSV As String = ""
                DatarowToCsv(WW_T0007tbl.Rows(i), WW_CSV)

                CS0011LOGWRITE.INFSUBCLASS = "L0001tbMonthlylTtlEdit"       'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "L0001tbMonthlylTtlEdit"           '
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
                CS0011LOGWRITE.TEXT = ex.ToString() & ControlChars.NewLine & " ERR DATA=(" & WW_CSV & ")"
                CS0011LOGWRITE.MESSAGENO = "00001"
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
                Throw
            End Try

        Next

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

    ' ***  Datarowを項目毎にカンマ区切りの文字列に変換
    Public Sub DatarowToCsv(ByVal iRow As DataRow, ByRef oCsv As String)
        Dim CSVstr As String = ""
        For i = 0 To iRow.ItemArray.Count - 1
            If i = 0 Then
                CSVstr = CSVstr & iRow.ItemArray(i).ToString
            Else
                CSVstr = CSVstr & ControlChars.Tab & iRow.ItemArray(i).ToString
            End If
        Next

        oCsv = CSVstr

    End Sub

    ''' <summary>
    ''' 部署コード変換
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
    ''' <param name="I_ORGCODE">変換元部署コード</param>
    ''' <param name="O_ORGCODE">変換後部署コード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Public Sub ConvORGCODE(ByVal I_COMPCODE As String, ByVal I_ORGCODE As String, ByRef O_ORGCODE As String, ByRef O_RTN As String)

        O_ORGCODE = I_ORGCODE
        O_RTN = C_MESSAGE_NO.NORMAL
        Try
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                '検索SQL文
                Dim SQLStr As String =
                      " SELECT CODE                             " _
                    & " FROM   M0006_STRUCT    M06              " _
                    & " WHERE  M06.CAMPCODE     = @COMPCODE     " _
                    & "   AND  M06.OBJECT       = @OBJECT       " _
                    & "   AND  M06.STRUCT       = @STRUCT       " _
                    & "   AND  M06.GRCODE01     = @ORGCODE      " _
                    & "   AND  M06.STYMD       <= @ENDYMD       " _
                    & "   AND  M06.ENDYMD      >= @STYMD        " _
                    & "   AND  M06.DELFLG      <> '1'           "
                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)

                    Dim P_COMPCODE As SqlParameter = SQLcmd.Parameters.Add("@COMPCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_ORGCODE As SqlParameter = SQLcmd.Parameters.Add("@ORGCODE", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_STYMD As SqlParameter = SQLcmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
                    Dim P_ENDYMD As SqlParameter = SQLcmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
                    Dim P_OBJECT As SqlParameter = SQLcmd.Parameters.Add("@OBJECT", System.Data.SqlDbType.NVarChar, 20)
                    Dim P_STRUCT As SqlParameter = SQLcmd.Parameters.Add("@STRUCT", System.Data.SqlDbType.NVarChar, 20)

                    P_COMPCODE.Value = I_COMPCODE
                    P_ORGCODE.Value = I_ORGCODE
                    P_STYMD.Value = Date.Now
                    P_ENDYMD.Value = Date.Now
                    P_OBJECT.Value = C_ROLE_VARIANT.USER_ORG
                    P_STRUCT.Value = "勤怠管理組織_営業所"

                    SQLcmd.CommandTimeout = 300
                    Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        O_ORGCODE = SQLdr("CODE")
                    End While

                    SQLdr.Dispose()
                    SQLdr = Nothing
                End Using
            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GRT0007COM"                   'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0006_STRUCT Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 固定値リスト取得
    ''' </summary>
    Public Function getList(ByVal cmp As String, ByVal cls As String) As ListBox

        Dim retListBox As ListBox = New ListBox
        GS0007FIXVALUElst.CAMPCODE = cmp
        GS0007FIXVALUElst.CLAS = cls
        GS0007FIXVALUElst.LISTBOX1 = retListBox
        GS0007FIXVALUElst.GS0007FIXVALUElst()
        retListBox = GS0007FIXVALUElst.LISTBOX1

        Return retListBox

    End Function
End Class

'■勤怠ＤＢ更新
Public Class GRT0007UPDATE

    '統計DB出力dll Interface
    Public Property SQLcon As SqlConnection                                   'DB接続文字列
    Public Property SQLtrn As SqlTransaction                                  'トランザクション
    Public Property T0005tbl As DataTable                                     '日報テーブル
    Public Property ENTRYDATE As Date                                         'エントリー日付
    Public Property ERR As String                                             'リターン値

    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION
    Private CS0026TblSort As New CS0026TBLSORT
    Private CS0043STAFFORGget As New CS0043STAFFORGget      '従業員マスタ取得
    Private CS0020JOURNAL As New CS0020JOURNAL              'Journal Out

    Public Sub T0007UPDtbl_ColumnsAdd(ByRef iTbl As DataTable)

        If iTbl.Columns.Count = 0 Then
        Else
            iTbl.Columns.Clear()
        End If

        'T0005DB項目作成
        iTbl.Clear()
        iTbl.Columns.Add("CAMPCODE", GetType(String))
        iTbl.Columns.Add("TAISHOYM", GetType(String))
        iTbl.Columns.Add("STAFFCODE", GetType(String))
        iTbl.Columns.Add("WORKDATE", GetType(String))
        iTbl.Columns.Add("HDKBN", GetType(String))
        iTbl.Columns.Add("RECODEKBN", GetType(String))
        iTbl.Columns.Add("SEQ", GetType(Integer))
        iTbl.Columns.Add("ENTRYDATE", GetType(String))
        iTbl.Columns.Add("NIPPOLINKCODE", GetType(String))
        iTbl.Columns.Add("MORG", GetType(String))
        iTbl.Columns.Add("HORG", GetType(String))
        iTbl.Columns.Add("SORG", GetType(String))
        iTbl.Columns.Add("STAFFKBN", GetType(String))
        iTbl.Columns.Add("HOLIDAYKBN", GetType(String))
        iTbl.Columns.Add("PAYKBN", GetType(String))
        iTbl.Columns.Add("SHUKCHOKKBN", GetType(String))
        iTbl.Columns.Add("WORKKBN", GetType(String))
        iTbl.Columns.Add("STDATE", GetType(String))
        iTbl.Columns.Add("STTIME", GetType(String))
        iTbl.Columns.Add("ENDDATE", GetType(String))
        iTbl.Columns.Add("ENDTIME", GetType(String))
        iTbl.Columns.Add("WORKTIME", GetType(Integer))
        iTbl.Columns.Add("MOVETIME", GetType(Integer))
        iTbl.Columns.Add("ACTTIME", GetType(Integer))
        iTbl.Columns.Add("BINDSTDATE", GetType(String))
        iTbl.Columns.Add("BINDTIME", GetType(Integer))
        iTbl.Columns.Add("NIPPOBREAKTIME", GetType(Integer))
        iTbl.Columns.Add("BREAKTIME", GetType(Integer))
        iTbl.Columns.Add("BREAKTIMECHO", GetType(Integer))
        iTbl.Columns.Add("NIGHTTIME", GetType(Integer))
        iTbl.Columns.Add("NIGHTTIMECHO", GetType(Integer))
        iTbl.Columns.Add("ORVERTIME", GetType(Integer))
        iTbl.Columns.Add("ORVERTIMECHO", GetType(Integer))
        iTbl.Columns.Add("WNIGHTTIME", GetType(Integer))
        iTbl.Columns.Add("WNIGHTTIMECHO", GetType(Integer))
        iTbl.Columns.Add("SWORKTIME", GetType(Integer))
        iTbl.Columns.Add("SWORKTIMECHO", GetType(Integer))
        iTbl.Columns.Add("SNIGHTTIME", GetType(Integer))
        iTbl.Columns.Add("SNIGHTTIMECHO", GetType(Integer))
        iTbl.Columns.Add("HWORKTIME", GetType(Integer))
        iTbl.Columns.Add("HWORKTIMECHO", GetType(Integer))
        iTbl.Columns.Add("HNIGHTTIME", GetType(Integer))
        iTbl.Columns.Add("HNIGHTTIMECHO", GetType(Integer))
        iTbl.Columns.Add("WORKNISSU", GetType(Integer))
        iTbl.Columns.Add("WORKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("SHOUKETUNISSU", GetType(Integer))
        iTbl.Columns.Add("SHOUKETUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("KUMIKETUNISSU", GetType(Integer))
        iTbl.Columns.Add("KUMIKETUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("ETCKETUNISSU", GetType(Integer))
        iTbl.Columns.Add("ETCKETUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("NENKYUNISSU", GetType(Integer))
        iTbl.Columns.Add("NENKYUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("TOKUKYUNISSU", GetType(Integer))
        iTbl.Columns.Add("TOKUKYUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("CHIKOKSOTAINISSU", GetType(Integer))
        iTbl.Columns.Add("CHIKOKSOTAINISSUCHO", GetType(Integer))
        iTbl.Columns.Add("STOCKNISSU", GetType(Integer))
        iTbl.Columns.Add("STOCKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("KYOTEIWEEKNISSU", GetType(Integer))
        iTbl.Columns.Add("KYOTEIWEEKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("WEEKNISSU", GetType(Integer))
        iTbl.Columns.Add("WEEKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("DAIKYUNISSU", GetType(Integer))
        iTbl.Columns.Add("DAIKYUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("NENSHINISSU", GetType(Integer))
        iTbl.Columns.Add("NENSHINISSUCHO", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKNNISSU", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKNNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKNISSU", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKNHLDNISSU", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKNHLDNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKHLDNISSU", GetType(Integer))
        iTbl.Columns.Add("SHUKCHOKHLDNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("TOKSAAKAISU", GetType(Integer))
        iTbl.Columns.Add("TOKSAAKAISUCHO", GetType(Integer))
        iTbl.Columns.Add("TOKSABKAISU", GetType(Integer))
        iTbl.Columns.Add("TOKSABKAISUCHO", GetType(Integer))
        iTbl.Columns.Add("TOKSACKAISU", GetType(Integer))
        iTbl.Columns.Add("TOKSACKAISUCHO", GetType(Integer))
        iTbl.Columns.Add("TENKOKAISU", GetType(Double))
        iTbl.Columns.Add("TENKOKAISUCHO", GetType(Double))
        iTbl.Columns.Add("HOANTIME", GetType(Integer))
        iTbl.Columns.Add("HOANTIMECHO", GetType(Integer))
        iTbl.Columns.Add("KOATUTIME", GetType(Integer))
        iTbl.Columns.Add("KOATUTIMECHO", GetType(Integer))
        iTbl.Columns.Add("TOKUSA1TIME", GetType(Integer))
        iTbl.Columns.Add("TOKUSA1TIMECHO", GetType(Integer))
        iTbl.Columns.Add("HAYADETIME", GetType(Integer))
        iTbl.Columns.Add("HAYADETIMECHO", GetType(Integer))
        iTbl.Columns.Add("PONPNISSU", GetType(Integer))
        iTbl.Columns.Add("PONPNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("BULKNISSU", GetType(Integer))
        iTbl.Columns.Add("BULKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("TRAILERNISSU", GetType(Integer))
        iTbl.Columns.Add("TRAILERNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("BKINMUKAISU", GetType(Integer))
        iTbl.Columns.Add("BKINMUKAISUCHO", GetType(Integer))
        iTbl.Columns.Add("SHARYOKBN", GetType(String))
        iTbl.Columns.Add("OILPAYKBN", GetType(String))
        iTbl.Columns.Add("UNLOADCNT", GetType(Integer))
        iTbl.Columns.Add("UNLOADCNTCHO", GetType(Integer))
        iTbl.Columns.Add("HAIDISTANCE", GetType(Double))
        iTbl.Columns.Add("HAIDISTANCECHO", GetType(Double))
        iTbl.Columns.Add("KAIDISTANCE", GetType(Double))
        iTbl.Columns.Add("KAIDISTANCECHO", GetType(Double))
        iTbl.Columns.Add("ORVERTIMEADD", GetType(Integer))
        iTbl.Columns.Add("WNIGHTTIMEADD", GetType(Integer))
        iTbl.Columns.Add("SWORKTIMEADD", GetType(Integer))
        iTbl.Columns.Add("SNIGHTTIMEADD", GetType(Integer))
        '2018/08/03　追加（承認・申請）
        iTbl.Columns.Add("YENDTIME", GetType(String))
        iTbl.Columns.Add("APPLYID", GetType(String))
        iTbl.Columns.Add("RIYU", GetType(String))
        iTbl.Columns.Add("RIYUETC", GetType(String))
        'NJS専用
        iTbl.Columns.Add("HAISOTIME", GetType(Integer))
        iTbl.Columns.Add("NENMATUNISSU", GetType(Integer))
        iTbl.Columns.Add("NENMATUNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("SHACHUHAKKBN", GetType(String))
        iTbl.Columns.Add("SHACHUHAKNISSU", GetType(Integer))
        iTbl.Columns.Add("SHACHUHAKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("MODELDISTANCE", GetType(Double))
        iTbl.Columns.Add("MODELDISTANCECHO", GetType(Double))
        iTbl.Columns.Add("JIKYUSHATIME", GetType(Integer))
        iTbl.Columns.Add("JIKYUSHATIMECHO", GetType(Integer))
        '近石専用
        iTbl.Columns.Add("HDAIWORKTIME", GetType(Integer))
        iTbl.Columns.Add("HDAIWORKTIMECHO", GetType(Integer))
        iTbl.Columns.Add("HDAINIGHTTIME", GetType(Integer))
        iTbl.Columns.Add("HDAINIGHTTIMECHO", GetType(Integer))
        iTbl.Columns.Add("SDAIWORKTIME", GetType(Integer))
        iTbl.Columns.Add("SDAIWORKTIMECHO", GetType(Integer))
        iTbl.Columns.Add("SDAINIGHTTIME", GetType(Integer))
        iTbl.Columns.Add("SDAINIGHTTIMECHO", GetType(Integer))
        iTbl.Columns.Add("WWORKTIME", GetType(Integer))
        iTbl.Columns.Add("WWORKTIMECHO", GetType(Integer))
        iTbl.Columns.Add("JYOMUTIME", GetType(Integer))
        iTbl.Columns.Add("JYOMUTIMECHO", GetType(Integer))
        iTbl.Columns.Add("HWORKNISSU", GetType(Integer))
        iTbl.Columns.Add("HWORKNISSUCHO", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT1_1", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO1_1", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT1_2", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO1_2", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT1_3", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO1_3", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT1_4", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO1_4", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT2_1", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO2_1", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT2_2", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO2_2", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT2_3", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO2_3", GetType(Integer))
        iTbl.Columns.Add("KAITENCNT2_4", GetType(Integer))
        iTbl.Columns.Add("KAITENCNTCHO2_4", GetType(Integer))
        'JKT専用
        iTbl.Columns.Add("SENJYOCNT", GetType(Integer))
        iTbl.Columns.Add("SENJYOCNTCHO", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT1", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT1CHO", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT2", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT2CHO", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT3", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT3CHO", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT4", GetType(Integer))
        iTbl.Columns.Add("UNLOADADDCNT4CHO", GetType(Integer))
        iTbl.Columns.Add("LOADINGCNT1", GetType(Integer))
        iTbl.Columns.Add("LOADINGCNT1CHO", GetType(Integer))
        iTbl.Columns.Add("LOADINGCNT2", GetType(Integer))
        iTbl.Columns.Add("LOADINGCNT2CHO", GetType(Integer))
        iTbl.Columns.Add("SHORTDISTANCE1", GetType(Integer))
        iTbl.Columns.Add("SHORTDISTANCE1CHO", GetType(Integer))
        iTbl.Columns.Add("SHORTDISTANCE2", GetType(Integer))
        iTbl.Columns.Add("SHORTDISTANCE2CHO", GetType(Integer))

        iTbl.Columns.Add("DELFLG", GetType(String))
        iTbl.Columns.Add("INITYMD", GetType(DateTime))
        iTbl.Columns.Add("UPDYMD", GetType(DateTime))
        iTbl.Columns.Add("UPDUSER", GetType(String))
        iTbl.Columns.Add("UPDTERMID", GetType(String))
        iTbl.Columns.Add("RECEIVEYMD", GetType(DateTime))

    End Sub

    ' ***  勤怠ＤＢ削除                                                          ***
    Public Sub T0007_Delete(ByRef iSQLcon As SqlConnection, ByRef iSQLtrn As SqlTransaction,
                            ByRef iRow As DataRow, ByVal iDATENOW As Date, ByRef oRtn As String,
                            ByVal DELUSERID As String, ByVal DELUSERTERMID As String)
        Dim CS0011LOGWRITE As New BASEDLL.CS0011LOGWrite

        Try
            oRtn = C_MESSAGE_NO.NORMAL

            '日報ＤＢ更新
            Dim SQLStr As String =
                        "UPDATE T0007_KINTAI " _
                      & "SET DELFLG      = '1' " _
                      & "  , UPDYMD      = @UPDYMD " _
                      & "  , UPDUSER     = @UPDUSER " _
                      & "  , UPDTERMID   = @UPDTERMID " _
                      & "  , RECEIVEYMD  = @RECEIVEYMD  " _
                      & "WHERE CAMPCODE  = @CAMPCODE " _
                      & "  and TAISHOYM  = @TAISHOYM " _
                      & "  and STAFFCODE = @STAFFCODE " _
                      & "  and WORKDATE  = @WORKDATE " _
                      & "  and RECODEKBN = @RECODEKBN " _
                      & "  and DELFLG   <> '1' ; "

            Dim SQLcmd As SqlCommand = New SqlCommand(SQLStr, iSQLcon, iSQLtrn)
            Dim P_CAMPCODE As SqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_TAISHOYM As SqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", System.Data.SqlDbType.NVarChar, 7)
            Dim P_STAFFCODE As SqlParameter = SQLcmd.Parameters.Add("@STAFFCODE", System.Data.SqlDbType.NVarChar, 20)
            Dim P_WORKDATE As SqlParameter = SQLcmd.Parameters.Add("@WORKDATE", System.Data.SqlDbType.Date)
            Dim P_RECODEKBN As SqlParameter = SQLcmd.Parameters.Add("@RECODEKBN", System.Data.SqlDbType.NVarChar, 1)
            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar, 20)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar, 30)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            P_CAMPCODE.Value = iRow("CAMPCODE")
            P_TAISHOYM.Value = iRow("TAISHOYM")
            P_WORKDATE.Value = iRow("WORKDATE")
            P_RECODEKBN.Value = iRow("RECODEKBN")
            P_STAFFCODE.Value = iRow("STAFFCODE")
            P_UPDYMD.Value = iDATENOW
            P_UPDUSER.Value = DELUSERID
            P_UPDTERMID.Value = DELUSERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            SQLcmd.CommandTimeout = 300
            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "T0007_Delete"                 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:UPDATE T0007_KINTAI"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT                                  '
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            oRtn = C_MESSAGE_NO.DB_ERROR
            Exit Sub

        End Try

    End Sub
End Class



Option Strict On
Imports System.Data.SqlClient
''' <summary>
''' 履歴テーブル登録クラス
''' </summary>
''' <remarks>各種履歴テーブルに登録する際はこちらに定義</remarks>
Public Class EntryHistory

    ''' <summary>
    ''' 受注履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="drOrder">履歴用の受注テーブル行オブジェクト</param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertOrderHistory(sqlCon As SqlConnection, sqlTran As SqlTransaction, drOrder As DataRow)
        '◯受注TBL
        Dim sqlOrderStat As New StringBuilder
        sqlOrderStat.AppendLine("INSERT INTO OIL.HIS0001_ORDER")
        sqlOrderStat.AppendLine("   (HISTORYNO,MAPID,ORDERNO,TRAINNO,TRAINNAME,ORDERYMD,OFFICECODE,OFFICENAME,ORDERTYPE,")
        sqlOrderStat.AppendLine("    SHIPPERSCODE,SHIPPERSNAME,BASECODE,BASENAME,CONSIGNEECODE,CONSIGNEENAME,")
        sqlOrderStat.AppendLine("    DEPSTATION,DEPSTATIONNAME,ARRSTATION,ARRSTATIONNAME,RETSTATION,RETSTATIONNAME,")
        sqlOrderStat.AppendLine("    CHANGERETSTATION,CHANGERETSTATIONNAME,ORDERSTATUS,ORDERINFO,EMPTYTURNFLG,STACKINGFLG,USEPROPRIETYFLG,CONTACTFLG,RESULTFLG,DELIVERYFLG,")
        sqlOrderStat.AppendLine("    LODDATE,DEPDATE,ARRDATE,ACCDATE,EMPARRDATE,ACTUALLODDATE,ACTUALDEPDATE,ACTUALARRDATE,ACTUALACCDATE,ACTUALEMPARRDATE,")
        sqlOrderStat.AppendLine("    RTANK,HTANK,TTANK,MTTANK,KTANK,K3TANK,K5TANK,K10TANK,LTANK,ATANK,")
        sqlOrderStat.AppendLine("    OTHER1OTANK,OTHER2OTANK,OTHER3OTANK,OTHER4OTANK,OTHER5OTANK,")
        sqlOrderStat.AppendLine("    OTHER6OTANK,OTHER7OTANK,OTHER8OTANK,OTHER9OTANK,OTHER10OTANK,")
        sqlOrderStat.AppendLine("    TOTALTANK,")
        sqlOrderStat.AppendLine("    RTANKCH,HTANKCH,TTANKCH,MTTANKCH,KTANKCH,K3TANKCH,K5TANKCH,K10TANKCH,LTANKCH,ATANKCH,")
        sqlOrderStat.AppendLine("    OTHER1OTANKCH,OTHER2OTANKCH,OTHER3OTANKCH,OTHER4OTANKCH,OTHER5OTANKCH,")
        sqlOrderStat.AppendLine("    OTHER6OTANKCH,OTHER7OTANKCH,OTHER8OTANKCH,OTHER9OTANKCH,OTHER10OTANKCH,")
        sqlOrderStat.AppendLine("    TOTALTANKCH,TANKLINKNO,TANKLINKNOMADE,KEIJYOYMD,")
        sqlOrderStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,OTFILENAME,RECEIVECOUNT,")
        sqlOrderStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlOrderStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD)")
        sqlOrderStat.AppendLine("    VALUES")
        sqlOrderStat.AppendLine("   (@HISTORYNO,@MAPID,@ORDERNO,@TRAINNO,@TRAINNAME,@ORDERYMD,@OFFICECODE,@OFFICENAME,@ORDERTYPE,")
        sqlOrderStat.AppendLine("    @SHIPPERSCODE,@SHIPPERSNAME,@BASECODE,@BASENAME,@CONSIGNEECODE,@CONSIGNEENAME,")
        sqlOrderStat.AppendLine("    @DEPSTATION,@DEPSTATIONNAME,@ARRSTATION,@ARRSTATIONNAME,@RETSTATION,@RETSTATIONNAME,")
        sqlOrderStat.AppendLine("    @CHANGERETSTATION,@CHANGERETSTATIONNAME,@ORDERSTATUS,@ORDERINFO,@EMPTYTURNFLG,@STACKINGFLG,@USEPROPRIETYFLG,@CONTACTFLG,@RESULTFLG,@DELIVERYFLG,")
        sqlOrderStat.AppendLine("    @LODDATE,@DEPDATE,@ARRDATE,@ACCDATE,@EMPARRDATE,@ACTUALLODDATE,@ACTUALDEPDATE,@ACTUALARRDATE,@ACTUALACCDATE,@ACTUALEMPARRDATE,")
        sqlOrderStat.AppendLine("    @RTANK,@HTANK,@TTANK,@MTTANK,@KTANK,@K3TANK,@K5TANK,@K10TANK,@LTANK,@ATANK,")
        sqlOrderStat.AppendLine("    @OTHER1OTANK,@OTHER2OTANK,@OTHER3OTANK,@OTHER4OTANK,@OTHER5OTANK,")
        sqlOrderStat.AppendLine("    @OTHER6OTANK,@OTHER7OTANK,@OTHER8OTANK,@OTHER9OTANK,@OTHER10OTANK,")
        sqlOrderStat.AppendLine("    @TOTALTANK,")
        sqlOrderStat.AppendLine("    @RTANKCH,@HTANKCH,@TTANKCH,@MTTANKCH,@KTANKCH,@K3TANKCH,@K5TANKCH,@K10TANKCH,@LTANKCH,@ATANKCH,")
        sqlOrderStat.AppendLine("    @OTHER1OTANKCH,@OTHER2OTANKCH,@OTHER3OTANKCH,@OTHER4OTANKCH,@OTHER5OTANKCH,")
        sqlOrderStat.AppendLine("    @OTHER6OTANKCH,@OTHER7OTANKCH,@OTHER8OTANKCH,@OTHER9OTANKCH,@OTHER10OTANKCH,")
        sqlOrderStat.AppendLine("    @TOTALTANKCH,@TANKLINKNO,@TANKLINKNOMADE,@KEIJYOYMD,")
        sqlOrderStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,@OTFILENAME,@RECEIVECOUNT,")
        sqlOrderStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlOrderStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD)")

        Using sqlOrderCmd As New SqlCommand(sqlOrderStat.ToString, sqlCon, sqlTran)
            With sqlOrderCmd.Parameters
                .Add("HISTORYNO", SqlDbType.NVarChar).Value = drOrder("HISTORYNO")
                .Add("MAPID", SqlDbType.NVarChar).Value = drOrder("MAPID")
                .Add("ORDERNO", SqlDbType.NVarChar).Value = drOrder("ORDERNO")
                .Add("TRAINNO", SqlDbType.NVarChar).Value = drOrder("TRAINNO")
                .Add("TRAINNAME", SqlDbType.NVarChar).Value = drOrder("TRAINNAME")
                .Add("ORDERYMD", SqlDbType.Date).Value = drOrder("ORDERYMD")
                .Add("OFFICECODE", SqlDbType.NVarChar).Value = drOrder("OFFICECODE")
                .Add("OFFICENAME", SqlDbType.NVarChar).Value = drOrder("OFFICENAME")
                .Add("ORDERTYPE", SqlDbType.NVarChar).Value = drOrder("ORDERTYPE")
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = drOrder("SHIPPERSCODE")
                .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = drOrder("SHIPPERSNAME")
                .Add("BASECODE", SqlDbType.NVarChar).Value = drOrder("BASECODE")
                .Add("BASENAME", SqlDbType.NVarChar).Value = drOrder("BASENAME")
                .Add("CONSIGNEECODE", SqlDbType.NVarChar).Value = drOrder("CONSIGNEECODE")
                .Add("CONSIGNEENAME", SqlDbType.NVarChar).Value = drOrder("CONSIGNEENAME")
                .Add("DEPSTATION", SqlDbType.NVarChar).Value = drOrder("DEPSTATION")
                .Add("DEPSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("DEPSTATIONNAME")
                .Add("ARRSTATION", SqlDbType.NVarChar).Value = drOrder("ARRSTATION")
                .Add("ARRSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("ARRSTATIONNAME")
                .Add("RETSTATION", SqlDbType.NVarChar).Value = drOrder("RETSTATION")
                .Add("RETSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("RETSTATIONNAME")
                .Add("CHANGERETSTATION", SqlDbType.NVarChar).Value = drOrder("CHANGERETSTATION")
                .Add("CHANGERETSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("CHANGERETSTATIONNAME")
                .Add("ORDERSTATUS", SqlDbType.NVarChar).Value = drOrder("ORDERSTATUS")
                .Add("ORDERINFO", SqlDbType.NVarChar).Value = drOrder("ORDERINFO")
                .Add("EMPTYTURNFLG", SqlDbType.NVarChar).Value = drOrder("EMPTYTURNFLG")
                .Add("STACKINGFLG", SqlDbType.NVarChar).Value = drOrder("STACKINGFLG")
                .Add("USEPROPRIETYFLG", SqlDbType.NVarChar).Value = drOrder("USEPROPRIETYFLG")
                .Add("CONTACTFLG", SqlDbType.NVarChar).Value = drOrder("CONTACTFLG")
                .Add("RESULTFLG", SqlDbType.NVarChar).Value = drOrder("RESULTFLG")
                .Add("DELIVERYFLG", SqlDbType.NVarChar).Value = drOrder("DELIVERYFLG")
                .Add("LODDATE", SqlDbType.Date).Value = drOrder("LODDATE")
                .Add("DEPDATE", SqlDbType.Date).Value = drOrder("DEPDATE")
                .Add("ARRDATE", SqlDbType.Date).Value = drOrder("ARRDATE")
                .Add("ACCDATE", SqlDbType.Date).Value = drOrder("ACCDATE")
                .Add("EMPARRDATE", SqlDbType.Date).Value = drOrder("EMPARRDATE")
                .Add("ACTUALLODDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALLODDATE"), CType(DBNull.Value, Object), drOrder("ACTUALLODDATE"))
                .Add("ACTUALDEPDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("ACTUALDEPDATE"))
                .Add("ACTUALARRDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALARRDATE"))
                .Add("ACTUALACCDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("ACTUALACCDATE"))
                .Add("ACTUALEMPARRDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALEMPARRDATE"))
                .Add("RTANK", SqlDbType.Int).Value = drOrder("RTANK")
                .Add("HTANK", SqlDbType.Int).Value = drOrder("HTANK")
                .Add("TTANK", SqlDbType.Int).Value = drOrder("TTANK")
                .Add("MTTANK", SqlDbType.Int).Value = drOrder("MTTANK")
                .Add("KTANK", SqlDbType.Int).Value = drOrder("KTANK")
                .Add("K3TANK", SqlDbType.Int).Value = drOrder("K3TANK")
                .Add("K5TANK", SqlDbType.Int).Value = drOrder("K5TANK")
                .Add("K10TANK", SqlDbType.Int).Value = drOrder("K10TANK")
                .Add("LTANK", SqlDbType.Int).Value = drOrder("LTANK")
                .Add("ATANK", SqlDbType.Int).Value = drOrder("ATANK")
                .Add("OTHER1OTANK", SqlDbType.Int).Value = drOrder("OTHER1OTANK")
                .Add("OTHER2OTANK", SqlDbType.Int).Value = drOrder("OTHER2OTANK")
                .Add("OTHER3OTANK", SqlDbType.Int).Value = drOrder("OTHER3OTANK")
                .Add("OTHER4OTANK", SqlDbType.Int).Value = drOrder("OTHER4OTANK")
                .Add("OTHER5OTANK", SqlDbType.Int).Value = drOrder("OTHER5OTANK")
                .Add("OTHER6OTANK", SqlDbType.Int).Value = drOrder("OTHER6OTANK")
                .Add("OTHER7OTANK", SqlDbType.Int).Value = drOrder("OTHER7OTANK")
                .Add("OTHER8OTANK", SqlDbType.Int).Value = drOrder("OTHER8OTANK")
                .Add("OTHER9OTANK", SqlDbType.Int).Value = drOrder("OTHER9OTANK")
                .Add("OTHER10OTANK", SqlDbType.Int).Value = drOrder("OTHER10OTANK")
                .Add("TOTALTANK", SqlDbType.Int).Value = drOrder("TOTALTANK")
                .Add("RTANKCH", SqlDbType.Int).Value = drOrder("RTANKCH")
                .Add("HTANKCH", SqlDbType.Int).Value = drOrder("HTANKCH")
                .Add("TTANKCH", SqlDbType.Int).Value = drOrder("TTANKCH")
                .Add("MTTANKCH", SqlDbType.Int).Value = drOrder("MTTANKCH")
                .Add("KTANKCH", SqlDbType.Int).Value = drOrder("KTANKCH")
                .Add("K3TANKCH", SqlDbType.Int).Value = drOrder("K3TANKCH")
                .Add("K5TANKCH", SqlDbType.Int).Value = drOrder("K5TANKCH")
                .Add("K10TANKCH", SqlDbType.Int).Value = drOrder("K10TANKCH")
                .Add("LTANKCH", SqlDbType.Int).Value = drOrder("LTANKCH")
                .Add("ATANKCH", SqlDbType.Int).Value = drOrder("ATANKCH")
                .Add("OTHER1OTANKCH", SqlDbType.Int).Value = drOrder("OTHER1OTANKCH")
                .Add("OTHER2OTANKCH", SqlDbType.Int).Value = drOrder("OTHER2OTANKCH")
                .Add("OTHER3OTANKCH", SqlDbType.Int).Value = drOrder("OTHER3OTANKCH")
                .Add("OTHER4OTANKCH", SqlDbType.Int).Value = drOrder("OTHER4OTANKCH")
                .Add("OTHER5OTANKCH", SqlDbType.Int).Value = drOrder("OTHER5OTANKCH")
                .Add("OTHER6OTANKCH", SqlDbType.Int).Value = drOrder("OTHER6OTANKCH")
                .Add("OTHER7OTANKCH", SqlDbType.Int).Value = drOrder("OTHER7OTANKCH")
                .Add("OTHER8OTANKCH", SqlDbType.Int).Value = drOrder("OTHER8OTANKCH")
                .Add("OTHER9OTANKCH", SqlDbType.Int).Value = drOrder("OTHER9OTANKCH")
                .Add("OTHER10OTANKCH", SqlDbType.Int).Value = drOrder("OTHER10OTANKCH")
                .Add("TOTALTANKCH", SqlDbType.Int).Value = drOrder("TOTALTANKCH")
                .Add("TANKLINKNO", SqlDbType.NVarChar).Value = drOrder("TANKLINKNO")
                .Add("TANKLINKNOMADE", SqlDbType.NVarChar).Value = drOrder("TANKLINKNOMADE")
                .Add("KEIJYOYMD", SqlDbType.Date).Value = If(drOrder.IsNull("KEIJYOYMD"), CType(DBNull.Value, Object), drOrder("KEIJYOYMD"))
                .Add("SALSE", SqlDbType.Int).Value = drOrder("SALSE")
                .Add("SALSETAX", SqlDbType.Int).Value = drOrder("SALSETAX")
                .Add("TOTALSALSE", SqlDbType.Int).Value = drOrder("TOTALSALSE")
                .Add("PAYMENT", SqlDbType.Int).Value = drOrder("PAYMENT")
                .Add("PAYMENTTAX", SqlDbType.Int).Value = drOrder("PAYMENTTAX")
                .Add("TOTALPAYMENT", SqlDbType.Int).Value = drOrder("TOTALPAYMENT")
                .Add("OTFILENAME", SqlDbType.NVarChar).Value = drOrder("OTFILENAME")
                .Add("RECEIVECOUNT", SqlDbType.Int).Value = If(Convert.ToString(drOrder("RECEIVECOUNT")) = "", CType(DBNull.Value, Object), drOrder("RECEIVECOUNT"))
                .Add("DELFLG", SqlDbType.NVarChar).Value = drOrder("DELFLG")
                .Add("INITYMD", SqlDbType.DateTime).Value = drOrder("INITYMD")
                .Add("INITUSER", SqlDbType.NVarChar).Value = drOrder("INITUSER")
                .Add("INITTERMID", SqlDbType.NVarChar).Value = drOrder("INITTERMID")
                .Add("UPDYMD", SqlDbType.DateTime).Value = drOrder("UPDYMD")
                .Add("UPDUSER", SqlDbType.NVarChar).Value = drOrder("UPDUSER")
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = drOrder("UPDTERMID")
                .Add("RECEIVEYMD", SqlDbType.DateTime).Value = drOrder("RECEIVEYMD")
            End With
            sqlOrderCmd.CommandTimeout = 300
            sqlOrderCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 受注明細履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="drOrder"></param>
    ''' <remarks>通常の受注明細テーブルに「履歴番号」(InsertOrderHistoryで採番した履歴番号と合わせる)
    ''' と「画面ID」(呼出し側のMe.Title）の
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertOrderDetailHistory(sqlCon As SqlConnection, sqlTran As SqlTransaction, drOrder As DataRow)

        '◯受注明細TBL
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO OIL.HIS0002_DETAIL")
        sqlDetailStat.AppendLine("   (HISTORYNO,MAPID,ORDERNO,DETAILNO,SHIPORDER,LINEORDER,TANKNO,KAMOKU,")
        sqlDetailStat.AppendLine("    STACKINGFLG,FIRSTRETURNFLG,AFTERRETURNFLG,ORDERINFO,SHIPPERSCODE,SHIPPERSNAME,")
        sqlDetailStat.AppendLine("    OILCODE,OILNAME,ORDERINGTYPE,ORDERINGOILNAME,")
        sqlDetailStat.AppendLine("    CARSNUMBER,CARSAMOUNT,RETURNDATETRAIN,")
        sqlDetailStat.AppendLine("    JOINTCODE,JOINT,REMARK,")
        sqlDetailStat.AppendLine("    CHANGETRAINNO,CHANGETRAINNAME,")
        sqlDetailStat.AppendLine("    SECONDCONSIGNEECODE,SECONDCONSIGNEENAME,")
        sqlDetailStat.AppendLine("    SECONDARRSTATION,SECONDARRSTATIONNAME,")
        sqlDetailStat.AppendLine("    CHANGERETSTATION,CHANGERETSTATIONNAME,")
        sqlDetailStat.AppendLine("    LINE,FILLINGPOINT,")
        sqlDetailStat.AppendLine("    LOADINGIRILINETRAINNO,LOADINGIRILINETRAINNAME,")
        sqlDetailStat.AppendLine("    LOADINGIRILINEORDER,LOADINGOUTLETTRAINNO,")
        sqlDetailStat.AppendLine("    LOADINGOUTLETTRAINNAME,LOADINGOUTLETORDER,")
        sqlDetailStat.AppendLine("    ACTUALLODDATE,ACTUALDEPDATE,ACTUALARRDATE,ACTUALACCDATE,ACTUALEMPARRDATE,")
        sqlDetailStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,")
        sqlDetailStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlDetailStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD )")
        sqlDetailStat.AppendLine("    VALUES")
        sqlDetailStat.AppendLine("   (@HISTORYNO,@MAPID,@ORDERNO,@DETAILNO,@SHIPORDER,@LINEORDER,@TANKNO,@KAMOKU,")
        sqlDetailStat.AppendLine("    @STACKINGFLG,@FIRSTRETURNFLG,@AFTERRETURNFLG,@ORDERINFO,@SHIPPERSCODE,@SHIPPERSNAME,")
        sqlDetailStat.AppendLine("    @OILCODE,@OILNAME,@ORDERINGTYPE,@ORDERINGOILNAME,")
        sqlDetailStat.AppendLine("    @CARSNUMBER,@CARSAMOUNT,@RETURNDATETRAIN,")
        sqlDetailStat.AppendLine("    @JOINTCODE,@JOINT,@REMARK,")
        sqlDetailStat.AppendLine("    @CHANGETRAINNO,@CHANGETRAINNAME,")
        sqlDetailStat.AppendLine("    @SECONDCONSIGNEECODE,@SECONDCONSIGNEENAME,")
        sqlDetailStat.AppendLine("    @SECONDARRSTATION,@SECONDARRSTATIONNAME,")
        sqlDetailStat.AppendLine("    @CHANGERETSTATION,@CHANGERETSTATIONNAME,")
        sqlDetailStat.AppendLine("    @LINE,@FILLINGPOINT,")
        sqlDetailStat.AppendLine("    @LOADINGIRILINETRAINNO,@LOADINGIRILINETRAINNAME,")
        sqlDetailStat.AppendLine("    @LOADINGIRILINEORDER,@LOADINGOUTLETTRAINNO,")
        sqlDetailStat.AppendLine("    @LOADINGOUTLETTRAINNAME,@LOADINGOUTLETORDER,")
        sqlDetailStat.AppendLine("    @ACTUALLODDATE,@ACTUALDEPDATE,@ACTUALARRDATE,@ACTUALACCDATE,@ACTUALEMPARRDATE,")
        sqlDetailStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,")
        sqlDetailStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlDetailStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD )")

        Using sqlDetailCmd As New SqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("HISTORYNO", SqlDbType.NVarChar).Value = drOrder("HISTORYNO")
                .Add("MAPID", SqlDbType.NVarChar).Value = drOrder("MAPID")
                .Add("ORDERNO", SqlDbType.NVarChar).Value = drOrder("ORDERNO")
                .Add("DETAILNO", SqlDbType.NVarChar).Value = drOrder("DETAILNO")
                .Add("SHIPORDER", SqlDbType.NVarChar).Value = drOrder("SHIPORDER")
                .Add("LINEORDER", SqlDbType.NVarChar).Value = drOrder("LINEORDER")
                .Add("TANKNO", SqlDbType.NVarChar).Value = drOrder("TANKNO")
                .Add("KAMOKU", SqlDbType.NVarChar).Value = drOrder("KAMOKU")
                .Add("STACKINGFLG", SqlDbType.NVarChar).Value = drOrder("STACKINGFLG")
                .Add("FIRSTRETURNFLG", SqlDbType.NVarChar).Value = drOrder("FIRSTRETURNFLG")
                Try
                    .Add("AFTERRETURNFLG", SqlDbType.NVarChar).Value = drOrder("AFTERRETURNFLG")
                Catch ex As Exception
                    .Add("AFTERRETURNFLG", SqlDbType.NVarChar).Value = ""
                End Try
                .Add("ORDERINFO", SqlDbType.NVarChar).Value = drOrder("ORDERINFO")
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = drOrder("SHIPPERSCODE")
                .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = drOrder("SHIPPERSNAME")
                .Add("OILCODE", SqlDbType.NVarChar).Value = drOrder("OILCODE")
                .Add("OILNAME", SqlDbType.NVarChar).Value = drOrder("OILNAME")
                .Add("ORDERINGTYPE", SqlDbType.NVarChar).Value = drOrder("ORDERINGTYPE")
                .Add("ORDERINGOILNAME", SqlDbType.NVarChar).Value = drOrder("ORDERINGOILNAME")
                .Add("CARSNUMBER", SqlDbType.NVarChar).Value = drOrder("CARSNUMBER")
                .Add("CARSAMOUNT", SqlDbType.NVarChar).Value = drOrder("CARSAMOUNT")
                .Add("RETURNDATETRAIN", SqlDbType.NVarChar).Value = drOrder("RETURNDATETRAIN")
                .Add("JOINTCODE", SqlDbType.NVarChar).Value = drOrder("JOINTCODE")
                .Add("JOINT", SqlDbType.NVarChar).Value = drOrder("JOINT")
                .Add("REMARK", SqlDbType.NVarChar).Value = drOrder("REMARK")
                .Add("CHANGETRAINNO", SqlDbType.NVarChar).Value = drOrder("CHANGETRAINNO")
                .Add("CHANGETRAINNAME", SqlDbType.NVarChar).Value = drOrder("CHANGETRAINNAME")
                .Add("SECONDCONSIGNEECODE", SqlDbType.NVarChar).Value = drOrder("SECONDCONSIGNEECODE")
                .Add("SECONDCONSIGNEENAME", SqlDbType.NVarChar).Value = drOrder("SECONDCONSIGNEENAME")
                .Add("SECONDARRSTATION", SqlDbType.NVarChar).Value = drOrder("SECONDARRSTATION")
                .Add("SECONDARRSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("SECONDARRSTATIONNAME")
                .Add("CHANGERETSTATION", SqlDbType.NVarChar).Value = drOrder("CHANGERETSTATION")
                .Add("CHANGERETSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("CHANGERETSTATIONNAME")
                .Add("LINE", SqlDbType.NVarChar).Value = drOrder("LINE")
                .Add("FILLINGPOINT", SqlDbType.NVarChar).Value = drOrder("FILLINGPOINT")
                .Add("LOADINGIRILINETRAINNO", SqlDbType.NVarChar).Value = drOrder("LOADINGIRILINETRAINNO")
                .Add("LOADINGIRILINETRAINNAME", SqlDbType.NVarChar).Value = drOrder("LOADINGIRILINETRAINNAME")
                .Add("LOADINGIRILINEORDER", SqlDbType.NVarChar).Value = drOrder("LOADINGIRILINEORDER")
                .Add("LOADINGOUTLETTRAINNO", SqlDbType.NVarChar).Value = drOrder("LOADINGOUTLETTRAINNO")
                .Add("LOADINGOUTLETTRAINNAME", SqlDbType.NVarChar).Value = drOrder("LOADINGOUTLETTRAINNAME")
                .Add("LOADINGOUTLETORDER", SqlDbType.NVarChar).Value = drOrder("LOADINGOUTLETORDER")
                .Add("ACTUALLODDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALLODDATE"), CType(DBNull.Value, Object), drOrder("ACTUALLODDATE"))
                .Add("ACTUALDEPDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("ACTUALDEPDATE"))
                .Add("ACTUALARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALARRDATE"))
                .Add("ACTUALACCDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("ACTUALACCDATE"))
                .Add("ACTUALEMPARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALEMPARRDATE"))
                .Add("SALSE", SqlDbType.NVarChar).Value = drOrder("SALSE")
                .Add("SALSETAX", SqlDbType.NVarChar).Value = drOrder("SALSETAX")
                .Add("TOTALSALSE", SqlDbType.NVarChar).Value = drOrder("TOTALSALSE")
                .Add("PAYMENT", SqlDbType.NVarChar).Value = drOrder("PAYMENT")
                .Add("PAYMENTTAX", SqlDbType.NVarChar).Value = drOrder("PAYMENTTAX")
                .Add("TOTALPAYMENT", SqlDbType.NVarChar).Value = drOrder("TOTALPAYMENT")
                .Add("DELFLG", SqlDbType.NVarChar).Value = drOrder("DELFLG")
                .Add("INITYMD", SqlDbType.NVarChar).Value = drOrder("INITYMD")
                .Add("INITUSER", SqlDbType.NVarChar).Value = drOrder("INITUSER")
                .Add("INITTERMID", SqlDbType.NVarChar).Value = drOrder("INITTERMID")
                .Add("UPDYMD", SqlDbType.NVarChar).Value = drOrder("UPDYMD")
                .Add("UPDUSER", SqlDbType.NVarChar).Value = drOrder("UPDUSER")
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = drOrder("UPDTERMID")
                .Add("RECEIVEYMD", SqlDbType.NVarChar).Value = drOrder("RECEIVEYMD")
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>
    ''' 回送履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="drOrder">履歴用の回送テーブル行オブジェクト</param>
    ''' <remarks>通常の回送明細テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertKaisouHistory(sqlCon As SqlConnection, sqlTran As SqlTransaction, drOrder As DataRow)

        '◯回送TBL
        Dim sqlKaisouStat As New StringBuilder
        sqlKaisouStat.AppendLine("INSERT INTO OIL.HIS0003_KAISOU")
        sqlKaisouStat.AppendLine("   (HISTORYNO,MAPID,KAISOUNO,KAISOUTYPE,TRAINNO,TRAINNAME,KAISOUYMD,OFFICECODE,OFFICENAME,")
        sqlKaisouStat.AppendLine("    SHIPPERSCODE,SHIPPERSNAME,BASECODE,BASENAME,CONSIGNEECODE,CONSIGNEENAME,")
        sqlKaisouStat.AppendLine("    DEPSTATION,DEPSTATIONNAME,ARRSTATION,ARRSTATIONNAME,OBJECTIVECODE,")
        sqlKaisouStat.AppendLine("    KAISOUSTATUS,KAISOUINFO,FAREFLG,USEPROPRIETYFLG,DELIVERYFLG,")
        sqlKaisouStat.AppendLine("    DEPDATE,ARRDATE,ACCDATE,EMPARRDATE,ACTUALDEPDATE,ACTUALARRDATE,ACTUALACCDATE,ACTUALEMPARRDATE,")
        sqlKaisouStat.AppendLine("    TOTALTANK,ORDERNO,KEIJYOYMD,")
        sqlKaisouStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,")
        sqlKaisouStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlKaisouStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD)")
        sqlKaisouStat.AppendLine("    VALUES")
        sqlKaisouStat.AppendLine("   (@HISTORYNO,@MAPID,@KAISOUNO,@KAISOUTYPE,@TRAINNO,@TRAINNAME,@KAISOUYMD,@OFFICECODE,@OFFICENAME,")
        sqlKaisouStat.AppendLine("    @SHIPPERSCODE,@SHIPPERSNAME,@BASECODE,@BASENAME,@CONSIGNEECODE,@CONSIGNEENAME,")
        sqlKaisouStat.AppendLine("    @DEPSTATION,@DEPSTATIONNAME,@ARRSTATION,@ARRSTATIONNAME,@OBJECTIVECODE,")
        sqlKaisouStat.AppendLine("    @KAISOUSTATUS,@KAISOUINFO,@FAREFLG,@USEPROPRIETYFLG,@DELIVERYFLG,")
        sqlKaisouStat.AppendLine("    @DEPDATE,@ARRDATE,@ACCDATE,@EMPARRDATE,@ACTUALDEPDATE,@ACTUALARRDATE,@ACTUALACCDATE,@ACTUALEMPARRDATE,")
        sqlKaisouStat.AppendLine("    @TOTALTANK,@ORDERNO,@KEIJYOYMD,")
        sqlKaisouStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,")
        sqlKaisouStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlKaisouStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD)")

        Using sqlKaisouCmd As New SqlCommand(sqlKaisouStat.ToString, sqlCon, sqlTran)
            With sqlKaisouCmd.Parameters
                .Add("HISTORYNO", SqlDbType.NVarChar).Value = drOrder("HISTORYNO")
                .Add("MAPID", SqlDbType.NVarChar).Value = drOrder("MAPID")
                .Add("KAISOUNO", SqlDbType.NVarChar).Value = drOrder("KAISOUNO")
                .Add("KAISOUTYPE", SqlDbType.NVarChar).Value = drOrder("KAISOUTYPE")
                .Add("TRAINNO", SqlDbType.NVarChar).Value = drOrder("TRAINNO")
                .Add("TRAINNAME", SqlDbType.NVarChar).Value = drOrder("TRAINNAME")
                .Add("KAISOUYMD", SqlDbType.Date).Value = drOrder("KAISOUYMD")
                .Add("OFFICECODE", SqlDbType.NVarChar).Value = drOrder("OFFICECODE")
                .Add("OFFICENAME", SqlDbType.NVarChar).Value = drOrder("OFFICENAME")
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = drOrder("SHIPPERSCODE")
                .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = drOrder("SHIPPERSNAME")
                .Add("BASECODE", SqlDbType.NVarChar).Value = drOrder("BASECODE")
                .Add("BASENAME", SqlDbType.NVarChar).Value = drOrder("BASENAME")
                .Add("CONSIGNEECODE", SqlDbType.NVarChar).Value = drOrder("CONSIGNEECODE")
                .Add("CONSIGNEENAME", SqlDbType.NVarChar).Value = drOrder("CONSIGNEENAME")
                .Add("DEPSTATION", SqlDbType.NVarChar).Value = drOrder("DEPSTATION")
                .Add("DEPSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("DEPSTATIONNAME")
                .Add("ARRSTATION", SqlDbType.NVarChar).Value = drOrder("ARRSTATION")
                .Add("ARRSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("ARRSTATIONNAME")
                .Add("OBJECTIVECODE", SqlDbType.NVarChar).Value = drOrder("OBJECTIVECODE")
                .Add("KAISOUSTATUS", SqlDbType.NVarChar).Value = drOrder("KAISOUSTATUS")
                .Add("KAISOUINFO", SqlDbType.NVarChar).Value = drOrder("KAISOUINFO")
                .Add("FAREFLG", SqlDbType.NVarChar).Value = drOrder("FAREFLG")
                .Add("USEPROPRIETYFLG", SqlDbType.NVarChar).Value = drOrder("USEPROPRIETYFLG")
                .Add("DELIVERYFLG", SqlDbType.NVarChar).Value = drOrder("DELIVERYFLG")
                .Add("DEPDATE", SqlDbType.Date).Value = drOrder("DEPDATE")
                .Add("ARRDATE", SqlDbType.Date).Value = drOrder("ARRDATE")
                .Add("ACCDATE", SqlDbType.Date).Value = drOrder("ACCDATE")
                .Add("EMPARRDATE", SqlDbType.Date).Value = drOrder("EMPARRDATE")
                .Add("ACTUALDEPDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("ACTUALDEPDATE"))
                .Add("ACTUALARRDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALARRDATE"))
                .Add("ACTUALACCDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("ACTUALACCDATE"))
                .Add("ACTUALEMPARRDATE", SqlDbType.Date).Value = If(drOrder.IsNull("ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALEMPARRDATE"))
                .Add("TOTALTANK", SqlDbType.Int).Value = drOrder("TOTALTANK")
                .Add("ORDERNO", SqlDbType.NVarChar).Value = drOrder("ORDERNO")
                .Add("KEIJYOYMD", SqlDbType.Date).Value = If(drOrder.IsNull("KEIJYOYMD"), CType(DBNull.Value, Object), drOrder("KEIJYOYMD"))
                .Add("SALSE", SqlDbType.Int).Value = drOrder("SALSE")
                .Add("SALSETAX", SqlDbType.Int).Value = drOrder("SALSETAX")
                .Add("TOTALSALSE", SqlDbType.Int).Value = drOrder("TOTALSALSE")
                .Add("PAYMENT", SqlDbType.Int).Value = drOrder("PAYMENT")
                .Add("PAYMENTTAX", SqlDbType.Int).Value = drOrder("PAYMENTTAX")
                .Add("TOTALPAYMENT", SqlDbType.Int).Value = drOrder("TOTALPAYMENT")
                .Add("DELFLG", SqlDbType.NVarChar).Value = drOrder("DELFLG")
                .Add("INITYMD", SqlDbType.DateTime).Value = drOrder("INITYMD")
                .Add("INITUSER", SqlDbType.NVarChar).Value = drOrder("INITUSER")
                .Add("INITTERMID", SqlDbType.NVarChar).Value = drOrder("INITTERMID")
                .Add("UPDYMD", SqlDbType.DateTime).Value = drOrder("UPDYMD")
                .Add("UPDUSER", SqlDbType.NVarChar).Value = drOrder("UPDUSER")
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = drOrder("UPDTERMID")
                .Add("RECEIVEYMD", SqlDbType.DateTime).Value = drOrder("RECEIVEYMD")
            End With
            sqlKaisouCmd.CommandTimeout = 300
            sqlKaisouCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' 回送明細履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="drOrder"></param>
    ''' <remarks>通常の回送明細テーブルに「履歴番号」(InsertKaisouHistoryで採番した履歴番号と合わせる)
    ''' と「画面ID」(呼出し側のMe.Title）の
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertKaisouDetailHistory(sqlCon As SqlConnection, sqlTran As SqlTransaction, drOrder As DataRow)

        '◯回送明細TBL
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO OIL.HIS0004_KAISOUDETAIL")
        sqlDetailStat.AppendLine("   (HISTORYNO,MAPID,KAISOUNO,DETAILNO,SHIPORDER,TANKNO,KAMOKU,")
        sqlDetailStat.AppendLine("    KAISOUINFO,CARSNUMBER,REMARK,")
        sqlDetailStat.AppendLine("    ACTUALDEPDATE,ACTUALARRDATE,ACTUALACCDATE,ACTUALEMPARRDATE,")
        sqlDetailStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,")
        sqlDetailStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlDetailStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD )")
        sqlDetailStat.AppendLine("    VALUES")
        sqlDetailStat.AppendLine("   (@HISTORYNO,@MAPID,@KAISOUNO,@DETAILNO,@SHIPORDER,@TANKNO,@KAMOKU,")
        sqlDetailStat.AppendLine("    @KAISOUINFO,@CARSNUMBER,@REMARK,")
        sqlDetailStat.AppendLine("    @ACTUALDEPDATE,@ACTUALARRDATE,@ACTUALACCDATE,@ACTUALEMPARRDATE,")
        sqlDetailStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,")
        sqlDetailStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlDetailStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD )")

        Using sqlDetailCmd As New SqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("HISTORYNO", SqlDbType.NVarChar).Value = drOrder("HISTORYNO")
                .Add("MAPID", SqlDbType.NVarChar).Value = drOrder("MAPID")
                .Add("KAISOUNO", SqlDbType.NVarChar).Value = drOrder("KAISOUNO")
                .Add("DETAILNO", SqlDbType.NVarChar).Value = drOrder("DETAILNO")
                .Add("SHIPORDER", SqlDbType.NVarChar).Value = drOrder("SHIPORDER")
                .Add("TANKNO", SqlDbType.NVarChar).Value = drOrder("TANKNO")
                .Add("KAMOKU", SqlDbType.NVarChar).Value = drOrder("KAMOKU")
                .Add("KAISOUINFO", SqlDbType.NVarChar).Value = drOrder("KAISOUINFO")
                .Add("CARSNUMBER", SqlDbType.NVarChar).Value = drOrder("CARSNUMBER")
                .Add("REMARK", SqlDbType.NVarChar).Value = drOrder("REMARK")
                .Add("ACTUALDEPDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("ACTUALDEPDATE"))
                .Add("ACTUALARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALARRDATE"))
                .Add("ACTUALACCDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("ACTUALACCDATE"))
                .Add("ACTUALEMPARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALEMPARRDATE"))
                .Add("SALSE", SqlDbType.NVarChar).Value = drOrder("SALSE")
                .Add("SALSETAX", SqlDbType.NVarChar).Value = drOrder("SALSETAX")
                .Add("TOTALSALSE", SqlDbType.NVarChar).Value = drOrder("TOTALSALSE")
                .Add("PAYMENT", SqlDbType.NVarChar).Value = drOrder("PAYMENT")
                .Add("PAYMENTTAX", SqlDbType.NVarChar).Value = drOrder("PAYMENTTAX")
                .Add("TOTALPAYMENT", SqlDbType.NVarChar).Value = drOrder("TOTALPAYMENT")
                .Add("DELFLG", SqlDbType.NVarChar).Value = drOrder("DELFLG")
                .Add("INITYMD", SqlDbType.NVarChar).Value = drOrder("INITYMD")
                .Add("INITUSER", SqlDbType.NVarChar).Value = drOrder("INITUSER")
                .Add("INITTERMID", SqlDbType.NVarChar).Value = drOrder("INITTERMID")
                .Add("UPDYMD", SqlDbType.NVarChar).Value = drOrder("UPDYMD")
                .Add("UPDUSER", SqlDbType.NVarChar).Value = drOrder("UPDUSER")
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = drOrder("UPDTERMID")
                .Add("RECEIVEYMD", SqlDbType.NVarChar).Value = drOrder("RECEIVEYMD")
            End With
            sqlDetailCmd.CommandTimeout = 300
            sqlDetailCmd.ExecuteNonQuery()
        End Using

    End Sub

    ''' <summary>
    ''' OT空回日報履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト(トランザクションを利用しない場合はNothing指定)</param>
    ''' <param name="drOrder">履歴用の受注テーブル行オブジェクト</param>
    ''' <remarks>通常のOT空回日報受信テーブルに「履歴番号」と「画面ID」(呼出し側のMe.Title）
    ''' フィールドを追加したデータ行オブジェクト</remarks>
    Public Shared Sub InsertOTNippouHistory(sqlCon As SqlConnection, sqlTran As SqlTransaction, drOrder As DataRow)
        '◯OT空回日報TBL
        Dim sqlOTNippouStat As New StringBuilder
        sqlOTNippouStat.AppendLine("INSERT INTO OIL.HIS0005_OTNIPPOU")
        sqlOTNippouStat.AppendLine("   (DATERECEIVEYMD,COMPANYNAME,TCODE,STATIONNAME,STATIONCODE,OFFICENAME,OFFICECODE,")
        sqlOTNippouStat.AppendLine("    TRAINNO,LODDATE,DEPDATE,ARRDATE,ACCDATE,TANKCOUNT,SHIPPERSNAME,SHIPPERSCODE,")
        sqlOTNippouStat.AppendLine("    ARRSTATIONNAME,ARRSTATIONCODE,OILCOUNT,OTOILNAME,OTOILCODE,TANKNUMBER,PREOILNAME,")
        sqlOTNippouStat.AppendLine("    RANKING,INSPECTIONDATE,RETUNEDATE,RETRAINNO,JOINTNAME,JOINTCODE,WARIATE,KIJI,BIKOU,")
        sqlOTNippouStat.AppendLine("    MAINOFFICE,FILENAME,RECEIVECOUNT,LASTRECEIVEYMD,DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlOTNippouStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD)")
        sqlOTNippouStat.AppendLine("    VALUES")
        sqlOTNippouStat.AppendLine("   (@DATERECEIVEYMD,@COMPANYNAME,@TCODE,@STATIONNAME,@STATIONCODE,@OFFICENAME,@OFFICECODE,")
        sqlOTNippouStat.AppendLine("    @TRAINNO,@LODDATE,@DEPDATE,@ARRDATE,@ACCDATE,@TANKCOUNT,@SHIPPERSNAME,@SHIPPERSCODE,")
        sqlOTNippouStat.AppendLine("    @ARRSTATIONNAME,@ARRSTATIONCODE,@OILCOUNT,@OTOILNAME,@OTOILCODE,@TANKNUMBER,@PREOILNAME,")
        sqlOTNippouStat.AppendLine("    @RANKING,@INSPECTIONDATE,@RETUNEDATE,@RETRAINNO,@JOINTNAME,@JOINTCODE,@WARIATE,@KIJI,@BIKOU,")
        sqlOTNippouStat.AppendLine("    @MAINOFFICE,@FILENAME,@RECEIVECOUNT,@LASTRECEIVEYMD,@DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlOTNippouStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD)")

        Using sqlOTNippouCmd As New SqlCommand(sqlOTNippouStat.ToString, sqlCon, sqlTran)
            With sqlOTNippouCmd.Parameters
                .Add("DATERECEIVEYMD", SqlDbType.DateTime).Value = drOrder("DATERECEIVEYMD")
                .Add("COMPANYNAME", SqlDbType.NVarChar).Value = drOrder("COMPANYNAME")
                .Add("TCODE", SqlDbType.NVarChar).Value = drOrder("TCODE")
                .Add("STATIONNAME", SqlDbType.NVarChar).Value = drOrder("STATIONNAME")
                .Add("STATIONCODE", SqlDbType.NVarChar).Value = drOrder("STATIONCODE")
                .Add("OFFICENAME", SqlDbType.NVarChar).Value = drOrder("OFFICENAME")
                .Add("OFFICECODE", SqlDbType.NVarChar).Value = drOrder("OFFICECODE")
                .Add("TRAINNO", SqlDbType.NVarChar).Value = drOrder("TRAINNO")
                .Add("LODDATE", SqlDbType.NVarChar).Value = drOrder("LODDATE")
                .Add("DEPDATE", SqlDbType.NVarChar).Value = drOrder("DEPDATE")
                .Add("ARRDATE", SqlDbType.NVarChar).Value = drOrder("ARRDATE")
                .Add("ACCDATE", SqlDbType.NVarChar).Value = drOrder("ACCDATE")
                .Add("TANKCOUNT", SqlDbType.NVarChar).Value = drOrder("TANKCOUNT")
                .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = drOrder("SHIPPERSNAME")
                .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = drOrder("SHIPPERSCODE")
                .Add("ARRSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("ARRSTATIONNAME")
                .Add("ARRSTATIONCODE", SqlDbType.NVarChar).Value = drOrder("ARRSTATIONCODE")
                .Add("OILCOUNT", SqlDbType.NVarChar).Value = drOrder("OILCOUNT")
                .Add("OTOILNAME", SqlDbType.NVarChar).Value = drOrder("OTOILNAME")
                .Add("OTOILCODE", SqlDbType.NVarChar).Value = drOrder("OTOILCODE")
                .Add("TANKNUMBER", SqlDbType.NVarChar).Value = drOrder("TANKNUMBER")
                .Add("PREOILNAME", SqlDbType.NVarChar).Value = drOrder("PREOILNAME")
                .Add("RANKING", SqlDbType.NVarChar).Value = drOrder("RANKING")
                .Add("INSPECTIONDATE", SqlDbType.NVarChar).Value = drOrder("INSPECTIONDATE")
                .Add("RETUNEDATE", SqlDbType.NVarChar).Value = drOrder("RETUNEDATE")
                .Add("RETRAINNO", SqlDbType.NVarChar).Value = drOrder("RETRAINNO")
                .Add("JOINTNAME", SqlDbType.NVarChar).Value = drOrder("JOINTNAME")
                .Add("JOINTCODE", SqlDbType.NVarChar).Value = drOrder("JOINTCODE")
                .Add("WARIATE", SqlDbType.NVarChar).Value = drOrder("WARIATE")
                .Add("KIJI", SqlDbType.NVarChar).Value = drOrder("KIJI")
                .Add("BIKOU", SqlDbType.NVarChar).Value = drOrder("BIKOU")
                .Add("MAINOFFICE", SqlDbType.NVarChar).Value = drOrder("MAINOFFICE")
                .Add("FILENAME", SqlDbType.NVarChar).Value = drOrder("FILENAME")
                .Add("RECEIVECOUNT", SqlDbType.Int).Value = drOrder("RECEIVECOUNT")
                .Add("LASTRECEIVEYMD", SqlDbType.DateTime).Value = drOrder("LASTRECEIVEYMD")
                .Add("DELFLG", SqlDbType.NVarChar).Value = drOrder("DELFLG")
                .Add("INITYMD", SqlDbType.DateTime).Value = drOrder("INITYMD")
                .Add("INITUSER", SqlDbType.NVarChar).Value = drOrder("INITUSER")
                .Add("INITTERMID", SqlDbType.NVarChar).Value = drOrder("INITTERMID")
                .Add("UPDYMD", SqlDbType.DateTime).Value = drOrder("UPDYMD")
                .Add("UPDUSER", SqlDbType.NVarChar).Value = drOrder("UPDUSER")
                .Add("UPDTERMID", SqlDbType.NVarChar).Value = drOrder("UPDTERMID")
                .Add("RECEIVEYMD", SqlDbType.DateTime).Value = drOrder("RECEIVEYMD")
            End With
            sqlOTNippouCmd.CommandTimeout = 300
            sqlOTNippouCmd.ExecuteNonQuery()
        End Using

    End Sub

End Class

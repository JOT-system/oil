Public Class ReportSignSQL
    ''' <summary>
    ''' 空回日報(帳票)表示用SQL
    ''' </summary>
    ''' <param name="mapID">画面ID</param>
    ''' <remarks>空回日報の帳票を表示する際のSQLを設定</remarks>
    Public Function EmptyTurnDairy(ByVal mapID As String)

        Dim SQLStr As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIT0002_OTHER.OFFICECODE                       AS OFFICECODE" _
            & " , OIT0002_OTHER.OFFICENAME                       AS OFFICENAME" _
            & " , OIT0002_OTHER.TRAINNO                          AS TRAINNO" _
            & " , OIT0002_OTHER.TRAINNAME                        AS TRAINNAME" _
            & " , OIT0003.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , OIT0002_OTHER.BASECODE                         AS BASECODE" _
            & " , OIT0002_OTHER.BASENAME                         AS BASENAME" _
            & " , OIT0002_OTHER.CONSIGNEECODE                    AS CONSIGNEECODE" _
            & " , OIT0002_OTHER.CONSIGNEENAME                    AS CONSIGNEENAME" _
            & " , OIT0002_OTHER.DEPSTATION                       AS DEPSTATION" _
            & " , OIT0002_OTHER.DEPSTATIONNAME                   AS DEPSTATIONNAME" _
            & " , OIT0002_OTHER.ARRSTATION                       AS ARRSTATION" _
            & " , OIT0002_OTHER.ARRSTATIONNAME                   AS ARRSTATIONNAME" _
            & " , OIT0002_OTHER.LODDATE                          AS LODDATE" _
            & " , OIT0002_OTHER.DEPDATE                          AS DEPDATE" _
            & " , OIT0002_OTHER.ARRDATE                          AS ARRDATE" _
            & " , OIT0002_OTHER.ACCDATE                          AS ACCDATE" _
            & " , OIT0002.EMPARRDATE                             AS EMPARRDATE" _
            & " , OIT0003.ACTUALLODDATE                          AS ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE                          AS ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE                          AS ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE                          AS ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE                       AS ACTUALEMPARRDATE" _
            & " , OIM0005.MODEL                                  AS MODEL" _
            & " , OIT0003.TANKNO                                 AS TANKNO" _
            & " , OIT0003.CARSNUMBER                             AS CARSNUMBER" _
            & " , OIT0003.CARSAMOUNT                             AS CARSAMOUNT" _
            & " , OIM0005.LOAD                                   AS LOAD" _
            & " , OIM0005.OWNERCODE                              AS OWNERCODE" _
            & " , OIM0005.OWNERNAME                              AS OWNERNAME" _
            & " , OIM0005.LEASECODE                              AS LEASECODE" _
            & " , OIM0005.LEASENAME                              AS LEASENAME" _
            & " , OIT0003.SHIPORDER                              AS SHIPORDER" _
            & " , OIT0003.LINEORDER                              AS LINEORDER" _
            & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
            & " , OIM0005.JRALLINSPECTIONDATE                    AS JRALLINSPECTIONDATE" _
            & " , OIT0003.RETURNDATETRAIN                        AS RETURNDATETRAIN" _
            & " , ISNULL(OIT0003.RETURNDATETRAIN, OIT0002.BTRAINNO) AS RETURNDATETRAINNO" _
            & " , OIT0003.JOINTCODE                              AS JOINTCODE" _
            & " , OIT0003.JOINT                                  AS JOINT" _
            & " , OIT0003.REMARK                                 AS REMARK" _
            & " , OIM0003.BIGOILCODE                             AS BIGOILCODE" _
            & " , OIM0003.BIGOILNAME                             AS BIGOILNAME" _
            & " , OIM0003.MIDDLEOILCODE                          AS MIDDLEOILCODE" _
            & " , OIM0003.MIDDLEOILNAME                          AS MIDDLEOILNAME" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIM0003.OTOILCODE                              AS OTOILCODE" _
            & " , OIM0003.OTOILNAME                              AS OTOILNAME" _
            & " , OIM0003.SHIPPEROILCODE                         AS SHIPPEROILCODE" _
            & " , OIM0003.SHIPPEROILNAME                         AS SHIPPEROILNAME" _
            & " , OIM0003.CHECKOILCODE                           AS CHECKOILCODE" _
            & " , OIM0003.CHECKOILNAME                           AS CHECKOILNAME" _
            & " , OIT0005.LASTOILCODE                            AS LASTOILCODE" _
            & " , OIT0005.LASTOILNAME                            AS LASTOILNAME" _
            & " , OIT0005.PREORDERINGTYPE                        AS PREORDERINGTYPE" _
            & " , OIT0005.PREORDERINGOILNAME                     AS PREORDERINGOILNAME" _
            & " , OTOILCT.OTOILCODE                              AS OTOILCTCODE" _
            & " , OTOILCT.CNT                                    AS OTOILCTCNT" _
            & " , OIM0026.DELIVERYCODE                           AS DELIVERYCODE" _
            & " , OIM0012.KUUKAICONSIGNEENAME                    AS KUUKAICONSIGNEENAME" _
            & " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "     (OIT0003.ORDERNO = OIT0002.ORDERNO OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIT0002_ORDER OIT0002_OTHER ON " _
            & "     OIT0002_OTHER.ORDERNO = OIT0003.ORDERNO " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0003.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIM0005.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIT0005_SHOZAI OIT0005 ON " _
            & "     OIT0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIT0005.DELFLG <> @P02 "

        '### 20200917 START 指摘票対応(No138)全体 ###################################################
        SQLStr &=
              " LEFT JOIN oil.OIM0026_DELIVERY OIM0026 ON " _
            & "     OIM0026.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0026.TRAINNAME = OIT0003.LOADINGIRILINETRAINNAME " _
            & " AND OIM0026.LINEORDER = OIT0003.LINEORDER " _
            & " AND OIM0026.DELFLG <> @P02 "
        '### 20200917 END   指摘票対応(No138)全体 ###################################################

        '### 20201008 START 指摘票対応(No157)全体 ###################################################
        SQLStr &=
              " LEFT JOIN oil.OIM0012_NIUKE OIM0012 ON " _
            & "     OIM0012.CONSIGNEECODE = OIT0002.CONSIGNEECODE " _
            & " AND OIM0012.DELFLG <> @P02 "
        '### 20201008 END   指摘票対応(No157)全体 ###################################################

        SQLStr &=
              " LEFT JOIN ( " _
            & "   SELECT " _
            & "         OIT0002.ORDERNO " _
            & "       , OIT0003.SHIPPERSCODE " _
            & "       , OIT0003.SHIPPERSNAME " _
            & "       , OIM0003.OTOILCODE " _
            & "       , OIM0003.OTOILNAME " _
            & "       , COUNT(1) AS CNT " _
            & "   FROM oil.OIT0002_ORDER OIT0002 " _
            & "   INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "       (OIT0003.ORDERNO = OIT0002.ORDERNO OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & "   AND OIT0003.DELFLG <> @P02 " _
            & "   INNER JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "       OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "   AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & "   AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & "   AND OIM0003.DELFLG <> @P02 "

        Select Case mapID
            '空回日報画面(ダウンロード)より出力
            Case "OIT0001"
                SQLStr &=
                  "   WHERE OIT0002.ORDERNO = @P01 "

            '受注一覧画面(帳票)より出力
            Case "OIT0003"
                SQLStr &=
                  "   WHERE OIT0002.OFFICECODE = @P03 " _
                & "   AND OIT0002.DELFLG <> @P02 " _
                & "   AND OIT0002.LODDATE = @P04 " _
                & "   AND OIT0002.ORDERSTATUS <> @P05 "

        End Select

        SQLStr &=
              "   GROUP BY " _
            & "         OIT0002.ORDERNO " _
            & "       , OIT0003.SHIPPERSCODE " _
            & "       , OIT0003.SHIPPERSNAME " _
            & "       , OIM0003.OTOILCODE " _
            & "       , OIM0003.OTOILNAME " _
            & " ) OTOILCT ON " _
            & "     OTOILCT.SHIPPERSCODE = OIT0003.SHIPPERSCODE " _
            & " AND OTOILCT.OTOILCODE = OIM0003.OTOILCODE "

        Select Case mapID
            '空回日報画面(ダウンロード)より出力
            Case "OIT0001"
                SQLStr &=
                  " WHERE OIT0002.ORDERNO = @P01 " _
                & " AND OIT0002.DELFLG <> @P02 "

            '受注一覧画面(帳票)より出力
            Case "OIT0003"
                SQLStr &=
                  " WHERE OIT0002.OFFICECODE = @P03 " _
                & " AND OIT0002.DELFLG <> @P02 " _
                & " AND OIT0002.LODDATE = @P04 " _
                & " AND OIT0002.ORDERSTATUS <> @P05 "
        End Select

        SQLStr &=
                " ORDER BY" _
            & "    OIT0003.SHIPPERSCODE" _
            & "  , OIT0002.DEPSTATION" _
            & "  , OIM0003.OTOILCODE" _
            & "  , OIT0003.LINEORDER" _
            & "  , OIT0003.TANKNO"

        Return SQLStr
    End Function
End Class

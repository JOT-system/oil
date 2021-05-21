Public Class ReportSignSQL
    ''' <summary>
    ''' 空回日報(帳票)表示用SQL
    ''' </summary>
    ''' <param name="mapID">画面ID</param>
    ''' <remarks>空回日報の帳票を表示する際のSQLを設定</remarks>
    Public Function EmptyTurnDairy(ByVal mapID As String, Optional ByVal dt As DataTable = Nothing)

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
            & " , OIT0002_OTHER.EMPARRDATE                       AS EMPARRDATE" _
            & " , OIT0003.ACTUALLODDATE                          AS ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE                          AS ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE                          AS ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE                          AS ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE                       AS ACTUALEMPARRDATE" _
            & " , OIT0003.UPGRADEFLG                             AS UPGRADEFLG " _
            & " , OIT0003.OTTRANSPORTFLG                         AS OTTRANSPORTFLG " _
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
            & " , ''                                             AS OTRANK" _
            & " , OIT0003.LOADINGIRILINETRAINNO                  AS LOADINGIRILINETRAINNO" _
            & " , OIT0003.LOADINGOUTLETTRAINNO                   AS LOADINGOUTLETTRAINNO" _
            & " , OIM0005.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
            & " , OIM0005.JRALLINSPECTIONDATE                    AS JRALLINSPECTIONDATE" _
            & " , OIT0003.RETURNDATETRAIN                        AS RETURNDATETRAIN" _
            & " , ISNULL(OIT0003.RETURNDATETRAIN, OIT0002.BTRAINNO) AS RETURNDATETRAINNO" _
            & " , OIT0003.JOINTCODE                              AS JOINTCODE" _
            & " , OIT0003.JOINT                                  AS JOINT" _
            & " , OIT0003.REMARK                                 AS REMARK" _
            & " , OIT0003.SECONDCONSIGNEECODE                    AS SECONDCONSIGNEECODE" _
            & " , OIT0003.SECONDCONSIGNEENAME                    AS SECONDCONSIGNEENAME" _
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
            & " , OIM0003_LAST.OTOILCODE                         AS LASTOTOILCODE" _
            & " , OIM0003_LAST.OTOILNAME                         AS LASTOTOILNAME" _
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
            & " AND OIT0005.DELFLG <> @P02 " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003_LAST ON " _
            & "     OIM0003_LAST.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003_LAST.OILCODE = OIT0005.LASTOILCODE " _
            & " AND OIM0003_LAST.SEGMENTOILCODE = OIT0005.PREORDERINGTYPE " _
            & " AND OIM0003_LAST.DELFLG <> @P02 "

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
            '空回日報一覧画面(ダウンロード)より出力
            Case "OIT0001L"
                SQLStr &=
                  "   WHERE OIT0002.ORDERNO IN ( "
                For Each dtrow As DataRow In dt.Select("OPERATION='on'")
                    If dtrow("LINECNT") = 1 Then
                        SQLStr &= "'" + dtrow("ORDERNO") + "'"
                    Else
                        SQLStr &= ",'" + dtrow("ORDERNO") + "'"
                    End If
                Next
                SQLStr &= ")" & " AND OIT0002.DELFLG <> @P02 "

            '空回日報明細画面(ダウンロード)より出力
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
            & "     OTOILCT.ORDERNO = OIT0002.ORDERNO " _
            & " AND OTOILCT.SHIPPERSCODE = OIT0003.SHIPPERSCODE " _
            & " AND OTOILCT.OTOILCODE = OIM0003.OTOILCODE "

        Select Case mapID
            '空回日報一覧画面(ダウンロード)より出力
            Case "OIT0001L"
                SQLStr &=
                  "   WHERE OIT0002.ORDERNO IN ( "
                For Each dtrow As DataRow In dt.Select("OPERATION='on'")
                    If dtrow("LINECNT") = 1 Then
                        SQLStr &= "'" + dtrow("ORDERNO") + "'"
                    Else
                        SQLStr &= ",'" + dtrow("ORDERNO") + "'"
                    End If
                Next
                SQLStr &= ")" & " AND OIT0002.DELFLG <> @P02 "

            '空回日報明細画面(ダウンロード)より出力
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
            & "    OIT0002.LODDATE" _
            & "  , OIT0003.SHIPPERSCODE" _
            & "  , OIT0002.TRAINNO" _
            & "  , OIT0002.DEPSTATION" _
            & "  , OIM0003.OTOILCODE" _
            & "  , CONVERT(INT,OIT0003.LINEORDER)" _
            & "  , OIT0003.TANKNO"

        Return SQLStr
    End Function

    ''' <summary>
    ''' ポラリス投入用(ダウンロード)SQL
    ''' </summary>
    ''' <param name="I_AricleName">品名</param>
    ''' <param name="I_ObjectiveName">指示内容</param>
    ''' <remarks>ポラリス投入用ファイルをダウンロードする際のSQLを設定</remarks>
    Public Function PolarisDownload(ByVal I_AricleName() As String, ByVal I_ObjectiveName() As String)

        '○ 取得SQL
        '　 説明　：　ポラリス投入用ファイルダウンロードSQL
        Dim SQLStr As String =
        " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , OIT0011.TRAINNO                                AS TRAINNO" _
            & " , OIT0011.CONVENTIONAL                           AS CONVENTIONAL" _
            & " , OIT0011.CONVENTIONALTIME                       AS CONVENTIONALTIME" _
            & " , OIT0011.AGOBEHINDFLG                           AS AGOBEHINDFLG" _
            & " , OIT0011.REGISTRATIONDATE                       AS REGISTRATIONDATE" _
            & " , OIT0011.SERIALNUMBER                           AS SERIALNUMBER" _
            & " , OIT0011.TRUCKSYMBOL                            AS TRUCKSYMBOL" _
            & " , OIT0011.TRUCKNO                                AS TRUCKNO" _
            & " , OIT0011.DEPSTATIONNAME                         AS DEPSTATIONNAME" _
            & " , OIT0011.ARRSTATIONNAME                         AS ARRSTATIONNAME"

        '### 20201021 START 指摘票対応(No183)全体 #############################################
        'SQLStr &=
        '      " , OIT0011.ARTICLENAME                            AS ARTICLENAME"
        SQLStr &=
              " , CASE ISNULL(RTRIM(OIT0005.TANKSITUATION), '')" _
            & "   WHEN @TANKSITUATION THEN '" & I_AricleName(0) & "'" _
            & "   ELSE OIT0011.ARTICLENAME" _
            & "   END                                            AS ARTICLENAME"
        '### 20201021 END   指摘票対応(No183)全体 #############################################

        '### 20210330 START ﾀｷ1000以外は交検を未反映 ##########################################
        'SQLStr &=
        '      " , ISNULL(OIT0011.INSPECTIONDATE, OIM0005.JRINSPECTIONDATE) AS INSPECTIONDATE"
        SQLStr &=
              " , CASE" _
            & "   WHEN SUBSTRING(TRUCKSYMBOL,1,2) = 'ｺｷ' OR SUBSTRING(TRUCKSYMBOL,1,1) = 'ﾁ' THEN NULL" _
            & "   ELSE ISNULL(OIT0011.INSPECTIONDATE, OIM0005.JRINSPECTIONDATE)" _
            & "   END                                            AS INSPECTIONDATE"
        '### 20210330 END   ﾀｷ1000以外は交検を未反映 ##########################################
        SQLStr &=
              " , OIT0011.CONVERSIONAMOUNT                       AS CONVERSIONAMOUNT" _
            & " , OIT0011.ARTICLE                                AS ARTICLE" _
            & " , OIT0011.CURRENTCARTOTAL                        AS CURRENTCARTOTAL" _
            & " , OIT0011.EXTEND                                 AS EXTEND" _
            & " , OIT0011.CONVERSIONTOTAL                        AS CONVERSIONTOTAL" _
            & " , OIT0011.OBJECTIVENAME                          AS OBJECTIVENAME" _
            & " , OIT0011.DAILYREPORTCODE                        AS DAILYREPORTCODE" _
            & " , OIT0011.DAILYREPORTOILNAME                     AS DAILYREPORTOILNAME" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME"

        '### 20210330 START ﾀｷ1000以外は前回油種を未反映 ######################################
        '### 20201021 START 指摘票対応(No189)全体 #############################################
        'SQLStr &=
        '      " , OIT0005_LASTOIL.LASTOILCODE                    AS LASTOILCODE" _
        '    & " , OIT0005_LASTOIL.LASTOILNAME                    AS LASTOILNAME" _
        '    & " , OIT0005_LASTOIL.PREORDERINGTYPE                AS PREORDERINGTYPE" _
        '    & " , OIT0005_LASTOIL.PREORDERINGOILNAME             AS PREORDERINGOILNAME"
        SQLStr &=
              " , OIT0005_LASTOIL.LASTOILCODE                    AS LASTOILCODE" _
            & " , OIT0005_LASTOIL.LASTOILNAME                    AS LASTOILNAME" _
            & " , OIT0005_LASTOIL.PREORDERINGTYPE                AS PREORDERINGTYPE" _
            & " , CASE" _
            & "   WHEN SUBSTRING(TRUCKSYMBOL,1,2) = 'ｺｷ' OR SUBSTRING(TRUCKSYMBOL,1,1) = 'ﾁ' THEN NULL" _
            & "   ELSE OIT0005_LASTOIL.PREORDERINGOILNAME" _
            & "   END                                            AS PREORDERINGOILNAME"
        '### 20201021 END   指摘票対応(No189)全体 #############################################
        '### 20210330 END   ﾀｷ1000以外は前回油種を未反映 ######################################

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              " , OIM0029.VALUE02                                AS REPORTOILNAME" _
            & " , OIM0029.VALUE05                                AS RINKAIOILKANA" _
            & " , OIM0029.VALUE06                                AS RINKAISEGMENTOILNAME"
        'SQLStr &=
        '      " , TMP0005.REPORTOILNAME                          AS REPORTOILNAME" _
        '    & " , TMP0005.RINKAIOILKANA                          AS RINKAIOILKANA" _
        '    & " , TMP0005.RINKAISEGMENTOILNAME                   AS RINKAISEGMENTOILNAME"
        '### 20201002 END   変換マスタに移行したため修正 ########################

        SQLStr &=
              " , OIT0011.POSITION                               AS POSITION" _
            & " , OIT0003.FILLINGPOINT                           AS FILLINGPOINT" _
            & " , OIT0003.LINE                                   AS LINE" _
            & " , OIT0003.LOADINGIRILINETRAINNO                  AS LOADINGIRILINETRAINNO" _
            & " , OIT0002.ARRSTATIONNAME                         AS LOADINGARRSTATIONNAME" _
            & " , OIT0011.LOADINGKTRAINNO                        AS LOADINGKTRAINNO" _
            & " , CASE " _
            & "   WHEN OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(2) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(3) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(8) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(4) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(5) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(6) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(7) & "' THEN OIT0011.LOADINGTRAINNO" _
            & "   ELSE OIT0011.LOADINGOTTRAINNO" _
            & "   END                                            AS ORDERTRAINNO " _
            & " , CASE " _
            & "   WHEN OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(2) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(3) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(8) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(4) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(5) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(6) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(7) & "' THEN OIT0011.LOADINGLODDATE" _
            & "   ELSE FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd')" _
            & "   END                                            AS ORDERLODDATE " _
            & " , CASE " _
            & "   WHEN OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(2) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(3) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(8) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(4) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(5) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(6) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(7) & "' THEN OIT0011.LOADINGDEPDATE" _
            & "   ELSE FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd')" _
            & "   END                                            AS ORDERDEPDATE " _
            & " , OIT0011.FORWARDINGARRSTATION                   AS FORWARDINGARRSTATION" _
            & " , OIT0011.FORWARDINGCONFIGURE                    AS FORWARDINGCONFIGURE" _
            & " , OIT0002.ORDERNO                                AS ORDERNO " _
            & " , OIT0003.DETAILNO                               AS DETAILNO " _
            & " , ''                                             AS ORDERTRKBN " _
            & " , OIT0003.OTTRANSPORTFLG                         AS OTTRANSPORTFLG " _
            & " FROM oil.OIT0011_RLINK OIT0011 " _
            & " LEFT JOIN oil.OIT0002_ORDER OIT0002 ON " _
            & "     OIT0002.ORDERNO = OIT0011.ORDERNO " _
            & " AND OIT0002.DELFLG <> @DELFLG " _
            & " LEFT JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0011.ORDERNO " _
            & " AND OIT0003.DETAILNO = OIT0011.DETAILNO " _
            & " AND OIT0003.DELFLG <> @DELFLG "

        '### 20201002 START 変換マスタに移行したため修正 ########################
        SQLStr &=
              " LEFT JOIN oil.OIM0029_CONVERT OIM0029 ON " _
            & "     OIM0029.CLASS = 'RINKAI_OILMASTER' " _
            & " AND OIM0029.KEYCODE01 = OIT0002.OFFICECODE " _
            & " AND OIM0029.KEYCODE04 = '1' " _
            & " AND OIM0029.KEYCODE05 = OIT0003.OILCODE " _
            & " AND OIM0029.KEYCODE08 = OIT0003.ORDERINGTYPE "
        'SQLStr &=
        '      " LEFT JOIN oil.TMP0005OILMASTER TMP0005 ON " _
        '    & "     TMP0005.OFFICECODE = OIT0002.OFFICECODE " _
        '    & " AND TMP0005.OILNo = '1' " _
        '    & " AND TMP0005.OILCODE = OIT0003.OILCODE " _
        '    & " AND TMP0005.SEGMENTOILCODE = OIT0003.ORDERINGTYPE "
        '### 20201002 END   変換マスタに移行したため修正 ########################

        '### 20201021 START 指摘票対応(No183)全体 #############################################
        SQLStr &=
                  " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
                & "     OIT0011.TRUCKNO = OIT0005.TANKNUMBER " _
                & " AND OIT0005.TANKSITUATION = @TANKSITUATION " _
                & " AND OIT0005.DELFLG <> @DELFLG "
        '### 20201021 END   指摘票対応(No183)全体 #############################################

        '### 20201021 START 指摘票対応(No189)全体 #############################################
        SQLStr &=
                  " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005_LASTOIL ON " _
                & "     OIT0011.TRUCKNO = OIT0005_LASTOIL.TANKNUMBER " _
                & " AND OIT0005_LASTOIL.DELFLG <> @DELFLG "
        '### 20201021 END   指摘票対応(No189)全体 #############################################

        SQLStr &=
              " LEFT JOIN oil.OIM0005_TANK OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0011.TRUCKNO " _
            & " AND OIM0005.DELFLG <> @DELFLG " _
            & " WHERE OIT0011.RLINKNO = @RLINKNO "

        Return SQLStr
    End Function

    ''' <summary>
    ''' 受注明細画面(費用)表示用SQL
    ''' </summary>
    ''' <param name="I_OFFICECODE">受注営業所コード</param>
    ''' <param name="I_TRKBN">輸送区分(１：OT輸送あり, ２：OT輸送なし)</param>
    ''' <remarks>受注明細画面(タブ(費用))を表示する際のSQLを設定</remarks>
    Public Function OrderRequestAccountDetail(ByVal I_OFFICECODE As String, ByVal I_TRKBN As String)
        '○ 取得SQL
        '　 説明　：　受注明細画面(費用)表示用SQL
        Dim SQLStr As String = ""
        Dim SQLSelectStr As String = ""
        Dim SQLFromStr1 As String = ""
        Dim SQLFromStr2 As String = ""

        '共通SELECT用
        SQLSelectStr =
          " SELECT" _
        & "   0                                                     AS LINECNT" _
        & " , ''                                                    AS OPERATION" _
        & " , ''                                                    AS TIMSTP" _
        & " , 1                                                     AS 'SELECT'" _
        & " , 0                                                     AS HIDDEN" _
        & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')                    AS ORDERNO" _
        & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')                   AS DETAILNO" _
        & " , ISNULL(RTRIM(VIW0012.PATCODE), '')                    AS PATCODE" _
        & " , ISNULL(RTRIM(VIW0012.PATNAME), '')                    AS PATNAME" _
        & " , ISNULL(RTRIM(VIW0012.ACCOUNTCODE), '')                AS ACCOUNTCODE" _
        & " , ISNULL(RTRIM(VIW0012.ACCOUNTNAME), '')                AS ACCOUNTNAME" _
        & " , ISNULL(RTRIM(VIW0012.SEGMENTCODE), '')                AS SEGMENTCODE" _
        & " , ISNULL(RTRIM(VIW0012.SEGMENTNAME), '')                AS SEGMENTNAME" _
        & " , ISNULL(RTRIM(VIW0012.BREAKDOWNCODE), '')              AS BREAKDOWNCODE" _
        & " , ISNULL(RTRIM(VIW0012.BREAKDOWN), '')                  AS BREAKDOWN" _
        & " , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '')               AS SHIPPERSCODE" _
        & " , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '')               AS SHIPPERSNAME" _
        & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                   AS BASECODE" _
        & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                   AS BASENAME" _
        & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')                 AS OFFICECODE" _
        & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')                 AS OFFICENAME" _
        & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')                 AS DEPSTATION" _
        & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')             AS DEPSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')                 AS ARRSTATION" _
        & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')             AS ARRSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')              AS CONSIGNEECODE" _
        & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')              AS CONSIGNEENAME" _
        & " , ISNULL(RTRIM(OIT0003.ACTUALLODDATE), FORMAT(GETDATE(), 'yyyy/MM/dd'))             AS KEIJYOYMD" _
        & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                    AS TRAINNO" _
        & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')                  AS TRAINNAME" _
        & " , ISNULL(RTRIM(OIM0005.MODEL), '')                      AS MODEL" _
        & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                     AS TANKNO" _
        & " , ISNULL(RTRIM(OIT0003.STACKINGFLG), '')                AS STACKINGFLG" _
        & " , ISNULL(RTRIM(OIT0003.WHOLESALEFLG), '')               AS WHOLESALEFLG" _
        & " , ISNULL(RTRIM(OIT0003.INSPECTIONFLG), '')              AS INSPECTIONFLG" _
        & " , ISNULL(RTRIM(OIT0003.DETENTIONFLG), '')               AS DETENTIONFLG" _
        & " , ISNULL(RTRIM(OIT0003.FIRSTRETURNFLG), '')             AS FIRSTRETURNFLG" _
        & " , ISNULL(RTRIM(OIT0003.AFTERRETURNFLG), '')             AS AFTERRETURNFLG" _
        & " , ISNULL(RTRIM(OIT0003.OTTRANSPORTFLG), '')             AS OTTRANSPORTFLG" _
        & " , ISNULL(RTRIM(OIT0003.UPGRADEFLG), '')                 AS UPGRADEFLG" _
        & " , ISNULL(RTRIM(OIT0003.CARSNUMBER), '')                 AS CARSNUMBER" _
        & " , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '')                 AS CARSAMOUNT" _
        & " , ISNULL(RTRIM(OIM0005.LOAD), '')                       AS LOAD" _
        & " , ISNULL(RTRIM(OIT0003.JOINTCODE), '')                  AS JOINTCODE" _
        & " , ISNULL(RTRIM(OIT0003.JOINT), '')                      AS JOINT" _
        & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                    AS OILCODE" _
        & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                    AS OILNAME" _
        & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')               AS ORDERINGTYPE" _
        & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')            AS ORDERINGOILNAME" _
        & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNO), '')              AS CHANGETRAINNO" _
        & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNAME), '')            AS CHANGETRAINNAME" _
        & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '')        AS SECONDCONSIGNEECODE" _
        & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '')        AS SECONDCONSIGNEENAME" _
        & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATION), '')           AS SECONDARRSTATION" _
        & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATIONNAME), '')       AS SECONDARRSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATION), '')           AS CHANGERETSTATION" _
        & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATIONNAME), '')       AS CHANGERETSTATIONNAME" _
        & " , ISNULL(RTRIM(VIW0012.TRKBN), '')                      AS TRKBN" _
        & " , ISNULL(RTRIM(VIW0012.TRKBNNAME), '')                  AS TRKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.KIRO), '')                       AS KIRO" _
        & " , ''                                                    AS BRANCH" _
        & " , ISNULL(RTRIM(VIW0012.CALCKBN), '')                    AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.CALCKBNNAME), '')                AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.JROILTYPE), '')                  AS JROILTYPE" _
        & " , ISNULL(RTRIM(VIW0012.FARE), '')                       AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT1), '')                  AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT2), '')                  AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT3), '')                  AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT4), '')                  AS DISCOUNT4" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT5), '')                  AS DISCOUNT5" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT6), '')                  AS DISCOUNT6" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNT7), '')                  AS DISCOUNT7" _
        & " , ISNULL(RTRIM(VIW0012.DISCOUNTFARE), '')               AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.INVOICECODE), '')                AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.INVOICENAME), '')                AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.INVOICEDEPTNAME), '')            AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.PAYEECODE), '')                  AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.PAYEENAME), '')                  AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.PAYEEDEPTNAME), '')              AS PAYEEDEPTNAME" _
        & " FROM OIL.OIT0002_ORDER OIT0002 " _
        & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        & "       OIT0003.ORDERNO = OIT0002.ORDERNO" _
        & "       AND OIT0003.DELFLG <> @P02" _
        & " INNER JOIN OIL.OIM0005_TANK OIM0005 ON " _
        & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
        & "       AND OIM0005.DELFLG <> @P02" _
        & " INNER JOIN OIL.VIW0012_ACCOUNTLIST VIW0012 ON " _
        & "       VIW0012.OFFICECODE = OIT0002.OFFICECODE" _
        & "       AND VIW0012.SHIPPERSCODE = OIT0003.SHIPPERSCODE" _
        & "       AND VIW0012.BASECODE = OIT0002.BASECODE" _
        & "       AND VIW0012.DEPSTATION = OIT0002.DEPSTATION" _
        & "       AND VIW0012.ARRSTATION = OIT0002.ARRSTATION" _
        & "       AND VIW0012.CONSIGNEECODE = OIT0002.CONSIGNEECODE " _
        & "       AND VIW0012.LOAD = OIM0005.LOAD" _
        & "       AND VIW0012.FROMYMD_RYOKIN <= OIT0003.ACTUALLODDATE" _
        & "       AND VIW0012.ENDYMD_RYOKIN >= OIT0003.ACTUALLODDATE"
        '& " , ISNULL(RTRIM(OIT0002.KEIJYOYMD), FORMAT(GETDATE(), 'yyyy/MM/dd'))             AS KEIJYOYMD" _
        '& "       AND VIW0012.CONSIGNEECODE = CASE WHEN OIT0003.SECONDCONSIGNEECODE = '' THEN OIT0002.CONSIGNEECODE ELSE OIT0003.SECONDCONSIGNEECODE END" _

        '★輸送形態が"M"(請負OT混載)の場合
        If I_OFFICECODE <> BaseDllConst.CONST_OFFICECODE_010402 AndAlso I_TRKBN = BaseDllConst.CONST_TRKBN_M Then
            SQLSelectStr &=
            "       AND VIW0012.TRKBN = CASE WHEN OIT0003.OTTRANSPORTFLG = '2' THEN 'C' ELSE 'O' END"
        End If

        '共通科目用SQL
        SQLStr =
            SQLSelectStr _
        & "       AND VIW0012.JROILTYPE = 'X'" _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '科目(危険品・普通品)用SQL
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & "       AND VIW0012.JROILTYPE <> 'X'" _
        & "       AND VIW0012.JROILTYPE = " _
        & String.Format("       CASE WHEN OIT0003.OILCODE = '{0}' OR OIT0003.OILCODE = '{1}' THEN 'D'",
                        BaseDllConst.CONST_HTank,
                        BaseDllConst.CONST_RTank) _
        & "       ELSE 'N' END " _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        Return SQLStr
    End Function


    ''' <summary>
    ''' 受注明細画面(費用)表示用SQL(旧バージョン)
    ''' </summary>
    ''' <remarks>受注明細画面(タブ(費用))を表示する際のSQLを設定</remarks>
    Public Function OrderRequestAccountDetail_OLD()
        '○ 取得SQL
        '　 説明　：　受注明細画面(費用)表示用SQL
        Dim SQLStr As String = ""
        Dim SQLSelectStr As String = ""
        Dim SQLFromStr1 As String = ""
        Dim SQLFromStr2 As String = ""

        '共通SELECT用
        SQLSelectStr =
          " SELECT" _
        & "   0                                                  AS LINECNT" _
        & " , ''                                                 AS OPERATION" _
        & " , ''                                                 AS TIMSTP" _
        & " , 1                                                  AS 'SELECT'" _
        & " , 0                                                  AS HIDDEN" _
        & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')                 AS ORDERNO" _
        & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')                AS DETAILNO" _
        & " , ISNULL(RTRIM(OIM0010.PATCODE), '')                 AS PATCODE" _
        & " , ISNULL(RTRIM(OIM0010.PATNAME), '')                 AS PATNAME" _
        & " , ISNULL(RTRIM(OIM0010.ACCOUNTCODE), '')             AS ACCOUNTCODE" _
        & " , ISNULL(RTRIM(VIW0012.ACCOUNTNAME), '')             AS ACCOUNTNAME" _
        & " , ISNULL(RTRIM(OIM0010.SEGMENTCODE), '')             AS SEGMENTCODE" _
        & " , ISNULL(RTRIM(VIW0012.SEGMENTNAME), '')             AS SEGMENTNAME" _
        & " , ISNULL(RTRIM(VIW0012.BREAKDOWNCODE), '')           AS BREAKDOWNCODE" _
        & " , ISNULL(RTRIM(VIW0012.BREAKDOWN), '')               AS BREAKDOWN" _
        & " , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
        & " , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
        & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                AS BASECODE" _
        & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                AS BASENAME" _
        & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')              AS OFFICECODE" _
        & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')              AS OFFICENAME" _
        & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')              AS DEPSTATION" _
        & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')              AS ARRSTATION" _
        & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')          AS ARRSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
        & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
        & " , ISNULL(RTRIM(OIT0002.KEIJYOYMD), FORMAT(GETDATE(), 'yyyy/MM/dd'))             AS KEIJYOYMD" _
        & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
        & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')               AS TRAINNAME" _
        & " , ISNULL(RTRIM(OIM0005.MODEL), '')                   AS MODEL" _
        & " , ISNULL(RTRIM(OIT0003.TANKNO), '')                  AS TANKNO" _
        & " , ISNULL(RTRIM(OIT0003.CARSNUMBER), '')              AS CARSNUMBER" _
        & " , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '')              AS CARSAMOUNT" _
        & " , ISNULL(RTRIM(OIM0005.LOAD), '')                    AS LOAD" _
        & " , ISNULL(RTRIM(OIT0003.OILCODE), '')                 AS OILCODE" _
        & " , ISNULL(RTRIM(OIT0003.OILNAME), '')                 AS OILNAME" _
        & " , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '')            AS ORDERINGTYPE" _
        & " , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '')         AS ORDERINGOILNAME" _
        & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNO), '')           AS CHANGETRAINNO" _
        & " , ISNULL(RTRIM(OIT0003.CHANGETRAINNAME), '')         AS CHANGETRAINNAME" _
        & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '')     AS SECONDCONSIGNEECODE" _
        & " , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '')     AS SECONDCONSIGNEENAME" _
        & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATION), '')        AS SECONDARRSTATION" _
        & " , ISNULL(RTRIM(OIT0003.SECONDARRSTATIONNAME), '')    AS SECONDARRSTATIONNAME" _
        & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATION), '')        AS CHANGERETSTATION" _
        & " , ISNULL(RTRIM(OIT0003.CHANGERETSTATIONNAME), '')    AS CHANGERETSTATIONNAME" _
        & " , ISNULL(RTRIM(VIW0012.TRKBN), '')                   AS TRKBN" _
        & " , ISNULL(RTRIM(VIW0012.TRKBNNAME), '')               AS TRKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.KIRO), '')                    AS KIRO" _
        & " , ISNULL(RTRIM(VIW0012.BRANCH), '')                  AS BRANCH"

        '共通FROM用1
        SQLFromStr1 =
          " FROM OIL.OIT0002_ORDER OIT0002 " _
        & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        & "       OIT0003.ORDERNO = OIT0002.ORDERNO" _
        & "       AND OIT0003.DELFLG <> @P02" _
        & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
        & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
        & "       AND OIM0005.DELFLG <> @P02" _
        & " LEFT JOIN OIL.OIM0010_PATTERN OIM0010 ON " _
        & "       OIM0010.PATCODE = OIT0002.ORDERTYPE" _
        & "       AND OIM0010.WORKCODE = '9'" _
        & "       AND OIM0010.DELFLG <> @P02"

        '共通FROM用2
        SQLFromStr2 =
          "       VIW0012.ACCOUNTCODE = OIM0010.ACCOUNTCODE" _
        & "       AND VIW0012.SEGMENTCODE = OIM0010.SEGMENTCODE" _
        & "       AND VIW0012.SHIPPERSCODE = OIT0003.SHIPPERSCODE" _
        & "       AND VIW0012.BASECODE = OIT0002.BASECODE" _
        & "       AND VIW0012.OFFICECODE = OIT0002.OFFICECODE" _
        & "       AND VIW0012.DEPSTATION = OIT0002.DEPSTATION" _
        & "       AND VIW0012.ARRSTATION = OIT0002.ARRSTATION" _
        & "       AND VIW0012.CONSIGNEECODE = OIT0002.CONSIGNEECODE" _
        & "       AND VIW0012.LOAD = OIM0005.LOAD"

        '★作成SQL
        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(タンク車使用料)
        '#############################################################################
        SQLStr =
            SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.TCCALCKBN), '')                  AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.TCCALCKBNNAME), '')              AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.TCCHARGE), '')                   AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.TCDISCOUNT1), '')                AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.TCDISCOUNT2), '')                AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.TCDISCOUNT3), '')                AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.TCAPPLYCHARGE), '')              AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.TCINVOICECODE), '')              AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.TCINVOICENAME), '')              AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.TCINVOICEDEPTNAME), '')          AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.TCPAYEECODE), '')                AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.TCPAYEENAME), '')                 AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.TCPAYEEDEPTNAME), '')            AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10101 VIW0012 ON " _
        & SQLFromStr2 _
        & "       And VIW0012.SENDAI_MORIOKA_FLAG =" _
        & "           Case WHEN OIT0002.BASECODE = '" & BaseDllConst.CONST_PLANTCODE_0401 & "' AND OIT0002.CONSIGNEECODE = '" & BaseDllConst.CONST_CONSIGNEECODE_51 & "' THEN" _
        & "                Case WHEN VIW0012.BREAKDOWNCODE = '1' THEN '3'" _
        & "                     WHEN OIT0003.OILCODE = '" & BaseDllConst.CONST_HTank & "' OR OIT0003.OILCODE = '" & BaseDllConst.CONST_RTank & "' THEN '1' ELSE '2' END" _
        & "           Else '0' END" _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(鉄道運賃)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.FARECALCKBN), '')                AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.FARECALCKBNNAME), '')            AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.SYOTEIHAZFARE), '')              AS CHARGE" _
        & " , ISNULL(RTRIM(VIW0012.HAZJRDISCOUNT), '')              AS JRDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HAZOTDISCOUNT), '')              AS OTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HAZJOTDISCOUNT), '')             AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HAZDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.HAZDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ''                                                    AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.HAZFARE), '')                    AS APPLYCHARGE" _
        & " , ISNULL(RTRIM(VIW0012.RETURNFARE), '')                 AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.FAREINVOICECODE), '')            AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.FAREINVOICENAME), '')            AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.FAREINVOICEDEPTNAME), '')        AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.FAREPAYEECODE), '')              AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.FAREPAYEENAME), '')               AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.FAREPAYEEDEPTNAME), '')          AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10102_1 VIW0012 ON " _
        & SQLFromStr2 _
        & "       And VIW0012.SENDAI_MORIOKA_FLAG =" _
        & "           Case WHEN OIT0003.OILCODE = '" & BaseDllConst.CONST_HTank & "' OR OIT0003.OILCODE = '" & BaseDllConst.CONST_RTank & "' THEN '1' ELSE '2' END" _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.MOTCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.MOTCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTCHARGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ''                                                    AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.MOTDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.MOTDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.MOTDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.MOTAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.MOTINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.MOTINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.MOTPAYEENAME), '')               AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.MOTPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10102_2 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(業務料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.WRKCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.WRKCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKCHARGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.WRKDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.WRKAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.WRKINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.WRKINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.WRKPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.WRKPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10103 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(取扱料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.HNDCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.HNDCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDCAHRGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.HNDDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.HNDAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.HNDINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.HNDINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.HNDPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.HNDPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10104 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(ＯＴ業務料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.OTCALCKBN), '')                  AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.OTCALCKBNNAME), '')              AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.OTCAHRGE), '')                   AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT), '')                 AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT1), '')                AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT2), '')                AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.OTDISCOUNT3), '')                AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.OTAPPLYCHARGE), '')              AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.OTINVOICECODE), '')              AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.OTINVOICENAME), '')              AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.OTINVOICEDEPTNAME), '')          AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.OTPAYEECODE), '')                AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.OTPAYEENAME), '')                 AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.OTPAYEEDEPTNAME), '')            AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10105 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(元請輸送)
        '　セグメント(運賃手数料)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.FRTCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.FRTCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTCAHRGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.FRTDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.FRTAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.FRTINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.FRTINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.FRTPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.FRTPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_41010101_10106 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        '#############################################################################
        '　勘定科目(委託作業費)
        '　セグメント(通運取扱その他)
        '#############################################################################
        SQLStr &=
          " UNION ALL " _
        & SQLSelectStr _
        & " , ISNULL(RTRIM(VIW0012.COMCALCKBN), '')                 AS CALCKBN" _
        & " , ISNULL(RTRIM(VIW0012.COMCALCKBNNAME), '')             AS CALCKBNNAME" _
        & " , ISNULL(RTRIM(VIW0012.COMCAHRGE), '')                  AS CHARGE" _
        & " , ''                                                    AS JRDISCOUNT" _
        & " , ''                                                    AS OTDISCOUNT" _
        & " , ''                                                    AS JOTDISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT), '')                AS DISCOUNT" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT1), '')               AS DISCOUNT1" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT2), '')               AS DISCOUNT2" _
        & " , ISNULL(RTRIM(VIW0012.COMDISCOUNT3), '')               AS DISCOUNT3" _
        & " , ISNULL(RTRIM(VIW0012.COMAPPLYCHARGE), '')             AS APPLYCHARGE" _
        & " , ''                                                    AS RETURNFARE" _
        & " , ISNULL(RTRIM(VIW0012.COMINVOICECODE), '')             AS INVOICECODE" _
        & " , ISNULL(RTRIM(VIW0012.COMINVOICENAME), '')             AS INVOICENAME" _
        & " , ISNULL(RTRIM(VIW0012.COMINVOICEDEPTNAME), '')         AS INVOICEDEPTNAME" _
        & " , ISNULL(RTRIM(VIW0012.COMPAYEECODE), '')               AS PAYEECODE" _
        & " , ISNULL(RTRIM(VIW0012.COMPAYEENAME), '')                AS PAYEENAME" _
        & " , ISNULL(RTRIM(VIW0012.COMPAYEEDEPTNAME), '')           AS PAYEEDEPTNAME" _
        & SQLFromStr1 _
        & " INNER JOIN OIL.VIW0012_SALES_51020104_10106 VIW0012 ON " _
        & SQLFromStr2 _
        & " WHERE OIT0002.ORDERNO = @P01 " _
        & " AND OIT0002.DELFLG <> @P02 "

        Return SQLStr
    End Function

    ''' <summary>
    ''' 空回日報(OT連携)比較用SQL
    ''' </summary>
    ''' <remarks>OT連携されたデータと空回日報との比較を表示する際のSQLを設定</remarks>
    Public Function EmptyTurnDairyOTCompare(ByVal dt As DataTable, ByRef O_inOrder As String)
        Dim inOrder As String = "("
        Dim i As Integer = 0
        For Each dtrow As DataRow In dt.Rows
            If i = 0 Then
                inOrder &= "'" + Convert.ToString(dtrow("ORDERNO")) + "'"
            Else
                inOrder &= ", '" + Convert.ToString(dtrow("ORDERNO")) + "'"
            End If
            i += 1
        Next
        inOrder &= ")"
        O_inOrder = inOrder

        '######################################################
        '# 受注TBLに存在するデータと比較(OT受注TBL)
        '######################################################
        Dim SQLStr As String = ""
        Dim SQLORDERStr As String =
              " SELECT " _
            & "   OIT0002.ORDERNO AS KEYCODE1" _
            & " , OIT0003.DETAILNO AS KEYCODE2 " _
            & " , OIT0002.OFFICECODE AS KEYCODE3 " _
            & " , CASE " _
            & "   WHEN OT.IMPORTFLG = '0' THEN '0' " _
            & "   WHEN OT.ORDERNO IS NULL THEN '4' " _
            & "   WHEN OIT0003.OILCODE + OIT0003.ORDERINGTYPE <> OT.OILCODE + OT.ORDERINGTYPE THEN '2' " _
            & "   WHEN OIT0003.TANKNO <> OT.TANKNO THEN '3' " _
            & "   ELSE '1' " _
            & "   END AS COMPAREINFOCD " _
            & " , CASE " _
            & "   WHEN OT.IMPORTFLG = '0' THEN '新規作成' " _
            & "   WHEN OT.ORDERNO IS NULL THEN 'データ削除' " _
            & "   WHEN OIT0003.OILCODE + OIT0003.ORDERINGTYPE <> OT.OILCODE + OT.ORDERINGTYPE THEN '油種不一致' " _
            & "   WHEN OIT0003.TANKNO <> OT.TANKNO THEN 'タンク車No不一致' " _
            & "   ELSE '一致' " _
            & "   END AS COMPAREINFONM " _
            & " , OIT0002.ORDERNO " _
            & " , OIT0003.DETAILNO " _
            & " , OIT0002.ORDERSTATUS " _
            & " , OIT0002.TRAINNO " _
            & " , OIT0002.TRAINNAME " _
            & " , OIT0002.ORDERYMD " _
            & " , OIT0002.OFFICECODE " _
            & " , OIT0002.OFFICENAME " _
            & " , OIT0003.SHIPPERSCODE " _
            & " , OIT0003.SHIPPERSNAME " _
            & " , OIT0002.BASECODE " _
            & " , OIT0002.BASENAME " _
            & " , OIT0002.CONSIGNEECODE " _
            & " , OIT0002.CONSIGNEENAME " _
            & " , OIT0002.DEPSTATION " _
            & " , OIT0002.DEPSTATIONNAME " _
            & " , OIT0002.ARRSTATION " _
            & " , OIT0002.ARRSTATIONNAME " _
            & " , OIT0003.SHIPORDER " _
            & " , OIT0003.LINEORDER " _
            & " , OIT0003.TANKNO " _
            & " , OIT0003.OILCODE " _
            & " , OIT0003.OILNAME " _
            & " , OIT0003.ORDERINGTYPE " _
            & " , OIT0003.ORDERINGOILNAME " _
            & " , OIT0003.CARSNUMBER " _
            & " , OIT0003.CARSAMOUNT " _
            & " , OIT0003.RETURNDATETRAIN " _
            & " , OIT0003.JOINTCODE " _
            & " , OIT0003.JOINT " _
            & " , OIT0003.REMARK " _
            & " , OIT0003.SECONDCONSIGNEECODE " _
            & " , OIT0003.SECONDCONSIGNEENAME " _
            & " , OIT0002.LODDATE " _
            & " , OIT0002.DEPDATE " _
            & " , OIT0002.ARRDATE " _
            & " , OIT0002.ACCDATE " _
            & " , OIT0002.EMPARRDATE " _
            & " , OIT0002.BTRAINNO " _
            & " , OT.IMPORTFLG AS IMPORTFLG " _
            & " , OT.ORDERNO AS OT_ORDERNO " _
            & " , OT.OTDETAILNO AS OT_DETAILNO " _
            & " , OT.ORDERSTATUS AS OT_ORDERSTATUS " _
            & " , OT.TRAINNO AS OT_TRAINNO " _
            & " , OT.TRAINNAME AS OT_TRAINNAME " _
            & " , OT.ORDERYMD AS OT_ORDERYMD " _
            & " , OT.OFFICECODE AS OT_OFFICECODE " _
            & " , OT.OFFICENAME AS OT_OFFICENAME " _
            & " , OT.SHIPPERSCODE AS OT_SHIPPERSCODE " _
            & " , OT.SHIPPERSNAME AS OT_SHIPPERSNAME " _
            & " , OT.BASECODE AS OT_BASECODE " _
            & " , OT.BASENAME AS OT_BASENAME " _
            & " , OT.CONSIGNEECODE AS OT_CONSIGNEECODE " _
            & " , OT.CONSIGNEENAME AS OT_CONSIGNEENAME " _
            & " , OT.DEPSTATION AS OT_DEPSTATION " _
            & " , OT.DEPSTATIONNAME AS OT_DEPSTATIONNAME " _
            & " , OT.ARRSTATION AS OT_ARRSTATION " _
            & " , OT.ARRSTATIONNAME AS OT_ARRSTATIONNAME " _
            & " , OT.SHIPORDER AS OT_SHIPORDER" _
            & " , OT.LINEORDER AS OT_LINEORDER " _
            & " , OT.TANKNO AS OT_TANKNO " _
            & " , OT.OILCODE AS OT_OILCODE " _
            & " , OT.OILNAME AS OT_OILNAME " _
            & " , OT.ORDERINGTYPE AS OT_ORDERINGTYPE " _
            & " , OT.ORDERINGOILNAME AS OT_ORDERINGOILNAME " _
            & " , OT.CARSNUMBER AS OT_CARSNUMBER " _
            & " , OT.CARSAMOUNT AS OT_CARSAMOUNT " _
            & " , OT.RETURNDATETRAIN OT_RETURNDATETRAIN " _
            & " , OT.JOINTCODE AS OT_JOINTCODE " _
            & " , OT.JOINT AS OT_JOINT " _
            & " , OT.REMARK AS OT_REMARK " _
            & " , OT.SECONDCONSIGNEECODE AS OT_SECONDCONSIGNEECODE " _
            & " , OT.SECONDCONSIGNEENAME AS OT_SECONDCONSIGNEENAME " _
            & " , OT.LODDATE AS OT_LODDATE " _
            & " , OT.DEPDATE AS OT_DEPDATE " _
            & " , OT.ARRDATE AS OT_ARRDATE " _
            & " , OT.ACCDATE AS OT_ACCDATE " _
            & " , OT.EMPARRDATE AS OT_EMPARRDATE " _
            & " , OT.BTRAINNO AS OT_BTRAINNO " _
            & " , '0' AS DELFLG " _
            & " , @INITYMD AS INITYMD " _
            & " , @INITUSER AS INITUSER " _
            & " , @INITTERMID AS INITTERMID " _
            & " , @UPDYMD AS UPDYMD " _
            & " , @UPDUSER AS UPDUSER " _
            & " , @UPDTERMID AS UPDTERMID " _
            & " , @RECEIVEYMD AS RECEIVEYMD "

        SQLORDERStr &=
              " FROM oil.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & " OIT0002.ORDERNO = OIT0003.ORDERNO " _
            & " LEFT JOIN ( " _
            & "     SELECT " _
            & "       OIT0016.IMPORTFLG " _
            & "     , OIT0016.ORDERNO " _
            & "     , OIT0017.OTDETAILNO " _
            & "     , OIT0016.ORDERSTATUS " _
            & "     , OIT0016.TRAINNO " _
            & "     , OIT0016.TRAINNAME " _
            & "     , OIT0016.ORDERYMD " _
            & "     , OIT0016.OFFICECODE " _
            & "     , OIT0016.OFFICENAME " _
            & "     , OIT0017.SHIPPERSCODE " _
            & "     , OIT0017.SHIPPERSNAME " _
            & "     , OIT0016.BASECODE " _
            & "     , OIT0016.BASENAME " _
            & "     , OIT0016.CONSIGNEECODE " _
            & "     , OIT0016.CONSIGNEENAME " _
            & "     , OIT0016.DEPSTATION " _
            & "     , OIT0016.DEPSTATIONNAME " _
            & "     , OIT0016.ARRSTATION " _
            & "     , OIT0016.ARRSTATIONNAME " _
            & "     , OIT0017.SHIPORDER " _
            & "     , OIT0017.LINEORDER " _
            & "     , OIT0017.TANKNO " _
            & "     , OIT0017.OILCODE " _
            & "     , OIT0017.OILNAME " _
            & "     , OIT0017.ORDERINGTYPE " _
            & "     , OIT0017.ORDERINGOILNAME " _
            & "     , OIT0017.CARSNUMBER " _
            & "     , OIT0017.CARSAMOUNT " _
            & "     , OIT0017.RETURNDATETRAIN " _
            & "     , OIT0017.JOINTCODE " _
            & "     , OIT0017.JOINT " _
            & "     , OIT0017.REMARK " _
            & "     , OIT0017.SECONDCONSIGNEECODE " _
            & "     , OIT0017.SECONDCONSIGNEENAME " _
            & "     , OIT0016.LODDATE " _
            & "     , OIT0016.DEPDATE " _
            & "     , OIT0016.ARRDATE " _
            & "     , OIT0016.ACCDATE " _
            & "     , OIT0016.EMPARRDATE " _
            & "     , OIT0016.BTRAINNO " _
            & "     FROM oil.OIT0016_OTORDER OIT0016 " _
            & "     INNER JOIN oil.OIT0017_OTDETAIL OIT0017 ON " _
            & "     OIT0017.ORDERNO = OIT0016.ORDERNO " _
            & "     WHERE OIT0016.OFFICECODE = @OFFICECODE " _
            & "     AND OIT0016.ORDERYMD = @ORDERYMD " _
            & "     ) OT ON " _
            & " OT.ORDERNO = OIT0002.ORDERNO " _
            & " AND OT.OTDETAILNO = OIT0003.DETAILNO "

        SQLORDERStr &=
              String.Format(" WHERE OIT0002.ORDERNO IN {0} ", inOrder) _
            & " AND OIT0002.OFFICECODE = @OFFICECODE " _

        '######################################################
        '# OT受注TBLにのみ存在するデータを取得(比較は受注TBL)
        '######################################################
        Dim SQLOTORDERStr As String =
              " SELECT " _
            & "   OIT0016.ORDERNO AS KEYCODE1 " _
            & " , OIT0017.OTDETAILNO AS KEYCODE2 " _
            & " , OIT0016.OFFICECODE AS KEYCODE3 " _
            & " , CASE " _
            & "   WHEN OIT0016.IMPORTFLG = '0' THEN '0' " _
            & "   WHEN JOT.ORDERNO IS NULL THEN '5' " _
            & "   WHEN OIT0017.OILCODE + OIT0017.ORDERINGTYPE <> JOT.OILCODE + JOT.ORDERINGTYPE THEN '2' " _
            & "   WHEN OIT0017.TANKNO <> JOT.TANKNO THEN '3' " _
            & "   ELSE '1' " _
            & "   END AS COMPAREINFOCD " _
            & " , CASE " _
            & "   WHEN OIT0016.IMPORTFLG = '0' THEN '新規作成' " _
            & "   WHEN JOT.ORDERNO IS NULL THEN 'データ追加' " _
            & "   WHEN OIT0017.OILCODE + OIT0017.ORDERINGTYPE <> JOT.OILCODE + JOT.ORDERINGTYPE THEN '油種不一致' " _
            & "   WHEN OIT0017.TANKNO <> JOT.TANKNO THEN 'タンク車No不一致' " _
            & "   ELSE '一致' " _
            & "   END AS COMPAREINFONM " _
            & " , JOT.ORDERNO AS JOT_ORDERNO " _
            & " , JOT.DETAILNO AS JOT_DETAILNO " _
            & " , JOT.ORDERSTATUS AS JOT_ORDERSTATUS " _
            & " , JOT.TRAINNO AS JOT_TRAINNO " _
            & " , JOT.TRAINNAME AS JOT_TRAINNAME " _
            & " , JOT.ORDERYMD AS JOT_ORDERYMD " _
            & " , JOT.OFFICECODE AS JOT_OFFICECODE " _
            & " , JOT.OFFICENAME AS JOT_OFFICENAME " _
            & " , JOT.SHIPPERSCODE AS JOT_SHIPPERSCODE " _
            & " , JOT.SHIPPERSNAME AS JOT_SHIPPERSNAME " _
            & " , JOT.BASECODE AS JOT_BASECODE " _
            & " , JOT.BASENAME AS JOT_BASENAME " _
            & " , JOT.CONSIGNEECODE AS JOT_CONSIGNEECODE " _
            & " , JOT.CONSIGNEENAME AS JOT_CONSIGNEENAME " _
            & " , JOT.DEPSTATION AS JOT_DEPSTATION " _
            & " , JOT.DEPSTATIONNAME AS JOT_DEPSTATIONNAME " _
            & " , JOT.ARRSTATION AS JOT_ARRSTATION " _
            & " , JOT.ARRSTATIONNAME AS JOT_ARRSTATIONNAME " _
            & " , JOT.SHIPORDER AS JOT_SHIPORDER " _
            & " , JOT.LINEORDER AS JOT_LINEORDER " _
            & " , JOT.TANKNO AS JOT_TANKNO " _
            & " , JOT.OILCODE AS JOT_OILCODE " _
            & " , JOT.OILNAME AS JOT_OILNAME " _
            & " , JOT.ORDERINGTYPE AS JOT_ORDERINGTYPE " _
            & " , JOT.ORDERINGOILNAME AS JOT_ORDERINGOILNAME " _
            & " , JOT.CARSNUMBER AS JOT_CARSNUMBER " _
            & " , JOT.CARSAMOUNT AS JOT_CARSAMOUNT " _
            & " , JOT.RETURNDATETRAIN AS JOT_RETURNDATETRAIN " _
            & " , JOT.JOINTCODE AS JOT_JOINTCODE " _
            & " , JOT.JOINT AS JOT_JOINT " _
            & " , JOT.REMARK AS JOT_REMARK" _
            & " , JOT.SECONDCONSIGNEECODE AS JOT_SECONDCONSIGNEECODE" _
            & " , JOT.SECONDCONSIGNEENAME AS JOT_SECONDCONSIGNEENAME" _
            & " , JOT.LODDATE AS JOT_LODDATE " _
            & " , JOT.DEPDATE AS JOT_DEPDATE " _
            & " , JOT.ARRDATE AS JOT_ARRDATE " _
            & " , JOT.ACCDATE AS JOT_ACCDATE " _
            & " , JOT.EMPARRDATE AS JOT_EMPARRDATE " _
            & " , JOT.BTRAINNO AS JOT_BTRAINNO " _
            & " , OIT0016.IMPORTFLG " _
            & " , OIT0016.ORDERNO " _
            & " , OIT0017.OTDETAILNO " _
            & " , OIT0016.ORDERSTATUS " _
            & " , OIT0016.TRAINNO " _
            & " , OIT0016.TRAINNAME " _
            & " , OIT0016.ORDERYMD " _
            & " , OIT0016.OFFICECODE " _
            & " , OIT0016.OFFICENAME " _
            & " , OIT0017.SHIPPERSCODE " _
            & " , OIT0017.SHIPPERSNAME " _
            & " , OIT0016.BASECODE " _
            & " , OIT0016.BASENAME " _
            & " , OIT0016.CONSIGNEECODE " _
            & " , OIT0016.CONSIGNEENAME " _
            & " , OIT0016.DEPSTATION " _
            & " , OIT0016.DEPSTATIONNAME " _
            & " , OIT0016.ARRSTATION " _
            & " , OIT0016.ARRSTATIONNAME " _
            & " , OIT0017.SHIPORDER " _
            & " , OIT0017.LINEORDER " _
            & " , OIT0017.TANKNO " _
            & " , OIT0017.OILCODE " _
            & " , OIT0017.OILNAME " _
            & " , OIT0017.ORDERINGTYPE " _
            & " , OIT0017.ORDERINGOILNAME " _
            & " , OIT0017.CARSNUMBER " _
            & " , OIT0017.CARSAMOUNT " _
            & " , OIT0017.RETURNDATETRAIN " _
            & " , OIT0017.JOINTCODE " _
            & " , OIT0017.JOINT " _
            & " , OIT0017.REMARK " _
            & " , OIT0017.SECONDCONSIGNEECODE " _
            & " , OIT0017.SECONDCONSIGNEENAME " _
            & " , OIT0016.LODDATE " _
            & " , OIT0016.DEPDATE " _
            & " , OIT0016.ARRDATE " _
            & " , OIT0016.ACCDATE " _
            & " , OIT0016.EMPARRDATE " _
            & " , OIT0016.BTRAINNO " _
            & " , '0' AS DELFLG " _
            & " , @INITYMD AS INITYMD " _
            & " , @INITUSER AS INITUSER " _
            & " , @INITTERMID AS INITTERMID " _
            & " , @UPDYMD AS UPDYMD " _
            & " , @UPDUSER AS UPDUSER " _
            & " , @UPDTERMID AS UPDTERMID " _
            & " , @RECEIVEYMD AS RECEIVEYMD "

        SQLOTORDERStr &=
              " FROM oil.OIT0016_OTORDER OIT0016 " _
            & " INNER JOIN oil.OIT0017_OTDETAIL OIT0017 ON " _
            & " OIT0016.ORDERNO = OIT0017.ORDERNO " _
            & " LEFT JOIN ( " _
            & "     SELECT " _
            & "       OIT0002.ORDERNO " _
            & "     , OIT0003.DETAILNO " _
            & "     , OIT0002.ORDERSTATUS " _
            & "     , OIT0002.TRAINNO " _
            & "     , OIT0002.TRAINNAME " _
            & "     , OIT0002.ORDERYMD " _
            & "     , OIT0002.OFFICECODE " _
            & "     , OIT0002.OFFICENAME " _
            & "     , OIT0003.SHIPPERSCODE " _
            & "     , OIT0003.SHIPPERSNAME " _
            & "     , OIT0002.BASECODE " _
            & "     , OIT0002.BASENAME " _
            & "     , OIT0002.CONSIGNEECODE " _
            & "     , OIT0002.CONSIGNEENAME " _
            & "     , OIT0002.DEPSTATION " _
            & "     , OIT0002.DEPSTATIONNAME " _
            & "     , OIT0002.ARRSTATION " _
            & "     , OIT0002.ARRSTATIONNAME " _
            & "     , OIT0003.SHIPORDER " _
            & "     , OIT0003.LINEORDER " _
            & "     , OIT0003.TANKNO " _
            & "     , OIT0003.OILCODE " _
            & "     , OIT0003.OILNAME " _
            & "     , OIT0003.ORDERINGTYPE " _
            & "     , OIT0003.ORDERINGOILNAME " _
            & "     , OIT0003.CARSNUMBER " _
            & "     , OIT0003.CARSAMOUNT " _
            & "     , OIT0003.RETURNDATETRAIN " _
            & "     , OIT0003.JOINTCODE " _
            & "     , OIT0003.JOINT " _
            & "     , OIT0003.REMARK " _
            & "     , OIT0003.SECONDCONSIGNEECODE " _
            & "     , OIT0003.SECONDCONSIGNEENAME " _
            & "     , OIT0002.LODDATE " _
            & "     , OIT0002.DEPDATE " _
            & "     , OIT0002.ARRDATE " _
            & "     , OIT0002.ACCDATE " _
            & "     , OIT0002.EMPARRDATE " _
            & "     , OIT0002.BTRAINNO " _
            & "     FROM oil.OIT0002_ORDER OIT0002 " _
            & "     INNER JOIN oil.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & String.Format(" WHERE OIT0002.ORDERNO IN {0} ", inOrder) _
            & "     AND OIT0002.OFFICECODE = @OFFICECODE " _
            & "     ) JOT ON " _
            & " JOT.ORDERNO = OIT0016.ORDERNO " _
            & " AND JOT.DETAILNO = OIT0017.OTDETAILNO "

        SQLOTORDERStr &=
              " WHERE OIT0016.OFFICECODE = @OFFICECODE " _
            & " AND OIT0016.ORDERYMD = @ORDERYMD " _
            & " AND JOT.ORDERNO IS NULL "

        '★SQL結合
        SQLStr = SQLORDERStr
        SQLStr &= " UNION ALL "
        SQLStr &= SQLOTORDERStr

        Return SQLStr
    End Function

    ''' <summary>
    ''' 空回日報(OT連携)比較結果(帳票)SQL
    ''' </summary>
    ''' <remarks>OT連携されたデータと空回日報との比較結果を表示する際のSQLを設定</remarks>
    Public Function EmptyTurnDairyOTComparePrint(ByVal mapID As String)
        '★共通項目
        Dim SQLStr As String =
              " SELECT " _
            & "   0                                                            AS LINECNT" _
            & " , ''                                                           AS OPERATION" _
            & " , '0'                                                          AS TIMSTP" _
            & " , 1                                                            AS 'SELECT'" _
            & " , 0                                                            AS HIDDEN" _
            & " , OIT0020.KEYCODE3                                             AS OFFICECODE" _
            & " , OIM0002.NAME                                                 AS OFFICENAME" _
            & " , OIT0020.COMPAREINFOCD                                        AS COMPAREINFOCD" _
            & " , OIT0020.COMPAREINFONM                                        AS COMPAREINFONM" _
            & " , ISNULL(OIT0020.TRAINNO,OIT0020.OT_TRAINNO)                   AS TRAINNO" _
            & " , ISNULL(OIT0020.TRAINNAME,OIT0020.OT_TRAINNAME)               AS TRAINNAME" _
            & " , ISNULL(OIT0020.SHIPPERSCODE,OIT0020.OT_SHIPPERSCODE)         AS SHIPPERSCODE" _
            & " , ISNULL(OIT0020.SHIPPERSNAME,OIT0020.OT_SHIPPERSNAME)         AS SHIPPERSNAME" _
            & " , ISNULL(OIT0020.DEPSTATION,OIT0020.OT_DEPSTATION)             AS DEPSTATION" _
            & " , ISNULL(OIT0020.DEPSTATIONNAME,OIT0020.OT_DEPSTATIONNAME)     AS DEPSTATIONNAME" _
            & " , ISNULL(OIT0020.ARRSTATION,OIT0020.OT_ARRSTATION)             AS ARRSTATION" _
            & " , ISNULL(OIT0020.ARRSTATIONNAME,OIT0020.OT_ARRSTATIONNAME)     AS ARRSTATIONNAME" _
            & " , ISNULL(OIT0020.LODDATE,OIT0020.OT_LODDATE)                   AS LODDATE" _
            & " , ISNULL(OIT0020.DEPDATE,OIT0020.OT_DEPDATE)                   AS DEPDATE" _
            & " , ISNULL(OIT0020.ARRDATE,OIT0020.OT_ARRDATE)                   AS ARRDATE" _
            & " , ISNULL(OIT0020.ACCDATE,OIT0020.OT_ACCDATE)                   AS ACCDATE" _
            & " , ISNULL(OIT0020.EMPARRDATE,OIT0020.OT_EMPARRDATE)             AS EMPARRDATE" _
            & " , ISNULL(OIM0003_JOT.BIGOILCODE,OIM0003_OT.BIGOILCODE)         AS BIGOILCODE" _
            & " , ISNULL(OIM0003_JOT.BIGOILNAME,OIM0003_OT.BIGOILNAME)         AS BIGOILNAME" _
            & " , ISNULL(OIM0003_JOT.MIDDLEOILCODE,OIM0003_OT.MIDDLEOILCODE)   AS MIDDLEOILCODE" _
            & " , ISNULL(OIM0003_JOT.MIDDLEOILNAME,OIM0003_OT.MIDDLEOILNAME)   AS MIDDLEOILNAME" _
            & " , ISNULL(OIM0003_JOT.OTOILCODE,OIM0003_OT.OTOILCODE)           AS OTOILCODE" _
            & " , ISNULL(OIM0003_JOT.OTOILNAME,OIM0003_OT.OTOILNAME)           AS OTOILNAME" _
            & " , ISNULL(OIM0003_JOT.SHIPPEROILCODE,OIM0003_OT.SHIPPEROILCODE) AS SHIPPEROILCODE" _
            & " , ISNULL(OIM0003_JOT.SHIPPEROILNAME,OIM0003_OT.SHIPPEROILNAME) AS SHIPPEROILNAME" _
            & " , ISNULL(OIM0003_JOT.CHECKOILCODE,OIM0003_OT.CHECKOILCODE)     AS CHECKOILCODE" _
            & " , ISNULL(OIM0003_JOT.CHECKOILNAME,OIM0003_OT.CHECKOILNAME)     AS CHECKOILNAME" _
            & " , OTOILCT.OTOILCODE                                            AS OTOILCTCODE" _
            & " , OTOILCT.CNT                                                  AS OTOILCTCNT"

        '★比較項目
        SQLStr &=
              " , OIT0020.OILCODE                                          AS OILCODE" _
            & " , OIT0020.OILNAME                                          AS OILNAME" _
            & " , OIT0020.ORDERINGTYPE                                     AS ORDERINGTYPE" _
            & " , OIT0020.ORDERINGOILNAME                                  AS ORDERINGOILNAME" _
            & " , OIM0003_JOT.OTOILCODE                                    AS JOT_OTOILCODE" _
            & " , OIM0003_JOT.OTOILNAME                                    AS JOT_OTOILNAME" _
            & " , OIT0020.OT_OILCODE                                       AS OT_OILCODE" _
            & " , OIT0020.OT_OILNAME                                       AS OT_OILNAME" _
            & " , OIT0020.OT_ORDERINGTYPE                                  AS OT_ORDERINGTYPE" _
            & " , OIT0020.OT_ORDERINGOILNAME                               AS OT_ORDERINGOILNAME" _
            & " , OIM0003_OT.OTOILCODE                                     AS OT_OTOILCODE" _
            & " , OIM0003_OT.OTOILNAME                                     AS OT_OTOILNAME" _
            & " , OIM0005_JOT.MODEL                                        AS MODEL" _
            & " , OIT0020.TANKNO                                           AS TANKNO" _
            & " , OIM0005_OT.MODEL                                         AS OT_MODEL" _
            & " , OIT0020.OT_TANKNO                                        AS OT_TANKNO" _
            & " , OIT0020.JOINT                                            AS JOINT" _
            & " , OIT0020.OT_JOINT                                         AS OT_JOINT" _
            & " FROM oil.OIT0020_OTCOMPARE OIT0020 " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003_JOT ON " _
            & "     OIM0003_JOT.OFFICECODE = OIT0020.KEYCODE3 " _
            & " AND OIM0003_JOT.OILCODE = OIT0020.OILCODE " _
            & " AND OIM0003_JOT.SEGMENTOILCODE = OIT0020.ORDERINGTYPE " _
            & " AND OIM0003_JOT.DELFLG <> @DELFLG " _
            & " LEFT JOIN oil.OIM0003_PRODUCT OIM0003_OT ON " _
            & "     OIM0003_OT.OFFICECODE = OIT0020.KEYCODE3 " _
            & " AND OIM0003_OT.OILCODE = OIT0020.OT_OILCODE " _
            & " AND OIM0003_OT.SEGMENTOILCODE = OIT0020.OT_ORDERINGTYPE " _
            & " AND OIM0003_OT.DELFLG <> @DELFLG " _
            & " LEFT JOIN oil.OIM0005_TANK OIM0005_JOT ON " _
            & "     OIM0005_JOT.TANKNUMBER = OIT0020.TANKNO " _
            & " AND OIM0005_JOT.DELFLG <> @DELFLG " _
            & " LEFT JOIN oil.OIM0005_TANK OIM0005_OT ON " _
            & "     OIM0005_OT.TANKNUMBER = OIT0020.OT_TANKNO " _
            & " AND OIM0005_OT.DELFLG <> @DELFLG " _
            & " LEFT JOIN oil.OIM0002_ORG OIM0002 ON " _
            & "     OIM0002.ORGCODE = OIT0020.KEYCODE3 " _
            & " AND OIM0002.DELFLG <> @DELFLG "

        SQLStr &=
              " LEFT JOIN ( " _
            & "   SELECT " _
            & "         OIT0020.KEYCODE1 AS ORDERNO " _
            & "       , ISNULL(OIT0020.SHIPPERSCODE,OIT0020.OT_SHIPPERSCODE) AS SHIPPERSCODE " _
            & "       , ISNULL(OIT0020.SHIPPERSNAME,OIT0020.OT_SHIPPERSNAME) AS SHIPPERSNAME " _
            & "       , OIM0003.OTOILCODE " _
            & "       , OIM0003.OTOILNAME " _
            & "       , COUNT(1) AS CNT " _
            & "   FROM oil.OIT0020_OTCOMPARE OIT0020 " _
            & "   INNER JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
            & "       OIM0003.OFFICECODE = OIT0020.KEYCODE3 " _
            & "   AND OIM0003.OILCODE = ISNULL(OIT0020.OILCODE, OIT0020.OT_OILCODE) " _
            & "   AND OIM0003.SEGMENTOILCODE = ISNULL(OIT0020.ORDERINGTYPE, OIT0020.OT_ORDERINGTYPE) " _
            & "   AND OIM0003.DELFLG <> @DELFLG " _
            & "   WHERE OIT0020.KEYCODE1 = @ORDERNO " _
            & "   GROUP BY " _
            & "         OIT0020.KEYCODE1 " _
            & "       , ISNULL(OIT0020.SHIPPERSCODE,OIT0020.OT_SHIPPERSCODE) " _
            & "       , ISNULL(OIT0020.SHIPPERSNAME,OIT0020.OT_SHIPPERSNAME) " _
            & "       , OIM0003.OTOILCODE " _
            & "       , OIM0003.OTOILNAME " _
            & " ) OTOILCT ON " _
            & "     OTOILCT.ORDERNO = OIT0020.KEYCODE1 " _
            & " AND OTOILCT.SHIPPERSCODE = ISNULL(OIT0020.SHIPPERSCODE, OIT0020.OT_SHIPPERSCODE) " _
            & " AND OTOILCT.OTOILCODE = ISNULL(OIM0003_JOT.OTOILCODE,OIM0003_OT.OTOILCODE) "

        SQLStr &=
              " WHERE OIT0020.KEYCODE1 = @ORDERNO " _
            & "   AND OIT0020.KEYCODE3 = @OFFICECODE "

        SQLStr &=
                " ORDER BY" _
            & "    ISNULL(OIT0020.SHIPPERSCODE,OIT0020.OT_SHIPPERSCODE)" _
            & "  , ISNULL(OIT0020.TRAINNO,OIT0020.OT_TRAINNO)" _
            & "  , ISNULL(OIT0020.DEPSTATION,OIT0020.OT_DEPSTATION)" _
            & "  , ISNULL(OIM0003_JOT.OTOILCODE,OIM0003_OT.OTOILCODE)" _
            & "  , ISNULL(OIT0020.LINEORDER,OIT0020.OT_LINEORDER)" _
            & "  , ISNULL(OIT0020.TANKNO,OIT0020.OT_TANKNO)"

        Return SQLStr
    End Function

    ''' <summary>
    ''' 受注(OT連携)比較用SQL
    ''' </summary>
    ''' <remarks>OT連携されたデータと受注との比較を表示する際のSQLを設定</remarks>
    Public Function OrderOTCompare()

        '★初回作成主体
        Dim SQLComStr1 As String =
              " SELECT " _
            & "   TMP0001.LINECNT                                AS LINECNT" _
            & " , TMP0001.OPERATION1                             AS OPERATION" _
            & " , TMP0001.TIMSTP                                 AS TIMSTP" _
            & " , TMP0001.SELECT1                                AS 'SELECT'" _
            & " , TMP0001.HIDDEN                                 AS HIDDEN" _
            & " , TMP0001.ORDERNO                                AS ORDERNO" _
            & " , TMP0001.DETAILNO                               AS DETAILNO" _
            & " , TMP0001.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , TMP0001.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , TMP0001.BASECODE                               AS BASECODE" _
            & " , TMP0001.BASENAME                               AS BASENAME" _
            & " , TMP0001.CONSIGNEECODE                          AS CONSIGNEECODE" _
            & " , TMP0001.CONSIGNEENAME                          AS CONSIGNEENAME" _
            & " , TMP0001.ORDERINFO                              AS ORDERINFO" _
            & " , TMP0001.ORDERINFONAME                          AS ORDERINFONAME" _
            & " , TMP0001.OILCODE                                AS OILCODE" _
            & " , TMP0001.OILNAME                                AS OILNAME" _
            & " , TMP0001.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , TMP0001.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIT0017.OILCODE                                AS OTOILCODE" _
            & " , OIT0017.OILNAME                                AS OILNAMEOTOILNAME" _
            & " , OIT0017.ORDERINGTYPE                           AS OTORDERINGTYPE" _
            & " , OIT0017.ORDERINGOILNAME                        AS OTORDERINGOILNAME" _
            & " , TMP0001.TANKQUOTA                              AS TANKQUOTA" _
            & " , TMP0001.LINKNO                                 AS LINKNO" _
            & " , TMP0001.LINKDETAILNO                           AS LINKDETAILNO" _
            & " , TMP0001.SHIPORDER                              AS SHIPORDER" _
            & " , TMP0001.TANKNO                                 AS TANKNO" _
            & " , OIT0017.TANKNO                                 AS OTTANKNO" _
            & " , TMP0001.TANKSTATUS                             AS TANKSTATUS" _
            & " , TMP0001.LINEORDER                              AS LINEORDER" _
            & " , TMP0001.MODEL                                  AS MODEL" _
            & " , TMP0001.JRINSPECTIONALERT                      AS JRINSPECTIONALERT" _
            & " , TMP0001.JRINSPECTIONALERTSTR                   AS JRINSPECTIONALERTSTR" _
            & " , TMP0001.JRINSPECTIONDATE                       AS JRINSPECTIONDATE" _
            & " , TMP0001.JRALLINSPECTIONALERT                   AS JRALLINSPECTIONALERT" _
            & " , TMP0001.JRALLINSPECTIONALERTSTR                AS JRALLINSPECTIONALERTSTR" _
            & " , TMP0001.JRALLINSPECTIONDATE                    AS JRALLINSPECTIONDATE" _
            & " , TMP0001.EMPTYTURNFLG                           AS EMPTYTURNFLG" _
            & " , TMP0001.STACKINGORDERNO                        AS STACKINGORDERNO" _
            & " , TMP0001.STACKINGFLG                            AS STACKINGFLG" _
            & " , TMP0001.OTTRANSPORTFLG                         AS OTTRANSPORTFLG" _
            & " , TMP0001.UPGRADEFLG                             AS UPGRADEFLG" _
            & " , TMP0001.DOWNGRADEFLG                           AS DOWNGRADEFLG" _
            & " , TMP0001.ACTUALLODDATE                          AS ACTUALLODDATE" _
            & " , TMP0001.JOINTCODE                              AS JOINTCODE" _
            & " , TMP0001.JOINT                                  AS JOINT" _
            & " , TMP0001.LASTOILCODE                            AS LASTOILCODE" _
            & " , TMP0001.LASTOILNAME                            AS LASTOILNAME" _
            & " , TMP0001.PREORDERINGTYPE                        AS PREORDERINGTYPE" _
            & " , TMP0001.PREORDERINGOILNAME                     AS PREORDERINGOILNAME" _
            & " , TMP0001.CHANGETRAINNO                          AS CHANGETRAINNO" _
            & " , TMP0001.CHANGETRAINNAME                        AS CHANGETRAINNAME" _
            & " , TMP0001.SECONDCONSIGNEECODE                    AS SECONDCONSIGNEECODE" _
            & " , TMP0001.SECONDCONSIGNEENAME                    AS SECONDCONSIGNEENAME" _
            & " , TMP0001.SECONDARRSTATION                       AS SECONDARRSTATION" _
            & " , TMP0001.SECONDARRSTATIONNAME                   AS SECONDARRSTATIONNAME" _
            & " , TMP0001.CHANGERETSTATION                       AS CHANGERETSTATION" _
            & " , TMP0001.CHANGERETSTATIONNAME                   AS CHANGERETSTATIONNAME" _
            & " , TMP0001.USEORDERNO                             AS USEORDERNO" _
            & " , TMP0001.DELFLG                                 AS DELFLG"

        '★2回目以降作成主体
        Dim SQLComStr2 As String =
              " SELECT " _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , ''                                             AS TIMSTP" _
            & " , '1'                                            AS 'SELECT'" _
            & " , '0'                                            AS HIDDEN" _
            & " , OIT0017.ORDERNO                                AS ORDERNO" _
            & " , OIT0017.DETAILNO                               AS DETAILNO" _
            & " , OIT0017.SHIPPERSCODE                           AS SHIPPERSCODE" _
            & " , OIT0017.SHIPPERSNAME                           AS SHIPPERSNAME" _
            & " , OIT0016.BASECODE                               AS BASECODE" _
            & " , OIT0016.BASENAME                               AS BASENAME" _
            & " , OIT0016.CONSIGNEECODE                          AS CONSIGNEECODE" _
            & " , OIT0016.CONSIGNEENAME                          AS CONSIGNEENAME" _
            & " , ''                              AS ORDERINFO" _
            & " , ''                              AS ORDERINFONAME" _
            & " , OIT0017.OILCODE                                AS OILCODE" _
            & " , OIT0017.OILNAME                                AS OILNAME" _
            & " , OIT0017.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0017.ORDERINGOILNAME                        AS ORDERINGOILNAME" _
            & " , OIT0017.OILCODE                                AS OTOILCODE" _
            & " , OIT0017.OILNAME                                AS OILNAMEOTOILNAME" _
            & " , OIT0017.ORDERINGTYPE                           AS OTORDERINGTYPE" _
            & " , OIT0017.ORDERINGOILNAME                        AS OTORDERINGOILNAME" _
            & " , '未割当'                                       AS TANKQUOTA" _
            & " , ''                                             AS LINKNO" _
            & " , ''                                             AS LINKDETAILNO" _
            & " , ''                                             AS SHIPORDER" _
            & " , ''                                             AS TANKNO" _
            & " , OIT0017.TANKNO                                 AS OTTANKNO" _
            & " , ''                                             AS TANKSTATUS" _
            & " , ''                                             AS LINEORDER" _
            & " , ''                                             AS MODEL" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONALERTSTR" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , '1'                                            AS EMPTYTURNFLG" _
            & " , ''                                             AS STACKINGORDERNO" _
            & " , ''                                             AS STACKINGFLG" _
            & " , ''                                             AS OTTRANSPORTFLG" _
            & " , ''                                             AS UPGRADEFLG" _
            & " , ''                                             AS DOWNGRADEFLG" _
            & " , NULL                                           AS ACTUALLODDATE" _
            & " , ''                                             AS JOINTCODE" _
            & " , ''                                             AS JOINT" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS PREORDERINGTYPE" _
            & " , ''                                             AS PREORDERINGOILNAME" _
            & " , ''                                             AS CHANGETRAINNO" _
            & " , ''                                             AS CHANGETRAINNAME" _
            & " , ''                                             AS SECONDCONSIGNEECODE" _
            & " , ''                                             AS SECONDCONSIGNEENAME" _
            & " , ''                                             AS SECONDARRSTATION" _
            & " , ''                                             AS SECONDARRSTATIONNAME" _
            & " , ''                                             AS CHANGERETSTATION" _
            & " , ''                                             AS CHANGERETSTATIONNAME" _
            & " , ''                                             AS USEORDERNO" _
            & " , ''                                             AS DELFLG"

        '○油種、タンク車(完全一致)
        Dim SQLStr As String =
              SQLComStr1 _
            & " FROM oil.TMP0001ORDER TMP0001 " _
            & " INNER JOIN oil.OIT0017_OTDETAIL OIT0017 ON " _
            & "     OIT0017.ORDERNO = TMP0001.ORDERNO " _
            & " AND OIT0017.OILCODE = TMP0001.OILCODE " _
            & " AND OIT0017.ORDERINGTYPE = TMP0001.ORDERINGTYPE " _
            & " AND OIT0017.TANKNO = TMP0001.TANKNO "

        '○油種のみ一致(タンク車未設定)
        SQLStr &=
              "UNION ALL" _
            & SQLComStr1 _
            & " FROM oil.TMP0001ORDER TMP0001 " _
            & " LEFT JOIN oil.OIT0017_OTDETAIL OIT0017 ON " _
            & "     OIT0017.ORDERNO = TMP0001.ORDERNO " _
            & " AND OIT0017.OILCODE = TMP0001.OILCODE " _
            & " AND OIT0017.ORDERINGTYPE = TMP0001.ORDERINGTYPE " _
            & " WHERE ISNULL(OIT0017.TANKNO,'') = '' "

        '○2回目以降でデータ削除された場合
        SQLStr &=
              "UNION ALL" _
            & SQLComStr1 _
            & " FROM oil.TMP0001ORDER TMP0001 " _
            & " LEFT JOIN oil.OIT0017_OTDETAIL OIT0017 ON " _
            & "     OIT0017.ORDERNO = TMP0001.ORDERNO " _
            & " AND OIT0017.OILCODE = TMP0001.OILCODE " _
            & " AND OIT0017.ORDERINGTYPE = TMP0001.ORDERINGTYPE " _
            & " AND OIT0017.TANKNO = TMP0001.TANKNO " _
            & " WHERE ISNULL(OIT0017.TANKNO,'') = '' "

        '○2回目でデータ追加された場合
        SQLStr &=
              "UNION ALL" _
            & SQLComStr2 _
            & " FROM oil.OIT0017_OTDETAIL OIT0017 " _
            & " INNER JOIN oil.OIT0016_OTORDER OIT0016 ON " _
            & "     OIT0016.ORDERNO = OIT0017.ORDERNO " _
            & " LEFT JOIN oil.TMP0001ORDER TMP0001 ON " _
            & " OIT0017.ORDERNO = TMP0001.ORDERNO " _
            & " AND OIT0017.OILCODE = TMP0001.OILCODE " _
            & " AND OIT0017.ORDERINGTYPE = TMP0001.ORDERINGTYPE " _
            & " AND OIT0017.TANKNO = TMP0001.TANKNO " _
            & " WHERE ISNULL(TMP0001.TANKNO,'') = '' "

        Return SQLStr
    End Function
End Class

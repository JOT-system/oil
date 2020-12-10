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
            & "     OTOILCT.ORDERNO = OIT0002.ORDERNO " _
            & " AND OTOILCT.SHIPPERSCODE = OIT0003.SHIPPERSCODE " _
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
            & "  , OIT0002.TRAINNO" _
            & "  , OIT0002.DEPSTATION" _
            & "  , OIM0003.OTOILCODE" _
            & "  , OIT0003.LINEORDER" _
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

        SQLStr &=
              " , ISNULL(OIT0011.INSPECTIONDATE, OIM0005.JRINSPECTIONDATE) AS INSPECTIONDATE" _
            & " , OIT0011.CONVERSIONAMOUNT                       AS CONVERSIONAMOUNT" _
            & " , OIT0011.ARTICLE                                AS ARTICLE" _
            & " , OIT0011.CURRENTCARTOTAL                        AS CURRENTCARTOTAL" _
            & " , OIT0011.EXTEND                                 AS EXTEND" _
            & " , OIT0011.CONVERSIONTOTAL                        AS CONVERSIONTOTAL" _
            & " , OIT0011.OBJECTIVENAME                          AS OBJECTIVENAME" _
            & " , OIT0003.OILCODE                                AS OILCODE" _
            & " , OIT0003.OILNAME                                AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE                           AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME                        AS ORDERINGOILNAME"

        '### 20201021 START 指摘票対応(No189)全体 #############################################
        SQLStr &=
              " , OIT0005_LASTOIL.LASTOILCODE                    AS LASTOILCODE" _
            & " , OIT0005_LASTOIL.LASTOILNAME                    AS LASTOILNAME" _
            & " , OIT0005_LASTOIL.PREORDERINGTYPE                AS PREORDERINGTYPE" _
            & " , OIT0005_LASTOIL.PREORDERINGOILNAME             AS PREORDERINGOILNAME"
        '### 20201021 END   指摘票対応(No189)全体 #############################################

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
              " , OIT0003.FILLINGPOINT                           AS FILLINGPOINT" _
            & " , OIT0003.LINE                                   AS LINE" _
            & " , OIT0003.LOADINGIRILINETRAINNO                  AS LOADINGIRILINETRAINNO" _
            & " , OIT0002.ARRSTATIONNAME                         AS LOADINGARRSTATIONNAME" _
            & " , CASE " _
            & "   WHEN OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(2) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(3) & "' THEN OIT0011.LOADINGTRAINNO" _
            & "   ELSE OIT0002.TRAINNO" _
            & "   END                                            AS ORDERTRAINNO " _
            & " , CASE " _
            & "   WHEN OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(2) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(3) & "' THEN OIT0011.LOADINGLODDATE" _
            & "   ELSE FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd')" _
            & "   END                                            AS ORDERLODDATE " _
            & " , CASE " _
            & "   WHEN OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(2) & "'" _
            & "        OR OIT0011.OBJECTIVENAME = '" & I_ObjectiveName(3) & "' THEN OIT0011.LOADINGDEPDATE" _
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
End Class

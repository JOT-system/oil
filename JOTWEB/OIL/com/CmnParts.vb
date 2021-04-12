Option Strict On
Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class CmnParts
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    ''' <summary>
    ''' 新規受注NO取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Public Function GetNewOrderNo(ByVal SQLcon As SqlConnection) As String

        Dim NEWORDERNOtbl As DataTable = Nothing
        If IsNothing(NEWORDERNOtbl) Then
            NEWORDERNOtbl = New DataTable
        End If

        If NEWORDERNOtbl.Columns.Count <> 0 Then
            NEWORDERNOtbl.Columns.Clear()
        End If

        NEWORDERNOtbl.Clear()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
            " SELECT" _
            & "   'O' + FORMAT(GETDATE(),'yyyyMMdd') + FORMAT(NEXT VALUE FOR oil.order_sequence,'00') AS ORDERNO"

        Dim orderNo As String = ""
        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        NEWORDERNOtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    NEWORDERNOtbl.Load(SQLdr)
                End Using
                orderNo = Convert.ToString(NEWORDERNOtbl.Rows(0)("ORDERNO"))
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return orderNo
    End Function
    ''' <summary>
    ''' アップロードされた各油種数から油種コードを取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GetOilcode(ByVal I_DTROW As DataRow,
                             ByRef O_TANKCODE() As String,
                             ByRef O_TANKNAME() As String,
                             ByRef O_TANKTYPE() As String,
                             ByRef O_TANKORDERNAME() As String)
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        Dim z As Integer = 0

        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_HTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("HTANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_HTank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_RTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("RTANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_RTank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_TTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("TTANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_TTank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_MTTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("MTTANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_MTTank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_KTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("KTANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_KTank1
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_K3Tank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("K3TANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_K3Tank1
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_K5Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("K5TANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_K5Tank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_K10Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("K10TANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_K10Tank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_LTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("LTANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_LTank1
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
        FixvalueMasterSearch("01" + Convert.ToString(I_DTROW("OFFICECODE")), "PRODUCTPATTERN", BaseDllConst.CONST_ATank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(Convert.ToString(I_DTROW("ATANK"))) - 1
            O_TANKCODE(z) = BaseDllConst.CONST_ATank
            O_TANKNAME(z) = WW_GetValue(0)
            O_TANKTYPE(z) = WW_GetValue(1)
            O_TANKORDERNAME(z) = WW_GetValue(2)
            z += 1
        Next
    End Sub
    ''' <summary>
    ''' 品種出荷期間検索処理
    ''' </summary>
    Public Sub OilTermSearch(ByVal I_OFFICECOE As String, ByVal I_CONSIGNEECODE As String, ByVal I_LODDATE As String, ByRef dtrow As DataRow)
        Dim Oiltermtbl As DataTable = Nothing
        If IsNothing(Oiltermtbl) Then
            Oiltermtbl = New DataTable
        End If
        If Oiltermtbl.Columns.Count <> 0 Then
            Oiltermtbl.Columns.Clear()
        End If
        Oiltermtbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            SqlConnection.ClearPool(SQLcon)

            '検索SQL文
            Dim SQLStr As String =
               " SELECT" _
                & "   OIM0030.OFFICECODE      AS OFFICECODE" _
                & " , OIM0030.SHIPPERCODE     AS SHIPPERCODE" _
                & " , OIM0030.PLANTCODE       AS PLANTCODE" _
                & " , OIM0030.CONSIGNEECODE   AS CONSIGNEECODE" _
                & " , OIM0030.ORDERFROMDATE   AS ORDERFROMDATE" _
                & " , OIM0030.ORDERTODATE     AS ORDERTODATE" _
                & " , OIM0003.OILCODE         AS OILCODE" _
                & " , OIM0003.OILNAME         AS OILNAME" _
                & " , OIM0003.SEGMENTOILCODE  AS SEGMENTOILCODE" _
                & " , OIM0003.SEGMENTOILNAME  AS SEGMENTOILNAME" _
                & " FROM OIL.OIM0030_OILTERM OIM0030 " _
                & " INNER JOIN oil.OIM0003_PRODUCT OIM0003 ON " _
                & "     OIM0003.OFFICECODE = OIM0030.OFFICECODE " _
                & " AND OIM0003.SHIPPERCODE = OIM0030.SHIPPERCODE " _
                & " AND OIM0003.PLANTCODE = OIM0030.PLANTCODE " _
                & " AND OIM0003.OILCODE = OIM0030.OILCODE " _
                & " AND OIM0003.SEGMENTOILCODE = OIM0030.SEGMENTOILCODE " _
                & " AND OIM0003.DELFLG <> @DELFLG " _
                & " WHERE OIM0030.OFFICECODE = @OFFICECODE " _
                & " AND OIM0030.CONSIGNEECODE = @CONSIGNEECODE " _
                & " AND OIM0030.OILCODE = @OILCODE " _
                & " AND OIM0030.ORDERFROMDATE <= @ORDERFROMDATE " _
                & " AND OIM0030.ORDERTODATE >= @ORDERTODATE " _
                & " AND OIM0030.DELFLG <> @DELFLG "

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim P_OFFICECODE As SqlParameter = SQLcmd.Parameters.Add("@OFFICECODE", System.Data.SqlDbType.NVarChar)
                Dim P_CONSIGNEECODE As SqlParameter = SQLcmd.Parameters.Add("@CONSIGNEECODE", System.Data.SqlDbType.NVarChar)
                Dim P_OILCODE As SqlParameter = SQLcmd.Parameters.Add("@OILCODE", System.Data.SqlDbType.NVarChar)
                Dim P_ORDERFROMDATE As SqlParameter = SQLcmd.Parameters.Add("@ORDERFROMDATE", System.Data.SqlDbType.Date)
                Dim P_ORDERTODATE As SqlParameter = SQLcmd.Parameters.Add("@ORDERTODATE", System.Data.SqlDbType.Date)
                Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)

                P_OFFICECODE.Value = I_OFFICECOE
                P_CONSIGNEECODE.Value = I_CONSIGNEECODE
                P_OILCODE.Value = dtrow("OILCODE")
                P_ORDERFROMDATE.Value = I_LODDATE
                P_ORDERTODATE.Value = I_LODDATE
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        Oiltermtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    Oiltermtbl.Load(SQLdr)
                End Using

                '★出荷期間内の油種があった場合
                If Oiltermtbl.Rows.Count <> 0 Then
                    dtrow("OILCODE") = Oiltermtbl.Rows(0)("OILCODE")
                    dtrow("OILNAME") = Oiltermtbl.Rows(0)("OILNAME")
                    dtrow("ORDERINGTYPE") = Oiltermtbl.Rows(0)("SEGMENTOILCODE")
                    dtrow("ORDERINGOILNAME") = Oiltermtbl.Rows(0)("SEGMENTOILNAME")
                End If

            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub
    ''' <summary>
    ''' 受注登録されているか確認
    ''' </summary>
    ''' <param name="I_TRAINNO"></param>
    ''' <param name="I_LODDATE"></param>
    ''' <param name="I_DEPDATE"></param>
    ''' <remarks></remarks>
    Public Function ORDERNO_CHECK(ByVal SQLcon As SqlConnection, ByVal I_TRAINNO As String, ByVal I_LODDATE As String, ByVal I_DEPDATE As String) As String
        Dim OrderChktbl As DataTable = Nothing
        If IsNothing(OrderChktbl) Then
            OrderChktbl = New DataTable
        End If
        If OrderChktbl.Columns.Count <> 0 Then
            OrderChktbl.Columns.Clear()
        End If
        OrderChktbl.Clear()
        Dim orderNo As String = ""

        '○ 取得SQL
        '　 説明　：　出荷予定表(計画枠)取得用SQL
        Dim SQLStr As String =
              " SELECT OIT0002.ORDERNO AS ORDERNO " _
            & " FROM   OIL.OIT0002_ORDER OIT0002 " _
            & " WHERE " _
            & String.Format("        OIT0002.TRAINNO  = '{0}' ", I_TRAINNO) _
            & String.Format("   AND  OIT0002.LODDATE  = '{0}' ", I_LODDATE) _
            & String.Format("   AND  OIT0002.DEPDATE  = '{0}' ", I_DEPDATE) _
            & String.Format("   AND  OIT0002.ORDERSTATUS <> '{0}' ", BaseDllConst.CONST_ORDERSTATUS_900) _
            & String.Format("   AND  OIT0002.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OrderChktbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OrderChktbl.Load(SQLdr)
                End Using
            End Using
            If OrderChktbl.Rows.Count <> 0 Then orderNo = Convert.ToString(OrderChktbl.Rows(0)("ORDERNO"))
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return orderNo
    End Function
    ''' <summary>
    ''' 本線列車に紐づく情報を取得
    ''' </summary>
    ''' <param name="I_OFFICECODE"></param>
    ''' <param name="I_TRAINNO"></param>
    ''' <remarks></remarks>
    Public Function TRAINNUMBER_FIND(ByVal I_OFFICECODE As String, ByVal I_TRAINNO As String) As String()
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        FixvalueMasterSearch(I_OFFICECODE, "TRAINNUMBER_FIND", I_TRAINNO, WW_GetValue)
        Return WW_GetValue
    End Function
    ''' <summary>
    ''' 営業所配下情報を取得
    ''' </summary>
    ''' <param name="I_OFFICECODE"></param>
    ''' <param name="I_ARRSTATIONCODE"></param>
    ''' <remarks></remarks>
    Public Function OFFICESTATUS_FIND(ByVal I_OFFICECODE As String, ByVal I_ARRSTATIONCODE As String) As String()
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
        FixvalueMasterSearch(I_OFFICECODE, "PATTERNMASTER", I_ARRSTATIONCODE, WW_GetValue)
        Return WW_GetValue
    End Function
    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub FixvalueMasterSearch(ByVal I_CODE As String,
                                       ByVal I_CLASS As String,
                                       ByVal I_KEYCODE As String,
                                       ByRef O_VALUE() As String,
                                       Optional ByVal I_LODDATE As String = Nothing,
                                       Optional ByVal I_PARA01 As String = Nothing)
        Dim Fixvaltbl As DataTable = Nothing
        If IsNothing(Fixvaltbl) Then
            Fixvaltbl = New DataTable
        End If

        If Fixvaltbl.Columns.Count <> 0 Then
            Fixvaltbl.Columns.Clear()
        End If

        Fixvaltbl.Clear()

        Try
            'DBより取得
            Fixvaltbl = FixvalueMasterDataGet(I_CODE, I_CLASS, I_KEYCODE, I_PARA01)

            If I_KEYCODE.Equals("") Then

                If IsNothing(I_PARA01) Then
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = Convert.ToString(dtfxrow("VALUE" & i.ToString()))
                        Next
                    Next
                ElseIf I_PARA01 = "1" Then    '### 油種登録用の油種コードを取得 ###
                    Dim i As Integer = 0
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        '### 20201030 START 積込日(予定)基準で油種の開始終了を制御 ################################################
                        Try
                            If Date.Parse(dtfxrow("STYMD").ToString()) <= Date.Parse(I_LODDATE) _
                                AndAlso Date.Parse(dtfxrow("ENDYMD").ToString()) >= Date.Parse(I_LODDATE) Then
                                O_VALUE(i) = Convert.ToString(dtfxrow("KEYCODE")).Replace(Convert.ToString(dtfxrow("VALUE2")), "")
                                i += 1
                            End If
                        Catch ex As Exception
                            Exit For
                        End Try
                        '### 20201030 END   積込日(予定)基準で油種の開始終了を制御 ################################################
                    Next
                End If

            Else
                If IsNothing(I_PARA01) Then
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = Convert.ToString(dtfxrow("VALUE" & i.ToString()))
                        Next
                    Next
                ElseIf I_PARA01 = "1" Then
                    Dim i As Integer = 0
                    For Each dtfxrow As DataRow In Fixvaltbl.Rows
                        Try
                            If Date.Parse(dtfxrow("STYMD").ToString()) <= Date.Parse(I_LODDATE) _
                                AndAlso Date.Parse(dtfxrow("ENDYMD").ToString()) >= Date.Parse(I_LODDATE) Then
                                O_VALUE(0) = Convert.ToString(dtfxrow("KEYCODE")).Replace(Convert.ToString(dtfxrow("VALUE2")), "")
                                O_VALUE(1) = Convert.ToString(dtfxrow("VALUE3"))
                                O_VALUE(2) = Convert.ToString(dtfxrow("VALUE2"))
                                O_VALUE(3) = Convert.ToString(dtfxrow("VALUE1"))
                                'O_VALUE(i) = Convert.ToString(OIT0003WKrow("KEYCODE")).Replace(Convert.ToString(OIT0003WKrow("VALUE2")), "")
                                'i += 1
                            End If
                        Catch ex As Exception
                            Exit For
                        End Try
                    Next
                End If
            End If

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub
    ''' <summary>
    ''' マスタ検索処理（同じパラメータならDB抽出せずに保持内容を返却）
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="I_PARA01"></param>
    ''' <returns></returns>
    Private Function FixvalueMasterDataGet(I_CODE As String, I_CLASS As String, I_KEYCODE As String, I_PARA01 As String) As DataTable
        Static keyValues As Dictionary(Of String, String)
        Static retDt As DataTable
        Dim retFilterdDt As DataTable
        'キー情報を比較または初期状態または異なるキーの場合は再抽出
        If keyValues Is Nothing OrElse
           (Not (keyValues("I_CODE") = I_CODE _
                 AndAlso keyValues("I_CLASS") = I_CLASS _
                 AndAlso keyValues("I_PARA01") = I_PARA01)) Then
            keyValues = New Dictionary(Of String, String) _
                      From {{"I_CODE", I_CODE}, {"I_CLASS", I_CLASS}, {"I_PARA01", I_PARA01}}
            retDt = New DataTable
        Else
            retFilterdDt = retDt
            '抽出キー情報が一致しているので保持内容を返却
            If I_KEYCODE <> "" Then
                Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
                If qKeyFilterd.Any Then
                    retFilterdDt = qKeyFilterd.CopyToDataTable
                Else
                    retFilterdDt = retDt.Clone
                End If
            End If

            Return retFilterdDt
        End If
        'キーが変更された場合の抽出処理
        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)
        SqlConnection.ClearPool(SQLcon)

        '検索SQL文
        Dim SQLStr As String =
           " SELECT" _
            & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '')    AS CAMPCODE" _
            & " , ISNULL(RTRIM(VIW0001.CLASS), '')       AS CLASS" _
            & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')     AS KEYCODE" _
            & " , ISNULL(RTRIM(VIW0001.STYMD), '')       AS STYMD" _
            & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')      AS ENDYMD" _
            & " , ISNULL(RTRIM(VIW0001.VALUE1), '')      AS VALUE1" _
            & " , ISNULL(RTRIM(VIW0001.VALUE2), '')      AS VALUE2" _
            & " , ISNULL(RTRIM(VIW0001.VALUE3), '')      AS VALUE3" _
            & " , ISNULL(RTRIM(VIW0001.VALUE4), '')      AS VALUE4" _
            & " , ISNULL(RTRIM(VIW0001.VALUE5), '')      AS VALUE5" _
            & " , ISNULL(RTRIM(VIW0001.VALUE6), '')      AS VALUE6" _
            & " , ISNULL(RTRIM(VIW0001.VALUE7), '')      AS VALUE7" _
            & " , ISNULL(RTRIM(VIW0001.VALUE8), '')      AS VALUE8" _
            & " , ISNULL(RTRIM(VIW0001.VALUE9), '')      AS VALUE9" _
            & " , ISNULL(RTRIM(VIW0001.VALUE10), '')     AS VALUE10" _
            & " , ISNULL(RTRIM(VIW0001.VALUE11), '')     AS VALUE11" _
            & " , ISNULL(RTRIM(VIW0001.VALUE12), '')     AS VALUE12" _
            & " , ISNULL(RTRIM(VIW0001.VALUE13), '')     AS VALUE13" _
            & " , ISNULL(RTRIM(VIW0001.VALUE14), '')     AS VALUE14" _
            & " , ISNULL(RTRIM(VIW0001.VALUE15), '')     AS VALUE15" _
            & " , ISNULL(RTRIM(VIW0001.VALUE16), '')     AS VALUE16" _
            & " , ISNULL(RTRIM(VIW0001.VALUE17), '')     AS VALUE17" _
            & " , ISNULL(RTRIM(VIW0001.VALUE18), '')     AS VALUE18" _
            & " , ISNULL(RTRIM(VIW0001.VALUE19), '')     AS VALUE19" _
            & " , ISNULL(RTRIM(VIW0001.VALUE20), '')     AS VALUE20" _
            & " , ISNULL(RTRIM(VIW0001.SYSTEMKEYFLG), '')   AS SYSTEMKEYFLG" _
            & " , ISNULL(RTRIM(VIW0001.DELFLG), '')      AS DELFLG" _
            & " FROM  OIL.VIW0001_FIXVALUE VIW0001" _
            & " WHERE VIW0001.CLASS = @P01" _
            & " AND VIW0001.DELFLG <> @P03"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '会社コード
        If Not String.IsNullOrEmpty(I_CODE) Then
            SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    VIW0001.KEYCODE"

        Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            'Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)

            PARA01.Value = I_CLASS
            'PARA02.Value = I_KEYCODE
            PARA03.Value = C_DELETE_FLG.DELETE

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '○ フィールド名とフィールドの型を取得
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    retDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                '○ テーブル検索結果をテーブル格納
                retDt.Load(SQLdr)
            End Using
            'CLOSE
            SQLcmd.Dispose()
        End Using

        retFilterdDt = retDt
        '抽出キー情報が一致しているので保持内容を返却
        If I_KEYCODE <> "" Then
            Dim qKeyFilterd = From dr In retDt Where dr("KEYCODE").Equals(I_KEYCODE)
            If qKeyFilterd.Any Then
                retFilterdDt = qKeyFilterd.CopyToDataTable
            Else
                retFilterdDt = retDt.Clone
            End If
        End If

        Return retFilterdDt
    End Function
End Class

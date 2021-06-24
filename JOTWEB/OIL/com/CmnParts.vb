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
    ''' 回送パターン自動設定用データ取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Public Sub GetKaisouTypeInfo(ByVal SQLcon As SqlConnection, ByVal I_OFFICECODE As String, ByRef dt As DataTable)

        If IsNothing(dt) Then
            dt = New DataTable
        End If

        If dt.Columns.Count <> 0 Then
            dt.Columns.Clear()
        End If

        dt.Clear()

        '○ 取得SQL
        '     条件指定に従い該当データを変換マスタテーブルから取得する
        Dim SQLStr As String =
            " SELECT" _
            & "   OIM0029.KEYCODE01               AS OFFICECODE" _
            & " , OIM0029.KEYCODE02               AS OFFICENAME" _
            & " , OIM0029.KEYCODE03               AS OBJECTIVECODE" _
            & " , OIM0029.KEYCODE04               AS DEFAULTKBN" _
            & " , OIM0029.KEYCODE05               AS PATCODE" _
            & " , OIM0029.KEYCODE06               AS PATNAME" _
            & " , OIM0029.VALUE01                 AS TRAINNO" _
            & " , OIM0029.VALUE02                 AS TRAINNAME" _
            & " , OIM0029.VALUE03                 AS DEPSTATION" _
            & " , OIM0029.VALUE04                 AS DEPSTATIONNAME" _
            & " , OIM0029.VALUE05                 AS ARRSTATION" _
            & " , OIM0029.VALUE06                 AS ARRSTATIONNAME" _
            & " , OIM0029.VALUE07                 AS DEPDAYS" _
            & " , OIM0029.VALUE08                 AS ARRDAYS" _
            & " , OIM0029.VALUE09                 AS DEPSTATIONRTNDAYS" _
            & " , OIM0029.VALUE10                 AS TGHSTATION" _
            & " , OIM0029.VALUE11                 AS TGHSTATIONNAME" _
            & " FROM OIL.OIM0029_CONVERT OIM0029 " _
            & " WHERE OIM0029.CLASS = 'KAISOU_PATTERNMASTER' "
        '& " AND OIM0029.KEYCODE04 = 'def' "

        '回送営業所コード
        SQLStr &= String.Format(" AND OIM0029.KEYCODE01 = '{0}' ", I_OFFICECODE)

        '削除フラグ
        SQLStr &= String.Format(" AND OIM0029.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub
    ''' <summary>
    ''' 計上年月取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Public Function GetKeijyoYM(ByVal SQLcon As SqlConnection) As String

        Dim KEIJYOYMtbl As DataTable = Nothing
        If IsNothing(KEIJYOYMtbl) Then
            KEIJYOYMtbl = New DataTable
        End If

        If KEIJYOYMtbl.Columns.Count <> 0 Then
            KEIJYOYMtbl.Columns.Clear()
        End If

        KEIJYOYMtbl.Clear()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
              " SELECT" _
            & "   FORMAT(OIT0019.KEIJYOYM, 'yyyy/MM') AS KEIJYOYM" _
            & " FROM OIL.OIT0019_KEIJYOYM OIT0019"

        Dim KeijyoYM As String = ""
        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        KEIJYOYMtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    KEIJYOYMtbl.Load(SQLdr)
                End Using
                KeijyoYM = Convert.ToString(KEIJYOYMtbl.Rows(0)("KEIJYOYM"))
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

        Return KeijyoYM
    End Function
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
    Public Sub FixvalueMasterSearch(ByVal I_CODE As String,
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

    ''' <summary>
    ''' 受注TBL登録検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Public Sub SelectOrder(ByVal SQLcon As SqlConnection,
                                 ByVal I_ORDERNO As String,
                                 ByRef O_dtORDER As DataTable,
                                 Optional I_OFFICECODE As String = Nothing,
                                 Optional I_TANKNO As String = Nothing)

        If IsNothing(O_dtORDER) Then
            O_dtORDER = New DataTable
        End If

        If O_dtORDER.Columns.Count <> 0 Then
            O_dtORDER.Columns.Clear()
        End If

        O_dtORDER.Clear()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
            " SELECT" _
            & "   OIT0002.ORDERNO                 AS ORDERNO" _
            & " , OIT0003.DETAILNO                AS DETAILNO" _
            & " , OIT0003.OTDETAILNO              AS OTDETAILNO" _
            & " , OIT0002.TRAINNO                 AS TRAINNO" _
            & " , OIT0002.TRAINNO + 'レ'          AS TRAINNO_NM" _
            & " , OIT0002.TRAINNAME               AS TRAINNAME" _
            & " , OIT0002.ORDERYMD                AS ORDERYMD" _
            & " , OIT0002.OFFICECODE              AS OFFICECODE" _
            & " , OIT0002.OFFICENAME              AS OFFICENAME" _
            & " , OIT0002.ORDERTYPE               AS ORDERTYPE" _
            & " , OIT0002.SHIPPERSCODE            AS SHIPPERSCODE" _
            & " , OIT0002.SHIPPERSNAME            AS SHIPPERSNAME" _
            & " , OIT0002.BASECODE                AS BASECODE" _
            & " , OIT0002.BASENAME                AS BASENAME" _
            & " , OIT0002.CONSIGNEECODE           AS CONSIGNEECODE" _
            & " , OIT0002.CONSIGNEENAME           AS CONSIGNEENAME" _
            & " , OIT0002.DEPSTATION              AS DEPSTATION" _
            & " , OIT0002.DEPSTATIONNAME          AS DEPSTATIONNAME" _
            & " , OIT0002.ARRSTATION              AS ARRSTATION" _
            & " , OIT0002.ARRSTATIONNAME          AS ARRSTATIONNAME" _
            & " , OIT0002.RETSTATION              AS RETSTATION" _
            & " , OIT0002.RETSTATIONNAME          AS RETSTATIONNAME" _
            & " , OIT0002.CHANGERETSTATION        AS CHANGERETSTATION" _
            & " , OIT0002.CHANGERETSTATIONNAME    AS CHANGERETSTATIONNAME" _
            & " , OIT0002.ORDERSTATUS             AS ORDERSTATUS" _
            & " , OIT0002.ORDERINFO               AS ORDERINFO" _
            & " , OIT0002.EMPTYTURNFLG            AS EMPTYTURNFLG" _
            & " , OIT0002.STACKINGFLG             AS STACKINGFLG" _
            & " , OIT0002.USEPROPRIETYFLG         AS USEPROPRIETYFLG" _
            & " , OIT0002.CONTACTFLG              AS CONTACTFLG" _
            & " , OIT0002.RESULTFLG               AS RESULTFLG" _
            & " , OIT0002.DELIVERYFLG             AS DELIVERYFLG" _
            & " , OIT0002.LODDATE                 AS LODDATE" _
            & " , OIT0002.DEPDATE                 AS DEPDATE" _
            & " , OIT0002.ARRDATE                 AS ARRDATE" _
            & " , OIT0002.ACCDATE                 AS ACCDATE" _
            & " , OIT0002.EMPARRDATE              AS EMPARRDATE" _
            & " , OIT0002.ACTUALLODDATE           AS ACTUALLODDATE" _
            & " , OIT0002.ACTUALDEPDATE           AS ACTUALDEPDATE" _
            & " , OIT0002.ACTUALARRDATE           AS ACTUALARRDATE" _
            & " , OIT0002.ACTUALACCDATE           AS ACTUALACCDATE" _
            & " , OIT0002.ACTUALEMPARRDATE        AS ACTUALEMPARRDATE" _
            & " , OIT0002.RTANK                   AS RTANK" _
            & " , OIT0002.HTANK                   AS HTANK" _
            & " , OIT0002.TTANK                   AS TTANK" _
            & " , OIT0002.MTTANK                  AS MTTANK" _
            & " , OIT0002.KTANK                   AS KTANK" _
            & " , OIT0002.K3TANK                  AS K3TANK" _
            & " , OIT0002.K5TANK                  AS K5TANK" _
            & " , OIT0002.K10TANK                 AS K10TANK" _
            & " , OIT0002.LTANK                   AS LTANK" _
            & " , OIT0002.ATANK                   AS ATANK" _
            & " , OIT0002.OTHER1OTANK             AS OTHER1OTANK" _
            & " , OIT0002.OTHER2OTANK             AS OTHER2OTANK" _
            & " , OIT0002.OTHER3OTANK             AS OTHER3OTANK" _
            & " , OIT0002.OTHER4OTANK             AS OTHER4OTANK" _
            & " , OIT0002.OTHER5OTANK             AS OTHER5OTANK" _
            & " , OIT0002.OTHER6OTANK             AS OTHER6OTANK" _
            & " , OIT0002.OTHER7OTANK             AS OTHER7OTANK" _
            & " , OIT0002.OTHER8OTANK             AS OTHER8OTANK" _
            & " , OIT0002.OTHER9OTANK             AS OTHER9OTANK" _
            & " , OIT0002.OTHER10OTANK            AS OTHER10OTANK" _
            & " , OIT0002.TOTALTANK               AS TOTALTANK" _
            & " , OIT0002.RTANKCH                 AS RTANKCH" _
            & " , OIT0002.HTANKCH                 AS HTANKCH" _
            & " , OIT0002.TTANKCH                 AS TTANKCH" _
            & " , OIT0002.MTTANKCH                AS MTTANKCH" _
            & " , OIT0002.KTANKCH                 AS KTANKCH" _
            & " , OIT0002.K3TANKCH                AS K3TANKCH" _
            & " , OIT0002.K5TANKCH                AS K5TANKCH" _
            & " , OIT0002.K10TANKCH               AS K10TANKCH" _
            & " , OIT0002.LTANKCH                 AS LTANKCH" _
            & " , OIT0002.ATANKCH                 AS ATANKCH" _
            & " , OIT0002.OTHER1OTANKCH           AS OTHER1OTANKCH" _
            & " , OIT0002.OTHER2OTANKCH           AS OTHER2OTANKCH" _
            & " , OIT0002.OTHER3OTANKCH           AS OTHER3OTANKCH" _
            & " , OIT0002.OTHER4OTANKCH           AS OTHER4OTANKCH" _
            & " , OIT0002.OTHER5OTANKCH           AS OTHER5OTANKCH" _
            & " , OIT0002.OTHER6OTANKCH           AS OTHER6OTANKCH" _
            & " , OIT0002.OTHER7OTANKCH           AS OTHER7OTANKCH" _
            & " , OIT0002.OTHER8OTANKCH           AS OTHER8OTANKCH" _
            & " , OIT0002.OTHER9OTANKCH           AS OTHER9OTANKCH" _
            & " , OIT0002.OTHER10OTANKCH          AS OTHER10OTANKCH" _
            & " , OIT0002.TOTALTANKCH             AS TOTALTANKCH" _
            & " , OIT0002.TANKLINKNO              AS TANKLINKNO" _
            & " , OIT0002.TANKLINKNOMADE          AS TANKLINKNOMADE" _
            & " , OIT0002.BILLINGNO               AS BILLINGNO" _
            & " , OIT0002.KEIJYOYMD               AS KEIJYOYMD" _
            & " , OIT0002.SALSE                   AS SALSE" _
            & " , OIT0002.SALSETAX                AS SALSETAX" _
            & " , OIT0002.TOTALSALSE              AS TOTALSALSE" _
            & " , OIT0002.PAYMENT                 AS PAYMENT" _
            & " , OIT0002.PAYMENTTAX              AS PAYMENTTAX" _
            & " , OIT0002.TOTALPAYMENT            AS TOTALPAYMENT" _
            & " , OIT0002.OTFILENAME              AS OTFILENAME" _
            & " , OIT0002.RECEIVECOUNT            AS RECEIVECOUNT" _
            & " , OIT0003.SHIPORDER               AS SHIPORDER" _
            & " , OIT0003.LINEORDER               AS LINEORDER" _
            & " , OIT0003.TANKNO                  AS TANKNO" _
            & " , OIT0003.KAMOKU                  AS KAMOKU" _
            & " , OIT0003.STACKINGORDERNO         AS STACKINGORDERNO" _
            & " , OIT0003.STACKINGFLG             AS DETAIL_STACKINGFLG" _
            & " , OIT0003.WHOLESALEFLG            AS WHOLESALEFLG" _
            & " , OIT0003.INSPECTIONFLG           AS INSPECTIONFLG" _
            & " , OIT0003.DETENTIONFLG            AS DETENTIONFLG" _
            & " , OIT0003.FIRSTRETURNFLG          AS FIRSTRETURNFLG" _
            & " , OIT0003.AFTERRETURNFLG          AS AFTERRETURNFLG" _
            & " , OIT0003.OTTRANSPORTFLG          AS OTTRANSPORTFLG" _
            & " , OIT0003.UPGRADEFLG              AS UPGRADEFLG" _
            & " , OIT0003.TESTPRODUCTFLG          AS TESTPRODUCTFLG" _
            & " , OIT0003.ORDERINFO               AS DETAIL_ORDERINFO" _
            & " , OIT0003.SHIPPERSCODE            AS DETAIL_SHIPPERSCODE" _
            & " , OIT0003.SHIPPERSNAME            AS DETAIL_SHIPPERSNAME" _
            & " , OIT0003.OILCODE                 AS OILCODE" _
            & " , OIT0003.OILNAME                 AS OILNAME" _
            & " , OIT0003.ORDERINGTYPE            AS ORDERINGTYPE" _
            & " , OIT0003.ORDERINGOILNAME         AS ORDERINGOILNAME" _
            & " , OIT0003.CARSNUMBER              AS CARSNUMBER" _
            & " , OIT0003.CARSAMOUNT              AS CARSAMOUNT" _
            & " , OIT0003.RETURNDATETRAIN         AS RETURNDATETRAIN" _
            & " , OIT0003.JOINTCODE               AS JOINTCODE" _
            & " , OIT0003.JOINT                   AS JOINT" _
            & " , OIT0003.REMARK                  AS REMARK" _
            & " , OIT0003.CHANGETRAINNO           AS CHANGETRAINNO" _
            & " , OIT0003.CHANGETRAINNAME         AS CHANGETRAINNAME" _
            & " , OIT0003.SECONDCONSIGNEECODE     AS SECONDCONSIGNEECODE" _
            & " , OIT0003.SECONDCONSIGNEENAME     AS SECONDCONSIGNEENAME" _
            & " , OIT0003.SECONDARRSTATION        AS SECONDARRSTATION" _
            & " , OIT0003.SECONDARRSTATIONNAME    AS SECONDARRSTATIONNAME" _
            & " , OIT0003.CHANGERETSTATION        AS DETAIL_CHANGERETSTATION" _
            & " , OIT0003.CHANGERETSTATIONNAME    AS DETAIL_CHANGERETSTATIONNAME" _
            & " , OIT0003.LINE                    AS LINE" _
            & " , OIT0003.FILLINGPOINT            AS FILLINGPOINT" _
            & " , OIT0003.LOADINGIRILINETRAINNO   AS LOADINGIRILINETRAINNO" _
            & " , OIT0003.LOADINGIRILINETRAINNAME AS LOADINGIRILINETRAINNAME" _
            & " , OIT0003.LOADINGIRILINEORDER     AS LOADINGIRILINEORDER" _
            & " , OIT0003.LOADINGOUTLETTRAINNO    AS LOADINGOUTLETTRAINNO" _
            & " , OIT0003.LOADINGOUTLETTRAINNAME  AS LOADINGOUTLETTRAINNAME" _
            & " , OIT0003.LOADINGOUTLETORDER      AS LOADINGOUTLETORDER" _
            & " , OIT0003.ACTUALLODDATE           AS DETAIL_ACTUALLODDATE" _
            & " , OIT0003.ACTUALDEPDATE           AS DETAIL_ACTUALDEPDATE" _
            & " , OIT0003.ACTUALARRDATE           AS DETAIL_ACTUALARRDATE" _
            & " , OIT0003.ACTUALACCDATE           AS DETAIL_ACTUALACCDATE" _
            & " , OIT0003.ACTUALEMPARRDATE        AS DETAIL_ACTUALEMPARRDATE" _
            & " , OIT0003.RESERVEDNO              AS RESERVEDNO" _
            & " , OIT0003.GYONO                   AS GYONO" _
            & " , OIT0003.OTSENDCOUNT             AS OTSENDCOUNT" _
            & " , OIT0003.DLRESERVEDCOUNT         AS DLRESERVEDCOUNT" _
            & " , OIT0003.DLTAKUSOUCOUNT          AS DLTAKUSOUCOUNT" _
            & " , OIT0003.SALSE                   AS DETAIL_SALSE" _
            & " , OIT0003.SALSETAX                AS DETAIL_SALSETAX" _
            & " , OIT0003.TOTALSALSE              AS DETAIL_TOTALSALSE" _
            & " , OIT0003.PAYMENT                 AS DETAIL_PAYMENT" _
            & " , OIT0003.PAYMENTTAX              AS DETAIL_PAYMENTTAX" _
            & " , OIT0003.TOTALPAYMENT            AS DETAIL_TOTALPAYMENT" _
            & " , OIT0003.ANASYORIFLG             AS ANASYORIFLG" _
            & " , OIT0003.VOLSYORIFLG             AS VOLSYORIFLG" _
            & " , OIT0003.TANKBACKORDERNO         AS TANKBACKORDERNO" _
            & " , OIT0003.TANKBACKINFO            AS TANKBACKINFO" _
            & " , OIT0003.DELFLG                  AS DELFLG" _
            & " , OIT0003.INITYMD                 AS INITYMD" _
            & " , OIT0003.INITUSER                AS INITUSER" _
            & " , OIT0003.INITTERMID              AS INITTERMID" _
            & " , OIT0003.UPDYMD                  AS UPDYMD" _
            & " , OIT0003.UPDUSER                 AS UPDUSER" _
            & " , OIT0003.UPDTERMID               AS UPDTERMID" _
            & " , OIT0003.RECEIVEYMD              AS RECEIVEYMD" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO "

        '○ 検索条件が指定されていれば追加する
        'タンク車№
        If Not String.IsNullOrEmpty(I_TANKNO) Then
            SQLStr &= String.Format(" AND OIT0003.TANKNO = '{0}' ", I_TANKNO)
        End If
        '削除フラグ
        SQLStr &= String.Format(" AND OIT0003.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        '受注No
        SQLStr &= String.Format(" WHERE OIT0002.ORDERNO = '{0}' ", I_ORDERNO)
        '受注営業所コード
        If Not String.IsNullOrEmpty(I_OFFICECODE) Then
            SQLStr &= String.Format(" AND OIT0002.OFFICECODE = '{0}' ", I_OFFICECODE)
        End If
        '受注進行ステータス
        SQLStr &= String.Format(" AND OIT0002.ORDERSTATUS <> '{0}' ", BaseDllConst.CONST_ORDERSTATUS_900)
        '削除フラグ
        SQLStr &= String.Format(" AND OIT0002.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtORDER.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtORDER.Load(SQLdr)
                End Using

                'Dim i As Integer = 0
                'For Each O_dtORDERrow As DataRow In O_dtORDER.Rows
                '    i += 1
                '    O_dtORDERrow("LINECNT") = i        'LINECNT
                'Next

            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try

    End Sub
    ''' <summary>
    ''' 回送TBL登録検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <remarks></remarks>
    Public Sub SelectKaisou(ByVal SQLcon As SqlConnection,
                                 ByVal I_KAISOUNO As String,
                                 ByRef O_dtKAISOU As DataTable,
                                 Optional I_OFFICECODE As String = Nothing,
                                 Optional I_TANKNO As String = Nothing)

        If IsNothing(O_dtKAISOU) Then
            O_dtKAISOU = New DataTable
        End If

        If O_dtKAISOU.Columns.Count <> 0 Then
            O_dtKAISOU.Columns.Clear()
        End If

        O_dtKAISOU.Clear()

        '○ 検索SQL
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String =
              " SELECT " _
            & "   OIT0006.KAISOUNO" _
            & " , OIT0007.DETAILNO" _
            & " , OIT0006.KAISOUYMD" _
            & " , OIT0006.KAISOUSTATUS" _
            & " , OIT0007.TRAINNO" _
            & " , OIT0007.OBJECTIVECODE" _
            & " , OIT0007.KAISOUTYPE" _
            & " , OIT0006.OFFICECODE" _
            & " , OIT0006.OFFICENAME" _
            & " , OIT0007.TANKNO" _
            & " , OIT0007.DEPSTATION" _
            & " , OIT0007.DEPSTATIONNAME" _
            & " , OIT0007.TGHSTATION" _
            & " , OIT0007.TGHSTATIONNAME" _
            & " , OIT0007.ARRSTATION" _
            & " , OIT0007.ARRSTATIONNAME" _
            & " , OIT0007.ACTUALDEPDATE" _
            & " , OIT0007.ACTUALEMPARRDATE" _
            & " , OIT0006.TOTALREPAIR" _
            & " , OIT0006.TOTALMC" _
            & " , OIT0006.TOTALINSPECTION" _
            & " , OIT0006.TOTALALLINSPECTION" _
            & " , OIT0006.TOTALINDWELLING" _
            & " , OIT0006.TOTALMOVE" _
            & " , OIT0006.TOTALTANK" _
            & " FROM oil.OIT0006_KAISOU OIT0006" _
            & " INNER JOIN oil.OIT0007_KAISOUDETAIL OIT0007 ON" _
            & " OIT0007.KAISOUNO = OIT0006.KAISOUNO" _

        '○ 検索条件が指定されていれば追加する
        'タンク車№
        If Not String.IsNullOrEmpty(I_TANKNO) Then
            SQLStr &= String.Format(" AND OIT0007.TANKNO = '{0}' ", I_TANKNO)
        End If
        '削除フラグ
        SQLStr &= String.Format(" AND OIT0007.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        '回送No
        SQLStr &= String.Format(" WHERE OIT0006.KAISOUNO = '{0}' ", I_KAISOUNO)
        '回送営業所コード
        If Not String.IsNullOrEmpty(I_OFFICECODE) Then
            SQLStr &= String.Format(" AND OIT0006.OFFICECODE = '{0}' ", I_OFFICECODE)
        End If
        '利用可否フラグ
        SQLStr &= String.Format(" AND OIT0006.USEPROPRIETYFLG = '{0}' ", "1")
        '回送進行ステータス
        SQLStr &= String.Format(" AND OIT0006.KAISOUSTATUS <> '{0}' ", BaseDllConst.CONST_KAISOUSTATUS_900)
        '削除フラグ
        SQLStr &= String.Format(" AND OIT0006.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE)

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtKAISOU.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtKAISOU.Load(SQLdr)
                End Using

                'Dim i As Integer = 0
                'For Each O_dtORDERrow As DataRow In O_dtORDER.Rows
                '    i += 1
                '    O_dtORDERrow("LINECNT") = i        'LINECNT
                'Next

            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' タンク車マスタTBL検索
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <param name="I_OFFICEFLG">営業所区分</param>
    ''' <param name="O_dtTANKMAS">検索結果取得用</param>
    ''' <param name="I_OTFLG">OTリース車対象有無</param>
    ''' <remarks></remarks>
    Public Sub SelectTankMaster(ByVal SQLcon As SqlConnection,
                                ByVal I_OFFICEFLG As String,
                                 ByRef O_dtTANKMAS As DataTable,
                                 Optional I_OTFLG As Boolean = False)
        If IsNothing(O_dtTANKMAS) Then
            O_dtTANKMAS = New DataTable
        End If

        If O_dtTANKMAS.Columns.Count <> 0 Then
            O_dtTANKMAS.Columns.Clear()
        End If

        O_dtTANKMAS.Clear()

        Dim SQLStr As String = ""
        Dim SQLCmnStr As String =
              " SELECT " _
            & "   OIM0005.TANKNUMBER" _
            & " , OIM0005.MODEL" _
            & " , OIM0005.MODELKANA" _
            & " , OIM0005.LOAD" _
            & " , OIM0005.LOADUNIT" _
            & " , OIM0005.VOLUME" _
            & " , OIM0005.VOLUMEUNIT" _
            & " , OIM0005.MYWEIGHT" _
            & " , OIM0005.LENGTH" _
            & " , OIM0005.TANKLENGTH" _
            & " , OIM0005.MAXCALIBER" _
            & " , OIM0005.MINCALIBER" _
            & " , OIM0005.LENGTHFLG" _
            & " , OIM0005.ORIGINOWNERCODE" _
            & " , OIM0005.ORIGINOWNERNAME" _
            & " , OIM0005.OWNERCODE" _
            & " , OIM0005.OWNERNAME" _
            & " , OIM0005.LEASECODE" _
            & " , OIM0005.LEASENAME" _
            & " , OIM0005.LEASECLASS" _
            & " , OIM0005.LEASECLASSNAME" _
            & " , OIM0005.AUTOEXTENTION" _
            & " , OIM0005.AUTOEXTENTIONNAME" _
            & " , OIM0005.LEASESTYMD" _
            & " , OIM0005.LEASEENDYMD" _
            & " , OIM0005.USERCODE" _
            & " , OIM0005.USERNAME" _
            & " , OIM0005.CURRENTSTATIONCODE" _
            & " , OIM0005.CURRENTSTATIONNAME" _
            & " , OIM0005.EXTRADINARYSTATIONCODE" _
            & " , OIM0005.EXTRADINARYSTATIONNAME" _
            & " , OIM0005.USERLIMIT" _
            & " , OIM0005.LIMITTEXTRADIARYSTATION" _
            & " , OIM0005.DEDICATETYPECODE" _
            & " , OIM0005.DEDICATETYPENAME" _
            & " , OIM0005.EXTRADINARYTYPECODE" _
            & " , OIM0005.EXTRADINARYTYPENAME" _
            & " , OIM0005.EXTRADINARYLIMIT" _
            & " , OIM0005.BIGOILCODE" _
            & " , OIM0005.BIGOILNAME" _
            & " , OIM0005.MIDDLEOILCODE" _
            & " , OIM0005.MIDDLEOILNAME" _
            & " , OIM0005.DOWNLOADDATE" _
            & " , OIM0005.OPERATIONBASECODE" _
            & " , OIM0005.OPERATIONBASENAME" _
            & " , OIM0005.COLORCODE" _
            & " , OIM0005.COLORNAME" _
            & " , OIM0005.MARKCODE" _
            & " , OIM0005.MARKNAME" _
            & " , OIM0005.JXTGTAGCODE1" _
            & " , OIM0005.JXTGTAGNAME1" _
            & " , OIM0005.JXTGTAGCODE2" _
            & " , OIM0005.JXTGTAGNAME2" _
            & " , OIM0005.JXTGTAGCODE3" _
            & " , OIM0005.JXTGTAGNAME3" _
            & " , OIM0005.JXTGTAGCODE4" _
            & " , OIM0005.JXTGTAGNAME4" _
            & " , OIM0005.IDSSTAGCODE" _
            & " , OIM0005.IDSSTAGNAME" _
            & " , OIM0005.COSMOTAGCODE" _
            & " , OIM0005.COSMOTAGNAME" _
            & " , OIM0005.RESERVE1" _
            & " , OIM0005.RESERVE2" _
            & " , OIM0005.JRINSPECTIONDATE" _
            & " , OIM0005.INSPECTIONDATE" _
            & " , OIM0005.JRSPECIFIEDDATE" _
            & " , OIM0005.SPECIFIEDDATE" _
            & " , OIM0005.JRALLINSPECTIONDATE" _
            & " , OIM0005.ALLINSPECTIONDATE" _
            & " , OIM0005.PREINSPECTIONDATE" _
            & " , OIM0005.GETDATE" _
            & " , OIM0005.TRANSFERDATE" _
            & " , OIM0005.OBTAINEDCODE" _
            & " , OIM0005.OBTAINEDNAME" _
            & " , OIM0005.PROGRESSYEAR" _
            & " , OIM0005.NEXTPROGRESSYEAR" _
            & " , OIM0005.EXCLUDEDATE" _
            & " , OIM0005.RETIRMENTDATE" _
            & " , OIM0005.JRTANKNUMBER" _
            & " , OIM0005.JRTANKTYPE" _
            & " , OIM0005.OLDTANKNUMBER" _
            & " , OIM0005.OTTANKNUMBER" _
            & " , OIM0005.JXTGTANKNUMBER1" _
            & " , OIM0005.JXTGTANKNUMBER2" _
            & " , OIM0005.JXTGTANKNUMBER3" _
            & " , OIM0005.JXTGTANKNUMBER4" _
            & " , OIM0005.COSMOTANKNUMBER" _
            & " , OIM0005.FUJITANKNUMBER" _
            & " , OIM0005.SHELLTANKNUMBER" _
            & " , OIM0005.SAPSHELLTANKNUMBER" _
            & " , OIM0005.RESERVE3" _
            & " , OIM0005.USEDFLG" _
            & " , OIM0005.INTERINSPECTYM" _
            & " , OIM0005.INTERINSPECTSTATION" _
            & " , OIM0005.INTERINSPECTORGCODE" _
            & " , OIM0005.SELFINSPECTYM" _
            & " , OIM0005.SELFINSPECTSTATION" _
            & " , OIM0005.SELFINSPECTORGCODE" _
            & " , OIM0005.INSPECTMEMBERNAME" _
            & " , OIM0005.DELFLG" _
            & " , OIM0005.INITYMD" _
            & " , OIM0005.INITUSER" _
            & " , OIM0005.INITTERMID" _
            & " , OIM0005.UPDYMD" _
            & " , OIM0005.UPDUSER" _
            & " , OIM0005.UPDTERMID" _
            & " , OIM0005.RECEIVEYMD" _
            & " FROM oil.OIM0005_TANK OIM0005"

        SQLStr = SQLCmnStr

        Select Case I_OFFICEFLG
            '○関東支店(千葉３営業所用(五井営業所、甲子営業所、袖ヶ浦営業所))
            Case "011409"
                SQLStr &= String.Format(" WHERE OIM0005.OPERATIONBASECODE IN ('{0}','{1}','{2}','{3}')", BaseDllConst.CONST_OFFICECODE_011401, BaseDllConst.CONST_OFFICECODE_011201, BaseDllConst.CONST_OFFICECODE_011202, BaseDllConst.CONST_OFFICECODE_011203)
                ''○東北支店(仙台新港営業所)
                'Case "010401"
                '    SQLStr &= String.Format(" WHERE OIM0005.OPERATIONBASECODE IN ('{0}','{1}')", BaseDllConst.CONST_OFFICECODE_010401, BaseDllConst.CONST_OFFICECODE_010402)
                ''○関東支店(五井営業所、甲子営業所、袖ヶ浦営業所、根岸営業所)
                'Case "011401"
                '    SQLStr &= String.Format(" WHERE OIM0005.OPERATIONBASECODE IN ('{0}','{1}','{2}','{3}','{4}')", BaseDllConst.CONST_OFFICECODE_011401, BaseDllConst.CONST_OFFICECODE_011201, BaseDllConst.CONST_OFFICECODE_011202, BaseDllConst.CONST_OFFICECODE_011203, BaseDllConst.CONST_OFFICECODE_011402)
                ''○関東支店(根岸営業所のみ)
                'Case "011402"
                '    SQLStr &= String.Format(" WHERE OIM0005.OPERATIONBASECODE IN ('{0}','{1}')", BaseDllConst.CONST_OFFICECODE_011401, BaseDllConst.CONST_OFFICECODE_011402)
                ''○中部支店(四日市営業所、三重塩浜営業所)
                'Case "012301"
                '    SQLStr &= String.Format(" WHERE OIM0005.OPERATIONBASECODE IN ('{0}','{1}','{2}')", BaseDllConst.CONST_OFFICECODE_012301, BaseDllConst.CONST_OFFICECODE_012401, BaseDllConst.CONST_OFFICECODE_012402)
        End Select

        '○OTリース対象の場合は対象に含める
        If I_OTFLG = True Then
            SQLStr &=
              " UNION " _
            & SQLCmnStr _
            & String.Format(" WHERE OIM0005.OWNERCODE = '{0}'", BaseDllConst.CONST_CAMPCODE_11) _
            & String.Format("    OR OIM0005.LEASECODE = '{0}'", BaseDllConst.CONST_CAMPCODE_11) _
            & String.Format("    OR OIM0005.ORIGINOWNERCODE = '{0}'", BaseDllConst.CONST_CAMPCODE_70)
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        O_dtTANKMAS.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    O_dtTANKMAS.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub


    ''' <summary>
    ''' 帳票ファイル名取得
    ''' </summary>
    ''' <returns></returns>
    ''' <param name="I_REPTYPE">帳票区分</param>
    ''' <param name="I_OFFICECODE">営業所コード</param>
    ''' <param name="I_LODDATE">積込日</param>
    ''' <param name="I_TRAINNO">列車番号</param>
    Public Function SetReportFileName(ByVal I_REPTYPE As String, ByVal I_OFFICECODE As String, ByVal I_LODDATE As String, ByVal I_TRAINNO As String) As String
        Dim fileName As String = ""

        '○帳票名
        Select Case I_REPTYPE
            '★空回日報
            Case "KUUKAI_SODEGAURA", "KUUKAI_LIST", "KUUKAI_MEISAI"
                Select Case I_REPTYPE
                    '○受注一覧の帳票（袖ヶ浦）, 空回日報明細からの帳票
                    Case "KUUKAI_SODEGAURA", "KUUKAI_MEISAI"
                        fileName = Date.Parse(I_LODDATE).ToString("MMdd") & "空回日報" & ".xlsx"
                    '○空回日報一覧からの帳票
                    Case "KUUKAI_LIST"
                        fileName = "空回日報" & ".xlsx"
                End Select

            '★積込指示書
            Case "LOADPLAN"
                '○営業所
                Select Case I_OFFICECODE
                    '★五井営業所, 袖ヶ浦営業所
                    Case BaseDllConst.CONST_OFFICECODE_011201,
                         BaseDllConst.CONST_OFFICECODE_011203
                        fileName = "積込指示書" & ".xlsx"
                    '★甲子営業所
                    Case BaseDllConst.CONST_OFFICECODE_011202
                        fileName = "タンク車積込指示書" & ".xlsx"
                    '★根岸営業所
                    Case BaseDllConst.CONST_OFFICECODE_011402
                        fileName = "回線別出荷予定表" & ".xlsx"
                    '★仙台新港営業所, 四日市営業所, 三重塩浜営業所
                    Case BaseDllConst.CONST_OFFICECODE_010402,
                         BaseDllConst.CONST_OFFICECODE_012401,
                         BaseDllConst.CONST_OFFICECODE_012402
                        fileName = "積込指示書" & Date.Parse(I_LODDATE).ToString("yyyy年MM月dd日") & ".xlsx"
                End Select

            '★OT積込指示書
            Case "OTLOADPLAN"
                '○営業所
                Select Case I_OFFICECODE
                    '★仙台新港営業所
                    Case BaseDllConst.CONST_OFFICECODE_010402
                        fileName = "OT積込指示書" & Date.Parse(I_LODDATE).ToString("yyyy年MM月dd日") & ".xlsx"
                End Select

            '★積込予定表(甲子用)
            Case "KINOENE_LOADPLAN"
                '○営業所
                Select Case I_OFFICECODE
                    '★甲子営業所
                    Case BaseDllConst.CONST_OFFICECODE_011202
                        fileName = "回線別タンク車積込指示書" & ".xlsx"
                End Select

            '★出荷予定表
            Case "SHIPPLAN"
                '○営業所
                Select Case I_OFFICECODE
                    '★根岸営業所, 五井営業所, 袖ヶ浦営業所, 三重塩浜営業所
                    Case BaseDllConst.CONST_OFFICECODE_011402,
                         BaseDllConst.CONST_OFFICECODE_011201,
                         BaseDllConst.CONST_OFFICECODE_011203,
                         BaseDllConst.CONST_OFFICECODE_012402
                        fileName = "出荷予定表" & ".xlsx"
                    '★甲子営業所
                    Case BaseDllConst.CONST_OFFICECODE_011202
                        fileName = "タンク車出荷予定表" & ".xlsx"
                End Select

            '★回線別充填ポイント表
            Case "FILLINGPOINT"
                '○営業所
                Select Case I_OFFICECODE
                    '★五井営業所
                    Case BaseDllConst.CONST_OFFICECODE_011201
                        fileName = "充填ポイント入線表" & ".xlsx"
                End Select

            '★入線方
            Case "LINEPLAN"
                '○営業所
                Select Case I_OFFICECODE
                    '★袖ヶ浦営業所
                    Case BaseDllConst.CONST_OFFICECODE_011203
                        fileName = Date.Parse(I_LODDATE).ToString("MMdd") & StrConv(I_TRAINNO, VbStrConv.Wide) & "入線方" & ".xlsx"
                End Select

            '★託送指示
            Case "DELIVERYPLAN"
                '○営業所
                Select Case I_OFFICECODE
                    '★三重塩浜営業所
                    Case BaseDllConst.CONST_OFFICECODE_012402
                        fileName = "託送状" & ".xlsx"
                End Select

            '★タンク車出荷連絡書
            Case "SHIPCONTACT"
                '○営業所
                Select Case I_OFFICECODE
                    '★三重塩浜営業所
                    Case BaseDllConst.CONST_OFFICECODE_012402
                        fileName = "タンク車出荷連絡書" & Date.Parse(I_LODDATE).ToString("yyyy年MM月dd日") & ".xlsx"
                End Select

        End Select

        Return fileName
    End Function

    ''' <summary>
    ''' 受注TBL更新(訂正更新用)
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Public Sub UpdateOrderCRT(ByVal SQLcon As SqlConnection, ByVal I_ORDERNO As String, ByVal I_Master As OILMasterPage,
                              ByVal I_Value As String, Optional ByVal I_PARA As String = Nothing)

        '更新SQL文
        Dim SQLStr As String =
              " UPDATE OIL.OIT0002_ORDER " _
            & "    SET "

        If Not String.IsNullOrEmpty(I_PARA) Then
            SQLStr &= String.Format("        {0}   = @VALUE, ", I_PARA)
        End If

        SQLStr &=
              "        UPDYMD      = @UPDYMD, " _
            & "        UPDUSER     = @UPDUSER, " _
            & "        UPDTERMID   = @UPDTERMID, " _
            & "        RECEIVEYMD  = @RECEIVEYMD  " _
            & "  WHERE ORDERNO     = @ORDERNO  " _
            & "    AND DELFLG     <> @DELFLG; "

        Try
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar)
            Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
            Dim P_VALUE As SqlParameter = SQLcmd.Parameters.Add("@VALUE", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            P_ORDERNO.Value = I_ORDERNO
            P_DELFLG.Value = C_DELETE_FLG.DELETE

            If I_Value <> "" Then
                P_VALUE.Value = I_Value
            Else
                P_VALUE.Value = DBNull.Value
            End If

            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = I_Master.USERID
            P_UPDTERMID.Value = I_Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' 受注明細TBL更新(訂正更新用)
    ''' </summary>
    ''' <param name="SQLcon">SQL接続</param>
    ''' <remarks></remarks>
    Public Sub UpdateOrderDetailCRT(ByVal SQLcon As SqlConnection, ByVal I_dtRow As DataRow, ByVal I_Master As OILMasterPage,
                                    ByVal I_Value As String, Optional ByVal I_PARA As String = Nothing)

        '更新SQL文
        Dim SQLStr As String =
              " UPDATE OIL.OIT0003_DETAIL " _
            & "    SET "

        If Not String.IsNullOrEmpty(I_PARA) Then
            SQLStr &= String.Format("        {0}   = @VALUE, ", I_PARA)
        End If

        SQLStr &=
              "        UPDYMD      = @UPDYMD, " _
            & "        UPDUSER     = @UPDUSER, " _
            & "        UPDTERMID   = @UPDTERMID, " _
            & "        RECEIVEYMD  = @RECEIVEYMD  " _
            & "  WHERE ORDERNO     = @ORDERNO  " _
            & "    AND DETAILNO    = @DETAILNO  " _
            & "    AND TANKNO      = @TANKNO  " _
            & "    AND DELFLG     <> @DELFLG; "

        Try
            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim P_ORDERNO As SqlParameter = SQLcmd.Parameters.Add("@ORDERNO", System.Data.SqlDbType.NVarChar)
            Dim P_DETAILNO As SqlParameter = SQLcmd.Parameters.Add("@DETAILNO", System.Data.SqlDbType.NVarChar)
            Dim P_TANKNO As SqlParameter = SQLcmd.Parameters.Add("@TANKNO", System.Data.SqlDbType.NVarChar)
            Dim P_DELFLG As SqlParameter = SQLcmd.Parameters.Add("@DELFLG", System.Data.SqlDbType.NVarChar)
            Dim P_VALUE As SqlParameter = SQLcmd.Parameters.Add("@VALUE", System.Data.SqlDbType.NVarChar)

            Dim P_UPDYMD As SqlParameter = SQLcmd.Parameters.Add("@UPDYMD", System.Data.SqlDbType.DateTime)
            Dim P_UPDUSER As SqlParameter = SQLcmd.Parameters.Add("@UPDUSER", System.Data.SqlDbType.NVarChar)
            Dim P_UPDTERMID As SqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", System.Data.SqlDbType.NVarChar)
            Dim P_RECEIVEYMD As SqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime)

            P_ORDERNO.Value = Convert.ToString(I_dtRow("ORDERNO"))
            P_DETAILNO.Value = Convert.ToString(I_dtRow("DETAILNO"))
            P_TANKNO.Value = Convert.ToString(I_dtRow("TANKNO"))
            P_DELFLG.Value = C_DELETE_FLG.DELETE

            If I_Value <> "" Then
                P_VALUE.Value = I_Value
            Else
                P_VALUE.Value = DBNull.Value
            End If

            P_UPDYMD.Value = Date.Now
            P_UPDUSER.Value = I_Master.USERID
            P_UPDTERMID.Value = I_Master.USERTERMID
            P_RECEIVEYMD.Value = C_DEFAULT_YMD

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Throw '呼び出し元の例外にスロー
        End Try
    End Sub

    ''' <summary>
    ''' 受注明細TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="drOrder"></param>
    Public Sub InsertOrderDetail(sqlCon As SqlConnection, sqlTran As SqlTransaction, drOrder As DataRow)

        '◯受注明細TBL
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine("INSERT INTO OIL.OIT0003_DETAIL")
        sqlDetailStat.AppendLine("   (ORDERNO,DETAILNO,OTDETAILNO,SHIPORDER,LINEORDER,TANKNO,KAMOKU,")
        sqlDetailStat.AppendLine("    STACKINGORDERNO,STACKINGFLG,WHOLESALEFLG,INSPECTIONFLG,DETENTIONFLG,")
        sqlDetailStat.AppendLine("    FIRSTRETURNFLG,AFTERRETURNFLG,OTTRANSPORTFLG,UPGRADEFLG,TESTPRODUCTFLG,")
        sqlDetailStat.AppendLine("    ORDERINFO,SHIPPERSCODE,SHIPPERSNAME,OILCODE,OILNAME,ORDERINGTYPE,ORDERINGOILNAME,")
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
        sqlDetailStat.AppendLine("    RESERVEDNO,GYONO,OTSENDCOUNT,DLRESERVEDCOUNT,DLTAKUSOUCOUNT,")
        sqlDetailStat.AppendLine("    SALSE,SALSETAX,TOTALSALSE,PAYMENT,PAYMENTTAX,TOTALPAYMENT,ANASYORIFLG,VOLSYORIFLG,")
        sqlDetailStat.AppendLine("    TANKBACKORDERNO,TANKBACKINFO,")
        sqlDetailStat.AppendLine("    DELFLG,INITYMD,INITUSER,INITTERMID,")
        sqlDetailStat.AppendLine("    UPDYMD,UPDUSER,UPDTERMID,RECEIVEYMD )")
        sqlDetailStat.AppendLine("    VALUES")
        sqlDetailStat.AppendLine("   (@ORDERNO,@DETAILNO,@OTDETAILNO,@SHIPORDER,@LINEORDER,@TANKNO,@KAMOKU,")
        sqlDetailStat.AppendLine("    @STACKINGORDERNO,@STACKINGFLG,@WHOLESALEFLG,@INSPECTIONFLG,@DETENTIONFLG,")
        sqlDetailStat.AppendLine("    @FIRSTRETURNFLG,@AFTERRETURNFLG,@OTTRANSPORTFLG,@UPGRADEFLG,@TESTPRODUCTFLG,")
        sqlDetailStat.AppendLine("    @ORDERINFO,@SHIPPERSCODE,@SHIPPERSNAME,@OILCODE,@OILNAME,@ORDERINGTYPE,@ORDERINGOILNAME,")
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
        sqlDetailStat.AppendLine("    @RESERVEDNO,@GYONO,@OTSENDCOUNT,@DLRESERVEDCOUNT,@DLTAKUSOUCOUNT,")
        sqlDetailStat.AppendLine("    @SALSE,@SALSETAX,@TOTALSALSE,@PAYMENT,@PAYMENTTAX,@TOTALPAYMENT,@ANASYORIFLG,@VOLSYORIFLG,")
        sqlDetailStat.AppendLine("    @TANKBACKORDERNO,@TANKBACKINFO,")
        sqlDetailStat.AppendLine("    @DELFLG,@INITYMD,@INITUSER,@INITTERMID,")
        sqlDetailStat.AppendLine("    @UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD )")

        Using sqlDetailCmd As New SqlCommand(sqlDetailStat.ToString, sqlCon, sqlTran)
            With sqlDetailCmd.Parameters
                .Add("ORDERNO", SqlDbType.NVarChar).Value = drOrder("ORDERNO")
                .Add("DETAILNO", SqlDbType.NVarChar).Value = drOrder("DETAILNO")
                .Add("OTDETAILNO", SqlDbType.NVarChar).Value = drOrder("OTDETAILNO")
                .Add("SHIPORDER", SqlDbType.NVarChar).Value = drOrder("SHIPORDER")
                .Add("LINEORDER", SqlDbType.NVarChar).Value = drOrder("LINEORDER")
                .Add("TANKNO", SqlDbType.NVarChar).Value = drOrder("TANKNO")
                .Add("KAMOKU", SqlDbType.NVarChar).Value = drOrder("KAMOKU")
                .Add("STACKINGORDERNO", SqlDbType.NVarChar).Value = drOrder("STACKINGORDERNO")
                Try
                    .Add("STACKINGFLG", SqlDbType.NVarChar).Value = drOrder("DETAIL_STACKINGFLG")
                Catch ex As Exception
                    If .Contains("STACKINGFLG") Then
                        .Item("STACKINGFLG").Value = drOrder("STACKINGFLG")
                    Else
                        .Add("STACKINGFLG", SqlDbType.NVarChar).Value = drOrder("STACKINGFLG")
                    End If
                End Try
                .Add("WHOLESALEFLG", SqlDbType.NVarChar).Value = drOrder("WHOLESALEFLG")
                .Add("INSPECTIONFLG", SqlDbType.NVarChar).Value = drOrder("INSPECTIONFLG")
                .Add("DETENTIONFLG", SqlDbType.NVarChar).Value = drOrder("DETENTIONFLG")
                .Add("FIRSTRETURNFLG", SqlDbType.NVarChar).Value = drOrder("FIRSTRETURNFLG")
                .Add("AFTERRETURNFLG", SqlDbType.NVarChar).Value = drOrder("AFTERRETURNFLG")
                .Add("OTTRANSPORTFLG", SqlDbType.NVarChar).Value = drOrder("OTTRANSPORTFLG")
                .Add("UPGRADEFLG", SqlDbType.NVarChar).Value = drOrder("UPGRADEFLG")
                Try
                    .Add("TESTPRODUCTFLG", SqlDbType.NVarChar).Value = drOrder("TESTPRODUCTFLG")
                Catch ex As Exception
                    If .Contains("TESTPRODUCTFLG") Then
                        .Item("TESTPRODUCTFLG").Value = "2"
                    Else
                        .Add("TESTPRODUCTFLG", SqlDbType.NVarChar).Value = "2"
                    End If
                End Try
                Try
                    .Add("ORDERINFO", SqlDbType.NVarChar).Value = drOrder("DETAIL_ORDERINFO")
                Catch ex As Exception
                    If .Contains("ORDERINFO") Then
                        .Item("ORDERINFO").Value = drOrder("ORDERINFO")
                    Else
                        .Add("ORDERINFO", SqlDbType.NVarChar).Value = drOrder("ORDERINFO")
                    End If
                End Try
                Try
                    .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = drOrder("DETAIL_SHIPPERSCODE")
                Catch ex As Exception
                    If .Contains("SHIPPERSCODE") Then
                        .Item("SHIPPERSCODE").Value = drOrder("SHIPPERSCODE")
                    Else
                        .Add("SHIPPERSCODE", SqlDbType.NVarChar).Value = drOrder("SHIPPERSCODE")
                    End If
                End Try
                Try
                    .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = drOrder("DETAIL_SHIPPERSNAME")
                Catch ex As Exception
                    If .Contains("SHIPPERSNAME") Then
                        .Item("SHIPPERSNAME").Value = drOrder("SHIPPERSNAME")
                    Else
                        .Add("SHIPPERSNAME", SqlDbType.NVarChar).Value = drOrder("SHIPPERSNAME")
                    End If
                End Try
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
                Try
                    .Add("CHANGERETSTATION", SqlDbType.NVarChar).Value = drOrder("DETAIL_CHANGERETSTATION")
                Catch ex As Exception
                    If .Contains("CHANGERETSTATION") Then
                        .Item("CHANGERETSTATION").Value = drOrder("CHANGERETSTATION")
                    Else
                        .Add("CHANGERETSTATION", SqlDbType.NVarChar).Value = drOrder("CHANGERETSTATION")
                    End If
                End Try
                Try
                    .Add("CHANGERETSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("DETAIL_CHANGERETSTATIONNAME")
                Catch ex As Exception
                    If .Contains("CHANGERETSTATIONNAME") Then
                        .Item("CHANGERETSTATIONNAME").Value = drOrder("CHANGERETSTATIONNAME")
                    Else
                        .Add("CHANGERETSTATIONNAME", SqlDbType.NVarChar).Value = drOrder("CHANGERETSTATIONNAME")
                    End If
                End Try
                .Add("LINE", SqlDbType.NVarChar).Value = drOrder("LINE")
                .Add("FILLINGPOINT", SqlDbType.NVarChar).Value = drOrder("FILLINGPOINT")
                .Add("LOADINGIRILINETRAINNO", SqlDbType.NVarChar).Value = drOrder("LOADINGIRILINETRAINNO")
                .Add("LOADINGIRILINETRAINNAME", SqlDbType.NVarChar).Value = drOrder("LOADINGIRILINETRAINNAME")
                .Add("LOADINGIRILINEORDER", SqlDbType.NVarChar).Value = drOrder("LOADINGIRILINEORDER")
                .Add("LOADINGOUTLETTRAINNO", SqlDbType.NVarChar).Value = drOrder("LOADINGOUTLETTRAINNO")
                .Add("LOADINGOUTLETTRAINNAME", SqlDbType.NVarChar).Value = drOrder("LOADINGOUTLETTRAINNAME")
                .Add("LOADINGOUTLETORDER", SqlDbType.NVarChar).Value = drOrder("LOADINGOUTLETORDER")
                Try
                    .Add("ACTUALLODDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("DETAIL_ACTUALLODDATE"), CType(DBNull.Value, Object), drOrder("DETAIL_ACTUALLODDATE"))
                Catch ex As Exception
                    If .Contains("ACTUALLODDATE") Then
                        .Item("ACTUALLODDATE").Value = If(drOrder.IsNull("ACTUALLODDATE"), CType(DBNull.Value, Object), drOrder("ACTUALLODDATE"))
                    Else
                        .Add("ACTUALLODDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALLODDATE"), CType(DBNull.Value, Object), drOrder("ACTUALLODDATE"))
                    End If
                End Try
                Try
                    .Add("ACTUALDEPDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("DETAIL_ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("DETAIL_ACTUALDEPDATE"))
                Catch ex As Exception
                    If .Contains("ACTUALDEPDATE") Then
                        .Item("ACTUALDEPDATE").Value = If(drOrder.IsNull("ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("ACTUALDEPDATE"))
                    Else
                        .Add("ACTUALDEPDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALDEPDATE"), CType(DBNull.Value, Object), drOrder("ACTUALDEPDATE"))
                    End If
                End Try
                Try
                    .Add("ACTUALARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("DETAIL_ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("DETAIL_ACTUALARRDATE"))
                Catch ex As Exception
                    If .Contains("ACTUALARRDATE") Then
                        .Item("ACTUALARRDATE").Value = If(drOrder.IsNull("ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALARRDATE"))
                    Else
                        .Add("ACTUALARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALARRDATE"))
                    End If
                End Try
                Try
                    .Add("ACTUALACCDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("DETAIL_ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("DETAIL_ACTUALACCDATE"))
                Catch ex As Exception
                    If .Contains("ACTUALACCDATE") Then
                        .Item("ACTUALACCDATE").Value = If(drOrder.IsNull("ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("ACTUALACCDATE"))
                    Else
                        .Add("ACTUALACCDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALACCDATE"), CType(DBNull.Value, Object), drOrder("ACTUALACCDATE"))
                    End If
                End Try
                Try
                    .Add("ACTUALEMPARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("DETAIL_ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("DETAIL_ACTUALEMPARRDATE"))
                Catch ex As Exception
                    If .Contains("ACTUALEMPARRDATE") Then
                        .Item("ACTUALEMPARRDATE").Value = If(drOrder.IsNull("ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALEMPARRDATE"))
                    Else
                        .Add("ACTUALEMPARRDATE", SqlDbType.NVarChar).Value = If(drOrder.IsNull("ACTUALEMPARRDATE"), CType(DBNull.Value, Object), drOrder("ACTUALEMPARRDATE"))
                    End If
                End Try
                .Add("RESERVEDNO", SqlDbType.NVarChar).Value = drOrder("RESERVEDNO")
                .Add("GYONO", SqlDbType.NVarChar).Value = drOrder("GYONO")
                .Add("OTSENDCOUNT", SqlDbType.Int).Value = drOrder("OTSENDCOUNT")
                .Add("DLRESERVEDCOUNT", SqlDbType.Int).Value = drOrder("DLRESERVEDCOUNT")
                .Add("DLTAKUSOUCOUNT", SqlDbType.Int).Value = drOrder("DLTAKUSOUCOUNT")
                Try
                    .Add("SALSE", SqlDbType.NVarChar).Value = drOrder("DETAIL_SALSE")
                Catch ex As Exception
                    If .Contains("SALSE") Then
                        .Item("SALSE").Value = drOrder("SALSE")
                    Else
                        .Add("SALSE", SqlDbType.NVarChar).Value = drOrder("SALSE")
                    End If
                End Try
                Try
                    .Add("SALSETAX", SqlDbType.NVarChar).Value = drOrder("DETAIL_SALSETAX")
                Catch ex As Exception
                    If .Contains("SALSETAX") Then
                        .Item("SALSETAX").Value = drOrder("SALSETAX")
                    Else
                        .Add("SALSETAX", SqlDbType.NVarChar).Value = drOrder("SALSETAX")
                    End If
                End Try
                Try
                    .Add("TOTALSALSE", SqlDbType.NVarChar).Value = drOrder("DETAIL_TOTALSALSE")
                Catch ex As Exception
                    If .Contains("TOTALSALSE") Then
                        .Item("TOTALSALSE").Value = drOrder("TOTALSALSE")
                    Else
                        .Add("TOTALSALSE", SqlDbType.NVarChar).Value = drOrder("TOTALSALSE")
                    End If
                End Try
                Try
                    .Add("PAYMENT", SqlDbType.NVarChar).Value = drOrder("DETAIL_PAYMENT")
                Catch ex As Exception
                    If .Contains("PAYMENT") Then
                        .Item("PAYMENT").Value = drOrder("PAYMENT")
                    Else
                        .Add("PAYMENT", SqlDbType.NVarChar).Value = drOrder("PAYMENT")
                    End If
                End Try
                Try
                    .Add("PAYMENTTAX", SqlDbType.NVarChar).Value = drOrder("DETAIL_PAYMENTTAX")
                Catch ex As Exception
                    If .Contains("PAYMENTTAX") Then
                        .Item("PAYMENTTAX").Value = drOrder("PAYMENTTAX")
                    Else
                        .Add("PAYMENTTAX", SqlDbType.NVarChar).Value = drOrder("PAYMENTTAX")
                    End If
                End Try
                Try
                    .Add("TOTALPAYMENT", SqlDbType.NVarChar).Value = drOrder("DETAIL_TOTALPAYMENT")
                Catch ex As Exception
                    If .Contains("TOTALPAYMENT") Then
                        .Item("TOTALPAYMENT").Value = drOrder("TOTALPAYMENT")
                    Else
                        .Add("TOTALPAYMENT", SqlDbType.NVarChar).Value = drOrder("TOTALPAYMENT")
                    End If
                End Try
                .Add("ANASYORIFLG", SqlDbType.NVarChar).Value = drOrder("ANASYORIFLG")
                .Add("VOLSYORIFLG", SqlDbType.NVarChar).Value = drOrder("VOLSYORIFLG")
                Try
                    .Add("TANKBACKORDERNO", SqlDbType.NVarChar).Value = drOrder("TANKBACKORDERNO")
                Catch ex As Exception
                    If .Contains("TANKBACKORDERNO") Then
                        .Item("TANKBACKORDERNO").Value = ""
                    Else
                        .Add("TANKBACKORDERNO", SqlDbType.NVarChar).Value = ""
                    End If
                End Try
                Try
                    .Add("TANKBACKINFO", SqlDbType.NVarChar).Value = drOrder("TANKBACKINFO")
                Catch ex As Exception
                    If .Contains("TANKBACKINFO") Then
                        .Item("TANKBACKINFO").Value = ""
                    Else
                        .Add("TANKBACKINFO", SqlDbType.NVarChar).Value = ""
                    End If
                End Try
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

End Class

Option Explicit On

Imports System.Data.SqlClient

''' <summary>
''' プロフビュー（PROFVIEW）取得
''' </summary>
''' <remarks>CS0029UPROFviewD置換　GB.COA0015ProfViewDから修正</remarks>
Public Class CS0029ProfViewD
    ''' <summary>
    ''' [IN]会社コードプロパティ
    ''' </summary>
    ''' <returns>[IN]CAMPCODE</returns>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' [IN]PROFIDプロパティ
    ''' </summary>
    ''' <returns>[IN]PROFID</returns>
    Public Property PROFID() As String
    ''' <summary>
    ''' [IN]MAPIDプロパティ
    ''' </summary>
    ''' <returns>[IN]MAPID</returns>
    Public Property MAPID As String
    ''' <summary>
    ''' [IN]変数プロパティ
    ''' </summary>
    ''' <returns>[IN]変数</returns>
    Public Property VARI As String
    ''' <summary>
    ''' [IN]TABプロパティ
    ''' </summary>
    ''' <returns>[IN]TABID</returns>
    Public Property TABID As String
    ''' <summary>
    ''' [OUT]プロフビュー取得結果データテーブル
    ''' </summary>
    ''' <returns>[OUT]プロフビューデータ</returns>
    Public Property TABLEDATA As System.Data.DataTable
    ''' <summary>
    ''' [OUT]最大行数プロパティ
    ''' </summary>
    ''' <returns>[OUT]最大行数</returns>
    Public Property ROWMAX As Integer
    ''' <summary>
    ''' [OUT]最大カラム数プロパティ
    ''' </summary>
    ''' <returns>[OUT]最大カラム</returns>
    Public Property COLMAX As Integer
    ''' <summary>
    ''' [OUT]ERRNoプロパティ
    ''' </summary>
    ''' <returns>[OUT]ERRNo</returns>
    Public Property ERR As String

    ''' <summary>
    ''' セッション管理
    ''' </summary>
    Private sm As CS0050SESSION

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub New()

        'プロパティ初期化
        Initialize()
        'セッション管理生成
        sm = New CS0050SESSION

    End Sub

    ''' <summary>
    ''' 初期化
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub Initialize()

        CAMPCODE = String.Empty
        PROFID = String.Empty
        MAPID = String.Empty
        VARI = String.Empty
        TABID = String.Empty
        TABLEDATA = Nothing

        ROWMAX = 0
        COLMAX = 0
        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    ''' <summary>
    ''' プロフビュー（PROFVIEW）取得
    ''' </summary>
    ''' <remarks></remarks> 
    Public Sub CS0029ProfViewD()

        Dim WW_DT As New DataTable
        Dim WW_ROWMAX As Integer = 0
        Dim WW_COLMAX As Integer = 0
        Dim WW_TBLDATA As New System.Data.DataTable
        Dim WW_TBLDATArow As DataRow

        Try
            '●In PARAMチェック
            '必須設定チェック
            If IsNothing(CAMPCODE) Then
                Throw New ArgumentNullException("CAMPCODE")
            End If
            If IsNothing(MAPID) Then
                Throw New ArgumentNullException("MAPID")
            End If

            'PARAM02: I_VARIANT
            If IsNothing(Me.VARI) Then
                Me.VARI = ""
            End If

            '●項目定義取得
            '検索SQL文
            Dim SQLStr As String =
                 "SELECT rtrim(FIELD) as FIELD , rtrim(FIELDNAMES) as FIELDNAMES , " _
                & " POSICOL , POSIROW, " _
                & " rtrim(EFFECT) as EFFECT , rtrim(LENGTH) as LENGTH , " _
                & " rtrim(WIDTH) as WIDTH , rtrim(REQUIRED) as REQUIRED , " _
                & " isnull(rtrim(ADDEVENT1),'') as ADDEVENT1 , isnull(rtrim(ADDFUNC1),'') as ADDFUNC1 , " _
                & " isnull(rtrim(ADDEVENT2),'') as ADDEVENT2 , isnull(rtrim(ADDFUNC2),'') as ADDFUNC2 , " _
                & " isnull(rtrim(ADDEVENT3),'') as ADDEVENT3 , isnull(rtrim(ADDFUNC3),'') as ADDFUNC3 , " _
                & " isnull(rtrim(ADDEVENT4),'') as ADDEVENT4 , isnull(rtrim(ADDFUNC4),'') as ADDFUNC4 , " _
                & " isnull(rtrim(ADDEVENT5),'') as ADDEVENT5 , isnull(rtrim(ADDFUNC5),'') as ADDFUNC5   " _
                & " FROM  com.S0025_PROFMVIEW  " _
                & " Where CAMPCODE = @CAMPCODE " _
                & "   and PROFID   = @PROFID " _
                & "   and MAPID    = @MAPID " _
                & "   and VARIANT  = @VARIANT " _
                & "   and TITLEKBN = 'I' " _
                & "   and HDKBN    = 'D' " _
                & "   and POSICOL  > 0 " _
                & "   and POSIROW  > 0 " _
                & "   and STYMD   <= @STYMD " _
                & "   and ENDYMD  >= @ENDYMD " _
                & "   and DELFLG  <> '" & CONST_FLAG_YES & "' "

            If Not String.IsNullOrEmpty(TABID) Then
                SQLStr += "   and TABID    = @TABID "
            End If

            SQLStr += "ORDER BY POSIROW "

            'DataBase接続文字
            Using SQLcon As New SqlConnection(sm.DBCon),
                    SQLcmd As New SqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                Dim param As SqlParameter = SQLcmd.Parameters.Add("@PROFID", SqlDbType.NVarChar)
                With SQLcmd.Parameters
                    .Add("@CAMPCODE", SqlDbType.NVarChar).Value = Me.CAMPCODE
                    .Add("@MAPID", SqlDbType.NVarChar).Value = Me.MAPID
                    .Add("@VARIANT", SqlDbType.NVarChar).Value = Me.VARI
                    .Add("@TABID", SqlDbType.NVarChar).Value = Me.TABID
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                End With
                'セッション変数のPROFIDでデータを取得し、取得できない場合はPROFID='Default'で検索
                For Each key As String In {PROFID, C_DEFAULT_DATAKEY}
                    param.Value = key
                    Using SQLda As New SqlDataAdapter(SQLcmd)
                        SQLda.Fill(WW_DT)
                    End Using

                    If WW_DT IsNot Nothing AndAlso WW_DT.Rows.Count > 0 Then
                        Exit For
                    End If
                    WW_DT = New DataTable
                Next

            End Using


                '■データ格納準備（テーブル列追加）
            WW_ROWMAX = CInt(WW_DT.Compute("Max(POSIROW)", ""))
            WW_COLMAX = CInt(WW_DT.Compute("Max(POSICOL)", ""))
            Me.ROWMAX = WW_ROWMAX
            Me.COLMAX = WW_COLMAX

            WW_TBLDATA.Clear()

            '出力DATATABLEに列(項目)追加
            With WW_TBLDATA.Columns
                For i As Integer = 1 To WW_COLMAX
                    Dim datacol As DataColumn = Nothing
                    .Add(String.Format("FIELDNM_{0}", i), GetType(String)).DefaultValue = ""    '項目見出し
                    .Add(String.Format("FIELD_{0}", i), GetType(String)).DefaultValue = ""      '項目
                    .Add(String.Format("VALUE_{0}", i), GetType(String)).DefaultValue = ""      '値
                    .Add(String.Format("VALUE_TEXT_{0}", i), GetType(String)).DefaultValue = "" '値テキスト
                    .Add(String.Format("EFFECT_{0}", i), GetType(String)).DefaultValue = ""     '表示有無
                    .Add(String.Format("LENGTH_{0}", i), GetType(Double)).DefaultValue = 0      'セルサイズ
                    .Add(String.Format("WIDTH_{0}", i), GetType(Double)).DefaultValue = 0       '横幅
                    .Add(String.Format("REQUIRED_{0}", i), GetType(String)).DefaultValue = ""   '入力必須
                    .Add(String.Format("ADDEVENT1_{0}", i), GetType(String)).DefaultValue = ""  '追加イベント１
                    .Add(String.Format("ADDFUNC1_{0}", i), GetType(String)).DefaultValue = ""   '追加ファンクション１
                    .Add(String.Format("ADDEVENT2_{0}", i), GetType(String)).DefaultValue = ""  '追加イベント２
                    .Add(String.Format("ADDFUNC2_{0}", i), GetType(String)).DefaultValue = ""   '追加ファンクション２
                    .Add(String.Format("ADDEVENT3_{0}", i), GetType(String)).DefaultValue = ""  '追加イベント３
                    .Add(String.Format("ADDFUNC3_{0}", i), GetType(String)).DefaultValue = ""   '追加ファンクション３
                    .Add(String.Format("ADDEVENT4_{0}", i), GetType(String)).DefaultValue = ""  '追加イベント４
                    .Add(String.Format("ADDFUNC4_{0}", i), GetType(String)).DefaultValue = ""   '追加ファンクション４
                    .Add(String.Format("ADDEVENT5_{0}", i), GetType(String)).DefaultValue = ""  '追加イベント５
                    .Add(String.Format("ADDFUNC5_{0}", i), GetType(String)).DefaultValue = ""   '追加ファンクション５
                Next i
            End With

            '○空明細作成
            If WW_ROWMAX > 0 Then
                For i As Integer = 0 To WW_ROWMAX - 1
                    WW_TBLDATArow = WW_TBLDATA.NewRow()
                    WW_TBLDATA.Rows.Add(WW_TBLDATArow)
                Next

                For i As Integer = 0 To WW_DT.Rows.Count - 1

                    Dim WW_DATAROW As DataRow
                    WW_DATAROW = WW_DT.Rows(i)
                    Dim WW_COL As Integer = CInt(WW_DATAROW("POSICOL"))
                    Dim WW_ROW As Integer = CInt(WW_DATAROW("POSIROW"))
                    With WW_TBLDATA.Rows(WW_ROW - 1)
                        .Item("FIELD_" & WW_COL) = WW_DATAROW("FIELD")
                        .Item("FIELDNM_" & WW_COL) = WW_DATAROW("FIELDNAMES")
                        .Item("LENGTH_" & WW_COL) = WW_DATAROW("LENGTH")
                        .Item("WIDTH_" & WW_COL) = WW_DATAROW("WIDTH")
                        .Item("REQUIRED_" & WW_COL) = WW_DATAROW("REQUIRED")
                        .Item("EFFECT_" & WW_COL) = WW_DATAROW("EFFECT")
                        .Item("ADDEVENT1_" & WW_COL) = WW_DATAROW("ADDEVENT1")
                        .Item("ADDFUNC1_" & WW_COL) = WW_DATAROW("ADDFUNC1")
                        .Item("ADDEVENT2_" & WW_COL) = WW_DATAROW("ADDEVENT2")
                        .Item("ADDFUNC2_" & WW_COL) = WW_DATAROW("ADDFUNC2")
                        .Item("ADDEVENT3_" & WW_COL) = WW_DATAROW("ADDEVENT3")
                        .Item("ADDFUNC3_" & WW_COL) = WW_DATAROW("ADDFUNC3")
                        .Item("ADDEVENT4_" & WW_COL) = WW_DATAROW("ADDEVENT4")
                        .Item("ADDFUNC4_" & WW_COL) = WW_DATAROW("ADDFUNC4")
                        .Item("ADDEVENT5_" & WW_COL) = WW_DATAROW("ADDEVENT5")
                        .Item("ADDFUNC5_" & WW_COL) = WW_DATAROW("ADDFUNC5")
                    End With

                Next i
            End If

            Me.TABLEDATA = WW_TBLDATA

            Me.ERR = C_MESSAGE_NO.NORMAL

        Catch ex As ArgumentNullException
            ' パラメータ（必須プロパティ）例外

            Me.ERR = C_MESSAGE_NO.DLL_IF_ERROR
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = Me.GetType.Name             'SUBクラス名
            CS0011LOGWRITE.INFPOSI = ex.ParamName
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

        Catch ex As Exception
            ' その他例外（基本的にDBエラー）

            Me.ERR = C_MESSAGE_NO.DB_ERROR

            Dim CS0011LOGWrite As New CS0011LOGWrite
            CS0011LOGWrite.INFSUBCLASS = Me.GetType.Name      'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:GRS0010_PROFVIEW Select"                  '
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT                              '
            CS0011LOGWrite.TEXT = ex.Message
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                     'ログ出力

        Finally

            'ワークテーブル解放
            WW_TBLDATA.Dispose()
            WW_TBLDATA = Nothing

        End Try

    End Sub

End Class
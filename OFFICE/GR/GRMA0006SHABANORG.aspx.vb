Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 車両台帳・車番部署マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class GRMA0006SHABANORG
    Inherits Page

    '○ 検索結果格納Table
    Private MA0006tbl As DataTable                          '一覧格納用テーブル
    Private MA0006INPtbl As DataTable                       'チェック用テーブル
    Private MA0006UPDtbl As DataTable                       '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45        '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 10         'マウススクロール時稼働行数
    Private Const CONST_BLANK_LINE As Integer = 50          '空行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite            'ログ出力
    Private CS0013ProfView As New CS0013ProfView            'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL              '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD          'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget          '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                '帳票出力
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    If Master.RecoverTable(MA0006tbl) Then
                        '○ 画面の情報反映
                        WF_TableChange()
                    Else
                        Exit Sub
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          'DB更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonEND"             '終了ボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_LeftBoxSubmit"         '(左ボックス)検索ボタン押下
                            WF_LeftBoxSubmit_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(MA0006tbl) Then
                MA0006tbl.Clear()
                MA0006tbl.Dispose()
                MA0006tbl = Nothing
            End If

            If Not IsNothing(MA0006INPtbl) Then
                MA0006INPtbl.Clear()
                MA0006INPtbl.Dispose()
                MA0006INPtbl = Nothing
            End If

            If Not IsNothing(MA0006UPDtbl) Then
                MA0006UPDtbl.Clear()
                MA0006UPDtbl.Dispose()
                MA0006UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = GRMA0006WRKINC.MAPID

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        leftview.activeListBox()
        rightview.resetindex()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ 右ボックスへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MA0006S Then
            'Grid情報保存先のファイル名
            Master.createXMLSaveFile()

            '運用部署
            WF_UORG.Text = work.WF_SEL_UORG.Text
            CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        End If

        '○ ヘルプボタン非表示
        Master.dispHelp = False

        '○ ファイルドロップ有無
        Master.eventDrop = True

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(MA0006tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(MA0006tbl)

        TBLview.RowFilter = "LINECNT >= 1"

        '○ 一覧表示データ編集(性能対策)
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(MA0006tbl) Then
            MA0006tbl = New DataTable
        End If

        If MA0006tbl.Columns.Count <> 0 Then
            MA0006tbl.Columns.Clear()
        End If

        MA0006tbl.Clear()

        '○ 検索SQL
        Dim SQLStr As String =
              " SELECT" _
            & "    0                                        AS LINECNT" _
            & "    , ''                                     AS OPERATION" _
            & "    , CAST(MA06.UPDTIMSTP AS bigint)         AS TIMSTP" _
            & "    , 1                                      AS 'SELECT'" _
            & "    , 0                                      AS HIDDEN" _
            & "    , ISNULL(RTRIM(MA06.CAMPCODE), '')       AS CAMPCODE" _
            & "    , ''                                     AS CAMPNAMES" _
            & "    , ISNULL(RTRIM(MA06.MANGUORG), '')       AS MANGUORG" _
            & "    , ''                                     AS UORGNAMES" _
            & "    , ISNULL(RTRIM(MA06.GSHABAN), '')        AS OLDGSHABAN" _
            & "    , ISNULL(RTRIM(MA06.GSHABAN), '')        AS GSHABAN" _
            & "    , ISNULL(RTRIM(MA06.SHARYOTYPEF), '')    AS SHARYOTYPEF" _
            & "    , ISNULL(RTRIM(MA06.TSHABANF), '')       AS TSHABANF" _
            & "    , ''                                     AS TSHARYOF" _
            & "    , ISNULL(RTRIM(MA06.TSHABANFNAMES), '')  AS TSHABANFNAMES" _
            & "    , ISNULL(RTRIM(MA04F.LICNPLTNO1), '')    +" _
            & "      ISNULL(RTRIM(MA04F.LICNPLTNO2), '')    AS LICNPLTNOF" _
            & "    , ISNULL(RTRIM(MA06.SHARYOTYPEB), '')    AS SHARYOTYPEB" _
            & "    , ISNULL(RTRIM(MA06.TSHABANB), '')       AS TSHABANB" _
            & "    , ''                                     AS TSHARYOB" _
            & "    , ISNULL(RTRIM(MA06.TSHABANBNAMES), '')  AS TSHABANBNAMES" _
            & "    , ISNULL(RTRIM(MA04B.LICNPLTNO1), '')    +" _
            & "      ISNULL(RTRIM(MA04B.LICNPLTNO2), '')    AS LICNPLTNOB" _
            & "    , ISNULL(RTRIM(MA06.SHARYOTYPEB2), '')   AS SHARYOTYPEBB" _
            & "    , ISNULL(RTRIM(MA06.TSHABANB2), '')      AS TSHABANBB" _
            & "    , ''                                     AS TSHARYOBB" _
            & "    , ISNULL(RTRIM(MA06.TSHABANB2NAMES), '') AS TSHABANBBNAMES" _
            & "    , ISNULL(RTRIM(MA04B2.LICNPLTNO1), '')   +" _
            & "      ISNULL(RTRIM(MA04B2.LICNPLTNO2), '')   AS LICNPLTNOBB" _
            & "    , ISNULL(RTRIM(MA02.MANGOILTYPE), '')    AS MANGOILTYPE" _
            & "    , ''                                     AS MANGOILTYPENAMES" _
            & "    , ISNULL(RTRIM(MA06.MANGOWNCONT), '')    AS MANGOWNCONT" _
            & "    , ''                                     AS MANGOWNCONTNAMES" _
            & "    , ISNULL(RTRIM(MA06.MANGSUPPL), '')      AS MANGSUPPL" _
            & "    , ''                                     AS MANGSUPPLNAMES" _
            & "    , ISNULL(RTRIM(MA06.YAZKSHABAN), '')     AS YAZKSHABAN" _
            & "    , ISNULL(RTRIM(MA06.KOEISHABAN), '')     AS KOEISHABAN" _
            & "    , ISNULL(RTRIM(MA06.JSRSHABAN), '')      AS JSRSHABAN" _
            & "    , ISNULL(RTRIM(MA06.SUISOKBN), '')       AS SUISOKBN" _
            & "    , ''                                     AS SUISOKBNNAMES" _
            & "    , ISNULL(RTRIM(MA06.OILKBN), '')         AS OILKBN" _
            & "    , ''                                     AS OILKBNNAMES" _
            & "    , ISNULL(RTRIM(MA06.SHARYOKBN), '')      AS SHARYOKBN" _
            & "    , ''                                     AS SHARYOKBNNAMES" _
            & "    , ISNULL(RTRIM(MA06.SEQ), '')            AS SEQ" _
            & "    , ISNULL(RTRIM(MA06.DELFLG), '')         AS DELFLG" _
            & "    , ISNULL(RTRIM(MA06.SHARYOINFO1), '')    AS SHARYOINFO1" _
            & "    , ISNULL(RTRIM(MA06.SHARYOINFO2), '')    AS SHARYOINFO2" _
            & "    , ISNULL(RTRIM(MA06.SHARYOINFO3), '')    AS SHARYOINFO3" _
            & "    , ISNULL(RTRIM(MA06.SHARYOINFO4), '')    AS SHARYOINFO4" _
            & "    , ISNULL(RTRIM(MA06.SHARYOINFO5), '')    AS SHARYOINFO5" _
            & "    , ISNULL(RTRIM(MA06.SHARYOINFO6), '')    AS SHARYOINFO6" _
            & " FROM" _
            & "    MA006_SHABANORG MA06" _
            & "    INNER JOIN S0006_ROLE S006A" _
            & "        ON  S006A.CAMPCODE     = @P1" _
            & "        AND S006A.OBJECT       = @P3" _
            & "        AND S006A.ROLE         = @P4" _
            & "        AND S006A.CODE         = MA06.MANGUORG" _
            & "        AND S006A.STYMD       <= @P7" _
            & "        AND S006A.ENDYMD      >= @P7" _
            & "        AND S006A.DELFLG      <> @P9" _
            & "    INNER JOIN S0012_SRVAUTHOR S012" _
            & "        ON  S012.TERMID        = @P5" _
            & "        AND S012.CAMPCODE      = @P1" _
            & "        AND S012.OBJECT        = @P6" _
            & "        AND S012.STYMD        <= @P7" _
            & "        AND S012.ENDYMD       >= @P7" _
            & "        AND S012.DELFLG       <> @P9" _
            & "    INNER JOIN S0006_ROLE S006B" _
            & "        ON  S006B.CAMPCODE     = S012.CAMPCODE" _
            & "        AND S006B.OBJECT       = S012.OBJECT" _
            & "        AND S006B.ROLE         = S012.ROLE" _
            & "        AND S006B.CODE         = MA06.MANGUORG" _
            & "        AND S006B.STYMD       <= @P7" _
            & "        AND S006B.ENDYMD      >= @P7" _
            & "        AND S006B.DELFLG      <> @P9" _
            & "    LEFT JOIN MA002_SHARYOA MA02" _
            & "        ON  MA02.CAMPCODE      = MA06.CAMPCODE" _
            & "        AND MA02.SHARYOTYPE    = MA06.SHARYOTYPEF" _
            & "        AND MA02.TSHABAN       = MA06.TSHABANF" _
            & "        AND MA02.STYMD        <= @P7" _
            & "        AND MA02.ENDYMD       >= @P7" _
            & "        AND MA02.DELFLG       <> @P9" _
            & "    LEFT JOIN MA004_SHARYOC MA04F" _
            & "        ON  MA04F.CAMPCODE     = MA06.CAMPCODE" _
            & "        AND MA04F.SHARYOTYPE   = MA06.SHARYOTYPEF" _
            & "        AND MA04F.TSHABAN      = MA06.TSHABANF" _
            & "        AND MA04F.STYMD       <= @P7" _
            & "        AND MA04F.ENDYMD      >= (" _
            & "            SELECT" _
            & "                MAX(ENDYMD)" _
            & "            FROM" _
            & "                MA004_SHARYOC" _
            & "            WHERE" _
            & "                CAMPCODE       = MA06.CAMPCODE" _
            & "                AND SHARYOTYPE = MA06.SHARYOTYPEF" _
            & "                AND TSHABAN    = MA06.TSHABANF" _
            & "                AND STYMD     <= @P7" _
            & "                AND ENDYMD    >= @P8" _
            & "                AND DELFLG    <> @P9)" _
            & "        AND MA04F.DELFLG      <> @P9" _
            & "    LEFT JOIN MA004_SHARYOC MA04B" _
            & "        ON  MA04B.CAMPCODE     = MA06.CAMPCODE" _
            & "        AND MA04B.SHARYOTYPE   = MA06.SHARYOTYPEB" _
            & "        AND MA04B.TSHABAN      = MA06.TSHABANB" _
            & "        AND MA04B.STYMD       <= @P7" _
            & "        AND MA04B.ENDYMD      >= (" _
            & "            SELECT" _
            & "                MAX(ENDYMD)" _
            & "            FROM" _
            & "                MA004_SHARYOC" _
            & "            WHERE" _
            & "                CAMPCODE       = MA06.CAMPCODE" _
            & "                AND SHARYOTYPE = MA06.SHARYOTYPEB" _
            & "                AND TSHABAN    = MA06.TSHABANB" _
            & "                AND STYMD     <= @P7" _
            & "                AND ENDYMD    >= @P8" _
            & "                AND DELFLG    <> @P9)" _
            & "        AND MA04B.DELFLG      <> @P9" _
            & "    LEFT JOIN MA004_SHARYOC MA04B2" _
            & "        ON  MA04B2.CAMPCODE    = MA06.CAMPCODE" _
            & "        AND MA04B2.SHARYOTYPE  = MA06.SHARYOTYPEB2" _
            & "        AND MA04B2.TSHABAN     = MA06.TSHABANB2" _
            & "        AND MA04B2.STYMD      <= @P7" _
            & "        AND MA04B2.ENDYMD     >= (" _
            & "            SELECT" _
            & "                MAX(ENDYMD)" _
            & "            FROM" _
            & "                MA004_SHARYOC" _
            & "            WHERE" _
            & "                CAMPCODE       = MA06.CAMPCODE" _
            & "                AND SHARYOTYPE = MA06.SHARYOTYPEB2" _
            & "                AND TSHABAN    = MA06.TSHABANB2" _
            & "                AND STYMD     <= @P7" _
            & "                AND ENDYMD    >= @P8" _
            & "                AND DELFLG    <> @P9)" _
            & "        AND MA04B2.DELFLG     <> @P9" _
            & " WHERE" _
            & "    MA06.CAMPCODE     = @P1" _
            & "    AND MA06.MANGUORG = @P2" _
            & "    AND MA06.DELFLG  <> @P9" _
            & " ORDER BY" _
            & "    MA06.SEQ" _
            & "    , MA06.GSHABAN"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '運用部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        'ロール
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 30)        '端末ＩＤ
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)        'オブジェクト
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.Date)                '現在日付
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.Date)                '現在日付-1月初日
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE.Text
                PARA2.Value = work.WF_SEL_UORG.Text
                PARA3.Value = C_ROLE_VARIANT.USER_ORG
                PARA4.Value = Master.ROLE_ORG
                PARA5.Value = CS0050SESSION.APSV_ID
                PARA6.Value = C_ROLE_VARIANT.SERV_ORG
                PARA7.Value = Date.Now
                PARA8.Value = Convert.ToDateTime(Date.Now.AddMonths(-1).ToString("yyyy/MM") & "/01")
                PARA9.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        MA0006tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    MA0006tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each MA0006row As DataRow In MA0006tbl.Rows
                    i += 1
                    MA0006row("LINECNT") = i        'LINECNT

                    '統一車番結合
                    MA0006row("TSHARYOF") = MA0006row("SHARYOTYPEF") & MA0006row("TSHABANF")            '前
                    MA0006row("TSHARYOB") = MA0006row("SHARYOTYPEB") & MA0006row("TSHABANB")            '後
                    MA0006row("TSHARYOBB") = MA0006row("SHARYOTYPEBB") & MA0006row("TSHABANBB")         '後2

                    '登録車番
                    If MA0006row("TSHABANFNAMES") = "" Then
                        MA0006row("TSHABANFNAMES") = MA0006row("LICNPLTNOF")
                    End If
                    If MA0006row("TSHABANBNAMES") = "" Then
                        MA0006row("TSHABANBNAMES") = MA0006row("LICNPLTNOB")
                    End If
                    If MA0006row("TSHABANBBNAMES") = "" Then
                        MA0006row("TSHABANBBNAMES") = MA0006row("LICNPLTNOBB")
                    End If

                    '水素区分
                    If MA0006row("SUISOKBN") = "0" Then
                        MA0006row("SUISOKBN") = ""
                    End If

                    '名称取得
                    CODENAME_get("CAMPCODE", MA0006row("CAMPCODE"), MA0006row("CAMPNAMES"), WW_DUMMY)                       '会社コード
                    CODENAME_get("UORG", MA0006row("MANGUORG"), MA0006row("UORGNAMES"), WW_DUMMY)                           '運用部署
                    CODENAME_get("MANGOILTYPE", MA0006row("MANGOILTYPE"), MA0006row("MANGOILTYPENAMES"), WW_DUMMY)          '油種
                    CODENAME_get("MANGOWNCONT", MA0006row("MANGOWNCONT"), MA0006row("MANGOWNCONTNAMES"), WW_DUMMY)          '契約区分
                    CODENAME_get("MANGSUPPL", MA0006row("MANGSUPPL"), MA0006row("MANGSUPPLNAMES"), WW_DUMMY)                '庸車会社
                    CODENAME_get("SUISOKBN", MA0006row("SUISOKBN"), MA0006row("SUISOKBNNAMES"), WW_DUMMY)                   '水素フラグ
                    CODENAME_get("OILKBN", MA0006row("OILKBN"), MA0006row("OILKBNNAMES"), WW_DUMMY)                         '油種区分（勤怠用）
                    CODENAME_get("SHARYOKBN", MA0006row("SHARYOKBN"), MA0006row("SHARYOKBNNAMES"), WW_DUMMY)                '車両区分（勤怠用）
                Next

                '○ 空行作成
                For j As Integer = 0 To CONST_BLANK_LINE - 1
                    Dim MA0006row As DataRow = MA0006tbl.NewRow
                    i += 1

                    For Each MA0006col As DataColumn In MA0006tbl.Columns
                        Select Case MA0006col.ColumnName
                            Case "LINECNT"
                                MA0006row.Item(MA0006col) = i
                            Case "SELECT"
                                MA0006row.Item(MA0006col) = 1
                            Case "HIDDEN", "TIMSTP"
                                MA0006row.Item(MA0006col) = 0
                            Case Else
                                MA0006row.Item(MA0006col) = ""
                        End Select
                    Next

                    'キー項目設定
                    MA0006row("CAMPCODE") = work.WF_SEL_CAMPCODE.Text
                    MA0006row("MANGUORG") = work.WF_SEL_UORG.Text

                    CODENAME_get("CAMPCODE", MA0006row("CAMPCODE"), MA0006row("CAMPNAMES"), WW_DUMMY)       '会社コード
                    CODENAME_get("UORG", MA0006row("MANGUORG"), MA0006row("UORGNAMES"), WW_DUMMY)           '運用部署

                    MA0006tbl.Rows.Add(MA0006row)
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA006_SHABANORG SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MA006_SHABANORG Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each MA0006row As DataRow In MA0006tbl.Rows
            If MA0006row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                MA0006row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(MA0006tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = C_DEFAULT_DATAKEY
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "Onchange"
        CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub


    ''' <summary>
    ''' DB更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ DetailBoxをtblへ退避
        DetailBoxToMA0006tbl()

        '○ 項目チェック
        TableCheck(WW_ERR_SW)

        If isNormal(WW_ERR_SW) Then
            'DataBase接続文字
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '車番部署マスタ更新
                UpdateShabanORGMaster(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(MA0006tbl)

        '○ メッセージ表示
        If Not isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToMA0006tbl()

        For i As Integer = 0 To MA0006tbl.Rows.Count - 1

            '業務車番
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "GSHABAN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("GSHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "GSHABAN" & (i + 1))) Then
                MA0006tbl.Rows(i)("GSHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "GSHABAN" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("GSHABAN"))

            '統一車番(前)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHARYOF" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("TSHARYOF") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOF" & (i + 1))) Then
                MA0006tbl.Rows(i)("TSHARYOF") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOF" & (i + 1)))
                MA0006tbl.Rows(i)("SHARYOTYPEF") = Mid(MA0006tbl.Rows(i)("TSHARYOF"), 1, 1)
                MA0006tbl.Rows(i)("TSHABANF") = Mid(MA0006tbl.Rows(i)("TSHARYOF"), 2, 19)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHARYOF"))
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOTYPEF"))
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHABANF"))

            '登録車番(前)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHABANFNAMES" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("TSHABANFNAMES") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANFNAMES" & (i + 1))) Then
                MA0006tbl.Rows(i)("TSHABANFNAMES") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANFNAMES" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHABANFNAMES"))

            '統一車番(後)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHARYOB" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("TSHARYOB") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOB" & (i + 1))) Then
                MA0006tbl.Rows(i)("TSHARYOB") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOB" & (i + 1)))
                MA0006tbl.Rows(i)("SHARYOTYPEB") = Mid(MA0006tbl.Rows(i)("TSHARYOB"), 1, 1)
                MA0006tbl.Rows(i)("TSHABANB") = Mid(MA0006tbl.Rows(i)("TSHARYOB"), 2, 19)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHARYOB"))
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOTYPEB"))
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHABANB"))

            '登録車番(後)
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHABANBNAMES" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("TSHABANBNAMES") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBNAMES" & (i + 1))) Then
                MA0006tbl.Rows(i)("TSHABANBNAMES") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBNAMES" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHABANBNAMES"))

            '統一車番(後)2
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHARYOBB" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("TSHARYOBB") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOBB" & (i + 1))) Then
                MA0006tbl.Rows(i)("TSHARYOBB") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOBB" & (i + 1)))
                MA0006tbl.Rows(i)("SHARYOTYPEBB") = Mid(MA0006tbl.Rows(i)("TSHARYOBB"), 1, 1)
                MA0006tbl.Rows(i)("TSHABANBB") = Mid(MA0006tbl.Rows(i)("TSHARYOBB"), 2, 19)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHARYOBB"))
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOTYPEBB"))
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHABANBB"))

            '登録車番(後)2
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHABANBBNAMES" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("TSHABANBBNAMES") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBBNAMES" & (i + 1))) Then
                MA0006tbl.Rows(i)("TSHABANBBNAMES") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBBNAMES" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("TSHABANBBNAMES"))

            '契約区分
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "MANGOWNCONT" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("MANGOWNCONT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGOWNCONT" & (i + 1))) Then
                MA0006tbl.Rows(i)("MANGOWNCONT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGOWNCONT" & (i + 1)))
                CODENAME_get("MANGOWNCONT", MA0006tbl(i)("MANGOWNCONT"), MA0006tbl(i)("MANGOWNCONTNAMES"), WW_DUMMY)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("MANGOWNCONT"))

            '傭車
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "MANGSUPPL" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("MANGSUPPL") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGSUPPL" & (i + 1))) Then
                MA0006tbl.Rows(i)("MANGSUPPL") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGSUPPL" & (i + 1)))
                CODENAME_get("MANGSUPPL", MA0006tbl(i)("MANGSUPPL"), MA0006tbl(i)("MANGSUPPLNAMES"), WW_DUMMY)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("MANGSUPPL"))

            '矢崎車端用車番コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YAZKSHABAN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("YAZKSHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YAZKSHABAN" & (i + 1))) Then
                MA0006tbl.Rows(i)("YAZKSHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YAZKSHABAN" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("YAZKSHABAN"))

            '光英車端用車番コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "KOEISHABAN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("KOEISHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KOEISHABAN" & (i + 1))) Then
                MA0006tbl.Rows(i)("KOEISHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KOEISHABAN" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("KOEISHABAN"))

            'JSR車番コード
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRSHABAN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("JSRSHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSHABAN" & (i + 1))) Then
                MA0006tbl.Rows(i)("JSRSHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSHABAN" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("JSRSHABAN"))

            '水素車
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SUISOKBN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SUISOKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SUISOKBN" & (i + 1))) Then
                MA0006tbl.Rows(i)("SUISOKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SUISOKBN" & (i + 1)))
                CODENAME_get("SUISOKBN", MA0006tbl(i)("SUISOKBN"), MA0006tbl(i)("SUISOKBNNAMES"), WW_DUMMY)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SUISOKBN"))

            '勤怠用油種区分
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "OILKBN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("OILKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "OILKBN" & (i + 1))) Then
                MA0006tbl.Rows(i)("OILKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "OILKBN" & (i + 1)))
                CODENAME_get("OILKBN", MA0006tbl(i)("OILKBN"), MA0006tbl(i)("OILKBNNAMES"), WW_DUMMY)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("OILKBN"))

            '勤怠用車両区分
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOKBN" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOKBN" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOKBN" & (i + 1)))
                CODENAME_get("SHARYOKBN", MA0006tbl(i)("SHARYOKBN"), MA0006tbl(i)("SHARYOKBNNAMES"), WW_DUMMY)
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOKBN"))

            'SEQ
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1))) Then
                MA0006tbl.Rows(i)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SEQ"))

            '削除
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("DELFLG") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1))) Then
                MA0006tbl.Rows(i)("DELFLG") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("DELFLG"))

            '車両情報１
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO1" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOINFO1") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO1" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOINFO1") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO1" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOINFO1"))

            '車両情報２
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO2" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOINFO2") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO2" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOINFO2") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO2" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOINFO2"))

            '車両情報３
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO3" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOINFO3") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO3" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOINFO3") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO3" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOINFO3"))

            '車両情報４
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO4" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOINFO4") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO4" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOINFO4") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO4" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOINFO4"))

            '車両情報５
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO5" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOINFO5") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO5" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOINFO5") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO5" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOINFO5"))

            '車両情報６
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO6" & (i + 1))) AndAlso
                MA0006tbl.Rows(i)("SHARYOINFO6") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO6" & (i + 1))) Then
                MA0006tbl.Rows(i)("SHARYOINFO6") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO6" & (i + 1)))
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            End If
            Master.eraseCharToIgnore(MA0006tbl.Rows(i)("SHARYOINFO6"))
        Next

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub TableCheck(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザ画面更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each MA0006row As DataRow In MA0006tbl.Rows

            '変更していない明細は飛ばす
            If MA0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then Continue For

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MA0006row("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MA0006row("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "UORG", MA0006row("MANGUORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MA0006row("MANGUORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '権限チェック
                CS0025AUTHORget.USERID = CS0050SESSION.USERID
                CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_ORG
                CS0025AUTHORget.CODE = MA0006row("MANGUORG")
                CS0025AUTHORget.STYMD = Date.Now
                CS0025AUTHORget.ENDYMD = Date.Now
                CS0025AUTHORget.CS0025AUTHORget()
                If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
                Else
                    WW_CheckMES1 = "・更新できないレコード(ユーザ部署更新権限なし)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '業務車番
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "GSHABAN", MA0006row("GSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(業務車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If MA0006row("DELFLG") <> C_DELETE_FLG.DELETE Then

                '統一車番(前)(上) 
                WW_TEXT = MA0006row("SHARYOTYPEF")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPEF", MA0006row("SHARYOTYPEF"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006row("SHARYOTYPEF") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SHARYOTYPE", MA0006row("SHARYOTYPEF"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) OrElse WW_DUMMY <> "前" Then
                            WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                            WW_CheckMES2 = "車両タイプエラーです。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(前)(下)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANF", MA0006row("TSHABANF"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(前)存在チェック
                If MA0006row("TSHARYOF") <> "" Then
                    CODENAME_get("TSHARYO", MA0006row("TSHARYOF"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If

                '登録車番(前)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANFNAMES", MA0006row("TSHABANFNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(登録車番(前)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(前)、登録車番(前)必須チェック
                'If MA0006row("TSHARYOF") = "" AndAlso MA0006row("TSHABANFNAMES") = "" Then
                'WW_CheckMES1 = "・更新できないレコード(統一車番(前)、登録車番(前)エラー)です。"
                'WW_CheckMES2 = "統一(前)か登録(前)のいずれかは必須です。"
                'WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                'WW_LINE_ERR = "ERR"
                'O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If

                '統一車番(後)(上)
                WW_TEXT = MA0006row("SHARYOTYPEB")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPEB", MA0006row("SHARYOTYPEB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006row("SHARYOTYPEB") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SHARYOTYPE", MA0006row("SHARYOTYPEB"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) OrElse WW_DUMMY <> "後" Then
                            WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                            WW_CheckMES2 = "車両タイプエラーです。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)(下)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANB", MA0006row("TSHABANB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)存在チェック
                If MA0006row("TSHARYOB") <> "" Then
                    CODENAME_get("TSHARYO", MA0006row("TSHARYOB"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If

                '登録車番(後)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANBNAMES", MA0006row("TSHABANBNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(登録車番(後)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(前)、登録車番(前),統一車番(後)、登録車番(後)必須チェック
                If MA0006row("TSHARYOF") = "" AndAlso MA0006row("TSHABANFNAMES") = "" AndAlso
                    MA0006row("TSHARYOB") = "" AndAlso MA0006row("TSHABANBNAMES") = "" Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(前)、登録車番(前)、統一車番(後)、登録車番(後)エラー)です。"
                    WW_CheckMES2 = "統一(前)か登録(前)または統一(後)か登録(後)のいずれかは必須です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)2(上)
                WW_TEXT = MA0006row("SHARYOTYPEBB")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPEB2", MA0006row("SHARYOTYPEBB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006row("SHARYOTYPEBB") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SHARYOTYPE", MA0006row("SHARYOTYPEBB"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) OrElse WW_DUMMY <> "後" Then
                            WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                            WW_CheckMES2 = "車両タイプエラーです。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)2(下)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANB2", MA0006row("TSHABANBB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)2存在チェック
                If MA0006row("TSHARYOBB") <> "" Then
                    CODENAME_get("TSHARYO", MA0006row("TSHARYOBB"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If

                '登録車番(後)2
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANB2NAMES", MA0006row("TSHABANBBNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(登録車番(後)2エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '契約区分
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MANGOWNCONT", MA0006row("MANGOWNCONT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("MANGOWNCONT", MA0006row("MANGOWNCONT"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(契約区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(契約区分エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '傭車
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MANGSUPPL", MA0006row("MANGSUPPL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("MANGSUPPL", MA0006row("MANGSUPPL"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(傭車エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(傭車エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '矢崎車端用車番コード
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "YAZKSHABAN", MA0006row("YAZKSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(矢崎車端用車番コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '光英車端用車番コード
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "KOEISHABAN", MA0006row("KOEISHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(光英車端用車番コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                'JSR車番コード
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "JSRSHABAN", MA0006row("JSRSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(JSR車番コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '水素車
                WW_TEXT = MA0006row("SUISOKBN")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SUISOKBN", MA0006row("SUISOKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" OrElse WW_TEXT = "0" Then
                        MA0006row("SUISOKBN") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SUISOKBN", MA0006row("SUISOKBN"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・更新できないレコード(水素車エラー)です。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(水素車エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '勤怠用油種区分
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "OILPAYKBN", MA0006row("OILKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("OILKBN", MA0006row("OILKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(勤怠用油種区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(勤怠用油種区分エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '勤怠用車両区分
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOKBN", MA0006row("SHARYOKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("SHARYOKBN", MA0006row("SHARYOKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(勤怠用車両区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(勤怠用車両区分エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                'SEQ
                WW_TEXT = MA0006row("SEQ")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SEQ", MA0006row("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" AndAlso MA0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                        MA0006row("SEQ") = ""
                    Else
                        Try
                            MA0006row("SEQ") = Format(CInt(MA0006row("SEQ")), "#0")
                        Catch ex As Exception
                            MA0006row("SEQ") = "0"
                        End Try
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '削除
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MA0006row("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MA0006row("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報１
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO1", MA0006row("SHARYOINFO1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報１エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報２
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO2", MA0006row("SHARYOINFO2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報２エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報３
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO3", MA0006row("SHARYOINFO3"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報３エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報４
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO4", MA0006row("SHARYOINFO4"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報４エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報５
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO5", MA0006row("SHARYOINFO5"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報５エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報６
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO6", MA0006row("SHARYOINFO6"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報６エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006row)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR <> "" Then
                MA0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '重複チェック
        For i As Integer = 0 To MA0006tbl.Rows.Count - 1

            If MA0006tbl.Rows(i)("GSHABAN") = "" OrElse
                MA0006tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                Continue For
            End If

            WW_LINE_ERR = ""

            For j As Integer = i + 1 To MA0006tbl.Rows.Count - 1

                If MA0006tbl.Rows(j)("GSHABAN") = "" OrElse
                    MA0006tbl.Rows(j)("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                If MA0006tbl.Rows(i)("GSHABAN") = MA0006tbl.Rows(j)("GSHABAN") Then
                    WW_CheckMES1 = "・更新できないレコード(業務車番重複エラー)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006tbl.Rows(i))
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit For
                End If
            Next

            If WW_LINE_ERR <> "" Then
                MA0006tbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' 車番部署マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateShabanORGMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        '更新前業務車番削除用SQL
        Dim SQLDel As String =
              " UPDATE MA006_SHABANORG" _
            & " SET" _
            & "    DELFLG       = @DP4" _
            & "    , UPDYMD     = @DP5" _
            & "    , UPDUSER    = @DP6" _
            & "    , UPDTERMID  = @DP7" _
            & "    , RECEIVEYMD = @DP8" _
            & " WHERE" _
            & "    CAMPCODE     = @DP1" _
            & "    AND MANGUORG = @DP2" _
            & "    AND GSHABAN  = @DP3"

        '車番部署マスタ更新用SQL
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & " SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & " SELECT" _
            & "    CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & " FROM" _
            & "    MA006_SHABANORG" _
            & " WHERE" _
            & "    CAMPCODE     = @P1" _
            & "    AND MANGUORG = @P2" _
            & "    AND GSHABAN  = @P3" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE MA006_SHABANORG" _
            & "    SET" _
            & "        GSHABAN          = @P3     , MANGSUPPL     = @P4" _
            & "        , SHARYOTYPEF    = @P5     , TSHABANF      = @P6" _
            & "        , TSHABANFNAMES  = @P7     , SHARYOTYPEB   = @P8" _
            & "        , TSHABANB       = @P9     , TSHABANBNAMES = @P10" _
            & "        , SHARYOTYPEB2   = @P11    , TSHABANB2     = @P12" _
            & "        , TSHABANB2NAMES = @P13    , YAZKSHABAN    = @P14" _
            & "        , KOEISHABAN     = @P15    , SHARYOINFO1   = @P16" _
            & "        , SHARYOINFO2    = @P17    , SHARYOINFO3   = @P18" _
            & "        , SHARYOINFO4    = @P19    , SHARYOINFO5   = @P20" _
            & "        , SHARYOINFO6    = @P21    , SEQ           = @P22" _
            & "        , SUISOKBN       = @P23    , OILKBN        = @P24" _
            & "        , SHARYOKBN      = @P25    , MANGOWNCONT   = @P26" _
            & "        , JSRSHABAN      = @P27    , DELFLG        = @P28" _
            & "        , UPDYMD         = @P30    , UPDUSER       = @P31" _
            & "        , UPDTERMID      = @P32    , RECEIVEYMD    = @P33" _
            & "    WHERE" _
            & "        CAMPCODE     = @P1" _
            & "        AND MANGUORG = @P2" _
            & "        AND GSHABAN  = @P3" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO MA006_SHABANORG" _
            & "        (CAMPCODE           , MANGUORG" _
            & "        , GSHABAN           , MANGSUPPL" _
            & "        , SHARYOTYPEF       , TSHABANF" _
            & "        , TSHABANFNAMES     , SHARYOTYPEB" _
            & "        , TSHABANB          , TSHABANBNAMES" _
            & "        , SHARYOTYPEB2      , TSHABANB2" _
            & "        , TSHABANB2NAMES    , YAZKSHABAN" _
            & "        , KOEISHABAN        , SHARYOINFO1" _
            & "        , SHARYOINFO2       , SHARYOINFO3" _
            & "        , SHARYOINFO4       , SHARYOINFO5" _
            & "        , SHARYOINFO6       , SEQ" _
            & "        , SUISOKBN          , OILKBN" _
            & "        , SHARYOKBN         , MANGOWNCONT" _
            & "        , JSRSHABAN         , DELFLG" _
            & "        , INITYMD           , UPDYMD" _
            & "        , UPDUSER           , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P1      , @P2" _
            & "        , @P3     , @P4" _
            & "        , @P5     , @P6" _
            & "        , @P7     , @P8" _
            & "        , @P9     , @P10" _
            & "        , @P11    , @P12" _
            & "        , @P13    , @P14" _
            & "        , @P15    , @P16" _
            & "        , @P17    , @P18" _
            & "        , @P19    , @P20" _
            & "        , @P21    , @P22" _
            & "        , @P23    , @P24" _
            & "        , @P25    , @P26" _
            & "        , @P27    , @P28" _
            & "        , @P29    , @P30" _
            & "        , @P31    , @P32" _
            & "        , @P33) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    CAMPCODE" _
            & "    , MANGUORG" _
            & "    , GSHABAN" _
            & "    , MANGSUPPL" _
            & "    , SHARYOTYPEF" _
            & "    , TSHABANF" _
            & "    , TSHABANFNAMES" _
            & "    , SHARYOTYPEB" _
            & "    , TSHABANB" _
            & "    , TSHABANBNAMES" _
            & "    , SHARYOTYPEB2" _
            & "    , TSHABANB2" _
            & "    , TSHABANB2NAMES" _
            & "    , YAZKSHABAN" _
            & "    , KOEISHABAN" _
            & "    , SHARYOINFO1" _
            & "    , SHARYOINFO2" _
            & "    , SHARYOINFO3" _
            & "    , SHARYOINFO4" _
            & "    , SHARYOINFO5" _
            & "    , SHARYOINFO6" _
            & "    , SEQ" _
            & "    , SUISOKBN" _
            & "    , OILKBN" _
            & "    , SHARYOKBN" _
            & "    , MANGOWNCONT" _
            & "    , JSRSHABAN" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    MA006_SHABANORG" _
            & " WHERE" _
            & "    CAMPCODE     = @P1" _
            & "    AND MANGUORG = @P2" _
            & "    AND GSHABAN IN (@P3, @P4)"

        Try
            Using SQLcmdDel As New SqlCommand(SQLDel, SQLcon),
                SQLcmd As New SqlCommand(SQLStr, SQLcon),
                SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)

                Dim DPARA1 As SqlParameter = SQLcmdDel.Parameters.Add("@DP1", SqlDbType.NVarChar, 20)        '会社コード
                Dim DPARA2 As SqlParameter = SQLcmdDel.Parameters.Add("@DP2", SqlDbType.NVarChar, 20)        '運用部署
                Dim DPARA3 As SqlParameter = SQLcmdDel.Parameters.Add("@DP3", SqlDbType.NVarChar, 20)        '旧業務車番
                Dim DPARA4 As SqlParameter = SQLcmdDel.Parameters.Add("@DP4", SqlDbType.NVarChar, 1)         '削除フラグ
                Dim DPARA5 As SqlParameter = SQLcmdDel.Parameters.Add("@DP5", SqlDbType.DateTime)            '更新年月日
                Dim DPARA6 As SqlParameter = SQLcmdDel.Parameters.Add("@DP6", SqlDbType.NVarChar, 20)        '更新ユーザーID
                Dim DPARA7 As SqlParameter = SQLcmdDel.Parameters.Add("@DP7", SqlDbType.NVarChar, 30)        '更新端末
                Dim DPARA8 As SqlParameter = SQLcmdDel.Parameters.Add("@DP8", SqlDbType.DateTime)            '集信日時

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20)            '運用部署
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 20)            '業務車番
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 20)            '庸車会社
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)             '統一車番(前)(上)
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)            '統一車番(前)(下)
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20)            '登録車番(前)
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 1)             '統一車番(後)(上)
                Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)            '統一車番(後)(下)
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '登録車番(後)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 1)           '統一車番(後)(上)2
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          '統一車番(後)(下)2
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)          '登録車番(後)2
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          '矢崎車番
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)          '光英車番
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 50)          '車両情報１
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 50)          '車両情報２
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 50)          '車両情報３
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 50)          '車両情報４
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 50)          '車両情報５
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 50)          '車両情報６
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Int)                   '表示順番
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)           '水素フラグ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 2)           '油種区分（勤怠用）
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 2)           '車両区分（勤怠用）
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 2)           '契約区分
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 20)          'JSR車番コード
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.DateTime)              '登録年月日
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.DateTime)              '更新年月日
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 30)          '更新端末
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.DateTime)              '集信日時

                Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 20)        '運用部署
                Dim JPARA3 As SqlParameter = SQLcmdJnl.Parameters.Add("@P3", SqlDbType.NVarChar, 20)        '旧業務車番
                Dim JPARA4 As SqlParameter = SQLcmdJnl.Parameters.Add("@P4", SqlDbType.NVarChar, 20)        '業務車番

                For Each MA0006row As DataRow In MA0006tbl.Rows
                    If Trim(MA0006row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        '更新前の業務車番を一旦削除
                        DPARA1.Value = MA0006row("CAMPCODE")
                        DPARA2.Value = MA0006row("MANGUORG")
                        DPARA3.Value = MA0006row("OLDGSHABAN")
                        DPARA4.Value = C_DELETE_FLG.DELETE
                        DPARA5.Value = WW_DATENOW
                        DPARA6.Value = Master.USERID
                        DPARA7.Value = Master.USERTERMID
                        DPARA8.Value = C_DEFAULT_YMD

                        SQLcmdDel.CommandTimeout = 300
                        SQLcmdDel.ExecuteNonQuery()

                        '車番部署マスタ更新
                        PARA1.Value = MA0006row("CAMPCODE")
                        PARA2.Value = MA0006row("MANGUORG")
                        PARA3.Value = MA0006row("GSHABAN")
                        PARA4.Value = MA0006row("MANGSUPPL")
                        PARA5.Value = MA0006row("SHARYOTYPEF")
                        PARA6.Value = MA0006row("TSHABANF")
                        PARA7.Value = MA0006row("TSHABANFNAMES")
                        PARA8.Value = MA0006row("SHARYOTYPEB")
                        PARA9.Value = MA0006row("TSHABANB")
                        PARA10.Value = MA0006row("TSHABANBNAMES")
                        PARA11.Value = MA0006row("SHARYOTYPEBB")
                        PARA12.Value = MA0006row("TSHABANBB")
                        PARA13.Value = MA0006row("TSHABANBBNAMES")
                        PARA14.Value = MA0006row("YAZKSHABAN")
                        PARA15.Value = MA0006row("KOEISHABAN")
                        PARA16.Value = MA0006row("SHARYOINFO1")
                        PARA17.Value = MA0006row("SHARYOINFO2")
                        PARA18.Value = MA0006row("SHARYOINFO3")
                        PARA19.Value = MA0006row("SHARYOINFO4")
                        PARA20.Value = MA0006row("SHARYOINFO5")
                        PARA21.Value = MA0006row("SHARYOINFO6")
                        PARA22.Value = MA0006row("SEQ")
                        PARA23.Value = If(MA0006row("SUISOKBN") = "", "0", MA0006row("SUISOKBN"))
                        PARA24.Value = MA0006row("OILKBN")
                        PARA25.Value = MA0006row("SHARYOKBN")
                        PARA26.Value = MA0006row("MANGOWNCONT")
                        PARA27.Value = MA0006row("JSRSHABAN")
                        PARA28.Value = MA0006row("DELFLG")
                        PARA29.Value = WW_DATENOW
                        PARA30.Value = WW_DATENOW
                        PARA31.Value = Master.USERID
                        PARA32.Value = Master.USERTERMID
                        PARA33.Value = C_DEFAULT_YMD

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        MA0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA1.Value = MA0006row("CAMPCODE")
                        JPARA2.Value = MA0006row("MANGUORG")
                        JPARA3.Value = MA0006row("OLDGSHABAN")
                        JPARA4.Value = MA0006row("GSHABAN")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(MA0006UPDtbl) Then
                                MA0006UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    MA0006UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            MA0006UPDtbl.Clear()
                            MA0006UPDtbl.Load(SQLdr)
                        End Using

                        For Each MA0006UPDrow As DataRow In MA0006UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "MA006_SHABANORG"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = MA0006UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                                Exit Sub
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MA006_SHABANORG UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:MA006_SHABANORG UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.getReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = MA0006tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub


    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.transitionPrevPage()

    End Sub


    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.setErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
            Exit Sub
        End If

        '○ CS0023XLSUPLOAD.TBLDATAの入力値整備
        Dim WW_COLUMNS As New List(Of String)
        For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
            WW_COLUMNS.Add(XLSTBLcol.ColumnName.ToString())
        Next

        Dim CS0023XLSTBLrow As DataRow = CS0023XLSUPLOAD.TBLDATA.NewRow
        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            CS0023XLSTBLrow.ItemArray = XLSTBLrow.ItemArray

            For Each XLSTBLcol As DataColumn In CS0023XLSUPLOAD.TBLDATA.Columns
                If IsDBNull(CS0023XLSTBLrow.Item(XLSTBLcol)) OrElse IsNothing(CS0023XLSTBLrow.Item(XLSTBLcol)) Then
                    CS0023XLSTBLrow.Item(XLSTBLcol) = ""
                End If
            Next

            XLSTBLrow.ItemArray = CS0023XLSTBLrow.ItemArray
        Next

        '○ XLSUPLOAD明細⇒INPtbl
        Master.CreateEmptyTable(MA0006INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows

            Dim MA0006INProw As DataRow = MA0006INPtbl.NewRow

            '○ 初期クリア
            For Each MA0006INPcol As DataColumn In MA0006INPtbl.Columns
                If IsDBNull(MA0006INProw.Item(MA0006INPcol)) OrElse IsNothing(MA0006INProw.Item(MA0006INPcol)) Then
                    Select Case MA0006INPcol.ColumnName
                        Case "LINECNT"
                            MA0006INProw.Item(MA0006INPcol) = 0
                        Case "OPERATION"
                            MA0006INProw.Item(MA0006INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            MA0006INProw.Item(MA0006INPcol) = 0
                        Case "SELECT"
                            MA0006INProw.Item(MA0006INPcol) = 1
                        Case "HIDDEN"
                            MA0006INProw.Item(MA0006INPcol) = 0
                        Case Else
                            MA0006INProw.Item(MA0006INPcol) = ""
                    End Select
                End If
            Next

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("MANGUORG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                For Each MA0006row As DataRow In MA0006tbl.Rows
                    If XLSTBLrow("CAMPCODE") = MA0006row("CAMPCODE") AndAlso
                        XLSTBLrow("MANGUORG") = MA0006row("MANGUORG") AndAlso
                        XLSTBLrow("GSHABAN") = MA0006row("GSHABAN") Then
                        MA0006INProw.ItemArray = MA0006row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '会社コード
            If WW_COLUMNS.IndexOf("CAMPCODE") >= 0 Then
                MA0006INProw("CAMPCODE") = XLSTBLrow("CAMPCODE")
            End If

            '運用部署
            If WW_COLUMNS.IndexOf("MANGUORG") >= 0 Then
                MA0006INProw("MANGUORG") = XLSTBLrow("MANGUORG")
            End If

            '業務車番
            If WW_COLUMNS.IndexOf("GSHABAN") >= 0 Then
                MA0006INProw("GSHABAN") = XLSTBLrow("GSHABAN")
            End If

            '統一車番(前)(上)
            If WW_COLUMNS.IndexOf("SHARYOTYPEF") >= 0 Then
                MA0006INProw("SHARYOTYPEF") = XLSTBLrow("SHARYOTYPEF")
            End If

            '統一車番(前)(下)
            If WW_COLUMNS.IndexOf("TSHABANF") >= 0 Then
                MA0006INProw("TSHABANF") = XLSTBLrow("TSHABANF")
            End If

            '統一車番(前)
            If WW_COLUMNS.IndexOf("TSHARYOF") >= 0 Then
                MA0006INProw("TSHARYOF") = XLSTBLrow("TSHARYOF")

                '統一車番(前)(上)
                If WW_COLUMNS.IndexOf("SHARYOTYPEF") < 0 Then
                    MA0006INProw("SHARYOTYPEF") = Mid(MA0006INProw("TSHARYOF"), 1, 1)
                End If

                '統一車番(前)(下)
                If WW_COLUMNS.IndexOf("TSHABANF") < 0 Then
                    MA0006INProw("TSHABANF") = Mid(MA0006INProw("TSHARYOF"), 2, 19)
                End If
            Else
                If WW_COLUMNS.IndexOf("SHARYOTYPEF") >= 0 AndAlso
                    WW_COLUMNS.IndexOf("TSHABANF") >= 0 Then
                    MA0006INProw("TSHARYOF") = XLSTBLrow("SHARYOTYPEF") & XLSTBLrow("TSHABANF")
                End If
            End If

            '登録車番(前)
            If WW_COLUMNS.IndexOf("TSHABANFNAMES") >= 0 Then
                MA0006INProw("TSHABANFNAMES") = XLSTBLrow("TSHABANFNAMES")
            End If

            '統一車番(後)(上)
            If WW_COLUMNS.IndexOf("SHARYOTYPEB") >= 0 Then
                MA0006INProw("SHARYOTYPEB") = XLSTBLrow("SHARYOTYPEB")
            End If

            '統一車番(後)(下)
            If WW_COLUMNS.IndexOf("TSHABANB") >= 0 Then
                MA0006INProw("TSHABANB") = XLSTBLrow("TSHABANB")
            End If

            '統一車番(後)
            If WW_COLUMNS.IndexOf("TSHARYOB") >= 0 Then
                MA0006INProw("TSHARYOB") = XLSTBLrow("TSHARYOB")

                '統一車番(後)(上)
                If WW_COLUMNS.IndexOf("SHARYOTYPEB") < 0 Then
                    MA0006INProw("SHARYOTYPEB") = Mid(MA0006INProw("TSHARYOB"), 1, 1)
                End If

                '統一車番(後)(下)
                If WW_COLUMNS.IndexOf("TSHABANB") < 0 Then
                    MA0006INProw("TSHABANB") = Mid(MA0006INProw("TSHARYOB"), 2, 19)
                End If
            Else
                If WW_COLUMNS.IndexOf("SHARYOTYPEB") >= 0 AndAlso
                    WW_COLUMNS.IndexOf("TSHABANB") >= 0 Then
                    MA0006INProw("TSHARYOB") = XLSTBLrow("SHARYOTYPEB") & XLSTBLrow("TSHABANB")
                End If
            End If

            '登録車番(後)
            If WW_COLUMNS.IndexOf("TSHABANBNAMES") >= 0 Then
                MA0006INProw("TSHABANBNAMES") = XLSTBLrow("TSHABANBNAMES")
            End If

            '統一車番(後)2
            If WW_COLUMNS.IndexOf("TSHARYOBB") >= 0 Then
                MA0006INProw("TSHARYOBB") = XLSTBLrow("TSHARYOBB")

                '統一車番(後)2(上)
                If WW_COLUMNS.IndexOf("SHARYOTYPEBB") < 0 Then
                    MA0006INProw("SHARYOTYPEBB") = Mid(MA0006INProw("TSHARYOBB"), 1, 1)
                End If

                '統一車番(後)2(下)
                If WW_COLUMNS.IndexOf("TSHABANBB") < 0 Then
                    MA0006INProw("TSHABANBB") = Mid(MA0006INProw("TSHARYOBB"), 2, 19)
                End If
            Else
                If WW_COLUMNS.IndexOf("SHARYOTYPEBB") >= 0 AndAlso
                    WW_COLUMNS.IndexOf("TSHABANBB") >= 0 Then
                    MA0006INProw("TSHARYOBB") = XLSTBLrow("SHARYOTYPEBB") & XLSTBLrow("TSHABANBB")
                End If
            End If

            '登録車番(後)2
            If WW_COLUMNS.IndexOf("TSHABANBBNAMES") >= 0 Then
                MA0006INProw("TSHABANBBNAMES") = XLSTBLrow("TSHABANBBNAMES")
            End If

            '契約区分
            If WW_COLUMNS.IndexOf("MANGOWNCONT") >= 0 Then
                MA0006INProw("MANGOWNCONT") = XLSTBLrow("MANGOWNCONT")
            End If

            '傭車
            If WW_COLUMNS.IndexOf("MANGSUPPL") >= 0 Then
                MA0006INProw("MANGSUPPL") = XLSTBLrow("MANGSUPPL")
            End If

            '矢崎車端用車番コード
            If WW_COLUMNS.IndexOf("YAZKSHABAN") >= 0 Then
                MA0006INProw("YAZKSHABAN") = XLSTBLrow("YAZKSHABAN")
            End If

            '光英車端用車番コード
            If WW_COLUMNS.IndexOf("KOEISHABAN") >= 0 Then
                MA0006INProw("KOEISHABAN") = XLSTBLrow("KOEISHABAN")
            End If

            'JSR車番コード
            If WW_COLUMNS.IndexOf("JSRSHABAN") >= 0 Then
                MA0006INProw("JSRSHABAN") = XLSTBLrow("JSRSHABAN")
            End If

            '水素車
            If WW_COLUMNS.IndexOf("SUISOKBN") >= 0 Then
                If XLSTBLrow("SUISOKBN") = "0" Then
                    XLSTBLrow("SUISOKBN") = ""
                End If
                MA0006INProw("SUISOKBN") = XLSTBLrow("SUISOKBN")
            End If

            '勤怠用油種区分
            If WW_COLUMNS.IndexOf("OILKBN") >= 0 Then
                MA0006INProw("OILKBN") = XLSTBLrow("OILKBN")
            End If

            '勤怠用車両区分
            If WW_COLUMNS.IndexOf("SHARYOKBN") >= 0 Then
                MA0006INProw("SHARYOKBN") = XLSTBLrow("SHARYOKBN")
            End If

            'SEQ
            If WW_COLUMNS.IndexOf("SEQ") >= 0 Then
                MA0006INProw("SEQ") = XLSTBLrow("SEQ")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                MA0006INProw("DELFLG") = XLSTBLrow("DELFLG")
            End If

            '車両情報１
            If WW_COLUMNS.IndexOf("SHARYOINFO1") >= 0 Then
                MA0006INProw("SHARYOINFO1") = XLSTBLrow("SHARYOINFO1")
            End If

            '車両情報２
            If WW_COLUMNS.IndexOf("SHARYOINFO2") >= 0 Then
                MA0006INProw("SHARYOINFO2") = XLSTBLrow("SHARYOINFO2")
            End If

            '車両情報３
            If WW_COLUMNS.IndexOf("SHARYOINFO3") >= 0 Then
                MA0006INProw("SHARYOINFO3") = XLSTBLrow("SHARYOINFO3")
            End If

            '車両情報４
            If WW_COLUMNS.IndexOf("SHARYOINFO4") >= 0 Then
                MA0006INProw("SHARYOINFO4") = XLSTBLrow("SHARYOINFO4")
            End If

            '車両情報５
            If WW_COLUMNS.IndexOf("SHARYOINFO5") >= 0 Then
                MA0006INProw("SHARYOINFO5") = XLSTBLrow("SHARYOINFO5")
            End If

            '車両情報６
            If WW_COLUMNS.IndexOf("SHARYOINFO6") >= 0 Then
                MA0006INProw("SHARYOINFO6") = XLSTBLrow("SHARYOINFO6")
            End If

            '名称取得
            CODENAME_get("MANGOWNCONT", MA0006INProw("MANGOWNCONT"), MA0006INProw("MANGOWNCONTNAMES"), WW_DUMMY)        '契約区分
            CODENAME_get("MANGSUPPL", MA0006INProw("MANGSUPPL"), MA0006INProw("MANGSUPPLNAMES"), WW_DUMMY)              '庸車会社
            CODENAME_get("SUISOKBN", MA0006INProw("SUISOKBN"), MA0006INProw("SUISOKBNNAMES"), WW_DUMMY)                 '水素フラグ
            CODENAME_get("OILKBN", MA0006INProw("OILKBN"), MA0006INProw("OILKBNNAMES"), WW_DUMMY)                       '油種区分（勤怠用）
            CODENAME_get("SHARYOKBN", MA0006INProw("SHARYOKBN"), MA0006INProw("SHARYOKBNNAMES"), WW_DUMMY)              '車両区分（勤怠用）

            MA0006INPtbl.Rows.Add(MA0006INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        MA0006tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(MA0006tbl)

        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        For Each MA0006INProw As DataRow In MA0006INPtbl.Rows

            WW_LINE_ERR = ""

            '会社コード
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", MA0006INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("CAMPCODE", MA0006INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_CAMPCODE.Text <> MA0006INProw("CAMPCODE") Then
                    WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                    WW_CheckMES2 = "検索条件の会社コードと一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用部署
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "UORG", MA0006INProw("MANGUORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("UORG", MA0006INProw("MANGUORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '対象チェック
                If work.WF_SEL_UORG.Text <> MA0006INProw("MANGUORG") Then
                    WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                    WW_CheckMES2 = "検索条件の会社コードと一致しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用部署エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '業務車番
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "GSHABAN", MA0006INProw("GSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(業務車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If MA0006INProw("DELFLG") <> C_DELETE_FLG.DELETE Then

                '統一車番(前)(上)
                WW_TEXT = MA0006INProw("SHARYOTYPEF")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPEF", MA0006INProw("SHARYOTYPEF"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006INProw("SHARYOTYPEF") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SHARYOTYPE", MA0006INProw("SHARYOTYPEF"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) OrElse WW_DUMMY <> "前" Then
                            WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                            WW_CheckMES2 = "車両タイプエラーです。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(前)(下)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANF", MA0006INProw("TSHABANF"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(前)存在チェック
                If MA0006INProw("TSHARYOF") <> "" Then
                    CODENAME_get("TSHARYO", MA0006INProw("TSHARYOF"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番(前)エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If

                '登録車番(前)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANFNAMES", MA0006INProw("TSHABANFNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(登録車番(前)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)(上)
                WW_TEXT = MA0006INProw("SHARYOTYPEB")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPEB", MA0006INProw("SHARYOTYPEB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006INProw("SHARYOTYPEB") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SHARYOTYPE", MA0006INProw("SHARYOTYPEB"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) OrElse WW_DUMMY <> "後" Then
                            WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                            WW_CheckMES2 = "車両タイプエラーです。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)(下)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANB", MA0006INProw("TSHABANB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)存在チェック
                If MA0006INProw("TSHARYOB") <> "" Then
                    CODENAME_get("TSHARYO", MA0006INProw("TSHARYOB"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番(後)エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If

                '登録車番(後)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANBNAMES", MA0006INProw("TSHABANBNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(登録車番(後)エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)2(上)
                WW_TEXT = MA0006INProw("SHARYOTYPEBB")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOTYPEB2", MA0006INProw("SHARYOTYPEBB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006INProw("SHARYOTYPEBB") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SHARYOTYPE", MA0006INProw("SHARYOTYPEBB"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) OrElse WW_DUMMY <> "後" Then
                            WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                            WW_CheckMES2 = "車両タイプエラーです。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)2(下)
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANB2", MA0006INProw("TSHABANBB"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '統一車番(後)2存在チェック
                If MA0006INProw("TSHARYOBB") <> "" Then
                    CODENAME_get("TSHARYO", MA0006INProw("TSHARYOBB"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(統一車番(後)2エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If

                '登録車番(後)2
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "TSHABANB2NAMES", MA0006INProw("TSHABANBBNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(登録車番(後)2エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '契約区分
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MANGOWNCONT", MA0006INProw("MANGOWNCONT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("MANGOWNCONT", MA0006INProw("MANGOWNCONT"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(契約区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(契約区分エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '傭車
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "MANGSUPPL", MA0006INProw("MANGSUPPL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("MANGSUPPL", MA0006INProw("MANGSUPPL"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(傭車エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(傭車エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '矢崎車端用車番コード
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "YAZKSHABAN", MA0006INProw("YAZKSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(矢崎車端用車番コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '光英車端用車番コード
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "KOEISHABAN", MA0006INProw("KOEISHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(光英車端用車番コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                'JSR車番コード
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "JSRSHABAN", MA0006INProw("JSRSHABAN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(JSR車番コードエラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '水素車
                WW_TEXT = MA0006INProw("SUISOKBN")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SUISOKBN", MA0006INProw("SUISOKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" OrElse WW_TEXT = "0" Then
                        MA0006INProw("SUISOKBN") = ""
                    Else
                        '存在チェック
                        CODENAME_get("SUISOKBN", MA0006INProw("SUISOKBN"), WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・更新できないレコード(水素車エラー)です。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(水素車エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '勤怠用油種区分
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "OILPAYKBN", MA0006INProw("OILKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("OILKBN", MA0006INProw("OILKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(勤怠用油種区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(勤怠用油種区分エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                '勤怠用車両区分
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOKBN", MA0006INProw("SHARYOKBN"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '存在チェック
                    CODENAME_get("SHARYOKBN", MA0006INProw("SHARYOKBN"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(勤怠用車両区分エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(勤怠用車両区分エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

                'SEQ
                WW_TEXT = MA0006INProw("SEQ")
                Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SEQ", MA0006INProw("SEQ"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT = "" Then
                        MA0006INProw("SEQ") = ""
                    Else
                        Try
                            MA0006INProw("SEQ") = Format(CInt(MA0006INProw("SEQ")), "#0")
                        Catch ex As Exception
                            MA0006INProw("SEQ") = "0"
                        End Try
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(表示順番エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '削除
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "DELFLG", MA0006INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '存在チェック
                CODENAME_get("DELFLG", MA0006INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報１
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO1", MA0006INProw("SHARYOINFO1"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報１エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報２
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO2", MA0006INProw("SHARYOINFO2"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報２エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報３
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO3", MA0006INProw("SHARYOINFO3"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報３エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報４
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO4", MA0006INProw("SHARYOINFO4"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報４エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報５
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO5", MA0006INProw("SHARYOINFO5"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報５エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車両情報６
            Master.checkFIeld(work.WF_SEL_CAMPCODE.Text, "SHARYOINFO6", MA0006INProw("SHARYOINFO6"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(車両情報６エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If MA0006INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MA0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MA0006INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

        '重複チェック
        For i As Integer = 0 To MA0006INPtbl.Rows.Count - 1

            If MA0006INPtbl.Rows(i)("GSHABAN") = "" OrElse
                MA0006INPtbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE Then
                Continue For
            End If

            WW_LINE_ERR = ""

            For j As Integer = i + 1 To MA0006INPtbl.Rows.Count - 1

                If MA0006INPtbl.Rows(j)("GSHABAN") = "" OrElse
                    MA0006INPtbl.Rows(j)("DELFLG") = C_DELETE_FLG.DELETE Then
                    Continue For
                End If

                If MA0006INPtbl.Rows(i)("GSHABAN") = MA0006INPtbl.Rows(j)("GSHABAN") Then
                    WW_CheckMES1 = "・更新できないレコード(業務車番重複エラー)です。"
                    WW_CheckMES2 = ""
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, MA0006INPtbl.Rows(i))
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit For
                End If
            Next

            If WW_LINE_ERR = "" Then
                If MA0006INPtbl.Rows(i)("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    MA0006INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                MA0006INPtbl.Rows(i)("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End If
        Next

    End Sub

    ''' <summary>
    ''' MA0006tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub MA0006tbl_UPD()

        '○ 追加変更判定
        For Each MA0006INProw As DataRow In MA0006INPtbl.Rows

            'エラーレコード読み飛ばし
            If MA0006INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            MA0006INProw("OPERATION") = "Insert"

            'KEY項目が等しい
            For Each MA0006row As DataRow In MA0006tbl.Rows
                If MA0006row("CAMPCODE") = MA0006INProw("CAMPCODE") AndAlso
                    MA0006row("MANGUORG") = MA0006INProw("MANGUORG") AndAlso
                    MA0006row("GSHABAN") = MA0006INProw("GSHABAN") Then

                    '変更無は操作無
                    If MA0006row("TSHARYOF") = MA0006INProw("TSHARYOF") AndAlso
                        MA0006row("TSHABANFNAMES") = MA0006INProw("TSHABANFNAMES") AndAlso
                        MA0006row("TSHARYOB") = MA0006INProw("TSHARYOB") AndAlso
                        MA0006row("TSHABANBNAMES") = MA0006INProw("TSHABANBNAMES") AndAlso
                        MA0006row("TSHARYOBB") = MA0006INProw("TSHARYOBB") AndAlso
                        MA0006row("TSHABANBBNAMES") = MA0006INProw("TSHABANBBNAMES") AndAlso
                        MA0006row("MANGOWNCONT") = MA0006INProw("MANGOWNCONT") AndAlso
                        MA0006row("MANGSUPPL") = MA0006INProw("MANGSUPPL") AndAlso
                        MA0006row("YAZKSHABAN") = MA0006INProw("YAZKSHABAN") AndAlso
                        MA0006row("KOEISHABAN") = MA0006INProw("KOEISHABAN") AndAlso
                        MA0006row("JSRSHABAN") = MA0006INProw("JSRSHABAN") AndAlso
                        MA0006row("SUISOKBN") = MA0006INProw("SUISOKBN") AndAlso
                        MA0006row("OILKBN") = MA0006INProw("OILKBN") AndAlso
                        MA0006row("SHARYOKBN") = MA0006INProw("SHARYOKBN") AndAlso
                        MA0006row("SEQ") = MA0006INProw("SEQ") AndAlso
                        MA0006row("DELFLG") = MA0006INProw("DELFLG") AndAlso
                        MA0006row("SHARYOINFO1") = MA0006INProw("SHARYOINFO1") AndAlso
                        MA0006row("SHARYOINFO2") = MA0006INProw("SHARYOINFO2") AndAlso
                        MA0006row("SHARYOINFO3") = MA0006INProw("SHARYOINFO3") AndAlso
                        MA0006row("SHARYOINFO4") = MA0006INProw("SHARYOINFO4") AndAlso
                        MA0006row("SHARYOINFO5") = MA0006INProw("SHARYOINFO5") AndAlso
                        MA0006row("SHARYOINFO6") = MA0006INProw("SHARYOINFO6") Then
                        MA0006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                        Exit For
                    End If

                    MA0006INProw("OPERATION") = "Update"
                    Exit For
                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each MA0006INProw As DataRow In MA0006INPtbl.Rows
            Select Case MA0006INProw("OPERATION")
                Case "Update"
                    TBL_UPDATE_SUB(MA0006INProw)
                Case "Insert"
                    TBL_INSERT_SUB(MA0006INProw)
                Case "エラー"
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="MA0006INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef MA0006INProw As DataRow)

        For Each MA0006row As DataRow In MA0006tbl.Rows

            '同一レコード
            If MA0006INProw("CAMPCODE") = MA0006row("CAMPCODE") AndAlso
                MA0006INProw("MANGUORG") = MA0006row("MANGUORG") AndAlso
                MA0006INProw("GSHABAN") = MA0006row("GSHABAN") Then

                '画面入力テーブル項目設定
                MA0006INProw("LINECNT") = MA0006row("LINECNT")
                MA0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                MA0006INProw("TIMSTP") = MA0006row("TIMSTP")
                MA0006INProw("SELECT") = 1
                MA0006INProw("HIDDEN") = 0

                '更新前業務車番を上書きしないように念のためコピー
                MA0006INProw("OLDGSHABAN") = MA0006row("OLDGSHABAN")

                '項目テーブル項目設定
                MA0006row.ItemArray = MA0006INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="MA0006INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef MA0006INProw As DataRow)

        Dim WW_INSERT As Boolean = False

        '○ 項目テーブル項目設定
        For Each MA0006row As DataRow In MA0006tbl.Rows

            '何かが入力されている行には追加しない
            If MA0006row("GSHABAN") <> "" OrElse
                MA0006row("SHARYOTYPEF") <> "" OrElse
                MA0006row("TSHABANF") <> "" OrElse
                MA0006row("TSHARYOF") <> "" OrElse
                MA0006row("TSHABANFNAMES") <> "" OrElse
                MA0006row("SHARYOTYPEB") <> "" OrElse
                MA0006row("TSHABANB") <> "" OrElse
                MA0006row("TSHARYOB") <> "" OrElse
                MA0006row("TSHABANBNAMES") <> "" OrElse
                MA0006row("SHARYOTYPEBB") <> "" OrElse
                MA0006row("TSHABANBB") <> "" OrElse
                MA0006row("TSHARYOBB") <> "" OrElse
                MA0006row("TSHABANBBNAMES") <> "" OrElse
                MA0006row("MANGOWNCONT") <> "" OrElse
                MA0006row("MANGSUPPL") <> "" OrElse
                MA0006row("YAZKSHABAN") <> "" OrElse
                MA0006row("KOEISHABAN") <> "" OrElse
                MA0006row("JSRSHABAN") <> "" OrElse
                MA0006row("SUISOKBN") <> "" OrElse
                MA0006row("OILKBN") <> "" OrElse
                MA0006row("SHARYOKBN") <> "" OrElse
                MA0006row("SEQ") <> "" OrElse
                MA0006row("DELFLG") <> "" OrElse
                MA0006row("SHARYOINFO1") <> "" OrElse
                MA0006row("SHARYOINFO2") <> "" OrElse
                MA0006row("SHARYOINFO3") <> "" OrElse
                MA0006row("SHARYOINFO4") <> "" OrElse
                MA0006row("SHARYOINFO5") <> "" OrElse
                MA0006row("SHARYOINFO6") <> "" Then
                Continue For
            End If

            '画面入力テーブル項目設定
            MA0006INProw("LINECNT") = MA0006row("LINECNT")
            MA0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            MA0006INProw("TIMSTP") = MA0006row("TIMSTP")
            MA0006INProw("SELECT") = 1
            MA0006INProw("HIDDEN") = 0

            '項目テーブル項目設定
            MA0006row.ItemArray = MA0006INProw.ItemArray
            WW_INSERT = True
            Exit For
        Next

        '○ 空行分を超えていた場合、新規行追加
        If Not WW_INSERT Then
            Dim MA0006row As DataRow = MA0006tbl.NewRow
            MA0006row.ItemArray = MA0006INProw.ItemArray

            MA0006row("LINECNT") = MA0006tbl.Rows.Count + 1
            MA0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
            MA0006row("TIMSTP") = "0"
            MA0006row("SELECT") = 1
            MA0006row("HIDDEN") = 0

            MA0006tbl.Rows.Add(MA0006row)
        End If

    End Sub

    ''' <summary>
    ''' 一覧変更情報取込処理
    ''' </summary>
    Protected Sub WF_TableChange()

        For Each MA0006row As DataRow In MA0006tbl.Rows
            WF_SelectedIndex.Value = CStr(MA0006row("LINECNT"))
            WF_ListChange(False)
        Next

        '○ 画面表示データ保存
        Master.SaveTable(MA0006tbl)

    End Sub

    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        WF_ListChange(True)

    End Sub
    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <param name="isSaving">更新保存可否</param>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(ByVal isSaving As Boolean)

        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectedIndex.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 変更チェック
        '業務車番
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "GSHABAN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("GSHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "GSHABAN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("GSHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "GSHABAN" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '登録車番(前)
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHABANFNAMES" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("TSHABANFNAMES") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANFNAMES" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("TSHABANFNAMES") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANFNAMES" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '統一車番(前)
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHARYOF" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("TSHARYOF") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOF" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("TSHARYOF") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOF" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("SHARYOTYPEF") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOF"), 1, 1)
            MA0006tbl.Rows(WW_LINECNT)("TSHABANF") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOF"), 2, 19)
            CODENAME_get("TSHARYO", MA0006tbl(WW_LINECNT)("TSHARYOF"), MA0006tbl(WW_LINECNT)("TSHABANFNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '登録車番(後)
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHABANBNAMES" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("TSHABANBNAMES") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBNAMES" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("TSHABANBNAMES") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBNAMES" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '統一車番(後)
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHARYOB" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("TSHARYOB") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOB" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("TSHARYOB") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOB" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("SHARYOTYPEB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOB"), 1, 1)
            MA0006tbl.Rows(WW_LINECNT)("TSHABANB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOB"), 2, 19)
            CODENAME_get("TSHARYO", MA0006tbl(WW_LINECNT)("TSHARYOB"), MA0006tbl(WW_LINECNT)("TSHABANBNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '登録車番(後)2
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHABANBBNAMES" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("TSHABANBBNAMES") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBBNAMES" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("TSHABANBBNAMES") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHABANBBNAMES" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '統一車番(後)2
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "TSHARYOBB" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOBB" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "TSHARYOBB" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("SHARYOTYPEBB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB"), 1, 1)
            MA0006tbl.Rows(WW_LINECNT)("TSHABANBB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB"), 2, 19)
            CODENAME_get("TSHARYO", MA0006tbl(WW_LINECNT)("TSHARYOBB"), MA0006tbl(WW_LINECNT)("TSHABANBBNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '契約区分
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "MANGOWNCONT" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("MANGOWNCONT") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGOWNCONT" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("MANGOWNCONT") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGOWNCONT" & WF_SelectedIndex.Value))
            CODENAME_get("MANGOWNCONT", MA0006tbl(WW_LINECNT)("MANGOWNCONT"), MA0006tbl(WW_LINECNT)("MANGOWNCONTNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '傭車
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "MANGSUPPL" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("MANGSUPPL") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGSUPPL" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("MANGSUPPL") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MANGSUPPL" & WF_SelectedIndex.Value))
            CODENAME_get("MANGSUPPL", MA0006tbl(WW_LINECNT)("MANGSUPPL"), MA0006tbl(WW_LINECNT)("MANGSUPPLNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '矢崎車端用車番コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "YAZKSHABAN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("YAZKSHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YAZKSHABAN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("YAZKSHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "YAZKSHABAN" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '光英車端用車番コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "KOEISHABAN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("KOEISHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KOEISHABAN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("KOEISHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "KOEISHABAN" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'JSR車番コード
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "JSRSHABAN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("JSRSHABAN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSHABAN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("JSRSHABAN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "JSRSHABAN" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '水素車
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SUISOKBN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SUISOKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SUISOKBN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SUISOKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SUISOKBN" & WF_SelectedIndex.Value))
            CODENAME_get("SUISOKBN", MA0006tbl(WW_LINECNT)("SUISOKBN"), MA0006tbl(WW_LINECNT)("SUISOKBNNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '勤怠用油種区分
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "OILKBN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("OILKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "OILKBN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("OILKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "OILKBN" & WF_SelectedIndex.Value))
            CODENAME_get("OILKBN", MA0006tbl(WW_LINECNT)("OILKBN"), MA0006tbl(WW_LINECNT)("OILKBNNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '勤怠用車両区分
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOKBN" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOKBN") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOKBN" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOKBN") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOKBN" & WF_SelectedIndex.Value))
            CODENAME_get("SHARYOKBN", MA0006tbl(WW_LINECNT)("SHARYOKBN"), MA0006tbl(WW_LINECNT)("SHARYOKBNNAMES"), WW_DUMMY)
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        'SEQ
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SEQ") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SEQ") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEQ" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '削除
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DELFLG" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("DELFLG") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("DELFLG") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DELFLG" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '車両情報１
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO1" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO1") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO1" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO1") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO1" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '車両情報２
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO2" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO2") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO2" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO2") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO2" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '車両情報３
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO3" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO3") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO3" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO3") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO3" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '車両情報４
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO4" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO4") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO4" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO4") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO4" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '車両情報５
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO5" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO5") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO5" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO5") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO5" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '車両情報６
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO6" & WF_SelectedIndex.Value)) AndAlso
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO6") <> Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO6" & WF_SelectedIndex.Value)) Then
            MA0006tbl.Rows(WW_LINECNT)("SHARYOINFO6") = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHARYOINFO6" & WF_SelectedIndex.Value))
            MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        End If

        '○ 画面表示データ保存
        If isSaving Then Master.SaveTable(MA0006tbl)

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Dim prmData = New Hashtable
                prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                Select Case WF_FIELD.Value
                    Case "TSHARYOF"             '統一車番(前)
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CARCODE
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.FRONT
                    Case "TSHARYOB"             '統一車番(後)
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CARCODE
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.REAR
                    Case "TSHARYOBB"            '統一車番(後)2
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CARCODE
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.REAR
                    Case "MANGOWNCONT"          '契約区分
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "MANGOWNCONT"
                    Case "MANGSUPPL"            '傭車
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_CUSTOMER
                    Case "SUISOKBN"             '水素車
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SUISOKBN"
                    Case "OILKBN"               '勤怠用油種区分
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OILPAYKBN"
                    Case "SHARYOKBN"            '勤怠用車両区分
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHARYOKBN"
                    Case "DELFLG"               '削除
                        WF_LeftMViewChange.Value = LIST_BOX_CLASSIFICATION.LC_DELFLG
                End Select

                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CARCODE
                        WF_LeftboxOpen.Value = "TSHABANTABLEOpen"
                        .seTTableList(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .LF_FILTER_CODE = C_FILTER_CODE.RESEACH
                        .LF_PARAM_DATA = work.WF_SEL_CAMPCODE.Text
                        .activeTable()
                    Case Else
                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
                End Select
            End With
        End If

    End Sub


    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_LINECNT As Integer = 0
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_SelectTable As String()

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_SelectLine.Value, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "TSHARYOF"             '統一車番(前)
                If leftview.WF_TBL_SELECT.Text <> "" Then
                    WW_SelectTable = leftview.WF_TBL_SELECT.Text.Split("|"c)
                    If MA0006tbl.Rows(WW_LINECNT)("TSHARYOF") <> Mid(WW_SelectTable(0), InStr(WW_SelectTable(0), "=") + 1, Len(WW_SelectTable(0))) Then
                        MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    MA0006tbl.Rows(WW_LINECNT)("TSHARYOF") = Mid(WW_SelectTable(0), InStr(WW_SelectTable(0), "=") + 1, Len(WW_SelectTable(0)))
                    MA0006tbl.Rows(WW_LINECNT)("TSHABANFNAMES") = Mid(WW_SelectTable(1), InStr(WW_SelectTable(1), "=") + 1, Len(WW_SelectTable(1)))

                    MA0006tbl.Rows(WW_LINECNT)("SHARYOTYPEF") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOF"), 1, 1)
                    MA0006tbl.Rows(WW_LINECNT)("TSHABANF") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOF"), 2, 19)
                End If

            Case "TSHARYOB"             '統一車番(後)
                If leftview.WF_TBL_SELECT.Text <> "" Then
                    WW_SelectTable = leftview.WF_TBL_SELECT.Text.Split("|"c)
                    If MA0006tbl.Rows(WW_LINECNT)("TSHARYOB") <> Mid(WW_SelectTable(0), InStr(WW_SelectTable(0), "=") + 1, Len(WW_SelectTable(0))) Then
                        MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    MA0006tbl.Rows(WW_LINECNT)("TSHARYOB") = Mid(WW_SelectTable(0), InStr(WW_SelectTable(0), "=") + 1, Len(WW_SelectTable(0)))
                    MA0006tbl.Rows(WW_LINECNT)("TSHABANBNAMES") = Mid(WW_SelectTable(1), InStr(WW_SelectTable(1), "=") + 1, Len(WW_SelectTable(1)))

                    MA0006tbl.Rows(WW_LINECNT)("SHARYOTYPEB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOB"), 1, 1)
                    MA0006tbl.Rows(WW_LINECNT)("TSHABANB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOB"), 2, 19)
                End If

            Case "TSHARYOBB"            '統一車番(後)2
                If leftview.WF_TBL_SELECT.Text <> "" Then
                    WW_SelectTable = leftview.WF_TBL_SELECT.Text.Split("|"c)
                    If MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB") <> Mid(WW_SelectTable(0), InStr(WW_SelectTable(0), "=") + 1, Len(WW_SelectTable(0))) Then
                        MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If
                    MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB") = Mid(WW_SelectTable(0), InStr(WW_SelectTable(0), "=") + 1, Len(WW_SelectTable(0)))
                    MA0006tbl.Rows(WW_LINECNT)("TSHABANBBNAMES") = Mid(WW_SelectTable(1), InStr(WW_SelectTable(1), "=") + 1, Len(WW_SelectTable(1)))

                    MA0006tbl.Rows(WW_LINECNT)("SHARYOTYPEBB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB"), 1, 1)
                    MA0006tbl.Rows(WW_LINECNT)("TSHABANBB") = Mid(MA0006tbl.Rows(WW_LINECNT)("TSHARYOBB"), 2, 19)
                End If

            Case "MANGOWNCONT"          '契約区分
                If MA0006tbl.Rows(WW_LINECNT)("MANGOWNCONT") <> WW_SelectValue Then
                    MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MA0006tbl.Rows(WW_LINECNT)("MANGOWNCONT") = WW_SelectValue
                MA0006tbl.Rows(WW_LINECNT)("MANGOWNCONTNAMES") = WW_SelectText

            Case "MANGSUPPL"            '傭車
                If MA0006tbl.Rows(WW_LINECNT)("MANGSUPPL") <> WW_SelectValue Then
                    MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MA0006tbl.Rows(WW_LINECNT)("MANGSUPPL") = WW_SelectValue
                MA0006tbl.Rows(WW_LINECNT)("MANGSUPPLNAMES") = WW_SelectText

            Case "SUISOKBN"             '水素車
                If WW_SelectValue = "0" Then
                    WW_SelectValue = ""
                End If
                If MA0006tbl.Rows(WW_LINECNT)("SUISOKBN") <> WW_SelectValue Then
                    MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MA0006tbl.Rows(WW_LINECNT)("SUISOKBN") = WW_SelectValue
                MA0006tbl.Rows(WW_LINECNT)("SUISOKBNNAMES") = WW_SelectText

            Case "OILKBN"               '勤怠用油種区分
                If MA0006tbl.Rows(WW_LINECNT)("OILKBN") <> WW_SelectValue Then
                    MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MA0006tbl.Rows(WW_LINECNT)("OILKBN") = WW_SelectValue
                MA0006tbl.Rows(WW_LINECNT)("OILKBNNAMES") = WW_SelectText

            Case "SHARYOKBN"            '勤怠用車両区分
                If MA0006tbl.Rows(WW_LINECNT)("SHARYOKBN") <> WW_SelectValue Then
                    MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MA0006tbl.Rows(WW_LINECNT)("SHARYOKBN") = WW_SelectValue
                MA0006tbl.Rows(WW_LINECNT)("SHARYOKBNNAMES") = WW_SelectText

            Case "DELFLG"               '削除
                If MA0006tbl.Rows(WW_LINECNT)("DELFLG") <> WW_SelectValue Then
                    MA0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
                MA0006tbl.Rows(WW_LINECNT)("DELFLG") = WW_SelectValue
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(MA0006tbl)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBox検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LeftBoxSubmit_Click()

        Dim WW_TEXT As String = ""

        If Not IsNothing(Request.Form("WF_LeftBoxParam")) Then
            WW_TEXT = Convert.ToString(Request.Form("WF_LeftBoxParam"))
        End If

        With leftview
            Dim prmData = New Hashtable
            prmData.Item(C_PARAMETERS.LP_COMPANY) = Convert.ToString(Request.Form("WF_LeftBoxParam"))

            Select Case WF_FIELD.Value
                Case "TSHARYOF"                     '統一車番(前)
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.FRONT
                Case "TSHARYOB", "TSHARYOBB"        '統一車番(後)
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0007CarList.LC_LORRY_TYPE.REAR
            End Select

            .seTTableList(LIST_BOX_CLASSIFICATION.LC_CARCODE, WW_DUMMY, prmData)
            .LF_FILTER_CODE = C_FILTER_CODE.RESEACH
            .LF_PARAM_DATA = WW_TEXT
            .activeTable()
        End With

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "TSHARYOF"             '統一車番(前)
            Case "TSHARYOB"             '統一車番(後)
            Case "TSHARYOBB"            '統一車番(後)2
            Case "MANGOWNCONT"          '契約区分
            Case "MANGSUPPL"            '傭車
            Case "SUISOKBN"             '水素車
            Case "OILKBN"               '勤怠用油種区分
            Case "SHARYOKBN"            '勤怠用車両区分
            Case "DELFLG"               '削除
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub


    ''' <summary>
    ''' RightBoxラジオボタン選択処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RadioButton_Click()

        If Not String.IsNullOrEmpty(WF_RightViewChange.Value) Then
            Try
                Integer.TryParse(WF_RightViewChange.Value, WF_RightViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            rightview.selectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub


    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="MA0006row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal MA0006row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(MA0006row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード    =" & MA0006row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用部署      =" & MA0006row("MANGUORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 業務車番      =" & MA0006row("GSHABAN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 統一車番(前)  =" & MA0006row("TSHARYOF") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 登録車番(前)  =" & MA0006row("TSHABANFNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 統一車番(後)  =" & MA0006row("TSHARYOB") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 登録車番(後)  =" & MA0006row("TSHABANBNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 統一車番(後)2 =" & MA0006row("TSHARYOBB") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 登録車番(後)2 =" & MA0006row("TSHABANBBNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 契約区分      =" & MA0006row("MANGOWNCONT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 傭車          =" & MA0006row("MANGSUPPL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順番      =" & MA0006row("SEQ") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除          =" & MA0006row("DELFLG")
        End If

        rightview.addErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"             '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"                 '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHARYOTYPE"           '統一車番(上)
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHARYOTYPE"
                    leftview.CodeToName(I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TSHARYO"              '統一車番
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CARCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MANGOILTYPE"          '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MANGOWNCONT"          '契約区分
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "MANGOWNCONT"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MANGSUPPL"            '庸車会社
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CUSTOMER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SUISOKBN"             '水素フラグ
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SUISOKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OILKBN"               '油種区分（勤怠用）
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "OILPAYKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHARYOKBN"            '車両区分（勤怠用）
                    prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHARYOKBN"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"               '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

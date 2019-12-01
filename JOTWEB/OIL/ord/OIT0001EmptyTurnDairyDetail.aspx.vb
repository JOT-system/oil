'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0001EmptyTurnDairyDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0001tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0001INPtbl As DataTable                              'チェック用テーブル
    Private OIT0001UPDtbl As DataTable                              '更新用テーブル
    Private OIT0001WKtbl As DataTable                               '作業用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 8                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""
    Private WW_ERRCODE As String                                    'サブ用リターンコード

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0001tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonLINE_LIFTED"     '行削除ボタン押下
                            WF_ButtonLINE_LIFTED_Click()
                        Case "WF_ButtonLINE_ADD"        '行追加ボタン押下
                            WF_ButtonLINE_ADD_Click()
                        Case "WF_ButtonCSV"             'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonUPDATE"          '明細更新ボタン押下
                            WF_ButtonUPDATE_Click()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
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
            If Not IsNothing(OIT0001tbl) Then
                OIT0001tbl.Clear()
                OIT0001tbl.Dispose()
                OIT0001tbl = Nothing
            End If

            If Not IsNothing(OIT0001INPtbl) Then
                OIT0001INPtbl.Clear()
                OIT0001INPtbl.Dispose()
                OIT0001INPtbl = Nothing
            End If

            If Not IsNothing(OIT0001UPDtbl) Then
                OIT0001UPDtbl.Clear()
                OIT0001UPDtbl.Dispose()
                OIT0001UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0001WRKINC.MAPIDD
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ''○ 検索画面からの遷移
        'If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0001S Then
        'Grid情報保存先のファイル名

        Master.CreateXMLSaveFile()

        'ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0001D Then
        '    Master.RecoverTable(OIT0001tbl, work.WF_SEL_INPTBL.Text)
        'End If

        '受注営業所
        'TxtOrderOffice.Text = work.WF_SEL_ORDERSALESOFFICE.Text
        '本社列車
        TxtHeadOfficeTrain.Text = work.WF_SEL_TRAIN.Text
        '発駅
        TxtDepstation.Text = work.WF_SEL_DEPARTURESTATION.Text
        '着駅
        TxtArrstation.Text = work.WF_SEL_ARRIVALSTATION.Text
        '(予定)積込日
        TxtLoadingDate.Text = work.WF_SEL_LOADINGDATE.Text
        '(予定)発日
        TxtDepDate.Text = work.WF_SEL_LOADINGCAR_DEPARTUREDATE.Text
        '(予定)積車着日
        TxtArrDate.Text = work.WF_SEL_LOADINGCAR_ARRIVALDATE.Text
        '(予定)受入日
        TxtAccDate.Text = work.WF_SEL_RECEIPTDATE.Text

        '合計車数
        TxtTotalTank.Text = work.WF_SEL_TANKCARTOTAL.Text
        '車数（レギュラー）
        TxtRTank.Text = work.WF_SEL_REGULAR_TANKCAR.Text
        '車数（ハイオク）
        TxtHTank.Text = work.WF_SEL_HIGHOCTANE_TANKCAR.Text
        '車数（灯油）
        TxtTTank.Text = work.WF_SEL_KEROSENE_TANKCAR.Text
        '車数（未添加灯油）
        TxtMTTank.Text = work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text
        '車数（軽油）
        TxtKTank.Text = work.WF_SEL_DIESEL_TANKCAR.Text
        '車数（３号軽油）
        TxtK3Tank.Text = work.WF_SEL_NUM3DIESEL_TANKCAR.Text
        '車数（５号軽油）
        TxtK5Tank.Text = work.WF_SEL_NUM5DIESEL_TANKCAR.Text
        '車数（１０号軽油）
        TxtK10Tank.Text = work.WF_SEL_NUM10DIESEL_TANKCAR.Text
        '車数（LSA）
        TxtLTank.Text = work.WF_SEL_LSA_TANKCAR.Text
        '車数（A重油）
        TxtATank.Text = work.WF_SEL_AHEAVY_TANKCAR.Text

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        '受注営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_SALESOFFICECODE.Text, TxtOrderOffice.Text, WW_DUMMY)
        '発駅
        CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_DUMMY)

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
        Master.SaveTable(OIT0001tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
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

        If IsNothing(OIT0001tbl) Then
            OIT0001tbl = New DataTable
        End If

        If OIT0001tbl.Columns.Count <> 0 Then
            OIT0001tbl.Columns.Clear()
        End If

        OIT0001tbl.Clear()

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        '○ 取得SQL
        '　検索説明　：　受注№の連番を決める
        Dim SQLStrNum As String =
        " SELECT " _
            & " ISNULL(FORMAT(MAX(SUBSTRING(OIT0002.ORDERNO, 10, 2)) + 1,'00'),'01') AS ORDERNO_NUM" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " WHERE SUBSTRING(OIT0002.ORDERNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd')"

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注、受注明細等のマスタから取得する
        Dim SQLStr As String = ""

        '新規登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = 1 Then

            SQLStr =
              " SELECT TOP (18)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , ''                                             AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , FORMAT(GETDATE(),'yyyy/MM/dd')                 AS ORDERYMD" _
            & " , ''                                             AS SHIPPERSNAME" _
            & " , ''                                             AS OILCODE" _
            & " , ''                                             AS OILNAME" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS RETURNDATETRAIN" _
            & " , ''                                             AS JOINT" _
            & " , @P2                                            AS DELFLG" _
            & " , 'O' + FORMAT(GETDATE(),'yyyyMMdd') + @P1       AS ORDERNO" _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS DETAILNO" _
            & " , ''                                             AS KAMOKU" _
            & " FROM sys.all_objects "

            SQLStr &=
                  " ORDER BY" _
                & "    LINECNT"

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = 2 Then
            SQLStr =
              " SELECT" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , CAST(OIT0002.UPDTIMSTP AS bigint)              AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '')            AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '   ')     AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0003.OILCODE), '')             AS OILCODE" _
            & " , ISNULL(RTRIM(OIM0003_NOW.OILNAME), '')         AS OILNAME" _
            & " , ISNULL(RTRIM(OIT0003.TANKNO), '')              AS TANKNO" _
            & " , ISNULL(RTRIM(OIT0005.LASTOILCODE), '')         AS LASTOILCODE" _
            & " , ISNULL(RTRIM(OIM0003_PAST.OILNAME), '')        AS LASTOILNAME" _
            & " , CASE" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
            & "   END                                                                      AS JRINSPECTIONALERT" _
            & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), '')               AS JRINSPECTIONDATE" _
            & " , CASE" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
            & "   END                                                                      AS JRALLINSPECTIONALERT" _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), '')            AS JRALLINSPECTIONDATE" _
            & " , ISNULL(FORMAT(OIT0003.RETURNDATETRAIN, 'yyyy/MM/dd'), '')                AS RETURNDATETRAIN" _
            & " , ISNULL(RTRIM(OIT0003.JOINT), '')               AS JOINT" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')              AS DELFLG" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')             AS ORDERNO" _
            & " , ISNULL(RTRIM(OIT0003.DETAILNO), '')            AS DETAILNO" _
            & " , ISNULL(RTRIM(OIT0003.KAMOKU), '')              AS KAMOKU" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "       OIT0002.ORDERNO = OIT0003.ORDERNO" _
            & "       AND OIT0003.DELFLG <> @P2" _
            & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            & "       OIT0003.TANKNO = OIT0005.TANKNUMBER" _
            & "       AND OIT0005.DELFLG <> @P2" _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       OIT0003.TANKNO = OIM0005.TANKNUMBER" _
            & "       AND OIM0005.DELFLG <> @P2" _
            & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_NOW ON " _
            & "       OIT0002.OFFICECODE = OIM0003_NOW.OFFICECODE" _
            & "       AND OIT0002.SHIPPERSCODE = OIM0003_NOW.SHIPPERCODE" _
            & "       AND OIT0002.BASECODE = OIM0003_NOW.PLANTCODE" _
            & "       AND OIT0003.OILCODE = OIM0003_NOW.OILCODE" _
            & "       AND OIM0003_NOW.DELFLG <> @P2" _
            & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003_PAST ON " _
            & "       OIT0002.OFFICECODE = OIM0003_PAST.OFFICECODE" _
            & "       AND OIT0002.SHIPPERSCODE = OIM0003_PAST.SHIPPERCODE" _
            & "       AND OIT0002.BASECODE = OIM0003_PAST.PLANTCODE" _
            & "       AND OIT0005.LASTOILCODE = OIM0003_PAST.OILCODE" _
            & "       AND OIM0003_PAST.DELFLG <> @P2" _
            & " WHERE OIT0002.ORDERNO = @P1" _
            & " AND OIT0002.DELFLG <> @P2"

            SQLStr &=
                  " ORDER BY" _
                & "    OIT0002.ORDERYMD" _
                & "    , OIT0002.SHIPPERSCODE" _
                & "    , OIT0003.OILCODE" _
                & "    , OIT0003.TANKNO"
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)  '削除フラグ

                If work.WF_SEL_CREATEFLG.Text = 1 Then
                    For Each OIT0001WKrow As DataRow In OIT0001WKtbl.Rows
                        PARA1.Value = OIT0001WKrow("ORDERNO_NUM")
                        PARA2.Value = C_DELETE_FLG.ALIVE
                    Next
                ElseIf work.WF_SEL_CREATEFLG.Text = 2 Then
                    PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                    PARA2.Value = C_DELETE_FLG.DELETE
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    i += 1
                    OIT0001row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D Select"
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
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            If OIT0001row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0001row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0001tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

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
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then

                    '会社コード
                    Dim prmData As New Hashtable
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                    '運用部署
                    If WF_FIELD.Value = "WF_UORG" Then
                        prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    End If

                    '本社列車
                    If WF_FIELD.Value = "TxtHeadOfficeTrain" Then
                        '                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, TxtHeadOfficeTrain.Text + work.WF_SEL_UORG.Text)
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICE.Text, TxtHeadOfficeTrain.Text)
                    End If

                    '油種
                    If WF_FIELD.Value = "OILNAME" Then
                        '                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICE.Text, "")
                    End If

                    'タンク車№
                    If WF_FIELD.Value = "TANKNO" Then
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICE.Text, "")
                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '(予定)積込日
                        Case "TxtLoadingDate"
                            .WF_Calendar.Text = TxtLoadingDate.Text
                        '(予定)発日
                        Case "TxtDepDate"
                            .WF_Calendar.Text = TxtDepDate.Text
                        '(予定)積車着日
                        Case "TxtArrDate"
                            .WF_Calendar.Text = TxtArrDate.Text
                        '(予定)受入日
                        Case "TxtAccDate"
                            .WF_Calendar.Text = TxtAccDate.Text
                    End Select
                    .ActiveCalendar()

                End If
            End With

        End If
    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0001tbl.Rows(i)("OPERATION") = "on" Then
                    OIT0001tbl.Rows(i)("OPERATION") = ""
                Else
                    OIT0001tbl.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            '会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '運用部署
            Case "WF_UORG"
                CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
            '本線列車
            Case "TxtHeadOfficeTrain"

            '発駅
            Case "TxtDepstation"
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            '着駅
            Case "TxtArrstation"
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
        End If
    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_GetValue() As String = {"", "", "", "", ""}

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()

            Case "WF_UORG"              '運用部署
                WF_UORG.Text = WW_SelectValue
                WF_UORG_TEXT.Text = WW_SelectText
                WF_UORG.Focus()

            Case "TxtHeadOfficeTrain"   '本社列車
                '                TxtHeadOfficeTrain.Text = WW_SelectValue.Substring(0, 4)
                TxtHeadOfficeTrain.Text = WW_SelectValue
                FixvalueMasterSearch("", "TRAINNUMBER", WW_SelectValue, WW_GetValue)

                '発駅
                TxtDepstation.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtArrstation.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()

            Case "TxtDepstation"        '発駅
                TxtDepstation.Text = WW_SelectValue
                LblDepstationName.Text = WW_SelectText
                TxtDepstation.Focus()

            Case "TxtArrstation"        '着駅
                TxtArrstation.Text = WW_SelectValue
                LblArrstationName.Text = WW_SelectText
                TxtArrstation.Focus()

            Case "TxtLoadingDate"       '(予定)積込日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtLoadingDate.Text = ""
                    Else
                        TxtLoadingDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtLoadingDate.Focus()
            Case "TxtDepDate"           '(予定)発日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtDepDate.Text = ""
                    Else
                        TxtDepDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtDepDate.Focus()
            Case "TxtArrDate"           '(予定)積車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtArrDate.Text = ""
                    Else
                        TxtArrDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtArrDate.Focus()
            Case "TxtAccDate"           '(予定)受入日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtAccDate.Text = ""
                    Else
                        TxtAccDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtAccDate.Focus()

            Case "OILNAME", "TANKNO"   '(一覧)油種, (一覧)タンク車№
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(OIT0001tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0001tbl.AsEnumerable.
                            FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
                If IsNothing(updHeader) Then Exit Sub

                '〇 一覧項目へ設定
                '油種名を一覧に設定
                If WF_FIELD.Value = "OILNAME" Then
                    updHeader.Item("OILCODE") = WW_SETVALUE
                    updHeader.Item(WF_FIELD.Value) = WW_SETTEXT

                    'タンク車№を一覧に設定
                ElseIf WF_FIELD.Value = "TANKNO" Then
                    Dim WW_TANKNUMBER As String = WW_SETTEXT.Substring(0, 8).Replace("-", "")
                    updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER

                    FixvalueMasterSearch("", "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)

                    '前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                    CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                    updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                    '交検日
                    Dim WW_JRINSPECTIONCNT As String
                    updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                    If WW_GetValue(2) <> "" Then
                        WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Now(), Date.Parse(WW_GetValue(2)))

                        Select Case WW_JRINSPECTIONCNT
                            Case 0 To 3
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            Case 4 To 6
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            Case Else
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                        End Select
                    Else
                        updHeader.Item("JRINSPECTIONALERT") = ""
                    End If

                    '全検日
                    Dim WW_JRALLINSPECTIONCNT As String
                    updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                    If WW_GetValue(3) <> "" Then
                        WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Now(), Date.Parse(WW_GetValue(3)))

                        Select Case WW_JRALLINSPECTIONCNT
                            Case 0 To 3
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            Case 4 To 6
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            Case Else
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                        End Select
                    Else
                        updHeader.Item("JRALLINSPECTIONALERT") = ""
                    End If

                End If
                    'updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                    '○ 画面表示データ保存
                    If Not Master.SaveTable(OIT0001tbl) Then Exit Sub

        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Focus()
            Case "WF_UORG"              '運用部署
                WF_UORG.Focus()
            Case "TxtHeadOfficeTrain"   '本社列車
                TxtHeadOfficeTrain.Focus()
            Case "TxtDepstation"        '発駅
                TxtDepstation.Focus()
            Case "TxtArrstation"        '着駅
                TxtArrstation.Focus()
            Case "TxtLoadingDate"       '(予定)積込日
                TxtLoadingDate.Focus()
            Case "TxtDepDate"           '(予定)発日
                TxtDepDate.Focus()
            Case "TxtArrDate"           '(予定)積車着日
                TxtArrDate.Focus()
            Case "TxtAccDate"           '(予定)受入日
                TxtAccDate.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0001tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0001tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '■■■ OIT0001tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注明細を一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0003_DETAIL       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = '1'        " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DETAILNO    = @P02       " _
                    & "    AND TANKNO      = @P03       " _
                    & "    AND KAMOKU      = @P04       " _
                    & "    AND DELFLG     <> '1'       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
            Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0001UPDrow In OIT0001tbl.Rows
                If OIT0001UPDrow("OPERATION") = "on" Then
                    j += 1
                    OIT0001UPDrow("LINECNT") = j        'LINECNT
                    OIT0001UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0001UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0001UPDrow("ORDERNO")
                    PARA02.Value = OIT0001UPDrow("DETAILNO")
                    PARA03.Value = OIT0001UPDrow("TANKNO")
                    PARA04.Value = OIT0001UPDrow("KAMOKU")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Else
                    i += 1
                    OIT0001UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_ADD_Click()

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim SQLStrNum As String =
        " SELECT " _
            & "  MAX(OIT0003_1.ORDERNO)                                      AS ORDERNO" _
            & ", FORMAT(MAX(SUBSTRING(OIT0003_1.ORDERNO, 10, 2)) + 1, '00')  AS ORDERNO_NUM" _
            & ", FORMAT(MAX(ISNULL(OIT0003_2.DETAILNO, '000')) + 1, '000')   AS DETAILNO_NUM" _
            & " FROM (" _
            & "  SELECT ISNULL(MAX(OIT0003.ORDERNO),'O' + FORMAT(GETDATE(),'yyyyMMdd') + '00') AS ORDERNO" _
            & "  FROM   OIL.OIT0003_DETAIL OIT0003" _
            & "  WHERE  SUBSTRING(OIT0003.ORDERNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd')" _
            & " ) OIT0003_1 " _
            & " LEFT JOIN OIL.OIT0003_DETAIL OIT0003_2 ON" _
            & " OIT0003_1.ORDERNO = OIT0003_2.ORDERNO"

        '" SELECT " _
        '    & " ISNULL(FORMAT(MAX(SUBSTRING(OIT0002.ORDERNO, 10, 2)) + 1,'00'),'01') AS ORDERNO" _
        '    & " FROM OIL.OIT0002_ORDER OIT0002 " _
        '    & " WHERE SUBSTRING(OIT0002.ORDERNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd')"

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
        " SELECT TOP (1)" _
            & "   0                                              AS LINECNT" _
            & " , ''                                             AS OPERATION" _
            & " , '0'                                            AS TIMSTP" _
            & " , 1                                              AS 'SELECT'" _
            & " , 0                                              AS HIDDEN" _
            & " , FORMAT(GETDATE(),'yyyy/MM/dd')                 AS ORDERYMD" _
            & " , ''                                             AS SHIPPERSNAME" _
            & " , ''                                             AS OILCODE" _
            & " , ''                                             AS OILNAME" _
            & " , ''                                             AS TANKNO" _
            & " , ''                                             AS LASTOILCODE" _
            & " , ''                                             AS LASTOILNAME" _
            & " , ''                                             AS JRINSPECTIONALERT" _
            & " , ''                                             AS JRINSPECTIONDATE" _
            & " , ''                                             AS JRALLINSPECTIONALERT" _
            & " , ''                                             AS JRALLINSPECTIONDATE" _
            & " , ''                                             AS RETURNDATETRAIN" _
            & " , ''                                             AS JOINT" _
            & " , '0'                                            AS DELFLG" _
            & " , 'O' + FORMAT(GETDATE(),'yyyyMMdd') + @P01      AS ORDERNO" _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS DETAILNO" _
            & " , ''                                             AS KAMOKU" _
            & " FROM sys.all_objects "
        SQLStr &=
                  " ORDER BY" _
                & "    LINECNT"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                For Each OIT0001WKrow As DataRow In OIT0001WKtbl.Rows
                    PARA1.Value = OIT0001WKrow("ORDERNO_NUM")
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    ''○ フィールド名とフィールドの型を取得
                    'For index As Integer = 0 To SQLdr.FieldCount - 1
                    '    OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    'Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                Dim intDetailNo As Integer
                Dim strOrderNoBak As String = ""
                For Each OIT0001row As DataRow In OIT0001tbl.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If OIT0001row("LINECNT") = 0 Then
                        If strOrderNoBak <> "" Then
                            OIT0001row("ORDERNO") = strOrderNoBak
                            intDetailNo += 1
                            OIT0001row("DETAILNO") = intDetailNo.ToString("000")
                        End If
                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If OIT0001row("HIDDEN") = 1 Then
                        j += 1
                        OIT0001row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        OIT0001row("LINECNT") = i        'LINECNT
                    End If
                    strOrderNoBak = OIT0001row("ORDERNO")
                    intDetailNo = OIT0001row("DETAILNO")
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = OIT0001tbl                       'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

    ''' <summary>
    ''' 明細更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonUPDATE_Click()

        ''○ エラーレポート準備
        'rightview.SetErrorReport("")

        'Dim WW_RESULT As String = ""

        ''○関連チェック
        'RelatedCheck(WW_ERRCODE)

        ''○ 同一レコードチェック
        'If isNormal(WW_ERRCODE) Then
        '    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '        SQLcon.Open()       'DataBase接続

        '        'マスタ更新
        '        UpdateMaster(SQLcon)
        '    End Using
        'End If

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0001tbl)

        ''○ GridView初期設定
        ''○ 画面表示データ再取得
        'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()       'DataBase接続

        '    MAPDataGet(SQLcon)
        'End Using

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0001tbl)

        ''○ 詳細画面クリア
        'If isNormal(WW_ERRCODE) Then
        '    DetailBoxClear()
        'End If

        ''○ メッセージ表示
        'If Not isNormal(WW_ERRCODE) Then
        '    Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        'End If

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

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

            rightview.SelectIndex(WF_RightViewChange.Value)
            WF_RightViewChange.Value = ""
        End If

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub FixvalueMasterSearch(ByVal I_CODE As String, ByVal I_CLASS As String, ByVal I_KEYCODE As String, ByRef O_VALUE() As String)

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
               " SELECT" _
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '   ') AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '   ')    AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '   ')  AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '   ')    AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '   ')   AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '   ')   AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '   ')   AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '   ')   AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '   ')   AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '   ')   AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '   ')   AS DELFLG" _
                & " FROM  OIL.VIW0001_FIXVALUE VIW0001" _
                & " WHERE VIW0001.CLASS = @P01" _
                & " AND VIW0001.KEYCODE = @P02" _
                & " AND VIW0001.DELFLG <> @P03"

            SQLStr &=
                  " ORDER BY" _
                & "    VIW0001.KEYCODE"

            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)

                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)

                PARA01.Value = I_CLASS
                PARA02.Value = I_KEYCODE
                PARA03.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0001WKrow As DataRow In OIT0001WKtbl.Rows
                    O_VALUE(0) = OIT0001WKrow("VALUE1")
                    O_VALUE(1) = OIT0001WKrow("VALUE2")
                    O_VALUE(2) = OIT0001WKrow("VALUE3")
                    O_VALUE(3) = OIT0001WKrow("VALUE4")
                    O_VALUE(4) = OIT0001WKrow("VALUE5")
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 登録データ関連チェック
    ''' </summary>
    ''' <param name="O_RTNCODE"></param>
    ''' <remarks></remarks>
    Protected Sub RelatedCheck(ByRef O_RTNCODE As String)

        ''○初期値設定
        'O_RTNCODE = C_MESSAGE_NO.NORMAL

        'Dim WW_LINEERR_SW As String = ""
        'Dim WW_DUMMY As String = ""
        'Dim WW_CheckMES1 As String = ""
        'Dim WW_CheckMES2 As String = ""

        ''○同一レコードチェック
        ''※開始終了期間を持っていないため現状意味無し
        ''For Each OIM000１row As DataRow In OIT0001tbl.Rows
        ''    '読み飛ばし
        ''    If OIT0001row("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING OrElse
        ''        OIT0001row("DELFLG") = C_DELETE_FLG.DELETE Then
        ''        Continue For
        ''    End If

        ''    WW_LINEERR_SW = ""

        ''    '期間重複チェック
        ''    For Each checkRow As DataRow In OIT0001tbl.Rows
        ''        '同一KEY以外は読み飛ばし
        ''        If checkRow("CAMPCODE") = OIT0001row("CAMPCODE") AndAlso
        ''            checkRow("UORG") = OIT0001row("UORG") AndAlso
        ''            checkRow("MODELPATTERN") = OIT0001row("MODELPATTERN") AndAlso
        ''            checkRow("TORICODES") = OIT0001row("TORICODES") AndAlso
        ''            checkRow("SHUKABASHO") = OIT0001row("SHUKABASHO") AndAlso
        ''            checkRow("TORICODET") = OIT0001row("TORICODET") AndAlso
        ''            checkRow("TODOKECODE") = OIT0001row("TODOKECODE") Then
        ''        Else
        ''            Continue For
        ''        End If
        ''    Next

        ''    If WW_LINEERR_SW = "" Then
        ''        If OIT0001row("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
        ''            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        ''        End If
        ''    Else
        ''        OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        ''    End If
        ''Next

    End Sub

    ''' <summary>
    ''' 受注・受注明細登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        ''○ ＤＢ更新
        'Dim SQLStr As String =
        '      " DECLARE @hensuu AS bigint ;" _
        '    & "    SET @hensuu = 0 ;" _
        '    & " DECLARE hensuu CURSOR FOR" _
        '    & "    SELECT" _
        '    & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
        '    & "    FROM" _
        '    & "        OIL.OIT0001_STATION" _
        '    & "    WHERE" _
        '    & "        STATIONCODE      = @P1" _
        '    & "        AND BRANCH       = @P2 ;" _
        '    & " OPEN hensuu ;" _
        '    & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
        '    & " IF (@@FETCH_STATUS = 0)" _
        '    & "    UPDATE OIL.OIT0001_STATION" _
        '    & "    SET" _
        '    & "        STATONNAME   = @P3     , STATIONNAMEKANA = @P4" _
        '    & "        , TYPENAME   = @P5     , TYPENAMEKANA = @P6" _
        '    & "        , DELFLG     = @P7" _
        '    & "        , INITYMD    = @P8     , INITUSER   = @P9" _
        '    & "        , INITTERMID = @P10    , UPDYMD    = @P11" _
        '    & "        , UPDUSER    = @P12    , UPDTERMID = @P13" _
        '    & "        , RECEIVEYMD = @P14" _
        '    & "    WHERE" _
        '    & "        STATIONCODE       = @P1" _
        '    & "        AND BRANCH       = @P2 ;" _
        '    & " IF (@@FETCH_STATUS <> 0)" _
        '    & "    INSERT INTO OIL.OIT0001_STATION" _
        '    & "        ( STATIONCODE , BRANCH" _
        '    & "        , STATONNAME , STATIONNAMEKANA" _
        '    & "        , TYPENAME   , TYPENAMEKANA  , DELFLG" _
        '    & "        , INITYMD    , INITUSER      , INITTERMID" _
        '    & "        , UPDYMD     , UPDUSER       , UPDTERMID" _
        '    & "        , RECEIVEYMD)" _
        '    & "    VALUES" _
        '    & "        ( @P1  , @P2" _
        '    & "        , @P3  , @P4" _
        '    & "        , @P5  , @P6 , @P7" _
        '    & "        , @P8  , @P9 , @P10" _
        '    & "        , @P11 , @P12, @P13" _
        '    & "        , @P14) ;" _
        '    & " CLOSE hensuu ;" _
        '    & " DEALLOCATE hensuu ;"

        ''○ 更新ジャーナル出力
        'Dim SQLJnl As String =
        '      " SELECT" _
        '    & "    STATIONCODE" _
        '    & "    , BRANCH" _
        '    & "    , STATONNAME" _
        '    & "    , STATIONNAMEKANA" _
        '    & "    , TYPENAME" _
        '    & "    , TYPENAMEKANA" _
        '    & "    , DELFLG" _
        '    & "    , INITYMD" _
        '    & "    , INITUSER" _
        '    & "    , INITTERMID" _
        '    & "    , UPDYMD" _
        '    & "    , UPDUSER" _
        '    & "    , UPDTERMID" _
        '    & "    , RECEIVEYMD" _
        '    & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
        '    & " FROM" _
        '    & "    OIL.OIT0001_STATION" _
        '    & " WHERE" _
        '    & "        STATIONCODE      = @P1" _
        '    & "        AND BRANCH       = @P2"

        'Try
        '    Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
        '        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)            '貨物駅コード
        '        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 3)            '貨物コード枝番
        '        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 200)          '貨物駅名称
        '        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 100)          '貨物駅名称カナ
        '        Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 40)           '貨物駅種別名称
        '        Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20)           '貨物駅種別名称カナ
        '        Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 1)            '削除フラグ
        '        Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.DateTime)               '登録年月日
        '        Dim PARA9 As SqlParameter = SQLcmd.Parameters.Add("@P9", SqlDbType.NVarChar, 20)           '登録ユーザーID
        '        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)         '登録端末
        '        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)             '更新年月日
        '        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)         '更新ユーザーID
        '        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 20)         '更新端末
        '        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)             '集信日時

        '        Dim JPARA1 As SqlParameter = SQLcmdJnl.Parameters.Add("@P1", SqlDbType.NVarChar, 4)        '貨物駅コード
        '        Dim JPARA2 As SqlParameter = SQLcmdJnl.Parameters.Add("@P2", SqlDbType.NVarChar, 3)        '貨物コード枝番

        '        For Each OIT0001row As DataRow In OIT0001tbl.Rows
        '            If Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
        '                Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
        '                Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
        '                '                        Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING Then
        '                Dim WW_DATENOW As DateTime = Date.Now

        '                'DB更新
        '                PARA1.Value = OIT0001row("STATIONCODE")
        '                PARA2.Value = OIT0001row("BRANCH")
        '                PARA3.Value = OIT0001row("STATONNAME")
        '                PARA4.Value = OIT0001row("STATIONNAMEKANA")
        '                PARA5.Value = OIT0001row("TYPENAME")
        '                PARA6.Value = OIT0001row("TYPENAMEKANA")
        '                PARA7.Value = OIT0001row("DELFLG")
        '                PARA8.Value = WW_DATENOW
        '                PARA9.Value = Master.USERID
        '                PARA10.Value = Master.USERTERMID
        '                PARA11.Value = WW_DATENOW
        '                PARA12.Value = Master.USERID
        '                PARA13.Value = Master.USERTERMID
        '                PARA14.Value = C_DEFAULT_YMD

        '                SQLcmd.CommandTimeout = 300
        '                SQLcmd.ExecuteNonQuery()

        '                OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

        '                '更新ジャーナル出力
        '                JPARA1.Value = OIT0001row("STATIONCODE")
        '                JPARA2.Value = OIT0001row("BRANCH")

        '                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
        '                    If IsNothing(OIT0001UPDtbl) Then
        '                        OIT0001UPDtbl = New DataTable

        '                        For index As Integer = 0 To SQLdr.FieldCount - 1
        '                            OIT0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
        '                        Next
        '                    End If

        '                    OIT0001UPDtbl.Clear()
        '                    OIT0001UPDtbl.Load(SQLdr)
        '                End Using

        '                For Each OIT0001UPDrow As DataRow In OIT0001UPDtbl.Rows
        '                    CS0020JOURNAL.TABLENM = "OIT0001L"
        '                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
        '                    CS0020JOURNAL.ROW = OIT0001UPDrow
        '                    CS0020JOURNAL.CS0020JOURNAL()
        '                    If Not isNormal(CS0020JOURNAL.ERR) Then
        '                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

        '                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
        '                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
        '                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
        '                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
        '                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
        '                        CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
        '                        Exit Sub
        '                    End If
        '                Next
        '            End If
        '        Next
        '    End Using
        'Catch ex As Exception
        '    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D UPDATE_INSERT")

        '    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
        '    CS0011LOGWrite.INFPOSI = "DB:OIT0001D UPDATE_INSERT"
        '    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
        '    CS0011LOGWrite.TEXT = ex.ToString()
        '    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
        '    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
        '    Exit Sub
        'End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        ''○ 状態をクリア
        'For Each OIT0001row As DataRow In OIT0001tbl.Rows
        '    Select Case OIT0001row("OPERATION")
        '        Case C_LIST_OPERATION_CODE.NODATA
        '            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '            WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

        '        Case C_LIST_OPERATION_CODE.NODISP
        '            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        '            WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

        '        Case C_LIST_OPERATION_CODE.SELECTED
        '            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        '            WW_ERR_SW = C_MESSAGE_NO.NORMAL

        '        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
        '            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        '            WW_ERR_SW = C_MESSAGE_NO.NORMAL

        '        Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        '            OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
        '            WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        '    End Select
        'Next

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0001tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT
        'TxtStationCode.Text = ""            '貨物駅コード
        'TxtBranch.Text = ""                 '貨物コード枝番
        'TxtStationName.Text = ""            '貨物駅名称
        'TxtStationNameKana.Text = ""        '貨物駅名称カナ
        'TxtTypeName.Text = ""               '貨物駅種別名称
        'TxtTypeNameKana.Text = ""           '貨物駅種別名称カナ
        'WF_DELFLG.Text = ""                 '削除
        'WF_DELFLG_TEXT.Text = ""            '削除名称

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

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                Case "SALESOFFICE"       '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

                Case "PRODUCTPATTERN"   '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_UORG.Text, "PRODUCTPATTERN"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
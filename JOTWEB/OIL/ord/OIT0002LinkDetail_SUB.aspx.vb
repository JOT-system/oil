﻿'Option Strict On
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
                        Case "WF_ListChange"            'リスト変更
                            WF_ListChange()
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
        work.WF_SEL_SALESOFFICE.Text = TxtOrderOffice.Text
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
            & " , '0'                                            AS DELFLG" _
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
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN ''" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4" _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>'" _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>'" _
            & "   END                                                                      AS JRINSPECTIONALERT" _
            & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), '')               AS JRINSPECTIONDATE" _
            & " , CASE" _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN ''" _
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
                & "    , OIT0003.DETAILNO" _
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
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, TxtHeadOfficeTrain.Text)
                    End If

                    '油種
                    If WF_FIELD.Value = "OILNAME" Then
                        '                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
                    End If

                    'タンク車№
                    If WF_FIELD.Value = "TANKNO" Then
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_SALESOFFICECODE.Text, "")
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
                        '返送日列車
                        Case "RETURNDATETRAIN"
                            .WF_Calendar.Text = Date.Now.ToString("yyyy/MM/dd")
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
            '本社列車
            Case "TxtHeadOfficeTrain"
                Dim WW_GetValue() As String = {"", "", "", "", ""}
                FixvalueMasterSearch("", "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)

                '発駅
                TxtDepstation.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtArrstation.Text = WW_GetValue(2)
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()
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

            Case "TxtOrderOffice"      '受注営業所
                '別の受注営業所が設定されて場合
                If TxtOrderOffice.Text <> WW_SelectText Then
                    TxtOrderOffice.Text = WW_SelectText
                    work.WF_SEL_SALESOFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_SALESOFFICE.Text = WW_SelectText

                    '本社列車, 発駅, 着駅のテキストボックスを初期化
                    TxtHeadOfficeTrain.Text = ""
                    TxtDepstation.Text = ""
                    LblDepstationName.Text = ""
                    TxtArrstation.Text = ""
                    LblArrstationName.Text = ""
                End If
                TxtOrderOffice.Focus()

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

            Case "OILNAME", "TANKNO", "RETURNDATETRAIN"   '(一覧)油種, (一覧)タンク車№, (一覧)返送日列車
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
                    'Dim WW_TANKNUMBER As String = WW_SETTEXT.Substring(0, 8).Replace("-", "")
                    Dim WW_TANKNUMBER As String = WW_SETVALUE
                    Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
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
                        WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(2)))

                        Dim WW_JRINSPECTIONFLG As String
                        If WW_JRINSPECTIONCNT <= 3 Then
                            WW_JRINSPECTIONFLG = "1"
                        ElseIf WW_JRINSPECTIONCNT >= 4 And WW_JRINSPECTIONCNT <= 6 Then
                            WW_JRINSPECTIONFLG = "2"
                        Else
                            WW_JRINSPECTIONFLG = "3"
                        End If
                        Select Case WW_JRINSPECTIONFLG
                            Case "1"
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            Case "2"
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            Case "3"
                                updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                        End Select
                    Else
                        updHeader.Item("JRINSPECTIONALERT") = ""
                    End If

                    '全検日
                    Dim WW_JRALLINSPECTIONCNT As String
                    updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                    If WW_GetValue(3) <> "" Then
                        WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(3)))

                        Dim WW_JRALLINSPECTIONFLG As String
                        If WW_JRALLINSPECTIONCNT <= 3 Then
                            WW_JRALLINSPECTIONFLG = "1"
                        ElseIf WW_JRALLINSPECTIONCNT >= 4 And WW_JRALLINSPECTIONCNT <= 6 Then
                            WW_JRALLINSPECTIONFLG = "2"
                        Else
                            WW_JRALLINSPECTIONFLG = "3"
                        End If
                        Select Case WW_JRALLINSPECTIONFLG
                            Case "1"
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            Case "2"
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            Case "3"
                                updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                        End Select
                    Else
                        updHeader.Item("JRALLINSPECTIONALERT") = ""
                    End If

                    '返送日列車を一覧に設定
                ElseIf WF_FIELD.Value = "RETURNDATETRAIN" Then
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            updHeader.Item(WF_FIELD.Value) = ""
                        Else
                            updHeader.Item(WF_FIELD.Value) = leftview.WF_Calendar.Text
                        End If
                    Catch ex As Exception
                    End Try

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

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        Dim WW_RESULT As String = ""

        '○関連チェック
        WW_Check(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            '受注DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrder(SQLcon)
            End Using

            '受注明細DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrderDetail(SQLcon)
            End Using

            '(受注TBL)タンク車数更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_UpdateOrderTankCnt(SQLcon)
            End Using

            '空回日報(一覧)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_OrderListTBLSet(SQLcon)
            End Using

            '空回日報(明細)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                work.WF_SEL_CREATEFLG.Text = 2
                MAPDataGet(SQLcon)
            End Using

        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

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
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange()

        '○ LINECNT取得
        Dim WW_LINECNT As Integer = 0
        If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

        '○ 対象ヘッダー取得
        Dim updHeader = OIT0001tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "OILNAME"           '(一覧)油種
                FixvalueMasterSearch(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN_N", WW_ListValue, WW_GetValue)
                updHeader.Item("OILCODE") = WW_GetValue(0)
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "TANKNO"            '(一覧)タンク車№
                FixvalueMasterSearch("", "TANKNUMBER", WW_ListValue, WW_GetValue)

                'タンク車№
                updHeader.Item("TANKNO") = WW_ListValue

                '前回油種
                Dim WW_LASTOILNAME As String = ""
                updHeader.Item("LASTOILCODE") = WW_GetValue(1)
                CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                updHeader.Item("LASTOILNAME") = WW_LASTOILNAME

                '交検日
                Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                Dim WW_JRINSPECTIONCNT As String
                updHeader.Item("JRINSPECTIONDATE") = WW_GetValue(2)
                If WW_GetValue(2) <> "" Then
                    WW_JRINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(2)))

                    Dim WW_JRINSPECTIONFLG As String
                    If WW_JRINSPECTIONCNT <= 3 Then
                        WW_JRINSPECTIONFLG = "1"
                    ElseIf WW_JRINSPECTIONCNT >= 4 And WW_JRINSPECTIONCNT <= 6 Then
                        WW_JRINSPECTIONFLG = "2"
                    Else
                        WW_JRINSPECTIONFLG = "3"
                    End If
                    Select Case WW_JRINSPECTIONFLG
                        Case "1"
                            updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                        Case "2"
                            updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                        Case "3"
                            updHeader.Item("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                    End Select
                Else
                    updHeader.Item("JRINSPECTIONALERT") = ""
                End If

                '全検日
                Dim WW_JRALLINSPECTIONCNT As String
                updHeader.Item("JRALLINSPECTIONDATE") = WW_GetValue(3)
                If WW_GetValue(3) <> "" Then
                    WW_JRALLINSPECTIONCNT = DateDiff(DateInterval.Day, Date.Parse(WW_Now), Date.Parse(WW_GetValue(3)))

                    Dim WW_JRALLINSPECTIONFLG As String
                    If WW_JRALLINSPECTIONCNT <= 3 Then
                        WW_JRALLINSPECTIONFLG = "1"
                    ElseIf WW_JRALLINSPECTIONCNT >= 4 And WW_JRALLINSPECTIONCNT <= 6 Then
                        WW_JRALLINSPECTIONFLG = "2"
                    Else
                        WW_JRALLINSPECTIONFLG = "3"
                    End If
                    Select Case WW_JRALLINSPECTIONFLG
                        Case "1"
                            updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                        Case "2"
                            updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                        Case "3"
                            updHeader.Item("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                    End Select
                Else
                    updHeader.Item("JRALLINSPECTIONALERT") = ""
                End If

            Case "RETURNDATETRAIN"   '(一覧)返送日列車
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

            Case "JOINT"             '(一覧)ジョイント
                updHeader.Item(WF_FIELD.Value) = WW_ListValue

        End Select

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '受注営業所
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", work.WF_SEL_SALESOFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", work.WF_SEL_SALESOFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "受注営業所 : " & work.WF_SEL_SALESOFFICECODE.Text)
                TxtOrderOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtOrderOffice.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '本線列車
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", TxtHeadOfficeTrain.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtHeadOfficeTrain.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '発駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", TxtDepstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "発駅 : " & TxtDepstation.Text)
                TxtDepstation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtDepstation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '着駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRSTATION", TxtArrstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "着駅 : " & TxtArrstation.Text)
                TxtArrstation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtArrstation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積込日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LODDATE", TxtLoadingDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtLoadingDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtLoadingDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)発日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPDATE", TxtDepDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtDepDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtDepDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)積車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ARRDATE", TxtArrDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtArrDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtArrDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)受入日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ACCDATE", TxtAccDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtAccDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtAccDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '(一覧)タンク車No
        Dim chkTankNo As String
        For Each OIT0001row As DataRow In OIT0001tbl.Rows
            chkTankNo = OIT0001row("TANKNO")
            If chkTankNo = "" Then
                Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR)
                O_RTN = "ERR"
                Exit Sub
            End If
        Next

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

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

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '貨物コード枝番
            If Not String.IsNullOrEmpty(I_CODE) Then
                SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
            End If

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
    ''' 受注TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrder(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0002_ORDER" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0002_ORDER" _
            & "    SET" _
            & "        OFFICECODE   = @P04    , OFFICENAME     = @P05" _
            & "        , TRAINNO    = @P02" _
            & "        , DEPSTATION = @P13    , DEPSTATIONNAME = @P14" _
            & "        , ARRSTATION = @P15    , ARRSTATIONNAME = @P16" _
            & "        , LODDATE    = @P24    , DEPDATE        = @P25" _
            & "        , ARRDATE    = @P26    , ACCDATE        = @P27" _
            & "        , UPDYMD     = @P87    , UPDUSER        = @P88" _
            & "        , UPDTERMID  = @P89    , RECEIVEYMD     = @P90" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0002_ORDER" _
            & "        ( ORDERNO      , TRAINNO         , ORDERYMD       , OFFICECODE          , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME   , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION     , DEPSTATIONNAME      , ARRSTATION , ARRSTATIONNAME" _
            & "        , RETSTATION   , RETSTATIONNAME  , CANGERETSTATION, CHANGEARRSTATIONNAME, ORDERSTATUS" _
            & "        , ORDERINFO    , USEPROPRIETYFLG , LODDATE        , DEPDATE             , ARRDATE" _
            & "        , ACCDATE      , EMPARRDATE      , ACTUALLODDATE  , ACTUALDEPDATE       , ACTUALARRDATE" _
            & "        , ACTUALACCDATE, ACTUALEMPARRDATE, RTANK          , HTANK               , TTANK" _
            & "        , MTTANK       , KTANK           , K3TANK         , K5TANK              , K10TANK" _
            & "        , LTANK        , ATANK           , OTHER1OTANK    , OTHER2OTANK         , OTHER3OTANK" _
            & "        , OTHER4OTANK  , OTHER5OTANK     , OTHER6OTANK    , OTHER7OTANK         , OTHER8OTANK" _
            & "        , OTHER9OTANK  , OTHER10OTANK    , TOTALTANK" _
            & "        , RTANKCH      , HTANKCH         , TTANKCH        , MTTANKCH            , KTANKCH" _
            & "        , K3TANKCH     , K5TANKCH        , K10TANKCH      , LTANKCH             , ATANKCH" _
            & "        , OTHER1OTANKCH, OTHER2OTANKCH   , OTHER3OTANKCH  , OTHER4OTANKCH       , OTHER5OTANKCH" _
            & "        , OTHER6OTANKCH, OTHER7OTANKCH   , OTHER8OTANKCH  , OTHER9OTANKCH       , OTHER10OTANKCH" _
            & "        , TOTALTANKCH" _
            & "        , TANKRINKNO   , SALSE           , SALSETAX       , TOTALSALSE          , PAYMENT" _
            & "        , PAYMENTTAX   , TOTALPAYMENT    , DELFLG" _
            & "        , INITYMD      , INITUSER        , INITTERMID" _
            & "        , UPDYMD       , UPDUSER         , UPDTERMID      , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15, @P16" _
            & "        , @P17, @P18, @P19, @P20, @P21" _
            & "        , @P22, @P23, @P24, @P25, @P26" _
            & "        , @P27, @P28, @P29, @P30, @P31" _
            & "        , @P32, @P33, @P34, @P35, @P36" _
            & "        , @P37, @P38, @P39, @P40, @P41" _
            & "        , @P42, @P43, @P44, @P45, @P46" _
            & "        , @P47, @P48, @P49, @P50, @P51" _
            & "        , @P52, @P53, @P54" _
            & "        , @P55, @P56, @P57, @P58, @P59" _
            & "        , @P60, @P61, @P62, @P63, @P64" _
            & "        , @P65, @P66, @P67, @P68, @P69" _
            & "        , @P70, @P71, @P72, @P73, @P74" _
            & "        , @P75" _
            & "        , @P76, @P77, @P78, @P79, @P80" _
            & "        , @P81, @P82, @P83" _
            & "        , @P84, @P85, @P86" _
            & "        , @P87, @P88, @P89, @P90) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , TRAINNO" _
            & "    , ORDERYMD" _
            & "    , OFFICECODE" _
            & "    , OFFICENAME" _
            & "    , ORDERTYPE" _
            & "    , SHIPPERSCODE" _
            & "    , SHIPPERSNAME" _
            & "    , BASECODE" _
            & "    , BASENAME" _
            & "    , CONSIGNEECODE" _
            & "    , CONSIGNEENAME" _
            & "    , DEPSTATION" _
            & "    , DEPSTATIONNAME" _
            & "    , ARRSTATION" _
            & "    , ARRSTATIONNAME" _
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , CANGERETSTATION" _
            & "    , CHANGEARRSTATIONNAME" _
            & "    , ORDERSTATUS" _
            & "    , ORDERINFO" _
            & "    , USEPROPRIETYFLG" _
            & "    , LODDATE" _
            & "    , DEPDATE" _
            & "    , ARRDATE" _
            & "    , ACCDATE" _
            & "    , EMPARRDATE" _
            & "    , ACTUALLODDATE" _
            & "    , ACTUALDEPDATE" _
            & "    , ACTUALARRDATE" _
            & "    , ACTUALACCDATE" _
            & "    , ACTUALEMPARRDATE" _
            & "    , RTANK" _
            & "    , HTANK" _
            & "    , TTANK" _
            & "    , MTTANK" _
            & "    , KTANK" _
            & "    , K3TANK" _
            & "    , K5TANK" _
            & "    , K10TANK" _
            & "    , LTANK" _
            & "    , ATANK" _
            & "    , OTHER1OTANK" _
            & "    , OTHER2OTANK" _
            & "    , OTHER3OTANK" _
            & "    , OTHER4OTANK" _
            & "    , OTHER5OTANK" _
            & "    , OTHER6OTANK" _
            & "    , OTHER7OTANK" _
            & "    , OTHER8OTANK" _
            & "    , OTHER9OTANK" _
            & "    , OTHER10OTANK" _
            & "    , TOTALTANK" _
            & "    , RTANKCH" _
            & "    , HTANKCH" _
            & "    , TTANKCH" _
            & "    , MTTANKCH" _
            & "    , KTANKCH" _
            & "    , K3TANKCH" _
            & "    , K5TANKCH" _
            & "    , K10TANKCH" _
            & "    , LTANKCH" _
            & "    , ATANKCH" _
            & "    , OTHER1OTANKCH" _
            & "    , OTHER2OTANKCH" _
            & "    , OTHER3OTANKCH" _
            & "    , OTHER4OTANKCH" _
            & "    , OTHER5OTANKCH" _
            & "    , OTHER6OTANKCH" _
            & "    , OTHER7OTANKCH" _
            & "    , OTHER8OTANKCH" _
            & "    , OTHER9OTANKCH" _
            & "    , OTHER10OTANKCH" _
            & "    , TOTALTANKCH" _
            & "    , TANKRINKNO" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIT0002_ORDER" _
            & " WHERE" _
            & "        ORDERNO      = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '受注登録日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '受注営業所名
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 3)  '受注パターン
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 9)  '荷主コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 40) '荷主名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 9)  '基地コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40) '基地名
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 9)  '荷受人コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40) '荷受人名
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 7)  '発駅コード
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40) '発駅名
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 7)  '着駅コード
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 40) '着駅名
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 7)  '空車着駅コード
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 40) '空車着駅名
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 7)  '空車着駅コード(変更後)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40) '空車着駅名(変更後)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 2)  '受注情報
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 1)  '利用可否フラグ
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.Date)         '積込日（予定）
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.Date)         '発日（予定）
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Date)         '積車着日（予定）
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.Date)         '受入日（予定）
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Date)         '空車着日（予定）
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.Date)         '積込日（実績）
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.Date)         '発日（実績）
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.Date)         '積車着日（実績）
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.Date)         '受入日（実績）
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.Date)         '空車着日（実績）
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.Int)          '車数（レギュラー）
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.Int)          '車数（ハイオク）
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.Int)          '車数（灯油）
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.Int)          '車数（未添加灯油）
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.Int)          '車数（軽油）
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.Int)          '車数（３号軽油）
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.Int)          '車数（５号軽油）
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.Int)          '車数（１０号軽油）
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.Int)          '車数（LSA）
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.Int)          '車数（A重油）
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.Int)          '車数（その他１）
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.Int)          '車数（その他２）
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.Int)          '車数（その他３）
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Int)          '車数（その他４）
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.Int)          '車数（その他５）
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Int)          '車数（その他６）
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.Int)          '車数（その他７）
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.Int)          '車数（その他８）
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", SqlDbType.Int)          '車数（その他９）
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", SqlDbType.Int)          '車数（その他１０）
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", SqlDbType.Int)          '合計車数
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", SqlDbType.Int)          '変更後_車数（レギュラー）
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", SqlDbType.Int)          '変更後_車数（ハイオク）
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", SqlDbType.Int)          '変更後_車数（灯油）
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.Int)          '変更後_車数（未添加灯油）
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.Int)          '変更後_車数（軽油）
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.Int)          '変更後_車数（３号軽油）
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.Int)          '変更後_車数（５号軽油）
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.Int)          '変更後_車数（１０号軽油）
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.Int)          '変更後_車数（LSA）
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.Int)          '変更後_車数（A重油）
                Dim PARA65 As SqlParameter = SQLcmd.Parameters.Add("@P65", SqlDbType.Int)          '変更後_車数（その他１）
                Dim PARA66 As SqlParameter = SQLcmd.Parameters.Add("@P66", SqlDbType.Int)          '変更後_車数（その他２）
                Dim PARA67 As SqlParameter = SQLcmd.Parameters.Add("@P67", SqlDbType.Int)          '変更後_車数（その他３）
                Dim PARA68 As SqlParameter = SQLcmd.Parameters.Add("@P68", SqlDbType.Int)          '変更後_車数（その他４）
                Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", SqlDbType.Int)          '変更後_車数（その他５）
                Dim PARA70 As SqlParameter = SQLcmd.Parameters.Add("@P70", SqlDbType.Int)          '変更後_車数（その他６）
                Dim PARA71 As SqlParameter = SQLcmd.Parameters.Add("@P71", SqlDbType.Int)          '変更後_車数（その他７）
                Dim PARA72 As SqlParameter = SQLcmd.Parameters.Add("@P72", SqlDbType.Int)          '変更後_車数（その他８）
                Dim PARA73 As SqlParameter = SQLcmd.Parameters.Add("@P73", SqlDbType.Int)          '変更後_車数（その他９）
                Dim PARA74 As SqlParameter = SQLcmd.Parameters.Add("@P74", SqlDbType.Int)          '変更後_車数（その他１０）
                Dim PARA75 As SqlParameter = SQLcmd.Parameters.Add("@P75", SqlDbType.Int)          '変更後_合計車数
                Dim PARA76 As SqlParameter = SQLcmd.Parameters.Add("@P76", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA77 As SqlParameter = SQLcmd.Parameters.Add("@P77", SqlDbType.Int)          '売上金額
                Dim PARA78 As SqlParameter = SQLcmd.Parameters.Add("@P78", SqlDbType.Int)          '売上消費税額
                Dim PARA79 As SqlParameter = SQLcmd.Parameters.Add("@P79", SqlDbType.Int)          '売上合計金額
                Dim PARA80 As SqlParameter = SQLcmd.Parameters.Add("@P80", SqlDbType.Int)          '支払金額
                Dim PARA81 As SqlParameter = SQLcmd.Parameters.Add("@P81", SqlDbType.Int)          '支払消費税額
                Dim PARA82 As SqlParameter = SQLcmd.Parameters.Add("@P82", SqlDbType.Int)          '支払合計金額
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.DateTime)     '登録年月日
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.NVarChar, 20) '登録ユーザーID
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.NVarChar, 20) '登録端末
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.DateTime)     '更新年月日
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.NVarChar, 20) '更新ユーザーID
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.NVarChar, 20) '更新端末
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.DateTime)     '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4) '受注№

                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    'If Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№
                    PARA01.Value = OIT0001row("ORDERNO")              '受注№
                    PARA02.Value = TxtHeadOfficeTrain.Text            '本線列車
                    PARA03.Value = OIT0001row("ORDERYMD")             '受注登録日
                    PARA04.Value = work.WF_SEL_SALESOFFICECODE.Text   '受注営業所コード
                    PARA05.Value = work.WF_SEL_SALESOFFICE.Text       '受注営業所名
                    PARA06.Value = ""       '受注パターン
                    PARA07.Value = ""       '荷主コード
                    PARA08.Value = ""       '荷主名
                    PARA09.Value = ""       '基地コード
                    PARA10.Value = ""       '基地名
                    PARA11.Value = ""       '荷受人コード
                    PARA12.Value = ""       '荷受人名
                    PARA13.Value = TxtDepstation.Text                 '発駅コード
                    PARA14.Value = LblDepstationName.Text             '発駅名
                    PARA15.Value = TxtArrstation.Text                 '着駅コード
                    PARA16.Value = LblArrstationName.Text             '着駅名
                    PARA17.Value = ""       '空車着駅コード
                    PARA18.Value = ""       '空車着駅名
                    PARA19.Value = ""       '空車着駅コード(変更後)
                    PARA20.Value = ""       '空車着駅名(変更後)
                    PARA21.Value = "100"                              '受注進行ステータス(100:受注受付)
                    PARA22.Value = ""       '受注情報
                    PARA23.Value = "0"                                '利用可否フラグ(0:利用可能)
                    PARA24.Value = TxtLoadingDate.Text                '積込日（予定）
                    PARA25.Value = TxtDepDate.Text                    '発日（予定）
                    PARA26.Value = TxtArrDate.Text                    '積車着日（予定）
                    PARA27.Value = TxtAccDate.Text                    '受入日（予定）
                    PARA28.Value = DBNull.Value                       '空車着日（予定）
                    PARA29.Value = DBNull.Value                       '積込日（実績）
                    PARA30.Value = DBNull.Value                       '発日（実績）
                    PARA31.Value = DBNull.Value                       '積車着日（実績）
                    PARA32.Value = DBNull.Value                       '受入日（実績）
                    PARA33.Value = DBNull.Value                       '空車着日（実績）
                    PARA34.Value = "0"                                '車数（レギュラー）
                    PARA35.Value = "0"                                '車数（ハイオク）
                    PARA36.Value = "0"                                '車数（灯油）
                    PARA37.Value = "0"                                '車数（未添加灯油）
                    PARA38.Value = "0"                                '車数（軽油）
                    PARA39.Value = "0"                                '車数（３号軽油）
                    PARA40.Value = "0"                                '車数（５号軽油）
                    PARA41.Value = "0"                                '車数（１０号軽油）
                    PARA42.Value = "0"                                '車数（LSA）
                    PARA43.Value = "0"                                '車数（A重油）
                    PARA44.Value = "0"                                '車数（その他１）
                    PARA45.Value = "0"                                '車数（その他２）
                    PARA46.Value = "0"                                '車数（その他３）
                    PARA47.Value = "0"                                '車数（その他４）
                    PARA48.Value = "0"                                '車数（その他５）
                    PARA49.Value = "0"                                '車数（その他６）
                    PARA50.Value = "0"                                '車数（その他７）
                    PARA51.Value = "0"                                '車数（その他８）
                    PARA52.Value = "0"                                '車数（その他９）
                    PARA53.Value = "0"                                '車数（その他１０）
                    PARA54.Value = "0"                                '合計車数
                    PARA55.Value = "0"                                '変更後_車数（レギュラー）
                    PARA56.Value = "0"                                '変更後_車数（ハイオク）
                    PARA57.Value = "0"                                '変更後_車数（灯油）
                    PARA58.Value = "0"                                '変更後_車数（未添加灯油）
                    PARA59.Value = "0"                                '変更後_車数（軽油）
                    PARA60.Value = "0"                                '変更後_車数（３号軽油）
                    PARA61.Value = "0"                                '変更後_車数（５号軽油）
                    PARA62.Value = "0"                                '変更後_車数（１０号軽油）
                    PARA63.Value = "0"                                '変更後_車数（LSA）
                    PARA64.Value = "0"                                '変更後_車数（A重油）
                    PARA65.Value = "0"                                '変更後_車数（その他１）
                    PARA66.Value = "0"                                '変更後_車数（その他２）
                    PARA67.Value = "0"                                '変更後_車数（その他３）
                    PARA68.Value = "0"                                '変更後_車数（その他４）
                    PARA69.Value = "0"                                '変更後_車数（その他５）
                    PARA70.Value = "0"                                '変更後_車数（その他６）
                    PARA71.Value = "0"                                '変更後_車数（その他７）
                    PARA72.Value = "0"                                '変更後_車数（その他８）
                    PARA73.Value = "0"                                '変更後_車数（その他９）
                    PARA74.Value = "0"                                '変更後_車数（その他１０）
                    PARA75.Value = "0"                                '変更後_合計車数
                    PARA76.Value = ""                                 '貨車連結順序表№
                    PARA77.Value = "0"                                '売上金額
                    PARA78.Value = "0"                                '売上消費税額
                    PARA79.Value = "0"                                '売上合計金額
                    PARA80.Value = "0"                                '支払金額
                    PARA81.Value = "0"                                '支払消費税額
                    PARA82.Value = "0"                                '支払合計金額
                    PARA83.Value = OIT0001row("DELFLG")               '削除フラグ
                    PARA84.Value = WW_DATENOW                         '登録年月日
                    PARA85.Value = Master.USERID                      '登録ユーザーID
                    PARA86.Value = Master.USERTERMID                  '登録端末
                    PARA87.Value = WW_DATENOW                         '更新年月日
                    PARA88.Value = Master.USERID                      '更新ユーザーID
                    PARA89.Value = Master.USERTERMID                  '更新端末
                    PARA90.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_ORDERNUMBER.Text

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0001UPDtbl) Then
                            OIT0001UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0001UPDtbl.Clear()
                        OIT0001UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0001UPDrow As DataRow In OIT0001UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0001L"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0001UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                            Exit Sub
                        End If
                    Next
                    'End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D UPDATE_INSERT_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D UPDATE_INSERT_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注明細TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderDetail(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0003_DETAIL" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & "   AND  DETAILNO = @P02" _
            & "   AND  TANKNO   = @P03" _
            & "   AND  KAMOKU   = @P04" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0003_DETAIL" _
            & "    SET" _
            & "        OILCODE     = @P05, RETURNDATETRAIN = @P07, JOINT = @P08" _
            & "        , UPDYMD    = @P19, UPDUSER         = @P20" _
            & "        , UPDTERMID = @P21, RECEIVEYMD      = @P22" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & "        AND DETAILNO     = @P02" _
            & "        AND TANKNO       = @P03" _
            & "        AND KAMOKU       = @P04" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0003_DETAIL" _
            & "        ( ORDERNO   , DETAILNO        , TANKNO     , KAMOKU       , OILCODE" _
            & "        , CARSNUMBER, RETURNDATETRAIN , JOINT      , SALSE        , SALSETAX" _
            & "        , TOTALSALSE, PAYMENT         , PAYMENTTAX , TOTALPAYMENT , DELFLG" _
            & "        , INITYMD   , INITUSER        , INITTERMID" _
            & "        , UPDYMD    , UPDUSER         , UPDTERMID  , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15" _
            & "        , @P16, @P17, @P18" _
            & "        , @P19, @P20, @P21, @P22) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "    ORDERNO" _
            & "    , DETAILNO" _
            & "    , TANKNO" _
            & "    , KAMOKU" _
            & "    , OILCODE" _
            & "    , CARSNUMBER" _
            & "    , RETURNDATETRAIN" _
            & "    , JOINT" _
            & "    , SALSE" _
            & "    , SALSETAX" _
            & "    , TOTALSALSE" _
            & "    , PAYMENT" _
            & "    , PAYMENTTAX" _
            & "    , TOTALPAYMENT" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , UPDTIMSTP" _
            & " FROM" _
            & "    OIL.OIT0003_DETAIL" _
            & " WHERE" _
            & "        ORDERNO  = @P01" _
            & "   AND  DETAILNO = @P02" _
            & "   AND  TANKNO   = @P03" _
            & "   AND  KAMOKU   = @P04"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)   '受注明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 8)   'タンク車№
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 7)   '費用科目
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 4)   '油種コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.Int)           '車数
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.DateTime)      '返送日列車
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 200) 'ジョイント
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.Int)           '売上金額
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.Int)           '売上消費税額
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.Int)           '売上合計金額
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Int)           '支払金額
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Int)           '支払消費税額
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Int)           '支払合計金額
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)      '登録年月日
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)  '登録ユーザーID
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)  '登録端末
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.DateTime)      '更新年月日
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)  '更新ユーザーID
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 20)  '更新端末
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.DateTime)      '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 4) '受注№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3) '受注明細№
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 8) 'タンク車№
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 7) '費用科目

                For Each OIT0001row As DataRow In OIT0001tbl.Rows
                    'If Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                    '    Trim(OIT0001row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_ORDERNUMBER.Text       '受注№
                    PARA01.Value = OIT0001row("ORDERNO")              '受注№
                    PARA02.Value = OIT0001row("DETAILNO")             '受注明細№
                    PARA03.Value = OIT0001row("TANKNO")               'タンク車№
                    PARA04.Value = OIT0001row("KAMOKU")               '費用科目
                    PARA05.Value = OIT0001row("OILCODE")              '油種コード
                    PARA06.Value = "1"                                '車数
                    If OIT0001row("RETURNDATETRAIN") <> "" Then
                        PARA07.Value = OIT0001row("RETURNDATETRAIN")  '返送日列車
                    Else
                        PARA07.Value = DBNull.Value
                    End If
                    PARA08.Value = OIT0001row("JOINT")                'ジョイント
                    PARA09.Value = "0"                                '売上金額
                    PARA10.Value = "0"                                '売上消費税額
                    PARA11.Value = "0"                                '売上合計金額
                    PARA12.Value = "0"                                '支払金額
                    PARA13.Value = "0"                                '支払消費税額
                    PARA14.Value = "0"                                '支払合計金額
                    PARA15.Value = OIT0001row("DELFLG")               '削除フラグ
                    PARA16.Value = WW_DATENOW                         '登録年月日
                    PARA17.Value = Master.USERID                      '登録ユーザーID
                    PARA18.Value = Master.USERTERMID                  '登録端末
                    PARA19.Value = WW_DATENOW                         '更新年月日
                    PARA20.Value = Master.USERID                      '更新ユーザーID
                    PARA21.Value = Master.USERTERMID                  '更新端末
                    PARA22.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_ORDERNUMBER.Text
                    JPARA01.Value = OIT0001row("ORDERNO")             '受注№
                    JPARA02.Value = OIT0001row("DETAILNO")            '受注明細№
                    JPARA03.Value = OIT0001row("TANKNO")              'タンク車№
                    JPARA04.Value = OIT0001row("KAMOKU")              '費用科目

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0001UPDtbl) Then
                            OIT0001UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0001UPDtbl.Clear()
                        OIT0001UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0001UPDrow As DataRow In OIT0001UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0001L"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0001UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                            Exit Sub
                        End If
                    Next
                    'End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D UPDATE_INSERT_ORDERDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D UPDATE_INSERT_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (受注TBL)タンク車数更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderTankCnt(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     受注明細テーブルから油種別タンク車数を取得する
        Dim SQLStr As String =
              " SELECT DISTINCT" _
            & "   ISNULL(RTRIM(OIT0003.ORDERNO), '') AS ORDERNO" _
            & " , ISNULL(RTRIM(OIM0003.OILCODE), '') AS OILCODE" _
            & " , ISNULL(RTRIM(OIM0003.OILNAME), '') AS OILNAME" _
            & " , CAST(SUM(1) OVER(PARTITION BY OIM0003.OILCODE ORDER BY OIM0003.OILCODE) AS int) AS CNT" _
            & " FROM OIL.OIT0003_DETAIL OIT0003 " _
            & "  INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON" _
            & "        OIM0003.OFFICECODE = @P02 " _
            & "    AND OIM0003.OILCODE    = OIT0003.OILCODE" _
            & " WHERE OIT0003.ORDERNO = @P01" _
            & "   AND OIT0003.DELFLG <> @P03"

        'Dim SQLStr As String =
        '      " SELECT" _
        '    & "  VIEW_TANKCNT.ORDERNO   AS ORDERNO" _
        '    & " , VIEW_TANKCNT.OILCODE  AS OILCODE" _
        '    & " , VIEW_TANKCNT.OILNAME  AS OILNAME" _
        '    & " , VIEW_TANKCNT.TANKTYPE AS TANKTYPE" _
        '    & " , COUNT(1)              AS CNT" _
        '    & " FROM (" _
        '    & "  SELECT " _
        '    & "  ISNULL(RTRIM(OIT0003.ORDERNO), '')   AS ORDERNO" _
        '    & "  , ISNULL(RTRIM(OIM0003.OILCODE), '') AS OILCODE" _
        '    & "  , ISNULL(RTRIM(OIM0003.OILNAME), '') AS OILNAME" _
        '    & "  , CASE" _
        '    & "    WHEN OIM0003.OILCODE = '1001' THEN 'HTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1101' THEN 'RTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1301' THEN 'TTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1302' THEN 'MTTANK'" _
        '    & "    WHEN OIM0003.OILCODE IN ('1401','1406') THEN 'KTANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1402' THEN 'K5TANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1403' THEN 'K10TANK'" _
        '    & "    WHEN OIM0003.OILCODE IN ('1404','1405') THEN 'K3TANK'" _
        '    & "    WHEN OIM0003.OILCODE = '1501' THEN ''" _
        '    & "    WHEN OIM0003.OILCODE = '2101' THEN 'ATANK'" _
        '    & "    WHEN OIM0003.OILCODE IN ('2201','2202') THEN 'LTANK'" _
        '    & "    END TANKTYPE" _
        '    & "  FROM OIL.OIT0003_DETAIL OIT0003 " _
        '    & "  INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON " _
        '    & "        OIM0003.OFFICECODE = @P02 " _
        '    & "    AND OIM0003.OILCODE    = OIT0003.OILCODE " _
        '    & "  WHERE OIT0003.ORDERNO = @P01" _
        '    & "    AND OIT0003.DELFLG <> @P03" _
        '    & " ) VIEW_TANKCNT" _
        '    & " GROUP BY VIEW_TANKCNT.ORDERNO, VIEW_TANKCNT.OILCODE, VIEW_TANKCNT.OILNAME, VIEW_TANKCNT.TANKTYPE"

        '更新SQL文･･･受注TBLのタンク車数を更新
        Dim SQLUpStr As String =
                    " UPDATE OIL.OIT0002_ORDER           " _
                    & "    SET HTANK        = @P11,      " _
                    & "        RTANK        = @P12,      " _
                    & "        TTANK        = @P13,      " _
                    & "        MTTANK       = @P14,      " _
                    & "        KTANK        = @P15,      " _
                    & "        K3TANK       = @P16,      " _
                    & "        K5TANK       = @P17,      " _
                    & "        K10TANK      = @P18,      " _
                    & "        LTANK        = @P19,      " _
                    & "        ATANK        = @P20,      " _
                    & "        OTHER1OTANK  = @P21,      " _
                    & "        OTHER2OTANK  = @P22,      " _
                    & "        OTHER3OTANK  = @P23,      " _
                    & "        OTHER4OTANK  = @P24,      " _
                    & "        OTHER5OTANK  = @P25,      " _
                    & "        OTHER6OTANK  = @P26,      " _
                    & "        OTHER7OTANK  = @P27,      " _
                    & "        OTHER8OTANK  = @P28,      " _
                    & "        OTHER9OTANK  = @P29,      " _
                    & "        OTHER10OTANK = @P30,      " _
                    & "        TOTALTANK    = @P31,      " _
                    & "        UPDYMD       = @P32,      " _
                    & "        UPDUSER      = @P33,      " _
                    & "        UPDTERMID    = @P34,      " _
                    & "        RECEIVEYMD   = @P35       " _
                    & "  WHERE ORDERNO      = @P01       " _
                    & "    AND OFFICECODE   = @P02       " _
                    & "    AND DELFLG      <> @P03      ;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLUpcmd As New SqlCommand(SQLUpStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = work.WF_SEL_ORDERNUMBER.Text
                PARA2.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Dim PARAUP01 As SqlParameter = SQLUpcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARAUP02 As SqlParameter = SQLUpcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 6)  '受注営業所コード
                Dim PARAUP03 As SqlParameter = SQLUpcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARAUP11 As SqlParameter = SQLUpcmd.Parameters.Add("@P11", SqlDbType.Int)          '車数（ハイオク）
                Dim PARAUP12 As SqlParameter = SQLUpcmd.Parameters.Add("@P12", SqlDbType.Int)          '車数（レギュラー）
                Dim PARAUP13 As SqlParameter = SQLUpcmd.Parameters.Add("@P13", SqlDbType.Int)          '車数（灯油）
                Dim PARAUP14 As SqlParameter = SQLUpcmd.Parameters.Add("@P14", SqlDbType.Int)          '車数（未添加灯油）
                Dim PARAUP15 As SqlParameter = SQLUpcmd.Parameters.Add("@P15", SqlDbType.Int)          '車数（軽油）
                Dim PARAUP16 As SqlParameter = SQLUpcmd.Parameters.Add("@P16", SqlDbType.Int)          '車数（３号軽油）
                Dim PARAUP17 As SqlParameter = SQLUpcmd.Parameters.Add("@P17", SqlDbType.Int)          '車数（５号軽油）
                Dim PARAUP18 As SqlParameter = SQLUpcmd.Parameters.Add("@P18", SqlDbType.Int)          '車数（１０号軽油）
                Dim PARAUP19 As SqlParameter = SQLUpcmd.Parameters.Add("@P19", SqlDbType.Int)          '車数（LSA）
                Dim PARAUP20 As SqlParameter = SQLUpcmd.Parameters.Add("@P20", SqlDbType.Int)          '車数（A重油）
                Dim PARAUP21 As SqlParameter = SQLUpcmd.Parameters.Add("@P21", SqlDbType.Int)          '車数（その他１）
                Dim PARAUP22 As SqlParameter = SQLUpcmd.Parameters.Add("@P22", SqlDbType.Int)          '車数（その他２）
                Dim PARAUP23 As SqlParameter = SQLUpcmd.Parameters.Add("@P23", SqlDbType.Int)          '車数（その他３）
                Dim PARAUP24 As SqlParameter = SQLUpcmd.Parameters.Add("@P24", SqlDbType.Int)          '車数（その他４）
                Dim PARAUP25 As SqlParameter = SQLUpcmd.Parameters.Add("@P25", SqlDbType.Int)          '車数（その他５）
                Dim PARAUP26 As SqlParameter = SQLUpcmd.Parameters.Add("@P26", SqlDbType.Int)          '車数（その他６）
                Dim PARAUP27 As SqlParameter = SQLUpcmd.Parameters.Add("@P27", SqlDbType.Int)          '車数（その他７）
                Dim PARAUP28 As SqlParameter = SQLUpcmd.Parameters.Add("@P28", SqlDbType.Int)          '車数（その他８）
                Dim PARAUP29 As SqlParameter = SQLUpcmd.Parameters.Add("@P29", SqlDbType.Int)          '車数（その他９）
                Dim PARAUP30 As SqlParameter = SQLUpcmd.Parameters.Add("@P30", SqlDbType.Int)          '車数（その他１０）
                Dim PARAUP31 As SqlParameter = SQLUpcmd.Parameters.Add("@P31", SqlDbType.Int)          '合計車数
                Dim PARAUP32 As SqlParameter = SQLUpcmd.Parameters.Add("@P32", SqlDbType.DateTime)
                Dim PARAUP33 As SqlParameter = SQLUpcmd.Parameters.Add("@P33", SqlDbType.NVarChar)
                Dim PARAUP34 As SqlParameter = SQLUpcmd.Parameters.Add("@P34", SqlDbType.NVarChar)
                Dim PARAUP35 As SqlParameter = SQLUpcmd.Parameters.Add("@P35", SqlDbType.DateTime)
                PARAUP01.Value = work.WF_SEL_ORDERNUMBER.Text
                PARAUP02.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARAUP03.Value = C_DELETE_FLG.DELETE
                PARAUP11.Value = "0"
                PARAUP12.Value = "0"
                PARAUP13.Value = "0"
                PARAUP14.Value = "0"
                PARAUP15.Value = "0"
                PARAUP16.Value = "0"
                PARAUP17.Value = "0"
                PARAUP18.Value = "0"
                PARAUP19.Value = "0"
                PARAUP20.Value = "0"
                PARAUP21.Value = "0"
                PARAUP22.Value = "0"
                PARAUP23.Value = "0"
                PARAUP24.Value = "0"
                PARAUP25.Value = "0"
                PARAUP26.Value = "0"
                PARAUP27.Value = "0"
                PARAUP28.Value = "0"
                PARAUP29.Value = "0"
                PARAUP30.Value = "0"
                PARAUP31.Value = "0"

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0001UPDrow As DataRow In OIT0001WKtbl.Rows

                    Select Case OIT0001UPDrow("OILCODE")
                        Case "1001"
                            PARAUP11.Value = OIT0001UPDrow("CNT")
                            TxtHTank.Text = OIT0001UPDrow("CNT")
                        Case "1101"
                            PARAUP12.Value = OIT0001UPDrow("CNT")
                            TxtRTank.Text = OIT0001UPDrow("CNT")
                        Case "1301"
                            PARAUP13.Value = OIT0001UPDrow("CNT")
                            TxtTTank.Text = OIT0001UPDrow("CNT")
                        Case "1302"
                            PARAUP14.Value = OIT0001UPDrow("CNT")
                            TxtMTTank.Text = OIT0001UPDrow("CNT")
                        Case "1401", "1406"
                            PARAUP15.Value = OIT0001UPDrow("CNT")
                            TxtKTank.Text = OIT0001UPDrow("CNT")
                        Case "1404", "1405"
                            PARAUP16.Value = OIT0001UPDrow("CNT")
                            TxtK3Tank.Text = OIT0001UPDrow("CNT")
                        Case "1402"
                            PARAUP17.Value = OIT0001UPDrow("CNT")
                            TxtK5Tank.Text = OIT0001UPDrow("CNT")
                        Case "1403"
                            PARAUP18.Value = OIT0001UPDrow("CNT")
                            TxtK10Tank.Text = OIT0001UPDrow("CNT")
                        Case "2201", "2202"
                            PARAUP19.Value = OIT0001UPDrow("CNT")
                            TxtLTank.Text = OIT0001UPDrow("CNT")
                        Case "2101"
                            PARAUP20.Value = OIT0001UPDrow("CNT")
                            TxtATank.Text = OIT0001UPDrow("CNT")
                    End Select

                    i += OIT0001UPDrow("CNT")
                    TxtTotalTank.Text = i
                    PARAUP31.Value = i
                    PARAUP32.Value = Date.Now
                    PARAUP33.Value = Master.USERID
                    PARAUP34.Value = Master.USERTERMID
                    PARAUP35.Value = C_DEFAULT_YMD

                    SQLUpcmd.ExecuteNonQuery()
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D ORDERTANKCNTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D ORDERTANKCNTSET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 空回日報(一覧)表示用
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OrderListTBLSet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0001WKtbl) Then
            OIT0001WKtbl = New DataTable
        End If

        If OIT0001WKtbl.Columns.Count <> 0 Then
            OIT0001WKtbl.Columns.Clear()
        End If

        OIT0001WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , CAST(OIT0002.UPDTIMSTP AS bigint)                  AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '   ')          AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0002.ORDERINFO), '')               AS ORDERINFO" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')              AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')              AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')              AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')          AS ARRSTATIONNAME" _
            & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '')  AS LODDATE" _
            & " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '')  AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), '')  AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), '')  AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0002.RTANK), '')                   AS RTANK" _
            & " , ISNULL(RTRIM(OIT0002.HTANK), '')                   AS HTANK" _
            & " , ISNULL(RTRIM(OIT0002.TTANK), '')                   AS TTANK" _
            & " , ISNULL(RTRIM(OIT0002.MTTANK), '')                  AS MTTANK" _
            & " , ISNULL(RTRIM(OIT0002.KTANK), '')                   AS KTANK" _
            & " , ISNULL(RTRIM(OIT0002.K3TANK), '')                  AS K3TANK" _
            & " , ISNULL(RTRIM(OIT0002.K5TANK), '')                  AS K5TANK" _
            & " , ISNULL(RTRIM(OIT0002.K10TANK), '')                 AS K10TANK" _
            & " , ISNULL(RTRIM(OIT0002.LTANK), '')                   AS LTANK" _
            & " , ISNULL(RTRIM(OIT0002.ATANK), '')                   AS ATANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER1OTANK), '')             AS OTHER1OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER2OTANK), '')             AS OTHER2OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER3OTANK), '')             AS OTHER3OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER4OTANK), '')             AS OTHER4OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER5OTANK), '')             AS OTHER5OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER6OTANK), '')             AS OTHER6OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER7OTANK), '')             AS OTHER7OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER8OTANK), '')             AS OTHER8OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER9OTANK), '')             AS OTHER9OTANK" _
            & " , ISNULL(RTRIM(OIT0002.OTHER10OTANK), '')            AS OTHER10OTANK" _
            & " , ISNULL(RTRIM(OIT0002.TOTALTANK), '')               AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & " WHERE OIT0002.OFFICECODE = @P1" _
            & "   AND OIT0002.LODDATE    >= @P2" _
            & "   AND OIT0002.DELFLG     <> @P3"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0002.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = work.WF_SEL_SALESOFFICECODE.Text
                PARA2.Value = work.WF_SEL_LOADING.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0001WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0001WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0001row As DataRow In OIT0001WKtbl.Rows
                    i += 1
                    OIT0001row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0001D ORDERLISTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0001D OrderListSet"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001WKtbl, work.WF_SEL_INPTBL.Text)

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

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

                Case "PRODUCTPATTERN"   '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_SALESOFFICECODE.Text, "PRODUCTPATTERN"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
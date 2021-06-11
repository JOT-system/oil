''************************************************************
' 品種マスタメンテ登録・更新画面
' 作成日 2020/11/09
' 更新日 2021/04/16
' 作成者 JOT常井
' 更新者 JOT伊草
'
' 修正履歴:2020/11/09 新規作成
'         :2021/01/25 1)品種出荷期間マスタ項目を追加
'                     2)表更新→DB更新に名称変更
'                     3)DB更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
'         :2021/02/04 一覧画面での品種出荷期間マスタ項目追加・DB更新に伴い
'                     品種出荷期間マスタ項目も一覧画面とやり取りするように修正 
'         :2021/04/16 1)DB更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)新規登録を行って一覧画面に戻った際に、追加したデータが表示されないバグに対応
'         :2021/06/10 項目「出荷口」「平均積込数量」「出荷計画枠」
'                     「帳票用油種名」「JR油種区分(JR油種区分名)」を追加
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 品種マスタ登録（登録）
''' </summary>
''' <remarks></remarks>
Public Class OIM0003ProductCreate
    Inherits Page

    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIM0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0003INPtbl As DataTable                              'チェック用テーブル
    Private OIM0003UPDtbl As DataTable                              '更新用テーブル

    Private OIM0030tbl As DataTable                                 '品種出荷期間テーブル
    Private OIM0030INPtbl As DataTable                              '品種出荷期間更新チェック用テーブル
    Private OIM0030UPDtbl As DataTable                              '品種出荷期間更新用テーブル

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
                    '品種マスタ
                    Master.RecoverTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "btnClearConfirmOk"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
                    End Select

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

            WF_BOXChange.Value = "detailbox"

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIM0003tbl) Then
                OIM0003tbl.Clear()
                OIM0003tbl.Dispose()
                OIM0003tbl = Nothing
            End If

            If Not IsNothing(OIM0003INPtbl) Then
                OIM0003INPtbl.Clear()
                OIM0003INPtbl.Dispose()
                OIM0003INPtbl = Nothing
            End If

            If Not IsNothing(OIM0003UPDtbl) Then
                OIM0003UPDtbl.Clear()
                OIM0003UPDtbl.Dispose()
                OIM0003UPDtbl = Nothing
            End If

            If Not IsNothing(OIM0030tbl) Then
                OIM0030tbl.Clear()
                OIM0030tbl.Dispose()
                OIM0030tbl = Nothing
            End If

            If Not IsNothing(OIM0030INPtbl) Then
                OIM0030INPtbl.Clear()
                OIM0030INPtbl.Dispose()
                OIM0030INPtbl = Nothing
            End If

            If Not IsNothing(OIM0030UPDtbl) Then
                OIM0030UPDtbl.Clear()
                OIM0030UPDtbl.Dispose()
                OIM0030UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0003WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True

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
        rightview.COMPCODE = Master.USERCAMP
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 一覧画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0003L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '開始年月日・終了年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        WF_ORDERFROMDATE.Attributes("onkeyPress") = "CheckCalendar()"
        WF_ORDERTODATE.Attributes("onkeyPress") = "CheckCalendar()"

        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '営業所コード
        WF_OFFICECODE.Text = work.WF_SEL_OFFICECODE2.Text
        CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_DUMMY)

        '荷主コード
        WF_SHIPPERCODE.Text = work.WF_SEL_SHIPPERCODE2.Text
        CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_DUMMY)

        '基地コード
        WF_PLANTCODE.Text = work.WF_SEL_PLANTCODE2.Text
        CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_DUMMY)

        '油種大分類コード
        WF_BIGOILCODE.Text = work.WF_SEL_BIGOILCODE2.Text

        '油種大分類名
        WF_BIGOILNAME.Text = work.WF_SEL_BIGOILNAME.Text

        '油種大分類名カナ
        WF_BIGOILKANA.Text = work.WF_SEL_BIGOILKANA.Text

        '油種中分類コード
        WF_MIDDLEOILCODE.Text = work.WF_SEL_MIDDLEOILCODE2.Text

        '油種中分類名
        WF_MIDDLEOILNAME.Text = work.WF_SEL_MIDDLEOILNAME.Text

        '油種中分類名カナ
        WF_MIDDLEOILKANA.Text = work.WF_SEL_MIDDLEOILKANA.Text

        '油種コード
        WF_OILCODE.Text = work.WF_SEL_OILCODE2.Text

        '油種名
        WF_OILNAME.Text = work.WF_SEL_OILNAME.Text

        '油種名カナ
        WF_OILKANA.Text = work.WF_SEL_OILKANA.Text

        '油種細分コード
        WF_SEGMENTOILCODE.Text = work.WF_SEL_SEGMENTOILCODE.Text

        '油種名（細分）
        WF_SEGMENTOILNAME.Text = work.WF_SEL_SEGMENTOILNAME.Text

        'OT油種コード
        WF_OTOILCODE.Text = work.WF_SEL_OTOILCODE.Text

        'OT油種名
        WF_OTOILNAME.Text = work.WF_SEL_OTOILNAME.Text

        '荷主油種コード
        WF_SHIPPEROILCODE.Text = work.WF_SEL_SHIPPEROILCODE.Text

        '荷主油種名
        WF_SHIPPEROILNAME.Text = work.WF_SEL_SHIPPEROILNAME.Text

        '積込チェック用油種コード
        WF_CHECKOILCODE.Text = work.WF_SEL_CHECKOILCODE.Text

        '積込チェック用油種名
        WF_CHECKOILNAME.Text = work.WF_SEL_CHECKOILNAME.Text

        '在庫管理対象フラグ
        WF_STOCKFLG.Text = work.WF_SEL_STOCKFLG.Text
        CODENAME_get("STOCKFLG", WF_STOCKFLG.Text, WF_STOCKFLG_TEXT.Text, WW_DUMMY)

        '受注登録可能期間FROM
        WF_ORDERFROMDATE.Text = work.WF_SEL_ORDERFROMDATE.Text

        '受注登録可能期間TO
        WF_ORDERTODATE.Text = work.WF_SEL_ORDERTODATE.Text

        '帳票用油種名
        WF_REPORTOILNAME.Text = work.WF_SEL_REPORTOILNAME.Text

        'JR油種区分
        WF_JROILTYPE.Text = work.WF_SEL_JROILTYPE.Text

        'JR油種区分名
        WF_JROILTYPENAME.Text = work.WF_SEL_JROILTYPENAME.Text

        '出荷口
        WF_SHIPPINGGATE.Text = work.WF_SEL_SHIPPINGGATE.Text

        '平均積込数量
        WF_AVERAGELOADAMOUNT.Text = work.WF_SEL_AVERAGELOADAMOUNT.Text

        '出荷計画枠
        WF_SHIPPINGPLAN.Text = work.WF_SEL_SHIPPINGPLAN.Text

        '削除フラグ
        WF_DELFLG.Text = work.WF_SEL_DELFLG2.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '荷受人マスタ＆品種出荷期間マスタテーブル
        If String.IsNullOrEmpty(work.WF_SEL_OILTERM_CONSIGNEECODE_01.Text) Then
            GetNIUKEWithOILTERM()
        Else
            SetNIUKEWithOILTERM()
        End If

        WF_OILTERMTBL.DataSource = OIM0030tbl
        WF_OILTERMTBL.DataBind()

    End Sub

    ''' <summary>
    ''' 品種Mレコードから品種出荷期間Mの設定
    ''' </summary>
    Protected Sub SetNIUKEWithOILTERM()
        OIM0030tbl = New DataTable
        OIM0030tbl.Columns.Add("CONSIGNEECODE", Type.GetType("System.String"))
        OIM0030tbl.Columns.Add("CONSIGNEENAME", Type.GetType("System.String"))
        OIM0030tbl.Columns.Add("ORDERFROMDATE", Type.GetType("System.String"))
        OIM0030tbl.Columns.Add("ORDERTODATE", Type.GetType("System.String"))
        OIM0030tbl.Columns.Add("DELFLG", Type.GetType("System.String"))
        Dim OIM0030row As DataRow

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_01.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_01.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_01.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_01.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_01.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_02.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_02.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_02.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_02.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_02.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_03.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_03.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_03.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_03.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_03.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_04.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_04.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_04.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_04.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_04.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_05.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_05.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_05.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_05.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_05.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_06.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_06.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_06.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_06.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_06.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_07.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_07.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_07.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_07.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_07.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_08.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_08.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_08.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_08.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_08.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_09.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_09.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_09.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_09.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_09.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_10.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_10.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_10.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_10.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_10.Text
        OIM0030tbl.Rows.Add(OIM0030row)

        OIM0030row = OIM0030tbl.NewRow
        OIM0030row("CONSIGNEECODE") = work.WF_SEL_OILTERM_CONSIGNEECODE_11.Text
        OIM0030row("CONSIGNEENAME") = work.WF_SEL_OILTERM_CONSIGNEENAME_11.Text
        OIM0030row("ORDERFROMDATE") = work.WF_SEL_OILTERM_ORDERFROMDATE_11.Text
        OIM0030row("ORDERTODATE") = work.WF_SEL_OILTERM_ORDERTODATE_11.Text
        OIM0030row("DELFLG") = work.WF_SEL_OILTERM_DELFLG_11.Text
        OIM0030tbl.Rows.Add(OIM0030row)
    End Sub

    ''' <summary>
    ''' 荷受人マスタ＆品種出荷期間マスタ取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetNIUKEWithOILTERM()
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            'DataBase接続
            SQLcon.Open()

            '○ 対象データ取得
            Dim SQLStr As String =
                  " SELECT" _
                & "    ISNULL(RTRIM(OIM0012.CONSIGNEECODE), '')                  AS CONSIGNEECODE" _
                & "    , ISNULL(RTRIM(OIM0012.CONSIGNEENAME), '')                AS CONSIGNEENAME" _
                & "    , ISNULL(FORMAT(OIM0030.ORDERFROMDATE, 'yyyy/MM/dd'), '') AS ORDERFROMDATE" _
                & "    , ISNULL(FORMAT(OIM0030.ORDERTODATE, 'yyyy/MM/dd'), '')   AS ORDERTODATE" _
                & "    , ISNULL(RTRIM(OIM0030.DELFLG), '')                       AS DELFLG" _
                & " FROM" _
                & "    oil.OIM0012_NIUKE OIM0012" _
                & "    LEFT OUTER JOIN (" _
                & "        SELECT" _
                & "            OIM0030.CONSIGNEECODE" _
                & "            , OIM0030.ORDERFROMDATE" _
                & "            , OIM0030.ORDERTODATE" _
                & "            , OIM0030.DELFLG" _
                & "        FROM" _
                & "            oil.OIM0030_OILTERM OIM0030" _
                & "            INNER JOIN oil.OIM0003_PRODUCT OIM0003" _
                & "                ON  OIM0030.OFFICECODE     = OIM0003.OFFICECODE" _
                & "                AND OIM0030.SHIPPERCODE    = OIM0003.SHIPPERCODE" _
                & "                AND OIM0030.PLANTCODE      = OIM0003.PLANTCODE" _
                & "                AND OIM0030.OILCODE        = OIM0003.OILCODE" _
                & "                AND OIM0030.SEGMENTOILCODE = OIM0003.SEGMENTOILCODE" _
                & "        WHERE" _
                & "            OIM0003.OFFICECODE     = @P01" _
                & "        AND OIM0003.SHIPPERCODE    = @P02" _
                & "        AND OIM0003.PLANTCODE      = @P03" _
                & "        AND OIM0003.OILCODE        = @P04" _
                & "        AND OIM0003.SEGMENTOILCODE = @P05" _
                & "    ) OIM0030" _
                & "    ON OIM0030.CONSIGNEECODE = OIM0012.CONSIGNEECODE" _
                & " WHERE" _
                & "    OIM0012.DELFLG <> @P00"

            Try
                Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)   '削除フラグ
                    Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)   '営業所コード
                    Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)  '荷主コード
                    Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 4)   '基地コード
                    Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)   '油種コード
                    Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)   '油種細分コード

                    PARA00.Value = C_DELETE_FLG.DELETE
                    PARA01.Value = WF_OFFICECODE.Text
                    PARA02.Value = WF_SHIPPERCODE.Text
                    PARA03.Value = WF_PLANTCODE.Text
                    PARA04.Value = WF_OILCODE.Text
                    PARA05.Value = WF_SEGMENTOILCODE.Text

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                        OIM0030tbl = New DataTable

                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0030tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        OIM0030tbl.Load(SQLdr)

                        If OIM0030tbl.Rows.Count <= 0 Then
                            '荷受人マスタのデータ未存在エラー
                            Master.Output(Messages.C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR)
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003C SELECT GetNIUKEWithOILTERM")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:OIM0003C GetNIUKEWithOILTERM"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                Exit Sub
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT" _
            & "     ISNULL(RTRIM(OIM0003.OFFICECODE), '')                       AS OFFICECODE" _
            & "     , ISNULL(RTRIM(OIM0003.SHIPPERCODE), '')                    AS SHIPPERCODE" _
            & "     , ISNULL(RTRIM(OIM0003.PLANTCODE), '')                      AS PLANTCODE" _
            & "     , ISNULL(RTRIM(OIM0003.OILCODE), '')                        AS OILCODE" _
            & "     , ISNULL(RTRIM(OIM0003.SEGMENTOILCODE), '')                 AS SEGMENTOILCODE" _
            & " FROM" _
            & "     OIL.OIM0003_PRODUCT OIM0003" _
            & " WHERE" _
            & "     OIM0003.OFFICECODE         =  @P01" _
            & "     AND OIM0003.SHIPPERCODE    =  @P02" _
            & "     AND OIM0003.PLANTCODE      =  @P03" _
            & "     AND OIM0003.OILCODE        =  @P04" _
            & "     AND OIM0003.SEGMENTOILCODE =  @P05" _
            & "     AND OIM0003.DELFLG         <> @P06"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)            '営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)           '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 4)            '基地コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)            '油種コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)            '油種細分コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 1)            '削除フラグ

                PARA01.Value = WF_OFFICECODE.Text
                PARA02.Value = WF_SHIPPERCODE.Text
                PARA03.Value = WF_PLANTCODE.Text
                PARA04.Value = WF_OILCODE.Text
                PARA05.Value = WF_SEGMENTOILCODE.Text
                PARA06.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0003Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0003Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0003Chk.Load(SQLdr)

                    If OIM0003Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0003C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0003INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '〇 DB更新
        If isNormal(WW_ERR_SW) Then
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If Not isInputChange() Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If

            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                '品種マスタ更新
                UpdateMasterProduct(SQLcon)
                If Not isNormal(WW_ERR_SW) Then
                    Exit Sub
                End If

                '品種出荷期間マスタ更新
                UpdateMasterOilTerm(SQLcon)
                If Not isNormal(WW_ERR_SW) Then
                    Exit Sub
                End If
            End Using
        End If

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0003tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        work.WF_SEL_DBUPDATE_MESSAGE.Text = ""
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
                work.WF_SEL_DBUPDATE_MESSAGE.Text = C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL
            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "営業所コード,荷主コード,基地コード,油種コード,油種細分コード,削除フラグ", needsPopUp:=True)
            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            End If
        End If

        If isNormal(WW_ERR_SW) Then
            '前ページ遷移
            Master.TransitionPrevPage()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIM0003INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIM0003INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0003INProw As DataRow = OIM0003INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0003INPcol As DataColumn In OIM0003INPtbl.Columns
            If IsDBNull(OIM0003INProw.Item(OIM0003INPcol)) OrElse IsNothing(OIM0003INProw.Item(OIM0003INPcol)) Then
                Select Case OIM0003INPcol.ColumnName
                    Case "LINECNT"
                        OIM0003INProw.Item(OIM0003INPcol) = 0
                    Case "OPERATION"
                        OIM0003INProw.Item(OIM0003INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        OIM0003INProw.Item(OIM0003INPcol) = 0
                    Case "SELECT"
                        OIM0003INProw.Item(OIM0003INPcol) = 1
                    Case "HIDDEN"
                        OIM0003INProw.Item(OIM0003INPcol) = 0
                    Case Else
                        OIM0003INProw.Item(OIM0003INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0003INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0003INProw("LINECNT"))
            Catch ex As Exception
                OIM0003INProw("LINECNT") = 0
            End Try
        End If

        OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0003INProw("TIMSTP") = 0
        OIM0003INProw("SELECT") = 1
        OIM0003INProw("HIDDEN") = 0

        OIM0003INProw("OFFICECODE") = WF_OFFICECODE.Text                '営業所コード
        OIM0003INProw("SHIPPERCODE") = WF_SHIPPERCODE.Text              '荷主コード
        OIM0003INProw("PLANTCODE") = WF_PLANTCODE.Text                  '基地コード
        OIM0003INProw("BIGOILCODE") = WF_BIGOILCODE.Text                '油種大分類コード
        OIM0003INProw("BIGOILNAME") = WF_BIGOILNAME.Text                '油種大分類名
        OIM0003INProw("BIGOILKANA") = WF_BIGOILKANA.Text                '油種大分類名カナ
        OIM0003INProw("MIDDLEOILCODE") = WF_MIDDLEOILCODE.Text          '油種中分類コード
        OIM0003INProw("MIDDLEOILNAME") = WF_MIDDLEOILNAME.Text          '油種中分類名
        OIM0003INProw("MIDDLEOILKANA") = WF_MIDDLEOILKANA.Text          '油種中分類名カナ
        OIM0003INProw("OILCODE") = WF_OILCODE.Text                      '油種コード
        OIM0003INProw("OILNAME") = WF_OILNAME.Text                      '油種名
        OIM0003INProw("OILKANA") = WF_OILKANA.Text                      '油種名カナ
        OIM0003INProw("SEGMENTOILCODE") = WF_SEGMENTOILCODE.Text        '油種細分コード
        OIM0003INProw("SEGMENTOILNAME") = WF_SEGMENTOILNAME.Text        '油種名（細分）
        OIM0003INProw("OTOILCODE") = WF_OTOILCODE.Text                  'OT油種コード
        OIM0003INProw("OTOILNAME") = WF_OTOILNAME.Text                  'OT油種名
        OIM0003INProw("SHIPPEROILCODE") = WF_SHIPPEROILCODE.Text        '荷主油種コード
        OIM0003INProw("SHIPPEROILNAME") = WF_SHIPPEROILNAME.Text        '荷主油種名
        OIM0003INProw("CHECKOILCODE") = WF_CHECKOILCODE.Text            '積込チェック用油種コード
        OIM0003INProw("CHECKOILNAME") = WF_CHECKOILNAME.Text            '積込チェック用油種名
        OIM0003INProw("STOCKFLG") = WF_STOCKFLG.Text                    '在庫管理対象フラグ
        OIM0003INProw("ORDERFROMDATE") = WF_ORDERFROMDATE.Text          '受注登録可能期間FROM
        OIM0003INProw("ORDERTODATE") = WF_ORDERTODATE.Text              '受注登録可能期間TO
        OIM0003INProw("REPORTOILNAME") = WF_REPORTOILNAME.Text          '帳票用油種名
        OIM0003INProw("JROILTYPE") = WF_JROILTYPE.Text                  'JR油種区分
        OIM0003INProw("JROILTYPENAME") = WF_JROILTYPENAME.Text          'JR油種区分名
        OIM0003INProw("SHIPPINGGATE") = WF_SHIPPINGGATE.Text            '出荷口
        OIM0003INProw("AVERAGELOADAMOUNT") = WF_AVERAGELOADAMOUNT.Text  '平均積込数量
        OIM0003INProw("SHIPPINGPLAN") = WF_SHIPPINGPLAN.Text            '出荷計画枠
        OIM0003INProw("DELFLG") = WF_DELFLG.Text                        '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0003INPtbl.Rows.Add(OIM0003INProw)

        '品種出荷期間テーブル
        OIM0030INPtbl = New DataTable()
        OIM0030INPtbl.Columns.Add("OFFICECODE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("SHIPPERCODE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("PLANTCODE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("OILCODE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("SEGMENTOILCODE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("CONSIGNEECODE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("CONSIGNEENAME", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("ORDERFROMDATE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("ORDERTODATE", System.Type.GetType("System.String"))
        OIM0030INPtbl.Columns.Add("DELFLG", System.Type.GetType("System.String"))

        For Each row As GridViewRow In WF_OILTERMTBL.Rows
            Dim WW_CONSIGNEECODE As String = ""
            Dim WW_CONSIGNEENAME As String = ""
            Dim WW_ORDERFROMDATE As String = ""
            Dim WW_ORDERTODATE As String = ""
            Dim WW_DELFLG As String = ""
            Dim addRow As DataRow = OIM0030INPtbl.NewRow

            '品種出荷期間テーブルの荷受人コード
            If row.FindControl("WF_OILTERMTBL_CONSIGNEECODE") IsNot Nothing Then
                WW_CONSIGNEECODE = DirectCast(row.FindControl("WF_OILTERMTBL_CONSIGNEECODE"), Label).Text
            End If
            '品種出荷期間テーブルの荷受人名
            If row.FindControl("WF_OILTERMTBL_CONSIGNEENAME") IsNot Nothing Then
                WW_CONSIGNEENAME = DirectCast(row.FindControl("WF_OILTERMTBL_CONSIGNEENAME"), Label).Text
            End If
            '品種出荷期間テーブルの受注登録可能期間FROM
            If row.FindControl("WF_OILTERMTBL_ORDERFROMDATE") IsNot Nothing Then
                WW_ORDERFROMDATE = DirectCast(row.FindControl("WF_OILTERMTBL_ORDERFROMDATE"), TextBox).Text
            End If
            '品種出荷期間テーブルの受注登録可能期間TO
            If row.FindControl("WF_OILTERMTBL_ORDERTODATE") IsNot Nothing Then
                WW_ORDERTODATE = DirectCast(row.FindControl("WF_OILTERMTBL_ORDERTODATE"), TextBox).Text
            End If
            '品種出荷期間テーブルの受注登録可能期間削除フラグ
            If row.FindControl("WF_OILTERMTBL_DELFLG") IsNot Nothing Then
                WW_DELFLG = DirectCast(row.FindControl("WF_OILTERMTBL_DELFLG"), TextBox).Text
            End If

            '更新対象に追加
            addRow("OFFICECODE") = WF_OFFICECODE.Text
            addRow("SHIPPERCODE") = WF_SHIPPERCODE.Text
            addRow("PLANTCODE") = WF_PLANTCODE.Text
            addRow("OILCODE") = WF_OILCODE.Text
            addRow("SEGMENTOILCODE") = WF_SEGMENTOILCODE.Text
            addRow("CONSIGNEECODE") = WW_CONSIGNEECODE
            addRow("CONSIGNEENAME") = WW_CONSIGNEENAME
            addRow("ORDERFROMDATE") = WW_ORDERFROMDATE
            addRow("ORDERTODATE") = WW_ORDERTODATE
            addRow("DELFLG") = WW_DELFLG
            OIM0030INPtbl.Rows.Add(addRow)
        Next

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0003INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '変更チェック
        If isInputChange() Then
            '変更がある場合は、確認ダイアログを表示
            Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
                needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOk")
        Else
            '変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
            WF_CLEAR_ConfirmOkClick()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_ConfirmOkClick()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            WW_ERR_SW = C_MESSAGE_NO.NORMAL
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA, C_LIST_OPERATION_CODE.NODISP
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0003tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_OFFICECODE.Text = ""             '営業所コード
        WF_SHIPPERCODE.Text = ""            '荷主コード
        WF_PLANTCODE.Text = ""              '基地コード
        WF_BIGOILCODE.Text = ""             '油種大分類コード
        WF_BIGOILNAME.Text = ""             '油種大分類名
        WF_BIGOILKANA.Text = ""             '油種大分類名カナ
        WF_MIDDLEOILCODE.Text = ""          '油種中分類コード
        WF_MIDDLEOILNAME.Text = ""          '油種中分類名
        WF_MIDDLEOILKANA.Text = ""          '油種中分類名カナ
        WF_OILCODE.Text = ""                '油種コード
        WF_OILNAME.Text = ""                '油種名
        WF_OILKANA.Text = ""                '油種名カナ
        WF_SEGMENTOILCODE.Text = ""         '油種細分コード
        WF_SEGMENTOILNAME.Text = ""         '油種名（細分）
        WF_OTOILCODE.Text = ""              'OT油種コード
        WF_OTOILNAME.Text = ""              'OT油種名
        WF_SHIPPEROILCODE.Text = ""         '荷主油種コード
        WF_SHIPPEROILNAME.Text = ""         '荷主油種名
        WF_CHECKOILCODE.Text = ""           '積込チェック用油種コード
        WF_CHECKOILNAME.Text = ""           '積込チェック用油種名
        WF_STOCKFLG.Text = ""               '在庫管理対象フラグ
        WF_ORDERFROMDATE.Text = ""          '受注登録可能期間FROM
        WF_ORDERTODATE.Text = ""            '受注登録可能期間TO
        WF_REPORTOILNAME.Text = ""          '帳票用油種名
        WF_JROILTYPE.Text = ""              'JR油種区分
        WF_JROILTYPENAME.Text = ""          'JR油種区分名
        WF_SHIPPINGGATE.Text = ""           '出荷口
        WF_AVERAGELOADAMOUNT.Text = ""      '平均積込数量
        WF_SHIPPINGPLAN.Text = ""           '出荷計画枠
        WF_DELFLG.Text = ""                 '削除フラグ

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

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case WF_ORDERFROMDATE.ID
                                '受注登録可能期間FROM
                                .WF_Calendar.Text = WF_ORDERFROMDATE.Text
                            Case WF_ORDERTODATE.ID
                                '受注登録可能期間TO
                                .WF_Calendar.Text = WF_ORDERTODATE.Text
                            Case Else
                                Dim rowIdx As Integer
                                '品種出荷期間テーブルの受注登録可能期間FROM
                                If WW_FIELD.Contains("WF_OILTERMTBL_ORDERFROMDATE") Then
                                    Integer.TryParse(WW_FIELD.Substring(WW_FIELD.Length - 3), rowIdx)
                                    If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERFROMDATE") IsNot Nothing Then
                                        .WF_Calendar.Text = DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERFROMDATE"), TextBox).Text
                                    End If
                                End If
                                '品種出荷期間テーブルの受注登録可能期間TO
                                If WW_FIELD.Contains("WF_OILTERMTBL_ORDERTODATE") Then
                                    Integer.TryParse(WW_FIELD.Substring(WW_FIELD.Length - 3), rowIdx)
                                    If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERTODATE") IsNot Nothing Then
                                        .WF_Calendar.Text = DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERTODATE"), TextBox).Text
                                    End If
                                End If
                        End Select
                        .ActiveCalendar()

                    Case Else
                        Dim prmData As New Hashtable

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case WF_OFFICECODE.ID
                                '営業所コード
                                prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, WF_OFFICECODE.Text)
                            Case WF_SHIPPERCODE.ID
                                '荷主コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                            Case WF_PLANTCODE.ID
                                '基地コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                            Case WF_BIGOILCODE.ID
                                '油種大分類コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                            Case WF_MIDDLEOILCODE.ID
                                '油種中分類コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                            Case WF_OTOILCODE.ID
                                'OT油種コード
                                prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                            Case WF_STOCKFLG.ID
                                '在庫管理対象フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PRODUCTSTOCKFLG")
                            Case WF_JROILTYPE.ID
                                'JR油種区分
                                prmData = work.CreateFIXParam(Master.USERCAMP, "JROILTYPE")
                            Case WF_SHIPPINGGATE.ID
                                '出荷口
                                prmData = work.CreateFIXParam(Master.USERCAMP, "SHIPPINGGATE")
                            Case WF_DELFLG.ID
                                '削除フラグ
                                prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                            Case Else
                                '品種出荷期間テーブルの削除フラグ
                                If WW_FIELD.Contains("WF_OILTERMTBL_DELFLG") Then
                                    prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                                End If
                        End Select
                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case WF_OFFICECODE.ID
                '営業所コード
                CODENAME_get("OFFICECODE", WF_OFFICECODE.Text, WF_OFFICECODE_TEXT.Text, WW_RTN_SW)
            Case WF_SHIPPERCODE.ID
                '荷主コード
                CODENAME_get("SHIPPERCODE", WF_SHIPPERCODE.Text, WF_SHIPPERCODE_TEXT.Text, WW_RTN_SW)
            Case WF_PLANTCODE.ID
                '基地コード
                CODENAME_get("PLANTCODE", WF_PLANTCODE.Text, WF_PLANTCODE_TEXT.Text, WW_RTN_SW)
            Case WF_BIGOILCODE.ID
                '油種大分類コード
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_MIDDLEOILCODE.ID
                '油種中分類コード
                CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE_TEXT.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_OTOILCODE.ID
                'OT油種コード
                CODENAME_get("OTOILCODE", WF_OTOILCODE.Text, WF_OTOILCODE_TEXT.Text, WW_RTN_SW)
            Case WF_STOCKFLG.ID
                '在庫管理対象フラグ
                CODENAME_get("STOCKFLG", WF_STOCKFLG.Text, WF_STOCKFLG_TEXT.Text, WW_RTN_SW)
            Case WF_JROILTYPE.ID
                'JR油種区分
                CODENAME_get("JROILTYPE", WF_JROILTYPE.Text, WF_JROILTYPENAME.Text, WW_RTN_SW)
            Case WF_JROILTYPE.ID
                '出荷口
                CODENAME_get("SHIPPINGGATE", WF_SHIPPINGGATE.Text, WW_DUMMY, WW_RTN_SW)
            Case WF_DELFLG.ID
                '削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case WF_OFFICECODE.ID
                    '営業所コード
                    WF_OFFICECODE.Text = WW_SelectValue
                    WF_OFFICECODE_TEXT.Text = WW_SelectText
                    WF_OFFICECODE.Focus()
                Case WF_SHIPPERCODE.ID
                    '荷主コード
                    WF_SHIPPERCODE.Text = WW_SelectValue
                    WF_SHIPPERCODE_TEXT.Text = WW_SelectText
                    WF_SHIPPERCODE.Focus()
                Case WF_PLANTCODE.ID
                    '基地コード
                    WF_PLANTCODE.Text = WW_SelectValue
                    WF_PLANTCODE_TEXT.Text = WW_SelectText
                    WF_PLANTCODE.Focus()
                Case WF_BIGOILCODE.ID
                    '油種大分類コード
                    WF_BIGOILCODE.Text = WW_SelectValue
                    WF_BIGOILCODE_TEXT.Text = WW_SelectText
                    WF_BIGOILCODE.Focus()
                Case WF_MIDDLEOILCODE.ID
                    '油種中分類コード
                    WF_MIDDLEOILCODE.Text = WW_SelectValue
                    WF_MIDDLEOILCODE_TEXT.Text = WW_SelectText
                    WF_MIDDLEOILCODE.Focus()
                Case WF_OTOILCODE.ID
                    'OT油種コード
                    WF_OTOILCODE.Text = WW_SelectValue
                    WF_OTOILCODE_TEXT.Text = WW_SelectText
                    WF_OTOILCODE.Focus()
                Case WF_STOCKFLG.ID
                    '在庫管理対象フラグ
                    WF_STOCKFLG.Text = WW_SelectValue
                    WF_STOCKFLG_TEXT.Text = WW_SelectText
                    WF_STOCKFLG.Focus()
                Case WF_ORDERFROMDATE.ID
                    '受注登録可能期間FROM
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ORDERFROMDATE.Text = ""
                        Else
                            WF_ORDERFROMDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ORDERFROMDATE.Focus()
                Case WF_ORDERTODATE.ID
                    '受注登録可能期間TO
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ORDERTODATE.Text = ""
                        Else
                            WF_ORDERTODATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ORDERTODATE.Focus()
                Case WF_JROILTYPE.ID
                    'JR油種区分
                    WF_JROILTYPE.Text = WW_SelectValue
                    WF_JROILTYPENAME.Text = WW_SelectText
                    WF_JROILTYPE.Focus()
                Case WF_SHIPPINGGATE.ID
                    '出荷口
                    WF_SHIPPINGGATE.Text = WW_SelectValue
                    WF_SHIPPINGGATE.Focus()
                Case WF_DELFLG.ID
                    '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()
                Case Else
                    Dim rowIdx As Integer
                    Dim WW_TextBox As TextBox = Nothing
                    '品種出荷期間テーブルの受注登録可能期間FROM
                    If WF_FIELD.Value.Contains("WF_OILTERMTBL_ORDERFROMDATE") Then
                        Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                        If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERFROMDATE") IsNot Nothing Then
                            WW_TextBox = DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERFROMDATE"), TextBox)
                            WW_TextBox.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                            WW_TextBox.Focus()
                        End If
                    End If
                    '品種出荷期間テーブルの受注登録可能期間TO
                    If WF_FIELD.Value.Contains("WF_OILTERMTBL_ORDERTODATE") Then
                        Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                        If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERTODATE") IsNot Nothing Then
                            WW_TextBox = DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERTODATE"), TextBox)
                            WW_TextBox.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                            WW_TextBox.Focus()
                        End If
                    End If
                    '品種出荷期間テーブルの削除フラグ
                    If WF_FIELD.Value.Contains("WF_OILTERMTBL_DELFLG") Then
                        Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                        If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_DELFLG") IsNot Nothing Then
                            WW_TextBox = DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_DELFLG"), TextBox)
                            WW_TextBox.Text = WW_SelectValue
                            WW_TextBox.Focus()
                        End If
                    End If
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case WF_OFFICECODE.ID
                    '営業所コード
                    WF_OFFICECODE.Focus()
                Case WF_SHIPPERCODE.ID
                    '荷主コード
                    WF_SHIPPERCODE.Focus()
                Case WF_PLANTCODE.ID
                    '基地コード
                    WF_PLANTCODE.Focus()
                Case WF_BIGOILCODE.ID
                    '油種大分類コード
                    WF_BIGOILCODE.Focus()
                Case WF_MIDDLEOILCODE.ID
                    '油種中分類コード
                    WF_MIDDLEOILCODE.Focus()
                Case WF_OTOILCODE.ID
                    'OT油種コード
                    WF_OTOILCODE.Focus()
                Case WF_STOCKFLG.ID
                    '在庫管理対象フラグ
                    WF_STOCKFLG.Focus()
                Case WF_ORDERFROMDATE.ID
                    '受注登録可能期間FROM
                    WF_ORDERFROMDATE.Focus()
                Case WF_ORDERTODATE.ID
                    '受注登録可能期間TO
                    WF_ORDERTODATE.Focus()
                Case WF_JROILTYPE.ID
                    'JR油種区分
                    WF_JROILTYPE.Focus()
                Case WF_SHIPPINGGATE.ID
                    'JR油種区分
                    WF_SHIPPINGGATE.Focus()
                Case WF_DELFLG.ID
                    '削除フラグ
                    WF_DELFLG.Focus()
                Case Else
                    Dim rowIdx As Integer
                    '品種出荷期間テーブルの受注登録可能期間FROM
                    If WF_FIELD.Value.Contains("WF_OILTERMTBL_ORDERFROMDATE") Then
                        Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                        If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERFROMDATE") IsNot Nothing Then
                            DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERFROMDATE"), TextBox).Focus()
                        End If
                    End If
                    '品種出荷期間テーブルの受注登録可能期間TO
                    If WF_FIELD.Value.Contains("WF_OILTERMTBL_ORDERTODATE") Then
                        Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                        If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERTODATE") IsNot Nothing Then
                            DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_ORDERTODATE"), TextBox).Focus()
                        End If
                    End If
                    '品種出荷期間テーブルの削除フラグ
                    If WF_FIELD.Value.Contains("WF_OILTERMTBL_DELFLG") Then
                        Integer.TryParse(WF_FIELD.Value.Substring(WF_FIELD.Value.Length - 3), rowIdx)
                        If WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_DELFLG") IsNot Nothing Then
                            DirectCast(WF_OILTERMTBL.Rows(rowIdx - 1).FindControl("WF_OILTERMTBL_DELFLG"), TextBox).Focus()
                        End If
                    End If

            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

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

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 品種マスタ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterProduct(ByVal SQLcon As SqlConnection)

        '○ DB更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIM0003_PRODUCT" _
            & "    WHERE" _
            & "        OFFICECODE         = @P01" _
            & "        AND SHIPPERCODE    = @P02" _
            & "        AND PLANTCODE      = @P03" _
            & "        AND OILCODE        = @P10" _
            & "        AND SEGMENTOILCODE = @P13;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0003_PRODUCT" _
            & "    SET" _
            & "        OFFICECODE     = @P01" _
            & "        , SHIPPERCODE    = @P02" _
            & "        , PLANTCODE      = @P03" _
            & "        , BIGOILCODE     = @P04" _
            & "        , BIGOILNAME     = @P05" _
            & "        , BIGOILKANA     = @P06" _
            & "        , MIDDLEOILCODE  = @P07" _
            & "        , MIDDLEOILNAME  = @P08" _
            & "        , MIDDLEOILKANA  = @P09" _
            & "        , OILCODE        = @P10" _
            & "        , OILNAME        = @P11" _
            & "        , OILKANA        = @P12" _
            & "        , SEGMENTOILCODE = @P13" _
            & "        , SEGMENTOILNAME = @P14" _
            & "        , OTOILCODE      = @P15" _
            & "        , OTOILNAME      = @P16" _
            & "        , SHIPPEROILCODE = @P17" _
            & "        , SHIPPEROILNAME = @P18" _
            & "        , CHECKOILCODE   = @P19" _
            & "        , CHECKOILNAME   = @P20" _
            & "        , STOCKFLG       = @P21" _
            & "        , ORDERFROMDATE  = @P22" _
            & "        , ORDERTODATE    = @P23" _
            & "        , REPORTOILNAME  = @P32" _
            & "        , JROILTYPE      = @P33" _
            & "        , JROILTYPENAME  = @P34" _
            & "        , SHIPPINGGATE   = @P35" _
            & "        , AVERAGELOADAMOUNT = @P36" _
            & "        , SHIPPINGPLAN   = @P37" _
            & "        , DELFLG         = @P24" _
            & "        , UPDYMD         = @P28" _
            & "        , UPDUSER        = @P29" _
            & "        , UPDTERMID      = @P30" _
            & "        , RECEIVEYMD     = @P31" _
            & "    WHERE" _
            & "        OFFICECODE         = @P01" _
            & "        AND SHIPPERCODE    = @P02" _
            & "        AND PLANTCODE      = @P03" _
            & "        AND OILCODE        = @P10" _
            & "        AND SEGMENTOILCODE = @P13;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0003_PRODUCT" _
            & "        ( OFFICECODE" _
            & "        , SHIPPERCODE" _
            & "        , PLANTCODE" _
            & "        , BIGOILCODE" _
            & "        , BIGOILNAME" _
            & "        , BIGOILKANA" _
            & "        , MIDDLEOILCODE" _
            & "        , MIDDLEOILNAME" _
            & "        , MIDDLEOILKANA" _
            & "        , OILCODE" _
            & "        , OILNAME" _
            & "        , OILKANA" _
            & "        , SEGMENTOILCODE" _
            & "        , SEGMENTOILNAME" _
            & "        , OTOILCODE" _
            & "        , OTOILNAME" _
            & "        , SHIPPEROILCODE" _
            & "        , SHIPPEROILNAME" _
            & "        , CHECKOILCODE" _
            & "        , CHECKOILNAME" _
            & "        , STOCKFLG" _
            & "        , ORDERFROMDATE" _
            & "        , ORDERTODATE" _
            & "        , REPORTOILNAME" _
            & "        , JROILTYPE" _
            & "        , JROILTYPENAME" _
            & "        , SHIPPINGGATE" _
            & "        , AVERAGELOADAMOUNT" _
            & "        , SHIPPINGPLAN" _
            & "        , DELFLG" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , RECEIVEYMD )" _
            & "    VALUES" _
            & "        ( @P01" _
            & "        , @P02" _
            & "        , @P03" _
            & "        , @P04" _
            & "        , @P05" _
            & "        , @P06" _
            & "        , @P07" _
            & "        , @P08" _
            & "        , @P09" _
            & "        , @P10" _
            & "        , @P11" _
            & "        , @P12" _
            & "        , @P13" _
            & "        , @P14" _
            & "        , @P15" _
            & "        , @P16" _
            & "        , @P17" _
            & "        , @P18" _
            & "        , @P19" _
            & "        , @P20" _
            & "        , @P21" _
            & "        , @P22" _
            & "        , @P23" _
            & "        , @P32" _
            & "        , @P33" _
            & "        , @P34" _
            & "        , @P35" _
            & "        , @P36" _
            & "        , @P37" _
            & "        , @P24" _
            & "        , @P25" _
            & "        , @P26" _
            & "        , @P27" _
            & "        , @P31 );" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    OFFICECODE" _
            & "    , SHIPPERCODE" _
            & "    , PLANTCODE" _
            & "    , BIGOILCODE" _
            & "    , BIGOILNAME" _
            & "    , BIGOILKANA" _
            & "    , MIDDLEOILCODE" _
            & "    , MIDDLEOILNAME" _
            & "    , MIDDLEOILKANA" _
            & "    , OILCODE" _
            & "    , OILNAME" _
            & "    , OILKANA" _
            & "    , SEGMENTOILCODE" _
            & "    , SEGMENTOILNAME" _
            & "    , OTOILCODE" _
            & "    , OTOILNAME" _
            & "    , SHIPPEROILCODE" _
            & "    , SHIPPEROILNAME" _
            & "    , CHECKOILCODE" _
            & "    , CHECKOILNAME" _
            & "    , STOCKFLG" _
            & "    , ORDERFROMDATE" _
            & "    , ORDERTODATE" _
            & "    , REPORTOILNAME" _
            & "    , JROILTYPE" _
            & "    , JROILTYPENAME" _
            & "    , SHIPPINGGATE" _
            & "    , AVERAGELOADAMOUNT" _
            & "    , SHIPPINGPLAN" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    OIL.OIM0003_PRODUCT" _
            & " WHERE" _
            & "    OFFICECODE         = @P01" _
            & "    AND SHIPPERCODE    = @P02" _
            & "    AND PLANTCODE      = @P03" _
            & "    AND OILCODE        = @P04" _
            & "    AND SEGMENTOILCODE = @P05;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)           '営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)          '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 4)           '基地コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)           '油種大分類コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 10)          '油種大分類名
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10)          '油種大分類名カナ
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 1)           '油種中分類コード
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 20)          '油種中分類名
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)          '油種中分類名カナ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4)           '油種コード
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 40)          '油種名
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40)          '油種名カナ
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 1)           '油種細分コード
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 40)          '油種名（細分）
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4)           'OT油種コード
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 10)          'OT油種名
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)          '荷主油種コード
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 40)          '荷主油種名
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4)           '積込チェック用油種コード
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40)          '積込チェック用油種名
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 1)           '在庫管理対象フラグ
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)                  '受注登録可能期間FROM
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.Date)                  '受注登録可能期間TO
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.DateTime)              '登録年月日
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.NVarChar, 20)          '登録ユーザーID
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.DateTime)              '更新年月日
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.DateTime)              '集信日時

                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 40)          '帳票用油種名
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 1)           'JR油種区分
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 40)          'JR油種区分名
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 40)          '出荷口
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.Float, 2, 1)           '平均積込数量
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.Int)                   '出荷計画枠

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 6)        '営業所コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 10)       '荷主コード
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 4)        '基地コード
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 4)        '油種コード
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 1)        '油種細分コード

                For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows
                    If Trim(OIM0003INProw("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                        Trim(OIM0003INProw("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                        Trim(OIM0003INProw("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                        Dim WW_DATENOW As DateTime = Date.Now

                        'DB更新
                        PARA01.Value = OIM0003INProw("OFFICECODE")
                        PARA02.Value = OIM0003INProw("SHIPPERCODE")
                        PARA03.Value = OIM0003INProw("PLANTCODE")
                        PARA04.Value = OIM0003INProw("BIGOILCODE")
                        PARA05.Value = OIM0003INProw("BIGOILNAME")
                        PARA06.Value = OIM0003INProw("BIGOILKANA")
                        PARA07.Value = OIM0003INProw("MIDDLEOILCODE")
                        PARA08.Value = OIM0003INProw("MIDDLEOILNAME")
                        PARA09.Value = OIM0003INProw("MIDDLEOILKANA")
                        PARA10.Value = OIM0003INProw("OILCODE")
                        PARA11.Value = OIM0003INProw("OILNAME")
                        PARA12.Value = OIM0003INProw("OILKANA")
                        PARA13.Value = OIM0003INProw("SEGMENTOILCODE")
                        PARA14.Value = OIM0003INProw("SEGMENTOILNAME")
                        PARA15.Value = OIM0003INProw("OTOILCODE")
                        PARA16.Value = OIM0003INProw("OTOILNAME")
                        PARA17.Value = OIM0003INProw("SHIPPEROILCODE")
                        PARA18.Value = OIM0003INProw("SHIPPEROILNAME")
                        PARA19.Value = OIM0003INProw("CHECKOILCODE")
                        PARA20.Value = OIM0003INProw("CHECKOILNAME")
                        PARA21.Value = OIM0003INProw("STOCKFLG")

                        If String.IsNullOrEmpty(OIM0003INProw("ORDERFROMDATE")) Then
                            PARA22.Value = DBNull.Value
                        Else
                            PARA22.Value = OIM0003INProw("ORDERFROMDATE")
                        End If

                        If String.IsNullOrEmpty(OIM0003INProw("ORDERTODATE")) Then
                            PARA23.Value = DBNull.Value
                        Else
                            PARA23.Value = OIM0003INProw("ORDERTODATE")
                        End If

                        PARA24.Value = OIM0003INProw("DELFLG")
                        PARA25.Value = WW_DATENOW
                        PARA26.Value = Master.USERID
                        PARA27.Value = Master.USERTERMID
                        PARA28.Value = WW_DATENOW
                        PARA29.Value = Master.USERID
                        PARA30.Value = Master.USERTERMID
                        PARA31.Value = C_DEFAULT_YMD

                        PARA32.Value = OIM0003INProw("REPORTOILNAME")
                        PARA33.Value = OIM0003INProw("JROILTYPE")
                        PARA34.Value = OIM0003INProw("JROILTYPENAME")
                        PARA35.Value = OIM0003INProw("SHIPPINGGATE")
                        If String.IsNullOrEmpty(OIM0003INProw("AVERAGELOADAMOUNT")) Then
                            PARA36.Value = 0.0
                        Else
                            PARA36.Value = OIM0003INProw("AVERAGELOADAMOUNT")
                        End If
                        If String.IsNullOrEmpty(OIM0003INProw("SHIPPINGPLAN")) Then
                            PARA37.Value = 0
                        Else
                            PARA37.Value = OIM0003INProw("SHIPPINGPLAN")
                        End If

                        SQLcmd.CommandTimeout = 300
                        SQLcmd.ExecuteNonQuery()

                        OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                        '更新ジャーナル出力
                        JPARA01.Value = OIM0003INProw("OFFICECODE")
                        JPARA02.Value = OIM0003INProw("SHIPPERCODE")
                        JPARA03.Value = OIM0003INProw("PLANTCODE")
                        JPARA04.Value = OIM0003INProw("OILCODE")
                        JPARA05.Value = OIM0003INProw("SEGMENTOILCODE")

                        Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                            If IsNothing(OIM0003UPDtbl) Then
                                OIM0003UPDtbl = New DataTable

                                For index As Integer = 0 To SQLdr.FieldCount - 1
                                    OIM0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                                Next
                            End If

                            OIM0003UPDtbl.Clear()
                            OIM0003UPDtbl.Load(SQLdr)
                        End Using

                        For Each OIM0003UPDrow As DataRow In OIM0003UPDtbl.Rows
                            CS0020JOURNAL.TABLENM = "OIM0003L"
                            CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                            CS0020JOURNAL.ROW = OIM0003UPDrow
                            CS0020JOURNAL.CS0020JOURNAL()
                            If Not isNormal(CS0020JOURNAL.ERR) Then
                                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

                                WW_ERR_SW = CS0020JOURNAL.ERR
                                Exit Sub
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0003L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 品種出荷期間マスタ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterOilTerm(ByVal SQLcon As SqlConnection)

        '○ DB更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIM0030_OILTERM" _
            & "    WHERE" _
            & "        OFFICECODE         = @P01" _
            & "        AND SHIPPERCODE    = @P02" _
            & "        AND PLANTCODE      = @P03" _
            & "        AND OILCODE        = @P04" _
            & "        AND SEGMENTOILCODE = @P05" _
            & "        AND CONSIGNEECODE  = @P06;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0030_OILTERM" _
            & "    SET" _
            & "        ORDERFROMDATE    = @P07" _
            & "        , ORDERTODATE    = @P08" _
            & "        , DELFLG         = @P09" _
            & "        , UPDYMD         = @P13" _
            & "        , UPDUSER        = @P14" _
            & "        , UPDTERMID      = @P15" _
            & "        , RECEIVEYMD     = @P16" _
            & "    WHERE" _
            & "        OFFICECODE         = @P01" _
            & "        AND SHIPPERCODE    = @P02" _
            & "        AND PLANTCODE      = @P03" _
            & "        AND OILCODE        = @P04" _
            & "        AND SEGMENTOILCODE = @P05" _
            & "        AND CONSIGNEECODE  = @P06;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0030_OILTERM" _
            & "        ( OFFICECODE" _
            & "        , SHIPPERCODE" _
            & "        , PLANTCODE" _
            & "        , OILCODE" _
            & "        , SEGMENTOILCODE" _
            & "        , CONSIGNEECODE" _
            & "        , ORDERFROMDATE" _
            & "        , ORDERTODATE" _
            & "        , DELFLG" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , RECEIVEYMD )" _
            & "    VALUES" _
            & "        ( @P01" _
            & "        , @P02" _
            & "        , @P03" _
            & "        , @P04" _
            & "        , @P05" _
            & "        , @P06" _
            & "        , @P07" _
            & "        , @P08" _
            & "        , @P09" _
            & "        , @P10" _
            & "        , @P11" _
            & "        , @P12" _
            & "        , @P16 );" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "    OFFICECODE" _
            & "    , SHIPPERCODE" _
            & "    , PLANTCODE" _
            & "    , OILCODE" _
            & "    , SEGMENTOILCODE" _
            & "    , CONSIGNEECODE" _
            & "    , ORDERFROMDATE" _
            & "    , ORDERTODATE" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP AS bigint) AS TIMSTP" _
            & " FROM" _
            & "    OIL.OIM0030_OILTERM" _
            & " WHERE" _
            & "    OFFICECODE         = @P01" _
            & "    AND SHIPPERCODE    = @P02" _
            & "    AND PLANTCODE      = @P03" _
            & "    AND OILCODE        = @P04" _
            & "    AND SEGMENTOILCODE = @P05" _
            & "    AND CONSIGNEECODE  = @P06;"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 6)           '営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 10)          '荷主コード
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 4)           '基地コード
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 4)           '油種コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 1)           '油種細分コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 10)          '荷受人コード
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)                  '受注登録可能期間FROM
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)                  '受注登録可能期間TO
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.DateTime)              '登録年月日
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '登録ユーザーID
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.DateTime)              '更新年月日
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          '更新ユーザーID
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.DateTime)              '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 6)        '営業所コード
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 10)       '荷主コード
                Dim JPARA03 As SqlParameter = SQLcmdJnl.Parameters.Add("@P03", SqlDbType.NVarChar, 4)        '基地コード
                Dim JPARA04 As SqlParameter = SQLcmdJnl.Parameters.Add("@P04", SqlDbType.NVarChar, 4)        '油種コード
                Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P05", SqlDbType.NVarChar, 1)        '油種細分コード
                Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P06", SqlDbType.NVarChar, 10)       '荷受人コード

                For Each OIM0030INProw As DataRow In OIM0030INPtbl.Rows

                    Dim WW_DATENOW As DateTime = Date.Now

                    '受注登録可能期間FROM、同TO、削除フラグのいずれの項目も設定されていない行は更新対象外
                    If String.IsNullOrEmpty(OIM0030INProw("ORDERFROMDATE")) AndAlso
                        String.IsNullOrEmpty(OIM0030INProw("ORDERTODATE")) AndAlso
                        String.IsNullOrEmpty(OIM0030INProw("DELFLG")) Then
                        Continue For
                    End If

                    'DB更新
                    PARA01.Value = OIM0030INProw("OFFICECODE")
                    PARA02.Value = OIM0030INProw("SHIPPERCODE")
                    PARA03.Value = OIM0030INProw("PLANTCODE")
                    PARA04.Value = OIM0030INProw("OILCODE")
                    PARA05.Value = OIM0030INProw("SEGMENTOILCODE")
                    PARA06.Value = OIM0030INProw("CONSIGNEECODE")
                    PARA07.Value = OIM0030INProw("ORDERFROMDATE")
                    PARA08.Value = OIM0030INProw("ORDERTODATE")
                    PARA09.Value = OIM0030INProw("DELFLG")
                    PARA10.Value = WW_DATENOW
                    PARA11.Value = Master.USERID
                    PARA12.Value = Master.USERTERMID
                    PARA13.Value = WW_DATENOW
                    PARA14.Value = Master.USERID
                    PARA15.Value = Master.USERTERMID
                    PARA16.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    '更新ジャーナル出力
                    JPARA01.Value = OIM0030INProw("OFFICECODE")
                    JPARA02.Value = OIM0030INProw("SHIPPERCODE")
                    JPARA03.Value = OIM0030INProw("PLANTCODE")
                    JPARA04.Value = OIM0030INProw("OILCODE")
                    JPARA05.Value = OIM0030INProw("SEGMENTOILCODE")
                    JPARA06.Value = OIM0030INProw("CONSIGNEECODE")

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIM0030UPDtbl) Then
                            OIM0030UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIM0030UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIM0030UPDtbl.Clear()
                        OIM0030UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIM0030UPDrow As DataRow In OIM0030UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIM0030_OILTERM"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIM0030UPDrow
                        CS0020JOURNAL.CS0020JOURNAL()
                        If Not isNormal(CS0020JOURNAL.ERR) Then
                            Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                            CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

                            WW_ERR_SW = CS0020JOURNAL.ERR
                            Exit Sub
                        End If
                    Next
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0003C OIM0030_OILTERM UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0003C OIM0030_OILTERM UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        Dim dateErrFlag As String = ""
        Dim WW_UniqueKeyCHECK As String = ""

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
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIM0003INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("DELFLG")) Then
                    '値存在チェック
                    CODENAME_get("DELFLG", OIM0003INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(削除フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '営業所コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OFFICECODE", OIM0003INProw("OFFICECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("OFFICECODE")) Then
                    '値存在チェック
                    CODENAME_get("OFFICECODE", OIM0003INProw("OFFICECODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(営業所コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(営業所コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SHIPPERCODE", OIM0003INProw("SHIPPERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("SHIPPERCODE")) Then
                    '値存在チェック
                    CODENAME_get("SHIPPERCODE", OIM0003INProw("SHIPPERCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷主コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '基地コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PLANTCODE", OIM0003INProw("PLANTCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("PLANTCODE")) Then
                    '値存在チェック
                    CODENAME_get("PLANTCODE", OIM0003INProw("PLANTCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(基地コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGOILCODE", OIM0003INProw("BIGOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("BIGOILCODE")) Then
                    '値存在チェック
                    CODENAME_get("BIGOILCODE", OIM0003INProw("BIGOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGOILNAME", OIM0003INProw("BIGOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種大分類名カナ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGOILKANA", OIM0003INProw("BIGOILKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MIDDLEOILCODE", OIM0003INProw("MIDDLEOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("MIDDLEOILCODE")) Then
                    '値存在チェック
                    CODENAME_get("MIDDLEOILCODE", OIM0003INProw("MIDDLEOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MIDDLEOILNAME", OIM0003INProw("MIDDLEOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種中分類名カナ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MIDDLEOILKANA", OIM0003INProw("MIDDLEOILKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OILCODE", OIM0003INProw("OILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OILNAME", OIM0003INProw("OILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名カナ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OILKANA", OIM0003INProw("OILKANA"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種細分コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SEGMENTOILCODE", OIM0003INProw("SEGMENTOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種細分コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種名（細分）(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SEGMENTOILNAME", OIM0003INProw("SEGMENTOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種名（細分）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'OT油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OTOILCODE", OIM0003INProw("OTOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("OTOILCODE")) Then
                    '値存在チェック
                    CODENAME_get("OTOILCODE", OIM0003INProw("OTOILCODE"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(OT油種コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(OT油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'OT油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "OTOILNAME", OIM0003INProw("OTOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(OT油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SHIPPEROILCODE", OIM0003INProw("SHIPPEROILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '荷主油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "SHIPPEROILNAME", OIM0003INProw("SHIPPEROILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷主油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CHECKOILCODE", OIM0003INProw("CHECKOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '積込チェック用油種名(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CHECKOILNAME", OIM0003INProw("CHECKOILNAME"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(積込チェック用油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '在庫管理対象フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STOCKFLG", OIM0003INProw("STOCKFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("STOCKFLG")) Then
                    '値存在チェック
                    CODENAME_get("STOCKFLG", OIM0003INProw("STOCKFLG"), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(在庫管理対象フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(在庫管理対象フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間FROM(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORDERFROMDATE", OIM0003INProw("ORDERFROMDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("ORDERFROMDATE")) Then
                    '年月日チェック
                    WW_CheckDate(OIM0003INProw("ORDERFROMDATE"), "受注登録可能期間FROM", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0003INProw("ORDERFROMDATE") = CDate(OIM0003INProw("ORDERFROMDATE")).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間TO(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORDERTODATE", OIM0003INProw("ORDERTODATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(OIM0003INProw("ORDERTODATE")) Then
                    '年月日チェック
                    WW_CheckDate(OIM0003INProw("ORDERTODATE"), "受注登録可能期間TO", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(受注登録可能期間TO入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKERR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0003INProw("ORDERTODATE") = CDate(OIM0003INProw("ORDERTODATE")).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(受注登録可能期間TO入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注登録可能期間FROM-TOチェック
            If Not String.IsNullOrEmpty(OIM0003INProw("ORDERFROMDATE")) AndAlso Not String.IsNullOrEmpty(OIM0003INProw("ORDERTODATE")) Then
                If CDate(OIM0003INProw("ORDERFROMDATE")).CompareTo(CDate(OIM0003INProw("ORDERTODATE"))) > 0 Then
                    WW_CheckMES1 = "・更新できないレコード(受注登録可能期間FROM-TO入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                End If
            End If

            '帳票用油種名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("REPORTOILNAME")
            Master.CheckField(Master.USERCAMP, "REPORTOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(帳票用油種名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JR油種区分(バリデーションチェック）
            WW_TEXT = OIM0003INProw("JROILTYPE")
            Master.CheckField(Master.USERCAMP, "JROILTYPE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("JROILTYPE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(JR油種区分入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(JR油種区分入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'JR油種区分名(バリデーションチェック）
            WW_TEXT = OIM0003INProw("JROILTYPENAME")
            Master.CheckField(Master.USERCAMP, "JROILTYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR油種区分名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '出荷口(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SHIPPINGGATE")
            Master.CheckField(Master.USERCAMP, "SHIPPINGGATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    '値存在チェック
                    CODENAME_get("SHIPPINGGATE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(出荷口入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷口入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '平均積込数量(バリデーションチェック）
            WW_TEXT = OIM0003INProw("AVERAGELOADAMOUNT")
            Master.CheckField(Master.USERCAMP, "AVERAGELOADAMOUNT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(平均積込数量入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '出荷計画枠(バリデーションチェック）
            WW_TEXT = OIM0003INProw("SHIPPINGPLAN")
            Master.CheckField(Master.USERCAMP, "SHIPPINGPLAN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If Not String.IsNullOrEmpty(WW_TEXT) Then
                    Try
                        Int32.Parse(WW_TEXT)
                    Catch ex As Exception
                        WW_CheckMES1 = "・更新できないレコード(出荷計画枠数値変換エラー)です。"
                        WW_CheckMES2 = ex.Message
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End Try
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(出荷計画枠入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、更新内容チェックを行う
            If OIM0003INProw("OFFICECODE") = work.WF_SEL_OFFICECODE2.Text AndAlso
                OIM0003INProw("SHIPPERCODE") = work.WF_SEL_SHIPPERCODE2.Text AndAlso
                OIM0003INProw("PLANTCODE") = work.WF_SEL_PLANTCODE2.Text AndAlso
                OIM0003INProw("OILCODE") = work.WF_SEL_OILCODE2.Text AndAlso
                OIM0003INProw("SEGMENTOILCODE") = work.WF_SEL_SEGMENTOILCODE.Text AndAlso
                OIM0003INProw("DELFLG") = work.WF_SEL_DELFLG2.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                   "([" & OIM0003INProw("OFFICECODE") & "]" &
                                   "([" & OIM0003INProw("SHIPPERCODE") & "]" &
                                   "([" & OIM0003INProw("PLANTCODE") & "]" &
                                   "([" & OIM0003INProw("OILCODE") & "]" &
                                   "([" & OIM0003INProw("SEGMENTOILCODE") & "]" &
                                   " [" & OIM0003INProw("DELFLG") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0003INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            '品種出荷期間テーブルのチェック
            If WW_LINE_ERR = "" AndAlso OIM0030INPtbl.Rows.Count > 0 Then
                For Each OIM0030INProw As DataRow In OIM0030INPtbl.Rows

                    '受注登録可能期間FROM、同TO、削除フラグのいずれの項目も設定されていない行はチェック対象外
                    If String.IsNullOrEmpty(OIM0030INProw("ORDERFROMDATE")) AndAlso
                        String.IsNullOrEmpty(OIM0030INProw("ORDERTODATE")) AndAlso
                        String.IsNullOrEmpty(OIM0030INProw("DELFLG")) Then
                        Continue For
                    End If

                    '品種出荷期間テーブルの受注登録可能期間FROM(バリデーションチェック）
                    Master.CheckField(Master.USERCAMP, "OILTERM_ORDERFROMDATE", OIM0030INProw("ORDERFROMDATE"),
                                      WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If isNormal(WW_CS0024FCHECKERR) Then
                        If Not String.IsNullOrEmpty(OIM0030INProw("ORDERFROMDATE")) Then
                            '年月日チェック
                            WW_CheckDate(OIM0030INProw("ORDERFROMDATE"),
                                         "品種出荷期間.受注登録可能期間FROM",
                                         WW_CS0024FCHECKERR, dateErrFlag)
                            If dateErrFlag = "1" Then
                                WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間FROM入力エラー)です。"
                                WW_CheckMES2 = WW_CS0024FCHECKERR
                                WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                                WW_LINE_ERR = "ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            Else
                                '品種マスタと品種出荷期間マスタの受注登録可能期間FROM相関チェック
                                If Not String.IsNullOrEmpty(OIM0003INProw("ORDERFROMDATE")) Then
                                    If CDate(OIM0030INProw("ORDERFROMDATE")).CompareTo(CDate(OIM0003INProw("ORDERFROMDATE"))) < 0 Then
                                        WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間FROM < 品種.受注登録可能期間FROM)です。"
                                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                        WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                                        WW_LINE_ERR = "ERR"
                                        O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                                    End If
                                End If
                            End If
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間FROM入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                        WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If

                    '品種出荷期間テーブルの受注登録可能期間TO(バリデーションチェック）
                    Master.CheckField(Master.USERCAMP, "OILTERM_ORDERTODATE", OIM0030INProw("ORDERTODATE"),
                                      WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If isNormal(WW_CS0024FCHECKERR) Then
                        If Not String.IsNullOrEmpty(OIM0030INProw("ORDERTODATE")) Then
                            '年月日チェック
                            WW_CheckDate(OIM0030INProw("ORDERTODATE"), "品種出荷期間.受注登録可能期間TO",
                                         WW_CS0024FCHECKERR, dateErrFlag)
                            If dateErrFlag = "1" Then
                                WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間TO入力エラー)です。"
                                WW_CheckMES2 = WW_CS0024FCHECKERR
                                WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                                WW_LINE_ERR = "ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            Else
                                '品種マスタと品種出荷期間マスタの受注登録可能期間TO相関チェック
                                If Not String.IsNullOrEmpty(OIM0003INProw("ORDERTODATE")) Then
                                    If CDate(OIM0030INProw("ORDERTODATE")).CompareTo(CDate(OIM0003INProw("ORDERTODATE"))) > 0 Then
                                        WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間TO > 品種.受注登録可能期間TO)です。"
                                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                                        WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                                        WW_LINE_ERR = "ERR"
                                        O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                                    End If
                                End If
                            End If
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間TO入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                        WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If

                    '品種出荷期間テーブルの受注登録可能期間FROM-TOチェック
                    If Not String.IsNullOrEmpty(OIM0030INProw("ORDERFROMDATE")) AndAlso
                        Not String.IsNullOrEmpty(OIM0030INProw("ORDERTODATE")) Then
                        If CDate(OIM0030INProw("ORDERFROMDATE")).CompareTo(CDate(OIM0030INProw("ORDERTODATE"))) > 0 Then
                            WW_CheckMES1 = "・更新できないレコード(品種出荷期間.受注登録可能期間FROM > 同TO)です。"
                            WW_CheckMES2 = WW_CS0024FCHECKREPORT
                            WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.START_END_DATE_RELATION_ERROR
                        End If
                    End If

                    '削除フラグ(バリデーションチェック）
                    Master.CheckField(Master.USERCAMP, "DELFLG", OIM0030INProw("DELFLG"),
                                      WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                    If isNormal(WW_CS0024FCHECKERR) Then
                        If Not String.IsNullOrEmpty(OIM0030INProw("DELFLG")) Then
                            '値存在チェック
                            CODENAME_get("DELFLG", OIM0030INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                            If Not isNormal(WW_RTN_SW) Then
                                WW_CheckMES1 = "・更新できないレコード(品種出荷期間.削除フラグ入力エラー)です。"
                                WW_CheckMES2 = "マスタに存在しません。"
                                WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                                WW_LINE_ERR = "ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            Else
                                '品種マスタ本体が削除されているのに、品種出荷期間マスタが削除でない場合
                                If OIM0003INProw("DELFLG") = C_DELETE_FLG.DELETE AndAlso
                                    OIM0030INProw("DELFLG") <> C_DELETE_FLG.DELETE Then
                                    WW_CheckMES1 = "・更新できないレコード(品種出荷期間.削除フラグ不一致エラー)です。"
                                    WW_CheckMES2 = "品種マスタの削除フラグが無効に設定されています。"
                                    WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                                    WW_LINE_ERR = "ERR"
                                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                                End If
                            End If
                        End If
                    Else
                        WW_CheckMES1 = "・更新できないレコード(品種出荷期間.削除フラグ入力エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                        WW_CheckERR_OILTERM(WW_CheckMES1, WW_CheckMES2, OIM0030INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Next
            End If

            If WW_LINE_ERR = "" Then
                If OIM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0003INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0003row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0003row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0003row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 営業所コード             =" & OIM0003row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード               =" & OIM0003row("SHIPPERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード               =" & OIM0003row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類コード         =" & OIM0003row("BIGOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名             =" & OIM0003row("BIGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名カナ         =" & OIM0003row("BIGOILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類コード         =" & OIM0003row("MIDDLEOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名             =" & OIM0003row("MIDDLEOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名カナ         =" & OIM0003row("MIDDLEOILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード               =" & OIM0003row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名                   =" & OIM0003row("OILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名カナ               =" & OIM0003row("OILKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種細分コード           =" & OIM0003row("SEGMENTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種名（細分）           =" & OIM0003row("SEGMENTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT油種コード             =" & OIM0003row("OTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT油種名                 =" & OIM0003row("OTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主油種コード           =" & OIM0003row("SHIPPEROILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主油種名               =" & OIM0003row("SHIPPEROILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種コード =" & OIM0003row("CHECKOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 積込チェック用油種名     =" & OIM0003row("CHECKOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 在庫管理対象フラグ       =" & OIM0003row("STOCKFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間FROM     =" & OIM0003row("ORDERFROMDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間TO       =" & OIM0003row("ORDERTODATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 帳票用油種名             =" & OIM0003row("REPORTOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR油種区分               =" & OIM0003row("JROILTYPE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR油種区分名             =" & OIM0003row("JROILTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出荷口                   =" & OIM0003row("SHIPPINGGATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 平均積込数量             =" & OIM0003row("AVERAGELOADAMOUNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出荷計画枠               =" & OIM0003row("SHIPPINGPLAN") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ               =" & OIM0003row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(品種出荷期間マスタ)
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0030row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR_OILTERM(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0030row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0030row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 営業所コード             =" & OIM0030row("OFFICECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷主コード               =" & OIM0030row("SHIPPERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 基地コード               =" & OIM0030row("PLANTCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種コード               =" & OIM0030row("OILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種細分コード           =" & OIM0030row("SEGMENTOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷受人コード             =" & OIM0030row("CONSIGNEECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間FROM     =" & OIM0030row("ORDERFROMDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 受注登録可能期間TO       =" & OIM0030row("ORDERTODATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ               =" & OIM0030row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0003tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0003tbl_UPD()

        '○ 画面状態設定
        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            Select Case OIM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA, C_LIST_OPERATION_CODE.NODISP, C_LIST_OPERATION_CODE.SELECTED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0003INProw As DataRow In OIM0003INPtbl.Rows
            TBL_REPLACE_SUB(OIM0003INProw)
        Next

    End Sub

    ''' <summary>
    ''' 品種出荷期間M更新レコードを品種Mレコードにコピーする
    ''' </summary>
    ''' <param name="OIM0003row">品種Mのレコード行</param>
    Private Sub CopyOIM0030RowToOIM0003Row(ByRef OIM0003row As DataRow)

        If OIM0030INPtbl.Rows.Count > 0 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(0)
            OIM0003row("OILTERM_CONSIGNEECODE_01") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_01") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_01") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_01") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_01") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 1 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(1)
            OIM0003row("OILTERM_CONSIGNEECODE_02") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_02") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_02") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_02") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_02") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 2 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(2)
            OIM0003row("OILTERM_CONSIGNEECODE_03") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_03") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_03") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_03") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_03") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 3 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(3)
            OIM0003row("OILTERM_CONSIGNEECODE_04") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_04") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_04") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_04") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_04") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 4 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(4)
            OIM0003row("OILTERM_CONSIGNEECODE_05") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_05") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_05") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_05") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_05") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 5 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(5)
            OIM0003row("OILTERM_CONSIGNEECODE_06") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_06") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_06") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_06") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_06") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 6 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(6)
            OIM0003row("OILTERM_CONSIGNEECODE_07") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_07") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_07") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_07") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_07") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 7 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(7)
            OIM0003row("OILTERM_CONSIGNEECODE_08") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_08") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_08") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_08") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_08") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 8 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(8)
            OIM0003row("OILTERM_CONSIGNEECODE_09") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_09") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_09") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_09") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_09") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 9 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(9)
            OIM0003row("OILTERM_CONSIGNEECODE_10") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_10") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_10") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_10") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_10") = OIM0030INProw("DELFLG")
        End If

        If OIM0030INPtbl.Rows.Count > 10 Then
            Dim OIM0030INProw As DataRow = OIM0030INPtbl.Rows(10)
            OIM0003row("OILTERM_CONSIGNEECODE_11") = OIM0030INProw("CONSIGNEECODE")
            OIM0003row("OILTERM_CONSIGNEENAME_11") = OIM0030INProw("CONSIGNEENAME")
            OIM0003row("OILTERM_ORDERFROMDATE_11") = OIM0030INProw("ORDERFROMDATE")
            OIM0003row("OILTERM_ORDERTODATE_11") = OIM0030INProw("ORDERTODATE")
            OIM0003row("OILTERM_DELFLG_11") = OIM0030INProw("DELFLG")
        End If

    End Sub

    ''' <summary>
    ''' 更新データの一覧更新・追加処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_REPLACE_SUB(ByRef OIM0003INProw As DataRow)

        Dim WK_FOUNDFLG As Boolean = False  '同一キーレコード発見フラグ

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then

                '同一キーレコード発見
                WK_FOUNDFLG = True

                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 0
                OIM0003INProw("HIDDEN") = 0

                'テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray

                '〇 名称設定
                '営業所
                CODENAME_get("OFFICECODE", OIM0003row("OFFICECODE"), OIM0003row("OFFICENAME"), WW_DUMMY)
                '荷主
                CODENAME_get("SHIPPERCODE", OIM0003row("SHIPPERCODE"), OIM0003row("SHIPPERNAME"), WW_DUMMY)
                '基地
                CODENAME_get("PLANTCODE", OIM0003row("PLANTCODE"), OIM0003row("PLANTNAME"), WW_DUMMY)
                '在庫管理対象フラグ
                CODENAME_get("STOCKFLG", OIM0003row("STOCKFLG"), OIM0003row("STOCKFLGNAME"), WW_DUMMY)
                'JR油種区分名
                CODENAME_get("JROILTYPE", OIM0003row("JROILTYPE"), OIM0003row("JROILTYPENAME"), WW_DUMMY)

                '品種出荷期間マスタ項目設定
                CopyOIM0030RowToOIM0003Row(OIM0003row)

                Exit For
            End If
        Next

        '同一キーレコードが発見できなかった場合、一覧に行を追加する
        If Not WK_FOUNDFLG Then

            Dim OIM0003row As DataRow = OIM0003tbl.NewRow

            'テーブル項目設定
            OIM0003row.ItemArray = OIM0003INProw.ItemArray

            '画面入力テーブル項目設定
            OIM0003row("LINECNT") = OIM0003tbl.Rows.Count + 1   '末尾に追加
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
            OIM0003row("TIMSTP") = "0"
            OIM0003row("SELECT") = 0
            OIM0003row("HIDDEN") = 0

            '〇 名称設定
            '営業所
            CODENAME_get("OFFICECODE", OIM0003row("OFFICECODE"), OIM0003row("OFFICENAME"), WW_DUMMY)
            '荷主
            CODENAME_get("SHIPPERCODE", OIM0003row("SHIPPERCODE"), OIM0003row("SHIPPERNAME"), WW_DUMMY)
            '基地
            CODENAME_get("PLANTCODE", OIM0003row("PLANTCODE"), OIM0003row("PLANTNAME"), WW_DUMMY)
            '在庫管理対象フラグ
            CODENAME_get("STOCKFLG", OIM0003row("STOCKFLG"), OIM0003row("STOCKFLGNAME"), WW_DUMMY)
            'JR油種区分名
            CODENAME_get("JROILTYPE", OIM0003row("JROILTYPE"), OIM0003row("JROILTYPENAME"), WW_DUMMY)

            '品種出荷期間マスタ項目設定
            CopyOIM0030RowToOIM0003Row(OIM0003row)

            OIM0003tbl.Rows.Add(OIM0003row)

        End If

    End Sub

#Region "不使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0003INProw As DataRow)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 1
                OIM0003INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0003INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0003row As DataRow = OIM0003tbl.NewRow
        OIM0003row.ItemArray = OIM0003INProw.ItemArray

        OIM0003row("LINECNT") = OIM0003tbl.Rows.Count + 1
        If OIM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0003row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0003row("TIMSTP") = "0"
        OIM0003row("SELECT") = 1
        OIM0003row("HIDDEN") = 0

        OIM0003tbl.Rows.Add(OIM0003row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0003INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0003INProw As DataRow)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows

            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                '画面入力テーブル項目設定
                OIM0003INProw("LINECNT") = OIM0003row("LINECNT")
                OIM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0003INProw("TIMSTP") = OIM0003row("TIMSTP")
                OIM0003INProw("SELECT") = 1
                OIM0003INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0003row.ItemArray = OIM0003INProw.ItemArray
                Exit For
            End If
        Next

    End Sub
#End Region

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
                Case "OFFICECODE"
                    '営業所コード
                    prmData = work.CreateSALESOFFICEParam(Master.USERCAMP, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPERCODE"
                    '荷主コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "JOINTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_JOINTLIST, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "PLANTCODE"
                    '基地コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PLANTMASTER")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BIGOILCODE"
                    '油種大分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "BIGOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MIDDLEOILCODE"
                    '油種中分類コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLEOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OTOILCODE"
                    'OT油種コード
                    prmData = work.CreateFIXParam(Master.USERCAMP, "OTOILCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STOCKFLG"
                    '在庫管理対象フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "PRODUCTSTOCKFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "JROILTYPE"
                    'JR油種区分
                    prmData = work.CreateFIXParam(Master.USERCAMP, "JROILTYPE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SHIPPINGGATE"
                    '出荷口
                    prmData = work.CreateFIXParam(Master.USERCAMP, "SHIPPINGGATE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"
                    '削除フラグ
                    prmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 入力データ変更確認
    ''' </summary>
    Private Function isInputChange() As Boolean

        '変更フラグ
        Dim inputChangeFlg = True

        Dim OIM0003INProw As DataRow = OIM0003INPtbl.Rows(0)

        For Each OIM0003row As DataRow In OIM0003tbl.Rows
            '同一レコードか判定
            If OIM0003INProw("OFFICECODE") = OIM0003row("OFFICECODE") AndAlso
                OIM0003INProw("SHIPPERCODE") = OIM0003row("SHIPPERCODE") AndAlso
                OIM0003INProw("PLANTCODE") = OIM0003row("PLANTCODE") AndAlso
                OIM0003INProw("OILCODE") = OIM0003row("OILCODE") AndAlso
                OIM0003INProw("SEGMENTOILCODE") = OIM0003row("SEGMENTOILCODE") Then
                'キー以外の内容が同一の場合、変更フラグをOFFにする
                If OIM0003row("BIGOILCODE") = OIM0003INProw("BIGOILCODE") AndAlso
                    OIM0003row("BIGOILNAME") = OIM0003INProw("BIGOILNAME") AndAlso
                    OIM0003row("BIGOILKANA") = OIM0003INProw("BIGOILKANA") AndAlso
                    OIM0003row("MIDDLEOILCODE") = OIM0003INProw("MIDDLEOILCODE") AndAlso
                    OIM0003row("MIDDLEOILNAME") = OIM0003INProw("MIDDLEOILNAME") AndAlso
                    OIM0003row("MIDDLEOILKANA") = OIM0003INProw("MIDDLEOILKANA") AndAlso
                    OIM0003row("OILNAME") = OIM0003INProw("OILNAME") AndAlso
                    OIM0003row("OILKANA") = OIM0003INProw("OILKANA") AndAlso
                    OIM0003row("SEGMENTOILNAME") = OIM0003INProw("SEGMENTOILNAME") AndAlso
                    OIM0003row("OTOILCODE") = OIM0003INProw("OTOILCODE") AndAlso
                    OIM0003row("OTOILNAME") = OIM0003INProw("OTOILNAME") AndAlso
                    OIM0003row("SHIPPEROILCODE") = OIM0003INProw("SHIPPEROILCODE") AndAlso
                    OIM0003row("SHIPPEROILNAME") = OIM0003INProw("SHIPPEROILNAME") AndAlso
                    OIM0003row("CHECKOILCODE") = OIM0003INProw("CHECKOILCODE") AndAlso
                    OIM0003row("CHECKOILNAME") = OIM0003INProw("CHECKOILNAME") AndAlso
                    OIM0003row("STOCKFLG") = OIM0003INProw("STOCKFLG") AndAlso
                    OIM0003row("ORDERFROMDATE") = OIM0003INProw("ORDERFROMDATE") AndAlso
                    OIM0003row("ORDERTODATE") = OIM0003INProw("ORDERTODATE") AndAlso
                    OIM0003row("REPORTOILNAME") = OIM0003INProw("REPORTOILNAME") AndAlso
                    OIM0003row("JROILTYPE") = OIM0003INProw("JROILTYPE") AndAlso
                    OIM0003row("JROILTYPENAME") = OIM0003INProw("JROILTYPENAME") AndAlso
                    OIM0003row("SHIPPINGGATE") = OIM0003INProw("SHIPPINGGATE") AndAlso
                    OIM0003row("AVERAGELOADAMOUNT") = OIM0003INProw("AVERAGELOADAMOUNT") AndAlso
                    OIM0003row("SHIPPINGPLAN") = OIM0003INProw("SHIPPINGPLAN") AndAlso
                    OIM0003row("DELFLG") = OIM0003INProw("DELFLG") AndAlso
                    Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIM0003row("OPERATION")) Then

                    '品種出荷期間マスタの内容を比較
                    Dim changeflg As Boolean = False
                    For i As Integer = 0 To OIM0030INPtbl.Rows.Count - 1
                        Dim strIdx = String.Format("{0:00}", i + 1)
                        If Not (
                            OIM0003row("OILTERM_CONSIGNEECODE_" + strIdx) = OIM0030INPtbl.Rows(i)("CONSIGNEECODE") AndAlso
                            OIM0003row("OILTERM_CONSIGNEENAME_" + strIdx) = OIM0030INPtbl.Rows(i)("CONSIGNEENAME") AndAlso
                            OIM0003row("OILTERM_ORDERFROMDATE_" + strIdx) = OIM0030INPtbl.Rows(i)("ORDERFROMDATE") AndAlso
                            OIM0003row("OILTERM_ORDERTODATE_" + strIdx) = OIM0030INPtbl.Rows(i)("ORDERTODATE") AndAlso
                            OIM0003row("OILTERM_DELFLG_" + strIdx) = OIM0030INPtbl.Rows(i)("DELFLG")
                        ) Then
                            changeflg = True
                            Exit For
                        End If
                    Next

                    inputChangeFlg = changeflg

                    Exit For
                End If

            End If
        Next

        Return inputChangeFlg
    End Function

End Class

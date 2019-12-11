''************************************************************
' ユーザIDマスタメンテ登録画面
' 作成日 2019/11/14
' 更新日 2019/11/14
' 作成者 JOT遠藤
' 更新車 JOT遠藤
'
' 修正履歴:
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' ユーザIDマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIT0002LinkDetail
    Inherits Page

    '○ 検索結果格納Table
    Private OIT0002tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0002INPtbl As DataTable                              'チェック用テーブル
    Private OIT0002UPDtbl As DataTable                              '更新用テーブル
    Private OIT0002WKtbl As DataTable                               '作業用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 7                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    Private Const CONST_TxtHTank As String = "1001"                 '油種(ハイオク)
    Private Const CONST_TxtRTank As String = "1101"                 '油種(レギュラー)
    Private Const CONST_TxtTTank As String = "1301"                 '油種(灯油)
    Private Const CONST_TxtMTTank As String = "1302"                '油種(未添加灯油)
    Private Const CONST_TxtKTank1 As String = "1401"                '油種(軽油)
    Private Const CONST_TxtKTank2 As String = "1406"
    Private Const CONST_TxtK3Tank1 As String = "1404"               '３号軽油
    Private Const CONST_TxtK3Tank2 As String = "1405"
    Private Const CONST_TxtK5Tank As String = "1402"                '軽油５
    Private Const CONST_TxtK10Tank As String = "1403"               '軽油１０
    Private Const CONST_TxtLTank1 As String = "2201"                'ＬＳＡ
    Private Const CONST_TxtLTank2 As String = "2202"
    Private Const CONST_TxtATank As String = "2101"                 'Ａ重油

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
                    Master.RecoverTable(OIT0002tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonRegister"          '登録ボタン押下
                            WF_ButtonINSERT_Click()
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
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            WF_FILEUPLOAD()
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

            '○ 作成モード(１：新規登録, ２：更新)設定
            If work.WF_SEL_CREATEFLG.Text = "1" Then
                WF_CREATEFLG.Value = "1"
            Else
                WF_CREATEFLG.Value = "2"
            End If
        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0002tbl) Then
                OIT0002tbl.Clear()
                OIT0002tbl.Dispose()
                OIT0002tbl = Nothing
            End If

            If Not IsNothing(OIT0002INPtbl) Then
                OIT0002INPtbl.Clear()
                OIT0002INPtbl.Dispose()
                OIT0002INPtbl = Nothing
            End If

            If Not IsNothing(OIT0002UPDtbl) Then
                OIT0002UPDtbl.Clear()
                OIT0002UPDtbl.Dispose()
                OIT0002UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0002WRKINC.MAPIDD
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
        'If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0002S Then
        'Grid情報保存先のファイル名

        Master.CreateXMLSaveFile()

        'ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0002D Then
        '    Master.RecoverTable(OIT0002tbl, work.WF_SEL_INPTBL.Text)
        'End If

        '登録営業所
        'TxtOrderOffice.Text = work.WF_SEL_ORDERSALESOFFICE.Text
        '本線列車
        TxtHeadOfficeTrain.Text = work.WF_SEL_TRAINNO2.Text
        '空車発駅
        TxtDepstation.Text = work.WF_SEL_DEPSTATION2.Text
        '空車着駅
        TxtRetstation.Text = work.WF_SEL_RETSTATION.Text
        '空車着日（予定）
        TxtEmpDate.Text = work.WF_SEL_EMPARRDATE.Text
        '空車着日（実績）
        TxtActEmpDate.Text = work.WF_SEL_ACTUALEMPARRDATE.Text

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

        '〇営業所配下情報を取得・設定
        Dim WW_GetValue() As String = {"", "", "", "", ""}
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PATTERNMASTER", work.WF_SEL_OFFICECODE.Text, WW_GetValue)
        Dim WF_SEL_SHIPPERSCODE As String = WW_GetValue(0)
        Dim WF_SEL_SHIPPERSNAME As String = WW_GetValue(1)
        Dim WF_SEL_BASECODE As String = WW_GetValue(2)
        Dim WF_SEL_BASENAME As String = WW_GetValue(3)
        Dim WF_SEL_CONSIGNEECODE As String = WW_GetValue(4)
        Dim WF_SEL_CONSIGNEENAME As String = ""

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("ORG", work.WF_SEL_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)
        '登録営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_DUMMY)
        work.WF_SEL_OFFICECODE.Text = TxtOrderOffice.Text
        '空車発駅
        CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
        work.WF_SEL_DEPSTATION.Text = TxtDepstation.Text
        '空車着駅
        CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)
        work.WF_SEL_RETSTATION.Text = TxtRetstation.Text

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0002tbl)

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
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection, ByVal O_INSCNT As Integer)

        If IsNothing(OIT0002tbl) Then
            OIT0002tbl = New DataTable
        End If

        If OIT0002tbl.Columns.Count <> 0 Then
            OIT0002tbl.Columns.Clear()
        End If

        OIT0002tbl.Clear()

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        '○ 取得SQL
        '　検索説明　：　受注№の連番を決める
        Dim SQLStrNum As String =
        " SELECT " _
            & " ISNULL(FORMAT(MAX(SUBSTRING(OIT0004.LINKNO, 10, 2)) + 1,'00'),'01') AS LINKNO_NUM " _
            & " FROM OIL.OIT0004_LINK OIT0004 " _
            & " WHERE SUBSTRING(OIT0004.LINKNO, 2, 8) = FORMAT(GETDATE(),'yyyyMMdd') "

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注、受注明細等のマスタから取得する
        Dim SQLStr As String = ""

        '新規登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = 1 Then

            SQLStr =
              " SELECT TOP (@P0)" _
            & "   0                                              AS LINECNT " _
            & " , ''                                             AS OPERATION " _
            & " , ''                                             AS UPDTIMSTP " _
            & " , 1                                              AS 'SELECT' " _
            & " , 0                                              AS HIDDEN " _
            & " , FORMAT(GETDATE(),'yyyy/MM/dd')                 AS INITYMD " _
            & " , ''                                             AS LINETRAINNO " _
            & " , ''                                             AS LINEORDER " _
            & " , ''                                             AS TANKNUMBER " _
            & " , ''                                             AS PREOILCODE " _
            & " , ''                                             AS PREOILNAME " _
            & " , ''                                             AS DEPSTATION " _
            & " , ''                                             AS RETSTATION " _
            & " , ''                                             AS JRINSPECTIONALERT " _
            & " , ''                                             AS JRINSPECTIONDATE " _
            & " , ''                                             AS JRALLINSPECTIONALERT " _
            & " , ''                                             AS JRALLINSPECTIONDATE " _
            & " , '0'                                            AS DELFLG " _
            & " , 'O' + FORMAT(GETDATE(),'yyyyMMdd') + @P1       AS LINKNO " _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS LINKDETAILNO " _
            & " FROM sys.all_objects "

            SQLStr &=
                  " ORDER BY " _
                & "    LINECNT "

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = 2 Then
            SQLStr =
              " SELECT " _
            & "   0                                              AS LINECNT " _
            & " , ''                                             AS OPERATION " _
            & " , CAST(OIT0004.UPDTIMSTP AS bigint)              AS UPDTIMSTP " _
            & " , 1                                              AS 'SELECT' " _
            & " , 0                                              AS HIDDEN " _
            & " , ISNULL(FORMAT(OIT0004.INITYMD, 'yyyy/MM/dd'), '')            AS INITYMD " _
            & " , ISNULL(RTRIM(OIT0004.LINETRAINNO), '   ')     AS LINETRAINNO " _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), '   ')       AS LINEORDER " _
            & " , ISNULL(RTRIM(OIT0004.TANKNUMBER), '')         AS TANKNUMBER " _
            & " , ISNULL(RTRIM(OIT0005.LASTOILCODE), '')        AS PREOILCODE " _
            & " , ISNULL(RTRIM(OIM0003.OILNAME), '')            AS PREOILNAME " _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATION), '')         AS DEPSTATION " _
            & " , ISNULL(RTRIM(OIT0004.RETSTATION), '')         AS RETSTATION " _
            & " , CASE " _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN '' " _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>' " _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4 " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>' " _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>' " _
            & "   END                                                                      AS JRINSPECTIONALERT " _
            & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), '')               AS JRINSPECTIONDATE " _
            & " , CASE " _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN '' " _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>' " _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4 " _
            & "    AND DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>' " _
            & "   WHEN DATEDIFF(day, GETDATE(), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>' " _
            & "   END                                                                      AS JRALLINSPECTIONALERT " _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), '')            AS JRALLINSPECTIONDATE " _
            & " , ISNULL(RTRIM(OIT0004.DELFLG), '')              AS DELFLG " _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')             AS LINKNO " _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')            AS LINKDETAILNO " _
            & " FROM OIL.OIT0004_LINK OIT0004 " _
            & " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            & "       OIT0004.TANKNUMBER = OIT0005.TANKNUMBER " _
            & "       AND OIT0005.DELFLG <> @P2 " _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       OIT0004.TANKNUMBER = OIM0005.TANKNUMBER " _
            & "       AND OIM0005.DELFLG <> @P2 " _
            & " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003 ON " _
            & "       OIT0005.LASTOILCODE = OIM0003.OILCODE " _
            & "       AND OIT0004.OFFICECODE = OIM0003.OFFICECODE " _
            & "       AND OIM0003.DELFLG <> @P2 " _
            & " WHERE OIT0004.LINKNO = @P1 " _
            & " AND OIT0004.DELFLG <> @P2 "

            SQLStr &=
                  " ORDER BY " _
                & "    OIT0004.LINKDETAILNO "
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", SqlDbType.Int)          '明細数(新規作成)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA0.Value = O_INSCNT

                If work.WF_SEL_CREATEFLG.Text = 1 Then
                    For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                        PARA1.Value = OIT0002WKrow("LINKNO_NUM")
                        PARA2.Value = C_DELETE_FLG.ALIVE
                    Next
                ElseIf work.WF_SEL_CREATEFLG.Text = 2 Then
                    PARA1.Value = work.WF_SEL_LINKNO.Text
                    PARA2.Value = C_DELETE_FLG.DELETE
                End If

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If i = 0 Then work.WF_SEL_LINKNO.Text = OIT0002row("LINKNO")
                    i += 1
                    OIT0002row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D Select"
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
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            If OIT0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0002tbl)

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
    ''' 登録ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        'タンク車数の件数カウント用
        Dim intTankCnt As Integer = 0
        intTankCnt += Integer.Parse(TxtHTank.Text)
        intTankCnt += Integer.Parse(TxtRTank.Text)
        intTankCnt += Integer.Parse(TxtTTank.Text)
        intTankCnt += Integer.Parse(TxtMTTank.Text)
        intTankCnt += Integer.Parse(TxtKTank.Text)
        intTankCnt += Integer.Parse(TxtK3Tank.Text)
        intTankCnt += Integer.Parse(TxtK5Tank.Text)
        intTankCnt += Integer.Parse(TxtK10Tank.Text)
        intTankCnt += Integer.Parse(TxtLTank.Text)
        intTankCnt += Integer.Parse(TxtATank.Text)
        TxtTotalTank.Text = intTankCnt.ToString()

        '○ 画面表示データ取得
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, intTankCnt)
        End Using

        '〇画面で設定された油種コードを取得
        Dim WW_GetValue() As String = {"", "", "", "", ""}
        Dim arrTankCode(intTankCnt) As String
        Dim arrTankName(intTankCnt) As String
        Dim z As Integer = 0

        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtHTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtHTank.Text) - 1
            arrTankCode(z) = CONST_TxtHTank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtRTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtRTank.Text) - 1
            arrTankCode(z) = CONST_TxtRTank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtTTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtTTank.Text) - 1
            arrTankCode(z) = CONST_TxtTTank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtMTTank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtMTTank.Text) - 1
            arrTankCode(z) = CONST_TxtMTTank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtKTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtKTank.Text) - 1
            arrTankCode(z) = CONST_TxtKTank1
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtK3Tank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtK3Tank.Text) - 1
            arrTankCode(z) = CONST_TxtK3Tank1
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtK5Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtK5Tank.Text) - 1
            arrTankCode(z) = CONST_TxtK5Tank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtK10Tank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtK10Tank.Text) - 1
            arrTankCode(z) = CONST_TxtK10Tank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtLTank1, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtLTank.Text) - 1
            arrTankCode(z) = CONST_TxtLTank1
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", CONST_TxtATank, WW_GetValue)
        For i As Integer = 0 To Integer.Parse(TxtATank.Text) - 1
            arrTankCode(z) = CONST_TxtATank
            arrTankName(z) = WW_GetValue(0)
            z += 1
        Next

        '〇取得した油種情報をTBLに設定
        z = 0
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            OIT0002row("OILCODE") = arrTankCode(z)
            OIT0002row("OILNAME") = arrTankName(z)
            z += 1
        Next

        ''〇 1件以上の登録があった場合
        'If intTankCnt <> 0 Then
        '    '作成フラグを"2"(更新)に切換え
        '    work.WF_SEL_CREATEFLG.Text = "2"
        'End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
                    If WF_FIELD.Value = "WF_ORG" Then
                        prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    End If

                    '受注営業所
                    If WF_FIELD.Value = "TxtOrderOffice" Then
                        prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderOffice.Text)
                    End If

                    '本線列車
                    If WF_FIELD.Value = "TxtHeadOfficeTrain" Then
                        '                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, TxtHeadOfficeTrain.Text + work.WF_SEL_ORG.Text)
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, TxtHeadOfficeTrain.Text)
                    End If

                    '発駅
                    If WF_FIELD.Value = "TxtDepstation" Then
                        prmData = work.CreateSTATIONPTParam(work.WF_SEL_OFFICECODE.Text, TxtDepstation.Text)
                    End If

                    '着駅
                    If WF_FIELD.Value = "TxtRetstation" Then
                        prmData = work.CreateSTATIONPTParam(work.WF_SEL_OFFICECODE.Text, TxtRetstation.Text)
                    End If

                    '油種
                    If WF_FIELD.Value = "OILNAME" Then
                        '                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, "")
                    End If

                    'タンク車№
                    If WF_FIELD.Value = "TANKNO" Then
                        prmData = work.CreateSALESOFFICEParam(work.WF_SEL_CAMPCODE.Text, "")
                        'prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, "")
                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        '(予定)空車着日
                        Case "TxtEmpDate"
                            .WF_Calendar.Text = TxtEmpDate.Text
                        '(実績)空車着日
                        Case "TxtActEmpDate"
                            .WF_Calendar.Text = TxtActEmpDate.Text
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
        Master.RecoverTable(OIT0002tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0002tbl.Rows(i)("OPERATION") = "on" Then
                    OIT0002tbl.Rows(i)("OPERATION") = ""
                Else
                    OIT0002tbl.Rows(i)("OPERATION") = "on"
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
                CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '登録営業所
            Case "TxtOrderOffice"
                CODENAME_get("ORG", TxtHeadOfficeTrain.Text, WF_ORG_TEXT.Text, WW_RTN_SW)
            '本線列車
            Case "TxtHeadOfficeTrain"
                Dim WW_GetValue() As String = {"", "", "", "", ""}
                FixvalueMasterSearch("", "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)

                '発駅
                TxtDepstation.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtRetstation.Text = WW_GetValue(2)
                CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()
            '発駅
            Case "TxtDepstation"
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            '着駅
            Case "TxtRetstation"
                CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_RTN_SW)

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

            Case "WF_ORG"              '運用部署
                WF_ORG.Text = WW_SelectValue
                WF_ORG_TEXT.Text = WW_SelectText
                WF_ORG.Focus()

            Case "TxtOrderOffice"      '登録営業所
                '別の登録営業所が設定されて場合
                If TxtOrderOffice.Text <> WW_SelectText Then
                    TxtOrderOffice.Text = WW_SelectText
                    work.WF_SEL_OFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_OFFICENAME.Text = WW_SelectText

                    '本線列車, 発駅, 着駅のテキストボックスを初期化
                    TxtHeadOfficeTrain.Text = ""
                    TxtDepstation.Text = ""
                    LblDepstationName.Text = ""
                    TxtRetstation.Text = ""
                    LblRetstationName.Text = ""
                    '○ 油種別タンク車数(車)の件数を初期化
                    TxtTotalTank.Text = "0"
                    TxtHTank.Text = "0"
                    TxtRTank.Text = "0"
                    TxtTTank.Text = "0"
                    TxtMTTank.Text = "0"
                    TxtKTank.Text = "0"
                    TxtK3Tank.Text = "0"
                    TxtK5Tank.Text = "0"
                    TxtK10Tank.Text = "0"
                    TxtLTank.Text = "0"
                    TxtATank.Text = "0"

                    '〇営業所配下情報を取得・設定
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PATTERNMASTER", work.WF_SEL_OFFICECODE.Text, WW_GetValue)
                    Dim WF_SEL_SHIPPERSCODE As String = WW_GetValue(0)
                    Dim WF_SEL_SHIPPERSNAME As String = WW_GetValue(1)
                    Dim WF_SEL_BASECODE As String = WW_GetValue(2)
                    Dim WF_SEL_BASENAME As String = WW_GetValue(3)
                    Dim WF_SEL_CONSIGNEECODE As String = WW_GetValue(4)
                    Dim WF_SEL_CONSIGNEENAME As String = ""

                    '○ 一覧の初期化画面表示データ取得
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        MAPDataGet(SQLcon, 0)
                    End Using

                    '○ 画面表示データ保存
                    Master.SaveTable(OIT0002tbl)

                End If

                '新規作成の場合(油種別タンク車数のテキストボックスの入力を可とする。)
                If work.WF_SEL_CREATEFLG.Text = "1" Then
                    WW_ScreenEnabledSet()
                End If
                TxtOrderOffice.Focus()

            Case "TxtHeadOfficeTrain"   '本線列車
                '                TxtHeadOfficeTrain.Text = WW_SelectValue.Substring(0, 4)
                TxtHeadOfficeTrain.Text = WW_SelectValue
                FixvalueMasterSearch("", "TRAINNUMBER", WW_SelectValue, WW_GetValue)

                '発駅
                TxtDepstation.Text = WW_GetValue(1)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '着駅
                TxtRetstation.Text = WW_GetValue(2)
                CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()

            Case "TxtDepstation"        '空車発駅
                TxtDepstation.Text = WW_SelectValue
                LblDepstationName.Text = WW_SelectText
                TxtDepstation.Focus()

            Case "TxtRetstation"        '空車着駅
                TxtRetstation.Text = WW_SelectValue
                LblRetstationName.Text = WW_SelectText
                TxtRetstation.Focus()

            Case "TxtEmpDate"       '(予定)空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtEmpDate.Text = ""
                    Else
                        TxtEmpDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtEmpDate.Focus()
            Case "TxtDepDate"           '(実績)空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActEmpDate.Text = ""
                    Else
                        TxtActEmpDate.Text = leftview.WF_Calendar.Text
                    End If
                Catch ex As Exception
                End Try
                TxtActEmpDate.Focus()
            Case "OILNAME", "TANKNO", "RETURNDATETRAIN"   '(一覧)油種, (一覧)タンク車№, (一覧)返送日列車
                '○ LINECNT取得
                Dim WW_LINECNT As Integer = 0
                If Not Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT) Then Exit Sub

                '○ 設定項目取得
                Dim WW_SETTEXT As String = WW_SelectText
                Dim WW_SETVALUE As String = WW_SelectValue

                '○ 画面表示データ復元
                If Not Master.RecoverTable(OIT0002tbl) Then Exit Sub

                '○ 対象ヘッダー取得
                Dim updHeader = OIT0002tbl.AsEnumerable.
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
                End If
                'updHeader("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

                '○ 画面表示データ保存
                If Not Master.SaveTable(OIT0002tbl) Then Exit Sub

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
            Case "WF_ORG"              '運用部署
                WF_ORG.Focus()
            Case "TxtHeadOfficeTrain"   '本線列車
                TxtHeadOfficeTrain.Focus()
            Case "TxtDepstation"        '空車発駅
                TxtDepstation.Focus()
            Case "TxtRetstation"        '空車着駅
                TxtRetstation.Focus()
            Case "TxtEmpDate"       '(予定)空車着日
                TxtEmpDate.Focus()
            Case "TxtActEmpDate"           '(実績)空車着日
                TxtActEmpDate.Focus()
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
        Master.RecoverTable(OIT0002tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0002tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0002tbl.Rows.Count - 1
            If OIT0002tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0002tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' 行削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0002tbl)

        '■■■ OIT0002tbl関連の受注・受注明細を論理削除 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･貨車連結順序表明細を一括論理削除
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0004_LINK       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        DELFLG      = @P03       " _
                    & "  WHERE LINKNO     = @P01       " _
                    & "    AND LINKDETAILNO    = @P02       " _
                    & "    AND DELFLG     <> @P03       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            '選択されている行は削除対象
            Dim i As Integer = 0
            Dim j As Integer = 9000
            For Each OIT0002UPDrow In OIT0002tbl.Rows
                If OIT0002UPDrow("OPERATION") = "on" Then
                    j += 1
                    OIT0002UPDrow("LINECNT") = j        'LINECNT
                    OIT0002UPDrow("DELFLG") = C_DELETE_FLG.DELETE
                    OIT0002UPDrow("HIDDEN") = 1

                    PARA01.Value = OIT0002UPDrow("LINKNO")
                    PARA02.Value = OIT0002UPDrow("LINKDETAILNO")
                    PARA03.Value = C_DELETE_FLG.ALIVE

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD

                    SQLcmd.ExecuteNonQuery()
                Else
                    i += 1
                    OIT0002UPDrow("LINECNT") = i        'LINECNT
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonLINE_ADD_Click()

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        'DataBase接続文字
        Dim SQLcon = CS0050SESSION.getConnection
        SQLcon.Open() 'DataBase接続(Open)

        Dim SQLStrNum As String

        If work.WF_SEL_LINKNO.Text = "" Then
            SQLStrNum =
            " SELECT " _
            & "  ISNULL(SUBSTRING(OIT0004.LINKNO,1,9) + CONVERT(varchar,FORMAT(OIT0004.num,'00')), DUAL.LINKNO) AS LINKNO" _
            & ", '001'                                     AS LINKDETAILNO" _
            & " FROM (" _
            & "  SELECT 'O' + FORMAT(GETDATE(),'yyyyMMdd') + '01' AS LINKNO" _
            & " ) DUAL " _
            & " LEFT JOIN (" _
            & "  SELECT OIT0004.LINKNO" _
            & "  ,  CONVERT(int,SUBSTRING(OIT0004.LINKNO,10,2)) + 1 AS num" _
            & "  ,  ROW_NUMBER() OVER(ORDER BY OIT0004.LINKNO DESC) AS RNUM" _
            & "  FROM OIL.OIT0004_LINK OIT0004" _
            & "  WHERE SUBSTRING(OIT0004.LINKNO,2,8) = FORMAT(GETDATE(),'yyyyMMdd')" _
            & " ) OIT0004 ON " _
            & "   SUBSTRING(OIT0004.LINKNO,2,8) = SUBSTRING(DUAL.LINKNO,2,8) " _
            & "   AND ISNULL(OIT0004.RNUM, 1) = 1"
        Else
            SQLStrNum =
            " SELECT " _
            & "  ISNULL(OIT0004.LINKNO,'')                                     AS LINKNO" _
            & ", ISNULL(FORMAT(CONVERT(INT, OIT0004.LINKDETAILNO) + 1,'000'),'000') AS LINKDETAILNO" _
            & " FROM (" _
            & "  SELECT OIT0004.LINKNO" _
            & "       , OIT0004.LINKDETAILNO" _
            & "       , ROW_NUMBER() OVER(PARTITION BY OIT0004.LINKNO ORDER BY OIT0004.LINKNO, OIT0004.LINKDETAILNO DESC) RNUM" _
            & "  FROM OIL.OIT0004_DETAIL OIT0004" _
            & "  WHERE OIT0004.LINKNO = @P01" _
            & " ) OIT0004 " _
            & " WHERE OIT0004.RNUM = 1"

        End If

        '○ 追加SQL
        '　 説明　：　行追加用SQL
        Dim SQLStr As String =
        " SELECT TOP (1)" _
            & "   0                                              AS LINECNT " _
            & " , ''                                             AS OPERATION " _
            & " , '0'                                             AS UPDTIMSTP " _
            & " , 1                                              AS 'SELECT' " _
            & " , 0                                              AS HIDDEN " _
            & " , FORMAT(GETDATE(),'yyyy/MM/dd')                 AS INITYMD " _
            & " , ''                                             AS LINETRAINNO " _
            & " , ''                                             AS LINEORDER " _
            & " , ''                                             AS TANKNUMBER " _
            & " , ''                                             AS PREOILCODE " _
            & " , ''                                             AS PREOILNAME " _
            & " , ''                                             AS DEPSTATION " _
            & " , ''                                             AS RETSTATION " _
            & " , ''                                             AS JRINSPECTIONALERT " _
            & " , ''                                             AS JRINSPECTIONDATE " _
            & " , ''                                             AS JRALLINSPECTIONALERT " _
            & " , ''                                             AS JRALLINSPECTIONDATE " _
            & " , '0'                                            AS DELFLG " _
            & " , @P1       AS LINKNO " _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS LINKDETAILNO " _
            & " FROM sys.all_objects "
        SQLStr &=
                  " ORDER BY" _
                & "    LINECNT"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdNum As New SqlCommand(SQLStrNum, SQLcon)
                Dim PARANUM1 As SqlParameter = SQLcmdNum.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                PARANUM1.Value = work.WF_SEL_LINKNO.Text

                Using SQLdrNum As SqlDataReader = SQLcmdNum.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdrNum.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdrNum.GetName(index), SQLdrNum.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdrNum)
                End Using

                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№

                Dim strOrderNo As String = ""
                Dim intDetailNo As Integer = 0

                For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    PARA1.Value = OIT0002WKrow("ORDERNO_NUM")
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    ''○ フィールド名とフィールドの型を取得
                    'For index As Integer = 0 To SQLdr.FieldCount - 1
                    '    OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    'Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim j As Integer = 9000
                For Each OIT0002row As DataRow In OIT0002tbl.Rows

                    '行追加データに既存の受注№を設定する。
                    '既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    If OIT0002row("LINECNT") = 0 Then
                        If work.WF_SEL_CREATEFLG.Text = "1" Then
                            OIT0002row("ORDERNO") = strOrderNo
                            OIT0002row("DETAILNO") = intDetailNo.ToString("000")
                        Else
                            OIT0002row("ORDERNO") = work.WF_SEL_LINKNO.Text
                            OIT0002row("DETAILNO") = intDetailNo.ToString("000")
                        End If
                    End If

                    '削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    If OIT0002row("HIDDEN") = 1 Then
                        j += 1
                        OIT0002row("LINECNT") = j        'LINECNT
                    Else
                        i += 1
                        OIT0002row("LINECNT") = i        'LINECNT
                    End If
                    'strOrderNoBak = OIT0002row("ORDERNO")
                    intDetailNo += 1
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
        CS0030REPORT.TBLDATA = OIT0002tbl                       'データ参照  Table
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
            '貨車連結表DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WW_UpdateOrder(SQLcon)
            End Using


            '貨車連結表(一覧)画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                WW_OrderListTBLSet(SQLcon)
            End Using

            ''貨車連結表(明細)画面表示データ取得
            'Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            '    SQLcon.Open()       'DataBase接続
            '    work.WF_SEL_CREATEFLG.Text = 2
            '    MAPDataGet(SQLcon)
            'End Using

        End If

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0002tbl)

        '○ GridView初期設定
        '○ 画面表示データ再取得(貨車連結表(明細)画面表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            work.WF_SEL_CREATEFLG.Text = 2
            MAPDataGet(SQLcon, 0)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

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
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ UPLOAD XLSデータ取得
        CS0023XLSUPLOAD.CAMPCODE = work.WF_SEL_CAMPCODE.Text        '会社コード
        CS0023XLSUPLOAD.MAPID = Master.MAPID                        '画面ID
        CS0023XLSUPLOAD.CS0023XLSUPLOAD()
        If isNormal(CS0023XLSUPLOAD.ERR) Then
            If CS0023XLSUPLOAD.TBLDATA.Rows.Count = 0 Then
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR)
                Exit Sub
            End If
        Else
            Master.Output(CS0023XLSUPLOAD.ERR, C_MESSAGE_TYPE.ABORT, "CS0023XLSUPLOAD")
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
        Master.CreateEmptyTable(OIT0002INPtbl)

        For Each XLSTBLrow As DataRow In CS0023XLSUPLOAD.TBLDATA.Rows
            Dim OIT0002INProw As DataRow = OIT0002INPtbl.NewRow

            '○ 初期クリア
            For Each OIT0002INPcol As DataColumn In OIT0002INPtbl.Columns
                If IsDBNull(OIT0002INProw.Item(OIT0002INPcol)) OrElse IsNothing(OIT0002INProw.Item(OIT0002INPcol)) Then
                    Select Case OIT0002INPcol.ColumnName
                        Case "LINECNT"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case "OPERATION"
                            OIT0002INProw.Item(OIT0002INPcol) = C_LIST_OPERATION_CODE.NODATA
                        Case "TIMSTP"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case "SELECT"
                            OIT0002INProw.Item(OIT0002INPcol) = 1
                        Case "HIDDEN"
                            OIT0002INProw.Item(OIT0002INPcol) = 0
                        Case Else
                            OIT0002INProw.Item(OIT0002INPcol) = ""
                    End Select
                End If
            Next

            ''○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("DEPSTATION") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If XLSTBLrow("LINKNO") = OIT0002row("LINKNO") AndAlso
                        XLSTBLrow("LINKDETAILNO") = OIT0002row("LINKDETAILNO") AndAlso
                        XLSTBLrow("STATUS") = OIT0002row("STATUS") AndAlso
                        XLSTBLrow("INFO") = OIT0002row("INFO") AndAlso
                        XLSTBLrow("PREORDERNO") = OIT0002row("PREORDERNO") AndAlso
                        XLSTBLrow("OFFICECODE") = OIT0002row("OFFICECODE") AndAlso
                        XLSTBLrow("DEPSTATION") = OIT0002row("DEPSTATION") AndAlso
                        XLSTBLrow("DEPSTATIONNAME") = OIT0002row("DEPSTATIONNAME") AndAlso
                        XLSTBLrow("RETSTATION") = OIT0002row("RETSTATION") AndAlso
                        XLSTBLrow("RETSTATIONNAME") = OIT0002row("RETSTATIONNAME") AndAlso
                        XLSTBLrow("EMPARRDATE") = OIT0002row("EMPARRDATE") AndAlso
                        XLSTBLrow("ACTUALEMPARRDATE") = OIT0002row("ACTUALEMPARRDATE") AndAlso
                        XLSTBLrow("LINETRAINNO") = OIT0002row("LINETRAINNO") AndAlso
                        XLSTBLrow("LINEORDER") = OIT0002row("LINEORDER") AndAlso
                        XLSTBLrow("TANKNUMBER") = OIT0002row("TANKNUMBER") AndAlso
                        XLSTBLrow("PREOILCODE") = OIT0002row("PREOILCODE") Then
                        OIT0002INProw.ItemArray = OIT0002row.ItemArray
                        Exit For
                    End If
                Next
            End If

            '○ 項目セット
            '貨車連結順序表№
            If WW_COLUMNS.IndexOf("LINKNO") >= 0 Then
                OIT0002INProw("LINKNO") = XLSTBLrow("LINKNO")
            End If

            '貨車連結順序表明細№
            If WW_COLUMNS.IndexOf("LINKDETAILNO") >= 0 Then
                OIT0002INProw("LINKDETAILNO") = XLSTBLrow("LINKDETAILNO")
            End If

            'ステータス
            If WW_COLUMNS.IndexOf("STATUS") >= 0 Then
                OIT0002INProw("STATUS") = XLSTBLrow("STATUS")
            End If

            '情報
            If WW_COLUMNS.IndexOf("INFO") >= 0 Then
                OIT0002INProw("INFO") = XLSTBLrow("INFO")
            End If

            '前回オーダー№
            If WW_COLUMNS.IndexOf("PREORDERNO") >= 0 Then
                OIT0002INProw("PREORDERNO") = XLSTBLrow("PREORDERNO")
            End If

            '本線列車
            If WW_COLUMNS.IndexOf("TRAINNO") >= 0 Then
                OIT0002INProw("TRAINNO") = XLSTBLrow("TRAINNO")
            End If

            '登録営業所コード
            If WW_COLUMNS.IndexOf("OFFICECODE") >= 0 Then
                OIT0002INProw("OFFICECODE") = XLSTBLrow("OFFICECODE")
            End If

            '空車発駅コード
            If WW_COLUMNS.IndexOf("DEPSTATION") >= 0 Then
                OIT0002INProw("DEPSTATION") = XLSTBLrow("DEPSTATION")
            End If

            '空車発駅名
            If WW_COLUMNS.IndexOf("DEPSTATIONNAME") >= 0 Then
                OIT0002INProw("DEPSTATIONNAME") = XLSTBLrow("DEPSTATIONNAME")
            End If

            '空車着駅コード
            If WW_COLUMNS.IndexOf("RETSTATION") >= 0 Then
                OIT0002INProw("RETSTATION") = XLSTBLrow("RETSTATION")
            End If

            '空車着駅名
            If WW_COLUMNS.IndexOf("RETSTATIONNAME") >= 0 Then
                OIT0002INProw("RETSTATIONNAME") = XLSTBLrow("RETSTATIONNAME")
            End If

            '空車着日（予定）
            If WW_COLUMNS.IndexOf("EMPARRDATE") >= 0 Then
                OIT0002INProw("EMPARRDATE") = XLSTBLrow("EMPARRDATE")
            End If

            '空車着日（実績）
            If WW_COLUMNS.IndexOf("ACTUALEMPARRDATE") >= 0 Then
                OIT0002INProw("ACTUALEMPARRDATE") = XLSTBLrow("ACTUALEMPARRDATE")
            End If

            '入線列車番号
            If WW_COLUMNS.IndexOf("LINETRAINNO") >= 0 Then
                OIT0002INProw("LINETRAINNO") = XLSTBLrow("LINETRAINNO")
            End If

            '入線順
            If WW_COLUMNS.IndexOf("LINEORDER") >= 0 Then
                OIT0002INProw("LINEORDER") = XLSTBLrow("LINEORDER")
            End If

            'タンク車№
            If WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 Then
                OIT0002INProw("TANKNUMBER") = XLSTBLrow("TANKNUMBER")
            End If

            '前回油種
            If WW_COLUMNS.IndexOf("PREOILCODE") >= 0 Then
                OIT0002INProw("PREOILCODE") = XLSTBLrow("PREOILCODE")
            End If

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIT0002INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIT0002INProw("DELFLG") = "0"
            End If

            OIT0002INPtbl.Rows.Add(OIT0002INProw)
        Next

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        OIT0002tbl_UPD()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)


        '○ メッセージ表示
        If isNormal(WW_ERR_SW) Then
            Master.Output(C_MESSAGE_NO.IMPORT_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Else
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
        End If

        '○ Close
        CS0023XLSUPLOAD.TBLDATA.Dispose()
        CS0023XLSUPLOAD.TBLDATA.Clear()

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
        Dim updHeader = OIT0002tbl.AsEnumerable.
                    FirstOrDefault(Function(x) x.Item("LINECNT") = WW_LINECNT)
        If IsNothing(updHeader) Then Exit Sub

        '○ 設定項目取得
        '対象フォーム項目取得
        Dim WW_ListValue = Request.Form("txt" & pnlListArea.ID & WF_FIELD.Value & WF_GridDBclick.Text)
        Dim WW_GetValue() As String = {"", "", "", "", ""}

        Select Case WF_FIELD.Value
            Case "OILNAME"           '(一覧)油種
                If WW_ListValue <> "" Then
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN_N", WW_ListValue, WW_GetValue)
                    updHeader.Item("OILCODE") = WW_GetValue(0)
                    updHeader.Item(WF_FIELD.Value) = WW_ListValue
                Else
                    updHeader.Item("OILCODE") = ""
                    updHeader.Item(WF_FIELD.Value) = ""
                End If

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
        Master.SaveTable(OIT0002tbl)

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_STYMD As Date
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '登録営業所
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", work.WF_SEL_OFFICECODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "登録営業所 : " & work.WF_SEL_OFFICECODE.Text)
                TxtOrderOffice.Focus()
                WW_CheckMES1 = "登録営業所入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
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

        '空車発駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", TxtDepstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "発駅 : " & TxtDepstation.Text)
                TxtDepstation.Focus()
                WW_CheckMES1 = "空車発駅入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtDepstation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '空車着駅
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RETSTATION", TxtRetstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "着駅 : " & TxtRetstation.Text)
                TxtRetstation.Focus()
                WW_CheckMES1 = "空車着駅入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtRetstation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)空車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TxtEmpDate", TxtEmpDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtEmpDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtEmpDate.Focus()
            WW_CheckMES1 = "(予定)空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(実績)空車着日
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TxtActEmpDate", TxtActEmpDate.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Try
                Date.TryParse(TxtActEmpDate.Text, WW_STYMD)
            Catch ex As Exception
                WW_STYMD = C_DEFAULT_YMD
            End Try
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            TxtActEmpDate.Focus()
            WW_CheckMES1 = "(実績)空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        WW_ERR_MES &= ControlChars.NewLine & "  --> 登録営業所         =" & TxtOrderOffice.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 本社列車           =" & TxtHeadOfficeTrain.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 空車発駅           =" & TxtDepstation.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着駅           =" & TxtRetstation.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (予定)空車着日     =" & TxtEmpDate.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> (実績)空車着日     =" & TxtActEmpDate.Text & " , "

        rightview.SetErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' エラーレポート編集(一覧用)
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIT0002row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckListERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIT0002row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIT0002row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 項番               =" & OIT0002row("LINECNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 登録日             =" & OIT0002row("INITYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線列車番号       =" & OIT0002row("LINETRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線順             =" & OIT0002row("LINEORDER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車番号       =" & OIT0002row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回油種　　       =" & OIT0002row("PREOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車発駅名 　　　　=" & OIT0002row("DEPSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着駅名 　　　　=" & OIT0002row("RETSTATIONNAME") & " , "
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub FixvalueMasterSearch(ByVal I_CODE As String, ByVal I_CLASS As String, ByVal I_KEYCODE As String, ByRef O_VALUE() As String)

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

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
                & " AND VIW0001.DELFLG <> @P03"

            '○ 条件指定で指定されたものでSQLで可能なものを追加する
            '会社コード
            If Not String.IsNullOrEmpty(I_CODE) Then
                SQLStr &= String.Format("    AND VIW0001.CAMPCODE = '{0}'", I_CODE)
            End If
            'マスターキー
            If Not String.IsNullOrEmpty(I_KEYCODE) Then
                SQLStr &= String.Format("    AND VIW0001.KEYCODE = '{0}'", I_KEYCODE)
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
                        OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    Dim i As Integer = 0
                    For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                        O_VALUE(i) = OIT0002WKrow("KEYCODE")
                        i += 1
                    Next
                Else
                    For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                        O_VALUE(0) = OIT0002WKrow("VALUE1")
                        O_VALUE(1) = OIT0002WKrow("VALUE2")
                        O_VALUE(2) = OIT0002WKrow("VALUE3")
                        O_VALUE(3) = OIT0002WKrow("VALUE4")
                        O_VALUE(4) = OIT0002WKrow("VALUE5")
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet()

        '〇各営業者で管理している油種を取得
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", ""}
        FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", "", WW_GetValue)

        '〇初期化
        'ハイオク
        TxtHTank.Enabled = False
        'レギュラー
        TxtRTank.Enabled = False
        '灯油
        TxtTTank.Enabled = False
        '未添加灯油
        TxtMTTank.Enabled = False
        '軽油
        TxtKTank.Enabled = False
        '３号軽油
        TxtK3Tank.Enabled = False
        '軽油５
        TxtK5Tank.Enabled = False
        '軽油１０
        TxtK10Tank.Enabled = False
        'ＬＳＡ
        TxtLTank.Enabled = False
        'Ａ重油
        TxtATank.Enabled = False

        For i As Integer = 0 To WW_GetValue.Length - 1
            Select Case WW_GetValue(i)
                    'ハイオク
                Case "1001"
                    TxtHTank.Enabled = True
                    'レギュラー
                Case "1101"
                    TxtRTank.Enabled = True
                    '灯油
                Case "1301"
                    TxtTTank.Enabled = True
                    '未添加灯油
                Case "1302"
                    TxtMTTank.Enabled = True
                    '軽油
                Case "1401", "1406"
                    TxtKTank.Enabled = True
                    '３号軽油
                Case "1404", "1405"
                    TxtK3Tank.Enabled = True
                    '軽油５
                Case "1402"
                    TxtK5Tank.Enabled = True
                    '軽油１０
                Case "1403"
                    TxtK10Tank.Enabled = True
                    'ＬＳＡ
                Case "2201", "2202"
                    TxtLTank.Enabled = True
                    'Ａ重油
                Case "2101"
                    TxtATank.Enabled = True
            End Select
        Next
    End Sub

    ''' <summary>
    ''' 貨車連結表TBL登録更新
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
            & "        OIL.OIT0004_ORDER" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0004_ORDER" _
            & "    SET" _
            & "        OFFICECODE   = @P04    , OFFICENAME     = @P05" _
            & "        , TRAINNO    = @P02" _
            & "        , DEPSTATION = @P13    , DEPSTATIONNAME = @P14" _
            & "        , RETSTATION = @P15    , RETSTATIONNAME = @P16" _
            & "        , LODDATE    = @P24    , DEPDATE        = @P25" _
            & "        , ARRDATE    = @P26    , ACCDATE        = @P27" _
            & "        , UPDYMD     = @P87    , UPDUSER        = @P88" _
            & "        , UPDTERMID  = @P89    , RECEIVEYMD     = @P90" _
            & "    WHERE" _
            & "        ORDERNO          = @P01" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0004_ORDER" _
            & "        ( ORDERNO      , TRAINNO         , ORDERYMD       , OFFICECODE          , OFFICENAME" _
            & "        , ORDERTYPE    , SHIPPERSCODE    , SHIPPERSNAME   , BASECODE            , BASENAME" _
            & "        , CONSIGNEECODE, CONSIGNEENAME   , DEPSTATION     , DEPSTATIONNAME      , RETSTATION , RETSTATIONNAME" _
            & "        , RETSTATION   , RETSTATIONNAME  , CANGERETSTATION, CHANGERETSTATIONNAME, ORDERSTATUS" _
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
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , CANGERETSTATION" _
            & "    , CHANGERETSTATIONNAME" _
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
            & "    OIL.OIT0004_ORDER" _
            & " WHERE" _
            & "        ORDERNO      = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 4)  '本線列車
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '受注登録日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 6)  '登録営業所コード
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '登録営業所名
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

                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    'If Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.UPDATING OrElse
                    '    Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.INSERTING OrElse
                    '    Trim(OIT0002row("OPERATION")) = C_LIST_OPERATION_CODE.SELECTED Then
                    Dim WW_DATENOW As DateTime = Date.Now

                    'DB更新
                    PARA01.Value = work.WF_SEL_LINKNO.Text       '受注№
                    PARA01.Value = OIT0002row("LINKNO")              '受注№
                    PARA02.Value = TxtHeadOfficeTrain.Text            '本線列車
                    PARA03.Value = OIT0002row("INITYMD")             '登録日
                    PARA04.Value = work.WF_SEL_OFFICECODE.Text   '登録営業所コード
                    PARA05.Value = work.WF_SEL_OFFICENAME.Text       '登録営業所名
                    PARA06.Value = ""       '受注パターン
                    PARA07.Value = ""       '荷主コード
                    PARA08.Value = ""       '荷主名
                    PARA09.Value = ""       '基地コード
                    PARA10.Value = ""       '基地名
                    PARA11.Value = ""       '荷受人コード
                    PARA12.Value = ""       '荷受人名
                    PARA13.Value = TxtDepstation.Text                 '発駅コード
                    PARA14.Value = LblDepstationName.Text             '発駅名
                    PARA15.Value = TxtRetstation.Text                 '着駅コード
                    PARA16.Value = LblRetstationName.Text             '着駅名
                    PARA17.Value = ""       '空車着駅コード
                    PARA18.Value = ""       '空車着駅名
                    PARA19.Value = ""       '空車着駅コード(変更後)
                    PARA20.Value = ""       '空車着駅名(変更後)
                    PARA21.Value = "100"                              '受注進行ステータス(100:受注受付)
                    PARA22.Value = ""       '受注情報
                    PARA23.Value = "0"                                '利用可否フラグ(0:利用可能)
                    PARA24.Value = TxtEmpDate.Text                '(予定)空車着日
                    PARA25.Value = TxtActEmpDate.Text                    '(実績)空車着日　
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
                    PARA83.Value = OIT0002row("DELFLG")               '削除フラグ
                    PARA84.Value = WW_DATENOW                         '登録年月日
                    PARA85.Value = Master.USERID                      '登録ユーザーID
                    PARA86.Value = Master.USERTERMID                  '登録端末
                    PARA87.Value = WW_DATENOW                         '更新年月日
                    PARA88.Value = Master.USERID                      '更新ユーザーID
                    PARA89.Value = Master.USERTERMID                  '更新端末
                    PARA90.Value = C_DEFAULT_YMD

                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                    '更新ジャーナル出力
                    JPARA01.Value = work.WF_SEL_LINKNO.Text

                    Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(OIT0002UPDtbl) Then
                            OIT0002UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                OIT0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        OIT0002UPDtbl.Clear()
                        OIT0002UPDtbl.Load(SQLdr)
                    End Using

                    For Each OIT0002UPDrow As DataRow In OIT0002UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "OIT0002L"
                        CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                        CS0020JOURNAL.ROW = OIT0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 貨車連結順序表(一覧)表示用
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_OrderListTBLSet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIT0002WKtbl) Then
            OIT0002WKtbl = New DataTable
        End If

        If OIT0002WKtbl.Columns.Count <> 0 Then
            OIT0002WKtbl.Columns.Clear()
        End If

        OIT0002WKtbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , CAST(OIT0004.UPDTIMSTP AS bigint)                  AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0004.ORDERNO), '')   　            AS ORDERNO" _
            & " , ISNULL(FORMAT(OIT0004.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0004.ORDERSTATUS), '   ')          AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIT0004.ORDERINFO), '')               AS ORDERINFO" _
            & " , ISNULL(RTRIM(OIT0004.OFFICENAME), '')              AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0004.TRAINNO), '')                 AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATION), '')              AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0004.RETSTATION), '')              AS RETSTATION" _
            & " , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')          AS RETSTATIONNAME" _
            & " , ISNULL(FORMAT(OIT0004.LODDATE, 'yyyy/MM/dd'), '')  AS LODDATE" _
            & " , ISNULL(FORMAT(OIT0004.DEPDATE, 'yyyy/MM/dd'), '')  AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0004.ARRDATE, 'yyyy/MM/dd'), '')  AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0004.ACCDATE, 'yyyy/MM/dd'), '')  AS ACCDATE" _
            & " , ISNULL(RTRIM(OIT0004.RTANK), '')                   AS RTANK" _
            & " , ISNULL(RTRIM(OIT0004.HTANK), '')                   AS HTANK" _
            & " , ISNULL(RTRIM(OIT0004.TTANK), '')                   AS TTANK" _
            & " , ISNULL(RTRIM(OIT0004.MTTANK), '')                  AS MTTANK" _
            & " , ISNULL(RTRIM(OIT0004.KTANK), '')                   AS KTANK" _
            & " , ISNULL(RTRIM(OIT0004.K3TANK), '')                  AS K3TANK" _
            & " , ISNULL(RTRIM(OIT0004.K5TANK), '')                  AS K5TANK" _
            & " , ISNULL(RTRIM(OIT0004.K10TANK), '')                 AS K10TANK" _
            & " , ISNULL(RTRIM(OIT0004.LTANK), '')                   AS LTANK" _
            & " , ISNULL(RTRIM(OIT0004.ATANK), '')                   AS ATANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER1OTANK), '')             AS OTHER1OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER2OTANK), '')             AS OTHER2OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER3OTANK), '')             AS OTHER3OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER4OTANK), '')             AS OTHER4OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER5OTANK), '')             AS OTHER5OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER6OTANK), '')             AS OTHER6OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER7OTANK), '')             AS OTHER7OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER8OTANK), '')             AS OTHER8OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER9OTANK), '')             AS OTHER9OTANK" _
            & " , ISNULL(RTRIM(OIT0004.OTHER10OTANK), '')            AS OTHER10OTANK" _
            & " , ISNULL(RTRIM(OIT0004.TOTALTANK), '')               AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0004.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0004_ORDER OIT0004 " _
            & " WHERE OIT0004.OFFICECODE = @P1" _
            & "   AND OIT0004.LODDATE    >= @P2" _
            & "   AND OIT0004.DELFLG     <> @P3"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        ''列車番号
        'If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
        '    SQLStr &= String.Format("    AND OIT0004.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        'End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0004.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10) '受注№
                'Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = work.WF_SEL_OFFICECODE.Text
                'PARA2.Value = work.WF_SEL_LOADING.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0002WKtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0002WKtbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0002row As DataRow In OIT0002WKtbl.Rows
                    i += 1
                    OIT0002row("LINECNT") = i        'LINECNT

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D ORDERLISTSET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0002D OrderListSet"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002WKtbl, work.WF_SEL_INPTBL.Text)

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        'WF_Sel_LINECNT.Text = ""            'LINECNT

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

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
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIT0002INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIT0002INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '油種(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OILCODE", OIT0002INProw("OILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "油種入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'タンク車(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNO", OIT0002INProw("TANKNO"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "タンク車入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            If WW_LINE_ERR = "" Then
                If OIT0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIT0002INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIT0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' OIT0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIT0002tbl_UPD()

        '○ 画面状態設定
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            Select Case OIT0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIT0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIT0002INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIT0002row As DataRow In OIT0002tbl.Rows
                If OIT0002row("ORDERNO") = OIT0002INProw("ORDERNO") AndAlso
                    OIT0002row("DETAILNO") = OIT0002INProw("DETAILNO") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIT0002row("DELFLG") = OIT0002INProw("DELFLG") AndAlso
                        OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIT0002INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIT0002INProw As DataRow In OIT0002INPtbl.Rows
            Select Case OIT0002INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIT0002INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIT0002INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIT0002INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIT0002INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIT0002INProw As DataRow)

        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '同一レコードか判定
            If OIT0002INProw("ORDERNO") = OIT0002row("ORDERNO") AndAlso
                OIT0002INProw("DETAILNO") = OIT0002row("DETAILNO") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIT0002INProw("TIMSTP") = OIT0002row("TIMSTP")
                OIT0002INProw("SELECT") = 1
                OIT0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0002row.ItemArray = OIT0002INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIT0002INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIT0002row As DataRow = OIT0002tbl.NewRow
        OIT0002row.ItemArray = OIT0002INProw.ItemArray

        OIT0002row("LINECNT") = OIT0002tbl.Rows.Count + 1
        If OIT0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
        End If

        OIT0002row("TIMSTP") = "0"
        OIT0002row("SELECT") = 1
        OIT0002row("HIDDEN") = 0

        OIT0002tbl.Rows.Add(OIT0002row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIT0002INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIT0002INProw As DataRow)

        For Each OIT0002row As DataRow In OIT0002tbl.Rows

            '同一レコードか判定
            If OIT0002INProw("ORDERNO") = OIT0002row("ORDERNO") AndAlso
               OIT0002INProw("DETAILNO") = OIT0002row("DETAILNO") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIT0002INProw("TIMSTP") = OIT0002row("TIMSTP")
                OIT0002INProw("SELECT") = 1
                OIT0002INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIT0002row.ItemArray = OIT0002INProw.ItemArray
                Exit For
            End If
        Next

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

                Case "ORG"             '運用部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "RETSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "RETSTATION"))

                Case "PRODUCTPATTERN"   '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
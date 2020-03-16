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
    Private Const CONST_INIT_ROWS As Integer = 5                    '新規登録時初期行数
    Private Const CONST_SCROLLCOUNT As Integer = 7                  'マウススクロール時稼働行数
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

    Private WW_ORDERINFOALERMFLG_80 As Boolean = False                   '受注情報セット可否(警告(80:タンク車数オーバー))
    Private WW_ORDERINFOALERM_80 As String = "80"                        '受注情報(80:タンク車数オーバー)用格納
    Private WW_ORDERINFOALERMNAME_80 As String = "タンク車数オーバー"

    Private WW_ORDERINFOALERMFLG_82 As Boolean = False                   '受注情報セット可否(警告(82:検査間近あり))
    Private WW_ORDERINFOALERM_82 As String = "82"                        '受注情報(82:検査間近あり)用格納
    Private WW_ORDERINFOALERMNAME_82 As String = "検査間近あり"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0002tbl)

                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(OIT0002tbl, pnlListArea) Then
                        Master.SaveTable(OIT0002tbl)
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonRegister"        '登録ボタン押下
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
                If WF_PANELFLG.Value <> "1" Then
                    WF_PANELFLG.Value = "2"
                End If
            Else
                WF_CREATEFLG.Value = "2"
                If WF_PANELFLG.Value <> "1" Then
                    WF_PANELFLG.Value = "1"
                End If
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

            WF_UPDERRFLG.Value = "0"
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

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        ''登録営業所
        'TxtOrderOffice.Text = work.WF_SEL_OFFICENAME.Text
        '利用可能日
        AvailableYMD.Text = work.WF_SEL_AVAILABLEYMD.Text
        '本線列車
        TxtHeadOfficeTrain.Text = work.WF_SEL_TRAINNO2.Text
        '空車発駅（着駅）
        TxtDepstation.Text = work.WF_SEL_DEPSTATION.Text
        '空車着駅（発駅）
        TxtRetstation.Text = work.WF_SEL_RETSTATION2.Text
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

        '新規作成の場合
        If work.WF_SEL_CREATEFLG.Text <> "1" Then
            '既存データの修正については、登録営業所は入力不可とする。
            TxtOrderOffice.Enabled = False
        End If

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("ORG", work.WF_SEL_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)
        '登録営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_DUMMY)
        'work.WF_SEL_OFFICECODE.Text = TxtOrderOffice.Text
        '空車発駅（着駅）
        CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
        '空車着駅（発駅）
        CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)

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

        '登録ボタン押下
        If work.WF_SEL_CREATEFLG.Text = 1 Then

            SQLStr =
              " SELECT TOP (@P0)" _
            & "   0                                              AS LINECNT " _
            & " , ''                                             AS OPERATION " _
            & " , ''                                             AS UPDTIMSTP " _
            & " , 1                                              AS 'SELECT' " _
            & " , 0                                              AS HIDDEN " _
            & " , ''                                             AS LINETRAINNO " _
            & " , ''                                             AS LINEORDER " _
            & " , ''                                             AS TANKNUMBER " _
            & " , ''                                             AS PREOILCODE " _
            & " , ''                                             AS PREOILNAME " _
            & " , ''                                             AS PREORDERINGTYPE " _
            & " , ''                                             AS PREORDERINGOILNAME " _
            & " , ''                                             AS DEPSTATION " _
            & " , @P3                                            AS DEPSTATIONNAME " _
            & " , ''                                             AS RETSTATION " _
            & " , @P4                                            AS RETSTATIONNAME " _
            & " , ''                                             AS JRINSPECTIONALERT " _
            & " , ''                                             AS JRINSPECTIONDATE " _
            & " , ''                                             AS JRINSPECTIONALERTSTR " _
            & " , ''                                             AS JRALLINSPECTIONALERT " _
            & " , ''                                             AS JRALLINSPECTIONDATE " _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR " _
            & " , @P8                                            AS AVAILABLEYMD " _
            & " , @P2                                            AS DELFLG " _
            & " , 'L' + FORMAT(GETDATE(),'yyyyMMdd') + @P1       AS LINKNO " _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS LINKDETAILNO " _
            & " FROM sys.all_objects "

            SQLStr &=
                  " ORDER BY " _
                & "    LINECNT "

            '明細データダブルクリック
        ElseIf work.WF_SEL_CREATEFLG.Text = 2 Then
            SQLStr =
              " SELECT " _
            & "   0                                             AS LINECNT " _
            & " , ''                                            AS OPERATION " _
            & " , CAST(OIT0004.UPDTIMSTP AS bigint)             AS UPDTIMSTP " _
            & " , 1                                             AS 'SELECT' " _
            & " , 0                                             AS HIDDEN " _
            & " , ISNULL(RTRIM(OIT0004.LINETRAINNO), '')        AS LINETRAINNO " _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), '')          AS LINEORDER " _
            & " , ISNULL(RTRIM(OIT0004.TANKNUMBER), '')         AS TANKNUMBER " _
            & " , ISNULL(RTRIM(OIT0004.PREOILCODE), '')         AS PREOILCODE " _
            & " , ISNULL(RTRIM(OIT0004.PREOILNAME), '')         AS PREOILNAME " _
            & " , ISNULL(RTRIM(OIT0004.PREORDERINGTYPE), '')    AS PREORDERINGTYPE " _
            & " , ISNULL(RTRIM(OIT0004.PREORDERINGOILNAME), '') AS PREORDERINGOILNAME " _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATION), '')         AS DEPSTATION " _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')     AS DEPSTATIONNAME " _
            & " , ISNULL(RTRIM(OIT0004.RETSTATION), '')         AS RETSTATION " _
            & " , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')     AS RETSTATIONNAME " _
            & " , CASE " _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN '' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4 " _
            & "    AND DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>' " _
            & "   END                                                                      AS JRINSPECTIONALERT " _
            & " , ISNULL(FORMAT(OIM0005.JRINSPECTIONDATE, 'yyyy/MM/dd'), '')               AS JRINSPECTIONDATE " _
            & " , CASE " _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '') = '' THEN '' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 3 THEN @P5 " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 4 " _
            & "    AND DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) <= 6 THEN @P6 " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRINSPECTIONDATE), '')) >= 7 THEN @P7 " _
            & "   END                                                                      AS JRINSPECTIONALERTSTR " _
            & " , CASE " _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN '' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN '<div style=""text-align:center;font-size:22px;color:red;"">●</div>' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4 " _
            & "    AND DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN '<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN '<div style=""text-align:center;font-size:22px;color:green;"">●</div>' " _
            & "   END                                                                      AS JRALLINSPECTIONALERT " _
            & " , ISNULL(FORMAT(OIM0005.JRALLINSPECTIONDATE, 'yyyy/MM/dd'), '')            AS JRALLINSPECTIONDATE " _
            & " , CASE " _
            & "   WHEN ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '') = '' THEN '' " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 3 THEN @P5 " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 4 " _
            & "    AND DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) <= 6 THEN @P6 " _
            & "   WHEN DATEDIFF(day, ISNULL(RTRIM(OIT0004.AVAILABLEYMD), ''), ISNULL(RTRIM(OIM0005.JRALLINSPECTIONDATE), '')) >= 7 THEN @P7 " _
            & "   END                                                                      AS JRALLINSPECTIONALERTSTR " _
            & " , ISNULL(FORMAT(OIT0004.AVAILABLEYMD, 'yyyy/MM/dd'), '')            AS AVAILABLEYMD " _
            & " , ISNULL(RTRIM(OIT0004.DELFLG), '')              AS DELFLG " _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')             AS LINKNO " _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')            AS LINKDETAILNO " _
            & " FROM OIL.OIT0004_LINK OIT0004 " _
            & " LEFT JOIN OIL.OIM0005_TANK OIM0005 ON " _
            & "       OIT0004.TANKNUMBER = OIM0005.TANKNUMBER " _
            & "       AND OIM0005.DELFLG <> @P2 " _
            & " WHERE OIT0004.LINKNO = @P1 " _
            & " AND OIT0004.DELFLG <> @P2 "

            '& " LEFT JOIN OIL.OIT0005_SHOZAI OIT0005 ON " _
            '& "       OIT0004.TANKNUMBER = OIT0005.TANKNUMBER " _
            '& "       AND OIT0005.DELFLG <> @P2 " _
            '& " LEFT JOIN OIL.OIM0003_PRODUCT OIM0003 ON " _
            '& "       OIT0005.LASTOILCODE = OIM0003.OILCODE " _
            '& "       AND OIT0004.OFFICECODE = OIM0003.OFFICECODE " _
            '& "       AND OIM0003.DELFLG <> @P2 " _

            SQLStr &=
                  " ORDER BY " _
                & "    OIT0004.LINKDETAILNO"
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

                Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P0", SqlDbType.Int)          '貨車連結順序表明細数(新規作成)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 7)  '空車発駅コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 7)  '空車着駅コード
                Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 20) '赤丸
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 20) '黄丸
                Dim PARA7 As SqlParameter = SQLcmd.Parameters.Add("@P7", SqlDbType.NVarChar, 20) '緑丸
                Dim PARA8 As SqlParameter = SQLcmd.Parameters.Add("@P8", SqlDbType.NVarChar, 20) '利用可能日

                PARA5.Value = C_INSPECTIONALERT.ALERT_RED
                PARA6.Value = C_INSPECTIONALERT.ALERT_YELLOW
                PARA7.Value = C_INSPECTIONALERT.ALERT_GREEN

                If work.WF_SEL_PANEL.Value <> "1" Or
                    work.WF_SEL_CREATEFLG.Text <> "1" Then
                    PARA0.Value = O_INSCNT
                    PARA3.Value = ""
                    PARA4.Value = ""
                    PARA8.Value = ""
                Else
                    PARA0.Value = CONST_INIT_ROWS
                    PARA3.Value = LblDepstationName.Text
                    PARA4.Value = LblRetstationName.Text
                    PARA8.Value = AvailableYMD.Text
                End If

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

        '○関連チェック
        WW_Check(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '〇日付妥当性チェック
        WW_CheckValidityDate(WW_ERRCODE)
        If WW_ERRCODE = "ERR" Then
            Exit Sub
        End If

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("ORG", work.WF_SEL_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)
        '登録営業所
        CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_DUMMY)
        '空車発駅
        CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
        '空車着駅
        CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)

        'パネルロックを解除
        work.WF_SEL_PANEL.Value = "1"

        WF_PANELFLG.Value = "1"

        '○ GridView初期設定
        '○ 画面表示データ再取得(貨車連結表(明細)画面表示データ取得)
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon, 0)
        End Using

        Dim i As Integer = 0
        For Each OIT0002row As DataRow In OIT0002tbl.Rows
            i += 1
            OIT0002row("LINEORDER") = i        '貨物駅入線順

        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        End If

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            '利用可能日
                            Case "AvailableYMD"
                                .WF_Calendar.Text = AvailableYMD.Text
                        '(予定)空車着日
                            Case "TxtEmpDate"
                                .WF_Calendar.Text = TxtEmpDate.Text
                        '(実績)空車着日
                            Case "TxtActEmpDate"
                                .WF_Calendar.Text = TxtActEmpDate.Text
                        End Select
                        .ActiveCalendar()
                    Case Else   '以外
                        '会社コード
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            '運用部署
                            Case "WF_ORG"
                                prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)

                            '登録営業所
                            Case "TxtOrderOffice"
                                prmData = work.CreateSALESOFFICEParam(Master.USER_ORG, TxtOrderOffice.Text)

                            '本線列車
                            Case "TxtHeadOfficeTrain"
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, TxtHeadOfficeTrain.Text)

                            '空車発駅（着駅）
                            Case "TxtDepstation"
                                prmData = work.CreateSTATIONPTParam(work.WF_SEL_OFFICECODE.Text + "2", TxtDepstation.Text)

                            '空車着駅（発駅）
                            Case "TxtRetstation"
                                prmData = work.CreateSTATIONPTParam(work.WF_SEL_OFFICECODE.Text + "1", TxtRetstation.Text)

                            'タンク車№
                            Case "TANKNUMBER"
                                prmData = work.CreateSALESOFFICEParam(work.WF_SEL_OFFICECODE.Text, "")
                                '### LeftBoxマルチ対応(20200217) START #####################################################
                                '↓暫定一覧対応 2020/02/13 グループ会社版を復活させ石油システムに合わない部分は直す
                                Dim enumVal = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(WF_LeftMViewChange.Value)), LIST_BOX_CLASSIFICATION)
                                .SetTableList(enumVal, WW_DUMMY, prmData)
                                .ActiveTable()
                                Return
                                '↑暫定一覧対応 2020/02/13
                                '### LeftBoxマルチ対応(20200217) END   #####################################################
                        End Select
                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
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
                CODENAME_get("CAMPCODE", TxtOrderOffice.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            '登録営業所
            Case "TxtOrderOffice"
                CODENAME_get("ORG", TxtHeadOfficeTrain.Text, WF_ORG_TEXT.Text, WW_RTN_SW)
            '本線列車
            Case "TxtHeadOfficeTrain"
                Dim WW_GetValue() As String = {"", "", "", "", ""}
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)
                'FixvalueMasterSearch("", "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)

                '空車発駅（着駅）
                TxtDepstation.Text = WW_GetValue(2)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '空車着駅（発駅）
                TxtRetstation.Text = WW_GetValue(1)
                CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()
            '空車発駅（着駅）
            Case "TxtDepstation"
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            '空車着駅（発駅）
            Case "TxtRetstation"
                CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Select Case WF_FIELD.Value
                Case "TxtHeadOfficeTrain"
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR, "登録営業所")
                Case "AvailableYMD"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "利用可能日")
                Case "TxtDepstation"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "空車発駅")
                Case "TxtRetstation"
                    Master.Output(C_MESSAGE_NO.OIL_STATION_MASTER_NOTFOUND, C_MESSAGE_TYPE.ERR, "空車着駅")
                Case Else
                    Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End Select
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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        '○ 選択内容を取得
        '### LeftBoxマルチ対応(20200217) START #####################################################
        If leftview.ActiveViewIdx = 2 Then
            '一覧表表示時
            Dim selectedLeftTableVal = leftview.GetLeftTableValue()
            WW_SelectValue = selectedLeftTableVal(LEFT_TABLE_SELECTED_KEY)
            WW_SelectText = selectedLeftTableVal("VALUE1")
            '### LeftBoxマルチ対応(20200217) END   #####################################################
        ElseIf leftview.WF_LeftListBox.SelectedIndex >= 0 Then
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
                '別の登録営業所が設定された場合
                If TxtOrderOffice.Text <> WW_SelectText Then
                    TxtOrderOffice.Text = WW_SelectText
                    work.WF_SEL_OFFICECODE.Text = WW_SelectValue
                    work.WF_SEL_OFFICENAME.Text = WW_SelectText

                    '本線列車, 空車発駅, 空車着駅のテキストボックスを初期化
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

                    '○ 一覧の初期化画面表示データ取得
                    Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()       'DataBase接続

                        MAPDataGet(SQLcon, 0)
                    End Using

                    '○ 画面表示データ保存
                    Master.SaveTable(OIT0002tbl)

                End If

                TxtOrderOffice.Focus()

            Case "TxtHeadOfficeTrain"   '本線列車

                If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
                    Dim selectedText = Me.Request.Form("commonLeftListSelectedText")
                    Dim selectedItem = leftview.WF_LeftListBox.Items.FindByText(selectedText)
                    WW_SelectValue = selectedItem.Value
                    WW_SelectText = selectedItem.Text
                End If

                TxtHeadOfficeTrain.Text = WW_SelectValue
                TxtHeadOfficeTrainName.Text = WW_SelectText
                'FixvalueMasterSearch("", "TRAINNUMBER", WW_SelectValue, WW_GetValue)
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TRAINNUMBER_FIND", WW_SelectText, WW_GetValue)

                '空車発駅（着駅）
                TxtDepstation.Text = WW_GetValue(2)
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_DUMMY)
                '空車着駅（発駅）
                TxtRetstation.Text = WW_GetValue(1)
                CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_DUMMY)
                TxtHeadOfficeTrain.Focus()

            Case "AvailableYMD"       '利用可能日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        AvailableYMD.Text = ""
                    Else
                        AvailableYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                AvailableYMD.Focus()

            Case "TxtDepstation"        '空車発駅（着駅）
                TxtDepstation.Text = WW_SelectValue
                LblDepstationName.Text = WW_SelectText
                TxtDepstation.Focus()

            Case "TxtRetstation"        '空車着駅（発駅）
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
                        TxtEmpDate.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                TxtEmpDate.Focus()

            Case "TxtActEmpDate"           '(実績)空車着日
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        TxtActEmpDate.Text = ""
                    Else
                        TxtActEmpDate.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                TxtActEmpDate.Focus()

            Case "TANKNUMBER"   '(一覧)タンク車№
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
                'タンク車№を一覧に設定
                If WF_FIELD.Value = "TANKNUMBER" Then
                    Dim WW_TANKNUMBER As String = WW_SETVALUE
                    Dim WW_Now As String = Now.ToString("yyyy/MM/dd")
                    updHeader.Item(WF_FIELD.Value) = WW_TANKNUMBER

                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)
                    'FixvalueMasterSearch("", "TANKNUMBER", WW_TANKNUMBER, WW_GetValue)

                    '前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("PREOILCODE") = WW_GetValue(1)
                    'CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                    'updHeader.Item("PREOILNAME") = WW_LASTOILNAME
                    updHeader.Item("PREOILNAME") = WW_GetValue(4)
                    updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                    updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)

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
            Case "WF_ORG"               '運用部署
                WF_ORG.Focus()
            Case "TxtHeadOfficeTrain"   '本線列車
                TxtHeadOfficeTrain.Focus()
            Case "AvailableYMD"         '利用可能日
                TxtHeadOfficeTrain.Focus()
            Case "TxtDepstation"        '空車発駅（着駅）
                TxtDepstation.Focus()
            Case "TxtRetstation"        '空車着駅（発駅）
                TxtRetstation.Focus()
            Case "TxtEmpDate"           '(予定)空車着日
                TxtEmpDate.Focus()
            Case "TxtActEmpDate"        '(実績)空車着日
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
    ''' <remarks></remarks>
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
                    & "  WHERE LINKNO       = @P01       " _
                    & "    AND LINKDETAILNO = @P02       " _
                    & "    AND DELFLG      <> @P03       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar)
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar)
            Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 1)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.DateTime)

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
                    PARA03.Value = C_DELETE_FLG.DELETE

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
            & "  SELECT 'L' + FORMAT(GETDATE(),'yyyyMMdd') + '01' AS LINKNO" _
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
            & "  FROM OIL.OIT0004_LINK OIT0004" _
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
            & " , '0'                                            AS UPDTIMSTP " _
            & " , 1                                              AS 'SELECT' " _
            & " , 0                                              AS HIDDEN " _
            & " , ''                                             AS LINETRAINNO " _
            & " , ''                                             AS LINEORDER " _
            & " , ''                                             AS TANKNUMBER " _
            & " , ''                                             AS PREOILCODE " _
            & " , ''                                             AS PREOILNAME " _
            & " , ''                                             AS PREORDERINGTYPE " _
            & " , ''                                             AS PREORDERINGOILNAME " _
            & " , ''                                             AS DEPSTATION " _
            & " , @P02                                           AS DEPSTATIONNAME " _
            & " , ''                                             AS RETSTATION " _
            & " , @P03                                           AS RETSTATIONNAME " _
            & " , ''                                             AS JRINSPECTIONALERT " _
            & " , ''                                             AS JRINSPECTIONDATE " _
            & " , ''                                             AS JRINSPECTIONALERTSTR " _
            & " , ''                                             AS JRALLINSPECTIONALERT " _
            & " , ''                                             AS JRALLINSPECTIONDATE " _
            & " , ''                                             AS JRALLINSPECTIONALERTSTR " _
            & " , @P04                                           AS AVAILABLEYMD " _
            & " , @P00                                           AS DELFLG" _
            & " , @P01                                           AS LINKNO" _
            & " , FORMAT(ROW_NUMBER() OVER(ORDER BY name),'000') AS LINKDETAILNO" _
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

                Dim PARA0 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)   '削除フラグ
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11)  '貨車連結順序表受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)  '空車発駅名
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)  '空車着駅名
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 11)  '利用可能日

                Dim strOrderNo As String = ""
                Dim intDetailNo As Integer = 0

                PARA0.Value = C_DELETE_FLG.ALIVE

                For Each OIT0002WKrow As DataRow In OIT0002WKtbl.Rows
                    'strOrderNo = OIT0002WKrow("LINKNO")
                    intDetailNo = OIT0002WKrow("LINKDETAILNO")
                    PARA1.Value = OIT0002WKrow("LINKNO")
                    PARA2.Value = LblDepstationName.Text
                    PARA3.Value = LblRetstationName.Text
                    PARA4.Value = AvailableYMD.Text
                Next

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ テーブル検索結果をテーブル格納
                    OIT0002tbl.Load(SQLdr)
                End Using

                Dim cnt As Integer = 0
                Dim i As Integer = 0
                Dim j As Integer = 9000
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    cnt += 1

                    If cnt = intDetailNo Then
                        OIT0002row("LINECNT") = intDetailNo
                        OIT0002row("LINKDETAILNO") = Format(intDetailNo, "000")
                    End If
                    ''行追加データに既存の受注№を設定する。
                    ''既存データがなく新規データの場合は、SQLでの項目[受注№]を利用
                    'If OIT0002row("LINECNT") = 0 Then
                    '    If work.WF_SEL_CREATEFLG.Text = "1" Then
                    '        OIT0002row("LINKNO") = strOrderNo
                    '        OIT0002row("LINKDETAILNO") = intDetailNo.ToString("000")
                    '    Else
                    '        OIT0002row("LINKNO") = work.WF_SEL_LINKNO.Text
                    '        OIT0002row("LINKDETAILNO") = intDetailNo.ToString("000")
                    '    End If
                    'End If

                    ''削除対象データと通常データとそれぞれでLINECNTを振り分ける
                    'If OIT0002row("HIDDEN") = 1 Then
                    '    j += 1
                    '    OIT0002row("LINECNT") = j        'LINECNT
                    'Else
                    '    i += 1
                    '    OIT0002row("LINECNT") = i        'LINECNT
                    'End If
                    'intDetailNo += 1
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
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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

        '○ 画面表示データ一時保存
        Dim OIT0002Tmptbl As DataTable = OIT0002tbl.Copy
        Master.SaveTable(OIT0002Tmptbl)

        '○ 同一レコードチェック
        If isNormal(WW_ERRCODE) Then
            '貨車連結表DB追加・更新
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                WF_UPDERRFLG.Value = "0"

                WW_UpdateOrder(SQLcon)
            End Using

            If WF_UPDERRFLG.Value <> "1" Then
                '貨車連結表(一覧)画面表示データ取得
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()       'DataBase接続
                    WW_OrderListTBLSet(SQLcon)
                End Using
            End If
        End If

        '○ GridView初期設定
        '○ 画面表示データ再取得(貨車連結表(明細)画面表示データ取得)
        If WF_UPDERRFLG.Value <> "1" Then
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon, 0)
            End Using
        End If

        If WF_UPDERRFLG.Value = "1" Then
            '○ 画面表示一時データ復元
            OIT0002tbl = OIT0002Tmptbl.Copy
            '○ 一時格納Table Close
            If Not IsNothing(OIT0002tbl) Then
                OIT0002Tmptbl.Clear()
                OIT0002Tmptbl.Dispose()
                OIT0002Tmptbl = Nothing
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0002tbl)

        '○ 詳細画面クリア
        If isNormal(WW_ERRCODE) Then
            DetailBoxClear()
        End If

        '○ メッセージ表示
        If Not isNormal(WW_ERRCODE) Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
                Master.Output(C_MESSAGE_NO.REGISTRATION_RECORD_NOT_EXIST_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
                        Case "UPDTIMSTP"
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

            '○ 変更元情報をデフォルト設定
            If WW_COLUMNS.IndexOf("LINETRAINNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINEORDER") >= 0 AndAlso
                WW_COLUMNS.IndexOf("TANKNUMBER") >= 0 AndAlso
                WW_COLUMNS.IndexOf("PREOILCODE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DEPSTATIONNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("RETSTATIONNAME") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRINSPECTIONALERTSTR") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRALLINSPECTIONALERTSTR") >= 0 AndAlso
                WW_COLUMNS.IndexOf("JRALLINSPECTIONDATE") >= 0 AndAlso
                WW_COLUMNS.IndexOf("AVAILABLEYMD") >= 0 AndAlso
                WW_COLUMNS.IndexOf("DELFLG") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINKNO") >= 0 AndAlso
                WW_COLUMNS.IndexOf("LINKDETAILNO") >= 0 Then
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If XLSTBLrow("LINETRAINNO") = OIT0002row("LINETRAINNO") AndAlso
                        XLSTBLrow("LINEORDER") = OIT0002row("LINEORDER") AndAlso
                        XLSTBLrow("TANKNUMBER") = OIT0002row("TANKNUMBER") AndAlso
                        XLSTBLrow("PREOILCODE") = OIT0002row("PREOILCODE") AndAlso
                        XLSTBLrow("DEPSTATIONNAME") = OIT0002row("DEPSTATIONNAME") AndAlso
                        XLSTBLrow("RETSTATIONNAME") = OIT0002row("RETSTATIONNAME") AndAlso
                        XLSTBLrow("JRINSPECTIONALERTSTR") = OIT0002row("JRINSPECTIONALERTSTR") AndAlso
                        XLSTBLrow("JRINSPECTIONDATE") = OIT0002row("JRINSPECTIONDATE") AndAlso
                        XLSTBLrow("JRALLINSPECTIONALERTSTR") = OIT0002row("JRALLINSPECTIONALERTSTR") AndAlso
                        XLSTBLrow("JRALLINSPECTIONDATE") = OIT0002row("JRALLINSPECTIONDATE") AndAlso
                        XLSTBLrow("AVAILABLEYMD") = OIT0002row("AVAILABLEYMD") AndAlso
                        XLSTBLrow("DELFLG") = OIT0002row("DELFLG") AndAlso
                        XLSTBLrow("LINKNO") = OIT0002row("LINKNO") AndAlso
                        XLSTBLrow("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
                        OIT0002INProw.ItemArray = OIT0002row.ItemArray
                        Exit For
                    End If
                Next
            End If

            Dim WW_GetValue() As String = {"", "", "", "", ""}

            '○ 項目セット
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

                '●タンク車№から対象データを自動で設定
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TANKNUMBER", OIT0002INProw("TANKNUMBER"), WW_GetValue)
                'FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIT0002INProw("TANKNUMBER"), WW_GetValue)
                OIT0002INProw("PREOILCODE") = WW_GetValue(1)

                '交検日
                OIT0002INProw("JRINSPECTIONDATE") = WW_GetValue(2)

                '交検日アラート
                If WW_GetValue(2) <> "" Then
                    Dim WW_JRINSPECTIONCNT As String = DateDiff(DateInterval.Day, Date.Parse(Now.ToString("yyyy/MM/dd")), Date.Parse(WW_GetValue(2)))
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
                            OIT0002INProw("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            OIT0002INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                        Case "2"
                            OIT0002INProw("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            OIT0002INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                        Case "3"
                            OIT0002INProw("JRINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                            OIT0002INProw("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                    End Select
                Else
                    OIT0002INProw("JRINSPECTIONALERT") = ""
                End If

                '全検日
                OIT0002INProw("JRALLINSPECTIONDATE") = WW_GetValue(3)

                '全検日アラート
                If WW_GetValue(3) <> "" Then
                    Dim WW_JRALLINSPECTIONCNT As String = DateDiff(DateInterval.Day, Date.Parse(Now.ToString("yyyy/MM/dd")), Date.Parse(WW_GetValue(3)))
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
                            OIT0002INProw("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:red;"">●</div>"
                            OIT0002INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED
                        Case "2"
                            OIT0002INProw("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:yellow;"">●</div>"
                            OIT0002INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW
                        Case "3"
                            OIT0002INProw("JRALLINSPECTIONALERT") = "<div style=""text-align:center;font-size:22px;color:green;"">●</div>"
                            OIT0002INProw("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_GREEN
                    End Select
                Else
                    OIT0002INProw("JRALLINSPECTIONALERT") = ""
                End If

                '前回油種名(前回油種コードから油種名を取得し設定)
                WW_GetValue = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN", OIT0002INProw("PREOILCODE"), WW_GetValue)
                OIT0002INProw("PREOILNAME") = WW_GetValue(0)
            End If

            '空車発駅（着駅）
            OIT0002INProw("DEPSTATIONNAME") = LblDepstationName.Text

            '空車着駅（発駅）
            OIT0002INProw("RETSTATIONNAME") = LblRetstationName.Text

            '利用可能日
            OIT0002INProw("AVAILABLEYMD") = AvailableYMD.Text

            '削除フラグ
            If WW_COLUMNS.IndexOf("DELFLG") >= 0 Then
                OIT0002INProw("DELFLG") = XLSTBLrow("DELFLG")
            Else
                OIT0002INProw("DELFLG") = "0"
            End If

            '貨車連結順序表№
            If WW_COLUMNS.IndexOf("LINKNO") >= 0 Then
                OIT0002INProw("LINKNO") = XLSTBLrow("LINKNO")
            End If

            '貨車連結順序表明細№
            If WW_COLUMNS.IndexOf("LINKDETAILNO") >= 0 Then
                OIT0002INProw("LINKDETAILNO") = XLSTBLrow("LINKDETAILNO")
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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
        Dim WW_GetValue() As String = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}

        Select Case WF_FIELD.Value
            '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "LINETRAINNO"          '入線番号
                updHeader.Item("LINETRAINNO") = WW_ListValue

            '(★サーバー側で設定しているため必要ないが念のため残す(20200302))
            Case "LINEORDER"            '入線順序
                updHeader.Item("LINEORDER") = WW_ListValue

            Case "TANKNUMBER"           '(一覧)タンク車№
                If WW_ListValue <> "" Then
                    FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)
                    'FixvalueMasterSearch(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", WW_ListValue, WW_GetValue)

                    'タンク車№
                    updHeader.Item("TANKNUMBER") = WW_ListValue

                    '前回油種
                    Dim WW_LASTOILNAME As String = ""
                    updHeader.Item("PREOILCODE") = WW_GetValue(1)
                    'CODENAME_get("PRODUCTPATTERN", WW_GetValue(1), WW_LASTOILNAME, WW_DUMMY)
                    'updHeader.Item("PREOILNAME") = WW_LASTOILNAME
                    updHeader.Item("PREOILNAME") = WW_GetValue(4)
                    updHeader.Item("PREORDERINGTYPE") = WW_GetValue(5)
                    updHeader.Item("PREORDERINGOILNAME") = WW_GetValue(6)

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
                Else
                    'タンク車№が空の場合
                    updHeader.Item("TANKNUMBER") = WW_ListValue
                    updHeader.Item("PREOILCODE") = WW_ListValue
                    updHeader.Item("PREOILNAME") = WW_ListValue
                    updHeader.Item("JRINSPECTIONDATE") = WW_ListValue
                    updHeader.Item("JRINSPECTIONALERT") = WW_ListValue
                    updHeader.Item("JRALLINSPECTIONDATE") = WW_ListValue
                    updHeader.Item("JRALLINSPECTIONALERT") = WW_ListValue
                End If
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
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim dateErrFlag As String = ""

        '○ 単項目チェック
        '登録営業所
        If TxtOrderOffice.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "登録営業所", needsPopUp:=True)
            TxtOrderOffice.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OFFICECODE", TxtOrderOffice.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("SALESOFFICE", work.WF_SEL_OFFICECODE.Text, TxtOrderOffice.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "登録営業所 : " & work.WF_SEL_OFFICECODE.Text, needsPopUp:=True)
                TxtOrderOffice.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="登録営業所", needsPopUp:=True)
            TxtOrderOffice.Focus()
            WW_CheckMES1 = "登録営業所入力エラー。"
            WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '本線列車
        If TxtHeadOfficeTrain.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "本線列車", needsPopUp:=True)
            TxtHeadOfficeTrain.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRAINNO", TxtHeadOfficeTrain.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If Not isNormal(WW_CS0024FCHECKERR) Then
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="本線列車", needsPopUp:=True)
            TxtHeadOfficeTrain.Focus()
            WW_CheckMES1 = "本線列車入力エラー。"
            WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '利用可能日
        If AvailableYMD.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "利用可能日", needsPopUp:=True)
            AvailableYMD.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        '年月日チェック
        WW_CheckDate(AvailableYMD.Text, "利用可能日", WW_CS0024FCHECKERR, dateErrFlag)
        If dateErrFlag = "1" Then
            AvailableYMD.Focus()
            WW_CheckMES1 = "利用可能日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            O_RTN = "ERR"
            Exit Sub
        Else
            AvailableYMD.Text = CDate(AvailableYMD.Text).ToString("yyyy/MM/dd")
        End If
        '日付過去チェック
        If AvailableYMD.Text <> "" Then
            Dim WW_DATE_AD As Date
            Try
                Date.TryParse(AvailableYMD.Text, WW_DATE_AD)

                If WW_DATE_AD < Today Then
                    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="利用可能日", needsPopUp:=True)
                    AvailableYMD.Focus()
                    WW_CheckMES1 = "利用可能日入力エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, AvailableYMD.Text)
                AvailableYMD.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        '空車発駅（着駅）
        If TxtDepstation.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "空車発駅", needsPopUp:=True)
            TxtDepstation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEPSTATION", TxtDepstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "空車発駅 : " & TxtDepstation.Text, needsPopUp:=True)
                TxtDepstation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="空車発駅", needsPopUp:=True)
            TxtDepstation.Focus()
            WW_CheckMES1 = "空車発駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '空車着駅（発駅）
        If TxtRetstation.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "空車着駅", needsPopUp:=True)
            TxtRetstation.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RETSTATION", TxtRetstation.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("RETSTATION", TxtRetstation.Text, LblRetstationName.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.Output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR,
                              "空車着駅 : " & TxtRetstation.Text, needsPopUp:=True)
                TxtRetstation.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.Output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, I_PARA01:="空車着駅", needsPopUp:=True)
            TxtRetstation.Focus()
            WW_CheckMES1 = "空車着駅入力エラー。"
            WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            O_RTN = "ERR"
            Exit Sub
        End If

        '(予定)空車着日
        If TxtEmpDate.Text = "" Then
            Master.Output(C_MESSAGE_NO.PREREQUISITE_ERROR, C_MESSAGE_TYPE.ERR, "(予定)空車着日", needsPopUp:=True)
            TxtEmpDate.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If
        '年月日チェック
        WW_CheckDate(TxtEmpDate.Text, "(予定)空車着日", WW_CS0024FCHECKERR, dateErrFlag)
        If dateErrFlag = "1" Then
            TxtEmpDate.Focus()
            WW_CheckMES1 = "(予定)空車着日入力エラー。"
            WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
            O_RTN = "ERR"
            Exit Sub
        Else
            TxtEmpDate.Text = CDate(TxtEmpDate.Text).ToString("yyyy/MM/dd")
        End If
        '日付過去チェック
        If TxtEmpDate.Text <> "" Then
            Dim WW_DATE_ED As Date
            Try
                Date.TryParse(TxtEmpDate.Text, WW_DATE_ED)

                If WW_DATE_ED < Today Then
                    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="(予定)空車着日", needsPopUp:=True)
                    TxtEmpDate.Focus()
                    WW_CheckMES1 = "(予定)空車着日入力エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtEmpDate.Text)
                TxtEmpDate.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        '(実績)空車着日
        If TxtActEmpDate.Text = "" Then
            '何もしない
        Else
            '年月日チェック
            WW_CheckDate(TxtActEmpDate.Text, "(実績)空車着日", WW_CS0024FCHECKERR, dateErrFlag)
            If dateErrFlag = "1" Then
                TxtActEmpDate.Focus()
                WW_CheckMES1 = "(実績)空車着日入力エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                O_RTN = "ERR"
                Exit Sub
            Else
                TxtActEmpDate.Text = CDate(TxtActEmpDate.Text).ToString("yyyy/MM/dd")
            End If
        End If
        '日付過去チェック
        If TxtActEmpDate.Text <> "" Then
            Dim WW_DATE_AED As Date
            Try
                Date.TryParse(TxtActEmpDate.Text, WW_DATE_AED)

                If WW_DATE_AED < Today Then
                    Master.Output(C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR, C_MESSAGE_TYPE.ERR, I_PARA01:="(実績)空車着日", needsPopUp:=True)
                    TxtActEmpDate.Focus()
                    WW_CheckMES1 = "(実績)空車着日入力エラー。"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_PASTDATE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                End If
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ABORT, TxtActEmpDate.Text)
                TxtActEmpDate.Focus()
                O_RTN = "ERR"
                Exit Sub
            End Try
        End If

        '(一覧)タンク車No(重複チェック)
        Dim OIT0002tbl_DUMMY As DataTable = OIT0002tbl.Copy
        Dim OIT0002tbl_dv As DataView = New DataView(OIT0002tbl_DUMMY)
        Dim chkTankNo As String = ""

        'タンク車Noでソートし、重複がないかチェックする。
        OIT0002tbl_dv.Sort = "TANKNUMBER"
        For Each drv As DataRowView In OIT0002tbl_dv
            If drv("TANKNUMBER") <> "" AndAlso chkTankNo = drv("TANKNUMBER") Then
                Master.Output(C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "タンク車№重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_OILTANKNO_REPEAT_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR"
                Exit Sub
            End If
            chkTankNo = drv("TANKNUMBER")
        Next

        '入線順序でソートし、重複がないかチェックする。
        OIT0002tbl_dv.Sort = "LINEORDER"
        For Each drv As DataRowView In OIT0002tbl_dv
            If drv("LINEORDER") <> "" AndAlso chkTankNo = drv("LINEORDER") Then
                Master.Output(C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                WW_CheckMES1 = "入線順序重複エラー。"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_LINEORDER_REPEAT_ERROR
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, drv.Row)
                O_RTN = "ERR"
                Exit Sub
            End If
            chkTankNo = drv("LINEORDER")
        Next

        '○ 正常メッセージ
        Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 年月日妥当性チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckValidityDate(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim iresult As Integer
        Dim decChkDay As Decimal

        '○ 過去日付チェック
        '例) iresult = dt1.Date.CompareTo(dt2.Date)
        '    iresultの意味
        '     0 : dt1とdt2は同じ日
        '    -1 : dt1はdt2より前の日
        '     1 : dt1はdt2より後の日
        '(予定)空車着日 と　利用可能日を比較
        iresult = Date.Parse(TxtEmpDate.Text).CompareTo(Date.Parse(AvailableYMD.Text))
        If iresult = 1 Then
            decChkDay = (Date.Parse(TxtEmpDate.Text) - Date.Parse(AvailableYMD.Text)).TotalDays
            '(予定)空車着日 と　利用可能日の日数を取得し判断
            '1 : (予定)空車着日が利用可能日の翌日の日付
            '2 : (予定)空車着日が利用可能日の翌々日の日付
            '※2以上の日数は未来日としてエラーの位置づけとする。
            If decChkDay > 1 Then
                Master.Output(C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_Y, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                TxtEmpDate.Focus()
                WW_CheckMES1 = "(予定)空車着日"
                WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_Y
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
                Exit Sub
            End If
        End If

        '(実績)空車着日 と　利用可能日を比較
        If TxtActEmpDate.Text <> "" Then
            iresult = Date.Parse(TxtActEmpDate.Text).CompareTo(Date.Parse(AvailableYMD.Text))
            If iresult = 1 Then
                decChkDay = (Date.Parse(TxtActEmpDate.Text) - Date.Parse(AvailableYMD.Text)).TotalDays
                '(実績)空車着日 と　利用可能日の日数を取得し判断
                '1 : (実績)空車着日が利用可能日の翌日の日付
                '2 : (実績)空車着日が利用可能日の翌々日の日付
                '※2以上の日数は未来日としてエラーの位置づけとする。
                If decChkDay > 1 Then
                    Master.Output(C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_J, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
                    TxtActEmpDate.Focus()
                    WW_CheckMES1 = "(実績)空車着日"
                    WW_CheckMES2 = C_MESSAGE_NO.OIL_DATE_AVAILABLEDATE_ERROR_J
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        End If

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
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        WW_ERR_MES &= ControlChars.NewLine & "  --> 登録営業所         =" & TxtOrderOffice.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 本線列車           =" & TxtHeadOfficeTrain.Text & " , "
        WW_ERR_MES &= ControlChars.NewLine & "  --> 利用可能日         =" & AvailableYMD.Text & " , "
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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 利用可能日             =" & OIT0002row("AVAILABLEYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線列車番号       =" & OIT0002row("LINETRAINNO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 入線順             =" & OIT0002row("LINEORDER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車番号       =" & OIT0002row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回油種　　       =" & OIT0002row("PREOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車発駅   　　　　=" & OIT0002row("DEPSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 空車着駅   　　　　=" & OIT0002row("RETSTATION") & " , "
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
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '') AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '')    AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')  AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '')    AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')   AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '')   AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '')   AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '')   AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '')   AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '')   AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.VALUE6), '')   AS VALUE6" _
                & " , ISNULL(RTRIM(VIW0001.VALUE7), '')   AS VALUE7" _
                & " , ISNULL(RTRIM(VIW0001.VALUE8), '')   AS VALUE8" _
                & " , ISNULL(RTRIM(VIW0001.VALUE9), '')   AS VALUE9" _
                & " , ISNULL(RTRIM(VIW0001.VALUE10), '')  AS VALUE10" _
                & " , ISNULL(RTRIM(VIW0001.VALUE11), '')  AS VALUE11" _
                & " , ISNULL(RTRIM(VIW0001.VALUE12), '')  AS VALUE12" _
                & " , ISNULL(RTRIM(VIW0001.VALUE13), '')  AS VALUE13" _
                & " , ISNULL(RTRIM(VIW0001.VALUE14), '')  AS VALUE14" _
                & " , ISNULL(RTRIM(VIW0001.VALUE15), '')  AS VALUE15" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '')   AS DELFLG" _
                & " FROM  OIL.VIW0001_FIXVALUE VIW0001" _
                & " WHERE VIW0001.CLASS = @P01" _
                & " AND VIW0001.DELFLG <> @P02"

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
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)

                PARA01.Value = I_CLASS
                PARA02.Value = C_DELETE_FLG.DELETE

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
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0002WKrow("VALUE" & i.ToString())
                        Next
                        'O_VALUE(0) = OIT0002WKrow("VALUE1")
                        'O_VALUE(1) = OIT0002WKrow("VALUE2")
                        'O_VALUE(2) = OIT0002WKrow("VALUE3")
                        'O_VALUE(3) = OIT0002WKrow("VALUE4")
                        'O_VALUE(4) = OIT0002WKrow("VALUE5")
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
    ''' 貨車連結表TBL登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrder(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim LNG_TxtHTank As Long = "0"                 '油種(ハイオク)
        Dim LNG_TxtRTank As Long = "0"                  '油種(レギュラー)
        Dim LNG_TxtTTank As Long = "0"                  '油種(灯油)
        Dim LNG_TxtMTTank As Long = "0"            '油種(未添加灯油)
        Dim LNG_TxtKTank1 As Long = "0"            '油種(軽油)
        'Dim LNG_TxtKTank2 As Long = "0"
        Dim LNG_TxtK3Tank1 As Long = "0"              '３号軽油
        'Dim LNG_TxtK3Tank2 As Long = "0"
        Dim LNG_TxtK5Tank As Long = "0"              '５号軽油
        Dim LNG_TxtK10Tank As Long = "0"               '１０号軽油
        Dim LNG_TxtLTank1 As Long = "0"                'ＬＳＡ
        'Dim LNG_TxtLTank2 As Long = "0"
        Dim LNG_TxtATank As Long = "0"               'Ａ重油
        Dim CNT_Total As Long = "0"      '合計

        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIT0004_LINK" _
            & "    WHERE" _
            & "        LINKNO          = @P01 " _
            & "   AND  LINKDETAILNO    = @P02 " _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIT0004_LINK" _
            & "    SET" _
            & "          AVAILABLEYMD    = @P03 , STATUS             = @P04" _
            & "        , INFO            = @P05 , PREORDERNO         = @P06" _
            & "        , TRAINNO         = @P07 , TRAINNAME          = @P19, OFFICECODE       = @P08" _
            & "        , DEPSTATION      = @P09 , DEPSTATIONNAME     = @P10" _
            & "        , RETSTATION      = @P11 , RETSTATIONNAME     = @P12" _
            & "        , EMPARRDATE      = @P13 , ACTUALEMPARRDATE   = @P14" _
            & "        , LINETRAINNO     = @P15 , LINEORDER          = @P16" _
            & "        , TANKNUMBER      = @P17" _
            & "        , PREOILCODE      = @P18 , PREOILNAME         = @P20" _
            & "        , PREORDERINGTYPE = @P21 , PREORDERINGOILNAME = @P22" _
            & "        , UPDYMD          = @P87 , UPDUSER            = @P88" _
            & "        , UPDTERMID       = @P89 , RECEIVEYMD         = @P90" _
            & "    WHERE" _
            & "        LINKNO            = @P01 " _
            & "        AND  LINKDETAILNO = @P02 " _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIT0004_LINK" _
            & "        ( LINKNO    , LINKDETAILNO  , AVAILABLEYMD, STATUS          , INFO " _
            & "        , PREORDERNO, TRAINNO       , TRAINNAME   , OFFICECODE      , DEPSTATION     , DEPSTATIONNAME " _
            & "        , RETSTATION, RETSTATIONNAME, EMPARRDATE  , ACTUALEMPARRDATE, LINETRAINNO " _
            & "        , LINEORDER , TANKNUMBER    , PREOILCODE  , PREOILNAME      , PREORDERINGTYPE, PREORDERINGOILNAME " _
            & "        , DELFLG    , INITYMD       , INITUSER    , INITTERMID " _
            & "        , UPDYMD    , UPDUSER       , UPDTERMID   , RECEIVEYMD)" _
            & "    VALUES" _
            & "        ( @P01, @P02, @P03, @P04, @P05" _
            & "        , @P06, @P07, @P19, @P08, @P09, @P10" _
            & "        , @P11, @P12, @P13, @P14, @P15" _
            & "        , @P16, @P17, @P18, @P20, @P21, @P22" _
            & "        , @P83, @P84, @P85, @P86" _
            & "        , @P87, @P88, @P89, @P90) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
            " SELECT" _
            & "      LINKNO" _
            & "    , LINKDETAILNO" _
            & "    , AVAILABLEYMD" _
            & "    , STATUS" _
            & "    , INFO" _
            & "    , PREORDERNO" _
            & "    , TRAINNO" _
            & "    , TRAINNAME" _
            & "    , OFFICECODE" _
            & "    , DEPSTATION" _
            & "    , DEPSTATIONNAME" _
            & "    , RETSTATION" _
            & "    , RETSTATIONNAME" _
            & "    , EMPARRDATE" _
            & "    , ACTUALEMPARRDATE" _
            & "    , LINETRAINNO" _
            & "    , LINEORDER" _
            & "    , TANKNUMBER" _
            & "    , PREOILCODE" _
            & "    , PREOILNAME" _
            & "    , PREORDERINGTYPE" _
            & "    , PREORDERINGOILNAME" _
            & "    , DELFLG" _
            & "    , INITYMD" _
            & "    , INITUSER" _
            & "    , INITTERMID" _
            & "    , UPDYMD" _
            & "    , UPDUSER" _
            & "    , UPDTERMID" _
            & "    , RECEIVEYMD" _
            & " FROM" _
            & "    OIL.OIT0004_LINK" _
            & " WHERE" _
            & "        LINKNO       = @P01" _
            & "   AND  LINKDETAILNO = @P02"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結順序表明細№
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '利用可能日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  'ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20) '情報
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 11) '前回オーダー№
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.NVarChar, 7)  '本線列車
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 20) '本線列車名
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.NVarChar, 6)  '登録営業所コード
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 7)  '空車発駅コード
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 40) '空車発駅名
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 7)  '空車着駅コード
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 40) '空車着駅名
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Date)         '空車着日（予定）
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.Date)         '空車着日（実績）
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4)  '入線列車番号
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 2)  '入線順
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 8)  'タンク車№
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 4)  '前回油種
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 40) '前回油種名
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 2)  '前回油種区分(受発注用)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 40) '前回油種名(受発注用)
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.DateTime)     '登録年月日
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.NVarChar, 20) '登録ユーザーID
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.NVarChar, 20) '登録端末
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.DateTime)     '更新年月日
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.NVarChar, 20) '更新ユーザーID
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.NVarChar, 20) '更新端末
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.DateTime)     '集信日時

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '貨車連結順序表№
                Dim JPARA02 As SqlParameter = SQLcmdJnl.Parameters.Add("@P02", SqlDbType.NVarChar, 3)  '貨車連結順序表明細№

                Dim CNT_ROWS As Long = 0

                Dim WW_GetValue() As String = {"", "", "", "", "", ""}
                FixvalueMasterSearch(work.WF_SEL_OFFICECODE.Text, "TRAINNUMBER", TxtHeadOfficeTrain.Text, WW_GetValue)

                '先にアラームの確認を行う
                Dim info As String = ""
                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    If Trim(OIT0002row("LINETRAINNO")) = "" Or
                                Trim(OIT0002row("LINEORDER")) = "" Or
                                Trim(OIT0002row("TANKNUMBER")) = "" Then
                        'エラー行は何もしない
                    Else
                        '受付情報が「検査間近有」の場合は優先して設定 
                        If OIT0002row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Or
                                   OIT0002row("JRINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW Or
                                   OIT0002row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_RED Or
                                    OIT0002row("JRALLINSPECTIONALERTSTR") = C_INSPECTIONALERT.ALERT_YELLOW Then
                            info = WW_ORDERINFOALERM_82
                            Exit For '優先度最大なので、判定にかかった段階でForループを抜ける

                            'タンク車数が「最大牽引タンク車数」より大きい場合
                        ElseIf Integer.Parse(TxtTotalTank.Text) > Integer.Parse(WW_GetValue(3)) Then
                            '80(タンク車数オーバー)を設定
                            info = WW_ORDERINFOALERM_80

                        Else
                            '何もしない
                        End If
                    End If
                Next

                For Each OIT0002row As DataRow In OIT0002tbl.Rows
                    '必須項目が全部空白の行はスキップする
                    If Trim(OIT0002row("LINETRAINNO")) = "" And
                            Trim(OIT0002row("LINEORDER")) = "" And
                            Trim(OIT0002row("TANKNUMBER")) = "" Then
                        '何もしない
                    Else    '必須項目が1～2個空白の行がある場合、エラーを出す
                        If Trim(OIT0002row("LINETRAINNO")) = "" Or
                                Trim(OIT0002row("LINEORDER")) = "" Or
                                Trim(OIT0002row("TANKNUMBER")) = "" Then

                            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_ORDER" & " （入線列車番号、入線順序、タンク車番号のいずれかが未入力です）", needsPopUp:=True)

                            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                            CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_ORDER"
                            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                            CS0011LOGWrite.TEXT = "必須項目エラー"
                            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.PREREQUISITE_ERROR
                            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                            WF_UPDERRFLG.Value = "1"
                            Exit Sub
                        Else '必須項目が入力されている行のカウント
                            CNT_ROWS += 1
                            Dim WW_DATENOW As DateTime = Date.Now

                            'DB更新
                            PARA01.Value = work.WF_SEL_LINKNO.Text            '貨車連結順序表№
                            PARA02.Value = OIT0002row("LINKDETAILNO")         '貨車連結順序表明細№
                            PARA03.Value = RTrim(CDate(AvailableYMD.Text).ToString("yyyy/MM/dd"))   '利用可能日
                            If work.WF_SEL_STATUS.Text <> "" Then             'ステータス
                                PARA04.Value = work.WF_SEL_STATUS.Text
                            Else
                                PARA04.Value = "1"
                            End If

                            If info = "" Then
                                If work.WF_SEL_INFO.Text <> "" Then             '情報
                                    PARA05.Value = work.WF_SEL_INFO.Text
                                Else
                                    PARA05.Value = ""
                                End If
                            Else
                                PARA05.Value = info
                            End If

                            If work.WF_SEL_PREORDERNO.Text <> "" Then             '前回オーダー№
                                PARA06.Value = work.WF_SEL_PREORDERNO.Text
                            Else
                                PARA06.Value = ""
                            End If

                            PARA07.Value = TxtHeadOfficeTrain.Text            '本線列車
                            PARA19.Value = TxtHeadOfficeTrainName.Text        '本線列車名
                            PARA08.Value = work.WF_SEL_OFFICECODE.Text        '登録営業所コード
                            PARA09.Value = TxtDepstation.Text                 '空車発駅（着駅）コード
                            PARA10.Value = LblDepstationName.Text             '空車発駅（着駅）名
                            PARA11.Value = TxtRetstation.Text                 '空車着駅（発駅）コード
                            PARA12.Value = LblRetstationName.Text             '空車着駅（発駅）名
                            PARA13.Value = RTrim(CDate(TxtEmpDate.Text).ToString("yyyy/MM/dd"))       '(予定)空車着日
                            If TxtActEmpDate.Text <> "" Then                                    '(実績)空車着日
                                PARA14.Value = CDate(TxtActEmpDate.Text).ToString("yyyy/MM/dd")
                            Else
                                PARA14.Value = DBNull.Value
                            End If
                            PARA15.Value = OIT0002row("LINETRAINNO")          '入線列車番号
                            PARA16.Value = OIT0002row("LINEORDER")            '入線順
                            PARA17.Value = OIT0002row("TANKNUMBER")           'タンク車№
                            PARA18.Value = OIT0002row("PREOILCODE")           '前回油種　
                            PARA20.Value = OIT0002row("PREOILNAME")           '前回油種名　
                            PARA21.Value = OIT0002row("PREORDERINGTYPE")      '前回油種区分(受発注用)　
                            PARA22.Value = OIT0002row("PREORDERINGOILNAME")   '前回油種名(受発注用)
                            Select Case PARA18.Value
                                Case BaseDllConst.CONST_HTank                 '油種(ハイオク)
                                    LNG_TxtHTank += 1
                                    CNT_Total += 1
                                Case BaseDllConst.CONST_RTank                  '油種(レギュラー)
                                    LNG_TxtRTank += 1
                                    CNT_Total += 1
                                Case BaseDllConst.CONST_TTank                  '油種(灯油)
                                    LNG_TxtTTank += 1
                                    CNT_Total += 1
                                Case BaseDllConst.CONST_MTTank            '油種(未添加灯油)
                                    LNG_TxtMTTank += 1
                                    CNT_Total += 1
                                Case BaseDllConst.CONST_KTank1            '油種(軽油)
                                    LNG_TxtKTank1 += 1
                                    CNT_Total += 1
                            'Case BaseDllConst.CONST_KTank2
                            'LNG_TxtKTank2 += 1
                            'CNT_Total += 1
                                Case BaseDllConst.CONST_K3Tank1              '３号軽油
                                    LNG_TxtK3Tank1 += 1
                                    CNT_Total += 1
                            'Case BaseDllConst.CONST_K3Tank2
                            'LNG_TxtK3Tank2 += 1
                            'CNT_Total += 1
                                Case BaseDllConst.CONST_K5Tank              '５号軽油
                                    LNG_TxtK5Tank += 1
                                    CNT_Total += 1
                                Case BaseDllConst.CONST_K10Tank               '１０号軽油
                                    LNG_TxtK10Tank += 1
                                    CNT_Total += 1
                                Case BaseDllConst.CONST_LTank1                'ＬＳＡ
                                    LNG_TxtLTank1 += 1
                                    CNT_Total += 1
                            'Case BaseDllConst.CONST_LTank2
                            'LNG_TxtLTank2 += 1
                            'CNT_Total += 1
                                Case BaseDllConst.CONST_ATank               'Ａ重油
                                    LNG_TxtATank += 1
                                    CNT_Total += 1
                            End Select
                            PARA83.Value = OIT0002row("DELFLG")               '削除フラグ
                            PARA84.Value = WW_DATENOW                         '登録年月日
                            PARA85.Value = Master.USERID                      '登録ユーザーID
                            PARA86.Value = Master.USERTERMID                  '登録端末
                            PARA87.Value = WW_DATENOW                         '更新年月日
                            PARA88.Value = Master.USERID                      '更新ユーザーID
                            PARA89.Value = Master.USERTERMID                  '更新端末
                            PARA90.Value = C_DEFAULT_YMD

                            OIT0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            SQLcmd.CommandTimeout = 300
                            SQLcmd.ExecuteNonQuery()

                            '更新ジャーナル出力
                            JPARA01.Value = OIT0002row("LINKNO")
                            JPARA02.Value = OIT0002row("LINKDETAILNO")

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
                                CS0020JOURNAL.TABLENM = "OIT0002D"
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
                        End If
                    End If
                Next

                '油種(ハイオク)
                TxtHTank.Text = LNG_TxtHTank
                '油種(レギュラー)
                TxtRTank.Text = LNG_TxtRTank
                '油種(灯油)
                TxtTTank.Text = LNG_TxtTTank
                '油種(未添加灯油)
                TxtMTTank.Text = LNG_TxtMTTank
                '油種(軽油)
                TxtKTank.Text = LNG_TxtKTank1
                'Case CONST_TxtKTank2
                '    WF_SEL_HIGHOCTANE_TANKCAR.Text = LNG_TxtKTank2 + 1
                '３号軽油
                TxtK3Tank.Text = LNG_TxtK3Tank1
                'Case CONST_TxtK3Tank2
                '    TxtK3Tank2.Text = LNG_TxtK3Tank2 + 1
                '５号軽油
                TxtK5Tank.Text = LNG_TxtK5Tank
                '１０号軽油
                TxtK10Tank.Text = LNG_TxtK10Tank
                'ＬＳＡ
                TxtLTank.Text = LNG_TxtLTank1
                'Case CONST_TxtLTank2
                '    TxtLTank2.Text = LNG_TxtLTank2 + 1
                'Ａ重油
                TxtATank.Text = LNG_TxtATank
                'タンク車合計
                TxtTotalTank.Text = CNT_Total

                If CNT_ROWS = 0 Then　'必須項目が入力されている行がない場合のエラー
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_ORDER" & " （入力済みの行が存在しません）")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:OIT0002D UPDATE_INSERT_ORDER"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = "必須項目エラー"
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.PREREQUISITE_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
                    WF_UPDERRFLG.Value = "1"
                    Exit Sub
                End If

                work.WF_SEL_CREATEFLG.Text = 2 'エラーが発生しなかった場合、更新モードに切り替える
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0002D UPDATE_INSERT_ORDER", needsPopUp:=True)

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
                " SELECT DISTINCT" _
            & "    0                                                   AS LINECNT " _
            & "    , ''                                                AS OPERATION " _
            & "    , 1                                                 AS 'SELECT' " _
            & "    , 0                                                 AS HIDDEN " _
            & "    , ISNULL(RTRIM(OIT0004.LINKNO), '')                    AS LINKNO " _
            & "    , ISNULL(RTRIM(OIT0004.STATUS), '')                      AS STATUS " _
            & "    , ''                                                   AS STATUSNOW " _
            & "    , ISNULL(RTRIM(OIT0004.INFO), '')                      AS INFO " _
            & "    , CASE " _
            & "      WHEN ISNULL(RTRIM(OIT0004.INFO), '') ='80' Then 'タンク車数オーバー' " _
            & "      WHEN  ISNULL(RTRIM(OIT0004.INFO), '') ='82' Then '検査間近あり' " _
            & "      Else '' End AS INFONOW " _
            & "    , ISNULL(RTRIM(OIT0004.PREORDERNO), '99999999999')                AS PREORDERNO " _
            & "    , ISNULL(RTRIM(OIT0004.TRAINNO), '')                   AS TRAINNO " _
            & "    , ISNULL(RTRIM(OIT0004.OFFICECODE), '')                AS OFFICECODE " _
            & "    , ''                                                   AS OFFICENAME " _
            & "    , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')            AS DEPSTATIONNAME " _
            & "    , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')            AS RETSTATIONNAME " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL01 Then 1 Else 0 End) AS HTANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL02 Then 1 Else 0 End) AS RTANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL03 Then 1 Else 0 End) AS TTANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL04 Then 1 Else 0 End) AS MTTANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL05 Then 1 Else 0 End) AS KTANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL06 Then 1 Else 0 End) AS K3TANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL07 Then 1 Else 0 End) AS K5TANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL08 Then 1 Else 0 End) AS K10TANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL09 Then 1 Else 0 End) AS LTANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE =@OIL10 Then 1 Else 0 End) AS ATANK " _
            & "	   , SUM(CASE WHEN OIT0004.PREOILCODE <>'' Then 1 Else 0 End) AS TOTALTANK " _
            & "    , ISNULL(FORMAT(OIT0004.EMPARRDATE, 'yyyy/MM/dd'), '')      AS EMPARRDATE " _
            & "    , ISNULL(FORMAT(OIT0004.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')      AS ACTUALEMPARRDATE " _
            & "    , ISNULL(FORMAT(OIT0004.AVAILABLEYMD, 'yyyy/MM/dd'), '')    AS AVAILABLEYMD " _
            & "    , ISNULL(RTRIM(OIT0004.DELFLG), '')                    AS DELFLG " _
            & "    , ISNULL(RTRIM(OIT0004.DEPSTATION), '')            AS DEPSTATION " _
            & "    , ISNULL(RTRIM(OIT0004.RETSTATION), '')            AS RETSTATION " _
            & " FROM " _
            & "    OIL.OIT0004_LINK OIT0004 " _
            & " WHERE OIT0004.RETSTATION   = @P1" _
            & "   AND OIT0004.AVAILABLEYMD >= @P2" _
            & "   AND OIT0004.DELFLG       <> @P6"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '列車番号
        If Not String.IsNullOrEmpty(Me.TxtHeadOfficeTrain.Text) Then
            SQLStr &= String.Format("   AND OIT0004.TRAINNO = '{0}'", Me.TxtHeadOfficeTrain.Text)
        End If

        'ステータス状態
        If work.WF_SEL_SELECT.Text = "1" Then
            SQLStr &= String.Format("   AND OIT0004.STATUS = '{0}'", work.WF_SEL_SELECT.Text)
        End If

        '有効年月日（終了）
        'If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
        '    SQLStr &= String.Format("    AND OIT0004.AVAILABLEYMD     <= '{0}'", work.WF_SEL_ENDYMD.Text)
        'End If

        'If TxtHeadOfficeTrain.Text <> "" Then
        '    If work.WF_SEL_SELECT.Text = "1" Then
        '        SQLStr &=
        '          " WHERE" _
        '        & "    OIT0004.RETSTATION        = @P1" _
        '        & "    AND OIT0004.AVAILABLEYMD >= @P2" _
        '        & "    AND OIT0004.TRAINNO       = @P4" _
        '        & "    AND OIT0004.STATUS        = @P5" _
        '        & "    AND OIT0004.DELFLG       <> @P6"
        '    Else
        '        SQLStr &=
        '          " WHERE" _
        '        & "    OIT0004.RETSTATION        = @P1" _
        '        & "    AND OIT0004.AVAILABLEYMD >= @P2" _
        '        & "    AND OIT0004.TRAINNO       = @P4" _
        '        & "    AND OIT0004.DELFLG       <> @P6"
        '    End If
        'Else
        '    If work.WF_SEL_SELECT.Text = "1" Then
        '        SQLStr &=
        '          " WHERE" _
        '        & "    OIT0004.RETSTATION        = @P1" _
        '        & "    AND OIT0004.AVAILABLEYMD >= @P2" _
        '        & "    AND OIT0004.STATUS        = @P5" _
        '        & "    AND OIT0004.DELFLG       <> @P6"
        '    Else
        '        SQLStr &=
        '          " WHERE" _
        '        & "    OIT0004.RETSTATION        = @P1" _
        '        & "    AND OIT0004.AVAILABLEYMD >= @P2" _
        '        & "    AND OIT0004.DELFLG       <> @P6"
        '    End If
        'End If

        SQLStr &=
              " GROUP BY " _
            & "      LINKNO " _
            & "	    ,TRAINNO " _
            & "	    ,STATUS " _
            & "	    ,INFO " _
            & "	    ,PREORDERNO " _
            & "	    ,OFFICECODE " _
            & "	    ,DEPSTATIONNAME " _
            & "	    ,RETSTATIONNAME " _
            & "	    ,EMPARRDATE " _
            & "	    ,ACTUALEMPARRDATE " _
            & "     ,AVAILABLEYMD " _
            & "	    ,DELFLG " _
            & "	    ,DEPSTATION " _
            & "	    ,RETSTATION " _
            & " ORDER BY " _
            & "     TRAINNO "

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 7)         '空車着駅（発駅）コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.Date)                '有効年月日(From)
                'Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.Date)                '有効年月日(To)
                'Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 4)         '本線列車
                'Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.NVarChar, 1)         'ステータス
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = TxtRetstation.Text
                PARA2.Value = work.WF_SEL_STYMD.Text
                'PARA3.Value = work.WF_SEL_ENDYMD.Text
                'PARA4.Value = TxtHeadOfficeTrain.Text
                'PARA5.Value = work.WF_SEL_SELECT.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Dim OILPARA01 As SqlParameter = SQLcmd.Parameters.Add("@OIL01", SqlDbType.NVarChar, 4)    '油種(ハイオク)
                Dim OILPARA02 As SqlParameter = SQLcmd.Parameters.Add("@OIL02", SqlDbType.NVarChar, 4)    '油種(レギュラー)
                Dim OILPARA03 As SqlParameter = SQLcmd.Parameters.Add("@OIL03", SqlDbType.NVarChar, 4)    '油種(灯油)
                Dim OILPARA04 As SqlParameter = SQLcmd.Parameters.Add("@OIL04", SqlDbType.NVarChar, 4)    '油種(未添加灯油)
                Dim OILPARA05 As SqlParameter = SQLcmd.Parameters.Add("@OIL05", SqlDbType.NVarChar, 4)    '油種(軽油)
                Dim OILPARA06 As SqlParameter = SQLcmd.Parameters.Add("@OIL06", SqlDbType.NVarChar, 4)    '３号軽油
                Dim OILPARA07 As SqlParameter = SQLcmd.Parameters.Add("@OIL07", SqlDbType.NVarChar, 4)    '５号軽油
                Dim OILPARA08 As SqlParameter = SQLcmd.Parameters.Add("@OIL08", SqlDbType.NVarChar, 4)    '１０号軽油
                Dim OILPARA09 As SqlParameter = SQLcmd.Parameters.Add("@OIL09", SqlDbType.NVarChar, 4)    'ＬＳＡ
                Dim OILPARA10 As SqlParameter = SQLcmd.Parameters.Add("@OIL10", SqlDbType.NVarChar, 4)   'Ａ重油
                'Dim OILPARA11 As SqlParameter = SQLcmd.Parameters.Add("@OIL11", SqlDbType.NVarChar, 4)
                'Dim OILPARA12 As SqlParameter = SQLcmd.Parameters.Add("@OIL12", SqlDbType.NVarChar, 4)
                'Dim OILPARA13 As SqlParameter = SQLcmd.Parameters.Add("@OIL13", SqlDbType.NVarChar, 4)

                OILPARA01.Value = BaseDllConst.CONST_HTank                '油種(ハイオク)
                OILPARA02.Value = BaseDllConst.CONST_RTank                '油種(レギュラー)
                OILPARA03.Value = BaseDllConst.CONST_TTank                '油種(灯油)
                OILPARA04.Value = BaseDllConst.CONST_MTTank               '油種(未添加灯油)
                OILPARA05.Value = BaseDllConst.CONST_KTank1               '油種(軽油)
                OILPARA06.Value = BaseDllConst.CONST_K3Tank1              '３号軽油
                OILPARA07.Value = BaseDllConst.CONST_K5Tank               '５号軽油
                OILPARA08.Value = BaseDllConst.CONST_K10Tank              '１０号軽油
                OILPARA09.Value = BaseDllConst.CONST_LTank1               'ＬＳＡ
                OILPARA10.Value = BaseDllConst.CONST_ATank                'Ａ重油
                'OILPARA11.Value = BaseDllConst.CONST_K3Tank2
                'OILPARA12.Value = BaseDllConst.CONST_KTank2
                'OILPARA13.Value = BaseDllConst.CONST_LTank2

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

                    '◯名称取得
                    '受注営業所
                    CODENAME_get("SALESOFFICE", OIT0002row("OFFICECODE"), OIT0002row("OFFICENAME"), WW_DUMMY)                               '会社コード
                    '利用可否フラグ
                    CODENAME_get("USEPROPRIETY", OIT0002row("STATUS"), OIT0002row("STATUSNOW"), WW_DUMMY)                               '会社コード
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
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PREOILCODE", OIT0002INProw("PREOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "油種入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckListERR(WW_CheckMES1, WW_CheckMES2, OIT0002INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'タンク車(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIT0002INProw("TANKNUMBER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
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
                If OIT0002row("LINKNO") = OIT0002INProw("LINKNO") AndAlso
                    OIT0002row("LINKDETAILNO") = OIT0002INProw("LINKDETAILNO") Then
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
            If OIT0002INProw("LINKNO") = OIT0002row("LINKNO") AndAlso
                OIT0002INProw("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIT0002INProw("UPDTIMSTP") = OIT0002row("UPDTIMSTP")
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

        OIT0002row("UPDTIMSTP") = "0"
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
            If OIT0002INProw("LINKNO") = OIT0002row("LINKNO") AndAlso
               OIT0002INProw("LINKDETAILNO") = OIT0002row("LINKDETAILNO") Then
                '画面入力テーブル項目設定
                OIT0002INProw("LINECNT") = OIT0002row("LINECNT")
                OIT0002INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIT0002INProw("UPDTIMSTP") = OIT0002row("UPDTIMSTP")
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

                Case "SALESOFFICE"      '登録営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "USEPROPRIETY"     '利用可否フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "USEPROPRIETY"))

                Case "DEPSTATION"       '空車発駅　（着駅）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text + "2", "DEPSTATION"))

                Case "RETSTATION"       '空車着駅　（発駅）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text + "1", "RETSTATION"))

                Case "PRODUCTPATTERN"   '油種
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_OFFICECODE.Text, "PRODUCTPATTERN"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
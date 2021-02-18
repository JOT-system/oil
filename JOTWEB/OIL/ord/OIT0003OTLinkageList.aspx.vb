Option Strict On
Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' OT連携一覧画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0003OTLinkageList
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0003INPtbl As DataTable                              'チェック用テーブル
    Private OIT0003UPDtbl As DataTable                              '更新用テーブル
    Private OIT0003WKtbl As DataTable                               '作業用テーブル
    Private OIT0003CsvOTLinkagetbl As DataTable                     'CSV用(OT発送日報)テーブル
    Private OIT0003Takusoutbl As DataTable                     '帳票用(託送指示)テーブル
    Private OIT0003Reserved As DataTable

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
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
                    Master.RecoverTable(OIT0003tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonFilter"
                            WF_ButtonFilter_Click(False)
                        Case "WF_ButtonFilterClear"
                            WF_ButtonFilter_Click(True)
                        Case "WF_ButtonOtSend"          'OT連携ボタン押下
                            WF_ButtonOtSend_Click()
                        Case "WF_ButtonReserved"          '製油所出荷予約ボタン押下時
                            WF_ButtonReserved_Click()
                        Case "WF_ButtonTakusou"          '託送指示ボタン押下時
                            WF_ButtonTakusou_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"             'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            'WF_Grid_DBClick()
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
            If Not IsNothing(OIT0003tbl) Then
                OIT0003tbl.Clear()
                OIT0003tbl.Dispose()
                OIT0003tbl = Nothing
            End If

            If Not IsNothing(OIT0003INPtbl) Then
                OIT0003INPtbl.Clear()
                OIT0003INPtbl.Dispose()
                OIT0003INPtbl = Nothing
            End If

            If Not IsNothing(OIT0003UPDtbl) Then
                OIT0003UPDtbl.Clear()
                OIT0003UPDtbl.Dispose()
                OIT0003UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0003WRKINC.MAPIDOTL
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
        '○ ボタン制御
        Dim flp As New FileLinkagePattern
        '営業所設定取得
        Dim settings = flp(work.WF_SEL_OTS_SALESOFFICECODE.Text)
        '営業所に応じ表示非表示を行う
        'OT発送日報
        WF_ButtonOtSend.Visible = settings.CanOtSend
        '製油所出荷予約
        WF_ButtonReserved.Visible = settings.CanReserved
        '託送指示
        WF_ButtonTakusou.Visible = settings.CanTakusou
        '幅調整の為ボタンの数量で
        Dim cssVal = Me.Form.Attributes("class")
        Dim btnCnt As Integer = If(settings.CanOtSend, 1, 0) +
                                If(settings.CanReserved, 1, 0) +
                                If(settings.CanTakusou, 1, 0)
        cssVal = cssVal & " btnCnt" & btnCnt
        Me.Form.Attributes("class") = cssVal
        '表示するデータが無ければ各種ボタンは非活性
        If OIT0003tbl Is Nothing OrElse OIT0003tbl.Rows.Count = 0 Then
            WF_ButtonOtSend.Disabled = True
            WF_ButtonReserved.Disabled = True
            WF_ButtonTakusou.Disabled = True
        End If


    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        'フィルタ初期選択値取得（PROFVARIより)
        Dim dummyTxt As New TextBox
        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "FILTERDATEFLD", dummyTxt.Text)       '会社コード
        If dummyTxt.Text <> "" AndAlso Me.rblFilterDateFiled.Items.FindByValue(dummyTxt.Text) IsNot Nothing Then
            Me.rblFilterDateFiled.SelectedValue = dummyTxt.Text
        Else
            If Me.rblFilterDateFiled IsNot Nothing AndAlso Me.rblFilterDateFiled.Items.Count > 0 Then
                Me.rblFilterDateFiled.SelectedIndex = 0
            End If
        End If

        Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "FILTERDATE", WF_FILTERDATE_TEXT.Text)       '会社コード
        '○ 受注一覧画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003D Then
            Master.RecoverTable(OIT0003tbl, work.WF_SEL_INPOTLINKAGETBL.Text)
        End If

        '

        ''○ 名称設定処理
        'CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        'CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_SELUORG_TEXT.Text, WW_DUMMY)                     '運用部署

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '登録画面からの遷移の場合はテーブルから取得しない
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0001D Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If
        Dim chkField As String = ""
        If Me.rblFilterDateFiled IsNot Nothing AndAlso Me.rblFilterDateFiled.SelectedIndex <> -1 Then
            chkField = rblFilterDateFiled.SelectedValue
        End If
        SetFilterValue(OIT0003tbl, chkField, Me.WF_FILTERDATE_TEXT.Text)
        '○ 表示対象行カウント(絞り込み対象)
        Dim WW_DataCNT As Integer = 0
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            If CInt(OIT0003row("HIDDEN")) = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPOTLINKAGETBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl)

        'TBLview.RowFilter = "HIDDEN = 0 and LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= 1 and SELECT <= " & CONST_DISPROWCOUNT


        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.Both).ToString
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
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

        If IsNothing(OIT0003tbl) Then
            OIT0003tbl = New DataTable
        End If

        If OIT0003tbl.Columns.Count <> 0 Then
            OIT0003tbl.Columns.Clear()
        End If

        OIT0003tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する
        '★共通SQL
        Dim SQLStrCmn As String =
              " SELECT" _
            & "   0                                                      AS LINECNT" _
            & " , ''                                                     AS OPERATION" _
            & " , 0                                                      AS TIMSTP" _
            & " , 1                                                      AS 'SELECT'" _
            & " , 0                                                      AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')                  AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')                  AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')                     AS ORDERNO" _

        '★積置フラグ無し用SQL
        Dim SQLStrNashi As String =
              SQLStrCmn _
            & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), NULL)    AS LODDATE "

        '★積置フラグ有り用SQL
        Dim SQLStrAri As String =
              SQLStrCmn _
            & " , ISNULL(FORMAT(OIT0003.ACTUALLODDATE, 'yyyy/MM/dd'), NULL)    AS LODDATE "

        SQLStrCmn =
              " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), NULL)    AS DEPDATE " _
            & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), NULL)    AS ARRDATE " _
            & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), NULL)    AS ACCDATE " _
            & " , ISNULL(FORMAT(OIT0002.EMPARRDATE, 'yyyy/MM/dd'), NULL) AS EMPARRDATE " _
            & " , ISNULL(RTRIM(OIT0003.STACKINGFLG), '')                 AS STACKINGFLG" _
            & " , ISNULL(RTRIM(OIS0015.VALUE1), '')                      AS STACKINGNAME" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                     AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')                   AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')                  AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')              AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')                  AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')              AS ARRSTATIONNAME" _
            & "	, COUNT(1)                                               AS TOTALTANK "

        '油種(ハイオク)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS HTANK ", BaseDllConst.CONST_HTank)
        '油種(レギュラー)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS RTANK ", BaseDllConst.CONST_RTank)
        '油種(灯油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS TTANK ", BaseDllConst.CONST_TTank)
        '油種(未添加灯油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS MTTANK ", BaseDllConst.CONST_MTTank)
        '油種(軽油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS KTANK ", BaseDllConst.CONST_KTank1)
        '油種(３号軽油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K3TANK ", BaseDllConst.CONST_K3Tank1)
        '油種(５号軽油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K5TANK ", BaseDllConst.CONST_K5Tank)
        '油種(１０号軽油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS K10TANK ", BaseDllConst.CONST_K10Tank)
        '油種(ＬＳＡ)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS LTANK ", BaseDllConst.CONST_LTank1)
        '油種(Ａ重油)
        SQLStrCmn &= String.Format(" , SUM(CASE WHEN OIT0003.OILCODE ='{0}' Then 1 Else 0 End) AS ATANK ", BaseDllConst.CONST_ATank)

        '★積置フラグ無し用SQL
        SQLStrNashi &=
              SQLStrCmn _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & "  INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "      (OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "       OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & "  AND OIT0003.DELFLG <> @P04 " _
            & "  AND (OIT0003.STACKINGFLG <> '1' OR OIT0003.STACKINGFLG IS NULL) "

        '★積置フラグ有り用SQL
        SQLStrAri &=
              SQLStrCmn _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & "  INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "      (OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "       OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & "  AND OIT0003.DELFLG <> @P04 " _
            & "  AND OIT0003.STACKINGFLG = '1' " _
            & "  AND OIT0003.ACTUALLODDATE >= @P02 "

        SQLStrCmn =
              "  INNER JOIN com.OIS0015_FIXVALUE OIS0015 ON " _
            & "      OIS0015.CLASS   = 'STACKING' " _
            & "  AND OIS0015.KEYCODE = OIT0003.STACKINGFLG " _
            & "  INNER JOIN oil.VIW0003_OFFICECHANGE VIW0003 ON " _
            & "      VIW0003.ORGCODE    = @P05 " _
            & "  AND VIW0003.OFFICECODE = OIT0002.OFFICECODE " _
            & " WHERE OIT0002.DELFLG      <> @P04" _
            & "   AND OIT0002.ORDERSTATUS BETWEEN @P03 AND @P06 " _

        '★積置フラグ無し用SQL(積み置きがが無いパターンでしか発日を使用するパターンは存在しない）
        'SQLStrNashi &=
        '      SQLStrCmn _
        '    & "   AND (    OIT0002.LODDATE     >= @P02" _
        '    & "         OR OIT0002.DEPDATE     >= @TODAY) "
        SQLStrNashi &=
              SQLStrCmn _
            & "   AND (    OIT0002.LODDATE     >= @TODAY" _
            & "         OR OIT0002.DEPDATE     >= @TODAY) "
        '★積置フラグ有り用SQL
        SQLStrAri &=
              SQLStrCmn

        SQLStrCmn =
              " GROUP BY" _
            & "    OIT0002.OFFICECODE" _
            & "  , OIT0002.OFFICENAME" _
            & "  , OIT0002.ORDERNO" _
            & "  , OIT0003.STACKINGFLG" _
            & "  , OIS0015.VALUE1" _
            & "  , OIT0002.TRAINNO" _
            & "  , OIT0002.TRAINNAME" _
            & "  , OIT0002.DEPSTATION" _
            & "  , OIT0002.DEPSTATIONNAME" _
            & "  , OIT0002.ARRSTATION" _
            & "  , OIT0002.ARRSTATIONNAME" _
            & "  , OIT0002.DEPDATE" _
            & "  , OIT0002.ARRDATE" _
            & "  , OIT0002.ACCDATE" _
            & "  , OIT0002.EMPARRDATE"

        '★積置フラグ無し用SQL
        SQLStrNashi &=
              SQLStrCmn _
            & "  , OIT0002.LODDATE"

        '★積置フラグ有り用SQL
        SQLStrAri &=
              SQLStrCmn _
            & "  , OIT0003.ACTUALLODDATE" _
            & " ORDER BY" _
            & "    OFFICECODE" _
            & "  , TRAINNO" _
            & "  , LODDATE"

        '◯積置フラグ無し用SQLと積置フラグ有り用SQLを結合
        SQLStrNashi &=
              " UNION ALL" _
            & SQLStrAri

        Try
            Dim targetDate As String = Format(Now.AddDays(1), "yyyy/MM/dd")
            Dim today As String = Now.ToString("yyyy/MM/dd")
            Using SQLcmd As New SqlCommand(SQLStrNashi, SQLcon)
                'Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)         '積込日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 3)  '受注進行ステータスFROM
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 6)  '組織コード
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 3)  '受注進行ステータスTO
                Dim PARA_TODAY As SqlParameter = SQLcmd.Parameters.Add("@TODAY", SqlDbType.Date)         '当日
                'PARA01.Value = OFFICECDE
                PARA02.Value = targetDate
                PARA_TODAY.Value = today
                'PARA02.Value = "2020/08/20"
                PARA03.Value = BaseDllConst.CONST_ORDERSTATUS_200
                PARA06.Value = BaseDllConst.CONST_ORDERSTATUS_310
                PARA04.Value = C_DELETE_FLG.DELETE
                PARA05.Value = work.WF_SEL_OTS_SALESOFFICECODE.Text
                Dim dtWrk As DataTable = New DataTable
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        dtWrk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    dtWrk.Load(SQLdr)
                End Using
                '各ボタンで処理が可能か判定するフラグフィールド（３ボタン分追加）
                'OT発送日報出力可否
                dtWrk.Columns.Add("CAN_OTSEND", GetType(String)).DefaultValue = "0"
                '製油所出荷予約出力可否
                dtWrk.Columns.Add("CAN_RESERVED", GetType(String)).DefaultValue = "0"
                '託送指示出力可否
                dtWrk.Columns.Add("CAN_TAKUSOU", GetType(String)).DefaultValue = "0"
                If dtWrk.Rows.Count <> 0 Then
                    OIT0003tbl = (From dr As DataRow In dtWrk Order By dr("LODDATE")).CopyToDataTable
                Else
                    OIT0003tbl = dtWrk
                End If

                Dim i As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT
                    'OT発送日報出力可否(発日 >= 当日)
                    If Convert.ToString(OIT0003row("DEPDATE")) >= today Then
                        OIT0003row("CAN_OTSEND") = "1"
                    Else
                        OIT0003row("CAN_OTSEND") = "0"
                    End If
                    '出荷予約出力可否(積日 >= 翌日)
                    If Convert.ToString(OIT0003row("LODDATE")) >= today Then
                        OIT0003row("CAN_RESERVED") = "1"
                    Else
                        OIT0003row("CAN_RESERVED") = "0"
                    End If
                    '託送指示出力可否(発日 >= 翌日)
                    If Convert.ToString(OIT0003row("DEPDATE")) >= targetDate Then
                        OIT0003row("CAN_TAKUSOU") = "1"
                    Else
                        OIT0003row("CAN_TAKUSOU") = "0"
                    End If
                    ''受注進行ステータス
                    'CODENAME_get("ORDERSTATUS", OIT0003row("STATUS"), OIT0003row("STATUS"), WW_DUMMY)
                    ''受注情報
                    'CODENAME_get("ORDERINFO", OIT0003row("INFO"), OIT0003row("INFO"), WW_DUMMY)
                    ''担当営業所
                    'CODENAME_get("SALESOFFICE", OIT0003row("OFFICECODE"), OIT0003row("OFFICENAME"), WW_DUMMY)
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If Convert.ToString(OIT0003tbl.Rows(i)("LINECNT")) = WF_SelectedIndex.Value Then
                If Convert.ToString(OIT0003tbl.Rows(i)("OPERATION")) = "" Then
                    OIT0003tbl.Rows(i)("OPERATION") = "on"
                Else
                    OIT0003tbl.Rows(i)("OPERATION") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If Convert.ToString(OIT0003tbl.Rows(i)("HIDDEN")) = "0" Then
                OIT0003tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0003tbl.Rows.Count - 1
            If Convert.ToString(OIT0003tbl.Rows(i)("HIDDEN")) = "0" Then
                OIT0003tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub
    ''' <summary>
    ''' フィルタ処理実行
    ''' </summary>
    ''' <param name="isClear"></param>
    Protected Sub WF_ButtonFilter_Click(isClear As Boolean)
        Dim chkField As String = ""
        If Me.rblFilterDateFiled IsNot Nothing AndAlso Me.rblFilterDateFiled.SelectedIndex <> -1 Then
            chkField = rblFilterDateFiled.SelectedValue
        End If
        Dim dataVal As String = ""
        If isClear = False Then
            dataVal = Me.WF_FILTERDATE_TEXT.Text
        End If
        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)
        '表示行制御実行
        OIT0003tbl = SetFilterValue(OIT0003tbl, chkField, dataVal)
        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)
    End Sub
    ''' <summary>
    ''' OT連携ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOtSend_Click()
        Dim selectedOrderInfo As New List(Of OutputOrdedrInfo)
        '一覧のチェックボックスが選択されているか確認
        If OIT0003tbl.Select("OPERATION = 'on'").Count = 0 Then
            '選択されていない場合は、エラーメッセージを表示し終了
            Master.Output(C_MESSAGE_NO.OIL_OTLINKAGELINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If
        '処理対象外のチェックがなされている場合
        Dim qCannotProc = From dr As DataRow In OIT0003tbl Where dr("OPERATION").Equals("on") _
                                                         AndAlso dr("CAN_OTSEND").Equals("0")

        If qCannotProc.Any Then
            '選択されていない場合は、エラーメッセージを表示し終了
            Master.Output(C_MESSAGE_NO.OIL_OTLINKAGELINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If
        '日付またがりチェック(出力帳票のレイアウト上、同じ発日以外許可しない）
        '対象の積日が統一されていない場合（同一積日以外は不許可）
        Dim qSameProcDateCnt = (From dr As DataRow In OIT0003tbl Where dr("OPERATION").Equals("on") Group By g = Convert.ToString(dr("LODDATE")) Into Group Select g).Count
        If qSameProcDateCnt > 1 Then
            '選択されていない場合は、エラーメッセージを表示し終了
            Master.Output(C_MESSAGE_NO.OIL_OTLINKAGELINE_NOT_ACCEPT_SEL_DAYS, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If
        '******************************
        'OT発送日報データ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            SqlConnection.ClearPool(SQLcon)
            selectedOrderInfo = OTLinkageDataGet(SQLcon)
            If selectedOrderInfo Is Nothing Then
                Return
            End If
        End Using

        '******************************
        'CSV作成処理の実行
        '******************************
        Dim OTFileName As String = SetCSVFileName()
        Using repCbj = New CsvCreate(OIT0003CsvOTLinkagetbl, I_FolderPath:=CS0050SESSION.OTFILESEND_PATH, I_FileName:=OTFileName, I_Enc:="UTF8N")
            Dim url As String
            Try
                url = repCbj.ConvertDataTableToCsv(False)
            Catch ex As Exception
                Return
            End Try
            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
        End Using
        '******************************
        'OT発送日報データの（本体）ダウンロードフラグ更新
        '                  （明細）ダウンロード数インクリメント
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            SqlConnection.ClearPool(SQLcon)
            Dim procDate As Date = Now
            Dim resProc As Boolean = False
            Dim orderDlFlags As Dictionary(Of String, String) = Nothing
            Using sqlTran As SqlTransaction = SQLcon.BeginTransaction
                'オーダー明細のダウンロードカウントのインクリメント
                resProc = IncrementDetailOutputCount(selectedOrderInfo, WF_ButtonClick.Value, SQLcon, sqlTran, procDate)
                If resProc = False Then
                    Return
                End If
                'オーダー明細よりダウンロードフラグを取得
                orderDlFlags = GetOutputFlag(selectedOrderInfo, WF_ButtonClick.Value, SQLcon, sqlTran)
                If orderDlFlags Is Nothing Then
                    Return
                End If
                'オーダーを更新
                resProc = UpdateOrderOutputFlag(orderDlFlags, WF_ButtonClick.Value, SQLcon, sqlTran, procDate)
                If resProc = False Then
                    Return
                End If
                '履歴登録用直近データ取得
                '直近履歴番号取得
                Dim historyNo As String = GetNewOrderHistoryNo(SQLcon, sqlTran)
                If historyNo = "" Then
                    Return
                End If
                Dim orderTbl As DataTable = GetUpdatedOrder(selectedOrderInfo, SQLcon, sqlTran)
                Dim detailTbl As DataTable = GetUpdatedOrderDetail(selectedOrderInfo, SQLcon, sqlTran)
                If orderTbl IsNot Nothing AndAlso detailTbl IsNot Nothing Then
                    Dim hisOrderTbl As DataTable = ModifiedHistoryDatatable(orderTbl, historyNo)
                    Dim hisDetailTbl As DataTable = ModifiedHistoryDatatable(detailTbl, historyNo)

                    '履歴テーブル登録
                    For Each dr As DataRow In hisOrderTbl.Rows
                        EntryHistory.InsertOrderHistory(SQLcon, sqlTran, dr)
                    Next
                    For Each dr As DataRow In hisDetailTbl.Rows
                        EntryHistory.InsertOrderDetailHistory(SQLcon, sqlTran, dr)
                    Next
                    'ジャーナル登録
                    OutputJournal(orderTbl, "OIT0002_ORDER")
                    OutputJournal(detailTbl, "OIT0003_DETAIL")
                End If

                'ここまで来たらコミット
                sqlTran.Commit()
            End Using

        End Using


        ''○ 遷移先(OT連携一覧画面)退避データ保存先の作成
        'WW_CreateXMLSaveFile()

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0003tbl, work.WF_SEL_INPOTLINKAGETBL.Text)

    End Sub
    ''' <summary>
    ''' 製油所出荷予約ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonReserved_Click()
        Try
            Dim selectedOrderInfo As New List(Of OutputOrdedrInfo)
            '一覧のチェックボックスが選択されているか確認
            If OIT0003tbl.Select("OPERATION = 'on'").Count = 0 Then
                '選択されていない場合は、エラーメッセージを表示し終了
                Master.Output(C_MESSAGE_NO.OIL_RESERVED_PRINT_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
            '処理対象外のチェックがなされている場合
            Dim qCannotProc = From dr As DataRow In OIT0003tbl Where dr("OPERATION").Equals("on") _
                                                         AndAlso dr("CAN_RESERVED").Equals("0")

            If qCannotProc.Any Then
                '選択されていない場合は、エラーメッセージを表示し終了
                Master.Output(C_MESSAGE_NO.OIL_RESERVED_PRINT_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
            '日付またがりチェック(出力帳票のレイアウト上、同じ発日以外許可しない）
            '対象の積日が統一されていない場合（同一積日以外は不許可）
            Dim qSameProcDateCnt = (From dr As DataRow In OIT0003tbl Where dr("OPERATION").Equals("on") Group By g = Convert.ToString(dr("LODDATE")) Into Group Select g).Count
            If qSameProcDateCnt > 1 Then
                '選択されていない場合は、エラーメッセージを表示し終了
                Master.Output(C_MESSAGE_NO.OIL_RESERVED_NOT_ACCEPT_SEL_DAYS, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
            '******************************
            '出荷予約データ取得処理
            '******************************
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                selectedOrderInfo = ReservedDataGet(SQLcon)
                If selectedOrderInfo Is Nothing Then
                    Return
                End If
            End Using

            '******************************
            '出力ファイル作成処理の実行
            '******************************
            '出力設定取得
            Dim flp As New FileLinkagePattern
            '営業所設定取得
            Dim settings = flp(work.WF_SEL_OTS_SALESOFFICECODE.Text)
            If settings.OutputFiledList Is Nothing OrElse settings.OutputFiledList.Count = 0 Then
                Return
            End If
            'Excel出力かCSV出力かに応じ処理分岐
            If {FileLinkagePatternItem.ReserveOutputFileType.Csv, FileLinkagePatternItem.ReserveOutputFileType.Seq}.Contains(settings.ReservedOutputType) Then
                'CSV出力
                Using repCbj = New OIT0003CustomReportReservedCsv(OIT0003Reserved, settings, settings.OutputReservedFileNameWithoutExtention, settings.OutputReservedFileExtention)
                    Dim url As String
                    Dim url2 As String = ""
                    Try
                        If FileLinkagePatternItem.ReserveOutputFileType.Csv = settings.ReservedOutputType Then
                            url = repCbj.ConvertDataTableToCsv(False)
                        Else
                            url = repCbj.CreateSequence()
                            url2 = repCbj.CreateSequenceRequest()
                        End If

                        If url = "" Then
                            Return
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL DLReserved")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL DLReserved"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = ex.ToString()
                        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWrite.CS0011LOGWrite()
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    If url2 <> "" Then
                        Dim url2Obj As New HiddenField
                        url2Obj.EnableViewState = False
                        url2Obj.ID = "WF_PrintURL2"
                        url2Obj.Value = url2
                        Me.Form.Controls.Add(url2Obj)
                    End If
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
            Else
                'Excel出力（現状袖ヶ浦のみの想定）※正しく設定クラスを作れば動作可能
                'CSV出力
                Using repCbj = New OIT0003CustomReportReservedExcel(OIT0003Reserved, settings, settings.OutputReservedFileNameWithoutExtention, settings.OutputReservedFileExtention)
                    Dim url As String
                    Try
                        url = repCbj.CreatePrintData()
                        If url = "" Then
                            Return
                        End If
                    Catch ex As Exception
                        Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL DLTakusou")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL DLReserved"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = ex.ToString()
                        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWrite.CS0011LOGWrite()
                        Return
                    End Try
                    '○ 別画面でExcelを表示
                    WF_PrintURL.Value = url
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
                End Using
            End If

            '******************************
            '出荷予約データの（本体）ダウンロードフラグ更新
            '                  （明細）ダウンロード数インクリメント
            '******************************
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                Dim procDate As Date = Now
                Dim resProc As Boolean = False
                Dim orderDlFlags As Dictionary(Of String, String) = Nothing
                Using sqlTran As SqlTransaction = SQLcon.BeginTransaction
                    'オーダー明細のダウンロードカウントのインクリメント
                    resProc = IncrementDetailOutputCount(selectedOrderInfo, WF_ButtonClick.Value, SQLcon, sqlTran, procDate, True)
                    If resProc = False Then
                        Return
                    End If
                    'オーダー明細よりダウンロードフラグを取得
                    orderDlFlags = GetOutputFlag(selectedOrderInfo, WF_ButtonClick.Value, SQLcon, sqlTran)
                    If orderDlFlags Is Nothing Then
                        Return
                    End If
                    'オーダーを更新
                    resProc = UpdateOrderOutputFlag(orderDlFlags, WF_ButtonClick.Value, SQLcon, sqlTran, procDate)
                    If resProc = False Then
                        Return
                    End If
                    '履歴登録用直近データ取得
                    '直近履歴番号取得
                    Dim historyNo As String = GetNewOrderHistoryNo(SQLcon, sqlTran)
                    If historyNo = "" Then
                        Return
                    End If
                    Dim orderTbl As DataTable = GetUpdatedOrder(selectedOrderInfo, SQLcon, sqlTran)
                    Dim detailTbl As DataTable = GetUpdatedOrderDetail(selectedOrderInfo, SQLcon, sqlTran)
                    If orderTbl IsNot Nothing AndAlso detailTbl IsNot Nothing Then
                        Dim hisOrderTbl As DataTable = ModifiedHistoryDatatable(orderTbl, historyNo)
                        Dim hisDetailTbl As DataTable = ModifiedHistoryDatatable(detailTbl, historyNo)

                        '履歴テーブル登録
                        For Each dr As DataRow In hisOrderTbl.Rows
                            EntryHistory.InsertOrderHistory(SQLcon, sqlTran, dr)
                        Next
                        For Each dr As DataRow In hisDetailTbl.Rows
                            EntryHistory.InsertOrderDetailHistory(SQLcon, sqlTran, dr)
                        Next
                        'ジャーナル登録
                        OutputJournal(orderTbl, "OIT0002_ORDER")
                        OutputJournal(detailTbl, "OIT0003_DETAIL")
                    End If

                    'ここまで来たらコミット
                    sqlTran.Commit()
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL DLReserved")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL DLReserved"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try

    End Sub
    ''' <summary>
    ''' 託送指示ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonTakusou_Click()
        Try
            '一旦三重塩浜、四日市ではない場合、素通り
            Dim targetOffice As String = work.WF_SEL_OTS_SALESOFFICECODE.Text
            If Not {"012401", "012402"}.Contains(targetOffice) Then
                Return
            End If
            '
            '一覧のチェックボックスが選択されているか確認
            If OIT0003tbl.Select("OPERATION = 'on'").Count = 0 Then
                '選択されていない場合は、エラーメッセージを表示し終了
                Master.Output(C_MESSAGE_NO.OIL_TAKUSOU_PRINT_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
            '処理対象外のチェックがなされている場合(ここは本来全て可能な想定だが念のため)
            Dim qCannotProc = From dr As DataRow In OIT0003tbl Where dr("OPERATION").Equals("on") _
                                                             AndAlso dr("CAN_TAKUSOU").Equals("0")

            If qCannotProc.Any Then
                '選択されていない場合は、エラーメッセージを表示し終了
                Master.Output(C_MESSAGE_NO.OIL_TAKUSOU_PRINT_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
            '日付またがりチェック(出力帳票のレイアウト上、同じ発日以外許可しない）
            '対象の発日が統一されていない場合（同一発日以外は不許可）
            Dim qSameProcDateCnt = (From dr As DataRow In OIT0003tbl Where dr("OPERATION").Equals("on") Group By g = Convert.ToString(dr("DEPDATE")) Into Group Select g).Count
            If qSameProcDateCnt > 1 Then
                '選択されていない場合は、エラーメッセージを表示し終了
                Master.Output(C_MESSAGE_NO.OIL_TAKUSOU_NOT_ACCEPT_SEL_DAYS, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If

            '処理対象のデータ明細を取得
            Dim selectedOrderInfo As New List(Of OutputOrdedrInfo)
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                selectedOrderInfo = TakusouDataGet(SQLcon)
                If selectedOrderInfo Is Nothing OrElse selectedOrderInfo.Count = 0 Then
                    Return
                End If
            End Using
            If selectedOrderInfo.Count = 0 Then
                Return '出力対象無し
            End If
            '******************************
            ' 出力データ生成
            '******************************
            Using repCbj = New OIT0003CustomReportTakusouExcel(work.WF_SEL_OTS_SALESOFFICECODE.Text, OIT0003Takusoutbl)
                repCbj.FileType = OIT0003CustomReportTakusouExcel.OutputFileType.Excel 'デバッグ用Excel出力に変更
                Dim url As String
                Try
                    url = repCbj.CreatePrintData()
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL DLTakusou")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL DLTakusou"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()
                    Return
                End Try
                '○ 別画面でExcelを表示
                WF_PrintURL.Value = url
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            End Using
            '******************************
            '託送指示データの（本体）ダウンロードフラグ更新
            '                  （明細）ダウンロード数インクリメント
            '******************************
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                Dim procDate As Date = Now
                Dim resProc As Boolean = False
                Dim orderDlFlags As Dictionary(Of String, String) = Nothing
                Using sqlTran As SqlTransaction = SQLcon.BeginTransaction
                    'オーダー明細のダウンロードカウントのインクリメント
                    resProc = IncrementDetailOutputCount(selectedOrderInfo, WF_ButtonClick.Value, SQLcon, sqlTran, procDate)
                    If resProc = False Then
                        Return
                    End If
                    'オーダー明細よりダウンロードフラグを取得
                    orderDlFlags = GetOutputFlag(selectedOrderInfo, WF_ButtonClick.Value, SQLcon, sqlTran)
                    If orderDlFlags Is Nothing Then
                        Return
                    End If
                    'オーダーを更新
                    resProc = UpdateOrderOutputFlag(orderDlFlags, WF_ButtonClick.Value, SQLcon, sqlTran, procDate)
                    If resProc = False Then
                        Return
                    End If
                    '履歴登録用直近データ取得
                    '直近履歴番号取得
                    Dim historyNo As String = GetNewOrderHistoryNo(SQLcon, sqlTran)
                    If historyNo = "" Then
                        Return
                    End If
                    Dim orderTbl As DataTable = GetUpdatedOrder(selectedOrderInfo, SQLcon, sqlTran)
                    Dim detailTbl As DataTable = GetUpdatedOrderDetail(selectedOrderInfo, SQLcon, sqlTran)
                    If orderTbl IsNot Nothing AndAlso detailTbl IsNot Nothing Then
                        Dim hisOrderTbl As DataTable = ModifiedHistoryDatatable(orderTbl, historyNo)
                        Dim hisDetailTbl As DataTable = ModifiedHistoryDatatable(detailTbl, historyNo)

                        '履歴テーブル登録
                        For Each dr As DataRow In hisOrderTbl.Rows
                            EntryHistory.InsertOrderHistory(SQLcon, sqlTran, dr)
                        Next
                        For Each dr As DataRow In hisDetailTbl.Rows
                            EntryHistory.InsertOrderDetailHistory(SQLcon, sqlTran, dr)
                        Next
                        'ジャーナル登録
                        OutputJournal(orderTbl, "OIT0002_ORDER")
                        OutputJournal(detailTbl, "OIT0003_DETAIL")
                    End If

                    'ここまで来たらコミット
                    sqlTran.Commit()
                End Using

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL DLTakusou")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL DLTakusou"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()
        End Try

    End Sub
    ''' <summary>
    ''' 受注履歴テーブル用の履歴番号取得
    ''' </summary>
    ''' <returns>履歴番号</returns>
    Private Function GetNewOrderHistoryNo(ByVal sqlCon As SqlConnection, sqlTran As SqlTransaction) As String
        Dim retVal As String = ""
        Try
            Dim sqlStr As New StringBuilder
            sqlStr.AppendLine("SELECT FX.KEYCODE  AS HISTORYNO")
            sqlStr.AppendLine("  FROM OIL.VIW0001_FIXVALUE FX")
            sqlStr.AppendLine(" WHERE FX.CLASS    = @CLASS")
            sqlStr.AppendLine("   AND FX.DELFLG   = @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStr.ToString, sqlCon, sqlTran)
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar).Value = "NEWHISTORYNOGET"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                End With

                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows Then
                        sqlDr.Read()
                        retVal = Convert.ToString(sqlDr("HISTORYNO"))
                    Else
                        Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL ORDER_HISTORYNOGET")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL ORDER_HISTORYNOGET"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "履歴番号の取得に失敗"
                        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                        retVal = ""
                    End If
                End Using 'sqlDr
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL ORDER_HISTORYNOGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL ORDER_HISTORYNOGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
        Return retVal
    End Function
    ''' <summary>
    ''' OT発送日報データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Function OTLinkageDataGet(ByVal SQLcon As SqlConnection) As List(Of OutputOrdedrInfo)
        Dim retVal As New List(Of OutputOrdedrInfo)
        If IsNothing(OIT0003CsvOTLinkagetbl) Then
            OIT0003CsvOTLinkagetbl = New DataTable
        End If

        If OIT0003CsvOTLinkagetbl.Columns.Count <> 0 Then
            OIT0003CsvOTLinkagetbl.Columns.Clear()
        End If

        OIT0003CsvOTLinkagetbl.Clear()

        '桁数
        Dim iOURDAILYBRANCHC As Integer = 2
        Dim iOTDAILYCONSIGNEEC As Integer = 2
        Dim iOTDAILYDEPSTATIONN As Integer = 8
        Dim iOTDAILYSHIPPERN As Integer = 8
        Dim iOTOILNAME As Integer = 12
        Dim iTANKNO As Integer = 6

        '○ 取得SQL
        '　 説明　：　帳票表示用SQL
        '★積置フラグ無し用SQL
        Dim SQLStrNashi As String =
              " SELECT " _
            & "   ISNULL(CONVERT(NCHAR(2), OIM0025.OURDAILYBRANCHC), SPACE (2))     AS OURDAILYBRANCHC" _
            & " , ISNULL(CONVERT(NCHAR(2), OIM0025.OTDAILYCONSIGNEEC), SPACE (2))   AS OTDAILYCONSIGNEEC" _
            & " , FORMAT(OIT0002.LODDATE, 'yyyyMMdd')            AS LODDATE"
        '  " SELECT " _
        '& "   CONVERT(VARCHAR (2), ISNULL(OIM0025.OURDAILYBRANCHC,''))" _
        '& "   +  REPLICATE(SPACE (1), 2 - DATALENGTH(CONVERT(VARCHAR (2), ISNULL(OIM0025.OURDAILYBRANCHC,''))))   AS OURDAILYBRANCHC" _
        '& " , CONVERT(VARCHAR (2), ISNULL(OIM0025.OTDAILYCONSIGNEEC,''))" _
        '& "   +  REPLICATE(SPACE (1), 2 - DATALENGTH(CONVERT(VARCHAR (2), ISNULL(OIM0025.OTDAILYCONSIGNEEC,'')))) AS OTDAILYCONSIGNEEC" _
        '& " , FORMAT(OIT0002.LODDATE, 'yyyyMMdd')            AS LODDATE"

        '★積置フラグ有り用SQL
        Dim SQLStrAri As String =
              " SELECT " _
            & "   ISNULL(CONVERT(NCHAR(2), OIM0025.OURDAILYBRANCHC), SPACE (2))     AS OURDAILYBRANCHC" _
            & " , ISNULL(CONVERT(NCHAR(2), OIM0025.OTDAILYCONSIGNEEC), SPACE (2))   AS OTDAILYCONSIGNEEC" _
            & " , FORMAT(OIT0003.ACTUALLODDATE, 'yyyyMMdd')      AS LODDATE"
        '  " SELECT " _
        '& "   CONVERT(VARCHAR (2), ISNULL(OIM0025.OURDAILYBRANCHC,''))" _
        '& "   +  REPLICATE(SPACE (1), 2 - DATALENGTH(CONVERT(VARCHAR (2), ISNULL(OIM0025.OURDAILYBRANCHC,''))))   AS OURDAILYBRANCHC" _
        '& " , CONVERT(VARCHAR (2), ISNULL(OIM0025.OTDAILYCONSIGNEEC,''))" _
        '& "   +  REPLICATE(SPACE (1), 2 - DATALENGTH(CONVERT(VARCHAR (2), ISNULL(OIM0025.OTDAILYCONSIGNEEC,'')))) AS OTDAILYCONSIGNEEC" _
        '& " , FORMAT(OIT0003.ACTUALLODDATE, 'yyyyMMdd')      AS LODDATE"

        '★共通SQL
        Dim SQLStrCmn As String =
              " , OIT0003.ORDERNO                                AS ORDERNO" _
            & " , OIT0003.DETAILNO                               AS DETAILNO" _
            & " , FORMAT(CONVERT(INT,OIT0002.TRAINNO), '0000')   AS TRAINNO" _
            & " , CONVERT(NCHAR(1), '')                          AS TRAINTYPE" _
            & " , CONVERT(NCHAR(2), OIT0002.TOTALTANKCH)         AS TOTALTANK" _
            & " , CONVERT(NCHAR(2), ISNULL(OIT0003.SHIPORDER,'')) AS SHIPORDER" _
            & " , ISNULL(OIM0025.OTDAILYFROMPLANT, SPACE (2))    AS OTDAILYFROMPLANT" _
            & " , CONVERT(NCHAR(1), '')                          AS LANDC" _
            & " , CONVERT(NCHAR(1), '')                          AS EMPTYFAREFLG" _
            & " , CONVERT(VARCHAR (8), ISNULL(OIM0025.OTDAILYDEPSTATIONN,''))" _
            & "   +  REPLICATE(SPACE (1), 8 - DATALENGTH(CONVERT(VARCHAR (8), ISNULL(OIM0025.OTDAILYDEPSTATIONN,'')))) AS OTDAILYDEPSTATIONN" _
            & " , ISNULL(CONVERT(NCHAR(2), OIM0025.OTDAILYSHIPPERC), SPACE (2))     AS OTDAILYSHIPPERC" _
            & " , CONVERT(VARCHAR (8), ISNULL(OIM0025.OTDAILYSHIPPERN,''))" _
            & "   +  REPLICATE(SPACE (1), 8 - DATALENGTH(CONVERT(VARCHAR (8), ISNULL(OIM0025.OTDAILYSHIPPERN,''))))    AS OTDAILYSHIPPERN" _
            & " , CONVERT(CHAR (4), OIM0003.OTOILCODE)           AS OTOILCODE" _
            & " , CONVERT(VARCHAR (12), ISNULL(OIM0003.OTOILNAME,''))" _
            & "   +  REPLICATE(SPACE (1), 12 - DATALENGTH(CONVERT(VARCHAR (12), ISNULL(OIM0003.OTOILNAME,''))))        AS OTOILNAME" _
            & " , CASE" _
            & "   WHEN OIM0005.MODELTANKNO IS NULL THEN SPACE(1)" _
            & "   ELSE CONVERT(VARCHAR (6), OIM0005.MODELTANKNO)" _
            & "   END" _
            & "   +  REPLICATE(SPACE (1), 6 - DATALENGTH(CONVERT(VARCHAR (6), ISNULL(OIM0005.MODELTANKNO,''))))        AS TANKNO" _
            & " , CONVERT(NCHAR(1), '0')                         AS OUTSIDEINFO" _
            & " , CONVERT(NCHAR(1), '')                          AS GENERALCARTYPE" _
            & " , CONVERT(NCHAR(1), '0')                         AS RUNINFO" _
            & " , REPLACE(CONVERT(NCHAR(5), CONVERT(INT, OIT0003.CARSAMOUNT)), SPACE(1), '0') AS CARSAMOUNT" _
            & " , CONVERT(NCHAR(4), '')                          AS REMARK" _
            & " FROM OIL.OIT0002_ORDER OIT0002 "
        '& " , REPLACE(CONVERT(NCHAR(4), ''), SPACE(1), '0')  AS TRAINNO" _
        '& " , ISNULL(CONVERT(NCHAR(8), OIM0025.OTDAILYDEPSTATIONN), SPACE (8))  AS OTDAILYDEPSTATIONN" _
        '& " , ISNULL(CONVERT(NCHAR(2), OIM0025.OTDAILYSHIPPERC), SPACE (2))     AS OTDAILYSHIPPERC" _
        '& " , ISNULL(CONVERT(NCHAR(8), OIM0025.OTDAILYSHIPPERN), SPACE (8))     AS OTDAILYSHIPPERN" _
        '& " , CONVERT(NCHAR(12), OIM0003.OTOILNAME)          AS OTOILNAME" _
        '& " , ISNULL(CONVERT(NCHAR(6), OIM0005.MODELTANKNO), SPACE (6))         AS TANKNO" _

        '★積置フラグ無し用SQL
        SQLStrNashi &=
              SQLStrCmn _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     (OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "      OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " AND (OIT0003.STACKINGFLG <> '1' OR OIT0003.STACKINGFLG IS NULL) "

        '★積置フラグ有り用SQL
        SQLStrAri &=
              SQLStrCmn _
            & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
            & "     OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " AND OIT0003.STACKINGFLG = '1' " _
            & " AND FORMAT(OIT0003.ACTUALLODDATE,'yyyy/MM') = @P05 "
        'SQLStrAri &=
        '      SQLStrCmn _
        '    & " INNER JOIN OIL.OIT0003_DETAIL OIT0003 ON " _
        '    & "     (OIT0003.ORDERNO = OIT0002.ORDERNO " _
        '    & "      OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
        '    & " AND OIT0003.DELFLG <> @P02 " _
        '    & " AND OIT0003.STACKINGFLG = '1' " _
        '    & " AND OIT0003.ACTUALLODDATE >= @P03 "

        '★共通SQL
        SQLStrCmn =
              " INNER JOIN OIL.OIM0003_PRODUCT OIM0003 ON " _
            & "     OIM0003.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0003.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & " AND OIM0003.PLANTCODE = OIT0002.BASECODE " _
            & " AND OIM0003.OILCODE = OIT0003.OILCODE " _
            & " AND OIM0003.SEGMENTOILCODE = OIT0003.ORDERINGTYPE " _
            & " AND OIM0003.DELFLG <> @P02 " _
            & " INNER JOIN OIL.OIM0010_PATTERN OIM0010 ON " _
            & "     OIM0010.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0010.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & " AND OIM0010.PLANTCODE = OIT0002.BASECODE " _
            & " AND OIM0010.CONSIGNEECODE = OIT0002.CONSIGNEECODE " _
            & " AND OIM0010.BRANCH = '1' " _
            & " AND OIM0010.KBN = 'O' " _
            & " AND OIM0010.DEFAULTKBN = 'def' " _
            & " AND OIM0010.DELFLG <> @P02 "

        SQLStrCmn &=
              " LEFT JOIN (SELECT  " _
            & "              OIM0005.TANKNUMBER " _
            & "            , CASE  " _
            & "              WHEN OIM0005.MODEL = 'タキ1000' THEN 100000 + CONVERT(INT, OIM0005.TANKNUMBER) " _
            & "              ELSE OIM0005.TANKNUMBER " _
            & "              END AS MODELTANKNO " _
            & "            , CONVERT(INT, OIM0005.LOAD) AS LOAD " _
            & "            , OIM0005.DELFLG " _
            & "            FROM oil.OIM0005_TANK OIM0005) OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIM0005.DELFLG <> @P02 "
        '& "            , CASE  " _
        '& "              WHEN CONVERT(VARCHAR, OIM0005.LOAD) <> '44.0' THEN '' " _
        '& "              ELSE CONVERT(VARCHAR, CONVERT(INT, OIM0005.LOAD)) " _
        '& "              END AS LOAD " _

        SQLStrCmn &=
              " LEFT JOIN OIL.OIM0025_OTLINKAGE OIM0025 ON " _
            & "     OIM0025.OFFICECODE = OIT0002.OFFICECODE " _
            & " AND OIM0025.SHIPPERCODE = OIT0002.SHIPPERSCODE " _
            & " AND OIM0025.PLANTCODE = OIT0002.BASECODE " _
            & " AND OIM0025.CONSIGNEECODE = OIT0002.CONSIGNEECODE " _
            & " AND OIM0025.OURDAILYMARKTUN = OIM0005.LOAD " _
            & " AND OIM0025.TRKBN = OIM0010.TRKBN " _
            & " AND OIM0025.OTTRANSPORTFLG = ISNULL(OIT0003.OTTRANSPORTFLG,'2') " _
            & " AND OIM0025.DELFLG <> @P02 " _
            & " WHERE OIT0002.DELFLG <> @P02 " _
            & "   AND OIT0002.ORDERSTATUS <= @P04 " _
            & "   AND OIT0002.ORDERNO IN ( "

        '一覧で指定された受注№を条件に設定
        Dim j As Integer = 0
        Dim checkedRow As DataTable = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "").CopyToDataTable
        For Each OIT0003row As DataRow In checkedRow.Rows

            If j = 0 Then
                SQLStrCmn &= "'" & Convert.ToString(OIT0003row("ORDERNO")) & "' "
            Else
                SQLStrCmn &= ", '" & Convert.ToString(OIT0003row("ORDERNO")) & "' "
            End If
            j += 1
        Next
        SQLStrCmn &= ")"

        '★積置フラグ無し用SQL
        SQLStrNashi &=
              SQLStrCmn _
            & "   AND (    OIT0002.LODDATE     >= @TODAY" _
            & "         OR OIT0002.DEPDATE     >= @TODAY) " _
            & "   AND FORMAT(OIT0002.LODDATE,'yyyy/MM') = @P05" _
        '★積置フラグ有り用SQL
        SQLStrAri &=
              SQLStrCmn _
            & " ORDER BY" _
            & "    OURDAILYBRANCHC" _
            & "  , SHIPORDER" _
            & "  , OTOILCODE"
        '& " ORDER BY" _
        '& "    OIM0025.OURDAILYBRANCHC" _
        '& "  , OIM0025.OURDAILYPLANTC" _
        '& "  , OIT0003.SHIPORDER" _
        '& "  , OIM0003.OTOILCODE"

        '◯積置フラグ無し用SQLと積置フラグ有り用SQLを結合
        SQLStrNashi &=
              " UNION ALL" _
            & SQLStrAri


        Try

            Using SQLcmd As New SqlCommand(SQLStrNashi, SQLcon)
                'Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 11) '受注No
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.Date)         '積込日
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar)     '積込日(年月)
                Dim PARATODAY As SqlParameter = SQLcmd.Parameters.Add("@TODAY", SqlDbType.Date)         '積込日
                'PARA01.Value = ""
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                'PARA03.Value = "2020/08/20"
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_310
                PARA05.Value = Format(Now.AddDays(1), "yyyy/MM")
                PARATODAY.Value = Format(Now, "yyyy/MM/dd")
                '★桁数設定
                Dim VALUE01 As SqlParameter = SQLcmd.Parameters.Add("@V01", SqlDbType.Int) '支店Ｃ(当社日報)
                VALUE01.Value = iOURDAILYBRANCHC
                Dim wrkDt As DataTable = New DataTable
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        wrkDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        If SQLdr.GetName(index) <> "ORDERNO" Then
                            OIT0003CsvOTLinkagetbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        End If
                    Next

                    '○ テーブル検索結果をテーブル格納
                    wrkDt.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                Dim sortedDt = From dr As DataRow In wrkDt Order By dr("LODDATE")
                For Each sortedDr As DataRow In sortedDt 'OIT0003CsvOTLinkagetbl.Rows
                    Dim qHasSelectedRow = From chkDr In checkedRow Where sortedDr("ORDERNO").Equals(chkDr("ORDERNO")) 'AndAlso
                    'Convert.ToString(sortedDr("LODDATE")) = Convert.ToString(chkDr("LODDATE")).Replace("/", "")
                    If qHasSelectedRow.Any Then
                        Dim newDr As DataRow = OIT0003CsvOTLinkagetbl.NewRow
                        For Each col As DataColumn In wrkDt.Columns
                            If Not {"ORDERNO", "DETAILNO"}.Contains(col.ColumnName) Then
                                newDr(col.ColumnName) = sortedDr(col.ColumnName)
                            End If
                        Next

                        OIT0003CsvOTLinkagetbl.Rows.Add(newDr)
                        retVal.Add(New OutputOrdedrInfo(Convert.ToString(sortedDr("ORDERNO")), Convert.ToString(sortedDr("DETAILNO"))))
                    End If
                    'i += 1
                    'OIT0003Csvrow("LINECNT") = i        'LINECNT

                Next

                '★積込日を[yyyymmdd]⇒[yymmdd]に変換
                For Each OIT0003row As DataRow In OIT0003CsvOTLinkagetbl.Rows
                    OIT0003row("LODDATE") = OIT0003row("LODDATE").ToString().Substring(OIT0003row("LODDATE").ToString().Length - 6)
                Next

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL CSV_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL CSV_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003CsvOTLinkagetbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
        Return retVal
    End Function
    ''' <summary>
    ''' 託送指示データを取得
    ''' </summary>
    ''' <param name="SQLcon">SQL接続文字</param>
    ''' <returns>処理対象の受注Noと明細No</returns>
    ''' <remarks>このロジックにたどりつけるのは積置無しのみ、積置を許容するなら要修正</remarks>
    Private Function TakusouDataGet(ByVal SQLcon As SqlConnection) As List(Of OutputOrdedrInfo)
        Dim retVal As New List(Of OutputOrdedrInfo)
        If IsNothing(Me.OIT0003Takusoutbl) Then
            Me.OIT0003Takusoutbl = New DataTable
        End If

        If Me.OIT0003Takusoutbl.Columns.Count <> 0 Then
            Me.OIT0003Takusoutbl.Columns.Clear()
        End If

        Me.OIT0003Takusoutbl.Clear()
        '画面上選択されたORDERNO一覧を生成
        Dim qcheckedRow = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "" Select Convert.ToString(dr("ORDERNO")))
        'ここまで来て未選択はありえないが念のため
        If qcheckedRow.Any = False Then
            Return Nothing
        End If
        Dim selectedOrderNo As List(Of String) = qcheckedRow.ToList
        Dim selectedOrderNoInStat As String = String.Join(",", (From odrNo In selectedOrderNo Select "'" & odrNo & "'"))
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ODR.ORDERNO")            'キー情報
        sqlStat.AppendLine("     , DET.DETAILNO")           'キー情報
        sqlStat.AppendLine("     , AGR.FIXEDNO")            '固定No
        sqlStat.AppendLine("     , AGR.AGREEMENTCODE")      '協定コード
        sqlStat.AppendLine("     , AGR.EXTRADISCOUNTCODE")  '割引コード
        sqlStat.AppendLine("     , OCNV.VALUE01 AS TAKUSOUOILCODE") '品目コード
        sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' THEN '437' ELSE '431' END AS TRTYPE") '車種コード
        sqlStat.AppendLine("     , ODR.TRAINNO")            '貨車番号
        sqlStat.AppendLine("     , TNK.TANKNUMBER")         '列車番号
        sqlStat.AppendLine("     , ODR.ARRSTATIONNAME")     '着駅名
        sqlStat.AppendLine("     , NIU.TAKUSOUNAME")        '荷受人名（帳票用）
        '帳票ヘッダー用項目(先頭レコードで設定)
        sqlStat.AppendLine("     , ODR.DEPSTATIONNAME")     '発駅名
        sqlStat.AppendLine("     , format(ODR.DEPDATE,'yyyy/MM/dd') AS HKDATE")     '発行日
        '帳票ソート用項目
        sqlStat.AppendLine("     , PRD.JROILTYPE")          'ソート条件利用用(D:危険品・N:通常）

        sqlStat.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        '明細結合ここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DET")
        sqlStat.AppendLine("    ON ODR.ORDERNO =  DET.ORDERNO")
        sqlStat.AppendLine("   AND DET.DELFLG  = @DELFLG")
        '明細結合ここまで↑
        '油種マスタここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIM0003_PRODUCT PRD")
        sqlStat.AppendLine("    ON PRD.OFFICECODE     = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND PRD.SHIPPERCODE    = ODR.SHIPPERSCODE")
        sqlStat.AppendLine("   AND PRD.PLANTCODE      = ODR.BASECODE")
        sqlStat.AppendLine("   AND PRD.OILCODE        = DET.OILCODE")
        sqlStat.AppendLine("   AND PRD.SEGMENTOILCODE = DET.ORDERINGTYPE")
        sqlStat.AppendLine("   AND PRD.DELFLG         = @DELFLG")
        '油種マスタここまで↑
        'パターンマスタここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIM0010_PATTERN PAT")
        sqlStat.AppendLine("    ON PAT.PATCODE     = ODR.ORDERTYPE")
        sqlStat.AppendLine("   AND PAT.BRANCH      = '1'")
        sqlStat.AppendLine("   AND PAT.DELFLG      = @DELFLG")
        'パターンマスタここまで↑
        'タンク車マスタここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIM0005_TANK TNK")
        sqlStat.AppendLine("    ON TNK.TANKNUMBER  = DET.TANKNO")
        sqlStat.AppendLine("   AND TNK.DELFLG      = @DELFLG")
        'タンク車マスタここまで↑
        '協定マスタここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0027_AGREEMENT AGR")
        sqlStat.AppendLine("    ON AGR.DEPSTATION      = ODR.DEPSTATION")
        sqlStat.AppendLine("   AND AGR.ARRSTATION      = ODR.ARRSTATION")
        sqlStat.AppendLine("   AND AGR.LOAD            = replace(CONVERT(varchar, TNK.LOAD, 1), '.0', '') + TNK.LOADUNIT")
        sqlStat.AppendLine("   AND AGR.TRAINNO         = ODR.TRAINNO")
        sqlStat.AppendLine("   AND AGR.PURPOSE         = PAT.KBNNAME") '回送の()つき、及びブランクの取り方が不明パターンのPURPOSEだと一致しない
        sqlStat.AppendLine("   AND AGR.LOADSHIPPRODUCT = PRD.JROILTYPENAME")
        sqlStat.AppendLine("   AND AGR.DELFLG          = @DELFLG")
        '協定マスタここまで↑
        '荷受人マスタここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIM0012_NIUKE NIU")
        sqlStat.AppendLine("    ON NIU.CONSIGNEECODE   = ODR.CONSIGNEECODE")
        sqlStat.AppendLine("   AND NIU.DELFLG          = @DELFLG")
        '荷受人マスタここまで↑
        '変換マスタ（油種コード⇒託送指示用油種コード）ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0029_CONVERT OCNV")
        sqlStat.AppendLine("    ON OCNV.CLASS          = 'TAKUSOUOIL'")
        sqlStat.AppendLine("   AND OCNV.KEYCODE01      = DET.OILCODE")
        sqlStat.AppendLine("   AND OCNV.DELFLG         = @DELFLG")
        '変換マスタ（油種コード⇒託送指示用油種コード）ここまで↑
        sqlStat.AppendLine(" WHERE ODR.ORDERSTATUS <= @ORDERSTATUS")
        sqlStat.AppendLine("   AND ODR.DELFLG       = @DELFLG")
        sqlStat.AppendFormat("   AND ODR.ORDERNO     IN({0})", selectedOrderNoInStat).AppendLine()
        Try
            '並び順は抽出後
            Using sqlCmd As New SqlCommand(sqlStat.ToString, SQLcon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                    .Add("@ORDERSTATUS", SqlDbType.NVarChar).Value = BaseDllConst.CONST_ORDERSTATUS_310
                End With
                'SQL実行
                Dim wrkDt As New DataTable
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        wrkDt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                        OIT0003Takusoutbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    wrkDt.Load(sqlDr)
                End Using

                Dim sortedDt = From dr As DataRow In wrkDt Order By Convert.ToString(dr("AGREEMENTCODE")), Convert.ToString(dr("JROILTYPE"))
                For Each sortedDr As DataRow In sortedDt
                    Dim newDr As DataRow = OIT0003Takusoutbl.NewRow

                    For Each col As DataColumn In wrkDt.Columns
                        newDr(col.ColumnName) = sortedDr(col.ColumnName)
                    Next

                    OIT0003Takusoutbl.Rows.Add(newDr)
                    retVal.Add(New OutputOrdedrInfo(Convert.ToString(sortedDr("ORDERNO")), Convert.ToString(sortedDr("DETAILNO"))))
                Next
            End Using
            Return retVal
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL TAKUSOU_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL TAKUSOU_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try

    End Function
    ''' <summary>
    ''' 出荷予約データを取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <returns>処理対象の受注Noと明細No</returns>
    ''' <remarks>このロジックにたどりつけるのは積置無しのみ、積置を許容するなら要修正</remarks>
    Private Function ReservedDataGet(ByVal SQLcon As SqlConnection) As List(Of OutputOrdedrInfo)
        '当処理の抽出結果の全フィールドを帳票に出すわけではない

        Dim retVal As New List(Of OutputOrdedrInfo)
        If IsNothing(Me.OIT0003Reserved) Then
            Me.OIT0003Reserved = New DataTable
        End If

        If Me.OIT0003Reserved.Columns.Count <> 0 Then
            Me.OIT0003Reserved.Columns.Clear()
        End If

        Me.OIT0003Reserved.Clear()
        '画面上選択されたORDERNO一覧を生成
        Dim qcheckedRow = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "" Select Convert.ToString(dr("ORDERNO")))
        'ここまで来て未選択はありえないが念のため
        If qcheckedRow.Any = False Then
            Return Nothing
        End If
        '先頭の選択された積込日取得（全て同一日想定）
        Dim lodDate = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "" Select Convert.ToString(dr("LODDATE"))).FirstOrDefault

        Dim selectedOrderNo As List(Of String) = qcheckedRow.ToList
        Dim selectedOrderNoInStat As String = String.Join(",", (From odrNo In selectedOrderNo Select "'" & odrNo & "'"))
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT ODR.ORDERNO")            'キー情報
        sqlStat.AppendLine("     , DET.DETAILNO")           'キー情報
        sqlStat.AppendLine("     , ODR.OFFICECODE AS OFFICECODE")     '営業所コード
        sqlStat.AppendLine("     , format(ODR.LODDATE,'yyyy/MM/dd') AS LODDATE")     '積込日
        sqlStat.AppendLine("     , format(ODR.LODDATE,'yyyyMMdd') AS LODDATE_WITHOUT_SLASH")     '積込日(スラなし）
        sqlStat.AppendLine("     , ISNULL(DET.RESERVEDNO,'')        AS RESERVEDNO")  '予約番号
        sqlStat.AppendLine("     , PRD.REPORTOILNAME")            '油種コード(甲子用？）
        sqlStat.AppendLine("     , LRV.RESERVEDQUANTITY")         '予約数量
        sqlStat.AppendLine("     , ODR.TRAINNO")
        sqlStat.AppendLine("     , RIGHT('0000' + ODR.TRAINNO,4) AS TRAINNO_PAD_ZERO")
        sqlStat.AppendLine("     , DET.TANKNO")
        sqlStat.AppendLine("     , TNK.MODEL")  'モデル⇒ﾀｷ1000の場合・・・など後続の設定処理の分岐で利用
        sqlStat.AppendLine("     , TNK.OLDTANKNUMBER") '旧JOT車番
        sqlStat.AppendLine("     , PRD.SHIPPEROILCODE") '荷主油種コード
        sqlStat.AppendLine("     , PRD.SHIPPEROILNAME") '荷主油種名
        sqlStat.AppendLine("     , ODR.SHIPPERSCODE")
        sqlStat.AppendLine("     , ODR.CONSIGNEECODE")
        sqlStat.AppendLine("     , CASE WHEN ISNULL(DET.SECONDCONSIGNEECODE,'') = '' THEN CCNV.VALUE01 ELSE CMICNV.VALUE01 END AS CONSIGNEECONVCODE")
        'sqlStat.AppendLine("     , CCNV.VALUE02 AS CONSIGNEECONVVALUE")
        sqlStat.AppendLine("     , CASE WHEN ISNULL(DET.SECONDCONSIGNEECODE,'') = '' THEN CCNV.VALUE02 ELSE CMICNV.VALUE02 END AS CONSIGNEECONVVALUE")
        'sqlStat.AppendLine("     , CCNV.VALUE03 AS TRANSNAME") '便名 現状袖ヶ浦のみ
        sqlStat.AppendLine("     , CASE WHEN ISNULL(DET.SECONDCONSIGNEECODE,'') = '' THEN CCNV.VALUE03 ELSE CMICNV.VALUE03 END AS TRANSNAME")
        sqlStat.AppendLine("     , SCNV.VALUE01 AS SHIPPERCONVCODE")
        sqlStat.AppendLine("     , SCNV.VALUE02 AS SHIPPERCONVVALUE")
        sqlStat.AppendLine("     , '1'          AS KINO_DATAKBN")
        sqlStat.AppendLine("     , ''           AS OUTPUTRESERVENO") '出力用予約番号(後続処理で番号を組み立てる）
        sqlStat.AppendLine("     , '2'          AS KINO_TOKUISAKICODE") '得意先コード（甲子）
        sqlStat.AppendLine("     , 'ＥＮＥＯＳ株式会社□□□□□'   AS KINO_TOKUISAKINAME") '得意先名（甲子）
        sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' THEN TNK.JXTGTANKNUMBER2 ELSE convert(nvarchar,convert(int,TNK.JXTGTANKNUMBER2)) END AS KINO_TRAINNO")
        sqlStat.AppendLine("     , '0'          AS NEG_TUMIKOMI_KAI")
        sqlStat.AppendLine("     , '0'          AS NEG_TUMIKOMI_POINT")
        'sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' AND convert(int,DET.TANKNO) between 1 and 999 THEN '1000-' + RIGHT('000' + DET.TANKNO,3)  ")
        'sqlStat.AppendLine("            WHEN TNK.MODEL = 'タキ1000' AND convert(int,DET.TANKNO) >= 1000           THEN '1001-' + RIGHT(DET.TANKNO,3)  ")
        'sqlStat.AppendLine("            ELSE DET.TANKNO END AS NEG_KASHANO")
        sqlStat.AppendLine("     , TNK.JXTGTANKNUMBER4 AS NEG_KASHANO")
        sqlStat.AppendLine("     , convert(int,PRD.SHIPPEROILCODE) AS NEG_SHIPPEROILCODE")
        sqlStat.AppendLine("     , '0'          AS NEG_SETTEI_NUM")
        sqlStat.AppendLine("     , '0'          AS NEG_ARM_CODE")
        sqlStat.AppendLine("     , '0'          AS NEG_TSUMI_NUM")

        sqlStat.AppendLine("     , '計画済'    AS SOD_STATUS")    '袖ヶ浦ステータス
        sqlStat.AppendLine("     , ''          AS SOD_SHELL_ORDERNO") '袖ヶ浦SHELL受注番号
        sqlStat.AppendLine("     , '0'         AS SOD_TRANS_KBN") '袖ヶ浦輸送方法
        sqlStat.AppendLine("     , PRD.SHIPPEROILCODE + '00000' AS SOD_SHIPPEROILCODE") '袖ヶ浦輸送方法
        sqlStat.AppendLine("     , CASE WHEN PRD.MIDDLEOILCODE = '1' THEN '課税' ELSE 'その他' END AS SOD_TAX_KBN") '袖ヶ浦課税区分
        sqlStat.AppendLine("     , format(LRV.RESERVEDQUANTITY,'#0.000') AS SOD_RESERVEDQUANTITY")    '袖ヶ浦用_予約数量
        sqlStat.AppendLine("     , ''          AS SOD_TRANS_COMP") '袖ヶ浦運送会社
        sqlStat.AppendLine("     , '0'         AS SOD_BACKNAME") '袖ヶ浦戻り
        sqlStat.AppendLine("     , '10'        AS SEQ_DATATYPE_RESERVED") 'シーケンスファイルデータ区部（予約）
        sqlStat.AppendLine("     , '1'         AS SEQ_PROC_KBN") 'シーケンスファイル処理区分(一旦新規のみ）
        sqlStat.AppendLine("     , CASE WHEN ODR.OFFICECODE = '011201' THEN '06' ELSE '08' END AS SEQ_DEPT_CODE") 'シーケンスファイル支店コード
        sqlStat.AppendLine("     , '03'         AS SEQ_TORIKBN") 'シーケンスファイル取引区分コード
        sqlStat.AppendLine("     , '00000'         AS SEQ_TOKUISAKI") 'シーケンスファイル得意先コード
        sqlStat.AppendLine("     , format(ODR.ARRDATE,'yyyyMMdd') AS ARRDATE_WITHOUT_SLASH")     '積車着日(スラなし）
        sqlStat.AppendLine("     , CASE WHEN PRD.MIDDLEOILCODE = '1' THEN '1' ELSE '0' END AS SEQ_TAX_KBN")     'シーケンスファイル課税区分
        sqlStat.AppendLine("     , '010'         AS SEQ_NISCODE") 'シーケンスファイル荷姿コード
        sqlStat.AppendLine("     , CASE WHEN ODR.OFFICECODE = '011201' THEN '0011'  ELSE '0012' END AS SEQ_UKEHARAI_CODE") 'シーケンスファイル受払い基地コード
        sqlStat.AppendLine("     , CASE WHEN ODR.OFFICECODE = '011201' THEN '01000' ELSE '99999' END AS SEQ_ORDERAMOUNT") 'シーケンスファイルオーダー数量
        sqlStat.AppendLine("     , '1'           AS SEQ_TRANSWAY") 'シーケンスファイル輸送方法
        sqlStat.AppendLine("     , CASE WHEN TRA.TRAINCLASS = 'O'")
        sqlStat.AppendLine("                 THEN '023'")
        sqlStat.AppendLine("            WHEN TRA.TRAINCLASS = 'J' AND DET.OTTRANSPORTFLG = '1'")
        sqlStat.AppendLine("                 THEN '023'")
        sqlStat.AppendLine("            WHEN TRA.TRAINCLASS = 'J' AND ODR.OFFICECODE = '011201'")
        sqlStat.AppendLine("                 THEN '022'")
        sqlStat.AppendLine("            WHEN TRA.TRAINCLASS = 'J' AND ODR.OFFICECODE = '012401'")
        sqlStat.AppendLine("                 THEN '017'")
        sqlStat.AppendLine("             END")
        sqlStat.AppendLine("            AS SEQ_GYOUSYACODE") 'シーケンス業者コード
        sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' OR TNK.MODEL = 'タキ43000'")
        sqlStat.AppendLine("                 THEN RIGHT('000000' + TNK.TANKNUMBER,6)")
        sqlStat.AppendLine("            WHEN TNK.MODEL = 'タキ243000'")
        sqlStat.AppendLine("                 THEN RIGHT('000000' + STUFF(TNK.TANKNUMBER, 3, 1 ,''),6)")
        sqlStat.AppendLine("            ELSE '000000'")
        sqlStat.AppendLine("             END")
        sqlStat.AppendLine("            AS SEQ_TANKNO") 'シーケンス業者コード
        sqlStat.AppendLine("     , CASE WHEN TNK.TANKNUMBER IS NULL THEN '0' ELSE '1' END AS SEQ_KAIJI") 'シーケンスファイル回次
        sqlStat.AppendLine("     , '00000' AS SEQ_DEN_NO") 'シーケンス伝票№
        sqlStat.AppendLine("     , '0'     AS SEQ_DEN_MEI_NO") 'シーケンス伝票明細№
        sqlStat.AppendLine("     , '00000' AS SEQ_ACCTUAL_AMOUNT") 'シーケンス実績数量
        sqlStat.AppendLine("     , '0000'  AS SEQ_NIYAKU_BEGIN_TIME") 'シーケンス荷役開始時刻
        sqlStat.AppendLine("     , '0000'  AS SEQ_NIYAKU_END_TIME") 'シーケンス荷役終了時刻
        sqlStat.AppendLine("     , RIGHT('0000' + ODR.TRAINNO,4)  AS SEQ_TRAINNO") 'シーケンス列車番号
        sqlStat.AppendLine("     , ''  AS SEQ_TOKUISAKI_KANA") 'シーケンス得意先名（略称カナ）
        sqlStat.AppendLine("     , ''  AS SEQ_HAISOU_KANA") 'シーケンス配送先名（略称カナ）
        sqlStat.AppendLine("     , ''  AS SEQ_HINMEI_KANA") 'シーケンス品名コード（略称カナ）
        sqlStat.AppendLine("     , ''  AS SEQ_TAXKBN_KANA") '税区分名（略称カナ）（略称カナ）
        sqlStat.AppendLine("     , Format(GetDate(),'yyyyMMddHHmm')  AS SEQ_CREATEDATETIME") 'シーケンスデータ作成年月日時分
        sqlStat.AppendLine("     , CASE WHEN ODR.OFFICECODE = '011201' THEN '046'  ELSE '071' END  AS SEQ_PLANTCODE") 'シーケンス当社基地コード
        sqlStat.AppendLine("     , '016'  AS SEQ_SHIPPERCODE") 'シーケンス当社荷主コード
        sqlStat.AppendLine("     , RIGHT('000000'+ODR.CONSIGNEECODE,6)  AS SEQ_CONSIGNEECODE") 'シーケンス当社着受荷受人（内部）C
        sqlStat.AppendLine("     , ''  AS SEQ_YOBI")
        sqlStat.AppendLine("     , TNK.LOAD") 'デバッグ用
        sqlStat.AppendLine("     , DET.OILCODE")  'デバッグ用
        sqlStat.AppendLine("     , DET.ORDERINGTYPE") 'デバッグ用
        sqlStat.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        '明細結合ここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DET")
        sqlStat.AppendLine("    ON ODR.ORDERNO =  DET.ORDERNO")
        sqlStat.AppendLine("   And DET.DELFLG  = @DELFLG")
        '明細結合ここまで↑
        '油種マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0003_PRODUCT PRD")
        sqlStat.AppendLine("    ON PRD.OFFICECODE     = ODR.OFFICECODE")
        sqlStat.AppendLine("   And PRD.SHIPPERCODE    = ODR.SHIPPERSCODE")
        sqlStat.AppendLine("   And PRD.PLANTCODE      = ODR.BASECODE")
        sqlStat.AppendLine("   And PRD.OILCODE        = DET.OILCODE")
        sqlStat.AppendLine("   And PRD.SEGMENTOILCODE = DET.ORDERINGTYPE")
        sqlStat.AppendLine("   And PRD.DELFLG         = @DELFLG")
        '油種マスタ結合ここまで↑
        'タンク車マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0005_TANK TNK")
        sqlStat.AppendLine("    ON TNK.TANKNUMBER  = DET.TANKNO")
        sqlStat.AppendLine("   And TNK.DELFLG      = @DELFLG")
        'タンク車マスタ結合ここまで↑
        '列車マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0007_TRAIN TRA")
        sqlStat.AppendLine("    ON TRA.OFFICECODE  = @OFFICECODE")
        sqlStat.AppendLine("   And TRA.TRAINNO     = ODR.TRAINNO")
        sqlStat.AppendLine("   And TRA.TSUMI       = CASE WHEN ODR.STACKINGFLG = '1' THEN 'T' ELSE 'N' END")
        sqlStat.AppendLine("   AND TRA.DEPSTATION  = ODR.DEPSTATION")
        sqlStat.AppendLine("   AND TRA.ARRSTATION  = ODR.ARRSTATION")
        sqlStat.AppendLine("   AND TRA.DELFLG      = @DELFLG")
        '列車マスタ結合ここまで↑
        '積込予約マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0021_LOADRESERVE LRV")
        sqlStat.AppendLine("    ON LRV.OFFICECODE     = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND ODR.LODDATE        BETWEEN LRV.FROMYMD AND LRV.TOYMD")
        sqlStat.AppendLine("   AND LRV.LOAD           = TNK.LOAD")
        sqlStat.AppendLine("   AND LRV.OILCODE        = DET.OILCODE")
        sqlStat.AppendLine("   AND LRV.SEGMENTOILCODE = DET.ORDERINGTYPE")
        sqlStat.AppendLine("   AND LRV.DELFLG         = @DELFLG")
        '積込予約マスタ結合ここまで↑
        '変換マスタ（荷受人）結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0029_CONVERT CCNV")
        sqlStat.AppendLine("    ON CCNV.CLASS          = 'RESERVED_NIUKE'")
        sqlStat.AppendLine("   AND CCNV.KEYCODE01      = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND CCNV.KEYCODE02      = ODR.CONSIGNEECODE")
        sqlStat.AppendLine("   AND CCNV.DELFLG         = @DELFLG")
        '変換マスタ（荷受人）結合ここまで↑
        '変換マスタ（荷主）結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0029_CONVERT SCNV")
        sqlStat.AppendLine("    ON SCNV.CLASS          = 'RESERVED_SHIPPER'")
        sqlStat.AppendLine("   AND SCNV.KEYCODE01      = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND SCNV.KEYCODE02      = ODR.SHIPPERSCODE")
        sqlStat.AppendLine("   AND SCNV.DELFLG         = @DELFLG")
        '変換マスタ（荷主）結合ここまで↑
        '変換マスタ（荷受人（構内取））結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0029_CONVERT CMICNV")
        sqlStat.AppendLine("    ON CMICNV.CLASS          = 'RESERVED_NIUKE'")
        sqlStat.AppendLine("   AND CMICNV.KEYCODE01      = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND CMICNV.KEYCODE02      = DET.SECONDCONSIGNEECODE")
        sqlStat.AppendLine("   AND CMICNV.DELFLG         = @DELFLG")
        '変換マスタ（荷受人（構内取））結合ここまで↑
        sqlStat.AppendLine(" WHERE ODR.ORDERSTATUS <= @ORDERSTATUS")
        sqlStat.AppendLine("   AND ODR.DELFLG       = @DELFLG")
        sqlStat.AppendFormat("   AND ODR.ORDERNO     IN({0})", selectedOrderNoInStat).AppendLine()

        Dim sqlMaxReservedNo As New StringBuilder
        sqlMaxReservedNo.AppendLine("SELECT ISNULL(MAX(DET.RESERVEDNO),'000') AS RESERVEDNO")
        sqlMaxReservedNo.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR")
        '明細結合ここから↓
        sqlMaxReservedNo.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DET")
        sqlMaxReservedNo.AppendLine("    ON ODR.ORDERNO =  DET.ORDERNO")
        '明細結合ここまで↑
        sqlMaxReservedNo.AppendLine(" WHERE ODR.LODDATE    = @LODDATE")
        sqlMaxReservedNo.AppendLine("   AND ODR.OFFICECODE = @OFFICECODE")
        Try
            '並び順は抽出後
            Using sqlCmd As New SqlCommand(sqlStat.ToString, SQLcon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                    .Add("@ORDERSTATUS", SqlDbType.NVarChar).Value = BaseDllConst.CONST_ORDERSTATUS_310
                    .Add("@LODDATE", SqlDbType.Date).Value = lodDate
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = work.WF_SEL_OTS_SALESOFFICECODE.Text
                End With
                'SQL実行
                Dim wrkDt As New DataTable
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        wrkDt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                        OIT0003Reserved.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    wrkDt.Load(sqlDr)
                End Using

                sqlCmd.CommandText = sqlMaxReservedNo.ToString
                Dim maxReservedNo As String = Convert.ToString(sqlCmd.ExecuteScalar)
                If maxReservedNo = "" Then
                    maxReservedNo = "000"
                End If

                Dim sortedDt = From dr As DataRow In wrkDt 'Order By Convert.ToString(dr("AGREEMENTCODE")), Convert.ToString(dr("JROILTYPE"))
                Dim officeCode As String = ""
                If sortedDt.Any Then
                    officeCode = Convert.ToString(sortedDt.First.Item("OFFICECODE"))
                End If
                For Each sortedDr As DataRow In sortedDt
                    Dim newDr As DataRow = OIT0003Reserved.NewRow

                    For Each col As DataColumn In wrkDt.Columns
                        newDr(col.ColumnName) = sortedDr(col.ColumnName)
                    Next
                    Dim reservedNo As String = Convert.ToString(sortedDr("RESERVEDNO"))
                    If reservedNo = "" Then
                        maxReservedNo = (CInt(maxReservedNo) + 1).ToString("000")
                        reservedNo = maxReservedNo
                    End If
                    Select Case officeCode
                        Case "011402" '根岸(前0無しの予約番号のみ)
                            newDr("OUTPUTRESERVENO") = Convert.ToString(CInt(reservedNo))
                        Case "011203" '袖ヶ浦(積込日+2桁0埋め予約番号)
                            newDr("OUTPUTRESERVENO") = Convert.ToString(newDr("LODDATE_WITHOUT_SLASH")) & CInt(reservedNo).ToString("00")
                        Case "011201", "012401" '五井、四日市（３桁の予約番号のみ）
                            newDr("OUTPUTRESERVENO") = CInt(reservedNo).ToString("000")
                        Case Else 'その他は積込日+3桁0埋め予約番号
                            newDr("OUTPUTRESERVENO") = Convert.ToString(newDr("LODDATE_WITHOUT_SLASH")) & reservedNo
                    End Select

                    OIT0003Reserved.Rows.Add(newDr)

                    Dim orderInf = New OutputOrdedrInfo(Convert.ToString(sortedDr("ORDERNO")), Convert.ToString(sortedDr("DETAILNO")))
                    orderInf.ReservedNo = reservedNo
                    retVal.Add(orderInf)
                Next
            End Using
            Return retVal
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL RESERVED_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL RESERVED_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' 受注・受注明細テーブルの各出力フラグ及び、カウントをインクリメント
    ''' </summary>
    ''' <param name="uploadOrderInfo">出力した受注キー情報</param>
    ''' <param name="callerButton">呼出し元ボタン</param>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="sqlTran">トランザクション</param>
    Private Function IncrementDetailOutputCount(uploadOrderInfo As List(Of OutputOrdedrInfo), callerButton As String, sqlCon As SqlConnection, sqlTran As SqlTransaction, Optional procDate As Date = #1900/1/1#, Optional updateReservedNo As Boolean = False) As Boolean
        Try

            Dim sqlStat As StringBuilder
            If procDate = #1900/1/1# Then
                procDate = Now
            End If

            '選択済の画面の行データ取得
            Dim checkedRow As DataTable = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "").CopyToDataTable

            '選択した受注No、積込日と合致する明細行のインクリメント
            'アップロード方式によりインクリメントフィールドを変更
            Dim incFieldName As String = ""
            Select Case callerButton
                Case "WF_ButtonOtSend" 'OT発送日報出力
                    incFieldName = "OTSENDCOUNT"
                Case "WF_ButtonReserved" '製油所出荷予約
                    incFieldName = "DLRESERVEDCOUNT"
                Case "WF_ButtonTakusou" '託送指示
                    incFieldName = "DLTAKUSOUCOUNT"
                Case Else
                    Throw New Exception("想定外のボタンが押されました")
            End Select
            sqlStat = New StringBuilder
            sqlStat.AppendLine("UPDATE OIL.OIT0003_DETAIL")
            sqlStat.AppendFormat("   SET {0} = ISNULL({0},0) + 1", incFieldName).AppendLine()
            If updateReservedNo Then
                sqlStat.AppendLine("       ,RESERVEDNO         = @RESERVEDNO")
            End If
            sqlStat.AppendLine("       ,UPDYMD             = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER            = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID          = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD         = @RECEIVEYMD")
            sqlStat.AppendLine(" WHERE ORDERNO  = @ORDERNO")
            sqlStat.AppendLine("   AND DETAILNO = @DETAILNO")
            sqlStat.AppendLine("   AND DELFLG   = @DELFLG") 'ここまで来て削除フラグ1はありえないが念の為

            For Each orderKey In uploadOrderInfo
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
                    With sqlCmd.Parameters
                        '値
                        If updateReservedNo Then
                            .Add("@RESERVEDNO", SqlDbType.NVarChar).Value = orderKey.ReservedNo
                        End If
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = C_DEFAULT_YMD
                        '条件
                        .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderKey.OrderNo
                        .Add("@DETAILNO", SqlDbType.NVarChar).Value = orderKey.DetailNo
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using
            Next orderKey
            Return True
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL INCREMENT_OUTPUT_CNT", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL INCREMENT_OUTPUT_CNT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return False
        End Try

    End Function
    ''' <summary>
    ''' 受注明細を検索、ダウンロード回数を判定し更新すべき出力フラグ値を取得
    ''' </summary>
    ''' <param name="uploadOrderInfo">更新した受注情報（オーダーNo,明細No）</param>
    ''' <param name="callerButton">呼出し元ボタン</param>
    ''' <param name="sqlCon">SQLコネクション</param>
    ''' <param name="sqlTran">トランザクションオブジェクト</param>
    ''' <returns>ORDER番号とフラグ値のディクショナリ※nothing:エラー発生時</returns>
    Private Function GetOutputFlag(uploadOrderInfo As List(Of OutputOrdedrInfo), callerButton As String, sqlCon As SqlConnection, sqlTran As SqlTransaction) As Dictionary(Of String, String)
        Try
            '更新したオーダー番号をユニークにする
            Dim orderNoList = (From itm In uploadOrderInfo Group By g = itm.OrderNo Into Group Select g).ToList
            '呼出し元ボタンに応じカウントアップしたフィールドを取得
            Dim incFieldName As String
            Select Case callerButton
                Case "WF_ButtonOtSend" 'OT発送日報出力
                    incFieldName = "OTSENDCOUNT"
                Case "WF_ButtonReserved" '製油所出荷予約
                    incFieldName = "DLRESERVEDCOUNT"
                Case "WF_ButtonTakusou" '託送指示
                    incFieldName = "DLTAKUSOUCOUNT"
                Case Else
                    Throw New Exception("想定外のボタンが押されました")
            End Select
            Dim sqlStat = New StringBuilder
            sqlStat.AppendLine("SELECT ORDERNO ")
            sqlStat.AppendFormat("      ,SUM(CASE WHEN ISNULL({0},0)  = 0 THEN 1 ELSE 0 END) AS CNT_ZERO", incFieldName).AppendLine()
            sqlStat.AppendFormat("      ,SUM(CASE WHEN ISNULL({0},0)  = 1 THEN 1 ELSE 0 END) AS CNT_ONE", incFieldName).AppendLine()
            sqlStat.AppendFormat("      ,SUM(CASE WHEN ISNULL({0},0) >= 2 THEN 1 ELSE 0 END) AS CNT_OVER2", incFieldName).AppendLine()
            sqlStat.AppendLine("      ,COUNT(DETAILNO) AS CNT_TTL")
            sqlStat.AppendLine("  FROM OIL.OIT0003_DETAIL WITH(nolock)")
            sqlStat.AppendLine(" WHERE DELFLG   = @DELFLG")
            Dim inStat As String = String.Join(",", (From itm In orderNoList Select "'" & itm & "'"))
            sqlStat.AppendFormat("   AND ORDERNO IN ({0})", inStat).AppendLine()
            sqlStat.AppendLine(" GROUP BY ORDERNO")
            Dim retDec As New Dictionary(Of String, String)

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
                sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        Dim orderNo As String = Convert.ToString(sqlDr("ORDERNO"))
                        Dim cntZero As Decimal = CDec(sqlDr("CNT_ZERO"))
                        Dim cntOne As Decimal = CDec(sqlDr("CNT_ONE"))
                        Dim cntOverTwo As Decimal = CDec(sqlDr("CNT_OVER2"))
                        Dim cntTotal As Decimal = CDec(sqlDr("CNT_TTL"))


                        If cntZero = cntTotal Then
                            '全件0の場合は未送信(そもそも
                            '　　　　　　　　　　当画面でこのケースはありえない）
                            retDec.Add(orderNo, "0")
                            Continue While
                        ElseIf callerButton <> "WF_ButtonOtSend" Then
                            '発送日報以外で未送信以外は基本的に送信済
                            '再送信の情報も押えない
                            retDec.Add(orderNo, "1")
                            Continue While
                        End If
                        '***************************
                        '以下は発送日報のみの制御
                        '***************************
                        If cntOne = cntTotal Then
                            '全て一度送信
                            retDec.Add(orderNo, "1")
                            Continue While
                        End If
                        '以下は発送日報のみの制御
                        If cntZero >= 1 AndAlso cntOverTwo = 0 Then
                            '未送信があり、再送信が無い場合
                            retDec.Add(orderNo, "2") '一部送信
                            Continue While
                        End If

                        If cntZero >= 1 AndAlso cntOverTwo >= 1 Then
                            '未送信があり、再送信がある場合
                            retDec.Add(orderNo, "3") '一部再送信済
                            Continue While
                        End If

                        'ここまで来たら全て送信、または再送信している状態
                        retDec.Add(orderNo, "4") '再送信済
                        Continue While
                    End While
                End Using 'sqlDr
            End Using 'sqlCmd
            Return retDec

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL GETOUTPUTFLG", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL GETOUTPUTFLG"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' 引数の情報を元に受注テーブルの出力フラグを更新
    ''' </summary>
    ''' <param name="orderOutputFlags">キー：オーダー番号、値：出力フラグ値</param>
    ''' <param name="callerButton">呼出し元ボタン</param>
    ''' <param name="sqlCon">SQL接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト</param>
    ''' <param name="procDate">処理日、※未指定日処理実行時点の日時</param>
    ''' <returns>処理結果：True:正常、False：異常</returns>
    Private Function UpdateOrderOutputFlag(orderOutputFlags As Dictionary(Of String, String), callerButton As String, sqlCon As SqlConnection, sqlTran As SqlTransaction, Optional procDate As Date = #1900/1/1#) As Boolean
        Try
            Dim sqlStat As StringBuilder
            If procDate = #1900/1/1# Then
                procDate = Now
            End If

            '選択済の画面の行データ取得
            Dim checkedRow As DataTable = (From dr As DataRow In OIT0003tbl Where Convert.ToString(dr("OPERATION")) <> "").CopyToDataTable

            '選択した受注No、積込日と合致する明細行のインクリメント
            'アップロード方式によりインクリメントフィールドを変更
            Dim updFieldName As String = ""
            Select Case callerButton
                Case "WF_ButtonOtSend" 'OT発送日報出力
                    updFieldName = "OTSENDSTATUS"
                Case "WF_ButtonReserved" '製油所出荷予約
                    updFieldName = "RESERVEDSTATUS"
                Case "WF_ButtonTakusou" '託送指示
                    updFieldName = "TAKUSOUSTATUS"
                Case Else
                    Throw New Exception("想定外のボタンが押されました")
            End Select
            sqlStat = New StringBuilder
            sqlStat.AppendLine("UPDATE OIL.OIT0002_ORDER")
            sqlStat.AppendFormat("   SET {0} = @FLAGVALUE", updFieldName).AppendLine()
            sqlStat.AppendLine("       ,UPDYMD             = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER            = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID          = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD         = @RECEIVEYMD")
            sqlStat.AppendLine(" WHERE ORDERNO  = @ORDERNO")
            sqlStat.AppendLine("   AND DELFLG   = @DELFLG") 'ここまで来て削除フラグ1はありえないが念の為

            For Each orderKey In orderOutputFlags
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
                    With sqlCmd.Parameters
                        '値
                        .Add("@FLAGVALUE", SqlDbType.NVarChar).Value = orderKey.Value
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = Master.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = Master.USERTERMID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = C_DEFAULT_YMD
                        '条件
                        .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderKey.Key
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using
            Next orderKey
            Return True
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL UPDATE_ORDER_UPLOADFLAG", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL UPDATE_ORDER_UPLOADFLAG"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return False
        End Try

    End Function
    ''' <summary>
    ''' 更新した受注明細の取得
    ''' </summary>
    ''' <param name="uploadOrderInfo"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Private Function GetUpdatedOrderDetail(uploadOrderInfo As List(Of OutputOrdedrInfo), sqlCon As SqlConnection, sqlTran As SqlTransaction) As DataTable
        Dim retDt As New DataTable
        Try
            Dim sqlStat = New StringBuilder
            sqlStat.AppendLine("SELECT *")
            sqlStat.AppendLine("  FROM OIL.OIT0003_DETAIL WITH(nolock)")
            sqlStat.AppendLine(" WHERE DELFLG   = @DELFLG")
            sqlStat.AppendLine("   AND (")
            Dim isFirst As Boolean = True
            For Each orderInfo In uploadOrderInfo
                If isFirst Then
                    isFirst = False
                    sqlStat.AppendFormat("     (ORDERNO = '{0}' AND DETAILNO = '{1}')", orderInfo.OrderNo, orderInfo.DetailNo).AppendLine()
                Else
                    sqlStat.AppendFormat(" OR  (ORDERNO = '{0}' AND DETAILNO = '{1}')", orderInfo.OrderNo, orderInfo.DetailNo).AppendLine()
                End If
            Next orderInfo

            sqlStat.AppendLine("       )")
            'SQL実行
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
                sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        retDt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    If retDt.Columns.Contains("UPDTIMSTP") Then
                        retDt.Columns.Remove("UPDTIMSTP")
                    End If
                    retDt.Load(sqlDr)
                End Using 'sqlDr
            End Using 'sqlCmd

            Return retDt
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL GET_UPDATED_ORDERDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL GET_UPDATED_ORDERDETAIL"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' 更新した受注の取得
    ''' </summary>
    ''' <param name="uploadOrderInfo"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Private Function GetUpdatedOrder(uploadOrderInfo As List(Of OutputOrdedrInfo), sqlCon As SqlConnection, sqlTran As SqlTransaction) As DataTable
        Dim retDt As New DataTable
        Try
            Dim sqlStat = New StringBuilder
            sqlStat.AppendLine("SELECT *")
            sqlStat.AppendLine("  FROM OIL.OIT0002_ORDER WITH(nolock)")
            sqlStat.AppendLine(" WHERE DELFLG   = @DELFLG")
            Dim orderNoList = (From itm In uploadOrderInfo Group By g = itm.OrderNo Into Group Select g).ToList
            Dim inStat As String = String.Join(",", (From itm In orderNoList Select "'" & itm & "'"))
            sqlStat.AppendFormat("   AND ORDERNO IN ({0})", inStat).AppendLine()
            'SQL実行
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
                sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        retDt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    If retDt.Columns.Contains("UPDTIMSTP") Then
                        retDt.Columns.Remove("UPDTIMSTP")
                    End If
                    retDt.Load(sqlDr)
                End Using 'sqlDr
            End Using 'sqlCmd

            Return retDt
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL GET_UPDATED_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003OTL GET_UPDATED_ORDER"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' 履歴テーブル用の情報を付与したデータテーブルに変換
    ''' </summary>
    ''' <returns></returns>
    Private Function ModifiedHistoryDatatable(dt As DataTable, historyNo As String) As DataTable
        Dim retDt As DataTable = dt.Clone
        '履歴とMAPIDの付与
        retDt.Columns.Add("HISTORYNO", GetType(String)).DefaultValue = historyNo
        retDt.Columns.Add("MAPID", GetType(String)).DefaultValue = Master.MAPID
        Dim retDr As DataRow = Nothing
        For Each dr As DataRow In dt.Rows
            retDr = retDt.NewRow
            For Each colName As DataColumn In dt.Columns
                retDr(colName.ColumnName) = dr(colName.ColumnName)
            Next
            retDt.Rows.Add(retDr)
        Next
        Return retDt
    End Function
    ''' <summary>
    ''' ジャーナル書き込み
    ''' </summary>
    ''' <param name="journalDt"></param>
    ''' <returns></returns>
    Private Function OutputJournal(journalDt As DataTable, tabName As String) As Boolean
        For Each dr As DataRow In journalDt.Rows
            CS0020JOURNAL.TABLENM = tabName
            CS0020JOURNAL.ACTION = "UPDATE"
            CS0020JOURNAL.ROW = dr
            CS0020JOURNAL.CS0020JOURNAL()
            If Not isNormal(CS0020JOURNAL.ERR) Then
                Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                Return False
            End If
        Next
        Return True
    End Function
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage(work.WF_SEL_CAMPCODE.Text)

    End Sub
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                WF_LeftMViewChange.Value = Integer.Parse(WF_LeftMViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case CInt(WF_LeftMViewChange.Value)
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_FILTERDATE"
                                .WF_Calendar.Text = WF_FILTERDATE_TEXT.Text
                        End Select
                        .ActiveCalendar()
                End Select
            End With
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
                WF_RightViewChange.Value = Integer.Parse(WF_RightViewChange.Value).ToString
            Catch ex As Exception
                Exit Sub
            End Try
            Dim enumVal = DirectCast([Enum].ToObject(GetType(GRIS0004RightBox.RIGHT_TAB_INDEX), CInt(WF_RightViewChange.Value)), GRIS0004RightBox.RIGHT_TAB_INDEX)
            rightview.SelectIndex(enumVal)
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
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            If CInt(OIT0003row("HIDDEN")) = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0003tbl)

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
        CS0013ProfView.SCROLLTYPE = CInt(CS0013ProfView.SCROLLTYPE_ENUM.Both).ToString
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = Convert.ToString(TBLview.Item(0)("SELECT"))
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 遷移先(OT連携一覧画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPOTLINKAGETBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPLINKTBL.txt"
    End Sub
    ''' <summary>
    ''' 一覧表フィルタ処理実行
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <param name="filterField">フィルタ対象フィールド</param>
    ''' <param name="filterDate">フィルタ対象日付</param>
    ''' <returns></returns>
    Private Function SetFilterValue(dt As DataTable, filterField As String, filterDate As String) As DataTable
        '対象のデータが無い場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return dt
        End If
        'フィルタフィールドが未指定または対象テーブルに未存在の場合はそのまま終了
        If filterField = "" OrElse dt.Columns.Contains(filterField) = False Then
            Return dt
        End If
        Dim dtFieldVal As String = filterDate
        If filterDate <> "" AndAlso IsDate(dtFieldVal) = False Then
            Return dt
        End If
        If filterDate <> "" Then
            dtFieldVal = CDate(dtFieldVal).ToString("yyyy/MM/dd")
        End If

        For Each dr As DataRow In dt.Rows
            If dtFieldVal <> "" AndAlso Not dr(filterField).Equals(dtFieldVal) Then
                dr("HIDDEN") = "1"
            Else
                dr("HIDDEN") = "0"
            End If
            'フィルタ再指定の場合はチェック状態をＯＦＦに変更
            If dtFieldVal <> "" Then
                dr("OPERATION") = ""
            End If
        Next

        Return dt
    End Function
    ''' <summary>
    ''' CSVファイル名取得
    ''' </summary>
    ''' <returns></returns>
    Private Function SetCSVFileName() As String
        Dim fileName As String = ""

        Select Case work.WF_SEL_OTS_SALESOFFICECODE.Text
            '★仙台新港営業所, 四日市営業所
            Case BaseDllConst.CONST_OFFICECODE_010402,
                 BaseDllConst.CONST_OFFICECODE_012401
                fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_OTRCV.FTP"

            '★五井営業所, 甲子営業所, 袖ヶ浦営業所, 根岸営業所
            Case BaseDllConst.CONST_OFFICECODE_011201,
                 BaseDllConst.CONST_OFFICECODE_011202,
                 BaseDllConst.CONST_OFFICECODE_011203,
                 BaseDllConst.CONST_OFFICECODE_011402
                fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_OTRCV7.FTP"

        End Select

        Return fileName
    End Function

    ''' <summary>
    ''' ファイル社外連携の各種出力ファイルの出力可否判定
    ''' </summary>
    Public Class FileLinkagePattern
        Private _Item As Dictionary(Of String, FileLinkagePatternItem)
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me._Item = New Dictionary(Of String, FileLinkagePatternItem)
            Dim fileLinkageItem As FileLinkagePatternItem
            With Me._Item
                Dim outFieldList As Dictionary(Of String, Integer)
                Dim outRequestFieldList As Dictionary(Of String, Integer) = Nothing
                '***************************
                '仙台新港営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "010402", True, False, False
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '五井営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011201", True, True, True
                    )
                outFieldList = New Dictionary(Of String, Integer)
                With outFieldList
                    .Add("SEQ_DATATYPE_RESERVED", 2)
                    .Add("SEQ_PROC_KBN", 1)
                    .Add("LODDATE_WITHOUT_SLASH", 8)
                    .Add("SEQ_DEPT_CODE", 2)
                    .Add("OUTPUTRESERVENO", 3)
                    .Add("SEQ_TORIKBN", 2)
                    .Add("SEQ_TOKUISAKI", 5)
                    .Add("CONSIGNEECONVCODE", 6)
                    .Add("ARRDATE_WITHOUT_SLASH", 8)
                    .Add("SHIPPEROILCODE", 7)
                    .Add("SEQ_TAX_KBN", 1)
                    .Add("SEQ_NISCODE", 3)
                    .Add("SEQ_UKEHARAI_CODE", 4)
                    .Add("SEQ_ORDERAMOUNT", 5)
                    .Add("SEQ_TRANSWAY", 1)
                    .Add("SEQ_GYOUSYACODE", 3)
                    .Add("SEQ_TANKNO", 6)
                    .Add("SEQ_KAIJI", 1)
                    .Add("SEQ_DEN_NO", 5)
                    .Add("SEQ_DEN_MEI_NO", 1)
                    .Add("SEQ_ACCTUAL_AMOUNT", 5)
                    .Add("SEQ_NIYAKU_BEGIN_TIME", 4)
                    .Add("SEQ_NIYAKU_END_TIME", 4)
                    .Add("SEQ_TRAINNO", 4)
                    .Add("SEQ_TOKUISAKI_KANA", 8)
                    .Add("SEQ_HAISOU_KANA", 8)
                    .Add("SEQ_HINMEI_KANA", 10)
                    .Add("SEQ_TAXKBN_KANA", 1)
                    .Add("SEQ_CREATEDATETIME", 12)
                    .Add("SEQ_PLANTCODE", 3)
                    .Add("SEQ_SHIPPERCODE", 3)
                    .Add("SEQ_CONSIGNEECODE", 6)
                    .Add("SEQ_YOBI", 109)
                End With
                fileLinkageItem.OutputFiledList = outFieldList
                fileLinkageItem.OutputReservedConstantField = False
                fileLinkageItem.OutputReservedFileNameWithoutExtention = "COSSO"
                fileLinkageItem.OutputReservedFileExtention = "SEQ"
                fileLinkageItem.ReservedOutputType = FileLinkagePatternItem.ReserveOutputFileType.Seq
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '甲子営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011202", True, True, True
                    )
                outFieldList = New Dictionary(Of String, Integer)
                outFieldList.Add("KINO_DATAKBN", 0)
                outFieldList.Add("LODDATE_WITHOUT_SLASH", 0)
                outFieldList.Add("OUTPUTRESERVENO", 0)
                outFieldList.Add("KINO_TRAINNO", 0)
                outFieldList.Add("REPORTOILNAME", 0)
                outFieldList.Add("RESERVEDQUANTITY", 0)
                outFieldList.Add("KINO_TOKUISAKICODE", 0)
                outFieldList.Add("KINO_TOKUISAKINAME", 0)
                outFieldList.Add("CONSIGNEECONVCODE", 0)
                outFieldList.Add("CONSIGNEECONVVALUE", 0)
                fileLinkageItem.OutputFiledList = outFieldList
                fileLinkageItem.OutputReservedConstantField = False
                fileLinkageItem.OutputReservedFileNameWithoutExtention = "SE183"
                fileLinkageItem.OutputReservedFileExtention = "CSV"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '袖ヶ浦営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011203", True, True, True
                    )
                outFieldList = New Dictionary(Of String, Integer)
                outFieldList.Add("SOD_STATUS", 0)
                outFieldList.Add("SOD_SHELL_ORDERNO", 0)
                outFieldList.Add("OUTPUTRESERVENO", 0)
                outFieldList.Add("LODDATE", 0)
                outFieldList.Add("SOD_TRANS_KBN", 0)
                outFieldList.Add("SHIPPERCONVCODE", 0)
                outFieldList.Add("SHIPPERCONVVALUE", 0)
                outFieldList.Add("CONSIGNEECONVCODE", 0)
                outFieldList.Add("CONSIGNEECONVVALUE", 0)
                outFieldList.Add("SOD_SHIPPEROILCODE", 0)
                outFieldList.Add("REPORTOILNAME", 0)
                outFieldList.Add("SOD_TAX_KBN", 0)
                outFieldList.Add("SOD_RESERVEDQUANTITY", 0)
                outFieldList.Add("SOD_TRANS_COMP", 0)
                outFieldList.Add("OLDTANKNUMBER", 0)
                outFieldList.Add("TRANSNAME", 0)
                outFieldList.Add("SOD_BACKNAME", 0)
                fileLinkageItem.OutputFiledList = outFieldList
                fileLinkageItem.OutputReservedConstantField = False
                fileLinkageItem.OutputReservedFileNameWithoutExtention = "富士石油貨車出荷データ"
                fileLinkageItem.OutputReservedFileExtention = "xlsx"
                fileLinkageItem.OutputReservedExcelDataStartAddress = "B4"
                fileLinkageItem.ReservedOutputType = FileLinkagePatternItem.ReserveOutputFileType.Excel2007
                'ヘッダー必要なら↓のコメントOFF
                fileLinkageItem.OutputReservedCustomOutputFiledHeader = "ステータス,SHELL受注番号,JOT受注番号,出荷日付,輸送方法,送荷先コード,送荷先,納入先コード,納入先,品名コード,品名,課税区分,実績数量,運送会社,輸送機関,便名,戻し"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '根岸営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011402", True, True, False
                    )
                outFieldList = New Dictionary(Of String, Integer)
                outFieldList.Add("LODDATE_WITHOUT_SLASH", 0)
                outFieldList.Add("OUTPUTRESERVENO", 0)
                outFieldList.Add("NEG_TUMIKOMI_KAI", 0)
                outFieldList.Add("TRAINNO_PAD_ZERO", 0)
                outFieldList.Add("NEG_TUMIKOMI_POINT", 0)
                outFieldList.Add("NEG_KASHANO", 0)
                outFieldList.Add("NEG_SHIPPEROILCODE", 0)
                outFieldList.Add("NEG_SETTEI_NUM", 0)
                outFieldList.Add("CONSIGNEECONVCODE", 0)
                outFieldList.Add("NEG_ARM_CODE", 0)
                outFieldList.Add("NEG_TSUMI_NUM", 0)
                fileLinkageItem.OutputFiledList = outFieldList
                fileLinkageItem.OutputReservedConstantField = False
                fileLinkageItem.OutputReservedFileNameWithoutExtention = "YOYAKU"
                fileLinkageItem.OutputReservedFileExtention = "CSV"
                fileLinkageItem.OutputReservedCustomOutputFiledHeader = "積込日,予約番号,積込回線,列車番号,積込ポイント,貨車番号,油種コード,設定数量,向先コード,アーム番号,積込数量"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '四日市営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "012401", True, True, True
                    )
                outFieldList = New Dictionary(Of String, Integer)
                With outFieldList
                    .Add("SEQ_DATATYPE_RESERVED", 2)
                    .Add("SEQ_PROC_KBN", 1)
                    .Add("LODDATE_WITHOUT_SLASH", 8)
                    .Add("SEQ_DEPT_CODE", 2)
                    .Add("OUTPUTRESERVENO", 3)
                    .Add("SEQ_TORIKBN", 2)
                    .Add("SEQ_TOKUISAKI", 5)
                    .Add("CONSIGNEECONVCODE", 6)
                    .Add("ARRDATE_WITHOUT_SLASH", 8)
                    .Add("SHIPPEROILCODE", 7)
                    .Add("SEQ_TAX_KBN", 1)
                    .Add("SEQ_NISCODE", 3)
                    .Add("SEQ_UKEHARAI_CODE", 4)
                    .Add("SEQ_ORDERAMOUNT", 5)
                    .Add("SEQ_TRANSWAY", 1)
                    .Add("SEQ_GYOUSYACODE", 3)
                    .Add("SEQ_TANKNO", 6)
                    .Add("SEQ_KAIJI", 1)
                    .Add("SEQ_DEN_NO", 5)
                    .Add("SEQ_DEN_MEI_NO", 1)
                    .Add("SEQ_ACCTUAL_AMOUNT", 5)
                    .Add("SEQ_NIYAKU_BEGIN_TIME", 4)
                    .Add("SEQ_NIYAKU_END_TIME", 4)
                    .Add("SEQ_TRAINNO", 4)
                    .Add("SEQ_TOKUISAKI_KANA", 8)
                    .Add("SEQ_HAISOU_KANA", 8)
                    .Add("SEQ_HINMEI_KANA", 10)
                    .Add("SEQ_TAXKBN_KANA", 1)
                    .Add("SEQ_CREATEDATETIME", 12)
                    .Add("SEQ_PLANTCODE", 3)
                    .Add("SEQ_SHIPPERCODE", 3)
                    .Add("SEQ_CONSIGNEECODE", 6)
                    .Add("SEQ_YOBI", 109)
                End With
                fileLinkageItem.OutputFiledList = outFieldList
                fileLinkageItem.OutputReservedConstantField = False
                fileLinkageItem.OutputReservedFileNameWithoutExtention = "COSSO"
                fileLinkageItem.OutputReservedFileExtention = "SEQ"
                fileLinkageItem.ReservedOutputType = FileLinkagePatternItem.ReserveOutputFileType.Seq
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '三重塩浜営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "012402", False, False, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
            End With
        End Sub
        ''' <summary>
        ''' デフォルトプロパティ
        ''' </summary>
        ''' <param name="officeCode">営業所コード</param>
        ''' <returns>表示パターンクラス</returns>
        Default Public ReadOnly Property Item(officeCode As String) As FileLinkagePatternItem
            Get
                If Me._Item.ContainsKey(officeCode) Then
                    Return Me._Item(officeCode)
                Else
                    '設定が存在しない場合は全てボタン非表示
                    Return New FileLinkagePatternItem(officeCode, False, False, False)
                End If

            End Get

        End Property

    End Class


    ''' <summary>
    ''' 外部連携パターンアイテムクラス
    ''' </summary>
    Public Class FileLinkagePatternItem
        ''' <summary>
        ''' 出荷予約出力フォーマット列挙
        ''' </summary>
        Public Enum ReserveOutputFileType
            Csv = 1
            Excel2007 = 2 '4文字拡張子
            Excel2003 = 4 '3文字拡張子（これはマクロが入るからやらない想定？）
            Pdf = 8       'Pdf（これは絶対に無い想定？）PDF作ってメール送信あるかも？
            Seq = 16
        End Enum

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="officeCode"></param>
        Public Sub New(officeCode As String)
            Me.New(officeCode, False, False, False)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="officeCode">営業所コード</param>
        ''' <param name="canOtSend">OT発送日報出力可否(True:可,False:不可)</param>
        ''' <param name="canReserved">製油所出荷予約出力可否(True:可,False:不可)</param>
        ''' <param name="canTakusou">託送指示出力可否(True:可,False:不可)</param>
        Public Sub New(officeCode As String, canOtSend As Boolean, canReserved As Boolean, canTakusou As Boolean)
            Me.OfficeCode = officeCode
            Me.CanOtSend = canOtSend
            Me.CanReserved = canReserved
            Me.CanTakusou = canTakusou
        End Sub

        ''' <summary>
        ''' 営業所コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String
        ''' <summary>
        ''' OT発送日報出力可否(True:可,False:不可)
        ''' </summary>
        ''' <returns></returns>
        Public Property CanOtSend As Boolean = False
        ''' <summary>
        ''' 製油所出荷予約出力可否(True:可,False:不可)
        ''' </summary>
        ''' <returns></returns>
        Public Property CanReserved As Boolean = False
        ''' <summary>
        ''' 託送指示出力可否(True:可,False:不可)
        ''' </summary>
        ''' <returns></returns>
        Public Property CanTakusou As Boolean = False

        ''' <summary>
        ''' 出荷予約出力フォーマット
        ''' </summary>
        ''' <returns></returns>
        Public Property ReservedOutputType As ReserveOutputFileType = ReserveOutputFileType.Csv
        ''' <summary> 
        ''' 出力フィールドリスト(フィールド名、固定長用フィールドサイズ）
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputFiledList As Dictionary(Of String, Integer)
        ''' <summary>
        ''' シーケンスファイル出力の実績要求ファイルのフィールドリスト（フィールド名、固定長用フィールドサイズ）
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputRequestFieldList As Dictionary(Of String, Integer)
        ''' <summary>
        ''' 出荷予約にてフィールドサイズ（OutputFiledList）で設定したサイズで出力する場合True、デフォルトはFalse
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputReservedConstantField As Boolean = False
        Public Property OutputReservedFileNameWithoutExtention As String
        Public Property OutputReservedFileExtention As String
        Public Property OutputReservedCustomOutputFiledHeader As String
        Public Property OutputReservedExcelDataStartAddress As String = ""
    End Class
    ''' <summary>
    ''' 出力したオーダーのキー情報を保持する為のクラス
    ''' </summary>
    Public Class OutputOrdedrInfo
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(orderNo As String, detailNo As String)
            Me.OrderNo = orderNo
            Me.DetailNo = detailNo
        End Sub
        ''' <summary>
        ''' オーダー番号
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String
        ''' <summary>
        ''' 明細番号
        ''' </summary>
        ''' <returns></returns>
        Public Property DetailNo As String
        ''' <summary>
        ''' 予約番号（連番部分のみ）
        ''' </summary>
        ''' <returns></returns>
        Public Property ReservedNo As String
    End Class
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

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex.ToString
            WW_SelectValue = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(CInt(WF_SelectedIndex.Value)).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_FILTERDATE"
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE <CDate(C_DEFAULT_YMD) Then
                        WF_FILTERDATE_TEXT.Text = ""
                    Else
                        WF_FILTERDATE_TEXT.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                WF_FILTERDATE_TEXT.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        'WF_LeftMViewChange.Value = ""  '★

    End Sub


    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_FILTERDATE"
                Me.WF_FILTERDATE_TEXT.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        'WF_LeftMViewChange.Value = ""

    End Sub
End Class
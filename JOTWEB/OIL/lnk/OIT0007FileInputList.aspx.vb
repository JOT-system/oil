Option Strict On
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox
''' <summary>
''' 社外連携取込一覧画面クラス
''' </summary>
Public Class OIT0007FileInputList
    Inherits System.Web.UI.Page
    '○ 検索結果格納Table
    Private OIT0007tbl As DataTable                                 '一覧格納用テーブル
    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID
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
    ''' アップロード許可拡張子"ext1", "ext2"と営業所に応じ定義 
    ''' </summary>
    ''' <returns></returns>
    Public Property AcceptExtentions As String = ""
    Public Property ShowUpdConfirm As String = ""
    ''' <summary>
    ''' ページロード時
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                If Me.hdnUpdateConfirmIsActive.Value <> "1" Then
                    Me.ShowUpdConfirm = ""
                    Me.repUpdateList.DataSource = Nothing
                    Me.repUpdateList.DataBind()
                    ViewState("VS_OUTPUTINFO") = Nothing
                Else
                    ShowUpdConfirm = "showUpdConfirm"
                End If

                '添付ファイルアップロード処理
                If Me.WF_FILENAMELIST.Value <> "" Then
                    'Dim retMes = UploadAttachments()
                    'If retMes.MessageNo <> C_MESSAGE_NO.NORMAL Then
                    '    Master.Output(retMes.MessageNo, C_MESSAGE_TYPE.ERR, retMes.Pram01, needsPopUp:=True)
                    'End If
                    Dim outPutInfo = FileUploaded()
                    outPutInfo = GetOrderUpdateOrderInfo(outPutInfo)

                    Me.WF_FILENAMELIST.Value = ""
                    '○ 一覧再表示処理
                    Master.RecoverTable(OIT0007tbl)
                    DisplayGrid()
                    'TODO outPutInfoの数が0なら対象外ファイルを上げたと想定
                    If outPutInfo Is Nothing OrElse outPutInfo.Count = 0 Then
                        Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ERR, "正しい出荷実績ファイルをアップロードしてください。", needsPopUp:=True)
                        Return
                    End If
                    '更新確認に値を設定
                    Me.repUpdateList.DataSource = outPutInfo
                    Me.repUpdateList.DataBind()
                    ViewState("VS_OUTPUTINFO") = outPutInfo
                    Me.hdnUpdateConfirmIsActive.Value = "1"
                    ShowUpdConfirm = "showUpdConfirm"
                End If
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0007tbl)

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
                        Case "WF_ButtonUpadteAmount"    '(更新ボタン押下時)
                            WF_ButtonUpadteAmount_Click()

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
            If Not IsNothing(OIT0007tbl) Then
                OIT0007tbl.Clear()
                OIT0007tbl.Dispose()
                OIT0007tbl = Nothing
            End If
            Dim flp As New FileLinkagePattern
            Dim settings = flp(work.WF_SEL_SALESOFFICECODE.Text)
            Dim inputExt As String = settings.InputExtention
            If inputExt = "xlsx" Then
                inputExt = inputExt & ",xls"
            End If
            Dim ext = (From itm In inputExt.Split(","c) Select """" & itm & """")
            If ext.Any Then
                Me.AcceptExtentions = String.Join(",", ext)
            End If

        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0007WRKINC.MAPIDL
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = False
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
        Dim settings = flp(work.WF_SEL_SALESOFFICECODE.Text)
        '営業所に応じ表示非表示を行う
        'OT発送日報
        btnFileUpload.Visible = settings.CanUpload
        '幅調整の為ボタンの数量で
        Dim cssVal = Me.Form.Attributes("class")
        Dim btnCnt As Integer = If(settings.CanUpload, 1, 0)
        cssVal = cssVal & " btnCnt" & btnCnt
        Me.Form.Attributes("class") = cssVal
        '表示するデータが無ければ各種ボタンは非活性

        If OIT0007tbl Is Nothing OrElse OIT0007tbl.Rows.Count = 0 Then
            btnFileUpload.Disabled = True
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0007L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
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
        SetFilterValue(OIT0007tbl, chkField, Me.WF_FILTERDATE_TEXT.Text)
        '○ 表示対象行カウント(絞り込み対象)
        Dim WW_DataCNT As Integer = 0
        For Each OIT0007row As DataRow In OIT0007tbl.Rows
            If CInt(OIT0007row("HIDDEN")) = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0007row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0007tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0007tbl)

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

        If IsNothing(OIT0007tbl) Then
            OIT0007tbl = New DataTable
        End If

        If OIT0007tbl.Columns.Count <> 0 Then
            OIT0007tbl.Columns.Clear()
        End If

        OIT0007tbl.Clear()

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
                PARA05.Value = work.WF_SEL_SALESOFFICECODE.Text
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
                    OIT0007tbl = (From dr As DataRow In dtWrk Order By dr("LODDATE")).CopyToDataTable
                Else
                    OIT0007tbl = dtWrk
                End If

                Dim i As Integer = 0
                For Each OIT0007row As DataRow In OIT0007tbl.Rows
                    i += 1
                    OIT0007row("LINECNT") = i        'LINECNT
                    '************************************
                    '以下各行の３帳票の出力可否状態を設定
                    '************************************
                    'OT発送日報出力可否(発日 >= 当日)
                    If Convert.ToString(OIT0007row("DEPDATE")) >= today Then
                        OIT0007row("CAN_OTSEND") = "1"
                    Else
                        OIT0007row("CAN_OTSEND") = "0"
                    End If
                    '出荷予約出力可否(積日 >= 翌日)
                    If Convert.ToString(OIT0007row("LODDATE")) >= today Then
                        OIT0007row("CAN_RESERVED") = "1"
                    Else
                        OIT0007row("CAN_RESERVED") = "0"
                    End If
                    '託送指示出力可否(発日 >= 翌日)
                    If Convert.ToString(OIT0007row("DEPDATE")) >= targetDate Then
                        OIT0007row("CAN_TAKUSOU") = "1"
                    Else
                        OIT0007row("CAN_TAKUSOU") = "0"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L SELECT"
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
        Master.RecoverTable(OIT0007tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0007tbl.Rows.Count - 1
            If Convert.ToString(OIT0007tbl.Rows(i)("LINECNT")) = WF_SelectedIndex.Value Then
                If Convert.ToString(OIT0007tbl.Rows(i)("OPERATION")) = "" Then
                    OIT0007tbl.Rows(i)("OPERATION") = "on"
                Else
                    OIT0007tbl.Rows(i)("OPERATION") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0007tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0007tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0007tbl.Rows.Count - 1
            If Convert.ToString(OIT0007tbl.Rows(i)("HIDDEN")) = "0" Then
                OIT0007tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0007tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0007tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0007tbl.Rows.Count - 1
            If Convert.ToString(OIT0007tbl.Rows(i)("HIDDEN")) = "0" Then
                OIT0007tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0007tbl)

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
        Master.RecoverTable(OIT0007tbl)
        '表示行制御実行
        OIT0007tbl = SetFilterValue(OIT0007tbl, chkField, dataVal)
        '○ 画面表示データ保存
        Master.SaveTable(OIT0007tbl)
    End Sub
    ''' <summary>
    ''' ポップアップの更新ボタン押下時
    ''' </summary>
    Protected Sub WF_ButtonUpadteAmount_Click()
        Dim updList = CollectScreenValue()
        'チェックボックスの値が１件もない場合
        Dim qChkItm = From itm In updList Where itm.InputCheck = True
        '１件もチェックがない場合は終了
        If qChkItm.Any = False Then
            Master.Output(C_MESSAGE_NO.SELECT_DETAIL_ERROR, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
            Me.repUpdateList.DataSource = updList
            Me.repUpdateList.DataBind()
            Return
        End If
        Dim chkItems = qChkItm.ToList
        Try
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                SqlConnection.ClearPool(SQLcon)
                Dim procDate As Date = Now
                Dim resProc As Boolean = False
                Using sqlTran As SqlTransaction = SQLcon.BeginTransaction
                    '数量更新
                    UpdateOrderDetailAmount(chkItems, SQLcon, sqlTran, procDate)
                    Dim historyNo As String = GetNewOrderHistoryNo(SQLcon, sqlTran)
                    If historyNo = "" Then
                        Return
                    End If
                    '履歴テーブル登録
                    Dim orderTbl As DataTable = GetUpdatedOrder(chkItems, SQLcon, sqlTran)
                    Dim detailTbl As DataTable = GetUpdatedOrderDetail(chkItems, SQLcon, sqlTran)
                    If detailTbl IsNot Nothing Then
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
                        OutputJournal(detailTbl, "OIT0003_DETAIL")
                    End If
                    'ここまで来たらコミット
                    sqlTran.Commit()
                End Using
            End Using
            Me.hdnUpdateConfirmIsActive.Value = ""
            Me.ShowUpdConfirm = ""
            Me.repUpdateList.DataSource = Nothing
            Me.repUpdateList.DataBind()
            ViewState("VS_OUTPUTINFO") = Nothing
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            Return
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, Me.Title)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "WF_ButtonUpadteAmount_Click"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
    End Sub
    ''' <summary>
    ''' 画面上のチェックボックスの値を収集しリストの一覧データを返却
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectScreenValue() As List(Of InputDataItem)
        Dim retValue As New List(Of InputDataItem)
        retValue = DirectCast(ViewState("VS_OUTPUTINFO"), List(Of InputDataItem))
        'そもそも一覧表示が無ければこれ以降の処理の意味が無い為終了
        If retValue Is Nothing OrElse retValue.Count = 0 Then
            Return retValue
        End If
        Dim hdnIdxObj As HiddenField = Nothing
        Dim checkBoxObj As CheckBox = Nothing
        For Each repItm As RepeaterItem In repUpdateList.Items
            hdnIdxObj = DirectCast(repItm.FindControl("hdnUpdIndex"), HiddenField)
            checkBoxObj = DirectCast(repItm.FindControl("chkUpdate"), CheckBox)
            If hdnIdxObj Is Nothing OrElse checkBoxObj Is Nothing Then
                Continue For
            End If
            If IsNumeric(hdnIdxObj.Value) = False Then
                Continue For
            End If
            Dim idx As Integer = CInt(hdnIdxObj.Value)
            With retValue.Item(idx)
                .InputCheck = checkBoxObj.Checked
            End With
        Next
        Return retValue
    End Function
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
                        Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L ORDER_HISTORYNOGET")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "DB:OIT0007L ORDER_HISTORYNOGET"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "履歴番号の取得に失敗"
                        CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                        CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
                        retVal = ""
                    End If
                End Using 'sqlDr
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L ORDER_HISTORYNOGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L ORDER_HISTORYNOGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
        Return retVal
    End Function

    ''' <summary>
    ''' 受注明細の実績値を更新
    ''' </summary>
    ''' <param name="uploadOrderInfo">出力した受注キー情報</param>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="sqlTran">トランザクション</param>
    Private Function UpdateOrderDetailAmount(uploadOrderInfo As List(Of InputDataItem), sqlCon As SqlConnection, sqlTran As SqlTransaction, Optional procDate As Date = #1900/1/1#) As Boolean
        Try

            Dim sqlStat As StringBuilder
            If procDate = #1900/1/1# Then
                procDate = Now
            End If

            sqlStat = New StringBuilder
            sqlStat.AppendLine("UPDATE OIL.OIT0003_DETAIL")
            sqlStat.AppendLine("   SET  CARSAMOUNT = @CARSAMOUNT")
            sqlStat.AppendLine("       ,UPDYMD     = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER    = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID  = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD = @RECEIVEYMD")
            sqlStat.AppendLine(" WHERE ORDERNO  = @ORDERNO")
            sqlStat.AppendLine("   AND DETAILNO = @DETAILNO")
            sqlStat.AppendLine("   AND DELFLG   = @DELFLG") 'ここまで来て削除フラグ1はありえないが念の為

            For Each orderKey In uploadOrderInfo
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)

                    With sqlCmd.Parameters
                        .Add("@CARSAMOUNT", SqlDbType.Decimal).Value = CDec(orderKey.InpCarsAmount)
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L UPDATE_ORDERDETAIL", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L UPDATE_ORDERDETAIL"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L GETOUTPUTFLG", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L GETOUTPUTFLG"
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
            Dim checkedRow As DataTable = (From dr As DataRow In OIT0007tbl Where Convert.ToString(dr("OPERATION")) <> "").CopyToDataTable

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L UPDATE_ORDER_UPLOADFLAG", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L UPDATE_ORDER_UPLOADFLAG"
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
    Private Function GetUpdatedOrderDetail(uploadOrderInfo As List(Of InputDataItem), sqlCon As SqlConnection, sqlTran As SqlTransaction) As DataTable
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L GET_UPDATED_ORDERDETAIL")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L GET_UPDATED_ORDERDETAIL"
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
    Private Function GetUpdatedOrder(uploadOrderInfo As List(Of InputDataItem), sqlCon As SqlConnection, sqlTran As SqlTransaction) As DataTable
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0007L GET_UPDATED_ORDER")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L GET_UPDATED_ORDER"
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
        For Each OIT0007row As DataRow In OIT0007tbl.Rows
            If CInt(OIT0007row("HIDDEN")) = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0007row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0007tbl)

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
    ''' ファイルアップロード時イベント
    ''' </summary>
    Private Function FileUploaded() As List(Of InputDataItem)
        Dim retItem As New List(Of InputDataItem)
        Dim uploadFiles As List(Of AttachmentFile)
        Dim tp As Type = GetType(List(Of AttachmentFile))
        Dim serializer As New Runtime.Serialization.Json.DataContractJsonSerializer(tp)
        Dim flp As New FileLinkagePattern
        Dim settings = flp(work.WF_SEL_SALESOFFICECODE.Text)
        Try
            'アップロードワークフォルダ
            Dim uploadWorkDir = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, "UPLOAD_TMP", CS0050SESSION.USERID)
            If Not IO.Directory.Exists(uploadWorkDir) Then
                Return Nothing
            End If

            Using stream As New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Me.WF_FILENAMELIST.Value))
                uploadFiles = DirectCast(serializer.ReadObject(stream), List(Of AttachmentFile))
            End Using
            Dim uploadFilePath As String = IO.Path.Combine(uploadWorkDir, uploadFiles(0).FileName)
            If {"xls", "xlsx"}.Contains(settings.InputExtention) Then
                Using excelReadObj As New OIT0007InputExcel(settings, uploadFilePath)
                    retItem = excelReadObj.ReadExcel()
                End Using
            Else
                Using csvReadObj As New OIT0007InputCsv(settings, uploadFilePath)
                    retItem = csvReadObj.ReadCsv()
                End Using
            End If
        Catch ex As Exception
            Return Nothing
        End Try
        Return retItem
    End Function
    ''' <summary>
    ''' オーダー情報付与
    ''' </summary>
    ''' <param name="inpData"></param>
    ''' <returns></returns>
    Private Function GetOrderUpdateOrderInfo(inpData As List(Of InputDataItem)) As List(Of InputDataItem)
        If inpData Is Nothing OrElse inpData.Count = 0 Then
            '0件の場合はやる意味がないので終了
            Return inpData
        End If

        Dim retVal = inpData
        '*********************************************
        '予約番号、積込予定日を元に受注データを取得
        '*********************************************
        Dim dt = ReservedDataGet(retVal)
        '*********************************************
        'アップロードデータに受注情報を付与
        '*********************************************
        For Each retItm In retVal
            '予約番号未設定
            If retItm.ReservedNo = "" Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.NoReservedNo
                Continue For
            End If
            '抽出結果を検索
            If dt Is Nothing Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.NoOrderInfo
                Continue For
            End If
            '↓2021/03 --- 並行稼働用の予約番号マッチングを行わないようにするため暫定的にコメント
            'Dim query = (From dr As DataRow In dt Where dr("RESERVEDNO").Equals(retItm.ReservedNo) AndAlso dr("LODDATE").Equals(retItm.LodDate))
            '↑2021/03 --- 並行稼働用の予約番号マッチングを行わないようにするため暫定的にコメント

            '↓2021/03 --- 並行稼働用の予約番号マッチング
            Dim fieldSettings = (From itm In {
                                     New With {Key .office = "011201", .oilField = "SHIPPEROILCODE", .tankField = "SEQ_TANKNO"},
                                     New With {Key .office = "012401", .oilField = "SHIPPEROILCODE", .tankField = "SEQ_TANKNO"},
                                     New With {Key .office = "011202", .oilField = "REPORTOILNAME", .tankField = "KINO_TRAINNO"},
                                     New With {Key .office = "011203", .oilField = "REPORTOILNAME", .tankField = "OLDTANKNUMBER"},
                                     New With {Key .office = "011402", .oilField = "NEG_SHIPPEROILCODE", .tankField = "NEG_KASHANO"}
                                     }).ToDictionary(Function(x) x.office, Function(x) x)
            Dim fieldSetting = fieldSettings(work.WF_SEL_SALESOFFICECODE.Text)
            Dim query = (From dr As DataRow In dt Where dr(fieldSetting.oilField).Equals(retItm.InpOilTypeName) AndAlso dr(fieldSetting.tankField).Equals(retItm.InpTnkNo) AndAlso dr("LODDATE").Equals(retItm.LodDate))
            '↑2021/03 --- 並行稼働用の予約番号マッチング
            If query.Any = False Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.NoOrderInfo
                Continue For
            End If
            Dim targetRows = query.ToList
            'ありえないが同一予約番号、積込予定日で複数件マッチした場合
            If targetRows.Count > 1 Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.TooMenyOrderInfo
                '念のためカンマ区切りで受注番号、明細番号保持
                retItm.OrderNo = String.Join(",", From dr In targetRows Select Convert.ToString(dr("ORDERNO")))
                retItm.DetailNo = String.Join(",", From dr In targetRows Select Convert.ToString(dr("DETAILNO")))
                Continue For
            End If
            Dim targetRow = targetRows(0)
            'ここから値転送
            retItm.DbReservedNo = Convert.ToString(targetRow("RESERVEDNO"))
            retItm.OrderNo = Convert.ToString(targetRow("ORDERNO"))
            retItm.DetailNo = Convert.ToString(targetRow("DETAILNO"))
            retItm.CarsAmount = Convert.ToString(targetRow("CARSAMOUNT"))
            retItm.CarsAmount = Convert.ToString(targetRow("CARSAMOUNT"))
            retItm.OilName = Convert.ToString(targetRow("OILNAME"))
            retItm.TankNo = Convert.ToString(targetRow("TANKNO"))
            retItm.DbLodDate = Convert.ToString(targetRow("LODDATE"))
            retItm.DepDate = Convert.ToString(targetRow("DEPDATE"))
            retItm.TrainNo = Convert.ToString(targetRow("TRAINNO"))
            retItm.OrderStatus = Convert.ToString(targetRow("ORDERSTATUS_NAME"))
            '受注情報とファイルを比較するため営業所により異なる設定の油種、及び貨車番号を取得
            Dim oilName As String = ""
            Dim tankNo As String = ""
            Select Case work.WF_SEL_SALESOFFICECODE.Text
                Case "011202"
                    '甲子
                    oilName = Convert.ToString(targetRow("REPORTOILNAME"))
                    tankNo = Convert.ToString(targetRow("KINO_TRAINNO"))
                Case "011402"
                    '根岸
                    oilName = Convert.ToString(targetRow("NEG_SHIPPEROILCODE"))
                    tankNo = Convert.ToString(targetRow("NEG_KASHANO"))
                Case "011203"
                    '袖ヶ浦
                    oilName = Convert.ToString(targetRow("REPORTOILNAME"))
                    tankNo = Convert.ToString(targetRow("OLDTANKNUMBER"))
                Case "011201", "012401"
                    '五井、四日市
                    oilName = Convert.ToString(targetRow("SHIPPEROILCODE"))
                    tankNo = Convert.ToString(targetRow("SEQ_TANKNO"))
            End Select

            '油種不一致チェック
            '取り込んだファイルと比較不一致なら油種不一致
            If Not retItm.InpOilTypeName = oilName Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.OilUnMatch
                Continue For
            End If
            '車番チェック
            If Not retItm.InpTnkNo = tankNo Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.TankUnmatch
                Continue For
            End If
            '受注ステータスチェック(200：手配～310：手配完了の間であること）
            If Not (Convert.ToString(targetRow("ORDERSTATUS")) >= BaseDllConst.CONST_ORDERSTATUS_200 AndAlso
                    Convert.ToString(targetRow("ORDERSTATUS")) <= BaseDllConst.CONST_ORDERSTATUS_310) Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.OrderStatusCannotAccept
                If Convert.ToString(targetRow("ORDERSTATUS")) = BaseDllConst.CONST_ORDERSTATUS_900 Then
                    retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.OrderStatusCancel
                ElseIf Convert.ToString(targetRow("ORDERSTATUS")) < BaseDllConst.CONST_ORDERSTATUS_200 Then
                    retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.OrderStatusBackToBefore200
                End If
                Continue For
            End If
            '出荷実績0チェック
            If IsNumeric(retItm.InpCarsAmount) AndAlso CDec(retItm.InpCarsAmount) = 0 Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.AmountZero
            End If
            'ここまで来てチェック状態が初期なら更新可能
            If retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.InitVal Then
                retItm.CheckReadonCode = InputDataItem.CheckReasonCodes.OK
                retItm.InputCheck = True
            End If
        Next
        Return retVal
    End Function
    ''' <summary>
    ''' 出荷予約データを取得
    ''' </summary>
    ''' <returns>処理対象の受注Noと明細No</returns>
    ''' <remarks>このロジックにたどりつけるのは積置無しのみ、積置を許容するなら要修正</remarks>
    Private Function ReservedDataGet(inpData As List(Of InputDataItem)) As DataTable
        '当処理の抽出結果の全フィールドを帳票に出すわけではない
        Dim retDt As New DataTable
        Dim retVal As New List(Of OutputOrdedrInfo)

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT ODR.ORDERNO")            'キー情報
        sqlStat.AppendLine("     , DET.DETAILNO")           'キー情報
        sqlStat.AppendLine("     , ODR.OFFICECODE AS OFFICECODE")     '営業所コード
        sqlStat.AppendLine("     , format(ODR.LODDATE,'yyyy/MM/dd') AS LODDATE")     '積込日
        sqlStat.AppendLine("     , format(ODR.DEPDATE,'yyyy/MM/dd') AS DEPDATE")     '発日
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
        sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' THEN TNK.JXTGTANKNUMBER2 ELSE convert(nvarchar,convert(int,TNK.JXTGTANKNUMBER2)) END AS KINO_TRAINNO")
        sqlStat.AppendLine("     , ODR.SHIPPERSCODE")
        sqlStat.AppendLine("     , ODR.CONSIGNEECODE")
        sqlStat.AppendLine("     , CCNV.VALUE01 AS CONSIGNEECONVCODE")
        sqlStat.AppendLine("     , CCNV.VALUE02 AS CONSIGNEECONVVALUE")
        sqlStat.AppendLine("     , CCNV.VALUE03 AS TRANSNAME") '便名 現状袖ヶ浦のみ
        sqlStat.AppendLine("     , SCNV.VALUE01 AS SHIPPERCONVCODE")
        sqlStat.AppendLine("     , SCNV.VALUE02 AS SHIPPERCONVVALUE")

        'sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' AND convert(int,DET.TANKNO) between 1 and 999 THEN '1000-' + RIGHT('000' + DET.TANKNO,3) ")
        'sqlStat.AppendLine("            WHEN TNK.MODEL = 'タキ1000' AND convert(int,DET.TANKNO) >= 1000           THEN '1001-' + RIGHT(DET.TANKNO,3)  ")
        'sqlStat.AppendLine("            ELSE DET.TANKNO END AS NEG_KASHANO")
        sqlStat.AppendLine("     , TNK.JXTGTANKNUMBER4 AS NEG_KASHANO")

        sqlStat.AppendLine("     , RIGHT('00000' + convert(nvarchar,convert(int,PRD.SHIPPEROILCODE)),5) AS NEG_SHIPPEROILCODE")



        sqlStat.AppendLine("     , substring(isnull(PRD.SHIPPEROILCODE,''),1,5) + '0000' AS SOD_SHIPPEROILCODE") '袖ヶ浦輸送方法
        sqlStat.AppendLine("     , CASE WHEN PRD.MIDDLEOILCODE = '1' THEN '課税' ELSE 'その他' END AS SOD_TAX_KBN") '袖ヶ浦課税区分
        sqlStat.AppendLine("     , format(LRV.RESERVEDQUANTITY,'#0.000') AS SOD_RESERVEDQUANTITY")    '袖ヶ浦用_予約数量
        sqlStat.AppendLine("     , ODR.ORDERSTATUS")
        sqlStat.AppendLine("     , FXOST.VALUE1   AS ORDERSTATUS_NAME")
        sqlStat.AppendLine("     , DET.CARSAMOUNT ")
        sqlStat.AppendLine("     , PRD.OILNAME ")
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
        'sqlStat.AppendLine("     , CASE WHEN TNK.MODEL = 'タキ1000' OR TNK.MODEL = 'タキ43000'")
        'sqlStat.AppendLine("                 THEN RIGHT('000000' + TNK.TANKNUMBER,6)")
        'sqlStat.AppendLine("            WHEN TNK.MODEL = 'タキ243000'")
        'sqlStat.AppendLine("                 THEN RIGHT('000000' + STUFF(TNK.TANKNUMBER, 3, 1 ,''),6)")
        'sqlStat.AppendLine("            ELSE '000000'")
        'sqlStat.AppendLine("             END")
        'sqlStat.AppendLine("            AS SEQ_TANKNO") 'シーケンス業者コード
        'sqlStat.AppendLine("     , CASE WHEN TNK.LOAD = 44")
        'sqlStat.AppendLine("                 THEN RIGHT('000000' + STUFF(TNK.TANKNUMBER, 3, 1 ,''),6)")
        'sqlStat.AppendLine("            ELSE RIGHT('000000' + ISNULL(TNK.TANKNUMBER,''),6)")
        'sqlStat.AppendLine("             END")
        'sqlStat.AppendLine("            AS SEQ_TANKNO") 'シーケンス業者コード
        sqlStat.AppendLine("     , CASE WHEN TNK.TANKNUMBER = '143645' THEN '043645'")
        sqlStat.AppendLine("            WHEN TNK.LOAD = 44")
        sqlStat.AppendLine("                 THEN RIGHT('000000' + STUFF(TNK.TANKNUMBER, 3, 1 ,''),6)")
        sqlStat.AppendLine("            ELSE RIGHT('000000' + ISNULL(TNK.TANKNUMBER,''),6)")
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
        sqlStat.AppendLine("  FROM      OIL.OIT0002_ORDER  ODR WITH(nolock)")
        '明細結合ここから↓
        sqlStat.AppendLine(" INNER JOIN OIL.OIT0003_DETAIL DET WITH(nolock)")
        sqlStat.AppendLine("    ON ODR.ORDERNO =  DET.ORDERNO")
        sqlStat.AppendLine("   AND DET.DELFLG  = @DELFLG")
        '明細結合ここまで↑
        '油種マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0003_PRODUCT PRD WITH(nolock)")
        sqlStat.AppendLine("    ON PRD.OFFICECODE     = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND PRD.SHIPPERCODE    = ODR.SHIPPERSCODE")
        sqlStat.AppendLine("   AND PRD.PLANTCODE      = ODR.BASECODE")
        sqlStat.AppendLine("   AND PRD.OILCODE        = DET.OILCODE")
        sqlStat.AppendLine("   AND PRD.SEGMENTOILCODE = DET.ORDERINGTYPE")
        sqlStat.AppendLine("   AND PRD.DELFLG         = @DELFLG")
        '油種マスタ結合ここまで↑
        'タンク車マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0005_TANK TNK WITH(nolock)")
        sqlStat.AppendLine("    ON TNK.TANKNUMBER  = DET.TANKNO")
        sqlStat.AppendLine("   AND TNK.DELFLG      = @DELFLG")
        'タンク車マスタ結合ここまで↑
        '列車マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0007_TRAIN TRA WITH(nolock)")
        sqlStat.AppendLine("    ON TRA.OFFICECODE  = @OFFICECODE")
        sqlStat.AppendLine("   And TRA.TRAINNO     = ODR.TRAINNO")
        sqlStat.AppendLine("   And TRA.TSUMI       = CASE WHEN ODR.STACKINGFLG = '1' THEN 'T' ELSE 'N' END")
        sqlStat.AppendLine("   AND TRA.DEPSTATION  = ODR.DEPSTATION")
        sqlStat.AppendLine("   AND TRA.ARRSTATION  = ODR.ARRSTATION")
        sqlStat.AppendLine("   AND TRA.DEFAULTKBN  = 'def'")
        sqlStat.AppendLine("   AND TRA.DELFLG      = @DELFLG")
        '列車マスタ結合ここまで↑
        '積込予約マスタ結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0021_LOADRESERVE LRV WITH(nolock)")
        sqlStat.AppendLine("    ON LRV.OFFICECODE     = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND ODR.LODDATE        BETWEEN LRV.FROMYMD AND LRV.TOYMD")
        sqlStat.AppendLine("   AND LRV.LOAD           = TNK.LOAD")
        sqlStat.AppendLine("   AND LRV.OILCODE        = DET.OILCODE")
        sqlStat.AppendLine("   AND LRV.SEGMENTOILCODE = DET.ORDERINGTYPE")
        sqlStat.AppendLine("   AND LRV.DELFLG         = @DELFLG")
        '積込予約マスタ結合ここまで↑
        '変換マスタ（荷受人）結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0029_CONVERT CCNV WITH(nolock)")
        sqlStat.AppendLine("    ON CCNV.CLASS          = 'RESERVED_NIUKE'")
        sqlStat.AppendLine("   AND CCNV.KEYCODE01      = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND CCNV.KEYCODE02      = ODR.CONSIGNEECODE")
        sqlStat.AppendLine("   AND CCNV.DELFLG         = @DELFLG")
        '変換マスタ（荷受人）結合ここまで↑
        '変換マスタ（荷主）結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN OIL.OIM0029_CONVERT SCNV WITH(nolock)")
        sqlStat.AppendLine("    ON SCNV.CLASS          = 'RESERVED_SHIPPER'")
        sqlStat.AppendLine("   AND SCNV.KEYCODE01      = ODR.OFFICECODE")
        sqlStat.AppendLine("   AND SCNV.KEYCODE02      = ODR.SHIPPERSCODE")
        sqlStat.AppendLine("   AND SCNV.DELFLG         = @DELFLG")
        '変換マスタ（荷主）結合ここまで↑
        '固定値マスタ（受注ステータス）結合ここから↓
        sqlStat.AppendLine(" LEFT JOIN COM.OIS0015_FIXVALUE FXOST WITH(nolock)")
        sqlStat.AppendLine("    ON FXOST.CAMPCODE   = '01'")
        sqlStat.AppendLine("   AND FXOST.CLASS      = 'ORDERSTATUS'")
        sqlStat.AppendLine("   AND FXOST.KEYCODE    = ODR.ORDERSTATUS")
        sqlStat.AppendLine("   AND FXOST.DELFLG     = @DELFLG")
        '固定値マスタ（受注ステータス）結合ここまで↑
        sqlStat.AppendLine(" WHERE ODR.DELFLG       = @DELFLG")
        sqlStat.AppendLine("   AND ODR.OFFICECODE   = @OFFICECODE")
        sqlStat.AppendLine("   AND ( ")
        Dim isFirst As Boolean = True
        Dim hasCondition As Boolean = False
        For Each inpItem In inpData
            '予約番号や積込予定日がない事はありえないが念の為スキップ
            If inpItem.ReservedNo = "" OrElse inpItem.LodDate = "" OrElse
               IsDate(inpItem.LodDate) = False Then
                Continue For
            End If
            '↓2021/03 --- 並行稼働用の予約番号マッチングを行わないようにするため暫定的にコメント
            'If isFirst Then
            '    isFirst = False
            '    sqlStat.AppendFormat("             ( DET.RESERVEDNO = {0} AND ODR.LODDATE = '{1}')", inpItem.ReservedNo, inpItem.LodDate).AppendLine()
            'Else
            '    sqlStat.AppendFormat("         OR  ( DET.RESERVEDNO = {0} AND ODR.LODDATE = '{1}')", inpItem.ReservedNo, inpItem.LodDate).AppendLine()
            'End If
            '↑2021/03 --- 並行稼働用の予約番号マッチングを行わないようにするため暫定的にコメント
            '↓2021/03 --- 並行稼働用ロジック
            If isFirst Then
                isFirst = False
                '暫定で予約番号なしだとキャンセルは重複の恐れが高まるので条件に含めない
                sqlStat.AppendFormat("             ODR.LODDATE = '{0}'", inpItem.LodDate).AppendLine()
                sqlStat.AppendFormat("        AND ODR.ORDERSTATUS <> '{0}'", CONST_ORDERSTATUS_900).AppendLine()
            End If
            '↑2021/03 --- 並行稼働用ロジック
            hasCondition = True
        Next
        If hasCondition = False Then
            '予約番号、受注日の条件が全くないので不一致になる条件を付与
            sqlStat.AppendLine("'0'='1'")
        End If
        sqlStat.AppendLine("       )")

        Try
            '並び順は抽出後

            Using sqlCon = CS0050SESSION.getConnection _
                 , sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()       'DataBase接続
                SqlConnection.ClearPool(sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = C_DELETE_FLG.ALIVE
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = work.WF_SEL_SALESOFFICECODE.Text
                    '.Add("@ORDERSTATUS", SqlDbType.NVarChar).Value = BaseDllConst.CONST_ORDERSTATUS_310
                End With
                'SQL実行
                Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        retDt.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    retDt.Load(sqlDr)
                End Using

            End Using
            Return retDt
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003OTL RESERVED_DATAGET")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0007L RESERVED_DATAGET"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Return Nothing
        End Try
    End Function
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
                Dim inpFieldList As Dictionary(Of String, Integer)
                '***************************
                '仙台新港営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "010402", False
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '五井営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011201", True
                    )
                inpFieldList = New Dictionary(Of String, Integer)
                With inpFieldList
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
                fileLinkageItem.InputFiledList = inpFieldList
                fileLinkageItem.InputExtention = "seq"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '甲子営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011202", True
                    )

                fileLinkageItem.InputExtention = "csv"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '袖ヶ浦営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011203", True
                    )
                fileLinkageItem.InputExtention = "xlsx"
                'ヘッダー必要なら↓のコメントOFF
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '根岸営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "011402", True
                    )
                fileLinkageItem.InputExtention = "csv"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '四日市営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "012401", True
                    )
                inpFieldList = New Dictionary(Of String, Integer)
                With inpFieldList
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
                fileLinkageItem.InputFiledList = inpFieldList
                fileLinkageItem.InputExtention = "seq"
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '***************************
                '三重塩浜営業所
                '***************************
                fileLinkageItem = New FileLinkagePatternItem(
                    "012402", False
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
                    Return New FileLinkagePatternItem(officeCode)
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
        Public Enum ReserveInputFileType
            Csv = 1
            Excel2007 = 2 '4文字拡張子
            Excel2003 = 4 '3文字拡張子（これはマクロが入るからやらない想定？）
        End Enum

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="officeCode">営業所コード</param>
        Public Sub New(officeCode As String)
            Me.New(officeCode, False)
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="officeCode">営業所コード</param>
        ''' <param name="canUpload">アップロード可否フラグ(True:可能,False：不可)</param>
        Public Sub New(officeCode As String, canUpload As Boolean)
            Me.OfficeCode = officeCode
            Me.CanUpload = canUpload
        End Sub

        ''' <summary>
        ''' 営業所コード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String

        ''' <summary>
        ''' 出荷予約出力フォーマット
        ''' </summary>
        ''' <returns></returns>
        Public Property ReservedInputType As ReserveInputFileType = ReserveInputFileType.Csv
        ''' <summary>
        ''' 取込可否
        ''' </summary>
        ''' <returns></returns>
        Public Property CanUpload As Boolean = False
        ''' <summary>
        ''' 取込対象拡張子
        ''' </summary>
        ''' <returns></returns>
        Public Property InputExtention As String = "csv"

        Public Property InputRowStart As String = "1" '先頭行から読み取り
        ''' <summary>
        ''' 固定長CSVフラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property InputConstantField As Boolean = False
        Public Property InputDelem As String = ","
        ''' <summary> 
        ''' 出力フィールドリスト(フィールド名、固定長用フィールドサイズ）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>SEQファイル専用</remarks>
        Public Property InputFiledList As Dictionary(Of String, Integer)
    End Class
    ''' <summary>
    ''' 出力したオーダーのキー情報を保持する為のクラス
    ''' </summary>
    Public Class OutputOrdedrInfo
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me.OrderNo = OrderNo
            Me.DetailNo = DetailNo
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
    ''' <summary>
    ''' 取込ファイル情報クラス
    ''' </summary>
    <Serializable>
    Public Class InputDataItem
        ''' <summary>
        ''' チェック結果コード
        ''' </summary>
        ''' <remarks>現在単一使用想定だがビットマスクを考慮した数値</remarks>
        Public Enum CheckReasonCodes
            ''' <summary>
            ''' 正常
            ''' </summary>
            OK = 0
            ''' <summary>
            ''' 予約番号が未設定
            ''' </summary>
            NoReservedNo = 1
            ''' <summary>
            ''' 受注情報無し
            ''' </summary>
            NoOrderInfo = 2
            ''' <summary>
            ''' アップロードファイルの油種と受注情報の油種が不一致
            ''' </summary>
            OilUnMatch = 4
            ''' <summary>
            ''' 数量書式エラー
            ''' </summary>
            AmountFormatError = 8
            ''' <summary>
            ''' 実績値が0
            ''' </summary>
            AmountZero = 16
            ''' <summary>
            ''' タンク車不一致
            ''' </summary>
            TankUnmatch = 32
            ''' <summary>
            ''' 受注ステータスが登録範囲外(実績登録済みです。)
            ''' </summary>
            OrderStatusCannotAccept = 64
            ''' <summary>
            ''' 受注ステータスが登録ｷｬﾝｾﾙ(受注がキャンセルされています)
            ''' </summary>
            OrderStatusCancel = 128
            ''' <summary>
            ''' 受注ステータスが受注受付に戻される(タンク車割当が確定していないです)
            ''' </summary>
            OrderStatusBackToBefore200 = 256
            ''' <summary>
            ''' 本来ありえないが同一の予約番号、積込予定日で複数合致した場合
            ''' </summary>
            TooMenyOrderInfo = 32768
            ''' <summary>
            ''' 初期値
            ''' </summary>
            InitVal = 65536
        End Enum
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
        End Sub
        ''' <summary>
        ''' 取込ファイルの行番号(念のため)
        ''' </summary>
        ''' <returns></returns>
        Public Property InpRowNum As Integer = 0
        ''' <summary>
        ''' 取込対象フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property InputCheck As Boolean = False
        ''' <summary>
        ''' 画面表示用予約番号（ファイルにあったまま）
        ''' </summary>
        ''' <returns></returns>
        Public Property InpReservedNo As String = ""
        ''' <summary>
        ''' ファイルにあったままのタンクNo
        ''' </summary>
        ''' <returns></returns>
        Public Property InpTnkNo As String = ""
        ''' <summary>
        ''' ファイルにあったままの列車No（根岸のみの想定）
        ''' </summary>
        ''' <returns></returns>
        Public Property InpTrainNo As String = ""
        ''' <summary>
        ''' ファイルにあったままの油種名
        ''' </summary>
        ''' <returns></returns>
        Public Property InpOilTypeName As String = ""
        ''' <summary>
        ''' ファイルにあったままの数量（この数値を受注テーブルのデータを更新）
        ''' </summary>
        ''' <returns></returns>
        Public Property InpCarsAmount As String = ""
        ''' <summary>
        ''' チェック結果コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CheckReadonCode As CheckReasonCodes = CheckReasonCodes.InitVal
        ''' <summary>
        ''' 取込理由、取込「OK」や取込不可の理由を設定
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CheckReason As String
            Get
                Select Case Me.CheckReadonCode
                    Case CheckReasonCodes.OK
                        Return "正常"
                    Case CheckReasonCodes.NoReservedNo
                        Return "予約番号無し"
                    Case CheckReasonCodes.NoOrderInfo
                        Return "受注情報無し"
                    Case CheckReasonCodes.OilUnMatch
                        Return "油種不一致"
                    Case CheckReasonCodes.AmountFormatError
                        Return "数量書式エラー"
                    Case CheckReasonCodes.AmountZero
                        Return "出荷実績0"
                    Case CheckReasonCodes.TankUnmatch
                        Return "車番不一致"
                    Case CheckReasonCodes.OrderStatusCannotAccept
                        Return "実績登録済みです"
                    Case CheckReasonCodes.OrderStatusCancel
                        Return "受注がキャンセルされています"
                    Case CheckReasonCodes.OrderStatusBackToBefore200
                        Return "タンク車割当が確定していないです"
                    Case CheckReasonCodes.TooMenyOrderInfo
                        '本来ありえない想定だが念の為
                        Return "受注結果複数"
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 受注テーブルに引き当てる予約番号
        ''' </summary>
        ''' <returns></returns>
        Public Property ReservedNo As String = ""
        ''' <summary>
        ''' 受注テーブルより取得した予約番号
        ''' </summary>
        ''' <returns></returns>
        Public Property DbReservedNo As String = ""
        ''' <summary>
        ''' 積込日（予定）予約番号とセットで受注テーブルに引当特定する
        ''' </summary>
        ''' <returns></returns>
        Public Property LodDate As String = ""
        ''' <summary>
        ''' 積込日（予定）(DBより取得した日付）
        ''' </summary>
        ''' <returns></returns>
        Public Property DbLodDate As String = ""
        ''' <summary>
        ''' 発日(予定)
        ''' </summary>
        ''' <returns></returns>
        Public Property DepDate As String = ""
        ''' <summary>
        ''' 受注番号
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String = ""
        ''' <summary>
        ''' 受注明細番号
        ''' </summary>
        ''' <returns></returns>
        Public Property DetailNo As String = ""
        ''' <summary>
        ''' チェック時点で保存されている数量
        ''' </summary>
        ''' <returns></returns>
        Public Property CarsAmount As String = ""
        ''' <summary>
        ''' 受注テーブル登録の油種名
        ''' </summary>
        ''' <returns></returns>
        Public Property OilName As String = ""
        ''' <summary>
        ''' 受注テーブル登録のタンクNo
        ''' </summary>
        ''' <returns></returns>
        Public Property TankNo As String = ""
        ''' <summary>
        ''' 列車番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TrainNo As String = ""
        ''' <summary>
        ''' 更新する実績積込日（ブランクの場合は更新しない）
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdActualLodDate As String = ""
        ''' <summary>
        ''' 受注状況
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderStatus As String = ""
        ''' <summary>
        ''' チェック可否プロパティ
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanUpdate As Boolean
            Get
                If Me.CheckReadonCode = CheckReasonCodes.OK Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property
    End Class
    ''' <summary>
    ''' ファイル情報クラス
    ''' </summary>
    <System.Runtime.Serialization.DataContract()>
    Public Class AttachmentFile
        <System.Runtime.Serialization.DataMember()>
        Public Property FileName As String
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
                    If WW_DATE < CDate(C_DEFAULT_YMD) Then
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
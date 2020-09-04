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
                        Case "WF_ButtonOtSend"          'OT連携ボタン押下
                            WF_ButtonOtSend_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
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

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 受注一覧画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003D Then
            Master.RecoverTable(OIT0003tbl, work.WF_SEL_INPOTLINKAGETBL.Text)
        End If

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

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPOTLINKAGETBL.Text)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

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
            & "   AND OIT0002.ORDERSTATUS <= @P03" _

        '★積置フラグ無し用SQL
        SQLStrNashi &=
              SQLStrCmn _
            & "   AND OIT0002.LODDATE     >= @P02"

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
            Using SQLcmd As New SqlCommand(SQLStrNashi, SQLcon)
                'Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 20) '受注営業所コード
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.Date)         '積込日
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 3)  '受注進行ステータス
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 1)  '削除フラグ
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 6)  '組織コード
                'PARA01.Value = OFFICECDE
                PARA02.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                'PARA02.Value = "2020/08/20"
                PARA03.Value = BaseDllConst.CONST_ORDERSTATUS_310
                PARA04.Value = C_DELETE_FLG.DELETE
                PARA05.Value = Master.USER_ORG

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003row As DataRow In OIT0003tbl.Rows
                    i += 1
                    OIT0003row("LINECNT") = i        'LINECNT

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
    ''' OT連携ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOtSend_Click()

        '一覧のチェックボックスが選択されているか確認
        If OIT0003tbl.Select("OPERATION = 'on'").Count = 0 Then
            '選択されていない場合は、エラーメッセージを表示し終了
            Master.Output(C_MESSAGE_NO.OIL_OTLINKAGELINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '******************************
        'OT発送日報データ取得処理
        '******************************
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            OTLinkageDataGet(SQLcon)
        End Using

        '******************************
        'CSV作成処理の実行
        '******************************
        Using repCbj = New CsvCreate(OIT0003CsvOTLinkagetbl, I_FolderPath:=CS0050SESSION.OTFILESEND_PATH)
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

        ''○ 遷移先(OT連携一覧画面)退避データ保存先の作成
        'WW_CreateXMLSaveFile()

        ''○ 画面表示データ保存
        'Master.SaveTable(OIT0003tbl, work.WF_SEL_INPOTLINKAGETBL.Text)

    End Sub

    ''' <summary>
    ''' OT発送日報データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub OTLinkageDataGet(ByVal SQLcon As SqlConnection)

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
              " , REPLACE(CONVERT(NCHAR(4), ''), SPACE(1), '0')  AS TRAINNO" _
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
            & " , OIM0003.OTOILCODE                              AS OTOILCODE" _
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
            & "     (OIT0003.ORDERNO = OIT0002.ORDERNO " _
            & "      OR OIT0003.STACKINGORDERNO = OIT0002.ORDERNO) " _
            & " AND OIT0003.DELFLG <> @P02 " _
            & " AND OIT0003.STACKINGFLG = '1' " _
            & " AND OIT0003.ACTUALLODDATE >= @P03 "

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
            & " AND OIM0010.DELFLG <> @P02 " _
            & " LEFT JOIN (SELECT  " _
            & "              OIM0005.TANKNUMBER " _
            & "            , CASE  " _
            & "              WHEN OIM0005.MODEL = 'タキ1000' THEN 100000 + CONVERT(INT, OIM0005.TANKNUMBER) " _
            & "              ELSE OIM0005.TANKNUMBER " _
            & "              END AS MODELTANKNO " _
            & "            , CASE  " _
            & "              WHEN CONVERT(VARCHAR, OIM0005.LOAD) <> '44.0' THEN '' " _
            & "              ELSE CONVERT(VARCHAR, CONVERT(INT, OIM0005.LOAD)) " _
            & "              END AS LOAD " _
            & "            , OIM0005.DELFLG " _
            & "            FROM oil.OIM0005_TANK OIM0005) OIM0005 ON " _
            & "     OIM0005.TANKNUMBER = OIT0003.TANKNO " _
            & " AND OIM0005.DELFLG <> @P02 " _
            & " LEFT JOIN OIL.OIM0025_OTLINKAGE OIM0025 ON " _
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
        For Each OIT0003row As DataRow In OIT0003tbl.Rows

            '★指定されていない行はSKIP
            If Convert.ToString(OIT0003row("OPERATION")) = "" Then Continue For

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
            & "   AND OIT0002.LODDATE >= @P03 "

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
                'PARA01.Value = ""
                PARA02.Value = C_DELETE_FLG.DELETE
                PARA03.Value = Format(Now.AddDays(1), "yyyy/MM/dd")
                'PARA03.Value = "2020/08/20"
                PARA04.Value = BaseDllConst.CONST_ORDERSTATUS_310

                '★桁数設定
                Dim VALUE01 As SqlParameter = SQLcmd.Parameters.Add("@V01", SqlDbType.Int) '支店Ｃ(当社日報)
                VALUE01.Value = iOURDAILYBRANCHC

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0003CsvOTLinkagetbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0003CsvOTLinkagetbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0003Csvrow As DataRow In OIT0003CsvOTLinkagetbl.Rows
                    'i += 1
                    'OIT0003Csvrow("LINECNT") = i        'LINECNT

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
            Exit Sub
        End Try

        '○ 画面表示データ保存
        'Master.SaveTable(OIT0003CsvOTLinkagetbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage(work.WF_SEL_CAMPCODE.Text + "2")

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
    Public Class fileLinkagePattern
        Private _Item As Dictionary(Of String, FileLinkagePatternItem)
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New()
            Me._Item = New Dictionary(Of String, FileLinkagePatternItem)
            Dim fileLinkageItem As FileLinkagePatternItem
            With Me._Item
                '仙台新港営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "011202", True, True, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '五井営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "011201", True, True, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '甲子営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "011202", True, True, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '袖ヶ浦営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "011203", True, True, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '根岸営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "011402", True, True, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '四日市営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "012401", True, True, True
                    )
                .Add(fileLinkageItem.OfficeCode, fileLinkageItem)
                '三重塩浜営業所
                fileLinkageItem = New FileLinkagePatternItem(
                    "012402", True, True, True
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
                Return Me._Item(officeCode)
            End Get

        End Property

    End Class


    ''' <summary>
    ''' 外部連携パターンアイテムクラス
    ''' </summary>
    Public Class FileLinkagePatternItem
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
    End Class

End Class
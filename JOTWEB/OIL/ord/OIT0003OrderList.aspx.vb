'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 受注一覧画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0003OrderList
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0003tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0003INPtbl As DataTable                              'チェック用テーブル
    Private OIT0003UPDtbl As DataTable                              '更新用テーブル
    Private OIT0003WKtbl As DataTable                               '作業用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー
    Private Const CONST_ORDERSTS_CAN As String = "900"              '受注キャンセル
    Private Const CONST_ORDERSTS_CANNM As String = "受注キャンセル" '受注キャンセル

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
    Private WW_ORDERSTATUS As String = ""                           '受注進行ステータス

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
                        Case "WF_ButtonORDER_CANCEL"     'キャンセルボタン押下
                            WF_ButtonORDER_CANCEL_Click()
                        Case "WF_ButtonINSERT"          '受注新規作成ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonLinkINSERT"      '貨車連結選択ボタン押下
                            WF_ButtonLinkINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
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
        Master.MAPID = OIT0003WRKINC.MAPIDL
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

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0003D Then
            Master.RecoverTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)
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
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0003D Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0003tbl)

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
        ''〇 受注キャンセルの場合はダブルクリックは無効
        'If (WW_ORDERSTATUS = CONST_ORDERSTS_CAN) Then
        '    CS0013ProfView.LEVENT = ""
        '    CS0013ProfView.LFUNC = ""
        'Else

        'End If

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

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                  AS LINECNT" _
            & " , ''                                                 AS OPERATION" _
            & " , CAST(OIT0002.UPDTIMSTP AS bigint)                  AS TIMSTP" _
            & " , 1                                                  AS 'SELECT'" _
            & " , 0                                                  AS HIDDEN" _
            & " , ISNULL(FORMAT(OIT0002.ORDERYMD, 'yyyy/MM/dd'), '') AS ORDERYMD" _
            & " , ISNULL(RTRIM(OIT0002.OFFICECODE), '')              AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0002.OFFICENAME), '')              AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERSTATUS), '')             AS ORDERSTATUS" _
            & " , ISNULL(RTRIM(OIS0015_1.VALUE1), '')                AS ORDERSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERINFO), '')               AS ORDERINFO" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '10' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '11' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '12' THEN '<div style=""letter-spacing:normal;color:blue;"">' + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIS0015_2.VALUE1), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIS0015_2.VALUE1), '')" _
            & "   END                                                AS ORDERINFONAME" _
            & " , ISNULL(RTRIM(OIT0002.STACKINGFLG), '')   　        AS STACKINGFLG" _
            & " , ISNULL(RTRIM(OIT0002.USEPROPRIETYFLG), '')   　    AS USEPROPRIETYFLG" _
            & " , ISNULL(RTRIM(OIT0002.ORDERNO), '')   　            AS ORDERNO" _
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   WHEN '81' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TRAINNO), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIT0002.TRAINNO), '')" _
            & "   END                                                AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0002.TRAINNAME), '')               AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0002.ORDERTYPE), '')               AS ORDERTYPE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSCODE), '')            AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0002.SHIPPERSNAME), '')            AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0002.BASECODE), '')                AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0002.BASENAME), '')                AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '')           AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '')           AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATION), '')              AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '')          AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATION), '')              AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '')          AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0002.CANGERETSTATION), '')         AS CHANGERETSTATION" _
            & " , ISNULL(RTRIM(OIT0002.CHANGEARRSTATIONNAME), '')    AS CHANGEARRSTATIONNAME" _
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
            & " , CASE ISNULL(RTRIM(OIT0002.ORDERINFO), '')" _
            & "   WHEN '80' THEN '<div style=""letter-spacing:normal;color:red;"">'  + ISNULL(RTRIM(OIT0002.TOTALTANK), '') + '</div>'" _
            & "   ELSE ISNULL(RTRIM(OIT0002.TOTALTANK), '')" _
            & "   END                                                AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0002.RTANKCH), '')                 AS RTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.HTANKCH), '')                 AS HTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.TTANKCH), '')                 AS TTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.MTTANKCH), '')                AS MTTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.KTANKCH), '')                 AS KTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.K3TANKCH), '')                AS K3TANKCH" _
            & " , ISNULL(RTRIM(OIT0002.K5TANKCH), '')                AS K5TANKCH" _
            & " , ISNULL(RTRIM(OIT0002.K10TANKCH), '')               AS K10TANKCH" _
            & " , ISNULL(RTRIM(OIT0002.LTANKCH), '')                 AS LTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.ATANKCH), '')                 AS ATANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER1OTANKCH), '')           AS OTHER1OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER2OTANKCH), '')           AS OTHER2OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER3OTANKCH), '')           AS OTHER3OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER4OTANKCH), '')           AS OTHER4OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER5OTANKCH), '')           AS OTHER5OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER6OTANKCH), '')           AS OTHER6OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER7OTANKCH), '')           AS OTHER7OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER8OTANKCH), '')           AS OTHER8OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER9OTANKCH), '')           AS OTHER9OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.OTHER10OTANKCH), '')          AS OTHER10OTANKCH" _
            & " , ISNULL(RTRIM(OIT0002.TOTALTANKCH), '')             AS TOTALTANKCH" _
            & " , ISNULL(FORMAT(OIT0002.LODDATE, 'yyyy/MM/dd'), '')           AS LODDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALLODDATE, 'yyyy/MM/dd'), '')     AS ACTUALLODDATE" _
            & " , ISNULL(FORMAT(OIT0002.DEPDATE, 'yyyy/MM/dd'), '')           AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALDEPDATE, 'yyyy/MM/dd'), '')     AS ACTUALDEPDATE" _
            & " , ISNULL(FORMAT(OIT0002.ARRDATE, 'yyyy/MM/dd'), '')           AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALARRDATE, 'yyyy/MM/dd'), '')     AS ACTUALARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACCDATE, 'yyyy/MM/dd'), '')           AS ACCDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALACCDATE, 'yyyy/MM/dd'), '')     AS ACTUALACCDATE" _
            & " , ISNULL(FORMAT(OIT0002.EMPARRDATE, 'yyyy/MM/dd'), '')        AS EMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')  AS ACTUALEMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0002.KEIJYOYMD, 'yyyy/MM/dd'), '')         AS KEIJYOYMD" _
            & " , ISNULL(RTRIM(OIT0002.SALSE), '')                   AS SALSE" _
            & " , ISNULL(RTRIM(OIT0002.SALSETAX), '')                AS SALSETAX" _
            & " , ISNULL(RTRIM(OIT0002.TOTALSALSE), '')              AS TOTALSALSE" _
            & " , ISNULL(RTRIM(OIT0002.PAYMENT), '')                 AS PAYMENT" _
            & " , ISNULL(RTRIM(OIT0002.PAYMENTTAX), '')              AS PAYMENTTAX" _
            & " , ISNULL(RTRIM(OIT0002.TOTALPAYMENT), '')            AS TOTALPAYMENT" _
            & " , ISNULL(RTRIM(OIT0002.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0002_ORDER OIT0002 " _
            & "  INNER JOIN OIL.VIW0003_OFFICECHANGE VIW0003 ON " _
            & "        VIW0003.ORGCODE    = @P1 " _
            & "    AND VIW0003.OFFICECODE = OIT0002.OFFICECODE " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_1 ON " _
            & "        OIS0015_1.CLASS   = 'ORDERSTATUS' " _
            & "    AND OIS0015_1.KEYCODE = OIT0002.ORDERSTATUS " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
            & "        OIS0015_2.CLASS   = 'ORDERINFO' " _
            & "    AND OIS0015_2.KEYCODE = OIT0002.ORDERINFO " _
            & " WHERE OIT0002.ORDERYMD   >= @P2" _
            & "   AND OIT0002.DELFLG     <> @P3"

        '& " , ISNULL(RTRIM(OIS0015_2.VALUE1), '')                AS ORDERINFONAME" _
        '& " , ISNULL(RTRIM(OIT0002.TRAINNO), '')                 AS TRAINNO" _
        '& " , ISNULL(RTRIM(OIT0002.TOTALTANK), '')               AS TOTALTANK" _


        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_SALESOFFICECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.OFFICECODE = '{0}'", work.WF_SEL_SALESOFFICECODE.Text)
        End If
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0002.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If
        '荷卸地(荷受人)
        If Not String.IsNullOrEmpty(work.WF_SEL_UNLOADINGCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.CONSIGNEECODE = '{0}'", work.WF_SEL_UNLOADINGCODE.Text)
        End If
        '状態(受注進行ステータス)
        If Not String.IsNullOrEmpty(work.WF_SEL_STATUSCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0002.ORDERSTATUS = '{0}'", work.WF_SEL_STATUSCODE.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0002.ORDERNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 10) '受注№
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '積込日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = Master.USER_ORG
                PARA2.Value = work.WF_SEL_DATE.Text
                PARA3.Value = C_DELETE_FLG.DELETE

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

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003L Select"
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
            If OIT0003tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0003tbl.Rows(i)("OPERATION") = "" AndAlso OIT0003tbl.Rows(i)("ORDERSTATUS") <> "900" Then
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
            If OIT0003tbl.Rows(i)("HIDDEN") = "0" AndAlso OIT0003tbl.Rows(i)("ORDERSTATUS") <> "900" Then
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
            If OIT0003tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0003tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

    End Sub

    ''' <summary>
    ''' キャンセルボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonORDER_CANCEL_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0003tbl)

        '■■■ OIT0003tbl関連の受注TBLの「受注進行ステータス」を「900:受注キャンセル」に更新 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0002_ORDER       " _
                    & "    SET UPDYMD      = @P11,      " _
                    & "        UPDUSER     = @P12,      " _
                    & "        UPDTERMID   = @P13,      " _
                    & "        RECEIVEYMD  = @P14,      " _
                    & "        ORDERSTATUS = @P15       " _
                    & "  WHERE ORDERNO     = @P01       " _
                    & "    AND DELFLG     <> '1'       ;"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)
            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)

            '選択されている行の受注進行ステータスを「900:受注キャンセル」に更新
            For Each OIT0003UPDrow In OIT0003tbl.Rows
                If OIT0003UPDrow("OPERATION") = "on" Then
                    PARA01.Value = OIT0003UPDrow("ORDERNO")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD
                    PARA15.Value = CONST_ORDERSTS_CAN

                    OIT0003UPDrow("ORDERSTATUS") = CONST_ORDERSTS_CAN
                    OIT0003UPDrow("ORDERSTATUSNAME") = CONST_ORDERSTS_CANNM

                    SQLcmd.ExecuteNonQuery()
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注新規作成ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = ""
        '受注営業所(名)
        work.WF_SEL_ORDERSALESOFFICE.Text = ""
        '受注営業所(コード)
        work.WF_SEL_ORDERSALESOFFICECODE.Text = ""
        '受注進行ステータス(名)
        work.WF_SEL_ORDERSTATUSNM.Text = "受注受付"
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = "100"
        '受注情報(名)
        work.WF_SEL_INFORMATIONNM.Text = ""
        '受注情報(コード)
        work.WF_SEL_INFORMATION.Text = ""
        '積置可否フラグ(１：積置あり, ２：積置なし)
        work.WF_SEL_STACKINGFLG.Text = "2"
        '利用可否フラグ(１：利用可, ２：利用不可)
        work.WF_SEL_USEPROPRIETYFLG.Text = "1"
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = ""
        '本線列車
        work.WF_SEL_TRAIN.Text = ""
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = ""
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = ""
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = ""
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = ""
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = ""
        'パターンコード(名)
        work.WF_SEL_PATTERNNAME.Text = ""
        'パターンコード
        work.WF_SEL_PATTERNCODE.Text = ""
        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = ""
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = ""
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = ""
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = ""
        '戻着駅(名)
        work.WF_SEL_CANGERETSTATIONNM.Text = ""
        '戻着駅(コード)
        work.WF_SEL_CANGERETSTATION.Text = ""

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = "0"
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = "0"
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = "0"
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = "0"
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = "0"
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = "0"
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = "0"
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = "0"
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = "0"
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = "0"
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = "0"

        '積込日(予定)
        work.WF_SEL_LODDATE.Text = ""
        '発日(予定)
        work.WF_SEL_DEPDATE.Text = ""
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = ""
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = ""
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = ""
        '積込日(実績)
        work.WF_SEL_ACTUALLODDATE.Text = ""
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = ""
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = ""
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = ""
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""

        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = ""
        '売上金額
        work.WF_SEL_SALSE.Text = "0"
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = "0"
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = "0"
        '支払金額
        work.WF_SEL_PAYMENT.Text = "0"
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = "0"
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = "0"

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "1"
        '作成フラグ(貨車連結未使用：1, 貨車連結使用：2)
        work.WF_SEL_CREATELINKFLG.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

    End Sub

    ''' <summary>
    ''' 貨車連結選択ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLinkINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""
        '登録日
        'work.WF_SEL_REGISTRATIONDATE.Text = DateTime.Now.ToString("d")
        work.WF_SEL_REGISTRATIONDATE.Text = work.WF_SEL_DATE.Text
        '受注営業所(名)
        work.WF_SEL_ORDERSALESOFFICE.Text = ""
        '受注営業所(コード)
        work.WF_SEL_ORDERSALESOFFICECODE.Text = ""
        '受注進行ステータス(名)
        work.WF_SEL_ORDERSTATUSNM.Text = "受注受付"
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = "100"
        '受注情報(名)
        work.WF_SEL_INFORMATIONNM.Text = ""
        '受注情報(コード)
        work.WF_SEL_INFORMATION.Text = ""
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = ""
        '本線列車
        work.WF_SEL_TRAIN.Text = ""
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = ""
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = ""
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = ""
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = ""
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = ""
        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = ""
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = ""
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = ""
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = ""
        '戻着駅(名)
        work.WF_SEL_CANGERETSTATIONNM.Text = ""
        '戻着駅(コード)
        work.WF_SEL_CANGERETSTATION.Text = ""

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = "0"
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = "0"
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = "0"
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = "0"
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = "0"
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = "0"
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = "0"
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = "0"
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = "0"
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = "0"
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = "0"

        '積込日(予定)
        work.WF_SEL_LODDATE.Text = ""
        '発日(予定)
        work.WF_SEL_DEPDATE.Text = ""
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = ""
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = ""
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = ""
        '積込日(実績)
        work.WF_SEL_ACTUALLODDATE.Text = ""
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = ""
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = ""
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = ""
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""

        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = ""
        '売上金額
        work.WF_SEL_SALSE.Text = "0"
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = "0"
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = "0"
        '支払金額
        work.WF_SEL_PAYMENT.Text = "0"
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = "0"
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = "0"

        '削除フラグ
        work.WF_SEL_DELFLG.Text = "0"
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "1"
        '作成フラグ(貨車連結未使用：1, 貨車連結使用：2)
        work.WF_SEL_CREATELINKFLG.Text = "2"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text + "1")

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0
        Dim WW_FIELD_OBJ As Object = Nothing
        Dim WW_VALUE As String = ""
        Dim WW_TEXT As String = ""

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '〇 受注進行ステータスが"900(受注キャンセル)"の場合は何もしない
        WW_ORDERSTATUS = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        If WW_ORDERSTATUS = CONST_ORDERSTS_CAN Then
            Exit Sub
        End If

        '選択行
        work.WF_SEL_LINECNT.Text = OIT0003tbl.Rows(WW_LINECNT)("LINECNT")
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERYMD")
        '受注営業所(名)
        work.WF_SEL_ORDERSALESOFFICE.Text = OIT0003tbl.Rows(WW_LINECNT)("OFFICENAME")
        '受注営業所(コード)
        work.WF_SEL_ORDERSALESOFFICECODE.Text = OIT0003tbl.Rows(WW_LINECNT)("OFFICECODE")
        '受注進行ステータス(名)
        work.WF_SEL_ORDERSTATUSNM.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUSNAME")
        '受注進行ステータス(コード)
        work.WF_SEL_ORDERSTATUS.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        '受注情報(名)
        'work.WF_SEL_INFORMATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERINFONAME")
        work.WF_SEL_INFORMATIONNM.Text = Regex.Replace(OIT0003tbl.Rows(WW_LINECNT)("ORDERINFONAME"), "<[^>]*?>", "")
        '受注情報(コード)
        work.WF_SEL_INFORMATION.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERINFO")
        '積置可否フラグ
        work.WF_SEL_STACKINGFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("STACKINGFLG")
        '利用可否フラグ
        work.WF_SEL_USEPROPRIETYFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("USEPROPRIETYFLG")
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERNO")
        '本線列車
        'work.WF_SEL_TRAIN.Text = OIT0003tbl.Rows(WW_LINECNT)("TRAINNO")
        work.WF_SEL_TRAIN.Text = Regex.Replace(OIT0003tbl.Rows(WW_LINECNT)("TRAINNO"), "<[^>]*?>", "")
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = OIT0003tbl.Rows(WW_LINECNT)("TRAINNAME")
        '受注パターン
        work.WF_SEL_PATTERNCODE.Text = OIT0003tbl.Rows(WW_LINECNT)("ORDERTYPE")
        '受注パターン(名)
        work.WF_SEL_PATTERNNAME.Text = ""
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = OIT0003tbl.Rows(WW_LINECNT)("SHIPPERSNAME")
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = OIT0003tbl.Rows(WW_LINECNT)("SHIPPERSCODE")
        '基地(名)
        work.WF_SEL_BASENAME.Text = OIT0003tbl.Rows(WW_LINECNT)("BASENAME")
        '基地(コード)
        work.WF_SEL_BASECODE.Text = OIT0003tbl.Rows(WW_LINECNT)("BASECODE")
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = OIT0003tbl.Rows(WW_LINECNT)("CONSIGNEENAME")
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = OIT0003tbl.Rows(WW_LINECNT)("CONSIGNEECODE")
        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPSTATIONNAME")
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPSTATION")
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("ARRSTATIONNAME")
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("ARRSTATION")
        '戻着駅(名)
        work.WF_SEL_CANGERETSTATIONNM.Text = OIT0003tbl.Rows(WW_LINECNT)("CHANGEARRSTATIONNAME")
        '戻着駅(コード)
        work.WF_SEL_CANGERETSTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("CHANGERETSTATION")

        '車数（レギュラー）
        work.WF_SEL_REGULAR_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("RTANK")
        '車数（ハイオク）
        work.WF_SEL_HIGHOCTANE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("HTANK")
        '車数（灯油）
        work.WF_SEL_KEROSENE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("TTANK")
        '車数（未添加灯油）
        work.WF_SEL_NOTADDED_KEROSENE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("MTTANK")
        '車数（軽油）
        work.WF_SEL_DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("KTANK")
        '車数（３号軽油）
        work.WF_SEL_NUM3DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K3TANK")
        '車数（５号軽油）
        work.WF_SEL_NUM5DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K5TANK")
        '車数（１０号軽油）
        work.WF_SEL_NUM10DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K10TANK")
        '車数（LSA）
        work.WF_SEL_LSA_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("LTANK")
        '車数（A重油）
        work.WF_SEL_AHEAVY_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("ATANK")
        '合計車数
        'work.WF_SEL_TANKCARTOTAL.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALTANK")
        work.WF_SEL_TANKCARTOTAL.Text = Regex.Replace(OIT0003tbl.Rows(WW_LINECNT)("TOTALTANK"), "<[^>]*?>", "")

        '車数（レギュラー）割当
        work.WF_SEL_REGULARCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("RTANKCH")
        '車数（ハイオク）割当
        work.WF_SEL_HIGHOCTANECH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("HTANKCH")
        '車数（灯油）割当
        work.WF_SEL_KEROSENECH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("TTANKCH")
        '車数（未添加灯油）割当
        work.WF_SEL_NOTADDED_KEROSENECH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("MTTANKCH")
        '車数（軽油）割当
        work.WF_SEL_DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("KTANKCH")
        '車数（３号軽油）割当
        work.WF_SEL_NUM3DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K3TANKCH")
        '車数（５号軽油）割当
        work.WF_SEL_NUM5DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K5TANKCH")
        '車数（１０号軽油）割当
        work.WF_SEL_NUM10DIESELCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K10TANKCH")
        '車数（LSA）割当
        work.WF_SEL_LSACH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("LTANKCH")
        '車数（A重油）割当
        work.WF_SEL_AHEAVYCH_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("ATANKCH")
        '合計車数（割当）
        work.WF_SEL_TANKCARTOTALCH.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALTANKCH")

        '積込日(予定)
        work.WF_SEL_LODDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("LODDATE")
        '発日(予定)
        work.WF_SEL_DEPDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPDATE")
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ARRDATE")
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACCDATE")
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("EMPARRDATE")
        '積込日(実績)
        work.WF_SEL_ACTUALLODDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALLODDATE")
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALDEPDATE")
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALARRDATE")
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALACCDATE")
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALEMPARRDATE")

        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = OIT0003tbl.Rows(WW_LINECNT)("KEIJYOYMD")
        '売上金額
        work.WF_SEL_SALSE.Text = OIT0003tbl.Rows(WW_LINECNT)("SALSE")
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = OIT0003tbl.Rows(WW_LINECNT)("SALSETAX")
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALSALSE")
        '支払金額
        work.WF_SEL_PAYMENT.Text = OIT0003tbl.Rows(WW_LINECNT)("PAYMENT")
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = OIT0003tbl.Rows(WW_LINECNT)("PAYMENTTAX")
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALPAYMENT")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("DELFLG")
        '作成フラグ(新規登録：1, 更新：2)
        work.WF_SEL_CREATEFLG.Text = "2"
        '作成フラグ(貨車連結未使用：1, 貨車連結使用：2)
        work.WF_SEL_CREATELINKFLG.Text = "1"

        '○ 状態をクリア
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            Select Case OIT0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case OIT0003tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIT0003tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPTBL.Text)

        '登録画面ページへ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text + "1")

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
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003row As DataRow In OIT0003tbl.Rows
            If OIT0003row("HIDDEN") = 0 Then
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
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub
End Class
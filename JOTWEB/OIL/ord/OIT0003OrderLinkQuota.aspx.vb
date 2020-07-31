'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 受注貨車連結割当画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0003OrderLinkQuota
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
                        Case "WF_ButtonINSERT"          'タンク車割当ボタン押下
                            WF_ButtonINSERT_Click()
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
        Master.MAPID = OIT0003WRKINC.MAPIDQ
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
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0001D Then
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
              " SELECT DISTINCT" _
            & "   0                                                           AS LINECNT" _
            & " , ''                                                          AS OPERATION" _
            & " , CAST(OIT0004.UPDTIMSTP AS bigint)                           AS TIMSTP" _
            & " , 1                                                           AS 'SELECT'" _
            & " , 0                                                           AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0004.LINKNO), '')   　                      AS LINKNO" _
            & " , ISNULL(RTRIM(OIT0004.LINKDETAILNO), '')                     AS LINKDETAILNO" _
            & " , ISNULL(FORMAT(OIT0004.AVAILABLEYMD, 'yyyy/MM/dd'), '')      AS AVAILABLEYMD" _
            & " , ISNULL(RTRIM(OIT0004.STATUS), '')                           AS STATUS" _
            & " , ISNULL(RTRIM(OIT0004.INFO), '')                             AS INFO" _
            & " , ISNULL(RTRIM(OIT0004.PREORDERNO), '')                       AS PREORDERNO" _
            & " , ISNULL(RTRIM(OIT0004.TRAINNO), '')                          AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0004.TRAINNAME), '')                        AS TRAINNAME" _
            & " , ISNULL(RTRIM(OIT0004.OFFICECODE), '')                       AS OFFICECODE" _
            & " , ''                                                          AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATION), '')                       AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0004.DEPSTATIONNAME), '')                   AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0004.RETSTATION), '')                       AS RETSTATION" _
            & " , ISNULL(RTRIM(OIT0004.RETSTATIONNAME), '')                   AS RETSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0004.HTANK), '')                            AS HTANK" _
            & " , ISNULL(RTRIM(OIT0004.RTANK), '')                            AS RTANK" _
            & " , ISNULL(RTRIM(OIT0004.TTANK), '')                            AS TTANK" _
            & " , ISNULL(RTRIM(OIT0004.MTTANK), '')                           AS MTTANK" _
            & " , ISNULL(RTRIM(OIT0004.KTANK), '')                            AS KTANK" _
            & " , ISNULL(RTRIM(OIT0004.K3TANK), '')                           AS K3TANK" _
            & " , ISNULL(RTRIM(OIT0004.K5TANK), '')                           AS K5TANK" _
            & " , ISNULL(RTRIM(OIT0004.K10TANK), '')                          AS K10TANK" _
            & " , ISNULL(RTRIM(OIT0004.LTANK), '')                            AS LTANK" _
            & " , ISNULL(RTRIM(OIT0004.ATANK), '')                            AS ATANK" _
            & " , ISNULL(RTRIM(OIT0004.TOTALTANK), '')                        AS TOTALTANK" _
            & " , ISNULL(FORMAT(OIT0004.EMPARRDATE, 'yyyy/MM/dd'), '')        AS EMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0004.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')  AS ACTUALEMPARRDATE" _
            & " , ISNULL(RTRIM(OIT0004.LINETRAINNO), '')                      AS LINETRAINNO" _
            & " , ISNULL(RTRIM(OIT0004.LINEORDER), '')                        AS LINEORDER" _
            & " , ISNULL(RTRIM(OIT0004.TANKNUMBER), '')                       AS TANKNUMBER" _
            & " , ISNULL(RTRIM(OIT0004.DELFLG), '')                           AS DELFLG" _
            & " , OIT0004.SORT_YMD                                            AS SORT_YMD" _
            & " FROM ( " _
            & "  SELECT " _
            & "    OIT0004.* " _
            & "  , OIT0004.AVAILABLEYMD AS SORT_YMD " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P10 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS HTANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P11 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS RTANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P12 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS TTANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P13 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS MTTANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P14 OR OIT0004.PREOILCODE = @P15 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS KTANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P16 OR OIT0004.PREOILCODE = @P17 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS K3TANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P18 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS K5TANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P19 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS K10TANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P20 OR OIT0004.PREOILCODE = @P21 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS LTANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE = @P22 THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS ATANK " _
            & "  , SUM(CASE WHEN OIT0004.PREOILCODE <>'' THEN 1 ELSE 0 END) OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO) AS TOTALTANK " _
            & "  , ROW_NUMBER() OVER (PARTITION BY OIT0004.OFFICECODE, OIT0004.LINKNO ORDER BY OIT0004.AVAILABLEYMD DESC) RNUM" _
            & "  FROM OIL.OIT0004_LINK OIT0004 " _
            & "  INNER JOIN OIL.VIW0003_OFFICECHANGE VIW0003 ON " _
            & "        VIW0003.ORGCODE    = @P1" _
            & "    AND VIW0003.OFFICECODE = OIT0004.OFFICECODE " _
            & "  WHERE OIT0004.AVAILABLEYMD >= @P2" _
            & "    AND OIT0004.STATUS       = '1'"

        '& "  WHERE OIT0004.OFFICECODE = @P1" _
        '& "    AND OIT0004.AVAILABLEYMD < @P2"
        '& "    AND OIT0004.TRAINNO = @P3" _
        '& "    AND OIT0004.DELFLG <> @P4" _
        '& "  ) OIT0004 " _
        '& " WHERE OIT0004.RNUM = 1"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '受注営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_ORDERSALESOFFICECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0004.OFFICECODE = '{0}'", work.WF_SEL_ORDERSALESOFFICECODE.Text)
        ElseIf Not String.IsNullOrEmpty(work.WF_SEL_SALESOFFICECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0004.OFFICECODE = '{0}'", work.WF_SEL_SALESOFFICECODE.Text)
        End If
        '列車番号
        'If Not String.IsNullOrEmpty(work.WF_SEL_TRAIN.Text) Then
        '    SQLStr &= String.Format("    AND OIT0004.TRAINNO = '{0}'", work.WF_SEL_TRAIN.Text)
        'End If
        '貨車連結順序表№(一度設定した貨車連結順序表№があれば紐づけを実施)
        If Not String.IsNullOrEmpty(work.WF_SEL_LINKNO.Text) Then
            SQLStr &= String.Format("    AND OIT0004.LINKNO = '{0}'", work.WF_SEL_LINKNO.Text)
        End If

        SQLStr &=
              "    AND OIT0004.DELFLG <> @P4" _
            & "  ) OIT0004 " _
            & " WHERE OIT0004.RNUM = 1"

        SQLStr &=
              " ORDER BY" _
            & "    OIT0004.SORT_YMD"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 6) '登録営業所コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)    '利用可能日
                'Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 4) '本線列車
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.NVarChar, 1) '削除フラグ
                PARA1.Value = Master.USER_ORG
                'PARA1.Value = work.WF_SEL_SALESOFFICECODE.Text
                '### START（2020年3月24日 森川) #############
                '利用可能日の条件を登録日⇒(予定)積込日へ変更
                'PARA2.Value = work.WF_SEL_REGISTRATIONDATE.Text
                If work.WF_SEL_LODDATE.Text.Equals("") Then
                    PARA2.Value = work.WF_SEL_REGISTRATIONDATE.Text
                Else
                    '### 20200709 START 利用可能日の条件を変更 #####################################
                    'PARA2.Value = work.WF_SEL_LODDATE.Text
                    '★(予定)積込日－1日(オーダーは積置の前日に作成するため)を条件とする。
                    PARA2.Value = Date.Parse(work.WF_SEL_LODDATE.Text).AddDays(-1).ToString("yyyy/MM/dd")
                    '### 20200709 END   利用可能日の条件を変更 #####################################
                End If
                '### END  ###################################
                'PARA3.Value = work.WF_SEL_TRAIN.Text
                PARA4.Value = C_DELETE_FLG.DELETE

                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 4) '油種(ハイオク)
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 4) '油種(レギュラー)
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.NVarChar, 4) '油種(灯油)
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.NVarChar, 4) '油種(未添加灯油)
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 4) '油種(軽油)
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 4) '油種(軽油)
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.NVarChar, 4) '油種(３号軽油)
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 4) '油種(３号軽油)
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 4) '油種(５号軽油)
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 4) '油種(１０号軽油)
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 4) '油種(ＬＳＡ)
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.NVarChar, 4) '油種(ＬＳＡ)
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.NVarChar, 4) '油種(Ａ重油)
                PARA10.Value = BaseDllConst.CONST_HTank
                PARA11.Value = BaseDllConst.CONST_RTank
                PARA12.Value = BaseDllConst.CONST_TTank
                PARA13.Value = BaseDllConst.CONST_MTTank
                PARA14.Value = BaseDllConst.CONST_KTank1
                PARA15.Value = BaseDllConst.CONST_KTank2
                PARA16.Value = BaseDllConst.CONST_K3Tank1
                PARA17.Value = BaseDllConst.CONST_K3Tank2
                PARA18.Value = BaseDllConst.CONST_K5Tank
                PARA19.Value = BaseDllConst.CONST_K10Tank
                PARA20.Value = BaseDllConst.CONST_LTank1
                PARA21.Value = BaseDllConst.CONST_LTank2
                PARA22.Value = BaseDllConst.CONST_ATank

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

                    '受注進行ステータス
                    CODENAME_get("ORDERSTATUS", OIT0003row("STATUS"), OIT0003row("STATUS"), WW_DUMMY)
                    '受注情報
                    CODENAME_get("ORDERINFO", OIT0003row("INFO"), OIT0003row("INFO"), WW_DUMMY)
                    '担当営業所
                    CODENAME_get("SALESOFFICE", OIT0003row("OFFICECODE"), OIT0003row("OFFICENAME"), WW_DUMMY)
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0003Q SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0003Q Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' タンク車割当ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SEL_LINK_LINECNT.Text = ""
        '貨車連結順序表№
        work.WF_SEL_LINK_LINKNO.Text = ""
        '登録日
        work.WF_SEL_LINK_REGISTRATIONDATE.Text = ""
        '受注進行ステータス
        work.WF_SEL_LINK_ORDERSTATUS.Text = ""
        '受注情報
        work.WF_SEL_LINK_INFORMATION.Text = ""
        '前回オーダー№
        work.WF_SEL_LINK_PREORDERNO.Text = ""
        '本線列車
        work.WF_SEL_LINK_TRAIN.Text = ""
        '本線列車名
        work.WF_SEL_LINK_TRAINNAME.Text = ""
        '担当営業所
        work.WF_SEL_LINK_ORDERSALESOFFICE.Text = ""
        '空車発駅(名)
        work.WF_SEL_LINK_DEPARTURESTATION.Text = ""
        '空車着駅(名)
        work.WF_SEL_LINK_ARRIVALSTATION.Text = ""
        '車数（レギュラー）
        work.WF_SEL_LINK_REGULAR_TANKCAR.Text = "0"
        '車数（ハイオク）
        work.WF_SEL_LINK_HIGHOCTANE_TANKCAR.Text = "0"
        '車数（灯油）
        work.WF_SEL_LINK_KEROSENE_TANKCAR.Text = "0"
        '車数（未添加灯油）
        work.WF_SEL_LINK_NOTADDED_KEROSENE_TANKCAR.Text = "0"
        '車数（軽油）
        work.WF_SEL_LINK_DIESEL_TANKCAR.Text = "0"
        '車数（３号軽油）
        work.WF_SEL_LINK_NUM3DIESEL_TANKCAR.Text = "0"
        '車数（５号軽油）
        work.WF_SEL_LINK_NUM5DIESEL_TANKCAR.Text = "0"
        '車数（１０号軽油）
        work.WF_SEL_LINK_NUM10DIESEL_TANKCAR.Text = "0"
        '車数（LSA）
        work.WF_SEL_LINK_LSA_TANKCAR.Text = "0"
        '車数（A重油）
        work.WF_SEL_LINK_AHEAVY_TANKCAR.Text = "0"
        '合計車数
        work.WF_SEL_LINK_TANKCARTOTAL.Text = "0"
        '空車着日（予定）
        work.WF_SEL_LINK_EMPARRDATE.Text = ""
        '空車着日（実績）
        work.WF_SEL_LINK_ACTUALEMPARRDATE.Text = ""

        '削除フラグ
        work.WF_SEL_LINK_DELFLG.Text = "0"
        '作成フラグ(貨車連結未使用：1, 貨車連結使用：2)
        work.WF_SEL_CREATELINKFLG.Text = "1"

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPLINKTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage(work.WF_SEL_CAMPCODE.Text + "1")

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

        ''〇 受注進行ステータスが"900(受注キャンセル)"の場合は何もしない
        'WW_ORDERSTATUS = OIT0003tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        'If WW_ORDERSTATUS = CONST_ORDERSTS_CAN Then
        '    Exit Sub
        'End If

        '選択行
        work.WF_SEL_LINK_LINECNT.Text = OIT0003tbl.Rows(WW_LINECNT)("LINECNT")
        '貨車連結順序表№
        work.WF_SEL_LINK_LINKNO.Text = OIT0003tbl.Rows(WW_LINECNT)("LINKNO")
        '登録日
        work.WF_SEL_LINK_REGISTRATIONDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("AVAILABLEYMD")
        '受注進行ステータス
        work.WF_SEL_LINK_ORDERSTATUS.Text = OIT0003tbl.Rows(WW_LINECNT)("STATUS")
        '受注情報
        work.WF_SEL_LINK_INFORMATION.Text = OIT0003tbl.Rows(WW_LINECNT)("INFO")
        '前回オーダー№
        work.WF_SEL_LINK_PREORDERNO.Text = OIT0003tbl.Rows(WW_LINECNT)("PREORDERNO")
        '本線列車
        work.WF_SEL_LINK_TRAIN.Text = OIT0003tbl.Rows(WW_LINECNT)("TRAINNO")
        '本線列車名
        work.WF_SEL_LINK_TRAINNAME.Text = OIT0003tbl.Rows(WW_LINECNT)("TRAINNAME")
        '担当営業所
        work.WF_SEL_LINK_ORDERSALESOFFICE.Text = OIT0003tbl.Rows(WW_LINECNT)("OFFICECODE")
        '空車発駅(名)
        work.WF_SEL_LINK_DEPARTURESTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("DEPSTATIONNAME")
        '空車着駅(名)
        work.WF_SEL_LINK_ARRIVALSTATION.Text = OIT0003tbl.Rows(WW_LINECNT)("RETSTATIONNAME")
        '車数（レギュラー）
        work.WF_SEL_LINK_REGULAR_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("RTANK")
        '車数（ハイオク）
        work.WF_SEL_LINK_HIGHOCTANE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("HTANK")
        '車数（灯油）
        work.WF_SEL_LINK_KEROSENE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("TTANK")
        '車数（未添加灯油）
        work.WF_SEL_LINK_NOTADDED_KEROSENE_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("MTTANK")
        '車数（軽油）
        work.WF_SEL_LINK_DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("KTANK")
        '車数（３号軽油）
        work.WF_SEL_LINK_NUM3DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K3TANK")
        '車数（５号軽油）
        work.WF_SEL_LINK_NUM5DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K5TANK")
        '車数（１０号軽油）
        work.WF_SEL_LINK_NUM10DIESEL_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("K10TANK")
        '車数（LSA）
        work.WF_SEL_LINK_LSA_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("LTANK")
        '車数（A重油）
        work.WF_SEL_LINK_AHEAVY_TANKCAR.Text = OIT0003tbl.Rows(WW_LINECNT)("ATANK")
        '合計車数
        work.WF_SEL_LINK_TANKCARTOTAL.Text = OIT0003tbl.Rows(WW_LINECNT)("TOTALTANK")
        '空車着日（予定）
        work.WF_SEL_LINK_EMPARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("EMPARRDATE")
        '空車着日（実績）
        work.WF_SEL_LINK_ACTUALEMPARRDATE.Text = OIT0003tbl.Rows(WW_LINECNT)("ACTUALEMPARRDATE")

        '削除フラグ
        work.WF_SEL_LINK_DELFLG.Text = OIT0003tbl.Rows(WW_LINECNT)("DELFLG")
        '作成フラグ(貨車連結未使用：1, 貨車連結使用：2)
        work.WF_SEL_CREATELINKFLG.Text = "2"

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
        'Master.SaveTable(OIT0003tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIT0003tbl, work.WF_SEL_INPLINKTBL.Text)

        '登録画面ページへ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

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

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
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
                Case "ORDERSTATUS"      '受注進行ステータス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERSTATUS"))
                Case "ORDERINFO"        '受注情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ORDERINFO"))
                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPLINKTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPLINKTBL.txt"

    End Sub

End Class
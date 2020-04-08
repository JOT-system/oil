'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 回送一覧画面
''' </summary>
''' <remarks></remarks>
Public Class OIT0006OutOfServiceList
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0006tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0006INPtbl As DataTable                              'チェック用テーブル
    Private OIT0006UPDtbl As DataTable                              '更新用テーブル
    Private OIT0006WKtbl As DataTable                               '作業用テーブル
    Private OIT0006Fixvaltbl As DataTable                           '作業用テーブル(固定値マスタ取得用)
    Private OIT0006His1tbl As DataTable                             '履歴格納用テーブル
    Private OIT0006His2tbl As DataTable                             '履歴格納用テーブル

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
    Private WW_KAISOUSTATUS As String = ""                           '受注進行ステータス

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIT0006tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonOUTOFSERVICE_CANCEL" 'キャンセルボタン押下
                            WF_ButtonOUTOFSERVICE_CANCEL_Click()
                        Case "WF_ButtonINSERT"          '回送新規作成ボタン押下
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
                        Case "btnCommonConfirmOk"       '確認メッセージ
                            WW_UpdateKaisouStatusCancel()
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
            If Not IsNothing(OIT0006tbl) Then
                OIT0006tbl.Clear()
                OIT0006tbl.Dispose()
                OIT0006tbl = Nothing
            End If

            If Not IsNothing(OIT0006INPtbl) Then
                OIT0006INPtbl.Clear()
                OIT0006INPtbl.Dispose()
                OIT0006INPtbl = Nothing
            End If

            If Not IsNothing(OIT0006UPDtbl) Then
                OIT0006UPDtbl.Clear()
                OIT0006UPDtbl.Dispose()
                OIT0006UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0006WRKINC.MAPIDL
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0006S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIT0006D Then
            Master.RecoverTable(OIT0006tbl, work.WF_SEL_INPTBL.Text)
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
        If Context.Handler.ToString().ToUpper() <> C_PREV_MAP_LIST.OIT0006D Then
            '○ 画面表示データ取得
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                MAPDataGet(SQLcon)
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(OIT0006tbl)

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

        If IsNothing(OIT0006tbl) Then
            OIT0006tbl = New DataTable
        End If

        If OIT0006tbl.Columns.Count <> 0 Then
            OIT0006tbl.Columns.Clear()
        End If

        OIT0006tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを受注テーブルから取得する

        Dim SQLStr As String =
              " SELECT" _
            & "   0                                                   AS LINECNT" _
            & " , ''                                                  AS OPERATION" _
            & " , CAST(OIT0006.UPDTIMSTP AS bigint)                   AS TIMSTP" _
            & " , 1                                                   AS 'SELECT'" _
            & " , 0                                                   AS HIDDEN" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUNO), '')   　            AS KAISOUNO" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUTYPE), '')   　          AS KAISOUTYPE" _
            & " , ISNULL(RTRIM(OIT0006.TRAINNO), '')                  AS TRAINNO" _
            & " , ISNULL(RTRIM(OIT0006.TRAINNAME), '')                AS TRAINNAME" _
            & " , ISNULL(FORMAT(OIT0006.KAISOUYMD, 'yyyy/MM/dd'), '') AS KAISOUYMD" _
            & " , ISNULL(RTRIM(OIT0006.OFFICECODE), '')               AS OFFICECODE" _
            & " , ISNULL(RTRIM(OIT0006.OFFICENAME), '')               AS OFFICENAME" _
            & " , ISNULL(RTRIM(OIT0006.SHIPPERSCODE), '')             AS SHIPPERSCODE" _
            & " , ISNULL(RTRIM(OIT0006.SHIPPERSNAME), '')             AS SHIPPERSNAME" _
            & " , ISNULL(RTRIM(OIT0006.BASECODE), '')                 AS BASECODE" _
            & " , ISNULL(RTRIM(OIT0006.BASENAME), '')                 AS BASENAME" _
            & " , ISNULL(RTRIM(OIT0006.CONSIGNEECODE), '')            AS CONSIGNEECODE" _
            & " , ISNULL(RTRIM(OIT0006.CONSIGNEENAME), '')            AS CONSIGNEENAME" _
            & " , ISNULL(RTRIM(OIT0006.DEPSTATION), '')               AS DEPSTATION" _
            & " , ISNULL(RTRIM(OIT0006.DEPSTATIONNAME), '')           AS DEPSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0006.ARRSTATION), '')               AS ARRSTATION" _
            & " , ISNULL(RTRIM(OIT0006.ARRSTATIONNAME), '')           AS ARRSTATIONNAME" _
            & " , ISNULL(RTRIM(OIT0006.OBJECTIVECODE), '')            AS OBJECTIVECODE" _
            & " , ''                                                  AS OBJECTIVENAME" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUSTATUS), '')             AS KAISOUSTATUS" _
            & " , ISNULL(RTRIM(OIS0015_1.VALUE1), '')                 AS KAISOUSTATUSNAME" _
            & " , ISNULL(RTRIM(OIT0006.KAISOUINFO), '')               AS KAISOUINFO" _
            & " , ISNULL(RTRIM(OIS0015_2.VALUE1), '')                 AS KAISOUINFONAME" _
            & " , ISNULL(RTRIM(OIT0006.FAREFLG), '')   　             AS FAREFLG" _
            & " , ISNULL(RTRIM(OIT0006.USEPROPRIETYFLG), '')   　     AS USEPROPRIETYFLG" _
            & " , ISNULL(RTRIM(OIT0006.DELIVERYFLG), '')   　         AS DELIVERYFLG" _
            & " , ISNULL(FORMAT(OIT0006.DEPDATE, 'yyyy/MM/dd'), '')           AS DEPDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALDEPDATE, 'yyyy/MM/dd'), '')     AS ACTUALDEPDATE" _
            & " , ISNULL(FORMAT(OIT0006.ARRDATE, 'yyyy/MM/dd'), '')           AS ARRDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALARRDATE, 'yyyy/MM/dd'), '')     AS ACTUALARRDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACCDATE, 'yyyy/MM/dd'), '')           AS ACCDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALACCDATE, 'yyyy/MM/dd'), '')     AS ACTUALACCDATE" _
            & " , ISNULL(FORMAT(OIT0006.EMPARRDATE, 'yyyy/MM/dd'), '')        AS EMPARRDATE" _
            & " , ISNULL(FORMAT(OIT0006.ACTUALEMPARRDATE, 'yyyy/MM/dd'), '')  AS ACTUALEMPARRDATE" _
            & " , ISNULL(RTRIM(OIT0006.TOTALTANK), '')   　           AS TOTALTANK" _
            & " , ISNULL(RTRIM(OIT0006.ORDERNO), '')                  AS ORDERNO" _
            & " , ISNULL(FORMAT(OIT0006.KEIJYOYMD, 'yyyy/MM/dd'), '')         AS KEIJYOYMD" _
            & " , ISNULL(RTRIM(OIT0006.SALSE), '')                   AS SALSE" _
            & " , ISNULL(RTRIM(OIT0006.SALSETAX), '')                AS SALSETAX" _
            & " , ISNULL(RTRIM(OIT0006.TOTALSALSE), '')              AS TOTALSALSE" _
            & " , ISNULL(RTRIM(OIT0006.PAYMENT), '')                 AS PAYMENT" _
            & " , ISNULL(RTRIM(OIT0006.PAYMENTTAX), '')              AS PAYMENTTAX" _
            & " , ISNULL(RTRIM(OIT0006.TOTALPAYMENT), '')            AS TOTALPAYMENT" _
            & " , ISNULL(RTRIM(OIT0006.DELFLG), '')                  AS DELFLG" _
            & " FROM OIL.OIT0006_KAISOU OIT0006 " _
            & "  INNER JOIN OIL.VIW0003_OFFICECHANGE VIW0003 ON " _
            & "        VIW0003.ORGCODE    = @P1 " _
            & "    AND VIW0003.OFFICECODE = OIT0006.OFFICECODE " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_1 ON " _
            & "        OIS0015_1.CLASS   = 'KAISOUSTATUS' " _
            & "    AND OIS0015_1.KEYCODE = OIT0006.KAISOUSTATUS " _
            & "  LEFT JOIN com.OIS0015_FIXVALUE OIS0015_2 ON " _
            & "        OIS0015_2.CLASS   = 'KAISOUINFO' " _
            & "    AND OIS0015_2.KEYCODE = OIT0006.KAISOUINFO " _
            & " WHERE OIT0006.DELFLG     <> @P3" _
            & "   AND OIT0006.DEPDATE    >= @P2"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '営業所
        If Not String.IsNullOrEmpty(work.WF_SEL_SALESOFFICECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0006.OFFICECODE = '{0}'", work.WF_SEL_SALESOFFICECODE.Text)
        End If
        '列車番号
        If Not String.IsNullOrEmpty(work.WF_SEL_TRAINNUMBER.Text) Then
            SQLStr &= String.Format("    AND OIT0006.TRAINNO = '{0}'", work.WF_SEL_TRAINNUMBER.Text)
        End If
        '状態(回送進行ステータス)
        If Not String.IsNullOrEmpty(work.WF_SEL_STATUSCODE.Text) Then
            SQLStr &= String.Format("    AND OIT0006.KAISOUSTATUS = '{0}'", work.WF_SEL_STATUSCODE.Text)
        End If
        '目的
        If Not String.IsNullOrEmpty(work.WF_SEL_OBJECTIVECODE.Text) Then
            SQLStr &= String.Format("    AND OIT0006.OBJECTIVECODE = '{0}'", work.WF_SEL_OBJECTIVECODE.Text)
        End If
        '着駅
        If Not String.IsNullOrEmpty(work.WF_SEL_ARRIVALSTATION.Text) Then
            SQLStr &= String.Format("    AND OIT0006.ARRSTATION = '{0}'", work.WF_SEL_ARRIVALSTATION.Text)
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIT0006.KAISOUNO"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar)     '組織コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.DateTime)     '年月日(開始)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = Master.USER_ORG
                PARA2.Value = work.WF_SEL_DATE.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIT0006row As DataRow In OIT0006tbl.Rows
                    i += 1
                    OIT0006row("LINECNT") = i        'LINECNT

                    '◯名称取得
                    '目的
                    CODENAME_get("OBJECTIVECODE", OIT0006row("OBJECTIVECODE"), OIT0006row("OBJECTIVENAME"), WW_RTN_SW)

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006L Select"
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
        Master.RecoverTable(OIT0006tbl)

        'チェックボックス判定
        For i As Integer = 0 To OIT0006tbl.Rows.Count - 1
            If OIT0006tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If OIT0006tbl.Rows(i)("OPERATION") = "" Then
                    If (OIT0006tbl.Rows(i)("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_900 _
                             OrElse OIT0006tbl.Rows(i)("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_500 _
                             OrElse OIT0006tbl.Rows(i)("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_550 _
                             OrElse OIT0006tbl.Rows(i)("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_600 _
                             OrElse OIT0006tbl.Rows(i)("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_700 _
                             OrElse OIT0006tbl.Rows(i)("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_800) Then
                        OIT0006tbl.Rows(i)("OPERATION") = ""

                    Else
                        OIT0006tbl.Rows(i)("OPERATION") = "on"

                    End If
                Else
                    OIT0006tbl.Rows(i)("OPERATION") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0006tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0006tbl.Rows.Count - 1
            If OIT0006tbl.Rows(i)("HIDDEN") = "0" AndAlso OIT0006tbl.Rows(i)("KAISOUSTATUS") <> BaseDllConst.CONST_KAISOUSTATUS_900 Then
                OIT0006tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0006tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0006tbl.Rows.Count - 1
            If OIT0006tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0006tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

    End Sub

    ''' <summary>
    ''' キャンセルボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonOUTOFSERVICE_CANCEL_Click()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '件数を取得
        intTblCnt = OIT0006tbl.Rows.Count

        '行が選択されているかチェック
        For Each OIT0006UPDrow In OIT0006tbl.Rows
            If OIT0006UPDrow("OPERATION") = "on" Then
                If OIT0006UPDrow("KAISOUSTATUS") <> BaseDllConst.CONST_KAISOUSTATUS_900 Then
                    SelectChk = True
                End If
            End If
        Next

        '○メッセージ表示
        '一覧件数が０件の時のキャンセルの場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.OIL_CANCELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub

            '一覧件数が１件以上で未選択によるキャンセルの場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.OIL_CANCELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '◯確認メッセージ(回送キャンセルの確認)
        Master.Output(C_MESSAGE_NO.OIL_CONFIRM_CANCEL_KAISOU,
                      C_MESSAGE_TYPE.QUES,
                      needsPopUp:=True,
                      messageBoxTitle:="",
                      IsConfirm:=True)

    End Sub

    ''' <summary>
    ''' 回送新規作成ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SEL_LINECNT.Text = ""
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = ""
        '回送営業所(名)
        work.WF_SEL_KAISOUSALESOFFICE.Text = ""
        '回送営業所(コード)
        work.WF_SEL_KAISOUSALESOFFICECODE.Text = ""
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
        '運賃フラグ
        work.WF_SEL_FAREFLG.Text = "2"

        '回送進行ステータス(名)
        CODENAME_get("KAISOUSTATUS", BaseDllConst.CONST_KAISOUSTATUS_100, work.WF_SEL_KAISOUSTATUSNM.Text, WW_RTN_SW)
        '回送進行ステータス(コード)
        work.WF_SEL_KAISOUSTATUS.Text = BaseDllConst.CONST_KAISOUSTATUS_100
        '回送情報(名)
        work.WF_SEL_INFORMATIONNM.Text = ""
        '回送情報(コード)
        work.WF_SEL_INFORMATION.Text = ""

        '回送№
        work.WF_SEL_KAISOUNUMBER.Text = ""
        '回送目的(名)
        work.WF_SEL_OBJECTIVENAME.Text = ""
        '回送目的(コード)
        work.WF_SEL_OBJECTIVECODE.Text = ""
        '本線列車
        work.WF_SEL_TRAIN.Text = ""
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = ""

        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = ""
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = ""
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = ""
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = ""
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = "0"

        '発日(予定)
        work.WF_SEL_DEPDATE.Text = ""
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = ""
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = ""
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = ""
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = ""
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = ""
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = ""
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = ""
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = ""

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
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "1"
        '託送指示フラグ(0：未手配, 1:手配)
        work.WF_SEL_DELIVERYFLG.Text = "0"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

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

        '〇 回送進行ステータスが"900(回送キャンセル)"の場合は何もしない
        WW_KAISOUSTATUS = OIT0006tbl.Rows(WW_LINECNT)("KAISOUSTATUS")
        If WW_KAISOUSTATUS = BaseDllConst.CONST_KAISOUSTATUS_900 Then
            Master.Output(C_MESSAGE_NO.OIL_CANCEL_ENTRY_OUTOFSERVICE, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '選択行
        work.WF_SEL_LINECNT.Text = OIT0006tbl.Rows(WW_LINECNT)("LINECNT")
        '登録日
        work.WF_SEL_REGISTRATIONDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUYMD")
        '回送営業所(名)
        work.WF_SEL_KAISOUSALESOFFICE.Text = OIT0006tbl.Rows(WW_LINECNT)("OFFICENAME")
        '回送営業所(コード)
        work.WF_SEL_KAISOUSALESOFFICECODE.Text = OIT0006tbl.Rows(WW_LINECNT)("OFFICECODE")
        '荷主(名)
        work.WF_SEL_SHIPPERSNAME.Text = OIT0006tbl.Rows(WW_LINECNT)("SHIPPERSNAME")
        '荷主(コード)
        work.WF_SEL_SHIPPERSCODE.Text = OIT0006tbl.Rows(WW_LINECNT)("SHIPPERSCODE")
        '荷受人(名)
        work.WF_SEL_CONSIGNEENAME.Text = OIT0006tbl.Rows(WW_LINECNT)("CONSIGNEENAME")
        '荷受人(コード)
        work.WF_SEL_CONSIGNEECODE.Text = OIT0006tbl.Rows(WW_LINECNT)("CONSIGNEECODE")
        'パターンコード(名)
        'work.WF_SEL_PATTERNNAME.Text = ""
        CODENAME_get("KAISOUTYPE", OIT0006tbl.Rows(WW_LINECNT)("KAISOUTYPE"), work.WF_SEL_PATTERNNAME.Text, WW_RTN_SW)
        'パターンコード
        work.WF_SEL_PATTERNCODE.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUTYPE")
        '運賃フラグ
        work.WF_SEL_FAREFLG.Text = OIT0006tbl.Rows(WW_LINECNT)("FAREFLG")

        '回送進行ステータス(名)
        work.WF_SEL_KAISOUSTATUSNM.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUSTATUSNAME")
        '回送進行ステータス(コード)
        work.WF_SEL_KAISOUSTATUS.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUSTATUS")
        '回送情報(名)
        work.WF_SEL_INFORMATIONNM.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUINFONAME")
        '回送情報(コード)
        work.WF_SEL_INFORMATION.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUINFO")

        '回送№
        work.WF_SEL_KAISOUNUMBER.Text = OIT0006tbl.Rows(WW_LINECNT)("KAISOUNO")
        '回送目的(名)
        work.WF_SEL_OBJECTIVENAME.Text = OIT0006tbl.Rows(WW_LINECNT)("OBJECTIVENAME")
        '回送目的(コード)
        work.WF_SEL_OBJECTIVECODE.Text = OIT0006tbl.Rows(WW_LINECNT)("OBJECTIVECODE")
        '本線列車
        work.WF_SEL_TRAIN.Text = OIT0006tbl.Rows(WW_LINECNT)("TRAINNO")
        '本線列車名
        work.WF_SEL_TRAINNAME.Text = OIT0006tbl.Rows(WW_LINECNT)("TRAINNAME")

        '発駅(名)
        work.WF_SEL_DEPARTURESTATIONNM.Text = OIT0006tbl.Rows(WW_LINECNT)("DEPSTATIONNAME")
        '発駅(コード)
        work.WF_SEL_DEPARTURESTATION.Text = OIT0006tbl.Rows(WW_LINECNT)("DEPSTATION")
        '着駅(名)
        work.WF_SEL_ARRIVALSTATIONNM.Text = OIT0006tbl.Rows(WW_LINECNT)("ARRSTATIONNAME")
        '着駅(コード)
        work.WF_SEL_ARRIVALSTATION.Text = OIT0006tbl.Rows(WW_LINECNT)("ARRSTATION")
        '合計車数
        work.WF_SEL_TANKCARTOTAL.Text = OIT0006tbl.Rows(WW_LINECNT)("TOTALTANK")

        '発日(予定)
        work.WF_SEL_DEPDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("DEPDATE")
        '着日(予定)
        work.WF_SEL_ARRDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("ARRDATE")
        '受入日(予定)
        work.WF_SEL_ACCDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("ACCDATE")
        '空車着日(予定)
        work.WF_SEL_EMPARRDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("EMPARRDATE")
        '発日(実績)
        work.WF_SEL_ACTUALDEPDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("ACTUALDEPDATE")
        '着日(実績)
        work.WF_SEL_ACTUALARRDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("ACTUALARRDATE")
        '受入日(実績)
        work.WF_SEL_ACTUALACCDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("ACTUALACCDATE")
        '空車着日(実績)
        work.WF_SEL_ACTUALEMPARRDATE.Text = OIT0006tbl.Rows(WW_LINECNT)("ACTUALEMPARRDATE")
        '受注№
        work.WF_SEL_ORDERNUMBER.Text = OIT0006tbl.Rows(WW_LINECNT)("ORDERNO")

        '計上年月日
        work.WF_SEL_KEIJYOYMD.Text = OIT0006tbl.Rows(WW_LINECNT)("KEIJYOYMD")
        '売上金額
        work.WF_SEL_SALSE.Text = OIT0006tbl.Rows(WW_LINECNT)("SALSE")
        '売上消費税額
        work.WF_SEL_SALSETAX.Text = OIT0006tbl.Rows(WW_LINECNT)("SALSETAX")
        '売上合計金額
        work.WF_SEL_TOTALSALSE.Text = OIT0006tbl.Rows(WW_LINECNT)("TOTALSALSE")
        '支払金額
        work.WF_SEL_PAYMENT.Text = OIT0006tbl.Rows(WW_LINECNT)("PAYMENT")
        '支払消費税額
        work.WF_SEL_PAYMENTTAX.Text = OIT0006tbl.Rows(WW_LINECNT)("PAYMENTTAX")
        '支払合計金額
        work.WF_SEL_TOTALPAYMENT.Text = OIT0006tbl.Rows(WW_LINECNT)("TOTALPAYMENT")

        '削除フラグ
        work.WF_SEL_DELFLG.Text = OIT0006tbl.Rows(WW_LINECNT)("DELFLG")
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "2"
        '託送指示フラグ(0：未手配, 1:手配)
        work.WF_SEL_DELIVERYFLG.Text = OIT0006tbl.Rows(WW_LINECNT)("DELIVERYFLG")

        '○ 状態をクリア
        For Each OIT0006row As DataRow In OIT0006tbl.Rows
            Select Case OIT0006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case OIT0006tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                OIT0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                OIT0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                OIT0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                OIT0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                OIT0006tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(OIT0006tbl, work.WF_SEL_INPTBL.Text)

        '回送明細画面ページへ遷移
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
        For Each OIT0006row As DataRow In OIT0006tbl.Rows
            If OIT0006row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0006row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(OIT0006tbl)

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
    ''' (回送TBL)回送進行ステータス(回送キャンセル)更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateKaisouStatusCancel()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0006tbl)

        '■■■ OIT0006tbl関連の受注TBLの「回送進行ステータス」を「900:回送キャンセル」に更新 ■■■

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･回送TBLを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0006_KAISOU " _
                    & "    SET UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14, " _
                    & "        KAISOUSTATUS = @P15  " _
                    & "  WHERE KAISOUNO     = @P01  " _
                    & "    AND DELFLG      <> '1';"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            '回送キャンセルする情報取得用
            Dim strKaisouSts As String = ""         '回送進行ステータス
            Dim strDepstation As String = ""        '発駅コード
            Dim strArrstation As String = ""        '着駅コード
            'Dim strLinkNoMade As String = ""        '作成_貨車連結順序表№

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)
            Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)

            '選択されている行の受注進行ステータスを「900:回送キャンセル」に更新
            For Each OIT0006UPDrow In OIT0006tbl.Rows
                If OIT0006UPDrow("OPERATION") = "on" Then
                    PARA01.Value = OIT0006UPDrow("KAISOUNO")
                    work.WF_SEL_KAISOUNUMBER.Text = OIT0006UPDrow("KAISOUNO")
                    strKaisouSts = OIT0006UPDrow("KAISOUSTATUS")
                    strDepstation = OIT0006UPDrow("DEPSTATION")
                    strArrstation = OIT0006UPDrow("ARRSTATION")
                    'strLinkNoMade = OIT0006UPDrow("TANKLINKNOMADE")

                    PARA11.Value = Date.Now
                    PARA12.Value = Master.USERID
                    PARA13.Value = Master.USERTERMID
                    PARA14.Value = C_DEFAULT_YMD
                    PARA15.Value = BaseDllConst.CONST_KAISOUSTATUS_900

                    OIT0006UPDrow("KAISOUSTATUS") = BaseDllConst.CONST_KAISOUSTATUS_900
                    CODENAME_get("KAISOUSTATUS", OIT0006UPDrow("KAISOUSTATUS"), OIT0006UPDrow("KAISOUSTATUSNAME"), WW_DUMMY)

                    SQLcmd.ExecuteNonQuery()
                End If
            Next

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            '### START 回送履歴テーブルの追加(2020/04/08) #############
            WW_InsertKaisouHistory(SQLcon)
            '### END   ################################################

            '### START 回送キャンセル時のタンク車所在の更新処理を追加(2020/03/31) ###############################
            For Each OIT0006His2tblrow In OIT0006His2tbl.Rows
                Select Case strKaisouSts
                    Case BaseDllConst.CONST_KAISOUSTATUS_100

                        '### 何もしない####################

                    '200:手配　～　310：手配完了
                    Case BaseDllConst.CONST_KAISOUSTATUS_200,
                         BaseDllConst.CONST_KAISOUSTATUS_210,
                         BaseDllConst.CONST_KAISOUSTATUS_250,
                         BaseDllConst.CONST_KAISOUSTATUS_300
                        '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                        '引数１：所在地コード　⇒　変更なし(空白)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更なし(空白)
                        WW_UpdateTankShozai("", "3", "", I_TANKNO:=OIT0006His2tblrow("TANKNO"))

                    '350：受注確定
                    Case BaseDllConst.CONST_KAISOUSTATUS_350
                        '★タンク車所在の更新(タンク車№を再度選択できるようにするため)
                        '引数１：所在地コード　⇒　変更あり(発駅)
                        '引数２：タンク車状態　⇒　変更あり("3"(到着))
                        '引数３：積車区分　　　⇒　変更なし(空白)
                        WW_UpdateTankShozai(strDepstation, "3", "", I_TANKNO:=OIT0006His2tblrow("TANKNO"))

                    '400：受入確認中, 450:受入確認中(受入日入力)
                    Case BaseDllConst.CONST_KAISOUSTATUS_400,
                         BaseDllConst.CONST_KAISOUSTATUS_450

                        '### 何もしない####################

                    '※"500：検収中"のステータス以降についてはキャンセルができない仕様だが
                    '　条件は追加しておく
                    Case BaseDllConst.CONST_KAISOUSTATUS_500,
                         BaseDllConst.CONST_KAISOUSTATUS_550,
                         BaseDllConst.CONST_KAISOUSTATUS_600,
                         BaseDllConst.CONST_KAISOUSTATUS_700,
                         BaseDllConst.CONST_KAISOUSTATUS_800,
                         BaseDllConst.CONST_KAISOUSTATUS_900

                        '### 何もしない####################

                End Select
            Next

            '回送進行ステータスの状態によって、貨車連結順序表を利用不可にする。
            'Select Case strKaisouSts
            '    Case BaseDllConst.CONST_KAISOUSTATUS_350,
            '         BaseDllConst.CONST_KAISOUSTATUS_400,
            '         BaseDllConst.CONST_KAISOUSTATUS_450

            '        WW_UpdateLink(strLinkNoMade, "2")

            '    Case BaseDllConst.CONST_KAISOUSTATUS_100,
            '         BaseDllConst.CONST_KAISOUSTATUS_200,
            '         BaseDllConst.CONST_KAISOUSTATUS_210,
            '         BaseDllConst.CONST_KAISOUSTATUS_250,
            '         BaseDllConst.CONST_KAISOUSTATUS_300,
            '         BaseDllConst.CONST_KAISOUSTATUS_500,
            '         BaseDllConst.CONST_KAISOUSTATUS_550,
            '         BaseDllConst.CONST_KAISOUSTATUS_600,
            '         BaseDllConst.CONST_KAISOUSTATUS_700,
            '         BaseDllConst.CONST_KAISOUSTATUS_800,
            '         BaseDllConst.CONST_KAISOUSTATUS_900

            '        '### 何もしない####################

            'End Select
            '### END  ###########################################################################################

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○ 画面表示データ保存
        Master.SaveTable(OIT0006tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' (タンク車所在TBL)所在地の内容を更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateTankShozai(ByVal I_LOCATION As String,
                                      ByVal I_STATUS As String,
                                      ByVal I_KBN As String,
                                      Optional ByVal I_TANKNO As String = Nothing)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '更新SQL文･･･受注TBLの託送指示フラグを更新
            Dim SQLStr As String =
                    " UPDATE OIL.OIT0005_SHOZAI " _
                    & "    SET "

            '○ 更新内容が指定されていれば追加する
            '所在地コード
            If Not String.IsNullOrEmpty(I_LOCATION) Then
                SQLStr &= String.Format("        LOCATIONCODE = '{0}', ", I_LOCATION)
            End If
            'タンク車状態コード
            If Not String.IsNullOrEmpty(I_STATUS) Then
                SQLStr &= String.Format("        TANKSTATUS   = '{0}', ", I_STATUS)
            End If
            '積車区分
            If Not String.IsNullOrEmpty(I_KBN) Then
                SQLStr &= String.Format("        LOADINGKBN   = '{0}', ", I_KBN)
            End If
            ''空車着日（予定）
            'If upEmparrDate = True Then
            '    SQLStr &= String.Format("        EMPARRDATE   = '{0}', ", I_EmparrDate)
            '    SQLStr &= String.Format("        ACTUALEMPARRDATE   = {0}, ", "NULL")
            'End If
            ''空車着日（実績）
            'If upActualEmparrDate = True Then
            '    SQLStr &= String.Format("        ACTUALEMPARRDATE   = '{0}', ", I_ActualEmparrDate)
            'End If

            SQLStr &=
                      "        UPDYMD       = @P11, " _
                    & "        UPDUSER      = @P12, " _
                    & "        UPDTERMID    = @P13, " _
                    & "        RECEIVEYMD   = @P14  " _
                    & "  WHERE TANKNUMBER   = @P01  " _
                    & "    AND DELFLG      <> @P02; "

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)  'タンク車№
            Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)  '削除フラグ

            Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.DateTime)
            Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
            Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
            Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.DateTime)

            PARA02.Value = C_DELETE_FLG.DELETE

            PARA11.Value = Date.Now
            PARA12.Value = Master.USERID
            PARA13.Value = Master.USERTERMID
            PARA14.Value = C_DEFAULT_YMD

            If I_TANKNO = "" Then
                '(一覧)で設定しているタンク車をKEYに更新
                For Each OIT0006row As DataRow In OIT0006tbl.Rows
                    PARA01.Value = OIT0006row("TANKNO")
                    SQLcmd.ExecuteNonQuery()
                Next
            Else
                '指定されたタンク車№をKEYに更新
                PARA01.Value = I_TANKNO
                SQLcmd.ExecuteNonQuery()

            End If

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006L_TANKSHOZAI UPDATE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006L_TANKSHOZAI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' 回送履歴TBL追加処理
    ''' </summary>
    ''' <param name="sqlCon">SQL接続</param>
    Private Sub WW_InsertKaisouHistory(ByVal SQLcon As SqlConnection)
        Dim WW_GetHistoryNo() As String = {""}
        WW_FixvalueMasterSearch("", "NEWHISTORYNOGET", "", WW_GetHistoryNo)

        '◯回送履歴テーブル格納用
        If IsNothing(OIT0006His1tbl) Then
            OIT0006His1tbl = New DataTable
        End If

        If OIT0006His1tbl.Columns.Count <> 0 Then
            OIT0006His1tbl.Columns.Clear()
        End If
        OIT0006His1tbl.Clear()

        '◯回送明細履歴テーブル格納用
        If IsNothing(OIT0006His2tbl) Then
            OIT0006His2tbl = New DataTable
        End If

        If OIT0006His2tbl.Columns.Count <> 0 Then
            OIT0006His2tbl.Columns.Clear()
        End If
        OIT0006His2tbl.Clear()

        '○ 回送TBL検索SQL
        Dim SQLOrderStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0006.*" _
            & " FROM OIL.OIT0006_KAISOU OIT0006 " _
            & String.Format(" WHERE OIT0006.KAISOUNO = '{0}'", work.WF_SEL_KAISOUNUMBER.Text)

        '○ 回送明細TBL検索SQL
        Dim SQLOrderDetailStr As String =
            "SELECT " _
            & String.Format("   '{0}' AS HISTORYNO", WW_GetHistoryNo(0)) _
            & String.Format(" , '{0}' AS MAPID", Me.Title) _
            & " , OIT0007.*" _
            & " FROM OIL.OIT0007_KAISOUDETAIL OIT0007 " _
            & String.Format(" WHERE OIT0007.KAISOUNO = '{0}'", work.WF_SEL_KAISOUNUMBER.Text)

        Try
            Using SQLcmd As New SqlCommand(SQLOrderStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006His1tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006His1tbl.Load(SQLdr)
                End Using
            End Using

            Using SQLcmd As New SqlCommand(SQLOrderDetailStr, SQLcon)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIT0006His2tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006His2tbl.Load(SQLdr)
                End Using
            End Using

            Using tran = SQLcon.BeginTransaction
                '■回送履歴テーブル
                EntryHistory.InsertKaisouHistory(SQLcon, tran, OIT0006His1tbl.Rows(0))

                '■回送明細履歴テーブル
                For Each OIT0001His2rowtbl In OIT0006His2tbl.Rows
                    EntryHistory.InsertKaisouDetailHistory(SQLcon, tran, OIT0001His2rowtbl)
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006L KAISOUHISTORY")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006L KAISOUHISTORY"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' マスタ検索処理
    ''' </summary>
    ''' <param name="I_CODE"></param>
    ''' <param name="I_CLASS"></param>
    ''' <param name="I_KEYCODE"></param>
    ''' <param name="O_VALUE"></param>
    Protected Sub WW_FixvalueMasterSearch(ByVal I_CODE As String,
                                          ByVal I_CLASS As String,
                                          ByVal I_KEYCODE As String,
                                          ByRef O_VALUE() As String,
                                          Optional ByVal I_PARA01 As String = Nothing)

        If IsNothing(OIT0006Fixvaltbl) Then
            OIT0006Fixvaltbl = New DataTable
        End If

        If OIT0006Fixvaltbl.Columns.Count <> 0 Then
            OIT0006Fixvaltbl.Columns.Clear()
        End If

        OIT0006Fixvaltbl.Clear()

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            Dim SQLStr As String =
               " SELECT" _
                & "   ISNULL(RTRIM(VIW0001.CAMPCODE), '')    AS CAMPCODE" _
                & " , ISNULL(RTRIM(VIW0001.CLASS), '')       AS CLASS" _
                & " , ISNULL(RTRIM(VIW0001.KEYCODE), '')     AS KEYCODE" _
                & " , ISNULL(RTRIM(VIW0001.STYMD), '')       AS STYMD" _
                & " , ISNULL(RTRIM(VIW0001.ENDYMD), '')      AS ENDYMD" _
                & " , ISNULL(RTRIM(VIW0001.VALUE1), '')      AS VALUE1" _
                & " , ISNULL(RTRIM(VIW0001.VALUE2), '')      AS VALUE2" _
                & " , ISNULL(RTRIM(VIW0001.VALUE3), '')      AS VALUE3" _
                & " , ISNULL(RTRIM(VIW0001.VALUE4), '')      AS VALUE4" _
                & " , ISNULL(RTRIM(VIW0001.VALUE5), '')      AS VALUE5" _
                & " , ISNULL(RTRIM(VIW0001.VALUE6), '')      AS VALUE6" _
                & " , ISNULL(RTRIM(VIW0001.VALUE7), '')      AS VALUE7" _
                & " , ISNULL(RTRIM(VIW0001.VALUE8), '')      AS VALUE8" _
                & " , ISNULL(RTRIM(VIW0001.VALUE9), '')      AS VALUE9" _
                & " , ISNULL(RTRIM(VIW0001.VALUE10), '')     AS VALUE10" _
                & " , ISNULL(RTRIM(VIW0001.VALUE11), '')     AS VALUE11" _
                & " , ISNULL(RTRIM(VIW0001.VALUE12), '')     AS VALUE12" _
                & " , ISNULL(RTRIM(VIW0001.VALUE13), '')     AS VALUE13" _
                & " , ISNULL(RTRIM(VIW0001.VALUE14), '')     AS VALUE14" _
                & " , ISNULL(RTRIM(VIW0001.VALUE15), '')     AS VALUE15" _
                & " , ISNULL(RTRIM(VIW0001.SYSTEMKEYFLG), '')   AS SYSTEMKEYFLG" _
                & " , ISNULL(RTRIM(VIW0001.DELFLG), '')      AS DELFLG" _
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
                        OIT0006Fixvaltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIT0006Fixvaltbl.Load(SQLdr)
                End Using

                If I_KEYCODE.Equals("") Then
                    'Dim i As Integer = 0 '2020/3/23 三宅 Delete
                    For Each OIT0006WKrow As DataRow In OIT0006Fixvaltbl.Rows '(全抽出結果回るので要検討
                        'O_VALUE(i) = OIT0006WKrow("KEYCODE") 2020/3/23 三宅 全部KEYCODE(列車NO)が格納されてしまうので修正しました（問題なければこのコメント消してください)
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0006WKrow("VALUE" & i.ToString())
                        Next
                        'i += 1 '2020/3/23 三宅 Delete
                    Next
                Else
                    For Each OIT0006WKrow As DataRow In OIT0006Fixvaltbl.Rows
                        For i = 1 To O_VALUE.Length
                            O_VALUE(i - 1) = OIT0006WKrow("VALUE" & i.ToString())
                        Next
                    Next
                End If
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIT0006D MASTER_SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIT0006D MASTER_SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try
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

                Case "KAISOUSTATUS"     '回送進行ステータス
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUSTATUS, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUSTATUS"))

                Case "KAISOUINFO"       '回送情報
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUINFO, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUINFO"))

                Case "SALESOFFICE"      '営業所
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SALESOFFICE"))

                Case "KAISOUTYPE"       '回送パターン
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KAISOUTYPE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "KAISOUPATTERN"))

                Case "OBJECTIVECODE"    '目的
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "OBJECTIVECODE"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

                Case "TANKNO"           'タンク車
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_TANKNUMBER, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TANKNO"))

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
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

End Class
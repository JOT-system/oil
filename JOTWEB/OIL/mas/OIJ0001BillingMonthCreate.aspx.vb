Imports JOTWEB.GRIS0005LeftBox
Imports System.Data.SqlClient
''' <summary>
''' 費用明細(月単位)作成画面
''' </summary>
''' <remarks></remarks>
Public Class OIJ0001BillingMonthCreate
    Inherits System.Web.UI.Page
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0052DetailView As New CS0052DetailView                'Repeterオブジェクト作成

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            '○ 各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value
                    'Case "WF_ButtonDO"                  '検索ボタン押下
                    '    WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                 '戻るボタン押下
                        WF_ButtonEND_Click()
                    Case "WF_ButtonINSERT"              '受注費用明細ボタン押下
                        WF_ButtonINSERT_Click()
                    Case "WF_Field_DBClick"             'フィールドダブルクリック
                        WF_FIELD_DBClick()
                    Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                        WF_FIELD_Change()
                    Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                        WF_ButtonSel_Click()
                    Case "WF_RIGHT_VIEW_DBClick"        '右ボックスダブルクリック
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                'メモ欄更新
                        WF_RIGHTBOX_Change()
                    Case "HELP"                         'ヘルプ表示
                        WF_HELP_Click()
                End Select
            End If
        Else
            '○ 初期化処理
            Initialize()
        End If
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""
        Master.MAPID = OIJ0001WRKINC.MAPIDC
        leftview.ActiveListBox()

        '○ 画面の値設定
        WW_MAPValueSet()
    End Sub
    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then         'メニューからの画面遷移
            '〇画面間の情報クリア
            work.Initialize()

            '〇初期変数設定処理
            '会社コード
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)
            '運用部署
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "UORG", WF_UORG.Text)
            '計上年月
            Master.GetFirstValue(work.WF_SEL_CAMPCODE.Text, "KEIJYOYM", Me.TxtKeijyoYM.Text)
            Try
                Dim dtKeijyoYM As Date = Date.Parse(Me.TxtKeijyoYM.Text)
                Me.TxtKeijyoYM.Text = dtKeijyoYM.ToString("yyyy/MM")
            Catch ex As Exception
                Me.TxtKeijyoYM.Text = Now.ToString("yyyy/MM")
            End Try
        End If

        '計上年月を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtKeijyoYM.Attributes("onkeyPress") = "CheckCalendar()"

        '○ RightBox情報設定
        rightview.MAPIDS = OIJ0001WRKINC.MAPIDC
        rightview.MAPID = OIJ0001WRKINC.MAPIDC
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW

        rightview.MENUROLE = Master.ROLE_MENU
        rightview.MAPROLE = Master.ROLE_MAP
        rightview.VIEWROLE = Master.ROLE_VIEWPROF
        rightview.RPRTROLE = Master.ROLE_RPRTPROF

        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 検索ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.TransitionPrevPage()

    End Sub
    ''' <summary>
    ''' 受注費用明細ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()
        '受注費用明細作成処理
        Using SQLcon As SqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            WW_ORDERDETAILBILLING(SQLcon)
        End Using
    End Sub
    ''' <summary>
    ''' 受注費用明細作成処理
    ''' </summary>
    ''' <remarks></remarks>
    ''' <param name="SQLcon">SQL接続文字</param>
    Protected Sub WW_ORDERDETAILBILLING(ByVal SQLcon As SqlConnection)
        Try
            '追加SQL文･･･受注費用明細TBLに月単位で明細を作成
            Dim SQLCmn1Str As String = ""
            Dim SQLCmn2Str As String = ""
            Dim SQLAct1Str As String = ""
            Dim SQLAct2Str As String = ""
            Dim SQLStr As String =
                    String.Format(" DELETE FROM OIL.OIT0013_ORDERDETAILBILLINGMTH WHERE BILLINGMONTH='{0}'; ", Me.TxtKeijyoYM.Text)

            '○INSERT分(投入用TBL)
            SQLStr &=
                  " INSERT INTO oil.OIT0013_ORDERDETAILBILLINGMTH " _
                & " (" _
                & "   BILLINGMONTH, BILLINGNO, ORDERNO, DETAILNO, PATCODE, PATNAME" _
                & " , ACCOUNTCODE, ACCOUNTNAME, SEGMENTCODE, SEGMENTNAME, BREAKDOWNCODE, BREAKDOWN" _
                & " , SHIPPERSCODE, SHIPPERSNAME, BASECODE, BASENAME, OFFICECODE, OFFICENAME" _
                & " , DEPSTATION, DEPSTATIONNAME, ARRSTATION, ARRSTATIONNAME, CONSIGNEECODE, CONSIGNEENAME" _
                & " , KEIJYOYMD, TRAINNO, TRAINNAME, MODEL, TANKNO, OTTRANSPORTFLG, CARSNUMBER, CARSAMOUNT" _
                & " , LOAD, OILCODE, OILNAME, ORDERINGTYPE, ORDERINGOILNAME, CHANGETRAINNO, CHANGETRAINNAME" _
                & " , SECONDCONSIGNEECODE, SECONDCONSIGNEENAME, SECONDARRSTATION, SECONDARRSTATIONNAME" _
                & " , CHANGERETSTATION, CHANGERETSTATIONNAME, TRKBN, TRKBNNAME, KIRO, CALCKBN, CALCKBNNAME" _
                & " , JROILTYPE, CHARGE, DISCOUNT1, DISCOUNT2, DISCOUNT3, DISCOUNT4, DISCOUNT5, DISCOUNT6, DISCOUNT7" _
                & " , APPLYCHARGE, AMOUNT, TAX, CONSUMPTIONTAX, INVOICECODE, INVOICENAME, INVOICEDEPTNAME" _
                & " , PAYEECODE, PAYEENAME, PAYEEDEPTNAME, DELFLG, INITYMD, INITUSER, INITTERMID, UPDYMD, UPDUSER, UPDTERMID, RECEIVEYMD" _
                & " )"

            '★共通1SQL
            SQLCmn1Str =
                  " SELECT" _
                & "    FORMAT(CONVERT(DATE,OIT0003.ACTUALLODDATE),'yyyy/MM') AS BILLINGMONTH" _
                & "  , ISNULL(OIT0002.BILLINGNO, '') AS BILLINGNO" _
                & "  , ISNULL(RTRIM(OIT0002.ORDERNO), '') AS ORDERNO" _
                & "  , ISNULL(RTRIM(OIT0003.DETAILNO), '') AS DETAILNO" _
                & "  , ISNULL(RTRIM(VIW0012.PATCODE), '') AS PATCODE" _
                & "  , ISNULL(RTRIM(VIW0012.PATNAME), '') AS PATNAME" _
                & "  , ISNULL(RTRIM(VIW0012.ACCOUNTCODE), '') AS ACCOUNTCODE" _
                & "  , ISNULL(RTRIM(VIW0012.ACCOUNTNAME), '') AS ACCOUNTNAME" _
                & "  , ISNULL(RTRIM(VIW0012.SEGMENTCODE), '') AS SEGMENTCODE" _
                & "  , ISNULL(RTRIM(VIW0012.SEGMENTNAME), '') AS SEGMENTNAME" _
                & "  , ISNULL(RTRIM(VIW0012.BREAKDOWNCODE), '') AS BREAKDOWNCODE" _
                & "  , ISNULL(RTRIM(VIW0012.BREAKDOWN), '') AS BREAKDOWN" _
                & "  , ISNULL(RTRIM(OIT0003.SHIPPERSCODE), '') AS SHIPPERSCODE" _
                & "  , ISNULL(RTRIM(OIT0003.SHIPPERSNAME), '') AS SHIPPERSNAME" _
                & "  , ISNULL(RTRIM(OIT0002.BASECODE), '') AS BASECODE" _
                & "  , ISNULL(RTRIM(OIT0002.BASENAME), '') AS BASENAME" _
                & "  , ISNULL(RTRIM(OIT0002.OFFICECODE), '') AS OFFICECODE" _
                & "  , ISNULL(RTRIM(OIT0002.OFFICENAME), '') AS OFFICENAME" _
                & "  , ISNULL(RTRIM(OIT0002.DEPSTATION), '') AS DEPSTATION" _
                & "  , ISNULL(RTRIM(OIT0002.DEPSTATIONNAME), '') AS DEPSTATIONNAME" _
                & "  , ISNULL(RTRIM(OIT0002.ARRSTATION), '') AS ARRSTATION" _
                & "  , ISNULL(RTRIM(OIT0002.ARRSTATIONNAME), '') AS ARRSTATIONNAME" _
                & "  , ISNULL(RTRIM(OIT0002.CONSIGNEECODE), '') AS CONSIGNEECODE" _
                & "  , ISNULL(RTRIM(OIT0002.CONSIGNEENAME), '') AS CONSIGNEENAME" _
                & "  , ISNULL( " _
                & "    RTRIM(OIT0003.ACTUALLODDATE)" _
                & "    , FORMAT(GETDATE(), 'yyyy/MM/dd')" _
                & "  ) AS KEIJYOYMD" _
                & "  , ISNULL(RTRIM(OIT0002.TRAINNO), '') AS TRAINNO" _
                & "  , ISNULL(RTRIM(OIT0002.TRAINNAME), '') AS TRAINNAME" _
                & "  , ISNULL(RTRIM(OIM0005.MODEL), '') AS MODEL" _
                & "  , ISNULL(RTRIM(OIT0003.TANKNO), '') AS TANKNO" _
                & "  , ISNULL(RTRIM(OIT0003.OTTRANSPORTFLG), '') AS OTTRANSPORTFLG" _
                & "  , ISNULL(RTRIM(OIT0003.CARSNUMBER), '') AS CARSNUMBER" _
                & "  , ISNULL(RTRIM(OIT0003.CARSAMOUNT), '') AS CARSAMOUNT" _
                & "  , ISNULL(RTRIM(OIM0005.LOAD), '') AS LOAD" _
                & "  , ISNULL(RTRIM(OIT0003.OILCODE), '') AS OILCODE" _
                & "  , ISNULL(RTRIM(OIT0003.OILNAME), '') AS OILNAME" _
                & "  , ISNULL(RTRIM(OIT0003.ORDERINGTYPE), '') AS ORDERINGTYPE" _
                & "  , ISNULL(RTRIM(OIT0003.ORDERINGOILNAME), '') AS ORDERINGOILNAME" _
                & "  , ISNULL(RTRIM(OIT0003.CHANGETRAINNO), '') AS CHANGETRAINNO" _
                & "  , ISNULL(RTRIM(OIT0003.CHANGETRAINNAME), '') AS CHANGETRAINNAME" _
                & "  , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEECODE), '') AS SECONDCONSIGNEECODE" _
                & "  , ISNULL(RTRIM(OIT0003.SECONDCONSIGNEENAME), '') AS SECONDCONSIGNEENAME" _
                & "  , ISNULL(RTRIM(OIT0003.SECONDARRSTATION), '') AS SECONDARRSTATION" _
                & "  , ISNULL(RTRIM(OIT0003.SECONDARRSTATIONNAME), '') AS SECONDARRSTATIONNAME" _
                & "  , ISNULL(RTRIM(OIT0003.CHANGERETSTATION), '') AS CHANGERETSTATION" _
                & "  , ISNULL(RTRIM(OIT0003.CHANGERETSTATIONNAME), '') AS CHANGERETSTATIONNAME" _
                & "  , ISNULL(RTRIM(VIW0012.TRKBN), '') AS TRKBN" _
                & "  , ISNULL(RTRIM(VIW0012.TRKBNNAME), '') AS TRKBNNAME" _
                & "  , ISNULL(RTRIM(VIW0012.KIRO), '') AS KIRO" _
                & "  , ISNULL(RTRIM(VIW0012.CALCKBN), '') AS CALCKBN" _
                & "  , ISNULL(RTRIM(VIW0012.CALCKBNNAME), '') AS CALCKBNNAME" _
                & "  , ISNULL(RTRIM(VIW0012.JROILTYPE), '') AS JROILTYPE" _
                & "  , ISNULL(RTRIM(VIW0012.FARE), '') AS CHARGE" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT1), '') AS DISCOUNT1" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT2), '') AS DISCOUNT2" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT3), '') AS DISCOUNT3" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT4), '') AS DISCOUNT4" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT5), '') AS DISCOUNT5" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT6), '') AS DISCOUNT6" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNT7), '') AS DISCOUNT7" _
                & "  , ISNULL(RTRIM(VIW0012.DISCOUNTFARE), '') AS APPLYCHARGE" _
                & "  , CASE" _
                & "    WHEN VIW0012.CALCKBN = '1' THEN OIT0003.CARSNUMBER * VIW0012.DISCOUNTFARE" _
                & "    WHEN VIW0012.CALCKBN = '2' THEN OIT0003.CARSAMOUNT * VIW0012.DISCOUNTFARE" _
                & "    WHEN VIW0012.CALCKBN = '3' THEN OIM0005.LOAD * VIW0012.DISCOUNTFARE" _
                & "    END                                    AS AMOUNT" _
                & "  , CASE" _
                & "    WHEN VIW0012.CALCKBN = '1' THEN OIT0003.CARSNUMBER * (VIW0012.DISCOUNTFARE * CONVERT(DECIMAL(5,2), OIS0015.KEYCODE))" _
                & "    WHEN VIW0012.CALCKBN = '2' THEN OIT0003.CARSAMOUNT * (VIW0012.DISCOUNTFARE * CONVERT(DECIMAL(5,2), OIS0015.KEYCODE))" _
                & "    WHEN VIW0012.CALCKBN = '3' THEN OIM0005.LOAD * (VIW0012.DISCOUNTFARE * CONVERT(DECIMAL(5,2), OIS0015.KEYCODE))" _
                & "    END                                    AS TAX" _
                & "  , CONVERT(DECIMAL(5,2), OIS0015.KEYCODE) AS CONSUMPTIONTAX" _
                & "  , ISNULL(RTRIM(VIW0012.INVOICECODE), '') AS INVOICECODE" _
                & "  , ISNULL(RTRIM(VIW0012.INVOICENAME), '') AS INVOICENAME" _
                & "  , ISNULL(RTRIM(VIW0012.INVOICEDEPTNAME), '') AS INVOICEDEPTNAME" _
                & "  , ISNULL(RTRIM(VIW0012.PAYEECODE), '') AS PAYEECODE" _
                & "  , ISNULL(RTRIM(VIW0012.PAYEENAME), '') AS PAYEENAME" _
                & "  , ISNULL(RTRIM(VIW0012.PAYEEDEPTNAME), '') AS PAYEEDEPTNAME" _
                & String.Format("  , '{0}' AS DELFLG", C_DELETE_FLG.ALIVE) _
                & String.Format("  , '{0}' AS INITYMD", Date.Now) _
                & String.Format("  , '{0}' AS INITUSER", Master.USERID) _
                & String.Format("  , '{0}' AS INITTERMID", Master.USERTERMID) _
                & String.Format("  , '{0}' AS UPDYMD", Date.Now) _
                & String.Format("  , '{0}' AS UPDUSER", Master.USERID) _
                & String.Format("  , '{0}' AS UPDTERMID", Master.USERTERMID) _
                & String.Format("  , '{0}' AS RECEIVEYMD", C_DEFAULT_YMD)

            SQLCmn1Str &=
                  " FROM" _
                & "  OIL.OIT0002_ORDER OIT0002 " _
                & "  INNER JOIN OIL.OIT0003_DETAIL OIT0003 " _
                & "    ON OIT0003.ORDERNO = OIT0002.ORDERNO " _
                & "    AND OIT0003.DELFLG <> '1' " _
                & String.Format("    AND FORMAT(CONVERT(DATE,OIT0003.ACTUALLODDATE),'yyyy/MM') = '{0}'", Me.TxtKeijyoYM.Text) _
                & "  INNER JOIN OIL.OIM0005_TANK OIM0005 " _
                & "    ON OIT0003.TANKNO = OIM0005.TANKNUMBER " _
                & "    AND OIM0005.DELFLG <> '1' "

            '料金作成用1(共通)
            SQLAct1Str =
                  "   INNER JOIN OIL.VIW0012_ACCOUNTLIST VIW0012 " _
                & "    ON VIW0012.OFFICECODE = OIT0002.OFFICECODE " _
                & "    AND VIW0012.SHIPPERSCODE = OIT0003.SHIPPERSCODE " _
                & "    AND VIW0012.BASECODE = OIT0002.BASECODE " _
                & "    AND VIW0012.DEPSTATION = OIT0002.DEPSTATION " _
                & "    AND VIW0012.ARRSTATION = OIT0002.ARRSTATION " _
                & "    AND VIW0012.CONSIGNEECODE = OIT0002.CONSIGNEECODE " _
                & "    AND VIW0012.LOAD = OIM0005.LOAD " _
                & "    AND VIW0012.JROILTYPE = 'X' "

            '料金作成用2(危険品・普通品)
            SQLAct2Str =
                  "   INNER JOIN OIL.VIW0012_ACCOUNTLIST VIW0012 " _
                & "    ON VIW0012.OFFICECODE = OIT0002.OFFICECODE " _
                & "    AND VIW0012.SHIPPERSCODE = OIT0003.SHIPPERSCODE " _
                & "    AND VIW0012.BASECODE = OIT0002.BASECODE " _
                & "    AND VIW0012.DEPSTATION = OIT0002.DEPSTATION " _
                & "    AND VIW0012.ARRSTATION = OIT0002.ARRSTATION " _
                & "    AND VIW0012.CONSIGNEECODE = OIT0002.CONSIGNEECODE " _
                & "    AND VIW0012.LOAD = OIM0005.LOAD " _
                & "    AND VIW0012.JROILTYPE <> 'X' " _
                & "    AND VIW0012.JROILTYPE = CASE " _
                & String.Format("      WHEN OIT0003.OILCODE = '{0}' ", BaseDllConst.CONST_HTank) _
                & String.Format("      OR OIT0003.OILCODE = '{0}' ", BaseDllConst.CONST_RTank) _
                & "        THEN 'D' " _
                & "      ELSE 'N' " _
                & "      END "

            '★共通2SQL
            SQLCmn2Str =
                  "  INNER JOIN com.OIS0015_FIXVALUE OIS0015 ON" _
               & "  OIS0015.CLASS = 'CONSUMPTIONTAX'" _
               & "  AND OIT0003.ACTUALLODDATE BETWEEN OIS0015.STYMD AND OIS0015.ENDYMD" _
               & " WHERE" _
               & String.Format("  OIT0002.DELFLG <> '{0}' ", C_DELETE_FLG.DELETE) _
               & String.Format("  AND OIT0002.ORDERSTATUS <> '{0}'", BaseDllConst.CONST_ORDERSTATUS_900)

            '★SQL組み立て
            SQLStr &=
                 SQLCmn1Str & SQLAct1Str & SQLCmn2Str _
               & "UNION ALL " _
               & SQLCmn1Str & SQLAct2Str & SQLCmn2Str

            Dim SQLcmd As New SqlCommand(SQLStr, SQLcon)
            SQLcmd.CommandTimeout = 300

            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIJ0001C_ORDERDETAILBILLING INSERT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIJ0001C_ORDERDETAILBILLING INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub

        End Try

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
                    prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                    '運用部署
                    If WF_FIELD.Value = "WF_UORG" Then
                        prmData = work.CreateUORGParam(WF_CAMPCODE.Text)
                    End If

                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                    .ActiveListBox()
                Else
                    '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "TxtKeijyoYM"
                            If Me.TxtKeijyoYM.Text = "" Then
                                .WF_Calendar.Text = Now.ToString("yyyy/MM/dd")
                            Else
                                .WF_Calendar.Text = Me.TxtKeijyoYM.Text + "/01"
                            End If
                    End Select
                    .ActiveCalendar()
                End If
            End With
        End If
    End Sub
    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

    End Sub
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
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValue
                WF_CAMPCODE_TEXT.Text = WW_SelectText
                WF_CAMPCODE.Focus()
            Case "WF_UORG"              '運用部署
                WF_UORG.Text = WW_SelectValue
                WF_UORG_TEXT.Text = WW_SelectText
                WF_UORG.Focus()
            Case "TxtKeijyoYM"          '計上年月
                Dim WW_DATE As Date
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                    If WW_DATE < C_DEFAULT_YMD Then
                        Me.TxtKeijyoYM.Text = ""
                    Else
                        Me.TxtKeijyoYM.Text = WW_DATE.ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                Me.TxtKeijyoYM.Focus()
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
            Case "TxtKeijyoYM"          '計上年月
                TxtKeijyoYM.Focus()
        End Select
        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub
    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        Master.ShowHelp()

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub
End Class
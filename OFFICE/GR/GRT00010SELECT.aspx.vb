Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

''' <summary>
''' 残業申請承認（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRT00010SELECT
    Inherits Page

    '共通関数宣言(BASEDLL)
    Private CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
    Private CS0050Session As New CS0050SESSION              'セッション情報
    Private T0008COM As New GRT0008COM                      '事務勤怠共通

    '○ 共通処理結果
    Private WW_ERR_SW As String
    Private WW_RTN_SW As String
    Private WW_DUMMY As String

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        If IsPostBack Then

            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                '○ 各ボタン押下処理
                Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDO"                  '実行ボタン押下
                            WF_ButtonDO_Click()
                        Case "WF_ButtonEND"                 '終了ボタン押下
                            WF_ButtonEND_Click()
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

        '○ 画面ID設定
        Master.MAPID = GRT00010WRKINC.MAPIDS

        WF_CAMPCODE.Focus()
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""
        WF_RightboxOpen.Value = ""

        leftview.activeListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ メニューからの画面遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            '画面間の情報クリア
            work.Initialize()

            '初期変数設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text                            '会社コード
            Master.getFirstValue(WF_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text)    '申請年月
            Dim WW_DATE As Date
            Try
                Date.TryParse(WF_TAISHOYM.Text, WW_DATE)
                WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
            Catch ex As Exception
                WF_TAISHOYM.Text = Date.Now.Year.ToString() & "/" & Date.Now.Month.ToString()
            End Try

            Master.getFirstValue(WF_CAMPCODE.Text, "HORG", WF_HORG.Text)            '配属部署

            Master.getFirstValue(WF_CAMPCODE.Text, "APPROVALDISPTYPE", WF_APPROVALDISPTYPE.Text)       '承認区分

        End If

        '○ 実行画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.T00010 Then
            '画面項目設定処理
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text                    '会社コード
            WF_TAISHOYM.Text = work.WF_SEL_TAISHOYM.Text                    '申請年月
            WF_HORG.Text = work.WF_SEL_HORG.Text                            '配属部署
            WF_APPROVALDISPTYPE.Text = work.WF_SEL_APPROVALDISPTYPE.Text    '承認区分

        End If

        '○ RightBox情報設定
        rightview.MAPIDS = GRT00010WRKINC.MAPIDS
        rightview.MAPID = GRT00010WRKINC.MAPID
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_DUMMY)

        '○ 名称設定処理
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)         '会社コード
        CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_DUMMY)                     '配属部署
        CODENAME_get("APPROVALDISPTYPE", WF_APPROVALDISPTYPE.Text, WF_APPROVALDISPTYPE_TEXT.Text, WW_DUMMY)     '承認区分

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○ 入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)          '会社コード
        Master.eraseCharToIgnore(WF_TAISHOYM.Text)          '申請年月
        Master.eraseCharToIgnore(WF_HORG.Text)              '配属部署
        Master.eraseCharToIgnore(WF_APPROVALDISPTYPE.Text)  '承認区分

        '○ チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        Dim WW_HORG As String = ""
        Dim WW_STAFFCODE As String = ""

        '従業員番号取得
        GetSTAFFCODE(WW_HORG, WW_STAFFCODE, WW_RTN_SW)
        If Not isNormal(WW_RTN_SW) Then
            Exit Sub
        End If

        Dim WW_FIND As Boolean = False
        Dim WW_APPROVALORG As DataTable = New DataTable
        '承認マスタ存在チェック（存在しなければ権限エラー）
        CheckAPPROVAL(WW_APPROVALORG, WW_RTN_SW)
        If WW_RTN_SW = C_MESSAGE_NO.NORMAL Then
            For Each ORGrow As DataRow In WW_APPROVALORG.Rows
                If WF_HORG.Text = ORGrow("ORG") Then
                    WW_FIND = True
                    Exit For
                Else
                    If T0008COM.isGeneralAffair(WF_CAMPCODE.Text, Master.USER_ORG, WW_RTN_SW) Then
                        WW_FIND = True
                        Exit For
                    End If
                End If
            Next
        End If
        If WW_RTN_SW = "" OrElse WW_FIND = False Then
            '権限なし（承認マスタ未登録）
            Master.output(C_MESSAGE_NO.AUTHORIZATION_ERROR, C_MESSAGE_TYPE.ABORT, "承認権限なし")
            Exit Sub
        End If
        If WW_RTN_SW <> C_MESSAGE_NO.NORMAL Then
            'DBエラー
            Exit Sub
        End If

        '○ 条件選択画面の入力値退避
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text        '会社コード
        work.WF_SEL_TAISHOYM.Text = WF_TAISHOYM.Text        '申請年月
        work.WF_SEL_HORG.Text = WF_HORG.Text                '配属部署
        work.WF_SEL_APPROVALDISPTYPE.Text = WF_APPROVALDISPTYPE.Text    '承認区分

        '○ 画面レイアウト設定
        Master.VIEWID = rightview.getViewId(WF_CAMPCODE.Text)

        '○ SQL異常対応
        work.SQLAbnormalityRepair()

        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '画面遷移
            Master.transitionPage()
        End If

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""
        Dim WW_TEXT As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""

        '○ 単項目チェック
        '会社コード
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック
            CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            If Not isNormal(WW_RTN_SW) Then
                Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "会社コード : " & WF_CAMPCODE.Text)
                WF_CAMPCODE.Focus()
                O_RTN = "ERR"
                Exit Sub
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '申請年月
        Master.checkFIeld(WF_CAMPCODE.Text, "TAISHOYM", WF_TAISHOYM.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WF_TAISHOYM.Text <> "" Then
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WF_TAISHOYM.Text, WW_DATE)
                    WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
                Catch ex As Exception
                    Master.output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, "申請年月 : " & WF_TAISHOYM.Text)
                    WF_TAISHOYM.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End Try
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_TAISHOYM.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '配属部署
        WW_TEXT = WF_HORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "HORG", WF_HORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_HORG.Text = ""
            Else
                '存在チェック
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "配属部署 : " & WF_HORG.Text)
                    WF_HORG.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_HORG.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '承認区分
        WW_TEXT = WF_APPROVALDISPTYPE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "APPROVALDISPTYPE", WF_APPROVALDISPTYPE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If WW_TEXT = "" Then
                WF_APPROVALDISPTYPE.Text = ""
            Else
                '存在チェック
                CODENAME_get("APPROVALDISPTYPE", WF_APPROVALDISPTYPE.Text, WF_APPROVALDISPTYPE_TEXT.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.NO_DATA_EXISTS_ERROR, C_MESSAGE_TYPE.ERR, "承認区分 : " & WF_APPROVALDISPTYPE.Text)
                    WF_APPROVALDISPTYPE.Focus()
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_APPROVALDISPTYPE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '○ 正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 終了ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '○ 前画面遷移
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        '〇フィールドダブルクリック処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) OrElse WF_LeftMViewChange.Value = "" Then
        Else
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
                            Case "WF_TAISHOYM"        '申請年月
                                .WF_Calendar.Text = WF_TAISHOYM.Text & "/01"
                        End Select
                        .activeCalendar()

                    Case Else
                        '上記以外

                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_HORG"              '配属部署
                                prmData = work.CreateHORGParam(WF_CAMPCODE.Text)
                            Case "WF_APPROVALDISPTYPE"  '承認区分
                                prmData = work.CreateAPPROVALDISPTYPEParam(WF_CAMPCODE.Text)
                        End Select

                        .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .activeListBox()
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
            Case "WF_CAMPCODE"          '会社コード
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            Case "WF_HORG"              '配属部署
                CODENAME_get("HORG", WF_HORG.Text, WF_HORG_TEXT.Text, WW_RTN_SW)
            Case "WF_APPROVALDISPTYPE"  '承認区分
                CODENAME_get("APPROVALDISPTYPE", WF_APPROVALDISPTYPE.Text, WF_APPROVALDISPTYPE_TEXT.Text, WW_RTN_SW)
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
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

        Dim WW_SelectValues As String() = Nothing

        '○ 選択内容を取得
        If Not IsNothing(leftview.getActiveValue) Then
            WW_SelectValues = leftview.getActiveValue
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_CAMPCODE"          '会社コード
                WF_CAMPCODE.Text = WW_SelectValues(0)
                WF_CAMPCODE_TEXT.Text = WW_SelectValues(1)
                WF_CAMPCODE.Focus()

            Case "WF_TAISHOYM"          '申請年月
                Dim WW_DATE As Date
                Try
                    Date.TryParse(WW_SelectValues(0), WW_DATE)
                    WF_TAISHOYM.Text = WW_DATE.ToString("yyyy/MM")
                Catch ex As Exception
                End Try
                WF_TAISHOYM.Focus()

            Case "WF_HORG"              '配属部署
                WF_HORG.Text = WW_SelectValues(0)
                WF_HORG_TEXT.Text = WW_SelectValues(1)
                WF_HORG.Focus()

            Case "WF_APPROVALDISPTYPE"  '承認区分
                WF_APPROVALDISPTYPE.Text = WW_SelectValues(0)
                WF_APPROVALDISPTYPE_TEXT.Text = WW_SelectValues(1)
                WF_APPROVALDISPTYPE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

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
            Case "WF_TAISHOYM"          '申請年月
                WF_TAISHOYM.Focus()
            Case "WF_HORG"              '配属部署
                WF_HORG.Focus()
            Case "WF_APPROVALDISPTYPE"  '承認区分
                WF_APPROVALDISPTYPE.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' RightBoxダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()

        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' RightBoxメモ欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()

        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' ヘルプ表示
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_HELP_Click()

        WF_RightboxOpen.Value = ""
        Master.showHelp()

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
        prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "HORG"             '配属部署
                    prmData = work.CreateHORGParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "APPROVALDISPTYPE" '承認区分
                    prmData = work.CreateAPPROVALDISPTYPEParam(WF_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 従業員番号取得処理
    ''' </summary>
    ''' <param name="O_ORG">取得した従業員の所属部署</param>
    ''' <param name="O_STAFFCODE">取得した従業員のコード</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub GetSTAFFCODE(ByRef O_ORG As String, ByRef O_STAFFCODE As String, ByRef O_RTN As String)

        O_ORG = String.Empty
        O_STAFFCODE = String.Empty
        O_RTN = C_MESSAGE_NO.NORMAL

        Try

            '○　従業員ListBox設定                
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                Dim SQLStr As String = ""
                Dim SQLdr As SqlDataReader
                Dim PARA(10) As SqlParameter

                SQLcon.Open() 'DataBase接続(Open)
                '検索SQL文
                SQLStr =
                     "SELECT isnull(rtrim(B.CODE),'')      as ORG       " _
                   & "      ,isnull(rtrim(A.STAFFCODE),'') as STAFFCODE " _
                   & " FROM       S0004_USER   A                        " _
                   & " INNER JOIN M0006_STRUCT B                        " _
                   & "    ON   B.CAMPCODE     = @P3                     " _
                   & "   and   B.OBJECT       = 'ORG'                   " _
                   & "   and   B.STRUCT       = '勤怠管理組織'          " _
                   & "   and   B.GRCODE01     = A.ORG                   " _
                   & "   and   B.STYMD       <= @P2                     " _
                   & "   and   B.ENDYMD      >= @P2                     " _
                   & "   and   B.DELFLG      <> '1'                     " _
                   & " WHERE   A.USERID       = @P1                     " _
                   & "   and   A.STYMD       <= @P2                     " _
                   & "   and   A.ENDYMD      >= @P2                     " _
                   & "   and   A.DELFLG      <> '1'                     "

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
                    PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                    PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
                    PARA(1).Value = Master.USERID
                    PARA(2).Value = Date.Now
                    PARA(3).Value = WF_CAMPCODE.Text

                    SQLdr = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        O_ORG = SQLdr("ORG")
                        O_STAFFCODE = SQLdr("STAFFCODE")
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0004_USER SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0004_USER Select"             '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 承認マスタ取得処理
    ''' </summary>
    ''' <param name="O_ORG">取得した従業員の所属部署</param>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub CheckAPPROVAL(ByRef O_ORG As DataTable, ByRef O_RTN As String)

        O_RTN = String.Empty

        Try
            If IsNothing(O_ORG) Then
            Else
                O_ORG.Clear()
            End If
            O_ORG.Columns.Add("ORG", GetType(String))

            '○　承認情報取得
            Using SQLcon As SqlConnection = CS0050Session.getConnection
                Dim SQLStr As String = ""
                Dim SQLdr As SqlDataReader
                Dim PARA(10) As SqlParameter

                SQLcon.Open() 'DataBase接続(Open)
                '検索SQL文
                SQLStr =
                     "SELECT isnull(rtrim(B.CODE),'') as APPROVALORG " _
                   & " FROM S0005_AUTHOR A " _
                   & " INNER JOIN S0006_ROLE B " _
                   & "    ON   B.CAMPCODE     = A.CAMPCODE " _
                   & "   and   B.OBJECT       = A.OBJECT   " _
                   & "   and   B.ROLE         = A.ROLE     " _
                   & "   and   B.STYMD       <= @P3 " _
                   & "   and   B.ENDYMD      >= @P3 " _
                   & "   and   B.DELFLG      <> '1' " _
                   & " WHERE   A.USERID       = @P1 " _
                   & "   and   A.CAMPCODE     = @P2 " _
                   & "   and   A.OBJECT      in ('APPROVAL1','APPROVAL2') " _
                   & "   and   A.STYMD       <= @P3 " _
                   & "   and   A.ENDYMD      >= @P3 " _
                   & "   and   A.DELFLG      <> '1' "

                Using SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
                    PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
                    PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
                    PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                    PARA(1).Value = Master.USERID
                    PARA(2).Value = WF_CAMPCODE.Text
                    PARA(3).Value = Date.Now

                    SQLdr = SQLcmd.ExecuteReader()

                    While SQLdr.Read
                        Dim oRow As DataRow = O_ORG.NewRow
                        oRow("ORG") = SQLdr("APPROVALORG")
                        O_ORG.Rows.Add(oRow)
                        O_RTN = C_MESSAGE_NO.NORMAL
                    End While

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
            End Using

        Catch ex As Exception
            Master.output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "S0005_AUTHOR SELECT")
            CS0011LOGWRITE.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:S0005_AUTHOR Select"           '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub
End Class
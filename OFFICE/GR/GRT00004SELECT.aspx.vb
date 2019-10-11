Imports System
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.Control

Imports System.Drawing
Imports System.Net
Imports System.Data
Imports Microsoft.Office.Interop
Imports OFFICE.GRIS0005LeftBox

''' <summary>
''' 配送受注（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRT00004SELECT
    Inherits System.Web.UI.Page

    '共通処理結果
    Private WW_ERRCODE As String                                    '
    Private WW_ERR_SW As String                                     '
    Private WW_RTN_SW As String                                     '
    Private WW_DUMMY As String                                      '

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If IsPostBack Then
            '〇各ボタン押下処理
            If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                Select Case WF_ButtonClick.Value

                    Case "WF_ButtonDO"                      '実行
                        WF_ButtonDO_Click()
                    Case "WF_ButtonEND"                     '終了
                        WF_ButtonEND_Click()
                    Case "WF_ButtonRESTART"                 '再開
                        WF_ButtonRESTART_Click()

                        '********* 入力フィールド *********
                    Case "WF_Field_DBClick"                 '項目DbClick
                        WF_Field_DBClick()
                    Case "WF_LeftBoxSelectClick"            'フィールドチェンジ
                        WF_LEFTBOX_SELECT_CLICK()

                        '********* 左BOX *********
                    Case "WF_ButtonSel"                     '選択
                        WF_ButtonSel_Click()
                    Case "WF_ButtonCan"                     'キャンセル
                        WF_ButtonCan_Click()
                    Case "WF_ListboxDBclick"                '値選択DbClick
                        WF_LEFTBOX_DBClick()

                        '********* 右BOX *********
                    Case "WF_RIGHT_VIEW_DBClick"            '右ボックス表示
                        WF_RIGHTBOX_DBClick()
                    Case "WF_MEMOChange"                    'メモ欄更新
                        WF_RIGHTBOX_Change()

                        '********* その他はMasterPageで処理 *********
                    Case Else
                End Select
            End If

        Else
            '〇初期化処理
            Initialize()
        End If

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()
        '○初期値設定
        Master.MAPID = GRT00004WRKINC.MAPIDS
        WF_FIELD.Value = ""
        WF_CAMPCODE.Focus()

        '〇ヘルプ有
        Master.dispHelp = True
        '〇ドラックアンドドロップOFF
        Master.eventDrop = False

        '左Boxへの値設定
        WF_LeftMViewChange.Value = ""
        leftview.activeListBox()

        '○画面の値設定
        WW_MAPValueSet()

        '○RightBox情報設定
        rightview.MAPID = GRT00004WRKINC.MAPID
        rightview.MAPIDS = GRT00004WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ ボタン活性／非活性
        If System.IO.File.Exists(work.WF_SEL_XMLsavePARM.Text) Then
            '一時保存ファイルが存在した場合、ボタン活性
            WF_Restart.Value = "TRUE"
        Else
            '一時保存ファイルが存在しない場合、ボタン非活性
            WF_Restart.Value = "FALSE"
        End If

    End Sub

    ''' <summary>
    ''' 終了ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        '〇画面戻先URL取得
        Master.transitionPrevPage()

    End Sub

    ''' <summary>
    ''' 実行ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonDO_Click()

        '○初期設定
        WF_FIELD.Value = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)                            '会社コード
        Master.eraseCharToIgnore(WF_SHUKODATEF.Text)                          '出庫日FROM
        Master.eraseCharToIgnore(WF_SHUKODATET.Text)                          '出庫日TO
        Master.eraseCharToIgnore(WF_SHUKADATEF.Text)                          '出荷日FROM
        Master.eraseCharToIgnore(WF_SHUKADATET.Text)                          '出荷日TO
        Master.eraseCharToIgnore(WF_TODOKEDATEF.Text)                         '届日FROM
        Master.eraseCharToIgnore(WF_TODOKEDATET.Text)                         '届日TO
        Master.eraseCharToIgnore(WF_ORDERORG.Text)                            '受注部署
        Master.eraseCharToIgnore(WF_SHIPORG.Text)                             '出荷部署
        Master.eraseCharToIgnore(WF_OILTYPE.Text)                             '油種

        If Master.ConfirmOK = False Then
            '〇 チェック処理
            WW_Check(WW_ERR_SW)
            If Not isNormal(WW_ERR_SW) Then
                Exit Sub
            End If

            ''〇 光英当日データチェック処理
            If (Not String.IsNullOrEmpty(WF_OILTYPE.Text) AndAlso WF_OILTYPE.Text <> GRT00004WRKINC.C_PRODUCT_OIL) Then
            Else
                '油種条件が01(石油)
                'FIXVALUE(T00004_KOUEIORG)が定義されている場合のみチェック対象

                Dim T5Com = New GRT0005COM
                If T5Com.IsKoueiAvailableOrg(WF_CAMPCODE.Text, WF_SHIPORG.Text, GRT00004WRKINC.C_KOUEI_CLASS_CODE, WW_ERRCODE) Then
                    Dim exists As Boolean = CheckKoueiData(WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        T5Com = Nothing
                        Exit Sub
                    ElseIf exists Then
                        '当日データあり

                        'ダイアログ確認
                        Master.ConfirmWindow(C_MESSAGE_NO.KOUEI_CHANGE_DATA_EXISTS)
                        WF_SHUKODATEF.Focus()
                        T5Com = Nothing
                        Exit Sub
                    End If
                End If
                T5Com = Nothing
            End If
        End If

        '○条件選択画面の入力値退避(選択情報のWF_SEL退避) 
        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '出庫日　
        work.WF_SEL_SHUKODATEF.Text = WF_SHUKODATEF.Text
        If String.IsNullOrWhiteSpace(WF_SHUKODATET.Text) Then
            work.WF_SEL_SHUKODATET.Text = WF_SHUKODATEF.Text
        Else
            work.WF_SEL_SHUKODATET.Text = WF_SHUKODATET.Text
        End If
        '出荷日　
        work.WF_SEL_SHUKADATEF.Text = WF_SHUKADATEF.Text
        If String.IsNullOrWhiteSpace(WF_SHUKADATET.Text) Then
            work.WF_SEL_SHUKADATET.Text = WF_SHUKADATEF.Text
        Else
            work.WF_SEL_SHUKADATET.Text = WF_SHUKADATET.Text
        End If
        '届日　
        work.WF_SEL_TODOKEDATEF.Text = WF_TODOKEDATEF.Text
        If String.IsNullOrWhiteSpace(WF_TODOKEDATET.Text) Then
            work.WF_SEL_TODOKEDATET.Text = WF_TODOKEDATEF.Text
        Else
            work.WF_SEL_TODOKEDATET.Text = WF_TODOKEDATET.Text
        End If
        '受注部署
        work.WF_SEL_ORDERORG.Text = WF_ORDERORG.Text
        '出荷部署
        work.WF_SEL_SHIPORG.Text = WF_SHIPORG.Text
        '油種
        work.WF_SEL_OILTYPE.Text = WF_OILTYPE.Text

        work.WF_SEL_RESTART.Text = ""

        '〇右ボックスからPROFID取得
        Master.VIEWID = rightview.getViewId(work.WF_SEL_CAMPCODE.Text)
        '〇 画面遷移実行
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.transitionPage()
        End If

    End Sub

    ''' <summary>
    ''' 再開ボタン処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonRESTART_Click()

        '一時保存ファイルに条件パラメータ出力
        Dim T0004PARMtbl As DataTable = New DataTable
        work.PARMtbl_ColumnsAdd(T0004PARMtbl)
        Master.RecoverTable(T0004PARMtbl, work.WF_SEL_XMLsavePARM.Text)

        Dim T0004PARMrow = (From parm In T0004PARMtbl.AsEnumerable Select parm).FirstOrDefault
        If IsNothing(T0004PARMrow) Then
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "一時保存条件パラメータ")
            Exit Sub
        End If

        '会社コード　
        work.WF_SEL_CAMPCODE.Text = T0004PARMrow("CAMPCODE")
        '出庫日
        work.WF_SEL_SHUKODATEF.Text = T0004PARMrow("SHUKODATEF")
        work.WF_SEL_SHUKODATET.Text = T0004PARMrow("SHUKODATET")
        '出荷日
        work.WF_SEL_SHUKADATEF.Text = T0004PARMrow("SHUKADATEF")
        work.WF_SEL_SHUKADATET.Text = T0004PARMrow("SHUKADATET")
        '届日　
        work.WF_SEL_TODOKEDATEF.Text = T0004PARMrow("TODOKEDATEF")
        work.WF_SEL_TODOKEDATET.Text = T0004PARMrow("TODOKEDATET")
        '受注部署
        work.WF_SEL_ORDERORG.Text = T0004PARMrow("ORDERORG")
        '出荷部署
        work.WF_SEL_SHIPORG.Text = T0004PARMrow("SHIPORG")
        '油種
        work.WF_SEL_OILTYPE.Text = T0004PARMrow("OILTYPE")
        '光英読込中ファイル
        work.WF_SEL_KOUEILOADFILE.Text = T0004PARMrow("KOUEILOADFILE")

        '再開実行
        work.WF_SEL_RESTART.Text = "RESTART"

        '〇右ボックスからPROFID取得
        Master.VIEWID = rightview.GetViewId(work.WF_SEL_CAMPCODE.Text)
        '〇 画面遷移実行
        Master.CheckParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.TransitionPage()
        End If

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************
    ''' <summary>
    ''' LEFTBOXの選択された値をフィールドに戻す
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        Dim WW_Select() As String = leftview.GetActiveValue()
        If WW_Select(0).Length = 0 Then Exit Sub

        Select Case leftview.WF_LEFTMView.ActiveViewIndex
            Case 0                'ListBox
                Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
                Dim WW_Label As Label = DirectCast(work.getControl(WF_FIELD.Value & "_Text"), Label)
                WW_TextBox.Text = WW_Select(0)
                WW_Label.Text = WW_Select(1)
                WW_TextBox.Focus()
            Case 1                'Calendar
                Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
                WW_TextBox.Text = WW_Select(0)
                WW_TextBox.Focus()
            Case Else
        End Select

        '○画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' leftBOXキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        Dim WW_TextBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
        WW_TextBox.Focus()

        '○ 画面左サイドボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

    End Sub
    ''' <summary>
    ''' 左リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_DBClick()
        '〇ListBoxダブルクリック処理()
        WF_ButtonSel_Click()
        WW_LeftBoxReSet()
    End Sub
    ''' <summary>
    ''' '〇TextBox変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_LEFTBOX_SELECT_CLICK()
        WW_LeftBoxReSet()
    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Field_DBClick()
        '〇フィールドダブルクリック時処理
        If String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then Exit Sub
        If Not Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value) Then Exit Sub

        With leftview
            If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                Dim prmData As Hashtable = work.createFIXParam(WF_CAMPCODE.Text)

                Select Case WF_FIELD.Value
                    Case "WF_CAMPCODE"
                    Case "WF_ORDERORG"
                        prmData = work.createORGParam(WF_CAMPCODE.Text, True)
                    Case "WF_SHIPORG"
                        prmData = work.createORGParam(WF_CAMPCODE.Text, False)
                    Case "WF_OILTYPE"
                    Case Else
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
            Else
                Select Case WF_FIELD.Value
                    Case "WF_SHUKODATEF"        '出庫日
                        .WF_Calendar.Text = WF_SHUKODATEF.Text
                    Case "WF_SHUKODATET"
                        .WF_Calendar.Text = WF_SHUKODATEF.Text
                    Case "WF_SHUKADATEF"        '出荷日
                        .WF_Calendar.Text = WF_SHUKADATEF.Text
                    Case "WF_SHUKADATET"
                        .WF_Calendar.Text = WF_SHUKADATEF.Text
                    Case "WF_TODOKEDATEF"       '届日
                        .WF_Calendar.Text = WF_TODOKEDATEF.Text
                    Case "WF_TODOKEDATET"
                        .WF_Calendar.Text = WF_TODOKEDATEF.Text
                End Select
                .WF_Calendar.Focus()
                .ActiveCalendar()
            End If
        End With
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' TextBox変更時LeftBox設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_LeftBoxReSet()

        WF_CAMPCODE_Text.Text = ""
        WF_ORDERORG_Text.Text = ""
        WF_SHIPORG_Text.Text = ""
        WF_OILTYPE_Text.Text = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.EraseCharToIgnore(WF_CAMPCODE.Text)                            '会社コード
        Master.EraseCharToIgnore(WF_SHUKODATEF.Text)                          '出庫日FROM
        Master.EraseCharToIgnore(WF_SHUKODATET.Text)                          '出庫日TO
        Master.EraseCharToIgnore(WF_SHUKADATEF.Text)                          '出荷日FROM
        Master.EraseCharToIgnore(WF_SHUKADATET.Text)                          '出荷日TO
        Master.EraseCharToIgnore(WF_TODOKEDATEF.Text)                         '届日FROM
        Master.EraseCharToIgnore(WF_TODOKEDATET.Text)                         '届日TO
        Master.EraseCharToIgnore(WF_ORDERORG.Text)                            '受注部署
        Master.EraseCharToIgnore(WF_SHIPORG.Text)                             '出荷部署
        Master.EraseCharToIgnore(WF_OILTYPE.Text)                             '油種

        '〇 チェック処理
        WW_Check(WW_ERR_SW)

        '〇名称設定
        WW_NAMESet()

    End Sub

    ' ******************************************************************************
    ' ***  rightBOX関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 右リストボックスダブルクリック処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_DBClick()
        rightview.InitViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '〇右Boxメモ変更時処理
        rightview.Save(Master.USERID, Master.USERTERMID, WW_DUMMY)
    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 画面遷移による初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()
        Dim CS0050SESSION As New CS0050SESSION              'セッション情報管理

        '■■■ 選択画面の入力初期値設定 ■■■
        work.WF_SEL_XMLsavePARM.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-" & GRT00004WRKINC.MAPIDS & "-" & "PARM.txt"
        work.WF_SEL_XMLsaveTmp.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" & Master.USERID & "-" & GRT00004WRKINC.MAPID & "-" & "TMP.txt"

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移

            '〇選択情報のWF_SELクリア
            work.initialize()

            '○画面項目設定（変数より）処理
            WW_VARISet()

            '○ユーザ所属部署取得


        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00004 Then     '実行画面からの画面遷移

            If System.IO.File.Exists(work.WF_SEL_XMLsavePARM.Text) Then
                Dim T0004PARMtbl As DataTable = New DataTable
                '○一時保存ファイルが存在する場合
                'テーブルデータ 復元
                work.PARMtbl_ColumnsAdd(T0004PARMtbl)
                If Not Master.RecoverTable(T0004PARMtbl, work.WF_SEL_XMLsavePARM.Text) Then
                    Exit Sub
                End If

                For Each PARMrow As DataRow In T0004PARMtbl.Rows
                    '会社コード　
                    work.WF_SEL_CAMPCODE.Text = PARMrow("CAMPCODE")
                    '出庫日
                    work.WF_SEL_SHUKODATEF.Text = PARMrow("SHUKODATEF")
                    work.WF_SEL_SHUKODATET.Text = PARMrow("SHUKODATET")
                    '出荷日
                    work.WF_SEL_SHUKADATEF.Text = PARMrow("SHUKADATEF")
                    work.WF_SEL_SHUKADATET.Text = PARMrow("SHUKADATET")
                    '届日　
                    work.WF_SEL_TODOKEDATEF.Text = PARMrow("TODOKEDATEF")
                    work.WF_SEL_TODOKEDATET.Text = PARMrow("TODOKEDATET")
                    '受注部署
                    work.WF_SEL_ORDERORG.Text = PARMrow("ORDERORG")
                    '出荷部署
                    work.WF_SEL_SHIPORG.Text = PARMrow("SHIPORG")
                    '油種
                    work.WF_SEL_OILTYPE.Text = PARMrow("OILTYPE")

                    '１レコードしか存在しない（念のためEXIT）
                    Exit For
                Next
            End If

            '会社コード　
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '出庫日
            WF_SHUKODATEF.Text = work.WF_SEL_SHUKODATEF.Text
            WF_SHUKODATET.Text = work.WF_SEL_SHUKODATET.Text
            '出荷日
            WF_SHUKADATEF.Text = work.WF_SEL_SHUKADATEF.Text
            WF_SHUKADATET.Text = work.WF_SEL_SHUKADATET.Text
            '届日　
            WF_TODOKEDATEF.Text = work.WF_SEL_TODOKEDATEF.Text
            WF_TODOKEDATET.Text = work.WF_SEL_TODOKEDATET.Text
            '受注部署
            WF_ORDERORG.Text = work.WF_SEL_ORDERORG.Text
            '出荷部署
            WF_SHIPORG.Text = work.WF_SEL_SHIPORG.Text
            '油種
            WF_OILTYPE.Text = work.WF_SEL_OILTYPE.Text
        End If

        '■名称設定
        WW_NAMESet()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' 変数設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_VARISet()

        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text)       '会社コード
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHUKODATEF", WF_SHUKODATEF.Text)   '出庫日(FROM)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHUKODATET", WF_SHUKODATET.Text)   '出庫日(TO)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHUKADATEF", WF_SHUKADATEF.Text)   '出荷日(FROM)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHUKADATET", WF_SHUKADATET.Text)   '出荷日(TO)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TODOKEDATEF", WF_TODOKEDATEF.Text) '届日(FROM)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "TODOKEDATET", WF_TODOKEDATET.Text) '届日(TO)
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ORDERORG", WF_ORDERORG.Text)       '受注部署
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "SHIPORG", WF_SHIPORG.Text)         '出荷部署
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "OILTYPE", WF_OILTYPE.Text)         '油種

    End Sub

    ''' <summary>
    ''' 名称設定処理処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_NAMESet()

        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)         '会社
        CODENAME_get("ORDERORG", WF_ORDERORG.Text, WF_ORDERORG_Text.Text, WW_DUMMY)         '受注部署
        CODENAME_get("SHIPORG", WF_SHIPORG.Text, WF_SHIPORG_Text.Text, WW_DUMMY)            '出荷部署
        CODENAME_get("OILTYPE", WF_OILTYPE.Text, WF_OILTYPE_Text.Text, WW_DUMMY)            '油種

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '〇 入力項目チェック
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        WF_FIELD.Value = ""

        '会社コード WF_CAMPCODE 
        Master.checkFIeld(WF_CAMPCODE.Text, "CAMPCODE", WF_CAMPCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_CAMPCODE.Text <> "" Then
                CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_CAMPCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●出荷日(FROM) WF_SHUKADATEF.Text
        Dim WW_SHUKADATEF As Date

        Master.checkFIeld(WF_CAMPCODE.Text, "SHUKADATEF", WF_SHUKADATEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_SHUKADATEF.Text, WW_SHUKADATEF) Then
                WW_SHUKADATEF = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "出荷日(FROM)")
            WF_SHUKADATEF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●出荷日(TO) WF_SHUKADATET.Text
        Dim WW_SHUKADATET As Date

        Master.checkFIeld(WF_CAMPCODE.Text, "SHUKADATET", WF_SHUKADATET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_SHUKADATET.Text, WW_SHUKADATET) Then
                WW_SHUKADATET = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, BASEDLL.C_MESSAGE_TYPE.ERR, "出荷日(TO)")
            WF_SHUKADATET.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_SHUKADATEF.Text <> "" And WF_SHUKADATET.Text <> "" Then
            If WW_SHUKADATEF > WW_SHUKADATET Then
                Master.output(BASEDLL.C_MESSAGE_NO.START_END_RELATION_ERROR, BASEDLL.C_MESSAGE_TYPE.ERR)
                WF_SHUKADATET.Focus()
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                Exit Sub
            End If
        End If

        '●届日(FROM) WF_TODOKEDATEF.Text
        Dim WW_TODOKEDATEF As Date

        Master.checkFIeld(WF_CAMPCODE.Text, "TODOKEDATEF", WF_TODOKEDATEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_TODOKEDATEF.Text, WW_TODOKEDATEF) Then
                WW_TODOKEDATEF = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "届日(FROM)")
            WF_TODOKEDATEF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●届日(TO) WF_TODOKEDATET.Text
        Dim WW_TODOKEDATET As Date

        Master.checkFIeld(WF_CAMPCODE.Text, "TODOKEDATET", WF_TODOKEDATET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_TODOKEDATET.Text, WW_TODOKEDATET) Then
                WW_TODOKEDATET = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, BASEDLL.C_MESSAGE_TYPE.ERR, "届日(TO)")
            WF_TODOKEDATET.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_TODOKEDATEF.Text <> "" And WF_TODOKEDATET.Text <> "" Then
            If WW_TODOKEDATEF > WW_TODOKEDATET Then
                Master.output(BASEDLL.C_MESSAGE_NO.START_END_RELATION_ERROR, BASEDLL.C_MESSAGE_TYPE.ERR)
                WF_TODOKEDATET.Focus()
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                Exit Sub
            End If
        End If

        '●出庫日(FROM) WF_SHUKODATEF.Text
        '入力チェック(出庫日)
        If String.IsNullOrEmpty(WF_SHUKODATEF.Text) Then
            Master.output(C_MESSAGE_NO.PREREQUISITE_ERROR, BASEDLL.C_MESSAGE_TYPE.ERR, "出庫日(FROM)")
            WF_SHUKODATEF.Focus()
            O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
            Exit Sub
        End If

        Dim WW_SHUKODATEF As Date

        Master.checkFIeld(WF_CAMPCODE.Text, "SHUKODATEF", WF_SHUKODATEF.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_SHUKODATEF.Text, WW_SHUKODATEF) Then
                WW_SHUKODATEF = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, BASEDLL.C_MESSAGE_TYPE.ERR, "出庫日(FROM)")
            WF_SHUKODATEF.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●出荷日(TO) WF_SHUKODATET.Text
        Dim WW_SHUKODATET As Date

        Master.checkFIeld(WF_CAMPCODE.Text, "SHUKODATET", WF_SHUKODATET.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            If Not Date.TryParse(WF_SHUKODATET.Text, WW_SHUKODATET) Then
                WW_SHUKODATET = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, BASEDLL.C_MESSAGE_TYPE.ERR, "出庫日(TO)")
            WF_SHUKODATET.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '関連チェック(開始＞終了)
        If WF_SHUKODATEF.Text <> "" And WF_SHUKODATET.Text <> "" Then
            If WW_SHUKODATEF > WW_SHUKODATET Then
                Master.output(BASEDLL.C_MESSAGE_NO.START_END_RELATION_ERROR, BASEDLL.C_MESSAGE_TYPE.ERR)
                WF_SHUKODATET.Focus()
                O_RTN = C_MESSAGE_NO.START_END_RELATION_ERROR
                Exit Sub
            End If
        End If

        '●受注部署 WF_ORDERORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "ORDERORG", WF_ORDERORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_ORDERORG.Text <> "" Then
                CODENAME_get("ORDERORG", WF_ORDERORG.Text, WF_ORDERORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_ORDERORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ORDERORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If


        '●出荷部署 WF_SHIPORG.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "SHIPORG", WF_SHIPORG.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_SHIPORG.Text <> "" Then
                CODENAME_get("SHIPORG", WF_SHIPORG.Text, WF_SHIPORG_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_SHIPORG.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_SHIPORG.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '●油種 WF_OILTYPE
        Master.checkFIeld(WF_CAMPCODE.Text, "OILTYPE", WF_OILTYPE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_OILTYPE.Text <> "" Then
                CODENAME_get("OILTYPE", WF_OILTYPE.Text, WF_OILTYPE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_OILTYPE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_OILTYPE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

    ''' <summary>
    ''' 光英当日データ有無チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function CheckKoueiData(ByRef O_RTN As String) As Boolean

        Dim exists As Boolean = False                   '当日データ有無
        O_RTN = C_MESSAGE_NO.NORMAL

        '受信ファイルリスト
        Dim dicFileList As New Dictionary(Of String, List(Of FileInfo))
        '光英ファイルFTP受信
        work.GetKoueiFile(WF_SHIPORG.Text, dicFileList, O_RTN, True)
        If Not isNormal(O_RTN) Then
            Master.output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ERR, "光英ファイルFTP受信")
            Return exists
        End If

        '***** 当日（出庫日FROM以前）データあり→配乗済データに変更が発生している為、条件選択の日付範囲の見直しを促す *****
        Dim sm = New CS0050SESSION
        'Dim dateF As String = sm.LOGONDATE.Replace("/", "")
        Dim dateF As String = Today.ToString("yyyyMMdd")
        Dim dateT As String = Date.Parse(WF_SHUKODATEF.Text).AddDays(-1).ToString("yyyyMMdd")
        Dim fileList = dicFileList.Where(Function(x) x.Key >= dateF AndAlso x.Key <= dateT).SelectMany(Function(x) x.Value)
        If fileList.Count > 0 Then
            exists = True
        End If

        Return exists

    End Function

    ''' <summary>
    ''' 左リストボックスより名称取得とチェックを行う
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        '○名称取得
        O_TEXT = ""
        O_RTN = C_MESSAGE_NO.NORMAL

        '入力値が空は終了
        If String.IsNullOrEmpty(I_VALUE) Then Exit Sub

        Select Case I_FIELD
            Case "CAMPCODE"
                '会社コード
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN)
            Case "ORDERORG"
                '受注部署
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(WF_CAMPCODE.Text, True))
            Case "SHIPORG"
                '出荷部署
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.createORGParam(WF_CAMPCODE.Text, False))
            Case "OILTYPE"
                '油種
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OILTYPE, I_VALUE, O_TEXT, O_RTN, work.createFIXParam(WF_CAMPCODE.Text))
            Case Else
        End Select

    End Sub

End Class
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
''' 勤怠締（条件）
''' </summary>
''' <remarks></remarks>
Public Class GRT00008SELECT
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
        Master.MAPID = GRT00008WRKINC.MAPIDS
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
        rightview.MAPID = GRT00008WRKINC.MAPID
        rightview.MAPIDS = GRT00008WRKINC.MAPIDS
        rightview.COMPCODE = WF_CAMPCODE.Text
        rightview.MAPVARI = Master.MAPvariant
        rightview.PROFID = Master.PROF_VIEW
        rightview.Initialize("画面レイアウト設定", WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
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
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)                          '会社コード
        Master.eraseCharToIgnore(WF_LIMITYM.Text)                           '締年月
        Master.eraseCharToIgnore(WF_ORGCODE.Text)                           '部署コード

        '〇 チェック処理
        WW_Check(WW_ERR_SW)
        If WW_ERR_SW = "ERR" Then
            Exit Sub
        End If

        '○条件選択画面の入力値退避(選択情報のWF_SEL退避) 
        '会社コード　
        work.WF_SEL_CAMPCODE.Text = WF_CAMPCODE.Text
        '締年月　
        work.WF_SEL_LIMITYM.Text = WF_LIMITYM.Text
        '部署コード
        work.WF_SEL_ORGCODE.Text = WF_ORGCODE.Text

        '〇右ボックスからPROFID取得
        Master.VIEWID = rightview.getViewId(work.WF_SEL_CAMPCODE.Text)
        '〇 画面遷移実行
        Master.checkParmissionCode(WF_CAMPCODE.Text)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            '〇画面遷移先URL取得
            Master.transitionPage()
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
        Dim WW_Select() As String = leftview.getActiveValue()
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

                Dim WW_DATE As Date
                If Date.TryParse(WW_Select(0), WW_DATE) Then
                    WW_TextBox.Text = CDate(WW_Select(0)).ToString("yyyy/MM")
                End If
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
                    Case "WF_ORGCODE"
                        prmData = work.createORGParam(WF_CAMPCODE.Text, Master.USER_ORG)
                    Case Else
                End Select

                .setListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .activeListBox()
            Else
                '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                Dim txtBox As TextBox = DirectCast(work.getControl(WF_FIELD.Value), TextBox)
                .WF_Calendar.Text = String.Format("{0}/01", txtBox.Text)
                .activeCalendar()
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
        WF_ORGCODE_Text.Text = ""

        '○入力文字置き換え(使用禁止文字排除)
        Master.eraseCharToIgnore(WF_CAMPCODE.Text)                          '会社コード
        Master.eraseCharToIgnore(WF_LIMITYM.Text)                           '締年月
        Master.eraseCharToIgnore(WF_ORGCODE.Text)                           '部署コード

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
        rightview.initViewID(WF_CAMPCODE.Text, WW_DUMMY)
    End Sub
    ''' <summary>
    ''' 右リストボックスMEMO欄更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_RIGHTBOX_Change()
        '〇右Boxメモ変更時処理
        rightview.save(Master.USERID, Master.USERTERMID, WW_DUMMY)
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

        If Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.MENU Then               'メニューからの画面遷移

            '〇選択情報のWF_SELクリア
            work.initialize()

            '○画面項目設定（変数より）処理
            WW_VARISet()

            '○ユーザ所属部署取得


        ElseIf Context.Handler.ToString().ToUpper = C_PREV_MAP_LIST.T00008 Then     '実行画面からの画面遷移

            '会社コード　
            WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
            '締年月
            WF_LIMITYM.Text = work.WF_SEL_LIMITYM.Text
            '部署コード
            WF_ORGCODE.Text = work.WF_SEL_ORGCODE.Text
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
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "LIMITYM", WF_LIMITYM.Text)         '締年月
        If IsDate(WF_LIMITYM.Text) Then
            WF_LIMITYM.Text = CDate(WF_LIMITYM.Text).ToString("yyyy/MM")
        End If
        Master.getFirstValue(work.WF_SEL_CAMPCODE.Text, "ORGCODE", WF_ORGCODE.Text)         '部署コード

    End Sub

    ''' <summary>
    ''' 名称設定処理処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_NAMESet()

        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_Text.Text, WW_DUMMY)         '会社
        CODENAME_get("ORGCODE", WF_ORGCODE.Text, WF_ORGCODE_Text.Text, WW_DUMMY)            '部署コード

    End Sub

    ''' <summary>
    ''' チェック処理
    ''' </summary>
    ''' <param name="O_RTN">可否判定</param>
    ''' <remarks></remarks>
    Protected Sub WW_Check(ByRef O_RTN As String)

        O_RTN = ""

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
                    O_RTN = "ERR"
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_CAMPCODE.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If

        '●締年月 WF_LIMITYM.Text
        Dim WW_Check = WF_LIMITYM.Text

        Master.checkFIeld(WF_CAMPCODE.Text, "LIMITYM", WW_Check, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            Dim WW_LIMITYM As Date
            If Not Date.TryParse(WF_LIMITYM.Text & "/01", WW_LIMITYM) Then
                WW_LIMITYM = C_DEFAULT_YMD
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR, "締年月")
            WF_LIMITYM.Focus()
            O_RTN = "ERR"
            Exit Sub
        End If


        '●部署コード WF_ORGCODE.Text
        Master.checkFIeld(WF_CAMPCODE.Text, "ORGCODE", WF_ORGCODE.Text, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
        If isNormal(WW_CS0024FCHECKERR) Then
            '存在チェック(LeftBoxチェック)
            If WF_ORGCODE.Text <> "" Then
                CODENAME_get("ORGCODE", WF_ORGCODE.Text, WF_ORGCODE_Text.Text, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    Master.output(C_MESSAGE_NO.INVALID_SELECTION_DATA, C_MESSAGE_TYPE.ERR)
                    WF_ORGCODE.Focus()
                    O_RTN = C_MESSAGE_NO.INVALID_SELECTION_DATA
                    Exit Sub
                End If
            End If
        Else
            Master.output(WW_CS0024FCHECKERR, C_MESSAGE_TYPE.ERR)
            WF_ORGCODE.Focus()
            O_RTN = WW_CS0024FCHECKERR
            Exit Sub
        End If

        '正常メッセージ
        Master.output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.NOR)

    End Sub

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
            Case "ORGCODE"
                '部署コード
                leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(WF_CAMPCODE.Text, WF_ORGCODE.Text))
            Case Else
        End Select

    End Sub

End Class
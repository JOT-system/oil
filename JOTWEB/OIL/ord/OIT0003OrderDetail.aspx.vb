﻿'Option Strict On
'Option Explicit On

Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

Public Class OIT0003OrderDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private OIT0001tbl As DataTable                                 '一覧格納用テーブル
    Private OIT0001INPtbl As DataTable                              'チェック用テーブル
    Private OIT0001UPDtbl As DataTable                              '更新用テーブル
    Private OIT0001WKtbl As DataTable                               '作業用テーブル

    'Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    'Private Const CONST_SCROLLCOUNT As Integer = 7                 'マウススクロール時稼働行数
    'Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部タブID

    Private Const CONST_DSPROWCOUNT As Integer = 45             '１画面表示対象
    Private Const CONST_SCROLLROWCOUNT As Integer = 10          'マウススクロール時の増分
    Private Const CONST_DETAIL_TABID As String = "DTL1"         '詳細部タブID
    Private Const CONST_MAX_TABID As Integer = 4               '詳細タブ数


    Private Const CONST_TxtHTank As String = "1001"                 '油種(ハイオク)
    Private Const CONST_TxtRTank As String = "1101"                 '油種(レギュラー)
    Private Const CONST_TxtTTank As String = "1301"                 '油種(灯油)
    Private Const CONST_TxtMTTank As String = "1302"                '油種(未添加灯油)
    Private Const CONST_TxtKTank1 As String = "1401"                '油種(軽油)
    Private Const CONST_TxtKTank2 As String = "1406"
    Private Const CONST_TxtK3Tank1 As String = "1404"               '３号軽油
    Private Const CONST_TxtK3Tank2 As String = "1405"
    Private Const CONST_TxtK5Tank As String = "1402"                '軽油５
    Private Const CONST_TxtK10Tank As String = "1403"               '軽油１０
    Private Const CONST_TxtLTank1 As String = "2201"                'ＬＳＡ
    Private Const CONST_TxtLTank2 As String = "2202"
    Private Const CONST_TxtATank As String = "2101"                 'Ａ重油

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
    Private CS0052DetailView As New CS0052DetailView                'Repeterオブジェクト作成

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
                    Master.RecoverTable(OIT0001tbl)

                    Select Case WF_ButtonClick.Value
                        'Case "WF_ButtonINSERT"          '登録ボタン押下
                        '    WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        'Case "WF_Field_DBClick"         'フィールドダブルクリック
                        '    WF_FIELD_DBClick()
                        'Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                        '    WF_CheckBoxSELECT_Click()
                        'Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                        '    WF_FIELD_Change()
                        'Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                        '    WF_ButtonSel_Click()
                        'Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                        '    WF_ButtonCan_Click()
                        'Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                        '    WF_ButtonSel_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        'Case "WF_ButtonLINE_LIFTED"     '行削除ボタン押下
                        '    WF_ButtonLINE_LIFTED_Click()
                        'Case "WF_ButtonLINE_ADD"        '行追加ボタン押下
                        '    WF_ButtonLINE_ADD_Click()
                        'Case "WF_ButtonCSV"             'ダウンロードボタン押下
                        '    WF_ButtonDownload_Click()
                        'Case "WF_ButtonUPDATE"          '明細更新ボタン押下
                        '    WF_ButtonUPDATE_Click()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        'Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                        '    WF_FILEUPLOAD()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                            'Case "WF_ListChange"            'リスト変更
                            '    WF_ListChange()
                    End Select

                    ''○ 一覧再表示処理
                    'DisplayGrid()
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

            '○ 作成モード(１：新規登録, ２：更新)設定
            If work.WF_SEL_CREATEFLG.Text = "1" Then
                WF_CREATEFLG.Value = "1"
            Else
                WF_CREATEFLG.Value = "2"
            End If
        Finally
            '○ 格納Table Close
            If Not IsNothing(OIT0001tbl) Then
                OIT0001tbl.Clear()
                OIT0001tbl.Dispose()
                OIT0001tbl = Nothing
            End If

            If Not IsNothing(OIT0001INPtbl) Then
                OIT0001INPtbl.Clear()
                OIT0001INPtbl.Dispose()
                OIT0001INPtbl = Nothing
            End If

            If Not IsNothing(OIT0001UPDtbl) Then
                OIT0001UPDtbl.Clear()
                OIT0001UPDtbl.Dispose()
                OIT0001UPDtbl = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIT0003WRKINC.MAPIDD
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

        ''○ GridView初期設定
        'GridViewInitialize()

        '○ 詳細-画面初期設定
        Repeater_INIT()
        WF_DTAB_CHANGE_NO.Value = "0"

        '〇 タブ切替
        WF_Detail_TABChange()

        '〇 タブ指定時表示判定処理
        TAB_DisplayCTRL()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        'Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        'ステータス
        TxtOrderStatus.Text = work.WF_SEL_ORDERSTATUSNM.Text
        '情報
        TxtOrderInfo.Text = work.WF_SEL_INFORMATIONNM.Text
        '###################################################
        '受注パターン
        TxtOrderType.Text = ""
        '###################################################
        'オーダー№
        TxtOrderNo.Text = work.WF_SEL_ORDERNUMBER.Text
        '荷主
        TxtShippersCode.Text = work.WF_SEL_SHIPPERSCODE.Text
        '荷受人
        TxtConsigneeCode.Text = work.WF_SEL_CONSIGNEECODE.Text
        '本社列車
        TxtTrainNo.Text = work.WF_SEL_TRAIN.Text
        '発駅
        TxtDepstationCode.Text = work.WF_SEL_DEPARTURESTATION.Text
        '着駅
        TxtArrstationCode.Text = work.WF_SEL_ARRIVALSTATION.Text

        '(予定)積込日
        TxtLoadingDate.Text = work.WF_SEL_LODDATE.Text
        '(予定)発日
        TxtDepDate.Text = work.WF_SEL_DEPDATE.Text
        '(予定)積車着日
        TxtArrDate.Text = work.WF_SEL_ARRDATE.Text
        '(予定)受入日
        TxtAccDate.Text = work.WF_SEL_ACCDATE.Text
        '(予定)空車着日
        TxtEmparrDate.Text = work.WF_SEL_EMPARRDATE.Text

        '(実績)積込日
        TxtActualLoadingDate.Text = work.WF_SEL_ACTUALLODDATE.Text
        '(実績)発日
        TxtActualDepDate.Text = work.WF_SEL_ACTUALDEPDATE.Text
        '(実績)積車着日
        TxtActualArrDate.Text = work.WF_SEL_ACTUALARRDATE.Text
        '(実績)受入日
        TxtActualAccDate.Text = work.WF_SEL_ACTUALACCDATE.Text
        '(実績)空車着日
        TxtActualEmparrDate.Text = work.WF_SEL_ACTUALEMPARRDATE.Text

        '○ 名称設定処理
        '会社コード
        CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)
        '運用部署
        CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_UORG_TEXT.Text, WW_DUMMY)
        '荷主
        CODENAME_get("SHIPPERS", TxtShippersCode.Text, LblShippersName.Text, WW_DUMMY)
        '荷受人
        CODENAME_get("CONSIGNEE", TxtConsigneeCode.Text, LblConsigneeName.Text, WW_DUMMY)
        '発駅
        CODENAME_get("DEPSTATION", TxtDepstationCode.Text, LblDepstationName.Text, WW_DUMMY)
        '着駅
        CODENAME_get("ARRSTATION", TxtArrstationCode.Text, LblArrstationName.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0001tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(OIT0001tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To OIT0001tbl.Rows.Count - 1
            If OIT0001tbl.Rows(i)("HIDDEN") = "0" Then
                OIT0001tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIT0001tbl)

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
    ''' 詳細画面 初期設定(空明細作成 イベント追加)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Repeater_INIT()
        Dim dataTable As DataTable = New DataTable
        '○詳細ヘッダーの設定
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_DUMMY)

        'WF_CAMPCODE.ReadOnly = True
        'WF_CAMPCODE.Style.Add("background-color", "rgb(213,208,181)")
        'WF_SHARYOTYPE.ReadOnly = True
        'WF_SHARYOTYPE.Style.Add("background-color", "rgb(213,208,181)")
        'WF_TSHABAN.ReadOnly = True
        'WF_TSHABAN.Style.Add("background-color", "rgb(213,208,181)")

        'カラム情報をリピーター作成用に取得
        Master.CreateEmptyTable(dataTable)
        dataTable.Rows.Add(dataTable.NewRow())

        '○ディテール01（タンク車割当）変数設定 
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "MANG"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep1
        CS0052DetailView.COLPREFIX = "WF_Rep1_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール02（タンク車明細）変数設定 
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "SYAB"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep2
        CS0052DetailView.COLPREFIX = "WF_Rep2_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール03（入換・積込指示）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "FCTR"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep3
        CS0052DetailView.COLPREFIX = "WF_Rep3_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール04（費用入力）変数設定
        CS0052DetailView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0052DetailView.PROFID = Master.PROF_VIEW
        CS0052DetailView.MAPID = Master.MAPID
        CS0052DetailView.VARI = Master.VIEWID
        CS0052DetailView.TABID = "OTNK"
        CS0052DetailView.SRCDATA = dataTable
        CS0052DetailView.REPEATER = WF_DViewRep4
        CS0052DetailView.COLPREFIX = "WF_Rep4_"
        CS0052DetailView.MaketDetailView()
        If Not isNormal(CS0052DetailView.ERR) Then
            Exit Sub
        End If

        '○ディテール01（管理）イベント設定 
        Dim WW_FIELD As Label = Nothing
        Dim WW_VALUE As TextBox = Nothing
        Dim WW_FIELDNM As Label = Nothing
        Dim WW_ATTR As String = ""

        For tabindex As Integer = 1 To CONST_MAX_TABID
            Dim rep As Repeater = CType(WF_DetailMView.FindControl("WF_DViewRep" & tabindex), Repeater)
            For Each reitem As RepeaterItem In rep.Items
                'ダブルクリック時コード検索イベント追加
                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label).Text <> "" Then
                    WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_1"), Label)
                    WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_1"), TextBox)
                    ATTR_get(WW_FIELD.Text, WW_ATTR)
                    If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
                        WW_VALUE.Attributes.Remove("ondblclick")
                        WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
                        WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_1"), Label)
                        WW_FIELDNM.Attributes.Remove("style")
                        WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
                    End If
                End If

                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label).Text <> "" Then
                    WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_2"), Label)
                    WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_2"), TextBox)
                    ATTR_get(WW_FIELD.Text, WW_ATTR)
                    If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
                        WW_VALUE.Attributes.Remove("ondblclick")
                        WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
                        WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_2"), Label)
                        WW_FIELDNM.Attributes.Remove("style")
                        WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
                    End If
                End If

                If CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label).Text <> "" Then
                    WW_FIELD = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELD_3"), Label)
                    WW_VALUE = CType(reitem.FindControl("WF_Rep" & tabindex & "_VALUE_3"), TextBox)
                    ATTR_get(WW_FIELD.Text, WW_ATTR)
                    If WW_ATTR <> "" AndAlso Not WW_VALUE.ReadOnly Then
                        WW_VALUE.Attributes.Remove("ondblclick")
                        WW_VALUE.Attributes.Add("ondblclick", WW_ATTR)
                        WW_FIELDNM = CType(reitem.FindControl("WF_Rep" & tabindex & "_FIELDNM_3"), Label)
                        WW_FIELDNM.Attributes.Remove("style")
                        WW_FIELDNM.Attributes.Add("style", "text-decoration: underline;")
                    End If
                End If
            Next
        Next

    End Sub

    ' *** 詳細画面-イベント文字取得
    Protected Sub ATTR_get(ByVal I_FIELD As String, ByRef O_ATTR As String)

        O_ATTR = ""
        Select Case I_FIELD
            Case "CAMPCODE"
                '会社コード
                O_ATTR = "REF_Field_DBclick('CAMPCODE', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_COMPANY & "');"
            Case "DELFLG"
                '削除フラグ
                O_ATTR = "REF_Field_DBclick('DELFLG', 'WF_Rep_FIELD' , '" & LIST_BOX_CLASSIFICATION.LC_DELFLG & "');"
        End Select

    End Sub

    ''' <summary>
    ''' タブ切替
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Detail_TABChange()

        Dim WW_DTABChange As Integer
        Try
            Integer.TryParse(WF_DTAB_CHANGE_NO.Value, WW_DTABChange)
        Catch ex As Exception
            WW_DTABChange = 0
        End Try

        WF_DetailMView.ActiveViewIndex = WW_DTABChange

        '初期値（書式）変更

        'タンク車割当
        WF_Dtab01.Style.Remove("color")
        WF_Dtab01.Style.Add("color", "black")
        WF_Dtab01.Style.Remove("background-color")
        WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab01.Style.Remove("border")
        WF_Dtab01.Style.Add("border", "1px solid black")
        WF_Dtab01.Style.Remove("font-weight")
        WF_Dtab01.Style.Add("font-weight", "normal")

        'タンク車明細
        WF_Dtab02.Style.Remove("color")
        WF_Dtab02.Style.Add("color", "black")
        WF_Dtab02.Style.Remove("background-color")
        WF_Dtab02.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab02.Style.Remove("border")
        WF_Dtab02.Style.Add("border", "1px solid black")
        WF_Dtab02.Style.Remove("font-weight")
        WF_Dtab02.Style.Add("font-weight", "normal")

        '入換・積込指示
        WF_Dtab03.Style.Remove("color")
        WF_Dtab03.Style.Add("color", "black")
        WF_Dtab03.Style.Remove("background-color")
        WF_Dtab03.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab03.Style.Remove("border")
        WF_Dtab03.Style.Add("border", "1px solid black")
        WF_Dtab03.Style.Remove("font-weight")
        WF_Dtab03.Style.Add("font-weight", "normal")

        '費用入力
        WF_Dtab04.Style.Remove("color")
        WF_Dtab04.Style.Add("color", "black")
        WF_Dtab04.Style.Remove("background-color")
        WF_Dtab04.Style.Add("background-color", "rgb(174,170,170)")
        WF_Dtab04.Style.Remove("border")
        WF_Dtab04.Style.Add("border", "1px solid black")
        WF_Dtab04.Style.Remove("font-weight")
        WF_Dtab04.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                'タンク車割当
                WF_Dtab01.Style.Remove("color")
                WF_Dtab01.Style.Add("color", "blue")
                WF_Dtab01.Style.Remove("background-color")
                WF_Dtab01.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab01.Style.Remove("border")
                WF_Dtab01.Style.Add("border", "1px solid blue")
                WF_Dtab01.Style.Remove("font-weight")
                WF_Dtab01.Style.Add("font-weight", "bold")
            Case 1
                'タンク車明細
                WF_Dtab02.Style.Remove("color")
                WF_Dtab02.Style.Add("color", "blue")
                WF_Dtab02.Style.Remove("background-color")
                WF_Dtab02.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab02.Style.Remove("border")
                WF_Dtab02.Style.Add("border", "1px solid blue")
                WF_Dtab02.Style.Remove("font-weight")
                WF_Dtab02.Style.Add("font-weight", "bold")
            Case 2
                '入換・積込指示
                WF_Dtab03.Style.Remove("color")
                WF_Dtab03.Style.Add("color", "blue")
                WF_Dtab03.Style.Remove("background-color")
                WF_Dtab03.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab03.Style.Remove("border")
                WF_Dtab03.Style.Add("border", "1px solid blue")
                WF_Dtab03.Style.Remove("font-weight")
                WF_Dtab03.Style.Add("font-weight", "bold")
            Case 3
                '費用入力
                WF_Dtab04.Style.Remove("color")
                WF_Dtab04.Style.Add("color", "blue")
                WF_Dtab04.Style.Remove("background-color")
                WF_Dtab04.Style.Add("background-color", "rgb(174,170,170)")
                WF_Dtab04.Style.Remove("border")
                WF_Dtab04.Style.Add("border", "1px solid blue")
                WF_Dtab04.Style.Remove("font-weight")
                WF_Dtab04.Style.Add("font-weight", "bold")
        End Select
    End Sub

    ''' <summary>
    ''' タブ指定時表示判定処理
    ''' </summary>
    Protected Sub TAB_DisplayCTRL()
        'Const C_SHARYOTYPE_FRONT As String = "前"
        'Const C_SHARYOTYPE_BACK As String = "後"
        'Dim WW_RESULT As String = ""
        'Dim WW_SHARYOTYPE2 As String = ""
        'Dim WW_MANGOILTYPE As String = ""

        WF_DViewRep1.Visible = False
        WF_DViewRep2.Visible = False
        WF_DViewRep3.Visible = False
        WF_DViewRep4.Visible = False

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                WF_DViewRep1.Visible = True
            Case 1
                WF_DViewRep2.Visible = True
            Case 2
                WF_DViewRep3.Visible = True
            Case 3
                WF_DViewRep4.Visible = True
        End Select

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

                Case "SHIPPERS"         '荷主
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIPPERS"))

                Case "CONSIGNEE"        '荷受人
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SALESOFFICE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SHIPPERS"))

                Case "DEPSTATION"       '発駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DEPSTATION"))

                Case "ARRSTATION"       '着駅
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "ARRSTATION"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
﻿''************************************************************
' ユーザIDマスタメンテ登録画面
' 作成日 2019/11/14
' 更新日 2021/04/09
' 作成者 JOT遠藤
' 更新車 JOT伊草
'
' 修正履歴:2019/11/14 新規作成
'         :2021/04/09 
'         :2021/04/09 1)表更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
'         :2021/04/15 1)表示順nが未入力の場合にエラーとなるバグに対応
'                     2)新規登録を行った際に、一覧画面に新規登録データが追加されないバグに対応
'                     3)検索画面で会社コードを'01'以外にしてメニューまで戻った場合に
'                       メニュー画面の左側ボタンが消失するバグに対応
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' ユーザIDマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIS0001UserCreate
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                     'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIS0001tbl As DataTable                                  '一覧格納用テーブル
    Private OIS0001INPtbl As DataTable                               'チェック用テーブル
    Private OIS0001UPDtbl As DataTable                               '更新用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_ORGCODE_INFOSYS As String = "010006"        '組織コード_情報システム部
    Private Const CONST_ORGCODE_OIL As String = "010007"            '組織コード_石油部
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

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                WF_PASSWORD.Attributes("Value") = WF_PASSWORD.Text
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 '戻るボタン押下
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "WF_RadioButonClick"       '(右ボックス)ラジオボタン選択
                            WF_RadioButton_Click()
                        Case "WF_MEMOChange"            '(右ボックス)メモ欄更新
                            WF_RIGHTBOX_Change()
                        Case "btnClearConfirmOk"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
                    End Select
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

            WF_BOXChange.Value = "detailbox"

        Finally
            '○ 格納Table Close
            If Not IsNothing(OIS0001tbl) Then
                OIS0001tbl.Clear()
                OIS0001tbl.Dispose()
                OIS0001tbl = Nothing
            End If

            If Not IsNothing(OIS0001INPtbl) Then
                OIS0001INPtbl.Clear()
                OIS0001INPtbl.Dispose()
                OIS0001INPtbl = Nothing
            End If

            If Not IsNothing(OIS0001UPDtbl) Then
                OIS0001UPDtbl.Clear()
                OIS0001UPDtbl.Dispose()
                OIS0001UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIS0001WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        'Master.CreateXMLSaveFile()

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
        rightview.COMPCODE = Master.USERCAMP
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIS0001L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        'ユーザID
        WF_USERID.Text = work.WF_SEL_USERID.Text

        '社員名（短）
        WF_STAFFNAMES.Text = work.WF_SEL_STAFFNAMES.Text

        '社員名（長）
        WF_STAFFNAMEL.Text = work.WF_SEL_STAFFNAMEL.Text

        '画面ＩＤ
        WF_MAPID.Text = "M00001"

        'パスワード
        WF_PASSWORD.Text = work.WF_SEL_PASSWORD.Text
        WF_PASSWORD.Attributes("Value") = work.WF_SEL_PASSWORD.Text

        '誤り回数
        WF_MISSCNT.Text = work.WF_SEL_MISSCNT.Text

        'パスワード有効期限
        WF_PASSENDYMD.Text = work.WF_SEL_PASSENDYMD.Text

        '開始年月日
        WF_STYMD.Text = work.WF_SEL_STYMD2.Text

        '終了年月日
        WF_ENDYMD.Text = work.WF_SEL_ENDYMD2.Text

        '会社コード
        WF_CAMPCODE.Text = work.WF_SEL_CAMPCODE3.Text
        CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)

        '組織コード
        WF_ORG.Text = work.WF_SEL_ORG2.Text
        CODENAME_get("ORG", WF_ORG.Text, WF_ORG_TEXT.Text, WW_DUMMY)

        'メールアドレス
        WF_EMAIL.Text = work.WF_SEL_EMAIL.Text

        'メニュー表示制御ロール
        WF_MENUROLE.Text = work.WF_SEL_MENUROLE.Text
        CODENAME_get("MENU", WF_MENUROLE.Text, WF_MENUROLE_TEXT.Text, WW_DUMMY)

        '画面参照更新制御ロール
        WF_MAPROLE.Text = work.WF_SEL_MAPROLE.Text
        CODENAME_get("MAP", WF_MAPROLE.Text, WF_MAPROLE_TEXT.Text, WW_DUMMY)

        '画面表示項目制御ロール
        WF_VIEWPROFID.Text = work.WF_SEL_VIEWPROFID.Text
        CODENAME_get("VIEW", WF_VIEWPROFID.Text, WF_VIEWPROFID_TEXT.Text, WW_DUMMY)

        'エクセル出力制御ロール
        WF_RPRTPROFID.Text = work.WF_SEL_RPRTPROFID.Text
        CODENAME_get("XML", WF_RPRTPROFID.Text, WF_RPRTPROFID_TEXT.Text, WW_DUMMY)

        '画面初期値ロール
        WF_VARIANT.Text = work.WF_SEL_VARIANT.Text

        '承認権限ロール
        WF_APPROVALID.Text = work.WF_SEL_APPROVALID.Text
        CODENAME_get("APPROVAL", WF_APPROVALID.Text, WF_APPROVALID_TEXT.Text, WW_DUMMY)

        '情報出力ID1
        WF_OUTPUTID1.Text = work.WF_SEL_OUTPUTID1.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID1.Text, WF_OUTPUTID1_TEXT.Text, WW_DUMMY)

        '表示フラグ1
        WF_ONOFF1.Text = work.WF_SEL_ONOFF1.Text
        CODENAME_get("ONOFF", WF_ONOFF1.Text, WF_ONOFF1_TEXT.Text, WW_DUMMY)

        '表示順1
        WF_SORTNO1.Text = work.WF_SEL_SORTNO1.Text

        '情報出力ID2
        WF_OUTPUTID2.Text = work.WF_SEL_OUTPUTID2.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID2.Text, WF_OUTPUTID2_TEXT.Text, WW_DUMMY)

        '表示フラグ2
        WF_ONOFF2.Text = work.WF_SEL_ONOFF2.Text
        CODENAME_get("ONOFF", WF_ONOFF2.Text, WF_ONOFF2_TEXT.Text, WW_DUMMY)

        '表示順2
        WF_SORTNO2.Text = work.WF_SEL_SORTNO2.Text

        '情報出力ID3
        WF_OUTPUTID3.Text = work.WF_SEL_OUTPUTID3.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID3.Text, WF_OUTPUTID3_TEXT.Text, WW_DUMMY)

        '表示フラグ3
        WF_ONOFF3.Text = work.WF_SEL_ONOFF3.Text
        CODENAME_get("ONOFF", WF_ONOFF3.Text, WF_ONOFF3_TEXT.Text, WW_DUMMY)

        '表示順3
        WF_SORTNO3.Text = work.WF_SEL_SORTNO3.Text

        '情報出力ID4
        WF_OUTPUTID4.Text = work.WF_SEL_OUTPUTID4.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID4.Text, WF_OUTPUTID4_TEXT.Text, WW_DUMMY)

        '表示フラグ4
        WF_ONOFF4.Text = work.WF_SEL_ONOFF4.Text
        CODENAME_get("ONOFF", WF_ONOFF4.Text, WF_ONOFF4_TEXT.Text, WW_DUMMY)

        '表示順4
        WF_SORTNO4.Text = work.WF_SEL_SORTNO4.Text

        '情報出力ID5
        WF_OUTPUTID5.Text = work.WF_SEL_OUTPUTID5.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID5.Text, WF_OUTPUTID5_TEXT.Text, WW_DUMMY)

        '表示フラグ5
        WF_ONOFF5.Text = work.WF_SEL_ONOFF5.Text
        CODENAME_get("ONOFF", WF_ONOFF5.Text, WF_ONOFF5_TEXT.Text, WW_DUMMY)

        '表示順5
        WF_SORTNO5.Text = work.WF_SEL_SORTNO5.Text

        '情報出力ID6
        WF_OUTPUTID6.Text = work.WF_SEL_OUTPUTID6.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID6.Text, WF_OUTPUTID6_TEXT.Text, WW_DUMMY)

        '表示フラグ6
        WF_ONOFF6.Text = work.WF_SEL_ONOFF6.Text
        CODENAME_get("ONOFF", WF_ONOFF6.Text, WF_ONOFF6_TEXT.Text, WW_DUMMY)

        '表示順6
        WF_SORTNO6.Text = work.WF_SEL_SORTNO6.Text

        '情報出力ID7
        WF_OUTPUTID7.Text = work.WF_SEL_OUTPUTID7.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID7.Text, WF_OUTPUTID7_TEXT.Text, WW_DUMMY)

        '表示フラグ7
        WF_ONOFF7.Text = work.WF_SEL_ONOFF7.Text
        CODENAME_get("ONOFF", WF_ONOFF7.Text, WF_ONOFF7_TEXT.Text, WW_DUMMY)

        '表示順7
        WF_SORTNO7.Text = work.WF_SEL_SORTNO7.Text

        '情報出力ID8
        WF_OUTPUTID8.Text = work.WF_SEL_OUTPUTID8.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID8.Text, WF_OUTPUTID8_TEXT.Text, WW_DUMMY)

        '表示フラグ8
        WF_ONOFF8.Text = work.WF_SEL_ONOFF8.Text
        CODENAME_get("ONOFF", WF_ONOFF8.Text, WF_ONOFF8_TEXT.Text, WW_DUMMY)

        '表示順8
        WF_SORTNO8.Text = work.WF_SEL_SORTNO8.Text

        '情報出力ID9
        WF_OUTPUTID9.Text = work.WF_SEL_OUTPUTID9.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID9.Text, WF_OUTPUTID9_TEXT.Text, WW_DUMMY)

        '表示フラグ9
        WF_ONOFF9.Text = work.WF_SEL_ONOFF9.Text
        CODENAME_get("ONOFF", WF_ONOFF9.Text, WF_ONOFF9_TEXT.Text, WW_DUMMY)

        '表示順9
        WF_SORTNO9.Text = work.WF_SEL_SORTNO9.Text

        '情報出力ID10
        WF_OUTPUTID10.Text = work.WF_SEL_OUTPUTID10.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID10.Text, WF_OUTPUTID10_TEXT.Text, WW_DUMMY)

        '表示フラグ10
        WF_ONOFF10.Text = work.WF_SEL_ONOFF10.Text
        CODENAME_get("ONOFF", WF_ONOFF10.Text, WF_ONOFF10_TEXT.Text, WW_DUMMY)

        '表示順10
        WF_SORTNO10.Text = work.WF_SEL_SORTNO10.Text

        '情報出力ID11
        WF_OUTPUTID11.Text = work.WF_SEL_OUTPUTID11.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID11.Text, WF_OUTPUTID11_TEXT.Text, WW_DUMMY)

        '表示フラグ11
        WF_ONOFF11.Text = work.WF_SEL_ONOFF11.Text
        CODENAME_get("ONOFF", WF_ONOFF11.Text, WF_ONOFF11_TEXT.Text, WW_DUMMY)

        '表示順11
        WF_SORTNO11.Text = work.WF_SEL_SORTNO11.Text

        '情報出力ID12
        WF_OUTPUTID12.Text = work.WF_SEL_OUTPUTID12.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID12.Text, WF_OUTPUTID12_TEXT.Text, WW_DUMMY)

        '表示フラグ12
        WF_ONOFF12.Text = work.WF_SEL_ONOFF12.Text
        CODENAME_get("ONOFF", WF_ONOFF12.Text, WF_ONOFF12_TEXT.Text, WW_DUMMY)

        '表示順12
        WF_SORTNO12.Text = work.WF_SEL_SORTNO12.Text

        '情報出力ID13
        WF_OUTPUTID13.Text = work.WF_SEL_OUTPUTID13.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID13.Text, WF_OUTPUTID13_TEXT.Text, WW_DUMMY)

        '表示フラグ13
        WF_ONOFF13.Text = work.WF_SEL_ONOFF13.Text
        CODENAME_get("ONOFF", WF_ONOFF13.Text, WF_ONOFF13_TEXT.Text, WW_DUMMY)

        '表示順13
        WF_SORTNO13.Text = work.WF_SEL_SORTNO13.Text

        '情報出力ID14
        WF_OUTPUTID14.Text = work.WF_SEL_OUTPUTID14.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID14.Text, WF_OUTPUTID14_TEXT.Text, WW_DUMMY)

        '表示フラグ14
        WF_ONOFF14.Text = work.WF_SEL_ONOFF14.Text
        CODENAME_get("ONOFF", WF_ONOFF14.Text, WF_ONOFF14_TEXT.Text, WW_DUMMY)

        '表示順14
        WF_SORTNO14.Text = work.WF_SEL_SORTNO14.Text

        '情報出力ID15
        WF_OUTPUTID15.Text = work.WF_SEL_OUTPUTID15.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID15.Text, WF_OUTPUTID15_TEXT.Text, WW_DUMMY)

        '表示フラグ15
        WF_ONOFF15.Text = work.WF_SEL_ONOFF15.Text
        CODENAME_get("ONOFF", WF_ONOFF15.Text, WF_ONOFF15_TEXT.Text, WW_DUMMY)

        '表示順15
        WF_SORTNO15.Text = work.WF_SEL_SORTNO15.Text

        '情報出力ID16
        WF_OUTPUTID16.Text = work.WF_SEL_OUTPUTID16.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID16.Text, WF_OUTPUTID16_TEXT.Text, WW_DUMMY)

        '表示フラグ16
        WF_ONOFF16.Text = work.WF_SEL_ONOFF16.Text
        CODENAME_get("ONOFF", WF_ONOFF16.Text, WF_ONOFF16_TEXT.Text, WW_DUMMY)

        '表示順16
        WF_SORTNO16.Text = work.WF_SEL_SORTNO16.Text

        '情報出力ID17
        WF_OUTPUTID17.Text = work.WF_SEL_OUTPUTID17.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID17.Text, WF_OUTPUTID17_TEXT.Text, WW_DUMMY)

        '表示フラグ17
        WF_ONOFF17.Text = work.WF_SEL_ONOFF17.Text
        CODENAME_get("ONOFF", WF_ONOFF17.Text, WF_ONOFF17_TEXT.Text, WW_DUMMY)

        '表示順17
        WF_SORTNO17.Text = work.WF_SEL_SORTNO17.Text

        '情報出力ID18
        WF_OUTPUTID18.Text = work.WF_SEL_OUTPUTID18.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID18.Text, WF_OUTPUTID18_TEXT.Text, WW_DUMMY)

        '表示フラグ18
        WF_ONOFF18.Text = work.WF_SEL_ONOFF18.Text
        CODENAME_get("ONOFF", WF_ONOFF18.Text, WF_ONOFF18_TEXT.Text, WW_DUMMY)

        '表示順18
        WF_SORTNO18.Text = work.WF_SEL_SORTNO18.Text

        '情報出力ID19
        WF_OUTPUTID19.Text = work.WF_SEL_OUTPUTID19.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID19.Text, WF_OUTPUTID19_TEXT.Text, WW_DUMMY)

        '表示フラグ19
        WF_ONOFF19.Text = work.WF_SEL_ONOFF19.Text
        CODENAME_get("ONOFF", WF_ONOFF19.Text, WF_ONOFF19_TEXT.Text, WW_DUMMY)

        '表示順19
        WF_SORTNO19.Text = work.WF_SEL_SORTNO19.Text

        '情報出力ID20
        WF_OUTPUTID20.Text = work.WF_SEL_OUTPUTID20.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID20.Text, WF_OUTPUTID20_TEXT.Text, WW_DUMMY)

        '表示フラグ20
        WF_ONOFF20.Text = work.WF_SEL_ONOFF20.Text
        CODENAME_get("ONOFF", WF_ONOFF20.Text, WF_ONOFF20_TEXT.Text, WW_DUMMY)

        '表示順20
        WF_SORTNO20.Text = work.WF_SEL_SORTNO20.Text

        '情報出力ID21
        WF_OUTPUTID21.Text = work.WF_SEL_OUTPUTID21.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID21.Text, WF_OUTPUTID21_TEXT.Text, WW_DUMMY)

        '表示フラグ21
        WF_ONOFF21.Text = work.WF_SEL_ONOFF21.Text
        CODENAME_get("ONOFF", WF_ONOFF21.Text, WF_ONOFF21_TEXT.Text, WW_DUMMY)

        '表示順21
        WF_SORTNO21.Text = work.WF_SEL_SORTNO21.Text

        '情報出力ID22
        WF_OUTPUTID22.Text = work.WF_SEL_OUTPUTID22.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID22.Text, WF_OUTPUTID22_TEXT.Text, WW_DUMMY)

        '表示フラグ22
        WF_ONOFF22.Text = work.WF_SEL_ONOFF22.Text
        CODENAME_get("ONOFF", WF_ONOFF22.Text, WF_ONOFF22_TEXT.Text, WW_DUMMY)

        '表示順22
        WF_SORTNO22.Text = work.WF_SEL_SORTNO22.Text

        '情報出力ID23
        WF_OUTPUTID23.Text = work.WF_SEL_OUTPUTID23.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID23.Text, WF_OUTPUTID23_TEXT.Text, WW_DUMMY)

        '表示フラグ23
        WF_ONOFF23.Text = work.WF_SEL_ONOFF23.Text
        CODENAME_get("ONOFF", WF_ONOFF23.Text, WF_ONOFF23_TEXT.Text, WW_DUMMY)

        '表示順23
        WF_SORTNO23.Text = work.WF_SEL_SORTNO23.Text

        '情報出力ID24
        WF_OUTPUTID24.Text = work.WF_SEL_OUTPUTID24.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID24.Text, WF_OUTPUTID24_TEXT.Text, WW_DUMMY)

        '表示フラグ24
        WF_ONOFF24.Text = work.WF_SEL_ONOFF24.Text
        CODENAME_get("ONOFF", WF_ONOFF24.Text, WF_ONOFF24_TEXT.Text, WW_DUMMY)

        '表示順24
        WF_SORTNO24.Text = work.WF_SEL_SORTNO24.Text

        '情報出力ID25
        WF_OUTPUTID25.Text = work.WF_SEL_OUTPUTID25.Text
        CODENAME_get("OUTPUTID", WF_OUTPUTID25.Text, WF_OUTPUTID25_TEXT.Text, WW_DUMMY)

        '表示フラグ25
        WF_ONOFF25.Text = work.WF_SEL_ONOFF25.Text
        CODENAME_get("ONOFF", WF_ONOFF25.Text, WF_ONOFF25_TEXT.Text, WW_DUMMY)

        '表示順25
        WF_SORTNO25.Text = work.WF_SEL_SORTNO25.Text

        '削除
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

        '削除フラグ・誤り回数・会社コード・組織コードを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_DELFLG.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_MISSCNT.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_CAMPCODE.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_ORG.Attributes("onkeyPress") = "CheckNum()"

        'パスワード有効期限・開始年月日・終了年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_PASSENDYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_STYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_ENDYMD.Attributes("onkeyPress") = "CheckCalendar()"

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As SqlConnection)

        If IsNothing(OIS0001tbl) Then
            OIS0001tbl = New DataTable
        End If

        If OIS0001tbl.Columns.Count <> 0 Then
            OIS0001tbl.Columns.Clear()
        End If

        OIS0001tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをユーザマスタ、ユーザIDマスタから取得する
        Dim SQLStr As String =
            " OPEN SYMMETRIC KEY loginpasskey DECRYPTION BY CERTIFICATE certjotoil; " _
            & " Select " _
            & "    0                                                   As LINECNT " _
            & "    , ''                                                AS OPERATION " _
            & "    , CAST(OIS0004.UPDTIMSTP AS BIGINT)                    AS UPDTIMSTP " _
            & "    , 1                                                 AS 'SELECT' " _
            & "    , 0                                                 AS HIDDEN " _
            & "    , ISNULL(RTRIM(OIS0004.DELFLG), '')                    AS DELFLG " _
            & "    , ISNULL(RTRIM(OIS0004.USERID), '')                    AS USERID " _
            & "    , ISNULL(RTRIM(OIS0004.STAFFNAMES), '')                AS STAFFNAMES " _
            & "    , ISNULL(RTRIM(OIS0004.STAFFNAMEL), '')                AS STAFFNAMEL " _
            & "    , ISNULL(RTRIM(OIS0004.MAPID), '')                     AS MAPID " _
            & "    , CONVERT(nvarchar, DecryptByKey(ISNULL(RTRIM(OIS0005.PASSWORD), ''))) As PASSWORD " _
            & "    , ISNULL(RTRIM(OIS0005.MISSCNT), '')                   AS MISSCNT " _
            & "    , ISNULL(FORMAT(OIS0005.PASSENDYMD, 'yyyy/MM/dd'), '') AS PASSENDYMD " _
            & "    , ISNULL(FORMAT(OIS0004.STYMD, 'yyyy/MM/dd'), '')      AS STYMD " _
            & "    , ISNULL(FORMAT(OIS0004.ENDYMD, 'yyyy/MM/dd'), '')     AS ENDYMD " _
            & "    , ISNULL(RTRIM(OIS0004.CAMPCODE), '')                  AS CAMPCODE " _
            & "    , ''                                                AS CAMPNAMES " _
            & "    , ISNULL(RTRIM(OIS0004.ORG), '')                       AS ORG " _
            & "    , ''                                                AS ORGNAMES " _
            & "    , ISNULL(RTRIM(OIS0004.EMAIL), '')                     AS EMAIL " _
            & "    , ISNULL(RTRIM(OIS0004.MENUROLE), '')                  AS MENUROLE " _
            & "    , ISNULL(RTRIM(OIS0004.MAPROLE), '')                   AS MAPROLE " _
            & "    , ISNULL(RTRIM(OIS0004.VIEWPROFID), '')                AS VIEWPROFID " _
            & "    , ISNULL(RTRIM(OIS0004.RPRTPROFID), '')                AS RPRTPROFID " _
            & "    , ISNULL(RTRIM(OIS0004.VARIANT), '')             AS VARIANT " _
            & "    , ISNULL(RTRIM(OIS0004.APPROVALID), '')                AS APPROVALID " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID1), '')                AS OUTPUTID1 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF1), '')                   AS ONOFF1 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO1), '')                  AS SORTNO1 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID2), '')                AS OUTPUTID2 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF2), '')                   AS ONOFF2 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO2), '')                  AS SORTNO2 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID3), '')                AS OUTPUTID3 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF3), '')                   AS ONOFF3 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO3), '')                  AS SORTNO3 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID4), '')                AS OUTPUTID4 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF4), '')                   AS ONOFF4 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO4), '')                  AS SORTNO4 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID5), '')                AS OUTPUTID5 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF5), '')                   AS ONOFF5 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO5), '')                  AS SORTNO5 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID6), '')                AS OUTPUTID6 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF6), '')                   AS ONOFF6 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO6), '')                  AS SORTNO6 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID7), '')                AS OUTPUTID7 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF7), '')                   AS ONOFF7 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO7), '')                  AS SORTNO7 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID8), '')                AS OUTPUTID8 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF8), '')                   AS ONOFF8 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO8), '')                  AS SORTNO8 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID9), '')                AS OUTPUTID9 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF9), '')                   AS ONOFF9 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO9), '')                  AS SORTNO9 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID10), '')                AS OUTPUTID10 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF10), '')                   AS ONOFF10 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO10), '')                  AS SORTNO10 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID11), '')                AS OUTPUTID11 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF11), '')                   AS ONOFF11 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO11), '')                  AS SORTNO11 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID12), '')                AS OUTPUTID12 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF12), '')                   AS ONOFF12 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO12), '')                  AS SORTNO12 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID13), '')                AS OUTPUTID13 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF13), '')                   AS ONOFF13 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO13), '')                  AS SORTNO13 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID14), '')                AS OUTPUTID14 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF14), '')                   AS ONOFF14 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO14), '')                  AS SORTNO14 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID15), '')                AS OUTPUTID15 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF15), '')                   AS ONOFF15 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO15), '')                  AS SORTNO15 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID16), '')                AS OUTPUTID16 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF16), '')                   AS ONOFF16 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO16), '')                  AS SORTNO16 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID17), '')                AS OUTPUTID17 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF17), '')                   AS ONOFF17 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO17), '')                  AS SORTNO17 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID18), '')                AS OUTPUTID18 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF18), '')                   AS ONOFF18 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO18), '')                  AS SORTNO18 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID19), '')                AS OUTPUTID19 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF19), '')                   AS ONOFF19 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO19), '')                  AS SORTNO19 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID20), '')                AS OUTPUTID20 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF20), '')                   AS ONOFF20 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO20), '')                  AS SORTNO20 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID21), '')                AS OUTPUTID21 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF21), '')                   AS ONOFF21 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO21), '')                  AS SORTNO21 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID22), '')                AS OUTPUTID22 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF22), '')                   AS ONOFF22 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO22), '')                  AS SORTNO22 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID23), '')                AS OUTPUTID23 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF23), '')                   AS ONOFF23 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO23), '')                  AS SORTNO23 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID24), '')                AS OUTPUTID24 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF24), '')                   AS ONOFF24 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO24), '')                  AS SORTNO24 " _
            & "    , ISNULL(RTRIM(OIS0004.OUTPUTID25), '')                AS OUTPUTID25 " _
            & "    , ISNULL(RTRIM(OIS0004.ONOFF25), '')                   AS ONOFF25 " _
            & "    , ISNULL(RTRIM(OIS0004.SORTNO25), '')                  AS SORTNO25 " _
            & " FROM " _
            & "    COM.OIS0004_USER OIS0004 " _
            & "    INNER JOIN COM.OIS0005_USERPASS OIS0005 " _
            & "        ON  OIS0005.USERID   = OIS0004.USERID" _
            & "        AND OIS0005.DELFLG  <> @P6" _
            & " WHERE" _
            & "    OIS0004.CAMPCODE    = @P1" _
            & "    AND OIS0004.STYMD  <= @P4" _
            & "    AND OIS0004.DELFLG <> @P6"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '組織コード
        If Not String.IsNullOrEmpty(work.WF_SEL_ORG.Text) Then
            SQLStr &= String.Format("    AND OIS0004.ORG     = '{0}'", work.WF_SEL_ORG.Text)
        End If

        '有効年月日（終了）
        If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
            SQLStr &= "    AND OIS0004.ENDYMD     >= @P5"
        End If

        SQLStr &=
              " ORDER BY" _
            & "    OIS0004.ORG" _
            & "  , OIS0004.USERID"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)        '会社コード
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", SqlDbType.Date)                '有効年月日(To)
                If Not String.IsNullOrEmpty(work.WF_SEL_ENDYMD.Text) Then
                    Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", SqlDbType.Date)            '有効年月日(From)
                    PARA5.Value = work.WF_SEL_ENDYMD.Text
                End If
                Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", SqlDbType.NVarChar, 1)         '削除フラグ

                PARA1.Value = work.WF_SEL_CAMPCODE2.Text
                PARA4.Value = work.WF_SEL_STYMD.Text
                PARA6.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIS0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIS0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each OIS0001row As DataRow In OIS0001tbl.Rows
                    i += 1
                    OIS0001row("LINECNT") = i        'LINECNT
                    ''名称取得
                    'CODENAME_get("CAMPCODE", OIS0001row("CAMPCODE"), OIS0001row("CAMPNAMES"), WW_DUMMY)                               '会社コード
                    'CODENAME_get("ORG", OIS0001row("ORG"), OIS0001row("ORGNAMES"), WW_DUMMY)                                          '組織コード
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001C Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT " _
            & "     USERID " _
            & "    , STYMD" _
            & " FROM" _
            & "    COM.OIS0004_USER" _
            & " WHERE" _
            & "     USERID   = @P1" _
            & " AND STYMD    = @P2" _
            & " AND DELFLG   <> @P3"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20) 'ユーザーID
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 20) '利用開始日
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", SqlDbType.NVarChar, 1)  '削除フラグ

                PARA1.Value = WF_USERID.Text
                PARA2.Value = WF_STYMD.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIS0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIS0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIS0001Chk.Load(SQLdr)

                    If OIS0001Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' ユーザIDマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新(ユーザマスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        COM.OIS0004_USER" _
            & "    WHERE" _
            & "        USERID       = @P001" _
            & "        AND STYMD    = @P008 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE COM.OIS0004_USER" _
            & "    SET" _
            & "        DELFLG = @P000" _
            & "        , STAFFNAMES = @P002" _
            & "        , STAFFNAMEL = @P003" _
            & "        , MAPID = @P004" _
            & "        , ENDYMD = @P009" _
            & "        , ORG = @P011" _
            & "        , EMAIL = @P012" _
            & "        , MENUROLE = @P013" _
            & "        , MAPROLE = @P014" _
            & "        , VIEWPROFID = @P015" _
            & "        , RPRTPROFID = @P016" _
            & "        , VARIANT = @P017" _
            & "        , APPROVALID = @P018" _
            & "        , OUTPUTID1 = @P019" _
            & "        , ONOFF1 = @P020" _
            & "        , SORTNO1 = @P021" _
            & "        , OUTPUTID2 = @P022" _
            & "        , ONOFF2 = @P023" _
            & "        , SORTNO2 = @P024" _
            & "        , OUTPUTID3 = @P025" _
            & "        , ONOFF3 = @P026" _
            & "        , SORTNO3 = @P027" _
            & "        , OUTPUTID4 = @P028" _
            & "        , ONOFF4 = @P029" _
            & "        , SORTNO4 = @P030" _
            & "        , OUTPUTID5 = @P031" _
            & "        , ONOFF5 = @P032" _
            & "        , SORTNO5 = @P033" _
            & "        , OUTPUTID6 = @P034" _
            & "        , ONOFF6 = @P035" _
            & "        , SORTNO6 = @P036" _
            & "        , OUTPUTID7 = @P037" _
            & "        , ONOFF7 = @P038" _
            & "        , SORTNO7 = @P039" _
            & "        , OUTPUTID8 = @P040" _
            & "        , ONOFF8 = @P041" _
            & "        , SORTNO8 = @P042" _
            & "        , OUTPUTID9 = @P043" _
            & "        , ONOFF9 = @P044" _
            & "        , SORTNO9 = @P045" _
            & "        , OUTPUTID10 = @P046" _
            & "        , ONOFF10 = @P047" _
            & "        , SORTNO10 = @P048" _
            & "        , OUTPUTID11 = @P049" _
            & "        , ONOFF11 = @P050" _
            & "        , SORTNO11 = @P051" _
            & "        , OUTPUTID12 = @P052" _
            & "        , ONOFF12 = @P053" _
            & "        , SORTNO12 = @P054" _
            & "        , OUTPUTID13 = @P055" _
            & "        , ONOFF13 = @P056" _
            & "        , SORTNO13 = @P057" _
            & "        , OUTPUTID14 = @P058" _
            & "        , ONOFF14 = @P059" _
            & "        , SORTNO14 = @P060" _
            & "        , OUTPUTID15 = @P061" _
            & "        , ONOFF15 = @P062" _
            & "        , SORTNO15 = @P063" _
            & "        , OUTPUTID16 = @P064" _
            & "        , ONOFF16 = @P065" _
            & "        , SORTNO16 = @P066" _
            & "        , OUTPUTID17 = @P067" _
            & "        , ONOFF17 = @P068" _
            & "        , SORTNO17 = @P069" _
            & "        , OUTPUTID18 = @P070" _
            & "        , ONOFF18 = @P071" _
            & "        , SORTNO18 = @P072" _
            & "        , OUTPUTID19 = @P073" _
            & "        , ONOFF19 = @P074" _
            & "        , SORTNO19 = @P075" _
            & "        , OUTPUTID20 = @P076" _
            & "        , ONOFF20 = @P077" _
            & "        , SORTNO20 = @P078" _
            & "        , OUTPUTID21 = @P079" _
            & "        , ONOFF21 = @P080" _
            & "        , SORTNO21 = @P081" _
            & "        , OUTPUTID22 = @P082" _
            & "        , ONOFF22 = @P083" _
            & "        , SORTNO22 = @P084" _
            & "        , OUTPUTID23 = @P085" _
            & "        , ONOFF23 = @P086" _
            & "        , SORTNO23 = @P087" _
            & "        , OUTPUTID24 = @P088" _
            & "        , ONOFF24 = @P089" _
            & "        , SORTNO24 = @P090" _
            & "        , OUTPUTID25 = @P091" _
            & "        , ONOFF25 = @P092" _
            & "        , SORTNO25 = @P093" _
            & "        , UPDYMD = @P097" _
            & "        , UPDUSER = @P098" _
            & "        , UPDTERMID = @P099" _
            & "        , RECEIVEYMD = @P100" _
            & "    WHERE" _
            & "        USERID       = @P001" _
            & "        AND STYMD    = @P008 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO COM.OIS0004_USER" _
            & "        (DELFLG" _
            & "        , USERID" _
            & "        , STAFFNAMES" _
            & "        , STAFFNAMEL" _
            & "        , MAPID" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , CAMPCODE" _
            & "        , ORG" _
            & "        , EMAIL" _
            & "        , MENUROLE" _
            & "        , MAPROLE" _
            & "        , VIEWPROFID" _
            & "        , RPRTPROFID" _
            & "        , VARIANT" _
            & "        , APPROVALID" _
            & "        , OUTPUTID1" _
            & "        , ONOFF1" _
            & "        , SORTNO1" _
            & "        , OUTPUTID2" _
            & "        , ONOFF2" _
            & "        , SORTNO2" _
            & "        , OUTPUTID3" _
            & "        , ONOFF3" _
            & "        , SORTNO3" _
            & "        , OUTPUTID4" _
            & "        , ONOFF4" _
            & "        , SORTNO4" _
            & "        , OUTPUTID5" _
            & "        , ONOFF5" _
            & "        , SORTNO5" _
            & "        , OUTPUTID6" _
            & "        , ONOFF6" _
            & "        , SORTNO6" _
            & "        , OUTPUTID7" _
            & "        , ONOFF7" _
            & "        , SORTNO7" _
            & "        , OUTPUTID8" _
            & "        , ONOFF8" _
            & "        , SORTNO8" _
            & "        , OUTPUTID9" _
            & "        , ONOFF9" _
            & "        , SORTNO9" _
            & "        , OUTPUTID10" _
            & "        , ONOFF10" _
            & "        , SORTNO10" _
            & "        , OUTPUTID11" _
            & "        , ONOFF11" _
            & "        , SORTNO11" _
            & "        , OUTPUTID12" _
            & "        , ONOFF12" _
            & "        , SORTNO12" _
            & "        , OUTPUTID13" _
            & "        , ONOFF13" _
            & "        , SORTNO13" _
            & "        , OUTPUTID14" _
            & "        , ONOFF14" _
            & "        , SORTNO14" _
            & "        , OUTPUTID15" _
            & "        , ONOFF15" _
            & "        , SORTNO15" _
            & "        , OUTPUTID16" _
            & "        , ONOFF16" _
            & "        , SORTNO16" _
            & "        , OUTPUTID17" _
            & "        , ONOFF17" _
            & "        , SORTNO17" _
            & "        , OUTPUTID18" _
            & "        , ONOFF18" _
            & "        , SORTNO18" _
            & "        , OUTPUTID19" _
            & "        , ONOFF19" _
            & "        , SORTNO19" _
            & "        , OUTPUTID20" _
            & "        , ONOFF20" _
            & "        , SORTNO20" _
            & "        , OUTPUTID21" _
            & "        , ONOFF21" _
            & "        , SORTNO21" _
            & "        , OUTPUTID22" _
            & "        , ONOFF22" _
            & "        , SORTNO22" _
            & "        , OUTPUTID23" _
            & "        , ONOFF23" _
            & "        , SORTNO23" _
            & "        , OUTPUTID24" _
            & "        , ONOFF24" _
            & "        , SORTNO24" _
            & "        , OUTPUTID25" _
            & "        , ONOFF25" _
            & "        , SORTNO25" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P000" _
            & "        , @P001" _
            & "        , @P002" _
            & "        , @P003" _
            & "        , @P004" _
            & "        , @P008" _
            & "        , @P009" _
            & "        , @P010" _
            & "        , @P011" _
            & "        , @P012" _
            & "        , @P013" _
            & "        , @P014" _
            & "        , @P015" _
            & "        , @P016" _
            & "        , @P017" _
            & "        , @P018" _
            & "        , @P019" _
            & "        , @P020" _
            & "        , @P021" _
            & "        , @P022" _
            & "        , @P023" _
            & "        , @P024" _
            & "        , @P025" _
            & "        , @P026" _
            & "        , @P027" _
            & "        , @P028" _
            & "        , @P029" _
            & "        , @P030" _
            & "        , @P031" _
            & "        , @P032" _
            & "        , @P033" _
            & "        , @P034" _
            & "        , @P035" _
            & "        , @P036" _
            & "        , @P037" _
            & "        , @P038" _
            & "        , @P039" _
            & "        , @P040" _
            & "        , @P041" _
            & "        , @P042" _
            & "        , @P043" _
            & "        , @P044" _
            & "        , @P045" _
            & "        , @P046" _
            & "        , @P047" _
            & "        , @P048" _
            & "        , @P049" _
            & "        , @P050" _
            & "        , @P051" _
            & "        , @P052" _
            & "        , @P053" _
            & "        , @P054" _
            & "        , @P055" _
            & "        , @P056" _
            & "        , @P057" _
            & "        , @P058" _
            & "        , @P059" _
            & "        , @P060" _
            & "        , @P061" _
            & "        , @P062" _
            & "        , @P063" _
            & "        , @P064" _
            & "        , @P065" _
            & "        , @P066" _
            & "        , @P067" _
            & "        , @P068" _
            & "        , @P069" _
            & "        , @P070" _
            & "        , @P071" _
            & "        , @P072" _
            & "        , @P073" _
            & "        , @P074" _
            & "        , @P075" _
            & "        , @P076" _
            & "        , @P077" _
            & "        , @P078" _
            & "        , @P079" _
            & "        , @P080" _
            & "        , @P081" _
            & "        , @P082" _
            & "        , @P083" _
            & "        , @P084" _
            & "        , @P085" _
            & "        , @P086" _
            & "        , @P087" _
            & "        , @P088" _
            & "        , @P089" _
            & "        , @P090" _
            & "        , @P091" _
            & "        , @P092" _
            & "        , @P093" _
            & "        , @P094" _
            & "        , @P095" _
            & "        , @P096" _
            & "        , @P097" _
            & "        , @P098" _
            & "        , @P099" _
            & "        , @P100) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " Select" _
            & "    DELFLG" _
            & "        , USERID" _
            & "        , STAFFNAMES" _
            & "        , STAFFNAMEL" _
            & "        , MAPID" _
            & "        , STYMD" _
            & "        , ENDYMD" _
            & "        , CAMPCODE" _
            & "        , ORG" _
            & "        , EMAIL" _
            & "        , MENUROLE" _
            & "        , MAPROLE" _
            & "        , VIEWPROFID" _
            & "        , RPRTPROFID" _
            & "        , VARIANT" _
            & "        , APPROVALID" _
            & "        , OUTPUTID1" _
            & "        , ONOFF1" _
            & "        , SORTNO1" _
            & "        , OUTPUTID2" _
            & "        , ONOFF2" _
            & "        , SORTNO2" _
            & "        , OUTPUTID3" _
            & "        , ONOFF3" _
            & "        , SORTNO3" _
            & "        , OUTPUTID4" _
            & "        , ONOFF4" _
            & "        , SORTNO4" _
            & "        , OUTPUTID5" _
            & "        , ONOFF5" _
            & "        , SORTNO5" _
            & "        , OUTPUTID6" _
            & "        , ONOFF6" _
            & "        , SORTNO6" _
            & "        , OUTPUTID7" _
            & "        , ONOFF7" _
            & "        , SORTNO7" _
            & "        , OUTPUTID8" _
            & "        , ONOFF8" _
            & "        , SORTNO8" _
            & "        , OUTPUTID9" _
            & "        , ONOFF9" _
            & "        , SORTNO9" _
            & "        , OUTPUTID10" _
            & "        , ONOFF10" _
            & "        , SORTNO10" _
            & "        , OUTPUTID11" _
            & "        , ONOFF11" _
            & "        , SORTNO11" _
            & "        , OUTPUTID12" _
            & "        , ONOFF12" _
            & "        , SORTNO12" _
            & "        , OUTPUTID13" _
            & "        , ONOFF13" _
            & "        , SORTNO13" _
            & "        , OUTPUTID14" _
            & "        , ONOFF14" _
            & "        , SORTNO14" _
            & "        , OUTPUTID15" _
            & "        , ONOFF15" _
            & "        , SORTNO15" _
            & "        , OUTPUTID16" _
            & "        , ONOFF16" _
            & "        , SORTNO16" _
            & "        , OUTPUTID17" _
            & "        , ONOFF17" _
            & "        , SORTNO17" _
            & "        , OUTPUTID18" _
            & "        , ONOFF18" _
            & "        , SORTNO18" _
            & "        , OUTPUTID19" _
            & "        , ONOFF19" _
            & "        , SORTNO19" _
            & "        , OUTPUTID20" _
            & "        , ONOFF20" _
            & "        , SORTNO20" _
            & "        , OUTPUTID21" _
            & "        , ONOFF21" _
            & "        , SORTNO21" _
            & "        , OUTPUTID22" _
            & "        , ONOFF22" _
            & "        , SORTNO22" _
            & "        , OUTPUTID23" _
            & "        , ONOFF23" _
            & "        , SORTNO23" _
            & "        , OUTPUTID24" _
            & "        , ONOFF24" _
            & "        , SORTNO24" _
            & "        , OUTPUTID25" _
            & "        , ONOFF25" _
            & "        , SORTNO25" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    COM.OIS0004_USER" _
            & " WHERE" _
            & "        USERID       = @P001" _
            & "        AND STYMD    = @P008"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA000 As SqlParameter = SQLcmd.Parameters.Add("@P000", SqlDbType.NVarChar, 1)         '削除フラグ
                Dim PARA001 As SqlParameter = SQLcmd.Parameters.Add("@P001", SqlDbType.NVarChar, 20)        'ユーザID
                Dim PARA002 As SqlParameter = SQLcmd.Parameters.Add("@P002", SqlDbType.NVarChar, 20)        '社員名（短）
                Dim PARA003 As SqlParameter = SQLcmd.Parameters.Add("@P003", SqlDbType.NVarChar, 50)        '社員名（長）
                Dim PARA004 As SqlParameter = SQLcmd.Parameters.Add("@P004", SqlDbType.NVarChar, 20)        '画面ＩＤ
                'Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P005", SqlDbType.NVarChar, 200)        'パスワード
                'Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P006", SqlDbType.Int)                  '誤り回数
                'Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P007", SqlDbType.Date)                 'パスワード有効期限
                Dim PARA008 As SqlParameter = SQLcmd.Parameters.Add("@P008", SqlDbType.Date)                '開始年月日
                Dim PARA009 As SqlParameter = SQLcmd.Parameters.Add("@P009", SqlDbType.Date)                '終了年月日
                Dim PARA010 As SqlParameter = SQLcmd.Parameters.Add("@P010", SqlDbType.NVarChar, 2)         '会社コード
                Dim PARA011 As SqlParameter = SQLcmd.Parameters.Add("@P011", SqlDbType.NVarChar, 6)         '組織コード
                Dim PARA012 As SqlParameter = SQLcmd.Parameters.Add("@P012", SqlDbType.NVarChar, 128)       'メールアドレス
                Dim PARA013 As SqlParameter = SQLcmd.Parameters.Add("@P013", SqlDbType.NVarChar, 20)        'メニュー表示制御ロール
                Dim PARA014 As SqlParameter = SQLcmd.Parameters.Add("@P014", SqlDbType.NVarChar, 20)        '画面参照更新制御ロール
                Dim PARA015 As SqlParameter = SQLcmd.Parameters.Add("@P015", SqlDbType.NVarChar, 20)        '画面表示項目制御ロール
                Dim PARA016 As SqlParameter = SQLcmd.Parameters.Add("@P016", SqlDbType.NVarChar, 20)        'エクセル出力制御ロール
                Dim PARA017 As SqlParameter = SQLcmd.Parameters.Add("@P017", SqlDbType.NVarChar, 20)        '画面初期値ロール
                Dim PARA018 As SqlParameter = SQLcmd.Parameters.Add("@P018", SqlDbType.NVarChar, 20)        '承認権限ロール
                Dim PARA019 As SqlParameter = SQLcmd.Parameters.Add("@P019", SqlDbType.NVarChar, 4)         '情報出力ID1
                Dim PARA020 As SqlParameter = SQLcmd.Parameters.Add("@P020", SqlDbType.NVarChar, 1)         '表示フラグ1
                Dim PARA021 As SqlParameter = SQLcmd.Parameters.Add("@P021", SqlDbType.Int)                 '表示順1
                Dim PARA022 As SqlParameter = SQLcmd.Parameters.Add("@P022", SqlDbType.NVarChar, 4)         '情報出力ID2
                Dim PARA023 As SqlParameter = SQLcmd.Parameters.Add("@P023", SqlDbType.NVarChar, 1)         '表示フラグ2
                Dim PARA024 As SqlParameter = SQLcmd.Parameters.Add("@P024", SqlDbType.Int)                 '表示順2
                Dim PARA025 As SqlParameter = SQLcmd.Parameters.Add("@P025", SqlDbType.NVarChar, 4)         '情報出力ID3
                Dim PARA026 As SqlParameter = SQLcmd.Parameters.Add("@P026", SqlDbType.NVarChar, 1)         '表示フラグ3
                Dim PARA027 As SqlParameter = SQLcmd.Parameters.Add("@P027", SqlDbType.Int)                 '表示順3
                Dim PARA028 As SqlParameter = SQLcmd.Parameters.Add("@P028", SqlDbType.NVarChar, 4)         '情報出力ID4
                Dim PARA029 As SqlParameter = SQLcmd.Parameters.Add("@P029", SqlDbType.NVarChar, 1)         '表示フラグ4
                Dim PARA030 As SqlParameter = SQLcmd.Parameters.Add("@P030", SqlDbType.Int)                 '表示順4
                Dim PARA031 As SqlParameter = SQLcmd.Parameters.Add("@P031", SqlDbType.NVarChar, 4)         '情報出力ID5
                Dim PARA032 As SqlParameter = SQLcmd.Parameters.Add("@P032", SqlDbType.NVarChar, 1)         '表示フラグ5
                Dim PARA033 As SqlParameter = SQLcmd.Parameters.Add("@P033", SqlDbType.Int)                 '表示順5
                Dim PARA034 As SqlParameter = SQLcmd.Parameters.Add("@P034", SqlDbType.NVarChar, 4)         '情報出力ID6
                Dim PARA035 As SqlParameter = SQLcmd.Parameters.Add("@P035", SqlDbType.NVarChar, 1)         '表示フラグ6
                Dim PARA036 As SqlParameter = SQLcmd.Parameters.Add("@P036", SqlDbType.Int)                 '表示順6
                Dim PARA037 As SqlParameter = SQLcmd.Parameters.Add("@P037", SqlDbType.NVarChar, 4)         '情報出力ID7
                Dim PARA038 As SqlParameter = SQLcmd.Parameters.Add("@P038", SqlDbType.NVarChar, 1)         '表示フラグ7
                Dim PARA039 As SqlParameter = SQLcmd.Parameters.Add("@P039", SqlDbType.Int)                 '表示順7
                Dim PARA040 As SqlParameter = SQLcmd.Parameters.Add("@P040", SqlDbType.NVarChar, 4)         '情報出力ID8
                Dim PARA041 As SqlParameter = SQLcmd.Parameters.Add("@P041", SqlDbType.NVarChar, 1)         '表示フラグ8
                Dim PARA042 As SqlParameter = SQLcmd.Parameters.Add("@P042", SqlDbType.Int)                 '表示順8
                Dim PARA043 As SqlParameter = SQLcmd.Parameters.Add("@P043", SqlDbType.NVarChar, 4)         '情報出力ID9
                Dim PARA044 As SqlParameter = SQLcmd.Parameters.Add("@P044", SqlDbType.NVarChar, 1)         '表示フラグ9
                Dim PARA045 As SqlParameter = SQLcmd.Parameters.Add("@P045", SqlDbType.Int)                 '表示順9
                Dim PARA046 As SqlParameter = SQLcmd.Parameters.Add("@P046", SqlDbType.NVarChar, 4)         '情報出力ID10
                Dim PARA047 As SqlParameter = SQLcmd.Parameters.Add("@P047", SqlDbType.NVarChar, 1)         '表示フラグ10
                Dim PARA048 As SqlParameter = SQLcmd.Parameters.Add("@P048", SqlDbType.Int)                 '表示順10
                Dim PARA049 As SqlParameter = SQLcmd.Parameters.Add("@P049", SqlDbType.NVarChar, 4)         '情報出力ID11
                Dim PARA050 As SqlParameter = SQLcmd.Parameters.Add("@P050", SqlDbType.NVarChar, 1)         '表示フラグ11
                Dim PARA051 As SqlParameter = SQLcmd.Parameters.Add("@P051", SqlDbType.Int)                 '表示順11
                Dim PARA052 As SqlParameter = SQLcmd.Parameters.Add("@P052", SqlDbType.NVarChar, 4)         '情報出力ID12
                Dim PARA053 As SqlParameter = SQLcmd.Parameters.Add("@P053", SqlDbType.NVarChar, 1)         '表示フラグ12
                Dim PARA054 As SqlParameter = SQLcmd.Parameters.Add("@P054", SqlDbType.Int)                 '表示順12
                Dim PARA055 As SqlParameter = SQLcmd.Parameters.Add("@P055", SqlDbType.NVarChar, 4)         '情報出力ID13
                Dim PARA056 As SqlParameter = SQLcmd.Parameters.Add("@P056", SqlDbType.NVarChar, 1)         '表示フラグ13
                Dim PARA057 As SqlParameter = SQLcmd.Parameters.Add("@P057", SqlDbType.Int)                 '表示順13
                Dim PARA058 As SqlParameter = SQLcmd.Parameters.Add("@P058", SqlDbType.NVarChar, 4)         '情報出力ID14
                Dim PARA059 As SqlParameter = SQLcmd.Parameters.Add("@P059", SqlDbType.NVarChar, 1)         '表示フラグ14
                Dim PARA060 As SqlParameter = SQLcmd.Parameters.Add("@P060", SqlDbType.Int)                 '表示順14
                Dim PARA061 As SqlParameter = SQLcmd.Parameters.Add("@P061", SqlDbType.NVarChar, 4)         '情報出力ID15
                Dim PARA062 As SqlParameter = SQLcmd.Parameters.Add("@P062", SqlDbType.NVarChar, 1)         '表示フラグ15
                Dim PARA063 As SqlParameter = SQLcmd.Parameters.Add("@P063", SqlDbType.Int)                 '表示順15
                Dim PARA064 As SqlParameter = SQLcmd.Parameters.Add("@P064", SqlDbType.NVarChar, 4)         '情報出力ID16
                Dim PARA065 As SqlParameter = SQLcmd.Parameters.Add("@P065", SqlDbType.NVarChar, 1)         '表示フラグ16
                Dim PARA066 As SqlParameter = SQLcmd.Parameters.Add("@P066", SqlDbType.Int)                 '表示順16
                Dim PARA067 As SqlParameter = SQLcmd.Parameters.Add("@P067", SqlDbType.NVarChar, 4)         '情報出力ID17
                Dim PARA068 As SqlParameter = SQLcmd.Parameters.Add("@P068", SqlDbType.NVarChar, 1)         '表示フラグ17
                Dim PARA069 As SqlParameter = SQLcmd.Parameters.Add("@P069", SqlDbType.Int)                 '表示順17
                Dim PARA070 As SqlParameter = SQLcmd.Parameters.Add("@P070", SqlDbType.NVarChar, 4)         '情報出力ID18
                Dim PARA071 As SqlParameter = SQLcmd.Parameters.Add("@P071", SqlDbType.NVarChar, 1)         '表示フラグ18
                Dim PARA072 As SqlParameter = SQLcmd.Parameters.Add("@P072", SqlDbType.Int)                 '表示順18
                Dim PARA073 As SqlParameter = SQLcmd.Parameters.Add("@P073", SqlDbType.NVarChar, 4)         '情報出力ID19
                Dim PARA074 As SqlParameter = SQLcmd.Parameters.Add("@P074", SqlDbType.NVarChar, 1)         '表示フラグ19
                Dim PARA075 As SqlParameter = SQLcmd.Parameters.Add("@P075", SqlDbType.Int)                 '表示順19
                Dim PARA076 As SqlParameter = SQLcmd.Parameters.Add("@P076", SqlDbType.NVarChar, 4)         '情報出力ID20
                Dim PARA077 As SqlParameter = SQLcmd.Parameters.Add("@P077", SqlDbType.NVarChar, 1)         '表示フラグ20
                Dim PARA078 As SqlParameter = SQLcmd.Parameters.Add("@P078", SqlDbType.Int)                 '表示順20
                Dim PARA079 As SqlParameter = SQLcmd.Parameters.Add("@P079", SqlDbType.NVarChar, 4)         '情報出力ID21
                Dim PARA080 As SqlParameter = SQLcmd.Parameters.Add("@P080", SqlDbType.NVarChar, 1)         '表示フラグ21
                Dim PARA081 As SqlParameter = SQLcmd.Parameters.Add("@P081", SqlDbType.Int)                 '表示順21
                Dim PARA082 As SqlParameter = SQLcmd.Parameters.Add("@P082", SqlDbType.NVarChar, 4)         '情報出力ID22
                Dim PARA083 As SqlParameter = SQLcmd.Parameters.Add("@P083", SqlDbType.NVarChar, 1)         '表示フラグ22
                Dim PARA084 As SqlParameter = SQLcmd.Parameters.Add("@P084", SqlDbType.Int)                 '表示順22
                Dim PARA085 As SqlParameter = SQLcmd.Parameters.Add("@P085", SqlDbType.NVarChar, 4)         '情報出力ID23
                Dim PARA086 As SqlParameter = SQLcmd.Parameters.Add("@P086", SqlDbType.NVarChar, 1)         '表示フラグ23
                Dim PARA087 As SqlParameter = SQLcmd.Parameters.Add("@P087", SqlDbType.Int)                 '表示順23
                Dim PARA088 As SqlParameter = SQLcmd.Parameters.Add("@P088", SqlDbType.NVarChar, 4)         '情報出力ID24
                Dim PARA089 As SqlParameter = SQLcmd.Parameters.Add("@P089", SqlDbType.NVarChar, 1)         '表示フラグ24
                Dim PARA090 As SqlParameter = SQLcmd.Parameters.Add("@P090", SqlDbType.Int)                 '表示順24
                Dim PARA091 As SqlParameter = SQLcmd.Parameters.Add("@P091", SqlDbType.NVarChar, 4)         '情報出力ID25
                Dim PARA092 As SqlParameter = SQLcmd.Parameters.Add("@P092", SqlDbType.NVarChar, 1)         '表示フラグ25
                Dim PARA093 As SqlParameter = SQLcmd.Parameters.Add("@P093", SqlDbType.Int)                 '表示順25
                Dim PARA094 As SqlParameter = SQLcmd.Parameters.Add("@P094", SqlDbType.DateTime)            '登録年月日
                Dim PARA095 As SqlParameter = SQLcmd.Parameters.Add("@P095", SqlDbType.NVarChar, 20)        '登録ユーザーＩＤ
                Dim PARA096 As SqlParameter = SQLcmd.Parameters.Add("@P096", SqlDbType.NVarChar, 20)        '登録端末
                Dim PARA097 As SqlParameter = SQLcmd.Parameters.Add("@P097", SqlDbType.DateTime)            '更新年月日
                Dim PARA098 As SqlParameter = SQLcmd.Parameters.Add("@P098", SqlDbType.NVarChar, 20)        '更新ユーザーＩＤ
                Dim PARA099 As SqlParameter = SQLcmd.Parameters.Add("@P099", SqlDbType.NVarChar, 20)        '更新端末
                Dim PARA100 As SqlParameter = SQLcmd.Parameters.Add("@P100", SqlDbType.DateTime)            '集信日時

                Dim JPARA000 As SqlParameter = SQLcmdJnl.Parameters.Add("@P000", SqlDbType.NVarChar, 1)     '削除フラグ
                Dim JPARA001 As SqlParameter = SQLcmdJnl.Parameters.Add("@P001", SqlDbType.NVarChar, 20)    'ユーザID
                Dim JPARA002 As SqlParameter = SQLcmdJnl.Parameters.Add("@P002", SqlDbType.NVarChar, 20)    '社員名（短）
                Dim JPARA003 As SqlParameter = SQLcmdJnl.Parameters.Add("@P003", SqlDbType.NVarChar, 50)    '社員名（長）
                Dim JPARA004 As SqlParameter = SQLcmdJnl.Parameters.Add("@P004", SqlDbType.NVarChar, 20)    '画面ＩＤ
                'Dim JPARA05 As SqlParameter = SQLcmdJnl.Parameters.Add("@P005", SqlDbType.NVarChar, 200)    'パスワード
                'Dim JPARA06 As SqlParameter = SQLcmdJnl.Parameters.Add("@P006", SqlDbType.Int)              '誤り回数
                'Dim JPARA07 As SqlParameter = SQLcmdJnl.Parameters.Add("@P007", SqlDbType.Date)             'パスワード有効期限
                Dim JPARA008 As SqlParameter = SQLcmdJnl.Parameters.Add("@P008", SqlDbType.Date)            '開始年月日
                Dim JPARA009 As SqlParameter = SQLcmdJnl.Parameters.Add("@P009", SqlDbType.Date)            '終了年月日
                Dim JPARA010 As SqlParameter = SQLcmdJnl.Parameters.Add("@P010", SqlDbType.NVarChar, 2)     '会社コード
                Dim JPARA011 As SqlParameter = SQLcmdJnl.Parameters.Add("@P011", SqlDbType.NVarChar, 6)     '組織コード
                Dim JPARA012 As SqlParameter = SQLcmdJnl.Parameters.Add("@P012", SqlDbType.NVarChar, 128)   'メールアドレス
                Dim JPARA013 As SqlParameter = SQLcmdJnl.Parameters.Add("@P013", SqlDbType.NVarChar, 20)    'メニュー表示制御ロール
                Dim JPARA014 As SqlParameter = SQLcmdJnl.Parameters.Add("@P014", SqlDbType.NVarChar, 20)    '画面参照更新制御ロール
                Dim JPARA015 As SqlParameter = SQLcmdJnl.Parameters.Add("@P015", SqlDbType.NVarChar, 20)    '画面表示項目制御ロール
                Dim JPARA016 As SqlParameter = SQLcmdJnl.Parameters.Add("@P016", SqlDbType.NVarChar, 20)    'エクセル出力制御ロール
                Dim JPARA017 As SqlParameter = SQLcmdJnl.Parameters.Add("@P017", SqlDbType.NVarChar, 20)    '画面初期値ロール
                Dim JPARA018 As SqlParameter = SQLcmdJnl.Parameters.Add("@P018", SqlDbType.NVarChar, 20)    '承認権限ロール
                Dim JPARA019 As SqlParameter = SQLcmdJnl.Parameters.Add("@P019", SqlDbType.NVarChar, 4)     '情報出力ID1
                Dim JPARA020 As SqlParameter = SQLcmdJnl.Parameters.Add("@P020", SqlDbType.NVarChar, 1)     '表示フラグ1
                Dim JPARA021 As SqlParameter = SQLcmdJnl.Parameters.Add("@P021", SqlDbType.Int)             '表示順1
                Dim JPARA022 As SqlParameter = SQLcmdJnl.Parameters.Add("@P022", SqlDbType.NVarChar, 4)     '情報出力ID2
                Dim JPARA023 As SqlParameter = SQLcmdJnl.Parameters.Add("@P023", SqlDbType.NVarChar, 1)     '表示フラグ2
                Dim JPARA024 As SqlParameter = SQLcmdJnl.Parameters.Add("@P024", SqlDbType.Int)             '表示順2
                Dim JPARA025 As SqlParameter = SQLcmdJnl.Parameters.Add("@P025", SqlDbType.NVarChar, 4)     '情報出力ID3
                Dim JPARA026 As SqlParameter = SQLcmdJnl.Parameters.Add("@P026", SqlDbType.NVarChar, 1)     '表示フラグ3
                Dim JPARA027 As SqlParameter = SQLcmdJnl.Parameters.Add("@P027", SqlDbType.Int)             '表示順3
                Dim JPARA028 As SqlParameter = SQLcmdJnl.Parameters.Add("@P028", SqlDbType.NVarChar, 4)     '情報出力ID4
                Dim JPARA029 As SqlParameter = SQLcmdJnl.Parameters.Add("@P029", SqlDbType.NVarChar, 1)     '表示フラグ4
                Dim JPARA030 As SqlParameter = SQLcmdJnl.Parameters.Add("@P030", SqlDbType.Int)             '表示順4
                Dim JPARA031 As SqlParameter = SQLcmdJnl.Parameters.Add("@P031", SqlDbType.NVarChar, 4)     '情報出力ID5
                Dim JPARA032 As SqlParameter = SQLcmdJnl.Parameters.Add("@P032", SqlDbType.NVarChar, 1)     '表示フラグ5
                Dim JPARA033 As SqlParameter = SQLcmdJnl.Parameters.Add("@P033", SqlDbType.Int)             '表示順5
                Dim JPARA034 As SqlParameter = SQLcmdJnl.Parameters.Add("@P034", SqlDbType.NVarChar, 4)     '情報出力ID6
                Dim JPARA035 As SqlParameter = SQLcmdJnl.Parameters.Add("@P035", SqlDbType.NVarChar, 1)     '表示フラグ6
                Dim JPARA036 As SqlParameter = SQLcmdJnl.Parameters.Add("@P036", SqlDbType.Int)             '表示順6
                Dim JPARA037 As SqlParameter = SQLcmdJnl.Parameters.Add("@P037", SqlDbType.NVarChar, 4)     '情報出力ID7
                Dim JPARA038 As SqlParameter = SQLcmdJnl.Parameters.Add("@P038", SqlDbType.NVarChar, 1)     '表示フラグ7
                Dim JPARA039 As SqlParameter = SQLcmdJnl.Parameters.Add("@P039", SqlDbType.Int)             '表示順7
                Dim JPARA040 As SqlParameter = SQLcmdJnl.Parameters.Add("@P040", SqlDbType.NVarChar, 4)     '情報出力ID8
                Dim JPARA041 As SqlParameter = SQLcmdJnl.Parameters.Add("@P041", SqlDbType.NVarChar, 1)     '表示フラグ8
                Dim JPARA042 As SqlParameter = SQLcmdJnl.Parameters.Add("@P042", SqlDbType.Int)             '表示順8
                Dim JPARA043 As SqlParameter = SQLcmdJnl.Parameters.Add("@P043", SqlDbType.NVarChar, 4)     '情報出力ID9
                Dim JPARA044 As SqlParameter = SQLcmdJnl.Parameters.Add("@P044", SqlDbType.NVarChar, 1)     '表示フラグ9
                Dim JPARA045 As SqlParameter = SQLcmdJnl.Parameters.Add("@P045", SqlDbType.Int)             '表示順9
                Dim JPARA046 As SqlParameter = SQLcmdJnl.Parameters.Add("@P046", SqlDbType.NVarChar, 4)     '情報出力ID10
                Dim JPARA047 As SqlParameter = SQLcmdJnl.Parameters.Add("@P047", SqlDbType.NVarChar, 1)     '表示フラグ10
                Dim JPARA048 As SqlParameter = SQLcmdJnl.Parameters.Add("@P048", SqlDbType.Int)             '表示順10
                Dim JPARA049 As SqlParameter = SQLcmdJnl.Parameters.Add("@P049", SqlDbType.NVarChar, 4)     '情報出力ID11
                Dim JPARA050 As SqlParameter = SQLcmdJnl.Parameters.Add("@P050", SqlDbType.NVarChar, 1)     '表示フラグ11
                Dim JPARA051 As SqlParameter = SQLcmdJnl.Parameters.Add("@P051", SqlDbType.Int)             '表示順11
                Dim JPARA052 As SqlParameter = SQLcmdJnl.Parameters.Add("@P052", SqlDbType.NVarChar, 4)     '情報出力ID12
                Dim JPARA053 As SqlParameter = SQLcmdJnl.Parameters.Add("@P053", SqlDbType.NVarChar, 1)     '表示フラグ12
                Dim JPARA054 As SqlParameter = SQLcmdJnl.Parameters.Add("@P054", SqlDbType.Int)             '表示順12
                Dim JPARA055 As SqlParameter = SQLcmdJnl.Parameters.Add("@P055", SqlDbType.NVarChar, 4)     '情報出力ID13
                Dim JPARA056 As SqlParameter = SQLcmdJnl.Parameters.Add("@P056", SqlDbType.NVarChar, 1)     '表示フラグ13
                Dim JPARA057 As SqlParameter = SQLcmdJnl.Parameters.Add("@P057", SqlDbType.Int)             '表示順13
                Dim JPARA058 As SqlParameter = SQLcmdJnl.Parameters.Add("@P058", SqlDbType.NVarChar, 4)     '情報出力ID14
                Dim JPARA059 As SqlParameter = SQLcmdJnl.Parameters.Add("@P059", SqlDbType.NVarChar, 1)     '表示フラグ14
                Dim JPARA060 As SqlParameter = SQLcmdJnl.Parameters.Add("@P060", SqlDbType.Int)             '表示順14
                Dim JPARA061 As SqlParameter = SQLcmdJnl.Parameters.Add("@P061", SqlDbType.NVarChar, 4)     '情報出力ID15
                Dim JPARA062 As SqlParameter = SQLcmdJnl.Parameters.Add("@P062", SqlDbType.NVarChar, 1)     '表示フラグ15
                Dim JPARA063 As SqlParameter = SQLcmdJnl.Parameters.Add("@P063", SqlDbType.Int)             '表示順15
                Dim JPARA064 As SqlParameter = SQLcmdJnl.Parameters.Add("@P064", SqlDbType.NVarChar, 4)     '情報出力ID16
                Dim JPARA065 As SqlParameter = SQLcmdJnl.Parameters.Add("@P065", SqlDbType.NVarChar, 1)     '表示フラグ16
                Dim JPARA066 As SqlParameter = SQLcmdJnl.Parameters.Add("@P066", SqlDbType.Int)             '表示順16
                Dim JPARA067 As SqlParameter = SQLcmdJnl.Parameters.Add("@P067", SqlDbType.NVarChar, 4)     '情報出力ID17
                Dim JPARA068 As SqlParameter = SQLcmdJnl.Parameters.Add("@P068", SqlDbType.NVarChar, 1)     '表示フラグ17
                Dim JPARA069 As SqlParameter = SQLcmdJnl.Parameters.Add("@P069", SqlDbType.Int)             '表示順17
                Dim JPARA070 As SqlParameter = SQLcmdJnl.Parameters.Add("@P070", SqlDbType.NVarChar, 4)     '情報出力ID18
                Dim JPARA071 As SqlParameter = SQLcmdJnl.Parameters.Add("@P071", SqlDbType.NVarChar, 1)     '表示フラグ18
                Dim JPARA072 As SqlParameter = SQLcmdJnl.Parameters.Add("@P072", SqlDbType.Int)             '表示順18
                Dim JPARA073 As SqlParameter = SQLcmdJnl.Parameters.Add("@P073", SqlDbType.NVarChar, 4)     '情報出力ID19
                Dim JPARA074 As SqlParameter = SQLcmdJnl.Parameters.Add("@P074", SqlDbType.NVarChar, 1)     '表示フラグ19
                Dim JPARA075 As SqlParameter = SQLcmdJnl.Parameters.Add("@P075", SqlDbType.Int)             '表示順19
                Dim JPARA076 As SqlParameter = SQLcmdJnl.Parameters.Add("@P076", SqlDbType.NVarChar, 4)     '情報出力ID20
                Dim JPARA077 As SqlParameter = SQLcmdJnl.Parameters.Add("@P077", SqlDbType.NVarChar, 1)     '表示フラグ20
                Dim JPARA078 As SqlParameter = SQLcmdJnl.Parameters.Add("@P078", SqlDbType.Int)             '表示順20
                Dim JPARA079 As SqlParameter = SQLcmdJnl.Parameters.Add("@P079", SqlDbType.NVarChar, 4)     '情報出力ID21
                Dim JPARA080 As SqlParameter = SQLcmdJnl.Parameters.Add("@P080", SqlDbType.NVarChar, 1)     '表示フラグ21
                Dim JPARA081 As SqlParameter = SQLcmdJnl.Parameters.Add("@P081", SqlDbType.Int)             '表示順21
                Dim JPARA082 As SqlParameter = SQLcmdJnl.Parameters.Add("@P082", SqlDbType.NVarChar, 4)     '情報出力ID22
                Dim JPARA083 As SqlParameter = SQLcmdJnl.Parameters.Add("@P083", SqlDbType.NVarChar, 1)     '表示フラグ22
                Dim JPARA084 As SqlParameter = SQLcmdJnl.Parameters.Add("@P084", SqlDbType.Int)             '表示順22
                Dim JPARA085 As SqlParameter = SQLcmdJnl.Parameters.Add("@P085", SqlDbType.NVarChar, 4)     '情報出力ID23
                Dim JPARA086 As SqlParameter = SQLcmdJnl.Parameters.Add("@P086", SqlDbType.NVarChar, 1)     '表示フラグ23
                Dim JPARA087 As SqlParameter = SQLcmdJnl.Parameters.Add("@P087", SqlDbType.Int)             '表示順23
                Dim JPARA088 As SqlParameter = SQLcmdJnl.Parameters.Add("@P088", SqlDbType.NVarChar, 4)     '情報出力ID24
                Dim JPARA089 As SqlParameter = SQLcmdJnl.Parameters.Add("@P089", SqlDbType.NVarChar, 1)     '表示フラグ24
                Dim JPARA090 As SqlParameter = SQLcmdJnl.Parameters.Add("@P090", SqlDbType.Int)             '表示順24
                Dim JPARA091 As SqlParameter = SQLcmdJnl.Parameters.Add("@P091", SqlDbType.NVarChar, 4)     '情報出力ID25
                Dim JPARA092 As SqlParameter = SQLcmdJnl.Parameters.Add("@P092", SqlDbType.NVarChar, 1)     '表示フラグ25
                Dim JPARA093 As SqlParameter = SQLcmdJnl.Parameters.Add("@P093", SqlDbType.Int)             '表示順25
                Dim JPARA094 As SqlParameter = SQLcmdJnl.Parameters.Add("@P094", SqlDbType.DateTime)        '登録年月日
                Dim JPARA095 As SqlParameter = SQLcmdJnl.Parameters.Add("@P095", SqlDbType.NVarChar, 20)    '登録ユーザーＩＤ
                Dim JPARA096 As SqlParameter = SQLcmdJnl.Parameters.Add("@P096", SqlDbType.NVarChar, 20)    '登録端末
                Dim JPARA097 As SqlParameter = SQLcmdJnl.Parameters.Add("@P097", SqlDbType.DateTime)        '更新年月日
                Dim JPARA098 As SqlParameter = SQLcmdJnl.Parameters.Add("@P098", SqlDbType.NVarChar, 20)    '更新ユーザーＩＤ
                Dim JPARA099 As SqlParameter = SQLcmdJnl.Parameters.Add("@P099", SqlDbType.NVarChar, 20)    '更新端末
                Dim JPARA100 As SqlParameter = SQLcmdJnl.Parameters.Add("@P100", SqlDbType.DateTime)        '集信日時

                Dim OIS0001row As DataRow = OIS0001INPtbl.Rows(0)

                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA000.Value = OIS0001row("DELFLG")
                PARA001.Value = OIS0001row("USERID")
                PARA002.Value = OIS0001row("STAFFNAMES")
                PARA003.Value = OIS0001row("STAFFNAMEL")
                PARA004.Value = OIS0001row("MAPID")
                'PARA05.Value = OIS0001row("PASSWORD")
                'If OIS0001row("MISSCNT") <> "" Then
                '    PARA06.Value = OIS0001row("MISSCNT")
                'Else
                '    PARA06.Value = "0"
                'End If
                'If RTrim(OIS0001row("PASSENDYMD")) <> "" Then
                '    PARA07.Value = RTrim(OIS0001row("PASSENDYMD"))
                'Else
                '    PARA07.Value = C_DEFAULT_YMD
                'End If
                If RTrim(OIS0001row("STYMD")) <> "" Then
                    PARA008.Value = RTrim(OIS0001row("STYMD"))
                Else
                    PARA008.Value = C_DEFAULT_YMD
                End If
                If RTrim(OIS0001row("ENDYMD")) <> "" Then
                    PARA009.Value = RTrim(OIS0001row("ENDYMD"))
                Else
                    PARA009.Value = C_DEFAULT_YMD
                End If
                PARA010.Value = OIS0001row("CAMPCODE")
                PARA011.Value = OIS0001row("ORG")
                PARA012.Value = OIS0001row("EMAIL")
                PARA013.Value = OIS0001row("MENUROLE")
                PARA014.Value = OIS0001row("MAPROLE")
                PARA015.Value = OIS0001row("VIEWPROFID")
                PARA016.Value = OIS0001row("RPRTPROFID")
                PARA017.Value = OIS0001row("VARIANT")
                PARA018.Value = OIS0001row("APPROVALID")
                PARA019.Value = OIS0001row("OUTPUTID1")
                PARA020.Value = OIS0001row("ONOFF1")
                PARA021.Value = OIS0001row("SORTNO1")
                PARA022.Value = OIS0001row("OUTPUTID2")
                PARA023.Value = OIS0001row("ONOFF2")
                PARA024.Value = OIS0001row("SORTNO2")
                PARA025.Value = OIS0001row("OUTPUTID3")
                PARA026.Value = OIS0001row("ONOFF3")
                PARA027.Value = OIS0001row("SORTNO3")
                PARA028.Value = OIS0001row("OUTPUTID4")
                PARA029.Value = OIS0001row("ONOFF4")
                PARA030.Value = OIS0001row("SORTNO4")
                PARA031.Value = OIS0001row("OUTPUTID5")
                PARA032.Value = OIS0001row("ONOFF5")
                PARA033.Value = OIS0001row("SORTNO5")
                PARA034.Value = OIS0001row("OUTPUTID6")
                PARA035.Value = OIS0001row("ONOFF6")
                PARA036.Value = OIS0001row("SORTNO6")
                PARA037.Value = OIS0001row("OUTPUTID7")
                PARA038.Value = OIS0001row("ONOFF7")
                PARA039.Value = OIS0001row("SORTNO7")
                PARA040.Value = OIS0001row("OUTPUTID8")
                PARA041.Value = OIS0001row("ONOFF8")
                PARA042.Value = OIS0001row("SORTNO8")
                PARA043.Value = OIS0001row("OUTPUTID9")
                PARA044.Value = OIS0001row("ONOFF9")
                PARA045.Value = OIS0001row("SORTNO9")
                PARA046.Value = OIS0001row("OUTPUTID10")
                PARA047.Value = OIS0001row("ONOFF10")
                PARA048.Value = OIS0001row("SORTNO10")
                PARA049.Value = OIS0001row("OUTPUTID11")
                PARA050.Value = OIS0001row("ONOFF11")
                PARA051.Value = OIS0001row("SORTNO11")
                PARA052.Value = OIS0001row("OUTPUTID12")
                PARA053.Value = OIS0001row("ONOFF12")
                PARA054.Value = OIS0001row("SORTNO12")
                PARA055.Value = OIS0001row("OUTPUTID13")
                PARA056.Value = OIS0001row("ONOFF13")
                PARA057.Value = OIS0001row("SORTNO13")
                PARA058.Value = OIS0001row("OUTPUTID14")
                PARA059.Value = OIS0001row("ONOFF14")
                PARA060.Value = OIS0001row("SORTNO14")
                PARA061.Value = OIS0001row("OUTPUTID15")
                PARA062.Value = OIS0001row("ONOFF15")
                PARA063.Value = OIS0001row("SORTNO15")
                PARA064.Value = OIS0001row("OUTPUTID16")
                PARA065.Value = OIS0001row("ONOFF16")
                PARA066.Value = OIS0001row("SORTNO16")
                PARA067.Value = OIS0001row("OUTPUTID17")
                PARA068.Value = OIS0001row("ONOFF17")
                PARA069.Value = OIS0001row("SORTNO17")
                PARA070.Value = OIS0001row("OUTPUTID18")
                PARA071.Value = OIS0001row("ONOFF18")
                PARA072.Value = OIS0001row("SORTNO18")
                PARA073.Value = OIS0001row("OUTPUTID19")
                PARA074.Value = OIS0001row("ONOFF19")
                PARA075.Value = OIS0001row("SORTNO19")
                PARA076.Value = OIS0001row("OUTPUTID20")
                PARA077.Value = OIS0001row("ONOFF20")
                PARA078.Value = OIS0001row("SORTNO20")
                PARA079.Value = OIS0001row("OUTPUTID21")
                PARA080.Value = OIS0001row("ONOFF21")
                PARA081.Value = OIS0001row("SORTNO21")
                PARA082.Value = OIS0001row("OUTPUTID22")
                PARA083.Value = OIS0001row("ONOFF22")
                PARA084.Value = OIS0001row("SORTNO22")
                PARA085.Value = OIS0001row("OUTPUTID23")
                PARA086.Value = OIS0001row("ONOFF23")
                PARA087.Value = OIS0001row("SORTNO23")
                PARA088.Value = OIS0001row("OUTPUTID24")
                PARA089.Value = OIS0001row("ONOFF24")
                PARA090.Value = OIS0001row("SORTNO24")
                PARA091.Value = OIS0001row("OUTPUTID25")
                PARA092.Value = OIS0001row("ONOFF25")
                PARA093.Value = OIS0001row("SORTNO25")
                PARA094.Value = WW_DATENOW
                PARA095.Value = Master.USERID
                PARA096.Value = Master.USERTERMID
                PARA097.Value = WW_DATENOW
                PARA098.Value = Master.USERID
                PARA099.Value = Master.USERTERMID
                PARA100.Value = C_DEFAULT_YMD
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                'OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA000.Value = OIS0001row("DELFLG")
                JPARA001.Value = OIS0001row("USERID")
                JPARA002.Value = OIS0001row("STAFFNAMES")
                JPARA003.Value = OIS0001row("STAFFNAMEL")
                JPARA004.Value = OIS0001row("MAPID")
                'JPARA005.Value = OIS0001row("PASSWORD")
                'If OIS0001row("MISSCNT") <> "" Then
                '    JPARA006.Value = OIS0001row("MISSCNT")
                'Else
                '    JPARA006.Value = "0"
                'End If
                'If RTrim(OIS0001row("PASSENDYMD")) <> "" Then
                '    JPARA070.Value = RTrim(OIS0001row("PASSENDYMD"))
                'Else
                '    JPARA007.Value = C_DEFAULT_YMD
                'End If
                If RTrim(OIS0001row("STYMD")) <> "" Then
                    JPARA008.Value = RTrim(OIS0001row("STYMD"))
                Else
                    JPARA008.Value = C_DEFAULT_YMD
                End If
                If RTrim(OIS0001row("ENDYMD")) <> "" Then
                    JPARA009.Value = RTrim(OIS0001row("ENDYMD"))
                Else
                    JPARA009.Value = C_DEFAULT_YMD
                End If
                JPARA010.Value = OIS0001row("CAMPCODE")
                JPARA011.Value = OIS0001row("ORG")
                JPARA012.Value = OIS0001row("EMAIL")
                JPARA013.Value = OIS0001row("MENUROLE")
                JPARA014.Value = OIS0001row("MAPROLE")
                JPARA015.Value = OIS0001row("VIEWPROFID")
                JPARA016.Value = OIS0001row("RPRTPROFID")
                JPARA017.Value = OIS0001row("VARIANT")
                JPARA018.Value = OIS0001row("APPROVALID")
                JPARA019.Value = OIS0001row("OUTPUTID1")
                JPARA020.Value = OIS0001row("ONOFF1")
                JPARA021.Value = OIS0001row("SORTNO1")
                JPARA022.Value = OIS0001row("OUTPUTID2")
                JPARA023.Value = OIS0001row("ONOFF2")
                JPARA024.Value = OIS0001row("SORTNO2")
                JPARA025.Value = OIS0001row("OUTPUTID3")
                JPARA026.Value = OIS0001row("ONOFF3")
                JPARA027.Value = OIS0001row("SORTNO3")
                JPARA028.Value = OIS0001row("OUTPUTID4")
                JPARA029.Value = OIS0001row("ONOFF4")
                JPARA030.Value = OIS0001row("SORTNO4")
                JPARA031.Value = OIS0001row("OUTPUTID5")
                JPARA032.Value = OIS0001row("ONOFF5")
                JPARA033.Value = OIS0001row("SORTNO5")
                JPARA034.Value = OIS0001row("OUTPUTID6")
                JPARA035.Value = OIS0001row("ONOFF6")
                JPARA036.Value = OIS0001row("SORTNO6")
                JPARA037.Value = OIS0001row("OUTPUTID7")
                JPARA038.Value = OIS0001row("ONOFF7")
                JPARA039.Value = OIS0001row("SORTNO7")
                JPARA040.Value = OIS0001row("OUTPUTID8")
                JPARA041.Value = OIS0001row("ONOFF8")
                JPARA042.Value = OIS0001row("SORTNO8")
                JPARA043.Value = OIS0001row("OUTPUTID9")
                JPARA044.Value = OIS0001row("ONOFF9")
                JPARA045.Value = OIS0001row("SORTNO9")
                JPARA046.Value = OIS0001row("OUTPUTID10")
                JPARA047.Value = OIS0001row("ONOFF10")
                JPARA048.Value = OIS0001row("SORTNO10")
                JPARA049.Value = OIS0001row("OUTPUTID11")
                JPARA050.Value = OIS0001row("ONOFF11")
                JPARA051.Value = OIS0001row("SORTNO11")
                JPARA052.Value = OIS0001row("OUTPUTID12")
                JPARA053.Value = OIS0001row("ONOFF12")
                JPARA054.Value = OIS0001row("SORTNO12")
                JPARA055.Value = OIS0001row("OUTPUTID13")
                JPARA056.Value = OIS0001row("ONOFF13")
                JPARA057.Value = OIS0001row("SORTNO13")
                JPARA058.Value = OIS0001row("OUTPUTID14")
                JPARA059.Value = OIS0001row("ONOFF14")
                JPARA060.Value = OIS0001row("SORTNO14")
                JPARA061.Value = OIS0001row("OUTPUTID15")
                JPARA062.Value = OIS0001row("ONOFF15")
                JPARA063.Value = OIS0001row("SORTNO15")
                JPARA064.Value = OIS0001row("OUTPUTID16")
                JPARA065.Value = OIS0001row("ONOFF16")
                JPARA066.Value = OIS0001row("SORTNO16")
                JPARA067.Value = OIS0001row("OUTPUTID17")
                JPARA068.Value = OIS0001row("ONOFF17")
                JPARA069.Value = OIS0001row("SORTNO17")
                JPARA070.Value = OIS0001row("OUTPUTID18")
                JPARA071.Value = OIS0001row("ONOFF18")
                JPARA072.Value = OIS0001row("SORTNO18")
                JPARA073.Value = OIS0001row("OUTPUTID19")
                JPARA074.Value = OIS0001row("ONOFF19")
                JPARA075.Value = OIS0001row("SORTNO19")
                JPARA076.Value = OIS0001row("OUTPUTID20")
                JPARA077.Value = OIS0001row("ONOFF20")
                JPARA078.Value = OIS0001row("SORTNO20")
                JPARA079.Value = OIS0001row("OUTPUTID21")
                JPARA080.Value = OIS0001row("ONOFF21")
                JPARA081.Value = OIS0001row("SORTNO21")
                JPARA082.Value = OIS0001row("OUTPUTID22")
                JPARA083.Value = OIS0001row("ONOFF22")
                JPARA084.Value = OIS0001row("SORTNO22")
                JPARA085.Value = OIS0001row("OUTPUTID23")
                JPARA086.Value = OIS0001row("ONOFF23")
                JPARA087.Value = OIS0001row("SORTNO23")
                JPARA088.Value = OIS0001row("OUTPUTID24")
                JPARA089.Value = OIS0001row("ONOFF24")
                JPARA090.Value = OIS0001row("SORTNO24")
                JPARA091.Value = OIS0001row("OUTPUTID25")
                JPARA092.Value = OIS0001row("ONOFF25")
                JPARA093.Value = OIS0001row("SORTNO25")
                JPARA094.Value = WW_DATENOW
                JPARA095.Value = Master.USERID
                JPARA096.Value = Master.USERTERMID
                JPARA097.Value = WW_DATENOW
                JPARA098.Value = Master.USERID
                JPARA099.Value = Master.USERTERMID
                JPARA100.Value = C_DEFAULT_YMD

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIS0001UPDtbl) Then
                        OIS0001UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIS0001UPDtbl.Clear()
                    OIS0001UPDtbl.Load(SQLdr)
                End Using

                For Each OIS0001UPDrow As DataRow In OIS0001UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIS0001C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIS0001UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS0001C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS0001C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ ＤＢ更新(ユーザパスワードマスタ)
        SQLStr =
            "OPEN SYMMETRIC KEY loginpasskey  DECRYPTION BY CERTIFICATE certjotoil;" _
            & " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        COM.OIS0005_USERPASS" _
            & "    WHERE" _
            & "        USERID       = @P001 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE COM.OIS0005_USERPASS" _
            & "    SET" _
            & "        DELFLG = @P000" _
            & "        , PASSWORD = EncryptByKey(Key_GUID('loginpasskey')  , @P005)" _
            & "        , MISSCNT = @P006" _
            & "        , PASSENDYMD = @P007" _
            & "        , UPDYMD = @P097" _
            & "        , UPDUSER = @P098" _
            & "        , UPDTERMID = @P099" _
            & "        , RECEIVEYMD = @P100" _
            & "    WHERE" _
            & "        USERID       = @P001" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO COM.OIS0005_USERPASS" _
            & "        (DELFLG" _
            & "        , USERID" _
            & "        , PASSWORD" _
            & "        , MISSCNT" _
            & "        , PASSENDYMD" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD)" _
            & "    VALUES" _
            & "        (@P000" _
            & "        , @P001" _
            & "        , EncryptByKey(Key_GUID('loginpasskey')  , @P005)" _
            & "        , @P006" _
            & "        , @P007" _
            & "        , @P094" _
            & "        , @P095" _
            & "        , @P096" _
            & "        , @P097" _
            & "        , @P098" _
            & "        , @P099" _
            & "        , @P100) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        SQLJnl =
              " Select" _
            & "    DELFLG" _
            & "        , USERID" _
            & "        , PASSWORD" _
            & "        , MISSCNT" _
            & "        , PASSENDYMD" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD" _
            & "    , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "    COM.OIS0005_USERPASS" _
            & " WHERE" _
            & "        USERID       = @P001"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA000 As SqlParameter = SQLcmd.Parameters.Add("@P000", SqlDbType.NVarChar, 1)         '削除フラグ
                Dim PARA001 As SqlParameter = SQLcmd.Parameters.Add("@P001", SqlDbType.NVarChar, 20)        'ユーザID
                'Dim PARA002 As SqlParameter = SQLcmd.Parameters.Add("@P002", SqlDbType.NVarChar, 20)        '社員名（短）
                'Dim PARA003 As SqlParameter = SQLcmd.Parameters.Add("@P003", SqlDbType.NVarChar, 50)        '社員名（長）
                'Dim PARA004 As SqlParameter = SQLcmd.Parameters.Add("@P004", SqlDbType.NVarChar, 20)        '画面ＩＤ
                Dim PARA005 As SqlParameter = SQLcmd.Parameters.Add("@P005", SqlDbType.NVarChar, 200)       'パスワード
                Dim PARA006 As SqlParameter = SQLcmd.Parameters.Add("@P006", SqlDbType.Int)                 '誤り回数
                Dim PARA007 As SqlParameter = SQLcmd.Parameters.Add("@P007", SqlDbType.Date)                'パスワード有効期限
                'Dim PARA008 As SqlParameter = SQLcmd.Parameters.Add("@P008", SqlDbType.Date)                '開始年月日
                'Dim PARA009 As SqlParameter = SQLcmd.Parameters.Add("@P009", SqlDbType.Date)                '終了年月日
                'Dim PARA010 As SqlParameter = SQLcmd.Parameters.Add("@P010", SqlDbType.NVarChar, 2)         '会社コード
                'Dim PARA011 As SqlParameter = SQLcmd.Parameters.Add("@P011", SqlDbType.NVarChar, 6)         '組織コード
                'Dim PARA012 As SqlParameter = SQLcmd.Parameters.Add("@P012", SqlDbType.NVarChar, 128)       'メールアドレス
                'Dim PARA013 As SqlParameter = SQLcmd.Parameters.Add("@P013", SqlDbType.NVarChar, 20)        'メニュー表示制御ロール
                'Dim PARA014 As SqlParameter = SQLcmd.Parameters.Add("@P014", SqlDbType.NVarChar, 20)        '画面参照更新制御ロール
                'Dim PARA015 As SqlParameter = SQLcmd.Parameters.Add("@P015", SqlDbType.NVarChar, 20)        '画面表示項目制御ロール
                'Dim PARA016 As SqlParameter = SQLcmd.Parameters.Add("@P016", SqlDbType.NVarChar, 20)        'エクセル出力制御ロール
                'Dim PARA017 As SqlParameter = SQLcmd.Parameters.Add("@P017", SqlDbType.NVarChar, 20)        '画面初期値ロール
                'Dim PARA018 As SqlParameter = SQLcmd.Parameters.Add("@P018", SqlDbType.NVarChar, 20)        '承認権限ロール
                Dim PARA094 As SqlParameter = SQLcmd.Parameters.Add("@P094", SqlDbType.DateTime)            '登録年月日
                Dim PARA095 As SqlParameter = SQLcmd.Parameters.Add("@P095", SqlDbType.NVarChar, 20)        '登録ユーザーＩＤ
                Dim PARA096 As SqlParameter = SQLcmd.Parameters.Add("@P096", SqlDbType.NVarChar, 20)        '登録端末
                Dim PARA097 As SqlParameter = SQLcmd.Parameters.Add("@P097", SqlDbType.DateTime)            '更新年月日
                Dim PARA098 As SqlParameter = SQLcmd.Parameters.Add("@P098", SqlDbType.NVarChar, 20)        '更新ユーザーＩＤ
                Dim PARA099 As SqlParameter = SQLcmd.Parameters.Add("@P099", SqlDbType.NVarChar, 20)        '更新端末
                Dim PARA100 As SqlParameter = SQLcmd.Parameters.Add("@P100", SqlDbType.DateTime)            '集信日時

                Dim JPARA000 As SqlParameter = SQLcmdJnl.Parameters.Add("@P000", SqlDbType.NVarChar, 1)     '削除フラグ
                Dim JPARA001 As SqlParameter = SQLcmdJnl.Parameters.Add("@P001", SqlDbType.NVarChar, 20)    'ユーザID
                'Dim JPARA002 As SqlParameter = SQLcmdJnl.Parameters.Add("@P002", SqlDbType.NVarChar, 20)    '社員名（短）
                'Dim JPARA003 As SqlParameter = SQLcmdJnl.Parameters.Add("@P003", SqlDbType.NVarChar, 50)    '社員名（長）
                'Dim JPARA004 As SqlParameter = SQLcmdJnl.Parameters.Add("@P004", SqlDbType.NVarChar, 20)    '画面ＩＤ
                Dim JPARA005 As SqlParameter = SQLcmdJnl.Parameters.Add("@P005", SqlDbType.NVarChar, 200)   'パスワード
                Dim JPARA006 As SqlParameter = SQLcmdJnl.Parameters.Add("@P006", SqlDbType.Int)             '誤り回数
                Dim JPARA007 As SqlParameter = SQLcmdJnl.Parameters.Add("@P007", SqlDbType.Date)            'パスワード有効期限
                'Dim JPARA008 As SqlParameter = SQLcmdJnl.Parameters.Add("@P008", SqlDbType.Date)            '開始年月日
                'Dim JPARA009 As SqlParameter = SQLcmdJnl.Parameters.Add("@P009", SqlDbType.Date)            '終了年月日
                'Dim JPARA010 As SqlParameter = SQLcmdJnl.Parameters.Add("@P010", SqlDbType.NVarChar, 2)     '会社コード
                'Dim JPARA011 As SqlParameter = SQLcmdJnl.Parameters.Add("@P011", SqlDbType.NVarChar, 6)     '組織コード
                'Dim JPARA012 As SqlParameter = SQLcmdJnl.Parameters.Add("@P012", SqlDbType.NVarChar, 128)   'メールアドレス
                'Dim JPARA013 As SqlParameter = SQLcmdJnl.Parameters.Add("@P013", SqlDbType.NVarChar, 20)    'メニュー表示制御ロール
                'Dim JPARA014 As SqlParameter = SQLcmdJnl.Parameters.Add("@P014", SqlDbType.NVarChar, 20)    '画面参照更新制御ロール
                'Dim JPARA015 As SqlParameter = SQLcmdJnl.Parameters.Add("@P015", SqlDbType.NVarChar, 20)    '画面表示項目制御ロール
                'Dim JPARA016 As SqlParameter = SQLcmdJnl.Parameters.Add("@P016", SqlDbType.NVarChar, 20)    'エクセル出力制御ロール
                'Dim JPARA017 As SqlParameter = SQLcmdJnl.Parameters.Add("@P017", SqlDbType.NVarChar, 20)    '画面初期値ロール
                'Dim JPARA018 As SqlParameter = SQLcmdJnl.Parameters.Add("@P018", SqlDbType.NVarChar, 20)    '承認権限ロール
                Dim JPARA094 As SqlParameter = SQLcmdJnl.Parameters.Add("@P094", SqlDbType.DateTime)        '登録年月日
                Dim JPARA095 As SqlParameter = SQLcmdJnl.Parameters.Add("@P095", SqlDbType.NVarChar, 20)    '登録ユーザーＩＤ
                Dim JPARA096 As SqlParameter = SQLcmdJnl.Parameters.Add("@P096", SqlDbType.NVarChar, 20)    '登録端末
                Dim JPARA097 As SqlParameter = SQLcmdJnl.Parameters.Add("@P097", SqlDbType.DateTime)        '更新年月日
                Dim JPARA098 As SqlParameter = SQLcmdJnl.Parameters.Add("@P098", SqlDbType.NVarChar, 20)    '更新ユーザーＩＤ
                Dim JPARA099 As SqlParameter = SQLcmdJnl.Parameters.Add("@P099", SqlDbType.NVarChar, 20)    '更新端末
                Dim JPARA100 As SqlParameter = SQLcmdJnl.Parameters.Add("@P100", SqlDbType.DateTime)        '集信日時

                Dim OIS0001row As DataRow = OIS0001INPtbl.Rows(0)

                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA000.Value = OIS0001row("DELFLG")
                PARA001.Value = OIS0001row("USERID")
                'PARA002.Value = OIS0001row("STAFFNAMES")
                'PARA003.Value = OIS0001row("STAFFNAMEL")
                'PARA004.Value = OIS0001row("MAPID")
                PARA005.Value = OIS0001row("PASSWORD")
                If OIS0001row("MISSCNT") <> "" Then
                    PARA006.Value = OIS0001row("MISSCNT")
                Else
                    PARA006.Value = "0"
                End If
                If RTrim(OIS0001row("PASSENDYMD")) <> "" Then
                    PARA007.Value = RTrim(OIS0001row("PASSENDYMD"))
                Else
                    PARA007.Value = C_DEFAULT_YMD
                End If
                'If RTrim(OIS0001row("STYMD")) <> "" Then
                '    PARA008.Value = RTrim(OIS0001row("STYMD"))
                'Else
                '    PARA008.Value = C_DEFAULT_YMD
                'End If
                'If RTrim(OIS0001row("ENDYMD")) <> "" Then
                '    PARA009.Value = RTrim(OIS0001row("ENDYMD"))
                'Else
                '    PARA009.Value = C_DEFAULT_YMD
                'End If
                'PARA010.Value = OIS0001row("CAMPCODE")
                'PARA011.Value = OIS0001row("ORG")
                'PARA012.Value = OIS0001row("EMAIL")
                'PARA013.Value = OIS0001row("MENUROLE")
                'PARA014.Value = OIS0001row("MAPROLE")
                'PARA015.Value = OIS0001row("VIEWPROFID")
                'PARA016.Value = OIS0001row("RPRTPROFID")
                'PARA017.Value = OIS0001row("VARIANT")
                'PARA018.Value = OIS0001row("APPROVALID")
                PARA094.Value = WW_DATENOW
                PARA095.Value = Master.USERID
                PARA096.Value = Master.USERTERMID
                PARA097.Value = WW_DATENOW
                PARA098.Value = Master.USERID
                PARA099.Value = Master.USERTERMID
                PARA100.Value = C_DEFAULT_YMD
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA000.Value = OIS0001row("DELFLG")
                JPARA001.Value = OIS0001row("USERID")
                'JPARA002.Value = OIS0001row("STAFFNAMES")
                'JPARA003.Value = OIS0001row("STAFFNAMEL")
                'JPARA004.Value = OIS0001row("MAPID")
                JPARA005.Value = OIS0001row("PASSWORD")
                If OIS0001row("MISSCNT") <> "" Then
                    JPARA006.Value = OIS0001row("MISSCNT")
                Else
                    JPARA006.Value = "0"
                End If
                If RTrim(OIS0001row("PASSENDYMD")) <> "" Then
                    JPARA007.Value = RTrim(OIS0001row("PASSENDYMD"))
                Else
                    JPARA007.Value = C_DEFAULT_YMD
                End If
                'If RTrim(OIS0001row("STYMD")) <> "" Then
                '    JPARA008.Value = RTrim(OIS0001row("STYMD"))
                'Else
                '    JPARA008.Value = C_DEFAULT_YMD
                'End If
                'If RTrim(OIS0001row("ENDYMD")) <> "" Then
                '    JPARA009.Value = RTrim(OIS0001row("ENDYMD"))
                'Else
                '    JPARA009.Value = C_DEFAULT_YMD
                'End If
                'JPARA010.Value = OIS0001row("CAMPCODE")
                'JPARA011.Value = OIS0001row("ORG")
                'JPARA012.Value = OIS0001row("EMAIL")
                'JPARA013.Value = OIS0001row("MENUROLE")
                'JPARA014.Value = OIS0001row("MAPROLE")
                'JPARA015.Value = OIS0001row("VIEWPROFID")
                'JPARA016.Value = OIS0001row("RPRTPROFID")
                'JPARA017.Value = OIS0001row("VARIANT")
                'JPARA018.Value = OIS0001row("APPROVALID")
                JPARA094.Value = WW_DATENOW
                JPARA095.Value = Master.USERID
                JPARA096.Value = Master.USERTERMID
                JPARA097.Value = WW_DATENOW
                JPARA098.Value = Master.USERID
                JPARA099.Value = Master.USERTERMID
                JPARA100.Value = C_DEFAULT_YMD

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIS0001UPDtbl) Then
                        OIS0001UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIS0001UPDtbl.Clear()
                    OIS0001UPDtbl.Load(SQLdr)
                End Using

                For Each OIS0001UPDrow As DataRow In OIS0001UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIS0001C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIS0001UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIS000CL UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIS000CL UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        'Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)
    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIS0001INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIS0001tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "ユーザIDかつ開始日年月日", needsPopUp:=True)

            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            End If
        End If

        If isNormal(WW_ERR_SW) Then
            '前ページ遷移
            Master.TransitionPrevPage()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToOIS0001INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIS0001INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIS0001INProw As DataRow = OIS0001INPtbl.NewRow

        '○ 初期クリア
        For Each OIS0001INPcol As DataColumn In OIS0001INPtbl.Columns
            If IsDBNull(OIS0001INProw.Item(OIS0001INPcol)) OrElse IsNothing(OIS0001INProw.Item(OIS0001INPcol)) Then
                Select Case OIS0001INPcol.ColumnName
                    Case "LINECNT"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case "OPERATION"
                        OIS0001INProw.Item(OIS0001INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case "SELECT"
                        OIS0001INProw.Item(OIS0001INPcol) = 1
                    Case "HIDDEN"
                        OIS0001INProw.Item(OIS0001INPcol) = 0
                    Case Else
                        OIS0001INProw.Item(OIS0001INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIS0001INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIS0001INProw("LINECNT"))
            Catch ex As Exception
                OIS0001INProw("LINECNT") = 0
            End Try
        End If

        OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIS0001INProw("UPDTIMSTP") = 0
        OIS0001INProw("SELECT") = 1
        OIS0001INProw("HIDDEN") = 0

        OIS0001INProw("DELFLG") = WF_DELFLG.Text                '削除フラグ

        OIS0001INProw("USERID") = WF_USERID.Text                'ユーザID

        OIS0001INProw("STAFFNAMES") = WF_STAFFNAMES.Text        '社員名（短）

        OIS0001INProw("STAFFNAMEL") = WF_STAFFNAMEL.Text        '社員名（長）

        OIS0001INProw("MAPID") = WF_MAPID.Text                  '画面ＩＤ

        OIS0001INProw("PASSWORD") = WF_PASSWORD.Text            'パスワード

        OIS0001INProw("MISSCNT") = WF_MISSCNT.Text              '誤り回数

        OIS0001INProw("PASSENDYMD") = WF_PASSENDYMD.Text        'パスワード有効期限

        OIS0001INProw("STYMD") = WF_STYMD.Text                  '開始年月日

        OIS0001INProw("ENDYMD") = WF_ENDYMD.Text                '終了年月日

        OIS0001INProw("CAMPCODE") = WF_CAMPCODE.Text            '会社コード

        OIS0001INProw("ORG") = WF_ORG.Text                      '組織コード

        OIS0001INProw("EMAIL") = WF_EMAIL.Text                  'メールアドレス

        OIS0001INProw("MENUROLE") = WF_MENUROLE.Text            'メニュー表示制御ロール

        OIS0001INProw("MAPROLE") = WF_MAPROLE.Text              '画面参照更新制御ロール

        OIS0001INProw("VIEWPROFID") = WF_VIEWPROFID.Text        '画面表示項目制御ロール

        OIS0001INProw("RPRTPROFID") = WF_RPRTPROFID.Text        'エクセル出力制御ロール

        OIS0001INProw("VARIANT") = WF_VARIANT.Text              '画面初期値ロール

        OIS0001INProw("APPROVALID") = WF_APPROVALID.Text        '承認権限ロール

        OIS0001INProw("OUTPUTID1") = WF_OUTPUTID1.Text          '情報出力ID1

        OIS0001INProw("ONOFF1") = WF_ONOFF1.Text                '表示フラグ1

        OIS0001INProw("SORTNO1") = WF_SORTNO1.Text              '表示順1

        OIS0001INProw("OUTPUTID2") = WF_OUTPUTID2.Text          '情報出力ID2

        OIS0001INProw("ONOFF2") = WF_ONOFF2.Text                '表示フラグ2

        OIS0001INProw("SORTNO2") = WF_SORTNO2.Text              '表示順2

        OIS0001INProw("OUTPUTID3") = WF_OUTPUTID3.Text          '情報出力ID3

        OIS0001INProw("ONOFF3") = WF_ONOFF3.Text                '表示フラグ3

        OIS0001INProw("SORTNO3") = WF_SORTNO3.Text              '表示順3

        OIS0001INProw("OUTPUTID4") = WF_OUTPUTID4.Text          '情報出力ID4

        OIS0001INProw("ONOFF4") = WF_ONOFF4.Text                '表示フラグ4

        OIS0001INProw("SORTNO4") = WF_SORTNO4.Text              '表示順4

        OIS0001INProw("OUTPUTID5") = WF_OUTPUTID5.Text          '情報出力ID5

        OIS0001INProw("ONOFF5") = WF_ONOFF5.Text                '表示フラグ5

        OIS0001INProw("SORTNO5") = WF_SORTNO5.Text              '表示順5

        OIS0001INProw("OUTPUTID6") = WF_OUTPUTID6.Text          '情報出力ID6

        OIS0001INProw("ONOFF6") = WF_ONOFF6.Text                '表示フラグ6

        OIS0001INProw("SORTNO6") = WF_SORTNO6.Text              '表示順6

        OIS0001INProw("OUTPUTID7") = WF_OUTPUTID7.Text          '情報出力ID7

        OIS0001INProw("ONOFF7") = WF_ONOFF7.Text                '表示フラグ7

        OIS0001INProw("SORTNO7") = WF_SORTNO7.Text              '表示順7

        OIS0001INProw("OUTPUTID8") = WF_OUTPUTID8.Text          '情報出力ID8

        OIS0001INProw("ONOFF8") = WF_ONOFF8.Text                '表示フラグ8

        OIS0001INProw("SORTNO8") = WF_SORTNO8.Text              '表示順8

        OIS0001INProw("OUTPUTID9") = WF_OUTPUTID9.Text          '情報出力ID9

        OIS0001INProw("ONOFF9") = WF_ONOFF9.Text                '表示フラグ9

        OIS0001INProw("SORTNO9") = WF_SORTNO9.Text              '表示順9

        OIS0001INProw("OUTPUTID10") = WF_OUTPUTID10.Text        '情報出力ID10

        OIS0001INProw("ONOFF10") = WF_ONOFF10.Text              '表示フラグ10

        OIS0001INProw("SORTNO10") = WF_SORTNO10.Text            '表示順10

        OIS0001INProw("OUTPUTID11") = WF_OUTPUTID11.Text        '情報出力ID11

        OIS0001INProw("ONOFF11") = WF_ONOFF11.Text              '表示フラグ11

        OIS0001INProw("SORTNO11") = WF_SORTNO11.Text            '表示順11

        OIS0001INProw("OUTPUTID12") = WF_OUTPUTID12.Text        '情報出力ID12

        OIS0001INProw("ONOFF12") = WF_ONOFF12.Text              '表示フラグ12

        OIS0001INProw("SORTNO12") = WF_SORTNO12.Text            '表示順12

        OIS0001INProw("OUTPUTID13") = WF_OUTPUTID13.Text        '情報出力ID13

        OIS0001INProw("ONOFF13") = WF_ONOFF13.Text              '表示フラグ13

        OIS0001INProw("SORTNO13") = WF_SORTNO13.Text            '表示順13

        OIS0001INProw("OUTPUTID14") = WF_OUTPUTID14.Text        '情報出力ID14

        OIS0001INProw("ONOFF14") = WF_ONOFF14.Text              '表示フラグ14

        OIS0001INProw("SORTNO14") = WF_SORTNO14.Text            '表示順14

        OIS0001INProw("OUTPUTID15") = WF_OUTPUTID15.Text        '情報出力ID15

        OIS0001INProw("ONOFF15") = WF_ONOFF15.Text              '表示フラグ15

        OIS0001INProw("SORTNO15") = WF_SORTNO15.Text            '表示順15

        OIS0001INProw("OUTPUTID16") = WF_OUTPUTID16.Text        '情報出力ID16

        OIS0001INProw("ONOFF16") = WF_ONOFF16.Text              '表示フラグ16

        OIS0001INProw("SORTNO16") = WF_SORTNO16.Text            '表示順16

        OIS0001INProw("OUTPUTID17") = WF_OUTPUTID17.Text        '情報出力ID17

        OIS0001INProw("ONOFF17") = WF_ONOFF17.Text              '表示フラグ17

        OIS0001INProw("SORTNO17") = WF_SORTNO17.Text            '表示順17

        OIS0001INProw("OUTPUTID18") = WF_OUTPUTID18.Text        '情報出力ID18

        OIS0001INProw("ONOFF18") = WF_ONOFF18.Text              '表示フラグ18

        OIS0001INProw("SORTNO18") = WF_SORTNO18.Text            '表示順18

        OIS0001INProw("OUTPUTID19") = WF_OUTPUTID19.Text        '情報出力ID19

        OIS0001INProw("ONOFF19") = WF_ONOFF19.Text              '表示フラグ19

        OIS0001INProw("SORTNO19") = WF_SORTNO19.Text            '表示順19

        OIS0001INProw("OUTPUTID20") = WF_OUTPUTID20.Text        '情報出力ID20

        OIS0001INProw("ONOFF20") = WF_ONOFF20.Text              '表示フラグ20

        OIS0001INProw("SORTNO20") = WF_SORTNO20.Text            '表示順20

        OIS0001INProw("OUTPUTID21") = WF_OUTPUTID21.Text        '情報出力ID21

        OIS0001INProw("ONOFF21") = WF_ONOFF21.Text              '表示フラグ21

        OIS0001INProw("SORTNO21") = WF_SORTNO21.Text            '表示順21

        OIS0001INProw("OUTPUTID22") = WF_OUTPUTID22.Text        '情報出力ID22

        OIS0001INProw("ONOFF22") = WF_ONOFF22.Text              '表示フラグ22

        OIS0001INProw("SORTNO22") = WF_SORTNO22.Text            '表示順22

        OIS0001INProw("OUTPUTID23") = WF_OUTPUTID23.Text        '情報出力ID23

        OIS0001INProw("ONOFF23") = WF_ONOFF23.Text              '表示フラグ23

        OIS0001INProw("SORTNO23") = WF_SORTNO23.Text            '表示順23

        OIS0001INProw("OUTPUTID24") = WF_OUTPUTID24.Text        '情報出力ID24

        OIS0001INProw("ONOFF24") = WF_ONOFF24.Text              '表示フラグ24

        OIS0001INProw("SORTNO24") = WF_SORTNO24.Text            '表示順24

        OIS0001INProw("OUTPUTID25") = WF_OUTPUTID25.Text        '情報出力ID25

        OIS0001INProw("ONOFF25") = WF_ONOFF25.Text              '表示フラグ25

        OIS0001INProw("SORTNO25") = WF_SORTNO25.Text            '表示順25

        '○ チェック用テーブルに登録する
        OIS0001INPtbl.Rows.Add(OIS0001INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIS0001INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim OIS0001INProw As DataRow = OIS0001INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            ' KEY項目が等しい時
            If OIS0001row("USERID") = OIS0001INProw("USERID") AndAlso
                OIS0001row("STYMD") = OIS0001INProw("STYMD") Then
                ' KEY項目以外の項目の差異をチェック
                If OIS0001row("DELFLG") = OIS0001INProw("DELFLG") AndAlso
                    OIS0001row("STAFFNAMES") = OIS0001INProw("STAFFNAMES") AndAlso
                    OIS0001row("STAFFNAMEL") = OIS0001INProw("STAFFNAMEL") AndAlso
                    OIS0001row("MAPID") = OIS0001INProw("MAPID") AndAlso
                    OIS0001row("PASSWORD") = OIS0001INProw("PASSWORD") AndAlso
                    OIS0001row("MISSCNT") = OIS0001INProw("MISSCNT") AndAlso
                    OIS0001row("PASSENDYMD") = OIS0001INProw("PASSENDYMD") AndAlso
                    OIS0001row("ENDYMD") = OIS0001INProw("ENDYMD") AndAlso
                    OIS0001row("CAMPCODE") = OIS0001INProw("CAMPCODE") AndAlso
                    OIS0001row("ORG") = OIS0001INProw("ORG") AndAlso
                    OIS0001row("EMAIL") = OIS0001INProw("EMAIL") AndAlso
                    OIS0001row("MENUROLE") = OIS0001INProw("MENUROLE") AndAlso
                    OIS0001row("MAPROLE") = OIS0001INProw("MAPROLE") AndAlso
                    OIS0001row("VIEWPROFID") = OIS0001INProw("VIEWPROFID") AndAlso
                    OIS0001row("RPRTPROFID") = OIS0001INProw("RPRTPROFID") AndAlso
                    OIS0001row("VARIANT") = OIS0001INProw("VARIANT") AndAlso
                    OIS0001row("APPROVALID") = OIS0001INProw("APPROVALID") AndAlso
                    OIS0001row("OUTPUTID1") = OIS0001INProw("OUTPUTID1") AndAlso
                    OIS0001row("ONOFF1") = OIS0001INProw("ONOFF1") AndAlso
                    OIS0001row("SORTNO1") = OIS0001INProw("SORTNO1") AndAlso
                    OIS0001row("OUTPUTID2") = OIS0001INProw("OUTPUTID2") AndAlso
                    OIS0001row("ONOFF2") = OIS0001INProw("ONOFF2") AndAlso
                    OIS0001row("SORTNO2") = OIS0001INProw("SORTNO2") AndAlso
                    OIS0001row("OUTPUTID3") = OIS0001INProw("OUTPUTID3") AndAlso
                    OIS0001row("ONOFF3") = OIS0001INProw("ONOFF3") AndAlso
                    OIS0001row("SORTNO3") = OIS0001INProw("SORTNO3") AndAlso
                    OIS0001row("OUTPUTID4") = OIS0001INProw("OUTPUTID4") AndAlso
                    OIS0001row("ONOFF4") = OIS0001INProw("ONOFF4") AndAlso
                    OIS0001row("SORTNO4") = OIS0001INProw("SORTNO4") AndAlso
                    OIS0001row("OUTPUTID5") = OIS0001INProw("OUTPUTID5") AndAlso
                    OIS0001row("ONOFF5") = OIS0001INProw("ONOFF5") AndAlso
                    OIS0001row("SORTNO5") = OIS0001INProw("SORTNO5") AndAlso
                    OIS0001row("OUTPUTID6") = OIS0001INProw("OUTPUTID6") AndAlso
                    OIS0001row("ONOFF6") = OIS0001INProw("ONOFF6") AndAlso
                    OIS0001row("SORTNO6") = OIS0001INProw("SORTNO6") AndAlso
                    OIS0001row("OUTPUTID7") = OIS0001INProw("OUTPUTID7") AndAlso
                    OIS0001row("ONOFF7") = OIS0001INProw("ONOFF7") AndAlso
                    OIS0001row("SORTNO7") = OIS0001INProw("SORTNO7") AndAlso
                    OIS0001row("OUTPUTID8") = OIS0001INProw("OUTPUTID8") AndAlso
                    OIS0001row("ONOFF8") = OIS0001INProw("ONOFF8") AndAlso
                    OIS0001row("SORTNO8") = OIS0001INProw("SORTNO8") AndAlso
                    OIS0001row("OUTPUTID9") = OIS0001INProw("OUTPUTID9") AndAlso
                    OIS0001row("ONOFF9") = OIS0001INProw("ONOFF9") AndAlso
                    OIS0001row("SORTNO9") = OIS0001INProw("SORTNO9") AndAlso
                    OIS0001row("OUTPUTID10") = OIS0001INProw("OUTPUTID10") AndAlso
                    OIS0001row("ONOFF10") = OIS0001INProw("ONOFF10") AndAlso
                    OIS0001row("SORTNO10") = OIS0001INProw("SORTNO10") AndAlso
                    OIS0001row("OUTPUTID11") = OIS0001INProw("OUTPUTID11") AndAlso
                    OIS0001row("ONOFF11") = OIS0001INProw("ONOFF11") AndAlso
                    OIS0001row("SORTNO11") = OIS0001INProw("SORTNO11") AndAlso
                    OIS0001row("OUTPUTID12") = OIS0001INProw("OUTPUTID12") AndAlso
                    OIS0001row("ONOFF12") = OIS0001INProw("ONOFF12") AndAlso
                    OIS0001row("SORTNO12") = OIS0001INProw("SORTNO12") AndAlso
                    OIS0001row("OUTPUTID13") = OIS0001INProw("OUTPUTID13") AndAlso
                    OIS0001row("ONOFF13") = OIS0001INProw("ONOFF13") AndAlso
                    OIS0001row("SORTNO13") = OIS0001INProw("SORTNO13") AndAlso
                    OIS0001row("OUTPUTID14") = OIS0001INProw("OUTPUTID14") AndAlso
                    OIS0001row("ONOFF14") = OIS0001INProw("ONOFF14") AndAlso
                    OIS0001row("SORTNO14") = OIS0001INProw("SORTNO14") AndAlso
                    OIS0001row("OUTPUTID15") = OIS0001INProw("OUTPUTID15") AndAlso
                    OIS0001row("ONOFF15") = OIS0001INProw("ONOFF15") AndAlso
                    OIS0001row("SORTNO15") = OIS0001INProw("SORTNO15") AndAlso
                    OIS0001row("OUTPUTID16") = OIS0001INProw("OUTPUTID16") AndAlso
                    OIS0001row("ONOFF16") = OIS0001INProw("ONOFF16") AndAlso
                    OIS0001row("SORTNO16") = OIS0001INProw("SORTNO16") AndAlso
                    OIS0001row("OUTPUTID17") = OIS0001INProw("OUTPUTID17") AndAlso
                    OIS0001row("ONOFF17") = OIS0001INProw("ONOFF17") AndAlso
                    OIS0001row("SORTNO17") = OIS0001INProw("SORTNO17") AndAlso
                    OIS0001row("OUTPUTID18") = OIS0001INProw("OUTPUTID18") AndAlso
                    OIS0001row("ONOFF18") = OIS0001INProw("ONOFF18") AndAlso
                    OIS0001row("SORTNO18") = OIS0001INProw("SORTNO18") AndAlso
                    OIS0001row("OUTPUTID19") = OIS0001INProw("OUTPUTID19") AndAlso
                    OIS0001row("ONOFF19") = OIS0001INProw("ONOFF19") AndAlso
                    OIS0001row("SORTNO19") = OIS0001INProw("SORTNO19") AndAlso
                    OIS0001row("OUTPUTID20") = OIS0001INProw("OUTPUTID20") AndAlso
                    OIS0001row("ONOFF20") = OIS0001INProw("ONOFF20") AndAlso
                    OIS0001row("SORTNO20") = OIS0001INProw("SORTNO20") AndAlso
                    OIS0001row("OUTPUTID21") = OIS0001INProw("OUTPUTID21") AndAlso
                    OIS0001row("ONOFF21") = OIS0001INProw("ONOFF21") AndAlso
                    OIS0001row("SORTNO21") = OIS0001INProw("SORTNO21") AndAlso
                    OIS0001row("OUTPUTID22") = OIS0001INProw("OUTPUTID22") AndAlso
                    OIS0001row("ONOFF22") = OIS0001INProw("ONOFF22") AndAlso
                    OIS0001row("SORTNO22") = OIS0001INProw("SORTNO22") AndAlso
                    OIS0001row("OUTPUTID23") = OIS0001INProw("OUTPUTID23") AndAlso
                    OIS0001row("ONOFF23") = OIS0001INProw("ONOFF23") AndAlso
                    OIS0001row("SORTNO23") = OIS0001INProw("SORTNO23") AndAlso
                    OIS0001row("OUTPUTID24") = OIS0001INProw("OUTPUTID24") AndAlso
                    OIS0001row("ONOFF24") = OIS0001INProw("ONOFF24") AndAlso
                    OIS0001row("SORTNO24") = OIS0001INProw("SORTNO24") AndAlso
                    OIS0001row("OUTPUTID25") = OIS0001INProw("OUTPUTID25") AndAlso
                    OIS0001row("ONOFF25") = OIS0001INProw("ONOFF25") AndAlso
                    OIS0001row("SORTNO25") = OIS0001INProw("SORTNO25") Then
                    ' 変更がない時は、入力変更フラグをOFFにする
                    inputChangeFlg = False
                End If

                Exit For

            End If
        Next

        If inputChangeFlg Then
            '変更がある場合は、確認ダイアログを表示
            Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
                needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOk")
        Else
            '変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
            WF_CLEAR_ConfirmOkClick()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時、確認ダイアログOKボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_ConfirmOkClick()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIS0001tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT

        WF_USERID.Text = ""            'ユーザID
        WF_STAFFNAMES.Text = ""            '社員名（短）
        WF_STAFFNAMEL.Text = ""            '社員名（長）
        WF_MAPID.Text = "M00001"            '画面ＩＤ
        WF_PASSWORD.Text = ""                   'パスワード
        WF_PASSWORD.Attributes("Value") = ""
        WF_MISSCNT.Text = ""            '誤り回数
        WF_PASSENDYMD.Text = ""            'パスワード有効期限
        WF_STYMD.Text = ""            '開始年月日
        WF_ENDYMD.Text = ""            '終了年月日
        WF_CAMPCODE.Text = ""            '会社コード
        WF_ORG.Text = ""            '組織コード
        WF_EMAIL.Text = ""            'メールアドレス
        WF_MENUROLE.Text = ""            'メニュー表示制御ロール
        WF_MAPROLE.Text = ""            '画面参照更新制御ロール
        WF_VIEWPROFID.Text = ""            '画面表示項目制御ロール
        WF_RPRTPROFID.Text = ""            'エクセル出力制御ロール
        WF_VARIANT.Text = ""            '画面初期値ロール
        WF_APPROVALID.Text = ""            '承認権限ロール
        WF_DELFLG.Text = ""                 '削除フラグ
        WF_DELFLG_TEXT.Text = ""            '削除フラグ名称
        WF_OUTPUTID1.Text = ""            '情報出力ID1
        WF_OUTPUTID1_TEXT.Text = ""            '情報出力ID1名称
        WF_ONOFF1.Text = ""            '表示フラグ1
        WF_ONOFF1_TEXT.Text = ""            '表示フラグ1名称
        WF_SORTNO1.Text = ""            '表示順1
        WF_OUTPUTID2.Text = ""            '情報出力ID2
        WF_OUTPUTID2_TEXT.Text = ""            '情報出力ID2名称
        WF_ONOFF2.Text = ""            '表示フラグ2
        WF_ONOFF2_TEXT.Text = ""            '表示フラグ2名称
        WF_SORTNO2.Text = ""            '表示順2
        WF_OUTPUTID3.Text = ""            '情報出力ID3
        WF_OUTPUTID3_TEXT.Text = ""            '情報出力ID3名称
        WF_ONOFF3.Text = ""            '表示フラグ3
        WF_ONOFF3_TEXT.Text = ""            '表示フラグ3名称
        WF_SORTNO3.Text = ""            '表示順3
        WF_OUTPUTID4.Text = ""            '情報出力ID4
        WF_OUTPUTID4_TEXT.Text = ""            '情報出力ID4名称
        WF_ONOFF4.Text = ""            '表示フラグ4
        WF_ONOFF4_TEXT.Text = ""            '表示フラグ4名称
        WF_SORTNO4.Text = ""            '表示順4
        WF_OUTPUTID5.Text = ""            '情報出力ID5
        WF_OUTPUTID5_TEXT.Text = ""            '情報出力ID5名称
        WF_ONOFF5.Text = ""            '表示フラグ5
        WF_ONOFF5_TEXT.Text = ""            '表示フラグ5名称
        WF_SORTNO5.Text = ""            '表示順5
        WF_OUTPUTID6.Text = ""            '情報出力ID6
        WF_OUTPUTID6_TEXT.Text = ""            '情報出力ID6名称
        WF_ONOFF6.Text = ""            '表示フラグ6
        WF_ONOFF6_TEXT.Text = ""            '表示フラグ6名称
        WF_SORTNO6.Text = ""            '表示順6
        WF_OUTPUTID7.Text = ""            '情報出力ID7
        WF_OUTPUTID7_TEXT.Text = ""            '情報出力ID7名称
        WF_ONOFF7.Text = ""            '表示フラグ7
        WF_ONOFF7_TEXT.Text = ""            '表示フラグ7名称
        WF_SORTNO7.Text = ""            '表示順7
        WF_OUTPUTID8.Text = ""            '情報出力ID8
        WF_OUTPUTID8_TEXT.Text = ""            '情報出力ID8名称
        WF_ONOFF8.Text = ""            '表示フラグ8
        WF_ONOFF8_TEXT.Text = ""            '表示フラグ8名称
        WF_SORTNO8.Text = ""            '表示順8
        WF_OUTPUTID9.Text = ""            '情報出力ID9
        WF_OUTPUTID9_TEXT.Text = ""            '情報出力ID9名称
        WF_ONOFF9.Text = ""            '表示フラグ9
        WF_ONOFF9_TEXT.Text = ""            '表示フラグ9名称
        WF_SORTNO9.Text = ""            '表示順9
        WF_OUTPUTID10.Text = ""            '情報出力ID10
        WF_OUTPUTID10_TEXT.Text = ""            '情報出力ID10名称
        WF_ONOFF10.Text = ""            '表示フラグ10
        WF_ONOFF10_TEXT.Text = ""            '表示フラグ10名称
        WF_SORTNO10.Text = ""            '表示順10
        WF_OUTPUTID11.Text = ""            '情報出力ID11
        WF_OUTPUTID11_TEXT.Text = ""            '情報出力ID11名称
        WF_ONOFF11.Text = ""            '表示フラグ11
        WF_ONOFF11_TEXT.Text = ""            '表示フラグ11名称
        WF_SORTNO11.Text = ""            '表示順11
        WF_OUTPUTID12.Text = ""            '情報出力ID12
        WF_OUTPUTID12_TEXT.Text = ""            '情報出力ID12名称
        WF_ONOFF12.Text = ""            '表示フラグ12
        WF_ONOFF12_TEXT.Text = ""            '表示フラグ12名称
        WF_SORTNO12.Text = ""            '表示順12
        WF_OUTPUTID13.Text = ""            '情報出力ID13
        WF_OUTPUTID13_TEXT.Text = ""            '情報出力ID13名称
        WF_ONOFF13.Text = ""            '表示フラグ13
        WF_ONOFF13_TEXT.Text = ""            '表示フラグ13名称
        WF_SORTNO13.Text = ""            '表示順13
        WF_OUTPUTID14.Text = ""            '情報出力ID14
        WF_OUTPUTID14_TEXT.Text = ""            '情報出力ID14名称
        WF_ONOFF14.Text = ""            '表示フラグ14
        WF_ONOFF14_TEXT.Text = ""            '表示フラグ14名称
        WF_SORTNO14.Text = ""            '表示順14
        WF_OUTPUTID15.Text = ""            '情報出力ID15
        WF_OUTPUTID15_TEXT.Text = ""            '情報出力ID15名称
        WF_ONOFF15.Text = ""            '表示フラグ15
        WF_ONOFF15_TEXT.Text = ""            '表示フラグ15名称
        WF_SORTNO15.Text = ""            '表示順15
        WF_OUTPUTID16.Text = ""            '情報出力ID16
        WF_OUTPUTID16_TEXT.Text = ""            '情報出力ID16名称
        WF_ONOFF16.Text = ""            '表示フラグ16
        WF_ONOFF16_TEXT.Text = ""            '表示フラグ16名称
        WF_SORTNO16.Text = ""            '表示順16
        WF_OUTPUTID17.Text = ""            '情報出力ID17
        WF_OUTPUTID17_TEXT.Text = ""            '情報出力ID17名称
        WF_ONOFF17.Text = ""            '表示フラグ17
        WF_ONOFF17_TEXT.Text = ""            '表示フラグ17名称
        WF_SORTNO17.Text = ""            '表示順17
        WF_OUTPUTID18.Text = ""            '情報出力ID18
        WF_OUTPUTID18_TEXT.Text = ""            '情報出力ID18名称
        WF_ONOFF18.Text = ""            '表示フラグ18
        WF_ONOFF18_TEXT.Text = ""            '表示フラグ18名称
        WF_SORTNO18.Text = ""            '表示順18
        WF_OUTPUTID19.Text = ""            '情報出力ID19
        WF_OUTPUTID19_TEXT.Text = ""            '情報出力ID19名称
        WF_ONOFF19.Text = ""            '表示フラグ19
        WF_ONOFF19_TEXT.Text = ""            '表示フラグ19名称
        WF_SORTNO19.Text = ""            '表示順19
        WF_OUTPUTID20.Text = ""            '情報出力ID20
        WF_OUTPUTID20_TEXT.Text = ""            '情報出力ID20名称
        WF_ONOFF20.Text = ""            '表示フラグ20
        WF_ONOFF20_TEXT.Text = ""            '表示フラグ20名称
        WF_SORTNO20.Text = ""            '表示順20
        WF_OUTPUTID21.Text = ""            '情報出力ID21
        WF_OUTPUTID21_TEXT.Text = ""            '情報出力ID21名称
        WF_ONOFF21.Text = ""            '表示フラグ21
        WF_ONOFF21_TEXT.Text = ""            '表示フラグ21名称
        WF_SORTNO21.Text = ""            '表示順21
        WF_OUTPUTID22.Text = ""            '情報出力ID22
        WF_OUTPUTID22_TEXT.Text = ""            '情報出力ID22名称
        WF_ONOFF22.Text = ""            '表示フラグ22
        WF_ONOFF22_TEXT.Text = ""            '表示フラグ22名称
        WF_SORTNO22.Text = ""            '表示順22
        WF_OUTPUTID23.Text = ""            '情報出力ID23
        WF_OUTPUTID23_TEXT.Text = ""            '情報出力ID23名称
        WF_ONOFF23.Text = ""            '表示フラグ23
        WF_ONOFF23_TEXT.Text = ""            '表示フラグ23名称
        WF_SORTNO23.Text = ""            '表示順23
        WF_OUTPUTID24.Text = ""            '情報出力ID24
        WF_OUTPUTID24_TEXT.Text = ""            '情報出力ID24名称
        WF_ONOFF24.Text = ""            '表示フラグ24
        WF_ONOFF24_TEXT.Text = ""            '表示フラグ24名称
        WF_SORTNO24.Text = ""            '表示順24
        WF_OUTPUTID25.Text = ""            '情報出力ID25
        WF_OUTPUTID25_TEXT.Text = ""            '情報出力ID25名称
        WF_ONOFF25.Text = ""            '表示フラグ25
        WF_ONOFF25_TEXT.Text = ""            '表示フラグ25名称
        WF_SORTNO25.Text = ""            '表示順25

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

            Dim WW_FIELD As String = ""
            If WF_FIELD_REP.Value = "" Then
                WW_FIELD = WF_FIELD.Value
            Else
                WW_FIELD = WF_FIELD_REP.Value
            End If

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        '日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_PASSENDYMD"    'パスワード有効期限
                                .WF_Calendar.Text = WF_PASSENDYMD.Text
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = WF_STYMD.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = WF_ENDYMD.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        Dim prmData As New Hashtable

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_CAMPCODE"       '会社コード
                                If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                                Else
                                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ROLE
                                End If
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = WF_CAMPCODE.Text

                            Case "WF_ORG"       '組織コード
                                Dim AUTHORITYALL_FLG As String = "0"
                                If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                    If WF_CAMPCODE.Text = "" Then '会社コードが空の場合
                                        AUTHORITYALL_FLG = "1"
                                    Else '会社コードに入力済みの場合
                                        AUTHORITYALL_FLG = "2"
                                    End If
                                End If
                                prmData = work.CreateORGParam(WF_CAMPCODE.Text, AUTHORITYALL_FLG)
                            Case "WF_MENUROLE"       'メニュー表示制御ロール
                                prmData = work.CreateRoleList(WF_CAMPCODE.Text, "MENU")
                            Case "WF_MAPROLE"       '画面参照更新制御ロール
                                prmData = work.CreateRoleList(WF_CAMPCODE.Text, "MAP")
                            Case "WF_VIEWPROFID"       '画面表示項目制御ロール
                                prmData = work.CreateRoleList(WF_CAMPCODE.Text, "VIEW")
                            Case "WF_RPRTPROFID"       'エクセル出力制御ロール
                                prmData = work.CreateRoleList(WF_CAMPCODE.Text, "XML")
                            Case "WF_APPROVALID"       '承認権限ロール
                                prmData = work.CreateRoleList(WF_CAMPCODE.Text, "APPROVAL")
                            Case "WF_OUTPUTID1"  '情報出力ID1
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF1"     '表示フラグ1
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID2"  '情報出力ID2
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF2"     '表示フラグ2
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID3"  '情報出力ID3
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF3"     '表示フラグ3
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID4"  '情報出力ID4
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF4"     '表示フラグ4
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID5"  '情報出力ID5
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF5"     '表示フラグ5
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID6"  '情報出力ID6
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF6"     '表示フラグ6
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID7"  '情報出力ID7
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF7"     '表示フラグ7
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID8"  '情報出力ID8
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF8"     '表示フラグ8
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID9"  '情報出力ID9
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF9"     '表示フラグ9
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID10"  '情報出力ID10
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF10"     '表示フラグ10
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID11"  '情報出力ID11
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF11"     '表示フラグ11
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID12"  '情報出力ID12
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF12"     '表示フラグ12
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID13"  '情報出力ID13
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF13"     '表示フラグ13
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID14"  '情報出力ID14
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF14"     '表示フラグ14
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID15"  '情報出力ID15
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF15"     '表示フラグ15
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID16"  '情報出力ID16
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF16"     '表示フラグ16
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID17"  '情報出力ID17
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF17"     '表示フラグ17
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID18"  '情報出力ID18
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF18"     '表示フラグ18
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID19"  '情報出力ID19
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF19"     '表示フラグ19
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID20"  '情報出力ID20
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF20"     '表示フラグ20
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID21"  '情報出力ID21
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF21"     '表示フラグ21
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID22"  '情報出力ID22
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF22"     '表示フラグ22
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID23"  '情報出力ID23
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF23"     '表示フラグ23
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID24"  '情報出力ID24
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF24"     '表示フラグ24
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_OUTPUTID25"  '情報出力ID25
                                prmData = work.CreateFIXParam(Master.USERCAMP, "PANEID")
                            Case "WF_ONOFF25"     '表示フラグ25
                                prmData = work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG")
                            Case "WF_DELFLG"
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                                prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
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

            Case "WF_ORG"               '組織コード
                CODENAME_get("ORG", WF_ORG.Text, WF_ORG_TEXT.Text, WW_RTN_SW)

            Case "WF_MENUROLE"               'メニュー表示制御ロール
                CODENAME_get("MENU", WF_MENUROLE.Text, WF_MENUROLE_TEXT.Text, WW_DUMMY)

            Case "WF_MAPROLE"               '画面参照更新制御ロール
                CODENAME_get("MAP", WF_MAPROLE.Text, WF_MAPROLE_TEXT.Text, WW_DUMMY)

            Case "WF_VIEWPROFID"               '画面表示項目制御ロール
                CODENAME_get("VIEW", WF_VIEWPROFID.Text, WF_VIEWPROFID_TEXT.Text, WW_DUMMY)

            Case "WF_RPRTPROFID"               'エクセル出力制御ロール
                CODENAME_get("XML", WF_RPRTPROFID.Text, WF_RPRTPROFID_TEXT.Text, WW_DUMMY)

            Case "WF_APPROVALID"               '承認権限ロール
                CODENAME_get("APPROVAL", WF_APPROVALID.Text, WF_APPROVALID_TEXT.Text, WW_DUMMY)

            Case "WF_OUTPUTID1"          '情報出力ID1
                CODENAME_get("OUTPUTID", WF_OUTPUTID1.Text, WF_OUTPUTID1_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF1"               '表示フラグ1
                CODENAME_get("ONOFF", WF_ONOFF1.Text, WF_ONOFF1_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID2"          '情報出力ID2
                CODENAME_get("OUTPUTID", WF_OUTPUTID2.Text, WF_OUTPUTID2_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF2"               '表示フラグ2
                CODENAME_get("ONOFF", WF_ONOFF2.Text, WF_ONOFF2_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID3"          '情報出力ID3
                CODENAME_get("OUTPUTID", WF_OUTPUTID3.Text, WF_OUTPUTID3_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF3"               '表示フラグ3
                CODENAME_get("ONOFF", WF_ONOFF3.Text, WF_ONOFF3_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID4"          '情報出力ID4
                CODENAME_get("OUTPUTID", WF_OUTPUTID4.Text, WF_OUTPUTID4_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF4"               '表示フラグ4
                CODENAME_get("ONOFF", WF_ONOFF4.Text, WF_ONOFF4_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID5"          '情報出力ID5
                CODENAME_get("OUTPUTID", WF_OUTPUTID5.Text, WF_OUTPUTID5_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF5"               '表示フラグ5
                CODENAME_get("ONOFF", WF_ONOFF5.Text, WF_ONOFF5_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID6"          '情報出力ID6
                CODENAME_get("OUTPUTID", WF_OUTPUTID6.Text, WF_OUTPUTID6_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF6"               '表示フラグ6
                CODENAME_get("ONOFF", WF_ONOFF6.Text, WF_ONOFF6_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID7"          '情報出力ID7
                CODENAME_get("OUTPUTID", WF_OUTPUTID7.Text, WF_OUTPUTID7_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF7"               '表示フラグ7
                CODENAME_get("ONOFF", WF_ONOFF7.Text, WF_ONOFF7_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID8"          '情報出力ID8
                CODENAME_get("OUTPUTID", WF_OUTPUTID8.Text, WF_OUTPUTID8_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF8"               '表示フラグ8
                CODENAME_get("ONOFF", WF_ONOFF8.Text, WF_ONOFF8_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID9"          '情報出力ID9
                CODENAME_get("OUTPUTID", WF_OUTPUTID9.Text, WF_OUTPUTID9_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF9"               '表示フラグ9
                CODENAME_get("ONOFF", WF_ONOFF9.Text, WF_ONOFF9_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID10"          '情報出力ID10
                CODENAME_get("OUTPUTID", WF_OUTPUTID10.Text, WF_OUTPUTID10_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF10"               '表示フラグ10
                CODENAME_get("ONOFF", WF_ONOFF10.Text, WF_ONOFF10_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID11"          '情報出力ID11
                CODENAME_get("OUTPUTID", WF_OUTPUTID11.Text, WF_OUTPUTID11_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF11"               '表示フラグ11
                CODENAME_get("ONOFF", WF_ONOFF11.Text, WF_ONOFF11_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID12"          '情報出力ID12
                CODENAME_get("OUTPUTID", WF_OUTPUTID12.Text, WF_OUTPUTID12_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF12"               '表示フラグ12
                CODENAME_get("ONOFF", WF_ONOFF12.Text, WF_ONOFF12_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID13"          '情報出力ID13
                CODENAME_get("OUTPUTID", WF_OUTPUTID13.Text, WF_OUTPUTID13_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF13"               '表示フラグ13
                CODENAME_get("ONOFF", WF_ONOFF13.Text, WF_ONOFF13_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID14"          '情報出力ID14
                CODENAME_get("OUTPUTID", WF_OUTPUTID14.Text, WF_OUTPUTID14_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF14"               '表示フラグ14
                CODENAME_get("ONOFF", WF_ONOFF14.Text, WF_ONOFF14_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID15"          '情報出力ID15
                CODENAME_get("OUTPUTID", WF_OUTPUTID15.Text, WF_OUTPUTID15_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF15"               '表示フラグ15
                CODENAME_get("ONOFF", WF_ONOFF15.Text, WF_ONOFF15_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID16"          '情報出力ID16
                CODENAME_get("OUTPUTID", WF_OUTPUTID16.Text, WF_OUTPUTID16_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF16"               '表示フラグ16
                CODENAME_get("ONOFF", WF_ONOFF16.Text, WF_ONOFF16_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID17"          '情報出力ID17
                CODENAME_get("OUTPUTID", WF_OUTPUTID17.Text, WF_OUTPUTID17_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF17"               '表示フラグ17
                CODENAME_get("ONOFF", WF_ONOFF17.Text, WF_ONOFF17_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID18"          '情報出力ID18
                CODENAME_get("OUTPUTID", WF_OUTPUTID18.Text, WF_OUTPUTID18_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF18"               '表示フラグ18
                CODENAME_get("ONOFF", WF_ONOFF18.Text, WF_ONOFF18_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID19"          '情報出力ID19
                CODENAME_get("OUTPUTID", WF_OUTPUTID19.Text, WF_OUTPUTID19_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF19"               '表示フラグ19
                CODENAME_get("ONOFF", WF_ONOFF19.Text, WF_ONOFF19_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID20"          '情報出力ID20
                CODENAME_get("OUTPUTID", WF_OUTPUTID20.Text, WF_OUTPUTID20_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF20"               '表示フラグ20
                CODENAME_get("ONOFF", WF_ONOFF20.Text, WF_ONOFF20_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID21"          '情報出力ID21
                CODENAME_get("OUTPUTID", WF_OUTPUTID21.Text, WF_OUTPUTID21_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF21"               '表示フラグ21
                CODENAME_get("ONOFF", WF_ONOFF21.Text, WF_ONOFF21_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID22"          '情報出力ID22
                CODENAME_get("OUTPUTID", WF_OUTPUTID22.Text, WF_OUTPUTID22_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF22"               '表示フラグ22
                CODENAME_get("ONOFF", WF_ONOFF22.Text, WF_ONOFF22_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID23"          '情報出力ID23
                CODENAME_get("OUTPUTID", WF_OUTPUTID23.Text, WF_OUTPUTID23_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF23"               '表示フラグ23
                CODENAME_get("ONOFF", WF_ONOFF23.Text, WF_ONOFF23_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID24"          '情報出力ID24
                CODENAME_get("OUTPUTID", WF_OUTPUTID24.Text, WF_OUTPUTID24_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF24"               '表示フラグ24
                CODENAME_get("ONOFF", WF_ONOFF24.Text, WF_ONOFF24_TEXT.Text, WW_RTN_SW)

            Case "WF_OUTPUTID25"          '情報出力ID25
                CODENAME_get("OUTPUTID", WF_OUTPUTID25.Text, WF_OUTPUTID25_TEXT.Text, WW_RTN_SW)

            Case "WF_ONOFF25"               '表示フラグ25
                CODENAME_get("ONOFF", WF_ONOFF25.Text, WF_ONOFF25_TEXT.Text, WW_RTN_SW)

            Case "WF_DELFLG"               '削除フラグ
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

            Case "WF_PASSWORD"
                WF_PASSWORD.Attributes("Value") = work.WF_SEL_PASSWORD.Text
        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
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
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "WF_DELFLG"            '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                Case "WF_PASSENDYMD"             'パスワード有効期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_PASSENDYMD.Text = ""
                        Else
                            WF_PASSENDYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_PASSENDYMD.Focus()

                Case "WF_STYMD"             '有効年月日(From)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_STYMD.Text = ""
                        Else
                            WF_STYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_STYMD.Focus()

                Case "WF_ENDYMD"            '有効年月日(To)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ENDYMD.Text = ""
                        Else
                            WF_ENDYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception

                    End Try
                    WF_ENDYMD.Focus()

                Case "WF_CAMPCODE"               '会社コード
                    WF_CAMPCODE.Text = WW_SelectValue
                    WF_CAMPCODE_TEXT.Text = WW_SelectText
                    WF_CAMPCODE.Focus()

                Case "WF_ORG"               '組織コード
                    WF_ORG.Text = WW_SelectValue
                    WF_ORG_TEXT.Text = WW_SelectText
                    WF_ORG.Focus()

                Case "WF_MENUROLE"               'メニュー表示制御ロール
                    WF_MENUROLE.Text = WW_SelectValue
                    WF_MENUROLE_TEXT.Text = WW_SelectText
                    WF_MENUROLE.Focus()

                Case "WF_MAPROLE"               '画面参照更新制御ロール
                    WF_MAPROLE.Text = WW_SelectValue
                    WF_MAPROLE_TEXT.Text = WW_SelectText
                    WF_MAPROLE.Focus()

                Case "WF_VIEWPROFID"               '画面表示項目制御ロール
                    WF_VIEWPROFID.Text = WW_SelectValue
                    WF_VIEWPROFID_TEXT.Text = WW_SelectText
                    WF_VIEWPROFID.Focus()

                Case "WF_RPRTPROFID"               'エクセル出力制御ロール
                    WF_RPRTPROFID.Text = WW_SelectValue
                    WF_RPRTPROFID_TEXT.Text = WW_SelectText
                    WF_RPRTPROFID.Focus()

                Case "WF_APPROVALID"               '承認権限ロール
                    WF_APPROVALID.Text = WW_SelectValue
                    WF_APPROVALID_TEXT.Text = WW_SelectText
                    WF_APPROVALID.Focus()

                Case "WF_OUTPUTID1"               '情報出力ID1
                    WF_OUTPUTID1.Text = WW_SelectValue
                    WF_OUTPUTID1_TEXT.Text = WW_SelectText
                    WF_OUTPUTID1.Focus()

                Case "WF_ONOFF1"                  '表示フラグ1
                    WF_ONOFF1.Text = WW_SelectValue
                    WF_ONOFF1_TEXT.Text = WW_SelectText
                    WF_ONOFF1.Focus()

                Case "WF_OUTPUTID2"               '情報出力ID2
                    WF_OUTPUTID2.Text = WW_SelectValue
                    WF_OUTPUTID2_TEXT.Text = WW_SelectText
                    WF_OUTPUTID2.Focus()

                Case "WF_ONOFF2"                  '表示フラグ2
                    WF_ONOFF2.Text = WW_SelectValue
                    WF_ONOFF2_TEXT.Text = WW_SelectText
                    WF_ONOFF2.Focus()

                Case "WF_OUTPUTID3"               '情報出力ID3
                    WF_OUTPUTID3.Text = WW_SelectValue
                    WF_OUTPUTID3_TEXT.Text = WW_SelectText
                    WF_OUTPUTID3.Focus()

                Case "WF_ONOFF3"                  '表示フラグ3
                    WF_ONOFF3.Text = WW_SelectValue
                    WF_ONOFF3_TEXT.Text = WW_SelectText
                    WF_ONOFF3.Focus()

                Case "WF_OUTPUTID4"               '情報出力ID4
                    WF_OUTPUTID4.Text = WW_SelectValue
                    WF_OUTPUTID4_TEXT.Text = WW_SelectText
                    WF_OUTPUTID4.Focus()

                Case "WF_ONOFF4"                  '表示フラグ4
                    WF_ONOFF4.Text = WW_SelectValue
                    WF_ONOFF4_TEXT.Text = WW_SelectText
                    WF_ONOFF4.Focus()

                Case "WF_OUTPUTID5"               '情報出力ID5
                    WF_OUTPUTID5.Text = WW_SelectValue
                    WF_OUTPUTID5_TEXT.Text = WW_SelectText
                    WF_OUTPUTID5.Focus()

                Case "WF_ONOFF5"                  '表示フラグ5
                    WF_ONOFF5.Text = WW_SelectValue
                    WF_ONOFF5_TEXT.Text = WW_SelectText
                    WF_ONOFF5.Focus()

                Case "WF_OUTPUTID6"               '情報出力ID6
                    WF_OUTPUTID6.Text = WW_SelectValue
                    WF_OUTPUTID6_TEXT.Text = WW_SelectText
                    WF_OUTPUTID6.Focus()

                Case "WF_ONOFF6"                  '表示フラグ6
                    WF_ONOFF6.Text = WW_SelectValue
                    WF_ONOFF6_TEXT.Text = WW_SelectText
                    WF_ONOFF6.Focus()

                Case "WF_OUTPUTID7"               '情報出力ID7
                    WF_OUTPUTID7.Text = WW_SelectValue
                    WF_OUTPUTID7_TEXT.Text = WW_SelectText
                    WF_OUTPUTID7.Focus()

                Case "WF_ONOFF7"                  '表示フラグ7
                    WF_ONOFF7.Text = WW_SelectValue
                    WF_ONOFF7_TEXT.Text = WW_SelectText
                    WF_ONOFF7.Focus()

                Case "WF_OUTPUTID8"               '情報出力ID8
                    WF_OUTPUTID8.Text = WW_SelectValue
                    WF_OUTPUTID8_TEXT.Text = WW_SelectText
                    WF_OUTPUTID8.Focus()

                Case "WF_ONOFF8"                  '表示フラグ8
                    WF_ONOFF8.Text = WW_SelectValue
                    WF_ONOFF8_TEXT.Text = WW_SelectText
                    WF_ONOFF8.Focus()

                Case "WF_OUTPUTID9"               '情報出力ID9
                    WF_OUTPUTID9.Text = WW_SelectValue
                    WF_OUTPUTID9_TEXT.Text = WW_SelectText
                    WF_OUTPUTID9.Focus()

                Case "WF_ONOFF9"                  '表示フラグ9
                    WF_ONOFF9.Text = WW_SelectValue
                    WF_ONOFF9_TEXT.Text = WW_SelectText
                    WF_ONOFF9.Focus()

                Case "WF_OUTPUTID10"               '情報出力ID10
                    WF_OUTPUTID10.Text = WW_SelectValue
                    WF_OUTPUTID10_TEXT.Text = WW_SelectText
                    WF_OUTPUTID10.Focus()

                Case "WF_ONOFF10"                  '表示フラグ10
                    WF_ONOFF10.Text = WW_SelectValue
                    WF_ONOFF10_TEXT.Text = WW_SelectText
                    WF_ONOFF10.Focus()

                Case "WF_OUTPUTID11"               '情報出力ID11
                    WF_OUTPUTID11.Text = WW_SelectValue
                    WF_OUTPUTID11_TEXT.Text = WW_SelectText
                    WF_OUTPUTID11.Focus()

                Case "WF_ONOFF11"                  '表示フラグ11
                    WF_ONOFF11.Text = WW_SelectValue
                    WF_ONOFF11_TEXT.Text = WW_SelectText
                    WF_ONOFF11.Focus()

                Case "WF_OUTPUTID12"               '情報出力ID12
                    WF_OUTPUTID12.Text = WW_SelectValue
                    WF_OUTPUTID12_TEXT.Text = WW_SelectText
                    WF_OUTPUTID12.Focus()

                Case "WF_ONOFF12"                  '表示フラグ12
                    WF_ONOFF12.Text = WW_SelectValue
                    WF_ONOFF12_TEXT.Text = WW_SelectText
                    WF_ONOFF12.Focus()

                Case "WF_OUTPUTID13"               '情報出力ID13
                    WF_OUTPUTID13.Text = WW_SelectValue
                    WF_OUTPUTID13_TEXT.Text = WW_SelectText
                    WF_OUTPUTID13.Focus()

                Case "WF_ONOFF13"                  '表示フラグ13
                    WF_ONOFF13.Text = WW_SelectValue
                    WF_ONOFF13_TEXT.Text = WW_SelectText
                    WF_ONOFF13.Focus()

                Case "WF_OUTPUTID14"               '情報出力ID14
                    WF_OUTPUTID14.Text = WW_SelectValue
                    WF_OUTPUTID14_TEXT.Text = WW_SelectText
                    WF_OUTPUTID14.Focus()

                Case "WF_ONOFF14"                  '表示フラグ14
                    WF_ONOFF14.Text = WW_SelectValue
                    WF_ONOFF14_TEXT.Text = WW_SelectText
                    WF_ONOFF14.Focus()

                Case "WF_OUTPUTID15"               '情報出力ID15
                    WF_OUTPUTID15.Text = WW_SelectValue
                    WF_OUTPUTID15_TEXT.Text = WW_SelectText
                    WF_OUTPUTID15.Focus()

                Case "WF_ONOFF15"                  '表示フラグ15
                    WF_ONOFF15.Text = WW_SelectValue
                    WF_ONOFF15_TEXT.Text = WW_SelectText
                    WF_ONOFF15.Focus()

                Case "WF_OUTPUTID16"               '情報出力ID16
                    WF_OUTPUTID16.Text = WW_SelectValue
                    WF_OUTPUTID16_TEXT.Text = WW_SelectText
                    WF_OUTPUTID16.Focus()

                Case "WF_ONOFF16"                  '表示フラグ16
                    WF_ONOFF16.Text = WW_SelectValue
                    WF_ONOFF16_TEXT.Text = WW_SelectText
                    WF_ONOFF16.Focus()

                Case "WF_OUTPUTID17"               '情報出力ID17
                    WF_OUTPUTID17.Text = WW_SelectValue
                    WF_OUTPUTID17_TEXT.Text = WW_SelectText
                    WF_OUTPUTID17.Focus()

                Case "WF_ONOFF17"                  '表示フラグ17
                    WF_ONOFF17.Text = WW_SelectValue
                    WF_ONOFF17_TEXT.Text = WW_SelectText
                    WF_ONOFF17.Focus()

                Case "WF_OUTPUTID18"               '情報出力ID18
                    WF_OUTPUTID18.Text = WW_SelectValue
                    WF_OUTPUTID18_TEXT.Text = WW_SelectText
                    WF_OUTPUTID18.Focus()

                Case "WF_ONOFF18"                  '表示フラグ18
                    WF_ONOFF18.Text = WW_SelectValue
                    WF_ONOFF18_TEXT.Text = WW_SelectText
                    WF_ONOFF18.Focus()

                Case "WF_OUTPUTID19"               '情報出力ID19
                    WF_OUTPUTID19.Text = WW_SelectValue
                    WF_OUTPUTID19_TEXT.Text = WW_SelectText
                    WF_OUTPUTID19.Focus()

                Case "WF_ONOFF19"                  '表示フラグ19
                    WF_ONOFF19.Text = WW_SelectValue
                    WF_ONOFF19_TEXT.Text = WW_SelectText
                    WF_ONOFF19.Focus()

                Case "WF_OUTPUTID20"               '情報出力ID20
                    WF_OUTPUTID20.Text = WW_SelectValue
                    WF_OUTPUTID20_TEXT.Text = WW_SelectText
                    WF_OUTPUTID20.Focus()

                Case "WF_ONOFF20"                  '表示フラグ20
                    WF_ONOFF20.Text = WW_SelectValue
                    WF_ONOFF20_TEXT.Text = WW_SelectText
                    WF_ONOFF20.Focus()

                Case "WF_OUTPUTID21"               '情報出力ID21
                    WF_OUTPUTID21.Text = WW_SelectValue
                    WF_OUTPUTID21_TEXT.Text = WW_SelectText
                    WF_OUTPUTID21.Focus()

                Case "WF_ONOFF21"                  '表示フラグ21
                    WF_ONOFF21.Text = WW_SelectValue
                    WF_ONOFF21_TEXT.Text = WW_SelectText
                    WF_ONOFF21.Focus()

                Case "WF_OUTPUTID22"               '情報出力ID22
                    WF_OUTPUTID22.Text = WW_SelectValue
                    WF_OUTPUTID22_TEXT.Text = WW_SelectText
                    WF_OUTPUTID22.Focus()

                Case "WF_ONOFF22"                  '表示フラグ22
                    WF_ONOFF22.Text = WW_SelectValue
                    WF_ONOFF22_TEXT.Text = WW_SelectText
                    WF_ONOFF22.Focus()

                Case "WF_OUTPUTID23"               '情報出力ID23
                    WF_OUTPUTID23.Text = WW_SelectValue
                    WF_OUTPUTID23_TEXT.Text = WW_SelectText
                    WF_OUTPUTID23.Focus()

                Case "WF_ONOFF23"                  '表示フラグ23
                    WF_ONOFF23.Text = WW_SelectValue
                    WF_ONOFF23_TEXT.Text = WW_SelectText
                    WF_ONOFF23.Focus()

                Case "WF_OUTPUTID24"               '情報出力ID24
                    WF_OUTPUTID24.Text = WW_SelectValue
                    WF_OUTPUTID24_TEXT.Text = WW_SelectText
                    WF_OUTPUTID24.Focus()

                Case "WF_ONOFF24"                  '表示フラグ24
                    WF_ONOFF24.Text = WW_SelectValue
                    WF_ONOFF24_TEXT.Text = WW_SelectText
                    WF_ONOFF24.Focus()

                Case "WF_OUTPUTID25"               '情報出力ID25
                    WF_OUTPUTID25.Text = WW_SelectValue
                    WF_OUTPUTID25_TEXT.Text = WW_SelectText
                    WF_OUTPUTID25.Focus()

                Case "WF_ONOFF25"                  '表示フラグ25
                    WF_ONOFF25.Text = WW_SelectValue
                    WF_ONOFF25_TEXT.Text = WW_SelectText
                    WF_ONOFF25.Focus()

            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "WF_DELFLG"            '削除フラグ
                    WF_DELFLG.Focus()

                Case "WF_PASSENDYMD"        'パスワード有効期限
                    WF_PASSENDYMD.Focus()

                Case "WF_STYMD"             '有効年月日(From)
                    WF_STYMD.Focus()

                Case "WF_ENDYMD"            '有効年月日(To)
                    WF_ENDYMD.Focus()

                Case "WF_CAMPCODE"          '会社コード
                    WF_CAMPCODE.Focus()

                Case "WF_ORG"               '組織コード
                    WF_ORG.Focus()

                Case "WF_MENUROLE"          'メニュー表示制御ロール
                    WF_MENUROLE.Focus()

                Case "WF_MAPROLE"           '画面参照更新制御ロール
                    WF_MAPROLE.Focus()

                Case "WF_VIEWPROFID"        '画面表示項目制御ロール
                    WF_VIEWPROFID.Focus()

                Case "WF_RPRTPROFID"        'エクセル出力制御ロール
                    WF_RPRTPROFID.Focus()

                Case "WF_APPROVALID"        '承認権限ロール
                    WF_APPROVALID.Focus()

                Case "WF_OUTPUTID1"         '情報出力ID1
                    WF_OUTPUTID1.Focus()

                Case "WF_ONOFF1"            '表示フラグ1
                    WF_ONOFF1.Focus()

                Case "WF_OUTPUTID2"         '情報出力ID2
                    WF_OUTPUTID2.Focus()

                Case "WF_ONOFF2"            '表示フラグ2
                    WF_ONOFF2.Focus()

                Case "WF_OUTPUTID3"         '情報出力ID3
                    WF_OUTPUTID3.Focus()

                Case "WF_ONOFF3"            '表示フラグ3
                    WF_ONOFF3.Focus()

                Case "WF_OUTPUTID4"         '情報出力ID4
                    WF_OUTPUTID4.Focus()

                Case "WF_ONOFF4"            '表示フラグ4
                    WF_ONOFF4.Focus()

                Case "WF_OUTPUTID5"         '情報出力ID5
                    WF_OUTPUTID5.Focus()

                Case "WF_ONOFF5"            '表示フラグ5
                    WF_ONOFF5.Focus()

                Case "WF_OUTPUTID6"         '情報出力ID6
                    WF_OUTPUTID6.Focus()

                Case "WF_ONOFF6"            '表示フラグ6
                    WF_ONOFF6.Focus()

                Case "WF_OUTPUTID7"         '情報出力ID7
                    WF_OUTPUTID7.Focus()

                Case "WF_ONOFF7"            '表示フラグ7
                    WF_ONOFF7.Focus()

                Case "WF_OUTPUTID8"         '情報出力ID8
                    WF_OUTPUTID8.Focus()

                Case "WF_ONOFF8"            '表示フラグ8
                    WF_ONOFF8.Focus()

                Case "WF_OUTPUTID9"         '情報出力ID9
                    WF_OUTPUTID9.Focus()

                Case "WF_ONOFF9"            '表示フラグ9
                    WF_ONOFF9.Focus()

                Case "WF_OUTPUTID10"        '情報出力ID10
                    WF_OUTPUTID10.Focus()

                Case "WF_ONOFF10"           '表示フラグ10
                    WF_ONOFF10.Focus()

                Case "WF_OUTPUTID11"        '情報出力ID11
                    WF_OUTPUTID11.Focus()

                Case "WF_ONOFF11"           '表示フラグ11
                    WF_ONOFF11.Focus()

                Case "WF_OUTPUTID12"        '情報出力ID12
                    WF_OUTPUTID12.Focus()

                Case "WF_ONOFF12"           '表示フラグ12
                    WF_ONOFF12.Focus()

                Case "WF_OUTPUTID13"        '情報出力ID13
                    WF_OUTPUTID13.Focus()

                Case "WF_ONOFF13"           '表示フラグ13
                    WF_ONOFF13.Focus()

                Case "WF_OUTPUTID14"        '情報出力ID14
                    WF_OUTPUTID14.Focus()

                Case "WF_ONOFF14"           '表示フラグ14
                    WF_ONOFF14.Focus()

                Case "WF_OUTPUTID15"        '情報出力ID15
                    WF_OUTPUTID15.Focus()

                Case "WF_ONOFF15"           '表示フラグ15
                    WF_ONOFF15.Focus()

                Case "WF_OUTPUTID16"        '情報出力ID16
                    WF_OUTPUTID16.Focus()

                Case "WF_ONOFF16"           '表示フラグ16
                    WF_ONOFF16.Focus()

                Case "WF_OUTPUTID17"        '情報出力ID17
                    WF_OUTPUTID17.Focus()

                Case "WF_ONOFF17"           '表示フラグ17
                    WF_ONOFF17.Focus()

                Case "WF_OUTPUTID18"        '情報出力ID18
                    WF_OUTPUTID18.Focus()

                Case "WF_ONOFF18"           '表示フラグ18
                    WF_ONOFF18.Focus()

                Case "WF_OUTPUTID19"        '情報出力ID19
                    WF_OUTPUTID19.Focus()

                Case "WF_ONOFF19"           '表示フラグ19
                    WF_ONOFF19.Focus()

                Case "WF_OUTPUTID20"        '情報出力ID20
                    WF_OUTPUTID20.Focus()

                Case "WF_ONOFF20"           '表示フラグ20
                    WF_ONOFF20.Focus()

                Case "WF_OUTPUTID21"        '情報出力ID21
                    WF_OUTPUTID21.Focus()

                Case "WF_ONOFF21"           '表示フラグ21
                    WF_ONOFF21.Focus()

                Case "WF_OUTPUTID22"        '情報出力ID22
                    WF_OUTPUTID22.Focus()

                Case "WF_ONOFF22"           '表示フラグ22
                    WF_ONOFF22.Focus()

                Case "WF_OUTPUTID23"        '情報出力ID23
                    WF_OUTPUTID23.Focus()

                Case "WF_ONOFF23"           '表示フラグ23
                    WF_ONOFF23.Focus()

                Case "WF_OUTPUTID24"        '情報出力ID24
                    WF_OUTPUTID24.Focus()

                Case "WF_ONOFF24"           '表示フラグ24
                    WF_ONOFF24.Focus()

                Case "WF_OUTPUTID25"        '情報出力ID25
                    WF_OUTPUTID25.Focus()

                Case "WF_ONOFF25"           '表示フラグ25
                    WF_ONOFF25.Focus()
            End Select
        Else
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

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

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LINE_ERR As String = ""
        Dim WW_TEXT As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCHECKERR As String = ""
        Dim WW_CS0024FCHECKREPORT As String = ""
        Dim dateErrFlag As String = ""
        Dim WW_UniqueKeyCHECK As String = ""

        '○ 画面操作権限チェック
        '権限チェック(操作者がデータ内USERの更新権限があるかチェック
        '　※権限判定時点：現在
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If isNormal(CS0025AUTHORget.ERR) AndAlso CS0025AUTHORget.PERMITCODE = C_PERMISSION.UPDATE Then
        Else
            WW_CheckMES1 = "・更新できないレコード(ユーザ更新権限なし)です。"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LINE_ERR = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", OIS0001INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIS0001INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・削除コード入力エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'ユーザID(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "USERID", OIS0001INProw("USERID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'CODENAME_get("USERID", OIS0001INProw("USERID"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(ユーザID入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(ユーザID入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '社員名（短）(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STAFFNAMES", OIS0001INProw("STAFFNAMES"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'CODENAME_get("STAFFNAMES", OIS0001INProw("STAFFNAMES"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(社員名（短）入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(社員名（短）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '社員名（長）(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STAFFNAMEL", OIS0001INProw("STAFFNAMEL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'CODENAME_get("STAFFNAMEL", OIS0001INProw("STAFFNAMEL"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(社員名（長）入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(社員名（長）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '誤り回数(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MISSCNT", OIS0001INProw("MISSCNT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'If OIS0001INProw("MISSCNT") <> "" Then
                'CODENAME_get("MISSCNT", OIS0001INProw("MISSCNT"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(誤り回数入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(誤り回数入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'パスワード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PASSWORD", OIS0001INProw("PASSWORD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'CODENAME_get("PASSWORD", OIS0001INProw("PASSWORD"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(パスワード入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(パスワード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'パスワード有効期限(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PASSENDYMD", OIS0001INProw("PASSENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIS0001INProw("PASSENDYMD"), "パスワード有効期限", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(パスワード有効期限エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIS0001INProw("PASSENDYMD") = CDate(OIS0001INProw("PASSENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(パスワード有効期限エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '開始年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STYMD", OIS0001INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIS0001INProw("STYMD"), "開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIS0001INProw("STYMD") = CDate(OIS0001INProw("STYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ENDYMD", OIS0001INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIS0001INProw("ENDYMD"), "終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIS0001INProw("ENDYMD") = CDate(OIS0001INProw("ENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CAMPCODE", OIS0001INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("CAMPCODE", OIS0001INProw("CAMPCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(会社コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(会社コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORG", OIS0001INProw("ORG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("ORG", OIS0001INProw("ORG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(組織コード入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(組織コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'メールアドレス(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "EMAIL", OIS0001INProw("EMAIL"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'CODENAME_get("EMAIL", OIS0001INProw("EMAIL"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(メールアドレス入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(メールアドレス入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'メニュー表示制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MENUROLE", OIS0001INProw("MENUROLE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("MENU", OIS0001INProw("MENUROLE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(メニュー表示制御ロール入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(メニュー表示制御ロール入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面参照更新制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MAPROLE", OIS0001INProw("MAPROLE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("MAP", OIS0001INProw("MAPROLE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(画面参照更新制御ロール入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面参照更新制御ロール入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面表示項目制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "VIEWPROFID", OIS0001INProw("VIEWPROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("VIEW", OIS0001INProw("VIEWPROFID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(画面表示項目制御ロール入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面表示項目制御ロール入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'エクセル出力制御ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "RPRTPROFID", OIS0001INProw("RPRTPROFID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("XML", OIS0001INProw("RPRTPROFID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(エクセル出力制御ロール入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(エクセル出力制御ロール入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面初期値ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "VARIANT", OIS0001INProw("VARIANT"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'If OIS0001INProw("VARIANT") <> "" Then
                'CODENAME_get("VARIANT", OIS0001INProw("VARIANT"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(画面初期値ロール入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(画面初期値ロール入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '承認権限ロール(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "APPROVALID", OIS0001INProw("APPROVALID"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("APPROVAL", OIS0001INProw("APPROVALID"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(承認権限ロール入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(承認権限ロール入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            Dim DuplicateCheckList_OUTPUTID As New ArrayList()
            Dim DuplicateCheckList_SORTNO As New ArrayList()

            For n As Integer = 1 To 25

                Dim fieldName_OUTPUTIDn As String = "OUTPUTID" & n.ToString()
                Dim fieldName_ONOFFn As String = "ONOFF" & n.ToString()
                Dim fieldName_SORTNOn As String = "SORTNO" & n.ToString()

                '情報出力IDn(バリデーションチェック）
                Master.CheckField(Master.USERCAMP, fieldName_OUTPUTIDn, OIS0001INProw(fieldName_OUTPUTIDn), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '値存在チェック
                    CODENAME_get("OUTPUTID", OIS0001INProw(fieldName_OUTPUTIDn), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(" & "情報出力ID" & n.ToString() & "入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(" & "情報出力ID" & n.ToString() & "入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit For
                End If

                '表示フラグn(バリデーションチェック）
                Master.CheckField(Master.USERCAMP, fieldName_ONOFFn, OIS0001INProw(fieldName_ONOFFn), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    '値存在チェック
                    CODENAME_get("ONOFF", OIS0001INProw(fieldName_ONOFFn), WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(" & "表示フラグ" & n.ToString() & "入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(" & "表示フラグ" & n.ToString() & "入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit For
                End If

                '表示順n(バリデーションチェック）
                Dim inputText As String = OIS0001INProw(fieldName_SORTNOn)
                Master.CheckField(Master.USERCAMP, fieldName_SORTNOn, OIS0001INProw(fieldName_SORTNOn), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "・更新できないレコード(" & "表示順" & n.ToString() & "入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Exit For
                Else
                    'CheckFieldで数値が前0埋めされてしまうので、数値が一致してれば元に戻す
                    '(そうしないと既存レコードの比較がうまくいかない為)
                    If String.IsNullOrEmpty(inputText) Then
                        OIS0001INProw(fieldName_SORTNOn) = 0
                    Else
                        If Not inputText.Equals(OIS0001INProw(fieldName_SORTNOn)) AndAlso
                            Long.Parse(inputText) = Long.Parse(OIS0001INProw(fieldName_SORTNOn)) Then
                            OIS0001INProw(fieldName_SORTNOn) = inputText
                        End If
                    End If
                End If

                '重複チェック
                If OIS0001INProw(fieldName_ONOFFn) = "1" Then

                    '情報出力IDn
                    If DuplicateCheckList_OUTPUTID.IndexOf(OIS0001INProw(fieldName_OUTPUTIDn)) >= 0 Then
                        WW_CheckMES1 = "・更新できないレコード(" & "情報出力ID" & n.ToString() & "重複エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    Else
                        DuplicateCheckList_OUTPUTID.Add(OIS0001INProw(fieldName_OUTPUTIDn))
                    End If

                    '表示順n
                    If DuplicateCheckList_OUTPUTID.IndexOf(OIS0001INProw(fieldName_SORTNOn)) >= 0 Then
                        WW_CheckMES1 = "・更新できないレコード(" & "表示順" & n.ToString() & "重複エラー)です。"
                        WW_CheckMES2 = WW_CS0024FCHECKREPORT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Exit For
                    Else
                        DuplicateCheckList_OUTPUTID.Add(OIS0001INProw(fieldName_SORTNOn))
                    End If

                End If

            Next

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIS0001INProw("USERID") = work.WF_SEL_USERID.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（ユーザID & 開始年月日）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIS0001INProw("USERID") & "]" &
                                       " [" & OIS0001INProw("STYMD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIS0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIS0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 年月日チェック
    ''' </summary>
    ''' <param name="I_DATE"></param>
    ''' <param name="I_DATENAME"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

        dateErrFlag = "1"
        Try
            '年取得
            Dim chkLeapYear As String = I_DATE.Substring(0, 4)
            '月日を取得
            Dim getMMDD As String = I_DATE.Remove(0, I_DATE.IndexOf("/") + 1)
            '月取得
            Dim getMonth As String = getMMDD.Remove(getMMDD.IndexOf("/"))
            '日取得
            Dim getDay As String = getMMDD.Remove(0, getMMDD.IndexOf("/") + 1)

            '閏年の場合はその旨のメッセージを出力
            If Not DateTime.IsLeapYear(chkLeapYear) _
            AndAlso (getMonth = "2" OrElse getMonth = "02") AndAlso getDay = "29" Then
                Master.Output(C_MESSAGE_NO.OIL_LEAPYEAR_NOTFOUND, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                '月と日の範囲チェック
            ElseIf getMonth >= 13 OrElse getDay >= 32 Then
                Master.Output(C_MESSAGE_NO.OIL_MONTH_DAY_OVER_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
            Else
                'Master.Output(I_VALUE, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
                'エラーなし
                dateErrFlag = "0"
            End If
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DATE_FORMAT_ERROR, C_MESSAGE_TYPE.ERR, I_DATENAME, needsPopUp:=True)
        End Try

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIS0001row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIS0001row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIS0001row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> ユーザID =" & OIS0001row("USERID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 社員名（短） =" & OIS0001row("STAFFNAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 社員名（長） =" & OIS0001row("STAFFNAMEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面ＩＤ =" & OIS0001row("MAPID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> パスワード =" & OIS0001row("PASSWORD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 誤り回数 =" & OIS0001row("MISSCNT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> パスワード有効期限 =" & OIS0001row("PASSENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日 =" & OIS0001row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日 =" & OIS0001row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード =" & OIS0001row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織コード =" & OIS0001row("ORG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> メールアドレス =" & OIS0001row("EMAIL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> メニュー表示制御ロール =" & OIS0001row("MENUROLE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面参照更新制御ロール =" & OIS0001row("MAPROLE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面表示項目制御ロール =" & OIS0001row("VIEWPROFID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> エクセル出力制御ロール =" & OIS0001row("RPRTPROFID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 画面初期値ロール =" & OIS0001row("VARIANT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 承認権限ロール =" & OIS0001row("APPROVALID") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID1 =" & OIS0001row("OUTPUTID1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ1 =" & OIS0001row("ONOFF1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順1 =" & OIS0001row("SORTNO1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID2 =" & OIS0001row("OUTPUTID2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ2 =" & OIS0001row("ONOFF2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順2 =" & OIS0001row("SORTNO2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID3 =" & OIS0001row("OUTPUTID3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ3 =" & OIS0001row("ONOFF3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順3 =" & OIS0001row("SORTNO3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID4 =" & OIS0001row("OUTPUTID4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ4 =" & OIS0001row("ONOFF4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順4 =" & OIS0001row("SORTNO4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID5 =" & OIS0001row("OUTPUTID5") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ5 =" & OIS0001row("ONOFF5") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順5 =" & OIS0001row("SORTNO5") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID6 =" & OIS0001row("OUTPUTID6") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ6 =" & OIS0001row("ONOFF6") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順6 =" & OIS0001row("SORTNO6") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID7 =" & OIS0001row("OUTPUTID7") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ7 =" & OIS0001row("ONOFF7") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順7 =" & OIS0001row("SORTNO7") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID8 =" & OIS0001row("OUTPUTID8") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ8 =" & OIS0001row("ONOFF8") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順8 =" & OIS0001row("SORTNO8") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID9 =" & OIS0001row("OUTPUTID9") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ9 =" & OIS0001row("ONOFF9") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順9 =" & OIS0001row("SORTNO9") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID10 =" & OIS0001row("OUTPUTID10") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ10 =" & OIS0001row("ONOFF10") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順10 =" & OIS0001row("SORTNO10") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID11 =" & OIS0001row("OUTPUTID11") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ11 =" & OIS0001row("ONOFF11") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順11 =" & OIS0001row("SORTNO11") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID12 =" & OIS0001row("OUTPUTID12") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ12 =" & OIS0001row("ONOFF12") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順12 =" & OIS0001row("SORTNO12") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID13 =" & OIS0001row("OUTPUTID13") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ13 =" & OIS0001row("ONOFF13") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順13 =" & OIS0001row("SORTNO13") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID14 =" & OIS0001row("OUTPUTID14") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ14 =" & OIS0001row("ONOFF14") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順14 =" & OIS0001row("SORTNO14") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID15 =" & OIS0001row("OUTPUTID15") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ15 =" & OIS0001row("ONOFF15") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順15 =" & OIS0001row("SORTNO15") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID16 =" & OIS0001row("OUTPUTID16") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ16 =" & OIS0001row("ONOFF16") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順16 =" & OIS0001row("SORTNO16") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID17 =" & OIS0001row("OUTPUTID17") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ17 =" & OIS0001row("ONOFF17") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順17 =" & OIS0001row("SORTNO17") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID18 =" & OIS0001row("OUTPUTID18") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ18 =" & OIS0001row("ONOFF18") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順18 =" & OIS0001row("SORTNO18") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID19 =" & OIS0001row("OUTPUTID19") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ19 =" & OIS0001row("ONOFF19") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順19 =" & OIS0001row("SORTNO19") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID20 =" & OIS0001row("OUTPUTID20") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ20 =" & OIS0001row("ONOFF20") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順20 =" & OIS0001row("SORTNO20") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID21 =" & OIS0001row("OUTPUTID21") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ21 =" & OIS0001row("ONOFF21") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順21 =" & OIS0001row("SORTNO21") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID22 =" & OIS0001row("OUTPUTID22") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ22 =" & OIS0001row("ONOFF22") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順22 =" & OIS0001row("SORTNO22") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID23 =" & OIS0001row("OUTPUTID23") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ23 =" & OIS0001row("ONOFF23") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順23 =" & OIS0001row("SORTNO23") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID24 =" & OIS0001row("OUTPUTID24") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ24 =" & OIS0001row("ONOFF24") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順24 =" & OIS0001row("SORTNO24") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 情報出力ID25 =" & OIS0001row("OUTPUTID25") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示フラグ25 =" & OIS0001row("ONOFF25") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 表示順25 =" & OIS0001row("SORTNO25") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIS0001row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIS0001tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIS0001tbl_UPD()

        '○ 画面状態設定
        For Each OIS0001row As DataRow In OIS0001tbl.Rows
            Select Case OIS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIS0001INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIS0001row As DataRow In OIS0001tbl.Rows
                ' KEY項目が等しい時
                If OIS0001row("USERID") = OIS0001INProw("USERID") AndAlso
                    OIS0001row("STYMD") = OIS0001INProw("STYMD") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIS0001row("DELFLG") = OIS0001INProw("DELFLG") AndAlso
                        OIS0001row("STAFFNAMES") = OIS0001INProw("STAFFNAMES") AndAlso
                        OIS0001row("STAFFNAMEL") = OIS0001INProw("STAFFNAMEL") AndAlso
                        OIS0001row("MAPID") = OIS0001INProw("MAPID") AndAlso
                        OIS0001row("PASSWORD") = OIS0001INProw("PASSWORD") AndAlso
                        OIS0001row("MISSCNT") = OIS0001INProw("MISSCNT") AndAlso
                        OIS0001row("PASSENDYMD") = OIS0001INProw("PASSENDYMD") AndAlso
                        OIS0001row("ENDYMD") = OIS0001INProw("ENDYMD") AndAlso
                        OIS0001row("CAMPCODE") = OIS0001INProw("CAMPCODE") AndAlso
                        OIS0001row("ORG") = OIS0001INProw("ORG") AndAlso
                        OIS0001row("EMAIL") = OIS0001INProw("EMAIL") AndAlso
                        OIS0001row("MENUROLE") = OIS0001INProw("MENUROLE") AndAlso
                        OIS0001row("MAPROLE") = OIS0001INProw("MAPROLE") AndAlso
                        OIS0001row("VIEWPROFID") = OIS0001INProw("VIEWPROFID") AndAlso
                        OIS0001row("RPRTPROFID") = OIS0001INProw("RPRTPROFID") AndAlso
                        OIS0001row("VARIANT") = OIS0001INProw("VARIANT") AndAlso
                        OIS0001row("APPROVALID") = OIS0001INProw("APPROVALID") AndAlso
                        OIS0001row("OUTPUTID1") = OIS0001INProw("OUTPUTID1") AndAlso
                        OIS0001row("ONOFF1") = OIS0001INProw("ONOFF1") AndAlso
                        OIS0001row("SORTNO1") = OIS0001INProw("SORTNO1") AndAlso
                        OIS0001row("OUTPUTID2") = OIS0001INProw("OUTPUTID2") AndAlso
                        OIS0001row("ONOFF2") = OIS0001INProw("ONOFF2") AndAlso
                        OIS0001row("SORTNO2") = OIS0001INProw("SORTNO2") AndAlso
                        OIS0001row("OUTPUTID3") = OIS0001INProw("OUTPUTID3") AndAlso
                        OIS0001row("ONOFF3") = OIS0001INProw("ONOFF3") AndAlso
                        OIS0001row("SORTNO3") = OIS0001INProw("SORTNO3") AndAlso
                        OIS0001row("OUTPUTID4") = OIS0001INProw("OUTPUTID4") AndAlso
                        OIS0001row("ONOFF4") = OIS0001INProw("ONOFF4") AndAlso
                        OIS0001row("SORTNO4") = OIS0001INProw("SORTNO4") AndAlso
                        OIS0001row("OUTPUTID5") = OIS0001INProw("OUTPUTID5") AndAlso
                        OIS0001row("ONOFF5") = OIS0001INProw("ONOFF5") AndAlso
                        OIS0001row("SORTNO5") = OIS0001INProw("SORTNO5") AndAlso
                        OIS0001row("OUTPUTID6") = OIS0001INProw("OUTPUTID6") AndAlso
                        OIS0001row("ONOFF6") = OIS0001INProw("ONOFF6") AndAlso
                        OIS0001row("SORTNO6") = OIS0001INProw("SORTNO6") AndAlso
                        OIS0001row("OUTPUTID7") = OIS0001INProw("OUTPUTID7") AndAlso
                        OIS0001row("ONOFF7") = OIS0001INProw("ONOFF7") AndAlso
                        OIS0001row("SORTNO7") = OIS0001INProw("SORTNO7") AndAlso
                        OIS0001row("OUTPUTID8") = OIS0001INProw("OUTPUTID8") AndAlso
                        OIS0001row("ONOFF8") = OIS0001INProw("ONOFF8") AndAlso
                        OIS0001row("SORTNO8") = OIS0001INProw("SORTNO8") AndAlso
                        OIS0001row("OUTPUTID9") = OIS0001INProw("OUTPUTID9") AndAlso
                        OIS0001row("ONOFF9") = OIS0001INProw("ONOFF9") AndAlso
                        OIS0001row("SORTNO9") = OIS0001INProw("SORTNO9") AndAlso
                        OIS0001row("OUTPUTID10") = OIS0001INProw("OUTPUTID10") AndAlso
                        OIS0001row("ONOFF10") = OIS0001INProw("ONOFF10") AndAlso
                        OIS0001row("SORTNO10") = OIS0001INProw("SORTNO10") AndAlso
                        OIS0001row("OUTPUTID11") = OIS0001INProw("OUTPUTID11") AndAlso
                        OIS0001row("ONOFF11") = OIS0001INProw("ONOFF11") AndAlso
                        OIS0001row("SORTNO11") = OIS0001INProw("SORTNO11") AndAlso
                        OIS0001row("OUTPUTID12") = OIS0001INProw("OUTPUTID12") AndAlso
                        OIS0001row("ONOFF12") = OIS0001INProw("ONOFF12") AndAlso
                        OIS0001row("SORTNO12") = OIS0001INProw("SORTNO12") AndAlso
                        OIS0001row("OUTPUTID13") = OIS0001INProw("OUTPUTID13") AndAlso
                        OIS0001row("ONOFF13") = OIS0001INProw("ONOFF13") AndAlso
                        OIS0001row("SORTNO13") = OIS0001INProw("SORTNO13") AndAlso
                        OIS0001row("OUTPUTID14") = OIS0001INProw("OUTPUTID14") AndAlso
                        OIS0001row("ONOFF14") = OIS0001INProw("ONOFF14") AndAlso
                        OIS0001row("SORTNO14") = OIS0001INProw("SORTNO14") AndAlso
                        OIS0001row("OUTPUTID15") = OIS0001INProw("OUTPUTID15") AndAlso
                        OIS0001row("ONOFF15") = OIS0001INProw("ONOFF15") AndAlso
                        OIS0001row("SORTNO15") = OIS0001INProw("SORTNO15") AndAlso
                        OIS0001row("OUTPUTID16") = OIS0001INProw("OUTPUTID16") AndAlso
                        OIS0001row("ONOFF16") = OIS0001INProw("ONOFF16") AndAlso
                        OIS0001row("SORTNO16") = OIS0001INProw("SORTNO16") AndAlso
                        OIS0001row("OUTPUTID17") = OIS0001INProw("OUTPUTID17") AndAlso
                        OIS0001row("ONOFF17") = OIS0001INProw("ONOFF17") AndAlso
                        OIS0001row("SORTNO17") = OIS0001INProw("SORTNO17") AndAlso
                        OIS0001row("OUTPUTID18") = OIS0001INProw("OUTPUTID18") AndAlso
                        OIS0001row("ONOFF18") = OIS0001INProw("ONOFF18") AndAlso
                        OIS0001row("SORTNO18") = OIS0001INProw("SORTNO18") AndAlso
                        OIS0001row("OUTPUTID19") = OIS0001INProw("OUTPUTID19") AndAlso
                        OIS0001row("ONOFF19") = OIS0001INProw("ONOFF19") AndAlso
                        OIS0001row("SORTNO19") = OIS0001INProw("SORTNO19") AndAlso
                        OIS0001row("OUTPUTID20") = OIS0001INProw("OUTPUTID20") AndAlso
                        OIS0001row("ONOFF20") = OIS0001INProw("ONOFF20") AndAlso
                        OIS0001row("SORTNO20") = OIS0001INProw("SORTNO20") AndAlso
                        OIS0001row("OUTPUTID21") = OIS0001INProw("OUTPUTID21") AndAlso
                        OIS0001row("ONOFF21") = OIS0001INProw("ONOFF21") AndAlso
                        OIS0001row("SORTNO21") = OIS0001INProw("SORTNO21") AndAlso
                        OIS0001row("OUTPUTID22") = OIS0001INProw("OUTPUTID22") AndAlso
                        OIS0001row("ONOFF22") = OIS0001INProw("ONOFF22") AndAlso
                        OIS0001row("SORTNO22") = OIS0001INProw("SORTNO22") AndAlso
                        OIS0001row("OUTPUTID23") = OIS0001INProw("OUTPUTID23") AndAlso
                        OIS0001row("ONOFF23") = OIS0001INProw("ONOFF23") AndAlso
                        OIS0001row("SORTNO23") = OIS0001INProw("SORTNO23") AndAlso
                        OIS0001row("OUTPUTID24") = OIS0001INProw("OUTPUTID24") AndAlso
                        OIS0001row("ONOFF24") = OIS0001INProw("ONOFF24") AndAlso
                        OIS0001row("SORTNO24") = OIS0001INProw("SORTNO24") AndAlso
                        OIS0001row("OUTPUTID25") = OIS0001INProw("OUTPUTID25") AndAlso
                        OIS0001row("ONOFF25") = OIS0001INProw("ONOFF25") AndAlso
                        OIS0001row("SORTNO25") = OIS0001INProw("SORTNO25") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIS0001row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIS0001INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIS0001INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIS0001INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIS0001INPtbl.Rows(0)("OPERATION")) Then
            '追加/更新の場合、DB更新処理
            Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                'DataBase接続
                SQLcon.Open()

                'マスタ更新
                UpdateMaster(SQLcon)

                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定　&　入力値反映
        For Each OIS0001INProw As DataRow In OIS0001INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            For Each OIS0001row As DataRow In OIS0001tbl.Rows

                '同一レコードか判定
                If OIS0001INProw("USERID") = OIS0001row("USERID") AndAlso
                    OIS0001INProw("STYMD") = OIS0001row("STYMD") Then
                    '画面入力テーブル項目設定
                    OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                    OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                    OIS0001INProw("SELECT") = 0
                    OIS0001INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIS0001row.ItemArray = OIS0001INProw.ItemArray

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIS0001tbl.NewRow
                nrow.ItemArray = OIS0001INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIS0001tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("UPDTIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                OIS0001tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIS0001INProw As DataRow)

        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '同一レコードか判定
            If OIS0001INProw("USERID") = OIS0001row("USERID") AndAlso
                OIS0001INProw("STYMD") = OIS0001row("STYMD") Then
                '画面入力テーブル項目設定
                OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                OIS0001INProw("SELECT") = 1
                OIS0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIS0001row.ItemArray = OIS0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIS0001INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIS0001row As DataRow = OIS0001tbl.NewRow
        OIS0001row.ItemArray = OIS0001INProw.ItemArray

        OIS0001row("LINECNT") = OIS0001tbl.Rows.Count + 1
        If OIS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIS0001row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIS0001row("UPDTIMSTP") = "0"
        OIS0001row("SELECT") = 1
        OIS0001row("HIDDEN") = 0

        OIS0001tbl.Rows.Add(OIS0001row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIS0001INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIS0001INProw As DataRow)

        For Each OIS0001row As DataRow In OIS0001tbl.Rows

            '同一レコードか判定
            If OIS0001INProw("USERID") = OIS0001row("USERID") AndAlso
                OIS0001INProw("STYMD") = OIS0001row("STYMD") Then
                '画面入力テーブル項目設定
                OIS0001INProw("LINECNT") = OIS0001row("LINECNT")
                OIS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIS0001INProw("UPDTIMSTP") = OIS0001row("UPDTIMSTP")
                OIS0001INProw("SELECT") = 1
                OIS0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIS0001row.ItemArray = OIS0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub
#End Region

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
                    If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    Else
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ROLE
                    End If
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "ORG"         '組織コード
                    Dim AUTHORITYALL_FLG As String = "0"
                    If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                        If WF_CAMPCODE.Text = "" Then '会社コードが空の場合
                            AUTHORITYALL_FLG = "1"
                        Else '会社コードに入力済みの場合
                            AUTHORITYALL_FLG = "2"
                        End If
                    End If
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE3.Text, AUTHORITYALL_FLG)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "MENU"           'メニュー表示制御ロール
                    prmData = work.CreateRoleList(WF_CAMPCODE.Text, I_FIELD)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "MAP"         '画面参照更新制御ロール
                    prmData = work.CreateRoleList(WF_CAMPCODE.Text, I_FIELD)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "VIEW"         '画面表示項目制御ロール
                    prmData = work.CreateRoleList(WF_CAMPCODE.Text, I_FIELD)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "XML"         'エクセル出力制御ロール
                    prmData = work.CreateRoleList(WF_CAMPCODE.Text, I_FIELD)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "APPROVAL"         '承認権限ロール
                    prmData = work.CreateRoleList(WF_CAMPCODE.Text, I_FIELD)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "OUTPUTID"         '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))

                Case "ONOFF"            '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))

                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

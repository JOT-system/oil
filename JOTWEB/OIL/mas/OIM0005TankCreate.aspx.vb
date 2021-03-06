﻿''************************************************************
' タンク車マスタメンテ登録画面
' 作成日 2019/11/08
' 更新日 2021/06/21
' 作成者 JOT遠藤
' 更新者 JOT伊草
'
' 修正履歴:2019/11/08 新規作成
'         :2021/04/13 1)表更新→更新、クリア→戻る、に名称変更
'                     2)戻るボタン押下時、確認ダイアログ表示→
'                       確認ダイアログでOK押下時、一覧画面に戻るように修正
'                     3)更新ボタン押下時、この画面でDB更新→
'                       一覧画面の表示データに更新後の内容反映して戻るように修正
'         :2021/04/14 DB更新後に一覧画面に戻る場合に
'                     コード名称が表示されなくなるバグを修正
'         :2021/04/15 新規登録を行った際に、一覧画面に新規登録データが追加されないバグに対応
'         :2021/05/11 1)入力項目「油種中分類」「中間点検年月」「中間点検場所」「中間点検実施者」
'         :             「自主点検年月」「自主点検場所」「自主点検実施者」を追加
'         :2021/05/18 1)項目「点検実施者(社員名)」を追加
'         :2021/06/21 1)項目「運用基地（サブ）」「削除理由区分」「全検計画年月」
'         :             「休車フラグ」「休車日」「取得価格」「内部塗装」
'         :             「安全弁」「センターバルブ情報」を追加
'         :             項目名称変更「請負リース区分」→「請負請負リース区分」
'         :           2)「戻る：ボタン押下時に確認ダイアログを表示しないように修正
'         :2021/06/28 1)DB更新処理の設定項目に以下の誤りがあったのを修正
'         :             「取得年月日」に「次回全検年月日」が設定されていた
'         :             「取得名」に「取得年月日」が設定されていた
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' タンク車マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class OIM0005TankCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0005tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0005INPtbl As DataTable                              'チェック用テーブル
    Private OIM0005UPDtbl As DataTable                              '更新用テーブル

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
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '表更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 'クリアボタン押下
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
                            'Case "btnClearConfirmOk"        '戻るボタン押下後の確認ダイアログでOK押下
                            '    WF_CLEAR_ConfirmOkClick()
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
            If Not IsNothing(OIM0005tbl) Then
                OIM0005tbl.Clear()
                OIM0005tbl.Dispose()
                OIM0005tbl = Nothing
            End If

            If Not IsNothing(OIM0005INPtbl) Then
                OIM0005INPtbl.Clear()
                OIM0005INPtbl.Dispose()
                OIM0005INPtbl = Nothing
            End If

            If Not IsNothing(OIM0005UPDtbl) Then
                OIM0005UPDtbl.Clear()
                OIM0005UPDtbl.Dispose()
                OIM0005UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0005WRKINC.MAPIDC
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
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '項目イベントハンドラ設定
        WF_GETDATE.Attributes.Add("onblur", "getDateBlur(this)")

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0005L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        'JOT車番
        WF_TANKNUMBER.Text = work.WF_SEL_TANKNUMBER2.Text

        '形式
        WF_MODEL.Text = work.WF_SEL_MODEL2.Text

        '形式カナ
        WF_MODELKANA.Text = work.WF_SEL_MODELKANA.Text

        '荷重
        WF_LOAD.Text = work.WF_SEL_LOAD.Text

        '荷重単位
        WF_LOADUNIT.Text = work.WF_SEL_LOADUNIT.Text
        CODENAME_get("UNIT", WF_LOADUNIT.Text, WF_LOADUNIT_TEXT.Text, WW_RTN_SW)

        '容積
        WF_VOLUME.Text = work.WF_SEL_VOLUME.Text

        '容積単位
        WF_VOLUMEUNIT.Text = work.WF_SEL_VOLUMEUNIT.Text
        CODENAME_get("UNIT", WF_VOLUMEUNIT.Text, WF_VOLUMEUNIT_TEXT.Text, WW_RTN_SW)

        '自重
        WF_MYWEIGHT.Text = work.WF_SEL_MYWEIGHT.Text

        'タンク車長
        WF_LENGTH.Text = work.WF_SEL_LENGTH.Text

        'タンク車体長
        WF_TANKLENGTH.Text = work.WF_SEL_TANKLENGTH.Text

        '最大口径
        WF_MAXCALIBER.Text = work.WF_SEL_MAXCALIBER.Text

        '最小口径
        WF_MINCALIBER.Text = work.WF_SEL_MINCALIBER.Text

        '長さフラグ
        WF_LENGTHFLG.Text = work.WF_SEL_LENGTHFLG2.Text
        CODENAME_get("LENGTHFLG", WF_LENGTHFLG.Text, WF_LENGTHFLG_TEXT.Text, WW_RTN_SW)

        '原籍所有者C, 原籍所有者
        WF_ORIGINOWNERCODE.Text = work.WF_SEL_ORIGINOWNERCODE.Text
        CODENAME_get("ORIGINOWNERCODE", WF_ORIGINOWNERCODE.Text, WF_ORIGINOWNERCODE_TEXT.Text, WW_RTN_SW)

        '名義所有者C, 名義所有者
        WF_OWNERCODE.Text = work.WF_SEL_OWNERCODE.Text
        CODENAME_get("ORIGINOWNERCODE", WF_OWNERCODE.Text, WF_OWNERCODE_TEXT.Text, WW_RTN_SW)

        'リース先C, リース先
        WF_LEASECODE.Text = work.WF_SEL_LEASECODE.Text
        CODENAME_get("CAMPCODE", WF_LEASECODE.Text, WF_LEASECODE_TEXT.Text, WW_RTN_SW)

        '請負リース区分C, リース区分
        WF_LEASECLASS.Text = work.WF_SEL_LEASECLASS.Text
        CODENAME_get("LEASECLASS", WF_LEASECLASS.Text, WF_LEASECLASS_TEXT.Text, WW_RTN_SW)

        '自動延長, 自動延長名
        WF_AUTOEXTENTION.Text = work.WF_SEL_AUTOEXTENTION.Text
        CODENAME_get("AUTOEXTENTION", WF_AUTOEXTENTION.Text, WF_AUTOEXTENTION_TEXT.Text, WW_RTN_SW)

        'リース開始年月日
        WF_LEASESTYMD.Text = work.WF_SEL_LEASESTYMD.Text

        'リース満了年月日
        WF_LEASEENDYMD.Text = work.WF_SEL_LEASEENDYMD.Text

        '第三者使用者C, 第三者使用者
        WF_USERCODE.Text = work.WF_SEL_USERCODE.Text
        CODENAME_get("USERCODE", WF_USERCODE.Text, WF_USERCODE_TEXT.Text, WW_RTN_SW)

        '原常備駅C, 原常備駅
        WF_CURRENTSTATIONCODE.Text = work.WF_SEL_CURRENTSTATIONCODE.Text
        CODENAME_get("STATIONPATTERN", WF_CURRENTSTATIONCODE.Text, WF_CURRENTSTATIONCODE_TEXT.Text, WW_RTN_SW)

        '臨時常備駅C, 臨時常備駅
        WF_EXTRADINARYSTATIONCODE.Text = work.WF_SEL_EXTRADINARYSTATIONCODE.Text
        CODENAME_get("STATIONPATTERN", WF_EXTRADINARYSTATIONCODE.Text, WF_EXTRADINARYSTATIONCODE_TEXT.Text, WW_RTN_SW)

        '第三者使用期限
        WF_USERLIMIT.Text = work.WF_SEL_USERLIMIT.Text

        '臨時常備駅期限
        WF_LIMITTEXTRADIARYSTATION.Text = work.WF_SEL_LIMITTEXTRADIARYSTATION.Text

        '原専用種別C, 原専用種別
        WF_DEDICATETYPECODE.Text = work.WF_SEL_DEDICATETYPECODE.Text
        CODENAME_get("DEDICATETYPECODE", WF_DEDICATETYPECODE.Text, WF_DEDICATETYPECODE_TEXT.Text, WW_RTN_SW)

        '臨時専用種別C, 臨時専用種別
        WF_EXTRADINARYTYPECODE.Text = work.WF_SEL_EXTRADINARYTYPECODE.Text
        CODENAME_get("EXTRADINARYTYPECODE", WF_EXTRADINARYTYPECODE.Text, WF_EXTRADINARYTYPECODE_TEXT.Text, WW_RTN_SW)

        '臨時専用期限
        WF_EXTRADINARYLIMIT.Text = work.WF_SEL_EXTRADINARYLIMIT.Text

        '油種大分類コード, 油種大分類名
        WF_BIGOILCODE.Text = work.WF_SEL_BIGOILCODE.Text
        CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)

        '油種中分類コード, 油種大分類名
        WF_MIDDLEOILCODE.Text = work.WF_SEL_MIDDLEOILCODE.Text
        CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)

        '運用基地C, 運用場所
        WF_OPERATIONBASECODE.Text = work.WF_SEL_OPERATIONBASECODE.Text
        CODENAME_get("BASE", WF_OPERATIONBASECODE.Text, WF_OPERATIONBASECODE_TEXT.Text, WW_RTN_SW)

        '運用基地C（サブ）, 運用場所（サブ）
        WF_SUBOPERATIONBASECODE.Text = work.WF_SEL_SUBOPERATIONBASECODE.Text
        CODENAME_get("BASE", WF_SUBOPERATIONBASECODE.Text, WF_SUBOPERATIONBASECODE_TEXT.Text, WW_RTN_SW)

        '塗色C, 塗色
        WF_COLORCODE.Text = work.WF_SEL_COLORCODE.Text
        CODENAME_get("COLORCODE", WF_COLORCODE.Text, WF_COLORCODE_TEXT.Text, WW_RTN_SW)

        'マークコード, マーク名
        WF_MARKCODE.Text = work.WF_SEL_MARKCODE.Text
        CODENAME_get("MARKCODE", WF_MARKCODE.Text, WF_MARKCODE_TEXT.Text, WW_RTN_SW)

        'JXTG仙台タグコード（とりあえず自由入力）
        WF_JXTGTAGCODE1.Text = work.WF_SEL_JXTGTAGCODE1.Text
        'CODENAME_get("JXTGTAGCODE1", WF_JXTGTAGCODE1.Text, WF_JXTGTAGCODE1_TEXT.Text, WW_RTN_SW)

        'JXTG千葉タグコード
        WF_JXTGTAGCODE2.Text = work.WF_SEL_JXTGTAGCODE2.Text
        CODENAME_get("TAGCODE", WF_JXTGTAGCODE2.Text, WF_JXTGTAGCODE2_TEXT.Text, WW_RTN_SW)

        'JXTG川崎タグコード（とりあえず自由入力）
        WF_JXTGTAGCODE3.Text = work.WF_SEL_JXTGTAGCODE3.Text
        'CODENAME_get("JXTGTAGCODE3", WF_JXTGTAGCODE3.Text, WF_JXTGTAGCODE3_TEXT.Text, WW_RTN_SW)

        'JXTG根岸タグコード（とりあえず自由入力）
        WF_JXTGTAGCODE4.Text = work.WF_SEL_JXTGTAGCODE4.Text
        'CODENAME_get("JXTGTAGCODE4", WF_JXTGTAGCODE4.Text, WF_JXTGTAGCODE4_TEXT.Text, WW_RTN_SW)

        '出光昭シタグコード
        WF_IDSSTAGCODE.Text = work.WF_SEL_IDSSTAGCODE.Text
        CODENAME_get("TAGCODE", WF_IDSSTAGCODE.Text, WF_IDSSTAGCODE_TEXT.Text, WW_RTN_SW)

        'コスモタグコード（とりあえず自由入力）
        WF_COSMOTAGCODE.Text = work.WF_SEL_COSMOTAGCODE.Text
        'CODENAME_get("COSMOTAGCODE", WF_COSMOTAGCODE.Text, WF_COSMOTAGCODE_TEXT.Text, WW_RTN_SW)

        '予備1
        WF_RESERVE1.Text = work.WF_SEL_RESERVE1.Text

        '予備2
        WF_RESERVE2.Text = work.WF_SEL_RESERVE2.Text

        '次回交検年月日(JR）
        WF_JRINSPECTIONDATE.Text = work.WF_SEL_JRINSPECTIONDATE.Text

        '次回交検年月日
        WF_INSPECTIONDATE.Text = work.WF_SEL_INSPECTIONDATE.Text

        '次回指定年月日(JR)
        WF_JRSPECIFIEDDATE.Text = work.WF_SEL_JRSPECIFIEDDATE.Text

        '次回指定年月日
        WF_SPECIFIEDDATE.Text = work.WF_SEL_SPECIFIEDDATE.Text

        '次回全検年月日(JR) 
        WF_JRALLINSPECTIONDATE.Text = work.WF_SEL_JRALLINSPECTIONDATE.Text

        '次回全検年月日
        WF_ALLINSPECTIONDATE.Text = work.WF_SEL_ALLINSPECTIONDATE.Text

        '前回全検年月日
        WF_PREINSPECTIONDATE.Text = work.WF_SEL_PREINSPECTIONDATE.Text

        '取得年月日
        WF_GETDATE.Text = work.WF_SEL_GETDATE.Text

        '車籍編入年月日
        WF_TRANSFERDATE.Text = work.WF_SEL_TRANSFERDATE.Text

        '取得先C, 取得先名
        WF_OBTAINEDCODE.Text = work.WF_SEL_OBTAINEDCODE.Text
        CODENAME_get("OBTAINEDCODE", WF_OBTAINEDCODE.Text, WF_OBTAINEDCODE_TEXT.Text, WW_RTN_SW)

        '現在経年
        WF_PROGRESSYEAR.Text = work.WF_SEL_PROGRESSYEAR.Text

        '次回全検時経年
        WF_NEXTPROGRESSYEAR.Text = work.WF_SEL_NEXTPROGRESSYEAR.Text

        '車籍除外年月日
        WF_EXCLUDEDATE.Text = work.WF_SEL_EXCLUDEDATE.Text

        '資産除却年月日
        WF_RETIRMENTDATE.Text = work.WF_SEL_RETIRMENTDATE.Text

        'JR車番
        WF_JRTANKNUMBER.Text = work.WF_SEL_JRTANKNUMBER.Text

        'JR車種コード
        WF_JRTANKTYPE.Text = work.WF_SEL_JRTANKTYPE.Text
        CODENAME_get("JRTANKTYPE", WF_JRTANKTYPE.Text, WF_JRTANKTYPE_TEXT.Text, WW_RTN_SW)

        '旧JOT車番
        WF_OLDTANKNUMBER.Text = work.WF_SEL_OLDTANKNUMBER.Text

        'OT車番
        WF_OTTANKNUMBER.Text = work.WF_SEL_OTTANKNUMBER.Text

        'JXTG仙台車番
        WF_JXTGTANKNUMBER1.Text = work.WF_SEL_JXTGTANKNUMBER1.Text

        'JXTG千葉車番
        WF_JXTGTANKNUMBER2.Text = work.WF_SEL_JXTGTANKNUMBER2.Text

        'JXTG川崎車番
        WF_JXTGTANKNUMBER3.Text = work.WF_SEL_JXTGTANKNUMBER3.Text

        'JXTG根岸車番
        WF_JXTGTANKNUMBER4.Text = work.WF_SEL_JXTGTANKNUMBER4.Text

        'コスモ車番
        WF_COSMOTANKNUMBER.Text = work.WF_SEL_COSMOTANKNUMBER.Text

        '富士石油車番
        WF_FUJITANKNUMBER.Text = work.WF_SEL_FUJITANKNUMBER.Text

        '出光昭シ車番
        WF_SHELLTANKNUMBER.Text = work.WF_SEL_SHELLTANKNUMBER.Text

        '出光昭シSAP車番
        WF_SAPSHELLTANKNUMBER.Text = work.WF_SEL_SAPSHELLTANKNUMBER.Text

        '予備
        WF_RESERVE3.Text = work.WF_SEL_RESERVE3.Text

        '利用フラグ
        WF_USEDFLG.Text = work.WF_SEL_USEDFLG2.Text
        CODENAME_get("USEDFLG", WF_USEDFLG.Text, WF_USEDFLG_TEXT.Text, WW_RTN_SW)

        '中間点検年月
        WF_INTERINSPECTYM.Text = work.WF_SEL_INTERINSPECTYM.Text

        '中間点検場所
        WF_INTERINSPECTSTATION.Text = work.WF_SEL_INTERINSPECTSTATION.Text
        CODENAME_get("STATIONFOCUSON", WF_INTERINSPECTSTATION.Text, WF_INTERINSPECTSTATION_TEXT.Text, WW_RTN_SW)

        '中間点検実施者
        WF_INTERINSPECTORGCODE.Text = work.WF_SEL_INTERINSPECTORGCODE.Text
        CODENAME_get("ORG", WF_INTERINSPECTORGCODE.Text, WF_INTERINSPECTORGCODE_TEXT.Text, WW_RTN_SW)

        '自主点検年月
        WF_SELFINSPECTYM.Text = work.WF_SEL_SELFINSPECTYM.Text

        '自主点検場所
        WF_SELFINSPECTSTATION.Text = work.WF_SEL_SELFINSPECTSTATION.Text
        CODENAME_get("STATIONFOCUSON", WF_SELFINSPECTSTATION.Text, WF_SELFINSPECTSTATION_TEXT.Text, WW_RTN_SW)

        '自主点検実施者
        WF_SELFINSPECTORGCODE.Text = work.WF_SEL_SELFINSPECTORGCODE.Text
        CODENAME_get("ORG", WF_SELFINSPECTORGCODE.Text, WF_SELFINSPECTORGCODE_TEXT.Text, WW_RTN_SW)

        '点検実施者(社員名)
        WF_INSPECTMEMBERNAME.Text = work.WF_SEL_INSPECTMEMBERNAME.Text

        '全検計画年月
        WF_ALLINSPECTPLANYM.Text = work.WF_SEL_ALLINSPECTPLANYM.Text

        '休車フラグ
        WF_SUSPENDFLG.Text = work.WF_SEL_SUSPENDFLG.Text
        CODENAME_get("SUSPENDFLG", WF_SUSPENDFLG.Text, WF_SUSPENDFLG_TEXT.Text, WW_RTN_SW)

        '休車日
        WF_SUSPENDDATE.Text = work.WF_SEL_SUSPENDDATE.Text

        '取得価格
        WF_PURCHASEPRICE.Text = work.WF_SEL_PURCHASEPRICE.Text

        '内部塗装
        WF_INTERNALCOATING.Text = work.WF_SEL_INTERNALCOATING.Text
        CODENAME_get("INTERNALCOATING", WF_INTERNALCOATING.Text, WF_INTERNALCOATING_TEXT.Text, WW_RTN_SW)

        '安全弁
        WF_SAFETYVALVE.Text = work.WF_SEL_SAFETYVALVE.Text

        'センターバルブ情報
        WF_CENTERVALVEINFO.Text = work.WF_SEL_CENTERVALVEINFO.Text

        '削除フラグ
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

        '削除理由区分
        WF_DELREASONKBN.Text = work.WF_SEL_DELREASONKBN.Text
        CODENAME_get("DELREASONKBN", WF_DELREASONKBN.Text, WF_DELREASONKBN_TEXT.Text, WW_RTN_SW)

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT " _
            & "     TANKNUMBER " _
            & " FROM" _
            & "    OIL.OIM0005_TANK" _
            & " WHERE" _
            & "     TANKNUMBER   = @P1" _
            & " AND DELFLG      <> @P2"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 20)  'JOT車番
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)   '削除フラグ
                PARA1.Value = WF_TANKNUMBER.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0005Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0005Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0005Chk.Load(SQLdr)

                    If OIM0005Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' タンク車マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As SqlConnection)

        '○ ＤＢ更新
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;" _
            & "    SET @hensuu = 0 ;" _
            & " DECLARE hensuu CURSOR FOR" _
            & "    SELECT" _
            & "        CAST(UPDTIMSTP AS bigint) AS hensuu" _
            & "    FROM" _
            & "        OIL.OIM0005_TANK" _
            & "    WHERE" _
            & "        TANKNUMBER       = @P01 ;" _
            & " OPEN hensuu ;" _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;" _
            & " IF (@@FETCH_STATUS = 0)" _
            & "    UPDATE OIL.OIM0005_TANK" _
            & "    SET" _
            & "        DELFLG = @P00" _
            & "        , ORIGINOWNERCODE = @P02" _
            & "        , OWNERCODE = @P03" _
            & "        , LEASECODE = @P04" _
            & "        , LEASECLASS = @P05" _
            & "        , AUTOEXTENTION = @P06" _
            & "        , LEASESTYMD = @P07" _
            & "        , LEASEENDYMD = @P08" _
            & "        , USERCODE = @P09" _
            & "        , CURRENTSTATIONCODE = @P10" _
            & "        , EXTRADINARYSTATIONCODE = @P11" _
            & "        , USERLIMIT = @P12" _
            & "        , LIMITTEXTRADIARYSTATION = @P13" _
            & "        , DEDICATETYPECODE = @P14" _
            & "        , EXTRADINARYTYPECODE = @P15" _
            & "        , EXTRADINARYLIMIT = @P16" _
            & "        , OPERATIONBASECODE = @P17" _
            & "        , COLORCODE = @P18" _
            & "        , MARKCODE = @P19" _
            & "        , MARKNAME = @P20" _
            & "        , GETDATE = @P21" _
            & "        , TRANSFERDATE = @P22" _
            & "        , OBTAINEDCODE = @P23" _
            & "        , MODEL = @P24" _
            & "        , MODELKANA = @P25" _
            & "        , LOAD = @P26" _
            & "        , LOADUNIT = @P27" _
            & "        , VOLUME = @P28" _
            & "        , VOLUMEUNIT = @P29" _
            & "        , ORIGINOWNERNAME = @P30" _
            & "        , OWNERNAME = @P31" _
            & "        , LEASENAME = @P32" _
            & "        , LEASECLASSNAME = @P33" _
            & "        , USERNAME = @P34" _
            & "        , CURRENTSTATIONNAME = @P35" _
            & "        , EXTRADINARYSTATIONNAME = @P36" _
            & "        , DEDICATETYPENAME = @P37" _
            & "        , EXTRADINARYTYPENAME = @P38" _
            & "        , OPERATIONBASENAME = @P39" _
            & "        , COLORNAME = @P40" _
            & "        , RESERVE1 = @P41" _
            & "        , RESERVE2 = @P42" _
            & "        , SPECIFIEDDATE = @P43" _
            & "        , JRALLINSPECTIONDATE = @P44" _
            & "        , PROGRESSYEAR = @P45" _
            & "        , NEXTPROGRESSYEAR = @P46" _
            & "        , JRINSPECTIONDATE = @P47" _
            & "        , INSPECTIONDATE = @P48" _
            & "        , JRSPECIFIEDDATE = @P49" _
            & "        , JRTANKNUMBER = @P50" _
            & "        , OLDTANKNUMBER = @P51" _
            & "        , OTTANKNUMBER = @P52" _
            & "        , JXTGTANKNUMBER1 = @P53" _
            & "        , COSMOTANKNUMBER = @P54" _
            & "        , FUJITANKNUMBER = @P55" _
            & "        , SHELLTANKNUMBER = @P56" _
            & "        , RESERVE3 = @P57" _
            & "        , USEDFLG = @P65" _
            & "        , UPDYMD = @P61" _
            & "        , UPDUSER = @P62" _
            & "        , UPDTERMID = @P63" _
            & "        , RECEIVEYMD = @P64" _
            & "        , MYWEIGHT = @P66" _
            & "        , AUTOEXTENTIONNAME = @P67" _
            & "        , BIGOILCODE = @P68" _
            & "        , BIGOILNAME = @P69" _
            & "        , JXTGTAGCODE1 = @P70" _
            & "        , JXTGTAGNAME1 = @P71" _
            & "        , JXTGTAGCODE2 = @P72" _
            & "        , JXTGTAGNAME2 = @P73" _
            & "        , JXTGTAGCODE3 = @P74" _
            & "        , JXTGTAGNAME3 = @P75" _
            & "        , JXTGTAGCODE4 = @P76" _
            & "        , JXTGTAGNAME4 = @P77" _
            & "        , IDSSTAGCODE = @P78" _
            & "        , IDSSTAGNAME = @P79" _
            & "        , COSMOTAGCODE = @P80" _
            & "        , COSMOTAGNAME = @P81" _
            & "        , ALLINSPECTIONDATE = @P82" _
            & "        , PREINSPECTIONDATE = @P83" _
            & "        , OBTAINEDNAME = @P84" _
            & "        , EXCLUDEDATE = @P85" _
            & "        , RETIRMENTDATE = @P86" _
            & "        , JRTANKTYPE = @P87" _
            & "        , JXTGTANKNUMBER2 = @P88" _
            & "        , JXTGTANKNUMBER3 = @P89" _
            & "        , JXTGTANKNUMBER4 = @P90" _
            & "        , SAPSHELLTANKNUMBER = @P91" _
            & "        , LENGTH = @P92" _
            & "        , TANKLENGTH = @P93" _
            & "        , MAXCALIBER = @P94" _
            & "        , MINCALIBER = @P95" _
            & "        , LENGTHFLG = @P96" _
            & "        , INTERINSPECTYM = @P97" _
            & "        , INTERINSPECTSTATION = @P98" _
            & "        , INTERINSPECTORGCODE = @P99" _
            & "        , SELFINSPECTYM = @P100" _
            & "        , SELFINSPECTSTATION = @P101" _
            & "        , SELFINSPECTORGCODE = @P102" _
            & "        , MIDDLEOILCODE = @P103" _
            & "        , MIDDLEOILNAME = @P104" _
            & "        , INSPECTMEMBERNAME = @P105" _
            & "        , SUBOPERATIONBASECODE = @P106" _
            & "        , SUBOPERATIONBASENAME = @P107" _
            & "        , ALLINSPECTPLANYM = @P108" _
            & "        , SUSPENDFLG = @P109" _
            & "        , SUSPENDDATE = @P110" _
            & "        , PURCHASEPRICE = @P111" _
            & "        , INTERNALCOATING = @P112" _
            & "        , SAFETYVALVE = @P113" _
            & "        , CENTERVALVEINFO = @P114" _
            & "        , DELREASONKBN = @P115" _
            & "    WHERE" _
            & "        TANKNUMBER       = @P01 ;" _
            & " IF (@@FETCH_STATUS <> 0)" _
            & "    INSERT INTO OIL.OIM0005_TANK" _
            & "        (DELFLG" _
            & "        , TANKNUMBER" _
            & "        , ORIGINOWNERCODE" _
            & "        , OWNERCODE" _
            & "        , LEASECODE" _
            & "        , LEASECLASS" _
            & "        , AUTOEXTENTION" _
            & "        , LEASESTYMD" _
            & "        , LEASEENDYMD" _
            & "        , USERCODE" _
            & "        , CURRENTSTATIONCODE" _
            & "        , EXTRADINARYSTATIONCODE" _
            & "        , USERLIMIT" _
            & "        , LIMITTEXTRADIARYSTATION" _
            & "        , DEDICATETYPECODE" _
            & "        , EXTRADINARYTYPECODE" _
            & "        , EXTRADINARYLIMIT" _
            & "        , OPERATIONBASECODE" _
            & "        , COLORCODE" _
            & "        , MARKCODE" _
            & "        , MARKNAME" _
            & "        , GETDATE" _
            & "        , TRANSFERDATE" _
            & "        , OBTAINEDCODE" _
            & "        , MODEL" _
            & "        , MODELKANA" _
            & "        , LOAD" _
            & "        , LOADUNIT" _
            & "        , VOLUME" _
            & "        , VOLUMEUNIT" _
            & "        , ORIGINOWNERNAME" _
            & "        , OWNERNAME" _
            & "        , LEASENAME" _
            & "        , LEASECLASSNAME" _
            & "        , USERNAME" _
            & "        , CURRENTSTATIONNAME" _
            & "        , EXTRADINARYSTATIONNAME" _
            & "        , DEDICATETYPENAME" _
            & "        , EXTRADINARYTYPENAME" _
            & "        , OPERATIONBASENAME" _
            & "        , COLORNAME" _
            & "        , RESERVE1" _
            & "        , RESERVE2" _
            & "        , SPECIFIEDDATE" _
            & "        , JRALLINSPECTIONDATE" _
            & "        , PROGRESSYEAR" _
            & "        , NEXTPROGRESSYEAR" _
            & "        , JRINSPECTIONDATE" _
            & "        , INSPECTIONDATE" _
            & "        , JRSPECIFIEDDATE" _
            & "        , JRTANKNUMBER" _
            & "        , OLDTANKNUMBER" _
            & "        , OTTANKNUMBER" _
            & "        , JXTGTANKNUMBER1" _
            & "        , COSMOTANKNUMBER" _
            & "        , FUJITANKNUMBER" _
            & "        , SHELLTANKNUMBER" _
            & "        , RESERVE3" _
            & "        , USEDFLG" _
            & "        , INITYMD" _
            & "        , INITUSER" _
            & "        , INITTERMID" _
            & "        , UPDYMD" _
            & "        , UPDUSER" _
            & "        , UPDTERMID" _
            & "        , RECEIVEYMD" _
            & "        , MYWEIGHT" _
            & "        , AUTOEXTENTIONNAME" _
            & "        , BIGOILCODE" _
            & "        , BIGOILNAME" _
            & "        , JXTGTAGCODE1" _
            & "        , JXTGTAGNAME1" _
            & "        , JXTGTAGCODE2" _
            & "        , JXTGTAGNAME2" _
            & "        , JXTGTAGCODE3" _
            & "        , JXTGTAGNAME3" _
            & "        , JXTGTAGCODE4" _
            & "        , JXTGTAGNAME4" _
            & "        , IDSSTAGCODE" _
            & "        , IDSSTAGNAME" _
            & "        , COSMOTAGCODE" _
            & "        , COSMOTAGNAME" _
            & "        , ALLINSPECTIONDATE" _
            & "        , PREINSPECTIONDATE" _
            & "        , OBTAINEDNAME" _
            & "        , EXCLUDEDATE" _
            & "        , RETIRMENTDATE" _
            & "        , JRTANKTYPE" _
            & "        , JXTGTANKNUMBER2" _
            & "        , JXTGTANKNUMBER3" _
            & "        , JXTGTANKNUMBER4" _
            & "        , SAPSHELLTANKNUMBER" _
            & "        , LENGTH" _
            & "        , TANKLENGTH" _
            & "        , MAXCALIBER" _
            & "        , MINCALIBER" _
            & "        , LENGTHFLG" _
            & "        , INTERINSPECTYM" _
            & "        , INTERINSPECTSTATION" _
            & "        , INTERINSPECTORGCODE" _
            & "        , SELFINSPECTYM" _
            & "        , SELFINSPECTSTATION" _
            & "        , SELFINSPECTORGCODE" _
            & "        , MIDDLEOILCODE" _
            & "        , MIDDLEOILNAME" _
            & "        , INSPECTMEMBERNAME" _
            & "        , SUBOPERATIONBASECODE" _
            & "        , SUBOPERATIONBASENAME" _
            & "        , ALLINSPECTPLANYM" _
            & "        , SUSPENDFLG" _
            & "        , SUSPENDDATE" _
            & "        , PURCHASEPRICE" _
            & "        , INTERNALCOATING" _
            & "        , SAFETYVALVE" _
            & "        , CENTERVALVEINFO" _
            & "        , DELREASONKBN)" _
            & "    VALUES" _
            & "        (@P00" _
            & "        , @P01" _
            & "        , @P02" _
            & "        , @P03" _
            & "        , @P04" _
            & "        , @P05" _
            & "        , @P06" _
            & "        , @P07" _
            & "        , @P08" _
            & "        , @P09" _
            & "        , @P10" _
            & "        , @P11" _
            & "        , @P12" _
            & "        , @P13" _
            & "        , @P14" _
            & "        , @P15" _
            & "        , @P16" _
            & "        , @P17" _
            & "        , @P18" _
            & "        , @P19" _
            & "        , @P20" _
            & "        , @P21" _
            & "        , @P22" _
            & "        , @P23" _
            & "        , @P24" _
            & "        , @P25" _
            & "        , @P26" _
            & "        , @P27" _
            & "        , @P28" _
            & "        , @P29" _
            & "        , @P30" _
            & "        , @P31" _
            & "        , @P32" _
            & "        , @P33" _
            & "        , @P34" _
            & "        , @P35" _
            & "        , @P36" _
            & "        , @P37" _
            & "        , @P38" _
            & "        , @P39" _
            & "        , @P40" _
            & "        , @P41" _
            & "        , @P42" _
            & "        , @P43" _
            & "        , @P44" _
            & "        , @P45" _
            & "        , @P46" _
            & "        , @P47" _
            & "        , @P48" _
            & "        , @P49" _
            & "        , @P50" _
            & "        , @P51" _
            & "        , @P52" _
            & "        , @P53" _
            & "        , @P54" _
            & "        , @P55" _
            & "        , @P56" _
            & "        , @P57" _
            & "        , @P65" _
            & "        , @P58" _
            & "        , @P59" _
            & "        , @P60" _
            & "        , @P61" _
            & "        , @P62" _
            & "        , @P63" _
            & "        , @P64" _
            & "        , @P66" _
            & "        , @P67" _
            & "        , @P68" _
            & "        , @P69" _
            & "        , @P70" _
            & "        , @P71" _
            & "        , @P72" _
            & "        , @P73" _
            & "        , @P74" _
            & "        , @P75" _
            & "        , @P76" _
            & "        , @P77" _
            & "        , @P78" _
            & "        , @P79" _
            & "        , @P80" _
            & "        , @P81" _
            & "        , @P82" _
            & "        , @P83" _
            & "        , @P84" _
            & "        , @P85" _
            & "        , @P86" _
            & "        , @P87" _
            & "        , @P88" _
            & "        , @P89" _
            & "        , @P90" _
            & "        , @P91" _
            & "        , @P92" _
            & "        , @P93" _
            & "        , @P94" _
            & "        , @P95" _
            & "        , @P96" _
            & "        , @P97" _
            & "        , @P98" _
            & "        , @P99" _
            & "        , @P100" _
            & "        , @P101" _
            & "        , @P102" _
            & "        , @P103" _
            & "        , @P104" _
            & "        , @P105" _
            & "        , @P106" _
            & "        , @P107" _
            & "        , @P108" _
            & "        , @P109" _
            & "        , @P110" _
            & "        , @P111" _
            & "        , @P112" _
            & "        , @P113" _
            & "        , @P114" _
            & "        , @P115) ;" _
            & " CLOSE hensuu ;" _
            & " DEALLOCATE hensuu ;"

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT" _
            & "     DELFLG" _
            & "     , TANKNUMBER" _
            & "     , MODEL" _
            & "     , MODELKANA" _
            & "     , LOAD" _
            & "     , LOADUNIT" _
            & "     , VOLUME" _
            & "     , VOLUMEUNIT" _
            & "     , MYWEIGHT" _
            & "     , LENGTH" _
            & "     , TANKLENGTH" _
            & "     , MAXCALIBER" _
            & "     , MINCALIBER" _
            & "     , LENGTHFLG" _
            & "     , ORIGINOWNERCODE" _
            & "     , ORIGINOWNERNAME" _
            & "     , OWNERCODE" _
            & "     , OWNERNAME" _
            & "     , LEASECODE" _
            & "     , LEASENAME" _
            & "     , LEASECLASS" _
            & "     , LEASECLASSNAME" _
            & "     , AUTOEXTENTION" _
            & "     , AUTOEXTENTIONNAME" _
            & "     , LEASESTYMD" _
            & "     , LEASEENDYMD" _
            & "     , USERCODE" _
            & "     , USERNAME" _
            & "     , CURRENTSTATIONCODE" _
            & "     , CURRENTSTATIONNAME" _
            & "     , EXTRADINARYSTATIONCODE" _
            & "     , EXTRADINARYSTATIONNAME" _
            & "     , USERLIMIT" _
            & "     , LIMITTEXTRADIARYSTATION" _
            & "     , DEDICATETYPECODE" _
            & "     , DEDICATETYPENAME" _
            & "     , EXTRADINARYTYPECODE" _
            & "     , EXTRADINARYTYPENAME" _
            & "     , EXTRADINARYLIMIT" _
            & "     , BIGOILCODE" _
            & "     , BIGOILNAME" _
            & "     , MIDDLEOILCODE" _
            & "     , MIDDLEOILNAME" _
            & "     , DOWNLOADDATE" _
            & "     , OPERATIONBASECODE" _
            & "     , OPERATIONBASENAME" _
            & "     , SUBOPERATIONBASECODE" _
            & "     , SUBOPERATIONBASENAME" _
            & "     , COLORCODE" _
            & "     , COLORNAME" _
            & "     , MARKCODE" _
            & "     , MARKNAME" _
            & "     , JXTGTAGCODE1" _
            & "     , JXTGTAGNAME1" _
            & "     , JXTGTAGCODE2" _
            & "     , JXTGTAGNAME2" _
            & "     , JXTGTAGCODE3" _
            & "     , JXTGTAGNAME3" _
            & "     , JXTGTAGCODE4" _
            & "     , JXTGTAGNAME4" _
            & "     , IDSSTAGCODE" _
            & "     , IDSSTAGNAME" _
            & "     , COSMOTAGCODE" _
            & "     , COSMOTAGNAME" _
            & "     , RESERVE1" _
            & "     , RESERVE2" _
            & "     , JRINSPECTIONDATE" _
            & "     , INSPECTIONDATE" _
            & "     , JRSPECIFIEDDATE" _
            & "     , SPECIFIEDDATE" _
            & "     , JRALLINSPECTIONDATE" _
            & "     , ALLINSPECTIONDATE" _
            & "     , PREINSPECTIONDATE" _
            & "     , GETDATE" _
            & "     , TRANSFERDATE" _
            & "     , OBTAINEDCODE" _
            & "     , OBTAINEDNAME" _
            & "     , PROGRESSYEAR" _
            & "     , NEXTPROGRESSYEAR" _
            & "     , EXCLUDEDATE" _
            & "     , RETIRMENTDATE" _
            & "     , JRTANKNUMBER" _
            & "     , JRTANKTYPE" _
            & "     , OLDTANKNUMBER" _
            & "     , OTTANKNUMBER" _
            & "     , JXTGTANKNUMBER1" _
            & "     , JXTGTANKNUMBER2" _
            & "     , JXTGTANKNUMBER3" _
            & "     , JXTGTANKNUMBER4" _
            & "     , COSMOTANKNUMBER" _
            & "     , FUJITANKNUMBER" _
            & "     , SHELLTANKNUMBER" _
            & "     , SAPSHELLTANKNUMBER" _
            & "     , RESERVE3" _
            & "     , USEDFLG" _
            & "     , INTERINSPECTYM" _
            & "     , INTERINSPECTSTATION" _
            & "     , INTERINSPECTORGCODE" _
            & "     , SELFINSPECTYM" _
            & "     , SELFINSPECTSTATION" _
            & "     , SELFINSPECTORGCODE" _
            & "     , INSPECTMEMBERNAME" _
            & "     , ALLINSPECTPLANYM" _
            & "     , SUSPENDFLG" _
            & "     , SUSPENDDATE" _
            & "     , PURCHASEPRICE" _
            & "     , INTERNALCOATING" _
            & "     , SAFETYVALVE" _
            & "     , CENTERVALVEINFO" _
            & "     , DELREASONKBN" _
            & "     , INITYMD" _
            & "     , INITUSER" _
            & "     , INITTERMID" _
            & "     , UPDYMD" _
            & "     , UPDUSER" _
            & "     , UPDTERMID" _
            & "     , RECEIVEYMD" _
            & "     , CAST(UPDTIMSTP As bigint) As UPDTIMSTP" _
            & " FROM" _
            & "     OIL.OIM0005_TANK" _
            & " WHERE" _
            & "     TANKNUMBER = @P01"

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon), SQLcmdJnl As New SqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As SqlParameter = SQLcmd.Parameters.Add("@P00", SqlDbType.NVarChar, 1)           '削除フラグ
                Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", SqlDbType.NVarChar, 8)           'JOT車番
                Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", SqlDbType.NVarChar, 20)          '原籍所有者C
                Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", SqlDbType.NVarChar, 20)          '名義所有者C
                Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", SqlDbType.NVarChar, 20)          'リース先C
                Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", SqlDbType.NVarChar, 20)          '請負リース区分C
                Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", SqlDbType.NVarChar, 1)           '自動延長
                Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", SqlDbType.Date)                  'リース開始年月日
                Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", SqlDbType.Date)                  'リース満了年月日
                Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", SqlDbType.NVarChar, 20)          '第三者使用者C
                Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", SqlDbType.NVarChar, 20)          '原常備駅C
                Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", SqlDbType.NVarChar, 20)          '臨時常備駅C
                Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", SqlDbType.Date)                  '第三者使用期限
                Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", SqlDbType.Date)                  '臨時常備駅期限
                Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", SqlDbType.NVarChar, 20)          '原専用種別C
                Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", SqlDbType.NVarChar, 20)          '臨時専用種別C
                Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", SqlDbType.Date)                  '臨時専用期限
                Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", SqlDbType.NVarChar, 20)          '運用基地C
                Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", SqlDbType.NVarChar, 20)          '塗色C
                Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", SqlDbType.NVarChar, 1)           'マークコード
                Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", SqlDbType.NVarChar, 20)          'マーク名
                Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", SqlDbType.Date)                  '取得年月日
                Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", SqlDbType.Date)                  '車籍編入年月日
                Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", SqlDbType.NVarChar, 2)           '取得先C
                Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", SqlDbType.NVarChar, 20)          '形式
                Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", SqlDbType.NVarChar, 10)          '形式カナ
                Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", SqlDbType.Float, 4, 1)           '荷重
                Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", SqlDbType.NVarChar, 2)           '荷重単位
                Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", SqlDbType.Float, 4, 1)           '容積
                Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", SqlDbType.NVarChar, 2)           '容積単位
                Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", SqlDbType.NVarChar, 20)          '原籍所有者
                Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", SqlDbType.NVarChar, 20)          '名義所有者
                Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", SqlDbType.NVarChar, 20)          'リース先
                Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", SqlDbType.NVarChar, 20)          'リース区分
                Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", SqlDbType.NVarChar, 20)          '第三者使用者
                Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", SqlDbType.NVarChar, 20)          '原常備駅
                Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", SqlDbType.NVarChar, 20)          '臨時常備駅
                Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", SqlDbType.NVarChar, 20)          '原専用種別
                Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", SqlDbType.NVarChar, 20)          '臨時専用種別
                Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", SqlDbType.NVarChar, 20)          '運用場所
                Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", SqlDbType.NVarChar, 20)          '塗色
                Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", SqlDbType.NVarChar, 20)          '予備1
                Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", SqlDbType.NVarChar, 20)          '予備2
                Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", SqlDbType.Date)                  '次回指定年月日
                Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", SqlDbType.Date)                  '次回全検年月日(JR) 
                Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", SqlDbType.Int)                   '現在経年
                Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", SqlDbType.Int)                   '次回全検時経年
                Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", SqlDbType.Date)                  '次回交検年月日(JR）
                Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", SqlDbType.Date)                  '次回交検年月日
                Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", SqlDbType.Date)                  '次回指定年月日(JR)
                Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", SqlDbType.NVarChar, 20)          'JR車番
                Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", SqlDbType.NVarChar, 20)          '旧JOT車番
                Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", SqlDbType.NVarChar, 20)          'OT車番
                Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", SqlDbType.NVarChar, 20)          'JXTG仙台車番
                Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", SqlDbType.NVarChar, 20)          'コスモ車番
                Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", SqlDbType.NVarChar, 20)          '富士石油車番
                Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", SqlDbType.NVarChar, 20)          '出光昭シ車番
                Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", SqlDbType.NVarChar, 20)          '予備
                Dim PARA65 As SqlParameter = SQLcmd.Parameters.Add("@P65", SqlDbType.NVarChar, 1)           '利用フラグ
                Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", SqlDbType.DateTime)              '登録年月日
                Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", SqlDbType.NVarChar, 20)          '登録ユーザーＩＤ
                Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", SqlDbType.NVarChar, 20)          '登録端末
                Dim PARA61 As SqlParameter = SQLcmd.Parameters.Add("@P61", SqlDbType.DateTime)              '更新年月日
                Dim PARA62 As SqlParameter = SQLcmd.Parameters.Add("@P62", SqlDbType.NVarChar, 20)          '更新ユーザーＩＤ
                Dim PARA63 As SqlParameter = SQLcmd.Parameters.Add("@P63", SqlDbType.NVarChar, 20)          '更新端末
                Dim PARA64 As SqlParameter = SQLcmd.Parameters.Add("@P64", SqlDbType.DateTime)              '集信日時
                Dim PARA66 As SqlParameter = SQLcmd.Parameters.Add("@P66", SqlDbType.Float, 4, 1)           '自重
                Dim PARA67 As SqlParameter = SQLcmd.Parameters.Add("@P67", SqlDbType.NVarChar, 20)          '自動延長名
                Dim PARA68 As SqlParameter = SQLcmd.Parameters.Add("@P68", SqlDbType.NVarChar, 1)           '油種大分類コード
                Dim PARA69 As SqlParameter = SQLcmd.Parameters.Add("@P69", SqlDbType.NVarChar, 10)          '油種大分類名
                Dim PARA70 As SqlParameter = SQLcmd.Parameters.Add("@P70", SqlDbType.NVarChar, 20)          'JXTG仙台タグコード
                Dim PARA71 As SqlParameter = SQLcmd.Parameters.Add("@P71", SqlDbType.NVarChar, 20)          'JXTG仙台タグ名
                Dim PARA72 As SqlParameter = SQLcmd.Parameters.Add("@P72", SqlDbType.NVarChar, 20)          'JXTG千葉タグコード
                Dim PARA73 As SqlParameter = SQLcmd.Parameters.Add("@P73", SqlDbType.NVarChar, 20)          'JXTG千葉タグ名
                Dim PARA74 As SqlParameter = SQLcmd.Parameters.Add("@P74", SqlDbType.NVarChar, 20)          'JXTG川崎タグコード
                Dim PARA75 As SqlParameter = SQLcmd.Parameters.Add("@P75", SqlDbType.NVarChar, 20)          'JXTG川崎タグ名
                Dim PARA76 As SqlParameter = SQLcmd.Parameters.Add("@P76", SqlDbType.NVarChar, 20)          'JXTG根岸タグコード
                Dim PARA77 As SqlParameter = SQLcmd.Parameters.Add("@P77", SqlDbType.NVarChar, 20)          'JXTG根岸タグ名
                Dim PARA78 As SqlParameter = SQLcmd.Parameters.Add("@P78", SqlDbType.NVarChar, 20)          '出光昭和シェルタグコード
                Dim PARA79 As SqlParameter = SQLcmd.Parameters.Add("@P79", SqlDbType.NVarChar, 20)          '出光昭和シェルタグ名
                Dim PARA80 As SqlParameter = SQLcmd.Parameters.Add("@P80", SqlDbType.NVarChar, 20)          'コスモタグコード
                Dim PARA81 As SqlParameter = SQLcmd.Parameters.Add("@P81", SqlDbType.NVarChar, 20)          'コスモタグ名
                Dim PARA82 As SqlParameter = SQLcmd.Parameters.Add("@P82", SqlDbType.Date)                  '次回全件年月日
                Dim PARA83 As SqlParameter = SQLcmd.Parameters.Add("@P83", SqlDbType.Date)                  '前回全件年月日
                Dim PARA84 As SqlParameter = SQLcmd.Parameters.Add("@P84", SqlDbType.NVarChar, 20)          '取得名
                Dim PARA85 As SqlParameter = SQLcmd.Parameters.Add("@P85", SqlDbType.Date)                  '車籍除外年月日
                Dim PARA86 As SqlParameter = SQLcmd.Parameters.Add("@P86", SqlDbType.Date)                  '資産除却年月日
                Dim PARA87 As SqlParameter = SQLcmd.Parameters.Add("@P87", SqlDbType.NVarChar, 20)          'JR車種コード
                Dim PARA88 As SqlParameter = SQLcmd.Parameters.Add("@P88", SqlDbType.NVarChar, 20)          'JXTG千葉車番
                Dim PARA89 As SqlParameter = SQLcmd.Parameters.Add("@P89", SqlDbType.NVarChar, 20)          'JXTG川崎車番
                Dim PARA90 As SqlParameter = SQLcmd.Parameters.Add("@P90", SqlDbType.NVarChar, 20)          'JXTG根岸車番
                Dim PARA91 As SqlParameter = SQLcmd.Parameters.Add("@P91", SqlDbType.NVarChar, 20)          '出光昭シSAP車番
                Dim PARA92 As SqlParameter = SQLcmd.Parameters.Add("@P92", SqlDbType.Int)                   'タンク車長
                Dim PARA93 As SqlParameter = SQLcmd.Parameters.Add("@P93", SqlDbType.Int)                   'タンク車体長
                Dim PARA94 As SqlParameter = SQLcmd.Parameters.Add("@P94", SqlDbType.Int)                   '最大口径
                Dim PARA95 As SqlParameter = SQLcmd.Parameters.Add("@P95", SqlDbType.Int)                   '最大口径
                Dim PARA96 As SqlParameter = SQLcmd.Parameters.Add("@P96", SqlDbType.NVarChar, 1)           '長さフラグ
                Dim PARA97 As SqlParameter = SQLcmd.Parameters.Add("@P97", SqlDbType.Date)                  '中間点検年月
                Dim PARA98 As SqlParameter = SQLcmd.Parameters.Add("@P98", SqlDbType.NVarChar, 7)           '中間点検場所
                Dim PARA99 As SqlParameter = SQLcmd.Parameters.Add("@P99", SqlDbType.NVarChar, 7)           '中間点検実施者
                Dim PARA100 As SqlParameter = SQLcmd.Parameters.Add("@P100", SqlDbType.Date)                '自主点検年月
                Dim PARA101 As SqlParameter = SQLcmd.Parameters.Add("@P101", SqlDbType.NVarChar, 7)         '自主点検場所
                Dim PARA102 As SqlParameter = SQLcmd.Parameters.Add("@P102", SqlDbType.NVarChar, 7)         '自主点検実施者
                Dim PARA103 As SqlParameter = SQLcmd.Parameters.Add("@P103", SqlDbType.NVarChar, 1)         '油種中分類コード
                Dim PARA104 As SqlParameter = SQLcmd.Parameters.Add("@P104", SqlDbType.NVarChar, 10)        '油種中分類名
                Dim PARA105 As SqlParameter = SQLcmd.Parameters.Add("@P105", SqlDbType.NVarChar, 20)        '点検実施者(社員名)
                Dim PARA106 As SqlParameter = SQLcmd.Parameters.Add("@P106", SqlDbType.NVarChar, 20)        '運用基地C（サブ）
                Dim PARA107 As SqlParameter = SQLcmd.Parameters.Add("@P107", SqlDbType.NVarChar, 20)        '運用基地（サブ）
                Dim PARA108 As SqlParameter = SQLcmd.Parameters.Add("@P108", SqlDbType.Date)                '全検計画年月
                Dim PARA109 As SqlParameter = SQLcmd.Parameters.Add("@P109", SqlDbType.NVarChar, 1)         '休車フラグ
                Dim PARA110 As SqlParameter = SQLcmd.Parameters.Add("@P110", SqlDbType.Date)                '休車日
                Dim PARA111 As SqlParameter = SQLcmd.Parameters.Add("@P111", SqlDbType.Money)               '取得価格
                Dim PARA112 As SqlParameter = SQLcmd.Parameters.Add("@P112", SqlDbType.NVarChar, 1)         '内部塗装
                Dim PARA113 As SqlParameter = SQLcmd.Parameters.Add("@P113", SqlDbType.NVarChar, 20)        '安全弁
                Dim PARA114 As SqlParameter = SQLcmd.Parameters.Add("@P114", SqlDbType.NVarChar, 20)        'センターバルブ情報
                Dim PARA115 As SqlParameter = SQLcmd.Parameters.Add("@P115", SqlDbType.NVarChar, 1)         '削除理由区分

                Dim JPARA01 As SqlParameter = SQLcmdJnl.Parameters.Add("@P01", SqlDbType.NVarChar, 8)       'JOT車番

                Dim OIM0005row As DataRow = OIM0005INPtbl.Rows(0)
                Dim WW_DATENOW As DateTime = Date.Now

                'DB更新
                PARA00.Value = OIM0005row("DELFLG")
                PARA01.Value = OIM0005row("TANKNUMBER")
                PARA02.Value = OIM0005row("ORIGINOWNERCODE")
                PARA03.Value = OIM0005row("OWNERCODE")
                PARA04.Value = OIM0005row("LEASECODE")
                PARA05.Value = OIM0005row("LEASECLASS")
                PARA06.Value = OIM0005row("AUTOEXTENTION")
                If OIM0005row("LEASESTYMD") <> "" Then
                    PARA07.Value = OIM0005row("LEASESTYMD")
                Else
                    PARA07.Value = DBNull.Value
                End If
                If OIM0005row("LEASEENDYMD") <> "" Then
                    PARA08.Value = OIM0005row("LEASEENDYMD")
                Else
                    PARA08.Value = DBNull.Value
                End If
                PARA09.Value = OIM0005row("USERCODE")
                PARA10.Value = OIM0005row("CURRENTSTATIONCODE")
                PARA11.Value = OIM0005row("EXTRADINARYSTATIONCODE")
                If OIM0005row("USERLIMIT") <> "" Then
                    PARA12.Value = OIM0005row("USERLIMIT")
                Else
                    PARA12.Value = DBNull.Value
                End If
                If OIM0005row("LIMITTEXTRADIARYSTATION") <> "" Then
                    PARA13.Value = OIM0005row("LIMITTEXTRADIARYSTATION")
                Else
                    PARA13.Value = DBNull.Value
                End If
                PARA14.Value = OIM0005row("DEDICATETYPECODE")
                PARA15.Value = OIM0005row("EXTRADINARYTYPECODE")
                If OIM0005row("EXTRADINARYLIMIT") <> "" Then
                    PARA16.Value = OIM0005row("EXTRADINARYLIMIT")
                Else
                    PARA16.Value = DBNull.Value
                End If
                PARA17.Value = OIM0005row("OPERATIONBASECODE")
                PARA18.Value = OIM0005row("COLORCODE")
                PARA19.Value = OIM0005row("MARKCODE")
                PARA20.Value = OIM0005row("MARKNAME")
                If OIM0005row("GETDATE") <> "" Then
                    PARA21.Value = OIM0005row("GETDATE")
                Else
                    PARA21.Value = DBNull.Value
                End If
                If OIM0005row("TRANSFERDATE") <> "" Then
                    PARA22.Value = OIM0005row("TRANSFERDATE")
                Else
                    PARA22.Value = DBNull.Value
                End If
                PARA23.Value = OIM0005row("OBTAINEDCODE")
                PARA24.Value = OIM0005row("MODEL")
                PARA25.Value = OIM0005row("MODELKANA")
                If OIM0005row("LOAD") <> "" Then
                    PARA26.Value = OIM0005row("LOAD")
                Else
                    PARA26.Value = "0.0"
                End If
                PARA27.Value = OIM0005row("LOADUNIT")
                If OIM0005row("VOLUME") <> "" Then
                    PARA28.Value = OIM0005row("VOLUME")
                Else
                    PARA28.Value = "0.0"
                End If
                PARA29.Value = OIM0005row("VOLUMEUNIT")
                PARA30.Value = OIM0005row("ORIGINOWNERNAME")
                PARA31.Value = OIM0005row("OWNERNAME")
                PARA32.Value = OIM0005row("LEASENAME")
                PARA33.Value = OIM0005row("LEASECLASSNAME")
                PARA34.Value = OIM0005row("USERNAME")
                PARA35.Value = OIM0005row("CURRENTSTATIONNAME")
                PARA36.Value = OIM0005row("EXTRADINARYSTATIONNAME")
                PARA37.Value = OIM0005row("DEDICATETYPENAME")
                PARA38.Value = OIM0005row("EXTRADINARYTYPENAME")
                PARA39.Value = OIM0005row("OPERATIONBASENAME")
                PARA40.Value = OIM0005row("COLORNAME")
                PARA41.Value = OIM0005row("RESERVE1")
                PARA42.Value = OIM0005row("RESERVE2")
                If RTrim(OIM0005row("SPECIFIEDDATE")) <> "" Then
                    PARA43.Value = OIM0005row("SPECIFIEDDATE")
                Else
                    PARA43.Value = DBNull.Value
                End If
                If OIM0005row("JRALLINSPECTIONDATE") <> "" Then
                    PARA44.Value = OIM0005row("JRALLINSPECTIONDATE")
                Else
                    PARA44.Value = DBNull.Value
                End If
                If OIM0005row("PROGRESSYEAR") <> "" Then
                    PARA45.Value = OIM0005row("PROGRESSYEAR")
                Else
                    PARA45.Value = "0"
                End If
                If OIM0005row("NEXTPROGRESSYEAR") <> "" Then
                    PARA46.Value = OIM0005row("NEXTPROGRESSYEAR")
                Else
                    PARA46.Value = "0"
                End If
                If OIM0005row("JRINSPECTIONDATE") <> "" Then
                    PARA47.Value = OIM0005row("JRINSPECTIONDATE")
                Else
                    PARA47.Value = DBNull.Value
                End If
                If OIM0005row("INSPECTIONDATE") <> "" Then
                    PARA48.Value = OIM0005row("INSPECTIONDATE")
                Else
                    PARA48.Value = DBNull.Value
                End If
                If OIM0005row("JRSPECIFIEDDATE") <> "" Then
                    PARA49.Value = OIM0005row("JRSPECIFIEDDATE")
                Else
                    PARA49.Value = DBNull.Value
                End If
                PARA50.Value = OIM0005row("JRTANKNUMBER")
                PARA51.Value = OIM0005row("OLDTANKNUMBER")
                PARA52.Value = OIM0005row("OTTANKNUMBER")
                PARA53.Value = OIM0005row("JXTGTANKNUMBER1")
                PARA54.Value = OIM0005row("COSMOTANKNUMBER")
                PARA55.Value = OIM0005row("FUJITANKNUMBER")
                PARA56.Value = OIM0005row("SHELLTANKNUMBER")
                PARA57.Value = OIM0005row("RESERVE3")
                PARA65.Value = OIM0005row("USEDFLG")
                PARA58.Value = WW_DATENOW
                PARA59.Value = Master.USERID
                PARA60.Value = Master.USERTERMID
                PARA61.Value = WW_DATENOW
                PARA62.Value = Master.USERID
                PARA63.Value = Master.USERTERMID
                PARA64.Value = C_DEFAULT_YMD
                If OIM0005row("MYWEIGHT") <> "" Then
                    PARA66.Value = OIM0005row("MYWEIGHT")
                Else
                    PARA66.Value = "0.0"
                End If
                PARA67.Value = OIM0005row("AUTOEXTENTIONNAME")
                PARA68.Value = OIM0005row("BIGOILCODE")
                PARA69.Value = OIM0005row("BIGOILNAME")
                PARA70.Value = OIM0005row("JXTGTAGCODE1")
                PARA71.Value = OIM0005row("JXTGTAGNAME1")
                PARA72.Value = OIM0005row("JXTGTAGCODE2")
                PARA73.Value = OIM0005row("JXTGTAGNAME2")
                PARA74.Value = OIM0005row("JXTGTAGCODE3")
                PARA75.Value = OIM0005row("JXTGTAGNAME3")
                PARA76.Value = OIM0005row("JXTGTAGCODE4")
                PARA77.Value = OIM0005row("JXTGTAGNAME4")
                PARA78.Value = OIM0005row("IDSSTAGCODE")
                PARA79.Value = OIM0005row("IDSSTAGNAME")
                PARA80.Value = OIM0005row("COSMOTAGCODE")
                PARA81.Value = OIM0005row("COSMOTAGNAME")
                If OIM0005row("ALLINSPECTIONDATE") <> "" Then
                    PARA82.Value = OIM0005row("ALLINSPECTIONDATE")
                Else
                    PARA82.Value = DBNull.Value
                End If
                If OIM0005row("PREINSPECTIONDATE") <> "" Then
                    PARA83.Value = OIM0005row("PREINSPECTIONDATE")
                Else
                    PARA83.Value = DBNull.Value
                End If
                PARA84.Value = OIM0005row("OBTAINEDNAME")
                If OIM0005row("EXCLUDEDATE") <> "" Then
                    PARA85.Value = OIM0005row("EXCLUDEDATE")
                Else
                    PARA85.Value = DBNull.Value
                End If
                If OIM0005row("RETIRMENTDATE") <> "" Then
                    PARA86.Value = OIM0005row("RETIRMENTDATE")
                Else
                    PARA86.Value = DBNull.Value
                End If
                PARA87.Value = OIM0005row("JRTANKTYPE")
                PARA88.Value = OIM0005row("JXTGTANKNUMBER2")
                PARA89.Value = OIM0005row("JXTGTANKNUMBER3")
                PARA90.Value = OIM0005row("JXTGTANKNUMBER4")
                PARA91.Value = OIM0005row("SAPSHELLTANKNUMBER")
                If OIM0005row("LENGTH") <> "" Then
                    PARA92.Value = OIM0005row("LENGTH")
                Else
                    PARA92.Value = "0"
                End If
                If OIM0005row("TANKLENGTH") <> "" Then
                    PARA93.Value = OIM0005row("TANKLENGTH")
                Else
                    PARA93.Value = "0"
                End If
                If OIM0005row("MAXCALIBER") <> "" Then
                    PARA94.Value = OIM0005row("MAXCALIBER")
                Else
                    PARA94.Value = "0"
                End If
                If OIM0005row("MINCALIBER") <> "" Then
                    PARA95.Value = OIM0005row("MINCALIBER")
                Else
                    PARA95.Value = "0"
                End If
                PARA96.Value = OIM0005row("LENGTHFLG")
                If OIM0005row("INTERINSPECTYM") <> "" Then
                    PARA97.Value = OIM0005row("INTERINSPECTYM") + "/01"
                Else
                    PARA97.Value = DBNull.Value
                End If
                PARA98.Value = OIM0005row("INTERINSPECTSTATION")
                PARA99.Value = OIM0005row("INTERINSPECTORGCODE")
                If OIM0005row("SELFINSPECTYM") <> "" Then
                    PARA100.Value = OIM0005row("SELFINSPECTYM") + "/01"
                Else
                    PARA100.Value = DBNull.Value
                End If
                PARA101.Value = OIM0005row("SELFINSPECTSTATION")
                PARA102.Value = OIM0005row("SELFINSPECTORGCODE")
                PARA103.Value = OIM0005row("MIDDLEOILCODE")
                PARA104.Value = OIM0005row("MIDDLEOILNAME")
                PARA105.Value = OIM0005row("INSPECTMEMBERNAME")
                PARA106.Value = OIM0005row("SUBOPERATIONBASECODE")
                PARA107.Value = OIM0005row("SUBOPERATIONBASENAME")
                If OIM0005row("ALLINSPECTPLANYM") <> "" Then
                    PARA108.Value = OIM0005row("ALLINSPECTPLANYM") + "/01"
                Else
                    PARA108.Value = DBNull.Value
                End If
                PARA109.Value = OIM0005row("SUSPENDFLG")
                If OIM0005row("SUSPENDDATE") <> "" Then
                    PARA110.Value = OIM0005row("SUSPENDDATE")
                Else
                    PARA110.Value = DBNull.Value
                End If
                If OIM0005row("PURCHASEPRICE") <> "" Then
                    PARA111.Value = OIM0005row("PURCHASEPRICE")
                Else
                    PARA111.Value = 0
                End If
                PARA112.Value = OIM0005row("INTERNALCOATING")
                PARA113.Value = OIM0005row("SAFETYVALVE")
                PARA114.Value = OIM0005row("CENTERVALVEINFO")
                PARA115.Value = OIM0005row("DELREASONKBN")

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                '更新ジャーナル出力
                JPARA01.Value = OIM0005row("TANKNUMBER")

                Using SQLdr As SqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(OIM0005UPDtbl) Then
                        OIM0005UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            OIM0005UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    OIM0005UPDtbl.Clear()
                    OIM0005UPDtbl.Load(SQLdr)
                End Using

                For Each OIM0005UPDrow As DataRow In OIM0005UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "OIM0005L"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = OIM0005UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0005L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0005L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToOIM0005INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0005tbl_UPD()
            '入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ERRCODE) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "JOT車番コード", needsPopUp:=True)

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
    Protected Sub DetailBoxToOIM0005INPtbl(ByRef O_RTN As String)

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

        Master.CreateEmptyTable(OIM0005INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0005INProw As DataRow = OIM0005INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0005INPcol As DataColumn In OIM0005INPtbl.Columns
            If IsDBNull(OIM0005INProw.Item(OIM0005INPcol)) OrElse IsNothing(OIM0005INProw.Item(OIM0005INPcol)) Then
                Select Case OIM0005INPcol.ColumnName
                    Case "LINECNT"
                        OIM0005INProw.Item(OIM0005INPcol) = 0
                    Case "OPERATION"
                        OIM0005INProw.Item(OIM0005INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "UPDTIMSTP"
                        OIM0005INProw.Item(OIM0005INPcol) = 0
                    Case "SELECT"
                        OIM0005INProw.Item(OIM0005INPcol) = 1
                    Case "HIDDEN"
                        OIM0005INProw.Item(OIM0005INPcol) = 0
                    Case Else
                        OIM0005INProw.Item(OIM0005INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0005INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0005INProw("LINECNT"))
            Catch ex As Exception
                OIM0005INProw("LINECNT") = 0
            End Try
        End If

        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0005INProw("UPDTIMSTP") = 0
        OIM0005INProw("SELECT") = 1
        OIM0005INProw("HIDDEN") = 0

        OIM0005INProw("TANKNUMBER") = WF_TANKNUMBER.Text                                'JOT車番
        OIM0005INProw("MODEL") = WF_MODEL.Text                                          '形式
        OIM0005INProw("MODELKANA") = WF_MODELKANA.Text                                  '形式カナ
        OIM0005INProw("LOAD") = WF_LOAD.Text                                            '荷重
        OIM0005INProw("LOADUNIT") = WF_LOADUNIT.Text                                    '荷重単位
        OIM0005INProw("VOLUME") = WF_VOLUME.Text                                        '容積
        OIM0005INProw("VOLUMEUNIT") = WF_VOLUMEUNIT.Text                                '容積単位
        OIM0005INProw("MYWEIGHT") = WF_MYWEIGHT.Text                                    '自重
        OIM0005INProw("LENGTH") = WF_LENGTH.Text                                        'タンク車長
        OIM0005INProw("TANKLENGTH") = WF_TANKLENGTH.Text                                'タンク車体長
        OIM0005INProw("MAXCALIBER") = WF_MAXCALIBER.Text                                '最大口径
        OIM0005INProw("MINCALIBER") = WF_MINCALIBER.Text                                '最小口径
        OIM0005INProw("LENGTHFLG") = WF_LENGTHFLG.Text                                  '長さフラグ
        OIM0005INProw("ORIGINOWNERCODE") = WF_ORIGINOWNERCODE.Text                      '原籍所有者C
        OIM0005INProw("ORIGINOWNERNAME") = WF_ORIGINOWNERCODE_TEXT.Text                 '原籍所有者
        OIM0005INProw("OWNERCODE") = WF_OWNERCODE.Text                                  '名義所有者C
        OIM0005INProw("OWNERNAME") = WF_OWNERCODE_TEXT.Text                             '名義所有者
        OIM0005INProw("LEASECODE") = WF_LEASECODE.Text                                  'リース先C
        OIM0005INProw("LEASENAME") = WF_LEASECODE_TEXT.Text                             'リース先
        OIM0005INProw("LEASECLASS") = WF_LEASECLASS.Text                                '請負リース区分C
        OIM0005INProw("LEASECLASSNAME") = WF_LEASECLASS_TEXT.Text                       'リース区分
        OIM0005INProw("AUTOEXTENTION") = WF_AUTOEXTENTION.Text                          '自動延長
        OIM0005INProw("AUTOEXTENTIONNAME") = WF_AUTOEXTENTION_TEXT.Text                 '自動延長名
        OIM0005INProw("LEASESTYMD") = WF_LEASESTYMD.Text                                'リース開始年月日
        OIM0005INProw("LEASEENDYMD") = WF_LEASEENDYMD.Text                              'リース満了年月日
        OIM0005INProw("USERCODE") = WF_USERCODE.Text                                    '第三者使用者C
        OIM0005INProw("USERNAME") = WF_USERCODE_TEXT.Text                               '第三者使用者
        OIM0005INProw("CURRENTSTATIONCODE") = WF_CURRENTSTATIONCODE.Text                '原常備駅C
        OIM0005INProw("CURRENTSTATIONNAME") = WF_CURRENTSTATIONCODE_TEXT.Text           '原常備駅
        OIM0005INProw("EXTRADINARYSTATIONCODE") = WF_EXTRADINARYSTATIONCODE.Text        '臨時常備駅C
        OIM0005INProw("EXTRADINARYSTATIONNAME") = WF_EXTRADINARYSTATIONCODE_TEXT.Text   '臨時常備駅
        OIM0005INProw("USERLIMIT") = WF_USERLIMIT.Text                                  '第三者使用期限
        OIM0005INProw("LIMITTEXTRADIARYSTATION") = WF_LIMITTEXTRADIARYSTATION.Text      '臨時常備駅期限
        OIM0005INProw("DEDICATETYPECODE") = WF_DEDICATETYPECODE.Text                    '原専用種別C
        OIM0005INProw("DEDICATETYPENAME") = WF_DEDICATETYPECODE_TEXT.Text               '原専用種別
        OIM0005INProw("EXTRADINARYTYPECODE") = WF_EXTRADINARYTYPECODE.Text              '臨時専用種別C
        OIM0005INProw("EXTRADINARYTYPENAME") = WF_EXTRADINARYTYPECODE_TEXT.Text         '臨時専用種別
        OIM0005INProw("EXTRADINARYLIMIT") = WF_EXTRADINARYLIMIT.Text                    '臨時専用期限
        OIM0005INProw("BIGOILCODE") = WF_BIGOILCODE.Text                                '油種大分類コード
        OIM0005INProw("BIGOILNAME") = WF_BIGOILCODE_TEXT.Text                           '油種大分類名
        OIM0005INProw("MIDDLEOILCODE") = WF_MIDDLEOILCODE.Text                          '油種中分類コード
        OIM0005INProw("MIDDLEOILNAME") = WF_MIDDLEOILCODE_TEXT.Text                     '油種中分類名
        OIM0005INProw("OPERATIONBASECODE") = WF_OPERATIONBASECODE.Text                  '運用基地C
        OIM0005INProw("OPERATIONBASENAME") = WF_OPERATIONBASECODE_TEXT.Text             '運用場所
        OIM0005INProw("COLORCODE") = WF_COLORCODE.Text                                  '塗色C
        OIM0005INProw("COLORNAME") = WF_COLORCODE_TEXT.Text                             '塗色
        OIM0005INProw("MARKCODE") = WF_MARKCODE.Text                                    'マークコード
        OIM0005INProw("MARKNAME") = WF_MARKCODE_TEXT.Text                               'マーク名
        OIM0005INProw("JXTGTAGCODE1") = WF_JXTGTAGCODE1.Text                            'JXTG仙台タグコード
        OIM0005INProw("JXTGTAGNAME1") = WF_JXTGTAGCODE1_TEXT.Text                       'JXTG仙台タグ名
        OIM0005INProw("JXTGTAGCODE2") = WF_JXTGTAGCODE2.Text                            'JXTG千葉タグコード
        OIM0005INProw("JXTGTAGNAME2") = WF_JXTGTAGCODE2_TEXT.Text                       'JXTG千葉タグ名
        OIM0005INProw("JXTGTAGCODE3") = WF_JXTGTAGCODE3.Text                            'JXTG川崎タグコード
        OIM0005INProw("JXTGTAGNAME3") = WF_JXTGTAGCODE3_TEXT.Text                       'JXTG川崎タグ名
        OIM0005INProw("JXTGTAGCODE4") = WF_JXTGTAGCODE4.Text                            'JXTG根岸タグコード
        OIM0005INProw("JXTGTAGNAME4") = WF_JXTGTAGCODE4_TEXT.Text                       'JXTG根岸タグ名
        OIM0005INProw("IDSSTAGCODE") = WF_IDSSTAGCODE.Text                              '出光昭シタグコード
        OIM0005INProw("IDSSTAGNAME") = WF_IDSSTAGCODE_TEXT.Text                         '出光昭シタグ名
        OIM0005INProw("COSMOTAGCODE") = WF_COSMOTAGCODE.Text                            'コスモタグコード
        OIM0005INProw("COSMOTAGNAME") = WF_COSMOTAGCODE_TEXT.Text                       'コスモタグ名
        OIM0005INProw("RESERVE1") = WF_RESERVE1.Text                                    '予備1
        OIM0005INProw("RESERVE2") = WF_RESERVE2.Text                                    '予備2
        OIM0005INProw("JRINSPECTIONDATE") = WF_JRINSPECTIONDATE.Text                    '次回交検年月日(JR）
        OIM0005INProw("INSPECTIONDATE") = WF_INSPECTIONDATE.Text                        '次回交検年月日
        OIM0005INProw("JRSPECIFIEDDATE") = WF_JRSPECIFIEDDATE.Text                      '次回指定年月日(JR)
        OIM0005INProw("SPECIFIEDDATE") = WF_SPECIFIEDDATE.Text                          '次回指定年月日
        OIM0005INProw("JRALLINSPECTIONDATE") = WF_JRALLINSPECTIONDATE.Text              '次回全検年月日(JR) 
        OIM0005INProw("ALLINSPECTIONDATE") = WF_ALLINSPECTIONDATE.Text                  '次回全検年月日
        OIM0005INProw("PREINSPECTIONDATE") = WF_PREINSPECTIONDATE.Text                  '前回全検年月日
        OIM0005INProw("GETDATE") = WF_GETDATE.Text                                      '取得年月日
        OIM0005INProw("TRANSFERDATE") = WF_TRANSFERDATE.Text                            '車籍編入年月日
        OIM0005INProw("OBTAINEDCODE") = WF_OBTAINEDCODE.Text                            '取得先C
        OIM0005INProw("OBTAINEDNAME") = WF_OBTAINEDCODE_TEXT.Text                       '取得先名
        OIM0005INProw("PROGRESSYEAR") = WF_PROGRESSYEAR.Text                            '現在経年
        OIM0005INProw("NEXTPROGRESSYEAR") = WF_NEXTPROGRESSYEAR.Text                    '次回全検時経年
        OIM0005INProw("EXCLUDEDATE") = WF_EXCLUDEDATE.Text                              '車籍除外年月日
        OIM0005INProw("RETIRMENTDATE") = WF_RETIRMENTDATE.Text                          '資産除却年月日
        OIM0005INProw("JRTANKNUMBER") = WF_JRTANKNUMBER.Text                            'JR車番
        OIM0005INProw("JRTANKTYPE") = WF_JRTANKTYPE.Text                                'JR車種コード
        OIM0005INProw("OLDTANKNUMBER") = WF_OLDTANKNUMBER.Text                          '旧JOT車番
        OIM0005INProw("OTTANKNUMBER") = WF_OTTANKNUMBER.Text                            'OT車番
        OIM0005INProw("JXTGTANKNUMBER1") = WF_JXTGTANKNUMBER1.Text                      'JXTG仙台車番
        OIM0005INProw("JXTGTANKNUMBER2") = WF_JXTGTANKNUMBER2.Text                      'JXTG千葉車番
        OIM0005INProw("JXTGTANKNUMBER3") = WF_JXTGTANKNUMBER3.Text                      'JXTG川崎車番
        OIM0005INProw("JXTGTANKNUMBER4") = WF_JXTGTANKNUMBER4.Text                      'JXTG根岸車番
        OIM0005INProw("COSMOTANKNUMBER") = WF_COSMOTANKNUMBER.Text                      'コスモ車番
        OIM0005INProw("FUJITANKNUMBER") = WF_FUJITANKNUMBER.Text                        '富士石油車番
        OIM0005INProw("SHELLTANKNUMBER") = WF_SHELLTANKNUMBER.Text                      '出光昭シ車番
        OIM0005INProw("SAPSHELLTANKNUMBER") = WF_SAPSHELLTANKNUMBER.Text                '出光昭シSAP車番
        OIM0005INProw("RESERVE3") = WF_RESERVE3.Text                                    '予備
        OIM0005INProw("USEDFLG") = WF_USEDFLG.Text                                      '利用フラグ
        OIM0005INProw("DELFLG") = WF_DELFLG.Text                                        '削除フラグ
        OIM0005INProw("INTERINSPECTYM") = WF_INTERINSPECTYM.Text                        '中間点検年月
        OIM0005INProw("INTERINSPECTSTATION") = WF_INTERINSPECTSTATION.Text              '中間点検場所
        OIM0005INProw("INTERINSPECTORGCODE") = WF_INTERINSPECTORGCODE.Text              '中間点検実施者
        OIM0005INProw("SELFINSPECTYM") = WF_SELFINSPECTYM.Text                          '自主点検年月
        OIM0005INProw("SELFINSPECTSTATION") = WF_SELFINSPECTSTATION.Text                '自主点検場所
        OIM0005INProw("SELFINSPECTORGCODE") = WF_SELFINSPECTORGCODE.Text                '自主点検実施者
        OIM0005INProw("INSPECTMEMBERNAME") = WF_INSPECTMEMBERNAME.Text                  '点検実施者(社員名)

        OIM0005INProw("SUBOPERATIONBASECODE") = WF_SUBOPERATIONBASECODE.Text            '運用基地C（サブ）
        OIM0005INProw("SUBOPERATIONBASENAME") = WF_SUBOPERATIONBASECODE_TEXT.Text       '運用基地（サブ）
        OIM0005INProw("ALLINSPECTPLANYM") = WF_ALLINSPECTPLANYM.Text                    '全検計画年月
        OIM0005INProw("SUSPENDFLG") = WF_SUSPENDFLG.Text                                '休車フラグ
        OIM0005INProw("SUSPENDDATE") = WF_SUSPENDDATE.Text                              '休車日
        OIM0005INProw("PURCHASEPRICE") = WF_PURCHASEPRICE.Text                          '取得価格
        OIM0005INProw("INTERNALCOATING") = WF_INTERNALCOATING.Text                      '内部塗装
        OIM0005INProw("SAFETYVALVE") = WF_SAFETYVALVE.Text                              '安全弁
        OIM0005INProw("CENTERVALVEINFO") = WF_CENTERVALVEINFO.Text                      'センターバルブ情報
        OIM0005INProw("DELREASONKBN") = WF_DELREASONKBN.Text                            '削除理由区分

        '○ チェック用テーブルに登録する
        OIM0005INPtbl.Rows.Add(OIM0005INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

        ''○ DetailBoxをINPtblへ退避
        'DetailBoxToOIM0005INPtbl(WW_ERR_SW)
        'If Not isNormal(WW_ERR_SW) Then
        '    Exit Sub
        'End If

        'Dim inputChangeFlg As Boolean = True
        'Dim OIM0005INProw As DataRow = OIM0005INPtbl.Rows(0)

        '' 既存レコードとの比較
        'For Each OIM0005row As DataRow In OIM0005tbl.Rows
        '    ' KEY項目が等しい時
        '    If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") Then
        '        ' KEY項目以外の項目の差異をチェック
        '        If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") AndAlso
        '            OIM0005row("MODEL") = OIM0005INProw("MODEL") AndAlso
        '            OIM0005row("MODELKANA") = OIM0005INProw("MODELKANA") AndAlso
        '            OIM0005row("LOAD") = OIM0005INProw("LOAD") AndAlso
        '            OIM0005row("LOADUNIT") = OIM0005INProw("LOADUNIT") AndAlso
        '            OIM0005row("VOLUME") = OIM0005INProw("VOLUME") AndAlso
        '            OIM0005row("VOLUMEUNIT") = OIM0005INProw("VOLUMEUNIT") AndAlso
        '            OIM0005row("MYWEIGHT") = OIM0005INProw("MYWEIGHT") AndAlso
        '            OIM0005row("LENGTH") = OIM0005INProw("LENGTH") AndAlso
        '            OIM0005row("TANKLENGTH") = OIM0005INProw("TANKLENGTH") AndAlso
        '            OIM0005row("MAXCALIBER") = OIM0005INProw("MAXCALIBER") AndAlso
        '            OIM0005row("MINCALIBER") = OIM0005INProw("MINCALIBER") AndAlso
        '            OIM0005row("LENGTHFLG") = OIM0005INProw("LENGTHFLG") AndAlso
        '            OIM0005row("ORIGINOWNERCODE") = OIM0005INProw("ORIGINOWNERCODE") AndAlso
        '            OIM0005row("ORIGINOWNERNAME") = OIM0005INProw("ORIGINOWNERNAME") AndAlso
        '            OIM0005row("OWNERCODE") = OIM0005INProw("OWNERCODE") AndAlso
        '            OIM0005row("OWNERNAME") = OIM0005INProw("OWNERNAME") AndAlso
        '            OIM0005row("LEASECODE") = OIM0005INProw("LEASECODE") AndAlso
        '            OIM0005row("LEASENAME") = OIM0005INProw("LEASENAME") AndAlso
        '            OIM0005row("LEASECLASS") = OIM0005INProw("LEASECLASS") AndAlso
        '            OIM0005row("LEASECLASSNAME") = OIM0005INProw("LEASECLASSNAME") AndAlso
        '            OIM0005row("AUTOEXTENTION") = OIM0005INProw("AUTOEXTENTION") AndAlso
        '            OIM0005row("AUTOEXTENTIONNAME") = OIM0005INProw("AUTOEXTENTIONNAME") AndAlso
        '            OIM0005row("LEASESTYMD") = OIM0005INProw("LEASESTYMD") AndAlso
        '            OIM0005row("LEASEENDYMD") = OIM0005INProw("LEASEENDYMD") AndAlso
        '            OIM0005row("USERCODE") = OIM0005INProw("USERCODE") AndAlso
        '            OIM0005row("USERNAME") = OIM0005INProw("USERNAME") AndAlso
        '            OIM0005row("CURRENTSTATIONCODE") = OIM0005INProw("CURRENTSTATIONCODE") AndAlso
        '            OIM0005row("CURRENTSTATIONNAME") = OIM0005INProw("CURRENTSTATIONNAME") AndAlso
        '            OIM0005row("EXTRADINARYSTATIONCODE") = OIM0005INProw("EXTRADINARYSTATIONCODE") AndAlso
        '            OIM0005row("EXTRADINARYSTATIONNAME") = OIM0005INProw("EXTRADINARYSTATIONNAME") AndAlso
        '            OIM0005row("USERLIMIT") = OIM0005INProw("USERLIMIT") AndAlso
        '            OIM0005row("LIMITTEXTRADIARYSTATION") = OIM0005INProw("LIMITTEXTRADIARYSTATION") AndAlso
        '            OIM0005row("DEDICATETYPECODE") = OIM0005INProw("DEDICATETYPECODE") AndAlso
        '            OIM0005row("DEDICATETYPENAME") = OIM0005INProw("DEDICATETYPENAME") AndAlso
        '            OIM0005row("EXTRADINARYTYPECODE") = OIM0005INProw("EXTRADINARYTYPECODE") AndAlso
        '            OIM0005row("EXTRADINARYTYPENAME") = OIM0005INProw("EXTRADINARYTYPENAME") AndAlso
        '            OIM0005row("EXTRADINARYLIMIT") = OIM0005INProw("EXTRADINARYLIMIT") AndAlso
        '            OIM0005row("BIGOILCODE") = OIM0005INProw("BIGOILCODE") AndAlso
        '            OIM0005row("BIGOILNAME") = OIM0005INProw("BIGOILNAME") AndAlso
        '            OIM0005row("MIDDLEOILCODE") = OIM0005INProw("MIDDLEOILCODE") AndAlso
        '            OIM0005row("MIDDLEOILNAME") = OIM0005INProw("MIDDLEOILNAME") AndAlso
        '            OIM0005row("OPERATIONBASECODE") = OIM0005INProw("OPERATIONBASECODE") AndAlso
        '            OIM0005row("OPERATIONBASENAME") = OIM0005INProw("OPERATIONBASENAME") AndAlso
        '            OIM0005row("SUBOPERATIONBASECODE") = OIM0005INProw("SUBOPERATIONBASECODE") AndAlso
        '            OIM0005row("SUBOPERATIONBASENAME") = OIM0005INProw("SUBOPERATIONBASENAME") AndAlso
        '            OIM0005row("COLORCODE") = OIM0005INProw("COLORCODE") AndAlso
        '            OIM0005row("COLORNAME") = OIM0005INProw("COLORNAME") AndAlso
        '            OIM0005row("MARKCODE") = OIM0005INProw("MARKCODE") AndAlso
        '            OIM0005row("MARKNAME") = OIM0005INProw("MARKNAME") AndAlso
        '            OIM0005row("JXTGTAGCODE1") = OIM0005INProw("JXTGTAGCODE1") AndAlso
        '            OIM0005row("JXTGTAGNAME1") = OIM0005INProw("JXTGTAGNAME1") AndAlso
        '            OIM0005row("JXTGTAGCODE2") = OIM0005INProw("JXTGTAGCODE2") AndAlso
        '            OIM0005row("JXTGTAGNAME2") = OIM0005INProw("JXTGTAGNAME2") AndAlso
        '            OIM0005row("JXTGTAGCODE3") = OIM0005INProw("JXTGTAGCODE3") AndAlso
        '            OIM0005row("JXTGTAGNAME3") = OIM0005INProw("JXTGTAGNAME3") AndAlso
        '            OIM0005row("JXTGTAGCODE4") = OIM0005INProw("JXTGTAGCODE4") AndAlso
        '            OIM0005row("JXTGTAGNAME4") = OIM0005INProw("JXTGTAGNAME4") AndAlso
        '            OIM0005row("IDSSTAGCODE") = OIM0005INProw("IDSSTAGCODE") AndAlso
        '            OIM0005row("IDSSTAGNAME") = OIM0005INProw("IDSSTAGNAME") AndAlso
        '            OIM0005row("COSMOTAGCODE") = OIM0005INProw("COSMOTAGCODE") AndAlso
        '            OIM0005row("COSMOTAGNAME") = OIM0005INProw("COSMOTAGNAME") AndAlso
        '            OIM0005row("RESERVE1") = OIM0005INProw("RESERVE1") AndAlso
        '            OIM0005row("RESERVE2") = OIM0005INProw("RESERVE2") AndAlso
        '            OIM0005row("JRINSPECTIONDATE") = OIM0005INProw("JRINSPECTIONDATE") AndAlso
        '            OIM0005row("INSPECTIONDATE") = OIM0005INProw("INSPECTIONDATE") AndAlso
        '            OIM0005row("JRSPECIFIEDDATE") = OIM0005INProw("JRSPECIFIEDDATE") AndAlso
        '            OIM0005row("SPECIFIEDDATE") = OIM0005INProw("SPECIFIEDDATE") AndAlso
        '            OIM0005row("JRALLINSPECTIONDATE") = OIM0005INProw("JRALLINSPECTIONDATE") AndAlso
        '            OIM0005row("ALLINSPECTIONDATE") = OIM0005INProw("ALLINSPECTIONDATE") AndAlso
        '            OIM0005row("PREINSPECTIONDATE") = OIM0005INProw("PREINSPECTIONDATE") AndAlso
        '            OIM0005row("GETDATE") = OIM0005INProw("GETDATE") AndAlso
        '            OIM0005row("TRANSFERDATE") = OIM0005INProw("TRANSFERDATE") AndAlso
        '            OIM0005row("OBTAINEDCODE") = OIM0005INProw("OBTAINEDCODE") AndAlso
        '            OIM0005row("OBTAINEDNAME") = OIM0005INProw("OBTAINEDNAME") AndAlso
        '            OIM0005row("PROGRESSYEAR") = OIM0005INProw("PROGRESSYEAR") AndAlso
        '            OIM0005row("NEXTPROGRESSYEAR") = OIM0005INProw("NEXTPROGRESSYEAR") AndAlso
        '            OIM0005row("EXCLUDEDATE") = OIM0005INProw("EXCLUDEDATE") AndAlso
        '            OIM0005row("RETIRMENTDATE") = OIM0005INProw("RETIRMENTDATE") AndAlso
        '            OIM0005row("JRTANKNUMBER") = OIM0005INProw("JRTANKNUMBER") AndAlso
        '            OIM0005row("JRTANKTYPE") = OIM0005INProw("JRTANKTYPE") AndAlso
        '            OIM0005row("OLDTANKNUMBER") = OIM0005INProw("OLDTANKNUMBER") AndAlso
        '            OIM0005row("OTTANKNUMBER") = OIM0005INProw("OTTANKNUMBER") AndAlso
        '            OIM0005row("JXTGTANKNUMBER1") = OIM0005INProw("JXTGTANKNUMBER1") AndAlso
        '            OIM0005row("JXTGTANKNUMBER2") = OIM0005INProw("JXTGTANKNUMBER2") AndAlso
        '            OIM0005row("JXTGTANKNUMBER3") = OIM0005INProw("JXTGTANKNUMBER3") AndAlso
        '            OIM0005row("JXTGTANKNUMBER4") = OIM0005INProw("JXTGTANKNUMBER4") AndAlso
        '            OIM0005row("COSMOTANKNUMBER") = OIM0005INProw("COSMOTANKNUMBER") AndAlso
        '            OIM0005row("FUJITANKNUMBER") = OIM0005INProw("FUJITANKNUMBER") AndAlso
        '            OIM0005row("SHELLTANKNUMBER") = OIM0005INProw("SHELLTANKNUMBER") AndAlso
        '            OIM0005row("SAPSHELLTANKNUMBER") = OIM0005INProw("SAPSHELLTANKNUMBER") AndAlso
        '            OIM0005row("RESERVE3") = OIM0005INProw("RESERVE3") AndAlso
        '            OIM0005row("USEDFLG") = OIM0005INProw("USEDFLG") AndAlso
        '            OIM0005row("INTERINSPECTYM") = OIM0005INProw("INTERINSPECTYM") AndAlso
        '            OIM0005row("INTERINSPECTSTATION") = OIM0005INProw("INTERINSPECTSTATION") AndAlso
        '            OIM0005row("INTERINSPECTORGCODE") = OIM0005INProw("INTERINSPECTORGCODE") AndAlso
        '            OIM0005row("SELFINSPECTYM") = OIM0005INProw("SELFINSPECTYM") AndAlso
        '            OIM0005row("SELFINSPECTSTATION") = OIM0005INProw("SELFINSPECTSTATION") AndAlso
        '            OIM0005row("SELFINSPECTORGCODE") = OIM0005INProw("SELFINSPECTORGCODE") AndAlso
        '            OIM0005row("INSPECTMEMBERNAME") = OIM0005INProw("INSPECTMEMBERNAME") AndAlso
        '            OIM0005row("ALLINSPECTPLANYM") = OIM0005INProw("ALLINSPECTPLANYM") AndAlso
        '            OIM0005row("SUSPENDFLG") = OIM0005INProw("SUSPENDFLG") AndAlso
        '            OIM0005row("SUSPENDDATE") = OIM0005INProw("SUSPENDDATE") AndAlso
        '            OIM0005row("PURCHASEPRICE") = OIM0005INProw("PURCHASEPRICE") AndAlso
        '            OIM0005row("INTERNALCOATING") = OIM0005INProw("INTERNALCOATING") AndAlso
        '            OIM0005row("SAFETYVALVE") = OIM0005INProw("SAFETYVALVE") AndAlso
        '            OIM0005row("CENTERVALVEINFO") = OIM0005INProw("CENTERVALVEINFO") AndAlso
        '            OIM0005row("DELFLG") = OIM0005INProw("DELFLG") AndAlso
        '            OIM0005row("DELREASONKBN") = OIM0005INProw("DELREASONKBN") Then
        '            ' 変更がないときは、入力変更フラグをOFFにする
        '            inputChangeFlg = False
        '        End If

        '        Exit For

        '    End If
        'Next

        'If inputChangeFlg Then
        '    '変更がある場合は、確認ダイアログを表示
        '    Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
        '        needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOk")
        'Else
        '    '変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
        '    WF_CLEAR_ConfirmOkClick()
        'End If

        WF_CLEAR_ConfirmOkClick()

    End Sub

    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
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
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            Select Case OIM0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0005tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""                    'LINECNT

        WF_TANKNUMBER.Text = ""                     'JOT車番
        WF_MODEL.Text = ""                          '形式
        WF_MODELKANA.Text = ""                      '形式カナ
        WF_LOAD.Text = ""                           '荷重
        WF_LOADUNIT.Text = ""                       '荷重単位
        WF_VOLUME.Text = ""                         '容積
        WF_VOLUMEUNIT.Text = ""                     '容積単位
        WF_MYWEIGHT.Text = ""                       '自重
        WF_LENGTH.Text = ""                         'タンク車長
        WF_TANKLENGTH.Text = ""                     'タンク車体長
        WF_MAXCALIBER.Text = ""                     '最大口径
        WF_MINCALIBER.Text = ""                     '最小口径
        WF_LENGTHFLG.Text = ""                      '長さフラグ
        WF_ORIGINOWNERCODE.Text = ""                '原籍所有者C
        WF_ORIGINOWNERCODE_TEXT.Text = ""           '原籍所有者
        WF_OWNERCODE.Text = ""                      '名義所有者C
        WF_OWNERCODE_TEXT.Text = ""                 '名義所有者
        WF_LEASECODE.Text = ""                      'リース先C
        WF_LEASECODE_TEXT.Text = ""                 'リース先
        WF_LEASECLASS.Text = ""                     '請負リース区分C
        WF_LEASECLASS_TEXT.Text = ""                'リース区分
        WF_AUTOEXTENTION.Text = ""                  '自動延長
        WF_AUTOEXTENTION_TEXT.Text = ""             '自動延長名
        WF_LEASESTYMD.Text = ""                     'リース開始年月日
        WF_LEASEENDYMD.Text = ""                    'リース満了年月日
        WF_USERCODE.Text = ""                       '第三者使用者C
        WF_USERCODE_TEXT.Text = ""                  '第三者使用者
        WF_CURRENTSTATIONCODE.Text = ""             '原常備駅C
        WF_CURRENTSTATIONCODE_TEXT.Text = ""        '原常備駅
        WF_EXTRADINARYSTATIONCODE.Text = ""         '臨時常備駅C
        WF_EXTRADINARYSTATIONCODE_TEXT.Text = ""    '臨時常備駅
        WF_USERLIMIT.Text = ""                      '第三者使用期限
        WF_LIMITTEXTRADIARYSTATION.Text = ""        '臨時常備駅期限
        WF_DEDICATETYPECODE.Text = ""               '原専用種別C
        WF_DEDICATETYPECODE_TEXT.Text = ""          '原専用種別
        WF_EXTRADINARYTYPECODE.Text = ""            '臨時専用種別C
        WF_EXTRADINARYTYPECODE_TEXT.Text = ""       '臨時専用種別
        WF_EXTRADINARYLIMIT.Text = ""               '臨時専用期限
        WF_BIGOILCODE.Text = ""                     '油種大分類コード
        WF_BIGOILCODE_TEXT.Text = ""                '油種大分類名
        WF_MIDDLEOILCODE.Text = ""                  '油種中分類コード
        WF_MIDDLEOILCODE_TEXT.Text = ""             '油種中分類名
        WF_OPERATIONBASECODE.Text = ""              '運用基地C
        WF_OPERATIONBASECODE_TEXT.Text = ""         '運用場所
        WF_SUBOPERATIONBASECODE.Text = ""           '運用基地C（サブ）
        WF_SUBOPERATIONBASECODE_TEXT.Text = ""      '運用場所（サブ）
        WF_COLORCODE.Text = ""                      '塗色C
        WF_COLORCODE_TEXT.Text = ""                 '塗色
        WF_MARKCODE.Text = ""                       'マークコード
        WF_MARKCODE_TEXT.Text = ""                  'マーク名
        WF_JXTGTAGCODE1.Text = ""                   'JXTG仙台タグコード
        WF_JXTGTAGCODE1_TEXT.Text = ""              'JXTG仙台タグ名
        WF_JXTGTAGCODE2.Text = ""                   'JXTG千葉タグコード
        WF_JXTGTAGCODE2_TEXT.Text = ""              'JXTG千葉タグ名
        WF_JXTGTAGCODE3.Text = ""                   'JXTG川崎タグコード
        WF_JXTGTAGCODE3_TEXT.Text = ""              'JXTG川崎タグ名
        WF_JXTGTAGCODE4.Text = ""                   'JXTG根岸タグコード
        WF_JXTGTAGCODE4_TEXT.Text = ""              'JXTG根岸タグ名
        WF_IDSSTAGCODE.Text = ""                    '出光昭シタグコード
        WF_IDSSTAGCODE_TEXT.Text = ""               '出光昭シタグ名
        WF_COSMOTAGCODE.Text = ""                   'コスモタグコード
        WF_COSMOTAGCODE_TEXT.Text = ""              'コスモタグ名
        WF_RESERVE1.Text = ""                       '予備1
        WF_RESERVE2.Text = ""                       '予備2
        WF_JRINSPECTIONDATE.Text = ""               '次回交検年月日(JR）
        WF_INSPECTIONDATE.Text = ""                 '次回交検年月日
        WF_JRSPECIFIEDDATE.Text = ""                '次回指定年月日(JR)
        WF_SPECIFIEDDATE.Text = ""                  '次回指定年月日
        WF_JRALLINSPECTIONDATE.Text = ""            '次回全検年月日(JR) 
        WF_ALLINSPECTIONDATE.Text = ""              '次回全検年月日
        WF_PREINSPECTIONDATE.Text = ""              '前回全検年月日
        WF_GETDATE.Text = ""                        '取得年月日
        WF_TRANSFERDATE.Text = ""                   '車籍編入年月日
        WF_OBTAINEDCODE.Text = ""                   '取得先C
        WF_OBTAINEDCODE_TEXT.Text = ""              '取得先名
        WF_PROGRESSYEAR.Text = ""                   '現在経年
        WF_NEXTPROGRESSYEAR.Text = ""               '次回全検時経年
        WF_EXCLUDEDATE.Text = ""                    '車籍除外年月日
        WF_RETIRMENTDATE.Text = ""                  '資産除却年月日
        WF_JRTANKNUMBER.Text = ""                   'JR車番
        WF_JRTANKTYPE.Text = ""                     'JR車種コード
        WF_OLDTANKNUMBER.Text = ""                  '旧JOT車番
        WF_OTTANKNUMBER.Text = ""                   'OT車番
        WF_JXTGTANKNUMBER1.Text = ""                'JXTG仙台車番
        WF_JXTGTANKNUMBER2.Text = ""                'JXTG千葉車番
        WF_JXTGTANKNUMBER3.Text = ""                'JXTG川崎車番
        WF_JXTGTANKNUMBER4.Text = ""                'JXTG根岸車番
        WF_COSMOTANKNUMBER.Text = ""                'コスモ車番
        WF_FUJITANKNUMBER.Text = ""                 '富士石油車番
        WF_SHELLTANKNUMBER.Text = ""                '出光昭シ車番
        WF_SAPSHELLTANKNUMBER.Text = ""             '出光昭シSAP車番
        WF_RESERVE3.Text = ""                       '予備
        WF_USEDFLG.Text = ""                        '利用フラグ
        WF_INTERINSPECTYM.Text = ""                 '中間点検年月
        WF_INTERINSPECTSTATION.Text = ""            '中間点検場所
        WF_INTERINSPECTORGCODE.Text = ""            '中間点検実施者
        WF_SELFINSPECTYM.Text = ""                  '自主点検年月
        WF_SELFINSPECTSTATION.Text = ""             '自主点検場所
        WF_SELFINSPECTORGCODE.Text = ""             '自主点検実施者
        WF_INSPECTMEMBERNAME.Text = ""              '点検実施者(社員名)
        WF_ALLINSPECTPLANYM.Text = ""               '全検計画年月
        WF_SUSPENDFLG.Text = ""                     '休車フラグ
        WF_SUSPENDFLG_TEXT.Text = ""                '休車フラグ(名)
        WF_SUSPENDDATE.Text = ""                    '休車日
        WF_PURCHASEPRICE.Text = ""                  '取得価格
        WF_INTERNALCOATING.Text = ""                '内部塗装
        WF_INTERNALCOATING_TEXT.Text = ""           '内部塗装(名)
        WF_SAFETYVALVE.Text = ""                    '安全弁
        WF_CENTERVALVEINFO.Text = ""                'センターバルブ情報
        WF_DELFLG.Text = ""                         '削除フラグ
        WF_DELREASONKBN.Text = ""                   '削除理由区分
        WF_DELREASONKBN_TEXT.Text = ""              '削除理由区分(名)

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
                            Case "WF_LEASESTYMD"                                    'リース開始年月日
                                .WF_Calendar.Text = WF_LEASESTYMD.Text
                            Case "WF_LEASEENDYMD"                                   'リース満了年月日
                                .WF_Calendar.Text = WF_LEASEENDYMD.Text
                            Case "WF_USERLIMIT"                                     '第三者使用期限
                                .WF_Calendar.Text = WF_USERLIMIT.Text
                            Case "WF_LIMITTEXTRADIARYSTATION"                       '臨時常備駅期限
                                .WF_Calendar.Text = WF_LIMITTEXTRADIARYSTATION.Text
                            Case "WF_EXTRADINARYLIMIT"                              '臨時専用期限
                                .WF_Calendar.Text = WF_EXTRADINARYLIMIT.Text
                            Case "WF_JRINSPECTIONDATE"                              '次回交検年月日(JR）
                                .WF_Calendar.Text = WF_JRINSPECTIONDATE.Text
                            Case "WF_INSPECTIONDATE"                                '次回交検年月日
                                .WF_Calendar.Text = WF_INSPECTIONDATE.Text
                            Case "WF_JRSPECIFIEDDATE"                               '次回指定年月日(JR)
                                .WF_Calendar.Text = WF_JRSPECIFIEDDATE.Text
                            Case "WF_SPECIFIEDDATE"                                 '次回指定年月日
                                .WF_Calendar.Text = WF_SPECIFIEDDATE.Text
                            Case "WF_JRALLINSPECTIONDATE"                           '次回全検年月日(JR)
                                .WF_Calendar.Text = WF_JRALLINSPECTIONDATE.Text
                            Case "WF_ALLINSPECTIONDATE"                             '次回全検年月日
                                .WF_Calendar.Text = WF_ALLINSPECTIONDATE.Text
                            Case "WF_PREINSPECTIONDATE"                             '前回全検年月日
                                .WF_Calendar.Text = WF_PREINSPECTIONDATE.Text
                            Case "WF_GETDATE"                                       '取得年月日
                                .WF_Calendar.Text = WF_GETDATE.Text
                            Case "WF_TRANSFERDATE"                                  '車籍編入年月日
                                .WF_Calendar.Text = WF_TRANSFERDATE.Text
                            Case "WF_EXCLUDEDATE"                                   '車籍除外年月日
                                .WF_Calendar.Text = WF_EXCLUDEDATE.Text
                            Case "WF_RETIRMENTDATE"                                 '資産除却年月日
                                .WF_Calendar.Text = WF_RETIRMENTDATE.Text
                            Case "WF_INTERINSPECTYM"                                '中間点検年月
                                .WF_Calendar.Text = WF_INTERINSPECTYM.Text + "/01"
                            Case "WF_SELFINSPECTYM"                                 '自主点検年月
                                .WF_Calendar.Text = WF_SELFINSPECTYM.Text + "/01"
                        End Select
                        .ActiveCalendar()

                    Case Else
                        'それ以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            'リース先C
                            Case "WF_LEASECODE"
                                prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL

                            '自動延長
                            Case "WF_AUTOEXTENTION"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION")

                            'マークコード
                            Case "WF_MARKCODE"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MARKCODE")

                            'JXTG千葉タグコード, 出光昭シタグコード
                            Case "WF_JXTGTAGCODE2", "WF_IDSSTAGCODE"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TAGCODE")

                            'JR車種コード
                            Case "WF_JRTANKTYPE"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "JRTANKTYPE")

                            '長さフラグ
                            Case "WF_LENGTHFLG"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LENGTHFLG")

                            '中間/自主点検実施者
                            Case "WF_INTERINSPECTORGCODE", "WF_SELFINSPECTORGCODE"
                                prmData = work.CreateORGParam(Master.USERCAMP)

                            '削除理由区分
                            Case "WF_DELREASONKBN"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELREASONKBN")

                            '休車フラグ
                            Case "WF_SUSPENDFLG"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SUSPENDFLG")

                            '内部塗装
                            Case "WF_INTERNALCOATING"
                                prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "INTERNALCOATING")
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
            '削除フラグ
            Case "WF_DELFLG"
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)
                WF_DELFLG.Focus()
            '削除理由区分
            Case "WF_DELREASONKBN"
                CODENAME_get("DELREASONKBN", WF_DELREASONKBN.Text, WF_DELREASONKBN_TEXT.Text, WW_RTN_SW)
                WF_DELREASONKBN.Focus()
            'JOT車番, 形式
            Case "WF_TANKNUMBER", "WF_MODEL"

                If String.IsNullOrWhiteSpace(WF_TANKNUMBER.Text) OrElse
                    String.IsNullOrWhiteSpace(WF_MODEL.Text) Then
                    Exit Select
                End If

                '入力値取得
                Dim tankNumber As Long = 0
                Dim modelL As String = ""
                Dim modelR As Long = 0
                Try
                    'JOT車番
                    tankNumber = Long.Parse(WF_TANKNUMBER.Text)
                    '形式
                    Dim re As New Regex("^(?<modelL>\w*?)(?<modelR>\d*?)$")
                    Dim m As Match = re.Match(WF_MODEL.Text)
                    While m.Success
                        modelL = StrConv(m.Groups("modelL").Value, VbStrConv.Wide)
                        modelR = Long.Parse(m.Groups("modelR").Value)
                        m = m.NextMatch()
                    End While
                Catch ex As Exception
                    Exit Select
                End Try

                'JR車番
                If String.IsNullOrWhiteSpace(WF_JRTANKNUMBER.Text) Then
                    'タキ1000: 記号形式-JOT車番
                    'その他: ﾀｷ-XXXXX
                    '※ﾀｷは半角カナ
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_JRTANKNUMBER.Text = String.Format("{0}{1}-{2}", StrConv(modelL, VbStrConv.Narrow), modelR, tankNumber)
                    Else
                        WF_JRTANKNUMBER.Text = String.Format("{0}-{1}", StrConv(modelL, VbStrConv.Narrow), modelR)
                    End If
                End If
                '旧JOT車番
                If String.IsNullOrWhiteSpace(WF_OLDTANKNUMBER.Text) Then
                    '文字列9桁
                    '先頭は車種コード
                    '5：タキ1000
                    '4：その他
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_OLDTANKNUMBER.Text = String.Format("5{0}", tankNumber.ToString().PadLeft(8, "0"c))
                    Else
                        WF_OLDTANKNUMBER.Text = String.Format("4{0}", modelR.ToString().PadLeft(8, "0"c))
                    End If
                End If
                'OT車番
                If String.IsNullOrWhiteSpace(WF_OTTANKNUMBER.Text) Then
                    '文字列6桁
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_OTTANKNUMBER.Text = String.Format("1{0}", tankNumber.ToString().PadLeft(5, "0"c))
                    Else
                        WF_OTTANKNUMBER.Text = modelR.ToString()
                    End If
                End If
                'JXTG千葉車番
                If String.IsNullOrWhiteSpace(WF_JXTGTANKNUMBER2.Text) Then
                    '文字列7桁
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_JXTGTANKNUMBER2.Text = String.Format("1{0}", tankNumber.ToString().PadLeft(6, "0"c))
                    Else
                        WF_JXTGTANKNUMBER2.Text = modelR.ToString().PadLeft(7, "0"c)
                    End If
                End If
                'JXTG根岸車番
                If String.IsNullOrWhiteSpace(WF_JXTGTANKNUMBER4.Text) Then
                    '文字列8桁
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        If tankNumber < 1000 Then
                            WF_JXTGTANKNUMBER4.Text = String.Format("{0}-{1}", modelR.ToString(), tankNumber.ToString().PadLeft(3, "0"c))
                        Else
                            '未定
                        End If
                    Else
                        WF_JXTGTANKNUMBER4.Text = modelR.ToString()
                    End If
                End If
                'コスモ車番
                If String.IsNullOrWhiteSpace(WF_COSMOTANKNUMBER.Text) Then
                    '文字列5桁
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_COSMOTANKNUMBER.Text = tankNumber.ToString()
                    Else
                        If modelR.ToString().Length >= 5 Then
                            WF_COSMOTANKNUMBER.Text = modelR.ToString().Substring(0, 2) & modelR.ToString().Substring(modelR.ToString().Length - 3)
                        End If
                    End If
                End If
                '富士石油車番
                If String.IsNullOrWhiteSpace(WF_FUJITANKNUMBER.Text) Then
                    '文字列5桁
                    '1000形式は6開始
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_FUJITANKNUMBER.Text = String.Format("6{0}", tankNumber.ToString().PadLeft(4, "0"c))
                    Else
                        If modelR.ToString().Length >= 5 Then
                            WF_FUJITANKNUMBER.Text = modelR.ToString().Substring(modelR.ToString().Length - 5)
                        End If
                    End If
                End If
                '出光タグ車番
                If String.IsNullOrWhiteSpace(WF_SHELLTANKNUMBER.Text) Then
                    '文字列6桁
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_SHELLTANKNUMBER.Text = String.Format("1{0}", tankNumber.ToString().PadLeft(5, "0"c))
                    Else
                        WF_SHELLTANKNUMBER.Text = modelR.ToString().PadLeft(6, "0"c)
                    End If
                End If
                '出光SAP車番
                If String.IsNullOrWhiteSpace(WF_SAPSHELLTANKNUMBER.Text) Then
                    '文字列8桁
                    If modelL = "タキ" AndAlso modelR = 1000 Then
                        WF_SAPSHELLTANKNUMBER.Text = String.Format("R{0}", tankNumber.ToString().PadLeft(7, "0"c))
                    Else
                        WF_SAPSHELLTANKNUMBER.Text = String.Format("R{0}", modelR.ToString().PadLeft(7, "0"c))
                    End If
                End If

                If WF_FIELD.ID.Equals("WF_TANKNUMBER") Then
                    WF_TANKNUMBER.Focus()
                Else
                    WF_MODEL.Focus()
                End If

            '荷重単位
            Case "WF_LOADUNIT"
                CODENAME_get("UNIT", WF_LOADUNIT.Text, WF_LOADUNIT_TEXT.Text, WW_RTN_SW)
                WF_LOADUNIT.Focus()
            '容積単位
            Case "WF_VOLUMEUNIT"
                CODENAME_get("UNIT", WF_VOLUMEUNIT.Text, WF_VOLUMEUNIT_TEXT.Text, WW_RTN_SW)
                WF_VOLUMEUNIT.Focus()
            '原籍所有者C
            Case "WF_ORIGINOWNERCODE"
                CODENAME_get("ORIGINOWNERCODE", WF_ORIGINOWNERCODE.Text, WF_ORIGINOWNERCODE_TEXT.Text, WW_RTN_SW)
                WF_ORIGINOWNERCODE.Focus()
            '名義所有者C
            Case "WF_OWNERCODE"
                CODENAME_get("ORIGINOWNERCODE", WF_OWNERCODE.Text, WF_OWNERCODE_TEXT.Text, WW_RTN_SW)
                WF_OWNERCODE.Focus()
            'リース先C
            Case "WF_LEASECODE"
                CODENAME_get("CAMPCODE", WF_LEASECODE.Text, WF_LEASECODE_TEXT.Text, WW_RTN_SW)
                WF_LEASECODE.Focus()
            '請負リース区分C
            Case "WF_LEASECLASS"
                CODENAME_get("LEASECLASS", WF_LEASECLASS.Text, WF_LEASECLASS_TEXT.Text, WW_RTN_SW)
                WF_LEASECLASS.Focus()
            '自動延長
            Case "WF_AUTOEXTENTION"
                CODENAME_get("AUTOEXTENTION", WF_AUTOEXTENTION.Text, WF_AUTOEXTENTION_TEXT.Text, WW_RTN_SW)
                WF_AUTOEXTENTION.Focus()
            '第三者使用者C
            Case "WF_USERCODE"
                CODENAME_get("USERCODE", WF_USERCODE.Text, WF_USERCODE_TEXT.Text, WW_RTN_SW)
                WF_USERCODE.Focus()
            '原常備駅C
            Case "WF_CURRENTSTATIONCODE"
                CODENAME_get("STATIONPATTERN", WF_CURRENTSTATIONCODE.Text, WF_CURRENTSTATIONCODE_TEXT.Text, WW_RTN_SW)
                WF_CURRENTSTATIONCODE.Focus()
            '原専用種別C
            Case "WF_DEDICATETYPECODE"
                CODENAME_get("DEDICATETYPECODE", WF_DEDICATETYPECODE.Text, WF_DEDICATETYPECODE_TEXT.Text, WW_RTN_SW)
                WF_DEDICATETYPECODE.Focus()
            '臨時常備駅C
            Case "WF_EXTRADINARYSTATIONCODE"
                CODENAME_get("STATIONPATTERN", WF_EXTRADINARYSTATIONCODE.Text, WF_EXTRADINARYSTATIONCODE_TEXT.Text, WW_RTN_SW)
                WF_EXTRADINARYSTATIONCODE.Focus()
            '臨時専用種別C
            Case "WF_EXTRADINARYTYPECODE"
                CODENAME_get("EXTRADINARYTYPECODE", WF_EXTRADINARYTYPECODE.Text, WF_EXTRADINARYTYPECODE_TEXT.Text, WW_RTN_SW)
                WF_EXTRADINARYTYPECODE.Focus()
            '油種大分類コード
            Case "WF_BIGOILCODE"
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
                WF_BIGOILCODE.Focus()
            '油種中分類コード
            Case "WF_MIDDLEOILCODE"
                CODENAME_get("MIDDLEOILCODE", WF_MIDDLEOILCODE.Text, WF_MIDDLEOILCODE_TEXT.Text, WW_RTN_SW)
                WF_MIDDLEOILCODE.Focus()
            '運用基地C
            Case "WF_OPERATIONBASECODE"
                CODENAME_get("BASE", WF_OPERATIONBASECODE.Text, WF_OPERATIONBASECODE_TEXT.Text, WW_RTN_SW)
                WF_OPERATIONBASECODE.Focus()
            '運用基地C（サブ）
            Case "WF_SUBOPERATIONBASECODE"
                CODENAME_get("BASE", WF_SUBOPERATIONBASECODE.Text, WF_SUBOPERATIONBASECODE_TEXT.Text, WW_RTN_SW)
                WF_SUBOPERATIONBASECODE.Focus()
            '塗色C
            Case "WF_COLORCODE"
                CODENAME_get("COLORCODE", WF_COLORCODE.Text, WF_COLORCODE_TEXT.Text, WW_RTN_SW)
                WF_COLORCODE.Focus()
            'マークコード
            Case "WF_MARKCODE"
                CODENAME_get("MARKCODE", WF_MARKCODE.Text, WF_MARKCODE_TEXT.Text, WW_RTN_SW)
                WF_MARKCODE.Focus()
            'JXTG千葉タグコード
            Case "WF_JXTGTAGCODE2"
                CODENAME_get("JXTGTAGCODE2", WF_JXTGTAGCODE2.Text, WF_JXTGTAGCODE2_TEXT.Text, WW_RTN_SW)
                WF_JXTGTAGCODE2.Focus()
            '出光昭シタグコード
            Case "WF_IDSSTAGCODE"
                CODENAME_get("IDSSTAGCODE", WF_IDSSTAGCODE.Text, WF_IDSSTAGCODE_TEXT.Text, WW_RTN_SW)
                WF_IDSSTAGCODE.Focus()
            '取得先C
            Case "WF_OBTAINEDCODE"
                CODENAME_get("OBTAINEDCODE", WF_OBTAINEDCODE.Text, WF_OBTAINEDCODE_TEXT.Text, WW_RTN_SW)
                WF_OBTAINEDCODE.Focus()
            'JR車種コード
            Case "WF_JRTANKTYPE"
                CODENAME_get("JRTANKTYPE", WF_JRTANKTYPE.Text, WF_JRTANKTYPE_TEXT.Text, WW_RTN_SW)
                WF_JRTANKTYPE.Focus()
            '長さフラグ
            Case "WF_LENGTHFLG"
                CODENAME_get("LENGTHFLG", WF_LENGTHFLG.Text, WF_LENGTHFLG_TEXT.Text, WW_RTN_SW)
                WF_LENGTHFLG.Focus()
            '利用フラグ
            Case "WF_USEDFLG"
                CODENAME_get("USEDFLG", WF_USEDFLG.Text, WF_USEDFLG_TEXT.Text, WW_RTN_SW)
                WF_USEDFLG.Focus()
            '中間点検場所
            Case "WF_INTERINSPECTSTATION"
                CODENAME_get("STATIONFOCUSON", WF_INTERINSPECTSTATION.Text, WF_INTERINSPECTSTATION_TEXT.Text, WW_RTN_SW)
                WF_INTERINSPECTSTATION.Focus()
            '中間点検場所
            Case "WF_INTERINSPECTORGCODE"
                CODENAME_get("ORG", WF_INTERINSPECTORGCODE.Text, WF_INTERINSPECTORGCODE_TEXT.Text, WW_RTN_SW)
                WF_INTERINSPECTORGCODE.Focus()
            '自主点検場所
            Case "WF_SELFINSPECTSTATION"
                CODENAME_get("STATIONFOCUSON", WF_SELFINSPECTSTATION.Text, WF_SELFINSPECTSTATION_TEXT.Text, WW_RTN_SW)
                WF_SELFINSPECTSTATION.Focus()
            '自主点検場所
            Case "WF_SELFINSPECTORGCODE"
                CODENAME_get("ORG", WF_SELFINSPECTORGCODE.Text, WF_SELFINSPECTORGCODE_TEXT.Text, WW_RTN_SW)
                WF_SELFINSPECTORGCODE.Focus()
            '休車フラグ
            Case "WF_SUSPENDFLG"
                CODENAME_get("SUSPENDFLG", WF_SUSPENDFLG.Text, WF_SUSPENDFLG_TEXT.Text, WW_RTN_SW)
                WF_SUSPENDFLG.Focus()
            '内部塗装
            Case "WF_INTERNALCOATING"
                CODENAME_get("INTERNALCOATING", WF_INTERNALCOATING.Text, WF_INTERNALCOATING_TEXT.Text, WW_RTN_SW)
                WF_INTERNALCOATING.Focus()
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

                Case "WF_DELFLG"    '削除フラグ
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                Case "WF_DELREASONKBN"    '削除理由区分
                    WF_DELREASONKBN.Text = WW_SelectValue
                    WF_DELREASONKBN_TEXT.Text = WW_SelectText
                    WF_DELREASONKBN.Focus()

                Case "WF_LOADUNIT"    '荷重単位
                    WF_LOADUNIT.Text = WW_SelectValue
                    WF_LOADUNIT_TEXT.Text = WW_SelectText
                    WF_LOADUNIT.Focus()

                Case "WF_VOLUMEUNIT"    '容積単位
                    WF_VOLUMEUNIT.Text = WW_SelectValue
                    WF_VOLUMEUNIT_TEXT.Text = WW_SelectText
                    WF_VOLUMEUNIT.Focus()

                Case "WF_ORIGINOWNERCODE"    '原籍所有者C
                    WF_ORIGINOWNERCODE.Text = WW_SelectValue
                    WF_ORIGINOWNERCODE_TEXT.Text = WW_SelectText
                    WF_ORIGINOWNERCODE.Focus()

                Case "WF_OWNERCODE"    '名義所有者C
                    WF_OWNERCODE.Text = WW_SelectValue
                    WF_OWNERCODE_TEXT.Text = WW_SelectText
                    WF_OWNERCODE.Focus()

                Case "WF_LEASECODE"    'リース先C
                    WF_LEASECODE.Text = WW_SelectValue
                    WF_LEASECODE_TEXT.Text = WW_SelectText
                    WF_LEASECODE.Focus()

                Case "WF_LEASECLASS"    '請負リース区分C
                    WF_LEASECLASS.Text = WW_SelectValue
                    WF_LEASECLASS_TEXT.Text = WW_SelectText
                    WF_LEASECLASS.Focus()

                Case "WF_AUTOEXTENTION"    '自動延長
                    WF_AUTOEXTENTION.Text = WW_SelectValue
                    WF_AUTOEXTENTION_TEXT.Text = WW_SelectText
                    WF_AUTOEXTENTION.Focus()

                Case "WF_LEASESTYMD"    'リース開始年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_LEASESTYMD.Text = ""
                        Else
                            WF_LEASESTYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_LEASESTYMD.Focus()

                Case "WF_LEASEENDYMD"    'リース満了年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_LEASEENDYMD.Text = ""
                        Else
                            WF_LEASEENDYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_LEASEENDYMD.Focus()

                Case "WF_USERCODE"    '第三者使用者C
                    WF_USERCODE.Text = WW_SelectValue
                    WF_USERCODE_TEXT.Text = WW_SelectText
                    WF_USERCODE.Focus()

                Case "WF_USERLIMIT"    '第三者使用期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_USERLIMIT.Text = ""
                        Else
                            WF_USERLIMIT.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_USERLIMIT.Focus()

                Case "WF_CURRENTSTATIONCODE"    '原常備駅C
                    WF_CURRENTSTATIONCODE.Text = WW_SelectValue
                    WF_CURRENTSTATIONCODE_TEXT.Text = WW_SelectText
                    WF_CURRENTSTATIONCODE.Focus()

                Case "WF_DEDICATETYPECODE"    '原専用種別C
                    WF_DEDICATETYPECODE.Text = WW_SelectValue
                    WF_DEDICATETYPECODE_TEXT.Text = WW_SelectText
                    WF_DEDICATETYPECODE.Focus()

                Case "WF_EXTRADINARYSTATIONCODE"    '臨時常備駅C
                    WF_EXTRADINARYSTATIONCODE.Text = WW_SelectValue
                    WF_EXTRADINARYSTATIONCODE_TEXT.Text = WW_SelectText
                    WF_EXTRADINARYSTATIONCODE.Focus()

                Case "WF_LIMITTEXTRADIARYSTATION"    '臨時常備駅期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_LIMITTEXTRADIARYSTATION.Text = ""
                        Else
                            WF_LIMITTEXTRADIARYSTATION.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_LIMITTEXTRADIARYSTATION.Focus()

                Case "WF_EXTRADINARYTYPECODE"    '臨時専用種別C
                    WF_EXTRADINARYTYPECODE.Text = WW_SelectValue
                    WF_EXTRADINARYTYPECODE_TEXT.Text = WW_SelectText
                    WF_EXTRADINARYTYPECODE.Focus()

                Case "WF_EXTRADINARYLIMIT"    '臨時専用期限
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_EXTRADINARYLIMIT.Text = ""
                        Else
                            WF_EXTRADINARYLIMIT.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_EXTRADINARYLIMIT.Focus()

                Case "WF_BIGOILCODE"    '油種大分類コード
                    WF_BIGOILCODE.Text = WW_SelectValue
                    WF_BIGOILCODE_TEXT.Text = WW_SelectText
                    WF_BIGOILCODE.Focus()

                Case "WF_MIDDLEOILCODE"    '油種中分類コード
                    WF_MIDDLEOILCODE.Text = WW_SelectValue
                    WF_MIDDLEOILCODE_TEXT.Text = WW_SelectText
                    WF_MIDDLEOILCODE.Focus()

                Case "WF_OPERATIONBASECODE"    '運用基地C
                    WF_OPERATIONBASECODE.Text = WW_SelectValue
                    WF_OPERATIONBASECODE_TEXT.Text = WW_SelectText
                    WF_OPERATIONBASECODE.Focus()

                Case "WF_SUBOPERATIONBASECODE"    '運用基地C（サブ）
                    WF_SUBOPERATIONBASECODE.Text = WW_SelectValue
                    WF_SUBOPERATIONBASECODE_TEXT.Text = WW_SelectText
                    WF_SUBOPERATIONBASECODE.Focus()

                Case "WF_COLORCODE"    '塗色C
                    WF_COLORCODE.Text = WW_SelectValue
                    WF_COLORCODE_TEXT.Text = WW_SelectText
                    WF_COLORCODE.Focus()

                Case "WF_MARKCODE"    'マークコード
                    WF_MARKCODE.Text = WW_SelectValue
                    WF_MARKCODE_TEXT.Text = WW_SelectText
                    WF_MARKCODE.Focus()

                Case "WF_JXTGTAGCODE2"    'JXTG千葉タグコード
                    WF_JXTGTAGCODE2.Text = WW_SelectValue
                    WF_JXTGTAGCODE2_TEXT.Text = WW_SelectText
                    WF_JXTGTAGCODE2.Focus()

                Case "WF_IDSSTAGCODE"    '出光昭シタグコード
                    WF_IDSSTAGCODE.Text = WW_SelectValue
                    WF_IDSSTAGCODE_TEXT.Text = WW_SelectText
                    WF_IDSSTAGCODE.Focus()

                Case "WF_JRINSPECTIONDATE"    '次回交検年月日(JR）
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_JRINSPECTIONDATE.Text = ""
                        Else
                            WF_JRINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_JRINSPECTIONDATE.Focus()

                Case "WF_INSPECTIONDATE"    '次回交検年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_INSPECTIONDATE.Text = ""
                        Else
                            WF_INSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_INSPECTIONDATE.Focus()

                Case "WF_JRSPECIFIEDDATE"    '次回指定年月日(JR)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_JRSPECIFIEDDATE.Text = ""
                        Else
                            WF_JRSPECIFIEDDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_JRSPECIFIEDDATE.Focus()

                Case "WF_SPECIFIEDDATE"    '次回指定年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_SPECIFIEDDATE.Text = ""
                        Else
                            WF_SPECIFIEDDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_SPECIFIEDDATE.Focus()

                Case "WF_JRALLINSPECTIONDATE"    '次回全検年月日(JR) 
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_JRALLINSPECTIONDATE.Text = ""
                        Else
                            WF_JRALLINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_JRALLINSPECTIONDATE.Focus()

                Case "WF_ALLINSPECTIONDATE"    '次回全検年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ALLINSPECTIONDATE.Text = ""
                        Else
                            WF_ALLINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ALLINSPECTIONDATE.Focus()

                Case "WF_PREINSPECTIONDATE"    '前回全検年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_PREINSPECTIONDATE.Text = ""
                        Else
                            WF_PREINSPECTIONDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_PREINSPECTIONDATE.Focus()

                Case "WF_GETDATE"    '取得年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_GETDATE.Text = ""
                        Else
                            WF_GETDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_GETDATE.Focus()

                Case "WF_TRANSFERDATE"    '車籍編入年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_TRANSFERDATE.Text = ""
                        Else
                            WF_TRANSFERDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_TRANSFERDATE.Focus()

                Case "WF_EXCLUDEDATE"    '車籍除外年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_EXCLUDEDATE.Text = ""
                        Else
                            WF_EXCLUDEDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_EXCLUDEDATE.Focus()

                Case "WF_RETIRMENTDATE"    '資産除却年月日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_RETIRMENTDATE.Text = ""
                        Else
                            WF_RETIRMENTDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_RETIRMENTDATE.Focus()

                Case "WF_OBTAINEDCODE"    '取得先C
                    WF_OBTAINEDCODE.Text = WW_SelectValue
                    WF_OBTAINEDCODE_TEXT.Text = WW_SelectText
                    WF_OBTAINEDCODE.Focus()

                Case "WF_JRTANKTYPE"    'JR車種コード
                    WF_JRTANKTYPE.Text = WW_SelectValue
                    WF_JRTANKTYPE_TEXT.Text = WW_SelectText
                    WF_JRTANKTYPE.Focus()

                Case "WF_LENGTHFLG"    '長さフラグ
                    WF_LENGTHFLG.Text = WW_SelectValue
                    WF_LENGTHFLG_TEXT.Text = WW_SelectText
                    WF_LENGTHFLG.Focus()

                Case "WF_USEDFLG"    '利用フラグ
                    WF_USEDFLG.Text = WW_SelectValue
                    WF_USEDFLG_TEXT.Text = WW_SelectText
                    WF_USEDFLG.Focus()

                Case "WF_INTERINSPECTYM"    '中間点検年月
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_INTERINSPECTYM.Text = ""
                        Else
                            WF_INTERINSPECTYM.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_INTERINSPECTYM.Focus()

                Case "WF_INTERINSPECTSTATION"    '中間点検場所
                    WF_INTERINSPECTSTATION.Text = WW_SelectValue
                    WF_INTERINSPECTSTATION_TEXT.Text = WW_SelectText
                    WF_INTERINSPECTSTATION.Focus()

                Case "WF_INTERINSPECTORGCODE"    '中間点検実施者
                    WF_INTERINSPECTORGCODE.Text = WW_SelectValue
                    WF_INTERINSPECTORGCODE_TEXT.Text = WW_SelectText
                    WF_INTERINSPECTORGCODE.Focus()

                Case "WF_SELFINSPECTYM"    '自主点検年月
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_SELFINSPECTYM.Text = ""
                        Else
                            WF_SELFINSPECTYM.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_SELFINSPECTYM.Focus()

                Case "WF_SELFINSPECTSTATION"    '自主点検場所
                    WF_SELFINSPECTSTATION.Text = WW_SelectValue
                    WF_SELFINSPECTSTATION_TEXT.Text = WW_SelectText
                    WF_SELFINSPECTSTATION.Focus()

                Case "WF_SELFINSPECTORGCODE"    '自主点検実施者
                    WF_SELFINSPECTORGCODE.Text = WW_SelectValue
                    WF_SELFINSPECTORGCODE_TEXT.Text = WW_SelectText
                    WF_SELFINSPECTORGCODE.Focus()

                Case "WF_ALLINSPECTPLANYM"    '全検計画年月
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_ALLINSPECTPLANYM.Text = ""
                        Else
                            WF_ALLINSPECTPLANYM.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_ALLINSPECTPLANYM.Focus()

                Case "WF_SUSPENDFLG"    '休車フラグ
                    WF_SUSPENDFLG.Text = WW_SelectValue
                    WF_SUSPENDFLG_TEXT.Text = WW_SelectText
                    WF_SUSPENDFLG.Focus()

                Case "WF_SUSPENDDATE"   '休車日
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            WF_SUSPENDDATE.Text = ""
                        Else
                            WF_SUSPENDDATE.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_SUSPENDDATE.Focus()

                Case "WF_INTERNALCOATING"   '内部塗装
                    WF_INTERNALCOATING.Text = WW_SelectValue
                    WF_INTERNALCOATING_TEXT.Text = WW_SelectText
                    WF_INTERNALCOATING.Focus()

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
                Case "WF_DELFLG"                        '削除フラグ
                    WF_DELFLG.Focus()

                Case "WF_DELREASONKBN"                  '削除理由区分
                    WF_DELREASONKBN.Focus()

                Case "WF_LOADUNIT"                      '荷重単位
                    WF_LOADUNIT.Focus()

                Case "WF_VOLUMEUNIT"                    '容積単位
                    WF_VOLUMEUNIT.Focus()

                Case "WF_ORIGINOWNERCODE"               '原籍所有者C
                    WF_ORIGINOWNERCODE.Focus()

                Case "WF_OWNERCODE"                     '名義所有者C
                    WF_OWNERCODE.Focus()

                Case "WF_LEASECODE"                     'リース先C
                    WF_LEASECODE.Focus()

                Case "WF_LEASECLASS"                    '請負リース区分C
                    WF_LEASECLASS.Focus()

                Case "WF_AUTOEXTENTION"                 '自動延長
                    WF_AUTOEXTENTION.Focus()

                Case "WF_LEASESTYMD"                    'リース開始年月日
                    WF_LEASESTYMD.Focus()

                Case "WF_LEASEENDYMD"                   'リース満了年月日
                    WF_LEASEENDYMD.Focus()

                Case "WF_USERCODE"                      '第三者使用者C
                    WF_USERCODE.Focus()

                Case "WF_USERLIMIT"                     '第三者使用期限
                    WF_USERLIMIT.Focus()

                Case "WF_CURRENTSTATIONCODE"            '原常備駅C
                    WF_CURRENTSTATIONCODE.Focus()

                Case "WF_DEDICATETYPECODE"              '原専用種別C
                    WF_DEDICATETYPECODE.Focus()

                Case "WF_EXTRADINARYSTATIONCODE"        '臨時常備駅C
                    WF_EXTRADINARYSTATIONCODE.Focus()

                Case "WF_LIMITTEXTRADIARYSTATION"       '臨時常備駅期限
                    WF_LIMITTEXTRADIARYSTATION.Focus()

                Case "WF_EXTRADINARYTYPECODE"           '臨時専用種別C
                    WF_EXTRADINARYTYPECODE.Focus()

                Case "WF_EXTRADINARYLIMIT"              '臨時専用期限
                    WF_EXTRADINARYLIMIT.Focus()

                Case "WF_BIGOILCODE"                    '油種大分類コード
                    WF_BIGOILCODE.Focus()

                Case "WF_OPERATIONBASECODE"             '運用基地C
                    WF_OPERATIONBASECODE.Focus()

                Case "WF_SUBOPERATIONBASECODE"          '運用基地C（サブ）
                    WF_SUBOPERATIONBASECODE.Focus()

                Case "WF_COLORCODE"                     '塗色C
                    WF_COLORCODE.Focus()

                Case "WF_MARKCODE"                      'マークコード
                    WF_MARKCODE.Focus()

                Case "WF_JXTGTAGCODE2"                  'JXTG千葉タグコード
                    WF_JXTGTAGCODE2.Focus()

                Case "WF_IDSSTAGCODE"                   '出光昭シタグコード
                    WF_IDSSTAGCODE.Focus()

                Case "WF_JRINSPECTIONDATE"              '次回交検年月日(JR）
                    WF_JRINSPECTIONDATE.Focus()

                Case "WF_INSPECTIONDATE"                '次回交検年月日
                    WF_INSPECTIONDATE.Focus()

                Case "WF_JRSPECIFIEDDATE"               '次回指定年月日(JR)
                    WF_JRSPECIFIEDDATE.Focus()

                Case "WF_SPECIFIEDDATE"                 '次回指定年月日
                    WF_SPECIFIEDDATE.Focus()

                Case "WF_JRALLINSPECTIONDATE"           '次回全検年月日(JR) 
                    WF_JRALLINSPECTIONDATE.Focus()

                Case "WF_ALLINSPECTIONDATE"             '次回全検年月日
                    WF_ALLINSPECTIONDATE.Focus()

                Case "WF_PREINSPECTIONDATE"             '前回全検年月日
                    WF_PREINSPECTIONDATE.Focus()

                Case "WF_GETDATE"                       '取得年月日
                    WF_GETDATE.Focus()

                Case "WF_TRANSFERDATE"                  '車籍編入年月日
                    WF_TRANSFERDATE.Focus()

                Case "WF_EXCLUDEDATE"                   '車籍除外年月日
                    WF_EXCLUDEDATE.Focus()

                Case "WF_RETIRMENTDATE"                 '資産除却年月日
                    WF_RETIRMENTDATE.Focus()

                Case "WF_OBTAINEDCODE"                  '取得先C
                    WF_OBTAINEDCODE.Focus()

                Case "WF_JRTANKTYPE"                    'JR車種コード
                    WF_JRTANKTYPE.Focus()

                Case "WF_LENGTHFLG"                     '長さフラグ
                    WF_LENGTHFLG.Focus()

                Case "WF_USEDFLG"                       '利用フラグ
                    WF_USEDFLG.Focus()

                Case "WF_INTERINSPECTYM"                '中間点検年月
                    WF_INTERINSPECTYM.Focus()

                Case "WF_INTERINSPECTSTATION"           '中間点検場所
                    WF_INTERINSPECTSTATION.Focus()

                Case "WF_INTERINSPECTORGCODE"           '中間点検実施者
                    WF_INTERINSPECTORGCODE.Focus()

                Case "WF_SELFINSPECTYM"                 '自主点検年月
                    WF_SELFINSPECTYM.Focus()

                Case "WF_SELFINSPECTSTATION"            '自主点検場所
                    WF_SELFINSPECTSTATION.Focus()

                Case "WF_SELFINSPECTORGCODE"            '自主点検実施者
                    WF_SELFINSPECTORGCODE.Focus()

                Case "WF_ALLINSPECTPLANYM"              '全検計画年月
                    WF_ALLINSPECTPLANYM.Focus()

                Case "WF_SUSPENDFLG"                    '休車フラグ
                    WF_SUSPENDFLG.Focus()

                Case "WF_SUSPENDDATE"                   '休車日
                    WF_SUSPENDDATE.Focus()

                Case "WF_INTERNALCOATING"               '内部塗装
                    WF_INTERNALCOATING.Focus()

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
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            WW_TEXT = OIM0005INProw("DELFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0005INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '-----------------------------------------------------
            ' 削除フラグ＝削除の場合、削除理由区分の入力チェック
            '-----------------------------------------------------
            If OIM0005INProw("DELFLG").ToString.Equals(C_DELETE_FLG.DELETE) Then
                ' 削除理由区分（バリデーションチェック）
                WW_TEXT = OIM0005INProw("DELREASONKBN")
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELREASONKBN", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If isNormal(WW_CS0024FCHECKERR) Then
                    If WW_TEXT <> "" Then
                        '値存在チェック
                        CODENAME_get("DELREASONKBN", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                        If Not isNormal(WW_RTN_SW) Then
                            WW_CheckMES1 = "・更新できないレコード(削除理由区分入力エラー)です。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                            WW_LINE_ERR = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・更新できないレコード(削除理由区分入力エラー)です。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'JOT車番(バリデーションチェック)
            WW_TEXT = OIM0005INProw("TANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JOT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原籍所有者C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("ORIGINOWNERCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORIGINOWNERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(原籍所有者C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原籍所有者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '名義所有者C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("OWNERCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORIGINOWNERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(名義所有者C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(名義所有者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース先C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("CAMPCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(リース先C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(リース先C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '請負リース区分C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASECLASS")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASS", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("LEASECLASS", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(請負リース区分C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(請負リース区分C入力エラー)です。"
                WW_CheckMES1 = "請負リース区分C入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '自動延長(バリデーションチェック)
            WW_TEXT = OIM0005INProw("AUTOEXTENTION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("AUTOEXTENTION", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(自動延長入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自動延長入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース開始年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASESTYMD")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASESTYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "リース開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(リース開始年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("LEASESTYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(リース開始年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'リース満了年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LEASEENDYMD")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASEENDYMD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "リース満了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(リース満了年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("LEASEENDYMD") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(リース満了年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用者C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("USERCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("USERCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(第三者使用者C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(第三者使用者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原常備駅C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("CURRENTSTATIONCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONPATTERN", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(原常備駅C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原常備駅C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("EXTRADINARYSTATIONCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONPATTERN", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(臨時常備駅C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時常備駅C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '第三者使用期限(バリデーションチェック)
            WW_TEXT = OIM0005INProw("USERLIMIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERLIMIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "第三者使用期限", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(第三者使用期限入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("USERLIMIT") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(第三者使用期限入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時常備駅期限(バリデーションチェック)
            WW_TEXT = OIM0005INProw("LIMITTEXTRADIARYSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LIMITTEXTRADIARYSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "臨時常備駅期限", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(臨時常備駅期限入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("LIMITTEXTRADIARYSTATION") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時常備駅期限入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原専用種別C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("DEDICATETYPECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("DEDICATETYPECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(原専用種別C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原専用種別C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用種別C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("EXTRADINARYTYPECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("EXTRADINARYTYPECODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(臨時専用種別C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時専用種別C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '臨時専用期限(バリデーションチェック)
            WW_TEXT = OIM0005INProw("EXTRADINARYLIMIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYLIMIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "臨時専用期限", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(臨時専用期限入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("EXTRADINARYLIMIT") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(臨時専用期限入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("OPERATIONBASECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("BASE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(運用基地C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用基地C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '運用基地C（サブ）(バリデーションチェック)
            WW_TEXT = OIM0005INProw("SUBOPERATIONBASECODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUBOPERATIONBASECODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("BASE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(運用基地C（サブ）入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用基地C（サブ）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '塗色C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("COLORCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("COLORCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(塗色C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(塗色C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'マークコード(バリデーションチェック)
            WW_TEXT = OIM0005INProw("MARKCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MARKCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("MARKCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(マークコード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(マークコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            'マーク名(バリデーションチェック)
            WW_TEXT = OIM0005INProw("MARKNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MARKNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(マーク名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("GETDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "GETDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "取得年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(取得年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("GETDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取得年月日入力エラー入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車籍編入年月日(バリデーションチェック)
            WW_TEXT = OIM0005INProw("TRANSFERDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRANSFERDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "車籍編入年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(車籍編入年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("TRANSFERDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車籍編入年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取得先C(バリデーションチェック)
            WW_TEXT = OIM0005INProw("OBTAINEDCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("OBTAINEDCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(取得先C入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(取得先C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 形式（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MODEL")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODEL", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(形式入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 形式カナ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MODELKANA")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MODELKANA", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(形式カナ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 荷重（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LOAD")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LOAD", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(荷重入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 荷重単位（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LOADUNIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LOADUNIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("UNIT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(荷重単位入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(荷重単位入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 容積（バリデーションチェック）
            WW_TEXT = OIM0005INProw("VOLUME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "VOLUME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(容積入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 容積単位（バリデーションチェック）
            WW_TEXT = OIM0005INProw("VOLUMEUNIT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "VOLUMEUNIT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("UNIT", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(容積単位入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(容積単位入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自重（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MYWEIGHT")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MYWEIGHT", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(自重入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' タンク車長（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LENGTH")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LENGTH", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(タンク車長入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' タンク車体長（バリデーションチェック）
            WW_TEXT = OIM0005INProw("TANKLENGTH")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKLENGTH", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(タンク車体長入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 最大口径（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MAXCALIBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MAXCALIBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(最大口径入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 最小口径（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MINCALIBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MINCALIBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(最小口径入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 長さフラグ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LENGTHFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LENGTHFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("LENGTHFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(長さフラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(長さフラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 原籍所有者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("ORIGINOWNERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(原籍所有者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 名義所有者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OWNERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(名義所有者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' リース先（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LEASENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(リース先入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 請負リース区分（バリデーションチェック）
            WW_TEXT = OIM0005INProw("LEASECLASSNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "LEASECLASSNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(請負リース区分入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自動延長名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("AUTOEXTENTIONNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTIONNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(自動延長名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 第三者使用者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("USERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(第三者使用者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 原常備駅（バリデーションチェック）
            WW_TEXT = OIM0005INProw("CURRENTSTATIONNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(原常備駅入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 臨時常備駅（バリデーションチェック）
            WW_TEXT = OIM0005INProw("EXTRADINARYSTATIONNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYSTATIONNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(臨時常備駅入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 原専用種別（バリデーションチェック）
            WW_TEXT = OIM0005INProw("DEDICATETYPENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DEDICATETYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(原専用種別入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 臨時専用種別（バリデーションチェック）
            WW_TEXT = OIM0005INProw("EXTRADINARYTYPENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXTRADINARYTYPENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(臨時専用種別入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種大分類コード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("BIGOILCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIGOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("BIGOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種大分類名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("BIGOILNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIGOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種大分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種中分類コード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MIDDLEOILCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("MIDDLEOILCODE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種中分類コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 油種中分類名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("MIDDLEOILNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "MIDDLEOILNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(油種中分類名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 運用場所（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OPERATIONBASENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(運用場所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 運用場所（サブ）（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SUBOPERATIONBASENAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUBOPERATIONBASENAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(運用場所（サブ）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 塗色（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COLORNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COLORNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(塗色入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG仙台タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG仙台タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG仙台タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG仙台タグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG千葉タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG千葉タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG千葉タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG千葉タグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG川崎タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG川崎タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG川崎タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG川崎タグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG根岸タグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGCODE4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGCODE4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG根岸タグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG根岸タグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTAGNAME4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTAGNAME4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG根岸タグ名入力入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シタグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("IDSSTAGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "IDSSTAGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シタグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シタグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("IDSSTAGNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "IDSSTAGNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シタグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コスモタグコード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COSMOTAGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COSMOTAGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(コスモタグコード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コスモタグ名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COSMOTAGNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COSMOTAGNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(コスモタグ名入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 予備1（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RESERVE1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RESERVE1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予備1入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 予備2（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RESERVE2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RESERVE2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予備2入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回交検年月日(JR）（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回交検年月日(JR）", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回交検年月日(JR）入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("JRINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回交検年月日(JR）入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回交検年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回交検年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回交検年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("INSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回交検年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回指定年月日(JR)（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRSPECIFIEDDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRSPECIFIEDDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回指定年月日(JR)", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回指定年月日(JR)入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("JRSPECIFIEDDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回指定年月日(JR)入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回指定年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SPECIFIEDDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SPECIFIEDDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回指定年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回指定年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("SPECIFIEDDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回指定年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回全検年月日(JR) （バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRALLINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRALLINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回全検年月日(JR)", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回全検年月日(JR)入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("JRALLINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回全検年月日(JR)入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回全検年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("ALLINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "次回全検年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(次回全検年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("ALLINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(次回全検年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 前回全検年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("PREINSPECTIONDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PREINSPECTIONDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "前回全検年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(前回全検年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("PREINSPECTIONDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(前回全検年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取得先名（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OBTAINEDNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OBTAINEDNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取得先名エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 現在経年（バリデーションチェック）
            WW_TEXT = OIM0005INProw("PROGRESSYEAR")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PROGRESSYEAR", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(現在経年入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 次回全検時経年（バリデーションチェック）
            WW_TEXT = OIM0005INProw("NEXTPROGRESSYEAR")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "NEXTPROGRESSYEAR", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(次回全検時経年入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 車籍除外年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("EXCLUDEDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "EXCLUDEDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "車籍除外年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(車籍除外年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("EXCLUDEDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車籍除外年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 資産除却年月日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RETIRMENTDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RETIRMENTDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "資産除却年月日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(資産除却年月日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("RETIRMENTDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(資産除却年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JR車番エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JR車種コード（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JRTANKTYPE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JRTANKTYPE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("JRTANKTYPE", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(JR車種コード入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(JR車種コード入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 旧JOT車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OLDTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OLDTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(旧JOT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' OT車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("OTTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OTTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(OT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG仙台車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER1")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER1", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG仙台車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG千葉車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER2")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER2", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG千葉車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG川崎車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG川崎車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' JXTG根岸車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("JXTGTANKNUMBER4")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "JXTGTANKNUMBER4", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(JXTG根岸車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コスモ車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("COSMOTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "COSMOTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(コスモ車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 富士石油車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("FUJITANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "FUJITANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(富士石油車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シ車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SHELLTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SHELLTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シ車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 出光昭シSAP車番（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SAPSHELLTANKNUMBER")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SAPSHELLTANKNUMBER", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(出光昭シSAP車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 予備（バリデーションチェック）
            WW_TEXT = OIM0005INProw("RESERVE3")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "RESERVE3", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(予備入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 利用フラグ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("USEDFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USEDFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("USEDFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(利用フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(利用フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中間点検年月（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERINSPECTYM")
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                WW_TEXT = WW_TEXT + "/01"
            End If
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERINSPECTYM", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "中間点検年月", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(中間点検年月入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("INTERINSPECTYM") = CDate(WW_TEXT).ToString("yyyy/MM")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(中間点検年月入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中間点検場所（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERINSPECTSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERINSPECTSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONFOCUSON", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(中間点検場所入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(中間点検場所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中間点検実施者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERINSPECTORGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERINSPECTORGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(中間点検実施者入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(中間点検実施者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検年月（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SELFINSPECTYM")
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                WW_TEXT = WW_TEXT + "/01"
            End If
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SELFINSPECTYM", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '年月日チェック
                    WW_CheckDate(WW_TEXT, "自主点検年月", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(自主点検年月入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("SELFINSPECTYM") = CDate(WW_TEXT).ToString("yyyy/MM")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自主点検年月入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検場所（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SELFINSPECTSTATION")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SELFINSPECTSTATION", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("STATIONFOCUSON", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(自主点検場所入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自主点検場所入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検実施者（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SELFINSPECTORGCODE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SELFINSPECTORGCODE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("ORG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(自主点検実施者入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(自主点検実施者入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 自主点検実施者(社員名)（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INSPECTMEMBERNAME")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INSPECTMEMBERNAME", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(自主点検実施者(社員名))です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 全検計画年月（バリデーションチェック）
            WW_TEXT = OIM0005INProw("ALLINSPECTPLANYM")
            If Not String.IsNullOrEmpty(WW_TEXT) Then
                WW_TEXT = WW_TEXT + "/01"
            End If
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ALLINSPECTPLANYM", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '全検計画年月
                    WW_CheckDate(WW_TEXT, "全検計画年月", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(全検計画年月入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("ALLINSPECTPLANYM") = CDate(WW_TEXT).ToString("yyyy/MM")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(全検計画年月入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 休車フラグ（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SUSPENDFLG")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUSPENDFLG", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("SUSPENDFLG", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(休車フラグ入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(休車フラグ入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 休車日（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SUSPENDDATE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SUSPENDDATE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '休車日
                    WW_CheckDate(WW_TEXT, "休車日", WW_CS0024FCHECKERR, dateErrFlag)
                    If dateErrFlag = "1" Then
                        WW_CheckMES1 = "・更新できないレコード(休車日入力エラー)です。"
                        WW_CheckMES2 = C_MESSAGE_NO.DATE_FORMAT_ERROR
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        OIM0005INProw("SUSPENDDATE") = CDate(WW_TEXT).ToString("yyyy/MM/dd")
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(休車日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取得価格（バリデーションチェック）
            WW_TEXT = OIM0005INProw("PURCHASEPRICE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "PURCHASEPRICE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(取得価格入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 内部塗装（バリデーションチェック）
            WW_TEXT = OIM0005INProw("INTERNALCOATING")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "INTERNALCOATING", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                If WW_TEXT <> "" Then
                    '値存在チェック
                    CODENAME_get("INTERNALCOATING", WW_TEXT, WW_DUMMY, WW_RTN_SW)
                    If Not isNormal(WW_RTN_SW) Then
                        WW_CheckMES1 = "・更新できないレコード(内部塗装入力エラー)です。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                        WW_LINE_ERR = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(内部塗装入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 安全弁（バリデーションチェック）
            WW_TEXT = OIM0005INProw("SAFETYVALVE")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "SAFETYVALVE", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(安全弁入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' センターバルブ情報（バリデーションチェック）
            WW_TEXT = OIM0005INProw("CENTERVALVEINFO")
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CENTERVALVEINFO", WW_TEXT, WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "・更新できないレコード(センターバルブ情報入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0005INProw("TANKNUMBER") = work.WF_SEL_TANKNUMBER2.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反（JOT車番）。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & OIM0005INProw("TANKNUMBER") & "]"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If


            If WW_LINE_ERR = "" Then
                If OIM0005INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0005INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0005INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' <param name="OIM0005row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0005row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0005row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> JOT車番 =" & OIM0005row("TANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式 =" & OIM0005row("MODEL") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 形式カナ =" & OIM0005row("MODELKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重 =" & OIM0005row("LOAD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 荷重単位 =" & OIM0005row("LOADUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積 =" & OIM0005row("VOLUME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 容積単位 =" & OIM0005row("VOLUMEUNIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自重 =" & OIM0005row("MYWEIGHT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車長 =" & OIM0005row("LENGTH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> タンク車体長 =" & OIM0005row("TANKLENGTH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 最大口径 =" & OIM0005row("MAXCALIBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 最小口径 =" & OIM0005row("MINCALIBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 長さフラグ =" & OIM0005row("LENGTHFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者C =" & OIM0005row("ORIGINOWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者 =" & OIM0005row("ORIGINOWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者C =" & OIM0005row("OWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者 =" & OIM0005row("OWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先C =" & OIM0005row("LEASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先 =" & OIM0005row("LEASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 請負リース区分C =" & OIM0005row("LEASECLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分 =" & OIM0005row("LEASECLASSNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長 =" & OIM0005row("AUTOEXTENTION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自動延長名 =" & OIM0005row("AUTOEXTENTIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース開始年月日 =" & OIM0005row("LEASESTYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース満了年月日 =" & OIM0005row("LEASEENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者C =" & OIM0005row("USERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用者 =" & OIM0005row("USERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅C =" & OIM0005row("CURRENTSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原常備駅 =" & OIM0005row("CURRENTSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅C =" & OIM0005row("EXTRADINARYSTATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅 =" & OIM0005row("EXTRADINARYSTATIONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 第三者使用期限 =" & OIM0005row("USERLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時常備駅期限 =" & OIM0005row("LIMITTEXTRADIARYSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別C =" & OIM0005row("DEDICATETYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原専用種別 =" & OIM0005row("DEDICATETYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別C =" & OIM0005row("EXTRADINARYTYPECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用種別 =" & OIM0005row("EXTRADINARYTYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 臨時専用期限 =" & OIM0005row("EXTRADINARYLIMIT") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類コード =" & OIM0005row("BIGOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種大分類名 =" & OIM0005row("BIGOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類コード =" & OIM0005row("MIDDLEOILCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 油種中分類名 =" & OIM0005row("MIDDLEOILNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C =" & OIM0005row("OPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所 =" & OIM0005row("OPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C（サブ） =" & OIM0005row("SUBOPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所（サブ） =" & OIM0005row("SUBOPERATIONBASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色C =" & OIM0005row("COLORCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 塗色 =" & OIM0005row("COLORNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> マークコード =" & OIM0005row("MARKCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> マーク名 =" & OIM0005row("MARKNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG仙台タグコード =" & OIM0005row("JXTGTAGCODE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG仙台タグ名 =" & OIM0005row("JXTGTAGNAME1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG千葉タグコード =" & OIM0005row("JXTGTAGCODE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG千葉タグ名 =" & OIM0005row("JXTGTAGNAME2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG川崎タグコード =" & OIM0005row("JXTGTAGCODE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG川崎タグ名 =" & OIM0005row("JXTGTAGNAME3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG根岸タグコード =" & OIM0005row("JXTGTAGCODE4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG根岸タグ名 =" & OIM0005row("JXTGTAGNAME4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シタグコード =" & OIM0005row("IDSSTAGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シタグ名 =" & OIM0005row("IDSSTAGNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモタグコード =" & OIM0005row("COSMOTAGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモタグ名 =" & OIM0005row("COSMOTAGNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備1 =" & OIM0005row("RESERVE1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備2 =" & OIM0005row("RESERVE2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日(JR） =" & OIM0005row("JRINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回交検年月日 =" & OIM0005row("INSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日(JR) =" & OIM0005row("JRSPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回指定年月日 =" & OIM0005row("SPECIFIEDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日(JR)  =" & OIM0005row("JRALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検年月日 =" & OIM0005row("ALLINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 前回全検年月日 =" & OIM0005row("PREINSPECTIONDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得年月日 =" & OIM0005row("GETDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍編入年月日 =" & OIM0005row("TRANSFERDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先C =" & OIM0005row("OBTAINEDCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得先名 =" & OIM0005row("OBTAINEDNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 現在経年 =" & OIM0005row("PROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 次回全検時経年 =" & OIM0005row("NEXTPROGRESSYEAR") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 車籍除外年月日 =" & OIM0005row("EXCLUDEDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 資産除却年月日 =" & OIM0005row("RETIRMENTDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車番 =" & OIM0005row("JRTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JR車種コード =" & OIM0005row("JRTANKTYPE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 旧JOT車番 =" & OIM0005row("OLDTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> OT車番 =" & OIM0005row("OTTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG仙台車番 =" & OIM0005row("JXTGTANKNUMBER1") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG千葉車番 =" & OIM0005row("JXTGTANKNUMBER2") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG川崎車番 =" & OIM0005row("JXTGTANKNUMBER3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> JXTG根岸車番 =" & OIM0005row("JXTGTANKNUMBER4") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> コスモ車番 =" & OIM0005row("COSMOTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 富士石油車番 =" & OIM0005row("FUJITANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シ車番 =" & OIM0005row("SHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 出光昭シSAP車番 =" & OIM0005row("SAPSHELLTANKNUMBER") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 予備 =" & OIM0005row("RESERVE3") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 利用フラグ =" & OIM0005row("USEDFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 中間点検年月 =" & OIM0005row("INTERINSPECTYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 中間点検場所 =" & OIM0005row("INTERINSPECTSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 中間点検実施者 =" & OIM0005row("INTERINSPECTORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自主点検年月 =" & OIM0005row("SELFINSPECTYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自主点検場所 =" & OIM0005row("SELFINSPECTSTATION") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 自主点検実施者 =" & OIM0005row("SELFINSPECTORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 点検実施者(社員名) =" & OIM0005row("INSPECTMEMBERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 全検計画年月 =" & OIM0005row("ALLINSPECTPLANYM") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 休車フラグ =" & OIM0005row("SUSPENDFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 休車日 =" & OIM0005row("SUSPENDDATE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 取得価格 =" & OIM0005row("PURCHASEPRICE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 内部塗装 =" & OIM0005row("INTERNALCOATING") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 安全弁 =" & OIM0005row("SAFETYVALVE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> センターバルブ情報 =" & OIM0005row("CENTERVALVEINFO") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0005row("DELFLG") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除理由区分 =" & OIM0005row("DELREASONKBN")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub

    ''' <summary>
    ''' OIM0005tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0005tbl_UPD()

        '○ 画面状態設定
        For Each OIM0005row As DataRow In OIM0005tbl.Rows
            Select Case OIM0005row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0005INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0005INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each OIM0005row As DataRow In OIM0005tbl.Rows
                ' KEY項目が等しい時
                If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") Then
                    ' KEY項目以外の項目の差異をチェック
                    If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") AndAlso
                        OIM0005row("MODEL") = OIM0005INProw("MODEL") AndAlso
                        OIM0005row("MODELKANA") = OIM0005INProw("MODELKANA") AndAlso
                        OIM0005row("LOAD") = OIM0005INProw("LOAD") AndAlso
                        OIM0005row("LOADUNIT") = OIM0005INProw("LOADUNIT") AndAlso
                        OIM0005row("VOLUME") = OIM0005INProw("VOLUME") AndAlso
                        OIM0005row("VOLUMEUNIT") = OIM0005INProw("VOLUMEUNIT") AndAlso
                        OIM0005row("MYWEIGHT") = OIM0005INProw("MYWEIGHT") AndAlso
                        OIM0005row("LENGTH") = OIM0005INProw("LENGTH") AndAlso
                        OIM0005row("TANKLENGTH") = OIM0005INProw("TANKLENGTH") AndAlso
                        OIM0005row("MAXCALIBER") = OIM0005INProw("MAXCALIBER") AndAlso
                        OIM0005row("MINCALIBER") = OIM0005INProw("MINCALIBER") AndAlso
                        OIM0005row("LENGTHFLG") = OIM0005INProw("LENGTHFLG") AndAlso
                        OIM0005row("ORIGINOWNERCODE") = OIM0005INProw("ORIGINOWNERCODE") AndAlso
                        OIM0005row("ORIGINOWNERNAME") = OIM0005INProw("ORIGINOWNERNAME") AndAlso
                        OIM0005row("OWNERCODE") = OIM0005INProw("OWNERCODE") AndAlso
                        OIM0005row("OWNERNAME") = OIM0005INProw("OWNERNAME") AndAlso
                        OIM0005row("LEASECODE") = OIM0005INProw("LEASECODE") AndAlso
                        OIM0005row("LEASENAME") = OIM0005INProw("LEASENAME") AndAlso
                        OIM0005row("LEASECLASS") = OIM0005INProw("LEASECLASS") AndAlso
                        OIM0005row("LEASECLASSNAME") = OIM0005INProw("LEASECLASSNAME") AndAlso
                        OIM0005row("AUTOEXTENTION") = OIM0005INProw("AUTOEXTENTION") AndAlso
                        OIM0005row("AUTOEXTENTIONNAME") = OIM0005INProw("AUTOEXTENTIONNAME") AndAlso
                        OIM0005row("LEASESTYMD") = OIM0005INProw("LEASESTYMD") AndAlso
                        OIM0005row("LEASEENDYMD") = OIM0005INProw("LEASEENDYMD") AndAlso
                        OIM0005row("USERCODE") = OIM0005INProw("USERCODE") AndAlso
                        OIM0005row("USERNAME") = OIM0005INProw("USERNAME") AndAlso
                        OIM0005row("CURRENTSTATIONCODE") = OIM0005INProw("CURRENTSTATIONCODE") AndAlso
                        OIM0005row("CURRENTSTATIONNAME") = OIM0005INProw("CURRENTSTATIONNAME") AndAlso
                        OIM0005row("EXTRADINARYSTATIONCODE") = OIM0005INProw("EXTRADINARYSTATIONCODE") AndAlso
                        OIM0005row("EXTRADINARYSTATIONNAME") = OIM0005INProw("EXTRADINARYSTATIONNAME") AndAlso
                        OIM0005row("USERLIMIT") = OIM0005INProw("USERLIMIT") AndAlso
                        OIM0005row("LIMITTEXTRADIARYSTATION") = OIM0005INProw("LIMITTEXTRADIARYSTATION") AndAlso
                        OIM0005row("DEDICATETYPECODE") = OIM0005INProw("DEDICATETYPECODE") AndAlso
                        OIM0005row("DEDICATETYPENAME") = OIM0005INProw("DEDICATETYPENAME") AndAlso
                        OIM0005row("EXTRADINARYTYPECODE") = OIM0005INProw("EXTRADINARYTYPECODE") AndAlso
                        OIM0005row("EXTRADINARYTYPENAME") = OIM0005INProw("EXTRADINARYTYPENAME") AndAlso
                        OIM0005row("EXTRADINARYLIMIT") = OIM0005INProw("EXTRADINARYLIMIT") AndAlso
                        OIM0005row("BIGOILCODE") = OIM0005INProw("BIGOILCODE") AndAlso
                        OIM0005row("BIGOILNAME") = OIM0005INProw("BIGOILNAME") AndAlso
                        OIM0005row("MIDDLEOILCODE") = OIM0005INProw("MIDDLEOILCODE") AndAlso
                        OIM0005row("MIDDLEOILNAME") = OIM0005INProw("MIDDLEOILNAME") AndAlso
                        OIM0005row("OPERATIONBASECODE") = OIM0005INProw("OPERATIONBASECODE") AndAlso
                        OIM0005row("OPERATIONBASENAME") = OIM0005INProw("OPERATIONBASENAME") AndAlso
                        OIM0005row("SUBOPERATIONBASECODE") = OIM0005INProw("SUBOPERATIONBASECODE") AndAlso
                        OIM0005row("SUBOPERATIONBASENAME") = OIM0005INProw("SUBOPERATIONBASENAME") AndAlso
                        OIM0005row("COLORCODE") = OIM0005INProw("COLORCODE") AndAlso
                        OIM0005row("COLORNAME") = OIM0005INProw("COLORNAME") AndAlso
                        OIM0005row("MARKCODE") = OIM0005INProw("MARKCODE") AndAlso
                        OIM0005row("MARKNAME") = OIM0005INProw("MARKNAME") AndAlso
                        OIM0005row("JXTGTAGCODE1") = OIM0005INProw("JXTGTAGCODE1") AndAlso
                        OIM0005row("JXTGTAGNAME1") = OIM0005INProw("JXTGTAGNAME1") AndAlso
                        OIM0005row("JXTGTAGCODE2") = OIM0005INProw("JXTGTAGCODE2") AndAlso
                        OIM0005row("JXTGTAGNAME2") = OIM0005INProw("JXTGTAGNAME2") AndAlso
                        OIM0005row("JXTGTAGCODE3") = OIM0005INProw("JXTGTAGCODE3") AndAlso
                        OIM0005row("JXTGTAGNAME3") = OIM0005INProw("JXTGTAGNAME3") AndAlso
                        OIM0005row("JXTGTAGCODE4") = OIM0005INProw("JXTGTAGCODE4") AndAlso
                        OIM0005row("JXTGTAGNAME4") = OIM0005INProw("JXTGTAGNAME4") AndAlso
                        OIM0005row("IDSSTAGCODE") = OIM0005INProw("IDSSTAGCODE") AndAlso
                        OIM0005row("IDSSTAGNAME") = OIM0005INProw("IDSSTAGNAME") AndAlso
                        OIM0005row("COSMOTAGCODE") = OIM0005INProw("COSMOTAGCODE") AndAlso
                        OIM0005row("COSMOTAGNAME") = OIM0005INProw("COSMOTAGNAME") AndAlso
                        OIM0005row("RESERVE1") = OIM0005INProw("RESERVE1") AndAlso
                        OIM0005row("RESERVE2") = OIM0005INProw("RESERVE2") AndAlso
                        OIM0005row("JRINSPECTIONDATE") = OIM0005INProw("JRINSPECTIONDATE") AndAlso
                        OIM0005row("INSPECTIONDATE") = OIM0005INProw("INSPECTIONDATE") AndAlso
                        OIM0005row("JRSPECIFIEDDATE") = OIM0005INProw("JRSPECIFIEDDATE") AndAlso
                        OIM0005row("SPECIFIEDDATE") = OIM0005INProw("SPECIFIEDDATE") AndAlso
                        OIM0005row("JRALLINSPECTIONDATE") = OIM0005INProw("JRALLINSPECTIONDATE") AndAlso
                        OIM0005row("ALLINSPECTIONDATE") = OIM0005INProw("ALLINSPECTIONDATE") AndAlso
                        OIM0005row("PREINSPECTIONDATE") = OIM0005INProw("PREINSPECTIONDATE") AndAlso
                        OIM0005row("GETDATE") = OIM0005INProw("GETDATE") AndAlso
                        OIM0005row("TRANSFERDATE") = OIM0005INProw("TRANSFERDATE") AndAlso
                        OIM0005row("OBTAINEDCODE") = OIM0005INProw("OBTAINEDCODE") AndAlso
                        OIM0005row("OBTAINEDNAME") = OIM0005INProw("OBTAINEDNAME") AndAlso
                        OIM0005row("PROGRESSYEAR") = OIM0005INProw("PROGRESSYEAR") AndAlso
                        OIM0005row("NEXTPROGRESSYEAR") = OIM0005INProw("NEXTPROGRESSYEAR") AndAlso
                        OIM0005row("EXCLUDEDATE") = OIM0005INProw("EXCLUDEDATE") AndAlso
                        OIM0005row("RETIRMENTDATE") = OIM0005INProw("RETIRMENTDATE") AndAlso
                        OIM0005row("JRTANKNUMBER") = OIM0005INProw("JRTANKNUMBER") AndAlso
                        OIM0005row("JRTANKTYPE") = OIM0005INProw("JRTANKTYPE") AndAlso
                        OIM0005row("OLDTANKNUMBER") = OIM0005INProw("OLDTANKNUMBER") AndAlso
                        OIM0005row("OTTANKNUMBER") = OIM0005INProw("OTTANKNUMBER") AndAlso
                        OIM0005row("JXTGTANKNUMBER1") = OIM0005INProw("JXTGTANKNUMBER1") AndAlso
                        OIM0005row("JXTGTANKNUMBER2") = OIM0005INProw("JXTGTANKNUMBER2") AndAlso
                        OIM0005row("JXTGTANKNUMBER3") = OIM0005INProw("JXTGTANKNUMBER3") AndAlso
                        OIM0005row("JXTGTANKNUMBER4") = OIM0005INProw("JXTGTANKNUMBER4") AndAlso
                        OIM0005row("COSMOTANKNUMBER") = OIM0005INProw("COSMOTANKNUMBER") AndAlso
                        OIM0005row("FUJITANKNUMBER") = OIM0005INProw("FUJITANKNUMBER") AndAlso
                        OIM0005row("SHELLTANKNUMBER") = OIM0005INProw("SHELLTANKNUMBER") AndAlso
                        OIM0005row("SAPSHELLTANKNUMBER") = OIM0005INProw("SAPSHELLTANKNUMBER") AndAlso
                        OIM0005row("RESERVE3") = OIM0005INProw("RESERVE3") AndAlso
                        OIM0005row("USEDFLG") = OIM0005INProw("USEDFLG") AndAlso
                        OIM0005row("INTERINSPECTYM") = OIM0005INProw("INTERINSPECTYM") AndAlso
                        OIM0005row("INTERINSPECTSTATION") = OIM0005INProw("INTERINSPECTSTATION") AndAlso
                        OIM0005row("INTERINSPECTORGCODE") = OIM0005INProw("INTERINSPECTORGCODE") AndAlso
                        OIM0005row("SELFINSPECTYM") = OIM0005INProw("SELFINSPECTYM") AndAlso
                        OIM0005row("SELFINSPECTSTATION") = OIM0005INProw("SELFINSPECTSTATION") AndAlso
                        OIM0005row("SELFINSPECTORGCODE") = OIM0005INProw("SELFINSPECTORGCODE") AndAlso
                        OIM0005row("INSPECTMEMBERNAME") = OIM0005INProw("INSPECTMEMBERNAME") AndAlso
                        OIM0005row("ALLINSPECTPLANYM") = OIM0005INProw("ALLINSPECTPLANYM") AndAlso
                        OIM0005row("SUSPENDFLG") = OIM0005INProw("SUSPENDFLG") AndAlso
                        OIM0005row("SUSPENDDATE") = OIM0005INProw("SUSPENDDATE") AndAlso
                        OIM0005row("PURCHASEPRICE") = OIM0005INProw("PURCHASEPRICE") AndAlso
                        OIM0005row("INTERNALCOATING") = OIM0005INProw("INTERNALCOATING") AndAlso
                        OIM0005row("SAFETYVALVE") = OIM0005INProw("SAFETYVALVE") AndAlso
                        OIM0005row("CENTERVALVEINFO") = OIM0005INProw("CENTERVALVEINFO") AndAlso
                        OIM0005row("DELFLG") = OIM0005INProw("DELFLG") AndAlso
                        OIM0005row("DELREASONKBN") = OIM0005INProw("DELREASONKBN") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(OIM0005row("OPERATION")) Then
                        ' 変更がないときは「操作」の項目は空白にする
                        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        OIM0005INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(OIM0005INPtbl.Rows(0)("OPERATION")) Then
            '更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ERRCODE = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(OIM0005INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(OIM0005INPtbl.Rows(0)("OPERATION")) Then
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
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows
            '発見フラグ
            Dim isFound As Boolean = False

            For Each OIM0005row As DataRow In OIM0005tbl.Rows

                '同一レコードか判定
                If OIM0005INProw("TANKNUMBER") = OIM0005row("TANKNUMBER") Then
                    '画面入力テーブル項目設定
                    OIM0005INProw("LINECNT") = OIM0005row("LINECNT")
                    OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    OIM0005INProw("UPDTIMSTP") = OIM0005row("UPDTIMSTP")
                    OIM0005INProw("SELECT") = 0
                    OIM0005INProw("HIDDEN") = 0

                    '項目テーブル項目設定
                    OIM0005row.ItemArray = OIM0005INProw.ItemArray

                    '〇名称設定
                    '原籍所有者
                    If Not String.IsNullOrEmpty(OIM0005row("ORIGINOWNERCODE")) Then
                        CODENAME_get("ORIGINOWNERCODE",
                                    OIM0005row("ORIGINOWNERCODE"),
                                    OIM0005row("ORIGINOWNERNAME"),
                                    WW_DUMMY)
                    End If
                    '名義所有者
                    If Not String.IsNullOrEmpty(OIM0005row("OWNERCODE")) Then
                        CODENAME_get("ORIGINOWNERCODE",
                                    OIM0005row("OWNERCODE"),
                                    OIM0005row("OWNERNAME"),
                                    WW_DUMMY)
                    End If
                    'リース先
                    If Not String.IsNullOrEmpty(OIM0005row("LEASECLASS")) Then
                        CODENAME_get("CAMPCODE",
                                    OIM0005row("LEASECODE"),
                                    OIM0005row("LEASENAME"),
                                    WW_DUMMY)
                    End If
                    '請負リース区分
                    If Not String.IsNullOrEmpty(OIM0005row("LEASECLASS")) Then
                        CODENAME_get("LEASECLASS",
                                    OIM0005row("LEASECLASS"),
                                    OIM0005row("LEASECLASSNAME"),
                                    WW_DUMMY)
                    End If
                    '自動延長名
                    If Not String.IsNullOrEmpty(OIM0005row("AUTOEXTENTION")) Then
                        CODENAME_get("AUTOEXTENTION",
                                    OIM0005row("AUTOEXTENTION"),
                                    OIM0005row("AUTOEXTENTIONNAME"),
                                    WW_DUMMY)
                    End If
                    '第三者使用者
                    If Not String.IsNullOrEmpty(OIM0005row("USERCODE")) Then
                        CODENAME_get("USERCODE",
                                    OIM0005row("USERCODE"),
                                    OIM0005row("USERNAME"),
                                    WW_DUMMY)
                    End If
                    '原常備駅
                    If Not String.IsNullOrEmpty(OIM0005row("CURRENTSTATIONCODE")) Then
                        CODENAME_get("STATIONPATTERN",
                                    OIM0005row("CURRENTSTATIONCODE"),
                                    OIM0005row("CURRENTSTATIONNAME"),
                                    WW_DUMMY)
                    End If
                    '臨時常備駅
                    If Not String.IsNullOrEmpty(OIM0005row("EXTRADINARYSTATIONCODE")) Then
                        CODENAME_get("STATIONPATTERN",
                                    OIM0005row("EXTRADINARYSTATIONCODE"),
                                    OIM0005row("EXTRADINARYSTATIONNAME"),
                                    WW_DUMMY)
                    End If
                    '原専用種別
                    If Not String.IsNullOrEmpty(OIM0005row("DEDICATETYPECODE")) Then
                        CODENAME_get("DEDICATETYPECODE",
                                    OIM0005row("DEDICATETYPECODE"),
                                    OIM0005row("DEDICATETYPENAME"),
                                    WW_DUMMY)
                    End If
                    '臨時専用種別
                    If Not String.IsNullOrEmpty(OIM0005row("EXTRADINARYTYPECODE")) Then
                        CODENAME_get("EXTRADINARYTYPECODE",
                                    OIM0005row("EXTRADINARYTYPECODE"),
                                    OIM0005row("EXTRADINARYTYPENAME"),
                                    WW_DUMMY)
                    End If
                    '油種大分類名
                    If Not String.IsNullOrEmpty(OIM0005row("BIGOILCODE")) Then
                        CODENAME_get("BIGOILCODE",
                                    OIM0005row("BIGOILCODE"),
                                    OIM0005row("BIGOILNAME"),
                                    WW_DUMMY)
                    End If
                    '油種中分類名
                    If Not String.IsNullOrEmpty(OIM0005row("MIDDLEOILCODE")) Then
                        CODENAME_get("MIDDLEOILCODE",
                                    OIM0005row("MIDDLEOILCODE"),
                                    OIM0005row("MIDDLEOILNAME"),
                                    WW_DUMMY)
                    End If
                    '運用場所
                    If Not String.IsNullOrEmpty(OIM0005row("OPERATIONBASECODE")) Then
                        CODENAME_get("BASE",
                                    OIM0005row("OPERATIONBASECODE"),
                                    OIM0005row("OPERATIONBASENAME"),
                                    WW_DUMMY)
                    End If
                    '運用場所（サブ）
                    If Not String.IsNullOrEmpty(OIM0005row("SUBOPERATIONBASECODE")) Then
                        CODENAME_get("BASE",
                                    OIM0005row("SUBOPERATIONBASECODE"),
                                    OIM0005row("SUBOPERATIONBASENAME"),
                                    WW_DUMMY)
                    End If
                    '塗色
                    If Not String.IsNullOrEmpty(OIM0005row("COLORCODE")) Then
                        CODENAME_get("COLORCODE",
                                    OIM0005row("COLORCODE"),
                                    OIM0005row("COLORNAME"),
                                    WW_DUMMY)
                    End If
                    'マーク名
                    If Not String.IsNullOrEmpty(OIM0005row("MARKCODE")) Then
                        CODENAME_get("MARKCODE",
                                    OIM0005row("MARKCODE"),
                                    OIM0005row("MARKNAME"),
                                    WW_DUMMY)
                    End If
                    'JXTG千葉タグ名
                    If Not String.IsNullOrEmpty(OIM0005row("JXTGTAGCODE2")) Then
                        CODENAME_get("TAGCODE",
                                    OIM0005row("JXTGTAGCODE2"),
                                    OIM0005row("JXTGTAGNAME2"),
                                    WW_DUMMY)
                    End If
                    '出光昭シタグ名
                    If Not String.IsNullOrEmpty(OIM0005row("IDSSTAGCODE")) Then
                        CODENAME_get("TAGCODE",
                                    OIM0005row("IDSSTAGCODE"),
                                    OIM0005row("IDSSTAGNAME"),
                                    WW_DUMMY)
                    End If
                    '取得先名
                    If Not String.IsNullOrEmpty(OIM0005row("OBTAINEDCODE")) Then
                        CODENAME_get("OBTAINEDCODE",
                                    OIM0005row("OBTAINEDCODE"),
                                    OIM0005row("OBTAINEDNAME"),
                                    WW_DUMMY)
                    End If
                    '利用フラグ
                    If Not String.IsNullOrEmpty(OIM0005row("USEDFLG")) Then
                        CODENAME_get("USEDFLG",
                                    OIM0005row("USEDFLG"),
                                    OIM0005row("USEDFLGNAME"),
                                    WW_DUMMY)
                    End If
                    '中間点検場所
                    If Not String.IsNullOrEmpty(OIM0005row("INTERINSPECTSTATION")) Then
                        CODENAME_get("STATIONFOCUSON",
                                    OIM0005row("INTERINSPECTSTATION"),
                                    OIM0005row("INTERINSPECTSTATIONNAME"),
                                    WW_DUMMY)
                    End If
                    '中間点検実施者
                    If Not String.IsNullOrEmpty(OIM0005row("INTERINSPECTORGCODE")) Then
                        CODENAME_get("ORG",
                                    OIM0005row("INTERINSPECTORGCODE"),
                                    OIM0005row("INTERINSPECTORGNAME"),
                                    WW_DUMMY)
                    End If
                    '自主点検場所
                    If Not String.IsNullOrEmpty(OIM0005row("SELFINSPECTSTATION")) Then
                        CODENAME_get("STATIONFOCUSON",
                                    OIM0005row("SELFINSPECTSTATION"),
                                    OIM0005row("SELFINSPECTSTATIONNAME"),
                                    WW_DUMMY)
                    End If
                    '自主点検実施者
                    If Not String.IsNullOrEmpty(OIM0005row("SELFINSPECTORGCODE")) Then
                        CODENAME_get("ORG",
                                    OIM0005row("SELFINSPECTORGCODE"),
                                    OIM0005row("SELFINSPECTORGNAME"),
                                    WW_DUMMY)
                    End If
                    '休車フラグ
                    If Not String.IsNullOrEmpty(OIM0005row("SUSPENDFLG")) Then
                        CODENAME_get("SUSPENDFLG",
                                    OIM0005row("SUSPENDFLG"),
                                    OIM0005row("SUSPENDFLGNAME"),
                                    WW_DUMMY)
                    End If
                    '内部塗装
                    If Not String.IsNullOrEmpty(OIM0005row("INTERNALCOATING")) Then
                        CODENAME_get("INTERNALCOATING",
                                    OIM0005row("INTERNALCOATING"),
                                    OIM0005row("INTERNALCOATINGNAME"),
                                    WW_DUMMY)
                    End If
                    '削除理由区分
                    If Not String.IsNullOrEmpty(OIM0005row("DELREASONKBN")) Then
                        CODENAME_get("DELREASONKBN",
                                    OIM0005row("DELREASONKBN"),
                                    OIM0005row("DELREASONKBNNAME"),
                                    WW_DUMMY)
                    End If

                    '発見フラグON
                    isFound = True
                    Exit For
                End If
            Next

            '同一レコードが発見できない場合は、追加する
            If Not isFound Then
                Dim nrow = OIM0005tbl.NewRow
                nrow.ItemArray = OIM0005INProw.ItemArray

                '画面入力テーブル項目設定
                nrow("LINECNT") = OIM0005tbl.Rows.Count + 1
                nrow("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                nrow("UPDTIMSTP") = "0"
                nrow("SELECT") = 0
                nrow("HIDDEN") = 0

                '〇名称設定
                '原籍所有者
                If Not String.IsNullOrEmpty(nrow("ORIGINOWNERCODE")) Then
                    CODENAME_get("ORIGINOWNERCODE",
                                nrow("ORIGINOWNERCODE"),
                                nrow("ORIGINOWNERNAME"),
                                WW_DUMMY)
                End If
                '名義所有者
                If Not String.IsNullOrEmpty(nrow("OWNERCODE")) Then
                    CODENAME_get("ORIGINOWNERCODE",
                                nrow("OWNERCODE"),
                                nrow("OWNERNAME"),
                                WW_DUMMY)
                End If
                'リース先
                If Not String.IsNullOrEmpty(nrow("LEASECLASS")) Then
                    CODENAME_get("CAMPCODE",
                                nrow("LEASECODE"),
                                nrow("LEASENAME"),
                                WW_DUMMY)
                End If
                '請負リース区分
                If Not String.IsNullOrEmpty(nrow("LEASECLASS")) Then
                    CODENAME_get("LEASECLASS",
                                nrow("LEASECLASS"),
                                nrow("LEASECLASSNAME"),
                                WW_DUMMY)
                End If
                '自動延長名
                If Not String.IsNullOrEmpty(nrow("AUTOEXTENTION")) Then
                    CODENAME_get("AUTOEXTENTION",
                                nrow("AUTOEXTENTION"),
                                nrow("AUTOEXTENTIONNAME"),
                                WW_DUMMY)
                End If
                '第三者使用者
                If Not String.IsNullOrEmpty(nrow("USERCODE")) Then
                    CODENAME_get("USERCODE",
                                nrow("USERCODE"),
                                nrow("USERNAME"),
                                WW_DUMMY)
                End If
                '原常備駅
                If Not String.IsNullOrEmpty(nrow("CURRENTSTATIONCODE")) Then
                    CODENAME_get("STATIONPATTERN",
                                nrow("CURRENTSTATIONCODE"),
                                nrow("CURRENTSTATIONNAME"),
                                WW_DUMMY)
                End If
                '臨時常備駅
                If Not String.IsNullOrEmpty(nrow("EXTRADINARYSTATIONCODE")) Then
                    CODENAME_get("STATIONPATTERN",
                                nrow("EXTRADINARYSTATIONCODE"),
                                nrow("EXTRADINARYSTATIONNAME"),
                                WW_DUMMY)
                End If
                '原専用種別
                If Not String.IsNullOrEmpty(nrow("DEDICATETYPECODE")) Then
                    CODENAME_get("DEDICATETYPECODE",
                                nrow("DEDICATETYPECODE"),
                                nrow("DEDICATETYPENAME"),
                                WW_DUMMY)
                End If
                '臨時専用種別
                If Not String.IsNullOrEmpty(nrow("EXTRADINARYTYPECODE")) Then
                    CODENAME_get("EXTRADINARYTYPECODE",
                                nrow("EXTRADINARYTYPECODE"),
                                nrow("EXTRADINARYTYPENAME"),
                                WW_DUMMY)
                End If
                '油種大分類名
                If Not String.IsNullOrEmpty(nrow("BIGOILCODE")) Then
                    CODENAME_get("BIGOILCODE",
                                nrow("BIGOILCODE"),
                                nrow("BIGOILNAME"),
                                WW_DUMMY)
                End If
                '油種中分類名
                If Not String.IsNullOrEmpty(nrow("MIDDLEOILCODE")) Then
                    CODENAME_get("MIDDLEOILCODE",
                                nrow("MIDDLEOILCODE"),
                                nrow("MIDDLEOILNAME"),
                                WW_DUMMY)
                End If
                '運用場所
                If Not String.IsNullOrEmpty(nrow("OPERATIONBASECODE")) Then
                    CODENAME_get("BASE",
                                nrow("OPERATIONBASECODE"),
                                nrow("OPERATIONBASENAME"),
                                WW_DUMMY)
                End If
                '運用場所（サブ）
                If Not String.IsNullOrEmpty(nrow("SUBOPERATIONBASECODE")) Then
                    CODENAME_get("BASE",
                                nrow("SUBOPERATIONBASECODE"),
                                nrow("SUBOPERATIONBASENAME"),
                                WW_DUMMY)
                End If
                '塗色
                If Not String.IsNullOrEmpty(nrow("COLORCODE")) Then
                    CODENAME_get("COLORCODE",
                                nrow("COLORCODE"),
                                nrow("COLORNAME"),
                                WW_DUMMY)
                End If
                'マーク名
                If Not String.IsNullOrEmpty(nrow("MARKCODE")) Then
                    CODENAME_get("MARKCODE",
                                nrow("MARKCODE"),
                                nrow("MARKNAME"),
                                WW_DUMMY)
                End If
                'JXTG千葉タグ名
                If Not String.IsNullOrEmpty(nrow("JXTGTAGCODE2")) Then
                    CODENAME_get("TAGCODE",
                                nrow("JXTGTAGCODE2"),
                                nrow("JXTGTAGNAME2"),
                                WW_DUMMY)
                End If
                '出光昭シタグ名
                If Not String.IsNullOrEmpty(nrow("IDSSTAGCODE")) Then
                    CODENAME_get("TAGCODE",
                                nrow("IDSSTAGCODE"),
                                nrow("IDSSTAGNAME"),
                                WW_DUMMY)
                End If
                '取得先名
                If Not String.IsNullOrEmpty(nrow("OBTAINEDCODE")) Then
                    CODENAME_get("OBTAINEDCODE",
                                nrow("OBTAINEDCODE"),
                                nrow("OBTAINEDNAME"),
                                WW_DUMMY)
                End If
                '利用フラグ
                If Not String.IsNullOrEmpty(nrow("USEDFLG")) Then
                    CODENAME_get("USEDFLG",
                                nrow("USEDFLG"),
                                nrow("USEDFLGNAME"),
                                WW_DUMMY)
                End If
                '中間点検場所
                If Not String.IsNullOrEmpty(nrow("INTERINSPECTSTATION")) Then
                    CODENAME_get("STATIONFOCUSON",
                                nrow("INTERINSPECTSTATION"),
                                nrow("INTERINSPECTSTATIONNAME"),
                                WW_DUMMY)
                End If
                '中間点検実施者
                If Not String.IsNullOrEmpty(nrow("INTERINSPECTORGCODE")) Then
                    CODENAME_get("ORG",
                                nrow("INTERINSPECTORGCODE"),
                                nrow("INTERINSPECTORGNAME"),
                                WW_DUMMY)
                End If
                '自主点検場所
                If Not String.IsNullOrEmpty(nrow("SELFINSPECTSTATION")) Then
                    CODENAME_get("STATIONFOCUSON",
                                nrow("SELFINSPECTSTATION"),
                                nrow("SELFINSPECTSTATIONNAME"),
                                WW_DUMMY)
                End If
                '自主点検実施者
                If Not String.IsNullOrEmpty(nrow("SELFINSPECTORGCODE")) Then
                    CODENAME_get("ORG",
                                nrow("SELFINSPECTORGCODE"),
                                nrow("SELFINSPECTORGNAME"),
                                WW_DUMMY)
                End If
                '休車フラグ
                If Not String.IsNullOrEmpty(nrow("SUSPENDFLG")) Then
                    CODENAME_get("SUSPENDFLG",
                                nrow("SUSPENDFLG"),
                                nrow("SUSPENDFLGNAME"),
                                WW_DUMMY)
                End If
                '内部塗装
                If Not String.IsNullOrEmpty(nrow("INTERNALCOATING")) Then
                    CODENAME_get("INTERNALCOATING",
                                nrow("INTERNALCOATING"),
                                nrow("INTERNALCOATINGNAME"),
                                WW_DUMMY)
                End If
                '削除理由区分
                If Not String.IsNullOrEmpty(nrow("DELREASONKBN")) Then
                    CODENAME_get("DELREASONKBN",
                                nrow("DELREASONKBN"),
                                nrow("DELREASONKBNNAME"),
                                WW_DUMMY)
                End If

                OIM0005tbl.Rows.Add(nrow)
            End If
        Next

    End Sub

#Region "未使用"
    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0005INProw As DataRow)

        For Each OIM0005row As DataRow In OIM0005tbl.Rows

            '同一レコードか判定
            If OIM0005INProw("TANKNUMBER") = OIM0005row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIM0005INProw("LINECNT") = OIM0005row("LINECNT")
                OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0005INProw("UPDTIMSTP") = OIM0005row("UPDTIMSTP")
                OIM0005INProw("SELECT") = 1
                OIM0005INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0005row.ItemArray = OIM0005INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0005INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0005row As DataRow = OIM0005tbl.NewRow
        OIM0005row.ItemArray = OIM0005INProw.ItemArray

        OIM0005row("LINECNT") = OIM0005tbl.Rows.Count + 1
        If OIM0005INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            OIM0005row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0005row("UPDTIMSTP") = "0"
        OIM0005row("SELECT") = 1
        OIM0005row("HIDDEN") = 0

        OIM0005tbl.Rows.Add(OIM0005row)

    End Sub

    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0005INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0005INProw As DataRow)

        For Each OIM0005row As DataRow In OIM0005tbl.Rows

            '同一レコードか判定
            If OIM0005INProw("TANKNUMBER") = OIM0005row("TANKNUMBER") Then
                '画面入力テーブル項目設定
                OIM0005INProw("LINECNT") = OIM0005row("LINECNT")
                OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0005INProw("UPDTIMSTP") = OIM0005row("UPDTIMSTP")
                OIM0005INProw("SELECT") = 1
                OIM0005INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0005row.ItemArray = OIM0005INProw.ItemArray
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
            prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

            Select Case I_FIELD
                Case "CAMPCODE"                     '会社コード
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"                          '運用部署
                    prmData = work.CreateORGParam(Master.USERCAMP)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UNIT"                         '荷重単位, 容積単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_UNIT, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORIGINOWNERCODE"              '原籍所有者C
                    prmData = work.CreateOriginOwnercodeParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORIGINOWNERCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LEASECLASS"                   '請負リース区分C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_LEASECLASS, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "AUTOEXTENTION"                '自動延長
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "AUTOEXTENTION")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USERCODE"                     '第三者使用者C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_THIRDUSER, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATIONPATTERN"　              '原常備駅C、臨時常備駅C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DEDICATETYPECODE"             '原専用種別C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DEDICATETYPE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "EXTRADINARYTYPECODE"          '臨時専用種別C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_EXTRADINARYTYPE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BIGOILCODE"                   '油種大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BIGOILCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MIDDLEOILCODE"                '油種中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_MIDDLEOILCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "BASE"                         '運用基地
                    prmData = work.CreateBaseParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_BASE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "COLORCODE"                    '塗色C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COLOR, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "MARKCODE"                     'マークコード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "MARKCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "TAGCODE"                      'タグコード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "TAGCODE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "OBTAINEDCODE"                 '取得先C
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_OBTAINED, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "JRTANKTYPE"                   'JR車種コード
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "JRTANKTYPE")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LENGTHFLG"                    '長さフラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "LENGTHFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "USEDFLG"                      '利用フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"                       '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "STATIONFOCUSON"　             '中間点検場所、自主点検場所(使用駅のみ)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATIONCODE_FOCUSON, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "SUSPENDFLG"                   '休車フラグ
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "SUSPENDFLG")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "INTERNALCOATING"              '内部塗装
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "INTERNALCOATING")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELREASONKBN"                 '削除理由区分
                    prmData = work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELREASONKBN")
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

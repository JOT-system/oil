''************************************************************
' タンク車マスタメンテ登録画面
' 作成日 2019/11/08
' 更新日 2019/11/08
' 作成者 JOT遠藤
' 更新車 JOT遠藤
'
' 修正履歴:
'         :
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
    Private OIM0005tbl As DataTable                                  '一覧格納用テーブル
    Private OIM0005INPtbl As DataTable                               'チェック用テーブル
    Private OIM0005UPDtbl As DataTable                               '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数

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

        '原籍所有者C, 原籍所有者
        WF_ORIGINOWNERCODE.Text = work.WF_SEL_ORIGINOWNERCODE.Text
        CODENAME_get("ORIGINOWNERCODE", WF_ORIGINOWNERCODE.Text, WF_ORIGINOWNERCODE_TEXT.Text, WW_RTN_SW)

        '名義所有者C, 名義所有者
        WF_OWNERCODE.Text = work.WF_SEL_OWNERCODE.Text
        CODENAME_get("ORIGINOWNERCODE", WF_OWNERCODE.Text, WF_OWNERCODE_TEXT.Text, WW_RTN_SW)

        'リース先C, リース先
        WF_LEASECODE.Text = work.WF_SEL_LEASECODE.Text
        CODENAME_get("CAMPCODE", WF_LEASECODE.Text, WF_LEASECODE_TEXT.Text, WW_RTN_SW)

        'リース区分C, リース区分
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

        '運用基地C, 運用場所
        WF_OPERATIONBASECODE.Text = work.WF_SEL_OPERATIONBASECODE.Text
        CODENAME_get("BASE", WF_OPERATIONBASECODE.Text, WF_OPERATIONBASECODE_TEXT.Text, WW_RTN_SW)

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

        '削除フラグ
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

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
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

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

        OIM0005INProw("TANKNUMBER") = WF_TANKNUMBER.Text                                    'JOT車番
        OIM0005INProw("MODEL") = WF_MODEL.Text                                              '形式
        OIM0005INProw("MODELKANA") = WF_MODELKANA.Text                                      '形式カナ
        OIM0005INProw("LOAD") = WF_LOAD.Text                                                '荷重
        OIM0005INProw("LOADUNIT") = WF_LOADUNIT.Text                                        '荷重単位
        OIM0005INProw("VOLUME") = WF_VOLUME.Text                                            '容積
        OIM0005INProw("VOLUMEUNIT") = WF_VOLUMEUNIT.Text                                    '容積単位
        OIM0005INProw("MYWEIGHT") = WF_MYWEIGHT.Text                                        '自重
        OIM0005INProw("ORIGINOWNERCODE") = WF_ORIGINOWNERCODE.Text                          '原籍所有者C
        OIM0005INProw("ORIGINOWNERNAME") = WF_ORIGINOWNERCODE_TEXT.Text                     '原籍所有者
        OIM0005INProw("OWNERCODE") = WF_OWNERCODE.Text                                      '名義所有者C
        OIM0005INProw("OWNERNAME") = WF_OWNERCODE_TEXT.Text                                 '名義所有者
        OIM0005INProw("LEASECODE") = WF_LEASECODE.Text                                      'リース先C
        OIM0005INProw("LEASENAME") = WF_LEASECODE_TEXT.Text                                 'リース先
        OIM0005INProw("LEASECLASS") = WF_LEASECLASS.Text                                    'リース区分C
        OIM0005INProw("LEASECLASSNEMAE") = WF_LEASECLASS_TEXT.Text                          'リース区分
        OIM0005INProw("AUTOEXTENTION") = WF_AUTOEXTENTION.Text                              '自動延長
        OIM0005INProw("AUTOEXTENTIONNAME") = WF_AUTOEXTENTION_TEXT.Text                     '自動延長名
        OIM0005INProw("LEASESTYMD") = WF_LEASESTYMD.Text                                    'リース開始年月日
        OIM0005INProw("LEASEENDYMD") = WF_LEASEENDYMD.Text                                  'リース満了年月日
        OIM0005INProw("USERCODE") = WF_USERCODE.Text                                        '第三者使用者C
        OIM0005INProw("USERNAME") = WF_USERCODE_TEXT.Text                                   '第三者使用者
        OIM0005INProw("CURRENTSTATIONCODE") = WF_CURRENTSTATIONCODE.Text                    '原常備駅C
        OIM0005INProw("CURRENTSTATIONNAME") = WF_CURRENTSTATIONCODE_TEXT.Text               '原常備駅
        OIM0005INProw("EXTRADINARYSTATIONCODE") = WF_EXTRADINARYSTATIONCODE.Text            '臨時常備駅C
        OIM0005INProw("EXTRADINARYSTATIONNAME") = WF_EXTRADINARYSTATIONCODE_TEXT.Text       '臨時常備駅
        OIM0005INProw("USERLIMIT") = WF_USERLIMIT.Text                                      '第三者使用期限
        OIM0005INProw("LIMITTEXTRADIARYSTATION") = WF_LIMITTEXTRADIARYSTATION.Text          '臨時常備駅期限
        OIM0005INProw("DEDICATETYPECODE") = WF_DEDICATETYPECODE.Text                        '原専用種別C
        OIM0005INProw("DEDICATETYPENAME") = WF_DEDICATETYPECODE_TEXT.Text                   '原専用種別
        OIM0005INProw("EXTRADINARYTYPECODE") = WF_EXTRADINARYTYPECODE.Text                  '臨時専用種別C
        OIM0005INProw("EXTRADINARYTYPENAME") = WF_EXTRADINARYTYPECODE_TEXT.Text             '臨時専用種別
        OIM0005INProw("EXTRADINARYLIMIT") = WF_EXTRADINARYLIMIT.Text                        '臨時専用期限
        OIM0005INProw("BIGOILCODE") = WF_BIGOILCODE.Text                                    '油種大分類コード
        OIM0005INProw("BIGOILNAME") = WF_BIGOILCODE_TEXT.Text                               '油種大分類名
        OIM0005INProw("OPERATIONBASECODE") = WF_OPERATIONBASECODE.Text                      '運用基地C
        OIM0005INProw("OPERATIONBASENAME") = WF_OPERATIONBASECODE_TEXT.Text                 '運用場所
        OIM0005INProw("COLORCODE") = WF_COLORCODE.Text                                      '塗色C
        OIM0005INProw("COLORNAME") = WF_COLORCODE_TEXT.Text                                 '塗色
        OIM0005INProw("MARKCODE") = WF_MARKCODE.Text                                        'マークコード
        OIM0005INProw("MARKNAME") = WF_MARKCODE_TEXT.Text                                   'マーク名
        OIM0005INProw("JXTGTAGCODE1") = WF_JXTGTAGCODE1.Text                                'JXTG仙台タグコード
        OIM0005INProw("JXTGTAGNAME1") = WF_JXTGTAGCODE1_TEXT.Text                           'JXTG仙台タグ名
        OIM0005INProw("JXTGTAGCODE2") = WF_JXTGTAGCODE2.Text                                'JXTG千葉タグコード
        OIM0005INProw("JXTGTAGNAME2") = WF_JXTGTAGCODE2_TEXT.Text                           'JXTG千葉タグ名
        OIM0005INProw("JXTGTAGCODE3") = WF_JXTGTAGCODE3.Text                                'JXTG川崎タグコード
        OIM0005INProw("JXTGTAGNAME3") = WF_JXTGTAGCODE3_TEXT.Text                           'JXTG川崎タグ名
        OIM0005INProw("JXTGTAGCODE4") = WF_JXTGTAGCODE4.Text                                'JXTG根岸タグコード
        OIM0005INProw("JXTGTAGNAME4") = WF_JXTGTAGCODE4_TEXT.Text                           'JXTG根岸タグ名
        OIM0005INProw("IDSSTAGCODE") = WF_IDSSTAGCODE.Text                                  '出光昭シタグコード
        OIM0005INProw("IDSSTAGNAME") = WF_IDSSTAGCODE_TEXT.Text                             '出光昭シタグ名
        OIM0005INProw("COSMOTAGCODE") = WF_COSMOTAGCODE.Text                                'コスモタグコード
        OIM0005INProw("COSMOTAGNAME") = WF_COSMOTAGCODE_TEXT.Text                           'コスモタグ名
        OIM0005INProw("RESERVE1") = WF_RESERVE1.Text                                        '予備1
        OIM0005INProw("RESERVE2") = WF_RESERVE2.Text                                        '予備2
        OIM0005INProw("JRINSPECTIONDATE") = WF_JRINSPECTIONDATE.Text                        '次回交検年月日(JR）
        OIM0005INProw("INSPECTIONDATE") = WF_INSPECTIONDATE.Text                            '次回交検年月日
        OIM0005INProw("JRSPECIFIEDDATE") = WF_JRSPECIFIEDDATE.Text                          '次回指定年月日(JR)
        OIM0005INProw("SPECIFIEDDATE") = WF_SPECIFIEDDATE.Text                              '次回指定年月日
        OIM0005INProw("JRALLINSPECTIONDATE") = WF_JRALLINSPECTIONDATE.Text                  '次回全検年月日(JR) 
        OIM0005INProw("ALLINSPECTIONDATE") = WF_ALLINSPECTIONDATE.Text                      '次回全検年月日
        OIM0005INProw("PREINSPECTIONDATE") = WF_PREINSPECTIONDATE.Text                      '前回全検年月日
        OIM0005INProw("GETDATE") = WF_GETDATE.Text                                          '取得年月日
        OIM0005INProw("TRANSFERDATE") = WF_TRANSFERDATE.Text                                '車籍編入年月日
        OIM0005INProw("OBTAINEDCODE") = WF_OBTAINEDCODE.Text                                '取得先C
        OIM0005INProw("OBTAINEDNAME") = WF_OBTAINEDCODE_TEXT.Text                           '取得先名
        OIM0005INProw("PROGRESSYEAR") = WF_PROGRESSYEAR.Text                                '現在経年
        OIM0005INProw("NEXTPROGRESSYEAR") = WF_NEXTPROGRESSYEAR.Text                        '次回全検時経年
        OIM0005INProw("EXCLUDEDATE") = WF_EXCLUDEDATE.Text                                  '車籍除外年月日
        OIM0005INProw("RETIRMENTDATE") = WF_RETIRMENTDATE.Text                              '資産除却年月日
        OIM0005INProw("JRTANKNUMBER") = WF_JRTANKNUMBER.Text                                'JR車番
        OIM0005INProw("JRTANKTYPE") = WF_JRTANKTYPE.Text                                    'JR車種コード
        OIM0005INProw("OLDTANKNUMBER") = WF_OLDTANKNUMBER.Text                              '旧JOT車番
        OIM0005INProw("OTTANKNUMBER") = WF_OTTANKNUMBER.Text                                'OT車番
        OIM0005INProw("JXTGTANKNUMBER1") = WF_JXTGTANKNUMBER1.Text                          'JXTG仙台車番
        OIM0005INProw("JXTGTANKNUMBER2") = WF_JXTGTANKNUMBER2.Text                          'JXTG千葉車番
        OIM0005INProw("JXTGTANKNUMBER3") = WF_JXTGTANKNUMBER3.Text                          'JXTG川崎車番
        OIM0005INProw("JXTGTANKNUMBER4") = WF_JXTGTANKNUMBER4.Text                          'JXTG根岸車番
        OIM0005INProw("COSMOTANKNUMBER") = WF_COSMOTANKNUMBER.Text                          'コスモ車番
        OIM0005INProw("FUJITANKNUMBER") = WF_FUJITANKNUMBER.Text                            '富士石油車番
        OIM0005INProw("SHELLTANKNUMBER") = WF_SHELLTANKNUMBER.Text                          '出光昭シ車番
        OIM0005INProw("SAPSHELLTANKNUMBER") = WF_SAPSHELLTANKNUMBER.Text                    '出光昭シSAP車番
        OIM0005INProw("RESERVE3") = WF_RESERVE3.Text                                        '予備
        OIM0005INProw("USEDFLG") = WF_USEDFLG.Text                                          '利用フラグ
        OIM0005INProw("DELFLG") = WF_DELFLG.Text                                            '削除フラグ

        '○ チェック用テーブルに登録する
        OIM0005INPtbl.Rows.Add(OIM0005INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()

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
        WF_ORIGINOWNERCODE.Text = ""                '原籍所有者C
        WF_ORIGINOWNERCODE_TEXT.Text = ""           '原籍所有者
        WF_OWNERCODE.Text = ""                      '名義所有者C
        WF_OWNERCODE_TEXT.Text = ""                 '名義所有者
        WF_LEASECODE.Text = ""                      'リース先C
        WF_LEASECODE_TEXT.Text = ""                 'リース先
        WF_LEASECLASS.Text = ""                     'リース区分C
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
        WF_OPERATIONBASECODE.Text = ""              '運用基地C
        WF_OPERATIONBASECODE_TEXT.Text = ""         '運用場所
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
        WF_DELFLG.Text = ""                         '削除フラグ

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
                        End Select
                        .ActiveCalendar()

                    Case Else
                        'それ以外
                        Dim prmData As New Hashtable
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value

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

                        End Select

                        .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                        .ActiveListBox()
                End Select
            End With
        End If

    End Sub

    Private Sub CreateTankNumber(ByVal I_TANKNUMBER As String, ByVal I_MODEL As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional toNarrow As Boolean = False)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrWhiteSpace(I_TANKNUMBER) OrElse String.IsNullOrWhiteSpace(I_MODEL) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            If toNarrow Then
                O_TEXT = String.Format("{0}-{1}", StrConv(I_MODEL, VbStrConv.Narrow), I_TANKNUMBER)
            Else

            End If
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.CAST_FORMAT_ERROR_EX
            Exit Sub
        End Try

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
                        WF_JRTANKNUMBER.Text = String.Format("{0}-{1}", StrConv(modelL, VbStrConv.Narrow), tankNumber)
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

            '形式
            Case "WF_MODEL"
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)
            '荷重単位
            Case "WF_LOADUNIT"
                CODENAME_get("UNIT", WF_LOADUNIT.Text, WF_LOADUNIT_TEXT.Text, WW_RTN_SW)
            '容積単位
            Case "WF_VOLUMEUNIT"
                CODENAME_get("UNIT", WF_VOLUMEUNIT.Text, WF_VOLUMEUNIT_TEXT.Text, WW_RTN_SW)
            '原籍所有者C
            Case "WF_ORIGINOWNERCODE"
                CODENAME_get("ORIGINOWNERCODE", WF_ORIGINOWNERCODE.Text, WF_ORIGINOWNERCODE_TEXT.Text, WW_RTN_SW)
            '名義所有者C
            Case "WF_OWNERCODE"
                CODENAME_get("ORIGINOWNERCODE", WF_OWNERCODE.Text, WF_OWNERCODE_TEXT.Text, WW_RTN_SW)
            'リース先C
            Case "WF_LEASECODE"
                CODENAME_get("CAMPCODE", WF_LEASECODE.Text, WF_LEASECODE_TEXT.Text, WW_RTN_SW)
            'リース区分C
            Case "WF_LEASECLASS"
                CODENAME_get("LEASECLASS", WF_LEASECLASS.Text, WF_LEASECLASS_TEXT.Text, WW_RTN_SW)
            '自動延長
            Case "WF_AUTOEXTENTION"
                CODENAME_get("AUTOEXTENTION", WF_AUTOEXTENTION.Text, WF_AUTOEXTENTION_TEXT.Text, WW_RTN_SW)
            '第三者使用者C
            Case "WF_USERCODE"
                CODENAME_get("USERCODE", WF_USERCODE.Text, WF_USERCODE_TEXT.Text, WW_RTN_SW)
            '原常備駅C
            Case "WF_CURRENTSTATIONCODE"
                CODENAME_get("STATIONPATTERN", WF_CURRENTSTATIONCODE.Text, WF_CURRENTSTATIONCODE_TEXT.Text, WW_RTN_SW)
            '原専用種別C
            Case "WF_DEDICATETYPECODE"
                CODENAME_get("DEDICATETYPECODE", WF_DEDICATETYPECODE.Text, WF_DEDICATETYPECODE_TEXT.Text, WW_RTN_SW)
            '臨時常備駅C
            Case "WF_EXTRADINARYSTATIONCODE"
                CODENAME_get("STATIONPATTERN", WF_EXTRADINARYSTATIONCODE.Text, WF_EXTRADINARYSTATIONCODE_TEXT.Text, WW_RTN_SW)
            '臨時専用種別C
            Case "WF_EXTRADINARYTYPECODE"
                CODENAME_get("EXTRADINARYTYPECODE", WF_EXTRADINARYTYPECODE.Text, WF_EXTRADINARYTYPECODE_TEXT.Text, WW_RTN_SW)
            '油種大分類コード
            Case "WF_BIGOILCODE"
                CODENAME_get("BIGOILCODE", WF_BIGOILCODE.Text, WF_BIGOILCODE_TEXT.Text, WW_RTN_SW)
            '運用基地C
            Case "WF_OPERATIONBASECODE"
                CODENAME_get("BASE", WF_OPERATIONBASECODE.Text, WF_OPERATIONBASECODE_TEXT.Text, WW_RTN_SW)
            '塗色C
            Case "WF_COLORCODE"
                CODENAME_get("COLORCODE", WF_COLORCODE.Text, WF_COLORCODE_TEXT.Text, WW_RTN_SW)
            'マークコード
            Case "WF_MARKCODE"
                CODENAME_get("MARKCODE", WF_MARKCODE.Text, WF_MARKCODE_TEXT.Text, WW_RTN_SW)
            'JXTG千葉タグコード
            Case "WF_JXTGTAGCODE2"
                CODENAME_get("JXTGTAGCODE2", WF_JXTGTAGCODE2.Text, WF_JXTGTAGCODE2_TEXT.Text, WW_RTN_SW)
            '出光昭シタグコード
            Case "WF_IDSSTAGCODE"
                CODENAME_get("IDSSTAGCODE", WF_IDSSTAGCODE.Text, WF_IDSSTAGCODE_TEXT.Text, WW_RTN_SW)
            '取得先C
            Case "WF_OBTAINEDCODE"
                CODENAME_get("OBTAINEDCODE", WF_OBTAINEDCODE.Text, WF_OBTAINEDCODE_TEXT.Text, WW_RTN_SW)
            'JR車種コード
            Case "WF_JRTANKTYPE"
                CODENAME_get("JRTANKTYPE", WF_JRTANKTYPE.Text, WF_JRTANKTYPE_TEXT.Text, WW_RTN_SW)
            '利用フラグ
            Case "WF_USEDFLG"
                CODENAME_get("USEDFLG", WF_USEDFLG.Text, WF_USEDFLG_TEXT.Text, WW_RTN_SW)
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

                Case "WF_LEASECLASS"    'リース区分C
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

                Case "WF_OPERATIONBASECODE"    '運用基地C
                    WF_OPERATIONBASECODE.Text = WW_SelectValue
                    WF_OPERATIONBASECODE_TEXT.Text = WW_SelectText
                    WF_OPERATIONBASECODE.Focus()

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

                Case "WF_USEDFLG"    '利用フラグ
                    WF_USEDFLG.Text = WW_SelectValue
                    WF_USEDFLG_TEXT.Text = WW_SelectText
                    WF_USEDFLG.Focus()

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

                Case "WF_LEASECLASS"                    'リース区分C
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

                Case "WF_USEDFLG"                       '利用フラグ
                    WF_USEDFLG.Focus()

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
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0005INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
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

            'JOT車番(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TANKNUMBER", OIM0005INProw("TANKNUMBER"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                ''値存在チェック
                'CODENAME_get("TANKNUMBER", OIM0005INProw("TANKNUMBER"), WW_DUMMY, WW_RTN_SW)
                'If Not isNormal(WW_RTN_SW) Then
                '    WW_CheckMES1 = "・更新できないレコード(JOT車番入力エラー)です。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                '    WW_LINE_ERR = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・更新できないレコード(JOT車番入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '原籍所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORIGINOWNERCODE", OIM0005INProw("ORIGINOWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("ORIGINOWNERCODE", OIM0005INProw("ORIGINOWNERCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(原籍所有者C入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原籍所有者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '名義所有者C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OWNERCODE", OIM0005INProw("OWNERCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("ORIGINOWNERCODE", OIM0005INProw("OWNERCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(名義所有者C入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(名義所有者C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '原常備駅C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CURRENTSTATIONCODE", OIM0005INProw("CURRENTSTATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("STATIONPATTERN", OIM0005INProw("CURRENTSTATIONCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(原常備駅C入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(原常備駅C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If



            '運用基地C(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "OPERATIONBASECODE", OIM0005INProw("OPERATIONBASECODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("BASE", OIM0005INProw("OPERATIONBASECODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(運用基地C入力エラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(運用基地C入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '車籍編入年月日(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "TRANSFERDATE", OIM0005INProw("TRANSFERDATE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIM0005INProw("TRANSFERDATE"), "車籍編入年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(車籍編入年月日入力エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0005INProw("TRANSFERDATE") = CDate(OIM0005INProw("TRANSFERDATE")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(車籍編入年月日入力エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '利用フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "USEDFLG", OIM0005INProw("USEDFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("USEDFLG", OIM0005INProw("USEDFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(利用フラグエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(利用フラグエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            '油種大分類コード(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BIGOILCODE", OIM0005INProw("BIGOILCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("BIGOILCODE", OIM0005INProw("BIGOILCODE"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(油種大分類コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0005INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(油種大分類コードエラー)です。"
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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者C =" & OIM0005row("ORIGINOWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 原籍所有者 =" & OIM0005row("ORIGINOWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者C =" & OIM0005row("OWNERCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 名義所有者 =" & OIM0005row("OWNERNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先C =" & OIM0005row("LEASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース先 =" & OIM0005row("LEASENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分C =" & OIM0005row("LEASECLASS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> リース区分 =" & OIM0005row("LEASECLASSNEMAE") & " , "
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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用基地C =" & OIM0005row("OPERATIONBASECODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 運用場所 =" & OIM0005row("OPERATIONBASENAME") & " , "
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
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除フラグ =" & OIM0005row("DELFLG")
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

            'KEY項目が等しい時
            For Each OIM0005row As DataRow In OIM0005tbl.Rows
                If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0005row("TANKNUMBER") = OIM0005INProw("TANKNUMBER") AndAlso
                        OIM0005row("MODEL") = OIM0005INProw("MODEL") AndAlso
                        OIM0005row("MODELKANA") = OIM0005INProw("MODELKANA") AndAlso
                        OIM0005row("LOAD") = OIM0005INProw("LOAD") AndAlso
                        OIM0005row("LOADUNIT") = OIM0005INProw("LOADUNIT") AndAlso
                        OIM0005row("VOLUME") = OIM0005INProw("VOLUME") AndAlso
                        OIM0005row("VOLUMEUNIT") = OIM0005INProw("VOLUMEUNIT") AndAlso
                        OIM0005row("MYWEIGHT") = OIM0005INProw("MYWEIGHT") AndAlso
                        OIM0005row("ORIGINOWNERCODE") = OIM0005INProw("ORIGINOWNERCODE") AndAlso
                        OIM0005row("ORIGINOWNERNAME") = OIM0005INProw("ORIGINOWNERNAME") AndAlso
                        OIM0005row("OWNERCODE") = OIM0005INProw("OWNERCODE") AndAlso
                        OIM0005row("OWNERNAME") = OIM0005INProw("OWNERNAME") AndAlso
                        OIM0005row("LEASECODE") = OIM0005INProw("LEASECODE") AndAlso
                        OIM0005row("LEASENAME") = OIM0005INProw("LEASENAME") AndAlso
                        OIM0005row("LEASECLASS") = OIM0005INProw("LEASECLASS") AndAlso
                        OIM0005row("LEASECLASSNEMAE") = OIM0005INProw("LEASECLASSNEMAE") AndAlso
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
                        OIM0005row("OPERATIONBASECODE") = OIM0005INProw("OPERATIONBASECODE") AndAlso
                        OIM0005row("OPERATIONBASENAME") = OIM0005INProw("OPERATIONBASENAME") AndAlso
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
                        OIM0005row("DELFLG") = OIM0005INProw("DELFLG") AndAlso
                        OIM0005INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0005INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0005INProw As DataRow In OIM0005INPtbl.Rows
            Select Case OIM0005INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0005INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0005INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0005INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0005INProw)
            End Select
        Next

    End Sub

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
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORG"                          '運用部署
                    prmData = work.CreateORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "UNIT"                         '荷重単位, 容積単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_UNIT, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "ORIGINOWNERCODE"              '原籍所有者C
                    prmData = work.CreateOriginOwnercodeParam(work.WF_SEL_CAMPCODE.Text, I_VALUE)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORIGINOWNERCODE, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "LEASECLASS"                   'リース区分C
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
                Case "USEDFLG"                      '利用フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY, I_VALUE, O_TEXT, O_RTN, prmData)
                Case "DELFLG"                       '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, prmData)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

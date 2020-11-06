''************************************************************
' 会社マスタメンテ登録画面
' 作成日 2020/05/26
' 更新日 2020/05/26
' 作成者 JOT廣田
' 更新者 JOT廣田
'
' 修正履歴:新規作成
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 組織マスタ登録（登録）
''' </summary>
''' <remarks></remarks>
Public Class OIM0001CampCreate
    Inherits Page

    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private OIM0001tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0001INPtbl As DataTable                              'チェック用テーブル
    Private OIM0001UPDtbl As DataTable                              '更新用テーブル

    Private Const CONST_DISPROWCOUNT As Integer = 45                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 20                 'マウススクロール時稼働行数
    Private Const CONST_DETAIL_TABID As String = "DTL1"             '明細部ID

    'Private Const CONST_PATTERN1 As String = "1"                    'モデル距離パターン　届先のみ
    'Private Const CONST_PATTERN2 As String = "2"                    'モデル距離パターン　届先、出荷場所
    'Private Const CONST_PATTERN3 As String = "3"                    'モデル距離パターン　出荷場所

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
    Public Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIM0001tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0001tbl) Then
                OIM0001tbl.Clear()
                OIM0001tbl.Dispose()
                OIM0001tbl = Nothing
            End If

            If Not IsNothing(OIM0001INPtbl) Then
                OIM0001INPtbl.Clear()
                OIM0001INPtbl.Dispose()
                OIM0001INPtbl = Nothing
            End If

            If Not IsNothing(OIM0001UPDtbl) Then
                OIM0001UPDtbl.Clear()
                OIM0001UPDtbl.Dispose()
                OIM0001UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0001WRKINC.MAPIDC
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

        '○ GridView初期設定
        '        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0001L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        'CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        'CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_SELUORG_TEXT.Text, WW_DUMMY)                     '運用部署

        '会社コード、運用部署、会社コード2・組織コード2・削除フラグを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtCampCodeMy.Attributes("onkeyPress") = "CheckNum()"
        'Me.TxtOrgCodeMy.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtCampCode.Attributes("onkeyPress") = "CheckNum()"
        'Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"
        '開始年月日・終了年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtStymd.Attributes("onkeyPress") = "CheckCalendar()"
        Me.TxtEndymd.Attributes("onkeyPress") = "CheckCalendar()"

        ''会社コード
        'TxtCampCodeMy.Text = work.WF_SEL_CAMPCODE.Text

        ''運用部署
        'TxtOrgCodeMy.Text = work.WF_SEL_ORGCODE.Text

        '選択行
        ''WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        ''会社コード2
        ''2020/06/16杉山修正
        ''TxtCampCode.Text = work.WF_SEL_CAMPCODE2.Text
        'TxtCampCode.Text = work.WF_SEL_CAMPCODE_L.Text
        'CODENAME_get("CAMPCODE", TxtCampCode.Text, Label2.Text, WW_RTN_SW)

        '組織コード2
        '2020/06/16杉山修正
        'TxtOrgCode.Text = work.WF_SEL_ORGCODE2.Text
        'TxtOrgCode.Text = work.WF_SEL_ORGCODE_L.Text
        ''CODENAME_get("ORGCODE", TxtOrgCode.Text, Label3.Text, WW_DUMMY)

        ''組織名称
        'TxtOrgName.Text = work.WF_SEL_ORGNAME.Text

        ''組織名称（短）
        'TxtOrgNameS.Text = work.WF_SEL_ORGNAMES.Text

        ''組織名称カナ
        'TxtOrgNameKana.Text = work.WF_SEL_ORGNAMEKANA.Text

        ''組織名称カナ（短）
        ''TxtOrgNameKanaS.Text = work.WF_SEL_ORGNAMEKANAS.Text

        ''開始年月日
        'TxtStymd.Text = work.WF_SEL_STYMD.Text

        ''終了年月日
        'TxtEndymd.Text = work.WF_SEL_ENDYMD.Text

        ''削除
        'TxtDelFlg.Text = work.WF_SEL_SELECT.Text
        'CODENAME_get("DELFLG", TxtDelFlg.Text, Label1.Text, WW_DUMMY)

        '2020/10/26 廣田修正
        '会社コード
        TxtCampCodeMy.Text = work.WF_SEL_CAMPCODE.Text

        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '開始年月日
        TxtStymd.Text = work.WF_SEL_STYMD2.Text

        '終了年月日
        TxtEndymd.Text = work.WF_SEL_ENDYMD.Text

        '会社名称
        TxtName.Text = work.WF_SEL_NAME.Text

        '会社名称（短）
        TxtNames.Text = work.WF_SEL_NAMES.Text

        '会社カナ名称
        TxtNameKana.Text = work.WF_SEL_NAMEKANA.Text

        '会社カナ名称（短）
        TxtNameKanas.Text = work.WF_SEL_NAMEKANAS.Text

        '郵便番号（上）
        TxtPostName1.Text = work.WF_SEL_POSTNUM1.Text

        '郵便番号（下）
        TxtPostName2.Text = work.WF_SEL_POSTNUM2.Text

        '住所１
        TxtAddr1.Text = work.WF_SEL_ADDR1.Text

        '住所２
        TxtAddr2.Text = work.WF_SEL_ADDR2.Text

        '住所３
        TxtAddr3.Text = work.WF_SEL_ADDR3.Text

        '住所４
        TxtAddr4.Text = work.WF_SEL_ADDR4.Text

        '電話番号
        TxtTel.Text = work.WF_SEL_TEL.Text

        'FAX番号
        TxtFax.Text = work.WF_SEL_FAX.Text

        'メールアドレス
        TxtMail.Text = work.WF_SEL_MAIL.Text

        '削除フラグ
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgText.Text, WW_DUMMY)



    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT" _
            & "    CAMPCODE" _
            & " FROM" _
            & "    OIL.OIM0001_CAMP" _
            & " WHERE" _
            & "        CAMPCODE      = @P1" _
            & "    AND DELFLG           <> @P2"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '組織コード
        If Not String.IsNullOrEmpty(TxtCampCode.Text) Then
            SQLStr &= String.Format("    AND CAMPCODE = '{0}'", TxtCampCode.Text)
        Else
            SQLStr &= String.Format("    AND CAMPCODE = '{0}'", "")
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 2)            '会社コード
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", SqlDbType.NVarChar, 1)            '削除フラグ

                PARA1.Value = TxtCampCode.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0001Chk.Load(SQLdr)

                    If OIM0001Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0001C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0001C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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
        DetailBoxToOIM0001INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0001tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0001tbl, work.WF_SEL_INPTBL.Text)

        ''○ 詳細画面初期化
        'If isNormal(WW_ERR_SW) Then
        '    DetailBoxClear()
        'End If

        '○ メッセージ表示
        If WW_ERR_SW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ERR_SW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)

            ElseIf WW_ERR_SW = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ERR_SW, C_MESSAGE_TYPE.ERR, "会社コード", needsPopUp:=True)

            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)

            End If
        End If

        '○画面切替設定
        'WF_BOXChange.Value = "headerbox"

        '############# おためし #############
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
    Protected Sub DetailBoxToOIM0001INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)            '削除

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(TxtDelFlg.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail")

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(OIM0001INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0001INProw As DataRow = OIM0001INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0001INPcol As DataColumn In OIM0001INPtbl.Columns
            If IsDBNull(OIM0001INProw.Item(OIM0001INPcol)) OrElse IsNothing(OIM0001INProw.Item(OIM0001INPcol)) Then
                Select Case OIM0001INPcol.ColumnName
                    Case "LINECNT"
                        OIM0001INProw.Item(OIM0001INPcol) = 0
                    Case "OPERATION"
                        OIM0001INProw.Item(OIM0001INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        OIM0001INProw.Item(OIM0001INPcol) = 0
                    Case "SELECT"
                        OIM0001INProw.Item(OIM0001INPcol) = 1
                    Case "HIDDEN"
                        OIM0001INProw.Item(OIM0001INPcol) = 0
                    Case Else
                        OIM0001INProw.Item(OIM0001INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0001INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0001INProw("LINECNT"))
            Catch ex As Exception
                OIM0001INProw("LINECNT") = 0
            End Try
        End If

        OIM0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0001INProw("TIMSTP") = 0
        OIM0001INProw("SELECT") = 1
        OIM0001INProw("HIDDEN") = 0

        'OIM0001INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text           '会社コード
        'OIM0001INProw("UORG") = work.WF_SEL_ORGCODE.Text                '運用部署

        OIM0001INProw("DELFLG") = Me.TxtDelFlg.Text                      '削除フラグ
        OIM0001INProw("CAMPCODE") = Me.TxtCampCode.Text                  '会社コード
        OIM0001INProw("STYMD") = Me.TxtStymd.Text                        '開始年月日
        OIM0001INProw("ENDYMD") = Me.TxtEndymd.Text                      '終了年月日
        OIM0001INProw("NAME") = Me.TxtName.Text                          '会社名称
        OIM0001INProw("NAMES") = Me.TxtNames.Text                        '会社名称（短）
        OIM0001INProw("NAMEKANA") = Me.TxtNameKana.Text                  '会社カナ名称
        OIM0001INProw("NAMEKANAS") = Me.TxtNameKanas.Text                '会社カナ名称（短）
        OIM0001INProw("POSTNUM1") = Me.TxtPostName1.Text                 '郵便番号（上）
        OIM0001INProw("POSTNUM2") = Me.TxtPostName2.Text                 '郵便番号（上）
        OIM0001INProw("ADDR1") = Me.TxtAddr1.Text                        '住所１
        OIM0001INProw("ADDR2") = Me.TxtAddr2.Text                        '住所２
        OIM0001INProw("ADDR3") = Me.TxtAddr3.Text                        '住所３
        OIM0001INProw("ADDR4") = Me.TxtAddr4.Text                        '住所４
        OIM0001INProw("TEL") = Me.TxtTel.Text                            '電話番号
        OIM0001INProw("FAX") = Me.TxtFax.Text                            'FAX番号
        OIM0001INProw("MAIL") = Me.TxtMail.Text                          'メールアドレス

        '○ チェック用テーブルに登録する
        OIM0001INPtbl.Rows.Add(OIM0001INProw)

    End Sub


    ''' <summary>
    ''' 詳細画面-クリアボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WF_CLEAR_Click()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        ''○画面切替設定
        'WF_BOXChange.Value = "headerbox"

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
    Public Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0001row As DataRow In OIM0001tbl.Rows
            Select Case OIM0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0001tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT
        'TxtCampCode.Text = ""               '会社コード
        'TxtOrgCode.Text = ""                '組織コード
        'TxtOrgName.Text = ""                '組織名称
        'TxtOrgNameS.Text = ""               '組織名称（短）
        'TxtOrgNameKana.Text = ""            '組織名称カナ
        'TxtOrgNameKanaS.Text = ""           '組織名称カナ（短）
        'TxtDelFlg.Text = ""                 '削除
        'Label1.Text = ""                    '削除名称

        TxtDelFlg.Text = ""                 '削除フラグ
        TxtCampCode.Text = ""               '会社コード
        TxtStymd.Text = ""                  '開始年月日
        TxtEndymd.Text = ""                 '終了年月日
        TxtName.Text = ""                   '会社名称
        TxtNames.Text = ""                  '会社名称（短）
        TxtNameKana.Text = ""               '会社カナ名称
        TxtNameKanas.Text = ""              '会社カナ名称（短）
        TxtPostName1.Text = ""              '郵便番号（上）
        TxtPostName2.Text = ""              '郵便番号（下）
        TxtAddr1.Text = ""                  '住所１
        TxtAddr2.Text = ""                  '住所２
        TxtAddr3.Text = ""                  '住所３
        TxtAddr4.Text = ""                  '住所４
        TxtTel.Text = ""                    '電話番号
        TxtFax.Text = ""                    'FAX番号
        TxtMail.Text = ""                   'メールアドレス

    End Sub


    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WF_FIELD_DBClick()

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
                            Case "WF_STYMD"         '有効年月日(From)
                                .WF_Calendar.Text = TxtStymd.Text
                            Case "WF_ENDYMD"        '有効年月日(To)
                                .WF_Calendar.Text = TxtEndymd.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        Dim prmData As New Hashtable

                        'フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "WF_CAMPCODE"       '会社コード
                                'If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                                'Else
                                '    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ROLE
                                'End If
                                prmData.Item(C_PARAMETERS.LP_COMPANY) = TxtCampCode.Text

                            Case "WF_ORGCODE"       '組織コード
                                Dim AUTHORITYALL_FLG As String = "0"
                                'If Master.USER_ORG = CONST_ORGCODE_INFOSYS Or CONST_ORGCODE_OIL Then   '情報システムか石油部の場合
                                If TxtCampCode.Text = "" Then '会社コードが空の場合
                                    AUTHORITYALL_FLG = "1"
                                Else '会社コードに入力済みの場合
                                    AUTHORITYALL_FLG = "2"
                                End If
                                'End If
                                prmData = work.CreateORGParam(TxtCampCode.Text, AUTHORITYALL_FLG)
                                'prmData = work.CreateORGParam2(TxtCampCode.Text)
                            'Case "WF_MENUROLE"       'メニュー表示制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "MENU")
                            'Case "WF_MAPROLE"       '画面参照更新制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "MAP")
                            'Case "WF_VIEWPROFID"       '画面表示項目制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "VIEW")
                            'Case "WF_RPRTPROFID"       'エクセル出力制御ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "XML")
                            'Case "WF_APPROVALID"       '承認権限ロール
                            '    prmData = work.CreateRoleList(WF_CAMPCODE.Text, "APPROVAL")
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
    Public Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            ''会社コード
            Case "WF_CAMPCODE"
                CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCode.Text, WW_RTN_SW)
            ''組織コード
            'Case "WF_UORG"
            '    CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
            '削除フラグ
            Case "TxtDelFlg"
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlg.Text, WW_RTN_SW)

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            If WF_FIELD.Value = "TxtDelFlg" Then
                Master.Output(C_MESSAGE_NO.OIL_DELFLG_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.ERR)
            End If
        End If
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WF_ButtonSel_Click()

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
                '削除
                Case "WF_DELFLG"
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgText.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "WF_STYMD"             '開始年月日(From)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            TxtStymd.Text = ""
                        Else
                            TxtStymd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtStymd.Focus()

                Case "WF_ENDYMD"            '終了年月日(To)
                    Dim WW_DATE As Date
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_DATE)
                        If WW_DATE < C_DEFAULT_YMD Then
                            TxtEndymd.Text = ""
                        Else
                            TxtEndymd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtEndymd.Focus()

                Case "WF_CAMPCODE"               '会社コード
                    TxtCampCode.Text = WW_SelectValue
                    LblCampCodetext.Text = WW_SelectText
                    TxtCampCode.Focus()


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
    Public Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                '削除
                Case "WF_DELFLG"
                    TxtDelFlg.Focus()
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
    Public Sub WF_RadioButton_Click()

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
    Public Sub WF_RIGHTBOX_Change()

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
    Public Sub INPTableCheck(ByRef O_RTN As String)
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
        For Each OIM0001INProw As DataRow In OIM0001INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0001INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0001INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '開始年月日(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STYMD", OIM0001INProw("STYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIM0001INProw("STYMD"), "開始年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0001INProw("STYMD") = CDate(OIM0001INProw("STYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(開始年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '終了年月日(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ENDYMD", OIM0001INProw("ENDYMD"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '年月日チェック
                WW_CheckDate(OIM0001INProw("ENDYMD"), "終了年月日", WW_CS0024FCHECKERR, dateErrFlag)
                If dateErrFlag = "1" Then
                    WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                    WW_CheckMES2 = C_MESSAGE_NO.PREREQUISITE_ERROR
                    O_RTN = "ERR"
                    Exit Sub
                Else
                    OIM0001INProw("ENDYMD") = CDate(OIM0001INProw("ENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(終了年月日エラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '会社コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "CAMPCODE", OIM0001INProw("CAMPCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "会社コード入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '組織コード(バリデーションチェック)
            '組織コードが設定されている場合のみチェック
            If Not String.IsNullOrEmpty(OIM0001INProw("ORGCODE")) Then
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "ORGCODE", OIM0001INProw("ORGCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "組織コード入力エラー。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            '2020/06/16杉山修正
            If OIM0001INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text Then

            Else
                Using SQLcon As SqlConnection = CS0050SESSION.getConnection
                    'DataBase接続
                    SQLcon.Open()

                    '一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_UniqueKeyCHECK)
                End Using

                If Not isNormal(WW_UniqueKeyCHECK) Then
                    WW_CheckMES1 = "一意制約違反。"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                   "([" & OIM0001INProw("CAMPCODE") & "]" &
                                   " [" & OIM0001INProw("ORGCODE") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0001INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OIL_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    Public Sub WW_CheckDate(ByVal I_DATE As String, ByVal I_DATENAME As String, ByVal I_VALUE As String, ByRef dateErrFlag As String)

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
    ''' <param name="OIM0001row"></param>
    ''' <remarks></remarks>
    Public Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0001row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0001row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 会社コード         =" & OIM0001row("CAMPCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織コード         =" & OIM0001row("ORGCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 開始年月日         =" & OIM0001row("STYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 終了年月日         =" & OIM0001row("ENDYMD") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称           =" & OIM0001row("NAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称（短）     =" & OIM0001row("NAMES") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称カナ       =" & OIM0001row("NAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 組織名称カナ（短） =" & OIM0001row("NAMEKANAS") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0001row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0001tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub OIM0001tbl_UPD()

        '○ 画面状態設定
        For Each OIM0001row As DataRow In OIM0001tbl.Rows
            Select Case OIM0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0001INProw As DataRow In OIM0001INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0001INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0001row As DataRow In OIM0001tbl.Rows
                If OIM0001row("CAMPCODE") = OIM0001INProw("CAMPCODE") AndAlso
                    OIM0001row("ORGCODE") = OIM0001INProw("ORGCODE") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0001row("DELFLG") = OIM0001INProw("DELFLG") AndAlso
                        OIM0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0001INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0001INProw As DataRow In OIM0001INPtbl.Rows
            Select Case OIM0001INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0001INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0001INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0001INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0001INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0001INProw"></param>
    ''' <remarks></remarks>
    Public Sub TBL_UPDATE_SUB(ByRef OIM0001INProw As DataRow)

        For Each OIM0001row As DataRow In OIM0001tbl.Rows

            '同一レコードか判定
            If OIM0001INProw("CAMPCODE") = OIM0001row("CAMPCODE") AndAlso
                OIM0001INProw("ORGCODE") = OIM0001row("ORGCODE") Then
                '画面入力テーブル項目設定
                OIM0001INProw("LINECNT") = OIM0001row("LINECNT")
                OIM0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0001INProw("TIMSTP") = OIM0001row("TIMSTP")
                OIM0001INProw("SELECT") = 1
                OIM0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0001row.ItemArray = OIM0001INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0001INProw"></param>
    ''' <remarks></remarks>
    Public Sub TBL_INSERT_SUB(ByRef OIM0001INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0001row As DataRow = OIM0001tbl.NewRow
        OIM0001row.ItemArray = OIM0001INProw.ItemArray

        OIM0001row("LINECNT") = OIM0001tbl.Rows.Count + 1
        If OIM0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            '            OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            OIM0001row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0001row("TIMSTP") = "0"
        OIM0001row("SELECT") = 1
        OIM0001row("HIDDEN") = 0

        OIM0001tbl.Rows.Add(OIM0001row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0001INProw"></param>
    ''' <remarks></remarks>
    Public Sub TBL_ERR_SUB(ByRef OIM0001INProw As DataRow)

        For Each OIM0001row As DataRow In OIM0001tbl.Rows

            '同一レコードか判定
            If OIM0001INProw("CAMPCODE") = OIM0001row("CAMPCODE") AndAlso
               OIM0001INProw("ORGCODE") = OIM0001row("ORGCODE") Then
                '画面入力テーブル項目設定
                OIM0001INProw("LINECNT") = OIM0001row("LINECNT")
                OIM0001INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0001INProw("TIMSTP") = OIM0001row("TIMSTP")
                OIM0001INProw("SELECT") = 1
                OIM0001INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0001row.ItemArray = OIM0001INProw.ItemArray
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
    Public Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

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
                    prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0001CompList.LC_COMPANY_TYPE.ALL
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

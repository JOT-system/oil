''************************************************************
' 貨物駅マスタメンテ登録画面
' 作成日 2019/10/29
' 更新日 2019/10/29
' 作成者 JOT森川
' 更新車 JOT森川
'
' 修正履歴:新規作成
'         :
''************************************************************
Imports System.Data.SqlClient
Imports JOTWEB.GRIS0005LeftBox

''' <summary>
''' 貨物駅マスタ登録（登録）
''' </summary>
''' <remarks></remarks>
Public Class OIM0004StationCreate
    Inherits Page

    '○ 検索結果格納Table
    Private OIM0004tbl As DataTable                                 '一覧格納用テーブル
    Private OIM0004INPtbl As DataTable                              'チェック用テーブル
    Private OIM0004UPDtbl As DataTable                              '更新用テーブル

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
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(OIM0004tbl) Then
                OIM0004tbl.Clear()
                OIM0004tbl.Dispose()
                OIM0004tbl = Nothing
            End If

            If Not IsNothing(OIM0004INPtbl) Then
                OIM0004INPtbl.Clear()
                OIM0004INPtbl.Dispose()
                OIM0004INPtbl = Nothing
            End If

            If Not IsNothing(OIM0004UPDtbl) Then
                OIM0004UPDtbl.Clear()
                OIM0004UPDtbl.Dispose()
                OIM0004UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = OIM0004WRKINC.MAPIDC
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        '        Master.CreateXMLSaveFile()

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
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.OIM0004L Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        'CODENAME_get("CAMPCODE", work.WF_SEL_CAMPCODE.Text, WF_SEL_CAMPNAME.Text, WW_DUMMY)             '会社コード
        'CODENAME_get("UORG", work.WF_SEL_UORG.Text, WF_SELUORG_TEXT.Text, WW_DUMMY)                     '運用部署

        '選択行
        WF_Sel_LINECNT.Text = work.WF_SEL_LINECNT.Text

        '貨物車コード
        TxtStationCode.Text = work.WF_SEL_STATIONCODE2.Text

        '貨物コード枝番
        TxtBranch.Text = work.WF_SEL_BRANCH2.Text

        '貨物駅名称
        TxtStationName.Text = work.WF_SEL_STATONNAME.Text

        '貨物駅名称カナ
        TxtStationNameKana.Text = work.WF_SEL_STATIONNAMEKANA.Text

        '貨物駅種別名称
        TxtTypeName.Text = work.WF_SEL_TYPENAME.Text

        '貨物駅種別名称カナ
        TxtTypeNameKana.Text = work.WF_SEL_TYPENAMEKANA.Text

        '削除
        WF_DELFLG.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_DUMMY)

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As SqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT" _
            & "    STATIONCODE" _
            & "    , BRANCH" _
            & " FROM" _
            & "    OIL.OIM0004_STATION" _
            & " WHERE" _
            & "        STATIONCODE      = @P1"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '貨物コード枝番
        If Not String.IsNullOrEmpty(TxtBranch.Text) Then
            SQLStr &= String.Format("    AND BRANCH = '{0}'", TxtBranch.Text)
        Else
            SQLStr &= String.Format("    AND BRANCH = '{0}'", "")
        End If

        Try
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", SqlDbType.NVarChar, 4)            '貨物駅コード
                PARA1.Value = TxtStationCode.Text

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                    Dim OIM0004Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        OIM0004Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    OIM0004Chk.Load(SQLdr)

                    If OIM0004Chk.Rows.Count > 0 Then
                        '重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.OVERLAP_DATA_ERROR
                    Else
                        '正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "OIM0004C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:OIM0004C UPDATE_INSERT"
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
        DetailBoxToOIM0004INPtbl(WW_ERR_SW)
        If Not isNormal(WW_ERR_SW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ERR_SW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ERR_SW) Then
            OIM0004tbl_UPD()
        End If

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

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
            Else
                Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.ERR)
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
    Protected Sub DetailBoxToOIM0004INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(WF_DELFLG.Text)            '削除

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(WF_Sel_LINECNT.Text) AndAlso
            String.IsNullOrEmpty(WF_DELFLG.Text) Then
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

        Master.CreateEmptyTable(OIM0004INPtbl, work.WF_SEL_INPTBL.Text)
        Dim OIM0004INProw As DataRow = OIM0004INPtbl.NewRow

        '○ 初期クリア
        For Each OIM0004INPcol As DataColumn In OIM0004INPtbl.Columns
            If IsDBNull(OIM0004INProw.Item(OIM0004INPcol)) OrElse IsNothing(OIM0004INProw.Item(OIM0004INPcol)) Then
                Select Case OIM0004INPcol.ColumnName
                    Case "LINECNT"
                        OIM0004INProw.Item(OIM0004INPcol) = 0
                    Case "OPERATION"
                        OIM0004INProw.Item(OIM0004INPcol) = C_LIST_OPERATION_CODE.NODATA
                    Case "TIMSTP"
                        OIM0004INProw.Item(OIM0004INPcol) = 0
                    Case "SELECT"
                        OIM0004INProw.Item(OIM0004INPcol) = 1
                    Case "HIDDEN"
                        OIM0004INProw.Item(OIM0004INPcol) = 0
                    Case Else
                        OIM0004INProw.Item(OIM0004INPcol) = ""
                End Select
            End If
        Next

        'LINECNT
        If WF_Sel_LINECNT.Text = "" Then
            OIM0004INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(WF_Sel_LINECNT.Text, OIM0004INProw("LINECNT"))
            Catch ex As Exception
                OIM0004INProw("LINECNT") = 0
            End Try
        End If

        OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        OIM0004INProw("TIMSTP") = 0
        OIM0004INProw("SELECT") = 1
        OIM0004INProw("HIDDEN") = 0

        'OIM0004INProw("CAMPCODE") = work.WF_SEL_CAMPCODE.Text        '会社コード
        'OIM0004INProw("UORG") = work.WF_SEL_UORG.Text                '運用部署

        OIM0004INProw("DELFLG") = WF_DELFLG.Text                     '削除

        OIM0004INProw("STATIONCODE") = TxtStationCode.Text           '貨物駅コード
        OIM0004INProw("BRANCH") = TxtBranch.Text                     '貨物コード枝番
        OIM0004INProw("STATONNAME") = TxtStationName.Text            '貨物駅名称
        OIM0004INProw("STATIONNAMEKANA") = TxtStationNameKana.Text   '貨物駅名称カナ
        OIM0004INProw("TypeName") = TxtTypeName.Text                 '貨物駅種別名称
        OIM0004INProw("TYPENAMEKANA") = TxtTypeNameKana.Text         '貨物駅種別名称カナ

        '○ 名称取得
        'CODENAME_get("TORICODES", OIM0004INProw("TORICODES"), OIM0004INProw("TORINAMES"), WW_DUMMY)           '取引先名称(出荷先)
        'CODENAME_get("SHUKABASHO", OIM0004INProw("SHUKABASHO"), OIM0004INProw("SHUKABASHONAMES"), WW_DUMMY)   '出荷場所名称

        'CODENAME_get("TORICODET", OIM0004INProw("TORICODET"), OIM0004INProw("TORINAMET"), WW_DUMMY)           '取引先名称(届先)
        'CODENAME_get("TODOKECODE", OIM0004INProw("TODOKECODE"), OIM0004INProw("TODOKENAME"), WW_DUMMY)        '届先名称

        '○ チェック用テーブルに登録する
        OIM0004INPtbl.Rows.Add(OIM0004INProw)

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
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ERR_SW = C_LIST_OPERATION_CODE.NODATA

                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ERR_SW = C_MESSAGE_NO.NORMAL

                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ERR_SW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(OIM0004tbl, work.WF_SEL_INPTBL.Text)

        WF_Sel_LINECNT.Text = ""            'LINECNT
        TxtStationCode.Text = ""            '貨物駅コード
        TxtBranch.Text = ""                 '貨物コード枝番
        TxtStationName.Text = ""            '貨物駅名称
        TxtStationNameKana.Text = ""        '貨物駅名称カナ
        TxtTypeName.Text = ""               '貨物駅種別名称
        TxtTypeNameKana.Text = ""           '貨物駅種別名称カナ
        WF_DELFLG.Text = ""                 '削除
        WF_DELFLG_TEXT.Text = ""            '削除名称

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
                '会社コード
                Dim prmData As New Hashtable

                'フィールドによってパラメーターを変える
                Select Case WW_FIELD
                    ''貨物車コード 
                    'Case "STATIONCODE"
                    '    prmData = work.CreateSTATIONPTParam(work.WF_SEL_CAMPCODE.Text, TxtStationCode.Text & TxtBranch.Text)

                    '削除フラグ   
                    Case "WF_DELFLG"
                        prmData.Item(C_PARAMETERS.LP_COMPANY) = work.WF_SEL_CAMPCODE.Text
                        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, prmData)
                .ActiveListBox()
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
            ''会社コード
            'Case "WF_CAMPCODE"
            '    CODENAME_get("CAMPCODE", WF_CAMPCODE.Text, WF_CAMPCODE_TEXT.Text, WW_RTN_SW)
            ''運用部署
            'Case "WF_UORG"
            '    CODENAME_get("UORG", WF_UORG.Text, WF_UORG_TEXT.Text, WW_RTN_SW)
            '削除フラグ
            Case "WF_DELFLG"
                CODENAME_get("DELFLG", WF_DELFLG.Text, WF_DELFLG_TEXT.Text, WW_RTN_SW)

        End Select

        '○ メッセージ表示
        If isNormal(WW_RTN_SW) Then
            Master.Output(WW_RTN_SW, C_MESSAGE_TYPE.NOR)
        Else
            If WF_FIELD.Value = "WF_DELFLG" Then
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
                '削除
                Case "WF_DELFLG"
                    WF_DELFLG.Text = WW_SelectValue
                    WF_DELFLG_TEXT.Text = WW_SelectText
                    WF_DELFLG.Focus()

                    '貨物駅コード
                Case "STATIONCODE"
                    TxtStationCode.Text = WW_SelectValue.Substring(0, 4)
                    LblStationCodeText.Text = WW_SelectText
                    TxtBranch.Text = WW_SelectValue.Substring(4)
                    TxtStationCode.Focus()

                    '貨物コード枝番
                Case "BRANCH"
                    TxtBranch.Text = WW_SelectValue
                    LblBranchText.Text = WW_SelectText
                    TxtBranch.Focus()

                    '貨物駅名称
                Case "STATONNAME"
                    TxtStationName.Text = WW_SelectValue
                    LblStationNameText.Text = WW_SelectText
                    TxtStationName.Focus()

                    '貨物駅名称カナ
                Case "STATIONNAMEKANA"
                    TxtStationNameKana.Text = WW_SelectValue
                    LblStationNameKanaText.Text = WW_SelectText
                    TxtStationNameKana.Focus()

                    '貨物駅種別名称
                Case "TYPENAME"
                    TxtTypeName.Text = WW_SelectValue
                    LblTypeNameText.Text = WW_SelectText
                    TxtTypeName.Focus()

                    '貨物駅種別名称カナ
                Case "TYPENAMEKANA"
                    TxtTypeNameKana.Text = WW_SelectValue
                    LblTypeNameKanaText.Text = WW_SelectText
                    TxtTypeNameKana.Focus()

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
                '削除
                Case "WF_DELFLG"
                    WF_DELFLG.Focus()

                    '貨物駅コード
                Case "STATIONCODE"
                    TxtStationCode.Focus()

                    '貨物コード枝番
                Case "BRANCH"
                    TxtBranch.Focus()

                    '貨物駅名称
                Case "STATONNAME"
                    TxtStationName.Focus()

                    '貨物駅名称カナ
                Case "STATIONNAMEKANA"
                    TxtStationNameKana.Focus()

                    '貨物駅種別名称
                Case "TYPENAME"
                    TxtTypeName.Focus()

                    '貨物駅種別名称カナ
                Case "TYPENAMEKANA"
                    TxtTypeNameKana.Focus()

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
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows

            WW_LINE_ERR = ""

            '削除フラグ(バリデーションチェック）
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "DELFLG", OIM0004INProw("DELFLG"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If isNormal(WW_CS0024FCHECKERR) Then
                '値存在チェック
                CODENAME_get("DELFLG", OIM0004INProw("DELFLG"), WW_DUMMY, WW_RTN_SW)
                If Not isNormal(WW_RTN_SW) Then
                    WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・更新できないレコード(削除コードエラー)です。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '貨物駅コード(バリデーションチェック)
            Master.CheckField(work.WF_SEL_CAMPCODE.Text, "STATIONCODE", OIM0004INProw("STATIONCODE"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
            If Not isNormal(WW_CS0024FCHECKERR) Then
                WW_CheckMES1 = "貨物駅コード入力エラー。"
                WW_CheckMES2 = WW_CS0024FCHECKREPORT
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                WW_LINE_ERR = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '貨物コード枝番(バリデーションチェック)
            '貨物コード枝番が設定されている場合のみチェック
            If Not String.IsNullOrEmpty(OIM0004INProw("BRANCH")) Then
                Master.CheckField(work.WF_SEL_CAMPCODE.Text, "BRANCH", OIM0004INProw("BRANCH"), WW_CS0024FCHECKERR, WW_CS0024FCHECKREPORT)
                If Not isNormal(WW_CS0024FCHECKERR) Then
                    WW_CheckMES1 = "貨物コード枝番入力エラー。"
                    WW_CheckMES2 = WW_CS0024FCHECKREPORT
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '一意制約チェック
            '同一レコードの更新の場合、チェック対象外
            If OIM0004INProw("STATIONCODE") = work.WF_SEL_STATIONCODE2.Text _
                AndAlso OIM0004INProw("BRANCH") = work.WF_SEL_BRANCH2.Text Then

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
                                   "([" & OIM0004INProw("STATIONCODE") & "]" &
                                   " [" & OIM0004INProw("BRANCH") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2, OIM0004INProw)
                    WW_LINE_ERR = "ERR"
                    O_RTN = C_MESSAGE_NO.OVERLAP_DATA_ERROR
                End If
            End If

            If WW_LINE_ERR = "" Then
                If OIM0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LINE_ERR = CONST_PATTERNERR Then
                    '関連チェックエラーをセット
                    OIM0004INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    '単項目チェックエラーをセット
                    OIM0004INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <param name="OIM0004row"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String, Optional ByVal OIM0004row As DataRow = Nothing)

        Dim WW_ERR_MES As String = ""
        WW_ERR_MES = MESSAGE1
        If MESSAGE2 <> "" Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> " & MESSAGE2 & " , "
        End If

        If Not IsNothing(OIM0004row) Then
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅コード       =" & OIM0004row("STATIONCODE") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物コード枝番     =" & OIM0004row("BRANCH") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称         =" & OIM0004row("STATONNAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅名称カナ     =" & OIM0004row("STATIONNAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称     =" & OIM0004row("TYPENAME") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 貨物駅種別名称カナ =" & OIM0004row("TYPENAMEKANA") & " , "
            WW_ERR_MES &= ControlChars.NewLine & "  --> 削除               =" & OIM0004row("DELFLG")
        End If

        rightview.AddErrorReport(WW_ERR_MES)

    End Sub


    ''' <summary>
    ''' OIM0004tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub OIM0004tbl_UPD()

        '○ 画面状態設定
        For Each OIM0004row As DataRow In OIM0004tbl.Rows
            Select Case OIM0004row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows

            'エラーレコード読み飛ばし
            If OIM0004INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            OIM0004INProw.Item("OPERATION") = CONST_INSERT

            'KEY項目が等しい時
            For Each OIM0004row As DataRow In OIM0004tbl.Rows
                If OIM0004row("STATIONCODE") = OIM0004INProw("STATIONCODE") AndAlso
                    OIM0004row("BRANCH") = OIM0004INProw("BRANCH") Then
                    'KEY項目以外の項目に変更がないときは「操作」の項目は空白にする
                    If OIM0004row("DELFLG") = OIM0004INProw("DELFLG") AndAlso
                        OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA Then
                    Else
                        'KEY項目以外の項目に変更がある時は「操作」の項目を「更新」に設定する
                        OIM0004INProw("OPERATION") = CONST_UPDATE
                        Exit For
                    End If

                    Exit For

                End If
            Next
        Next

        '○ 変更有無判定　&　入力値反映
        For Each OIM0004INProw As DataRow In OIM0004INPtbl.Rows
            Select Case OIM0004INProw("OPERATION")
                Case CONST_UPDATE
                    TBL_UPDATE_SUB(OIM0004INProw)
                Case CONST_INSERT
                    TBL_INSERT_SUB(OIM0004INProw)
                Case CONST_PATTERNERR
                    '関連チェックエラーの場合、キーが変わるため、行追加してエラーレコードを表示させる
                    TBL_INSERT_SUB(OIM0004INProw)
                Case C_LIST_OPERATION_CODE.ERRORED
                    TBL_ERR_SUB(OIM0004INProw)
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 更新予定データの一覧更新時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_UPDATE_SUB(ByRef OIM0004INProw As DataRow)

        For Each OIM0004row As DataRow In OIM0004tbl.Rows

            '同一レコードか判定
            If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
                OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                '画面入力テーブル項目設定
                OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                OIM0004INProw("SELECT") = 1
                OIM0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0004row.ItemArray = OIM0004INProw.ItemArray
                Exit For
            End If
        Next

    End Sub

    ''' <summary>
    ''' 追加予定データの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_INSERT_SUB(ByRef OIM0004INProw As DataRow)

        '○ 項目テーブル項目設定
        Dim OIM0004row As DataRow = OIM0004tbl.NewRow
        OIM0004row.ItemArray = OIM0004INProw.ItemArray

        OIM0004row("LINECNT") = OIM0004tbl.Rows.Count + 1
        If OIM0004INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.UPDATING Then
            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        Else
            '            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            OIM0004row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        End If

        OIM0004row("TIMSTP") = "0"
        OIM0004row("SELECT") = 1
        OIM0004row("HIDDEN") = 0

        OIM0004tbl.Rows.Add(OIM0004row)

    End Sub


    ''' <summary>
    ''' エラーデータの一覧登録時処理
    ''' </summary>
    ''' <param name="OIM0004INProw"></param>
    ''' <remarks></remarks>
    Protected Sub TBL_ERR_SUB(ByRef OIM0004INProw As DataRow)

        For Each OIM0004row As DataRow In OIM0004tbl.Rows

            '同一レコードか判定
            If OIM0004INProw("STATIONCODE") = OIM0004row("STATIONCODE") AndAlso
               OIM0004INProw("BRANCH") = OIM0004row("BRANCH") Then
                '画面入力テーブル項目設定
                OIM0004INProw("LINECNT") = OIM0004row("LINECNT")
                OIM0004INProw("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                OIM0004INProw("TIMSTP") = OIM0004row("TIMSTP")
                OIM0004INProw("SELECT") = 1
                OIM0004INProw("HIDDEN") = 0

                '項目テーブル項目設定
                OIM0004row.ItemArray = OIM0004INProw.ItemArray
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
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "UORG"             '運用部署
                    prmData = work.CreateUORGParam(work.WF_SEL_CAMPCODE.Text)
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, prmData)

                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(work.WF_SEL_CAMPCODE.Text, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class

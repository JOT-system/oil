Imports System.Drawing

Public Class GRIS0005LeftBox
    Inherits UserControl
    ''' <summary>
    ''' ソート機能
    ''' </summary>
    Public Property LF_SORTING_CODE As String
    ''' <summary>
    ''' フィルターの有無
    ''' </summary>
    Public Property LF_FILTER_CODE As String
    ''' <summary>
    ''' 再検索時の主要パラメータ（１つ）
    ''' </summary>
    Public Property LF_PARAM_DATA As String
    ''' <summary>
    ''' ソート機能の条件一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_SORTING_CODE
        ''' <summary>
        ''' ソート機能：なし
        ''' </summary>
        Public Const HIDE As String = "0"
        ''' <summary>
        ''' ソート機能：名称
        ''' </summary>
        Public Const NAME As String = "1"
        ''' <summary>
        ''' ソート機能：コード
        ''' </summary>
        Public Const CODE As String = "2"
        ''' <summary>
        ''' ソート機能：名称・コード
        ''' </summary>
        Public Const BOTH As String = "3"
    End Class
    ''' <summary>
    ''' フィルター機能の条件一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_FILTER_CODE
        ''' <summary>
        ''' フィルター機能：なし
        ''' </summary>
        Public Const DISABLE As String = "0"
        ''' <summary>
        ''' フィルター機能：あり
        ''' </summary>
        Public Const ENABLE As String = "1"
        ''' <summary>
        ''' フィルター機能：再検索
        ''' </summary>
        Public Const RESEACH As String = "2"
    End Class
    ''' <summary>
    ''' 左リストの作成情報一覧
    ''' </summary>
    ''' <list type="number">
    ''' <item><description>LC_COMPANY       : 会社のリストを作成</description></item>
    ''' <item><description>LC_CUSTOMER      : 顧客のリストを作成</description></item>
    ''' <item><description>LC_DISTINATION   : 届先のリストを作成</description></item>
    ''' <item><description>LC_ORG           : 部署のリストを作成</description></item>
    ''' <item><description>LC_STAFFCODE     : 社員のリストを作成</description></item>
    ''' <item><description>LC_GOODS         : 油種・品名のリストを作成</description></item>
    ''' <item><description>LC_CARCODE       : 統一車番のリストを作成</description></item>
    ''' <item><description>LC_WORKLORRY     : 業務車番のリストを作成(品名または固定値のFast)</description></item>
    ''' <item><description>LC_URIKBN 　　　 : 売上区分のリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_STAFFKBN      : 社員区分のリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_DELFLG        : 削除区分のリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_TERM          : 端末一覧のリストを作成</description></item>
    ''' <item><description>LC_ROLE          : 権限のリストを作成</description></item>
    ''' <item><description>LC_URL           : URLのリストを作成</description></item>
    ''' <item><description>LC_MODELPT       : モデル距離パターンのリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_EXTRA_LIST    : 指定されたリストを使用する</description></item>
    ''' <item><description>LC_CALENDAR      : カレンダー表示を行う</description></item>
    ''' <item><description>LC_FIX_VALUE     : 固定値区分のリストを作成</description></item>
    ''' <item><description>LC_STATIONCODE   : 貨物駅パターンのリストを作成</description></item>
    ''' </list>
    Public Enum LIST_BOX_CLASSIFICATION
        LC_COMPANY
        LC_CUSTOMER
        LC_DISTINATION
        LC_ORG
        LC_STAFFCODE
        LC_GOODS
        LC_CARCODE
        LC_WORKLORRY
        LC_OILTYPE
        LC_URIKBN
        LC_STAFFKBN
        LC_DELFLG
        LC_TERM
        LC_ROLE
        LC_URL
        LC_MODELPT
        LC_EXTRA_LIST
        LC_CALENDAR
        LC_FIX_VALUE
        LC_STATIONCODE
        LC_TANKNUMBER
        LC_TANKMODEL
        LC_SALESOFFICE
        LC_TRAINNUMBER
    End Enum
    ''' <summary>
    ''' パラメタ群
    ''' </summary>
    ''' <remarks>
    ''' <list type="number">
    ''' <item><description>LP_COMPANY       : 検索条件に会社コードを指定</description></item>
    ''' <item><description>LP_TYPEMODE      : 検索条件に各検索の条件区分値を指定</description></item>
    ''' <item><description>LP_PERMISSION    : 検索条件に権限を指定</description></item>
    ''' <item><description>LP_CUSTOMER      : 検索条件に取引先コードを指定</description></item>
    ''' <item><description>LP_CLASSCODE     : 検索条件に区分値を指定</description></item>
    ''' <item><description>LP_STAFF_KBN_LIST: 検索条件に社員区分一覧を指定</description></item>
    ''' <item><description>LP_ORG_COMP      : 検索条件に部署における会社コードを指定</description></item>
    ''' <item><description>LP_ORG           : 検索条件に部署コードを指定</description></item>
    ''' <item><description>LP_ORG_CATEGORYS : 検索条件に部署の区分け条件を指定</description></item>
    ''' <item><description>LP_OILTYPE       : 検索条件に油種コードを指定</description></item>
    ''' <item><description>LP_PRODCODE1     : 検索条件に品名１コードを指定</description></item>
    ''' <item><description>LP_FIX_CLASS     : 検索条件に固定値区分コードを指定</description></item>
    ''' <item><description>LP_LIST          : 画面表示させたい一覧を指定</description></item>
    ''' </list>
    ''' </remarks>
    Public Enum C_PARAMETERS
        LP_COMPANY
        LP_STYMD
        LP_ENDYMD
        LP_TYPEMODE
        LP_PERMISSION
        LP_CUSTOMER
        LP_CLASSCODE
        LP_STAFF_KBN_LIST
        LP_ORG_COMP
        LP_ORG
        LP_ORG_CATEGORYS
        LP_OILTYPE
        LP_PRODCODE1
        LP_FIX_CLASS
        LP_LIST
        LP_MODELPT
        LP_DEFAULT_SORT
        LP_DISPLAY_FORMAT
        LP_ROLE
        LP_SELECTED_CODE
        LP_STATIONCODE
        LP_TANKNUMBER
        LP_TANKMODEL
        LP_SALESOFFICE
        LP_TRAINNUMBER
    End Enum

    ''' <summary>
    ''' 作成一覧情報の保持
    ''' </summary>
    Protected LbMap As New Hashtable

    Protected C_TABLE_SPLIT As String = "|"

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL
        If IsPostBack Then
            Select Case DirectCast(Page.Master.FindControl("contents1").FindControl("WF_ButtonClick"), HtmlInputText).Value
                Case "WF_Field_DBClick", "WF_LeftBoxSubmit"            'フィールドダブルクリック
                    ViewState("LF_FILTER_CODE") = LF_FILTER_CODE
                    ViewState("LF_SORTING_CODE") = LF_SORTING_CODE
                    ViewState("LF_PARAM_DATA") = LF_PARAM_DATA
                Case "WF_ListboxDBclick", "WF_ButtonCan", "WF_ButtonSel"
                    '〇初期化
                    ViewState("LF_FILTER_CODE") = Nothing
                    ViewState("LF_SORTING_CODE") = Nothing
                    ViewState("LF_PARAM_DATA") = Nothing
                    ViewState("LF_LIST_SELECT") = Nothing
                    ViewState("LF_PARAMS") = Nothing
                Case Else
                    restore(O_RTN)
                    '〇取得
                    LF_FILTER_CODE = If(ViewState("LF_FILTER_CODE"), "0")
                    LF_SORTING_CODE = If(ViewState("LF_SORTING_CODE"), "0")
                    LF_PARAM_DATA = If(ViewState("LF_PARAM_DATA"), "0")
            End Select

        End If
    End Sub

    ''' <summary>
    ''' 左リストボックス設定処理
    ''' </summary>
    ''' <param name="ListCode">一覧を作成したい種別</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>会社    ：TYPEMODE, ROLE </para>
    ''' <para>取引先  ：TYPEMODE, COMPANY, ORGCODE, ROLE, PERMISSION</para>
    ''' <para>届先    ：TYPEMODE, COMPANY, ORGCODE, TORICODE, CLASSCODE, ROLE, PERMISSION</para>
    ''' <para>部署    ：TYPEMODE, COMPANY, CATEGORYS, ROLE, PERMISSION </para>
    ''' <para>社員    ：TYPEMODE, COMPANY, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANY, ORGCODE,  ROLE, PERMISSION</para>
    ''' <para>業務車番：COMPANY, ORGCODE, OILTYPE </para>
    ''' <para>品名    ：TYPEMODE, COMPANY, ORG_COMPANY, ORGCODE, OILTYPE, PRODUCT1, ROLE, PERMISSION</para>
    ''' <para>端末　　： </para>
    ''' <para>権限　　：TYPEMODE, COMPANY</para>
    ''' <para>ＵＲＬ　：TYPEMODE</para>
    ''' <para>拡張型　：LISTBOX</para>
    ''' <para>固定一覧：COMPANY, FIXVALUENAME</para>
    ''' </param>
    ''' <remarks>
    ''' <para>左リストボックスを作成する</para>
    ''' <para>ソート・フィルタの設定は一覧作成後に行う</para>
    ''' </remarks>
    Public Sub SetListBox(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        LF_SORTING_CODE = C_SORTING_CODE.BOTH
        LF_FILTER_CODE = C_FILTER_CODE.ENABLE
        ListToView(createListData(ListCode, O_RTN, Params))
        backup(ListCode, Params)
    End Sub
    ''' <summary>
    ''' 左リストボックス設定処理
    ''' </summary>
    ''' <param name="ListCode">一覧を作成したい種別</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>社員    ：TYPEMODE, COMPANYCODE, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANYCODE, ORGCODE</para>
    ''' </param>
    ''' <remarks>
    ''' <para>左リストボックスを作成する</para>
    ''' <para>ソート・フィルタの設定は一覧作成後に行う</para>
    ''' </remarks>
    Public Sub SetTableList(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        LF_SORTING_CODE = C_SORTING_CODE.HIDE
        LF_FILTER_CODE = C_FILTER_CODE.DISABLE
        createTableList(ListCode, O_RTN, Params)
        backup(ListCode, Params)

    End Sub
    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="ListCode">名称を取得したい種別</param>
    ''' <param name="I_VALUE">名称を取得したいコード値</param>
    ''' <param name="O_TEXT">取得した名称</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>会社    ：TYPEMODE, ROLE </para>
    ''' <para>取引先  ：TYPEMODE, COMPANY, ORGCODE, ROLE, PERMISSION</para>
    ''' <para>届先    ：TYPEMODE, COMPANY, ORGCODE, TORICODE, CLASSCODE, ROLE, PERMISSION</para>
    ''' <para>部署    ：TYPEMODE, COMPANY, CATEGORYS, ROLE, PERMISSION </para>
    ''' <para>社員    ：TYPEMODE, COMPANY, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANY, ORGCODE,  ROLE, PERMISSION</para>
    ''' <para>業務車番：COMPANY, ORGCODE, OILTYPE </para>
    ''' <para>品名    ：TYPEMODE, COMPANY, ORG_COMPANY, ORGCODE, OILTYPE, PRODUCT1, ROLE, PERMISSION</para>
    ''' <para>端末　　： </para>
    ''' <para>権限　　：TYPEMODE, COMPANY</para>
    ''' <para>ＵＲＬ　：TYPEMODE</para>
    ''' <para>拡張型　：LISTBOX</para>
    ''' <para>固定一覧：COMPANY, FIXVALUENAME</para>
    ''' </param>
    ''' <remarks></remarks>
    Public Sub CodeToName(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)

        O_TEXT = getListText(createListData(ListCode, O_RTN, Params), I_VALUE, O_RTN)
    End Sub
    ''' <summary>
    ''' 固定値マスタよりサブコードを取得する
    ''' </summary>
    ''' <param name="I_VALUE">名称を取得したいコード値</param>
    ''' <param name="O_TEXT">取得した名称</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params">一覧作成に必要なパラメータ群</param>
    ''' <param name="I_SUBCODE" >取得したいサブコード番号</param>
    ''' <remarks></remarks>
    Public Sub CodeToName(ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, ByVal Params As Hashtable, Optional ByVal I_SUBCODE As Integer = 2)

        O_TEXT = getListText(createSubCodeList(Params, O_RTN, I_SUBCODE), I_VALUE, O_RTN)
    End Sub
    ''' <summary>
    ''' テーブル表示時
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ActiveTable()
        WF_LEFTMView.ActiveViewIndex = 2
    End Sub
    ''' <summary>
    ''' カレンダー表示時
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ActiveCalendar()
        WF_LEFTMView.ActiveViewIndex = 1
        WF_Calendar.Focus()
    End Sub
    ''' <summary>
    ''' 一覧表示
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ActiveListBox()
        WF_LEFTMView.ActiveViewIndex = 0
        WF_LeftListBox.Focus()
    End Sub
    ''' <summary>
    ''' 左ボックスで指定した値を取得する
    ''' </summary>
    ''' <returns>
    ''' <para>LISTBOX：選択値、選択名称</para>
    ''' <para>CALENAR：選択日付(変換有)、選択日付(無変換)</para>
    ''' <para>TABLE  ：選択値群（選択項目＝選択値）</para>
    ''' </returns>
    ''' <remarks></remarks>
    Public Function GetActiveValue() As String()
        Select Case WF_LEFTMView.ActiveViewIndex
            Case 2
                Return WF_TBL_SELECT.Text.Split(C_TABLE_SPLIT)
            Case 1
                Dim Value As String() = {"", ""}
                Value(0) = WF_Calendar.Text
                Value(1) = WF_Calendar.Text
                If (Value(0) < C_DEFAULT_YMD) Then
                    Value(0) = C_DEFAULT_YMD
                End If
                Return Value
            Case 0
                Dim Value As String() = {"", ""}
                If WF_LeftListBox.SelectedIndex >= 0 Then
                    Value(0) = WF_LeftListBox.SelectedItem.Value
                    Value(1) = WF_LeftListBox.SelectedItem.Text
                End If
                Return Value
        End Select
        Return Nothing
    End Function
    ''' <summary>
    ''' 一覧情報を作成する
    ''' </summary>
    ''' <param name="ListCode">作成する一覧の内容</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>会社    ：TYPEMODE, ROLE </para>
    ''' <para>取引先  ：TYPEMODE, COMPANY, ORGCODE, ROLE, PERMISSION</para>
    ''' <para>届先    ：TYPEMODE, COMPANY, ORGCODE, TORICODE, CLASSCODE, ROLE, PERMISSION</para>
    ''' <para>部署    ：TYPEMODE, COMPANY, CATEGORYS, ROLE, PERMISSION </para>
    ''' <para>社員    ：TYPEMODE, COMPANY, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANY, ORGCODE,  ROLE, PERMISSION</para>
    ''' <para>業務車番：COMPANY, ORGCODE, OILTYPE </para>
    ''' <para>品名    ：TYPEMODE, COMPANY, ORG_COMPANY, ORGCODE, OILTYPE, PRODUCT1, ROLE, PERMISSION</para>
    ''' <para>端末　　： </para>
    ''' <para>権限　　：TYPEMODE, COMPANY</para>
    ''' <para>ＵＲＬ　：TYPEMODE</para>
    ''' <para>拡張型　：LISTBOX</para>
    ''' <para>固定一覧：COMPANY, FIXVALUENAME</para>
    ''' </param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateListData(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        If IsNothing(Params) Then
            Params = New Hashtable
        End If
        Dim lbox As ListBox
        Select Case ListCode
            Case LIST_BOX_CLASSIFICATION.LC_COMPANY
                '会社一覧設定
                lbox = createCompList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
            '    '取引先
            '    lbox = createCustomerList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_DISTINATION
            '    '届先
            '    lbox = createDistinationList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ORG
                '部署
                lbox = createOrg(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
            '    '社員
            '    lbox = createStaff(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_CARCODE
            '    '車両
            '    lbox = createCarCode(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_WORKLORRY
            '    '業務車番
            '    lbox = createWorkLorry(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_GOODS
            '    '品名
            '    lbox = createGoods(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_STAFFKBN
            '    '社員区分
            '    Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "STAFFKBN"
            '    lbox = createFixValueList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_URIKBN
            '    '売上計上区分
            '    Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "URIKBN"
            '    lbox = createFixValueList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_OILTYPE
            '    '油種
            '    Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "OILTYPE"
            '    lbox = createFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_DELFLG
                '削除区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "DELFLG"
                lbox = createFixValueList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_TERM
            '    '端末
            '    lbox = createTermList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ROLE
                '権限コード
                lbox = CreateRoleList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_URL
            '    'URL
            '    lbox = createURLList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_EXTRA_LIST
            '    '拡張リスト
            '    lbox = createExtra(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_MODELPT
            '    'モデル距離パターン
            '    Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "MODELPATTERN"
            '    lbox = createFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_STATIONCODE
                '貨物駅パターン
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "STATIONPATTERN"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_SALESOFFICE
                '営業所(組織コード)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SALESOFFICE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER
                '本線列車番号
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TRAINNO"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                'カレンダー
                lbox = Nothing
            Case Else
                lbox = createFixValueList(Params, O_RTN)
        End Select
        Return lbox
    End Function
    ''' <summary>
    ''' 一覧情報を作成する
    ''' </summary>
    ''' <param name="ListCode">作成する一覧の内容</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>社員    ：TYPEMODE, COMPANYCODE, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANYCODE, ORGCODE</para>
    ''' </param>
    ''' <remarks></remarks>
    Protected Sub CreateTableList(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        Select Case ListCode
            'Case LIST_BOX_CLASSIFICATION.LC_STAFFCODE
            '    '社員
            '    Using GL0005StaffList As New GL0005StaffList With {
            '       .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0005StaffList.LC_STAFF_TYPE.ALL) _
            '     , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
            '     , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
            '     , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
            '     , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "") _
            '     , .STAFFKBN = If(Params.Item(C_PARAMETERS.LP_STAFF_KBN_LIST), Nothing) _
            '     , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG) _
            '     , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE) _
            '     , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), GL0005StaffList.C_DEFAULT_SORT.SEQ) _
            '     , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0005StaffList.C_VIEW_FORMAT_PATTERN.NAMES) _
            '     , .STAFFCODE = If(Params.Item(C_PARAMETERS.LP_SELECTED_CODE), String.Empty) _
            '     , .AREA = pnlLeftList
            '    }
            '        GL0005StaffList.getTable()
            '        O_RTN = GL0005StaffList.ERR
            '    End Using
            'Case LIST_BOX_CLASSIFICATION.LC_CARCODE
            '    '車両
            '    Using GL0007CarList As New GL0007CarList With {
            '       .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0007CarList.LC_LORRY_TYPE.ALL) _
            '     , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
            '     , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
            '     , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
            '     , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "") _
            '     , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
            '     , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0007CarList.C_VIEW_FORMAT_PATTERN.NAMES) _
            '     , .AREA = pnlLeftList
            '    }
            '        GL0007CarList.getTable()
            '        O_RTN = GL0007CarList.ERR
            '    End Using
            Case Else
                Exit Sub
        End Select
    End Sub
    ''' <summary>
    ''' 会社コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateCompList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0001CompList.LC_COMPANY_TYPE.ROLE) _
                          & If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES) _
                          & LIST_BOX_CLASSIFICATION.LC_COMPANY

        If Not LbMap.ContainsKey(key) Then
            '○会社コードListBox設定
            Using CL0001CompList As New GL0001CompList With {
                   .TYPEMODE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0001CompList.LC_COMPANY_TYPE.ROLE) _
                 , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
                 , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
                 , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_MAP) _
                 , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
                 , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES)
            }
                CL0001CompList.getList()
                Dim lsbx As ListBox = CL0001CompList.LIST
                O_RTN = CL0001CompList.ERR
                LbMap.Add(key, lsbx)
            End Using
        End If
        Return LbMap.Item(key)
    End Function

    '''' <summary>
    '''' 取引先一覧の作成
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateCustomerList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    '○取引先ListBox設定
    '    Dim KeyCode As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0003CustomerList.LC_CUSTOMER_TYPE.ALL) _
    '                          & If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") _
    '                          & If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0003CustomerList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '                          & If(Params.Item(C_PARAMETERS.LP_ORG), "-") _
    '                          & LIST_BOX_CLASSIFICATION.LC_CUSTOMER
    '    If Not LbMap.ContainsKey(KeyCode) Then
    '        Using CL0003CustomerList As New GL0003CustomerList With {
    '              .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0003CustomerList.LC_CUSTOMER_TYPE.ALL) _
    '            , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '            , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '            , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '            , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0003CustomerList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '            , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG) _
    '            , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE) _
    '            , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
    '            , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "")
    '        }
    '            CL0003CustomerList.getList()
    '            O_RTN = CL0003CustomerList.ERR
    '            Dim lsbx = CL0003CustomerList.LIST
    '            LbMap.Add(KeyCode, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(KeyCode)
    'End Function

    '''' <summary>
    '''' 届先一覧の作成
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateDistinationList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    '○取引先ListBox設定
    '    Dim KeyCode As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0004DestinationList.LC_CUSTOMER_TYPE.ALL) _
    '                          & If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") _
    '                          & If(Params.Item(C_PARAMETERS.LP_CUSTOMER), "-") _
    '                          & If(Params.Item(C_PARAMETERS.LP_CLASSCODE), "-") _
    '                          & If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0004DestinationList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '                          & LIST_BOX_CLASSIFICATION.LC_DISTINATION
    '    If Not LbMap.ContainsKey(KeyCode) Then
    '        Using GL0004DestinationList As New GL0004DestinationList With {
    '              .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0004DestinationList.LC_CUSTOMER_TYPE.ALL) _
    '            , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '            , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '            , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '            , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0004DestinationList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '            , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG) _
    '            , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE) _
    '            , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
    '            , .TORICODE = If(Params.Item(C_PARAMETERS.LP_CUSTOMER), "") _
    '            , .CLASSCODE = If(Params.Item(C_PARAMETERS.LP_CLASSCODE), "") _
    '            , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "")
    '        }
    '            GL0004DestinationList.getList()
    '            O_RTN = GL0004DestinationList.ERR
    '            Dim lsbx = GL0004DestinationList.LIST
    '            LbMap.Add(KeyCode, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(KeyCode)
    'End Function

    ''' <summary>
    ''' 部署(管理・配属)
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateOrg(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○部署ListBox設定
        Dim Categorys As String() = TryCast(Params.Item(C_PARAMETERS.LP_ORG_CATEGORYS), String())
        Dim Key As String = If(Params.Item(C_PARAMETERS.LP_COMPANY), "-")
        For Each category As String In Categorys
            Key = Key & category
        Next
        Key = Key & If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0002OrgList.C_VIEW_FORMAT_PATTERN.NAMES) _
                  & LIST_BOX_CLASSIFICATION.LC_ORG

        If Not LbMap.ContainsKey(Key) Then
            Using CL0002OrgList As New GL0002OrgList With {
                  .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
                , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
                , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
                , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0002OrgList.C_VIEW_FORMAT_PATTERN.NAMES) _
                , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
                , .AUTHWITH = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY) _
                , .Categorys = Categorys _
                , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_MAP) _
                , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE) _
                , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), DirectCast(Parent.Page.Master, OILMasterPage).USER_ORG)
             }
                CL0002OrgList.getList()
                O_RTN = CL0002OrgList.ERR
                Dim lsbx As ListBox = CL0002OrgList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return LbMap.Item(Key)
    End Function

    '''' <summary>
    '''' 社員コード取得
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateStaff(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    '○社員ListBox設定   

    '    Dim Key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0005StaffList.LC_STAFF_TYPE.ALL) &
    '                        If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_ORG), "-")
    '    If Not IsNothing(Params.Item(C_PARAMETERS.LP_STAFF_KBN_LIST)) Then
    '        For Each category In Params.Item(C_PARAMETERS.LP_STAFF_KBN_LIST)
    '            Key = Key & category
    '        Next
    '    End If
    '    Key = Key & If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0005StaffList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '              & LIST_BOX_CLASSIFICATION.LC_STAFFCODE

    '    If Not LbMap.ContainsKey(Key) Then
    '        Using GL0005StaffList As New GL0005StaffList With {
    '           .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0005StaffList.LC_STAFF_TYPE.ALL) _
    '         , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '         , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '         , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '         , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0005StaffList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '         , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
    '         , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "") _
    '         , .STAFFKBN = If(Params.Item(C_PARAMETERS.LP_STAFF_KBN_LIST), Nothing) _
    '         , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG) _
    '         , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE) _
    '         , .STAFFCODE = If(Params.Item(C_PARAMETERS.LP_SELECTED_CODE), String.Empty)
    '        }
    '            GL0005StaffList.getList()
    '            O_RTN = GL0005StaffList.ERR
    '            Dim lsbx As ListBox = GL0005StaffList.LIST
    '            LbMap.Add(Key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(Key)
    'End Function

    '''' <summary>
    '''' 統一車番コード取得
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateCarCode(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    '○統一車番ListBox設定   

    '    Dim Key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0007CarList.LC_LORRY_TYPE.ALL) &
    '                        If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_ORG), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0007CarList.C_VIEW_FORMAT_PATTERN.NAMES) &
    '                         LIST_BOX_CLASSIFICATION.LC_CARCODE
    '    If Not LbMap.ContainsKey(Key) Then
    '        Using GL007CarList As New GL0007CarList With {
    '           .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0007CarList.LC_LORRY_TYPE.ALL) _
    '         , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '         , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '         , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '         , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0007CarList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '         , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
    '         , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "") _
    '         , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG) _
    '         , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE)
    '        }
    '            GL007CarList.getList()
    '            O_RTN = GL007CarList.ERR
    '            Dim lsbx As ListBox = GL007CarList.LIST
    '            LbMap.Add(Key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(Key)
    'End Function

    '''' <summary>
    '''' 業務車番コード取得
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateWorkLorry(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    '○業務車番ListBox設定   

    '    Dim Key As String = If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_ORG), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_OILTYPE), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0008WorkLorryList.C_VIEW_FORMAT_PATTERN.NAMES) &
    '                         LIST_BOX_CLASSIFICATION.LC_WORKLORRY
    '    If Not LbMap.ContainsKey(Key) Then
    '        Using GL008List As New GL0008WorkLorryList With {
    '           .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '         , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '         , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '         , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0008WorkLorryList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '         , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
    '         , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "") _
    '         , .OILTYPE = If(Params.Item(C_PARAMETERS.LP_OILTYPE), "")
    '        }
    '            GL008List.getList()
    '            O_RTN = GL008List.ERR
    '            Dim lsbx As ListBox = GL008List.LIST
    '            LbMap.Add(Key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(Key)
    'End Function

    '''' <summary>
    '''' 品名コード取得
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateGoods(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    '○品名ListBox設定   
    '    Dim key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0006GoodsList.LC_GOODS_TYPE.ALL) &
    '                             If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") &
    '                             If(Params.Item(C_PARAMETERS.LP_OILTYPE), "-") &
    '                             If(Params.Item(C_PARAMETERS.LP_PRODCODE1), "-") &
    '                             If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0006GoodsList.C_VIEW_FORMAT_PATTERN.NAMES) &
    '                              LIST_BOX_CLASSIFICATION.LC_GOODS
    '    If Not LbMap.ContainsKey(key) Then
    '        Using GL006GoodsList As New GL0006GoodsList With {
    '           .TYPE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0006GoodsList.LC_GOODS_TYPE.ALL) _
    '         , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '         , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '         , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '         , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0006GoodsList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '         , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "") _
    '         , .ORGCAMPCODE = If(Params.Item(C_PARAMETERS.LP_ORG_COMP), "") _
    '         , .ORGCODE = If(Params.Item(C_PARAMETERS.LP_ORG), "") _
    '         , .ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG) _
    '         , .PERMISSION = If(Params.Item(C_PARAMETERS.LP_PERMISSION), C_PERMISSION.REFERLANCE) _
    '         , .OILTYPE = If(Params.Item(C_PARAMETERS.LP_OILTYPE), "") _
    '         , .PRODUCT1 = If(Params.Item(C_PARAMETERS.LP_PRODCODE1), "")
    '        }
    '            GL006GoodsList.getList()
    '            O_RTN = GL006GoodsList.ERR
    '            Dim lsbx As ListBox = GL006GoodsList.LIST
    '            LbMap.Add(key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(key)
    'End Function

    '''' <summary>
    '''' 端末コード一覧を作成する
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateTermList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    Dim key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0011TermList.LC_TERM_TYPE.TERMINAL) &
    '                     If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0011TermList.C_VIEW_FORMAT_PATTERN.NAMES) &
    '                      LIST_BOX_CLASSIFICATION.LC_TERM

    '    If Not LbMap.ContainsKey(key) Then
    '        '○会社コードListBox設定
    '        Using GL0011TermList As New GL0011TermList With {
    '           .TYPEMODE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0011TermList.LC_TERM_TYPE.TERMINAL) _
    '         , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '         , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '         , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '         , .CLASSCODE = If(Params.Item(C_PARAMETERS.LP_CLASSCODE), C_TERMCLASS.BASE) _
    '         , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0011TermList.C_VIEW_FORMAT_PATTERN.NAMES)
    '        }
    '            GL0011TermList.getList()
    '            Dim lsbx As ListBox = GL0011TermList.LIST
    '            O_RTN = GL0011TermList.ERR
    '            LbMap.Add(key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(key)
    'End Function

    ''' <summary>
    ''' 権限コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateRoleList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '---20191120追加---_OIS0001USERに利用するため修正
        Dim I_COMP = If(Params.Item(C_PARAMETERS.LP_COMPANY), C_DEFAULT_DATAKEY)
        Dim I_CLASS = Params.Item(C_PARAMETERS.LP_CLASSCODE)
        Dim I_STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now)
        Dim I_ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now)

        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GL0012RoleList As New GL0012RoleList With {
                   .CAMPCODE = I_COMP _
                 , .OBJCODE = I_CLASS _
                 , .STYMD = I_STYMD _
                 , .ENDYMD = I_ENDYMD _
                 , .LIST = lsbx
                }
                GL0012RoleList.getList()
                O_RTN = GL0012RoleList.ERR
                lsbx = GL0012RoleList.LIST
                Dim cnt As Long = lsbx.Rows
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return LbMap.Item(key)
    End Function
    '---20191120追加---

    '---20191120無効化---_修正前のオリジナル
    'Protected Function CreateRoleList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    Dim key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), C_ROLE_VARIANT.USER_ORG) &
    '                        If(Params.Item(C_PARAMETERS.LP_COMPANY), "-") &
    '                        If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0012RoleList.C_VIEW_FORMAT_PATTERN.NAMES) &
    '                        LIST_BOX_CLASSIFICATION.LC_ROLE

    '    If Not LbMap.ContainsKey(key) Then
    '        Using GL0012RoleList As New GL0012RoleList With {
    '            .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '          , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '          , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '          , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0012RoleList.C_VIEW_FORMAT_PATTERN.NAMES) _
    '          , .CAMPCODE = If(Params.Item(C_PARAMETERS.LP_COMPANY), "")
    '        }
    '            Select Case If(Params.Item(C_PARAMETERS.LP_TYPEMODE), "")
    '                Case C_ROLE_VARIANT.USER_COMP
    '                    GL0012RoleList.OBJCODE = C_ROLE_VARIANT.USER_COMP
    '                    GL0012RoleList.ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_COMP)
    '                Case C_ROLE_VARIANT.USER_ORG
    '                    GL0012RoleList.OBJCODE = C_ROLE_VARIANT.USER_ORG
    '                    GL0012RoleList.ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_ORG)
    '                Case C_ROLE_VARIANT.USER_PERTMIT
    '                    GL0012RoleList.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
    '                    GL0012RoleList.ROLECODE = If(Params.Item(C_PARAMETERS.LP_ROLE), DirectCast(Parent.Page.Master, OILMasterPage).ROLE_MAP)
    '            End Select
    '            GL0012RoleList.getList()
    '            Dim lsbx As ListBox = GL0012RoleList.LIST
    '            O_RTN = GL0012RoleList.ERR
    '            LbMap.Add(key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(key)
    'End Function
    '---20191120無効化---

    '''' <summary>
    '''' URL/MAPID一覧を作成する
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateURLList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
    '    Dim key As String = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0013URLList.LC_URL_TYPE.MAPID) &
    '                        If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0013URLList.C_VIEW_FORMAT_PATTERN.NAMES) &
    '                        LIST_BOX_CLASSIFICATION.LC_URL
    '    If Not LbMap.ContainsKey(key) Then
    '        Using GL0013URLList As New GL0013URLList With {
    '            .TYPECODE = If(Params.Item(C_PARAMETERS.LP_TYPEMODE), GL0013URLList.LC_URL_TYPE.MAPID) _
    '          , .STYMD = If(Params.Item(C_PARAMETERS.LP_STYMD), Date.Now) _
    '          , .ENDYMD = If(Params.Item(C_PARAMETERS.LP_ENDYMD), Date.Now) _
    '          , .DEFAULT_SORT = If(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT), String.Empty) _
    '          , .VIEW_FORMAT = If(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT), GL0013URLList.C_VIEW_FORMAT_PATTERN.NAMES)
    '            }
    '            GL0013URLList.getList()
    '            Dim lsbx As ListBox = GL0013URLList.LIST
    '            O_RTN = GL0013URLList.ERR
    '            LbMap.Add(key, lsbx)
    '        End Using
    '    End If
    '    Return LbMap.Item(key)
    'End Function

    '''' <summary>
    '''' 拡張リスト一覧を作成する
    '''' </summary>
    '''' <param name="Params">取得用パラメータ</param>
    '''' <param name="O_RTN">成功可否</param>
    '''' <returns>作成した一覧情報</returns>
    '''' <remarks></remarks>
    'Protected Function CreateExtra(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox

    '    Return If(Params.Item(C_PARAMETERS.LP_LIST), New ListBox)
    'End Function

    ''' <summary>
    ''' ListBox設定共通サブ
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks>固定値一覧情報からリストボックスに表示する固定値を取得する</remarks>
    Protected Function CreateFixValueList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP = If(Params.Item(C_PARAMETERS.LP_COMPANY), C_DEFAULT_DATAKEY)
        Dim I_CLASS = Params.Item(C_PARAMETERS.LP_FIX_CLASS)
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GS0007FIXVALUElst As New GS0007FIXVALUElst With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .LISTBOX1 = lsbx
                }
                GS0007FIXVALUElst.GS0007FIXVALUElst()
                O_RTN = GS0007FIXVALUElst.ERR
                lsbx = GS0007FIXVALUElst.LISTBOX1
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return LbMap.Item(key)
    End Function
    ''' <summary>
    ''' コードからサブコードを取得する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_SUBCODE">サブコード番号</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateSubCodeList(ByVal Params As Hashtable, ByRef O_RTN As String, ByVal I_SUBCODE As Integer) As ListBox
        Dim I_CLASS As String = Params.Item(C_PARAMETERS.LP_FIX_CLASS)
        Dim I_COMP As String = If(Params.Item(C_PARAMETERS.LP_COMPANY), C_DEFAULT_DATAKEY)
        Dim I_KEY As String = I_CLASS & I_SUBCODE
        If Not LbMap.ContainsKey(I_KEY) Then
            Using GS0007FIXVALUElst As New GS0007FIXVALUElst
                Dim lsbx As New ListBox
                GS0007FIXVALUElst.CAMPCODE = I_COMP
                GS0007FIXVALUElst.CLAS = I_CLASS
                Select Case I_SUBCODE
                    Case 3
                        GS0007FIXVALUElst.LISTBOX3 = lsbx
                    Case 4
                        GS0007FIXVALUElst.LISTBOX4 = lsbx
                    Case 5
                        GS0007FIXVALUElst.LISTBOX5 = lsbx
                    Case Else
                        GS0007FIXVALUElst.LISTBOX2 = lsbx
                End Select
                GS0007FIXVALUElst.GS0007FIXVALUElst()
                O_RTN = GS0007FIXVALUElst.ERR
                Select Case I_SUBCODE
                    Case 3
                        lsbx = GS0007FIXVALUElst.LISTBOX3
                    Case 4
                        lsbx = GS0007FIXVALUElst.LISTBOX4
                    Case 5
                        lsbx = GS0007FIXVALUElst.LISTBOX5
                    Case Else
                        lsbx = GS0007FIXVALUElst.LISTBOX2
                End Select
                LbMap.Add(I_KEY, lsbx)
            End Using
        End If

        Return LbMap.Item(I_KEY)
    End Function
    ''' <summary>
    ''' リスト検索
    ''' </summary>
    ''' <param name="I_LISTBOX">検索するリストボックス</param>
    ''' <param name="I_VALUE">検索するKEY</param>
    ''' <param name="O_RTN">成否判定　00000：成功　それ以外：失敗</param>
    ''' <returns >検索結果の値</returns>
    ''' <remarks></remarks>
    Protected Function GetListText(ByVal I_LISTBOX As ListBox, ByVal I_VALUE As String, ByRef O_RTN As String) As String
        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
        '空なら探さない
        If IsNothing(I_LISTBOX) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Return String.Empty
        End If
        For Each item As ListItem In I_LISTBOX.Items
            If item.Value = I_VALUE Then
                O_RTN = C_MESSAGE_NO.NORMAL
                Return item.Text
                Exit For
            End If
        Next
        Return String.Empty

    End Function
    ''' <summary>
    ''' 表示用一覧に追加する
    ''' </summary>
    ''' <param name="box">設定情報</param>
    ''' <remarks></remarks>
    Protected Friend Sub ListToView(ByVal box As ListBox)
        WF_LeftListBox.Items.Clear()
        '空なら設定しない
        If IsNothing(box) Then
            Exit Sub
        End If
        '設定項目があるなら設定する
        For Each item As ListItem In box.Items
            WF_LeftListBox.Items.Add(item)
        Next
    End Sub
    ''' <summary>
    ''' 選択情報の保持
    ''' </summary>
    ''' <param name="SELECT_VALUE"></param>
    ''' <param name="PARAMS"></param>
    ''' <remarks></remarks>
    Protected Sub Backup(ByVal SELECT_VALUE As LIST_BOX_CLASSIFICATION, ByVal PARAMS As Hashtable)
        '〇EXTRA＿LISTはTABLE化する
        If Not IsNothing(PARAMS(C_PARAMETERS.LP_LIST)) Then
            Dim list As ListBox = PARAMS(C_PARAMETERS.LP_LIST)
            Dim htbl As New Hashtable
            For Each item As ListItem In list.Items
                htbl.Add(item.Value, item.Text)
            Next
            PARAMS(C_PARAMETERS.LP_LIST) = htbl
        End If

        ViewState.Add("LF_PARAMS", PARAMS)
        ViewState.Add("LF_LIST_SELECT", SELECT_VALUE)
    End Sub
    ''' <summary>
    ''' 保持した情報の反映
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub Restore(ByRef O_RTN As String)

        If Not IsNothing(ViewState("LF_LIST_SELECT")) Then
            If WF_LEFTMView.ActiveViewIndex = 2 Then
                SetTableList(ViewState("LF_LIST_SELECT"), O_RTN, ViewState("LF_PARAMS"))
            ElseIf WF_LEFTMView.ActiveViewIndex = 0 Then
                Dim params As Hashtable = ViewState("LF_PARAMS")
                '〇EXTRA＿LISTはLISTBOX化する
                If Not IsNothing(params(C_PARAMETERS.LP_LIST)) Then
                    Dim list As New ListBox
                    Dim htbl As Hashtable = params(C_PARAMETERS.LP_LIST)
                    For Each key As String In htbl.Keys
                        list.Items.Add(New ListItem(htbl.Item(key), key))
                    Next
                    params(C_PARAMETERS.LP_LIST) = list
                End If
                SetListBox(ViewState("LF_LIST_SELECT"), O_RTN, ViewState("LF_PARAMS"))
            End If
        End If
    End Sub
End Class
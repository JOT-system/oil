Option Strict On
''' <summary>
''' 左ボックス共通ユーザーコントロールクラス
''' </summary>
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
        LC_PRODUCTLIST
        LC_ORDERSTATUS
        LC_ORDERINFO
        LC_USEPROPRIETY
        LC_BIGOILCODE
        LC_MIDDLEOILCODE
        LC_TRAINCLASS
        LC_SPEEDCLASS
        LC_ORIGINOWNER
        LC_OWNER
        LC_LEASE
        LC_LEASECLASS
        LC_THIRDUSER
        LC_DEDICATETYPE
        LC_EXTRADINARYTYPE
        LC_BASE
        LC_COLOR
        LC_OBTAINED
        LC_SHIPPERSLIST
        LC_CONSIGNEELIST
        LC_STATION
        LC_ORDERTYPE
        LC_PRODUCTSEGLIST
        LC_RINKAITRAIN_INLIST
        LC_RINKAITRAIN_OUTLIST
        LC_RINKAITRAIN_LINELIST
        LC_DEPARRSTATIONLIST
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
        LP_PRODUCTLIST
        LP_ORDERSTATUS
        LP_ORDERINFO
        LP_USEPROPRIETY
        LP_BIGOILCODE
        LP_MIDDLEOILCODE
        LP_TRAINCLASS
        LP_SPEEDCLASS
        LP_ORIGINOWNER
        LP_OWNER
        LP_LEASE
        LP_LEASECLASS
        LP_THIRDUSER
        LP_DEDICATETYPE
        LP_EXTRADINARYTYPE
        LP_BASE
        LP_COLOR
        LP_OBTAINED
        LP_SHIPPERSLIST
        LP_CONSIGNEELIST
        LP_STATION
        LP_ORDERTYPE
        LP_PRODUCTSEGLIST
        LP_ADDITINALCONDITION
        LP_RINKAITRAIN_INLIST
        LP_RINKAITRAIN_OUTLIST
        LP_RINKAITRAIN_LINELIST
        LP_DEPARRSTATIONLIST
    End Enum
    Public Const LEFT_TABLE_SELECTED_KEY As String = "LEFT_TABLE_SELECTED_KEY"
    ''' <summary>
    ''' 作成一覧情報の保持
    ''' </summary>
    Protected LbMap As New Hashtable

    Protected C_TABLE_SPLIT As String = "|"
    Public ReadOnly Property ActiveViewIdx As Integer
        Get
            Return Me.WF_LEFTMView.ActiveViewIndex
        End Get
    End Property
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
                    Restore(O_RTN)
                    '〇取得
                    LF_FILTER_CODE = If(ViewState("LF_FILTER_CODE") Is Nothing, "0", Convert.ToString(ViewState("LF_FILTER_CODE")))
                    LF_SORTING_CODE = If(ViewState("LF_SORTING_CODE") Is Nothing, "0", Convert.ToString(ViewState("LF_SORTING_CODE")))
                    LF_PARAM_DATA = If(ViewState("LF_PARAM_DATA") Is Nothing, "0", Convert.ToString(ViewState("LF_PARAM_DATA")))
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
        LF_LEFTBOX.Style.Clear()
        LF_SORTING_CODE = C_SORTING_CODE.BOTH
        LF_FILTER_CODE = C_FILTER_CODE.ENABLE
        ListToView(CreateListData(ListCode, O_RTN, Params))
        Backup(ListCode, Params)
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
        LF_LEFTBOX.Style.Add(HtmlTextWriterStyle.PaddingBottom, "0")
        LF_LEFTBOX.Style.Add(HtmlTextWriterStyle.PaddingRight, "0")
        LF_LEFTBOX.Style.Add(HtmlTextWriterStyle.Width, "50%")
        LF_LEFTBOX.Style.Add("min-width", "400px")
        LF_LEFTBOX.Style.Add("overflow-y", "hidden")
        LF_SORTING_CODE = C_SORTING_CODE.HIDE
        LF_FILTER_CODE = C_FILTER_CODE.DISABLE
        CreateTableList(ListCode, O_RTN, Params)
        Backup(ListCode, Params)

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

        O_TEXT = GetListText(CreateListData(ListCode, O_RTN, Params), I_VALUE, O_RTN)
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

        O_TEXT = GetListText(CreateSubCodeList(Params, O_RTN, I_SUBCODE), I_VALUE, O_RTN)
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
    ''' 左ボックスで選択した情報を取得
    ''' </summary>
    ''' <returns></returns>
    Public Function GetLeftTableValue() As Dictionary(Of String, String)
        If WF_LEFTMView.ActiveViewIndex <> 2 Then
            Return Nothing
        End If
        Dim retVal As New Dictionary(Of String, String)
        retVal.Add(LEFT_TABLE_SELECTED_KEY, Me.hdnLeftTableSelectedKey.Value)
        Dim retArr As New List(Of String)
        retArr.AddRange(WF_TBL_SELECT.Text.Split(C_TABLE_SPLIT.ToCharArray))
        For Each itm In retArr
            Dim fieldValuePair = itm.Split("=".ToCharArray, 2)
            Dim fieldName As String = fieldValuePair(0)
            Dim value As String
            If fieldValuePair.Count > 2 Then
                value = fieldValuePair(1)
            Else
                value = ""
            End If
            retVal.Add(fieldName, value)
        Next
        Return retVal
    End Function

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
                Dim retArr As New List(Of String)
                retArr.Add(Me.hdnLeftTableSelectedKey.Value)
                retArr.AddRange(WF_TBL_SELECT.Text.Split(C_TABLE_SPLIT.ToCharArray))
                Return retArr.ToArray
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
    Protected Function CreateListData(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing) As ListBox
        If IsNothing(Params) Then
            Params = New Hashtable
        End If
        Dim lbox As ListBox
        Select Case ListCode
            Case LIST_BOX_CLASSIFICATION.LC_COMPANY
                '会社一覧設定
                lbox = CreateCompList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_CUSTOMER
            '    '取引先
            '    lbox = createCustomerList(Params, O_RTN)
            'Case LIST_BOX_CLASSIFICATION.LC_DISTINATION
            '    '届先
            '    lbox = createDistinationList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ORG
                '部署
                lbox = CreateOrg(Params, O_RTN)
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
                lbox = CreateFixValueList(Params, O_RTN)
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
            Case LIST_BOX_CLASSIFICATION.LC_TANKNUMBER
                'タンク車番号
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TANKNUMBER"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_TANKMODEL
                'タンク車型式
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TANKMODEL"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_SALESOFFICE
                '営業所(組織コード)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SALESOFFICE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_TRAINNUMBER
                '本線列車番号
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TRAINNUMBER"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_PRODUCTLIST
                '品種パターン
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "PRODUCTPATTERN"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS
                '受注進行ステータス
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ORDERSTATUS"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ORDERINFO
                '受注情報
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ORDERINFO"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_USEPROPRIETY
                '利用可否
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "USEPROPRIETY"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_BIGOILCODE
                '油種大分類コード
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "BIGOILCODE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_MIDDLEOILCODE
                '油種中分類コード
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "MIDDLEOILCODE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_TRAINCLASS
                '列車区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TRAINCLASS"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_SPEEDCLASS
                '高速列車区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SPEEDCLASS"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ORIGINOWNER
                '原籍所有者
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ORIGINOWNER"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_OWNER
                '名義所有者
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "OWNER"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_LEASE
                'リース先
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "LEASE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_LEASECLASS
                'リース区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "LEASECLASS"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_THIRDUSER
                '第三者使用者
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "THIRDUSER"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_DEDICATETYPE
                '原専用種別
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "DEDICATETYPE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_EXTRADINARYTYPE
                '臨時専用種別
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "EXTRADINARYTYPE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_BASE
                '運用基地
                lbox = CreateBaseList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_COLOR
                '塗色
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "COLOR"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_OBTAINED
                '取得先
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "OBTAINED"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_SHIPPERSLIST
                '荷主
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHIPPERSMASTER"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_CONSIGNEELIST
                '荷受人
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "CONSIGNEEPATTERN"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_ORDERTYPE
                '受注パターン
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ORDERPATTERN"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_PRODUCTSEGLIST
                '品種パターン(受発注用)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "PRODUCTPATTERN_SEG"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_RINKAITRAIN_INLIST
                '臨海鉄道列車番号(入線)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "RINKAITRAIN_I"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_RINKAITRAIN_OUTLIST
                '臨海鉄道列車番号(出線)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "RINKAITRAIN_O"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_RINKAITRAIN_LINELIST
                '臨海鉄道(回線)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "RINKAITRAIN_LINE"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_DEPARRSTATIONLIST
                '発着駅フラグ
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "DEPARRSTATIONFLG"
                lbox = CreateFixValueList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_STATION
                '貨物駅
                lbox = CreateStationList(Params, O_RTN)
            Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                'カレンダー
                lbox = Nothing
            Case Else
                lbox = CreateFixValueList(Params, O_RTN)
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
    Private Sub CreateTableList(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        Dim dispDt As DataTable
        Dim dispFieldsDef As List(Of LeftTableDefItem) = Nothing
        Select Case ListCode
            Case LIST_BOX_CLASSIFICATION.LC_TANKNUMBER
                'タンク車番号
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TANKNUMBER"
                dispDt = CreateFixValueTable(Params, O_RTN)
                '上記データテーブルの表示対象項目を定義(フィールド、表示名）
                dispFieldsDef = New List(Of LeftTableDefItem) From
                    {New LeftTableDefItem("VALUE13", "情報"),
                     New LeftTableDefItem("VALUE12", "状態"),
                     New LeftTableDefItem("VALUE15", "所在地"),
                     New LeftTableDefItem("VALUE5", "油種"),
                     New LeftTableDefItem("VALUE14", "積車"),
                     New LeftTableDefItem("KEYCODE", "車番", True),
                     New LeftTableDefItem("VALUE1", "型式", 10),
                     New LeftTableDefItem("VALUE3", "交換日")}

            Case Else
                Exit Sub
        End Select
        '上記Select Caseで取得したデータテーブル、表示フィールド定義を元に
        '左ボックスのパネルコントロールにレンダリング
        If dispDt IsNot Nothing AndAlso dispFieldsDef IsNot Nothing Then
            MakeTableObject(dispFieldsDef, dispDt, pnlLeftList)
        End If

    End Sub
    ''' <summary>
    ''' 会社コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateCompList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim typeMode As String = ""
        If Params.Item(C_PARAMETERS.LP_TYPEMODE) Is Nothing Then
            typeMode = Convert.ToString(GL0001CompList.LC_COMPANY_TYPE.ROLE)
        Else
            typeMode = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE)).ToString
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
        Dim listClassComp As String = CInt(LIST_BOX_CLASSIFICATION.LC_COMPANY).ToString

        Dim key As String = ""
        key = typeMode & dispFormat & listClassComp

        If Not LbMap.ContainsKey(key) Then
            Dim paramStYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
                paramStYmd = CDate(Params.Item(C_PARAMETERS.LP_STYMD))
            End If
            Dim paramEndYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_ENDYMD) IsNot Nothing Then
                paramEndYmd = CDate(Params.Item(C_PARAMETERS.LP_ENDYMD))
            End If
            Dim roleCode As String = DirectCast(Parent.Page.Master, OILMasterPage).ROLE_MAP
            If Params.Item(C_PARAMETERS.LP_ROLE) IsNot Nothing Then
                roleCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ROLE))
            End If
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            '○会社コードListBox設定
            Using CL0001CompList As New GL0001CompList With {
                   .TYPEMODE = typeMode _
                 , .STYMD = paramStYmd _
                 , .ENDYMD = paramEndYmd _
                 , .ROLECODE = roleCode _
                 , .DEFAULT_SORT = defaultSort _
                 , .VIEW_FORMAT = viewFormat
            }
                CL0001CompList.getList()
                Dim lsbx As ListBox = CL0001CompList.LIST
                O_RTN = CL0001CompList.ERR
                LbMap.Add(key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(key), ListBox)
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
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        For Each category As String In Categorys
            Key = Key & category
        Next
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_ORG).ToString

        If Not LbMap.ContainsKey(Key) Then
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            Dim paramStYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
                paramStYmd = CDate(Params.Item(C_PARAMETERS.LP_STYMD))
            End If
            Dim paramEndYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_ENDYMD) IsNot Nothing Then
                paramEndYmd = CDate(Params.Item(C_PARAMETERS.LP_ENDYMD))
            End If
            Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
            Dim campCode As String = ""
            If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
                campCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
            End If
            Dim authWith = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim authWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                authWith = DirectCast([Enum].ToObject(GetType(GL0002OrgList.LS_AUTHORITY_WITH), CInt(authWithNum)), GL0002OrgList.LS_AUTHORITY_WITH)
            End If
            Dim roleCode As String = DirectCast(Parent.Page.Master, OILMasterPage).ROLE_MAP
            If Params.Item(C_PARAMETERS.LP_ROLE) IsNot Nothing Then
                roleCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ROLE))
            End If
            Dim permission As String = C_PERMISSION.REFERLANCE
            If Params.Item(C_PARAMETERS.LP_PERMISSION) IsNot Nothing Then
                permission = Convert.ToString(Params.Item(C_PARAMETERS.LP_PERMISSION))
            End If
            Dim orgCode As String = DirectCast(Parent.Page.Master, OILMasterPage).USER_ORG
            If Params.Item(C_PARAMETERS.LP_ORG) IsNot Nothing Then
                orgCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ORG))
            End If
            Using CL0002OrgList As New GL0002OrgList With {
                  .DEFAULT_SORT = defaultSort _
                , .STYMD = paramStYmd _
                , .ENDYMD = paramEndYmd _
                , .VIEW_FORMAT = viewFormat _
                , .CAMPCODE = campCode _
                , .AUTHWITH = authWith _
                , .Categorys = Categorys _
                , .ROLECODE = roleCode _
                , .PERMISSION = permission _
                , .ORGCODE = orgCode
             }
                CL0002OrgList.getList()
                O_RTN = CL0002OrgList.ERR
                Dim lsbx As ListBox = CL0002OrgList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
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
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_CLASSCODE))
        Dim I_STYMD As Date = Date.Now
        If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
            I_STYMD = CDate(Params.Item(C_PARAMETERS.LP_STYMD))
        End If
        Dim I_ENDYMD As Date = Date.Now
        If Params.Item(C_PARAMETERS.LP_ENDYMD) IsNot Nothing Then
            I_ENDYMD = CDate(Params.Item(C_PARAMETERS.LP_ENDYMD))
        End If
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

        Return DirectCast(LbMap.Item(key), ListBox)
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
    ''' 基地コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateBaseList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP = If(Params.Item(C_PARAMETERS.LP_COMPANY), C_DEFAULT_DATAKEY)

        Dim key As String = Convert.ToString(I_COMP)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GL0014PLANTList As New GL0014PLANTList With {
                   .CAMPCODE = key _
                 , .LIST = lsbx
                }
                GL0014PLANTList.getList()
                O_RTN = GL0014PLANTList.ERR
                lsbx = GL0014PLANTList.LIST
                Dim cnt As Long = lsbx.Rows
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' 貨物駅一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateStationList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim I_DEPARRFLG As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_STATION))
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GL0015StationList As New GL0015StationList With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .DEPARRSTATIONFLG = I_DEPARRFLG _
                 , .LIST = lsbx
                }
                GL0015StationList.getList()
                O_RTN = GL0015StationList.ERR
                lsbx = GL0015StationList.LIST
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' ListBox設定共通サブ
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks>固定値一覧情報からリストボックスに表示する固定値を取得する</remarks>
    Protected Function CreateFixValueList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GS0007FIXVALUElst As New GS0007FIXVALUElst With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .LISTBOX1 = lsbx
                }
                'FixValue抽出用の追加条件付与
                If Params.ContainsKey(C_PARAMETERS.LP_ADDITINALCONDITION) AndAlso
                   Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION)) <> "" Then
                    GS0007FIXVALUElst.ADDITIONAL_CONDITION = Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION))
                End If
                GS0007FIXVALUElst.GS0007FIXVALUElst()
                O_RTN = GS0007FIXVALUElst.ERR
                lsbx = GS0007FIXVALUElst.LISTBOX1
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function
    ''' <summary>
    ''' Datatable設定共通サブ
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks>固定値一覧情報からリストボックスに表示する固定値を取得する</remarks>
    Protected Function CreateFixValueTable(ByVal Params As Hashtable, ByRef O_RTN As String) As DataTable
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        Dim retDt As DataTable = Nothing
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GS0007FIXVALUElst As New GS0007FIXVALUElst With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .LISTBOX1 = lsbx
                }
                'FixValue抽出用の追加条件付与
                If Params.ContainsKey(C_PARAMETERS.LP_ADDITINALCONDITION) AndAlso
                   Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION)) <> "" Then
                    GS0007FIXVALUElst.ADDITIONAL_CONDITION = Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION))
                End If
                retDt = GS0007FIXVALUElst.GS0007FIXVALUETbl()
                O_RTN = GS0007FIXVALUElst.ERR
            End Using
        End If

        Return retDt
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
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
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

        Return DirectCast(LbMap.Item(I_KEY), ListBox)
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
            Dim list As ListBox = DirectCast(PARAMS(C_PARAMETERS.LP_LIST), ListBox)
            Dim htbl As New Hashtable
            For Each item As ListItem In list.Items
                htbl.Add(item.Value, item.Text)
            Next
            PARAMS(C_PARAMETERS.LP_LIST) = htbl
        End If

        ViewState.Add("LF_PARAMS", PARAMS)
        ViewState.Add("LF_LIST_SELECT", CInt(SELECT_VALUE).ToString)
    End Sub
    ''' <summary>
    ''' 保持した情報の反映
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub Restore(ByRef O_RTN As String)

        If Not IsNothing(ViewState("LF_LIST_SELECT")) Then
            Dim listClass = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(ViewState("LF_LIST_SELECT"))), LIST_BOX_CLASSIFICATION)
            If WF_LEFTMView.ActiveViewIndex = 2 Then
                SetTableList(listClass, O_RTN, DirectCast(ViewState("LF_PARAMS"), Hashtable))
            ElseIf WF_LEFTMView.ActiveViewIndex = 0 Then
                Dim params As Hashtable = DirectCast(ViewState("LF_PARAMS"), Hashtable)
                '〇EXTRA＿LISTはLISTBOX化する
                If Not IsNothing(params(C_PARAMETERS.LP_LIST)) Then
                    Dim list As New ListBox
                    Dim htbl As Hashtable = DirectCast(params(C_PARAMETERS.LP_LIST), Hashtable)
                    For Each key As String In htbl.Keys
                        list.Items.Add(New ListItem(Convert.ToString(htbl.Item(key)), key))
                    Next
                    params(C_PARAMETERS.LP_LIST) = list
                End If
                SetListBox(listClass, O_RTN, DirectCast(ViewState("LF_PARAMS"), Hashtable))
            End If
        End If
    End Sub
#Region "左ボックスのテーブル表処理関連"
    ''' <summary>
    ''' テーブルオブジェクト展開
    ''' </summary>
    ''' <param name="leftTableDefs">カラム定義</param>
    ''' <param name="outArea">出力先(Panel)コントロール</param>
    Private Sub MakeTableObject(ByVal leftTableDefs As List(Of LeftTableDefItem), ByVal srcTbl As DataTable, outArea As Panel)

        '●項目定義取得
        Dim outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim lenghtFix As Integer = 0
        Dim leftFixAll As Integer = 32
        Dim rightLengthFixAll As Integer = 0

        'ソートキー領域作成
        Dim sortItemId As String = "hdnListSortValue" & outArea.Page.Form.ClientID & outArea.ID
        Dim sortValue As String = ""
        Dim sortItems As New HiddenField With {.ID = sortItemId, .ViewStateMode = UI.ViewStateMode.Disabled}
        If outArea.Page.Request.Form.GetValues(sortItemId) IsNot Nothing Then
            sortValue = outArea.Page.Request.Form.GetValues(sortItemId)(0)
        End If
        sortItems.Value = sortValue
        outArea.Controls.Add(sortItems)
        'テーブル全体のタグ
        Dim tableObj As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        tableObj.Attributes.Add("class", "leftTable")
        ' ヘッダー作成
        Dim wholeHeaderWrapper As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        wholeHeaderWrapper.Attributes.Add("class", "leftTableHeaderWrapper")
        Dim wholeHeader As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim keyFieldName As String = ""
        wholeHeader.Attributes.Add("class", "leftTableHeader")
        For Each leftTableDef In leftTableDefs
            'データテーブルに対象カラムが含まれていない場合はスキップ
            If srcTbl IsNot Nothing AndAlso srcTbl.Columns.Contains(leftTableDef.FieldName) = False Then
                leftTableDef.HasDtColumn = False
                Continue For
            End If

            If keyFieldName = "" AndAlso leftTableDef.KeyField Then
                keyFieldName = leftTableDef.FieldName
            End If

            Dim headerCell As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
            Dim headerCellValue As New HtmlGenericControl("span") With {.ViewStateMode = UI.ViewStateMode.Disabled}
            headerCellValue.Attributes.Add("data-fieldname", leftTableDef.FieldName)
            headerCellValue.InnerHtml = leftTableDef.DispFieldName
            lenghtFix = leftTableDef.Length * 16

            If lenghtFix = 0 Then
                headerCell.Style.Add("display", "none")
            Else
                headerCell.Style.Add("width", lenghtFix.ToString & "px")
                headerCell.Style.Add("min-width", lenghtFix.ToString & "px")
            End If
            headerCell.Controls.Add(headerCellValue)
            wholeHeader.Controls.Add(headerCell)
        Next leftTableDef
        'キーフィールド設定が無い場合は最左のフィールドをキーとする
        If keyFieldName = "" Then
            keyFieldName = (From val In leftTableDefs Where val.HasDtColumn).FirstOrDefault.FieldName
        End If

        wholeHeaderWrapper.Controls.Add(wholeHeader)
        tableObj.Controls.Add(wholeHeaderWrapper)
        ' データ
        Dim scrDr As DataRow = Nothing
        Dim wholeDataRowWrapper As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        wholeDataRowWrapper.Attributes.Add("class", "leftTableDataWrapper")
        Dim wholeDataRow As HtmlGenericControl
        Dim dataCell As HtmlGenericControl
        Dim dataCellValue As HtmlGenericControl
        Dim keyValue As String = ""
        'Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim dicFieldValues As Dictionary(Of String, String)
        'Dim base64Str As String = ""
        'Dim noConpressionByte As Byte()

        For i As Integer = 0 To srcTbl.Rows.Count - 1
            scrDr = srcTbl(i)
            dicFieldValues = New Dictionary(Of String, String)
            wholeDataRow = New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}

            For Each leftTableDef In leftTableDefs
                If leftTableDef.HasDtColumn = False Then
                    Continue For
                End If
                dataCell = New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
                dataCellValue = New HtmlGenericControl("span") With {.ViewStateMode = UI.ViewStateMode.Disabled}

                Dim fieldName As String = leftTableDef.FieldName
                Dim fieldValue As String = Convert.ToString(scrDr(fieldName))
                dataCellValue.InnerHtml = fieldValue
                dicFieldValues.Add(fieldName, fieldValue)
                'テーブルセルのサイズ
                If leftTableDef.Length * 16 = 0 Then
                    dataCell.Style.Add("display", "none")
                Else
                    Dim cellWidth As String = (leftTableDef.Length * 16).ToString
                    dataCell.Style.Add("width", cellWidth & "px")
                    dataCell.Style.Add("min-width", cellWidth & "px")
                    If leftTableDef.Align <> "" Then
                        dataCell.Style.Add(HtmlTextWriterStyle.TextAlign, leftTableDef.Align)
                    End If
                End If
                dataCell.Attributes.Add("data-fieldname", leftTableDef.FieldName)
                dataCell.Controls.Add(dataCellValue)
                wholeDataRow.Controls.Add(dataCell)
            Next leftTableDef

            keyValue = Convert.ToString(scrDr(keyFieldName))
            ''クラスをシリアライズ
            'Using ms As New IO.MemoryStream()
            '    formatter.Serialize(ms, dicFieldValues)
            '    noConpressionByte = ms.ToArray
            'End Using
            ''圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
            'Using ms As New IO.MemoryStream(),
            '  ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            '    ds.Write(noConpressionByte, 0, noConpressionByte.Length)
            '    ds.Close()
            '    Dim byteDat = ms.ToArray
            '    base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
            'End Using
            Dim fieldValuesStr = String.Join(C_TABLE_SPLIT, (From x In dicFieldValues Select String.Format("{0}={1}", x.Key, x.Value)))
            wholeDataRow.Style.Add("order", (i + 1).ToString)
            wholeDataRow.Attributes.Add("data-initorder", (i + 1).ToString)
            wholeDataRow.Attributes.Add("data-key", keyValue)
            wholeDataRow.Attributes.Add("data-values", fieldValuesStr)
            wholeDataRow.Attributes.Add("onclick", "WF_TableF_DbClick(this);")
            If srcTbl.Rows.Count - 1 = i Then
                wholeDataRow.Attributes.Add("class", "leftTableDataRow lastRow")
            Else
                wholeDataRow.Attributes.Add("class", "leftTableDataRow")
            End If
            wholeDataRowWrapper.Controls.Add(wholeDataRow)
        Next i

        tableObj.Controls.Add(wholeDataRowWrapper)
        outArea.Controls.Add(tableObj)
        Dim style As New HtmlGenericControl("style") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        'Edgeで背面のdivContensbox横スクロールBox効くので抑止(leftBox表示中は)
        style.InnerHtml = "#divContensbox {overflow:hidden;}"
        outArea.Controls.Add(style)
    End Sub
    ''' <summary>
    ''' 左ボックス用テーブルの出力フィールド定義(1カラム分)
    ''' </summary>
    Public Class LeftTableDefItem
        ''' <summary>
        ''' フィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 画面表示フィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispFieldName As String
        ''' <summary>
        ''' 表示幅
        ''' </summary>
        ''' <returns></returns>
        Public Property Length As Integer
        ''' <summary>
        ''' 参照テーブルに対象のフィールド名を保持しているか
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>描画処理で使用するので設定の使う側は設定の意識不要</remarks>
        Public Property HasDtColumn As Boolean = True
        Public Property TextAlign As StyleCollection
        ''' <summary>
        ''' キーフィールド設定、選択したキーとなるフィールド（True：キー、False：非キー）
        ''' 未設定の場合、表示上最左（先頭列がキーとなる）複数ある場合は１つのみ
        ''' </summary>
        ''' <returns></returns>
        Public Property KeyField As Boolean = False
        ''' <summary>
        ''' テキストの表示位置設定（"left","right","center"等を設定）
        ''' </summary>
        ''' <returns></returns>
        Public Property Align As String = ""
        ''' <summary>
        ''' 数字フィールド（True:数字フィールド,False:通常フィールド）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>一旦未使用</remarks>
        Public Property IsNumericField As Boolean = False
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示フィールド名</param>
        ''' <param name="length">幅</param>
        ''' <param name="align">テキスト表示位置</param>
        ''' <param name="keyField">キーフィールド</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer, align As String, keyField As Boolean)
            Me.FieldName = fieldName
            Me.DispFieldName = dispFieldName
            Me.Length = length
            Me.Align = align
            Me.KeyField = keyField
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示フィールド名</param>
        ''' <param name="length">幅</param>
        ''' <param name="align">テキスト表示位置</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer, align As String)
            Me.New(fieldName, dispFieldName, length, align, False)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="DispFieldName">画面表示カラム</param>
        ''' <param name="Length">サイズ</param>
        ''' <param name = "keyField" > キーフィールド</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer, keyField As Boolean)
            Me.New(fieldName, dispFieldName, length, "")
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="DispFieldName">画面表示カラム</param>
        ''' <param name="Length">サイズ</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer)
            Me.New(fieldName, dispFieldName, length, "")
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示カラム</param>
        ''' <param name="keyField">キーフィールド</param>
        Public Sub New(fieldName As String, dispFieldName As String, keyField As Boolean)
            Me.New(fieldName, dispFieldName, 6, "", keyField)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示カラム</param>
        Public Sub New(fieldName As String, dispFieldName As String)
            Me.New(fieldName, dispFieldName, 6)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="param"></param>
        Public Sub New(ParamArray param() As String)
            Me.FieldName = param(0)
            Me.DispFieldName = param(1)
            If param.Length = 3 Then
                Me.Length = CInt(param(3))
            End If
        End Sub
    End Class

#End Region

End Class
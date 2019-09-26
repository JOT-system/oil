Imports System.Data.SqlClient
Imports OFFICE.GRIS0005LeftBox
Imports BASEDLL

Public Class GRT00007WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "T00007S"       'MAPID(条件)
    Public Const MAPIDI As String = "T00007I"       'MAPID(一覧)
    Public Const MAPIDINJS As String = "T00007INJS" 'MAPID(一覧)
    Public Const MAPIDIKNK As String = "T00007IKNK" 'MAPID(一覧)
    Public Const MAPIDIJKT As String = "T00007IJKT" 'MAPID(一覧)
    Public Const MAPID As String = "T00007"         'MAPID(実行)
    Public Const MAPIDNJS As String = "T00007NJS"   'MAPID(実行)
    Public Const MAPIDKNK As String = "T00007KNK"   'MAPID(実行)
    Public Const MAPIDJKT As String = "T00007JKT"   'MAPID(実行)
    Public Const MAPIDNIP As String = "T00005"      'MAPID(日報)

    Public Const MAPVR As String = "ENX"            'MAPVARIANT
    Public Const MAPVRNJS As String = "NJS"         'MAPVARIANT
    Public Const MAPVRKNK As String = "KNK"         'MAPVARIANT
    Public Const MAPVRJKT As String = "JKT"         'MAPVARIANT

    Public Const CONST_OTHER = "OTHERWORK"          'その他作業
    Public Const CONST_SPEC = "SPECIALORG"          '特殊計算
    Public Const CONST_HAYA = "HAYADETIME"          '早出補填入力

    '○ 共通関数宣言(BASEDLL)
    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' SQL異常対応
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SQLAbnormalityRepair()

        '会社コード
        If WF_T7SEL_CAMPCODE.Text = Nothing Then
            WF_T7SEL_CAMPCODE.Text = ""
        End If

        '対象年月
        If WF_T7SEL_TAISHOYM.Text = Nothing Then
            WF_T7SEL_TAISHOYM.Text = ""
        End If

        '運用部署
        If WF_T7SEL_HORG.Text = Nothing Then
            WF_T7SEL_HORG.Text = ""
        End If

        '職種区分
        If WF_T7SEL_STAFFKBN.Text = Nothing Then
            WF_T7SEL_STAFFKBN.Text = ""
        End If

        '従業員コード
        If WF_T7SEL_STAFFCODE.Text = Nothing Then
            WF_T7SEL_STAFFCODE.Text = ""
        End If

        '従業員名称
        If WF_T7SEL_STAFFNAME.Text = Nothing Then
            WF_T7SEL_STAFFNAME.Text = ""
        End If

        '画面ID
        If WF_SEL_VIEWID.Text = Nothing Then
            WF_SEL_VIEWID.Text = ""
        End If

        '画面ID（個別）
        If WF_SEL_VIEWID_DTL.Text = Nothing Then
            WF_SEL_VIEWID_DTL.Text = ""
        End If

        'MAP変数
        If WF_SEL_MAPvariant.Text = Nothing Then
            WF_SEL_MAPvariant.Text = ""
        End If

        'MAP権限
        If WF_SEL_MAPpermitcode.Text = Nothing Then
            WF_SEL_MAPpermitcode.Text = ""
        End If

        '押下ボタン
        If WF_T7SEL_BUTTON.Text = Nothing Then
            WF_T7SEL_BUTTON.Text = ""
        End If

        '締状態
        If WF_T7SEL_LIMITFLG.Text = Nothing Then
            WF_T7SEL_LIMITFLG.Text = ""
        End If

        '権限
        If WF_T7SEL_PERMITCODE.Text = Nothing Then
            WF_T7SEL_PERMITCODE.Text = ""
        End If

        'サーバー状態
        If WF_T7SEL_SRVSTAT.Text = Nothing Then
            WF_T7SEL_SRVSTAT.Text = ""
        End If

        '画面一覧保存パス
        If WF_T7SEL_XMLsaveTmp.Text = Nothing Then
            WF_T7SEL_XMLsaveTmp.Text = ""
        End If

        '抽出条件保存パス
        If WF_T7SEL_XMLsavePARM.Text = Nothing Then
            WF_T7SEL_XMLsavePARM.Text = ""
        End If

        '画面一覧保存パス
        If WF_T7I_XMLsaveF.Text = Nothing Then
            WF_T7I_XMLsaveF.Text = ""
        End If

        'GridView位置
        If WF_T7I_GridPosition.Text = Nothing Then
            WF_T7I_GridPosition.Text = ""
        End If

        'ヘッダの従業員
        If WF_T7I_Head_STAFFCODE.Text = Nothing Then
            WF_T7I_Head_STAFFCODE.Text = ""
        End If

        'ヘッダの日付
        If WF_T7I_Head_WORKDATE.Text = Nothing Then
            WF_T7I_Head_WORKDATE.Text = ""
        End If

        'ヘッダの日報日FROM
        If WF_T7I_Head_NIPPO_FROM.Text = Nothing Then
            WF_T7I_Head_NIPPO_FROM.Text = ""
        End If

        'ヘッダの日報日TO
        If WF_T7I_Head_NIPPO_TO.Text = Nothing Then
            WF_T7I_Head_NIPPO_TO.Text = ""
        End If

        '選択番号
        If WF_T7KIN_LINECNT.Text = Nothing Then
            WF_T7KIN_LINECNT.Text = ""
        End If

        '明細の日付
        If WF_T7KIN_WORKDATE.Text = Nothing Then
            WF_T7KIN_WORKDATE.Text = ""
        End If

        '明細の従業員
        If WF_T7KIN_STAFFCODE.Text = Nothing Then
            WF_T7KIN_STAFFCODE.Text = ""
        End If

        '明細のレコード区分
        If WF_T7KIN_RECODEKBN.Text = Nothing Then
            WF_T7KIN_RECODEKBN.Text = ""
        End If

        '画面一覧保存パス
        If WF_T7KIN_XMLsaveF.Text = Nothing Then
            WF_T7KIN_XMLsaveF.Text = ""
        End If

        'モデル保存パス
        If WF_T7KIN_XMLsaveF2.Text = Nothing Then
            WF_T7KIN_XMLsaveF2.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5I_XMLsaveF.Text = Nothing Then
            WF_T5I_XMLsaveF.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5I_XMLsaveF9.Text = Nothing Then
            WF_T5I_XMLsaveF9.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5I_LINECNT.Text = Nothing Then
            WF_T5I_LINECNT.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5I_GridPosition.Text = Nothing Then
            WF_T5I_GridPosition.Text = ""
        End If

        ''会社
        'If WF_T5_CAMPCODE.Text = Nothing Then
        '    WF_T5_CAMPCODE.Text = ""
        'End If

        '出庫年月日開始
        If WF_T5_STYMD.Text = Nothing Then
            WF_T5_STYMD.Text = ""
        End If

        '出庫年月日終了
        If WF_T5_ENDYMD.Text = Nothing Then
            WF_T5_ENDYMD.Text = ""
        End If

        '運用部署
        If WF_T5_UORG.Text = Nothing Then
            WF_T5_UORG.Text = ""
        End If

        '従業員
        If WF_T5_STAFFCODE.Text = Nothing Then
            WF_T5_STAFFCODE.Text = ""
        End If

        '従業員名
        If WF_T5_STAFFNAME.Text = Nothing Then
            WF_T5_STAFFNAME.Text = ""
        End If

        '画面ID
        If WF_T5_VIEWID.Text = Nothing Then
            WF_T5_VIEWID.Text = ""
        End If

        '画面ID（個別）
        If WF_T5_VIEWID_DTL.Text = Nothing Then
            WF_T5_VIEWID_DTL.Text = ""
        End If

        'MAP変数
        If WF_T5_MAPvariant.Text = Nothing Then
            WF_T5_MAPvariant.Text = ""
        End If

        'MAP変数
        If WF_T5_MAPpermitcode.Text = Nothing Then
            WF_T5_MAPpermitcode.Text = ""
        End If

        '押下ボタン
        If WF_T5_BUTTON.Text = Nothing Then
            WF_T5_BUTTON.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5_XMLsaveTmp.Text = Nothing Then
            WF_T5_XMLsaveTmp.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5_XMLsaveTmp9.Text = Nothing Then
            WF_T5_XMLsaveTmp9.Text = ""
        End If

        '抽出条件保存パス
        If WF_T5_XMLsavePARM.Text = Nothing Then
            WF_T5_XMLsavePARM.Text = ""
        End If

        '画面一覧保存パス
        If WF_T5_XMLsaveF2.Text = Nothing Then
            WF_T5_XMLsaveF2.Text = ""
        End If

        'ヘッダの日付
        If WF_T5_YMD.Text = Nothing Then
            WF_T5_YMD.Text = ""
        End If

        '呼出元MAPID
        If WF_T5_FROMMAPID.Text = Nothing Then
            WF_T5_FROMMAPID.Text = ""
        End If

        '呼出元MAPVARI
        If WF_T5_FROMMAPVARIANT.Text = Nothing Then
            WF_T5_FROMMAPVARIANT.Text = ""
        End If

        '会社コード
        If WF_SEL_CAMPCODE.Text = Nothing Then
            WF_SEL_CAMPCODE.Text = ""
        End If

        '出庫年月日開始
        If WF_SEL_STYMD.Text = Nothing Then
            WF_SEL_STYMD.Text = ""
        End If

        '出庫年月日終了
        If WF_SEL_ENDYMD.Text = Nothing Then
            WF_SEL_ENDYMD.Text = ""
        End If

        '運用部署
        If WF_SEL_UORG.Text = Nothing Then
            WF_SEL_UORG.Text = ""
        End If

        '画面一覧保存パス
        If WF_SEL_XMLsaveF.Text = Nothing Then
            WF_SEL_XMLsaveF.Text = ""
        End If

        '画面一覧保存パス
        If WF_SEL_XMLsaveF9.Text = Nothing Then
            WF_SEL_XMLsaveF9.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' 乗務員一覧取得
    ''' </summary>
    ''' <param name="CAMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="TAISHOYM">対象年月</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function getStaffCodeList(ByVal CAMPCODE As String, ByVal TAISHOYM As String, ByVal ORGCODE As String) As Hashtable
        Dim prmData As New Hashtable
        Dim wDATE As Date
        Try
            wDATE = TAISHOYM & "/01"
        Catch ex As Exception
            wDATE = Date.Now
        End Try
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = CAMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0005StaffList.LC_STAFF_TYPE.ATTENDANCE_FOR_DRIVER_IN_AORG
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_STYMD) = TAISHOYM & "/" & "01"
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ENDYMD) = TAISHOYM & "/" & DateTime.DaysInMonth(wDATE.Year, wDATE.Month).ToString("00")
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_DEFAULT_SORT) = GL0000.C_DEFAULT_SORT.SEQ
        Return prmData
    End Function

    ''' <summary>
    ''' 部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE,
            GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE,
            GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT,
            GL0002OrgList.C_CATEGORY_LIST.OFFICER}

        CreateORGParam = prmData

    End Function

    ''' <summary>
    ''' 配属部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateHORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.UPDATE
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateHORGParam = prmData

    End Function

    ''' <summary>
    ''' 端末区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateTERMKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "TERMKBN"

        CreateTERMKBNParam = prmData

    End Function

    ''' <summary>
    ''' 乗務区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateCREWKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "CREWKBN"

        CreateCREWKBNParam = prmData

    End Function

    ''' <summary>
    ''' 曜日パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateWORKINGWEEKParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "WORKINGWEEK"

        CreateWORKINGWEEKParam = prmData

    End Function

    ''' <summary>
    ''' レコード区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateRECODEKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "RECODEKBN"

        CreateRECODEKBNParam = prmData

    End Function

    ''' <summary>
    ''' 車両区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateSHARYOKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "SHARYOKBN"

        CreateSHARYOKBNParam = prmData

    End Function

    ''' <summary>
    ''' 油種給与パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateOILPAYKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "OILPAYKBN"

        CreateOILPAYKBNParam = prmData

    End Function

    ''' <summary>
    ''' 休日区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateHOLIDAYKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "HOLIDAYKBN"

        CreateHOLIDAYKBNParam = prmData

    End Function

    ''' <summary>
    ''' 勤怠区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreatePAYKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "PAYKBN"

        CreatePAYKBNParam = prmData

    End Function

    ''' <summary>
    ''' 宿日直区分パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateSHUKCHOKKBNParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "SHUKCHOKKBN"

        CreateSHUKCHOKKBNParam = prmData

    End Function

    ''' <summary>
    ''' 緯度パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateLATITUDEParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "LATITUDE"

        CreateLATITUDEParam = prmData

    End Function

    ''' <summary>
    ''' 経度パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateLONGITUDEParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "LONGITUDE"

        CreateLONGITUDEParam = prmData

    End Function

    ''' <summary>
    ''' モデル特殊処理コードパラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateMODELCODEParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "MODELCODE"

        CreateMODELCODEParam = prmData

    End Function

    ''' <summary>
    ''' モデル特殊処理距離パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateMODELDISTANCEParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_FIX_CLASS) = "MODELDISTANCE"

        CreateMODELDISTANCEParam = prmData

    End Function

    ''' <summary>
    ''' 職務区分パラメーター
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateStaffKbnParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        Dim StaffKbnList As New ListBox
        Dim SQLcon = CS0050SESSION.getConnection()
        SQLcon.Open()
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing
        Dim PARA(10) As SqlParameter
        Dim SQLStr As New StringBuilder

        '○ 職務区分リストボックス作成
        Try

            '検索SQL文
            SQLStr.AppendLine(" SELECT rtrim(KEYCODE) as KEYCODE    ")
            SQLStr.AppendLine("       ,rtrim(VALUE1)  as VALUE1     ")
            SQLStr.AppendLine(" FROM  MC001_FIXVALUE                ")
            SQLStr.AppendLine(" Where CAMPCODE  = @P1               ")
            SQLStr.AppendLine("   and CLASS     = @P2               ")
            SQLStr.AppendLine("   and STYMD    <= @P3               ")
            SQLStr.AppendLine("   and ENDYMD   >= @P4               ")
            SQLStr.AppendLine("   and DELFLG   <> @P5               ")
            SQLStr.AppendLine("   and KEYCODE LIKE '03%'        ")
            SQLStr.AppendLine("ORDER BY KEYCODE                     ")

            SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)
            PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            PARA(4) = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            PARA(5) = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.NVarChar)
            PARA(1).Value = I_COMPCODE
            PARA(2).Value = "STAFFKBN"
            PARA(3).Value = Date.Now
            PARA(4).Value = Date.Now
            PARA(5).Value = C_DELETE_FLG.DELETE

            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                StaffKbnList.Items.Add(New ListItem(SQLdr("VALUE1"), SQLdr("KEYCODE")))
            End While

            prmData.Item(C_PARAMETERS.LP_LIST) = StaffKbnList

        Finally
            If Not IsNothing(SQLdr) Then
                SQLdr.Close()
                SQLdr = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close()
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

        CreateStaffKbnParam = prmData

    End Function

    ''' <summary>
    ''' 取引先名称パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateCustomerParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL

        CreateCustomerParam = prmData

    End Function

    ''' <summary>
    ''' 届先名称パラメーター
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="ORGCODE">部署コード</param>
    ''' <param name="SHIPCODE">取引先コード</param>
    ''' <param name="CLASSCODE">区分コード</param>
    ''' <returns>検索条件一覧</returns>
    ''' <remarks></remarks>
    Function createDistinationParam(ByVal COMPCODE As String, ByVal ORGCODE As String, ByVal SHIPCODE As String, ByVal CLASSCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_ORG) = ORGCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CUSTOMER) = SHIPCODE
        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_CLASSCODE) = CLASSCODE

        prmData.Item(GRIS0005LeftBox.C_PARAMETERS.LP_TYPEMODE) = GL0004DestinationList.LC_CUSTOMER_TYPE.ALL
        Return prmData
    End Function

    ''' <summary>
    ''' 品名パラメーター
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreatePRODUCTParam(ByVal I_COMPCODE As String, ByVal I_HORG As String) As Hashtable

        Dim prmData As New Hashtable
        Dim ProductCodeList As New ListBox
        Dim SQLcon = CS0050SESSION.getConnection()
        SQLcon.Open()
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing
        Dim PARA(10) As SqlParameter
        Dim SQLStr As New StringBuilder

        '○ 品名リストボックス作成
        Try

            '検索SQL文
            SQLStr.AppendLine(" SELECT isnull(rtrim(A.OILTYPE),'')  as OILTYPE      ")
            SQLStr.AppendLine("       ,isnull(rtrim(A.PRODUCT1),'') as PRODUCT1     ")
            SQLStr.AppendLine("       ,isnull(rtrim(A.PRODUCT2),'') as PRODUCT2     ")
            SQLStr.AppendLine("       ,isnull(rtrim(B.NAMES),'')    as PRODUCT2NAME ")
            SQLStr.AppendLine("       ,isnull(rtrim(B.STANI),'')    as STANI        ")
            SQLStr.AppendLine(" FROM  MC005_PRODORG A                               ")
            SQLStr.AppendLine(" INNER JOIN MC004_PRODUCT B                          ")
            SQLStr.AppendLine("         ON    B.OILTYPE     = A.OILTYPE             ")
            SQLStr.AppendLine("         and   B.PRODUCT1    = A.PRODUCT1            ")
            SQLStr.AppendLine("         and   B.PRODUCT2    = A.PRODUCT2            ")
            SQLStr.AppendLine("         and   B.STYMD      <= @P3                   ")
            SQLStr.AppendLine("         and   B.ENDYMD     >= @P3                   ")
            SQLStr.AppendLine("         and   B.DELFLG     <> '1'                   ")
            SQLStr.AppendLine("       Where   A.CAMPCODE    = @P1                   ")
            SQLStr.AppendLine("         and   A.UORG        = @P2                   ")
            SQLStr.AppendLine("         and   A.STYMD      <= @P3                   ")
            SQLStr.AppendLine("         and   A.ENDYMD     >= @P3                   ")
            SQLStr.AppendLine("         and   A.DELFLG     <> '1'                   ")
            SQLStr.AppendLine(" GROUP BY A.OILTYPE , A.PRODUCT1 , A.PRODUCT2 , B.NAMES, B.STANI ")
            SQLStr.AppendLine(" ORDER BY A.OILTYPE , A.PRODUCT1 , A.PRODUCT2 , B.NAMES, B.STANI ")

            SQLcmd = New SqlCommand(SQLStr.ToString, SQLcon)
            PARA(1) = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            PARA(2) = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.NVarChar)
            PARA(3) = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            PARA(1).Value = I_COMPCODE
            PARA(2).Value = I_HORG
            PARA(3).Value = Date.Now
            SQLdr = SQLcmd.ExecuteReader()

            Dim WW_PRODUCT As String = ""
            While SQLdr.Read
                WW_PRODUCT = SQLdr("OILTYPE") & SQLdr("PRODUCT1") & SQLdr("PRODUCT2")
                ProductCodeList.Items.Add(New ListItem(SQLdr("PRODUCT2NAME"), WW_PRODUCT))
            End While

            prmData.Item(C_PARAMETERS.LP_LIST) = ProductCodeList

        Finally
            If Not IsNothing(SQLdr) Then
                SQLdr.Close()
                SQLdr = Nothing
            End If

            SQLcmd.Dispose()
            SQLcmd = Nothing

            SQLcon.Close()
            SQLcon.Dispose()
            SQLcon = Nothing
        End Try

        CreatePRODUCTParam = prmData

    End Function
End Class
Option Strict On
Imports System.Web
Imports System.Data.SqlClient
Imports System.Web.SessionState
Imports System.Configuration

''' <summary>
''' セッション情報操作処理
''' </summary>
''' <remarks></remarks>
Public Class CS0050SESSION : Implements IDisposable
    ''' <summary>
    ''' セッション情報
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SESSION As HttpSessionState
    ''' <summary>
    ''' 名前空間名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMESPACE_VALUE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.NAMESPACE_VALUE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.NAMESPACE_VALUE) = value
        End Set
    End Property
    ''' <summary>
    ''' クラス名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLASS_NAME As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.CLASS_NAME))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.CLASS_NAME) = value
        End Set
    End Property
    ''' <summary>
    ''' DB接続文字列
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DBCon As String
        Get
            Dim GetStr As String
            GetStr = ConfigurationManager.AppSettings(C_SESSION_KEY.DB_CONNECT)

            If GetStr = Nothing Then
                SESSION = If(SESSION, HttpContext.Current.Session)
                Return Convert.ToString(SESSION(C_SESSION_KEY.DB_CONNECT))
            Else
                Return GetStr
            End If
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.DB_CONNECT) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.USER_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.USER_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.USER_TERM_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.USER_TERM_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末保持会社
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERM_COMPANY As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_COMPANY))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_COMPANY) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末保持部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERM_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末管理部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERM_M_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_MANAGMENT_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_MANAGMENT_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' 選択別会社
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SELECTED_COMPANY As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SELECTED_ANOTHER_COMPANY))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SELECTED_ANOTHER_COMPANY) = value
        End Set
    End Property
    ''' <summary>
    ''' TERM_DRIVERS
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DRIVERS As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_DRIVERS))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_DRIVERS) = value
        End Set
    End Property
    ''' <summary>
    ''' ログ格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LOG_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.LOGGING_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.LOGGING_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' 情報退避XML格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PDF_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.PDF_PRINT_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.PDF_PRINT_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' アップロードFILE格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property UPLOAD_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.UPLOADED_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.UPLOADED_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' 更新ジャーナル格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property JORNAL_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.UPDATE_JORNALING_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.UPDATE_JORNALING_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' システム格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SYSTEM_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SYSTEM_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SYSTEM_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_ID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_TERM_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_TERM_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末保持会社
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_COMPANY As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_FOUNDIION_COMPAY))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_FOUNDIION_COMPAY) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末保持部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_FOUNDIION_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_FOUNDIION_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末管理部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_M_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_MANAGMENT_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_MANAGMENT_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' MAPID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MAPID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_DISPLAY_MAP_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_DISPLAY_MAP_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' MENU_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MENU_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_MENU_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_MENU_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' MAP_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MAP_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_MAP_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_MAP_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' VIEWPROF_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_VIEWPROF_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_VIEWPROF_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_VIEWPROF_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' RPRTPROF_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_RPRTPROF_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_RPRTPROF_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_RPRTPROF_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' APPROVALID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_APPROVALID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_APPROVALID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_APPROVALID) = value
        End Set
    End Property
    ''' <summary>
    ''' MAPVARIANT
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MAP_VARIANT As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_MAP_VARIANT))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_MAP_VARIANT) = value
        End Set
    End Property
    ''' <summary>
    ''' PERTMISSION
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_PERMIT As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_PERMISSION_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_PERMISSION_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' ETC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAP_ETC As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_ETC_VALUE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_ETC_VALUE) = value
        End Set
    End Property
    ''' <summary>
    ''' ヘルプ表示画面ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HELP_ID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_HELP_MAP_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_HELP_MAP_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' ヘルプ表示会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HELP_COMP As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_HELP_COMP_CD))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_HELP_COMP_CD) = value
        End Set
    End Property
    ''' <summary>
    ''' LOGONDATE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LOGONDATE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.LOGON_LOGIN_DATE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.LOGON_LOGIN_DATE) = value
        End Set
    End Property
    ''' <summary>
    ''' 開始年月日（特殊）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SELECTED_START_DATE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SELECTED_START_DATE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SELECTED_START_DATE) = value
        End Set
    End Property
    ''' <summary>
    ''' 終了年月日（特殊）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SELECTED_END_DATE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SELECTED_END_DATE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SELECTED_END_DATE) = value
        End Set
    End Property
    ''' <summary>
    ''' メニューリスト表示リスト
    ''' </summary>
    ''' <returns></returns>
    Public Property UserMenuCostomList As List(Of UserMenuCostomItem)
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return DirectCast(SESSION(C_SESSION_KEY.USERMENU_COSTOM_LIST), List(Of UserMenuCostomItem))
        End Get

        Set(value As List(Of UserMenuCostomItem))
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.USERMENU_COSTOM_LIST) = value
        End Set
    End Property
    ''' <summary>
    ''' DBの接続情報を作成する
    ''' </summary>
    ''' <param name="connect"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getConnection(Optional ByRef connect As SqlConnection = Nothing) As SqlConnection
        'DataBase接続文字
        Dim SQLcon As New SqlConnection(DBCon)
        If Not IsNothing(connect) Then
            connect = SQLcon
        End If
        getConnection = SQLcon
    End Function


    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        'GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose(ByVal isDispose As Boolean)
        If isDispose Then

        End If
    End Sub
    ''' <summary>
    ''' ユーザーメニューのカスタマイズ
    ''' </summary>
    Public Class UserMenuCostomItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(outputId As String, onOff As String, sortNo As Integer)
            Me.OutputId = outputId
            If onOff = "1" Then
                Me.OnOff = True
            Else
                Me.OnOff = False
            End If

            Me.SortNo = sortNo
        End Sub

        ''' <summary>
        ''' 表示ID
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputId As String
        ''' <summary>
        ''' 表示非表示(True:表示,False:非表示)
        ''' </summary>
        ''' <returns></returns>
        Public Property OnOff As Boolean
        ''' <summary>
        ''' 並び順
        ''' </summary>
        ''' <returns></returns>
        Public Property SortNo As Integer
    End Class
End Class



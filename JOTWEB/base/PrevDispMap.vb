''' <summary>
''' 共通で利用する定数定義
''' </summary>
Public Module PrevDispMap

    ''' <summary>
    ''' 前画面の識別用名称の一覧
    ''' </summary>
    Public Class C_PREV_MAP_LIST
        ''' <summary>
        ''' ログイン画面
        ''' </summary>
        Public Const LOGIN As String = "ASP.M00000LOGON_ASPX"
        ''' <summary>
        ''' メニュー画面
        ''' </summary>
        Public Const MENU As String = "ASP.OIL_M00001MENU_ASPX"

        ''' メニュー画面(マスタメンテ）
        ''' </summary>
        Public Const MENU2 As String = "ASP.OIL_M00002MENU_ASPX"

        ''' <summary>
        ''' サブメニュー画面
        ''' </summary>
        Public Const SUBMENU As String = "ASP.OIL_M00002MENU_ASPX"
        ''' <summary>
        ''' 空回日報検索
        ''' </summary>
        Public Const OIT0001S As String = "ASP.OIL_ORD_OIT0001EMPTYTURNDAIRYSEARCH_ASPX"
        ''' <summary>
        ''' 空回日報一覧
        ''' </summary>
        Public Const OIT0001L As String = "ASP.OIL_ORD_OIT0001EMPTYTURNDAIRYLIST_ASPX"
        ''' <summary>
        ''' 空回日報明細
        ''' </summary>
        Public Const OIT0001D As String = "ASP.OIL_ORD_OIT0001EMPTYTURNDAIRYDETAIL_ASPX"
        ''' <summary>
        ''' 貨車連結順序表検索
        ''' </summary>
        Public Const OIT0002S As String = "ASP.OIL_ORD_OIT0002LINKSEARCH_ASPX"
        ''' <summary>
        ''' 貨車連結順序表一覧
        ''' </summary>
        Public Const OIT0002L As String = "ASP.OIL_ORD_OIT0002LINKLIST_ASPX"
        ''' <summary>
        ''' 貨車連結順序表明細
        ''' </summary>
        Public Const OIT0002D As String = "ASP.OIL_ORD_OIT0002LINKDETAIL_ASPX"
        ''' <summary>
        ''' 受注検索
        ''' </summary>
        Public Const OIT0003S As String = "ASP.OIL_ORD_OIT0003ORDERSEARCH_ASPX"
        ''' <summary>
        ''' 受注一覧
        ''' </summary>
        Public Const OIT0003L As String = "ASP.OIL_ORD_OIT0003ORDERLIST_ASPX"
        ''' <summary>
        ''' 受注明細
        ''' </summary>
        Public Const OIT0003D As String = "ASP.OIL_ORD_OIT0003ORDERDETAIL_ASPX"
        ''' <summary>
        ''' 在庫管理表検索
        ''' </summary>
        Public Const OIT0004S As String = "ASP.OIL_INV_OIT0004OILSTOCKSEARCH_ASPX"
        ''' <summary>
        ''' 在庫管理表登録
        ''' </summary>
        Public Const OIT0004C As String = "ASP.OIL_INV_OIT0004OILSTOCKCREATE_ASPX"
        ''' <summary>
        ''' ユーザIDマスタメンテナンス（検索）
        ''' </summary>
        Public Const OIS0001S As String = "ASP.OIL_MAS_OIS0001USERSEARCH_ASPX"
        ''' <summary>
        ''' ユーザIDマスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIS0001L As String = "ASP.OIL_MAS_OIS0001USERLIST_ASPX"
        ''' <summary>
        ''' ユーザIDマスタメンテナンス（登録）
        ''' </summary>
        Public Const OIS0001C As String = "ASP.OIL_MAS_OIS0001USERCREATE_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0004S As String = "ASP.OIL_MAS_OIM0004STATIONSEARCH_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0004L As String = "ASP.OIL_MAS_OIM0004STATIONLIST_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0004C As String = "ASP.OIL_MAS_OIM0004STATIONCREATE_ASPX"
        ''' <summary>
        ''' タンク車マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0005S As String = "ASP.OIL_MAS_OIM0005TANKSEARCH_ASPX"
        ''' <summary>
        ''' タンク車マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0005L As String = "ASP.OIL_MAS_OIM0005TANKLIST_ASPX"
        ''' <summary>
        ''' タンク車マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0005C As String = "ASP.OIL_MAS_OIM0005TANKCREATE_ASPX"
    End Class

End Module 'End BaseDllConst
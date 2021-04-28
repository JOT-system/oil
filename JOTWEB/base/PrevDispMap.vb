Option Strict On
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

        ''' <summary>
        ''' サブメニュー画面
        ''' </summary>
        Public Const SUBMENU As String = "ASP.OIL_M00001MENU_ASPX"
        'Public Const SUBMENU As String = "ASP.OIL_M00002MENU_ASPX"
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
        ''' タンク車所在管理検索
        ''' </summary>
        Public Const OIT0005S As String = "ASP.OIL_LOC_OIT0005TANKLOCSEARCH_ASPX"
        ''' <summary>
        ''' タンク車所在管理状況
        ''' </summary>
        Public Const OIT0005C As String = "ASP.OIL_LOC_OIT0005TANKLOCCONDITION_ASPX"
        ''' <summary>
        ''' タンク車所在管理一覧
        ''' </summary>
        Public Const OIT0005L As String = "ASP.OIL_LOC_OIT0005TANKLOCLIST_ASPX"
        ''' <summary>
        ''' タンク車所在登録
        ''' </summary>
        Public Const OIT0005D As String = "ASP.OIL_LOC_OIT0005TANKLOCDETAIL_ASPX"
        ''' <summary>
        ''' 回送検索
        ''' </summary>
        Public Const OIT0006S As String = "ASP.OIL_FOR_OIT0006OUTOFSERVICESEARCH_ASPX"
        ''' <summary>
        ''' 回送一覧
        ''' </summary>
        Public Const OIT0006L As String = "ASP.OIL_FOR_OIT0006OUTOFSERVICELIST_ASPX"
        ''' <summary>
        ''' 回送明細
        ''' </summary>
        Public Const OIT0006D As String = "ASP.OIL_FOR_OIT0006OUTOFSERVICEDETAIL_ASPX"
        ''' <summary>
        ''' 社外連携 取込検索
        ''' </summary>
        Public Const OIT0007S As String = "ASP.OIL_LOC_OIT0007FILEINPUTSEARCH_ASPX"
        ''' <summary>
        ''' 社外連携 取込一覧
        ''' </summary>
        Public Const OIT0007L As String = "ASP.OIL_LOC_OIT0007FILEINPUTLIST_ASPX"

        ''' <summary>
        ''' 費用管理
        ''' </summary>
        Public Const OIT0008M As String = "ASP.OIL_PAY_OIT0008COSTMANAGEMENT_ASPX"

        ''' <summary>
        ''' 費用管理明細表示
        ''' </summary>
        Public Const OIT0008D As String = "ASP.OIL_PAY_OIT0008COSTDETAIL_ASPX"

        ''' <summary>
        ''' 費用管理明細入力
        ''' </summary>
        Public Const OIT0008C As String = "ASP.OIL_PAY_OIT0008COSTDETAILCREATE_ASPX"

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
        ''' 会社マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0001S As String = "ASP.OIL_MAS_OIM0001CAMPSEARCH_ASPX"
        ''' <summary>
        ''' 会社マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0001L As String = "ASP.OIL_MAS_OIM0001CAMPLIST_ASPX"
        ''' <summary>
        ''' 会社マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0001C As String = "ASP.OIL_MAS_OIM0001CAMPCREATE_ASPX"

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
        ''' <summary>
        ''' 列車マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0007S As String = "ASP.OIL_MAS_OIM0007TRAINSEARCH_ASPX"
        ''' <summary>
        ''' 列車マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0007L As String = "ASP.OIL_MAS_OIM0007TRAINLIST_ASPX"
        ''' <summary>
        ''' 列車マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0007C As String = "ASP.OIL_MAS_OIM0007TRAINCREATE_ASPX"
        ''' <summary>
        ''' 基地マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0009S As String = "ASP.OIL_MAS_OIM0009PLANTSEARCH_ASPX"
        ''' <summary>
        ''' 基地マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0009L As String = "ASP.OIL_MAS_OIM0009PLANTLIST_ASPX"
        ''' <summary>
        ''' 基地マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0009C As String = "ASP.OIL_MAS_OIM0009PLANTCREATE_ASPX"
        ''' <summary>
        ''' 取引先マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0011S As String = "ASP.OIL_MAS_OIM0011TORISEARCH_ASPX"
        ''' <summary>
        ''' 取引先マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0011L As String = "ASP.OIL_MAS_OIM0011TORILIST_ASPX"
        ''' <summary>
        ''' 取引先マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0011C As String = "ASP.OIL_MAS_OIM0011TORICREATE_ASPX"
        ''' <summary>
        ''' 荷受人マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0012S As String = "ASP.OIL_MAS_OIM0012NIUKESEARCH_ASPX"
        ''' <summary>
        ''' 荷受人マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0012L As String = "ASP.OIL_MAS_OIM0012NIUKELIST_ASPX"
        ''' <summary>
        ''' 荷受人マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0012C As String = "ASP.OIL_MAS_OIM0012NIUKECREATE_ASPX"
        ''' <summary>
        ''' 積込スペックマスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0013S As String = "ASP.OIL_MAS_OIM0013LOADSEARCH_ASPX"
        ''' <summary>
        ''' 積込スペックマスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0013L As String = "ASP.OIL_MAS_OIM0013LOADLIST_ASPX"
        ''' <summary>
        ''' 積込スペックマスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0013C As String = "ASP.OIL_MAS_OIM0013LOADCREATE_ASPX"
        ''' <summary>
        ''' 積込可能車数マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0014S As String = "ASP.OIL_MAS_OIM0014LOADCALCSEARCH_ASPX"
        ''' <summary>
        ''' 積込可能車数マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0014L As String = "ASP.OIL_MAS_OIM0014LOADCALCLIST_ASPX"
        ''' <summary>
        ''' 積込可能車数マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0014C As String = "ASP.OIL_MAS_OIM0014LOADCALCCREATE_ASPX"
        ''' <summary>
        ''' 油槽所諸元マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0015S As String = "ASP.OIL_MAS_OIM0015SYOGENSEARCH_ASPX"
        ''' <summary>
        ''' 油槽所諸元マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0015L As String = "ASP.OIL_MAS_OIM0015SYOGENLIST_ASPX"
        ''' <summary>
        ''' 油槽所諸元マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0015C As String = "ASP.OIL_MAS_OIM0015SYOGENCREATE_ASPX"
        ''' <summary>
        ''' 列車マスタ (臨海)メンテナンス（検索）
        ''' </summary>
        Public Const OIM0016S As String = "ASP.OIL_MAS_OIM0016RTRAINSEARCH_ASPX"
        ''' <summary>
        ''' 列車マスタ (臨海)メンテナンス（一覧）
        ''' </summary>
        Public Const OIM0016L As String = "ASP.OIL_MAS_OIM0016RTRAINLIST_ASPX"
        ''' <summary>
        ''' 列車マスタ (臨海)メンテナンス（登録）
        ''' </summary>
        Public Const OIM0016C As String = "ASP.OIL_MAS_OIM0016RTRAINCREATE_ASPX"
        ''' <summary>
        ''' 列車運行管理マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0017S As String = "ASP.OIL_MAS_OIM0017TRAINOPERATIONSEARCH_ASPX"
        ''' <summary>
        ''' 列車運行管理マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0017L As String = "ASP.OIL_MAS_OIM0017TRAINOPERATIONLIST_ASPX"
        ''' <summary>
        ''' 列車運行管理マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0017C As String = "ASP.OIL_MAS_OIM0017TRAINOPERATIONCREATE_ASPX"
        ''' <summary>
        ''' 勘定科目マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0019S As String = "ASP.OIL_MAS_OIM0019ACCOUNTSEARCH_ASPX"
        ''' <summary>
        ''' 勘定科目マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0019L As String = "ASP.OIL_MAS_OIM0019ACCOUNTLIST_ASPX"
        ''' <summary>
        ''' 勘定科目マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0019C As String = "ASP.OIL_MAS_OIM0019ACCOUNTCREATE_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0020S As String = "ASP.OIL_MAS_OIM0020GUIDANCESEARCH_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0020L As String = "ASP.OIL_MAS_OIM0020GUIDANCELIST_ASPX"
        ''' <summary>
        ''' ガイダンスマスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0020C As String = "ASP.OIL_MAS_OIM0020GUIDANCECREATE_ASPX"
        ''' <summary>
        ''' 積込予約マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0021S As String = "ASP.OIL_MAS_OIM0021LOADRESERVESEARCH_ASPX"
        ''' <summary>
        ''' 積込予約マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0021L As String = "ASP.OIL_MAS_OIM0021LOADRESERVELIST_ASPX"
        ''' <summary>
        ''' 積込予約マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0021C As String = "ASP.OIL_MAS_OIM0021LOADRESERVECREATE_ASPX"
        ''' <summary>
        ''' 列車マスタ (返送)メンテナンス（検索）
        ''' </summary>
        Public Const OIM0023S As String = "ASP.OIL_MAS_OIM0023BTRAINSEARCH_ASPX"
        ''' <summary>
        ''' 列車マスタ (返送)メンテナンス（一覧）
        ''' </summary>
        Public Const OIM0023L As String = "ASP.OIL_MAS_OIM0023BTRAINLIST_ASPX"
        ''' <summary>
        ''' 列車マスタ (返送)メンテナンス（登録）
        ''' </summary>
        Public Const OIM0023C As String = "ASP.OIL_MAS_OIM0023BTRAINCREATE_ASPX"
        ''' <summary>
        ''' 積込優先油種マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0024S As String = "ASP.OIL_MAS_OIM0024PRIORITYSEARCH_ASPX"
        ''' <summary>
        ''' 積込優先油種マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0024L As String = "ASP.OIL_MAS_OIM0024PRIORITYLIST_ASPX"
        ''' <summary>
        ''' 積込優先油種マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0024C As String = "ASP.OIL_MAS_OIM0024PRIORITYCREATE_ASPX"
        ''' <summary>
        ''' 組織マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0002S As String = "ASP.OIL_MAS_OIM0002ORGSEARCH_ASPX"
        ''' <summary>
        ''' 組織マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0002L As String = "ASP.OIL_MAS_OIM0002ORGLIST_ASPX"
        ''' <summary>
        ''' 組織マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0002C As String = "ASP.OIL_MAS_OIM0002ORGCREATE_ASPX"
        ''' <summary>
        ''' 品種マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0003S As String = "ASP.OIL_MAS_OIM0003PRODUCTSEARCH_ASPX"
        ''' <summary>
        ''' 品種マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0003L As String = "ASP.OIL_MAS_OIM0003PRODUCTLIST_ASPX"
        ''' <summary>
        ''' 品種マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0003C As String = "ASP.OIL_MAS_OIM0003PRODUCTCREATE_ASPX"

    End Class

End Module 'End BaseDllConst
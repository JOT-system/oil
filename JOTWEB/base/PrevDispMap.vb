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
        ''' ユーザIDマスタ入力（条件）
        ''' </summary>
        Public Const CO0004S As String = "ASP.GR_GRCO0004SELECT_ASPX"
        ''' <summary>
        ''' ユーザIDマスタ入力（実行）
        ''' </summary>
        Public Const CO0004 As String = "ASP.GR_GRCO0004USER_ASPX"
        ''' <summary>
        ''' ロールマスタ入力（条件）
        ''' </summary>
        Public Const CO0006S As String = "ASP.GR_GRCO0006SELECT_ASPX"
        ''' <summary>
        ''' ロールマスタ入力（実行）
        ''' </summary>
        Public Const CO0006 As String = "ASP.GR_GRCO0006ROLE_ASPX"
        ''' <summary>
        ''' 変数入力（条件）
        ''' </summary>
        Public Const CO0007S As String = "ASP.GR_GRCO0007SELECT_ASPX"
        ''' <summary>
        ''' 変数入力（実行）
        ''' </summary>
        Public Const CO0007 As String = "ASP.GR_GRCO0007PROFMVARI_ASPX"
        ''' <summary>
        ''' メニュー入力（条件）
        ''' </summary>
        Public Const CO0008S As String = "ASP.GR_GRCO0008SELECT_ASPX"
        ''' <summary>
        ''' メニュー入力（実行）
        ''' </summary>
        Public Const CO0008 As String = "ASP.GR_GRCO0008PROFMMAP_ASPX"
        ''' <summary>
        ''' メニュー入力（条件）
        ''' </summary>
        Public Const CO0010S As String = "ASP.GR_GRCO0010SELECT_ASPX"
        ''' <summary>
        ''' メニュー入力（実行）
        ''' </summary>
        Public Const CO0010 As String = "ASP.GR_GRCO0010PROFMVIEW_ASPX"
        ''' <summary>
        ''' 帳票入力（条件）
        ''' </summary>
        Public Const CO0011S As String = "ASP.GR_GRCO0011SELECT_ASPX"
        ''' <summary>
        ''' 帳票入力（実行）
        ''' </summary>
        Public Const CO0011 As String = "ASP.GR_GRCO0011PROFMXLS_ASPX"
        ''' <summary>
        ''' サーバ権限マスタ入力（条件）
        ''' </summary>
        Public Const CO0012S As String = "ASP.GR_GRCO0012SELECT_ASPX"
        ''' <summary>
        ''' サーバ権限マスタ入力（実行）
        ''' </summary>
        Public Const CO0012 As String = "ASP.GR_GRCO0012SRVAUTHOR_ASPX"
        ''' <summary>
        ''' パスワード登録
        ''' </summary>
        Public Const CO0014 As String = "ASP.GR_GRCO0014USERPASS_ASPX"
        ''' <summary>
        ''' 運用ガイダンス登録
        ''' </summary>
        Public Const CO0015 As String = "ASP.GR_GRCO0015ONLINESTAT_ASPX"
        ''' <summary>
        ''' EXCEL書式登録(条件)
        ''' </summary>
        Public Const CO0102S As String = "ASP.GR_GRCO0102SELECT_ASPX"
        ''' <summary>
        ''' EXCEL書式登録(実行)
        ''' </summary>
        Public Const CO0102 As String = "ASP.GR_GRCO0102XLSFORM_ASPX"
        ''' <summary>
        ''' ライブラリ配布
        ''' </summary>
        Public Const CO0106 As String = "ASP.GR_GRCO0106LIBSEND_ASPX"
        ''' <summary>
        ''' 組織マスタ入力（条件）
        ''' </summary>
        Public Const M00002S As String = "ASP.GR_GRM00002SELECT_ASPX"
        ''' <summary>
        ''' 組織マスタ入力（実行）
        ''' </summary>
        Public Const M00002 As String = "ASP.GR_GRM00002ORG_ASPX"
        ''' <summary>
        ''' 構造マスタ入力（条件）
        ''' </summary>
        Public Const M00006S As String = "ASP.GR_GRM00006SELECT_ASPX"
        ''' <summary>
        ''' 構造マスタ入力（実行）
        ''' </summary>
        Public Const M00006 As String = "ASP.GR_GRM00006STRUCT_ASPX"
        ''' <summary>
        ''' 車両台帳・車両管理マスタ登録（条件）
        ''' </summary>
        Public Const MA0002S As String = "ASP.GR_GRMA0002SELECT_ASPX"
        ''' <summary>
        ''' 車両台帳・車両管理マスタ登録（入力）
        ''' </summary>
        Public Const MA0002 As String = "ASP.GR_GRMA0002SHARYOA_ASPX"
        ''' <summary>
        ''' 車両台帳・車両基本マスタ登録（条件）
        ''' </summary>
        Public Const MA0003S As String = "ASP.GR_GRMA0003SELECT_ASPX"
        ''' <summary>
        ''' 車両台帳・車両基本マスタ登録（入力）
        ''' </summary>
        Public Const MA0003 As String = "ASP.GR_GRMA0003SHARYOB_ASPX"
        ''' <summary>
        ''' 車両台帳・申請登録（条件）
        ''' </summary>
        Public Const MA0004S As String = "ASP.GR_GRMA0004SELECT_ASPX"
        ''' <summary>
        ''' 車両台帳・申請登録（入力）
        ''' </summary>
        Public Const MA0004 As String = "ASP.GR_GRMA0004SHARYOC_ASPX"
        ''' <summary>
        ''' 車両台帳・車番部署マスタ登録（条件）
        ''' </summary>
        Public Const MA0006S As String = "ASP.GR_GRMA0006SELECT_ASPX"
        ''' <summary>
        ''' 車両台帳・車番部署マスタ登録（入力）
        ''' </summary>
        Public Const MA0006 As String = "ASP.GR_GRMA0006SHABANORG_ASPX"
        ''' <summary>
        ''' 従業員マスタ登録（条件）
        ''' </summary>
        Public Const MB0001S As String = "ASP.GR_GRMB0001SELECT_ASPX"
        ''' <summary>
        ''' 従業員マスタ登録（入力）
        ''' </summary>
        Public Const MB0001 As String = "ASP.GR_GRMB0001STAFF_ASPX"
        ''' <summary>
        ''' 従業員部署マスタ登録（条件）
        ''' </summary>
        Public Const MB0002S As String = "ASP.GR_GRMB0002SELECT_ASPX"
        ''' <summary>
        ''' 従業員部署マスタ登録（入力）
        ''' </summary>
        Public Const MB0002 As String = "ASP.GR_GRMB0002STAFFORG_ASPX"
        ''' <summary>
        ''' 乗務員休日予定登録（条件）
        ''' </summary>
        Public Const MB0003S As String = "ASP.GR_GRMB0003SELECT_ASPX"
        ''' <summary>
        ''' 乗務員休日予定登録（実行）
        ''' </summary>
        Public Const MB0003 As String = "ASP.GR_GRMB0003HSTAFF_ASPX"
        ''' <summary>
        ''' 所定労働時間登録（条件）
        ''' </summary>
        Public Const MB0004S As String = "ASP.GR_GRMB0004SELECT_ASPX"
        ''' <summary>
        ''' 所定労働時間登録（実行）
        ''' </summary>
        Public Const MB0004 As String = "ASP.GR_GRMB0004WORKINGH_ASPX"
        ''' <summary>
        ''' カレンダー（条件）
        ''' </summary>
        Public Const MB0005S As String = "ASP.GR_GRMB0005SELECT_ASPX"
        ''' <summary>
        ''' カレンダー（入力）
        ''' </summary>
        Public Const MB0005 As String = "ASP.GR_GRMB0005CALENDAR_ASPX"
        ''' <summary>
        ''' 従業員マスタ登録(傭車)（条件）
        ''' </summary>
        Public Const MB0007S As String = "ASP.GR_GRMB0007SELECT_ASPX"
        ''' <summary>
        ''' 従業員マスタ登録(傭車)（入力）
        ''' </summary>
        Public Const MB0007 As String = "ASP.GR_GRMB0007STAFF_ASPX"
        ''' <summary>
        ''' 固定値マスタ入力（条件）
        ''' </summary>
        Public Const MC0001S As String = "ASP.GR_GRMC0001SELECT_ASPX"
        ''' <summary>
        ''' 固定値マスタ入力（実行）
        ''' </summary>
        Public Const MC0001 As String = "ASP.GR_GRMC0001FIXVALUE_ASPX"
        ''' <summary>
        ''' 取引先マスタ入力（条件）
        ''' </summary>
        Public Const MC0002S As String = "ASP.GR_GRMC0002SELECT_ASPX"
        ''' <summary>
        ''' 取引先マスタ入力（実行）
        ''' </summary>
        Public Const MC0002 As String = "ASP.GR_GRMC0002TORIHIKISAKI_ASPX"
        ''' <summary>
        ''' 取引先部署マスタ入力（条件）
        ''' </summary>
        Public Const MC0003S As String = "ASP.GR_GRMC0003SELECT_ASPX"
        ''' <summary>
        ''' 取引先部署マスタ入力（実行）
        ''' </summary>
        Public Const MC0003 As String = "ASP.GR_GRMC0003TORIORG_ASPX"
        ''' <summary>
        ''' 品名マスタ（条件）
        ''' </summary>
        Public Const MC0004S As String = "ASP.GR_GRMC0004SELECT_ASPX"
        ''' <summary>
        ''' 品名マスタ（実行）
        ''' </summary>
        Public Const MC0004 As String = "ASP.GR_GRMC0004PRODUCT_ASPX"
        ''' <summary>
        ''' 品名部署マスタ登録（条件）
        ''' </summary>
        Public Const MC0005S As String = "ASP.GR_GRMC0005SELECT_ASPX"
        ''' <summary>
        ''' 品名部署マスタ登録（実行）
        ''' </summary>
        Public Const MC0005 As String = "ASP.GR_GRMC0005PRODORG_ASPX"
        ''' <summary>
        ''' 届先マスタ入力（条件）
        ''' </summary>
        Public Const MC0006S As String = "ASP.GR_GRMC0006SELECT_ASPX"
        ''' <summary>
        ''' 届先マスタ入力（実行）
        ''' </summary>
        Public Const MC0006 As String = "ASP.GR_GRMC0006TODOKESAKI_ASPX"
        ''' <summary>
        ''' 届先マスタ照会（条件）
        ''' </summary>
        Public Const MC0006S_R As String = "ASP.GR_GRMC0006SELECT_R_ASPX"
        ''' <summary>
        ''' 届先マスタ照会（実行）
        ''' </summary>
        Public Const MC0006_R As String = "ASP.GR_GRMC0006TODOKESAKI_R_ASPX"
        ''' <summary>
        ''' 届先マスタ照会（JX,COSMO）（条件）
        ''' </summary>
        Public Const MC0006S_JC As String = "ASP.GR_GRMC0006SELECT_JC_ASPX"
        ''' <summary>
        ''' 届先マスタ照会（JX,COSMO）（実行）
        ''' </summary>
        Public Const MC0006_JC As String = "ASP.GR_GRMC0006TODOKESAKI_JC_ASPX"
        ''' <summary>
        ''' 届先部署マスタ入力（条件）
        ''' </summary>
        Public Const MC0007S As String = "ASP.GR_GRMC0007SELECT_ASPX"
        ''' <summary>
        ''' 届先部署マスタ入力（実行）
        ''' </summary>
        Public Const MC0007 As String = "ASP.GR_GRMC0007TODKORG_ASPX"
        ''' <summary>
        ''' 受注集計条件入力（条件）
        ''' </summary>
        Public Const MC0010S As String = "ASP.GR_GRMC0010SELECT_ASPX"
        ''' <summary>
        ''' 受注集計条件入力（実行）
        ''' </summary>
        Public Const MC0010 As String = "ASP.GR_GRMC0010T3CNTL_ASPX"
        ''' <summary>
        ''' 出荷地・届先別モデル距離（条件）
        ''' </summary>
        Public Const MC0012S As String = "ASP.GR_GRMC0012SELECT_ASPX"
        ''' <summary>
        ''' 出荷地・届先別モデル距離（実行）
        ''' </summary>
        Public Const MC0012 As String = "ASP.GR_GRMC0012MODEL_ASPX"
        ''' <summary>
        ''' 品名マスタ（条件）
        ''' </summary>
        Public Const MD0001S As String = "ASP.GR_GRMD0001SELECT_ASPX"
        ''' <summary>
        ''' 品名マスタ（実行）
        ''' </summary>
        Public Const MD0001 As String = "ASP.GR_GRMD0001PRODUCT_ASPX"
        ''' <summary>
        ''' 品名部署マスタ登録（条件）
        ''' </summary>
        Public Const MD0002S As String = "ASP.GR_GRMD0002SELECT_ASPX"
        ''' <summary>
        ''' 品名部署マスタ登録（実行）
        ''' </summary>
        Public Const MD0002 As String = "ASP.GR_GRMD0002PRODORG_ASPX"

        ''' <summary>
        ''' 荷主受注登録（条件）
        ''' </summary>
        Public Const T00003S As String = "ASP.GR_GRT00003SELECT_ASPX"
        ''' <summary>
        ''' 荷主受注登録（実行）
        ''' </summary>
        Public Const T00003 As String = "ASP.GR_GRT00003NIORDER_ASPX"
        ''' <summary>
        ''' 配送受注（条件）
        ''' </summary>
        Public Const T00004S As String = "ASP.GR_GRT00004SELECT_ASPX"
        ''' <summary>
        ''' 配送受注（実行）
        ''' </summary>
        Public Const T00004 As String = "ASP.GR_GRT00004HORDER_ASPX"
        ''' <summary>
        ''' 運転日報取込（条件）
        ''' </summary>
        Public Const T00005S As String = "ASP.GR_GRT00005SELECT_ASPX"
        ''' <summary>
        ''' 運転日報取込
        ''' </summary>
        Public Const T00005I As String = "ASP.GR_GRT00005IMPORT_ASPX"
        ''' <summary>
        ''' 運転日報訂正
        ''' </summary>
        Public Const T00005 As String = "ASP.GR_GRT00005NIPPO_ASPX"
        ''' <summary>
        ''' 車端ファイル作成（条件）
        ''' </summary>
        Public Const T00006S As String = "ASP.GR_GRT00006SELECT_ASPX"
        ''' <summary>
        ''' 車端ファイル作成（実行）
        ''' </summary>
        Public Const T00006 As String = "ASP.GR_GRT00006EXPORT_ASPX"
        ''' <summary>
        ''' 営業勤務入力（条件）
        ''' </summary>
        Public Const T00007S As String = "ASP.GR_GRT00007SELECT_ASPX"
        ''' <summary>
        ''' 営業勤務入力（一覧）
        ''' </summary>
        Public Const T00007I As String = "ASP.GR_GRT00007ICHIRAN_ASPX"
        ''' <summary>
        ''' 営業勤務入力（一覧）（NJS）
        ''' </summary>
        Public Const T00007INJS As String = "ASP.GR_GRT00007ICHIRAN_NJS_ASPX"
        ''' <summary>
        ''' 営業勤務入力（一覧）（KNK）
        ''' </summary>
        Public Const T00007IKNK As String = "ASP.GR_GRT00007ICHIRAN_KNK_ASPX"
        ''' <summary>
        ''' 営業勤務入力（一覧）（JKT）
        ''' </summary>
        Public Const T00007IJKT As String = "ASP.GR_GRT00007ICHIRAN_JKT_ASPX"
        ''' <summary>
        ''' 営業勤務入力（実行）
        ''' </summary>
        Public Const T00007 As String = "ASP.GR_GRT00007KINTAI_ASPX"
        ''' <summary>
        ''' 営業勤務入力（実行）（NJS）
        ''' </summary>
        Public Const T00007NJS As String = "ASP.GR_GRT00007KINTAI_NJS_ASPX"
        ''' <summary>
        ''' 営業勤務入力（実行）（KNK）
        ''' </summary>
        Public Const T00007KNK As String = "ASP.GR_GRT00007KINTAI_KNK_ASPX"
        ''' <summary>
        ''' 営業勤務入力（実行）（JKT）
        ''' </summary>
        Public Const T00007JKT As String = "ASP.GR_GRT00007KINTAI_JKT_ASPX"
        ''' <summary>
        ''' 営業勤務締（条件）
        ''' </summary>
        Public Const T00008S As String = "ASP.GR_GRT00008SELECT_ASPX"
        ''' <summary>
        ''' 営業勤務締（実行）
        ''' </summary>
        Public Const T00008 As String = "ASP.GR_GRT00008KINTAISTAT_ASPX"
        ''' <summary>
        ''' 事務員勤務入力（条件）
        ''' </summary>
        Public Const T00009S As String = "ASP.GR_GRT00009SELECT_ASPX"
        ''' <summary>
        ''' 事務員勤務入力（実行）
        ''' </summary>
        Public Const T00009 As String = "ASP.GR_GRT00009JIMKINTAI_ASPX"
        ''' <summary>
        ''' 承認（条件）
        ''' </summary>
        Public Const T00010S As String = "ASP.GR_GRT00010SELECT_ASPX"
        ''' <summary>
        ''' 承認（実行）
        ''' </summary>
        Public Const T00010 As String = "ASP.GR_GRT00010APPROVE_ASPX"
        ''' <summary>
        ''' 実績数量修正（条件）
        ''' </summary>
        Public Const T00011S As String = "ASP.GR_GRT00011SELECT_ASPX"
        ''' <summary>
        ''' 実績数量修正（実行）
        ''' </summary>
        Public Const T00011 As String = "ASP.GR_GRT00011ACTUALQTY_ASPX"
        ''' <summary>
        ''' 配送照会（条件）
        ''' </summary>
        Public Const TA0001S As String = "ASP.GR_GRTA0001SELECT_ASPX"
        ''' <summary>
        ''' 配送照会（実行）
        ''' </summary>
        Public Const TA0001 As String = "ASP.GR_GRTA0001HAISHA_ASPX"
        ''' <summary>
        ''' 勤務状況リスト（条件）
        ''' </summary>
        Public Const TA0002S As String = "ASP.GR_GRTA0002SELECT_ASPX"
        ''' <summary>
        ''' 勤務状況リスト（実行）
        ''' </summary>
        Public Const TA0002 As String = "ASP.GR_GRTA0002KINTAILIST_ASPX"
        ''' <summary>
        ''' 給与ジャーナル一覧（条件）
        ''' </summary>
        Public Const TA0003S As String = "ASP.GR_GRTA0003SELECT_ASPX"
        ''' <summary>
        ''' 給与ジャーナル一覧（実行）
        ''' </summary>
        Public Const TA0003 As String = "ASP.GR_GRTA0003KYUYOLIST_ASPX"
        ''' <summary>
        ''' 統計DBレポート(日報)
        ''' </summary>
        Public Const TA0004S As String = "ASP.GR_GRTA0004SELECT_ASPX"
        ''' <summary>
        ''' 統計DBレポート(日報)
        ''' </summary>
        Public Const TA0004 As String = "ASP.GR_GRTA0004LMNIPPO_ASPX"
        ''' <summary>
        ''' 実績レポート(勤怠)
        ''' </summary>
        Public Const TA0005S As String = "ASP.GR_GRTA0005SELECT_ASPX"
        ''' <summary>
        ''' 実績レポート(勤怠)
        ''' </summary>
        Public Const TA0005 As String = "ASP.GR_GRTA0005LMKINTAI_ASPX"
        ''' <summary>
        ''' 事務員勤務状況リスト（条件）
        ''' </summary>
        Public Const TA0006S As String = "ASP.GR_GRTA0006SELECT_ASPX"
        ''' <summary>
        ''' 事務員勤務状況リスト（実行）
        ''' </summary>
        Public Const TA0006 As String = "ASP.GR_GRTA0006JIMKINTAILIST_ASPX"
        ''' <summary>
        ''' 事務員給与ジャーナル一覧（条件）
        ''' </summary>
        Public Const TA0007S As String = "ASP.GR_GRTA0007SELECT_ASPX"
        ''' <summary>
        ''' 事務員給与ジャーナル一覧（実行）
        ''' </summary>
        Public Const TA0007 As String = "ASP.GR_GRTA0007JIMKYUYOLIST_ASPX"
        ''' <summary>
        ''' 実績レポート(応援者勤怠)
        ''' </summary>
        Public Const TA0008S As String = "ASP.GR_GRTA0008SELECT_ASPX"
        ''' <summary>
        ''' 実績レポート(応援者勤怠)
        ''' </summary>
        Public Const TA0008 As String = "ASP.GR_GRTA0008LMKINTAISPPT_ASPX"
        ''' <summary>
        ''' 実績レポート(乗務員勤怠)
        ''' </summary>
        Public Const TA0009S As String = "ASP.GR_GRTA0009SELECT_ASPX"
        ''' <summary>
        ''' 実績レポート(乗務員勤怠)
        ''' </summary>
        Public Const TA0009 As String = "ASP.GR_GRTA0009LMKINTAICREW_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（検索）
        ''' </summary>
        Public Const OIM0004S As String = "ASP.OIL_OIM0004STATIONSEARCH_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（一覧）
        ''' </summary>
        Public Const OIM0004L As String = "ASP.OIL_OIM0004STATIONLIST_ASPX"
        ''' <summary>
        ''' 貨物駅マスタメンテナンス（登録）
        ''' </summary>
        Public Const OIM0004C As String = "ASP.OIL_OIM0004STATIONCREATE_ASPX"
        ''' <summary>
        ''' タンク車マスタ（条件）
        ''' </summary>
        Public Const OIM0005S As String = "ASP.OIL_OIM0005TankSearch_ASPX"
        ''' <summary>
        ''' タンク車マスタ（実行）
        ''' </summary>
        Public Const OIM0005L As String = "ASP.OIL_OIM0005TankList_ASPX"
    End Class

End Module 'End BaseDllConst
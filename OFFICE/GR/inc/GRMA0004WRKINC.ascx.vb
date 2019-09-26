Imports OFFICE.GRIS0005LeftBox

Public Class GRMA0004WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "MA0004S"       'MAPID(選択)
    Public Const MAPID As String = "MA0004"         'MAPID(実行)

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXNUM"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, ByVal FIXNUM As Integer) As Hashtable
        Dim FIXCODE As String = ""
        Select Case FIXNUM
            Case 901 : FIXCODE = "MANGOWNCONT"  ' 契約区分
            Case 902 : FIXCODE = "BASELEASE"    ' 車両所有
            Case 903 : FIXCODE = "FCTRAXLE"     ' リフトアクスル
            Case 904 : FIXCODE = "FCTRTMAKER"   ' タンクメーカー
            Case 905 : FIXCODE = "FCTRDPR"      ' DPR
            Case 906 : FIXCODE = "FCTRFUELMATE" ' 燃料タンク材質
            Case 907 : FIXCODE = "FCTRSHFTNUM"  ' 軸数
            Case 908 : FIXCODE = "FCTRSUSP"     ' サスペンション種類
            Case 909 : FIXCODE = "FCTRTMISSION" ' ミッション
            Case 910 : FIXCODE = "SHARYOTYPE"   ' 車両タイプ
            Case 911 : FIXCODE = "FCTRUREA"     ' 尿素
            Case 912 : FIXCODE = "OTNKBPIPE"    ' 後配管
            Case 913 : FIXCODE = "OTNKCVALVE"   ' 中間ﾊﾞﾙﾌﾞ有無
            Case 914 : FIXCODE = "OTNKDCD"      ' DCD装備
            Case 915 : FIXCODE = "FCTRSMAKER"   ' 車両メーカー
            Case 916 : FIXCODE = "OTNKDETECTOR" ' 検水管
            Case 917 : FIXCODE = "OTNKDISGORGE" ' 吐出口
            Case 918 : FIXCODE = "OTNKHTECH"    ' ハイテク種別
            Case 919 : FIXCODE = "OTNKLVALVE"   ' 底弁形式
            Case 920 : FIXCODE = "OTNKMATERIAL" ' タンク材質
            Case 921 : FIXCODE = "OTNKPIPE"     ' 配管形態名
            Case 922 : FIXCODE = "OTNKPIPESIZE" ' 配管サイズ
            Case 923 : FIXCODE = "OTNKPUMP"     ' ポンプ
            Case 924 : FIXCODE = "HPRSPMPDR"    ' ポンプ駆動
            Case 925 : FIXCODE = "OTNKVAPOR"    ' ベーパー
            Case 926 : FIXCODE = "CHEMDISGORGE" ' 吐出口
            Case 927 : FIXCODE = "CHEMHOSE"     ' ホースボックス
            Case 928 : FIXCODE = "CHEMMANOMTR"  ' 圧力計
            Case 929 : FIXCODE = "CHEMMATERIAL" ' タンク材質
            Case 930 : FIXCODE = "CHEMPMPDR"    ' ポンプ駆動方法
            Case 931 : FIXCODE = "CHEMPRESDRV"  ' 加温装置
            Case 932 : FIXCODE = "CHEMPRESEQ"   ' 均圧配管
            Case 933 : FIXCODE = "CHEMPUMP"     ' ポンプ
            Case 934 : FIXCODE = "CHEMSTRUCT"   ' タンク構造
            Case 935 : FIXCODE = "CHEMTHERM"    ' 温度計
            Case 936 : FIXCODE = "HPRSINSULATE" ' 断熱構造
            Case 937 : FIXCODE = "HPRSMATR"     ' タンク材質
            Case 938 : FIXCODE = "HPRSPIPE"     ' 配管形状（仮）
            Case 939 : FIXCODE = "HPRSPIPENUM"  ' 配管口数
            Case 940 : FIXCODE = "HPRSPUMP"     ' ポンプ
            Case 941 : FIXCODE = "HPRSRESSRE"   ' 加圧器
            Case 942 : FIXCODE = "HPRSSTRUCT"   ' タンク構造
            Case 943 : FIXCODE = "HPRSVALVE"    ' 底弁形式
            Case 944 : FIXCODE = "OTHRBMONITOR" ' バックモニター
            Case 945 : FIXCODE = "OTHRBSONAR"   ' バックソナー
            Case 946 : FIXCODE = "FCTRTIRE"     ' タイヤメーカー
            Case 947 : FIXCODE = "OTHRDRRECORD" ' ﾄﾞﾗｲﾌﾞﾚｺｰﾀﾞｰ
            Case 948 : FIXCODE = "OTHRPAINTING" ' 塗装
            Case 949 : FIXCODE = "OTHRRADIOCON" ' 無線（有・無）
            Case 950 : FIXCODE = "OTHRRTARGET"  ' 一括修理非対象車
            Case 951 : FIXCODE = "OTHRTERMINAL" ' 車載端末
            Case 952 : FIXCODE = "LICNPLTNO1"   ' 登録番号(陸運局)
            Case 953 : FIXCODE = "OTNKEXHASIZE" ' 吐出口サイズ
            Case 954 : FIXCODE = "HPRSHOSE"     ' ホースボックス
            Case 955 : FIXCODE = "CONTSHAPE"    ' シャーシ形状
            Case 956 : FIXCODE = "CONTPUMP"     ' ポンプ
            Case 957 : FIXCODE = "CONTPMPDR"    ' ポンプ駆動方法
            Case 958 : FIXCODE = "OTHRTPMS"     ' TPMS
            Case 959 : FIXCODE = "OTNKTMAKER"   ' 石油タンクメーカー
            Case 960 : FIXCODE = "HPRSTMAKER"   ' 高圧タンクメーカー
            Case 961 : FIXCODE = "CHEMTMAKER"   ' 化成品タンクメーカー
            Case 962 : FIXCODE = "CONTTMAKER"   ' コンテナタンクメーカー
            Case 963 : FIXCODE = "SHARYOSTATUS" ' 運行状況
            Case 964 : FIXCODE = "INSKBN"       ' 検査区分
        End Select
        CreateFIXParam = CreateFIXParam(COMPCODE, FIXCODE)
    End Function

    ''' <summary>
    ''' 届先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="ORGCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateTODOParam(ByVal COMPCODE As String, Optional ByVal ORGCODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.OWNER
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        CreateTODOParam = prmData
    End Function

    ''' <summary>
    ''' 届先一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="ORGCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateYOTORIParam(ByVal COMPCODE As String, Optional ByVal ORGCODE As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG) = ORGCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.CARRIDE
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        CreateYOTORIParam = prmData
    End Function

    ''' <summary>
    ''' 品名一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="OILTYPE"></param>
    ''' <param name="GOODS1"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateGoodsParam(ByVal COMPCODE As String, ByVal OILTYPE As String, Optional ByVal GOODS1 As String = Nothing) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_OILTYPE) = OILTYPE
        If IsNothing(GOODS1) Then
            prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS1_MST
        Else
            prmData.Item(C_PARAMETERS.LP_PRODCODE1) = GOODS1
            prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0006GoodsList.LC_GOODS_TYPE.GOODS2_MST
        End If

        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        CreateGoodsParam = prmData
    End Function

    ''' <summary>
    ''' 部署一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="ISMANAGE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateORGParam(ByVal COMPCODE As String, ByVal ISMANAGE As Boolean) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        If ISMANAGE Then
            prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.DEPARTMENT, GL0002OrgList.C_CATEGORY_LIST.BRANCH_OFFICE}

        Else
            prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {GL0002OrgList.C_CATEGORY_LIST.CARAGE}
        End If
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.USER
        CreateORGParam = prmData
    End Function

End Class

Imports System.Data.SqlClient

''' <summary>
''' 統計DB出力
''' </summary>
''' <remarks></remarks>
Public Structure CS0044L1INSERT
    ''' <summary>
    ''' DBコネクション
    ''' </summary>
    ''' <value>DBコネクション</value>
    ''' <returns>DBコネクション</returns>
    ''' <remarks></remarks>
    Public Property SQLCON() As SqlConnection
    ''' <summary>
    ''' トランザクション
    ''' </summary>
    ''' <value>トランザクション</value>
    ''' <returns>トランザクション</returns>
    ''' <remarks></remarks>
    Public Property SQLTRN() As SqlTransaction
    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value>エラーコード</value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String
    ''' <summary>
    ''' 登録する統計情報DB名
    ''' </summary>
    ''' <remarks></remarks>
    Private Const INSERT_TABLE_NAME As String = "L0001_TOKEI"
    ''' <summary>
    ''' 一時登録するテンポラリテーブル名
    ''' </summary>
    ''' <remarks></remarks>
    Private Const INSERT_TEMP_TABLE_NAME As String = "#myTemp"
    ''' <summary>
    ''' 統計情報登録用テーブルデータを初期化する
    ''' </summary>
    ''' <param name="I_TBL">統計情報登録に使用するテーブルデータ</param>
    ''' <remarks></remarks>
    Public Sub CS0044L1ColmnsAdd(ByRef I_TBL As DataTable)
        'データが存在する場合初期化する
        If I_TBL.Columns.Count <> 0 Then
            I_TBL.Columns.Clear()
        End If

        'L0001DB項目作成
        I_TBL.Clear()
        I_TBL.Columns.Add("CAMPCODE", GetType(String))  '会社コード
        I_TBL.Columns.Add("MOTOCHO", GetType(String))   '元帳
        I_TBL.Columns.Add("VERSION", GetType(String))   'バージョン
        I_TBL.Columns.Add("DENTYPE", GetType(String))   '伝票タイプ
        I_TBL.Columns.Add("TENKI", GetType(String)) '統計転記
        I_TBL.Columns.Add("KEIJOYMD", GetType(Date))  '計上日付
        I_TBL.Columns.Add("DENYMD", GetType(Date))    '伝票日付
        I_TBL.Columns.Add("DENNO", GetType(String)) '伝票番号
        I_TBL.Columns.Add("KANRENDENNO", GetType(String))   '関連伝票No＋明細No
        I_TBL.Columns.Add("DTLNO", GetType(String)) '明細番号
        I_TBL.Columns.Add("INQKBN", GetType(String))    '照会区分
        I_TBL.Columns.Add("ACDCKBN", GetType(String))   '貸借区分
        I_TBL.Columns.Add("ACACHANTEI", GetType(String))    '勘定科目判定コード
        I_TBL.Columns.Add("ACCODE", GetType(String))    '勘定科目コード
        I_TBL.Columns.Add("SUBACCODE", GetType(String)) '補助科目コード
        I_TBL.Columns.Add("ACTORICODE", GetType(String))    '取引先コード
        I_TBL.Columns.Add("ACOILTYPE", GetType(String)) '油種
        I_TBL.Columns.Add("ACSHARYOTYPE", GetType(String))  '統一車番(上)
        I_TBL.Columns.Add("ACTSHABAN", GetType(String)) '統一車番(下)
        I_TBL.Columns.Add("ACSTAFFCODE", GetType(String))   '従業員コード
        I_TBL.Columns.Add("ACBANKAC", GetType(String))  '銀行口座
        I_TBL.Columns.Add("ACKEIJOMORG", GetType(String))   '計上管理部署コード
        I_TBL.Columns.Add("ACKEIJOORG", GetType(String))    '計上部署コード
        I_TBL.Columns.Add("ACTAXKBN", GetType(String))  '税区分
        I_TBL.Columns.Add("ACAMT", GetType(Integer)) '金額
        I_TBL.Columns.Add("NACSHUKODATE", GetType(Date))  '出庫日
        I_TBL.Columns.Add("NACSHUKADATE", GetType(Date))  '出荷日
        I_TBL.Columns.Add("NACTODOKEDATE", GetType(Date)) '届日
        I_TBL.Columns.Add("NACTORICODE", GetType(String))   '荷主コード
        I_TBL.Columns.Add("NACURIKBN", GetType(String)) '売上計上基準
        I_TBL.Columns.Add("NACTODOKECODE", GetType(String)) '届先コード
        I_TBL.Columns.Add("NACSTORICODE", GetType(String))  '販売店コード
        I_TBL.Columns.Add("NACSHUKABASHO", GetType(String)) '出荷場所
        I_TBL.Columns.Add("NACTORITYPE01", GetType(String)) '取引先・取引タイプ01
        I_TBL.Columns.Add("NACTORITYPE02", GetType(String)) '取引先・取引タイプ02
        I_TBL.Columns.Add("NACTORITYPE03", GetType(String)) '取引先・取引タイプ03
        I_TBL.Columns.Add("NACTORITYPE04", GetType(String)) '取引先・取引タイプ04
        I_TBL.Columns.Add("NACTORITYPE05", GetType(String)) '取引先・取引タイプ05
        I_TBL.Columns.Add("NACOILTYPE", GetType(String))    '油種
        I_TBL.Columns.Add("NACPRODUCT1", GetType(String))   '品名１
        I_TBL.Columns.Add("NACPRODUCT2", GetType(String))   '品名２
        I_TBL.Columns.Add("NACPRODUCTCODE", GetType(String))   '品名コード
        I_TBL.Columns.Add("NACGSHABAN", GetType(String))    '業務車番
        I_TBL.Columns.Add("NACSUPPLIERKBN", GetType(String))    '社有・庸車区分
        I_TBL.Columns.Add("NACSUPPLIER", GetType(String))   '庸車会社
        I_TBL.Columns.Add("NACSHARYOOILTYPE", GetType(String))  '車両登録油種
        I_TBL.Columns.Add("NACSHARYOTYPE1", GetType(String))    '統一車番(上)1
        I_TBL.Columns.Add("NACTSHABAN1", GetType(String))   '統一車番(下)1
        I_TBL.Columns.Add("NACMANGMORG1", GetType(String))  '車両管理部署1
        I_TBL.Columns.Add("NACMANGSORG1", GetType(String))  '車両設置部署1
        I_TBL.Columns.Add("NACMANGUORG1", GetType(String))  '車両運用部署1
        I_TBL.Columns.Add("NACBASELEASE1", GetType(String)) '車両所有1
        I_TBL.Columns.Add("NACSHARYOTYPE2", GetType(String))    '統一車番(上)2
        I_TBL.Columns.Add("NACTSHABAN2", GetType(String))   '統一車番(下)2
        I_TBL.Columns.Add("NACMANGMORG2", GetType(String))  '車両管理部署2
        I_TBL.Columns.Add("NACMANGSORG2", GetType(String))  '車両設置部署2
        I_TBL.Columns.Add("NACMANGUORG2", GetType(String))  '車両運用部署2
        I_TBL.Columns.Add("NACBASELEASE2", GetType(String)) '車両所有2
        I_TBL.Columns.Add("NACSHARYOTYPE3", GetType(String))    '統一車番(上)3
        I_TBL.Columns.Add("NACTSHABAN3", GetType(String))   '統一車番(下)3
        I_TBL.Columns.Add("NACMANGMORG3", GetType(String))  '車両管理部署3
        I_TBL.Columns.Add("NACMANGSORG3", GetType(String))  '車両設置部署3
        I_TBL.Columns.Add("NACMANGUORG3", GetType(String))  '車両運用部署3
        I_TBL.Columns.Add("NACBASELEASE3", GetType(String)) '車両所有3
        I_TBL.Columns.Add("NACCREWKBN", GetType(String))    '正副区分
        I_TBL.Columns.Add("NACSTAFFCODE", GetType(String))  '従業員コード（正）
        I_TBL.Columns.Add("NACSTAFFKBN", GetType(String))   '社員区分（正）
        I_TBL.Columns.Add("NACMORG", GetType(String))   '管理部署（正）
        I_TBL.Columns.Add("NACHORG", GetType(String))   '配属部署（正）
        I_TBL.Columns.Add("NACSORG", GetType(String))   '作業部署（正）
        I_TBL.Columns.Add("NACSTAFFCODE2", GetType(String)) '従業員コード（副）
        I_TBL.Columns.Add("NACSTAFFKBN2", GetType(String))  '社員区分（副）
        I_TBL.Columns.Add("NACMORG2", GetType(String))  '管理部署（副）
        I_TBL.Columns.Add("NACHORG2", GetType(String))  '配属部署（副）
        I_TBL.Columns.Add("NACSORG2", GetType(String))  '作業部署（副）
        I_TBL.Columns.Add("NACORDERNO", GetType(String))    '受注番号
        I_TBL.Columns.Add("NACDETAILNO", GetType(String))   '明細№
        I_TBL.Columns.Add("NACTRIPNO", GetType(String)) 'トリップ
        I_TBL.Columns.Add("NACDROPNO", GetType(String)) 'ドロップ
        I_TBL.Columns.Add("NACSEQ", GetType(String))    'SEQ
        I_TBL.Columns.Add("NACORDERORG", GetType(String))   '受注部署
        I_TBL.Columns.Add("NACSHIPORG", GetType(String))    '配送部署
        I_TBL.Columns.Add("NACSURYO", GetType(Decimal))  '受注・数量
        I_TBL.Columns.Add("NACTANI", GetType(String))   '受注・単位
        I_TBL.Columns.Add("NACJSURYO", GetType(Decimal)) '実績・配送数量
        I_TBL.Columns.Add("NACSTANI", GetType(String))  '実績・配送単位
        I_TBL.Columns.Add("NACHAIDISTANCE", GetType(Decimal))    '実績・配送距離
        I_TBL.Columns.Add("NACKAIDISTANCE", GetType(Decimal))    '実績・回送作業距離
        I_TBL.Columns.Add("NACCHODISTANCE", GetType(Decimal))    '実績・勤怠調整距離
        I_TBL.Columns.Add("NACTTLDISTANCE", GetType(Decimal))    '実績・配送距離合計Σ
        I_TBL.Columns.Add("NACHAISTDATE", GetType(DateTime))  '実績・配送作業開始日時
        I_TBL.Columns.Add("NACHAIENDDATE", GetType(DateTime)) '実績・配送作業終了日時
        I_TBL.Columns.Add("NACHAIWORKTIME", GetType(Decimal))    '実績・配送作業時間（分）
        I_TBL.Columns.Add("NACGESSTDATE", GetType(DateTime))  '実績・下車作業開始日時
        I_TBL.Columns.Add("NACGESENDDATE", GetType(DateTime)) '実績・下車作業終了日時
        I_TBL.Columns.Add("NACGESWORKTIME", GetType(Decimal))    '実績・下車作業時間（分）
        I_TBL.Columns.Add("NACCHOWORKTIME", GetType(Decimal))    '実績・勤怠調整時間（分）
        I_TBL.Columns.Add("NACTTLWORKTIME", GetType(Decimal))    '実績・配送合計時間Σ（分）
        I_TBL.Columns.Add("NACOUTWORKTIME", GetType(Decimal))    '実績・就業外時間Σ（分）
        I_TBL.Columns.Add("NACBREAKSTDATE", GetType(DateTime))    '実績・休憩開始日時
        I_TBL.Columns.Add("NACBREAKENDDATE", GetType(DateTime))   '実績・休憩終了日時
        I_TBL.Columns.Add("NACBREAKTIME", GetType(Decimal))  '実績・休憩時間（分）
        I_TBL.Columns.Add("NACCHOBREAKTIME", GetType(Decimal))   '実績・休憩調整時間（分）
        I_TBL.Columns.Add("NACTTLBREAKTIME", GetType(Decimal))   '実績・休憩合計時間Σ（分）
        I_TBL.Columns.Add("NACCASH", GetType(Integer))   '実績・現金
        I_TBL.Columns.Add("NACETC", GetType(Integer))    '実績・ETC
        I_TBL.Columns.Add("NACTICKET", GetType(Integer)) '実績・回数券
        I_TBL.Columns.Add("NACKYUYU", GetType(Decimal))  '実績・軽油
        I_TBL.Columns.Add("NACUNLOADCNT", GetType(Decimal))  '実績・荷卸回数
        I_TBL.Columns.Add("NACCHOUNLOADCNT", GetType(Decimal))   '実績・荷卸回数調整
        I_TBL.Columns.Add("NACTTLUNLOADCNT", GetType(Decimal))   '実績・荷卸回数合計Σ
        I_TBL.Columns.Add("NACKAIJI", GetType(Decimal))   '実績・回次
        I_TBL.Columns.Add("NACJITIME", GetType(Decimal)) '実績・実車時間（分）
        I_TBL.Columns.Add("NACJICHOSTIME", GetType(Decimal)) '実績・実車時間調整（分）
        I_TBL.Columns.Add("NACJITTLETIME", GetType(Decimal)) '実績・実車時間合計Σ（分）
        I_TBL.Columns.Add("NACKUTIME", GetType(Decimal)) '実績・空車時間（分）
        I_TBL.Columns.Add("NACKUCHOTIME", GetType(Decimal))  '実績・空車時間調整（分）
        I_TBL.Columns.Add("NACKUTTLTIME", GetType(Decimal))  '実績・空車時間合計Σ（分）
        I_TBL.Columns.Add("NACJIDISTANCE", GetType(Decimal)) '実績・実車距離
        I_TBL.Columns.Add("NACJICHODISTANCE", GetType(Decimal))  '実績・実車距離調整
        I_TBL.Columns.Add("NACJITTLDISTANCE", GetType(Decimal))  '実績・実車距離合計Σ
        I_TBL.Columns.Add("NACKUDISTANCE", GetType(Decimal)) '実績・空車距離
        I_TBL.Columns.Add("NACKUCHODISTANCE", GetType(Decimal))  '実績・空車距離調整
        I_TBL.Columns.Add("NACKUTTLDISTANCE", GetType(Decimal))  '実績・空車距離合計Σ
        I_TBL.Columns.Add("NACTARIFFFARE", GetType(Integer)) '実績・運賃タリフ額
        I_TBL.Columns.Add("NACFIXEDFARE", GetType(Integer))  '実績・運賃固定額
        I_TBL.Columns.Add("NACINCHOFARE", GetType(Integer))  '実績・運賃手入力調整額
        I_TBL.Columns.Add("NACTTLFARE", GetType(Integer))    '実績・運賃合計額Σ
        I_TBL.Columns.Add("NACOFFICESORG", GetType(String)) '実績・作業部署
        I_TBL.Columns.Add("NACOFFICETIME", GetType(Decimal)) '実績・事務時間（分）
        I_TBL.Columns.Add("NACOFFICEBREAKTIME", GetType(Decimal))    '実績・事務休憩時間（分）
        I_TBL.Columns.Add("PAYSHUSHADATE", GetType(DateTime)) '出社日時
        I_TBL.Columns.Add("PAYTAISHADATE", GetType(DateTime)) '退社日時
        I_TBL.Columns.Add("PAYSTAFFKBN", GetType(String))   '社員区分
        I_TBL.Columns.Add("PAYSTAFFCODE", GetType(String))  '従業員コード
        I_TBL.Columns.Add("PAYMORG", GetType(String))   '従業員管理部署
        I_TBL.Columns.Add("PAYHORG", GetType(String))   '従業員配属部署
        I_TBL.Columns.Add("PAYHOLIDAYKBN", GetType(String)) '休日区分
        I_TBL.Columns.Add("PAYKBN", GetType(String))    '勤怠区分
        I_TBL.Columns.Add("PAYSHUKCHOKKBN", GetType(String))    '宿日直区分
        I_TBL.Columns.Add("PAYJYOMUKBN", GetType(String))   '乗務区分
        I_TBL.Columns.Add("PAYOILKBN", GetType(String))   '勤怠用油種区分
        I_TBL.Columns.Add("PAYSHARYOKBN", GetType(String))   '勤怠用車両区分
        I_TBL.Columns.Add("PAYWORKNISSU", GetType(Decimal))  '所労
        I_TBL.Columns.Add("PAYSHOUKETUNISSU", GetType(Decimal))  '傷欠
        I_TBL.Columns.Add("PAYKUMIKETUNISSU", GetType(Decimal))  '組欠
        I_TBL.Columns.Add("PAYETCKETUNISSU", GetType(Decimal))   '他欠
        I_TBL.Columns.Add("PAYNENKYUNISSU", GetType(Decimal))    '年休
        I_TBL.Columns.Add("PAYTOKUKYUNISSU", GetType(Decimal))   '特休
        I_TBL.Columns.Add("PAYCHIKOKSOTAINISSU", GetType(Decimal))   '遅早
        I_TBL.Columns.Add("PAYSTOCKNISSU", GetType(Decimal)) 'ストック休暇
        I_TBL.Columns.Add("PAYKYOTEIWEEKNISSU", GetType(Decimal))    '協定週休
        I_TBL.Columns.Add("PAYWEEKNISSU", GetType(Decimal))  '週休
        I_TBL.Columns.Add("PAYDAIKYUNISSU", GetType(Decimal))    '代休
        I_TBL.Columns.Add("PAYWORKTIME", GetType(Decimal))   '所定労働時間（分）
        I_TBL.Columns.Add("PAYWWORKTIME", GetType(Decimal))  '所定内時間（分）
        I_TBL.Columns.Add("PAYNIGHTTIME", GetType(Decimal))  '所定深夜時間（分）
        I_TBL.Columns.Add("PAYORVERTIME", GetType(Decimal))  '平日残業時間（分）
        I_TBL.Columns.Add("PAYWNIGHTTIME", GetType(Decimal)) '平日深夜時間（分）
        I_TBL.Columns.Add("PAYWSWORKTIME", GetType(Decimal)) '日曜出勤時間（分）
        I_TBL.Columns.Add("PAYSNIGHTTIME", GetType(Decimal)) '日曜深夜時間（分）
        I_TBL.Columns.Add("PAYSDAIWORKTIME", GetType(Decimal)) '日曜代休出勤時間（分）
        I_TBL.Columns.Add("PAYSDAINIGHTTIME", GetType(Decimal)) '日曜代休深夜時間（分）
        I_TBL.Columns.Add("PAYHWORKTIME", GetType(Decimal))  '休日出勤時間（分）
        I_TBL.Columns.Add("PAYHNIGHTTIME", GetType(Decimal)) '休日深夜時間（分）
        I_TBL.Columns.Add("PAYHDAIWORKTIME", GetType(Decimal))  '休日代休出勤時間（分）
        I_TBL.Columns.Add("PAYHDAINIGHTTIME", GetType(Decimal)) '休日代休深夜時間（分）
        I_TBL.Columns.Add("PAYBREAKTIME", GetType(Decimal))  '休憩時間（分）
        I_TBL.Columns.Add("PAYNENSHINISSU", GetType(Decimal))    '年始出勤
        I_TBL.Columns.Add("PAYNENMATUNISSU", GetType(Decimal))    '年末出勤
        I_TBL.Columns.Add("PAYSHUKCHOKNNISSU", GetType(Decimal)) '宿日直年始
        I_TBL.Columns.Add("PAYSHUKCHOKNISSU", GetType(Decimal))  '宿日直通常
        I_TBL.Columns.Add("PAYSHUKCHOKNHLDNISSU", GetType(Decimal)) '宿日直年始（翌日休み）
        I_TBL.Columns.Add("PAYSHUKCHOKHLDNISSU", GetType(Decimal))  '宿日直通常（翌日休み）
        I_TBL.Columns.Add("PAYTOKSAAKAISU", GetType(Decimal))    '特作A
        I_TBL.Columns.Add("PAYTOKSABKAISU", GetType(Decimal))    '特作B
        I_TBL.Columns.Add("PAYTOKSACKAISU", GetType(Decimal))    '特作C
        I_TBL.Columns.Add("PAYTENKOKAISU", GetType(Decimal))     '点呼回数
        I_TBL.Columns.Add("PAYHOANTIME", GetType(Decimal))   '保安検査入力（分）
        I_TBL.Columns.Add("PAYKOATUTIME", GetType(Decimal))  '高圧作業入力（分）
        I_TBL.Columns.Add("PAYTOKUSA1TIME", GetType(Decimal))    '特作Ⅰ（分）
        I_TBL.Columns.Add("PAYPONPNISSU", GetType(Decimal))  'ポンプ
        I_TBL.Columns.Add("PAYBULKNISSU", GetType(Decimal))  'バルク
        I_TBL.Columns.Add("PAYTRAILERNISSU", GetType(Decimal))   'トレーラ
        I_TBL.Columns.Add("PAYBKINMUKAISU", GetType(Decimal))    'B勤務
        I_TBL.Columns.Add("PAYYENDTIME", GetType(String))    '予定退社時刻
        I_TBL.Columns.Add("PAYAPPLYID", GetType(String))    '申請ID
        I_TBL.Columns.Add("PAYRIYU", GetType(String))    '理由
        I_TBL.Columns.Add("PAYRIYUETC", GetType(String))    '理由その他
        I_TBL.Columns.Add("PAYHAYADETIME", GetType(Decimal))      '早出補填時間
        I_TBL.Columns.Add("PAYHAISOTIME", GetType(Decimal))       '配送時間
        I_TBL.Columns.Add("PAYSHACHUHAKNISSU", GetType(Decimal))  '車中泊日数
        I_TBL.Columns.Add("PAYMODELDISTANCE", GetType(Decimal))   'モデル距離
        I_TBL.Columns.Add("PAYJIKYUSHATIME", GetType(Decimal))    '時給者時間
        I_TBL.Columns.Add("PAYJYOMUTIME", GetType(Decimal))       '乗務時間
        I_TBL.Columns.Add("PAYHWORKNISSU", GetType(Decimal))      '休日出勤日数
        I_TBL.Columns.Add("PAYKAITENCNT", GetType(Decimal))       '回転数
        I_TBL.Columns.Add("PAYSENJYOCNT", GetType(Decimal))       '洗浄回数
        I_TBL.Columns.Add("PAYUNLOADADDCNT1", GetType(Decimal))   '危険物荷卸回数1
        I_TBL.Columns.Add("PAYUNLOADADDCNT2", GetType(Decimal))   '危険物荷卸回数2
        I_TBL.Columns.Add("PAYUNLOADADDCNT3", GetType(Decimal))   '危険物荷卸回数3
        I_TBL.Columns.Add("PAYUNLOADADDCNT4", GetType(Decimal))   '危険物荷卸回数4
        I_TBL.Columns.Add("PAYSHORTDISTANCE1", GetType(Decimal))  '短距離手当1
        I_TBL.Columns.Add("PAYSHORTDISTANCE2", GetType(Decimal))  '短距離手当2
        I_TBL.Columns.Add("APPKIJUN", GetType(String))  '配賦基準
        I_TBL.Columns.Add("APPKEY", GetType(String))    '配賦統計キー

        I_TBL.Columns.Add("WORKKBN", GetType(String))    '作業区分
        I_TBL.Columns.Add("KEYSTAFFCODE", GetType(String))    '従業員コードキー
        I_TBL.Columns.Add("KEYGSHABAN", GetType(String))    '業務車番キー
        I_TBL.Columns.Add("KEYTRIPNO", GetType(String))    'トリップキー
        I_TBL.Columns.Add("KEYDROPNO", GetType(String))    'ドロップキー

        I_TBL.Columns.Add("DELFLG", GetType(String))    '削除フラグ
        I_TBL.Columns.Add("INITYMD", GetType(String))   '登録年月日
        I_TBL.Columns.Add("UPDYMD", GetType(String))    '更新年月日
        I_TBL.Columns.Add("UPDUSER", GetType(String))   '更新ユーザＩＤ
        I_TBL.Columns.Add("UPDTERMID", GetType(String)) '更新端末
        I_TBL.Columns.Add("RECEIVEYMD", GetType(String))    '集信日時

    End Sub
    ''' <summary>
    ''' 統計情報用テンポラリテーブルを作成する
    ''' </summary>
    ''' <param name="I_TBL_Name">テンポラリテーブル名</param>
    ''' <remarks></remarks>
    Public Sub CS0044L1CreTempTbl(ByVal I_TBL_Name As String)

        Dim SQLStr As String = ""

        'テンポラリーテーブルを作成する
        SQLStr = "CREATE TABLE " & I_TBL_Name _
                & " ( " _
                & "  CAMPCODE nvarchar(20)," _
                & "  MOTOCHO nvarchar(20)," _
                & "  VERSION nvarchar(3)," _
                & "  DENTYPE nvarchar(20)," _
                & "  TENKI nvarchar(20)," _
                & "  KEIJOYMD date," _
                & "  DENYMD date," _
                & "  DENNO nvarchar(20)," _
                & "  KANRENDENNO nvarchar(50)," _
                & "  DTLNO nvarchar(10)," _
                & "  INQKBN nvarchar(10)," _
                & "  ACDCKBN nvarchar(1)," _
                & "  ACACHANTEI nvarchar(20)," _
                & "  ACCODE nvarchar(20)," _
                & "  SUBACCODE nvarchar(20)," _
                & "  ACTORICODE nvarchar(20)," _
                & "  ACOILTYPE nvarchar(20)," _
                & "  ACSHARYOTYPE nvarchar(1)," _
                & "  ACTSHABAN nvarchar(19)," _
                & "  ACSTAFFCODE nvarchar(20)," _
                & "  ACBANKAC nvarchar(20)," _
                & "  ACKEIJOMORG nvarchar(20)," _
                & "  ACKEIJOORG nvarchar(20)," _
                & "  ACTAXKBN nvarchar(10)," _
                & "  ACAMT int," _
                & "  NACSHUKODATE date," _
                & "  NACSHUKADATE date," _
                & "  NACTODOKEDATE date," _
                & "  NACTORICODE nvarchar(20)," _
                & "  NACURIKBN nvarchar(1)," _
                & "  NACTODOKECODE nvarchar(20)," _
                & "  NACSTORICODE nvarchar(20)," _
                & "  NACSHUKABASHO nvarchar(20)," _
                & "  NACTORITYPE01 nvarchar(3)," _
                & "  NACTORITYPE02 nvarchar(3)," _
                & "  NACTORITYPE03 nvarchar(3)," _
                & "  NACTORITYPE04 nvarchar(3)," _
                & "  NACTORITYPE05 nvarchar(3)," _
                & "  NACOILTYPE nvarchar(20)," _
                & "  NACPRODUCT1 nvarchar(20)," _
                & "  NACPRODUCT2 nvarchar(20)," _
                & "  NACPRODUCTCODE nvarchar(30)," _
                & "  NACGSHABAN nvarchar(20)," _
                & "  NACSUPPLIERKBN nvarchar(1)," _
                & "  NACSUPPLIER nvarchar(20)," _
                & "  NACSHARYOOILTYPE nvarchar(20)," _
                & "  NACSHARYOTYPE1 nvarchar(1)," _
                & "  NACTSHABAN1 nvarchar(19)," _
                & "  NACMANGMORG1 nvarchar(20)," _
                & "  NACMANGSORG1 nvarchar(20)," _
                & "  NACMANGUORG1 nvarchar(20)," _
                & "  NACBASELEASE1 nvarchar(20)," _
                & "  NACSHARYOTYPE2 nvarchar(1)," _
                & "  NACTSHABAN2 nvarchar(19)," _
                & "  NACMANGMORG2 nvarchar(20)," _
                & "  NACMANGSORG2 nvarchar(20)," _
                & "  NACMANGUORG2 nvarchar(20)," _
                & "  NACBASELEASE2 nvarchar(20)," _
                & "  NACSHARYOTYPE3 nvarchar(1)," _
                & "  NACTSHABAN3 nvarchar(19)," _
                & "  NACMANGMORG3 nvarchar(20)," _
                & "  NACMANGSORG3 nvarchar(20)," _
                & "  NACMANGUORG3 nvarchar(20)," _
                & "  NACBASELEASE3 nvarchar(20)," _
                & "  NACCREWKBN nvarchar(1)," _
                & "  NACSTAFFCODE nvarchar(20)," _
                & "  NACSTAFFKBN nvarchar(5)," _
                & "  NACMORG nvarchar(20)," _
                & "  NACHORG nvarchar(20)," _
                & "  NACSORG nvarchar(20)," _
                & "  NACSTAFFCODE2 nvarchar(20)," _
                & "  NACSTAFFKBN2 nvarchar(5)," _
                & "  NACMORG2 nvarchar(20)," _
                & "  NACHORG2 nvarchar(20)," _
                & "  NACSORG2 nvarchar(20)," _
                & "  NACORDERNO nvarchar(10)," _
                & "  NACDETAILNO nvarchar(10)," _
                & "  NACTRIPNO nvarchar(10)," _
                & "  NACDROPNO nvarchar(10)," _
                & "  NACSEQ nvarchar(2)," _
                & "  NACORDERORG nvarchar(20)," _
                & "  NACSHIPORG nvarchar(20)," _
                & "  NACSURYO numeric(10, 3)," _
                & "  NACTANI nvarchar(10)," _
                & "  NACJSURYO numeric(10, 3)," _
                & "  NACSTANI nvarchar(10)," _
                & "  NACHAIDISTANCE numeric(10, 3)," _
                & "  NACKAIDISTANCE numeric(10, 3)," _
                & "  NACCHODISTANCE numeric(10, 3)," _
                & "  NACTTLDISTANCE numeric(10, 3)," _
                & "  NACHAISTDATE datetime," _
                & "  NACHAIENDDATE datetime," _
                & "  NACHAIWORKTIME numeric(10, 3)," _
                & "  NACGESSTDATE datetime," _
                & "  NACGESENDDATE datetime," _
                & "  NACGESWORKTIME numeric(10, 3)," _
                & "  NACCHOWORKTIME numeric(10, 3)," _
                & "  NACTTLWORKTIME numeric(10, 3)," _
                & "  NACOUTWORKTIME numeric(10, 3)," _
                & "  NACBREAKSTDATE datetime," _
                & "  NACBREAKENDDATE datetime," _
                & "  NACBREAKTIME numeric(10, 3)," _
                & "  NACCHOBREAKTIME numeric(10, 3)," _
                & "  NACTTLBREAKTIME numeric(10, 3)," _
                & "  NACCASH int," _
                & "  NACETC int," _
                & "  NACTICKET int," _
                & "  NACKYUYU numeric(10, 3)," _
                & "  NACUNLOADCNT numeric(10, 3)," _
                & "  NACCHOUNLOADCNT numeric(10, 3)," _
                & "  NACTTLUNLOADCNT numeric(10, 3)," _
                & "  NACKAIJI numeric(10, 3)," _
                & "  NACJITIME numeric(10, 3)," _
                & "  NACJICHOSTIME numeric(10, 3)," _
                & "  NACJITTLETIME numeric(10, 3)," _
                & "  NACKUTIME numeric(10, 3)," _
                & "  NACKUCHOTIME numeric(10, 3)," _
                & "  NACKUTTLTIME numeric(10, 3)," _
                & "  NACJIDISTANCE numeric(10, 3)," _
                & "  NACJICHODISTANCE numeric(10, 3)," _
                & "  NACJITTLDISTANCE numeric(10, 3)," _
                & "  NACKUDISTANCE numeric(10, 3)," _
                & "  NACKUCHODISTANCE numeric(10, 3)," _
                & "  NACKUTTLDISTANCE numeric(10, 3)," _
                & "  NACTARIFFFARE int," _
                & "  NACFIXEDFARE int," _
                & "  NACINCHOFARE int," _
                & "  NACTTLFARE int," _
                & "  NACOFFICESORG nvarchar(20)," _
                & "  NACOFFICETIME numeric(10, 3)," _
                & "  NACOFFICEBREAKTIME numeric(10, 3)," _
                & "  PAYSHUSHADATE datetime," _
                & "  PAYTAISHADATE datetime," _
                & "  PAYSTAFFKBN nvarchar(5)," _
                & "  PAYSTAFFCODE nvarchar(20)," _
                & "  PAYMORG nvarchar(20)," _
                & "  PAYHORG nvarchar(20)," _
                & "  PAYHOLIDAYKBN nvarchar(1)," _
                & "  PAYKBN nvarchar(20)," _
                & "  PAYSHUKCHOKKBN nvarchar(20)," _
                & "  PAYJYOMUKBN nvarchar(20)," _
                & "  PAYOILKBN nvarchar(20)," _
                & "  PAYSHARYOKBN nvarchar(1)," _
                & "  PAYWORKNISSU numeric(10, 3)," _
                & "  PAYSHOUKETUNISSU numeric(10, 3)," _
                & "  PAYKUMIKETUNISSU numeric(10, 3)," _
                & "  PAYETCKETUNISSU numeric(10, 3)," _
                & "  PAYNENKYUNISSU numeric(10, 3)," _
                & "  PAYTOKUKYUNISSU numeric(10, 3)," _
                & "  PAYCHIKOKSOTAINISSU numeric(10, 3)," _
                & "  PAYSTOCKNISSU numeric(10, 3)," _
                & "  PAYKYOTEIWEEKNISSU numeric(10, 3)," _
                & "  PAYWEEKNISSU numeric(10, 3)," _
                & "  PAYDAIKYUNISSU numeric(10, 3)," _
                & "  PAYWORKTIME numeric(10, 3)," _
                & "  PAYWWORKTIME numeric(10, 3)," _
                & "  PAYNIGHTTIME numeric(10, 3)," _
                & "  PAYORVERTIME numeric(10, 3)," _
                & "  PAYWNIGHTTIME numeric(10, 3)," _
                & "  PAYWSWORKTIME numeric(10, 3)," _
                & "  PAYSNIGHTTIME numeric(10, 3)," _
                & "  PAYSDAIWORKTIME numeric(10, 3)," _
                & "  PAYSDAINIGHTTIME numeric(10, 3)," _
                & "  PAYHWORKTIME numeric(10, 3)," _
                & "  PAYHNIGHTTIME numeric(10, 3)," _
                & "  PAYHDAIWORKTIME numeric(10, 3)," _
                & "  PAYHDAINIGHTTIME numeric(10, 3)," _
                & "  PAYBREAKTIME numeric(10, 3)," _
                & "  PAYNENSHINISSU numeric(10, 3)," _
                & "  PAYNENMATUNISSU numeric(10, 3)," _
                & "  PAYSHUKCHOKNNISSU numeric(10, 3)," _
                & "  PAYSHUKCHOKNISSU numeric(10, 3)," _
                & "  PAYSHUKCHOKNHLDNISSU numeric(10, 3)," _
                & "  PAYSHUKCHOKHLDNISSU numeric(10, 3)," _
                & "  PAYTOKSAAKAISU numeric(10, 3)," _
                & "  PAYTOKSABKAISU numeric(10, 3)," _
                & "  PAYTOKSACKAISU numeric(10, 3)," _
                & "  PAYTENKOKAISU numeric(10, 3)," _
                & "  PAYHOANTIME numeric(10, 3)," _
                & "  PAYKOATUTIME numeric(10, 3)," _
                & "  PAYTOKUSA1TIME numeric(10, 3)," _
                & "  PAYPONPNISSU numeric(10, 3)," _
                & "  PAYBULKNISSU numeric(10, 3)," _
                & "  PAYTRAILERNISSU numeric(10, 3)," _
                & "  PAYBKINMUKAISU numeric(10, 3)," _
                & "  PAYYENDTIME time," _
                & "  PAYAPPLYID nvarchar(30)," _
                & "  PAYRIYU nvarchar(2)," _
                & "  PAYRIYUETC nvarchar(200)," _
                & "  PAYHAYADETIME numeric(10, 3)," _
                & "  PAYHAISOTIME numeric(10, 3)," _
                & "  PAYSHACHUHAKNISSU numeric(10, 3)," _
                & "  PAYMODELDISTANCE numeric(10, 3)," _
                & "  PAYJIKYUSHATIME numeric(10, 3)," _
                & "  PAYJYOMUTIME numeric(10, 3)," _
                & "  PAYHWORKNISSU numeric(10, 3)," _
                & "  PAYKAITENCNT numeric(10, 3)," _
                & "  PAYSENJYOCNT numeric(10, 3)," _
                & "  PAYUNLOADADDCNT1 numeric(10, 3)," _
                & "  PAYUNLOADADDCNT2 numeric(10, 3)," _
                & "  PAYUNLOADADDCNT3 numeric(10, 3)," _
                & "  PAYUNLOADADDCNT4 numeric(10, 3)," _
                & "  PAYSHORTDISTANCE1 numeric(10, 3)," _
                & "  PAYSHORTDISTANCE2 numeric(10, 3)," _
                & "  APPKIJUN nvarchar(20)," _
                & "  APPKEY nvarchar(20)," _
                & "  WORKKBN nvarchar(2)," _
                & "  KEYSTAFFCODE nvarchar(20)," _
                & "  KEYGSHABAN nvarchar(20)," _
                & "  KEYTRIPNO nvarchar(10)," _
                & "  KEYDROPNO nvarchar(10)," _
                & "  DELFLG nvarchar(1)," _
                & "  INITYMD smalldatetime," _
                & "  UPDYMD datetime," _
                & "  UPDUSER nvarchar(20)," _
                & "  UPDTERMID nvarchar(30)," _
                & "  RECEIVEYMD datetime " _
                & " ) "

        Dim SQLcmd As New SqlCommand(SQLStr, SQLCON)
        SQLcmd.ExecuteNonQuery()
        SQLcmd.Dispose()
        SQLcmd = Nothing

    End Sub
    ''' <summary>
    ''' 統計情報DBに登録する
    ''' </summary>
    ''' <param name="I_TBL">登録するテーブルデータ</param>
    ''' <remarks></remarks>
    Public Sub CS0044L1Insert2(ByVal I_TBL As DataTable)
        Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
        Try
            ERR = C_MESSAGE_NO.NORMAL

            Dim bc As New SqlClient.SqlBulkCopy(SQLCON)
            bc.DestinationTableName = INSERT_TABLE_NAME
            bc.WriteToServer(I_TBL)

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "CS0044T6INSERT2"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:L0001_TOKEI Insert"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' 統計情報DBに登録する
    ''' </summary>
    ''' <param name="I_TBL">登録するテーブルデータ</param>
    ''' <remarks></remarks>
    Public Sub CS0044L1Insert(ByVal I_TBL As DataTable)

        Try
            'テンポラリテーブル作成
            CS0044L1CreTempTbl(INSERT_TEMP_TABLE_NAME)

            'テンポラリテーブルに一旦出力（バルクコピー）する（内部テーブルと外部テーブルのカラムが同じであること）
            Dim bc As New SqlClient.SqlBulkCopy(SQLCON)
            bc.DestinationTableName = INSERT_TEMP_TABLE_NAME
            bc.WriteToServer(I_TBL)

            '検索SQL文
            '〇配送受注DB登録
            Dim SQLStr As String =
                      " INSERT INTO " _
                    & INSERT_TABLE_NAME _
                    & " ( " _
                    & "        CAMPCODE, " _
                    & "        MOTOCHO, " _
                    & "        VERSION, " _
                    & "        DENTYPE, " _
                    & "        TENKI, " _
                    & "        KEIJOYMD, " _
                    & "        DENYMD, " _
                    & "        DENNO, " _
                    & "        KANRENDENNO, " _
                    & "        DTLNO, " _
                    & "        INQKBN, " _
                    & "        ACDCKBN, " _
                    & "        ACACHANTEI, " _
                    & "        ACCODE, " _
                    & "        SUBACCODE, " _
                    & "        ACTORICODE, " _
                    & "        ACOILTYPE, " _
                    & "        ACSHARYOTYPE, " _
                    & "        ACTSHABAN, " _
                    & "        ACSTAFFCODE, " _
                    & "        ACBANKAC, " _
                    & "        ACKEIJOMORG, " _
                    & "        ACKEIJOORG, " _
                    & "        ACTAXKBN, " _
                    & "        ACAMT, " _
                    & "        NACSHUKODATE, " _
                    & "        NACSHUKADATE, " _
                    & "        NACTODOKEDATE, " _
                    & "        NACTORICODE, " _
                    & "        NACURIKBN, " _
                    & "        NACTODOKECODE, " _
                    & "        NACSTORICODE, " _
                    & "        NACSHUKABASHO, " _
                    & "        NACTORITYPE01, " _
                    & "        NACTORITYPE02, " _
                    & "        NACTORITYPE03, " _
                    & "        NACTORITYPE04, " _
                    & "        NACTORITYPE05, " _
                    & "        NACOILTYPE, " _
                    & "        NACPRODUCT1, " _
                    & "        NACPRODUCT2, " _
                    & "        NACPRODUCTCODE, " _
                    & "        NACGSHABAN, " _
                    & "        NACSUPPLIERKBN, " _
                    & "        NACSUPPLIER, " _
                    & "        NACSHARYOOILTYPE, " _
                    & "        NACSHARYOTYPE1, " _
                    & "        NACTSHABAN1, " _
                    & "        NACMANGMORG1, " _
                    & "        NACMANGSORG1, " _
                    & "        NACMANGUORG1, " _
                    & "        NACBASELEASE1, " _
                    & "        NACSHARYOTYPE2, " _
                    & "        NACTSHABAN2, " _
                    & "        NACMANGMORG2, " _
                    & "        NACMANGSORG2, " _
                    & "        NACMANGUORG2, " _
                    & "        NACBASELEASE2, " _
                    & "        NACSHARYOTYPE3, " _
                    & "        NACTSHABAN3, " _
                    & "        NACMANGMORG3, " _
                    & "        NACMANGSORG3, " _
                    & "        NACMANGUORG3, " _
                    & "        NACBASELEASE3, " _
                    & "        NACCREWKBN, " _
                    & "        NACSTAFFCODE, " _
                    & "        NACSTAFFKBN, " _
                    & "        NACMORG, " _
                    & "        NACHORG, " _
                    & "        NACSORG, " _
                    & "        NACSTAFFCODE2, " _
                    & "        NACSTAFFKBN2, " _
                    & "        NACMORG2, " _
                    & "        NACHORG2, " _
                    & "        NACSORG2, " _
                    & "        NACORDERNO, " _
                    & "        NACDETAILNO, " _
                    & "        NACTRIPNO, " _
                    & "        NACDROPNO, " _
                    & "        NACSEQ, " _
                    & "        NACORDERORG, " _
                    & "        NACSHIPORG, " _
                    & "        NACSURYO, " _
                    & "        NACTANI, " _
                    & "        NACJSURYO, " _
                    & "        NACSTANI, " _
                    & "        NACHAIDISTANCE, " _
                    & "        NACKAIDISTANCE, " _
                    & "        NACCHODISTANCE, " _
                    & "        NACTTLDISTANCE, " _
                    & "        NACHAISTDATE, " _
                    & "        NACHAIENDDATE, " _
                    & "        NACHAIWORKTIME, " _
                    & "        NACGESSTDATE, " _
                    & "        NACGESENDDATE, " _
                    & "        NACGESWORKTIME, " _
                    & "        NACCHOWORKTIME, " _
                    & "        NACTTLWORKTIME, " _
                    & "        NACOUTWORKTIME, " _
                    & "        NACBREAKSTDATE, " _
                    & "        NACBREAKENDDATE, " _
                    & "        NACBREAKTIME, " _
                    & "        NACCHOBREAKTIME, " _
                    & "        NACTTLBREAKTIME, " _
                    & "        NACCASH, " _
                    & "        NACETC, " _
                    & "        NACTICKET, " _
                    & "        NACKYUYU, " _
                    & "        NACUNLOADCNT, " _
                    & "        NACCHOUNLOADCNT, " _
                    & "        NACTTLUNLOADCNT, " _
                    & "        NACKAIJI, " _
                    & "        NACJITIME, " _
                    & "        NACJICHOSTIME, " _
                    & "        NACJITTLETIME, " _
                    & "        NACKUTIME, " _
                    & "        NACKUCHOTIME, " _
                    & "        NACKUTTLTIME, " _
                    & "        NACJIDISTANCE, " _
                    & "        NACJICHODISTANCE, " _
                    & "        NACJITTLDISTANCE, " _
                    & "        NACKUDISTANCE, " _
                    & "        NACKUCHODISTANCE, " _
                    & "        NACKUTTLDISTANCE, " _
                    & "        NACTARIFFFARE, " _
                    & "        NACFIXEDFARE, " _
                    & "        NACINCHOFARE, " _
                    & "        NACTTLFARE, " _
                    & "        NACOFFICESORG, " _
                    & "        NACOFFICETIME, " _
                    & "        NACOFFICEBREAKTIME, " _
                    & "        PAYSHUSHADATE, " _
                    & "        PAYTAISHADATE, " _
                    & "        PAYSTAFFKBN, " _
                    & "        PAYSTAFFCODE, " _
                    & "        PAYMORG, " _
                    & "        PAYHORG, " _
                    & "        PAYHOLIDAYKBN, " _
                    & "        PAYKBN, " _
                    & "        PAYSHUKCHOKKBN, " _
                    & "        PAYJYOMUKBN, " _
                    & "        PAYOILKBN, " _
                    & "        PAYSHARYOKBN, " _
                    & "        PAYWORKNISSU, " _
                    & "        PAYSHOUKETUNISSU, " _
                    & "        PAYKUMIKETUNISSU, " _
                    & "        PAYETCKETUNISSU, " _
                    & "        PAYNENKYUNISSU, " _
                    & "        PAYTOKUKYUNISSU, " _
                    & "        PAYCHIKOKSOTAINISSU, " _
                    & "        PAYSTOCKNISSU, " _
                    & "        PAYKYOTEIWEEKNISSU, " _
                    & "        PAYWEEKNISSU, " _
                    & "        PAYDAIKYUNISSU, " _
                    & "        PAYWORKTIME, " _
                    & "        PAYWWORKTIME, " _
                    & "        PAYNIGHTTIME, " _
                    & "        PAYORVERTIME, " _
                    & "        PAYWNIGHTTIME, " _
                    & "        PAYWSWORKTIME, " _
                    & "        PAYSNIGHTTIME, " _
                    & "        PAYSDAIWORKTIME, " _
                    & "        PAYSDAINIGHTTIME, " _
                    & "        PAYHWORKTIME, " _
                    & "        PAYHNIGHTTIME, " _
                    & "        PAYHDAIWORKTIME, " _
                    & "        PAYHDAINIGHTTIME, " _
                    & "        PAYBREAKTIME, " _
                    & "        PAYNENSHINISSU, " _
                    & "        PAYNENMATUNISSU, " _
                    & "        PAYSHUKCHOKNNISSU, " _
                    & "        PAYSHUKCHOKNISSU, " _
                    & "        PAYSHUKCHOKNHLDNISSU, " _
                    & "        PAYSHUKCHOKHLDNISSU, " _
                    & "        PAYTOKSAAKAISU, " _
                    & "        PAYTOKSABKAISU, " _
                    & "        PAYTOKSACKAISU, " _
                    & "        PAYTENKOKAISU, " _
                    & "        PAYHOANTIME, " _
                    & "        PAYKOATUTIME, " _
                    & "        PAYTOKUSA1TIME, " _
                    & "        PAYPONPNISSU, " _
                    & "        PAYBULKNISSU, " _
                    & "        PAYTRAILERNISSU, " _
                    & "        PAYBKINMUKAISU, " _
                    & "        PAYYENDTIME, " _
                    & "        PAYAPPLYID, " _
                    & "        PAYRIYU, " _
                    & "        PAYRIYUETC, " _
                    & "        PAYHAYADETIME, " _
                    & "        PAYHAISOTIME, " _
                    & "        PAYSHACHUHAKNISSU, " _
                    & "        PAYMODELDISTANCE, " _
                    & "        PAYJIKYUSHATIME, " _
                    & "        PAYJYOMUTIME, " _
                    & "        PAYHWORKNISSU, " _
                    & "        PAYKAITENCNT, " _
                    & "        PAYSENJYOCNT, " _
                    & "        PAYUNLOADADDCNT1, " _
                    & "        PAYUNLOADADDCNT2, " _
                    & "        PAYUNLOADADDCNT3, " _
                    & "        PAYUNLOADADDCNT4, " _
                    & "        PAYSHORTDISTANCE1, " _
                    & "        PAYSHORTDISTANCE2, " _
                    & "        APPKIJUN, " _
                    & "        APPKEY, " _
                    & "        WORKKBN, " _
                    & "        KEYSTAFFCODE, " _
                    & "        KEYGSHABAN, " _
                    & "        KEYTRIPNO, " _
                    & "        KEYDROPNO, " _
                    & "        DELFLG, " _
                    & "        INITYMD, " _
                    & "        UPDYMD, " _
                    & "        UPDUSER, " _
                    & "        UPDTERMID, " _
                    & "        RECEIVEYMD " _
                    & " ) " _
                    & " SELECT  " _
                    & "        CAMPCODE, " _
                    & "        MOTOCHO, " _
                    & "        VERSION, " _
                    & "        DENTYPE, " _
                    & "        TENKI, " _
                    & "        KEIJOYMD, " _
                    & "        DENYMD, " _
                    & "        DENNO, " _
                    & "        KANRENDENNO, " _
                    & "        DTLNO, " _
                    & "        INQKBN, " _
                    & "        ACDCKBN, " _
                    & "        ACACHANTEI, " _
                    & "        ACCODE, " _
                    & "        SUBACCODE, " _
                    & "        ACTORICODE, " _
                    & "        ACOILTYPE, " _
                    & "        ACSHARYOTYPE, " _
                    & "        ACTSHABAN, " _
                    & "        ACSTAFFCODE, " _
                    & "        ACBANKAC, " _
                    & "        ACKEIJOMORG, " _
                    & "        ACKEIJOORG, " _
                    & "        ACTAXKBN, " _
                    & "        ACAMT, " _
                    & "        NACSHUKODATE, " _
                    & "        NACSHUKADATE, " _
                    & "        NACTODOKEDATE, " _
                    & "        NACTORICODE, " _
                    & "        NACURIKBN, " _
                    & "        NACTODOKECODE, " _
                    & "        NACSTORICODE, " _
                    & "        NACSHUKABASHO, " _
                    & "        NACTORITYPE01, " _
                    & "        NACTORITYPE02, " _
                    & "        NACTORITYPE03, " _
                    & "        NACTORITYPE04, " _
                    & "        NACTORITYPE05, " _
                    & "        NACOILTYPE, " _
                    & "        NACPRODUCT1, " _
                    & "        NACPRODUCT2, " _
                    & "        NACPRODUCTCODE," _
                    & "        NACGSHABAN, " _
                    & "        NACSUPPLIERKBN, " _
                    & "        NACSUPPLIER, " _
                    & "        NACSHARYOOILTYPE, " _
                    & "        NACSHARYOTYPE1, " _
                    & "        NACTSHABAN1, " _
                    & "        NACMANGMORG1, " _
                    & "        NACMANGSORG1, " _
                    & "        NACMANGUORG1, " _
                    & "        NACBASELEASE1, " _
                    & "        NACSHARYOTYPE2, " _
                    & "        NACTSHABAN2, " _
                    & "        NACMANGMORG2, " _
                    & "        NACMANGSORG2, " _
                    & "        NACMANGUORG2, " _
                    & "        NACBASELEASE2, " _
                    & "        NACSHARYOTYPE3, " _
                    & "        NACTSHABAN3, " _
                    & "        NACMANGMORG3, " _
                    & "        NACMANGSORG3, " _
                    & "        NACMANGUORG3, " _
                    & "        NACBASELEASE3, " _
                    & "        NACCREWKBN, " _
                    & "        NACSTAFFCODE, " _
                    & "        NACSTAFFKBN, " _
                    & "        NACMORG, " _
                    & "        NACHORG, " _
                    & "        NACSORG, " _
                    & "        NACSTAFFCODE2, " _
                    & "        NACSTAFFKBN2, " _
                    & "        NACMORG2, " _
                    & "        NACHORG2, " _
                    & "        NACSORG2, " _
                    & "        NACORDERNO, " _
                    & "        NACDETAILNO, " _
                    & "        NACTRIPNO, " _
                    & "        NACDROPNO, " _
                    & "        NACSEQ, " _
                    & "        NACORDERORG, " _
                    & "        NACSHIPORG, " _
                    & "        NACSURYO, " _
                    & "        NACTANI, " _
                    & "        NACJSURYO, " _
                    & "        NACSTANI, " _
                    & "        NACHAIDISTANCE, " _
                    & "        NACKAIDISTANCE, " _
                    & "        NACCHODISTANCE, " _
                    & "        NACTTLDISTANCE, " _
                    & "        NACHAISTDATE, " _
                    & "        NACHAIENDDATE, " _
                    & "        NACHAIWORKTIME, " _
                    & "        NACGESSTDATE, " _
                    & "        NACGESENDDATE, " _
                    & "        NACGESWORKTIME, " _
                    & "        NACCHOWORKTIME, " _
                    & "        NACTTLWORKTIME, " _
                    & "        NACOUTWORKTIME, " _
                    & "        NACBREAKSTDATE, " _
                    & "        NACBREAKENDDATE, " _
                    & "        NACBREAKTIME, " _
                    & "        NACCHOBREAKTIME, " _
                    & "        NACTTLBREAKTIME, " _
                    & "        NACCASH, " _
                    & "        NACETC, " _
                    & "        NACTICKET, " _
                    & "        NACKYUYU, " _
                    & "        NACUNLOADCNT, " _
                    & "        NACCHOUNLOADCNT, " _
                    & "        NACTTLUNLOADCNT, " _
                    & "        NACKAIJI, " _
                    & "        NACJITIME, " _
                    & "        NACJICHOSTIME, " _
                    & "        NACJITTLETIME, " _
                    & "        NACKUTIME, " _
                    & "        NACKUCHOTIME, " _
                    & "        NACKUTTLTIME, " _
                    & "        NACJIDISTANCE, " _
                    & "        NACJICHODISTANCE, " _
                    & "        NACJITTLDISTANCE, " _
                    & "        NACKUDISTANCE, " _
                    & "        NACKUCHODISTANCE, " _
                    & "        NACKUTTLDISTANCE, " _
                    & "        NACTARIFFFARE, " _
                    & "        NACFIXEDFARE, " _
                    & "        NACINCHOFARE, " _
                    & "        NACTTLFARE, " _
                    & "        NACOFFICESORG, " _
                    & "        NACOFFICETIME, " _
                    & "        NACOFFICEBREAKTIME, " _
                    & "        PAYSHUSHADATE, " _
                    & "        PAYTAISHADATE, " _
                    & "        PAYSTAFFKBN, " _
                    & "        PAYSTAFFCODE, " _
                    & "        PAYMORG, " _
                    & "        PAYHORG, " _
                    & "        PAYHOLIDAYKBN, " _
                    & "        PAYKBN, " _
                    & "        PAYSHUKCHOKKBN, " _
                    & "        PAYJYOMUKBN, " _
                    & "        PAYOILKBN, " _
                    & "        PAYSHARYOKBN, " _
                    & "        PAYWORKNISSU, " _
                    & "        PAYSHOUKETUNISSU, " _
                    & "        PAYKUMIKETUNISSU, " _
                    & "        PAYETCKETUNISSU, " _
                    & "        PAYNENKYUNISSU, " _
                    & "        PAYTOKUKYUNISSU, " _
                    & "        PAYCHIKOKSOTAINISSU, " _
                    & "        PAYSTOCKNISSU, " _
                    & "        PAYKYOTEIWEEKNISSU, " _
                    & "        PAYWEEKNISSU, " _
                    & "        PAYDAIKYUNISSU, " _
                    & "        PAYWORKTIME, " _
                    & "        PAYWWORKTIME, " _
                    & "        PAYNIGHTTIME, " _
                    & "        PAYORVERTIME, " _
                    & "        PAYWNIGHTTIME, " _
                    & "        PAYWSWORKTIME, " _
                    & "        PAYSNIGHTTIME, " _
                    & "        PAYSDAIWORKTIME, " _
                    & "        PAYSDAINIGHTTIME, " _
                    & "        PAYHWORKTIME, " _
                    & "        PAYHNIGHTTIME, " _
                    & "        PAYHDAIWORKTIME, " _
                    & "        PAYHDAINIGHTTIME, " _
                    & "        PAYBREAKTIME, " _
                    & "        PAYNENSHINISSU, " _
                    & "        PAYNENMATUNISSU, " _
                    & "        PAYSHUKCHOKNNISSU, " _
                    & "        PAYSHUKCHOKNISSU, " _
                    & "        PAYSHUKCHOKNHLDNISSU, " _
                    & "        PAYSHUKCHOKHLDNISSU, " _
                    & "        PAYTOKSAAKAISU, " _
                    & "        PAYTOKSABKAISU, " _
                    & "        PAYTOKSACKAISU, " _
                    & "        PAYTENKOKAISU, " _
                    & "        PAYHOANTIME, " _
                    & "        PAYKOATUTIME, " _
                    & "        PAYTOKUSA1TIME, " _
                    & "        PAYPONPNISSU, " _
                    & "        PAYBULKNISSU, " _
                    & "        PAYTRAILERNISSU, " _
                    & "        PAYBKINMUKAISU, " _
                    & "        PAYYENDTIME, " _
                    & "        PAYAPPLYID, " _
                    & "        PAYRIYU, " _
                    & "        PAYRIYUETC, " _
                    & "        PAYHAYADETIME, " _
                    & "        PAYHAISOTIME, " _
                    & "        PAYSHACHUHAKNISSU, " _
                    & "        PAYMODELDISTANCE, " _
                    & "        PAYJIKYUSHATIME, " _
                    & "        PAYJYOMUTIME, " _
                    & "        PAYHWORKNISSU, " _
                    & "        PAYKAITENCNT, " _
                    & "        PAYSENJYOCNT, " _
                    & "        PAYUNLOADADDCNT1, " _
                    & "        PAYUNLOADADDCNT2, " _
                    & "        PAYUNLOADADDCNT3, " _
                    & "        PAYUNLOADADDCNT4, " _
                    & "        PAYSHORTDISTANCE1, " _
                    & "        PAYSHORTDISTANCE2, " _
                    & "        APPKIJUN, " _
                    & "        APPKEY, " _
                    & "        WORKKBN, " _
                    & "        KEYSTAFFCODE, " _
                    & "        KEYGSHABAN, " _
                    & "        KEYTRIPNO, " _
                    & "        KEYDROPNO, " _
                    & "        DELFLG, " _
                    & "        INITYMD, " _
                    & "        UPDYMD, " _
                    & "        UPDUSER, " _
                    & "        UPDTERMID, " _
                    & "        RECEIVEYMD " _
                    & "   FROM " & INSERT_TEMP_TABLE_NAME & ";"

            Dim SQLcmd As New SqlCommand(SQLStr, SQLCON)
            SQLcmd.CommandTimeout = 300
            SQLcmd.ExecuteNonQuery()

            'CLOSE
            SQLcmd.Dispose()
            SQLcmd = Nothing

            ERR = C_MESSAGE_NO.NORMAL

        Catch ex As Exception
            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get
            CS0011LOGWRITE.INFSUBCLASS = "CS0044T6INSERT"               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:L0001_TOKEI Insert"            '
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            ERR = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 部署コードより管理部署コードを取得する
    ''' </summary>
    ''' <param name="ORGCODE"></param>
    ''' <returns></returns>
    Public Function getMOrgCode(ByVal ORGCODE As String) As String
        '●部署情報取得
        Dim MORG As String = ORGCODE
        Try
            '****************
            '*** 共通宣言 ***
            '****************
            'Message検索SQL文
            Dim SQLStr As String =
                     "SELECT " _
                   & "   rtrim(MORGCODE) as MORGCODE " _
                   & " FROM  OIL.M0002_ORG " _
                   & " Where ORGCODE    = @P1 " _
                   & "   and STYMD     <= @P3 " _
                   & "   and ENDYMD    >= @P2 " _
                   & "   and DELFLG    <> @P4 "
            Using SQLcmd As New SqlCommand(SQLStr, SQLcon)
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar, 15)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar, 1)
                PARA1.Value = ORGCODE
                PARA2.Value = Date.Now
                PARA3.Value = Date.Now
                PARA4.Value = C_DELETE_FLG.DELETE
                Dim SQLdr As SqlDataReader = SQLcmd.ExecuteReader()

                If SQLdr.Read Then
                    MORG = SQLdr("MORGCODE")
                    ERR = C_MESSAGE_NO.NORMAL
                Else
                    ERR = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                End If

                'Close
                SQLdr.Close() 'Reader(Close)
                SQLdr = Nothing
            End Using

            Return MORG
        Catch ex As Exception

            Dim CS0011LOGWRITE As New CS0011LOGWrite                    'LogOutput DirString Get

            CS0011LOGWRITE.INFSUBCLASS = "CS0044L1INSERT"               'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "DB:M0002_ORG Select"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            ERR = C_MESSAGE_NO.DB_ERROR
            Return MORG
            Exit Function

        End Try
    End Function

    ''' <summary>
    ''' <para>部署コードより管理部署コードを取得する</para>
    ''' <para>変換対象はNACMORG、NACMORG2</para>
    ''' </summary>
    ''' <param name="IO_TBL"></param>
    Public Sub convNACMORG(ByRef IO_TBL As DataTable)
        For Each IO_ROW As DataRow In IO_TBL.Rows
            IO_ROW("NACMORG") = getMOrgCode(IO_ROW("NACMORG"))
            IO_ROW("NACMORG2") = getMOrgCode(IO_ROW("NACMORG2"))

        Next
    End Sub

End Structure

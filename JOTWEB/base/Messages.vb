Option Strict On
''' <summary>
''' メッセージ関連の定数定義
''' </summary>
Public Module Messages

    ''' <summary>
    ''' メッセージタイプ
    ''' </summary>
    Public Class C_MESSAGE_TYPE
        ''' <summary>
        ''' Normal
        ''' </summary>
        Public Const NOR As String = "N"
        ''' <summary>
        ''' Info
        ''' </summary>
        Public Const INF As String = "I"
        ''' <summary>
        ''' Warning
        ''' </summary>
        Public Const WAR As String = "W"
        ''' <summary>
        ''' Error
        ''' </summary>
        Public Const ERR As String = "E"
        ''' <summary>
        ''' 異常
        ''' </summary>
        Public Const ABORT As String = "A"
        ''' <summary>
        ''' 確認
        ''' </summary>
        Public Const QUES As String = "Q"
    End Class

    ''' <summary>
    ''' メッセージNO
    ''' </summary>
    Public Class C_MESSAGE_NO
        ''' <summary>
        ''' 正常終了時
        ''' </summary>
        Public Const NORMAL As String = "00000"
        ''' <summary>
        ''' システム管理者へ連絡
        ''' </summary>
        Public Const SYSTEM_ADM_ERROR As String = "00001"
        ''' <summary>
        ''' DLL I/F エラー
        ''' </summary>
        Public Const DLL_IF_ERROR As String = "00002"
        ''' <summary>
        ''' DBエラー
        ''' </summary>
        Public Const DB_ERROR As String = "00003"
        ''' <summary>
        ''' File I/Oエラー
        ''' </summary>
        Public Const FILE_IO_ERROR As String = "00004"
        ''' <summary>
        ''' システム起動不能
        ''' </summary>
        Public Const SYSTEM_CANNOT_WAKEUP As String = "00005"
        ''' <summary>
        ''' EXCEL　OPENエラー
        ''' </summary>
        Public Const EXCEL_OPEN_ERROR As String = "00006"
        ''' <summary>
        ''' 型変換エラー
        ''' </summary>
        Public Const CAST_FORMAT_ERROR As String = "00007"
        ''' <summary>
        ''' ディレクトリ未存在
        ''' </summary>
        Public Const DIRECTORY_NOT_EXISTS_ERROR As String = "00008"
        ''' <summary>
        ''' ファイル未存在
        ''' </summary>
        Public Const FILE_NOT_EXISTS_ERROR As String = "00009"
        ''' <summary>
        ''' FIELD名アンマッチ
        ''' </summary>
        Public Const FIELD_NOT_FOUND_ERROR As String = "00010"
        ''' <summary>
        ''' 型変換エラー
        ''' </summary>
        Public Const CAST_FORMAT_ERROR_EX As String = "00011"
        ''' <summary>
        ''' FTP送信エラー
        ''' </summary>
        Public Const FILE_SEND_ERROR As String = "00012"
        ''' <summary>
        ''' ID　パスワード　入力依頼
        ''' </summary>
        Public Const INPUT_ID_PASSWD As String = "10000"
        ''' <summary>
        ''' ID　パスワード　誤入力
        ''' </summary>
        Public Const UNMATCH_ID_PASSWD_ERROR As String = "10001"
        ''' <summary>
        ''' パスワード　期限切れ期間が近い
        ''' </summary>
        Public Const PASSWORD_INVALID_AT_SOON As String = "10002"
        ''' <summary>
        ''' 権限エラー
        ''' </summary>
        Public Const AUTHORIZATION_ERROR As String = "10003"
        ''' <summary>
        ''' サービス停止
        ''' </summary>
        Public Const CLOSED_SERVICE As String = "10004"
        ''' <summary>
        ''' 書式エラー
        ''' </summary>
        Public Const FORMAT_ERROR As String = "10005"
        ''' <summary>
        ''' データ未選択エラー
        ''' </summary>
        Public Const NO_DATA_SELECT_ERROR As String = "10006"
        ''' <summary>
        ''' データ更新エラー（キー変更）
        ''' </summary>
        Public Const PRIMARY_KEY_NO_CHANGE_ERROR As String = "10007"
        ''' <summary>
        ''' マスタ未存在エラー
        ''' </summary>
        Public Const MASTER_NOT_FOUND_ERROR As String = "10008"
        ''' <summary>
        ''' データ重複登録エラー
        ''' </summary>
        Public Const ALREADY_UPDATE_ERROR As String = "10009"
        ''' <summary>
        ''' 印刷用EXCELファイル未存在エラー
        ''' </summary>
        Public Const REPORT_EXCEL_NOT_FOUND_ERROR As String = "10010"
        ''' <summary>
        ''' 帳票ID未存在エラー
        ''' </summary>
        Public Const REPORT_ID_NOT_EXISTS As String = "10011"
        ''' <summary>
        ''' INDEXサポートエラー
        ''' </summary>
        Public Const INDEX_SUPPORT_ERROR As String = "10012"
        ''' <summary>
        ''' 日付書式エラー
        ''' </summary>
        Public Const DATE_FORMAT_ERROR As String = "10013"
        ''' <summary>
        ''' 開始　終了　日付関連エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const START_END_DATE_RELATION_ERROR As String = "10014"
        ''' <summary>
        ''' データ未存在エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const NO_DATA_EXISTS_ERROR As String = "10015"
        ''' <summary>
        ''' 開始終了の関連エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const START_END_RELATION_ERROR As String = "10016"
        ''' <summary>
        ''' BOXエラー存在
        ''' </summary>
        ''' <remarks></remarks>
        Public Const BOX_ERROR_EXIST As String = "10018"
        ''' <summary>
        ''' 登録データ期間重複エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const UPDATE_DATA_RELATION_ERROR As String = "10019"
        ''' <summary>
        ''' 必須項目エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const PREREQUISITE_ERROR As String = "10020"
        ''' <summary>
        ''' 選択無効データ
        ''' </summary>
        Public Const INVALID_SELECTION_DATA As String = "10021"
        ''' <summary>
        ''' 追加可能データ未存在
        ''' </summary>
        Public Const REGISTRATION_RECORD_NOT_EXIST_ERROR As String = "10022"
        ''' <summary>
        ''' 追加不可データ
        ''' </summary>
        Public Const INVALID_REGIST_RECORD_ERROR As String = "10023"
        ''' <summary>
        ''' エラーレコード存在
        ''' </summary>
        Public Const ERROR_RECORD_EXIST As String = "10024"
        ''' <summary>
        ''' 表追加　正常終了
        ''' </summary>
        Public Const TABLE_ADDION_SUCCESSFUL As String = "10025"
        ''' <summary>
        ''' クリア　正常終了
        ''' </summary>
        Public Const DATA_CLEAR_SUCCESSFUL As String = "10026"
        ''' <summary>
        ''' 絞り込み　正常終了
        ''' </summary>
        Public Const DATA_FILTER_SUCCESSFUL As String = "10027"
        ''' <summary>
        ''' DB更新　正常終了
        ''' </summary>
        Public Const DATA_UPDATE_SUCCESSFUL As String = "10028"
        ''' <summary>
        ''' 更新不可データ
        ''' </summary>
        Public Const INVALID_UPDATE_RECORD_ERROR As String = "10029"
        ''' <summary>
        ''' インポートエラー
        ''' </summary>
        Public Const IMPORT_ERROR As String = "10030"
        ''' <summary>
        ''' パスワードの有効期限
        ''' </summary>
        Public Const PASSWORD_VALID_LIMIT As String = "10031"
        ''' <summary>
        ''' 再入力値不一致
        ''' </summary>
        Public Const REINPUT_DATA_UNMATCH_ERROR As String = "10032"
        ''' <summary>
        ''' 数値項目エラー
        ''' </summary>
        Public Const NUMERIC_VALUE_ERROR As String = "10033"
        ''' <summary>
        ''' 整数部桁数超過エラー
        ''' </summary>
        Public Const INTEGER_LENGTH_OVER_ERROR As String = "10034"
        ''' <summary>
        ''' 小数部桁数超過エラー
        ''' </summary>
        Public Const DECIMAL_LENGTH_OVER_ERROR As String = "10035"
        ''' <summary>
        ''' 文字数桁数超過エラー
        ''' </summary>
        Public Const STRING_LENGTH_OVER_ERROR As String = "10036"
        ''' <summary>
        ''' 数値範囲エラー
        ''' </summary>
        Public Const NUMBER_RANGE_ERROR As String = "10037"
        ''' <summary>
        ''' 明細未選択エラー
        ''' </summary>
        Public Const SELECT_DETAIL_ERROR As String = "10038"
        ''' <summary>
        ''' インポート成功
        ''' </summary>
        Public Const IMPORT_SUCCESSFUL As String = "10039"
        ''' <summary>
        ''' 明細表示　正常
        ''' </summary>
        Public Const DETAIL_VIEW_SUCCESSFUL As String = "10040"
        ''' <summary>
        ''' PDF情報は再読込
        ''' </summary>
        Public Const PDF_DATA_REVIEW_SUCCESSFUL As String = "10041"
        ''' <summary>
        ''' 他Excel処理完了待ち
        ''' </summary>
        Public Const WAIT_OTHER_EXCEL_JOB As String = "10042"
        ''' <summary>
        ''' 端末IDエラー
        ''' </summary>
        Public Const INVALID_TERMINAL_ID_ERROR As String = "10043"
        ''' <summary>
        ''' 無効な処理
        ''' </summary>
        Public Const INVALID_PROCCESS_ERROR As String = "10044"
        ''' <summary>
        ''' Excel書式定義エラー
        ''' </summary>
        Public Const EXCEL_COLUMNS_FORMAT_ERROR As String = "10045"
        ''' <summary>
        ''' 集計指定選択
        ''' </summary>
        Public Const SELECT_AGGREGATE_CONDITION As String = "10046"
        ''' <summary>
        ''' 警告レコード存在
        ''' </summary>
        Public Const WORNING_RECORD_EXIST As String = "10047"
        ''' <summary>
        ''' 保持時間超過エラー
        ''' </summary>
        Public Const OVER_RETENTION_PERIOD_ERROR As String = "10048"
        ''' <summary>
        ''' 他車庫の登録操作エラー
        ''' </summary>
        Public Const ANOTHER_SERVER_REGISTLATION_ERROR As String = "10049"
        ''' <summary>
        ''' 更新権限エラー
        ''' </summary>
        Public Const UPDATE_AUTHORIZATION_ERROR As String = "10050"
        ''' <summary>
        ''' 勤怠締後の変更エラー
        ''' </summary>
        Public Const OVER_CLOSING_DATE_ERROR As String = "10051"
        ''' <summary>
        ''' 重複データエラー
        ''' </summary>
        Public Const OVERLAP_DATA_ERROR As String = "10052"
        ''' <summary>
        '''EXCEL UPLOADエラー
        ''' </summary>
        Public Const EXCEL_UPLOAD_ERROR As String = "10053"

        ''' <summary>
        '''データ表示件数オーバー
        ''' </summary>
        Public Const DISPLAY_RECORD_OVER As String = "10054"

        ''' <summary>
        ''' 代行違反エラー
        ''' </summary>
        Public Const ACTING_LOGON_ERROR As String = "10055"

        ''' <summary>
        ''' ファイルアップロードエラー
        ''' </summary>
        Public Const FILE_UPLOAD_ERROR As String = "10056"

        ''' <summary>
        ''' 光英変更データ発生
        ''' </summary>
        Public Const KOUEI_CHANGE_DATA_EXISTS As String = "10057"

        ''' <summary>
        ''' FTP接続エラー
        ''' </summary>
        Public Const FTP_CONNECT_ERROR As String = "11001"

        ''' <summary>
        ''' FTPファイル取得エラー
        ''' </summary>
        Public Const FTP_FILE_GET_ERROR As String = "11002"

        ''' <summary>
        ''' FTPファイル未存在
        ''' </summary>
        Public Const FTP_FILE_NOTFOUND As String = "11003"

        ''' <summary>
        ''' FTPファイルインポート成功
        ''' </summary>
        Public Const FTP_IMPORT_SUCCESSFUL As String = "11004"

        ''' <summary>
        ''' FTPファイル送信エラー
        ''' </summary>
        Public Const FTP_FILE_PUT_ERROR As String = "11005"

        ''' <summary>
        ''' FTPファイル送信データ件数不一致
        ''' </summary>
        Public Const FTP_RECORD_UNMATCH As String = "11006"

        ''' <summary>
        ''' FTPファイル送信成功
        ''' </summary>
        Public Const FTP_EXPORT_SUCCESSFUL As String = "11007"

#Region "石油向け"
        ''' <summary>
        ''' 削除フラグ未存在
        ''' </summary>
        Public Const OIL_DELFLG_NOTFOUND As String = "20000"

        ''' <summary>
        ''' 削除行未存在
        ''' </summary>
        Public Const OIL_DELLINE_NOTFOUND As String = "20001"

        ''' <summary>
        ''' 貨物駅マスタ未存在
        ''' </summary>
        Public Const OIL_STATION_MASTER_NOTFOUND As String = "20002"

        ''' <summary>
        ''' 列車マスタ未存在
        ''' </summary>
        Public Const OIL_TRAIN_MASTER_NOTFOUND As String = "20003"

        ''' <summary>
        ''' 閏年未存在
        ''' </summary>
        Public Const OIL_LEAPYEAR_NOTFOUND As String = "20004"

        ''' <summary>
        ''' 月日範囲エラー
        ''' </summary>
        Public Const OIL_MONTH_DAY_OVER_ERROR As String = "20005"

        ''' <summary>
        ''' 年月日妥当性エラー
        ''' </summary>
        Public Const OIL_DATE_VALIDITY_ERROR As String = "20006"

        ''' <summary>
        ''' 油種別タンク車数入力エラー
        ''' </summary>
        Public Const OIL_OILTANK_INPUT_ERROR As String = "20007"

        ''' <summary>
        ''' タンク車№重複エラー
        ''' </summary>
        Public Const OIL_OILTANKNO_REPEAT_ERROR As String = "20008"

        ''' <summary>
        ''' 入線順序重複エラー
        ''' </summary>
        Public Const OIL_LINEORDER_REPEAT_ERROR As String = "20009"

        ''' <summary>
        ''' 前回油種と油種の整合性エラー
        ''' </summary>
        Public Const OIL_LASTOIL_CONSISTENCY_ERROR As String = "20010"

        ''' <summary>
        ''' 年月日妥当性エラー(現在日付より過去日付)
        ''' </summary>
        Public Const OIL_DATE_PASTDATE_ERROR As String = "20011"

        ''' <summary>
        ''' 一意制約エラー
        ''' </summary>
        Public Const OIL_PRIMARYKEY_REPEAT_ERROR As String = "20012"

        ''' <summary>
        ''' 受注営業所未選択
        ''' </summary>
        Public Const OIL_ORDEROFFICE_UNSELECT As String = "20013"

        ''' <summary>
        ''' タンク車№未割当エラー
        ''' </summary>
        Public Const OIL_TANKNO_MIWARIATE_ERROR As String = "20014"

        ''' <summary>
        ''' 日付関連エラー(利用可能日より（予定）空車着日)
        ''' </summary>
        Public Const OIL_DATE_AVAILABLEDATE_ERROR_Y As String = "20015"

        ''' <summary>
        ''' 列車牽引車数オーバー
        ''' </summary>
        Public Const OIL_TRAINCARS_ERROR As String = "20016"

        ''' <summary>
        ''' 高速列車非対応タンク車エラー
        ''' </summary>
        Public Const OIL_SPEEDTRAINTANK_ERROR As String = "20017"

        ''' <summary>
        ''' 積場スペックエラー
        ''' </summary>
        Public Const OIL_LOADINGSPECS_ERROR As String = "20018"

        ''' <summary>
        ''' 充填ポイント重複エラー
        ''' </summary>
        Public Const OIL_FILLINGPOINT_REPEAT_ERROR As String = "20019"

        ''' <summary>
        ''' 積込可能油種件数オーバー
        ''' </summary>
        Public Const OIL_LOADING_OIL_RECORD_OVER As String = "20020"

        ''' <summary>
        ''' 日付関連エラー(利用可能日より（実績）空車着日)
        ''' </summary>
        Public Const OIL_DATE_AVAILABLEDATE_ERROR_J As String = "20021"

        ''' <summary>
        ''' 荷受人(油槽所)受入油種NG
        ''' </summary>
        Public Const OIL_CONSIGNEE_OILCODE_NG As String = "20022"

        ''' <summary>
        ''' 受注登録内容の重複エラー
        ''' </summary>
        Public Const OIL_ORDER_REPEAT_ERROR As String = "20023"

        ''' <summary>
        ''' 油種数登録ボタン未使用(新規登録時において)
        ''' </summary>
        Public Const OIL_OILREGISTER_NOTUSE As String = "20024"

        ''' <summary>
        ''' タンク車数量エラー
        ''' </summary>
        Public Const OIL_TANKNO_NUMBER_ERROR As String = "20025"
        ''' <summary>
        ''' 受注作成する列車が選ばれていません。
        ''' </summary>
        Public Const OIL_ORDER_NO_CHECKED_ERROR As String = "20026"
        ''' <summary>
        ''' 同一受入予定日の複数受注データが存在するため更新をスキップしました。
        ''' </summary>
        Public Const OIL_ORDER_DUPULICATE_ACCDATE_ERROR As String = "20027"
        ''' <summary>
        ''' 削除データ未存在
        ''' </summary>
        Public Const OIL_DELDATA_NOTFOUND As String = "20028"
        ''' <summary>
        ''' 受注ステータスが手配完了以降に進んでいる為スキップしました。
        ''' </summary>
        Public Const OIL_THIS_ORDER_STATUS_ISNOT_PROC As String = "20029"
        ''' <summary>
        ''' 受注登録可能な提案車数変更がありません。
        ''' </summary>
        Public Const OIL_CANNOT_ENTRY_ORDER As String = "20030"
        ''' <summary>
        ''' 荷主がコスモの車数減少は不可の為、全油種スキップしました。
        ''' </summary>
        Public Const OIL_ASYNC_DELETE_SHIPPER As String = "20031"
        ''' <summary>
        ''' 新規受注№が取得できませんでした。
        ''' </summary>
        Public Const OIL_CANNOT_GET_NEW_ORDERNO As String = "20032"
        ''' <summary>
        ''' 受注キャンセルしますよろしいですか？
        ''' </summary>
        Public Const OIL_CONFIRM_CANCEL_ORDER As String = "20033"
        ''' <summary>
        ''' キャンセルデータ未存在
        ''' </summary>
        Public Const OIL_CANCELDATA_NOTFOUND As String = "20034"
        ''' <summary>
        ''' キャンセル行未存在
        ''' </summary>
        Public Const OIL_CANCELLINE_NOTFOUND As String = "20035"
        ''' <summary>
        ''' 前回揮発油で今回黒油、または灯軽油の整合性エラー
        ''' </summary>
        Public Const OIL_LASTVOLATILEOIL_BLACKLIGHTOIL_ERROR As String = "20036"
        ''' <summary>
        ''' 発送順序重複エラー
        ''' </summary>
        Public Const OIL_SHIPORDER_REPEAT_ERROR As String = "20037"

        ''' <summary>
        ''' 受注作成をスキップした受注提案タンク車数表の日付/列車があります
        ''' </summary>
        Public Const OIL_SKIPPED_ORDER_ENTRIES_EXISTS As String = "20038"
        ''' <summary>
        ''' 受注提案可能な油槽所ですが、取り扱える列車が存在しないため。「受注提案タンク車数表」を非表示にしました。
        ''' </summary>
        Public Const OIL_SUGGEST_TRAIN_NOTEXISTS As String = "20039"
        ''' <summary>
        ''' 取り扱える油種が存在しません。油種マスタの確認をしてください。
        ''' </summary>
        Public Const OIL_STOCK_OILINFO_NOTEXISTS As String = "20040"
        ''' <summary>
        ''' 受注がキャンセルされているため選択できません。
        ''' </summary>
        Public Const OIL_CANCEL_ENTRY_ORDER As String = "20041"
        ''' <summary>
        ''' 回送がキャンセルされているため選択できません。
        ''' </summary>
        Public Const OIL_CANCEL_ENTRY_OUTOFSERVICE As String = "20042"
        ''' <summary>
        ''' 回送キャンセルしますよろしいですか？
        ''' </summary>
        Public Const OIL_CONFIRM_CANCEL_KAISOU As String = "20043"
        ''' <summary>
        ''' 回送営業所未選択
        ''' </summary>
        Public Const OIL_KAISOUOFFICE_UNSELECT As String = "20044"
        ''' <summary>
        ''' タンク所在を更新します。本当によろしいですか？
        ''' </summary>
        Public Const OIL_CONFIRM_UPDATE_TANKLOCATION As String = "20045"
        ''' <summary>
        ''' タンク車(積車)使用メッセージ
        ''' </summary>
        Public Const OIL_TANKNO_LOADING_USE As String = "20046"
        ''' <summary>
        ''' タンク車(発送中)使用メッセージ
        ''' </summary>
        Public Const OIL_TANKNO_SHIPPING_USE As String = "20047"
        ''' <summary>
        ''' 添付ファイル最大数超えメッセージ
        ''' </summary>
        Public Const OIL_ATTACHMENT_COUNTOVER As String = "20048"
        ''' <summary>
        ''' タンク車状態未到着エラー
        ''' </summary>
        Public Const OIL_TANKSTATUS_ERROR As String = "20049"

        ''' <summary>
        ''' 油種数登録ボタン未使用(新規登録時(受注割当時)において)
        ''' </summary>
        Public Const OIL_OILREGISTER_ORDER_NOTUSE As String = "20050"

        ''' <summary>
        ''' タンク車(交検日(過去日))エラー
        ''' </summary>
        Public Const OIL_TANKNO_KOUKENBI_PAST_ERROR As String = "20051"

        ''' <summary>
        ''' タンク車(インフォメーション)メッセージ
        ''' </summary>
        Public Const OIL_TANKNO_INFO_MESSAGE As String = "20052"

        ''' <summary>
        ''' 受注No(警告)メッセージ
        ''' </summary>
        Public Const OIL_ORDERNO_WAR_MESSAGE As String = "20053"

        ''' <summary>
        ''' 受注オーダー時の発日チェック(列車が同一かつ発日が同一の場合)
        ''' </summary>
        Public Const OIL_ORDER_DEPDATE_SAMETRAIN As String = "20054"
        ''' <summary>
        ''' 受注オーダー時の発日チェック(列車が同一かつ発日が同一かつタンク車が複数の場合)
        ''' </summary>
        Public Const OIL_ORDER_DEPDATE_SAMETRAINTANKNO As String = "20055"
        ''' <summary>
        ''' 受注オーダー時の発日チェック(列車が別かつ発日が同一かつタンク車が複数の場合)
        ''' </summary>
        Public Const OIL_ORDER_DEPDATE_DIFFTRAINTANKNO As String = "20056"

#End Region


        Shared Function REPORTID() As String
            Throw New NotImplementedException
        End Function

    End Class
    ''' <summary>
    ''' メッセージの固定文字列
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_MESSAGE_TEXT
        ''' <summary>
        ''' パラメータエラーによるシステム管理者に問い合わせのメッセージ
        ''' </summary>
        ''' <remarks></remarks>
        Public Const IN_PARAM_ERROR_TEXT As String = "システム管理者へ連絡して下さい(In PARAM Err)"
        ''' <summary>
        ''' 選択無効エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const SELECT_INVALID_VALUE_ERROR As String = "選択不可能な値です。"
        ''' <summary>
        ''' 日付書式エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DATE_FORMAT_ERROR_TEXT As String = "日付書式エラー"
        ''' <summary>
        ''' 日付超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DATE_MAX_OVER_ERROR_TEXT As String = "最大日付超（最大：2099/12/31）エラー"
        ''' <summary>
        ''' 時刻書式エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const TIME_FORMAT_ERROR_TEXT As String = "時刻書式エラー"
        ''' <summary>
        ''' 時刻書式エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const TIME_FORMAT_SPLIT_ERROR_TEXT As String = "分単位で入力してください。"
        ''' <summary>
        ''' 必須項目時のエラーメッセージ
        ''' </summary>
        ''' <remarks></remarks>
        Public Const PREREQUISITE_ERROR_TEXT As String = "必須チェックエラー"
        ''' <summary>
        ''' 数値項目エラーメッセージ
        ''' </summary>
        ''' <remarks></remarks>
        Public Const NUMERIC_ERROR_TEXT As String = "数値エラー"
        ''' <summary>
        ''' 整数部桁数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const INTEGER_LENGTH_OVER_ERROR_TEXT As String = "整数桁数エラー"
        ''' <summary>
        ''' 小数部桁数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DECIMAL_LENGTH_OVER_ERROR_TEXT As String = "少数桁数エラー"
        ''' <summary>
        ''' 文字数超過エラー
        ''' </summary>
        ''' <remarks></remarks>
        Public Const STRING_LENGTH_OVER_ERROR_TEXT As String = "文字桁数エラー"
    End Class
    ''' <summary>
    ''' メッセージNOが正常終了か判定する
    ''' </summary>
    ''' <param name="message">判定するメッセージNO</param>
    ''' <param name="O_RTN" >成否判定　TRUE：正常終了　FALSE：それ以外</param>
    ''' <returns>成否判定　TRUE：正常終了　FALSE：それ以外</returns>
    ''' <remarks></remarks>
    Public Function isNormal(ByVal message As String, Optional ByRef O_RTN As String = "TRUE") As Boolean

        If message = C_MESSAGE_NO.NORMAL Then
            isNormal = True
            If Not O_RTN Is Nothing Then
                O_RTN = "TRUE"
            End If
        Else
            isNormal = False
            If Not O_RTN Is Nothing Then
                O_RTN = "FALSE"
            End If
        End If
    End Function
End Module 'End BaseDllConst
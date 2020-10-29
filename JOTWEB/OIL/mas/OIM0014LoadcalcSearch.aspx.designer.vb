'------------------------------------------------------------------------------
' <自動生成>
'     このコードはツールによって生成されました。
'
'     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
'     コードが再生成されるときに損失したりします。 
' </自動生成>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On


Partial Public Class OIM0014LoadcalcSearch
    
    '''<summary>
    '''WF_PLANTCODE コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_PLANTCODE As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_PLANTCODE_TEXT コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_PLANTCODE_TEXT As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''WF_BIGOILCODE コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_BIGOILCODE As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_BIGOILCODE_TEXT コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_BIGOILCODE_TEXT As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''WF_CHECKOILCODE コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_CHECKOILCODE As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''WF_CHECKOILCODE_TEXT コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_CHECKOILCODE_TEXT As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''rightview コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents rightview As Global.JOTWEB.GRIS0003SRightBox
    
    '''<summary>
    '''leftview コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents leftview As Global.JOTWEB.GRIS0005LeftBox
    
    '''<summary>
    '''work コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents work As Global.JOTWEB.OIM0014WRKINC
    
    '''<summary>
    '''WF_FIELD コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_FIELD As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_SelectedIndex コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_SelectedIndex As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_LeftboxOpen コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_LeftboxOpen As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_RightboxOpen コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_RightboxOpen As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_LeftMViewChange コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_LeftMViewChange As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''WF_ButtonClick コントロール。
    '''</summary>
    '''<remarks>
    '''自動生成されたフィールド。
    '''変更するには、フィールドの宣言をデザイナー ファイルから分離コード ファイルに移動します。
    '''</remarks>
    Protected WithEvents WF_ButtonClick As Global.System.Web.UI.HtmlControls.HtmlInputText
    
    '''<summary>
    '''Master プロパティ。
    '''</summary>
    '''<remarks>
    '''自動生成されたプロパティ。
    '''</remarks>
    Public Shadows ReadOnly Property Master() As JOTWEB.OILMasterPage
        Get
            Return CType(MyBase.Master,JOTWEB.OILMasterPage)
        End Get
    End Property
End Class

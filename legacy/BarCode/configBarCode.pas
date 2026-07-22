unit configBarCode;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, cxLookAndFeelPainters, StdCtrls, cxButtons, cxControls,
  cxPC, ExtCtrls, cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxSpinEdit,
  cxDBEdit, cxGraphics, cxDropDownEdit, cxMemo, DB, ZDataSet, DataMatrixBarcode, dmtx,
  cxImageComboBox,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, dxBarBuiltInMenu;

type
  TfrmConfigBarCode = class(TForm)
    btnOk: TcxButton;
    PageControl: TcxPageControl;
    tabContinut: TcxTabSheet;
    tabConfig: TcxTabSheet;
    imgBarcode: TImage;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    edtModuleSize: TcxDBSpinEdit;
    edtMarginSize: TcxDBSpinEdit;
    memText: TcxMemo;
    qryConfig: TZQuery;
    DTConfig: TDataSource;
    btnCreateBarcode: TcxButton;
    cbRotation: TcxDBImageComboBox;
    cbMethod: TcxDBImageComboBox;
    cbScheme: TcxDBImageComboBox;
    cbMatrixSize: TcxDBImageComboBox;
    procedure btnCreateBarcodeClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
    InLoad : Boolean;
  public
    { Public declarations }
  end;


procedure InitDataMatrixOptions;
procedure EncodeDataMatrixImage(text : String; const Bitmap : TBitmap);

var
  frmConfigBarCode: TfrmConfigBarCode;
  GlobalOptions : DatamatrixEncodeOptions;
  Intitalized : Boolean;

implementation

uses
  dateUnit, ZeosDBUtile;

{$R *.dfm}


procedure InitDataMatrixOptions;
var
  aQry : TZReadOnlyQuery;
begin
  if Intitalized then Exit;
  aQry := GetTmpADOQuery;
  with aQry do
  try
    SQL.Add('select top 1 * from BARCODE_CONFIG');
    Open;
    GlobalOptions:=InitializeDatamatrixEncodeOptions;
    GlobalOptions.moduleSize:= aQry.FieldByName('MODULE_SIZE').AsInteger;
    GlobalOptions.marginSize:= aQry.FieldByName('MARGIN_SIZE').AsInteger;
    GlobalOptions.rotate:=90*aQry.FieldByName('ROTATE').AsInteger;
    GlobalOptions.method:=DmtxEncodeMethod(aQry.FieldByName('METHOD').AsInteger);
    GlobalOptions.scheme:=DmtxSchemeEncode(aQry.FieldByName('SCHEME').AsInteger);
    GlobalOptions.sizeIdx:=aQry.FieldByName('MATRIX_SIZE').AsInteger-2;
    GlobalOptions.mosaic:= False;
    Intitalized := True;
  finally
    Free;
  end;
end;

procedure EncodeDataMatrixImage(text : String; const Bitmap : TBitmap);
begin
  if not Intitalized then InitDataMatrixOptions;
  EncodeDatamatrix(text,Bitmap,GlobalOptions);
end;

procedure TfrmConfigBarCode.btnCreateBarcodeClick(Sender: TObject);
//var options:DatamatrixEncodeOptions;
begin
  if InLoad then Exit;
  if DBIsInEdit(qryConfig) and DBPost(qryConfig) then begin
    DBGoEdit(qryConfig);
    Intitalized := False;
  end;

  {
  options:=InitializeDatamatrixEncodeOptions;
  options.moduleSize:= qryConfig.FieldByName('MODULE_SIZE').AsInteger;
  options.marginSize:= qryConfig.FieldByName('MARGIN_SIZE').AsInteger;
  options.rotate:=90*qryConfig.FieldByName('ROTATE').AsInteger;
  options.method:=DmtxEncodeMethod(qryConfig.FieldByName('METHOD').AsInteger);
  options.scheme:=DmtxSchemeEncode(qryConfig.FieldByName('SCHEME').AsInteger);
  options.sizeIdx:=qryConfig.FieldByName('MATRIX_SIZE').AsInteger-2;
  options.mosaic:= False;
  EncodeDatamatrix(memText.Text,imgBarcode.Picture.Bitmap,options);
  }
  EncodeDataMatrixImage(memText.Text,imgBarcode.Picture.Bitmap);
end;


procedure TfrmConfigBarCode.FormCreate(Sender: TObject);
begin
  InLoad := True;
  DBRefresh(qryConfig);
  InLoad := False;
  btnCreateBarcodeClick(nil);
end;

initialization
  Intitalized := False;
end.

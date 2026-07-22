unit IntretinereTipProduse;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, 
  cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls, cxStyles,
  cxGraphics, cxDataStorage, cxEdit, DB,
  cxDBData, cxGridLevel, cxClasses, cxControls, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxVGrid, cxContainer, cxGroupBox, cxGridCustomPopupMenu,
  cxGridPopupMenu, cxInplaceContainer, cxDBVGrid, ZDataSet, dxPScxCommon,
  dxPScxVGridLnk, cxNavigator, cxDBNavigator, AppEvnts, cxTextEdit, cxMemo,
  cxCheckBox, ImgList, Menus, 
  cxSplitter, DegradePanel, 
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, dxDateRanges,
  dxScrollbarAnnotations, dxBarBuiltInMenu;

type
  TfrmIntretinereTipProd = class(TForm)
    pnContent: TPanel;
    cxGridTipProdusDBTableView1: TcxGridDBTableView;
    cxGridTipProdusLevel1: TcxGridLevel;
    cxGridTipProdus: TcxGrid;
    cxGroupBox1: TcxGroupBox;
    cxDBVerticalGrid1: TcxDBVerticalGrid;
    DTProdus: TDataSource;
    QryTipProdus: TZQuery;
    cxDBVerticalGrid1ID_GEST_TIP_PRODUSE: TcxDBEditorRow;
    cxDBVerticalGrid1TIP_PRODUS: TcxDBEditorRow;
    cxDBVerticalGrid1DENUMIRE: TcxDBEditorRow;
    cxDBVerticalGrid1SE_AFISEAZA: TcxDBEditorRow;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    cxStyle6: TcxStyle;
    cxStyle7: TcxStyle;
    cxStyle8: TcxStyle;
    cxStyle9: TcxStyle;
    cxStyle10: TcxStyle;
    cxGridTipProdusDBTableView1ID_GEST_TIP_PRODUSE: TcxGridDBColumn;
    cxGridTipProdusDBTableView1TIP_PRODUS: TcxGridDBColumn;
    cxGridTipProdusDBTableView1DENUMIRE: TcxGridDBColumn;
    cxGridTipProdusDBTableView1SE_AFISEAZA: TcxGridDBColumn;
    cxDBVerticalGrid1CategoryRow1: TcxCategoryRow;
    cxStyle11: TcxStyle;
    cxStyle12: TcxStyle;
    cxStyle13: TcxStyle;
    cxStyle14: TcxStyle;
    cxStyle15: TcxStyle;
    cxStyle16: TcxStyle;
    cxStyle17: TcxStyle;
    cxStyle18: TcxStyle;
    cxStyle19: TcxStyle;
    cxStyle20: TcxStyle;
    cxStyle21: TcxStyle;
    cxStyle22: TcxStyle;
    GridTableViewStyleSheetUserFormat4: TcxGridTableViewStyleSheet;
    cxStyle23: TcxStyle;
    cxStyle24: TcxStyle;
    cxStyle25: TcxStyle;
    cxStyle26: TcxStyle;
    cxStyle27: TcxStyle;
    cxStyle28: TcxStyle;
    cxStyle29: TcxStyle;
    cxStyle30: TcxStyle;
    cxStyle31: TcxStyle;
    cxStyle32: TcxStyle;
    cxStyle33: TcxStyle;
    cxStyle34: TcxStyle;
    cxStyle35: TcxStyle;
    cxStyle36: TcxStyle;
    cxStyle37: TcxStyle;
    cxStyle38: TcxStyle;
    cxStyle39: TcxStyle;
    cxStyle40: TcxStyle;
    cxStyle41: TcxStyle;
    cxStyle42: TcxStyle;
    cxStyle43: TcxStyle;
    cxStyle44: TcxStyle;
    cxStyle45: TcxStyle;
    cxStyle46: TcxStyle;
    cxStyle47: TcxStyle;
    cxStyle48: TcxStyle;
    cxStyle49: TcxStyle;
    cxStyle50: TcxStyle;
    cxStyle51: TcxStyle;
    cxStyle52: TcxStyle;
    cxStyle53: TcxStyle;
    cxStyle54: TcxStyle;
    cxStyle55: TcxStyle;
    cxStyle56: TcxStyle;
    cxVerticalGridStyleSheetStormVGA: TcxVerticalGridStyleSheet;
    cxStyle57: TcxStyle;
    cxStyle58: TcxStyle;
    cxStyle59: TcxStyle;
    cxStyle60: TcxStyle;
    cxStyle61: TcxStyle;
    cxStyle62: TcxStyle;
    cxDBNavigator1: TcxDBNavigator;
    ConfMenu: TPopupMenu;
    mnuConfigureazaTipMat: TMenuItem;
    mnuDeseleteazaTot: TMenuItem;
    ImgList: TImageList;
    mnuInfluentaStock: TMenuItem;
    btnOk: TcxButton;
    cxGridPopupMenu: TcxGridPopupMenu;
    pnBottom: TPanel;
    cxSplitter1: TcxSplitter;
    pnTop: TDegradePanel;
    btnAddProdus: TcxButton;
    btnStergeProd: TcxButton;
    procedure btnAddProdusClick(Sender: TObject);
    procedure btnStergeProdClick(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmIntretinereTipProd: TfrmIntretinereTipProd;

implementation

uses DateUnit;

{$R *.dfm}

procedure TfrmIntretinereTipProd.btnAddProdusClick(Sender: TObject);
begin
  //
  with QryTipProdus do
    try
      Append;
      FieldByName('DENUMIRE').AsString := 'Produs Nou';
      FieldByName('SE_AFISEAZA').AsBoolean := False;
      Post;
      Edit;
    finally
    end;
end;

procedure TfrmIntretinereTipProd.btnStergeProdClick(Sender: TObject);
begin
//
  if (MessageDlg(
     Format('Doriti stergerea produsului %s (%d)', [ QryTipProdus.FieldByName('DENUMIRE').AsString , QryTipProdus.FieldByName('ID_GEST_TIP_PRODUSE').AsInteger ]),
     mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
  QryTipProdus.Delete;
end;

procedure TfrmIntretinereTipProd.btnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmIntretinereTipProd.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if QryTipProdus.State in [dsEdit, dsInsert] then QryTipProdus.Post;
  
    Action := caFree;
end;

procedure TfrmIntretinereTipProd.FormCreate(Sender: TObject);
begin
  if QryTipProdus.Active then
     QryTipProdus.Close;
  QryTipProdus.Open;
  //WindowState := wsMaximized;  
end;

procedure TfrmIntretinereTipProd.pnBottomResize(Sender: TObject);
begin
   btnOk.Left := pnBottom.Width - btnOk.Width - 5;
end;

end.

unit frmContaSolduriInitialeUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics,
  Controls, Forms, Dialogs, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxSplitter, dxLayoutContainer, dxLayoutControl,
  cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage, cxEdit, DB,
  cxDBData, cxContainer, Menus, StdCtrls, ActnList, ZAbstractRODataset,
  ZAbstractDataset, ZDataset, dxmdaset, cxButtons, cxTextEdit, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, ExtCtrls, cxGridLevel,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxClasses,
  cxGridCustomView, cxGrid;

type
  TfrmContaSolduriInitiale = class(TForm)
    DTSolduriComplet: TDataSource;
    qrySolduri: TZQuery;
    gridLstSolduri: TcxGrid;
    gridViewSolduri: TcxGridDBTableView;
    gridViewSoldurinume: TcxGridDBColumn;
    gridViewSoldurian: TcxGridDBColumn;
    gridViewSolduricont: TcxGridDBColumn;
    gridViewSoldurisold_debitor: TcxGridDBColumn;
    gridViewSoldurisold_creditor: TcxGridDBColumn;
    gridSolduriLevel: TcxGridLevel;
    pnTop: TPanel;
    edtCont: TcxImageComboBox;
    edtAnFiscal: TcxImageComboBox;
    ActiuniSI: TActionList;
    actAdauga: TAction;
    actSterge: TAction;
    actModifica: TAction;
    pnBottom: TPanel;
    lcBasic: TdxLayoutControl;
    dxLayoutGroup1: TdxLayoutGroup;
    Split: TcxSplitter;
    Panel1: TPanel;
    cxButton1: TcxButton;
    cxButton2: TcxButton;
    cxButton3: TcxButton;
    Label1: TLabel;
    Label2: TLabel;
    procedure FormCreate(Sender: TObject);
  private
    FAnFiscal: Integer;
    FReadOnly: Boolean;
    FCont: string;
    procedure SetAnFiscal(const Value: Integer);
    procedure SetReadOnly(const Value: Boolean);
    procedure SetCont(const Value: string);
    { Private declarations }
  public
    { Public declarations }
    procedure OpenDataSet;
    procedure SetupColumns;
    property Cont : string read FCont write SetCont;
    property AnFiscal : Integer read FAnFiscal write SetAnFiscal;
    property ReadOnly : Boolean read FReadOnly write SetReadOnly;
  end;


implementation

uses
  ZeosDBUtile, DateUnit;

{$R *.dfm}

{ TfrmContaSolduriInitiale }

procedure TfrmContaSolduriInitiale.OpenDataSet;
begin
  qrySolduri.Close;
  qrySolduri.ParamByName('cont').Value := edtCont.EditValue;
  qrySolduri.Open;
end;

procedure TfrmContaSolduriInitiale.SetAnFiscal(const Value: Integer);
begin
  FAnFiscal := Value;
end;

procedure TfrmContaSolduriInitiale.SetCont(const Value: string);
begin
  FCont := Value;
end;

procedure TfrmContaSolduriInitiale.SetReadOnly(const Value: Boolean);
begin
  FReadOnly := Value;
  lcBasic.Enabled := FReadOnly;
end;

procedure TfrmContaSolduriInitiale.SetupColumns;
var
  lDataSet  : TDataSet;
  lColumn   : TcxGridDBColumn;
begin
  lDataSet := DBNewQuery('exec spCNoteSoldColumns');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lColumn := gridViewSolduri.CreateColumn;
      lColumn.DataBinding.FieldName := lDataSet.FieldByName('fieldName').AsString;
      lColumn.Caption               := lDataSet.FieldByName('fieldCaption').AsString;
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmContaSolduriInitiale.FormCreate(Sender: TObject);
begin
  SetupColumns;
end;

end.

unit RepartitorContBanca;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms, DB,
  Dialogs, ExtCtrls, DegradePanel, StdCtrls, 
  cxGraphics, cxCheckBox, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, cxDBEdit, cxControls, cxContainer,
  cxEdit, cxTextEdit, cxMRUEdit, Menus, cxLookAndFeelPainters, cxButtons,
  cxLookAndFeels;

type
  TfrmRepartitorContBanca = class(TForm)
    pnTop: TDegradePanel;
    pnContent: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    edCont: TcxDBTextEdit;
    edTipValuta: TcxDBImageComboBox;
    edBancaDenumire: TcxDBMRUEdit;
    edBancaDenumireScurta: TcxDBMRUEdit;
    edBancaAdresa: TcxDBMRUEdit;
    edBancaCod: TcxDBMRUEdit;
    edContDefault: TcxDBCheckBox;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    procedure BtnCancelClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure edBancaDenumirePropertiesButtonClick(Sender: TObject);
    procedure edBancaAdresaPropertiesButtonClick(Sender: TObject);
    procedure edBancaDenumireScurtaPropertiesButtonClick(Sender: TObject);
  private
    FDateCont: TDataSource;
    procedure SetDataCont(const Value: TDataSource);
    { Private declarations }
  public
    { Public declarations }
    property DateCont : TDataSource read FDateCont write SetDataCont;
  end;


procedure EditRepartitorCont(FData : TDataSource);


implementation

uses
  ZeosDBUtile, dxCompsUtile, DateUnit, ZDataSet, CommonDBVar, OERepartitoriUnit;

{$R *.dfm}

procedure EditRepartitorCont(FData : TDataSource);
var
  lfrmRepartitorContBanca: TfrmRepartitorContBanca;
begin
  lfrmRepartitorContBanca := TfrmRepartitorContBanca.Create(nil);
  with lfrmRepartitorContBanca do
    try
      DateCont := FData;
      ShowModal;
      if ModalResult = mrOk then begin
        if FData.DataSet.State in [dsEdit, dsInsert] then FData.DataSet.Post;
      end
      else begin
        Abort;
      end;
    finally
      Free;
    end;
end;

procedure TfrmRepartitorContBanca.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmRepartitorContBanca.BtnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmRepartitorContBanca.SetDataCont(const Value: TDataSource);
begin
  FDateCont := Value;
  edCont.DataBinding.DataSource := FDateCont;
  edTipValuta.DataBinding.DataSource := FDateCont;
  edBancaDenumire.DataBinding.DataSource := FDateCont;
  edBancaDenumireScurta.DataBinding.DataSource := FDateCont;
  edBancaAdresa.DataBinding.DataSource := FDateCont;
  edBancaCod.DataBinding.DataSource := FDateCont;
  edContDefault.DataBinding.DataSource := FDateCont;
end;

procedure TfrmRepartitorContBanca.FormCreate(Sender: TObject);
begin
  FillImageCombo(edTipValuta.Properties, 'spNmclValute', 0, 1);
  FillMRUCombo(edBancaDenumire.Properties, 'SELECT DISTINCT BANCA_DENUMIRE FROM REPARTITORI_CONTURI WHERE  BANCA_DENUMIRE IS NOT NULL', 0);
  FillMRUCombo(edBancaDenumireScurta.Properties, 'SELECT DISTINCT BANCA_DENUMIRE_SCURTA FROM REPARTITORI_CONTURI WHERE BANCA_DENUMIRE_SCURTA IS NOT NULL', 0);
  FillMRUCombo(edBancaAdresa.Properties, 'SELECT DISTINCT BANCA_ADRESA FROM REPARTITORI_CONTURI WHERE BANCA_ADRESA IS NOT NULL', 0);
  FillMRUCombo(edBancaCod.Properties, 'SELECT DISTINCT BANCA_COD FROM REPARTITORI_CONTURI WHERE BANCA_COD IS NOT NULL', 0);
end;

procedure TfrmRepartitorContBanca.edBancaDenumirePropertiesButtonClick(
  Sender: TObject);
var Id : Integer;
    Den, Adresa, DenScurta : String;
begin
   SelectRepartitor(Id, Den, Adresa, DenScurta);
   if Id <> - 1 then begin
      if not (DateCont.DataSet.State in [dsEdit, dsInsert]) then DateCont.DataSet.Edit;
      DateCont.DataSet.FieldByName('ID_BANCA').AsInteger := Id;
      DateCont.DataSet.FieldByName('BANCA_DENUMIRE').AsString := Den;
      if DenScurta <> '' then
        DateCont.DataSet.FieldByName('BANCA_DENUMIRE_SCURTA').AsString := DenScurta
      else
        DateCont.DataSet.FieldByName('BANCA_DENUMIRE_SCURTA').AsString := Den;
      DateCont.DataSet.FieldByName('BANCA_ADRESA').AsString := Adresa;
      DateCont.DataSet.Post;
   end;
end;

procedure TfrmRepartitorContBanca.edBancaAdresaPropertiesButtonClick(
  Sender: TObject);
var
  IdRep   : Integer;
  lAdresa : String;
begin
  IdRep := DateCont.DataSet.FieldByName('ID_BANCA').AsInteger;
  if IdRep > 0 then begin
    lAdresa := DBGetScallar('SELECT ADRESA FROM REPARTITORI WHERE ID_REPARTITORI = ' + IntToStr(IdRep));
    if not (DateCont.DataSet.State in [dsEdit, dsInsert]) then DateCont.DataSet.Edit;
    DateCont.DataSet.FieldByName('BANCA_ADRESA').AsString := lAdresa;
    DateCont.DataSet.Post;
  end;
end;

procedure TfrmRepartitorContBanca.edBancaDenumireScurtaPropertiesButtonClick(
  Sender: TObject);
var
  IdRep : Integer;
  lAdresa: String;
begin
  IdRep := DateCont.DataSet.FieldByName('ID_BANCA').AsInteger;
  if IdRep > 0 then begin
    lAdresa := DBGetScallar('SELECT ISNULL(CODSECTIE, NUME) FROM REPARTITORI WHERE ID_REPARTITORI = ' + IntToStr(IdRep));
    if not (DateCont.DataSet.State in [dsEdit, dsInsert]) then DateCont.DataSet.Edit;
    DateCont.DataSet.FieldByName('BANCA_ADRESA').AsString := lAdresa;
    DateCont.DataSet.Post;
  end;
end;

end.

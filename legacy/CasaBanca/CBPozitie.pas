unit CBPozitie;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxPC,
  ExtCtrls, cxControls, StdCtrls, cxContainer, cxEdit, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, cxCurrencyEdit,
  cxDBEdit, cxImageComboBox, cxCalendar, cxCheckBox, cxMemo, cxGroupBox,
  DB, 
  cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, ZDataSet, 
  cxButtons, cxLookAndFeelPainters, cxGraphics, 
  cxDataStorage, cxDBData, Menus,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData;

type
  TfrmCBPozitie = class(TForm)
    PageRegistru: TcxPageControl;
    tabGeneral: TcxTabSheet;
    tabDocConex: TcxTabSheet;
    tabContabil: TcxTabSheet;
    gbInfo: TcxGroupBox;
    Label1: TLabel;
    edtTipDoc: TcxDBImageComboBox;
    Label2: TLabel;
    edtNrDoc: TcxDBTextEdit;
    Label3: TLabel;
    edtDataDoc: TcxDBDateEdit;
    Label4: TLabel;
    edtEsteValuta: TcxDBCheckBox;
    Label6: TLabel;
    edtDataCursValutar: TcxDBDateEdit;
    Label7: TLabel;
    edtTipValuta: TcxDBImageComboBox;
    Label8: TLabel;
    edtValoareValuta: TcxDBCurrencyEdit;
    Label9: TLabel;
    edtExplicatie: TcxDBMemo;
    cxGroupBox1: TcxGroupBox;
    edtCont: TcxDBPopupEdit;
    Label10: TLabel;
    edtRep: TcxDBPopupEdit;
    edtFCT: TcxDBPopupEdit;
    Label11: TLabel;
    Label12: TLabel;
    GridConta: TcxGridDBTableView;
    GridContaL: TcxGridLevel;
    cxGridConta: TcxGrid;
    qryContabil: TZQuery;
    qryRegistru: TZQuery;
    DTContabil: TDataSource;
    DTRegistru: TDataSource;
    gbInfoEntitate: TcxGroupBox;
    Label13: TLabel;
    Label14: TLabel;
    edContCasa: TcxDBPopupEdit;
    edDenCasa: TcxDBImageComboBox;
    edValoare: TcxDBCurrencyEdit;
    GridContaID_CB_CONTABIL: TcxGridDBColumn;
    GridContaID_CB_REGISTRU: TcxGridDBColumn;
    GridContaESTE_PLATA: TcxGridDBColumn;
    GridContaVALOARE: TcxGridDBColumn;
    GridContaEXPLICATIE: TcxGridDBColumn;
    GridContaCONT_DEBIT: TcxGridDBColumn;
    GridContaREPARTITOR_DEBIT: TcxGridDBColumn;
    GridContaCONT_CREDIT: TcxGridDBColumn;
    GridContaREPARTITOR_CREDIT: TcxGridDBColumn;
    GridContaCOD_FUNCTIONAL: TcxGridDBColumn;
    GridContaCOD_ECONOMIC: TcxGridDBColumn;
    GridContaID_UTILIZATOR: TcxGridDBColumn;
    GridContaSTARE: TcxGridDBColumn;
    GridContaID_CNOTE: TcxGridDBColumn;
    GridContaID_TCV: TcxGridDBColumn;
    GridContaID_CASA: TcxGridDBColumn;
    edtEstePlata: TcxDBCheckBox;
    edtEsteIncasare: TcxDBCheckBox;
    pnBottom: TPanel;
    btnAdd: TcxButton;
    cxButton2: TcxButton;
    cxButton3: TcxButton;
    edtCursSchimb: TcxDBCurrencyEdit;
    Label5: TLabel;
    procedure qryContabilNewRecord(DataSet: TDataSet);
  private
    FCodPozitie: Integer;
    procedure SetCodPozitie(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    property CodPozitie : Integer read FCodPozitie write SetCodPozitie;
  end;


  procedure EditarePozitie(IdPoz : Integer);
  procedure AdaugarePozitie(const CodEntitate : Integer = -1; const este_plata : Boolean = True);

var
  frmCBPozitie: TfrmCBPozitie;

implementation

uses
  dateUnit, CommonDBVar;

{$R *.dfm}

procedure EditarePozitie(IdPoz : Integer);
begin
  with TfrmCBPozitie.Create(nil) do
    try
      CodPozitie := IdPoz;
      ShowModal;
    finally
      Free;
    end;
end;


procedure AdaugarePozitie;
var
  idPoz : Integer;
begin
  with GetTmpADOQuery do
  try
    SQL.Add('exec spCBAddNewRecord ' + IntToStr(IdUtilizator) + ', :IdEntitate, :EstePlata');
    if CodEntitate = -1 then
      Params.ParamByName('IdEntitate').Value := Null
    else
      Params.ParamByName('IdEntitate').Value := CodEntitate;
    Params.ParamByName('EstePlata').Value :=   Abs(Integer(este_plata));
    Open;
    idPoz := Fields[0].AsInteger;
  finally
    Free;
  end;
  if idPoz <> -1 then
    EditarePozitie(idPoz);
end;



{ TfrmCBPozitie }

procedure TfrmCBPozitie.SetCodPozitie(const Value: Integer);
begin
  FCodPozitie := Value;
  DoCheckClose(qryRegistru);
  DoCheckClose(qryContabil);
  qryRegistru.Params.ParamByName('ID_CB_REGISTRU').Value := FCodPozitie;
  qryContabil.Params.ParamByName('ID_CB_REGISTRU').Value := FCodPozitie;
  DoCheckOpen(qryRegistru);
  DoCheckOpen(qryContabil);  
end;

procedure TfrmCBPozitie.qryContabilNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_CB_REGISTRU').AsInteger := FCodPozitie;
end;


end.

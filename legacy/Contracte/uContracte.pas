unit uContracte;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, DegradePanel, cxGraphics, cxLookAndFeels,
  cxLookAndFeelPainters, Menus, StdCtrls, cxButtons, cxControls, cxStyles,
  cxCustomData, cxFilter, cxData, cxDataStorage, cxEdit, DB, cxDBData,
  cxCurrencyEdit, cxGridLevel, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxClasses, cxGridCustomView, cxGrid, cxContainer,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxImageComboBox,
  ZAbstractRODataset, ZAbstractDataset, ZDataset, dxLayoutContainer,
  cxGridCardView, cxGridDBCardView, dxLayoutcxEditAdapters,
  dxLayoutControl, cxCalc, cxDBEdit,
  cxGridCustomPopupMenu, cxGridPopupMenu, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmContracte = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    btnCancel: TcxButton;
    btnRaportare: TcxButton;
    Panel1: TPanel;
    cxGridContracte: TcxGrid;
    Contracte: TcxGridDBTableView;
    Aditionale: TcxGridDBTableView;
    Level1: TcxGridLevel;
    Level2: TcxGridLevel;
    Panel2: TPanel;
    Label3: TLabel;
    Label1: TLabel;
    edOperator: TcxImageComboBox;
    edProiect: TcxImageComboBox;
    BtnModificare: TcxButton;
    btnAnuleaza: TcxButton;
    btnRefresh: TcxButton;
    btnNew: TcxButton;
    qryOperatori: TZQuery;
    qryProiecte: TZQuery;
    DTAditional: TDataSource;
    qryAditional: TZQuery;
    DTContract: TDataSource;
    qryContract: TZQuery;
    ContracteID_CONTRACTE: TcxGridDBColumn;
    ContracteID_PARINTE: TcxGridDBColumn;
    ContracteNR_CONTRACT: TcxGridDBColumn;
    ContracteDATA_CONTRACT: TcxGridDBColumn;
    ContracteID_PRESTATOR: TcxGridDBColumn;
    ContracteID_BENEFICIAR: TcxGridDBColumn;
    ContracteVALOARE_VALUTA: TcxGridDBColumn;
    ContracteID_TIP_VALUTA: TcxGridDBColumn;
    ContracteCURS_VALUTAR: TcxGridDBColumn;
    ContracteVALOARE: TcxGridDBColumn;
    ContracteDURATA_CONTRACT: TcxGridDBColumn;
    ContracteDATA_SEMNARE: TcxGridDBColumn;
    ContracteDATA_INCEPUT: TcxGridDBColumn;
    ContracteDATA_SFARSIT: TcxGridDBColumn;
    ContracteARE_ADITIONAL: TcxGridDBColumn;
    ContracteARE_LITIGII: TcxGridDBColumn;
    ContracteID_OI_PROIECTE: TcxGridDBColumn;
    ContracteGARANTIE_PROCENT: TcxGridDBColumn;
    ContracteGARANTIE_DATA_EXPIRARE: TcxGridDBColumn;
    ContracteGARANTIE_VALOARE: TcxGridDBColumn;
    ContracteGARANTIE_CONTITUITA: TcxGridDBColumn;
    ContracteGARANTIE_VARSATA: TcxGridDBColumn;
    ContracteACHIZITIE_NR_DOSAR: TcxGridDBColumn;
    ContracteACHIZITIE_DATA_DOSAR: TcxGridDBColumn;
    ContracteTOTAL_VALOARE_VALUTA: TcxGridDBColumn;
    ContracteTOTAL_VALOARE: TcxGridDBColumn;
    ContracteTOTAL_GARANTIE: TcxGridDBColumn;
    ContracteTOTAL_GARANTIE_CONSTITUITA: TcxGridDBColumn;
    ContracteTOTAL_GARANTIE_VARSATA: TcxGridDBColumn;
    ContracteSTARE: TcxGridDBColumn;
    ContracteID_UTILIZATORI: TcxGridDBColumn;
    ContracteDATA_OPERARE: TcxGridDBColumn;
    ContracteDATA_MODIFICARE: TcxGridDBColumn;
    ContracteDATA_STERGERE: TcxGridDBColumn;
    ContractePrestator: TcxGridDBColumn;
    ContracteBeneficiar: TcxGridDBColumn;
    ContracteSimbolValuta: TcxGridDBColumn;
    ContracteDenumireProiect: TcxGridDBColumn;
    qryDate: TZQuery;
    lyContract: TdxLayoutControl;
    edTotalValuta: TcxDBCalcEdit;
    edTotal: TcxDBCalcEdit;
    edGarantieTotal: TcxDBCalcEdit;
    edGarantieConstituita: TcxDBCalcEdit;
    edGarantieVarsata: TcxDBCalcEdit;
    lyContractGroup_Root1: TdxLayoutGroup;
    Totalizare: TdxLayoutGroup;
    lyContractItem15: TdxLayoutItem;
    lyContractItem16: TdxLayoutItem;
    lyContractGroup3: TdxLayoutGroup;
    lyContractItem17: TdxLayoutItem;
    lyContractItem18: TdxLayoutItem;
    lyContractItem19: TdxLayoutItem;
    AditionaleID_CONTRACTE: TcxGridDBColumn;
    AditionaleID_PARINTE: TcxGridDBColumn;
    AditionaleNR_CONTRACT: TcxGridDBColumn;
    AditionaleDATA_CONTRACT: TcxGridDBColumn;
    AditionaleID_PRESTATOR: TcxGridDBColumn;
    AditionaleID_BENEFICIAR: TcxGridDBColumn;
    AditionaleVALOARE_VALUTA: TcxGridDBColumn;
    AditionaleID_TIP_VALUTA: TcxGridDBColumn;
    AditionaleCURS_VALUTAR: TcxGridDBColumn;
    AditionaleVALOARE: TcxGridDBColumn;
    AditionaleDURATA_CONTRACT: TcxGridDBColumn;
    AditionaleDATA_SEMNARE: TcxGridDBColumn;
    AditionaleDATA_INCEPUT: TcxGridDBColumn;
    AditionaleDATA_SFARSIT: TcxGridDBColumn;
    AditionaleID_OI_PROIECTE: TcxGridDBColumn;
    AditionaleACHIZITIE_NR_DOSAR: TcxGridDBColumn;
    AditionaleACHIZITIE_DATA_DOSAR: TcxGridDBColumn;
    AditionaleSTARE: TcxGridDBColumn;
    AditionaleID_UTILIZATORI: TcxGridDBColumn;
    AditionaleDATA_OPERARE: TcxGridDBColumn;
    AditionaleDATA_MODIFICARE: TcxGridDBColumn;
    AditionaleDATA_STERGERE: TcxGridDBColumn;
    AditionalePrestator: TcxGridDBColumn;
    AditionaleBeneficiar: TcxGridDBColumn;
    AditionaleSimbolValuta: TcxGridDBColumn;
    AditionaleDenumireProiect: TcxGridDBColumn;
    lyContractSplitterItem1: TdxLayoutSplitterItem;
    lyContractSplitterItem2: TdxLayoutSplitterItem;
    cxGridPopupMenu: TcxGridPopupMenu;
    procedure btnNewClick(Sender: TObject);
    procedure BtnModificareClick(Sender: TObject);
    procedure btnAnuleazaClick(Sender: TObject);
    procedure ContracteFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnRefreshClick(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure AditionaleFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure btnOkClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
  private
    FIdContract: Integer;
    FDescContract: String;
    { Private declarations }
  public
    { Public declarations }
    procedure ReportClick(Sender: TObject);
    procedure RefreshScreen;
    property IdContract : Integer read FIdContract;
    property DescContract : String read FDescContract;
  end;


function SelectieContract(var aText : String)  : Integer;


implementation

uses
  dxCompsUtile, ZeosDBUtile, dateUnit, CommonDBVar, uContracteEdit, RapInclude, PersistGridSettings;

{$R *.dfm}

function SelectieContract (var aText : String) : Integer;
begin
  with TfrmContracte.Create(nil) do
  try
    Visible := False;
    btnNew.Visible := False;
    btnAnuleaza.Visible := False;
    btnRaportare.Visible := False;
    BtnModificare.Visible := False;
    ShowModal;
    aText := '';
   if ModalResult = mrOk then begin
      aText :=  DescContract;
      Result := IdContract;
    end else Result := -1;
  finally
    Free;
  end;
end;

procedure TfrmContracte.btnNewClick(Sender: TObject);
var
  lIdContract : Integer;
begin
  lIdContract := TfrmContractEdit.NewContract;
  ModificareContract(lIdContract);
  RefreshScreen;
end;

procedure TfrmContracte.BtnModificareClick(Sender: TObject);
begin
  ModificareContract(FIdContract);
  RefreshScreen;
end;

procedure TfrmContracte.btnAnuleazaClick(Sender: TObject);
var
  aFocusedRecord : TcxCustomGridRecord;
  lColNr, lColData : Integer;
begin
  if Aditionale.Focused then begin
    aFocusedRecord := Aditionale.Controller.FocusedRecord;
    lColNr := AditionaleNR_CONTRACT.Index;
    lColData := AditionaleDATA_CONTRACT.Index;
  end
  else begin
    aFocusedRecord := Contracte.Controller.FocusedRecord;
    lColNr := ContracteNR_CONTRACT.Index;
    lColData := ContracteDATA_CONTRACT.Index;
  end;
  if aFocusedRecord = nil then Exit;
  if MessageDlg(
    Format('Doriti stergerea contractului nr. %s din data %s ? ',
      [ GetString(aFocusedRecord, lColNr), GetString(aFocusedRecord, lColData) ]
      ) , mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
    DBExecSQLFmt('exec [spContractAnuleaza] %d, %d', [FIdContract, IdUtilizator]);
    RefreshScreen;
  end;
end;

procedure TfrmContracte.ContracteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
   if (AFocusedRecord <> nil) and AFocusedRecord.IsData then begin
     //FIdContract :=  Contracte.DataController.GetRecordId(AFocusedRecord.RecordIndex);
     FIdContract :=  Contracte.DataController.GetKeyFieldsValues;

     FDescContract := Format('Contrac nr. %s din %s', [AFocusedRecord.DisplayTexts[ContracteNR_CONTRACT.Index],
                                                       AFocusedRecord.DisplayTexts[ContracteDATA_CONTRACT.Index]]
                            );
   end;
end;

procedure TfrmContracte.FormCreate(Sender: TObject);
begin
  Contracte.RestoreFromStorage(Self.Name + '.'+ Contracte.Name, TcxDBIniFileReader);
  PopulateReportContext(Self.ClassName, btnRaportare, ReportClick);
  RefreshScreen;
  FIdContract := -1;
end;

procedure TfrmContracte.RefreshScreen;
begin
  DBRefresh([qryOperatori, qryProiecte]);
  FillImageCombo(edOperator.Properties, qryOperatori, 'ID_UTILIZATORI', 'NUMEINTREG', Null, 'Toti Utilizatorii');
  FillImageCombo(edProiect.Properties, qryProiecte, 'Id', 'Denumire', Null, 'Toate proiectele/unitatile');

  qryContract.Params.ParamByName('IdUtilizator').AsInteger := edOperator.EditValue;
  qryContract.Params.ParamByName('IdAnalitic').AsInteger := edProiect.EditValue;
  qryAditional.Params.ParamByName('IdUtilizator').AsInteger := edOperator.EditValue;
  qryAditional.Params.ParamByName('IdAnalitic').AsInteger := edProiect.EditValue;
  DBRefresh([qryContract, qryAditional]);
  cxCreateMissingColumns(qryContract, Contracte);
end;

procedure TfrmContracte.FormDestroy(Sender: TObject);
begin
  Contracte.StoreToStorage(Self.Name + '.'+ Contracte.Name, TcxDBIniFileWriter);
end;

procedure TfrmContracte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmContracte.btnRefreshClick(Sender: TObject);
begin
  RefreshScreen;
end;


procedure TfrmContracte.ReportClick(Sender: TObject);
begin
  SetRapParam('ID_CONTRACTE', FIdContract);
  LoadReport(TMenuItem(Sender).Tag);
end;

procedure TfrmContracte.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  BtnOk.Left := BtnCancel.Left - BtnOk.Width - 2;
end;

procedure TfrmContracte.AditionaleFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
    if (AFocusedRecord <> nil) and (AFocusedRecord.IsData) then begin
      FIdContract := Aditionale.DataController.GetKeyFieldsValues;
//     FIdContract :=  Aditionale.DataController.GetRecordId(AFocusedRecord.RecordIndex);

     FDescContract := Format('Contract nr. %s din %s', [AFocusedRecord.DisplayTexts[AditionaleNR_CONTRACT.Index],
                                                       AFocusedRecord.DisplayTexts[AditionaleDATA_CONTRACT.Index]]
                            );
   end;
end;

procedure TfrmContracte.btnOkClick(Sender: TObject);
begin
  if fsModal in FFormState then
    ModalResult := mrOk
  else
    Close;
end;

procedure TfrmContracte.btnCancelClick(Sender: TObject);
begin
  if fsModal in FFormState then
    ModalResult := mrCancel
  else
    Close;
end;

end.

unit fmContareBugetaraUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, Buttons, ExtCtrls, cxEdit, DB, cxGridLevel,
  cxClasses, cxControls, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGrid, cxGridBandedTableView, cxGridDBBandedTableView,
  ZDataSet, cxContainer, cxLabel, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, dxmdaset, Menus, cxGridCustomPopupMenu, AlopDisponibil,
  cxGridPopupMenu, cxGraphics, cxDataStorage, cxDBData, ZAbstractRODataset,
  ZAbstractDataset, cxLookAndFeelPainters, cxLookAndFeels, cxStyles, cxCustomData,
  cxFilter, cxData, cxButtons, cxCalendar, Vcl.ComCtrls, dxCore, cxDateUtils,
  cxNavigator, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxBarBuiltInMenu, dxDateRanges;

type
  TfrmContareBugetara = class(TForm)
    pnParametrii: TPanel;
    pnClient: TGroupBox;
    gridDocsLevel: TcxGridLevel;
    gridDocumente: TcxGrid;
    gridDocsView: TcxGridDBBandedTableView;
    DTDocList: TDataSource;
    cmbTipContari: TcxImageComboBox;
    LbTipContari: TcxLabel;
    memDocList: TdxMemData;
    ppDetaliiMenu: TPopupMenu;
    ppIntroducereClasific: TMenuItem;
    popupGrid: TcxGridPopupMenu;
    ppAnulareClasificatie: TMenuItem;
    btnOpen: TcxButton;
    Label1: TLabel;
    Label2: TLabel;
    edDataStart: TcxDateEdit;
    edDataEnd: TcxDateEdit;
    qryDocProvider: TZQuery;
    gridDocsViewid_EB_Contare: TcxGridDBBandedColumn;
    gridDocsViewInternal_ID: TcxGridDBBandedColumn;
    gridDocsViewInternal_Status: TcxGridDBBandedColumn;
    gridDocsViewInternal_UpdateUser: TcxGridDBBandedColumn;
    gridDocsViewInternal_Date: TcxGridDBBandedColumn;
    gridDocsViewInternal_ID_Utilizatori: TcxGridDBBandedColumn;
    gridDocsViewInternal_TimeImport: TcxGridDBBandedColumn;
    gridDocsViewNota_Id: TcxGridDBBandedColumn;
    gridDocsViewNota_Modul: TcxGridDBBandedColumn;
    gridDocsViewNota_Jurnal: TcxGridDBBandedColumn;
    gridDocsViewNota_Nr: TcxGridDBBandedColumn;
    gridDocsViewNota_Data: TcxGridDBBandedColumn;
    gridDocsViewNota_Cont: TcxGridDBBandedColumn;
    gridDocsViewNota_ContCrsp: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Id: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Id_Defalcare: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Tip: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Nr: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Data: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Gestiune: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Furnizor: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Suma: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Explicatie: TcxGridDBBandedColumn;
    gridDocsViewDPrimar_Operator: TcxGridDBBandedColumn;
    gridDocsViewEB_Tip: TcxGridDBBandedColumn;
    gridDocsViewEB_Nr: TcxGridDBBandedColumn;
    gridDocsViewEB_Data: TcxGridDBBandedColumn;
    gridDocsViewEB_CF: TcxGridDBBandedColumn;
    gridDocsViewEB_Unitate: TcxGridDBBandedColumn;
    gridDocsViewEB_CE: TcxGridDBBandedColumn;
    gridDocsViewEB_Proiect: TcxGridDBBandedColumn;
    gridDocsViewEB_Id_Ang: TcxGridDBBandedColumn;
    gridDocsViewEB_Id_Ord: TcxGridDBBandedColumn;
    gridDocsViewContract_Id: TcxGridDBBandedColumn;
    gridDocsViewContract_Nr: TcxGridDBBandedColumn;
    gridDocsViewContract_Data: TcxGridDBBandedColumn;
    pnRight: TPanel;
    N1: TMenuItem;
    AsociereContract1: TMenuItem;
    DezasociereContract1: TMenuItem;
    N2: TMenuItem;
    IntroducereProiect1: TMenuItem;
    AnuleazaProiect1: TMenuItem;
    procedure btnOpenClick(Sender: TObject);
    procedure ppIntroducereClasificClick(Sender: TObject);
    procedure itemFisaMaterialClick(Sender: TObject);
    procedure ppAnulareClasificatieClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);    
    procedure FormShow(Sender: TObject);
    procedure pnParametriiResize(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure AsociereContract1Click(Sender: TObject);
    procedure DezasociereContract1Click(Sender: TObject);
    procedure IntroducereProiect1Click(Sender: TObject);
    procedure AnuleazaProiect1Click(Sender: TObject);
  protected
    FTipCulegere : Integer;
  private
    { Private declarations }
    FDetaliereDocum : TfrmAlopDisponibil;
    function GetCurrentNode : TcxCustomGridRow;
    function fmDetaliereDocum: TfrmAlopDisponibil;
    function SetToStr(lVar : Variant; const withQuotes: boolean = false) : String;
    procedure AplicaContBugetar(lItemID : Integer; ltipCulegere:Integer;
       lCodF, lCodE, lIdUnitate, lIdProiect, lIdAngajament, lIdOrdonantare : Variant );
    procedure CitireDateServer;
    procedure GolireDateServer;
    procedure SetProjectID(ARecord: TcxCustomGridRecord; AProjectID: Variant);
    procedure SetProjectIDList(AProjectID: Variant);
  public
    { Public declarations }
  end;

implementation

{$R *.dfm}

uses
  OI_Proiecte,
  frmSelectieContractUnit,
  ZeosDBUtile,
  dxCompsUtile,
  unitMemTableEx,
  DateUnit,
  SetParamsUnitADO,
  rapInclude,
  CommonDBVar,
  PersistGridSettings;

procedure TfrmContareBugetara.AsociereContract1Click(Sender: TObject);
var
  lSelectieContract : TfrmSelectieContract;
  lRecord : TcxCustomGridRecord;

    function SafeValue(const aFieldName: String): Variant;
    var
      lField: TcxCustomGridTableItem;
    begin
      lField := gridDocsView.FindItemByName(AFieldName);
      if Assigned(lField) then
        Result := lRecord.Values[lField.Index]
      else
        Result := Null;
    end;

    function SafeInt(const AFieldName: String): Integer;
    begin
      Result := ValueSafeToInt(SafeValue(AFieldName));
    end;

begin
  lRecord := gridDocsView.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    lSelectieContract := TfrmSelectieContract.Create(nil);
    try
      lSelectieContract.RefreshContracte;
      lSelectieContract.IdPredator := SafeInt('DPrimar_Id_Gestiune');
      lSelectieContract.IdPrimitor := SafeInt('DPrimar_Id_Furnizor');
      lSelectieContract.IdContract := SafeInt('ID_CONTRACTE');
      lSelectieContract.edNrContract.EditValue := SafeValue('NR_CONTRACT');
      lSelectieContract.edDataContract.EditValue  := SafeValue('DATA_CONTRACT');
      lSelectieContract.FilterContractByDepartament(lSelectieContract.IdPredator);
      lSelectieContract.FilterContractByPrestator(lSelectieContract.IdPrimitor, False);
      lSelectieContract.ShowModal;
      if lSelectieContract.ModalResult = mrOk then begin
        DBExecSQLFmt('exec [spUpdateContractExecutie] %d, %d, %s, %s, %s, %s, %s',
          [
            iSessionID,
            iUserID,
            ValueToStr(lRecord.Values[gridDocsViewInternal_ID.Index]),
            ValueToStr(SafeValue('EB_Id_Ang')),
            ValueToStr(lSelectieContract.IdContract),
            ValueToStr(lSelectieContract.NrContract),
            ValueToStr(lSelectieContract.DataContract)
          ]);
        CitireDateServer;
      end;
    finally
      {lSelectieContract.FilterContractByDepartament(-1);
      lSelectieContract.FilterContractByPrestator(-1);}
      lSelectieContract.Free;
    end;
  end;
end;


procedure TfrmContareBugetara.btnOpenClick(Sender: TObject);
var
  lId : Integer;
  lTopIndex : Integer;
  lFocusedRowIndex, lFocusedColumnIndex : Integer;
begin
  lTopIndex := gridDocsView.Controller.TopRecordIndex;
  lFocusedColumnIndex := gridDocsView.Controller.FocusedColumnIndex;
  lFocusedRowIndex := gridDocsView.Controller.FocusedRowIndex;
  lId := -1;
  FTipCulegere := -1;
  try
    memDocList.DisableControls;
    if memDocList.Active then
      lId := memDocList.FieldbyName('Internal_ID').AsInteger;
    memDocList.Close;
    qryDocProvider.Close;
    qryDocProvider.Params[0].Value := edDataStart.Date;
    qryDocProvider.Params[1].Value := edDataEnd.Date;
    qryDocProvider.Params[2].Value := cmbTipContari.EditValue;
    qryDocProvider.Params[3].Value := IdUtilizator;
    qryDocProvider.Params[4].Value := 1;
    qryDocProvider.Open;
    DBCopyFromDataSet(memDocList, qryDocProvider, False);
    qryDocProvider.Close;
    cxCreateMissingColumns(memDocList, gridDocsView);
    memDocList.EnableControls;
  finally
  end;
  if lId <> -1 then
    memDocList.Locate('Internal_Id', lId, []);
  gridDocsView.Controller.FocusedColumnIndex := lFocusedColumnIndex;
  gridDocsView.Controller.FocusedRowIndex := lFocusedRowIndex;
  gridDocsView.Controller.TopRecordIndex := lTopIndex;
  FTipCulegere := cmbTipContari.EditValue;
end;

procedure TfrmContareBugetara.ppIntroducereClasificClick(Sender: TObject);
var
  lNode  : TcxCustomGridRow;
  lItemId,
  lFurnizor,
  lIdAng,
  lIdOrd,
  lCodF,
  lCodEc,
  lIdUnitate,
  lIdProiect  : Variant;
  I           : Integer;
  lSuma       : Currency;
begin

  lNode := GetCurrentNode;
  if not Assigned(lNode) then Exit;
  lFurnizor   := Null;
  lIdAng      := Null;
  lCodEc      := Null;
  lCodF       := Null;
  lIdUnitate  := Null;
  lIdProiect  := Null;
  
  lItemId := lNode.Values[gridDocsViewInternal_Id.Index];
  lSuma   := ValueSafeToCurrency(lNode.Values[gridDocsViewDPrimar_Suma.Index]);

  if memDocList.Locate('Internal_Id', lItemId, [] ) then begin
    lFurnizor   := memDocList['EB_Id_Furnizor'];
    lIdAng      := memDocList['EB_Id_Ang'];
    lIdOrd      := memDocList['EB_Id_Ord'];
    lIdUnitate  := memDocList['EB_Id_Unitate'];
    lIdProiect  := memDocList['EB_Id_Proiect'];
    lCodF       := memDocList['EB_CF'];
    lCodEc      := memDocList['EB_CE'];
  end;

  fmDetaliereDocum.Position     := poScreenCenter;
  fmDetaliereDocum.SumaCautare  := lSuma;
  fmDetaliereDocum.PrepareCulegere(lFurnizor, lCodF, lCodEc, lIdAng, lIdOrd, lIdUnitate, lIdProiect, Null);
  if fmDetaliereDocum.ShowModal = mrOk then begin
    try
      memDocList.DisableControls;
      if gridDocsView.Controller.SelectedRecordCount > 1 then begin
       for I := 0 to gridDocsView.Controller.SelectedRecordCount -1 do begin
          lItemId := gridDocsView.Controller.SelectedRecords[I].Values[gridDocsViewInternal_Id.Index];
          AplicaContBugetar(lItemId, FTipCulegere, fmDetaliereDocum.CodFunctional, fmDetaliereDocum.CodEconomic,
             fmDetaliereDocum.IdUnitati, fmDetaliereDocum.IdProiecte, fmDetaliereDocum.IdAngajament, fmDetaliereDocum.IdOrdonantare);
       end;
      end
      else
       AplicaContBugetar(lItemId, FTipCulegere, fmDetaliereDocum.CodFunctional, fmDetaliereDocum.CodEconomic,
            fmDetaliereDocum.IdUnitati, fmDetaliereDocum.IdProiecte, fmDetaliereDocum.IdAngajament, fmDetaliereDocum.IdOrdonantare);
      CitireDateServer;
    finally
      memDocList.EnableControls;
    end;
  end;
end;

procedure TfrmContareBugetara.itemFisaMaterialClick(Sender: TObject);
var
  lNode  : TcxCustomGridRow;
begin
  lNode := GetCurrentNode;
  if not Assigned(lNode) then
     Exit;
  LoadReport(DateUnit.GetItemId('FisaMaterial'));
end;

function TfrmContareBugetara.fmDetaliereDocum: TfrmAlopDisponibil;
begin
  if not Assigned(FDetaliereDocum) then
    FDetaliereDocum := TfrmAlopDisponibil.Create(Self);
  Result := FDetaliereDocum;
end;

procedure TfrmContareBugetara.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

function TfrmContareBugetara.GetCurrentNode: TcxCustomGridRow;
begin
  Result := gridDocsView.Controller.FocusedRow;
  if ( Assigned(Result) ) and
     (  ( not Result.IsData) or
        ( Result.Expandable ) ) then Result := nil;
end;

procedure TfrmContareBugetara.ppAnulareClasificatieClick(Sender: TObject);
var
  lNode  : TcxCustomGridRow;
  lItemId, I: Integer;
begin
  lNode := GetCurrentNode;
  if not Assigned(lNode) then Exit;
  lItemId := lNode.Values[gridDocsViewInternal_Id.Index];
  if gridDocsView.Controller.SelectedRecordCount > 1 then begin
    for I := 0 to gridDocsView.Controller.SelectedRecordCount -1 do begin
      lItemId := gridDocsView.Controller.SelectedRecords[I].Values[gridDocsViewInternal_ID.Index];
      AplicaContBugetar(lItemId, FTipCulegere, null,null, null, null, null, null);
    end;
  end
  else
    AplicaContBugetar(lItemId, FTipCulegere, null,null, null, null, null, null);
  CitireDateServer;         
end;

procedure TfrmContareBugetara.FormCreate(Sender: TObject);
begin
  FillImageCombo(cmbTipContari.Properties, 'exec [spGetListTipConsumuri]', 'ID_TIP_CONSUM', 'DENUMIRE');
  StorageReadCxView(gridDocsView);
  edDataStart.Date := MinDataFisc;
  edDataEnd.Date := MaxDataFisc;

end;

procedure TfrmContareBugetara.FormShow(Sender: TObject);
begin
  WindowState := wsMaximized;
end;

procedure TfrmContareBugetara.AnuleazaProiect1Click(Sender: TObject);
var
  lMessageStr: String;
begin
  if gridDocsView.Controller.SelectedRecordCount > 1 then
    lMessageStr := Format('Doriti anularea proiectului pentru cele %d pozitii selectate?', [gridDocsView.Controller.SelectedRecordCount])
  else
    lMessageStr := 'Doriti anularea proiectului pentru pozitia curent selectata?';
  if MessageDlg(lMessageStr, mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    SetProjectIDList(Null);
end;

procedure TfrmContareBugetara.AplicaContBugetar(lItemID,
  ltipCulegere: Integer; lCodF, lCodE, lIdUnitate, lIdProiect,
  lIdAngajament, lIdOrdonantare: Variant);
begin
  DBExecSQLFmt('exec [spEBAplicaContBugetar] %s, %s, %s, %s, %d, %d, %s, %s, %d, %s, %s',
    [
      SetToStr(lIdAngajament),
      SetToStr(lIdOrdonantare),
      SetToStr(lCodF, True),
      SetToStr(lCodE, True),
      ltipCulegere,
      lItemId,
      SetToStr(lIdUnitate),
      SetToStr(lIdProiect),
      IdUtilizator,
      ValueToStr(qryDocProvider.Params[0].Value),
      ValueToStr(qryDocProvider.Params[1].Value)
    ]);
end;

function TfrmContareBugetara.SetToStr(lVar: Variant; const withQuotes: boolean = false): String;
begin
 if VarIsEmpty(lVar) or VarIsNull(lVar) or (VarToStr(lVar) = '0') or (VarToStr(lVar) = '-1') then
   Result := 'null'
 else begin
   Result := VarToStr(lVar);
   if withQuotes then Result := QuotedStr(Result);
 end;
end;

procedure TfrmContareBugetara.pnParametriiResize(Sender: TObject);
begin
  cmbTipContari.Width := pnRight.Left - cmbTipContari.Left;
end;

procedure TfrmContareBugetara.CitireDateServer;
var
  lNode   : TcxCustomGridRecord;
  lItemId : Variant;
begin
  Screen.Cursor := crHourGlass;
  try
    lNode := GetCurrentNode;
    if Assigned(lNode) then lItemId := lNode.Values[gridDocsViewInternal_Id.Index] else lItemID := Null;

    qryDocProvider.Close;
    qryDocProvider.Params[0].Value := edDataStart.Date;
    qryDocProvider.Params[1].Value := edDataEnd.Date;
    qryDocProvider.Params[2].Value := cmbTipContari.EditValue;
    qryDocProvider.Params[3].Value := IdUtilizator;
    qryDocProvider.Params[4].Clear;
    qryDocProvider.Open;
    DBCopyFromDataSet(memDocList, qryDocProvider, False);

    if ValueHasValue(lItemID) then memDocList.Locate('Internal_id', lItemID, []);

  finally
    Screen.Cursor := crDefault;
  end;

end;

procedure TfrmContareBugetara.DezasociereContract1Click(Sender: TObject);
var
  lRecord : TcxCustomGridRecord;

    function SafeValue(const aFieldName: String): Variant;
    var
      lField: TcxCustomGridTableItem;
    begin
      lField := gridDocsView.FindItemByName(AFieldName);
      if Assigned(lField) then
        Result := lRecord.Values[lField.Index]
      else
        Result := Null;
    end;

begin
  lRecord := GetCurrentNode;
  if Assigned(lRecord) then begin
    if MessageDlg(
      Format('Doriti dezasocierea contractului de la nota %s - %s ?',
            [ValueSafeToStr(lRecord.Values[gridDocsViewNota_Nr.Index]),
             ValueSafeToStr(lRecord.Values[gridDocsViewNota_Data.Index])])
      , mtConfirmation, [mbYes, mbNo], 0) = mrYes then
        DBExecSQLFmt('exec [spUpdateContractExecutie] %d, %d, %s, %s, NULL, NULL, NULL',
          [
            iSessionID,
            iUserID,
            ValueToStr(lRecord.Values[gridDocsViewInternal_ID.Index]),
            ValueToStr(SafeValue('EB_Id_Ang'))
          ]);
        CitireDateServer;
  end;
end;
procedure TfrmContareBugetara.GolireDateServer;
begin
   qryDocProvider.Close;
   qryDocProvider.Params[0].Value := edDataStart.Date;
   qryDocProvider.Params[1].Value := edDataEnd.Date;
   qryDocProvider.Params[2].Value := cmbTipContari.EditValue;
   qryDocProvider.Params[3].Value := IdUtilizator;
   qryDocProvider.Params[4].Value := 2;
   qryDocProvider.ExecSQL;
end;

procedure TfrmContareBugetara.SetProjectID(ARecord: TcxCustomGridRecord; AProjectID: Variant);
begin
  DBExecSQLFmt('exec [spSetNotaProject] %d, %d, %s, %s',
    [iUserID, iSessionID, ValueToStr(ARecord.Values[gridDocsViewInternal_ID.Index]), ValueToStr(AProjectID) ]);
end;

procedure TfrmContareBugetara.SetProjectIDList(AProjectID: Variant);
var
  I: Integer;
begin
  if gridDocsView.Controller.SelectedRecordCount > 1 then begin
    for I := 0 to gridDocsView.Controller.SelectedRecordCount-1 do
      SetProjectID(gridDocsView.Controller.SelectedRecords[I], AProjectID);
  end
  else
    SetProjectID(gridDocsView.Controller.FocusedRecord, AProjectID);
  CitireDateServer;
end;

procedure TfrmContareBugetara.IntroducereProiect1Click(Sender: TObject);
var
  lProjectID: Variant;
begin
  if SelectProject(lProjectID) then
    SetProjectIDList(lProjectID);
end;

procedure TfrmContareBugetara.FormDestroy(Sender: TObject);
begin
  StorageWriteCxView(gridDocsView);
end;

end.

unit AlopObligatii;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxControls, cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxButtonEdit,
  cxPC, dxExEdtr, cxSplitter, dxDBGrid, dxGrClms, dxDBTLCl, dxTL, dxDBCtrl, dxCntner, DB, ZDataSet, Menus,
  cxLookAndFeelPainters, StdCtrls, cxButtons, dxmdaset, cxGraphics, cxStyles, cxTL, cxDBTL,
  cxGrid, cxDataStorage, cxDBData, cxCalendar, cxCurrencyEdit, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridCustomView, cxClasses, cxGridLevel, cxCheckBox,
  AlopDisponibil, cxGridCustomPopupMenu, cxGridPopupMenu, ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, dxBarBuiltInMenu, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmAlopObligatii = class(TForm)
    pnTop: TPanel;
    edAngajament: TcxButtonEdit;
    pnBottom: TPanel;
    PCObligatii: TcxPageControl;
    tabFacturi: TcxTabSheet;
    tabNote: TcxTabSheet;
    Splitter: TcxSplitter;
    DTDocum: TDataSource;
    QryDocumListaDocum: TZQuery;
    DTItemsi: TDataSource;
    QryItemsiListaDocum: TZQuery;
    btnRefresh: TcxButton;
    MemDocum: TdxMemData;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    GridItemsi: TcxGrid;
    GridItemsiL: TcxGridLevel;
    GridItemsiV: TcxGridDBTableView;
    GridItemsiVTIPMAT: TcxGridDBColumn;
    GridItemsiVDESCRIERE: TcxGridDBColumn;
    GridItemsiVDENMAT: TcxGridDBColumn;
    GridItemsiVUM: TcxGridDBColumn;
    GridItemsiVDATA_COD: TcxGridDBColumn;
    GridItemsiVDATA_EXPIRARE: TcxGridDBColumn;
    GridItemsiVTIP_MATERIAL: TcxGridDBColumn;
    GridItemsiVCANTITATE: TcxGridDBColumn;
    GridItemsiVPRET_UNITAR: TcxGridDBColumn;
    GridItemsiVPRET_UNITAR_VALUTA: TcxGridDBColumn;
    GridItemsiVCOTA_TVA: TcxGridDBColumn;
    GridItemsiVPRET_TVA: TcxGridDBColumn;
    GridItemsiVPRET_TOTAL: TcxGridDBColumn;
    GridItemsiVTVA: TcxGridDBColumn;
    GridItemsiVPRET_TOTAL_TVA: TcxGridDBColumn;
    GridItemsiVCODMAT: TcxGridDBColumn;
    GridDocumView: TcxGridDBTableView;
    GridDocumLevel: TcxGridLevel;
    GridDocum: TcxGrid;
    GridDocumViewSELECTAT: TcxGridDBColumn;
    GridDocumViewID_GEST_DOCUM: TcxGridDBColumn;
    GridDocumViewID_INITIAL: TcxGridDBColumn;
    GridDocumViewCOD_DOCUM: TcxGridDBColumn;
    GridDocumViewPREDATOR: TcxGridDBColumn;
    GridDocumViewPRIMITOR: TcxGridDBColumn;
    GridDocumViewNR_DOCUM: TcxGridDBColumn;
    GridDocumViewDATA_DOCUM: TcxGridDBColumn;
    GridDocumViewTOTAL_DOCUMENT: TcxGridDBColumn;
    GridDocumViewTOTAL_TVA: TcxGridDBColumn;
    GridDocumViewNUMEINTREG: TcxGridDBColumn;
    GridDocumViewDATA_OPERARE: TcxGridDBColumn;
    GridDocumViewID_FURNIZOR: TcxGridDBColumn;
    GridDocumViewID_DOCUMENT_CONEX: TcxGridDBColumn;
    GridDocumViewID_TRANZACTIE: TcxGridDBColumn;
    GridDocumViewAUTOGENERAT: TcxGridDBColumn;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    ppDetaliiMenu: TPopupMenu;
    ppIntroducereClasific: TMenuItem;
    GridItemsiPopupMenu: TcxGridPopupMenu;
    MemItemsi: TdxMemData;
    GridNoteDetaliiV: TcxGridDBTableView;
    GridNoteDetaliiL: TcxGridLevel;
    GridNoteDetalii: TcxGrid;
    GridItemsiVSELECTED: TcxGridDBColumn;
    GridItemsiVVALOARE_LICHIDARE: TcxGridDBColumn;
    tabLichidate: TcxTabSheet;
    QryOrdonantareLichidare: TZQuery;
    GridLichidareV: TcxGridDBTableView;
    GridLichidareL: TcxGridLevel;
    GridLichidare: TcxGrid;
    DTOrdonantareLichidare: TDataSource;
    GridLichidareVID_ALOP_ORDONATARE_LICHIDARE: TcxGridDBColumn;
    GridLichidareVID_ALOP_ORDONANTARE: TcxGridDBColumn;
    GridLichidareVID_ALOP_ORDONANTARE_DEFALCARE: TcxGridDBColumn;
    GridLichidareVID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn;
    GridLichidareVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridLichidareVCOD_ECONOMIC: TcxGridDBColumn;
    GridLichidareVID_TCV: TcxGridDBColumn;
    GridLichidareVID_CNOTE: TcxGridDBColumn;
    GridLichidareVSUMA_OBLIGATII: TcxGridDBColumn;
    GridLichidareVSUMA_AVANS: TcxGridDBColumn;
    GridLichidareVSUMA_PLATA: TcxGridDBColumn;
    GridLichidareVEXPLICATIE: TcxGridDBColumn;
    GridLichidareVID_GEST_DOCUM: TcxGridDBColumn;
    GridDocumPopup: TcxGridPopupMenu;
    QryNote: TZQuery;
    DTNote: TDataSource;
    MemNote: TdxMemData;
    GridNotePopupMenu: TcxGridPopupMenu;
    GridNoteDetaliiVSelected: TcxGridDBColumn;
    GridNoteDetaliiVvaloare_lichidare: TcxGridDBColumn;
    GridNoteDetaliiVnr: TcxGridDBColumn;
    GridNoteDetaliiVcod: TcxGridDBColumn;
    GridNoteDetaliiVpoz: TcxGridDBColumn;
    GridNoteDetaliiVjurnal: TcxGridDBColumn;
    GridNoteDetaliiVnrdoc: TcxGridDBColumn;
    GridNoteDetaliiVdata: TcxGridDBColumn;
    GridNoteDetaliiVexplicatie: TcxGridDBColumn;
    GridNoteDetaliiVvaloare: TcxGridDBColumn;
    GridNoteDetaliiVcontd: TcxGridDBColumn;
    GridNoteDetaliiVcontc: TcxGridDBColumn;
    GridNoteDetaliiVcod_functional: TcxGridDBColumn;
    GridNoteDetaliiVcod_economic: TcxGridDBColumn;
    GridNoteDetaliiVrepartitor_debit: TcxGridDBColumn;
    GridNoteDetaliiVrepartitor_credit: TcxGridDBColumn;
    GridNoteDetaliiVnume_rep_debit: TcxGridDBColumn;
    GridNoteDetaliiVnume_rep_credit: TcxGridDBColumn;
    GridNoteDetaliiVramas_de_lichidat: TcxGridDBColumn;
    GridNoteDetaliiVramas_de_plata: TcxGridDBColumn;
    GridNoteDetaliiVramas_apoi_lichidat: TcxGridDBColumn;
    procedure btnRefreshClick(Sender: TObject);
    procedure GridDocumViewFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure ppIntroducereClasificClick(Sender: TObject);
    procedure QryItemsiListaDocumAfterOpen(DataSet: TDataSet);
    procedure GridItemsiVSELECTEDPropertiesChange(Sender: TObject);
    procedure GridDocumViewSELECTATPropertiesChange(Sender: TObject);
    procedure QryNoteAfterOpen(DataSet: TDataSet);
    procedure QryDocumListaDocumAfterOpen(DataSet: TDataSet);
    procedure GridNoteDetaliiVSelectedPropertiesChange(Sender: TObject);
  private
   { Private declarations }
    FDetaliereDocum : TfrmAlopDisponibil;
    FIdAngajament   : Variant;
    FIdOrdonantare  : Variant;
    FIdAngDefalcare : Variant;
    function  fmDetaliereDocum: TfrmAlopDisponibil;
    procedure ChangeValoareLichidare(Sender : TField);
    procedure ChangeNoteValoareLichidare(Sender : TField);
    procedure SetIdOrdonantare(const Value: Variant);
    procedure SetIdAngajament(const Value: Variant);
    procedure SetIdAngDefalcare(const Value: Variant);
    function GetDocumenteJustificative: String;
    function GetSumaDePlataTotala: Currency;
  public
    { Public declarations }
    function Execute : Boolean;
    procedure RefreshDocumente;
    procedure RefreshListaNote;
    property  IdAngajament    : Variant read FIdAngajament write SetIdAngajament;
    property  IdAngDefalcare  : Variant read FIdAngDefalcare write SetIdAngDefalcare;
    property  IdOrdonantare   : Variant read FIdOrdonantare write SetIdOrdonantare;
    property  DocumenteJustificative: String read GetDocumenteJustificative;
    property  SumaDePlataTotala : Currency read GetSumaDePlataTotala;
  end;

var
  frmAlopObligatii: TfrmAlopObligatii;

implementation

uses
  ZeosDBUtile, unitMemTableEx, CommonDBVar, DateUnit;

{$R *.dfm}

procedure TfrmAlopObligatii.btnRefreshClick(Sender: TObject);
begin
  RefreshDocumente;
  RefreshListaNote;
end;

procedure TfrmAlopObligatii.RefreshDocumente;
begin
  QryDocumListaDocum.Params.ParamByName('idOrdonantare').Value := FIdOrdonantare;
  QryDocumListaDocum.Params.ParamByName('idAng').Value := FIdAngajament;  
  DBRefresh(QryDocumListaDocum);
end;

procedure TfrmAlopObligatii.GridDocumViewFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
//var lIdDocum: Integer;
begin
  if fsCreating in FormState then Exit;
  QryItemsiListaDocum.Close;
  if not Assigned(AFocusedRecord) then Exit;
  GridDocumView.Invalidate;// Docum.Invalidate;
  //lIdDocum := GetInteger(AFocusedRecord, GridDocumViewID_GEST_DOCUM.Index);
  QryItemsiListaDocum.Open;
end;

procedure TfrmAlopObligatii.SetIdAngajament(const Value: Variant);
begin
  if ValueSafeToInt(Value, -1) = -1 then FIdAngajament := Null else FIdAngajament := ValueSafeToInt(Value);
  QryDocumListaDocum.Params.ParamByName('idAng').Value := FIdAngajament;
end;

procedure TfrmAlopObligatii.ppIntroducereClasificClick(Sender: TObject);
var
  lItemId,
  lFurnizor,
  lIdAng,
  lCodF,
  lCodEc  : Variant;
begin
  if not Assigned(GridItemsiV.Controller.FocusedRecord) then Exit;
  if not GridItemsiV.Controller.FocusedRecord.IsData then Exit;
  
  lItemId   := GridItemsiV.DataController.GetRecordId(GridItemsiV.Controller.FocusedRecordIndex);
  lFurnizor := Null;
  lIdAng    := Null;
  lCodEc    := Null;
  lCodF     := Null;

  if QryItemsiListaDocum.Locate('id_gest_itemsi', lItemId, [] ) then begin
    if QryItemsiListaDocum.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger > 0 then
      lIdAng := QryItemsiListaDocum['ID_ANGAJAMENTE_DEFALCARE'];
    lCodF   := QryItemsiListaDocum['cod_functional'];
    lCodEc  := QryItemsiListaDocum['cod_economic'];
  end;

  fmDetaliereDocum.Position := poScreenCenter;
  fmDetaliereDocum.PrepareCulegere(lFurnizor, lCodF, lCodEc, lIdAng, Null, Null, Null, Null);
  if fmDetaliereDocum.ShowModal = mrOk then begin
    DBExecSQLFmt('UPDATE GEST_ITEMSI SET ID_ANGAJAMENTE_DEFALCARE = %s, COD_FUNCTIONAL = %s, COD_ECONOMIC = %s WHERE ID_GEST_ITEMSI = %s',
                  [
                    ValueToStr(fmDetaliereDocum.IdAngajament),
                    ValueToStr(fmDetaliereDocum.CodFunctional),
                    ValueToStr(fmDetaliereDocum.CodEconomic),
                    ValueToStr(lItemId)
                  ]);
    QryItemsiListaDocum.Close;
    QryItemsiListaDocum.Open;
  end;
  
end;

function TfrmAlopObligatii.fmDetaliereDocum: TfrmAlopDisponibil;
begin
  if not Assigned(FDetaliereDocum) then
    FDetaliereDocum := TfrmAlopDisponibil.Create(Self);
  Result := FDetaliereDocum;
end;

procedure TfrmAlopObligatii.RefreshListaNote;
begin
  QryNote.Params.ParamByName('idAng').Value := FIdAngajament;
  QryNote.Params.ParamByName('idOrdonantare').Value := FIdOrdonantare;
  DBRefresh(QryNote);
end;

procedure TfrmAlopObligatii.QryItemsiListaDocumAfterOpen(
  DataSet: TDataSet);
begin
  MemItemsi.Tag := 1;
  MemItemsi.Active := False;
  MemItemsi.Active := True;
  DBLoadFromDataSet(MemItemsi, QryItemsiListaDocum, False);
  MemItemsi.FieldByName('VALOARE_LICHIDARE').OnChange := ChangeValoareLichidare;
  MemItemsi.First;
  while not MemItemsi.eof do begin
    if QryOrdonantareLichidare.Active and QryOrdonantareLichidare.Locate('ID_GEST_DOCUM;ID_TCV',
       VarArrayOf([MemItemsi.FieldByName('ID_GEST_DOCUM').AsInteger, MemItemsi.FieldByName('ID_GEST_ITEMSI').AsInteger]), []) then begin
       MemItemsi.Edit;
       MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency := QryOrdonantareLichidare.FieldByName('SUMA_PLATA').AsCurrency;
       if MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency <> 0 then
         MemItemsi.FieldByName('SELECTED').AsBoolean := True;
       MemItemsi.Post;
    end;
    MemItemsi.Next;
  end;
  MemItemsi.Tag := 0;
end;

procedure TfrmAlopObligatii.GridItemsiVSELECTEDPropertiesChange(
  Sender: TObject);
var
  lChecked : Boolean ;
begin
  if MemItemsi.Tag <> 0 then Exit;
  lChecked := False;
  if (GridItemsiV.Controller.FocusedRecord <> nil) and (GridItemsiV.Controller.FocusedRecord.IsData) then
    lChecked := GridItemsiV.Controller.FocusedRecord.Values[GridItemsiVSELECTED.Index];
  if not lChecked and (MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency <> 0) then begin
    MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency := 0;
  end
  else begin
    MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency := MemItemsi.FieldByName('PRET_TOTAL_TVA').AsCurrency;
  end;
end;

procedure TfrmAlopObligatii.ChangeValoareLichidare(Sender: TField);
var
  lIdGestItemsi : Integer;
  lIdGestDocum : Integer;
  lValoare : Currency;
begin
  if Sender.DataSet.Tag <> 0 then Exit;
  lIdGestItemsi := Sender.DataSet.FieldByName('ID_GEST_ITEMSI').AsInteger;
  lIdGestDocum  := Sender.DataSet.FieldByName('ID_GEST_DOCUM').AsInteger;
  lValoare := Sender.AsCurrency;
  if (lValoare <> 0) and not (Sender.DataSet.FieldByName('SELECTED').AsBoolean) then begin
    if not (Sender.DataSet.State in [dsEdit, dsInsert]) then Sender.DataSet.Edit;
    Sender.DataSet.FieldByName('SELECTED').AsBoolean := True;
    Sender.DataSet.Post;
  end;
  if not QryOrdonantareLichidare.Active then Exit;
  with QryOrdonantareLichidare do begin
    if Locate('ID_GEST_DOCUM;ID_TCV',VarArrayOf([lIdGestDocum, lIdGestItemsi]), []) then if lValoare = 0 then Delete else Edit
                                                                                    else Append;
    if lValoare = 0 then Exit;
    FieldByName('ID_ALOP_ORDONANTARE').AsInteger := FIdOrdonantare;
    FieldByName('COD_FUNCTIONAL').AsString       := MemItemsi.FieldByName('COD_FUNCTIONAL').AsString;
    FieldByName('COD_ECONOMIC').AsString         := MemItemsi.FieldByName('COD_ECONOMIC').AsString;
    FieldByName('ID_GEST_DOCUM').AsInteger       := MemItemsi.FieldByName('ID_GEST_DOCUM').AsInteger;
    FieldByName('ID_TCV').AsInteger              := MemItemsi.FieldByName('ID_GEST_ITEMSI').AsInteger;
    FieldByName('SUMA_PLATA').AsCurrency         := lValoare;
    Post;
  end;
end;

procedure TfrmAlopObligatii.SetIdOrdonantare(const Value: Variant);
begin
  if ValueSafeToInt(Value, -1) = -1 then
    FIdOrdonantare := Null
  else
    FIdOrdonantare := ValueSafeToInt(Value);
  if QryOrdonantareLichidare.Active then QryOrdonantareLichidare.Close;
  if QryNote.Active then QryNote.Close;
  QryNote.Params.ParamByName('IdOrdonantare').Value                       := FIdOrdonantare;
  QryOrdonantareLichidare.Params.ParamByName('ID_ALOP_ORDONANTARE').Value := FIdOrdonantare;
  QryOrdonantareLichidare.Open;
  QryNote.Open;
end;

procedure TfrmAlopObligatii.GridDocumViewSELECTATPropertiesChange(
  Sender: TObject);
var
  lChecked : Boolean ;
begin
 //facem check pe toate din copii
  if MemItemsi.Tag <> 0 then Exit;
  lChecked := False;
  if (GridDocumView.Controller.FocusedRecord <> nil) and (GridDocumView.Controller.FocusedRecord.IsData) then
    lChecked := GridDocumView.Controller.FocusedRecord.Values[GridDocumViewSELECTAT.Index];
  if not lChecked then begin
    MemItemsi.First;
    while not MemItemsi.Eof do begin
      MemItemsi.Edit;
      MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency := 0;
      MemItemsi.Next;
    end;
  end
  else begin
    MemItemsi.First;
    while not MemItemsi.Eof do begin
      MemItemsi.Edit;
      MemItemsi.FieldByName('VALOARE_LICHIDARE').AsCurrency := MemItemsi.FieldByName('PRET_TOTAL_TVA').AsCurrency;
      MemItemsi.Next;
    end;
  end;
end;

procedure TfrmAlopObligatii.QryNoteAfterOpen(DataSet: TDataSet);
begin
  MemNote.Tag := 1;
  MemNote.Active := False;
  MemNote.Active := True;
  DBLoadFromDataSet(MemNote, QryNote, False);
  MemNote.FieldByName('VALOARE_LICHIDARE').OnChange := ChangeNoteValoareLichidare;
  MemNote.First;
  while not MemNote.eof do begin
    if QryOrdonantareLichidare.Active and QryOrdonantareLichidare.Locate('ID_CNOTE', MemNote.FieldByName('NR').AsInteger, []) then begin
       MemNote.Edit;
       MemNote.FieldByName('VALOARE_LICHIDARE').AsCurrency := QryOrdonantareLichidare.FieldByName('SUMA_PLATA').AsCurrency;
       if MemNote.FieldByName('VALOARE_LICHIDARE').AsCurrency <> 0 then
         MemNote.FieldByName('SELECTED').AsBoolean := True;
       MemNote.Post;
    end;
    MemNote.Next;
  end;
  MemNote.Tag := 0;
end;

procedure TfrmAlopObligatii.ChangeNoteValoareLichidare(Sender: TField);
var
  lNr : Integer;
  lValoare : Currency;
begin
  if Sender.DataSet.Tag <> 0 then Exit;
  if not (Sender.DataSet.State in [dsEdit, dsInsert]) then Sender.DataSet.Edit;
  Sender.DataSet.FieldByName('ramas_apoi_lichidat').AsCurrency  :=
    Sender.DataSet.FieldByName('ramas_de_lichidat').AsCurrency - Sender.AsCurrency;
  Sender.DataSet.Post;

  lNr := Sender.DataSet.FieldByName('NR').AsInteger;
  lValoare := Sender.AsCurrency;
  if (lValoare <> 0) and not (Sender.DataSet.FieldByName('SELECTED').AsBoolean) then begin
    if not (Sender.DataSet.State in [dsEdit, dsInsert]) then Sender.DataSet.Edit;
    Sender.DataSet.FieldByName('SELECTED').AsBoolean := True;
    Sender.DataSet.Post;
  end;
  if not QryOrdonantareLichidare.Active then Exit;
  with QryOrdonantareLichidare do begin
    if Locate('ID_CNOTE',lNr, []) then if lValoare = 0 then Delete else Edit
                                                                                    else Append;
    if lValoare = 0 then Exit;
    FieldByName('ID_ALOP_ORDONANTARE').AsInteger := FIdOrdonantare;
    FieldByName('COD_FUNCTIONAL').AsString       := Sender.DataSet.FieldByName('COD_FUNCTIONAL').AsString;
    FieldByName('COD_ECONOMIC').AsString         := Sender.DataSet.FieldByName('COD_ECONOMIC').AsString;
    FieldByName('ID_CNOTE').AsInteger            := Sender.DataSet.FieldByName('NR').AsInteger;
    FieldByName('SUMA_PLATA').AsCurrency         := lValoare;
    Post;
  end;
end;

procedure TfrmAlopObligatii.QryDocumListaDocumAfterOpen(DataSet: TDataSet);
begin
  MemDocum.Active := False;
  MemDocum.Active := True;
  DBLoadFromDataSet(MemDocum, QryDocumListaDocum, False);
end;

procedure TfrmAlopObligatii.GridNoteDetaliiVSelectedPropertiesChange(
  Sender: TObject);
var
  lChecked : Boolean ;
begin
  if MemNote.Tag <> 0 then Exit;
  lChecked := False;
  if (GridNoteDetaliiV.Controller.FocusedRecord <> nil) and (GridNoteDetaliiV.Controller.FocusedRecord.IsData) then
    lChecked := GridNoteDetaliiV.Controller.FocusedRecord.Values[GridNoteDetaliiVSELECTED.Index];
  if not lChecked and (MemNote.FieldByName('VALOARE_LICHIDARE').AsCurrency <> 0) then begin
    MemNote.FieldByName('VALOARE_LICHIDARE').AsCurrency := 0;
  end
  else begin
    MemNote.FieldByName('VALOARE_LICHIDARE').AsCurrency := MemNote.FieldByName('ramas_de_lichidat').AsCurrency;
  end;
end;

procedure TfrmAlopObligatii.SetIdAngDefalcare(const Value: Variant);
begin
  if ValueSafeToInt(Value, -1) = -1 then FIdAngDefalcare := Null else FIdAngDefalcare := ValueSafeToInt(Value);
  QryDocumListaDocum.Params.ParamByName('idAngDef').Value := FIdAngDefalcare;
  QryNote.Params.ParamByName('idAng').Value               := FIdAngDefalcare;
end;

function TfrmAlopObligatii.GetDocumenteJustificative: String;
begin
  Result := ValueSafeToStr(DBGetScallarFmt('exec [spGetDocumenteOrdonantate] %s', [ValueSafeToStr(FIdOrdonantare)]));
end;

function TfrmAlopObligatii.Execute: Boolean;
begin
  RefreshDocumente;
  RefreshListaNote;
  Result := ShowModal = mrOk;
end;

function TfrmAlopObligatii.GetSumaDePlataTotala: Currency;
begin
  Result := ValueSafeToCurrency(DBGetScallarFmt('exec [spGetSumaDocumenteOrdonantate] %s', [ValueSafeToStr(FIdOrdonantare)]));
end;

end.

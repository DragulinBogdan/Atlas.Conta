unit ImperechereNote;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, DB, ZDataSet,
  cxSplitter, StdCtrls, cxButtons,
  cxControls, cxContainer, cxEdit, cxTextEdit, cxCurrencyEdit, ExtCtrls,
  cxLookAndFeelPainters, ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxDBData, cxMaskEdit, cxCalendar, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGridCustomView, cxClasses,
  cxGridLevel, cxGrid, cxProgressBar, cxGridBandedTableView,
  cxGridDBBandedTableView, cxGridCustomPopupMenu, cxGridPopupMenu, cxStatusKeeper,
  cxNavigator, dxDateRanges, dxScrollbarAnnotations, dxBarBuiltInMenu;

type
  TfrmImperechereNote = class(TForm)
    pnTools: TPanel;
    edSuma: TcxCurrencyEdit;
    BtnAdd: TcxButton;
    BtnModify: TcxButton;
    BtnDelete: TcxButton;
    BtnAutoDecont: TcxButton;
    btnCautaAutomat: TcxButton;
    pnBottom: TPanel;
    pnImperecheate: TGroupBox;
    pnDeconturi: TPanel;
    GrCasa: TGroupBox;
    GrDocumente: TGroupBox;
    DTIncasari1: TDataSource;
    QryIncasari: TZQuery;
    DTDocumente1: TDataSource;
    QryDocumente: TZQuery;
    DTImperecheri1: TDataSource;
    QryImperecheri: TZQuery;
    ppDecontari: TPopupMenu;
    ppLocalizare: TMenuItem;
    BtnDefalcare: TcxButton;
    splitTop: TcxSplitter;
    splitBotom: TcxSplitter;
    btnRefresh: TcxButton;
    btnSwitch: TcxButton;
    cxGridIncasari: TcxGrid;
    cxGridIncasariLevel1: TcxGridLevel;
    GridIncasari: TcxGridDBTableView;
    GridIncasariNR_PLATA: TcxGridDBColumn;
    GridIncasaricod_document: TcxGridDBColumn;
    GridIncasarinr_document: TcxGridDBColumn;
    GridIncasaridata_document: TcxGridDBColumn;
    GridIncasariCONT_DEBT: TcxGridDBColumn;
    GridIncasariCONT_CRED: TcxGridDBColumn;
    GridIncasariDOCUMENT: TcxGridDBColumn;
    GridIncasariDATA_REFERINTA: TcxGridDBColumn;
    GridIncasariSUMA: TcxGridDBColumn;
    GridIncasariASIGNAT: TcxGridDBColumn;
    GridIncasariRAMAS: TcxGridDBColumn;
    GridIncasariPROCENT: TcxGridDBColumn;
    GridIncasariCODREP: TcxGridDBColumn;
    GridIncasariNUME: TcxGridDBColumn;
    cxGridDocumente: TcxGrid;
    cxGridDocumenteLevel1: TcxGridLevel;
    GridDocumente: TcxGridDBTableView;
    GridDocumenteNR_OBL: TcxGridDBColumn;
    GridDocumentecod_document: TcxGridDBColumn;
    GridDocumentenr_document: TcxGridDBColumn;
    GridDocumentedata_document: TcxGridDBColumn;
    GridDocumenteCONT_DEBT: TcxGridDBColumn;
    GridDocumenteCONT_CRED: TcxGridDBColumn;
    GridDocumenteDOCUMENT: TcxGridDBColumn;
    GridDocumenteDATA_REFERINTA: TcxGridDBColumn;
    GridDocumenteSUMA: TcxGridDBColumn;
    GridDocumenteASIGNAT: TcxGridDBColumn;
    GridDocumenteRAMAS: TcxGridDBColumn;
    GridDocumentePROCENT: TcxGridDBColumn;
    GridDocumenteCODREP: TcxGridDBColumn;
    GridDocumenteNUME: TcxGridDBColumn;
    cxGridImperecheri: TcxGrid;
    cxGridImperecheriLevel1: TcxGridLevel;
    GridImperecheri: TcxGridDBBandedTableView;
    GridImperecheriID_CNOTE_IMPERECHERE: TcxGridDBBandedColumn;
    GridImperecheriNR_OBL: TcxGridDBBandedColumn;
    GridImperecheriNR_PLATA: TcxGridDBBandedColumn;
    GridImperecheriSUMA: TcxGridDBBandedColumn;
    GridImperecheriNRDOC_OBL: TcxGridDBBandedColumn;
    GridImperecheriDATA_OBL: TcxGridDBBandedColumn;
    GridImperecheriEXPL_OBL: TcxGridDBBandedColumn;
    GridImperecheriNRDOC_PLATA: TcxGridDBBandedColumn;
    GridImperecheriDATA_PLATA: TcxGridDBBandedColumn;
    GridImperecheriEXPL_PLATA: TcxGridDBBandedColumn;
    GridImperecheriREP_OBL: TcxGridDBBandedColumn;
    GridImperecheriREP_PLATA: TcxGridDBBandedColumn;
    pmGridImperecheri: TcxGridPopupMenu;
    procedure BtnAddClick(Sender: TObject);
    procedure edSumaPropertiesChange(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure QryImperecheriAfterOpen(DataSet: TDataSet);
    procedure BtnModifyClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure ppLocalizareClick(Sender: TObject);
    procedure BtnAutoDecontClick(Sender: TObject);
    procedure ppDecontariPopup(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure btnCautaAutomatClick(Sender: TObject);
    procedure edSumaPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure BtnDefalcareClick(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure btnRefreshClick(Sender: TObject);
    procedure btnSwitchClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure GridIncasariFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridIncasariDblClick(Sender: TObject);
    procedure GridIncasariDataControllerFilterChanged(Sender: TObject);
    procedure GridIncasariPROCENTGetDisplayText(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      var AText: String);
    procedure GridIncasariPROCENTCustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure GridDocumenteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridDocumenteDblClick(Sender: TObject);
    procedure GridDocumenteDataControllerFilterChanged(Sender: TObject);
    procedure GridDocumentePROCENTGetDisplayText(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      var AText: String);
    procedure GridDocumentePROCENTCustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure GridImperecheriDblClick(Sender: TObject);
  private
    { Private declarations }
    { Private declarations }
    FIsInLoading    : Boolean;
    FIsInternalLoad : Boolean;

//    FReconciliere   : TCustomForm;
//    FIdRepartitor   : Integer;
//    FCurentFilter : String;
    FNumeRepartitor : String;

    FIdDocument   : Integer;
    FIdDocCasa    : Integer;

    FDecontSpecificCod : Integer;

    FPivotGrid : TcxGridDBTableView;
    FSearchGrid : TcxGridDBTableView;
    FSearchSuma : Currency;

    FStatusGrid : TcxStatusKeeper;

    procedure SetupKeeper(aView : TcxCustomGridview);

    function DisponibilIncasare: Currency;
    function DisponibilDocum: Currency;
    function GetDecontareText(ANode: TcxCustomGridRecord): String;

    procedure DrawProcent ( ACanvas: TCanvas; ARect: TRect; AProcent : Integer);
    procedure SetIdDocument(const Value: Integer);
    procedure SetIdDocCasa(const Value: Integer);

    procedure SetCurentStatus;
    procedure TryToSearchSuma;

  protected
    procedure ReasignareDocum(Sender: TObject);
    function Reconcilieri: TCustomForm;

  public
    { Public declarations }
    procedure RefreshIncasari;

    property IdDocument: Integer read FIdDocument write SetIdDocument;
    property IdDocCasa : Integer read FIdDocCasa  write SetIdDocCasa;
  end;


var
  frmImperechereNote: TfrmImperechereNote;

implementation

{$R *.dfm}

uses
  ZeosDBUtile, DateUnit, cxDataUtils,
  DefalcareNoteImperecheate,
  ConcurentUsersUnit;


procedure TfrmImperechereNote.BtnAddClick(Sender: TObject);
var
  lSuma : Currency;
  aSQL : TZReadOnlyQuery;
begin
  edSumaPropertiesChange(edSuma);
  aSQL := GetTmpADOQuery;
  with aSQL do
    try
       lSuma := edSuma.Value;
       Sql.Add('exec spContAddNoteImperechere :ID_OBL, :ID_PLATA, :SUMA');
       Params[0].Value := FIdDocument;
       Params[1].Value := FIdDocCasa;
       Params[2].Value := lSuma;
       ExecSQL;
       RefreshIncasari;
    finally
       Free;
    end;
end;

function TfrmImperechereNote.DisponibilDocum: Currency;
begin
  if
     (GridIncasari.ViewData.RecordCount = 0) or (not QryDocumente.Active) then  Result := 0
  else
    Result := QryDocumente.FieldByName('SUMA').AsCurrency - QryDocumente.FieldByName('ASIGNAT').AsCurrency;
end;

function TfrmImperechereNote.DisponibilIncasare: Currency;
begin
  if (GridDocumente.ViewData.RecordCount = 0) or not QryIncasari.Active then Result := 0
  else
    Result := QryIncasari.FieldByName('SUMA').AsCurrency - QryIncasari.FieldByName('ASIGNAT').AsCurrency;
end;

procedure TfrmImperechereNote.DrawProcent(ACanvas: TCanvas; ARect: TRect;
  AProcent: Integer);
var SRect: TRect;
    S    : String;
begin
  SRect := ARect;
  SRect.Left := SRect.Left + 1; SRect.Right := SRect.Right - 1;
  SRect.Top := SRect.Top + 1; SRect.Bottom := SRect.Bottom - 1;
  ACanvas.Pen.Color := clNavy;
  ACanvas.Rectangle(SRect.Left, SRect.Top, SRect.Right, SRect.Bottom);
  if AProcent > 0 then begin
    SRect.Right := SRect.Left + Trunc( (SRect.Right - SRect.Left) * AProcent / 10000 );
    SRect.Left := SRect.Left + 1; SRect.Right := SRect.Right - 1;
    SRect.Top := SRect.Top + 1; SRect.Bottom := SRect.Bottom - 1;
    ACanvas.Brush.Color := clAqua;
    ACanvas.FillRect(SRect);
  end;
  ACanvas.Font.Color := clBlue;
  SetBkMode(aCanvas.Handle, TRANSPARENT);
  SRect := ARect;
  SRect.Top   := SRect.Top + 1;
  SRect.Bottom:= SRect.Bottom - 1;
  S := Format('%2d.%2d', [AProcent div 100, AProcent mod 100])+'%';
  DrawText(ACanvas.Handle, PChar(S), Length(S), SRect, DT_CENTER + DT_SINGLELINE + DT_VCENTER);
end;

procedure TfrmImperechereNote.edSumaPropertiesChange(Sender: TObject);
var lValMax: Currency;
    lSuma : Currency;
begin
  if FIsInLoading then Exit;
  BtnAdd.Enabled := False;
  lSuma := edSuma.Value;
  lValMax := DisponibilIncasare;
  if lValMax > DisponibilDocum then
     lValMax := DisponibilDocum;
  if lValMax < lSuma then begin
    MessageDlg('Suma introdusa '+CurrToStr(lSuma)+' este mai mare decat valoarea maxima : '+CurrToStr(lValMax), mtError, [mbOk], 0);
    Abort;
  end;
  BtnAdd.Enabled := lSuma > 0;

end;

function TfrmImperechereNote.GetDecontareText(ANode: TcxCustomGridRecord): String;
begin
  if Assigned(ANode) then
    with ANode do
      Result := DisplayTexts[GridImperecheriNRDOC_PLATA.Index]+' '+DisplayTexts[GridImperecheriDATA_PLATA.Index]+' '+DisplayTexts[GridImperecheriREP_PLATA.Index]+' -> '+
                DisplayTexts[GridImperecheriNRDOC_OBL.Index]+' '+DisplayTexts[GridImperecheriDATA_OBL.Index]+' '+DisplayTexts[GridImperecheriREP_OBL.Index]
  else Result := 'Neasignat';

end;

procedure TfrmImperechereNote.ReasignareDocum(Sender: TObject);
//var    lId: Integer;
begin
(*  if Assigned(Sender) then begin
     lId := TMenuItem(Sender).Tag;
     with GetTmpADOQuery do
      try
         Sql.Add('exec spDecontContUpdateTCV :ID_DOCUM, :ID');
         Params[0].Value := lId;
         Params[1].Value := ppReparareDoc.Tag;
         ExecSql;
         RefreshIncasari;
      finally
         Free;
      end;
  end;
*)
// ???
end;

function TfrmImperechereNote.Reconcilieri: TCustomForm;
begin
(*
  if FReconciliere = nil then begin
     FReconciliere := TFrmReconcilereDecontari.Create(Self);
     TFrmReconcilereDecontari(FReconciliere).Decontari := Self;
  end;
  Result := FReconciliere;
 *) //???
end;

procedure TfrmImperechereNote.RefreshIncasari;

begin
  try
    FStatusGrid.SaveState;
    QryIncasari.Close;
    QryDocumente.Close;

    QryImperecheri.Close;

    QryIncasari.Open;
    QryDocumente.Open;
    QryImperecheri.Open;

    {if TabList.TabIndex div 3 = 1 then
       GridIncasariChangeNode(GridIncasari, nil, GridIncasari.FocusedNode)
    else GridDocumenteChangeNode(GridDocumente, nil, GridDocumente.FocusedNode);}

 //  TFrmReconcilereDecontari(Reconcilieri).SetDecontari; ???

  finally
    FStatusGrid.LoadState;
    SetCurentStatus;
  end;
end;


procedure TfrmImperechereNote.SetCurentStatus;
  var
    lValMax : Currency;
    lNode  : TcxCustomGridRecord;
    //isAprox : Boolean;
begin
//  isAprox := False;
  BtnAdd.Enabled := True;
  if (FIdDocument > 0) and (FIdDocCasa > 0) then begin
    lValMax := DisponibilIncasare;
    if lValMax > DisponibilDocum then
       lValMax := DisponibilDocum;

    edSuma.Properties.MaxValue := lValMax;
    edSuma.Value := lValMax;
    BtnAdd.Enabled := (lValMax <> 0);
  end;
  edSuma.Enabled := BtnAdd.Enabled;
  { Incercam localizarea dupa imperechere }
  with GetTmpADOQuery do
    try
       ParamCheck := False;
       Sql.Add('SELECT ID_CNOTE_IMPERECHERE FROM CNOTE_IMPERECHERE WHERE NR_OBL = '+IntToStr(FIdDocument)+' AND NR_PLATA = '+IntToStr(FIdDocCasa));
       Open;
         //folosim pivotul ca referinta
       if IsEmpty then
        begin
          Sql.Clear;
          Sql.Add('SELECT top 1 ID_CNOTE_IMPERECHERE FROM CNOTE_IMPERECHERE WHERE ');
          if GridIncasari = FPivotGrid then
            SQL.Add(' NR_PLATA = '+IntToStr(FIdDocCasa))
          else
            SQL.Add(' NR_OBL = '+IntToStr(FIdDocument));
          Open;
//          isAprox := True;
         end;
       if not IsEmpty then begin
          lNode :=  GridImperecheri.ViewData.GetRecordByRecordIndex(GridImperecheri.DataController.FindRecordIndexByKey(Fields[0].AsInteger));
          if Assigned(lNode) then begin
             lNode.MakeVisible;
             lNode.Focused := True;
          end;
       end;
    finally
       Free;
    end;

end;

procedure TfrmImperechereNote.SetIdDocCasa(const Value: Integer);
var
    lNode     : TcxCustomGridRecord;
begin
  { Sa nu permitem ciclare }
  if FIdDocCasa = Value then Exit;
  FIdDocCasa := Value;

  if FIsInternalLoad then Exit;
  FIsInternalLoad := True;

  if FPivotGrid = GridIncasari then begin
    GridDocumente.DataController.Filter.Clear;
    lNode := GridIncasari.Controller.FocusedRecord;
    if Assigned(lNode) and lNode.IsData then begin
       if  GridIncasari.DataController.GetRecordId(lNode.RecordIndex) <> Value then
          lNode := GridIncasari.ViewData.GetRecordByRecordIndex(GridIncasari.DataController.FindRecordIndexByKey(Value));
       if Assigned(lNode) then begin
          FNumeRepartitor := Trim(lNode.DisplayTexts[GridIncasariNUME.Index]);
          GridDocumente.DataController.Filter.AddItem(nil, GridDocumenteNUME, foEqual, FNumeRepartitor, 'Nume Document : '+FNumeRepartitor);
       end;
    end;
  end;

  FSearchSuma := DisponibilIncasare;
  SetCurentStatus;
  FIsInternalLoad := False;
end;

procedure TfrmImperechereNote.SetIdDocument(const Value: Integer);
var
    lNode     : TcxCustomGridRecord;
begin
  { Sa evitam ciclare }
  if FIdDocument = Value then Exit;
  FIdDocument := Value;

  if FIsInternalLoad then Exit;
  FIsInternalLoad := True;

  if FPivotGrid = GridDocumente then begin
    GridIncasari.DataController.Filter.Clear;
    lNode := GridDocumente.Controller.FocusedRecord;
    if Assigned(lNode) and lNode.IsData then begin
       if GridDocumente.DataController.GetRecordId(lNode.RecordIndex) <> Value then
          lNode := GridDocumente.ViewData.GetRecordByRecordIndex(GridDocumente.DataController.FindRecordIndexByKey(Value));
       if Assigned(lNode) then begin
          FNumeRepartitor := Trim(lNode.DisplayTexts[GridDocumenteNUME.Index]);
          GridIncasari.DataController.Filter.AddItem(nil, GridIncasariNUME, foEqual, FNumeRepartitor, 'Nume Document : '+FNumeRepartitor);
       end;
    end;
  end;


  FSearchSuma := DisponibilDocum;
  SetCurentStatus;
  FIsInternalLoad := False;
end;

procedure TfrmImperechereNote.TryToSearchSuma;
var aNode, oldNode : TcxCustomGridRecord;
    aCol : TcxGridColumn;
    gasit : Boolean;
    lSearchGrid : TcxGridDBTableView;
    lSearchSuma : Currency;
begin
 if FSearchSuma = 0 then begin
   ppLocalizareClick(nil);
   Exit;
 end;
 if Assigned(FSearchGrid) then begin
//   aNode := nil;
   lSearchSuma := FSearchSuma;
   aCol := FSearchGrid.GetColumnByFieldName('RAMAS');
   if aCol = nil then begin
     if FSearchGrid = GridIncasari then aCol := GridIncasariRAMAS
                                   else aCol := GridDocumenteRAMAS;
   end;
   lSearchGrid := FSearchGrid;
   gasit := False;
   oldNode := nil;
   if Assigned(lSearchGrid.ViewData.Records[0]) then begin
     lSearchGrid.ViewData.Records[0].Focused := True;
     lSearchGrid.ViewData.Records[0].MakeVisible;
   end;
   while not gasit do begin
     aNode := lSearchGrid.ViewData.GetRecordByRecordIndex(lSearchGrid.DataController.FindRecordIndexByText(0, aCol.Index, CurrToStr(lSearchSuma), False, True, True));
     gasit := True;
     if (aNode <> nil) and (aNode.Visible) then begin
        aNode.Focused := True;
        aNode.MakeVisible;
     end;
     if (aNode <> nil) and (aNode.DisplayTexts[aCol.Index] <> '') and (not (aNode.Values[aCol.Index]=lSearchSuma)) then gasit := false;
     {if (aNode <> nil) and (lSearchGrid.FindColumnByFieldName('ASIGNAT') <> nil) and (aNode.DisplayTexts[lSearchGrid.FindColumnByFieldName('ASIGNAT').Index] <> '')
       and not(Trim(aNode.DisplayTexts[lSearchGrid.FindColumnByFieldName('ASIGNAT').Index]) = '0') then gasit := false;
     }
     if oldNode = aNode then gasit := True;
     oldNode := aNode;
   end;
 end;
end;

procedure TfrmImperechereNote.FormCreate(Sender: TObject);
begin
  SetupKeeper(GridIncasari);
  SetupKeeper(GridDocumente);
  SetupKeeper(GridImperecheri);


  DBExecSql('exec sp_get_note_imperechere');
  FIsInLoading := False;
  FDecontSpecificCod := -1;
  FPivotGrid := GridIncasari;
  FSearchGrid := GridDocumente;
  WindowState := wsMaximized;
  FIsInLoading := True;
  QryIncasari.Open;
  QryDocumente.Open;
  QryImperecheri.Open;
  FIsInLoading := False;  
end;

procedure TfrmImperechereNote.QryImperecheriAfterOpen(DataSet: TDataSet);
begin
  GridImperecheri.ApplyBestFit(nil);
end;

procedure TfrmImperechereNote.BtnModifyClick(Sender: TObject);
var lNode: TcxCustomGridRecord;
  lSuma : Currency;
begin
  edSumaPropertiesChange(edSuma);
  lNode := GridImperecheri.Controller.FocusedRecord;
  if Assigned(lNode) and lNode.IsData and (MessageDlg('Doriti inlocuirea decontarii curente ?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         lSuma := edSuma.Value;
         Sql.Add('exec spContUpdateDecont :SUMA, :NR_PLATA, :NR_OBL, '  + VarToStr(GridImperecheri.DataController.GetRecordId(lNode.RecordIndex)));
         Params[0].Value := lSuma;
         Params[1].Value := FIdDocCasa;
         Params[2].Value := FIdDocument;
         ExecSQL;
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmImperechereNote.BtnDeleteClick(Sender: TObject);
var lNode: TcxCustomGridRecord;
begin
  lNode := GridImperecheri.Controller.FocusedRecord;
  if (Assigned(lNode)) and lNode.IsData and
     (MessageDlg('Doriti stergera conexiunii intre documentul de plata si cel de obligatii?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         ParamCheck := False;
         Sql.Add('exec spContDeleteDecont ' + VarToStr(GridImperecheri.DataController.GetRecordId(lNode.RecordIndex)));
         ExecSQL;
{
         ParamCheck := False;
         Sql.Add('DELETE CNOTE_IMPERECHERE WHERE ID_CNOTE_IMPERECHERE = '+ VarToStr(lNode.Id));
         ExecSQL;
         Close;
         SQl.Text := 'DELETE CNOTE_DEFALCARE_DECONTARI WHERE ID_CNOTE_IMPERECHERE =' + VarToStr(lNode.Id);
         ExecSQL;
}
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmImperechereNote.ppLocalizareClick(Sender: TObject);
var lCasa, lItemsi: Variant;
    lCasaNode, lItemsiNode: TcxCustomGridRecord;
    Node    : TcxCustomGridRecord;
begin
  Node := GridImperecheri.Controller.FocusedRecord;
  { Ne Pozitionam pe pozitia corecta din casa si pozitia corecta din itemsi }
  if not Assigned(Node) then Exit;
  if not Node.IsData then Exit;  
  Screen.Cursor := crHourGlass;
  try
    lCasa := Node.Values[GridImperecheriNR_PLATA.Index];
    lItemsi := Node.Values[GridImperecheriNR_OBL.Index];

    lCasaNode := GridIncasari.ViewData.GetRecordByRecordIndex(GridIncasari.DataController.FindRecordIndexByKey(lCasa));
    if Assigned(lCasaNode) then begin
       lCasaNode.MakeVisible;
       lCasaNode.Focused := True;
    end;
    lItemsiNode := GridDocumente.ViewData.GetRecordByRecordIndex(GridDocumente.DataController.FindRecordIndexByKey(lItemsi));
    if Assigned(lItemsiNode) then begin
       lItemsiNode.MakeVisible;
       lItemsiNode.Focused := True;
    end;
  finally
    Screen.Cursor := crDefault;
  end;
end;

procedure TfrmImperechereNote.BtnAutoDecontClick(Sender: TObject);
begin
  { Decontam automat documentele de 1 - 1 }
  { Trebuie sa avem grija la mecanismele de ecran }
  { Conditiile dupa care se face decontarea sunt :
    Incasare :
             Se relationeaza Repartitor din Casa cu Primitor din documente, data documentului mai mica
                decat data din registrul de casa
                Tip de relationare :
                    1 document gest  - 1 document casa
                    n documente gest - 1 document casa
                    n documente gest - n documente casa
                    1 document gest  - n documente casa }

end;

procedure TfrmImperechereNote.ppDecontariPopup(Sender: TObject);
(*
var Node   : TdxTreeListNode;
    I      : Integer;
    lWhere : String;
    lNewIdDocument: Integer;
    lIdDocum: String;
*)
(*     procedure AddInnerItem(AIdDocum: Integer; ADesc: String);
     var lpItem: TMenuItem;
      begin
        lpItem := TMenuItem.Create(ppReparareDoc);
        with lpItem do begin
          Caption := ADesc;
          Tag     := AIdDocum;
          OnClick := ReasignareDocum;
        end;
        ppReparareDoc.Add(lpItem);
      end;
*)
begin
(* TODO ???
  if Sender <> nil then Exit;
  Node := GridImperecheri.FocusedNode;

  { Distrugem documentele conexe anterioare }
  for I := ppReparareDoc.Count-1 downto 0 do
    ppReparareDoc.Items[I].Free;
  ppReparareDoc.Enabled := False;
  if not Assigned(Node) then Exit;

  { Tag-ul de pe bara de meniu de refacere contine id-ul din decontari care urmeaza sa fie modificat }
  ppReparareDoc.Tag := TdxDBGridNode(Node).Id;
  lIdDocum := Trim(Node.DisplayTexts[GridImperecheriID_GEST_DOCUM.Index]);
  ppReparareDoc.Enabled := (Node.DisplayTexts[GridImperecheriSTARE.Index] <> '1') and (lIdDocum > '');
  { Daca nu este valid citim si lista de documente care pot fi reasignate }
  if ppReparareDoc.Enabled then begin
    { Daca este stare pe 0 in documente gest cautam documentul valid care a rezultat din documentul
      imperecheat }
      lNewIdDocument := ParseAndGetNewIdDoc(lIdDocum);
      if lNewIdDocument = -1 then
         { Este posibil sa nu fi fost modificat ci anulat si refacut
           In cazul acesta mergem pe alta clauza de where si cautam documentele cu acelasi tip acelasi numar si aceeasi data }
         lWhere := 'JOIN GEST_DOCUM E ON (E.ID_GEST_TIP_DOCUM = A.ID_GEST_TIP_DOCUM AND FLOOR(CONVERT(FLOAT, E.DATA_DOCUM)) = FLOOR(CONVERT(FLOAT, A.DATA_DOCUM)) AND RTRIM(LTRIM(E.NR_DOCUM)) = RTRIM(LTRIM(A.NR_DOCUM)))'#13#10+
                    'WHERE E.ID_GEST_DOCUM = '+lIdDocum+' AND A.STARE=1'
      else lWhere := 'WHERE A.STARE=1 AND A.ID_GEST_DOCUM = '+IntToStr(lNewIdDocument);
      with GetTmpADOQuery do
         try
            ParamCheck := False;
            Sql.Add('SELECT A.ID_GEST_DOCUM,');
            Sql.Add('RTRIM(LTRIM(B.COD_DOCUM))+'' ''+RTRIM(LTRIM(ISNULL(A.NR_DOCUM,''-'')))+'' Din. ''+CONVERT(VARCHAR(10), A.DATA_DOCUM, 103)+'' Total : ''+RTRIM(LTRIM(STR(ISNULL(A.TOTALDOC,0))))+'' ''+RTRIM(LTRIM(C.NUME))+''->''+RTRIM(LTRIM(D.NUME)) AS DESCRIERE');
            Sql.Add('FROM GEST_DOCUM A');
            Sql.Add('JOIN GEST_TIP_DOCUM B ON (A.ID_GEST_TIP_DOCUM = B.ID_GEST_TIP_DOCUM)');
            Sql.Add('JOIN REPARTITORI C ON (C.ID_REPARTITORI = A.ID_PREDATOR)');
            Sql.Add('JOIN REPARTITORI D ON (D.ID_REPARTITORI = A.ID_PRIMITOR)');
            Sql.Add(lWhere);
            Open;
            if not IsEmpty then
               while not Eof do begin
                 AddInnerItem(Fields[0].AsInteger, Fields[1].AsString);
                 Next;
               end;
         finally
            Free;
         end;
    end;
 *)
end;

procedure TfrmImperechereNote.FormShow(Sender: TObject);
begin
  if (GridIncasari.ViewData.RecordCount > 0) and Assigned(GridIncasari.ViewData.Records[0]) then
     GridIncasariFocusedRecordChanged(GridIncasari, nil, GridIncasari.ViewData.Records[0], True);
end;

procedure TfrmImperechereNote.btnCautaAutomatClick(Sender: TObject);
begin
  TryToSearchSuma;
end;

procedure TfrmImperechereNote.edSumaPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if (Error) and (edSuma.Value > edSuma.Properties.MaxValue) then begin
    DisplayValue := edSuma.Properties.MaxValue;
    Error := False;
  end;

end;

procedure TfrmImperechereNote.BtnDefalcareClick(Sender: TObject);
var lCasaNode, lItemsiNode: TcxCustomGridRecord;
    lIdBreg, lDocum: Integer;
    lSuma : Currency;
begin
  // TODO Defalcarea
  lSuma := edSuma.Value;
  lCasaNode := GridIncasari.Controller.FocusedRecord;
  if Assigned(lCasaNode) and lCasaNode.IsData then lIdBreg := GridIncasari.DataController.GetRecordId(lCasaNode.RecordIndex)
  else lIdBreg := -1;
  lItemsiNode := GridDocumente.Controller.FocusedRecord;
  if Assigned(lItemsiNode) and lItemsiNode.IsData then lDocum := GridDocumente.DataController.GetRecordId(lItemsiNode.RecordIndex)
  else lDocum := -1;
  if (lDocum > -1) and (lIdBreg > -1) then
     if EditDefalcareNota(lIdBreg, lDocum, lSuma) then RefreshIncasari;
end;

procedure TfrmImperechereNote.FormDestroy(Sender: TObject);
begin
  ExitSingleUser;
end;

procedure TfrmImperechereNote.btnRefreshClick(Sender: TObject);
begin
  RefreshIncasari;
end;

procedure TfrmImperechereNote.btnSwitchClick(Sender: TObject);
begin
  btnSwitch.Tag := btnSwitch.Tag xor 1;
  GridIncasari.DataController.Filter.Clear;
  GridDocumente.DataController.Filter.Clear;
  if btnSwitch.Tag = 0 then begin
     FPivotGrid := GridIncasari;
     FSearchGrid := GridDocumente;
     cxGridDocumente.Parent := GrDocumente;
     cxGridIncasari.Parent  := GrCasa;
     GrCasa.Caption       := 'Lista inregistrarilor de casa, banca, trezorerie, decontari';
     GrDocumente.Caption  := 'Lista documente (facturi, nir, etc.)';
     btnSwitch.Caption := 'Obligatii - Plati';
  end else begin
     FPivotGrid := GridDocumente;
     FSearchGrid := GridIncasari;
     cxGridDocumente.Parent := GrCasa;
     cxGridIncasari.Parent  := GrDocumente;
     GrDocumente.Caption  := 'Lista inregistrarilor de casa, banca, trezorerie, decontari';
     GrCasa.Caption       := 'Lista documente (facturi, nir, etc.)';
     btnSwitch.Caption := 'Plati - Obligatii';     
  end;
  RefreshIncasari;
end;

procedure TfrmImperechereNote.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmImperechereNote.GridIncasariFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if FIsInLoading then Exit;
  if Assigned(AFocusedRecord) and (AFocusedRecord.IsData) then IdDocCasa := GridIncasari.DataController.GetRecordId(AFocusedRecord.RecordIndex);
end;

procedure TfrmImperechereNote.GridIncasariDblClick(Sender: TObject);
begin
  if FPivotGrid = GridIncasari then TryToSearchSuma;
end;

procedure TfrmImperechereNote.GridIncasariDataControllerFilterChanged(
  Sender: TObject);
begin
  if GridIncasari.ViewData.RecordCount = 0 then Exit;
  if GridIncasari.Controller.FocusedRecord = nil then
    GridIncasariFocusedRecordChanged(GridIncasari, nil, GridIncasari.ViewData.Records[0], True)
  else
    GridIncasariFocusedRecordChanged(GridIncasari, nil, GridIncasari.Controller.FocusedRecord, False);
end;

procedure TfrmImperechereNote.GridIncasariPROCENTGetDisplayText(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  var AText: String);
var
  lCurent,
  lTotal : Currency;
begin
  { Afisam cat la suta este deja asignat }
  if Trim(VarToStr(ARecord.Values[GridIncasariSUMA.Index])) <> '' then
     lTotal := ARecord.Values[GridIncasariSUMA.Index]
  else lTotal := 0;

  if Trim(VarToStr(ARecord.Values[GridIncasariASIGNAT.Index])) <> '' then
     lCurent := ARecord.Values[GridIncasariASIGNAT.Index]
  else lCurent := 0;
  if lTotal > 0 then AText := CurrToStr(lCurent/lTotal * 100)
  else AText := '0';
end;


procedure TfrmImperechereNote.GridIncasariPROCENTCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var
  lProcent: Currency;
begin
  { Afisam cat la suta este deja asignat }
  ACanvas.Brush.Color := clWindow;
  ACanvas.FillRect(AViewInfo.ContentBounds);
  if (Trim(AViewInfo.GridRecord.DisplayTexts[GridIncasariPROCENT.Index]) > '') and
     (not VarIsEmpty(AViewInfo.GridRecord.Values[GridIncasariPROCENT.Index])) and
     (not VarIsNull(AViewInfo.GridRecord.Values[GridIncasariPROCENT.Index])) then
     lProcent := AViewInfo.GridRecord.Values[GridIncasariPROCENT.Index]
  else lProcent := 0;
  DrawProcent(ACanvas.Canvas, AViewInfo.ContentBounds, Trunc(lProcent * 100));
  ADone := True;
end;

procedure TfrmImperechereNote.GridDocumenteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and (AFocusedRecord.IsData) then IdDocument := GridDocumente.DataController.GetRecordId(AFocusedRecord.RecordIndex);
end;

procedure TfrmImperechereNote.GridDocumenteDblClick(Sender: TObject);
begin
  if FPivotGrid = GridDocumente then TryToSearchSuma;
end;

procedure TfrmImperechereNote.GridDocumenteDataControllerFilterChanged(
  Sender: TObject);
begin
  if GridDocumente.ViewData.RecordCount = 0 then Exit; 
  if GridDocumente.Controller.FocusedRecord = nil then
    GridDocumenteFocusedRecordChanged(GridDocumente, nil, GridDocumente.ViewData.Records[0], True)
  else
    GridDocumenteFocusedRecordChanged(GridDocumente, nil, GridDocumente.Controller.FocusedRecord, False);
end;

procedure TfrmImperechereNote.GridDocumentePROCENTGetDisplayText(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  var AText: String);
var
  lCurent,
  lTotal : Currency;
begin
  { Afisam cat la suta este deja asignat }
  if Trim(VarToStr(ARecord.Values[GridDocumenteSUMA.Index])) <> '' then
     lTotal := ARecord.Values[GridDocumenteSUMA.Index]
  else lTotal := 0;

  if Trim(VarToStr(ARecord.Values[GridDocumenteASIGNAT.Index])) <> '' then
     lCurent := ARecord.Values[GridDocumenteASIGNAT.Index]
  else lCurent := 0;
  if lTotal > 0 then AText := CurrToStr(lCurent/lTotal * 100)
  else AText := '0';
end;

procedure TfrmImperechereNote.GridDocumentePROCENTCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var
  lProcent: Currency;
begin
  { Afisam cat la suta este deja asignat }
  ACanvas.Brush.Color := clWindow;
  ACanvas.FillRect(AViewInfo.ContentBounds);
  if (Trim(AViewInfo.GridRecord.DisplayTexts[GridDocumentePROCENT.Index]) > '') and
     (not VarIsEmpty(AViewInfo.GridRecord.Values[GridDocumentePROCENT.Index])) and
     (not VarIsNull(AViewInfo.GridRecord.Values[GridDocumentePROCENT.Index])) then
     lProcent := AViewInfo.GridRecord.Values[GridDocumentePROCENT.Index]
  else lProcent := 0;
  DrawProcent(ACanvas.Canvas, AViewInfo.ContentBounds, Trunc(lProcent * 100));
  ADone := True;
end;

procedure TfrmImperechereNote.GridImperecheriDblClick(Sender: TObject);
var lDecontNode: TcxCustomGridRecord;
    lDocum, lIdBreg    : Integer;
    lSuma : Currency;
begin
  lSuma := edSuma.Value;
  lDecontNode := GridImperecheri.Controller.FocusedRecord;
  if Assigned(lDecontNode) then begin
     lIdBreg := lDecontNode.Values[GridImperecheriNR_PLATA.Index];
     lDocum  := lDecontNode.Values[GridImperecheriNR_OBL.Index];
     if (lDocum > -1) and (lIdBreg > -1) then
        if EditDefalcareNota(lIdBreg, lDocum, lSuma) then RefreshIncasari;
  end;
end;

procedure TfrmImperechereNote.SetupKeeper(aView: TcxCustomGridview);
begin
  if FStatusGrid = nil then
     FStatusGrid := TcxStatusKeeper.Create(Self);
  FStatusGrid.Storages.Add.Component := aView;
end;

end.

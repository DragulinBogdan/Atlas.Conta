unit ImperechereUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, cxControls, cxPC, Db, ZDataSet, Menus,
  
  cxContainer, cxEdit, cxTextEdit, cxCurrencyEdit, cxCheckBox,
  cxButtons, cxSplitter, 
  cxGraphics, cxFilter, cxDBData, 
  cxGridCustomTableView, cxGridTableView,
  cxGridBandedTableView, cxGridDBBandedTableView, cxGridCustomView,
  cxClasses, cxGridLevel, cxGrid, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxGridDBTableView, cxDataStorage,
  cxMaskEdit, cxCalendar, cxImageComboBox, cxLookAndFeelPainters,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxData, cxNavigator, dxBarBuiltInMenu;


type
  TfrmImperecheri = class(TForm)
    Panel1: TPanel;
    Panel2: TPanel;
    GroupBox3: TGroupBox;
    TabList: TcxTabControl;
    Panel3: TPanel;
    GrCasa: TGroupBox;
    GrDocumente: TGroupBox;
    DTIncasari: TDataSource;
    DTDocumente: TDataSource;
    DTImperecheri: TDataSource;
    QryIncasari: TZQuery;
    QryDocumente: TZQuery;
    QryImperecheri: TZQuery;
    pnTools: TPanel;
    edSuma: TcxCurrencyEdit;
    ppDecontari: TPopupMenu;
    ppReparareDoc: TMenuItem;
    ppModificareDocum: TMenuItem;
    ppLocalizare: TMenuItem;
    ppReconciliere: TMenuItem;
    BtnAdd: TcxButton;
    BtnModify: TcxButton;
    BtnDelete: TcxButton;
    BtnDefalcare: TcxButton;
    BtnAutoDecont: TcxButton;
    btnCautaAutomat: TcxButton;
    Splitter1: TcxSplitter;
    Splitter2: TcxSplitter;
    GridImperecheriL: TcxGridLevel;
    cxGridImperecheri: TcxGrid;
    GridImperecheri: TcxGridDBBandedTableView;
    GridImperecheriTIPDOC: TcxGridDBBandedColumn;
    GridImperecheriNRDOC: TcxGridDBBandedColumn;
    GridImperecheriDATA: TcxGridDBBandedColumn;
    GridImperecheriEXPLICATIE: TcxGridDBBandedColumn;
    GridImperecheriIS_BANCA: TcxGridDBBandedColumn;
    GridImperecheriSUMA: TcxGridDBBandedColumn;
    GridImperecheriPREDATOR: TcxGridDBBandedColumn;
    GridImperecheriCOD_DOCUM: TcxGridDBBandedColumn;
    GridImperecheriNR_DOCUM: TcxGridDBBandedColumn;
    GridImperecheriPRIMITOR: TcxGridDBBandedColumn;
    GridImperecheriDATA_DOCUM: TcxGridDBBandedColumn;
    GridImperecheriID_GEST_DOCUM: TcxGridDBBandedColumn;
    GridImperecheriID_BREGISTRU: TcxGridDBBandedColumn;
    GridImperecheriSTARE: TcxGridDBBandedColumn;
    pmGridImperecheri: TcxGridPopupMenu;
    GridImperecheriID_GEST_DECONTARI: TcxGridDBBandedColumn;
    cxGridIncasariL: TcxGridLevel;
    cxGridIncasari: TcxGrid;
    GridIncasari: TcxGridDBTableView;
    GridIncasariTIPDOC: TcxGridDBColumn;
    GridIncasariDENUMIRE: TcxGridDBColumn;
    GridIncasariNRDOC: TcxGridDBColumn;
    GridIncasariDATA: TcxGridDBColumn;
    GridIncasariTIP_PLATA: TcxGridDBColumn;
    GridIncasariTOTAL: TcxGridDBColumn;
    GridIncasariASIGNAT: TcxGridDBColumn;
    GridIncasariPROCENT: TcxGridDBColumn;
    GridIncasariEXPLICATIE: TcxGridDBColumn;
    GridIncasariNUME: TcxGridDBColumn;
    GridIncasariCODGEST: TcxGridDBColumn;
    GridIncasariRAMAS: TcxGridDBColumn;
    GridIncasariCOD: TcxGridDBColumn;
    cxGridDocumenteL: TcxGridLevel;
    cxGridDocumente: TcxGrid;
    GridDocumente: TcxGridDBTableView;
    GridDocumenteCOD_DOCUM: TcxGridDBColumn;
    GridDocumenteNR_DOCUM: TcxGridDBColumn;
    GridDocumenteDATA_DOCUM: TcxGridDBColumn;
    GridDocumenteTOTALDOC: TcxGridDBColumn;
    GridDocumenteASIGNAT: TcxGridDBColumn;
    GridDocumentePROCENT: TcxGridDBColumn;
    GridDocumentePREDATOR: TcxGridDBColumn;
    GridDocumentePRIMITOR: TcxGridDBColumn;
    GridDocumentePREDATOR_INTERN: TcxGridDBColumn;
    GridDocumentePRIMITOR_INTERN: TcxGridDBColumn;
    GridDocumenteID_PREDATOR: TcxGridDBColumn;
    GridDocumenteID_PRIMITOR: TcxGridDBColumn;
    GridDocumenteRAMAS: TcxGridDBColumn;
    GridDocumenteID_GEST_DOCUM: TcxGridDBColumn;
    pmGridIncasari: TcxGridPopupMenu;
    pmGridDocumente: TcxGridPopupMenu;
    procedure FormCreate(Sender: TObject);
    procedure QryImperecheriAfterOpen(DataSet: TDataSet);
    procedure BtnAddClick(Sender: TObject);
    procedure BtnModifyClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure BtnDefalcareClick(Sender: TObject);
    procedure Panel3Resize(Sender: TObject);
    procedure lGridDocumenteFilterChanged(Sender: TObject;
      ADataSet: TDataSet; const AFilterText: String);
    procedure TabListChange(Sender: TObject);
    procedure ppLocalizareClick(Sender: TObject);
    procedure BtnAutoDecontClick(Sender: TObject);
    procedure ppModificareDocumClick(Sender: TObject);
    procedure ppDecontariPopup(Sender: TObject);
    procedure ppReconciliereClick(Sender: TObject);
    procedure edSumaPropertiesChange(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure lGridIncasariDblClick(Sender: TObject);
    procedure lGridDocumenteDblClick(Sender: TObject);
    procedure btnCautaAutomatClick(Sender: TObject);
    procedure edSumaPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure lGridIncasariFilterChanged(Sender: TObject;
      ADataSet: TDataSet; const AFilterText: String);
    procedure GridImperecheriCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure GridImperecheriDblClick(Sender: TObject);
    procedure GridIncasariPROCENTGetDisplayText(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      var AText: String);
    procedure GridIncasariPROCENTCustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure GridIncasariFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridDocumenteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridDocumentePROCENTCustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure GridDocumentePROCENTGetDisplayText(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      var AText: String);
  private
    { Private declarations }
    FReconciliere   : TCustomForm;
    FIsInLoading    : Boolean;
    FIsInternalLoad : Boolean;

    FIdRepartitor   : Integer;
    FNumeRepartitor : String;

    FIdDocument   : Integer;
    FIdDocCasa    : Integer;
    FCurentFilter : String;
    FDecontSpecificCod : Integer;

    FPivotGrid : TcxGridDBTableView;
    FSearchGrid : TcxGridDBTableView;
    FSearchSuma : Currency;

    procedure ReasignareDocum(Sender: TObject);

    function Reconcilieri: TCustomForm;

    function DisponibilIncasare: Currency;
    function DisponibilDocum: Currency;
    function GetDecontareText(ANode: TcxCustomGridRecord): String;

    procedure DrawProcent ( ACanvas: TCanvas; ARect: TRect; AProcent : Integer);
    procedure SetIdDocument(const Value: Integer);
    procedure SetIdDocCasa(const Value: Integer);

    procedure SetCurentStatus;
    procedure TryToSearchSuma;

  public
    { Public declarations }
    procedure RefreshIncasari;
    
    property IdDocument: Integer read FIdDocument write SetIdDocument;
    property IdDocCasa : Integer read FIdDocCasa  write SetIdDocCasa;
  end;



 procedure DoDecont( BancaCod : Integer);


 var
   frmImperecheri : TfrmImperecheri;
   
implementation

{$R *.DFM}

uses Variants, DateUnit, DefalcareDecontareUnit, CommonDBVar, cxDataUtils;



procedure DoDecont( BancaCod : Integer);
var Cod : Integer;
begin
  with TfrmImperecheri.Create(Application) do
    try
       FDecontSpecificCod := BancaCod;
       //GrCasa.Visible := False;
       TabList.Visible := False;
       RefreshIncasari;
       if QryIncasari.IsEmpty then
         Cod := BancaCod
       else
         Cod := QryIncasari.FieldByName('COD').AsInteger;
       IdDocCasa := Cod;
       ShowModal;
    finally
       Free;
    end;
end;

procedure TfrmImperecheri.FormCreate(Sender: TObject);
begin
  FIsInLoading := False;
  FDecontSpecificCod := -1;
  FPivotGrid := GridIncasari;
  FSearchGrid := GridDocumente;
  WindowState := wsMaximized;
end;

procedure TfrmImperecheri.DrawProcent(ACanvas: TCanvas; ARect: TRect; AProcent: Integer);
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

procedure TfrmImperecheri.QryImperecheriAfterOpen(DataSet: TDataSet);
begin
  GridImperecheri.ApplyBestFit(nil);
end;

procedure TfrmImperecheri.SetIdDocument(const Value: Integer);
var IsPlata   : Boolean;
    lNode     : TcxCustomGridRecord;
begin
  { Sa evitam ciclare }
  if FIdDocument = Value then Exit;

  FIdDocument := Value;

  if FIsInternalLoad then Exit;
  
  FIsInternalLoad := True;

  { Daca TabIndex in (2,3) => Furnizorul sau Clientul este pivot }
  if TabList.TabIndex div 3 = 1 then begin
     GridIncasari.DataController.Filter.Clear;
     { Luam nodul corect }
     lNode := GridDocumente.Controller.FocusedRecord;
     if Assigned(lNode) and lNode.IsData then begin
        FCurentFilter := '';
        if lNode.Values[GridDocumenteID_GEST_DOCUM.Index] <> Value then
           lNode := cxFindNodeByKeyValue(GridDocumente, Value);
        if Assigned(lNode) then begin
          { Setam daca avem nevoie de plati sau incasari }
          IsPlata := GetBoolean(lNode, GridDocumentePRIMITOR_INTERN.Index);
          if IsPlata then begin
             FIdRepartitor := lNode.Values[GridDocumenteID_PREDATOR.Index];
             FNumeRepartitor := lNode.DisplayTexts[GridDocumentePREDATOR.Index];
          end
          else begin
             FIdRepartitor := lNode.Values[GridDocumenteID_PRIMITOR.Index];
             FNumeRepartitor := lNode.DisplayTexts[GridDocumentePRIMITOR.Index];
          end;

          if IsPlata then GridIncasari.DataController.Filter.AddItem(nil, GridIncasariTIP_PLATA, foEqual, '1', 'Plati efectuate de unitate')
          else GridIncasari.DataController.Filter.AddItem(nil, GridIncasariTIP_PLATA, foEqual, '0', 'Incasari efectuate de unitate');

          if FIdRepartitor > 0 then GridIncasari.DataController.Filter.AddItem(nil, GridIncasariNUME, foEqual, FNumeRepartitor, 'Reparitorul :'+FNumeRepartitor);
        end;
     end;
  end;
  FSearchSuma := DisponibilDocum;
  SetCurentStatus;

  FIsInternalLoad := False;
end;

procedure TfrmImperecheri.BtnAddClick(Sender: TObject);
var
  lSuma : Currency;
begin
  edSumaPropertiesChange(edSuma);
  with GetTmpADOQuery do
    try
       lSuma := edSuma.Value;
       Sql.Add('exec spDecontContAdd :ID_GEST_DOCUM, :ID_BREGISTRU, :SUMA');
       Params[0].Value := FIdDocument;
       Params[1].Value := FIdDocCasa;
       Params[2].Value := lSuma;
       ExecSQL;
       RefreshIncasari;
    finally
       Free;
    end;
end;

procedure TfrmImperecheri.RefreshIncasari;
begin
  try
    QryIncasari.Close;
    QryDocumente.Close;

    QryImperecheri.Close;
    QryDocumente.Params.ParamByName('IS_PLATA').Value := 2;
    QryIncasari.Params.ParamByName('IsCasaBanca').Value := (TabList.TabIndex mod 3);

    if FDecontSpecificCod <> - 1 then
      QryIncasari.Params.ParamByName('SpecificCod').Value := FDecontSpecificCod
    else
      QryIncasari.Params.ParamByName('SpecificCod').Value := null;

    QryIncasari.Open;
    QryDocumente.Open;
    QryImperecheri.Open;

    {if TabList.TabIndex div 3 = 1 then
       GridIncasariChangeNode(GridIncasari, nil, GridIncasari.FocusedNode)
    else GridDocumenteChangeNode(GridDocumente, nil, GridDocumente.FocusedNode);}

  // TFrmReconcilereDecontari(Reconcilieri).SetDecontari;

  finally
    SetCurentStatus;
  end;
end;

function TfrmImperecheri.DisponibilDocum: Currency;
begin
  if (GridDocumente.ViewData.RecordCount = 0) then Result := 0
  else
    with QryDocumente do Result := FieldByName('TOTALDOC').AsCurrency - FieldByName('ASIGNAT').AsCurrency;
end;

function TfrmImperecheri.DisponibilIncasare: Currency;
begin
  if (GridIncasari.ViewData.RecordCount = 0) then Result := 0
  else
    with QryIncasari do Result := FieldByName('TOTAL').AsCurrency - FieldByName('ASIGNAT').AsCurrency;
end;

procedure TfrmImperecheri.BtnModifyClick(Sender: TObject);
var lNode: TcxCustomGridRecord;
  lSuma : Currency;
begin
  edSumaPropertiesChange(edSuma);
  lNode := GridImperecheri.Controller.FocusedRecord;
  if Assigned(lNode) and lNode.IsData and (MessageDlg('Doriti inlocuirea decontarii curente ?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         lSuma := edSuma.Value;
         Sql.Add('exec spDecontContUpdate :SUMA, :ID_BREGISTRU, :ID_GEST_DOCUM, '  + VarToStr(lNode.Values[GridImperecheriID_GEST_DECONTARI.Index]));
         Params[0].Value := lSuma;
         Params[1].Value := FIdDocCasa;
         Params[2].Value := FIdDocument;
         ExecSQL;
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmImperecheri.BtnDeleteClick(Sender: TObject);
var lNode: TcxCustomGridRecord;
begin
  lNode := GridImperecheri.Controller.FocusedRecord;
  if (Assigned(lNode)) and lNode.IsData and
     (MessageDlg('Doriti stergera conexiunii intre documentul de casa si cel de tranzactii?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         ParamCheck := False;
         Sql.Add('exec spDecontContDel ' + VarToStr(lNode.Values[GridImperecheriID_GEST_DECONTARI.Index]));
         ExecSQL;
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmImperecheri.BtnDefalcareClick(Sender: TObject);
var lCasaNode, lItemsiNode: TcxCustomGridRecord;
    lIdBreg, lDocum: Integer;
    lSuma : Currency;
begin
  // TODO Defalcarea
  lSuma := edSuma.Value;
  lCasaNode := GridIncasari.Controller.FocusedRecord;
  if Assigned(lCasaNode) and lCasaNode.IsData then lIdBreg := lCasaNode.Values[GridIncasariCOD.Index]
  else lIdBreg := -1;
  lItemsiNode := GridDocumente.Controller.FocusedRecord;
  if Assigned(lItemsiNode) and lItemsiNode.IsData then lDocum := lItemsiNode.Values[GridDocumenteID_GEST_DOCUM.Index]
  else lDocum := -1;
  if (lDocum > -1) and (lIdBreg > -1) then
     if EditDefalcare(lIdBreg, lDocum, lSuma) then RefreshIncasari;
end;

procedure TfrmImperecheri.Panel3Resize(Sender: TObject);
begin
  GrCasa.Height := Panel3.Height div 2; 
end;

procedure TfrmImperecheri.lGridDocumenteFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  if (GridDocumente.Controller.FocusedRecord = nil) or not (GridDocumente.Controller.FocusedRecord.IsData) then
    GridDocumenteFocusedRecordChanged(GridDocumente, nil, GridDocumente.ViewData.Records[0], False)
  else
    GridDocumenteFocusedRecordChanged(GridDocumente, nil, GridDocumente.Controller.FocusedRecord, False);
end;

procedure TfrmImperecheri.TabListChange(Sender: TObject);
begin
  GridIncasari.DataController.Filter.Clear;
  GridDocumente.DataController.Filter.Clear;
  if TabList.TabIndex >= 6 then Exit;
  if TabList.TabIndex div 3 = 0 then begin
     { Avem Casa -> Furnizor }
     FPivotGrid := GridIncasari;
     FSearchGrid := GridDocumente;
     cxGridDocumente.Parent := GrDocumente;
     cxGridIncasari.Parent  := GrCasa;
     GrCasa.Caption       := 'Lista inregistrarilor de casa';
     GrDocumente.Caption  := 'Lista documente (facturi, nir, etc.)';
  end
  else begin
     { Avem Furnizor -> Casa }
     FPivotGrid := GridDocumente;
     FSearchGrid := GridIncasari;
     cxGridDocumente.Parent := GrCasa;
     cxGridIncasari.Parent  := GrDocumente;
     GrDocumente.Caption  := 'Lista inregistrarilor de casa';
     GrCasa.Caption       := 'Lista documente (facturi, nir, etc.)';
  end;
  RefreshIncasari;
end;

procedure TfrmImperecheri.ppLocalizareClick(Sender: TObject);
var lCasa, lItemsi: Variant;
    lCasaNode, lItemsiNode: TcxCustomGridRecord;
    IsBanca : Boolean;
    Node    : TcxCustomGridRecord;
begin
  Node := GridImperecheri.Controller.FocusedRecord;
  { Ne Pozitionam pe pozitia corecta din casa si pozitia corecta din itemsi }
  if not Assigned(Node) then Exit;
  if not Node.IsData then Exit;
  Screen.Cursor := crHourGlass;
  try
    IsBanca := GetBoolean(Node, GridImperecheriIS_BANCA.Index);
    lCasa := Node.Values[GridImperecheriID_BREGISTRU.Index];
    lItemsi := Node.Values[GridImperecheriID_GEST_DOCUM.Index];
    { Daca nu suntem pe banca si avem banca => schimbam locatia }
    if TabList.TabIndex mod 3 <> 0 then begin
      if (IsBanca) and (TabList.TabIndex = 0) then TabList.TabIndex := 1;
      if (not IsBanca) and (TabList.TabIndex = 1) then TabList.TabIndex := 0;
    end;
    lCasaNode := cxFindNodeByKeyValue(GridIncasari, lCasa);
    if Assigned(lCasaNode) then begin
       lCasaNode.MakeVisible;
       lCasaNode.Focused := True;
    end;
    lItemsiNode := cxFindNodeByKeyValue(GridDocumente, lItemsi);
    if Assigned(lItemsiNode) then begin
       lItemsiNode.MakeVisible;
       lItemsiNode.Focused := True;
    end;
  finally
    Screen.Cursor := crDefault;
  end;
end;

procedure TfrmImperecheri.BtnAutoDecontClick(Sender: TObject);
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

procedure TfrmImperecheri.ppModificareDocumClick(Sender: TObject);
begin
  GridImperecheriDblClick(GridImperecheri);
end;

procedure TfrmImperecheri.ppDecontariPopup(Sender: TObject);
var Node   : TcxCustomGridRecord;
    I      : Integer;
    lWhere : String;
    lNewIdDocument: Integer;
    lIdDocum: String;

     procedure AddInnerItem(AIdDocum: Integer; ADesc: String);
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

begin
  if Sender <> nil then Exit;
  Node := GridImperecheri.Controller.FocusedRecord;

  { Distrugem documentele conexe anterioare }
  for I := ppReparareDoc.Count-1 downto 0 do
    ppReparareDoc.Items[I].Free;
  ppReparareDoc.Enabled := False;
  if not Assigned(Node)  then Exit;
  if not Node.IsData then Exit;

  { Tag-ul de pe bara de meniu de refacere contine id-ul din decontari care urmeaza sa fie modificat }
  ppReparareDoc.Tag := Node.Values[GridImperecheriID_GEST_DECONTARI.Index];
  lIdDocum := Trim(Node.DisplayTexts[GridImperecheriID_GEST_DOCUM.Index]);
  ppReparareDoc.Enabled := (Node.DisplayTexts[GridImperecheriSTARE.Index] <> '1') and (lIdDocum > '');
  { Daca nu este valid citim si lista de documente care pot fi reasignate }
  if ppReparareDoc.Enabled then begin
    { Daca este stare pe 0 in documente gest cautam documentul valid care a rezultat din documentul
      imperecheat }
      {??? lNewIdDocument := ParseAndGetNewIdDoc(lIdDocum);}
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
end;

procedure TfrmImperecheri.ReasignareDocum(Sender: TObject);
var lId: Integer;
begin
  if Assigned(Sender) then begin
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
end;

procedure TfrmImperecheri.SetIdDocCasa(const Value: Integer);
var lFilter   : String;
    IsPlata   : Boolean;
    lNode     : TcxCustomGridRecord;
begin
  { Sa nu permitem ciclare }
  if FIdDocCasa = Value then Exit;
  FIdDocCasa := Value;
  if FIsInternalLoad then Exit;
  FIsInternalLoad := True;

  { Daca TabIndex in (0,1) => Casa este pivot }
  if TabList.TabIndex div 3 = 0 then begin
     GridDocumente.DataController.Filter.Clear;
     { Luam nodul corect }
     lNode := GridIncasari.Controller.FocusedRecord;
     if Assigned(lNode) and lNode.IsData then begin
       if lNode.Values[GridIncasariCOD.Index] <> Value then
          lNode := cxFindNodeByKeyValue(GridIncasari, Value);
       if Assigned(lNode) then begin
         { Resetam repartitorul curent }
         IsPlata := lNode.Values[GridIncasariTIP_PLATA.Index] = '1';
         if Trim(lNode.DisplayTexts[GridIncasariCODGEST.Index]) > '' then FIdRepartitor := lNode.Values[GridIncasariCODGEST.Index]
         else FIdRepartitor := -1;
         FNumeRepartitor := Trim(lNode.DisplayTexts[GridIncasariNUME.Index]);

         // 1 = Plata, 0 = Incasare
         // C.GESTINT = Predator Intern
         // D.GESTINT = Primitor Intern

         if IsPlata then lFilter := '1'
         else lFilter := '0';
         { Verificam daca trebuie sa reactualizam documentele }
         if FCurentFilter <> lFilter then begin
            QryDocumente.Close;
            QryDocumente.Params.ParamByName('IS_PLATA').Value := StrToInt(lFilter);
            //QryDocumente.Sql[8] := lFilter;
            QryDocumente.Open;
            FCurentFilter := lFilter;
         end;
         { Daca avem repartitor facem automat filtrarea }
         if FIdRepartitor > 0 then
            if IsPlata then GridDocumente.DataController.Filter.AddItem(nil, GridDocumentePREDATOR, foEqual, FNumeRepartitor, 'Predator Document : '+FNumeRepartitor)
            else GridDocumente.DataController.Filter.AddItem(nil, GridDocumentePRIMITOR, foEqual, FNumeRepartitor, 'Primitor Document : '+FNumeRepartitor);
       end;
     end;
  end;
  FSearchSuma := DisponibilIncasare;
  SetCurentStatus;
  FIsInternalLoad := False;
end;

procedure TfrmImperecheri.SetCurentStatus;
var lValMax : Currency;
//    lNode  : TcxCustomGridRecord;
//    isAprox : Boolean;
    lRecIndex : Integer;
begin
  //isAprox := False;
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
       Sql.Add('SELECT ID_GEST_DECONTARI FROM GEST_DECONTARI WHERE ID_GEST_DOCUM = '+IntToStr(FIdDocument)+' AND ID_BREGISTRU = '+IntToStr(FIdDocCasa));
       Open;
         //folosim pivotul ca referinta
       if IsEmpty then
        begin
          Sql.Clear;
          Sql.Add('SELECT top 1 ID_GEST_DECONTARI FROM GEST_DECONTARI WHERE ');
          if GridIncasari = FPivotGrid then
            SQL.Add(' ID_BREGISTRU = '+IntToStr(FIdDocCasa))
          else
            SQL.Add(' ID_GEST_DOCUM = '+IntToStr(FIdDocument));
          Open;
//          isAprox := True;
         end;
       if not IsEmpty then begin
          lRecIndex := GridImperecheri.DataController.FindRecordIndexByKey(Fields[0].AsInteger);
          if lRecIndex <> -1 then
            GridImperecheri.Controller.FocusedRecordIndex := lRecIndex;
          {if isAprox then begin
            if FPivotGrid = GridIncasari then
               lNode := GridDocumente.FindNodeByKeyValue(lNode.Values[GridImperecheriID_GEST_DOCUM.Index])
            else
               lNode := GridIncasari.FindNodeByKeyValue(lNode.Values[GridImperecheriID_BREGISTRU.Index]);
            if Assigned(lNode) then begin
               lNode.MakeVisible;
               lNode.Focused := True;
            end;
          end;}
       end;
    finally
       Free;
    end;
end;

function TfrmImperecheri.GetDecontareText(ANode: TcxCustomGridRecord): String;
begin
  if Assigned(ANode) then
    with ANode do
      Result := DisplayTexts[GridImperecheriTIPDOC.Index]+' '+DisplayTexts[GridImperecheriNRDOC.Index]+' '+DisplayTexts[GridImperecheriDATA.Index]+' -> '+
                DisplayTexts[GridImperecheriCOD_DOCUM.Index]+' '+DisplayTexts[GridImperecheriPREDATOR.Index]+' '+DisplayTexts[GridImperecheriNR_DOCUM.Index]+' '+DisplayTexts[GridImperecheriDATA_DOCUM.Index]+
                DisplayTexts[GridImperecheriPRIMITOR.Index]
  else Result := 'Neasignat';
end;

procedure TfrmImperecheri.ppReconciliereClick(Sender: TObject);
begin
  { Luam toate pozitiile invalide si propunem solutii pentru fiecare in parte }
  ppReconciliere.Checked := not ppReconciliere.Checked;
  Reconcilieri.Visible := ppReconciliere.Checked;
end;

function TfrmImperecheri.Reconcilieri: TCustomForm;
begin
  if FReconciliere = nil then begin
     {TODO FReconciliere := TFrmReconcilereDecontari.Create(Self);
     TFrmReconcilereDecontari(FReconciliere).Decontari := Self;}
  end;
  Result := FReconciliere;
end;

procedure TfrmImperecheri.edSumaPropertiesChange(Sender: TObject);
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

procedure TfrmImperecheri.FormShow(Sender: TObject);
begin
   TabListChange(TabList);
   if (GridIncasari.ViewData.RecordCount > 0) and (GridIncasari.ViewData.Records[0] <> nil) then
     GridIncasariFocusedRecordChanged(GridIncasari, nil, GridIncasari.ViewData.Records[0], False);
end;

procedure TfrmImperecheri.TryToSearchSuma;
var aNode, oldNode : TcxCustomGridRecord;
    aCol : TcxGridColumn;
    gasit : Boolean;
    lSearchGrid : TcxGridDBTableView;
    lSearchSuma : Currency;
begin
 if FSearchSuma = 0 then Exit;
 if Assigned(FSearchGrid) then begin
   lSearchSuma := FSearchSuma;
   aCol := TcxGridDBColumn(FSearchGrid.GetColumnByFieldName('RAMAS'));
   if aCol = nil then begin
     if FSearchGrid = GridIncasari then aCol := GridIncasariTOTAL
                                   else aCol := GridDocumenteTOTALDOC;
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
     {if (aNode <> nil) and (lSearchGrid.FindColumnByFieldName('ASIGNAT') <> nil) and (aNode.Strings[lSearchGrid.FindColumnByFieldName('ASIGNAT').Index] <> '')
       and not(Trim(aNode.Strings[lSearchGrid.FindColumnByFieldName('ASIGNAT').Index]) = '0') then gasit := false;
     }
     if oldNode = aNode then gasit := True;
     oldNode := aNode;
   end;
 end;
end;

procedure TfrmImperecheri.lGridIncasariDblClick(Sender: TObject);
begin
  if FPivotGrid = GridIncasari then TryToSearchSuma;
end;

procedure TfrmImperecheri.lGridDocumenteDblClick(Sender: TObject);
begin
  if FPivotGrid = GridDocumente then TryToSearchSuma;
end;

procedure TfrmImperecheri.btnCautaAutomatClick(Sender: TObject);
begin
  TryToSearchSuma;
end;

procedure TfrmImperecheri.edSumaPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if (Error) and (edSuma.Value > edSuma.Properties.MaxValue) then begin
    DisplayValue := edSuma.Properties.MaxValue;
    Error := False;
  end;
end;

procedure TfrmImperecheri.lGridIncasariFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  if (GridIncasari.Controller.FocusedRecord = nil) or not(GridIncasari.Controller.FocusedRecord.IsData) then
    GridIncasariFocusedRecordChanged(GridIncasari, nil, GridIncasari.ViewData.Records[0], False)
  else
    GridIncasariFocusedRecordChanged(GridIncasari, nil, GridIncasari.Controller.FocusedRecord, False);
end;

procedure TfrmImperecheri.GridImperecheriCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var Invalid: Boolean;
begin
  if TcxGridDBBandedColumn(AViewInfo.Item).Position.BandIndex = 2 then begin
     Invalid := AViewInfo.GridRecord.Values[GridImperecheriSTARE.Index] = '0';
     if Invalid then begin
        ACanvas.Font.Style := ACanvas.Font.Style + [fsStrikeOut];
        ACanvas.Font.Color := clRed;
     end;
  end
  else
  if TcxGridDBBandedColumn(AViewInfo.Item).Position.BandIndex = 0 then begin
     Invalid :=AViewInfo.GridRecord.Values[GridImperecheriSTARE.Index] = '2';
     if Invalid then begin
        ACanvas.Font.Style := ACanvas.Font.Style + [fsStrikeOut];
        ACanvas.Font.Color := clRed;
     end;
  end;
end;

procedure TfrmImperecheri.GridImperecheriDblClick(Sender: TObject);
var lDecontNode: TcxCustomGridRecord;
    lDocum, lIdBreg    : Integer;
    lSuma : Currency;
begin
  lSuma := edSuma.Value;
  lDecontNode := GridImperecheri.Controller.FocusedRecord;
  if Assigned(lDecontNode) and lDecontNode.IsData then begin
     lIdBreg := lDecontNode.Values[GridImperecheriID_BREGISTRU.Index];
     lDocum  := lDecontNode.Values[GridImperecheriID_GEST_DOCUM.Index];
     if (lDocum > -1) and (lIdBreg > -1) then
        if EditDefalcare(lIdBreg, lDocum, lSuma) then RefreshIncasari;
  end;
end;

procedure TfrmImperecheri.GridIncasariPROCENTGetDisplayText(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  var AText: String);
var
  lCurent,
  lTotal : Currency;
begin
  { Afisam cat la suta este deja asignat }
  if Trim(VarToStr(ARecord.Values[GridIncasariTOTAL.Index])) <> '' then
     lTotal := ARecord.Values[GridIncasariTOTAL.Index]
  else lTotal := 0;

  if Trim(VarToStr(ARecord.Values[GridIncasariASIGNAT.Index])) <> '' then
     lCurent := ARecord.Values[GridIncasariASIGNAT.Index]
  else lCurent := 0;
  if lTotal > 0 then AText := CurrToStr(lCurent/lTotal * 100)
  else AText := '0';
end;

procedure TfrmImperecheri.GridIncasariPROCENTCustomDrawCell(
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

procedure TfrmImperecheri.GridIncasariFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) then IdDocCasa := AFocusedRecord.Values[GridIncasariCOD.Index];
end;

procedure TfrmImperecheri.GridDocumenteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and (AFocusedRecord.IsData) then IdDocument := AFocusedRecord.Values[GridDocumenteID_GEST_DOCUM.Index];
end;

procedure TfrmImperecheri.GridDocumentePROCENTCustomDrawCell(
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

procedure TfrmImperecheri.GridDocumentePROCENTGetDisplayText(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  var AText: String);
var
  lCurent,
  lTotal : Currency;
begin
  { Afisam cat la suta este deja asignat }
  if Trim(VarToStr(ARecord.Values[GridDocumenteTOTALDOC.Index])) <> '' then
     lTotal := ARecord.Values[GridDocumenteTOTALDOC.Index]
  else lTotal := 0;

  if Trim(VarToStr(ARecord.Values[GridDocumenteASIGNAT.Index])) <> '' then
     lCurent := ARecord.Values[GridDocumenteASIGNAT.Index]
  else lCurent := 0;
  if lTotal > 0 then AText := CurrToStr(lCurent/lTotal * 100)
  else AText := '0';
end;

end.

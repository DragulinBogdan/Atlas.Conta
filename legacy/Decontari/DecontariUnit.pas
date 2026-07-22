unit DecontariUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, dxCntner, dxTL, dxDBCtrl, dxDBGrid, cxControls,
  cxPC, Db, ZDataSet, dxDBTLCl, dxGrClms, dxEditor, dxExEdtr, Buttons, Menus,
  cxContainer, cxEdit, cxTextEdit, cxCurrencyEdit, cxCheckBox, cxLookAndFeelPainters,
  cxButtons, cxSplitter, ZAbstractRODataset, ZAbstractDataset, cxGraphics, cxLookAndFeels,
  dxBarBuiltInMenu, cxGroupBox;

type
  TfrmDecontari = class(TForm)
    Panel1: TPanel;
    Panel2: TPanel;
    GroupBox3: TcxGroupBox;
    GridImperecheri: TdxDBGrid;
    TabList: TcxTabControl;
    Panel3: TPanel;
    GrCasa: TcxGroupBox;
    GridIncasari: TdxDBGrid;
    GrDocumente: TcxGroupBox;
    GridDocumente: TdxDBGrid;
    DTIncasari: TDataSource;
    DTDocumente: TDataSource;
    DTImperecheri: TDataSource;
    QryIncasari: TZQuery;
    QryDocumente: TZQuery;
    QryImperecheri: TZQuery;
    GridIncasariTIPDOC: TdxDBGridMaskColumn;
    GridIncasariNRDOC: TdxDBGridMaskColumn;
    GridIncasariDATA: TdxDBGridDateColumn;
    GridIncasariEXPLICATIE: TdxDBGridMaskColumn;
    GridIncasariNUME: TdxDBGridMaskColumn;
    GridIncasariTIP_PLATA: TdxDBGridImageColumn;
    GridIncasariASIGNAT: TdxDBGridCurrencyColumn;
    GridIncasariTOTAL: TdxDBGridCurrencyColumn;
    GridDocumenteCOD_DOCUM: TdxDBGridMaskColumn;
    GridDocumenteNR_DOCUM: TdxDBGridMaskColumn;
    GridDocumenteDATA_DOCUM: TdxDBGridDateColumn;
    GridDocumentePREDATOR: TdxDBGridMaskColumn;
    GridDocumentePRIMITOR: TdxDBGridMaskColumn;
    GridDocumenteTOTALDOC: TdxDBGridCurrencyColumn;
    GridDocumenteASIGNAT: TdxDBGridCurrencyColumn;
    GridImperecheriTIPDOC: TdxDBGridMaskColumn;
    GridImperecheriNRDOC: TdxDBGridMaskColumn;
    GridImperecheriDATA: TdxDBGridDateColumn;
    GridImperecheriEXPLICATIE: TdxDBGridMaskColumn;
    GridImperecheriCOD_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriNR_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriDATA_DOCUM: TdxDBGridDateColumn;
    GridImperecheriPREDATOR: TdxDBGridMaskColumn;
    GridImperecheriPRIMITOR: TdxDBGridMaskColumn;
    GridIncasariCODGEST: TdxDBGridMaskColumn;
    pnTools: TPanel;
    edSuma: TcxCurrencyEdit;
    GridImperecheriID_GEST_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriID_BREGISTRU: TdxDBGridMaskColumn;
    GridImperecheriSUMA: TdxDBGridCurrencyColumn;
    ChkStyle: TdxCheckEditStyleController;
    GridIncasariPROCENT: TdxDBGridColumn;
    GridDocumentePROCENT: TdxDBGridColumn;
    GridIncasariDENUMIRE: TdxDBGridMaskColumn;
    GridImperecheriSTARE: TdxDBGridImageColumn;
    ppDecontari: TPopupMenu;
    ppReparareDoc: TMenuItem;
    ppModificareDocum: TMenuItem;
    ppLocalizare: TMenuItem;
    GridImperecheriIS_BANCA: TdxDBGridCheckColumn;
    GridDocumentePREDATOR_INTERN: TdxDBGridCheckColumn;
    GridDocumentePRIMITOR_INTERN: TdxDBGridCheckColumn;
    GridDocumenteID_PREDATOR: TdxDBGridMaskColumn;
    GridDocumenteID_PRIMITOR: TdxDBGridMaskColumn;
    ppReconciliere: TMenuItem;
    BtnAdd: TcxButton;
    BtnModify: TcxButton;
    BtnDelete: TcxButton;
    BtnDefalcare: TcxButton;
    BtnAutoDecont: TcxButton;
    btnCautaAutomat: TcxButton;
    Splitter1: TcxSplitter;
    Splitter2: TcxSplitter;
    GridIncasariRAMAS: TdxDBGridCurrencyColumn;
    GridDocumenteRAMAS: TdxDBGridCurrencyColumn;
    btnRefresh: TcxButton;
    procedure GridIncasariCustomDrawPreviewCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      ASelected: Boolean; var AText: String; var AColor,
      ATextColor: TColor; AFont: TFont; var ADone: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure GridDocumenteCustomDrawPreviewCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      ASelected: Boolean; var AText: String; var AColor,
      ATextColor: TColor; AFont: TFont; var ADone: Boolean);
    procedure GridIncasariChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure QryImperecheriAfterOpen(DataSet: TDataSet);
    procedure GridDocumenteChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure BtnAddClick(Sender: TObject);
    procedure BtnModifyClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure BtnDefalcareClick(Sender: TObject);
    procedure Panel3Resize(Sender: TObject);
    procedure GridDocumenteFilterChanged(Sender: TObject;
      ADataSet: TDataSet; const AFilterText: String);
    procedure GridIncasariPROCENTCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure GridDocumentePROCENTCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure TabListChange(Sender: TObject);
    procedure GridImperecheriDblClick(Sender: TObject);
    procedure GridImperecheriCustomDraw(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxDBTreeListColumn;
      const AText: String; AFont: TFont; var AColor: TColor; ASelected,
      AFocused: Boolean; var ADone: Boolean);
    procedure ppLocalizareClick(Sender: TObject);
    procedure BtnAutoDecontClick(Sender: TObject);
    procedure ppModificareDocumClick(Sender: TObject);
    procedure ppDecontariPopup(Sender: TObject);
    procedure GridIncasariPROCENTGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure ppReconciliereClick(Sender: TObject);
    procedure edSumaPropertiesChange(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure GridIncasariDblClick(Sender: TObject);
    procedure GridDocumenteDblClick(Sender: TObject);
    procedure btnCautaAutomatClick(Sender: TObject);
    procedure edSumaPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure GridIncasariFilterChanged(Sender: TObject;
      ADataSet: TDataSet; const AFilterText: String);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnRefreshClick(Sender: TObject);
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

    FPivotGrid : TdxDBGrid;
    FSearchGrid : TdxDBGrid;
    FSearchSuma : Currency;

    procedure ReasignareDocum(Sender: TObject);

    function Reconcilieri: TCustomForm;

    function DisponibilIncasare: Currency;
    function DisponibilDocum: Currency;
    function GetDecontareText(ANode: TdxTreeListNode): String;

    procedure DrawProcent ( ACanvas: TCanvas; ARect: TRect; AProcent : Integer);
    procedure SetIdDocument(const Value: Integer);
    procedure SetIdDocCasa(const Value: Integer);

    procedure SetCurentStatus;
    procedure TryToSearchSuma;

  public
    { Public declarations }
    FTabListNo    : Integer;
    procedure RefreshIncasari;

    property IdDocument: Integer read FIdDocument write SetIdDocument;
    property IdDocCasa : Integer read FIdDocCasa  write SetIdDocCasa;
  end;





 procedure DoDecont( BancaCod : Integer);

implementation

{$R *.DFM}

uses
  ZeosDBUtile, Variants, DateUnit, ReconciliereDecontariUnit, dxGridRefresher, DefalcareDecontareUnit,
  CommonDBVar;

procedure DoDecont( BancaCod : Integer);
var Cod : Integer;
begin
  with TfrmDecontari.Create(Application) do
    try
       FDecontSpecificCod := BancaCod;
       //GrCasa.Visible := False;
       TabList.Visible := False;
       FTabListNo := 0;
       RefreshIncasari;
       if QryIncasari.IsEmpty then
         Cod := BancaCod
       else
         Cod := QryIncasari.FieldByName('COD').AsInteger;
       IdDocCasa := Cod;
       ppLocalizareClick(nil);
       ShowModal;
    finally
       Free;
    end;
end;

procedure TfrmDecontari.GridIncasariCustomDrawPreviewCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  ASelected: Boolean; var AText: String; var AColor, ATextColor: TColor;
  AFont: TFont; var ADone: Boolean);
var
  lProcent: Currency;
begin
  { Afisam cat la suta este deja asignat }
  ACanvas.Brush.Color := clWindow;
  ACanvas.FillRect(ARect);
  if (Trim(ANode.Strings[GridIncasariPROCENT.Index]) > '') and
     (not VarIsEmpty(ANode.Values[GridIncasariPROCENT.Index])) and
     (not VarIsNull(ANode.Values[GridIncasariPROCENT.Index])) then
     lProcent := ANode.Values[GridIncasariPROCENT.Index]
  else lProcent := 0;

  DrawProcent(ACanvas, ARect, Trunc(lProcent * 100));
  ADone := True;
end;

procedure TfrmDecontari.FormCreate(Sender: TObject);
begin
  FIsInLoading := False;
  FDecontSpecificCod := -1;
  FPivotGrid := GridIncasari;
  FSearchGrid := GridDocumente;
  FTabListNo := -1;  
  //WindowState := wsMaximized;
end;

procedure TfrmDecontari.DrawProcent(ACanvas: TCanvas; ARect: TRect; AProcent: Integer);
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

procedure TfrmDecontari.GridDocumenteCustomDrawPreviewCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  ASelected: Boolean; var AText: String; var AColor, ATextColor: TColor;
  AFont: TFont; var ADone: Boolean);
var
  lProcent: Currency;
begin
  { Afisam cat la suta este deja asignat }
  ACanvas.Brush.Color := clWindow;
  ACanvas.FillRect(ARect);
  if (Trim(ANode.Strings[GridDocumentePROCENT.Index]) > '') and
     (not VarIsEmpty(ANode.Values[GridDocumentePROCENT.Index])) and
     (not VarIsNull(ANode.Values[GridDocumentePROCENT.Index])) then
     lProcent := ANode.Values[GridDocumentePROCENT.Index]
  else lProcent := 0;

  DrawProcent(ACanvas, ARect, Trunc(lProcent * 100));
  ADone := True;
end;

procedure TfrmDecontari.GridIncasariChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Assigned(Node) then IdDocCasa := TdxDBGridNode(Node).Id;
end;

procedure TfrmDecontari.QryImperecheriAfterOpen(DataSet: TDataSet);
begin
  GridImperecheri.ApplyBestFit(nil);
end;

procedure TfrmDecontari.GridDocumenteChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Assigned(Node) then IdDocument := TdxDBGridNode(Node).Id;
end;

procedure TfrmDecontari.SetIdDocument(const Value: Integer);
var IsPlata   : Boolean;
    lNode     : TdxTreeListNode;
begin
  { Sa evitam ciclare }
  if FIdDocument = Value then Exit;

  FIdDocument := Value;

  if FIsInternalLoad then Exit;
  
  FIsInternalLoad := True;

  { Daca TabIndex in (2,3) => Furnizorul sau Clientul este pivot }
  if TabList.TabIndex div 3 = 1 then begin
     GridIncasari.Filter.Clear;
     { Luam nodul corect }
     lNode := GridDocumente.FocusedNode;
     if Assigned(lNode) then begin
        FCurentFilter := '';
        if TdxDBGridNode(lNode).Id <> Value then
           lNode := GridDocumente.FindNodeByKeyValue(Value);
        if Assigned(lNode) then begin
          { Setam daca avem nevoie de plati sau incasari }
          IsPlata :=  GetBoolean(lNode.Values[GridDocumentePRIMITOR_INTERN.Index]);
          if IsPlata then begin
             FIdRepartitor := lNode.Values[GridDocumenteID_PREDATOR.Index];
             FNumeRepartitor := lNode.Strings[GridDocumentePREDATOR.Index];
          end
          else begin
             FIdRepartitor := lNode.Values[GridDocumenteID_PRIMITOR.Index];
             FNumeRepartitor := lNode.Strings[GridDocumentePRIMITOR.Index];
          end;

          if IsPlata then GridIncasari.Filter.Add(GridIncasariTIP_PLATA, '1', 'Plati efectuate de unitate')
          else GridIncasari.Filter.Add(GridIncasariTIP_PLATA, '0', 'Incasari efectuate de unitate');

          if FIdRepartitor > 0 then GridIncasari.Filter.Add(GridIncasariNUME, FNumeRepartitor, 'Reparitorul :'+FNumeRepartitor);
        end;
     end;
  end;
  FSearchSuma := DisponibilDocum;
  SetCurentStatus;

  FIsInternalLoad := False;
end;

procedure TfrmDecontari.BtnAddClick(Sender: TObject);
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

procedure TfrmDecontari.RefreshIncasari;
begin
  QryDocumente.Params.ParamByName('IS_PLATA').Value := 2;
  QryIncasari.Params.ParamByName('IsCasaBanca').Value := (TabList.TabIndex mod 3);
  if FDecontSpecificCod <> - 1 then
    QryIncasari.Params.ParamByName('SpecificCod').Value := FDecontSpecificCod
  else
    QryIncasari.Params.ParamByName('SpecificCod').Value := null;
  DBRefresh([QryIncasari, QryDocumente, QryImperecheri]);
  TFrmReconcilereDecontari(Reconcilieri).SetDecontari;
  SetCurentStatus;
end;

function TfrmDecontari.DisponibilDocum: Currency;
begin
  if GridIncasari.Count = 0 then  Result := 0
  else
    with QryDocumente do Result := FieldByName('TOTALDOC').AsCurrency - FieldByName('ASIGNAT').AsCurrency;
end;

function TfrmDecontari.DisponibilIncasare: Currency;
begin
  if GridDocumente.Count = 0 then Result := 0
  else
    with QryIncasari do Result := FieldByName('TOTAL').AsCurrency - FieldByName('ASIGNAT').AsCurrency;
end;

procedure TfrmDecontari.BtnModifyClick(Sender: TObject);
var lNode: TdxDBGridNode;
  lSuma : Currency;
begin
  edSumaPropertiesChange(edSuma);
  lNode := TdxDBGridNode(GridImperecheri.FocusedNode);
  if Assigned(lNode) and (MessageDlg('Doriti inlocuirea decontarii curente ?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         lSuma := edSuma.Value;
         Sql.Add('exec spDecontContUpdate :SUMA, :ID_BREGISTRU, :ID_GEST_DOCUM, '  + VarToStr(lNode.Id));
         //Sql.Add('UPDATE GEST_DECONTARI SET SUMA = :SUMA, ID_BREGISTRU = :ID_BREGISTRU, ID_GEST_DOCUM = :ID_GEST_DOCUM WHERE ID_GEST_DECONTARI = '+VarToStr(lNode.Id));
         Params[0].Value := lSuma;
         Params[1].Value := FIdDocCasa;
         Params[2].Value := FIdDocument;
         ExecSQL;
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmDecontari.BtnDeleteClick(Sender: TObject);
var lNode: TdxDBGridNode;
begin
  lNode := TdxDBGridNode(GridImperecheri.FocusedNode);
  if (Assigned(lNode)) and
     (MessageDlg('Doriti stergera conexiunii intre documentul de casa si cel de tranzactii?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         ParamCheck := False;
         Sql.Add('exec spDecontContDel ' + VarToStr(lNode.Id));
         ExecSQL;
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmDecontari.BtnDefalcareClick(Sender: TObject);
var lCasaNode, lItemsiNode: TdxDBGridNode;
    lIdBreg, lDocum: Integer;
    lSuma : Currency;
begin
  // TODO Defalcarea
  lSuma := edSuma.Value;
  lCasaNode := TdxDBGridNode(GridIncasari.FocusedNode);
  if Assigned(lCasaNode) then lIdBreg := lCasaNode.Id
  else lIdBreg := -1;
  lItemsiNode := TdxDBGridNode(GridDocumente.FocusedNode);
  if Assigned(lItemsiNode) then lDocum := lItemsiNode.Id
  else lDocum := -1;
  if (lDocum > -1) and (lIdBreg > -1) then
     if EditDefalcare(lIdBreg, lDocum, lSuma) then RefreshIncasari;
end;

procedure TfrmDecontari.Panel3Resize(Sender: TObject);
begin
  GrCasa.Height := Panel3.Height div 2; 
end;

procedure TfrmDecontari.GridDocumenteFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  if GridDocumente.FocusedNode = nil then
    GridDocumenteChangeNode(GridDocumente, nil, GridDocumente.TopNode)
  else
  GridDocumenteChangeNode(GridDocumente, nil, GridDocumente.FocusedNode);
end;

procedure TfrmDecontari.GridIncasariPROCENTCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var ATextColor : TColor;
begin
  GridIncasariCustomDrawPreviewCell(Sender, ACanvas, ARect,ANode, ASelected, AText,
                                    AColor, ATextColor, AFont, ADone);
end;

procedure TfrmDecontari.GridDocumentePROCENTCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var ATextColor : TColor;
begin
  GridDocumenteCustomDrawPreviewCell(Sender, ACanvas, ARect,ANode, ASelected, AText,
                                    AColor, ATextColor, AFont, ADone);
end;

procedure TfrmDecontari.TabListChange(Sender: TObject);
begin
  if FTabListNo = TabList.TabIndex then Exit;
  FTabListNo := TabList.TabIndex;
  GridIncasari.Filter.Clear;
  GridDocumente.Filter.Clear;
  if TabList.TabIndex >= 6 then Exit;
  if TabList.TabIndex div 3 = 0 then begin
     { Avem Casa -> Furnizor }
     FPivotGrid := GridIncasari;
     FSearchGrid := GridDocumente;
     GridDocumente.Parent := GrDocumente;
     GridIncasari.Parent  := GrCasa;
     GrCasa.Caption       := 'Lista inregistrarilor de casa';
     GrDocumente.Caption  := 'Lista documente (facturi, nir, etc.)';
  end
  else begin
     { Avem Furnizor -> Casa }
     FPivotGrid := GridDocumente;
     FSearchGrid := GridIncasari;
     GridDocumente.Parent := GrCasa;
     GridIncasari.Parent  := GrDocumente;
     GrDocumente.Caption  := 'Lista inregistrarilor de casa';
     GrCasa.Caption       := 'Lista documente (facturi, nir, etc.)';
  end;
  RefreshIncasari;
end;

procedure TfrmDecontari.GridImperecheriDblClick(Sender: TObject);
var lDecontNode: TdxDBGridNode;
    lDocum, lIdBreg    : Integer;
    lSuma : Currency;  
begin
  lSuma := edSuma.Value;
  lDecontNode := TdxDBGridNode(GridImperecheri.FocusedNode);
  if Assigned(lDecontNode) then begin
     lIdBreg := lDecontNode.Values[GridImperecheriID_BREGISTRU.Index];
     lDocum  := lDecontNode.Values[GridImperecheriID_GEST_DOCUM.Index];
     if (lDocum > -1) and (lIdBreg > -1) then
        if EditDefalcare(lIdBreg, lDocum, lSuma) then RefreshIncasari;
  end;
end;

procedure TfrmDecontari.GridImperecheriCustomDraw(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxDBTreeListColumn; const AText: String; AFont: TFont;
  var AColor: TColor; ASelected, AFocused: Boolean; var ADone: Boolean);
var Invalid: Boolean;
begin
  if AColumn.BandIndex = 2 then begin
     Invalid := ANode.Strings[GridImperecheriSTARE.Index] = '0';
     if Invalid then begin
        AFont.Style := AFont.Style + [fsStrikeOut];
        AFont.Color := clRed; 
     end;
  end
  else if AColumn.BandIndex = 0 then begin
     Invalid := ANode.Strings[GridImperecheriSTARE.Index] = '2';
     if Invalid then begin
        AColor := clRed;
        AFont.Style := AFont.Style + [fsStrikeOut];
        AFont.Color := clRed;
     end;
  end;
end;

procedure TfrmDecontari.ppLocalizareClick(Sender: TObject);
var lCasa, lItemsi: Variant;
    lCasaNode, lItemsiNode: TdxDBGridNode;
    IsBanca : Boolean;
    Node    : TdxTreeListNode;
begin
  Node := GridImperecheri.FocusedNode;
  { Ne Pozitionam pe pozitia corecta din casa si pozitia corecta din itemsi }
  if not Assigned(Node) then Exit;
  Screen.Cursor := crHourGlass;
  try
    IsBanca := GetBoolean(Node.Values[GridImperecheriIS_BANCA.Index]);
    lCasa := Node.Values[GridImperecheriID_BREGISTRU.Index];
    lItemsi := Node.Values[GridImperecheriID_GEST_DOCUM.Index];
    { Daca nu suntem pe banca si avem banca => schimbam locatia }
    if TabList.TabIndex mod 3 <> 0 then begin
      if (IsBanca) and (TabList.TabIndex = 0) then TabList.TabIndex := 1;
      if (not IsBanca) and (TabList.TabIndex = 1) then TabList.TabIndex := 0;
    end;
    lCasaNode := GridIncasari.FindNodeByKeyValue(lCasa);
    if Assigned(lCasaNode) then begin
       lCasaNode.MakeVisible;
       lCasaNode.Focused := True;
    end;
    lItemsiNode := GridDocumente.FindNodeByKeyValue(lItemsi);
    if Assigned(lItemsiNode) then begin
       lItemsiNode.MakeVisible;
       lItemsiNode.Focused := True;
    end;
  finally
    Screen.Cursor := crDefault;
  end;
end;

procedure TfrmDecontari.BtnAutoDecontClick(Sender: TObject);
begin
  if MessageDlg('Doriti imperecherea automata a facturilor cu platile corespunzatoare?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    DBExecSQLFmt('exec [spDecontareAutomataFactur] %d, %d', [iUserId, iSessionId]);
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

procedure TfrmDecontari.ppModificareDocumClick(Sender: TObject);
begin
  GridImperecheriDblClick(GridImperecheri);
end;

procedure TfrmDecontari.ppDecontariPopup(Sender: TObject);
var Node   : TdxTreeListNode;
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
  Node := GridImperecheri.FocusedNode;

  { Distrugem documentele conexe anterioare }
  for I := ppReparareDoc.Count-1 downto 0 do
    ppReparareDoc.Items[I].Free;
  ppReparareDoc.Enabled := False;
  if not Assigned(Node) then Exit;

  { Tag-ul de pe bara de meniu de refacere contine id-ul din decontari care urmeaza sa fie modificat }
  ppReparareDoc.Tag := TdxDBGridNode(Node).Id;
  lIdDocum := Trim(Node.Strings[GridImperecheriID_GEST_DOCUM.Index]);
  ppReparareDoc.Enabled := (Node.Strings[GridImperecheriSTARE.Index] <> '1') and (lIdDocum > '');
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
end;

procedure TfrmDecontari.ReasignareDocum(Sender: TObject);
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

procedure TfrmDecontari.SetIdDocCasa(const Value: Integer);
var lFilter   : String;
    IsPlata   : Boolean;
    lNode     : TdxTreeListNode;  
begin
  { Sa nu permitem ciclare }
  if FIdDocCasa = Value then Exit;
  FIdDocCasa := Value;
  if FIsInternalLoad then Exit;
  FIsInternalLoad := True;

  { Daca TabIndex in (0,1) => Casa este pivot }
  if TabList.TabIndex div 3 = 0 then begin
     GridDocumente.Filter.Clear;
     { Luam nodul corect }
     lNode := GridIncasari.FocusedNode;
     if Assigned(lNode) then begin
       if TdxDBGridNode(lNode).Id <> Value then
          lNode := GridIncasari.FindNodeByKeyValue(Value);
       if Assigned(lNode) then begin
         { Resetam repartitorul curent }
         IsPlata := lNode.Strings[GridIncasariTIP_PLATA.Index] = '1';
         if Trim(lNode.Strings[GridIncasariCODGEST.Index]) > '' then FIdRepartitor := lNode.Values[GridIncasariCODGEST.Index]
         else FIdRepartitor := -1;
         FNumeRepartitor := Trim(lNode.Strings[GridIncasariNUME.Index]);

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
            if IsPlata then GridDocumente.Filter.Add(GridDocumentePREDATOR, FNumeRepartitor, 'Predator Document : '+FNumeRepartitor)
            else GridDocumente.Filter.Add(GridDocumentePRIMITOR, FNumeRepartitor, 'Primitor Document : '+FNumeRepartitor);
            
       end;
     end;
  end;
  FSearchSuma := DisponibilIncasare;
  SetCurentStatus;
  FIsInternalLoad := False;
end;

procedure TfrmDecontari.SetCurentStatus;
var lValMax : Currency;
    lNode  : TdxDBGridNode;
//    isAprox : Boolean;
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
          //isAprox := True;
         end;
       if not IsEmpty then begin
          lNode := GridImperecheri.FindNodeByKeyValue(Fields[0].AsInteger);
          if Assigned(lNode) then begin
             lNode.MakeVisible;
             lNode.Focused := True;
          end;
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

function TfrmDecontari.GetDecontareText(ANode: TdxTreeListNode): String;
begin
  if Assigned(ANode) then
    with ANode do
      Result := Strings[GridImperecheriTIPDOC.Index]+' '+Strings[GridImperecheriNRDOC.Index]+' '+Strings[GridImperecheriDATA.Index]+' -> '+
                Strings[GridImperecheriCOD_DOCUM.Index]+' '+Strings[GridImperecheriPREDATOR.Index]+' '+Strings[GridImperecheriNR_DOCUM.Index]+' '+Strings[GridImperecheriDATA_DOCUM.Index]+
                Strings[GridImperecheriPRIMITOR.Index]
  else Result := 'Neasignat';
end;

procedure TfrmDecontari.GridIncasariPROCENTGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var
  lCurent,
  lTotal : Currency;
begin
  { Afisam cat la suta este deja asignat }
  if ANode.Strings[GridIncasariTOTAL.Index] > '' then
     lTotal := ANode.Values[GridIncasariTOTAL.Index]
  else lTotal := 0;

  if ANode.Strings[GridIncasariASIGNAT.Index] > '' then
     lCurent := ANode.Values[GridIncasariASIGNAT.Index]
  else lCurent := 0;
  if lTotal > 0 then AText := CurrToStr(lCurent/lTotal * 100)
  else AText := '0';
end;

procedure TfrmDecontari.ppReconciliereClick(Sender: TObject);
begin
  { Luam toate pozitiile invalide si propunem solutii pentru fiecare in parte }
  ppReconciliere.Checked := not ppReconciliere.Checked;
  Reconcilieri.Visible := ppReconciliere.Checked;
end;

function TfrmDecontari.Reconcilieri: TCustomForm;
begin
  if FReconciliere = nil then begin
     FReconciliere := TFrmReconcilereDecontari.Create(Self);
     TFrmReconcilereDecontari(FReconciliere).Decontari := Self;
  end;
  Result := FReconciliere;
end;

procedure TfrmDecontari.edSumaPropertiesChange(Sender: TObject);
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

procedure TfrmDecontari.FormShow(Sender: TObject);
begin
   TabListChange(TabList);
   if GridIncasari.TopNode <> nil then
     GridIncasariChangeNode(GridIncasari, nil, GridIncasari.TopNode);
end;
type
 TCrackATSGrid = class(TdxDBGrid);

procedure TfrmDecontari.TryToSearchSuma;
var aNode,oldNode : TdxTreeListNode;
    aCol : TdxDBGridColumn;
    gasit : Boolean;
    lSearchType : TdxTLSearchType;
    lSearchGrid : TdxDBGrid;
    lSearchSuma : Currency;
begin
 if FSearchSuma = 0 then begin
   ppLocalizareClick(nil);
   Exit;
 end;
 if Assigned(FSearchGrid) then begin
   aNode := nil;
   lSearchSuma := FSearchSuma;
   aCol := TdxDBGridColumn(FSearchGrid.FindColumnByFieldName('RAMAS'));
   if aCol = nil then begin
     if FSearchGrid = GridIncasari then aCol := TdxDBGridColumn(GridIncasariTOTAL)
                                   else aCol := TdxDBGridColumn(GridDocumenteTOTALDOC);
   end;
   lSearchGrid := FSearchGrid;
   gasit := False;
   oldNode := nil;
   lSearchType := lSearchGrid.SearchType;
   lSearchGrid.SearchType := stExact;
   if Assigned(lSearchGrid.TopNode) then begin
     lSearchGrid.TopNode.Focused := True;
     lSearchGrid.TopNode.MakeVisible;
   end;
   while not gasit do begin
     TCrackATSGrid(lSearchGrid).FindNodeByText(aCol.Index, CurrToStr(lSearchSuma), sdDown, aNode);
     gasit := True;
     if (aNode <> nil) and (aNode.IsVisible) then begin
        aNode.Focused := True;
        aNode.MakeVisible;
     end;
     if (aNode <> nil) and (aNode.Strings[aCol.Index] <> '') and (not (aNode.Values[aCol.Index]=lSearchSuma)) then gasit := false;
     {if (aNode <> nil) and (lSearchGrid.FindColumnByFieldName('ASIGNAT') <> nil) and (aNode.Strings[lSearchGrid.FindColumnByFieldName('ASIGNAT').Index] <> '')
       and not(Trim(aNode.Strings[lSearchGrid.FindColumnByFieldName('ASIGNAT').Index]) = '0') then gasit := false;
     }
     if oldNode = aNode then gasit := True;
     oldNode := aNode;
   end;
   lSearchGrid.SearchType := lSearchType;
 end;
end;

procedure TfrmDecontari.GridIncasariDblClick(Sender: TObject);
begin
  if FPivotGrid = GridIncasari then TryToSearchSuma;
end;

procedure TfrmDecontari.GridDocumenteDblClick(Sender: TObject);
begin
  if FPivotGrid = GridDocumente then TryToSearchSuma;
end;

procedure TfrmDecontari.btnCautaAutomatClick(Sender: TObject);
begin
  TryToSearchSuma;
end;

procedure TfrmDecontari.edSumaPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if (Error) and (edSuma.Value > edSuma.Properties.MaxValue) then begin
    DisplayValue := edSuma.Properties.MaxValue;
    Error := False;
  end;
end;

procedure TfrmDecontari.GridIncasariFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  if GridIncasari.FocusedNode = nil then
    GridIncasariChangeNode(GridIncasari, nil, GridIncasari.TopNode)
  else
    GridIncasariChangeNode(GridIncasari, nil, GridIncasari.FocusedNode);
end;

procedure TfrmDecontari.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmDecontari.btnRefreshClick(Sender: TObject);
begin
   RefreshIncasari;
end;

end.

unit DecontariCompensareUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, dxCntner, dxTL, dxDBCtrl, dxDBGrid, cxControls,
  cxPC, Db, ZDataSet, dxDBTLCl, dxGrClms, dxEditor, dxExEdtr,
  Buttons, Menus,
  cxContainer, cxEdit, cxTextEdit, cxCurrencyEdit, cxCheckBox,
  cxLookAndFeelPainters, cxButtons, cxSplitter,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels, dxBarBuiltInMenu;


type
  TfrmCompensari = class(TForm)
    Panel2: TPanel;
    GroupBox3: TGroupBox;
    GridImperecheri: TdxDBGrid;
    TabList: TcxTabControl;
    Panel3: TPanel;
    GrCasa: TGroupBox;
    GridFurnizori: TdxDBGrid;
    GrDocumente: TGroupBox;
    GridClienti: TdxDBGrid;
    DTFurnizori: TDataSource;
    DTClienti: TDataSource;
    DTImperecheri: TDataSource;
    QryFurnizori: TZQuery;
    QryClienti: TZQuery;
    QryImperecheri: TZQuery;

    GridFurnizoriCOD_DOCUM: TdxDBGridMaskColumn;
    GridFurnizoriNR_DOCUM: TdxDBGridMaskColumn;
    GridFurnizoriDATA_DOCUM: TdxDBGridDateColumn;
    GridFurnizoriPREDATOR: TdxDBGridMaskColumn;
    GridFurnizoriPRIMITOR: TdxDBGridMaskColumn;
    GridFurnizoriTOTALDOC: TdxDBGridCurrencyColumn;
    GridFurnizoriASIGNAT: TdxDBGridCurrencyColumn;
    GridFurnizoriPREDATOR_INTERN: TdxDBGridCheckColumn;
    GridFurnizoriPRIMITOR_INTERN: TdxDBGridCheckColumn;
    GridFurnizoriID_PREDATOR: TdxDBGridMaskColumn;
    GridFurnizoriID_PRIMITOR: TdxDBGridMaskColumn;
    GridFurnizoriPROCENT: TdxDBGridColumn;
    GridFurnizoriRAMAS: TdxDBGridCurrencyColumn;


    GridClientiCOD_DOCUM: TdxDBGridMaskColumn;
    GridClientiNR_DOCUM: TdxDBGridMaskColumn;
    GridClientiDATA_DOCUM: TdxDBGridDateColumn;
    GridClientiPREDATOR: TdxDBGridMaskColumn;
    GridClientiPRIMITOR: TdxDBGridMaskColumn;
    GridClientiTOTALDOC: TdxDBGridCurrencyColumn;
    GridClientiASIGNAT: TdxDBGridCurrencyColumn;
    GridClientiPREDATOR_INTERN: TdxDBGridCheckColumn;
    GridClientiPRIMITOR_INTERN: TdxDBGridCheckColumn;
    GridClientiID_PREDATOR: TdxDBGridMaskColumn;
    GridClientiID_PRIMITOR: TdxDBGridMaskColumn;
    GridClientiPROCENT: TdxDBGridColumn;
    GridClientiRAMAS: TdxDBGridCurrencyColumn;
    pnTools: TPanel;
    edSuma: TcxCurrencyEdit;
    GridImperecheriSUMA: TdxDBGridCurrencyColumn;
    GridImperecheriSTARE: TdxDBGridImageColumn;
    GridImperecheriFURNIZOR_COD_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriFURNIZOR_NR_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriFURNIZOR_DATA_DOCUM: TdxDBGridDateColumn;
    GridImperecheriFURNIZOR_PREDATOR: TdxDBGridMaskColumn;
    GridImperecheriFURNIZOR_PRIMITOR: TdxDBGridMaskColumn;
    GridImperecheriFURNIZOR_ID_GEST_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriCLIENT_COD_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriCLIENT_NR_DOCUM: TdxDBGridMaskColumn;
    GridImperecheriCLIENT_DATA_DOCUM: TdxDBGridDateColumn;
    GridImperecheriCLIENT_PREDATOR: TdxDBGridMaskColumn;
    GridImperecheriCLIENT_PRIMITOR: TdxDBGridMaskColumn;
    GridImperecheriCLIENT_ID_GEST_DOCUM: TdxDBGridMaskColumn;
    ChkStyle: TdxCheckEditStyleController;


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

    btnRefresh: TcxButton;
    procedure GridFurnizoriCustomDrawPreviewCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      ASelected: Boolean; var AText: String; var AColor,
      ATextColor: TColor; AFont: TFont; var ADone: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure GridClientiCustomDrawPreviewCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      ASelected: Boolean; var AText: String; var AColor,
      ATextColor: TColor; AFont: TFont; var ADone: Boolean);
    procedure GridFurnizoriChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure QryImperecheriAfterOpen(DataSet: TDataSet);
    procedure GridClientiChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure BtnAddClick(Sender: TObject);
    procedure BtnModifyClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure BtnDefalcareClick(Sender: TObject);
    procedure Panel3Resize(Sender: TObject);
    procedure GridClientiFilterChanged(Sender: TObject;
      ADataSet: TDataSet; const AFilterText: String);
    procedure GridFurnizoriPROCENTCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure GridClientiPROCENTCustomDrawCell(Sender: TObject;
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
    procedure GridFurnizoriPROCENTGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure ppReconciliereClick(Sender: TObject);
    procedure edSumaPropertiesChange(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure GridFurnizoriDblClick(Sender: TObject);
    procedure GridClientiDblClick(Sender: TObject);
    procedure btnCautaAutomatClick(Sender: TObject);
    procedure edSumaPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure GridFurnizoriFilterChanged(Sender: TObject;
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
  ZeosDBUtile, Variants, DateUnit, DefalcareDecontareUnit;



procedure DoDecont( BancaCod : Integer);
var Cod : Integer;
begin
  with TfrmCompensari.Create(Application) do
    try
       FDecontSpecificCod := BancaCod;
       //GrCasa.Visible := False;
       TabList.Visible := False;
       FTabListNo := 0;
       RefreshIncasari;
       if QryFurnizori.IsEmpty then
         Cod := BancaCod
       else
         Cod := QryFurnizori.FieldByName('COD').AsInteger;
       IdDocCasa := Cod;
       ppLocalizareClick(nil);
       ShowModal;
    finally
       Free;
    end;
end;

procedure TfrmCompensari.GridFurnizoriCustomDrawPreviewCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  ASelected: Boolean; var AText: String; var AColor, ATextColor: TColor;
  AFont: TFont; var ADone: Boolean);
var
  lProcent: Currency;
begin
  { Afisam cat la suta este deja asignat }
  ACanvas.Brush.Color := clWindow;
  ACanvas.FillRect(ARect);
  if (Trim(ANode.Strings[GridFurnizoriPROCENT.Index]) > '') and
     (not VarIsEmpty(ANode.Values[GridFurnizoriPROCENT.Index])) and
     (not VarIsNull(ANode.Values[GridFurnizoriPROCENT.Index])) then
     lProcent := ANode.Values[GridFurnizoriPROCENT.Index]
  else lProcent := 0;

  DrawProcent(ACanvas, ARect, Trunc(lProcent * 100));
  ADone := True;
end;

procedure TfrmCompensari.FormCreate(Sender: TObject);
begin
  FIsInLoading := False;
  FDecontSpecificCod := -1;
  FPivotGrid := GridFurnizori;
  FSearchGrid := GridClienti;
  FTabListNo := -1;  
  //WindowState := wsMaximized;
end;

procedure TfrmCompensari.DrawProcent(ACanvas: TCanvas; ARect: TRect; AProcent: Integer);
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

procedure TfrmCompensari.GridClientiCustomDrawPreviewCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  ASelected: Boolean; var AText: String; var AColor, ATextColor: TColor;
  AFont: TFont; var ADone: Boolean);
var
  lProcent: Currency;
begin
  { Afisam cat la suta este deja asignat }
  ACanvas.Brush.Color := clWindow;
  ACanvas.FillRect(ARect);
  if (Trim(ANode.Strings[GridClientiPROCENT.Index]) > '') and
     (not VarIsEmpty(ANode.Values[GridClientiPROCENT.Index])) and
     (not VarIsNull(ANode.Values[GridClientiPROCENT.Index])) then
     lProcent := ANode.Values[GridClientiPROCENT.Index]
  else lProcent := 0;

  DrawProcent(ACanvas, ARect, Trunc(lProcent * 100));
  ADone := True;
end;

procedure TfrmCompensari.GridFurnizoriChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Assigned(Node) then IdDocCasa := TdxDBGridNode(Node).Id;
end;

procedure TfrmCompensari.QryImperecheriAfterOpen(DataSet: TDataSet);
begin
  GridImperecheri.ApplyBestFit(nil);
end;

procedure TfrmCompensari.GridClientiChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Assigned(Node) then IdDocument := TdxDBGridNode(Node).Id;
end;

procedure TfrmCompensari.SetIdDocument(const Value: Integer);
var IsPlata   : Boolean;
    lNode     : TdxTreeListNode;
begin
  { Sa evitam ciclare }
  if FIdDocument = Value then Exit;

  FIdDocument := Value;

  if FIsInternalLoad then Exit;
  
  FIsInternalLoad := True;

  FSearchSuma := DisponibilDocum;
  SetCurentStatus;

  FIsInternalLoad := False;
end;

procedure TfrmCompensari.BtnAddClick(Sender: TObject);
var
  lSuma : Currency;
begin
  edSumaPropertiesChange(edSuma);
  with GetTmpADOQuery do
    try
       lSuma := edSuma.Value;
       Sql.Add('exec spCompensareAdd :ID_DOCUM_FURNIZOR, :ID_DOCUM_CLIENT, :SUMA');
       Params[0].Value := FIdDocCasa;
       Params[1].Value := FIdDocument;
       Params[2].Value := lSuma;
       ExecSQL;
       RefreshIncasari;
    finally
       Free;
    end;
end;

procedure TfrmCompensari.RefreshIncasari;
begin
  DBRefresh([QryFurnizori, QryClienti, QryImperecheri]);
end;

function TfrmCompensari.DisponibilDocum: Currency;
begin
  if GridFurnizori.Count = 0 then  Result := 0
  else
    with QryClienti do Result := FieldByName('TOTALDOC').AsCurrency - FieldByName('ASIGNAT').AsCurrency;
end;

function TfrmCompensari.DisponibilIncasare: Currency;
begin
  if GridClienti.Count = 0 then Result := 0
  else
    with QryFurnizori do Result := FieldByName('TOTALDOC').AsCurrency - FieldByName('ASIGNAT').AsCurrency;
end;

procedure TfrmCompensari.BtnModifyClick(Sender: TObject);
var lNode: TdxDBGridNode;
  lSuma : Currency;
begin
  edSumaPropertiesChange(edSuma);
  lNode := TdxDBGridNode(GridImperecheri.FocusedNode);
  if Assigned(lNode) and (MessageDlg('Doriti inlocuirea compensarii curente ?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         lSuma := edSuma.Value;
         Sql.Add('exec spCompensareUpdate :SUMA, :Id_Doc_Furnizor, :Id_Doc_Client, '  + VarToStr(lNode.Id));
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

procedure TfrmCompensari.BtnDeleteClick(Sender: TObject);
var lNode: TdxDBGridNode;
begin
  lNode := TdxDBGridNode(GridImperecheri.FocusedNode);
  if (Assigned(lNode)) and
     (MessageDlg('Doriti stergerea conexiunii intre documentele tranzactionate?'#13#10+GetDecontareText(lNode), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    with GetTmpADOQuery do
      try
         ParamCheck := False;
         Sql.Add('exec spCompensareDel ' + VarToStr(lNode.Id));
         ExecSQL;
         RefreshIncasari;
      finally
         Free;
      end;
end;

procedure TfrmCompensari.BtnDefalcareClick(Sender: TObject);
var lCasaNode, lItemsiNode: TdxDBGridNode;
    lIdBreg, lDocum: Integer;
    lSuma : Currency;
begin
  // TODO Defalcarea
  lSuma := edSuma.Value;
  lCasaNode := TdxDBGridNode(GridFurnizori.FocusedNode);
  if Assigned(lCasaNode) then lIdBreg := lCasaNode.Id
  else lIdBreg := -1;
  lItemsiNode := TdxDBGridNode(GridClienti.FocusedNode);
  if Assigned(lItemsiNode) then lDocum := lItemsiNode.Id
  else lDocum := -1;
  if (lDocum > -1) and (lIdBreg > -1) then
     if EditDefalcare(lIdBreg, lDocum, lSuma) then RefreshIncasari;
end;

procedure TfrmCompensari.Panel3Resize(Sender: TObject);
begin
  GrCasa.Height := Panel3.Height div 2; 
end;

procedure TfrmCompensari.GridClientiFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  if GridClienti.FocusedNode = nil then
    GridClientiChangeNode(GridClienti, nil, GridClienti.TopNode)
  else
  GridClientiChangeNode(GridClienti, nil, GridClienti.FocusedNode);
end;

procedure TfrmCompensari.GridFurnizoriPROCENTCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var ATextColor : TColor;
begin
  GridFurnizoriCustomDrawPreviewCell(Sender, ACanvas, ARect,ANode, ASelected, AText,
                                    AColor, ATextColor, AFont, ADone);
end;

procedure TfrmCompensari.GridClientiPROCENTCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var ATextColor : TColor;
begin
  GridClientiCustomDrawPreviewCell(Sender, ACanvas, ARect,ANode, ASelected, AText,
                                    AColor, ATextColor, AFont, ADone);
end;

procedure TfrmCompensari.TabListChange(Sender: TObject);
begin
  if FTabListNo = TabList.TabIndex then Exit;
  FTabListNo := TabList.TabIndex;
  GridFurnizori.Filter.Clear;
  GridClienti.Filter.Clear;
  if TabList.TabIndex = 0 then begin
     { Avem Furnizor -> Client }
     FPivotGrid := GridFurnizori;
     FSearchGrid := GridClienti;
     GridClienti.Parent := GrDocumente;
     GridFurnizori.Parent  := GrCasa;
     GrCasa.Caption       := 'Furnizori';
     GrDocumente.Caption  := 'Clienti';
  end
  else begin
     { Avem Furnizor -> Casa }
     FPivotGrid := GridClienti;
     FSearchGrid := GridFurnizori;
     GridClienti.Parent := GrCasa;
     GridFurnizori.Parent  := GrDocumente;
     GrDocumente.Caption  := 'Furnizori';
     GrCasa.Caption       := 'Clienti';
  end;
  RefreshIncasari;
end;

procedure TfrmCompensari.GridImperecheriDblClick(Sender: TObject);
var lDecontNode: TdxDBGridNode;
    lDocum, lIdBreg    : Integer;
    lSuma : Currency;  
begin
  lSuma := edSuma.Value;
  lDecontNode := TdxDBGridNode(GridImperecheri.FocusedNode);
  if Assigned(lDecontNode) then begin
     lIdBreg := lDecontNode.Values[GridImperecheriFURNIZOR_ID_GEST_DOCUM.Index];
     lDocum  := lDecontNode.Values[GridImperecheriCLIENT_ID_GEST_DOCUM.Index];
     if (lDocum > -1) and (lIdBreg > -1) then
        if EditDefalcare(lIdBreg, lDocum, lSuma) then RefreshIncasari;
  end;
end;

procedure TfrmCompensari.GridImperecheriCustomDraw(Sender: TObject;
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

procedure TfrmCompensari.ppLocalizareClick(Sender: TObject);
var lFurnizorId, lClientId: Variant;
    lFurnizorNode, lClientNode: TdxDBGridNode;
    IsBanca : Boolean;
    Node    : TdxTreeListNode;
begin
  Node := GridImperecheri.FocusedNode;
  { Ne Pozitionam pe pozitia corecta din casa si pozitia corecta din itemsi }
  if not Assigned(Node) then Exit;
  Screen.Cursor := crHourGlass;
  try
    lFurnizorId := Node.Values[GridImperecheriFURNIZOR_ID_GEST_DOCUM.Index];
    lClientId := Node.Values[GridImperecheriCLIENT_ID_GEST_DOCUM.Index];
    { Daca nu suntem pe banca si avem banca => schimbam locatia }
    lFurnizorNode := GridFurnizori.FindNodeByKeyValue(lFurnizorId);
    if Assigned(lFurnizorNode) then begin
       lFurnizorNode.MakeVisible;
       lFurnizorNode.Focused := True;
    end;
    lClientNode := GridClienti.FindNodeByKeyValue(lClientId);
    if Assigned(lClientNode) then begin
       lClientNode.MakeVisible;
       lClientNode.Focused := True;
    end;
  finally
    Screen.Cursor := crDefault;
  end;
end;

procedure TfrmCompensari.BtnAutoDecontClick(Sender: TObject);
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

procedure TfrmCompensari.ppModificareDocumClick(Sender: TObject);
begin
  GridImperecheriDblClick(GridImperecheri);
end;

procedure TfrmCompensari.ppDecontariPopup(Sender: TObject);
var
    Node   : TdxTreeListNode;
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
  (*
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
    *)
end;

procedure TfrmCompensari.ReasignareDocum(Sender: TObject);
var lId: Integer;
begin
  {
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
  }
end;

procedure TfrmCompensari.SetIdDocCasa(const Value: Integer);
var lFilter   : String;
    IsPlata   : Boolean;
    lNode     : TdxTreeListNode;
begin
  { Sa nu permitem ciclare }
  if FIdDocCasa = Value then Exit;
  FIdDocCasa := Value;
  if FIsInternalLoad then Exit;
  FIsInternalLoad := True;
(*
  { Daca TabIndex in (0,1) => Casa este pivot }
  if TabList.TabIndex  = 0 then begin
     GridClienti.Filter.Clear;
     { Luam nodul corect }
     lNode := GridFurnizori.FocusedNode;
     if Assigned(lNode) then begin
       if TdxDBGridNode(lNode).Id <> Value then
          lNode := GridFurnizori.FindNodeByKeyValue(Value);
       if Assigned(lNode) then begin
         { Resetam repartitorul curent }
         IsPlata := lNode.Strings[GridFurnizoriTIP_PLATA.Index] = '1';
         if Trim(lNode.Strings[GridFurnizoriCODGEST.Index]) > '' then FIdRepartitor := lNode.Values[GridFurnizoriCODGEST.Index]
         else FIdRepartitor := -1;
         FNumeRepartitor := Trim(lNode.Strings[GridFurnizoriNUME.Index]);

         // 1 = Plata, 0 = Incasare
         // C.GESTINT = Predator Intern
         // D.GESTINT = Primitor Intern

         if IsPlata then lFilter := '1'
         else lFilter := '0';
         { Verificam daca trebuie sa reactualizam documentele }
         if FCurentFilter <> lFilter then begin
            QryClienti.Close;
            QryClienti.Params.ParamByName('IS_PLATA').Value := StrToInt(lFilter);
            //QryClienti.Sql[8] := lFilter;
            QryClienti.Open;
            FCurentFilter := lFilter;
         end;
         { Daca avem repartitor facem automat filtrarea }
         if FIdRepartitor > 0 then
            if IsPlata then GridClienti.Filter.Add(GridClientiPREDATOR, FNumeRepartitor, 'Predator Document : '+FNumeRepartitor)
            else GridClienti.Filter.Add(GridClientiPRIMITOR, FNumeRepartitor, 'Primitor Document : '+FNumeRepartitor);
            
       end;
     end;
  end; *)
  FSearchSuma := DisponibilIncasare;
  SetCurentStatus;
  FIsInternalLoad := False;
end;

procedure TfrmCompensari.SetCurentStatus;
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
       Sql.Add(' exec spCompensareByIds  ' +IntToStr(FIdDocCasa) + ','+ IntToStr(FIdDocument));
       Open;
       //folosim pivotul ca referinta
       if not IsEmpty then begin
          lNode := GridImperecheri.FindNodeByKeyValue(Fields[0].AsInteger);
          if Assigned(lNode) then begin
             lNode.MakeVisible;
             lNode.Focused := True;
          end;
       end;
    finally
       Free;
    end;
end;

function TfrmCompensari.GetDecontareText(ANode: TdxTreeListNode): String;
begin
  if Assigned(ANode) then
    with ANode do
      Result := Strings[GridImperecheriFURNIZOR_COD_DOCUM.Index]+' '+Strings[GridImperecheriFURNIZOR_NR_DOCUM.Index]+' '+Strings[GridImperecheriFURNIZOR_DATA_DOCUM.Index] + ' ' +
                Strings[GridImperecheriFURNIZOR_PREDATOR.Index]
                +' -> '+
                Strings[GridImperecheriCLIENT_COD_DOCUM.Index]+' '+' '+Strings[GridImperecheriCLIENT_NR_DOCUM.Index]+' '+Strings[GridImperecheriCLIENT_DATA_DOCUM.Index]+
                Strings[GridImperecheriCLIENT_PRIMITOR.Index]
  else Result := 'Neasignat';
end;

procedure TfrmCompensari.GridFurnizoriPROCENTGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var
  lCurent,
  lTotal : Currency;
begin
  { Afisam cat la suta este deja asignat }
  if ANode.Strings[GridFurnizoriTOTALDOC.Index] > '' then
     lTotal := ANode.Values[GridFurnizoriTOTALDOC.Index]
  else lTotal := 0;

  if ANode.Strings[GridFurnizoriASIGNAT.Index] > '' then
     lCurent := ANode.Values[GridFurnizoriASIGNAT.Index]
  else lCurent := 0;
  if lTotal > 0 then AText := CurrToStr(lCurent/lTotal * 100)
  else AText := '0';
end;

procedure TfrmCompensari.ppReconciliereClick(Sender: TObject);
begin
  { Luam toate pozitiile invalide si propunem solutii pentru fiecare in parte }
  ppReconciliere.Checked := not ppReconciliere.Checked;
  Reconcilieri.Visible := ppReconciliere.Checked;
end;

function TfrmCompensari.Reconcilieri: TCustomForm;
begin
  {
  if FReconciliere = nil then begin
     FReconciliere := TFrmReconcilereDecontari.Create(Self);
     TFrmReconcilereDecontari(FReconciliere).Decontari := Self;
  end;
  Result := FReconciliere;
  }
end;

procedure TfrmCompensari.edSumaPropertiesChange(Sender: TObject);
var
  lValMax: Currency;
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

procedure TfrmCompensari.FormShow(Sender: TObject);
begin
   TabListChange(TabList);
   if GridFurnizori.TopNode <> nil then
     GridFurnizoriChangeNode(GridFurnizori, nil, GridFurnizori.TopNode);
end;

type
 TCrackATSGrid = class(TdxDBGrid);

procedure TfrmCompensari.TryToSearchSuma;
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
     if FSearchGrid = GridFurnizori then aCol := TdxDBGridColumn(GridFurnizoriTOTALDOC)
                                    else aCol := TdxDBGridColumn(GridClientiTOTALDOC);
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

procedure TfrmCompensari.GridFurnizoriDblClick(Sender: TObject);
begin
  if FPivotGrid = GridFurnizori then TryToSearchSuma;
end;

procedure TfrmCompensari.GridClientiDblClick(Sender: TObject);
begin
  if FPivotGrid = GridClienti then TryToSearchSuma;
end;

procedure TfrmCompensari.btnCautaAutomatClick(Sender: TObject);
begin
  TryToSearchSuma;
end;

procedure TfrmCompensari.edSumaPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if (Error) and (edSuma.Value > edSuma.Properties.MaxValue) then begin
    DisplayValue := edSuma.Properties.MaxValue;
    Error := False;
  end;
end;

procedure TfrmCompensari.GridFurnizoriFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  if GridFurnizori.FocusedNode = nil then
    GridFurnizoriChangeNode(GridFurnizori, nil, GridFurnizori.TopNode)
  else
    GridFurnizoriChangeNode(GridFurnizori, nil, GridFurnizori.FocusedNode);
end;

procedure TfrmCompensari.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmCompensari.btnRefreshClick(Sender: TObject);
begin
  RefreshIncasari;
end;

end.

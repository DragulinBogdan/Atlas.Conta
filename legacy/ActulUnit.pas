unit ActulUnit;

interface

uses Dialogs, Windows, ImgList, Controls, dxmdaset, DB, ZDataSet, StdCtrls, ExtCtrls,
  Graphics, Classes, Forms, OleCtrls, IdBaseComponent,
  IdComponent, IdTCPServer, IdCustomHTTPServer, IdHTTPServer, IdContext,
  Messages, Mask, Buttons, SHDocVw, dxGrClms, dxTL, dxDBCtrl, dxCntner,
  DegradePanel, dxDBTL, dxfProgressBar, dxDBGrid, dxDBTLCl,
   frxClass, frxPreview, dxExEdtr, Menus, cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels, IdCustomTCPServer;

type
  TCrackPreview = class(TfrxPreview);

  TfrmActual = class(TForm)
    DTCereri: TDataSource;
    StareDocument: TImageList;
    Cereri: TdxMemData;
    DTFunctii: TDataSource;
    Functii: TdxMemData;
    RefreshTimer: TTimer;
    pnClient: TPanel;
    Splitter1: TSplitter;
    pnDocSide: TPanel;
    GridInbox: TdxDBGrid;
    pnDocument: TPanel;
    Splitter2: TSplitter;
    GridInboxID_GEST_DOCUM: TdxDBGridMaskColumn;
    GridInboxNR_DOCUM: TdxDBGridMaskColumn;
    GridInboxDATA_DOCUM: TdxDBGridDateColumn;
    GridInboxDOCUMENT: TdxDBGridMaskColumn;
    GridInboxPREDATOR: TdxDBGridMaskColumn;
    GridInboxPRIMITOR: TdxDBGridMaskColumn;
    GridInboxID_FUNCTIUNI: TdxDBGridImageColumn;
    DocProvider: TIdHTTPServer;
    GridInboxDATA_EXPIRARE: TdxDBGridDateColumn;
    GridInboxZILE_RAMASE: TdxDBGridMaskColumn;
    GridInboxPRIORITATE: TdxDBGridImageColumn;
    pnLeft: TPanel;
    TreeFunctiuni: TdxDBTreeList;
    TreeFunctiuniDENUMIRE: TdxDBTreeListMaskColumn;
    TreeFunctiuniNR_UTILIZATORI: TdxDBTreeListMaskColumn;
    TreeFunctiuniNR_DOCUMENTE: TdxDBTreeListMaskColumn;
    Splitter3: TSplitter;
    pnValidari: TPanel;
    Label1: TLabel;
    ProgressValidare: TdxfProgressBar;
    GridValidari: TdxDBGrid;
    DTValidari: TDataSource;
    QryValidari: TZQuery;
    GridValidariID_FUNCTIUNE: TdxDBGridImageColumn;
    GridValidariDATA_VALIDARE: TdxDBGridDateColumn;
    GridValidariVALIDAT: TdxDBGridImageColumn;
    GridInboxARE_DOC_FIZIC: TdxDBGridColumn;
    pnlStatusBar: TPanel;
    Progress: TdxfProgressBar;
    LbInfo: TLabel;
    pnTop: TDegradePanel;
    BtnOk: TcxButton;
    BtnValidare: TcxButton;
    procedure FormCreate(Sender: TObject);
    procedure TreeFunctiuniCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure TreeFunctiuniGetImageIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeFunctiuniGetStateIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeFunctiuniGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeFunctiuniDENUMIREGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TreeFunctiuniChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure CereriFilterRecord(DataSet: TDataSet; var Accept: Boolean);
    procedure RefreshTimerTimer(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure DocProviderCommandGet(AThread: TIdContext;
      ARequestInfo: TIdHTTPRequestInfo;
      AResponseInfo: TIdHTTPResponseInfo);
    procedure GridInboxChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridInboxDblClick(Sender: TObject);
    procedure GridInboxCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
      ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
      var AColor: TColor; AFont: TFont; var AAlignment: TAlignment;
      var ADone: Boolean);
    procedure QryValidariAfterOpen(DataSet: TDataSet);
    procedure BtnValidareClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure spbPreviewPrintClick(Sender: TObject);
    procedure spbPreviewFirstClick(Sender: TObject);
    procedure spbPreviewPriorClick(Sender: TObject);
    procedure spbPreviewNextClick(Sender: TObject);
    procedure spbPreviewLastClick(Sender: TObject);
    procedure ppViewerStatusChange(Sender: TObject);
    procedure GridInboxFilterChanged(Sender: TObject; ADataSet: TDataSet;
      const AFilterText: String);
  private
    { Private declarations }
    CurentFuncId: Integer;
    FAutoRefreh: Integer;
    FValidate: Boolean;
    FIsWebBased: Boolean;
    FViewer    : TComponent;
    FPreviewForm : TfrxPreviewForm;
    //FReader    : TStreamArchiveReader;

    procedure LocalDeleteRecord(const AId: Integer);
    procedure GetFunctionTree(AList: TStringList);
    procedure SetAutoRefresh(const Value: Integer);
    function IsExistNrDocs(lNode: TdxTreeListNode;DocIndex: Integer): boolean;
    function GetAllDocNumbers(lNode: TdxTreeListNode;DocIndex: Integer): Integer;
    procedure SetValidate(const Value: Boolean);

    function DescDocum(lNode: TdxDBGridNode): String;

    //function  ppView: TppViewer;
    function  frxView: TfrxReport;

  public
    procedure LoadWebDocument(AWebPath: String);
    procedure LoadArhiveDocument(AId: String);
    procedure LoadAndDisplayFRReport(AId: String);
    procedure OpenDefault;
    procedure InitViewPanel;
    procedure LoadCurentDocum(Node: TdxTreeListNode);
    procedure RefreshQry;
    property  AutoRefresh: Integer read FAutoRefreh write SetAutoRefresh;
    property  Validate : Boolean read FValidate write SetValidate;
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, frxZLib, Variants, SysUtils, ATSZDBUtils, CommonDBVar, DateUnit;

procedure TfrmActual.OpenDefault;
var
  lDataSet: TDataSet;
begin
  Cereri.DisableControls;
  Functii.DisableControls;
  try
    Cereri.Active  := False;
    Functii.Active := False;
    Cereri.ReadOnly  := False;
    Functii.ReadOnly := False;
    if FValidate then
      lDataSet := DBNewQueryFmt('exec [SP_GEST_VALIDARI_DOCUM] %d', [IdUtilizator])
    else
      lDataSet := DBNewQueryFmt('exec [SP_GEST_NEED_VALIDARI_DOCUM] %d', [IdUtilizator]);
    try
      lDataSet.Open;
      Cereri.LoadFromDataSet(lDataSet);
      DBSetSQLQueryFmt(lDataSet, 'exec [SP_GET_TREE_FUNCTII] %d, %d', [IdUtilizator, Integer(FValidate)]);
      lDataSet.Open;
      Functii.LoadFromDataSet(lDataSet);
    finally
      lDataSet.Free;
    end;
    Cereri.ReadOnly  := True;
    Functii.ReadOnly := True;
  finally
    Cereri.EnableControls;
    Functii.EnableControls;
  end;
  if (Cereri.RecordCount > 0) and (Assigned(TreeFunctiuni.TopNode)) then begin
     TreeFunctiuni.TopNode.Focused := True;
     TreeFunctiuni.TopNode.MakeVisible;
     TreeFunctiuniChangeNode(TreeFunctiuni, nil, TreeFunctiuni.TopNode);
  end;
end;

procedure TfrmActual.RefreshQry;
begin
  RefreshTimer.Enabled := False;
  Screen.Cursor := crHourGlass;
  OpenDefault;
  Screen.Cursor := crDefault;
  RefreshTimer.Enabled := True;
end;

procedure TfrmActual.FormCreate(Sender: TObject);
begin
  FIsWebBased := False;
  InitViewPanel;
  AutoRefresh := 1000 * 60;
  PopulateImage(FrmData.QryFunctiuni,
                GridInboxID_FUNCTIUNI.Values,
                GridInboxID_FUNCTIUNI.Descriptions,
                'ID_FUNCTIUNI',
                'DENUMIRE');
  GridValidariID_FUNCTIUNE.Values.Assign(GridInboxID_FUNCTIUNI.Values);
  GridValidariID_FUNCTIUNE.Descriptions.Assign(GridInboxID_FUNCTIUNI.Descriptions);
end;

procedure TfrmActual.TreeFunctiuniCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);

begin
  if IsExistNrDocs(ANode, TreeFunctiuniNR_DOCUMENTE.Index) then
    AFont.Style := AFont.Style + [fsBold];
end;

procedure TfrmActual.TreeFunctiuniGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
var NrUtilizatori: Integer;
begin
  NrUtilizatori := Node.Values[TreeFunctiuniNR_UTILIZATORI.Index];
  if NrUtilizatori > 1 then Index := 2
  else Index := NrUtilizatori;
end;

procedure TfrmActual.TreeFunctiuniGetStateIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
var NrDocs: Integer;
begin
  NrDocs := Node.Values[TreeFunctiuniNR_DOCUMENTE.Index];
  if NrDocs > 0 then Index := 3
  else if IsExistNrDocs(Node, TreeFunctiuniNR_DOCUMENTE.Index) then Index := 6
  else Index := -1;
end;

procedure TfrmActual.TreeFunctiuniGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TfrmActual.TreeFunctiuniDENUMIREGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := AText + ' ('+Trim(ANode.Strings[TreeFunctiuniNR_DOCUMENTE.Index])+'/'+IntToStr(GetAllDocNumbers(ANode,TreeFunctiuniNR_DOCUMENTE.Index))+')';
end;

procedure TfrmActual.TreeFunctiuniChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  if Assigned(Node) then
     CurentFuncId := TdxDBTreeListNode(Node).Id
  else CurentFuncId := -1;
  Cereri.UpdateFilters;
  GridInboxChangeNode(GridInbox, nil, GridInbox.TopNode);
end;

procedure TfrmActual.CereriFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
var lFuncList: TStringList;
begin
  lFuncList := TStringList.Create;
  try
     lFuncList.Sorted :=True;
     lFuncList.Duplicates := dupIgnore;
     GetFunctionTree(lFuncList);
     Accept := lFuncList.IndexOf(DataSet.FieldByName('ID_FUNCTIUNI').AsString) > -1;
  finally
     lFuncList.Free;
  end;
end;

procedure TfrmActual.SetAutoRefresh(const Value: Integer);
begin
  FAutoRefreh := Value;
  RefreshTimer.Enabled  := False;
  RefreshTimer.Interval := Value * 1000;
  RefreshTimer.Enabled  := True;
end;

procedure TfrmActual.RefreshTimerTimer(Sender: TObject);
begin
  RefreshQry;
end;

function TfrmActual.IsExistNrDocs(lNode: TdxTreeListNode;
  DocIndex: Integer): boolean;
var J: Integer;
begin
  Result := lNode.Strings[DocIndex] > '0';
  if not Result then
    for J := 0 to lNode.Count-1 do begin
      Result := IsExistNrDocs(lNode.Items[J], DocIndex);
      if Result then break;
    end;
end;

function TfrmActual.GetAllDocNumbers(lNode: TdxTreeListNode;
  DocIndex: Integer): Integer;
var J: Integer;
begin
  Result := lNode.Values[DocIndex];
  for J := 0 to lNode.Count-1 do begin
    Result := Result + GetAllDocNumbers(lNode.Items[J], DocIndex);
  end;
end;

procedure TfrmActual.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmActual.GetFunctionTree(AList: TStringList);
var lNode: TdxDBTreeListNode;
  procedure AddInnerFunction(ANode: TdxDBTreeListNode);
  var I: Integer;
    begin
      AList.Add(IntToStr(Integer(ANode.Id)));
      for I := 0 to ANode.Count-1 do
        AddInnerFunction(TdxDBTreeListNode(ANode.Items[I]));
    end;
    
begin
  lNode := TreeFunctiuni.FindNodeByKeyValue(CurentFuncId);
  if Assigned(lNode) then AddInnerFunction(lNode);
end;

procedure TfrmActual.DocProviderCommandGet(AThread: TIdContext;
  ARequestInfo: TIdHTTPRequestInfo; AResponseInfo: TIdHTTPResponseInfo);
  
const
  sauthenticationrealm = 'ATLAS Internal Viewer';

  procedure AuthFailed;
  begin
    AResponseInfo.ContentText := '<html><head><title>Error</title></head><body><h1>Autentificare esuata</h1>'#13 +
      'Nu puteti apela direct view-urul<br><ul><li>Folositi programele ATLAS pentru vizualizarea<b>documentelor</b>generate<b>de program</b>!</ul></body></html>';
    AResponseInfo.AuthRealm := sauthenticationrealm;
  end;

  procedure AccessDenied;
  begin
    AResponseInfo.ContentText := '<html><head><title>Eroare</title></head><body><h1>Accesul este restrictionat</h1>'#13 +
      'Nu aveti suficiente drepturi pentru a accesa informatiile.</body></html>';
    AResponseInfo.ResponseNo := 403;
  end;

var
  LocalDoc : String;
  lDocNumber: String;
  lPDFFile : TMemoryStream;
  lDataSet : TDataSet;
begin
  if (ARequestInfo.AuthUsername <> 'ats') or
     (ARequestInfo.AuthPassword <> 'atlas') then begin
    AuthFailed;
    Exit;
  end;
  { Citim numele "fisierului" ... de fapt este id-ul documentului }
  { Citim Documentul din Tabela }
  LocalDoc := ARequestInfo.Document;
  Delete(LocalDoc, 1, 1);
  lDocNumber := ChangeFileExt(LocalDoc, '');
  lPDFFile := TMemoryStream.Create;
  try
    lDataSet := DBNewQuery('exec spGestGetFormaFizica '+lDocNumber);
    try
      lDataSet.Open;
      TBlobField(lDataSet.Fields[0]).SaveToStream(lPDFFile);
    finally
      lDataSet.Free;
    end;
    AResponseInfo.ContentType := DocProvider.MIMETable.GetFileMIMEType(LocalDoc);

    AResponseInfo.ContentLength := lPDFFile.Size;
    AResponseInfo.WriteHeader;
    lPDFFile.Position := 0;
    aThread.Connection.IOHandler.Write(lPDFFile);
  finally
    lPDFFile.Free;
  end;
end;

procedure TfrmActual.GridInboxChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  { Resetam imaginea afisata }
  QryValidari.Close;
  if (Assigned(Node)) and (not Node.HasChildren) then QryValidari.Sql[1] := 'WHERE ID_GEST_DOCUM = '+Node.Strings[GridInboxID_GEST_DOCUM.Index]
  else QryValidari.Sql[1] := 'WHERE ID_GEST_DOCUM = -1';
  QryValidari.Open;
  if Node = nil then Exit;
  if not FIsWebBased and (Trim(Node.Strings[GridInboxARE_DOC_FIZIC.Index]) <> '') then LoadCurentDocum(Node);
end;

procedure TfrmActual.GridInboxDblClick(Sender: TObject);
begin
  //if FIsWebBased then
  GridInboxChangeNode(GridInBox, nil, GridInBox.FocusedNode);
//  LoadCurentDocum(GridInBox.FocusedNode);
end;

procedure TfrmActual.GridInboxCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);

  function GetRealIndex: Integer;
   var I: Integer;
   begin
     Result := -1;
     for I := 0 to GridInbox.VisibleColumnCount-1 do
       if GridInbox.VisibleColumns[I] = AColumn then begin
          Result := I;
          Break;
       end;
   end;

var lZileRamase: Integer;
begin
  if ANode.HasChildren then Exit;
  if AFocused then
     if GetRealIndex = GridInbox.FocusedColumn then begin
        AColor := clAqua;
        AFont.Color := clBlack;
     end
     else Exit;
  if ANode.Strings[GridInboxARE_DOC_FIZIC.Index] = '0' then AFont.Style := AFont.Style + [fsStrikeOut];
  if (Trim(ANode.Strings[GridInboxZILE_RAMASE.Index]) = '') or
     (VarIsEmpty(ANode.Values[GridInboxZILE_RAMASE.Index])) or
     (VarIsEmpty(ANode.Values[GridInboxZILE_RAMASE.Index])) then begin
     AFont.Color := clNavy;
  end
  else begin
     lZileRamase := ANode.Values[GridInboxZILE_RAMASE.Index];
     if lZileRamase < 0 then begin
        AColor := clRed;
        AFont.Color := clBlack;
     end
     else if lZileRamase in [1..5] then begin
        AColor := $005E5EFF;
        AFont.Color := clBlack;
     end
     else if lZileRamase in [5..10] then begin
        AColor := $00C1C1FF;
        AFont.Color := clBlack;
     end
     else begin
        AColor := clWindow;
        AFont.Color := clBlack;
     end;
  end;
end;

procedure TfrmActual.QryValidariAfterOpen(DataSet: TDataSet);
var
   lMin,
   lMax,
   lPos,
   lPercent: Integer;
begin
  { Marcam starea curenta pentru validari }
  lMin := 0; lMax := 0; lPos := 0;
  with DataSet do begin
    First;
    while not Eof do begin
      Inc(lMax);
      if FieldByName('VALIDAT').AsInteger = 1 then Inc(lPos);
      Next;
    end;
  end;
  if lMax > 0 then
     lPercent := Trunc(lPos / lMax * 100)
  else lPercent := 0;
  if lPercent > 40 then begin
     ProgressValidare.Font.Color := clYellow;
     ProgressValidare.BeginColor := clNavy;
  end
  else begin
     ProgressValidare.Font.Color := clBlack;
     ProgressValidare.BeginColor := clRed;
  end;
  ProgressValidare.Min := lMin;
  ProgressValidare.Max := lMax;
  ProgressValidare.Position := lPos;
end;

procedure TfrmActual.BtnValidareClick(Sender: TObject);
var lNode: TdxDBGridNode;
    lId  : Integer;
    lIdDocum,
    lIdFunctiune : Integer;
begin
  lNode := TdxDBGridNode(GridInbox.FocusedNode);
  if (Assigned(lNode)) and (not lNode.HasChildren) then begin
    lIdDocum := lNode.Values[GridInboxID_GEST_DOCUM.Index];
    lId      := lNode.Id;
    lIdFunctiune := lNode.Values[GridInboxID_FUNCTIUNI.Index];
    if ConfirmaParola then begin
      DBExecSQLFmt('UPDATE GEST_VALIDARI_DOCUM SET ID_LOGARE_USERS = %d, DATA_VALIDARE = GETDATE() WHERE ID_GEST_DOCUM = %d AND ID_FUNCTIUNE = %d',
                    [
                      idLogin,
                      lIdDocum,
                      lIdFunctiune]);
      DBExecSQLFmt('UPDATE GEST_DOCUM SET VALIDAT = 1 FROM GEST_DOCUM AS A WHERE ID_GEST_DOCUM = %d '#13#10+
                   'AND NOT EXISTS (SELECT TOP 1 1 FROM GEST_VALIDARI_DOCUM WHERE ID_GEST_DOCUM = A.ID_GEST_DOCUM AND ID_LOGARE_USERS IS NULL)', [lIdDocum]);
      LocalDeleteRecord(lId);
    end;
  end;
end;

procedure TfrmActual.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmActual.LoadCurentDocum(Node: TdxTreeListNode);
begin
  if FIsWebBased then LoadWebDocument('about:blank')
  else LoadArhiveDocument('');
  if (Assigned(Node)) and (not (Node.HasChildren)) then
     if FIsWebBased then LoadWebDocument('http://ats:atlas@127.0.0.1:21504/'+Node.Strings[GridInboxID_GEST_DOCUM.Index]+'.pdf')
     else LoadArhiveDocument(Node.Strings[GridInboxID_GEST_DOCUM.Index]);
end;

procedure TfrmActual.SetValidate(const Value: Boolean);
begin
  FValidate := Value;
  if FValidate then begin
     BtnValidare.Enabled := False;
     BtnValidare.Visible := False;
     Caption := 'Documente validate';
     pnTop.Caption := 'Lista documente validate';
  end
  else begin
     Caption := 'Documente care trebuie validate';
     pnTop.Caption := 'Lista documente care trebuie validate';
  end;
  OpenDefault;
end;

procedure TfrmActual.InitViewPanel;
begin
  if FIsWebBased then
    try
       DocProvider.Active := True;
    except
      { Daca nu putem web pornim pe arhive reader }
      FIsWebBased := False;
    end;

  if FIsWebBased then begin
    FViewer := TWebBrowser.Create(Self);
  end
  else begin
    FViewer := TfrxReport.Create(Self);
    with TfrxReport(FViewer) do begin
      FileName             := 'RaportNou';
      ReportOptions.Name   := FileName;
      ReportOptions.Author := 'Advanced Technology Systems';
      ReportOptions.CreateDate   := Now;
      EngineOptions.SilentMode   := False;
      EngineOptions.UseFileCache := False;
      OldStyleProgress := True;
      ShowProgress := False;
    end;
    FPreviewForm := TfrxPreviewForm.Create(Self);
    FPreviewForm.PopupMenu := nil;
    FPreviewForm.Parent := pnDocument;
    FPreviewForm.Align := alClient;
    FPreviewForm.BorderStyle := bsNone;
    FPreviewForm.Visible := True;
    TfrxReport(FViewer).Preview := FPreviewForm.Preview;
//  TfrxReport(FViewer).PreviewOptions.Modal := False;
//  TfrxReport(FViewer).PreviewFormParent := pnDocument;
  end;
end;

{ Ca sa ne asiguram ca se incarca serializat din Stream }
{ In acelasi timp trebuie sa avem grija sa incarcam ulitmul raport
  altfel nu o sa mai avem corelare intre ce este pe ecran si ce avem in preview la raport }
procedure TfrmActual.LoadArhiveDocument(AId: String);
const
   IsInPrint : Boolean = False;
   LastID: String = '';
begin
  if IsInPrint then begin
     LastID := AId;
     Exit;
  end;
  if AId = LastID then LastID := '';

  IsInPrint := True;
  try
    LoadAndDisplayFRReport(AId);
  finally
     IsInPrint := False;
     if LastID > '' then LoadArhiveDocument(LastID);
  end;

  LastID := '';
end;

procedure TfrmActual.LoadWebDocument(AWebPath: String);
var Flags: OleVariant;
    TargetFrameName: OleVariant;
    PostData: OleVariant;
    Headers: OleVariant;
begin
  if not TOLEControl(FViewer).Visible then TOLEControl(FViewer).Visible := True;
  TWebBrowser(FViewer).Navigate(AWebPath, Flags, TargetFrameName, PostData, Headers);
end;


procedure TfrmActual.spbPreviewPrintClick(Sender: TObject);
begin
  TCrackPreview(frxView.Preview).Print;
end;

procedure TfrmActual.spbPreviewFirstClick(Sender: TObject);
begin
  TCrackPreview(frxView.Preview).First;
end;

procedure TfrmActual.spbPreviewPriorClick(Sender: TObject);
begin
  TCrackPreview(frxView.Preview).Prior;
end;

procedure TfrmActual.spbPreviewNextClick(Sender: TObject);
begin
  TCrackPreview(frxView.Preview).Next;
end;

procedure TfrmActual.spbPreviewLastClick(Sender: TObject);
begin
  TCrackPreview(frxView.Preview).Last;
end;

procedure TfrmActual.ppViewerStatusChange(Sender: TObject);
begin
  pnlStatusBar.Caption :=   frxView.ReportOptions.Name;
end;

function TfrmActual.DescDocum(lNode: TdxDBGridNode): String;
begin
  with lNode do
    Result := Strings[GridInboxDOCUMENT.Index]+' '+Strings[GridInboxNR_DOCUM.Index]+' '+Strings[GridInboxDATA_DOCUM.Index];
end;

procedure TfrmActual.LocalDeleteRecord(const AId: Integer);
begin
  GridInbox.BeginUpdate;
  try
     if Cereri.RecIdField.AsInteger <> AId then
        if not Cereri.Locate(Cereri.RecIdField.FieldName, AId, []) then Exit;
     Cereri.Delete;
  finally
    GridInbox.EndUpdate;
  end;
end;

procedure TfrmActual.GridInboxFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  GridInboxChangeNode(GridInBox, nil, GridInBox.FocusedNode);
end;

function TfrmActual.frxView: TfrxReport;
begin
  Result := TfrxReport(FViewer);
end;

procedure TfrmActual.LoadAndDisplayFRReport(AId: String);
var
  lDataSet   : TDataSet;
  lZipStream : TZDecompressionStream;
  lStream    : TStream;
  lMemStream : TMemoryStream;
begin
  if (AId > '') and (frxView <> nil) then begin
    try
      lDataSet := DBNewQuery('exec spGestGetFormaFizica '+AId);
      try
        lDataSet.Open;
        if not lDataSet.IsEmpty then begin
          lStream := lDataSet.CreateBlobStream(TBlobField(lDataSet.Fields[0]), bmRead);
          try
            lStream.Position := 0;
            lZipStream := TZDecompressionStream.Create(lStream);
            lMemStream := TMemoryStream.Create;
            try
               lMemStream.CopyFrom(lZipStream, 0);
               lMemStream.Position := 0;
               TCrackPreview(frxView.Preview).Workspace.Repaint;
               try
                  TCrackPreview(frxView.Preview).Lock;
                  frxView.PreviewPages.LoadFromStream(lMemStream);
               finally
                 TCrackPreview(frxView.Preview).PageNo := 1;
                 TCrackPreview(frxView.Preview).Unlock;
               end;
            finally
               lZipStream.Free;
               lMemStream.Free;
            end;
          finally
            lStream.Free;
          end;
        end;
      finally
        lDataSet.Free;
      end;
    except
    end;
  end;
end;

end.

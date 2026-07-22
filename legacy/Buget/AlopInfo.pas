unit AlopInfo;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxControls, cxPC, cxGraphics, cxDropDownEdit, cxImageComboBox,
  cxCalendar, cxButtonEdit, cxContainer, cxEdit, cxTextEdit, cxMaskEdit,
  StdCtrls, cxSpinEdit, cxDBEdit, Menus, cxLookAndFeelPainters, cxButtons,
  cxMemo, DB, ZDataSet, cxTL,
  cxInplaceContainer, cxDBTL, cxTLData,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxStyles, dxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TfrmALOPInfo = class(TForm)
    cxPageInfo: TcxPageControl;
    tabBuget: TcxTabSheet;
    tabAngajament: TcxTabSheet;
    tabOrdonantare: TcxTabSheet;
    LbPredator: TLabel;
    edPredator: TcxPopupEdit;
    lbDocument: TLabel;
    LbDataNota: TLabel;
    LbScopul: TLabel;
    LbPrimitor: TLabel;
    edPrimitor: TcxPopupEdit;
    lbNrOrdonantare: TLabel;
    edtNrOrdonantare: TcxDBButtonEdit;
    lbDataEmitere: TLabel;
    edtDataEmitere: TcxDBDateEdit;
    lbNrOrdine: TLabel;
    edtNrOrdine: TcxDBSpinEdit;
    Label1: TLabel;
    btnAngajamentOrd: TcxButton;
    Label2: TLabel;
    edtNrAngajament: TcxDBTextEdit;
    Label3: TLabel;
    edtDataAngajament: TcxDBDateEdit;
    Label4: TLabel;
    edtNaturaCheltuielii: TcxDBMemo;
    btnSaveAng: TcxButton;
    edNumarDoc: TcxDBButtonEdit;
    edDataDoc: TcxDBDateEdit;
    edTipAngajament: TcxDBImageComboBox;
    DTAng: TDataSource;
    qryAng: TZQuery;
    cxTreeRepartitori: TcxDBTreeList;
    cxTreeRepartitoriNUME: TcxDBTreeListColumn;
    cxTreeRepartitoriADRESA: TcxDBTreeListColumn;
    cxTreeRepartitoriCONT: TcxDBTreeListColumn;
    cxTreeRepartitoriCODFISC: TcxDBTreeListColumn;
    cxTreeRepartitoriGESTINT: TcxDBTreeListColumn;
    DTAngD: TDataSource;
    qryAngD: TZQuery;
    Label5: TLabel;
    edtScop: TcxDBTextEdit;
    procedure btnSaveAngClick(Sender: TObject);
    procedure DTAngDataChange(Sender: TObject; Field: TField);
    procedure FormCreate(Sender: TObject);
    procedure edPredatorPropertiesPopup(Sender: TObject);
    procedure edPredatorPropertiesInitPopup(Sender: TObject);
    procedure edPredatorPropertiesCloseUp(Sender: TObject);
    procedure qryAngAfterOpen(DataSet: TDataSet);
    procedure edPredatorKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxTreeRepartitoriDblClick(Sender: TObject);
    procedure cxTreeRepartitoriKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edNumarDocPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure qryAngDNewRecord(DataSet: TDataSet);
    procedure qryAngNewRecord(DataSet: TDataSet);
  private
    FIdAngajament: Integer;
    FHandleRefresh: Integer;
    FAngEnabled: Boolean;
    FCodFunctional: String;
    FCodEconomic: String;
    FSumaAngajament: Currency;
    FDataEmitere: TDateTime;
    FIdOrdonantare: Integer;
    FOrdEnabled: Boolean;
    procedure SetIdAngajament(const Value: Integer);
    procedure PopulateTipAngajament;
    procedure DisplayRepAng;
    procedure SendRefresh;
    procedure SetAngEnabled(const Value: Boolean);
    procedure SetSumaAngajament(const Value: Currency);
    procedure CheckAddAngD;
    procedure SetDataEmitere(const Value: TDateTime);
    procedure SetIdOrdonantare(const Value: Integer);
    procedure SetOrdEnabled(const Value: Boolean);
    { Private declarations }
  public
    { Public declarations }

    procedure StergeAngajament;
    procedure StergeOrdonantare;

    property CodFunctional : String read FCodFunctional write FCodFunctional;
    property CodEconomic : String read FCodEconomic write FCodEconomic;
    property DataEmitere : TDateTime read FDataEmitere write SetDataEmitere;


    property IdAngajament : Integer read FIdAngajament write SetIdAngajament;
    property SumaAngajament : Currency read FSumaAngajament write SetSumaAngajament;
    property AngEnabled : Boolean read FAngEnabled write SetAngEnabled;


    property IdOrdonantare : Integer read FIdOrdonantare write SetIdOrdonantare;
    property OrdEnabled : Boolean read FOrdEnabled write SetOrdEnabled;

    property HandleRefresh : Integer read FHandleRefresh write FHandleRefresh;
  end;


implementation

uses
  dxCompsUtile, ZeosDBUtile, dateUnit, CommonDBVar;

{$R *.dfm}

procedure TfrmALOPInfo.btnSaveAngClick(Sender: TObject);
begin
  DBSetFieldValue(qryAng, 'ID_UTILIZATORI', IdUtilizator);
  DBSetFieldValue(qryAng, 'DATA_OPERARE', Date);
  DBSetFieldValue(qryAng, 'COD_FUNCTIONAL', FCodFunctional);
  DBSetFieldValue(qryAng, 'VALIDAT', Integer(1));
  DBSetFieldValue(qryAng, 'STARE', Integer(1));
  DoCheckPostDataSet(qryAng);
  SendRefresh;
end;

procedure TfrmALOPInfo.SetIdAngajament(const Value: Integer);
begin
  FIdAngajament := Value;
  with qryAng do begin
    Close;
    Params.ParamByName('idAng').Value := FIdAngajament;
    Open;
  end;

  with qryAngD do begin
    Close;
    Params.ParamByName('idAng').Value := FIdAngajament;
    Open;
  end;
  
  FAngEnabled := (FIdAngajament > 0);
end;

procedure TfrmALOPInfo.DTAngDataChange(Sender: TObject; Field: TField);
begin
  btnSaveAng.Enabled := True;
end;

procedure TfrmALOPInfo.FormCreate(Sender: TObject);
begin
  PopulateTipAngajament;
  FHandleRefresh := -1;
end;

procedure TfrmALOPInfo.PopulateTipAngajament;
begin
  edTipAngajament.Properties.Items.Clear;
  if DBProcExists('spAlopTipAngagament') then
    FillImageCombo(edTipAngajament.Properties, 'exec spAlopTipAngagament', 'ID', 'DENUMIRE')
  else begin
    with edTipAngajament.Properties.Items.Add do begin
      Description := 'Angajament';
      Value := Integer(-1);
    end;
  end;
  edTipAngajament.ItemIndex := 0;
end;

procedure TfrmALOPInfo.edPredatorPropertiesPopup(Sender: TObject);
var
  lNode : TcxTreeListNode;
  lIdRep : Integer;
begin
  if not (Sender is TcxPopupEdit) then Exit;
  lIdRep := TcxPopupEdit(Sender).Tag;
  lNode := cxTreeRepartitori.FindNodeByKeyValue(lIdRep, nil);
  if lNode <> nil then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

procedure TfrmALOPInfo.edPredatorPropertiesInitPopup(Sender: TObject);
const
 TipGest: array[Boolean] of String = ('False', 'True');
var
  lEdit: TcxPopupEdit;
begin
  SetFilterOnDataSet(FrmData.QryRepartitori, 'GESTINT = '+TipGest[Sender = edPredator]);
  lEdit := TcxPopupEdit(Sender);
  if lEdit.Properties.PopupWidth < lEdit.Width then lEdit.Properties.PopupWidth := lEdit.Width;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmALOPInfo.edPredatorPropertiesCloseUp(Sender: TObject);
var
  lNode : TcxDBTreeListNode;
begin
  if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then begin
     lNode := TcxDBTreeListNode(cxTreeRepartitori.FocusedNode);
     if Assigned(lNode) then begin
        TcxPopupEdit(Sender).Tag  := lNode.KeyValue;
        if Sender = edPredator then DBSetFieldValue(qryAng, 'ID_DEPARTAMENT', lNode.KeyValue)
                               else begin
                                 DBSetFieldValue(qryAng, 'ID_LST_REPARTITORI', lNode.KeyValue);
                                 SendRefresh;
                               end;
        TcxPopupEdit(Sender).Text := lNode.Texts[cxTreeRepartitoriNUME.ItemIndex];
     end;
  end;
  SetFilterOnDataSet(FrmData.QryRepartitori, '');
end;

procedure TfrmALOPInfo.qryAngAfterOpen(DataSet: TDataSet);
begin
  edPredator.Tag := -1;
  edPrimitor.Tag := -1;
  if not DataSet.IsEmpty then begin
    if DataSet.FieldByName('ID_DEPARTAMENT').AsInteger <> 0 then edPredator.Tag := DataSet.FieldByName('ID_DEPARTAMENT').AsInteger;
    if DataSet.FieldByName('ID_LST_REPARTITORI').AsInteger <> 0 then  edPrimitor.Tag := DataSet.FieldByName('ID_LST_REPARTITORI').AsInteger;
  end;
  DisplayRepAng;
end;

procedure TfrmALOPInfo.DisplayRepAng;
var
  lNode : TcxTreeListNode;
begin
  if edPredator.Tag = -1 then edPredator.Text := ''
  else begin
    lNode := cxTreeRepartitori.FindNodeByKeyValue(edPredator.Tag, nil);
    if lNode <> nil then begin
      edPredator.Text := lNode.Texts[cxTreeRepartitoriNUME.ItemIndex];
    end;
  end;
  if edPrimitor.Tag = -1 then edPrimitor.Text := ''
  else begin
    lNode := cxTreeRepartitori.FindNodeByKeyValue(edPrimitor.Tag, nil);
    if lNode <> nil then begin
      edPrimitor.Text := lNode.Texts[cxTreeRepartitoriNUME.ItemIndex];
    end;
  end;

end;

procedure TfrmALOPInfo.edPredatorKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (Shift = []) and ((Key > 32) or (Key in [8,27, 13])) then begin
     with TcxPopupEdit(Sender) do DroppedDown := True;
     Key := 0;
  end;
end;

procedure TfrmALOPInfo.cxTreeRepartitoriDblClick(Sender: TObject);
begin
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk
end;

procedure TfrmALOPInfo.cxTreeRepartitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmALOPInfo.SendRefresh;
begin
  if FHandleRefresh = -1 then Exit;
  PostMessage(FHandleRefresh, WM_USER+1, 0, 0); 
end;

procedure TfrmALOPInfo.edNumarDocPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
begin
  edNumarDoc.EditValue := DBGetScallarFmt('exec [spAlopNumarAngajament] %d', [FIdAngajament]);
  DBSetFieldValue(qryAng, 'NUMAR', edNumarDoc.EditValue);
end;

procedure TfrmALOPInfo.SetAngEnabled(const Value: Boolean);
begin
  FAngEnabled := Value;
  tabAngajament.Enabled := FAngEnabled;
end;

procedure TfrmALOPInfo.SetSumaAngajament(const Value: Currency);
begin
  FSumaAngajament := Value;
  CheckAddAngD;
end;

procedure TfrmALOPInfo.CheckAddAngD;
var
  lId : Integer;
begin
  if qryAngD.IsEmpty then begin
    qryAngD.Append;
    DBSetFieldValue(qryAngD, 'ID_UTILIZATORI', IdUtilizator);
    DBSetFieldValue(qryAngD, 'COD_ECONOMIC', CodEconomic);
    DBSetFieldValue(qryAngD, 'ID_VALUTA', 1);
    DBSetFieldValue(qryAngD, 'ANGAJAT_VALUTA', FSumaAngajament);
    DBSetFieldValue(qryAngD, 'CURS_VALUTAR', 1);
    DBSetFieldValue(qryAngD, 'ANGAJAT', FSumaAngajament);
    DBSetFieldValue(qryAngD, 'VALIDAT', 1);
    lId := qryAngD.FieldByName('ID_ALOP_ANGAJAMENTE_DEFALCARE').AsInteger;
    DBExecSQLFmt('exec [spAlopRefaDefalcare] %d', [lId]);
    SendRefresh;
  end
  else begin
    DBSetFieldValue(qryAngD, 'ANGAJAT_VALUTA', FSumaAngajament);
    DBSetFieldValue(qryAngD, 'CURS_VALUTAR', 1);
    DBSetFieldValue(qryAngD, 'ANGAJAT', FSumaAngajament);
  end;
end;

procedure TfrmALOPInfo.qryAngDNewRecord(DataSet: TDataSet);
begin
  if qryAng.IsEmpty then begin
    qryAng.Append;
    qryAng.Post;
    FIdAngajament := qryAng.FieldByName('id_alop_angajamente').AsInteger;
    edNumarDocPropertiesButtonClick(nil, 0);
    DBSetFieldValue(qryAng, 'ID_UTILIZATORI', IdUtilizator);
    DBSetFieldValue(qryAng, 'DATA_OPERARE', Date);
    DBSetFieldValue(qryAng, 'DATA_EMITERE', FDataEmitere);
    DBSetFieldValue(qryAng, 'TIP_ANGAJAMENT', Integer(1));
    DBSetFieldValue(qryAng, 'VALIDAT', Integer(1));
    DBSetFieldValue(qryAng, 'STARE', Integer(1));
  end;
  DoCheckPostDataSet(qryAng);
  DataSet.FieldByName('id_alop_angajamente').AsInteger := qryAng.FieldByName('id_alop_angajamente').AsInteger;
end;

procedure TfrmALOPInfo.qryAngNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('COD_FUNCTIONAL').AsString := FCodFunctional;
end;

procedure TfrmALOPInfo.SetDataEmitere(const Value: TDateTime);
begin
  FDataEmitere := Value;
end;

procedure TfrmALOPInfo.StergeAngajament;
begin
  DBExecSQLFmt('exec [spAlopDeleteAngajamentDefalcare] %d', [FIdAngajament]);
end;

procedure TfrmALOPInfo.SetIdOrdonantare(const Value: Integer);
begin
  FIdOrdonantare := Value;
end;

procedure TfrmALOPInfo.StergeOrdonantare;
begin
  DBExecSQLFmt('exec [spAlopDeleteOrdonantareDefalcare] %d', [FIdOrdonantare]);
end;

procedure TfrmALOPInfo.SetOrdEnabled(const Value: Boolean);
begin
  FOrdEnabled := Value;
  tabOrdonantare.Enabled := FOrdEnabled;
end;

end.

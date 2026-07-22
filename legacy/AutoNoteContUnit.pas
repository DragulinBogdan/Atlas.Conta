unit AutoNoteContUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, dxCntner, dxEditor, dxExEdtr, dxEdLib, dxTL, dxDBCtrl, dxDBGrid,
  Db, dxmdaset, ZDataSet, dxDBTLCl, dxGrClms, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, dxCustomWizardControl,
  dxWizardControl, unitMemTableEx, cxContainer, cxEdit, cxCheckBox,
  cxProgressBar, cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage,
  cxNavigator, cxDBData, cxMaskEdit, cxImageComboBox, cxCalendar,
  cxCurrencyEdit, cxTextEdit, cxClasses, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxGridCustomView, cxGrid,
  cxDataControllerConditionalFormattingRulesManagerDialog;

type

  TNoteContabile = record
    Modul     : Integer;
    ModulDesc : String[50];
    Selectat  : Boolean;
    Stornate   : Integer;
    Generate   : Integer;
    CheckBox   : TcxCheckBox;
  end;

  TfrmAutoNoteCont = class(TForm)
    Wizard: TdxWizardControl;
    Inceput: TdxWizardControlPage;
    SelectieModul: TdxWizardControlPage;
    SelectiePerioada: TdxWizardControlPage;
    NoteStornate: TdxWizardControlPage;
    NoteGenerate: TdxWizardControlPage;
    Finalizare: TdxWizardControlPage;
    FinishInfo: TdxMemo;
    LbCaption: TLabel;
    LbTotal: TLabel;
    LocalProgress: TcxProgressBar;
    GlobalProgress: TcxProgressBar;
    LbCurentProgress: TLabel;
    Label10: TLabel;
    Label11: TLabel;
    Label12: TLabel;
    Label13: TLabel;
    edDataMinima: TdxDateEdit;
    edDataMaxima: TdxDateEdit;
    dtNoteStornate: TDataSource;
    dtNoteGenerate: TDataSource;
    Label15: TLabel;
    Label17: TLabel;
    Label14: TLabel;
    edAnul: TdxMRUEdit;
    edLuna: TdxPickEdit;
    NoteEronate: TdxWizardControlPage;
    dtNoteEronate: TDataSource;
    chkAll: TcxCheckBox;
    BifeScroll: TScrollBox;
    Label6: TLabel;
    viewNoteEronate: TcxGridDBTableView;
    nivelNoteEronate: TcxGridLevel;
    gridNoteEronate: TcxGrid;
    viewNoteEronateNR_DOCUM: TcxGridDBColumn;
    viewNoteEronateTIP_EROARE: TcxGridDBColumn;
    viewNoteEronateID_DOCUMENT: TcxGridDBColumn;
    viewNoteEronateDATA: TcxGridDBColumn;
    viewNoteEronateNUME_REPARTITOR: TcxGridDBColumn;
    viewNoteEronateDOCUMENT: TcxGridDBColumn;
    viewNoteEronatePOZITIE: TcxGridDBColumn;
    viewNoteEronateEXPLICATIE: TcxGridDBColumn;
    viewNoteEronateVALOARE: TcxGridDBColumn;
    viewNoteEronateCONT_CRED: TcxGridDBColumn;
    viewNoteEronateCONT_DEBT: TcxGridDBColumn;
    viewNoteEronateMODUL: TcxGridDBColumn;
    viewNoteEronateCOD_FUNCTIONAL: TcxGridDBColumn;
    viewNoteEronateCOD_ECONOMIC: TcxGridDBColumn;
    viewNoteGenerate: TcxGridDBTableView;
    nivelNoteGenerate: TcxGridLevel;
    gridNoteGenerate: TcxGrid;
    viewNoteGenerateDATA: TcxGridDBColumn;
    viewNoteGenerateNUME_REPARTITOR: TcxGridDBColumn;
    viewNoteGenerateCONT_DEBT: TcxGridDBColumn;
    viewNoteGenerateVALOARE: TcxGridDBColumn;
    viewNoteGenerateCONT_CRED: TcxGridDBColumn;
    viewNoteGenerateDOCUMENT: TcxGridDBColumn;
    viewNoteGeneratePOZITIE: TcxGridDBColumn;
    viewNoteGenerateMODUL: TcxGridDBColumn;
    viewNoteGenerateCOD_FUNCTIONAL: TcxGridDBColumn;
    viewNoteGenerateCOD_ECONOMIC: TcxGridDBColumn;
    viewNoteStornate: TcxGridDBTableView;
    nivelNoteStornate: TcxGridLevel;
    gridNoteStornate: TcxGrid;
    viewNoteStornateJURNAL: TcxGridDBColumn;
    viewNoteStornateNRDOC: TcxGridDBColumn;
    viewNoteStornateDATA: TcxGridDBColumn;
    viewNoteStornateCONT_DEBT: TcxGridDBColumn;
    viewNoteStornateVALOARE: TcxGridDBColumn;
    viewNoteStornateCONT_CRED: TcxGridDBColumn;
    viewNoteStornateEXPLICATIE: TcxGridDBColumn;
    viewNoteStornateNUME_DEBIT: TcxGridDBColumn;
    viewNoteStornateNUME_CREDIT: TcxGridDBColumn;
    procedure FormCreate(Sender: TObject);
    procedure WizardFinishButtonClick(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure edAnulChange(Sender: TObject);
    procedure WizardCancelButtonClick(Sender: TObject);
    procedure chkAllClick(Sender: TObject);
    procedure WizardButtonClick(Sender: TObject;
      AKind: TdxWizardControlButtonKind; var AHandled: Boolean);
  private
    TblNoteStornate: TAtsMemData;
    TblNoteGenerate: TAtsMemData;
    TblNoteEronate: TAtsMemData;
  protected
    procedure EnterFinalizare;
    procedure EnterSelectieModul;
    procedure ExitSelectieModul;
    procedure ExitSelectiePerioada;
  private
    { Private declarations }
    NoteContabile : array[0..99] of TNoteContabile;
    FIsClosing : Boolean;
    FIsWriting : Boolean;
    procedure GenerateBife;
  public
    { Public declarations }
    procedure SalveazaNoteContabile;
  end;

procedure ShowWizardNoteCont;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, CommonDBVar, DateUnit;

procedure ShowWizardNoteCont;
begin
  with TfrmAutoNoteCont.Create(Application) do
    try
       if ShowModal = mrOk then
          SalveazaNoteContabile;
    finally
       Free;
    end;
end;

procedure TfrmAutoNoteCont.SalveazaNoteContabile;
begin
  { Salvam notele contabile }
end;

procedure TfrmAutoNoteCont.FormCreate(Sender: TObject);
var
  Y, M, D: Word;
begin

  TblNoteStornate := TAtsMemData.Create(Self);
  TblNoteGenerate := TAtsMemData.Create(Self);
  TblNoteEronate  := TAtsMemData.Create(Self);
  dtNoteStornate.DataSet := TblNoteStornate;
  dtNoteGenerate.DataSet := TblNoteGenerate;
  dtNoteEronate.DataSet  := TblNoteEronate;

  { Initializam combbox-ul cu anii }
  DecodeDate(Date, Y, M, D);
  edAnul.Items.Add(IntToStr(Y-2));
  edAnul.Items.Add(IntToStr(Y-1));
  edAnul.Items.Add(IntToStr(Y));
  for D := Low(LongMonthNames) to High(LongMonthNames) do
    edLuna.Items.Add(LongMonthNames[D]);
  edAnul.Text := IntToStr(Y);
  edLuna.Text := edLuna.Items[M-1];
  edAnulChange(edAnul);

  GenerateBife;

end;

procedure TfrmAutoNoteCont.WizardFinishButtonClick(Sender: TObject);
var
  I : Integer;

  procedure ResetProgress;
  begin
    GlobalProgress.Properties.Min := 0;
    GlobalProgress.Properties.Max := 0;
    GlobalProgress.Position := 0;

    LocalProgress.Properties.Min := 0;
    LocalProgress.Properties.Max := 0;
    LocalProgress.Position := 0;
   end;

begin

  try
    if chkAll.Checked then
      DBExecSQL('exec [SP_GENERARE_NOTE_SERVER]')
    else
    for I := Low(NoteContabile) to High(NoteContabile) do
      if Assigned(NoteContabile[I].CheckBox) and NoteContabile[I].CheckBox.Checked then
        DBExecSQLFmt('exec [SP_GENERARE_NOTE_SERVER] %d', [NoteContabile[I].Modul]);
    ResetProgress;
    FIsClosing  := True;
    ModalResult := mrOk;
  except
    on E: Exception do begin
       FIsWriting := False;
       ResetProgress;
       raise EContaHandledError.Create('Eroare la importul notelor contabile !'#13#10'EROARE : '+E.Message);
    end;
  end;

end;

procedure TfrmAutoNoteCont.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  CanClose := (FIsClosing) or (MessageDlg('Doriti parasirea vrajitorului pentru importul notelor contabile !',
                                        mtConfirmation, [mbYes, mbNo], 0) = mrYes);
end;

procedure TfrmAutoNoteCont.edAnulChange(Sender: TObject);
var lYear,
    lMonth: Integer;
begin
  lYear  := StrToInt(edAnul.Text);
  if edLuna.ItemIndex = -1 then
    lMonth := 1
  else
    lMonth := edLuna.ItemIndex+1;
    
  edDataMinima.Date := EncodeDate(lYear, lMonth, 1);
  if lMonth < 12 then
     edDataMaxima.Date := EncodeDate(lYear, lMonth+1, 1)-1
  else edDataMaxima.Date := EncodeDate(lYear+1, 1, 1) - 1;
end;

procedure TfrmAutoNoteCont.EnterFinalizare;
var
  I : Integer;
begin
  { Completam informatiile de import }
  with FinishInfo do begin
    Lines.Clear;
    Lines.Add('Ati ales urmatoarele optiuni pentru importul automat');
    Lines.Add('');
    Lines.Add('Modulele selectate');
    for I := Low(NoteContabile) to High(NoteContabile) do
        if Assigned(NoteContabile[I].CheckBox) then
           if NoteContabile[I].CheckBox.Checked then begin
             Lines.Add('    ' + NoteContabile[I].ModulDesc);
             Lines.Add('        Note stornate automat : '+IntToStr(NoteContabile[I].Stornate));
             Lines.Add('        Note generate automat : '+IntToStr(NoteContabile[I].Generate));
           end;
  end;
end;

procedure TfrmAutoNoteCont.EnterSelectieModul;
var
  I: Integer;
begin
  { Validam modulele pentru care se face validare }
  for I := Low(NoteContabile) to High(NoteContabile) do
    if Assigned(NoteContabile[I].CheckBox) then
      NoteContabile[I].CheckBox.Checked := NoteContabile[I].Selectat;
end;

procedure TfrmAutoNoteCont.ExitSelectieModul;
var
  I: Integer;
begin
  { Validam modulele pentru care se face validare }
  for I := Low(NoteContabile) to High(NoteContabile) do
    if Assigned(NoteContabile[I].CheckBox) then
      NoteContabile[I].Selectat := NoteContabile[I].CheckBox.Checked;
end;

procedure TfrmAutoNoteCont.ExitSelectiePerioada;

  function Power2(AExp: Integer): Integer;
   begin
     case AExp of
       0: Result := 1;
       1: Result := 2;
       else Result := 2 shl (AExp - 1);
     end;
   end;

var
  I,
  lModul: Integer;
begin
  { Citim notele contabile }
  TblNoteStornate.Active := False;
  TblNoteGenerate.Active := False;
  TblNoteEronate.Active  := False;
  lModul := 0;
  for I := Low(NoteContabile) to High(NoteContabile) do
    if Assigned(NoteContabile[I].CheckBox) then
      if NoteContabile[I].Selectat then lModul := lModul or Power2(I);

  DBCopyDataSetFmt(TblNoteStornate, 'exec [SP_GET_NOTE_STORNARE] %d, %s, %s', [lModul, ValueToStr(edDataMinima.Date), ValueToStr(edDataMaxima.Date)]);
  DBCopyDataSetFmt(TblNoteGenerate, 'exec [SP_GET_NOTE_GENERATE] %d, %s, %s', [lModul, ValueToStr(edDataMinima.Date), ValueToStr(edDataMaxima.Date)]);
  DBCopyDataSetFmt(TblNoteEronate , 'exec [SP_GET_NOTE_INVALIDE] %d, %s, %s', [lModul, ValueToStr(edDataMinima.Date), ValueToStr(edDataMaxima.Date)]);
  viewNoteStornate.ApplyBestFit(nil);
  viewNoteGenerate.ApplyBestFit(nil);
  viewNoteEronate.ApplyBestFit(nil);

  for I := Low(NoteContabile) to High(NoteContabile) do
  if Assigned(NoteContabile[I].CheckBox) then begin
    lModul := TblNoteStornate.GetValueCount('MODUL', Power2(I));
    if lModul = -1 then lModul := 0;
    NoteContabile[I].Stornate := lModul;
    lModul := TblNoteGenerate.GetValueCount('MODUL', Power2(I));
    if lModul = -1 then lModul := 0;
    NoteContabile[I].Generate := lModul;
  end;

end;

procedure TfrmAutoNoteCont.WizardButtonClick(Sender: TObject;
  AKind: TdxWizardControlButtonKind; var AHandled: Boolean);
begin
  case AKind of
    wcbkBack: ;
    wcbkNext: ;
    wcbkCancel:
      Close;
    wcbkHelp: ;
    wcbkFinish: ;
  end;
end;

procedure TfrmAutoNoteCont.WizardCancelButtonClick(Sender: TObject);
begin
  if FIsWriting then
     FIsWriting := MessageDlg('Doriti abandonarea procesului de import de note ?', mtConfirmation,
                              [mbYes, mbNo], 0) <> mrYes;
end;

procedure TfrmAutoNoteCont.chkAllClick(Sender: TObject);
var I : Integer;
begin
  for I := Low(NoteContabile) to High(NoteContabile) do
     if Assigned(NoteContabile[I].CheckBox) then
        NoteContabile[I].CheckBox.Enabled  := not chkAll.Checked;
end;

procedure TfrmAutoNoteCont.GenerateBife;
var
  lDataSet  : TDataSet;
  I         : Integer;
  lChk      : TcxCheckBox;
  lLabel    : TLabel;
begin

  for I := BifeScroll.ComponentCount - 1 downto 0 do
    BifeScroll.Components[I].Free;

  I := 0;
  lDataSet := DBNewQuery('exec [spImportNoteListaModule]');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lChk          := TcxCheckBox.Create(BifeScroll);
      lChk.Anchors  := [akLeft,akTop];
      lChk.Parent   := BifeScroll;
      lChk.AutoSize := False;
      lChk.Caption  := lDataSet.FieldByName('descriere').AsString;
      lChk.Checked  := True;
      lChk.Tag      := lDataSet.FieldByName('modul').AsInteger;
      lChk.Left     := 10;
      lChk.Top      := 8 + I * 50;
      lChk.AutoSize := True;

      lLabel          := TLabel.Create(BifeScroll);
      lLabel.Parent   := BifeScroll;
      lLabel.AutoSize := False;
      lLabel.Width    := 345;
      lLabel.Left     := 40;
      lLabel.Height   := 25;
      lLabel.Top      := 30 + I * 50;
      lLabel.Caption  := lDataSet.FieldByName('comentariu').AsString;
      lLabel.Anchors  := [akLeft,akTop];
      //lLabel.Anchors := [akLeft,akTop,akRight,akBottom];
      lLabel.WordWrap := True;
      with NoteContabile[I] do begin
        Stornate  := 0;
        Generate  := 0;
        Modul     := 0;
        Selectat  := True;
        CheckBox  := lChk;
        ModulDesc := lChk.Caption;
      end;
      Inc(I);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;

end;

end.

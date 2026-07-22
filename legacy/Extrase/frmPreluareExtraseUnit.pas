unit frmPreluareExtraseUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics,
  Controls, Forms, Dialogs, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxContainer, cxEdit, Menus, StdCtrls, cxButtons,
  ExtCtrls, cxGroupBox, cxLabel, cxTextEdit, cxMaskEdit, cxButtonEdit, cxPC,
  cxMemo, cxRichEdit, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, DB, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  dxmdaset, cxGridBandedTableView, cxGridDBBandedTableView,
  cxGridCustomPopupMenu, cxGridPopupMenu, cxProgressBar, dxBarBuiltInMenu,
  cxNavigator, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxDateRanges, dxScrollbarAnnotations;

type

  TfrmPreluareExtrase = class(TForm)
    grHeader: TcxGroupBox;
    pnBottom: TPanel;
    BtnOk: TcxButton;
    edNumeFisier: TcxButtonEdit;
    lbNumeFisier: TcxLabel;
    btnPreia: TcxButton;
    extrasDialog: TOpenDialog;
    pagePreluare: TcxPageControl;
    tabContinut: TcxTabSheet;
    tabListaExtras: TcxTabSheet;
    nivelExtras: TcxGridLevel;
    gridExtras: TcxGrid;
    tblExtras: TdxMemData;
    dtExtras: TDataSource;
    tblExtrascontClient: TStringField;
    tblExtrasnumeContClient: TStringField;
    tblExtrascontCrsp: TStringField;
    tblExtrasnrDoc: TStringField;
    tblExtrasdataDoc: TDateField;
    tblExtrasdataPlata: TDateField;
    tblExtrascuiClient: TStringField;
    tblExtrascuiPlatitor: TStringField;
    tblExtrascuiBeneficiar: TStringField;
    tblExtrassumaDebit: TCurrencyField;
    tblExtrassumaCredit: TCurrencyField;
    tblExtrasibanClient: TStringField;
    tblExtrasibanPlatitor: TStringField;
    tblExtrasibanBeneficiar: TStringField;
    tblExtrasnumePlatitor: TStringField;
    tblExtrasnumeBeneficiar: TStringField;
    tblExtrasbicBancaDest: TStringField;
    tblExtrasbicBancaExt: TStringField;
    tblExtrasexplicatii: TStringField;
    tblExtrasData: TDateField;
    viewExtras: TcxGridDBBandedTableView;
    viewExtrasRecId: TcxGridDBBandedColumn;
    viewExtrascontClient: TcxGridDBBandedColumn;
    viewExtrasnumeContClient: TcxGridDBBandedColumn;
    viewExtrascontCrsp: TcxGridDBBandedColumn;
    viewExtrasnrDoc: TcxGridDBBandedColumn;
    viewExtrasdataDoc: TcxGridDBBandedColumn;
    viewExtrasdataPlata: TcxGridDBBandedColumn;
    viewExtrascuiClient: TcxGridDBBandedColumn;
    viewExtrascuiPlatitor: TcxGridDBBandedColumn;
    viewExtrascuiBeneficiar: TcxGridDBBandedColumn;
    viewExtrassumaDebit: TcxGridDBBandedColumn;
    viewExtrassumaCredit: TcxGridDBBandedColumn;
    viewExtrasibanClient: TcxGridDBBandedColumn;
    viewExtrasibanPlatitor: TcxGridDBBandedColumn;
    viewExtrasibanBeneficiar: TcxGridDBBandedColumn;
    viewExtrasnumePlatitor: TcxGridDBBandedColumn;
    viewExtrasnumeBeneficiar: TcxGridDBBandedColumn;
    viewExtrasbicBancaDest: TcxGridDBBandedColumn;
    viewExtrasbicBancaExt: TcxGridDBBandedColumn;
    viewExtrasexplicatii: TcxGridDBBandedColumn;
    viewExtrasData: TcxGridDBBandedColumn;
    popupGrid: TcxGridPopupMenu;
    lbProgresProcesare: TcxLabel;
    progresProcesare: TcxProgressBar;
    lbProgresTransfer: TcxLabel;
    progresTransfer: TcxProgressBar;
    prelInfo: TcxMemo;
    lbProgresAnaliza: TcxLabel;
    progresAnaliza: TcxProgressBar;
    procedure BtnOkClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure edNumeFisierPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure btnPreiaClick(Sender: TObject);
  private
    { Private declarations }
    procedure DoUpdateTotalProgress(Sender: TObject; const TotalProgress: Integer);
    procedure DoUpdateProgress(Sender: TObject; const Position: Integer);
  public
    { Public declarations }
  end;


implementation

uses
  StrUtils, importExtraseUnit, DateUnit, CommonDBVar;

{$R *.dfm}

procedure TfrmPreluareExtrase.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmPreluareExtrase.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmPreluareExtrase.edNumeFisierPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
begin
  extrasDialog.FileName := edNumeFisier.Text;
  if extrasDialog.Execute then
    edNumeFisier.Text := extrasDialog.FileName;
end;

procedure DumpExtras(AExtras: TExtras; AInfo: TStrings; AProgress: TcxProgressBar);

  procedure DumpLinie(const AHeader: String; ALinie: TLinieExtras; Indent: Integer = 1);
  begin
    if ALinie.ExtrasType = etUnknown then
      AInfo.Add(Format(DupeString(' ', Indent) + '[%s] = [Gol]', [AHeader]))
    else begin
      if AHeader > '' then begin
        AInfo.Add(Format(DupeString(' ', Indent) + '[%s]', [AHeader]));
        Inc(Indent);
      end;
      with ALinie do begin
        AInfo.Add(Format(DupeString(' ', Indent) + 'Cont Client       : %s', [ContClient]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Nume Cont Client  : %s', [NumeContClient]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Cont Corespondent : %s', [ContCoresp]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Numar Document    : %s', [NrDoc]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Data Document     : %s', [FormatDateTime('dd.mm.yyyy', DataDoc)]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Data Plata        : %s', [FormatDateTime('dd.mm.yyyy', DataPlata)]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Cui Client        : %s', [CuiClient]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Cui Platitor      : %s', [CuiPlatitor]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Cui Beneficiar    : %s', [CuiBeneficiar]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Suma Debit        : %f', [SumaDebit]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Suma Credit       : %f', [SumaCredit]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'IBAN Client       : %s', [IbanClient]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'IBAN Platitor     : %s', [IbanPlatitor]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'IBAN Beneficiar   : %s', [IbanBeneficiar]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Nume Platitor     : %s', [NumePlatitor]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Nume Beneficiar   : %s', [NumeBeneficiar]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Bic Banca Dest    : %s', [BicBancaDest]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Bic Banca Exp     : %s', [BicBancaExp]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Explicatii        : %s', [Explicatii]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'An                : %d', [An]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Tip               : %s', [szTipStr[ExtrasType]]));
        AInfo.Add(Format(DupeString(' ', Indent) + 'Data              : %s', [FormatDateTime('dd.mm.yyyy', DataZi)]));
      end;
    end;
  end;

var
  I, J: Integer;
begin
  AProgress.Properties.Max := AExtras.RulajCount;
  AProgress.Position := 0;
  AInfo.BeginUpdate;
  try
    AInfo.Add(Format('Conturi : %d', [AExtras.ContCount]));
    AInfo.Add(Format('Rulaje  : %d', [AExtras.RulajCount]));
    { Parcurgem conturile }
    AInfo.Add('Conturi');
    AInfo.Add('-------');
    for I := 0 to AExtras.ContCount-1 do
      with AExtras.ContExtras[I] do begin
        AInfo.Add('');
        AInfo.Add('-----------------------------------');
        AInfo.Add('Simbol cont :  ' + SoldPrecedent.ContClient);
        AInfo.Add('-----------------------------------');
        DumpLinie('Sold Precedent', SoldPrecedent);
        DumpLinie('Rulaj Zi'      , RulajZi);
        DumpLinie('Total Sume'    , TotalSume);
        DumpLinie('Total Desc'    , TotalDesc);
        DumpLinie('Disponibil'    , Disponibil);
        DumpLinie('Sold Final'    , SoldFinal);
        AInfo.Add(Format(' Numar Miscari : %d', [MiscariCount]));
        for J := 0 to MiscariCount-1 do
          DumpLinie(' Rulaj', Miscari[J], 2);
        AInfo.Add('-----------------------------------');
      end;
    AInfo.Add('');
    AInfo.Add('Lista Rulaje');
    for I := 0 to AExtras.RulajCount-1 do begin
      DumpLinie('', AExtras.Rulaj[I], 1);
      AProgress.Position := I;
      Application.ProcessMessages;
    end;
    AInfo.Add('-----------------------------------');
    AInfo.Add('-----------------------------------');
  finally
    AInfo.EndUpdate;
  end;
end;

procedure TfrmPreluareExtrase.btnPreiaClick(Sender: TObject);
var
  lExtras: TExtras;
  lRulaj : TLinieExtras;
  I: Integer;
  lInfoLines: TStrings;
begin
  lExtras := TExtras.Create(edNumeFisier.Text);
  try
    lExtras.OnStartProgress := DoUpdateTotalProgress;
    lExtras.OnProgress := DoUpdateProgress;
    lExtras.Open;
    lInfoLines := TStringList.Create;
    try
      DumpExtras(lExtras, lInfoLines, progresAnaliza);
      prelInfo.Lines.Assign(lInfoLines);
    finally
      lInfoLines.Free;
    end;
    tblExtras.Close;
    tblExtras.Open;
    tblExtras.DisableControls;
    try
      progresTransfer.Position := 0;
      progresTransfer.Properties.Max := lExtras.RulajCount;
      for I := 0 to lExtras.RulajCount-1 do begin
        lRulaj := lExtras.Rulaj[I];
        tblExtras.Append;
        tblExtrascontClient.AsString      := lRulaj.ContClient;
        tblExtrasnumeContClient.AsString  := lRulaj.NumeContClient;
        tblExtrascontCrsp.AsString        := lRulaj.ContCoresp;
        tblExtrasnrDoc.AsString           := lRulaj.NrDoc;
        tblExtrasdataDoc.AsDateTime       := lRulaj.DataDoc;
        tblExtrasdataPlata.AsDateTime     := lRulaj.DataPlata;
        tblExtrascuiClient.AsString       := lRulaj.CuiClient;
        tblExtrascuiPlatitor.AsString     := lRulaj.CuiPlatitor;
        tblExtrascuiBeneficiar.AsString   := lRulaj.CuiBeneficiar;
        tblExtrassumaDebit.AsCurrency     := lRulaj.SumaDebit;
        tblExtrassumaCredit.AsCurrency    := lRulaj.SumaCredit;
        tblExtrasibanClient.AsString      := lRulaj.IbanClient;
        tblExtrasibanPlatitor.AsString    := lRulaj.IbanPlatitor;
        tblExtrasibanBeneficiar.AsString  := lRulaj.IbanBeneficiar;
        tblExtrasnumePlatitor.AsString    := lRulaj.NumePlatitor;
        tblExtrasnumeBeneficiar.AsString  := lRulaj.NumeBeneficiar;
        tblExtrasbicBancaDest.AsString    := lRulaj.BicBancaDest;
        tblExtrasbicBancaExt.AsString     := lRulaj.BicBancaExp;
        tblExtrasData.AsDateTime          := lRulaj.DataZi;
        tblExtrasexplicatii.AsString      := lRulaj.Explicatii;
        tblExtras.Post;
        progresTransfer.Position := I;
        Application.ProcessMessages;
      end;
    finally
      tblExtras.EnableControls;
    end;
  finally
    lExtras.Free;
  end;
end;

procedure TfrmPreluareExtrase.DoUpdateProgress(Sender: TObject;
  const Position: Integer);
begin
  progresProcesare.Position := Position;
  Application.ProcessMessages;
end;

procedure TfrmPreluareExtrase.DoUpdateTotalProgress(Sender: TObject;
  const TotalProgress: Integer);
begin
  progresProcesare.Properties.Max := TotalProgress;
  progresProcesare.Position := 0;
  Application.ProcessMessages;
end;

end.

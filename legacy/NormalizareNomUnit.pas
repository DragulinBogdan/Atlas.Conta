unit NormalizareNomUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, dxExEdtr, dxCntner, dxTL, dxDBCtrl, dxDBGrid, StdCtrls,
  dxEditor, dxEdLib, cxButtons, DB, ZDataSet, dxDBTLCl, dxGrClms, dxmdaset,
  Menus, ActnList, ImgList, DegradePanel, cxControls, cxContainer, cxEdit,
  cxProgressBar,cxLookAndFeelPainters, cxSplitter, cxTextEdit, cxMemo, cxDBEdit,
  ZAbstractRODataset, ZAbstractDataset, cxGraphics, cxLookAndFeels, ATSDBEvaluator,
  cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage, cxNavigator,
  cxDBData, cxMaskEdit, cxCheckBox, cxImageComboBox, cxCurrencyEdit, cxClasses,
  cxGridCustomTableView, cxGridTableView, cxGridBandedTableView,
  cxGridDBBandedTableView, cxGridCustomView, cxGridLevel, cxGrid, cxDropDownEdit,
  dxBarCode, dxDBBarCode, dxDateRanges, dxScrollbarAnnotations;

type
  TfrmNormalizareNom = class(TForm)
    pnBottom: TPanel;
    pnRest: TPanel;
    btnOk: TcxButton;
    EditStyle: TdxEditStyleController;
    QryNomenclator: TZQuery;
    DTNomenclator: TDataSource;
    MemNomenclator: TdxMemData;
    btnSelectTot: TcxButton;
    SelectMenu: TPopupMenu;
    mnuSelecteazaTot: TMenuItem;
    mnuDeseleteazaTot: TMenuItem;
    NomActionList: TActionList;
    SelImgList: TImageList;
    CmdSelectAll: TAction;
    CmdDeselectAll: TAction;
    pnTop: TDegradePanel;
    Image1: TImage;
    Progress: TcxProgressBar;
    pnCodBare: TPanel;
    btnConfigBarCode: TcxButton;
    Split: TcxSplitter;
    DTDetaliiBare: TDataSource;
    qryDetaliiBare: TZQuery;
    edtCodBara: TcxDBMemo;
    nivelNomenclator: TcxGridLevel;
    gridNomenclator: TcxGrid;
    viewNomenclator: TcxGridDBBandedTableView;
    viewNomenclatorCODMAT: TcxGridDBBandedColumn;
    viewNomenclatorTIPMAT: TcxGridDBBandedColumn;
    viewNomenclatorDENMAT: TcxGridDBBandedColumn;
    viewNomenclatorUM: TcxGridDBBandedColumn;
    viewNomenclatorGrupaContabila: TcxGridDBBandedColumn;
    viewNomenclatorID_GEST_SUMATOR: TcxGridDBBandedColumn;
    viewNomenclatorPRET_UNITAR: TcxGridDBBandedColumn;
    viewNomenclatorCOTA_TVA: TcxGridDBBandedColumn;
    viewNomenclatorPRET_RECEPTIE_TVA: TcxGridDBBandedColumn;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    barCode: TdxDBBarCode;
    cxSplitter1: TcxSplitter;
    Panel1: TPanel;
    procedure FormCreate(Sender: TObject);
    procedure SetSelectatOnState(aState : Integer = 2);
    procedure gridNmclDblClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnConfigBarCodeClick(Sender: TObject);
    procedure CmdSelectAllExecute(Sender: TObject);
    procedure CmdDeselectAllExecute(Sender: TObject);
  private
    { Private declarations }
    //FCurentPret : Currency;
    procedure ReOpenNomenclator;
    procedure RefreshTipMaterial;
  public
    { Public declarations }
  end;

implementation

uses
  dxCompsUtile, ZeosDBUtile, unitMemTableEx, DateUnit, DetaliiMaterialUnit, CommonDBVar, configBarCode;

{$R *.dfm}

procedure TfrmNormalizareNom.FormCreate(Sender: TObject);
begin
  SetCheckedView(viewNomenclator);
  Progress.Visible := False;
  ReOpenNomenclator;
  RefreshTipMaterial;
end;

procedure TfrmNormalizareNom.ReOpenNomenclator;
var
  I         : Integer;
  lColumn   : TcxGridDBBandedColumn;
begin
  Progress.Visible := True;
  QryNomenclator.Close;
  QryNomenclator.Open;
  DBCopyFromDataSet(MemNomenclator, QryNomenclator, False);
  MemNomenclator.SortedField := 'CODMAT';
  for I := 0 to MemNomenclator.Fields.Count-1 do begin
    lColumn := viewNomenclator.CreateColumn;
    lColumn.DataBinding.FieldName := MemNomenclator.Fields[I].FieldName;
    lColumn.HeaderAlignmentHorz   := taCenter;
    lColumn.Caption               := GetNiceText(lColumn.DataBinding.FieldName);
    lColumn.Options.Editing       := False;
    lColumn.Position.BandIndex    := 1;
  end;
  viewNomenclator.DataController.DataSource := DTNomenclator;
  QryNomenclator.Close;
  DBRefresh(qryDetaliiBare);
  Progress.Visible := False;
end;

procedure TfrmNormalizareNom.RefreshTipMaterial;
begin
  FillImageCombo(viewNomenclatorGrupaContabila.Properties, 'select * from gest_tip_material', 'ID_GEST_TIP_MATERIAL', 'DENUMIRE', Null, '[Toate]');
end;

procedure TfrmNormalizareNom.SetSelectatOnState(aState: Integer);
var
  lLastPoz: TBookmark;
begin
  MemNomenclator.DisableControls;
  try
    lLastPoz := MemNomenclator.GetBookmark;
    try
      MemNomenclator.First;
      while not MemNomenclator.Eof do begin
        case aState of
          1 : MemNomenclator['SELECTAT'] := True;
          2 : MemNomenclator['SELECTAT'] := not MemNomenclator.FieldByName('SELECTAT').AsBoolean
          else
            MemNomenclator['SELECTAT'] := False;
        end;
        MemNomenclator.Next;
      end;
    finally
      MemNomenclator.GotoBookmark(lLastPoz);
      MemNomenclator.FreeBookmark(lLastPoz);
    end;
  finally
    MemNomenclator.EnableControls;
  end;
end;

procedure TfrmNormalizareNom.gridNmclDblClick(Sender: TObject);
var
  lRecord : TcxCustomGridRecord;
  lStrings: TStringList;
  I,
  lCodSumator : Integer;
  lTipMat,
  lDenMat,
  lUM     : String;
  lPretUnitar,
  lCotaTva,
  lPretReceptie : Currency;

begin
  lRecord := viewNomenclator.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    lTipMat         := ValueSafeToStr(lRecord.Values[viewNomenclatorTIPMAT.Index]);
    lDenMat         := ValueSafeToStr(lRecord.Values[viewNomenclatorDENMAT.Index]);
    lUM             := ValueSafeToStr(lRecord.Values[viewNomenclatorUM.Index]);
    lPretUnitar     := ValueSafeToCurrency(lRecord.Values[viewNomenclatorPRET_UNITAR.Index]);
    lCotaTva        := ValueSafeToCurrency(lRecord.Values[viewNomenclatorCOTA_TVA.Index]);
    lPretReceptie   := ValueSafeToCurrency(lRecord.Values[viewNomenclatorPRET_RECEPTIE_TVA.Index]);
    lCodSumator     := ValueSafeToInt(lRecord.Values[viewNomenclatorID_GEST_SUMATOR.Index]);
    if AdministrareMaterial(lTipMat, lDenMat, lUM, lPretUnitar, lCotaTva, lPretReceptie) then begin
      lStrings := TStringList.Create;
      try
        lStrings.Duplicates := dupIgnore;
        lStrings.Sorted     := True;
        GetCheckedViewList(viewNomenclator, viewNomenclatorCODMAT, lStrings);
        lStrings.Add( ValueSafeToStr(lRecord.Values[viewNomenclatorCODMAT.Index]) );
        for I := 0 to lStrings.Count-1 do
          DBExecSQLFmt('exec [SP_UPDATE_GEST_GNMCL] %s, %d, %s, %s, %s', [lStrings[I], lCodSumator, lTipMat, lDenMat, lUM]);
      finally
        lStrings.Free;
      end;
    end;
  end;
end;

procedure TfrmNormalizareNom.CmdDeselectAllExecute(Sender: TObject);
begin
  SetCheckFilterItems(viewNomenclator, False);
end;

procedure TfrmNormalizareNom.CmdSelectAllExecute(Sender: TObject);
begin
  SetCheckFilterItems(viewNomenclator, True);
end;

procedure TfrmNormalizareNom.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmNormalizareNom.btnConfigBarCodeClick(Sender: TObject);
begin
  with TfrmConfigBarCode.Create(nil) do
  try
     memText.Text := edtCodBara.Text;
     ShowModal;
  finally
    free;
  end;
end;

end.

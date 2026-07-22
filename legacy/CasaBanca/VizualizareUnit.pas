unit VizualizareUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, HeadPanel, dxCntner, dxTL, dxDBCtrl, dxDBGrid,  Db,
  dxmdaset, dxEdLib, dxExEdtr, dxDBTLCl, dxGrClms;

type
  TFrmListaCasa = class(TForm)
    pnTop: THeadPanel;
    pnContent: TPanel;
    GridVisual: TdxDBGrid;
    DTVisual: TDataSource;
    MemLista: TdxMemData;
    MemListaID_LISTA: TStringField;
    MemListaID_PARINTE: TStringField;
    MemListaCOD_CB: TIntegerField;
    MemListaCOD: TIntegerField;
    MemListaCODGEST: TStringField;
    MemListaDATA: TDateTimeField;
    MemListaTIPDOC: TStringField;
    MemListaNRDOC: TStringField;
    MemListaPOZ: TIntegerField;
    MemListaEXPLICATIE: TStringField;
    MemListaINCASARI: TBCDField;
    MemListaPLATI: TBCDField;
    MemListaSOLD: TBCDField;
    MemListaCONT_CSP: TStringField;
    MemListaVAL_CRSP: TBCDField;
    MemListaACHITAT: TBCDField;
    MemListaDATAEM: TDateTimeField;
    MemListaC_O: TIntegerField;
    MemListaNR_LIST: TIntegerField;
    MemListaCURS_SCHIM: TBCDField;
    MemListaECL: TWordField;
    MemListaON_SERVER: TIntegerField;
    MemListaSOLD_NOU: TCurrencyField;
    MemListaSORTFIELD: TStringField;
    MemListaID_PROIECT: TStringField;
    MemListaPEXPLIC: TMemoField;
    MemListaMEXPLIC: TMemoField;
    MemListaVALIDATA: TWordField;
    MemListaTRANSFER: TIntegerField;
    MemListaCOD_CBT: TIntegerField;
    MemListaCOD_TRANSFER: TIntegerField;
    MemListaDATA_ACCEPT: TDateTimeField;
    MemListaID_TIPURI_CHELTVEN: TIntegerField;
    MemListaPARENT_COD: TIntegerField;
    MemListaNR_DECONT: TIntegerField;
    MemListaDATA_DECONT: TDateTimeField;
    MemListaID_ORGANIGRAMA: TIntegerField;
    MemListaID_RESURSA: TIntegerField;
    GridVisualRecId: TdxDBGridColumn;
    GridVisualID_LISTA: TdxDBGridMaskColumn;
    GridVisualID_PARINTE: TdxDBGridMaskColumn;
    GridVisualCOD: TdxDBGridMaskColumn;
    GridVisualDATA: TdxDBGridDateColumn;
    GridVisualTIPDOC: TdxDBGridMaskColumn;
    GridVisualNRDOC: TdxDBGridMaskColumn;
    GridVisualPOZ: TdxDBGridMaskColumn;
    GridVisualEXPLICATIE: TdxDBGridMaskColumn;
    GridVisualINCASARI: TdxDBGridMaskColumn;
    GridVisualPLATI: TdxDBGridMaskColumn;
    GridVisualSOLD: TdxDBGridMaskColumn;
    GridVisualCONT_CSP: TdxDBGridMaskColumn;
    GridVisualVAL_CRSP: TdxDBGridMaskColumn;
    GridVisualACHITAT: TdxDBGridMaskColumn;
    GridVisualDATAEM: TdxDBGridDateColumn;
    GridVisualC_O: TdxDBGridMaskColumn;
    GridVisualNR_LIST: TdxDBGridMaskColumn;
    GridVisualCURS_SCHIM: TdxDBGridMaskColumn;
    GridVisualECL: TdxDBGridMaskColumn;
    GridVisualON_SERVER: TdxDBGridMaskColumn;
    GridVisualSOLD_NOU: TdxDBGridColumn;
    GridVisualSORTFIELD: TdxDBGridColumn;
    GridVisualPEXPLIC: TdxDBGridMemoColumn;
    GridVisualMEXPLIC: TdxDBGridMemoColumn;
    GridVisualVALIDATA: TdxDBGridMaskColumn;
    GridVisualTRANSFER: TdxDBGridMaskColumn;
    GridVisualCOD_CBT: TdxDBGridMaskColumn;
    GridVisualCOD_TRANSFER: TdxDBGridMaskColumn;
    GridVisualDATA_ACCEPT: TdxDBGridDateColumn;
    GridVisualPARENT_COD: TdxDBGridMaskColumn;
    GridVisualNR_DECONT: TdxDBGridMaskColumn;
    GridVisualDATA_DECONT: TdxDBGridDateColumn;
    GridVisualCOD_CB: TdxDBGridImageColumn;
    GridVisualID_PROIECT: TdxDBGridImageColumn;
    GridVisualID_TIPURI_CHELTVEN: TdxDBGridImageColumn;
    GridVisualID_ORGANIGRAMA: TdxDBGridImageColumn;
    GridVisualID_RESURSA: TdxDBGridImageColumn;
    GridVisualCODGEST: TdxDBGridImageColumn;
    procedure GridVisualChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridVisualCustomDraw(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxDBTreeListColumn;
      const AText: String; AFont: TFont; var AColor: TColor; ASelected,
      AFocused: Boolean; var ADone: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure FormShow(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
    CodCurent   : String;
    DataEmitere : String;
    CasaHolder  : TdxImageEdit;
  end;


implementation

uses DateUnit;

{$R *.DFM}

procedure TFrmListaCasa.GridVisualChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
//svar lParent: Integer;
begin
{  DataEmitere := Node.Strings[GridVisualDATAEM.Index];
  try
    lParent := Node.Values[GridVisualID_PARINTE.Index];
  except
    lParent := -1;
  end;
  if lParent = -1 then CodCurent := VarToStr(TdxDBGridNode(Node).Id)
  else CodCurent := IntToStr(lParent);

  GridVisual.Invalidate;}
end;

procedure TFrmListaCasa.GridVisualCustomDraw(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxDBTreeListColumn; const AText: String; AFont: TFont;
  var AColor: TColor; ASelected, AFocused: Boolean; var ADone: Boolean);
begin
{  if AFocused then begin
     AColor := clBlue;
     AFont.Color := clYellow;
     Exit;
  end;
  if DataEmitere <> '' then
  if DataEmitere = ANode.Strings[GridVisualDATAEM.Index] then begin
     if CodCurent = ANode.Values[GridVisualID_PARINTE.Index] then
         AColor := clTeal
     else if VarToStr(TdxDBGridNode(ANode).Id) = CodCurent then
              AColor := clGreen
           else AColor := clAqua; z
  end;}
end;

procedure TFrmListaCasa.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TFrmListaCasa.FormCreate(Sender: TObject);
begin
  {deschidem si setul de date}
  if Assigned(CasaHolder) then begin
    GridVisualCOD_CB.Descriptions.Assign(CasaHolder.Descriptions);
    GridVisualCOD_CB.Values.Assign(CasaHolder.Values);
  end;

  PopulateImage(FrmData.QryBGPlanFunctional,
                GridVisualID_PROIECT.Values, GridVisualID_PROIECT.Descriptions,
                'ID_BG_PLAN_FUNCTIONAL', 'DENUMIRE');

  PopulateImage(FrmData.QryBGPlanEconomic, GridVisualID_TIPURI_CHELTVEN.Values,
    GridVisualID_TIPURI_CHELTVEN.Descriptions, 'ID_BG_PLAN_ECONOMIC', 'DENUMIRE');

  PopulateImage(FrmData.QryCasaFunctie, GridVisualID_ORGANIGRAMA.Values,
    GridVisualID_ORGANIGRAMA.Descriptions, 'ID_ORGANIGRAMA', 'DENUMIRE');

  PopulateImage(FrmData.QryCasaSalariati, GridVisualID_RESURSA.Values,
    GridVisualID_RESURSA.Descriptions, 'ID_REPARTITORI', 'NUME');

  PopulateImage(frmData.QryRepartitori, GridVisualCODGEST.Values, GridVisualCODGEST.Descriptions,
    'ID_REPARTITORI', 'NUME');
end;


procedure TFrmListaCasa.FormShow(Sender: TObject);
begin
  pnTop.Info := Format('Vizualizare %s', ['']);
end;

end.

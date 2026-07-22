unit RegistruUnitEx;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, Menus, ImgList, ActnList, ZDataSet, Db, dxmdaset, dxExEdtr, dxEdLib, dxDBELib,
  StdCtrls, Spin, dxCntner, dxEditor, Buttons, dxGrClEx, dxTL, dxDBCtrl, SyncProgressUnit,
  CommonCasa, CommonDBVar, AcceptTransferUnit, ContainerUnit, MaintenanceUnit, cxControls,
  dxStatusBar, AlopDisponibil, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters,
  cxCustomData, cxStyles, cxTL, cxMaskEdit, cxCalendar, cxButtonEdit, cxImageComboBox,
  cxDropDownEdit, cxCurrencyEdit, cxTextEdit, cxCheckBox, cxTLdxBarBuiltInMenu, cxContainer,
  cxEdit, ZAbstractRODataset, ZAbstractDataset, dxDBTLCl,
  dxDBTL, cxButtons, cxInplaceContainer, cxDBTL, cxTLData, cxSpinEdit, DBClient, cxMemo, cxBlobEdit,
  Vcl.ComCtrls, dxCore, cxDateUtils, cxClasses, cxGroupBox, FontGroupBox,
  cxLabel, dxScrollbarAnnotations;


type
  TFrmRegistruEx = class(TForm)
    pnRest: TPanel;
    Splitter2: TSplitter;
    pnTop: TcxGroupBox;
    dtRegistru: TDataSource;
    Cmd_RegistruCasa: TActionList;
    Cmd_EchilibrarePlata: TAction;
    Cmd_AdaugaPlata: TAction;
    Cmd_DeletePlata: TAction;
    Cmd_SalveazaPlata: TAction;
    GridRegistruPopup: TPopupMenu;
    AdaugaPlataIncasare: TMenuItem;
    StergerePlata: TMenuItem;
    EchilibreazaPlataIncasare: TMenuItem;
    Cmd_TransferaPlata: TAction;
    pnDetail: TcxCollapsedGroup;
    Splitter1: TSplitter;
    DBExplicCont: TdxDBMemo;
    DBExplicProj: TdxDBMemo;
    Transferinaltacasa: TMenuItem;
    Cmd_AcceptaTransfer: TAction;

    AcceptaTransfer: TMenuItem;
    Cmd_Validate: TAction;
    Validare: TMenuItem;
    Cmd_UnValidate: TAction;
    Cmd_AnuleazaTransfer: TAction;
    Cmd_GenereazaDiferenta: TAction;
    GenereazaDiferenta: TMenuItem;

    AnuleazaTransfer: TMenuItem;
    Cmd_JustificareAvans: TAction;
    Cmd_Renumeroteaza: TAction;
    Renumeroteaza: TMenuItem;
    N1: TMenuItem;
    N2: TMenuItem;
    N3: TMenuItem;
    Splitter3: TSplitter;
    pnFilter: TPanel;
    chkFilter: TcxCheckBox;
    lblFilter: TLabel;
    pnSummary: TPanel;
    SummStatus: TdxStatusBar;
    SelectedSumm: TdxStatusBar;
    DeValidare1: TMenuItem;
    N4: TMenuItem;
    JustificareAvans: TMenuItem;
    Cmd_RenumeroteazaAll: TAction;
    RenumeroteazaEcran1: TMenuItem;
    Cmd_Import: TAction;
    ImportdinaltaCasa1: TMenuItem;
    CmdErrors: TAction;
    Cmd_SaveLocal: TAction;
    Cmd_RecalculateSold: TAction;
    Cmd_ShowDetail: TAction;
    Cmd_ShowSummary: TAction;
    Cmd_ShowLegend: TAction;
    Cmd_ValideazaIesire: TAction;
    Cmd_Flag: TAction;
    FlagInregistrareaCurenta1: TMenuItem;
    qryListaBanci: TZQuery;
    dtListaBanci: TDataSource;
    Cmd_GotoRecord: TAction;
    Cmd_VenitCasa: TAction;
    ImportCasaBanca1: TMenuItem;
    PozitionareInregistrare1: TMenuItem;
    Cmd_SetBandSize: TAction;
    SetareMarimeBanda1: TMenuItem;
    Cmd_DispozitiePlata: TAction;
    TiparesteDispozitiePlata1: TMenuItem;
    edtTextFiltru: TdxEdit;
    CmdDecont: TAction;
    DecontareDocumentFurnizor1: TMenuItem;
    Cmd_TransferaPozitie: TAction;
    CmdCopyColumn: TAction;
    Copiazacoloanacurenta1: TMenuItem;
    treeRegistru: TcxDBTreeList;
    treeCasierii: TcxDBTreeList;
    treeCasieriiCOD_CB: TcxDBTreeListColumn;
    treeCasieriiCOD_PARINTE: TcxDBTreeListColumn;
    treeCasieriiDENUMIRE: TcxDBTreeListColumn;
    treeCasieriiDENV: TcxDBTreeListColumn;
    treeCasieriiC_O: TcxDBTreeListColumn;
    treeCasieriiDATA_SOLD: TcxDBTreeListColumn;
    treeCasieriiCASIER: TcxDBTreeListColumn;
    treeCasieriiVALIDATOR: TcxDBTreeListColumn;
    treeCasieriiADMIN: TcxDBTreeListColumn;
    treeCasieriiIS_BANCA: TcxDBTreeListColumn;
    treeCasieriiIS_AVANS: TcxDBTreeListColumn;
    treeCasieriiIS_TEMPOR: TcxDBTreeListColumn;
    treeCasieriiID_REPARTITORI: TcxDBTreeListColumn;
    treeCasieriiICON: TcxDBTreeListColumn;
    treeCasieriiID_VALUTA: TcxDBTreeListColumn;
    treeCasieriiDESCRIERE: TcxDBTreeListColumn;
    treeCasieriiCRSP_LEI: TcxDBTreeListColumn;
    treeCasieriicodFunctional: TcxDBTreeListColumn;
    qryRegistru: TZQuery;
    treeRegistruidRegistru: TcxDBTreeListColumn;
    treeRegistrurefInitial: TcxDBTreeListColumn;
    treeRegistrurefRepartitor: TcxDBTreeListColumn;
    treeRegistrurefParinte: TcxDBTreeListColumn;
    treeRegistrunumeTipDoc: TcxDBTreeListColumn;
    treeRegistrunumarDoc: TcxDBTreeListColumn;
    treeRegistrudescScurta: TcxDBTreeListColumn;
    treeRegistrudescLunga: TcxDBTreeListColumn;
    treeRegistrucursSchimb: TcxDBTreeListColumn;
    treeRegistruvalIncasare: TcxDBTreeListColumn;
    treeRegistruvalPlata: TcxDBTreeListColumn;
    treeRegistruisOnCredit: TcxDBTreeListColumn;
    treeRegistrusemnSuma: TcxDBTreeListColumn;
    treeRegistrusold: TcxDBTreeListColumn;
    treeRegistrusoldRON: TcxDBTreeListColumn;
    treeRegistruechilibrata: TcxDBTreeListColumn;
    treeRegistruvalidata: TcxDBTreeListColumn;
    treeRegistrurefTipTransfer: TcxDBTreeListColumn;
    treeRegistrurefTransfer: TcxDBTreeListColumn;
    treeRegistrurefCasaTransfer: TcxDBTreeListColumn;
    treeRegistrucontCorespondent: TcxDBTreeListColumn;
    treeRegistrudataEmitere: TcxDBTreeListColumn;
    treeRegistrunrDecont: TcxDBTreeListColumn;
    treeRegistrudataDecont: TcxDBTreeListColumn;
    treeRegistrurefUserAdaugare: TcxDBTreeListColumn;
    treeRegistrurefUserValidare: TcxDBTreeListColumn;
    treeRegistruhashValidare: TcxDBTreeListColumn;
    treeRegistrunumarExtras: TcxDBTreeListColumn;
    treeRegistrudataExtras: TcxDBTreeListColumn;
    treeRegistruversiuneRand: TcxDBTreeListColumn;
    treeRegistrurefCasierie: TcxDBTreeListColumn;
    treeRegistrudataRegistru: TcxDBTreeListColumn;
    treeRegistrupozRegistru: TcxDBTreeListColumn;
    stiluriRegistru: TcxStyleRepository;
    stilPrimulNivel: TcxStyle;
    stilNivelulDoi: TcxStyle;
    stilNivelDoiSters: TcxStyle;
    stilCurentNivelUnu: TcxStyle;
    stilCurentNivelDoi: TcxStyle;
    stilValidat: TcxStyle;
    stilDataCurenta: TcxStyle;
    stilAreFocus: TcxStyle;
    edCurentHouse: TcxPopupEdit;
    edListaData: TcxImageComboBox;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    function GetFullXML: String;
  end;

var
  cstVersiuneRegistru : String = '1.01';

implementation

{$R *.DFM}

uses
  ZeosDBUtile;

procedure TFrmRegistruEx.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

function TFrmRegistruEx.GetFullXML: String;
var
  lPrevDataSetState : TDisabledDataSetEntry;
begin
  if qryRegistru.UpdatesPending then begin
    lPrevDataSetState := DBDisableDataSet(qryRegistru);
    qryRegistru.ShowRecordTypes := [usDeleted, usModified, usInserted];
    try
      Result := Format(
                      '<registruCasa ver="%s" codCB="%d" denCB="%s" data="%s" soldIni="%s" peZi="%s" nrZile="%d" tipDefalcare="%d" regCount="%d">',
                        [
                          cstVersiuneRegistru,
//                          FCurentHouse,
                          edCurentHouse.EditText,
//                          FormatDateTime('yyyy-MM-dd', edData.Date),
//                          ValueToStr(FSoldInitial, False, ''),
//                          ValueToStr(chkEfectiv.Checked, False, ''),
//                          ValueToStr(edNrZile.EditValue, False, ''),
//                          GetDefalcareType,
                          qryRegistru.RecordCount
                        ]);
      qryRegistru.First;
      while not qryRegistru.Eof do begin
        Result := Result + Format(#13#10#9'<regEntry%s/>', [DBRowToXML(qryRegistru, 'idRegistru;tipNivel')]);
        qryRegistru.Next;
      end;
      Result := Result + #13#10'</registruCasa>';
    finally
      qryRegistru.ShowRecordTypes := [usUnmodified, usModified, usInserted];
      DBEnableDataSet(lPrevDataSetState);
    end;
  end;
end;

end.

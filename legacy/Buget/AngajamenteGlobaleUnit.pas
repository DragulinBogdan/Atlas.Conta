unit AngajamenteGlobaleUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, dxCntner, dxTL, dxDBCtrl, dxDBGrid, Db,
  ZDataSet, StdCtrls, dxEditor, dxExEdtr, dxEdLib, dxDBTLCl, dxGrClms,
  dxDBTL, Buttons,
  ZAbstractRODataset, ZAbstractDataset, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxContainer, cxEdit, cxGroupBox, cxRepartitorPanel,
  cxTextEdit, cxMaskEdit, cxSpinEdit, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxDBData,
  cxCurrencyEdit, cxCalendar, cxClasses, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxGridCustomView, cxGrid, cxTL,
  cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData, cxDBTL;

type
  TfrmAngajamenteGlobale = class(TForm)
    Panel1: TPanel;
    Panel2: TPanel;
    Panel3: TPanel;
    Departament: TcxRepartitorPanel;
    edRepartitor: TcxRepartitorPanel;
    DTGlobale: TDataSource;
    QryGlobale: TZQuery;
    Label3: TLabel;
    edAnFiscal: TcxSpinEdit;
    TreeRepartitori: TdxDBTreeList;
    TreeRepartitoriNUME: TdxDBTreeListMaskColumn;
    TreeRepartitoriADRESA: TdxDBTreeListMaskColumn;
    TreeRepartitoriCONT: TdxDBTreeListMaskColumn;
    TreeRepartitoriCODFISC: TdxDBTreeListMaskColumn;
    TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn;
    BtnGenerare: TSpeedButton;
    viewAngajamenteLocale: TcxGridDBTableView;
    nivelAngajamenteGlobale: TcxGridLevel;
    gridAngajamenteGlobale: TcxGrid;
    viewAngajamenteLocaleCOD_FUNCTIONAL: TcxGridDBColumn;
    viewAngajamenteLocaleCOD_ECONOMIC: TcxGridDBColumn;
    viewAngajamenteLocaleAPROBATE: TcxGridDBColumn;
    viewAngajamenteLocaleTOTAL_ANGAJATE: TcxGridDBColumn;
    viewAngajamenteLocaleANGAJAT: TcxGridDBColumn;
    viewAngajamenteLocaleDATA: TcxGridDBColumn;
    procedure Panel1Resize(Sender: TObject);
    procedure edAnFiscalChange(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnGenerareClick(Sender: TObject);
    procedure TreeRepartitoriDblClick(Sender: TObject);
    procedure TreeRepartitoriKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure DepartamentValidate(Sender: TObject; var AKeyValue: Variant);
    procedure edRepartitorValidate(Sender: TObject;
      var AKeyValue: Variant);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
    FIdDepartament,
    FIdRepartitor: Integer;
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses DateUnit, CommonDBVar;

procedure TfrmAngajamenteGlobale.Panel1Resize(Sender: TObject);
begin
  Departament.Left   := 10;
  Departament.Width  := edAnFiscal.Left - Departament.Left - 10;
  edRepartitor.Left  := edAnFiscal.Left + edAnFiscal.Width + 10;
  edRepartitor.Width := Panel1.Width - edRepartitor.Left - 10;
end;

procedure TfrmAngajamenteGlobale.edAnFiscalChange(Sender: TObject);
begin
  QryGlobale.Close;
  QryGlobale.Params[0].Value := edAnFiscal.EditValue;
  QryGlobale.Open;
end;

procedure TfrmAngajamenteGlobale.FormCreate(Sender: TObject);
var Y, M, D: Word;
begin
  DecodeDate(Date, Y, M, D);
  edAnFiscal.EditValue := Y;
  edAnFiscalChange(edAnFiscal);
end;

procedure TfrmAngajamenteGlobale.BtnGenerareClick(Sender: TObject);
var lStartNr: Integer;
begin
  with GetTmpADOQuery do
    try
       Sql.Add('DELETE FROM ANGAJAMENTE_DEFALCARE WHERE ID_ANGAJAMENT IN (SELECT ID_ANGAJAMENT FROM ANGAJAMENTE WHERE YEAR(DATA_EMITERE) = '+IntToStr(edAnFiscal.EditValue)+' AND TIP_ANGAJAMENT = 0)');
       Sql.Add('DELETE FROM ANGAJAMENTE WHERE YEAR(DATA_EMITERE) = '+IntToStr(edAnFiscal.EditValue)+' AND TIP_ANGAJAMENT = 0');
       ExecSql;
       Sql.Clear;
       DataSource := DTGlobale;
       Sql.Add('INSERT INTO ANGAJAMENTE (ID_UTILIZATORI, DATA_EMITERE, ID_DEPARTAMENT, NUMAR, SCOPUL, ID_LST_REPARTITORI, VALIDAT, CLASA_FUNCTIONALA, TIP_ANGAJAMENT)');
       Sql.Add('VALUES ('+IntToStr(IdUtilizator)+', :DATA, '+IntToStr(FIdDepartament)+', :NUMAR_ACT, ''GLOBAL'', '+IntToStr(FIdRepartitor)+', 1, :COD_FUNCTIONAL, 0)');
       Sql.Add('INSERT INTO ANGAJAMENTE_DEFALCARE (ID_UTILIZATORI, ID_ANGAJAMENT, COD_ECONOMIC, APROBATE, TOTAL_ANGAJATE, DISPONIBIL, ANGAJAT, RAMAS_DE_ANGAJAT, VALIDAT, DESCRIERE, CLASA_ECONOMICA)');
       Sql.Add('VALUES ('+IntToStr(IdUtilizator)+', SCOPE_IDENTITY(), :COD_ECONOMIC, :APROBATE, :TOTAL_ANGAJATE, :DISPONIBIL, :ANGAJAT, :RAMAS_DE_ANGAJAT, 1, ''GLOBAL'', :CLASA_ECONOMICA)');
       QryGlobale.First;
       lStartNr := 0;
       while not QryGlobale.Eof do begin
         lStartNr := lStartNr + 1;
         Params.ParamByName('NUMAR_ACT').Value := IntToStr(lStartNr);
         ExecSQL;
         QryGlobale.Next;
       end;
    finally
       Free;
    end;
end;

procedure TfrmAngajamenteGlobale.TreeRepartitoriDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmAngajamenteGlobale.TreeRepartitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (not (ssCtrl in Shift)) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmAngajamenteGlobale.DepartamentValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  FIdDepartament := AKeyValue;
end;

procedure TfrmAngajamenteGlobale.edRepartitorValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  FIdRepartitor := AKeyValue;
end;

procedure TfrmAngajamenteGlobale.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

end.

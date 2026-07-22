unit SelBugetUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls,
  cxGraphics, cxTL, cxMaskEdit, cxCurrencyEdit,
  cxCheckBox, cxInplaceContainer, cxDBTL, cxTLData, cxPC, cxControls, DB,
  ZDataSet,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxStyles, dxBarBuiltInMenu,
  dxScrollbarAnnotations;

type
  TfrmSelBuget = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    btnCancel: TcxButton;
    PaginaClasificatii: TcxPageControl;
    cxTabFunctional: TcxTabSheet;
    pnFunctClient: TPanel;
    cxTreeFunctional: TcxDBTreeList;
    cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalCLASA: TcxDBTreeListColumn;
    cxTreeFunctionalCAPITOL: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_NIVEL_RAPORTARE: TcxDBTreeListColumn;
    cxTabEconomic: TcxTabSheet;
    pnEcoClient: TPanel;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    DTPlanFunctional: TDataSource;
    DTPlanEconomic: TDataSource;
    qryPlanFunctional: TZQuery;
    qryPlanEconomic: TZQuery;
    cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicid_parinte: TcxDBTreeListColumn;
    cxTreeEconomiceste_local: TcxDBTreeListColumn;
    cxTreeEconomiceste_standard: TcxDBTreeListColumn;
    cxTreeEconomicbold: TcxDBTreeListColumn;
    cxTreeEconomictip_reflectare: TcxDBTreeListColumn;
    cxTreeEconomicid_oi_proiecte: TcxDBTreeListColumn;
    cxTreeEconomiccod_ecran: TcxDBTreeListColumn;
    cxTreeEconomicclasa_ecran: TcxDBTreeListColumn;
    cxTreeEconomictip: TcxDBTreeListColumn;
    cxTreeEconomicid_analitic: TcxDBTreeListColumn;
    cxTreeEconomiccod_functional: TcxDBTreeListColumn;
    cxTreeEconomicid_oi_unitati: TcxDBTreeListColumn;
    cxTreeFunctionalID_BG_TIPURI_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeFunctionalID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeFunctionalID_PARINTE: TcxDBTreeListColumn;
    cxTreeFunctionalTIP_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_STANDARD: TcxDBTreeListColumn;
    cxTreeFunctionalBOLD: TcxDBTreeListColumn;
    cxTreeFunctionalTIP_REFLECTARE: TcxDBTreeListColumn;
    cxTreeFunctionalID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeFunctionalcod_ecran: TcxDBTreeListColumn;
    cxTreeFunctionalclasa_ecran: TcxDBTreeListColumn;
    cxTreeFunctionalTIP: TcxDBTreeListColumn;
    cxTreeFunctionalID_ANALITIC: TcxDBTreeListColumn;
    procedure pnBottomResize(Sender: TObject);
    procedure cxTreeFunctionalDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeFunctionalFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure cxTreeFunctionalDblClick(Sender: TObject);
    procedure cxTreeFunctionalKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
  public
    { Public declarations }
    FOnlyChild : Boolean;
  end;


function NewSelectarePlanEconomic(var aCodEconomic : String; var aIdProiect : Integer;  const aCodFunctional : String = ''; const aIdUnitate : Integer = -1; const NumaiFrunze : Boolean = False; AOwner: TComponent = nil) : String;
function NewSelectarePlanFunctional(var aCodFunctional : String; var aIdUnitate : Integer; const NumaiFrunze : Boolean = False; const FaraUnitati : boolean = False; AOwner: TComponent = nil) : String;


implementation

uses
  dateUnit, ZeosDBUtile, CommonDBVar;

{$R *.dfm}


function NewSelectarePlanFunctional(var aCodFunctional : String; var aIdUnitate : Integer; const NumaiFrunze : Boolean = False; const FaraUnitati : boolean = False; AOwner: TComponent = nil) : String;
var
  frmPlanBugete : TfrmSelBuget;
  aNode : TcxTreeListNode;
  aId : Integer;
begin
  frmPlanBugete := TfrmSelBuget.Create(AOwner);
  Result := '';
  with frmPlanBugete do
    try
      frmPlanBugete.Visible := False;
      FOnlyChild := NumaiFrunze;
      Caption := 'Selectie cod functional';
      cxtabFunctional.TabVisible := False;
      cxtabEconomic.TabVisible := False;
      PaginaClasificatii.ActivePageIndex := 0;
      cxTreeFunctional.PopupMenu := nil;
      DBRefresh(qryPlanFunctional);
      if FaraUnitati then begin
         qryPlanFunctional.Filtered := False;
         //qryPlanFunctional.Filter := 'ID_OI_UNITATI <> 0 and ID_OI_UNITATI IS NOT NULL';
         qryPlanFunctional.Filter := 'ID_ANALITIC = 0 and ID_ANALITIC IS NULL';
         qryPlanFunctional.Filtered := True;
         if qryPlanFunctional.RecordCount = 0 then begin
            qryPlanFunctional.Filtered := False;
            qryPlanFunctional.Filter := '';
         end;
      end
      else begin
         qryPlanFunctional.Filtered := False;
         qryPlanFunctional.Filter := '';
      end;
      if Trim(aCodFunctional) <> '' then begin
        aId := -1;
        if aIdUnitate > 0 then begin
          if qryPlanFunctional.Locate('COD_FUNCTIONAL;ID_OI_UNITATI', VarArrayOf([aCodFunctional, aIdUnitate]), []) then
            aId := qryPlanFunctional.FieldByName('ID_BG_PLAN_FUNCTIONAL').AsInteger;
        end
        else
          if qryPlanFunctional.Locate('COD_FUNCTIONAL', aCodFunctional, []) then
            aId := qryPlanFunctional.FieldByName('ID_BG_PLAN_FUNCTIONAL').AsInteger;
        aNode := cxTreeFunctional.FindNodeByKeyValue(aId, nil);
      end
      else
        aNode := cxTreeFunctional.TopNode;

      if aNode <> nil then begin
        aNode.MakeVisible;
        aNode.Focused := True;
      end;
      ActiveControl := cxTreeFunctional;
      ShowModal;
      if ModalResult = mrOk then begin
        aNode := cxTreeFunctional.FocusedNode;
        if (NumaiFrunze) and (aNode <> nil) and (aNode.HasChildren) then raise EContaHandledError.Create('Codul Functional selectat trebuie sa fie un analitic ! Va rugam refaceti selectia !');
        if (aNode <> nil) then begin
          aCodFunctional := aNode.Texts[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex];
          if aNode.Texts[cxTreeFunctionalID_ANALITIC.ItemIndex] <> '' then
            aIdUnitate := aNode.Values[cxTreeFunctionalID_ANALITIC.ItemIndex]
          else
            aIdUnitate := -1;
          Result := aNode.Texts[cxTreeFunctionalcod_ecran.ItemIndex];
        end;
      end
      else Result := '<Anulat>';
    finally
      Free;
    end;
end;


function NewSelectarePlanEconomic(var aCodEconomic : String; var aIdProiect : Integer;  const aCodFunctional : String = ''; const aIdUnitate : Integer = -1; const NumaiFrunze : Boolean = False; AOwner: TComponent = nil) : String;
var
  frmPlanBugete : TfrmSelBuget;
  aNode : TcxTreeListNode;
  aId : Integer;
begin
  frmPlanBugete := TfrmSelBuget.Create(AOwner);
  Result := '';
  with frmPlanBugete do
    try
      FOnlyChild := NumaiFrunze;
      Caption := 'Selectie cod economic';
      cxtabFunctional.TabVisible := False;
      cxtabEconomic.TabVisible := False;
      PaginaClasificatii.ActivePageIndex := 1;
      cxTreeEconomic.PopupMenu := nil;
      if qryPlanEconomic.Active then qryPlanEconomic.Close;
      if aCodFunctional = '' then
        qryPlanEconomic.Params.ParamByName('codFunctional').Value := Null
      else
        qryPlanEconomic.Params.ParamByName('codFunctional').Value := aCodFunctional;
      if aIdUnitate = -1 then
        qryPlanEconomic.Params.ParamByName('idUnitate').Value := null
      else
        qryPlanEconomic.Params.ParamByName('idUnitate').Value := aIdUnitate;
      qryPlanEconomic.Open;
      if Trim(aCodEconomic) <> '' then begin
        aId := -1;
        if aIdUnitate > 0 then begin
          if qryPlanEconomic.Locate('COD_ECONOMIC;ID_OI_PROIECTE', VarArrayOf([aCodEconomic, aIdProiect]), []) then
            aId := qryPlanEconomic.FieldByName('ID_BG_PLAN_ECONOMIC').AsInteger;
        end
        else
          if qryPlanEconomic.Locate('COD_ECONOMIC', aCodFunctional, []) then
            aId := qryPlanEconomic.FieldByName('ID_BG_PLAN_ECONOMIC').AsInteger;
        aNode := cxTreeEconomic.FindNodeByKeyValue(aId, nil);
      end
      else
        aNode := cxTreeEconomic.TopNode;

      if aNode <> nil then begin
        aNode.MakeVisible;
        aNode.Focused := True;
      end;
      ActiveControl := cxTreeEconomic;
      Position := poOwnerFormCenter;
      ShowModal;
      if ModalResult = mrOk then begin
        aNode := cxTreeEconomic.FocusedNode;
        if (NumaiFrunze) and (aNode <> nil) and (aNode.HasChildren) then raise EContaHandledError.Create('Codul Functional selectat trebuie sa fie un analitic ! Va rugam refaceti selectia !');
        if (aNode <> nil) then begin
          aCodEconomic := aNode.Texts[cxTreeEconomicCOD_ECONOMIC.ItemIndex];
          if aNode.Texts[cxTreeEconomicid_oi_proiecte.ItemIndex] <> '' then
            aIdProiect := aNode.Values[cxTreeEconomicid_oi_proiecte.ItemIndex]
          else
            aIdProiect := -1;
          Result := aNode.Texts[cxTreeEconomiccod_ecran.ItemIndex];
        end;
      end
      else Result := '<Anulat>';
    finally
      Free;
    end;
end;


procedure TfrmSelBuget.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  BtnOk.Left := BtnCancel.Left - BtnOk.Width - 6;
end;

procedure TfrmSelBuget.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeFunctionalcod_ecran.ItemIndex] + ' : ' +
    ANode.Texts[cxTreeFunctionalDENUMIRE.ItemIndex];
end;

procedure TfrmSelBuget.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeEconomiccod_ecran.ItemIndex] + ' : ' +
    ANode.Texts[cxTreeEconomicDENUMIRE.ItemIndex];
end;

procedure TfrmSelBuget.cxTreeFunctionalFocusedNodeChanged(Sender: TcxCustomTreeList;
  APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
  BtnOk.Enabled := Assigned(AFocusedNode) and (not FOnlyChild or not AFocusedNode.HasChildren);
end;

procedure TfrmSelBuget.cxTreeFunctionalDblClick(Sender: TObject);
begin
 if Assigned(Sender) and (Sender is TcxDBTreeList) then
  with TcxDBTreeList(Sender) do begin
    if (FocusedNode <> nil) and BtnOk.Enabled then
       BtnOk.Click;
  end;
end;

procedure TfrmSelBuget.cxTreeFunctionalKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    cxTreeFunctionalDblClick(Sender)
  else if Key = VK_ESCAPE then
   btnCancel.Click;
end;

end.

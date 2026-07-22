unit ContainerUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxTL, dxCntner, dxDBCtrl, Db, ZDataSet, StdCtrls, dxExEdtr,
  ImgList, dxDBTLCl, dxDBTL;

type
  TCrackATSDBTreeList = class(TdxDBTreeList);

  TfrmCasaContainer = class(TForm)
    TreeFunctional: TdxDBTreeList;
    Label1: TLabel;
    TreeEconomic: TdxDBTreeList;
    Label2: TLabel;
    TreeOrganigrama: TdxDBTreeList;
    Label3: TLabel;
    TreeCheltituitori: TdxDBTreeList;
    Label4: TLabel;
    TreeFunctionalID_BUGET_PLAN_FUNCTIONAL: TdxDBTreeListMaskColumn;
    TreeFunctionalCOD_BUGET: TdxDBTreeListMaskColumn;
    TreeFunctionalDENUMIRE: TdxDBTreeListMaskColumn;
    TreeFunctionalDESCRIERE: TdxDBTreeListMaskColumn;
    TreeFunctionalNUMAR_RAND: TdxDBTreeListMaskColumn;
    TreeFunctionalID_PARINTE: TdxDBTreeListMaskColumn;
    TreeFunctionalPLANIFICAT1: TdxDBTreeListMaskColumn;
    TreeFunctionalPLANIFICAT2: TdxDBTreeListMaskColumn;
    TreeFunctionalPLANIFICAT3: TdxDBTreeListMaskColumn;
    TreeFunctionalPLANIFICAT4: TdxDBTreeListMaskColumn;
    TreeFunctionalPLANIFICAT: TdxDBTreeListMaskColumn;
    TreeFunctionalCLASA: TdxDBTreeListMaskColumn;
    TreeEconomicID_BUGET_PLAN_ECONOMIC: TdxDBTreeListMaskColumn;
    TreeEconomicCOD_BUGET: TdxDBTreeListMaskColumn;
    TreeEconomicDENUMIRE: TdxDBTreeListMaskColumn;
    TreeEconomicDESCRIERE: TdxDBTreeListMaskColumn;
    TreeEconomicNUMAR_RAND: TdxDBTreeListMaskColumn;
    TreeEconomicID_PARINTE: TdxDBTreeListMaskColumn;
    TreeEconomicPLANIFICAT1: TdxDBTreeListMaskColumn;
    TreeEconomicPLANIFICAT2: TdxDBTreeListMaskColumn;
    TreeEconomicPLANIFICAT3: TdxDBTreeListMaskColumn;
    TreeEconomicPLANIFICAT4: TdxDBTreeListMaskColumn;
    TreeEconomicPLANIFICAT: TdxDBTreeListMaskColumn;
    TreeEconomicCLASA: TdxDBTreeListMaskColumn;
    TreeOrganigramaID_ORGANIGRAMA: TdxDBTreeListMaskColumn;
    TreeOrganigramaDENUMIRE: TdxDBTreeListMaskColumn;
    TreeOrganigramaID_PARINTE: TdxDBTreeListMaskColumn;
    TreeCheltituitoriID_REPARTITORI: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCODSECTIE: TdxDBTreeListMaskColumn;
    TreeCheltituitoriNUME: TdxDBTreeListMaskColumn;
    TreeCheltituitoriADRESA: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCONT: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCONT_CEC: TdxDBTreeListMaskColumn;
    TreeCheltituitoriBANCA: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCODCLASM: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCOD_FISCAL: TdxDBTreeListMaskColumn;
    TreeCheltituitoriREG_COMERT: TdxDBTreeListMaskColumn;
    TreeCheltituitoriID_TARI: TdxDBTreeListMaskColumn;
    TreeCheltituitoriID_JUDETE: TdxDBTreeListMaskColumn;
    TreeCheltituitoriTELEFON: TdxDBTreeListMaskColumn;
    TreeCheltituitoriFAX: TdxDBTreeListMaskColumn;
    TreeCheltituitoriEMAIL: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCOMERCIANT: TdxDBTreeListMaskColumn;
    TreeCheltituitoriGESTINT: TdxDBTreeListCheckColumn;
    TreeCheltituitoriCOTA_DISCOUNT: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCOTA_ADAOS: TdxDBTreeListMaskColumn;
    TreeCheltituitoriDATA_STOC_INI: TdxDBTreeListDateColumn;
    TreeCheltituitoriDATA_SOLD_INI: TdxDBTreeListDateColumn;
    TreeCheltituitoriSOLD_INITIAL: TdxDBTreeListMaskColumn;
    TreeCheltituitoriSNM: TdxDBTreeListMaskColumn;
    TreeCheltituitoriCONT_CRSP: TdxDBTreeListMaskColumn;
    TreeCheltituitoriPREFERAT: TdxDBTreeListCheckColumn;
    TreeCheltituitoriID_GEST_TIP_GEST: TdxDBTreeListMaskColumn;
    TreeCheltituitoriTIP_GESTIUNE: TdxDBTreeListMaskColumn;
    TreeCheltituitoriGRUP_LJ: TdxDBTreeListCheckColumn;
    TreeCheltituitoriID_UTILIZATORI: TdxDBTreeListMaskColumn;
    TreeCheltituitoriID_PARINTE: TdxDBTreeListMaskColumn;
    TreeCheltituitoriINORG: TdxDBTreeListImageColumn;
    CheckList: TImageList;
    TreeRepartitori: TdxDBTreeList;
    TreeRepartitoriID_REPARTITORI: TdxDBTreeListMaskColumn;
    TreeRepartitoriCONT: TdxDBTreeListMaskColumn;
    TreeRepartitoriNUME: TdxDBTreeListMaskColumn;
    TreeRepartitoriCODSECTIE: TdxDBTreeListMaskColumn;
    TreeRepartitoriADRESA: TdxDBTreeListMaskColumn;
    TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn;
    TreeRepartitoriTIPGEST: TdxDBTreeListMaskColumn;
    TreeTipDoc: TdxDBTreeList;
    TreeTipDocTIP_DOC: TdxDBTreeListMaskColumn;
    TreeTipDocDENUMIRE: TdxDBTreeListMaskColumn;
    TreeTipDocID_TIPURI_DOC: TdxDBTreeListMaskColumn;
    TreePlan: TdxDBTreeList;
    TreePlanCONT: TdxDBTreeListMaskColumn;
    TreePlanROMANA: TdxDBTreeListMaskColumn;
    TreePlanSID: TdxDBTreeListMaskColumn;
    TreePlanSIC: TdxDBTreeListMaskColumn;
    TreeOrdonantari: TdxDBTreeList;
    TreeOrdonantariNR_NOTA: TdxDBTreeListMaskColumn;
    TreeOrdonantariId_angajament: TdxDBTreeListMaskColumn;
    TreeOrdonantariID_UNIC_MODUL: TdxDBTreeListMaskColumn;
    TreeOrdonantariCont: TdxDBTreeListMaskColumn;
    TreeOrdonantariCont_Coresp: TdxDBTreeListMaskColumn;
    TreeOrdonantarinr_conex: TdxDBTreeListMaskColumn;
    TreeOrdonantariData_Scadenta: TdxDBTreeListDateColumn;
    TreeOrdonantariDocument: TdxDBTreeListMaskColumn;
    TreeOrdonantariData_Document: TdxDBTreeListDateColumn;
    TreeOrdonantariDocument_Detaliu: TdxDBTreeListMaskColumn;
    TreeOrdonantariPlata: TdxDBTreeListCheckColumn;
    TreeOrdonantariData: TdxDBTreeListDateColumn;
    TreeOrdonantariValoare: TdxDBTreeListCurrencyColumn;
    TreeOrdonantariExplicatie: TdxDBTreeListMaskColumn;
    TreeOrdonantaricod_functional: TdxDBTreeListMaskColumn;
    TreeOrdonantaricod_economic: TdxDBTreeListMaskColumn;
    TreeOrdonantariREST_PLATA: TdxDBTreeListCurrencyColumn;
    TreeOrdonantariColumn19: TdxDBTreeListColumn;
    TreeOrdonantariID_ALOP_ORDONANTARE: TdxDBTreeListMaskColumn;
    TreeListOrd: TdxDBTreeList;
    AtsDBTreeListMaskColumn1: TdxDBTreeListMaskColumn;
    AtsDBTreeListMaskColumn2: TdxDBTreeListMaskColumn;
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure TreeCheltituitoriMouseUp(Sender: TObject;
      Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
    procedure TreeCheltituitoriDblClick(Sender: TObject);
    procedure TreeCheltituitoriKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreeCheltituitoriGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeCheltituitoriCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure TreeListOrdDblClick(Sender: TObject);
  private
    { Private declarations }
  public

    { Public declarations }
    FacturiList, FunctionalList, EconomicList, OrganigrList, CheltuitoriList, OrdonantariList : TStringList;
    InKey : Boolean;
    function GetProjList(aTree : TdxDBTreeList) : TStringList;
    function GetProjIndex(aTree : TdxDBTreeList) : Integer;
    procedure PrepareData(aId : Integer);
    function GetNodeFullString(ANode : TdxTreeListNode; DbTreeList : TdxDbTreeList) : String;
  end;

var
  frmCasaContainer: TfrmCasaContainer;

implementation

uses DateUnit, CommonCasa, Variants;

{$R *.DFM}


procedure TfrmCasaContainer.FormCreate(Sender: TObject);
Var I : Integer;
begin
    FunctionalList  := TStringList.Create;
    FacturiList  := TStringList.Create;
    EconomicList    := TStringList.Create;
    OrganigrList    := TStringList.Create;
    CheltuitoriList := TStringList.Create;
    OrdonantariList := TStringList.Create;

    FunctionalList.Duplicates := dupIgnore;
    FacturiList.Duplicates := dupIgnore;
    EconomicList.Duplicates := dupIgnore;
    OrganigrList.Duplicates := dupIgnore;
    OrdonantariList.Duplicates := dupIgnore;
    CheltuitoriList.Duplicates := dupIgnore;

    TreeFunctional.SearchType := ModDeCautare;
    TreeEconomic.SearchType := ModDeCautare;
    TreeOrganigrama.SearchType := ModDeCautare;
    TreeCheltituitori.SearchType := ModDeCautare;
    TreeOrdonantari.SearchType := ModdeCautare;

    for I:= 0 to ComponentCount -1 do
      if Components[I] is TDataSet then
        TDataSet(Components[I]).Open;
end;

procedure TfrmCasaContainer.FormDestroy(Sender: TObject);
begin
  FunctionalList.Free;
  FacturiList.Free;
  EconomicList.Free;
  OrganigrList.Free;
  CheltuitoriList.Free;
  OrdonantariList.Free;
end;

function TfrmCasaContainer.GetProjList(aTree: TdxDBTreeList): TStringList;
begin
   if aTree = TreeFunctional then Result := FunctionalList
   else if aTree = TreeEconomic then Result := EconomicList
        else if aTree = TreeOrganigrama then Result := OrganigrList
        else if aTree = TreeOrdonantari then Result := OrdonantariList
             else Result := CheltuitoriList;
end;

procedure TfrmCasaContainer.TreeCheltituitoriMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var Info: TdxTreeListHitInfo;
    CurentState : Integer;
    CheckedProjList : TStringList;
    aNode : TdxTreeListNode;
    InternalIndex : Integer;

    procedure DeleteFormList(var aList : TStringList; aValue : String);
    var I:Integer;
    begin
      I:= aList.IndexOf(aValue);
      if I > -1 then aList.Delete(I);
    end;

    procedure PuneCopii(aNode: TdxTreeListNode; State: Integer);
    var J,I: Integer;
     begin
       I:= aNode.Values[InternalIndex];
       if (State = 0) or (aNode.HasChildren) then
          DeleteFormList(CheckedProjList, IntToStr(I))
       else begin
          CheckedProjList.Add(IntToStr(I));
       end;

       for J := 0 to aNode.Count-1 do begin
         aNode.Items[J].ImageIndex := State;
         PuneCopii(aNode.Items[J], State);
       end;
     end;

    procedure PuneParinti(aNode: TdxTreeListNode; State: Integer);
    var J,I : Integer;
    begin
      if not Assigned(aNode) then Exit;
      if State = 2 then begin
         aNode.ImageIndex := State;
         I:= aNode.Values[InternalIndex];
         if (State = 1) and not(aNode.HasChildren) then
           CheckedProjList.Add(IntToStr(I))
         else
           DeleteFormList(CheckedProjList, IntToStr(I));
         PuneParinti(aNode.Parent, 2);
      end
      else begin
        for J := 0 to aNode.Count-1 do
          if aNode.Items[J].ImageIndex <> State then begin
             State := 2;
             Break;
          end;
        I:= aNode.Values[InternalIndex] ;
        if (State = 1)  and not(aNode.HasChildren) then
           CheckedProjList.Add(IntToStr(I))
        else
           DeleteFormList(CheckedProjList, IntToStr(I));
        aNode.ImageIndex := State;
        PuneParinti(aNode.Parent, State)
      end;
    end;
begin
  CheckedProjList := GetProjList(TdxDBTreeList(Sender));
  InternalIndex := GetProjIndex(TdxDBTreeList(Sender));
  Info := TdxDBTreeList(Sender).GetHitInfo(Point(X,Y));
  aNode := nil;
  if InKey then aNode := TdxDBTreeList(Sender).FocusedNode;
  if not(InKey) then aNode := Info.Node;
  if aNode = nil then Exit;

  if (Info.hitType = htIcon) or InKey then begin
    if aNode.ImageIndex = 1 then begin
       aNode.ImageIndex := 0 ;
    end
    else begin
       aNode.ImageIndex := 1;
    end;
    CurentState := aNode.ImageIndex;
    PuneCopii(aNode, CurentState);
    PuneParinti(aNode.Parent, CurentState);
  end;
  InKey := False;
end;

procedure TfrmCasaContainer.TreeCheltituitoriDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then begin
      if Assigned(TdxDBTreeList(Sender).OnMouseUp) and (GetProjList(TdxDBTreeList(Sender)).Count = 0) then begin
         InKey := True;
         TreeCheltituitoriMouseUp(Sender, mbLeft, [], 0, 0);
       end;
      (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
      if TdxDBTreeList(Sender) = TreeOrganigrama then
        PrepareData(FocusedNode.Values[TreeOrganigramaID_ORGANIGRAMA.Index]);
    end;
end;

procedure TfrmCasaContainer.TreeCheltituitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if Assigned(TdxDBTreeList(Sender).OnMouseUp) and (Key = VK_SPACE) then begin
    InKey := True;
    TreeCheltituitoriMouseUp(Sender, mbLeft, Shift, 0, 0);
  end;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
  begin
     if Assigned(TdxDBTreeList(Sender).OnMouseUp) and  (GetProjList(TdxDBTreeList(Sender)).Count = 0) then begin
       InKey := True;
       TreeCheltituitoriMouseUp(Sender, mbLeft, Shift, 0, 0);
     end;
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
     if TdxDBTreeList(Sender) = TreeOrganigrama then PrepareData(TreeOrganigrama.FocusedNode.Values[TreeOrganigramaID_ORGANIGRAMA.Index]);
  end;
end;

procedure TfrmCasaContainer.PrepareData(aId: Integer);
begin
  with FrmData.QryCasaSalariati do begin
    Close;
    Params.ParamByName('ID_ORGANIGRAMA').Value := aId;
    Open;
  end;
end;

procedure TfrmCasaContainer.TreeCheltituitoriGetSelectedIndex(
  Sender: TObject; Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TfrmCasaContainer.TreeCheltituitoriCustomDrawCell(
  Sender: TObject; ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
begin
  if aNode.Strings[TreeCheltituitoriINORG.Index] = '1' then aColor := clAqua;
end;



function TfrmCasaContainer.GetProjIndex(aTree: TdxDBTreeList): Integer;
begin
  if aTree = TreeFunctional then Result := TreeFunctionalID_BUGET_PLAN_FUNCTIONAL.Index
  else if aTree = TreeEconomic then Result := TreeEconomicID_BUGET_PLAN_ECONOMIC.Index
       else if aTree = TreeOrganigrama then Result := TreeOrganigramaID_ORGANIGRAMA.Index
            else Result := TreeCheltituitoriID_REPARTITORI.Index;
end;

function TfrmCasaContainer.GetNodeFullString(ANode: TdxTreeListNode;
  DbTreeList: TdxDbTreeList): String;
begin
  Result := '';

end;

procedure TfrmCasaContainer.TreeListOrdDblClick(Sender: TObject);
begin
   (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

end.

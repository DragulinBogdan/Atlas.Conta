unit ImportCasaUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, StdCtrls, Buttons, Db, ZDataSet, dxTL,
  dxCntner, dxDBCtrl, ImgList, dxEditor, dxExEdtr, dxEdLib, dxDBTLCl,
  dxDBTL,
  ZAbstractRODataset, ZAbstractDataset;

type
  TfrmImportCasa = class(TForm)
    pnLeft: TPanel;
    Splitter1: TSplitter;
    pnRest: TPanel;
    pnBottom: TPanel;
    btnOk: TBitBtn;
    btnCancel: TBitBtn;
    QryDecont: TZQuery;
    DTDecont: TDataSource;
    GridDecont: TdxDBTreeList;
    GridDecontCOD_CB: TdxDBTreeListImageColumn;
    GridDecontCOD: TdxDBTreeListMaskColumn;
    GridDecontCODGEST: TdxDBTreeListImageColumn;
    GridDecontDATA: TdxDBTreeListDateColumn;
    GridDecontTIPDOC: TdxDBTreeListMaskColumn;
    GridDecontNRDOC: TdxDBTreeListMaskColumn;
    GridDecontPOZ: TdxDBTreeListMaskColumn;
    GridDecontEXPLICATIE: TdxDBTreeListMaskColumn;
    GridDecontINCASARI: TdxDBTreeListMaskColumn;
    GridDecontPLATI: TdxDBTreeListMaskColumn;
    GridDecontSOLD: TdxDBTreeListMaskColumn;
    GridDecontCONT_CSP: TdxDBTreeListMaskColumn;
    GridDecontVAL_CRSP: TdxDBTreeListMaskColumn;
    GridDecontACHITAT: TdxDBTreeListMaskColumn;
    GridDecontDATAEM: TdxDBTreeListDateColumn;
    GridDecontC_O: TdxDBTreeListMaskColumn;
    GridDecontNR_LIST: TdxDBTreeListMaskColumn;
    GridDecontMEXPLIC: TdxDBTreeListMemoColumn;
    GridDecontCURS_SCHIM: TdxDBTreeListMaskColumn;
    GridDecontSOLD_INITIAL: TdxDBTreeListMaskColumn;
    GridDecontCOD_ARHIVA: TdxDBTreeListMaskColumn;
    GridDecontECL: TdxDBTreeListMaskColumn;
    GridDecontVALIDATA: TdxDBTreeListMaskColumn;
    GridDecontTRANSFER: TdxDBTreeListMaskColumn;
    GridDecontCOD_CBT: TdxDBTreeListMaskColumn;
    GridDecontCOD_TRANSFER: TdxDBTreeListMaskColumn;
    GridDecontNR_DECONT: TdxDBTreeListMaskColumn;
    GridDecontDATA_DECONT: TdxDBTreeListDateColumn;
    GridDecontPARENT_COD: TdxDBTreeListMaskColumn;
    CheckList: TImageList;
    GridDecontNUME_REPARTITOR: TdxDBTreeListMaskColumn;
    GridDecontNUME_CASA: TdxDBTreeListMaskColumn;
    edtHouse: TdxImageEdit;
    chkFiltCasa: TCheckBox;
    ChkRep: TCheckBox;
    edtRepartitor: TdxPopupEdit;
    edtPlati: TdxImageEdit;
    chkTipPlata: TCheckBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    lbHouse: TLabel;
    lbRep: TLabel;
    lbTipIntrare: TLabel;
    lbPlati: TLabel;
    lbIncasari: TLabel;
    lbSold: TdxCurrencyEdit;
    Bevel1: TBevel;
    procedure GridDecontGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure GridDecontMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure GridDecontKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure chkFiltCasaClick(Sender: TObject);
    procedure edtHouseChange(Sender: TObject);
    procedure edtPlatiChange(Sender: TObject);
    procedure edtRepartitorCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
  private
    FIsDecont: Integer;
    procedure SetDecont(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    InKey : Boolean;
    ImportList : TStringList;
    IndexRepartitor : Integer;
    procedure RefreshDataSet;
    procedure RefreshLabels;
    property IsDecont : Integer read FIsDecont write SetDecont;
  end;

var
  frmImportCasa: TfrmImportCasa;

implementation

uses DateUnit, Variants, CommonCasa;

{$R *.DFM}

procedure TfrmImportCasa.GridDecontGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TfrmImportCasa.GridDecontMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var Info: TdxTreeListHitInfo;
    CurentState : Integer;
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
          DeleteFormList(ImportList, IntToStr(I))
       else begin
          ImportList.Add(IntToStr(I));
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
           ImportList.Add(IntToStr(I))
         else
           DeleteFormList(ImportList, IntToStr(I));
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
           ImportList.Add(IntToStr(I))
        else
           DeleteFormList(ImportList, IntToStr(I));
        aNode.ImageIndex := State;
        PuneParinti(aNode.Parent, State)
      end;
    end;
begin
  InternalIndex := GridDecontCOD.Index;
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
  RefreshLabels;
end;

procedure TfrmImportCasa.GridDecontKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if Key = VK_SPACE then begin
    InKey := True;
    GridDecontMouseUp(Sender, mbLeft, Shift, 0, 0);
  end;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then begin
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
     //if TdxDBTreeList(Sender) = TreeOrganigrama then PrepareData(TreeOrganigrama.FocusedNode.Values[TreeOrganigramaID_ORGANIGRAMA.Index]);
  end;
end;

procedure TfrmImportCasa.FormCreate(Sender: TObject);
begin
  ImportList := TStringList.Create;
end;

procedure TfrmImportCasa.FormDestroy(Sender: TObject);
begin
  ImportList.Free;
end;

procedure TfrmImportCasa.chkFiltCasaClick(Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmImportCasa.RefreshDataSet;
begin
  with QryDecont do begin
    Close;
    if (ChkRep.Checked) and (edtRepartitor.Tag > 0) then
      Params.ParamByName('CODGEST').Value := edtRepartitor.Tag
    else
      Params.ParamByName('CODGEST').Value := Null;

    //1 - Plata 0--Incasare 2 --ambele
    if (Trim(edtPlati.Text)<>'') and (chkTipPlata.Checked) then 
      Params.ParamByName('IS_PLATA').Value := edtPlati.Text
    else
      Params.ParamByName('IS_PLATA').Value := 2;

    if (chkFiltCasa.Checked) and (Trim(edtHouse.Text) <> '') then begin
       Params.ParamByName('COD_CB').Value := edtHouse.Text;
       Params.ParamByName('IS_DECONT').Value :=0;
    end
    else begin
      Params.ParamByName('COD_CB').Value := Null;
      //1 - casa decont 2--casa normala 0 - ambele
      Params.ParamByName('IS_DECONT').Value := FIsDecont;
    end;
    Open;
    RefreshLabels;
  end;
end;

procedure TfrmImportCasa.SetDecont(const Value: Integer);
var I: Integer;
begin
  FIsDecont := Value;
  for I := edtHouse.Values.Count -1 downto 0 do
    if ((FIsDecont <> 0) and (TTipCasa(edtHouse.Values.Objects[I]).IsAvans <> Boolean(FIsDecont mod 2)))
    or (not(TTipCasa(edtHouse.Values.Objects[I]).IsTempor)) then begin
      edtHouse.Descriptions.Delete(I);
      edtHouse.Values.Delete(I);
    end;
end;

procedure TfrmImportCasa.edtHouseChange(Sender: TObject);
begin
  chkFiltCasa.Checked := True;
  RefreshDataSet;  
end;

procedure TfrmImportCasa.edtPlatiChange(Sender: TObject);
begin
  chkTipPlata.Checked := True;
  RefreshDataSet;
end;

procedure TfrmImportCasa.edtRepartitorCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var aNode : TdxDBTreeListNode;
begin
  if Accept then begin
     with TdxPopupEdit(Sender) do
       aNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(aNode) then begin
          Text := IntToStr(aNode.Id) + ':' + Trim(aNode.Strings[IndexRepartitor]);
          TdxPopupEdit(Sender).Text := Text;
          TdxPopupEdit(Sender).Tag  := aNode.Id;
       end;
    end;
  ChkRep.Checked := Accept;
  RefreshDataSet;
end;

procedure TfrmImportCasa.RefreshLabels;
var I : Integer;
    Plati, Incasari: Currency;


  procedure GetAllPlati(aNode : TdxTreeListNode);
  var  J : Integer;
  begin
    if not aNode.HasChildren then begin
      if aNode.ImageIndex = 1 then begin
        Plati := Plati + (-1)* GetAsCurrency(aNode, GridDecontPLATI.Index);
        Incasari := Incasari + GetAsCurrency(aNode, GridDecontINCASARI.Index);
      end;
    end
    else
      for J := 0 to aNode.Count - 1 do
        GetAllPlati(aNode.Items[J])
  end;


begin
 if (chkFiltCasa.Checked) and (Trim(edtHouse.Text)<> '') then
   lbHouse.Caption := edtHouse.Descriptions.Strings[edtHouse.Values.IndexOf(edtHouse.Text)]
 else
   lbHouse.Caption := 'Toate';

 if (ChkRep.Checked) and (edtRepartitor.Tag>0) then
   lbRep.Caption := edtRepartitor.Text
 else
   lbRep.Caption := 'Toti Repartitorii';

 if (chkTipPlata.Checked) and (Trim(edtPlati.Text) <> '') then
   lbTipIntrare.Caption := edtPlati.Descriptions.Strings[edtPlati.Values.IndexOf(edtPlati.Text)]
 else
   lbTipIntrare.Caption := 'Incasari/Plati';

 Plati := 0;
 Incasari := 0;
 for I :=  0 to GridDecont.Count - 1 do
    GetAllPlati(GridDecont.Items[I]);

 lbPlati.Caption := FormatFloat(CurrFormat, Plati);
 lbIncasari.Caption := FormatFloat(CurrFormat, Incasari);
 lbSold.Value := Incasari+Plati;
end;

end.
  

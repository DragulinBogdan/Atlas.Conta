unit ConflictUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxCntner, dxTL, dxDBCtrl, dxDBGrid, ExtCtrls, HeadPanel, Db, Variants,
  dxmdaset, ZDataSet, dxGrClms, StdCtrls, Buttons, CommonCasa, dxExEdtr,
  ImgList, dxDBTLCl, unitMemTableEx;

type
  TfrmConflict = class(TForm)
    Info: THeadPanel;
    GridConflict: TdxDBGrid;
    DTConflict: TDataSource;
    MemConflict: TdxMemData;
    GridConflictRecId: TdxDBGridColumn;
    GridConflictID_LISTA: TdxDBGridMaskColumn;
    GridConflictID_PARINTE: TdxDBGridMaskColumn;
    GridConflictCOD_CB: TdxDBGridMaskColumn;
    GridConflictCOD: TdxDBGridMaskColumn;
    GridConflictCODGEST: TdxDBGridMaskColumn;
    GridConflictDATA: TdxDBGridDateColumn;
    GridConflictTIPDOC: TdxDBGridMaskColumn;
    GridConflictNRDOC: TdxDBGridMaskColumn;
    GridConflictPOZ: TdxDBGridMaskColumn;
    GridConflictEXPLICATIE: TdxDBGridMaskColumn;
    GridConflictINCASARI: TdxDBGridMaskColumn;
    GridConflictPLATI: TdxDBGridMaskColumn;
    GridConflictSOLD: TdxDBGridMaskColumn;
    GridConflictCONT_CSP: TdxDBGridMaskColumn;
    GridConflictVAL_CRSP: TdxDBGridMaskColumn;
    GridConflictACHITAT: TdxDBGridMaskColumn;
    GridConflictDATAEM: TdxDBGridDateColumn;
    GridConflictC_O: TdxDBGridMaskColumn;
    GridConflictNR_LIST: TdxDBGridMaskColumn;
    GridConflictMEXPLIC: TdxDBGridMemoColumn;
    GridConflictCURS_SCHIM: TdxDBGridMaskColumn;
    GridConflictECL: TdxDBGridMaskColumn;
    GridConflictON_SERVER: TdxDBGridMaskColumn;
    GridConflictKEEP: TdxDBGridCheckColumn;

    MemConflictID_LISTA: TStringField;
    MemConflictID_PARINTE: TStringField;
    MemConflictCOD_CB: TIntegerField;
    MemConflictCOD: TIntegerField;
    MemConflictCODGEST: TStringField;
    MemConflictDATA: TDateTimeField;
    MemConflictTIPDOC: TStringField;
    MemConflictNRDOC: TStringField;
    MemConflictPOZ: TIntegerField;
    MemConflictEXPLICATIE: TStringField;
    MemConflictINCASARI: TBCDField;
    MemConflictPLATI: TBCDField;
    MemConflictSOLD: TBCDField;
    MemConflictCONT_CSP: TStringField;
    MemConflictVAL_CRSP: TBCDField;
    MemConflictACHITAT: TBCDField;
    MemConflictDATAEM: TDateTimeField;
    MemConflictC_O: TIntegerField;
    MemConflictNR_LIST: TIntegerField;
    MemConflictCURS_SCHIM: TBCDField;
    MemConflictECL: TWordField;
    MemConflictON_SERVER: TIntegerField;
    MemConflictSOLD_NOU: TCurrencyField;
    MemConflictID_PROIECT: TIntegerField;
    MemConflictPROIECT: TStringField;
    MemConflictPEXPLIC: TMemoField;
    MemConflictMEXPLIC: TMemoField;
    MemConflictVALIDATA: TWordField;
    MemConflictTRANSFER: TIntegerField;
    MemConflictCOD_CBT: TIntegerField;
    MemConflictCOD_TRANSFER: TIntegerField;
    MemConflictDATA_ACCEPT: TDateTimeField;
    MemConflictTIP_CHELTVEN: TIntegerField;
    MemConflictKEEP: TBooleanField;
    MemConflictOLD_ID: TIntegerField;
    btnOk: TBitBtn;
    GridConflictOLD_ID: TdxDBGridMaskColumn;
    Imagini: TImageList;
    StareImg: TImageList;
    procedure GridConflictChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridConflictCustomDraw(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxDBTreeListColumn;
      const AText: String; AFont: TFont; var AColor: TColor; ASelected,
      AFocused: Boolean; var ADone: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure GridConflictOLD_IDValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure GridConflictCODGESTGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridConflictKEEPToggleClick(Sender: TObject;
      const Text: String; State: TdxCheckBoxState);
    procedure GridConflictC_OGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
  private
    FDestData: TdxMemData;
    FStartDate: TDate;
    FEndDate: TDate;
    FCurentHouse: Integer;
    FTipLista: TTipLista;
    FOldId : String;
    { Private declarations }
  protected
    //procedura reexecuta procedura pentru conflict si aduce rezultatul in fdestdata
    procedure BringDataOnLine;
    //aduga in functie de datele de pe client corespondentele din datasetul operat offline
    procedure AddOffline(aMemData : TdxMemData);
    //copie inregistrarea curenta din aStartDatabase in aDestDataBase
    procedure CopyDataLineTo(aStartData : TdxMemData; var aDestData : TdxMemData);
    //filtram inregistrariile care sunt selectate
    procedure FilterMemConflict(DataSet: TDataSet; var Accept: Boolean);
  public
    { Public declarations }
    CurrentNode : TdxTreeListNode;
    //se configureaza gridul
    procedure BringOnScreen;
    procedure ResolvaConflict(var aDataSet : TdxMemData);
    //se configureaza datasetul pentru rezolvarea conflictului
    procedure BringDataToScreen(aDataSet : TdxMemData);
    property  DestData        : TdxMemData read FDestData write FDestData;
    property  StartDate       : TDate read FStartDate write FStartDate;
    property  EndDate         : TDate read FEndDate write FEndDate;
    property  CurentHouse     : Integer read FCurentHouse write FCurentHouse;
    property  TipLista        : TTipLista read FTipLista write FTipLista;
  end;


function HandleConflict(var aDataSet : TdxMemData; Data1, Data2:TDate; CasaCurenta : Integer; Defalcare : TTipLista) : Boolean;


implementation
{$R *.DFM}

uses
  Math, ZeosDBUtile, CommonDBVar, DateUnit;

{ TfrmConflict }

procedure TfrmConflict.AddOffline(aMemData: TdxMemData);
var ID :Integer;
begin
  if not(MemConflict.Active) then MemConflict.Active := True;
  FDestData.First;
  while not(FDestData.Eof) do begin
    if FDestData.FieldByName('OLD_ID').AsInteger > 0 then begin
      ID := FDestData.FieldByName('OLD_ID').AsInteger;
      if aMemData.Locate('COD', ID, []) then begin
         CopyDataLineTo(aMemData, MemConflict);
         if not(MemConflict.State in [dsEdit, dsInsert]) then MemConflict.Edit;
         MemConflict.FieldByName('OLD_ID').AsInteger := ID;
         MemConflict.FieldByName('KEEP').AsBoolean := True;
         MemConflict.Post;
      end;
    end;
    FDestData.Next;
  end;
  DBLoadFromDataSet(MemConflict, FDestData, False);
end;

procedure TfrmConflict.BringDataOnLine;
var
  lDataSet  : TDataSet;
  lProcName : String;
begin
  //aducem date cu care rezolvam conflictu'
  FDestData.DisableControls;
  if FDestData.Active then FDestData.Active := False;
  if not(FDestData.Active) then FDestData.Active := True;
  lProcName := GetExec(FTipLista);
  lDataSet  := DBNewQueryFmt('exec %s %d, %s, %s, %d, %d',
    [
      lProcName,
      FCurentHouse,
      ValueDateToStr(FStartDate),
      ValueDateToStr(FEndDate),
      IdUtilizator,
      1]);
  try
    lDataSet.Open;
    DBLoadFromDataSet(FDestData, lDataSet, False);
  finally
    lDataSet.Free;
  end;
  FDestData.EnableControls;
end;

procedure TfrmConflict.BringDataToScreen(aDataSet: TdxMemData);
begin
  //reexecutam interogarea care va aduce inregistrariile diferite fata de setul initial luat
  BringDataOnLine;
  //adaugam forma de acum a acestor difernte
  AddOffline(aDataSet);
  //setam nodul curent
  CurrentNode := GridConflict.TopNode;
end;

procedure TfrmConflict.BringOnScreen;
begin
//
end;

procedure TfrmConflict.CopyDataLineTo(aStartData: TdxMemData;
  var aDestData: TdxMemData);
var I:Integer;
   aField : TField;
begin
  aDestData.Append;
  for I:=0 to aStartData.FieldCount -1 do begin
    aField := aDestData.FindField(aStartData.Fields[I].FieldName);
    if Assigned(aField) then aField.AsVariant := aStartData.Fields[I].Value;
  end;
  aDestData.Post;
end;

procedure TfrmConflict.ResolvaConflict(var aDataSet : TdxMemData);
var ID, CurentCod : Integer;
    CurentID_LISTA, CurentID_PARINTE : String;
begin
  if MemConflict.State in [dsEdit, dsInsert] then MemConflict.Post; 
  if not(aDataSet.Active) then Exit;

  if FDestData.Active then FDestData.Active := False;
  FDestData.Fields.Clear;
  FDestData.Active := True;
  MemConflict.OnFilterRecord := FilterMemConflict;
  FDestData.LoadFromDataSet(MemConflict);

  FDestData.First;
  while not(FDestData.Eof) do begin
    if FDestData.FieldByName('OLD_ID').AsInteger > 0 then begin
      ID := FDestData.FieldByName('OLD_ID').AsInteger;
      if aDataSet.Locate('COD', ID, []) then begin
          CurentCod := aDataSet.FieldByName('COD').AsInteger;
          CurentID_LISTA := aDataSet.FieldByName('ID_LISTA').AsString;
          CurentID_PARINTE := aDataSet.FieldByName('ID_PARINTE').AsString;
          aDataSet.Delete;
          if not(FDestData.State in [dsEdit, dsInsert]) then FDestData.Edit;
          FDestData.FieldByName('COD').AsInteger := CurentCod;
          FDestData.FieldByName('ID_LISTA').AsString := CurentID_LISTA;
          FDestData.FieldByName('ID_PARINTE').AsString := CurentID_PARINTE;
          FDestData.Post;
      end;
      if not(FDestData.State in [dsEdit, dsInsert]) then FDestData.Edit;
      FDestData.FieldByName('ON_SERVER').AsInteger := 0;
      FDestData.Post;
    end;
    FDestData.Next;
  end;

  DBLoadFromDataSet(aDataSet, FDestData, False);
  MemConflict.OnFilterRecord := nil;
end;


function HandleConflict(var aDataSet : TdxMemData; Data1, Data2:TDate; CasaCurenta : Integer; Defalcare : TTipLista) : Boolean;
var FrmConflict : TfrmConflict;
begin
  FrmConflict := TfrmConflict.Create(nil);
  with FrmConflict do
    try
      StartDate := Data1;
      EndDate   := Data2;
      CurentHouse := CasaCurenta;
      TipLista := Defalcare;
      BringDataToScreen(aDataSet);
      ShowModal;
      Result := (ModalResult = mrOk);
      if Result then
        ResolvaConflict(aDataSet);
    finally
      Free;
    end;
end;

procedure TfrmConflict.GridConflictKEEPToggleClick(Sender: TObject;
  const Text: String; State: TdxCheckBoxState);
var I:Integer;
begin
  if Not Assigned(CurrentNode) then Exit;
  if (State = cbsChecked) then
     for I:= 0 to GridConflict.Count - 1 do  begin
       if GridConflict.Items[I].Values[GridConflictOLD_ID.Index] = CurrentNode.Values[GridConflictOLD_ID.Index] then
          GridConflict.Items[I].Strings[GridConflictKEEP.Index] := 'False';
     end;
end;

procedure TfrmConflict.GridConflictChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  CurrentNode := Node;
  FOldId := Node.Strings[GridConflictOLD_ID.Index];
  GridConflict.Invalidate;
end;

procedure TfrmConflict.GridConflictCustomDraw(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxDBTreeListColumn; const AText: String; AFont: TFont;
  var AColor: TColor; ASelected, AFocused: Boolean; var ADone: Boolean);
begin
  if not Assigned(CurrentNode) then  AColor := clWindow
  else
     if (ANode.Strings[GridConflictOLD_ID.Index] = FOldId) then
        AColor := clAqua;
end;

procedure TfrmConflict.FormCreate(Sender: TObject);
begin
  FDestData := TdxMemData.Create(Self);
end;

procedure TfrmConflict.FormDestroy(Sender: TObject);
begin
  FDestData.Destroy;
end;

procedure TfrmConflict.GridConflictOLD_IDValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
//var aNode : TdxTreeListNode;

procedure SetRestValues(aOldId : Integer; Node : TdxTreeListNode);
var I : Integer;
begin
  for I:= 0 to GridConflict.Count -1 do
    if (GridConflict.Items[I] <> Node) and (GridConflict.Items[I].Values[GridConflictOLD_ID.Index] = aOldId) then
       GridConflict.Items[I].Strings[GridConflictKEEP.Index] := 'False';
end;

begin
{  if not Assigned(GridConflict.FocusedNode) then Exit;
  aNode := GridConflict.FocusedNode;
  with (Sender as TdxTreeListNode) do begin
     SetRestValues(aNode.Values[GridConflictOLD_ID.Index], aNode);
  end;
 }
end;

procedure TfrmConflict.FilterMemConflict(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := DataSet.FieldByName('KEEP').AsBoolean;
end;

procedure TfrmConflict.GridConflictCODGESTGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  if AText <> '' then begin
     with FrmData.QryRepartitori do
     if Locate('ID_REPARTITORI', aText, []) then
         AText := Trim(FieldByName('CODSECTIE').AsString)+' : '+Trim(FieldByName('NUME').AsString);
  end;
end;

procedure TfrmConflict.GridConflictC_OGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  if AText <> '' then begin
     with FrmData.QryOperatori do
     if Locate('ID_UTILIZATORI', aText, []) then
         AText := Trim(FieldByName('NUME').AsString)+'('+Trim(FieldByName('NUMEINTREG').AsString)+')';
  end;
end;

end.

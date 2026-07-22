unit ReconciliereDecontariUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs, dxDBGrid,
  dxCntner, dxTL, dxDBCtrl, ExtCtrls, dxTLClms, ImgList, StdCtrls,
  dxExEdtr, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels;

type
  TFrmReconcilereDecontari = class(TForm)
    pnBottom: TPanel;
    pnClient: TPanel;
    ListaDecontari: TdxTreeList;
    ListaDecontariCOD_DOCUM: TdxTreeListColumn;
    ListaDecontariNR_DOCUM: TdxTreeListColumn;
    ListaDecontariDATA_DOCUM: TdxTreeListColumn;
    ListaDecontariPREDATOR: TdxTreeListColumn;
    ListaDecontariPRIMITOR: TdxTreeListColumn;
    ListaDecontariSUMA: TdxTreeListCurrencyColumn;
    ListaDecontariTIPDOC: TdxTreeListColumn;
    ListaDecontariNRDOC: TdxTreeListColumn;
    ListaDecontariDATA: TdxTreeListColumn;
    ListaDecontariEXPLIC: TdxTreeListColumn;
    Imagini: TImageList;
    StareImg: TImageList;
    BtnOk: TcxButton;
    procedure BtnOkClick(Sender: TObject);
    procedure ListaDecontariGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure ListaDecontariMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure ListaDecontariChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
  private
    { Private declarations }
    { Realizeaza salvarea in baza de date }
    procedure AplicaModificari;
  public
    { Public declarations }
    { Salvam formul de decontari de care apartine reconcilierea curenta }
    Decontari : TCustomForm;
    { Initializam lista de documente ce urmeaza a fi reconciliate }
    procedure SetDecontari;
  end;

{ Functia intoarce noul id_gest_docum (cel final) in cazul in care
  a fost modificat succesiv }
function ParseAndGetNewIdDoc(AID: String): Integer;

implementation

{$R *.DFM}

uses
  ZDataSet, ZeosDBUtile, Db, DecontariUnit, DateUnit;

function ParseAndGetNewIdDoc(AID: String): Integer;
var
  lDataSet: TDataSet;
begin
  Result := -1;
  lDataSet := DBNewQuery('SELECT ID_GEST_DOCUM, STARE FROM GEST_DOCUM WHERE ID_MODIFICARE = :AID'#13#10+
                         // Tinem cont sa nu fie pozitia pe nou adaugata ID_MODIFICARE <> ID_GEST_DOCUM
                         // Tinem cont sa fie documentul de baza ID_INITIAL = ID_GEST_DOCUM
                         'AND ID_MODIFICARE <> ID_GEST_DOCUM AND ID_INITIAL = ID_GEST_DOCUM');
  try
    DBRefreshParamsEx(lDataSet, [AID], True);
    while not lDataSet.IsEmpty and (lDataSet.Fields[1].AsInteger <> 1) do begin
      DBRefreshParamsEx(lDataSet, [lDataSet.Fields[0].AsInteger], True);
    end;
    if not lDataSet.IsEmpty then
      Result := lDataSet.Fields[0].AsInteger;
  finally
    lDataSet.Free;
  end;
end;

function GetNewCasaId(AID: String): Integer;
begin
  Result := ValueSafeToInt( DBGetScallar('SELECT ISNULL(NR_LIST, COD) AS COD FROM BREGISTRU WHERE COD = .DBO.FN_CURENT_COD('+AID+')') );
end;

{ TFrmReconcilereDecontari }

procedure TFrmReconcilereDecontari.SetDecontari;
var FDecont : TfrmDecontari;
    I: Integer;
    lNewDocId: Integer;
    lNewCasaId: Integer;
    lDecId: Integer;
    lDocPatern: String;
    lWhere: String;
    lDocId: String;
    lCasaId: String;
    lNode: TdxDBGridNode;
    lDestNode: TdxTreeListNode;
    lDupList: TStringList;

    function NewCopyNode: TdxTreeListNode;
     begin
       Result := ListaDecontari.Add;
       { Descrierea imperecherii }
        with Result, FDecont do begin
          Data := Pointer(lDecId);
          StateIndex := 1;
          ImageIndex := 0;
          Strings[ListaDecontariCOD_DOCUM.Index]  := lNode.Strings[GridImperecheriCOD_DOCUM.Index];
          Strings[ListaDecontariNR_DOCUM.Index]   := lNode.Strings[GridImperecheriNR_DOCUM.Index];
          Strings[ListaDecontariDATA_DOCUM.Index] := lNode.Strings[GridImperecheriDATA_DOCUM.Index];
          Strings[ListaDecontariPREDATOR.Index]   := lNode.Strings[GridImperecheriPREDATOR.Index];
          Strings[ListaDecontariPRIMITOR.Index]   := lNode.Strings[GridImperecheriPRIMITOR.Index];
          Strings[ListaDecontariTIPDOC.Index]     := lNode.Strings[GridImperecheriTIPDOC.Index];
          Strings[ListaDecontariNRDOC.Index]      := lNode.Strings[GridImperecheriNRDOC.Index];
          Strings[ListaDecontariDATA.Index]       := lNode.Strings[GridImperecheriDATA.Index];
          Strings[ListaDecontariEXPLIC.Index]     := lNode.Strings[GridImperecheriEXPLICATIE.Index];
          Values[ListaDecontariSUMA.Index]        := lNode.Values[GridImperecheriSUMA.Index];
        end;
     end;

begin
  ListaDecontari.ClearNodes;
  FDecont := TfrmDecontari(Decontari);
  if FDecont = nil then Exit;
  { Eliminam si dublurile nu stiu eu cat de ortodox este dar .... }
  lDupList := TStringList.Create;
  try
      lDupList.Sorted := True;
      lDupList.Duplicates := dupIgnore;
      { Intai luam lista de duplicate astfel incat sa putem elimina eventualele decontari busite care
        sunt in dublu si cu o versiune noua }
      with FDecont do
        for I := 0 to GridImperecheri.Count-1 do begin
          lNode  := TdxDBGridNode(GridImperecheri.Items[I]);
          if lNode.Strings[GridImperecheriSTARE.Index] = '1' then begin
            lDecId := lNode.Id;
            lDocId := Trim(lNode.Strings[GridImperecheriID_GEST_DOCUM.Index]);
            lCasaId := Trim(lNode.Strings[GridImperecheriID_BREGISTRU.Index]);
            lDocPatern := Format('%20.20s - %20.20s', [lDocId, lCasaId]);
            if lDupList.IndexOf(lDocPatern) > -1 then NewCopyNode
            else lDupList.Add(lDocPatern);
          end;
        end;
      { Initializam toate documentele care urmeaza sa fie reconciliate }
      with FDecont do begin
        for I := 0 to GridImperecheri.Count-1 do begin
          lNode  := TdxDBGridNode(GridImperecheri.Items[I]);
          if lNode.Strings[GridImperecheriSTARE.Index] <> '1' then begin
            lDecId := lNode.Id;
            lDocId := Trim(lNode.Strings[GridImperecheriID_GEST_DOCUM.Index]);
            lCasaId := Trim(lNode.Strings[GridImperecheriID_BREGISTRU.Index]);
            lDocPatern := Format('%20.20s - %20.20s', [lDocId, lCasaId]);
            lDestNode := NewCopyNode;
            { Daca este invalid din cauza documentului din gestiune .... }
            if lNode.Strings[GridImperecheriSTARE.Index] = '0' then begin
               lNewDocId := ParseAndGetNewIdDoc(lDocId);
               if lNewDocId = -1 then
                 { Este posibil sa nu fi fost modificat ci anulat si refacut
                 In cazul acesta mergem pe alta clauza de where si cautam documentele cu acelasi tip acelasi numar si aceeasi data }
                 lWhere := 'JOIN GEST_DOCUM E ON (E.ID_GEST_TIP_DOCUM = A.ID_GEST_TIP_DOCUM AND FLOOR(CONVERT(FLOAT, E.DATA_DOCUM)) = FLOOR(CONVERT(FLOAT, A.DATA_DOCUM)) AND RTRIM(LTRIM(E.NR_DOCUM)) = RTRIM(LTRIM(A.NR_DOCUM)))'#13#10+
                           'WHERE E.ID_GEST_DOCUM = '+lDocId+' AND A.STARE=1'
               else lWhere := 'WHERE A.STARE=1 AND A.ID_GEST_DOCUM = '+IntToStr(lNewDocId);
               with TZReadOnlyQuery(DBNewQuery()) do
                 try
                    ParamCheck := False;
                    Sql.Add('SELECT A.ID_GEST_DOCUM, B.COD_DOCUM, A.NR_DOCUM, A.DATA_DOCUM, ');
                    Sql.Add('C.NUME AS PREDATOR, D.NUME AS PRIMITOR, A.TOTALDOC');
                    Sql.Add('FROM GEST_DOCUM A');
                    Sql.Add('JOIN GEST_TIP_DOCUM B ON (A.ID_GEST_TIP_DOCUM = B.ID_GEST_TIP_DOCUM)');
                    Sql.Add('JOIN REPARTITORI C ON (C.ID_REPARTITORI = A.ID_PREDATOR)');
                    Sql.Add('JOIN REPARTITORI D ON (D.ID_REPARTITORI = A.ID_PRIMITOR)');
                    Sql.Add(lWhere);
                    Open;
                    if not IsEmpty then
                       while not Eof do begin
                         with lDestNode.AddChild do begin
                           Data := Pointer(Fields[0].AsInteger);
                           if lDestNode.ImageIndex <> 1 then begin
                              StateIndex := 1;
                              ImageIndex := 1;
                              { Daca nu este duplicat }
                              if lDupList.IndexOf(lDocPatern) = -1 then begin
                                 lDestNode.ImageIndex := 1;
                                 { Adaugam in lista de cautat duplicate }
                                 lDupList.Add(lDocPatern);
                              end;
                           end else ImageIndex := 2;
                           Strings[ListaDecontariCOD_DOCUM.Index]  := Fields[1].AsString;
                           Strings[ListaDecontariNR_DOCUM.Index]   := Fields[2].AsString;
                           Strings[ListaDecontariDATA_DOCUM.Index] := Fields[3].AsString;
                           Strings[ListaDecontariPREDATOR.Index]   := Fields[4].AsString;
                           Strings[ListaDecontariPRIMITOR.Index]   := Fields[5].AsString;
                           if Fields[6].AsCurrency <= lDestNode.Values[ListaDecontariSUMA.Index] then
                              Values[ListaDecontariSUMA.Index] := Fields[6].AsCurrency
                           else Values[ListaDecontariSUMA.Index] := lDestNode.Values[ListaDecontariSUMA.Index];
                         end;
                         Next;
                       end;
                 finally
                    Free;
                 end;
               end
            else begin
               { Luam si inregistrarile care au chitantele trosnite }
               lNewCasaId := GetNewCasaId(lCasaId);
               with TZReadOnlyQuery(DBNewQuery()) do
                 try
                    ParamCheck := False;
                    Sql.Add('SELECT ISNULL(NR_LIST, COD) AS ID, TIPDOC, CASE WHEN PLATI IS NULL THEN 1 ELSE 0 END AS TIP,');
                    Sql.Add('NRDOC, DATA, ISNULL(PLATI, INCASARI) AS SUMA, B.NUME FROM BREGISTRU A');
                    Sql.Add('JOIN REPARTITORI B ON (A.CODGEST = B.ID_REPARTITORI)');
                    Sql.Add('WHERE ISNULL(NR_LIST, COD) = '+IntToStr(lNewCasaId));
                    Open;
                    if not IsEmpty then
                       while not Eof do begin
                         with lDestNode.AddChild do begin
                           Data := Pointer(Fields[0].AsInteger);
                           if lDestNode.ImageIndex <> 1 then begin
                              StateIndex := 1;
                              ImageIndex := 3;
                              if lDupList.IndexOf(lDocPatern) = -1 then begin
                                 lDestNode.ImageIndex := 3;
                                 { Adaugam in lista de cautat duplicate }
                                 lDupList.Add(lDocPatern);
                              end;
                           end else ImageIndex := 2;
                           if Trim(FieldByName('TIPDOC').AsString) > '' then
                              Strings[ListaDecontariCOD_DOCUM.Index]  := FieldByName('TIPDOC').AsString
                           else if FieldByName('TIP').AsInteger = 1 then Strings[ListaDecontariCOD_DOCUM.Index] := 'PLT'
                                else Strings[ListaDecontariCOD_DOCUM.Index] := 'INC';
                           Strings[ListaDecontariNR_DOCUM.Index]   := FieldByName('NRDOC').AsString;
                           Strings[ListaDecontariDATA_DOCUM.Index] := FieldByName('DATA').AsString;
                           Strings[ListaDecontariPRIMITOR.Index]   := FieldByName('NUME').AsString;
                           if FieldByName('SUMA').AsCurrency <= lDestNode.Values[ListaDecontariSUMA.Index] then
                              Values[ListaDecontariSUMA.Index] := FieldByName('SUMA').AsCurrency
                           else Values[ListaDecontariSUMA.Index] := lDestNode.Values[ListaDecontariSUMA.Index];
                         end;
                         Next;
                       end;
                 finally
                    Free;
                 end;
            end;
          end;
        end;
      end;
  finally
     lDupList.Free;
  end;
  ListaDecontari.ApplyBestFit(nil);
end;

procedure TFrmReconcilereDecontari.BtnOkClick(Sender: TObject);
begin
  AplicaModificari;
end;

procedure TFrmReconcilereDecontari.AplicaModificari;
var I: Integer;
    lNode: TdxTreeListNode;
    lDocId,
    lDecId: Integer;

    function GetInnerNode(ANode: TdxTreeListNode): TdxTreeListNode;
    var J: Integer;
     begin
       Result := nil;
       for J := 0 to ANode.Count-1 do begin
         if ANode.Items[J].StateIndex = 1 then Result := ANode.Items[J];
         Break;
       end;
     end;

begin
  with TZReadOnlyQuery(DBNewQuery()) do
    try
       ParamCheck := False;
       for I := 0 to ListaDecontari.Count-1 do
        if ListaDecontari.Items[I].StateIndex = 1 then begin
           lDecId := Integer(ListaDecontari.Items[I].Data);
           lNode  := GetInnerNode(ListaDecontari.Items[I]);
           if lNode <> nil then begin
              lDocId := Integer(lNode.Data);
              case ListaDecontari.Items[I].ImageIndex of
                0:
                  { Stergere decontare }
                  begin
                    Sql.Clear;
                    Sql.Add('DELETE FROM GEST_DECONTARI WHERE ID_GEST_DECONTARI = '+IntToStr(lDecId));
                    ExecSql;
                  end;
                1:
                  { Actulizam documentul din gestiuni }
                  begin
                    Sql.Clear;
                    Sql.Add('UPDATE GEST_DECONTARI SET ID_GEST_DOCUM = '+IntToStr(lDocId)+' WHERE ID_GEST_DECONTARI = '+IntToStr(lDecId));
                    ExecSql;
                  end;
                3:
                  { Actulizam documentul din casa/banca }
                  begin
                    Sql.Clear;
                    Sql.Add('UPDATE GEST_DECONTARI SET ID_BREGISTRU = '+IntToStr(lDocId)+' WHERE ID_GEST_DECONTARI = '+IntToStr(lDecId));
                    ExecSql;
                  end;
              end;
           end
           else begin
              Sql.Clear;
              Sql.Add('DELETE FROM GEST_DECONTARI WHERE ID_GEST_DECONTARI = '+IntToStr(lDecId));
              ExecSql;
           end;
       end;
    finally
       Free;
    end;
    TfrmDecontari(Decontari).RefreshIncasari;
end;

procedure TFrmReconcilereDecontari.ListaDecontariGetSelectedIndex(
  Sender: TObject; Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TFrmReconcilereDecontari.ListaDecontariMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var aHit: TdxTreeListHitInfo;
    lNode: TdxTreeListNode;
begin
  if Button <> mbLeft then Exit;
  aHit := ListaDecontari.GetHitInfo(Point(X,Y));
  lNode := aHit.Node;
  if (aHit.hitType = htStateIcon) and (lNode <> nil) and (lNode.Parent = nil) then
     lNode.StateIndex := 1 - lNode.StateIndex;
end;

procedure TFrmReconcilereDecontari.ListaDecontariChangeNode(
  Sender: TObject; OldNode, Node: TdxTreeListNode);
var lGridId: Integer;
    lNode  : TdxDBGridNode;
begin
  if Node = nil then Exit;
  lGridId := Integer(Node.Data);
  lNode := TfrmDecontari(Decontari).GridImperecheri.FindNodeByKeyValue(lGridId);
  if lNode <> nil then begin
     lNode.MakeVisible;
     lNode.Focused := True;
  end;
end; 

end.

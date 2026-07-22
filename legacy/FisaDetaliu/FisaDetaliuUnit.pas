unit FisaDetaliuUnit;


interface
uses
  Windows, Messages, SysUtils, Variants, Classes, Controls;

type
  PTipFisa = ^TTipFisa;
  TTipFisa = record
    Id : Integer;
    Denumire : String;
    SQLProc : string;
    FieldId : string;
    FieldParinte : string;
    TipLista : Integer;
    SQLConditie : string;
  end;

procedure EmptyTipFise(aFisaHolder : TStrings);
procedure PopulateTipFiseBySQL(aSQL : String; aFisaHolder : TStrings);


implementation


uses ZDataset, DB, ZeosDBUtile;

procedure EmptyTipFise(aFisaHolder : TStrings);
var
  I : Integer;
  lTipFisa : PTipFisa;
begin
  for I := aFisaHolder.Count - 1 downto 0 do begin
    if aFisaHolder.Objects[I] <> nil then begin
      lTipFisa := PTipFisa(aFisaHolder.Objects[I]);
      Dispose(lTipFisa);
    end;
    aFisaHolder.Delete(I);
  end;
end;

function SetIfFieldFound(aDataSet : TDataSet; aFieldName : String; const aFieldType : TFieldType =ftString ) : Variant;
var
  lField : TField;
begin
  lField := aDataSet.FindField(aFieldName);
  if lField <> nil then begin
    case aFieldType of
      ftInteger : Result := lField.AsInteger;
      ftString : Result := lField.AsString
      else
        Result := lField.Value;
    end;
  end
  else begin
    case aFieldType of
      ftInteger : Result := 0;
      ftString : Result := ''
      else
        Result := Null
    end;
  end;
end;


procedure PopulateTipFiseBySQL(aSQL : String; aFisaHolder : TStrings);
var
  lQry : TZReadOnlyQuery;
  lTipFisa : PTipFisa;
begin
  lQry := DBNewQuery();
  EmptyTipFise(aFisaHolder);
  try
    try
      lQry.SQL.Text := aSQL; //'exec spGestFisaProdusTipuri';
      lQry.Open;
      lQry.First;
      while not lQry.Eof do begin
         New(lTipFisa);
         FillMemory(lTipFisa, SizeOf(TTipFisa), 0);

         lTipFisa^.Id := SetIfFieldFound(lQry, 'ID', ftInteger);
         lTipFisa^.Denumire := SetIfFieldFound(lQry, 'denumire');
         lTipFisa^.SQLProc :=   SetIfFieldFound(lQry, 'SQLProc'); //lQry.FieldByName('SQLProc').AsString;
         lTipFisa^.TipLista := SetIfFieldFound(lQry, 'TipLista', ftInteger); //lQry.FieldByName('TipLista').AsInteger;
         lTipFisa^.FieldId := SetIfFieldFound(lQry, 'FieldID'); //lQry.FieldByName('FieldID').AsString;
         lTipFisa^.FieldParinte := SetIfFieldFound(lQry, 'FieldParinte'); //lQry.FieldByName('FieldParinte').AsString;
         lTipFisa^.SQLConditie :=  SetIfFieldFound(lQry, 'sqlConditie'); //lQry.FieldByName('sqlConditie').AsString;

         aFisaHolder.AddObject(lTipFisa^.Denumire, TObject(lTipFisa));
         lQry.Next;
      end;
    except
    end;
  finally
    lQry.Free;
  end;
end;

end.

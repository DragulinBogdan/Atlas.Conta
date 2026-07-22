unit FunctiiUtilizator;

interface

uses Graphics, SysUtils, Forms, raFunc, ppRTTI, Classes, ppReport, ppDB;

type


 TAtlasFunctions = class(TraSystemFunction)
   public
     class function Category : String; override;
   end;


 TResizeFont = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

             
 TParamByName = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TNumeOperator = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TNumeCompletOperator = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TAdresaUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TLocalitateUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TJudetUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TCodFiscalUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TDenumireUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TTelefonUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TEmailUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TSiglaUnitate = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

 TLinkData = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

  TSetFilter = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;

  TNextRecord = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams: TraParamList); override;
     class function GetSignature: String; override;
   end;


 TMoneySpell = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams : TraParamList); override;
     class function GetSignature: String; override;
   end;

  TOpenRaport = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams : TraParamList); override;
     class function GetSignature: String; override;
   end;

  TMoveSummaryBottom = class(TAtlasFunctions)
   public
     procedure ExecuteFunction(aParams : TraParamList); override;
     class function GetSignature: String; override;
   end;



implementation

uses Variants, SetParamsUnitADO, ppClass, ppDBBDE, ZDataSet, CommonDBVar, ppDBPipe, daQueryDataView,
   ppUtils, ppTypes, ppCtrls, ppDevice, ppSubRpt, ppRelatv;

{ TAtlasFunctions }

class function TAtlasFunctions.Category: String;
begin
  Result := 'Atlas Functions';
end;

{ TParamByName }

procedure TParamByName.ExecuteFunction(aParams: TraParamList);
var
  lParam: TAdoCRParam;
  lParamName: String;
begin
  GetParamValue(0, lParamName);
  lParam := CRAdoParamByName(lParamName);
  if lParam <> nil then
     lParamName := VarToStr(lParam.LastValue);
  SetParamValue(1, lParamName)
  { Intoarcem Numele Operatorului }
end;

class function TParamByName.GetSignature: String;
begin
  Result := 'function ParamByName(aName: String): String;';
end;

{ TLinkData }

procedure TLinkData.ExecuteFunction(aParams: TraParamList);
var
  lLink: TppMasterFieldLink;
  lMasterPipeline: TppDatapipeline;
  lDetailPipeline: TppDatapipeline;
  FirstData, SecondData : TppDatapipeline;
  FirstField, SecondField : String;
  aResult : Boolean;
//  ppPipeLine : TppBDEPipeline; 
begin
{  aResult := True;
  GetParamValue(0, FirstData);
  GetParamValue(1, SecondData);
  GetParamValue(2, FirstField);
  GetParamValue(3, SecondField);

  if (not Assigned(FirstData)) and (not Assigned(SecondData)) then begin
    aResult := False;
    SetParamValue(4, aResult);
    Exit;
  end;

  lMasterPipeline := FirstData;
  lDetailPipeline := SecondData;

  TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Active := False;
  TZQuery(TppBDEPipeline(lDetailPipeline).DataSource.DataSet).Active := False;
  TdaDataview(lMasterPipeline.DataView).Active := False;
  TdaDataview(lDetailPipeline.DataView).Active := False;

  TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).DataSource
  := TppBDEPipeline(lDetailPipeline).DataSource;
  lDetailPipeline.MasterDatapipeline := lMasterPipeline;

  lLink := TppMasterFieldLink.Create(nil);
  lLink.Parent := lDetailPipeline;
  lLink.Name := lDetailPipeline.GetValidName(lLink);
  lLink.DetailFieldName := FirstField;
  lLink.MasterFieldName := SecondField;

  TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Active := True;
  TZQuery(TppBDEPipeline(lDetailPipeline).DataSource.DataSet).Active := True;

  TdaDataview(lMasterPipeline.DataView).Active := True;
  TdaDataview(lDetailPipeline.DataView).Active := True;


  SetParamValue(4, aResult);}

    aResult := True;
  GetParamValue(0, FirstData);
  GetParamValue(1, SecondData);
  GetParamValue(2, FirstField);
  GetParamValue(3, SecondField);

  if (not Assigned(FirstData)) and (not Assigned(SecondData)) then begin
    aResult := False;
    SetParamValue(4, aResult);
    Exit;
  end;

  lMasterPipeline := FirstData;
  lDetailPipeline := SecondData;

  TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Active := False;
  TZQuery(TppBDEPipeline(lDetailPipeline).DataSource.DataSet).Active := False;
  TdaDataview(lMasterPipeline.DataView).Active := False;
  TdaDataview(lDetailPipeline.DataView).Active := False;
  lDetailPipeline.MasterDatapipeline := lMasterPipeline;
  
  TZQuery(TppBDEPipeline(lDetailPipeline).DataSource.DataSet).DataSource
   := TppBDEPipeline(lMasterPipeline).DataSource;


  if (FirstField > '') or (SecondField > '') then begin
    lLink := TppMasterFieldLink.Create(lMasterPipeline);
    lLink.Name := lDetailPipeline.GetValidName(lLink);
    lLink.Parent := lDetailPipeline;
    lLink.DetailFieldName := SecondField;
    lLink.MasterFieldName := FirstField;
  end;
//  ppPipeLine := TppBDEPipeline.Create(Self);
//  ppPipeLine.DataSource := TZQuery(TppBDEPipeline(lDetailPipeline).DataSource.DataSet).DataSource;
//  ppPipeLine.MasterDataSource := TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).DataSource;
//  ppPipeLine.MasterFieldLinks :=



  TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Active := True;
  TZQuery(TppBDEPipeline(lDetailPipeline).DataSource.DataSet).Active := True;

  TdaDataview(lMasterPipeline.DataView).Active := True;
  TdaDataview(lDetailPipeline.DataView).Active := True;


  SetParamValue(4, aResult);
end;

class function TLinkData.GetSignature: String;
begin
  Result := 'function LinkData(FirstData, SecondData : TppDatapipeline; FirstField, SecondField : String): Boolean;';
end;

{ TMoneySpell }

procedure TMoneySpell.ExecuteFunction(aParams: TraParamList);
var Value  : Currency;
    StrValue : String;

Const

  aFirst20Str: array[0..19] of String =
     ('', 'unu', 'doi', 'trei', 'patru', 'cinci', 'sase', 'sapte', 'opt', 'noua', 'zece',
      'unsprezece', 'doisprezece', 'treisprezece', 'paisprezece', 'cincisprezece',
      'saisprezece', 'saptesprezece', 'optsprezece', 'nouasprezece');
  aZeciStr: array[1..8] of String =
     ('douazeci', 'treizeci', 'patruzeci', 'cincizeci', 'saizeci', 'saptezeci', 'optzeci', 'nouazeci');


    function IntegerToSpell(aInt: Int64;IsNested: Boolean=False): String;
    var FirstPart,
        SecondPart: Int64;
    begin
      if aInt >= 1000000000 then begin
         FirstPart  := aInt div 1000000000;
         SecondPart := aInt mod 1000000000;
         if FirstPart = 1 then Result := 'miliard'
         else if FirstPart > 19 then Result := IntegerToSpell(FirstPart, True)+' ' + 'de' + ' '+'miliarde'
              else Result := IntegerToSpell(FirstPart, True)+' '+'miliarde';
         Result := Result + ' ' + IntegerToSpell(SecondPart, True);
      end
      else
      if aInt >= 1000000 then begin
         FirstPart  := aInt div 1000000;
         SecondPart := aInt mod 1000000;
         if FirstPart = 1 then Result := 'un milion'
         else if FirstPart > 19 then Result := IntegerToSpell(FirstPart, True)+' ' + 'de' + ' '+'milioane'
              else Result := IntegerToSpell(FirstPart, True)+' '+'milioane';
         Result := Result + ' ' + IntegerToSpell(SecondPart, True);
      end
      else
        if aInt >= 1000 then begin
           FirstPart  := aInt div 1000;
           SecondPart := aInt mod 1000;
           if FirstPart = 1 then Result := 'o mie'
           else if FirstPart mod 100 > 19 then Result := IntegerToSpell(FirstPart, True)+ ' ' +'de' + ' '+'mii'
                else Result := IntegerToSpell(FirstPart, True)+' '+'mii';
           Result := Result + ' ' + IntegerToSpell(SecondPart, True);
        end
        else
          if aInt >= 100 then begin
             FirstPart  := aInt div 100;
             SecondPart := aInt mod 100;
             if FirstPart = 1 then Result := 'o suta'
             else Result := IntegerToSpell(FirstPart, True)+' '+'sute';
             Result := Result + ' ' + IntegerToSpell(SecondPart, True);
          end
          else
            if aInt >= 20 then begin
               FirstPart  := aInt div 10;
               SecondPart := aInt mod 10;
               Result := aZeciStr[FirstPart-1];
               if SecondPart > 0 then Result := Result + ' ' +'si' + ' ' + IntegerToSpell(SecondPart, True);
               //if FirstPart > 0 then Result := Result + 'de';
            end
            else
               if (aInt = 2) and (IsNested)  then
                  Result := 'doua'
               else Result := aFirst20Str[aInt];
    end;


  function CurrencyToSpell(aValue: Currency): String;
  begin
    Result :=
      IntegerToSpell(Trunc(aValue));
    if (aValue > 100) and (Trunc(aValue) mod 100> 20) then
       Result := Result + ' ' + 'de';
    Result := Result + ' ' + 'lei';
    if Round(Frac(aValue)*100) > 0 then
       Result := Result + ' ' + 'si'+' '+
                 IntegerToSpell(Round(Frac(aValue)*100))+' '+
                 'bani';
  end;

begin
  GetParamValue(0, Value);
  StrValue := CurrencyToSpell(Value);
  SetParamValue(1, StrValue);
end;


class function TMoneySpell.GetSignature: String;
begin
  Result := 'function MoneySpell(Value:Currency):String;';
end;

{ TSetFilter }

procedure TSetFilter.ExecuteFunction(aParams: TraParamList);
var
  lMasterPipeline: TppDatapipeline;
  Condition : String;
  aResult : Boolean;
begin
  aResult := True;
  GetParamValue(0, lMasterPipeline);
  GetParamValue(1, Condition);

  if (lMasterPipeline <> nil) then
      if Trim(Condition) = '' then
        TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Filtered := False
      else begin
        TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Filtered := True;
        TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Filter := Condition;
      end;
      
  SetParamValue(2, aResult);
end;

class function TSetFilter.GetSignature: String;
begin
  Result := 'function SetFilter(DataPipeLine : TppDatapipeline; FieldCondition : String): Boolean;';
end;

{ TNextRecord }

procedure TNextRecord.ExecuteFunction(aParams: TraParamList);
var
  lMasterPipeline: TppDatapipeline;
  aResult : Boolean;
begin
  aResult := True;
  GetParamValue(0, lMasterPipeline);
  if lMasterPipeline <> nil then
     TZQuery(TppBDEPipeline(lMasterPipeline).DataSource.DataSet).Next;
  SetParamValue(1, aResult);
end;

class function TNextRecord.GetSignature: String;
begin
  Result := 'function NextRecord(DataPipeLine : TppDatapipeline): Boolean;';
end;

procedure TNumeOperator.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, NumeLogin);
end;

class function TNumeOperator.GetSignature: String;
begin
  Result := 'function NumeOperator: String;';
end;

{ TNumeCompletOperator }

procedure TNumeCompletOperator.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0,  NumeLoginComplet);
end;

class function TNumeCompletOperator.GetSignature: String;
begin
  Result := 'function NumeCompletOperator: String;';
end;

{ TAdresaUnitate }

procedure TAdresaUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetAdresaSocietate);
end;

class function TAdresaUnitate.GetSignature: String;
begin
  Result := 'function AdresaUnitate: String;';
end;

{ TDenumireUnitate }

procedure TDenumireUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetNumeSocietate);
end;

class function TDenumireUnitate.GetSignature: String;
begin
  Result := 'function DenumireUnitate: String;';
end;

{ TTelefonUnitate }

procedure TTelefonUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetTelefon);
end;

class function TTelefonUnitate.GetSignature: String;
begin
  Result := 'function TelefonUnitate: String;';
end;

{ TEmailUnitate }

procedure TEmailUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetEmail);
end;

class function TEmailUnitate.GetSignature: String;
begin
  Result := 'function EmailUnitate: String;';
end;

{ TLocalitateUnitate }

procedure TLocalitateUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetLocalitate);
end;

class function TLocalitateUnitate.GetSignature: String;
begin
  Result := 'function LocalitateUnitate: String;';
end;

{ TSiglaUnitate }

procedure TSiglaUnitate.ExecuteFunction(aParams: TraParamList);
var aPicture: TObject;
    Result: Boolean;
begin
  Result := False;
  GetParamValue(0, aPicture);
  if Assigned(aPicture) then begin
     TPicture(aPicture).Graphic := antetImagine;
     Result := True;
  end;
  SetParamValue(1, Result);
  { Intoarcem Numele Operatorului }
end;

class function TSiglaUnitate.GetSignature: String;
begin
  Result := 'function SiglaUnitate(aPicture: TObject): Boolean;';
end;

{ TJudetUnitate }

procedure TJudetUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetJudet);
end;

class function TJudetUnitate.GetSignature: String;
begin
  Result := 'function JudetUnitate: String;';
end;

{ TCodFiscalUnitate }

procedure TCodFiscalUnitate.ExecuteFunction(aParams: TraParamList);
begin
  { Intoarcem Numele Operatorului }
  SetParamValue(0, antetCodFiscal);
end;

class function TCodFiscalUnitate.GetSignature: String;
begin
  Result := 'function CodFiscalUnitate: String;';
end;

{ TOpenRaport }


function GetProcedure(const TextExecutie : String):String;
var
   P,P1, P2 : PChar;
   Name : String;
   StatementFound : Boolean;
   ExecStat : Boolean;
begin
  P := PChar(TextExecutie);
  P2 := P;
  ExecStat := False;
  repeat
    P1 := P;
    while not(P^ in [' ', ':', ',', #0, #$D, #$A]) do Inc(P);
    SetString(Name, P1, P-P1);
    ExecStat := ExecStat or ((AnsiCompareText('EXEC', Name) = 0) or  (AnsiCompareText('EXECUTE', Name) = 0));
    StatementFound := (ExecStat or (P1=P2)) and not((AnsiCompareText('EXEC', Name) = 0) or  (AnsiCompareText('EXECUTE', Name) = 0));
    if StatementFound then begin
      Result := Name;
      Break;
    end;
    while (P^ in [' ', ':', ',']) and (P^ <> #0) do Inc(P);
    if P^ = #0 then Break;
  until StatementFound;
end;


procedure TOpenRaport.ExecuteFunction(aParams: TraParamList);
var
  ItemId             : Integer;
  ReportName         : String;
  PipelineParams     : String;
//  aQry               : TZQuery;
  aRap               : TppReport;
  DataSources        : TList;
  I                  : Integer;
  //PipelineParamsList : TStringList;
  SQLExec            : String;
  ParamList          : String; 
begin
  inherited;
  aParams.GetParamValue(0, ItemId);
  aParams.GetParamValue(1, ReportName);
  aParams.GetParamValue(2, PipelineParams);
  try
    aRap := LoadReportEx(ItemId);
    if Trim(PipelineParams) <> '' then begin
      DataSources := TList.Create;
      aRap.GetDataPipelines(DataSources);
      for I := 0 to DataSources.Count - 1 do
        if Pos('<' + TppDataPipeline(DataSources[I]).UserName + '>', PipelineParams) > 0 then begin
          ParamList :=
               Copy(PipelineParams,
                    Pos('<' + TppDataPipeline(DataSources[I]).UserName + '>', PipelineParams) + Length('<' + TppDataPipeline(DataSources[I]).UserName + '>'),
                    Pos('</' + TppDataPipeline(DataSources[I]).UserName + '>', PipelineParams) - Pos('<' + TppDataPipeline(DataSources[I]).UserName + '>', PipelineParams)- Length('<' + TppDataPipeline(DataSources[I]).UserName + '>')
               );
          SQLExec := TdaQueryDataView(TppDataPipeline(DataSources[I]).DataView).SQL.MagicSQLText.Text;
          SQLExec := 'EXEC ' + GetProcedure(SQLExec) + ' ' + ParamList;
          TdaQueryDataView(TppDataPipeline(DataSources[I]).DataView).SQL.MagicSQLText.Text := SQLExec;
        end;

    end;
    aRap.Engine.Init;
    aRap.DeviceType := 'Screen';
    aRap.Print;
  finally
    aRap.Free;
  end;
end;



class function TOpenRaport.GetSignature: String;
begin
  Result := 'function OpenRaport(ItemId : Integer; ReportName : String; PipelineParams : String):Integer;';
end;


{ TResizeFont }

procedure TResizeFont.ExecuteFunction(aParams: TraParamList);
var aSize : Integer;
    aReport : TppReport;
    liTextWidth: Integer;
    liTextHeight : Integer;
    liTextHeight1 : Integer;
    liComponentWidth: Integer;
    lbKeepShrinking: Boolean;
    lsText: String;
    aDbText : TppDBText;
    Factor  : Double;
begin
  GetParamValue(0, aSize);
  GetParamValue(1, aReport);
  GetParamValue(2, aDbText);
  GetParamValue(3, lsText);
  aReport.Printer.Canvas.Font.Size := aSize;
  aDbText.Font.Size := aSize;
  lbKeepShrinking := True;
  while lbKeepShrinking do begin
      liComponentWidth := ppToMMThousandths(aDbText.Width, utInches, pprtHorizontal, aReport.Printer);
      liTextWidth := aReport.Printer.Canvas.TextWidth(lsText);
      liTextWidth := ppToMMThousandths(liTextWidth, utPrinterPixels, pprtHorizontal, aReport.Printer);

      liTextHeight := ppToMMThousandths(aDbText.Height, utInches, pprtVertical{ Horizontal}, aReport.Printer);
      liTextHeight1 := aReport.Printer.Canvas.TextHeight(lsText);
      liTextHeight1 := ppToMMThousandths(liTextHeight1, utPrinterPixels, pprtHorizontal, aReport.Printer);
      Factor := liTextHeight/liTextHeight1;
      Factor := Round(Factor);

      if (liTextWidth > liComponentWidth*Factor) then begin
          aReport.Printer.Canvas.Font.Size := aReport.Printer.Canvas.Font.Size - 1;
          aDBText.Font.Size := aDBText.Font.Size - 1;
      end
      else lbKeepShrinking := False;
  end;
  SetParamValue(2, aDbText);
end;

class function TResizeFont.GetSignature: String;
begin
  Result := 'function ResizeFont(aSize: Integer; aRaport : TppReport; aDbText : TppDBText; Text : String): Integer;';
end;

{ TMoveSummaryBottom }

procedure TMoveSummaryBottom.ExecuteFunction(aParams: TraParamList);
var
  ppReport : TppReport;
  liSummaryTop: Integer;
  liNewSummaryTop: Integer;
  liIndex: Integer;
  lDrawCommand: TppDrawCommand;

  function TestInBand(aDrawCmd : TppDrawCommand; aBand : TppBand) : Boolean;
  var
     I, J, K : Integer;
  begin
    Result := (aDrawCmd.Band = aBand);
    if not Result then
      for I := 0 to aBand.ObjectCount -1 do
        if aBand.Objects[I] is TppSubReport then
          for J := 0 to TppSubReport(aBand.Objects[I]).ChildCount-1 do
           if TppSubReport(aBand.Objects[I]).Children[J] is TppChildReport then
              for K := 0 to TppChildReport(TppSubReport(aBand.Objects[I]).Children[J]).BandCount-1 do begin
                 Result := TestInBand(aDrawCmd, TppChildReport(TppSubReport(aBand.Objects[I]).Children[J]).Bands[K]);
                 if Result then Break;
              end;
  end;

begin
  inherited;
  aParams.GetParamValue(0, ppReport);
  if (ppReport = nil) or (ppReport.Summary = nil) then Exit;
  liSummaryTop := ppReport.Summary.PrintPosRect.Top;

  liNewSummaryTop := ppReport.Engine.PageBottom - ppReport.Summary.SpaceUsed;

  for liIndex := 0 to ppReport.Engine.Page.DrawCommandCount -1 do
      if TestInBand(ppReport.Engine.Page.DrawCommands[liIndex], ppReport.Summary) then begin
          lDrawCommand := ppReport.Engine.Page.DrawCommands[liIndex];
          lDrawCommand.Top := (lDrawCommand.Top - liSummaryTop) + liNewSummaryTop;
      end;
end;

class function TMoveSummaryBottom.GetSignature: String;
begin
  Result := 'function SummaryToBootom(ppReport : TppReport) : Integer;';
end;

initialization
  raRegisterFunction('ParamByName', TParamByName);
  raRegisterFunction('LinkData', TLinkData);
  raRegisterFunction('SetFilter', TSetFilter);
  raRegisterFunction('NextRecord', TNextRecord);
  raRegisterFunction('MoneySpell', TMoneySpell);
  raRegisterFunction('NumeOperator', TNumeOperator);
  raRegisterFunction('NumeCompletOperator', TNumeCompletOperator);
  raRegisterFunction('EmailUnitate', TEmailUnitate);
  raRegisterFunction('AdresaUnitate', TAdresaUnitate);
  raRegisterFunction('TelefonUnitate', TTelefonUnitate);
  raRegisterFunction('DenumireUnitate', TDenumireUnitate);
  raRegisterFunction('LocalitateUnitate', TLocalitateUnitate);
  raRegisterFunction('JudetUnitate', TJudetUnitate);
  raRegisterFunction('CodFiscalUnitate', TCodFiscalUnitate);
  raRegisterFunction('SiglaUnitate', TSiglaUnitate);
  raRegisterFunction('OpenRaport', TOpenRaport);
  raRegisterFunction('ResizeFont', TResizeFont);
  raRegisterFunction('SummaryToBootom', TMoveSummaryBottom);
end.

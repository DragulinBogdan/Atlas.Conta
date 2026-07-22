unit UnitFormule;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, db, ToolWin, ComCtrls, Buttons, Menus, ActnList, ImgList,
  ZDataSet, cxLookAndFeelPainters, cxButtons, cxGraphics,cxLookAndFeels, cxControls,
  cxContainer, cxEdit, cxTextEdit, cxMemo, cxRichEdit;

type

  TTipFormula = (tfConditie,tfFormula);
  TDataBaseType = (tdbtADO, tdbtBDE);

  TFrmFormula = class(TForm)
    Label1: TLabel;
    FunctionPopup: TPopupMenu;
    FunctiiAritmetice1: TMenuItem;
    Round: TMenuItem;
    Floor: TMenuItem;
    Ceilling: TMenuItem;
    Abs: TMenuItem;
    Power: TMenuItem;
    Sqrt: TMenuItem;
    Exp: TMenuItem;
    Log: TMenuItem;
    Square: TMenuItem;
    Log10: TMenuItem;
    Sgn: TMenuItem;
    FunctiiTrigonometrice1: TMenuItem;
    ACos: TMenuItem;
    Cos: TMenuItem;
    Sin: TMenuItem;
    ASin: TMenuItem;
    Tan: TMenuItem;
    ATan: TMenuItem;
    Cot: TMenuItem;
    PI: TMenuItem;
    Radians: TMenuItem;
    Rand: TMenuItem;
    FunctiiDeTipData1: TMenuItem;
    GetDate: TMenuItem;
    DateDiff: TMenuItem;
    Year: TMenuItem;
    Month: TMenuItem;
    Day: TMenuItem;
    Degrees: TMenuItem;
    Captura: TLabel;
    btnCampuri: TcxButton;
    btnVariabile: TcxButton;
    btnFunctii: TcxButton;
    DataFieldPopup: TPopupMenu;
    FunctiiDeConversie1: TMenuItem;
    Int: TMenuItem;
    VarChar: TMenuItem;
    SmallDateTime: TMenuItem;
    Money: TMenuItem;
    Float: TMenuItem;
    Conditite1: TMenuItem;
    FunctiiPentruSiruri1: TMenuItem;
    CharIndex: TMenuItem;
    NChar: TMenuItem;
    PatIndex: TMenuItem;
    Ascii: TMenuItem;
    Char: TMenuItem;
    Diference: TMenuItem;
    Left: TMenuItem;
    Len: TMenuItem;
    Lower: TMenuItem;
    LTrim: TMenuItem;
    Replace: TMenuItem;
    QuoteName: TMenuItem;
    Replicate: TMenuItem;
    Reverse: TMenuItem;
    Right: TMenuItem;
    RTrim: TMenuItem;
    Soundex: TMenuItem;
    Space: TMenuItem;
    Str: TMenuItem;
    SubString: TMenuItem;
    Upper: TMenuItem;
    FormulaEdit: TcxRichEdit;
    ppFormule: TPopupMenu;
    ActiuniFormule: TActionList;
    ImgFormule: TImageList;
    Cmd_Undo: TAction;
    Cmd_Redo: TAction;
    Cmd_Copy: TAction;
    Cmd_Cut: TAction;
    Cmd_Paste: TAction;
    Cmd_SubQuery: TAction;
    Cmd_SpellCheck: TAction;
    Undo1: TMenuItem;
    Redo1: TMenuItem;
    N1: TMenuItem;
    Cut1: TMenuItem;
    Copy1: TMenuItem;
    Paste1: TMenuItem;
    N2: TMenuItem;
    Verifica1: TMenuItem;
    N3: TMenuItem;
    ppAvansat: TMenuItem;
    TblButoane: TToolBar;
    ToolButton1: TToolButton;
    ToolButton2: TToolButton;
    ToolButton3: TToolButton;
    ToolButton4: TToolButton;
    ToolButton5: TToolButton;
    ToolButton6: TToolButton;
    ppLitereMari: TMenuItem;
    ppLitereMici: TMenuItem;
    ppFont: TMenuItem;
    AdaugaSubinterogare1: TMenuItem;
    Cmd_Execute: TAction;
    Cmd_SetFont: TAction;
    Executa1: TMenuItem;
    ToolButton8: TToolButton;
    FontDialog: TFontDialog;
    OrBtn: TcxButton;
    PlusBtn: TcxButton;
    MinusBtn: TcxButton;
    OriBtn: TcxButton;
    ImpartitBtn: TcxButton;
    EgalBtn: TcxButton;
    MaiMicBtn: TcxButton;
    MaiMareBtn: TcxButton;
    DiferitBtn: TcxButton;
    MaiMicEgalBtn: TcxButton;
    MaiMareEgalBtn: TcxButton;
    NotBtn: TcxButton;
    AndBtn: TcxButton;
    VerifyBtn: TcxButton;
    ClearBtn: TcxButton;
    OkBtn: TcxButton;
    btnClose: TcxButton;
    procedure AddField(Sender: TObject);
    procedure btnCloseClick(Sender: TObject);
    procedure ClearBtnClick(Sender: TObject);
    procedure OkBtnClick(Sender: TObject);
    procedure RandClick(Sender: TObject);
    procedure FormulaEditChange(Sender: TObject);
    procedure MinusBtnClick(Sender: TObject);
    procedure VerifyBtnClick(Sender: TObject);
    procedure FloatClick(Sender: TObject);
    procedure Conditite1Click(Sender: TObject);
    procedure Cmd_UndoExecute(Sender: TObject);
    procedure Cmd_RedoExecute(Sender: TObject);
    procedure Cmd_CopyExecute(Sender: TObject);
    procedure Cmd_CutExecute(Sender: TObject);
    procedure Cmd_PasteExecute(Sender: TObject);
    procedure Cmd_SpellCheckExecute(Sender: TObject);
    procedure Cmd_SetFontExecute(Sender: TObject);
    procedure ppLitereMariClick(Sender: TObject);
    procedure ppLitereMiciClick(Sender: TObject);
    procedure ppFormuleDrawMargin(Sender: TMenu; Rect: TRect);
    procedure Cmd_ExecuteExecute(Sender: TObject);
  private
    FOnValidare: TNotifyEvent;
    procedure Validate(Sender: TObject);
    procedure ShowResults;
    procedure AddToFormula(const AValue: String);
    { Private declarations }
  public
    Validare   : Pointer;
    Tip        : TTipFormula;
    TipConex   : TDataBaseType;
    DataSource : TDataSource;
    procedure AddItems(AMenu:TMenu; TableName: String; AName:String); overload;
    procedure AddItems(AMenu:TMenu; TableName: String); overload;
    procedure AddItems(AMenu:TMenu; aDataSet : TDataSet); overload;
    procedure AddItems(AMenu:TMenu; aDataSet : TDataSet; AddToCompletition : Boolean; FOnClick : TNotifyEvent = nil); overload;
    procedure AddFieldCont(Sender : TObject);

    property OnValidare : TNotifyEvent read FOnValidare write FOnValidare;
    { Public declarations }
  end;

function IsNumber(AStr:String):Boolean;


function FilterCondition(aDataSource : TDataSource;var aFilter : String) : Boolean;


const Operand:array[0..12] of String=('+','-','*','/','=','<','>','<>','<=','>=',' NOT ',' AND ',' OR ');

implementation

uses
  ATSDBEvaluator, UnitParametrii, cxButtonEdit, PlanConturiUnit, CommonDBVar;

{$R *.DFM}

function IsNumber(AStr:String):Boolean;
var i:Integer;
begin
  Result:=True;
  for i:=1 to Length(AStr) do begin
    Result:=AStr[i] in ['0'..'9'];
    if not Result then
       Break;
  end;
end;

function EsteRelationata(aTable: String): Boolean;
begin
   Result := (aTable <> 'STIPSAL') and (aTable <> 'STIPAVANS') and (aTable <> 'STIPTOT_DREP') and (aTable <> 'STIPTOT_RETIN') ;
end;

(*
function GetFormula(ADataBase:TDataBase; Pentru:String; szCaptura: String; var OldFormula:String; AValidate:Pointer; TipFormula:TTipFormula):Boolean; overload;
var I:Integer;
begin
  with TFrmFormula.Create(nil) do
    try
       Tip:=TipFormula;
       if TipFormula=tfFormula then
          for I:=ComponentCount-1 downto 0 do
            if (Components[i] is TSpeedButton) and (Components[i].Tag>3) then
               Components[i].Free;
       TableName:=Pentru;
       Validare:=AValidate;
       if Tip=tfFormula then
          Captura.Caption:=szCaptura
       else
          Captura.Caption:='Se Editeaza Conditia Pentru '+szCaptura;
       FormulaEdit.Text:=Trim(OldFormula);
       if Assigned(aDataBase) then begin
          DataBase:= aDataBase;
          if EsteRelationata(Trim(UpperCase(TableName))) then
             AddItems(DataFieldPopup,TableName);
       end
       else
          DataBase:=nil;
       AddItems(DataFieldPopup,'SSAL');
       AddItems(DataFieldPopup,'SPERS');
       AddItems(DataFieldPopup,'SPARAM');
       AddItems(DataFieldPopup,'SLOCM');
  //     if AnsiCompareText(Pentru, 'SPRIME') = 0 then
  //        AddItems(DataFieldPopup, 'SPRIME');
       Result:=ShowModal=mrOk;
       if Result then
          OldFormula:=Trim(FormulaEdit.Text);
    finally
       Free;
    end;
end;
*)
procedure TFrmFormula.btnCloseClick(Sender: TObject);
begin
  ModalResult:=mrCancel;
end;

procedure TFrmFormula.ClearBtnClick(Sender: TObject);
begin
  FormulaEdit.Text:=Trim(FormulaEdit.Text);
  if (FormulaEdit.Text<>'') and
     (MessageDlg('Doriti Stergerea Formulei Introduse ?',mtConfirmation,[mbYes,mbNo],0)=mrYes) then
     FormulaEdit.Clear;
end;

procedure TFrmFormula.OkBtnClick(Sender: TObject);
begin
  ModalResult:=mrOk;
end;

procedure TFrmFormula.Validate(Sender: TObject);
var
  lEvaluator: TATSEvaluator;
begin
  lEvaluator := TATSEvaluator.Create(nil);
  try
    lEvaluator.AddDataSet(DataSource.DataSet);
    lEvaluator.AddStaticFormula('', Trim(FormulaEdit.Text)).Evaluate(nil);
    okBtn.Visible := True;
  finally
    lEvaluator.Free;
  end;
end;

procedure TFrmFormula.RandClick(Sender: TObject);
var
  i,NrParams:Integer;
  ALabel:TLabel;
  AComboEdit: TcxButtonEdit;
  Params: String;
begin
  with TMenuItem(Sender) do begin
    NrParams:=Tag;
    if NrParams>0 then
       with TFrmParametrii.Create(Self) do
         try
            {ATableName:=TableName;
            ADataBase:=DataBase;}
            LbInfo.Caption:=Format(LbInfo.Caption,[Functie,NrParams]);
            Functie:=TMenuItem(Sender).Name;
            for i:=1 to NrParams do begin
                ALabel:=TLabel(FindComponent('LbParam'+IntToStr(i)));
                if Assigned(ALabel) then
                   ALabel.Visible:=True;
                AComboEdit:=TcxButtonEdit(FindComponent('EditParam'+IntToStr(i)));
                if AssigneD(AComboEdit) then
                   AComboEdit.Visible:=True;
            end;
            if ShowModal=mrOk then begin
               Params:=TMenuItem(Sender).Name+'('+Trim(EditParam1.Text);
               for i:=2 to NrParams do begin
                   AComboEdit:=TcxButtonEdit(FindComponent('EditParam'+IntToStr(i)));
                   if Assigned(AComboEdit) then
                      Params:=Params+','+Trim(AComboEdit.Text);
               end;
               Params:=Params+')';
               AddToFormula(Params);
            end;
         finally
           Free;
         end
    else
      AddToFormula(Name+'()');
  end;
end;

procedure TFrmFormula.FormulaEditChange(Sender: TObject);
var Cont:Integer;
    IsOperand:Boolean;
    Formula:String;
begin
  { Stabilim comenzile active }
  Cmd_Copy.Enabled := FormulaEdit.SelLength > 0;
  Cmd_Cut.Enabled  := Cmd_Copy.Enabled;
  Cmd_Undo.Enabled := FormulaEdit.CanUndo;
  Cmd_Redo.Enabled := FormulaEdit.CanUndo;
  Cmd_SpellCheck.Enabled := FormulaEdit.Text <> '';

  OkBtn.Visible:=False;
  Formula:=Trim(FormulaEdit.Text);
  IsOperand:=Formula='';
  Cont:=0;
  while (not IsOperand) and (Cont<13) do begin
    IsOperand:=copy(Formula,Length(Formula)-Length(Trim(Operand[Cont]))+1,Length(Trim(Operand[Cont])))=Trim(Operand[Cont]);
    Cont:=Cont+1;
  end;
  
  if IsOperand then
    for Cont:=0 to ComponentCount-1 do begin
       if Components[Cont] is TcxButton then
         if TcxButton(Components[Cont]).Tag = -1 then
             TcxButton(Components[Cont]).Enabled := True
         else
             TcxButton(Components[Cont]).Enabled := False;
    end
  else
    for Cont:=0 to ComponentCount-1 do
       if Components[Cont] is TcxButton then
         if TcxButton(Components[Cont]).Tag = -1 then
             TcxButton(Components[Cont]).Enabled := False
         else
             TcxButton(Components[Cont]).Enabled := True;
  VerifyBtn.Visible:=(Formula<>'') and (not IsOperand);
end;

procedure TFrmFormula.MinusBtnClick(Sender: TObject);
begin
  AddToFormula(Operand[TcxButton(Sender).Tag]);
end;

procedure TFrmFormula.AddField(Sender: TObject);
begin
  AddToFormula(TMenuItem(Sender).Name);
end;

procedure TFrmFormula.AddItems(AMenu: TMenu; TableName: String; AName:String);
var AItem, AItem1: TMenuItem;
begin
  AItem:=TMenuItem.Create(AMenu);
  AItem.Caption:=AName;
  AItem.Hint:='Coloanele Tabelei '+AName;
  AItem.Name:=AName;
  AMenu.Items.Add(AItem);
  with TZQuery.Create(Self) do
    try
       Sql.Add('SELECT FIELD_NAME, FIELD_ALIAS FROM RB_FIELD WHERE TABLE_NAME LIKE :TABLE');
       Params.ParamByName('TABLE').Value := TableName;
       Open;
       while not Eof do begin
         AItem1:=TMenuItem.Create(AItem);
         AItem1.Caption:=Fields[1].AsString;
         AItem1.Hint:='Campul '+AItem1.Caption;
         AItem1.Name:=Trim(Fields[0].AsString);
         AItem1.OnClick := AddField;
         AItem.Add(AItem1);
         Next;
       end;
    finally
       Free;
    end;
end;

procedure TFrmFormula.AddItems(AMenu: TMenu; TableName: String);
begin
  AddItems(AMenu, TableName, TableName);
end;

procedure TFrmFormula.VerifyBtnClick(Sender: TObject);
var AMethod: TNotifyEvent;
begin
  if Assigned(FOnValidare) then
     FOnValidare(Self)
  else
    if Validare = nil then
       Validate(Sender)
    else begin
      TMethod(AMethod).Data:=Self;
      TMethod(AMethod).Code:=Validare;
      AMethod(Sender);
    end;
//  OkBtn.Visible:=True;
end;

procedure TFrmFormula.FloatClick(Sender: TObject);
begin
  with TFrmParametrii.Create(Self) do
    try
      LbInfo.Caption:=Format(LbInfo.Caption,['Convert',1]);
      Functie:=TMenuItem(Sender).Parent.Name;
      LbParam1.Visible:=True;
      EditParam1.Visible:=True;
      if ShowModal=mrOk then
        AddToFormula('Convert('+TMenuItem(Sender).Name+','+EditParam1.Text+')');
    finally
      Free;
    end;
end;

procedure TFrmFormula.Conditite1Click(Sender: TObject);
begin
  with TFrmParametrii.Create(Self) do
    try
      LbInfo.Caption:=Format(LbInfo.Caption,['IIF',3]);
      Functie:='IIF';
      Caption:=Caption+'IIF';
      LbParam1.Visible:=True;
      EditParam1.Visible:=True;
      EditParam1.Tag:=1;
      LbParam2.Visible:=True;
      EditParam2.Visible:=True;
      LbParam3.Visible:=True;
      EditParam3.Visible:=True;
      if ShowModal=mrOk then
        AddToFormula('CASE WHEN '+EditParam1.Text+' THEN '+EditParam2.Text+' ELSE '+EditParam3.Text+' END');
    finally
      Free;
    end;
end;

procedure TFrmFormula.Cmd_UndoExecute(Sender: TObject);
begin
  FormulaEdit.Undo;
end;

procedure TFrmFormula.Cmd_RedoExecute(Sender: TObject);
begin
  FormulaEdit.Undo;
end;

procedure TFrmFormula.Cmd_CopyExecute(Sender: TObject);
begin
  FormulaEdit.CopyToClipboard;
end;

procedure TFrmFormula.Cmd_CutExecute(Sender: TObject);
begin
  FormulaEdit.CutToClipboard;
end;

procedure TFrmFormula.Cmd_PasteExecute(Sender: TObject);
begin
  FormulaEdit.PasteFromClipboard;
end;

procedure TFrmFormula.Cmd_SpellCheckExecute(Sender: TObject);
begin
  VerifyBtnClick(VerifyBtn);
end;

procedure TFrmFormula.Cmd_SetFontExecute(Sender: TObject);
begin
  FontDialog.Font.Assign(FormulaEdit.Style.Font);
  if FontDialog.Execute then
     FormulaEdit.Style.Font.Assign(FontDialog.Font);
end;

procedure TFrmFormula.ppLitereMariClick(Sender: TObject);
begin
  FormulaEdit.SelText := UpperCase(FormulaEdit.SelText);
end;

procedure TFrmFormula.ppLitereMiciClick(Sender: TObject);
begin
  FormulaEdit.SelText := LowerCase(FormulaEdit.SelText);
end;

procedure TFrmFormula.ppFormuleDrawMargin(Sender: TMenu; Rect: TRect);
(*const Text='Formula';*)
begin
(*  with ppFormule.Canvas.Font do begin
    Name := 'Times New Roman';
    Size := 14;
    Color := clYellow;
    Handle := CreateRotatedFont(ppFormule.Canvas.Font, 90);
  end;

  ppFormule.DefaultDrawMargin(Rect, clLime, RGB(GetRValue(clLime) div 4,
                              GetGValue(clLime) div 4, GetBValue(clLime) div 4));
  SetBkMode(ppFormule.Canvas.Handle, TRANSPARENT);
  ExtTextOut(ppFormule.Canvas.Handle, Rect.Left, Rect.Bottom - 5, ETO_CLIPPED,
            @Rect, Text, Length(Text), nil);*)
end;

procedure TFrmFormula.Cmd_ExecuteExecute(Sender: TObject);
begin
  with TZQuery.Create(Self) do
    try
       Sql.Add(FormulaEdit.SelText);
       try
          Open;
          ShowResults;
       except
         on E:Exception do
          raise EContaHandledError.Create('Ordin Select Invalid !'#13#10'Eroare : '+E.Message);
       end;
    finally
       Free;
    end;
end;

procedure TFrmFormula.ShowResults;
begin
  //
end;

function FilterCondition(aDataSource : TDataSource;var aFilter : String) : Boolean;
var aFrm : TFrmFormula;
begin
   aFrm := TFrmFormula.Create(nil);
   with aFrm do
     try
       DataSource := aDataSource;
       FormulaEdit.Text:=Trim(aFilter);
       AddItems(DataFieldPopup, aDataSource.DataSet);
       Result:=ShowModal=mrOk;
       if Result then
          aFilter:=Trim(FormulaEdit.Text);
     finally
       aFrm.Free;
      end;
end;

procedure TFrmFormula.AddItems(AMenu: TMenu; aDataSet: TDataSet);
var AItem, AItem1: TMenuItem;
    I : Integer;
begin
  AItem:=TMenuItem.Create(AMenu);
  AItem.Caption:= aDataSet.Name;
  AItem.Hint:='Coloanele Tabelei '+aDataSet.Name;
  AItem.Name:=aDataSet.Name;
  AMenu.Items.Add(AItem);
  for I := 0 to aDataSet.FieldCount-1 do begin
      AItem1:=TMenuItem.Create(AItem);
      AItem1.Caption:= aDataSet.Fields[I].FullName;
      AItem1.Hint:='Campul '+AItem1.Caption;
      AItem1.Name:=Trim(aDataSet.Fields[I].FieldName);
      AItem1.OnClick:=AddField;
      AItem.Add(AItem1);
  end;
end;

procedure TFrmFormula.AddItems(AMenu: TMenu; aDataSet: TDataSet;
  AddToCompletition: Boolean; FOnClick : TNotifyEvent = nil);
var AItem, AItem1: TMenuItem;
begin
  if aDataSet.RecordCount = 0 then Exit;
  AItem:=TMenuItem.Create(AMenu);
  AItem.Caption:= aDataSet.Name;
  AItem.Hint:='Coloanele Tabelei '+aDataSet.Name;
  AItem.Name:=aDataSet.Name;
  AMenu.Items.Add(AItem);
  with aDataSet do begin
    First;
    //tabela are Valoare, Denumire, Descriere(pt auto complet)
    while not eof do begin
      AItem1         := TMenuItem.Create(AItem);
      AItem1.Caption := FieldByName('Denumire').AsString;
      AItem1.Hint    :='Campul '+ AItem1.Caption;
      AItem1.Name    :=Trim(FieldByName('Valoare').AsString);
      if Assigned(FOnClick) then
        AItem1.OnClick := FOnClick
      else
        AItem1.OnClick :=AddField;
      AItem.Add(AItem1);
      Next;
    end;
  end;
end;

procedure TFrmFormula.AddToFormula(const AValue: String);
begin
  if FormulaEdit.CaretPos.Y < FormulaEdit.Lines.Count then
    FormulaEdit.Lines[FormulaEdit.CaretPos.Y] := FormulaEdit.Lines[FormulaEdit.CaretPos.Y] + AValue
  else
    FormulaEdit.Lines.Add(AValue);
  FormulaEdit.SelStart  := Length(FormulaEdit.Text);
  FormulaEdit.SelLength := 0;
end;

procedure TFrmFormula.AddFieldCont(Sender: TObject);
var
  aCont : String;
  aExactCont : Boolean;
begin
  AddField(Sender);
  aExactCont := True;
  aCont := '';
  if SelectareContPlan(aCont, aExactCont) then
    AddToFormula(aCont);
end;

end.

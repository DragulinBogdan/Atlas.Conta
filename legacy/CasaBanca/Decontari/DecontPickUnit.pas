unit DecontPickUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, dxCntner, dxEditor, dxExEdtr, dxEdLib, 
  ExtCtrls, HeadPanel, dxDBCtrl, dxTL, dxDBGrid, Db, ZDataSet,
  dxDBELib, dxDBTLCl, dxGrClms,
  ZAbstractRODataset, ZAbstractDataset;

type
  TfrmDecontPick = class(TForm)
    StyleController: TdxEditStyleController;
    pnTop: THeadPanel;
    pnRest: TPanel;
    pnBottom: TPanel;
    btnOk: TSpeedButton;
    btnCancel: TSpeedButton;
    PickGrid: TdxDBGrid;
    DTPickDecont: TDataSource;
    QryPickDecont: TZQuery;
    PickGridCOD: TdxDBGridMaskColumn;
    PickGridNR_DECONT: TdxDBGridMaskColumn;
    PickGridDATA_DECONT: TdxDBGridDateColumn;
    PickGridAN: TdxDBGridMaskColumn;
    PickGridLUNA: TdxDBGridMaskColumn;
    PickGridCOD_CB: TdxDBGridMaskColumn;
    PickGridDENUMIRE_COD_CB: TdxDBGridMaskColumn;
    PickGridCHEIE: TdxDBGridMaskColumn;
    PickGridCODGEST: TdxDBGridMaskColumn;
    PickGridSUMA_DECONT: TdxDBGridMaskColumn;
    PickGridCOD_CBT: TdxDBGridMaskColumn;
    PickGridDENUMIRE_COD_CBT: TdxDBGridMaskColumn;
    PickGridCODSECTIE: TdxDBGridMaskColumn;
    PickGridNUME: TdxDBGridMaskColumn;
    PickGridJUSTIFICAT: TdxDBGridMaskColumn;
    PickGridPROCENT: TdxDBGridMaskColumn;
    PickGridNR_INTRARI: TdxDBGridMaskColumn;
    Label1: TLabel;
    Label2: TLabel;
    edtNrDecont: TdxDBEdit;
    edtDataDecont: TdxDBDateEdit;
    procedure btnOkClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure FormKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure PickGridDblClick(Sender: TObject);
  private
    FCasaPlecare: Integer;
    FCasaSosire: Integer;
    FArriveHouse: Integer;
    FLeaveHouse: Integer;
    FNrDecont: Integer;
    FDataDecont: TDateTime;
    FCodRepartitor: Integer;
    procedure SetCasaPlecare(const Value: Integer);
    procedure SetCasaSosire(const Value: Integer);
    procedure OpenDataSet;
    { Private declarations }
  public
    { Public declarations }
    constructor Create(AOwner: TComponent); override;

    procedure DechideSetDate(pCasaPlecare, pCasaSosire, pCodRepartitor : Integer);

    property CasaPlecare : Integer read FCasaPlecare write SetCasaPlecare default -1;
    property CasaSosire : Integer read FCasaSosire write SetCasaSosire default -1;
    property CodRepartitor : Integer read FCodRepartitor write FCodRepartitor default -1;

    property NrDecont : Integer read FNrDecont;
    property DataDecont : TDateTime read FDataDecont;
    property LeaveHouse : Integer read FLeaveHouse;
    property ArriveHouse : Integer read FArriveHouse;
  end;

implementation

uses CommonDBVar,  Variants, DateUnit;

{$R *.DFM}




procedure TfrmDecontPick.OpenDataSet;

procedure SetParam(ParamName : String; Value : Variant);
var aParam : TParam;
begin
  aParam := QryPickDecont.Params.ParamByName(ParamName);
  if aParam <> nil then begin
    aParam.Value := Null;
    if Value <> -1 then
      aParam.Value := Value;
  end;
end;

begin
 with QryPickDecont do begin
   if Active then Active := False;

   if FCasaSosire <> - 1 then
     Params.ParamByName('CASA_SOS').Value := FCasaSosire
   else
     Params.ParamByName('CASA_SOS').Value := Null;

   if FCasaPlecare <> -1 then
     Params.ParamByName('CASA_PLEC').Value := FCasaPlecare
   else
     Params.ParamByName('CASA_PLEC').Value := Null;

   SetParam('COD_REP', FCodRepartitor);

   Open;
 end;
end;

procedure TfrmDecontPick.SetCasaPlecare(const Value: Integer);
begin
  FCasaPlecare := Value;
  OpenDataSet;
end;

procedure TfrmDecontPick.SetCasaSosire(const Value: Integer);
begin
  FCasaSosire := Value;
  OpenDataSet;
end;

procedure TfrmDecontPick.btnOkClick(Sender: TObject);
var
  aNode : TdxTreeListNode;
begin
  if not Assigned(PickGrid.FocusedNode) then Exit;
  aNode := PickGrid.FocusedNode;

  FNrDecont := aNode.Values[PickGridNR_DECONT.Index];
  FDataDecont := aNode.Values[PickGridDATA_DECONT.Index];
  FLeaveHouse := aNode.Values[PickGridCOD_CBT.Index];
  FArriveHouse := aNode.Values[PickGridCOD_CB.Index];
  FCodRepartitor := aNode.Values[PickGridCODGEST.Index];
  
  ModalResult := mrOk;
end;

procedure TfrmDecontPick.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;


constructor TfrmDecontPick.Create(AOwner: TComponent);
begin
  inherited;
  FCasaPlecare := -1;
  FCasaSosire := -1;
  FCodRepartitor := -1;
end;

procedure TfrmDecontPick.FormKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
      btnOk.Click;
end;

procedure TfrmDecontPick.PickGridDblClick(Sender: TObject);
begin
 if Assigned(PickGrid.FocusedNode) then
   btnOk.Click;
end;

procedure TfrmDecontPick.DechideSetDate(pCasaPlecare, pCasaSosire,
  pCodRepartitor: Integer);
begin
  FCasaPlecare := pCasaPlecare;
  FCasaSosire := pCasaSosire;
  FCodRepartitor := pCodRepartitor;
  OpenDataSet;
end;

end.

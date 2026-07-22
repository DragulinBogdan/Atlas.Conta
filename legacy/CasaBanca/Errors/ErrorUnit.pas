unit ErrorUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, dxCntner, ExtCtrls, cxControls, cxPC, Buttons,
  dxExEdtr, dxEdLib, dxEditor, dxTL, dxDBCtrl, dxDBGrid, Db, ZDataSet,
  ImgList, dxfCheckBox, 
   dxDBTLCl, dxGrClms,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxLookAndFeelPainters,
  cxLookAndFeels, dxBarBuiltInMenu;

type
  TfrmSearchErrors = class(TForm)
    ErrorPageControl: TcxTabControl;
    pnClient: TPanel;
    pnSetari: TPanel;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    edStartDate: TdxDateEdit;
    edEndDate: TdxDateEdit;
    edCasa: TdxImageEdit;
    btnCauta: TBitBtn;
    pnList: TPanel;
    GridErrors: TdxDBGrid;
    DTError: TDataSource;
    QryErrors: TZQuery;
    GridErrorsCOD: TdxDBGridMaskColumn;
    GridErrorsDATA: TdxDBGridDateColumn;
    GridErrorsNRDOC: TdxDBGridMaskColumn;
    GridErrorsTIPDOC: TdxDBGridMaskColumn;
    GridErrorsTIP_EROARE: TdxDBGridImageColumn;
    ErrorList: TImageList;
    chkHouse: TdxfCheckBox;
    GridErrorsCOD_CB: TdxDBGridImageColumn;
    procedure btnCautaClick(Sender: TObject);
    procedure ErrorPageControlChange(Sender: TObject);
    procedure chkHouseClick(Sender: TObject);
    procedure GridErrorsDblClick(Sender: TObject);
    procedure ErrorPageControlDrawTabEx(AControl: TcxCustomTabControl;
      ATab: TcxTab; Font: TFont);
    procedure GridErrorsKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
    OldTabIndex : Integer;
    FShowDuplicates: Boolean;
  public
    { Public declarations }
    LocalizeHandle : THandle;
    TabsValues : array[0..4] of Integer;
    procedure AttachToRegion(aRegion : TWinControl; Index, OldIndex : Integer);
    property ShowDuplicates : Boolean read FShowDuplicates write FShowDuplicates;
  end;


const Tabs :array[0..4] of String = ('Setari', 'Validari', 'Transferuri', 'Nechilibrate', 'Duplicate');

var
  frmSearchErrors: TfrmSearchErrors;


procedure IfAvailableDestroy;

implementation

uses
  ZeosDBUtile, DateUnit, CommonCasa, RegistruUnit, CasaUnit, ATSZDBUtils;

{$R *.DFM}

procedure IfAvailableDestroy;
begin
//  if Assigned(frmSearchErrors) then frmSearchErrors.Free;
  frmSearchErrors := nil;
end;

procedure TfrmSearchErrors.AttachToRegion(aRegion: TWinControl; Index,
  OldIndex: Integer);
begin
  pnSetari.Visible := not(Index > 0);
  pnList.Visible := Index>0;
  if Index>0 then begin
    GridErrors.Filter.Add(GridErrorsTIP_EROARE, Index, 'Filtrare');
    FShowDuplicates := (Index = 4);
  end;
end;

procedure TfrmSearchErrors.btnCautaClick(Sender: TObject);
var I : Integer;
begin
  btnCauta.Enabled := False;
  try
    with QryErrors do begin
      if Active then Active := False;
      Params.ParamByName('DATA_IN').Value := edStartDate.Date;
      Params.ParamByName('DATA_OUT').Value := edEndDate.Date;
      if (chkHouse.Checked) and (edCasa.Text <> '') then
         Params.ParamByName('COD_CB').Value := edCasa.Text;
      Open;
      //shit outhere
      for I := 0 to High(TabsValues) do begin
        GridErrors.Filter.Add(GridErrorsTIP_EROARE, I, 'Filtrare');
        TabsValues[I] := GridErrors.Count;
      end;
      ErrorPageControl.Refresh;
    end;
  finally
    btnCauta.Enabled := True;
  end;
end;

procedure TfrmSearchErrors.ErrorPageControlChange(Sender: TObject);
begin
  AttachToRegion(pnClient, ErrorPageControl.TabIndex, OldTabIndex);
  OldTabIndex := ErrorPageControl.TabIndex;
end;

procedure TfrmSearchErrors.chkHouseClick(Sender: TObject);
begin
  edCasa.Enabled := chkHouse.Checked;
end;

procedure TfrmSearchErrors.GridErrorsDblClick(Sender: TObject);
var aNode : TdxTreeListNode;
    aLocalizeRecord : TLocalizeRecord;
begin
  if FShowDuplicates then Exit;
  Application.ProcessMessages;
  if not Assigned(GridErrors.FocusedNode) then Exit;
  aNode := GridErrors.FocusedNode;
  with aLocalizeRecord do begin
    Cod := GetAsInteger(aNode, GridErrorsCOD.Index);
    CodCb := GetAsInteger(aNode, GridErrorsCOD_CB.Index);
    Data := aNode.Values[GridErrorsDATA.Index];
  end;
  PostMessage(LocalizeHandle, WM_LOCALIZE, Integer(aNode), Integer(@aLocalizeRecord));
end;

procedure TfrmSearchErrors.ErrorPageControlDrawTabEx(
  AControl: TcxCustomTabControl; ATab: TcxTab; Font: TFont);
var aStr : String;
begin
  if  TabsValues[aTab.Index] >0 then begin
    aStr := '(' +IntToStr(TabsValues[aTab.Index]) +')';
    Font.Style := [fsBold];
  end
  else begin
    aStr := '';
    Font.Style := [];
  end;
  ATab.Caption := Tabs[aTab.Index] + aStr;
end;

procedure TfrmSearchErrors.GridErrorsKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
Var I : Integer;  
begin
  if (Key = Vk_Delete) and (ssCtrl in Shift) then begin
    //facem mutarea in arhiva a turor inregistrariilor aduse
    if not Assigned(frmCasa) then Exit;
    if not Assigned(frmCasa.FrmRegistru) then Exit;
    if not FShowDuplicates then Exit;
    for I := 0 to GridErrors.Count -1 do begin
       frmCasa.FrmRegistru.MoveToArchive('BREGISTRU', GridErrors.Items[I].Values[GridErrorsCOD.Index], -2, -100);
    end;
    frmCasa.FrmRegistru.IsModified := True;
    try
      DBStartTransaction;
      {$IFNDEF SQLMOVE}
        if frmCasa.FrmRegistru.QryShare_Point.UpdateStatus in [usModified, usInserted, usDeleted] then
           begin frmCasa.FrmRegistru.QryShare_Point.ApplyUpdates; frmCasa.FrmRegistru.QryShare_Point.CommitUpdates; end;
      {$ENDIF}
      frmCasa.FrmRegistru.DeleteShare(-100);
      DBCommit;
    except
      DBRollBack;
    end;
    frmCasa.FrmRegistru.IsModified := False;
  end;
end;

end.
  

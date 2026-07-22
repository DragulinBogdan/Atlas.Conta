unit IntretinNmclUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, StdCtrls, dxCntner, dxTL, dxDBCtrl, dxDBGrid, Db, dxExEdtr,
  DegradePanel, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels, cxControls, cxContainer, cxEdit, cxGroupBox, cxStyles,
  cxCustomData, cxFilter, cxData, cxDataStorage, cxNavigator, dxDateRanges,
  dxScrollbarAnnotations, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid;

type
  TfrmIntretinereNmcl = class(TForm)
    dtIntretinere: TDataSource;
    Info: TDegradePanel;
    pnBotomSelect: TcxGroupBox;
    btnOkSelect: TcxButton;
    btnCancelSelect: TcxButton;
    viewIntretinere: TcxGridDBTableView;
    nivelIntretinere: TcxGridLevel;
    gridIntretinere: TcxGrid;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormDestroy(Sender: TObject);
    procedure btnOkSelectClick(Sender: TObject);
    procedure btnCancelSelectClick(Sender: TObject);
  private
    FCaption    : String;
    FTableName  : String;
    FKeyField   : String;
    procedure SetDataSet(const Value: TDataSet);
    procedure SetCaption(const Value: String);
    procedure SetKeyField(const Value: String);
    procedure SetTableName(const Value: String);
    { Private declarations }
  protected
    function GetNiceName: String;
  public
    property TableName: String read FTableName write SetTableName;
    property KeyField : String read FKeyField write SetKeyField;
    property Caption  : String read FCaption write SetCaption;
    { Public declarations }
  end;

function AddNmclForm(const ATableName, AKeyFieldName: String; ACaption: String = ''; AIsParented: Boolean = True): TfrmIntretinereNmcl;

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, ZDataSet;

function CreateNmclForm(const ATableName, AKeyFieldName: String; ACaption: String = ''; AIsParented: Boolean = True): TfrmIntretinereNmcl;
begin
  Result := TfrmIntretinereNmcl.Create(Application);
  Result.KeyField   := AKeyFieldName;
  Result.FCaption   := ACaption;
  Result.Caption    := Result.GetNiceName;
  Result.Info.Caption := Result.Caption;
  Result.TableName  := ATableName;
end;

function AddNmclForm(const ATableName, AKeyFieldName: String; ACaption: String = ''; AIsParented: Boolean = True): TfrmIntretinereNmcl;
var
  lForm: TfrmIntretinereNmcl;
begin
  lForm := CreateNmclForm(ATableName, AKeyFieldName, ACaption, AIsParented);
  Result := lForm;
  if not AIsParented then
    try
      lForm.ShowModal;
    finally
      lForm.Free;
    end;
end;


procedure TfrmIntretinereNmcl.btnCancelSelectClick(Sender: TObject);
begin
  ModalResult := mrCancel;
  Close;
end;

procedure TfrmIntretinereNmcl.btnOkSelectClick(Sender: TObject);
begin
  DBCommitUpdates(dtIntretinere.DataSet);
  ModalResult := mrOk;
  Close;
end;

procedure TfrmIntretinereNmcl.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DBPost(dtIntretinere.DataSet);
  Action := caFree;
end;

procedure TfrmIntretinereNmcl.FormDestroy(Sender: TObject);
begin
  dtIntretinere.DataSet.Free;
end;

function TfrmIntretinereNmcl.GetNiceName: String;
begin
  Result := Caption;
  if Length(Result) > 0 then begin
    //Result := FTableName;
    if Length(Result) > 0 then
      Result := UpCase(Result[1]) + LowerCase(Copy(Result, 2, Length(Result) - 1));
    Result := 'Întreținere ' + Result;
  end;
end;

procedure TfrmIntretinereNmcl.SetCaption(const Value: String);
begin
  FCaption := Value;
end;

procedure TfrmIntretinereNmcl.SetDataSet(const Value: TDataSet);
begin
  dtIntretinere.DataSet := Value;
  viewIntretinere.BeginUpdate;
  try
    viewIntretinere.DataController.DataSource := dtIntretinere;
    while viewIntretinere.ItemCount > 0 do
      viewIntretinere.Items[0].Free;
    viewIntretinere.DataController.CreateAllItems();
    viewIntretinere.DataController.KeyFieldNames := FKeyField;
    if gridIntretinere.Width < Application.MainForm.ClientWidth then
      viewIntretinere.OptionsView.ColumnAutoWidth := True;
  finally
    viewIntretinere.EndUpdate;
  end;
  viewIntretinere.ApplyBestFit();
end;

procedure TfrmIntretinereNmcl.SetKeyField(const Value: String);
begin
  FKeyField := Value;
end;

procedure TfrmIntretinereNmcl.SetTableName(const Value: String);
begin
  FTableName := Value;
  dtIntretinere.DataSet := DBNewUpdateQueryFmt('select * from %s', [Value]);
  TZQuery(dtIntretinere.DataSet).CachedUpdates := True;
  dtIntretinere.DataSet.Open;
  SetDataSet(dtIntretinere.DataSet);
end;

end.
